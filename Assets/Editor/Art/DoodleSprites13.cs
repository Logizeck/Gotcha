using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Deformazioni dei labirinti senza vespe: pareti a zig-zag, ondulate, a vortice, a bolle o inclinate.</summary>
    public enum MazeWarp { None, ZigZag, Wave, Swirl, SwirlBack, Bubble, Slant }

    public static partial class DoodleSprites
    {
        static float Tri(float t) => (float)(2.0 / Math.PI * Math.Asin(Math.Sin(Math.PI * t)));

        /// <summary>
        /// Deforma un punto del labirinto (coordinate locali). È una deformazione continua e senza pieghe: i corridoi
        /// restano collegati come prima (percorso verificato), ma le pareti diventano curve o inclinate.
        /// </summary>
        public static void WarpPoint(MazeWarp w, ref float x, ref float y)
        {
            float ox = x, oy = y;
            switch (w)
            {
                case MazeWarp.ZigZag: x = 0.88f * ox + 0.26f * Tri(oy); y = 0.95f * oy + 0.15f * Tri(ox / 1.1f); break;
                case MazeWarp.Wave: x = 0.95f * ox + 0.12f * (float)Math.Sin(1.3f * oy); y = 0.93f * oy + 0.28f * (float)Math.Sin(1.7f * ox); break;
                case MazeWarp.Swirl:
                case MazeWarp.SwirlBack:
                {
                    float k = w == MazeWarp.Swirl ? 0.6f : -0.55f;
                    float r = (float)Math.Sqrt(ox * ox + oy * oy);
                    float th = k * (float)Math.Pow(Math.Max(0f, 1f - r / 4.6f), 1.5);
                    float c = (float)Math.Cos(th), s = (float)Math.Sin(th);
                    x = 0.97f * (c * ox - s * oy); y = 0.97f * (s * ox + c * oy);
                    break;
                }
                case MazeWarp.Bubble: x = 0.93f * ox + 0.2f * (float)(Math.Sin(1.9f * oy) * Math.Cos(0.7f * ox)); y = 0.95f * oy + 0.22f * (float)Math.Sin(1.6f * ox + 0.5f * oy); break;
                case MazeWarp.Slant: x = 0.8f * ox + 0.16f * oy + 0.08f * (float)Math.Sin(2f * oy); y = oy + 0.1f * (float)Math.Sin(1.5f * ox); break;
            }
        }

        /// <summary>Segmenti spezzati in tratti corti e deformati.</summary>
        public static MazeSeg[] Warped(IList<MazeSeg> segs, MazeWarp w, float step = 0.25f)
        {
            if (w == MazeWarp.None) return new List<MazeSeg>(segs).ToArray();
            var outList = new List<MazeSeg>();
            foreach (MazeSeg s in segs)
            {
                float len = (float)Math.Sqrt((s.x2 - s.x1) * (s.x2 - s.x1) + (s.y2 - s.y1) * (s.y2 - s.y1));
                int n = Math.Max(1, (int)Math.Ceiling(len / step));
                for (int i = 0; i < n; i++)
                {
                    float ax = s.x1 + (s.x2 - s.x1) * i / n, ay = s.y1 + (s.y2 - s.y1) * i / n;
                    float bx = s.x1 + (s.x2 - s.x1) * (i + 1) / n, by = s.y1 + (s.y2 - s.y1) * (i + 1) / n;
                    WarpPoint(w, ref ax, ref ay);
                    WarpPoint(w, ref bx, ref by);
                    outList.Add(new MazeSeg(ax, ay, bx, by, s.color));
                }
            }
            return outList.ToArray();
        }

        public static MazeSeg WarpSeg(MazeSeg s, MazeWarp w)
        {
            float ax = s.x1, ay = s.y1, bx = s.x2, by = s.y2;
            WarpPoint(w, ref ax, ref ay);
            WarpPoint(w, ref bx, ref by);
            return new MazeSeg(ax, ay, bx, by, s.color);
        }

        /// <summary>Deforma una lista di coppie x,y (buche, posizioni).</summary>
        public static float[] WarpPairs(float[] xy, MazeWarp w)
        {
            var r = (float[])xy.Clone();
            for (int i = 0; i + 1 < r.Length; i += 2) WarpPoint(w, ref r[i], ref r[i + 1]);
            return r;
        }

        // ---------- Livello 16: labirinto con trappole circolare ----------

        static void Arc(List<MazeSeg> l, float r, float a0, float a1)
        {
            int n = Math.Max(1, (int)(Math.Abs(a1 - a0) / 4f));
            for (int i = 0; i < n; i++)
            {
                double t0 = (a0 + (a1 - a0) * i / n) * Math.PI / 180, t1 = (a0 + (a1 - a0) * (i + 1) / n) * Math.PI / 180;
                l.Add(new MazeSeg(r * (float)Math.Cos(t0), r * (float)Math.Sin(t0), r * (float)Math.Cos(t1), r * (float)Math.Sin(t1), NeonCyan));
            }
        }

        static void Radial(List<MazeSeg> l, float a, float r0, float r1)
        {
            float c = (float)Math.Cos(a * Math.PI / 180), s = (float)Math.Sin(a * Math.PI / 180);
            l.Add(new MazeSeg(r0 * c, r0 * s, r1 * c, r1 * s, NeonCyan));
        }

        /// <summary>
        /// Tre cerchi concentrici: il bordo (r 2.7), un anello (r 1.85) aperto in alto e uno (r 1.0) aperto in basso,
        /// con due muri radiali che obbligano a fare il giro lungo. Si parte in basso nel corridoio esterno, la stella è
        /// al centro. Percorso verificato con le buche (la sfera passa tra buca e parete).
        /// </summary>
        public static MazeSeg[] CircleTrapMaze()
        {
            var l = new List<MazeSeg>();
            Arc(l, 2.7f, 0f, 360f);
            Arc(l, 1.85f, 102f, 438f);   // varco in alto (78°-102°)
            Arc(l, 1.0f, -70f, 250f);    // varco in basso (250°-290°)
            Radial(l, 300f, 1.85f, 2.7f);   // il corridoio esterno si percorre a sinistra
            Radial(l, 135f, 1.0f, 1.85f);   // quello interno a destra
            return l.ToArray();
        }

        static float[] Polar(params float[] ar)
        {
            var r = new float[ar.Length];
            for (int i = 0; i < ar.Length; i += 2) { r[i] = ar[i + 1] * (float)Math.Cos(ar[i] * Math.PI / 180); r[i + 1] = ar[i + 1] * (float)Math.Sin(ar[i] * Math.PI / 180); }
            return r;
        }

        /// <summary>Buche (x,y) appoggiate a una parete, alternando lato.</summary>
        public static readonly float[] CircleTrapPits = Concat(Polar(225f, 1.95f, 185f, 2.6f, 140f, 1.95f, 45f, 1.75f, 10f, 1.1f, 330f, 1.75f), new[] { 0.42f, 0.3f });
        /// <summary>Laser come segmenti x1,y1,x2,y2: uno per corridoio, di traverso.</summary>
        public static readonly float[] CircleTrapLasers = { -2.62f, 0f, -1.93f, 0f, 1.08f, 0f, 1.77f, 0f };

        static float[] Concat(float[] a, float[] b) { var r = new float[a.Length + b.Length]; a.CopyTo(r, 0); b.CopyTo(r, a.Length); return r; }

        // labirinto -> deformazione (usata sia per il disegno sia dal builder per i collider)
        public const MazeWarp Maze2Warp = MazeWarp.ZigZag, TrapWarp = MazeWarp.Wave, Trap2Warp = MazeWarp.Swirl,
            Trap3Warp = MazeWarp.Bubble, DarkWarp = MazeWarp.Slant, Dark2Warp = MazeWarp.SwirlBack;
    }
}
