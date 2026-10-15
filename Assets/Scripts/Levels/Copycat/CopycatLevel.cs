using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 40 — copia il gatto. Il gatto suona una sequenza sui quattro tasti colorati (luce e nota); tu la
    /// ripeti. Tre turni, sempre più lunghi (3, 4, 5 note). Un tasto sbagliato e il gatto ride: si riprova.
    /// </summary>
    public class CopycatLevel : LevelController
    {
        [SerializeField] TapButton[] pads;
        [SerializeField] SpriteRenderer[] padArt;
        [SerializeField] SpriteRenderer[] padGlow;
        [SerializeField] SpeechBubble catBubble;
        [SerializeField] TextMesh roundLabel;
        [SerializeField] int[] roundLengths = { 3, 4, 5 };
        [SerializeField] float noteSeconds = 0.45f;

        static readonly float[] Notes = { 523.25f, 659.25f, 783.99f, 987.77f };   // do, mi, sol, si
        readonly List<int> sequence = new List<int>();
        AudioClip[] tones;
        AudioClip buzz;
        Color[] colors;
        AudioSource sfx;
        int round, input;
        bool listening, done;

        public override void OnLevelStart()
        {
            colors = new Color[padArt.Length];
            for (int i = 0; i < padArt.Length; i++) colors[i] = padArt[i].color;   // il colore pieno viene dal builder
            sfx = gameObject.AddComponent<AudioSource>();
            tones = new AudioClip[Notes.Length];
            for (int i = 0; i < Notes.Length; i++) tones[i] = Tone(Notes[i], 0.35f);
            buzz = Tone(150f, 0.4f);
            for (int i = 0; i < pads.Length; i++) { int index = i; pads[i].Pressed += b => OnPad(index); }
            Light(-1);
            StartCoroutine(NextRound(1.2f));
        }

        void OnDestroy()
        {
            if (tones != null) foreach (AudioClip c in tones) if (c != null) Destroy(c);
            if (buzz != null) Destroy(buzz);
        }

        IEnumerator NextRound(float delay)
        {
            listening = false;
            while (sequence.Count < roundLengths[round]) sequence.Add(Random.Range(0, pads.Length));
            if (roundLabel != null) roundLabel.text = "ROUND " + (round + 1) + " / " + roundLengths.Length;
            if (catBubble != null) catBubble.Show(round == 0 ? "Copy me!" : "Meow! Again!");
            yield return new WaitForSeconds(delay);
            if (catBubble != null) catBubble.Hide();
            foreach (int p in sequence)
            {
                Light(p);
                sfx.PlayOneShot(tones[p]);
                yield return new WaitForSeconds(noteSeconds);
                Light(-1);
                yield return new WaitForSeconds(0.15f);
            }
            input = 0;
            listening = true;
        }

        void OnPad(int p)
        {
            if (!listening || done) return;
            StartCoroutine(Flash(p));
            sfx.PlayOneShot(tones[p]);
            if (p != sequence[input]) { StartCoroutine(Wrong()); return; }
            input++;
            if (input < sequence.Count) return;
            round++;
            if (round >= roundLengths.Length) StartCoroutine(Bravo());
            else StartCoroutine(NextRound(1.0f));
        }

        IEnumerator Flash(int p)
        {
            Light(p);
            yield return new WaitForSeconds(0.2f);
            if (!done) Light(-1);
        }

        void Light(int p)
        {
            for (int i = 0; i < padArt.Length; i++)
            {
                bool on = i == p;
                Color c = colors[i];
                padArt[i].color = on ? Color.Lerp(c, Color.white, 0.35f) : c * 0.75f + new Color(0f, 0f, 0f, 0.25f);
                if (padGlow != null && i < padGlow.Length && padGlow[i] != null) padGlow[i].enabled = on;
            }
        }

        IEnumerator Wrong()
        {
            listening = false;
            done = true;
            sfx.PlayOneShot(buzz);
            if (catBubble != null) catBubble.Show("Mrr-hahaha!");
            yield return new WaitForSeconds(1.4f);
            Fail();
        }

        IEnumerator Bravo()
        {
            listening = false;
            done = true;
            if (catBubble != null) catBubble.Show("Purrr... you're good!");
            for (int k = 0; k < 6; k++) { Light(k % pads.Length); yield return new WaitForSeconds(0.12f); }
            Light(-1);
            yield return new WaitForSeconds(0.8f);
            Win();
        }

        static AudioClip Tone(float f, float seconds)
        {
            const int rate = 22050;
            int n = Mathf.RoundToInt(seconds * rate);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Min(1f, t / 0.01f) * Mathf.Exp(-t * 5f);
                data[i] = (Mathf.Sin(t * f * Mathf.PI * 2f) + 0.3f * Mathf.Sin(t * f * Mathf.PI * 4f)) * 0.3f * env;
            }
            AudioClip clip = AudioClip.Create("tone", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
