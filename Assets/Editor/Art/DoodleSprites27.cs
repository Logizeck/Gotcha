using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli compositi della versione 0.40 (gruppo D).</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV40D() => new DoodleArt[]
        {
            D40Dot(),
            // Garden seasons
            D40GardenBackdrop(), D40Seed(), D40SeedPacket(), D40DugHole(), D40SoilMound(), D40WateringCan(), D40Sprout(), D40Plant(),
            D40Umbrella(), D40Shadow(), D40Pumpkin(), D40Mole(), D40MoleHole(), D40Greenhouse(), D40SnowLump(), D40Flake(), D40SnowGround(),
            // Museum night
            D40MuseumBackdrop(), D40Guard(), D40Cone(), D40Burglar(), D40Statue(), D40FakePainting(), D40LootBag(),
            // Ice cream shop
            D40ShopBackdrop(), D40Counter(), D40Cone2(), D40Scoop(), D40Sprinkles(), D40Tub(), D40TubCream(), D40Shaker(), D40CherryBowl(),
            D40Bin(), D40GreenButton(), D40Ticket(), D40CatCustomer(),
            // Pirate cannon
            D40SeaBackdrop(), D40Waves(), D40Ship(), D40JollyFlag(), D40PowderBarrel(), D40Captain(), D40Tricorn(), D40CannonBase(),
            D40CannonBarrel(), D40Ball(), D40Hook(), D40Chest(false), D40Chest(true), D40RedButton(),
        };

        static readonly Rgba D27Ink = DoodlePainter.DefaultInk;
        static readonly Rgba D27Dirt = Rgba.Hex("9A6A43"), D27DirtShade = Rgba.Hex("7A5030"), D27DirtDark = Rgba.Hex("3A2418");

        static void V40D_Eye(DoodlePainter p, float x, float y, float r)
        {
            p.Fill(Sd.Circle(x, y, r), D27Ink);
            p.Fill(Sd.Circle(x - r * 0.35f, y + r * 0.35f, r * 0.38f), Rgba.White);
        }

        /// <summary>Pallino bianco con bordo (si tinge; diametro 1).</summary>
        public static DoodleArt D40Dot()
        {
            var p = new DoodlePainter(-0.55f, -0.55f, 0.55f, 0.55f, 80) { Ink = 0.12f, Wobble = 0.01f };
            p.Shape(Sd.Circle(0, 0, 0.44f), Rgba.White);
            return new DoodleArt("d40_dot", p);
        }

        // ================= GARDEN SEASONS =================

        /// <summary>Orto in un prato (coordinate del livello): steccato, aiuola di terra al centro (0,-2.6), prato fino in basso.</summary>
        public static DoodleArt D40GardenBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 5f, 7.5f), (x, y) => Rgba.Lerp(Rgba.Hex("D8F0FF"), Rgba.Hex("7CC4F2"), Clamp((y + 0.5f) / 5.5f)));
            // colline lontane
            p.Fill(Sd.Ellipse(-2.8f, -0.9f, 3.4f, 1.25f), Rgba.Hex("A9DB8C"));
            p.Fill(Sd.Ellipse(2.9f, -1.0f, 3.6f, 1.4f), Rgba.Hex("97CF7B"));
            // steccato
            for (float x = -4.4f; x < 4.5f; x += 0.55f)
                p.Shape(Sd.Polygon(x - 0.17f, -1.0f, x + 0.17f, -1.0f, x + 0.17f, -0.25f, x, -0.08f, x - 0.17f, -0.25f), Rgba.Hex("FFF4DC"), 0.045f);
            p.Shape(Sd.Box(0, -0.45f, 5f, 0.07f), Rgba.Hex("F2E2C0"), 0.04f);
            // prato
            p.Shape(Sd.Box(0, -4.4f, 5.2f, 3.55f), Rgba.Hex("7ACB5E"));
            var rnd = new Random(40);
            for (int i = 0; i < 70; i++)
            {
                float x = (float)rnd.NextDouble() * 9f - 4.5f, y = -1.0f - (float)rnd.NextDouble() * 6f;
                p.Line(Rgba.Hex("5AAE45"), 0.03f, x, y, x + 0.04f, y + 0.12f);
            }
            // aiuola
            Sdf bed = Sd.Box(0, -2.6f, 2.75f, 1.05f, 0.35f);
            p.Shape(Sd.Grow(bed, 0.08f), Rgba.Hex("6E4A2E"), 0.04f);
            Body(p, bed, D27Dirt, D27DirtShade, null, 0.05f, -0.06f);
            for (int i = 0; i < 4; i++)
                p.Line(D27DirtShade, 0.04f, -2.4f, -1.95f - i * 0.42f, -0.8f, -1.9f - i * 0.42f, 0.8f, -1.97f - i * 0.42f, 2.4f, -1.93f - i * 0.42f);
            for (int i = 0; i < 25; i++)
            {
                float x = (float)rnd.NextDouble() * 5f - 2.5f, y = -1.75f - (float)rnd.NextDouble() * 1.7f;
                p.Fill(Sd.Circle(x, y, 0.03f + (float)rnd.NextDouble() * 0.03f), Rgba.Hex("B88A60"));
            }
            // fiorellini sul prato
            foreach (var f in new[] { new[] { -2.3f, -4.3f }, new[] { -0.9f, -4.6f }, new[] { 0.6f, -4.35f }, new[] { 2.4f, -4.55f }, new[] { -2.6f, -1.25f }, new[] { 2.65f, -1.3f } })
            {
                for (int k = 0; k < 5; k++)
                {
                    double a = k * Math.PI * 2 / 5;
                    p.Shape(Sd.Circle(f[0] + (float)Math.Cos(a) * 0.08f, f[1] + (float)Math.Sin(a) * 0.08f, 0.06f), Rgba.Hex("FFFFFF"), 0.02f);
                }
                p.Shape(Sd.Circle(f[0], f[1], 0.05f), Rgba.Hex("FFD24A"), 0.02f);
            }
            return new DoodleArt("gs_bg", p);
        }

        /// <summary>Seme di zucca con la faccina (centro 0,0, alto 0.66).</summary>
        public static DoodleArt D40Seed()
        {
            var p = new DoodlePainter(-0.3f, -0.36f, 0.3f, 0.4f, 160) { Ink = 0.04f };
            Sdf seed = Sd.Union(Sd.Ellipse(0, -0.05f, 0.22f, 0.27f), Sd.Polygon(-0.13f, 0.1f, 0.13f, 0.1f, 0f, 0.34f));
            Body(p, seed, Rgba.Hex("FFF1C4"), Rgba.Hex("E6CE8E"));
            p.Line(Rgba.Hex("E6CE8E"), 0.025f, -0.13f, -0.15f, -0.1f, 0.12f);
            V40D_Eye(p, -0.07f, 0.0f, 0.035f);
            V40D_Eye(p, 0.07f, 0.0f, 0.035f);
            p.InkLine(0.018f, -0.04f, -0.08f, 0f, -0.11f, 0.04f, -0.08f);
            p.Fill(Sd.Circle(-0.13f, -0.07f, 0.03f), BlushColor.WithAlpha(0.6f));
            p.Fill(Sd.Circle(0.13f, -0.07f, 0.03f), BlushColor.WithAlpha(0.6f));
            return new DoodleArt("gs_seed", p);
        }

        /// <summary>Bustina di semi con la zucca disegnata (centro 0,0, 0.96 x 1.36).</summary>
        public static DoodleArt D40SeedPacket()
        {
            var p = new DoodlePainter(-0.55f, -0.75f, 0.55f, 0.75f, 120) { Ink = 0.04f };
            Sdf bag = Sd.Polygon(-0.48f, -0.68f, 0.48f, -0.68f, 0.48f, 0.55f, 0.36f, 0.66f, 0.24f, 0.55f, 0.12f, 0.66f, 0f, 0.55f, -0.12f, 0.66f, -0.24f, 0.55f, -0.36f, 0.66f, -0.48f, 0.55f);
            Body(p, bag, Rgba.Hex("F7E3B5"), Rgba.Hex("DDBF85"));
            p.Shape(Sd.Box(0, 0.05f, 0.36f, 0.36f, 0.05f), Rgba.Hex("BFE6FF"), 0.03f);
            p.Fill(Sd.Box(0, -0.2f, 0.36f, 0.11f), Rgba.Hex("7ACB5E"));
            Body(p, Sd.Union(Sd.Ellipse(-0.1f, -0.05f, 0.13f, 0.15f), Sd.Ellipse(0.1f, -0.05f, 0.13f, 0.15f), Sd.Ellipse(0, -0.05f, 0.14f, 0.16f)), Rgba.Hex("FF9A2E"), Rgba.Hex("E06F12"));
            p.InkLine(0.03f, 0f, 0.1f, 0.03f, 0.2f);
            p.Shape(Sd.Box(0, -0.52f, 0.36f, 0.08f, 0.03f), Rgba.Hex("E8433F"), 0.025f);
            return new DoodleArt("gs_packet", p);
        }

        /// <summary>Buca appena scavata con mucchietto di terra (centro 0,0).</summary>
        public static DoodleArt D40DugHole()
        {
            var p = new DoodlePainter(-0.75f, -0.3f, 0.85f, 0.32f, 120) { Ink = 0.035f };
            Body(p, Sd.Ellipse(0.55f, 0.02f, 0.25f, 0.17f), Rgba.Hex("B88A60"), D27DirtShade);
            p.Shape(Sd.Ellipse(0, 0, 0.55f, 0.22f), Rgba.Hex("B88A60"));
            p.Shape(Sd.Ellipse(0, -0.01f, 0.4f, 0.14f), D27DirtDark, 0.03f);
            return new DoodleArt("gs_hole", p);
        }

        /// <summary>Monticello di terra chiara (base in y = -0.05; si scurisce col colore dello sprite quando è bagnato).</summary>
        public static DoodleArt D40SoilMound()
        {
            var p = new DoodlePainter(-0.65f, -0.15f, 0.65f, 0.38f, 120) { Ink = 0.035f };
            Sdf mound = Sd.Intersect(Sd.Ellipse(0, -0.05f, 0.56f, 0.36f), Sd.Box(0, 0.2f, 0.7f, 0.25f));
            Body(p, mound, Rgba.Hex("E2C29A"), Rgba.Hex("C9A275"), null, 0.05f, -0.05f);
            foreach (var s in new[] { new[] { -0.25f, 0.05f }, new[] { 0.15f, 0.15f }, new[] { 0.3f, 0.02f }, new[] { -0.05f, 0.22f } })
                p.Fill(Sd.Circle(s[0], s[1], 0.03f), Rgba.Hex("B08860"));
            return new DoodleArt("gs_mound", p);
        }

        /// <summary>Annaffiatoio col beccuccio a sinistra (perno nel manico 0,0; uscita dell'acqua in -0.95, 0.3).</summary>
        public static DoodleArt D40WateringCan()
        {
            var p = new DoodlePainter(-1.1f, -0.52f, 0.78f, 0.66f, 110) { Ink = 0.045f };
            Rgba blue = Rgba.Hex("5AA9E6"), blueShade = Rgba.Hex("3A84C4");
            p.Shape(Sd.Polygon(-0.2f, -0.32f, -0.2f, -0.02f, -0.86f, 0.33f, -0.92f, 0.24f), blue);
            p.Shape(Sd.Rotate(Sd.Ellipse(-0.93f, 0.3f, 0.07f, 0.13f), -0.93f, 0.3f, -30f), Rgba.Hex("2F6FAE"));
            p.Shape(Sd.Polyline(0.05f, -0.1f, 0.2f, 0.05f, 0.55f, 0.4f, 0.55f, 0.55f, 0.2f), blue);
            Sdf body = Sd.Box(0.18f, -0.12f, 0.42f, 0.34f, 0.1f);
            Body(p, body, blue, blueShade);
            p.Fill(Sd.Box(0.18f, -0.12f, 0.42f, 0.05f), Rgba.Hex("FFD24A").WithAlpha(0.9f));
            p.Shape(body, Rgba.Clear);
            p.Fill(Sd.Capsule(-0.05f, 0.05f, -0.05f, -0.3f, 0.03f), Rgba.White.WithAlpha(0.5f));
            return new DoodleArt("gs_can", p);
        }

        /// <summary>Germoglio con due foglioline (base 0,0, alto 0.6).</summary>
        public static DoodleArt D40Sprout()
        {
            var p = new DoodlePainter(-0.45f, -0.05f, 0.45f, 0.72f, 150) { Ink = 0.035f };
            p.Shape(Sd.Polyline(0.035f, 0f, 0f, 0.02f, 0.25f, 0f, 0.5f), Leaf);
            p.Shape(Sd.Rotate(Sd.Ellipse(-0.18f, 0.52f, 0.19f, 0.09f), -0.18f, 0.52f, 25f), Leaf);
            p.Shape(Sd.Rotate(Sd.Ellipse(0.18f, 0.55f, 0.19f, 0.09f), 0.18f, 0.55f, -25f), Leaf);
            p.Line(LeafShade, 0.02f, -0.3f, 0.47f, -0.05f, 0.52f);
            p.Line(LeafShade, 0.02f, 0.3f, 0.5f, 0.05f, 0.55f);
            return new DoodleArt("gs_sprout", p);
        }

        /// <summary>Pianta di zucca cresciuta con fiore giallo (base 0,0, alta 1.7).</summary>
        public static DoodleArt D40Plant()
        {
            var p = new DoodlePainter(-0.9f, -0.05f, 0.9f, 1.85f, 110) { Ink = 0.045f };
            p.Shape(Sd.Polyline(0.05f, 0f, 0f, 0.08f, 0.5f, -0.06f, 1.0f, 0.02f, 1.42f), Leaf);
            float[][] leaves = { new[] { -0.4f, 0.35f, 25f }, new[] { 0.42f, 0.5f, -25f }, new[] { -0.45f, 0.95f, 30f }, new[] { 0.42f, 1.1f, -30f }, new[] { -0.2f, 1.35f, 50f } };
            foreach (float[] l in leaves)
            {
                Sdf leaf = Sd.Rotate(Sd.Union(Sd.Ellipse(l[0], l[1], 0.36f, 0.2f), Sd.Circle(l[0] - Math.Sign(l[0]) * 0.1f, l[1] + 0.08f, 0.15f)), l[0], l[1], l[2]);
                Body(p, leaf, Leaf, LeafShade);
                p.Line(LeafShade, 0.025f, l[0] * 0.3f, l[1] - 0.03f, l[0] * 1.6f, l[1] + 0.05f);
            }
            for (int k = 0; k < 5; k++)
            {
                double a = Math.PI / 2 + k * Math.PI * 2 / 5;
                float px = 0.02f + (float)Math.Cos(a) * 0.17f, py = 1.55f + (float)Math.Sin(a) * 0.17f;
                p.Shape(Sd.Circle(px, py, 0.13f), Rgba.Hex("FFD24A"), 0.035f);
            }
            p.Shape(Sd.Circle(0.02f, 1.55f, 0.1f), Rgba.Hex("FF9A2E"), 0.03f);
            // viticcio
            p.Line(LeafShade, 0.025f, 0.08f, 0.75f, 0.3f, 0.78f, 0.35f, 0.68f, 0.27f, 0.65f);
            return new DoodleArt("gs_plant", p);
        }

        /// <summary>Ombrellone a spicchi (centro della cupola 0,0; il palo arriva a y = -1.15).</summary>
        public static DoodleArt D40Umbrella()
        {
            var p = new DoodlePainter(-0.95f, -1.25f, 0.95f, 0.62f, 110) { Ink = 0.045f };
            p.Shape(Sd.Polyline(0.045f, 0f, 0.4f, 0f, -1.05f, 0.1f, -1.15f, 0.2f, -1.08f), Rgba.Hex("D8C7A8"));
            Sdf canopy = Sd.Subtract(Sd.Intersect(Sd.Ellipse(0, -0.12f, 0.88f, 0.62f), Sd.Box(0, 0.25f, 0.9f, 0.38f)),
                Sd.Union(Sd.Circle(-0.66f, -0.17f, 0.16f), Sd.Circle(-0.22f, -0.17f, 0.16f), Sd.Circle(0.22f, -0.17f, 0.16f), Sd.Circle(0.66f, -0.17f, 0.16f)));
            p.FillFunc(canopy, (x, y) =>
            {
                double a = Math.Atan2(y + 0.4f, x);
                int band = (int)Math.Floor(a / (Math.PI / 8));
                return band % 2 == 0 ? Rgba.Hex("E8433F") : Rgba.Hex("FFF4DC");
            });
            p.Fill(Sd.Crescent(canopy, 0.06f, -0.08f), Rgba.Hex("000000", 0.12f));
            p.Shape(canopy, Rgba.Clear);
            p.Shape(Sd.Circle(0, 0.5f, 0.06f), Rgba.Hex("FFD24A"), 0.03f);
            return new DoodleArt("gs_umbrella", p);
        }

        /// <summary>Ombra ovale morbida (centro 0,0, 1.2 x 0.3).</summary>
        public static DoodleArt D40Shadow()
        {
            var p = new DoodlePainter(-0.7f, -0.2f, 0.7f, 0.2f, 80) { Wobble = 0f };
            p.FillFunc(Sd.Ellipse(0, 0, 0.62f, 0.16f), (x, y) => Rgba.Hex("1A1030", 0.38f * (1f - Clamp((x * x / 0.38f + y * y / 0.026f) - 0.3f))));
            return new DoodleArt("gs_shadow", p);
        }

        /// <summary>Zucca arancione (centro 0,0, base in y = -0.3).</summary>
        public static DoodleArt D40Pumpkin()
        {
            var p = new DoodlePainter(-0.5f, -0.38f, 0.5f, 0.48f, 140) { Ink = 0.045f };
            Rgba o = Rgba.Hex("FF9A2E"), os = Rgba.Hex("E06F12");
            p.Shape(Sd.Polyline(0.05f, 0f, 0.2f, 0.03f, 0.33f, 0.1f, 0.38f), Rgba.Hex("6E8A3A"));
            Body(p, Sd.Ellipse(-0.22f, -0.02f, 0.22f, 0.28f), o, os);
            Body(p, Sd.Ellipse(0.22f, -0.02f, 0.22f, 0.28f), o, os);
            Body(p, Sd.Ellipse(0, -0.02f, 0.2f, 0.3f), Rgba.Hex("FFAA45"), os);
            p.Fill(Sd.Ellipse(-0.06f, 0.1f, 0.04f, 0.1f), Rgba.White.WithAlpha(0.45f));
            p.Shape(Sd.Rotate(Sd.Ellipse(0.2f, 0.32f, 0.12f, 0.06f), 0.2f, 0.32f, -20f), Leaf, 0.03f);
            return new DoodleArt("gs_pumpkin", p);
        }

        /// <summary>Talpa con l'elmetto da minatore (base 0,0, alta 0.95).</summary>
        public static DoodleArt D40Mole()
        {
            var p = new DoodlePainter(-0.5f, -0.05f, 0.5f, 1.05f, 140) { Ink = 0.045f };
            Rgba fur = Rgba.Hex("6E5A6E"), furShade = Rgba.Hex("4E3F52");
            Sdf body = Sd.Union(Sd.Ellipse(0, 0.4f, 0.36f, 0.45f), Sd.Box(0, 0.1f, 0.36f, 0.12f));
            Body(p, body, fur, furShade);
            p.Fill(Sd.Ellipse(0, 0.28f, 0.2f, 0.22f), Rgba.Hex("9C8AA0"));
            // elmetto con lampada
            p.Shape(Sd.Intersect(Sd.Circle(0, 0.62f, 0.33f), Sd.Box(0, 0.85f, 0.4f, 0.22f)), Rgba.Hex("FFD24A"));
            p.Shape(Sd.Box(0, 0.65f, 0.38f, 0.04f, 0.02f), Rgba.Hex("E5A91B"), 0.03f);
            p.Shape(Sd.Circle(0, 0.86f, 0.08f), Rgba.Hex("FFF7C2"), 0.03f);
            // occhietti strizzati, naso rosa, dentoni
            p.InkLine(0.03f, -0.17f, 0.52f, -0.1f, 0.55f);
            p.InkLine(0.03f, 0.1f, 0.55f, 0.17f, 0.52f);
            p.Shape(Sd.Box(0, 0.36f, 0.04f, 0.05f), Rgba.White, 0.02f);
            p.Shape(Sd.Ellipse(0, 0.45f, 0.11f, 0.08f), Rgba.Hex("FF8FB0"), 0.03f);
            // zampone
            foreach (int s in new[] { -1, 1 })
            {
                p.Shape(Sd.Ellipse(s * 0.33f, 0.2f, 0.13f, 0.1f), Rgba.Hex("FFB3C6"), 0.03f);
                for (int k = -1; k <= 1; k++) p.InkLine(0.015f, s * 0.33f + k * 0.05f, 0.14f, s * 0.33f + k * 0.06f, 0.1f);
            }
            return new DoodleArt("gs_mole", p);
        }

        /// <summary>Tana della talpa: anello di terra con buco scuro (centro 0,0).</summary>
        public static DoodleArt D40MoleHole()
        {
            var p = new DoodlePainter(-0.6f, -0.28f, 0.6f, 0.28f, 120) { Ink = 0.035f };
            p.Shape(Sd.Ellipse(0, 0, 0.52f, 0.22f), Rgba.Hex("7A5030"));
            p.Shape(Sd.Ellipse(0, 0.02f, 0.38f, 0.13f), D27DirtDark, 0.03f);
            return new DoodleArt("gs_molehole", p);
        }

        /// <summary>Serra di vetro (base 0,0, larga 2.6; gronde in y = 1.5, colmo in 0, 2.35).</summary>
        public static DoodleArt D40Greenhouse()
        {
            var p = new DoodlePainter(-1.5f, -0.1f, 1.5f, 2.5f, 90) { Ink = 0.045f };
            Sdf glass = Sd.Polygon(-1.3f, 0.3f, 1.3f, 0.3f, 1.3f, 1.5f, 0f, 2.35f, -1.3f, 1.5f);
            p.FillFunc(glass, (x, y) => Rgba.Hex("D8F4FF", 0.25f + 0.15f * Clamp((y - 0.3f) / 2f)));
            // riflessi
            p.Line(Rgba.White.WithAlpha(0.6f), 0.05f, -1.0f, 0.6f, -0.6f, 1.2f);
            p.Line(Rgba.White.WithAlpha(0.6f), 0.04f, 0.5f, 0.6f, 0.8f, 1.05f);
            Rgba frame = Rgba.Hex("F4F4F7");
            Sdf bars = Sd.Union(
                Sd.Polyline(0.045f, -1.3f, 0.3f, -1.3f, 1.5f, 0f, 2.35f, 1.3f, 1.5f, 1.3f, 0.3f),
                Sd.Polyline(0.035f, -1.3f, 1.5f, 1.3f, 1.5f),
                Sd.Polyline(0.03f, -0.65f, 0.3f, -0.65f, 1.92f), Sd.Polyline(0.03f, 0f, 0.3f, 0f, 2.35f), Sd.Polyline(0.03f, 0.65f, 0.3f, 0.65f, 1.92f),
                Sd.Polyline(0.03f, -1.3f, 0.9f, 1.3f, 0.9f));
            p.Shape(bars, frame, 0.03f);
            // muretto di mattoni
            Sdf wall = Sd.Box(0, 0.15f, 1.4f, 0.16f, 0.03f);
            Body(p, wall, Rgba.Hex("C9654A"), Rgba.Hex("A34A35"));
            for (float x = -1.2f; x < 1.4f; x += 0.4f) p.Line(D27Ink.WithAlpha(0.5f), 0.02f, x, 0.0f, x, 0.3f);
            p.Line(D27Ink.WithAlpha(0.5f), 0.02f, -1.38f, 0.15f, 1.38f, 0.15f);
            return new DoodleArt("gs_greenhouse", p);
        }

        /// <summary>Mucchietto di neve (centro 0,0).</summary>
        public static DoodleArt D40SnowLump()
        {
            var p = new DoodlePainter(-0.32f, -0.15f, 0.32f, 0.24f, 130) { Ink = 0.03f };
            Sdf s = Sd.Union(Sd.Circle(-0.14f, 0f, 0.12f), Sd.Circle(0.02f, 0.06f, 0.15f), Sd.Circle(0.17f, -0.01f, 0.11f), Sd.Box(0, -0.06f, 0.26f, 0.06f, 0.05f));
            Body(p, s, Snow, SnowShade, null, 0.04f, -0.05f);
            return new DoodleArt("gs_snow_lump", p);
        }

        /// <summary>Fiocco di neve (centro 0,0).</summary>
        public static DoodleArt D40Flake()
        {
            var p = new DoodlePainter(-0.13f, -0.13f, 0.13f, 0.13f, 200) { Wobble = 0f };
            for (int k = 0; k < 3; k++)
            {
                double a = k * Math.PI / 3;
                float c = (float)Math.Cos(a) * 0.1f, s = (float)Math.Sin(a) * 0.1f;
                p.Line(Rgba.Hex("9CC3EA"), 0.04f, -c, -s, c, s);
                p.Line(Rgba.White, 0.022f, -c, -s, c, s);
            }
            return new DoodleArt("gs_flake", p);
        }

        /// <summary>Manto di neve sul prato (coordinate del livello, bordo ondulato verso y = -0.85).</summary>
        public static DoodleArt D40SnowGround()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, -0.5f, 40) { Ink = 0.05f };
            Sdf top = (x, y) => y - (-0.85f + 0.07f * (float)Math.Sin(x * 2.3f) + 0.04f * (float)Math.Sin(x * 5.1f + 1f));
            p.Shape(top, Snow);
            p.Hatch(Sd.Intersect(top, Sd.Box(0, -4f, 5f, 3f)), SnowShade.WithAlpha(0.6f), 0.35f, 0.02f, 160f);
            for (float x = -4f; x < 4.5f; x += 1.1f) p.Fill(Sd.Ellipse(x, -1.15f, 0.3f, 0.06f), SnowShade.WithAlpha(0.7f));
            return new DoodleArt("gs_snow_ground", p);
        }

        // ================= MUSEUM NIGHT =================

        /// <summary>Sala del museo di notte (coordinate del livello): pavimento sotto y = -2.4, uscita a sinistra (x = -2.35), luce del quadro in 1.75.</summary>
        public static DoodleArt D40MuseumBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 2.5f, 5f, 5f), (x, y) => Rgba.Lerp(Rgba.Hex("232650"), Rgba.Hex("363A72"), Clamp((y + 2.4f) / 7f)));
            // lucernario con la luna e raggio di luce lunare
            Sdf win = Sd.Union(Sd.Box(-0.5f, 2.35f, 0.75f, 0.55f), Sd.Circle(-0.5f, 2.9f, 0.75f));
            p.Shape(Sd.Grow(win, 0.1f), Rgba.Hex("4A4F8C"));
            p.FillFunc(win, (x, y) => Rgba.Lerp(Rgba.Hex("0C1130"), Rgba.Hex("24306B"), Clamp((y - 1.8f) / 2f)));
            p.Shape(Sd.Subtract(Sd.Circle(-0.2f, 3.0f, 0.28f), Sd.Circle(-0.08f, 3.1f, 0.24f)), Rgba.Hex("FFE680"), 0.03f);
            p.InkLine(0.05f, -0.5f, 1.8f, -0.5f, 3.65f);
            p.InkLine(0.05f, -1.25f, 2.55f, 0.25f, 2.55f);
            p.Shape(win, Rgba.Clear, 0.05f);
            p.Fill(Sd.Polygon(-1.2f, 1.8f, 0.2f, 1.8f, 1.0f, -2.4f, -0.9f, -2.4f), Rgba.Hex("B9C8FF", 0.07f));
            // cornici di decorazione
            p.Shape(Sd.Box(-1.05f, 0.65f, 0.42f, 0.36f, 0.03f), Gold);
            p.Fill(Sd.Box(-1.05f, 0.65f, 0.32f, 0.26f), Rgba.Hex("2E6B8F"));
            p.Shape(Sd.Circle(-1.1f, 0.62f, 0.13f), Rgba.Hex("E8433F"), 0.025f);
            p.Shape(Sd.Box(-0.92f, 0.72f, 0.07f, 0.12f), Rgba.Hex("FFD24A"), 0.025f);
            p.Shape(Sd.Box(0.25f, 0.4f, 0.3f, 0.4f, 0.03f), Gold);
            p.Fill(Sd.Box(0.25f, 0.4f, 0.21f, 0.31f), Rgba.Hex("6A3E7A"));
            p.Line(Rgba.Hex("FFD24A"), 0.04f, 0.1f, 0.2f, 0.25f, 0.6f, 0.4f, 0.2f);
            // lampada del quadro principale
            p.Shape(Sd.Box(1.75f, 2.12f, 0.35f, 0.06f, 0.03f), Gold, 0.035f);
            p.FillFunc(Sd.Polygon(1.45f, 2.06f, 2.05f, 2.06f, 2.6f, -0.6f, 0.9f, -0.6f), (x, y) => Rgba.Hex("FFE9A8", 0.18f * Clamp((y + 0.6f) / 2.6f)));
            // zoccolo
            p.Fill(Sd.Box(0, -1.85f, 5f, 0.55f), Rgba.Hex("1C1E40"));
            for (float x = -4.2f; x < 4.5f; x += 1.2f) p.Shape(Sd.Box(x, -1.85f, 0.5f, 0.38f, 0.04f), Rgba.Hex("232650"), 0.03f);
            p.Line(D27Ink, 0.05f, -4.5f, -1.3f, 4.5f, -1.3f);
            // uscita a sinistra con l'insegna verde
            Sdf door = Sd.Box(-2.35f, -1.4f, 0.48f, 1.0f, 0.05f);
            p.Shape(Sd.Grow(door, 0.1f), Rgba.Hex("5A4A6A"));
            p.FillFunc(door, (x, y) => Rgba.Lerp(Rgba.Hex("0A0A18"), Rgba.Hex("1A1A2E"), Clamp((y + 2.4f) / 2f)));
            p.Shape(Sd.Box(-2.35f, -0.05f, 0.42f, 0.16f, 0.04f), Rgba.Hex("2FD27A"), 0.035f);
            p.Fill(Sd.Box(-2.35f, -0.05f, 0.7f, 0.4f, 0.3f), Rgba.Hex("2FD27A", 0.12f));
            // pavimento a scacchi di marmo
            p.FillFunc(Sd.Box(0, -4.8f, 5f, 2.4f), (x, y) =>
            {
                int cx = (int)Math.Floor((x + 10f) / 0.6f), cy = (int)Math.Floor((y + 10f) / 0.6f);
                return (cx + cy) % 2 == 0 ? Rgba.Hex("2A2440") : Rgba.Hex("3A3258");
            });
            p.Shape(Sd.Box(0, -2.4f, 5f, 0.06f), Rgba.Hex("4A4270"));
            return new DoodleArt("mu_bg", p);
        }

        /// <summary>Guardiano di profilo verso destra con la torcia (piedi in 0,0; torcia in 0.8, 1.12).</summary>
        public static DoodleArt D40Guard()
        {
            var p = new DoodlePainter(-0.55f, -0.05f, 0.95f, 2.2f, 110) { Ink = 0.045f };
            Rgba navy = Rgba.Hex("2E3E7A"), navyShade = Rgba.Hex("1F2A58");
            // gambe e scarpe
            p.Shape(Sd.Capsule(-0.1f, 0.12f, -0.08f, 0.8f, 0.1f), navyShade);
            p.Shape(Sd.Capsule(0.1f, 0.12f, 0.08f, 0.8f, 0.1f), navy);
            p.Shape(Sd.Box(-0.06f, 0.07f, 0.16f, 0.07f, 0.05f), Rgba.Hex("1E1A24"));
            p.Shape(Sd.Box(0.14f, 0.07f, 0.16f, 0.07f, 0.05f), Rgba.Hex("1E1A24"));
            // corpo (con pancetta)
            Sdf body = Sd.Union(Sd.Box(0, 1.15f, 0.3f, 0.42f, 0.15f), Sd.Ellipse(0.1f, 1.0f, 0.3f, 0.3f));
            Body(p, body, navy, navyShade);
            p.Fill(Sd.Box(0.02f, 0.8f, 0.31f, 0.05f), Rgba.Hex("1E1A24"));
            p.Shape(Sd.Box(0.16f, 0.8f, 0.06f, 0.05f), Gold, 0.02f);
            p.Shape(Sd.Polygon(0.08f, 1.42f, 0.12f, 1.33f, 0.21f, 1.33f, 0.14f, 1.27f, 0.17f, 1.18f, 0.08f, 1.24f, -0.01f, 1.18f, 0.02f, 1.27f, -0.05f, 1.33f, 0.04f, 1.33f), Gold, 0.02f);
            // testa con berretto
            p.Shape(Sd.Circle(0.08f, 1.8f, 0.26f), Skin);
            p.Fill(Sd.Crescent(Sd.Circle(0.08f, 1.8f, 0.26f), 0.05f, -0.05f), SkinShade);
            p.Shape(Sd.Circle(0.08f, 1.8f, 0.26f), Rgba.Clear);
            p.Shape(Sd.Circle(0.33f, 1.78f, 0.06f), Skin, 0.03f);
            p.Shape(Sd.Union(Sd.Box(0.06f, 2.02f, 0.27f, 0.1f, 0.06f), Sd.Polygon(0.2f, 1.94f, 0.48f, 1.92f, 0.45f, 1.98f, 0.2f, 2.0f)), navy);
            p.Shape(Sd.Box(0.06f, 2.05f, 0.06f, 0.04f), Gold, 0.02f);
            V40D_Eye(p, 0.2f, 1.84f, 0.035f);
            p.Shape(Sd.Union(Sd.Ellipse(0.24f, 1.68f, 0.14f, 0.05f), Sd.Ellipse(0.12f, 1.66f, 0.08f, 0.04f)), Rgba.Hex("6B4A3A"), 0.025f);
            // braccio teso con la torcia
            p.Shape(Sd.Capsule(0.05f, 1.35f, 0.45f, 1.15f, 0.08f), navy);
            p.Shape(Sd.Circle(0.48f, 1.13f, 0.08f), Skin, 0.03f);
            p.Shape(Sd.Box(0.62f, 1.12f, 0.16f, 0.06f, 0.03f), Rgba.Hex("4A4655"), 0.03f);
            p.Shape(Sd.Box(0.79f, 1.12f, 0.04f, 0.09f, 0.02f), Rgba.Hex("FFF1B0"), 0.03f);
            return new DoodleArt("mu_guard", p);
        }

        /// <summary>Cono di luce della torcia verso destra e in basso (vertice 0,0; raggio 2.7, tra -5° e -39°).</summary>
        public static DoodleArt D40Cone()
        {
            var p = new DoodlePainter(0f, -1.8f, 2.8f, 0.1f, 50) { Wobble = 0f };
            const float L = 2.7f;
            double a1 = -5 * Math.PI / 180, a2 = -39 * Math.PI / 180;
            Sdf cone = Sd.Polygon(0f, 0f, L * (float)Math.Cos(a1), L * (float)Math.Sin(a1), L * (float)Math.Cos(a2), L * (float)Math.Sin(a2));
            Sdf disk = Sd.Intersect(Sd.Circle(0, 0, L), Sd.Grow(cone, 0.3f));
            Sdf shape = Sd.Intersect(disk, (x, y) =>
            {
                double a = Math.Atan2(y, x);
                return (float)Math.Max(a - a1, a2 - a) * 1.5f;
            });
            p.FillFunc(shape, (x, y) =>
            {
                float d = (float)Math.Sqrt(x * x + y * y) / L;
                return Rgba.Hex("FFF3B0", 0.55f * (1f - d * 0.55f));
            });
            return new DoodleArt("mu_cone", p);
        }

        /// <summary>Gatto ladro acquattato di profilo verso destra, mascherina e maglia a righe (piedi in 0,0, alto 1.1).</summary>
        public static DoodleArt D40Burglar()
        {
            var p = new DoodlePainter(-0.72f, -0.05f, 0.72f, 1.2f, 130) { Ink = 0.045f };
            Rgba fur = Rgba.Hex("8A8798"), furShade = Rgba.Hex("6A6778");
            // coda
            p.Shape(Sd.Polyline(0.07f, -0.35f, 0.45f, -0.55f, 0.6f, -0.6f, 0.9f, -0.5f, 1.05f), fur);
            // zampe
            foreach (float x in new[] { -0.25f, -0.1f, 0.18f, 0.32f }) p.Shape(Sd.Capsule(x, 0.08f, x + 0.02f, 0.3f, 0.065f), x < 0f ? furShade : fur);
            // corpo con maglia a righe
            Sdf body = Sd.Ellipse(0, 0.42f, 0.42f, 0.25f);
            p.FillFunc(body, (x, y) => ((int)Math.Floor((y + 2f) / 0.1f)) % 2 == 0 ? Rgba.Hex("1E1A24") : Rgba.Hex("F4F4F7"));
            p.Fill(Sd.Crescent(body, -0.05f, 0.06f), Rgba.Hex("000000", 0.15f));
            p.Shape(body, Rgba.Clear);
            // sacco sulla schiena
            Body(p, Sd.Ellipse(-0.18f, 0.72f, 0.24f, 0.2f), Rgba.Hex("C9A36A"), Rgba.Hex("A3824E"));
            p.InkLine(0.03f, -0.02f, 0.8f, 0.05f, 0.88f);
            // testa
            Sdf earL = Sd.Polygon(0.25f, 0.85f, 0.3f, 1.12f, 0.43f, 0.92f);
            Sdf earR = Sd.Polygon(0.47f, 0.92f, 0.6f, 1.1f, 0.63f, 0.84f);
            p.Shape(Sd.Union(earL, earR), fur);
            p.Fill(Sd.Grow(Sd.Union(earL, earR), -0.05f), CatPink);
            Sdf head = Sd.Ellipse(0.45f, 0.72f, 0.24f, 0.21f);
            Body(p, head, fur, furShade);
            // mascherina nera
            p.Shape(Sd.Box(0.47f, 0.76f, 0.24f, 0.06f, 0.05f), Rgba.Hex("1E1A24"), 0.02f);
            p.Fill(Sd.Circle(0.56f, 0.76f, 0.035f), Rgba.White);
            p.Fill(Sd.Circle(0.57f, 0.76f, 0.018f), D27Ink);
            p.Fill(Sd.Circle(0.4f, 0.76f, 0.03f), Rgba.White);
            p.Fill(Sd.Circle(0.41f, 0.76f, 0.015f), D27Ink);
            p.Shape(Sd.Circle(0.68f, 0.68f, 0.03f), CatPink, 0.015f);
            p.InkLine(0.015f, 0.6f, 0.63f, 0.66f, 0.6f);
            p.Line(D27Ink, 0.012f, 0.62f, 0.66f, 0.78f, 0.7f);
            p.Line(D27Ink, 0.012f, 0.62f, 0.64f, 0.78f, 0.61f);
            return new DoodleArt("mu_burglar", p);
        }

        /// <summary>Busto di marmo su un piedistallo alto (base 0,0; il piedistallo è alto 1.25).</summary>
        public static DoodleArt D40Statue()
        {
            var p = new DoodlePainter(-0.62f, -0.05f, 0.62f, 2.3f, 100) { Ink = 0.045f };
            Rgba marble = Rgba.Hex("E4DFF0"), marbleShade = Rgba.Hex("B9B2D0");
            Body(p, Sd.Box(0, 0.08f, 0.55f, 0.08f, 0.02f), marble, marbleShade);
            Body(p, Sd.Box(0, 0.65f, 0.45f, 0.5f, 0.02f), marble, marbleShade, null, 0.06f, -0.04f);
            p.Line(marbleShade, 0.02f, -0.2f, 0.3f, -0.05f, 0.6f, -0.15f, 0.9f);
            p.Line(marbleShade, 0.02f, 0.2f, 0.95f, 0.28f, 0.6f);
            Body(p, Sd.Box(0, 1.2f, 0.55f, 0.07f, 0.02f), marble, marbleShade);
            // busto
            Body(p, Sd.Intersect(Sd.Ellipse(0, 1.28f, 0.42f, 0.38f), Sd.Box(0, 1.5f, 0.5f, 0.22f)), marble, marbleShade);
            p.Shape(Sd.Box(0, 1.72f, 0.09f, 0.08f), marble, 0.03f);
            Body(p, Sd.Ellipse(0, 1.97f, 0.22f, 0.26f), marble, marbleShade);
            p.Shape(Sd.Polygon(0.2f, 1.98f, 0.3f, 1.9f, 0.2f, 1.86f), marble, 0.03f);
            p.InkLine(0.02f, 0.06f, 2.0f, 0.13f, 2.0f);
            p.InkLine(0.02f, 0.07f, 1.83f, 0.14f, 1.85f);
            // corona d'alloro
            for (int k = 0; k < 6; k++)
            {
                float x = -0.2f + k * 0.08f, y = 2.17f + (float)Math.Sin(k * 0.6f) * 0.03f;
                p.Shape(Sd.Rotate(Sd.Ellipse(x, y, 0.07f, 0.035f), x, y, 30f - k * 12f), Rgba.Hex("D7C878"), 0.02f);
            }
            return new DoodleArt("mu_statue", p);
        }

        /// <summary>Il falso: un disegno a pastelli del ritratto del gatto in una cornice di cartone (stesse misure di "portrait").</summary>
        public static DoodleArt D40FakePainting()
        {
            var p = new DoodlePainter(-1.0f, -1.15f, 1.0f, 1.4f, 110) { Ink = 0.05f };
            p.InkLine(0.025f, -0.55f, 1.0f, 0f, 1.35f, 0.55f, 1.0f);
            p.Shape(Sd.Circle(0, 1.35f, 0.045f), Rgba.Hex("B7BCC9"), 0.02f);
            Body(p, Sd.Box(0, 0, 0.95f, 1.1f, 0.02f), Rgba.Hex("C9A36A"), Rgba.Hex("A3824E"));
            p.Line(Rgba.Hex("A3824E"), 0.02f, -0.85f, 0.95f, -0.4f, 1.0f);
            p.Shape(Sd.Box(0, 0, 0.78f, 0.92f), Rgba.Hex("FFFDF5"), 0.03f);
            // pastelli: verde, viola, gatto storto
            p.Hatch(Sd.Box(0, 0.35f, 0.74f, 0.53f), Rgba.Hex("7ACB5E", 0.7f), 0.08f, 0.03f, 20f);
            p.Hatch(Sd.Ellipse(0, -0.68f, 0.6f, 0.3f), Rgba.Hex("7C63D9", 0.75f), 0.07f, 0.03f, -30f);
            p.Line(Rgba.Hex("FF8A1F"), 0.04f, -0.32f, 0.15f, -0.38f, 0.55f, -0.15f, 0.38f, 0.12f, 0.42f, 0.35f, 0.6f, 0.32f, 0.12f, 0.2f, -0.2f, -0.2f, -0.22f, -0.35f, -0.05f, -0.32f, 0.15f);
            p.Fill(Sd.Circle(-0.12f, 0.15f, 0.05f), Rgba.Hex("2FA84F"));
            p.Fill(Sd.Circle(0.14f, 0.18f, 0.06f), Rgba.Hex("2FA84F"));
            p.Line(Rgba.Hex("E8433F"), 0.03f, -0.08f, -0.05f, 0.02f, -0.1f, 0.12f, -0.04f);
            p.Line(D27Ink, 0.02f, -0.45f, 0.0f, -0.2f, 0.02f);
            p.Line(D27Ink, 0.02f, 0.25f, 0.05f, 0.5f, 0.0f);
            // sole a pastello e firma
            p.Fill(Sd.Circle(0.55f, 0.72f, 0.12f), Rgba.Hex("FFD24A"));
            p.Line(D27Ink.WithAlpha(0.7f), 0.02f, 0.3f, -0.8f, 0.4f, -0.72f, 0.48f, -0.8f, 0.58f, -0.72f);
            // nastro adesivo agli angoli
            foreach (int sx in new[] { -1, 1 }) foreach (int sy in new[] { -1, 1 })
                p.Fill(Sd.Rotate(Sd.Box(sx * 0.75f, sy * 0.88f, 0.14f, 0.05f), sx * 0.75f, sy * 0.88f, sx * sy * 40f), Rgba.Hex("F2E9C8", 0.9f));
            return new DoodleArt("mu_fake", p);
        }

        /// <summary>Sacco del bottino col dollaro (centro 0,0).</summary>
        public static DoodleArt D40LootBag()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.5f, 0.58f, 130) { Ink = 0.045f };
            Sdf sack = Sd.Union(Sd.Ellipse(0, -0.12f, 0.42f, 0.36f), Sd.Polygon(-0.15f, 0.15f, 0.15f, 0.15f, 0.22f, 0.42f, -0.22f, 0.42f));
            Body(p, sack, Rgba.Hex("C9A36A"), Rgba.Hex("A3824E"));
            p.Shape(Sd.Box(0, 0.24f, 0.17f, 0.04f, 0.02f), Rgba.Hex("8A5A3B"), 0.025f);
            p.Line(D27Ink, 0.06f, 0.08f, -0.0f, -0.06f, -0.0f, -0.08f, -0.1f, 0.08f, -0.14f, 0.07f, -0.26f, -0.08f, -0.26f);
            p.Line(D27Ink, 0.04f, 0f, 0.06f, 0f, -0.32f);
            return new DoodleArt("mu_bag", p);
        }

        // ================= ICE CREAM SHOP =================

        /// <summary>Gelateria (coordinate del livello): parete a righe sopra y = 0.7, piano di lavoro a piastrelle sotto.</summary>
        public static DoodleArt D40ShopBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 3.5f, 5f, 3.6f), (x, y) => ((int)Math.Floor((x + 10f) / 0.45f)) % 2 == 0 ? Rgba.Hex("FFD1DC") : Rgba.Hex("FFE6EC"));
            // tendina con festoni
            for (float x = -4.4f; x < 4.5f; x += 0.7f)
                p.Shape(Sd.Intersect(Sd.Circle(x + 0.35f, 4.0f, 0.36f), Sd.Box(x + 0.35f, 3.8f, 0.4f, 0.2f)), ((int)((x + 4.4f) / 0.7f)) % 2 == 0 ? Rgba.Hex("6FC3B2") : Rgba.White, 0.04f);
            // lavagnetta dei gusti
            p.Shape(Sd.Box(-1.5f, 2.6f, 0.8f, 0.55f, 0.05f), Rgba.Hex("3C4A46"));
            Rgba[] tints = { Rgba.Hex("FF9BB5"), Rgba.Hex("8A5A3B"), Rgba.Hex("FFF1C9"), Rgba.Hex("9BE3B5") };
            for (int i = 0; i < 4; i++)
            {
                p.Shape(Sd.Circle(-2.05f + i * 0.37f, 2.75f, 0.12f), tints[i], 0.025f);
                p.Line(Rgba.White.WithAlpha(0.7f), 0.03f, -2.15f + i * 0.37f, 2.45f, -1.95f + i * 0.37f, 2.45f);
            }
            // piano di lavoro a piastrelle
            p.FillFunc(Sd.Box(0, -3.6f, 5f, 3.6f), (x, y) =>
            {
                float fx = (x + 10f) / 0.5f, fy = (y + 10f) / 0.5f;
                float gx = Math.Abs(fx - (float)Math.Round(fx)), gy = Math.Abs(fy - (float)Math.Round(fy));
                return gx < 0.05f || gy < 0.05f ? Rgba.Hex("A9D9CB") : Rgba.Hex("D6F2E9");
            });
            // vassoio d'acciaio dei gusti
            p.Shape(Sd.Box(0, -3.55f, 2.85f, 1.25f, 0.15f), Rgba.Hex("C9D1DA"));
            p.Line(Rgba.White.WithAlpha(0.7f), 0.05f, -2.6f, -2.45f, 2.6f, -2.45f);
            return new DoodleArt("ic_bg", p);
        }

        /// <summary>Bancone davanti ai clienti (coordinate del livello, piano in y = 0.72).</summary>
        public static DoodleArt D40Counter()
        {
            var p = new DoodlePainter(-4.5f, -0.35f, 4.5f, 0.85f, 50) { Ink = 0.06f };
            p.Shape(Sd.Box(0, 0.15f, 5f, 0.45f), Rgba.Hex("FF8FAE"));
            for (float x = -4.2f; x < 4.5f; x += 0.6f) p.Fill(Sd.Circle(x, 0.1f, 0.08f), Rgba.White.WithAlpha(0.6f));
            Body(p, Sd.Box(0, 0.66f, 5f, 0.1f, 0.03f), Rgba.Hex("F4F4F7"), Rgba.Hex("C9CAD3"));
            return new DoodleArt("ic_counter", p);
        }

        /// <summary>Cono di cialda (bordo superiore in 0,0, punta in y = -1.0).</summary>
        public static DoodleArt D40Cone2()
        {
            var p = new DoodlePainter(-0.45f, -1.08f, 0.45f, 0.1f, 140) { Ink = 0.045f };
            Sdf cone = Sd.Polygon(-0.36f, 0f, 0.36f, 0f, 0f, -1.0f);
            p.Fill(cone, Rgba.Hex("E9B36A"));
            p.Hatch(cone, Rgba.Hex("C98A3E"), 0.13f, 0.025f, 60f);
            p.Hatch(cone, Rgba.Hex("C98A3E"), 0.13f, 0.025f, -60f);
            p.Shape(cone, Rgba.Clear);
            p.Shape(Sd.Box(0, -0.02f, 0.38f, 0.06f, 0.03f), Rgba.Hex("F2C47E"));
            return new DoodleArt("ic_cone", p);
        }

        /// <summary>Pallina di gelato bianca con le gocce (si tinge col colore dello sprite; centro 0,0).</summary>
        public static DoodleArt D40Scoop()
        {
            var p = new DoodlePainter(-0.5f, -0.32f, 0.5f, 0.35f, 140) { Ink = 0.045f };
            Sdf scoop = Sd.Union(Sd.Ellipse(0, 0.05f, 0.42f, 0.26f), Sd.Circle(-0.3f, -0.12f, 0.1f), Sd.Circle(-0.1f, -0.15f, 0.11f), Sd.Circle(0.12f, -0.13f, 0.1f), Sd.Circle(0.31f, -0.14f, 0.09f), Sd.Capsule(0.12f, -0.1f, 0.12f, -0.24f, 0.05f));
            Body(p, scoop, Rgba.Hex("FFFFFF"), Rgba.Hex("D4D4DE"), null, 0.06f, -0.06f);
            p.Fill(Sd.Ellipse(-0.18f, 0.16f, 0.1f, 0.05f), Rgba.White.WithAlpha(0.9f));
            return new DoodleArt("ic_scoop", p);
        }

        /// <summary>Codette colorate da mettere sopra una pallina (stesse misure di ic_scoop).</summary>
        public static DoodleArt D40Sprinkles()
        {
            var p = new DoodlePainter(-0.5f, -0.32f, 0.5f, 0.35f, 140) { Wobble = 0f };
            Rgba[] cols = { Rgba.Hex("E8433F"), Rgba.Hex("FFD24A"), Rgba.Hex("3E9BDB"), Rgba.Hex("7C63D9"), Rgba.Hex("2FD27A") };
            var rnd = new Random(12);
            for (int i = 0; i < 22; i++)
            {
                float x = (float)rnd.NextDouble() * 0.66f - 0.33f, y = -0.02f + (float)rnd.NextDouble() * 0.22f;
                if (x * x / 0.13f + (y - 0.05f) * (y - 0.05f) / 0.045f > 1f) continue;
                double a = rnd.NextDouble() * Math.PI;
                float dx = (float)Math.Cos(a) * 0.035f, dy = (float)Math.Sin(a) * 0.035f;
                p.Fill(Sd.Capsule(x - dx, y - dy, x + dx, y + dy, 0.018f), cols[i % cols.Length]);
            }
            return new DoodleArt("ic_sprinkles", p);
        }

        /// <summary>Vaschetta d'acciaio (centro 0,0, il gelato sopra in y ≈ 0.15).</summary>
        public static DoodleArt D40Tub()
        {
            var p = new DoodlePainter(-0.62f, -0.38f, 0.62f, 0.25f, 120) { Ink = 0.045f };
            Sdf tub = Sd.Polygon(-0.55f, 0.15f, 0.55f, 0.15f, 0.47f, -0.32f, -0.47f, -0.32f);
            Body(p, tub, Rgba.Hex("D5DCE4"), Rgba.Hex("A9B3BF"), null, 0.06f, -0.05f);
            p.Shape(Sd.Box(0, 0.16f, 0.6f, 0.05f, 0.02f), Rgba.Hex("EEF2F6"));
            p.Line(Rgba.White.WithAlpha(0.8f), 0.04f, -0.38f, 0.0f, -0.33f, -0.22f);
            return new DoodleArt("ic_tub", p);
        }

        /// <summary>Montagnola di gelato nella vaschetta (bianca, si tinge; base 0,0).</summary>
        public static DoodleArt D40TubCream()
        {
            var p = new DoodlePainter(-0.58f, -0.05f, 0.58f, 0.42f, 120) { Ink = 0.04f };
            Sdf cream = Sd.Intersect(Sd.Union(Sd.Circle(-0.3f, 0.05f, 0.24f), Sd.Circle(0.02f, 0.12f, 0.28f), Sd.Circle(0.32f, 0.04f, 0.22f)), Sd.Box(0, 0.25f, 0.55f, 0.25f));
            Body(p, cream, Rgba.Hex("FFFFFF"), Rgba.Hex("D4D4DE"), null, 0.05f, -0.05f);
            p.Line(Rgba.Hex("D4D4DE"), 0.025f, -0.15f, 0.2f, 0.05f, 0.25f, 0.2f, 0.18f);
            return new DoodleArt("ic_tub_cream", p);
        }

        /// <summary>Barattolo spargi-codette (centro 0,0).</summary>
        public static DoodleArt D40Shaker()
        {
            var p = new DoodlePainter(-0.36f, -0.45f, 0.36f, 0.5f, 130) { Ink = 0.04f };
            Sdf jar = Sd.Box(0, -0.1f, 0.28f, 0.32f, 0.08f);
            p.Shape(jar, Rgba.Hex("F4FBFF"));
            Rgba[] cols = { Rgba.Hex("E8433F"), Rgba.Hex("FFD24A"), Rgba.Hex("3E9BDB"), Rgba.Hex("7C63D9"), Rgba.Hex("2FD27A") };
            var rnd = new Random(5);
            for (int i = 0; i < 26; i++)
            {
                float x = (float)rnd.NextDouble() * 0.42f - 0.21f, y = -0.36f + (float)rnd.NextDouble() * 0.42f;
                double a = rnd.NextDouble() * Math.PI;
                float dx = (float)Math.Cos(a) * 0.03f, dy = (float)Math.Sin(a) * 0.03f;
                p.Fill(Sd.Capsule(x - dx, y - dy, x + dx, y + dy, 0.016f), cols[i % cols.Length]);
            }
            p.Shape(jar, Rgba.Clear);
            p.Shape(Sd.Box(0, 0.3f, 0.3f, 0.1f, 0.05f), Rgba.Hex("FF8FAE"));
            foreach (float x in new[] { -0.12f, 0f, 0.12f }) p.Fill(Sd.Circle(x, 0.33f, 0.025f), D27Ink);
            return new DoodleArt("ic_shaker", p);
        }

        /// <summary>Ciotolina di ciliegie (centro 0,0).</summary>
        public static DoodleArt D40CherryBowl()
        {
            var p = new DoodlePainter(-0.5f, -0.35f, 0.5f, 0.45f, 130) { Ink = 0.04f };
            foreach (var c in new[] { new[] { -0.2f, 0.05f }, new[] { 0.18f, 0.06f }, new[] { 0f, 0.12f } })
            {
                p.InkLine(0.025f, c[0], c[1] + 0.08f, c[0] + 0.06f, c[1] + 0.28f);
                Body(p, Sd.Circle(c[0], c[1], 0.12f), Rgba.Hex("E8233F"), Rgba.Hex("B0172D"));
                p.Fill(Sd.Circle(c[0] - 0.04f, c[1] + 0.04f, 0.03f), Rgba.White.WithAlpha(0.8f));
            }
            Sdf bowl = Sd.Intersect(Sd.Ellipse(0, 0.0f, 0.45f, 0.32f), Sd.Box(0, -0.18f, 0.5f, 0.18f));
            Body(p, bowl, Rgba.Hex("9BE3D5"), Rgba.Hex("6FC3B2"));
            return new DoodleArt("ic_cherry_bowl", p);
        }

        /// <summary>Bidone della spazzatura col coperchio (centro 0,0).</summary>
        public static DoodleArt D40Bin()
        {
            var p = new DoodlePainter(-0.42f, -0.48f, 0.42f, 0.5f, 130) { Ink = 0.045f };
            Sdf bin = Sd.Polygon(-0.3f, 0.25f, 0.3f, 0.25f, 0.25f, -0.42f, -0.25f, -0.42f);
            Body(p, bin, Rgba.Hex("8FA3B0"), Rgba.Hex("6A7F8C"));
            foreach (float x in new[] { -0.12f, 0f, 0.12f }) p.Line(Rgba.Hex("6A7F8C"), 0.03f, x, 0.15f, x * 0.85f, -0.32f);
            p.Shape(Sd.Box(0, 0.3f, 0.36f, 0.06f, 0.03f), Rgba.Hex("A9BAC6"));
            p.Shape(Sd.Box(0, 0.4f, 0.08f, 0.04f, 0.03f), Rgba.Hex("A9BAC6"), 0.03f);
            return new DoodleArt("ic_bin", p);
        }

        /// <summary>Pulsantone verde arrotondato (centro 0,0, 1.24 x 0.76).</summary>
        public static DoodleArt D40GreenButton()
        {
            var p = new DoodlePainter(-0.66f, -0.42f, 0.66f, 0.42f, 120) { Ink = 0.05f };
            p.Shape(Sd.Box(0, -0.05f, 0.6f, 0.33f, 0.2f), Rgba.Hex("2E8F3E"));
            Body(p, Sd.Box(0, 0.03f, 0.6f, 0.33f, 0.2f), Rgba.Hex("5BC46A"), Rgba.Hex("3EA34E"), null, 0.05f, -0.06f);
            p.Fill(Sd.Capsule(-0.4f, 0.22f, -0.15f, 0.25f, 0.035f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("ic_serve", p);
        }

        /// <summary>Foglietto dell'ordinazione con la puntina (centro 0,0, 1.4 x 1.9).</summary>
        public static DoodleArt D40Ticket()
        {
            var p = new DoodlePainter(-0.75f, -1.05f, 0.75f, 1.05f, 110) { Ink = 0.04f };
            var pts = new List<float> { -0.68f, 0.92f, 0.68f, 0.92f };
            for (int i = 0; i <= 12; i++) { pts.Add(0.68f - i * (1.36f / 12f)); pts.Add(i % 2 == 0 ? -0.92f : -0.98f); }
            Sdf paper = Sd.Polygon(pts.ToArray());
            Body(p, paper, Rgba.Hex("FFFDF2"), Rgba.Hex("EDE6CC"), null, 0.04f, -0.04f);
            p.Fill(Sd.Box(0, 0.72f, 0.6f, 0.11f), Rgba.Hex("FFD1DC"));
            for (float y = 0.4f; y > -0.85f; y -= 0.22f) p.Line(Rgba.Hex("BFE6FF"), 0.015f, -0.6f, y, 0.6f, y);
            p.Shape(Sd.Circle(0, 0.9f, 0.08f), Rgba.Hex("E8433F"), 0.03f);
            return new DoodleArt("ic_ticket", p);
        }

        /// <summary>Gatto cliente seduto, si lecca i baffi (base 0,0, alto 1.5).</summary>
        public static DoodleArt D40CatCustomer()
        {
            var p = new DoodlePainter(-0.8f, -0.05f, 0.8f, 1.6f, 120) { Ink = 0.05f };
            Body(p, Sd.Ellipse(0, 0.35f, 0.55f, 0.45f), CatOrange, CatShade);
            p.Fill(Sd.Ellipse(0, 0.3f, 0.3f, 0.3f), Rgba.Hex("FFE2C2"));
            Sdf earL = Sd.Polygon(-0.48f, 1.05f, -0.42f, 1.5f, -0.12f, 1.25f);
            Sdf earR = Sd.Polygon(0.48f, 1.05f, 0.42f, 1.5f, 0.12f, 1.25f);
            p.Shape(Sd.Union(earL, earR), CatOrange);
            p.Fill(Sd.Grow(Sd.Union(earL, earR), -0.07f), CatPink);
            Sdf head = Sd.Ellipse(0, 1.0f, 0.52f, 0.4f);
            Body(p, head, CatOrange, CatShade);
            foreach (float x in new[] { -0.12f, 0f, 0.12f }) p.Line(CatShade, 0.04f, x, 1.38f, x, 1.25f);
            V40D_Eye(p, -0.2f, 1.06f, 0.07f);
            V40D_Eye(p, 0.2f, 1.06f, 0.07f);
            p.Shape(Sd.Polygon(-0.05f, 0.95f, 0.05f, 0.95f, 0f, 0.9f), CatPink, 0.02f);
            p.InkLine(0.025f, -0.1f, 0.84f, 0f, 0.88f, 0.1f, 0.84f);
            p.Shape(Sd.Ellipse(0.1f, 0.8f, 0.06f, 0.05f), Rgba.Hex("FF7F9C"), 0.02f);
            foreach (int s in new[] { -1, 1 })
            {
                p.Line(D27Ink, 0.018f, s * 0.25f, 0.92f, s * 0.6f, 0.98f);
                p.Line(D27Ink, 0.018f, s * 0.25f, 0.88f, s * 0.6f, 0.85f);
                p.Shape(Sd.Ellipse(s * 0.3f, 0.06f, 0.16f, 0.1f), CatOrange, 0.035f);
            }
            p.Fill(Sd.Circle(-0.3f, 0.95f, 0.06f), BlushColor.WithAlpha(0.5f));
            p.Fill(Sd.Circle(0.3f, 0.95f, 0.06f), BlushColor.WithAlpha(0.5f));
            return new DoodleArt("ic_cat_customer", p);
        }

        // ================= PIRATE CANNON =================

        /// <summary>Mare aperto (coordinate del livello): orizzonte in y = 0.1, scoglio col cannone in basso a sinistra.</summary>
        public static DoodleArt D40SeaBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 3.5f, 5f, 3.6f), (x, y) => Rgba.Lerp(Rgba.Hex("FFE7B8"), Rgba.Hex("8FD3FF"), Clamp((y - 0.1f) / 3.5f)));
            foreach (var c in new[] { new[] { -1.6f, 2.4f }, new[] { 1.9f, 3.1f } })
                p.Fill(Sd.Union(Sd.Circle(c[0] - 0.3f, c[1], 0.25f), Sd.Circle(c[0], c[1] + 0.12f, 0.32f), Sd.Circle(c[0] + 0.35f, c[1], 0.22f), Sd.Box(c[0], c[1] - 0.1f, 0.55f, 0.12f, 0.1f)), Rgba.White.WithAlpha(0.85f));
            // isoletta lontana con la palma
            p.Fill(Sd.Ellipse(-2.6f, 0.1f, 0.9f, 0.25f), Rgba.Hex("E9CF8E"));
            p.Line(Rgba.Hex("8A5A3B"), 0.06f, -2.5f, 0.2f, -2.45f, 0.6f, -2.55f, 0.95f);
            foreach (float a in new[] { 150f, 110f, 60f, 20f })
            {
                double r = a * Math.PI / 180;
                p.Fill(Sd.Rotate(Sd.Ellipse(-2.55f + (float)Math.Cos(r) * 0.25f, 0.95f + (float)Math.Sin(r) * 0.1f, 0.28f, 0.07f), -2.55f + (float)Math.Cos(r) * 0.25f, 0.95f + (float)Math.Sin(r) * 0.1f, a > 90 ? a - 180 + 20 : a - 20), Rgba.Hex("4E9E44"));
            }
            // mare
            p.FillFunc(Sd.Box(0, -3.6f, 5f, 3.7f), (x, y) => Rgba.Lerp(Rgba.Hex("1E5F9E"), Rgba.Hex("4DA6E0"), Clamp((y + 4f) / 4.1f)));
            var rnd = new Random(9);
            for (int i = 0; i < 40; i++)
            {
                float x = (float)rnd.NextDouble() * 9f - 4.5f, y = -(float)rnd.NextDouble() * 5f;
                p.Line(Rgba.White.WithAlpha(0.35f), 0.03f, x - 0.15f, y, x, y + 0.04f, x + 0.15f, y);
            }
            // scoglio
            Sdf rock = Sd.Union(Sd.Ellipse(-2.9f, -3.9f, 2.2f, 1.25f), Sd.Ellipse(-1.4f, -4.5f, 1.4f, 0.9f), Sd.Ellipse(-3.4f, -3.0f, 1.2f, 0.9f));
            Body(p, rock, Rgba.Hex("9A8E86"), Rgba.Hex("6E625C"), null, 0.08f, -0.08f);
            p.Line(Rgba.Hex("6E625C"), 0.04f, -2.6f, -3.6f, -2.2f, -3.9f, -2.3f, -4.3f);
            p.Line(Rgba.Hex("6E625C"), 0.04f, -1.2f, -4.2f, -0.8f, -4.5f);
            p.Fill(Sd.Ellipse(-2.0f, -2.85f, 0.75f, 0.1f), Rgba.Hex("000000", 0.15f));
            return new DoodleArt("pc_bg", p);
        }

        /// <summary>Striscia di onde in primo piano (linea dell'acqua in y = 0; larga 9).</summary>
        public static DoodleArt D40Waves()
        {
            var p = new DoodlePainter(-4.6f, -0.65f, 4.6f, 0.25f, 60) { Ink = 0.04f };
            Sdf wave = (x, y) => y - (0.06f * (float)Math.Sin(x * 4.2f) + 0.03f * (float)Math.Sin(x * 9f + 1f));
            p.FillFunc(Sd.Intersect(wave, Sd.Box(0, -0.3f, 5f, 0.4f)), (x, y) => Rgba.Hex("3E95D4", Clamp((y + 0.65f) / 0.35f)));
            p.Line(Rgba.White.WithAlpha(0.85f), 0.05f, V40D_Wave(-4.6f, 4.6f));
            return new DoodleArt("pc_waves", p);
        }

        static float[] V40D_Wave(float x0, float x1)
        {
            var l = new List<float>();
            for (float x = x0; x <= x1; x += 0.08f) { l.Add(x); l.Add(0.06f * (float)Math.Sin(x * 4.2f) + 0.03f * (float)Math.Sin(x * 9f + 1f)); }
            return l.ToArray();
        }

        /// <summary>Nave pirata vuota (linea dell'acqua 0,0; ponte in y = 0.45; albero in x = 0.1 fino a 2.45; prua a sinistra).</summary>
        public static DoodleArt D40Ship()
        {
            var p = new DoodlePainter(-1.85f, -0.6f, 1.55f, 2.55f, 90) { Ink = 0.05f };
            // bompresso
            p.Shape(Sd.Polyline(0.04f, -1.25f, 0.4f, -1.8f, 0.75f), Rgba.Hex("7A3F1D"));
            // albero e vela
            p.Shape(Sd.Box(0.1f, 1.45f, 0.06f, 1.0f), WoodDark);
            Sdf sail = Sd.Polygon(0.0f, 2.15f, 0.0f, 0.95f, -0.75f, 1.05f, -0.85f, 1.55f, -0.75f, 2.05f);
            Body(p, sail, Rgba.Hex("F2E6C8"), Rgba.Hex("D6C49A"));
            p.Shape(Sd.Box(-0.5f, 1.3f, 0.12f, 0.1f), Rgba.Hex("E2D2A8"), 0.02f);
            p.Shape(Sd.Circle(-0.4f, 1.7f, 0.13f), Rgba.Hex("1E1A24"), 0.02f);
            p.Fill(Sd.Circle(-0.44f, 1.72f, 0.03f), Rgba.White);
            p.Fill(Sd.Circle(-0.36f, 1.72f, 0.03f), Rgba.White);
            p.Shape(Sd.Box(0.1f, 2.2f, 0.2f, 0.05f, 0.02f), WoodDark, 0.03f);
            // scafo
            Sdf hull = Sd.Polygon(-1.35f, 0.5f, 1.35f, 0.5f, 1.05f, -0.5f, -1.0f, -0.5f);
            Body(p, hull, Rgba.Hex("A35E2E"), Rgba.Hex("7A3F1D"), null, 0.06f, -0.06f);
            for (int i = 1; i <= 3; i++) p.Line(Rgba.Hex("6E3A1A"), 0.025f, -1.3f + i * 0.08f, 0.5f - i * 0.25f, 1.32f - i * 0.08f, 0.5f - i * 0.25f);
            foreach (float x in new[] { -0.6f, 0f, 0.6f }) { p.Shape(Sd.Circle(x, 0.18f, 0.09f), Rgba.Hex("1E1A24"), 0.03f); }
            // castello di poppa
            Body(p, Sd.Box(1.05f, 0.68f, 0.32f, 0.2f, 0.03f), Rgba.Hex("B86E38"), Rgba.Hex("8A4E22"));
            p.Shape(Sd.Box(0, 0.52f, 1.38f, 0.04f, 0.02f), Rgba.Hex("6E3A1A"), 0.03f);
            return new DoodleArt("pc_ship", p);
        }

        /// <summary>Bandiera nera col teschio (attacco all'asta in 0,0, sventola verso destra).</summary>
        public static DoodleArt D40JollyFlag()
        {
            var p = new DoodlePainter(-0.05f, -0.32f, 0.78f, 0.32f, 140) { Ink = 0.035f };
            p.Shape(Sd.Polygon(0f, 0.25f, 0.35f, 0.2f, 0.72f, 0.24f, 0.63f, 0f, 0.72f, -0.22f, 0.35f, -0.25f, 0f, -0.23f), Rgba.Hex("1E1A24"));
            p.Line(Rgba.White, 0.035f, 0.18f, -0.15f, 0.5f, 0.1f);
            p.Line(Rgba.White, 0.035f, 0.18f, 0.1f, 0.5f, -0.15f);
            p.Fill(Sd.Circle(0.34f, 0.0f, 0.1f), Rgba.White);
            p.Fill(Sd.Circle(0.3f, 0.01f, 0.025f), Rgba.Hex("1E1A24"));
            p.Fill(Sd.Circle(0.38f, 0.01f, 0.025f), Rgba.Hex("1E1A24"));
            return new DoodleArt("pc_flag", p);
        }

        /// <summary>Barile di polvere da sparo (base 0,0, alto 0.55).</summary>
        public static DoodleArt D40PowderBarrel()
        {
            var p = new DoodlePainter(-0.3f, -0.03f, 0.3f, 0.6f, 140) { Ink = 0.04f };
            Sdf barrel = Sd.Union(Sd.Ellipse(0, 0.28f, 0.25f, 0.3f), Sd.Box(0, 0.28f, 0.2f, 0.27f));
            Body(p, barrel, Wood, WoodShade);
            foreach (float y in new[] { 0.1f, 0.46f }) p.Shape(Sd.Box(0, y, 0.24f, 0.03f), Rgba.Hex("4A4655"), 0.02f);
            p.Shape(Sd.Box(0, 0.28f, 0.12f, 0.07f, 0.02f), Rgba.Hex("FFF1C9"), 0.02f);
            p.Line(Rgba.Hex("E8433F"), 0.03f, -0.07f, 0.28f, 0.07f, 0.28f);
            return new DoodleArt("pc_barrel", p);
        }

        /// <summary>Capitano pirata dalla vita in su, con benda e barba (base 0,0; testa in 0, 0.72).</summary>
        public static DoodleArt D40Captain()
        {
            var p = new DoodlePainter(-0.42f, -0.03f, 0.42f, 0.98f, 130) { Ink = 0.045f };
            Body(p, Sd.Box(0, 0.25f, 0.3f, 0.27f, 0.1f), Rgba.Hex("C0392B"), Rgba.Hex("922B21"));
            p.Fill(Sd.Box(0, 0.25f, 0.06f, 0.26f), Rgba.Hex("FFD24A"));
            p.Shape(Sd.Capsule(-0.28f, 0.42f, -0.36f, 0.1f, 0.07f), Rgba.Hex("C0392B"));
            p.Shape(Sd.Capsule(0.28f, 0.42f, 0.36f, 0.12f, 0.07f), Rgba.Hex("C0392B"));
            p.InkLine(0.04f, 0.36f, 0.06f, 0.4f, 0.0f, 0.34f, -0.02f);
            p.Shape(Sd.Circle(0, 0.7f, 0.2f), Skin);
            p.Shape(Sd.Polygon(-0.2f, 0.66f, 0.2f, 0.66f, 0.12f, 0.45f, 0f, 0.4f, -0.12f, 0.45f), Rgba.Hex("2B2133"));
            p.InkLine(0.03f, -0.2f, 0.8f, 0.2f, 0.72f);
            p.Shape(Sd.Circle(-0.08f, 0.76f, 0.05f), Rgba.Hex("1E1A24"), 0.02f);
            V40D_Eye(p, 0.08f, 0.74f, 0.03f);
            p.Shape(Sd.Circle(0.0f, 0.66f, 0.04f), SkinShade, 0.02f);
            return new DoodleArt("pc_captain", p);
        }

        /// <summary>Tricorno nero col teschio (base 0,0).</summary>
        public static DoodleArt D40Tricorn()
        {
            var p = new DoodlePainter(-0.36f, -0.06f, 0.36f, 0.3f, 160) { Ink = 0.035f };
            p.Shape(Sd.Polygon(-0.32f, 0.02f, -0.18f, 0.2f, 0f, 0.27f, 0.18f, 0.2f, 0.32f, 0.02f, 0f, 0.07f), Rgba.Hex("1E1A24"));
            p.Line(Rgba.Hex("FFD24A"), 0.025f, -0.28f, 0.04f, 0f, 0.09f, 0.28f, 0.04f);
            p.Fill(Sd.Circle(0, 0.16f, 0.05f), Rgba.White);
            return new DoodleArt("pc_hat", p);
        }

        /// <summary>Affusto di legno con la ruota (asse del cannone in 0,0).</summary>
        public static DoodleArt D40CannonBase()
        {
            var p = new DoodlePainter(-0.65f, -0.55f, 0.65f, 0.15f, 120) { Ink = 0.045f };
            Body(p, Sd.Polygon(-0.6f, -0.35f, 0.45f, -0.35f, 0.3f, 0.08f, -0.35f, 0.08f), Wood, WoodShade);
            foreach (float x in new[] { -0.35f, 0.25f })
            {
                p.Shape(Sd.Circle(x, -0.33f, 0.18f), WoodDark);
                p.Shape(Sd.Circle(x, -0.33f, 0.06f), Rgba.Hex("4A4655"), 0.025f);
            }
            return new DoodleArt("pc_cannon_base", p);
        }

        /// <summary>Canna del cannone verso destra (perno in 0,0; bocca in 1.1, 0).</summary>
        public static DoodleArt D40CannonBarrel()
        {
            var p = new DoodlePainter(-0.42f, -0.28f, 1.18f, 0.28f, 130) { Ink = 0.045f };
            Rgba iron = Rgba.Hex("4A4655"), ironShade = Rgba.Hex("2F2C38");
            p.Shape(Sd.Circle(-0.32f, 0, 0.08f), iron);
            Body(p, Sd.Polygon(-0.25f, 0.22f, 1.0f, 0.15f, 1.0f, -0.15f, -0.25f, -0.22f), iron, ironShade, null, 0.04f, -0.06f);
            p.Shape(Sd.Box(1.05f, 0, 0.07f, 0.2f, 0.03f), iron);
            p.Shape(Sd.Box(0.2f, 0, 0.05f, 0.21f, 0.02f), Rgba.Hex("6B6677"), 0.03f);
            p.Fill(Sd.Capsule(-0.1f, 0.1f, 0.8f, 0.07f, 0.03f), Rgba.White.WithAlpha(0.3f));
            return new DoodleArt("pc_cannon_barrel", p);
        }

        /// <summary>Palla di cannone (centro 0,0).</summary>
        public static DoodleArt D40Ball()
        {
            var p = new DoodlePainter(-0.16f, -0.16f, 0.16f, 0.16f, 200) { Ink = 0.03f };
            Body(p, Sd.Circle(0, 0, 0.12f), Rgba.Hex("4A4655"), Rgba.Hex("2F2C38"));
            p.Fill(Sd.Circle(-0.04f, 0.04f, 0.03f), Rgba.White.WithAlpha(0.5f));
            return new DoodleArt("pc_ball", p);
        }

        /// <summary>Rampino a tre punte che vola verso destra (anello della corda in -0.2, 0).</summary>
        public static DoodleArt D40Hook()
        {
            var p = new DoodlePainter(-0.3f, -0.28f, 0.3f, 0.28f, 200) { Ink = 0.03f };
            Rgba steel = Rgba.Hex("B7BCC9");
            p.Shape(Sd.Capsule(-0.18f, 0, 0.12f, 0, 0.035f), steel);
            p.Shape(Sd.Polyline(0.03f, 0.12f, 0f, 0.2f, 0.12f, 0.08f, 0.22f), steel);
            p.Shape(Sd.Polyline(0.03f, 0.12f, 0f, 0.2f, -0.12f, 0.08f, -0.22f), steel);
            p.Shape(Sd.Polyline(0.03f, 0.12f, 0f, 0.24f, 0f), steel);
            p.Shape(Sd.Subtract(Sd.Circle(-0.22f, 0, 0.06f), Sd.Circle(-0.22f, 0, 0.03f)), steel, 0.025f);
            return new DoodleArt("pc_hook", p);
        }

        /// <summary>Forziere chiuso o aperto pieno d'oro (centro della cassa 0,0).</summary>
        public static DoodleArt D40Chest(bool open)
        {
            var p = new DoodlePainter(-0.52f, -0.32f, 0.52f, 0.85f, 130) { Ink = 0.045f };
            if (open)
            {
                Body(p, Sd.Polygon(-0.45f, 0.15f, 0.45f, 0.15f, 0.4f, 0.7f, -0.4f, 0.7f), Rgba.Hex("8A4E22"), Rgba.Hex("6E3A1A"));
                p.Shape(Sd.Union(Sd.Circle(-0.22f, 0.18f, 0.14f), Sd.Circle(0.02f, 0.25f, 0.18f), Sd.Circle(0.25f, 0.17f, 0.13f)), Gold);
                foreach (var c in new[] { new[] { -0.2f, 0.22f }, new[] { 0.05f, 0.32f }, new[] { 0.24f, 0.2f } }) p.Shape(Sd.Circle(c[0], c[1], 0.05f), GoldLight, 0.02f);
                p.Shape(Sd.Rotate(Sd.Box(-0.05f, 0.42f, 0.04f, 0.08f, 0.01f), -0.05f, 0.42f, 20f), Rgba.Hex("3CE6FF"), 0.02f);
            }
            Sdf box = Sd.Box(0, -0.05f, 0.45f, 0.25f, 0.04f);
            Body(p, box, Rgba.Hex("A35E2E"), Rgba.Hex("7A3F1D"));
            if (!open)
            {
                Sdf lid = Sd.Intersect(Sd.Ellipse(0, 0.2f, 0.45f, 0.22f), Sd.Box(0, 0.3f, 0.5f, 0.12f));
                Body(p, Sd.Union(lid, Sd.Box(0, 0.22f, 0.45f, 0.04f)), Rgba.Hex("B86E38"), Rgba.Hex("8A4E22"));
            }
            foreach (float x in new[] { -0.3f, 0.3f }) p.Shape(Sd.Box(x, open ? -0.05f : 0.05f, 0.05f, open ? 0.25f : 0.35f, 0.01f), Gold, 0.025f);
            p.Shape(Sd.Box(0, 0.12f, 0.08f, 0.1f, 0.02f), Gold, 0.03f);
            p.Fill(Sd.Circle(0, 0.1f, 0.025f), D27Ink);
            return new DoodleArt(open ? "pc_chest_open" : "pc_chest", p);
        }

        /// <summary>Pulsantone rosso rotondo (centro 0,0, raggio 0.58).</summary>
        public static DoodleArt D40RedButton()
        {
            var p = new DoodlePainter(-0.66f, -0.66f, 0.66f, 0.66f, 120) { Ink = 0.05f };
            p.Shape(Sd.Circle(0, -0.05f, 0.6f), Rgba.Hex("4A4655"));
            p.Shape(Sd.Circle(0, -0.06f, 0.5f), Rgba.Hex("9E1F1A"));
            Body(p, Sd.Circle(0, 0.03f, 0.5f), Rgba.Hex("E8433F"), Rgba.Hex("B92E2A"), null, 0.05f, -0.06f);
            p.Fill(Sd.Ellipse(-0.18f, 0.28f, 0.15f, 0.07f), Rgba.White.WithAlpha(0.5f));
            return new DoodleArt("pc_fire", p);
        }
    }
}
