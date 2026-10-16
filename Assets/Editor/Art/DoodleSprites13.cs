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

        // labirinto -> deformazione (usata sia per il disegno sia dal builder per i collider)
        public const MazeWarp Maze2Warp = MazeWarp.ZigZag, TrapWarp = MazeWarp.Wave, Trap2Warp = MazeWarp.Swirl,
            Trap3Warp = MazeWarp.Bubble, DarkWarp = MazeWarp.Slant, Dark2Warp = MazeWarp.SwirlBack;
    }
}
