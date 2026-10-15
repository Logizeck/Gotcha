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
        /// Tubo al neon (9-slice): estremità arrotondate, nucleo quasi bianco, corpo colorato e alone diffuso.
        /// Allungato dal 9-slice diventa una capsula; dove due tubi si incontrano gli alone si sommano come nel neon vero.
        /// Raggio del tubo 0.08, alone fino a 0.23 dal centro.
        /// </summary>
        public static DoodleArt NeonTube(string name, Rgba color)
        {
            const float radius = 0.08f, half = 0.23f;
            var p = new DoodlePainter(-half, -half, half, half, 300) { Wobble = 0f };
            Sdf tube = Sd.Box(0, 0, radius, radius, radius);   // un cerchio: il 9-slice lo allunga in capsula
            p.FillFunc(Sd.Box(0, 0, half, half), (x, y) =>
            {
                float d = Math.Max(0f, tube(x, y));
                return color.WithAlpha((float)Math.Exp(-d * 20f) * 0.6f);
            });
            p.FillFunc(tube, (x, y) =>
            {
                float t = Clamp(-tube(x, y) / radius);            // 0 sul bordo del tubo, 1 al centro
                Rgba c = Rgba.Lerp(Rgba.Lerp(color, Rgba.White, 0.15f), Rgba.White, Clamp((t - 0.45f) / 0.45f));
                return Rgba.Lerp(Rgba.Lerp(color, Ink, 0.15f), c, Clamp(t * 4f));
            });
            return new DoodleArt(name, p, 68);   // bordo = metà sprite meno 2 px centrali da stirare
        }

        /// <summary>
        /// Biglia d'acciaio (raggio 0.5): solo un disco neutro con bordo sottile. Tutto l'aspetto metallico
        /// (riflessi dell'ambiente, luci, fresnel) lo calcola lo shader Gotcha/ChromeBall in tempo reale.
        /// Se lo shader mancasse, resta comunque una sfera grigia sfumata.
        /// </summary>
        public static DoodleArt ChromeBall()
        {
            var p = new DoodlePainter(-0.54f, -0.54f, 0.54f, 0.54f, 220) { Ink = 0.02f, Wobble = 0f };
            Sdf ball = Sd.Circle(0, 0, 0.5f);
            p.FillFunc(ball, (x, y) =>
            {
                float r = (float)Math.Sqrt(x * x + y * y) / 0.5f;
                return Rgba.Lerp(Rgba.Hex("D9DEE4"), Rgba.Hex("6E7782"), Clamp(r * r + (0.1f - y) * 0.4f));
            });
            p.Shape(ball, Rgba.Clear);
            return new DoodleArt("chrome_ball", p);
        }

        /// <summary>Ombra morbida sotto la biglia.</summary>
        public static DoodleArt BallShadow()
        {
            var p = new DoodlePainter(-0.7f, -0.7f, 0.7f, 0.7f, 100) { Wobble = 0f };
            p.FillFunc(Sd.Circle(0, 0, 0.7f), (x, y) =>
            {
                float d = (float)Math.Sqrt(x * x + y * y) / 0.7f;
                return new Rgba(0, 0, 0, 0.75f * Clamp(1f - d) * Clamp(1f - d) * 1.6f);
            });
            return new DoodleArt("ball_shadow", p);
        }

        static float Clamp(float v) { return v < 0f ? 0f : v > 1f ? 1f : v; }

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

        // Il pupazzo ha sembianze umane e una testa enorme (buffa): gambe tozze, fianchi, busto, braccia di rametti.
        // Posa di 3/4 (guarda a sinistra, la schiena è a destra); il sedere è solo accennato dalla curva dei fianchi.
        // Coordinate: piedi in (0,0). Le parti hanno il pivot sul punto d'appoggio per sciogliersi dalla testa in giù.
        public const float SnowMiddlePivot = 1.5f, SnowShoulderY = 2.45f, SnowHeadPivot = 2.72f, SnowHatPivot = 4.62f;
        public const float SnowHeadCenterX = -0.1f, SnowHeadCenterY = 3.7f, SnowHeadRadius = 1.0f;
        public const float SnowButtX = 0.86f, SnowButtY = 1.02f;   // punto del sedere (retro dei fianchi)

        /// <summary>Forma di neve con ombra a due toni, tratteggio, puntini e scintilla.</summary>
        static void SnowBody(DoodlePainter p, Sdf shape, float cx, float cy, float r, int seed)
        {
            float wobble = p.Wobble, freq = p.WobbleFreq;
            p.Wobble = 0.018f; p.WobbleFreq = 2.4f;   // contorno da neve, un po' bitorzoluto
            p.Fill(shape, Snow);
            p.Fill(Sd.Crescent(shape, -r * 0.1f, r * 0.12f), Rgba.Hex("E4EFFA"));
            Sdf deep = Sd.Crescent(shape, -r * 0.22f, r * 0.24f);
            p.Fill(deep, SnowShade);
            p.Hatch(deep, SnowHatch, 0.07f, 0.016f, 55f);
            var rnd = new Random(seed);
            for (int i = 0; i < (int)(r * 9); i++)
            {
                double a = rnd.NextDouble() * Math.PI * 2, d = Math.Sqrt(rnd.NextDouble()) * r * 0.8;
                float x = cx + (float)(Math.Cos(a) * d), y = cy + (float)(Math.Sin(a) * d);
                p.Fill(Sd.Intersect(Sd.Circle(x, y, 0.018f + (float)rnd.NextDouble() * 0.015f), shape), Rgba.Hex("BFD6EE"), false);
            }
            p.Fill(Sd.Intersect(Sd.Capsule(cx - r * 0.55f, cy + r * 0.4f, cx - r * 0.3f, cy + r * 0.6f, r * 0.05f), shape), Rgba.White);
            p.Shape(shape, Rgba.Clear);
            p.Wobble = wobble; p.WobbleFreq = freq;
        }

        /// <summary>Base: cumulo di neve, gambe tozze e fianchi (col sedere appena accennato sul retro).</summary>
        public static DoodleArt SnowBase()
        {
            var p = new DoodlePainter(-1.6f, -0.25f, 1.6f, 1.75f, 100) { Ink = 0.06f };
            Sdf mound = Sd.Union(Sd.Ellipse(0.0f, 0.08f, 1.3f, 0.2f), Sd.Circle(-1.0f, 0.14f, 0.2f), Sd.Circle(1.05f, 0.12f, 0.17f));
            Body(p, mound, Snow, SnowShade, SnowHatch, -0.05f, 0.08f);
            // gamba lontana (dietro), poi quella vicina
            SnowBody(p, Sd.Capsule(0.32f, 0.22f, 0.3f, 0.9f, 0.27f), 0.31f, 0.55f, 0.35f, 5);
            SnowBody(p, Sd.Capsule(-0.36f, 0.2f, -0.34f, 0.9f, 0.3f), -0.35f, 0.55f, 0.38f, 7);
            // fianchi: la curva sul retro (destra) suggerisce il sedere, senza altri dettagli
            Sdf hips = Sd.Union(Sd.Ellipse(-0.02f, 1.18f, 0.8f, 0.5f), Sd.Ellipse(0.5f, 1.04f, 0.4f, 0.38f));
            SnowBody(p, hips, 0f, 1.15f, 0.8f, 11);
            p.Line(SnowShade, 0.04f, 0.88f, 0.9f, 0.78f, 0.84f);
            return new DoodleArt("sm_base", p);
        }

        /// <summary>Busto con i bottoni spostati verso sinistra (il davanti), in 3/4.</summary>
        public static DoodleArt SnowMiddle()
        {
            var p = new DoodlePainter(-1.0f, 1.25f, 1.0f, 3.0f, 100) { Ink = 0.055f };
            Sdf torso = Sd.Union(Sd.Ellipse(-0.05f, 2.05f, 0.7f, 0.72f), Sd.Ellipse(-0.03f, 1.62f, 0.62f, 0.3f));
            SnowBody(p, torso, -0.05f, 2.05f, 0.72f, 23);
            foreach (var b in new[] { new[] { -0.38f, 2.45f }, new[] { -0.42f, 2.15f }, new[] { -0.4f, 1.85f } })
            {
                Sdf btn = Sd.Ellipse(b[0], b[1], 0.065f, 0.075f);
                p.Shape(btn, CoalColor, 0.03f);
                p.Fill(Sd.Circle(b[0] - 0.02f, b[1] + 0.025f, 0.02f), Rgba.White.WithAlpha(0.6f));
            }
            return new DoodleArt("sm_middle", p);
        }

        /// <summary>Braccia di rametti con le muffole (spalle a y = 2.45).</summary>
        public static DoodleArt SnowArms()
        {
            var p = new DoodlePainter(-2.15f, 2.15f, 1.95f, 3.45f, 100) { Ink = 0.05f };
            for (int side = -1; side <= 1; side += 2)
            {
                float s = side, len = side < 0 ? 1f : 0.85f, x0 = side < 0 ? -0.6f : 0.5f, y0 = SnowShoulderY;
                Sdf arm = Sd.Union(
                    Sd.Polyline(0.045f, x0, y0, x0 + s * 0.6f * len, y0 + 0.32f, x0 + s * 1.05f * len, y0 + 0.5f),
                    Sd.Polyline(0.032f, x0 + s * 0.6f * len, y0 + 0.32f, x0 + s * 0.68f * len, y0 + 0.65f),
                    Sd.Polyline(0.028f, x0 + s * 0.32f * len, y0 + 0.16f, x0 + s * 0.28f * len, y0 - 0.02f));
                p.Shape(arm, Twig, 0.035f);
                float mx = x0 + s * 1.22f * len, my = y0 + 0.58f;
                Sdf mitten = Sd.Union(Sd.Rotate(Sd.Ellipse(mx, my, 0.17f * len, 0.21f * len), mx, my, -s * 25f),
                                      Sd.Rotate(Sd.Ellipse(mx - s * 0.17f * len, my + 0.12f * len, 0.06f, 0.1f), mx, my, s * 35f));
                Body(p, mitten, Scarf, Rgba.Hex("B92D2A"), null, -s * 0.03f, 0.04f);
                p.Shape(Sd.Rotate(Sd.Box(mx - s * 0.1f * len, my - 0.17f * len, 0.16f * len, 0.06f, 0.04f), mx, my, -s * 25f), ScarfStripe, 0.03f);
            }
            return new DoodleArt("sm_arms", p);
        }

        /// <summary>Testa enorme (senza faccia) e sciarpa a maglia con la coda sul retro.</summary>
        public static DoodleArt SnowHead()
        {
            var p = new DoodlePainter(-1.25f, 1.9f, 1.25f, 4.78f, 100) { Ink = 0.06f };
            Sdf tail = Sd.Rotate(Sd.Box(0.5f, 2.38f, 0.14f, 0.42f, 0.07f), 0.5f, 2.38f, 10f);
            Body(p, tail, Scarf, Rgba.Hex("B92D2A"));
            p.Hatch(Sd.Grow(tail, -0.03f), ScarfStripe, 0.22f, 0.08f, 100f);
            p.Shape(tail, Rgba.Clear);
            for (int i = 0; i < 4; i++)
            {
                float x = 0.4f + i * 0.075f;
                p.Line(Rgba.Hex("B92D2A"), 0.03f, x + 0.08f, 1.98f, x + 0.07f, 1.9f);
            }

            Sdf head = Sd.Circle(SnowHeadCenterX, SnowHeadCenterY, SnowHeadRadius);
            SnowBody(p, head, SnowHeadCenterX, SnowHeadCenterY, SnowHeadRadius, 37);

            Sdf wrap = Sd.Rotate(Sd.Box(-0.05f, 2.82f, 0.7f, 0.16f, 0.14f), -0.05f, 2.82f, -5f);
            Body(p, wrap, Scarf, Rgba.Hex("B92D2A"));
            p.Hatch(Sd.Grow(wrap, -0.03f), ScarfStripe, 0.24f, 0.09f, 82f);
            for (int i = -4; i <= 4; i++)
            {
                float x = -0.05f + i * 0.14f, y = 2.82f - (x + 0.05f) * 0.09f;
                p.Line(Rgba.Hex("8E1F1C", 0.6f), 0.014f, x - 0.03f, y + 0.04f, x, y, x + 0.03f, y + 0.04f);
            }
            p.Shape(wrap, Rgba.Clear);
            return new DoodleArt("sm_head", p);
        }

        /// <summary>Cilindro inclinato con toppa e agrifoglio, appoggiato sulla testa grande.</summary>
        public static DoodleArt SnowHat()
        {
            var p = new DoodlePainter(-1.2f, 4.45f, 1.0f, 5.85f, 100) { Ink = 0.055f };
            const float tilt = 9f, cx = -0.1f, by = 4.66f;
            Sdf brim = Sd.Rotate(Sd.Box(cx, by, 0.82f, 0.09f, 0.06f), cx, by, tilt);
            Sdf crown = Sd.Rotate(Sd.Box(cx, by + 0.47f, 0.52f, 0.46f, 0.07f), cx, by, tilt);
            Sdf band = Sd.Intersect(crown, Sd.Rotate(Sd.Box(cx, by + 0.17f, 0.6f, 0.1f), cx, by, tilt));
            Body(p, crown, CoalColor, Rgba.Hex("24212A"));
            p.Fill(band, Scarf);
            Sdf patch = Sd.Rotate(Sd.Box(cx - 0.2f, by + 0.62f, 0.12f, 0.11f, 0.01f), cx, by, tilt + 6f);
            p.Shape(patch, Rgba.Hex("6B5E7A"), 0.02f);
            p.Shape(crown, Rgba.Clear);
            Body(p, brim, CoalColor, Rgba.Hex("24212A"));
            p.Fill(Sd.Rotate(Sd.Capsule(cx - 0.35f, by + 0.78f, cx - 0.35f, by + 0.35f, 0.03f), cx, by, tilt), Rgba.White.WithAlpha(0.35f));
            foreach (float a in new[] { -35f, 25f })
                p.Shape(Sd.Rotate(Sd.Ellipse(cx + 0.38f, by + 0.25f, 0.15f, 0.065f), cx + 0.3f, by + 0.23f, a), Leaf, 0.025f);
            foreach (var b in new[] { new[] { 0.27f, 0.21f }, new[] { 0.34f, 0.16f }, new[] { 0.23f, 0.13f } })
                p.Shape(Sd.Circle(cx + b[0], by + b[1], 0.05f), Rgba.Hex("E23B3B"), 0.02f);
            return new DoodleArt("sm_hat", p);
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

        // ---------- Livello 4: non fare niente ----------

        static readonly Rgba CatOrange = Rgba.Hex("F6A04D"), CatShade = Rgba.Hex("D9792A"), CatPink = Rgba.Hex("FF9BB0");

        /// <summary>Stanza di notte con finestra, luna e tappeto (coordinate del livello; pavimento sotto y = -2.3).</summary>
        public static DoodleArt NightRoom()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            Sdf wall = Sd.Box(0, 2.4f, 5f, 4.7f);
            p.FillFunc(wall, (x, y) => Rgba.Lerp(Rgba.Hex("2A3266"), Rgba.Hex("3D4A8C"), Clamp((y + 2.3f) / 7f)));
            var rnd = new Random(3);
            for (int i = 0; i < 40; i++)
            {
                float x = (float)rnd.NextDouble() * 9f - 4.5f, y = (float)rnd.NextDouble() * 9f - 2f;
                p.Fill(Sd.Circle(x, y, 0.03f), Rgba.Hex("5B67A8"), false);
            }
            Sdf floor = Sd.Box(0, -4.7f, 5f, 2.4f);
            p.Fill(floor, Rgba.Hex("6B4A3A"), false);
            foreach (float y in new[] { -2.9f, -3.6f, -4.5f, -5.6f }) p.Line(Ink.WithAlpha(0.45f), 0.03f, -4.5f, y, 4.5f, y + 0.03f);
            p.Shape(Sd.Box(0, -2.28f, 5f, 0.09f), Rgba.Hex("4A3128"));
            Sdf rug = Sd.Ellipse(0, -3.3f, 2.5f, 0.75f);
            Body(p, rug, Rgba.Hex("C94F6D"), Rgba.Hex("A33D57"));
            p.Shape(Sd.Ellipse(0, -3.3f, 1.9f, 0.5f), Rgba.Hex("E8B04A"), 0.04f);
            p.Shape(Sd.Ellipse(0, -3.3f, 1.2f, 0.3f), Rgba.Hex("C94F6D"), 0.04f);

            // finestra con luna e stelle
            Sdf glass = Sd.Box(1.6f, 2.3f, 0.8f, 0.95f, 0.05f);
            p.Shape(Sd.Box(1.6f, 2.3f, 0.95f, 1.1f, 0.08f), Rgba.Hex("E9DCC4"));
            p.FillFunc(glass, (x, y) => Rgba.Lerp(Rgba.Hex("0F1638"), Rgba.Hex("24306B"), Clamp((y - 1.3f) / 2f)));
            p.Shape(Sd.Subtract(Sd.Circle(1.85f, 2.75f, 0.32f), Sd.Circle(2.0f, 2.85f, 0.27f)), Rgba.Hex("FFE680"), 0.035f);
            foreach (var st in new[] { new[] { 1.1f, 2.9f }, new[] { 1.35f, 1.8f }, new[] { 2.1f, 1.7f }, new[] { 1.25f, 2.4f } })
            {
                p.Line(Rgba.Hex("FFF4B8"), 0.025f, st[0] - 0.07f, st[1], st[0] + 0.07f, st[1]);
                p.Line(Rgba.Hex("FFF4B8"), 0.025f, st[0], st[1] - 0.07f, st[0], st[1] + 0.07f);
            }
            p.Shape(glass, Rgba.Clear, 0.04f);
            p.InkLine(0.04f, 1.6f, 1.35f, 1.6f, 3.25f);
            p.InkLine(0.04f, 0.8f, 2.3f, 2.4f, 2.3f);
            // tende
            foreach (int side in new[] { -1, 1 })
            {
                float x = 1.6f + side * 1.05f;
                Sdf curtain = Sd.Polygon(x - 0.22f, 3.55f, x + 0.22f, 3.55f, x + 0.12f * side + 0.1f, 1.0f, x - 0.1f + 0.12f * side, 1.0f);
                Body(p, curtain, Rgba.Hex("7C63D9"), Rgba.Hex("5D47B3"));
            }
            // quadro col mare: il pesce (oggetto a parte) si può prendere e dare al gatto
            p.Shape(Sd.Box(-1.65f, 2.3f, 0.68f, 0.56f, 0.04f), Gold);
            Sdf picture = Sd.Box(-1.65f, 2.3f, 0.54f, 0.42f);
            p.FillFunc(picture, (x, y) => Rgba.Lerp(Rgba.Hex("8FD8FF"), Rgba.Hex("3E9BDB"), Clamp((2.6f - y) / 0.7f)));
            for (int i = 0; i < 3; i++)
                p.Line(Rgba.White.WithAlpha(0.7f), 0.02f, -2.1f, 2.0f + i * 0.2f, -1.95f, 2.05f + i * 0.2f, -1.8f, 2.0f + i * 0.2f, -1.65f, 2.05f + i * 0.2f, -1.5f, 2.0f + i * 0.2f, -1.35f, 2.05f + i * 0.2f, -1.2f, 2.0f + i * 0.2f);
            p.Shape(Sd.Union(Sd.Ellipse(-1.95f, 1.95f, 0.1f, 0.18f), Sd.Ellipse(-1.35f, 1.93f, 0.08f, 0.13f)), Leaf, 0.02f);
            p.Shape(picture, Rgba.Clear, 0.035f);
            return new DoodleArt("night_bg", p);
        }

        /// <summary>Gatto arancione che dorme acciambellato su un cuscino (centro del cuscino in 0,-0.45).</summary>
        public static DoodleArt SleepingCat()
        {
            var p = new DoodlePainter(-1.6f, -0.95f, 1.6f, 1.15f, 120) { Ink = 0.05f };
            // cuscino con nappine
            Sdf cushion = Sd.Box(0, -0.45f, 1.35f, 0.3f, 0.26f);
            foreach (float x in new[] { -1.32f, 1.32f })
            {
                p.InkLine(0.025f, x, -0.6f, x + Math.Sign(x) * 0.12f, -0.82f);
                p.Shape(Sd.Circle(x + Math.Sign(x) * 0.13f, -0.85f, 0.06f), Rgba.Hex("FFD25A"), 0.025f);
            }
            Body(p, cushion, Rgba.Hex("8E6CEF"), Rgba.Hex("6B4CC9"));
            p.Line(Rgba.Hex("B9A3FF"), 0.02f, -1.1f, -0.45f, 1.1f, -0.45f);
            foreach (float x in new[] { -0.6f, 0f, 0.6f }) p.Shape(Sd.Circle(x, -0.45f, 0.035f), Rgba.Hex("B9A3FF"), 0.02f);

            // coda che avvolge il corpo
            Sdf tail = Sd.Polyline(0.11f, 0.92f, 0.05f, 1.1f, -0.15f, 0.85f, -0.2f, 0.3f, -0.17f, -0.15f, -0.12f);
            p.Shape(tail, CatOrange);
            // orecchie
            Sdf earL = Sd.Polygon(-1.05f, 0.5f, -0.98f, 0.92f, -0.72f, 0.66f);
            Sdf earR = Sd.Polygon(-0.68f, 0.68f, -0.42f, 0.9f, -0.38f, 0.52f);
            p.Shape(Sd.Union(earL, earR), CatOrange);
            p.Fill(Sd.Grow(Sd.Union(earL, earR), -0.06f), CatPink);
            // corpo
            Sdf body = Sd.Ellipse(0.12f, 0.12f, 0.95f, 0.5f);
            Body(p, body, CatOrange, CatShade, null, -0.08f, 0.1f);
            for (int i = 0; i < 4; i++)
            {
                float x = -0.1f + i * 0.28f;
                p.Line(CatShade, 0.05f, x, 0.6f, x + 0.08f, 0.4f, x + 0.04f, 0.22f);
            }
            p.Shape(body, Rgba.Clear);
            // testa
            Sdf head = Sd.Ellipse(-0.72f, 0.33f, 0.42f, 0.37f);
            Body(p, head, CatOrange, CatShade, null, -0.04f, 0.06f);
            p.Line(CatShade, 0.04f, -0.78f, 0.68f, -0.74f, 0.55f);
            p.Line(CatShade, 0.04f, -0.66f, 0.68f, -0.66f, 0.55f);
            p.Fill(Sd.Ellipse(-0.7f, 0.15f, 0.2f, 0.12f), Rgba.Hex("FFE3C2"));
            // occhi chiusi, naso, baffi
            p.InkLine(0.025f, -0.92f, 0.36f, -0.86f, 0.31f, -0.79f, 0.36f);
            p.InkLine(0.025f, -0.64f, 0.36f, -0.58f, 0.31f, -0.51f, 0.36f);
            p.Shape(Sd.Polygon(-0.75f, 0.22f, -0.65f, 0.22f, -0.7f, 0.16f), CatPink, 0.02f);
            p.InkLine(0.015f, -0.7f, 0.16f, -0.74f, 0.11f, -0.79f, 0.12f);
            p.InkLine(0.015f, -0.7f, 0.16f, -0.66f, 0.11f, -0.61f, 0.12f);
            foreach (float dy in new[] { 0.0f, 0.06f })
            {
                p.InkLine(0.012f, -0.86f, 0.17f + dy, -1.15f, 0.2f + dy * 1.6f);
                p.InkLine(0.012f, -0.54f, 0.17f + dy, -0.27f, 0.2f + dy * 1.6f);
            }
            // zampe davanti
            foreach (float x in new[] { -0.42f, -0.18f })
                p.Shape(Sd.Ellipse(x, -0.22f, 0.13f, 0.08f), Rgba.Hex("FFE3C2"), 0.035f);
            return new DoodleArt("cat", p);
        }

        /// <summary>Occhi spalancati del gatto (si sovrappongono a quelli chiusi).</summary>
        public static DoodleArt CatEyesOpen()
        {
            var p = new DoodlePainter(-1.05f, 0.18f, -0.38f, 0.55f, 160) { Ink = 0.025f };
            foreach (float x in new[] { -0.86f, -0.58f })
            {
                p.Shape(Sd.Ellipse(x, 0.35f, 0.11f, 0.13f), Rgba.Hex("FFF7A8"));
                p.Fill(Sd.Ellipse(x + 0.01f, 0.34f, 0.035f, 0.09f), Ink);
                p.Fill(Sd.Circle(x - 0.03f, 0.39f, 0.02f), Rgba.White);
            }
            return new DoodleArt("cat_eyes", p);
        }

        /// <summary>Punto esclamativo rosso.</summary>
        public static DoodleArt Alert()
        {
            var p = new DoodlePainter(-0.2f, -0.35f, 0.2f, 0.4f, 120) { Ink = 0.04f };
            p.Shape(Sd.Polygon(-0.11f, 0.33f, 0.11f, 0.33f, 0.05f, -0.1f, -0.05f, -0.1f), Rgba.Hex("FF4D5E"));
            p.Shape(Sd.Circle(0, -0.24f, 0.07f), Rgba.Hex("FF4D5E"));
            return new DoodleArt("alert", p);
        }

        /// <summary>Lettera Z del sonno.</summary>
        public static DoodleArt Zzz()
        {
            var p = new DoodlePainter(-0.24f, -0.24f, 0.24f, 0.24f, 120) { Ink = 0.035f };
            p.Shape(Sd.Polyline(0.045f, -0.14f, 0.14f, 0.14f, 0.14f, -0.14f, -0.14f, 0.14f, -0.14f), Rgba.Hex("BFE3FF"));
            return new DoodleArt("zzz", p);
        }

        /// <summary>Cuoricino.</summary>
        public static DoodleArt Heart()
        {
            var p = new DoodlePainter(-0.24f, -0.24f, 0.24f, 0.22f, 120) { Ink = 0.035f };
            Sdf heart = Sd.Union(Sd.Circle(-0.08f, 0.06f, 0.1f), Sd.Circle(0.08f, 0.06f, 0.1f), Sd.Polygon(-0.17f, 0.02f, 0.17f, 0.02f, 0, -0.18f));
            Body(p, heart, Rgba.Hex("FF5C8A"), Rgba.Hex("D93D6B"), null, -0.02f, 0.03f);
            p.Fill(Sd.Circle(-0.1f, 0.09f, 0.025f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("heart", p);
        }

        /// <summary>Pulsantone rosso: base grigia e cupola (centro in 0,0).</summary>
        public static DoodleArt BigButton() { return BigButton("big_button", Rgba.Hex("FF4D5E"), Rgba.Hex("D12F43")); }

        public static DoodleArt BigButton(string name, Rgba dome, Rgba domeShade)
        {
            var p = new DoodlePainter(-0.95f, -0.5f, 0.95f, 0.55f, 120) { Ink = 0.05f };
            Sdf baseShape = Sd.Box(0, -0.22f, 0.85f, 0.22f, 0.12f);
            Body(p, baseShape, Rgba.Hex("8D93A6"), Rgba.Hex("666B7D"));
            p.Fill(Sd.Box(0, -0.06f, 0.78f, 0.04f), Rgba.Hex("B9BED0"));
            Sdf domeShape = Sd.Intersect(Sd.Ellipse(0, 0.0f, 0.62f, 0.45f), Sd.Box(0, 0.25f, 0.7f, 0.27f));
            Body(p, domeShape, dome, domeShade, null, -0.06f, 0.06f);
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.25f, 0.3f, 0.14f, 0.05f), -0.25f, 0.3f, 20f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt(name, p);
        }

        /// <summary>Pesce del quadro (centro in 0,0, guarda a sinistra).</summary>
        public static DoodleArt Fish()
        {
            var p = new DoodlePainter(-0.36f, -0.22f, 0.36f, 0.22f, 160) { Ink = 0.03f };
            Sdf fish = Sd.Union(Sd.Ellipse(-0.04f, 0, 0.22f, 0.13f), Sd.Polygon(0.12f, 0, 0.3f, 0.13f, 0.3f, -0.13f));
            Body(p, fish, Rgba.Hex("FF9F43"), Rgba.Hex("E07B1F"), null, -0.02f, 0.03f);
            p.Line(Rgba.Hex("E07B1F"), 0.02f, 0.04f, 0.1f, 0.0f, 0f, 0.04f, -0.1f);
            p.Fill(Sd.Circle(-0.15f, 0.03f, 0.035f), Rgba.White);
            p.Fill(Sd.Circle(-0.155f, 0.03f, 0.018f), Ink);
            p.Shape(Sd.Polygon(-0.02f, 0.11f, 0.1f, 0.2f, 0.12f, 0.08f), Rgba.Hex("FFC36B"), 0.02f);
            return new DoodleArt("fish", p);
        }

        // ---------- Livello 5: taglia la mela ----------

        static readonly Rgba AppleRed = Rgba.Hex("E8443A"), AppleShade = Rgba.Hex("B92E2A"), Flesh = Rgba.Hex("FFF1C9");
        public const float AppleRadius = 0.75f;

        /// <summary>Cucina: piastrelle, mensola con barattoli, piano di lavoro (coordinate del livello; piano sotto y = -2).</summary>
        public static DoodleArt Kitchen()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            Sdf wall = Sd.Box(0, 2.5f, 5f, 4.6f);
            p.Fill(wall, Rgba.Hex("D4F1E8"), false);
            p.Hatch(wall, Rgba.Hex("A8DCCB"), 0.5f, 0.03f, 0f);
            p.Hatch(wall, Rgba.Hex("A8DCCB"), 0.5f, 0.03f, 90f);
            // mensola con barattoli
            p.Shape(Sd.Box(-1.2f, 1.6f, 1.4f, 0.07f, 0.02f), Wood);
            Rgba[] jar = { Rgba.Hex("FFD25A"), Rgba.Hex("FF8FA3"), Rgba.Hex("7FD1FF") };
            for (int i = 0; i < 3; i++)
            {
                float x = -2.1f + i * 0.85f;
                Body(p, Sd.Box(x, 2.0f, 0.25f, 0.33f, 0.08f), jar[i], Rgba.Lerp(jar[i], Ink, 0.25f));
                p.Shape(Sd.Box(x, 2.38f, 0.2f, 0.06f, 0.02f), Rgba.Hex("8C6A55"), 0.03f);
            }
            // padella appesa
            p.InkLine(0.03f, 1.9f, 3.1f, 1.9f, 2.7f);
            p.Shape(Sd.Capsule(1.9f, 2.6f, 1.9f, 1.9f, 0.06f), Rgba.Hex("5A4A55"));
            Body(p, Sd.Circle(1.9f, 1.4f, 0.5f), Rgba.Hex("6B6F7E"), Rgba.Hex("4A4E5C"));
            p.Fill(Sd.Circle(1.9f, 1.4f, 0.36f), Rgba.Hex("3A3D49"));
            // piano di lavoro
            Sdf counter = Sd.Box(0, -4.6f, 5f, 2.6f);
            p.Fill(counter, Rgba.Hex("C98B55"), false);
            foreach (float y in new[] { -2.6f, -3.4f, -4.4f }) p.Line(Ink.WithAlpha(0.4f), 0.03f, -4.5f, y, 4.5f, y + 0.03f);
            p.Shape(Sd.Box(0, -2.0f, 5f, 0.12f), Rgba.Hex("E2A86F"));
            return new DoodleArt("kitchen_bg", p);
        }

        /// <summary>Tagliere di legno (centro in 0,0).</summary>
        public static DoodleArt Board()
        {
            var p = new DoodlePainter(-1.8f, -0.4f, 1.8f, 0.4f, 100) { Ink = 0.05f };
            Sdf board = Sd.Union(Sd.Box(-0.2f, 0, 1.4f, 0.28f, 0.2f), Sd.Box(1.4f, 0, 0.32f, 0.12f, 0.1f));
            Body(p, board, Rgba.Hex("E9B97A"), Rgba.Hex("C98F4E"));
            p.Shape(Sd.Circle(1.55f, 0, 0.05f), Rgba.Hex("C98F4E"), 0.02f);
            p.Line(Rgba.Hex("C98F4E"), 0.02f, -1.3f, 0.08f, -0.5f, 0.1f, 0.3f, 0.07f);
            return new DoodleArt("board", p);
        }

        /// <summary>Mela intera (centro in 0,0, raggio 0.75).</summary>
        public static DoodleArt Apple()
        {
            var p = new DoodlePainter(-0.85f, -0.82f, 0.85f, 1.15f, 120) { Ink = 0.05f };
            float r = AppleRadius;
            p.Shape(Sd.Polyline(0.04f, 0.0f, 0.62f, 0.05f, 0.85f, 0.12f, 1.0f), Rgba.Hex("7A4A2A"), 0.03f);
            p.Shape(Sd.Rotate(Sd.Ellipse(0.32f, 0.88f, 0.22f, 0.09f), 0.12f, 0.85f, 20f), Leaf, 0.035f);
            p.Line(LeafShade, 0.015f, 0.14f, 0.85f, 0.48f, 0.94f);
            Sdf apple = Sd.Union(Sd.Circle(-0.2f, 0.02f, r * 0.82f), Sd.Circle(0.2f, 0.02f, r * 0.82f), Sd.Circle(0, -0.08f, r * 0.85f));
            Body(p, apple, AppleRed, AppleShade, null, -0.08f, 0.1f);
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.35f, 0.25f, 0.13f, 0.07f), -0.35f, 0.25f, 40f), Rgba.White.WithAlpha(0.8f));
            p.Line(AppleShade, 0.025f, -0.05f, 0.55f, 0.0f, 0.62f, 0.05f, 0.55f);
            foreach (var d in new[] { new[] { 0.3f, -0.2f }, new[] { 0.42f, 0.1f }, new[] { -0.1f, -0.45f } })
                p.Fill(Sd.Circle(d[0], d[1], 0.025f), Rgba.Hex("FFB0A0"), false);
            return new DoodleArt("apple", p);
        }

        /// <summary>Metà mela vista dal taglio: buccia, polpa e semi. side = -1 sinistra, +1 destra (il taglio è in x = 0).</summary>
        public static DoodleArt AppleHalf(int side)
        {
            float s = side, r = AppleRadius;
            var p = new DoodlePainter(side < 0 ? -0.82f : -0.05f, -0.82f, side < 0 ? 0.05f : 0.82f, 0.95f, 120) { Ink = 0.05f };
            Sdf whole = Sd.Union(Sd.Circle(-0.2f, 0.02f, r * 0.82f), Sd.Circle(0.2f, 0.02f, r * 0.82f), Sd.Circle(0, -0.08f, r * 0.85f));
            Sdf half = Sd.Intersect(whole, Sd.Box(s * 0.5f, 0, 0.5f, 1.2f));
            Body(p, half, AppleRed, AppleShade, null, -0.08f, 0.1f);
            Sdf face = Sd.Intersect(Sd.Grow(whole, -0.07f), Sd.Box(s * 0.32f, 0, 0.32f, 1.2f));
            p.Fill(face, Flesh);
            p.Hatch(face, Rgba.Hex("F4DFA6"), 0.08f, 0.015f, 70f);
            p.Shape(Sd.Ellipse(s * 0.06f, -0.02f, 0.05f, 0.3f), Rgba.Hex("F4DFA6"), 0.02f);
            foreach (float y in new[] { 0.1f, -0.15f })
                p.Shape(Sd.Rotate(Sd.Ellipse(s * 0.13f, y, 0.035f, 0.06f), s * 0.13f, y, s * 20f), Rgba.Hex("5A3420"), 0.015f);
            p.Shape(half, Rgba.Clear);
            return new DoodleArt(side < 0 ? "apple_left" : "apple_right", p);
        }

        /// <summary>Goccia di succo.</summary>
        public static DoodleArt JuiceDrop()
        {
            var p = new DoodlePainter(-0.12f, -0.12f, 0.12f, 0.16f, 120) { Ink = 0.025f };
            Sdf drop = Sd.Union(Sd.Circle(0, -0.03f, 0.07f), Sd.Polygon(-0.06f, 0f, 0.06f, 0f, 0, 0.13f));
            p.Shape(drop, Rgba.Hex("FFE58A"));
            p.Fill(Sd.Circle(-0.025f, -0.01f, 0.018f), Rgba.White);
            return new DoodleArt("juice", p);
        }

        // ---------- Livello 6: i suoni ----------

        public static readonly string[] MorseCodes = { "...", "---", ".--", "-.-", ".-." };

        /// <summary>Stanza con la carta da parati verde e la scrivania (coordinate del livello; scrivania sotto y = -0.6).</summary>
        public static DoodleArt RadioRoom()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            Sdf wall = Sd.Box(0, 3f, 5f, 4f);
            p.Fill(wall, Rgba.Hex("2F6B5E"), false);
            p.Hatch(wall, Rgba.Hex("3A7D6E"), 0.55f, 0.2f, 0f);
            Sdf desk = Sd.Box(0, -3.9f, 5f, 3.3f);
            p.Fill(desk, Rgba.Hex("9C6B43"), false);
            foreach (float y in new[] { -1.4f, -2.6f, -3.9f, -5.2f }) p.Line(Rgba.Hex("7E5232"), 0.04f, -4.5f, y, 4.5f, y + 0.05f);
            p.Shape(Sd.Box(0, -0.62f, 5f, 0.1f), Rgba.Hex("B98457"));
            // poster con le onde sonore
            p.Shape(Sd.Box(-2.0f, 3.1f, 0.62f, 0.45f, 0.04f), Rgba.Hex("FFF3D6"));
            p.InkLine(0.03f, -2.5f, 3.1f, -2.35f, 3.35f, -2.2f, 2.85f, -2.05f, 3.4f, -1.9f, 2.8f, -1.75f, 3.3f, -1.6f, 3.1f, -1.45f, 3.1f);
            return new DoodleArt("radio_bg", p);
        }

        /// <summary>Vecchia radio di legno con altoparlante, manopole e spia spenta (centro 0,0).</summary>
        public static DoodleArt Radio()
        {
            var p = new DoodlePainter(-1.45f, -1.05f, 1.45f, 1.25f, 100) { Ink = 0.06f };
            p.InkLine(0.04f, 0.6f, 0.95f, 1.1f, 1.2f);   // antenna
            p.Shape(Sd.Circle(1.12f, 1.2f, 0.05f), Rgba.Hex("E23B3B"), 0.025f);
            Sdf box = Sd.Box(0, 0, 1.35f, 0.95f, 0.25f);
            Body(p, box, Rgba.Hex("C9773B"), Rgba.Hex("9C5A28"));
            Sdf grill = Sd.Box(-0.38f, 0.0f, 0.75f, 0.68f, 0.18f);
            p.Shape(grill, Rgba.Hex("F2E3C6"), 0.04f);
            p.Hatch(Sd.Grow(grill, -0.06f), Rgba.Hex("B9A27C"), 0.12f, 0.04f, 0f);
            p.Shape(Sd.Circle(0.85f, 0.32f, 0.22f), Rgba.Hex("3A3540"), 0.035f);
            p.InkLine(0.03f, 0.85f, 0.32f, 0.85f, 0.5f);
            p.Shape(Sd.Circle(0.85f, -0.32f, 0.22f), Rgba.Hex("3A3540"), 0.035f);
            p.InkLine(0.03f, 0.85f, -0.32f, 0.98f, -0.2f);
            p.Shape(Sd.Circle(0.45f, 0.72f, 0.1f), Rgba.Hex("7A6B5A"), 0.03f);   // spia (spenta)
            return new DoodleArt("radio", p);
        }

        /// <summary>Alone luminoso della spia della radio.</summary>
        public static DoodleArt RadioLamp()
        {
            var p = new DoodlePainter(-0.35f, -0.35f, 0.35f, 0.35f, 120) { Ink = 0.03f, Wobble = 0f };
            p.FillFunc(Sd.Circle(0, 0, 0.35f), (x, y) => Rgba.Hex("FFE36B").WithAlpha((float)Math.Exp(-Math.Sqrt(x * x + y * y) * 9f) * 0.9f));
            p.Shape(Sd.Circle(0, 0, 0.1f), Rgba.Hex("FFF27A"));
            return new DoodleArt("radio_lamp", p);
        }

        /// <summary>Tasto con il codice disegnato (punti e linee).</summary>
        public static DoodleArt MorseKey(int index)
        {
            var p = new DoodlePainter(-0.8f, -0.55f, 0.8f, 0.55f, 120) { Ink = 0.055f };
            Sdf key = Sd.Box(0, 0, 0.72f, 0.46f, 0.18f);
            Body(p, key, Rgba.Hex("FFF3D6"), Rgba.Hex("E2CFA6"));
            string code = MorseCodes[index];
            const float dot = 0.08f, dash = 0.36f, gap = 0.1f;
            float total = 0f;
            foreach (char c in code) total += (c == '.' ? dot * 2f : dash) + gap;
            total -= gap;
            float x = -total / 2f;
            foreach (char c in code)
            {
                if (c == '.') { p.Fill(Sd.Circle(x + dot, 0, dot), Ink); x += dot * 2f + gap; }
                else { p.Fill(Sd.Capsule(x + 0.06f, 0, x + dash - 0.06f, 0, 0.06f), Ink); x += dash + gap; }
            }
            return new DoodleArt("morse_" + index, p);
        }

        // ---------- Livello 7: la bomba ----------

        /// <summary>Laboratorio con pannello forato e banco (coordinate del livello).</summary>
        public static DoodleArt Workshop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            Sdf wall = Sd.Box(0, 0, 5f, 7.1f);
            p.Fill(wall, Rgba.Hex("4B4E63"), false);
            var rnd = new Random(9);
            for (float y = -6.5f; y < 6.5f; y += 0.45f)
                for (float x = -4.3f; x < 4.4f; x += 0.45f)
                    p.Fill(Sd.Circle(x, y, 0.045f), Rgba.Hex("3A3C4E"), false);
            // banco da lavoro
            Sdf bench = Sd.Box(0, -5.5f, 5f, 2.2f);
            p.Fill(bench, Rgba.Hex("7E5A3C"), false);
            p.Shape(Sd.Box(0, -3.35f, 5f, 0.1f), Rgba.Hex("9C744F"));
            // attrezzi appesi
            p.Shape(Sd.Rotate(Sd.Box(-2.2f, 2.9f, 0.08f, 0.6f, 0.04f), -2.2f, 2.9f, 10f), Rgba.Hex("C94F4F"));
            p.Shape(Sd.Rotate(Sd.Box(-2.13f, 3.6f, 0.25f, 0.12f, 0.03f), -2.2f, 2.9f, 10f), Rgba.Hex("A7AEBF"));
            p.Shape(Sd.Rotate(Sd.Box(2.2f, 2.8f, 0.07f, 0.55f, 0.04f), 2.2f, 2.8f, -8f), Rgba.Hex("E8B04A"));
            p.Shape(Sd.Rotate(Sd.Subtract(Sd.Circle(2.15f, 3.45f, 0.22f), Sd.Box(2.15f, 3.62f, 0.08f, 0.12f)), 2.2f, 2.8f, -8f), Rgba.Hex("A7AEBF"));
            return new DoodleArt("workshop_bg", p);
        }

        /// <summary>Tre candelotti di dinamite legati col nastro; il timer sta sopra (centro 0,0).</summary>
        public static DoodleArt Dynamite()
        {
            var p = new DoodlePainter(-1.05f, -1.15f, 1.05f, 1.0f, 100) { Ink = 0.055f };
            foreach (float x in new[] { -0.62f, 0.62f, 0f })
            {
                Sdf stick = Sd.Box(x, -0.1f, 0.3f, 0.95f, 0.12f);
                Body(p, stick, Rgba.Hex("E23B3B"), Rgba.Hex("B22A2A"), null, -0.05f, 0.05f);
                p.Fill(Sd.Ellipse(x, 0.83f, 0.24f, 0.07f), Rgba.Hex("F2C6A0"));
                p.Shape(Sd.Ellipse(x, 0.83f, 0.24f, 0.07f), Rgba.Clear, 0.025f);
            }
            foreach (float y in new[] { 0.45f, -0.65f })
            {
                Sdf tape = Sd.Box(0, y, 0.95f, 0.1f, 0.03f);
                Body(p, tape, Rgba.Hex("6B6F7E"), Rgba.Hex("4A4E5C"), null, -0.02f, 0.03f);
            }
            return new DoodleArt("dynamite", p);
        }

        /// <summary>Scatola del timer con display LCD e morsetto in basso (centro 0,0).</summary>
        public static DoodleArt TimerBox()
        {
            var p = new DoodlePainter(-0.9f, -0.62f, 0.9f, 0.5f, 100) { Ink = 0.055f };
            Sdf box = Sd.Box(0, 0, 0.82f, 0.42f, 0.08f);
            Body(p, box, Rgba.Hex("3A3D49"), Rgba.Hex("2A2C36"));
            p.Shape(Sd.Box(0, 0.03f, 0.66f, 0.27f, 0.04f), Rgba.Hex("1A0D0D"), 0.035f);
            foreach (float x in new[] { -0.35f, 0.35f })
                p.Shape(Sd.Box(x, -0.5f, 0.09f, 0.08f, 0.02f), Rgba.Hex("B7BCC9"), 0.03f);   // morsetti
            return new DoodleArt("timer_box", p);
        }

        /// <summary>Estremità libera del cavo: fili di rame sfilacciati (punta verso destra).</summary>
        public static DoodleArt WireEnd()
        {
            var p = new DoodlePainter(-0.12f, -0.12f, 0.2f, 0.12f, 150) { Ink = 0.02f };
            p.Line(Rgba.Hex("E08A3C"), 0.025f, -0.05f, 0, 0.15f, 0.06f);
            p.Line(Rgba.Hex("E08A3C"), 0.025f, -0.05f, 0, 0.17f, 0f);
            p.Line(Rgba.Hex("E08A3C"), 0.025f, -0.05f, 0, 0.14f, -0.07f);
            return new DoodleArt("wire_end", p);
        }

        /// <summary>Esplosione a stella.</summary>
        public static DoodleArt Boom()
        {
            var p = new DoodlePainter(-2.0f, -2.0f, 2.0f, 2.0f, 80) { Ink = 0.08f };
            float[] outer = new float[32], inner = new float[32];
            var rnd = new Random(4);
            for (int i = 0; i < 16; i++)
            {
                double a = i * Math.PI / 8;
                float r = i % 2 == 0 ? 1.85f - (float)rnd.NextDouble() * 0.3f : 1.1f;
                outer[i * 2] = (float)Math.Cos(a) * r; outer[i * 2 + 1] = (float)Math.Sin(a) * r;
                inner[i * 2] = (float)Math.Cos(a) * r * 0.62f; inner[i * 2 + 1] = (float)Math.Sin(a) * r * 0.62f;
            }
            p.Shape(Sd.Polygon(outer), Rgba.Hex("FF6B3D"));
            p.Shape(Sd.Polygon(inner), Rgba.Hex("FFE14A"));
            return new DoodleArt("boom", p);
        }
    }
}
