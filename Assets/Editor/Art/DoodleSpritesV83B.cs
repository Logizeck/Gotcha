using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.83 per il livello "Luck Hack" (roadmap n. 54): lo sfondo (parete verde a righe con il ferro
    /// di cavallo appeso al contrario e il quadrifoglio incorniciato, il tavolo da gioco col panno verde e il bordo di
    /// legno, il pavimento scuro dove sta il pulsante), le due facce della moneta (HEADS: un re sorridente con la corona;
    /// TAILS: una coda arricciata; la parola la scrive il livello con un testo), la moneta vista di taglio (per la
    /// giravolta), il pulsante FLIP (la scritta è un testo del livello) e la targhetta del punteggio sulla parete.
    /// L'ombra della moneta è "ball_shadow", la scintilla "sparkle_star", il bagliore "glow". Prefisso v83_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV83B() => new DoodleArt[]
        {
            V83B_Bg(), V83B_Coin(true), V83B_Coin(false), V83B_CoinEdge(), V83B_Button(), V83B_Plaque(),
        };

        /// <summary>Piano del tavolo (y del panno dove poggia la moneta) e raggio della moneta.</summary>
        public const float V83BTableY = -1.35f, V83BCoinR = 0.6f;

        static readonly Rgba V83BFelt = Rgba.Hex("2E8A57"), V83BFeltShade = Rgba.Hex("1F6B42");
        static readonly Rgba V83BCoinGold = Rgba.Hex("FFCB45"), V83BCoinShade = Rgba.Hex("E09A1E"), V83BCoinDeep = Rgba.Hex("C98316");

        static float V83B_Clamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        /// <summary>
        /// Lo sfondo (coordinate del mondo): parete verde scuro a righe fino a y -0.2 (ferro di cavallo capovolto a
        /// sinistra, quadrifoglio incorniciato a destra in basso), il tavolo da gioco (panno verde da y -0.2 a -2.2 in
        /// leggera prospettiva, bordo di legno, fronte di legno fino a -2.75), il pavimento scuro sotto (lì il pulsante).
        /// </summary>
        public static DoodleArt V83B_Bg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            // parete a righe
            p.FillFunc(Sd.Box(0f, 1.2f, 3.3f, 4.1f), (x, y) =>
            {
                Rgba c = (int)Math.Floor((x + 10f) * 1.8f) % 2 == 0 ? Rgba.Hex("2D5B4E") : Rgba.Hex("285246");
                return Rgba.Lerp(c, Rgba.Hex("1C3A33"), V83B_Clamp((Math.Abs(x) - 1.6f) / 1.6f) * 0.7f);
            });
            // modanatura
            p.Shape(Sd.Box(0f, 0.55f, 3.3f, 0.06f), Rgba.Hex("C9A55A"), 0.035f);
            // ferro di cavallo appeso al contrario (apertura in giù: la fortuna "cade fuori")
            const float hx = -1.75f, hy = 1.6f;
            Sdf arch = Sd.Intersect(Sd.Subtract(Sd.Circle(hx, hy, 0.42f), Sd.Circle(hx, hy, 0.24f)), Sd.Box(hx, hy + 0.25f, 0.5f, 0.25f));
            Sdf shoe = Sd.Union(arch, Sd.Box(hx - 0.33f, hy - 0.16f, 0.09f, 0.17f, 0.03f), Sd.Box(hx + 0.33f, hy - 0.16f, 0.09f, 0.17f, 0.03f));
            Body(p, shoe, Rgba.Hex("B8BEC8"), Rgba.Hex("858C99"));
            p.InkLine(0.025f, hx, hy + 0.62f, hx, hy + 0.42f);
            p.Shape(Sd.Circle(hx, hy + 0.64f, 0.045f), Rgba.Hex("6E5A3A"), 0.02f);
            foreach (float sx in new[] { -1f, 1f })
                foreach (float ny in new[] { 0.05f, -0.18f }) p.Shape(Sd.Circle(hx + sx * 0.33f, hy + ny, 0.03f), Rgba.Hex("5E6470"), 0.012f);
            // tre stelline che "cadono" dal ferro
            for (int i = 0; i < 3; i++) p.Shape(Sd.Circle(hx - 0.3f + i * 0.3f, hy - 0.55f - 0.12f * (i % 2), 0.035f), Rgba.Hex("FFE28A"), 0.012f);
            // quadrifoglio in cornice (in basso a destra della parete, lontano dalla traiettoria della moneta)
            Body(p, Sd.Box(1.85f, 1.25f, 0.42f, 0.5f, 0.04f), Rgba.Hex("B07A3C"), Rgba.Hex("845628"));
            p.Shape(Sd.Box(1.85f, 1.25f, 0.32f, 0.4f, 0.02f), Rgba.Hex("F3EBD6"), 0.025f);
            for (int i = 0; i < 4; i++)
            {
                double a = Math.PI / 4 + i * Math.PI / 2;
                float lx = 1.85f + (float)Math.Cos(a) * 0.11f, ly = 1.32f + (float)Math.Sin(a) * 0.11f;
                p.Shape(Sd.Circle(lx, ly, 0.1f), Rgba.Hex("4DB868"), 0.02f);
            }
            p.Line(Rgba.Hex("2F7A40"), 0.03f, 1.85f, 1.3f, 1.88f, 1.08f, 1.95f, 0.98f);
            // tavolo: panno verde in prospettiva con il bordo di legno
            Sdf top = Sd.Polygon(-2.75f, -2.2f, 2.75f, -2.2f, 2.45f, -0.2f, -2.45f, -0.2f);
            Body(p, Sd.Grow(top, 0.13f), Wood, WoodShade);
            p.FillFunc(top, (x, y) =>
            {
                float vign = V83B_Clamp((float)Math.Sqrt(x * x * 0.12f + (y + 1.3f) * (y + 1.3f) * 0.35f));
                return Rgba.Lerp(V83BFelt, V83BFeltShade, vign * 0.8f + Noise.Value(x * 6f, y * 6f) * 0.12f);
            });
            p.Shape(top, Rgba.Clear, 0.04f);
            // filo dorato del panno
            p.Shape(Sd.Subtract(Sd.Grow(Sd.Polygon(-2.45f, -2.0f, 2.45f, -2.0f, 2.2f, -0.38f, -2.2f, -0.38f), 0.015f),
                                Sd.Grow(Sd.Polygon(-2.45f, -2.0f, 2.45f, -2.0f, 2.2f, -0.38f, -2.2f, -0.38f), -0.015f)), Rgba.Hex("E7C46A"), 0f);
            // pavimento scuro (il pulsante FLIP sta qui)
            p.FillFunc(Sd.Box(0f, -4.1f, 3.3f, 1.35f), (x, y) => (int)Math.Floor((x + 10f) * 1.25f) % 2 == 0 ? Rgba.Hex("3B2A33") : Rgba.Hex("33242C"));
            // fronte del tavolo e gambe
            Body(p, Sd.Box(0f, -2.55f, 2.85f, 0.22f, 0.04f), Rgba.Hex("B66E33"), WoodDark);
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Box(s * 2.45f, -3.25f, 0.14f, 0.5f, 0.03f), WoodShade, WoodDark);
            p.Fill(Sd.Box(0f, -2.82f, 3.3f, 0.05f), Rgba.Hex("000000", 0.3f));
            return new DoodleArt("v83_b_bg", p);
        }

        /// <summary>
        /// Una faccia della moneta (centro 0,0, raggio <see cref="V83BCoinR"/>): bordo zigrinato e disco interno; HEADS un
        /// re sorridente con la corona, TAILS una coda arricciata. In basso (y ≈ -0.3) resta spazio per la parola (testo).
        /// </summary>
        public static DoodleArt V83B_Coin(bool heads)
        {
            float r = V83BCoinR;
            var p = new DoodlePainter(-r - 0.06f, -r - 0.06f, r + 0.06f, r + 0.06f, 160) { Ink = 0.04f };
            Body(p, Sd.Circle(0f, 0f, r), V83BCoinGold, V83BCoinShade, null, -0.08f, 0.08f);
            for (int i = 0; i < 24; i++)
            {
                double a = i * Math.PI * 2 / 24;
                p.Fill(Sd.Circle((float)Math.Cos(a) * (r - 0.07f), (float)Math.Sin(a) * (r - 0.07f), 0.022f), V83BCoinDeep);
            }
            p.Shape(Sd.Circle(0f, 0f, r - 0.14f), Rgba.Hex("FFD966"), 0.025f);
            if (heads)
            {
                // re sorridente con la corona
                float fy = 0.08f;
                Body(p, Sd.Circle(0f, fy, 0.2f), Rgba.Hex("F7BE3A"), V83BCoinShade, null, -0.04f, 0.04f);
                p.Shape(Sd.Polygon(-0.19f, fy + 0.14f, -0.21f, fy + 0.36f, -0.1f, fy + 0.25f, 0f, fy + 0.4f, 0.1f, fy + 0.25f, 0.21f, fy + 0.36f, 0.19f, fy + 0.14f),
                        Rgba.Hex("FFE28A"), 0.025f);
                p.Shape(Sd.Circle(0f, fy + 0.4f, 0.035f), Rgba.Hex("E8433F"), 0.015f);
                foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Circle(s * 0.075f, fy + 0.03f, 0.028f), DoodlePainter.DefaultInk);
                p.InkLine(0.025f, -0.1f, fy - 0.06f, -0.05f, fy - 0.1f, 0.05f, fy - 0.1f, 0.1f, fy - 0.06f);
                // baffi regali
                p.InkLine(0.02f, -0.12f, fy - 0.035f, -0.04f, fy - 0.05f);
                p.InkLine(0.02f, 0.12f, fy - 0.035f, 0.04f, fy - 0.05f);
            }
            else
            {
                // coda arricciata e pelosa (proprio una "coda"): cerchi sempre più grossi lungo una spirale, punta bianca
                Rgba fur = Rgba.Hex("E59A2A");
                var puffs = new List<Sdf>();
                for (int i = 0; i <= 40; i++)
                {
                    float k = i / 40f;
                    double a = -2.3 + k * 4.9;                   // dalla base in basso a sinistra gira su e si arriccia verso il centro
                    float rad = 0.27f - 0.17f * k;
                    puffs.Add(Sd.Circle(0.02f + (float)Math.Cos(a) * rad, 0.15f + (float)Math.Sin(a) * rad, 0.03f + 0.07f * (float)Math.Sin(k * Math.PI * 0.85)));
                }
                p.Shape(Sd.Union(puffs.ToArray()), fur, 0.025f);
                double ta = -2.3 + 4.9;
                p.Shape(Sd.Circle(0.02f + (float)Math.Cos(ta) * 0.1f, 0.15f + (float)Math.Sin(ta) * 0.1f, 0.06f), Rgba.Hex("FFF3D6"), 0.022f);
            }
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.26f, 0.3f, 0.12f, 0.045f), -0.26f, 0.3f, 40f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt(heads ? "v83_b_coin_heads" : "v83_b_coin_tails", p);
        }

        /// <summary>La moneta vista di taglio (centro 0,0, larga quanto il diametro): una striscia dorata zigrinata.</summary>
        public static DoodleArt V83B_CoinEdge()
        {
            float r = V83BCoinR;
            var p = new DoodlePainter(-r - 0.05f, -0.1f, r + 0.05f, 0.1f, 160) { Ink = 0.035f };
            Body(p, Sd.Box(0f, 0f, r, 0.06f, 0.05f), V83BCoinGold, V83BCoinShade);
            for (int i = -9; i <= 9; i++) p.Line(V83BCoinDeep, 0.014f, i * 0.06f, -0.035f, i * 0.06f, 0.035f);
            return new DoodleArt("v83_b_coin_edge", p);
        }

        /// <summary>Il pulsante FLIP (centro 0,0, largo 2.4, alto 1): tasto rosso a pillola sulla base scura; la scritta è un testo.</summary>
        public static DoodleArt V83B_Button()
        {
            var p = new DoodlePainter(-1.28f, -0.6f, 1.28f, 0.56f, 110) { Ink = 0.05f };
            p.Shape(Sd.Box(0f, -0.14f, 1.2f, 0.42f, 0.4f), Rgba.Hex("7E1F24"));
            Body(p, Sd.Box(0f, 0.04f, 1.16f, 0.42f, 0.4f), Rgba.Hex("F2554F"), Rgba.Hex("C73A36"), null, -0.06f, 0.07f);
            p.Fill(Sd.Box(-0.55f, 0.32f, 0.34f, 0.045f, 0.04f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("v83_b_button", p);
        }

        /// <summary>La targhetta del punteggio (centro 0,0, larga 1.7): ottone con le viti; il punteggio è un testo.</summary>
        public static DoodleArt V83B_Plaque()
        {
            var p = new DoodlePainter(-0.9f, -0.36f, 0.9f, 0.36f, 120) { Ink = 0.035f };
            Body(p, Sd.Box(0f, 0f, 0.85f, 0.31f, 0.06f), Rgba.Hex("2B2233"), Rgba.Hex("1A1420"));
            p.Shape(Sd.Box(0f, 0f, 0.77f, 0.23f, 0.04f), Rgba.Hex("3A2F45"), 0.02f);
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Circle(s * 0.81f, 0f, 0.025f), Rgba.Hex("C9A55A"), 0.012f);
            return new DoodleArt("v83_b_plaque", p);
        }
    }
}
