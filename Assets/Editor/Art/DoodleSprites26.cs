using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli compositi della versione 0.40 (gruppo C).</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV40C()
        {
            var list = new List<DoodleArt>
            {
                V40C_Dot(),
                BombCase(), BombManual(), BombWire(false), BombWire(true), BombButton(), BombKey(), BombFlag(),
                PotionBackdrop(), PotionBook(), PotionCauldron(), PotionLiquid(), PotionThermo(), PotionLadle(),
                PotionWater(), PotionMilk(), PotionEgg(), PotionHoney(), PotionMushroom(),
                FactoryBackdrop(), FactoryBelt(), RobotTorso(), RobotHead(), RobotArm(), RobotLegs(), Blueprint(), PartsBin(),
                FloorTile(), FactoryCrate(), MiniBot(), ChargeCell(), CommandCard(0), CommandCard(1), CommandCard(2), CommandSlot(), RunButton(),
                TrainBackdrop(), TrainLoco(), TrainStation(), TrainSwitch(), TrainArrow(), TrainTunnel(),
            };
            for (int i = 0; i < 11; i++) list.Add(BombSymbol(i));
            return list;
        }

        static readonly Rgba V40C_Ink = DoodlePainter.DefaultInk;

        /// <summary>Pallino del progresso delle fasi (bianco: si tinge).</summary>
        public static DoodleArt V40C_Dot()
        {
            var p = new DoodlePainter(-0.14f, -0.14f, 0.14f, 0.14f, 200) { Ink = 0.035f, Wobble = 0.004f };
            p.Shape(Sd.Circle(0, 0, 0.11f), Rgba.White);
            p.Fill(Sd.Circle(-0.035f, 0.035f, 0.03f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("v40c_dot", p);
        }

        // ---------- Bomb squad ----------

        /// <summary>Corpo della bomba con i vani dei moduli (coordinate del livello: x -0.05..2.85, y -4.45..3.05).</summary>
        public static DoodleArt BombCase()
        {
            var p = new DoodlePainter(-0.15f, -4.55f, 2.95f, 3.15f, 80) { Ink = 0.05f };
            Sdf body = Sd.Box(1.4f, -0.7f, 1.45f, 3.75f, 0.16f);
            Body(p, body, Rgba.Hex("5E6575"), Rgba.Hex("454B58"));
            // bulloni agli angoli
            foreach (var c in new[] { (0.1f, 2.95f), (2.7f, 2.95f), (0.1f, -4.35f), (2.7f, -4.35f) })
            {
                p.Shape(Sd.Circle(c.Item1, c.Item2, 0.06f), Rgba.Hex("A7ADBA"), 0.025f);
                p.InkLine(0.015f, c.Item1 - 0.035f, c.Item2, c.Item1 + 0.035f, c.Item2);
            }
            Rgba bay = Rgba.Hex("2E323C"), bayEdge = Rgba.Hex("232630");
            // display del timer e vano degli errori
            p.Shape(Sd.Box(1.0f, 2.6f, 0.82f, 0.3f, 0.06f), Rgba.Hex("140F12"));
            p.Shape(Sd.Box(2.27f, 2.6f, 0.38f, 0.3f, 0.06f), Rgba.Hex("140F12"));
            // vani dei moduli
            foreach (var b in new[] { (1.35f, 0.75f), (-0.6f, 0.85f), (-3.0f, 1.3f) })
            {
                Sdf s = Sd.Box(1.4f, b.Item1, 1.32f, b.Item2, 0.08f);
                p.Shape(s, bay);
                p.Fill(Sd.Box(1.4f, b.Item1 + b.Item2 - 0.05f, 1.28f, 0.04f), bayEdge);
            }
            // fessura della striscia luminosa del pulsante
            p.Shape(Sd.Box(2.2f, -0.6f, 0.2f, 0.7f, 0.05f), Rgba.Hex("15171C"));
            // cavi decorativi fra i vani
            p.Line(Rgba.Hex("C2463E"), 0.04f, 0.06f, 2.2f, -0.02f, 1.5f, 0.06f, 0.4f);
            p.Line(Rgba.Hex("3E7BD8"), 0.04f, 2.76f, 0.4f, 2.84f, -0.3f, 2.76f, -1.6f);
            return new DoodleArt("bs_case", p);
        }

        /// <summary>Pagina del manuale con gli anelli (coordinate del livello: x -2.8..-0.15, y -4.45..3.05).</summary>
        public static DoodleArt BombManual()
        {
            var p = new DoodlePainter(-2.9f, -4.55f, -0.05f, 3.25f, 80) { Ink = 0.045f };
            Sdf page = Sd.Box(-1.475f, -0.7f, 1.3f, 3.72f, 0.08f);
            p.Shape(Sd.Offset(page, 0.05f, -0.05f), Rgba.Hex("D8CDB0"));
            p.Shape(page, Rgba.Hex("FBF3DC"));
            for (float y = 2.55f; y > -3.9f; y -= 0.3f) p.Line(Rgba.Hex("B9D3E8", 0.55f), 0.012f, -2.65f, y, -0.3f, y);
            p.Line(Rgba.Hex("E59A9A", 0.7f), 0.015f, -2.45f, 2.9f, -2.45f, -4.3f);
            foreach (float x in new[] { -2.3f, -1.475f, -0.65f })
            {
                p.Shape(Sd.Circle(x, 2.88f, 0.07f), Rgba.Hex("6B6677"), 0.025f);
                p.Shape(Sd.Subtract(Sd.Box(x, 3.02f, 0.06f, 0.16f, 0.06f), Sd.Box(x, 2.95f, 0.025f, 0.12f, 0.025f)), Rgba.Hex("B8BCC6"), 0.025f);
            }
            return new DoodleArt("bs_manual", p);
        }

        /// <summary>Filo bianco (si tinge) con i morsetti (verticale, lungo 1.36, centro 0,0); tagliato = due monconi.</summary>
        public static DoodleArt BombWire(bool cut)
        {
            var p = new DoodlePainter(-0.2f, -0.72f, 0.2f, 0.72f, 160) { Ink = 0.03f, Wobble = 0.004f };
            if (!cut)
                p.Shape(Sd.Polyline(0.055f, 0, -0.6f, 0.07f, -0.3f, -0.06f, 0f, 0.07f, 0.3f, 0, 0.6f), Rgba.White);
            else
            {
                p.Shape(Sd.Polyline(0.055f, 0, -0.6f, 0.07f, -0.3f, -0.02f, -0.12f), Rgba.White);
                p.Shape(Sd.Polyline(0.055f, 0.02f, 0.12f, 0.07f, 0.3f, 0, 0.6f), Rgba.White);
                p.Line(Rgba.Hex("C9A060"), 0.02f, -0.04f, -0.12f, -0.07f, -0.06f);
                p.Line(Rgba.Hex("C9A060"), 0.02f, 0.02f, 0.12f, 0.06f, 0.06f);
            }
            foreach (float y in new[] { -0.62f, 0.62f })
                p.Shape(Sd.Box(0, y, 0.12f, 0.06f, 0.02f), Rgba.Hex("B8BCC6"), 0.025f);
            return new DoodleArt(cut ? "bs_wire_cut" : "bs_wire", p);
        }

        /// <summary>Pulsante grande bianco (si tinge; raggio 0.55, centro 0,0).</summary>
        public static DoodleArt BombButton()
        {
            var p = new DoodlePainter(-0.66f, -0.66f, 0.66f, 0.66f, 140) { Ink = 0.045f };
            p.Shape(Sd.Circle(0, -0.04f, 0.6f), Rgba.Hex("9AA0AC"));
            Body(p, Sd.Circle(0, 0.02f, 0.52f), Rgba.White, Rgba.Hex("D2D2D2"));
            p.Fill(Sd.Ellipse(-0.2f, 0.25f, 0.14f, 0.07f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("bs_button", p);
        }

        /// <summary>Tasto del tastierino a simboli (1.05 x 1.05, centro 0,0).</summary>
        public static DoodleArt BombKey()
        {
            var p = new DoodlePainter(-0.58f, -0.6f, 0.58f, 0.58f, 120) { Ink = 0.04f };
            p.Shape(Sd.Box(0, -0.04f, 0.52f, 0.52f, 0.08f), Rgba.Hex("8F8A7C"));
            Body(p, Sd.Box(0, 0.01f, 0.5f, 0.5f, 0.08f), Rgba.White, Rgba.Hex("E2DCCB"));
            return new DoodleArt("bs_key", p);
        }

        /// <summary>Simbolo del tastierino (inchiostro, circa 0.5 di lato, centro 0,0).</summary>
        public static DoodleArt BombSymbol(int k)
        {
            var p = new DoodlePainter(-0.3f, -0.3f, 0.3f, 0.3f, 160) { Ink = 0.035f, Wobble = 0.004f };
            Rgba ink = V40C_Ink;
            const float w = 0.05f;
            switch (k)
            {
                case 0:   // stella
                {
                    var pts = new float[22];
                    for (int i = 0; i <= 10; i++)
                    {
                        double a = Math.PI / 2 + i * Math.PI / 5;
                        float r = i % 2 == 0 ? 0.25f : 0.1f;
                        pts[i * 2] = (float)Math.Cos(a) * r;
                        pts[i * 2 + 1] = (float)Math.Sin(a) * r - 0.02f;
                    }
                    p.Line(ink, w, pts);
                    break;
                }
                case 1:   // luna
                    p.Fill(Sd.Subtract(Sd.Circle(0, 0, 0.24f), Sd.Circle(0.1f, 0.06f, 0.2f)), ink);
                    break;
                case 2:   // omega
                    p.Line(ink, w, -0.24f, -0.22f, -0.1f, -0.22f, -0.17f, -0.05f, -0.15f, 0.12f, -0.05f, 0.22f, 0.05f, 0.22f, 0.15f, 0.12f, 0.17f, -0.05f, 0.1f, -0.22f, 0.24f, -0.22f);
                    break;
                case 3:   // tridente
                    p.Line(ink, w, 0, -0.26f, 0, 0.24f);
                    p.Line(ink, w, -0.2f, 0.22f, -0.2f, 0.05f, 0, -0.04f, 0.2f, 0.05f, 0.2f, 0.22f);
                    p.Line(ink, w, -0.1f, -0.16f, 0.1f, -0.16f);
                    break;
                case 4:   // spirale
                {
                    var pts = new List<float>();
                    for (int i = 0; i <= 40; i++)
                    {
                        double a = i * 0.42;
                        float r = 0.02f + i * 0.0055f;
                        pts.Add((float)Math.Cos(a) * r);
                        pts.Add((float)Math.Sin(a) * r);
                    }
                    p.Line(ink, w * 0.9f, pts.ToArray());
                    break;
                }
                case 5:   // occhio
                    p.Shape(Sd.Intersect(Sd.Circle(0, -0.18f, 0.32f), Sd.Circle(0, 0.18f, 0.32f)), Rgba.White);
                    p.Fill(Sd.Circle(0, 0, 0.07f), ink);
                    break;
                case 6:   // triangolo col punto
                    p.Line(ink, w, 0, 0.24f, -0.24f, -0.18f, 0.24f, -0.18f, 0, 0.24f);
                    p.Fill(Sd.Circle(0, -0.04f, 0.05f), ink);
                    break;
                case 7:   // fulmine
                    p.Fill(Sd.Polygon(0.06f, 0.26f, -0.16f, -0.02f, -0.01f, -0.02f, -0.08f, -0.26f, 0.17f, 0.05f, 0.02f, 0.05f), ink);
                    break;
                case 8:   // clessidra
                    p.Line(ink, w, -0.18f, 0.22f, 0.18f, 0.22f, -0.18f, -0.22f, 0.18f, -0.22f, -0.18f, 0.22f);
                    break;
                case 9:   // cerchio con la croce
                    p.Line(ink, w, 0, -0.25f, 0, 0.25f);
                    p.Line(ink, w, -0.25f, 0, 0.25f, 0);
                    p.Fill(Sd.Subtract(Sd.Circle(0, 0, 0.19f), Sd.Circle(0, 0, 0.14f)), ink);
                    break;
                default:  // rombo con la barra
                    p.Line(ink, w, 0, 0.25f, -0.2f, 0, 0, -0.25f, 0.2f, 0, 0, 0.25f);
                    p.Line(ink, w, -0.26f, -0.12f, 0.26f, 0.12f);
                    break;
            }
            return new DoodleArt("bs_sym" + k, p);
        }

        /// <summary>Bandierina "BANG!" su un'asta che salta fuori dalla bomba (asta in 0,0, bandiera in alto).</summary>
        public static DoodleArt BombFlag()
        {
            var p = new DoodlePainter(-0.1f, -0.1f, 1.35f, 1.6f, 120) { Ink = 0.04f };
            p.Shape(Sd.Box(0.03f, 0.7f, 0.035f, 0.72f, 0.02f), Rgba.Hex("B8BCC6"));
            p.Shape(Sd.Polygon(0.07f, 1.45f, 1.25f, 1.38f, 1.2f, 0.72f, 0.07f, 0.78f), Rgba.Hex("FFF6D8"));
            p.Line(Rgba.Hex("E85A4F"), 0.05f, 0.15f, 1.38f, 1.17f, 1.32f);
            p.Line(Rgba.Hex("E85A4F"), 0.05f, 0.15f, 0.85f, 1.14f, 0.8f);
            return new DoodleArt("bs_flag", p);
        }

        // ---------- Potion lab ----------

        /// <summary>Laboratorio della strega (coordinate del livello; pavimento sotto y = -2.1, mensola in basso a y = -4.15).</summary>
        public static DoodleArt PotionBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 2.5f, 5f, 5f), (x, y) => Rgba.Lerp(Rgba.Hex("3A2C55"), Rgba.Hex("4D3B6E"), Clamp((y + 2f) / 8f)));
            // pietre del muro
            for (int row = 0; row < 18; row++)
            {
                float y0 = -2.1f + row * 0.5f;
                float shift = row % 2 == 0 ? 0f : 0.45f;
                for (float x = -4.6f + shift; x < 4.6f; x += 0.9f)
                {
                    float n = Noise.Value(x * 2.7f, y0 * 1.9f);
                    p.Shape(Sd.Box(x + 0.45f, y0 + 0.25f, 0.4f, 0.2f, 0.08f), Rgba.Lerp(Rgba.Hex("54437A"), Rgba.Hex("62508A"), n).WithAlpha(0.9f), 0.025f);
                }
            }
            // finestrella con la luna
            Sdf win = Sd.Union(Sd.Box(1.9f, 0.75f, 0.55f, 0.45f), Sd.Circle(1.9f, 1.2f, 0.55f));
            p.Shape(Sd.Grow(win, 0.1f), Rgba.Hex("7A6A9A"));
            p.FillFunc(win, (x, y) => Rgba.Lerp(Rgba.Hex("10163A"), Rgba.Hex("27306A"), Clamp((y - 0.3f) / 1.4f)));
            p.Shape(Sd.Subtract(Sd.Circle(2.1f, 1.35f, 0.22f), Sd.Circle(2.2f, 1.43f, 0.19f)), Rgba.Hex("FFE680"), 0.03f);
            p.Line(Rgba.Hex("7A6A9A"), 0.06f, 1.9f, 0.3f, 1.9f, 1.75f);
            p.Line(Rgba.Hex("7A6A9A"), 0.06f, 1.35f, 0.95f, 2.45f, 0.95f);
            // ragnatela nell'angolo
            for (int i = 0; i < 4; i++) p.Line(Rgba.White.WithAlpha(0.35f), 0.015f, -2.95f, 3.6f, -2.95f + 0.9f * (float)Math.Cos(-i * 0.5f), 3.6f + 0.9f * (float)Math.Sin(-i * 0.5f));
            foreach (float r in new[] { 0.35f, 0.65f }) p.Line(Rgba.White.WithAlpha(0.3f), 0.012f, -2.95f + r, 3.6f, -2.95f + r * 0.9f, 3.6f - r * 0.45f, -2.95f + r * 0.55f, 3.6f - r * 0.85f, -2.95f, 3.6f - r);
            // pavimento di pietra
            p.Fill(Sd.Box(0, -4.6f, 5f, 2.5f), Rgba.Hex("3B3150"), false);
            for (float y = -2.5f; y > -4f; y -= 0.45f) p.Line(V40C_Ink.WithAlpha(0.3f), 0.025f, -4.5f, y, 4.5f, y - 0.03f);
            p.Shape(Sd.Box(0, -2.1f, 5f, 0.05f), Rgba.Hex("54437A"));
            // mensola degli ingredienti
            Body(p, Sd.Box(0, -4.25f, 2.75f, 0.12f, 0.03f), Rgba.Hex("A56A3A"), Rgba.Hex("7E4C25"));
            foreach (float x in new[] { -2.3f, 2.3f }) p.Shape(Sd.Polygon(x - 0.08f, -4.37f, x + 0.08f, -4.37f, x + 0.03f, -4.75f, x - 0.03f, -4.75f), Rgba.Hex("7E4C25"));
            return new DoodleArt("pt_bg", p);
        }

        /// <summary>Libro degli incantesimi aperto (4.7 x 1.7, centro 0,0; pagine x -2.2..-0.12 e 0.12..2.2).</summary>
        public static DoodleArt PotionBook()
        {
            var p = new DoodlePainter(-2.5f, -1.0f, 2.5f, 0.98f, 90) { Ink = 0.045f };
            p.Shape(Sd.Box(0, -0.05f, 2.38f, 0.88f, 0.1f), Rgba.Hex("7A2E3A"));
            foreach (float s in new[] { -1f, 1f })
            {
                Sdf page = Sd.Polygon(s * 0.05f, 0.75f, s * 1.2f, 0.85f, s * 2.25f, 0.78f, s * 2.25f, -0.8f, s * 1.2f, -0.72f, s * 0.05f, -0.82f);
                p.Shape(Sd.Offset(page, 0, -0.04f), Rgba.Hex("D9C9A0"));
                p.Shape(page, Rgba.Hex("F6EBCB"));
                p.Fill(Sd.Box(s * 0.25f, -0.02f, 0.18f, 0.78f), Rgba.Hex("D9C9A0", 0.5f));
            }
            p.InkLine(0.04f, 0, 0.78f, 0, -0.84f);
            // segnalibro
            p.Shape(Sd.Polygon(1.7f, 0.86f, 1.9f, 0.86f, 1.9f, 0.3f, 1.8f, 0.4f, 1.7f, 0.3f), Rgba.Hex("E2B33C"), 0.03f);
            return new DoodleArt("pt_book", p);
        }

        /// <summary>Calderone nero con le zampe (bocca in 0,0.5, fondo in 0,-1.4).</summary>
        public static DoodleArt PotionCauldron()
        {
            var p = new DoodlePainter(-1.35f, -1.5f, 1.35f, 0.82f, 110) { Ink = 0.055f };
            Rgba iron = Rgba.Hex("3A3946"), ironShade = Rgba.Hex("262530");
            foreach (float s in new[] { -1f, 1f })
                p.Shape(Sd.Polygon(s * 0.6f, -0.9f, s * 0.82f, -0.9f, s * 0.95f, -1.42f, s * 0.75f, -1.42f), ironShade);
            Sdf belly = Sd.Intersect(Sd.Ellipse(0, -0.25f, 1.12f, 0.95f), Sd.Box(0, -0.2f, 1.3f, 0.7f));
            Body(p, belly, iron, ironShade, null, -0.12f, 0.1f);
            p.Fill(Sd.Ellipse(-0.55f, -0.35f, 0.12f, 0.3f), Rgba.White.WithAlpha(0.15f));
            // bordo
            p.Shape(Sd.Subtract(Sd.Ellipse(0, 0.5f, 1.22f, 0.3f), Sd.Ellipse(0, 0.52f, 1.0f, 0.2f)), Rgba.Hex("4D4C5C"));
            p.Fill(Sd.Ellipse(0, 0.52f, 1.0f, 0.2f), Rgba.Hex("15141B"));
            p.Shape(Sd.Ellipse(0, 0.52f, 1.0f, 0.2f), Rgba.Clear, 0.035f);
            // maniglie
            foreach (float s in new[] { -1f, 1f })
                p.Shape(Sd.Subtract(Sd.Circle(s * 1.15f, 0.15f, 0.17f), Sd.Circle(s * 1.15f, 0.15f, 0.09f)), Rgba.Hex("4D4C5C"), 0.035f);
            return new DoodleArt("pt_cauldron", p);
        }

        /// <summary>Superficie della pozione (bianca: si tinge; ellisse centrata in 0,0).</summary>
        public static DoodleArt PotionLiquid()
        {
            var p = new DoodlePainter(-1.02f, -0.22f, 1.02f, 0.22f, 120) { Wobble = 0f };
            p.Fill(Sd.Ellipse(0, 0, 0.98f, 0.18f), Rgba.White);
            p.Fill(Sd.Ellipse(-0.35f, 0.04f, 0.3f, 0.05f), Rgba.White.WithAlpha(0.6f));
            foreach (var b in new[] { (0.3f, -0.03f, 0.05f), (0.55f, 0.04f, 0.035f), (-0.1f, -0.08f, 0.04f) })
                p.Fill(Sd.Subtract(Sd.Circle(b.Item1, b.Item2, b.Item3), Sd.Circle(b.Item1, b.Item2, b.Item3 - 0.012f)), Rgba.Hex("2B2233", 0.35f));
            return new DoodleArt("pt_liquid", p);
        }

        /// <summary>Termometro grande con zona verde (tubo interno da y -0.9 a 1.15; verde al 55-75%, rosso oltre il 92%).</summary>
        public static DoodleArt PotionThermo()
        {
            var p = new DoodlePainter(-0.42f, -1.42f, 0.42f, 1.42f, 120) { Ink = 0.045f };
            p.Shape(Sd.Union(Sd.Box(0, 0.15f, 0.17f, 1.17f, 0.17f), Sd.Circle(0, -1.07f, 0.28f)), Rgba.Hex("F4F6FA"));
            const float bottom = -0.9f, len = 2.05f;
            p.Fill(Sd.Box(0, bottom + len * 0.65f, 0.11f, len * 0.1f), Rgba.Hex("7BD36A"));
            p.Fill(Sd.Box(0, bottom + len * 0.96f, 0.11f, len * 0.04f), Rgba.Hex("F07A6A"));
            p.Fill(Sd.Circle(0, -1.07f, 0.2f), Rgba.Hex("E2483C"));
            p.Fill(Sd.Box(0, -0.95f, 0.06f, 0.08f), Rgba.Hex("E2483C"));
            for (int i = 0; i <= 8; i++)
            {
                float y = bottom + len * i / 8f;
                p.InkLine(0.022f, 0.17f, y, i % 2 == 0 ? 0.3f : 0.25f, y);
            }
            p.Fill(Sd.Box(-0.08f, 0.2f, 0.025f, 0.9f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("pt_thermo", p);
        }

        /// <summary>Mestolo di legno (centro della paletta in 0,0, manico verso l'alto a destra).</summary>
        public static DoodleArt PotionLadle()
        {
            var p = new DoodlePainter(-0.3f, -0.25f, 0.9f, 1.55f, 120) { Ink = 0.04f };
            p.Shape(Sd.Capsule(0.05f, 0.15f, 0.72f, 1.42f, 0.07f), Rgba.Hex("C98B4E"));
            Body(p, Sd.Ellipse(0, 0, 0.24f, 0.17f), Rgba.Hex("D9A060"), Rgba.Hex("A8713A"));
            p.Line(Rgba.Hex("A8713A"), 0.02f, 0.3f, 0.6f, 0.5f, 0.95f);
            return new DoodleArt("pt_ladle", p);
        }

        /// <summary>Boccetta d'acqua (base in 0,-0.4).</summary>
        public static DoodleArt PotionWater()
        {
            var p = new DoodlePainter(-0.36f, -0.45f, 0.36f, 0.45f, 150) { Ink = 0.035f };
            Sdf flask = Sd.Union(Sd.Circle(0, -0.12f, 0.28f), Sd.Box(0, 0.2f, 0.09f, 0.18f, 0.03f));
            p.Shape(flask, Rgba.Hex("E8F6FF"));
            p.Fill(Sd.Intersect(Sd.Circle(0, -0.12f, 0.24f), Sd.Box(0, -0.25f, 0.3f, 0.16f)), Rgba.Hex("5BB4F0"));
            p.Shape(flask, Rgba.Clear);
            p.Shape(Sd.Box(0, 0.38f, 0.1f, 0.05f, 0.02f), Rgba.Hex("B07A48"), 0.03f);
            p.Shape(Sd.Polygon(0, 0.0f, -0.07f, -0.13f, 0, -0.18f, 0.07f, -0.13f), Rgba.White, 0.02f);
            p.Fill(Sd.Ellipse(-0.14f, -0.02f, 0.04f, 0.09f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("pt_water", p);
        }

        /// <summary>Bottiglia del latte (base in 0,-0.4).</summary>
        public static DoodleArt PotionMilk()
        {
            var p = new DoodlePainter(-0.3f, -0.45f, 0.3f, 0.45f, 150) { Ink = 0.035f };
            Sdf bottle = Sd.Union(Sd.Box(0, -0.12f, 0.22f, 0.28f, 0.08f), Sd.Polygon(-0.2f, 0.1f, 0.2f, 0.1f, 0.1f, 0.3f, -0.1f, 0.3f));
            Body(p, bottle, Rgba.White, Rgba.Hex("DCE4EE"));
            p.Shape(Sd.Box(0, 0.34f, 0.11f, 0.06f, 0.03f), Rgba.Hex("3E8EF7"), 0.03f);
            p.Shape(Sd.Box(0, -0.12f, 0.16f, 0.1f, 0.03f), Rgba.Hex("BFE0FF"), 0.025f);
            p.Fill(Sd.Circle(-0.05f, -0.12f, 0.035f), V40C_Ink);
            p.Fill(Sd.Circle(0.06f, -0.1f, 0.025f), V40C_Ink);
            return new DoodleArt("pt_milk", p);
        }

        /// <summary>Uovo (base in 0,-0.3).</summary>
        public static DoodleArt PotionEgg()
        {
            var p = new DoodlePainter(-0.3f, -0.35f, 0.3f, 0.38f, 150) { Ink = 0.035f };
            Body(p, Sd.Union(Sd.Ellipse(0, -0.05f, 0.24f, 0.25f), Sd.Ellipse(0, 0.08f, 0.19f, 0.26f)), Rgba.Hex("FFF4E0"), Rgba.Hex("EAD3AE"));
            p.Fill(Sd.Ellipse(-0.08f, 0.12f, 0.04f, 0.08f), Rgba.White);
            return new DoodleArt("pt_egg", p);
        }

        /// <summary>Barattolo del miele (base in 0,-0.38).</summary>
        public static DoodleArt PotionHoney()
        {
            var p = new DoodlePainter(-0.36f, -0.45f, 0.36f, 0.45f, 150) { Ink = 0.035f };
            Body(p, Sd.Box(0, -0.1f, 0.27f, 0.28f, 0.1f), Rgba.Hex("F2B030"), Rgba.Hex("D18A16"));
            p.Shape(Sd.Box(0, 0.24f, 0.3f, 0.08f, 0.04f), Rgba.Hex("D8443A"), 0.03f);
            p.Line(Rgba.Hex("FFE08A"), 0.03f, -0.15f, 0.12f, -0.15f, -0.05f);
            p.Shape(Sd.Box(0, -0.12f, 0.15f, 0.08f, 0.03f), Rgba.Hex("FFF4D0"), 0.025f);
            p.Fill(Sd.Ellipse(0, -0.12f, 0.05f, 0.035f), Rgba.Hex("F2B030"));
            return new DoodleArt("pt_honey", p);
        }

        /// <summary>Fungo rosso a pallini (base in 0,-0.35).</summary>
        public static DoodleArt PotionMushroom()
        {
            var p = new DoodlePainter(-0.4f, -0.42f, 0.4f, 0.38f, 150) { Ink = 0.035f };
            Body(p, Sd.Box(0, -0.18f, 0.1f, 0.18f, 0.06f), Rgba.Hex("FFF4E0"), Rgba.Hex("EAD3AE"));
            Sdf cap = Sd.Intersect(Sd.Ellipse(0, 0.0f, 0.34f, 0.32f), Sd.Box(0, 0.2f, 0.4f, 0.2f));
            Body(p, cap, Rgba.Hex("E2483C"), Rgba.Hex("B8302A"));
            foreach (var d in new[] { (-0.15f, 0.12f, 0.05f), (0.08f, 0.22f, 0.045f), (0.2f, 0.07f, 0.04f) })
                p.Fill(Sd.Circle(d.Item1, d.Item2, d.Item3), Rgba.White);
            return new DoodleArt("pt_mushroom", p);
        }

        // ---------- Robot factory ----------

        /// <summary>Fabbrica: muro di lamiera, tubi e pavimento (coordinate del livello).</summary>
        public static DoodleArt FactoryBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 5f, 7.5f), (x, y) => Rgba.Lerp(Rgba.Hex("3D5368"), Rgba.Hex("56708A"), Clamp((y + 4f) / 10f)));
            for (float x = -4.2f; x < 4.5f; x += 1.2f) p.Line(V40C_Ink.WithAlpha(0.25f), 0.03f, x, -7f, x, 7f);
            for (float y = -6.5f; y < 7f; y += 1.6f)
                for (float x = -4.2f; x < 4.5f; x += 1.2f)
                    foreach (float dx in new[] { 0.1f, 1.1f }) p.Fill(Sd.Circle(x + dx, y, 0.035f), Rgba.Hex("8FA3B8"));
            // tubi
            p.Shape(Sd.Box(0, 3.75f, 5f, 0.12f), Rgba.Hex("C9A23C"));
            p.Shape(Sd.Box(-2.6f, 1f, 0.12f, 2.9f), Rgba.Hex("C9A23C"));
            p.Shape(Sd.Circle(-2.6f, 3.75f, 0.2f), Rgba.Hex("E2B94A"));
            // strisce di sicurezza
            Sdf hazard = Sd.Box(0, -4.6f, 5f, 0.14f);
            p.FillFunc(hazard, (x, y) => ((int)Math.Floor((x + y) / 0.3f + 100) % 2 == 0) ? Rgba.Hex("F2C230") : Rgba.Hex("2B2233"));
            p.Fill(Sd.Box(0, -6f, 5f, 1.27f), Rgba.Hex("4A4F58"), false);
            return new DoodleArt("rf_bg", p);
        }

        /// <summary>Nastro trasportatore (6.6 x 0.6, centro 0,0; superficie a y = 0.2).</summary>
        public static DoodleArt FactoryBelt()
        {
            var p = new DoodlePainter(-3.4f, -0.75f, 3.4f, 0.4f, 70) { Ink = 0.05f };
            p.Shape(Sd.Box(0, -0.05f, 3.3f, 0.26f, 0.24f), Rgba.Hex("2E3038"));
            for (float x = -3.0f; x <= 3.01f; x += 0.6f)
            {
                p.Shape(Sd.Circle(x, -0.05f, 0.17f), Rgba.Hex("9AA0AC"), 0.03f);
                p.Fill(Sd.Circle(x, -0.05f, 0.05f), V40C_Ink);
            }
            foreach (float x in new[] { -2.4f, 2.4f })
                p.Shape(Sd.Polygon(x - 0.12f, -0.3f, x + 0.12f, -0.3f, x + 0.2f, -0.72f, x - 0.2f, -0.72f), Rgba.Hex("6B7080"));
            return new DoodleArt("rf_belt", p);
        }

        static void V40C_Metal(DoodlePainter p, Sdf s) { Body(p, s, Rgba.Hex("D6DEE8"), Rgba.Hex("A9B5C4")); }

        /// <summary>Busto del robot (centro 0,0; 0.9 x 0.9; prese di testa, braccia e gambe).</summary>
        public static DoodleArt RobotTorso()
        {
            var p = new DoodlePainter(-0.55f, -0.55f, 0.55f, 0.55f, 150) { Ink = 0.04f };
            V40C_Metal(p, Sd.Box(0, 0, 0.45f, 0.45f, 0.1f));
            p.Shape(Sd.Box(0, 0.02f, 0.24f, 0.16f, 0.04f), Rgba.Hex("3C4558"), 0.03f);
            p.Fill(Sd.Circle(-0.1f, 0.04f, 0.04f), Rgba.Hex("6CE07A"));
            p.Fill(Sd.Circle(0.02f, 0.04f, 0.04f), Rgba.Hex("F2C230"));
            p.Fill(Sd.Circle(0.14f, 0.04f, 0.04f), Rgba.Hex("E85A4F"));
            foreach (float x in new[] { -0.3f, 0.3f }) p.Fill(Sd.Circle(x, -0.3f, 0.035f), Rgba.Hex("7C8899"));
            return new DoodleArt("rf_torso", p);
        }

        /// <summary>Testa del robot (attacco al collo in 0,0, testa sopra).</summary>
        public static DoodleArt RobotHead()
        {
            var p = new DoodlePainter(-0.42f, -0.06f, 0.42f, 0.8f, 150) { Ink = 0.04f };
            p.Shape(Sd.Box(0, 0.05f, 0.1f, 0.08f), Rgba.Hex("7C8899"));
            V40C_Metal(p, Sd.Box(0, 0.38f, 0.34f, 0.26f, 0.1f));
            p.InkLine(0.03f, 0, 0.64f, 0, 0.72f);
            p.Shape(Sd.Circle(0, 0.74f, 0.045f), Rgba.Hex("E85A4F"), 0.025f);
            foreach (float x in new[] { -0.13f, 0.13f })
            {
                p.Shape(Sd.Circle(x, 0.42f, 0.08f), Rgba.Hex("BFF3FF"), 0.03f);
                p.Fill(Sd.Circle(x, 0.42f, 0.035f), V40C_Ink);
            }
            p.InkLine(0.025f, -0.1f, 0.24f, 0.1f, 0.24f);
            return new DoodleArt("rf_head", p);
        }

        /// <summary>Braccio sinistro del robot (spalla in 0,0, il braccio va a sinistra e in giù).</summary>
        public static DoodleArt RobotArm()
        {
            var p = new DoodlePainter(-0.5f, -0.62f, 0.08f, 0.12f, 150) { Ink = 0.04f };
            p.Shape(Sd.Capsule(-0.02f, 0f, -0.3f, -0.35f, 0.07f), Rgba.Hex("A9B5C4"));
            p.Shape(Sd.Circle(-0.32f, -0.45f, 0.12f), Rgba.Hex("D6DEE8"));
            p.Fill(Sd.Circle(-0.32f, -0.45f, 0.05f), Rgba.Hex("7C8899"));
            return new DoodleArt("rf_arm", p);
        }

        /// <summary>Gambe del robot (anca in 0,0, piedi in basso a y = -0.55).</summary>
        public static DoodleArt RobotLegs()
        {
            var p = new DoodlePainter(-0.42f, -0.65f, 0.42f, 0.06f, 150) { Ink = 0.04f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Box(s * 0.2f, -0.22f, 0.07f, 0.22f, 0.03f), Rgba.Hex("A9B5C4"));
                V40C_Metal(p, Sd.Box(s * 0.24f, -0.53f, 0.15f, 0.08f, 0.05f));
            }
            return new DoodleArt("rf_legs", p);
        }

        /// <summary>Foglio del progetto (blu con la griglia, 2.4 x 1.5, centro 0,0).</summary>
        public static DoodleArt Blueprint()
        {
            var p = new DoodlePainter(-1.3f, -0.85f, 1.3f, 0.85f, 90) { Ink = 0.045f };
            Sdf sheet = Sd.Box(0, 0, 1.2f, 0.75f, 0.04f);
            p.Shape(Sd.Offset(sheet, 0.04f, -0.04f), Rgba.Hex("1D3E78"));
            p.Shape(sheet, Rgba.Hex("2F64B8"));
            for (float x = -1.1f; x < 1.2f; x += 0.2f) p.Line(Rgba.White.WithAlpha(0.18f), 0.01f, x, -0.7f, x, 0.7f);
            for (float y = -0.65f; y < 0.75f; y += 0.2f) p.Line(Rgba.White.WithAlpha(0.18f), 0.01f, -1.15f, y, 1.15f, y);
            p.Line(Rgba.White.WithAlpha(0.6f), 0.02f, -1.1f, -0.65f, 1.1f, -0.65f, 1.1f, 0.65f, -1.1f, 0.65f, -1.1f, -0.65f);
            foreach (float x in new[] { -1.05f, 1.05f }) p.Shape(Sd.Circle(x, 0.62f, 0.05f), Rgba.Hex("E85A4F"), 0.02f);
            return new DoodleArt("rf_blueprint", p);
        }

        /// <summary>Cassetta dei pezzi (1.3 x 0.7, bordo superiore a y = 0.2).</summary>
        public static DoodleArt PartsBin()
        {
            var p = new DoodlePainter(-0.7f, -0.42f, 0.7f, 0.3f, 120) { Ink = 0.045f };
            Body(p, Sd.Polygon(-0.62f, 0.2f, 0.62f, 0.2f, 0.52f, -0.36f, -0.52f, -0.36f), Rgba.Hex("F2C230"), Rgba.Hex("D19A18"));
            p.Shape(Sd.Box(0, -0.1f, 0.25f, 0.06f, 0.03f), Rgba.Hex("3C4558"), 0.025f);
            return new DoodleArt("rf_bin", p);
        }

        /// <summary>Piastrella del pavimento della griglia (0.72 x 0.72).</summary>
        public static DoodleArt FloorTile()
        {
            var p = new DoodlePainter(-0.38f, -0.38f, 0.38f, 0.38f, 120) { Ink = 0.025f, Wobble = 0.004f };
            p.Shape(Sd.Box(0, 0, 0.34f, 0.34f, 0.04f), Rgba.Hex("C9D3DE"));
            p.Fill(Sd.Box(-0.02f, 0.02f, 0.28f, 0.28f, 0.03f), Rgba.Hex("DCE4EC"));
            foreach (var c in new[] { (-0.26f, 0.26f), (0.26f, 0.26f), (-0.26f, -0.26f), (0.26f, -0.26f) })
                p.Fill(Sd.Circle(c.Item1, c.Item2, 0.025f), Rgba.Hex("8FA3B8"));
            return new DoodleArt("rf_tile", p);
        }

        /// <summary>Cassa di legno (ostacolo della griglia, 0.64 x 0.64).</summary>
        public static DoodleArt FactoryCrate()
        {
            var p = new DoodlePainter(-0.36f, -0.36f, 0.36f, 0.36f, 120) { Ink = 0.035f };
            Body(p, Sd.Box(0, 0, 0.3f, 0.3f, 0.04f), Rgba.Hex("C98B4E"), Rgba.Hex("9C6532"));
            p.Shape(Sd.Subtract(Sd.Box(0, 0, 0.3f, 0.3f, 0.04f), Sd.Box(0, 0, 0.22f, 0.22f)), Rgba.Hex("B07A42"), 0.025f);
            p.InkLine(0.03f, -0.22f, -0.22f, 0.22f, 0.22f);
            return new DoodleArt("rf_crate", p);
        }

        /// <summary>Robottino visto dall'alto per la griglia (centro 0,0, il muso guarda in su).</summary>
        public static DoodleArt MiniBot()
        {
            var p = new DoodlePainter(-0.36f, -0.34f, 0.36f, 0.4f, 150) { Ink = 0.035f };
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Box(s * 0.26f, -0.02f, 0.07f, 0.26f, 0.04f), Rgba.Hex("3C4558"));
            V40C_Metal(p, Sd.Box(0, 0, 0.22f, 0.26f, 0.09f));
            p.Shape(Sd.Polygon(0, 0.36f, -0.12f, 0.22f, 0.12f, 0.22f), Rgba.Hex("F2C230"), 0.025f);
            foreach (float x in new[] { -0.09f, 0.09f }) { p.Shape(Sd.Circle(x, 0.1f, 0.055f), Rgba.Hex("BFF3FF"), 0.02f); p.Fill(Sd.Circle(x, 0.11f, 0.025f), V40C_Ink); }
            p.Fill(Sd.Circle(0, -0.12f, 0.04f), Rgba.Hex("E85A4F"));
            return new DoodleArt("rf_minibot", p);
        }

        /// <summary>Pila verde con il fulmine (0.5 x 0.6, centro 0,0).</summary>
        public static DoodleArt ChargeCell()
        {
            var p = new DoodlePainter(-0.3f, -0.36f, 0.3f, 0.4f, 150) { Ink = 0.035f };
            p.Shape(Sd.Box(0, 0.3f, 0.08f, 0.05f, 0.02f), Rgba.Hex("B8BCC6"));
            Body(p, Sd.Box(0, -0.02f, 0.21f, 0.29f, 0.06f), Rgba.Hex("6CE07A"), Rgba.Hex("3FAE52"));
            p.Shape(Sd.Polygon(0.04f, 0.18f, -0.1f, -0.03f, -0.01f, -0.03f, -0.05f, -0.22f, 0.11f, 0.02f, 0.02f, 0.02f), Rgba.Hex("FFE14A"), 0.025f);
            return new DoodleArt("rf_cell", p);
        }

        /// <summary>Carta comando: 0 avanti, 1 gira a sinistra, 2 gira a destra (0.86 x 0.86, centro 0,0).</summary>
        public static DoodleArt CommandCard(int kind)
        {
            var p = new DoodlePainter(-0.47f, -0.47f, 0.47f, 0.47f, 140) { Ink = 0.04f };
            Rgba[] fills = { Rgba.Hex("7BD36A"), Rgba.Hex("6FB6F2"), Rgba.Hex("F2A65A") };
            Body(p, Sd.Box(0, 0, 0.4f, 0.4f, 0.08f), fills[kind], Rgba.Lerp(fills[kind], V40C_Ink, 0.25f));
            Rgba w = Rgba.White;
            if (kind == 0)
                p.Shape(Sd.Polygon(0, 0.28f, -0.2f, 0.04f, -0.07f, 0.04f, -0.07f, -0.26f, 0.07f, -0.26f, 0.07f, 0.04f, 0.2f, 0.04f), w, 0.03f);
            else
            {
                float s = kind == 1 ? -1f : 1f;
                // curva: sale e piega verso il lato della svolta
                var curve = new List<float>();
                for (int i = 0; i <= 12; i++)
                {
                    double a = Math.PI / 2 * i / 12;
                    curve.Add(s * (0.2f - (float)Math.Cos(a) * 0.3f));
                    curve.Add(-0.25f + (float)Math.Sin(a) * 0.38f);
                }
                p.Shape(Sd.Polyline(0.065f, curve.ToArray()), w, 0.03f);
                p.Shape(Sd.Polygon(s * 0.32f, 0.13f, s * 0.14f, 0.27f, s * 0.14f, -0.01f), w, 0.03f);
            }
            return new DoodleArt("rf_card" + kind, p);
        }

        /// <summary>Casella del programma (tratteggiata, 0.86 x 0.86).</summary>
        public static DoodleArt CommandSlot()
        {
            var p = new DoodlePainter(-0.46f, -0.46f, 0.46f, 0.46f, 120) { Ink = 0.03f, Wobble = 0.004f };
            p.Fill(Sd.Box(0, 0, 0.4f, 0.4f, 0.08f), Rgba.Hex("2B2233", 0.35f));
            for (int i = 0; i < 4; i++)
            {
                float a = -0.32f + i * 0.22f;
                p.Line(Rgba.White.WithAlpha(0.7f), 0.03f, a, 0.4f, a + 0.12f, 0.4f);
                p.Line(Rgba.White.WithAlpha(0.7f), 0.03f, a, -0.4f, a + 0.12f, -0.4f);
                p.Line(Rgba.White.WithAlpha(0.7f), 0.03f, 0.4f, a, 0.4f, a + 0.12f);
                p.Line(Rgba.White.WithAlpha(0.7f), 0.03f, -0.4f, a, -0.4f, a + 0.12f);
            }
            return new DoodleArt("rf_slot", p);
        }

        /// <summary>Pulsante RUN rotondo verde (raggio 0.5).</summary>
        public static DoodleArt RunButton()
        {
            var p = new DoodlePainter(-0.6f, -0.62f, 0.6f, 0.6f, 120) { Ink = 0.045f };
            p.Shape(Sd.Circle(0, -0.05f, 0.54f), Rgba.Hex("2E8A3E"));
            Body(p, Sd.Circle(0, 0.01f, 0.5f), Rgba.Hex("6CE07A"), Rgba.Hex("44B858"));
            p.Fill(Sd.Ellipse(-0.18f, 0.24f, 0.13f, 0.06f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("rf_run", p);
        }

        // ---------- Train switches ----------

        /// <summary>Prato visto dall'alto con alberi e laghetto (coordinate del livello).</summary>
        public static DoodleArt TrainBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 5f, 7.5f), (x, y) => Rgba.Lerp(Rgba.Hex("8FCF6A"), Rgba.Hex("A6DB7E"), Noise.Value(x * 0.6f, y * 0.6f)));
            for (int i = 0; i < 120; i++)
            {
                float x = -4.4f + Noise.Value(i * 3.1f, 1.7f) * 8.8f, y = -6.8f + Noise.Value(i * 1.3f, 9.1f) * 13.6f;
                p.Line(Rgba.Hex("6DB050", 0.7f), 0.025f, x, y, x + 0.04f, y + 0.1f);
            }
            // laghetto
            p.Shape(Sd.Ellipse(-1.75f, 1.35f, 0.62f, 0.38f), Rgba.Hex("6FC3EE"));
            p.Fill(Sd.Ellipse(-1.85f, 1.42f, 0.25f, 0.07f), Rgba.White.WithAlpha(0.5f));
            // alberi
            foreach (var t in new[] { (2.1f, 1.7f, 0.42f), (2.45f, 0.9f, 0.3f), (-2.45f, -1.2f, 0.32f), (0.0f, -0.75f, 0.33f), (2.5f, -1.4f, 0.28f), (-0.95f, 3.0f, 0.3f), (1.2f, 3.1f, 0.33f) })
            {
                p.Fill(Sd.Circle(t.Item1 + 0.06f, t.Item2 - 0.06f, t.Item3), Rgba.Hex("2B2233", 0.18f));
                Body(p, Sd.Circle(t.Item1, t.Item2, t.Item3), Rgba.Hex("4FA84A"), Rgba.Hex("3B8A3A"));
                p.Fill(Sd.Circle(t.Item1 - t.Item3 * 0.3f, t.Item2 + t.Item3 * 0.3f, t.Item3 * 0.25f), Rgba.Hex("7BC86A"));
            }
            // fiorellini
            for (int i = 0; i < 26; i++)
            {
                float x = -2.7f + Noise.Value(i * 5.3f, 3.3f) * 5.4f, y = -4.6f + Noise.Value(i * 2.9f, 7.7f) * 8.6f;
                p.Fill(Sd.Circle(x, y, 0.04f), i % 2 == 0 ? Rgba.White : Rgba.Hex("FFD84A"));
            }
            return new DoodleArt("tr_bg", p);
        }

        /// <summary>Locomotiva vista dall'alto (bianca: si tinge; muso verso +x, 0.86 x 0.46).</summary>
        public static DoodleArt TrainLoco()
        {
            var p = new DoodlePainter(-0.5f, -0.3f, 0.52f, 0.3f, 160) { Ink = 0.035f };
            Body(p, Sd.Box(0, 0, 0.42f, 0.2f, 0.08f), Rgba.White, Rgba.Hex("D8D8D8"));
            p.Shape(Sd.Box(-0.24f, 0, 0.14f, 0.17f, 0.04f), Rgba.Hex("3C4558"), 0.03f);
            p.Fill(Sd.Box(-0.24f, 0, 0.09f, 0.12f, 0.03f), Rgba.Hex("BFF3FF"));
            p.Shape(Sd.Circle(0.22f, 0, 0.07f), Rgba.Hex("3C4558"), 0.03f);
            p.Shape(Sd.Polygon(0.42f, -0.17f, 0.5f, -0.1f, 0.5f, 0.1f, 0.42f, 0.17f), Rgba.Hex("9AA0AC"), 0.025f);
            return new DoodleArt("tr_loco", p);
        }

        /// <summary>Stazione bianca con ombre grigie (si tinge col colore della linea; centro 0,0).</summary>
        public static DoodleArt TrainStation()
        {
            var p = new DoodlePainter(-0.68f, -0.5f, 0.68f, 0.55f, 120) { Ink = 0.04f };
            p.Shape(Sd.Box(0, -0.38f, 0.62f, 0.08f, 0.02f), Rgba.Hex("D8D8D8"));
            Body(p, Sd.Box(0, -0.08f, 0.48f, 0.24f, 0.03f), Rgba.White, Rgba.Hex("E2E2E2"));
            p.Shape(Sd.Box(0, -0.12f, 0.1f, 0.18f, 0.03f), Rgba.Hex("8C5A30"), 0.03f);
            foreach (float x in new[] { -0.3f, 0.3f }) p.Shape(Sd.Box(x, -0.04f, 0.09f, 0.08f, 0.02f), Rgba.Hex("BFF3FF"), 0.025f);
            Body(p, Sd.Polygon(-0.62f, 0.14f, 0.62f, 0.14f, 0.38f, 0.48f, -0.38f, 0.48f), Rgba.White, Rgba.Hex("CFCFCF"));
            return new DoodleArt("tr_station", p);
        }

        /// <summary>Base dello scambio (bottone rotondo grigio, raggio 0.34).</summary>
        public static DoodleArt TrainSwitch()
        {
            var p = new DoodlePainter(-0.4f, -0.42f, 0.4f, 0.4f, 150) { Ink = 0.04f };
            p.Shape(Sd.Circle(0, -0.04f, 0.35f), Rgba.Hex("6B7080"));
            Body(p, Sd.Circle(0, 0, 0.33f), Rgba.Hex("F4F1E6"), Rgba.Hex("D6D0BE"));
            return new DoodleArt("tr_switch", p);
        }

        /// <summary>Freccia dello scambio (punta verso +x, base in 0,0).</summary>
        public static DoodleArt TrainArrow()
        {
            var p = new DoodlePainter(-0.12f, -0.18f, 0.32f, 0.18f, 200) { Ink = 0.03f, Wobble = 0.004f };
            p.Shape(Sd.Polygon(-0.08f, -0.05f, 0.12f, -0.05f, 0.12f, -0.14f, 0.28f, 0f, 0.12f, 0.14f, 0.12f, 0.05f, -0.08f, 0.05f), Rgba.Hex("E85A4F"));
            return new DoodleArt("tr_arrow", p);
        }

        /// <summary>Bocca della galleria del deposito (binario che esce verso il basso; centro 0,0).</summary>
        public static DoodleArt TrainTunnel()
        {
            var p = new DoodlePainter(-0.9f, -0.45f, 0.9f, 0.6f, 120) { Ink = 0.045f };
            Body(p, Sd.Ellipse(0, 0.0f, 0.82f, 0.48f), Rgba.Hex("9A9184"), Rgba.Hex("7A7266"));
            for (int i = 0; i < 7; i++)
            {
                double a = Math.PI * (0.1 + 0.8 * i / 6.0);
                p.Shape(Sd.Circle((float)Math.Cos(a) * 0.6f, (float)Math.Sin(a) * 0.32f - 0.02f, 0.11f), Rgba.Hex("B5AC9E"), 0.025f);
            }
            p.Shape(Sd.Intersect(Sd.Ellipse(0, -0.1f, 0.34f, 0.4f), Sd.Box(0, 0, 0.4f, 0.42f)), Rgba.Hex("1B1820"));
            return new DoodleArt("tr_tunnel", p);
        }
    }
}
