using System;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni della versione 0.29 (livelli 80, 85, 88, 98).</summary>
    public static partial class DoodleSprites
    {
        /// <summary>Giostra vista dall'alto: disco a spicchi colorati, cavallini, palo centrale (centro 0,0, raggio 1.9).</summary>
        public static DoodleArt CarouselTop()
        {
            var p = new DoodlePainter(-2.05f, -2.05f, 2.05f, 2.05f, 90) { Ink = 0.06f };
            string[] cols = { "FF5AA8", "FFE14A", "6FC3F2", "5DE07A", "FF8A2E", "8E5FD1" };
            p.FillFunc(Sd.Circle(0, 0, 1.9f), (x, y) =>
            {
                double a = Math.Atan2(y, x) + Math.PI;
                int k = (int)(a / (Math.PI * 2) * 12) % 12;
                return Rgba.Hex(cols[k % cols.Length]);
            });
            p.Shape(Sd.Circle(0, 0, 1.9f), Rgba.Clear, 0.06f);
            for (int i = 0; i < 6; i++)
            {
                double a = i * Math.PI / 3 + 0.26;
                float hx = (float)Math.Cos(a) * 1.35f, hy = (float)Math.Sin(a) * 1.35f;
                Body(p, Sd.Rotate(Sd.Ellipse(hx, hy, 0.32f, 0.13f), hx, hy, (float)(a * 180 / Math.PI) + 90f), Rgba.White, Rgba.Hex("D9DEE6"));   // cavallino
                p.Fill(Sd.Circle(hx, hy, 0.05f), Gold);
            }
            Body(p, Sd.Circle(0, 0, 0.4f), Gold, GoldShade);
            p.Shape(Sd.Circle(0, 0, 0.15f), Rgba.Hex("E8433F"), 0.03f);
            return new DoodleArt("carousel_top", p);
        }

        /// <summary>Cronometro (quadrante centro 0,0 raggio 1.3, lancetta a parte).</summary>
        public static DoodleArt Stopwatch()
        {
            var p = new DoodlePainter(-1.55f, -1.55f, 1.55f, 2.0f, 100) { Ink = 0.06f };
            Body(p, Sd.Box(0, 1.6f, 0.22f, 0.22f, 0.06f), Rgba.Hex("C9CFD6"), Rgba.Hex("8D93A6"));
            Body(p, Sd.Box(0, 1.85f, 0.4f, 0.1f, 0.05f), Rgba.Hex("E8433F"), Rgba.Hex("B92E2A"));
            Body(p, Sd.Circle(0, 0, 1.45f), Rgba.Hex("C9CFD6"), Rgba.Hex("8D93A6"));
            p.Shape(Sd.Circle(0, 0, 1.25f), Rgba.Hex("FFF6E0"), 0.04f);
            for (int i = 0; i < 60; i++)
            {
                double a = i * Math.PI / 30;
                float r0 = i % 5 == 0 ? 1.0f : 1.1f;
                p.InkLine(i % 5 == 0 ? 0.04f : 0.015f, (float)Math.Sin(a) * r0, (float)Math.Cos(a) * r0, (float)Math.Sin(a) * 1.2f, (float)Math.Cos(a) * 1.2f);
            }
            return new DoodleArt("stopwatch", p);
        }

        /// <summary>Carta da gioco (centro 0,0, 0.5 x 0.75) con un cuore.</summary>
        public static DoodleArt PlayingCard()
        {
            var p = new DoodlePainter(-0.3f, -0.42f, 0.3f, 0.42f, 160) { Ink = 0.025f };
            p.Shape(Sd.Box(0, 0, 0.25f, 0.37f, 0.04f), Rgba.White, 0.025f);
            Sdf heart = Sd.Union(Sd.Union(Sd.Circle(-0.05f, 0.04f, 0.06f), Sd.Circle(0.05f, 0.04f, 0.06f)), Sd.Polygon(-0.105f, 0.02f, 0.105f, 0.02f, 0f, -0.1f));
            p.Fill(heart, Rgba.Hex("E8433F"));
            p.Fill(Sd.Circle(-0.17f, 0.28f, 0.03f), Rgba.Hex("E8433F"));
            p.Fill(Sd.Circle(0.17f, -0.28f, 0.03f), Rgba.Hex("E8433F"));
            return new DoodleArt("playing_card", p);
        }

        /// <summary>Finestra con la tenda (la tenda è a parte: ruota dall'alto).</summary>
        public static DoodleArt WindowFrame()
        {
            var p = new DoodlePainter(-1.0f, -1.2f, 1.0f, 1.2f, 90) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 0.9f, 1.1f), (x, y) => Rgba.Lerp(Rgba.Hex("BFE6FA"), Rgba.Hex("6FC0EE"), Clamp((y + 1.1f) / 2.2f)));
            p.Shape(Sd.Subtract(Sd.Box(0, 0, 0.95f, 1.15f, 0.04f), Sd.Box(0, 0, 0.85f, 1.05f)), Rgba.White, 0.04f);
            p.Line(Rgba.White, 0.06f, 0, -1.05f, 0, 1.05f);
            p.Line(Rgba.White, 0.06f, -0.85f, 0.1f, 0.85f, 0.1f);
            return new DoodleArt("window_frame", p);
        }

        /// <summary>Tenda appesa (attacco in alto in 0,0, scende per 2.1).</summary>
        public static DoodleArt Curtain()
        {
            var p = new DoodlePainter(-0.55f, -2.2f, 0.55f, 0.1f, 90) { Ink = 0.04f };
            Sdf c = Sd.Polygon(-0.45f, 0f, 0.45f, 0f, 0.35f, -2.1f, -0.5f, -2.1f);
            p.FillFunc(c, (x, y) => ((int)Math.Floor((x + 1f) * 6f) & 1) == 0 ? Rgba.Hex("FF8FB0") : Rgba.Hex("F06A94"));
            p.Shape(c, Rgba.Clear, 0.04f);
            return new DoodleArt("curtain", p);
        }

        /// <summary>Cappellino minuscolo per i topolini della folla (base in 0,0).</summary>
        public static DoodleArt TinyHat(bool red)
        {
            var p = new DoodlePainter(-0.3f, -0.08f, 0.3f, 0.45f, 120) { Ink = 0.03f };
            p.Shape(Sd.Polygon(-0.22f, 0f, 0.22f, 0f, 0f, 0.4f), red ? Rgba.Hex("E8233F") : Rgba.Hex("3E6FE0"), 0.03f);
            p.Fill(Sd.Circle(0f, 0.4f, 0.06f), Rgba.White);
            return new DoodleArt(red ? "tiny_hat_red" : "tiny_hat_blue", p);
        }

        /// <summary>Occhi a spirale del gatto stordito.</summary>
        public static DoodleArt DizzyEyes()
        {
            var p = new DoodlePainter(-0.6f, -0.25f, 0.6f, 0.25f, 140) { Wobble = 0f };
            foreach (float sx in new[] { -1f, 1f })
            {
                var pts = new float[40];
                for (int i = 0; i < 20; i++) { double a = i * 0.7; float r = 0.01f * i; pts[i * 2] = sx * 0.3f + (float)Math.Cos(a) * r; pts[i * 2 + 1] = (float)Math.Sin(a) * r; }
                p.Fill(Sd.Circle(sx * 0.3f, 0, 0.22f), Rgba.White);
                p.InkLine(0.03f, pts);
            }
            return new DoodleArt("dizzy_eyes", p);
        }
    }
}
