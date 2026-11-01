using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.75 per il livello 275 "Toni's Armchair" (Toni è il solito rig di toni_lie/toni_stand; la
    /// TV e il mobiletto sono quelli del soggiorno della 0.72, "v72_b_tv" / "v72_b_cabinet" / "v72_b_remote"; la
    /// scatola dei biscotti è "hw_biscuit_box", il biscotto "cookie"): il soggiorno accogliente (sfondo intero), la
    /// poltrona (due uguali), il tavolino, il cuscino "YOU", il giocattolo che squittisce, l'aspirapolvere, la coperta a
    /// quadri, la tazza, e due sovrapposizioni per Toni che non cambiano il rig: l'orecchio che si muove e la palpebra
    /// chiusa (un occhio solo aperto). Prefisso v75_a_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV75A() => new DoodleArt[]
        {
            V75A_LivingBg(), V75A_Armchair(), V75A_SideTable(), V75A_YouCushion(), V75A_Squeaky(), V75A_Vacuum(),
            V75A_Blanket(), V75A_Mug(), V75A_ToniEar(), V75A_ToniLid(), V75A_Noise(), V75A_TvPrematch(),
        };

        static readonly Rgba V75AChair = Rgba.Hex("D2694E"), V75AChairShade = Rgba.Hex("A9503A"), V75AChairLight = Rgba.Hex("E2856A");
        static readonly Rgba V75AWood = Rgba.Hex("9C6438"), V75AWoodShade = Rgba.Hex("774726");

        /// <summary>Linea del battiscopa (y del mondo): sopra la parete, sotto il parquet.</summary>
        public const float V75AFloorY = -1.0f;
        /// <summary>Altezza del piano di seduta della poltrona (coordinate locali, piedi in 0,0).</summary>
        public const float V75ASeatY = 0.79f;

        /// <summary>
        /// Il soggiorno accogliente, di sera: carta da parati color crema a righe con i rombini, un quadretto in alto a
        /// sinistra (-1.55, 1.95), l'orologio a destra (1.6, 2.05), il battiscopa (y -1.0), il parquet caldo e il tappeto
        /// ovale (centro 0, -3.5). Coordinate del mondo.
        /// </summary>
        public static DoodleArt V75A_LivingBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f, Wobble = 0.006f };
            Sdf wall = Sd.Box(0f, 3f, 5f, 4.05f);
            p.FillFunc(wall, (x, y) =>
            {
                float u = x / 0.6f - (float)Math.Floor(x / 0.6f);
                Rgba c = u < 0.5f ? Rgba.Hex("F4E3BC") : Rgba.Hex("EFD9AA");
                // un velo più caldo in basso (la luce della lampada)
                return Rgba.Lerp(c, Rgba.Hex("F1CF96"), Math.Max(0f, Math.Min(1f, (0.5f - y) / 3f)) * 0.5f);
            });
            for (int r = 0; r < 14; r++)
                for (int c = 0; c < 16; c++)
                {
                    float x = -4.5f + c * 0.6f + 0.3f, y = -0.7f + r * 0.6f + (c % 2) * 0.3f;
                    p.Fill(Sd.Rotate(Sd.Box(x, y, 0.045f, 0.045f), x, y, 45f), Rgba.Hex("D9B77C", 0.7f));
                }
            // il quadretto: una collina al tramonto
            float fx = -1.55f, fy = 1.95f;
            p.Shape(Sd.Box(fx, fy, 0.5f, 0.4f, 0.04f), Rgba.Hex("C9973F"), 0.05f);
            Sdf canvas = Sd.Box(fx, fy, 0.38f, 0.28f);
            p.FillFunc(canvas, (x, y) => Rgba.Lerp(Rgba.Hex("F7B27A"), Rgba.Hex("F6D58E"), Math.Max(0f, Math.Min(1f, (y - fy + 0.28f) / 0.56f))));
            p.Fill(Sd.Intersect(Sd.Circle(fx + 0.12f, fy + 0.02f, 0.11f), canvas), Rgba.Hex("FFF1B8"));
            p.Fill(Sd.Intersect(Sd.Ellipse(fx - 0.1f, fy - 0.3f, 0.5f, 0.24f), canvas), Rgba.Hex("7FB069"));
            p.Fill(Sd.Intersect(Sd.Ellipse(fx + 0.3f, fy - 0.32f, 0.4f, 0.18f), canvas), Rgba.Hex("5E9450"));
            p.Shape(canvas, Rgba.Clear, 0.03f);
            p.InkLine(0.02f, fx - 0.3f, fy + 0.4f, fx, fy + 0.62f, fx + 0.3f, fy + 0.4f);
            // l'orologio a muro
            float cx = 1.6f, cy = 2.05f;
            p.Shape(Sd.Circle(cx, cy, 0.36f), Rgba.Hex("7E9C8F"), 0.05f);
            p.Shape(Sd.Circle(cx, cy, 0.28f), Rgba.Hex("FFFBEF"), 0.03f);
            for (int i = 0; i < 12; i++)
            {
                double a = i * Math.PI / 6;
                p.Fill(Sd.Circle(cx + (float)Math.Cos(a) * 0.22f, cy + (float)Math.Sin(a) * 0.22f, 0.018f), Rgba.Hex("2B2233"));
            }
            p.InkLine(0.03f, cx, cy, cx + 0.02f, cy + 0.17f);
            p.InkLine(0.025f, cx, cy, cx + 0.14f, cy - 0.06f);
            // battiscopa
            p.Fill(Sd.Box(0f, V75AFloorY + 0.07f, 5f, 0.08f), Rgba.Hex("FFFDF6"), false);
            p.Line(Rgba.Hex("2B2233", 0.6f), 0.04f, -4.5f, V75AFloorY + 0.15f, 4.5f, V75AFloorY + 0.15f);
            // parquet a listoni
            Sdf floor = Sd.Box(0f, V75AFloorY - 3.5f, 5f, 3.5f);
            p.FillFunc(floor, (x, y) =>
            {
                int row = (int)Math.Floor((V75AFloorY - y) / 0.42f);
                float shift = (row % 3) * 0.55f;
                float u = (x + shift) / 1.7f - (float)Math.Floor((x + shift) / 1.7f);
                Rgba c = row % 2 == 0 ? Rgba.Hex("C78A55") : Rgba.Hex("BD7F4B");
                if (u < 0.012f) c = Rgba.Hex("8F5A31");
                float v = (V75AFloorY - y) / 0.42f - row;
                if (v < 0.04f) c = Rgba.Hex("9C6438");
                return c;
            });
            p.Line(Rgba.Hex("2B2233", 0.5f), 0.03f, -4.5f, V75AFloorY, 4.5f, V75AFloorY);
            // il tappeto ovale
            Sdf rug = Sd.Ellipse(0f, -3.5f, 2.55f, 0.9f);
            p.Shape(rug, Rgba.Hex("3F7F86"), 0.04f);
            p.Shape(Sd.Ellipse(0f, -3.5f, 2.2f, 0.72f), Rgba.Hex("F2E6C8"), 0.025f);
            p.Shape(Sd.Ellipse(0f, -3.5f, 1.65f, 0.5f), Rgba.Hex("E8A15A"), 0.025f);
            p.Shape(Sd.Ellipse(0f, -3.5f, 0.9f, 0.26f), Rgba.Hex("F2E6C8"), 0.02f);
            return new DoodleArt("v75_a_living_bg", p);
        }

        /// <summary>
        /// La poltrona (piedi in 0,0, larga 1.9): schienale capitonné (fino a y 1.78), braccioli a rullo (x ±0.64..0.96,
        /// cima y 1.05), cuscino della seduta con il piano in y <see cref="V75ASeatY"/>. Le due poltrone sono identiche.
        /// </summary>
        public static DoodleArt V75A_Armchair()
        {
            var p = new DoodlePainter(-1.02f, -0.05f, 1.02f, 1.85f, 110) { Ink = 0.045f };
            foreach (float s in new[] { -1f, 1f })
                p.Shape(Sd.Box(s * 0.72f, 0.07f, 0.06f, 0.09f, 0.03f), V75AWoodShade, 0.03f);
            // schienale con i bottoni
            Sdf back = Sd.Box(0f, 1.2f, 0.74f, 0.58f, 0.3f);
            Body(p, back, V75AChair, V75AChairShade);
            foreach (float x in new[] { -0.38f, 0f, 0.38f })
            {
                p.Fill(Sd.Circle(x, 1.3f, 0.035f), V75AChairShade);
                p.Line(V75AChairShade, 0.018f, x - 0.1f, 1.42f, x, 1.3f, x + 0.1f, 1.42f);
            }
            // base
            Sdf baseS = Sd.Box(0f, 0.38f, 0.92f, 0.26f, 0.1f);
            Body(p, baseS, V75AChair, V75AChairShade);
            p.Line(V75AChairShade, 0.02f, -0.8f, 0.2f, 0.8f, 0.2f);
            // cuscino della seduta
            Sdf seat = Sd.Box(0f, 0.66f, 0.68f, 0.13f, 0.1f);
            Body(p, seat, V75AChairLight, V75AChair);
            p.Line(Rgba.White.WithAlpha(0.35f), 0.02f, -0.5f, 0.74f, 0.4f, 0.74f);
            // braccioli a rullo
            foreach (float s in new[] { -1f, 1f })
            {
                Sdf arm = Sd.Box(s * 0.8f, 0.6f, 0.16f, 0.42f, 0.12f);
                Body(p, arm, V75AChair, V75AChairShade);
                Sdf roll = Sd.Ellipse(s * 0.8f, 1.0f, 0.2f, 0.1f);
                Body(p, roll, V75AChairLight, V75AChair);
                p.Shape(Sd.Circle(s * 0.8f, 1.0f, 0.045f), V75AChairShade, 0.02f);
            }
            return new DoodleArt("v75_a_armchair", p);
        }

        /// <summary>Il tavolino tondo tra le poltrone (piedi in 0,0, piano in y 0.72, largo 0.66).</summary>
        public static DoodleArt V75A_SideTable()
        {
            var p = new DoodlePainter(-0.4f, -0.05f, 0.4f, 0.82f, 120) { Ink = 0.04f };
            p.Shape(Sd.Ellipse(0f, 0.05f, 0.22f, 0.05f), V75AWoodShade, 0.03f);
            Body(p, Sd.Box(0f, 0.38f, 0.045f, 0.32f, 0.02f), V75AWood, V75AWoodShade);
            Body(p, Sd.Box(0f, 0.7f, 0.33f, 0.055f, 0.04f), Rgba.Hex("B87A48"), V75AWood);
            p.Line(Rgba.White.WithAlpha(0.35f), 0.015f, -0.25f, 0.73f, 0.2f, 0.73f);
            return new DoodleArt("v75_a_side_table", p);
        }

        /// <summary>Il cuscino "YOU" (centro 0,0, 1.3 x 0.9): giallo, con le nappine e le cuciture (la scritta è a parte).</summary>
        public static DoodleArt V75A_YouCushion()
        {
            var p = new DoodlePainter(-0.78f, -0.56f, 0.78f, 0.56f, 120) { Ink = 0.045f };
            foreach (float sx in new[] { -1f, 1f })
                foreach (float sy in new[] { -1f, 1f })
                    p.Shape(Sd.Circle(sx * 0.66f, sy * 0.42f, 0.07f), Rgba.Hex("E46A6A"), 0.025f);
            Sdf c = Sd.Box(0f, 0f, 0.63f, 0.42f, 0.22f);
            Body(p, c, Rgba.Hex("FFD15C"), Rgba.Hex("E8A93A"));
            p.Line(Rgba.Hex("E8A93A"), 0.02f, -0.48f, 0.3f, 0.48f, 0.3f);
            p.Line(Rgba.Hex("E8A93A"), 0.02f, -0.48f, -0.3f, 0.48f, -0.3f);
            p.Line(Rgba.White.WithAlpha(0.45f), 0.03f, -0.42f, 0.22f, -0.2f, 0.27f);
            return new DoodleArt("v75_a_you_cushion", p);
        }

        /// <summary>La paperella di gomma che squittisce (centro 0,0, larga 0.7).</summary>
        public static DoodleArt V75A_Squeaky()
        {
            var p = new DoodlePainter(-0.38f, -0.28f, 0.42f, 0.34f, 160) { Ink = 0.035f };
            Body(p, Sd.Ellipse(-0.03f, -0.08f, 0.32f, 0.17f), Rgba.Hex("FFD84A"), Rgba.Hex("E5AE22"));
            Body(p, Sd.Circle(0.17f, 0.13f, 0.13f), Rgba.Hex("FFD84A"), Rgba.Hex("E5AE22"));
            p.Shape(Sd.Polygon(0.27f, 0.14f, 0.4f, 0.1f, 0.28f, 0.06f), Rgba.Hex("F28A2E"), 0.025f);
            p.Fill(Sd.Circle(0.2f, 0.17f, 0.025f), Rgba.Hex("2B2233"));
            p.Line(Rgba.Hex("E5AE22"), 0.025f, -0.2f, -0.02f, -0.05f, -0.1f, 0.08f, -0.03f);
            // il fischietto sotto la coda
            p.Shape(Sd.Circle(-0.3f, -0.12f, 0.035f), Rgba.Hex("F28A2E"), 0.02f);
            return new DoodleArt("v75_a_squeaky", p);
        }

        /// <summary>L'aspirapolvere verticale (ruote in 0,0, manico in alto fino a y 1.5).</summary>
        public static DoodleArt V75A_Vacuum()
        {
            var p = new DoodlePainter(-0.4f, -0.05f, 0.4f, 1.56f, 120) { Ink = 0.04f };
            // manico
            p.Shape(Sd.Capsule(0.03f, 0.75f, 0.1f, 1.38f, 0.035f), Rgba.Hex("3A3442"), 0.03f);
            p.Shape(Sd.Capsule(0.0f, 1.42f, 0.24f, 1.46f, 0.055f), Rgba.Hex("2D2A35"), 0.03f);
            // corpo col sacchetto
            Sdf body = Sd.Box(0.02f, 0.52f, 0.19f, 0.33f, 0.12f);
            Body(p, body, Rgba.Hex("4B8FD6"), Rgba.Hex("336BA8"));
            p.Shape(Sd.Box(0.02f, 0.58f, 0.11f, 0.18f, 0.06f), Rgba.Hex("BFD9F2"), 0.025f);
            p.Shape(Sd.Circle(0.02f, 0.3f, 0.05f), Rgba.Hex("E8473F"), 0.02f);   // l'interruttore
            // spazzola e ruote
            Body(p, Sd.Box(0f, 0.12f, 0.32f, 0.09f, 0.06f), Rgba.Hex("6E6E7A"), Rgba.Hex("4D4D57"));
            p.Shape(Sd.Circle(0.22f, 0.06f, 0.06f), Rgba.Hex("2D2A35"), 0.02f);
            p.Line(Rgba.Hex("3A3442"), 0.02f, -0.28f, 0.03f, 0.1f, 0.03f);
            return new DoodleArt("v75_a_vacuum", p);
        }

        /// <summary>La coperta a quadri piegata (centro 0,0, 1.1 x 0.42), con le frange.</summary>
        public static DoodleArt V75A_Blanket()
        {
            var p = new DoodlePainter(-0.62f, -0.26f, 0.62f, 0.26f, 130) { Ink = 0.04f };
            for (int i = 0; i < 7; i++)
            {
                float y = -0.15f + i * 0.05f;
                p.Line(Rgba.Hex("7A3B5A"), 0.012f, -0.55f, y, -0.6f, y - 0.01f);
            }
            Sdf b = Sd.Box(0f, 0f, 0.54f, 0.2f, 0.07f);
            p.FillFunc(b, (x, y) =>
            {
                float u = (x + 1f) / 0.18f - (float)Math.Floor((x + 1f) / 0.18f);
                float v = (y + 1f) / 0.18f - (float)Math.Floor((y + 1f) / 0.18f);
                Rgba c = Rgba.Hex("B8507A");
                if (u < 0.3f || v < 0.3f) c = Rgba.Hex("9E3F66");
                if (u < 0.3f && v < 0.3f) c = Rgba.Hex("7A3B5A");
                if (u > 0.55f && u < 0.62f) c = Rgba.Hex("F3D27A");
                return c;
            });
            p.Fill(Sd.Crescent(b, -0.04f, 0.05f), new Rgba(0f, 0f, 0f, 0.18f));
            p.Shape(b, Rgba.Clear);
            p.Line(Rgba.Hex("5E2C45"), 0.02f, -0.5f, 0.02f, 0.5f, 0.02f);
            return new DoodleArt("v75_a_blanket", p);
        }

        /// <summary>La tazza di cioccolata calda (centro 0,0, alta 0.3), con il vapore.</summary>
        public static DoodleArt V75A_Mug()
        {
            var p = new DoodlePainter(-0.2f, -0.18f, 0.24f, 0.42f, 180) { Ink = 0.03f };
            p.Shape(Sd.Subtract(Sd.Circle(0.14f, 0.0f, 0.085f), Sd.Circle(0.14f, 0.0f, 0.045f)), Rgba.Hex("F5EEDF"), 0.025f);
            Sdf mug = Sd.Box(0f, 0f, 0.13f, 0.15f, 0.03f);
            Body(p, mug, Rgba.Hex("F5EEDF"), Rgba.Hex("D9CFBC"));
            p.Fill(Sd.Box(0f, -0.02f, 0.13f, 0.035f), Rgba.Hex("D2694E"));
            p.Shape(Sd.Ellipse(0f, 0.15f, 0.12f, 0.03f), Rgba.Hex("7A4A2A"), 0.02f);
            foreach (float x in new[] { -0.05f, 0.05f })
                p.Line(Rgba.White.WithAlpha(0.8f), 0.02f, x, 0.2f, x + 0.03f, 0.26f, x - 0.02f, 0.32f, x + 0.02f, 0.38f);
            return new DoodleArt("v75_a_mug", p);
        }

        /// <summary>
        /// Lo schermo della TV col pre-partita (centro 0,0, misura dello schermo di "v72_b_tv": 2.1 x 1.44): il campo
        /// a strisce visto dall'alto, sfocato e scuro, così il conto alla rovescia scritto sopra si legge bene.
        /// </summary>
        public static DoodleArt V75A_TvPrematch()
        {
            var p = new DoodlePainter(-1.08f, -0.75f, 1.08f, 0.75f, 100) { Ink = 0.03f, Wobble = 0f };
            Sdf scr = Sd.Box(0f, 0f, 1.05f, 0.72f, 0.16f);
            p.FillFunc(scr, (x, y) =>
            {
                float u = (x + 2f) / 0.3f - (float)Math.Floor((x + 2f) / 0.3f);
                Rgba c = u < 0.5f ? Rgba.Hex("2F6E3A") : Rgba.Hex("29623A");
                return Rgba.Lerp(c, Rgba.Hex("14301C"), Math.Max(0f, Math.Min(1f, (0.72f - Math.Abs(y)) * 0.6f)) * 0.5f);
            });
            p.Line(Rgba.White.WithAlpha(0.35f), 0.025f, 0f, -0.72f, 0f, 0.72f);
            Sdf ring = Sd.Subtract(Sd.Circle(0f, 0f, 0.36f), Sd.Circle(0f, 0f, 0.33f));
            p.Fill(ring, Rgba.White.WithAlpha(0.35f), false);
            p.Line(Rgba.White.WithAlpha(0.35f), 0.025f, -1.05f, -0.4f, -0.8f, -0.4f, -0.8f, 0.4f, -1.05f, 0.4f);
            p.Line(Rgba.White.WithAlpha(0.35f), 0.025f, 1.05f, -0.4f, 0.8f, -0.4f, 0.8f, 0.4f, 1.05f, 0.4f);
            // la fascia "LIVE" in alto a sinistra
            p.Fill(Sd.Box(-0.78f, 0.55f, 0.17f, 0.07f, 0.03f), Rgba.Hex("E8473F"), false);
            p.Fill(Sd.Circle(-0.88f, 0.55f, 0.025f), Rgba.White, false);
            return new DoodleArt("v75_a_tv_prematch", p);
        }

        // Le sovrapposizioni per Toni: stesse forme e stessi colori del suo disegno (DoodleSprites15), così da ferme
        // coincidono con l'orecchio e con l'occhio disegnati.

        /// <summary>Attacco dell'orecchio destro di Toni sulla testa (coordinate locali di Toni).</summary>
        public const float V75AEarX = -0.4f, V75AEarY = 0.7f;

        /// <summary>
        /// L'orecchio ripiegato di Toni (quello verso la schiena), solo la parte fuori dalla testa; l'origine è
        /// l'attacco (<see cref="V75AEarX"/>, <see cref="V75AEarY"/>): ruotandolo l'orecchio "si muove".
        /// </summary>
        public static DoodleArt V75A_ToniEar()
        {
            var p = new DoodlePainter(-0.12f, -0.2f, 0.3f, 0.16f, 130) { Ink = 0.05f };
            Sdf ear = Sd.Rotate(Sd.Ellipse(-0.32f, 0.68f, 0.14f, 0.1f), -0.32f, 0.68f, -35f);
            Sdf head = Sd.Ellipse(-0.72f, 0.38f, 0.46f, 0.42f);
            Sdf part = Sd.Offset(Sd.Subtract(ear, head), -V75AEarX, -V75AEarY);
            Body(p, part, PugShade, Rgba.Hex("141418"));
            return new DoodleArt("v75_a_toni_ear", p);
        }

        /// <summary>La palpebra chiusa di Toni (centro 0,0 = centro dell'occhio): copre un occhio aperto, l'altro guarda.</summary>
        public static DoodleArt V75A_ToniLid()
        {
            var p = new DoodlePainter(-0.17f, -0.17f, 0.17f, 0.17f, 200) { Ink = 0.025f, Wobble = 0f };
            p.Fill(Sd.Circle(0f, 0f, PugEyeR + 0.045f), PugFawn, false);
            p.Line(PugLine, 0.03f, -0.09f, 0.02f, -0.05f, -0.02f, 0f, -0.035f, 0.05f, -0.02f, 0.09f, 0.02f);
            return new DoodleArt("v75_a_toni_lid", p);
        }

        /// <summary>Le onde del rumore dell'aspirapolvere (tre archi a destra dell'origine).</summary>
        public static DoodleArt V75A_Noise()
        {
            var p = new DoodlePainter(-0.05f, -0.4f, 0.6f, 0.4f, 120) { Ink = 0.03f };
            for (int i = 0; i < 3; i++)
            {
                float r = 0.18f + i * 0.17f;
                var pts = new List<float>();
                for (int k = 0; k <= 10; k++)
                {
                    double a = -0.75 + 1.5 * k / 10.0;
                    pts.Add((float)Math.Cos(a) * r);
                    pts.Add((float)Math.Sin(a) * r);
                }
                p.Line(Rgba.Hex("2B2233"), 0.04f, pts.ToArray());
            }
            return new DoodleArt("v75_a_noise", p);
        }
    }
}
