using System;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli 32-35 (pizza chef, palloncino, occhi del gatto, frittella).</summary>
    public static partial class DoodleSprites
    {
        // ---------- Livello 32: pizza chef ----------

        static readonly Rgba Chalk = Rgba.Hex("F2F2EA", 0.9f);

        /// <summary>Lavagna della cucina con disegni a gesso: pomodoro, mozzarella, fungo, oliva... e altri scarabocchi.</summary>
        public static DoodleArt Chalkboard()
        {
            var p = new DoodlePainter(-1.35f, -0.9f, 1.35f, 0.9f, 110) { Ink = 0.05f };
            Body(p, Sd.Box(0, 0, 1.28f, 0.84f, 0.05f), Rgba.Hex("A0663A"), Rgba.Hex("7A4A26"));
            Sdf board = Sd.Box(0, 0.02f, 1.13f, 0.7f);
            p.FillFunc(board, (x, y) => Rgba.Lerp(Rgba.Hex("2F4A3E"), Rgba.Hex("26392F"), Clamp(Noise.Value(x * 4f, y * 4f))));
            p.Hatch(board, Rgba.Hex("FFFFFF", 0.04f), 0.15f, 0.06f, 20f);   // aloni di gesso cancellato
            // disegnini a gesso (sembrano scarabocchi di una nonna, non una lista)
            ChalkCircle(p, -0.78f, 0.3f, 0.2f);                                                  // pomodoro con le foglioline
            p.Line(Chalk, 0.022f, -0.88f, 0.52f, -0.78f, 0.47f, -0.68f, 0.52f);
            p.Line(Chalk, 0.022f, -0.78f, 0.47f, -0.78f, 0.58f);
            ChalkCircle(p, -0.15f, 0.3f, 0.2f);                                                  // mozzarella (pallina con il nodino)
            p.Line(Chalk, 0.022f, -0.2f, 0.5f, -0.15f, 0.56f, -0.1f, 0.5f);
            p.Line(Chalk, 0.016f, -0.25f, 0.28f, -0.2f, 0.32f);
            p.Line(Chalk, 0.025f, 0.35f, 0.3f, 0.42f, 0.48f, 0.6f, 0.55f, 0.78f, 0.48f, 0.85f, 0.3f, 0.35f, 0.3f);   // fungo
            p.Line(Chalk, 0.025f, 0.52f, 0.3f, 0.52f, 0.1f, 0.68f, 0.1f, 0.68f, 0.3f);
            ChalkOval(p, -0.5f, -0.38f, 0.16f, 0.11f);                                           // oliva col buco
            ChalkCircle(p, -0.5f, -0.38f, 0.04f);
            p.Line(Chalk, 0.02f, 0.1f, -0.42f, 0.22f, -0.3f, 0.34f, -0.42f, 0.22f, -0.56f, 0.1f, -0.42f);   // cuoricino
            foreach (float[] st in new[] { new[] { 0.7f, -0.32f }, new[] { 0.9f, -0.5f }, new[] { -0.95f, -0.15f } })   // stelline
            {
                p.Line(Chalk, 0.016f, st[0] - 0.06f, st[1], st[0] + 0.06f, st[1]);
                p.Line(Chalk, 0.016f, st[0], st[1] - 0.06f, st[0], st[1] + 0.06f);
            }
            p.Shape(board, Rgba.Clear, 0.03f);
            p.Shape(Sd.Box(0.75f, -0.78f, 0.18f, 0.04f, 0.02f), Rgba.White, 0.015f);   // gessetto
            return new DoodleArt("chalkboard", p);
        }

        static void ChalkCircle(DoodlePainter p, float cx, float cy, float r) { ChalkOval(p, cx, cy, r, r); }

        static void ChalkOval(DoodlePainter p, float cx, float cy, float rx, float ry)
        {
            var pts = new float[2 * 21];
            for (int i = 0; i <= 20; i++)
            {
                double a = i * Math.PI * 2 / 20;
                pts[2 * i] = cx + (float)Math.Cos(a) * rx;
                pts[2 * i + 1] = cy + (float)Math.Sin(a) * ry;
            }
            p.Line(Chalk, 0.024f, pts);
        }

        /// <summary>Forno per pizza a cupola (base in 0,0; bocca al centro a y 0.75).</summary>
        public static DoodleArt PizzaOven()
        {
            var p = new DoodlePainter(-1.2f, -0.1f, 1.2f, 2.0f, 110) { Ink = 0.05f };
            Body(p, Sd.Box(0, 0.2f, 1.1f, 0.2f, 0.04f), Rgba.Hex("8D93A6"), Rgba.Hex("666B7D"));
            Sdf dome = Sd.Intersect(Sd.Ellipse(0, 0.4f, 1.05f, 1.15f), Sd.Box(0, 1.0f, 1.1f, 0.6f));
            Body(p, dome, Rgba.Hex("D9784F"), Rgba.Hex("B0573A"), Rgba.Hex("9C4A30", 0.4f));
            for (int r = 0; r < 4; r++)
                for (int c = -3; c <= 3; c++)
                {
                    float x = c * 0.3f + (r % 2) * 0.15f, y = 0.55f + r * 0.26f;
                    p.Line(Rgba.Hex("9C4A30", 0.5f), 0.015f, x - 0.12f, y, x + 0.12f, y);
                }
            p.Shape(Sd.Box(0.62f, 1.62f, 0.12f, 0.25f, 0.03f), Rgba.Hex("8D93A6"), 0.03f);   // canna fumaria
            Sdf mouth = Sd.Intersect(Sd.Ellipse(0, 0.42f, 0.55f, 0.55f), Sd.Box(0, 0.7f, 0.6f, 0.28f));
            p.FillFunc(mouth, (x, y) => Rgba.Lerp(Rgba.Hex("2A1A16"), Rgba.Hex("6B2A1A"), Clamp((0.95f - y) / 0.5f)));
            p.Shape(mouth, Rgba.Clear, 0.04f);
            return new DoodleArt("oven", p);
        }

        static Sdf PizzaClip(bool quarter) { return quarter ? Sd.Box(0.6f, 0.6f, 0.6f, 0.6f) : Sd.Circle(0, 0, 2f); }

        static DoodleArt PizzaLayer(string name, bool quarter, Action<DoodlePainter, Sdf> draw)
        {
            var p = quarter ? new DoodlePainter(-0.1f, -0.1f, 1.2f, 1.2f, 110) : new DoodlePainter(-1.2f, -1.2f, 1.2f, 1.2f, 110);
            p.Ink = 0.045f;
            draw(p, PizzaClip(quarter));
            return new DoodleArt(name, p);
        }

        /// <summary>Pasta stesa (bianca: in forno si tinge di dorato).</summary>
        public static DoodleArt Dough(bool quarter)
        {
            return PizzaLayer(quarter ? "dough_q" : "dough", quarter, (p, clip) =>
            {
                Sdf d = Sd.Intersect(Sd.Circle(0, 0, 1.1f), clip);
                p.FillFunc(d, (x, y) => Rgba.Lerp(Rgba.Hex("FFF1D6"), Rgba.Hex("F2DDB5"), Clamp(Noise.Value(x * 6f, y * 6f))));
                p.Fill(Sd.Intersect(Sd.Subtract(Sd.Circle(0, 0, 1.1f), Sd.Circle(0, 0, 0.94f)), clip), Rgba.Hex("EAD0A0"));
                p.Shape(d, Rgba.Clear);
            });
        }

        public static DoodleArt Sauce(bool quarter)
        {
            return PizzaLayer(quarter ? "sauce_q" : "sauce", quarter, (p, clip) =>
            {
                Sdf s = Sd.Intersect(Sd.Circle(0, 0, 0.92f), clip);
                p.FillFunc(s, (x, y) => Rgba.Lerp(Rgba.Hex("E2483C"), Rgba.Hex("C23A30"), Clamp(Noise.Value(x * 8f, y * 8f))));
            });
        }

        public static DoodleArt Cheese(bool quarter)
        {
            return PizzaLayer(quarter ? "cheese_q" : "cheese", quarter, (p, clip) =>
            {
                var rnd = new Random(3);
                for (int i = 0; i < 24; i++)
                {
                    // distribuzione a girasole: copre tutta la pizza in modo uniforme
                    double a = i * 2.39996, r = Math.Sqrt((i + 0.5) / 24.0) * 0.75;
                    float x = (float)(Math.Cos(a) * r) + (float)(rnd.NextDouble() - 0.5) * 0.05f, y = (float)(Math.Sin(a) * r);
                    p.Shape(Sd.Intersect(Sd.Ellipse(x, y, 0.15f, 0.12f), clip), Rgba.Hex("FFF8E8"), 0.02f);
                }
            });
        }

        /// <summary>Un pezzetto di condimento (centro 0,0): fungo, oliva, peperoncino, ananas.</summary>
        public static DoodleArt Topping(string kind)
        {
            var p = new DoodlePainter(-0.22f, -0.22f, 0.22f, 0.22f, 160) { Ink = 0.025f };
            switch (kind)
            {
                case "mushroom":
                    Body(p, Sd.Intersect(Sd.Ellipse(0, 0.02f, 0.17f, 0.13f), Sd.Box(0, 0.08f, 0.2f, 0.1f)), Rgba.Hex("D9C2A0"), Rgba.Hex("B79E74"));
                    p.Shape(Sd.Box(0, -0.06f, 0.05f, 0.08f, 0.02f), Rgba.Hex("EFE2C8"), 0.02f);
                    break;
                case "olive":
                    p.Shape(Sd.Subtract(Sd.Circle(0, 0, 0.11f), Sd.Circle(0, 0, 0.045f)), Rgba.Hex("2A2C36"), 0.02f);
                    break;
                case "chili":
                    Body(p, Sd.Polyline(0.05f, -0.15f, 0.05f, -0.02f, 0.02f, 0.1f, -0.06f, 0.16f, -0.14f), Rgba.Hex("E8261E"), Rgba.Hex("A8170F"));
                    p.Shape(Sd.Box(-0.16f, 0.06f, 0.04f, 0.03f, 0.01f), Leaf, 0.015f);
                    break;
                default:   // ananas
                    Body(p, Sd.Polygon(-0.13f, -0.1f, 0.13f, -0.1f, 0.08f, 0.12f, -0.08f, 0.12f), Rgba.Hex("FFE14A"), Rgba.Hex("E3B81B"));
                    break;
            }
            return new DoodleArt("top_" + kind, p);
        }

        /// <summary>Ciotola degli ingredienti (centro 0,0; il contenuto è un disegno sopra).</summary>
        public static DoodleArt IngredientBowl()
        {
            var p = new DoodlePainter(-0.45f, -0.3f, 0.45f, 0.2f, 140) { Ink = 0.035f };
            Body(p, Sd.Intersect(Sd.Ellipse(0, 0.05f, 0.42f, 0.32f), Sd.Box(0, -0.12f, 0.45f, 0.17f)), Rgba.Hex("7FB6E8"), Rgba.Hex("4F86B8"));
            p.Shape(Sd.Ellipse(0, 0.05f, 0.42f, 0.07f), Rgba.Hex("A9D4F5"), 0.03f);
            return new DoodleArt("ing_bowl", p);
        }

        /// <summary>Lingua di fuori (per i topi piccanti).</summary>
        public static DoodleArt Tongue()
        {
            var p = new DoodlePainter(-0.12f, -0.2f, 0.12f, 0.04f, 200) { Ink = 0.02f };
            p.Shape(Sd.Union(Sd.Box(0, -0.06f, 0.06f, 0.08f, 0.04f), Sd.Circle(0, -0.13f, 0.065f)), Rgba.Hex("FF6F8A"));
            p.Line(Rgba.Hex("D9465F"), 0.012f, 0, -0.02f, 0, -0.14f);
            return new DoodleArt("tongue", p);
        }

        // ---------- Livello 33: il palloncino ----------

        /// <summary>Palloncino rosso (nodo in 0,0, gonfio fino a y ~2.1 a scala 1).</summary>
        public static DoodleArt Balloon()
        {
            var p = new DoodlePainter(-0.85f, -0.15f, 0.85f, 2.2f, 110) { Ink = 0.05f };
            Sdf b = Sd.Union(Sd.Ellipse(0, 1.15f, 0.78f, 0.95f), Sd.Polygon(-0.25f, 0.3f, 0.25f, 0.3f, 0f, 0.15f));
            Body(p, b, Rgba.Hex("FF4D5E"), Rgba.Hex("D12F43"));
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.35f, 1.55f, 0.12f, 0.25f), -0.35f, 1.55f, 25f), Rgba.White.WithAlpha(0.7f));
            p.Shape(Sd.Polygon(-0.1f, 0.0f, 0.1f, 0.0f, 0.05f, 0.15f, -0.05f, 0.15f), Rgba.Hex("D12F43"), 0.025f);   // nodo
            return new DoodleArt("balloon", p);
        }

        /// <summary>Sagoma tratteggiata del palloncino gonfio al massimo (stesse coordinate di balloon a scala 1).</summary>
        public static DoodleArt BalloonOutline()
        {
            var p = new DoodlePainter(-0.9f, -0.15f, 0.9f, 2.25f, 110) { Ink = 0f, Wobble = 0f };
            Sdf b = Sd.Union(Sd.Ellipse(0, 1.15f, 0.78f, 0.95f), Sd.Polygon(-0.25f, 0.3f, 0.25f, 0.3f, 0f, 0.15f));
            p.FillFunc(Sd.Subtract(Sd.Grow(b, 0.035f), Sd.Grow(b, -0.035f)), (x, y) =>
            {
                double a = Math.Atan2(y - 1.15f, x);
                bool dash = ((int)Math.Floor((a + Math.PI) / (Math.PI / 18))) % 2 == 0;
                return dash ? Rgba.Hex("2B2233", 0.75f) : Rgba.Clear;
            });
            return new DoodleArt("balloon_outline", p);
        }

        /// <summary>Pompa a mano (base in 0,0, manico in alto a y ~1.6): il manico è un disegno a parte.</summary>
        public static DoodleArt Pump()
        {
            var p = new DoodlePainter(-0.55f, -0.1f, 0.55f, 1.3f, 120) { Ink = 0.045f };
            Body(p, Sd.Box(0, 0.06f, 0.45f, 0.08f, 0.03f), Rgba.Hex("666B7D"), Rgba.Hex("444856"));
            Body(p, Sd.Box(0, 0.62f, 0.17f, 0.52f, 0.06f), Rgba.Hex("2F6FE0"), Rgba.Hex("1F4FAE"));
            p.Shape(Sd.Box(0, 0.62f, 0.17f, 0.04f), Rgba.Hex("FFC93C"), 0.02f);
            return new DoodleArt("pump", p);
        }

        public static DoodleArt PumpHandle()
        {
            var p = new DoodlePainter(-0.5f, -0.75f, 0.5f, 0.15f, 120) { Ink = 0.045f };
            p.Shape(Sd.Box(0, -0.35f, 0.04f, 0.38f, 0.01f), Rgba.Hex("B7BCC9"), 0.025f);
            Body(p, Sd.Box(0, 0.02f, 0.42f, 0.09f, 0.07f), Rgba.Hex("2A2C36"), Rgba.Hex("111219"));
            return new DoodleArt("pump_handle", p);
        }

        /// <summary>Brandelli di gomma del palloncino scoppiato.</summary>
        public static DoodleArt BalloonScrap()
        {
            var p = new DoodlePainter(-0.2f, -0.2f, 0.2f, 0.2f, 160) { Ink = 0.025f };
            p.Shape(Sd.Polygon(-0.15f, -0.05f, 0.02f, 0.14f, 0.15f, 0.02f, 0.03f, -0.12f), Rgba.Hex("FF4D5E"));
            return new DoodleArt("balloon_scrap", p);
        }

        // ---------- Livello 34: gli occhi del gatto ----------

        /// <summary>Faccione del gatto (centro 0,0, raggio ~1.5) con gli occhi vuoti: iride e pupilla sono a parte.</summary>
        public static DoodleArt CatBigFace()
        {
            var p = new DoodlePainter(-1.85f, -1.5f, 1.85f, 1.9f, 100) { Ink = 0.06f };
            foreach (float s in new[] { -1f, 1f })
            {
                Sdf ear = Sd.Polygon(s * 1.35f, 0.55f, s * 1.25f, 1.85f, s * 0.45f, 1.15f);
                Body(p, ear, CatOrange, CatShade);
                p.Fill(Sd.Grow(ear, -0.15f), CatPink);
            }
            Sdf head = Sd.Ellipse(0, 0, 1.55f, 1.3f);
            Body(p, head, CatOrange, CatShade, null, -0.1f, 0.12f);
            for (int i = 0; i < 3; i++) p.Line(CatShade, 0.08f, -0.25f + i * 0.25f, 1.25f, -0.2f + i * 0.25f, 0.9f);
            p.Fill(Sd.Ellipse(0, -0.55f, 0.75f, 0.55f), Rgba.Hex("FFE2C2"));
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Ellipse(s * 0.58f, 0.25f, 0.42f, 0.4f), Rgba.White, 0.045f);   // bianchi degli occhi
            p.Shape(Sd.Polygon(-0.14f, -0.3f, 0.14f, -0.3f, 0f, -0.45f), CatPink, 0.03f);
            p.InkLine(0.035f, 0f, -0.45f, 0f, -0.6f);
            p.InkLine(0.035f, -0.3f, -0.68f, -0.15f, -0.7f, 0f, -0.6f, 0.15f, -0.7f, 0.3f, -0.68f);
            foreach (float s in new[] { -1f, 1f })
                for (int i = 0; i < 3; i++) p.InkLine(0.02f, s * 0.55f, -0.45f - i * 0.1f, s * 1.45f, -0.35f - i * 0.18f);
            return new DoodleArt("cat_big_face", p);
        }

        /// <summary>Iride verde con la pupilla a fessura (centro 0,0).</summary>
        public static DoodleArt CatPupil()
        {
            var p = new DoodlePainter(-0.28f, -0.3f, 0.28f, 0.3f, 160) { Ink = 0.03f };
            p.FillFunc(Sd.Circle(0, 0, 0.24f), (x, y) => Rgba.Lerp(Rgba.Hex("C8F04A"), Rgba.Hex("6BB82E"), Clamp((float)Math.Sqrt(x * x + y * y) / 0.24f)));
            p.Shape(Sd.Circle(0, 0, 0.24f), Rgba.Clear);
            p.Fill(Sd.Ellipse(0, 0, 0.06f, 0.2f), Ink);
            p.Fill(Sd.Circle(-0.08f, 0.09f, 0.045f), Rgba.White);
            return new DoodleArt("cat_pupil", p);
        }

        /// <summary>Tana nel muro da cui sbuca il topolino (centro 0,0).</summary>
        public static DoodleArt MouseHole()
        {
            var p = new DoodlePainter(-0.55f, -0.5f, 0.55f, 0.5f, 120) { Ink = 0.04f };
            Sdf hole = Sd.Union(Sd.Circle(0, 0.05f, 0.42f), Sd.Box(0, -0.2f, 0.42f, 0.25f));
            p.FillFunc(Sd.Intersect(hole, Sd.Box(0, 0, 0.5f, 0.45f)), (x, y) => Rgba.Lerp(Rgba.Hex("2A1E1C"), Rgba.Hex("4A3530"), Clamp((y + 0.45f) / 0.9f)));
            p.Shape(Sd.Intersect(hole, Sd.Box(0, 0, 0.5f, 0.45f)), Rgba.Clear, 0.04f);
            return new DoodleArt("mouse_hole", p);
        }

        // ---------- Livello 35: la frittella ----------

        /// <summary>Padella vista di lato (centro del fondo in 0,0; manico a destra).</summary>
        public static DoodleArt Pan()
        {
            var p = new DoodlePainter(-1.0f, -0.25f, 2.0f, 0.3f, 110) { Ink = 0.05f };
            Body(p, Sd.Box(1.4f, 0.0f, 0.6f, 0.08f, 0.07f), Rgba.Hex("2A2C36"), Rgba.Hex("111219"));
            Body(p, Sd.Polygon(-0.9f, 0.18f, 0.9f, 0.18f, 0.75f, -0.18f, -0.75f, -0.18f), Rgba.Hex("4A4E5C"), Rgba.Hex("2E313B"));
            p.Fill(Sd.Box(0, 0.16f, 0.88f, 0.03f), Rgba.Hex("8D93A6"));
            return new DoodleArt("pan", p);
        }

        /// <summary>Frittella (centro 0,0): lato cotto (dorato) o lato crudo (pallido).</summary>
        static DoodleArt Pancake(string name, bool cooked)
        {
            var p = new DoodlePainter(-0.85f, -0.22f, 0.85f, 0.22f, 130) { Ink = 0.04f };
            Sdf c = Sd.Ellipse(0, 0, 0.78f, 0.13f);
            if (cooked)
            {
                p.FillFunc(c, (x, y) => Rgba.Lerp(Rgba.Hex("E8A64A"), Rgba.Hex("C9802E"), Clamp(Noise.Value(x * 9f, y * 20f))));
                p.Fill(Sd.Ellipse(-0.25f, 0.05f, 0.18f, 0.03f), Rgba.Hex("F2C46A"));
            }
            else
            {
                p.Fill(c, Rgba.Hex("FFF1C9"));
                var rnd = new Random(4);
                for (int i = 0; i < 9; i++) p.Fill(Sd.Circle((float)rnd.NextDouble() * 1.2f - 0.6f, (float)rnd.NextDouble() * 0.12f - 0.06f, 0.025f), Rgba.Hex("E8D6A0"));   // bollicine
            }
            p.Shape(c, Rgba.Clear);
            return new DoodleArt(name, p);
        }

        public static DoodleArt PancakeCooked() { return Pancake("pancake_cooked", true); }
        public static DoodleArt PancakeRaw() { return Pancake("pancake_raw", false); }
    }
}
