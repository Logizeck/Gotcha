using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli della specifica 105-200, pacchetto G.</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV41G() => new DoodleArt[]
        {
            // comuni
            S41G_Button(),
            // Unfair race
            S41G_RaceBg(), S41G_Hare(), S41G_Snail(), S41G_Finish(),
            // The bottomless cup
            S41G_KitchenBg(), S41G_Tap(), S41G_TapHandle(), S41G_Glass(), S41G_Cork(),
            // The dirty secret
            S41G_BeachBg(), S41G_ExitButton(), S41G_FingerPrint(), S41G_Grease(), S41G_RoundSmudge(), S41G_HatchHole(),
            S41G_Cleaner(0), S41G_Cleaner(1), S41G_Cleaner(2), S41G_Cleaner(3), S41G_Rag(),
            // The fake virus
            S41G_DesktopBg(), S41G_Virus(false), S41G_Virus(true), S41G_Folder(), S41G_Bin(),
            // Slow motion
            S41G_LanesBg(), S41G_Cannon(), S41G_Ball(), S41G_Bumper(), S41G_Hourglass(),
            // The overworked robot
            S41G_LabBg(), S41G_Robot(false), S41G_Robot(true), S41G_RobotHand(), S41G_Switch(false), S41G_Switch(true), S41G_Pad(),
            // The missing letter
            S41G_GateBg(), S41G_GateHalf(), S41G_SignBoard(), S41G_Cloud(),
            // Anti-gravity taxi
            S41G_CityBg(), S41G_Taxi(), S41G_Platform(), S41G_Client(), S41G_GravBadge(), S41G_GravArrow(),
        };

        static float S41G_Sin(float v) => (float)Math.Sin(v);
        static float S41G_Cos(float v) => (float)Math.Cos(v);
        static float S41G_Clamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        /// <summary>Occhio tondo con pupilla e lucina.</summary>
        static void S41G_Eye(DoodlePainter p, float x, float y, float r, float lookX = 0f, float lookY = 0f)
        {
            p.Shape(Sd.Circle(x, y, r), Rgba.White, r * 0.3f);
            p.Fill(Sd.Circle(x + lookX * r * 0.4f, y + lookY * r * 0.4f, r * 0.52f), Ink);
            p.Fill(Sd.Circle(x + lookX * r * 0.4f - r * 0.18f, y + lookY * r * 0.4f + r * 0.2f, r * 0.17f), Rgba.White);
        }

        // ================= COMUNI =================

        /// <summary>Pulsante a pillola chiaro (si colora con SpriteRenderer.color): 1.6 x 0.7, centro 0,0.</summary>
        public static DoodleArt S41G_Button()
        {
            var p = new DoodlePainter(-0.86f, -0.43f, 0.86f, 0.4f, 120) { Ink = 0.045f };
            p.Shape(Sd.Box(0, -0.06f, 0.8f, 0.3f, 0.3f), Rgba.Hex("9C97A8"));
            p.Shape(Sd.Box(0, 0.02f, 0.8f, 0.3f, 0.3f), Rgba.Hex("F4F2F8"));
            p.Fill(Sd.Box(-0.2f, 0.17f, 0.45f, 0.05f, 0.05f), Rgba.White.WithAlpha(0.9f));
            return new DoodleArt("s41g_button", p);
        }

        // ================= UNFAIR RACE =================

        /// <summary>Pista: corsia della lepre a y 0.2, della lumaca a y -1.6 (alte 1.1), partenza a x -2.0.</summary>
        public static DoodleArt S41G_RaceBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("BFE8FF"), Rgba.Hex("7CC6F2"), S41G_Clamp((y - 1f) / 4f)));
            // colline e tribuna
            p.Shape(Sd.Union(Sd.Circle(-2.2f, 0.9f, 2.0f), Sd.Circle(1.6f, 0.6f, 2.4f)), Rgba.Hex("9ED37A"));
            p.Shape(Sd.Box(0.2f, 1.85f, 2.0f, 0.45f, 0.05f), Rgba.Hex("E9D6B4"));
            for (int i = 0; i < 9; i++)
            {
                float x = -1.55f + i * 0.44f;
                Rgba c = i % 3 == 0 ? Rgba.Hex("FF7A8A") : i % 3 == 1 ? Rgba.Hex("FFD15A") : Rgba.Hex("7FB8FF");
                p.Shape(Sd.Circle(x, 2.1f + (i % 2) * 0.05f, 0.14f), c, 0.03f);
                p.Shape(Sd.Circle(x, 1.72f, 0.12f), Rgba.Lerp(c, Rgba.White, 0.4f), 0.03f);
            }
            // bandierine
            p.Line(Ink, 0.03f, -3.2f, 2.85f, -1.6f, 2.6f, 0f, 2.85f, 1.6f, 2.6f, 3.2f, 2.85f);
            for (int i = 0; i < 14; i++)
            {
                float x = -3.0f + i * 0.45f, y = 2.85f - 0.25f * Math.Abs(S41G_Sin((x + 3.2f) / 1.6f * 1.5708f));
                Rgba c = i % 3 == 0 ? Rgba.Hex("FF6B6B") : i % 3 == 1 ? Rgba.Hex("FFD15A") : Rgba.Hex("6BCB77");
                p.Shape(Sd.Polygon(x - 0.13f, y, x + 0.13f, y, x, y - 0.3f), c, 0.025f);
            }
            // prato
            p.FillFunc(Sd.Box(0, -2.4f, 3.3f, 2.9f), (x, y) => Rgba.Lerp(Rgba.Hex("6FBF55"), Rgba.Hex("86D06A"), S41G_Clamp((y + 5f) / 5f)));
            p.Line(Ink, 0.05f, -3.3f, 1.22f, 3.3f, 1.22f);
            // corsie di terra
            foreach (float cy in new[] { 0.2f, -1.6f })
            {
                p.FillFunc(Sd.Box(0, cy, 3.3f, 0.55f), (x, y) => Rgba.Lerp(Rgba.Hex("D9A066"), Rgba.Hex("E8B47A"), 0.5f + 0.5f * S41G_Sin(x * 3f + y * 7f)));
                p.Line(Rgba.White, 0.06f, -3.3f, cy + 0.55f, 3.3f, cy + 0.55f);
                p.Line(Rgba.White, 0.06f, -3.3f, cy - 0.55f, 3.3f, cy - 0.55f);
                for (int k = 0; k < 10; k++) p.Fill(Sd.Circle(-2.9f + k * 0.63f, cy - 0.2f + (k % 3) * 0.15f, 0.04f), Rgba.Hex("B9824E"));
            }
            // linea di partenza a scacchi
            for (int j = 0; j < 14; j++)
                for (int i = 0; i < 2; i++)
                {
                    float y = -2.15f + j * 0.2f + 0.1f;
                    if (y > -1.05f && y < -0.35f) continue;
                    p.Fill(Sd.Box(-2.05f + i * 0.1f + 0.05f, y, 0.05f, 0.1f), (i + j) % 2 == 0 ? Rgba.White : Rgba.Hex("2B2233"), false);
                }
            // fiori nel prato
            for (int i = 0; i < 12; i++)
            {
                float x = -2.8f + (i * 1.37f) % 5.6f, y = -2.6f - (i * 0.71f) % 2.4f;
                if (Math.Abs(x) < 1.1f && y < -2.9f && y > -3.9f) continue;
                p.Shape(Sd.Circle(x, y, 0.08f), i % 2 == 0 ? Rgba.Hex("FFF3A8") : Rgba.Hex("FFB3C7"), 0.025f);
            }
            return new DoodleArt("s41g_race_bg", p);
        }

        /// <summary>Lepre che corre verso destra, spavalda (piedi in y 0).</summary>
        public static DoodleArt S41G_Hare()
        {
            var p = new DoodlePainter(-0.65f, -0.05f, 0.75f, 1.65f, 150) { Ink = 0.04f };
            Rgba fur = Rgba.Hex("C9A27E"), furL = Rgba.Hex("F2E3D2"), pink = Rgba.Hex("FF9BB0");
            // orecchie all'indietro
            p.Shape(Sd.Rotate(Sd.Ellipse(-0.05f, 1.35f, 0.12f, 0.36f), -0.05f, 1.1f, 35f), fur);
            p.Shape(Sd.Rotate(Sd.Ellipse(0.12f, 1.38f, 0.12f, 0.36f), 0.12f, 1.1f, 25f), fur);
            p.Fill(Sd.Rotate(Sd.Ellipse(0.12f, 1.4f, 0.05f, 0.25f), 0.12f, 1.1f, 25f), pink);
            // zampe dietro e coda
            p.Shape(Sd.Capsule(-0.3f, 0.45f, -0.5f, 0.1f, 0.1f), fur);
            p.Shape(Sd.Ellipse(-0.5f, 0.06f, 0.2f, 0.07f), furL);
            p.Shape(Sd.Circle(-0.45f, 0.62f, 0.12f), Rgba.White);
            // corpo inclinato in avanti
            p.Shape(Sd.Rotate(Sd.Ellipse(-0.05f, 0.62f, 0.38f, 0.26f), -0.05f, 0.62f, -15f), fur);
            p.Fill(Sd.Rotate(Sd.Ellipse(0.05f, 0.56f, 0.22f, 0.15f), 0.05f, 0.56f, -15f), furL);
            // pettorina col numero
            p.Shape(Sd.Box(0.02f, 0.66f, 0.13f, 0.11f, 0.02f), Rgba.White, 0.025f);
            p.Line(Rgba.Hex("E8443A"), 0.035f, -0.02f, 0.72f, 0.03f, 0.75f, 0.03f, 0.57f);
            // zampa davanti
            p.Shape(Sd.Capsule(0.2f, 0.45f, 0.42f, 0.12f, 0.08f), fur);
            p.Shape(Sd.Ellipse(0.46f, 0.08f, 0.15f, 0.06f), furL);
            p.Shape(Sd.Capsule(0.18f, 0.72f, 0.45f, 0.62f, 0.06f), fur);
            // testa
            p.Shape(Sd.Circle(0.33f, 1.0f, 0.26f), fur);
            p.Fill(Sd.Ellipse(0.47f, 0.93f, 0.14f, 0.1f), furL);
            // fascia da corsa
            p.Shape(Sd.Rotate(Sd.Box(0.3f, 1.16f, 0.27f, 0.05f), 0.3f, 1.16f, -8f), Rgba.Hex("E8443A"), 0.025f);
            // occhio mezzo chiuso (sicuro di sé)
            p.Shape(Sd.Circle(0.42f, 1.03f, 0.07f), Rgba.White, 0.025f);
            p.Fill(Sd.Circle(0.45f, 1.02f, 0.035f), Ink);
            p.Fill(Sd.Box(0.42f, 1.08f, 0.08f, 0.03f), fur);
            p.Line(Ink, 0.025f, 0.34f, 1.07f, 0.5f, 1.08f);
            p.Fill(Sd.Circle(0.6f, 0.95f, 0.04f), pink);
            p.Line(Ink, 0.025f, 0.44f, 0.86f, 0.52f, 0.84f, 0.58f, 0.88f);
            p.Shape(Sd.Box(0.5f, 0.82f, 0.03f, 0.03f), Rgba.White, 0.015f);
            return new DoodleArt("s41g_hare", p);
        }

        /// <summary>Lumaca determinata verso destra (base in y 0).</summary>
        public static DoodleArt S41G_Snail()
        {
            var p = new DoodlePainter(-0.55f, -0.05f, 0.6f, 0.95f, 150) { Ink = 0.04f };
            Rgba body = Rgba.Hex("B6E38A"), shell = Rgba.Hex("F2A65A");
            // corpo
            p.Shape(Sd.Union(Sd.Box(0.0f, 0.1f, 0.48f, 0.1f, 0.1f), Sd.Capsule(0.32f, 0.12f, 0.36f, 0.42f, 0.13f)), body);
            // antenne
            p.Line(Ink, 0.03f, 0.3f, 0.5f, 0.24f, 0.78f);
            p.Line(Ink, 0.03f, 0.42f, 0.5f, 0.5f, 0.8f);
            S41G_Eye(p, 0.24f, 0.8f, 0.075f, 0.6f, 0f);
            S41G_Eye(p, 0.51f, 0.82f, 0.075f, 0.6f, 0f);
            p.Line(Ink, 0.025f, 0.38f, 0.33f, 0.44f, 0.3f, 0.5f, 0.34f);
            p.Fill(Sd.Circle(0.47f, 0.4f, 0.035f), Rgba.Hex("FF9BB0"));
            // fascia
            p.Shape(Sd.Box(0.37f, 0.53f, 0.15f, 0.035f), Rgba.Hex("E8443A"), 0.02f);
            // guscio a spirale
            p.Shape(Sd.Circle(-0.12f, 0.42f, 0.33f), shell);
            p.Fill(Sd.Crescent(Sd.Circle(-0.12f, 0.42f, 0.3f), 0.06f, 0.06f), Rgba.Hex("D9822F"));
            var pts = new List<float>();
            for (float a = 0f; a < 12f; a += 0.25f)
            {
                float r = 0.27f * (1f - a / 13f);
                pts.Add(-0.12f + r * S41G_Cos(-a)); pts.Add(0.42f + r * S41G_Sin(-a));
            }
            p.Line(Ink, 0.03f, pts.ToArray());
            return new DoodleArt("s41g_snail", p);
        }

        /// <summary>Traguardo verticale a scacchi (alto 2.3, centro 0,0) con pomelli alle estremità.</summary>
        public static DoodleArt S41G_Finish()
        {
            var p = new DoodlePainter(-0.32f, -1.32f, 0.32f, 1.32f, 120) { Ink = 0.04f };
            p.Shape(Sd.Box(0, 0, 0.16f, 1.15f), Rgba.White);
            for (int j = 0; j < 23; j++)
                for (int i = 0; i < 3; i++)
                    if ((i + j) % 2 == 0) p.Fill(Sd.Box(-0.107f + i * 0.107f, -1.1f + j * 0.1f, 0.053f, 0.05f), Rgba.Hex("2B2233"), false);
            p.Shape(Sd.Box(0, 0, 0.16f, 1.15f), Rgba.Clear);
            foreach (float y in new[] { -1.18f, 1.18f }) p.Shape(Sd.Circle(0, y, 0.12f), Rgba.Hex("E8443A"), 0.035f);
            return new DoodleArt("s41g_finish", p);
        }

        // ================= THE BOTTOMLESS CUP =================

        /// <summary>Cucina: piastrelle, bancone con piano in y -2.9.</summary>
        public static DoodleArt S41G_KitchenBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.Fill(Sd.Box(0, 0, 3.3f, 5.3f), Rgba.Hex("E9F4F2"), false);
            for (float y = -2.9f; y < 5.3f; y += 0.6f) p.Line(Rgba.Hex("BFD9D4"), 0.03f, -3.3f, y, 3.3f, y);
            for (int r = 0; r < 14; r++)
                for (float x = -3.3f + (r % 2) * 0.3f; x < 3.3f; x += 0.6f)
                    p.Line(Rgba.Hex("BFD9D4"), 0.03f, x, -2.9f + r * 0.6f, x, -2.3f + r * 0.6f);
            // finestrella con vaso
            p.Shape(Sd.Box(-2.0f, 2.2f, 0.7f, 0.6f, 0.05f), Rgba.Hex("A8DBF7"));
            p.Line(Ink, 0.04f, -2.0f, 1.6f, -2.0f, 2.8f);
            p.Line(Ink, 0.04f, -2.7f, 2.2f, -1.3f, 2.2f);
            p.Shape(Sd.Box(2.1f, 2.4f, 0.55f, 0.08f), Wood, 0.035f);
            p.Shape(Sd.Box(1.85f, 2.68f, 0.14f, 0.2f, 0.04f), Rgba.Hex("FF8FA3"), 0.03f);
            p.Shape(Sd.Box(2.3f, 2.62f, 0.12f, 0.14f, 0.04f), Rgba.Hex("7FC8F8"), 0.03f);
            // bancone
            p.Shape(Sd.Box(0, -2.98f, 3.4f, 0.14f), Rgba.Hex("F4EEE3"));
            p.FillFunc(Sd.Box(0, -4.25f, 3.3f, 1.15f), (x, y) => Rgba.Lerp(WoodShade, Wood, S41G_Clamp((y + 5f) / 2.5f)));
            foreach (float x in new[] { -1.6f, 1.6f })
            {
                p.Shape(Sd.Box(x, -4.15f, 1.35f, 0.95f, 0.06f), Rgba.Hex("E39A55"), 0.04f);
                p.Shape(Sd.Box(x + (x < 0 ? 1.0f : -1.0f), -3.75f, 0.06f, 0.22f, 0.05f), Rgba.Hex("FFE6A8"), 0.03f);
            }
            return new DoodleArt("s41g_kitchen_bg", p);
        }

        /// <summary>Rubinetto dal muro: la bocca è in 0,0 (punta in giù), il tubo sale a destra.</summary>
        public static DoodleArt S41G_Tap()
        {
            var p = new DoodlePainter(-0.3f, -0.1f, 1.9f, 1.4f, 130) { Ink = 0.045f };
            Rgba steel = Rgba.Hex("C9D3DD"), steelD = Rgba.Hex("8E9BAA");
            p.Shape(Sd.Box(1.7f, 0.95f, 0.18f, 0.35f, 0.05f), steelD);
            p.Shape(Sd.Union(Sd.Capsule(1.6f, 0.95f, 0.2f, 0.95f, 0.15f), Sd.Capsule(0.0f, 0.95f, 0.0f, 0.2f, 0.15f), Sd.Circle(0.1f, 0.85f, 0.2f)), steel);
            p.Shape(Sd.Box(0, 0.08f, 0.2f, 0.1f, 0.04f), steelD);
            p.Line(Rgba.White.WithAlpha(0.8f), 0.05f, 0.35f, 1.02f, 1.4f, 1.02f);
            p.Line(Rgba.White.WithAlpha(0.8f), 0.04f, -0.06f, 0.75f, -0.06f, 0.3f);
            return new DoodleArt("s41g_tap", p);
        }

        /// <summary>Manopola rossa a croce del rubinetto (raggio 0.45, centro 0,0).</summary>
        public static DoodleArt S41G_TapHandle()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.5f, 0.5f, 130) { Ink = 0.045f };
            Sdf cross = Sd.Union(Sd.Capsule(-0.4f, 0, 0.4f, 0, 0.12f), Sd.Capsule(0, -0.4f, 0, 0.4f, 0.12f));
            p.Shape(cross, Rgba.Hex("E8443A"));
            p.Shape(Sd.Circle(0, 0, 0.17f), Rgba.Hex("FF8A7A"));
            p.Fill(Sd.Circle(-0.05f, 0.05f, 0.05f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("s41g_tap_handle", p);
        }

        /// <summary>Bicchiere trasparente (base in y 0, interno x ±0.72, y 0.12..2.1) col buco sul fondo e la tacca "pieno".</summary>
        public static DoodleArt S41G_Glass()
        {
            var p = new DoodlePainter(-0.95f, -0.12f, 0.95f, 2.3f, 120) { Ink = 0.05f };
            Sdf outer = Sd.Box(0, 1.1f, 0.85f, 1.1f, 0.12f);
            Sdf inner = Sd.Box(0, 1.15f, 0.72f, 1.05f, 0.08f);
            p.Shape(Sd.Subtract(outer, inner), Rgba.Hex("DFF3FF", 0.75f));
            p.Shape(inner, Rgba.Hex("FFFFFF", 0.12f), 0.025f);
            p.Line(Rgba.White.WithAlpha(0.85f), 0.07f, -0.55f, 1.9f, -0.55f, 0.45f);
            p.Line(Rgba.White.WithAlpha(0.6f), 0.04f, -0.4f, 1.9f, -0.4f, 1.4f);
            // tacca del livello pieno (85%)
            float full = 0.12f + 1.98f * 0.85f;
            for (float x = -0.7f; x < 0.7f; x += 0.22f) p.Line(Rgba.Hex("2E9E5B"), 0.04f, x, full, x + 0.12f, full);
            // buco sul fondo con crepe
            p.Shape(Sd.Ellipse(0, 0.07f, 0.13f, 0.07f), Rgba.Hex("2B2233"), 0.03f);
            p.Line(Ink, 0.02f, 0.13f, 0.08f, 0.26f, 0.13f, 0.32f, 0.08f);
            p.Line(Ink, 0.02f, -0.13f, 0.06f, -0.25f, 0.12f);
            return new DoodleArt("s41g_glass", p);
        }

        /// <summary>Tappo di sughero (centro 0,0).</summary>
        public static DoodleArt S41G_Cork()
        {
            var p = new DoodlePainter(-0.25f, -0.2f, 0.25f, 0.2f, 160) { Ink = 0.035f };
            p.Shape(Sd.Polygon(-0.2f, 0.14f, 0.2f, 0.14f, 0.14f, -0.14f, -0.14f, -0.14f), Rgba.Hex("D9A066"));
            for (int i = 0; i < 6; i++) p.Fill(Sd.Circle(-0.1f + (i * 0.073f) % 0.2f, -0.06f + (i % 3) * 0.07f, 0.018f), Rgba.Hex("A8743F"));
            return new DoodleArt("s41g_cork", p);
        }

        // ================= THE DIRTY SECRET =================

        /// <summary>Lo "schermo" pulito: spiaggia al sole con palma.</summary>
        public static DoodleArt S41G_BeachBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("FFE2B8"), Rgba.Hex("7CC6F2"), S41G_Clamp((y + 0.5f) / 4.5f)));
            p.Shape(Sd.Circle(1.6f, 2.0f, 0.65f), Rgba.Hex("FFD45A"));
            p.Shape(Sd.Union(Sd.Circle(-1.8f, 2.6f, 0.4f), Sd.Circle(-1.35f, 2.75f, 0.45f), Sd.Circle(-0.9f, 2.55f, 0.35f)), Rgba.White);
            // mare
            p.FillFunc(Sd.Box(0, -0.9f, 3.3f, 1.0f), (x, y) => Rgba.Lerp(Rgba.Hex("2E9ED6"), Rgba.Hex("5BC2EB"), S41G_Clamp((y + 1.9f) / 2f)));
            p.Line(Ink, 0.05f, -3.3f, 0.1f, 3.3f, 0.1f);
            for (int i = 0; i < 8; i++) p.Line(Rgba.White.WithAlpha(0.7f), 0.04f, -2.8f + i * 0.75f, -0.4f - (i % 3) * 0.35f, -2.55f + i * 0.75f, -0.35f - (i % 3) * 0.35f);
            // barchetta
            p.Shape(Sd.Polygon(-2.0f, -0.25f, -1.2f, -0.25f, -1.35f, -0.5f, -1.85f, -0.5f), Rgba.Hex("E8443A"), 0.035f);
            p.Shape(Sd.Polygon(-1.62f, -0.2f, -1.62f, 0.55f, -1.25f, -0.2f), Rgba.White, 0.03f);
            // sabbia
            p.FillFunc(Sd.Union(Sd.Box(0, -3.8f, 3.3f, 1.5f), Sd.Circle(0.8f, -4.0f, 2.4f)), (x, y) => Rgba.Lerp(Rgba.Hex("F2CF8A"), Rgba.Hex("FFE3A8"), S41G_Clamp((y + 5f) / 3f)));
            // palma
            p.Shape(Sd.Polyline(0.13f, 1.9f, -2.9f, 2.0f, -1.8f, 1.8f, -0.6f, 1.5f, 0.3f), Rgba.Hex("B9824E"));
            foreach (float a in new[] { 20f, 70f, 120f, 160f, -20f })
            {
                p.Shape(Sd.Rotate(Sd.Ellipse(1.5f + 0.6f, 0.3f, 0.65f, 0.16f), 1.5f, 0.3f, a), Leaf);
            }
            p.Shape(Sd.Circle(1.45f, 0.15f, 0.12f), Rgba.Hex("8A5A3B"), 0.03f);
            p.Shape(Sd.Circle(1.62f, 0.12f, 0.12f), Rgba.Hex("8A5A3B"), 0.03f);
            // ombrellone e paperella
            p.Shape(Sd.Capsule(-1.4f, -3.9f, -1.2f, -2.2f, 0.04f), Rgba.Hex("F4F2F8"), 0.03f);
            p.Shape(Sd.Intersect(Sd.Circle(-1.2f, -2.4f, 0.95f), Sd.Box(-1.2f, -1.8f, 1.0f, 0.6f)), Rgba.Hex("FF7A8A"));
            p.Shape(Sd.Intersect(Sd.Circle(-1.2f, -2.4f, 0.95f), Sd.Box(-1.2f, -1.8f, 0.3f, 0.6f)), Rgba.White);
            p.Shape(Sd.Union(Sd.Ellipse(0.9f, -3.4f, 0.32f, 0.2f), Sd.Circle(1.08f, -3.15f, 0.16f)), Rgba.Hex("FFD45A"));
            p.Shape(Sd.Polygon(1.22f, -3.12f, 1.4f, -3.18f, 1.22f, -3.24f), Rgba.Hex("FF8A1F"), 0.025f);
            p.Fill(Sd.Circle(1.1f, -3.1f, 0.03f), Ink);
            return new DoodleArt("s41g_beach_bg", p);
        }

        /// <summary>Pulsante verde "uscita" con porticina (1.6 x 0.9, centro 0,0).</summary>
        public static DoodleArt S41G_ExitButton()
        {
            var p = new DoodlePainter(-0.88f, -0.52f, 0.88f, 0.5f, 120) { Ink = 0.05f };
            p.Shape(Sd.Box(0, -0.06f, 0.8f, 0.4f, 0.16f), Rgba.Hex("2E8B4E"));
            p.Shape(Sd.Box(0, 0.02f, 0.8f, 0.4f, 0.16f), Rgba.Hex("5BCB77"));
            p.Shape(Sd.Box(-0.5f, 0.02f, 0.17f, 0.27f, 0.03f), Rgba.Hex("FFF3D6"), 0.03f);
            p.Fill(Sd.Circle(-0.42f, 0.0f, 0.03f), Ink);
            p.Fill(Sd.Box(-0.1f, 0.3f, 0.6f, 0.04f, 0.04f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("s41g_exit_btn", p);
        }

        /// <summary>Ditata unta (ellisse con le creste dell'impronta), semitrasparente.</summary>
        public static DoodleArt S41G_FingerPrint()
        {
            var p = new DoodlePainter(-0.32f, -0.4f, 0.32f, 0.4f, 100) { Ink = 0.02f, Wobble = 0.006f };
            Rgba smear = Rgba.Hex("8C7A5C", 0.5f);
            p.Fill(Sd.Ellipse(0, 0, 0.27f, 0.36f), Rgba.Hex("B8A57E", 0.45f));
            for (int k = 1; k < 6; k++)
                p.Fill(Sd.Subtract(Sd.Ellipse(0, -0.02f * k, 0.05f * k, 0.066f * k), Sd.Ellipse(0, -0.02f * k, 0.05f * k - 0.02f, 0.066f * k - 0.02f)), smear);
            return new DoodleArt("s41g_fprint", p);
        }

        /// <summary>Macchia d'unto (centro 0,0, ~0.5).</summary>
        public static DoodleArt S41G_Grease()
        {
            var p = new DoodlePainter(-0.36f, -0.36f, 0.36f, 0.36f, 100) { Ink = 0.02f, Wobble = 0.03f, WobbleFreq = 6f };
            p.Fill(Sd.Union(Sd.Circle(0, 0, 0.24f), Sd.Circle(0.14f, 0.12f, 0.15f), Sd.Circle(-0.16f, -0.1f, 0.14f)), Rgba.Hex("A58B55", 0.55f));
            p.Fill(Sd.Circle(-0.05f, 0.05f, 0.12f), Rgba.Hex("7D6638", 0.35f));
            p.Fill(Sd.Circle(0.08f, 0.1f, 0.04f), Rgba.White.WithAlpha(0.35f));
            return new DoodleArt("s41g_grease", p);
        }

        /// <summary>La macchia tonda sospetta nell'angolo (raggio 0.42): a ben guardare ha un bordo da botola.</summary>
        public static DoodleArt S41G_RoundSmudge()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.5f, 0.5f, 120) { Ink = 0.02f, Wobble = 0.004f };
            p.Fill(Sd.Circle(0, 0, 0.42f), Rgba.Hex("9C8A66", 0.75f));
            p.Fill(Sd.Subtract(Sd.Circle(0, 0, 0.4f), Sd.Circle(0, 0, 0.36f)), Rgba.Hex("6E5E40", 0.6f));
            p.Fill(Sd.Circle(0.0f, -0.24f, 0.05f), Rgba.Hex("6E5E40", 0.6f));
            p.Fill(Sd.Circle(-0.12f, 0.12f, 0.1f), Rgba.Hex("C9B68C", 0.5f));
            return new DoodleArt("s41g_round_smudge", p);
        }

        /// <summary>Buco buio sotto la botola, con la cima di una scaletta.</summary>
        public static DoodleArt S41G_HatchHole()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.5f, 0.5f, 120) { Ink = 0.04f };
            p.Shape(Sd.Circle(0, 0, 0.42f), Rgba.Hex("1E1A26"));
            p.Line(Rgba.Hex("8E9BAA"), 0.04f, -0.15f, -0.35f, -0.15f, 0.2f);
            p.Line(Rgba.Hex("8E9BAA"), 0.04f, 0.15f, -0.35f, 0.15f, 0.2f);
            for (float y = -0.25f; y < 0.2f; y += 0.15f) p.Line(Rgba.Hex("8E9BAA"), 0.035f, -0.15f, y, 0.15f, y);
            return new DoodleArt("s41g_hatch_hole", p);
        }

        /// <summary>
        /// Omino delle pulizie in mutande (piedi in y 0). Pose: 0 sporca col panno, 1 fischietta innocente,
        /// 2 pernacchia, 3 seduto sulla sedia da ufficio a rotelle.
        /// </summary>
        public static DoodleArt S41G_Cleaner(int pose)
        {
            var p = new DoodlePainter(-0.75f, -0.05f, 0.75f, 1.75f, 140) { Ink = 0.04f };
            Rgba skin = Skin, pants = Rgba.Hex("7FB8FF"), dots = Rgba.Hex("FFFFFF");
            float lift = pose == 3 ? 0.45f : 0f;
            if (pose == 3)
            {
                // sedia: rotelle, stelo, seduta, schienale
                foreach (float x in new[] { -0.4f, 0f, 0.4f }) p.Shape(Sd.Circle(x, 0.08f, 0.07f), Rgba.Hex("3A3540"), 0.025f);
                p.Shape(Sd.Capsule(-0.4f, 0.15f, 0.4f, 0.15f, 0.04f), Rgba.Hex("6B6675"), 0.025f);
                p.Shape(Sd.Capsule(0, 0.15f, 0, 0.45f, 0.05f), Rgba.Hex("6B6675"), 0.025f);
                p.Shape(Sd.Box(-0.42f, 0.95f, 0.08f, 0.45f, 0.06f), Rgba.Hex("3A3540"));
            }
            // gambe
            if (pose == 3)
            {
                p.Shape(Sd.Capsule(-0.1f, 0.6f, 0.5f, 0.75f, 0.07f), skin);
                p.Shape(Sd.Capsule(0.05f, 0.58f, 0.58f, 0.62f, 0.07f), skin);
            }
            else
            {
                p.Shape(Sd.Capsule(-0.12f, 0.1f, -0.1f, 0.5f, 0.07f), skin);
                p.Shape(Sd.Capsule(0.12f, 0.1f, 0.1f, 0.5f, 0.07f), skin);
                p.Shape(Sd.Ellipse(-0.16f, 0.06f, 0.11f, 0.05f), Rgba.Hex("3A3540"));
                p.Shape(Sd.Ellipse(0.16f, 0.06f, 0.11f, 0.05f), Rgba.Hex("3A3540"));
            }
            // pancia
            p.Shape(Sd.Ellipse(0, 0.82f + lift * 0.1f, 0.3f, 0.34f), skin);
            p.Fill(Sd.Circle(0.02f, 0.82f + lift * 0.1f, 0.025f), Ink);
            // mutande a pois
            Sdf briefs = Sd.Intersect(Sd.Ellipse(0, 0.82f + lift * 0.1f, 0.3f, 0.34f), Sd.Box(0, 0.5f + lift * 0.1f, 0.4f, 0.17f));
            p.Shape(briefs, pants, 0.035f);
            for (int i = 0; i < 4; i++) p.Fill(Sd.Circle(-0.15f + i * 0.1f, 0.58f + lift * 0.1f - (i % 2) * 0.06f, 0.025f), dots);
            float hy = 1.32f + lift * 0.1f;
            // braccia secondo la posa
            switch (pose)
            {
                case 0:
                    p.Shape(Sd.Capsule(0.22f, 1.0f, 0.55f, 1.35f, 0.06f), skin);
                    p.Shape(Sd.Capsule(-0.22f, 1.0f, -0.42f, 0.75f, 0.06f), skin);
                    p.Shape(Sd.Union(Sd.Circle(0.58f, 1.42f, 0.13f), Sd.Circle(0.66f, 1.3f, 0.1f)), Rgba.Hex("B8A57E"));
                    break;
                case 1:
                    p.Shape(Sd.Capsule(0.22f, 1.0f, 0.1f, 0.72f, 0.06f), skin);
                    p.Shape(Sd.Capsule(-0.22f, 1.0f, -0.1f, 0.72f, 0.06f), skin);
                    break;
                case 2:
                    p.Shape(Sd.Capsule(0.22f, 1.02f, 0.38f, hy + 0.05f, 0.06f), skin);
                    p.Shape(Sd.Capsule(-0.22f, 1.02f, -0.38f, hy + 0.05f, 0.06f), skin);
                    foreach (float s in new[] { -1f, 1f })
                        for (int k = 0; k < 3; k++) p.Shape(Sd.Capsule(s * 0.38f, hy + 0.05f, s * (0.42f + k * 0.04f), hy + 0.2f + k * 0.03f, 0.03f), skin, 0.02f);
                    break;
                default:
                    p.Shape(Sd.Capsule(0.22f, 1.05f, 0.5f, 1.25f, 0.06f), skin);
                    p.Shape(Sd.Capsule(-0.22f, 1.05f, -0.45f, 1.3f, 0.06f), skin);
                    break;
            }
            // testa con pochi capelli e baffi
            p.Shape(Sd.Circle(0, hy, 0.24f), skin);
            p.Line(Ink, 0.025f, -0.05f, hy + 0.24f, -0.1f, hy + 0.33f);
            p.Line(Ink, 0.025f, 0.03f, hy + 0.24f, 0.05f, hy + 0.34f);
            float look = pose == 0 ? 0.5f : pose == 1 ? -0.7f : 0f;
            S41G_Eye(p, -0.08f, hy + 0.05f, 0.065f, look, pose == 1 ? 0.6f : 0f);
            S41G_Eye(p, 0.08f, hy + 0.05f, 0.065f, look, pose == 1 ? 0.6f : 0f);
            p.Shape(Sd.Union(Sd.Ellipse(-0.06f, hy - 0.08f, 0.07f, 0.03f), Sd.Ellipse(0.06f, hy - 0.08f, 0.07f, 0.03f)), Rgba.Hex("6B4A2E"), 0.02f);
            if (pose == 1)
            {
                p.Shape(Sd.Circle(0.02f, hy - 0.15f, 0.03f), Rgba.Hex("2B2233"), 0.015f);
                // notine del fischio
                p.Line(Ink, 0.025f, 0.3f, hy + 0.1f, 0.3f, hy + 0.28f, 0.38f, hy + 0.25f);
                p.Fill(Sd.Ellipse(0.27f, hy + 0.1f, 0.05f, 0.035f), Ink);
            }
            else if (pose == 2)
            {
                p.Shape(Sd.Ellipse(0, hy - 0.16f, 0.06f, 0.09f), Rgba.Hex("FF7F9C"), 0.025f);
                p.Fill(Sd.Circle(-0.18f, hy - 0.05f, 0.04f), BlushColor.WithAlpha(0.6f));
                p.Fill(Sd.Circle(0.18f, hy - 0.05f, 0.04f), BlushColor.WithAlpha(0.6f));
            }
            else if (pose == 3) p.Line(Ink, 0.025f, -0.06f, hy - 0.15f, 0.0f, hy - 0.18f, 0.07f, hy - 0.14f);
            else p.Line(Ink, 0.025f, -0.06f, hy - 0.15f, 0.07f, hy - 0.15f);
            return new DoodleArt("s41g_cleaner_" + pose, p);
        }

        /// <summary>Straccio sporco (centro 0,0).</summary>
        public static DoodleArt S41G_Rag()
        {
            var p = new DoodlePainter(-0.3f, -0.25f, 0.3f, 0.25f, 140) { Ink = 0.035f };
            p.Shape(Sd.Polygon(-0.24f, 0.12f, -0.05f, 0.2f, 0.2f, 0.14f, 0.24f, -0.1f, 0.05f, -0.2f, -0.2f, -0.14f), Rgba.Hex("B8A57E"));
            p.Fill(Sd.Circle(0.05f, 0.0f, 0.08f), Rgba.Hex("7D6638", 0.5f));
            return new DoodleArt("s41g_rag", p);
        }

        // ================= THE FAKE VIRUS =================

        /// <summary>Monitor da cartone: schermo x ±2.6, y -3.25..2.75 con sfondo a onde, barra in basso.</summary>
        public static DoodleArt S41G_DesktopBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.Fill(Sd.Box(0, 0, 3.3f, 5.3f), Rgba.Hex("3B3550"), false);
            p.Shape(Sd.Box(0, -0.25f, 2.95f, 3.75f, 0.25f), Rgba.Hex("E9E2D2"));
            p.FillFunc(Sd.Box(0, -0.25f, 2.62f, 3.02f, 0.08f), (x, y) =>
            {
                float w = 0.5f + 0.5f * S41G_Sin(x * 1.3f + y * 0.6f);
                return Rgba.Lerp(Rgba.Hex("2FA6A0"), Rgba.Hex("54C7B9"), w * S41G_Clamp((y + 3.3f) / 6f) + 0.15f);
            });
            p.Shape(Sd.Box(0, -0.25f, 2.62f, 3.02f, 0.08f), Rgba.Clear, 0.05f);
            // barra delle applicazioni
            p.Shape(Sd.Box(0, -2.98f, 2.6f, 0.27f), Rgba.Hex("D8D0E8"), 0.04f);
            p.Shape(Sd.Box(-2.2f, -2.98f, 0.32f, 0.18f, 0.06f), Rgba.Hex("7BD389"), 0.03f);
            for (int i = 0; i < 3; i++) p.Shape(Sd.Box(-1.5f + i * 0.45f, -2.98f, 0.17f, 0.15f, 0.04f), Rgba.Hex("F4F2F8"), 0.025f);
            // lucina e piedistallo
            p.Shape(Sd.Circle(2.4f, -3.75f, 0.07f), Rgba.Hex("7BFF8A"), 0.03f);
            p.Shape(Sd.Polygon(-0.9f, -4.0f, 0.9f, -4.0f, 1.4f, -4.9f, -1.4f, -4.9f), Rgba.Hex("D2C9B6"));
            return new DoodleArt("s41g_desktop_bg", p);
        }

        /// <summary>Creaturina-virus viola con antenne e zampette (centro 0,0); variante che starnutisce.</summary>
        public static DoodleArt S41G_Virus(bool sneeze)
        {
            var p = new DoodlePainter(-0.48f, -0.45f, 0.48f, 0.55f, 150) { Ink = 0.04f };
            Rgba body = Rgba.Hex("B27BFF"), dark = Rgba.Hex("7E4FD6");
            foreach (float s in new[] { -1f, 1f })
            {
                p.Line(Ink, 0.03f, s * 0.12f, 0.25f, s * 0.2f, 0.45f);
                p.Shape(Sd.Circle(s * 0.21f, 0.46f, 0.06f), Rgba.Hex("7BFF8A"), 0.025f);
                p.Shape(Sd.Capsule(s * 0.15f, -0.25f, s * 0.2f, -0.38f, 0.045f), dark, 0.025f);
            }
            Sdf b = sneeze ? Sd.Ellipse(0, 0, 0.4f, 0.3f) : Sd.Circle(0, 0, 0.3f);
            p.Shape(b, body);
            p.Fill(Sd.Crescent(b, 0.06f, 0.06f), dark.WithAlpha(0.6f));
            foreach (var d in new[] { (-0.18f, -0.12f), (0.2f, 0.12f), (0.1f, -0.18f) }) p.Fill(Sd.Circle(d.Item1, d.Item2, 0.035f), dark);
            if (sneeze)
            {
                p.Line(Ink, 0.03f, -0.16f, 0.07f, -0.08f, 0.04f, -0.16f, 0.01f);
                p.Line(Ink, 0.03f, 0.16f, 0.07f, 0.08f, 0.04f, 0.16f, 0.01f);
                p.Shape(Sd.Ellipse(0, -0.11f, 0.08f, 0.07f), Rgba.Hex("5A2E8A"), 0.025f);
                p.Fill(Sd.Circle(-0.24f, -0.05f, 0.05f), BlushColor.WithAlpha(0.6f));
                p.Fill(Sd.Circle(0.24f, -0.05f, 0.05f), BlushColor.WithAlpha(0.6f));
            }
            else
            {
                S41G_Eye(p, -0.1f, 0.06f, 0.08f, 0.3f, 0f);
                S41G_Eye(p, 0.1f, 0.06f, 0.08f, 0.3f, 0f);
                p.Line(Ink, 0.025f, -0.08f, -0.1f, 0f, -0.14f, 0.08f, -0.1f);
            }
            return new DoodleArt(sneeze ? "s41g_virus_sneeze" : "s41g_virus", p);
        }

        /// <summary>Icona cartella gialla (centro 0,0, ~0.7).</summary>
        public static DoodleArt S41G_Folder()
        {
            var p = new DoodlePainter(-0.4f, -0.32f, 0.4f, 0.32f, 130) { Ink = 0.035f };
            p.Shape(Sd.Union(Sd.Box(0, -0.04f, 0.34f, 0.24f, 0.04f), Sd.Box(-0.18f, 0.2f, 0.14f, 0.06f, 0.03f)), Rgba.Hex("F2B53A"));
            p.Shape(Sd.Box(0, -0.08f, 0.34f, 0.2f, 0.04f), Rgba.Hex("FFD45A"));
            return new DoodleArt("s41g_folder", p);
        }

        /// <summary>Cestino con coperchio a bocca (centro 0,0; alto ~1.1).</summary>
        public static DoodleArt S41G_Bin()
        {
            var p = new DoodlePainter(-0.55f, -0.58f, 0.55f, 0.6f, 130) { Ink = 0.045f };
            Rgba c = Rgba.Hex("9FB4C7"), d = Rgba.Hex("7088A0");
            p.Shape(Sd.Polygon(-0.4f, 0.3f, 0.4f, 0.3f, 0.32f, -0.52f, -0.32f, -0.52f), c);
            for (int i = -1; i <= 1; i++) p.Line(d, 0.04f, i * 0.17f, 0.18f, i * 0.14f, -0.42f);
            p.Shape(Sd.Box(0, 0.37f, 0.48f, 0.07f, 0.04f), d);
            p.Shape(Sd.Box(0, 0.48f, 0.12f, 0.05f, 0.03f), d);
            // simbolo riciclo semplice
            p.Shape(Sd.Circle(0, -0.08f, 0.14f), Rgba.Hex("7BD389"), 0.03f);
            p.Line(Rgba.White, 0.03f, -0.06f, -0.12f, 0f, -0.02f, 0.06f, -0.12f, -0.06f, -0.12f);
            return new DoodleArt("s41g_bin", p);
        }

        // ================= SLOW MOTION =================

        /// <summary>Sala giochi: 4 binari a y 1.2, 0, -1.2, -2.4 da x -2.0 a 2.35; fossa a destra.</summary>
        public static DoodleArt S41G_LanesBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("2A2147"), Rgba.Hex("473A78"), S41G_Clamp((y + 5f) / 10f)));
            for (int i = 0; i < 18; i++) p.Fill(Sd.Circle(-3f + (i * 1.13f) % 6f, 2.2f - (i * 0.97f) % 7.5f, 0.03f + (i % 3) * 0.01f), Rgba.Hex("FFFFFF", 0.4f));
            // pannello delle corsie
            p.Shape(Sd.Box(0.1f, -0.6f, 2.75f, 2.45f, 0.15f), Rgba.Hex("5B4D93"));
            foreach (float y in new[] { 1.2f, 0f, -1.2f, -2.4f })
            {
                p.Shape(Sd.Box(0.15f, y, 2.2f, 0.36f, 0.08f), Rgba.Hex("3B3060"), 0.035f);
                p.Line(Rgba.Hex("FFD45A", 0.8f), 0.04f, -2.0f, y - 0.32f, 2.35f, y - 0.32f);
                p.Line(Rgba.Hex("FFD45A", 0.8f), 0.04f, -2.0f, y + 0.32f, 2.35f, y + 0.32f);
                for (float x = -1.6f; x < 2.2f; x += 0.6f) p.Fill(Sd.Polygon(x, y + 0.08f, x + 0.12f, y, x, y - 0.08f), Rgba.Hex("FFFFFF", 0.18f));
            }
            // fossa
            p.Shape(Sd.Box(2.6f, -0.6f, 0.28f, 2.35f, 0.06f), Rgba.Hex("120D22"));
            for (float y = -2.8f; y < 1.6f; y += 0.4f) p.Line(Rgba.Hex("E8443A"), 0.05f, 2.4f, y, 2.8f, y + 0.2f);
            return new DoodleArt("s41g_lanes_bg", p);
        }

        /// <summary>Cannoncino lanciapalle rivolto a destra (centro 0,0).</summary>
        public static DoodleArt S41G_Cannon()
        {
            var p = new DoodlePainter(-0.5f, -0.45f, 0.62f, 0.45f, 140) { Ink = 0.045f };
            p.Shape(Sd.Box(0.18f, 0.02f, 0.38f, 0.2f, 0.06f), Rgba.Hex("E8443A"));
            p.Shape(Sd.Box(0.52f, 0.02f, 0.06f, 0.25f, 0.03f), Rgba.Hex("FFD45A"));
            p.Shape(Sd.Circle(-0.15f, 0f, 0.32f), Rgba.Hex("FF7A6A"));
            S41G_Eye(p, -0.22f, 0.08f, 0.08f, 1f, 0f);
            S41G_Eye(p, -0.04f, 0.08f, 0.08f, 1f, 0f);
            p.Line(Ink, 0.03f, -0.25f, 0.2f, -0.06f, 0.15f);
            p.Line(Ink, 0.025f, -0.2f, -0.12f, -0.05f, -0.15f);
            return new DoodleArt("s41g_cannon", p);
        }

        /// <summary>Palla a spicchi rossa e bianca (raggio 0.28).</summary>
        public static DoodleArt S41G_Ball()
        {
            var p = new DoodlePainter(-0.32f, -0.32f, 0.32f, 0.32f, 160) { Ink = 0.035f };
            p.Shape(Sd.Circle(0, 0, 0.28f), Rgba.White);
            p.Fill(Sd.Intersect(Sd.Circle(0, 0, 0.27f), Sd.Box(0, 0, 0.09f, 0.3f)), Rgba.Hex("E8443A"));
            p.Fill(Sd.Intersect(Sd.Circle(0, 0, 0.27f), Sd.Box(0, 0, 0.3f, 0.07f)), Rgba.Hex("3A7BFF"));
            p.Shape(Sd.Circle(0, 0, 0.28f), Rgba.Clear);
            p.Fill(Sd.Circle(-0.1f, 0.1f, 0.06f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("s41g_ball", p);
        }

        /// <summary>Paratia imbottita trascinabile (0.4 x 1.0, centro 0,0).</summary>
        public static DoodleArt S41G_Bumper()
        {
            var p = new DoodlePainter(-0.28f, -0.56f, 0.28f, 0.56f, 140) { Ink = 0.045f };
            p.Shape(Sd.Box(0, 0, 0.2f, 0.5f, 0.12f), Rgba.Hex("3ED6C0"));
            for (int i = -1; i <= 1; i++) p.Line(Rgba.Hex("1E9C8A"), 0.03f, -0.12f, i * 0.25f, 0.12f, i * 0.25f);
            p.Fill(Sd.Box(-0.08f, 0.1f, 0.04f, 0.3f, 0.04f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("s41g_bumper", p);
        }

        /// <summary>Clessidra (icona del rallentatore, centro 0,0).</summary>
        public static DoodleArt S41G_Hourglass()
        {
            var p = new DoodlePainter(-0.25f, -0.3f, 0.25f, 0.3f, 160) { Ink = 0.03f };
            p.Shape(Sd.Polygon(-0.15f, 0.22f, 0.15f, 0.22f, 0.03f, 0f, 0.15f, -0.22f, -0.15f, -0.22f, -0.03f, 0f), Rgba.Hex("DFF3FF"));
            p.Fill(Sd.Polygon(-0.1f, -0.2f, 0.1f, -0.2f, 0f, -0.08f), Rgba.Hex("FFC93C"));
            p.Fill(Sd.Polygon(-0.07f, 0.15f, 0.07f, 0.15f, 0f, 0.06f), Rgba.Hex("FFC93C"));
            p.Shape(Sd.Box(0, 0.25f, 0.2f, 0.035f), Wood, 0.025f);
            p.Shape(Sd.Box(0, -0.25f, 0.2f, 0.035f), Wood, 0.025f);
            return new DoodleArt("s41g_hourglass", p);
        }

        // ================= THE OVERWORKED ROBOT =================

        /// <summary>Officina: parete a pannelli, console in basso con piano in y -1.2.</summary>
        public static DoodleArt S41G_LabBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("BCC6D6"), Rgba.Hex("DCE3EE"), S41G_Clamp((y + 1f) / 5f)));
            for (float x = -3.0f; x < 3.3f; x += 1.5f) p.Line(Rgba.Hex("A3AEC0"), 0.04f, x, -1.2f, x, 5.3f);
            for (float x = -3.0f; x < 3.3f; x += 1.5f)
                foreach (float y in new[] { 2.9f, 0.2f })
                    p.Fill(Sd.Circle(x + 0.12f, y, 0.04f), Rgba.Hex("8E9BAA"));
            // tubi e spia
            p.Shape(Sd.Capsule(-3.3f, 2.4f, -1.9f, 2.4f, 0.1f), Rgba.Hex("E39A55"), 0.035f);
            p.Shape(Sd.Capsule(-1.9f, 2.4f, -1.9f, 3.4f, 0.1f), Rgba.Hex("E39A55"), 0.035f);
            p.Shape(Sd.Box(2.2f, 2.0f, 0.5f, 0.3f, 0.05f), Rgba.Hex("3B3550"));
            p.Fill(Sd.Box(2.2f, 2.0f, 0.42f, 0.22f, 0.04f), Rgba.Hex("4FE38A", 0.5f));
            p.Line(Rgba.Hex("7BFF8A"), 0.03f, 1.82f, 2.0f, 2.0f, 2.0f, 2.08f, 2.15f, 2.2f, 1.85f, 2.3f, 2.0f, 2.58f, 2.0f);
            // console
            p.Shape(Sd.Box(0, -3.25f, 3.4f, 2.05f, 0.1f), Rgba.Hex("6E7C93"));
            p.Shape(Sd.Box(0, -1.25f, 3.4f, 0.1f), Rgba.Hex("97A6BC"));
            for (int i = 0; i < 7; i++) p.Shape(Sd.Circle(-2.6f + i * 0.85f, -4.6f, 0.08f), i % 2 == 0 ? Rgba.Hex("FFD45A") : Rgba.Hex("FF7A8A"), 0.03f);
            p.Hatch(Sd.Box(0, -4.85f, 3.3f, 0.25f), Rgba.Hex("FFD45A", 0.7f), 0.3f, 0.12f, 45f);
            return new DoodleArt("s41g_lab_bg", p);
        }

        /// <summary>Robot stanco senza mani (le mani sono a parte): spalle in (±0.6, 0.35), base in y -0.9.</summary>
        public static DoodleArt S41G_Robot(bool happy)
        {
            var p = new DoodlePainter(-0.95f, -1.0f, 0.95f, 1.75f, 120) { Ink = 0.05f };
            Rgba metal = Rgba.Hex("DCE3EE"), dark = Rgba.Hex("8E9BAA");
            // ruota/base
            p.Shape(Sd.Box(0, -0.75f, 0.45f, 0.15f, 0.12f), Rgba.Hex("3B3550"));
            p.Shape(Sd.Capsule(0, -0.55f, 0, -0.3f, 0.1f), dark);
            // corpo
            p.Shape(Sd.Box(0, 0.1f, 0.6f, 0.45f, 0.15f), metal);
            p.Shape(Sd.Box(0, 0.1f, 0.3f, 0.22f, 0.06f), Rgba.Hex("3B3550"));
            p.Fill(Sd.Box(-0.15f, 0.1f, 0.08f, 0.12f), happy ? Rgba.Hex("7BFF8A") : Rgba.Hex("FF5A5A"));
            p.Fill(Sd.Box(0.0f, 0.03f, 0.05f, 0.05f), Rgba.Hex("FFD45A"));
            p.Fill(Sd.Box(0.15f, 0.15f, 0.05f, 0.05f), Rgba.Hex("6BB8FF"));
            // spalle
            p.Shape(Sd.Circle(-0.62f, 0.35f, 0.14f), dark);
            p.Shape(Sd.Circle(0.62f, 0.35f, 0.14f), dark);
            // collo e testa
            p.Shape(Sd.Box(0, 0.65f, 0.1f, 0.1f), dark);
            p.Shape(Sd.Box(0, 1.08f, 0.48f, 0.36f, 0.14f), metal);
            p.Line(Ink, 0.035f, 0, 1.44f, 0, 1.62f);
            p.Shape(Sd.Circle(0, 1.65f, 0.07f), happy ? Rgba.Hex("7BFF8A") : Rgba.Hex("FF5A5A"), 0.03f);
            p.Shape(Sd.Box(0, 1.1f, 0.38f, 0.2f, 0.08f), Rgba.Hex("3B3550"), 0.03f);
            if (happy)
            {
                p.Line(Rgba.Hex("7BFF8A"), 0.05f, -0.24f, 1.1f, -0.16f, 1.18f, -0.08f, 1.1f);
                p.Line(Rgba.Hex("7BFF8A"), 0.05f, 0.08f, 1.1f, 0.16f, 1.18f, 0.24f, 1.1f);
            }
            else
            {
                p.Fill(Sd.Circle(-0.16f, 1.1f, 0.07f), Rgba.Hex("6BE3FF"));
                p.Fill(Sd.Circle(0.16f, 1.1f, 0.07f), Rgba.Hex("6BE3FF"));
                p.Fill(Sd.Box(-0.16f, 1.17f, 0.09f, 0.04f), Rgba.Hex("3B3550"));
                p.Fill(Sd.Box(0.16f, 1.17f, 0.09f, 0.04f), Rgba.Hex("3B3550"));
                // goccia di sudore
                p.Shape(Sd.Union(Sd.Circle(0.56f, 1.15f, 0.07f), Sd.Polygon(0.5f, 1.17f, 0.62f, 1.17f, 0.56f, 1.32f)), Rgba.Hex("8FD8FF"), 0.025f);
            }
            p.Line(Ink, 0.03f, -0.12f, 0.82f, 0.12f, 0.82f);
            return new DoodleArt(happy ? "s41g_robot_happy" : "s41g_robot", p);
        }

        /// <summary>Mano-guanto del robot vista dall'alto; il palmo spinge verso il basso (centro 0,0).</summary>
        public static DoodleArt S41G_RobotHand()
        {
            var p = new DoodlePainter(-0.48f, -0.4f, 0.48f, 0.4f, 140) { Ink = 0.045f };
            Rgba glove = Rgba.Hex("FFC93C");
            for (int i = 0; i < 4; i++) p.Shape(Sd.Capsule(-0.27f + i * 0.18f, -0.05f, -0.27f + i * 0.18f, -0.3f, 0.08f), glove);
            p.Shape(Sd.Box(0, 0.08f, 0.38f, 0.2f, 0.12f), glove);
            p.Shape(Sd.Box(0, 0.3f, 0.26f, 0.08f, 0.04f), Rgba.Hex("8E9BAA"));
            p.Line(Rgba.Hex("E39B1B"), 0.03f, -0.2f, 0.08f, 0.2f, 0.08f);
            return new DoodleArt("s41g_robot_hand", p);
        }

        /// <summary>Interruttore a levetta su placca (0.7 x 0.8, centro 0,0), spento o acceso.</summary>
        public static DoodleArt S41G_Switch(bool on)
        {
            var p = new DoodlePainter(-0.4f, -0.45f, 0.4f, 0.45f, 140) { Ink = 0.045f };
            p.Shape(Sd.Box(0, 0, 0.33f, 0.4f, 0.08f), Rgba.Hex("F4F2F8"));
            p.Shape(Sd.Box(0, -0.05f, 0.15f, 0.22f, 0.06f), Rgba.Hex("3B3550"));
            float ty = on ? 0.1f : -0.2f;
            p.Shape(Sd.Capsule(0, -0.05f, 0, ty, 0.07f), on ? Rgba.Hex("7BFF8A") : Rgba.Hex("FF7A6A"));
            p.Shape(Sd.Circle(0, 0.3f, 0.05f), on ? Rgba.Hex("7BFF8A") : Rgba.Hex("6B6675"), 0.025f);
            if (on) p.Fill(Sd.Circle(0, 0.3f, 0.12f), Rgba.Hex("7BFF8A", 0.3f));
            return new DoodleArt(on ? "s41g_switch_on" : "s41g_switch_off", p);
        }

        /// <summary>Zona tratteggiata sotto una mano del robot (1.7 x 1.0, centro 0,0).</summary>
        public static DoodleArt S41G_Pad()
        {
            var p = new DoodlePainter(-0.9f, -0.55f, 0.9f, 0.55f, 100) { Ink = 0.04f, Wobble = 0f };
            p.Fill(Sd.Box(0, 0, 0.84f, 0.49f, 0.12f), Rgba.Hex("FFD45A", 0.22f), false);
            // tratteggio sul bordo
            p.FillFunc(Sd.Subtract(Sd.Box(0, 0, 0.84f, 0.49f, 0.12f), Sd.Box(0, 0, 0.8f, 0.45f, 0.1f)), (x, y) =>
            {
                float u = (x + y * 1.7f) * 6f;
                return (u - (float)Math.Floor(u)) < 0.55f ? Rgba.Hex("FFD45A") : Rgba.Clear;
            });
            return new DoodleArt("s41g_pad", p);
        }

        // ================= THE MISSING LETTER =================

        /// <summary>Campagna: cielo, colline, albero a destra, staccionata, pilastri del cancello a x ±1.35, terreno in y -3.3.</summary>
        public static DoodleArt S41G_GateBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("D8F1FF"), Rgba.Hex("86CDF5"), S41G_Clamp((y + 1f) / 5f)));
            p.Shape(Sd.Union(Sd.Circle(-2.4f, -1.6f, 2.0f), Sd.Circle(2.0f, -2.0f, 2.4f)), Rgba.Hex("A6D987"));
            // prato
            p.FillFunc(Sd.Box(0, -4.3f, 3.3f, 1.05f), (x, y) => Rgba.Lerp(Rgba.Hex("5FB04A"), Rgba.Hex("7CC860"), S41G_Clamp((y + 5f) / 2f)));
            p.Line(Ink, 0.05f, -3.3f, -3.25f, 3.3f, -3.25f);
            // staccionata ai lati
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Box(s * 2.3f, -2.75f, 0.9f, 0.06f), Rgba.Hex("F4EEE3"), 0.03f);
                p.Shape(Sd.Box(s * 2.3f, -2.45f, 0.9f, 0.06f), Rgba.Hex("F4EEE3"), 0.03f);
                for (int k = 0; k < 4; k++) p.Shape(Sd.Box(s * (1.75f + k * 0.42f), -2.65f, 0.08f, 0.6f, 0.04f), Rgba.Hex("F4EEE3"), 0.03f);
            }
            // albero a destra
            p.Shape(Sd.Box(2.45f, -1.2f, 0.18f, 1.9f), Rgba.Hex("A8743F"));
            p.Shape(Sd.Union(Sd.Circle(2.1f, 0.9f, 0.75f), Sd.Circle(2.8f, 1.2f, 0.7f), Sd.Circle(2.5f, 1.7f, 0.6f)), Leaf);
            p.Fill(Sd.Crescent(Sd.Circle(2.1f, 0.9f, 0.72f), 0.12f, 0.12f), LeafShade.WithAlpha(0.6f));
            // pilastri del cancello
            foreach (float x in new[] { -1.35f, 1.35f })
            {
                p.Shape(Sd.Box(x, -1.95f, 0.2f, 1.35f, 0.04f), Rgba.Hex("C9B8A8"));
                p.Shape(Sd.Box(x, -0.52f, 0.26f, 0.1f, 0.03f), Rgba.Hex("A8968A"));
                p.Shape(Sd.Circle(x, -0.28f, 0.16f), Rgba.Hex("A8968A"));
                for (float y = -3.0f; y < -0.8f; y += 0.45f) p.Line(Rgba.Hex("A8968A"), 0.025f, x - 0.18f, y, x + 0.18f, y);
            }
            // fiori
            foreach (var f in new[] { (-2.4f, -3.7f), (-1.8f, -4.1f), (-0.6f, -4.3f), (0.9f, -4.0f), (2.2f, -3.8f), (2.6f, -4.4f) })
            {
                p.Line(LeafShade, 0.03f, f.Item1, f.Item2, f.Item1, f.Item2 - 0.25f);
                p.Shape(Sd.Union(Sd.Circle(f.Item1 - 0.07f, f.Item2, 0.07f), Sd.Circle(f.Item1 + 0.07f, f.Item2, 0.07f), Sd.Circle(f.Item1, f.Item2 + 0.07f, 0.07f), Sd.Circle(f.Item1, f.Item2 - 0.07f, 0.07f)), Rgba.Hex("FFB3C7"), 0.025f);
            }
            return new DoodleArt("s41g_gate_bg", p);
        }

        /// <summary>Anta sinistra del cancello in ferro: cardine in x 0, larga 1.15, alta 2.3 (base in y 0).</summary>
        public static DoodleArt S41G_GateHalf()
        {
            var p = new DoodlePainter(-0.08f, -0.08f, 1.22f, 2.42f, 110) { Ink = 0.04f };
            Rgba iron = Rgba.Hex("4A4458");
            p.Shape(Sd.Box(0.575f, 0.2f, 0.575f, 0.07f), iron);
            p.Shape(Sd.Box(0.575f, 1.6f, 0.575f, 0.07f), iron);
            for (int i = 0; i < 5; i++)
            {
                float x = 0.08f + i * 0.25f;
                p.Shape(Sd.Union(Sd.Box(x, 1.05f, 0.04f, 1.05f), Sd.Polygon(x - 0.09f, 2.1f, x + 0.09f, 2.1f, x, 2.32f)), iron, 0.03f);
            }
            p.Shape(Sd.Circle(0.6f, 0.9f, 0.22f), Rgba.Clear, 0.05f);
            p.Shape(Sd.Circle(1.05f, 0.9f, 0.07f), Gold, 0.03f);
            return new DoodleArt("s41g_gate_half", p);
        }

        /// <summary>Insegna di legno appesa (3.4 x 1.5, centro 0,0) con catenelle.</summary>
        public static DoodleArt S41G_SignBoard()
        {
            var p = new DoodlePainter(-1.8f, -0.8f, 1.8f, 1.2f, 100) { Ink = 0.05f };
            p.Line(Ink, 0.035f, -1.2f, 0.7f, -1.0f, 1.15f);
            p.Line(Ink, 0.035f, 1.2f, 0.7f, 1.0f, 1.15f);
            p.FillFunc(Sd.Box(0, 0, 1.7f, 0.72f, 0.1f), (x, y) => Rgba.Lerp(Rgba.Hex("E2A764"), Rgba.Hex("F2C98A"), 0.5f + 0.5f * S41G_Sin(y * 22f + S41G_Sin(x * 3f) * 2f)));
            p.Shape(Sd.Box(0, 0, 1.7f, 0.72f, 0.1f), Rgba.Clear);
            foreach (float x in new[] { -1.52f, 1.52f }) foreach (float y in new[] { -0.55f, 0.55f }) p.Shape(Sd.Circle(x, y, 0.05f), Rgba.Hex("8E9BAA"), 0.02f);
            return new DoodleArt("s41g_sign_board", p);
        }

        /// <summary>Nuvola senza faccia (centro 0,0, ~2.2 x 1.1).</summary>
        public static DoodleArt S41G_Cloud()
        {
            var p = new DoodlePainter(-1.2f, -0.6f, 1.2f, 0.7f, 100) { Ink = 0.045f };
            Sdf c = Sd.Union(Sd.Box(0, -0.2f, 1.0f, 0.3f, 0.3f), Sd.Circle(-0.45f, 0.1f, 0.42f), Sd.Circle(0.25f, 0.2f, 0.5f), Sd.Circle(0.75f, 0.0f, 0.35f));
            p.Shape(c, Rgba.White);
            p.Fill(Sd.Intersect(c, Sd.Box(0, -0.42f, 1.2f, 0.1f)), SnowShade);
            return new DoodleArt("s41g_cloud", p);
        }

        // ================= ANTI-GRAVITY TAXI =================

        /// <summary>Città: palazzi ai lati (x &lt; -2.35 e x &gt; 2.35), strada in y -3.6.</summary>
        public static DoodleArt S41G_CityBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("FFD6A8"), Rgba.Hex("8FB8F0"), S41G_Clamp((y + 2f) / 6f)));
            // palazzi lontani
            for (int i = 0; i < 6; i++)
            {
                float x = -2.0f + i * 0.8f, h = 1.0f + (i * 0.73f) % 1.4f;
                p.Fill(Sd.Box(x, -3.6f + h, 0.35f, h), Rgba.Hex("B9C3E6"));
                for (float y = -3.3f; y < -3.6f + 2f * h - 0.3f; y += 0.4f) p.Fill(Sd.Box(x, y, 0.15f, 0.08f), Rgba.Hex("E9EEFF"));
            }
            // palazzi ai lati
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Box(s * 2.85f, 0.0f, 0.5f, 5.4f), s < 0 ? Rgba.Hex("E89A7A") : Rgba.Hex("8FC9A8"));
                for (float y = -3.0f; y < 4.6f; y += 0.8f) p.Shape(Sd.Box(s * 2.85f, y, 0.22f, 0.25f, 0.03f), Rgba.Hex("FFF3B0"), 0.03f);
            }
            // marciapiede e strada
            p.Shape(Sd.Box(0, -3.75f, 3.4f, 0.15f), Rgba.Hex("C9C3D6"));
            p.Fill(Sd.Box(0, -4.6f, 3.3f, 0.7f), Rgba.Hex("4A4458"));
            for (float x = -2.6f; x < 2.8f; x += 0.9f) p.Fill(Sd.Box(x, -4.55f, 0.25f, 0.05f), Rgba.Hex("FFD45A"));
            return new DoodleArt("s41g_city_bg", p);
        }

        /// <summary>Taxi giallo verso destra (ruote in y 0, largo 1.5).</summary>
        public static DoodleArt S41G_Taxi()
        {
            var p = new DoodlePainter(-0.8f, -0.05f, 0.8f, 0.95f, 140) { Ink = 0.045f };
            Rgba y = Rgba.Hex("FFC93C");
            p.Shape(Sd.Union(Sd.Box(0, 0.33f, 0.74f, 0.2f, 0.1f), Sd.Polygon(-0.42f, 0.5f, -0.28f, 0.78f, 0.3f, 0.78f, 0.5f, 0.5f)), y);
            p.Shape(Sd.Polygon(-0.3f, 0.52f, -0.21f, 0.72f, 0.0f, 0.72f, 0.0f, 0.52f), Rgba.Hex("BDE7FF"), 0.03f);
            p.Shape(Sd.Polygon(0.06f, 0.52f, 0.06f, 0.72f, 0.26f, 0.72f, 0.4f, 0.52f), Rgba.Hex("BDE7FF"), 0.03f);
            p.Shape(Sd.Box(0, 0.86f, 0.15f, 0.06f, 0.02f), Rgba.White, 0.03f);
            for (int i = 0; i < 6; i++) p.Fill(Sd.Box(-0.55f + i * 0.22f, 0.3f, 0.055f, 0.05f), i % 2 == 0 ? Ink : Rgba.White);
            p.Shape(Sd.Circle(0.7f, 0.38f, 0.05f), Rgba.Hex("FFF3B0"), 0.02f);
            foreach (float x in new[] { -0.45f, 0.45f })
            {
                p.Shape(Sd.Circle(x, 0.13f, 0.14f), Rgba.Hex("3A3540"));
                p.Fill(Sd.Circle(x, 0.13f, 0.05f), Rgba.Hex("C9D3DD"));
            }
            return new DoodleArt("s41g_taxi", p);
        }

        /// <summary>Piattaforma volante (2.0 x 0.3, centro 0,0; piano in y 0.15) con razzetti sotto.</summary>
        public static DoodleArt S41G_Platform()
        {
            var p = new DoodlePainter(-1.08f, -0.6f, 1.08f, 0.22f, 120) { Ink = 0.045f };
            foreach (float x in new[] { -0.6f, 0.6f })
            {
                p.Shape(Sd.Union(Sd.Circle(x, -0.42f, 0.09f), Sd.Polygon(x - 0.08f, -0.4f, x + 0.08f, -0.4f, x, -0.6f)), Rgba.Hex("FF8A1F"), 0.025f);
                p.Shape(Sd.Box(x, -0.25f, 0.12f, 0.12f, 0.03f), Rgba.Hex("8E9BAA"));
            }
            p.Shape(Sd.Box(0, 0, 1.0f, 0.15f, 0.06f), Rgba.Hex("A3AEC0"));
            p.Hatch(Sd.Box(0, 0, 0.95f, 0.1f), Rgba.Hex("FFD45A"), 0.25f, 0.08f, 45f);
            p.Shape(Sd.Box(0, 0, 1.0f, 0.15f, 0.06f), Rgba.Clear);
            return new DoodleArt("s41g_platform", p);
        }

        /// <summary>Cliente: topo elegante con valigetta che chiama il taxi (piedi in y 0).</summary>
        public static DoodleArt S41G_Client()
        {
            var p = new DoodlePainter(-0.5f, -0.05f, 0.55f, 1.35f, 150) { Ink = 0.04f };
            Rgba fur = Rgba.Hex("B7B3C2"), suit = Rgba.Hex("4A5A8A"), pink = Rgba.Hex("FF9BB0");
            p.Shape(Sd.Capsule(-0.08f, 0.08f, -0.08f, 0.35f, 0.06f), suit);
            p.Shape(Sd.Capsule(0.08f, 0.08f, 0.08f, 0.35f, 0.06f), suit);
            p.Shape(Sd.Box(-0.33f, 0.28f, 0.14f, 0.1f, 0.02f), Rgba.Hex("8A5A3B"), 0.03f);
            p.Shape(Sd.Box(0, 0.55f, 0.2f, 0.25f, 0.08f), suit);
            p.Shape(Sd.Polygon(-0.05f, 0.78f, 0.05f, 0.78f, 0.0f, 0.55f), Rgba.White, 0.02f);
            p.Shape(Sd.Capsule(-0.18f, 0.7f, -0.3f, 0.42f, 0.05f), suit);
            p.Shape(Sd.Capsule(0.18f, 0.7f, 0.38f, 1.05f, 0.05f), suit);
            p.Shape(Sd.Circle(0.4f, 1.09f, 0.06f), fur, 0.025f);
            p.Shape(Sd.Circle(-0.2f, 1.12f, 0.12f), fur);
            p.Shape(Sd.Circle(0.2f, 1.12f, 0.12f), fur);
            p.Fill(Sd.Circle(-0.2f, 1.12f, 0.06f), pink);
            p.Fill(Sd.Circle(0.2f, 1.12f, 0.06f), pink);
            p.Shape(Sd.Ellipse(0, 0.95f, 0.2f, 0.17f), fur);
            S41G_Eye(p, -0.07f, 0.98f, 0.05f, 0.4f, 0.3f);
            S41G_Eye(p, 0.07f, 0.98f, 0.05f, 0.4f, 0.3f);
            p.Fill(Sd.Circle(0, 0.88f, 0.03f), pink);
            // bombetta
            p.Shape(Sd.Union(Sd.Box(0, 1.1f, 0.2f, 0.03f, 0.02f), Sd.Box(0, 1.2f, 0.12f, 0.1f, 0.06f)), Rgba.Hex("2B2233"), 0.02f);
            return new DoodleArt("s41g_client", p);
        }

        /// <summary>Medaglione della gravità (raggio 0.75, centro 0,0) con tacche ogni 45°.</summary>
        public static DoodleArt S41G_GravBadge()
        {
            var p = new DoodlePainter(-0.82f, -0.82f, 0.82f, 0.82f, 120) { Ink = 0.05f };
            p.Shape(Sd.Circle(0, 0, 0.75f), Rgba.Hex("6B4FD6"));
            p.Shape(Sd.Circle(0, 0, 0.62f), Rgba.Hex("E9E2FF"));
            for (int k = 0; k < 8; k++)
            {
                float a = k * 0.7854f;
                p.Line(Rgba.Hex("6B4FD6"), 0.04f, 0.5f * S41G_Cos(a), 0.5f * S41G_Sin(a), 0.6f * S41G_Cos(a), 0.6f * S41G_Sin(a));
            }
            return new DoodleArt("s41g_grav_badge", p);
        }

        /// <summary>Freccia della gravità (punta in giù, centro di rotazione 0,0).</summary>
        public static DoodleArt S41G_GravArrow()
        {
            var p = new DoodlePainter(-0.32f, -0.58f, 0.32f, 0.5f, 140) { Ink = 0.045f };
            p.Shape(Sd.Union(Sd.Box(0, 0.12f, 0.1f, 0.34f, 0.04f), Sd.Polygon(-0.27f, -0.18f, 0.27f, -0.18f, 0, -0.52f)), Rgba.Hex("FF5A7A"));
            p.Fill(Sd.Box(-0.03f, 0.15f, 0.025f, 0.22f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("s41g_grav_arrow", p);
        }
    }
}
