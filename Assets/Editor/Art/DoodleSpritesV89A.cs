using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.89 per il livello 299 "The Impossible Print" (id lost_sheet): la scrivania senza contratto
    /// (coordinate del mondo, nello stesso ufficio di PAPERWORK NIGHTMARE: sfondo "s41c_office_bg"), la stampante in due
    /// pezzi (il vassoio d'ingresso vuoto DIETRO, che sporge in alto, e il corpo con il pannello, lo schermino, la spia, la
    /// fessura d'ingresso in cima e quella d'uscita davanti col vassoietto: il foglio che entra passa fra i due pezzi,
    /// niente SpriteMask), il pulsante PRINT (la scritta è un testo), la cucitrice e il foglio stampato con l'uovo d'oro
    /// (la scritta "You rock!" è un testo sotto l'uovo). Il foglio portato da PAPERWORK NIGHTMARE è proprio "s41c_sheet",
    /// il cestino "s41c_fancy_bin", la spia e i bagliori "glow", le scintille "sparkle_star". Prefisso v89_a_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV89A() => new DoodleArt[]
        {
            V89A_Desk(), V89A_PrinterBack(), V89A_Printer(), V89A_Button(), V89A_Stapler(), V89A_Page(),
        };

        // misure condivise col builder (coordinate locali della stampante: origine al centro della base, scala 1)
        /// <summary>Schermino (centro) e mezza larghezza; spia; pulsante PRINT; fessura d'ingresso; fessura d'uscita.</summary>
        public const float V89ALcdX = -0.3f, V89ALcdY = 0.82f, V89ALcdHalfW = 0.46f;
        public const float V89ALightX = 0.27f, V89ALightY = 0.82f;
        public const float V89AButtonX = 0.64f, V89AButtonY = 0.82f;
        public const float V89AInSlotY = 1.17f, V89AOutSlotY = 0.42f;
        /// <summary>Piano della scrivania nel mondo: bordo dietro e bordo davanti.</summary>
        public const float V89ADeskBackY = -0.55f, V89ADeskFrontY = -1.75f;

        static readonly Rgba V89APlastic = Rgba.Hex("EEF0F6"), V89APlasticShade = Rgba.Hex("C9CDDA"), V89ASlot = Rgba.Hex("3A3446");

        /// <summary>Uovo (più stretto in alto) come distanza approssimata.</summary>
        static Sdf V89A_Egg(float cx, float cy, float rx, float ry) => (x, y) =>
        {
            float k = 1f - 0.16f * Math.Max(-1f, Math.Min(1f, (y - cy) / ry));
            return Sd.Ellipse(cx, cy, rx * k, ry)(x, y);
        };

        /// <summary>Stellina a quattro punte.</summary>
        static Sdf V89A_Star(float cx, float cy, float r) =>
            Sd.Polygon(cx, cy + r, cx + r * 0.25f, cy + r * 0.25f, cx + r, cy, cx + r * 0.25f, cy - r * 0.25f, cx, cy - r, cx - r * 0.25f, cy - r * 0.25f, cx - r, cy, cx - r * 0.25f, cy + r * 0.25f);

        /// <summary>Scrivania di legno senza niente sopra (coordinate del mondo): piano da y -0.55 a -1.75, gambe fino a -3.5.</summary>
        public static DoodleArt V89A_Desk()
        {
            var p = new DoodlePainter(-2.95f, -3.6f, 2.95f, -0.4f, 70) { Ink = 0.05f };
            foreach (float x in new[] { -2.4f, 2.4f }) p.Shape(Sd.Box(x, -2.8f, 0.17f, 0.72f), WoodShade);
            p.Shape(Sd.Box(0f, -1.95f, 2.85f, 0.2f, 0.03f), WoodDark);
            Sdf top = Sd.Polygon(-2.6f, V89ADeskBackY, 2.6f, V89ADeskBackY, 2.8f, V89ADeskFrontY, -2.8f, V89ADeskFrontY);
            p.FillFunc(top, (x, y) => Rgba.Lerp(Wood, WoodShade, Noise.Value(x * 2.5f, y * 12f) * 0.25f));
            p.Shape(top, Rgba.Clear);
            for (float y = -1.45f; y > -1.75f && y < -0.6f; y += 0.4f) p.Line(WoodShade.WithAlpha(0.6f), 0.025f, -2.6f, y, 2.6f, y);
            p.Line(WoodShade.WithAlpha(0.6f), 0.025f, -2.62f, -1.05f, 2.62f, -1.05f);
            // cassetto con pomello
            p.Shape(Sd.Box(-1.3f, -1.95f, 0.6f, 0.12f, 0.03f), Rgba.Hex("8A4A22"), 0.03f);
            p.Shape(Sd.Circle(-1.3f, -1.95f, 0.05f), Rgba.Hex("E8C77A"), 0.02f);
            return new DoodleArt("v89_a_desk", p);
        }

        /// <summary>Il vassoio d'ingresso della carta, VUOTO, che sporge dietro la stampante (y 1.0..1.9).</summary>
        public static DoodleArt V89A_PrinterBack()
        {
            var p = new DoodlePainter(-0.85f, 0.9f, 0.85f, 2.0f, 110) { Ink = 0.04f };
            Sdf tray = Sd.Polygon(-0.74f, 1.0f, 0.74f, 1.0f, 0.62f, 1.9f, -0.62f, 1.9f);
            Body(p, tray, Rgba.Hex("D5D8E2"), Rgba.Hex("B6BACA"));
            // guide della carta e la scritta "vuoto" fatta di righine
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Box(s * 0.5f, 1.45f, 0.04f, 0.32f, 0.02f), Rgba.Hex("A6AABB"), 0.02f);
            p.Line(Rgba.Hex("A6AABB"), 0.02f, -0.25f, 1.7f, 0.25f, 1.7f);
            return new DoodleArt("v89_a_printer_back", p);
        }

        /// <summary>
        /// Il corpo della stampante (origine al centro della base, largo 2, alto 1.25): fessura d'ingresso in cima, pannello
        /// con lo schermino verde (testo sopra), la spia spenta (il bagliore la colora), il posto del pulsante PRINT, la
        /// fessura d'uscita e il vassoietto davanti, i piedini.
        /// </summary>
        public static DoodleArt V89A_Printer()
        {
            var p = new DoodlePainter(-1.1f, -0.1f, 1.1f, 1.35f, 110) { Ink = 0.045f };
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Box(s * 0.78f, 0.02f, 0.12f, 0.05f, 0.02f), Rgba.Hex("4A4456"), 0.025f);
            Sdf body = Sd.Box(0f, 0.66f, 1.0f, 0.58f, 0.16f);
            Body(p, body, V89APlastic, V89APlasticShade);
            p.Fill(Sd.Box(0f, 1.15f, 0.92f, 0.06f, 0.04f), Rgba.Hex("FAFBFF"));
            p.Shape(Sd.Box(0f, V89AInSlotY, 0.7f, 0.035f, 0.02f), V89ASlot, 0.02f);
            p.Shape(Sd.Box(0f, 0.82f, 0.9f, 0.22f, 0.06f), Rgba.Hex("DADDE8"), 0.03f);
            p.Shape(Sd.Box(V89ALcdX, V89ALcdY, V89ALcdHalfW, 0.14f, 0.04f), Rgba.Hex("9FD4A4"), 0.03f);
            p.Fill(Sd.Box(V89ALcdX - 0.3f, V89ALcdY + 0.07f, 0.12f, 0.02f), Rgba.White.WithAlpha(0.45f));
            p.Shape(Sd.Circle(V89ALightX, V89ALightY, 0.07f), Rgba.Hex("6A6676"), 0.025f);
            p.Shape(Sd.Box(V89AButtonX, V89AButtonY, 0.25f, 0.16f, 0.07f), Rgba.Hex("BFC3D1"), 0.025f);
            p.Shape(Sd.Box(0f, V89AOutSlotY, 0.75f, 0.04f, 0.02f), V89ASlot, 0.02f);
            Body(p, Sd.Polygon(-0.78f, 0.36f, 0.78f, 0.36f, 0.94f, 0.1f, -0.94f, 0.1f), V89APlasticShade, Rgba.Hex("AEB2C2"));
            return new DoodleArt("v89_a_printer", p);
        }

        /// <summary>Il pulsante PRINT (pillola verde, origine al centro; la scritta è un testo).</summary>
        public static DoodleArt V89A_Button()
        {
            var p = new DoodlePainter(-0.28f, -0.19f, 0.28f, 0.19f, 160) { Ink = 0.03f };
            Body(p, Sd.Box(0f, 0f, 0.22f, 0.13f, 0.08f), Rgba.Hex("5BC46A"), Rgba.Hex("3E9A4E"));
            return new DoodleArt("v89_a_button", p);
        }

        /// <summary>La cucitrice rossa (origine al centro della base, lunga 0.85).</summary>
        public static DoodleArt V89A_Stapler()
        {
            var p = new DoodlePainter(-0.5f, -0.05f, 0.5f, 0.42f, 140) { Ink = 0.035f };
            p.Shape(Sd.Box(0f, 0.05f, 0.43f, 0.05f, 0.03f), Rgba.Hex("4A4456"), 0.03f);
            Body(p, Sd.Polygon(-0.42f, 0.13f, 0.38f, 0.15f, 0.43f, 0.24f, 0.36f, 0.32f, -0.4f, 0.3f), Rgba.Hex("E8483B"), Rgba.Hex("B8332A"));
            p.Shape(Sd.Circle(-0.36f, 0.2f, 0.06f), Rgba.Hex("C9C9D2"), 0.02f);
            p.Fill(Sd.Box(0.05f, 0.28f, 0.22f, 0.015f), Rgba.White.WithAlpha(0.4f));
            return new DoodleArt("v89_a_stapler", p);
        }

        /// <summary>
        /// Il foglio stampato (come "s41c_sheet": 0.88 x 1.12, centro 0,0) con l'uovo d'oro lucido in alto (centro 0,0.13),
        /// stelline intorno; sotto c'è posto per la scritta "You rock!" (testo in y -0.36).
        /// </summary>
        public static DoodleArt V89A_Page()
        {
            var p = new DoodlePainter(-0.5f, -0.62f, 0.5f, 0.62f, 160) { Ink = 0.03f };
            p.Shape(Sd.Box(0f, 0f, 0.44f, 0.56f, 0.02f), Rgba.White);
            p.Shape(Sd.Polygon(0.24f, 0.56f, 0.44f, 0.56f, 0.44f, 0.36f), Rgba.Hex("E6E6EE"), 0.02f);
            p.Fill(Sd.Circle(0f, 0.13f, 0.34f), Gold.WithAlpha(0.22f));
            Sdf egg = V89A_Egg(0f, 0.13f, 0.21f, 0.27f);
            Body(p, egg, Gold, GoldShade, null, -0.05f, 0.06f);
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.08f, 0.22f, 0.04f, 0.09f), -0.08f, 0.22f, 20f), GoldLight.WithAlpha(0.9f));
            p.Fill(Sd.Circle(-0.04f, 0.33f, 0.022f), GoldLight);
            foreach (var s in new[] { (-0.3f, 0.36f, 0.07f), (0.3f, 0.25f, 0.06f), (0.27f, -0.06f, 0.045f), (-0.29f, 0.0f, 0.05f) })
                p.Shape(V89A_Star(s.Item1, s.Item2, s.Item3), Gold, 0.015f);
            p.Line(Rgba.Hex("D8D8E2"), 0.015f, -0.32f, -0.5f, 0.32f, -0.5f);
            return new DoodleArt("v89_a_page", p);
        }
    }
}
