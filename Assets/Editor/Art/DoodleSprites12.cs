using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Logo della schermata del titolo: "G!" fatto di pareti al neon, come un labirinto.</summary>
    public static partial class DoodleSprites
    {
        const float LogoGx = -0.72f, LogoR = 1.25f, Logor = 0.68f, LogoW = 0.57f, LogoEx = 1.38f;

        /// <summary>
        /// Contorni chiusi delle pareti del logo (coordinate locali, centro 0,0): il canale della G (con la stanghetta),
        /// il canale del punto esclamativo e l'anello del puntino. Le palline rotolano dentro i canali.
        /// </summary>
        public static List<float[]> TitleLogoLoops()
        {
            var loops = new List<float[]>();
            var g = new List<float>();
            void Add(List<float> l, double x, double y) { l.Add((float)x); l.Add((float)y); }
            double cx = LogoGx, cy = 0, R = LogoR, r = Logor, w = LogoW;
            double a0 = 40 * Math.PI / 180;
            double ao = 2 * Math.PI - Math.Asin(w * 0.5 / R), ai = 2 * Math.PI - Math.Asin(w * 0.5 / r);
            for (int i = 0; i <= 40; i++) { double a = a0 + (ao - a0) * i / 40; Add(g, cx + R * Math.Cos(a), cy + R * Math.Sin(a)); }   // bordo esterno
            Add(g, cx + R, cy - w * 0.5);
            Add(g, cx + R, cy + w * 0.5);                       // fine della stanghetta (a destra)
            Add(g, cx + 0.12, cy + w * 0.5);
            Add(g, cx + 0.12, cy - w * 0.5);                    // la stanghetta entra verso il centro
            Add(g, cx + r * Math.Cos(ai), cy - w * 0.5);
            for (int i = 0; i <= 34; i++) { double a = ai + (a0 - ai) * i / 34; Add(g, cx + r * Math.Cos(a), cy + r * Math.Sin(a)); }   // bordo interno
            loops.Add(g.ToArray());
            // punto esclamativo: canale verticale con le estremità tonde
            var bar = new List<float>();
            double hw = w * 0.5, top = 1.25 - hw, bottom = -0.25;
            for (int i = 0; i <= 12; i++) { double a = Math.PI * i / 12; Add(bar, LogoEx + hw * Math.Cos(a), top + hw * Math.Sin(a)); }
            for (int i = 0; i <= 12; i++) { double a = Math.PI + Math.PI * i / 12; Add(bar, LogoEx + hw * Math.Cos(a), bottom + hw * Math.Sin(a)); }
            loops.Add(bar.ToArray());
            // puntino: un anello
            var dot = new List<float>();
            for (int i = 0; i < 24; i++) { double a = Math.PI * 2 * i / 24; Add(dot, LogoEx + 0.33 * Math.Cos(a), -0.98 + 0.33 * Math.Sin(a)); }
            loops.Add(dot.ToArray());
            return loops;
        }

        public static DoodleArt TitleLogo()
        {
            var segs = new List<MazeSeg>();
            foreach (float[] l in TitleLogoLoops())
            {
                int n = l.Length / 2;
                for (int i = 0; i < n; i++)
                {
                    int j = (i + 1) % n;
                    segs.Add(new MazeSeg(l[i * 2], l[i * 2 + 1], l[j * 2], l[j * 2 + 1], NeonCyan));
                }
            }
            var p = new DoodlePainter(-2.15f, -1.5f, 1.95f, 1.5f, 150) { Wobble = 0f };
            NeonTubes(p, Sd.Box(-0.1f, 0, 2.1f, 1.5f), segs, true);
            return new DoodleArt("title_logo", p);
        }
    }
}
