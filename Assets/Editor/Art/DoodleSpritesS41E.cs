using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli della specifica 105-200, pacchetto E.</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV41E() => new DoodleArt[]
        {
            // Pixel rebellion
            S41E_PixelSpace(), S41E_PixelCritter(false), S41E_PixelCritter(true), S41E_PixelLeader(), S41E_Podium(),
            // Cold coffee
            S41E_OfficeBg(), S41E_Desk(), S41E_Worker(false), S41E_Worker(true), S41E_Cup(false), S41E_Cup(true),
            S41E_HeatLamp(), S41E_HeatButton(), S41E_Thermo(),
            // The missing second
            S41E_NoonBg(), S41E_NoonShutter(), S41E_ClockPanel(),
            // Mirror twins
            S41E_TwinsBg(), S41E_Twin(true), S41E_Twin(false), S41E_StandMirror(), S41E_TwinDoor(false), S41E_TwinDoor(true),
            // The hungry printer
            S41E_Shelf(), S41E_Printer(), S41E_InstructionSheet(), S41E_BlankPage(), S41E_MapSheet(),
            // Finger maze
            S41E_FingerMazeBg(), S41E_Fingerprint(),
            // False finish
            S41E_RaceBg(), S41E_Runner(), S41E_NextButton(), S41E_Trophy(),
            // Heartbeat
            S41E_WardBg(), S41E_Monitor(), S41E_Monster(false), S41E_Monster(true), S41E_HeartButton(), S41E_Speaker(true), S41E_Speaker(false),
        };

        static float S41E_Clamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
        static float S41E_Sin(float v) => (float)Math.Sin(v);

        /// <summary>Numero pseudo-casuale 0..1 deterministico.</summary>
        static float S41E_Hash(int i)
        {
            uint h = (uint)i * 2654435761u;
            h ^= h >> 13; h *= 1274126177u; h ^= h >> 16;
            return (h & 0xFFFFFF) / (float)0xFFFFFF;
        }

        static void S41E_Eyes(DoodlePainter p, float cx, float cy, float gap, float r, float lookX = 0f, float lookY = 0f)
        {
            foreach (float s in new[] { -1f, 1f })
            {
                float x = cx + s * gap;
                p.Shape(Sd.Circle(x, cy, r), Rgba.White, r * 0.35f);
                p.Fill(Sd.Circle(x + lookX * r * 0.35f, cy + lookY * r * 0.35f, r * 0.5f), Ink);
                p.Fill(Sd.Circle(x + lookX * r * 0.35f - r * 0.18f, cy + lookY * r * 0.35f + r * 0.2f, r * 0.17f), Rgba.White);
            }
        }

        // ================= PIXEL REBELLION =================

        /// <summary>Spazio quasi nero con polvere di stelle fioca (6 x 10).</summary>
        public static DoodleArt S41E_PixelSpace()
        {
            var p = new DoodlePainter(-3.0f, -5.1f, 3.0f, 5.1f, 40) { Wobble = 0f };
            p.FillFunc(Sd.Box(0, 0, 3.1f, 5.2f), (x, y) => Rgba.Lerp(Rgba.Hex("05060C"), Rgba.Hex("0E0B1C"), S41E_Clamp((y + 5f) / 10f)));
            for (int i = 0; i < 70; i++)
            {
                float x = -2.9f + S41E_Hash(i * 3 + 1) * 5.8f, y = -5f + S41E_Hash(i * 3 + 2) * 10f;
                float s = 0.012f + S41E_Hash(i * 3 + 3) * 0.02f;
                p.Fill(Sd.Box(x, y, s, s), Rgba.Hex(i % 3 == 0 ? "6C6A9A" : "3E3C5E", 0.8f), false);
            }
            return new DoodleArt("s41e_pixel_space", p);
        }

        static readonly string[] S41E_CritterRows =
        {
            "..d....d..",
            "...d..d...",
            "..dddddd..",
            ".dggggggd.",
            "dgwwggwwgd",
            "dgkwggkwgd",
            "dggggggggd",
            "dgggppgggd",
            ".dggggggd.",
            ".dd.dd.dd.",
        };

        static readonly string[] S41E_CritterMadRows =
        {
            "..d....d..",
            "...d..d...",
            "..dddddd..",
            ".dkgggkgd.",
            "dggkgkgggd",
            "dgwkggwkgd",
            "dggggggggd",
            "dggppppggd",
            ".dgpggpgd.",
            ".dd.dd.dd.",
        };

        static void S41E_PixelGrid(DoodlePainter p, string[] rows, float size, float left, float top, Func<char, Rgba?> col)
        {
            for (int r = 0; r < rows.Length; r++)
                for (int c = 0; c < rows[r].Length; c++)
                {
                    Rgba? k = col(rows[r][c]);
                    if (!k.HasValue) continue;
                    float x = left + (c + 0.5f) * size, y = top - (r + 0.5f) * size;
                    p.Fill(Sd.Box(x, y, size * 0.5f + 0.002f, size * 0.5f + 0.002f), k.Value, false);
                }
        }

        /// <summary>Creaturina di pixel 10x10 (lato 1, centro 0,0; occhio sinistro in x -0.3..-0.1, y -0.1..0.1).</summary>
        public static DoodleArt S41E_PixelCritter(bool mad)
        {
            var p = new DoodlePainter(-0.52f, -0.52f, 0.52f, 0.52f, 80) { Wobble = 0f };
            Rgba body = mad ? Rgba.Hex("FF4A5A") : Rgba.Hex("5CFF8A");
            S41E_PixelGrid(p, mad ? S41E_CritterMadRows : S41E_CritterRows, 0.1f, -0.5f, 0.5f, ch =>
            {
                switch (ch)
                {
                    case 'd': return mad ? Rgba.Hex("7A0E1E") : Rgba.Hex("127A3A");
                    case 'g': return body;
                    case 'w': return Rgba.White;
                    case 'k': return Rgba.Hex("10121E");
                    case 'p': return Rgba.Hex("FF8FC8");
                    default: return null;
                }
            });
            return new DoodleArt(mad ? "s41e_pixel_critter_mad" : "s41e_pixel_critter", p);
        }

        /// <summary>Pixel sindacalista: caschetto giallo e megafono (lato 1.2, piedi in y -0.6).</summary>
        public static DoodleArt S41E_PixelLeader()
        {
            string[] rows =
            {
                "...yyyyyy...",
                "..yyyyyyyy..",
                ".yyyyyyyyyy.",
                "..bbbbbbbb..",
                "..bwkbbwkb..",
                "..bwkbbwkb.m",
                "..bbbbbbbbmm",
                "..bbrrrrbmmm",
                "..bbbbbbbbmm",
                "...bbbbbb..m",
                "...b....b...",
                "..bb....bb..",
            };
            var p = new DoodlePainter(-0.62f, -0.62f, 0.62f, 0.62f, 80) { Wobble = 0f };
            S41E_PixelGrid(p, rows, 0.1f, -0.6f, 0.6f, ch =>
            {
                switch (ch)
                {
                    case 'y': return Rgba.Hex("FFD23A");
                    case 'b': return Rgba.Hex("4AB8FF");
                    case 'w': return Rgba.White;
                    case 'k': return Rgba.Hex("10121E");
                    case 'r': return Rgba.Hex("D8263A");
                    case 'm': return Rgba.Hex("E8E8F0");
                    default: return null;
                }
            });
            return new DoodleArt("s41e_pixel_leader", p);
        }

        /// <summary>Podio a cubo (1.4 x 1.0, base in y 0).</summary>
        public static DoodleArt S41E_Podium()
        {
            var p = new DoodlePainter(-0.8f, -0.06f, 0.8f, 1.12f, 100) { Ink = 0.045f };
            p.Shape(Sd.Polygon(-0.7f, 0.85f, -0.45f, 1.05f, 0.75f, 1.05f, 0.5f, 0.85f), Rgba.Hex("FF8A5A"));
            p.Shape(Sd.Polygon(0.5f, 0.85f, 0.75f, 1.05f, 0.75f, 0.2f, 0.5f, 0.0f), Rgba.Hex("B8402A"));
            Sdf front = Sd.Box(-0.1f, 0.425f, 0.6f, 0.425f);
            p.FillFunc(front, (x, y) => Rgba.Lerp(Rgba.Hex("D85A3A"), Rgba.Hex("F07050"), S41E_Clamp(y / 0.85f)));
            p.Shape(front, Rgba.Clear);
            p.Fill(Sd.Box(-0.1f, 0.42f, 0.42f, 0.14f), Rgba.Hex("FFF1C8"), false);
            return new DoodleArt("s41e_podium", p);
        }

        // ================= COLD COFFEE =================

        /// <summary>Ufficio: parete azzurrina, finestra con veneziane, orologio, pavimento (6.2 x 10.4).</summary>
        public static DoodleArt S41E_OfficeBg()
        {
            var p = new DoodlePainter(-3.1f, -5.2f, 3.1f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.2f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("BFD7E6"), Rgba.Hex("DCEAF2"), S41E_Clamp((y + 2f) / 7f)));
            for (float x = -3.1f; x < 3.2f; x += 0.62f) p.Line(Rgba.Hex("AECADC", 0.6f), 0.03f, x, -2.5f, x, 5.2f);
            // finestra con veneziane
            Sdf win = Sd.Box(-0.6f, 1.9f, 1.4f, 1.05f, 0.05f);
            p.FillFunc(win, (x, y) => Rgba.Lerp(Rgba.Hex("8FD0FF"), Rgba.Hex("D8F1FF"), S41E_Clamp((y - 0.9f) / 2f)));
            for (float y = 2.85f; y > 0.9f; y -= 0.2f) p.Fill(Sd.Box(-0.6f, y, 1.35f, 0.05f), Rgba.Hex("F4F4F0"), false);
            p.Shape(win, Rgba.Clear, 0.07f);
            p.InkLine(0.03f, 0.4f, 3.0f, 0.4f, 1.1f);
            // orologio
            p.Shape(Sd.Circle(2.1f, 2.6f, 0.42f), Rgba.White, 0.05f);
            p.InkLine(0.05f, 2.1f, 2.6f, 2.1f, 2.88f);
            p.InkLine(0.05f, 2.1f, 2.6f, 2.3f, 2.55f);
            // battiscopa e pavimento
            p.Fill(Sd.Box(0, -3.9f, 3.2f, 1.4f), Rgba.Hex("9C8A7E"), false);
            for (float x = -3.1f; x < 3.2f; x += 0.7f) p.Line(Rgba.Hex("857468"), 0.025f, x, -2.5f, x - 0.3f, -5.2f);
            p.Shape(Sd.Box(0, -2.5f, 3.2f, 0.08f), Rgba.Hex("E8E0D8"), 0.035f);
            // poster "MONDAY"
            p.Shape(Sd.Box(-2.3f, -0.6f, 0.55f, 0.7f, 0.03f), Rgba.Hex("FFE58A"), 0.04f);
            p.Shape(Sd.Circle(-2.3f, -0.45f, 0.28f), Rgba.Hex("FF9A5A"), 0.03f);
            p.InkLine(0.03f, -2.42f, -0.52f, -2.18f, -0.52f);
            p.Fill(Sd.Circle(-2.4f, -0.4f, 0.03f), Ink);
            p.Fill(Sd.Circle(-2.2f, -0.4f, 0.03f), Ink);
            return new DoodleArt("s41e_office_bg", p);
        }

        /// <summary>Scrivania: piano con il bordo superiore in y 0 (larga 6.2), fronte con cassetti fino a y -2.6.</summary>
        public static DoodleArt S41E_Desk()
        {
            var p = new DoodlePainter(-3.15f, -2.65f, 3.15f, 0.08f, 50) { Ink = 0.05f };
            Sdf front = Sd.Box(0, -1.4f, 2.9f, 1.25f);
            Body(p, front, Rgba.Hex("C98A55"), Rgba.Hex("A86B3A"));
            foreach (float y in new[] { -0.75f, -1.55f })
            {
                p.Shape(Sd.Box(-1.9f, y, 0.8f, 0.32f, 0.04f), Rgba.Hex("D99A62"), 0.035f);
                p.Shape(Sd.Capsule(-2.15f, y, -1.65f, y, 0.05f), Rgba.Hex("6E4A2A"), 0.025f);
            }
            Sdf top = Sd.Box(0, -0.12f, 3.1f, 0.14f, 0.04f);
            p.FillFunc(top, (x, y) => Rgba.Lerp(Rgba.Hex("B9773F"), Rgba.Hex("E2A469"), S41E_Clamp((y + 0.26f) / 0.28f)));
            p.Shape(top, Rgba.Clear);
            return new DoodleArt("s41e_desk", p);
        }

        /// <summary>Impiegato dalla cintola in su (vita in y 0, testa in 0, 1.1): furioso a braccia conserte o felice.</summary>
        public static DoodleArt S41E_Worker(bool happy)
        {
            var p = new DoodlePainter(-0.95f, -0.1f, 0.95f, 1.75f, 120) { Ink = 0.045f };
            // busto
            Sdf torso = Sd.Box(0, 0.32f, 0.5f, 0.4f, 0.18f);
            Body(p, torso, Rgba.Hex("F4F4F8"), Rgba.Hex("D2D6E2"));
            p.Shape(Sd.Polygon(0f, 0.68f, -0.08f, 0.58f, 0f, 0.05f, 0.08f, 0.58f), Rgba.Hex("D8263A"), 0.025f);
            p.Shape(Sd.Polygon(-0.17f, 0.72f, 0f, 0.62f, 0.17f, 0.72f, 0f, 0.68f), Rgba.White, 0.025f);
            // testa
            Sdf head = Sd.Ellipse(0, 1.1f, 0.4f, 0.42f);
            Rgba skin = happy ? Rgba.Hex("FFD6B0") : Rgba.Hex("FF9C84");
            Body(p, head, skin, happy ? Rgba.Hex("F2BC92") : Rgba.Hex("E8765E"));
            p.Shape(Sd.Union(Sd.Ellipse(0, 1.43f, 0.36f, 0.14f), Sd.Circle(-0.3f, 1.3f, 0.12f), Sd.Circle(0.3f, 1.3f, 0.12f)), Rgba.Hex("5A3A2A"));
            p.Shape(Sd.Ellipse(-0.4f, 1.08f, 0.07f, 0.1f), skin, 0.03f);
            p.Shape(Sd.Ellipse(0.4f, 1.08f, 0.07f, 0.1f), skin, 0.03f);
            if (happy)
            {
                p.InkLine(0.035f, -0.22f, 1.14f, -0.15f, 1.19f, -0.08f, 1.14f);
                p.InkLine(0.035f, 0.08f, 1.14f, 0.15f, 1.19f, 0.22f, 1.14f);
                p.Shape(Sd.Intersect(Sd.Circle(0, 0.98f, 0.15f), Sd.Box(0, 0.9f, 0.2f, 0.08f)), Rgba.Hex("8A2A3A"), 0.03f);
                p.Fill(Sd.Circle(-0.25f, 0.98f, 0.06f), BlushColor.WithAlpha(0.6f));
                p.Fill(Sd.Circle(0.25f, 0.98f, 0.06f), BlushColor.WithAlpha(0.6f));
                // braccia: pollice in su
                p.Shape(Sd.Capsule(-0.48f, 0.5f, -0.75f, 0.85f, 0.1f), Rgba.Hex("F4F4F8"));
                p.Shape(Sd.Circle(-0.78f, 0.92f, 0.11f), skin);
                p.Shape(Sd.Capsule(-0.8f, 1.0f, -0.8f, 1.12f, 0.04f), skin, 0.025f);
                p.Shape(Sd.Capsule(0.48f, 0.45f, 0.7f, 0.2f, 0.1f), Rgba.Hex("F4F4F8"));
            }
            else
            {
                S41E_Eyes(p, 0, 1.12f, 0.15f, 0.08f, 0f, 0f);
                p.InkLine(0.05f, -0.27f, 1.29f, -0.07f, 1.2f);
                p.InkLine(0.05f, 0.27f, 1.29f, 0.07f, 1.2f);
                p.Shape(Sd.Box(0, 0.93f, 0.13f, 0.045f, 0.03f), Rgba.White, 0.03f);
                p.InkLine(0.015f, -0.13f, 0.93f, 0.13f, 0.93f);
                // braccia conserte
                p.Shape(Sd.Capsule(-0.4f, 0.32f, 0.35f, 0.38f, 0.12f), Rgba.Hex("E8EAF2"));
                p.Shape(Sd.Capsule(0.4f, 0.28f, -0.3f, 0.24f, 0.12f), Rgba.Hex("F4F4F8"));
                // nuvoletta di rabbia
                p.Shape(Sd.Union(Sd.Circle(0.6f, 1.55f, 0.1f), Sd.Circle(0.72f, 1.6f, 0.09f)), Rgba.Hex("FFFFFF", 0.9f), 0.025f);
                p.Line(Rgba.Hex("E8263A"), 0.035f, 0.55f, 1.32f, 0.62f, 1.4f, 0.69f, 1.32f);
            }
            return new DoodleArt(happy ? "s41e_worker_happy" : "s41e_worker_angry", p);
        }

        /// <summary>Tazza (fondo in y 0, alta 0.6): congelata con un blocco di ghiaccio o col caffè caldo.</summary>
        public static DoodleArt S41E_Cup(bool hot)
        {
            var p = new DoodlePainter(-0.48f, -0.06f, 0.5f, 0.98f, 150) { Ink = 0.04f };
            p.Shape(Sd.Subtract(Sd.Circle(0.3f, 0.32f, 0.16f), Sd.Circle(0.3f, 0.32f, 0.08f)), Rgba.White);
            Sdf cup = Sd.Union(Sd.Box(0, 0.3f, 0.3f, 0.3f, 0.08f), Sd.Box(0, 0.52f, 0.3f, 0.08f));
            Body(p, cup, Rgba.White, Rgba.Hex("D8DCE8"));
            p.Fill(Sd.Box(0, 0.3f, 0.28f, 0.05f), Rgba.Hex("6FB0E8"), false);
            p.Shape(Sd.Ellipse(0, 0.6f, 0.3f, 0.07f), hot ? Rgba.Hex("6A3A1E") : Rgba.Hex("8A6A5A"), 0.035f);
            if (hot)
            {
                p.Fill(Sd.Ellipse(-0.08f, 0.61f, 0.12f, 0.025f), Rgba.Hex("C98A55", 0.8f));
            }
            else
            {
                // blocco di ghiaccio che spunta
                Sdf ice = Sd.Rotate(Sd.Box(0.0f, 0.75f, 0.22f, 0.18f, 0.03f), 0f, 0.75f, -8f);
                p.FillFunc(ice, (x, y) => Rgba.Lerp(Rgba.Hex("BFE6FF", 0.95f), Rgba.Hex("F2FBFF", 0.95f), S41E_Clamp((y - 0.6f) / 0.35f)));
                p.Fill(Sd.Box(-0.03f, 0.72f, 0.12f, 0.06f), Rgba.Hex("8A6A5A", 0.5f));
                p.Line(Rgba.White, 0.03f, -0.12f, 0.86f, 0.0f, 0.88f);
                p.Shape(ice, Rgba.Clear, 0.035f);
                // ghiaccioli
                foreach (float x in new[] { -0.2f, 0.05f, 0.22f })
                    p.Shape(Sd.Polygon(x - 0.04f, 0.6f, x + 0.04f, 0.6f, x, 0.45f), Rgba.Hex("D8F1FF"), 0.02f);
            }
            return new DoodleArt(hot ? "s41e_cup_hot" : "s41e_cup_frozen", p);
        }

        /// <summary>Lampada da scrivania: base in (0,0), paralume con l'apertura centrata in (-0.8, 2.0) verso il basso.</summary>
        public static DoodleArt S41E_HeatLamp()
        {
            var p = new DoodlePainter(-1.35f, -0.08f, 0.55f, 2.75f, 110) { Ink = 0.045f };
            p.Shape(Sd.Ellipse(0, 0.08f, 0.45f, 0.1f), Rgba.Hex("3A3A4A"));
            p.Shape(Sd.Capsule(0, 0.12f, 0.15f, 1.3f, 0.06f), Rgba.Hex("D8263A"));
            p.Shape(Sd.Capsule(0.15f, 1.3f, -0.45f, 2.35f, 0.06f), Rgba.Hex("D8263A"));
            p.Shape(Sd.Circle(0.15f, 1.3f, 0.1f), Rgba.Hex("3A3A4A"), 0.03f);
            Sdf shade = Sd.Polygon(-1.25f, 2.0f, -0.35f, 2.0f, -0.55f, 2.55f, -1.05f, 2.55f);
            Body(p, shade, Rgba.Hex("FF5A4A"), Rgba.Hex("D8263A"));
            p.Shape(Sd.Circle(-0.45f, 2.38f, 0.09f), Rgba.Hex("3A3A4A"), 0.03f);
            p.Shape(Sd.Ellipse(-0.8f, 2.0f, 0.36f, 0.07f), Rgba.Hex("FFF3B0"), 0.03f);
            return new DoodleArt("s41e_heat_lamp", p);
        }

        /// <summary>Pulsante tondo rosso con fiammella (raggio 0.6).</summary>
        public static DoodleArt S41E_HeatButton()
        {
            var p = new DoodlePainter(-0.72f, -0.72f, 0.72f, 0.72f, 120) { Ink = 0.05f };
            p.Shape(Sd.Circle(0, -0.06f, 0.62f), Rgba.Hex("8A1A2A"));
            Sdf top = Sd.Circle(0, 0.04f, 0.6f);
            Body(p, top, Rgba.Hex("FF4A4A"), Rgba.Hex("D82A3A"));
            Sdf flame = Sd.Union(Sd.Circle(0, 0.0f, 0.2f), Sd.Polygon(-0.19f, 0.06f, 0.19f, 0.06f, 0.04f, 0.42f));
            p.Shape(flame, Rgba.Hex("FFC93C"), 0.035f);
            p.Fill(Sd.Circle(0, -0.03f, 0.09f), Rgba.Hex("FFF3B0"));
            return new DoodleArt("s41e_heat_btn", p);
        }

        /// <summary>Termometro: vetro con interno trasparente (x ±0.14, y -1.5..1.7) e bulbo rosso in (0,-1.75).</summary>
        public static DoodleArt S41E_Thermo()
        {
            var p = new DoodlePainter(-0.4f, -2.1f, 0.4f, 1.95f, 110) { Ink = 0.045f };
            Sdf tube = Sd.Box(0, 0.15f, 0.2f, 1.62f, 0.2f);
            Sdf hole = Sd.Box(0, 0.12f, 0.13f, 1.58f, 0.12f);
            p.Shape(Sd.Subtract(tube, hole), Rgba.Hex("F4F8FF"));
            p.Shape(Sd.Circle(0, -1.75f, 0.3f), Rgba.Hex("FF4A4A"));
            p.Fill(Sd.Box(0, -1.45f, 0.12f, 0.12f), Rgba.Hex("FF4A4A"), false);
            p.Fill(Sd.Circle(-0.1f, -1.68f, 0.07f), Rgba.White.WithAlpha(0.7f));
            for (float y = -1.2f; y < 1.7f; y += 0.32f) p.InkLine(0.025f, 0.2f, y, 0.32f, y);
            return new DoodleArt("s41e_thermo", p);
        }

        // ================= THE MISSING SECOND =================

        /// <summary>Corridoio con il bancone "Cheese bar" dietro una porta (vano 0.4..2.2 x -2.6..0).</summary>
        public static DoodleArt S41E_NoonBg()
        {
            var p = new DoodlePainter(-3.1f, -5.2f, 3.1f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.2f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("F2C98A"), Rgba.Hex("FBE3B8"), S41E_Clamp((y + 2.6f) / 7f)));
            for (float x = -3.0f; x < 3.2f; x += 0.5f) p.Fill(Sd.Box(x, 1.2f, 0.06f, 3.9f), Rgba.Hex("EBB977", 0.5f), false);
            // pavimento a scacchi
            p.FillFunc(Sd.Box(0, -3.95f, 3.2f, 1.35f), (x, y) =>
            {
                int k = (int)Math.Floor((x + 10f) / 0.5f) + (int)Math.Floor((y + 10f) / 0.5f);
                return k % 2 == 0 ? Rgba.Hex("E8E0D0") : Rgba.Hex("B8A890");
            });
            p.Shape(Sd.Box(0, -2.6f, 3.2f, 0.06f), Rgba.Hex("8A5A3A"), 0.03f);
            // vano della porta: interno illuminato col formaggio
            Sdf frame = Sd.Box(1.3f, -1.25f, 1.05f, 1.45f, 0.05f);
            p.Shape(frame, Rgba.Hex("8A5128"), 0.05f);
            p.FillFunc(Sd.Box(1.3f, -1.3f, 0.9f, 1.3f), (x, y) => Rgba.Lerp(Rgba.Hex("FFB84A"), Rgba.Hex("FFE9A8"), S41E_Clamp((y + 2.6f) / 2.6f)));
            p.Shape(Sd.Polygon(0.8f, -2.2f, 1.9f, -2.2f, 1.9f, -1.6f), Rgba.Hex("FFD24A"), 0.035f);
            p.Fill(Sd.Circle(1.5f, -2.0f, 0.08f), Rgba.Hex("E8A82A"));
            p.Fill(Sd.Circle(1.75f, -1.85f, 0.06f), Rgba.Hex("E8A82A"));
            // insegna sopra la porta
            p.Shape(Sd.Box(1.3f, 0.55f, 1.0f, 0.28f, 0.08f), Rgba.Hex("FFE58A"), 0.04f);
            // quadretto a sinistra
            p.Shape(Sd.Box(-1.9f, -0.3f, 0.5f, 0.4f, 0.03f), Rgba.Hex("BDE6FF"), 0.04f);
            p.Shape(Sd.Polygon(-2.3f, -0.6f, -1.95f, -0.1f, -1.6f, -0.6f), Rgba.Hex("8CCB6E"), 0.03f);
            return new DoodleArt("s41e_noon_bg", p);
        }

        /// <summary>Saracinesca della porta: bordo alto in y 0, larga 1.8, alta 2.6 (si arrotola scalando y).</summary>
        public static DoodleArt S41E_NoonShutter()
        {
            var p = new DoodlePainter(-0.95f, -2.65f, 0.95f, 0.05f, 100) { Ink = 0.04f };
            Sdf sh = Sd.Box(0, -1.3f, 0.9f, 1.3f, 0.03f);
            p.FillFunc(sh, (x, y) => Rgba.Lerp(Rgba.Hex("9AA4B8"), Rgba.Hex("C8D0DE"), 0.5f + 0.5f * S41E_Sin(y * 28f)));
            for (float y = -0.15f; y > -2.6f; y -= 0.22f) p.Line(Rgba.Hex("6E7890"), 0.02f, -0.88f, y, 0.88f, y);
            p.Shape(sh, Rgba.Clear);
            p.Shape(Sd.Box(0, -1.15f, 0.62f, 0.3f, 0.06f), Rgba.Hex("FFF6E0"), 0.035f);
            p.Shape(Sd.Box(0, -2.45f, 0.25f, 0.08f, 0.04f), Rgba.Hex("6E7890"), 0.03f);
            return new DoodleArt("s41e_noon_shutter", p);
        }

        /// <summary>Pannello dell'orologio digitale (5.0 x 1.5, centro 0,0) con lo schermo LCD scuro.</summary>
        public static DoodleArt S41E_ClockPanel()
        {
            var p = new DoodlePainter(-2.62f, -0.9f, 2.62f, 0.9f, 90) { Ink = 0.06f };
            Body(p, Sd.Box(0, 0, 2.5f, 0.75f, 0.2f), Rgba.Hex("5A5A78"), Rgba.Hex("42425E"));
            p.FillFunc(Sd.Box(0, 0, 2.3f, 0.58f, 0.1f), (x, y) => Rgba.Lerp(Rgba.Hex("0E1A14"), Rgba.Hex("1A2E22"), S41E_Clamp((y + 0.6f) / 1.2f)));
            p.Line(Rgba.White.WithAlpha(0.12f), 0.08f, -2.1f, 0.45f, 1.0f, 0.45f);
            foreach (float x in new[] { -2.4f, 2.4f })
                p.Shape(Sd.Circle(x, 0.6f, 0.06f), Rgba.Hex("C8C8D8"), 0.02f);
            return new DoodleArt("s41e_clock_panel", p);
        }

        // ================= MIRROR TWINS =================

        /// <summary>Sala degli specchi: pavimento in y -2.4 con una crepa profonda tra x -1.0 e -0.3.</summary>
        public static DoodleArt S41E_TwinsBg()
        {
            var p = new DoodlePainter(-3.1f, -5.2f, 3.1f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.2f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("B79AE0"), Rgba.Hex("D9C8F2"), S41E_Clamp((y + 2.4f) / 7f)));
            // archi decorativi
            for (int i = 0; i < 4; i++)
            {
                float x = -2.4f + i * 1.6f;
                p.Shape(Sd.Subtract(Sd.Circle(x, 2.2f, 0.65f), Sd.Circle(x, 2.2f, 0.5f)), Rgba.Hex("FFD86A"), 0.03f);
                p.FillFunc(Sd.Circle(x, 2.2f, 0.5f), (px, py) => Rgba.Lerp(Rgba.Hex("CFEFFF"), Rgba.Hex("F2FBFF"), S41E_Clamp((py - 1.7f))));
                p.Line(Rgba.White.WithAlpha(0.8f), 0.05f, x - 0.25f, 2.0f, x - 0.05f, 2.45f);
            }
            // pavimento
            p.FillFunc(Sd.Box(0, -3.85f, 3.2f, 1.45f), (x, y) => Rgba.Lerp(Rgba.Hex("6A4AA0"), Rgba.Hex("8A6AC0"), S41E_Clamp((y + 5.2f) / 2.8f)));
            p.Shape(Sd.Box(0, -2.4f, 3.2f, 0.06f), Rgba.Hex("FFD86A"), 0.03f);
            // crepa
            Sdf crack = Sd.Polygon(-1.0f, -2.34f, -0.3f, -2.34f, -0.45f, -3.2f, -0.6f, -3.6f, -0.75f, -3.1f);
            p.Shape(crack, Rgba.Hex("1A1028"), 0.04f);
            return new DoodleArt("s41e_twins_bg", p);
        }

        /// <summary>Gemello (piedi in y 0, alto 1.6): con la mano alzata (in 0.45, 1.4) o che saluta.</summary>
        public static DoodleArt S41E_Twin(bool holding)
        {
            var p = new DoodlePainter(-0.65f, -0.06f, 0.7f, 1.75f, 130) { Ink = 0.045f };
            p.Shape(Sd.Capsule(-0.13f, 0.08f, -0.13f, 0.45f, 0.08f), Rgba.Hex("3A5AA8"));
            p.Shape(Sd.Capsule(0.13f, 0.08f, 0.13f, 0.45f, 0.08f), Rgba.Hex("3A5AA8"));
            p.Shape(Sd.Ellipse(-0.17f, 0.05f, 0.13f, 0.06f), Rgba.Hex("E84A4A"));
            p.Shape(Sd.Ellipse(0.17f, 0.05f, 0.13f, 0.06f), Rgba.Hex("E84A4A"));
            // braccia
            p.Shape(Sd.Capsule(-0.3f, 0.75f, -0.5f, 0.45f, 0.07f), Rgba.Hex("FFD6B0"));
            if (holding) p.Shape(Sd.Capsule(0.3f, 0.8f, 0.45f, 1.35f, 0.07f), Rgba.Hex("FFD6B0"));
            else p.Shape(Sd.Capsule(0.3f, 0.8f, 0.55f, 1.15f, 0.07f), Rgba.Hex("FFD6B0"));
            // maglia a righe
            Sdf shirt = Sd.Box(0, 0.65f, 0.3f, 0.27f, 0.1f);
            p.Fill(shirt, Rgba.Hex("FFC93C"));
            for (float y = 0.48f; y < 0.92f; y += 0.12f) p.Fill(Sd.Intersect(shirt, Sd.Box(0, y, 0.4f, 0.035f)), Rgba.Hex("E84A4A"));
            p.Shape(shirt, Rgba.Clear);
            // testa
            Sdf head = Sd.Circle(0, 1.2f, 0.3f);
            Body(p, head, Rgba.Hex("FFD6B0"), Rgba.Hex("F2BC92"));
            p.Shape(Sd.Union(Sd.Ellipse(0, 1.45f, 0.3f, 0.12f), Sd.Circle(-0.2f, 1.42f, 0.1f), Sd.Circle(0.22f, 1.44f, 0.1f), Sd.Circle(0.05f, 1.55f, 0.09f)), Rgba.Hex("E87A2A"));
            p.Fill(Sd.Circle(-0.1f, 1.22f, 0.035f), Ink);
            p.Fill(Sd.Circle(0.1f, 1.22f, 0.035f), Ink);
            p.InkLine(0.03f, -0.08f, 1.08f, 0f, 1.04f, 0.08f, 1.08f);
            p.Fill(Sd.Circle(-0.18f, 1.12f, 0.05f), BlushColor.WithAlpha(0.6f));
            p.Fill(Sd.Circle(0.18f, 1.12f, 0.05f), BlushColor.WithAlpha(0.6f));
            p.Shape(Sd.Circle(holding ? 0.46f : 0.57f, holding ? 1.4f : 1.2f, 0.08f), Rgba.Hex("FFD6B0"), 0.03f);
            return new DoodleArt(holding ? "s41e_twin_hold" : "s41e_twin_wave", p);
        }

        /// <summary>Specchio verticale (vetro y -1..1, largo 0.5) con cornice dorata, centro 0,0.</summary>
        public static DoodleArt S41E_StandMirror()
        {
            var p = new DoodlePainter(-0.42f, -1.15f, 0.42f, 1.15f, 120) { Ink = 0.045f };
            Sdf frame = Sd.Box(0, 0, 0.33f, 1.08f, 0.3f);
            p.Shape(frame, Gold);
            Sdf glass = Sd.Box(0, 0, 0.24f, 0.98f, 0.22f);
            p.FillFunc(glass, (x, y) => Rgba.Lerp(Rgba.Hex("A8E0FF"), Rgba.Hex("E8F8FF"), S41E_Clamp(0.5f + (x + y * 0.3f) * 1.2f)));
            p.Line(Rgba.White.WithAlpha(0.9f), 0.05f, -0.12f, 0.3f, 0.06f, 0.7f);
            p.Line(Rgba.White.WithAlpha(0.7f), 0.03f, -0.1f, 0.05f, 0.0f, 0.25f);
            p.Shape(glass, Rgba.Clear, 0.03f);
            p.Shape(Sd.Circle(0, 1.08f, 0.07f), GoldLight, 0.025f);
            return new DoodleArt("s41e_stand_mirror", p);
        }

        /// <summary>Porta 1.0 x 2.2 (base in y 0, centro x 0), serratura in (-0.25, 1.4); aperta = vano scuro.</summary>
        public static DoodleArt S41E_TwinDoor(bool open)
        {
            var p = new DoodlePainter(-0.62f, -0.05f, 0.62f, 2.32f, 100) { Ink = 0.05f };
            p.Shape(Sd.Box(0, 1.12f, 0.56f, 1.15f, 0.06f), Rgba.Hex("FFD86A"));
            if (open)
            {
                p.FillFunc(Sd.Box(0, 1.1f, 0.46f, 1.08f), (x, y) => Rgba.Lerp(Rgba.Hex("FFF3B0"), Rgba.Hex("FFFFFF"), S41E_Clamp(y / 2.2f)));
                p.Shape(Sd.Polygon(0.46f, 0.02f, 0.46f, 2.18f, 0.6f, 2.0f, 0.6f, 0.15f), Rgba.Hex("A85E26"), 0.035f);
            }
            else
            {
                Sdf leaf = Sd.Box(0, 1.1f, 0.46f, 1.08f, 0.04f);
                Body(p, leaf, Rgba.Hex("D2843F"), Rgba.Hex("A85E26"));
                p.Shape(Sd.Box(0, 1.65f, 0.3f, 0.32f, 0.04f), Rgba.Hex("E39B55"), 0.03f);
                p.Shape(Sd.Box(0, 0.55f, 0.3f, 0.32f, 0.04f), Rgba.Hex("E39B55"), 0.03f);
                p.Shape(Sd.Box(-0.25f, 1.4f, 0.13f, 0.2f, 0.05f), Gold, 0.03f);
                p.Fill(Sd.Circle(-0.25f, 1.45f, 0.045f), Ink);
                p.Fill(Sd.Polygon(-0.28f, 1.42f, -0.22f, 1.42f, -0.25f, 1.3f), Ink);
            }
            return new DoodleArt(open ? "s41e_twin_door_open" : "s41e_twin_door", p);
        }

        // ================= THE HUNGRY PRINTER =================

        /// <summary>Mensola di legno (x ±2.5, piano superiore in y 0) con due staffe.</summary>
        public static DoodleArt S41E_Shelf()
        {
            var p = new DoodlePainter(-2.6f, -0.5f, 2.6f, 0.05f, 80) { Ink = 0.045f };
            foreach (float x in new[] { -1.8f, 1.8f }) p.Shape(Sd.Polygon(x - 0.08f, -0.12f, x + 0.08f, -0.12f, x + 0.08f, -0.45f), Rgba.Hex("6E7890"), 0.03f);
            Body(p, Sd.Box(0, -0.07f, 2.5f, 0.08f, 0.03f), Wood, WoodShade);
            return new DoodleArt("s41e_shelf", p);
        }

        /// <summary>Stampante con faccia (base in y 0, larga 2.6): fessura di carica in alto (0, 1.62), bocca di uscita in (0, 0.55).</summary>
        public static DoodleArt S41E_Printer()
        {
            var p = new DoodlePainter(-1.42f, -0.08f, 1.42f, 2.02f, 100) { Ink = 0.05f };
            // vassoio di carica dietro
            p.Shape(Sd.Polygon(-0.85f, 1.45f, 0.85f, 1.45f, 0.95f, 1.95f, -0.95f, 1.95f), Rgba.Hex("C8CCD8"), 0.04f);
            Sdf body = Sd.Box(0, 0.78f, 1.3f, 0.75f, 0.2f);
            Body(p, body, Rgba.Hex("EEF0F6"), Rgba.Hex("C8CCD8"));
            p.Shape(Sd.Box(0, 1.55f, 0.95f, 0.07f, 0.04f), Rgba.Hex("3A3A4A"), 0.03f);
            // occhi a LED
            foreach (float x in new[] { -0.75f, -0.35f })
            {
                p.Shape(Sd.Circle(x, 1.12f, 0.13f), Rgba.White, 0.035f);
                p.Fill(Sd.Circle(x + 0.02f, 1.1f, 0.06f), Ink);
            }
            // display
            p.Shape(Sd.Box(0.6f, 1.12f, 0.55f, 0.2f, 0.05f), Rgba.Hex("9AE0A8"), 0.035f);
            // bocca di uscita con dentini
            p.Shape(Sd.Box(0, 0.55f, 0.95f, 0.14f, 0.06f), Rgba.Hex("3A2A3A"), 0.04f);
            for (float x = -0.8f; x <= 0.81f; x += 0.2f) p.Fill(Sd.Polygon(x - 0.06f, 0.68f, x + 0.06f, 0.68f, x, 0.57f), Rgba.White);
            // vassoio di uscita
            p.Shape(Sd.Polygon(-0.9f, 0.36f, 0.9f, 0.36f, 1.05f, 0.12f, -1.05f, 0.12f), Rgba.Hex("B8BCC8"), 0.035f);
            p.Shape(Sd.Box(0, 0.02f, 1.1f, 0.05f, 0.02f), Rgba.Hex("6E7890"), 0.03f);
            return new DoodleArt("s41e_printer", p);
        }

        /// <summary>Foglio delle istruzioni appeso (1.9 x 1.25, centro 0,0) con puntina e angolo piegato.</summary>
        public static DoodleArt S41E_InstructionSheet()
        {
            var p = new DoodlePainter(-1.02f, -0.7f, 1.02f, 0.75f, 110) { Ink = 0.04f };
            Sdf sheet = Sd.Polygon(-0.95f, 0.62f, 0.95f, 0.62f, 0.95f, -0.38f, 0.72f, -0.62f, -0.95f, -0.62f);
            p.FillFunc(sheet, (x, y) => Rgba.Lerp(Rgba.Hex("F2EEE0"), Rgba.Hex("FFFDF6"), S41E_Clamp((y + 0.6f) / 1.2f)));
            for (float y = -0.15f; y > -0.55f; y -= 0.13f) p.Line(Rgba.Hex("B8C8E8"), 0.02f, -0.85f, y, 0.6f, y);
            p.Shape(sheet, Rgba.Clear);
            p.Shape(Sd.Polygon(0.72f, -0.62f, 0.72f, -0.38f, 0.95f, -0.38f), Rgba.Hex("D8D2C0"), 0.03f);
            p.Shape(Sd.Circle(0, 0.6f, 0.08f), Rgba.Hex("E84A4A"), 0.03f);
            return new DoodleArt("s41e_instr_sheet", p);
        }

        /// <summary>Foglio bianco (0.8 x 1.0, centro 0,0).</summary>
        public static DoodleArt S41E_BlankPage()
        {
            var p = new DoodlePainter(-0.45f, -0.55f, 0.45f, 0.55f, 100) { Ink = 0.035f };
            p.Shape(Sd.Box(0, 0, 0.4f, 0.5f, 0.02f), Rgba.White);
            return new DoodleArt("s41e_blank_page", p);
        }

        /// <summary>Mappa del tesoro stampata (1.0 x 1.3, centro 0,0).</summary>
        public static DoodleArt S41E_MapSheet()
        {
            var p = new DoodlePainter(-0.56f, -0.71f, 0.56f, 0.71f, 130) { Ink = 0.035f };
            Sdf sheet = Sd.Box(0, 0, 0.5f, 0.65f, 0.02f);
            p.Fill(sheet, Rgba.White);
            p.Shape(Sd.Union(Sd.Circle(-0.15f, 0.05f, 0.3f), Sd.Circle(0.12f, -0.12f, 0.28f), Sd.Circle(0.05f, 0.25f, 0.2f)), Rgba.Hex("BFE59A"), 0.025f);
            p.Line(Rgba.Hex("D8263A"), 0.025f, -0.38f, -0.5f, -0.3f, -0.4f);
            p.Line(Rgba.Hex("D8263A"), 0.025f, -0.24f, -0.32f, -0.16f, -0.2f);
            p.Line(Rgba.Hex("D8263A"), 0.025f, -0.08f, -0.1f, 0.0f, 0.0f);
            p.Line(Rgba.Hex("D8263A"), 0.05f, 0.05f, 0.08f, 0.23f, 0.26f);
            p.Line(Rgba.Hex("D8263A"), 0.05f, 0.23f, 0.08f, 0.05f, 0.26f);
            p.Shape(Sd.Circle(-0.3f, 0.45f, 0.1f), Rgba.Hex("FFF3B0"), 0.02f);
            p.InkLine(0.02f, -0.3f, 0.38f, -0.3f, 0.52f);
            p.Shape(sheet, Rgba.Clear);
            return new DoodleArt("s41e_map_sheet", p);
        }

        // ================= FINGER MAZE =================

        /// <summary>Labirinto della "Finger maze": righe di caratteri, # muro; celle sulle posizioni dispari.</summary>
        public static readonly string[] S41E_FingerMazeRows =
        {
            "#########.#",
            "#...#.....#",
            "#.###.###.#",
            "#.#...#...#",
            "#.#.###.#.#",
            "#...#...#.#",
            "#.###.###.#",
            "#...#.#...#",
            "#####.#.###",
            "#...#.#.#.#",
            "#.#.#.#.#.#",
            "#.#.#.#.#.#",
            "#.#.#.#.#.#",
            "#.#...#...#",
            "###########",
        };

        public const float S41E_MazeLeft = -2.25f, S41E_MazeTop = 2.25f, S41E_MazeCell = 0.9f, S41E_MazeWall = 0.14f;

        static void S41E_MazeSpan(int i, float origin, float sign, out float lo, out float hi)
        {
            if (i % 2 == 0)
            {
                float line = origin + sign * (i / 2) * S41E_MazeCell;
                lo = line - S41E_MazeWall * 0.5f; hi = line + S41E_MazeWall * 0.5f;
            }
            else
            {
                int k = (i - 1) / 2;
                float a = origin + sign * k * S41E_MazeCell, b = origin + sign * (k + 1) * S41E_MazeCell;
                lo = Math.Min(a, b) + S41E_MazeWall * 0.5f; hi = Math.Max(a, b) - S41E_MazeWall * 0.5f;
            }
        }

        /// <summary>Muri del labirinto come rettangoli (cx, cy, hx, hy) in coordinate di gioco.</summary>
        public static List<float[]> S41E_MazeBoxes()
        {
            var list = new List<float[]>();
            string[] g = S41E_FingerMazeRows;
            int rows = g.Length, cols = g[0].Length;
            // tratti orizzontali nelle righe pari
            for (int r = 0; r < rows; r += 2)
            {
                int c = 0;
                while (c < cols)
                {
                    if (g[r][c] != '#') { c++; continue; }
                    int s = c;
                    while (c < cols && g[r][c] == '#') c++;
                    S41E_MazeSpan(s, S41E_MazeLeft, 1f, out float x0, out _);
                    S41E_MazeSpan(c - 1, S41E_MazeLeft, 1f, out _, out float x1);
                    S41E_MazeSpan(r, S41E_MazeTop, -1f, out float y0, out float y1);
                    list.Add(new[] { (x0 + x1) * 0.5f, (y0 + y1) * 0.5f, (x1 - x0) * 0.5f, (y1 - y0) * 0.5f });
                }
            }
            // tratti verticali nelle colonne pari
            for (int c = 0; c < cols; c += 2)
            {
                int r = 0;
                while (r < rows)
                {
                    if (g[r][c] != '#') { r++; continue; }
                    int s = r;
                    while (r < rows && g[r][c] == '#') r++;
                    if (r - s < 2) continue;
                    S41E_MazeSpan(s, S41E_MazeTop, -1f, out float ya, out float yb);
                    S41E_MazeSpan(r - 1, S41E_MazeTop, -1f, out float yc, out float yd);
                    float y0 = Math.Min(ya, yc), y1 = Math.Max(yb, yd);
                    S41E_MazeSpan(c, S41E_MazeLeft, 1f, out float x0, out float x1);
                    list.Add(new[] { (x0 + x1) * 0.5f, (y0 + y1) * 0.5f, (x1 - x0) * 0.5f, (y1 - y0) * 0.5f });
                }
            }
            return list;
        }

        /// <summary>Tavolo con il labirinto disegnato (x -2.25..2.25, y 2.25..-4.05), uscita in alto a destra.</summary>
        public static DoodleArt S41E_FingerMazeBg()
        {
            var p = new DoodlePainter(-3.1f, -5.2f, 3.1f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.2f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("7AB86A"), Rgba.Hex("A6D88A"), S41E_Clamp((y + 5f) / 10f)));
            for (int i = 0; i < 40; i++)
            {
                float x = -3f + S41E_Hash(i * 5 + 11) * 6f, y = -5f + S41E_Hash(i * 5 + 12) * 10f;
                p.Line(Rgba.Hex("5E9A50", 0.6f), 0.025f, x, y, x + 0.05f, y + 0.12f);
            }
            float right = S41E_MazeLeft + 5 * S41E_MazeCell, bottom = S41E_MazeTop - 7 * S41E_MazeCell;
            Sdf board = Sd.Box((S41E_MazeLeft + right) * 0.5f, (S41E_MazeTop + bottom) * 0.5f, (right - S41E_MazeLeft) * 0.5f + 0.15f, (S41E_MazeTop - bottom) * 0.5f + 0.15f, 0.12f);
            p.Shape(board, Rgba.Hex("FFF6E0"), 0.06f);
            // puntini di partenza
            p.Fill(Sd.Circle(S41E_MazeLeft + 0.45f, bottom + 0.45f, 0.25f), Rgba.Hex("FFD86A", 0.7f));
            var walls = new List<Sdf>();
            foreach (float[] b in S41E_MazeBoxes()) walls.Add(Sd.Box(b[0], b[1], b[2], b[3], 0.04f));
            Sdf all = Sd.Union(walls.ToArray());
            p.Shape(all, Rgba.Hex("4A6AD8"), 0.03f);
            return new DoodleArt("s41e_fmaze_bg", p);
        }

        /// <summary>Impronta digitale gigante (centro 0,0; rettangolo smussato 2.55 x 3.45 di semi-lati), opaca.</summary>
        public static DoodleArt S41E_Fingerprint()
        {
            var p = new DoodlePainter(-2.7f, -3.6f, 2.7f, 3.6f, 40) { Ink = 0.06f };
            Sdf pad = Sd.Box(0, 0, 2.55f, 3.45f, 1.6f);
            p.FillFunc(pad, (x, y) =>
            {
                float d = (float)Math.Sqrt(x * x * 1.3f + (y + 0.6f) * (y + 0.6f) * 0.75f);
                float w = d * 7.5f + 0.5f * S41E_Sin(x * 2.1f + y * 1.3f) + 0.35f * S41E_Sin(y * 3.7f);
                float ridge = 0.5f + 0.5f * S41E_Sin(w);
                return Rgba.Lerp(Rgba.Hex("D9C9B8"), Rgba.Hex("8A7466"), ridge * ridge);
            });
            p.Shape(pad, Rgba.Clear, 0.06f);
            return new DoodleArt("s41e_fingerprint", p);
        }

        // ================= FALSE FINISH =================

        /// <summary>Stadio: tribune in alto, due corsie (y 0.2 e -1.4), traguardo a scacchi in x 1.9.</summary>
        public static DoodleArt S41E_RaceBg()
        {
            var p = new DoodlePainter(-3.1f, -5.2f, 3.1f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.2f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("BDE6FF"), Rgba.Hex("7FC8FF"), S41E_Clamp((y - 1f) / 4f)));
            // tribune con pubblico
            p.Shape(Sd.Box(0, 2.0f, 3.2f, 0.9f), Rgba.Hex("8A6AC0"), 0.05f);
            string[] crowd = { "FF7F9C", "FFC93C", "6FD0FF", "8CCB6E", "FF9A5A", "C9A0FF" };
            for (int row = 0; row < 3; row++)
                for (int i = 0; i < 11; i++)
                {
                    float x = -2.9f + i * 0.58f + (row % 2) * 0.29f, y = 1.4f + row * 0.45f;
                    p.Shape(Sd.Circle(x, y, 0.17f), Rgba.Hex(crowd[(i + row * 2) % crowd.Length]), 0.03f);
                }
            // pista
            p.Fill(Sd.Box(0, -0.6f, 3.2f, 1.6f), Rgba.Hex("E2734A"), false);
            foreach (float y in new[] { 1.0f, -0.6f, -2.2f }) p.Fill(Sd.Box(0, y, 3.2f, 0.035f), Rgba.White, false);
            p.Line(Rgba.Hex("2B2233"), 0.05f, -3.2f, 1.0f, 3.2f, 1.0f);
            // erba
            p.FillFunc(Sd.Box(0, -3.75f, 3.2f, 1.55f), (x, y) => Rgba.Lerp(Rgba.Hex("6FB55A"), Rgba.Hex("8CCB6E"), S41E_Clamp((y + 5.2f) / 3f)));
            p.Line(Rgba.Hex("2B2233"), 0.05f, -3.2f, -2.2f, 3.2f, -2.2f);
            // traguardo a scacchi
            p.FillFunc(Sd.Box(1.9f, -0.6f, 0.14f, 1.6f), (x, y) =>
            {
                int k = (int)Math.Floor((x - 1.76f) / 0.14f) + (int)Math.Floor((y + 10f) / 0.14f);
                return k % 2 == 0 ? Rgba.White : Rgba.Hex("2B2233");
            });
            // pali e striscione
            foreach (float x in new[] { 1.7f, 2.1f }) p.Shape(Sd.Box(x, 1.55f, 0.05f, 0.55f), Rgba.Hex("D8D8E0"), 0.03f);
            p.Shape(Sd.Box(1.9f, 2.05f, 0.45f, 0.18f, 0.04f), Rgba.Hex("FFE58A"), 0.035f);
            return new DoodleArt("s41e_race_bg", p);
        }

        /// <summary>Corridore con fascia e pettorina (piedi in y 0, alto 1.35), in corsa verso destra.</summary>
        public static DoodleArt S41E_Runner()
        {
            var p = new DoodlePainter(-0.7f, -0.06f, 0.75f, 1.45f, 130) { Ink = 0.045f };
            p.Shape(Sd.Capsule(-0.05f, 0.45f, -0.4f, 0.12f, 0.08f), Rgba.Hex("3A3A4A"));
            p.Shape(Sd.Capsule(0.05f, 0.45f, 0.35f, 0.15f, 0.08f), Rgba.Hex("3A3A4A"));
            p.Shape(Sd.Ellipse(-0.45f, 0.08f, 0.14f, 0.07f), Rgba.Hex("6FD0FF"));
            p.Shape(Sd.Ellipse(0.42f, 0.1f, 0.14f, 0.07f), Rgba.Hex("6FD0FF"));
            p.Shape(Sd.Capsule(-0.15f, 0.8f, -0.5f, 0.6f, 0.07f), Rgba.Hex("FFD6B0"));
            Sdf torso = Sd.Box(0, 0.7f, 0.27f, 0.27f, 0.1f);
            Body(p, torso, Rgba.Hex("4AB8FF"), Rgba.Hex("2A8AD8"));
            p.Shape(Sd.Box(0, 0.68f, 0.14f, 0.11f, 0.02f), Rgba.White, 0.025f);
            p.Shape(Sd.Capsule(0.15f, 0.8f, 0.5f, 1.0f, 0.07f), Rgba.Hex("FFD6B0"));
            Sdf head = Sd.Circle(0.08f, 1.15f, 0.27f);
            Body(p, head, Rgba.Hex("FFD6B0"), Rgba.Hex("F2BC92"));
            p.Shape(Sd.Box(0.08f, 1.27f, 0.28f, 0.05f, 0.02f), Rgba.Hex("E84A4A"), 0.025f);
            p.Fill(Sd.Circle(0.2f, 1.15f, 0.035f), Ink);
            p.InkLine(0.03f, 0.12f, 1.02f, 0.22f, 1.0f, 0.3f, 1.04f);
            p.Line(Rgba.White.WithAlpha(0.9f), 0.04f, -0.7f, 0.9f, -0.45f, 0.9f);
            p.Line(Rgba.White.WithAlpha(0.9f), 0.04f, -0.65f, 0.6f, -0.45f, 0.6f);
            return new DoodleArt("s41e_runner", p);
        }

        /// <summary>Grosso pulsante verde con freccia (1.8 x 0.8, centro 0,0).</summary>
        public static DoodleArt S41E_NextButton()
        {
            var p = new DoodlePainter(-1.0f, -0.52f, 1.0f, 0.48f, 110) { Ink = 0.05f };
            p.Shape(Sd.Box(0, -0.06f, 0.9f, 0.38f, 0.2f), Rgba.Hex("2A8A3A"));
            Body(p, Sd.Box(0, 0.02f, 0.9f, 0.38f, 0.2f), Rgba.Hex("6AD86A"), Rgba.Hex("4AB84A"));
            p.Shape(Sd.Polygon(0.45f, 0.2f, 0.75f, 0.02f, 0.45f, -0.16f), Rgba.White, 0.03f);
            return new DoodleArt("s41e_next_btn", p);
        }

        /// <summary>Coppa dorata (base in y 0, alta 0.8).</summary>
        public static DoodleArt S41E_Trophy()
        {
            var p = new DoodlePainter(-0.5f, -0.05f, 0.5f, 0.85f, 150) { Ink = 0.04f };
            p.Shape(Sd.Subtract(Sd.Circle(-0.3f, 0.6f, 0.13f), Sd.Circle(-0.3f, 0.6f, 0.07f)), Gold, 0.03f);
            p.Shape(Sd.Subtract(Sd.Circle(0.3f, 0.6f, 0.13f), Sd.Circle(0.3f, 0.6f, 0.07f)), Gold, 0.03f);
            p.Shape(Sd.Box(0, 0.07f, 0.25f, 0.07f, 0.02f), Rgba.Hex("8A5128"), 0.03f);
            p.Shape(Sd.Box(0, 0.22f, 0.05f, 0.1f), GoldShade, 0.03f);
            Sdf bowl = Sd.Intersect(Sd.Circle(0, 0.68f, 0.3f), Sd.Box(0, 0.55f, 0.35f, 0.25f));
            Body(p, Sd.Union(bowl, Sd.Box(0, 0.78f, 0.3f, 0.03f)), Gold, GoldShade);
            p.Fill(Sd.Ellipse(-0.12f, 0.6f, 0.05f, 0.1f), GoldLight);
            return new DoodleArt("s41e_trophy", p);
        }

        // ================= HEARTBEAT =================

        /// <summary>Corsia d'ospedale: pareti menta, finestra, letto con cuscino (materasso in y -1.4).</summary>
        public static DoodleArt S41E_WardBg()
        {
            var p = new DoodlePainter(-3.1f, -5.2f, 3.1f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.2f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("B8E6D8"), Rgba.Hex("D8F4EA"), S41E_Clamp((y + 2f) / 7f)));
            p.FillFunc(Sd.Box(0, -4.0f, 3.2f, 1.25f), (x, y) =>
            {
                int k = (int)Math.Floor((x + 10f) / 0.6f) + (int)Math.Floor((y + 10f) / 0.6f);
                return k % 2 == 0 ? Rgba.Hex("E8F0F4") : Rgba.Hex("C8D8E2");
            });
            p.Shape(Sd.Box(0, -2.75f, 3.2f, 0.06f), Rgba.Hex("8AB8A8"), 0.03f);
            // finestra a sinistra in alto e croce verde
            p.Shape(Sd.Box(-1.9f, 2.5f, 0.75f, 0.6f, 0.05f), Rgba.Hex("BDE6FF"), 0.05f);
            p.InkLine(0.04f, -1.9f, 1.9f, -1.9f, 3.1f);
            p.Shape(Sd.Union(Sd.Box(1.9f, 2.6f, 0.12f, 0.35f), Sd.Box(1.9f, 2.6f, 0.35f, 0.12f)), Rgba.Hex("5ACB7A"), 0.04f);
            // letto
            p.Shape(Sd.Box(-2.45f, -1.6f, 0.1f, 1.1f, 0.05f), Rgba.Hex("C8CCD8"), 0.04f);
            p.Shape(Sd.Box(2.45f, -1.9f, 0.1f, 0.8f, 0.05f), Rgba.Hex("C8CCD8"), 0.04f);
            p.Shape(Sd.Box(0, -1.85f, 2.4f, 0.45f, 0.1f), Rgba.Hex("FFFFFF"), 0.045f);
            p.Shape(Sd.Box(0, -2.35f, 2.45f, 0.08f, 0.03f), Rgba.Hex("9AA4B8"), 0.035f);
            foreach (float x in new[] { -2.2f, 2.2f }) p.Shape(Sd.Box(x, -2.6f, 0.06f, 0.2f), Rgba.Hex("6E7890"), 0.03f);
            return new DoodleArt("s41e_ward_bg", p);
        }

        /// <summary>Monitor del battito: schermo scuro 4.0 x 1.4 (x ±2.0, y ±0.7) col braccio, centro 0,0.</summary>
        public static DoodleArt S41E_Monitor()
        {
            var p = new DoodlePainter(-2.35f, -1.05f, 2.35f, 1.0f, 80) { Ink = 0.055f };
            Body(p, Sd.Box(0, 0, 2.25f, 0.92f, 0.2f), Rgba.Hex("D8DCE8"), Rgba.Hex("B8BCC8"));
            p.FillFunc(Sd.Box(0, 0, 2.0f, 0.7f, 0.1f), (x, y) => Rgba.Lerp(Rgba.Hex("061A10"), Rgba.Hex("0E2A1C"), S41E_Clamp((y + 0.7f) / 1.4f)));
            for (float x = -1.8f; x < 2f; x += 0.4f) p.Line(Rgba.Hex("1E4A30", 0.6f), 0.012f, x, -0.65f, x, 0.65f);
            for (float y = -0.6f; y < 0.7f; y += 0.3f) p.Line(Rgba.Hex("1E4A30", 0.6f), 0.012f, -1.95f, y, 1.95f, y);
            p.Shape(Sd.Circle(1.9f, -0.82f, 0.06f), Rgba.Hex("FF5A5A"), 0.02f);
            p.Shape(Sd.Circle(1.65f, -0.82f, 0.06f), Rgba.Hex("5ACB7A"), 0.02f);
            return new DoodleArt("s41e_monitor", p);
        }

        /// <summary>Mostriciattolo viola seduto nel letto (base y 0, alto 1.7): addormentato o sveglio e felice.</summary>
        public static DoodleArt S41E_Monster(bool awake)
        {
            var p = new DoodlePainter(-1.0f, -0.08f, 1.0f, 1.85f, 120) { Ink = 0.045f };
            Rgba fur = Rgba.Hex("A87AE8"), furShade = Rgba.Hex("8458C8");
            // corna
            p.Shape(Sd.Polygon(-0.45f, 1.35f, -0.25f, 1.45f, -0.5f, 1.8f), Rgba.Hex("FFF1C8"), 0.03f);
            p.Shape(Sd.Polygon(0.45f, 1.35f, 0.25f, 1.45f, 0.5f, 1.8f), Rgba.Hex("FFF1C8"), 0.03f);
            if (awake)
            {
                p.Shape(Sd.Capsule(-0.55f, 0.6f, -0.9f, 1.15f, 0.12f), fur);
                p.Shape(Sd.Capsule(0.55f, 0.6f, 0.9f, 1.15f, 0.12f), fur);
            }
            Sdf body = Sd.Union(Sd.Ellipse(0, 0.75f, 0.62f, 0.72f), Sd.Box(0, 0.25f, 0.62f, 0.25f, 0.1f));
            Body(p, body, fur, furShade, Rgba.Hex("6A40A8"));
            p.Fill(Sd.Ellipse(0, 0.55f, 0.35f, 0.3f), Rgba.Hex("D8C0FF"));
            if (awake)
            {
                S41E_Eyes(p, 0, 1.05f, 0.2f, 0.13f, 0f, 0.3f);
                p.Shape(Sd.Intersect(Sd.Circle(0, 0.82f, 0.18f), Sd.Box(0, 0.72f, 0.25f, 0.1f)), Rgba.Hex("8A2A3A"), 0.03f);
                p.Fill(Sd.Polygon(-0.08f, 0.82f, 0.0f, 0.82f, -0.04f, 0.74f), Rgba.White);
            }
            else
            {
                p.InkLine(0.04f, -0.32f, 1.05f, -0.2f, 1.0f, -0.08f, 1.05f);
                p.InkLine(0.04f, 0.08f, 1.05f, 0.2f, 1.0f, 0.32f, 1.05f);
                p.Shape(Sd.Ellipse(0, 0.8f, 0.07f, 0.05f), Rgba.Hex("6A2A4A"), 0.025f);
            }
            p.Fill(Sd.Circle(-0.38f, 0.9f, 0.07f), BlushColor.WithAlpha(0.55f));
            p.Fill(Sd.Circle(0.38f, 0.9f, 0.07f), BlushColor.WithAlpha(0.55f));
            // coperta
            Sdf blanket = Sd.Box(0, 0.12f, 0.95f, 0.2f, 0.08f);
            p.Shape(blanket, Rgba.Hex("6FD0FF"));
            p.Line(Rgba.White.WithAlpha(0.8f), 0.04f, -0.85f, 0.2f, 0.85f, 0.2f);
            return new DoodleArt(awake ? "s41e_monster_awake" : "s41e_monster_sleep", p);
        }

        /// <summary>Grosso cuore rosso (largo 1.4, centro 0,0).</summary>
        public static DoodleArt S41E_HeartButton()
        {
            var p = new DoodlePainter(-0.8f, -0.75f, 0.8f, 0.7f, 120) { Ink = 0.055f };
            Sdf heart = Sd.Union(Sd.Circle(-0.33f, 0.2f, 0.37f), Sd.Circle(0.33f, 0.2f, 0.37f), Sd.Polygon(-0.69f, 0.16f, 0.69f, 0.16f, 0f, -0.68f), Sd.Box(0, 0.18f, 0.4f, 0.16f));
            Body(p, heart, Rgba.Hex("FF4A6A"), Rgba.Hex("D82A4A"));
            p.Fill(Sd.Ellipse(-0.38f, 0.32f, 0.12f, 0.08f), Rgba.White.WithAlpha(0.75f));
            return new DoodleArt("s41e_heart_btn", p);
        }

        /// <summary>Icona altoparlante (0.7, centro 0,0), acceso con le onde o spento con la X.</summary>
        public static DoodleArt S41E_Speaker(bool on)
        {
            var p = new DoodlePainter(-0.42f, -0.4f, 0.45f, 0.4f, 120) { Ink = 0.04f };
            p.Shape(Sd.Circle(0, 0, 0.38f), on ? Rgba.Hex("FFE58A") : Rgba.Hex("D8D8E0"), 0.04f);
            p.Shape(Sd.Union(Sd.Box(-0.15f, 0, 0.07f, 0.08f), Sd.Polygon(-0.1f, 0.08f, 0.05f, 0.2f, 0.05f, -0.2f, -0.1f, -0.08f)), Rgba.Hex("3A3A4A"), 0.02f);
            if (on)
            {
                p.InkLine(0.035f, 0.12f, 0.1f, 0.16f, 0f, 0.12f, -0.1f);
                p.InkLine(0.035f, 0.2f, 0.18f, 0.27f, 0f, 0.2f, -0.18f);
            }
            else
            {
                p.Line(Rgba.Hex("D8263A"), 0.045f, 0.1f, 0.1f, 0.26f, -0.1f);
                p.Line(Rgba.Hex("D8263A"), 0.045f, 0.1f, -0.1f, 0.26f, 0.1f);
            }
            return new DoodleArt(on ? "s41e_speaker_on" : "s41e_speaker_off", p);
        }
    }
}
