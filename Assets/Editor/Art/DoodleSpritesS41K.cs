using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli della specifica 105-200, pacchetto K.</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV41K() => new DoodleArt[]
        {
            // comuni
            S41K_Button(), S41K_Dot(), S41K_Cloud(),
            // The two-finger pianist
            S41K_ConcertBg(), S41K_Piano(), S41K_PianoKey(), S41K_MusicStand(),
            // The moon thief
            S41K_NightBg(), S41K_Moon(), S41K_Thief(false), S41K_Thief(true), S41K_Net(),
            // Back to the future
            S41K_LabBg(), S41K_Scientist(), S41K_Calendar(), S41K_Page(), S41K_FutureDoor(false), S41K_FutureDoor(true), S41K_Sign(),
            // The three idiots
            S41K_FieldBg(), S41K_Rock(), S41K_Idiot(0), S41K_Idiot(1), S41K_Idiot(2), S41K_Whistle(),
            // The shy star
            S41K_SkyBg(), S41K_Star(false), S41K_Star(true), S41K_Glow(), S41K_Mirror(),
            // The broken internet
            S41K_DeskBg(), S41K_Computer(false), S41K_Computer(true), S41K_Router(), S41K_Cable(), S41K_Socket(),
            S41K_Plug(0), S41K_Plug(1), S41K_Plug(2), S41K_Plug(3), S41K_Spark(),
            // The laughing lock
            S41K_LockRoomBg(), S41K_WoodDoor(), S41K_Lock(0), S41K_Lock(1), S41K_Lock(2), S41K_Lock(3), S41K_Handle(), S41K_Crack(),
            // The fake boss
            S41K_ArenaBg(), S41K_Boss(), S41K_Puppet(), S41K_Hero(), S41K_HpBar(),
        };

        static float S41K_Sin(float v) => (float)Math.Sin(v);
        static float S41K_Cos(float v) => (float)Math.Cos(v);
        static float S41K_Clamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        /// <summary>Vertici di una stella a n punte.</summary>
        static float[] S41K_StarPts(float cx, float cy, float ro, float ri, int n, float rotDeg = 90f)
        {
            var pts = new float[n * 4];
            for (int i = 0; i < n * 2; i++)
            {
                float a = (rotDeg + i * 180f / n) * (float)Math.PI / 180f;
                float r = i % 2 == 0 ? ro : ri;
                pts[i * 2] = cx + r * S41K_Cos(a);
                pts[i * 2 + 1] = cy + r * S41K_Sin(a);
            }
            return pts;
        }

        static void S41K_Eye(DoodlePainter p, float x, float y, float r, float lookX = 0f, float lookY = 0f)
        {
            p.Shape(Sd.Circle(x, y, r), Rgba.White, r * 0.3f);
            p.Fill(Sd.Circle(x + lookX * r * 0.4f, y + lookY * r * 0.4f, r * 0.52f), Ink);
            p.Fill(Sd.Circle(x + lookX * r * 0.4f - r * 0.18f, y + lookY * r * 0.4f + r * 0.2f, r * 0.17f), Rgba.White);
        }

        // ================= COMUNI =================

        /// <summary>Pulsante a pillola chiaro (si colora con SpriteRenderer.color): 1.6 x 0.7, centro 0,0.</summary>
        public static DoodleArt S41K_Button()
        {
            var p = new DoodlePainter(-0.86f, -0.43f, 0.86f, 0.4f, 120) { Ink = 0.045f };
            p.Shape(Sd.Box(0, -0.06f, 0.8f, 0.3f, 0.3f), Rgba.Hex("9C97A8"));
            p.Shape(Sd.Box(0, 0.02f, 0.8f, 0.3f, 0.3f), Rgba.Hex("F4F2F8"));
            p.Fill(Sd.Box(-0.2f, 0.17f, 0.45f, 0.05f, 0.05f), Rgba.White.WithAlpha(0.9f));
            return new DoodleArt("s41k_button", p);
        }

        /// <summary>Pallino bianco (si colora): raggio 0.13.</summary>
        public static DoodleArt S41K_Dot()
        {
            var p = new DoodlePainter(-0.17f, -0.17f, 0.17f, 0.17f, 150) { Ink = 0.035f };
            p.Shape(Sd.Circle(0, 0, 0.13f), Rgba.White);
            return new DoodleArt("s41k_dot", p);
        }

        /// <summary>Nuvola soffice e opaca (centro 0,0, larga 2.3).</summary>
        public static DoodleArt S41K_Cloud()
        {
            var p = new DoodlePainter(-1.3f, -0.72f, 1.3f, 0.8f, 100) { Ink = 0.05f };
            Sdf c = Sd.Union(Sd.Circle(-0.7f, -0.1f, 0.48f), Sd.Circle(-0.15f, 0.2f, 0.58f), Sd.Circle(0.5f, 0.05f, 0.5f),
                Sd.Circle(0.85f, -0.22f, 0.36f), Sd.Box(0, -0.35f, 1.05f, 0.27f, 0.25f));
            p.Shape(c, Rgba.Hex("F3F1FF"));
            p.Fill(Sd.Intersect(c, Sd.Box(0, -0.52f, 1.2f, 0.12f)), Rgba.Hex("CFCBEA"));
            p.Fill(Sd.Circle(-0.3f, 0.42f, 0.14f), Rgba.White);
            return new DoodleArt("s41k_cloud", p);
        }

        // ================= THE TWO-FINGER PIANIST =================

        /// <summary>Palco con sipario rosso e faro (coordinate mondo).</summary>
        public static DoodleArt S41K_ConcertBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("3B2556"), Rgba.Hex("6A4590"), S41K_Clamp((y + 1f) / 6f)));
            // faro
            p.FillFunc(Sd.Polygon(-0.5f, 5.3f, 0.5f, 5.3f, 2.9f, -1f, -2.9f, -1f), (x, y) => Rgba.Hex("FFF3C4", 0.18f * S41K_Clamp((y + 1f) / 6f)));
            // pavimento del palco
            p.FillFunc(Sd.Box(0, -3.1f, 3.3f, 2.3f), (x, y) => Rgba.Lerp(Rgba.Hex("A8653A"), Rgba.Hex("C98450"), 0.5f + 0.5f * S41K_Sin(x * 2.1f)));
            for (int i = -6; i <= 6; i++) p.Line(Rgba.Hex("8A4E2A"), 0.03f, i * 0.5f, -0.8f, i * 0.75f, -5.3f);
            p.Line(Ink, 0.05f, -3.3f, -0.8f, 3.3f, -0.8f);
            // sipari laterali con pieghe
            foreach (int s in new[] { -1, 1 })
            {
                Sdf cur = Sd.Polygon(s * 3.3f, 5.3f, s * 2.2f, 5.3f, s * 2.55f, 1.5f, s * 2.45f, -0.6f, s * 3.3f, -0.6f);
                p.Shape(cur, Rgba.Hex("D4384A"));
                for (int k = 0; k < 3; k++)
                {
                    float x = s * (2.5f + k * 0.25f);
                    p.Line(Rgba.Hex("A51F33"), 0.06f, x, 5.2f, x + s * 0.1f, -0.5f);
                }
            }
            // mantovana smerlata in alto
            Sdf val = Sd.Union(Sd.Box(0, 4.85f, 3.3f, 0.45f), Sd.Circle(-2.4f, 4.4f, 0.42f), Sd.Circle(-1.2f, 4.4f, 0.42f),
                Sd.Circle(0f, 4.4f, 0.42f), Sd.Circle(1.2f, 4.4f, 0.42f), Sd.Circle(2.4f, 4.4f, 0.42f));
            p.Shape(val, Rgba.Hex("E04A5A"));
            for (int k = -2; k <= 2; k++) p.Fill(Sd.Circle(k * 1.2f, 4.15f, 0.08f), Rgba.Hex("FFD15A"));
            return new DoodleArt("s41k_concert_bg", p);
        }

        /// <summary>Pianoforte nero laccato (coordinate mondo): tasti da y -3.8 a -1.2.</summary>
        public static DoodleArt S41K_Piano()
        {
            var p = new DoodlePainter(-2.95f, -4.75f, 2.95f, 0.75f, 60) { Ink = 0.055f };
            Rgba black = Rgba.Hex("2E2633"), shine = Rgba.Hex("5A4E66");
            // gambe
            p.Shape(Sd.Box(-2.45f, -4.5f, 0.16f, 0.25f, 0.04f), black);
            p.Shape(Sd.Box(2.45f, -4.5f, 0.16f, 0.25f, 0.04f), black);
            // cassa
            p.Shape(Sd.Box(0, -1.85f, 2.82f, 2.55f, 0.12f), black);
            p.Shape(Sd.Box(0, 0.38f, 2.9f, 0.22f, 0.1f), black);
            p.Fill(Sd.Box(-1.2f, 0.45f, 1.2f, 0.04f, 0.04f), shine);
            // pannello frontale con filetto dorato
            p.Shape(Sd.Box(0, -0.42f, 2.6f, 0.38f, 0.06f), Rgba.Hex("3B3142"), 0.035f);
            p.Line(Rgba.Hex("E8B93A"), 0.035f, -2.45f, -0.42f, 2.45f, -0.42f);
            // vano dei tasti
            p.Fill(Sd.Box(0, -2.5f, 2.6f, 1.42f, 0.05f), Rgba.Hex("17121B"));
            p.Fill(Sd.Box(-2.0f, -0.95f, 0.5f, 0.05f, 0.04f), shine);
            return new DoodleArt("s41k_piano", p);
        }

        /// <summary>Tasto bianco 0.64 x 2.56 (centro 0,0).</summary>
        public static DoodleArt S41K_PianoKey()
        {
            var p = new DoodlePainter(-0.36f, -1.36f, 0.36f, 1.36f, 100) { Ink = 0.04f };
            Sdf k = Sd.Box(0, 0, 0.31f, 1.28f, 0.08f);
            p.Shape(k, Rgba.Hex("FBF8F2"));
            p.Fill(Sd.Intersect(k, Sd.Box(0, -1.2f, 0.4f, 0.12f)), Rgba.Hex("DCD5C8"));
            p.Fill(Sd.Box(-0.17f, 0.3f, 0.04f, 0.8f, 0.04f), Rgba.White);
            return new DoodleArt("s41k_key", p);
        }

        /// <summary>Leggio con lo spartito (centro 0,0, 2.6 x 1.3).</summary>
        public static DoodleArt S41K_MusicStand()
        {
            var p = new DoodlePainter(-1.45f, -0.8f, 1.45f, 0.8f, 100) { Ink = 0.045f };
            p.Shape(Sd.Box(0, 0, 1.35f, 0.7f, 0.08f), Rgba.Hex("B9773F"));
            p.Shape(Sd.Box(0, 0.02f, 1.2f, 0.56f, 0.04f), Rgba.Hex("FFFBEA"), 0.035f);
            for (int i = 0; i < 5; i++)
            {
                float y = -0.36f + i * 0.18f;
                p.Line(Rgba.Hex("D8CFB4"), 0.02f, -1.1f, y, 1.1f, y);
            }
            // chiave di violino stilizzata
            p.Line(Rgba.Hex("B8AD90"), 0.04f, -0.95f, -0.45f, -0.95f, 0.45f, -0.82f, 0.38f, -0.88f, 0.15f, -1.02f, 0.0f, -0.95f, -0.15f, -0.85f, -0.05f);
            return new DoodleArt("s41k_stand", p);
        }

        // ================= THE MOON THIEF =================

        /// <summary>Cielo notturno con tetti (coordinate mondo): tetto piatto del ladro con il bordo a y -2.3.</summary>
        public static DoodleArt S41K_NightBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("3A3F8A"), Rgba.Hex("161C44"), S41K_Clamp((y + 2f) / 7f)));
            for (int i = 0; i < 40; i++)
            {
                float x = -3.1f + (i * 1.731f) % 6.2f, y = -1.5f + (i * 2.377f) % 6.5f;
                float r = 0.025f + (i % 4) * 0.012f;
                p.Fill(Sd.Circle(x, y, r), Rgba.Hex("FFF6C8", 0.85f));
            }
            // case lontane
            for (int i = 0; i < 6; i++)
            {
                float x = -3f + i * 1.15f, h = -1.9f + (i % 3) * 0.35f;
                p.Fill(Sd.Box(x, h - 2f, 0.5f, 2f), Rgba.Hex("2A2550"));
                p.Fill(Sd.Box(x - 0.15f, h - 0.5f, 0.08f, 0.1f), Rgba.Hex("FFD87A", 0.8f));
            }
            // tetto piatto del ladro (sinistra) e casa a destra col camino
            p.Shape(Sd.Box(-1.2f, -3.8f, 1.6f, 1.5f), Rgba.Hex("6B5A7E"));
            p.Shape(Sd.Box(-1.2f, -2.4f, 1.75f, 0.12f, 0.04f), Rgba.Hex("8A7A9C"));
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    p.Fill(Sd.Box(-2.4f + c * 0.42f + (r % 2) * 0.2f, -2.75f - r * 0.32f, 0.17f, 0.11f), Rgba.Hex("5D4D70"));
            p.Shape(Sd.Box(-0.6f, -3.6f, 0.3f, 0.35f, 0.04f), Rgba.Hex("FFD87A"));
            p.Line(Ink, 0.04f, -0.6f, -3.95f, -0.6f, -3.25f);
            p.Shape(Sd.Polygon(0.8f, -2.2f, 2.0f, -1.1f, 3.3f, -2.2f, 3.3f, -5.3f, 0.8f, -5.3f), Rgba.Hex("4D4266"));
            p.Shape(Sd.Box(2.5f, -1.15f, 0.2f, 0.4f), Rgba.Hex("9B5A4A"));
            p.Shape(Sd.Box(1.6f, -3.4f, 0.3f, 0.35f, 0.04f), Rgba.Hex("FFD87A"));
            p.Line(Ink, 0.04f, 1.3f, -3.4f, 1.9f, -3.4f);
            return new DoodleArt("s41k_night_bg", p);
        }

        /// <summary>Luna piena con faccina serena (centro 0,0, raggio 0.75).</summary>
        public static DoodleArt S41K_Moon()
        {
            var p = new DoodlePainter(-0.85f, -0.85f, 0.85f, 0.85f, 120) { Ink = 0.045f };
            p.Fill(Sd.Circle(0, 0, 0.84f), Rgba.Hex("FFF6C8", 0.25f));
            Sdf m = Sd.Circle(0, 0, 0.72f);
            p.Shape(m, Rgba.Hex("FFF1B0"));
            p.Fill(Sd.Crescent(m, -0.12f, 0.1f), Rgba.Hex("F0D888"));
            p.Shape(Sd.Circle(-0.35f, 0.38f, 0.12f), Rgba.Hex("EAD68E"), 0.025f);
            p.Shape(Sd.Circle(0.4f, -0.4f, 0.09f), Rgba.Hex("EAD68E"), 0.025f);
            p.Shape(Sd.Circle(0.42f, 0.32f, 0.07f), Rgba.Hex("EAD68E"), 0.025f);
            p.Line(Ink, 0.04f, -0.32f, 0.05f, -0.22f, 0.12f, -0.12f, 0.05f);
            p.Line(Ink, 0.04f, 0.12f, 0.05f, 0.22f, 0.12f, 0.32f, 0.05f);
            p.Line(Ink, 0.04f, -0.15f, -0.22f, 0f, -0.3f, 0.15f, -0.22f);
            p.Fill(Sd.Ellipse(-0.42f, -0.1f, 0.1f, 0.06f), Rgba.Hex("FF9BB0", 0.7f));
            p.Fill(Sd.Ellipse(0.42f, -0.1f, 0.1f, 0.06f), Rgba.Hex("FF9BB0", 0.7f));
            return new DoodleArt("s41k_moon", p);
        }

        /// <summary>Ladro con mascherina, berretto e sacco (piedi in y 0; la mano col retino in 0.42, 0.85).</summary>
        public static DoodleArt S41K_Thief(bool sad)
        {
            var p = new DoodlePainter(-0.75f, -0.05f, 0.7f, 1.85f, 140) { Ink = 0.04f };
            Rgba skin = Rgba.Hex("F2C6A0"), dark = Rgba.Hex("3A3346");
            // sacco sulla schiena
            p.Shape(Sd.Circle(-0.42f, 0.9f, 0.3f), Rgba.Hex("C9A46A"));
            p.Fill(Sd.Box(-0.45f, 0.95f, 0.07f, 0.03f), Rgba.Hex("8A6A3A"));
            // gambe e scarpe
            p.Shape(Sd.Capsule(-0.12f, 0.55f, -0.16f, 0.12f, 0.08f), dark);
            p.Shape(Sd.Capsule(0.12f, 0.55f, 0.16f, 0.12f, 0.08f), dark);
            p.Shape(Sd.Ellipse(-0.2f, 0.07f, 0.14f, 0.07f), Rgba.Hex("1E1A24"));
            p.Shape(Sd.Ellipse(0.22f, 0.07f, 0.14f, 0.07f), Rgba.Hex("1E1A24"));
            // maglia a righe
            Sdf body = Sd.Box(0, 0.78f, 0.27f, 0.3f, 0.12f);
            p.Shape(body, Rgba.White);
            for (int i = 0; i < 4; i++) p.Fill(Sd.Intersect(body, Sd.Box(0, 0.58f + i * 0.14f, 0.4f, 0.04f)), dark);
            // braccia
            p.Shape(Sd.Capsule(-0.22f, 0.95f, -0.38f, 0.7f, 0.06f), Rgba.White);
            p.Shape(Sd.Capsule(0.22f, 0.95f, 0.42f, 0.85f, 0.06f), Rgba.White);
            p.Shape(Sd.Circle(0.43f, 0.85f, 0.07f), skin);
            // testa
            p.Shape(Sd.Circle(0, 1.33f, 0.27f), skin);
            p.Shape(Sd.Box(0, 1.37f, 0.27f, 0.07f, 0.05f), dark);
            if (sad)
            {
                p.Line(Rgba.White, 0.035f, -0.16f, 1.34f, -0.07f, 1.39f);
                p.Line(Rgba.White, 0.035f, 0.07f, 1.39f, 0.16f, 1.34f);
                p.Line(Ink, 0.035f, -0.08f, 1.16f, 0f, 1.2f, 0.08f, 1.16f);
                p.Fill(Sd.Ellipse(-0.17f, 1.22f, 0.03f, 0.06f), Rgba.Hex("7FC4FF"));
                p.Fill(Sd.Ellipse(0.18f, 1.2f, 0.03f, 0.06f), Rgba.Hex("7FC4FF"));
            }
            else
            {
                p.Fill(Sd.Circle(-0.1f, 1.37f, 0.04f), Rgba.White);
                p.Fill(Sd.Circle(0.12f, 1.37f, 0.04f), Rgba.White);
                p.Line(Ink, 0.035f, -0.1f, 1.2f, 0.02f, 1.16f, 0.12f, 1.22f);
            }
            // berretto
            p.Shape(Sd.Intersect(Sd.Circle(0, 1.45f, 0.29f), Sd.Box(0, 1.65f, 0.4f, 0.2f)), Rgba.Hex("4B4060"));
            p.Shape(Sd.Circle(0, 1.76f, 0.07f), Rgba.Hex("E8443A"), 0.03f);
            return new DoodleArt(sad ? "s41k_thief_sad" : "s41k_thief", p);
        }

        /// <summary>Retino col manico lungo (impugnatura in 0,0, cerchio in alto a destra).</summary>
        public static DoodleArt S41K_Net()
        {
            var p = new DoodlePainter(-0.15f, -0.15f, 1.05f, 2.15f, 120) { Ink = 0.035f };
            p.Shape(Sd.Capsule(0, 0, 0.5f, 1.55f, 0.04f), Rgba.Hex("B9773F"));
            Sdf ring = Sd.Circle(0.65f, 1.82f, 0.3f);
            p.Fill(ring, Rgba.Hex("FFFFFF", 0.25f));
            p.Hatch(ring, Rgba.Hex("FFFFFF", 0.6f), 0.1f, 0.015f, 45f);
            p.Hatch(ring, Rgba.Hex("FFFFFF", 0.6f), 0.1f, 0.015f, -45f);
            p.Shape(Sd.Subtract(Sd.Circle(0.65f, 1.82f, 0.32f), Sd.Circle(0.65f, 1.82f, 0.27f)), Rgba.Hex("8A8F9C"), 0.025f);
            return new DoodleArt("s41k_net", p);
        }

        // ================= BACK TO THE FUTURE =================

        /// <summary>Laboratorio (coordinate mondo): pavimento a y -3.4.</summary>
        public static DoodleArt S41K_LabBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 1f, 3.3f, 4.4f), (x, y) => Rgba.Lerp(Rgba.Hex("7FC8C2"), Rgba.Hex("A7DDD5"), S41K_Clamp((y + 3f) / 8f)));
            for (int i = -6; i <= 6; i++) p.Line(Rgba.Hex("6FB5AF"), 0.025f, i * 0.55f, -3.4f, i * 0.55f, 5.3f);
            for (int j = 0; j < 16; j++) p.Line(Rgba.Hex("6FB5AF"), 0.025f, -3.3f, -3.4f + j * 0.55f, 3.3f, -3.4f + j * 0.55f);
            // pavimento a scacchi
            p.Fill(Sd.Box(0, -4.3f, 3.3f, 0.9f), Rgba.Hex("E9E4F2"));
            for (int i = 0; i < 12; i++)
                for (int j = 0; j < 4; j++)
                    if ((i + j) % 2 == 0) p.Fill(Sd.Box(-3.0f + i * 0.55f, -3.65f - j * 0.45f, 0.275f, 0.225f), Rgba.Hex("54476A"), false);
            p.Line(Ink, 0.05f, -3.3f, -3.4f, 3.3f, -3.4f);
            // mensola con le ampolle (in alto a sinistra)
            p.Shape(Sd.Box(-1.6f, 2.55f, 1.3f, 0.07f), Rgba.Hex("B9773F"));
            Rgba[] liq = { Rgba.Hex("FF6B9A"), Rgba.Hex("8CE05A"), Rgba.Hex("6BB8FF") };
            for (int i = 0; i < 3; i++)
            {
                float x = -2.4f + i * 0.75f;
                p.Shape(Sd.Union(Sd.Circle(x, 2.85f, 0.2f), Sd.Box(x, 3.15f, 0.06f, 0.15f)), Rgba.Hex("E8F6FF", 0.9f), 0.035f);
                p.Fill(Sd.Intersect(Sd.Circle(x, 2.85f, 0.17f), Sd.Box(x, 2.75f, 0.2f, 0.12f)), liq[i]);
            }
            // cavi e lampadine
            p.Line(Rgba.Hex("3A3346"), 0.05f, 2.9f, 5.3f, 2.6f, 3.2f, 3.0f, 2.2f);
            p.Shape(Sd.Circle(3.0f, 2.05f, 0.14f), Rgba.Hex("FFE36A"), 0.035f);
            return new DoodleArt("s41k_lab_bg", p);
        }

        /// <summary>Scienziato pazzo col camice e i capelli dritti (piedi in y 0).</summary>
        public static DoodleArt S41K_Scientist()
        {
            var p = new DoodlePainter(-0.75f, -0.05f, 0.75f, 2.15f, 130) { Ink = 0.045f };
            Rgba skin = Rgba.Hex("F5CDA8");
            p.Shape(Sd.Capsule(-0.14f, 0.6f, -0.16f, 0.12f, 0.08f), Rgba.Hex("4B4060"));
            p.Shape(Sd.Capsule(0.14f, 0.6f, 0.16f, 0.12f, 0.08f), Rgba.Hex("4B4060"));
            p.Shape(Sd.Ellipse(-0.2f, 0.07f, 0.15f, 0.07f), Rgba.Hex("3A3346"));
            p.Shape(Sd.Ellipse(0.22f, 0.07f, 0.15f, 0.07f), Rgba.Hex("3A3346"));
            // camice
            p.Shape(Sd.Polygon(-0.38f, 0.45f, 0.38f, 0.45f, 0.3f, 1.25f, -0.3f, 1.25f), Rgba.White);
            p.Line(Ink, 0.03f, 0f, 0.5f, 0f, 1.2f);
            p.Shape(Sd.Box(0.18f, 0.95f, 0.08f, 0.06f, 0.02f), Rgba.Hex("BFE6FF"), 0.025f);
            p.Shape(Sd.Capsule(-0.3f, 1.15f, -0.5f, 0.75f, 0.07f), Rgba.White);
            p.Shape(Sd.Capsule(0.3f, 1.15f, 0.55f, 1.45f, 0.07f), Rgba.White);
            p.Shape(Sd.Circle(0.57f, 1.5f, 0.08f), skin);
            p.Shape(Sd.Circle(-0.52f, 0.72f, 0.08f), skin);
            // testa e capelli
            Sdf hair = Sd.Union(Sd.Circle(-0.28f, 1.75f, 0.17f), Sd.Circle(0.28f, 1.75f, 0.17f), Sd.Circle(0f, 1.9f, 0.2f),
                Sd.Circle(-0.18f, 1.95f, 0.13f), Sd.Circle(0.2f, 1.95f, 0.13f));
            p.Shape(hair, Rgba.Hex("F2F2F6"));
            p.Shape(Sd.Circle(0, 1.55f, 0.27f), skin);
            // occhialoni
            p.Shape(Sd.Circle(-0.11f, 1.6f, 0.1f), Rgba.Hex("CFF1FF"), 0.035f);
            p.Shape(Sd.Circle(0.11f, 1.6f, 0.1f), Rgba.Hex("CFF1FF"), 0.035f);
            p.Fill(Sd.Circle(-0.09f, 1.6f, 0.04f), Ink);
            p.Fill(Sd.Circle(0.13f, 1.6f, 0.04f), Ink);
            p.Line(Ink, 0.035f, -0.1f, 1.42f, 0f, 1.37f, 0.1f, 1.42f);
            return new DoodleArt("s41k_scientist", p);
        }

        /// <summary>Calendario-macchina del tempo (centro 0,0): la pagina va in 0,-0.15 (1.7 x 1.8).</summary>
        public static DoodleArt S41K_Calendar()
        {
            var p = new DoodlePainter(-1.3f, -1.4f, 1.3f, 1.8f, 100) { Ink = 0.05f };
            // antenna
            p.Line(Ink, 0.05f, 0.6f, 1.2f, 0.85f, 1.6f);
            p.Shape(Sd.Circle(0.87f, 1.63f, 0.1f), Rgba.Hex("FF5A5A"), 0.03f);
            p.Shape(Sd.Box(0, 0.05f, 1.2f, 1.3f, 0.15f), Rgba.Hex("9AA3B5"));
            p.Fill(Sd.Box(0, 1.2f, 1.1f, 0.08f, 0.05f), Rgba.Hex("BFC7D6"));
            // bulloni
            foreach (float x in new[] { -1.05f, 1.05f })
                foreach (float y in new[] { -1.1f, 1.2f })
                    p.Shape(Sd.Circle(x, y, 0.05f), Rgba.Hex("D8DEE8"), 0.02f);
            // sede della pagina
            p.Fill(Sd.Box(0, -0.15f, 0.9f, 0.95f, 0.05f), Rgba.Hex("5A6276"));
            // anelli
            for (int i = -2; i <= 2; i++) p.Shape(Sd.Box(i * 0.32f, 0.82f, 0.05f, 0.12f, 0.04f), Rgba.Hex("E8EBF2"), 0.025f);
            return new DoodleArt("s41k_calendar", p);
        }

        /// <summary>Pagina del calendario (centro 0,0, 1.7 x 1.8) con testata rossa.</summary>
        public static DoodleArt S41K_Page()
        {
            var p = new DoodlePainter(-0.9f, -0.95f, 0.9f, 0.95f, 100) { Ink = 0.04f };
            Sdf pg = Sd.Box(0, 0, 0.84f, 0.88f, 0.05f);
            p.Shape(pg, Rgba.Hex("FFFDF6"));
            p.Fill(Sd.Intersect(pg, Sd.Box(0, 0.72f, 0.9f, 0.18f)), Rgba.Hex("E8443A"));
            p.Fill(Sd.Intersect(pg, Sd.Box(0, -0.84f, 0.9f, 0.06f)), Rgba.Hex("E7E0CC"));
            return new DoodleArt("s41k_page", p);
        }

        /// <summary>Porta del futuro (base in y 0, 1.4 x 2.6): piena o solo un contorno tratteggiato.</summary>
        public static DoodleArt S41K_FutureDoor(bool ghost)
        {
            var p = new DoodlePainter(-0.9f, -0.05f, 0.9f, 2.8f, 100) { Ink = 0.05f };
            Sdf frame = Sd.Box(0, 1.35f, 0.8f, 1.35f, 0.25f);
            if (ghost)
            {
                Sdf ring = Sd.Subtract(frame, Sd.Grow(frame, -0.07f));
                p.FillFunc(ring, (x, y) => S41K_Sin((x + y) * 18f) > 0f ? Rgba.Hex("BFF3FF", 0.95f) : Rgba.Clear);
                p.Fill(Sd.Grow(frame, -0.07f), Rgba.Hex("BFF3FF", 0.12f));
                p.FillFunc(Sd.Circle(0, 1.35f, 0.3f), (x, y) => Rgba.Hex("BFF3FF", 0.5f + 0.4f * S41K_Sin(y * 30f)));
                return new DoodleArt("s41k_door_ghost", p);
            }
            p.Shape(frame, Rgba.Hex("7A88A8"));
            p.Shape(Sd.Box(0, 1.3f, 0.62f, 1.18f, 0.18f), Rgba.Hex("5ED0E8"));
            p.Fill(Sd.Box(-0.25f, 1.6f, 0.06f, 0.75f, 0.05f), Rgba.Hex("A8F0FF"));
            p.Line(Rgba.Hex("2B8FA8"), 0.04f, 0f, 0.2f, 0f, 2.4f);
            p.Shape(Sd.Circle(0.38f, 1.25f, 0.08f), Rgba.Hex("FFD15A"), 0.03f);
            for (int i = 0; i < 3; i++) p.Fill(Sd.Circle(-0.3f + i * 0.3f, 2.55f, 0.05f), i == 1 ? Rgba.Hex("8CE05A") : Rgba.Hex("FF6B6B"));
            return new DoodleArt("s41k_door", p);
        }

        /// <summary>Cartello sul palo (centro del cartello 0,0, 1.9 x 1.0).</summary>
        public static DoodleArt S41K_Sign()
        {
            var p = new DoodlePainter(-1.05f, -1.2f, 1.05f, 0.6f, 100) { Ink = 0.045f };
            p.Shape(Sd.Box(0, -0.85f, 0.07f, 0.35f), Rgba.Hex("8A5A3B"));
            p.Shape(Sd.Box(0, 0, 0.95f, 0.5f, 0.08f), Rgba.Hex("FFE9A8"));
            p.Fill(Sd.Box(0, 0.42f, 0.85f, 0.03f), Rgba.Hex("E8C870"));
            return new DoodleArt("s41k_sign", p);
        }

        // ================= THE THREE IDIOTS =================

        /// <summary>Prato visto dall'alto con sentiero e buca (coordinate mondo): buca in 2.05, -0.3.</summary>
        public static DoodleArt S41K_FieldBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("7CC35A"), Rgba.Hex("93D66E"), 0.5f + 0.5f * S41K_Sin(x * 1.3f + y * 0.9f)));
            for (int i = 0; i < 60; i++)
            {
                float x = -3.1f + (i * 1.371f) % 6.2f, y = -5f + (i * 2.917f) % 10f;
                p.Line(Rgba.Hex("5FA846"), 0.025f, x, y, x + 0.05f, y + 0.12f);
            }
            // sentiero
            p.FillFunc(Sd.Box(0, -0.3f, 3.3f, 1.55f, 0.4f), (x, y) => Rgba.Lerp(Rgba.Hex("D9B27C"), Rgba.Hex("E6C291"), 0.5f + 0.5f * S41K_Sin(x * 4f + y * 3f)));
            p.Line(Rgba.Hex("B8915C"), 0.05f, -3.3f, 1.25f, 3.3f, 1.25f);
            p.Line(Rgba.Hex("B8915C"), 0.05f, -3.3f, -1.85f, 3.3f, -1.85f);
            for (int i = 0; i < 14; i++) p.Fill(Sd.Circle(-3f + (i * 0.83f) % 6f, -1.5f + (i * 0.53f) % 2.5f, 0.05f), Rgba.Hex("B8915C"));
            // buca
            p.Shape(Sd.Circle(2.05f, -0.3f, 0.85f), Rgba.Hex("8A6A45"));
            p.FillFunc(Sd.Circle(2.05f, -0.3f, 0.68f), (x, y) => Rgba.Lerp(Rgba.Hex("1E1712"), Rgba.Hex("4A3826"), S41K_Clamp(((x - 2.05f) * (x - 2.05f) + (y + 0.3f) * (y + 0.3f)) / 0.46f)));
            // cespugli e fiori
            foreach (var b in new[] { new[] { -2.3f, 2.6f }, new[] { 1.9f, 2.3f }, new[] { -1.8f, -3.4f }, new[] { 2.2f, -3.2f } })
                p.Shape(Sd.Union(Sd.Circle(b[0], b[1], 0.42f), Sd.Circle(b[0] + 0.35f, b[1] + 0.15f, 0.32f), Sd.Circle(b[0] - 0.3f, b[1] + 0.2f, 0.3f)), Rgba.Hex("4E9E3E"));
            for (int i = 0; i < 9; i++)
            {
                float x = -2.6f + (i * 1.91f) % 5.4f, y = i % 2 == 0 ? 2.0f + (i % 3) * 0.3f : -2.4f - (i % 3) * 0.4f;
                p.Shape(Sd.Circle(x, y, 0.08f), i % 2 == 0 ? Rgba.Hex("FFF3A8") : Rgba.Hex("FFB3C7"), 0.025f);
            }
            return new DoodleArt("s41k_field_bg", p);
        }

        /// <summary>Masso visto dall'alto (centro 0,0, raggio ~0.85).</summary>
        public static DoodleArt S41K_Rock()
        {
            var p = new DoodlePainter(-1f, -1f, 1f, 1f, 100) { Ink = 0.05f };
            Sdf r = Sd.Polygon(-0.8f, 0.1f, -0.55f, 0.65f, 0.05f, 0.85f, 0.6f, 0.6f, 0.85f, 0.05f, 0.65f, -0.6f, 0.05f, -0.85f, -0.6f, -0.62f);
            p.Shape(Sd.Grow(r, 0.04f), Rgba.Hex("A8A2B4"));
            p.Fill(Sd.Crescent(r, -0.15f, 0.15f), Rgba.Hex("8A8498"));
            p.Fill(Sd.Ellipse(-0.25f, 0.35f, 0.22f, 0.12f), Rgba.Hex("C9C4D4"));
            p.Line(Ink, 0.035f, 0.1f, 0.2f, 0.3f, -0.05f, 0.25f, -0.35f);
            p.Line(Ink, 0.03f, -0.45f, -0.2f, -0.25f, -0.35f);
            return new DoodleArt("s41k_rock", p);
        }

        /// <summary>Omino visto dall'alto che guarda verso +x con le braccia tese (centro 0,0).</summary>
        public static DoodleArt S41K_Idiot(int v)
        {
            var p = new DoodlePainter(-0.5f, -0.55f, 0.65f, 0.55f, 130) { Ink = 0.04f };
            Rgba shirt = v == 0 ? Rgba.Hex("FF6B6B") : v == 1 ? Rgba.Hex("5FA8FF") : Rgba.Hex("FFC93C");
            Rgba skin = Rgba.Hex("F5CDA8");
            // braccia tese in avanti
            p.Shape(Sd.Capsule(0f, 0.3f, 0.45f, 0.25f, 0.07f), shirt);
            p.Shape(Sd.Capsule(0f, -0.3f, 0.45f, -0.25f, 0.07f), shirt);
            p.Shape(Sd.Circle(0.5f, 0.25f, 0.08f), skin);
            p.Shape(Sd.Circle(0.5f, -0.25f, 0.08f), skin);
            // spalle
            p.Shape(Sd.Ellipse(-0.08f, 0, 0.22f, 0.4f), shirt);
            // testa con occhi rivolti in avanti
            p.Shape(Sd.Circle(0.02f, 0, 0.24f), skin);
            p.Fill(Sd.Intersect(Sd.Circle(0.02f, 0, 0.24f), Sd.Box(-0.2f, 0, 0.12f, 0.3f)), Rgba.Hex("6A4630"));
            p.Shape(Sd.Circle(0.14f, 0.09f, 0.06f), Rgba.White, 0.02f);
            p.Shape(Sd.Circle(0.14f, -0.09f, 0.06f), Rgba.White, 0.02f);
            p.Fill(Sd.Circle(0.17f, 0.09f, 0.03f), Ink);
            p.Fill(Sd.Circle(0.17f, -0.09f, 0.03f), Ink);
            p.Shape(Sd.Ellipse(0.27f, 0, 0.04f, 0.05f), Rgba.Hex("F0A884"), 0.02f);
            return new DoodleArt("s41k_idiot_" + v, p);
        }

        /// <summary>Fischietto su un bottone giallo rotondo (centro 0,0, raggio 0.6).</summary>
        public static DoodleArt S41K_Whistle()
        {
            var p = new DoodlePainter(-0.7f, -0.7f, 0.7f, 0.7f, 110) { Ink = 0.045f };
            p.Shape(Sd.Circle(0, -0.04f, 0.62f), Rgba.Hex("D69A1E"));
            p.Shape(Sd.Circle(0, 0.02f, 0.62f), Rgba.Hex("FFD15A"));
            p.Shape(Sd.Union(Sd.Circle(-0.08f, -0.05f, 0.25f), Sd.Box(0.25f, 0.08f, 0.25f, 0.1f, 0.03f)), Rgba.Hex("C9CED8"));
            p.Fill(Sd.Circle(-0.08f, -0.05f, 0.09f), Ink);
            p.Fill(Sd.Box(0.1f, 0.15f, 0.06f, 0.03f), Ink);
            p.Line(Ink, 0.03f, -0.3f, 0.12f, -0.35f, 0.38f, -0.15f, 0.42f);
            p.Line(Ink, 0.03f, 0.5f, 0.3f, 0.55f, 0.42f);
            p.Line(Ink, 0.03f, 0.45f, 0.4f, 0.42f, 0.52f);
            return new DoodleArt("s41k_whistle", p);
        }

        // ================= THE SHY STAR =================

        /// <summary>Cielo al tramonto con colline (coordinate mondo).</summary>
        public static DoodleArt S41K_SkyBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) =>
            {
                float t = S41K_Clamp((y + 4f) / 9f);
                return t < 0.35f ? Rgba.Lerp(Rgba.Hex("F29BB0"), Rgba.Hex("8C6BC0"), t / 0.35f) : Rgba.Lerp(Rgba.Hex("8C6BC0"), Rgba.Hex("232A5C"), (t - 0.35f) / 0.65f);
            });
            for (int i = 0; i < 30; i++)
            {
                float x = -3.1f + (i * 1.913f) % 6.2f, y = 0.5f + (i * 1.377f) % 4.7f;
                p.Fill(Sd.Circle(x, y, 0.02f + (i % 3) * 0.012f), Rgba.Hex("FFF6C8", 0.8f));
            }
            p.Shape(Sd.Union(Sd.Circle(-2.2f, -5.6f, 2.6f), Sd.Circle(1.9f, -6f, 3.2f)), Rgba.Hex("4A5A8C"));
            p.Shape(Sd.Box(0, -4.8f, 3.3f, 0.6f), Rgba.Hex("3E4C7A"));
            return new DoodleArt("s41k_sky_bg", p);
        }

        /// <summary>Stellina: timida e spenta, oppure felice e luminosa (centro 0,0, raggio 0.5).</summary>
        public static DoodleArt S41K_Star(bool happy)
        {
            var p = new DoodlePainter(-0.62f, -0.6f, 0.62f, 0.62f, 130) { Ink = 0.045f };
            Sdf st = Sd.Grow(Sd.Polygon(S41K_StarPts(0, 0, 0.48f, 0.22f, 5)), 0.04f);
            p.Shape(st, happy ? Rgba.Hex("FFD84A") : Rgba.Hex("E8DDB0"));
            p.Fill(Sd.Intersect(st, Sd.Circle(-0.12f, 0.15f, 0.16f)), Rgba.White.WithAlpha(happy ? 0.7f : 0.35f));
            p.Fill(Sd.Ellipse(-0.17f, -0.1f, 0.07f, 0.04f), Rgba.Hex("FF8FA8", 0.8f));
            p.Fill(Sd.Ellipse(0.17f, -0.1f, 0.07f, 0.04f), Rgba.Hex("FF8FA8", 0.8f));
            if (happy)
            {
                S41K_Eye(p, -0.1f, 0.03f, 0.06f, 0f, 0.3f);
                S41K_Eye(p, 0.1f, 0.03f, 0.06f, 0f, 0.3f);
                p.Shape(Sd.Intersect(Sd.Circle(0, -0.1f, 0.1f), Sd.Box(0, -0.18f, 0.12f, 0.08f)), Rgba.Hex("D9485F"), 0.025f);
            }
            else
            {
                // occhi chiusi rivolti di lato e boccuccia timida
                p.Line(Ink, 0.03f, -0.16f, 0.05f, -0.1f, 0.02f, -0.04f, 0.05f);
                p.Line(Ink, 0.03f, 0.04f, 0.05f, 0.1f, 0.02f, 0.16f, 0.05f);
                p.Line(Ink, 0.025f, -0.04f, -0.14f, 0.0f, -0.12f, 0.04f, -0.14f);
            }
            return new DoodleArt(happy ? "s41k_star_happy" : "s41k_star", p);
        }

        /// <summary>Alone luminoso morbido (centro 0,0, raggio 1).</summary>
        public static DoodleArt S41K_Glow()
        {
            var p = new DoodlePainter(-1f, -1f, 1f, 1f, 60) { Wobble = 0f };
            p.FillFunc(Sd.Circle(0, 0, 0.98f), (x, y) =>
            {
                float d = (float)Math.Sqrt(x * x + y * y);
                float a = S41K_Clamp(1f - d);
                return Rgba.Hex("FFE680", a * a * 0.8f);
            });
            return new DoodleArt("s41k_glow", p);
        }

        /// <summary>Specchio da toeletta sul suo piedistallo (centro del vetro 0,0).</summary>
        public static DoodleArt S41K_Mirror()
        {
            var p = new DoodlePainter(-0.75f, -1.25f, 0.75f, 0.85f, 110) { Ink = 0.045f };
            Rgba gold = Rgba.Hex("F2B93B");
            p.Shape(Sd.Capsule(0, -0.6f, 0, -1.0f, 0.06f), gold);
            p.Shape(Sd.Ellipse(0, -1.08f, 0.45f, 0.1f), gold);
            p.Shape(Sd.Ellipse(0, 0, 0.62f, 0.78f), gold);
            p.FillFunc(Sd.Ellipse(0, 0, 0.5f, 0.66f), (x, y) => Rgba.Lerp(Rgba.Hex("BFE6FF"), Rgba.Hex("E8F7FF"), S41K_Clamp((y + 0.6f) / 1.2f)));
            p.Fill(Sd.Rotate(Sd.Box(-0.2f, 0.2f, 0.05f, 0.28f, 0.04f), -0.2f, 0.2f, -25f), Rgba.White.WithAlpha(0.85f));
            p.Fill(Sd.Rotate(Sd.Box(-0.02f, 0.25f, 0.025f, 0.15f, 0.02f), -0.02f, 0.25f, -25f), Rgba.White.WithAlpha(0.85f));
            p.Shape(Sd.Circle(0, 0.78f, 0.08f), Rgba.Hex("FF8FA8"), 0.03f);
            return new DoodleArt("s41k_mirror", p);
        }

        // ================= THE BROKEN INTERNET =================

        /// <summary>Parete e grande piano della scrivania (coordinate mondo): piano sotto y 0.2.</summary>
        public static DoodleArt S41K_DeskBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 2.8f, 3.3f, 2.6f), (x, y) => Rgba.Lerp(Rgba.Hex("FFE2B8"), Rgba.Hex("FFD39A"), S41K_Clamp((y - 0.2f) / 5f)));
            for (int i = -3; i <= 3; i++) p.Line(Rgba.Hex("F3C88E"), 0.06f, i * 1f, 0.2f, i * 1f, 5.3f);
            // finestra
            p.Shape(Sd.Box(1.6f, 3.4f, 0.9f, 0.7f, 0.06f), Rgba.Hex("9FD8FF"));
            p.Line(Ink, 0.05f, 1.6f, 2.7f, 1.6f, 4.1f);
            p.Line(Ink, 0.05f, 0.7f, 3.4f, 2.5f, 3.4f);
            p.Shape(Sd.Union(Sd.Circle(1.2f, 3.6f, 0.15f), Sd.Circle(1.4f, 3.65f, 0.2f)), Rgba.White, 0.03f);
            // scrivania
            p.FillFunc(Sd.Box(0, -2.5f, 3.3f, 2.7f), (x, y) => Rgba.Lerp(Rgba.Hex("C88A55"), Rgba.Hex("D99C66"), 0.5f + 0.5f * S41K_Sin(y * 5f + S41K_Sin(x * 2f))));
            for (int j = 0; j < 6; j++) p.Line(Rgba.Hex("B07440"), 0.03f, -3.3f, -0.4f - j * 0.75f, 3.3f, -0.5f - j * 0.75f);
            p.Line(Ink, 0.06f, -3.3f, 0.2f, 3.3f, 0.2f);
            p.Shape(Sd.Box(0, -4.85f, 3.4f, 0.4f), Rgba.Hex("A0663A"));
            return new DoodleArt("s41k_desk_bg", p);
        }

        /// <summary>Computerino a tubo catodico (centro dello schermo 0,0): schermo blu triste o verde felice.</summary>
        public static DoodleArt S41K_Computer(bool ok)
        {
            var p = new DoodlePainter(-1.2f, -1.25f, 1.2f, 0.95f, 100) { Ink = 0.05f };
            p.Shape(Sd.Box(0, -1.05f, 0.65f, 0.13f, 0.06f), Rgba.Hex("D8D2C2"));
            p.Shape(Sd.Box(0, -0.85f, 0.25f, 0.12f), Rgba.Hex("C8C2B2"));
            p.Shape(Sd.Box(0, 0, 1.1f, 0.85f, 0.15f), Rgba.Hex("EDE7D6"));
            p.FillFunc(Sd.Box(0, 0.05f, 0.88f, 0.62f, 0.12f), (x, y) => ok
                ? Rgba.Lerp(Rgba.Hex("3E9E5A"), Rgba.Hex("6CCB80"), S41K_Clamp((y + 0.6f) / 1.2f))
                : Rgba.Lerp(Rgba.Hex("2C3A6A"), Rgba.Hex("4A5C9A"), S41K_Clamp((y + 0.6f) / 1.2f)));
            p.Fill(Sd.Rotate(Sd.Box(-0.55f, 0.4f, 0.05f, 0.18f), -0.55f, 0.4f, -30f), Rgba.White.WithAlpha(0.35f));
            p.Shape(Sd.Circle(0.85f, -0.7f, 0.05f), ok ? Rgba.Hex("8CE05A") : Rgba.Hex("FF6B6B"), 0.02f);
            return new DoodleArt(ok ? "s41k_computer_ok" : "s41k_computer", p);
        }

        /// <summary>Router con antenne e lucine (centro 0,0).</summary>
        public static DoodleArt S41K_Router()
        {
            var p = new DoodlePainter(-0.8f, -0.45f, 0.8f, 1.0f, 110) { Ink = 0.045f };
            p.Shape(Sd.Capsule(-0.45f, 0.1f, -0.6f, 0.85f, 0.05f), Rgba.Hex("3A3346"));
            p.Shape(Sd.Capsule(0.45f, 0.1f, 0.6f, 0.85f, 0.05f), Rgba.Hex("3A3346"));
            p.Shape(Sd.Box(0, 0, 0.7f, 0.3f, 0.1f), Rgba.Hex("F4F2F8"));
            for (int i = 0; i < 4; i++) p.Fill(Sd.Circle(-0.4f + i * 0.27f, -0.05f, 0.05f), i == 3 ? Rgba.Hex("FF6B6B") : Rgba.Hex("8CE05A"));
            return new DoodleArt("s41k_router", p);
        }

        /// <summary>Spezzone di cavo con due spinotti (centro 0,0, lungo 1.3).</summary>
        public static DoodleArt S41K_Cable()
        {
            var p = new DoodlePainter(-0.72f, -0.32f, 0.72f, 0.32f, 110) { Ink = 0.04f };
            var pts = new List<float>();
            for (int i = 0; i <= 12; i++) { float x = -0.5f + i / 12f; pts.Add(x); pts.Add(0.07f * S41K_Sin(x * 9f)); }
            p.Shape(Sd.Polyline(0.08f, pts.ToArray()), Rgba.Hex("D5D9E2"));
            p.Shape(Sd.Box(-0.55f, 0, 0.1f, 0.2f, 0.05f), Rgba.Hex("8E95A5"));
            p.Shape(Sd.Box(0.55f, 0, 0.1f, 0.2f, 0.05f), Rgba.Hex("8E95A5"));
            return new DoodleArt("s41k_cable", p);
        }

        /// <summary>Presa a muro su cui si attacca lo spinotto (centro 0,0).</summary>
        public static DoodleArt S41K_Socket()
        {
            var p = new DoodlePainter(-0.3f, -0.3f, 0.3f, 0.3f, 130) { Ink = 0.04f };
            p.Shape(Sd.Box(0, 0, 0.24f, 0.24f, 0.06f), Rgba.Hex("6E7480"));
            return new DoodleArt("s41k_socket", p);
        }

        /// <summary>Forma del connettore: 0 quadrato blu, 1 triangolo rosso, 2 cerchio verde, 3 stella gialla.</summary>
        public static DoodleArt S41K_Plug(int kind)
        {
            var p = new DoodlePainter(-0.18f, -0.18f, 0.18f, 0.18f, 170) { Ink = 0.03f };
            switch (kind)
            {
                case 0: p.Shape(Sd.Box(0, 0, 0.12f, 0.12f, 0.02f), Rgba.Hex("3E8EF7")); break;
                case 1: p.Shape(Sd.Polygon(-0.14f, -0.11f, 0.14f, -0.11f, 0f, 0.14f), Rgba.Hex("F05A5A")); break;
                case 2: p.Shape(Sd.Circle(0, 0, 0.13f), Rgba.Hex("4CC06A")); break;
                default: p.Shape(Sd.Polygon(S41K_StarPts(0, 0, 0.15f, 0.07f, 5)), Rgba.Hex("FFD13B")); break;
            }
            return new DoodleArt("s41k_plug_" + kind, p);
        }

        /// <summary>Scintilla elettrica (centro 0,0).</summary>
        public static DoodleArt S41K_Spark()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.5f, 0.5f, 110) { Ink = 0.035f };
            p.Shape(Sd.Polygon(S41K_StarPts(0, 0, 0.45f, 0.17f, 8, 0f)), Rgba.Hex("FFE36A"));
            p.Fill(Sd.Circle(0, 0, 0.12f), Rgba.White);
            return new DoodleArt("s41k_spark", p);
        }

        // ================= THE LAUGHING LOCK =================

        /// <summary>Muro di mattoni con la porta aperta su una stanza col formaggio (coordinate mondo): vano da -1.5 a 1.5.</summary>
        public static DoodleArt S41K_LockRoomBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.Fill(Sd.Box(0, 0, 3.3f, 5.3f), Rgba.Hex("C9705A"));
            for (int r = 0; r < 26; r++)
                for (int c = 0; c < 8; c++)
                {
                    float x = -3.2f + c * 0.9f + (r % 2) * 0.45f, y = -4.2f + r * 0.38f;
                    p.Shape(Sd.Box(x, y, 0.4f, 0.15f, 0.04f), Rgba.Lerp(Rgba.Hex("D47E64"), Rgba.Hex("BC664F"), ((r * 7 + c * 3) % 5) / 5f), 0.025f);
                }
            // pavimento
            p.Shape(Sd.Box(0, -4.75f, 3.4f, 0.65f), Rgba.Hex("8A6A55"));
            // vano della porta con la stanza del tesoro
            p.Shape(Sd.Box(0, -0.95f, 1.62f, 2.95f, 0.05f), Rgba.Hex("6E4A32"));
            p.FillFunc(Sd.Box(0, -1.0f, 1.45f, 2.85f), (x, y) => Rgba.Lerp(Rgba.Hex("FFE9A0"), Rgba.Hex("FFC94A"), S41K_Clamp((y + 3f) / 5f)));
            p.Shape(Sd.Polygon(-0.7f, -3.6f, 0.8f, -3.6f, 0.8f, -2.9f, -0.7f, -3.25f), Rgba.Hex("FFD15A"));
            p.Fill(Sd.Circle(0.1f, -3.35f, 0.08f), Rgba.Hex("E8A82A"));
            p.Fill(Sd.Circle(0.45f, -3.25f, 0.06f), Rgba.Hex("E8A82A"));
            return new DoodleArt("s41k_lockroom_bg", p);
        }

        /// <summary>Porta di legno con la cerniera a sinistra (origine = cerniera in basso, 2.9 x 5.7).</summary>
        public static DoodleArt S41K_WoodDoor()
        {
            var p = new DoodlePainter(-0.05f, -0.05f, 3.0f, 5.75f, 60) { Ink = 0.055f };
            Sdf d = Sd.Box(1.45f, 2.85f, 1.42f, 2.82f, 0.06f);
            p.Shape(d, Rgba.Hex("B9773F"));
            for (int i = 1; i < 4; i++) p.Line(Rgba.Hex("8F5629"), 0.04f, i * 0.72f, 0.1f, i * 0.72f, 5.6f);
            foreach (float y in new[] { 1.0f, 4.7f })
            {
                p.Shape(Sd.Box(1.45f, y, 1.4f, 0.12f), Rgba.Hex("6E6878"), 0.035f);
                for (int k = 0; k < 5; k++) p.Fill(Sd.Circle(0.3f + k * 0.6f, y, 0.04f), Rgba.Hex("C9C4D4"));
            }
            return new DoodleArt("s41k_wood_door", p);
        }

        /// <summary>Lucchetto con la faccia: 0 ride, 1 stanco, 2 sbadiglia, 3 aperto (centro del corpo 0,0; bocca in 0,-0.2).</summary>
        public static DoodleArt S41K_Lock(int mood)
        {
            var p = new DoodlePainter(-0.95f, -0.85f, 0.95f, 1.95f, 110) { Ink = 0.05f };
            Rgba gold = Rgba.Hex("FFC93C"), shade = Rgba.Hex("E39B1B");
            // arco del lucchetto (aperto: sollevato a destra)
            float lift = mood == 3 ? 0.4f : 0f;
            Sdf arc = Sd.Subtract(Sd.Circle(0, 0.75f + lift, 0.5f), Sd.Circle(0, 0.75f + lift, 0.32f));
            p.Shape(Sd.Union(Sd.Intersect(arc, Sd.Box(0, 1.1f + lift, 0.6f, 0.4f)), Sd.Box(-0.41f, 0.6f + lift * 0.5f, 0.09f, 0.18f + lift * 0.5f),
                Sd.Box(0.41f, 0.6f + lift, 0.09f, 0.18f)), Rgba.Hex("C9CED8"));
            Sdf body = Sd.Box(0, 0, 0.78f, 0.68f, 0.2f);
            p.Shape(body, gold);
            p.Fill(Sd.Crescent(body, -0.08f, 0.1f), shade);
            p.Fill(Sd.Box(-0.45f, 0.45f, 0.18f, 0.06f, 0.05f), Rgba.Hex("FFF0B0"));
            switch (mood)
            {
                case 0:
                    // occhi chiusi dalle risate, lacrime di gioia, boccone aperto con la lingua
                    p.Line(Ink, 0.05f, -0.42f, 0.22f, -0.3f, 0.34f, -0.18f, 0.22f);
                    p.Line(Ink, 0.05f, 0.18f, 0.22f, 0.3f, 0.34f, 0.42f, 0.22f);
                    p.Fill(Sd.Ellipse(-0.5f, 0.12f, 0.05f, 0.09f), Rgba.Hex("7FC4FF"));
                    p.Fill(Sd.Ellipse(0.5f, 0.12f, 0.05f, 0.09f), Rgba.Hex("7FC4FF"));
                    {
                        Sdf m = Sd.Intersect(Sd.Ellipse(0, -0.12f, 0.38f, 0.32f), Sd.Box(0, -0.3f, 0.5f, 0.2f));
                        p.Shape(m, Rgba.Hex("7A2236"), 0.04f);
                        p.Fill(Sd.Intersect(m, Sd.Circle(0.05f, -0.48f, 0.17f)), Rgba.Hex("FF7A8A"));
                        p.Fill(Sd.Intersect(m, Sd.Box(0, -0.13f, 0.35f, 0.04f)), Rgba.White);
                    }
                    break;
                case 1:
                    // palpebre a mezz'asta, boccuccia stanca
                    foreach (float x in new[] { -0.3f, 0.3f })
                    {
                        p.Shape(Sd.Circle(x, 0.25f, 0.13f), Rgba.White, 0.035f);
                        p.Fill(Sd.Circle(x, 0.2f, 0.06f), Ink);
                        p.Shape(Sd.Intersect(Sd.Circle(x, 0.25f, 0.15f), Sd.Box(x, 0.36f, 0.2f, 0.1f)), shade, 0.035f);
                    }
                    p.Line(Ink, 0.045f, -0.2f, -0.25f, -0.05f, -0.2f, 0.1f, -0.27f, 0.22f, -0.22f);
                    break;
                case 2:
                    // sbadiglio: occhi strizzati, bocca enorme col buco della serratura
                    p.Line(Ink, 0.05f, -0.45f, 0.3f, -0.3f, 0.25f, -0.17f, 0.3f);
                    p.Line(Ink, 0.05f, 0.17f, 0.3f, 0.3f, 0.25f, 0.45f, 0.3f);
                    p.Shape(Sd.Ellipse(0, -0.2f, 0.3f, 0.38f), Rgba.Hex("4A1426"), 0.045f);
                    p.Fill(Sd.Union(Sd.Circle(0, -0.1f, 0.09f), Sd.Polygon(-0.06f, -0.12f, 0.06f, -0.12f, 0.09f, -0.38f, -0.09f, -0.38f)), Rgba.Hex("120810"));
                    p.Line(Rgba.Hex("FFF0B0"), 0.03f, -0.55f, 0.55f, -0.62f, 0.68f);
                    break;
                default:
                    // aperto: offeso, con gli occhi a spirale
                    foreach (float x in new[] { -0.3f, 0.3f })
                    {
                        p.Shape(Sd.Circle(x, 0.25f, 0.13f), Rgba.White, 0.035f);
                        p.Line(Ink, 0.025f, x, 0.25f, x + 0.06f, 0.25f, x + 0.06f, 0.31f, x - 0.06f, 0.31f, x - 0.06f, 0.19f, x + 0.09f, 0.19f);
                    }
                    p.Shape(Sd.Circle(0, -0.25f, 0.1f), Rgba.Hex("4A1426"), 0.035f);
                    break;
            }
            string[] names = { "s41k_lock_laugh", "s41k_lock_tired", "s41k_lock_yawn", "s41k_lock_open" };
            return new DoodleArt(names[mood], p);
        }

        /// <summary>Maniglia a leva (perno in 0,0, leva verso destra).</summary>
        public static DoodleArt S41K_Handle()
        {
            var p = new DoodlePainter(-0.3f, -0.32f, 0.9f, 0.3f, 130) { Ink = 0.04f };
            p.Shape(Sd.Box(0, 0, 0.18f, 0.26f, 0.06f), Rgba.Hex("8E95A5"));
            p.Shape(Sd.Capsule(0, 0.03f, 0.75f, -0.02f, 0.08f), Rgba.Hex("C9CED8"));
            p.Shape(Sd.Circle(0, 0.03f, 0.1f), Rgba.Hex("E8EBF2"));
            return new DoodleArt("s41k_handle", p);
        }

        /// <summary>Crepa scura da sovrapporre alla chiave (centro 0,0).</summary>
        public static DoodleArt S41K_Crack()
        {
            var p = new DoodlePainter(-0.2f, -0.25f, 0.2f, 0.25f, 160) { Ink = 0.03f };
            p.Line(Ink, 0.035f, -0.05f, 0.22f, 0.04f, 0.08f, -0.06f, -0.02f, 0.05f, -0.12f, -0.02f, -0.22f);
            return new DoodleArt("s41k_crack", p);
        }

        // ================= THE FAKE BOSS =================

        /// <summary>Arena del castello (coordinate mondo): pavimento sotto y -0.8, corsie a x -1.7, 0, 1.7.</summary>
        public static DoodleArt S41K_ArenaBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 2.3f, 3.3f, 3.1f), (x, y) => Rgba.Lerp(Rgba.Hex("3A2C52"), Rgba.Hex("231A36"), S41K_Clamp((y + 0.8f) / 6f)));
            for (int r = 0; r < 10; r++)
                for (int c = 0; c < 6; c++)
                {
                    float x = -3.0f + c * 1.2f + (r % 2) * 0.6f, y = -0.5f + r * 0.6f;
                    p.Shape(Sd.Box(x, y, 0.55f, 0.26f, 0.06f), Rgba.Lerp(Rgba.Hex("4A3A66"), Rgba.Hex("3E3058"), ((r + c) % 3) / 3f), 0.03f);
                }
            // torce
            foreach (int s in new[] { -1, 1 })
            {
                p.Shape(Sd.Box(s * 2.55f, 1.1f, 0.08f, 0.3f), Rgba.Hex("8A5A3B"));
                p.Shape(Sd.Union(Sd.Circle(s * 2.55f, 1.5f, 0.17f), Sd.Polygon(s * 2.55f - 0.15f, 1.55f, s * 2.55f + 0.15f, 1.55f, s * 2.55f, 1.95f)), Rgba.Hex("FF9A3A"));
                p.Fill(Sd.Circle(s * 2.55f, 1.5f, 0.08f), Rgba.Hex("FFE36A"));
            }
            // pavimento in pietra
            p.FillFunc(Sd.Box(0, -3.0f, 3.3f, 2.2f), (x, y) => Rgba.Lerp(Rgba.Hex("6E6080"), Rgba.Hex("857796"), S41K_Clamp((y + 5f) / 4f)));
            p.Line(Ink, 0.05f, -3.3f, -0.8f, 3.3f, -0.8f);
            for (int j = 0; j < 5; j++) p.Line(Rgba.Hex("5D5070"), 0.03f, -3.3f, -1.6f - j * 0.8f, 3.3f, -1.6f - j * 0.8f);
            foreach (float x in new[] { -0.85f, 0.85f }) p.Line(Rgba.Hex("A99BBA", 0.7f), 0.04f, x * 0.8f, -0.8f, x * 1.2f, -5.2f);
            return new DoodleArt("s41k_arena_bg", p);
        }

        /// <summary>Boss gigante con corna, corona e pugni enormi (centro 0,0).</summary>
        public static DoodleArt S41K_Boss()
        {
            var p = new DoodlePainter(-2.15f, -1.55f, 2.15f, 1.75f, 80) { Ink = 0.06f };
            Rgba skin = Rgba.Hex("8E3FB0"), dark = Rgba.Hex("6A2A88"), belly = Rgba.Hex("C27ADB");
            // corna
            p.Shape(Sd.Polygon(-0.85f, 0.95f, -0.45f, 1.0f, -1.15f, 1.65f), Rgba.Hex("F4EEDC"));
            p.Shape(Sd.Polygon(0.85f, 0.95f, 0.45f, 1.0f, 1.15f, 1.65f), Rgba.Hex("F4EEDC"));
            // braccia e pugni
            p.Shape(Sd.Capsule(-1.0f, 0.3f, -1.6f, -0.6f, 0.25f), dark);
            p.Shape(Sd.Capsule(1.0f, 0.3f, 1.6f, -0.6f, 0.25f), dark);
            p.Shape(Sd.Circle(-1.65f, -0.9f, 0.45f), skin);
            p.Shape(Sd.Circle(1.65f, -0.9f, 0.45f), skin);
            for (int k = -1; k <= 1; k++)
            {
                p.Line(Ink, 0.04f, -1.65f + k * 0.18f, -0.6f, -1.65f + k * 0.18f, -0.75f);
                p.Line(Ink, 0.04f, 1.65f + k * 0.18f, -0.6f, 1.65f + k * 0.18f, -0.75f);
            }
            // corpo
            Sdf body = Sd.Ellipse(0, 0, 1.2f, 1.1f);
            p.Shape(body, skin);
            p.Fill(Sd.Crescent(body, -0.15f, 0.2f), dark);
            p.Fill(Sd.Ellipse(0, -0.55f, 0.65f, 0.45f), belly);
            // occhi arrabbiati
            S41K_Eye(p, -0.4f, 0.35f, 0.2f, 0.2f, -0.2f);
            S41K_Eye(p, 0.4f, 0.35f, 0.2f, -0.2f, -0.2f);
            p.Line(Ink, 0.09f, -0.7f, 0.68f, -0.2f, 0.5f);
            p.Line(Ink, 0.09f, 0.7f, 0.68f, 0.2f, 0.5f);
            // bocca con le zanne
            Sdf mouth = Sd.Intersect(Sd.Ellipse(0, -0.1f, 0.55f, 0.32f), Sd.Box(0, -0.25f, 0.6f, 0.17f));
            p.Shape(mouth, Rgba.Hex("3A1030"));
            p.Shape(Sd.Polygon(-0.35f, -0.08f, -0.2f, -0.08f, -0.28f, -0.28f), Rgba.White, 0.03f);
            p.Shape(Sd.Polygon(0.35f, -0.08f, 0.2f, -0.08f, 0.28f, -0.28f), Rgba.White, 0.03f);
            // corona
            p.Shape(Sd.Polygon(-0.45f, 0.95f, 0.45f, 0.95f, 0.5f, 1.45f, 0.25f, 1.2f, 0f, 1.5f, -0.25f, 1.2f, -0.5f, 1.45f), Rgba.Hex("FFC93C"));
            p.Fill(Sd.Circle(0, 1.12f, 0.07f), Rgba.Hex("FF5A5A"));
            return new DoodleArt("s41k_boss", p);
        }

        /// <summary>Il boss "vero": un calzino-pupazzo imbarazzato (stessa scala del boss).</summary>
        public static DoodleArt S41K_Puppet()
        {
            var p = new DoodlePainter(-1.3f, -1.55f, 1.3f, 1.6f, 80) { Ink = 0.07f };
            Rgba sock = Rgba.Hex("B27ACB");
            p.Shape(Sd.Union(Sd.Box(0, -0.6f, 0.75f, 0.9f, 0.2f), Sd.Ellipse(0.15f, 0.4f, 0.95f, 0.75f)), sock);
            for (int i = 0; i < 3; i++) p.Fill(Sd.Box(0, -1.25f + i * 0.22f, 0.76f, 0.06f), Rgba.Hex("8E58A8"));
            p.Shape(Sd.Polygon(-0.4f, 0.95f, -0.15f, 1.0f, -0.45f, 1.4f), Rgba.Hex("F4EEDC"));
            p.Shape(Sd.Polygon(0.45f, 0.95f, 0.7f, 1.0f, 0.65f, 1.4f), Rgba.Hex("F4EEDC"));
            S41K_Eye(p, -0.15f, 0.5f, 0.24f, 0.3f, 0.5f);
            S41K_Eye(p, 0.45f, 0.55f, 0.2f, -0.4f, 0.5f);
            p.Fill(Sd.Ellipse(-0.35f, 0.15f, 0.2f, 0.1f), Rgba.Hex("FF7F9C", 0.9f));
            p.Fill(Sd.Ellipse(0.7f, 0.2f, 0.18f, 0.09f), Rgba.Hex("FF7F9C", 0.9f));
            p.Line(Ink, 0.06f, -0.1f, 0.05f, 0.1f, 0.1f, 0.3f, 0.03f, 0.5f, 0.08f);
            p.Line(Rgba.Hex("7FC4FF"), 0.07f, 1.0f, 0.95f, 1.05f, 0.75f);
            return new DoodleArt("s41k_puppet", p);
        }

        /// <summary>Eroe minuscolo senza armi (centro 0,0, alto 1).</summary>
        public static DoodleArt S41K_Hero()
        {
            var p = new DoodlePainter(-0.45f, -0.55f, 0.45f, 0.6f, 150) { Ink = 0.04f };
            p.Shape(Sd.Capsule(-0.1f, -0.2f, -0.12f, -0.45f, 0.06f), Rgba.Hex("6E6878"));
            p.Shape(Sd.Capsule(0.1f, -0.2f, 0.12f, -0.45f, 0.06f), Rgba.Hex("6E6878"));
            p.Shape(Sd.Polygon(-0.3f, 0.15f, -0.38f, -0.25f, 0f, -0.18f), Rgba.Hex("E8443A"));
            p.Shape(Sd.Box(0, -0.05f, 0.2f, 0.2f, 0.07f), Rgba.Hex("C9CED8"));
            p.Shape(Sd.Capsule(0.18f, 0.05f, 0.35f, 0.25f, 0.05f), Rgba.Hex("C9CED8"));
            p.Shape(Sd.Capsule(-0.18f, 0.05f, -0.35f, 0.25f, 0.05f), Rgba.Hex("C9CED8"));
            p.Shape(Sd.Circle(0, 0.32f, 0.2f), Rgba.Hex("D8DEE8"));
            p.Fill(Sd.Box(0, 0.32f, 0.13f, 0.035f), Ink);
            p.Shape(Sd.Polygon(-0.04f, 0.5f, 0.04f, 0.5f, 0.1f, 0.6f, -0.05f, 0.58f), Rgba.Hex("E8443A"), 0.025f);
            p.Fill(Sd.Circle(-0.28f, 0.42f, 0.05f), Rgba.Hex("7FC4FF"));
            return new DoodleArt("s41k_hero", p);
        }

        /// <summary>Cornice della barra della vita (centro 0,0, 4.5 x 0.55): teschio a sinistra, vano da x -1.75 a 2.1.</summary>
        public static DoodleArt S41K_HpBar()
        {
            var p = new DoodlePainter(-2.32f, -0.34f, 2.32f, 0.34f, 90) { Ink = 0.05f };
            Sdf frame = Sd.Box(0.18f, 0, 2.05f, 0.25f, 0.08f);
            p.Shape(Sd.Subtract(frame, Sd.Box(0.18f, 0, 1.94f, 0.17f, 0.04f)), Rgba.Hex("FFC93C"));
            p.Shape(Sd.Circle(-2.0f, 0, 0.28f), Rgba.Hex("F4EEDC"));
            p.Fill(Sd.Circle(-2.08f, 0.04f, 0.06f), Ink);
            p.Fill(Sd.Circle(-1.92f, 0.04f, 0.06f), Ink);
            p.Line(Ink, 0.03f, -2.1f, -0.13f, -1.9f, -0.13f);
            return new DoodleArt("s41k_hpbar", p);
        }
    }
}
