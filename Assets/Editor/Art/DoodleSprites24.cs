using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli compositi della versione 0.40 (gruppo A): mattina del gatto, colpo in banca, sottomarino, casa stregata.</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV40A()
        {
            var list = new List<DoodleArt>
            {
                V40A_Dot(), V40A_Disc(),
                // mattina
                MorningBedroom(), MorningCurtain(), MorningKitchen(), MorningBowl(0), MorningBowl(1), MorningBowl(2),
                MorningCerealFill(), MorningFlake(), MorningCerealBox(), MorningMilk(), MorningCat(false), MorningCat(true),
                // banca
                HeistVault(), HeistPainting(), HeistUvTorch(), HeistVaultDoor(), HeistVaultWheel(), HeistVaultInside(),
                HeistKeypad(), HeistAlarmBox(), HeistWire(true), HeistWire(false), HeistNote(),
                // sottomarino
                SubSea(), SubTunnel(), SubSubmarine(), SubSeafloor(), SubWreck(), SubFlag(), SubRock(0), SubRock(1),
                SubEel(), SubSymbol(0), SubSymbol(1), SubSymbol(2), SubSymbol(3), SubDial(), SubLockPlate(), SubPingButton(),
                SubRing(), SubBubble(),
                // casa stregata
                HauntHall(), HauntCandle(), HauntMatchbox(), HauntMatch(false), HauntMatch(true), HauntGhost(0), HauntGhost(1),
                HauntGhost(2), HauntEyes(), HauntDoor(), HauntDoorOpen(),
            };
            for (int d = 0; d < 10; d++) list.Add(HeistInkDigit(d));
            return list;
        }

        static readonly Rgba V40A_Ink = Rgba.Hex("2B2133");

        static float V40A_Clamp(float v) { return v < 0f ? 0f : v > 1f ? 1f : v; }

        /// <summary>Puntino del progresso delle fasi (bianco col contorno, si tinge).</summary>
        public static DoodleArt V40A_Dot()
        {
            var p = new DoodlePainter(-0.17f, -0.17f, 0.17f, 0.17f, 200) { Ink = 0.04f, Wobble = 0.004f };
            p.Shape(Sd.Circle(0, 0, 0.13f), Rgba.White);
            p.Fill(Sd.Circle(-0.04f, 0.04f, 0.035f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("v40a_dot", p);
        }

        /// <summary>Disco bianco pieno di raggio 0,5 (maschera per le luci rotonde).</summary>
        public static DoodleArt V40A_Disc()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.5f, 0.5f, 128) { Wobble = 0f };
            p.Fill(Sd.Circle(0, 0, 0.49f), Rgba.White, false);
            return new DoodleArt("v40a_disc", p);
        }

        // =====================================================================
        // MORNING ROUTINE
        // =====================================================================

        /// <summary>
        /// Camera da letto (9×14, centro 0,0): parete azzurrina, finestra in (0, 1,6) larga 2,6 e alta 2,2 col bastone
        /// delle tende a y 2,85, pavimento da y -2,3, letto a sinistra (materasso a y -1,55), comodino a destra (piano a y -1,7).
        /// </summary>
        public static DoodleArt MorningBedroom()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Wobble = 0.006f };
            p.FillFunc(Sd.Box(0, 2.35f, 4.5f, 4.65f), (x, y) => Rgba.Lerp(Rgba.Hex("BFD3EE"), Rgba.Hex("A9C2E6"), V40A_Clamp((y + 2f) / 8f)));
            // righe verticali della carta da parati
            for (float x = -4.3f; x < 4.5f; x += 0.6f) p.Fill(Sd.Box(x, 2.35f, 0.08f, 4.65f), Rgba.Hex("B3C9EA"), false);
            // pavimento
            p.FillFunc(Sd.Box(0, -4.65f, 4.5f, 2.35f), (x, y) => Rgba.Lerp(Rgba.Hex("B98656"), Rgba.Hex("CF9C68"), V40A_Clamp((y + 7f) / 4.7f)));
            for (float y = -2.75f; y > -7f; y -= 0.5f) p.Line(Rgba.Hex("A57446"), 0.03f, -4.5f, y, 4.5f, y);
            p.Fill(Sd.Box(0, -2.35f, 4.5f, 0.09f), Rgba.Hex("F4EEE4"), false);
            p.InkLine(0.04f, -4.5f, -2.25f, 4.5f, -2.25f);
            // tappeto
            p.Shape(Sd.Ellipse(0.3f, -3.6f, 1.9f, 0.5f), Rgba.Hex("E58FA3"), 0.04f);
            p.Fill(Sd.Ellipse(0.3f, -3.6f, 1.4f, 0.3f), Rgba.Hex("F2B6C4"));
            // finestra: cielo del mattino col sole
            Sdf glass = Sd.Box(0, 1.6f, 1.3f, 1.1f);
            p.FillFunc(glass, (x, y) => Rgba.Lerp(Rgba.Hex("FFE7A8"), Rgba.Hex("8FD0F5"), V40A_Clamp((y - 0.5f) / 2.2f)));
            p.Fill(Sd.Circle(0.55f, 1.3f, 0.75f), Rgba.Hex("FFF3B8").WithAlpha(0.6f));
            p.Shape(Sd.Circle(0.55f, 1.3f, 0.42f), Rgba.Hex("FFD54A"), 0.04f);
            p.Fill(Sd.Union(Sd.Ellipse(-0.6f, 2.2f, 0.45f, 0.16f), Sd.Ellipse(-0.3f, 2.3f, 0.3f, 0.18f)), Rgba.White.WithAlpha(0.9f));
            p.Shape(Sd.Subtract(Sd.Box(0, 1.6f, 1.45f, 1.25f), glass), Rgba.Hex("FFFFFF"), 0.05f);
            p.Shape(Sd.Box(0, 1.6f, 0.05f, 1.1f), Rgba.White, 0.035f);
            p.Shape(Sd.Box(0, 1.6f, 1.3f, 0.05f), Rgba.White, 0.035f);
            p.Shape(Sd.Box(0, 0.3f, 1.65f, 0.1f, 0.04f), Rgba.White, 0.045f);   // davanzale
            // bastone delle tende
            p.Shape(Sd.Box(0, 2.85f, 2.2f, 0.06f, 0.05f), Rgba.Hex("8A5A3A"), 0.035f);
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Circle(s * 2.25f, 2.85f, 0.11f), Rgba.Hex("B07A50"), 0.035f);
            // letto a sinistra
            Sdf head = Sd.Box(-2.95f, -0.55f, 0.18f, 1.25f, 0.12f);
            Body(p, head, Rgba.Hex("8F6BC4"), Rgba.Hex("7353A8"));
            Body(p, Sd.Box(-1.4f, -2.05f, 1.7f, 0.42f, 0.08f), Rgba.Hex("8F6BC4"), Rgba.Hex("7353A8"));
            Body(p, Sd.Box(-1.4f, -1.55f, 1.62f, 0.18f, 0.12f), Rgba.Hex("F7F3FF"), Rgba.Hex("DCD4F0"));
            p.Shape(Sd.Box(-0.5f, -1.75f, 0.85f, 0.28f, 0.14f), Rgba.Hex("7FC3E8"), 0.04f);   // coperta
            p.Hatch(Sd.Box(-0.5f, -1.75f, 0.8f, 0.24f, 0.12f), Rgba.Hex("67AEDA"), 0.14f, 0.03f, 45f);
            foreach (float s in new[] { -2.9f, 0.15f }) p.Shape(Sd.Box(s, -2.6f, 0.1f, 0.18f), Rgba.Hex("6B4B92"), 0.035f);
            // comodino
            Body(p, Sd.Box(2.2f, -2.0f, 0.75f, 0.32f, 0.05f), Rgba.Hex("C9935C"), Rgba.Hex("A87443"));
            p.Shape(Sd.Box(2.2f, -1.72f, 0.82f, 0.06f, 0.03f), Rgba.Hex("D9A671"), 0.035f);
            p.Shape(Sd.Circle(2.2f, -2.0f, 0.05f), Rgba.Hex("FFD54A"), 0.02f);
            p.Shape(Sd.Box(1.6f, -2.45f, 0.06f, 0.14f), Rgba.Hex("A87443"), 0.03f);
            p.Shape(Sd.Box(2.8f, -2.45f, 0.06f, 0.14f), Rgba.Hex("A87443"), 0.03f);
            // quadretto
            p.Shape(Sd.Box(2.6f, 1.9f, 0.4f, 0.5f), Rgba.Hex("FFE9A8"), 0.04f);
            p.Shape(Sd.Circle(2.6f, 1.95f, 0.18f), Rgba.Hex("F2A65A"), 0.03f);
            p.Fill(Sd.Polygon(2.47f, 2.08f, 2.52f, 2.22f, 2.57f, 2.1f), Rgba.Hex("F2A65A"));
            p.Fill(Sd.Polygon(2.63f, 2.1f, 2.68f, 2.22f, 2.73f, 2.08f), Rgba.Hex("F2A65A"));
            return new DoodleArt("morning_bedroom_bg", p);
        }

        /// <summary>Tenda a righe calda (centro in alto 0,0, larga 1,4, alta 2,7): appesa al bastone, si allarga in basso.</summary>
        public static DoodleArt MorningCurtain()
        {
            var p = new DoodlePainter(-0.8f, -2.75f, 0.8f, 0.12f, 90) { Ink = 0.045f };
            Sdf cloth = Sd.Polygon(-0.68f, 0.02f, 0.68f, 0.02f, 0.74f, -2.65f, -0.74f, -2.65f);
            p.FillFunc(cloth, (x, y) =>
            {
                float f = (float)Math.Sin((x + 0.7f) / 1.4f * Math.PI * 4f);
                return Rgba.Lerp(Rgba.Hex("F2A33A"), Rgba.Hex("FFC768"), 0.5f + 0.5f * f);
            });
            for (int i = 0; i < 4; i++)
            {
                float x0 = -0.52f + i * 0.35f;
                p.Line(Rgba.Hex("D9842A"), 0.03f, x0, -0.05f, x0 + (x0 * 0.06f), -2.6f);
            }
            p.Fill(Sd.Box(0, -2.45f, 0.75f, 0.09f), Rgba.Hex("D9842A"));
            p.Shape(cloth, Rgba.Clear);
            for (int i = 0; i < 5; i++) p.Shape(Sd.Circle(-0.56f + i * 0.28f, 0.0f, 0.07f), Rgba.Hex("C8C8D6"), 0.025f);
            return new DoodleArt("morning_curtain", p);
        }

        /// <summary>
        /// Cucina (9×14, centro 0,0): piastrelle, finestrella, mensola; tavolo con il piano a y -1,6 (bordo davanti
        /// fino a -1,95) e gambe fino al pavimento.
        /// </summary>
        public static DoodleArt MorningKitchen()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Wobble = 0.006f };
            p.FillFunc(Sd.Box(0, 1.5f, 4.5f, 5.5f), (x, y) => Rgba.Lerp(Rgba.Hex("CDEBDD"), Rgba.Hex("B7E0CE"), V40A_Clamp((y + 3f) / 8f)));
            for (float x = -4.5f; x <= 4.5f; x += 0.55f) p.Line(Rgba.Hex("A8D3C0"), 0.025f, x, -4f, x, 7f);
            for (float y = -4f; y <= 7f; y += 0.55f) p.Line(Rgba.Hex("A8D3C0"), 0.025f, -4.5f, y, 4.5f, y);
            p.FillFunc(Sd.Box(0, -5.5f, 4.5f, 1.5f), (x, y) => Rgba.Lerp(Rgba.Hex("9C7A5C"), Rgba.Hex("B08D6C"), V40A_Clamp((y + 7f) / 3f)));
            p.InkLine(0.04f, -4.5f, -4f, 4.5f, -4f);
            // finestrella
            Sdf glass = Sd.Box(-1.6f, 2.0f, 0.8f, 0.7f);
            p.FillFunc(glass, (x, y) => Rgba.Lerp(Rgba.Hex("FFF0B8"), Rgba.Hex("8FD0F5"), V40A_Clamp((y - 1.3f) / 1.4f)));
            p.Shape(Sd.Circle(-1.3f, 1.75f, 0.25f), Rgba.Hex("FFD54A"), 0.035f);
            p.Shape(Sd.Subtract(Sd.Box(-1.6f, 2.0f, 0.92f, 0.82f), glass), Rgba.White, 0.045f);
            p.Shape(Sd.Box(-1.6f, 2.0f, 0.04f, 0.7f), Rgba.White, 0.03f);
            // mensola con barattoli
            p.Shape(Sd.Box(1.6f, 1.6f, 1.0f, 0.06f), Rgba.Hex("B98656"), 0.035f);
            string[] jar = { "F28B82", "FFD166", "8EC5FF" };
            for (int i = 0; i < 3; i++)
            {
                float x = 1.0f + i * 0.6f;
                p.Shape(Sd.Box(x, 1.92f, 0.2f, 0.26f, 0.06f), Rgba.Hex("EAF6FF"), 0.035f);
                p.Fill(Sd.Box(x, 1.84f, 0.16f, 0.15f, 0.04f), Rgba.Hex(jar[i]));
                p.Shape(Sd.Box(x, 2.2f, 0.22f, 0.05f, 0.02f), Rgba.Hex(jar[i]), 0.03f);
            }
            // orologio a muro
            p.Shape(Sd.Circle(2.4f, 3.1f, 0.38f), Rgba.White, 0.045f);
            p.InkLine(0.04f, 2.4f, 3.1f, 2.4f, 3.33f);
            p.InkLine(0.04f, 2.4f, 3.1f, 2.56f, 3.02f);
            // tavolo
            p.Fill(Sd.Box(0, -1.45f, 4.5f, 0.16f), Rgba.Hex("E8B07A"));
            p.Fill(Sd.Box(0, -1.32f, 4.5f, 0.03f), Rgba.Hex("F4C892"));
            p.Fill(Sd.Box(0, -1.78f, 4.5f, 0.17f), Rgba.Hex("C98A52"));
            p.InkLine(0.045f, -4.5f, -1.61f, 4.5f, -1.61f);
            p.InkLine(0.045f, -4.5f, -1.95f, 4.5f, -1.95f);
            p.InkLine(0.045f, -4.5f, -1.29f, 4.5f, -1.29f);
            foreach (float x in new[] { -2.4f, 2.4f })
            {
                Body(p, Sd.Box(x, -3.0f, 0.15f, 1.05f, 0.04f), Rgba.Hex("C98A52"), Rgba.Hex("A86E3C"));
            }
            // tovaglietta a quadri sotto la ciotola
            Sdf mat = Sd.Box(0, -1.45f, 1.7f, 0.13f, 0.05f);
            p.Fill(mat, Rgba.Hex("F28B82"));
            p.Hatch(mat, Rgba.Hex("FFFFFF").WithAlpha(0.6f), 0.18f, 0.06f, 90f);
            return new DoodleArt("morning_kitchen_bg", p);
        }

        // geometria della ciotola di vetro (coordinate locali): bordo a y 0,6 largo 2,4, fondo a y -0,6
        static Sdf V40A_BowlShape() => Sd.Intersect(Sd.Ellipse(0, 0.6f, 1.2f, 1.2f), Sd.Box(0, 0f, 1.3f, 0.6f));

        /// <summary>
        /// Ciotola di vetro (centro 0,0; bordo a y 0,6 largo 2,4; fondo a y -0,6). kind 0 = dietro (vetro interno e
        /// bordo posteriore), 1 = davanti (vetro con riflessi, linea tratteggiata dei cereali a y -0,05 e del latte a
        /// y 0,35), 2 = maschera bianca dell'interno.
        /// </summary>
        public static DoodleArt MorningBowl(int kind)
        {
            var p = new DoodlePainter(-1.32f, -0.82f, 1.32f, 0.85f, 110) { Ink = 0.05f, Wobble = 0.006f };
            Sdf bowl = V40A_BowlShape();
            Sdf rim = Sd.Ellipse(0, 0.6f, 1.2f, 0.16f);
            if (kind == 2)
            {
                p.Fill(Sd.Union(bowl, Sd.Intersect(rim, Sd.Box(0, 0.5f, 1.3f, 0.1f))), Rgba.White, false);
                return new DoodleArt("morning_bowl_mask", p);
            }
            if (kind == 0)
            {
                p.Fill(bowl, Rgba.Hex("DDF2FF").WithAlpha(0.35f));
                p.Fill(rim, Rgba.Hex("DDF2FF").WithAlpha(0.4f));
                p.Line(V40A_Ink, 0.045f, V40A_EllipseArc(0, 0.6f, 1.2f, 0.16f, 0f, 180f, 24));
                return new DoodleArt("morning_bowl_back", p);
            }
            // davanti: vetro con riflessi e tratteggi
            p.Fill(bowl, Rgba.Hex("CDEBFF").WithAlpha(0.22f));
            p.Fill(Sd.Intersect(bowl, Sd.Capsule(-0.85f, 0.35f, -0.55f, -0.35f, 0.07f)), Rgba.White.WithAlpha(0.65f));
            p.Fill(Sd.Intersect(bowl, Sd.Capsule(-0.62f, 0.42f, -0.5f, 0.18f, 0.035f)), Rgba.White.WithAlpha(0.65f));
            V40A_Dashes(p, bowl, -0.05f, Rgba.Hex("E0902A"));
            V40A_Dashes(p, bowl, 0.35f, Rgba.Hex("3A7BD5"));
            p.Shape(bowl, Rgba.Clear);
            p.Line(V40A_Ink, 0.05f, V40A_EllipseArc(0, 0.6f, 1.2f, 0.16f, 180f, 360f, 24));
            // piedino
            p.Shape(Sd.Box(0, -0.68f, 0.5f, 0.08f, 0.05f), Rgba.Hex("CDEBFF").WithAlpha(0.7f), 0.04f);
            return new DoodleArt("morning_bowl_front", p);
        }

        static void V40A_Dashes(DoodlePainter p, Sdf bowl, float y, Rgba color)
        {
            for (float x = -1.2f; x < 1.2f; x += 0.24f)
            {
                if (bowl(x, y) > -0.08f || bowl(x + 0.12f, y) > -0.08f) continue;
                p.Line(color, 0.045f, x, y, x + 0.12f, y);
            }
        }

        static float[] V40A_EllipseArc(float cx, float cy, float rx, float ry, float a0, float a1, int n)
        {
            var pts = new float[(n + 1) * 2];
            for (int i = 0; i <= n; i++)
            {
                double a = (a0 + (a1 - a0) * i / n) * Math.PI / 180.0;
                pts[i * 2] = cx + rx * (float)Math.Cos(a);
                pts[i * 2 + 1] = cy + ry * (float)Math.Sin(a);
            }
            return pts;
        }

        /// <summary>Riempimento di cereali (rettangolo 2,6×1,5): il bordo in alto (y 0) è una fila di anellini.</summary>
        public static DoodleArt MorningCerealFill()
        {
            var p = new DoodlePainter(-1.3f, -1.5f, 1.3f, 0.12f, 90) { Ink = 0.025f, Wobble = 0.004f };
            p.Fill(Sd.Box(0, -0.75f, 1.3f, 0.75f), Rgba.Hex("E9A23B"), false);
            var rnd = new Random(41);
            for (int i = 0; i < 90; i++)
            {
                float x = -1.25f + (float)rnd.NextDouble() * 2.5f, y = -1.45f + (float)rnd.NextDouble() * 1.45f;
                p.Shape(Sd.Subtract(Sd.Circle(x, y, 0.09f), Sd.Circle(x, y, 0.035f)), Rgba.Hex(rnd.Next(2) == 0 ? "F7C35F" : "F0B04A"));
            }
            for (float x = -1.22f; x < 1.3f; x += 0.17f)
            {
                float y = -0.02f + (float)rnd.NextDouble() * 0.05f;
                p.Shape(Sd.Subtract(Sd.Circle(x, y, 0.09f), Sd.Circle(x, y, 0.035f)), Rgba.Hex("FFD27A"));
            }
            return new DoodleArt("morning_cereal_fill", p);
        }

        /// <summary>Un anellino di cereale (centro 0,0, diametro 0,18).</summary>
        public static DoodleArt MorningFlake()
        {
            var p = new DoodlePainter(-0.12f, -0.12f, 0.12f, 0.12f, 200) { Ink = 0.025f, Wobble = 0.003f };
            p.Shape(Sd.Subtract(Sd.Circle(0, 0, 0.09f), Sd.Circle(0, 0, 0.035f)), Rgba.Hex("F7C35F"));
            return new DoodleArt("morning_flake", p);
        }

        /// <summary>Scatola dei cereali (centro 0,0, 1×1,5): aperta in alto a sinistra (la bocca è in -0,3, 0,75).</summary>
        public static DoodleArt MorningCerealBox()
        {
            var p = new DoodlePainter(-0.62f, -0.82f, 0.62f, 0.95f, 110) { Ink = 0.045f };
            Sdf box = Sd.Box(0, -0.02f, 0.5f, 0.75f, 0.03f);
            Body(p, box, Rgba.Hex("E8453C"), Rgba.Hex("C42F2A"));
            // linguetta aperta
            p.Shape(Sd.Polygon(-0.5f, 0.73f, -0.08f, 0.73f, -0.2f, 0.9f, -0.58f, 0.86f), Rgba.Hex("F06A5E"), 0.035f);
            p.Fill(Sd.Box(-0.29f, 0.71f, 0.19f, 0.03f), V40A_Ink);
            // etichetta: tazza con anellini e sole
            p.Shape(Sd.Box(0, 0.36f, 0.4f, 0.17f, 0.06f), Rgba.Hex("FFE36E"), 0.03f);
            p.Line(Rgba.Hex("C42F2A"), 0.05f, -0.27f, 0.36f, -0.1f, 0.36f);
            p.Line(Rgba.Hex("C42F2A"), 0.05f, 0.02f, 0.36f, 0.27f, 0.36f);
            p.Shape(Sd.Intersect(Sd.Circle(0, -0.2f, 0.33f), Sd.Box(0, -0.38f, 0.4f, 0.18f)), Rgba.White, 0.035f);
            for (int i = 0; i < 5; i++)
            {
                float x = -0.24f + i * 0.12f, y = -0.17f + (i % 2) * 0.05f;
                p.Shape(Sd.Subtract(Sd.Circle(x, y, 0.07f), Sd.Circle(x, y, 0.027f)), Rgba.Hex("F7C35F"), 0.02f);
            }
            p.Fill(Sd.Box(0, -0.66f, 0.46f, 0.04f), Rgba.Hex("FFE36E"));
            return new DoodleArt("morning_cereal_box", p);
        }

        /// <summary>Cartone del latte (centro 0,0, 0,8×1,5): tetto a capanna con il beccuccio in alto a sinistra (-0,3, 0,85).</summary>
        public static DoodleArt MorningMilk()
        {
            var p = new DoodlePainter(-0.5f, -0.8f, 0.48f, 1.0f, 120) { Ink = 0.04f };
            Sdf body = Sd.Box(0, -0.2f, 0.38f, 0.58f, 0.03f);
            Body(p, body, Rgba.White, Rgba.Hex("DDE6F2"));
            p.Shape(Sd.Polygon(-0.38f, 0.38f, 0.38f, 0.38f, 0.0f, 0.78f), Rgba.Hex("E8F0FA"), 0.035f);
            p.Shape(Sd.Polygon(-0.38f, 0.38f, -0.2f, 0.78f, -0.42f, 0.86f), Rgba.Hex("F4F8FF"), 0.03f);   // beccuccio aperto
            p.Shape(Sd.Box(0, 0.83f, 0.06f, 0.06f), Rgba.Hex("E8F0FA"), 0.03f);
            // macchie da mucca e scritta a goccia
            p.Fill(Sd.Ellipse(-0.18f, 0.12f, 0.12f, 0.08f), V40A_Ink);
            p.Fill(Sd.Ellipse(0.2f, -0.6f, 0.13f, 0.09f), V40A_Ink);
            p.Fill(Sd.Ellipse(0.24f, 0.18f, 0.07f, 0.05f), V40A_Ink);
            p.Shape(Sd.Box(0, -0.2f, 0.3f, 0.16f, 0.06f), Rgba.Hex("4D8EF0"), 0.03f);
            p.Fill(Sd.Union(Sd.Circle(0, -0.25f, 0.07f), Sd.Polygon(-0.06f, -0.22f, 0.06f, -0.22f, 0f, -0.08f)), Rgba.White);
            return new DoodleArt("morning_milk", p);
        }

        /// <summary>
        /// Gatto affacciato al tavolo (testa centrata in 0,0, zampe sul bordo a y -0,75): affamato (occhi grandi) o
        /// felice (occhi chiusi, guance rosse, briciole).
        /// </summary>
        public static DoodleArt MorningCat(bool happy)
        {
            var p = new DoodlePainter(-1.05f, -1.0f, 1.05f, 1.05f, 110) { Ink = 0.05f };
            Rgba fur = Rgba.Hex("F5A04A"), shade = Rgba.Hex("D97F2E"), pale = Rgba.Hex("FFE3C4");
            Sdf body = Sd.Ellipse(0, -0.75f, 0.7f, 0.45f);
            Body(p, body, fur, shade);
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Polygon(s * 0.62f, 0.3f, s * 0.68f, 0.98f, s * 0.18f, 0.6f), fur, shade);
                p.Fill(Sd.Polygon(s * 0.55f, 0.42f, s * 0.6f, 0.82f, s * 0.3f, 0.6f), Rgba.Hex("F7B6C2"));
            }
            Sdf head = Sd.Ellipse(0, 0.1f, 0.78f, 0.62f);
            Body(p, head, fur, shade);
            for (int i = -1; i <= 1; i++) p.Line(shade, 0.05f, i * 0.14f, 0.7f, i * 0.12f, 0.5f);
            p.Shape(Sd.Ellipse(0, -0.18f, 0.36f, 0.24f), pale, 0.03f);
            p.Fill(Sd.Polygon(-0.07f, -0.02f, 0.07f, -0.02f, 0f, -0.09f), Rgba.Hex("F28B9B"));
            if (happy)
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    p.InkLine(0.05f, s * 0.42f, 0.12f, s * 0.3f, 0.22f, s * 0.18f, 0.12f);
                    p.Fill(Sd.Ellipse(s * 0.5f, -0.08f, 0.13f, 0.08f), Rgba.Hex("FF7A8A").WithAlpha(0.7f));
                }
                p.Shape(Sd.Intersect(Sd.Circle(0, -0.2f, 0.16f), Sd.Box(0, -0.3f, 0.2f, 0.1f)), Rgba.Hex("B03A4E"), 0.03f);
                p.Fill(Sd.Circle(0.06f, -0.32f, 0.05f), Rgba.Hex("F28B9B"));
                p.Shape(Sd.Subtract(Sd.Circle(-0.5f, -0.45f, 0.05f), Sd.Circle(-0.5f, -0.45f, 0.02f)), Rgba.Hex("F7C35F"), 0.015f);
                p.Shape(Sd.Subtract(Sd.Circle(0.42f, -0.5f, 0.05f), Sd.Circle(0.42f, -0.5f, 0.02f)), Rgba.Hex("F7C35F"), 0.015f);
            }
            else
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    p.Shape(Sd.Ellipse(s * 0.3f, 0.17f, 0.16f, 0.19f), Rgba.White, 0.035f);
                    p.Fill(Sd.Ellipse(s * 0.28f, 0.15f, 0.1f, 0.13f), V40A_Ink);
                    p.Fill(Sd.Circle(s * 0.25f, 0.21f, 0.04f), Rgba.White);
                }
                p.InkLine(0.035f, -0.12f, -0.17f, 0f, -0.12f, 0.12f, -0.17f);
                p.Fill(Sd.Ellipse(0.12f, -0.27f, 0.05f, 0.08f), Rgba.Hex("8EC5FF"));   // acquolina
            }
            foreach (float s in new[] { -1f, 1f })
            {
                p.InkLine(0.025f, s * 0.42f, -0.1f, s * 0.95f, -0.02f);
                p.InkLine(0.025f, s * 0.42f, -0.18f, s * 0.95f, -0.22f);
                Body(p, Sd.Ellipse(s * 0.5f, -0.86f, 0.24f, 0.14f), fur, shade);   // zampe sul tavolo
                p.InkLine(0.02f, s * 0.5f, -0.86f, s * 0.5f, -0.95f);
            }
            return new DoodleArt(happy ? "morning_cat_happy" : "morning_cat_hungry", p);
        }

        // =====================================================================
        // BANK HEIST
        // =====================================================================

        /// <summary>Caveau (9×14, centro 0,0): pareti d'acciaio a pannelli con rivetti, pavimento scuro da y -2,6.</summary>
        public static DoodleArt HeistVault()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Wobble = 0.004f };
            p.FillFunc(Sd.Box(0, 2.2f, 4.5f, 4.8f), (x, y) => Rgba.Lerp(Rgba.Hex("5B6B78"), Rgba.Hex("6F8190"), V40A_Clamp(1f - Math.Abs(x) / 4.5f)));
            for (float x = -4.2f; x < 4.5f; x += 1.4f)
                for (float y = -2.2f; y < 7f; y += 1.6f)
                {
                    p.Shape(Sd.Box(x + 0.7f, y + 0.8f, 0.64f, 0.74f, 0.06f), Rgba.Hex("6A7C8B"), 0.03f);
                    foreach (float dx in new[] { -0.52f, 0.52f })
                        foreach (float dy in new[] { -0.62f, 0.62f })
                            p.Shape(Sd.Circle(x + 0.7f + dx, y + 0.8f + dy, 0.045f), Rgba.Hex("9AABB8"), 0.015f);
                }
            p.FillFunc(Sd.Box(0, -4.8f, 4.5f, 2.2f), (x, y) => Rgba.Lerp(Rgba.Hex("2E3740"), Rgba.Hex("434F5A"), V40A_Clamp((y + 7f) / 4.4f)));
            p.Fill(Sd.Box(0, -2.62f, 4.5f, 0.06f), Rgba.Hex("2B2133"));
            for (float x = -4.5f; x < 4.6f; x += 0.9f) p.Line(Rgba.Hex("3A444E"), 0.03f, x, -2.66f, x * 1.5f, -7f);
            // luce dall'alto
            p.FillFunc(Sd.Polygon(-0.6f, 7f, 0.6f, 7f, 2.6f, -2.6f, -2.6f, -2.6f), (x, y) => Rgba.Hex("FFF6D8").WithAlpha(0.08f));
            return new DoodleArt("heist_vault_bg", p);
        }

        /// <summary>
        /// Quadro con cornice dorata (centro 0,0; 3,2×2,5; tela da -1,35..1,35 × -1,0..1,0): ritratto di un gatto
        /// nobile con monocolo, su sfondo verde scuro.
        /// </summary>
        public static DoodleArt HeistPainting()
        {
            var p = new DoodlePainter(-1.65f, -1.3f, 1.65f, 1.3f, 100) { Ink = 0.05f };
            Sdf frame = Sd.Box(0, 0, 1.6f, 1.25f, 0.08f);
            Sdf canvas = Sd.Box(0, 0, 1.35f, 1.0f);
            Body(p, frame, Rgba.Hex("E2B13C"), Rgba.Hex("B8862A"));
            for (int i = 0; i < 14; i++)
            {
                float t = i / 14f * 2f * (float)Math.PI;
                p.Shape(Sd.Circle(1.48f * (float)Math.Cos(t), 1.12f * (float)Math.Sin(t), 0.06f), Rgba.Hex("F5D26B"), 0.02f);
            }
            p.FillFunc(canvas, (x, y) => Rgba.Lerp(Rgba.Hex("2F5A46"), Rgba.Hex("467A5E"), V40A_Clamp((y + 1f) / 2f)));
            // gatto nobile
            Rgba fur = Rgba.Hex("9AA3B5"), shade = Rgba.Hex("7A8396");
            Body(p, Sd.Intersect(Sd.Ellipse(0, -1.0f, 0.95f, 0.65f), canvas), Rgba.Hex("7A2E45"), Rgba.Hex("5E2034"));
            p.Shape(Sd.Polygon(-0.2f, -0.45f, 0.2f, -0.45f, 0f, -0.75f), Rgba.White, 0.03f);   // colletto
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Polygon(s * 0.5f, 0.25f, s * 0.5f, 0.85f, s * 0.12f, 0.55f), fur, shade);
            Body(p, Sd.Ellipse(0, 0.08f, 0.58f, 0.5f), fur, shade);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Ellipse(s * 0.22f, 0.17f, 0.1f, 0.07f), Rgba.Hex("D9E36A"), 0.025f);
                p.Fill(Sd.Ellipse(s * 0.22f, 0.17f, 0.025f, 0.06f), V40A_Ink);
            }
            p.Shape(Sd.Subtract(Sd.Circle(0.22f, 0.17f, 0.15f), Sd.Circle(0.22f, 0.17f, 0.12f)), Rgba.Hex("E2B13C"), 0.015f);   // monocolo
            p.InkLine(0.02f, 0.36f, 0.12f, 0.45f, -0.3f);
            p.Fill(Sd.Polygon(-0.05f, -0.02f, 0.05f, -0.02f, 0f, -0.07f), Rgba.Hex("E58FA3"));
            p.InkLine(0.03f, -0.18f, -0.15f, -0.05f, -0.1f, 0f, -0.15f, 0.05f, -0.1f, 0.18f, -0.15f);   // baffoni
            p.Shape(canvas, Rgba.Clear, 0.04f);
            return new DoodleArt("heist_painting", p);
        }

        /// <summary>Torcia UV vista di lato verso l'alto (lente viola in cima a 0, 0; impugnatura fino a y -1,2).</summary>
        public static DoodleArt HeistUvTorch()
        {
            var p = new DoodlePainter(-0.3f, -1.25f, 0.3f, 0.1f, 140) { Ink = 0.04f };
            Body(p, Sd.Box(0, -0.75f, 0.13f, 0.45f, 0.05f), Rgba.Hex("3C3F4F"), Rgba.Hex("272936"));
            for (int i = 0; i < 4; i++) p.Line(Rgba.Hex("5A5E73"), 0.025f, -0.1f, -0.55f - i * 0.12f, 0.1f, -0.55f - i * 0.12f);
            p.Shape(Sd.Box(0.0f, -0.62f, 0.06f, 0.06f, 0.02f), Rgba.Hex("B05CFF"), 0.02f);
            Body(p, Sd.Polygon(-0.13f, -0.3f, 0.13f, -0.3f, 0.24f, -0.02f, -0.24f, -0.02f), Rgba.Hex("4C5066"), Rgba.Hex("33364A"));
            p.Shape(Sd.Box(0, 0f, 0.25f, 0.05f, 0.03f), Rgba.Hex("C78BFF"), 0.035f);
            return new DoodleArt("heist_uv_torch", p);
        }

        /// <summary>Portellone rotondo del caveau (centro 0,0, diametro 3,4) con bulloni; il volante è a parte.</summary>
        public static DoodleArt HeistVaultDoor()
        {
            var p = new DoodlePainter(-1.8f, -1.8f, 1.8f, 1.8f, 90) { Ink = 0.06f };
            Body(p, Sd.Circle(0, 0, 1.7f), Rgba.Hex("A9B4BE"), Rgba.Hex("86929E"));
            p.Shape(Sd.Circle(0, 0, 1.38f), Rgba.Hex("BAC4CD"), 0.04f);
            p.Fill(Sd.Crescent(Sd.Circle(0, 0, 1.38f), -0.08f, 0.08f), Rgba.Hex("9CA8B3"));
            for (int i = 0; i < 16; i++)
            {
                double a = i / 16.0 * 2 * Math.PI;
                p.Shape(Sd.Circle(1.54f * (float)Math.Cos(a), 1.54f * (float)Math.Sin(a), 0.07f), Rgba.Hex("D5DCE2"), 0.02f);
            }
            p.Shape(Sd.Circle(0, 0, 0.3f), Rgba.Hex("7E8A96"), 0.035f);
            return new DoodleArt("heist_vault_door", p);
        }

        /// <summary>Volante a 4 razze del portellone (centro 0,0, raggio 0,9): gira mentre si apre.</summary>
        public static DoodleArt HeistVaultWheel()
        {
            var p = new DoodlePainter(-1.0f, -1.0f, 1.0f, 1.0f, 110) { Ink = 0.045f };
            Rgba steel = Rgba.Hex("E2B13C");
            for (int i = 0; i < 4; i++)
            {
                double a = i * Math.PI / 4 + Math.PI / 8;
                float cx = (float)Math.Cos(a), sy = (float)Math.Sin(a);
                p.Shape(Sd.Capsule(-0.85f * cx, -0.85f * sy, 0.85f * cx, 0.85f * sy, 0.07f), steel, 0.035f);
            }
            for (int i = 0; i < 8; i++)
            {
                double a = i * Math.PI / 4 + Math.PI / 8;
                p.Shape(Sd.Circle(0.88f * (float)Math.Cos(a), 0.88f * (float)Math.Sin(a), 0.11f), Rgba.Hex("F5D26B"), 0.035f);
            }
            p.Shape(Sd.Circle(0, 0, 0.22f), Rgba.Hex("F5D26B"), 0.04f);
            return new DoodleArt("heist_vault_wheel", p);
        }

        /// <summary>Interno del caveau dietro al portellone (centro 0,0, raggio 1,6): buio con lingotti e sacchi d'oro.</summary>
        public static DoodleArt HeistVaultInside()
        {
            var p = new DoodlePainter(-1.75f, -1.75f, 1.75f, 1.75f, 90) { Ink = 0.045f };
            Sdf hole = Sd.Circle(0, 0, 1.62f);
            p.FillFunc(hole, (x, y) => Rgba.Lerp(Rgba.Hex("15121C"), Rgba.Hex("2E2633"), V40A_Clamp((y + 1.6f) / 3.2f)));
            Rgba gold = Rgba.Hex("FFC93C"), goldShade = Rgba.Hex("E09A1E");
            for (int row = 0; row < 3; row++)
                for (int i = 0; i < 5 - row; i++)
                {
                    float x = -0.95f + row * 0.24f + i * 0.48f, y = -0.95f + row * 0.26f;
                    Sdf bar = Sd.Intersect(Sd.Polygon(x - 0.24f, y - 0.12f, x + 0.24f, y - 0.12f, x + 0.18f, y + 0.12f, x - 0.18f, y + 0.12f), hole);
                    Body(p, bar, gold, goldShade);
                }
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Intersect(Sd.Union(Sd.Ellipse(s * 0.95f, -0.3f, 0.35f, 0.4f), Sd.Box(s * 0.95f, 0.15f, 0.1f, 0.12f)), hole), Rgba.Hex("C9A46A"), Rgba.Hex("A8854E"));
                p.Fill(Sd.Circle(s * 0.95f, -0.35f, 0.12f), goldShade);
            }
            p.Fill(Sd.Polygon(0.2f, 0.9f, 0.27f, 1.0f, 0.34f, 0.9f, 0.27f, 0.8f), Rgba.White.WithAlpha(0.9f));
            p.Shape(Sd.Subtract(Sd.Circle(0, 0, 1.75f), hole), Rgba.Hex("4A5560"), 0.05f);
            return new DoodleArt("heist_vault_inside", p);
        }

        /// <summary>Tastierino a muro (centro 0,0; 3×4,2): display scuro in alto (0, 1,6), spazio per 12 tasti sotto.</summary>
        public static DoodleArt HeistKeypad()
        {
            var p = new DoodlePainter(-1.55f, -2.15f, 1.55f, 2.15f, 90) { Ink = 0.05f };
            Body(p, Sd.Box(0, 0, 1.48f, 2.08f, 0.16f), Rgba.Hex("C3CBD3"), Rgba.Hex("9EA8B2"));
            p.Shape(Sd.Box(0, 1.6f, 1.25f, 0.33f, 0.08f), Rgba.Hex("123A2A"), 0.04f);
            p.Fill(Sd.Box(-0.6f, 1.75f, 0.45f, 0.05f), Rgba.White.WithAlpha(0.12f));
            foreach (float s in new[] { -1f, 1f }) foreach (float t in new[] { -1f, 1f })
                p.Shape(Sd.Circle(s * 1.3f, t * 1.9f, 0.06f), Rgba.Hex("E2E7EC"), 0.02f);
            return new DoodleArt("heist_keypad", p);
        }

        /// <summary>
        /// Centralina dell'allarme aperta (centro 0,0; 3,6×2,9): lampeggiante in alto (0, 1,25), morsetti in alto a
        /// y 0,75 e in basso a y -1,05 per 4 fili (x -1,2, -0,4, 0,4, 1,2).
        /// </summary>
        public static DoodleArt HeistAlarmBox()
        {
            var p = new DoodlePainter(-1.9f, -1.5f, 1.9f, 1.6f, 90) { Ink = 0.05f };
            Body(p, Sd.Box(0, -0.15f, 1.8f, 1.3f, 0.12f), Rgba.Hex("D9DEE3"), Rgba.Hex("B5BDC5"));
            p.Shape(Sd.Box(0, -0.15f, 1.6f, 1.08f, 0.06f), Rgba.Hex("2E3440"), 0.04f);
            foreach (float x in new[] { -1.2f, -0.4f, 0.4f, 1.2f })
            {
                p.Shape(Sd.Box(x, 0.75f, 0.17f, 0.1f, 0.03f), Rgba.Hex("B0B8C0"), 0.03f);
                p.Shape(Sd.Box(x, -1.05f, 0.17f, 0.1f, 0.03f), Rgba.Hex("B0B8C0"), 0.03f);
            }
            p.Shape(Sd.Box(0, 1.22f, 0.4f, 0.12f, 0.04f), Rgba.Hex("8E98A2"), 0.035f);
            p.Shape(Sd.Intersect(Sd.Circle(0, 1.3f, 0.3f), Sd.Box(0, 1.5f, 0.32f, 0.2f)), Rgba.Hex("FF5A5A"), 0.04f);
            p.Fill(Sd.Ellipse(-0.1f, 1.43f, 0.07f, 0.04f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("heist_alarm_box", p);
        }

        /// <summary>
        /// Metà di un filo bianco (si tinge): top = da y 0,75 (morsetto) fino al taglio a y -0,15; altrimenti dal taglio
        /// fino al morsetto in basso a y -1,05. Il filo ondeggia un po'. Origine (0,0) = centro della centralina.
        /// </summary>
        public static DoodleArt HeistWire(bool top)
        {
            var p = new DoodlePainter(-0.2f, top ? -0.25f : -1.1f, 0.2f, top ? 0.8f : -0.05f, 140) { Ink = 0.035f };
            float[] pts = top ? new[] { 0f, 0.75f, 0.06f, 0.45f, -0.06f, 0.15f, 0f, -0.15f } : new[] { 0f, -0.15f, 0.06f, -0.45f, -0.06f, -0.75f, 0f, -1.05f };
            Sdf wire = Sd.Polyline(0.07f, pts);
            p.Shape(wire, Rgba.White);
            p.Line(Rgba.White.WithAlpha(0.6f), 0.025f, -0.03f, pts[1] * 0.9f, -0.03f, pts[3] * 0.95f);
            p.Shape(Sd.Box(0, top ? -0.15f : -0.15f, 0.04f, 0.04f), Rgba.Hex("E2B13C"), 0.02f);   // rame
            return new DoodleArt(top ? "heist_wire_top" : "heist_wire_bot", p);
        }

        /// <summary>Bigliettino di carta a righe con lo scotch (centro 0,0; 4×1,5).</summary>
        public static DoodleArt HeistNote()
        {
            var p = new DoodlePainter(-2.1f, -0.85f, 2.1f, 0.9f, 80) { Ink = 0.04f };
            Sdf paper = Sd.Rotate(Sd.Box(0, 0, 1.98f, 0.72f, 0.04f), 0, 0, -1.5f);
            p.Shape(paper, Rgba.Hex("FFF8DC"));
            for (float y = 0.4f; y > -0.7f; y -= 0.25f) p.Fill(Sd.Intersect(paper, Sd.Box(0, y, 2f, 0.012f)), Rgba.Hex("A9C7E8"));
            p.Fill(Sd.Intersect(paper, Sd.Box(-1.7f, 0, 0.012f, 0.8f)), Rgba.Hex("F2A0A0"));
            p.Fill(Sd.Rotate(Sd.Box(0, 0.75f, 0.4f, 0.11f), 0, 0.75f, 4f), Rgba.Hex("F5F0C8").WithAlpha(0.8f));
            return new DoodleArt("heist_note", p);
        }

        // tratti delle cifre scritte a mano (riquadro circa ±0,2 × ±0,32)
        static float[][] V40A_DigitStrokes(int d)
        {
            switch (d)
            {
                case 0: return new[] { V40A_EllipseArc(0, 0, 0.17f, 0.3f, 100f, 470f, 28) };
                case 1: return new[] { new[] { -0.09f, 0.18f, 0.02f, 0.3f, 0.02f, -0.3f } };
                case 2: return new[] { new[] { -0.17f, 0.17f, -0.08f, 0.28f, 0.08f, 0.28f, 0.17f, 0.16f, 0.12f, 0.02f, -0.17f, -0.3f, 0.18f, -0.3f } };
                case 3: return new[] { new[] { -0.16f, 0.24f, 0f, 0.3f, 0.15f, 0.2f, 0.12f, 0.06f, -0.03f, 0.01f, 0.14f, -0.06f, 0.17f, -0.2f, 0.02f, -0.31f, -0.17f, -0.24f } };
                case 4: return new[] { new[] { 0.08f, -0.32f, 0.08f, 0.3f, -0.18f, -0.1f, 0.2f, -0.1f } };
                case 5: return new[] { new[] { 0.16f, 0.3f, -0.12f, 0.3f, -0.15f, 0.03f, 0.05f, 0.07f, 0.17f, -0.06f, 0.15f, -0.24f, 0f, -0.31f, -0.17f, -0.24f } };
                case 6: return new[] { new[] { 0.14f, 0.27f, 0f, 0.31f, -0.14f, 0.18f, -0.17f, -0.12f, -0.06f, -0.3f, 0.1f, -0.29f, 0.17f, -0.14f, 0.1f, -0.01f, -0.06f, -0.01f, -0.16f, -0.1f } };
                case 7: return new[] { new[] { -0.17f, 0.29f, 0.18f, 0.29f, 0f, -0.32f }, new[] { -0.06f, 0f, 0.14f, 0f } };
                case 8: return new[] { V40A_EllipseArc(0, 0.15f, 0.12f, 0.14f, 0f, 360f, 20), V40A_EllipseArc(0, -0.15f, 0.15f, 0.16f, 0f, 360f, 22) };
                default: return new[] { new[] { 0.15f, 0.12f, 0.04f, 0f, -0.1f, 0.02f, -0.16f, 0.15f, -0.08f, 0.29f, 0.08f, 0.3f, 0.16f, 0.16f, 0.15f, -0.1f, 0.05f, -0.3f, -0.12f, -0.27f } };
            }
        }

        /// <summary>Cifra d (0-9) scritta con l'inchiostro invisibile: tratto azzurro-viola fluorescente con alone (centro 0,0).</summary>
        public static DoodleArt HeistInkDigit(int d)
        {
            var p = new DoodlePainter(-0.32f, -0.44f, 0.32f, 0.44f, 160) { Wobble = 0.004f };
            float[][] strokes = V40A_DigitStrokes(d);
            var parts = new Sdf[strokes.Length];
            for (int i = 0; i < strokes.Length; i++) parts[i] = Sd.Polyline(0f, strokes[i]);
            Sdf all = Sd.Union(parts);
            p.FillFunc(Sd.Box(0, 0, 0.32f, 0.44f), (x, y) =>
            {
                float dist = Math.Max(0f, all(x, y));
                return Rgba.Hex("B98CFF").WithAlpha(0.55f * (float)Math.Exp(-dist * 22f));
            });
            foreach (float[] s in strokes) p.Line(Rgba.Hex("9CFFF0"), 0.06f, s);
            foreach (float[] s in strokes) p.Line(Rgba.Hex("F2FFFD"), 0.025f, s);
            return new DoodleArt("heist_ink_" + d, p);
        }

        // =====================================================================
        // SUBMARINE
        // =====================================================================

        /// <summary>
        /// Percorso del tunnel subacqueo: terne (y, centro x, mezza larghezza) dall'alto (y 4) verso il basso (y -27),
        /// coordinate del disegno "sub_tunnel". Le usa anche il livello (copiate dal builder) per gli urti.
        /// </summary>
        public static readonly float[] V40A_TunnelPath =
        {
            4f, 0f, 2.6f,
            0f, 0f, 1.9f,
            -3f, 0.6f, 1.45f,
            -6f, -0.5f, 1.3f,
            -9f, -1.0f, 1.25f,
            -12f, 0.3f, 1.2f,
            -15f, 1.0f, 1.25f,
            -18f, 0.2f, 1.15f,
            -21f, -0.7f, 1.25f,
            -24f, 0f, 1.6f,
            -27f, 0f, 2.6f,
        };

        /// <summary>Centro e mezza larghezza del tunnel all'altezza y (interpolazione morbida tra i punti).</summary>
        public static void V40A_TunnelAt(float[] path, float y, out float cx, out float w)
        {
            int n = path.Length / 3;
            if (y >= path[0]) { cx = path[1]; w = path[2]; return; }
            for (int i = 0; i < n - 1; i++)
            {
                float y0 = path[i * 3], y1 = path[i * 3 + 3];
                if (y <= y0 && y >= y1)
                {
                    float t = (y0 - y) / (y0 - y1);
                    t = t * t * (3f - 2f * t);
                    cx = path[i * 3 + 1] + (path[i * 3 + 4] - path[i * 3 + 1]) * t;
                    w = path[i * 3 + 2] + (path[i * 3 + 5] - path[i * 3 + 2]) * t;
                    return;
                }
            }
            cx = path[n * 3 - 2]; w = path[n * 3 - 1];
        }

        /// <summary>Mare aperto (9×14, centro 0,0): blu che scurisce verso il basso, raggi di luce dall'alto.</summary>
        public static DoodleArt SubSea()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 40) { Wobble = 0f };
            p.FillFunc(Sd.Box(0, 0, 4.5f, 7f), (x, y) => Rgba.Lerp(Rgba.Hex("0B2346"), Rgba.Hex("2C7DB8"), V40A_Clamp((y + 7f) / 14f)));
            foreach (float x in new[] { -2.5f, -0.6f, 1.4f, 3.0f })
                p.Fill(Sd.Polygon(x - 0.3f, 7f, x + 0.3f, 7f, x - 0.6f, -3f, x - 1.4f, -3f), Rgba.Hex("BFE8FF").WithAlpha(0.07f), false);
            var rnd = new Random(7);
            for (int i = 0; i < 40; i++)
            {
                float x = -4.4f + (float)rnd.NextDouble() * 8.8f, y = -6.8f + (float)rnd.NextDouble() * 13.6f;
                p.Fill(Sd.Circle(x, y, 0.02f + (float)rnd.NextDouble() * 0.03f), Rgba.Hex("D8F2FF").WithAlpha(0.35f), false);
            }
            return new DoodleArt("sub_sea_bg", p);
        }

        /// <summary>Le pareti di roccia del tunnel (da x -3,6 a 3,6; da y 5 a -28), percorso in V40A_TunnelPath.</summary>
        public static DoodleArt SubTunnel()
        {
            var p = new DoodlePainter(-3.6f, -28.5f, 3.6f, 5f, 30) { Ink = 0.07f, Wobble = 0.03f, WobbleFreq = 1.6f };
            float[] path = V40A_TunnelPath;
            Sdf rock = (x, y) => { V40A_TunnelAt(path, y, out float cx, out float w); return w - Math.Abs(x - cx); };
            p.FillFunc(rock, (x, y) =>
            {
                float n = Noise.Value(x * 1.3f, y * 1.3f);
                return Rgba.Lerp(Rgba.Hex("3B3550"), Rgba.Hex("5A4F6E"), n);
            });
            p.Hatch(Sd.Intersect(rock, (x, y) => -rock(x, y) - 0.5f), Rgba.Hex("2F2A42"), 0.3f, 0.05f, 60f);
            // bordo chiaro verso l'acqua
            p.Fill(Sd.Intersect(rock, (x, y) => -rock(x, y) - 0.18f), Rgba.Hex("7A6C93"));
            p.Fill(Sd.Intersect(rock, (x, y) => -rock(x, y) - 0.09f), Rgba.Hex("5A4F6E"));
            p.Shape(rock, Rgba.Clear);
            // alghe e coralli sui bordi
            var rnd = new Random(40);
            for (float y = 2f; y > -27f; y -= 1.3f)
            {
                V40A_TunnelAt(path, y, out float cx, out float w);
                float s = rnd.Next(2) == 0 ? -1f : 1f;
                float bx = cx + s * (w + 0.02f);
                if (rnd.Next(2) == 0)
                    p.Line(Rgba.Hex("4CC27A"), 0.07f, bx, y, bx - s * 0.18f, y + 0.2f, bx - s * 0.08f, y + 0.4f, bx - s * 0.22f, y + 0.6f);
                else
                {
                    p.Shape(Sd.Circle(bx - s * 0.1f, y, 0.11f), Rgba.Hex("FF7F7F"), 0.035f);
                    p.Shape(Sd.Circle(bx - s * 0.06f, y + 0.2f, 0.08f), Rgba.Hex("FF9F6E"), 0.03f);
                }
            }
            return new DoodleArt("sub_tunnel", p);
        }

        /// <summary>Sottomarino giallo visto di lato (centro 0,0; 1,3×0,9), muso a destra, oblò e periscopio.</summary>
        public static DoodleArt SubSubmarine()
        {
            var p = new DoodlePainter(-0.72f, -0.4f, 0.72f, 0.58f, 140) { Ink = 0.04f };
            Rgba y0 = Rgba.Hex("FFD23F"), y1 = Rgba.Hex("E0A91E");
            p.Shape(Sd.Union(Sd.Box(-0.62f, 0f, 0.05f, 0.2f, 0.03f), Sd.Polygon(-0.6f, 0f, -0.68f, 0.18f, -0.68f, -0.18f)), Rgba.Hex("8A93A6"), 0.03f);
            p.InkLine(0.03f, 0.05f, 0.25f, 0.05f, 0.48f, 0.22f, 0.48f);
            p.Shape(Sd.Box(0.24f, 0.48f, 0.05f, 0.04f), Rgba.Hex("8A93A6"), 0.02f);
            Body(p, Sd.Box(-0.05f, 0.24f, 0.2f, 0.1f, 0.06f), y0, y1);
            Body(p, Sd.Ellipse(0, -0.02f, 0.58f, 0.27f), y0, y1);
            foreach (float x in new[] { -0.2f, 0.1f, 0.38f })
            {
                p.Shape(Sd.Circle(x, 0f, 0.09f), Rgba.Hex("BFE8FF"), 0.03f);
                p.Fill(Sd.Circle(x - 0.025f, 0.025f, 0.025f), Rgba.White);
            }
            return new DoodleArt("sub_sub", p);
        }

        /// <summary>Fondale (9×14, centro 0,0): acqua profonda, sabbia da y -2,4 con sassolini e alghe.</summary>
        public static DoodleArt SubSeafloor()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 40) { Wobble = 0.01f };
            p.FillFunc(Sd.Box(0, 0, 4.5f, 7f), (x, y) => Rgba.Lerp(Rgba.Hex("0A1C3A"), Rgba.Hex("1E5A8E"), V40A_Clamp((y + 3f) / 10f)));
            foreach (float x in new[] { -2f, 0.5f, 2.6f })
                p.Fill(Sd.Polygon(x - 0.25f, 7f, x + 0.25f, 7f, x - 0.4f, -2.4f, x - 1.2f, -2.4f), Rgba.Hex("BFE8FF").WithAlpha(0.06f), false);
            Sdf sand = (x, y) => y - (-2.3f + 0.15f * (float)Math.Sin(x * 1.3f));
            p.FillFunc(sand, (x, y) => Rgba.Lerp(Rgba.Hex("B59A6A"), Rgba.Hex("D9C08E"), V40A_Clamp((y + 7f) / 4.7f)));
            p.Shape(sand, Rgba.Clear, 0.05f);
            var rnd = new Random(9);
            for (int i = 0; i < 30; i++)
            {
                float x = -4.3f + (float)rnd.NextDouble() * 8.6f, y = -6.5f + (float)rnd.NextDouble() * 4f;
                p.Fill(Sd.Ellipse(x, y, 0.06f, 0.035f), Rgba.Hex("9C8158"));
            }
            foreach (float x in new[] { -3.6f, -2.9f, 3.1f, 3.8f })
                p.Line(Rgba.Hex("3FAE6A"), 0.08f, x, -2.4f, x + 0.15f, -1.8f, x - 0.1f, -1.2f, x + 0.12f, -0.6f);
            return new DoodleArt("sub_seafloor_bg", p);
        }

        /// <summary>Relitto di una nave inclinata nella sabbia (scafo centrato in 0,0, largo 4); albero spezzato fino a (0,4, 2,7).</summary>
        public static DoodleArt SubWreck()
        {
            var p = new DoodlePainter(-2.2f, -1.0f, 2.2f, 2.85f, 70) { Ink = 0.055f };
            p.Shape(Sd.Rotate(Sd.Box(0.4f, 1.3f, 0.07f, 1.4f), 0.4f, 0f, -6f), Rgba.Hex("6B4A34"));
            p.Shape(Sd.Rotate(Sd.Box(0.55f, 1.9f, 0.6f, 0.05f), 0.4f, 0f, -6f), Rgba.Hex("6B4A34"), 0.04f);
            Sdf hull = Sd.Rotate(Sd.Polygon(-2.0f, 0.5f, 2.0f, 0.5f, 1.5f, -0.65f, -1.6f, -0.65f), 0, 0, -6f);
            Body(p, hull, Rgba.Hex("7A5A3E"), Rgba.Hex("5C412C"));
            p.Hatch(hull, Rgba.Hex("5C412C"), 0.25f, 0.03f, -6f);
            p.Fill(Sd.Rotate(Sd.Circle(-0.9f, 0.05f, 0.25f), 0, 0, -6f), Rgba.Hex("1B1424"));   // buco nello scafo
            foreach (float x in new[] { -0.2f, 0.5f, 1.2f }) p.Shape(Sd.Rotate(Sd.Circle(x, 0.15f, 0.1f), 0, 0, -6f), Rgba.Hex("3A6E8F"), 0.03f);
            p.Line(Rgba.Hex("3FAE6A"), 0.06f, -1.3f, 0.4f, -1.2f, 0.8f, -1.35f, 1.1f);
            p.Fill(Sd.Box(0, -0.85f, 2.2f, 0.18f), Rgba.Hex("C9AD7C"));
            return new DoodleArt("sub_wreck", p);
        }

        /// <summary>Bandiera strappata (asta a sinistra in 0,0; telo da x 0 a 1,7 e da y -0,95 a 0): ospita 3 simboli.</summary>
        public static DoodleArt SubFlag()
        {
            var p = new DoodlePainter(-0.08f, -1.05f, 1.85f, 0.08f, 100) { Ink = 0.045f };
            Sdf cloth = Sd.Polygon(0f, 0f, 0.6f, -0.06f, 1.2f, 0.02f, 1.75f, -0.05f, 1.6f, -0.3f, 1.72f, -0.5f, 1.58f, -0.7f, 1.75f, -0.92f,
                1.2f, -0.85f, 0.6f, -0.95f, 0f, -0.9f);
            p.Shape(cloth, Rgba.Hex("F2EBD8"));
            p.Fill(Sd.Intersect(cloth, Sd.Box(0.85f, -0.47f, 0.95f, 0.035f)), Rgba.Hex("D9CFB6"));
            return new DoodleArt("sub_flag", p);
        }

        /// <summary>Masso del fondale (base a y 0, largo circa 1,1); due forme.</summary>
        public static DoodleArt SubRock(int kind)
        {
            var p = new DoodlePainter(-0.65f, -0.1f, 0.65f, 0.95f, 110) { Ink = 0.045f };
            Sdf rock = kind == 0
                ? Sd.Union(Sd.Ellipse(0, 0.35f, 0.58f, 0.4f), Sd.Ellipse(0.15f, 0.55f, 0.35f, 0.3f))
                : Sd.Union(Sd.Ellipse(0, 0.32f, 0.55f, 0.36f), Sd.Ellipse(-0.18f, 0.55f, 0.3f, 0.32f), Sd.Ellipse(0.25f, 0.45f, 0.25f, 0.24f));
            Body(p, rock, Rgba.Hex(kind == 0 ? "6E6A80" : "7A7088"), Rgba.Hex("524D63"), Rgba.Hex("45405A"));
            p.Fill(Sd.Ellipse(kind == 0 ? -0.2f : 0.1f, 0.6f, 0.08f, 0.04f), Rgba.White.WithAlpha(0.25f));
            p.Line(Rgba.Hex("3FAE6A"), 0.05f, 0.4f, 0.05f, 0.45f, 0.3f, 0.38f, 0.5f);
            return new DoodleArt("sub_rock_" + kind, p);
        }

        /// <summary>Murena che sbuca a bocca aperta (base del corpo in 0,0, testa in alto a destra verso 0,4, 0,9).</summary>
        public static DoodleArt SubEel()
        {
            var p = new DoodlePainter(-0.35f, -0.1f, 0.8f, 1.15f, 120) { Ink = 0.04f };
            Rgba g = Rgba.Hex("7FB24A"), gs = Rgba.Hex("5E8F33");
            Body(p, Sd.Polyline(0.13f, 0f, 0f, 0.05f, 0.35f, 0.2f, 0.65f), g, gs);
            Sdf head = Sd.Rotate(Sd.Ellipse(0.42f, 0.82f, 0.3f, 0.2f), 0.42f, 0.82f, 20f);
            Body(p, head, g, gs);
            p.Fill(Sd.Rotate(Sd.Polygon(0.45f, 0.78f, 0.75f, 0.95f, 0.75f, 0.68f), 0.42f, 0.82f, 20f), Rgba.Hex("6B1A2A"));
            for (int i = 0; i < 3; i++) p.Fill(Sd.Rotate(Sd.Polygon(0.52f + i * 0.07f, 0.84f, 0.555f + i * 0.07f, 0.79f, 0.59f + i * 0.07f, 0.85f), 0.42f, 0.82f, 20f), Rgba.White);
            p.Shape(Sd.Circle(0.38f, 0.93f, 0.06f), Rgba.Hex("FFE36E"), 0.02f);
            p.Fill(Sd.Circle(0.39f, 0.93f, 0.025f), V40A_Ink);
            return new DoodleArt("sub_eel", p);
        }

        /// <summary>Simbolo a inchiostro (centro 0,0, circa 0,55): 0 pesce, 1 ancora, 2 stella, 3 conchiglia.</summary>
        public static DoodleArt SubSymbol(int kind)
        {
            var p = new DoodlePainter(-0.32f, -0.32f, 0.32f, 0.32f, 200) { Ink = 0.035f };
            string[] names = { "sub_sym_fish", "sub_sym_anchor", "sub_sym_star", "sub_sym_shell" };
            switch (kind)
            {
                case 0:
                    p.Shape(Sd.Union(Sd.Ellipse(0.05f, 0, 0.2f, 0.13f), Sd.Polygon(-0.12f, 0f, -0.28f, 0.13f, -0.28f, -0.13f)), Rgba.Hex("4DA3F0"));
                    p.Fill(Sd.Circle(0.14f, 0.03f, 0.03f), V40A_Ink);
                    break;
                case 1:
                    p.InkLine(0.06f, 0f, 0.2f, 0f, -0.24f);
                    p.InkLine(0.06f, -0.12f, 0.1f, 0.12f, 0.1f);
                    p.Line(V40A_Ink, 0.06f, V40A_EllipseArc(0, -0.08f, 0.2f, 0.17f, 190f, 350f, 14));
                    p.Shape(Sd.Subtract(Sd.Circle(0, 0.25f, 0.06f), Sd.Circle(0, 0.25f, 0.025f)), Rgba.Hex("8A93A6"), 0.02f);
                    break;
                case 2:
                    var pts = new float[20];
                    for (int i = 0; i < 10; i++)
                    {
                        double a = Math.PI / 2 + i * Math.PI / 5;
                        float r = i % 2 == 0 ? 0.28f : 0.12f;
                        pts[i * 2] = r * (float)Math.Cos(a); pts[i * 2 + 1] = r * (float)Math.Sin(a);
                    }
                    p.Shape(Sd.Polygon(pts), Rgba.Hex("FFC93C"));
                    break;
                default:
                    Sdf shell = Sd.Union(Sd.Intersect(Sd.Circle(0, -0.08f, 0.3f), Sd.Box(0, 0.06f, 0.3f, 0.16f)), Sd.Box(0, -0.15f, 0.08f, 0.06f));
                    p.Shape(shell, Rgba.Hex("F7A8B8"));
                    for (int i = -2; i <= 2; i++) p.InkLine(0.02f, 0f, -0.08f, i * 0.11f, 0.18f - Math.Abs(i) * 0.03f);
                    break;
            }
            return new DoodleArt(names[kind], p);
        }

        /// <summary>Rotella in ottone della combinazione (centro 0,0, raggio 0,5) con finestrella chiara al centro.</summary>
        public static DoodleArt SubDial()
        {
            var p = new DoodlePainter(-0.55f, -0.55f, 0.55f, 0.55f, 140) { Ink = 0.045f };
            Body(p, Sd.Circle(0, 0, 0.5f), Rgba.Hex("E2B13C"), Rgba.Hex("B8862A"));
            for (int i = 0; i < 12; i++)
            {
                double a = i * Math.PI / 6;
                p.InkLine(0.025f, 0.42f * (float)Math.Cos(a), 0.42f * (float)Math.Sin(a), 0.48f * (float)Math.Cos(a), 0.48f * (float)Math.Sin(a));
            }
            p.Shape(Sd.Circle(0, 0, 0.35f), Rgba.Hex("FFF6DC"), 0.035f);
            return new DoodleArt("sub_dial", p);
        }

        /// <summary>Piastra della serratura (centro 0,0; 3,8×1,5) con tre incavi per le rotelle (x -1,2, 0, 1,2).</summary>
        public static DoodleArt SubLockPlate()
        {
            var p = new DoodlePainter(-2.0f, -0.82f, 2.0f, 0.82f, 90) { Ink = 0.05f };
            Body(p, Sd.Box(0, 0, 1.9f, 0.72f, 0.18f), Rgba.Hex("8C6A3E"), Rgba.Hex("6E5130"));
            foreach (float x in new[] { -1.2f, 0f, 1.2f }) p.Shape(Sd.Circle(x, 0, 0.56f), Rgba.Hex("4E3A22"), 0.035f);
            foreach (float s in new[] { -1f, 1f }) foreach (float t in new[] { -1f, 1f })
                p.Shape(Sd.Circle(s * 1.75f, t * 0.55f, 0.06f), Rgba.Hex("E2B13C"), 0.02f);
            return new DoodleArt("sub_lock_plate", p);
        }

        /// <summary>Pulsante tondo verde del sonar (centro 0,0, raggio 0,55) con le onde disegnate sopra.</summary>
        public static DoodleArt SubPingButton()
        {
            var p = new DoodlePainter(-0.62f, -0.66f, 0.62f, 0.6f, 140) { Ink = 0.045f };
            p.Shape(Sd.Circle(0, -0.07f, 0.55f), Rgba.Hex("2A6E4A"));
            Body(p, Sd.Circle(0, 0, 0.53f), Rgba.Hex("4CD38A"), Rgba.Hex("2FAF6C"));
            p.Fill(Sd.Circle(0, 0, 0.06f), Rgba.Hex("E9FFF2"));
            foreach (float r in new[] { 0.18f, 0.3f })
                p.Line(Rgba.Hex("E9FFF2"), 0.035f, V40A_EllipseArc(0, 0, r, r, 20f, 160f, 10));
            return new DoodleArt("sub_ping_btn", p);
        }

        /// <summary>Anello sottile del sonar (centro 0,0, raggio 1), verde, da scalare.</summary>
        public static DoodleArt SubRing()
        {
            var p = new DoodlePainter(-1.05f, -1.05f, 1.05f, 1.05f, 100) { Wobble = 0f };
            p.FillFunc(Sd.Circle(0, 0, 1.04f), (x, y) =>
            {
                float d = Math.Abs((float)Math.Sqrt(x * x + y * y) - 0.98f);
                return Rgba.Hex("7CFFB2").WithAlpha((float)Math.Exp(-d * 60f));
            });
            return new DoodleArt("sub_ring", p);
        }

        /// <summary>Bollicina d'aria (centro 0,0, raggio 0,1).</summary>
        public static DoodleArt SubBubble()
        {
            var p = new DoodlePainter(-0.12f, -0.12f, 0.12f, 0.12f, 200) { Ink = 0.02f, InkColor = Rgba.Hex("D8F2FF"), Wobble = 0.003f };
            p.Shape(Sd.Circle(0, 0, 0.09f), Rgba.Hex("BFE8FF").WithAlpha(0.25f));
            p.Fill(Sd.Circle(-0.03f, 0.03f, 0.025f), Rgba.White.WithAlpha(0.9f));
            return new DoodleArt("sub_bubble", p);
        }

        // =====================================================================
        // HAUNTED HOUSE
        // =====================================================================

        /// <summary>
        /// Salone stregato (9×14, centro 0,0): carta da parati viola con motivi, ragnatele, quadri storti, pavimento
        /// di assi da y -0,7. Le porte e le candele sono a parte.
        /// </summary>
        public static DoodleArt HauntHall()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Wobble = 0.008f };
            p.FillFunc(Sd.Box(0, 3.15f, 4.5f, 3.85f), (x, y) => Rgba.Lerp(Rgba.Hex("4A3A66"), Rgba.Hex("5E4A80"), V40A_Clamp((y + 0.7f) / 7f)));
            for (float x = -4.2f; x < 4.5f; x += 0.9f)
                for (float y = -0.2f; y < 7f; y += 1.0f)
                {
                    float ox = ((int)Math.Round((y + 0.2f) / 1.0f) % 2) * 0.45f;
                    p.Fill(Sd.Union(Sd.Ellipse(x + ox, y, 0.1f, 0.18f), Sd.Ellipse(x + ox, y, 0.18f, 0.06f)), Rgba.Hex("6B5690"));
                }
            p.Fill(Sd.Box(0, -0.6f, 4.5f, 0.12f), Rgba.Hex("3A2D50"), false);
            p.InkLine(0.04f, -4.5f, -0.72f, 4.5f, -0.72f);
            p.FillFunc(Sd.Box(0, -3.86f, 4.5f, 3.14f), (x, y) => Rgba.Lerp(Rgba.Hex("3E2C26"), Rgba.Hex("5A4034"), V40A_Clamp((y + 7f) / 6.3f)));
            for (float x = -4.5f; x < 4.6f; x += 0.75f) p.Line(Rgba.Hex("2E201B"), 0.035f, x, -0.75f, x * 1.6f, -7f);
            // quadri storti
            p.Shape(Sd.Rotate(Sd.Box(-2.75f, 2.6f, 0.4f, 0.5f), -2.75f, 2.6f, 8f), Rgba.Hex("3B3050"), 0.07f);
            p.Fill(Sd.Rotate(Sd.Ellipse(-2.75f, 2.55f, 0.18f, 0.24f), -2.75f, 2.6f, 8f), Rgba.Hex("8A7FA0"));
            p.Shape(Sd.Rotate(Sd.Box(2.75f, 2.4f, 0.45f, 0.35f), 2.75f, 2.4f, -10f), Rgba.Hex("3B3050"), 0.07f);
            p.Fill(Sd.Rotate(Sd.Circle(2.7f, 2.38f, 0.15f), 2.75f, 2.4f, -10f), Rgba.Hex("D9D2E6"));
            // ragnatele negli angoli
            foreach (float s in new[] { -1f, 1f })
            {
                float cx = s * 4.5f, cy = 7f;
                for (int i = 0; i < 5; i++)
                {
                    double a = (s < 0 ? -Math.PI / 2 : Math.PI) + i * Math.PI / 8 * (s < 0 ? 1 : 1);
                    p.Line(Rgba.Hex("C9C2D9").WithAlpha(0.6f), 0.02f, cx, cy, cx + 2.2f * (float)Math.Cos(a), cy + 2.2f * (float)Math.Sin(a));
                }
                for (float r = 0.5f; r < 2.2f; r += 0.45f)
                    p.Line(Rgba.Hex("C9C2D9").WithAlpha(0.5f), 0.02f, V40A_EllipseArc(cx, cy, r, r, s < 0 ? 270f : 180f, s < 0 ? 360f : 270f, 8));
            }
            // tappeto lungo
            p.Shape(Sd.Polygon(-1.0f, -0.8f, 1.0f, -0.8f, 1.6f, -7f, -1.6f, -7f), Rgba.Hex("7A2E45"), 0.05f);
            p.Fill(Sd.Polygon(-0.75f, -0.9f, 0.75f, -0.9f, 1.2f, -7f, -1.2f, -7f), Rgba.Hex("8E3A54"));
            return new DoodleArt("haunt_hall_bg", p);
        }

        /// <summary>Candela bianca con colature in un portacandela d'ottone (base in 0,0; stoppino in cima a 0, 0,72).</summary>
        public static DoodleArt HauntCandle()
        {
            var p = new DoodlePainter(-0.4f, -0.05f, 0.4f, 0.8f, 150) { Ink = 0.035f };
            Body(p, Sd.Ellipse(0, 0.08f, 0.33f, 0.08f), Rgba.Hex("E2B13C"), Rgba.Hex("B8862A"));
            Body(p, Sd.Box(0, 0.16f, 0.12f, 0.06f, 0.02f), Rgba.Hex("E2B13C"), Rgba.Hex("B8862A"));
            Sdf wax = Sd.Union(Sd.Box(0, 0.42f, 0.11f, 0.24f, 0.03f), Sd.Capsule(-0.11f, 0.6f, -0.11f, 0.42f, 0.035f), Sd.Capsule(0.08f, 0.62f, 0.08f, 0.35f, 0.03f));
            Body(p, wax, Rgba.Hex("F7F2E6"), Rgba.Hex("DCD3BE"));
            p.InkLine(0.025f, 0f, 0.66f, 0.01f, 0.73f);
            p.Shape(Sd.Capsule(0.3f, 0.12f, 0.36f, 0.22f, 0.04f), Rgba.Hex("E2B13C"), 0.025f);   // manico
            return new DoodleArt("haunt_candle", p);
        }

        /// <summary>Scatola di fiammiferi (centro 0,0; 1,9×0,9) con la striscia ruvida marrone in alto (y da 0,2 a 0,4).</summary>
        public static DoodleArt HauntMatchbox()
        {
            var p = new DoodlePainter(-1.0f, -0.5f, 1.0f, 0.5f, 110) { Ink = 0.045f };
            Body(p, Sd.Box(0, -0.02f, 0.92f, 0.43f, 0.05f), Rgba.Hex("E8453C"), Rgba.Hex("C42F2A"));
            Sdf strip = Sd.Box(0, 0.3f, 0.86f, 0.09f, 0.03f);
            p.Fill(strip, Rgba.Hex("7A4A2E"));
            var rnd = new Random(3);
            for (int i = 0; i < 60; i++)
                p.Fill(Sd.Circle(-0.82f + (float)rnd.NextDouble() * 1.64f, 0.23f + (float)rnd.NextDouble() * 0.14f, 0.012f), Rgba.Hex("4E2E1C"));
            p.Shape(strip, Rgba.Clear, 0.03f);
            p.Shape(Sd.Box(0, -0.12f, 0.5f, 0.2f, 0.05f), Rgba.Hex("FFE36E"), 0.03f);
            p.Shape(Sd.Capsule(-0.25f, -0.12f, 0.2f, -0.12f, 0.03f), Rgba.Hex("E8C27A"), 0.02f);
            p.Shape(Sd.Circle(0.26f, -0.12f, 0.06f), Rgba.Hex("E8453C"), 0.02f);
            return new DoodleArt("haunt_matchbox", p);
        }

        /// <summary>Fiammifero in piedi (capocchia in alto in 0,0; bastoncino fino a y -1,0). burnt = capocchia nera, legno annerito.</summary>
        public static DoodleArt HauntMatch(bool burnt)
        {
            var p = new DoodlePainter(-0.14f, -1.05f, 0.14f, 0.14f, 180) { Ink = 0.03f };
            p.Shape(Sd.Box(0, -0.5f, 0.045f, 0.48f, 0.02f), Rgba.Hex(burnt ? "8A6A4A" : "E8C27A"));
            if (burnt) p.Fill(Sd.Box(0, -0.15f, 0.04f, 0.15f), Rgba.Hex("3A2A20"));
            p.Shape(Sd.Ellipse(0, -0.02f, 0.08f, 0.11f), Rgba.Hex(burnt ? "2B2133" : "E8453C"));
            if (!burnt) p.Fill(Sd.Circle(-0.025f, 0.02f, 0.025f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt(burnt ? "haunt_match_burnt" : "haunt_match", p);
        }

        /// <summary>Fantasma (centro 0,0, circa 1,6×2): 0 calmo, 1 che saluta col braccio alzato, 2 "BOO!" a bocca spalancata.</summary>
        public static DoodleArt HauntGhost(int mood)
        {
            var p = new DoodlePainter(-1.05f, -1.1f, 1.05f, 1.15f, 110) { Ink = 0.05f };
            Rgba body = Rgba.Hex("F4F1FF"), shade = Rgba.Hex("CFC8EC");
            Sdf ghost = Sd.Union(Sd.Circle(0, 0.3f, 0.7f), Sd.Box(0, -0.3f, 0.7f, 0.6f),
                Sd.Circle(-0.47f, -0.9f, 0.23f), Sd.Circle(0f, -0.92f, 0.24f), Sd.Circle(0.47f, -0.9f, 0.23f));
            // braccini
            Sdf armL = Sd.Ellipse(-0.75f, -0.15f, 0.22f, 0.12f);
            Sdf armR = mood == 1 ? Sd.Rotate(Sd.Ellipse(0.82f, 0.45f, 0.24f, 0.12f), 0.82f, 0.45f, 60f) : Sd.Ellipse(0.75f, -0.15f, 0.22f, 0.12f);
            if (mood == 2) { armL = Sd.Rotate(Sd.Ellipse(-0.85f, 0.4f, 0.26f, 0.13f), -0.85f, 0.4f, -55f); armR = Sd.Rotate(Sd.Ellipse(0.85f, 0.4f, 0.26f, 0.13f), 0.85f, 0.4f, 55f); }
            Body(p, armL, body, shade);
            Body(p, armR, body, shade);
            p.Fill(ghost, body);
            p.Fill(Sd.Crescent(ghost, -0.1f, 0.08f), shade);
            p.Shape(ghost, Rgba.Clear);
            if (mood == 2)
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    p.Fill(Sd.Ellipse(s * 0.25f, 0.45f, 0.13f, 0.17f), V40A_Ink);
                    p.InkLine(0.05f, s * 0.08f, 0.68f, s * 0.38f, 0.75f);
                }
                p.Fill(Sd.Ellipse(0, -0.1f, 0.25f, 0.32f), V40A_Ink);
                p.Fill(Sd.Ellipse(0, -0.25f, 0.13f, 0.12f), Rgba.Hex("7A2E45"));
            }
            else
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    p.Fill(Sd.Ellipse(s * 0.24f, 0.42f, 0.1f, 0.14f), V40A_Ink);
                    p.Fill(Sd.Circle(s * 0.24f - 0.03f, 0.47f, 0.035f), Rgba.White);
                    p.Fill(Sd.Ellipse(s * 0.42f, 0.18f, 0.1f, 0.05f), Rgba.Hex("FF9FB8").WithAlpha(0.7f));
                }
                if (mood == 1) p.Shape(Sd.Intersect(Sd.Circle(0, 0.2f, 0.15f), Sd.Box(0, 0.1f, 0.2f, 0.1f)), Rgba.Hex("7A2E45"), 0.03f);
                else p.Fill(Sd.Ellipse(0, 0.12f, 0.06f, 0.08f), V40A_Ink);
            }
            return new DoodleArt(mood == 0 ? "haunt_ghost" : mood == 1 ? "haunt_ghost_wave" : "haunt_ghost_boo", p);
        }

        /// <summary>Due occhi gialli nel buio (centro 0,0, larghi 0,6): un'esca mentre si cercano le candele.</summary>
        public static DoodleArt HauntEyes()
        {
            var p = new DoodlePainter(-0.4f, -0.2f, 0.4f, 0.2f, 160) { Ink = 0.03f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Ellipse(s * 0.18f, 0, 0.16f, 0.13f), Rgba.Hex("FFE36E").WithAlpha(0.35f));
                p.Shape(Sd.Intersect(Sd.Ellipse(s * 0.18f, 0, 0.12f, 0.09f), Sd.Box(s * 0.18f, -0.03f, 0.13f, 0.09f)), Rgba.Hex("FFE36E"));
                p.Fill(Sd.Ellipse(s * 0.18f, -0.02f, 0.025f, 0.06f), V40A_Ink);
            }
            return new DoodleArt("haunt_eyes", p);
        }

        /// <summary>Porta ad arco di legno scuro (base in 0,0; larga 1,3, alta 2,2) con targhetta vuota in alto (0, 1,75).</summary>
        public static DoodleArt HauntDoor()
        {
            var p = new DoodlePainter(-0.75f, -0.08f, 0.75f, 2.3f, 100) { Ink = 0.05f };
            Sdf door = Sd.Union(Sd.Box(0, 0.75f, 0.62f, 0.75f), Sd.Circle(0, 1.5f, 0.62f));
            p.Shape(Sd.Grow(door, 0.08f), Rgba.Hex("2E2238"));
            Body(p, door, Rgba.Hex("6B4A34"), Rgba.Hex("50362A"));
            foreach (float x in new[] { -0.3f, 0f, 0.3f }) p.Fill(Sd.Intersect(door, Sd.Box(x, 1f, 0.015f, 1.2f)), Rgba.Hex("50362A"));
            p.Shape(Sd.Circle(0.4f, 0.8f, 0.07f), Rgba.Hex("E2B13C"), 0.025f);
            p.Shape(Sd.Box(0, 1.65f, 0.48f, 0.2f, 0.05f), Rgba.Hex("E8DCC0"), 0.03f);
            return new DoodleArt("haunt_door", p);
        }

        /// <summary>Vano della porta aperta (stesse misure di haunt_door): luce calda dall'altra parte.</summary>
        public static DoodleArt HauntDoorOpen()
        {
            var p = new DoodlePainter(-0.75f, -0.08f, 0.75f, 2.3f, 100) { Ink = 0.05f };
            Sdf door = Sd.Union(Sd.Box(0, 0.75f, 0.62f, 0.75f), Sd.Circle(0, 1.5f, 0.62f));
            p.Shape(Sd.Grow(door, 0.08f), Rgba.Hex("2E2238"));
            p.FillFunc(door, (x, y) => Rgba.Lerp(Rgba.Hex("FFF3C4"), Rgba.Hex("FFC768"), V40A_Clamp(y / 2.1f)));
            p.Shape(door, Rgba.Clear);
            p.Shape(Sd.Box(0, 1.65f, 0.48f, 0.2f, 0.05f), Rgba.Hex("E8DCC0"), 0.03f);
            return new DoodleArt("haunt_door_open", p);
        }
    }
}
