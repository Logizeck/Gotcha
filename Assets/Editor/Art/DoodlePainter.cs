using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Colore RGBA in float, alpha non premoltiplicata.</summary>
    public struct Rgba
    {
        public float r, g, b, a;

        public Rgba(float r, float g, float b, float a = 1f) { this.r = r; this.g = g; this.b = b; this.a = a; }

        public static Rgba Hex(string hex, float a = 1f)
        {
            hex = hex.TrimStart('#');
            return new Rgba(
                Convert.ToInt32(hex.Substring(0, 2), 16) / 255f,
                Convert.ToInt32(hex.Substring(2, 2), 16) / 255f,
                Convert.ToInt32(hex.Substring(4, 2), 16) / 255f, a);
        }

        public Rgba WithAlpha(float alpha) { return new Rgba(r, g, b, alpha); }

        public static Rgba Lerp(Rgba x, Rgba y, float t)
        {
            return new Rgba(x.r + (y.r - x.r) * t, x.g + (y.g - x.g) * t, x.b + (y.b - x.b) * t, x.a + (y.a - x.a) * t);
        }

        public static readonly Rgba Clear = new Rgba(0, 0, 0, 0);
        public static readonly Rgba White = new Rgba(1, 1, 1, 1);
    }

    /// <summary>Distanza con segno: negativa dentro la forma, positiva fuori (unità mondo).</summary>
    public delegate float Sdf(float x, float y);

    /// <summary>
    /// Pittore "doodle" in puro C# (nessuna dipendenza da Unity): disegna forme definite da funzioni distanza
    /// con riempimento, contorno a inchiostro tremolante, tratteggi e linee. Lavora in unità mondo: le coordinate
    /// dei disegni coincidono con quelle locali dell'oggetto in gioco. Pixels[0] è in basso a sinistra.
    /// </summary>
    public class DoodlePainter
    {
        public static readonly Rgba DefaultInk = Rgba.Hex("2B2233");

        public readonly int Width, Height;
        public readonly float Ppu;
        public readonly float XMin, YMin, XMax, YMax;
        public readonly Rgba[] Pixels;

        public Rgba InkColor = DefaultInk;
        public float Ink = 0.05f;         // spessore del contorno
        public float Wobble = 0.012f;     // ampiezza del tremolio del tratto
        public float WobbleFreq = 3.2f;   // frequenza del tremolio
        public int Seed = 1;

        public float CenterX => (XMin + XMax) * 0.5f;
        public float CenterY => (YMin + YMax) * 0.5f;

        public DoodlePainter(float xMin, float yMin, float xMax, float yMax, float ppu)
        {
            XMin = xMin; YMin = yMin; Ppu = ppu;
            Width = Math.Max(4, (int)Math.Ceiling((xMax - xMin) * ppu));
            Height = Math.Max(4, (int)Math.Ceiling((yMax - yMin) * ppu));
            XMax = xMin + Width / ppu;
            YMax = yMin + Height / ppu;
            Pixels = new Rgba[Width * Height];
        }

        // ---------- Primitive di disegno ----------

        /// <summary>Forma piena con contorno a inchiostro.</summary>
        public DoodlePainter Shape(Sdf sdf, Rgba fill, float? ink = null)
        {
            float t = ink ?? Ink;
            int seed = Seed++;
            ForEach((x, y) =>
            {
                float d = sdf(x, y) + Jitter(x, y, seed);
                float outer = Cov(d - t * 0.5f);
                if (outer <= 0f) return Rgba.Clear;
                float inner = Cov(d + t * 0.5f);
                float ia = inner * fill.a, ka = (outer - inner) * InkColor.a;
                float a = ia + ka;
                if (a <= 0f) return Rgba.Clear;
                return new Rgba((fill.r * ia + InkColor.r * ka) / a, (fill.g * ia + InkColor.g * ka) / a,
                                (fill.b * ia + InkColor.b * ka) / a, a);
            });
            return this;
        }

        /// <summary>Riempimento senza contorno (ombre, luci).</summary>
        public DoodlePainter Fill(Sdf sdf, Rgba fill, bool wobble = true)
        {
            int seed = Seed++;
            ForEach((x, y) =>
            {
                float d = sdf(x, y) + (wobble ? Jitter(x, y, seed) : 0f);
                float c = Cov(d);
                return c <= 0f ? Rgba.Clear : fill.WithAlpha(fill.a * c);
            });
            return this;
        }

        /// <summary>Riempimento con colore che dipende dal punto (sfumature).</summary>
        public DoodlePainter FillFunc(Sdf sdf, Func<float, float, Rgba> color)
        {
            ForEach((x, y) =>
            {
                float c = Cov(sdf(x, y));
                if (c <= 0f) return Rgba.Clear;
                Rgba col = color(x, y);
                return col.WithAlpha(col.a * c);
            });
            return this;
        }

        /// <summary>Tratteggio a righe parallele dentro una forma (ombre "a matita").</summary>
        public DoodlePainter Hatch(Sdf region, Rgba color, float spacing, float lineWidth, float angleDeg)
        {
            float a = angleDeg * (float)Math.PI / 180f;
            float ca = (float)Math.Cos(a), sa = (float)Math.Sin(a);
            int seed = Seed++;
            ForEach((x, y) =>
            {
                float rc = Cov(region(x, y));
                if (rc <= 0f) return Rgba.Clear;
                float u = (x * ca + y * sa) / spacing + Jitter(x, y, seed) * 6f;
                float f = Math.Abs(u - (float)Math.Floor(u) - 0.5f) * spacing;
                float lc = Cov(f - lineWidth * 0.5f);
                return lc <= 0f ? Rgba.Clear : color.WithAlpha(color.a * lc * rc);
            });
            return this;
        }

        /// <summary>Linea a mano libera (spezzata con estremi arrotondati).</summary>
        public DoodlePainter Line(Rgba color, float width, params float[] xy)
        {
            Sdf s = Sd.Polyline(width * 0.5f, xy);
            return Fill(s, color);
        }

        public DoodlePainter InkLine(float width, params float[] xy) { return Line(InkColor, width, xy); }

        // ---------- Interni ----------

        float Cov(float d) { return Clamp01(0.5f - d * Ppu); }

        float Jitter(float x, float y, int seed)
        {
            if (Wobble <= 0f) return 0f;
            return (Noise.Value(x * WobbleFreq + seed * 17.3f, y * WobbleFreq - seed * 5.1f) - 0.5f) * 2f * Wobble;
        }

        void ForEach(Func<float, float, Rgba> src)
        {
            for (int j = 0; j < Height; j++)
            {
                float y = YMin + (j + 0.5f) / Ppu;
                for (int i = 0; i < Width; i++)
                {
                    float x = XMin + (i + 0.5f) / Ppu;
                    Rgba s = src(x, y);
                    if (s.a <= 0f) continue;
                    int k = j * Width + i;
                    Pixels[k] = Over(s, Pixels[k]);
                }
            }
        }

        static Rgba Over(Rgba s, Rgba d)
        {
            float a = s.a + d.a * (1f - s.a);
            if (a <= 0f) return Rgba.Clear;
            float k = d.a * (1f - s.a);
            return new Rgba((s.r * s.a + d.r * k) / a, (s.g * s.a + d.g * k) / a, (s.b * s.a + d.b * k) / a, a);
        }

        static float Clamp01(float v) { return v < 0f ? 0f : v > 1f ? 1f : v; }
    }

    /// <summary>Funzioni distanza delle forme di base e loro combinazioni.</summary>
    public static class Sd
    {
        public static Sdf Circle(float cx, float cy, float r)
        {
            return (x, y) => Len(x - cx, y - cy) - r;
        }

        public static Sdf Ellipse(float cx, float cy, float rx, float ry)
        {
            // approssimazione di Quilez: buona anche per ellissi molto schiacciate
            return (x, y) =>
            {
                float px = x - cx, py = y - cy;
                float k0 = Len(px / rx, py / ry), k1 = Len(px / (rx * rx), py / (ry * ry));
                return k1 > 1e-6f ? k0 * (k0 - 1f) / k1 : -Math.Min(rx, ry);
            };
        }

        /// <summary>Rettangolo (semi-lati hx, hy) con angoli arrotondati di raggio round.</summary>
        public static Sdf Box(float cx, float cy, float hx, float hy, float round = 0f)
        {
            return (x, y) =>
            {
                float qx = Math.Abs(x - cx) - hx + round, qy = Math.Abs(y - cy) - hy + round;
                return Len(Math.Max(qx, 0f), Math.Max(qy, 0f)) + Math.Min(Math.Max(qx, qy), 0f) - round;
            };
        }

        public static Sdf Capsule(float ax, float ay, float bx, float by, float r)
        {
            return (x, y) => SegDist(x, y, ax, ay, bx, by) - r;
        }

        public static Sdf Polyline(float r, params float[] xy)
        {
            return (x, y) =>
            {
                float best = float.MaxValue;
                for (int i = 0; i + 3 < xy.Length; i += 2)
                    best = Math.Min(best, SegDist(x, y, xy[i], xy[i + 1], xy[i + 2], xy[i + 3]));
                return best - r;
            };
        }

        /// <summary>Poligono chiuso (vertici x0,y0,x1,y1,...).</summary>
        public static Sdf Polygon(params float[] xy)
        {
            int n = xy.Length / 2;
            return (x, y) =>
            {
                float d = float.MaxValue;
                bool inside = false;
                for (int i = 0, j = n - 1; i < n; j = i++)
                {
                    float xi = xy[i * 2], yi = xy[i * 2 + 1], xj = xy[j * 2], yj = xy[j * 2 + 1];
                    d = Math.Min(d, SegDist(x, y, xi, yi, xj, yj));
                    if ((yi > y) != (yj > y) && x < (xj - xi) * (y - yi) / (yj - yi) + xi) inside = !inside;
                }
                return inside ? -d : d;
            };
        }

        public static Sdf Union(params Sdf[] s)
        {
            return (x, y) => { float d = float.MaxValue; foreach (Sdf f in s) d = Math.Min(d, f(x, y)); return d; };
        }

        public static Sdf Intersect(Sdf a, Sdf b) { return (x, y) => Math.Max(a(x, y), b(x, y)); }
        public static Sdf Subtract(Sdf a, Sdf b) { return (x, y) => Math.Max(a(x, y), -b(x, y)); }
        public static Sdf Offset(Sdf a, float dx, float dy) { return (x, y) => a(x - dx, y - dy); }
        public static Sdf Grow(Sdf a, float amount) { return (x, y) => a(x, y) - amount; }

        /// <summary>Ruota la forma attorno a (cx, cy) di deg gradi (antiorario).</summary>
        public static Sdf Rotate(Sdf a, float cx, float cy, float deg)
        {
            float r = -deg * (float)Math.PI / 180f;
            float c = (float)Math.Cos(r), s = (float)Math.Sin(r);
            return (x, y) =>
            {
                float px = x - cx, py = y - cy;
                return a(cx + px * c - py * s, cy + px * s + py * c);
            };
        }

        /// <summary>Parte "in ombra" di una forma: ciò che resta togliendo la forma spostata verso la luce.</summary>
        public static Sdf Crescent(Sdf shape, float dx, float dy)
        {
            return Subtract(shape, Offset(shape, dx, dy));
        }

        static float Len(float x, float y) { return (float)Math.Sqrt(x * x + y * y); }

        static float SegDist(float px, float py, float ax, float ay, float bx, float by)
        {
            float vx = bx - ax, vy = by - ay, wx = px - ax, wy = py - ay;
            float l2 = vx * vx + vy * vy;
            float t = l2 > 0f ? Math.Max(0f, Math.Min(1f, (wx * vx + wy * vy) / l2)) : 0f;
            return Len(wx - vx * t, wy - vy * t);
        }
    }

    /// <summary>Value noise 2D deterministico, morbido (per il tremolio dei tratti).</summary>
    public static class Noise
    {
        public static float Value(float x, float y)
        {
            int xi = (int)Math.Floor(x), yi = (int)Math.Floor(y);
            float fx = x - xi, fy = y - yi;
            float u = fx * fx * (3f - 2f * fx), v = fy * fy * (3f - 2f * fy);
            float a = Hash(xi, yi), b = Hash(xi + 1, yi), c = Hash(xi, yi + 1), d = Hash(xi + 1, yi + 1);
            return a + (b - a) * u + (c - a) * v + (a - b - c + d) * u * v;
        }

        static float Hash(int x, int y)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / (float)0xFFFFFF;
            }
        }
    }

    /// <summary>Un disegno pronto: pittore + bordo 9-slice opzionale (in pixel).</summary>
    public class DoodleArt
    {
        public string Name;
        public DoodlePainter Painter;
        public int Border;   // 0 = nessun 9-slice
        public int[] Borders;   // bordi 9-slice diversi per lato, in pixel: sinistra, sotto, destra, sopra (null = Border)
        public DoodleArt(string name, DoodlePainter painter, int border = 0) { Name = name; Painter = painter; Border = border; }
        public DoodleArt(string name, DoodlePainter painter, int left, int bottom, int right, int top)
        { Name = name; Painter = painter; Borders = new[] { left, bottom, right, top }; }
    }

    /// <summary>Raccolta di disegni con nome (usata dal builder e dall'anteprima fuori da Unity).</summary>
    public static class DoodleCatalog
    {
        public static List<DoodleArt> All()
        {
            var all = new List<DoodleArt>
            {
                DoodleSprites.Panel(), DoodleSprites.Key(), DoodleSprites.DoorFrame(), DoodleSprites.DoorPanel(),
                DoodleSprites.Keyhole(), DoodleSprites.RoomBackdrop(), DoodleSprites.MazeBackdrop(),
                DoodleSprites.MazeNeon(), DoodleSprites.ChromeBall(), DoodleSprites.BallShadow(), DoodleSprites.Goal(),
                DoodleSprites.SnowBackdrop(), DoodleSprites.SnowBase(), DoodleSprites.SnowMiddle(), DoodleSprites.SnowArm(-1), DoodleSprites.SnowArm(1), DoodleSprites.SnowHead(),
                DoodleSprites.SnowHat(), DoodleSprites.Blush(), DoodleSprites.Puddle(),
                DoodleSprites.Carrot(), DoodleSprites.Coal(), DoodleSprites.Puff(), DoodleSprites.BubbleBox(),
                DoodleSprites.NightRoom(), DoodleSprites.SleepingCat(), DoodleSprites.CatEyesOpen(), DoodleSprites.Alert(),
                DoodleSprites.Zzz(), DoodleSprites.Heart(), DoodleSprites.BigButton(), DoodleSprites.Fish(),
                DoodleSprites.Kitchen(), DoodleSprites.Board(), DoodleSprites.Apple(), DoodleSprites.AppleHalf(-1),
                DoodleSprites.AppleHalf(1), DoodleSprites.JuiceDrop(),
                DoodleSprites.RadioRoom(), DoodleSprites.Radio(), DoodleSprites.RadioLamp(),
                DoodleSprites.MorseKey(0), DoodleSprites.MorseKey(1), DoodleSprites.MorseKey(2), DoodleSprites.MorseKey(3), DoodleSprites.MorseKey(4),
                DoodleSprites.Workshop(), DoodleSprites.Dynamite(), DoodleSprites.TimerBox(), DoodleSprites.WireEnd(), DoodleSprites.Boom(),
                DoodleSprites.BigButton("blue_button", Rgba.Hex("3E8EF7"), Rgba.Hex("2563C9")),
                DoodleSprites.Guy(false), DoodleSprites.Guy(true), DoodleSprites.Sweat(),
                DoodleSprites.CatFace(0), DoodleSprites.CatFace(1), DoodleSprites.CatFace(2), DoodleSprites.CatFace(3),
                DoodleSprites.AngerMark(), DoodleSprites.HiFi(), DoodleSprites.Speaker(), DoodleSprites.Note(),
                DoodleSprites.Wasp(), DoodleSprites.WaspWing(), DoodleSprites.WaspDead(), DoodleSprites.Garden(),
                DoodleSprites.Swatter(), DoodleSprites.StingFlash(), DoodleSprites.AppIcon(),
                DoodleSprites.Plain(), DoodleSprites.HangingLamp(), DoodleSprites.Glow(), DoodleSprites.LightCone(),
                DoodleSprites.Jar(), DoodleSprites.Cookie(), DoodleSprites.Mouse(), DoodleSprites.PartyRoom(), DoodleSprites.Cake(),
                DoodleSprites.Candle(), DoodleSprites.Flame(), DoodleSprites.Confetti(),
                DoodleSprites.TrapMazeNeon(), DoodleSprites.DarkMazeNeon(), DoodleSprites.Maze2Neon(), DoodleSprites.TrapMaze2Neon(), DoodleSprites.DarkMaze2Neon(), DoodleSprites.TrapMaze3Neon(), DoodleSprites.DarkMaze3Neon(), DoodleSprites.WaspMaze3Neon(), DoodleSprites.WaspMaze6Neon(), DoodleSprites.GateBar(), DoodleSprites.Pit(),
                DoodleSprites.LaserBeam(), DoodleSprites.LaserNode(), DoodleSprites.Darkness(), DoodleSprites.WaspFurious(),
                DoodleSprites.Led(), DoodleSprites.Portrait(), DoodleSprites.PaintingBack(), DoodleSprites.Safe(),
                DoodleSprites.SafeOpen(), DoodleSprites.KeypadKey(), DoodleSprites.BedsideLamp(), DoodleSprites.LaunchSite(),
                DoodleSprites.Rocket(), DoodleSprites.SafetyCover(),
                DoodleSprites.RobotOff(), DoodleSprites.RobotOn(), DoodleSprites.BatteryFrame(), DoodleSprites.Bolt(),
                DoodleSprites.SoupBowl(), DoodleSprites.Thermometer(), DoodleSprites.IceBlock(), DoodleSprites.CampSite(),
                DoodleSprites.CampLogs(), DoodleSprites.FireStick(), DoodleSprites.ButtonFace(), DoodleSprites.ButtonLegs(),
                DoodleSprites.Drop(), DoodleSprites.Pot(), DoodleSprites.PlantDry(), DoodleSprites.PlantBloom(),
                DoodleSprites.BoxCup(), DoodleSprites.StickyNote(),
                DoodleSprites.MouthO(), DoodleSprites.MouthSmile(), DoodleSprites.Sun(), DoodleSprites.Cloud(),
                DoodleSprites.Pizza(), DoodleSprites.PizzaSlice(), DoodleSprites.SnowScarf(), DoodleSprites.SnowBodyFull(), DoodleSprites.RobotSad(), DoodleSprites.CrankWheel(), DoodleSprites.CrankArm(), DoodleSprites.Slingshot(), DoodleSprites.StillLifeWithKey(), DoodleSprites.StillLifeEmpty(), DoodleSprites.AlarmClock(), DoodleSprites.RingWaves(), DoodleSprites.RobotMecha(), DoodleSprites.MouseSkeleton(), DoodleSprites.MouseTail(), DoodleSprites.Moon(), DoodleSprites.SnowForeground(),
                DoodleSprites.Chalkboard(), DoodleSprites.PizzaOven(), DoodleSprites.Dough(false), DoodleSprites.Dough(true),
                DoodleSprites.Sauce(false), DoodleSprites.Sauce(true), DoodleSprites.Cheese(false), DoodleSprites.Cheese(true),
                DoodleSprites.Topping("mushroom"), DoodleSprites.Topping("olive"), DoodleSprites.Topping("chili"), DoodleSprites.Topping("pineapple"),
                DoodleSprites.IngredientBowl(), DoodleSprites.Tongue(), DoodleSprites.Balloon(), DoodleSprites.BalloonOutline(),
                DoodleSprites.Pump(), DoodleSprites.PumpHandle(), DoodleSprites.BalloonScrap(), DoodleSprites.CatBigFace(),
                DoodleSprites.CatPupil(), DoodleSprites.MouseHole(), DoodleSprites.Pan(), DoodleSprites.PancakeCooked(), DoodleSprites.PancakeRaw(),
                DoodleSprites.WaspMazeNeon(), DoodleSprites.AppleTree(), DoodleSprites.Basket(), DoodleSprites.Cup(), DoodleSprites.Pad(),
                DoodleSprites.Roach(), DoodleSprites.RoachLegs(0), DoodleSprites.RoachLegs(1), DoodleSprites.RoachDead(), DoodleSprites.SprayCan(), DoodleSprites.CookieBroken(), DoodleSprites.BurstRays(), DoodleSprites.ShockRing(), DoodleSprites.ArmorPiece(0), DoodleSprites.ArmorPiece(1), DoodleSprites.ArmorPiece(2), DoodleSprites.ArmorPiece(3), DoodleSprites.ChalkIcon("tomato"), DoodleSprites.ChalkIcon("mozzarella"), DoodleSprites.ChalkIcon("mushroom"), DoodleSprites.ChalkIcon("olive"), DoodleSprites.ChalkIcon("pineapple"), DoodleSprites.Bird(0), DoodleSprites.Bird(1), DoodleSprites.Bird(2), DoodleSprites.Windsock(), DoodleSprites.Pouch(), DoodleSprites.Umbrella(false), DoodleSprites.Umbrella(true), DoodleSprites.MorseKey5(0), DoodleSprites.MorseKey5(1), DoodleSprites.MorseKey5(2), DoodleSprites.MorseKey5(3), DoodleSprites.MorseKey5(4), DoodleSprites.CanyonBackground(), DoodleSprites.CheeseWedge(), DoodleSprites.Scroll(), DoodleSprites.MagicCircle(), DoodleSprites.Chest(false), DoodleSprites.Chest(true), DoodleSprites.HallBackground(), DoodleSprites.ShapedKey(true), DoodleSprites.ShapedKey(false), DoodleSprites.FloorKeyCamo(), DoodleSprites.Doorway(), DoodleSprites.ColorDoor(true), DoodleSprites.ColorDoor(false), DoodleSprites.Lady(), DoodleSprites.Monster(), DoodleSprites.HallPainting(true), DoodleSprites.HallPainting(false), /*V24*/
                DoodleSprites.Bat(false), DoodleSprites.Bat(true), DoodleSprites.LightSwitch(false), DoodleSprites.LightSwitch(true), DoodleSprites.Bathroom(), DoodleSprites.Mirror(), DoodleSprites.MirrorFog(), DoodleSprites.Cabinet(false), DoodleSprites.Cabinet(true), DoodleSprites.Acrobat(), DoodleSprites.PaperPlane(), DoodleSprites.SkyBackground(), DoodleSprites.Hoop(), DoodleSprites.Hoop(true), /*V26b*/
                DoodleSprites.SnowBuddy(), DoodleSprites.SafeDial(), DoodleSprites.Domino(), DoodleSprites.Bell(), DoodleSprites.Pond(), DoodleSprites.Magnet(), DoodleSprites.TinCan(), DoodleSprites.Horseshoe(), DoodleSprites.Piranha(), DoodleSprites.FishingRod(), DoodleSprites.CuckooClock(), DoodleSprites.Firefly(), DoodleSprites.WolfEyes(), DoodleSprites.Wolf(), DoodleSprites.Bush(), DoodleSprites.Cherry(), DoodleSprites.SoapBubble(), DoodleSprites.NightLivingRoom(), DoodleSprites.GuardDog(), DoodleSprites.Sofa(), DoodleSprites.CardboardBox(), DoodleSprites.PlantTop(), DoodleSprites.MoleHole(false), DoodleSprites.MoleHole(true), DoodleSprites.Mole(0), DoodleSprites.Mole(1), DoodleSprites.Mole(2), DoodleSprites.Mallet(), /*V26*/
                DoodleSprites.LaserPen(), DoodleSprites.MirrorTile(), DoodleSprites.Vase(), DoodleSprites.PipeTile(0), DoodleSprites.PipeTile(1), DoodleSprites.PipeTile(2), DoodleSprites.Faucet(), DoodleSprites.Coin(), DoodleSprites.Sheep(), DoodleSprites.Card(false), DoodleSprites.Card(true), DoodleSprites.Paw(), DoodleSprites.BeatNote(false), DoodleSprites.BeatNote(true), DoodleSprites.BeatRing(), DoodleSprites.DiscoBackground(), DoodleSprites.TitleLogo(), DoodleSprites.CarouselTop(), DoodleSprites.Stopwatch(), DoodleSprites.PlayingCard(), DoodleSprites.WindowFrame(), DoodleSprites.Curtain(), DoodleSprites.TinyHat(true), DoodleSprites.TinyHat(false), DoodleSprites.DizzyEyes(), DoodleSprites.PugCushion(), DoodleSprites.Toni(false), DoodleSprites.Toni(true), DoodleSprites.PugTail(), DoodleSprites.PugEyes(false), DoodleSprites.PugEyes(true), DoodleSprites.PugPupil(), DoodleSprites.PugMouthOpen(), DoodleSprites.PugTongue(), DoodleSprites.Bone(), DoodleSprites.ToniBall(), DoodleSprites.Sausages(), DoodleSprites.Feather(), DoodleSprites.DogCake(), DoodleSprites.CreamSplat(), DoodleSprites.WallSection(), DoodleSprites.DogFlap(), DoodleSprites.Butter(), DoodleSprites.Sock(), DoodleSprites.GattaiKanji(), DoodleSprites.LightPillar(), DoodleSprites.LightStreak(), DoodleSprites.SparkleStar(), DoodleSprites.ToniFaceBig(), DoodleSprites.ToniTongueBig(), DoodleSprites.CreamBlob(), DoodleSprites.DreamCloud(), DoodleSprites.Slipper(), DoodleSprites.TeddyBear(), DoodleSprites.ParkBackground(), DoodleSprites.ParkPond(), DoodleSprites.YarnBall(), DoodleSprites.DogPawPrint(), /*V28*/
            };
            foreach (Func<IEnumerable<DoodleArt>> pack in new Func<IEnumerable<DoodleArt>>[] { DoodleSprites.ArtV39A, DoodleSprites.ArtV39B, DoodleSprites.ArtV39C, DoodleSprites.ArtV39D, DoodleSprites.ArtV39E,
                DoodleSprites.ArtV40A, DoodleSprites.ArtV40B, DoodleSprites.ArtV40C, DoodleSprites.ArtV40D, DoodleSprites.ArtV40E,
                DoodleSprites.ArtRw1, DoodleSprites.ArtRw2, DoodleSprites.ArtRw3,
                DoodleSprites.ArtV41A, DoodleSprites.ArtV41B, DoodleSprites.ArtV41C, DoodleSprites.ArtV41D, DoodleSprites.ArtV41E, DoodleSprites.ArtV41F, DoodleSprites.ArtV41G, DoodleSprites.ArtV41H, DoodleSprites.ArtV41I, DoodleSprites.ArtV41J, DoodleSprites.ArtV41K, DoodleSprites.ArtV41L, DoodleSprites.ArtHalloween, DoodleSprites.ArtPuzzle, DoodleSprites.ArtV62, DoodleSprites.ArtV63, DoodleSprites.ArtV64A, DoodleSprites.ArtV64B, DoodleSprites.ArtV64C, DoodleSprites.ArtV67A, DoodleSprites.ArtV67B, DoodleSprites.ArtV68A, DoodleSprites.ArtV68B, DoodleSprites.ArtV69A, DoodleSprites.ArtV69B, DoodleSprites.ArtV70A, DoodleSprites.ArtV70B, DoodleSprites.ArtTutorial })
                all.AddRange(pack());
            return all;
        }
    }
}
