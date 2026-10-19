using System.Collections.Generic;
using UnityEngine;

namespace Gotcha.Levels
{
    /// <summary>
    /// Suoni sintetizzati dei livelli compositi del gruppo A (0.40): sveglia, sirena, ping del sonar, sussurro,
    /// "BOO!", sfregamento del fiammifero, fiammata, sgranocchiata. Play() li suona una volta; Loop() li ripete.
    /// </summary>
    public static class Synth40A
    {
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(40);

        static float Noise() => (float)rnd.NextDouble() * 2f - 1f;

        public static AudioClip Clip(string kind)
        {
            if (cache.TryGetValue(kind, out AudioClip c) && c != null) return c;
            switch (kind)
            {
                case "ring":   // trillo della sveglia: due campanelli alternati
                    c = Make(kind, 0.8f, t =>
                    {
                        float on = (t % 0.4f) < 0.3f ? 1f : 0f;
                        float bell = Mathf.Sin(t * 2f * Mathf.PI * 1250f) * 0.5f + Mathf.Sin(t * 2f * Mathf.PI * 1870f) * 0.25f;
                        return bell * (0.6f + 0.4f * Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * 24f))) * on * 0.6f;
                    });
                    break;
                case "siren":   // sirena che sale e scende
                    c = Make(kind, 1.2f, t =>
                    {
                        float f = Mathf.Lerp(620f, 1150f, Mathf.PingPong(t / 0.6f, 1f));
                        return Mathf.Sign(Mathf.Sin(PhaseSweep(t, 620f, 1150f, 0.6f))) * 0.25f + Mathf.Sin(t * 2f * Mathf.PI * f) * 0.2f;
                    });
                    break;
                case "ping":   // sonar: nota lunga e pulita con eco
                    c = Make(kind, 1.6f, t =>
                    {
                        float a = Mathf.Sin(t * 2f * Mathf.PI * 1480f) * Mathf.Exp(-t * 3.5f);
                        float e = t > 0.45f ? Mathf.Sin((t - 0.45f) * 2f * Mathf.PI * 1480f) * Mathf.Exp(-(t - 0.45f) * 5f) * 0.3f : 0f;
                        return (a + e) * 0.6f * Mathf.Min(1f, t * 200f);
                    });
                    break;
                case "whisper":   // sussurro: fruscio a sillabe
                    c = Make(kind, 1.3f, t =>
                    {
                        float syl = Mathf.Max(0f, Mathf.Sin(t / 1.3f * Mathf.PI * 4f));
                        float hiss = Noise() * 0.5f + Mathf.Sin(t * 2f * Mathf.PI * 2600f) * Noise() * 0.3f;
                        return hiss * syl * syl * 0.7f;
                    });
                    break;
                case "boo":   // "BOO!": voce grave che scende con un ruggito
                    c = Make(kind, 0.7f, t =>
                    {
                        float f = Mathf.Lerp(260f, 110f, t / 0.7f);
                        float v = Mathf.Sin(t * 2f * Mathf.PI * f) + Mathf.Sin(t * 2f * Mathf.PI * f * 2.02f) * 0.5f + Noise() * 0.25f;
                        return v * Mathf.Min(1f, t * 30f) * Mathf.Exp(-t * 2.5f) * 0.55f;
                    });
                    break;
                case "scratch":   // sfregamento del fiammifero
                    c = Make(kind, 0.12f, t => Noise() * Mathf.Sin(t / 0.12f * Mathf.PI) * 0.5f);
                    break;
                case "flare":   // fiammata
                    c = Make(kind, 0.6f, t => Noise() * Mathf.Min(1f, t * 40f) * Mathf.Exp(-t * 5f) * 0.6f);
                    break;
                case "munch":   // sgranocchiata
                    c = Make(kind, 0.16f, t => Noise() * Mathf.Exp(-t * 25f) * 0.7f + Mathf.Sin(t * 2f * Mathf.PI * 180f) * Mathf.Exp(-t * 30f) * 0.4f);
                    break;
                default:
                    c = Make(kind, 0.1f, t => Mathf.Sin(t * 2f * Mathf.PI * 660f) * Mathf.Exp(-t * 30f));
                    break;
            }
            cache[kind] = c;
            return c;
        }

        // fase di un'onda la cui frequenza va avanti e indietro tra f0 e f1 (periodo 2*half)
        static float PhaseSweep(float t, float f0, float f1, float half)
        {
            float cycles = Mathf.Floor(t / (2f * half));
            float r = t - cycles * 2f * half;
            float per = (f0 + f1) * half;   // fase di un ciclo completo (in giri)
            float ph;
            if (r < half) ph = f0 * r + (f1 - f0) * r * r / (2f * half);
            else { float u = r - half; ph = (f0 + f1) * half * 0.5f + f1 * u - (f1 - f0) * u * u / (2f * half); }
            return (cycles * per + ph) * 2f * Mathf.PI;
        }

        static AudioClip Make(string name, float seconds, System.Func<float, float> f)
        {
            const int rate = 22050;
            int n = Mathf.Max(1, (int)(seconds * rate));
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(f(i / (float)rate), -1f, 1f);
            AudioClip clip = AudioClip.Create("v40a_" + name, n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Suona una volta dalla sorgente audio dell'oggetto (creata se manca).</summary>
        public static void Play(Component owner, string kind, float volume = 0.6f)
        {
            if (owner == null) return;
            AudioSource src = owner.GetComponent<AudioSource>();
            if (src == null) { src = owner.gameObject.AddComponent<AudioSource>(); src.playOnAwake = false; }
            src.PlayOneShot(Clip(kind), volume);
        }

        /// <summary>Suono ripetuto su una sorgente a parte (figlia dell'oggetto): fermarlo con Stop() o distruggendolo.</summary>
        public static AudioSource Loop(Component owner, string kind, float volume = 0.6f)
        {
            var go = new GameObject("Loop_" + kind);
            go.transform.SetParent(owner.transform, false);
            AudioSource src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.clip = Clip(kind);
            src.loop = true;
            src.volume = volume;
            src.Play();
            return src;
        }
    }
}
