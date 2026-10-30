using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.70 per il livello 267 "The Unswattable Fly" (il resto riusa gli sprite esistenti: la
    /// cucina del mattino, la mosca, la paletta, lo spray, la torta, la nuvoletta dei pensieri, l'albero, la nebbia dello
    /// specchio, la stellina): la finestra a ghigliottina (il paesaggio fuori, la cornice col davanzale, il vetro di sopra
    /// fisso col nasello del gancio, il vetro di sotto che scorre, il gancio a mezzaluna), la mascherina antigas della
    /// mosca e la torta spiaccicata. Tutti i pezzi della finestra hanno l'origine al centro dell'apertura.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV70A() => new DoodleArt[]
        {
            V70A_WindowView(), V70A_WindowFrame(), V70A_PaneTop(), V70A_Sash(), V70A_Latch(), V70A_GasMask(), V70A_CakeSplat(),
        };

        // misure della finestra (origine al centro dell'apertura; il livello usa gli stessi numeri)
        const float V70A_HalfW = 1.15f, V70A_HalfH = 1.2f, V70A_Rail = 0.1f;

        static float Clamp01V70A(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        /// <summary>Fuori dalla finestra: cielo, sole, nuvolette, le colline e un bell'albero (la "casa" della mosca).</summary>
        public static DoodleArt V70A_WindowView()
        {
            var p = new DoodlePainter(-V70A_HalfW - 0.02f, -V70A_HalfH - 0.02f, V70A_HalfW + 0.02f, V70A_HalfH + 0.02f, 80) { Ink = 0.035f };
            Sdf all = Sd.Box(0f, 0f, V70A_HalfW + 0.02f, V70A_HalfH + 0.02f);
            p.FillFunc(all, (x, y) => Rgba.Lerp(Rgba.Hex("E9F7FF"), Rgba.Hex("8CCBF2"), Clamp01V70A((y + 0.4f) / 1.6f)));
            // sole e nuvolette
            p.Fill(Sd.Circle(0.66f, 0.74f, 0.3f), Rgba.Hex("FFF3B0", 0.6f));
            p.Shape(Sd.Circle(0.66f, 0.74f, 0.2f), Rgba.Hex("FFD54A"), 0.03f);
            foreach (var c in new[] { (-0.55f, 0.85f, 1f), (0.15f, 0.42f, 0.7f) })
            {
                float s = c.Item3;
                Sdf cloud = Sd.Union(Sd.Ellipse(c.Item1, c.Item2, 0.28f * s, 0.11f * s), Sd.Circle(c.Item1 - 0.08f * s, c.Item2 + 0.07f * s, 0.12f * s),
                    Sd.Circle(c.Item1 + 0.1f * s, c.Item2 + 0.05f * s, 0.1f * s));
                p.Fill(cloud, new Rgba(1f, 1f, 1f, 0.92f));
            }
            // colline
            Sdf hillBack = (x, y) => y - (-0.35f + 0.12f * (float)Math.Sin(x * 2.2f + 0.6f));
            p.Fill(Sd.Intersect(all, hillBack), Rgba.Hex("A6DB8A"), false);
            Sdf hillFront = (x, y) => y - (-0.72f + 0.1f * (float)Math.Sin(x * 3.1f - 0.4f));
            p.Fill(Sd.Intersect(all, hillFront), Rgba.Hex("7CC46A"), false);
            p.Line(Rgba.Hex("5FA552", 0.6f), 0.025f, -1.1f, -0.66f, -0.6f, -0.75f, 0f, -0.68f, 0.6f, -0.8f, 1.1f, -0.7f);
            // l'albero (a sinistra, dove andrà la mosca)
            Body(p, Sd.Box(-0.5f, -0.62f, 0.07f, 0.3f, 0.03f), Rgba.Hex("8A5A3C"), Rgba.Hex("6B4129"));
            Sdf crown = Sd.Union(Sd.Circle(-0.5f, -0.05f, 0.36f), Sd.Circle(-0.76f, -0.22f, 0.24f), Sd.Circle(-0.24f, -0.22f, 0.25f),
                Sd.Circle(-0.5f, 0.24f, 0.24f));
            Body(p, crown, Rgba.Hex("5CC24E"), Rgba.Hex("3E9A3A"));
            foreach (var a in new[] { (-0.62f, 0.02f), (-0.33f, -0.14f), (-0.48f, 0.26f), (-0.75f, -0.26f) })
                p.Shape(Sd.Circle(a.Item1, a.Item2, 0.045f), Rgba.Hex("E8433F"), 0.02f);
            // un cespuglio a destra e due fiori
            Body(p, Sd.Union(Sd.Circle(0.75f, -0.82f, 0.17f), Sd.Circle(0.95f, -0.84f, 0.14f), Sd.Circle(0.58f, -0.86f, 0.12f)), Rgba.Hex("66BF55"), Rgba.Hex("4AA040"));
            foreach (var f in new[] { (0.1f, -0.95f, "FF8FB1"), (0.3f, -1.05f, "FFD166") })
                p.Shape(Sd.Circle(f.Item1, f.Item2, 0.04f), Rgba.Hex(f.Item3), 0.018f);
            return new DoodleArt("v70_a_window_view", p);
        }

        /// <summary>La cornice di legno bianco intorno all'apertura, con il davanzale sporgente in basso.</summary>
        public static DoodleArt V70A_WindowFrame()
        {
            float ow = V70A_HalfW + 0.2f, oh = V70A_HalfH + 0.2f;
            var p = new DoodlePainter(-ow - 0.22f, -oh - 0.2f, ow + 0.22f, oh + 0.06f, 80) { Ink = 0.045f };
            Sdf hole = Sd.Box(0f, 0f, V70A_HalfW, V70A_HalfH);
            Sdf frame = Sd.Subtract(Sd.Box(0f, 0f, ow, oh, 0.04f), hole);
            Body(p, frame, Rgba.Hex("FBFAF5"), Rgba.Hex("E2DED2"));
            // l'ombra dentro l'apertura (spessore del muro)
            p.Fill(Sd.Subtract(Sd.Box(0f, 0f, V70A_HalfW, V70A_HalfH), Sd.Box(0.04f, -0.04f, V70A_HalfW - 0.02f, V70A_HalfH - 0.03f)), Rgba.Hex("2B2233", 0.18f), false);
            // il davanzale
            Sdf sill = Sd.Box(0f, -oh - 0.04f, ow + 0.18f, 0.1f, 0.03f);
            Body(p, sill, Rgba.Hex("FFFFFF"), Rgba.Hex("DCD7CA"));
            p.Line(Rgba.Hex("2B2233", 0.25f), 0.02f, -ow - 0.1f, -oh - 0.13f, ow + 0.1f, -oh - 0.13f);
            return new DoodleArt("v70_a_window_frame", p);
        }

        static void V70A_Glass(DoodlePainter p, float cy, float hh)
        {
            Sdf glass = Sd.Box(0f, cy, V70A_HalfW - V70A_Rail, hh);
            p.Fill(glass, new Rgba(0.86f, 0.95f, 1f, 0.22f), false);
            // riflessi obliqui
            float x0 = -0.6f, y0 = cy - hh * 0.5f;
            p.Fill(Sd.Intersect(glass, Sd.Capsule(x0, y0, x0 + 0.5f, y0 + hh * 1.2f, 0.06f)), new Rgba(1f, 1f, 1f, 0.38f));
            p.Fill(Sd.Intersect(glass, Sd.Capsule(x0 + 0.22f, y0, x0 + 0.62f, y0 + hh * 0.9f, 0.025f)), new Rgba(1f, 1f, 1f, 0.32f));
            p.Fill(Sd.Intersect(glass, Sd.Capsule(0.55f, cy - hh * 0.2f, 0.8f, cy + hh * 0.5f, 0.03f)), new Rgba(1f, 1f, 1f, 0.25f));
        }

        /// <summary>Il vetro di sopra (fisso), da y 0 a HalfH, con il nasello del gancio sul traverso basso.</summary>
        public static DoodleArt V70A_PaneTop()
        {
            var p = new DoodlePainter(-V70A_HalfW - 0.03f, -0.05f, V70A_HalfW + 0.03f, V70A_HalfH + 0.03f, 80) { Ink = 0.035f };
            float hh = V70A_HalfH * 0.5f, cy = hh;
            V70A_Glass(p, cy, hh - V70A_Rail);
            Sdf rails = Sd.Subtract(Sd.Box(0f, cy, V70A_HalfW, hh), Sd.Box(0f, cy, V70A_HalfW - V70A_Rail, hh - V70A_Rail));
            Body(p, rails, Rgba.Hex("F7F5EE"), Rgba.Hex("DDD8CA"));
            // il nasello (dove si aggancia la mezzaluna)
            Body(p, Sd.Box(0.17f, 0.05f, 0.08f, 0.04f, 0.015f), Rgba.Hex("E2B44A"), Rgba.Hex("B88A2A"));
            return new DoodleArt("v70_a_pane_top", p);
        }

        /// <summary>Il vetro di sotto (scorre in su), da -HalfH a poco sopra 0, con le due maniglie per alzarlo.</summary>
        public static DoodleArt V70A_Sash()
        {
            var p = new DoodlePainter(-V70A_HalfW - 0.03f, -V70A_HalfH - 0.03f, V70A_HalfW + 0.03f, 0.1f, 80) { Ink = 0.04f };
            float top = 0.05f, bot = -V70A_HalfH, hh = (top - bot) * 0.5f, cy = (top + bot) * 0.5f;
            V70A_Glass(p, cy, hh - V70A_Rail);
            Sdf rails = Sd.Subtract(Sd.Box(0f, cy, V70A_HalfW, hh), Sd.Box(0f, cy, V70A_HalfW - V70A_Rail, hh - V70A_Rail));
            Body(p, rails, Rgba.Hex("FFFFFF"), Rgba.Hex("E0DBCD"));
            // le maniglie per alzarlo, sul traverso basso
            foreach (float s in new[] { -1f, 1f })
                p.Shape(Sd.Polyline(0.025f, s * 0.55f - 0.09f, bot + 0.05f, s * 0.55f - 0.09f, bot + 0.13f, s * 0.55f + 0.09f, bot + 0.13f, s * 0.55f + 0.09f, bot + 0.05f), Rgba.Hex("E2B44A"), 0.025f);
            return new DoodleArt("v70_a_sash", p);
        }

        /// <summary>Il gancio a mezzaluna (perno in 0,0, la leva punta a destra verso il nasello quando è chiuso).</summary>
        public static DoodleArt V70A_Latch()
        {
            var p = new DoodlePainter(-0.14f, -0.14f, 0.34f, 0.14f, 200) { Ink = 0.025f };
            Sdf lever = Sd.Crescent(Sd.Ellipse(0.12f, 0f, 0.17f, 0.075f), 0f, -0.05f);
            Body(p, Sd.Union(Sd.Ellipse(0.12f, 0.01f, 0.17f, 0.06f), Sd.Capsule(0f, 0f, 0.26f, 0.01f, 0.035f)), Rgba.Hex("F2C45A"), Rgba.Hex("C9962E"));
            p.Fill(lever, Rgba.Hex("FFF3B0", 0.5f));
            Body(p, Sd.Circle(0f, 0f, 0.08f), Rgba.Hex("E2B44A"), Rgba.Hex("B88A2A"));
            p.Shape(Sd.Circle(0f, 0f, 0.025f), Rgba.Hex("7A5A1A"), 0.015f);
            return new DoodleArt("v70_a_latch", p);
        }

        /// <summary>La mascherina antigas della mosca (coordinate della mosca: testa in 0.18,0, guarda a destra).</summary>
        public static DoodleArt V70A_GasMask()
        {
            var p = new DoodlePainter(0.02f, -0.2f, 0.46f, 0.2f, 240) { Ink = 0.02f };
            // il laccio intorno alla testa
            p.Line(Rgba.Hex("3A3540"), 0.03f, 0.13f, -0.12f, 0.12f, 0f, 0.13f, 0.12f);
            // il musetto e il filtro tondo
            Body(p, Sd.Box(0.27f, 0f, 0.07f, 0.1f, 0.05f), Rgba.Hex("7C8C6A"), Rgba.Hex("5C6A4E"));
            p.Shape(Sd.Circle(0.24f, 0.065f, 0.032f), Rgba.Hex("D7F0FF", 0.9f), 0.015f);
            p.Shape(Sd.Circle(0.24f, -0.065f, 0.032f), Rgba.Hex("D7F0FF", 0.9f), 0.015f);
            Body(p, Sd.Circle(0.37f, 0f, 0.065f), Rgba.Hex("A7B1BA"), Rgba.Hex("7D8791"));
            foreach (float a in new[] { -0.03f, 0f, 0.03f }) p.Line(Rgba.Hex("3A3540", 0.7f), 0.01f, 0.35f, a, 0.4f, a);
            return new DoodleArt("v70_a_gas_mask", p);
        }

        /// <summary>La torta spiaccicata dalla paletta (centro 0,0): panna, glassa rosa, briciole e codette sparse sul piatto.</summary>
        public static DoodleArt V70A_CakeSplat()
        {
            var p = new DoodlePainter(-0.95f, -0.32f, 0.95f, 0.36f, 120) { Ink = 0.03f };
            Sdf blob = Sd.Union(Sd.Ellipse(0f, 0f, 0.62f, 0.2f), Sd.Circle(-0.55f, 0.06f, 0.12f), Sd.Circle(0.6f, 0.03f, 0.13f),
                Sd.Capsule(-0.3f, 0.05f, -0.42f, 0.24f, 0.05f), Sd.Capsule(0.25f, 0.06f, 0.38f, 0.26f, 0.045f), Sd.Capsule(0f, 0f, 0.05f, -0.24f, 0.05f),
                Sd.Circle(-0.78f, -0.08f, 0.06f), Sd.Circle(0.82f, 0.12f, 0.05f));
            Body(p, blob, Rgba.Hex("F2C28B"), Rgba.Hex("D9A06A"));
            Sdf cream = Sd.Union(Sd.Ellipse(-0.1f, 0.05f, 0.4f, 0.12f), Sd.Circle(0.3f, 0.02f, 0.1f), Sd.Circle(-0.42f, 0.0f, 0.08f));
            p.Shape(cream, Rgba.Hex("FFF6E8"), 0.022f);
            Sdf pink = Sd.Union(Sd.Ellipse(0.05f, 0.1f, 0.2f, 0.06f), Sd.Circle(-0.3f, 0.08f, 0.06f), Sd.Circle(0.35f, 0.12f, 0.05f));
            p.Shape(pink, Rgba.Hex("FF8FB1"), 0.02f);
            string[] cols = { "5B8DEF", "FFD166", "7FD18B", "E8433F" };
            var rnd = new Random(70);
            for (int i = 0; i < 14; i++)
            {
                float x = (float)(rnd.NextDouble() * 1.5 - 0.75), y = (float)(rnd.NextDouble() * 0.36 - 0.16);
                float a = (float)(rnd.NextDouble() * Math.PI);
                p.Line(Rgba.Hex(cols[i % cols.Length]), 0.025f, x, y, x + 0.05f * (float)Math.Cos(a), y + 0.05f * (float)Math.Sin(a));
            }
            foreach (var c in new[] { (-0.88f, 0.2f), (0.9f, -0.15f), (-0.65f, -0.22f), (0.7f, 0.25f) })
                p.Shape(Sd.Circle(c.Item1, c.Item2, 0.035f), Rgba.Hex("F2C28B"), 0.015f);
            return new DoodleArt("v70_a_cake_splat", p);
        }
    }
}
