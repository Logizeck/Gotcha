using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.68 per il livello 263 "The Stuck Snack" (il resto riusa gli sprite esistenti: il
    /// corridoio, la moneta, il biscotto, la caramella, la mela, la bottiglietta, il tasto): il distributore (corpo
    /// intero e la sua parte bassa da rimettere davanti alle barrette che cadono), il vetro, la spirale, la barretta di
    /// cioccolato, il sacchetto di patatine, lo sportello, l'adesivo "non scuotere", la falena, la mano con la moneta e il
    /// braccio che si infila nello sportello. Il distributore è disegnato già nelle coordinate del mondo (origine = 0,0).
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV68A() => new DoodleArt[]
        {
            V68_Vending(false), V68_Vending(true), V68_Glass(), V68_Coil(), V68_Choco(), V68_Chips(), V68_Flap(),
            V68_Sticker(), V68_Moth(), V68_Palm(), V68_Arm(),
        };

        // misure del distributore (unità del mondo; il costruttore del livello usa gli stessi numeri)
        const float V68A_MachineX = -0.35f, V68A_MachineHalfW = 1.85f, V68A_MachineBottom = -3.15f, V68A_MachineTop = 2.8f;
        const float V68A_WinLeft = -2.0f, V68A_WinRight = 0.62f, V68A_WinBottom = -0.75f, V68A_WinTop = 2.2f;
        /// <summary>Sotto questa quota la parte bassa del distributore copre le barrette che cadono.</summary>
        const float V68A_LowTop = -0.84f;

        /// <summary>
        /// Il distributore di snack visto di fronte: cassa rossa con l'insegna gialla, la vetrina buia con tre ripiani
        /// (cartellini e bottoncini sotto ogni scomparto), il pannello a destra (display, fessura della moneta, vaschetta
        /// del resto), l'apertura dello sportello e lo zoccolo. lowOnly: solo la parte sotto la vetrina (stesso disegno,
        /// ritagliato), da mettere davanti alle barrette che cadono.
        /// </summary>
        public static DoodleArt V68_Vending(bool lowOnly)
        {
            var p = new DoodlePainter(-2.3f, -3.35f, 1.6f, lowOnly ? V68A_LowTop : 2.95f, 100) { Ink = 0.05f };
            Rgba ink = Rgba.Hex("2B2233");
            // ombra e piedini
            p.Fill(Sd.Ellipse(V68A_MachineX, -3.2f, 2.05f, 0.13f), new Rgba(0f, 0f, 0f, 0.22f));
            foreach (float fx in new[] { -1.95f, 1.25f }) p.Shape(Sd.Box(fx, -3.18f, 0.16f, 0.08f, 0.03f), Rgba.Hex("3A3442"), 0.03f);
            // la cassa
            float cy = (V68A_MachineBottom + V68A_MachineTop) * 0.5f, hy = (V68A_MachineTop - V68A_MachineBottom) * 0.5f;
            Body(p, Sd.Box(V68A_MachineX, cy, V68A_MachineHalfW, hy, 0.12f), Rgba.Hex("E2483D"), Rgba.Hex("B8332B"));
            p.Line(Rgba.Hex("F07A6E"), 0.04f, -2.08f, 2.4f, -2.08f, -2.8f);
            // l'insegna
            p.Shape(Sd.Box(V68A_MachineX, 2.53f, 1.7f, 0.2f, 0.07f), Rgba.Hex("F2C14E"), 0.04f);
            // la vetrina (interno buio con una luce dall'alto)
            float wx = (V68A_WinLeft + V68A_WinRight) * 0.5f, wy = (V68A_WinBottom + V68A_WinTop) * 0.5f;
            float whx = (V68A_WinRight - V68A_WinLeft) * 0.5f, why = (V68A_WinTop - V68A_WinBottom) * 0.5f;
            p.Shape(Sd.Box(wx, wy, whx + 0.05f, why + 0.05f, 0.06f), Rgba.Hex("9AA3B5"), 0.045f);
            p.FillFunc(Sd.Box(wx, wy, whx, why, 0.03f), (x, y) => Rgba.Lerp(Rgba.Hex("232838"), Rgba.Hex("3D4660"), (y - V68A_WinBottom) / (V68A_WinTop - V68A_WinBottom)));
            // i tre ripiani con i cartellini (A1..C3) e il bottoncino di ogni scomparto
            float rowH = (V68A_WinTop - V68A_WinBottom) / 3f, colW = (V68A_WinRight - V68A_WinLeft) / 3f;
            for (int r = 0; r < 3; r++)
            {
                float by = V68A_WinTop - rowH * (r + 1);
                p.Shape(Sd.Box(wx, by + 0.08f, whx - 0.02f, 0.07f, 0.02f), Rgba.Hex("B9C0CD"), 0.025f);
                for (int c = 0; c < 3; c++)
                {
                    float cx = V68A_WinLeft + colW * (c + 0.5f);
                    p.Shape(Sd.Box(cx - 0.13f, by + 0.08f, 0.17f, 0.05f, 0.015f), Rgba.Hex("FBF8EF"), 0.015f);
                    p.Shape(Sd.Circle(cx + 0.24f, by + 0.08f, 0.05f), Rgba.Hex("6CC3E0"), 0.02f);
                }
            }
            // il pannello a destra
            p.Shape(Sd.Box(1.07f, wy, 0.33f, why + 0.05f, 0.06f), Rgba.Hex("C9CED8"), 0.045f);
            p.Shape(Sd.Box(1.07f, 1.95f, 0.27f, 0.17f, 0.04f), Rgba.Hex("123B2A"), 0.035f);
            // fessura della moneta
            Body(p, Sd.Box(1.07f, 0.62f, 0.22f, 0.3f, 0.06f), Rgba.Hex("E3E7EE"), Rgba.Hex("B7BFCC"));
            p.Fill(Sd.Box(1.07f, 0.6f, 0.035f, 0.16f, 0.02f), ink);
            p.Fill(Sd.Polygon(1.0f, 0.88f, 1.14f, 0.88f, 1.07f, 0.8f), Rgba.Hex("F2C14E"));
            // vaschetta del resto
            p.Shape(Sd.Box(1.07f, -0.36f, 0.22f, 0.18f, 0.07f), Rgba.Hex("8D95A5"), 0.035f);
            p.Fill(Sd.Box(1.07f, -0.39f, 0.15f, 0.1f, 0.05f), Rgba.Hex("1E1B26"));
            // l'apertura dello sportello e lo zoccolo
            p.Shape(Sd.Box(-0.7f, -1.9f, 1.22f, 0.56f, 0.08f), Rgba.Hex("9AA3B5"), 0.045f);
            p.Fill(Sd.Box(-0.7f, -1.9f, 1.13f, 0.48f, 0.05f), Rgba.Hex("1E1B26"));
            p.Shape(Sd.Box(V68A_MachineX, -2.93f, 1.72f, 0.14f, 0.05f), Rgba.Hex("8F2A23"), 0.035f);
            for (int i = 0; i < 9; i++) p.Line(Rgba.Hex("6E1F1A"), 0.025f, -1.85f + i * 0.38f, -2.98f, -1.75f + i * 0.38f, -2.88f);
            return new DoodleArt(lowOnly ? "v68_vending_low" : "v68_vending", p);
        }

        /// <summary>Il vetro della vetrina: azzurrino trasparente con due riflessi in diagonale (stesse coordinate del distributore).</summary>
        public static DoodleArt V68_Glass()
        {
            var p = new DoodlePainter(V68A_WinLeft, V68A_WinBottom, V68A_WinRight, V68A_WinTop, 80) { Ink = 0.03f };
            float wx = (V68A_WinLeft + V68A_WinRight) * 0.5f, wy = (V68A_WinBottom + V68A_WinTop) * 0.5f;
            Sdf win = Sd.Box(wx, wy, (V68A_WinRight - V68A_WinLeft) * 0.5f, (V68A_WinTop - V68A_WinBottom) * 0.5f, 0.03f);
            p.Fill(win, new Rgba(0.75f, 0.9f, 1f, 0.1f), false);
            p.Fill(Sd.Intersect(win, Sd.Rotate(Sd.Box(-1.25f, 1.25f, 0.13f, 2.2f), -1.25f, 1.25f, -32f)), new Rgba(1f, 1f, 1f, 0.16f), false);
            p.Fill(Sd.Intersect(win, Sd.Rotate(Sd.Box(-0.8f, 1.15f, 0.04f, 2.2f), -0.8f, 1.15f, -32f)), new Rgba(1f, 1f, 1f, 0.2f), false);
            p.Fill(Sd.Intersect(win, Sd.Rotate(Sd.Box(0.25f, -0.2f, 0.06f, 1.2f), 0.25f, -0.2f, -32f)), new Rgba(1f, 1f, 1f, 0.12f), false);
            return new DoodleArt("v68_glass", p);
        }

        /// <summary>La spirale vista di fronte (centro 0,0; larga 0.8): una fila di anelli di filo metallico.</summary>
        public static DoodleArt V68_Coil()
        {
            var p = new DoodlePainter(-0.44f, -0.16f, 0.44f, 0.16f, 160) { Ink = 0.02f };
            for (int i = 0; i < 6; i++)
            {
                float x = -0.34f + i * 0.136f;
                Sdf ring = Sd.Rotate(Sd.Subtract(Sd.Ellipse(x, 0f, 0.075f, 0.12f), Sd.Ellipse(x, 0f, 0.05f, 0.095f)), x, 0f, -22f);
                p.Shape(ring, Rgba.Hex("C7CEDA"));
            }
            return new DoodleArt("v68_coil", p);
        }

        /// <summary>La barretta di cioccolato (centro 0,0; 0.64 x 0.32): incarto viola con i bordi seghettati e la fascia gialla.</summary>
        public static DoodleArt V68_Choco()
        {
            var p = new DoodlePainter(-0.36f, -0.2f, 0.36f, 0.2f, 160) { Ink = 0.03f };
            // il poligono gira: lato sinistro dal basso in alto, lato destro dall'alto in basso
            var poly = new List<float>();
            for (int i = 0; i <= 6; i++) { poly.Add(-0.33f + (i % 2) * 0.03f); poly.Add(-0.15f + i * 0.05f); }
            for (int i = 6; i >= 0; i--) { poly.Add(0.33f - (i % 2) * 0.03f); poly.Add(-0.15f + i * 0.05f); }
            Body(p, Sd.Polygon(poly.ToArray()), Rgba.Hex("7A3FA0"), Rgba.Hex("5B2C7A"));
            p.Fill(Sd.Box(0.02f, 0f, 0.16f, 0.15f), Rgba.Hex("F2C14E"));
            p.Line(Rgba.Hex("2B2233"), 0.02f, -0.14f, 0.15f, -0.14f, -0.15f);
            p.Line(Rgba.Hex("2B2233"), 0.02f, 0.18f, 0.15f, 0.18f, -0.15f);
            p.Shape(Sd.Polygon(0.02f, 0.1f, 0.045f, 0.03f, 0.11f, 0.03f, 0.06f, -0.01f, 0.08f, -0.08f, 0.02f, -0.04f, -0.04f, -0.08f, -0.02f, -0.01f, -0.07f, 0.03f, -0.005f, 0.03f), Rgba.Hex("E2483D"), 0.015f);
            p.Line(Rgba.Hex("A77BC4"), 0.025f, -0.27f, 0.09f, -0.2f, 0.09f);
            return new DoodleArt("v68_choco", p);
        }

        /// <summary>Sacchetto di patatine (centro 0,0; 0.5 x 0.66) con la patatina disegnata sopra.</summary>
        public static DoodleArt V68_Chips()
        {
            var p = new DoodlePainter(-0.32f, -0.38f, 0.32f, 0.38f, 160) { Ink = 0.03f };
            var poly = new List<float>();
            for (int i = 0; i <= 8; i++) { poly.Add(-0.24f + i * 0.06f); poly.Add(-0.33f + (i % 2) * 0.03f); }
            poly.AddRange(new[] { 0.26f, -0.1f, 0.24f, 0.29f });
            for (int i = 8; i >= 0; i--) { poly.Add(-0.24f + i * 0.06f); poly.Add(0.33f - (i % 2) * 0.03f); }
            poly.AddRange(new[] { -0.26f, -0.1f });
            Body(p, Sd.Polygon(poly.ToArray()), Rgba.Hex("F28C28"), Rgba.Hex("C96A16"));
            p.Shape(Sd.Ellipse(0f, -0.02f, 0.14f, 0.11f), Rgba.Hex("F7D774"), 0.02f);
            p.Fill(Sd.Circle(-0.04f, 0f, 0.015f), Rgba.Hex("D9A441"));
            p.Fill(Sd.Circle(0.05f, -0.04f, 0.015f), Rgba.Hex("D9A441"));
            p.Fill(Sd.Box(0f, 0.19f, 0.17f, 0.04f, 0.02f), Rgba.Hex("E2483D"));
            return new DoodleArt("v68_chips", p);
        }

        /// <summary>Lo sportello di ritiro (cerniera in 0,0 in alto; pende fino a -0.9).</summary>
        public static DoodleArt V68_Flap()
        {
            var p = new DoodlePainter(-1.18f, -0.98f, 1.18f, 0.06f, 100) { Ink = 0.04f };
            Body(p, Sd.Box(0f, -0.46f, 1.1f, 0.44f, 0.05f), Rgba.Hex("4A4F5C"), Rgba.Hex("383C47"));
            p.Line(Rgba.Hex("6B7182"), 0.04f, -1.0f, -0.1f, 1.0f, -0.1f);
            p.Shape(Sd.Box(0f, -0.02f, 1.12f, 0.04f, 0.02f), Rgba.Hex("9AA3B5"), 0.025f);
            return new DoodleArt("v68_flap", p);
        }

        /// <summary>Adesivo giallo e nero "attenzione" (centro 0,0; 0.6 x 0.64) con il triangolo e lo spazio per la scritta.</summary>
        public static DoodleArt V68_Sticker()
        {
            var p = new DoodlePainter(-0.34f, -0.36f, 0.34f, 0.36f, 160) { Ink = 0.025f };
            Sdf card = Sd.Box(0f, 0f, 0.3f, 0.32f, 0.04f);
            p.Fill(card, Rgba.Hex("F7D23A"));
            p.Hatch(Sd.Subtract(card, Sd.Box(0f, 0f, 0.25f, 0.27f, 0.02f)), Rgba.Hex("2B2233"), 0.08f, 0.035f, 45f);
            p.Shape(card, Rgba.Clear);
            p.Shape(Sd.Polygon(-0.1f, 0.06f, 0.1f, 0.06f, 0f, 0.23f), Rgba.Hex("2B2233"), 0.015f);
            p.Fill(Sd.Capsule(0f, 0.17f, 0f, 0.12f, 0.012f), Rgba.Hex("F7D23A"));
            p.Fill(Sd.Circle(0f, 0.09f, 0.013f), Rgba.Hex("F7D23A"));
            return new DoodleArt("v68_sticker", p);
        }

        /// <summary>Una falena grigia con le ali aperte e le antenne (centro 0,0).</summary>
        public static DoodleArt V68_Moth()
        {
            var p = new DoodlePainter(-0.32f, -0.22f, 0.32f, 0.26f, 160) { Ink = 0.025f };
            p.InkLine(0.015f, -0.02f, 0.1f, -0.08f, 0.21f);
            p.InkLine(0.015f, 0.02f, 0.1f, 0.08f, 0.21f);
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Rotate(Sd.Ellipse(s * 0.15f, 0.04f, 0.15f, 0.1f), s * 0.15f, 0.04f, s * 15f), Rgba.Hex("B8AE9C"), Rgba.Hex("968B78"));
                Body(p, Sd.Rotate(Sd.Ellipse(s * 0.11f, -0.1f, 0.09f, 0.07f), s * 0.11f, -0.1f, -s * 20f), Rgba.Hex("A79C88"), Rgba.Hex("8A7F6C"));
                p.Fill(Sd.Circle(s * 0.16f, 0.05f, 0.03f), Rgba.Hex("7C705E"));
            }
            Body(p, Sd.Ellipse(0f, 0f, 0.04f, 0.13f), Rgba.Hex("7C705E"), Rgba.Hex("5E5446"));
            return new DoodleArt("v68_moth", p);
        }

        /// <summary>La mano del giocatore, palmo in su, che entra da destra (la moneta va appoggiata in 0,0).</summary>
        public static DoodleArt V68_Palm()
        {
            var p = new DoodlePainter(-0.6f, -0.5f, 1.0f, 0.3f, 140) { Ink = 0.035f };
            Rgba skin = Rgba.Hex("F2C6A0"), shade = Rgba.Hex("D9A47F");
            Body(p, Sd.Rotate(Sd.Box(0.62f, -0.24f, 0.38f, 0.17f, 0.05f), 0.62f, -0.24f, 12f), Rgba.Hex("5B8DEF"), Rgba.Hex("3F6CC4"));
            Body(p, Sd.Rotate(Sd.Box(0.32f, -0.2f, 0.06f, 0.2f, 0.03f), 0.32f, -0.2f, 12f), Rgba.Hex("EDEAE3"), Rgba.Hex("C9C3B6"));
            Sdf fingers = Sd.Union(Sd.Capsule(-0.2f, -0.12f, -0.5f, -0.02f, 0.06f), Sd.Capsule(-0.2f, -0.2f, -0.52f, -0.13f, 0.06f),
                Sd.Capsule(-0.2f, -0.27f, -0.48f, -0.24f, 0.055f));
            Body(p, Sd.Union(Sd.Ellipse(0.02f, -0.18f, 0.28f, 0.15f), fingers), skin, shade);
            p.Line(shade, 0.02f, -0.3f, -0.1f, -0.42f, -0.06f);
            Body(p, Sd.Capsule(0.12f, -0.08f, -0.04f, 0.04f, 0.06f), skin, shade);
            return new DoodleArt("v68_palm", p);
        }

        /// <summary>Braccio che sale dal basso con la mano aperta in cima (centro della mano in 0,0; la manica scende fino a -3.6).</summary>
        public static DoodleArt V68_Arm()
        {
            var p = new DoodlePainter(-0.36f, -3.65f, 0.36f, 0.48f, 120) { Ink = 0.035f };
            Rgba skin = Rgba.Hex("F2C6A0"), shade = Rgba.Hex("D9A47F");
            Body(p, Sd.Box(0f, -2.02f, 0.2f, 1.57f, 0.05f), Rgba.Hex("5B8DEF"), Rgba.Hex("3F6CC4"));
            Body(p, Sd.Box(0f, -0.42f, 0.22f, 0.07f, 0.03f), Rgba.Hex("EDEAE3"), Rgba.Hex("C9C3B6"));
            Body(p, Sd.Box(0f, -0.2f, 0.13f, 0.17f, 0.05f), skin, shade);
            Sdf hand = Sd.Union(Sd.Box(0f, 0.02f, 0.17f, 0.13f, 0.08f),
                Sd.Capsule(-0.12f, 0.08f, -0.13f, 0.3f, 0.045f), Sd.Capsule(-0.04f, 0.1f, -0.04f, 0.38f, 0.045f),
                Sd.Capsule(0.04f, 0.1f, 0.05f, 0.37f, 0.045f), Sd.Capsule(0.12f, 0.08f, 0.13f, 0.3f, 0.045f),
                Sd.Capsule(-0.14f, -0.04f, -0.29f, 0.12f, 0.05f));
            Body(p, hand, skin, shade);
            return new DoodleArt("v68_arm", p);
        }
    }
}
