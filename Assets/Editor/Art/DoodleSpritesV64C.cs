using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I pochi disegni nuovi della 0.64 per i livelli 257 (The perfect liar) e 258 (The rigged three-card game).
    /// Tutto il resto riusa gli sprite esistenti (stanza degli interrogatori, macchina della verità, sospettato, ladro,
    /// impiegato, guardia, manette, mago, cilindro, monete, strada di notte). Qui solo: la manina-cursore, il dorso e
    /// le facce delle carte (sulla sagoma di "playing_card"), la macchia d'inchiostro a corona e il mazziere (il mago
    /// di "s41b_magician" senza cilindro, che diventa uno sprite a parte e gli può volare via).
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV64C() => new DoodleArt[]
        {
            V64C_HandCursor(),
            V64C_CardBack(), V64C_CardTwoSpades(), V64C_CardThreeClubs(), V64C_CardThreeHearts(), V64C_CardQueen(), V64C_CardQueenInk(),
            V64C_CardJoker(), V64C_InkCrown(), V64C_Dealer(), V64C_DealerHat(),
        };

        static readonly Rgba V64C_Red = Rgba.Hex("E8433F"), V64C_Black = Rgba.Hex("2B2233"), V64C_InkBlue = Rgba.Hex("1F2C6B");

        // ---------- 257 ----------

        /// <summary>Manina-cursore col guanto bianco che indica in su (punta del dito in 0, 0.3; polso in 0, -0.27).</summary>
        public static DoodleArt V64C_HandCursor()
        {
            var p = new DoodlePainter(-0.24f, -0.34f, 0.26f, 0.36f, 220) { Ink = 0.03f, Wobble = 0.004f };
            Sdf finger = Sd.Capsule(-0.02f, 0.02f, -0.02f, 0.27f, 0.055f);
            Sdf palm = Sd.Box(0.04f, -0.08f, 0.13f, 0.12f, 0.06f);
            Sdf curled = Sd.Union(Sd.Circle(0.07f, 0.03f, 0.05f), Sd.Circle(0.13f, 0.01f, 0.045f), Sd.Circle(0.17f, -0.04f, 0.04f));
            Sdf thumb = Sd.Capsule(-0.06f, -0.08f, -0.15f, 0.0f, 0.045f);
            p.Shape(Sd.Box(0.04f, -0.24f, 0.12f, 0.045f, 0.02f), Rgba.Hex("D8DCE8"), 0.03f);   // polsino
            p.Shape(Sd.Union(finger, palm, curled, thumb), Rgba.White);
            p.Fill(Sd.Crescent(Sd.Union(palm, curled), -0.03f, 0.03f), Rgba.Hex("D8DCE8"));
            p.InkLine(0.014f, 0.1f, -0.02f, 0.1f, -0.08f);
            p.InkLine(0.014f, 0.155f, -0.04f, 0.155f, -0.09f);
            p.InkLine(0.014f, 0.0f, -0.12f, 0.06f, -0.14f, 0.12f, -0.12f);
            return new DoodleArt("v64_hand_cursor", p);
        }

        // ---------- 258: le carte (stessa sagoma e stesso centro di "playing_card") ----------

        static DoodlePainter V64C_Card(Rgba face)
        {
            var p = new DoodlePainter(-0.3f, -0.42f, 0.3f, 0.42f, 160) { Ink = 0.025f };
            p.Shape(Sd.Box(0, 0, 0.25f, 0.37f, 0.04f), face, 0.025f);
            return p;
        }

        static Sdf V64C_Heart(float cx, float cy, float s) =>
            Sd.Union(Sd.Circle(cx - 0.05f * s, cy + 0.04f * s, 0.06f * s), Sd.Circle(cx + 0.05f * s, cy + 0.04f * s, 0.06f * s),
                Sd.Polygon(cx - 0.105f * s, cy + 0.02f * s, cx + 0.105f * s, cy + 0.02f * s, cx, cy - 0.1f * s));

        static Sdf V64C_Spade(float cx, float cy, float s) =>
            Sd.Union(Sd.Rotate(V64C_Heart(cx, cy, s), cx, cy, 180f),
                Sd.Polygon(cx - 0.045f * s, cy - 0.14f * s, cx + 0.045f * s, cy - 0.14f * s, cx, cy - 0.03f * s));

        static Sdf V64C_Club(float cx, float cy, float s) =>
            Sd.Union(Sd.Circle(cx, cy + 0.055f * s, 0.048f * s), Sd.Circle(cx - 0.055f * s, cy - 0.01f * s, 0.048f * s),
                Sd.Circle(cx + 0.055f * s, cy - 0.01f * s, 0.048f * s),
                Sd.Polygon(cx - 0.04f * s, cy - 0.12f * s, cx + 0.04f * s, cy - 0.12f * s, cx, cy));

        /// <summary>Indice d'angolo (un "2", un "3" o una "Q" a mano) in alto a sinistra e capovolto in basso a destra.</summary>
        static void V64C_Index(DoodlePainter p, char glyph, Rgba color, float width = 0.018f)
        {
            foreach (int k in new[] { 1, -1 })
            {
                float cx = -0.175f * k, cy = 0.29f * k, s = 0.9f * k;
                Sdf g;
                switch (glyph)
                {
                    case '2':
                        g = Sd.Polyline(width * 0.5f, cx - 0.03f * s, cy + 0.025f * s, cx - 0.015f * s, cy + 0.045f * s, cx + 0.01f * s, cy + 0.048f * s,
                            cx + 0.03f * s, cy + 0.03f * s, cx + 0.025f * s, cy + 0.005f * s, cx - 0.03f * s, cy - 0.045f * s, cx + 0.035f * s, cy - 0.045f * s);
                        break;
                    case '3':
                        g = Sd.Polyline(width * 0.5f, cx - 0.03f * s, cy + 0.045f * s, cx + 0.03f * s, cy + 0.045f * s, cx, cy + 0.005f * s,
                            cx + 0.03f * s, cy - 0.015f * s, cx + 0.02f * s, cy - 0.04f * s, cx - 0.005f * s, cy - 0.048f * s, cx - 0.03f * s, cy - 0.035f * s);
                        break;
                    default:   // Q
                        g = Sd.Union(Sd.Subtract(Sd.Ellipse(cx, cy, 0.035f, 0.045f), Sd.Ellipse(cx, cy, 0.035f - width, 0.045f - width)),
                            Sd.Polyline(width * 0.5f, cx + 0.005f * s, cy - 0.02f * s, cx + 0.045f * s, cy - 0.06f * s));
                        break;
                }
                p.Fill(g, color, false);
            }
        }

        /// <summary>Dorso rosso a rombi col bordo bianco.</summary>
        public static DoodleArt V64C_CardBack()
        {
            DoodlePainter p = V64C_Card(Rgba.White);
            p.FillFunc(Sd.Box(0, 0, 0.2f, 0.32f, 0.02f), (x, y) =>
            {
                float u = (x + y) * 14f, v = (x - y) * 14f;
                bool line = System.Math.Abs(u - System.Math.Round(u)) < 0.12f || System.Math.Abs(v - System.Math.Round(v)) < 0.12f;
                return line ? Rgba.Hex("F7C6C2") : Rgba.Hex("C7302C");
            });
            p.Shape(Sd.Box(0, 0, 0.2f, 0.32f, 0.02f), Rgba.Clear, 0.015f);
            p.Shape(Sd.Ellipse(0, 0, 0.07f, 0.1f), Rgba.Hex("FFD24A"), 0.015f);
            return new DoodleArt("v64_card_back", p);
        }

        /// <summary>Il due di picche (la carta che esce sempre).</summary>
        public static DoodleArt V64C_CardTwoSpades()
        {
            DoodlePainter p = V64C_Card(Rgba.White);
            p.Fill(V64C_Spade(0f, 0.15f, 0.9f), V64C_Black);
            p.Fill(Sd.Rotate(V64C_Spade(0f, 0.15f, 0.9f), 0f, 0f, 180f), V64C_Black);
            V64C_Index(p, '2', V64C_Black);
            return new DoodleArt("v64_card_two_spades", p);
        }

        /// <summary>Tre di fiori (una delle due carte "normali" mostrate all'inizio).</summary>
        public static DoodleArt V64C_CardThreeClubs()
        {
            DoodlePainter p = V64C_Card(Rgba.White);
            foreach (float y in new[] { 0.19f, 0.01f, -0.17f }) p.Fill(V64C_Club(0f, y, 0.8f), V64C_Black);
            V64C_Index(p, '3', V64C_Black);
            return new DoodleArt("v64_card_three_clubs", p);
        }

        /// <summary>Tre di cuori (l'altra carta "normale" mostrata all'inizio).</summary>
        public static DoodleArt V64C_CardThreeHearts()
        {
            DoodlePainter p = V64C_Card(Rgba.White);
            foreach (float y in new[] { 0.19f, 0.01f, -0.17f }) p.Fill(V64C_Heart(0f, y, 0.85f), V64C_Red);
            V64C_Index(p, '3', V64C_Red);
            return new DoodleArt("v64_card_three_hearts", p);
        }

        /// <summary>La Regina di cuori stampata (quella mostrata all'inizio: corona d'oro, viso, capelli, cuore).</summary>
        public static DoodleArt V64C_CardQueen()
        {
            DoodlePainter p = V64C_Card(Rgba.White);
            p.Shape(Sd.Box(0, -0.06f, 0.12f, 0.16f, 0.06f), Rgba.Hex("E8433F"), 0.018f);                      // abito
            p.Shape(Sd.Ellipse(0, 0.06f, 0.1f, 0.1f), Rgba.Hex("F2C14E"), 0.018f);                              // capelli
            p.Shape(Sd.Ellipse(0, 0.05f, 0.065f, 0.075f), Rgba.Hex("FFD7B5"), 0.018f);                          // viso
            p.Fill(Sd.Circle(-0.025f, 0.065f, 0.011f), V64C_Black);
            p.Fill(Sd.Circle(0.025f, 0.065f, 0.011f), V64C_Black);
            p.Line(Rgba.Hex("C0392B"), 0.012f, -0.02f, 0.022f, 0f, 0.012f, 0.02f, 0.022f);
            p.Shape(Sd.Polygon(-0.08f, 0.13f, 0.08f, 0.13f, 0.09f, 0.22f, 0.045f, 0.17f, 0f, 0.235f, -0.045f, 0.17f, -0.09f, 0.22f),
                Rgba.Hex("FFC83D"), 0.016f);                                                                    // corona
            p.Fill(V64C_Heart(0f, -0.1f, 0.55f), Rgba.White);
            V64C_Index(p, 'Q', V64C_Red);
            p.Fill(V64C_Heart(-0.175f, 0.2f, 0.25f), V64C_Red);
            p.Fill(V64C_Heart(0.175f, -0.2f, 0.25f), V64C_Red);
            return new DoodleArt("v64_card_queen", p);
        }

        /// <summary>
        /// La Regina "fatta in casa": il due di picche con la corona d'inchiostro blu stampata sopra, un viso e le "Q"
        /// scarabocchiate sopra i 2. Ben diversa da tutte le altre carte.
        /// </summary>
        public static DoodleArt V64C_CardQueenInk()
        {
            DoodlePainter p = V64C_Card(Rgba.Hex("FFF9EC"));
            Rgba faint = V64C_Black.WithAlpha(0.25f);
            p.Fill(V64C_Spade(0f, -0.17f, 0.7f), faint);
            V64C_Index(p, '2', faint);
            Rgba ink = V64C_InkBlue.WithAlpha(0.92f);
            p.Fill(V64C_CrownShape(0f, 0.15f, 0.62f), ink);                                                    // la corona trasferita
            p.Shape(Sd.Ellipse(0f, -0.02f, 0.085f, 0.1f), Rgba.Hex("FFE6CC"), 0.016f);                          // viso a mano
            p.Fill(Sd.Circle(-0.03f, 0.0f, 0.012f), ink);
            p.Fill(Sd.Circle(0.03f, 0.0f, 0.012f), ink);
            p.Line(ink, 0.012f, -0.035f, -0.05f, 0f, -0.07f, 0.035f, -0.05f);
            p.Line(ink, 0.016f, -0.09f, 0.04f, -0.12f, -0.1f, -0.1f, -0.2f);                                   // capelli
            p.Line(ink, 0.016f, 0.09f, 0.04f, 0.12f, -0.1f, 0.1f, -0.2f);
            V64C_Index(p, 'Q', ink, 0.022f);
            p.Fill(Sd.Circle(0.14f, 0.24f, 0.02f), ink);                                                        // schizzi
            p.Fill(Sd.Circle(-0.15f, -0.1f, 0.013f), ink);
            return new DoodleArt("v64_card_queen_ink", p);
        }

        /// <summary>Il Jolly: cappello da giullare a tre punte coi sonagli (la scritta JOKER la mette il livello).</summary>
        public static DoodleArt V64C_CardJoker()
        {
            DoodlePainter p = V64C_Card(Rgba.White);
            p.Shape(Sd.Polygon(-0.13f, -0.02f, -0.2f, 0.17f, -0.05f, 0.05f), Rgba.Hex("7B4FD6"), 0.016f);
            p.Shape(Sd.Polygon(0.13f, -0.02f, 0.2f, 0.17f, 0.05f, 0.05f), Rgba.Hex("3FA34D"), 0.016f);
            p.Shape(Sd.Polygon(-0.07f, 0.0f, 0f, 0.22f, 0.07f, 0.0f), V64C_Red, 0.016f);
            foreach (float[] b in new[] { new[] { -0.2f, 0.18f }, new[] { 0.2f, 0.18f }, new[] { 0f, 0.235f } })
                p.Shape(Sd.Circle(b[0], b[1], 0.025f), Rgba.Hex("FFD24A"), 0.012f);
            p.Shape(Sd.Box(0, -0.03f, 0.14f, 0.035f, 0.02f), Rgba.Hex("FFD24A"), 0.016f);
            p.Shape(Sd.Ellipse(0, -0.13f, 0.075f, 0.075f), Rgba.Hex("FFE6CC"), 0.016f);
            p.Fill(Sd.Circle(-0.028f, -0.115f, 0.01f), V64C_Black);
            p.Fill(Sd.Circle(0.028f, -0.115f, 0.01f), V64C_Black);
            p.Line(V64C_Black, 0.012f, -0.04f, -0.155f, 0f, -0.18f, 0.04f, -0.155f);
            return new DoodleArt("v64_card_joker", p);
        }

        // ---------- 258: macchia e mazziere ----------

        static Sdf V64C_CrownShape(float cx, float cy, float s) =>
            Sd.Polygon(cx - 0.2f * s, cy - 0.1f * s, cx + 0.2f * s, cy - 0.1f * s, cx + 0.23f * s, cy + 0.12f * s, cx + 0.11f * s, cy + 0.01f * s,
                cx, cy + 0.16f * s, cx - 0.11f * s, cy + 0.01f * s, cx - 0.23f * s, cy + 0.12f * s);

        /// <summary>Macchiolina d'inchiostro a forma (vaga) di corona, con due schizzi (centro 0,0, larga 0.5).</summary>
        public static DoodleArt V64C_InkCrown()
        {
            var p = new DoodlePainter(-0.3f, -0.2f, 0.3f, 0.22f, 200) { Ink = 0.02f, Wobble = 0.012f, WobbleFreq = 9f };
            Rgba ink = V64C_InkBlue.WithAlpha(0.88f);
            p.Fill(V64C_CrownShape(0f, 0f, 1.05f), ink);
            p.Fill(Sd.Circle(-0.245f, 0.15f, 0.025f), ink);
            p.Fill(Sd.Circle(0.255f, 0.16f, 0.02f), ink);
            p.Fill(Sd.Circle(0f, 0.18f, 0.022f), ink);
            p.Fill(Sd.Circle(0.2f, -0.15f, 0.016f), ink);
            p.Fill(Sd.Ellipse(-0.06f, -0.03f, 0.07f, 0.03f), Rgba.Hex("4A5BA8", 0.5f));                     // riflesso bagnato
            return new DoodleArt("v64_ink_crown", p);
        }

        /// <summary>
        /// Il mazziere: il mago di "s41b_magician" (stesse coordinate: alto 2.4, piedi in -1.2) ma senza cilindro, coi
        /// capelli impomatati. Il cilindro è "v64_dealer_hat", nello stesso riquadro: gli può volare via.
        /// </summary>
        public static DoodleArt V64C_Dealer()
        {
            var p = new DoodlePainter(-0.8f, -1.25f, 0.8f, 1.25f, 110) { Ink = 0.045f };
            p.Shape(Sd.Polygon(-0.55f, -1.1f, 0.5f, -1.1f, 0.3f, 0.45f, -0.35f, 0.45f), Rgba.Hex("3B2A5C"));
            p.Shape(Sd.Box(-0.25f, -0.5f, 0.12f, 0.1f, 0.02f), Rgba.Hex("7F6BAE"), 0.025f);
            p.InkLine(0.015f, -0.33f, -0.5f, -0.17f, -0.5f);
            foreach (float x in new[] { -0.15f, 0.12f })
            {
                p.Shape(Sd.Box(x, -1.0f, 0.07f, 0.15f), Rgba.Hex("1E1A26"), 0.025f);
                p.Shape(Sd.Ellipse(x + 0.04f, -1.15f, 0.13f, 0.06f), Rgba.Hex("1E1A26"), 0.02f);
            }
            Body(p, Sd.Box(0, -0.25f, 0.24f, 0.55f, 0.1f), Rgba.Hex("2B2B3A"), Rgba.Hex("1E1E2A"));
            p.Fill(Sd.Polygon(-0.1f, 0.3f, 0.1f, 0.3f, 0f, -0.1f), Rgba.White);
            p.Shape(Sd.Polygon(-0.12f, 0.27f, 0f, 0.22f, 0.12f, 0.27f, 0.12f, 0.17f, 0f, 0.22f, -0.12f, 0.17f), Rgba.Hex("D6453B"), 0.015f);
            p.Shape(Sd.Capsule(0.2f, 0.2f, 0.42f, 0.15f, 0.06f), Rgba.Hex("2B2B3A"), 0.025f);
            p.Shape(Sd.Circle(0.45f, 0.15f, 0.07f), Rgba.White, 0.02f);
            p.Shape(Sd.Capsule(-0.2f, 0.2f, -0.35f, -0.15f, 0.06f), Rgba.Hex("2B2B3A"), 0.025f);
            p.Shape(Sd.Circle(-0.36f, -0.18f, 0.07f), Rgba.White, 0.02f);
            Sdf head = Sd.Ellipse(0, 0.55f, 0.2f, 0.24f);
            Body(p, head, Rgba.Hex("FFD7B5"), Rgba.Hex("E8B48C"), null, -0.04f, 0.04f);
            p.Shape(Sd.Intersect(Sd.Ellipse(0, 0.6f, 0.215f, 0.25f), Sd.Box(0, 0.79f, 0.3f, 0.1f)), Rgba.Hex("1E1A26"), 0.03f);   // capelli impomatati
            p.Line(Rgba.White.WithAlpha(0.35f), 0.02f, -0.1f, 0.78f, 0.05f, 0.8f);
            S41B_Eyes(p, -0.07f, 0.08f, 0.6f, 0.045f, 0.4f, 0f);
            p.Shape(Sd.Union(Sd.Ellipse(-0.08f, 0.47f, 0.09f, 0.035f), Sd.Ellipse(0.08f, 0.47f, 0.09f, 0.035f)), Rgba.Hex("3A2A22"), 0.015f);
            p.Shape(Sd.Ellipse(0.03f, 0.53f, 0.04f, 0.035f), Rgba.Hex("F2A08A"), 0.015f);
            return new DoodleArt("v64_dealer", p);
        }

        /// <summary>Il cilindro del mazziere (lo stesso disegnato su "s41b_magician"), nello stesso riquadro di "v64_dealer".</summary>
        public static DoodleArt V64C_DealerHat()
        {
            var p = new DoodlePainter(-0.8f, -1.25f, 0.8f, 1.25f, 110) { Ink = 0.045f };
            p.Shape(Sd.Box(0, 0.78f, 0.28f, 0.035f, 0.02f), Rgba.Hex("1E1A26"));
            p.Shape(Sd.Rotate(Sd.Box(0.02f, 1.0f, 0.17f, 0.2f, 0.03f), 0f, 0.8f, -6f), Rgba.Hex("1E1A26"));
            p.Fill(Sd.Rotate(Sd.Box(0.02f, 0.87f, 0.17f, 0.04f), 0f, 0.8f, -6f), Rgba.Hex("C02A3A"));
            return new DoodleArt("v64_dealer_hat", p);
        }
    }
}
