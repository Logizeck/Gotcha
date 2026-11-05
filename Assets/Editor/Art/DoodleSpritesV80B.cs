using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.80 per il livello 30 della roadmap "The Invisible Treasure" (la X rossa è "v40b_x_mark", le
    /// buche "v40b_sand_hole", il baule "chest"/"chest_open", lo scarpone "fish_boot", la lattina "tin_can", le monete
    /// "coin", le scintille "sparkle_star", gli sbuffi di sabbia "puff"): lo sfondo (cielo, mare, isoletta di sabbia), la
    /// palma, la mappa del tesoro SENZA la X (la X è un oggetto a parte che si trascina), la pala (origine sulla punta), il
    /// granchio e l'anello tratteggiato che segna dove scaverà la pala. Nessuna scritta disegnata. Prefisso v80_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV80B() => new DoodleArt[]
        {
            V80B_IslandBg(), V80B_Palm(), V80B_Map(), V80B_Shovel(), V80B_Crab(), V80B_DigRing(),
        };

        // misure condivise col builder e col livello
        /// <summary>L'isola di sabbia (coordinate del mondo): ellisse di centro (X, Y) e semiassi Rx, Ry.</summary>
        public const float V80BIslandX = 0f, V80BIslandY = -1.45f, V80BIslandRx = 2.3f, V80BIslandRy = 2.0f;
        /// <summary>Linea dell'orizzonte (sopra il cielo, sotto il mare).</summary>
        public const float V80BHorizon = 1.2f;
        /// <summary>Base del tronco della palma (coordinate del mondo).</summary>
        public const float V80BPalmX = 1.3f, V80BPalmY = -0.3f;
        /// <summary>Il posto (coordinate del mondo) indicato dalla X della mappa: accanto alla palma. Lì non c'è niente.</summary>
        public const float V80BSpotX = 0.5f, V80BSpotY = -0.75f;
        /// <summary>Mezza larghezza e mezza altezza del foglio della mappa (origine al centro).</summary>
        public const float V80BMapHalfW = 1.1f, V80BMapHalfH = 0.7f;
        /// <summary>Centro dell'isola disegnata sulla mappa (coordinate locali della mappa) e rimpicciolimento mondo -> mappa.</summary>
        public const float V80BMapIslandX = 0.02f, V80BMapIslandY = -0.08f, V80BMapScale = 0.27f;

        static readonly Rgba V80BSand = Rgba.Hex("F4D58D"), V80BSandShade = Rgba.Hex("E2B86A"), V80BWetSand = Rgba.Hex("D9A95E");
        static readonly Rgba V80BPaper = Rgba.Hex("F6E3B4"), V80BPaperShade = Rgba.Hex("E3C98C"), V80BMapInk = Rgba.Hex("7A5A36");
        static readonly Rgba V80BLeaf = Rgba.Hex("4FAE4E"), V80BLeafShade = Rgba.Hex("3A8A3E"), V80BTrunk = Rgba.Hex("A8743F");
        static readonly Rgba V80BCrab = Rgba.Hex("F0553E"), V80BCrabShade = Rgba.Hex("C93E2E");

        static float V80BClamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        /// <summary>Distanza (con segno, approssimata) dall'isola di sabbia, con un bordo che ondeggia appena.</summary>
        static Sdf V80BIslandShape(float grow)
        {
            Sdf e = Sd.Ellipse(V80BIslandX, V80BIslandY, V80BIslandRx + grow, V80BIslandRy + grow);
            return (x, y) => e(x, y) + 0.035f * (float)Math.Sin(x * 3.1f + y * 1.7f);
        }

        /// <summary>
        /// Lo sfondo (coordinate del mondo): cielo azzurro con sole e nuvolette sopra <see cref="V80BHorizon"/>, mare con le
        /// onde, l'isola di sabbia (ellisse <see cref="V80BIslandX"/>...) con la sabbia bagnata e la schiuma attorno, qualche
        /// conchiglia e sassolino.
        /// </summary>
        public static DoodleArt V80B_IslandBg()
        {
            var p = new DoodlePainter(-4.5f, -5.6f, 4.5f, 5.6f, 50) { Ink = 0.06f, Wobble = 0.006f };
            float hz = V80BHorizon;
            // cielo
            p.FillFunc(Sd.Box(0f, 3.5f, 4.6f, 2.35f), (x, y) => Rgba.Lerp(Rgba.Hex("BFE9FF"), Rgba.Hex("5DB8F0"), V80BClamp((y - hz) / 3.5f)));
            p.Fill(Sd.Circle(2.15f, 2.55f, 0.55f), Rgba.Hex("FFF3B0", 0.45f), false);
            p.Shape(Sd.Circle(2.15f, 2.55f, 0.34f), Rgba.Hex("FFD84A"), 0.045f);
            foreach (float[] c in new[] { new[] { -0.2f, 2.75f, 1f }, new[] { 2.6f, 1.75f, 0.8f }, new[] { -2.9f, 1.9f, 0.7f } })
            {
                float s = c[2];
                Sdf cloud = Sd.Union(Sd.Ellipse(c[0], c[1], 0.5f * s, 0.16f * s), Sd.Circle(c[0] - 0.12f * s, c[1] + 0.1f * s, 0.2f * s),
                    Sd.Circle(c[0] + 0.18f * s, c[1] + 0.08f * s, 0.16f * s));
                p.Shape(cloud, Rgba.Hex("FFFFFF"), 0.04f);
            }
            // mare
            p.FillFunc(Sd.Box(0f, (hz - 5.6f) * 0.5f, 4.6f, (hz + 5.6f) * 0.5f),
                (x, y) => Rgba.Lerp(Rgba.Hex("1E86C8"), Rgba.Hex("5CC6E8"), V80BClamp((y + 5.6f) / (hz + 5.6f))));
            p.Line(Rgba.Hex("2C6FA6"), 0.04f, -4.5f, hz, 4.5f, hz);
            var rnd = new Random(3030);
            for (int i = 0; i < 46; i++)
            {
                float x = (float)rnd.NextDouble() * 8.8f - 4.4f, y = (float)rnd.NextDouble() * (hz + 5.3f) - 5.4f;
                if (Sd.Ellipse(V80BIslandX, V80BIslandY, V80BIslandRx + 0.45f, V80BIslandRy + 0.45f)(x, y) < 0f) continue;
                float w = 0.12f + (float)rnd.NextDouble() * 0.12f;
                p.Line(Rgba.Hex("E8FAFF", 0.75f), 0.035f, x - w, y, x - w * 0.5f, y + 0.06f, x, y, x + w * 0.5f, y + 0.06f, x + w, y);
            }
            // schiuma, sabbia bagnata, sabbia
            p.Fill(V80BIslandShape(0.2f), Rgba.Hex("F4FCFF", 0.9f));
            p.Fill(V80BIslandShape(0.1f), V80BWetSand);
            p.FillFunc(V80BIslandShape(0f), (x, y) => Rgba.Lerp(V80BSandShade, V80BSand, V80BClamp((y - V80BIslandY + 1.6f) / 2.6f)));
            p.Shape(V80BIslandShape(0f), Rgba.Clear, 0.06f);
            // puntini di sabbia, conchiglie e sassolini (piccoli: non sembrano cose da trovare)
            for (int i = 0; i < 90; i++)
            {
                float x = (float)rnd.NextDouble() * 4.4f - 2.2f, y = V80BIslandY + (float)rnd.NextDouble() * 3.8f - 1.9f;
                if (V80BIslandShape(-0.15f)(x, y) > 0f) continue;
                p.Fill(Sd.Circle(x, y, 0.014f + (float)rnd.NextDouble() * 0.012f), Rgba.Hex("C99A55", 0.6f), false);
            }
            foreach (float[] s in new[] { new[] { -1.85f, -1.0f }, new[] { 1.55f, -2.75f }, new[] { -0.35f, -3.05f } })
            {
                p.Shape(Sd.Ellipse(s[0], s[1], 0.09f, 0.065f), Rgba.Hex("FFE6E0"), 0.025f);
                p.Line(Rgba.Hex("E0A49A"), 0.015f, s[0], s[1] - 0.05f, s[0], s[1] + 0.05f);
            }
            foreach (float[] s in new[] { new[] { 1.85f, -1.75f }, new[] { -0.9f, 0.1f }, new[] { -2.0f, -2.35f } })
                p.Shape(Sd.Ellipse(s[0], s[1], 0.07f, 0.05f), Rgba.Hex("A9A39A"), 0.02f);
            return new DoodleArt("v80_b_island_bg", p);
        }

        /// <summary>La palma (origine alla base del tronco): tronco a segmenti curvo verso sinistra, ciuffo di foglie e noci di cocco.</summary>
        public static DoodleArt V80B_Palm()
        {
            var p = new DoodlePainter(-1.45f, -0.2f, 1.05f, 2.45f, 90) { Ink = 0.045f };
            // tronco: segmenti lungo una curva dalla base (0, 0) alla cima (-0.35, 1.85)
            const int n = 7;
            for (int i = n - 1; i >= 0; i--)
            {
                float t0 = i / (float)n, t1 = (i + 1) / (float)n;
                float x0 = -0.35f * t0 * t0, y0 = 1.85f * t0, x1 = -0.35f * t1 * t1, y1 = 1.85f * t1;
                float w0 = 0.15f - 0.05f * t0, w1 = 0.15f - 0.05f * t1;
                p.Shape(Sd.Polygon(x0 - w0, y0, x0 + w0, y0, x1 + w1 * 1.15f, y1 + 0.03f, x1 - w1 * 1.15f, y1 + 0.03f), V80BTrunk, 0.035f);
            }
            float cx = -0.35f, cy = 1.88f;
            // foglie: lunghe gocce piegate
            float[][] leaves =
            {
                new[] { -1.3f, 1.35f }, new[] { -1.15f, 2.2f }, new[] { -0.35f, 2.4f }, new[] { 0.5f, 2.2f }, new[] { 0.9f, 1.4f }, new[] { -0.6f, 1.1f },
            };
            foreach (float[] l in leaves)
            {
                float mx = (cx + l[0]) * 0.5f, my = (cy + l[1]) * 0.5f + 0.18f;
                Sdf leaf = Sd.Union(Sd.Polyline(0.13f, cx, cy, mx, my), Sd.Polyline(0.08f, mx, my, l[0], l[1]));
                p.Shape(leaf, V80BLeaf, 0.04f);
                p.Line(V80BLeafShade, 0.025f, cx, cy, mx, my, l[0], l[1]);
            }
            foreach (float[] c in new[] { new[] { -0.45f, 1.74f }, new[] { -0.22f, 1.7f }, new[] { -0.34f, 1.56f } })
                p.Shape(Sd.Circle(c[0], c[1], 0.1f), Rgba.Hex("6B4A2B"), 0.03f);
            return new DoodleArt("v80_b_palm", p);
        }

        /// <summary>
        /// La mappa del tesoro (origine al centro, mezza misura <see cref="V80BMapHalfW"/> x <see cref="V80BMapHalfH"/>):
        /// foglio ingiallito coi bordi strappati, l'isola rimpicciolita di <see cref="V80BMapScale"/> col suo mare, la palma
        /// nello stesso posto, una rosa dei venti e un sentiero tratteggiato che porta dove sta la X (la X è a parte).
        /// </summary>
        public static DoodleArt V80B_Map()
        {
            float hw = V80BMapHalfW, hh = V80BMapHalfH;
            var p = new DoodlePainter(-hw - 0.08f, -hh - 0.08f, hw + 0.08f, hh + 0.08f, 120) { Ink = 0.04f, Wobble = 0.01f };
            Sdf paper = (x, y) => Sd.Box(0f, 0f, hw - 0.02f, hh - 0.02f)(x, y) + 0.02f * (float)Math.Sin(x * 23f + y * 17f);
            p.FillFunc(paper, (x, y) => Rgba.Lerp(V80BPaperShade, V80BPaper, V80BClamp(1f - (Math.Abs(x) / hw + Math.Abs(y) / hh) * 0.45f)));
            p.Shape(paper, Rgba.Clear, 0.045f);
            p.Line(V80BPaperShade, 0.02f, -0.3f, -hh + 0.03f, -0.22f, -0.2f, -0.4f, 0.3f, -0.3f, hh - 0.03f);   // piega
            // il mare della mappa (righine) e l'isola
            float s = V80BMapScale, ix = V80BMapIslandX, iy = V80BMapIslandY;
            Sdf island = Sd.Ellipse(ix, iy, V80BIslandRx * s, V80BIslandRy * s);
            p.Hatch(Sd.Subtract(Sd.Ellipse(ix, iy, V80BIslandRx * s + 0.22f, V80BIslandRy * s + 0.18f), island), Rgba.Hex("6FB7D9", 0.55f), 0.08f, 0.018f, 0f);
            p.Shape(island, Rgba.Hex("EBC77E"), 0.03f);
            // la palma nello stesso posto che a terra
            float px = ix + (V80BPalmX - V80BIslandX) * s, py = iy + (V80BPalmY - V80BIslandY) * s;
            p.Line(Rgba.Hex("8A5A2E"), 0.035f, px, py, px - 0.03f, py + 0.2f, px - 0.06f, py + 0.32f);
            foreach (float a in new[] { 20f, 70f, 120f, 160f, 200f })
            {
                double r = a * Math.PI / 180;
                p.Line(Rgba.Hex("3E8E44"), 0.035f, px - 0.06f, py + 0.32f, px - 0.06f + 0.14f * (float)Math.Cos(r), py + 0.32f + 0.1f * (float)Math.Sin(r));
            }
            // rosa dei venti in alto a sinistra
            float rx = -hw + 0.3f, ry = hh - 0.3f;
            p.Shape(Sd.Polygon(rx, ry + 0.2f, rx + 0.05f, ry, rx, ry - 0.2f, rx - 0.05f, ry), Rgba.Hex("C9453A"), 0.02f);
            p.Shape(Sd.Polygon(rx - 0.2f, ry, rx, ry + 0.05f, rx + 0.2f, ry, rx, ry - 0.05f), V80BMapInk, 0.02f);
            // sentiero tratteggiato dall'angolo in basso a sinistra fin quasi alla X (che non è disegnata qui)
            float tx = ix + (V80BSpotX - V80BIslandX) * s, ty = iy + (V80BSpotY - V80BIslandY) * s;
            float[] path = { -hw + 0.18f, -hh + 0.16f, -0.62f, -0.48f, -0.35f, -0.12f, tx - 0.12f, ty - 0.06f };
            for (int i = 0; i + 3 < path.Length; i += 2)
            {
                float ax = path[i], ay = path[i + 1], bx = path[i + 2], by = path[i + 3];
                for (float t = 0f; t < 1f; t += 0.25f)
                {
                    float t2 = Math.Min(1f, t + 0.13f);
                    p.Line(Rgba.Hex("B8432F"), 0.025f, ax + (bx - ax) * t, ay + (by - ay) * t, ax + (bx - ax) * t2, ay + (by - ay) * t2);
                }
            }
            return new DoodleArt("v80_b_map", p);
        }

        /// <summary>La pala (origine sulla PUNTA della lama: lì si scava): lama di metallo, manico di legno, impugnatura a T.</summary>
        public static DoodleArt V80B_Shovel()
        {
            var p = new DoodlePainter(-0.36f, -0.08f, 0.36f, 2.18f, 110) { Ink = 0.04f };
            p.Shape(Sd.Box(0f, 1.3f, 0.055f, 0.72f, 0.03f), Rgba.Hex("B98248"));                       // manico
            p.Shape(Sd.Box(0f, 2.06f, 0.24f, 0.07f, 0.06f), Rgba.Hex("2E2E3A"));                        // impugnatura
            p.Shape(Sd.Box(0f, 1.94f, 0.06f, 0.1f), Rgba.Hex("2E2E3A"));
            Sdf blade = Sd.Union(Sd.Polygon(-0.25f, 0.62f, 0.25f, 0.62f, 0.27f, 0.25f, 0f, 0f, -0.27f, 0.25f), Sd.Box(0f, 0.66f, 0.09f, 0.08f));
            p.FillFunc(blade, (x, y) => Rgba.Lerp(Rgba.Hex("8E99A8"), Rgba.Hex("D7DEE6"), V80BClamp(0.5f + x * 1.6f)));
            p.Shape(blade, Rgba.Clear, 0.04f);
            p.Line(Rgba.Hex("FFFFFF", 0.7f), 0.03f, 0.12f, 0.52f, 0.17f, 0.28f);
            return new DoodleArt("v80_b_shovel", p);
        }

        /// <summary>Il granchio rosso con le chele alzate (origine al centro del guscio).</summary>
        public static DoodleArt V80B_Crab()
        {
            var p = new DoodlePainter(-0.6f, -0.32f, 0.6f, 0.52f, 120) { Ink = 0.035f };
            foreach (float sx in new[] { -1f, 1f })
            {
                for (int i = 0; i < 3; i++)
                {
                    float y = -0.05f - i * 0.07f;
                    p.Line(p.InkColor, 0.07f, sx * 0.2f, y, sx * 0.38f, y - 0.05f, sx * 0.45f, y - 0.16f);
                    p.Line(V80BCrabShade, 0.035f, sx * 0.2f, y, sx * 0.38f, y - 0.05f, sx * 0.45f, y - 0.16f);
                }
                p.Line(p.InkColor, 0.08f, sx * 0.22f, 0.08f, sx * 0.36f, 0.22f);
                p.Line(V80BCrab, 0.045f, sx * 0.22f, 0.08f, sx * 0.36f, 0.22f);
                Sdf claw = Sd.Subtract(Sd.Circle(sx * 0.4f, 0.32f, 0.14f), Sd.Polygon(sx * 0.4f, 0.3f, sx * 0.62f, 0.52f, sx * 0.4f, 0.55f));
                p.Shape(claw, V80BCrab, 0.035f);
            }
            p.Shape(Sd.Ellipse(0f, 0f, 0.27f, 0.18f), V80BCrab);
            p.Fill(Sd.Ellipse(0f, -0.07f, 0.2f, 0.07f), V80BCrabShade);
            foreach (float sx in new[] { -0.08f, 0.08f })
            {
                p.InkLine(0.03f, sx, 0.14f, sx, 0.26f);
                p.Shape(Sd.Circle(sx, 0.28f, 0.045f), Rgba.White, 0.025f);
                p.Fill(Sd.Circle(sx, 0.28f, 0.018f), p.InkColor, false);
            }
            p.InkLine(0.025f, -0.06f, 0.02f, 0f, 0.05f, 0.06f, 0.02f);
            return new DoodleArt("v80_b_crab", p);
        }

        /// <summary>L'anello tratteggiato bianco (sotto la punta della pala mentre la si trascina: "si scava qui"); il livello lo colora.</summary>
        public static DoodleArt V80B_DigRing()
        {
            var p = new DoodlePainter(-0.42f, -0.28f, 0.42f, 0.28f, 120) { Ink = 0f, Wobble = 0f };
            for (int i = 0; i < 12; i++)
            {
                double a0 = i * Math.PI * 2 / 12, a1 = a0 + Math.PI * 2 / 12 * 0.55;
                p.Line(Rgba.White, 0.05f, 0.34f * (float)Math.Cos(a0), 0.21f * (float)Math.Sin(a0),
                    0.34f * (float)Math.Cos((a0 + a1) * 0.5), 0.21f * (float)Math.Sin((a0 + a1) * 0.5),
                    0.34f * (float)Math.Cos(a1), 0.21f * (float)Math.Sin(a1));
            }
            return new DoodleArt("v80_b_dig_ring", p);
        }
    }
}
