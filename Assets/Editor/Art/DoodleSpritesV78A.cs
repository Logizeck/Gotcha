using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.78 per il livello "The Aristocratic Cat" (roadmap n. 13): lo sfondo (salotto elegante:
    /// carta da parati a righe con gigli, zoccolo bianco, parquet a spina, ritratto di un gatto con la corona, tappeto),
    /// l'armadio altissimo, il gatto snob bianco (monocolo d'oro, farfallino rosso) in sei pose — sdraiato tranquillo,
    /// sdraiato infastidito, aggrappato con le unghie, in caduta, seduto composto e mentre si liscia il pelo —, la scala
    /// di legno, la ciotola di porcellana, il topolino-giocattolo con la piuma e i graffi delle unghie. Il fumetto è il
    /// solito "bubble_box", la polvere "puff", la scintilla "sparkle_star". Prefisso v78_a_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV78A() => new DoodleArt[]
        {
            V78A_Bg(), V78A_Wardrobe(), V78A_CatLie(0), V78A_CatLie(1), V78A_CatLie(2), V78A_CatFall(), V78A_CatSit(false), V78A_CatSit(true),
            V78A_Ladder(), V78A_Bowl(), V78A_Toy(), V78A_Scratch(),
        };

        // ---- misure della scena (coordinate del mondo), usate anche dal builder ----
        public const float V78AFloorY = -2.55f;                       // attaccatura muro/pavimento
        public const float V78AWardLeft = 0.05f, V78AWardRight = 2.15f; // fianchi dell'armadio
        public const float V78AWardBottom = -3.05f, V78AWardTop = 0.9f; // piedini e piano in cima (dove sta il gatto)

        static readonly Rgba V78AFur = Rgba.Hex("F7F3EC"), V78AFurShade = Rgba.Hex("D9D0C4"), V78APink = Rgba.Hex("FFA9BC");
        static readonly Rgba V78AGold = Rgba.Hex("F2C44D"), V78ABow = Rgba.Hex("C8304E"), V78AEye = Rgba.Hex("6FB6E8");
        static readonly Rgba V78AWood = Rgba.Hex("8E5B3B"), V78AWoodShade = Rgba.Hex("6B4129"), V78AWoodLight = Rgba.Hex("A8714B");

        /// <summary>Sfondo del salotto (coordinate del mondo, più largo dello schermo per i telefoni lunghi).</summary>
        public static DoodleArt V78A_Bg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 40) { Ink = 0.05f, Wobble = 0.006f };
            // carta da parati verde salvia a righe, con piccoli gigli dorati
            Sdf wall = Sd.Box(0f, (7.1f + V78AFloorY) * 0.5f, 4.6f, (7.1f - V78AFloorY) * 0.5f);
            p.FillFunc(wall, (x, y) => Rgba.Lerp(Rgba.Hex("BFD6B6"), Rgba.Hex("9EC096"), Math.Max(0f, Math.Min(1f, (4f - y) / 7f))));
            for (float x = -4.4f; x < 4.6f; x += 0.7f)
                p.Fill(Sd.Box(x, 2f, 0.12f, 5.1f), Rgba.Hex("FFFFFF", 0.16f), false);
            for (float x = -4.05f; x < 4.6f; x += 0.7f)
                for (float y = -1.9f + ((int)Math.Round((x + 4.05f) / 0.7f) % 2) * 0.45f; y < 7f; y += 0.9f)
                {
                    Sdf lily = Sd.Union(Sd.Ellipse(x, y + 0.06f, 0.035f, 0.08f), Sd.Rotate(Sd.Ellipse(x - 0.06f, y + 0.02f, 0.03f, 0.06f), x - 0.06f, y + 0.02f, 35f),
                        Sd.Rotate(Sd.Ellipse(x + 0.06f, y + 0.02f, 0.03f, 0.06f), x + 0.06f, y + 0.02f, -35f), Sd.Box(x, y - 0.04f, 0.07f, 0.015f));
                    p.Fill(lily, Rgba.Hex("D9B95A", 0.55f));
                }
            // zoccolo bianco
            p.Shape(Sd.Box(0f, V78AFloorY + 0.14f, 4.6f, 0.15f), Rgba.Hex("FBF7EE"), 0.035f);
            p.Line(Rgba.Hex("D8CFBF"), 0.025f, -4.5f, V78AFloorY + 0.2f, 4.5f, V78AFloorY + 0.2f);
            // parquet a listelli sfalsati
            Sdf floor = Sd.Box(0f, (V78AFloorY - 7.1f) * 0.5f, 4.6f, (V78AFloorY + 7.1f) * 0.5f);
            p.FillFunc(floor, (x, y) => Rgba.Lerp(Rgba.Hex("D49A62"), Rgba.Hex("B87A45"), Math.Max(0f, Math.Min(1f, (V78AFloorY - y) / 3f))));
            int row = 0;
            for (float y = V78AFloorY - 0.35f; y > -7f; y -= 0.35f, row++)
            {
                p.Line(Rgba.Hex("8F5A2E", 0.55f), 0.02f, -4.5f, y, 4.5f, y);
                for (float x = -4.5f + (row % 2) * 0.6f; x < 4.6f; x += 1.2f)
                    p.Line(Rgba.Hex("8F5A2E", 0.5f), 0.02f, x, y, x, y + 0.35f);
            }
            // tappeto ovale viola con frangia (sotto gli oggetti)
            Sdf rug = Sd.Ellipse(-0.1f, -3.75f, 2.1f, 0.5f);
            p.Shape(rug, Rgba.Hex("7D4E9E"), 0.035f);
            p.Shape(Sd.Ellipse(-0.1f, -3.75f, 1.75f, 0.36f), Rgba.Hex("9A6BBC"), 0.025f);
            p.Line(Rgba.Hex("E8C766", 0.8f), 0.025f, -1.5f, -3.75f, -1.1f, -3.62f, -0.6f, -3.6f, -0.1f, -3.62f, 0.4f, -3.6f, 0.9f, -3.62f, 1.3f, -3.75f);
            // ritratto del gatto con la corona (cornice dorata)
            float px = -1.25f, py = 1.75f;
            p.Fill(Sd.Box(px + 0.06f, py - 0.07f, 0.52f, 0.64f, 0.05f), Rgba.Hex("2B2233", 0.22f), false);
            p.Shape(Sd.Box(px, py, 0.52f, 0.64f, 0.05f), V78AGold, 0.035f);
            Sdf canvas = Sd.Box(px, py, 0.38f, 0.5f, 0.02f);
            p.Shape(canvas, Rgba.Hex("5B3A6E"), 0.025f);
            p.Shape(Sd.Union(Sd.Ellipse(px, py - 0.32f, 0.28f, 0.2f), Sd.Circle(px, py + 0.02f, 0.2f),
                Sd.Polygon(px - 0.19f, py + 0.08f, px - 0.15f, py + 0.3f, px - 0.04f, py + 0.17f),
                Sd.Polygon(px + 0.19f, py + 0.08f, px + 0.15f, py + 0.3f, px + 0.04f, py + 0.17f)), V78AFur, 0.022f);
            p.Shape(Sd.Polygon(px - 0.12f, py + 0.2f, px - 0.12f, py + 0.36f, px - 0.06f, py + 0.27f, px, py + 0.38f, px + 0.06f, py + 0.27f, px + 0.12f, py + 0.36f, px + 0.12f, py + 0.2f), V78AGold, 0.02f);
            p.InkLine(0.018f, px - 0.1f, py + 0.03f, px - 0.04f, py + 0.03f);
            p.InkLine(0.018f, px + 0.04f, py + 0.03f, px + 0.1f, py + 0.03f);
            p.Shape(Sd.Polygon(px - 0.07f, py - 0.17f, px + 0.07f, py - 0.17f, px, py - 0.11f), V78ABow, 0.015f);
            return new DoodleArt("v78_a_bg", p);
        }

        /// <summary>L'armadio altissimo (coordinate del mondo): cornice in cima, due ante con pannelli, pomelli d'oro, piedini.</summary>
        public static DoodleArt V78A_Wardrobe()
        {
            float l = V78AWardLeft, r = V78AWardRight, b = V78AWardBottom, t = V78AWardTop;
            float cx = (l + r) * 0.5f, hx = (r - l) * 0.5f;
            var p = new DoodlePainter(l - 0.25f, b - 0.1f, r + 0.25f, t + 0.1f, 70) { Ink = 0.05f };
            float bodyTop = t - 0.2f, bodyBottom = b + 0.22f;
            // ombra sul muro
            p.Fill(Sd.Box(cx + 0.12f, (bodyTop + bodyBottom) * 0.5f - 0.05f, hx, (bodyTop - bodyBottom) * 0.5f), Rgba.Hex("2B2233", 0.2f), false);
            // piedini a cipolla
            foreach (float x in new[] { l + 0.25f, r - 0.25f })
                p.Shape(Sd.Union(Sd.Ellipse(x, b + 0.12f, 0.13f, 0.1f), Sd.Box(x, b + 0.2f, 0.09f, 0.05f)), V78AWoodShade, 0.035f);
            // corpo
            Sdf body = Sd.Box(cx, (bodyTop + bodyBottom) * 0.5f, hx - 0.05f, (bodyTop - bodyBottom) * 0.5f, 0.03f);
            Body(p, body, V78AWood, V78AWoodShade, null, 0.06f, 0.05f);
            // zoccolo
            p.Shape(Sd.Box(cx, bodyBottom + 0.1f, hx, 0.1f, 0.03f), V78AWoodShade, 0.035f);
            // ante con pannelli
            float doorTop = bodyTop - 0.12f, doorBottom = bodyBottom + 0.28f, dh = (doorTop - doorBottom) * 0.5f, dcy = (doorTop + doorBottom) * 0.5f;
            foreach (float s in new[] { -1f, 1f })
            {
                float dx = cx + s * (hx - 0.1f) * 0.5f;
                Sdf door = Sd.Box(dx, dcy, (hx - 0.18f) * 0.5f, dh, 0.03f);
                p.Shape(door, V78AWoodLight, 0.035f);
                p.Shape(Sd.Box(dx, dcy + dh * 0.45f, (hx - 0.18f) * 0.5f - 0.13f, dh * 0.4f, 0.06f), V78AWood, 0.028f);
                p.Shape(Sd.Box(dx, dcy - dh * 0.5f, (hx - 0.18f) * 0.5f - 0.13f, dh * 0.36f, 0.06f), V78AWood, 0.028f);
                // venature
                p.Line(Rgba.Hex("6B4129", 0.4f), 0.015f, dx - 0.15f, dcy + dh * 0.7f, dx - 0.1f, dcy + dh * 0.3f, dx - 0.16f, dcy + dh * 0.1f);
                p.Line(Rgba.Hex("6B4129", 0.4f), 0.015f, dx + 0.12f, dcy - dh * 0.2f, dx + 0.17f, dcy - dh * 0.55f, dx + 0.1f, dcy - dh * 0.8f);
                // pomello
                p.Shape(Sd.Circle(cx + s * 0.13f, dcy, 0.06f), V78AGold, 0.025f);
            }
            // cornice in cima (più larga) con modanatura
            p.Shape(Sd.Box(cx, t - 0.1f, hx + 0.1f, 0.1f, 0.03f), V78AWoodShade, 0.04f);
            p.Shape(Sd.Box(cx, bodyTop - 0.02f, hx + 0.02f, 0.05f, 0.02f), V78AWoodLight, 0.03f);
            p.Line(Rgba.Hex("C98F62", 0.8f), 0.02f, l - 0.03f, t - 0.04f, r + 0.03f, t - 0.04f);
            return new DoodleArt("v78_a_wardrobe", p);
        }

        /// <summary>Testa del gatto (guarda a sinistra, mento all'insù) con orecchie, muso, monocolo. mood 0 calmo, 1 seccato, 2 spaventato.</summary>
        static void V78ACatHead(DoodlePainter p, float hx, float hy, int mood, float tilt = 0f)
        {
            // orecchie (seccato: abbassate all'indietro; spaventato: dritte)
            float earDrop = mood == 1 ? 0.12f : 0f;
            Sdf earL = Sd.Polygon(hx - 0.3f, hy + 0.12f - earDrop * 0.5f, hx - 0.28f + (mood == 1 ? -0.12f : 0f), hy + 0.45f - earDrop, hx - 0.06f, hy + 0.27f);
            Sdf earR = Sd.Polygon(hx + 0.04f, hy + 0.3f, hx + 0.28f + (mood == 1 ? 0.12f : 0f), hy + 0.44f - earDrop, hx + 0.3f, hy + 0.1f - earDrop * 0.5f);
            p.Shape(Sd.Union(earL, earR), V78AFur);
            p.Fill(Sd.Grow(Sd.Union(earL, earR), -0.06f), V78APink);
            // testa a ciuffi (pelo lungo)
            Sdf head = Sd.Union(Sd.Ellipse(hx, hy, 0.36f, 0.32f), Sd.Polygon(hx - 0.36f, hy - 0.08f, hx - 0.46f, hy - 0.2f, hx - 0.28f, hy - 0.22f),
                Sd.Polygon(hx + 0.36f, hy - 0.08f, hx + 0.46f, hy - 0.2f, hx + 0.28f, hy - 0.22f));
            Body(p, head, V78AFur, V78AFurShade, null, -0.04f, 0.06f);
            // muso chiaro
            p.Fill(Sd.Ellipse(hx - 0.04f, hy - 0.13f, 0.17f, 0.11f), Rgba.White);
            // occhi
            float ex1 = hx - 0.16f, ex2 = hx + 0.1f, ey = hy + 0.04f;
            if (mood == 2)
            {
                foreach (float ex in new[] { ex1, ex2 })
                {
                    p.Shape(Sd.Circle(ex, ey, 0.085f), Rgba.White, 0.022f);
                    p.Fill(Sd.Circle(ex - 0.01f, ey, 0.028f), DoodlePainter.DefaultInk);
                }
            }
            else
            {
                // palpebre a mezz'asta (snob); seccato: fessure
                float open = mood == 1 ? 0.025f : 0.045f;
                foreach (float ex in new[] { ex1, ex2 })
                {
                    Sdf eye = Sd.Intersect(Sd.Ellipse(ex, ey, 0.075f, 0.065f), Sd.Box(ex, ey - 0.065f + open, 0.09f, open));
                    p.Fill(eye, V78AEye);
                    p.Fill(Sd.Intersect(Sd.Circle(ex - 0.015f, ey - 0.03f, 0.025f), Sd.Box(ex, ey - 0.065f + open, 0.09f, open)), DoodlePainter.DefaultInk);
                    p.InkLine(0.026f, ex - 0.085f, ey - 0.065f + open * 2f, ex + 0.085f, ey - 0.065f + open * 2f + (mood == 1 ? -0.02f : 0.01f));
                }
                // sopracciglio alzato sopra il monocolo
                p.InkLine(0.022f, ex2 - 0.06f, ey + 0.11f, ex2 + 0.02f, ey + 0.15f, ex2 + 0.09f, ey + 0.13f);
                if (mood == 1) p.InkLine(0.022f, ex1 - 0.08f, ey + 0.07f, ex1 + 0.06f, ey + 0.04f);
            }
            // monocolo d'oro sull'occhio destro (spaventato: è saltato via e pende dalla catenella)
            if (mood == 2)
            {
                p.Line(V78AGold, 0.015f, hx + 0.2f, hy - 0.05f, hx + 0.3f, hy - 0.3f, hx + 0.33f, hy - 0.42f);
                p.Shape(Sd.Circle(hx + 0.35f, hy - 0.5f, 0.08f), Rgba.Hex("D8EEFF", 0.5f), 0.03f);
                p.Shape(Sd.Subtract(Sd.Circle(hx + 0.35f, hy - 0.5f, 0.095f), Sd.Circle(hx + 0.35f, hy - 0.5f, 0.07f)), V78AGold, 0.015f);
            }
            else
            {
                p.Fill(Sd.Circle(ex2, ey, 0.105f), Rgba.Hex("D8EEFF", 0.35f));
                p.Shape(Sd.Subtract(Sd.Circle(ex2, ey, 0.12f), Sd.Circle(ex2, ey, 0.095f)), V78AGold, 0.018f);
                p.Fill(Sd.Rotate(Sd.Ellipse(ex2 - 0.04f, ey + 0.05f, 0.03f, 0.012f), ex2 - 0.04f, ey + 0.05f, 35f), Rgba.White.WithAlpha(0.9f));
                p.Line(V78AGold, 0.014f, ex2 + 0.11f, ey - 0.05f, ex2 + 0.2f, ey - 0.2f, ex2 + 0.22f, ey - 0.32f);
            }
            // naso e bocca
            p.Shape(Sd.Polygon(hx - 0.1f, hy - 0.07f, hx - 0.01f, hy - 0.07f, hx - 0.055f, hy - 0.12f), V78APink, 0.018f);
            if (mood == 2) p.Shape(Sd.Ellipse(hx - 0.055f, hy - 0.2f, 0.04f, 0.05f), Rgba.Hex("8A2E44"), 0.018f);
            else if (mood == 1) p.InkLine(0.018f, hx - 0.12f, hy - 0.2f, hx - 0.055f, hy - 0.17f, hx + 0.01f, hy - 0.2f);
            else p.InkLine(0.018f, hx - 0.11f, hy - 0.17f, hx - 0.055f, hy - 0.15f, hx, hy - 0.17f);
            // baffi
            foreach (float dy in new[] { -0.02f, 0.03f })
            {
                p.InkLine(0.011f, hx - 0.17f, hy - 0.12f + dy, hx - 0.5f, hy - 0.08f + dy * 2f);
                p.InkLine(0.011f, hx + 0.07f, hy - 0.12f + dy, hx + 0.4f, hy - 0.08f + dy * 2f);
            }
        }

        /// <summary>Farfallino rosso con il nodo (centro bx, by).</summary>
        static void V78ABowTie(DoodlePainter p, float bx, float by, float s = 1f)
        {
            p.Shape(Sd.Union(Sd.Polygon(bx, by, bx - 0.17f * s, by + 0.09f * s, bx - 0.17f * s, by - 0.09f * s),
                Sd.Polygon(bx, by, bx + 0.17f * s, by + 0.09f * s, bx + 0.17f * s, by - 0.09f * s)), V78ABow, 0.025f);
            p.Shape(Sd.Circle(bx, by, 0.045f * s), Rgba.Hex("E2526F"), 0.02f);
        }

        /// <summary>
        /// Il gatto sdraiato sul piano dell'armadio (y = 0 è il piano, guarda a sinistra, largo circa 1.9): pelo lungo
        /// bianco, coda che pende davanti all'armadio. mood 0 calmo (palpebre a mezz'asta), 1 seccato (orecchie giù,
        /// fessure), 2 aggrappato (schiacciato, unghie piantate sul bordo, occhi sbarrati, monocolo saltato, pelo ritto).
        /// </summary>
        public static DoodleArt V78A_CatLie(int mood)
        {
            var p = new DoodlePainter(-1.25f, -0.75f, 1.15f, 1.1f, 120) { Ink = 0.045f };
            bool cling = mood == 2;
            float bodyRy = cling ? 0.25f : 0.31f, bodyY = cling ? 0.24f : 0.3f;
            // coda che pende davanti (aggrappato: gonfia e a zigzag)
            Sdf tail = cling
                ? Sd.Polyline(0.13f, 0.8f, 0.22f, 0.98f, 0.05f, 0.9f, -0.12f, 1.0f, -0.3f, 0.9f, -0.48f, 0.98f, -0.6f)
                : Sd.Polyline(0.1f, 0.8f, 0.22f, 0.95f, 0.05f, 0.95f, -0.2f, 0.88f, -0.4f, 0.78f, -0.5f);
            Body(p, tail, V78AFur, V78AFurShade);
            // ombra sul piano
            p.Fill(Sd.Ellipse(0.05f, 0.02f, 0.95f, 0.05f), Rgba.Hex("2B2233", 0.2f), false);
            // corpo (pelo lungo: ciuffi sul bordo)
            Sdf body = Sd.Union(Sd.Ellipse(0.12f, bodyY, 0.82f, bodyRy),
                Sd.Polygon(-0.2f, bodyY + bodyRy - 0.04f, -0.08f, bodyY + bodyRy + (cling ? 0.16f : 0.07f), 0.05f, bodyY + bodyRy - 0.02f),
                Sd.Polygon(0.2f, bodyY + bodyRy - 0.03f, 0.33f, bodyY + bodyRy + (cling ? 0.17f : 0.06f), 0.45f, bodyY + bodyRy - 0.06f),
                Sd.Polygon(0.58f, bodyY + bodyRy - 0.1f, 0.72f, bodyY + bodyRy + (cling ? 0.1f : 0.0f), 0.82f, bodyY + 0.1f));
            Body(p, body, V78AFur, V78AFurShade, null, -0.06f, 0.07f);
            p.Line(V78AFurShade, 0.03f, 0.35f, bodyY + 0.12f, 0.45f, bodyY - 0.05f);
            p.Line(V78AFurShade, 0.03f, 0.55f, bodyY + 0.1f, 0.63f, bodyY - 0.07f);
            // zampa di dietro
            p.Shape(Sd.Ellipse(0.62f, 0.07f, 0.2f, 0.08f), V78AFur, 0.035f);
            // zampe davanti: incrociate con grazia, oppure tese e aggrappate al bordo con le unghie
            if (cling)
            {
                foreach (float y in new[] { 0.06f, -0.06f })
                {
                    p.Shape(Sd.Capsule(-0.55f, 0.16f, -1.0f, y, 0.075f), V78AFur, 0.035f);
                    for (int i = 0; i < 3; i++)
                        p.Shape(Sd.Polygon(-1.07f, y - 0.03f + i * 0.035f, -1.0f, y - 0.01f + i * 0.035f, -1.1f, y - 0.11f + i * 0.035f), Rgba.White, 0.012f);
                }
                // gocce di sudore
                p.Shape(Sd.Union(Sd.Circle(0.25f, 1.0f, 0.045f), Sd.Polygon(0.21f, 1.02f, 0.29f, 1.02f, 0.25f, 1.09f)), Rgba.Hex("9AD8FF"), 0.015f);
                p.Shape(Sd.Union(Sd.Circle(-0.95f, 0.98f, 0.035f), Sd.Polygon(-0.98f, 0.99f, -0.92f, 0.99f, -0.95f, 1.05f)), Rgba.Hex("9AD8FF"), 0.015f);
            }
            else
            {
                p.Shape(Sd.Ellipse(-0.5f, 0.06f, 0.2f, 0.07f), V78AFur, 0.035f);
                p.Shape(Sd.Rotate(Sd.Ellipse(-0.44f, 0.12f, 0.2f, 0.07f), -0.44f, 0.12f, -12f), V78AFur, 0.035f);
            }
            // farfallino e testa
            float hx = cling ? -0.68f : -0.6f, hy = cling ? 0.5f : 0.62f;
            V78ABowTie(p, hx + 0.08f, hy - 0.33f, 0.9f);
            V78ACatHead(p, hx, hy, mood);
            // segni di paura
            if (cling)
            {
                p.InkLine(0.022f, hx - 0.35f, hy + 0.42f, hx - 0.42f, hy + 0.52f);
                p.InkLine(0.022f, hx - 0.15f, hy + 0.47f, hx - 0.16f, hy + 0.6f);
                p.InkLine(0.022f, hx + 0.12f, hy + 0.47f, hx + 0.16f, hy + 0.58f);
            }
            string name = mood == 0 ? "v78_a_cat_lie" : mood == 1 ? "v78_a_cat_annoyed" : "v78_a_cat_cling";
            return new DoodleArt(name, p);
        }

        /// <summary>Il gatto in caduta (centro in 0, 0.5: si ruota attorno a lì), a zampe larghe, occhi sbarrati.</summary>
        public static DoodleArt V78A_CatFall()
        {
            var p = new DoodlePainter(-0.95f, -0.25f, 0.95f, 1.3f, 120) { Ink = 0.045f };
            // coda ritta e gonfia
            Body(p, Sd.Polyline(0.12f, 0.3f, 0.35f, 0.62f, 0.5f, 0.75f, 0.85f, 0.7f, 1.1f), V78AFur, V78AFurShade);
            // zampe larghe
            foreach (float[] l in new[] { new[] { -0.25f, 0.55f, -0.75f, 0.85f }, new[] { 0.25f, 0.55f, 0.75f, 0.85f }, new[] { -0.2f, 0.25f, -0.62f, -0.05f }, new[] { 0.2f, 0.25f, 0.62f, -0.05f } })
            {
                p.Shape(Sd.Capsule(l[0], l[1], l[2], l[3], 0.08f), V78AFur, 0.035f);
                p.Shape(Sd.Circle(l[2], l[3], 0.09f), V78AFur, 0.03f);
            }
            Sdf body = Sd.Union(Sd.Ellipse(0f, 0.4f, 0.36f, 0.42f), Sd.Polygon(-0.36f, 0.3f, -0.48f, 0.42f, -0.34f, 0.5f), Sd.Polygon(0.36f, 0.3f, 0.48f, 0.42f, 0.34f, 0.5f));
            Body(p, body, V78AFur, V78AFurShade);
            p.Fill(Sd.Ellipse(0f, 0.35f, 0.2f, 0.26f), Rgba.White);
            V78ABowTie(p, 0f, 0.72f, 0.85f);
            V78ACatHead(p, 0.04f, 0.98f, 2);
            return new DoodleArt("v78_a_cat_fall", p);
        }

        /// <summary>
        /// Il gatto seduto composto sul pavimento (y = 0 il pavimento, largo circa 1.3): mento in su, occhi chiusi,
        /// monocolo, farfallino, coda attorno alle zampe. groom: una zampa alzata che si liscia il pelo del petto.
        /// </summary>
        public static DoodleArt V78A_CatSit(bool groom)
        {
            var p = new DoodlePainter(-0.85f, -0.15f, 0.85f, 1.75f, 120) { Ink = 0.045f };
            p.Fill(Sd.Ellipse(0f, 0.02f, 0.6f, 0.07f), Rgba.Hex("2B2233", 0.2f), false);
            // coda avvolta
            Body(p, Sd.Polyline(0.1f, 0.35f, 0.12f, 0.6f, 0.15f, 0.55f, 0.08f, 0.2f, 0.07f, -0.15f, 0.08f), V78AFur, V78AFurShade);
            // corpo a pera con ciuffi
            Sdf body = Sd.Union(Sd.Ellipse(0f, 0.45f, 0.45f, 0.45f), Sd.Ellipse(0f, 0.85f, 0.3f, 0.3f),
                Sd.Polygon(-0.44f, 0.5f, -0.56f, 0.3f, -0.4f, 0.28f), Sd.Polygon(0.44f, 0.5f, 0.56f, 0.3f, 0.4f, 0.28f));
            Body(p, body, V78AFur, V78AFurShade);
            // petto a ciuffo (lisciato: con le linee del pelo pettinato)
            Sdf chest = Sd.Union(Sd.Ellipse(-0.04f, 0.62f, 0.22f, 0.3f), Sd.Polygon(-0.2f, 0.45f, -0.04f, 0.22f, 0.12f, 0.45f));
            p.Fill(chest, Rgba.White);
            p.Line(V78AFurShade, 0.02f, -0.1f, 0.75f, -0.08f, 0.5f);
            p.Line(V78AFurShade, 0.02f, 0.03f, 0.75f, 0.04f, 0.5f);
            // zampe davanti
            p.Shape(Sd.Ellipse(-0.2f, 0.06f, 0.13f, 0.08f), V78AFur, 0.035f);
            if (!groom) p.Shape(Sd.Ellipse(0.12f, 0.06f, 0.13f, 0.08f), V78AFur, 0.035f);
            V78ABowTie(p, -0.02f, 1.0f, 0.95f);
            // testa con il mento all'insù e gli occhi chiusi di soddisfazione
            float hx = -0.04f, hy = 1.3f;
            Sdf earL = Sd.Polygon(hx - 0.3f, hy + 0.12f, hx - 0.28f, hy + 0.46f, hx - 0.06f, hy + 0.27f);
            Sdf earR = Sd.Polygon(hx + 0.04f, hy + 0.29f, hx + 0.28f, hy + 0.46f, hx + 0.3f, hy + 0.1f);
            p.Shape(Sd.Union(earL, earR), V78AFur);
            p.Fill(Sd.Grow(Sd.Union(earL, earR), -0.06f), V78APink);
            Sdf head = Sd.Union(Sd.Ellipse(hx, hy, 0.36f, 0.32f), Sd.Polygon(hx - 0.36f, hy - 0.08f, hx - 0.46f, hy - 0.2f, hx - 0.28f, hy - 0.22f),
                Sd.Polygon(hx + 0.36f, hy - 0.08f, hx + 0.46f, hy - 0.2f, hx + 0.28f, hy - 0.22f));
            Body(p, head, V78AFur, V78AFurShade, null, -0.04f, 0.06f);
            p.Fill(Sd.Ellipse(hx - 0.02f, hy - 0.12f, 0.17f, 0.11f), Rgba.White);
            float ex1 = hx - 0.14f, ex2 = hx + 0.12f, ey = hy + 0.05f;
            p.InkLine(0.026f, ex1 - 0.07f, ey, ex1, ey + 0.04f, ex1 + 0.07f, ey);
            p.InkLine(0.026f, ex2 - 0.07f, ey, ex2, ey + 0.04f, ex2 + 0.07f, ey);
            p.Fill(Sd.Circle(ex2, ey, 0.105f), Rgba.Hex("D8EEFF", 0.35f));
            p.Shape(Sd.Subtract(Sd.Circle(ex2, ey, 0.12f), Sd.Circle(ex2, ey, 0.095f)), V78AGold, 0.018f);
            p.Fill(Sd.Rotate(Sd.Ellipse(ex2 - 0.04f, ey + 0.05f, 0.03f, 0.012f), ex2 - 0.04f, ey + 0.05f, 35f), Rgba.White.WithAlpha(0.9f));
            p.Line(V78AGold, 0.014f, ex2 + 0.11f, ey - 0.05f, ex2 + 0.2f, ey - 0.2f, ex2 + 0.18f, ey - 0.4f);
            p.Shape(Sd.Polygon(hx - 0.07f, hy - 0.06f, hx + 0.02f, hy - 0.06f, hx - 0.025f, hy - 0.11f), V78APink, 0.018f);
            p.InkLine(0.018f, hx - 0.09f, hy - 0.17f, hx - 0.025f, hy - 0.14f, hx + 0.04f, hy - 0.17f);
            foreach (float dy in new[] { -0.02f, 0.03f })
            {
                p.InkLine(0.011f, hx - 0.15f, hy - 0.11f + dy, hx - 0.48f, hy - 0.07f + dy * 2f);
                p.InkLine(0.011f, hx + 0.1f, hy - 0.11f + dy, hx + 0.43f, hy - 0.07f + dy * 2f);
            }
            if (groom)
            {
                // zampa alzata che liscia il petto, con un luccichio
                p.Shape(Sd.Capsule(0.18f, 0.25f, 0.06f, 0.66f, 0.085f), V78AFur, 0.035f);
                p.Shape(Sd.Ellipse(0.04f, 0.72f, 0.11f, 0.09f), V78AFur, 0.035f);
                p.Fill(Sd.Ellipse(0.03f, 0.7f, 0.04f, 0.03f), V78APink);
                p.Shape(Sd.Union(Sd.Box(0.42f, 0.95f, 0.012f, 0.08f), Sd.Box(0.42f, 0.95f, 0.08f, 0.012f)), Rgba.Hex("FFF3A8"), 0.012f);
            }
            string name = groom ? "v78_a_cat_groom" : "v78_a_cat_sit";
            return new DoodleArt(name, p);
        }

        /// <summary>La scala di legno (centro 0,0; alta 3.6, larga 0.7).</summary>
        public static DoodleArt V78A_Ladder()
        {
            var p = new DoodlePainter(-0.42f, -1.88f, 0.42f, 1.88f, 70) { Ink = 0.045f };
            for (float y = -1.45f; y < 1.7f; y += 0.42f)
                p.Shape(Sd.Box(0f, y, 0.28f, 0.045f, 0.02f), Rgba.Hex("D9A86A"), 0.03f);
            foreach (float x in new[] { -0.3f, 0.3f })
            {
                Sdf rail = Sd.Box(x, 0f, 0.065f, 1.8f, 0.04f);
                Body(p, rail, Rgba.Hex("C98F55"), Rgba.Hex("A5703D"));
            }
            return new DoodleArt("v78_a_ladder", p);
        }

        /// <summary>La ciotola di porcellana col bordo d'oro, piena di crocchette a pesciolino (centro 0,0).</summary>
        public static DoodleArt V78A_Bowl()
        {
            var p = new DoodlePainter(-0.55f, -0.32f, 0.55f, 0.36f, 130) { Ink = 0.04f };
            // crocchette
            foreach (float[] k in new[] { new[] { -0.22f, 0.1f }, new[] { 0.0f, 0.15f }, new[] { 0.2f, 0.1f }, new[] { -0.1f, 0.2f }, new[] { 0.12f, 0.22f } })
                p.Shape(Sd.Union(Sd.Ellipse(k[0], k[1], 0.08f, 0.05f), Sd.Polygon(k[0] + 0.06f, k[1], k[0] + 0.13f, k[1] + 0.05f, k[0] + 0.13f, k[1] - 0.05f)), Rgba.Hex("E39A5B"), 0.02f);
            Sdf bowl = Sd.Intersect(Sd.Ellipse(0f, 0.08f, 0.48f, 0.36f), Sd.Box(0f, -0.1f, 0.6f, 0.18f));
            Body(p, bowl, Rgba.White, Rgba.Hex("D8DDF0"));
            p.Shape(Sd.Box(0f, 0.08f, 0.49f, 0.035f, 0.03f), V78AGold, 0.025f);
            p.Shape(Sd.Box(0f, -0.26f, 0.25f, 0.035f, 0.02f), V78AGold, 0.025f);
            // coroncina blu sul davanti
            p.Shape(Sd.Polygon(-0.1f, -0.12f, -0.1f, 0f, -0.05f, -0.06f, 0f, 0.01f, 0.05f, -0.06f, 0.1f, 0f, 0.1f, -0.12f), Rgba.Hex("4D6BC8"), 0.018f);
            return new DoodleArt("v78_a_bowl", p);
        }

        /// <summary>Il topolino-giocattolo con la piuma per coda (centro 0,0, guarda a sinistra).</summary>
        public static DoodleArt V78A_Toy()
        {
            var p = new DoodlePainter(-0.45f, -0.25f, 0.62f, 0.38f, 140) { Ink = 0.04f };
            // piuma
            Sdf feather = Sd.Rotate(Sd.Ellipse(0.4f, 0.12f, 0.2f, 0.07f), 0.4f, 0.12f, 25f);
            p.Shape(feather, Rgba.Hex("3FB7C9"), 0.025f);
            p.InkLine(0.015f, 0.2f, 0.0f, 0.57f, 0.2f);
            p.Line(Rgba.Hex("2A8C9C"), 0.012f, 0.35f, 0.1f, 0.33f, 0.18f);
            p.Line(Rgba.Hex("2A8C9C"), 0.012f, 0.45f, 0.15f, 0.43f, 0.23f);
            // corpo grigio di stoffa con le cuciture
            Sdf body = Sd.Union(Sd.Ellipse(0f, -0.04f, 0.27f, 0.17f), Sd.Polygon(-0.24f, 0.06f, -0.42f, -0.08f, -0.22f, -0.15f));
            Body(p, body, Rgba.Hex("A7A3B5"), Rgba.Hex("807B91"));
            p.Line(Rgba.Hex("6D6880", 0.8f), 0.012f, -0.05f, 0.12f, -0.02f, 0.05f, -0.05f, -0.02f, -0.02f, -0.09f);
            p.Shape(Sd.Circle(-0.12f, 0.12f, 0.08f), Rgba.Hex("FFB3C3"), 0.025f);
            p.Fill(Sd.Circle(-0.27f, 0.0f, 0.025f), DoodlePainter.DefaultInk);
            p.Fill(Sd.Circle(-0.42f, -0.08f, 0.03f), Rgba.Hex("FF8FA3"));
            return new DoodleArt("v78_a_toy", p);
        }

        /// <summary>Tre graffi d'unghie bianchi sul legno (centro 0,0).</summary>
        public static DoodleArt V78A_Scratch()
        {
            var p = new DoodlePainter(-0.3f, -0.2f, 0.3f, 0.2f, 140) { Ink = 0.02f };
            for (int i = 0; i < 3; i++)
            {
                float x = -0.14f + i * 0.13f;
                p.Line(Rgba.Hex("4A2A18", 0.7f), 0.04f, x + 0.05f, 0.14f, x - 0.02f, -0.02f, x - 0.05f, -0.15f);
                p.Line(Rgba.Hex("F2D6B5"), 0.018f, x + 0.05f, 0.14f, x - 0.02f, -0.02f, x - 0.05f, -0.15f);
            }
            return new DoodleArt("v78_a_scratch", p);
        }
    }
}
