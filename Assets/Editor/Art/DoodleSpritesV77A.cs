using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.77 per il livello "Love the Bug" (roadmap n. 8): lo sfondo (scrivania-carta da parati a
    /// righine), la finestra di una app di fantasia ("MY APP v0.1", tutta a fumetto, nessuna interfaccia vera di sistema:
    /// barra del titolo con un cerottino, area a quadretti, crepa nell'angolo, la vaschetta vuota della barra di
    /// caricamento), l'icona-faccina rotta e quella riparata, l'insettino verde visto di lato (guarda a destra), le sue
    /// guance rosse da sovrapporre, e l'insetto diventato cuore. Il cuore del misuratore e i cuoricini sono il solito
    /// "heart", lo sbuffo dello sdoppiamento è "puff", la vampata finale "sparkle_star". Prefisso v77_a_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV77A() => new DoodleArt[]
        {
            V77A_Bg(), V77A_Window(), V77A_Smiley(false), V77A_Smiley(true), V77A_Bug(), V77A_BugCheeks(), V77A_BugHeart(),
        };

        static readonly Rgba V77AGreen = Rgba.Hex("7BD957"), V77AGreenShade = Rgba.Hex("4FA83A"), V77AShell = Rgba.Hex("5DC24A");

        /// <summary>La finestra (coordinate del mondo): bordi, barra del titolo e vaschetta della barra di caricamento.</summary>
        public const float V77AWinLeft = -2.4f, V77AWinRight = 2.4f, V77AWinBottom = -3.95f, V77AWinTop = 2.75f;
        public const float V77ATitleY = 2.45f;            // centro della barra del titolo
        public const float V77ABarY = -3.2f;              // centro della barra di caricamento
        public const float V77ABarHalfW = 1.75f, V77ABarHalfH = 0.13f;   // metà della vaschetta (interno)

        /// <summary>Sfondo: carta da parati lilla a righine diagonali (dietro la finestra). Coordinate del mondo.</summary>
        public static DoodleArt V77A_Bg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 40) { Ink = 0.05f, Wobble = 0.006f };
            Sdf all = Sd.Box(0f, 0f, 4.6f, 7.1f);
            p.FillFunc(all, (x, y) =>
            {
                float t = Math.Max(0f, Math.Min(1f, (3.5f - y) / 8f));
                return Rgba.Lerp(Rgba.Hex("E4DAF5"), Rgba.Hex("F6E6EF"), t);
            });
            p.Hatch(all, Rgba.Hex("FFFFFF", 0.45f), 0.42f, 0.05f, 35f);
            // qualche cuoricino-disegnino sbiadito sulla carta da parati (indizio scherzoso)
            float[][] dots = { new[] { -2.0f, 3.9f }, new[] { 2.0f, 4.2f }, new[] { -1.95f, -4.6f }, new[] { 1.9f, -4.4f } };
            foreach (float[] d in dots)
                p.Fill(Sd.Union(Sd.Circle(d[0] - 0.07f, d[1] + 0.05f, 0.09f), Sd.Circle(d[0] + 0.07f, d[1] + 0.05f, 0.09f),
                    Sd.Polygon(d[0] - 0.15f, d[1] + 0.02f, d[0] + 0.15f, d[1] + 0.02f, d[0], d[1] - 0.17f)), Rgba.Hex("D9A9C8", 0.45f));
            return new DoodleArt("v77_a_bg", p);
        }

        /// <summary>
        /// La finestra della app di fantasia (coordinate del mondo): cornice arrotondata, barra del titolo blu con un
        /// cerottino e un pulsante "?" disegnato a mano (a destra c'è il contatore BUGS come testo a parte), area bianca a
        /// quadretti, una crepa nell'angolo in basso a sinistra e la vaschetta vuota della barra di caricamento
        /// (il riempimento è una barra a parte, la scritta pure).
        /// </summary>
        public static DoodleArt V77A_Window()
        {
            var p = new DoodlePainter(V77AWinLeft - 0.12f, V77AWinBottom - 0.16f, V77AWinRight + 0.12f, V77AWinTop + 0.12f, 70) { Ink = 0.05f };
            float cx = (V77AWinLeft + V77AWinRight) * 0.5f, cy = (V77AWinBottom + V77AWinTop) * 0.5f;
            float hx = (V77AWinRight - V77AWinLeft) * 0.5f, hy = (V77AWinTop - V77AWinBottom) * 0.5f;
            // ombra
            p.Fill(Sd.Box(cx + 0.07f, cy - 0.09f, hx, hy, 0.18f), Rgba.Hex("2B2233", 0.25f), false);
            Sdf frame = Sd.Box(cx, cy, hx, hy, 0.18f);
            p.Shape(frame, Rgba.Hex("FFFDF7"));
            // area di lavoro a quadretti
            Sdf area = Sd.Box(cx, (V77AWinBottom + V77ATitleY - 0.3f) * 0.5f + 0.02f, hx - 0.14f, (V77ATitleY - 0.3f - V77AWinBottom) * 0.5f - 0.12f, 0.08f);
            p.Fill(area, Rgba.Hex("F4F8FF"), false);
            p.Hatch(area, Rgba.Hex("C9D8EE", 0.6f), 0.3f, 0.012f, 0f);
            p.Hatch(area, Rgba.Hex("C9D8EE", 0.6f), 0.3f, 0.012f, 90f);
            p.Shape(area, Rgba.Clear, 0.03f);
            // barra del titolo
            Sdf title = Sd.Intersect(frame, Sd.Box(cx, V77ATitleY + 0.05f, hx + 0.2f, 0.3f));
            p.Fill(title, Rgba.Hex("6C8CF5"), false);
            p.Fill(Sd.Intersect(title, Sd.Box(cx, V77ATitleY + 0.3f, hx + 0.2f, 0.06f)), Rgba.Hex("8FA9FF"), false);
            p.InkLine(0.04f, V77AWinLeft + 0.05f, V77ATitleY - 0.27f, V77AWinRight - 0.05f, V77ATitleY - 0.27f);
            // icona a sinistra: un ingranaggio storto (fumetto)
            float gx = V77AWinLeft + 0.32f;
            p.Shape(Sd.Union(Sd.Circle(gx, V77ATitleY, 0.13f), Sd.Box(gx, V77ATitleY, 0.17f, 0.04f), Sd.Box(gx, V77ATitleY, 0.04f, 0.17f),
                Sd.Rotate(Sd.Box(gx, V77ATitleY, 0.17f, 0.04f), gx, V77ATitleY, 45f), Sd.Rotate(Sd.Box(gx, V77ATitleY, 0.04f, 0.17f), gx, V77ATitleY, 45f)), Rgba.Hex("FFD45C"), 0.03f);
            p.Shape(Sd.Circle(gx, V77ATitleY, 0.05f), Rgba.Hex("6C8CF5"), 0.025f);
            // cerottino storto sulla barra del titolo
            float bx = V77AWinRight - 0.95f, by = V77ATitleY + 0.2f;
            p.Shape(Sd.Rotate(Sd.Box(bx, by, 0.24f, 0.075f, 0.06f), bx, by, -18f), Rgba.Hex("F5C9A0"), 0.025f);
            p.Fill(Sd.Rotate(Sd.Box(bx, by, 0.07f, 0.055f, 0.02f), bx, by, -18f), Rgba.Hex("E0A47A"));
            // pulsantino "?" disegnato (non un pulsante di sistema)
            float qx = V77AWinRight - 0.3f;
            p.Shape(Sd.Circle(qx, V77ATitleY, 0.15f), Rgba.Hex("FF8FA3"), 0.03f);
            p.InkLine(0.035f, qx - 0.05f, V77ATitleY + 0.05f, qx - 0.02f, V77ATitleY + 0.09f, qx + 0.04f, V77ATitleY + 0.08f, qx + 0.05f, V77ATitleY + 0.03f, qx, V77ATitleY - 0.01f, qx, V77ATitleY - 0.04f);
            p.Fill(Sd.Circle(qx, V77ATitleY - 0.1f, 0.022f), DoodlePainter.DefaultInk);
            // crepa nell'angolo
            p.InkLine(0.03f, V77AWinLeft + 0.14f, V77AWinBottom + 1.15f, V77AWinLeft + 0.36f, V77AWinBottom + 0.95f, V77AWinLeft + 0.3f, V77AWinBottom + 0.75f, V77AWinLeft + 0.55f, V77AWinBottom + 0.55f);
            p.InkLine(0.022f, V77AWinLeft + 0.3f, V77AWinBottom + 0.75f, V77AWinLeft + 0.18f, V77AWinBottom + 0.6f);
            // vaschetta della barra di caricamento
            Sdf trough = Sd.Box(0f, V77ABarY, V77ABarHalfW + 0.04f, V77ABarHalfH + 0.04f, 0.1f);
            p.Shape(trough, Rgba.Hex("E1E4EC"), 0.04f);
            return new DoodleArt("v77_a_window", p);
        }

        /// <summary>
        /// L'icona-faccina della app (centro 0,0, raggio 0.7): rotta (spaccata, un occhio a X e uno a spirale, bocca
        /// storta, cerotto) oppure riparata (occhi felici, sorrisone, guance).
        /// </summary>
        public static DoodleArt V77A_Smiley(bool happy)
        {
            var p = new DoodlePainter(-0.85f, -0.85f, 0.85f, 0.85f, 140) { Ink = 0.045f };
            Sdf face = Sd.Circle(0f, 0f, 0.7f);
            Body(p, face, happy ? Rgba.Hex("FFE066") : Rgba.Hex("E9D27A"), happy ? Rgba.Hex("F2B93B") : Rgba.Hex("C9AE55"));
            if (happy)
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    // occhi felici ad arco
                    p.InkLine(0.06f, s * 0.36f, 0.16f, s * 0.26f, 0.27f, s * 0.14f, 0.16f);
                    p.Fill(Sd.Ellipse(s * 0.42f, -0.06f, 0.11f, 0.065f), Rgba.Hex("FF7A9A", 0.7f));
                }
                Sdf mouth = Sd.Intersect(Sd.Circle(0f, -0.05f, 0.38f), Sd.Box(0f, -0.3f, 0.5f, 0.24f));
                p.Shape(mouth, Rgba.Hex("7A2335"), 0.04f);
                p.Fill(Sd.Ellipse(0f, -0.37f, 0.15f, 0.06f), Rgba.Hex("FF8FA3"));
                p.Fill(Sd.Rotate(Sd.Ellipse(-0.36f, 0.42f, 0.13f, 0.06f), -0.36f, 0.42f, 35f), Rgba.White.WithAlpha(0.7f));
                return new DoodleArt("v77_a_smiley_happy", p);
            }
            // crepa che attraversa la faccia
            p.InkLine(0.035f, 0.05f, 0.7f, -0.04f, 0.45f, 0.08f, 0.28f, -0.02f, 0.05f, 0.06f, -0.12f);
            // occhio a X
            p.InkLine(0.05f, -0.4f, 0.32f, -0.2f, 0.12f);
            p.InkLine(0.05f, -0.4f, 0.12f, -0.2f, 0.32f);
            // occhio a spirale
            p.InkLine(0.035f, 0.3f, 0.22f, 0.34f, 0.22f, 0.34f, 0.26f, 0.28f, 0.27f, 0.26f, 0.19f, 0.34f, 0.15f, 0.41f, 0.22f, 0.36f, 0.32f, 0.23f, 0.31f);
            // bocca storta a zigzag
            p.InkLine(0.045f, -0.32f, -0.3f, -0.18f, -0.22f, -0.04f, -0.33f, 0.1f, -0.24f, 0.24f, -0.36f, 0.34f, -0.3f);
            // cerotto sulla guancia
            p.Shape(Sd.Rotate(Sd.Box(0.4f, -0.08f, 0.17f, 0.06f, 0.05f), 0.4f, -0.08f, 30f), Rgba.Hex("F5C9A0"), 0.025f);
            return new DoodleArt("v77_a_smiley_broken", p);
        }

        /// <summary>
        /// L'insettino verde visto di lato, guarda a destra (centro del corpo in 0,0; largo circa 1.0): corpo tondo con
        /// guscio a puntini, testa con un occhione e un sorrisetto, antenne a ricciolo, sei zampette.
        /// </summary>
        public static DoodleArt V77A_Bug()
        {
            var p = new DoodlePainter(-0.5f, -0.4f, 0.62f, 0.62f, 170) { Ink = 0.035f };
            // zampette (dietro al corpo)
            foreach (float x in new[] { -0.22f, 0f, 0.2f })
                p.InkLine(0.035f, x, -0.12f, x - 0.06f, -0.3f, x - 0.01f, -0.33f);
            // ombra morbida
            p.Fill(Sd.Ellipse(0f, -0.35f, 0.38f, 0.045f), Rgba.Hex("2B2233", 0.18f), false);
            // antenne a ricciolo
            p.InkLine(0.028f, 0.3f, 0.2f, 0.33f, 0.36f, 0.42f, 0.47f);
            p.InkLine(0.028f, 0.38f, 0.18f, 0.48f, 0.3f, 0.55f, 0.36f);
            p.Shape(Sd.Circle(0.43f, 0.49f, 0.045f), Rgba.Hex("FF8FA3"), 0.022f);
            p.Shape(Sd.Circle(0.56f, 0.38f, 0.04f), Rgba.Hex("FF8FA3"), 0.022f);
            // corpo e guscio
            Sdf body = Sd.Ellipse(-0.06f, -0.02f, 0.36f, 0.27f);
            Body(p, body, V77AGreen, V77AGreenShade);
            Sdf shell = Sd.Intersect(Sd.Ellipse(-0.08f, 0.0f, 0.33f, 0.25f), Sd.Box(-0.08f, 0.1f, 0.4f, 0.2f));
            p.Shape(shell, V77AShell, 0.03f);
            p.InkLine(0.025f, -0.08f, 0.24f, -0.06f, -0.08f);
            foreach (float[] d in new[] { new[] { -0.25f, 0.1f }, new[] { 0.06f, 0.14f }, new[] { -0.16f, -0.02f } })
                p.Fill(Sd.Circle(d[0], d[1], 0.045f), Rgba.Hex("3C8F2E"));
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.2f, 0.17f, 0.09f, 0.035f), -0.2f, 0.17f, 20f), Rgba.White.WithAlpha(0.55f));
            // testa
            Sdf head = Sd.Circle(0.3f, 0.06f, 0.18f);
            Body(p, head, V77AGreen, V77AGreenShade, null, -0.04f, 0.05f);
            // occhione
            p.Shape(Sd.Ellipse(0.35f, 0.11f, 0.075f, 0.09f), Rgba.White, 0.022f);
            p.Fill(Sd.Circle(0.37f, 0.1f, 0.045f), DoodlePainter.DefaultInk);
            p.Fill(Sd.Circle(0.355f, 0.125f, 0.017f), Rgba.White);
            // sorrisetto
            p.InkLine(0.022f, 0.33f, -0.03f, 0.38f, -0.06f, 0.44f, -0.03f);
            return new DoodleArt("v77_a_bug", p);
        }

        /// <summary>La guancia rossa dell'insetto (stesse coordinate di v77_a_bug, sulla testa) e un cuoricino sopra.</summary>
        public static DoodleArt V77A_BugCheeks()
        {
            var p = new DoodlePainter(-0.5f, -0.4f, 0.62f, 0.62f, 170) { Ink = 0.02f };
            Sdf cheek = Sd.Ellipse(0.27f, 0.0f, 0.075f, 0.045f);
            p.Fill(cheek, Rgba.Hex("FF6F94", 0.85f));
            p.Hatch(cheek, Rgba.Hex("E2456E", 0.7f), 0.04f, 0.012f, 60f);
            // tutto il corpo un po' rosato (velatura)
            p.Fill(Sd.Union(Sd.Ellipse(-0.06f, -0.02f, 0.34f, 0.25f), Sd.Circle(0.3f, 0.06f, 0.16f)), Rgba.Hex("FF8FB0", 0.25f));
            return new DoodleArt("v77_a_bug_cheeks", p);
        }

        /// <summary>L'insetto diventato cuore (centro 0,0, largo circa 1.0): cuore rosa con antenne, occhi felici, guance.</summary>
        public static DoodleArt V77A_BugHeart()
        {
            var p = new DoodlePainter(-0.6f, -0.6f, 0.6f, 0.75f, 150) { Ink = 0.045f };
            // antenne
            p.InkLine(0.035f, -0.12f, 0.3f, -0.18f, 0.5f, -0.3f, 0.6f);
            p.InkLine(0.035f, 0.12f, 0.3f, 0.18f, 0.5f, 0.3f, 0.6f);
            p.Shape(Sd.Circle(-0.32f, 0.62f, 0.06f), Rgba.Hex("7BD957"), 0.025f);
            p.Shape(Sd.Circle(0.32f, 0.62f, 0.06f), Rgba.Hex("7BD957"), 0.025f);
            Sdf heart = Sd.Union(Sd.Circle(-0.22f, 0.12f, 0.27f), Sd.Circle(0.22f, 0.12f, 0.27f),
                Sd.Polygon(0f, -0.55f, 0.47f, 0.02f, 0.22f, 0.12f, -0.22f, 0.12f, -0.47f, 0.02f));
            Body(p, heart, Rgba.Hex("FF5C8A"), Rgba.Hex("D93D6B"), null, -0.05f, 0.06f);
            foreach (float s in new[] { -1f, 1f })
            {
                p.InkLine(0.04f, s * 0.24f, 0.1f, s * 0.17f, 0.17f, s * 0.1f, 0.1f);
                p.Fill(Sd.Ellipse(s * 0.28f, -0.03f, 0.07f, 0.04f), Rgba.Hex("FFC2D3", 0.85f));
            }
            p.InkLine(0.035f, -0.08f, -0.06f, 0f, -0.12f, 0.08f, -0.06f);
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.27f, 0.25f, 0.08f, 0.04f), -0.27f, 0.25f, 35f), Rgba.White.WithAlpha(0.75f));
            return new DoodleArt("v77_a_bug_heart", p);
        }
    }
}
