using System;

namespace Gotcha.EditorArt
{
    /// <summary>Livelli di Toni 125 (SWEET DREAMS: nuvoletta, ciabatta, orsetto) e 150 (FETCH!: parco, stagno, gomitolo).</summary>
    public static partial class DoodleSprites
    {
        /// <summary>
        /// Nuvoletta del sogno: nuvola grande (centro 0,0, circa 3×2,2) azzurrina all'interno, con tre pallini che
        /// scendono in basso a sinistra verso la testa di chi sogna (l'ultimo in -1,5, -1,9).
        /// </summary>
        public static DoodleArt DreamCloud()
        {
            var p = new DoodlePainter(-1.8f, -2.1f, 1.7f, 1.25f, 90) { Ink = 0.05f };
            Sdf cloud = Sd.Union(Sd.Ellipse(0, -0.15f, 1.35f, 0.75f), Sd.Circle(-0.85f, 0.1f, 0.55f), Sd.Circle(-0.25f, 0.5f, 0.6f),
                Sd.Circle(0.5f, 0.45f, 0.58f), Sd.Circle(1.05f, 0.0f, 0.5f), Sd.Circle(0.45f, -0.6f, 0.5f), Sd.Circle(-0.5f, -0.6f, 0.5f));
            p.FillFunc(cloud, (x, y) => Rgba.Lerp(Rgba.Hex("E9F1FF"), Rgba.Hex("C9D9FF"), Math.Max(0f, Math.Min(1f, (0.9f - y) / 1.8f))));
            p.Fill(Sd.Crescent(cloud, -0.08f, 0.1f), Rgba.Hex("B6C8F5"));
            p.Shape(cloud, Rgba.Clear);
            // pallini del pensiero
            foreach (var d in new[] { (-1.0f, -1.12f, 0.2f), (-1.28f, -1.55f, 0.14f), (-1.5f, -1.9f, 0.09f) })
                p.Shape(Sd.Circle(d.Item1, d.Item2, d.Item3), Rgba.Hex("E1EBFF"), 0.04f);
            // stelline del sogno
            foreach (var s in new[] { (-1.0f, 0.55f), (1.15f, 0.6f), (1.2f, -0.55f), (-1.15f, -0.45f) })
                p.Fill(Sd.Polygon(s.Item1, s.Item2 + 0.1f, s.Item1 + 0.03f, s.Item2 + 0.03f, s.Item1 + 0.1f, s.Item2, s.Item1 + 0.03f, s.Item2 - 0.03f,
                    s.Item1, s.Item2 - 0.1f, s.Item1 - 0.03f, s.Item2 - 0.03f, s.Item1 - 0.1f, s.Item2, s.Item1 - 0.03f, s.Item2 + 0.03f), Rgba.Hex("FFE36E"));
            return new DoodleArt("dream_cloud", p);
        }

        /// <summary>Ciabatta a quadretti vista di lato (centro 0,0, lunga circa 1,2).</summary>
        public static DoodleArt Slipper()
        {
            var p = new DoodlePainter(-0.68f, -0.3f, 0.68f, 0.32f, 150) { Ink = 0.03f };
            Sdf sole = Sd.Box(0, -0.18f, 0.6f, 0.07f, 0.06f);
            Sdf top = Sd.Intersect(Sd.Ellipse(0.12f, -0.12f, 0.5f, 0.36f), Sd.Box(0, 0.05f, 0.7f, 0.17f));
            Body(p, top, Rgba.Hex("7FA7E8"), Rgba.Hex("5C84C8"), null);
            p.Hatch(top, Rgba.Hex("4E6FB0"), 0.12f, 0.025f, 0f);
            p.Hatch(top, Rgba.Hex("4E6FB0"), 0.12f, 0.025f, 90f);
            p.Shape(top, Rgba.Clear);
            p.Shape(sole, Rgba.Hex("F2E3C6"));
            p.Shape(Sd.Circle(0.38f, 0.12f, 0.08f), Rgba.White, 0.025f);   // pompon
            return new DoodleArt("slipper", p);
        }

