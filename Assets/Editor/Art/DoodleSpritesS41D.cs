using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli della specifica 105-200, pacchetto D.</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV41D() => new DoodleArt[]
        {
            // The fourth wall
            S41D_StageBg(), S41D_FrameBar(true), S41D_FrameBar(false), S41D_FrameCanvas(), S41D_Actor(),
            // Hot potato
            S41D_Buddy(), S41D_Potato(), S41D_Mash(), S41D_Signal(),
            // Shake to undo
            S41D_Painter(), S41D_Masterpiece(), S41D_Pants(0), S41D_Pants(1), S41D_Pants(2), S41D_UndoButton(),
            // The lonely cloud
            S41D_MeadowBg(), S41D_Puff(true), S41D_Puff(false), S41D_CloudFace(false), S41D_CloudFace(true),
            S41D_Flower(false), S41D_Flower(true), S41D_Raindrop(),
            // Ghost selfie
            S41D_Ghost(false), S41D_Ghost(true), S41D_Viewfinder(), S41D_Shutter(), S41D_Polaroid(),
            // Magnet mayhem
            S41D_MazeBg(), S41D_MagnetKey(), S41D_Magnet(true), S41D_Magnet(false), S41D_Zap(), S41D_LockPlate(false), S41D_LockPlate(true),
            // The fake crash
            S41D_CrashWindow(), S41D_CrashFace(), S41D_CrashButton(),
            // Rainbow thief
            S41D_RainbowBg(), S41D_Band(0), S41D_Band(1), S41D_Band(2), S41D_Band(3), S41D_Band(4), S41D_Band(5), S41D_Band(6),
            S41D_Crate(), S41D_Orb(), S41D_Venn(),
        };

        static float S41D_Sin(float v) => (float)Math.Sin(v);
        static float S41D_Cos(float v) => (float)Math.Cos(v);
        static float S41D_Clamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        /// <summary>Due occhietti tondi con pupilla e lucina.</summary>
        static void S41D_Eyes(DoodlePainter p, float cx, float cy, float gap, float r, float lookX = 0f, float lookY = 0f)
        {
            foreach (float s in new[] { -1f, 1f })
            {
                float x = cx + s * gap;
                p.Shape(Sd.Circle(x, cy, r), Rgba.White, r * 0.35f);
                p.Fill(Sd.Circle(x + lookX * r * 0.35f, cy + lookY * r * 0.35f, r * 0.5f), Ink);
                p.Fill(Sd.Circle(x + lookX * r * 0.35f - r * 0.18f, cy + lookY * r * 0.35f + r * 0.2f, r * 0.17f), Rgba.White);
            }
        }

        // ================= THE FOURTH WALL =================

        /// <summary>Teatro: parete scura, sipari rossi ai lati, palco di legno con il bordo in y -2.6.</summary>
        public static DoodleArt S41D_StageBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("3A2140"), Rgba.Hex("5C3360"), S41D_Clamp((y + 2.6f) / 7f)));
            // occhio di bue sulla parete
            p.FillFunc(Sd.Circle(0, 0.4f, 3.0f), (x, y) =>
            {
                float d = (float)Math.Sqrt(x * x + (y - 0.4f) * (y - 0.4f)) / 3f;
                return Rgba.Hex("FFE9A8", 0.28f * (1f - d) * (1f - d));
            });
            // palco
            p.FillFunc(Sd.Box(0, -3.95f, 3.3f, 1.35f), (x, y) => Rgba.Lerp(Rgba.Hex("8A5128"), Rgba.Hex("B9733A"), S41D_Clamp((y + 5.2f) / 2.6f)));
            for (float y = -2.95f; y > -5.3f; y -= 0.45f) p.Line(Rgba.Hex("6E3E1C"), 0.03f, -3.3f, y, 3.3f, y);
            p.Shape(Sd.Box(0, -2.6f, 3.3f, 0.07f), Rgba.Hex("D99A55"), 0.04f);
            // sipari
            foreach (float s in new[] { -1f, 1f })
            {
                Sdf cur = Sd.Polygon(s * 3.3f, 5.3f, s * 1.9f, 5.3f, s * 2.25f, 1.2f, s * 2.0f, -2.6f, s * 3.3f, -2.6f);
                p.FillFunc(cur, (x, y) => Rgba.Lerp(Rgba.Hex("B8202E"), Rgba.Hex("E2394A"), 0.5f + 0.5f * S41D_Sin(x * 9f)));
                for (int k = 0; k < 4; k++)
                {
                    float x0 = s * (2.15f + k * 0.28f);
                    p.Line(Rgba.Hex("7E1220", 0.7f), 0.04f, x0, 5.2f, x0 + s * 0.08f, 1.2f, x0 - s * 0.05f, -2.6f);
                }
                p.Shape(cur, Rgba.Clear);
                // cordone dorato
                p.Shape(Sd.Capsule(s * 2.05f, 0.6f, s * 3.3f, 0.3f, 0.07f), Gold, 0.03f);
                p.Shape(Sd.Circle(s * 2.1f, 0.55f, 0.13f), Gold, 0.03f);
            }
            // mantovana in alto
            Sdf val = Sd.Union(Sd.Box(0, 4.9f, 3.3f, 0.4f), Sd.Circle(-1.6f, 4.5f, 0.45f), Sd.Circle(-0.55f, 4.45f, 0.45f), Sd.Circle(0.55f, 4.45f, 0.45f), Sd.Circle(1.6f, 4.5f, 0.45f));
            p.Shape(val, Rgba.Hex("C92A3A"));
            p.Line(GoldShade, 0.06f, -3.2f, 4.2f, 3.2f, 4.2f);
            return new DoodleArt("s41d_stage_bg", p);
        }

        /// <summary>Lato della cornice dorata: orizzontale (lungo 3.7, x ±1.85) o verticale (alto 2.7, y ±1.35); spessore 0.3.</summary>
        public static DoodleArt S41D_FrameBar(bool horizontal)
        {
            float hx = horizontal ? 1.85f : 0.15f, hy = horizontal ? 0.15f : 1.35f;
            var p = new DoodlePainter(-hx - 0.06f, -hy - 0.06f, hx + 0.06f, hy + 0.06f, 120) { Ink = 0.04f };
            Sdf bar = Sd.Box(0, 0, hx, hy, 0.03f);
            p.FillFunc(bar, (x, y) =>
            {
                float t = horizontal ? (y + hy) / (2f * hy) : (x + hx) / (2f * hx);
                return Rgba.Lerp(GoldShade, Gold, 0.4f + 0.6f * S41D_Sin(t * 3.1416f));
            });
            // decori a riccioli
            float len = horizontal ? hx : hy;
            for (float u = -len + 0.3f; u < len - 0.2f; u += 0.42f)
            {
                float x = horizontal ? u : 0f, y = horizontal ? 0f : u;
                p.Fill(Sd.Circle(x, y, 0.06f), GoldLight);
                p.Shape(Sd.Circle(x, y, 0.06f), Rgba.Clear, 0.02f);
            }
            p.Line(GoldLight.WithAlpha(0.7f), 0.025f, horizontal ? -hx + 0.08f : -0.07f, horizontal ? 0.08f : -hy + 0.08f, horizontal ? hx - 0.08f : -0.07f, horizontal ? 0.08f : hy - 0.08f);
            p.Shape(bar, Rgba.Clear);
            return new DoodleArt(horizontal ? "s41d_frame_h" : "s41d_frame_v", p);
        }

        /// <summary>Paesaggio dipinto dentro la cornice (3.4 x 3.0, centro 0,0; pavimento dipinto in y -1.2).</summary>
        public static DoodleArt S41D_FrameCanvas()
        {
            var p = new DoodlePainter(-1.72f, -1.52f, 1.72f, 1.52f, 80) { Ink = 0.04f };
            p.FillFunc(Sd.Box(0, 0, 1.72f, 1.52f), (x, y) => Rgba.Lerp(Rgba.Hex("FFE2B8"), Rgba.Hex("9FD3F0"), S41D_Clamp((y + 0.6f) / 2f)));
            p.Shape(Sd.Circle(1.0f, 0.85f, 0.3f), Rgba.Hex("FFD45A"), 0.03f);
            p.Shape(Sd.Union(Sd.Circle(-1.4f, -1.0f, 1.1f), Sd.Circle(0.2f, -1.4f, 1.0f)), Rgba.Hex("8CCB6E"), 0.03f);
            p.Shape(Sd.Circle(1.6f, -1.3f, 0.9f), Rgba.Hex("6FB55A"), 0.03f);
            p.Shape(Sd.Box(-1.05f, -0.35f, 0.05f, 0.3f), Wood, 0.025f);
            p.Shape(Sd.Circle(-1.05f, 0.05f, 0.28f), Rgba.Hex("4FA34A"), 0.03f);
            p.Fill(Sd.Box(0, -1.36f, 1.72f, 0.16f), Rgba.Hex("C99A63"), false);
            p.Line(Rgba.Hex("8A6238"), 0.025f, -1.72f, -1.2f, 1.72f, -1.2f);
            // pennellate
            for (int i = 0; i < 14; i++)
            {
                float x = -1.6f + i * 0.24f, y = 1.2f - (i % 3) * 0.12f;
                p.Line(Rgba.White.WithAlpha(0.25f), 0.03f, x, y, x + 0.15f, y + 0.04f);
            }
            return new DoodleArt("s41d_frame_canvas", p);
        }

        /// <summary>Attore con farfallino e maglia a righe (piedi in y 0, alto 1.1).</summary>
        public static DoodleArt S41D_Actor()
        {
            var p = new DoodlePainter(-0.5f, -0.06f, 0.5f, 1.2f, 160) { Ink = 0.035f };
            // gambe e scarpe
            p.Shape(Sd.Capsule(-0.1f, 0.08f, -0.1f, 0.35f, 0.06f), Rgba.Hex("2E2A3A"));
            p.Shape(Sd.Capsule(0.1f, 0.08f, 0.1f, 0.35f, 0.06f), Rgba.Hex("2E2A3A"));
            p.Shape(Sd.Ellipse(-0.13f, 0.05f, 0.1f, 0.05f), Rgba.Hex("1E1A26"));
            p.Shape(Sd.Ellipse(0.13f, 0.05f, 0.1f, 0.05f), Rgba.Hex("1E1A26"));
            // braccia aperte
            p.Shape(Sd.Capsule(-0.18f, 0.6f, -0.38f, 0.78f, 0.05f), Rgba.White);
            p.Shape(Sd.Capsule(0.18f, 0.6f, 0.38f, 0.78f, 0.05f), Rgba.White);
            p.Shape(Sd.Circle(-0.4f, 0.8f, 0.06f), Rgba.White);
            p.Shape(Sd.Circle(0.4f, 0.8f, 0.06f), Rgba.White);
            // busto a righe
            Sdf torso = Sd.Box(0, 0.52f, 0.19f, 0.2f, 0.08f);
            p.Fill(torso, Rgba.White);
            for (float y = 0.36f; y < 0.72f; y += 0.09f) p.Fill(Sd.Intersect(torso, Sd.Box(0, y, 0.3f, 0.025f)), Rgba.Hex("2E2A3A"));
            p.Shape(torso, Rgba.Clear);
            // testa da mimo
            p.Shape(Sd.Circle(0, 0.9f, 0.2f), Rgba.Hex("FFF6EE"));
            p.Fill(Sd.Circle(-0.09f, 0.82f, 0.04f), BlushColor.WithAlpha(0.7f));
            p.Fill(Sd.Circle(0.09f, 0.82f, 0.04f), BlushColor.WithAlpha(0.7f));
            p.Fill(Sd.Circle(-0.07f, 0.93f, 0.028f), Ink);
            p.Fill(Sd.Circle(0.07f, 0.93f, 0.028f), Ink);
            p.InkLine(0.02f, -0.12f, 1.0f, -0.03f, 1.02f);
            p.InkLine(0.02f, 0.03f, 1.02f, 0.12f, 1.0f);
            p.Shape(Sd.Ellipse(0, 0.8f, 0.04f, 0.03f), Rgba.Hex("D8434F"), 0.015f);
            // basco e farfallino
            p.Shape(Sd.Ellipse(0.03f, 1.08f, 0.17f, 0.06f), Rgba.Hex("2E2A3A"));
            p.Shape(Sd.Polygon(0f, 0.71f, -0.1f, 0.76f, -0.1f, 0.66f), Rgba.Hex("D8434F"), 0.015f);
            p.Shape(Sd.Polygon(0f, 0.71f, 0.1f, 0.76f, 0.1f, 0.66f), Rgba.Hex("D8434F"), 0.015f);
            return new DoodleArt("s41d_actor", p);
        }

        // ================= HOT POTATO =================

        /// <summary>Omino tondo bianco (si tinge), braccia in su; piedi in y 0, alto 1.3.</summary>
        public static DoodleArt S41D_Buddy()
        {
            var p = new DoodlePainter(-0.72f, -0.06f, 0.72f, 1.42f, 140) { Ink = 0.045f };
            p.Shape(Sd.Ellipse(-0.22f, 0.06f, 0.16f, 0.07f), Rgba.Hex("E8E8EE"));
            p.Shape(Sd.Ellipse(0.22f, 0.06f, 0.16f, 0.07f), Rgba.Hex("E8E8EE"));
            p.Shape(Sd.Capsule(-0.38f, 0.75f, -0.6f, 1.1f, 0.07f), Rgba.White);
            p.Shape(Sd.Capsule(0.38f, 0.75f, 0.6f, 1.1f, 0.07f), Rgba.White);
            p.Shape(Sd.Circle(-0.62f, 1.14f, 0.09f), Rgba.White);
            p.Shape(Sd.Circle(0.62f, 1.14f, 0.09f), Rgba.White);
            Sdf body = Sd.Ellipse(0, 0.62f, 0.48f, 0.55f);
            Body(p, body, Rgba.White, Rgba.Hex("D6D6E0"));
            S41D_Eyes(p, 0, 0.78f, 0.15f, 0.1f, 0f, 0.5f);
            p.Shape(Sd.Ellipse(0, 0.5f, 0.12f, 0.08f), Rgba.Hex("5A2A3A"), 0.03f);
            p.Fill(Sd.Ellipse(0, 0.46f, 0.07f, 0.03f), Rgba.Hex("FF8FA0"));
            p.Fill(Sd.Circle(-0.3f, 0.58f, 0.06f), BlushColor.WithAlpha(0.6f));
            p.Fill(Sd.Circle(0.3f, 0.58f, 0.06f), BlushColor.WithAlpha(0.6f));
            return new DoodleArt("s41d_buddy", p);
        }

        /// <summary>Patata bollente con vapore (centro 0,0, larga 0.7).</summary>
        public static DoodleArt S41D_Potato()
        {
            var p = new DoodlePainter(-0.42f, -0.32f, 0.42f, 0.6f, 180) { Ink = 0.035f };
            Sdf pot = Sd.Rotate(Sd.Ellipse(0, 0, 0.36f, 0.26f), 0, 0, 12f);
            p.FillFunc(pot, (x, y) => Rgba.Lerp(Rgba.Hex("FF6A2A"), Rgba.Hex("E2A35A"), S41D_Clamp((y + 0.26f) / 0.52f + x * 0.4f)));
            foreach (var d in new[] { (-0.15f, 0.08f), (0.12f, -0.1f), (0.2f, 0.1f), (-0.05f, -0.12f) })
                p.Fill(Sd.Circle(d.Item1, d.Item2, 0.025f), Rgba.Hex("9A5A2A"));
            p.Fill(Sd.Ellipse(-0.12f, 0.12f, 0.1f, 0.04f), Rgba.White.WithAlpha(0.35f));
            p.Shape(pot, Rgba.Clear);
            // occhi arrabbiati
            p.InkLine(0.025f, -0.14f, 0.07f, -0.05f, 0.03f);
            p.InkLine(0.025f, 0.14f, 0.07f, 0.05f, 0.03f);
            p.Fill(Sd.Circle(-0.08f, 0f, 0.03f), Ink);
            p.Fill(Sd.Circle(0.08f, 0f, 0.03f), Ink);
            // vapore
            foreach (float x in new[] { -0.15f, 0.02f, 0.18f })
                p.Line(Rgba.White.WithAlpha(0.85f), 0.035f, x, 0.3f, x + 0.05f, 0.38f, x - 0.04f, 0.46f, x + 0.03f, 0.55f);
            return new DoodleArt("s41d_potato", p);
        }

        /// <summary>Purè spiaccicato (centro 0,0, largo 1.8).</summary>
        public static DoodleArt S41D_Mash()
        {
            var p = new DoodlePainter(-1f, -0.6f, 1f, 0.6f, 110) { Ink = 0.04f };
            var parts = new List<Sdf> { Sd.Ellipse(0, 0, 0.6f, 0.35f) };
            for (int i = 0; i < 9; i++)
            {
                float a = i / 9f * 6.283f + 0.3f;
                parts.Add(Sd.Circle(S41D_Cos(a) * 0.65f, S41D_Sin(a) * 0.38f, 0.14f + (i % 3) * 0.04f));
            }
            Sdf splat = Sd.Union(parts.ToArray());
            Body(p, splat, Rgba.Hex("FFE7A3"), Rgba.Hex("E9C574"));
            p.Shape(Sd.Box(0.05f, 0.06f, 0.16f, 0.1f, 0.03f), Rgba.Hex("FFD447"), 0.03f);
            p.Fill(Sd.Circle(-0.85f, 0.4f, 0.06f), Rgba.Hex("FFE7A3"));
            p.Fill(Sd.Circle(0.88f, -0.42f, 0.05f), Rgba.Hex("FFE7A3"));
            return new DoodleArt("s41d_mash", p);
        }

        /// <summary>Spia luminosa bianca (si tinge; raggio 0.16).</summary>
        public static DoodleArt S41D_Signal()
        {
            var p = new DoodlePainter(-0.2f, -0.2f, 0.2f, 0.2f, 200) { Ink = 0.035f };
            p.Shape(Sd.Circle(0, 0, 0.16f), Rgba.White);
            p.Fill(Sd.Circle(-0.05f, 0.05f, 0.045f), Rgba.White.WithAlpha(0.9f));
            return new DoodleArt("s41d_signal", p);
        }

        // ================= SHAKE TO UNDO =================

        /// <summary>Pittore col basco e i baffi, pennello in mano (piedi in y 0, alto 2.3).</summary>
        public static DoodleArt S41D_Painter()
        {
            var p = new DoodlePainter(-0.85f, -0.06f, 0.95f, 2.4f, 110) { Ink = 0.045f };
            p.Shape(Sd.Ellipse(-0.2f, 0.07f, 0.2f, 0.08f), Rgba.Hex("5A3A2A"));
            p.Shape(Sd.Ellipse(0.22f, 0.07f, 0.2f, 0.08f), Rgba.Hex("5A3A2A"));
            p.Shape(Sd.Box(-0.16f, 0.35f, 0.1f, 0.25f), Rgba.Hex("3E4A6E"));
            p.Shape(Sd.Box(0.16f, 0.35f, 0.1f, 0.25f), Rgba.Hex("3E4A6E"));
            // camice con macchie di colore
            Sdf smock = Sd.Polygon(-0.45f, 0.55f, 0.45f, 0.55f, 0.36f, 1.45f, -0.36f, 1.45f);
            Body(p, smock, Rgba.Hex("F4F1E8"), Rgba.Hex("D9D3C2"));
            foreach (var s in new[] { (-0.2f, 0.8f, "E8433F"), (0.15f, 1.1f, "3E8EF7"), (0.2f, 0.7f, "FFC93C"), (-0.1f, 1.25f, "5BBF4A") })
                p.Fill(Sd.Circle(s.Item1, s.Item2, 0.06f), Rgba.Hex(s.Item3));
            p.Shape(Sd.Polygon(0f, 1.4f, -0.12f, 1.47f, -0.12f, 1.33f), Rgba.Hex("E8433F"), 0.02f);
            p.Shape(Sd.Polygon(0f, 1.4f, 0.12f, 1.47f, 0.12f, 1.33f), Rgba.Hex("E8433F"), 0.02f);
            // braccio col pennello
            p.Shape(Sd.Capsule(0.35f, 1.3f, 0.7f, 1.05f, 0.08f), Rgba.Hex("F4F1E8"));
            p.Shape(Sd.Capsule(0.68f, 0.98f, 0.88f, 1.5f, 0.03f), Wood);
            p.Shape(Sd.Capsule(0.87f, 1.48f, 0.9f, 1.58f, 0.045f), Rgba.Hex("E8433F"), 0.025f);
            p.Shape(Sd.Circle(0.7f, 1.05f, 0.08f), Rgba.Hex("FFD9B8"));
            // tavolozza
            p.Shape(Sd.Capsule(-0.35f, 1.25f, -0.6f, 0.95f, 0.08f), Rgba.Hex("F4F1E8"));
            Sdf pal = Sd.Subtract(Sd.Ellipse(-0.62f, 0.85f, 0.24f, 0.17f), Sd.Circle(-0.52f, 0.8f, 0.04f));
            p.Shape(pal, Wood);
            foreach (var c in new[] { (-0.75f, 0.9f, "E8433F"), (-0.62f, 0.95f, "3E8EF7"), (-0.7f, 0.78f, "FFC93C") })
                p.Fill(Sd.Circle(c.Item1, c.Item2, 0.035f), Rgba.Hex(c.Item3));
            // testa
            p.Shape(Sd.Circle(0, 1.78f, 0.34f), Rgba.Hex("FFD9B8"));
            p.Fill(Sd.Circle(-0.18f, 1.68f, 0.06f), BlushColor.WithAlpha(0.5f));
            p.Fill(Sd.Circle(0.18f, 1.68f, 0.06f), BlushColor.WithAlpha(0.5f));
            p.Fill(Sd.Circle(-0.11f, 1.85f, 0.035f), Ink);
            p.Fill(Sd.Circle(0.11f, 1.85f, 0.035f), Ink);
            p.Shape(Sd.Circle(0, 1.74f, 0.06f), Rgba.Hex("FFB89A"), 0.02f);
            // baffi a manubrio
            p.InkLine(0.04f, -0.02f, 1.66f, -0.14f, 1.63f, -0.24f, 1.7f);
            p.InkLine(0.04f, 0.02f, 1.66f, 0.14f, 1.63f, 0.24f, 1.7f);
            // basco
            p.Shape(Sd.Ellipse(-0.05f, 2.07f, 0.38f, 0.13f), Rgba.Hex("C0304A"));
            p.Shape(Sd.Capsule(-0.05f, 2.2f, -0.05f, 2.27f, 0.025f), Rgba.Hex("C0304A"), 0.02f);
            return new DoodleArt("s41d_painter", p);
        }

        /// <summary>Cavalletto con il capolavoro (tela centrata in 0,0.4, 2.5 x 1.9).</summary>
        public static DoodleArt S41D_Masterpiece()
        {
            var p = new DoodlePainter(-1.45f, -1.75f, 1.45f, 1.6f, 90) { Ink = 0.05f };
            // gambe del cavalletto
            p.Shape(Sd.Capsule(-0.9f, -1.65f, -0.35f, 1.5f, 0.06f), Wood);
            p.Shape(Sd.Capsule(0.9f, -1.65f, 0.35f, 1.5f, 0.06f), Wood);
            p.Shape(Sd.Capsule(0f, -1.5f, 0f, 1.5f, 0.05f), WoodShade);
            p.Shape(Sd.Box(0, -0.6f, 1.3f, 0.07f, 0.03f), Wood);
            // tela: tramonto sul lago
            Sdf canvas = Sd.Box(0, 0.4f, 1.25f, 0.95f, 0.02f);
            p.FillFunc(canvas, (x, y) => Rgba.Lerp(Rgba.Hex("FFB36B"), Rgba.Hex("8C7BD6"), S41D_Clamp((y - 0.1f) / 1.2f)));
            p.Fill(Sd.Intersect(canvas, Sd.Circle(0.35f, 0.35f, 0.42f)), Rgba.Hex("FFE36A"));
            p.Fill(Sd.Intersect(canvas, Sd.Polygon(-1.3f, 0.1f, -0.6f, 0.95f, 0.05f, 0.1f)), Rgba.Hex("6A5AA8"));
            p.Fill(Sd.Intersect(canvas, Sd.Polygon(-0.3f, 0.1f, 0.6f, 0.75f, 1.3f, 0.1f)), Rgba.Hex("7D6CC0"));
            p.Fill(Sd.Intersect(canvas, Sd.Box(0, -0.2f, 1.3f, 0.32f)), Rgba.Hex("4F8FD6"));
            for (int i = 0; i < 5; i++) p.Line(Rgba.Hex("FFE36A", 0.8f), 0.04f, 0.1f + i * 0.04f, 0.0f - i * 0.1f, 0.6f - i * 0.04f, 0.0f - i * 0.1f);
            p.Shape(Sd.Box(-0.6f, -0.35f, 0.12f, 0.03f), Rgba.Hex("8A5128"), 0.02f);
            p.Shape(canvas, Rgba.Clear, 0.08f);
            // firma
            p.Line(Ink.WithAlpha(0.8f), 0.02f, 0.75f, -0.42f, 0.85f, -0.35f, 0.92f, -0.44f, 1.05f, -0.36f);
            return new DoodleArt("s41d_masterpiece", p);
        }

        /// <summary>Tratti delle mutande giganti sulla tela (0 contorno, 1 riempimento rosa, 2 cuoricini ed elastico).</summary>
        public static DoodleArt S41D_Pants(int stroke)
        {
            var p = new DoodlePainter(-1.2f, -0.6f, 1.2f, 1.4f, 90) { Ink = 0.05f, Wobble = 0.03f };
            float[] pts = { -0.85f, 1.12f, 0.85f, 1.12f, 0.92f, 0.42f, 0.6f, 0.02f, 0.22f, -0.38f, -0.22f, -0.38f, -0.6f, 0.02f, -0.92f, 0.42f };
            Sdf shape = Sd.Polygon(pts);
            if (stroke == 0)
            {
                var loop = new List<float>(pts) { pts[0], pts[1] };
                p.Line(Rgba.Hex("2A2030"), 0.1f, loop.ToArray());
                p.Line(Rgba.Hex("2A2030"), 0.07f, -0.45f, 0.2f, -0.3f, 0.45f);
                p.Line(Rgba.Hex("2A2030"), 0.07f, 0.45f, 0.2f, 0.3f, 0.45f);
            }
            else if (stroke == 1)
            {
                p.Fill(shape, Rgba.Hex("FF8FC0", 0.92f));
                p.Hatch(shape, Rgba.Hex("FF5FA0", 0.6f), 0.13f, 0.05f, 30f);
            }
            else
            {
                p.Fill(Sd.Box(0, 1.0f, 0.86f, 0.1f), Rgba.Hex("FFFFFF", 0.95f));
                for (float x = -0.75f; x < 0.8f; x += 0.17f) p.Line(Rgba.Hex("3E8EF7"), 0.035f, x, 0.93f, x + 0.06f, 1.07f);
                foreach (var h in new[] { (-0.5f, 0.6f), (0.1f, 0.7f), (0.55f, 0.5f), (-0.2f, 0.2f), (0.25f, 0.15f), (0f, -0.18f), (-0.65f, 0.3f) })
                {
                    Sdf heart = Sd.Union(Sd.Circle(h.Item1 - 0.05f, h.Item2 + 0.03f, 0.06f), Sd.Circle(h.Item1 + 0.05f, h.Item2 + 0.03f, 0.06f),
                        Sd.Polygon(h.Item1 - 0.105f, h.Item2 + 0.01f, h.Item1 + 0.105f, h.Item2 + 0.01f, h.Item1, h.Item2 - 0.12f));
                    p.Fill(heart, Rgba.Hex("E8233F"));
                }
            }
            return new DoodleArt("s41d_pants_" + stroke, p);
        }

        /// <summary>Pulsante UNDO crepato e col cerotto (centro 0,0, 1.1 x 0.7).</summary>
        public static DoodleArt S41D_UndoButton()
        {
            var p = new DoodlePainter(-0.6f, -0.4f, 0.6f, 0.4f, 160) { Ink = 0.04f };
            Sdf b = Sd.Box(0, 0, 0.52f, 0.3f, 0.12f);
            Body(p, b, Rgba.Hex("B9C4D6"), Rgba.Hex("8E9AB0"));
            // freccia curva
            p.Line(Ink, 0.05f, 0.22f, -0.08f, 0.22f, 0.05f, 0.1f, 0.13f, -0.15f, 0.13f);
            p.Shape(Sd.Polygon(-0.27f, 0.13f, -0.13f, 0.24f, -0.13f, 0.02f), Ink, 0.01f);
            // crepa e cerotto
            p.Line(Ink, 0.025f, -0.35f, -0.3f, -0.25f, -0.12f, -0.33f, -0.02f, -0.2f, 0.12f);
            Sdf band = Sd.Rotate(Sd.Box(0.3f, -0.15f, 0.2f, 0.06f, 0.04f), 0.3f, -0.15f, 30f);
            p.Shape(band, Rgba.Hex("F3C99A"), 0.02f);
            p.Fill(Sd.Circle(0.3f, -0.15f, 0.015f), Rgba.Hex("C99A6A"));
            return new DoodleArt("s41d_undo_btn", p);
        }

        // ================= THE LONELY CLOUD =================

        /// <summary>Prato con cielo azzurro; il terreno comincia in y -2.9.</summary>
        public static DoodleArt S41D_MeadowBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("D9F1FF"), Rgba.Hex("7FC4F0"), S41D_Clamp((y + 2f) / 7f)));
            p.Shape(Sd.Union(Sd.Circle(-2.4f, -3.1f, 1.6f), Sd.Circle(2.6f, -3.0f, 1.8f)), Rgba.Hex("A5D98A"));
            p.FillFunc(Sd.Box(0, -4.2f, 3.3f, 1.3f), (x, y) => Rgba.Lerp(Rgba.Hex("6FB55A"), Rgba.Hex("8CCB6E"), S41D_Clamp((y + 5.2f) / 2.3f)));
            p.Line(Rgba.Hex("4F8F42"), 0.05f, -3.3f, -2.9f, 3.3f, -2.9f);
            for (int i = 0; i < 22; i++)
            {
                float x = -3f + i * 0.29f, y = -3.4f - (i * 37 % 13) * 0.12f;
                p.Line(Rgba.Hex("4F8F42"), 0.03f, x, y, x + 0.05f, y + 0.16f);
                p.Line(Rgba.Hex("4F8F42"), 0.03f, x + 0.1f, y, x + 0.07f, y + 0.13f);
            }
            // staccionata
            for (float x = -3.0f; x < 3.2f; x += 0.55f) p.Shape(Sd.Box(x, -2.55f, 0.07f, 0.4f, 0.03f), Rgba.Hex("F4E6CC"), 0.035f);
            p.Shape(Sd.Box(0, -2.4f, 3.3f, 0.05f), Rgba.Hex("F4E6CC"), 0.035f);
            // sole timido dietro le colline
            p.Fill(Sd.Circle(2.3f, 3.3f, 0.55f), Rgba.Hex("FFE58A", 0.6f));
            return new DoodleArt("s41d_meadow_bg", p);
        }

        /// <summary>Sbuffo della nuvola: contorno scuro (raggio 0.5) o riempimento bianco (raggio 0.45). Si sovrappongono.</summary>
        public static DoodleArt S41D_Puff(bool ink)
        {
            var p = new DoodlePainter(-0.54f, -0.54f, 0.54f, 0.54f, 140) { Wobble = 0.01f };
            if (ink) p.Fill(Sd.Circle(0, 0, 0.5f), Ink);
            else p.FillFunc(Sd.Circle(0, 0, 0.45f), (x, y) => Rgba.Lerp(Rgba.Hex("D3DCEA"), Rgba.White, S41D_Clamp((y + 0.35f) / 0.5f)));
            return new DoodleArt(ink ? "s41d_puff_ink" : "s41d_puff", p);
        }

        /// <summary>Faccina della nuvola: triste con lacrimoni o felice (centro 0,0).</summary>
        public static DoodleArt S41D_CloudFace(bool happy)
        {
            var p = new DoodlePainter(-0.5f, -0.4f, 0.5f, 0.3f, 160) { Ink = 0.035f };
            if (happy)
            {
                p.InkLine(0.04f, -0.25f, 0.08f, -0.18f, 0.15f, -0.11f, 0.08f);
                p.InkLine(0.04f, 0.11f, 0.08f, 0.18f, 0.15f, 0.25f, 0.08f);
                p.Shape(Sd.Intersect(Sd.Circle(0, -0.02f, 0.14f), Sd.Box(0, -0.12f, 0.2f, 0.1f)), Rgba.Hex("D8434F"), 0.03f);
            }
            else
            {
                p.InkLine(0.04f, -0.25f, 0.12f, -0.18f, 0.06f, -0.11f, 0.12f);
                p.InkLine(0.04f, 0.11f, 0.12f, 0.18f, 0.06f, 0.25f, 0.12f);
                p.InkLine(0.035f, -0.1f, -0.12f, 0f, -0.05f, 0.1f, -0.12f);
                p.Shape(Sd.Capsule(-0.18f, 0.0f, -0.2f, -0.3f, 0.04f), Rgba.Hex("7FC8FF"), 0.02f);
                p.Shape(Sd.Capsule(0.18f, 0.0f, 0.2f, -0.3f, 0.04f), Rgba.Hex("7FC8FF"), 0.02f);
            }
            p.Fill(Sd.Circle(-0.33f, -0.02f, 0.06f), BlushColor.WithAlpha(0.55f));
            p.Fill(Sd.Circle(0.33f, -0.02f, 0.06f), BlushColor.WithAlpha(0.55f));
            return new DoodleArt(happy ? "s41d_cloud_happy" : "s41d_cloud_sad", p);
        }

        /// <summary>Fiore nel vaso: appassito o fiorito (base del vaso in y 0, vaso alto 0.7).</summary>
        public static DoodleArt S41D_Flower(bool happy)
        {
            var p = new DoodlePainter(-0.75f, -0.06f, 0.75f, 2.0f, 130) { Ink = 0.04f };
            Rgba stem = happy ? Rgba.Hex("4FAF45") : Rgba.Hex("9AA05A");
            if (happy)
            {
                p.Line(Ink, 0.1f, 0f, 0.65f, 0.02f, 1.1f, 0f, 1.5f);
                p.Line(stem, 0.06f, 0f, 0.65f, 0.02f, 1.1f, 0f, 1.5f);
                p.Shape(Sd.Rotate(Sd.Ellipse(0.2f, 1.0f, 0.18f, 0.07f), 0.02f, 1.0f, 25f), Rgba.Hex("6FC85A"));
                p.Shape(Sd.Rotate(Sd.Ellipse(-0.18f, 1.15f, 0.16f, 0.06f), 0.02f, 1.15f, -25f), Rgba.Hex("6FC85A"));
                for (int i = 0; i < 8; i++)
                {
                    float a = i / 8f * 6.283f;
                    p.Shape(Sd.Circle(S41D_Cos(a) * 0.24f, 1.6f + S41D_Sin(a) * 0.24f, 0.13f), Rgba.Hex("FF8FB8"), 0.03f);
                }
                p.Shape(Sd.Circle(0, 1.6f, 0.16f), Rgba.Hex("FFD447"));
                p.InkLine(0.025f, -0.07f, 1.6f, -0.04f, 1.63f);
                p.InkLine(0.025f, 0.04f, 1.63f, 0.07f, 1.6f);
                p.InkLine(0.025f, -0.05f, 1.54f, 0f, 1.51f, 0.05f, 1.54f);
            }
            else
            {
                p.Line(Ink, 0.1f, 0f, 0.65f, 0.08f, 1.0f, 0.3f, 1.2f, 0.5f, 1.05f);
                p.Line(stem, 0.06f, 0f, 0.65f, 0.08f, 1.0f, 0.3f, 1.2f, 0.5f, 1.05f);
                p.Shape(Sd.Rotate(Sd.Ellipse(-0.12f, 0.85f, 0.15f, 0.05f), -0.0f, 0.85f, -40f), Rgba.Hex("A8AE62"));
                for (int i = 0; i < 6; i++)
                {
                    float a = i / 6f * 6.283f;
                    p.Shape(Sd.Circle(0.55f + S41D_Cos(a) * 0.15f, 0.88f + S41D_Sin(a) * 0.15f, 0.08f), Rgba.Hex("C9A0A8"), 0.025f);
                }
                p.Shape(Sd.Circle(0.55f, 0.88f, 0.1f), Rgba.Hex("C9B060"));
                p.InkLine(0.02f, 0.5f, 0.88f, 0.54f, 0.86f);
                p.InkLine(0.02f, 0.57f, 0.86f, 0.61f, 0.88f);
            }
            Sdf pot = Sd.Polygon(-0.38f, 0.6f, 0.38f, 0.6f, 0.28f, 0.02f, -0.28f, 0.02f);
            Body(p, pot, Rgba.Hex("D9784A"), Rgba.Hex("B85A30"));
            p.Shape(Sd.Box(0, 0.62f, 0.44f, 0.08f, 0.03f), Rgba.Hex("E58A5A"));
            p.Fill(Sd.Box(0, 0.67f, 0.36f, 0.02f), Rgba.Hex("6A4024"));
            return new DoodleArt(happy ? "s41d_flower_happy" : "s41d_flower_sad", p);
        }

        /// <summary>Goccia di pioggia (centro 0,0, alta 0.24).</summary>
        public static DoodleArt S41D_Raindrop()
        {
            var p = new DoodlePainter(-0.1f, -0.12f, 0.1f, 0.15f, 220) { Ink = 0.025f };
            Sdf d = Sd.Union(Sd.Circle(0, -0.04f, 0.065f), Sd.Polygon(-0.058f, -0.01f, 0.058f, -0.01f, 0f, 0.12f));
            p.Shape(d, Rgba.Hex("6FC0FF"));
            p.Fill(Sd.Circle(-0.02f, -0.03f, 0.018f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("s41d_raindrop", p);
        }

        // ================= GHOST SELFIE =================

        /// <summary>Fantasmino (centro 0,0, alto 1.4); in posa fa la V con la mano e sorride.</summary>
        public static DoodleArt S41D_Ghost(bool pose)
        {
            var p = new DoodlePainter(-0.85f, -0.8f, 0.85f, 0.8f, 140) { Ink = 0.045f };
            var parts = new List<Sdf> { Sd.Circle(0, 0.15f, 0.55f), Sd.Box(0, -0.25f, 0.55f, 0.4f) };
            Sdf sheet = Sd.Union(parts.ToArray());
            for (int i = 0; i < 4; i++) sheet = Sd.Subtract(sheet, Sd.Circle(-0.41f + i * 0.275f, -0.72f, 0.12f));
            Sdf armL = Sd.Capsule(-0.5f, 0.0f, -0.72f, -0.15f, 0.1f);
            Sdf armR = pose ? Sd.Capsule(0.5f, 0.05f, 0.68f, 0.38f, 0.1f) : Sd.Capsule(0.5f, 0.0f, 0.72f, -0.15f, 0.1f);
            Sdf all = Sd.Union(sheet, armL, armR);
            p.FillFunc(all, (x, y) => Rgba.Lerp(Rgba.Hex("D8E4FF"), Rgba.White, S41D_Clamp((y + 0.6f) / 1f)));
            p.Shape(all, Rgba.Clear);
            if (pose)
            {
                // dita a V
                p.Shape(Sd.Capsule(0.66f, 0.42f, 0.6f, 0.68f, 0.045f), Rgba.White, 0.03f);
                p.Shape(Sd.Capsule(0.72f, 0.42f, 0.8f, 0.66f, 0.045f), Rgba.White, 0.03f);
                // occhiolino e sorrisone
                p.Shape(Sd.Ellipse(-0.2f, 0.25f, 0.11f, 0.14f), Ink);
                p.Fill(Sd.Circle(-0.23f, 0.3f, 0.04f), Rgba.White);
                p.InkLine(0.04f, 0.1f, 0.25f, 0.2f, 0.3f, 0.3f, 0.25f);
                p.Shape(Sd.Intersect(Sd.Circle(0.03f, 0.02f, 0.2f), Sd.Box(0.03f, -0.1f, 0.25f, 0.12f)), Rgba.Hex("5A2A4A"), 0.03f);
                p.Fill(Sd.Ellipse(0.03f, -0.12f, 0.09f, 0.04f), Rgba.Hex("FF8FA0"));
            }
            else
            {
                p.Shape(Sd.Ellipse(-0.2f, 0.25f, 0.1f, 0.15f), Ink);
                p.Shape(Sd.Ellipse(0.2f, 0.25f, 0.1f, 0.15f), Ink);
                p.Fill(Sd.Circle(-0.23f, 0.3f, 0.04f), Rgba.White);
                p.Fill(Sd.Circle(0.17f, 0.3f, 0.04f), Rgba.White);
                p.Shape(Sd.Ellipse(0, 0.0f, 0.06f, 0.08f), Rgba.Hex("5A2A4A"), 0.03f);
            }
            p.Fill(Sd.Circle(-0.36f, 0.08f, 0.07f), BlushColor.WithAlpha(0.55f));
            p.Fill(Sd.Circle(0.36f, 0.08f, 0.07f), BlushColor.WithAlpha(0.55f));
            return new DoodleArt(pose ? "s41d_ghost_pose" : "s41d_ghost", p);
        }

        /// <summary>Mirino della fotocamera: quattro angoli e crocetta (riquadro 3.2 x 2.6, centro 0,0).</summary>
        public static DoodleArt S41D_Viewfinder()
        {
            var p = new DoodlePainter(-1.7f, -1.4f, 1.7f, 1.4f, 100) { Ink = 0.035f, Wobble = 0.006f };
            foreach (float sx in new[] { -1f, 1f })
                foreach (float sy in new[] { -1f, 1f })
                {
                    p.Line(Ink, 0.13f, sx * 1.55f, sy * 0.95f, sx * 1.55f, sy * 1.25f, sx * 1.15f, sy * 1.25f);
                    p.Line(Rgba.White, 0.07f, sx * 1.55f, sy * 0.95f, sx * 1.55f, sy * 1.25f, sx * 1.15f, sy * 1.25f);
                }
            p.Line(Rgba.White.WithAlpha(0.7f), 0.03f, -0.15f, 0, 0.15f, 0);
            p.Line(Rgba.White.WithAlpha(0.7f), 0.03f, 0, -0.15f, 0, 0.15f);
            p.Shape(Sd.Circle(-1.2f, 0.95f, 0.07f), Rgba.Hex("FF4A4A"), 0.02f);
            return new DoodleArt("s41d_viewfinder", p);
        }

        /// <summary>Pulsante di scatto con la macchinetta (raggio 0.62).</summary>
        public static DoodleArt S41D_Shutter()
        {
            var p = new DoodlePainter(-0.7f, -0.7f, 0.7f, 0.7f, 150) { Ink = 0.05f };
            p.Shape(Sd.Circle(0, 0, 0.62f), Rgba.White);
            Body(p, Sd.Circle(0, 0, 0.5f), Rgba.Hex("FF6B6B"), Rgba.Hex("D94848"));
            p.Shape(Sd.Box(0, -0.03f, 0.26f, 0.17f, 0.05f), Rgba.White, 0.035f);
            p.Shape(Sd.Box(0, 0.17f, 0.09f, 0.05f, 0.02f), Rgba.White, 0.03f);
            p.Shape(Sd.Circle(0, -0.03f, 0.1f), Rgba.Hex("3A3A4A"), 0.03f);
            p.Fill(Sd.Circle(-0.03f, 0f, 0.03f), Rgba.White);
            return new DoodleArt("s41d_shutter", p);
        }

        /// <summary>Foto istantanea: cornice bianca, interno scuro (x ±0.75, y -0.55..0.85).</summary>
        public static DoodleArt S41D_Polaroid()
        {
            var p = new DoodlePainter(-0.95f, -1.15f, 0.95f, 1.05f, 120) { Ink = 0.045f };
            p.Shape(Sd.Box(0, -0.05f, 0.9f, 1.05f, 0.04f), Rgba.Hex("FBF8F0"));
            p.FillFunc(Sd.Box(0, 0.15f, 0.75f, 0.7f), (x, y) => Rgba.Lerp(Rgba.Hex("2C2546"), Rgba.Hex("4A3E70"), S41D_Clamp((y + 0.5f) / 1.4f)));
            p.Fill(Sd.Box(0, -0.4f, 0.75f, 0.15f), Rgba.Hex("3A2E30"));
            p.Shape(Sd.Box(0, 0.15f, 0.75f, 0.7f), Rgba.Clear, 0.03f);
            return new DoodleArt("s41d_polaroid", p);
        }

        // ================= MAGNET MAYHEM =================

        /// <summary>Labirinto su lastra metallica. Righe: bordo x ±2.55, y -4.25..2.75; muri interni y -2.4 (x -2.55..1.2),
        /// y -0.6 (x -1.2..2.55), y 1.2 (x -2.55..1.2); muri spessi 0.2.</summary>
        public static DoodleArt S41D_MazeBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("2E3B4E"), Rgba.Hex("3C4E66"), S41D_Clamp((y + 5f) / 10f)));
            // pavimento del labirinto a griglia
            Sdf floor = Sd.Box(0, -0.75f, 2.55f, 3.5f);
            p.Fill(floor, Rgba.Hex("D8E2EA"), false);
            for (float x = -2.5f; x < 2.6f; x += 0.35f) p.Line(Rgba.Hex("B9C7D4"), 0.015f, x, -4.25f, x, 2.75f);
            for (float y = -4.2f; y < 2.8f; y += 0.35f) p.Line(Rgba.Hex("B9C7D4"), 0.015f, -2.55f, y, 2.55f, y);
            Rgba wall = Rgba.Hex("6A7890");
            var walls = new[]
            {
                Sd.Box(0, 2.75f, 2.65f, 0.1f), Sd.Box(0, -4.25f, 2.65f, 0.1f), Sd.Box(-2.55f, -0.75f, 0.1f, 3.6f), Sd.Box(2.55f, -0.75f, 0.1f, 3.6f),
                Sd.Box(-0.675f, -2.4f, 1.875f, 0.1f, 0.05f), Sd.Box(0.675f, -0.6f, 1.875f, 0.1f, 0.05f), Sd.Box(-0.675f, 1.2f, 1.875f, 0.1f, 0.05f),
            };
            foreach (Sdf w in walls) p.Fill(Sd.Offset(w, 0.04f, -0.06f), Ink.WithAlpha(0.3f), false);
            foreach (Sdf w in walls) p.Shape(w, wall, 0.04f);
            // bulloni
            foreach (var b in new[] { (-2.85f, 4.6f), (2.85f, 4.6f), (-2.85f, -4.8f), (2.85f, -4.8f) }) p.Shape(Sd.Circle(b.Item1, b.Item2, 0.12f), Rgba.Hex("9AA8BC"), 0.03f);
            // partenza
            p.Fill(Sd.Circle(-1.9f, -3.35f, 0.38f), Rgba.Hex("8FD18A", 0.5f));
            return new DoodleArt("s41d_maze_bg", p);
        }

        /// <summary>Chiave di ferro compatta (centro 0,0, lunga 0.6).</summary>
        public static DoodleArt S41D_MagnetKey()
        {
            var p = new DoodlePainter(-0.36f, -0.2f, 0.36f, 0.2f, 220) { Ink = 0.03f };
            Sdf bow = Sd.Subtract(Sd.Circle(-0.18f, 0, 0.14f), Sd.Circle(-0.18f, 0, 0.05f));
            Sdf key = Sd.Union(bow, Sd.Box(0.08f, 0, 0.2f, 0.04f, 0.02f), Sd.Box(0.22f, -0.07f, 0.035f, 0.05f), Sd.Box(0.12f, -0.06f, 0.03f, 0.04f));
            Body(p, key, Rgba.Hex("C9D2DE"), Rgba.Hex("8D99AA"));
            p.Fill(Sd.Circle(-0.22f, 0.06f, 0.025f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("s41d_mkey", p);
        }

        /// <summary>Calamita a ferro di cavallo rossa (attira) o blu (respinge); centro 0,0, larga 0.8.</summary>
        public static DoodleArt S41D_Magnet(bool red)
        {
            var p = new DoodlePainter(-0.48f, -0.5f, 0.48f, 0.5f, 160) { Ink = 0.04f };
            Rgba c = red ? Rgba.Hex("E8433F") : Rgba.Hex("3E8EF7"), cs = red ? Rgba.Hex("B82A2A") : Rgba.Hex("2563C9");
            Sdf shoe = Sd.Union(Sd.Subtract(Sd.Circle(0, 0.05f, 0.4f), Sd.Circle(0, 0.05f, 0.18f)), Sd.Box(-0.29f, -0.18f, 0.11f, 0.23f), Sd.Box(0.29f, -0.18f, 0.11f, 0.23f));
            shoe = Sd.Subtract(shoe, Sd.Box(0, -0.2f, 0.18f, 0.25f));
            Body(p, shoe, c, cs);
            p.Shape(Sd.Box(-0.29f, -0.36f, 0.11f, 0.08f, 0.02f), Rgba.Hex("D8DEE6"), 0.03f);
            p.Shape(Sd.Box(0.29f, -0.36f, 0.11f, 0.08f, 0.02f), Rgba.Hex("D8DEE6"), 0.03f);
            // segno + o -
            p.Line(Rgba.White, 0.05f, -0.1f, 0.32f, 0.1f, 0.32f);
            if (red) p.Line(Rgba.White, 0.05f, 0f, 0.22f, 0f, 0.42f);
            return new DoodleArt(red ? "s41d_magnet_red" : "s41d_magnet_blue", p);
        }

        /// <summary>Trappola elettrica: barra con fulmini (centro 0,0, 1.0 x 0.36).</summary>
        public static DoodleArt S41D_Zap()
        {
            var p = new DoodlePainter(-0.56f, -0.24f, 0.56f, 0.24f, 160) { Ink = 0.035f };
            p.Shape(Sd.Box(0, 0, 0.5f, 0.16f, 0.06f), Rgba.Hex("3A3540"));
            for (float x = -0.42f; x < 0.45f; x += 0.28f)
                p.Shape(Sd.Polygon(x - 0.06f, 0.13f, x + 0.08f, 0.13f, x + 0.01f, 0.02f, x + 0.09f, 0.02f, x - 0.05f, -0.14f, x, -0.03f, x - 0.08f, -0.03f), Rgba.Hex("FFE23C"), 0.02f);
            p.Fill(Sd.Box(0, 0, 0.52f, 0.2f), Rgba.Hex("FFF27A", 0.12f));
            return new DoodleArt("s41d_zap", p);
        }

        /// <summary>Lucchetto del traguardo, chiuso o aperto (centro 0,0, 0.8 x 0.9).</summary>
        public static DoodleArt S41D_LockPlate(bool open)
        {
            var p = new DoodlePainter(-0.48f, -0.5f, 0.48f, 0.62f, 150) { Ink = 0.045f };
            Sdf shackle = Sd.Subtract(Sd.Circle(0, open ? 0.32f : 0.15f, 0.26f), Sd.Circle(0, open ? 0.32f : 0.15f, 0.15f));
            shackle = Sd.Subtract(shackle, Sd.Box(0, open ? 0.15f : -0.02f, 0.3f, 0.17f));
            p.Shape(Sd.Union(shackle, Sd.Box(-0.205f, open ? 0.2f : 0.05f, 0.055f, open ? 0.12f : 0.1f), Sd.Box(0.205f, 0.05f, 0.055f, 0.1f)), Rgba.Hex("B9C4D6"));
            Body(p, Sd.Box(0, -0.18f, 0.36f, 0.28f, 0.07f), Gold, GoldShade);
            p.Fill(Sd.Circle(0, -0.12f, 0.06f), Ink);
            p.Fill(Sd.Polygon(-0.03f, -0.13f, 0.03f, -0.13f, 0.045f, -0.32f, -0.045f, -0.32f), Ink);
            return new DoodleArt(open ? "s41d_lock_open" : "s41d_lock", p);
        }

        // ================= THE FAKE CRASH =================

        /// <summary>Finestra d'errore da cartone: giallo e viola, bordo spesso, crepa e cerotto (4.8 x 3.4, centro 0,0).</summary>
        public static DoodleArt S41D_CrashWindow()
        {
            var p = new DoodlePainter(-2.55f, -1.85f, 2.55f, 1.95f, 70) { Ink = 0.07f, Wobble = 0.02f };
            Sdf win = Sd.Box(0, 0, 2.4f, 1.7f, 0.25f);
            p.Fill(Sd.Offset(win, 0.1f, -0.12f), Ink.WithAlpha(0.35f), false);
            p.Fill(win, Rgba.Hex("FFF2A8"));
            // barra del titolo viola a pois
            Sdf bar = Sd.Intersect(win, Sd.Box(0, 1.42f, 2.6f, 0.32f));
            p.Fill(bar, Rgba.Hex("A35BE0"));
            for (float x = -2.1f; x < 2.3f; x += 0.45f) p.Fill(Sd.Intersect(bar, Sd.Circle(x, 1.42f + ((int)((x + 3) * 3) % 2) * 0.12f - 0.06f, 0.07f)), Rgba.Hex("C58CF5"));
            p.Line(Ink, 0.06f, -2.4f, 1.1f, 2.4f, 1.1f);
            // tre bottoncini a caramella
            string[] cols = { "FF6B6B", "FFC93C", "5BBF4A" };
            for (int i = 0; i < 3; i++) p.Shape(Sd.Circle(1.25f + i * 0.33f, 1.42f, 0.11f), Rgba.Hex(cols[i]), 0.035f);
            // crepa e cerotto
            p.Line(Ink, 0.035f, 2.4f, -0.6f, 2.05f, -0.75f, 2.2f, -1.0f, 1.9f, -1.2f);
            p.Shape(Sd.Rotate(Sd.Box(-2.1f, -1.4f, 0.32f, 0.1f, 0.06f), -2.1f, -1.4f, 40f), Rgba.Hex("F3C99A"), 0.03f);
            p.Shape(win, Rgba.Clear);
            return new DoodleArt("s41d_crash_window", p);
        }

        /// <summary>Monitor svenuto con gli occhi a X e la lingua fuori (centro 0,0, 1 x 1).</summary>
        public static DoodleArt S41D_CrashFace()
        {
            var p = new DoodlePainter(-0.55f, -0.55f, 0.55f, 0.55f, 160) { Ink = 0.04f };
            p.Shape(Sd.Box(0, -0.42f, 0.22f, 0.06f, 0.03f), Rgba.Hex("8E9AB0"));
            p.Shape(Sd.Box(0, -0.3f, 0.06f, 0.08f), Rgba.Hex("8E9AB0"));
            Body(p, Sd.Box(0, 0.08f, 0.48f, 0.36f, 0.1f), Rgba.Hex("B9C4D6"), Rgba.Hex("8E9AB0"));
            p.Shape(Sd.Box(0, 0.1f, 0.38f, 0.27f, 0.06f), Rgba.Hex("8FE3FF"), 0.03f);
            foreach (float s in new[] { -1f, 1f })
            {
                p.InkLine(0.04f, s * 0.17f - 0.07f, 0.22f, s * 0.17f + 0.07f, 0.08f);
                p.InkLine(0.04f, s * 0.17f - 0.07f, 0.08f, s * 0.17f + 0.07f, 0.22f);
            }
            p.InkLine(0.035f, -0.14f, -0.02f, 0.14f, -0.02f);
            p.Shape(Sd.Capsule(0.06f, -0.04f, 0.08f, -0.13f, 0.05f), Rgba.Hex("FF7F9C"), 0.025f);
            // stelline che girano
            foreach (var s in new[] { (-0.42f, 0.5f), (0.4f, 0.5f) })
                p.Shape(Sd.Polygon(s.Item1, s.Item2 + 0.07f, s.Item1 + 0.025f, s.Item2 + 0.02f, s.Item1 + 0.07f, s.Item2, s.Item1 + 0.025f, s.Item2 - 0.02f, s.Item1, s.Item2 - 0.07f, s.Item1 - 0.025f, s.Item2 - 0.02f, s.Item1 - 0.07f, s.Item2, s.Item1 - 0.025f, s.Item2 + 0.02f), Gold, 0.015f);
            return new DoodleArt("s41d_crash_face", p);
        }

        /// <summary>Bottone a pillola bianco con ombra (si tinge; 1.5 x 0.56).</summary>
        public static DoodleArt S41D_CrashButton()
        {
            var p = new DoodlePainter(-0.8f, -0.36f, 0.8f, 0.32f, 150) { Ink = 0.045f };
            p.Fill(Sd.Box(0.03f, -0.06f, 0.74f, 0.27f, 0.27f), Ink.WithAlpha(0.5f), false);
            p.Shape(Sd.Box(0, 0, 0.74f, 0.27f, 0.27f), Rgba.White);
            p.Fill(Sd.Capsule(-0.5f, 0.13f, 0.3f, 0.13f, 0.04f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("s41d_crash_btn", p);
        }

        // ================= RAINBOW THIEF =================

        /// <summary>Cielo con colline, nuvolette ai piedi dell'arcobaleno (centro dell'arco in 0,0.6).</summary>
        public static DoodleArt S41D_RainbowBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("E6F4FF"), Rgba.Hex("9CC9F0"), S41D_Clamp((y + 1f) / 6f)));
            p.Shape(Sd.Union(Sd.Circle(-2.2f, -2.4f, 1.6f), Sd.Circle(1.0f, -2.7f, 2.0f), Sd.Circle(3.2f, -2.2f, 1.3f)), Rgba.Hex("A5D98A"));
            p.FillFunc(Sd.Box(0, -4.0f, 3.3f, 1.4f), (x, y) => Rgba.Lerp(Rgba.Hex("6FB55A"), Rgba.Hex("8CCB6E"), S41D_Clamp((y + 5.2f) / 2.5f)));
            p.Line(Rgba.Hex("4F8F42"), 0.05f, -3.3f, -2.6f, 3.3f, -2.6f);
            return new DoodleArt("s41d_rainbow_bg", p);
        }

        /// <summary>Fascia k dell'arcobaleno (bianca, si tinge): semicerchio con centro in 0,0, raggio 2.6 - 0.16 k.</summary>
        public static DoodleArt S41D_Band(int k)
        {
            float r = 2.6f - 0.16f * k;
            var p = new DoodlePainter(-2.68f, -0.02f, 2.68f, 2.68f, 60) { Ink = 0.02f, Wobble = 0.006f };
            Sdf band = Sd.Intersect(Sd.Subtract(Sd.Circle(0, 0, r), Sd.Circle(0, 0, r - 0.16f)), Sd.Box(0, 1.5f, 3f, 1.5f));
            p.Fill(band, Rgba.White);
            p.Shape(band, Rgba.Clear, 0.02f);
            return new DoodleArt("s41d_band_" + k, p);
        }

        /// <summary>Cassa di vetro trasparente (centro 0,0, 1.2 x 1.2).</summary>
        public static DoodleArt S41D_Crate()
        {
            var p = new DoodlePainter(-0.66f, -0.66f, 0.66f, 0.66f, 130) { Ink = 0.04f };
            p.Fill(Sd.Box(0, 0, 0.58f, 0.58f, 0.06f), Rgba.Hex("E8F6FF", 0.28f));
            Sdf frame = Sd.Subtract(Sd.Box(0, 0, 0.58f, 0.58f, 0.06f), Sd.Box(0, 0, 0.47f, 0.47f));
            p.Shape(frame, Rgba.Hex("D9E4EE"));
            p.Line(Rgba.Hex("D9E4EE"), 0.05f, -0.47f, -0.47f, 0.47f, 0.47f);
            p.Line(Ink, 0.015f, -0.47f, -0.47f, 0.47f, 0.47f);
            p.Line(Rgba.White.WithAlpha(0.8f), 0.04f, -0.36f, 0.3f, -0.2f, 0.42f);
            return new DoodleArt("s41d_crate", p);
        }

        /// <summary>Sfera di luce bianca (si tinge; raggio 0.27).</summary>
        public static DoodleArt S41D_Orb()
        {
            var p = new DoodlePainter(-0.32f, -0.32f, 0.32f, 0.32f, 200) { Ink = 0.035f };
            p.FillFunc(Sd.Circle(0, 0, 0.27f), (x, y) => Rgba.Lerp(Rgba.Hex("D8D8D8"), Rgba.White, S41D_Clamp(1f - (float)Math.Sqrt((x + 0.08f) * (x + 0.08f) + (y - 0.08f) * (y - 0.08f)) / 0.35f)));
            p.Shape(Sd.Circle(0, 0, 0.27f), Rgba.Clear);
            p.Fill(Sd.Circle(-0.09f, 0.09f, 0.06f), Rgba.White);
            return new DoodleArt("s41d_orb", p);
        }

        /// <summary>Esempio luminoso: due luci (rossa e verde) che si sovrappongono in giallo su un pannellino scuro.</summary>
        public static DoodleArt S41D_Venn()
        {
            var p = new DoodlePainter(-0.95f, -0.6f, 0.95f, 0.6f, 130) { Ink = 0.04f };
            p.Shape(Sd.Box(0, 0, 0.88f, 0.52f, 0.12f), Rgba.Hex("2A2440"));
            Sdf r = Sd.Circle(-0.2f, 0.02f, 0.32f), g = Sd.Circle(0.2f, 0.02f, 0.32f);
            p.Fill(r, Rgba.Hex("FF3B3B"), false);
            p.Fill(g, Rgba.Hex("3BEB5A"), false);
            p.Fill(Sd.Intersect(r, g), Rgba.Hex("FFF23B"), false);
            p.Fill(Sd.Box(0, 0, 0.88f, 0.52f, 0.12f), Rgba.White.WithAlpha(0.05f));
            return new DoodleArt("s41d_venn", p);
        }
    }
}
