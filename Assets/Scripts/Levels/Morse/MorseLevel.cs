using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 6 — ascolta e rispondi. La radio suona a caso uno di questi segnali:
    /// "..." (tre brevi), "---" (tre lunghi), ".--", "-.-", ".-.". Bisogna premere il tasto col disegno giusto.
    /// Toccando la radio il segnale si riascolta. Tasto sbagliato = GOTCHA! (si riprova con un nuovo segnale).
    /// La spia della radio si accende insieme ai toni, così si gioca anche col telefono silenzioso.
    /// </summary>
    public class MorseLevel : LevelController
    {
        public static readonly string[] Codes = { "...", "---", ".--", "-.-", ".-." };

        [SerializeField] AudioSource source;
        [SerializeField] TapButton[] keys;        // keys[i] corrisponde a Codes[i]
        [SerializeField] TapButton radio;         // tocco = riascolta
        [SerializeField] GameObject lamp;
        [SerializeField] Transform shakeTarget;
        [SerializeField] float frequency = 760f;
        [SerializeField] float dotSeconds = 0.13f;
        [SerializeField] float dashSeconds = 0.42f;
        [SerializeField] float gapSeconds = 0.14f;

        int answer;
        bool playing, done;
        AudioClip dot, dash, buzz;

        public override void OnLevelStart()
        {
            answer = Random.Range(0, Codes.Length);
            dot = Tone("dot", frequency, dotSeconds);
            dash = Tone("dash", frequency, dashSeconds);
            buzz = Tone("buzz", 140f, 0.45f, true);
            for (int i = 0; i < keys.Length; i++) if (keys[i] != null) keys[i].Pressed += OnKey;
            if (radio != null) radio.Pressed += OnRadio;
            if (lamp != null) lamp.SetActive(false);
            StartCoroutine(PlayCode(0.8f));
        }

        void OnDestroy()
        {
            if (keys != null) foreach (TapButton k in keys) if (k != null) k.Pressed -= OnKey;
            if (radio != null) radio.Pressed -= OnRadio;
            if (MusicPlayer.Instance != null) MusicPlayer.Instance.Unduck(this);
            foreach (AudioClip c in new[] { dot, dash, buzz }) if (c != null) Destroy(c);
        }

        void OnRadio(TapButton b)
        {
            if (!playing && !done) StartCoroutine(PlayCode(0.1f));
        }

        IEnumerator PlayCode(float delay)
        {
            playing = true;
            if (MusicPlayer.Instance != null) MusicPlayer.Instance.Duck(this);
            yield return new WaitForSeconds(delay);
            foreach (char c in Codes[answer])
            {
                bool isDot = c == '.';
                source.PlayOneShot(isDot ? dot : dash);
                if (lamp != null) lamp.SetActive(true);
                yield return new WaitForSeconds(isDot ? dotSeconds : dashSeconds);
                if (lamp != null) lamp.SetActive(false);
                yield return new WaitForSeconds(gapSeconds);
            }
            if (MusicPlayer.Instance != null) MusicPlayer.Instance.Unduck(this);
            playing = false;
        }

        void OnKey(TapButton key)
        {
            if (done || IsFinished) return;
            done = true;
            int chosen = System.Array.IndexOf(keys, key);
            StartCoroutine(chosen == answer ? RightRoutine() : WrongRoutine());
        }

        IEnumerator RightRoutine()
        {
            for (int i = 0; i < 3; i++)   // la spia lampeggia di gioia
            {
                if (lamp != null) lamp.SetActive(true);
                yield return new WaitForSeconds(0.12f);
                if (lamp != null) lamp.SetActive(false);
                yield return new WaitForSeconds(0.08f);
            }
            Win();
        }

        IEnumerator WrongRoutine()
        {
            source.PlayOneShot(buzz);
            Vector3 start = shakeTarget != null ? shakeTarget.localPosition : Vector3.zero;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                if (shakeTarget != null) shakeTarget.localPosition = start + Vector3.right * Mathf.Sin(k * 50f) * 0.08f * (1f - k);
                yield return null;
            }
            if (shakeTarget != null) shakeTarget.localPosition = start;
            Fail();
        }

        /// <summary>Tono sinusoidale (o ronzio a onda quadra) con attacco e rilascio morbidi per evitare i "click".</summary>
        static AudioClip Tone(string name, float freq, float seconds, bool square = false)
        {
            const int rate = 44100;
            int n = Mathf.CeilToInt(seconds * rate);
            var data = new float[n];
            int fade = rate / 200;   // 5 ms
            for (int i = 0; i < n; i++)
            {
                float s = Mathf.Sin(2f * Mathf.PI * freq * i / rate);
                if (square) s = Mathf.Sign(s) * 0.6f;
                float env = Mathf.Min(1f, Mathf.Min(i, n - 1 - i) / (float)fade);
                data[i] = s * env * 0.6f;
            }
            AudioClip clip = AudioClip.Create(name, n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
