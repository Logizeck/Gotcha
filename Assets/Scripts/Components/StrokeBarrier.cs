using System.Collections.Generic;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Barriere disegnate col dito (EdgeCollider2D sotto uno StrokeDrawer) usate come geometria pura: un corpo
    /// tondo che si muove da A a B non può né attraversarle né starci dentro. Si controlla sia la distanza dalla
    /// linea sia l'attraversamento del movimento (anche veloce), e il corpo resta sempre dal lato da cui arrivava.
    /// </summary>
    public static class StrokeBarrier
    {
        static readonly List<Vector2> segs = new List<Vector2>();

        /// <summary>
        /// Corregge <paramref name="to"/> se il movimento da <paramref name="from"/> urta una linea.
        /// Restituisce true se c'è stato un urto (con la normale verso il lato di provenienza).
        /// </summary>
        public static bool Resolve(Transform pen, Vector2 from, ref Vector2 to, float radius, float thickness, out Vector2 normal)
        {
            normal = Vector2.zero;
            if (pen == null) return false;
            Collect(pen);
            bool hitAny = false;
            for (int pass = 0; pass < 3; pass++)
            {
                bool hit = false;
                for (int i = 0; i < segs.Count; i += 2)
                {
                    Vector2 a = segs[i], b = segs[i + 1];
                    Vector2 ab = b - a;
                    if (ab.sqrMagnitude < 1e-8f) continue;
                    Vector2 n = new Vector2(-ab.y, ab.x).normalized;
                    float sideFrom = Vector2.Dot(from - a, n);
                    if (Mathf.Abs(sideFrom) < 1e-4f) sideFrom = Vector2.Dot(to - a, n);
                    Vector2 toward = sideFrom >= 0f ? n : -n;     // il lato da cui arriva il corpo
                    bool crosses = SegmentsCross(from, to, a, b);
                    Vector2 c = Closest(a, b, to);
                    float d = Vector2.Distance(to, c);
                    float r = radius + thickness;
                    if (!crosses && d >= r) continue;
                    // rimesso dal suo lato, appena fuori dalla linea
                    to = c + toward * (r + 0.02f);
                    normal = toward;
                    hit = hitAny = true;
                }
                if (!hit) break;
            }
            return hitAny;
        }

        static void Collect(Transform pen)
        {
            segs.Clear();
            foreach (EdgeCollider2D e in pen.GetComponentsInChildren<EdgeCollider2D>())
            {
                if (!e.enabled) continue;
                Vector2[] pts = e.points;
                for (int i = 0; i + 1 < pts.Length; i++)
                {
                    segs.Add(e.transform.TransformPoint(pts[i] + e.offset));
                    segs.Add(e.transform.TransformPoint(pts[i + 1] + e.offset));
                }
            }
        }

        static Vector2 Closest(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return a + ab * t;
        }

        static bool SegmentsCross(Vector2 p, Vector2 q, Vector2 a, Vector2 b)
        {
            float d1 = Cross(a, b, p), d2 = Cross(a, b, q), d3 = Cross(p, q, a), d4 = Cross(p, q, b);
            return d1 * d2 < 0f && d3 * d4 < 0f;
        }

        static float Cross(Vector2 o, Vector2 a, Vector2 b) { return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x); }
    }
}
