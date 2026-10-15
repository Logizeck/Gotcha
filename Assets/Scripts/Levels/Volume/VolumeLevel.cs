using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 8 — il gatto arrabbiato. Lo stereo suona a tutto volume e il gatto è furioso. Si risolve abbassando
    /// al minimo il volume del telefono: man mano che scende il gatto passa da furioso ad arrabbiato, seccato,
    /// assonnato; a volume zero si addormenta. (Nell'Editor il volume si simula coi tasti - e +.)
    /// Se il volume è già a zero all'inizio, bisogna prima alzarlo: la musica deve essere "sentita" almeno una volta.
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

        bool armed, done;
        float silentFor;
        int lastMood = -1;

        public override void OnLevelStart()
        {
            if (MusicPlayer.Instance != null)
            {
                MusicPlayer.Instance.SetDucked(true, 0f);   // suona solo lo stereo
                if (stereo != null && stereo.clip == null) stereo.clip = MusicPlayer.Instance.soundtrack;
            }
            if (stereo != null) { stereo.loop = true; stereo.Play(); }
            if (zzz != null) zzz.Stop();
            if (bubble != null) { bubble.Show("HISSS!"); StartCoroutine(HideLater(1.4f)); }
        }

        void OnDestroy()
        {
            if (MusicPlayer.Instance != null) MusicPlayer.Instance.SetDucked(false);
        }

        void Update()
        {
            if (done || IsFinished) return;
            float v = SensorHub.Instance != null ? SensorHub.Instance.SystemVolume01 : 1f;
            if (v > silentBelow) armed = true;

            int mood = v > 0.75f ? 0 : v > 0.45f ? 1 : v > 0.15f ? 2 : 3;
            if (mood != lastMood) SetMood(mood);

            if (notes != null) notes.puffsPerSecond = Mathf.Lerp(0.5f, 6f, v);
            if (speakers != null) foreach (Pulse s in speakers) if (s != null) s.amount = Mathf.Lerp(0f, 0.08f, v);

            silentFor = armed && v <= silentBelow ? silentFor + Time.deltaTime : 0f;
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
            yield return new WaitForSeconds(1.6f);
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null) bubble.Hide();
        }
    }
}
