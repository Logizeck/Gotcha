using System.Collections.Generic;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>Piccoli suoni sintetizzati (clic, tonfi, campanelli) per i livelli che non hanno file audio.</summary>
    public static class Sfx
    {
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

        /// <summary>Suona una nota smorzata (freq Hz) dalla sorgente audio dell'oggetto (creata se manca).</summary>
        public static void Play(Component owner, string kind, float volume = 0.6f)
        {
            if (owner == null) return;
            AudioSource src = owner.GetComponent<AudioSource>();
            if (src == null) { src = owner.gameObject.AddComponent<AudioSource>(); src.playOnAwake = false; }
            src.PlayOneShot(Clip(kind), volume);
        }

        public static AudioClip Clip(string kind)
        {
            if (cache.TryGetValue(kind, out AudioClip c) && c != null) return c;
            switch (kind)
            {
                case "tick": c = Make(kind, 0.03f, t => Noise(t) * Mathf.Exp(-t * 300f)); break;
                case "clunk": c = Make(kind, 0.18f, t => (Mathf.Sin(t * 2f * Mathf.PI * 90f) * 0.8f + Noise(t) * 0.4f) * Mathf.Exp(-t * 30f)); break;
                case "pop": c = Make(kind, 0.08f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(900f, 300f, t / 0.08f)) * Mathf.Exp(-t * 40f)); break;
                case "bonk": c = Make(kind, 0.15f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(220f, 90f, t / 0.15f)) * Mathf.Exp(-t * 25f)); break;
                case "ding": c = Make(kind, 1.2f, t => (Mathf.Sin(t * 2f * Mathf.PI * 1320f) + Mathf.Sin(t * 2f * Mathf.PI * 1980f) * 0.4f) * Mathf.Exp(-t * 3f) * 0.6f); break;
                case "buzz": c = Make(kind, 0.35f, t => Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * 110f)) * 0.35f * Mathf.Exp(-t * 4f)); break;
                case "woof": c = Make(kind, 0.22f, t => (Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(330f, 180f, t / 0.22f)) + Noise(t) * 0.35f) * Mathf.Min(1f, t * 60f) * Mathf.Exp(-t * 9f)); break;
                case "snore": c = Make(kind, 0.55f, t => (Mathf.Sin(t * 2f * Mathf.PI * 72f) * 0.6f + Noise(t) * 0.45f) * Mathf.Sin(t / 0.55f * Mathf.PI) * (0.6f + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * 18f))); break;
                case "whoosh": c = Make(kind, 0.4f, t => Noise(t) * Mathf.Sin(t / 0.4f * Mathf.PI) * 0.5f); break;
                case "mock_laugh": c = MakeMockLaugh(); break;
                default: c = Make(kind, 0.1f, t => Mathf.Sin(t * 2f * Mathf.PI * 660f) * Mathf.Exp(-t * 30f)); break;
            }
            cache[kind] = c;
            return c;
        }

        static System.Random rnd = new System.Random(7);
        static float Noise(float t) => (float)rnd.NextDouble() * 2f - 1f;

        /// <summary>
        /// Risata da cartone che prende in giro: "qua, qua, qua-ha-ha-ha-ha!" (anatra nasale e voce che scende).
        /// Ogni sillaba è una somma di armoniche pesate da formanti: "qu" che si apre in "a" per i qua, "ha" con il
        /// soffio iniziale per la risata.
        /// </summary>
        static AudioClip MakeMockLaugh()
        {
            const int rate = 22050;
            // (inizio, durata, nota iniziale, nota finale, è un "qua"?)
            var syl = new List<(float s, float d, float f0, float f1, bool quack)>
            {
                (0.00f, 0.17f, 560f, 390f, true),
                (0.24f, 0.17f, 580f, 400f, true),
                (0.48f, 0.14f, 600f, 430f, true),
            };
            float[] ha = { 640f, 610f, 575f, 540f, 500f, 455f, 410f };
            for (int i = 0; i < ha.Length; i++) syl.Add((0.66f + i * 0.115f, 0.09f, ha[i], ha[i] * 0.93f, false));
            float total = 0.66f + ha.Length * 0.115f + 0.1f;
            int n = (int)(total * rate);
            var data = new float[n];
            float phase = 0f, peak = 0.001f;
            var noise = new System.Random(11);
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                int k = -1;
                for (int j = 0; j < syl.Count; j++) if (t >= syl[j].s && t < syl[j].s + syl[j].d) { k = j; break; }
                if (k < 0) continue;
                var y = syl[k];
                float u = (t - y.s) / y.d;
                float f0 = Mathf.Lerp(y.f0, y.f1, u) * (1f + 0.02f * Mathf.Sin(t * 2f * Mathf.PI * 9f));
                phase += f0 / rate;
                if (phase > 1000f) phase -= 1000f;
                float F1 = y.quack ? Mathf.Lerp(330f, 1050f, Mathf.Min(1f, u * 4f)) : 850f;
                float F2 = y.quack ? 1700f : 1300f;
                float v = 0f;
                for (int h = 1; h <= 16; h++)
                {
                    float fh = h * f0;
                    if (fh > 5000f) break;
                    float w = Formant(fh, F1, 230f) + 0.6f * Formant(fh, F2, 300f) + 0.25f * Formant(fh, 2700f, 400f);
                    v += Mathf.Sin(2f * Mathf.PI * h * phase) * w;
                }
                float env = y.quack ? Mathf.Min(1f, u * 25f) * (u < 0.7f ? 1f : (1f - u) / 0.3f) : Mathf.Sin(u * Mathf.PI);
                if (!y.quack) v += ((float)noise.NextDouble() * 2f - 1f) * 0.8f * Mathf.Max(0f, 1f - u * 3f);   // il soffio della "h"
                data[i] = v * env;
                peak = Mathf.Max(peak, Mathf.Abs(data[i]));
            }
            for (int i = 0; i < n; i++) data[i] = data[i] / peak * 0.9f;
            AudioClip clip = AudioClip.Create("sfx_mock_laugh", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float Formant(float f, float center, float width)
        {
            float d = (f - center) / width;
            return Mathf.Exp(-d * d);
        }

        static AudioClip Make(string name, float seconds, System.Func<float, float> f)
        {
            const int rate = 22050;
            int n = Mathf.Max(1, (int)(seconds * rate));
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(f(i / (float)rate), -1f, 1f);
            AudioClip clip = AudioClip.Create("sfx_" + name, n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Breve vibrazione del telefono (solo su dispositivo).</summary>
        public static void Buzz() { Haptics.Vibrate(60); }

#if UNITY_ANDROID && !UNITY_EDITOR
        // riferimento a Handheld.Vibrate: così Unity aggiunge il permesso VIBRATE al manifest
        static void KeepVibratePermission() { Handheld.Vibrate(); }
#endif
    }
}
