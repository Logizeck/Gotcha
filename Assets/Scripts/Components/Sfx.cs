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
                default: c = Make(kind, 0.1f, t => Mathf.Sin(t * 2f * Mathf.PI * 660f) * Mathf.Exp(-t * 30f)); break;
            }
            cache[kind] = c;
            return c;
        }

        static System.Random rnd = new System.Random(7);
        static float Noise(float t) => (float)rnd.NextDouble() * 2f - 1f;

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
