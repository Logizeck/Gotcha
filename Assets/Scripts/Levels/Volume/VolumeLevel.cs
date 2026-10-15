using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 7 — il gatto arrabbiato. Lo stereo suona a tutto volume e il gatto è furioso. Si risolve abbassando
    /// al minimo il volume del telefono: man mano che scende il gatto passa da furioso ad arrabbiato, seccato,
    /// assonnato; a volume zero si addormenta. (Nell'Editor il volume si simula coi tasti - e +.)
    /// Serve sempre almeno una pressione di "volume -" durante il livello (VolumeDownWatch): un telefono già a volume
    /// zero non risolve il livello da solo; finché non premi il tasto il gatto resta arrabbiato.
    /// </summary>
    public class VolumeLevel : LevelController
    {
        [SerializeField] AudioSource stereo;            // suona la colonna sonora dallo stereo
        [SerializeField] GameObject[] faces;            // 0 furioso, 1 arrabbiato, 2 seccato, 3 assonnato
        [SerializeField] GameObject anger;              // vena della rabbia (solo da furioso)
        [SerializeField] SmokeEmitter steam;            // vapore dalle orecchie (solo da furioso)
        [SerializeField] SmokeEmitter notes;            // note musicali dallo stereo
        [SerializeField] SmokeEmitter zzz;
        [SerializeField] Pulse[] speakers;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float silentBelow = 0.01f;
        [SerializeField] float holdSeconds = 0.8f;

        bool done;
        readonly VolumeDownWatch watch = new VolumeDownWatch();
        float silentFor;
        int lastMood = -1;

        public override void OnLevelStart()
        {
            if (MusicPlayer.Instance != null)
            {
                MusicPlayer.Instance.Duck(this, 0f);   // suona solo lo stereo
                if (stereo != null && stereo.clip == null) stereo.clip = MusicPlayer.Instance.soundtrack;
            }
            if (stereo != null) { stereo.loop = true; stereo.Play(); }
            watch.Begin();
            if (zzz != null) zzz.Stop();
            if (bubble != null) { bubble.Show("HISSS!"); StartCoroutine(HideLater(1.4f)); }
        }

        void OnDestroy()
        {
            if (MusicPlayer.Instance != null) MusicPlayer.Instance.Unduck(this);
        }

        void Update()
        {
            if (done || IsFinished) return;
            float v = SensorHub.Instance != null ? SensorHub.Instance.SystemVolume01 : 1f;
            bool silenced = watch.Tick(silentBelow);
            if (!watch.PressedSinceStart) v = Mathf.Max(v, 0.8f);   // senza il tasto il gatto non si calma

            int mood = v > 0.75f ? 0 : v > 0.45f ? 1 : v > 0.15f ? 2 : 3;
            if (mood != lastMood) SetMood(mood);

            if (notes != null) notes.puffsPerSecond = Mathf.Lerp(0.5f, 6f, v);
            if (speakers != null) foreach (Pulse s in speakers) if (s != null) s.amount = Mathf.Lerp(0f, 0.08f, v);

            silentFor = silenced ? silentFor + Time.deltaTime : 0f;
            if (silentFor >= holdSeconds) StartCoroutine(FallAsleep());
        }

        void SetMood(int mood)
        {
            lastMood = mood;
            for (int i = 0; i < faces.Length; i++) if (faces[i] != null) faces[i].SetActive(i == mood);
            if (anger != null) anger.SetActive(mood == 0);
            if (steam != null) { if (mood == 0) steam.Play(); else steam.Stop(); }
        }

        IEnumerator FallAsleep()
        {
            done = true;
            foreach (GameObject f in faces) if (f != null) f.SetActive(false);   // occhi chiusi del gatto che dorme
            if (anger != null) anger.SetActive(false);
            if (steam != null) steam.Stop();
            if (notes != null) notes.Stop();
            if (stereo != null) stereo.Stop();
            if (zzz != null) zzz.Play();
            // il gatto dorme: la colonna sonora del gioco torna subito (il volume del telefono però è a zero)
            if (MusicPlayer.Instance != null) MusicPlayer.Instance.Unduck(this);
            if (bubble != null) bubble.Show("Purrr...");
            yield return new WaitForSeconds(2.2f);
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null) bubble.Hide();
        }
    }
}
