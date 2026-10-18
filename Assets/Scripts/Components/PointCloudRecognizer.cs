using System.Collections.Generic;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Riconoscitore di forme "a nuvola di punti" ($P): confronta la FORMA del tratto, non l'ordine in cui è stato
    /// disegnato, quindi non importa da dove si parte né in che verso si gira (la stella va bene sia a pentagramma
    /// sia col contorno, partendo da qualsiasi punta). Prova anche piccole rotazioni (±16°) del disegno.
    /// Distanza: 0 = identico; per le rune un buon disegno sta sotto 1,6.
    /// </summary>
    public static class PointCloudRecognizer
    {
        const int N = 32;
        static readonly float[] Angles = { -16f, -8f, 0f, 8f, 16f };

        /// <summary>Prepara un modello (da fare una volta sola).</summary>
        public static Vector2[] Prepare(IList<Vector2> pts) => Normalize(pts);

        /// <summary>Distanza minima tra il tratto e uno qualsiasi dei modelli (varianti della stessa forma).</summary>
        public static float Distance(IList<Vector2> stroke, IList<Vector2[]> templates)
        {
            if (stroke == null || stroke.Count < 4) return float.MaxValue;
            float best = float.MaxValue;
            foreach (float deg in Angles)
            {
                Vector2[] s = Normalize(Rotate(stroke, deg * Mathf.Deg2Rad));
                foreach (Vector2[] t in templates) best = Mathf.Min(best, Greedy(s, t));
            }
            return best;
        }

        static List<Vector2> Rotate(IList<Vector2> p, float a)
        {
            float c = Mathf.Cos(a), s = Mathf.Sin(a);
            var r = new List<Vector2>(p.Count);
            foreach (Vector2 q in p) r.Add(new Vector2(q.x * c - q.y * s, q.x * s + q.y * c));
            return r;
        }

        static Vector2[] Normalize(IList<Vector2> pts)
        {
            List<Vector2> p = Resample(pts, N);
            Vector2 min = p[0], max = p[0];
            foreach (Vector2 q in p) { min = Vector2.Min(min, q); max = Vector2.Max(max, q); }
            float size = Mathf.Max(max.x - min.x, max.y - min.y, 1e-3f);
            Vector2 c = Vector2.zero;
            for (int i = 0; i < p.Count; i++) { p[i] = (p[i] - min) / size; c += p[i]; }
            c /= p.Count;
            var o = new Vector2[N];
            for (int i = 0; i < N; i++) o[i] = p[i] - c;
            return o;
        }

        static float Greedy(Vector2[] a, Vector2[] b)
        {
            int step = Mathf.FloorToInt(Mathf.Sqrt(N));
            float min = float.MaxValue;
            for (int i = 0; i < N; i += step) min = Mathf.Min(min, Mathf.Min(Cloud(a, b, i), Cloud(b, a, i)));
            return min;
        }

        static float Cloud(Vector2[] a, Vector2[] b, int start)
        {
            var matched = new bool[N];
            float sum = 0f;
            int i = start;
            do
            {
                float best = float.MaxValue;
                int index = 0;
                for (int j = 0; j < N; j++)
                {
                    if (matched[j]) continue;
                    float d = Vector2.Distance(a[i], b[j]);
                    if (d < best) { best = d; index = j; }
                }
                matched[index] = true;
                sum += (1f - ((i - start + N) % N) / (float)N) * best;
                i = (i + 1) % N;
            } while (i != start);
            return sum;
        }

        static List<Vector2> Resample(IList<Vector2> pts, int n)
        {
            float total = 0f;
            for (int i = 1; i < pts.Count; i++) total += Vector2.Distance(pts[i - 1], pts[i]);
            float interval = total / (n - 1), acc = 0f;
            var src = new List<Vector2>(pts);
            var outp = new List<Vector2> { src[0] };
            for (int i = 1; i < src.Count; i++)
            {
                float d = Vector2.Distance(src[i - 1], src[i]);
                if (acc + d >= interval && d > 0f)
                {
                    Vector2 q = src[i - 1] + (interval - acc) / d * (src[i] - src[i - 1]);
                    outp.Add(q);
                    src.Insert(i, q);
                    acc = 0f;
                }
                else acc += d;
            }
            while (outp.Count < n) outp.Add(src[src.Count - 1]);
            if (outp.Count > n) outp.RemoveRange(n, outp.Count - n);
            return outp;
        }
    }
}
