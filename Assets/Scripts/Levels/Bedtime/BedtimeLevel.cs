using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 19 — a nanna. Il gatto ha sonno ma non riesce a dormire. Tre cose, in quest'ordine logico:
    /// 1) spegni l'abat-jour (tocco);  2) abbassa a zero il volume del telefono col tasto "volume -" (anche se è già a zero va premuto);
    /// 3) poi non toccare più niente per 5 secondi. Un tocco lo sveglia ("Mrr?") e l'attesa riparte.
    /// Se riaccendi la luce o rialzi il volume si riparte da quel punto. (Editor: volume coi tasti - e +.)
    /// </summary>
    public class BedtimeLevel : LevelController
    {
        [SerializeField] TapButton lampSwitch;
        [SerializeField] GameObject lampLight;
        [SerializeField] SpriteRenderer roomDark;
        [SerializeField] AudioSource stereo;
        [SerializeField] SmokeEmitter notes;
        [SerializeField] GameObject catEyesOpen;
        [SerializeField] GameObject catEyesHalf;
        [SerializeField] SmokeEmitter zzz;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float darkAlpha = 0.6f;
        [SerializeField] float silentBelow = 0.01f;
        [SerializeField] float sleepAfter = 5f;

        bool lightOn = true, done;
        readonly VolumeDownWatch watch = new VolumeDownWatch();
        float still, darkNow;
        int lastState = -1;

        public override void OnLevelStart()
        {
            if (lampSwitch != null) lampSwitch.Pressed += OnLamp;
            if (MusicPlayer.Instance != null)
            {
                MusicPlayer.Instance.Duck(this, 0f);   // suona solo lo stereo della stanza
                if (stereo != null && stereo.clip == null) stereo.clip = MusicPlayer.Instance.soundtrack;
            }
            if (stereo != null) { stereo.loop = true; stereo.Play(); }
            if (zzz != null) zzz.Stop();
            watch.Begin();
            if (bubble != null) { bubble.Show("Too bright... too loud..."); StartCoroutine(HideLater(2.2f)); }
            Apply();
        }

        void OnDestroy()
        {
            if (lampSwitch != null) lampSwitch.Pressed -= OnLamp;
            if (MusicPlayer.Instance != null) MusicPlayer.Instance.Unduck(this);
        }

        void OnLamp(TapButton b)
        {
            if (done) return;
            lightOn = !lightOn;
            Apply();
        }

        void Apply()
        {
            if (lampLight != null) lampLight.SetActive(lightOn);
        }

        void Update()
        {
            if (done || IsFinished) return;
            float v = SensorHub.Instance != null ? SensorHub.Instance.SystemVolume01 : 1f;
            bool quiet = watch.Tick(silentBelow);   // serve almeno una pressione di "volume -" durante il livello
            if (stereo != null) stereo.volume = v <= silentBelow ? 0f : 1f;
            if (notes != null) { if (quiet || v <= silentBelow) notes.Stop(); else notes.Play(); notes.puffsPerSecond = Mathf.Lerp(0.5f, 6f, v); }

            darkNow = Mathf.MoveTowards(darkNow, lightOn ? 0f : darkAlpha, Time.deltaTime * 2f);
            if (roomDark != null) roomDark.color = new Color(0.02f, 0.02f, 0.08f, darkNow);

            // stato del gatto: 0 sveglio, 1 luce spenta o silenzio, 2 entrambi (si sta addormentando)
            int state = (!lightOn ? 1 : 0) + (quiet ? 1 : 0);
            if (state != lastState)
            {
                if (state > lastState && lastState >= 0 && bubble != null) { bubble.Show(state == 2 ? "Yaaawn..." : "Better..."); StartCoroutine(HideLater(1.2f)); }
                lastState = state;
                still = 0f;
            }
            if (catEyesOpen != null) catEyesOpen.SetActive(state < 2);
            if (catEyesHalf != null) catEyesHalf.SetActive(state == 2 && still < sleepAfter * 0.5f);
            if (state < 2) { if (zzz != null) zzz.Stop(); return; }

            // ultimo passo: nessun tocco per qualche secondo
            if (PointerInput.AnyTouchBegan() && !PointerInput.IsOverUi())
            {
                if (still > 0.6f && bubble != null) { bubble.Show("Wuff?"); StartCoroutine(HideLater(0.9f)); }
                still = 0f;
                if (zzz != null) zzz.Stop();
                // il tocco sull'abat-jour lo gestisce OnLamp: qui conta solo che il gatto si è svegliato
            }
            still += Time.deltaTime;
            if (zzz != null && still > 1.5f) zzz.Play();
            if (still >= sleepAfter) StartCoroutine(Asleep());
        }

        IEnumerator Asleep()
        {
            done = true;
            if (catEyesHalf != null) catEyesHalf.SetActive(false);
            if (stereo != null) stereo.Stop();
            if (MusicPlayer.Instance != null) MusicPlayer.Instance.Unduck(this);
            if (bubble != null) bubble.Show("Zzz... *snort*");
            yield return new WaitForSeconds(1.6f);
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
