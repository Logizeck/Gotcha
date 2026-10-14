using System;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// Tutti i disegni del gioco in stile cartone "doodle": contorni a inchiostro tremolanti, colori pieni,
    /// ombre a tratteggio. Le coordinate sono quelle locali dell'oggetto che userà lo sprite
    /// (es. la chiave ha la punta in 0,0; il pupazzo ha i piedi in 0,0).
    /// </summary>
    public static class DoodleSprites
    {
        static readonly Rgba Ink = DoodlePainter.DefaultInk;
        static readonly Rgba Gold = Rgba.Hex("FFC93C"), GoldShade = Rgba.Hex("E39B1B"), GoldLight = Rgba.Hex("FFF3B0");
        static readonly Rgba Wood = Rgba.Hex("D2843F"), WoodShade = Rgba.Hex("A85E26"), WoodDark = Rgba.Hex("7A3F1D");
        static readonly Rgba Snow = Rgba.Hex("FFFFFF"), SnowShade = Rgba.Hex("CFE3F7"), SnowHatch = Rgba.Hex("9CC3EA");
        static readonly Rgba CarrotColor = Rgba.Hex("FF8A1F"), CarrotShade = Rgba.Hex("E0600B");
        static readonly Rgba Leaf = Rgba.Hex("5BBF4A"), LeafShade = Rgba.Hex("3E9A35");
        static readonly Rgba CoalColor = Rgba.Hex("3A3540"), Twig = Rgba.Hex("8A5A3B");
        static readonly Rgba Scarf = Rgba.Hex("E8433F"), ScarfStripe = Rgba.Hex("FFD25A");
        static readonly Rgba BlushColor = Rgba.Hex("FF7F9C");

        // Colori delle pareti del labirinto, presi dal logo: barre smussate giallo/magenta con neon ciano
        public static readonly Rgba NeonYellow = Rgba.Hex("F2FF1F"), NeonYellowDeep = Rgba.Hex("C8F01A");
        public static readonly Rgba NeonMagenta = Rgba.Hex("FF2BD6"), NeonMagentaDeep = Rgba.Hex("C21FC9");
        public static readonly Rgba NeonCyan = Rgba.Hex("3CE6FF");
        public static readonly Rgba NightFloor = Rgba.Hex("0A0E1C");

        /// <summary>Corpo con ombra a mezzaluna, tratteggio, luce e contorno finale.</summary>
        static void Body(DoodlePainter p, Sdf s, Rgba fill, Rgba shade, Rgba? hatch = null, float lightDx = -0.06f, float lightDy = 0.07f)
        {
            p.Fill(s, fill);
            Sdf cres = Sd.Crescent(s, lightDx, lightDy);
            p.Fill(cres, shade);
            if (hatch.HasValue) p.Hatch(cres, hatch.Value, 0.07f, 0.018f, 55f);
            p.Shape(s, Rgba.Clear);
        }

        // ---------- UI ----------

        /// <summary>Pannello/pulsante bianco con bordo a inchiostro (9-slice, si tinge col colore del pulsante).</summary>
        public static DoodleArt Panel()
        {
            var p = new DoodlePainter(-0.64f, -0.64f, 0.64f, 0.64f, 100) { Ink = 0.07f, Wobble = 0.01f };
            p.Shape(Sd.Box(0, 0, 0.54f, 0.54f, 0.26f), Rgba.White);
            return new DoodleArt("ui_panel", p, 48);
        }

        // ---------- Chiave e porta ----------

        /// <summary>Chiave con la punta in (0,0), rivolta a destra.</summary>
        public static DoodleArt Key()
        {
            var p = new DoodlePainter(-1.32f, -0.42f, 0.12f, 0.42f, 180) { Ink = 0.045f };
            Sdf bow = Sd.Subtract(Sd.Circle(-0.95f, 0, 0.32f), Sd.Circle(-0.95f, 0, 0.12f));
            Sdf key = Sd.Union(bow,
                Sd.Box(-0.64f, 0, 0.05f, 0.13f, 0.03f),
                Sd.Box(-0.33f, 0, 0.33f, 0.075f, 0.07f),
                Sd.Box(-0.12f, -0.13f, 0.05f, 0.08f, 0.02f),
                Sd.Box(-0.3f, -0.12f, 0.05f, 0.065f, 0.02f));
            p.Fill(key, Gold);
            p.Fill(Sd.Crescent(key, -0.03f, 0.04f), GoldShade);
            p.Fill(Sd.Capsule(-1.13f, 0.1f, -1.02f, 0.22f, 0.035f), GoldLight);
            p.Fill(Sd.Capsule(-0.55f, 0.03f, -0.12f, 0.03f, 0.018f), GoldLight);
            p.Shape(key, Rgba.Clear);
            // tre segnetti decorativi sull'anello
            p.InkLine(0.02f, -0.72f, 0.16f, -0.69f, 0.2f);
            p.InkLine(0.02f, -0.7f, -0.17f, -0.67f, -0.2f);
            return new DoodleArt("key", p);
        }

        /// <summary>Telaio della porta con luce calda dietro (centro in 0,0, porta 2.2 x 3.6).</summary>
        public static DoodleArt DoorFrame()
        {
            var p = new DoodlePainter(-1.42f, -1.95f, 1.42f, 2.02f, 100) { Ink = 0.06f };
            Sdf opening = Sd.Box(0, 0, 1.1f, 1.8f, 0.03f);
            p.FillFunc(opening, (x, y) => Rgba.Lerp(Rgba.Hex("FFF6C7"), Rgba.Hex("FFD86B"), Math.Min(1f, Math.Abs(x) * 0.8f)));
            p.Hatch(opening, Rgba.Hex("FFB84A", 0.35f), 0.25f, 0.06f, 70f);
            Sdf frame = Sd.Subtract(Sd.Box(0, 0.06f, 1.27f, 1.9f, 0.08f), opening);
            Body(p, frame, WoodDark, Rgba.Hex("5A2C12"));
            return new DoodleArt("door_frame", p);
        }

        /// <summary>Anta della porta: cardini in x=0, si estende fino a x=2.2.</summary>
        public static DoodleArt DoorPanel()
        {
            var p = new DoodlePainter(-0.08f, -1.87f, 2.28f, 1.87f, 100) { Ink = 0.055f };
            Sdf board = Sd.Box(1.1f, 0, 1.1f, 1.8f, 0.07f);
            Body(p, board, Wood, WoodShade, null, -0.05f, 0.05f);
            // assi verticali e venature
            foreach (float x in new[] { 0.55f, 1.1f, 1.65f }) p.Line(Ink.WithAlpha(0.8f), 0.025f, x, -1.75f, x + 0.01f, 1.75f);
            p.Line(WoodShade, 0.02f, 0.25f, 0.9f, 0.3f, 0.6f, 0.26f, 0.3f);
            p.Line(WoodShade, 0.02f, 0.85f, -0.3f, 0.8f, -0.7f, 0.84f, -1.0f);
            p.Line(WoodShade, 0.02f, 1.35f, 1.3f, 1.4f, 0.9f);
            p.Line(WoodShade, 0.02f, 1.9f, -1.2f, 1.86f, -1.5f);
            // traverse con chiodi
            foreach (float y in new[] { 1.15f, -1.15f })
            {
                Sdf batten = Sd.Box(1.1f, y, 1.0f, 0.13f, 0.04f);
                Body(p, batten, WoodShade, WoodDark, null, -0.02f, 0.03f);
                foreach (float x in new[] { 0.25f, 1.95f }) p.Shape(Sd.Circle(x, y, 0.035f), CoalColor, 0.02f);
            }
            Sdf knob = Sd.Circle(1.85f, 0.3f, 0.13f);
            Body(p, knob, Gold, GoldShade, null, -0.03f, 0.04f);
            p.Fill(Sd.Circle(1.81f, 0.34f, 0.035f), GoldLight);
            return new DoodleArt("door_panel", p);
        }

        /// <summary>Placca della serratura (centro in 0,0).</summary>
        public static DoodleArt Keyhole()
        {
            var p = new DoodlePainter(-0.2f, -0.3f, 0.2f, 0.3f, 200) { Ink = 0.035f, Wobble = 0.006f };
            Sdf plate = Sd.Box(0, 0, 0.13f, 0.24f, 0.07f);
            Body(p, plate, Gold, GoldShade, null, -0.02f, 0.03f);
            p.Fill(Sd.Union(Sd.Circle(0, 0.05f, 0.055f), Sd.Polygon(-0.035f, 0.03f, 0.035f, 0.03f, 0.028f, -0.13f, -0.028f, -0.13f)), Ink);
            return new DoodleArt("keyhole", p);
        }

        /// <summary>Stanza del livello 1 (coordinate del livello; pavimento sotto y = -2.6).</summary>
        public static DoodleArt RoomBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            Sdf wall = Sd.Box(0, 2.2f, 5f, 4.8f);
            p.Fill(wall, Rgba.Hex("FFF1D6"), false);
            p.Hatch(wall, Rgba.Hex("FFE2B3"), 0.6f, 0.22f, 0f);
            p.Hatch(wall, Rgba.Hex("F7C98F", 0.5f), 0.6f, 0.03f, 0f);

            Sdf floor = Sd.Box(0, -5f, 5f, 2.4f);
            p.Fill(floor, Rgba.Hex("DE9F5E"), false);
            foreach (float y in new[] { -3.2f, -3.95f, -4.8f, -5.8f }) p.Line(Ink.WithAlpha(0.5f), 0.03f, -4.5f, y, 4.5f, y + 0.03f);
            float[] seams = { -3.5f, -1.2f, 0.9f, 3.1f, -2.4f, 0.1f, 2.2f };
            for (int i = 0; i < seams.Length; i++)
            {
                float y0 = i < 4 ? -3.2f : -3.95f, y1 = i < 4 ? -3.95f : -4.8f;
                p.Line(Ink.WithAlpha(0.5f), 0.03f, seams[i], y0, seams[i], y1);
            }
            p.Shape(Sd.Box(0, -2.55f, 5f, 0.1f), Rgba.Hex("8C4A22"));

            // Quadretto con montagne
            Sdf frame = Sd.Box(-1.9f, 2.4f, 0.62f, 0.5f, 0.04f);
            Sdf canvas = Sd.Box(-1.9f, 2.4f, 0.48f, 0.36f);
            p.Shape(frame, Gold);
            p.Fill(canvas, Rgba.Hex("BDE6FF"));
            p.Fill(Sd.Intersect(canvas, Sd.Polygon(-2.4f, 2.04f, -2.05f, 2.62f, -1.8f, 2.3f, -1.55f, 2.55f, -1.4f, 2.04f)), Rgba.Hex("6CC56B"));
            p.Fill(Sd.Circle(-1.6f, 2.6f, 0.08f), Rgba.Hex("FFD24A"));
            p.Shape(canvas, Rgba.Clear, 0.035f);
            p.InkLine(0.025f, -2.3f, 2.9f, -1.9f, 3.25f, -1.5f, 2.9f);

            // Pianta in vaso
            Sdf pot = Sd.Polygon(-2.35f, -1.95f, -1.75f, -1.95f, -1.85f, -2.55f, -2.25f, -2.55f);
            for (int i = 0; i < 5; i++)
            {
                float a = -50f + i * 25f;
                p.Shape(Sd.Rotate(Sd.Ellipse(-2.05f, -1.45f, 0.12f, 0.42f), -2.05f, -1.9f, a), i % 2 == 0 ? Leaf : LeafShade);
            }
            Body(p, pot, Rgba.Hex("E07A4F"), Rgba.Hex("B85B35"));
            p.Shape(Sd.Box(-2.05f, -1.95f, 0.34f, 0.07f, 0.03f), Rgba.Hex("E98C61"));

            // Lampada appesa
            p.InkLine(0.03f, 1.9f, 4.6f, 1.9f, 3.4f);
            Body(p, Sd.Polygon(1.55f, 2.9f, 2.25f, 2.9f, 2.05f, 3.4f, 1.75f, 3.4f), Rgba.Hex("6EC6FF"), Rgba.Hex("3E9BDB"));
            p.Fill(Sd.Polygon(1.6f, 2.88f, 2.2f, 2.88f, 2.6f, 1.6f, 1.2f, 1.6f), Rgba.Hex("FFF3B0", 0.35f));
            return new DoodleArt("room_bg", p);
        }

        // ---------- Labirinto ----------

        /// <summary>Fondo notte del labirinto (coordinate del livello).</summary>
        public static DoodleArt MazeBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 40) { Wobble = 0f };
            Sdf all = Sd.Box(0, 0, 4.6f, 7.1f);
            p.FillFunc(all, (x, y) =>
            {
                float n = Noise.Value(x * 9f, y * 9f) * 0.035f + Noise.Value(x * 1.3f, y * 1.3f) * 0.03f;
                return new Rgba(NightFloor.r + n, NightFloor.g + n, NightFloor.b + n * 1.6f);
            });
            p.Hatch(all, NeonCyan.WithAlpha(0.05f), 0.5f, 0.02f, 0f);
            p.Hatch(all, NeonCyan.WithAlpha(0.05f), 0.5f, 0.02f, 90f);
            return new DoodleArt("maze_bg", p);
        }

        /// <summary>
        /// Pezzo di parete al neon (9-slice): barra smussata con facce gialle (su/sinistra) e magenta (giù/destra),
        /// contorno ciano luminoso. Nucleo di semi-spessore 0.11, alone di 0.11 attorno.
        /// </summary>
        public static DoodleArt WallPiece()
        {
            const float core = 0.11f, half = 0.22f;
            var p = new DoodlePainter(-half - 0.01f, -half - 0.01f, half + 0.01f, half + 0.01f, 300) { Wobble = 0f };
            Sdf bar = Sd.Box(0, 0, core, core, 0.015f);
            // alone e linea al neon
            p.FillFunc(Sd.Box(0, 0, half, half), (x, y) =>
            {
                float d = Math.Max(0f, bar(x, y));
                float a = (float)Math.Exp(-d * 28f) * 0.55f;
                return NeonCyan.WithAlpha(a);
            });
            p.Fill(Sd.Subtract(Sd.Grow(bar, 0.035f), Sd.Grow(bar, 0.018f)), Rgba.Lerp(NeonCyan, Rgba.White, 0.35f), false);
            // facce smussate
            p.FillFunc(bar, (x, y) =>
            {
                bool vertical = Math.Abs(y) >= Math.Abs(x);
                bool lit = vertical ? y > 0 : x < 0;
                float depth = 1f - Math.Max(Math.Abs(x), Math.Abs(y)) / core;   // 0 sul bordo, 1 sulla cresta
                Rgba face = lit ? Rgba.Lerp(NeonYellow, NeonYellowDeep, depth * 0.6f) : Rgba.Lerp(NeonMagenta, NeonMagentaDeep, depth * 0.6f);
                if (!vertical && lit) face = Rgba.Lerp(face, Rgba.Hex("B8FF3A"), 0.35f);
                if (!vertical && !lit) face = Rgba.Lerp(face, Rgba.Hex("FF7BE9"), 0.25f);
                return face;
            });
            // spigoli: cresta centrale e diagonali, più un filo di luce sul bordo esterno
            p.Fill(Sd.Intersect(bar, Sd.Union(Sd.Capsule(-core, -core, core, core, 0.004f), Sd.Capsule(-core, core, core, -core, 0.004f))), Rgba.Hex("1A0F25", 0.45f), false);
            p.Fill(Sd.Subtract(bar, Sd.Grow(bar, -0.012f)), Rgba.White.WithAlpha(0.55f), false);
            return new DoodleArt("maze_wall", p, 68);   // bordo = metà sprite meno 2 px centrali da stirare
        }

        /// <summary>Sfera cromata (raggio 0.5): base argentata, il riflesso dell'ambiente lo aggiunge lo shader.</summary>
        public static DoodleArt ChromeBall()
        {
            var p = new DoodlePainter(-0.56f, -0.56f, 0.56f, 0.56f, 160) { Ink = 0.055f, Wobble = 0.004f };
            Sdf ball = Sd.Circle(0, 0, 0.5f);
            p.FillFunc(ball, (x, y) =>
            {
                Rgba sky = Rgba.Lerp(Rgba.Hex("8E9BAA"), Rgba.Hex("F4F7FA"), Math.Min(1f, Math.Max(0f, (y + 0.05f) / 0.5f)));
                Rgba ground = Rgba.Lerp(Rgba.Hex("3F4650"), Rgba.Hex("A8B3BF"), Math.Min(1f, Math.Max(0f, (-y - 0.05f) / 0.45f)));
                Rgba c = y > -0.05f ? sky : ground;
                float r = (float)Math.Sqrt(x * x + y * y) / 0.5f;
                return Rgba.Lerp(c, Rgba.Hex("2E343C"), Math.Max(0f, r - 0.75f) * 1.6f);
            });
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.17f, 0.22f, 0.15f, 0.08f), -0.17f, 0.22f, 35f), Rgba.White.WithAlpha(0.95f));
            p.Fill(Sd.Circle(0.2f, -0.25f, 0.04f), Rgba.White.WithAlpha(0.7f));
            p.Shape(ball, Rgba.Clear);
            return new DoodleArt("chrome_ball", p);
        }

        /// <summary>Stella luminosa del traguardo.</summary>
        public static DoodleArt Goal()
        {
            var p = new DoodlePainter(-0.6f, -0.6f, 0.6f, 0.6f, 120) { Ink = 0.04f };
            p.FillFunc(Sd.Circle(0, 0, 0.6f), (x, y) =>
            {
                float d = (float)Math.Sqrt(x * x + y * y);
                return Rgba.Hex("FFF27A").WithAlpha((float)Math.Exp(-d * 5f) * 0.8f);
            });
            float[] pts = new float[20];
            for (int i = 0; i < 10; i++)
            {
                double a = Math.PI / 2 + i * Math.PI / 5;
                float r = i % 2 == 0 ? 0.3f : 0.13f;
                pts[i * 2] = (float)Math.Cos(a) * r;
                pts[i * 2 + 1] = (float)Math.Sin(a) * r;
            }
            Sdf star = Sd.Polygon(pts);
            Body(p, star, Rgba.Hex("FFE14A"), Rgba.Hex("FFB21E"), null, -0.03f, 0.04f);
            return new DoodleArt("goal_star", p);
        }

        // ---------- Pupazzo di neve ----------

        /// <summary>Paesaggio innevato (coordinate del livello; neve sotto y = -3).</summary>
        public static DoodleArt SnowBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 4.6f, 7.1f), (x, y) =>
                Rgba.Lerp(Rgba.Hex("E9F7FF"), Rgba.Hex("8FD0FF"), Math.Min(1f, Math.Max(0f, (y + 2f) / 7f))));
            // nuvole
            foreach (var c in new[] { new[] { -2.0f, 3.1f }, new[] { 2.1f, 2.4f } })
            {
                Sdf cloud = Sd.Union(Sd.Circle(c[0] - 0.4f, c[1], 0.32f), Sd.Circle(c[0], c[1] + 0.15f, 0.42f),
                                     Sd.Circle(c[0] + 0.45f, c[1] - 0.02f, 0.3f), Sd.Box(c[0], c[1] - 0.15f, 0.7f, 0.17f, 0.15f));
                Body(p, cloud, Rgba.White, Rgba.Hex("DCEBFA"));
            }
            // abete
            Sdf trunk = Sd.Box(2.45f, -2.75f, 0.12f, 0.3f, 0.03f);
            Body(p, trunk, Rgba.Hex("8A5A3B"), Rgba.Hex("6B4129"));
            for (int i = 0; i < 3; i++)
            {
                float y = -2.45f + i * 0.55f, w = 0.85f - i * 0.2f;
                Sdf tier = Sd.Polygon(2.45f - w, y, 2.45f + w, y, 2.45f, y + 0.85f);
                Body(p, tier, Rgba.Hex("4FB06A"), Rgba.Hex("2F8A4C"), null);
                p.Shape(Sd.Union(Sd.Circle(2.45f, y + 0.72f, 0.12f), Sd.Ellipse(2.45f - w * 0.5f, y + 0.08f, 0.18f, 0.07f)), Snow, 0.03f);
            }
            // neve a terra con gobbe
            Sdf ground = Sd.Union(Sd.Box(0, -5.2f, 4.7f, 2.2f), Sd.Circle(-2.6f, -3.6f, 1.0f), Sd.Circle(0.4f, -3.9f, 1.2f),
                                  Sd.Circle(3.3f, -3.4f, 0.9f), Sd.Circle(-4.4f, -3.3f, 0.8f));
            Body(p, ground, Snow, SnowShade, SnowHatch, -0.1f, 0.12f);
            // fiocchi
            var rnd = new Random(7);
            for (int i = 0; i < 26; i++)
            {
                float x = (float)rnd.NextDouble() * 8.6f - 4.3f, y = (float)rnd.NextDouble() * 6f - 1.8f;
                p.Shape(Sd.Circle(x, y, 0.04f + (float)rnd.NextDouble() * 0.03f), Snow, 0.02f);
            }
            return new DoodleArt("snow_bg", p);
        }

        /// <summary>Pupazzo di neve con cappello, sciarpa e braccia di legno. Piedi in (0,0), testa in (0, 4.05).</summary>
        public static DoodleArt Snowman()
        {
            var p = new DoodlePainter(-2.15f, -0.15f, 2.15f, 5.4f, 100) { Ink = 0.06f };
            // braccia (dietro)
            Sdf armL = Sd.Polyline(0.045f, -0.7f, 2.7f, -1.4f, 3.12f, -1.95f, 3.5f);
            Sdf armL2 = Sd.Union(Sd.Polyline(0.035f, -1.4f, 3.12f, -1.55f, 3.48f), Sd.Polyline(0.03f, -1.95f, 3.5f, -2.05f, 3.72f),
                                 Sd.Polyline(0.03f, -1.95f, 3.5f, -2.1f, 3.42f));
            Sdf armR = Sd.Polyline(0.045f, 0.7f, 2.7f, 1.4f, 3.02f, 1.9f, 3.28f);
            Sdf armR2 = Sd.Union(Sd.Polyline(0.035f, 1.4f, 3.02f, 1.52f, 3.38f), Sd.Polyline(0.03f, 1.9f, 3.28f, 2.05f, 3.48f),
                                 Sd.Polyline(0.03f, 1.9f, 3.28f, 2.08f, 3.2f));
            p.Shape(Sd.Union(armL, armL2, armR, armR2), Twig, 0.035f);

            // neve sotto i piedi
            Body(p, Sd.Ellipse(0, 0.08f, 1.55f, 0.2f), Snow, SnowShade);

            Sdf bottom = Sd.Circle(0, 1.15f, 1.15f), middle = Sd.Circle(0, 2.75f, 0.85f), head = Sd.Circle(0, 4.05f, 0.62f);
            Body(p, bottom, Snow, SnowShade, SnowHatch, -0.2f, 0.22f);
            Body(p, middle, Snow, SnowShade, SnowHatch, -0.16f, 0.18f);
            foreach (float y in new[] { 3.0f, 2.68f, 2.36f })
            {
                Sdf b = Sd.Circle(0.03f, y, 0.075f);
                p.Shape(b, CoalColor, 0.03f);
                p.Fill(Sd.Circle(0.01f, y + 0.025f, 0.02f), Rgba.White.WithAlpha(0.6f));
            }

            // sciarpa: giro attorno al collo e coda
            Sdf tail = Sd.Rotate(Sd.Box(0.4f, 3.08f, 0.13f, 0.38f, 0.07f), 0.4f, 3.08f, 14f);
            Body(p, tail, Scarf, Rgba.Hex("B92D2A"));
            p.Hatch(Sd.Grow(tail, -0.03f), ScarfStripe, 0.2f, 0.07f, 104f);
            foreach (float dx in new[] { -0.08f, 0f, 0.08f }) p.InkLine(0.025f, 0.33f + dx + 0.09f, 2.72f, 0.31f + dx + 0.1f, 2.6f);

            Body(p, head, Snow, SnowShade, SnowHatch, -0.12f, 0.14f);

            Sdf wrap = Sd.Rotate(Sd.Box(0, 3.5f, 0.64f, 0.14f, 0.12f), 0, 3.5f, -4f);
            Body(p, wrap, Scarf, Rgba.Hex("B92D2A"));
            p.Hatch(Sd.Grow(wrap, -0.03f), ScarfStripe, 0.22f, 0.08f, 80f);
            p.Shape(wrap, Rgba.Clear);

            // cilindro inclinato
            const float tilt = 9f;
            Sdf brim = Sd.Rotate(Sd.Box(0.03f, 4.6f, 0.6f, 0.075f, 0.05f), 0, 4.6f, tilt);
            Sdf crown = Sd.Rotate(Sd.Box(0.03f, 4.97f, 0.39f, 0.36f, 0.06f), 0, 4.6f, tilt);
            Sdf band = Sd.Intersect(crown, Sd.Rotate(Sd.Box(0.03f, 4.73f, 0.5f, 0.08f), 0, 4.6f, tilt));
            Body(p, crown, CoalColor, Rgba.Hex("24212A"));
            p.Fill(band, Scarf);
            p.Shape(crown, Rgba.Clear);
            Body(p, brim, CoalColor, Rgba.Hex("24212A"));
            p.Fill(Sd.Rotate(Sd.Capsule(-0.22f, 5.15f, -0.22f, 4.9f, 0.025f), 0, 4.6f, tilt), Rgba.White.WithAlpha(0.35f));
            return new DoodleArt("snowman", p);
        }

        /// <summary>Guance rosse (centrate sulla faccia).</summary>
        public static DoodleArt Blush()
        {
            var p = new DoodlePainter(-0.62f, -0.25f, 0.62f, 0.25f, 120) { Ink = 0.025f };
            foreach (float x in new[] { -0.36f, 0.36f })
            {
                Sdf cheek = Sd.Ellipse(x, 0, 0.17f, 0.1f);
                p.Fill(cheek, BlushColor.WithAlpha(0.85f));
                p.Hatch(cheek, Rgba.Hex("E2456E", 0.7f), 0.06f, 0.018f, 60f);
            }
            return new DoodleArt("blush", p);
        }

        /// <summary>Pozzanghera di neve sciolta.</summary>
        public static DoodleArt Puddle()
        {
            var p = new DoodlePainter(-2.25f, -0.45f, 2.25f, 0.45f, 80) { Ink = 0.05f };
            Sdf water = Sd.Union(Sd.Ellipse(0, 0, 2.0f, 0.3f), Sd.Ellipse(-1.3f, -0.05f, 0.8f, 0.3f), Sd.Ellipse(1.2f, 0.05f, 0.85f, 0.28f));
            Body(p, water, Rgba.Hex("DDF2FF"), Rgba.Hex("A9D8F5"));
            p.InkLine(0.02f, -1.2f, 0.08f, -0.6f, 0.12f);
            p.InkLine(0.02f, 0.5f, -0.1f, 1.1f, -0.06f);
            return new DoodleArt("puddle", p);
        }

        /// <summary>Carota con ciuffo, punta a destra (centro in 0,0).</summary>
        public static DoodleArt Carrot()
        {
            var p = new DoodlePainter(-0.66f, -0.3f, 0.52f, 0.3f, 200) { Ink = 0.035f };
            for (int i = -1; i <= 1; i++)
            {
                Sdf leaf = Sd.Rotate(Sd.Ellipse(-0.5f, 0, 0.17f, 0.06f), -0.34f, 0, i * 32f);
                p.Shape(leaf, i == 0 ? Leaf : LeafShade, 0.025f);
            }
            Sdf body = Sd.Union(Sd.Polygon(-0.32f, 0.135f, 0.46f, 0.008f, 0.46f, -0.008f, -0.32f, -0.135f), Sd.Ellipse(-0.31f, 0, 0.07f, 0.135f));
            Body(p, body, CarrotColor, CarrotShade, null, -0.02f, 0.035f);
            p.InkLine(0.016f, -0.16f, 0.1f, -0.13f, 0.03f);
            p.InkLine(0.016f, 0.0f, -0.08f, 0.03f, -0.02f);
            p.InkLine(0.016f, 0.15f, 0.06f, 0.17f, 0.01f);
            p.InkLine(0.014f, 0.29f, -0.035f, 0.3f, 0.0f);
            p.Fill(Sd.Capsule(-0.25f, 0.07f, 0.05f, 0.045f, 0.012f), Rgba.Hex("FFC27A"));
            return new DoodleArt("carrot", p);
        }

        /// <summary>Pezzo di carbone (occhio).</summary>
        public static DoodleArt Coal()
        {
            var p = new DoodlePainter(-0.19f, -0.19f, 0.19f, 0.19f, 220) { Ink = 0.03f, Wobble = 0.018f, WobbleFreq = 14f };
            Body(p, Sd.Circle(0, 0, 0.13f), CoalColor, Rgba.Hex("231F28"), null, -0.02f, 0.03f);
            p.Fill(Sd.Circle(-0.04f, 0.05f, 0.03f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("coal", p);
        }

        /// <summary>Sbuffo di fumo.</summary>
        public static DoodleArt Puff()
        {
            var p = new DoodlePainter(-0.42f, -0.3f, 0.42f, 0.34f, 120) { Ink = 0.035f };
            Sdf puff = Sd.Union(Sd.Circle(-0.16f, -0.03f, 0.15f), Sd.Circle(0.04f, 0.07f, 0.2f), Sd.Circle(0.21f, -0.05f, 0.14f), Sd.Box(0.02f, -0.1f, 0.3f, 0.08f, 0.08f));
            Body(p, puff, Rgba.Hex("F4F4F7"), Rgba.Hex("C9CAD3"));
            return new DoodleArt("puff", p);
        }

        /// <summary>Fumetto con codina in basso a sinistra.</summary>
        public static DoodleArt Bubble()
        {
            var p = new DoodlePainter(-1.55f, -0.85f, 1.55f, 0.72f, 100) { Ink = 0.06f };
            Sdf bubble = Sd.Union(Sd.Box(0, 0.05f, 1.4f, 0.58f, 0.42f), Sd.Polygon(-0.55f, -0.4f, -0.1f, -0.4f, -0.85f, -0.8f));
            p.Shape(bubble, Rgba.White);
            return new DoodleArt("bubble", p);
        }
    }
}
