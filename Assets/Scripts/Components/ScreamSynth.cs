using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Urlo femminile da cartone ("AAAAAH!") sintetizzato al volo, se non c'è un file audio vero
    /// (Assets/Audio/Scream.wav ha la precedenza). Voce acuta con le formanti della vocale "A", attacco di colpo,
    /// salita di tono, vibrato isterico, un filo di soffio e chiusura che si spezza.
    /// </summary>
    public static class ScreamSynth
    {
        /// <summary>Urletto di topolino: lo stesso urlo, ma acutissimo e più corto.</summary>
        public static AudioClip Mouse() { return Create(1.1f, 22050, 1.85f); }

        public static AudioClip Create(float seconds = 1.7f, int rate = 22050, float pitch = 1f)
        {
            int n = Mathf.RoundToInt(seconds * rate);
            var data = new float[n];
            // formanti di una "A" urlata da voce femminile (frequenza, larghezza, guadagno)
            float fk = Mathf.Min(pitch, 1.5f);   // le formanti salgono meno del tono (bocca piccola)
            float[] fF = { 1050f * fk, 1650f * fk, 3000f * fk, 4100f * fk };
            float[] fW = { 220f, 260f, 380f, 500f };
            float[] fG = { 1f, 0.7f, 0.45f, 0.2f };
            const int harmonics = 14;
            var phase = new float[harmonics];
            var rnd = new System.Random(11);
            float noiseLp = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate, k = t / seconds;
                // tono: scatta su, sale ancora, trema e alla fine crolla
                float f0 = pitch * (780f + 360f * Mathf.SmoothStep(0f, 1f, Mathf.Min(1f, t / 0.18f)) + 90f * k
                           + Mathf.Sin(t * Mathf.PI * 2f * 7.5f) * 38f * Mathf.Min(1f, t / 0.3f)
                           - 420f * Mathf.Pow(Mathf.Max(0f, (k - 0.82f) / 0.18f), 2f));
                float s = 0f;
                for (int h = 0; h < harmonics; h++)
                {
                    float fh = f0 * (h + 1);
                    if (fh > rate * 0.45f) break;
                    phase[h] += fh / rate;
                    if (phase[h] > 1f) phase[h] -= 1f;
                    float g = 0f;
                    for (int j = 0; j < fF.Length; j++)
                    {
                        float d = (fh - fF[j]) / fW[j];
                        g += fG[j] / (1f + d * d);
                    }
                    s += Mathf.Sin(phase[h] * Mathf.PI * 2f) * g / Mathf.Sqrt(h + 1);
                }
                // soffio (aria nella gola) e un po' di ruvidità
                float white = (float)rnd.NextDouble() * 2f - 1f;
                noiseLp += (white - noiseLp) * 0.35f;
                s += noiseLp * 0.18f;
                s *= 1f + 0.12f * Mathf.Sin(t * Mathf.PI * 2f * 31f);
                float env = Mathf.Min(1f, t / 0.03f) * Mathf.Min(1f, (seconds - t) / 0.25f);
                data[i] = Mathf.Clamp(s * 0.32f * env, -1f, 1f);
            }
            AudioClip clip = AudioClip.Create("scream", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
