using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.84 per il livello "The Short Ladder" (roadmap n. 72): lo sfondo (facciata di mattoni con il
    /// pluviale, il bordo del tetto e il cielo in alto, lo zoccolo di pietra e il marciapiede col vasetto), la finestra in
    /// tre pezzi (la stanza illuminata dietro, la cornice col davanzale e la fioriera, il vetro con la croce che sparisce
    /// quando l'omino entra), la scala a pezzi (un tratto con un piolo che si ripete, i piedini di gomma e i tappi in
    /// cima: così i pioli restano sempre equidistanti) e l'omino (in piedi di fronte, di spalle mentre sale, e il busto
    /// felice col braccio a parte per salutare dalla finestra). L'ombra è "ball_shadow", la scintilla "sparkle_star".
    /// Nessuna scritta disegnata. Prefisso v84_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV84B() => new DoodleArt[]
        {
            V84B_Bg(), V84B_WindowBack(), V84B_WindowFrame(), V84B_WindowGlass(),
            V84B_LadderSeg(), V84B_LadderFoot(), V84B_LadderTop(),
            V84B_Man(), V84B_ManClimb(), V84B_ManBust(), V84B_ManArm(),
        };

        // misure condivise col builder (le stesse della logica ShortLadderLogic)
        /// <summary>Quota del pavimento (piedi della scala e dell'omino).</summary>
        public const float V84BFloorY = -3.6f;
        /// <summary>Centro della finestra e mezze misure dell'apertura (il davanzale è in basso, y = centro - mezza altezza).</summary>
        public const float V84BWindowX = 0.9f, V84BWindowY = 2.15f, V84BWindowHx = 0.55f, V84BWindowHy = 0.6f;
        /// <summary>Altezza di un tratto della scala e mezza distanza fra i montanti.</summary>
        public const float V84BSegH = 0.5f, V84BRailX = 0.32f;

        static readonly Rgba V84BBrick = Rgba.Hex("C9674B"), V84BBrickDark = Rgba.Hex("B0573E"), V84BMortar = Rgba.Hex("E8D3BE");
        static readonly Rgba V84BRail = Rgba.Hex("E2A64A"), V84BRailShade = Rgba.Hex("B87E2C"), V84BRung = Rgba.Hex("D69238");
        static readonly Rgba V84BSkin = Rgba.Hex("F2C49B"), V84BSkinShade = Rgba.Hex("D9A57B");
        static readonly Rgba V84BOveralls = Rgba.Hex("3F7FD6"), V84BOverallsShade = Rgba.Hex("2E62AD");
        static readonly Rgba V84BShirt = Rgba.Hex("F2D24B"), V84BHat = Rgba.Hex("E8433F"), V84BHatShade = Rgba.Hex("B9302E");
        static readonly Rgba V84BShoe = Rgba.Hex("3A2B2B"), V84BHair = Rgba.Hex("6B4129");

        static float V84B_Frac(float v) => v - (float)Math.Floor(v);

        /// <summary>
        /// Lo sfondo (coordinate del mondo): cielo azzurro sopra y 4.35 col bordo del tetto, facciata di mattoni sfalsati
        /// (righe da 0.25, mattoni da 0.5), pluviale grigio a sinistra (x -2.6), zoccolo di pietra sopra il pavimento
        /// (y -3.6), marciapiede a lastre con il bordo d'erba e un vasetto di fiori a destra.
        /// </summary>
        public static DoodleArt V84B_Bg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            // cielo
            p.FillFunc(Sd.Box(0f, 4.8f, 3.3f, 0.5f), (x, y) => Rgba.Lerp(Rgba.Hex("BFE6FF"), Rgba.Hex("8FD0FF"), (y - 4.3f) / 0.9f));
            // facciata di mattoni
            p.FillFunc(Sd.Box(0f, 0.45f, 3.3f, 3.9f), (x, y) =>
            {
                float row = (float)Math.Floor((y + 10f) / 0.25f);
                float bx = (x + 10f + (row % 2 == 0 ? 0f : 0.25f)) / 0.5f;
                float fy = V84B_Frac((y + 10f) / 0.25f), fx = V84B_Frac(bx);
                if (fy < 0.12f || fx < 0.05f) return V84BMortar;
                float shade = Noise.Value((float)Math.Floor(bx) * 3.1f, row * 1.7f);
                Rgba c = Rgba.Lerp(V84BBrick, V84BBrickDark, shade * 0.9f);
                return Rgba.Lerp(c, Rgba.Hex("8E4632"), Math.Max(0f, (Math.Abs(x) - 2.0f) / 1.2f) * 0.35f);
            });
            // bordo del tetto (grondaia)
            Body(p, Sd.Box(0f, 4.38f, 3.4f, 0.1f, 0.03f), Rgba.Hex("8A8F99"), Rgba.Hex("6E737D"));
            p.Fill(Sd.Box(0f, 4.24f, 3.4f, 0.04f), Rgba.Hex("000000", 0.2f));
            // pluviale
            Body(p, Sd.Box(-2.62f, 0.4f, 0.07f, 3.95f, 0.03f), Rgba.Hex("A3A9B3"), Rgba.Hex("7E848F"));
            foreach (float y in new[] { -2.6f, -0.6f, 1.4f, 3.4f }) p.Shape(Sd.Box(-2.62f, y, 0.11f, 0.035f, 0.01f), Rgba.Hex("6E737D"), 0.02f);
            // zoccolo di pietra
            p.FillFunc(Sd.Box(0f, -3.43f, 3.3f, 0.17f), (x, y) => Rgba.Lerp(Rgba.Hex("B5B0A8"), Rgba.Hex("9C978F"), Noise.Value(x * 3f, y * 3f) * 0.8f));
            p.InkLine(0.03f, -3.3f, -3.26f, 3.3f, -3.26f);
            // marciapiede a lastre
            p.FillFunc(Sd.Box(0f, -4.35f, 3.3f, 0.75f), (x, y) =>
            {
                float fx = V84B_Frac((x + 10f) / 0.9f);
                Rgba c = Rgba.Lerp(Rgba.Hex("CFCBC4"), Rgba.Hex("B9B4AC"), (y + 3.6f) / -1.5f);
                return fx < 0.03f ? Rgba.Hex("9C978F") : c;
            });
            p.InkLine(0.035f, -3.3f, -3.6f, 3.3f, -3.6f);
            // bordo d'erba in fondo
            p.FillFunc(Sd.Box(0f, -5.05f, 3.3f, 0.2f), (x, y) => Rgba.Lerp(Rgba.Hex("6DBF4E"), Rgba.Hex("4F9E39"), Noise.Value(x * 8f, y * 8f)));
            for (int i = 0; i < 26; i++)
            {
                float gx = -3.1f + i * 0.25f;
                p.Line(Rgba.Hex("4F9E39"), 0.025f, gx, -4.86f, gx + 0.05f, -4.72f);
            }
            // vasetto di fiori sul marciapiede (a destra)
            const float vx = 2.3f;
            Body(p, Sd.Polygon(vx - 0.22f, -3.3f, vx + 0.22f, -3.3f, vx + 0.16f, -3.62f, vx - 0.16f, -3.62f), Rgba.Hex("D9773F"), Rgba.Hex("B35A2A"));
            p.Shape(Sd.Box(vx, -3.28f, 0.25f, 0.05f, 0.02f), Rgba.Hex("C46632"), 0.03f);
            p.Line(Rgba.Hex("3E8E3A"), 0.03f, vx, -3.25f, vx - 0.05f, -2.95f);
            p.Line(Rgba.Hex("3E8E3A"), 0.03f, vx, -3.25f, vx + 0.12f, -3.0f);
            p.Shape(Sd.Circle(vx - 0.05f, -2.9f, 0.08f), Rgba.Hex("FF7AB0"), 0.02f);
            p.Shape(Sd.Circle(vx + 0.13f, -2.96f, 0.07f), Rgba.Hex("FFE15A"), 0.02f);
            return new DoodleArt("v84_b_bg", p);
        }

        /// <summary>La stanza dietro la finestra (origine al centro dell'apertura): luce calda, tende ai lati, una lampada.</summary>
        public static DoodleArt V84B_WindowBack()
        {
            float hx = V84BWindowHx, hy = V84BWindowHy;
            var p = new DoodlePainter(-hx - 0.04f, -hy - 0.04f, hx + 0.04f, hy + 0.04f, 110) { Ink = 0f };
            p.FillFunc(Sd.Box(0f, 0f, hx + 0.02f, hy + 0.02f), (x, y) =>
            {
                float d = (float)Math.Sqrt((x - 0.15f) * (x - 0.15f) + (y - 0.25f) * (y - 0.25f));
                return Rgba.Lerp(Rgba.Hex("FFE6A3"), Rgba.Hex("D99A5B"), Math.Min(1f, d / 0.9f));
            });
            // lampada appesa
            p.Line(Rgba.Hex("6B4129"), 0.02f, 0.15f, hy, 0.15f, 0.3f);
            p.Fill(Sd.Polygon(0.0f, 0.3f, 0.3f, 0.3f, 0.22f, 0.42f, 0.08f, 0.42f), Rgba.Hex("FFB347"));
            p.Fill(Sd.Circle(0.15f, 0.27f, 0.05f), Rgba.Hex("FFF6C8"));
            // tende ai lati
            foreach (float s in new[] { -1f, 1f })
            {
                Sdf curtain = Sd.Polygon(s * hx, hy + 0.02f, s * (hx - 0.2f), hy + 0.02f, s * (hx - 0.1f), -hy - 0.02f, s * hx, -hy - 0.02f);
                p.Fill(curtain, Rgba.Hex("E85D75"));
                p.Line(Rgba.Hex("C2455C"), 0.02f, s * (hx - 0.08f), hy, s * (hx - 0.05f), -hy);
            }
            return new DoodleArt("v84_b_window_back", p);
        }

        /// <summary>
        /// La cornice della finestra (origine al centro dell'apertura, apertura vuota): stipiti bianchi, architrave, davanzale
        /// sporgente in basso (piano in y -<see cref="V84BWindowHy"/>) e la fioriera verde sotto il davanzale.
        /// </summary>
        public static DoodleArt V84B_WindowFrame()
        {
            float hx = V84BWindowHx, hy = V84BWindowHy;
            var p = new DoodlePainter(-hx - 0.3f, -hy - 0.5f, hx + 0.3f, hy + 0.3f, 110) { Ink = 0.04f };
            Sdf hole = Sd.Box(0f, 0f, hx, hy);
            Body(p, Sd.Subtract(Sd.Box(0f, 0f, hx + 0.12f, hy + 0.12f, 0.02f), hole), Rgba.Hex("F5F0E6"), Rgba.Hex("D8D0C2"));
            Body(p, Sd.Box(0f, hy + 0.15f, hx + 0.2f, 0.07f, 0.02f), Rgba.Hex("EDE6D8"), Rgba.Hex("CFC6B6"));
            Body(p, Sd.Box(0f, -hy - 0.07f, hx + 0.24f, 0.07f, 0.02f), Rgba.Hex("EDE6D8"), Rgba.Hex("CFC6B6"));
            // fioriera
            Body(p, Sd.Box(0f, -hy - 0.28f, hx + 0.05f, 0.13f, 0.03f), Rgba.Hex("4FA35A"), Rgba.Hex("3A8045"));
            for (int i = 0; i < 6; i++)
            {
                float fx = -hx + 0.12f + i * (2f * hx - 0.24f) / 5f;
                p.Shape(Sd.Circle(fx, -hy - 0.1f, 0.06f), i % 2 == 0 ? Rgba.Hex("FF7AB0") : Rgba.Hex("FFE15A"), 0.018f);
            }
            return new DoodleArt("v84_b_window_frame", p);
        }

        /// <summary>Il vetro (origine al centro dell'apertura): due ante azzurrine semitrasparenti con la croce e il riflesso.</summary>
        public static DoodleArt V84B_WindowGlass()
        {
            float hx = V84BWindowHx, hy = V84BWindowHy;
            var p = new DoodlePainter(-hx - 0.02f, -hy - 0.02f, hx + 0.02f, hy + 0.02f, 110) { Ink = 0.03f };
            p.Fill(Sd.Box(0f, 0f, hx, hy), Rgba.Hex("BFE6FF", 0.45f));
            p.Shape(Sd.Box(0f, 0f, 0.035f, hy, 0f), Rgba.Hex("F5F0E6"), 0.02f);
            p.Shape(Sd.Box(0f, 0.05f, hx, 0.03f, 0f), Rgba.Hex("F5F0E6"), 0.02f);
            foreach (float s in new[] { -1f, 1f })
                p.Line(Rgba.White.WithAlpha(0.75f), 0.04f, s * 0.42f - 0.12f, 0.45f, s * 0.42f + 0.02f, 0.25f);
            return new DoodleArt("v84_b_window_glass", p);
        }

        /// <summary>
        /// Un tratto di scala (origine in basso al centro, alto <see cref="V84BSegH"/>): i due montanti di legno chiaro a x
        /// ±<see cref="V84BRailX"/> (contorno solo sui lati lunghi: i tratti si attaccano senza giunture) e un piolo a metà.
        /// </summary>
        public static DoodleArt V84B_LadderSeg()
        {
            float h = V84BSegH, rx = V84BRailX;
            var p = new DoodlePainter(-rx - 0.1f, -0.01f, rx + 0.1f, h + 0.01f, 120) { Ink = 0.035f, Wobble = 0.003f };
            // piolo (sotto i montanti)
            p.Shape(Sd.Box(0f, h * 0.5f, rx, 0.04f, 0.015f), V84BRung, 0.03f);
            p.Fill(Sd.Box(0f, h * 0.5f - 0.022f, rx, 0.012f), V84BRailShade);
            foreach (float s in new[] { -1f, 1f })
            {
                float x = s * rx;
                p.Fill(Sd.Box(x, h * 0.5f, 0.045f, h * 0.5f + 0.012f), V84BRail, false);
                p.Fill(Sd.Box(x + 0.022f, h * 0.5f, 0.018f, h * 0.5f + 0.012f), V84BRailShade, false);
                p.InkLine(0.03f, x - 0.045f, -0.01f, x - 0.045f, h + 0.01f);
                p.InkLine(0.03f, x + 0.045f, -0.01f, x + 0.045f, h + 0.01f);
            }
            return new DoodleArt("v84_b_ladder_seg", p);
        }

        /// <summary>I piedini di gomma della scala (origine sul pavimento, al centro).</summary>
        public static DoodleArt V84B_LadderFoot()
        {
            float rx = V84BRailX;
            var p = new DoodlePainter(-rx - 0.12f, -0.04f, rx + 0.12f, 0.1f, 120) { Ink = 0.03f, Wobble = 0.003f };
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Box(s * rx, 0.025f, 0.065f, 0.045f, 0.02f), Rgba.Hex("3A2B2B"));
            return new DoodleArt("v84_b_ladder_foot", p);
        }

        /// <summary>I tappi in cima ai montanti (origine alla fine dell'ultimo tratto, al centro).</summary>
        public static DoodleArt V84B_LadderTop()
        {
            float rx = V84BRailX;
            var p = new DoodlePainter(-rx - 0.1f, -0.04f, rx + 0.1f, 0.1f, 120) { Ink = 0.03f, Wobble = 0.003f };
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Box(s * rx, 0.02f, 0.05f, 0.05f, 0.04f), V84BRailShade);
            return new DoodleArt("v84_b_ladder_top", p);
        }

        /// <summary>Gambe, scarpe e salopette comuni dell'omino (origine ai piedi).</summary>
        static void V84B_Legs(DoodlePainter p)
        {
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Box(s * 0.1f, 0.25f, 0.075f, 0.2f, 0.03f), V84BOveralls, V84BOverallsShade);
                p.Shape(Sd.Box(s * 0.12f, 0.04f, 0.11f, 0.05f, 0.04f), V84BShoe);
            }
        }

        /// <summary>Il berretto rosso col pompon (sulla testa di centro cx, cy, raggio r).</summary>
        static void V84B_Hat(DoodlePainter p, float cx, float cy, float r)
        {
            Sdf hat = Sd.Intersect(Sd.Circle(cx, cy + 0.02f, r + 0.02f), Sd.Box(cx, cy + r * 0.8f, r + 0.05f, r * 0.45f));
            Body(p, hat, V84BHat, V84BHatShade);
            p.Shape(Sd.Box(cx, cy + r * 0.4f, r + 0.02f, 0.045f, 0.03f), Rgba.Hex("F5F0E6"), 0.025f);
            p.Shape(Sd.Circle(cx, cy + r + 0.07f, 0.06f), Rgba.Hex("F5F0E6"), 0.025f);
        }

        /// <summary>L'omino in piedi, di fronte (origine ai piedi, alto circa 1.3): salopette blu, maglietta gialla, berretto rosso, faccia speranzosa.</summary>
        public static DoodleArt V84B_Man()
        {
            var p = new DoodlePainter(-0.42f, -0.04f, 0.42f, 1.4f, 120) { Ink = 0.04f };
            V84B_Legs(p);
            // braccia lungo i fianchi
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Capsule(s * 0.22f, 0.78f, s * 0.29f, 0.5f, 0.055f), V84BShirt);
                p.Shape(Sd.Circle(s * 0.3f, 0.45f, 0.06f), V84BSkin);
            }
            Body(p, Sd.Box(0f, 0.65f, 0.21f, 0.22f, 0.07f), V84BShirt, Rgba.Hex("D9B634"));
            Body(p, Sd.Box(0f, 0.55f, 0.18f, 0.14f, 0.04f), V84BOveralls, V84BOverallsShade);
            foreach (float s in new[] { -1f, 1f }) p.Line(V84BOveralls, 0.04f, s * 0.13f, 0.66f, s * 0.15f, 0.85f);
            p.Shape(Sd.Circle(0f, 0.57f, 0.025f), Rgba.Hex("FFE15A"), 0.015f);
            // testa
            const float hy = 1.06f, r = 0.21f;
            Body(p, Sd.Circle(0f, hy, r), V84BSkin, V84BSkinShade);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Ellipse(s * 0.075f, hy - 0.025f, 0.028f, 0.038f), DoodlePainter.DefaultInk);
                p.Fill(Sd.Circle(s * 0.075f - 0.01f, hy - 0.012f, 0.01f), Rgba.White);
                p.Fill(Sd.Ellipse(s * 0.13f, hy - 0.07f, 0.04f, 0.025f), Rgba.Hex("FF9AB0", 0.7f));
            }
            p.InkLine(0.016f, -0.065f, hy - 0.095f, -0.03f, hy - 0.125f, 0.03f, hy - 0.125f, 0.065f, hy - 0.095f);
            V84B_Hat(p, 0f, hy, r);
            return new DoodleArt("v84_b_man", p);
        }

        /// <summary>L'omino di spalle che sale (origine ai piedi): braccia in su (mani a circa 1.35), berretto, niente faccia.</summary>
        public static DoodleArt V84B_ManClimb()
        {
            var p = new DoodlePainter(-0.42f, -0.04f, 0.42f, 1.48f, 120) { Ink = 0.04f };
            V84B_Legs(p);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Capsule(s * 0.2f, 0.8f, s * 0.3f, 1.3f, 0.055f), V84BShirt);
                p.Shape(Sd.Circle(s * 0.31f, 1.36f, 0.065f), V84BSkin);
            }
            Body(p, Sd.Box(0f, 0.65f, 0.21f, 0.22f, 0.07f), V84BShirt, Rgba.Hex("D9B634"));
            Body(p, Sd.Box(0f, 0.55f, 0.18f, 0.14f, 0.04f), V84BOveralls, V84BOverallsShade);
            foreach (float s in new[] { -1f, 1f }) p.Line(V84BOveralls, 0.04f, s * 0.13f, 0.66f, s * 0.05f, 0.85f);
            const float hy = 1.06f, r = 0.21f;
            Body(p, Sd.Circle(0f, hy, r), V84BHair, Rgba.Hex("4E2E1C"));
            V84B_Hat(p, 0f, hy, r);
            return new DoodleArt("v84_b_man_climb", p);
        }

        /// <summary>Il busto felice dell'omino nella finestra (origine sul davanzale, al centro): sorrisone, occhi chiusi a "^", senza il braccio che saluta.</summary>
        public static DoodleArt V84B_ManBust()
        {
            var p = new DoodlePainter(-0.36f, -0.04f, 0.36f, 0.95f, 120) { Ink = 0.04f };
            p.Shape(Sd.Capsule(-0.22f, 0.3f, -0.3f, 0.06f, 0.055f), V84BShirt);
            Body(p, Sd.Box(0f, 0.18f, 0.21f, 0.2f, 0.07f), V84BShirt, Rgba.Hex("D9B634"));
            Body(p, Sd.Box(0f, 0.08f, 0.18f, 0.1f, 0.04f), V84BOveralls, V84BOverallsShade);
            const float hy = 0.6f, r = 0.21f;
            Body(p, Sd.Circle(0f, hy, r), V84BSkin, V84BSkinShade);
            foreach (float s in new[] { -1f, 1f })
            {
                p.InkLine(0.025f, s * 0.12f, hy - 0.04f, s * 0.075f, hy + 0.01f, s * 0.03f, hy - 0.04f);
                p.Fill(Sd.Ellipse(s * 0.14f, hy - 0.07f, 0.045f, 0.028f), Rgba.Hex("FF9AB0", 0.85f));
            }
            Sdf smile = Sd.Intersect(Sd.Ellipse(0f, hy - 0.08f, 0.1f, 0.09f), Sd.Box(0f, hy - 0.14f, 0.11f, 0.06f));
            p.Shape(smile, Rgba.Hex("5A1F3A"), 0.022f);
            V84B_Hat(p, 0f, hy, r);
            return new DoodleArt("v84_b_man_bust", p);
        }

        /// <summary>Il braccio che saluta (origine alla spalla): manica gialla in su e la mano aperta in cima (y circa 0.42).</summary>
        public static DoodleArt V84B_ManArm()
        {
            var p = new DoodlePainter(-0.12f, -0.08f, 0.14f, 0.56f, 120) { Ink = 0.035f };
            p.Shape(Sd.Capsule(0f, 0f, 0.02f, 0.34f, 0.055f), V84BShirt);
            Sdf hand = Sd.Union(Sd.Circle(0.02f, 0.42f, 0.07f),
                Sd.Capsule(-0.03f, 0.45f, -0.05f, 0.52f, 0.018f), Sd.Capsule(0.0f, 0.47f, 0.0f, 0.54f, 0.018f),
                Sd.Capsule(0.04f, 0.47f, 0.05f, 0.53f, 0.018f), Sd.Capsule(0.07f, 0.42f, 0.11f, 0.46f, 0.018f));
            p.Shape(hand, V84BSkin);
            return new DoodleArt("v84_b_man_arm", p);
        }
    }
}
