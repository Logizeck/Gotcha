using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli nuovi della versione 0.39 (gruppo A).</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV39A() => new DoodleArt[]
        {
            AtticBackground(), BulbBoard(), Bulb(false), Bulb(true), AtticMap(), ResetButton(),
            GlassTube(), TubeShine(), TubeBall("tube_ball_red", Rgba.Hex("F0524A"), Rgba.Hex("C0352F")),
            TubeBall("tube_ball_blue", Rgba.Hex("4A9BEF"), Rgba.Hex("2D6FC4")), TubeBall("tube_ball_yellow", Rgba.Hex("FFD23F"), Rgba.Hex("E0A51C")),
            HamsterBall(), ToyShelfBackground(),
            Jug(5), Jug(3), DrainSink(), CatBowl(), BowlWater(),
            YarnButton(), YarnRugBackground(),
            PaperSheet(), InkDot(), HouseFly(),
        };

        // ---------- Bulb grid: la soffitta ----------

        /// <summary>Soffitta di legno con tetto a capanna, travi, ragnatele e scatoloni (sfondo 9 x 14).</summary>
        public static DoodleArt AtticBackground()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            Sdf all = Sd.Box(0, 0, 4.5f, 7f);
            // parete di assi verticali
            p.FillFunc(all, (x, y) => ((int)Math.Floor((x + 9f) / 0.7f) & 1) == 0 ? Rgba.Hex("B98B5E") : Rgba.Hex("AD7F53"));
            for (float x = -4.2f; x < 4.5f; x += 0.7f) p.Line(Ink.WithAlpha(0.35f), 0.03f, x, -2.6f, x, 7f);
            // tetto a capanna: spioventi scuri in alto
            foreach (float s in new[] { -1f, 1f })
            {
                Sdf roof = Sd.Polygon(s * 4.6f, 7.1f, s * 4.6f, 1.2f, 0f, 6.2f, 0f, 7.1f);
                p.Fill(roof, Rgba.Hex("6B4730"));
                p.Hatch(roof, Rgba.Hex("5A3A26"), 0.45f, 0.08f, s * 30f);
                p.Line(Ink, 0.22f, s * 4.6f, 1.2f, 0f, 6.2f);
                p.Line(Rgba.Hex("8A5C3B"), 0.14f, s * 4.6f, 1.2f, 0f, 6.2f);
            }
            // trave orizzontale
            p.Shape(Sd.Box(0, 4.3f, 4.6f, 0.13f), Rgba.Hex("8A5C3B"));
            // pavimento
            Sdf floor = Sd.Box(0, -5f, 5f, 2.4f);
            p.Fill(floor, Rgba.Hex("9C6B42"), false);
            foreach (float y in new[] { -3.3f, -4.1f, -4.9f, -5.8f }) p.Line(Ink.WithAlpha(0.45f), 0.03f, -4.5f, y, 4.5f, y + 0.03f);
            p.Shape(Sd.Box(0, -2.6f, 5f, 0.09f), Rgba.Hex("6B4730"));
            // scatoloni a sinistra e baule a destra
            Body(p, Sd.Box(-2.35f, -2.05f, 0.5f, 0.45f, 0.03f), Rgba.Hex("D9A866"), Rgba.Hex("B98745"));
            p.Line(Rgba.Hex("B98745"), 0.08f, -2.35f, -1.6f, -2.35f, -2.5f);
            Body(p, Sd.Box(-2.5f, -1.3f, 0.35f, 0.3f, 0.03f), Rgba.Hex("E2B677"), Rgba.Hex("C09050"));
            Body(p, Sd.Box(2.35f, -2.15f, 0.55f, 0.38f, 0.08f), Rgba.Hex("7A4A9A"), Rgba.Hex("5C3478"));
            p.Shape(Sd.Box(2.35f, -1.95f, 0.57f, 0.06f), Gold, 0.03f);
            p.Shape(Sd.Box(2.35f, -2.05f, 0.07f, 0.1f, 0.02f), Gold, 0.025f);
            // ragnatele negli angoli sotto il tetto
            foreach (float s in new[] { -1f, 1f })
            {
                float cx = s * 2.9f, cy = 3.35f;
                for (int i = 0; i < 4; i++)
                {
                    float a = (float)((s > 0 ? 200 : -20) + s * i * 22) * (float)Math.PI / 180f;
                    p.Line(Rgba.White.WithAlpha(0.5f), 0.02f, cx, cy, cx + (float)Math.Cos(a) * 1.1f, cy + (float)Math.Sin(a) * 1.1f);
                }
                for (int r = 1; r <= 3; r++)
                {
                    var pts = new List<float>();
                    for (int i = 0; i < 4; i++)
                    {
                        float a = (float)((s > 0 ? 200 : -20) + s * i * 22) * (float)Math.PI / 180f;
                        pts.Add(cx + (float)Math.Cos(a) * r * 0.33f); pts.Add(cy + (float)Math.Sin(a) * r * 0.33f);
                    }
                    p.Line(Rgba.White.WithAlpha(0.4f), 0.018f, pts.ToArray());
                }
            }
            return new DoodleArt("attic_bg", p);
        }

        /// <summary>Tavola di legno con nove portalampade (griglia 3x3, passo 1.4, centro 0,0).</summary>
        public static DoodleArt BulbBoard()
        {
            var p = new DoodlePainter(-2.35f, -2.35f, 2.35f, 2.35f, 90) { Ink = 0.06f };
            Sdf board = Sd.Box(0, 0, 2.2f, 2.2f, 0.18f);
            Body(p, board, Rgba.Hex("8A5C3B"), Rgba.Hex("6B4730"));
            p.Hatch(board, Rgba.Hex("7A5034", 0.6f), 0.35f, 0.05f, 4f);
            p.Shape(board, Rgba.Clear);
            foreach (float s in new[] { -1f, 1f }) foreach (float t in new[] { -1f, 1f })
                    p.Shape(Sd.Circle(s * 1.95f, t * 1.95f, 0.07f), Steel, 0.025f);
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                {
                    float x = i * 1.4f, y = j * 1.4f;
                    p.Shape(Sd.Ellipse(x, y - 0.5f, 0.26f, 0.1f), Rgba.Hex("3A2E30"), 0.035f);   // portalampada
                }
            return new DoodleArt("bulb_board", p);
        }

        /// <summary>Lampadina avvitata (base in 0,-0.5; vetro centrato in 0,0.12). Spenta grigia o accesa gialla.</summary>
        public static DoodleArt Bulb(bool on)
        {
            var p = new DoodlePainter(-0.45f, -0.65f, 0.45f, 0.55f, 160) { Ink = 0.04f };
            Sdf glass = Sd.Union(Sd.Circle(0, 0.12f, 0.34f), Sd.Box(0, -0.2f, 0.17f, 0.14f, 0.05f));
            if (on)
            {
                p.FillFunc(glass, (x, y) => Rgba.Lerp(Rgba.Hex("FFFBD9"), Rgba.Hex("FFD23F"), Clamp((float)Math.Sqrt(x * x + (y - 0.12f) * (y - 0.12f)) / 0.38f)));
                p.Shape(glass, Rgba.Clear);
                p.Line(Rgba.Hex("FF8A1F"), 0.035f, -0.1f, -0.2f, -0.1f, 0.05f, -0.05f, 0.15f, 0f, 0.05f, 0.05f, 0.15f, 0.1f, 0.05f, 0.1f, -0.2f);
            }
            else
            {
                Body(p, glass, Rgba.Hex("C9D3DC"), Rgba.Hex("A7B3C0"));
                p.Line(Rgba.Hex("6E6E7A"), 0.03f, -0.1f, -0.2f, -0.1f, 0.05f, -0.05f, 0.15f, 0f, 0.05f, 0.05f, 0.15f, 0.1f, 0.05f, 0.1f, -0.2f);
            }
            p.Line(Rgba.White.WithAlpha(0.85f), 0.05f, -0.22f, 0.05f, -0.2f, 0.22f, -0.1f, 0.33f);
            Sdf screw = Sd.Box(0, -0.42f, 0.18f, 0.13f, 0.04f);
            Body(p, screw, Steel, SteelShade);
            for (int i = 0; i < 3; i++) p.InkLine(0.02f, -0.17f, -0.34f - i * 0.08f, 0.17f, -0.31f - i * 0.08f);
            p.Shape(Sd.Box(0, -0.58f, 0.07f, 0.04f, 0.02f), SteelDark, 0.025f);
            return new DoodleArt(on ? "bulb_on" : "bulb_off", p);
        }

        /// <summary>Mappa del tesoro (pergamena, percorso tratteggiato fino al formaggio), centro 0,0.</summary>
        public static DoodleArt AtticMap()
        {
            var p = new DoodlePainter(-0.85f, -0.62f, 0.85f, 0.62f, 140) { Ink = 0.04f };
            Sdf paper = Sd.Polygon(-0.78f, 0.5f, -0.3f, 0.55f, 0.2f, 0.5f, 0.78f, 0.55f, 0.74f, -0.1f, 0.78f, -0.55f, 0.1f, -0.5f, -0.4f, -0.56f, -0.76f, -0.5f, -0.72f, 0.05f);
            Body(p, paper, Rgba.Hex("F4DFA8"), Rgba.Hex("DDBF7C"));
            p.Fill(Sd.Circle(-0.45f, 0.2f, 0.18f), Rgba.Hex("8FCB6A", 0.7f));
            p.Fill(Sd.Circle(0.4f, -0.25f, 0.14f), Rgba.Hex("7FC3E8", 0.7f));
            float[] path = { -0.6f, -0.35f, -0.4f, -0.15f, -0.1f, -0.2f, 0.05f, 0.1f, 0.3f, 0.2f, 0.45f, 0.3f };
            for (int i = 0; i + 3 < path.Length; i += 2)
                for (float k = 0f; k < 1f; k += 0.34f)
                {
                    float x0 = path[i] + (path[i + 2] - path[i]) * k, y0 = path[i + 1] + (path[i + 3] - path[i + 1]) * k;
                    float x1 = path[i] + (path[i + 2] - path[i]) * (k + 0.18f), y1 = path[i + 1] + (path[i + 3] - path[i + 1]) * (k + 0.18f);
                    p.Line(Rgba.Hex("C0352F"), 0.035f, x0, y0, x1, y1);
                }
            p.Line(Rgba.Hex("C0352F"), 0.05f, 0.47f, 0.25f, 0.63f, 0.41f);
            p.Line(Rgba.Hex("C0352F"), 0.05f, 0.47f, 0.41f, 0.63f, 0.25f);
            p.Shape(Sd.Polygon(0.5f, 0.05f, 0.72f, 0.12f, 0.7f, -0.02f), Rgba.Hex("FFD23F"), 0.02f);   // formaggio
            return new DoodleArt("attic_map", p);
        }

        /// <summary>Pulsante tondo arancione con la freccia circolare (riavvia), centro 0,0.</summary>
        public static DoodleArt ResetButton()
        {
            var p = new DoodlePainter(-0.48f, -0.48f, 0.48f, 0.48f, 140) { Ink = 0.05f };
            Body(p, Sd.Circle(0, 0, 0.42f), Rgba.Hex("F7A23B"), Rgba.Hex("D9801E"));
            var arc = new List<float>();
            for (int a = 60; a <= 330; a += 15)
            {
                float r = a * (float)Math.PI / 180f;
                arc.Add((float)Math.Cos(r) * 0.22f); arc.Add((float)Math.Sin(r) * 0.22f);
            }
            p.Line(Rgba.White, 0.08f, arc.ToArray());
            float ex = (float)Math.Cos(60 * Math.PI / 180) * 0.22f, ey = (float)Math.Sin(60 * Math.PI / 180) * 0.22f;
            p.Fill(Sd.Polygon(ex - 0.15f, ey + 0.05f, ex + 0.08f, ey + 0.13f, ex + 0.02f, ey - 0.12f), Rgba.White);
            return new DoodleArt("reset_btn", p);
        }

        // ---------- Color tubes ----------

        /// <summary>Provetta di vetro (fondo interno in y -0.1, bordo in y 2.75), larga 0.8: le palline si mettono davanti.</summary>
        public static DoodleArt GlassTube()
        {
            var p = new DoodlePainter(-0.52f, -0.22f, 0.52f, 2.88f, 120) { Ink = 0.05f };
            Sdf tube = Sd.Box(0, 1.315f, 0.4f, 1.435f, 0.38f);
            p.Fill(tube, Rgba.Hex("DDF2FF", 0.35f));
            p.Shape(tube, Rgba.Clear);
            p.Shape(Sd.Box(0, 2.75f, 0.48f, 0.08f, 0.04f), Rgba.Hex("E9F6FF"), 0.04f);
            return new DoodleArt("glass_tube", p);
        }

        /// <summary>Riflesso del vetro della provetta (va davanti alle palline).</summary>
        public static DoodleArt TubeShine()
        {
            var p = new DoodlePainter(-0.52f, -0.22f, 0.52f, 2.88f, 120) { Wobble = 0.004f };
            p.Fill(Sd.Capsule(-0.26f, 0.35f, -0.26f, 2.45f, 0.045f), Rgba.White.WithAlpha(0.55f));
            p.Fill(Sd.Capsule(0.27f, 2.0f, 0.27f, 2.4f, 0.03f), Rgba.White.WithAlpha(0.4f));
            return new DoodleArt("tube_shine", p);
        }

        /// <summary>Pallina colorata lucida (raggio 0.28, centro 0,0).</summary>
        public static DoodleArt TubeBall(string name, Rgba fill, Rgba shade)
        {
            var p = new DoodlePainter(-0.32f, -0.32f, 0.32f, 0.32f, 160) { Ink = 0.04f };
            Body(p, Sd.Circle(0, 0, 0.28f), fill, shade);
            p.Fill(Sd.Ellipse(-0.1f, 0.12f, 0.08f, 0.05f), Rgba.White.WithAlpha(0.85f));
            return new DoodleArt(name, p);
        }

        /// <summary>Criceto grigio arrotolato a palla: sembra una pallina, ma ha un orecchio, un occhio chiuso e la coda.</summary>
        public static DoodleArt HamsterBall()
        {
            var p = new DoodlePainter(-0.34f, -0.32f, 0.34f, 0.36f, 160) { Ink = 0.04f };
            p.Shape(Sd.Circle(0.14f, 0.24f, 0.07f), Rgba.Hex("FF9BB0"), 0.025f);   // orecchio
            Body(p, Sd.Circle(0, 0, 0.28f), Rgba.Hex("B8B4C0"), Rgba.Hex("8F8A9C"));
            p.Fill(Sd.Ellipse(-0.1f, 0.12f, 0.07f, 0.045f), Rgba.White.WithAlpha(0.7f));
            p.InkLine(0.022f, 0.06f, 0.06f, 0.11f, 0.03f, 0.16f, 0.06f);   // occhio chiuso
            p.Fill(Sd.Circle(0.21f, -0.04f, 0.025f), Rgba.Hex("FF7F9C"));     // nasino
            p.Line(Rgba.Hex("FF9BB0"), 0.025f, -0.24f, -0.17f, -0.31f, -0.2f, -0.3f, -0.26f);   // codina
            return new DoodleArt("hamster_ball", p);
        }

        /// <summary>Stanza dei giochi: parete azzurra, mensola e pavimento (sfondo 9 x 14; mensola in y -2.75).</summary>
        public static DoodleArt ToyShelfBackground()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            Sdf wall = Sd.Box(0, 1.8f, 5f, 5.3f);
            p.Fill(wall, Rgba.Hex("CFE9F5"), false);
            p.Hatch(wall, Rgba.Hex("BEDFEE"), 0.9f, 0.3f, 90f);
            // stelline sulla carta da parati
            var rnd = new Random(39);
            for (int i = 0; i < 26; i++)
            {
                float x = (float)rnd.NextDouble() * 8.6f - 4.3f, y = (float)rnd.NextDouble() * 9f - 2f;
                p.Fill(Sd.Polygon(x, y + 0.1f, x + 0.03f, y + 0.03f, x + 0.1f, y, x + 0.03f, y - 0.03f, x, y - 0.1f, x - 0.03f, y - 0.03f, x - 0.1f, y, x - 0.03f, y + 0.03f), Rgba.White.WithAlpha(0.8f));
            }
            Sdf floor = Sd.Box(0, -5.4f, 5f, 2.0f);
            p.Fill(floor, Rgba.Hex("E2B27A"), false);
            foreach (float y in new[] { -4.0f, -4.8f, -5.7f }) p.Line(Ink.WithAlpha(0.4f), 0.03f, -4.5f, y, 4.5f, y + 0.03f);
            p.Shape(Sd.Box(0, -3.4f, 5f, 0.08f), Rgba.Hex("B98745"));
            // mensola con le staffe
            Body(p, Sd.Box(0, -2.85f, 2.75f, 0.12f, 0.04f), Wood, WoodShade);
            foreach (float s in new[] { -1.9f, 1.9f }) p.Shape(Sd.Polygon(s - 0.12f, -2.97f, s + 0.12f, -2.97f, s, -3.3f), WoodShade, 0.04f);
            return new DoodleArt("toyshelf_bg", p);
        }

        // ---------- Four liters ----------

        /// <summary>Caraffa graduata di vetro: interno largo 1 (x -0.5..0.5) dal fondo y 0, 0.36 per litro; tacche a ogni litro.</summary>
        public static DoodleArt Jug(int liters)
        {
            float h = liters * 0.36f;
            var p = new DoodlePainter(-0.82f, -0.16f, 0.95f, h + 0.32f, 120) { Ink = 0.05f };
            Sdf body = Sd.Union(Sd.Box(0, h * 0.5f + 0.02f, 0.56f, h * 0.5f + 0.12f, 0.08f),
                                Sd.Polygon(-0.56f, h + 0.1f, -0.78f, h + 0.25f, -0.56f, h - 0.15f));
            // manico
            p.Line(Ink, 0.16f, 0.5f, h - 0.15f, 0.82f, h - 0.15f, 0.82f, 0.3f, 0.5f, 0.3f);
            p.Line(Rgba.Hex("DDF2FF"), 0.08f, 0.5f, h - 0.15f, 0.82f, h - 0.15f, 0.82f, 0.3f, 0.5f, 0.3f);
            p.Fill(body, Rgba.Hex("DDF2FF", 0.28f));
            p.Shape(body, Rgba.Clear);
            for (int l = 1; l <= liters; l++)
            {
                float y = l * 0.36f;
                p.Line(Ink.WithAlpha(0.7f), 0.03f, -0.5f, y, l == liters ? -0.05f : -0.25f, y);
            }
            p.Fill(Sd.Capsule(0.36f, 0.15f, 0.36f, h - 0.1f, 0.035f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("jug_" + liters + "l", p);
        }

        /// <summary>Lavello/vaschetta per svuotare (centro 0,0, bordo in y 0.35) con lo scarico.</summary>
        public static DoodleArt DrainSink()
        {
            var p = new DoodlePainter(-0.95f, -0.6f, 0.95f, 0.5f, 120) { Ink = 0.05f };
            Sdf basin = Sd.Polygon(-0.85f, 0.35f, 0.85f, 0.35f, 0.65f, -0.5f, -0.65f, -0.5f);
            Body(p, basin, Steel, SteelShade);
            p.Shape(Sd.Ellipse(0, 0.3f, 0.72f, 0.1f), Rgba.Hex("8D93A6"), 0.04f);
            p.Shape(Sd.Ellipse(0, -0.2f, 0.12f, 0.05f), Rgba.Hex("3A3540"), 0.03f);
            p.Shape(Sd.Box(0, 0.38f, 0.88f, 0.05f, 0.03f), Rgba.Hex("D9DEE6"), 0.04f);
            return new DoodleArt("drain_sink", p);
        }

        /// <summary>Ciotola rosa del gatto con il pesciolino (centro 0,0; bocca in y 0.25).</summary>
        public static DoodleArt CatBowl()
        {
            var p = new DoodlePainter(-0.85f, -0.4f, 0.85f, 0.4f, 120) { Ink = 0.05f };
            Sdf bowl = Sd.Polygon(-0.78f, 0.25f, 0.78f, 0.25f, 0.6f, -0.32f, -0.6f, -0.32f);
            Body(p, bowl, Rgba.Hex("FF8FB0"), Rgba.Hex("E2668C"));
            p.Shape(Sd.Ellipse(0, 0.25f, 0.78f, 0.1f), Rgba.Hex("E2668C"), 0.04f);
            p.Shape(Sd.Ellipse(-0.02f, -0.06f, 0.16f, 0.08f), Rgba.White, 0.025f);
            p.Shape(Sd.Polygon(0.13f, -0.06f, 0.24f, 0.02f, 0.24f, -0.14f), Rgba.White, 0.025f);
            return new DoodleArt("cat_bowl", p);
        }

        /// <summary>Acqua nella ciotola (ellisse azzurra, centro 0,0).</summary>
        public static DoodleArt BowlWater()
        {
            var p = new DoodlePainter(-0.75f, -0.13f, 0.75f, 0.13f, 120) { Ink = 0.03f };
            p.Shape(Sd.Ellipse(0, 0, 0.7f, 0.09f), Rgba.Hex("6EC6F5"));
            p.Fill(Sd.Ellipse(-0.25f, 0.02f, 0.2f, 0.025f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("bowl_water", p);
        }

        // ---------- Untangle ----------

        /// <summary>Bottone da cucito bianco (si tinge) con quattro buchi, raggio 0.3.</summary>
        public static DoodleArt YarnButton()
        {
            var p = new DoodlePainter(-0.34f, -0.34f, 0.34f, 0.34f, 160) { Ink = 0.045f };
            Body(p, Sd.Circle(0, 0, 0.3f), Rgba.White, Rgba.Hex("D8D2E0"));
            p.Shape(Sd.Circle(0, 0, 0.2f), Rgba.Clear, 0.025f);
            foreach (float s in new[] { -1f, 1f }) foreach (float t in new[] { -1f, 1f })
                    p.Fill(Sd.Circle(s * 0.075f, t * 0.075f, 0.04f), Ink);
            return new DoodleArt("yarn_button", p);
        }

        /// <summary>Salotto visto dall'alto: pavimento e grande tappeto tondo (sfondo 9 x 14).</summary>
        public static DoodleArt YarnRugBackground()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            Sdf all = Sd.Box(0, 0, 4.5f, 7f);
            p.FillFunc(all, (x, y) => ((int)Math.Floor((y + 14f) / 0.8f) & 1) == 0 ? Rgba.Hex("D9A36A") : Rgba.Hex("CF975E"));
            for (float y = -6.8f; y < 7f; y += 0.8f) p.Line(Ink.WithAlpha(0.3f), 0.03f, -4.5f, y, 4.5f, y);
            Rgba[] rings = { Rgba.Hex("7FB8A4"), Rgba.Hex("F2E6C9"), Rgba.Hex("E58F7A"), Rgba.Hex("F2E6C9"), Rgba.Hex("7FB8A4") };
            for (int i = 0; i < rings.Length; i++)
            {
                Sdf r = Sd.Ellipse(0, -0.2f, 3.0f - i * 0.12f, 4.1f - i * 0.12f);
                if (i == 0) p.Shape(r, rings[i]); else p.Fill(r, rings[i]);
            }
            p.Fill(Sd.Ellipse(0, -0.2f, 2.45f, 3.55f), Rgba.Hex("F7EEDB"));
            p.Hatch(Sd.Ellipse(0, -0.2f, 2.45f, 3.55f), Rgba.Hex("EFE2C6"), 0.3f, 0.06f, 45f);
            return new DoodleArt("yarn_rug_bg", p);
        }

        // ---------- Dot to dot ----------

        /// <summary>Foglio di carta a quadretti con lo scotch agli angoli (5 x 4.3, centro 0,0).</summary>
        public static DoodleArt PaperSheet()
        {
            var p = new DoodlePainter(-2.8f, -2.35f, 2.8f, 2.4f, 80) { Ink = 0.05f };
            Sdf paper = Sd.Box(0, 0, 2.5f, 2.13f, 0.04f);
            p.Fill(paper, Rgba.Hex("FFFDF5"));
            p.Hatch(paper, Rgba.Hex("BFD9F2", 0.7f), 0.3f, 0.015f, 0f);
            p.Hatch(paper, Rgba.Hex("BFD9F2", 0.7f), 0.3f, 0.015f, 90f);
            p.Line(Rgba.Hex("F2A3A3"), 0.025f, -2.15f, -2.1f, -2.15f, 2.1f);
            p.Shape(paper, Rgba.Clear);
            foreach (float s in new[] { -1f, 1f })
                p.Fill(Sd.Rotate(Sd.Box(s * 2.35f, 2.05f, 0.32f, 0.11f), s * 2.35f, 2.05f, -s * 35f), Rgba.Hex("F5E9A8", 0.75f));
            return new DoodleArt("paper_sheet", p);
        }

        /// <summary>Puntino d'inchiostro del gioco "unisci i puntini" (raggio 0.08).</summary>
        public static DoodleArt InkDot()
        {
            var p = new DoodlePainter(-0.12f, -0.12f, 0.12f, 0.12f, 200) { Ink = 0.02f, Wobble = 0.003f };
            p.Shape(Sd.Circle(0, 0, 0.08f), Rgba.Hex("3A3540"));
            return new DoodleArt("ink_dot", p);
        }

        /// <summary>Mosca vista dall'alto con ali trasparenti e occhioni rossi (centro 0,0, testa a destra).</summary>
        public static DoodleArt HouseFly()
        {
            var p = new DoodlePainter(-0.42f, -0.36f, 0.42f, 0.36f, 160) { Ink = 0.035f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.InkLine(0.025f, 0.05f, s * 0.05f, 0.12f, s * 0.2f, 0.2f, s * 0.24f);
                p.InkLine(0.025f, -0.05f, s * 0.05f, -0.08f, s * 0.22f, -0.15f, s * 0.26f);
                p.Shape(Sd.Rotate(Sd.Ellipse(-0.15f, s * 0.17f, 0.22f, 0.1f), 0f, 0f, s * 18f), Rgba.Hex("E6F4FF", 0.6f), 0.025f);
            }
            Body(p, Sd.Ellipse(-0.06f, 0, 0.2f, 0.12f), Rgba.Hex("4A4A5A"), Rgba.Hex("2E2E3A"));
            Body(p, Sd.Circle(0.18f, 0, 0.1f), Rgba.Hex("4A4A5A"), Rgba.Hex("2E2E3A"));
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Circle(0.22f, s * 0.07f, 0.055f), Rgba.Hex("E8433F"), 0.02f);
            return new DoodleArt("house_fly", p);
        }
    }
}
