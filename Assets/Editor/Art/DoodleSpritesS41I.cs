using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli della specifica 105-200, pacchetto I.</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV41I() => new DoodleArt[]
        {
            // The stupid genius
            S41I_Scientist(), S41I_Machine(), S41I_Gear(), S41I_StartButton(), S41I_TargetButton(), S41I_Glove(),
            // The escaping cursor
            S41I_DesktopBg(), S41I_Cursor(), S41I_Bullseye(),
            // Upside-down code
            S41I_CodePlate(), S41I_KeyButton(),
            // Adpocalypse
            S41I_AdBanner(), S41I_AdX(), S41I_AdXAlive(), S41I_AdSmall(), S41I_IconPants(), S41I_IconPigeon(), S41I_IconLungs(),
            S41I_Executive(), S41I_Picket(),
            // The glass cage
            S41I_Dome(),
            // The invisible ink
            S41I_BlankMap(), S41I_Dryer(), S41I_Scorch(), S41I_InkX(),
            // The last drop
            S41I_Bottle(),
        };

        static float S41I_Sin(float v) => (float)Math.Sin(v);
        static float S41I_Cos(float v) => (float)Math.Cos(v);
        static float S41I_Clamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        static void S41I_Eyes(DoodlePainter p, float cx, float cy, float gap, float r, float lookX = 0f, float lookY = 0f)
        {
            foreach (float s in new[] { -1f, 1f })
            {
                float x = cx + s * gap;
                p.Shape(Sd.Circle(x, cy, r), Rgba.White, r * 0.35f);
                p.Fill(Sd.Circle(x + lookX * r * 0.35f, cy + lookY * r * 0.35f, r * 0.5f), Ink);
                p.Fill(Sd.Circle(x + lookX * r * 0.35f - r * 0.18f, cy + lookY * r * 0.35f + r * 0.2f, r * 0.17f), Rgba.White);
            }
        }

        // ================= THE STUPID GENIUS =================

        /// <summary>Scienziato col camice, capelli sparati e occhialoni (piedi in y 0, alto 2).</summary>
        public static DoodleArt S41I_Scientist()
        {
            var p = new DoodlePainter(-0.75f, -0.06f, 0.8f, 2.15f, 110) { Ink = 0.045f };
            p.Shape(Sd.Ellipse(-0.2f, 0.07f, 0.19f, 0.08f), Rgba.Hex("3A2E2A"));
            p.Shape(Sd.Ellipse(0.2f, 0.07f, 0.19f, 0.08f), Rgba.Hex("3A2E2A"));
            p.Shape(Sd.Box(-0.15f, 0.35f, 0.09f, 0.25f), Rgba.Hex("4A4E6E"));
            p.Shape(Sd.Box(0.15f, 0.35f, 0.09f, 0.25f), Rgba.Hex("4A4E6E"));
            // camice
            Sdf coat = Sd.Polygon(-0.48f, 0.45f, 0.48f, 0.45f, 0.34f, 1.42f, -0.34f, 1.42f);
            Body(p, coat, Rgba.Hex("F6F8FC"), Rgba.Hex("D3DAE6"));
            p.InkLine(0.025f, 0f, 1.4f, 0f, 0.47f);
            p.Shape(Sd.Box(0.22f, 1.12f, 0.08f, 0.06f), Rgba.Hex("BFE3FF"), 0.02f);
            p.Line(Rgba.Hex("E8433F"), 0.025f, 0.18f, 1.13f, 0.18f, 1.25f);
            p.Shape(Sd.Polygon(0f, 1.36f, -0.1f, 1.42f, 0f, 1.2f, 0.1f, 1.42f), Rgba.Hex("E8433F"), 0.02f);
            // braccia: una alzata col dito puntato
            p.Shape(Sd.Capsule(-0.33f, 1.3f, -0.6f, 0.95f, 0.08f), Rgba.Hex("F6F8FC"));
            p.Shape(Sd.Circle(-0.62f, 0.92f, 0.08f), Skin);
            p.Shape(Sd.Capsule(0.33f, 1.3f, 0.6f, 1.6f, 0.08f), Rgba.Hex("F6F8FC"));
            p.Shape(Sd.Circle(0.62f, 1.63f, 0.08f), Skin);
            p.Shape(Sd.Capsule(0.64f, 1.68f, 0.68f, 1.84f, 0.03f), Skin, 0.025f);
            // testa
            Sdf hair = Sd.Union(Sd.Circle(-0.32f, 1.85f, 0.16f), Sd.Circle(0.32f, 1.85f, 0.16f), Sd.Circle(-0.22f, 2.02f, 0.13f),
                                Sd.Circle(0.22f, 2.02f, 0.13f), Sd.Circle(-0.4f, 1.68f, 0.12f), Sd.Circle(0.4f, 1.68f, 0.12f));
            p.Shape(hair, Rgba.Hex("F2F2F6"));
            p.Shape(Sd.Circle(0, 1.72f, 0.3f), Skin);
            p.Fill(Sd.Ellipse(0, 1.97f, 0.16f, 0.05f), Rgba.Hex("FFE6CC"));
            // occhialoni
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Circle(s * 0.13f, 1.76f, 0.11f), Rgba.Hex("E9F6FF"), 0.035f);
                p.Fill(Sd.Circle(s * 0.13f + 0.02f, 1.75f, 0.04f), Ink);
            }
            p.InkLine(0.03f, -0.02f, 1.77f, 0.02f, 1.77f);
            // baffi e bocca
            p.Shape(Sd.Union(Sd.Ellipse(-0.08f, 1.58f, 0.1f, 0.045f), Sd.Ellipse(0.08f, 1.58f, 0.1f, 0.045f)), Rgba.Hex("F2F2F6"), 0.025f);
            p.Shape(Sd.Ellipse(0, 1.5f, 0.05f, 0.03f), Rgba.Hex("7A2A3A"), 0.02f);
            return new DoodleArt("s41i_scientist", p);
        }

        /// <summary>Macchinario enorme e inutile: caldaia, tubi, scivolo, molla col braccio (centro 0,0, ~2.9x3.2).
        /// La punta del braccio a molla è in (1.35, -1.25); il buco dell'ingranaggio in (-0.35, 0.55).</summary>
        public static DoodleArt S41I_Machine()
        {
            var p = new DoodlePainter(-1.55f, -1.75f, 1.55f, 1.75f, 70) { Ink = 0.05f };
            Rgba copper = Rgba.Hex("E08A4A"), copperShade = Rgba.Hex("B8622E"), metal = Rgba.Hex("B7BCC9"), metalShade = Rgba.Hex("8D93A6");
            // gambe
            p.Shape(Sd.Box(-1.2f, -1.4f, 0.07f, 0.3f), metalShade);
            p.Shape(Sd.Box(0.3f, -1.4f, 0.07f, 0.3f), metalShade);
            p.Shape(Sd.Ellipse(-1.2f, -1.68f, 0.15f, 0.05f), Rgba.Hex("4A4E5E"));
            p.Shape(Sd.Ellipse(0.3f, -1.68f, 0.15f, 0.05f), Rgba.Hex("4A4E5E"));
            // tubi
            p.Shape(Sd.Polyline(0.09f, -0.7f, 1.0f, -0.7f, 1.45f, 0.6f, 1.45f, 0.6f, 0.9f), Rgba.Hex("6FB3E0"));
            p.Shape(Sd.Polyline(0.08f, 0.6f, 0.9f, 1.25f, 0.9f, 1.25f, -0.3f), Rgba.Hex("7CC86A"));
            p.Shape(Sd.Polyline(0.07f, -1.3f, -0.3f, -1.45f, -0.3f, -1.45f, 0.8f, -1.15f, 0.8f), Rgba.Hex("C98BE0"));
            // caldaia
            Sdf boiler = Sd.Box(-0.7f, 0.15f, 0.62f, 0.85f, 0.35f);
            p.FillFunc(boiler, (x, y) => Rgba.Lerp(copperShade, copper, S41I_Clamp(0.5f + 0.5f * S41I_Sin((x + 0.7f) * 2.5f))));
            for (float y = -0.4f; y < 0.9f; y += 0.4f) p.Line(copperShade, 0.03f, -1.3f, y, -0.1f, y);
            p.Shape(boiler, Rgba.Clear);
            // camino a imbuto
            p.Shape(Sd.Polygon(-0.95f, 0.95f, -0.45f, 0.95f, -0.3f, 1.45f, -1.1f, 1.45f), metal);
            p.Shape(Sd.Box(-0.7f, 1.5f, 0.45f, 0.06f, 0.03f), metalShade);
            // manometro
            p.Shape(Sd.Circle(-1.0f, -0.3f, 0.2f), Rgba.White, 0.035f);
            p.Line(Rgba.Hex("E8433F"), 0.03f, -1.0f, -0.3f, -0.9f, -0.18f);
            p.Fill(Sd.Circle(-1.0f, -0.3f, 0.03f), Ink);
            // sede dell'ingranaggio
            p.Shape(Sd.Circle(-0.35f, 0.55f, 0.12f), metalShade, 0.03f);
            // scatola dei comandi e scivolo
            p.Shape(Sd.Box(0.3f, -0.85f, 0.4f, 0.3f, 0.06f), Rgba.Hex("F2D06B"));
            foreach (float x in new[] { 0.12f, 0.3f, 0.48f }) p.Shape(Sd.Circle(x, -0.78f, 0.06f), x < 0.2f ? Rgba.Hex("E8433F") : x < 0.4f ? Rgba.Hex("5BBF4A") : Rgba.Hex("3E8EF7"), 0.025f);
            p.Shape(Sd.Polygon(0.0f, 0.35f, 1.3f, -0.35f, 1.3f, -0.2f, 0.0f, 0.5f), Wood);
            p.Shape(Sd.Circle(0.25f, 0.42f, 0.09f), Rgba.Hex("D9D9E2"));
            // molla e braccio
            var spring = new List<float>();
            for (int i = 0; i <= 8; i++) { spring.Add(0.72f + i * 0.06f); spring.Add(-0.95f + (i % 2 == 0 ? 0.12f : -0.12f) - i * 0.04f); }
            p.Line(Ink, 0.07f, spring.ToArray());
            p.Line(metal, 0.035f, spring.ToArray());
            p.Shape(Sd.Capsule(1.2f, -1.27f, 1.38f, -1.27f, 0.07f), metalShade);
            return new DoodleArt("s41i_machine", p);
        }

        /// <summary>Ingranaggio (raggio 0.38, centro 0,0).</summary>
        public static DoodleArt S41I_Gear()
        {
            var p = new DoodlePainter(-0.45f, -0.45f, 0.45f, 0.45f, 140) { Ink = 0.035f };
            var parts = new List<Sdf> { Sd.Circle(0, 0, 0.29f) };
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * 6.2832f;
                parts.Add(Sd.Rotate(Sd.Box(S41I_Cos(a) * 0.32f, S41I_Sin(a) * 0.32f, 0.07f, 0.06f, 0.02f), S41I_Cos(a) * 0.32f, S41I_Sin(a) * 0.32f, a * 57.3f));
            }
            Sdf gear = Sd.Subtract(Sd.Union(parts.ToArray()), Sd.Circle(0, 0, 0.08f));
            Body(p, gear, Rgba.Hex("FFC93C"), Rgba.Hex("E39B1B"));
            p.Shape(Sd.Circle(0, 0, 0.16f), Rgba.Clear, 0.025f);
            return new DoodleArt("s41i_gear", p);
        }

        /// <summary>Pulsantone verde START su base (centro 0,0, largo 1).</summary>
        public static DoodleArt S41I_StartButton()
        {
            var p = new DoodlePainter(-0.55f, -0.35f, 0.55f, 0.35f, 140) { Ink = 0.04f };
            p.Shape(Sd.Box(0, -0.18f, 0.5f, 0.14f, 0.05f), Rgba.Hex("6A6E80"));
            Sdf dome = Sd.Intersect(Sd.Ellipse(0, -0.04f, 0.4f, 0.32f), Sd.Box(0, 0.15f, 0.5f, 0.19f));
            Body(p, dome, Rgba.Hex("5BCF5A"), Rgba.Hex("3E9A35"));
            p.Fill(Sd.Ellipse(-0.15f, 0.17f, 0.1f, 0.04f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("s41i_start_btn", p);
        }

        /// <summary>Pulsantino rosso su un piedistallo (centro 0,0, largo 0.6).</summary>
        public static DoodleArt S41I_TargetButton()
        {
            var p = new DoodlePainter(-0.36f, -0.36f, 0.36f, 0.3f, 180) { Ink = 0.035f };
            p.Shape(Sd.Box(0, -0.22f, 0.12f, 0.12f), Rgba.Hex("B7BCC9"));
            p.Shape(Sd.Box(0, -0.08f, 0.3f, 0.06f, 0.03f), Rgba.Hex("8D93A6"));
            Sdf top = Sd.Intersect(Sd.Ellipse(0, -0.02f, 0.2f, 0.2f), Sd.Box(0, 0.1f, 0.3f, 0.12f));
            Body(p, top, Rgba.Hex("F2484A"), Rgba.Hex("B92E2A"));
            p.Fill(Sd.Ellipse(-0.07f, 0.12f, 0.06f, 0.025f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("s41i_target_btn", p);
        }

        /// <summary>Guantone da boxe rosso che punta a destra (centro 0,0).</summary>
        public static DoodleArt S41I_Glove()
        {
            var p = new DoodlePainter(-0.4f, -0.3f, 0.4f, 0.3f, 160) { Ink = 0.035f };
            p.Shape(Sd.Box(-0.28f, 0f, 0.1f, 0.13f, 0.03f), Rgba.White);
            Sdf g = Sd.Union(Sd.Ellipse(0.08f, 0f, 0.27f, 0.22f), Sd.Circle(0.02f, 0.14f, 0.12f));
            Body(p, g, Rgba.Hex("E8433F"), Rgba.Hex("B92E2A"));
            p.InkLine(0.02f, -0.02f, 0.05f, 0.18f, 0.08f);
            p.Fill(Sd.Ellipse(0.12f, 0.1f, 0.08f, 0.04f), Rgba.White.WithAlpha(0.5f));
            return new DoodleArt("s41i_glove", p);
        }

        // ================= THE ESCAPING CURSOR =================

        /// <summary>Desktop da cartone: sfondo con colline, icone a sinistra e barra in basso (6.4 x 10.4).</summary>
        public static DoodleArt S41I_DesktopBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("7FD3E8"), Rgba.Hex("3F8FD0"), S41I_Clamp((y + 1f) / 6f)));
            p.Fill(Sd.Circle(1.7f, 3.0f, 0.7f), Rgba.Hex("FFF1A8", 0.8f));
            p.Shape(Sd.Union(Sd.Circle(-2.2f, -5.6f, 3.2f), Sd.Circle(1.8f, -6.0f, 3.6f)), Rgba.Hex("7CCB6A"), 0.04f);
            p.Shape(Sd.Circle(0.2f, -6.6f, 3.1f), Rgba.Hex("5FB356"), 0.04f);
            // icone
            for (int i = 0; i < 3; i++)
            {
                float y = 2.4f - i * 1.0f;
                if (i == 2) { p.Shape(Sd.Box(-2.55f, y, 0.22f, 0.26f, 0.04f), Rgba.Hex("D9DEE8")); p.Shape(Sd.Box(-2.55f, y + 0.3f, 0.27f, 0.05f), Rgba.Hex("B7BCC9")); }
                else { p.Shape(Sd.Box(-2.55f, y, 0.3f, 0.22f, 0.05f), Rgba.Hex("FFD45A")); p.Shape(Sd.Box(-2.68f, y + 0.24f, 0.14f, 0.05f, 0.02f), Rgba.Hex("F2B21E")); }
                p.Fill(Sd.Box(-2.55f, y - 0.42f, 0.3f, 0.05f), Rgba.White.WithAlpha(0.8f));
            }
            // barra
            p.Shape(Sd.Box(0, -4.9f, 3.3f, 0.32f), Rgba.Hex("E9ECF4"));
            p.Shape(Sd.Box(-2.5f, -4.9f, 0.45f, 0.2f, 0.12f), Rgba.Hex("7CCB6A"), 0.035f);
            p.Shape(Sd.Box(2.45f, -4.9f, 0.5f, 0.18f, 0.06f), Rgba.Hex("D9DEE8"), 0.03f);
            return new DoodleArt("s41i_desktop_bg", p);
        }

        /// <summary>Freccia del cursore con occhietti; la punta è in (0,0).</summary>
        public static DoodleArt S41I_Cursor()
        {
            var p = new DoodlePainter(-0.08f, -1.02f, 0.68f, 0.08f, 160) { Ink = 0.045f };
            Sdf arrow = Sd.Polygon(0f, 0f, 0f, -0.8f, 0.19f, -0.62f, 0.34f, -0.94f, 0.48f, -0.87f, 0.33f, -0.56f, 0.6f, -0.56f);
            Body(p, arrow, Rgba.White, Rgba.Hex("D3DAE6"));
            p.Fill(Sd.Circle(0.12f, -0.4f, 0.045f), Ink);
            p.Fill(Sd.Circle(0.24f, -0.44f, 0.045f), Ink);
            p.Fill(Sd.Circle(0.11f, -0.385f, 0.015f), Rgba.White);
            p.Fill(Sd.Circle(0.23f, -0.425f, 0.015f), Rgba.White);
            return new DoodleArt("s41i_cursor", p);
        }

        /// <summary>Bersaglio a cerchi (raggio 0.45).</summary>
        public static DoodleArt S41I_Bullseye()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.5f, 0.5f, 160) { Ink = 0.04f };
            p.Shape(Sd.Circle(0, 0, 0.45f), Rgba.Hex("F2484A"));
            p.Shape(Sd.Circle(0, 0, 0.33f), Rgba.White, 0.025f);
            p.Shape(Sd.Circle(0, 0, 0.21f), Rgba.Hex("F2484A"), 0.025f);
            p.Shape(Sd.Circle(0, 0, 0.09f), Rgba.White, 0.025f);
            return new DoodleArt("s41i_target", p);
        }

        // ================= UPSIDE-DOWN CODE =================

        /// <summary>Targa metallica con viti e display scuro (2.8 x 1.0).</summary>
        public static DoodleArt S41I_CodePlate()
        {
            var p = new DoodlePainter(-1.48f, -0.56f, 1.48f, 0.56f, 110) { Ink = 0.04f };
            Sdf plate = Sd.Box(0, 0, 1.4f, 0.5f, 0.1f);
            p.FillFunc(plate, (x, y) => Rgba.Lerp(Rgba.Hex("8D93A6"), Rgba.Hex("D2D6E0"), S41I_Clamp((y + 0.5f) + 0.1f * S41I_Sin(x * 20f))));
            p.Shape(plate, Rgba.Clear);
            p.Shape(Sd.Box(0, 0, 1.15f, 0.33f, 0.05f), Rgba.Hex("1E2430"), 0.03f);
            foreach (float sx in new[] { -1.28f, 1.28f })
                foreach (float sy in new[] { -0.36f, 0.36f })
                {
                    p.Shape(Sd.Circle(sx, sy, 0.06f), Rgba.Hex("B7BCC9"), 0.02f);
                    p.InkLine(0.015f, sx - 0.04f, sy - 0.02f, sx + 0.04f, sy + 0.02f);
                }
            return new DoodleArt("s41i_code_plate", p);
        }

        /// <summary>Tasto del tastierino (0.74 x 0.6).</summary>
        public static DoodleArt S41I_KeyButton()
        {
            var p = new DoodlePainter(-0.42f, -0.36f, 0.42f, 0.36f, 140) { Ink = 0.035f };
            p.Shape(Sd.Box(0, -0.04f, 0.37f, 0.28f, 0.08f), Rgba.Hex("6A6E80"));
            p.Shape(Sd.Box(0, 0.02f, 0.37f, 0.27f, 0.08f), Rgba.Hex("E9ECF4"));
            p.Fill(Sd.Box(0, 0.2f, 0.28f, 0.04f, 0.03f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("s41i_key_btn", p);
        }

        // ================= ADPOCALYPSE =================

        /// <summary>Banner pubblicitario finto e chiassoso (4.8 x 3.6, centro 0,0).</summary>
        public static DoodleArt S41I_AdBanner()
        {
            var p = new DoodlePainter(-2.48f, -1.88f, 2.48f, 1.88f, 60) { Ink = 0.06f };
            Sdf frame = Sd.Box(0, 0, 2.4f, 1.8f, 0.12f);
            p.FillFunc(frame, (x, y) =>
            {
                double a = Math.Atan2(y + 0.3f, x);
                bool ray = ((int)Math.Floor((a + Math.PI) / (Math.PI / 10))) % 2 == 0;
                return ray ? Rgba.Hex("FFE04A") : Rgba.Hex("FFB833");
            });
            p.Shape(Sd.Box(0, 0, 2.4f, 1.8f, 0.12f), Rgba.Clear, 0.1f);
            p.Shape(Sd.Box(0, 1.35f, 2.3f, 0.36f, 0.06f), Rgba.Hex("FF3D8A"));
            // stella "SALE"
            var pts = new List<float>();
            for (int i = 0; i < 24; i++)
            {
                float a = i / 24f * 6.2832f, r = i % 2 == 0 ? 0.62f : 0.46f;
                pts.Add(1.55f + S41I_Cos(a) * r); pts.Add(-0.95f + S41I_Sin(a) * r);
            }
            p.Shape(Sd.Polygon(pts.ToArray()), Rgba.Hex("3ED0FF"));
            // formaggio gigante disegnato sul banner
            Sdf cheese = Sd.Polygon(-1.9f, -1.35f, 0.6f, -1.35f, 0.6f, -0.45f, -1.9f, -0.85f);
            Body(p, cheese, Rgba.Hex("FFD447"), Rgba.Hex("E9A82A"));
            foreach (var h in new[] { (-1.2f, -1.05f, 0.14f), (-0.3f, -0.9f, 0.1f), (0.2f, -1.15f, 0.12f) })
                p.Shape(Sd.Circle(h.Item1, h.Item2, h.Item3), Rgba.Hex("E9A82A"), 0.03f);
            // scintille
            foreach (var s in new[] { (-2.0f, 0.6f), (1.9f, 0.5f), (-0.4f, 0.0f) })
            {
                p.Line(Rgba.White, 0.05f, s.Item1 - 0.15f, s.Item2, s.Item1 + 0.15f, s.Item2);
                p.Line(Rgba.White, 0.05f, s.Item1, s.Item2 - 0.15f, s.Item1, s.Item2 + 0.15f);
            }
            return new DoodleArt("s41i_ad_banner", p);
        }

        /// <summary>Quadratino rosso con la X di chiusura (0.56).</summary>
        public static DoodleArt S41I_AdX()
        {
            var p = new DoodlePainter(-0.3f, -0.3f, 0.3f, 0.3f, 180) { Ink = 0.035f };
            p.Shape(Sd.Box(0, 0, 0.26f, 0.26f, 0.06f), Rgba.Hex("F2484A"));
            p.Line(Rgba.White, 0.07f, -0.12f, -0.12f, 0.12f, 0.12f);
            p.Line(Rgba.White, 0.07f, -0.12f, 0.12f, 0.12f, -0.12f);
            return new DoodleArt("s41i_ad_x", p);
        }

        /// <summary>X viva e arrabbiata con le gambette (piedi in y -0.45).</summary>
        public static DoodleArt S41I_AdXAlive()
        {
            var p = new DoodlePainter(-0.42f, -0.5f, 0.42f, 0.42f, 160) { Ink = 0.035f };
            p.Shape(Sd.Capsule(-0.08f, -0.2f, -0.12f, -0.42f, 0.03f), Ink);
            p.Shape(Sd.Capsule(0.08f, -0.2f, 0.12f, -0.42f, 0.03f), Ink);
            Sdf x = Sd.Union(Sd.Capsule(-0.28f, -0.22f, 0.28f, 0.32f, 0.1f), Sd.Capsule(-0.28f, 0.32f, 0.28f, -0.22f, 0.1f));
            Body(p, x, Rgba.Hex("F2484A"), Rgba.Hex("B92E2A"));
            p.Fill(Sd.Circle(-0.07f, 0.1f, 0.04f), Rgba.White);
            p.Fill(Sd.Circle(0.07f, 0.1f, 0.04f), Rgba.White);
            p.Fill(Sd.Circle(-0.065f, 0.095f, 0.02f), Ink);
            p.Fill(Sd.Circle(0.065f, 0.095f, 0.02f), Ink);
            p.InkLine(0.025f, -0.13f, 0.2f, -0.02f, 0.15f);
            p.InkLine(0.025f, 0.13f, 0.2f, 0.02f, 0.15f);
            p.InkLine(0.02f, -0.04f, -0.01f, 0f, 0.01f, 0.04f, -0.01f);
            return new DoodleArt("s41i_ad_x_alive", p);
        }

        /// <summary>Finestrella pubblicitaria bianca (si tinge; 2.2 x 1.5) con la barretta in alto (la X è un oggetto a parte in 0.88, 0.58).</summary>
        public static DoodleArt S41I_AdSmall()
        {
            var p = new DoodlePainter(-1.15f, -0.8f, 1.15f, 0.8f, 90) { Ink = 0.045f };
            p.Shape(Sd.Box(0, 0, 1.08f, 0.73f, 0.08f), Rgba.White);
            p.Shape(Sd.Box(0, 0.58f, 1.06f, 0.13f, 0.04f), Rgba.Hex("D9DEE8"), 0.03f);
            return new DoodleArt("s41i_ad_small", p);
        }

        /// <summary>Mutande a pois (icona, ~0.7).</summary>
        public static DoodleArt S41I_IconPants()
        {
            var p = new DoodlePainter(-0.4f, -0.32f, 0.4f, 0.3f, 160) { Ink = 0.035f };
            Sdf pants = Sd.Polygon(-0.35f, 0.22f, 0.35f, 0.22f, 0.33f, -0.05f, 0.12f, -0.25f, 0f, -0.08f, -0.12f, -0.25f, -0.33f, -0.05f);
            p.Fill(pants, Rgba.Hex("7FC4FF"));
            foreach (var d in new[] { (-0.2f, 0.08f), (0.1f, 0.1f), (0.22f, -0.05f), (-0.05f, -0.02f), (-0.22f, -0.1f) })
                p.Fill(Sd.Circle(d.Item1, d.Item2, 0.035f), Rgba.Hex("FF7FB0"));
            p.Shape(pants, Rgba.Clear);
            p.Shape(Sd.Box(0, 0.2f, 0.35f, 0.04f), Rgba.White, 0.025f);
            return new DoodleArt("s41i_icon_pants", p);
        }

        /// <summary>Piccione col tocco da laureato (icona, ~0.8).</summary>
        public static DoodleArt S41I_IconPigeon()
        {
            var p = new DoodlePainter(-0.45f, -0.35f, 0.45f, 0.45f, 160) { Ink = 0.035f };
            p.Shape(Sd.Capsule(-0.05f, -0.25f, -0.08f, -0.32f, 0.02f), Rgba.Hex("FF8A6A"));
            p.Shape(Sd.Capsule(0.08f, -0.25f, 0.1f, -0.32f, 0.02f), Rgba.Hex("FF8A6A"));
            Body(p, Sd.Ellipse(0f, -0.05f, 0.3f, 0.22f), Rgba.Hex("A9AFC2"), Rgba.Hex("8189A0"));
            p.Shape(Sd.Ellipse(-0.08f, -0.05f, 0.15f, 0.1f), Rgba.Hex("8D93A6"), 0.025f);
            p.Shape(Sd.Circle(0.22f, 0.16f, 0.13f), Rgba.Hex("7FA88A"));
            p.Shape(Sd.Polygon(0.33f, 0.18f, 0.43f, 0.14f, 0.33f, 0.11f), Rgba.Hex("FFB84A"), 0.02f);
            p.Fill(Sd.Circle(0.25f, 0.18f, 0.025f), Ink);
            p.Shape(Sd.Polygon(0.06f, 0.33f, 0.24f, 0.4f, 0.4f, 0.33f, 0.24f, 0.26f), Rgba.Hex("2E2A3A"), 0.02f);
            p.Line(Rgba.Hex("FFC93C"), 0.02f, 0.33f, 0.33f, 0.38f, 0.2f);
            return new DoodleArt("s41i_icon_pigeon", p);
        }

        /// <summary>Polmoni rosa con la coroncina "premium" (icona, ~0.8).</summary>
        public static DoodleArt S41I_IconLungs()
        {
            var p = new DoodlePainter(-0.42f, -0.38f, 0.42f, 0.45f, 160) { Ink = 0.035f };
            p.Shape(Sd.Capsule(0f, 0.25f, 0f, 0.05f, 0.04f), Rgba.Hex("F2A5C0"));
            foreach (float s in new[] { -1f, 1f })
                Body(p, Sd.Ellipse(s * 0.17f, -0.1f, 0.15f, 0.25f), Rgba.Hex("FF8FB0"), Rgba.Hex("E2638C"));
            p.Shape(Sd.Polygon(-0.14f, 0.28f, -0.14f, 0.42f, -0.07f, 0.35f, 0f, 0.43f, 0.07f, 0.35f, 0.14f, 0.42f, 0.14f, 0.28f), Gold, 0.025f);
            return new DoodleArt("s41i_icon_lungs", p);
        }

        /// <summary>Dirigente minuscolo in giacca e cravatta che urla (piedi in y 0, alto 1).</summary>
        public static DoodleArt S41I_Executive()
        {
            var p = new DoodlePainter(-0.48f, -0.05f, 0.5f, 1.08f, 160) { Ink = 0.035f };
            p.Shape(Sd.Ellipse(-0.1f, 0.04f, 0.1f, 0.045f), Ink);
            p.Shape(Sd.Ellipse(0.1f, 0.04f, 0.1f, 0.045f), Ink);
            p.Shape(Sd.Box(-0.07f, 0.2f, 0.05f, 0.14f), Rgba.Hex("3A3E52"));
            p.Shape(Sd.Box(0.07f, 0.2f, 0.05f, 0.14f), Rgba.Hex("3A3E52"));
            // braccia agitate
            p.Shape(Sd.Capsule(-0.18f, 0.55f, -0.38f, 0.82f, 0.045f), Rgba.Hex("4A4E6E"));
            p.Shape(Sd.Capsule(0.18f, 0.55f, 0.38f, 0.82f, 0.045f), Rgba.Hex("4A4E6E"));
            p.Shape(Sd.Circle(-0.39f, 0.85f, 0.05f), Skin, 0.025f);
            p.Shape(Sd.Circle(0.39f, 0.85f, 0.05f), Skin, 0.025f);
            Body(p, Sd.Box(0, 0.48f, 0.19f, 0.17f, 0.06f), Rgba.Hex("4A4E6E"), Rgba.Hex("363A55"));
            p.Shape(Sd.Polygon(-0.07f, 0.65f, 0.07f, 0.65f, 0f, 0.5f), Rgba.White, 0.02f);
            p.Shape(Sd.Polygon(-0.03f, 0.63f, 0.03f, 0.63f, 0.04f, 0.4f, 0f, 0.35f, -0.04f, 0.4f), Rgba.Hex("E8433F"), 0.018f);
            // testa che urla
            p.Shape(Sd.Circle(0, 0.82f, 0.18f), Skin);
            p.Shape(Sd.Intersect(Sd.Circle(0, 0.84f, 0.185f), Sd.Box(0, 0.98f, 0.2f, 0.06f)), Rgba.Hex("5A3A2A"), 0.025f);
            p.InkLine(0.025f, -0.11f, 0.9f, -0.03f, 0.86f);
            p.InkLine(0.025f, 0.11f, 0.9f, 0.03f, 0.86f);
            p.Fill(Sd.Circle(-0.06f, 0.83f, 0.022f), Ink);
            p.Fill(Sd.Circle(0.06f, 0.83f, 0.022f), Ink);
            p.Shape(Sd.Ellipse(0, 0.73f, 0.06f, 0.05f), Rgba.Hex("7A2A3A"), 0.02f);
            p.Fill(Sd.Circle(0.17f, 0.95f, 0.025f), Rgba.Hex("7FC4FF"));
            return new DoodleArt("s41i_exec", p);
        }

        /// <summary>Cartello da sciopero (tabella bianca in y 0.2, asta fino a -0.7).</summary>
        public static DoodleArt S41I_Picket()
        {
            var p = new DoodlePainter(-0.5f, -0.75f, 0.5f, 0.5f, 140) { Ink = 0.035f };
            p.Shape(Sd.Box(0, -0.25f, 0.04f, 0.45f), Wood);
            p.Shape(Sd.Box(0, 0.2f, 0.45f, 0.24f, 0.03f), Rgba.Hex("FFF8E8"));
            return new DoodleArt("s41i_picket", p);
        }

        // ================= THE GLASS CAGE =================

        /// <summary>Campana di vetro vista dall'alto (raggio 1.35) con l'apertura alla base verso destra (angolo 0, ±24°).</summary>
        public static DoodleArt S41I_Dome()
        {
            var p = new DoodlePainter(-1.6f, -1.6f, 1.6f, 1.6f, 90) { Ink = 0.045f };
            const float R = 1.35f, gap = 24f * 3.14159f / 180f;
            Func<float, float, bool> inGap = (x, y) => Math.Abs(Math.Atan2(y, x)) < gap;
            p.FillFunc(Sd.Circle(0, 0, R), (x, y) => Rgba.Hex("CFF3FF", 0.22f + 0.12f * S41I_Clamp((x + y) / 2.5f + 0.5f)));
            // bordo spesso del vetro, interrotto dall'apertura
            Sdf ring = Sd.Subtract(Sd.Circle(0, 0, R), Sd.Circle(0, 0, R - 0.16f));
            p.FillFunc(ring, (x, y) => inGap(x, y) ? Rgba.Clear : Rgba.Hex("A8E4F5", 0.95f));
            p.S41I_DomeRing(R, gap);
            // riflessi
            p.Line(Rgba.White.WithAlpha(0.85f), 0.08f, -0.95f, 0.45f, -0.75f, 0.8f, -0.45f, 1.0f);
            p.Line(Rgba.White.WithAlpha(0.6f), 0.05f, -0.9f, 0.15f, -0.88f, 0.3f);
            p.Shape(Sd.Circle(0, 0, 0.12f), Rgba.Hex("CFF3FF", 0.7f), 0.03f);
            // segnetti ai lati dell'apertura
            foreach (float s in new[] { -1f, 1f })
            {
                float a = s * gap;
                p.Fill(Sd.Circle(S41I_Cos(a) * (R - 0.08f), S41I_Sin(a) * (R - 0.08f), 0.09f), Rgba.Hex("FFD45A"));
            }
            return new DoodleArt("s41i_dome", p);
        }

        // ================= THE INVISIBLE INK =================

        /// <summary>Pergamena vuota con i bordi strappati (4.4 x 5.0, centro 0,0).</summary>
        public static DoodleArt S41I_BlankMap()
        {
            var p = new DoodlePainter(-2.3f, -2.62f, 2.3f, 2.62f, 60) { Ink = 0.05f };
            Sdf sheet = (x, y) =>
            {
                float edge = 0.04f * S41I_Sin(x * 13f + 1f) + 0.035f * S41I_Sin(y * 11f);
                return Sd.Box(0, 0, 2.2f, 2.5f, 0.06f)(x, y) + edge;
            };
            p.FillFunc(sheet, (x, y) =>
            {
                float n = 0.5f + 0.25f * S41I_Sin(x * 3.1f + y * 1.7f) + 0.25f * S41I_Sin(y * 4.3f - x * 2.2f);
                float edge = S41I_Clamp(1f - (-Sd.Box(0, 0, 2.2f, 2.5f)(x, y)) / 0.5f);
                return Rgba.Lerp(Rgba.Lerp(Rgba.Hex("FBF0D2"), Rgba.Hex("F1DDB0"), n), Rgba.Hex("D9B77E"), edge * 0.8f);
            });
            p.Fill(Sd.Circle(1.3f, -1.7f, 0.35f), Rgba.Hex("D9B77E", 0.25f));
            p.Shape(sheet, Rgba.Clear);
            return new DoodleArt("s41i_blank_map", p);
        }

        /// <summary>Asciugacapelli rosa col beccuccio in giù; la punta del beccuccio è in (0,0).</summary>
        public static DoodleArt S41I_Dryer()
        {
            var p = new DoodlePainter(-0.6f, -0.06f, 0.95f, 1.45f, 120) { Ink = 0.045f };
            p.Shape(Sd.Capsule(0.3f, 0.95f, 0.78f, 0.45f, 0.13f), Rgba.Hex("E2638C"));
            p.Shape(Sd.Box(0.62f, 0.57f, 0.06f, 0.04f, 0.02f), Rgba.Hex("FFD45A"), 0.02f);
            p.Shape(Sd.Polygon(-0.2f, 0.62f, 0.2f, 0.62f, 0.13f, 0.02f, -0.13f, 0.02f), Rgba.Hex("8D93A6"));
            for (float y = 0.12f; y < 0.55f; y += 0.12f) p.InkLine(0.02f, -0.15f + (0.62f - y) * 0.0f, y, 0.15f, y);
            Body(p, Sd.Circle(0, 1.0f, 0.42f), Rgba.Hex("FF8FB0"), Rgba.Hex("E2638C"));
            p.Shape(Sd.Circle(0, 1.0f, 0.22f), Rgba.Hex("FFD6E2"), 0.035f);
            for (int i = 0; i < 4; i++)
            {
                float a = i * 1.5708f + 0.4f;
                p.InkLine(0.025f, 0f, 1.0f, S41I_Cos(a) * 0.18f, 1.0f + S41I_Sin(a) * 0.18f);
            }
            p.Fill(Sd.Ellipse(-0.18f, 1.25f, 0.1f, 0.05f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("s41i_dryer", p);
        }

        /// <summary>Bruciacchiatura morbida (raggio 0.32; si sfuma con l'alfa).</summary>
        public static DoodleArt S41I_Scorch()
        {
            var p = new DoodlePainter(-0.36f, -0.36f, 0.36f, 0.36f, 100) { Ink = 0.03f };
            p.FillFunc(Sd.Circle(0, 0, 0.34f), (x, y) =>
            {
                float d = (float)Math.Sqrt(x * x + y * y) / 0.34f;
                return Rgba.Lerp(Rgba.Hex("3A1E0E"), Rgba.Hex("A8682E"), d).WithAlpha(S41I_Clamp(1.2f - d));
            });
            return new DoodleArt("s41i_scorch", p);
        }

        /// <summary>X del tesoro a inchiostro (0.6).</summary>
        public static DoodleArt S41I_InkX()
        {
            var p = new DoodlePainter(-0.35f, -0.35f, 0.35f, 0.35f, 160) { Ink = 0.03f };
            Rgba ink = Rgba.Hex("5A2A6E");
            p.Line(ink, 0.1f, -0.25f, -0.25f, 0.25f, 0.25f);
            p.Line(ink, 0.1f, -0.25f, 0.25f, 0.25f, -0.25f);
            return new DoodleArt("s41i_ink_x", p);
        }

        // ================= THE LAST DROP =================

        /// <summary>Bottiglia di plastica capovolta, vuota; l'origine è il punto d'appoggio in alto (fondo della bottiglia).
        /// Il tappo trasparente sta in y -2.55 (camera interna x ±0.3), il foro in (0, -2.72).</summary>
        public static DoodleArt S41I_Bottle()
        {
            var p = new DoodlePainter(-0.6f, -2.85f, 0.6f, 0.12f, 110) { Ink = 0.04f };
            Sdf body = Sd.Union(Sd.Box(0, -0.95f, 0.45f, 0.85f, 0.12f), Sd.Polygon(-0.45f, -1.6f, 0.45f, -1.6f, 0.17f, -2.15f, -0.17f, -2.15f),
                                Sd.Box(0, -2.27f, 0.17f, 0.16f));
            p.FillFunc(body, (x, y) => Rgba.Hex("BFEFFF", 0.3f + 0.15f * S41I_Clamp(0.5f + x)));
            p.Fill(Sd.Box(0, -1.0f, 0.45f, 0.22f), Rgba.Hex("6FC0F0", 0.85f));
            p.Fill(Sd.Box(0, -1.0f, 0.3f, 0.1f, 0.05f), Rgba.White.WithAlpha(0.8f));
            for (float y = -0.35f; y > -0.6f; y -= 0.12f) p.Line(Rgba.Hex("8FD3F0", 0.8f), 0.025f, -0.42f, y, 0.42f, y);
            p.Line(Rgba.White.WithAlpha(0.8f), 0.06f, -0.3f, -0.3f, -0.3f, -0.7f);
            p.Line(Rgba.White.WithAlpha(0.8f), 0.05f, -0.3f, -1.3f, -0.3f, -1.55f);
            p.Shape(body, Rgba.Clear);
            // tappo sportivo trasparente, largo
            Sdf cap = Sd.Box(0, -2.55f, 0.4f, 0.16f, 0.07f);
            p.FillFunc(cap, (x, y) => Rgba.Hex("FF8FB0", 0.4f));
            p.Shape(Sd.Subtract(cap, Sd.Box(0, -2.55f, 0.32f, 0.09f, 0.04f)), Rgba.Hex("FF6FA0", 0.9f), 0.035f);
            p.Shape(Sd.Box(-0.18f, -2.6f, 0.025f, 0.05f), Rgba.Hex("FF6FA0"), 0.02f);   // scalino che trattiene la goccia
            p.Fill(Sd.Box(0, -2.7f, 0.05f, 0.04f), Ink);
            return new DoodleArt("s41i_bottle", p);
        }
    }

    static class S41IPainterExt
    {
        /// <summary>Contorno a inchiostro dell'anello della campana, con le due estremità tonde ai lati dell'apertura.</summary>
        public static void S41I_DomeRing(this DoodlePainter p, float r, float gap)
        {
            Sdf outer = (x, y) =>
            {
                double a = Math.Atan2(y, x);
                if (Math.Abs(a) < gap) return 1f;
                return Math.Abs((float)Math.Sqrt(x * x + y * y) - r) - 0.02f;
            };
            Sdf inner = (x, y) =>
            {
                double a = Math.Atan2(y, x);
                if (Math.Abs(a) < gap) return 1f;
                return Math.Abs((float)Math.Sqrt(x * x + y * y) - (r - 0.16f)) - 0.015f;
            };
            p.Fill(outer, DoodlePainter.DefaultInk, false);
            p.Fill(inner, DoodlePainter.DefaultInk, false);
            foreach (float s in new[] { -1f, 1f })
            {
                float cx = (float)Math.Cos(s * gap) * (r - 0.08f), cy = (float)Math.Sin(s * gap) * (r - 0.08f);
                p.Shape(Sd.Circle(cx, cy, 0.08f), Rgba.Hex("A8E4F5"), 0.035f);
            }
        }
    }
}
