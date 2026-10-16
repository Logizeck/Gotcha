using System.Collections.Generic;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Riconoscitore di forme disegnate col dito ("$1 recognizer"): il tratto viene ricampionato in 64 punti,
    /// ruotato sul suo angolo caratteristico, scalato in un quadrato e centrato; poi si confronta con i modelli
    /// (distanza media punto a punto, cercando la rotazione migliore entro ±45°). Punteggio 0..1.
    /// </summary>
    public static class ShapeRecognizer
    {
        const int N = 64;
        const float Size = 250f;
        static readonly float HalfDiagonal = 0.5f * Mathf.Sqrt(2f * Size * Size);

        /// <summary>Punteggio (0..1) del tratto contro un modello; il modello vale anche percorso al contrario.</summary>
        public static float Score(IList<Vector2> stroke, IList<Vector2> template)
        {
            if (stroke == null || stroke.Count < 4 || template == null || template.Count < 2) return 0f;
            List<Vector2> a = Normalize(stroke);
            List<Vector2> b = Normalize(template);
            var rev = new List<Vector2>(template);
            rev.Reverse();
            List<Vector2> c = Normalize(rev);
            float d = Mathf.Min(BestDistance(a, b), BestDistance(a, c));
            return 1f - d / HalfDiagonal;
        }

        static List<Vector2> Normalize(IList<Vector2> pts)
        {
            List<Vector2> p = Resample(pts, N);
            Vector2 c = Centroid(p);
            float angle = Mathf.Atan2(c.y - p[0].y, c.x - p[0].x);
            p = Rotate(p, -angle);
            // scala in un quadrato (senza deformare troppo le forme molto strette)
            Vector2 min = p[0], max = p[0];
            foreach (Vector2 q in p) { min = Vector2.Min(min, q); max = Vector2.Max(max, q); }
            Vector2 size = max - min;
            float w = Mathf.Max(size.x, 1e-3f), h = Mathf.Max(size.y, 1e-3f);
            float ratio = Mathf.Min(w, h) / Mathf.Max(w, h);
            for (int i = 0; i < p.Count; i++)
                p[i] = ratio < 0.3f ? p[i] * (Size / Mathf.Max(w, h)) : new Vector2(p[i].x * Size / w, p[i].y * Size / h);
            c = Centroid(p);
            for (int i = 0; i < p.Count; i++) p[i] -= c;
            return p;
        }

        static float BestDistance(List<Vector2> a, List<Vector2> b)
        {
            float phi = 0.5f * (-1f + Mathf.Sqrt(5f));
            float lo = -45f * Mathf.Deg2Rad, hi = 45f * Mathf.Deg2Rad, eps = 2f * Mathf.Deg2Rad;
            float x1 = phi * lo + (1f - phi) * hi, f1 = PathDistance(Rotate(a, x1), b);
            float x2 = (1f - phi) * lo + phi * hi, f2 = PathDistance(Rotate(a, x2), b);
            while (Mathf.Abs(hi - lo) > eps)
            {
                if (f1 < f2) { hi = x2; x2 = x1; f2 = f1; x1 = phi * lo + (1f - phi) * hi; f1 = PathDistance(Rotate(a, x1), b); }
                else { lo = x1; x1 = x2; f1 = f2; x2 = (1f - phi) * lo + phi * hi; f2 = PathDistance(Rotate(a, x2), b); }
            }
            return Mathf.Min(f1, f2);
        }

        static float PathDistance(List<Vector2> a, List<Vector2> b)
        {
            int n = Mathf.Min(a.Count, b.Count);
            float d = 0f;
            for (int i = 0; i < n; i++) d += Vector2.Distance(a[i], b[i]);
            return d / n;
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

        static Vector2 Centroid(List<Vector2> p)
        {
            Vector2 c = Vector2.zero;
            foreach (Vector2 q in p) c += q;
            return c / p.Count;
        }

        static List<Vector2> Rotate(List<Vector2> p, float angle)
        {
            float cs = Mathf.Cos(angle), sn = Mathf.Sin(angle);
            Vector2 c = Centroid(p);
            var r = new List<Vector2>(p.Count);
            foreach (Vector2 q in p)
            {
                Vector2 d = q - c;
                r.Add(new Vector2(d.x * cs - d.y * sn, d.x * sn + d.y * cs) + c);
            }
            return r;
        }
    }
}
