using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.77 per il livello 10 della roadmap "Half Full" (il fumetto è "bubble_box", le scintille
    /// "sparkle_star", l'acqua nel bicchiere è "plain" tinto): la stanza, l'omino pessimista (corpo, testa triste e testa
    /// col sorriso esagerato), il tavolino, il bicchiere (fondo e vetro davanti, l'acqua sta in mezzo), la brocca, lo
    /// schizzo di quando trabocca, il cartellino 9-slice (le scritte sono TextMesh) col suo chiodo e lo spago. Prefisso v77_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV77B() => new DoodleArt[]
        {
            V77B_RoomBg(), V77B_ManBody(), V77B_ManHead(false), V77B_ManHead(true), V77B_Table(), V77B_GlassBack(),
            V77B_GlassFront(), V77B_Jug(), V77B_Splash(), V77B_Sign(), V77B_SignNail(),
        };

        static readonly Rgba V77BSkin = Rgba.Hex("F2C9A0"), V77BSkinShade = Rgba.Hex("D9A57A"), V77BCheek = Rgba.Hex("F29A9A");
        static readonly Rgba V77BSweater = Rgba.Hex("8C93A8"), V77BSweaterShade = Rgba.Hex("6C7288");
        static readonly Rgba V77BPants = Rgba.Hex("4F5470"), V77BPantsShade = Rgba.Hex("3B3F57"), V77BHair = Rgba.Hex("5A4636");
        static readonly Rgba V77BWood = Rgba.Hex("C89060"), V77BWoodShade = Rgba.Hex("A06E44"), V77BWoodLight = Rgba.Hex("E2B07E");
        static readonly Rgba V77BWater = Rgba.Hex("8FD3F2"), V77BGlass = Rgba.Hex("DDF3FB");

        /// <summary>
        /// La stanza, coordinate del mondo: parete grigio-azzurra con una carta da parati a righine, il battiscopa (y -3.2)
        /// e il pavimento di legno. Un po' spoglia e grigia: è la casa di un pessimista.
        /// </summary>
        public static DoodleArt V77B_RoomBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f, Wobble = 0.006f };
            p.FillFunc(Sd.Box(0f, 1.9f, 5f, 5.2f), (x, y) => Rgba.Lerp(Rgba.Hex("B9C2CF"), Rgba.Hex("D3D9E2"), Math.Max(0f, Math.Min(1f, (y + 3f) / 8f))));
            for (int i = -9; i <= 9; i++)
            {
                float x = i * 0.5f;
                p.Line(Rgba.Hex("A9B2C0", 0.55f), 0.035f, x, -3.15f, x, 7f);
            }
            // una macchia d'umido in un angolo (tristezza)
            p.Fill(Sd.Union(Sd.Ellipse(-2.15f, 2.5f, 0.35f, 0.25f), Sd.Ellipse(-1.95f, 2.35f, 0.22f, 0.18f)), Rgba.Hex("9AA3B2", 0.45f));
            // battiscopa e pavimento
            p.FillFunc(Sd.Box(0f, -5.2f, 5f, 2.05f), (x, y) => Rgba.Lerp(Rgba.Hex("B88A62"), Rgba.Hex("8E6646"), Math.Max(0f, Math.Min(1f, (-3.3f - y) / 3f))));
            for (int i = 0; i < 6; i++)
            {
                float y = -3.55f - i * 0.55f;
                p.Line(Rgba.Hex("6E4E36", 0.45f), 0.025f, -4.5f, y, 4.5f, y);
            }
            p.Shape(Sd.Box(0f, -3.2f, 5f, 0.1f), Rgba.Hex("EDEAE4"), 0.04f);
            return new DoodleArt("v77_b_room_bg", p);
        }

        /// <summary>
        /// Il corpo dell'omino in piedi (origine ai piedi): gambe, maglione con le spalle cadenti, braccia penzoloni. La
        /// testa è un disegno a parte (il collo arriva in y 1.72; la testa sta in y 2.05).
        /// </summary>
        public static DoodleArt V77B_ManBody()
        {
            var p = new DoodlePainter(-0.85f, -0.12f, 0.85f, 1.9f, 100) { Ink = 0.05f };
            p.Fill(Sd.Ellipse(0f, 0.0f, 0.6f, 0.09f), new Rgba(0f, 0f, 0f, 0.18f));
            // le gambe e le scarpe
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Box(s * 0.17f, 0.45f, 0.13f, 0.4f, 0.06f), V77BPants, V77BPantsShade);
                p.Shape(Sd.Ellipse(s * 0.2f + s * 0.05f, 0.06f, 0.19f, 0.09f), Rgba.Hex("3A2F2A"), 0.04f);
            }
            // il maglione (spalle cadenti)
            Sdf torso = Sd.Union(Sd.Box(0f, 1.2f, 0.4f, 0.42f, 0.18f), Sd.Polygon(-0.4f, 1.5f, -0.5f, 1.35f, -0.4f, 0.85f), Sd.Polygon(0.4f, 1.5f, 0.5f, 1.35f, 0.4f, 0.85f));
            Body(p, torso, V77BSweater, V77BSweaterShade);
            p.Line(V77BSweaterShade, 0.035f, -0.38f, 0.86f, 0.38f, 0.86f);
            // un piccolo rammendo sul maglione
            p.Shape(Sd.Box(0.18f, 1.05f, 0.08f, 0.07f, 0.02f), Rgba.Hex("A7AEC2"), 0.02f);
            p.Line(Rgba.Hex("2B2233", 0.6f), 0.012f, 0.12f, 1.0f, 0.24f, 1.1f);
            // le braccia penzoloni
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Capsule(s * 0.47f, 1.45f, s * 0.55f, 0.82f, 0.11f), V77BSweater, V77BSweaterShade);
                p.Shape(Sd.Circle(s * 0.56f, 0.72f, 0.1f), V77BSkin, 0.035f);
            }
            // il collo
            p.Shape(Sd.Box(0f, 1.68f, 0.09f, 0.08f, 0.03f), V77BSkin, 0.035f);
            return new DoodleArt("v77_b_man_body", p);
        }

        /// <summary>
        /// La testa (origine al centro): triste (occhi a mezz'asta, sopracciglia in giù, bocca all'ingiù, una lacrimuccia)
        /// oppure felice in modo esagerato (occhi chiusi a mezzaluna, guance rosse, sorrisone da orecchio a orecchio).
        /// </summary>
        public static DoodleArt V77B_ManHead(bool happy)
        {
            var p = new DoodlePainter(-0.6f, -0.55f, 0.6f, 0.6f, 120) { Ink = 0.045f };
            // orecchie
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Ellipse(s * 0.42f, 0f, 0.08f, 0.12f), V77BSkin, 0.035f);
            Sdf head = Sd.Ellipse(0f, 0f, 0.42f, 0.46f);
            Body(p, head, V77BSkin, V77BSkinShade);
            // capelli radi (tre ciuffi)
            p.Line(V77BHair, 0.05f, -0.2f, 0.4f, -0.12f, 0.52f);
            p.Line(V77BHair, 0.05f, 0f, 0.44f, 0.02f, 0.57f);
            p.Line(V77BHair, 0.05f, 0.18f, 0.41f, 0.13f, 0.53f);
            if (!happy)
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    float ex = s * 0.16f;
                    p.Shape(Sd.Ellipse(ex, 0.06f, 0.08f, 0.07f), Rgba.White, 0.025f);
                    p.Fill(Sd.Circle(ex, 0.03f, 0.035f), Rgba.Hex("2B2233"));
                    // palpebra a mezz'asta
                    p.Fill(Sd.Intersect(Sd.Ellipse(ex, 0.06f, 0.085f, 0.075f), Sd.Box(ex, 0.12f, 0.1f, 0.045f)), V77BSkinShade);
                    p.InkLine(0.025f, ex - 0.08f, 0.075f, ex + 0.08f, 0.075f);
                    // sopracciglio in giù verso l'esterno
                    p.InkLine(0.03f, ex - s * 0.07f, 0.22f, ex + s * 0.09f, 0.16f);
                }
                p.Line(V77BSkinShade, 0.025f, -0.03f, -0.02f, 0.0f, -0.09f, 0.04f, -0.08f);
                // bocca all'ingiù
                p.InkLine(0.035f, -0.15f, -0.27f, -0.08f, -0.21f, 0.0f, -0.19f, 0.08f, -0.21f, 0.15f, -0.27f);
                // lacrimuccia
                p.Shape(Sd.Union(Sd.Circle(0.25f, -0.12f, 0.035f), Sd.Polygon(0.218f, -0.11f, 0.282f, -0.11f, 0.25f, -0.04f)), V77BWater, 0.015f);
            }
            else
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    float ex = s * 0.16f;
                    // occhi chiusi dalla gioia (archetti in su)
                    p.InkLine(0.035f, ex - 0.08f, 0.04f, ex - 0.04f, 0.1f, ex, 0.12f, ex + 0.04f, 0.1f, ex + 0.08f, 0.04f);
                    // sopracciglia alte
                    p.InkLine(0.03f, ex - 0.08f, 0.24f, ex, 0.28f, ex + 0.08f, 0.25f);
                    p.Fill(Sd.Ellipse(s * 0.28f, -0.1f, 0.09f, 0.06f), V77BCheek.WithAlpha(0.8f));
                }
                // il sorrisone esagerato, da orecchio a orecchio
                Sdf mouth = Sd.Intersect(Sd.Ellipse(0f, -0.08f, 0.33f, 0.3f), Sd.Box(0f, -0.25f, 0.4f, 0.17f));
                p.Shape(mouth, Rgba.Hex("7A2433"), 0.035f);
                p.Fill(Sd.Intersect(mouth, Sd.Box(0f, -0.13f, 0.4f, 0.05f)), Rgba.White);
                p.Fill(Sd.Intersect(mouth, Sd.Ellipse(0f, -0.38f, 0.16f, 0.08f)), Rgba.Hex("E96F86"));
                p.InkLine(0.03f, -0.33f, -0.08f, 0.33f, -0.08f);
            }
            return new DoodleArt(happy ? "v77_b_man_head_happy" : "v77_b_man_head_sad", p);
        }

        /// <summary>Il tavolino (origine al centro del piano, che è in y 0): piano di legno e due gambe fino a y -1.35.</summary>
        public static DoodleArt V77B_Table()
        {
            var p = new DoodlePainter(-1.15f, -1.45f, 1.15f, 0.2f, 100) { Ink = 0.05f };
            p.Fill(Sd.Ellipse(0f, -1.36f, 1.0f, 0.08f), new Rgba(0f, 0f, 0f, 0.15f));
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Box(s * 0.82f, -0.7f, 0.07f, 0.65f, 0.03f), V77BWood, V77BWoodShade);
            Body(p, Sd.Box(0f, -0.08f, 1.05f, 0.1f, 0.04f), V77BWood, V77BWoodShade);
            p.Line(V77BWoodLight, 0.025f, -0.95f, -0.03f, 0.9f, -0.03f);
            return new DoodleArt("v77_b_table", p);
        }

        // il bicchiere: vano interno da x -0.25 a 0.25, da y 0.06 a 0.9 (l'acqua, "plain" tinto, si mette lì dentro)

        /// <summary>Il fondo del bicchiere (dietro all'acqua): un velo azzurrino. Origine alla base del bicchiere.</summary>
        public static DoodleArt V77B_GlassBack()
        {
            var p = new DoodlePainter(-0.4f, -0.05f, 0.4f, 1.05f, 120) { Ink = 0.04f, Wobble = 0.004f };
            p.Fill(Sd.Box(0f, 0.48f, 0.27f, 0.44f, 0.04f), V77BGlass.WithAlpha(0.45f));
            return new DoodleArt("v77_b_glass_back", p);
        }

        /// <summary>Il vetro davanti (sopra all'acqua): contorno, fondo spesso, riflessi. Origine alla base.</summary>
        public static DoodleArt V77B_GlassFront()
        {
            var p = new DoodlePainter(-0.4f, -0.05f, 0.4f, 1.05f, 120) { Ink = 0.04f, Wobble = 0.004f };
            Sdf outer = Sd.Polygon(-0.3f, 0.95f, 0.3f, 0.95f, 0.27f, 0f, -0.27f, 0f);
            Sdf inner = Sd.Polygon(-0.25f, 0.97f, 0.25f, 0.97f, 0.23f, 0.06f, -0.23f, 0.06f);
            p.Fill(Sd.Subtract(outer, inner), V77BGlass.WithAlpha(0.85f));
            p.Shape(outer, Rgba.Clear);
            p.Line(Rgba.White.WithAlpha(0.85f), 0.035f, -0.17f, 0.82f, -0.15f, 0.25f);
            p.Line(Rgba.White.WithAlpha(0.7f), 0.025f, 0.17f, 0.8f, 0.16f, 0.62f);
            p.Line(Rgba.Hex("2B2233", 0.5f), 0.02f, -0.24f, 0.07f, 0.24f, 0.07f);
            return new DoodleArt("v77_b_glass_front", p);
        }

        /// <summary>La brocca piena d'acqua (origine alla base), col beccuccio a sinistra e il manico a destra.</summary>
        public static DoodleArt V77B_Jug()
        {
            var p = new DoodlePainter(-0.6f, -0.05f, 0.6f, 1.25f, 110) { Ink = 0.045f };
            Sdf handle = Sd.Subtract(Sd.Ellipse(0.38f, 0.65f, 0.2f, 0.28f), Sd.Ellipse(0.38f, 0.65f, 0.1f, 0.18f));
            p.Shape(handle, Rgba.Hex("E8E2D6"), 0.04f);
            Sdf body = Sd.Union(Sd.Polygon(-0.32f, 1.1f, 0.3f, 1.1f, 0.36f, 0.05f, -0.36f, 0.05f), Sd.Polygon(-0.32f, 1.1f, -0.5f, 1.16f, -0.3f, 0.92f));
            p.Shape(body, Rgba.Hex("E8E2D6", 0.95f), 0.045f);
            p.Fill(Sd.Intersect(body, Sd.Box(0f, 0.45f, 0.4f, 0.37f)), V77BWater.WithAlpha(0.85f));
            p.Line(Rgba.Hex("5FA8CC"), 0.025f, -0.33f, 0.82f, 0.32f, 0.82f);
            p.Shape(body, Rgba.Clear);
            p.Line(Rgba.White.WithAlpha(0.8f), 0.03f, -0.2f, 1.0f, -0.24f, 0.2f);
            return new DoodleArt("v77_b_jug", p);
        }

        /// <summary>Lo schizzo d'acqua che trabocca dall'orlo (origine sull'orlo del bicchiere) con gocce che cadono di lato.</summary>
        public static DoodleArt V77B_Splash()
        {
            var p = new DoodlePainter(-0.6f, -0.9f, 0.6f, 0.35f, 110) { Ink = 0.035f };
            p.Shape(Sd.Union(Sd.Ellipse(0f, 0.05f, 0.32f, 0.1f), Sd.Circle(-0.15f, 0.13f, 0.07f), Sd.Circle(0.12f, 0.15f, 0.06f)), V77BWater, 0.03f);
            foreach (var (x, y, r) in new[] { (-0.38f, -0.15f, 0.05f), (-0.42f, -0.5f, 0.04f), (0.38f, -0.25f, 0.05f), (0.44f, -0.62f, 0.035f), (-0.36f, -0.8f, 0.03f) })
                p.Shape(Sd.Union(Sd.Circle(x, y, r), Sd.Polygon(x - r * 0.9f, y + r * 0.3f, x + r * 0.9f, y + r * 0.3f, x, y + r * 2.4f)), V77BWater, 0.02f);
            return new DoodleArt("v77_b_splash", p);
        }

        /// <summary>
        /// Il cartellino di legno chiaro (9-slice: il livello lo allarga sulla misura delle parole "STAY FULL"), con un
        /// bordino. Bordi 30 px per lato.
        /// </summary>
        public static DoodleArt V77B_Sign()
        {
            var p = new DoodlePainter(-0.5f, -0.4f, 0.5f, 0.4f, 100) { Ink = 0.045f, Wobble = 0f };
            p.Shape(Sd.Box(0f, 0f, 0.45f, 0.35f, 0.08f), Rgba.Hex("F3E3C4"), 0.045f);
            p.Shape(Sd.Box(0f, 0f, 0.36f, 0.26f, 0.05f), Rgba.Hex("F7ECD6"), 0.015f);
            return new DoodleArt("v77_b_sign", p, 30);
        }

        /// <summary>Il chiodo con lo spago a triangolo che tiene il cartellino (origine sul chiodo).</summary>
        public static DoodleArt V77B_SignNail()
        {
            var p = new DoodlePainter(-0.75f, -0.5f, 0.75f, 0.12f, 100) { Ink = 0.035f };
            p.Line(Rgba.Hex("7A5A3E"), 0.025f, -0.65f, -0.45f, 0f, 0f, 0.65f, -0.45f);
            p.Shape(Sd.Circle(0f, 0f, 0.06f), Rgba.Hex("9A9AA6"), 0.025f);
            return new DoodleArt("v77_b_sign_nail", p);
        }
    }
}
