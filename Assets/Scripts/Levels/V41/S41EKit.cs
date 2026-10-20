using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>Piccoli aiuti condivisi dai livelli del pacchetto 41E (coriandoli, rimbalzi, toni sintetizzati).</summary>
    public static class S41EKit
    {
        static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        public static void Confetti(SmokeEmitter[] confetti, bool on)
        {
            if (confetti == null) return;
            foreach (SmokeEmitter c in confetti)
                if (c != null) { if (on) c.Play(); else c.Stop(); }
        }

        public static void Say(SpeechBubble bubble, string text) { if (bubble != null) bubble.Show(text); }

        public static void SetAlpha(SpriteRenderer s, float a) { if (s != null) { Color c = s.color; c.a = a; s.color = c; } }
        public static void SetAlpha(TextMesh t, float a) { if (t != null) { Color c = t.color; c.a = a; t.color = c; } }

        /// <summary>Puntatore primario in coordinate mondo.</summary>
        public static bool Pointer(Camera cam, out Vector2 world, out bool began, out bool ended)
        {
            world = default;
            if (cam == null || !PointerInput.TryGetPrimary(out Vector2 sp, out began, out ended)) { began = ended = false; return false; }
            world = PointerInput.ToWorld(cam, sp);
            return true;
        }

        /// <summary>Rimbalzo di scala (da 1 a peak e ritorno) attorno alla scala attuale.</summary>
        public static IEnumerator Pop(Transform t, float peak, float seconds)
        {
            if (t == null) yield break;
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.localScale = s * (1f + (peak - 1f) * Mathf.Sin(k * Mathf.PI));
                yield return null;
            }
            t.localScale = s;
        }

        /// <summary>Scossa orizzontale breve.</summary>
        public static IEnumerator Shake(Transform t, float amount, float seconds)
        {
            if (t == null) yield break;
            Vector3 p = t.localPosition;
            for (float k = 0f; k < seconds; k += Time.deltaTime)
            {
                t.localPosition = p + new Vector3(Mathf.Sin(k * 60f) * amount * (1f - k / seconds), 0f, 0f);
                yield return null;
            }
            t.localPosition = p;
        }

        /// <summary>Movimento morbido (ease in-out) verso una posizione locale, con saltello opzionale.</summary>
        public static IEnumerator Move(Transform t, Vector3 to, float seconds, float hop = 0f)
        {
            if (t == null) yield break;
            Vector3 from = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                float e = k * k * (3f - 2f * k);
                t.localPosition = Vector3.Lerp(from, to, e) + Vector3.up * Mathf.Sin(k * Mathf.PI) * hop;
                yield return null;
            }
            t.localPosition = to;
        }

        /// <summary>Tono sintetizzato: "square" (8 bit), "thump" (battito), "sine", "beep".</summary>
        public static void Tone(MonoBehaviour owner, float hz, float seconds, string kind, float volume = 0.6f)
        {
            if (owner == null) return;
            AudioSource s = owner.GetComponent<AudioSource>();
            if (s == null) { s = owner.gameObject.AddComponent<AudioSource>(); s.playOnAwake = false; }
            s.PlayOneShot(Clip(hz, seconds, kind), volume);
        }

        static AudioClip Clip(float hz, float seconds, string kind)
        {
            string key = kind + Mathf.RoundToInt(hz) + "_" + Mathf.RoundToInt(seconds * 100f);
            if (clips.TryGetValue(key, out AudioClip c) && c != null) return c;
            const int rate = 22050;
            int n = Mathf.Max(1, (int)(seconds * rate));
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate, k = t / seconds;
                float w = 2f * Mathf.PI * hz * t;
                float env = Mathf.Min(1f, t * 200f) * Mathf.Min(1f, (1f - k) * 12f);
                float v;
                switch (kind)
                {
                    case "square": v = Mathf.Sign(Mathf.Sin(w)) * 0.22f * env; break;
                    case "thump": v = (Mathf.Sin(w) + Mathf.Sin(w * 0.5f) * 0.7f) * 0.6f * env * (0.6f + 0.4f * Mathf.Exp(-t * 6f)); break;
                    case "beep": v = Mathf.Sin(w) * 0.35f * env; break;
                    default: v = Mathf.Sin(w) * 0.5f * env; break;
                }
                data[i] = v;
            }
            c = AudioClip.Create("s41e_" + key, n, 1, rate, false);
            c.SetData(data, 0);
            clips[key] = c;
            return c;
        }
    }
}
