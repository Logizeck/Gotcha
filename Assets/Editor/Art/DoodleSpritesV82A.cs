using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.82 per il livello "The Polite Bomb" (roadmap n. 42): lo sfondo (officina: parete a
    /// pannello forato con attrezzi appesi, banco di legno su cui sta la bomba, pavimento), la bomba-cartone (corpo blu
    /// notte con riflesso, ghiera e miccia, papillon da bomba educata, il retro del vano batteria sul fianco destro), il
    /// bordo anteriore del vano (copre la parte della batteria che sta dentro), la batteria, il display (cornice e
    /// schermo spento) e la sua luce verde (si spegne alla fine), cinque facce (educata, servizievole, preoccupata,
    /// imbarazzata, addormentata), tre fili (rosso, blu, giallo: origine nel punto dove escono dal display), il pulsante
    /// STOP (basamento a strisce di pericolo + calotta rossa che si abbassa; la scritta è un testo) e la scintilla della
    /// miccia. Riusati: "puff" (lo sbuffo a 00:00), "zzz", "s41e_trophy" (la coppa della vittoria), "bubble_box".
    /// Coordinate dei disegni della bomba = coordinate locali della bomba (centro del corpo in 0,0). Prefisso v82_a_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV82A() => new DoodleArt[]
        {
            V82A_Bg(), V82A_Bomb(), V82A_Slot(), V82A_Battery(), V82A_Display(), V82A_DisplayGlow(),
            V82A_Face(0), V82A_Face(1), V82A_Face(2), V82A_Face(3), V82A_Face(4),
            V82A_Wire(0), V82A_Wire(1), V82A_Wire(2), V82A_StopBase(), V82A_StopCap(), V82A_Spark(),
        };

        static readonly Rgba V82ANavy = Rgba.Hex("2F3756"), V82ANavyShade = Rgba.Hex("1D2239");
        static readonly Rgba V82AMetal = Rgba.Hex("B9C2CF"), V82AMetalShade = Rgba.Hex("8D97A8");

        /// <summary>Raggio del corpo della bomba (coordinate locali).</summary>
        public const float V82ABombR = 1.25f;
        /// <summary>Centro del display (locale della bomba).</summary>
        public const float V82ADisplayX = 0f, V82ADisplayY = -0.32f;
        /// <summary>Punto da cui escono i fili (locale della bomba): origine dei disegni dei fili.</summary>
        public const float V82AWireX = -0.7f, V82AWireY = -0.34f;
        /// <summary>Centro della batteria nel vano (locale della bomba).</summary>
        public const float V82ABatteryX = 1.42f, V82ABatteryY = -0.55f;
        /// <summary>Punta della miccia (locale della bomba): lì brilla la scintilla.</summary>
        public const float V82AFuseX = 0.86f, V82AFuseY = 1.86f;
        /// <summary>Altezza della scritta STOP sulla calotta (locale del pulsante).</summary>
        public const float V82AStopLabelY = 0.17f;

        /// <summary>
        /// I tre fili (coordinate dalla loro origine, il punto d'uscita dal display): penzolano a sinistra oltre il
        /// fianco della bomba. 0 rosso (il più esterno), 1 blu, 2 giallo (resta sul corpo).
        /// </summary>
        public static readonly float[][] V82AWirePaths =
        {
            new[] { 0f, 0f, -0.35f, 0.13f, -0.75f, 0.1f, -1.05f, -0.12f, -1.16f, -0.52f, -1.06f, -0.9f },
            new[] { 0f, -0.06f, -0.3f, -0.24f, -0.62f, -0.28f, -0.84f, -0.52f, -0.9f, -0.92f, -0.8f, -1.14f },
            new[] { 0f, -0.12f, -0.2f, -0.42f, -0.44f, -0.6f, -0.54f, -0.92f, -0.42f, -1.22f },
        };

        /// <summary>
        /// Lo sfondo (coordinate del mondo): parete color crema con un pannello forato dietro la bomba e due attrezzi
        /// appesi ai lati, il piano del banco a y −1.15 (la bomba ci appoggia), il fronte del banco fino a y −3.7 (lì è
        /// montato il pulsante STOP), il pavimento sotto.
        /// </summary>
        public static DoodleArt V82A_Bg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 40) { Ink = 0.05f, Wobble = 0.006f };
            p.FillFunc(Sd.Box(0f, 0f, 4.6f, 7.1f), (x, y) =>
                Rgba.Lerp(Rgba.Hex("F2E6D0"), Rgba.Hex("E3D3B6"), Math.Max(0f, Math.Min(1f, (4f - y) / 7f))));
            // righe verticali della carta da parati
            for (float x = -4.4f; x <= 4.4f; x += 0.5f) p.Fill(Sd.Box(x, 2f, 0.05f, 5.2f), Rgba.Hex("D9C6A4", 0.35f), false);
            // pannello forato dietro la bomba
            Sdf board = Sd.Box(0f, 0.75f, 2.65f, 1.95f, 0.08f);
            p.Shape(board, Rgba.Hex("C9A57A"));
            for (float x = -2.4f; x <= 2.41f; x += 0.3f)
                for (float y = -1.0f; y <= 2.55f; y += 0.3f)
                    p.Fill(Sd.Circle(x, y, 0.035f), Rgba.Hex("7A5A3A", 0.75f), false);
            // attrezzi appesi ai lati (sagome)
            p.Shape(Sd.Union(Sd.Capsule(-2.35f, 2.2f, -2.35f, 1.1f, 0.07f), Sd.Circle(-2.35f, 2.3f, 0.18f)), Rgba.Hex("9AA4B5"));
            p.Fill(Sd.Circle(-2.35f, 2.33f, 0.08f), Rgba.Hex("C9A57A"));
            p.Shape(Sd.Capsule(2.3f, 2.25f, 2.3f, 1.75f, 0.1f), Rgba.Hex("E5534B"));
            p.Shape(Sd.Capsule(2.3f, 1.7f, 2.3f, 1.05f, 0.03f), Rgba.Hex("C9D0DA"));
            // piano del banco e fronte
            p.Shape(Sd.Box(0f, -1.3f, 4.6f, 0.16f), Rgba.Hex("D69A62"));
            p.Fill(Sd.Box(0f, -1.19f, 4.6f, 0.035f), Rgba.Hex("F0BE88", 0.8f));
            p.Shape(Sd.Box(0f, -2.58f, 4.6f, 1.12f), Rgba.Hex("A96D40"));
            for (float y = -1.75f; y >= -3.5f; y -= 0.55f) p.Line(Rgba.Hex("7E4E2B", 0.5f), 0.03f, -4.5f, y, 4.5f, y + 0.02f);
            p.Fill(Sd.Box(0f, -1.55f, 4.6f, 0.08f), Rgba.Hex("6A3F22", 0.35f), false);
            // pavimento
            p.FillFunc(Sd.Box(0f, -5.4f, 4.6f, 1.7f), (x, y) => Rgba.Lerp(Rgba.Hex("6A7286"), Rgba.Hex("4E556A"), Math.Max(0f, Math.Min(1f, (-3.7f - y) / 2.5f))));
            p.Fill(Sd.Box(0f, -3.74f, 4.6f, 0.04f), Rgba.Hex("2B2233", 0.6f), false);
            return new DoodleArt("v82_a_bg", p);
        }

        /// <summary>
        /// Il corpo della bomba (centro 0,0, raggio <see cref="V82ABombR"/>): sfera blu notte con riflesso, ghiera
        /// argentata in alto a destra, miccia di corda che finisce in (<see cref="V82AFuseX"/>, <see cref="V82AFuseY"/>),
        /// papillon rosso sotto il display, e sul fianco destro il fondo scuro del vano batteria.
        /// </summary>
        public static DoodleArt V82A_Bomb()
        {
            var p = new DoodlePainter(-1.42f, -1.42f, 1.62f, 2.0f, 110) { Ink = 0.05f, Wobble = 0.008f };
            // ombra sul banco
            p.Fill(Sd.Ellipse(0.05f, -1.22f, 1.05f, 0.1f), Rgba.Hex("2B2233", 0.25f), false);
            // fondo del vano batteria (dietro al bordo, che è un disegno a parte)
            p.Shape(Sd.Box(1.14f, -0.55f, 0.34f, 0.25f, 0.05f), Rgba.Hex("2A2E38"));
            // miccia
            p.Line(Rgba.Hex("2B2233"), 0.11f, 0.42f, 1.3f, 0.5f, 1.52f, 0.66f, 1.64f, 0.8f, 1.74f, V82AFuseX, V82AFuseY);
            p.Line(Rgba.Hex("D9B27A"), 0.06f, 0.42f, 1.3f, 0.5f, 1.52f, 0.66f, 1.64f, 0.8f, 1.74f, V82AFuseX, V82AFuseY);
            for (int i = 0; i < 4; i++)
            {
                float t = 0.2f + i * 0.2f;
                float x = 0.46f + t * 0.4f, y = 1.4f + t * 0.42f;
                p.InkLine(0.018f, x - 0.03f, y - 0.03f, x + 0.03f, y + 0.02f);
            }
            // corpo
            Sdf body = Sd.Circle(0f, 0f, V82ABombR);
            Body(p, body, V82ANavy, V82ANavyShade, null, -0.12f, 0.12f);
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.55f, 0.68f, 0.32f, 0.14f), -0.55f, 0.68f, 35f), Rgba.White.WithAlpha(0.32f));
            p.Fill(Sd.Circle(-0.85f, 0.32f, 0.06f), Rgba.White.WithAlpha(0.3f));
            // ghiera della miccia
            Sdf cap = Sd.Rotate(Sd.Box(0.36f, 1.17f, 0.24f, 0.15f, 0.04f), 0.36f, 1.17f, -28f);
            Body(p, cap, V82AMetal, V82AMetalShade);
            p.Fill(Sd.Rotate(Sd.Box(0.33f, 1.24f, 0.17f, 0.025f), 0.33f, 1.24f, -28f), Rgba.White.WithAlpha(0.55f));
            // papillon (bomba educata)
            Rgba bow = Rgba.Hex("E2434B");
            p.Shape(Sd.Polygon(-0.04f, -0.86f, -0.34f, -0.72f, -0.34f, -1.04f), bow, 0.035f);
            p.Shape(Sd.Polygon(0.04f, -0.86f, 0.34f, -0.72f, 0.34f, -1.04f), bow, 0.035f);
            p.Shape(Sd.Box(0f, -0.87f, 0.07f, 0.07f, 0.03f), Rgba.Hex("B8303A"), 0.035f);
            return new DoodleArt("v82_a_bomb", p);
        }

        /// <summary>
        /// Il vano della batteria visto davanti (coordinate locali della bomba): guscio metallico con rivetti e la bocca
        /// scura a destra. Sta SOPRA la batteria e ne copre la parte infilata dentro.
        /// </summary>
        public static DoodleArt V82A_Slot()
        {
            var p = new DoodlePainter(0.7f, -0.92f, 1.5f, -0.18f, 140) { Ink = 0.04f, Wobble = 0.005f };
            Sdf shell = Sd.Box(1.09f, -0.55f, 0.3f, 0.29f, 0.05f);
            Body(p, shell, V82AMetal, V82AMetalShade, null, -0.05f, 0.06f);
            p.Shape(Sd.Ellipse(1.39f, -0.55f, 0.06f, 0.26f), Rgba.Hex("1C1F27"), 0.035f);
            foreach (float y in new[] { -0.33f, -0.77f })
                foreach (float x in new[] { 0.9f, 1.2f })
                    p.Shape(Sd.Circle(x, y, 0.028f), Rgba.Hex("D8DEE6"), 0.02f);
            p.Fill(Sd.Box(1.05f, -0.3f, 0.22f, 0.02f), Rgba.White.WithAlpha(0.5f));
            return new DoodleArt("v82_a_slot", p);
        }

        /// <summary>La batteria (centro 0,0, lunga 1, sdraiata): corpo nero, parte rame con fulmine, polo + a destra.</summary>
        public static DoodleArt V82A_Battery()
        {
            var p = new DoodlePainter(-0.6f, -0.28f, 0.66f, 0.28f, 150) { Ink = 0.04f, Wobble = 0.004f };
            p.Shape(Sd.Box(0.555f, 0f, 0.05f, 0.085f, 0.02f), V82AMetal, 0.03f);
            Sdf body = Sd.Box(0f, 0f, 0.5f, 0.2f, 0.06f);
            p.Fill(body, Rgba.Hex("2E2E36"));
            p.Fill(Sd.Intersect(body, Sd.Box(0.3f, 0f, 0.22f, 0.3f)), Rgba.Hex("E39A3C"));
            p.Fill(Sd.Intersect(body, Sd.Box(0f, -0.13f, 0.6f, 0.08f)), Rgba.Hex("000000", 0.18f));
            p.Shape(body, Rgba.Clear);
            p.Fill(Sd.Box(0f, 0.12f, 0.44f, 0.025f), Rgba.White.WithAlpha(0.35f));
            // fulmine giallo sulla parte rame
            p.Shape(Sd.Polygon(0.3f, 0.15f, 0.2f, -0.01f, 0.28f, -0.01f, 0.22f, -0.16f, 0.38f, 0.03f, 0.3f, 0.03f, 0.36f, 0.15f), Rgba.Hex("FFE45C"), 0.022f);
            // segno + (bianco sul nero)
            p.Line(Rgba.White, 0.04f, -0.32f, 0f, -0.16f, 0f);
            p.Line(Rgba.White, 0.04f, -0.24f, -0.08f, -0.24f, 0.08f);
            return new DoodleArt("v82_a_battery", p);
        }

        /// <summary>Il display (centro 0,0 = (<see cref="V82ADisplayX"/>, <see cref="V82ADisplayY"/>) della bomba): cornice e schermo spento.</summary>
        public static DoodleArt V82A_Display()
        {
            var p = new DoodlePainter(-0.88f, -0.42f, 0.88f, 0.42f, 140) { Ink = 0.045f, Wobble = 0.004f };
            Body(p, Sd.Box(0f, 0f, 0.78f, 0.33f, 0.08f), Rgba.Hex("5A6276"), Rgba.Hex("434A5C"));
            p.Shape(Sd.Box(0f, 0f, 0.65f, 0.22f, 0.04f), Rgba.Hex("14201A"), 0.035f);
            foreach (float s in new[] { -1f, 1f })
                foreach (float t in new[] { -1f, 1f })
                    p.Shape(Sd.Circle(s * 0.72f, t * 0.26f, 0.025f), Rgba.Hex("C9D0DA"), 0.015f);
            return new DoodleArt("v82_a_display", p);
        }

        /// <summary>La luce verde dello schermo acceso (senza contorno, con righe orizzontali): si spegne con l'alfa.</summary>
        public static DoodleArt V82A_DisplayGlow()
        {
            var p = new DoodlePainter(-0.66f, -0.23f, 0.66f, 0.23f, 140) { Wobble = 0f };
            Sdf screen = Sd.Box(0f, 0f, 0.62f, 0.2f, 0.035f);
            p.FillFunc(screen, (x, y) => Rgba.Lerp(Rgba.Hex("2E7D44"), Rgba.Hex("1E5A31"), Math.Abs(y) / 0.2f));
            for (float y = -0.18f; y <= 0.18f; y += 0.05f) p.Fill(Sd.Intersect(screen, Sd.Box(0f, y, 0.7f, 0.006f)), Rgba.Hex("0E2A17", 0.35f), false);
            p.Fill(Sd.Box(-0.2f, 0.12f, 0.35f, 0.03f), Rgba.White.WithAlpha(0.12f), false);
            return new DoodleArt("v82_a_display_glow", p);
        }

        /// <summary>
        /// Le facce (coordinate locali della bomba, occhi in ±0.4, 0.58, bocca intorno a 0.25):
        /// 0 educata, 1 servizievole (occhi felici, sorrisone), 2 preoccupata, 3 imbarazzata (rossore, sguardo basso,
        /// goccia), 4 addormentata (occhi chiusi, boccuccia).
        /// </summary>
        public static DoodleArt V82A_Face(int mood)
        {
            string[] names = { "polite", "eager", "worried", "embarrassed", "sleepy" };
            var p = new DoodlePainter(-0.85f, -0.05f, 0.95f, 1.08f, 140) { Ink = 0.04f, Wobble = 0.006f };
            p.InkColor = Rgba.Hex("0E1020");
            const float ey = 0.58f;
            Rgba blush = Rgba.Hex("FF6F91", mood == 3 ? 0.75f : 0.4f);
            if (mood != 4 && mood != 2) foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Ellipse(s * 0.66f, 0.3f, 0.15f, 0.08f), blush);
            if (mood == 3)
                foreach (float s in new[] { -1f, 1f })
                    for (int i = -1; i <= 1; i++) p.Line(Rgba.Hex("C9305A", 0.8f), 0.02f, s * 0.66f + i * 0.07f - 0.03f, 0.25f, s * 0.66f + i * 0.07f + 0.03f, 0.35f);
            foreach (float s in new[] { -1f, 1f })
            {
                float ex = s * 0.4f;
                switch (mood)
                {
                    case 1:   // occhi felici ad arco
                        p.Line(Rgba.White, 0.07f, ex - 0.15f, ey - 0.04f, ex - 0.07f, ey + 0.07f, ex + 0.07f, ey + 0.07f, ex + 0.15f, ey - 0.04f);
                        break;
                    case 4:   // occhi chiusi assonnati (archi verso il basso) e palpebre pesanti
                        p.Line(Rgba.White, 0.06f, ex - 0.15f, ey + 0.02f, ex - 0.07f, ey - 0.06f, ex + 0.07f, ey - 0.06f, ex + 0.15f, ey + 0.02f);
                        break;
                    default:
                    {
                        float rx = mood == 2 ? 0.19f : 0.16f, ry = mood == 2 ? 0.23f : 0.19f;
                        p.Shape(Sd.Ellipse(ex, ey, rx, ry), Rgba.White, 0.035f);
                        float px = ex + (mood == 3 ? -0.06f : 0.02f), py = ey + (mood == 3 ? -0.08f : 0f);
                        float pr = mood == 2 ? 0.05f : 0.075f;
                        p.Fill(Sd.Circle(px, py, pr), Rgba.Hex("1A1626"));
                        p.Fill(Sd.Circle(px - 0.025f, py + 0.03f, 0.025f), Rgba.White);
                        break;
                    }
                }
                // sopracciglia
                if (mood == 2) p.Line(Rgba.White, 0.045f, ex - s * 0.16f, ey + 0.3f, ex + s * 0.13f, ey + 0.38f);
                else if (mood == 3) p.Line(Rgba.White, 0.04f, ex - 0.13f, ey + 0.3f, ex + 0.13f, ey + 0.27f + s * 0.03f);
                else if (mood == 0) p.Line(Rgba.White, 0.04f, ex - 0.12f, ey + 0.27f, ex, ey + 0.3f, ex + 0.12f, ey + 0.27f);
            }
            switch (mood)
            {
                case 0:   // sorriso educato
                    p.Line(Rgba.White, 0.05f, -0.2f, 0.27f, -0.08f, 0.19f, 0.08f, 0.19f, 0.2f, 0.27f);
                    break;
                case 1:   // sorrisone aperto
                {
                    Sdf mouth = Sd.Intersect(Sd.Ellipse(0f, 0.28f, 0.3f, 0.22f), Sd.Box(0f, 0.16f, 0.36f, 0.13f));
                    p.Shape(mouth, Rgba.Hex("7A2440"), 0.04f);
                    p.Fill(Sd.Ellipse(0f, 0.1f, 0.13f, 0.05f), Rgba.Hex("F06A86"));
                    p.Fill(Sd.Box(0f, 0.25f, 0.24f, 0.025f), Rgba.White);
                    break;
                }
                case 2:   // boccuccia tremante e goccia di sudore
                    p.Shape(Sd.Ellipse(0f, 0.2f, 0.08f, 0.1f), Rgba.Hex("7A2440"), 0.035f);
                    p.Shape(Sd.Union(Sd.Circle(0.74f, 0.78f, 0.07f), Sd.Polygon(0.68f, 0.81f, 0.74f, 0.95f, 0.8f, 0.81f)), Rgba.Hex("8FD3FF"), 0.025f);
                    break;
                case 3:   // bocca ondulata imbarazzata e goccia
                    p.Line(Rgba.White, 0.045f, -0.18f, 0.2f, -0.09f, 0.25f, 0f, 0.2f, 0.09f, 0.25f, 0.18f, 0.2f);
                    p.Shape(Sd.Union(Sd.Circle(-0.74f, 0.8f, 0.07f), Sd.Polygon(-0.8f, 0.83f, -0.74f, 0.97f, -0.68f, 0.83f)), Rgba.Hex("8FD3FF"), 0.025f);
                    break;
                default:  // addormentata: boccuccia aperta
                    p.Shape(Sd.Ellipse(0.03f, 0.22f, 0.07f, 0.06f), Rgba.Hex("7A2440"), 0.035f);
                    break;
            }
            return new DoodleArt("v82_a_face_" + names[Math.Max(0, Math.Min(4, mood))], p);
        }

        /// <summary>Un filo (origine nel punto d'uscita dal display): tubetto colorato con contorno e puntale di rame sfilacciato.</summary>
        public static DoodleArt V82A_Wire(int i)
        {
            string[] names = { "red", "blue", "yellow" };
            Rgba[] cols = { Rgba.Hex("E5434B"), Rgba.Hex("3D8BEB"), Rgba.Hex("F5C93A") };
            var p = new DoodlePainter(-1.32f, -1.38f, 0.12f, 0.28f, 140) { Ink = 0.04f, Wobble = 0.004f };
            float[] xy = V82AWirePaths[i];
            p.Line(Rgba.Hex("0E1020"), 0.12f, xy);
            p.Line(cols[i], 0.07f, xy);
            int n = xy.Length;
            float ex = xy[n - 2], ey = xy[n - 1];
            // puntale di rame con i fili sfilacciati
            p.Line(Rgba.Hex("D88A3A"), 0.025f, ex, ey, ex - 0.06f, ey - 0.12f);
            p.Line(Rgba.Hex("D88A3A"), 0.025f, ex, ey, ex + 0.01f, ey - 0.14f);
            p.Line(Rgba.Hex("D88A3A"), 0.025f, ex, ey, ex + 0.07f, ey - 0.11f);
            p.Shape(Sd.Circle(ex, ey, 0.05f), cols[i], 0.03f);
            p.Fill(Sd.Circle(0f, xy[1], 0.06f), Rgba.Hex("0E1020"));
            return new DoodleArt("v82_a_wire_" + names[i], p);
        }

        /// <summary>Il basamento del pulsante STOP (centro 0,0): scatola con strisce di pericolo gialle e nere, colletto.</summary>
        public static DoodleArt V82A_StopBase()
        {
            var p = new DoodlePainter(-0.98f, -0.68f, 0.98f, 0.22f, 120) { Ink = 0.05f, Wobble = 0.006f };
            Sdf box = Sd.Box(0f, -0.3f, 0.88f, 0.32f, 0.08f);
            p.Fill(box, Rgba.Hex("F5C93A"));
            for (float x = -1.2f; x <= 1.2f; x += 0.3f)
                p.Fill(Sd.Intersect(box, Sd.Rotate(Sd.Box(x, -0.3f, 0.075f, 0.6f), x, -0.3f, -35f)), Rgba.Hex("2B2233"));
            p.Shape(box, Rgba.Clear);
            p.Shape(Sd.Ellipse(0f, 0.02f, 0.76f, 0.16f), Rgba.Hex("4A5164"));
            p.Fill(Sd.Ellipse(0f, 0.04f, 0.66f, 0.1f), Rgba.Hex("2A2E38"));
            return new DoodleArt("v82_a_stop_base", p);
        }

        /// <summary>La calotta rossa del pulsante STOP (centro della base in 0,0): cupola lucida; la scritta è un testo a y <see cref="V82AStopLabelY"/>.</summary>
        public static DoodleArt V82A_StopCap()
        {
            var p = new DoodlePainter(-0.76f, -0.14f, 0.76f, 0.56f, 130) { Ink = 0.05f, Wobble = 0.006f };
            Sdf dome = Sd.Union(Sd.Intersect(Sd.Ellipse(0f, 0.04f, 0.64f, 0.46f), Sd.Box(0f, 0.3f, 0.7f, 0.27f)), Sd.Box(0f, 0.0f, 0.64f, 0.07f, 0.04f));
            Body(p, dome, Rgba.Hex("E5343E"), Rgba.Hex("B0222C"), null, -0.08f, 0.08f);
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.3f, 0.36f, 0.18f, 0.06f), -0.3f, 0.36f, 18f), Rgba.White.WithAlpha(0.55f));
            return new DoodleArt("v82_a_stop_cap", p);
        }

        /// <summary>La scintilla della miccia (centro 0,0): stella arancio con cuore giallo.</summary>
        public static DoodleArt V82A_Spark()
        {
            var p = new DoodlePainter(-0.3f, -0.3f, 0.3f, 0.3f, 140) { Ink = 0.03f, Wobble = 0.004f };
            var pts = new List<float>();
            for (int k = 0; k < 16; k++)
            {
                double a = k * Math.PI / 8.0;
                float r = k % 2 == 0 ? 0.26f : 0.11f;
                pts.Add((float)Math.Cos(a) * r);
                pts.Add((float)Math.Sin(a) * r);
            }
            p.Shape(Sd.Polygon(pts.ToArray()), Rgba.Hex("FF8A2B"), 0.025f);
            p.Fill(Sd.Circle(0f, 0f, 0.09f), Rgba.Hex("FFE45C"));
            p.Fill(Sd.Circle(-0.02f, 0.02f, 0.04f), Rgba.White);
            return new DoodleArt("v82_a_spark", p);
        }
    }
}
