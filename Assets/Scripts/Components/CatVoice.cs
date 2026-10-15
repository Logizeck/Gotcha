using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Versi del gatto sintetizzati al volo (nessun file audio): miagolio ("mi-aaao" con le vocali che scivolano
    /// da i ad a ad o), soffio arrabbiato, fusa, trillo interrogativo e sbadiglio. Il fumetto di un gatto
    /// (SpeechBubble.catVoice) sceglie il verso in base a quello che dice.
    /// </summary>
    public static class CatVoice
    {
        const int Rate = 22050;
        static AudioClip meow, meowHigh, hiss, purr, trill, yawn;

        public static AudioClip For(string message)
        {
            string m = message.ToLowerInvariant();
            if (m.Contains("hiss")) return hiss ?? (hiss = Hiss());
            if (m.Contains("purr") || m.Contains("zzz")) return purr ?? (purr = Purr());
            if (m.Contains("mrr")) return trill ?? (trill = Meow("trill", 0.38f, 560f, 700f, 520f, 0.5f, 28f));
            if (m.Contains("yaa") || m.Contains("yawn")) return yawn ?? (yawn = Meow("yawn", 1.1f, 360f, 520f, 300f, 1.4f, 0f));
            if (message == message.ToUpperInvariant() && message.Contains("!"))   // MEOW!! urlato
                return meowHigh ?? (meowHigh = Meow("meow_loud", 0.65f, 640f, 980f, 560f, 0.8f, 0f));
            return meow ?? (meow = Meow("meow", 0.7f, 520f, 780f, 430f, 0.7f, 0f));
        }

        /// <summary>Miagolio: tono che sale e scende, formanti da "i" a "a" a "o", un filo di vibrato.</summary>
        static AudioClip Meow(string name, float seconds, float f0Start, float f0Peak, float f0End, float gain, float trillHz)
        {
            int n = Mathf.RoundToInt(seconds * Rate);
            var data = new float[n];
            var phase = new float[16];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate, k = t / seconds;
                float f0 = k < 0.25f ? Mathf.Lerp(f0Start, f0Peak, Smooth(k / 0.25f)) : Mathf.Lerp(f0Peak, f0End, Smooth((k - 0.25f) / 0.75f));
                f0 *= 1f + Mathf.Sin(t * Mathf.PI * 2f * 5.5f) * 0.015f;
                // vocali: i (300/2300) → a (900/1400) → o (550/900)
                float f1 = k < 0.4f ? Mathf.Lerp(330f, 950f, k / 0.4f) : Mathf.Lerp(950f, 560f, (k - 0.4f) / 0.6f);
                float f2 = k < 0.4f ? Mathf.Lerp(2300f, 1450f, k / 0.4f) : Mathf.Lerp(1450f, 950f, (k - 0.4f) / 0.6f);
                float s = 0f;
                for (int h = 0; h < phase.Length; h++)
                {
                    float fh = f0 * (h + 1);
                    if (fh > Rate * 0.45f) break;
                    phase[h] += fh / Rate;
                    if (phase[h] > 1f) phase[h] -= 1f;
                    float g = Peak(fh, f1, 180f) + 0.6f * Peak(fh, f2, 260f) + 0.25f * Peak(fh, 3200f, 500f);
                    s += Mathf.Sin(phase[h] * Mathf.PI * 2f) * g / Mathf.Sqrt(h + 1);
                }
                if (trillHz > 0f) s *= 0.55f + 0.45f * Mathf.Sin(t * Mathf.PI * 2f * trillHz);
                float env = Mathf.Min(1f, t / 0.04f) * Mathf.Min(1f, (seconds - t) / 0.18f);
                data[i] = Mathf.Clamp(s * 0.3f * gain * env, -1f, 1f);
            }
            return Make(name, data);
        }

        static AudioClip Hiss()
        {
            const float seconds = 0.9f;
            int n = Mathf.RoundToInt(seconds * Rate);
            var data = new float[n];
            var rnd = new System.Random(5);
            float prev = 0f, lp = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float w = (float)rnd.NextDouble() * 2f - 1f;
                float hp = w - prev;   // passa-alto: soffio sibilante
                prev = w;
                lp += (hp - lp) * 0.6f;
                float env = Mathf.Min(1f, t / 0.02f) * Mathf.Pow(Mathf.Max(0f, 1f - t / seconds), 0.6f);
                data[i] = Mathf.Clamp(lp * 0.45f * env, -1f, 1f);
            }
            return Make("hiss", data);
        }

        static AudioClip Purr()
        {
            const float seconds = 1.6f;
            int n = Mathf.RoundToInt(seconds * Rate);
            var data = new float[n];
            var rnd = new System.Random(9);
            float lp = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float w = (float)rnd.NextDouble() * 2f - 1f;
                lp += (w - lp) * 0.04f;   // rombo grave
                float pulse = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 2f * 26f), 4f);
                float breath = 0.55f + 0.45f * Mathf.Sin(t * Mathf.PI * 2f * 1.25f);   // dentro e fuori
                float env = Mathf.Min(1f, t / 0.1f) * Mathf.Min(1f, (seconds - t) / 0.3f);
                data[i] = Mathf.Clamp(lp * pulse * breath * env * 3.2f, -1f, 1f);
            }
            return Make("purr", data);
        }

        static float Peak(float f, float center, float width) { float d = (f - center) / width; return 1f / (1f + d * d); }
        static float Smooth(float x) { x = Mathf.Clamp01(x); return x * x * (3f - 2f * x); }

        static AudioClip Make(string name, float[] data)
        {
            AudioClip clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