        /// <summary>Orsetto di peluche seduto (centro 0,0, alto circa 1,1).</summary>
        public static DoodleArt TeddyBear()
        {
            var p = new DoodlePainter(-0.55f, -0.58f, 0.55f, 0.6f, 150) { Ink = 0.03f };
            Rgba fur = Rgba.Hex("C98B52"), shade = Rgba.Hex("A86E3C"), pale = Rgba.Hex("F0D2A8");
            Sdf body = Sd.Ellipse(0, -0.28f, 0.34f, 0.3f);
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Circle(s * 0.25f, -0.5f, 0.12f), fur, shade);       // piedi
                Body(p, Sd.Capsule(s * 0.3f, -0.12f, s * 0.42f, -0.3f, 0.09f), fur, shade);   // braccia
            }
            Body(p, body, fur, shade);
            p.Shape(Sd.Ellipse(0, -0.3f, 0.18f, 0.16f), pale, 0.02f);
            foreach (float s in new[] { -1f, 1f }) { Body(p, Sd.Circle(s * 0.22f, 0.42f, 0.1f), fur, shade); p.Fill(Sd.Circle(s * 0.22f, 0.42f, 0.05f), pale); }
            Sdf head = Sd.Circle(0, 0.22f, 0.27f);
            Body(p, head, fur, shade);
            p.Shape(Sd.Ellipse(0, 0.14f, 0.12f, 0.09f), pale, 0.02f);
            p.Fill(Sd.Ellipse(0, 0.18f, 0.045f, 0.03f), Rgba.Hex("2B2133"));
            foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Circle(s * 0.1f, 0.3f, 0.032f), Rgba.Hex("2B2133"));
            p.InkLine(0.015f, -0.04f, 0.1f, 0f, 0.08f, 0.04f, 0.1f);
            return new DoodleArt("teddy_bear", p);
        }

        /// <summary>Parco (9×14, centro 0,0): cielo, colline lontane, prato da y 0,9 in giù con un sentierino.</summary>
        public static DoodleArt ParkBackground()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Wobble = 0.004f };
            p.FillFunc(Sd.Box(0, 0, 4.5f, 7f), (x, y) => Rgba.Lerp(Rgba.Hex("BFE6FF"), Rgba.Hex("7CC4F2"), Math.Max(0f, Math.Min(1f, (y - 1f) / 5f))));
            p.Fill(Sd.Union(Sd.Ellipse(-2.6f, 0.9f, 2.6f, 1.1f), Sd.Ellipse(1.8f, 0.8f, 3.2f, 1.25f)), Rgba.Hex("9FD38A"));
            Sdf grass = Sd.Box(0, -3.1f, 4.5f, 4.0f);
            p.FillFunc(grass, (x, y) => Rgba.Lerp(Rgba.Hex("6CC160"), Rgba.Hex("85D275"), Math.Max(0f, Math.Min(1f, (y + 6f) / 6f))));
            p.InkLine(0.05f, -4.5f, 0.9f, -2f, 0.95f, 1f, 0.88f, 4.5f, 0.92f);
            p.Fill(Sd.Polygon(-0.9f, -7f, 0.9f, -7f, 0.35f, -2.2f, -0.2f, -2.2f), Rgba.Hex("E8D3A6"));
            var rnd = new Random(150);
            for (int i = 0; i < 70; i++)
            {
                float x = (float)rnd.NextDouble() * 9f - 4.5f, y = -6.8f + (float)rnd.NextDouble() * 7.4f;
                p.Line(Rgba.Hex("4FA448"), 0.03f, x, y, x + 0.04f, y + 0.18f);
            }
            foreach (var c in new[] { (-3.2f, 4.6f), (2.4f, 5.3f) })
                p.Shape(Sd.Union(Sd.Ellipse(c.Item1, c.Item2, 0.8f, 0.3f), Sd.Circle(c.Item1 - 0.25f, c.Item2 + 0.2f, 0.32f), Sd.Circle(c.Item1 + 0.25f, c.Item2 + 0.22f, 0.38f)), Rgba.White, 0.04f);
            return new DoodleArt("park_bg", p);
        }

        /// <summary>Stagno visto dall'alto in prospettiva (centro 0,0, 4,6×1,7) con riva di sassi e riflessi.</summary>
        public static DoodleArt ParkPond()
        {
            var p = new DoodlePainter(-2.45f, -0.95f, 2.45f, 0.95f, 100) { Ink = 0.04f };
            Sdf shore = Sd.Ellipse(0, 0, 2.35f, 0.88f);
            Sdf water = Sd.Ellipse(0, 0.02f, 2.15f, 0.72f);
            p.Shape(shore, Rgba.Hex("B9A27E"));
            p.FillFunc(water, (x, y) => Rgba.Lerp(Rgba.Hex("3E9BD1"), Rgba.Hex("7DCBF0"), Math.Max(0f, Math.Min(1f, (y + 0.7f) / 1.4f))));
            p.Shape(water, Rgba.Clear, 0.03f);
            foreach (var r in new[] { (-1.1f, 0.25f, 0.45f), (0.7f, -0.2f, 0.6f), (1.3f, 0.35f, 0.3f), (-0.4f, -0.35f, 0.35f) })
                p.Line(Rgba.Hex("D6F1FF"), 0.035f, r.Item1 - r.Item3, r.Item2, r.Item1 - r.Item3 * 0.3f, r.Item2 + 0.04f, r.Item1 + r.Item3 * 0.3f, r.Item2 + 0.04f, r.Item1 + r.Item3, r.Item2);
            foreach (var l in new[] { (1.7f, -0.3f), (-1.75f, 0.2f) })
            {
                p.Shape(Sd.Subtract(Sd.Ellipse(l.Item1, l.Item2, 0.22f, 0.12f), Sd.Polygon(l.Item1, l.Item2, l.Item1 + 0.3f, l.Item2 + 0.06f, l.Item1 + 0.3f, l.Item2 - 0.06f)), Rgba.Hex("6CC160"), 0.025f);
            }
            return new DoodleArt("park_pond", p);
        }

        /// <summary>Gomitolo di lana rosa con il filo che scappa (centro 0,0, diametro 0,7).</summary>
        public static DoodleArt YarnBall()
        {
            var p = new DoodlePainter(-0.45f, -0.4f, 0.75f, 0.4f, 150) { Ink = 0.03f };
            Sdf ball = Sd.Circle(0, 0, 0.35f);
            Body(p, ball, Rgba.Hex("FF8FB8"), Rgba.Hex("E36A98"));
            foreach (float a in new[] { -0.2f, 0f, 0.2f })
                p.Line(Rgba.Hex("C94F80"), 0.025f, -0.3f, a - 0.1f, -0.1f, a + 0.08f, 0.15f, a + 0.1f, 0.3f, a - 0.05f);
            p.Line(Rgba.Hex("C94F80"), 0.025f, -0.2f, 0.25f, 0.05f, -0.05f, 0.2f, -0.28f);
            p.Shape(ball, Rgba.Clear);
            p.Line(Rgba.Hex("E36A98"), 0.03f, 0.3f, -0.2f, 0.5f, -0.3f, 0.62f, -0.18f, 0.72f, -0.3f);
            return new DoodleArt("yarn_ball", p);
        }
    }
}
