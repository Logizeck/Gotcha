using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>Piccoli aiuti comuni ai livelli del pacchetto 41B (puntatore, toni sintetizzati, rimbalzi, cuori).</summary>
    public static class Kit41B
    {
        static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        /// <summary>Puntatore primario in coordinate mondo (ignora i tocchi sopra l'interfaccia quando iniziano).</summary>
        public static bool Pointer(Camera cam, out Vector2 world, out bool began, out bool ended)
        {
            world = default;
            if (cam == null || !PointerInput.TryGetPrimary(out Vector2 sp, out began, out ended)) { began = ended = false; return false; }
            if (began && PointerInput.IsOverUi()) { began = false; return false; }
            world = PointerInput.ToWorld(cam, sp);
            return true;
        }

        /// <summary>Sorgente audio del livello (creata al primo uso).</summary>
        public static AudioSource Source(MonoBehaviour owner)
        {
            AudioSource s = owner.GetComponent<AudioSource>();
            if (s == null) { s = owner.gameObject.AddComponent<AudioSource>(); s.playOnAwake = false; }
            return s;
        }

        /// <summary>Suona un tono sintetizzato: "bell" (vetro), "thud" (sordo), "square" (allarme), "sine".</summary>
        public static void Tone(MonoBehaviour owner, float hz, float seconds, string kind, float volume = 0.6f)
        {
            if (owner == null) return;
            Source(owner).PlayOneShot(Clip(hz, seconds, kind), volume);
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
                float v;
                switch (kind)
                {
                    case "bell": v = (Mathf.Sin(w) + Mathf.Sin(w * 2.76f) * 0.45f + Mathf.Sin(w * 5.4f) * 0.2f) * Mathf.Exp(-t * 7f) * Mathf.Min(1f, t * 300f) * 0.5f; break;
                    case "thud": v = (Mathf.Sin(w) + Mathf.Sin(w * 0.5f) * 0.6f) * Mathf.Exp(-t * 9f) * Mathf.Min(1f, t * 200f) * 0.6f; break;
                    case "square": v = Mathf.Sign(Mathf.Sin(w)) * 0.25f * Mathf.Min(1f, (1f - k) * 20f); break;
                    default: v = Mathf.Sin(w) * Mathf.Min(1f, t * 80f) * Mathf.Min(1f, (1f - k) * 10f) * 0.5f; break;
                }
                data[i] = v;
            }
            c = AudioClip.Create("k41b_" + key, n, 1, rate, false);
            c.SetData(data, 0);
            clips[key] = c;
            return c;
        }

        /// <summary>Rimbalzo di scala.</summary>
        public static IEnumerator Pop(Transform t, float amount = 0.18f, float seconds = 0.25f)
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

        /// <summary>Scossone orizzontale (errore).</summary>
        public static IEnumerator Shake(Transform t, float amount = 0.12f, float seconds = 0.35f)
        {
            if (t == null) yield break;
            Vector3 p = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.localPosition = p + Vector3.right * Mathf.Sin(k * 40f) * amount * (1f - k);
                yield return null;
            }
            t.localPosition = p;
        }

        /// <summary>Porta l'opacità di uno sprite a "to".</summary>
        public static IEnumerator Fade(SpriteRenderer sr, float to, float seconds)
        {
            if (sr == null) yield break;
            sr.gameObject.SetActive(true);
            float from = sr.color.a;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(from, to, k);
                sr.color = c;
                yield return null;
            }
            Color e = sr.color;
            e.a = to;
            sr.color = e;
        }

        /// <summary>Cuori dei tentativi: pieni quelli rimasti, grigi quelli persi.</summary>
        public static void Hearts(SpriteRenderer[] hearts, int left)
        {
            if (hearts == null) return;
            for (int i = 0; i < hearts.Length; i++)
                if (hearts[i] != null) hearts[i].color = i < left ? Color.white : new Color(0.2f, 0.2f, 0.2f, 0.35f);
        }

        public static void Say(Gotcha.Components.SpeechBubble b, string text) { if (b != null) b.Show(text); }
    }
}
