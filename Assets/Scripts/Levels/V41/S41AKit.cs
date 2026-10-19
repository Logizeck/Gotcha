using System.Collections;
using UnityEngine;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>Piccoli aiuti comuni ai livelli del pacchetto A (specifica 105-112).</summary>
    public static class S41AKit
    {
        /// <summary>Puntatore primario in coordinate mondo (false se nessun dito/mouse o se è sopra la UI all'inizio).</summary>
        public static bool Pointer(Camera cam, out Vector2 world, out bool began, out bool ended)
        {
            world = default;
            if (cam == null || !PointerInput.TryGetPrimary(out Vector2 sp, out began, out ended)) { began = ended = false; return false; }
            if (began && PointerInput.IsOverUi()) { began = false; ended = false; return false; }
            world = PointerInput.ToWorld(cam, sp);
            return true;
        }

        /// <summary>Rimbalzo veloce della scala (punch).</summary>
        public static IEnumerator Bounce(Transform t, float amount = 0.15f, float seconds = 0.25f)
        {
            if (t == null) yield break;
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.localScale = s * (1f + Mathf.Sin(k * Mathf.PI) * amount);
                yield return null;
            }
            t.localScale = s;
        }

        /// <summary>Sposta un oggetto verso un punto con una curva morbida.</summary>
        public static IEnumerator Move(Transform t, Vector3 to, float seconds, float hop = 0f)
        {
            if (t == null) yield break;
            Vector3 from = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                float e = Mathf.SmoothStep(0f, 1f, k);
                t.position = Vector3.Lerp(from, to, e) + Vector3.up * Mathf.Sin(k * Mathf.PI) * hop;
                yield return null;
            }
            t.position = to;
        }

        public static void Alpha(SpriteRenderer sr, float a)
        {
            if (sr == null) return;
            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }

        /// <summary>Nota sintetica breve (frequenza Hz), per bip e jingle.</summary>
        static readonly System.Collections.Generic.Dictionary<int, AudioClip> tones = new System.Collections.Generic.Dictionary<int, AudioClip>();

        public static AudioClip Tone(float hz, float seconds = 0.18f, float decay = 9f)
        {
            int key = Mathf.RoundToInt(hz) * 1000 + Mathf.RoundToInt(seconds * 100f);
            if (tones.TryGetValue(key, out AudioClip cached) && cached != null) return cached;
            const int rate = 22050;
            int n = Mathf.Max(1, (int)(seconds * rate));
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                data[i] = (Mathf.Sin(t * 2f * Mathf.PI * hz) * 0.7f + Mathf.Sin(t * 4f * Mathf.PI * hz) * 0.2f) * Mathf.Min(1f, t * 200f) * Mathf.Exp(-t * decay) * 0.6f;
            }
            AudioClip c = AudioClip.Create("s41a_tone", n, 1, rate, false);
            c.SetData(data, 0);
            tones[key] = c;
            return c;
        }

        /// <summary>Suona una nota dalla sorgente audio dell'oggetto.</summary>
        public static void Beep(Component owner, float hz, float volume = 0.5f, float seconds = 0.18f)
        {
            if (owner == null) return;
            AudioSource src = owner.GetComponent<AudioSource>();
            if (src == null) { src = owner.gameObject.AddComponent<AudioSource>(); src.playOnAwake = false; }
            src.PlayOneShot(Tone(hz, seconds), volume);
        }
    }
}
