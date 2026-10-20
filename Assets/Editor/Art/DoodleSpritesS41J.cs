using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli della specifica 105-200, pacchetto J.</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV41J() => new DoodleArt[]
        {
            S41J_Pill(),
            // The angry keyboard
            S41J_KbdBg(), S41J_KbdMonitor(), S41J_KbdSlot(), S41J_KbdBase(), S41J_KbdKey(), S41J_KbdFace(false), S41J_KbdFace(true),
            // The cowardly dragon
            S41J_DragonBg(), S41J_Dragon(false), S41J_Dragon(true), S41J_DragonShadow(), S41J_Barricade(), S41J_FireBreath(),
            // Hidden frequency
            S41J_FreqBg(), S41J_FreqSafe(false), S41J_FreqSafe(true), S41J_FreqDial(), S41J_FreqLock(), S41J_FreqWiggle(), S41J_FreqLight(),
            // The crooked bridge
            S41J_ChasmBg(), S41J_BridgePlank(), S41J_BridgeKnob(),
            // The final wrong answer
            S41J_QuizBg(), S41J_QuizDoor(false), S41J_QuizDoor(true), S41J_QuizSign(), S41J_WallHole(), S41J_QuizX(),
            // The impatient clock
            S41J_ClockBg(), S41J_ClockBody(), S41J_ClockFace(false), S41J_ClockFace(true), S41J_ClockHand(false), S41J_ClockHand(true),
            // The frozen waterfall
            S41J_FallsBg(), S41J_IceFall(), S41J_WaterFall(), S41J_Lens(), S41J_LensAim(),
            // The fake checkpoint
            S41J_CpBg(), S41J_CpPole(), S41J_CpFlag(), S41J_CpDoor(false), S41J_CpDoor(true), S41J_CpHero(),
        };

        static float S41J_Clamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
        static float S41J_Sin(float v) => (float)Math.Sin(v);
        static float S41J_Cos(float v) => (float)Math.Cos(v);

        static void S41J_Eyes(DoodlePainter p, float cx, float cy, float gap, float r, float lookX = 0f, float lookY = 0f)
        {
            foreach (float s in new[] { -1f, 1f })
            {
                float x = cx + s * gap;
                p.Shape(Sd.Circle(x, cy, r), Rgba.White, r * 0.35f);
                p.Fill(Sd.Circle(x + lookX * r * 0.35f, cy + lookY * r * 0.35f, r * 0.5f), Ink);
                p.Fill(Sd.Circle(x + lookX * r * 0.35f - r * 0.18f, cy + lookY * r * 0.35f + r * 0.2f, r * 0.17f), Rgba.White);
            }
        }

        /// <summary>Pulsante a pillola bianco (si tinge), 1.7 x 0.7.</summary>
        public static DoodleArt S41J_Pill()
        {
            var p = new DoodlePainter(-0.92f, -0.42f, 0.92f, 0.42f, 120) { Ink = 0.05f };
            p.Shape(Sd.Box(0, -0.04f, 0.85f, 0.33f, 0.33f), Rgba.Hex("BFBFC8"));
            p.Shape(Sd.Box(0, 0.02f, 0.85f, 0.33f, 0.33f), Rgba.White);
            p.Fill(Sd.Box(-0.35f, 0.2f, 0.3f, 0.05f, 0.05f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("s41j_pill", p);
        }

        // ================= THE ANGRY KEYBOARD =================

        /// <summary>Ufficio: parete fino a y -0.6, scrivania sotto.</summary>
        public static DoodleArt S41J_KbdBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 2.3f, 3.3f, 2.95f), (x, y) => Rgba.Lerp(Rgba.Hex("8FD3C7"), Rgba.Hex("5FB3A8"), S41J_Clamp((y + 0.6f) / 5.8f)));
            for (float x = -3.0f; x < 3.3f; x += 0.8f) p.Fill(Sd.Box(x, 2.3f, 0.18f, 2.95f), Rgba.Hex("A6DED4", 0.35f));
            // orologio e post-it sulla parete
            p.Shape(Sd.Box(-2.35f, 3.0f, 0.3f, 0.3f, 0.03f), Rgba.Hex("FFE36E"), 0.04f);
            p.Shape(Sd.Box(2.3f, 2.95f, 0.28f, 0.28f, 0.03f), Rgba.Hex("FF9FC0"), 0.04f);
            // scrivania
            p.FillFunc(Sd.Box(0, -2.9f, 3.3f, 2.35f), (x, y) => Rgba.Lerp(Rgba.Hex("A8693F"), Rgba.Hex("D2914F"), S41J_Clamp((y + 5.2f) / 4.6f)));
            for (float y = -1.4f; y > -5.3f; y -= 0.9f) p.Line(Rgba.Hex("8A532E", 0.5f), 0.03f, -3.3f, y, 3.3f, y + 0.1f);
            p.Shape(Sd.Box(0, -0.6f, 3.3f, 0.09f), Rgba.Hex("E2A465"), 0.04f);
            // tazza a sinistra
            p.Shape(Sd.Box(-2.55f, -0.35f, 0.2f, 0.22f, 0.05f), Rgba.Hex("FF8A65"), 0.04f);
            return new DoodleArt("s41j_kbd_bg", p);
        }

        /// <summary>Monitor (centro 0,0 = centro dello schermo): schermo ±2.25 x ±1.12, piede fino a y -1.65.</summary>
        public static DoodleArt S41J_KbdMonitor()
        {
            var p = new DoodlePainter(-2.6f, -1.75f, 2.6f, 1.45f, 80) { Ink = 0.06f };
            p.Shape(Sd.Polygon(-0.5f, -1.65f, 0.5f, -1.65f, 0.25f, -1.2f, -0.25f, -1.2f), Rgba.Hex("7C6F94"));
            p.Shape(Sd.Box(0, 0, 2.5f, 1.35f, 0.18f), Rgba.Hex("D9D3E6"));
            p.Fill(Sd.Box(0.05f, -0.05f, 2.42f, 1.27f, 0.15f), Rgba.Hex("B8AFCB"));
            p.Shape(Sd.Box(0, 0.02f, 2.25f, 1.12f, 0.06f), Rgba.Hex("17223A"), 0.04f);
            p.FillFunc(Sd.Box(0, 0.02f, 2.2f, 1.07f, 0.05f), (x, y) => Rgba.Lerp(Rgba.Hex("1C3554"), Rgba.Hex("10203A"), S41J_Clamp((y + 1f) / 2f)));
            for (float y = -1.0f; y < 1.1f; y += 0.12f) p.Line(Rgba.Hex("2A4A70", 0.35f), 0.02f, -2.15f, y, 2.15f, y);
            p.Fill(Sd.Polygon(-2.1f, 1.0f, -1.5f, 1.0f, -2.1f, 0.4f), Rgba.White.WithAlpha(0.12f));
            p.Shape(Sd.Circle(2.25f, -1.22f, 0.05f), Rgba.Hex("6CFF7A"), 0.02f);
            return new DoodleArt("s41j_kbd_monitor", p);
        }

        /// <summary>Casella della risposta (0.8 x 0.85).</summary>
        public static DoodleArt S41J_KbdSlot()
        {
            var p = new DoodlePainter(-0.45f, -0.48f, 0.45f, 0.48f, 140) { Ink = 0.04f };
            p.Shape(Sd.Box(0, 0, 0.38f, 0.41f, 0.07f), Rgba.Hex("EAF4FF"));
            p.Fill(Sd.Box(0, -0.3f, 0.26f, 0.025f), Rgba.Hex("7FA6D6"));
            return new DoodleArt("s41j_kbd_slot", p);
        }

        /// <summary>Base della tastiera (centro 0,0): 5.6 x 3.9, cornice in alto per la faccia.</summary>
        public static DoodleArt S41J_KbdBase()
        {
            var p = new DoodlePainter(-2.95f, -2.08f, 2.95f, 2.08f, 70) { Ink = 0.06f };
            p.Shape(Sd.Box(0, -0.08f, 2.82f, 1.95f, 0.3f), Rgba.Hex("4E3F66"));
            p.Shape(Sd.Box(0, 0.0f, 2.82f, 1.95f, 0.3f), Rgba.Hex("8E7BB0"));
            p.Fill(Sd.Box(0, -0.25f, 2.65f, 1.6f, 0.15f), Rgba.Hex("6B5A8A"));
            p.Fill(Sd.Box(-1.6f, 1.75f, 0.8f, 0.05f, 0.05f), Rgba.White.WithAlpha(0.35f));
            return new DoodleArt("s41j_kbd_base", p);
        }

        /// <summary>Tasto vuoto (0.78 x 0.78, centro 0,0).</summary>
        public static DoodleArt S41J_KbdKey()
        {
            var p = new DoodlePainter(-0.45f, -0.45f, 0.45f, 0.45f, 140) { Ink = 0.04f };
            p.Shape(Sd.Box(0, -0.03f, 0.38f, 0.37f, 0.1f), Rgba.Hex("BDB3CC"));
            p.Shape(Sd.Box(0, 0.04f, 0.32f, 0.3f, 0.08f), Rgba.Hex("FBF7FF"), 0.03f);
            p.Fill(Sd.Box(-0.12f, 0.24f, 0.12f, 0.03f, 0.03f), Rgba.White);
            return new DoodleArt("s41j_kbd_key", p);
        }

        /// <summary>Faccia sulla cornice della tastiera (2.2 x 0.5): arrabbiata o sconfitta.</summary>
        public static DoodleArt S41J_KbdFace(bool defeated)
        {
            var p = new DoodlePainter(-1.15f, -0.3f, 1.15f, 0.32f, 140) { Ink = 0.035f };
            foreach (float s in new[] { -1f, 1f })
            {
                float x = s * 0.7f;
                p.Shape(Sd.Ellipse(x, -0.03f, 0.2f, 0.16f), Rgba.White, 0.03f);
                p.Fill(Sd.Circle(x - s * 0.04f, defeated ? -0.08f : -0.04f, 0.08f), Ink);
                if (defeated) p.Fill(Sd.Box(x, 0.08f, 0.22f, 0.09f), Rgba.Hex("8E7BB0"));   // palpebre giù
                else p.InkLine(0.06f, x - s * 0.26f, 0.24f, x + s * 0.2f, 0.08f);          // sopracciglia arrabbiate
            }
            if (defeated) p.InkLine(0.04f, -0.22f, -0.18f, -0.07f, -0.12f, 0.07f, -0.18f, 0.22f, -0.12f);
            else
            {
                p.Shape(Sd.Box(0, -0.12f, 0.24f, 0.08f, 0.03f), Rgba.White, 0.03f);
                for (float x = -0.12f; x < 0.2f; x += 0.12f) p.InkLine(0.02f, x, -0.2f, x, -0.04f);
            }
            return new DoodleArt(defeated ? "s41j_kbd_face_defeated" : "s41j_kbd_face_angry", p);
        }

        // ================= THE COWARDLY DRAGON =================

        /// <summary>Muro del castello (parete d'ombra) fino a y 1.4, porta ad arco a x 1.9, prato da y -2.6.</summary>
        public static DoodleArt S41J_DragonBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 3.3f, 3.3f, 2.0f), (x, y) => Rgba.Lerp(Rgba.Hex("FFD9A0"), Rgba.Hex("8EC9F0"), S41J_Clamp((y - 1.4f) / 3.5f)));
            p.Fill(Sd.Polygon(-3.3f, 1.4f, -2.0f, 2.6f, -0.8f, 1.6f, 0.4f, 2.9f, 1.8f, 1.7f, 3.3f, 2.4f, 3.3f, 1.4f), Rgba.Hex("B9A6D6"));
            // muro di pietra
            Sdf wall = Sd.Box(0, -0.6f, 3.3f, 2.0f);
            p.FillFunc(wall, (x, y) => Rgba.Lerp(Rgba.Hex("C8BBA8"), Rgba.Hex("E2D6C2"), S41J_Clamp((y + 2.6f) / 4f)));
            for (int r = 0; r < 7; r++)
            {
                float y = -2.6f + r * 0.58f;
                p.Line(Rgba.Hex("A99A84", 0.7f), 0.03f, -3.3f, y, 3.3f, y);
                for (float x = -3.3f + (r % 2) * 0.45f; x < 3.3f; x += 0.9f) p.Line(Rgba.Hex("A99A84", 0.7f), 0.03f, x, y, x, y + 0.58f);
            }
            // merli
            for (float x = -3.0f; x < 3.3f; x += 0.9f) p.Shape(Sd.Box(x, 1.6f, 0.28f, 0.22f), Rgba.Hex("E2D6C2"), 0.05f);
            p.Shape(Sd.Box(0, 1.4f, 3.4f, 0.06f), Rgba.Hex("D2C4AE"), 0.05f);
            // porta ad arco (uscita) dietro la barricata
            Sdf gate = Sd.Union(Sd.Box(1.9f, -1.85f, 0.65f, 0.75f), Sd.Circle(1.9f, -1.1f, 0.65f));
            p.Shape(gate, Rgba.Hex("3A2C48"), 0.06f);
            p.Fill(Sd.Box(1.9f, -2.3f, 0.5f, 0.3f), Rgba.Hex("6FBF5A", 0.5f));
            // prato
            p.FillFunc(Sd.Box(0, -3.95f, 3.3f, 1.35f), (x, y) => Rgba.Lerp(Rgba.Hex("6FBF5A"), Rgba.Hex("97D66E"), S41J_Clamp((y + 5.2f) / 2.6f)));
            p.Shape(Sd.Box(0, -2.6f, 3.4f, 0.05f), Rgba.Hex("5AA548"), 0.04f);
            for (float x = -2.8f; x < 3f; x += 0.7f) p.Line(Rgba.Hex("4E9A3E", 0.7f), 0.03f, x, -3.4f + S41J_Sin(x * 3f) * 0.3f, x + 0.08f, -3.2f + S41J_Sin(x * 3f) * 0.3f);
            return new DoodleArt("s41j_dragon_bg", p);
        }

        /// <summary>Draghetto verde che guarda a destra (piedi in 0,0; bocca in 0.95,1.4).</summary>
        public static DoodleArt S41J_Dragon(bool scared)
        {
            var p = new DoodlePainter(-1.35f, -0.08f, 1.25f, 2.1f, 110) { Ink = 0.05f };
            Rgba g = scared ? Rgba.Hex("A9D99A") : Rgba.Hex("7CCB6A"), gs = scared ? Rgba.Hex("86BC78") : Rgba.Hex("57A64A");
            Rgba belly = Rgba.Hex("FFE7A8"), wing = Rgba.Hex("B98AE0");
            // coda
            p.Shape(Sd.Polyline(0.13f, -0.3f, 0.35f, -0.8f, 0.3f, -1.1f, 0.5f), g);
            p.Shape(Sd.Polygon(-1.1f, 0.45f, -1.32f, 0.72f, -1.0f, 0.68f), Rgba.Hex("FF8FB0"), 0.035f);
            // ala
            p.Shape(Sd.Polygon(-0.3f, 1.2f, -1.0f, 1.75f, -0.85f, 1.35f, -1.05f, 1.3f, -0.75f, 1.05f, -0.85f, 0.95f, -0.35f, 0.9f), wing);
            // zampe dietro
            p.Shape(Sd.Box(-0.35f, 0.2f, 0.17f, 0.2f, 0.07f), gs);
            // corpo
            Body(p, Sd.Ellipse(-0.05f, 0.75f, 0.62f, 0.68f), g, gs);
            p.Fill(Sd.Ellipse(0.12f, 0.62f, 0.36f, 0.48f), belly);
            for (float y = 0.35f; y < 1.05f; y += 0.16f) p.Line(Rgba.Hex("E5C27A", 0.8f), 0.025f, -0.12f, y, 0.36f, y);
            p.Shape(Sd.Box(0.2f, 0.17f, 0.17f, 0.17f, 0.07f), g);
            // spine sulla schiena
            foreach (float a in new[] { 2.0f, 2.35f, 2.7f })
                p.Shape(Sd.Polygon(-0.05f + S41J_Cos(a) * 0.62f, 0.75f + S41J_Sin(a) * 0.68f, -0.05f + S41J_Cos(a) * 0.82f, 0.75f + S41J_Sin(a) * 0.85f, -0.05f + S41J_Cos(a + 0.2f) * 0.6f, 0.75f + S41J_Sin(a + 0.2f) * 0.66f), Rgba.Hex("FF8FB0"), 0.03f);
            // braccini
            p.Shape(Sd.Capsule(0.35f, 0.95f, scared ? 0.62f : 0.55f, scared ? 1.25f : 0.8f, 0.08f), g, 0.035f);
            // testa
            Sdf head = Sd.Union(Sd.Circle(0.42f, 1.45f, 0.42f), Sd.Ellipse(0.78f, 1.36f, 0.36f, 0.25f));
            Body(p, head, g, gs, null, -0.05f, 0.06f);
            // cornini
            p.Shape(Sd.Polygon(0.15f, 1.78f, 0.05f, 2.05f, 0.3f, 1.84f), Rgba.Hex("FFF1C9"), 0.03f);
            p.Shape(Sd.Polygon(0.45f, 1.85f, 0.45f, 2.08f, 0.6f, 1.82f), Rgba.Hex("FFF1C9"), 0.03f);
            p.Fill(Sd.Circle(1.0f, 1.42f, 0.035f), Ink);
            if (scared)
            {
                p.Shape(Sd.Circle(0.48f, 1.58f, 0.16f), Rgba.White, 0.035f);
                p.Fill(Sd.Circle(0.53f, 1.58f, 0.045f), Ink);
                p.Shape(Sd.Ellipse(0.88f, 1.2f, 0.12f, 0.1f), Rgba.Hex("7A2E3A"), 0.03f);   // bocca a "o"
                p.Shape(Sd.Polygon(0.2f, 1.75f, 0.12f, 1.55f, 0.28f, 1.55f), Rgba.Hex("9FD8FF"), 0.02f);   // goccia di sudore
                p.InkLine(0.025f, 0.32f, 1.82f, 0.62f, 1.78f);
            }
            else
            {
                p.Shape(Sd.Ellipse(0.5f, 1.55f, 0.11f, 0.13f), Rgba.White, 0.03f);
                p.Fill(Sd.Circle(0.53f, 1.52f, 0.05f), Ink);
                p.InkLine(0.025f, 0.38f, 1.74f, 0.6f, 1.7f);   // sopracciglio preoccupato
                p.InkLine(0.03f, 0.75f, 1.22f, 0.85f, 1.25f, 0.95f, 1.21f);
                p.Fill(Sd.Circle(0.62f, 1.3f, 0.07f), BlushColor.WithAlpha(0.6f));
            }
            return new DoodleArt(scared ? "s41j_dragon_scared" : "s41j_dragon", p);
        }

        /// <summary>Ombra mostruosa (base in 0,0, guarda a sinistra, alta 2.3).</summary>
        public static DoodleArt S41J_DragonShadow()
        {
            var p = new DoodlePainter(-1.4f, -0.05f, 1.4f, 2.45f, 70) { Ink = 0f, Wobble = 0.03f };
            Rgba c = Rgba.Hex("1E1030");
            Sdf body = Sd.Union(
                Sd.Ellipse(0.1f, 0.85f, 0.75f, 0.8f),
                Sd.Circle(-0.45f, 1.6f, 0.5f),
                Sd.Ellipse(-0.85f, 1.45f, 0.45f, 0.3f),
                Sd.Polygon(0.2f, 1.2f, 1.35f, 2.35f, 1.0f, 1.6f, 1.38f, 1.55f, 0.95f, 1.15f, 1.3f, 0.9f, 0.6f, 0.75f),
                Sd.Polygon(-0.3f, 2.0f, -0.1f, 2.45f, -0.05f, 1.9f),
                Sd.Polygon(-0.65f, 1.95f, -0.75f, 2.4f, -0.45f, 2.0f),
                Sd.Polyline(0.12f, 0.7f, 0.3f, 1.1f, 0.15f, 1.3f, 0.45f),
                Sd.Box(-0.3f, 0.2f, 0.2f, 0.22f), Sd.Box(0.4f, 0.2f, 0.2f, 0.22f),
                Sd.Capsule(-0.4f, 1.0f, -0.95f, 0.75f, 0.11f));
            foreach (float a in new[] { 1.2f, 1.5f, 1.8f })
                body = Sd.Union(body, Sd.Polygon(0.1f + S41J_Cos(a) * 0.7f, 0.85f + S41J_Sin(a) * 0.75f, 0.1f + S41J_Cos(a + 0.12f) * 1.05f, 0.85f + S41J_Sin(a + 0.12f) * 1.1f, 0.1f + S41J_Cos(a + 0.25f) * 0.7f, 0.85f + S41J_Sin(a + 0.25f) * 0.75f));
            p.Fill(body, c);
            // artigli
            foreach (float k in new[] { -1.05f, -0.98f, -0.9f }) p.Fill(Sd.Polygon(k, 0.72f, k - 0.12f, 0.6f, k + 0.04f, 0.68f), c);
            // occhi infuocati e denti
            p.Fill(Sd.Polygon(-0.75f, 1.75f, -0.45f, 1.65f, -0.7f, 1.6f), Rgba.Hex("FF5A3A", 0.95f));
            p.Fill(Sd.Polygon(-0.25f, 1.78f, -0.4f, 1.66f, -0.18f, 1.64f), Rgba.Hex("FF5A3A", 0.95f));
            p.Fill(Sd.Polygon(-1.25f, 1.38f, -0.6f, 1.38f, -0.68f, 1.28f, -0.78f, 1.36f, -0.88f, 1.26f, -0.98f, 1.36f, -1.08f, 1.26f, -1.18f, 1.36f), Rgba.Hex("FFE36A", 0.85f));
            return new DoodleArt("s41j_dragon_shadow", p);
        }

        /// <summary>Barricata di assi e casse (base in 0,0, larga 1.5, alta 2.1).</summary>
        public static DoodleArt S41J_Barricade()
        {
            var p = new DoodlePainter(-0.85f, -0.08f, 0.85f, 2.2f, 110) { Ink = 0.045f };
            p.Shape(Sd.Box(-0.35f, 0.38f, 0.38f, 0.38f, 0.03f), Wood);
            p.InkLine(0.03f, -0.7f, 0.05f, 0f, 0.72f);
            p.InkLine(0.03f, -0.7f, 0.72f, 0f, 0.05f);
            p.Shape(Sd.Box(0.4f, 0.32f, 0.32f, 0.32f, 0.03f), WoodShade);
            foreach (float y in new[] { 0.95f, 1.35f, 1.75f })
                p.Shape(Sd.Rotate(Sd.Box(0, y, 0.75f, 0.13f, 0.03f), 0, y, y * 9f - 12f), y > 1.5f ? WoodShade : Wood);
            p.Shape(Sd.Box(-0.55f, 1.2f, 0.07f, 0.95f), WoodDark, 0.035f);
            p.Shape(Sd.Box(0.55f, 1.2f, 0.07f, 0.95f), WoodDark, 0.035f);
            foreach (float y in new[] { 0.95f, 1.35f, 1.75f })
                foreach (float x in new[] { -0.55f, 0.55f }) p.Fill(Sd.Circle(x, y, 0.035f), Rgba.Hex("D8D8E0"));
            p.Shape(Sd.Box(0.1f, 2.0f, 0.45f, 0.13f, 0.03f), Rgba.Hex("FFF1C9"), 0.03f);
            p.Fill(Sd.Box(0.1f, 2.0f, 0.3f, 0.025f), Rgba.Hex("D03A3A"));
            return new DoodleArt("s41j_barricade", p);
        }

        /// <summary>Fiammata orizzontale (bocca in 0,0, lunga 2.6 verso destra).</summary>
        public static DoodleArt S41J_FireBreath()
        {
            var p = new DoodlePainter(-0.1f, -0.6f, 2.75f, 0.6f, 90) { Ink = 0.04f, Wobble = 0.04f };
            Sdf outer = Sd.Union(Sd.Polygon(0f, 0.08f, 2.0f, 0.5f, 2.65f, 0.1f, 2.6f, -0.25f, 2.0f, -0.5f, 0f, -0.08f), Sd.Circle(2.2f, 0f, 0.42f));
            p.Shape(outer, Rgba.Hex("FF6A2E"));
            p.Fill(Sd.Union(Sd.Polygon(0.1f, 0.04f, 1.9f, 0.3f, 2.4f, 0f, 1.9f, -0.3f, 0.1f, -0.04f), Sd.Circle(2.1f, 0f, 0.28f)), Rgba.Hex("FFB23A"));
            p.Fill(Sd.Union(Sd.Polygon(0.3f, 0.02f, 1.8f, 0.14f, 2.15f, 0f, 1.8f, -0.14f, 0.3f, -0.02f), Sd.Circle(1.95f, 0f, 0.13f)), Rgba.Hex("FFF3B0"));
            return new DoodleArt("s41j_fire_breath", p);
        }

        // ================= HIDDEN FREQUENCY =================

        public static DoodleArt S41J_FreqBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 1.6f, 3.3f, 3.6f), (x, y) => Rgba.Lerp(Rgba.Hex("4B5A78"), Rgba.Hex("6E7FA0"), S41J_Clamp((y + 2f) / 7f)));
            for (float y = -1.9f; y < 5.2f; y += 0.7f)
                for (float x = -3.2f + (((int)((y + 2f) / 0.7f)) % 2) * 0.5f; x < 3.3f; x += 1.0f)
                    p.Shape(Sd.Box(x, y, 0.47f, 0.32f, 0.04f), Rgba.Hex("5E6E8F"), 0.03f);
            p.FillFunc(Sd.Box(0, -3.6f, 3.3f, 1.6f), (x, y) => Rgba.Lerp(Rgba.Hex("2E3448"), Rgba.Hex("454C66"), S41J_Clamp((y + 5.2f) / 3.2f)));
            p.Shape(Sd.Box(0, -2.0f, 3.4f, 0.06f), Rgba.Hex("7A86A6"), 0.04f);
            // luce dall'alto
            p.Fill(Sd.Polygon(-1.2f, 5.2f, 1.2f, 5.2f, 3.0f, -2.0f, -3.0f, -2.0f), Rgba.Hex("FFF6D0", 0.08f));
            return new DoodleArt("s41j_freq_bg", p);
        }

        /// <summary>Cassaforte (centro 0,0): corpo ±2.4 x da -2.7 a 2.4, pannello luci in alto (y 1.85), porta sotto.</summary>
        public static DoodleArt S41J_FreqSafe(bool open)
        {
            var p = new DoodlePainter(-2.6f, -3.0f, 2.6f, 2.6f, 70) { Ink = 0.06f };
            Rgba steel = Rgba.Hex("9AA3B8"), steelD = Rgba.Hex("6E7790"), steelL = Rgba.Hex("C4CBDA");
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Box(s * 1.8f, -2.75f, 0.3f, 0.15f, 0.05f), steelD);
            p.Shape(Sd.Box(0, -0.15f, 2.4f, 2.55f, 0.2f), steelD);
            p.Shape(Sd.Box(0, -0.08f, 2.38f, 2.5f, 0.2f), steel);
            // pannello delle luci
            p.Shape(Sd.Box(0, 1.85f, 1.9f, 0.42f, 0.12f), Rgba.Hex("3A4258"), 0.04f);
            foreach (float x in new[] { -1.1f, 0f, 1.1f }) p.Shape(Sd.Circle(x, 1.85f, 0.33f), Rgba.Hex("2A3044"), 0.03f);
            Sdf door = Sd.Box(0, -0.75f, 2.05f, 1.75f, 0.14f);
            if (!open)
            {
                p.Shape(door, steelL);
                p.Fill(Sd.Box(0.08f, -0.83f, 1.95f, 1.65f, 0.12f), Rgba.Hex("B0B8CB"));
                p.Shape(Sd.Box(0, -0.75f, 2.05f, 1.75f, 0.14f), Rgba.Clear);
                foreach (float y in new[] { 0.5f, -2.0f }) p.Shape(Sd.Box(-2.12f, y, 0.12f, 0.3f, 0.04f), steelD, 0.035f);
                for (int i = 0; i < 10; i++)
                {
                    float a = i * 0.628f;
                    p.Fill(Sd.Circle(S41J_Cos(a) * 1.75f * 0.95f, -0.75f + S41J_Sin(a) * 1.5f, 0.05f), steelD);
                }
            }
            else
            {
                p.Shape(door, Rgba.Hex("1E2232"));
                p.Fill(Sd.Box(0, -2.1f, 1.95f, 0.35f), Rgba.Hex("2E3448"));
                // lingotti e monete
                foreach (float x in new[] { -1.1f, -0.2f, 0.7f })
                    p.Shape(Sd.Polygon(x - 0.42f, -2.2f, x + 0.42f, -2.2f, x + 0.3f, -1.85f, x - 0.3f, -1.85f), Gold, 0.04f);
                p.Shape(Sd.Polygon(-0.65f, -1.85f, 0.2f, -1.85f, 0.08f, -1.5f, -0.53f, -1.5f), Gold, 0.04f);
                foreach (float x in new[] { 1.3f, 1.55f }) p.Shape(Sd.Ellipse(x, -2.1f, 0.22f, 0.08f), GoldLight, 0.03f);
                p.Fill(Sd.Circle(-0.3f, -1.2f, 0.08f), Rgba.White);
                p.Fill(Sd.Circle(0.6f, -1.4f, 0.06f), Rgba.White);
                // porta aperta di lato
                p.Shape(Sd.Polygon(-2.2f, 1.0f, -2.75f, 1.3f, -2.75f, -2.8f, -2.2f, -2.5f), steelL);
            }
            p.Fill(Sd.Box(-1.8f, 2.25f, 0.4f, 0.05f, 0.05f), Rgba.White.WithAlpha(0.45f));
            return new DoodleArt(open ? "s41j_freq_safe_open" : "s41j_freq_safe", p);
        }

        /// <summary>Manopola senza numeri (raggio 0.95, centro 0,0) con tacca in alto.</summary>
        public static DoodleArt S41J_FreqDial()
        {
            var p = new DoodlePainter(-1.05f, -1.05f, 1.05f, 1.05f, 120) { Ink = 0.05f };
            p.Shape(Sd.Circle(0, 0, 0.95f), Rgba.Hex("3A4258"));
            for (int i = 0; i < 48; i++)
            {
                float a = i * (float)Math.PI * 2f / 48f, r1 = i % 4 == 0 ? 0.7f : 0.78f;
                p.Line(Rgba.Hex("C4CBDA", 0.8f), 0.025f, S41J_Cos(a) * r1, S41J_Sin(a) * r1, S41J_Cos(a) * 0.88f, S41J_Sin(a) * 0.88f);
            }
            Sdf knob = Sd.Circle(0, 0, 0.6f);
            for (int i = 0; i < 16; i++) knob = Sd.Union(knob, Sd.Circle(S41J_Cos(i * 0.3927f) * 0.6f, S41J_Sin(i * 0.3927f) * 0.6f, 0.06f));
            Body(p, knob, Rgba.Hex("D9DEE8"), Rgba.Hex("A3ABBE"));
            p.Shape(Sd.Circle(0, 0, 0.3f), Rgba.Hex("EEF1F6"), 0.035f);
            p.Shape(Sd.Polygon(-0.09f, 0.38f, 0.09f, 0.38f, 0f, 0.62f), Rgba.Hex("FF5A5A"), 0.03f);
            return new DoodleArt("s41j_freq_dial", p);
        }

        /// <summary>Piastra della serratura (1.1 x 0.8) con griglia d'ascolto.</summary>
        public static DoodleArt S41J_FreqLock()
        {
            var p = new DoodlePainter(-0.62f, -0.45f, 0.62f, 0.45f, 120) { Ink = 0.045f };
            p.Shape(Sd.Box(0, 0, 0.55f, 0.38f, 0.1f), Gold);
            p.Fill(Sd.Box(0.04f, -0.05f, 0.5f, 0.32f, 0.08f), GoldShade.WithAlpha(0.5f));
            for (float x = -0.35f; x < 0.4f; x += 0.14f) p.Shape(Sd.Box(x, 0.02f, 0.035f, 0.22f, 0.03f), Rgba.Hex("6A4A10"), 0.02f);
            return new DoodleArt("s41j_freq_lock", p);
        }

        /// <summary>Archetti di vibrazione ai lati della serratura (2.0 x 0.9).</summary>
        public static DoodleArt S41J_FreqWiggle()
        {
            var p = new DoodlePainter(-1.05f, -0.5f, 1.05f, 0.5f, 120) { Ink = 0.0f };
            Rgba c = Rgba.Hex("FFE36A");
            foreach (float s in new[] { -1f, 1f })
                for (int k = 0; k < 2; k++)
                {
                    float r = 0.72f + k * 0.18f;
                    for (int i = 0; i < 6; i++)
                    {
                        float a0 = -0.5f + i * 0.17f, a1 = a0 + 0.17f;
                        p.Line(c, 0.05f, s * S41J_Cos(a0) * r, S41J_Sin(a0) * r, s * S41J_Cos(a1) * r, S41J_Sin(a1) * r);
                    }
                }
            return new DoodleArt("s41j_freq_wiggle", p);
        }

        /// <summary>Lampadina bianca (raggio 0.28, si tinge).</summary>
        public static DoodleArt S41J_FreqLight()
        {
            var p = new DoodlePainter(-0.34f, -0.34f, 0.34f, 0.34f, 150) { Ink = 0.035f };
            p.Shape(Sd.Circle(0, 0, 0.28f), Rgba.White);
            p.Fill(Sd.Circle(-0.09f, 0.1f, 0.08f), Rgba.White.WithAlpha(0.9f));
            p.Fill(Sd.Crescent(Sd.Circle(0, 0, 0.27f), -0.05f, 0.06f), Rgba.Hex("000000", 0.15f));
            return new DoodleArt("s41j_freq_light", p);
        }

        // ================= THE CROOKED BRIDGE =================

        /// <summary>Burrone: scogliere a sinistra (fino a x -1.6) e destra (da x 1.6), cima in y -1.0.</summary>
        public static DoodleArt S41J_ChasmBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("FFD6B0"), Rgba.Hex("8AC6F2"), S41J_Clamp((y + 1f) / 6f)));
            p.Fill(Sd.Polygon(-3.3f, -1.0f, -2.2f, 0.4f, -1.0f, -0.4f, 0.2f, 0.9f, 1.4f, -0.3f, 2.4f, 0.5f, 3.3f, -0.2f, 3.3f, -1.0f), Rgba.Hex("C7B3E6"));
            foreach (float[] c in new[] { new[] { -1.6f, 2.4f }, new[] { 1.3f, 1.6f } })
                p.Shape(Sd.Union(Sd.Circle(c[0], c[1], 0.35f), Sd.Circle(c[0] + 0.35f, c[1] + 0.1f, 0.4f), Sd.Circle(c[0] + 0.7f, c[1], 0.3f)), Rgba.White, 0.04f);
            // fondo del burrone
            p.FillFunc(Sd.Box(0, -3.4f, 1.8f, 2.5f), (x, y) => Rgba.Lerp(Rgba.Hex("2A1E3A"), Rgba.Hex("6A5A8A"), S41J_Clamp((y + 5.2f) / 4.5f)));
            foreach (float s in new[] { -1f, 1f })
            {
                Sdf cliff = Sd.Polygon(s * 3.4f, -5.3f, s * 3.4f, -1.0f, s * 1.6f, -1.0f, s * 1.45f, -1.8f, s * 1.7f, -2.6f, s * 1.5f, -3.6f, s * 1.75f, -5.3f);
                p.FillFunc(cliff, (x, y) => Rgba.Lerp(Rgba.Hex("8A5A3A"), Rgba.Hex("B9805A"), S41J_Clamp((y + 5f) / 4f)));
                p.Shape(cliff, Rgba.Clear);
                p.Shape(Sd.Box(s * 2.5f, -1.05f, 0.92f, 0.12f, 0.06f), Rgba.Hex("7CC46A"), 0.04f);
                for (float y = -1.8f; y > -5f; y -= 0.8f) p.Line(Rgba.Hex("6E4A30", 0.6f), 0.04f, s * 2.0f, y, s * 2.6f, y - 0.25f);
                // paletti del ponte
                p.Shape(Sd.Box(s * 1.75f, -0.7f, 0.07f, 0.35f, 0.03f), WoodShade, 0.04f);
            }
            return new DoodleArt("s41j_chasm_bg", p);
        }

        /// <summary>Asse del ponte di corda (centro 0,0, lunga 3.2).</summary>
        public static DoodleArt S41J_BridgePlank()
        {
            var p = new DoodlePainter(-1.65f, -0.2f, 1.65f, 0.2f, 120) { Ink = 0.04f };
            for (float x = -1.5f; x < 1.55f; x += 0.25f) p.Shape(Sd.Box(x, -0.02f, 0.1f, 0.12f, 0.02f), (int)((x + 2f) / 0.25f) % 2 == 0 ? Wood : WoodShade, 0.03f);
            p.Line(Rgba.Hex("C9A46A"), 0.05f, -1.6f, 0.1f, 1.6f, 0.1f);
            p.Line(Rgba.Hex("C9A46A"), 0.05f, -1.6f, -0.13f, 1.6f, -0.13f);
            return new DoodleArt("s41j_bridge_plank", p);
        }

        /// <summary>Maniglia dell'estremità (anello con frecce su/giù, raggio 0.3).</summary>
        public static DoodleArt S41J_BridgeKnob()
        {
            var p = new DoodlePainter(-0.36f, -0.55f, 0.36f, 0.55f, 140) { Ink = 0.035f };
            p.Shape(Sd.Circle(0, 0, 0.26f), Rgba.Hex("FFB84A"));
            p.Fill(Sd.Circle(0, 0, 0.1f), Rgba.Hex("FFF1C9"));
            p.Shape(Sd.Polygon(-0.12f, 0.32f, 0.12f, 0.32f, 0f, 0.5f), Rgba.White, 0.03f);
            p.Shape(Sd.Polygon(-0.12f, -0.32f, 0.12f, -0.32f, 0f, -0.5f), Rgba.White, 0.03f);
            return new DoodleArt("s41j_bridge_knob", p);
        }

        // ================= THE FINAL WRONG ANSWER =================

        /// <summary>Studio del quiz: parete viola con lampadine, palco da y -2.4.</summary>
        public static DoodleArt S41J_QuizBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 1.4f, 3.3f, 3.8f), (x, y) => Rgba.Lerp(Rgba.Hex("5A3A8A"), Rgba.Hex("8A5AC2"), S41J_Clamp((y + 2.4f) / 7.5f)));
            for (float x = -3.0f; x < 3.3f; x += 1.0f) p.Fill(Sd.Box(x, 1.4f, 0.04f, 3.8f), Rgba.Hex("4A2E78", 0.5f));
            // raggi di luce dall'alto
            foreach (float x in new[] { -2.2f, 2.2f }) p.Fill(Sd.Polygon(x - 0.2f, 5.2f, x + 0.2f, 5.2f, x * 0.2f + 1.0f, -2.4f, x * 0.2f - 1.0f, -2.4f), Rgba.Hex("FFF1A8", 0.1f));
            // palco
            p.FillFunc(Sd.Box(0, -3.8f, 3.3f, 1.4f), (x, y) => Rgba.Lerp(Rgba.Hex("3A2450"), Rgba.Hex("6A3E8A"), S41J_Clamp((y + 5.2f) / 2.8f)));
            p.Shape(Sd.Box(0, -2.4f, 3.4f, 0.08f), Rgba.Hex("FFC93C"), 0.04f);
            for (float x = -3.0f; x < 3.2f; x += 0.5f) p.Shape(Sd.Circle(x, -2.4f, 0.06f), Rgba.Hex("FFF3B0"), 0.02f);
            p.Fill(Sd.Ellipse(0, -3.8f, 2.2f, 0.5f), Rgba.Hex("FFFFFF", 0.07f));
            return new DoodleArt("s41j_quiz_bg", p);
        }

        /// <summary>Porta del quiz (base in 0,0, larga 1.3, alta 2.3): chiusa o aperta (buio dentro).</summary>
        public static DoodleArt S41J_QuizDoor(bool open)
        {
            var p = new DoodlePainter(-0.85f, -0.08f, 0.85f, 2.45f, 110) { Ink = 0.05f };
            p.Shape(Sd.Box(0, 1.15f, 0.72f, 1.2f, 0.12f), Rgba.Hex("FFC93C"));
            Sdf hole = Sd.Box(0, 1.1f, 0.58f, 1.08f, 0.08f);
            if (!open)
            {
                p.Shape(hole, Rgba.Hex("FF7FA8"));
                p.Fill(Sd.Box(0.06f, 1.05f, 0.5f, 1.0f, 0.06f), Rgba.Hex("E85F8C"));
                p.Shape(Sd.Box(0, 1.6f, 0.42f, 0.34f, 0.08f), Rgba.White, 0.035f);   // targhetta della risposta
                p.Shape(Sd.Circle(0.38f, 0.95f, 0.07f), Gold, 0.03f);
            }
            else
            {
                p.FillFunc(hole, (x, y) => Rgba.Lerp(Rgba.Hex("120A1E"), Rgba.Hex("2E1E44"), S41J_Clamp(y / 2.2f)));
                p.Shape(hole, Rgba.Clear);
                p.Shape(Sd.Polygon(-0.58f, 0.02f, -0.85f, -0.05f, -0.85f, 2.3f, -0.58f, 2.18f), Rgba.Hex("FF7FA8"));
            }
            return new DoodleArt(open ? "s41j_quiz_door_open" : "s41j_quiz_door", p);
        }

        /// <summary>Cartello della domanda con lampadine (centro 0,0, 4.2 x 1.25).</summary>
        public static DoodleArt S41J_QuizSign()
        {
            var p = new DoodlePainter(-2.25f, -0.75f, 2.25f, 0.95f, 90) { Ink = 0.05f };
            p.InkLine(0.04f, -1.2f, 0.62f, 0f, 0.9f, 1.2f, 0.62f);
            p.Shape(Sd.Circle(0, 0.88f, 0.06f), Gold, 0.03f);
            p.Shape(Sd.Box(0, 0, 2.1f, 0.62f, 0.14f), Rgba.Hex("FFC93C"));
            p.Shape(Sd.Box(0, 0, 1.9f, 0.45f, 0.08f), Rgba.Hex("2A1B48"), 0.04f);
            for (int i = 0; i < 14; i++)
            {
                float x = -1.95f + i * 0.3f;
                p.Fill(Sd.Circle(x, 0.53f, 0.045f), Rgba.Hex("FFF3B0"));
                p.Fill(Sd.Circle(x, -0.53f, 0.045f), Rgba.Hex("FFF3B0"));
            }
            return new DoodleArt("s41j_quiz_sign", p);
        }

        /// <summary>Buco nel muro con luce e freccia d'uscita (centro 0,0, 1.6 x 1.1).</summary>
        public static DoodleArt S41J_WallHole()
        {
            var p = new DoodlePainter(-0.95f, -0.7f, 0.95f, 0.7f, 110) { Ink = 0.05f };
            Sdf hole = Sd.Polygon(-0.8f, -0.5f, -0.85f, 0.1f, -0.55f, 0.55f, 0.1f, 0.6f, 0.7f, 0.45f, 0.85f, -0.1f, 0.75f, -0.55f, 0.1f, -0.6f);
            p.Shape(hole, Rgba.Hex("FFF3C0"));
            p.FillFunc(Sd.Grow(hole, -0.08f), (x, y) => Rgba.Lerp(Rgba.Hex("FFF6D8"), Rgba.Hex("FFD978"), S41J_Clamp(-y + 0.3f)));
            p.Shape(Sd.Polygon(-0.3f, -0.08f, 0.15f, -0.08f, 0.15f, -0.22f, 0.42f, 0.02f, 0.15f, 0.26f, 0.15f, 0.12f, -0.3f, 0.12f), Rgba.Hex("6FCF6A"), 0.03f);
            foreach (float[] c in new[] { new[] { -0.75f, 0.45f }, new[] { 0.8f, -0.5f }, new[] { 0.7f, 0.55f } })
                p.Shape(Sd.Box(c[0], c[1], 0.12f, 0.07f, 0.02f), Rgba.Hex("B17ADA"), 0.025f);
            return new DoodleArt("s41j_wall_hole", p);
        }

        /// <summary>Grande X rossa (dietro la porta sbagliata).</summary>
        public static DoodleArt S41J_QuizX()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.5f, 0.5f, 120) { Ink = 0.04f };
            p.Shape(Sd.Union(Sd.Rotate(Sd.Box(0, 0, 0.4f, 0.1f, 0.05f), 0, 0, 45f), Sd.Rotate(Sd.Box(0, 0, 0.4f, 0.1f, 0.05f), 0, 0, -45f)), Rgba.Hex("FF4A4A"));
            return new DoodleArt("s41j_quiz_x", p);
        }

        // ================= THE IMPATIENT CLOCK =================

        public static DoodleArt S41J_ClockBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 1.0f, 3.3f, 4.3f), (x, y) => Rgba.Lerp(Rgba.Hex("FFDCA8"), Rgba.Hex("FFC98A"), S41J_Clamp((y + 3f) / 8f)));
            for (float x = -3.0f; x < 3.3f; x += 0.6f) p.Fill(Sd.Box(x, 1.0f, 0.12f, 4.3f), Rgba.Hex("FFE9C8", 0.5f));
            p.FillFunc(Sd.Box(0, -4.3f, 3.3f, 1.0f), (x, y) => Rgba.Lerp(Rgba.Hex("8A5A3A"), Rgba.Hex("A8693F"), S41J_Clamp((y + 5.2f) / 2f)));
            p.Shape(Sd.Box(0, -3.3f, 3.4f, 0.1f), Rgba.Hex("6E4A30"), 0.04f);
            // mensola con libri sotto l'orologio
            p.Shape(Sd.Box(0, -2.65f, 2.0f, 0.1f, 0.03f), Wood, 0.04f);
            foreach (float[] b in new[] { new[] { -1.6f, 0.3f, 0f }, new[] { -1.3f, 0.26f, 1f }, new[] { 1.5f, 0.32f, 2f } })
                p.Shape(Sd.Box(b[0], -2.55f + b[1], 0.12f, b[1], 0.02f), b[2] == 0f ? Rgba.Hex("6FA8E8") : b[2] == 1f ? Rgba.Hex("FF8A65") : Rgba.Hex("7CCB6A"), 0.035f);
            return new DoodleArt("s41j_clock_bg", p);
        }

        /// <summary>Sveglia gigante (centro del quadrante 0,0, raggio 2.25) con campanelle e piedini.</summary>
        public static DoodleArt S41J_ClockBody()
        {
            var p = new DoodlePainter(-2.6f, -2.75f, 2.6f, 2.95f, 70) { Ink = 0.07f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Capsule(s * 1.3f, -1.9f, s * 1.75f, -2.55f, 0.13f), Rgba.Hex("C93A3A"));
                Sdf bell = Sd.Rotate(Sd.Intersect(Sd.Circle(s * 1.55f, 2.05f, 0.62f), Sd.Box(s * 1.55f, 2.35f, 0.7f, 0.35f)), s * 1.55f, 2.05f, -s * 30f);
                p.Shape(bell, Gold);
            }
            p.Shape(Sd.Box(0, 2.45f, 0.1f, 0.3f), Rgba.Hex("C93A3A"), 0.05f);
            p.Shape(Sd.Circle(0, 2.75f, 0.14f), Gold, 0.05f);
            Body(p, Sd.Circle(0, 0, 2.25f), Rgba.Hex("E8473F"), Rgba.Hex("B92E2A"));
            p.Shape(Sd.Circle(0, 0, 1.95f), Rgba.Hex("FFF8E8"), 0.05f);
            for (int i = 0; i < 60; i++)
            {
                float a = i * (float)Math.PI / 30f, r0 = i % 5 == 0 ? 1.62f : 1.75f;
                if (i == 0) continue;
                p.Line(i % 5 == 0 ? Ink : Rgba.Hex("B9A88A"), i % 5 == 0 ? 0.06f : 0.025f, S41J_Sin(a) * r0, S41J_Cos(a) * r0, S41J_Sin(a) * 1.85f, S41J_Cos(a) * 1.85f);
            }
            p.Shape(Sd.Polygon(-0.13f, 1.86f, 0.13f, 1.86f, 0f, 1.62f), Rgba.Hex("FF4A4A"), 0.03f);
            p.Fill(Sd.Ellipse(-0.9f, 1.4f, 0.4f, 0.15f), Rgba.White.WithAlpha(0.5f));
            return new DoodleArt("s41j_clock_body", p);
        }

        /// <summary>Faccia sul quadrante (occhi in alto, bocca in basso, centro 0,0).</summary>
        public static DoodleArt S41J_ClockFace(bool happy)
        {
            var p = new DoodlePainter(-1.2f, -1.25f, 1.2f, 1.0f, 110) { Ink = 0.045f };
            if (happy)
            {
                foreach (float s in new[] { -1f, 1f }) p.InkLine(0.07f, s * 0.95f, 0.55f, s * 0.75f, 0.72f, s * 0.55f, 0.55f);
                p.Shape(Sd.Intersect(Sd.Circle(0, -0.62f, 0.45f), Sd.Box(0, -0.85f, 0.5f, 0.25f)), Rgba.Hex("7A2E3A"));
                p.Fill(Sd.Ellipse(0, -0.95f, 0.2f, 0.08f), Rgba.Hex("FF8FA8"));
                foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Circle(s * 1.0f, 0.2f, 0.16f), BlushColor.WithAlpha(0.55f));
            }
            else
            {
                S41J_Eyes(p, 0f, 0.55f, 0.75f, 0.24f, 0f, -0.3f);
                foreach (float s in new[] { -1f, 1f }) p.InkLine(0.08f, s * 1.08f, 0.95f, s * 0.45f, 0.75f);
                p.Shape(Sd.Ellipse(0, -0.75f, 0.42f, 0.28f), Rgba.Hex("7A2E3A"));
                p.Fill(Sd.Box(0, -0.55f, 0.3f, 0.06f), Rgba.White);
            }
            return new DoodleArt(happy ? "s41j_clock_face_happy" : "s41j_clock_face_angry", p);
        }

        /// <summary>Lancetta (perno in 0,0, punta in alto): ore corta e scura, minuti lunga e blu.</summary>
        public static DoodleArt S41J_ClockHand(bool minute)
        {
            float len = minute ? 1.9f : 1.25f, w = minute ? 0.09f : 0.14f;
            var p = new DoodlePainter(-0.25f, -0.3f, 0.25f, len + 0.12f, 140) { Ink = 0.035f };
            Rgba c = minute ? Rgba.Hex("3A7AD8") : Rgba.Hex("3A2C48");
            p.Shape(Sd.Union(Sd.Polygon(-w, 0f, w, 0f, w * 0.6f, len - 0.25f, 0f, len, -w * 0.6f, len - 0.25f), Sd.Capsule(0, -0.22f, 0, 0, w * 0.8f)), c);
            p.Shape(Sd.Circle(0, 0, 0.16f), Gold, 0.035f);
            return new DoodleArt(minute ? "s41j_clock_minute" : "s41j_clock_hour", p);
        }

        // ================= THE FROZEN WATERFALL =================

        /// <summary>Montagna innevata: cielo, scogliera a destra (cascata in x 1.2..2.5), terreno in y -3.3.</summary>
        public static DoodleArt S41J_FallsBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("D8F0FF"), Rgba.Hex("7FC3F0"), S41J_Clamp((y + 1f) / 6f)));
            p.Fill(Sd.Polygon(-3.3f, -1.5f, -2.0f, 0.6f, -1.2f, -0.4f, -0.2f, 1.0f, 0.8f, -1.5f), Rgba.Hex("B8D4EE"));
            p.Fill(Sd.Polygon(-2.35f, 0.05f, -2.0f, 0.6f, -1.65f, 0.05f), Rgba.White);
            // scogliera della cascata
            Sdf cliff = Sd.Polygon(0.95f, -3.3f, 0.95f, 2.2f, 1.4f, 2.6f, 3.4f, 2.8f, 3.4f, -3.3f);
            p.FillFunc(cliff, (x, y) => Rgba.Lerp(Rgba.Hex("6E7896"), Rgba.Hex("9AA6C2"), S41J_Clamp((y + 3f) / 6f)));
            p.Shape(cliff, Rgba.Clear);
            p.Shape(Sd.Union(Sd.Ellipse(1.6f, 2.55f, 0.8f, 0.2f), Sd.Ellipse(2.7f, 2.75f, 0.8f, 0.2f)), Rgba.White, 0.04f);
            // terreno innevato e pozza
            p.FillFunc(Sd.Box(0, -4.3f, 3.3f, 1.0f), (x, y) => Rgba.Lerp(Rgba.Hex("D6E6F6"), Rgba.White, S41J_Clamp((y + 5.2f) / 2f)));
            p.Shape(Sd.Box(0, -3.3f, 3.4f, 0.06f), Rgba.Hex("EAF4FF"), 0.04f);
            p.Shape(Sd.Ellipse(1.75f, -3.55f, 1.0f, 0.22f), Rgba.Hex("9FD2F0"), 0.04f);
            foreach (float x in new[] { -2.6f, -0.6f }) p.Shape(Sd.Polygon(x - 0.3f, -3.3f, x + 0.3f, -3.3f, x, -2.6f), Rgba.Hex("4E9A6E"), 0.04f);
            return new DoodleArt("s41j_falls_bg", p);
        }

        /// <summary>Cascata ghiacciata (centro 0,0, 1.4 x 5.0) con ghiaccioli.</summary>
        public static DoodleArt S41J_IceFall()
        {
            var p = new DoodlePainter(-0.8f, -2.6f, 0.8f, 2.6f, 70) { Ink = 0.05f };
            Sdf ice = Sd.Polygon(-0.6f, 2.5f, 0.6f, 2.5f, 0.7f, -1.0f, 0.6f, -2.5f, -0.65f, -2.5f, -0.72f, -0.5f);
            p.FillFunc(ice, (x, y) => Rgba.Lerp(Rgba.Hex("A8DEFF"), Rgba.Hex("E8F8FF"), S41J_Clamp((x + 0.7f) / 1.4f)));
            for (float x = -0.4f; x < 0.6f; x += 0.28f) p.Line(Rgba.White.WithAlpha(0.8f), 0.06f, x, 2.3f, x + 0.05f, -2.3f);
            p.Line(Rgba.Hex("7FB8E0", 0.7f), 0.03f, -0.2f, 0.8f, 0.1f, 0.3f, -0.1f, -0.2f);
            p.Shape(ice, Rgba.Clear);
            for (float x = -0.55f; x < 0.6f; x += 0.22f) p.Shape(Sd.Polygon(x - 0.08f, -2.45f, x + 0.08f, -2.45f, x, -2.58f + S41J_Sin(x * 9f) * 0.03f), Rgba.Hex("D8F2FF"), 0.02f);
            return new DoodleArt("s41j_ice_fall", p);
        }

        /// <summary>Cascata d'acqua che scorre (stessa forma della ghiacciata).</summary>
        public static DoodleArt S41J_WaterFall()
        {
            var p = new DoodlePainter(-0.8f, -2.6f, 0.8f, 2.6f, 70) { Ink = 0.05f };
            Sdf water = Sd.Polygon(-0.55f, 2.5f, 0.55f, 2.5f, 0.62f, -2.3f, -0.62f, -2.3f);
            p.FillFunc(water, (x, y) => Rgba.Lerp(Rgba.Hex("4FA8E8"), Rgba.Hex("8ED0FF"), S41J_Clamp((x + 0.6f) / 1.2f)));
            for (float x = -0.4f; x < 0.5f; x += 0.2f) p.Line(Rgba.White.WithAlpha(0.65f), 0.04f, x, 2.3f, x + 0.03f, 0.6f + x, x, -2.1f);
            p.Shape(water, Rgba.Clear);
            foreach (float x in new[] { -0.5f, 0f, 0.5f }) p.Shape(Sd.Circle(x, -2.35f, 0.18f), Rgba.White, 0.03f);
            return new DoodleArt("s41j_water_fall", p);
        }

        /// <summary>Lente vista di taglio in cornice d'ottone (centro 0,0; il raggio esce verso +x).</summary>
        public static DoodleArt S41J_Lens()
        {
            var p = new DoodlePainter(-0.4f, -0.6f, 0.4f, 0.6f, 150) { Ink = 0.04f };
            Sdf lens = Sd.Intersect(Sd.Circle(-0.55f, 0, 0.8f), Sd.Circle(0.55f, 0, 0.8f));
            p.Shape(Sd.Grow(lens, 0.06f), GoldShade);
            p.FillFunc(lens, (x, y) => Rgba.Lerp(Rgba.Hex("BFEAFF"), Rgba.Hex("F0FBFF"), S41J_Clamp(0.5f - x * 2f)));
            p.Fill(Sd.Ellipse(-0.06f, 0.2f, 0.04f, 0.14f), Rgba.White);
            p.Shape(Sd.Box(0, -0.53f, 0.12f, 0.05f, 0.02f), Gold, 0.03f);
            p.Shape(Sd.Box(0, 0.53f, 0.12f, 0.05f, 0.02f), Gold, 0.03f);
            return new DoodleArt("s41j_lens", p);
        }

        /// <summary>Anello di mira della lente (raggio 0.28).</summary>
        public static DoodleArt S41J_LensAim()
        {
            var p = new DoodlePainter(-0.36f, -0.36f, 0.36f, 0.36f, 150) { Ink = 0.035f };
            p.Shape(Sd.Subtract(Sd.Circle(0, 0, 0.28f), Sd.Circle(0, 0, 0.17f)), Rgba.Hex("FF7A3A"));
            p.Fill(Sd.Circle(0, 0, 0.17f), Rgba.Hex("FFFFFF", 0.35f));
            p.Fill(Sd.Circle(0, 0, 0.05f), Rgba.Hex("FF7A3A"));
            return new DoodleArt("s41j_lens_aim", p);
        }

        // ================= THE FAKE CHECKPOINT =================

        /// <summary>Videogioco a piattaforme: cielo, nuvole squadrate, terreno da y -2.0 con mattoni.</summary>
        public static DoodleArt S41J_CpBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 1.6f, 3.3f, 3.6f), (x, y) => Rgba.Lerp(Rgba.Hex("BDE9FF"), Rgba.Hex("6EC3F5"), S41J_Clamp((y + 2f) / 7f)));
            foreach (float[] c in new[] { new[] { -1.8f, 2.2f }, new[] { 1.4f, 1.3f } })
                p.Shape(Sd.Union(Sd.Box(c[0], c[1], 0.6f, 0.2f), Sd.Box(c[0] + 0.15f, c[1] + 0.25f, 0.3f, 0.2f)), Rgba.White, 0.05f);
            // colline
            p.Shape(Sd.Union(Sd.Circle(-2.0f, -2.2f, 1.4f), Sd.Circle(0.6f, -2.4f, 1.1f)), Rgba.Hex("7FD06A"), 0.05f);
            // terreno a mattoni
            p.Fill(Sd.Box(0, -3.6f, 3.3f, 1.6f), Rgba.Hex("C8743A"));
            for (int r = 0; r < 6; r++)
            {
                float y = -2.3f - r * 0.5f;
                p.Line(Rgba.Hex("8A4A20"), 0.04f, -3.3f, y, 3.3f, y);
                for (float x = -3.3f + (r % 2) * 0.4f; x < 3.3f; x += 0.8f) p.Line(Rgba.Hex("8A4A20"), 0.04f, x, y, x, y + 0.5f);
            }
            p.Shape(Sd.Box(0, -2.12f, 3.4f, 0.14f), Rgba.Hex("5FC34A"), 0.05f);
            // cartello START
            p.Shape(Sd.Box(-2.55f, -1.55f, 0.05f, 0.45f), WoodShade, 0.04f);
            p.Shape(Sd.Box(-2.55f, -1.25f, 0.32f, 0.17f, 0.03f), Wood, 0.04f);
            return new DoodleArt("s41j_cp_bg", p);
        }

        /// <summary>Palo del checkpoint (base in 0,0, cima in 0,1.9).</summary>
        public static DoodleArt S41J_CpPole()
        {
            var p = new DoodlePainter(-0.25f, -0.08f, 0.25f, 2.1f, 130) { Ink = 0.04f };
            p.Shape(Sd.Box(0, 0.08f, 0.2f, 0.08f, 0.03f), Rgba.Hex("6A6A7A"));
            p.Shape(Sd.Box(0, 1.0f, 0.045f, 0.95f), Rgba.Hex("D8D8E0"), 0.03f);
            p.Shape(Sd.Circle(0, 1.95f, 0.1f), Gold, 0.03f);
            return new DoodleArt("s41j_cp_pole", p);
        }

        /// <summary>Bandierina del checkpoint (attacco al palo in 0,0, telo verso destra e in giù).</summary>
        public static DoodleArt S41J_CpFlag()
        {
            var p = new DoodlePainter(-0.08f, -0.7f, 1.0f, 0.08f, 130) { Ink = 0.04f };
            Sdf cloth = Sd.Polygon(0f, 0f, 0.5f, -0.06f, 0.92f, 0.0f, 0.86f, -0.32f, 0.92f, -0.62f, 0.48f, -0.56f, 0f, -0.6f);
            p.FillFunc(cloth, (x, y) => Rgba.Lerp(Rgba.Hex("FF4A6A"), Rgba.Hex("FF8A5A"), S41J_Clamp(x)));
            p.Shape(cloth, Rgba.Clear);
            // stellina
            Sdf star = Sd.Polygon(0.45f, -0.12f, 0.5f, -0.25f, 0.64f, -0.26f, 0.53f, -0.35f, 0.57f, -0.49f, 0.45f, -0.4f, 0.33f, -0.49f, 0.37f, -0.35f, 0.26f, -0.26f, 0.4f, -0.25f);
            p.Shape(star, Rgba.Hex("FFE36A"), 0.025f);
            return new DoodleArt("s41j_cp_flag", p);
        }

        /// <summary>Porta del castello (base in 0,0, larga 1.1, alta 1.8).</summary>
        public static DoodleArt S41J_CpDoor(bool open)
        {
            var p = new DoodlePainter(-0.75f, -0.08f, 0.75f, 2.0f, 110) { Ink = 0.05f };
            Sdf frame = Sd.Union(Sd.Box(0, 0.6f, 0.65f, 0.6f), Sd.Circle(0, 1.2f, 0.65f));
            p.Shape(frame, Rgba.Hex("9AA0B0"));
            Sdf door = Sd.Union(Sd.Box(0, 0.55f, 0.48f, 0.55f), Sd.Circle(0, 1.1f, 0.48f));
            if (open)
            {
                p.FillFunc(door, (x, y) => Rgba.Lerp(Rgba.Hex("FFE9A0"), Rgba.Hex("FFF8DA"), S41J_Clamp(y / 1.6f)));
                p.Shape(door, Rgba.Clear);
                p.Fill(Sd.Circle(0, 0.9f, 0.3f), Rgba.White.WithAlpha(0.8f));
            }
            else
            {
                p.Shape(door, WoodShade);
                for (float x = -0.3f; x < 0.35f; x += 0.2f) p.Line(WoodDark.WithAlpha(0.7f), 0.03f, x, 0.05f, x, 1.45f);
                p.Shape(Sd.Circle(0.3f, 0.6f, 0.06f), Gold, 0.03f);
            }
            return new DoodleArt(open ? "s41j_cp_door_open" : "s41j_cp_door", p);
        }

        /// <summary>Eroe tondo col mantello e la spada di legno (piedi in 0,0, alto 1).</summary>
        public static DoodleArt S41J_CpHero()
        {
            var p = new DoodlePainter(-0.55f, -0.05f, 0.6f, 1.12f, 150) { Ink = 0.04f };
            p.Shape(Sd.Polygon(-0.2f, 0.75f, -0.5f, 0.15f, -0.05f, 0.3f), Rgba.Hex("E8433F"));
            foreach (float x in new[] { -0.13f, 0.13f }) p.Shape(Sd.Box(x, 0.08f, 0.09f, 0.08f, 0.04f), Rgba.Hex("6A4A30"), 0.03f);
            Body(p, Sd.Circle(0, 0.5f, 0.38f), Rgba.Hex("6FB8F0"), Rgba.Hex("4A90CF"));
            p.Shape(Sd.Box(0.48f, 0.65f, 0.04f, 0.3f, 0.02f), Rgba.Hex("D8D8E0"), 0.025f);
            p.Shape(Sd.Box(0.48f, 0.38f, 0.12f, 0.03f, 0.02f), Wood, 0.025f);
            p.Shape(Sd.Circle(0.36f, 0.42f, 0.08f), Rgba.Hex("FFD3A8"), 0.025f);
            p.Shape(Sd.Union(Sd.Box(0, 0.9f, 0.3f, 0.07f, 0.03f), Sd.Polygon(-0.25f, 0.9f, 0.25f, 0.9f, 0f, 1.1f)), Rgba.Hex("FFC93C"), 0.03f);
            S41J_Eyes(p, 0.08f, 0.58f, 0.12f, 0.08f, 0.6f, 0f);
            p.InkLine(0.025f, 0.02f, 0.38f, 0.1f, 0.34f, 0.2f, 0.38f);
            return new DoodleArt("s41j_cp_hero", p);
        }
    }
}
