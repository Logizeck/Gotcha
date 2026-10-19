using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli nuovi della versione 0.39 (gruppo C): magazzino, serpente, flipper, air hockey, basket.</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV39C() => new DoodleArt[]
        {
            SokFloor(), SokWall(), SokCrate(), SokGlow(), SokMark(), SokPawShadow(), SokButton(),
            SnakeBoard(), SnakeHead(), SnakeTongue(), SnakeBody(), SnakeTail(),
            PinBackdrop(), PinFlipper(), PinBumper(false), PinBumper(true), PinCat(false), PinCat(true), PinPlunger(),
            HockeyTable(), HockeyMallet(), HockeyPawMallet(), HockeyPuck(), HockeyBigPaw(),
            DunkGym(), DunkBall(), DunkHoopBack(), DunkHoopFront(),
        };

        static readonly Rgba Ink20 = Rgba.Hex("2B2133");

        // ---------- Magazzino (Sokoban) ----------

        /// <summary>Pavimento del magazzino 7×8 caselle da 0.72 (centro 0,0): assi di legno a scacchi chiari e scuri.</summary>
        public static DoodleArt SokFloor()
        {
            const float c = 0.72f, hx = 3.5f * c, hy = 4f * c;
            var p = new DoodlePainter(-hx - 0.1f, -hy - 0.1f, hx + 0.1f, hy + 0.1f, 60) { Wobble = 0.004f };
            p.FillFunc(Sd.Box(0, 0, hx, hy), (x, y) =>
            {
                int i = (int)Math.Floor((x + hx) / c), j = (int)Math.Floor((y + hy) / c);
                bool dark = (i + j) % 2 == 0;
                float grain = (float)Math.Sin(y * 40f + i * 3f) * 0.03f;
                Rgba a = dark ? Rgba.Hex("D9B98C") : Rgba.Hex("E6C99C");
                return Rgba.Lerp(a, Rgba.Hex("B8915E"), 0.15f + grain);
            });
            for (int i = 0; i <= 7; i++) p.Line(Rgba.Hex("B8915E"), 0.02f, -hx + i * c, -hy, -hx + i * c, hy);
            for (int j = 0; j <= 8; j++) p.Line(Rgba.Hex("B8915E"), 0.02f, -hx, -hy + j * c, hx, -hy + j * c);
            p.Shape(Sd.Box(0, 0, hx, hy, 0.06f), Rgba.Clear, 0.07f);
            return new DoodleArt("sok_floor", p);
        }

        /// <summary>Blocco di muro del magazzino (casella 0.72, centro 0,0): mattoni rossi.</summary>
        public static DoodleArt SokWall()
        {
            var p = new DoodlePainter(-0.38f, -0.38f, 0.38f, 0.38f, 120) { Ink = 0.03f, Wobble = 0.004f };
            Sdf b = Sd.Box(0, 0, 0.35f, 0.35f, 0.05f);
            Body(p, b, Rgba.Hex("C96A4E"), Rgba.Hex("A04E38"));
            Rgba mortar = Rgba.Hex("7E3A2A");
            for (int r = 0; r < 3; r++)
            {
                float y0 = -0.35f + r * 0.233f, y1 = y0 + 0.233f;
                if (r > 0) p.Line(mortar, 0.02f, -0.33f, y0, 0.33f, y0);
                float off = r % 2 == 0 ? 0f : 0.17f;
                for (float x = -0.17f + off; x < 0.34f; x += 0.34f)
                    if (x > -0.33f) p.Line(mortar, 0.02f, x, y0 + 0.02f, x, y1 - 0.02f);
            }
            p.Shape(b, Rgba.Clear);
            p.Fill(Sd.Capsule(-0.26f, 0.27f, -0.08f, 0.27f, 0.025f), Rgba.White.WithAlpha(0.35f));
            return new DoodleArt("sok_wall", p);
        }

        /// <summary>Cassa di legno piena di formaggio (centro 0,0, lato 0.62).</summary>
        public static DoodleArt SokCrate()
        {
            var p = new DoodlePainter(-0.36f, -0.36f, 0.36f, 0.42f, 140) { Ink = 0.03f };
            Rgba cheese = Rgba.Hex("FFD24A"), hole = Rgba.Hex("E0A82A");
            // bordo posteriore della cassa aperta
            p.Shape(Sd.Box(0, 0.08f, 0.3f, 0.05f, 0.02f), Rgba.Hex("8A5530"), 0.025f);
            // mucchio di formaggio che spunta
            p.Shape(Sd.Polygon(-0.27f, 0.06f, 0.02f, 0.06f, -0.2f, 0.36f), cheese, 0.028f);
            p.Fill(Sd.Circle(-0.16f, 0.17f, 0.03f), hole);
            p.Shape(Sd.Polygon(-0.04f, 0.06f, 0.28f, 0.06f, 0.24f, 0.3f), Rgba.Hex("FFC83A"), 0.028f);
            p.Fill(Sd.Circle(0.15f, 0.13f, 0.028f), hole);
            p.Fill(Sd.Circle(0.22f, 0.21f, 0.018f), hole);
            p.Shape(Sd.Circle(0.0f, 0.12f, 0.1f), cheese, 0.028f);
            p.Fill(Sd.Circle(0.02f, 0.14f, 0.025f), hole);
            // fronte della cassa
            Sdf box = Sd.Box(0, -0.12f, 0.31f, 0.2f, 0.04f);
            Body(p, box, Rgba.Hex("C98B4E"), Rgba.Hex("9C6438"));
            Rgba plank = Rgba.Hex("8A5530");
            p.Line(plank, 0.02f, -0.29f, -0.12f, 0.29f, -0.12f);
            p.Line(plank, 0.04f, -0.22f, -0.27f, 0.22f, 0.03f);
            p.Shape(box, Rgba.Clear);
            foreach (float x in new[] { -0.24f, 0.24f }) foreach (float y in new[] { -0.02f, -0.24f }) p.Fill(Sd.Circle(x, y, 0.018f), Ink20);
            return new DoodleArt("sok_crate", p);
        }

        /// <summary>Alone dorato della cassa arrivata sul segno (centro 0,0).</summary>
        public static DoodleArt SokGlow()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.5f, 0.5f, 80) { Wobble = 0f };
            p.FillFunc(Sd.Box(0, 0, 0.5f, 0.5f), (x, y) =>
            {
                float d = Math.Max(Math.Abs(x), Math.Abs(y));
                float a = d < 0.3f ? 0.9f : Math.Max(0f, 1f - (d - 0.3f) / 0.2f) * 0.9f;
                return Rgba.Hex("FFE96B").WithAlpha(a);
            });
            return new DoodleArt("sok_glow", p);
        }

        /// <summary>Segno di arrivo sul pavimento: cerchio tratteggiato con la sagoma del formaggio (centro 0,0).</summary>
        public static DoodleArt SokMark()
        {
            var p = new DoodlePainter(-0.34f, -0.34f, 0.34f, 0.34f, 140) { Wobble = 0.004f };
            Rgba red = Rgba.Hex("E5484D");
            for (int i = 0; i < 12; i++)
            {
                double a0 = i * Math.PI / 6, a1 = a0 + Math.PI / 10;
                p.Line(red, 0.045f, (float)Math.Cos(a0) * 0.27f, (float)Math.Sin(a0) * 0.27f, (float)Math.Cos(a1) * 0.27f, (float)Math.Sin(a1) * 0.27f);
            }
            p.Line(red, 0.05f, -0.12f, -0.12f, 0.12f, 0.12f);
            p.Line(red, 0.05f, -0.12f, 0.12f, 0.12f, -0.12f);
            return new DoodleArt("sok_mark", p);
        }

        /// <summary>Ombra della zampa in arrivo (macchia scura, centro 0,0).</summary>
        public static DoodleArt SokPawShadow()
        {
            var p = new DoodlePainter(-0.4f, -0.4f, 0.4f, 0.4f, 80) { Wobble = 0f };
            p.FillFunc(Sd.Circle(0, 0, 0.4f), (x, y) =>
            {
                float d = (float)Math.Sqrt(x * x + y * y);
                return Rgba.Hex("2B2133").WithAlpha(d < 0.25f ? 0.55f : Math.Max(0f, 0.55f * (0.4f - d) / 0.15f));
            });
            return new DoodleArt("sok_paw_shadow", p);
        }

        /// <summary>Pulsante a pillola (1.5×0.75, centro 0,0) per ANNULLA/RICOMINCIA.</summary>
        public static DoodleArt SokButton()
        {
            var p = new DoodlePainter(-0.8f, -0.42f, 0.8f, 0.42f, 120) { Ink = 0.04f };
            Sdf b = Sd.Box(0, 0, 0.74f, 0.36f, 0.3f);
            p.Fill(Sd.Offset(b, 0, -0.05f), Rgba.Hex("2B2133").WithAlpha(0.35f));
            Body(p, b, Rgba.Hex("FFF4D6"), Rgba.Hex("EBD7A8"));
            return new DoodleArt("sok_button", p);
        }

        // ---------- Serpente ----------

        /// <summary>Prato a scacchi 9×12 caselle da 0.6 con cornice di legno (centro 0,0).</summary>
        public static DoodleArt SnakeBoard()
        {
            const float c = 0.6f, hx = 4.5f * c, hy = 6f * c;
            var p = new DoodlePainter(-hx - 0.16f, -hy - 0.16f, hx + 0.16f, hy + 0.16f, 50) { Ink = 0.06f, Wobble = 0.004f };
            p.Shape(Sd.Box(0, 0, hx + 0.1f, hy + 0.1f, 0.12f), Rgba.Hex("A8743F"));
            p.FillFunc(Sd.Box(0, 0, hx, hy), (x, y) =>
            {
                int i = (int)Math.Floor((x + hx) / c), j = (int)Math.Floor((y + hy) / c);
                return (i + j) % 2 == 0 ? Rgba.Hex("A8DB6A") : Rgba.Hex("96CF5B");
            });
            var rnd = new Random(5);
            for (int k = 0; k < 40; k++)
            {
                float x = (float)(rnd.NextDouble() * 2 - 1) * (hx - 0.1f), y = (float)(rnd.NextDouble() * 2 - 1) * (hy - 0.1f);
                p.Line(Rgba.Hex("7DBA48"), 0.018f, x, y, x - 0.03f, y + 0.08f);
                p.Line(Rgba.Hex("7DBA48"), 0.018f, x + 0.04f, y, x + 0.06f, y + 0.07f);
            }
            p.Shape(Sd.Box(0, 0, hx, hy), Rgba.Clear, 0.04f);
            return new DoodleArt("snake_board", p);
        }

        static readonly Rgba SnakeGreen20 = Rgba.Hex("5BC46A"), SnakeShade20 = Rgba.Hex("3E9A50");

        /// <summary>Testa del serpente che guarda verso +x (centro 0,0, larga circa 0.62): occhioni e guance.</summary>
        public static DoodleArt SnakeHead()
        {
            var p = new DoodlePainter(-0.44f, -0.36f, 0.4f, 0.36f, 160) { Ink = 0.035f };
            Sdf head = Sd.Union(Sd.Ellipse(0.02f, 0, 0.33f, 0.29f), Sd.Circle(-0.15f, 0, 0.25f));
            Body(p, head, SnakeGreen20, SnakeShade20);
            p.Fill(Sd.Ellipse(0.0f, -0.05f, 0.2f, 0.12f), Rgba.Hex("C9F0B0").WithAlpha(0.7f));
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Circle(0.06f, s * 0.14f, 0.1f), Rgba.White, 0.025f);
                p.Fill(Sd.Circle(0.1f, s * 0.13f, 0.05f), Ink20);
                p.Fill(Sd.Circle(0.115f, s * 0.13f + 0.02f, 0.017f), Rgba.White);
            }
            p.Fill(Sd.Circle(0.28f, 0.05f, 0.018f), Ink20);
            p.Fill(Sd.Circle(0.28f, -0.05f, 0.018f), Ink20);
            p.Fill(Sd.Ellipse(-0.06f, 0.2f, 0.05f, 0.03f), Rgba.Hex("FF9AA8").WithAlpha(0.7f));
            p.Fill(Sd.Ellipse(-0.06f, -0.2f, 0.05f, 0.03f), Rgba.Hex("FF9AA8").WithAlpha(0.7f));
            return new DoodleArt("snake_head", p);
        }

        /// <summary>Lingua biforcuta (parte da 0,0 verso +x).</summary>
        public static DoodleArt SnakeTongue()
        {
            var p = new DoodlePainter(-0.02f, -0.1f, 0.3f, 0.1f, 200) { Ink = 0.02f };
            Rgba red = Rgba.Hex("E8384F");
            p.Line(Ink20, 0.05f, 0.0f, 0f, 0.18f, 0f, 0.26f, 0.06f);
            p.Line(Ink20, 0.05f, 0.18f, 0f, 0.26f, -0.06f);
            p.Line(red, 0.025f, 0.0f, 0f, 0.18f, 0f, 0.26f, 0.06f);
            p.Line(red, 0.025f, 0.18f, 0f, 0.26f, -0.06f);
            return new DoodleArt("snake_tongue", p);
        }

        /// <summary>Segmento del corpo (tondo, centro 0,0, diametro 0.56) con una macchia gialla.</summary>
        public static DoodleArt SnakeBody()
        {
            var p = new DoodlePainter(-0.31f, -0.31f, 0.31f, 0.31f, 160) { Ink = 0.035f };
            Sdf b = Sd.Circle(0, 0, 0.27f);
            Body(p, b, SnakeGreen20, SnakeShade20);
            p.Fill(Sd.Circle(0.0f, 0.02f, 0.09f), Rgba.Hex("F5D85A"));
            p.Shape(b, Rgba.Clear);
            return new DoodleArt("snake_body", p);
        }

        /// <summary>Coda: punta rivolta verso +x (centro 0,0).</summary>
        public static DoodleArt SnakeTail()
        {
            var p = new DoodlePainter(-0.3f, -0.28f, 0.36f, 0.28f, 160) { Ink = 0.035f };
            Sdf t = Sd.Union(Sd.Circle(-0.06f, 0, 0.22f), Sd.Polygon(-0.06f, 0.22f, 0.32f, 0.0f, -0.06f, -0.22f));
            Body(p, t, SnakeGreen20, SnakeShade20);
            return new DoodleArt("snake_tail", p);
        }

        // ---------- Flipper ----------

        /// <summary>Sfondo del flipper (6×10, centro 0,0): blu notte con stelline e un tappeto viola nel piano di gioco.</summary>
        public static DoodleArt PinBackdrop()
        {
            var p = new DoodlePainter(-3f, -5f, 3f, 5f, 40) { Wobble = 0f };
            p.FillFunc(Sd.Box(0, 0, 3f, 5f), (x, y) => Rgba.Lerp(Rgba.Hex("2A1F5C"), Rgba.Hex("12183A"), (y + 5f) / 10f));
            p.FillFunc(Sd.Box(0.06f, -0.55f, 2.62f, 4.05f, 1.0f), (x, y) =>
            {
                float stripe = (float)Math.Sin((x + y) * 6f) * 0.5f + 0.5f;
                return Rgba.Lerp(Rgba.Hex("4B2E8C"), Rgba.Hex("5A389E"), stripe * 0.6f);
            });
            var rnd = new Random(21);
            for (int i = 0; i < 40; i++)
            {
                float x = (float)(rnd.NextDouble() * 5.6 - 2.8), y = (float)(rnd.NextDouble() * 9.6 - 4.8), r = 0.015f + (float)rnd.NextDouble() * 0.025f;
                p.Fill(Sd.Circle(x, y, r), Rgba.Hex("FFF3B0").WithAlpha(0.6f));
            }
            // frecce luminose verso il bersaglio
            foreach (float x in new[] { -1.0f, 0.6f })
                for (int k = 0; k < 3; k++)
                {
                    float y = -1.8f + k * 0.3f;
                    p.Line(Rgba.Hex("FF7AC8").WithAlpha(0.55f), 0.05f, x - 0.15f, y, x, y + 0.15f, x + 0.15f, y);
                }
            return new DoodleArt("pin_bg", p);
        }

        /// <summary>Paletta del flipper: perno in 0,0, punta verso +x (lunga 0.86), simmetrica sopra e sotto.</summary>
        public static DoodleArt PinFlipper()
        {
            var p = new DoodlePainter(-0.2f, -0.2f, 1.0f, 0.2f, 160) { Ink = 0.035f };
            Sdf f = Sd.Union(Sd.Circle(0, 0, 0.13f), Sd.Circle(0.84f, 0, 0.07f), Sd.Polygon(0f, 0.13f, 0.84f, 0.07f, 0.84f, -0.07f, 0f, -0.13f));
            p.Fill(f, Rgba.Hex("FF5A6E"));
            p.Fill(Sd.Capsule(0.05f, 0.04f, 0.72f, 0.03f, 0.025f), Rgba.White.WithAlpha(0.5f));
            p.Shape(f, Rgba.Clear);
            p.Shape(Sd.Circle(0, 0, 0.05f), Rgba.Hex("FFD24A"), 0.025f);
            return new DoodleArt("pin_flipper", p);
        }

        /// <summary>Respingente tondo (raggio 0.36, centro 0,0): spento azzurro, acceso giallo con stella.</summary>
        public static DoodleArt PinBumper(bool lit)
        {
            var p = new DoodlePainter(-0.48f, -0.48f, 0.48f, 0.48f, 140) { Ink = 0.04f };
            if (lit) p.FillFunc(Sd.Circle(0, 0, 0.48f), (x, y) => Rgba.Hex("FFF07A").WithAlpha(Math.Max(0f, 0.6f - (float)Math.Sqrt(x * x + y * y)) * 1.2f));
            Rgba ring = lit ? Rgba.Hex("FFB03A") : Rgba.Hex("3AA7E8"), top = lit ? Rgba.Hex("FFE96B") : Rgba.Hex("BDE6FF");
            p.Shape(Sd.Circle(0, 0, 0.36f), ring);
            p.Shape(Sd.Circle(0, 0.03f, 0.25f), top, 0.03f);
            float r1 = 0.15f, r2 = 0.065f;
            var pts = new float[20];
            for (int i = 0; i < 10; i++)
            {
                double a = Math.PI / 2 + i * Math.PI / 5;
                float r = i % 2 == 0 ? r1 : r2;
                pts[i * 2] = (float)Math.Cos(a) * r; pts[i * 2 + 1] = 0.03f + (float)Math.Sin(a) * r;
            }
            p.Shape(Sd.Polygon(pts), lit ? Rgba.Hex("FF5A6E") : Rgba.Hex("7FC6F0"), 0.02f);
            p.Fill(Sd.Ellipse(-0.1f, 0.17f, 0.06f, 0.03f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt(lit ? "pin_bumper_lit" : "pin_bumper", p);
        }

        /// <summary>Bersaglio a faccia di gatto (centro 0,0, raggio 0.6): bocca chiusa o spalancata (bocca in 0,-0.25).</summary>
        public static DoodleArt PinCat(bool open)
        {
            var p = new DoodlePainter(-0.75f, -0.72f, 0.75f, 0.85f, 140) { Ink = 0.04f };
            Rgba fur = Rgba.Hex("FFA54A"), shade = Rgba.Hex("E07F2A");
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Polygon(s * 0.58f, 0.18f, s * 0.52f, 0.78f, s * 0.15f, 0.5f), fur);
                p.Fill(Sd.Polygon(s * 0.5f, 0.28f, s * 0.48f, 0.64f, s * 0.26f, 0.47f), Rgba.Hex("FFB3C1"));
            }
            Sdf face = Sd.Ellipse(0, 0, 0.66f, 0.58f);
            Body(p, face, fur, shade);
            if (open)
            {
                p.Shape(Sd.Ellipse(0, -0.24f, 0.3f, 0.26f), Rgba.Hex("6B1A2A"), 0.04f);
                p.Fill(Sd.Ellipse(0, -0.36f, 0.16f, 0.09f), Rgba.Hex("E8687E"));
                p.Shape(Sd.Polygon(-0.17f, -0.03f, -0.11f, -0.03f, -0.14f, -0.12f), Rgba.White, 0.015f);
                p.Shape(Sd.Polygon(0.17f, -0.03f, 0.11f, -0.03f, 0.14f, -0.12f), Rgba.White, 0.015f);
                foreach (float s in new[] { -1f, 1f })
                {
                    p.Shape(Sd.Circle(s * 0.25f, 0.2f, 0.13f), Rgba.White, 0.03f);
                    p.Fill(Sd.Circle(s * 0.25f, 0.2f, 0.06f), Ink20);
                }
            }
            else
            {
                foreach (float s in new[] { -1f, 1f })
                    p.InkLine(0.045f, s * 0.36f, 0.22f, s * 0.25f, 0.29f, s * 0.14f, 0.22f);
                p.InkLine(0.04f, -0.16f, -0.2f, -0.08f, -0.26f, 0f, -0.2f, 0.08f, -0.26f, 0.16f, -0.2f);
                p.Fill(Sd.Ellipse(-0.36f, -0.1f, 0.09f, 0.05f), Rgba.Hex("FF8FA0").WithAlpha(0.6f));
                p.Fill(Sd.Ellipse(0.36f, -0.1f, 0.09f, 0.05f), Rgba.Hex("FF8FA0").WithAlpha(0.6f));
            }
            p.Shape(Sd.Polygon(-0.06f, 0.0f, 0.06f, 0.0f, 0f, -0.07f), Rgba.Hex("FF8FA0"), 0.02f);
            foreach (float s in new[] { -1f, 1f })
            {
                p.InkLine(0.02f, s * 0.42f, -0.02f, s * 0.72f, 0.03f);
                p.InkLine(0.02f, s * 0.42f, -0.08f, s * 0.72f, -0.12f);
            }
            return new DoodleArt(open ? "pin_cat_open" : "pin_cat_closed", p);
        }

        /// <summary>Molla del lanciatore (centro in alto 0,0; si allunga verso il basso fino a -0.6).</summary>
        public static DoodleArt PinPlunger()
        {
            var p = new DoodlePainter(-0.26f, -0.75f, 0.26f, 0.08f, 140) { Ink = 0.03f };
            p.Shape(Sd.Box(0, -0.02f, 0.22f, 0.06f, 0.03f), Rgba.Hex("FF5A6E"));
            for (int i = 0; i < 5; i++)
            {
                float y = -0.12f - i * 0.1f;
                p.InkLine(0.04f, -0.14f, y, 0.14f, y - 0.05f);
                p.Line(Rgba.Hex("C9D3E0"), 0.02f, -0.14f, y, 0.14f, y - 0.05f);
            }
            p.Shape(Sd.Box(0, -0.66f, 0.18f, 0.06f, 0.03f), Rgba.Hex("7A7F99"));
            return new DoodleArt("pin_plunger", p);
        }

        // ---------- Air hockey ----------

        /// <summary>Tavolo da air hockey (5.2×7.4, centro 0,0): bordo blu, linea di metà campo, porte larghe 1.5 in alto e in basso.</summary>
        public static DoodleArt HockeyTable()
        {
            const float hx = 2.5f, hy = 3.6f;
            var p = new DoodlePainter(-hx - 0.2f, -hy - 0.2f, hx + 0.2f, hy + 0.2f, 50) { Ink = 0.06f, Wobble = 0.004f };
            p.Shape(Sd.Box(0, 0, hx + 0.16f, hy + 0.16f, 0.35f), Rgba.Hex("3A6FD8"));
            p.FillFunc(Sd.Box(0, 0, hx, hy, 0.25f), (x, y) => Rgba.Lerp(Rgba.Hex("F2FAFF"), Rgba.Hex("D8EEFA"), (float)Math.Abs(y) / hy));
            for (float x = -2.2f; x <= 2.21f; x += 0.4f)
                for (float y = -3.3f; y <= 3.31f; y += 0.4f)
                    p.Fill(Sd.Circle(x, y, 0.018f), Rgba.Hex("A9C6DA"));
            Rgba red = Rgba.Hex("E5484D"), blue = Rgba.Hex("3A6FD8");
            p.Line(red, 0.05f, -hx, 0, hx, 0);
            p.Fill(Sd.Subtract(Sd.Circle(0, 0, 0.62f), Sd.Circle(0, 0, 0.57f)), red);
            p.Fill(Sd.Circle(0, 0, 0.08f), red);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Intersect(Sd.Subtract(Sd.Circle(0, s * hy, 1.05f), Sd.Circle(0, s * hy, 1.0f)), Sd.Box(0, 0, hx, hy)), blue);
                p.Line(blue, 0.04f, -hx, s * 1.9f, hx, s * 1.9f);
                // porta: fessura scura nel bordo
                p.Shape(Sd.Box(0, s * (hy + 0.08f), 0.75f, 0.14f, 0.06f), Rgba.Hex("1D1A33"), 0.04f);
            }
            return new DoodleArt("hockey_table", p);
        }

        /// <summary>Mazzuolo del giocatore visto dall'alto (raggio 0.38, centro 0,0).</summary>
        public static DoodleArt HockeyMallet()
        {
            var p = new DoodlePainter(-0.44f, -0.44f, 0.44f, 0.44f, 140) { Ink = 0.04f };
            Body(p, Sd.Circle(0, 0, 0.38f), Rgba.Hex("E5484D"), Rgba.Hex("B8323A"));
            p.Shape(Sd.Circle(0, 0, 0.24f), Rgba.Hex("F26A6E"), 0.03f);
            p.Shape(Sd.Circle(0, 0.02f, 0.14f), Rgba.Hex("FF8C8F"), 0.03f);
            p.Fill(Sd.Ellipse(-0.05f, 0.08f, 0.05f, 0.03f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("hockey_mallet", p);
        }

        /// <summary>Mazzuolo del gatto: zampa arancione vista da sotto con i polpastrelli (raggio 0.38, centro 0,0).</summary>
        public static DoodleArt HockeyPawMallet()
        {
            var p = new DoodlePainter(-0.44f, -0.44f, 0.44f, 0.44f, 140) { Ink = 0.04f };
            Body(p, Sd.Circle(0, 0, 0.38f), Rgba.Hex("FFA54A"), Rgba.Hex("E07F2A"));
            Rgba pink = Rgba.Hex("FF8FA8");
            p.Shape(Sd.Ellipse(0, -0.1f, 0.15f, 0.12f), pink, 0.025f);
            p.Shape(Sd.Circle(-0.18f, 0.08f, 0.065f), pink, 0.025f);
            p.Shape(Sd.Circle(-0.07f, 0.18f, 0.065f), pink, 0.025f);
            p.Shape(Sd.Circle(0.07f, 0.18f, 0.065f), pink, 0.025f);
            p.Shape(Sd.Circle(0.18f, 0.08f, 0.065f), pink, 0.025f);
            return new DoodleArt("hockey_paw_mallet", p);
        }

        /// <summary>Disco (raggio 0.24, centro 0,0).</summary>
        public static DoodleArt HockeyPuck()
        {
            var p = new DoodlePainter(-0.29f, -0.29f, 0.29f, 0.29f, 160) { Ink = 0.035f };
            p.Shape(Sd.Circle(0, 0, 0.24f), Rgba.Hex("3B3550"));
            p.Fill(Sd.Subtract(Sd.Circle(0, 0, 0.17f), Sd.Circle(0, 0, 0.14f)), Rgba.Hex("FFD24A"));
            p.Fill(Sd.Ellipse(-0.08f, 0.1f, 0.06f, 0.03f), Rgba.White.WithAlpha(0.4f));
            return new DoodleArt("hockey_puck", p);
        }

        /// <summary>Zampone del gatto che copre la porta: polpastrelli in basso (larghi 1.8, centro 0,0), braccio verso l'alto.</summary>
        public static DoodleArt HockeyBigPaw()
        {
            var p = new DoodlePainter(-1.0f, -0.62f, 1.0f, 1.9f, 90) { Ink = 0.05f };
            Rgba fur = Rgba.Hex("FFA54A"), shade = Rgba.Hex("E07F2A");
            Sdf arm = Sd.Box(0, 1.2f, 0.62f, 0.9f, 0.1f);
            Sdf paw = Sd.Union(Sd.Ellipse(0, 0, 0.92f, 0.56f), Sd.Box(0, 0.4f, 0.62f, 0.3f));
            Body(p, Sd.Union(arm, paw), fur, shade);
            for (int i = 0; i < 4; i++) p.Line(Rgba.Hex("D06A1A"), 0.05f, -0.4f + i * 0.08f, 1.0f + i * 0.2f, 0.4f - i * 0.05f, 1.15f + i * 0.2f);
            Rgba pink = Rgba.Hex("FF8FA8");
            p.Shape(Sd.Ellipse(0, -0.12f, 0.36f, 0.26f), pink, 0.04f);
            p.Shape(Sd.Circle(-0.6f, 0.08f, 0.14f), pink, 0.04f);
            p.Shape(Sd.Circle(-0.22f, 0.3f, 0.14f), pink, 0.04f);
            p.Shape(Sd.Circle(0.22f, 0.3f, 0.14f), pink, 0.04f);
            p.Shape(Sd.Circle(0.6f, 0.08f, 0.14f), pink, 0.04f);
            return new DoodleArt("hockey_big_paw", p);
        }

        // ---------- Basket ----------

        /// <summary>Palestra (6×10, centro 0,0): parete azzurra con righe, parquet in basso in prospettiva.</summary>
        public static DoodleArt DunkGym()
        {
            var p = new DoodlePainter(-3f, -5f, 3f, 5f, 40) { Wobble = 0.004f };
            p.FillFunc(Sd.Box(0, 0, 3f, 5f), (x, y) => Rgba.Lerp(Rgba.Hex("BFE0F2"), Rgba.Hex("8EC3E0"), (y + 5f) / 10f));
            for (float y = -0.6f; y < 5f; y += 0.9f) p.Line(Rgba.Hex("A7D0E8"), 0.04f, -3f, y, 3f, y);
            p.Fill(Sd.Box(0, 4.4f, 3f, 0.25f), Rgba.Hex("FF8A3D").WithAlpha(0.6f));
            // parquet: trapezio in prospettiva
            Sdf floor = Sd.Box(0, -3.3f, 3f, 1.7f);
            p.FillFunc(floor, (x, y) =>
            {
                float t = (y + 5f) / 3.4f;
                float u = x / (1.8f + (1f - t) * 1.6f);
                float board = (float)Math.Abs(Math.Sin(u * 9f));
                return Rgba.Lerp(Rgba.Hex("E6A75E"), Rgba.Hex("D18D45"), board * 0.5f);
            });
            p.InkLine(0.05f, -3f, -1.6f, 3f, -1.6f);
            p.Line(Rgba.White.WithAlpha(0.85f), 0.06f, -2.2f, -1.6f, -2.9f, -5f);
            p.Line(Rgba.White.WithAlpha(0.85f), 0.06f, 2.2f, -1.6f, 2.9f, -5f);
            p.Fill(Sd.Subtract(Sd.Ellipse(0, -1.6f, 1.4f, 0.5f), Sd.Ellipse(0, -1.6f, 1.33f, 0.45f)), Rgba.White.WithAlpha(0.85f));
            return new DoodleArt("dunk_gym", p);
        }

        /// <summary>Pallone da basket (raggio 0.45, centro 0,0).</summary>
        public static DoodleArt DunkBall()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.5f, 0.5f, 160) { Ink = 0.04f };
            Sdf b = Sd.Circle(0, 0, 0.45f);
            Body(p, b, Rgba.Hex("F28A2E"), Rgba.Hex("C9631A"));
            Rgba seam = Ink20;
            p.Fill(Sd.Intersect(b, Sd.Box(0, 0, 0.5f, 0.018f)), seam);
            p.Fill(Sd.Intersect(b, Sd.Box(0, 0, 0.018f, 0.5f)), seam);
            p.Fill(Sd.Intersect(b, Sd.Subtract(Sd.Circle(-0.62f, 0, 0.42f), Sd.Circle(-0.62f, 0, 0.385f))), seam);
            p.Fill(Sd.Intersect(b, Sd.Subtract(Sd.Circle(0.62f, 0, 0.42f), Sd.Circle(0.62f, 0, 0.385f))), seam);
            p.Shape(b, Rgba.Clear);
            p.Fill(Sd.Ellipse(-0.17f, 0.22f, 0.08f, 0.05f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("dunk_ball", p);
        }

        /// <summary>Tabellone con la metà posteriore del ferro (ferro in 0,0, largo 1.0; tabellone sopra).</summary>
        public static DoodleArt DunkHoopBack()
        {
            var p = new DoodlePainter(-0.95f, -0.25f, 0.95f, 1.35f, 120) { Ink = 0.045f };
            Sdf board = Sd.Box(0, 0.68f, 0.88f, 0.6f, 0.06f);
            Body(p, board, Rgba.Hex("FFFFFF"), Rgba.Hex("E2E8F0"));
            p.Fill(Sd.Subtract(Sd.Box(0, 0.42f, 0.36f, 0.3f), Sd.Box(0, 0.42f, 0.3f, 0.24f)), Rgba.Hex("E5484D"));
            p.Shape(Sd.Box(0, 0.06f, 0.08f, 0.08f), Rgba.Hex("9AA3B5"), 0.03f);
            // metà dietro del ferro (ellisse in alto)
            Sdf ring = Sd.Subtract(Sd.Ellipse(0, 0, 0.52f, 0.13f), Sd.Ellipse(0, 0, 0.45f, 0.08f));
            p.Shape(Sd.Intersect(ring, Sd.Box(0, 0.2f, 0.6f, 0.2f)), Rgba.Hex("C9431A"), 0.03f);
            return new DoodleArt("dunk_hoop_back", p);
        }

        /// <summary>Metà davanti del ferro con la retina (ferro in 0,0).</summary>
        public static DoodleArt DunkHoopFront()
        {
            var p = new DoodlePainter(-0.6f, -0.85f, 0.6f, 0.18f, 120) { Ink = 0.035f };
            Rgba net = Rgba.White.WithAlpha(0.95f);
            for (int i = 0; i <= 6; i++)
            {
                float x0 = -0.48f + i * 0.16f, x1 = -0.3f + i * 0.1f;
                p.Line(Ink20, 0.035f, x0, -0.04f, x1, -0.78f);
                p.Line(net, 0.018f, x0, -0.04f, x1, -0.78f);
            }
            for (int k = 1; k <= 3; k++)
            {
                float y = -0.04f - k * 0.2f, w = 0.48f - k * 0.05f;
                p.Line(Ink20, 0.035f, -w, y, w, y);
                p.Line(net, 0.018f, -w, y, w, y);
            }
            Sdf ring = Sd.Subtract(Sd.Ellipse(0, 0, 0.52f, 0.13f), Sd.Ellipse(0, 0, 0.45f, 0.08f));
            p.Shape(Sd.Intersect(ring, Sd.Box(0, -0.2f, 0.6f, 0.2f)), Rgba.Hex("E85A24"), 0.03f);
            return new DoodleArt("dunk_hoop_front", p);
        }
    }
}
