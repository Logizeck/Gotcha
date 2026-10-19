using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli rifatti nella versione 0.40 (gruppo 3).</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtRw3() => new DoodleArt[]
        {
            HockeyTableWide(),
            FoldPaperCenter(), FoldFlapLeft(), FoldFlapRight(), FoldFlapTop(),
            FoldBackLeft(), FoldBackRight(), FoldBackTop(), FoldXGlow(), FoldButton(),
            PuzzleToni(), PuzzleToniMouth(), LickSmear(),
            MirrorLegend(), CtrlArrow(), SwapIcon(),
        };

        // ---------- Air hockey: porta del gatto più larga ----------

        /// <summary>Tavolo da air hockey (5.2×7.4, centro 0,0) come hockey_table, ma la porta in alto (del gatto) è larga 2.3.</summary>
        public static DoodleArt HockeyTableWide()
        {
            const float hx = 2.5f, hy = 3.6f;
            var p = new DoodlePainter(-hx - 0.2f, -hy - 0.2f, hx + 0.2f, hy + 0.2f, 50) { Ink = 0.06f, Wobble = 0.004f };
            p.Shape(Sd.Box(0, 0, hx + 0.16f, hy + 0.16f, 0.35f), Rgba.Hex("3A6FD8"));
            p.FillFunc(Sd.Box(0, 0, hx, hy, 0.25f), (x, y) => Rgba.Lerp(Rgba.Hex("F2FAFF"), Rgba.Hex("D8EEFA"), (float)Math.Abs(y) / hy));
            for (float x = -2.2f; x <= 2.21f; x += 0.4f)
                for (float y = -3.3f; y <= 3.31f; y += 0.4f)
                    p.Fill(Sd.Circle(x, y, 0.018f), Rgba.Hex("A9C6DA"));
            Rgba red = Rgba.Hex("E5484D"), blue = Rgba.Hex("3A6FD8");
            p.Line(red, 0.05f, -hx, 0, hx, 0);
            p.Fill(Sd.Subtract(Sd.Circle(0, 0, 0.62f), Sd.Circle(0, 0, 0.57f)), red);
            p.Fill(Sd.Circle(0, 0, 0.08f), red);
            foreach (float s in new[] { -1f, 1f })
            {
                float mouth = s > 0 ? 1.15f : 0.75f, crease = s > 0 ? 1.45f : 1.05f;
                p.Fill(Sd.Intersect(Sd.Subtract(Sd.Circle(0, s * hy, crease), Sd.Circle(0, s * hy, crease - 0.05f)), Sd.Box(0, 0, hx, hy)), blue);
                p.Line(blue, 0.04f, -hx, s * 1.9f, hx, s * 1.9f);
                // porta: fessura scura nel bordo
                p.Shape(Sd.Box(0, s * (hy + 0.08f), mouth, 0.14f, 0.06f), Rgba.Hex("1D1A33"), 0.04f);
            }
            return new DoodleArt("hockey_table_wide", p);
        }

        // ---------- Fold It: la mappa del tesoro da piegare ----------
        // Coordinate comuni a tutti i pezzi: il pannello centrale va da -1.25 a 1.25 (celle da 1.25).
        // Linee di piega: x = -1.25 (lembo sinistro), x = 1.25 (lembo destro), y = 1.25 (lembo in alto).
        // I retri sono disegnati già "atterrati" sul pannello centrale (il builder li specchia).

        const float FoldC = 1.25f;
        static readonly Rgba MapPaper = Rgba.Hex("F6E7C1"), MapPaperShade = Rgba.Hex("E9D3A2"), MapBack = Rgba.Hex("EAD9B0");
        static readonly Rgba MapPath = Rgba.Hex("B5402B"), MapSea = Rgba.Hex("8FCBE0"), MapSeaDeep = Rgba.Hex("5FA9C9");
        static readonly Rgba MapFold = Rgba.Hex("8A6A3A");

        /// <summary>Puntini del sentiero lungo una spezzata (passo costante).</summary>
        static void MapDots(DoodlePainter p, Rgba color, params float[] xy)
        {
            const float step = 0.17f;
            float carry = 0f;
            for (int i = 0; i + 3 < xy.Length; i += 2)
            {
                float ax = xy[i], ay = xy[i + 1], bx = xy[i + 2], by = xy[i + 3];
                float len = (float)Math.Sqrt((bx - ax) * (bx - ax) + (by - ay) * (by - ay));
                for (float d = carry; d <= len; d += step)
                {
                    float t = d / len;
                    p.Fill(Sd.Circle(ax + (bx - ax) * t, ay + (by - ay) * t, 0.05f), color);
                    carry = d + step - len;
                }
            }
        }

        /// <summary>Carta con contorno a inchiostro solo sui lati "liberi" (non sulle pieghe).</summary>
        static void MapSheet(DoodlePainter p, float x0, float y0, float x1, float y1, Rgba fill, bool inkL, bool inkR, bool inkB, bool inkT, bool back)
        {
            Sdf sheet = Sd.Box((x0 + x1) * 0.5f, (y0 + y1) * 0.5f, (x1 - x0) * 0.5f, (y1 - y0) * 0.5f);
            p.Fill(sheet, fill, false);
            p.Hatch(sheet, back ? Rgba.Hex("DCC79A", 0.6f) : Rgba.Hex("EFDDB0", 0.7f), back ? 0.12f : 0.35f, back ? 0.012f : 0.06f, back ? 45f : 20f);
            // macchie da mappa vecchia
            p.Fill(Sd.Intersect(Sd.Circle(x0 + 0.15f, y0 + 0.2f, 0.35f), sheet), MapPaperShade.WithAlpha(0.6f));
            p.Fill(Sd.Intersect(Sd.Circle(x1 - 0.1f, y1 - 0.15f, 0.25f), sheet), MapPaperShade.WithAlpha(0.5f));
            float w = 0.06f;
            if (inkL) p.InkLine(w, x0, y0, x0, y1);
            if (inkR) p.InkLine(w, x1, y0, x1, y1);
            if (inkB) p.InkLine(w, x0, y0, x1, y0);
            if (inkT) p.InkLine(w, x0, y1, x1, y1);
        }

        static void FoldDashes(DoodlePainter p, float ax, float ay, float bx, float by)
        {
            float len = (float)Math.Sqrt((bx - ax) * (bx - ax) + (by - ay) * (by - ay));
            for (float d = 0.04f; d < len; d += 0.2f)
            {
                float t0 = d / len, t1 = Math.Min(1f, (d + 0.11f) / len);
                p.Line(MapFold, 0.035f, ax + (bx - ax) * t0, ay + (by - ay) * t0, ax + (bx - ax) * t1, ay + (by - ay) * t1);
            }
        }

        static void Palm(DoodlePainter p, float x, float y, float s)
        {
            Body(p, Sd.Ellipse(x, y - 0.12f * s, 0.32f * s, 0.12f * s), Rgba.Hex("F2D48A"), Rgba.Hex("D9B566"));
            p.InkLine(0.04f * s, x, y - 0.1f * s, x + 0.04f * s, y + 0.18f * s, x + 0.02f * s, y + 0.35f * s);
            for (int i = 0; i < 4; i++)
                p.Shape(Sd.Rotate(Sd.Ellipse(x + 0.02f * s, y + 0.35f * s + 0.12f * s, 0.06f * s, 0.16f * s), x + 0.02f * s, y + 0.35f * s, -70f + i * 47f), i % 2 == 0 ? Leaf : LeafShade, 0.025f);
        }

        static void Skull(DoodlePainter p, float x, float y, float s)
        {
            p.Shape(Sd.Union(Sd.Circle(x, y + 0.04f * s, 0.15f * s), Sd.Box(x, y - 0.1f * s, 0.08f * s, 0.06f * s, 0.02f)), Rgba.Hex("FFFDF5"), 0.025f);
            foreach (float sx in new[] { -1f, 1f }) p.Fill(Sd.Circle(x + sx * 0.06f * s, y + 0.04f * s, 0.04f * s), Ink);
        }

        static void Waves(DoodlePainter p, float x, float y)
        {
            p.Line(MapSeaDeep, 0.03f, x - 0.18f, y, x - 0.09f, y + 0.05f, x, y, x + 0.09f, y + 0.05f, x + 0.18f, y);
        }

        static void BigX(DoodlePainter p, float x, float y, float r, Rgba color)
        {
            p.Shape(Sd.Union(Sd.Capsule(x - r, y - r, x + r, y + r, r * 0.28f), Sd.Capsule(x - r, y + r, x + r, y - r, r * 0.28f)), color, 0.03f);
        }

        /// <summary>Pannello centrale (fronte): la partenza (bandierina) in basso a sinistra, il sentiero si ferma al bordo.</summary>
        public static DoodleArt FoldPaperCenter()
        {
            float c = FoldC;
            var p = new DoodlePainter(-c - 0.06f, -c - 0.06f, c + 0.06f, c + 0.06f, 90) { Ink = 0.05f, Wobble = 0.003f };
            MapSheet(p, -c, -c, c, c, MapPaper, false, false, true, false, false);
            // in basso a sinistra: partenza e sentiero fino al bordo della cella
            Body(p, Sd.Ellipse(-0.85f, -1.0f, 0.22f, 0.08f), Rgba.Hex("C9B07A"), Rgba.Hex("AE9460"));
            p.InkLine(0.035f, -0.9f, -1.0f, -0.9f, -0.55f);
            p.Shape(Sd.Polygon(-0.9f, -0.55f, -0.6f, -0.65f, -0.9f, -0.75f), Rgba.Hex("E8433F"), 0.025f);
            MapDots(p, MapPath, -0.75f, -0.95f, -0.45f, -0.9f, -0.3f, -0.7f, -0.15f, -0.63f, 0f, -0.625f);
            // in basso a destra: un lago e un teschio (vicolo cieco)
            Body(p, Sd.Ellipse(0.65f, -0.75f, 0.42f, 0.25f), MapSea, MapSeaDeep);
            Waves(p, 0.6f, -0.75f);
            Skull(p, 0.75f, -0.25f, 1f);
            // in alto: montagne e un'isoletta
            foreach (float x in new[] { 0.35f, 0.75f })
                Body(p, Sd.Polygon(x - 0.28f, 0.35f, x, 0.85f, x + 0.28f, 0.35f), Rgba.Hex("B8B0A0"), Rgba.Hex("948B7A"));
            Palm(p, -0.6f, 0.55f, 1f);
            // pieghe tratteggiate su tre lati
            FoldDashes(p, -c, -c, -c, c);
            FoldDashes(p, c, -c, c, c);
            FoldDashes(p, -c, c, c, c);
            return new DoodleArt("fold_paper_center", p);
        }

        /// <summary>Lembo sinistro (fronte): una X rossa bella grossa... ma è un tranello.</summary>
        public static DoodleArt FoldFlapLeft()
        {
            float c = FoldC;
            var p = new DoodlePainter(-2 * c - 0.06f, -c - 0.06f, -c + 0.02f, c + 0.06f, 90) { Ink = 0.05f, Wobble = 0.003f };
            MapSheet(p, -2 * c, -c, -c, c, MapPaper, true, false, true, true, false);
            Body(p, Sd.Ellipse(-1.9f, -0.5f, 0.45f, 0.4f), MapSea, MapSeaDeep);
            Waves(p, -1.95f, -0.45f);
            Waves(p, -1.8f, -0.65f);
            BigX(p, -1.85f, 0.55f, 0.22f, Rgba.Hex("E8433F"));
            return new DoodleArt("fold_flap_left", p);
        }

        /// <summary>Lembo destro (fronte): mare con una barchetta.</summary>
        public static DoodleArt FoldFlapRight()
        {
            float c = FoldC;
            var p = new DoodlePainter(c - 0.02f, -c - 0.06f, 2 * c + 0.06f, c + 0.06f, 90) { Ink = 0.05f, Wobble = 0.003f };
            MapSheet(p, c, -c, 2 * c, c, MapPaper, false, true, true, true, false);
            Body(p, Sd.Box(1.875f, 0f, 0.5f, 1.0f, 0.3f), MapSea, MapSeaDeep);
            Waves(p, 1.7f, -0.6f);
            Waves(p, 2.0f, 0.5f);
            Body(p, Sd.Polygon(1.65f, 0.0f, 2.1f, 0.0f, 2.0f, -0.15f, 1.75f, -0.15f), Wood, WoodShade);
            p.InkLine(0.03f, 1.88f, 0.0f, 1.88f, 0.4f);
            p.Shape(Sd.Polygon(1.9f, 0.38f, 2.1f, 0.12f, 1.9f, 0.12f), Rgba.White, 0.02f);
            return new DoodleArt("fold_flap_right", p);
        }

        /// <summary>Lembo in alto (fronte): la rosa dei venti.</summary>
        public static DoodleArt FoldFlapTop()
        {
            float c = FoldC;
            var p = new DoodlePainter(-c - 0.06f, c - 0.02f, c + 0.06f, 2 * c + 0.06f, 90) { Ink = 0.05f, Wobble = 0.003f };
            MapSheet(p, -c, c, c, 2 * c, MapPaper, true, true, false, true, false);
            float x = 0f, y = 1.875f;
            p.Shape(Sd.Polygon(x, y + 0.5f, x + 0.1f, y, x, y - 0.5f, x - 0.1f, y), Rgba.Hex("E8433F"), 0.025f);
            p.Shape(Sd.Polygon(x - 0.5f, y, x, y + 0.1f, x + 0.5f, y, x, y - 0.1f), Rgba.Hex("FFFDF5"), 0.025f);
            p.Shape(Sd.Circle(x, y, 0.06f), Gold, 0.02f);
            Palm(p, -0.85f, 1.65f, 0.8f);
            Waves(p, 0.85f, 1.6f);
            Waves(p, 0.75f, 2.2f);
            return new DoodleArt("fold_flap_top", p);
        }

        /// <summary>Retro del lembo sinistro, già piegato sulla colonna sinistra: un serpente marino copre la partenza.</summary>
        public static DoodleArt FoldBackLeft()
        {
            float c = FoldC;
            var p = new DoodlePainter(-c - 0.06f, -c - 0.06f, 0.02f, c + 0.06f, 90) { Ink = 0.05f, Wobble = 0.003f };
            MapSheet(p, -c, -c, 0f, c, MapBack, true, true, true, true, true);
            Body(p, Sd.Box(-0.62f, 0f, 0.45f, 1.0f, 0.3f), MapSea, MapSeaDeep);
            Rgba snake = Rgba.Hex("6CC56B"), snakeShade = Rgba.Hex("3E9A35");
            foreach (float y in new[] { -0.7f, -0.2f, 0.3f })
                Body(p, Sd.Subtract(Sd.Circle(-0.62f, y, 0.2f), Sd.Box(-0.62f, y - 0.15f, 0.3f, 0.15f)), snake, snakeShade);
            Body(p, Sd.Ellipse(-0.55f, 0.75f, 0.24f, 0.16f), snake, snakeShade);
            p.Fill(Sd.Circle(-0.5f, 0.8f, 0.04f), Ink);
            Skull(p, -0.85f, -1.0f, 0.8f);
            return new DoodleArt("fold_back_left", p);
        }

        /// <summary>Retro del lembo destro, già piegato sulla colonna destra: il sentiero sale dalla partenza verso l'alto.</summary>
        public static DoodleArt FoldBackRight()
        {
            float c = FoldC;
            var p = new DoodlePainter(-0.02f, -c - 0.06f, c + 0.06f, c + 0.06f, 90) { Ink = 0.05f, Wobble = 0.003f };
            MapSheet(p, 0f, -c, c, c, MapBack, true, true, true, true, true);
            // sotto: da ovest a nord (si aggancia al sentiero della partenza)
            MapDots(p, MapPath, 0f, -0.625f, 0.25f, -0.62f, 0.5f, -0.5f, 0.62f, -0.3f, 0.625f, 0f);
            // sopra: prosegue dritto verso il bordo (dove arriverà la X del lembo in alto)
            MapDots(p, MapPath, 0.625f, 0f, 0.55f, 0.35f, 0.7f, 0.75f, 0.625f, 1.2f);
            p.Shape(Sd.Ellipse(0.25f, 0.6f, 0.12f, 0.08f), Rgba.Hex("B8B0A0"), 0.02f);
            Body(p, Sd.Ellipse(0.95f, -1.0f, 0.2f, 0.12f), MapSea, MapSeaDeep);
            return new DoodleArt("fold_back_right", p);
        }

        /// <summary>Retro del lembo in alto, già piegato sulla riga in alto: la X del tesoro (a destra) e un'isola.</summary>
        public static DoodleArt FoldBackTop()
        {
            float c = FoldC;
            var p = new DoodlePainter(-c - 0.06f, -0.02f, c + 0.06f, c + 0.06f, 90) { Ink = 0.05f, Wobble = 0.003f };
            MapSheet(p, -c, 0f, c, c, MapBack, true, true, true, true, true);
            MapDots(p, MapPath, 0.625f, 0f, 0.58f, 0.2f, 0.62f, 0.45f);
            BigX(p, 0.65f, 0.72f, 0.2f, Rgba.Hex("E8433F"));
            Body(p, Sd.Ellipse(-0.6f, 0.55f, 0.5f, 0.35f), MapSea, MapSeaDeep);
            Palm(p, -0.6f, 0.55f, 0.8f);
            return new DoodleArt("fold_back_top", p);
        }

        /// <summary>La X che si accende (alone dorato, centro 0,0).</summary>
        public static DoodleArt FoldXGlow()
        {
            var p = new DoodlePainter(-0.7f, -0.7f, 0.7f, 0.7f, 90) { Ink = 0.04f };
            p.FillFunc(Sd.Circle(0, 0, 0.68f), (x, y) => Rgba.Hex("FFE14A", 0.75f * (1f - (float)Math.Sqrt(x * x + y * y) / 0.68f)));
            for (int i = 0; i < 8; i++)
            {
                double a = i * Math.PI / 4;
                p.Line(Rgba.Hex("FFC93C"), 0.04f, (float)Math.Cos(a) * 0.38f, (float)Math.Sin(a) * 0.38f, (float)Math.Cos(a) * 0.58f, (float)Math.Sin(a) * 0.58f);
            }
            BigX(p, 0f, 0f, 0.22f, Gold);
            return new DoodleArt("fold_x_glow", p);
        }

        /// <summary>Pulsante largo (2.0×0.75, centro 0,0) per "UNFOLD".</summary>
        public static DoodleArt FoldButton()
        {
            var p = new DoodlePainter(-1.08f, -0.45f, 1.08f, 0.45f, 100) { Ink = 0.06f };
            Body(p, Sd.Box(0, 0, 1.0f, 0.37f, 0.3f), Rgba.Hex("FFC93C"), Rgba.Hex("E39B1B"));
            return new DoodleArt("fold_button", p);
        }

        // ---------- Slide puzzle: il ritratto di Toni ----------

        /// <summary>Ritratto quadrato di Toni, carlino nero (3.6 x 3.6) da tagliare a tasselli. La bocca è in 0, -0.72.</summary>
        public static DoodleArt PuzzleToni()
        {
            var p = new DoodlePainter(-1.8f, -1.8f, 1.8f, 1.8f, 90) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 1.8f, 1.8f), (x, y) => Rgba.Lerp(Rgba.Hex("FFE08A"), Rgba.Hex("9FE3C8"), Clamp((x - y + 3.6f) / 7.2f)));
            // cuoricini e osso sullo sfondo: ogni tassello ha qualcosa di riconoscibile
            p.Shape(Sd.Union(Sd.Circle(-1.38f, 1.42f, 0.13f), Sd.Circle(-1.18f, 1.42f, 0.13f), Sd.Polygon(-1.5f, 1.36f, -1.06f, 1.36f, -1.28f, 1.1f)), Rgba.Hex("FF7F9C"), 0.03f);
            p.Shape(Sd.Union(Sd.Capsule(1.0f, 1.4f, 1.5f, 1.4f, 0.07f), Sd.Circle(0.98f, 1.47f, 0.08f), Sd.Circle(0.98f, 1.33f, 0.08f), Sd.Circle(1.52f, 1.47f, 0.08f), Sd.Circle(1.52f, 1.33f, 0.08f)), Rgba.Hex("FFFDF5"), 0.03f);
            Body(p, Sd.Box(0, -1.55f, 1.8f, 0.3f), Rgba.Hex("5B8BD9"), Rgba.Hex("4270BD"));   // cuscino
            // orecchie
            foreach (float sx in new[] { -1f, 1f })
                Body(p, Sd.Rotate(Sd.Ellipse(sx * 1.08f, 0.95f, 0.42f, 0.27f), sx * 1.08f, 0.95f, sx * -30f), PugShade, Rgba.Hex("141418"));
            Sdf head = Sd.Ellipse(0, 0.05f, 1.42f, 1.2f);
            Body(p, head, PugFawn, PugShade, null, -0.18f, 0.2f);
            p.Line(PugLine, 0.05f, -0.48f, 0.9f, -0.24f, 1.0f, 0f, 0.93f, 0.24f, 1.0f, 0.48f, 0.9f);
            p.Line(PugLine, 0.045f, -0.3f, 0.72f, 0f, 0.79f, 0.3f, 0.72f);
            foreach (float sx in new[] { -1f, 1f })
            {
                p.Shape(Sd.Circle(sx * 0.57f, 0.3f, 0.35f), Rgba.Hex("2B211E"), 0.035f);
                p.Fill(Sd.Circle(sx * 0.57f, 0.3f, 0.3f), Rgba.Hex("FFFDF5"));
                p.Fill(Sd.Circle(sx * 0.54f, 0.36f, 0.19f), Rgba.Hex("2A1A12"));
                p.Fill(Sd.Circle(sx * 0.48f, 0.45f, 0.065f), Rgba.White);
            }
            Body(p, Sd.Ellipse(0, -0.45f, 0.8f, 0.58f), PugMask, PugShade, null, -0.08f, 0.1f);
            p.Shape(Sd.Ellipse(0, -0.18f, 0.29f, 0.18f), Rgba.Hex("120D0C"), 0.03f);
            p.Fill(Sd.Ellipse(-0.08f, -0.13f, 0.08f, 0.04f), Rgba.White.WithAlpha(0.55f));
            p.Line(PugLine, 0.04f, 0f, -0.36f, 0f, -0.55f);
            p.Line(PugLine, 0.04f, -0.35f, -0.62f, -0.15f, -0.72f, 0f, -0.6f, 0.15f, -0.72f, 0.35f, -0.62f);   // sorrisetto
            // collare rosso con medaglietta
            p.Shape(Sd.Box(0, -1.08f, 0.6f, 0.09f, 0.045f), Rgba.Hex("E8433F"), 0.025f);
            p.Shape(Sd.Circle(0, -1.22f, 0.1f), Gold, 0.02f);
            p.Shape(Sd.Box(0, 0, 1.78f, 1.78f), Rgba.Clear, 0.06f);
            return new DoodleArt("puzzle_toni", p);
        }

        /// <summary>Bocca aperta del ritratto di Toni (centro 0,0): si sovrappone al sorrisetto quando lecca.</summary>
        public static DoodleArt PuzzleToniMouth()
        {
            var p = new DoodlePainter(-0.5f, -0.32f, 0.5f, 0.32f, 120) { Ink = 0.04f };
            p.Shape(Sd.Ellipse(0, 0, 0.42f, 0.25f), Rgba.Hex("5A1424"));
            p.Fill(Sd.Ellipse(0, -0.1f, 0.25f, 0.1f), Rgba.Hex("FF7A93"));
            return new DoodleArt("puzzle_toni_mouth", p);
        }

        /// <summary>Strisciata di bava sul vetro (centro 0,0, larga 1.4): sfuma via.</summary>
        public static DoodleArt LickSmear()
        {
            var p = new DoodlePainter(-0.8f, -0.4f, 0.8f, 0.4f, 90) { Ink = 0.02f, Wobble = 0.02f };
            Sdf smear = Sd.Union(Sd.Capsule(-0.55f, 0f, 0.55f, 0.02f, 0.24f), Sd.Circle(0.6f, -0.12f, 0.1f), Sd.Circle(-0.3f, -0.2f, 0.08f));
            p.Fill(smear, Rgba.Hex("DFF3FF", 0.7f));
            p.Fill(Sd.Capsule(-0.4f, 0.1f, 0.35f, 0.12f, 0.05f), Rgba.White.WithAlpha(0.75f));
            p.Shape(Sd.Circle(0.15f, -0.05f, 0.05f), Rgba.White.WithAlpha(0.6f), 0.012f);
            p.Shape(Sd.Circle(-0.15f, 0.0f, 0.035f), Rgba.White.WithAlpha(0.6f), 0.01f);
            return new DoodleArt("lick_smear", p);
        }

        // ---------- Mirror Path: legenda dei comandi invertiti ----------

        /// <summary>Pannello della legenda (4.0×2.1, centro 0,0): carta bianca con bordo.</summary>
        public static DoodleArt MirrorLegend()
        {
            var p = new DoodlePainter(-2.08f, -1.13f, 2.08f, 1.13f, 80) { Ink = 0.06f };
            Body(p, Sd.Box(0, 0, 1.98f, 1.03f, 0.22f), Rgba.Hex("FFFDF5"), Rgba.Hex("E9E2D2"));
            p.Line(Rgba.Hex("CFC6B3"), 0.03f, -0.45f, -0.85f, -0.45f, 0.65f);   // separa la croce dalle scritte
            return new DoodleArt("mirror_legend", p);
        }

        /// <summary>Freccia dei comandi (punta a destra, centro 0,0, ~0.5): bianca, si tinge.</summary>
        public static DoodleArt CtrlArrow()
        {
            var p = new DoodlePainter(-0.3f, -0.26f, 0.32f, 0.26f, 150) { Ink = 0.035f };
            p.Shape(Sd.Polygon(-0.24f, -0.09f, 0.02f, -0.09f, 0.02f, -0.2f, 0.27f, 0f, 0.02f, 0.2f, 0.02f, 0.09f, -0.24f, 0.09f), Rgba.White);
            return new DoodleArt("ctrl_arrow", p);
        }

        /// <summary>Icona "scambiati": doppia freccia orizzontale con lo specchio in mezzo (centro 0,0). Bianca, si tinge.</summary>
        public static DoodleArt SwapIcon()
        {
            var p = new DoodlePainter(-0.5f, -0.34f, 0.5f, 0.34f, 150) { Ink = 0.035f };
            p.Shape(Sd.Box(0, 0, 0.05f, 0.3f, 0.02f), Rgba.Hex("BFE6FF"), 0.025f);   // lo specchio
            foreach (float s in new[] { -1f, 1f })
                p.Shape(Sd.Polygon(s * 0.1f, -0.06f, s * 0.26f, -0.06f, s * 0.26f, -0.16f, s * 0.46f, 0f, s * 0.26f, 0.16f, s * 0.26f, 0.06f, s * 0.1f, 0.06f), Rgba.White);
            return new DoodleArt("swap_icon", p);
        }
    }
}
