using System;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni della versione 0.26, seconda parte (livelli 85-100).</summary>
    public static partial class DoodleSprites
    {
        // ---------- 85: il pupazzo sul ghiaccio ----------

        /// <summary>Lago ghiacciato tra le colline innevate, cielo chiaro (coordinate del livello; ghiaccio sotto y = -1.6).</summary>
        public static DoodleArt IceLake()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 4.6f, 7.1f), (x, y) => Rgba.Lerp(Rgba.Hex("FFE7C2"), Rgba.Hex("8FCDF2"), Clamp((y + 1f) / 7f)));
            Body(p, Sd.Union(Sd.Ellipse(-2.8f, -1.4f, 3f, 1.1f), Sd.Ellipse(2.7f, -1.3f, 2.8f, 1.2f)), Rgba.White, Rgba.Hex("D3E6F2"));
            p.FillFunc(Sd.Box(0, -4.6f, 4.6f, 3.0f), (x, y) => Rgba.Lerp(Rgba.Hex("BFE6F7"), Rgba.Hex("E6F6FD"), Clamp((y + 7f) / 5.4f)));
            p.Shape(Sd.Box(0, -4.6f, 4.6f, 3.0f), Rgba.Clear, 0.05f);
            for (int i = 0; i < 9; i++)
            {
                float x = -3.8f + i * 0.95f, y = -2.4f - (i % 3) * 1.1f;
                p.Line(Rgba.White.WithAlpha(0.8f), 0.04f, x, y, x + 0.6f, y + 0.12f);
            }
            return new DoodleArt("ice_lake_bg", p);
        }

        /// <summary>Abete alto con la neve sui rami (base del tronco in 0,0).</summary>
        public static DoodleArt Pine()
        {
            var p = new DoodlePainter(-1.3f, -0.1f, 1.3f, 3.6f, 80) { Ink = 0.05f };
            Body(p, Sd.Box(0, 0.35f, 0.17f, 0.4f, 0.04f), Rgba.Hex("8A5A3B"), Rgba.Hex("5E3A22"));
            for (int i = 0; i < 3; i++)
            {
                float b = 0.7f + i * 0.85f, w = 1.2f - i * 0.3f;
                Body(p, Sd.Polygon(-w, b, w, b, 0, b + 1.3f), Leaf, LeafShade);
                p.Shape(Sd.Polygon(-w * 0.55f, b + 0.6f, w * 0.55f, b + 0.6f, 0, b + 1.3f), Rgba.White, 0.03f);
            }
            return new DoodleArt("pine", p);
        }

        /// <summary>Pupazzetto di neve con sciarpa (base in 0,0, alto 1.3).</summary>
        public static DoodleArt SnowBuddy()
        {
            var p = new DoodlePainter(-0.6f, -0.08f, 0.6f, 1.45f, 130) { Ink = 0.04f };
            Body(p, Sd.Circle(0, 0.4f, 0.42f), Rgba.White, Rgba.Hex("CFE3F2"));
            Body(p, Sd.Circle(0, 1.0f, 0.3f), Rgba.White, Rgba.Hex("CFE3F2"));
            p.Shape(Sd.Box(0, 0.74f, 0.3f, 0.07f, 0.04f), Rgba.Hex("E8433F"), 0.025f);
            p.Shape(Sd.Box(0.2f, 0.6f, 0.06f, 0.15f, 0.03f), Rgba.Hex("E8433F"), 0.025f);
            foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Circle(s * 0.1f, 1.06f, 0.035f), Ink);
            p.Shape(Sd.Polygon(0, 0.98f, 0.25f, 0.94f, 0, 0.92f), Rgba.Hex("FF8A2E"), 0.015f);
            for (int i = 0; i < 2; i++) p.Fill(Sd.Circle(0, 0.32f + i * 0.2f, 0.035f), Ink);
            return new DoodleArt("snow_buddy", p);
        }

        /// <summary>Ombra morbida (ellisse larga 1, alta 0.4, centro 0,0): si allunga con la scala x.</summary>
        public static DoodleArt Shade()
        {
            var p = new DoodlePainter(-0.55f, -0.25f, 0.55f, 0.25f, 100) { Wobble = 0f };
            p.FillFunc(Sd.Ellipse(0, 0, 0.5f, 0.2f), (x, y) =>
            {
                float r = (float)Math.Sqrt(x * x / 0.25f + y * y / 0.04f);
                return Rgba.Hex("2B4A6B").WithAlpha(0.45f * Clamp((1f - r) * 4f));
            });
            return new DoodleArt("shade", p);
        }

        // ---------- 87: lo scassinatore ----------

        /// <summary>Manopola della cassaforte con 40 tacche (centro 0,0, raggio 0.95). I numeri sono testi a parte.</summary>
        public static DoodleArt SafeDial()
        {
            var p = new DoodlePainter(-1.05f, -1.05f, 1.05f, 1.05f, 140) { Ink = 0.045f };
            Body(p, Sd.Circle(0, 0, 0.95f), Rgba.Hex("C9CFD6"), Rgba.Hex("8D93A6"));
            p.Shape(Sd.Circle(0, 0, 0.62f), Rgba.Hex("2B2133"), 0.03f);
            for (int i = 0; i < 40; i++)
            {
                double a = i * Math.PI * 2 / 40;
                float r0 = i % 5 == 0 ? 0.74f : 0.82f;
                p.InkLine(i % 5 == 0 ? 0.03f : 0.015f, (float)Math.Sin(a) * r0, (float)Math.Cos(a) * r0, (float)Math.Sin(a) * 0.92f, (float)Math.Cos(a) * 0.92f);
            }
            Body(p, Sd.Box(0, 0, 0.12f, 0.45f, 0.06f), Rgba.Hex("6B7085"), Rgba.Hex("3F4357"));
            return new DoodleArt("safe_dial", p);
        }

        // ---------- 89: il caffè ----------

        /// <summary>Caffettiera di profilo, beccuccio a destra (in 0.78,0.55), perno sul manico a sinistra (0,0).</summary>
        public static DoodleArt CoffeePot()
        {
            var p = new DoodlePainter(-0.45f, -0.7f, 1.0f, 0.95f, 130) { Ink = 0.045f };
            p.Shape(Sd.Subtract(Sd.Circle(0.0f, 0.05f, 0.33f), Sd.Circle(0.0f, 0.05f, 0.2f)), Rgba.Hex("2B2133"), 0.03f);   // manico
            Body(p, Sd.Polygon(0.45f, 0.35f, 0.82f, 0.62f, 0.86f, 0.52f, 0.55f, 0.15f), Rgba.Hex("D9DEE6"), Rgba.Hex("9AA1B4"));   // beccuccio
            Body(p, Sd.Box(0.3f, -0.05f, 0.33f, 0.55f, 0.12f), Rgba.Hex("E8433F"), Rgba.Hex("B92E2A"));
            Body(p, Sd.Box(0.3f, 0.6f, 0.25f, 0.08f, 0.04f), Rgba.Hex("2B2133"), Rgba.Hex("1A1420"));
            p.Fill(Sd.Circle(0.3f, 0.75f, 0.07f), Rgba.Hex("2B2133"));
            p.Line(Rgba.White.WithAlpha(0.5f), 0.04f, 0.1f, -0.4f, 0.1f, 0.3f);
            return new DoodleArt("coffee_pot", p);
        }

        /// <summary>Tazza: interno da x -0.4 a 0.4, fondo a y 0.08, bordo a y 1.0 (base in 0,0). Il caffè è una barra a parte.</summary>
        public static DoodleArt Mug(bool front)
        {
            var p = new DoodlePainter(-0.75f, -0.08f, 0.6f, 1.12f, 130) { Ink = 0.045f };
            Sdf cup = Sd.Box(0, 0.52f, 0.47f, 0.5f, 0.1f);
            if (!front)
            {
                p.Shape(Sd.Subtract(Sd.Circle(-0.47f, 0.55f, 0.25f), Sd.Circle(-0.47f, 0.55f, 0.13f)), Rgba.Hex("6FC3F2"), 0.03f);
                p.Fill(Sd.Box(0, 0.55f, 0.4f, 0.47f), Rgba.Hex("FFFFFF", 0.35f));
                p.Shape(cup, Rgba.Clear, 0.045f);
            }
            else
            {
                // solo il vetro davanti: bordi e riflessi, lascia vedere il caffè
                p.Shape(Sd.Subtract(cup, Sd.Box(0, 0.55f, 0.4f, 0.47f)), Rgba.Hex("6FC3F2"), 0.045f);
                p.Line(Rgba.White.WithAlpha(0.6f), 0.04f, -0.3f, 0.25f, -0.3f, 0.85f);
            }
            return new DoodleArt(front ? "mug_front" : "mug_back", p);
        }

        // ---------- 90: il domino ----------

        /// <summary>Tessera del domino in piedi, di profilo (base in 0,0, larga 0.22, alta 1).</summary>
        public static DoodleArt Domino()
        {
            var p = new DoodlePainter(-0.16f, -0.04f, 0.16f, 1.04f, 160) { Ink = 0.02f };
            Body(p, Sd.Box(0, 0.5f, 0.11f, 0.5f, 0.03f), Rgba.Hex("F4F1E8"), Rgba.Hex("CFC8B6"));
            p.InkLine(0.015f, -0.08f, 0.5f, 0.08f, 0.5f);
            foreach (float y in new[] { 0.25f, 0.72f, 0.82f }) p.Fill(Sd.Circle(0, y, 0.03f), Ink);
            return new DoodleArt("domino", p);
        }

        /// <summary>Campanello da banco (base in 0,0).</summary>
        public static DoodleArt Bell()
        {
            var p = new DoodlePainter(-0.5f, -0.06f, 0.5f, 0.85f, 140) { Ink = 0.04f };
            Body(p, Sd.Box(0, 0.07f, 0.45f, 0.07f, 0.03f), Rgba.Hex("8A5A3B"), Rgba.Hex("5E3A22"));
            Body(p, Sd.Intersect(Sd.Circle(0, 0.15f, 0.42f), Sd.Box(0, 0.4f, 0.5f, 0.26f)), Gold, GoldShade);
            p.Shape(Sd.Box(0, 0.72f, 0.04f, 0.08f, 0.02f), Rgba.Hex("2B2133"), 0.02f);
            p.Fill(Sd.Circle(0, 0.82f, 0.06f), Rgba.Hex("2B2133"));
            return new DoodleArt("bell", p);
        }

        // ---------- 92: pesca con la calamita ----------

        /// <summary>Molo e stagno: cielo sopra, acqua verde scuro sotto y = 1.2, fondo sabbioso (coordinate del livello).</summary>
        public static DoodleArt Pond()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 4.6f, 7.1f), (x, y) => Rgba.Lerp(Rgba.Hex("D8F0FB"), Rgba.Hex("8CCDF0"), Clamp((y - 1.2f) / 4f)));
            p.FillFunc(Sd.Box(0, -3f, 4.6f, 4.2f), (x, y) => Rgba.Lerp(Rgba.Hex("1E5B5A"), Rgba.Hex("3E9A92"), Clamp((y + 4f) / 5.2f)));
            p.Line(Rgba.White.WithAlpha(0.7f), 0.05f, -4.5f, 1.2f, 4.5f, 1.2f);
            Body(p, Sd.Box(0, -5.2f, 4.6f, 1.6f), Rgba.Hex("D8C3A5"), Rgba.Hex("BFA582"));
            for (int i = 0; i < 6; i++) p.Line(Rgba.Hex("5BBF4A"), 0.06f, -2.6f + i * 1.05f, -3.6f, -2.5f + i * 1.05f + (i % 2) * 0.2f, -2.4f - (i % 3) * 0.3f);
            Body(p, Sd.Box(-3.0f, 1.5f, 1.6f, 0.15f, 0.03f), Rgba.Hex("B98A5A"), Rgba.Hex("8A5A3B"));   // molo
            Body(p, Sd.Box(-2.0f, 0.6f, 0.1f, 0.9f), Rgba.Hex("8A5A3B"), Rgba.Hex("5E3A22"));
            return new DoodleArt("pond_bg", p);
        }

        public static DoodleArt Magnet()
        {
            var p = new DoodlePainter(-0.35f, -0.35f, 0.35f, 0.3f, 160) { Ink = 0.035f };
            Sdf u = Sd.Subtract(Sd.Union(Sd.Circle(0, -0.02f, 0.27f), Sd.Box(0, 0.12f, 0.27f, 0.14f)), Sd.Union(Sd.Circle(0, -0.02f, 0.12f), Sd.Box(0, 0.14f, 0.12f, 0.16f)));
            Body(p, Sd.Intersect(u, Sd.Box(0, -0.15f, 0.3f, 0.2f)), Rgba.Hex("E8433F"), Rgba.Hex("B92E2A"));
            Body(p, Sd.Intersect(u, Sd.Box(0, 0.15f, 0.3f, 0.1f)), Rgba.Hex("D9DEE6"), Rgba.Hex("9AA1B4"));
            return new DoodleArt("magnet", p);
        }

        public static DoodleArt TinCan()
        {
            var p = new DoodlePainter(-0.3f, -0.36f, 0.3f, 0.36f, 150) { Ink = 0.035f };
            Body(p, Sd.Box(0, 0, 0.22f, 0.3f, 0.04f), Rgba.Hex("C9CFD6"), Rgba.Hex("8D93A6"));
            p.Shape(Sd.Box(0, 0, 0.22f, 0.13f), Rgba.Hex("3E6FE0"), 0.02f);
            p.Fill(Sd.Circle(0, 0, 0.06f), Rgba.Hex("FFE14A"));
            return new DoodleArt("tin_can", p);
        }

        public static DoodleArt Horseshoe()
        {
            var p = new DoodlePainter(-0.32f, -0.32f, 0.32f, 0.32f, 150) { Ink = 0.035f };
            Sdf h = Sd.Subtract(Sd.Circle(0, 0.02f, 0.27f), Sd.Union(Sd.Circle(0, 0.02f, 0.15f), Sd.Box(0, -0.25f, 0.1f, 0.15f)));
            Body(p, h, Rgba.Hex("8D93A6"), Rgba.Hex("5E6275"));
            for (int i = 0; i < 5; i++) { double a = Math.PI * (0.1 + i * 0.2); p.Fill(Sd.Circle((float)Math.Cos(a) * 0.21f, 0.02f + (float)Math.Sin(a) * 0.21f, 0.025f), Ink); }
            return new DoodleArt("horseshoe", p);
        }

        /// <summary>Piranha che guarda a destra (centro 0,0), denti in bella vista.</summary>
        public static DoodleArt Piranha()
        {
            var p = new DoodlePainter(-0.6f, -0.4f, 0.55f, 0.4f, 140) { Ink = 0.035f };
            Body(p, Sd.Polygon(-0.3f, 0f, -0.58f, 0.25f, -0.58f, -0.25f), Rgba.Hex("6B7085"), Rgba.Hex("3F4357"));
            Body(p, Sd.Ellipse(0.05f, 0, 0.42f, 0.3f), Rgba.Hex("8D93A6"), Rgba.Hex("5E6275"));
            p.Fill(Sd.Intersect(Sd.Ellipse(0.05f, 0, 0.42f, 0.3f), Sd.Box(0.05f, -0.2f, 0.45f, 0.12f)), Rgba.Hex("E8433F", 0.85f));
            p.Shape(Sd.Polygon(0.47f, 0.02f, 0.2f, -0.02f, 0.45f, -0.14f), Rgba.Hex("3A0F1E"), 0.02f);
            for (int i = 0; i < 3; i++) p.Shape(Sd.Polygon(0.24f + i * 0.07f, -0.01f, 0.27f + i * 0.07f, -0.08f, 0.3f + i * 0.07f, -0.01f), Rgba.White, 0.01f);
            p.Shape(Sd.Circle(0.25f, 0.12f, 0.06f), Rgba.Hex("FFE14A"), 0.015f);
            p.Fill(Sd.Circle(0.26f, 0.12f, 0.025f), Ink);
            p.InkLine(0.025f, 0.14f, 0.22f, 0.32f, 0.17f);
            return new DoodleArt("piranha", p);
        }

        /// <summary>Canna da pesca appoggiata al molo; la punta è in 0,0.</summary>
        public static DoodleArt FishingRod()
        {
            var p = new DoodlePainter(-1.9f, -1.0f, 0.1f, 0.1f, 100) { Ink = 0.04f };
            p.Line(Rgba.Hex("8A5A3B"), 0.07f, -1.8f, -0.9f, 0f, 0f);
            p.Shape(Sd.Circle(-1.3f, -0.62f, 0.12f), Rgba.Hex("2B2133"), 0.02f);
            return new DoodleArt("fishing_rod", p);
        }

        // ---------- 93: l'orologio a cucù ----------

        /// <summary>Orologio a cucù a casetta: quadrante (centro 0,0, raggio 0.95), sportellino in alto (0, 1.45).</summary>
        public static DoodleArt CuckooClock()
        {
            var p = new DoodlePainter(-1.5f, -1.4f, 1.5f, 2.4f, 100) { Ink = 0.05f };
            Body(p, Sd.Polygon(-1.45f, 1.0f, 0, 2.3f, 1.45f, 1.0f), Rgba.Hex("6B3A1E"), Rgba.Hex("4A2512"));
            Body(p, Sd.Box(0, 0, 1.2f, 1.3f, 0.06f), Rgba.Hex("B98A5A"), Rgba.Hex("8A5A3B"));
            p.Shape(Sd.Box(0, 1.45f, 0.3f, 0.3f, 0.05f), Rgba.Hex("2B2133"), 0.03f);
            p.Shape(Sd.Circle(0, 0, 0.98f), Rgba.Hex("FFF6E0"), 0.04f);
            for (int i = 0; i < 12; i++)
            {
                double a = i * Math.PI / 6;
                p.InkLine(i % 3 == 0 ? 0.06f : 0.03f, (float)Math.Sin(a) * 0.78f, (float)Math.Cos(a) * 0.78f, (float)Math.Sin(a) * 0.9f, (float)Math.Cos(a) * 0.9f);
            }
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Polygon(s * 0.3f, -1.3f, s * 0.42f, -1.9f, s * 0.18f, -1.9f), Gold, 0.03f);   // pigne
            return new DoodleArt("cuckoo_clock", p);
        }

        // ---------- 94: le lucciole ----------

        /// <summary>Lucciola minuscola (centro 0,0). La luce è l'alone "glow" a parte.</summary>
        public static DoodleArt Firefly()
        {
            var p = new DoodlePainter(-0.15f, -0.12f, 0.15f, 0.12f, 200) { Ink = 0.02f };
            p.Shape(Sd.Ellipse(-0.04f, 0.06f, 0.07f, 0.04f), Rgba.Hex("FFFFFF", 0.6f), 0.01f);
            p.Shape(Sd.Ellipse(0.02f, 0, 0.09f, 0.05f), Rgba.Hex("3A2E4A"), 0.012f);
            p.Shape(Sd.Circle(-0.07f, 0f, 0.045f), Rgba.Hex("FFF27A"), 0.012f);
            return new DoodleArt("firefly", p);
        }

        /// <summary>Due occhi gialli nel buio (centro 0,0): aperti, socchiusi o spenti.</summary>
        public static DoodleArt WolfEyes()
        {
            var p = new DoodlePainter(-0.4f, -0.2f, 0.4f, 0.2f, 160) { Wobble = 0f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.FillFunc(Sd.Circle(s * 0.2f, 0, 0.16f), (x, y) => Rgba.Hex("FFE14A").WithAlpha(0.35f * Clamp(1f - (float)Math.Sqrt((x - s * 0.2f) * (x - s * 0.2f) + y * y) / 0.16f)));
                p.Fill(Sd.Ellipse(s * 0.2f, 0, 0.09f, 0.06f), Rgba.Hex("FFE14A"));
                p.Fill(Sd.Ellipse(s * 0.2f, 0, 0.02f, 0.05f), Rgba.Hex("1A1420"));
            }
            return new DoodleArt("wolf_eyes", p);
        }

        /// <summary>Lupo che salta fuori dal cespuglio (centro 0,0).</summary>
        public static DoodleArt Wolf()
        {
            var p = new DoodlePainter(-1.0f, -0.9f, 1.0f, 1.0f, 110) { Ink = 0.05f };
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Polygon(s * 0.25f, 0.4f, s * 0.75f, 0.95f, s * 0.75f, 0.2f), Rgba.Hex("6B7085"), Rgba.Hex("3F4357"));
            Body(p, Sd.Ellipse(0, 0.1f, 0.75f, 0.65f), Rgba.Hex("8D93A6"), Rgba.Hex("5E6275"));
            Body(p, Sd.Ellipse(0, -0.35f, 0.42f, 0.33f), Rgba.Hex("D9DEE6"), Rgba.Hex("9AA1B4"));
            p.Shape(Sd.Ellipse(0, -0.5f, 0.3f, 0.18f), Rgba.Hex("3A0F1E"), 0.025f);
            for (int i = 0; i < 4; i++) p.Shape(Sd.Polygon(-0.22f + i * 0.14f, -0.38f, -0.16f + i * 0.14f, -0.5f, -0.1f + i * 0.14f, -0.38f), Rgba.White, 0.012f);
            p.Shape(Sd.Ellipse(0, -0.18f, 0.12f, 0.08f), Rgba.Hex("2B2133"), 0.02f);
            foreach (float s in new[] { -1f, 1f }) { p.Shape(Sd.Ellipse(s * 0.27f, 0.22f, 0.11f, 0.08f), Rgba.Hex("FFE14A"), 0.02f); p.Fill(Sd.Ellipse(s * 0.27f, 0.22f, 0.025f, 0.07f), Ink); }
            p.Fill(Sd.Polygon(-0.45f, 0.42f, -0.12f, 0.3f, -0.14f, 0.36f), Ink);
            p.Fill(Sd.Polygon(0.45f, 0.42f, 0.12f, 0.3f, 0.14f, 0.36f), Ink);
            return new DoodleArt("wolf", p);
        }

        /// <summary>Cespuglio scuro (base in 0,0).</summary>
        public static DoodleArt Bush()
        {
            var p = new DoodlePainter(-1.0f, -0.08f, 1.0f, 1.0f, 100) { Ink = 0.05f };
            Body(p, Sd.Union(Sd.Union(Sd.Circle(-0.5f, 0.4f, 0.42f), Sd.Circle(0.45f, 0.4f, 0.45f)), Sd.Union(Sd.Circle(0, 0.6f, 0.4f), Sd.Box(0, 0.2f, 0.9f, 0.2f, 0.1f))),
                Rgba.Hex("1F4A3A"), Rgba.Hex("123228"));
            return new DoodleArt("bush", p);
        }

        // ---------- 95: la torta ----------

        public static DoodleArt Plate()
        {
            var p = new DoodlePainter(-1.7f, -0.25f, 1.7f, 0.12f, 100) { Ink = 0.045f };
            Body(p, Sd.Box(0, -0.06f, 1.6f, 0.1f, 0.08f), Rgba.White, Rgba.Hex("D6E0E8"));
            Body(p, Sd.Box(0, -0.18f, 0.6f, 0.06f, 0.03f), Rgba.Hex("D6E0E8"), Rgba.Hex("AEBBC8"));
            return new DoodleArt("plate", p);
        }

        public static DoodleArt Cherry()
        {
            var p = new DoodlePainter(-0.25f, -0.22f, 0.25f, 0.35f, 160) { Ink = 0.03f };
            p.Line(Rgba.Hex("3E9A35"), 0.025f, 0f, 0.1f, 0.1f, 0.32f);
            Body(p, Sd.Circle(0, 0f, 0.17f), Rgba.Hex("E8233F"), Rgba.Hex("A3122A"));
            p.Fill(Sd.Circle(-0.06f, 0.06f, 0.04f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("cherry", p);
        }

        // ---------- 96: le bolle ----------

        /// <summary>Bolla di sapone iridescente (centro 0,0, raggio 0.5).</summary>
        public static DoodleArt SoapBubble()
        {
            var p = new DoodlePainter(-0.56f, -0.56f, 0.56f, 0.56f, 140) { Wobble = 0f };
            p.FillFunc(Sd.Circle(0, 0, 0.5f), (x, y) =>
            {
                float r = (float)Math.Sqrt(x * x + y * y) / 0.5f;
                Rgba tint = Rgba.Lerp(Rgba.Hex("9FE6FF"), Rgba.Hex("FFB3F0"), Clamp((x + y + 0.5f) / 1f));
                return tint.WithAlpha(0.12f + 0.55f * r * r * r * r);
            });
            p.Line(Rgba.White.WithAlpha(0.85f), 0.04f, -0.3f, 0.2f, -0.18f, 0.32f);
            p.Shape(Sd.Circle(0, 0, 0.5f), Rgba.Clear, 0.015f);
            return new DoodleArt("soap_bubble", p);
        }

        // ---------- 97: il gatto furtivo ----------

        /// <summary>Soggiorno di notte, visto dall'alto in prospettiva: pavimento a listoni blu scuri (coordinate del livello).</summary>
        public static DoodleArt NightLivingRoom()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 4.6f, 7.1f), (x, y) =>
            {
                float f = x / 0.7f - (float)Math.Floor(x / 0.7f);
                return f < 0.04f ? Rgba.Hex("1C2236") : Rgba.Lerp(Rgba.Hex("2B3350"), Rgba.Hex("323B5C"), Noise.Value((float)Math.Floor(x / 0.7f), y * 0.4f));
            });
            p.Fill(Sd.Ellipse(0.4f, -1.2f, 2.2f, 1.4f), Rgba.Hex("5A3A5E", 0.6f));   // tappeto
            return new DoodleArt("night_floor_bg", p);
        }

        /// <summary>Cane da guardia seduto con la torcia in bocca (torcia in 0,0 che punta in basso).</summary>
        public static DoodleArt GuardDog()
        {
            var p = new DoodlePainter(-0.8f, -0.25f, 0.8f, 1.3f, 120) { Ink = 0.045f };
            Body(p, Sd.Ellipse(0, 0.75f, 0.55f, 0.5f), Rgba.Hex("B98A5A"), Rgba.Hex("8A5A3B"));
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Ellipse(s * 0.55f, 0.8f, 0.17f, 0.35f), Rgba.Hex("6B3A1E"), Rgba.Hex("4A2512"));
            Body(p, Sd.Ellipse(0, 0.42f, 0.3f, 0.22f), Rgba.Hex("E9D3B2"), Rgba.Hex("C9AE88"));
            p.Fill(Sd.Ellipse(0, 0.55f, 0.1f, 0.07f), Ink);
            foreach (float s in new[] { -1f, 1f }) { p.Shape(Sd.Circle(s * 0.2f, 0.85f, 0.07f), Rgba.White, 0.015f); p.Fill(Sd.Circle(s * 0.2f, 0.83f, 0.035f), Ink); }
            Body(p, Sd.Box(0, 0.12f, 0.1f, 0.25f, 0.04f), Rgba.Hex("3E6FE0"), Rgba.Hex("2A4FAE"));   // torcia
            p.Shape(Sd.Box(0, -0.12f, 0.13f, 0.06f, 0.02f), Rgba.Hex("FFF27A"), 0.02f);
            return new DoodleArt("guard_dog", p);
        }

        /// <summary>Divano visto dall'alto (centro 0,0, 2.2 x 1).</summary>
        public static DoodleArt Sofa()
        {
            var p = new DoodlePainter(-1.25f, -0.6f, 1.25f, 0.6f, 100) { Ink = 0.05f };
            Body(p, Sd.Box(0, 0, 1.15f, 0.5f, 0.15f), Rgba.Hex("8E5FD1"), Rgba.Hex("6A3FAE"));
            Body(p, Sd.Box(0, 0.32f, 1.15f, 0.18f, 0.1f), Rgba.Hex("7A4FC0"), Rgba.Hex("5A3398"));
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Box(s * 1.0f, 0, 0.15f, 0.5f, 0.08f), Rgba.Hex("7A4FC0"), Rgba.Hex("5A3398"));
            p.InkLine(0.02f, 0f, -0.45f, 0f, 0.12f);
            return new DoodleArt("sofa", p);
        }

        /// <summary>Scatolone di cartone visto dall'alto (centro 0,0, 1 x 0.9).</summary>
        public static DoodleArt CardboardBox()
        {
            var p = new DoodlePainter(-0.6f, -0.55f, 0.6f, 0.55f, 110) { Ink = 0.045f };
            Body(p, Sd.Box(0, 0, 0.5f, 0.45f, 0.04f), Rgba.Hex("D9A86A"), Rgba.Hex("B98A4A"));
            p.InkLine(0.02f, -0.5f, 0f, 0.5f, 0f);
            p.Line(Rgba.Hex("C9B48A"), 0.08f, 0f, -0.45f, 0f, 0.45f);
            return new DoodleArt("box", p);
        }

        /// <summary>Pianta in vaso vista dall'alto (centro 0,0).</summary>
        public static DoodleArt PlantTop()
        {
            var p = new DoodlePainter(-0.65f, -0.65f, 0.65f, 0.65f, 110) { Ink = 0.04f };
            Body(p, Sd.Circle(0, 0, 0.35f), Rgba.Hex("D2693A"), Rgba.Hex("A14A26"));
            for (int i = 0; i < 7; i++)
            {
                double a = i * Math.PI * 2 / 7;
                p.Shape(Sd.Rotate(Sd.Ellipse((float)Math.Cos(a) * 0.32f, (float)Math.Sin(a) * 0.32f, 0.26f, 0.1f), (float)Math.Cos(a) * 0.32f, (float)Math.Sin(a) * 0.32f, (float)(a * 180 / Math.PI)), i % 2 == 0 ? Leaf : LeafShade, 0.025f);
            }
            return new DoodleArt("plant_top", p);
        }

        // ---------- 98: il bowling ----------

        /// <summary>Pista vista dall'alto: legno chiaro tra x -1.5 e 1.5, canaline scure ai lati (coordinate del livello).</summary>
        public static DoodleArt Lane()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.Fill(Sd.Box(0, 0, 4.6f, 7.1f), Rgba.Hex("2B2133"));
            p.Fill(Sd.Box(0, 0, 1.85f, 7.1f), Rgba.Hex("4A4060"));
            p.FillFunc(Sd.Box(0, 0, 1.5f, 7.1f), (x, y) =>
            {
                float f = x / 0.25f - (float)Math.Floor(x / 0.25f);
                return f < 0.06f ? Rgba.Hex("C99A5E") : Rgba.Lerp(Rgba.Hex("E8C08A"), Rgba.Hex("F2D2A2"), Noise.Value((float)Math.Floor(x / 0.25f) * 3f, y * 0.3f));
            });
            p.Shape(Sd.Box(0, 0, 1.5f, 7.1f), Rgba.Clear, 0.05f);
            for (int i = -2; i <= 2; i++) p.Fill(Sd.Polygon(i * 0.45f, -1.6f, i * 0.45f - 0.1f, -1.85f, i * 0.45f + 0.1f, -1.85f), Rgba.Hex("8E5FD1"));   // frecce
            p.Line(Rgba.Hex("E8433F"), 0.05f, -1.5f, -2.6f, 1.5f, -2.6f);   // linea di lancio
            return new DoodleArt("lane_bg", p);
        }

        public static DoodleArt BowlingBall()
        {
            var p = new DoodlePainter(-0.32f, -0.32f, 0.32f, 0.32f, 160) { Ink = 0.035f };
            Body(p, Sd.Circle(0, 0, 0.28f), Rgba.Hex("3E6FE0"), Rgba.Hex("2A4FAE"));
            foreach (var h in new[] { (-0.07f, 0.1f), (0.07f, 0.1f), (0f, -0.04f) }) p.Fill(Sd.Circle(h.Item1, h.Item2, 0.035f), Rgba.Hex("1A1420"));
            return new DoodleArt("bowling_ball", p);
        }

        /// <summary>Birillo visto dall'alto (cerchio bianco con la fascia rossa, centro 0,0, raggio 0.15).</summary>
        public static DoodleArt PinTop()
        {
            var p = new DoodlePainter(-0.2f, -0.2f, 0.2f, 0.2f, 180) { Ink = 0.025f };
            Body(p, Sd.Circle(0, 0, 0.16f), Rgba.White, Rgba.Hex("D6E0E8"));
            p.Shape(Sd.Subtract(Sd.Circle(0, 0, 0.1f), Sd.Circle(0, 0, 0.065f)), Rgba.Hex("E8433F"), 0.01f);
            p.Fill(Sd.Circle(0, 0, 0.04f), Rgba.White);
            return new DoodleArt("pin_top", p);
        }

        /// <summary>Cono stradale "NON TOCCARE" visto dall'alto (centro 0,0, raggio 0.35).</summary>
        public static DoodleArt TrafficCone()
        {
            var p = new DoodlePainter(-0.42f, -0.42f, 0.42f, 0.42f, 150) { Ink = 0.035f };
            p.Shape(Sd.Box(0, 0, 0.36f, 0.36f, 0.06f), Rgba.Hex("FF8A2E"), 0.03f);
            Body(p, Sd.Circle(0, 0, 0.25f), Rgba.Hex("FF8A2E"), Rgba.Hex("D2601A"));
            p.Shape(Sd.Subtract(Sd.Circle(0, 0, 0.18f), Sd.Circle(0, 0, 0.11f)), Rgba.White, 0.015f);
            p.Fill(Sd.Circle(0, 0, 0.05f), Rgba.Hex("2B2133"));
            return new DoodleArt("traffic_cone", p);
        }

        // ---------- 99: acchiappa la talpa ----------

        /// <summary>Buca nel prato (centro 0,0): bordo di terra dietro e davanti (davanti a parte, per coprire la talpa).</summary>
        public static DoodleArt MoleHole(bool front)
        {
            var p = new DoodlePainter(-0.75f, -0.35f, 0.75f, 0.3f, 120) { Ink = 0.045f };
            if (!front) p.Shape(Sd.Ellipse(0, 0, 0.62f, 0.22f), Rgba.Hex("2B1A12"), 0.04f);
            else Body(p, Sd.Subtract(Sd.Ellipse(0, -0.08f, 0.72f, 0.24f), Sd.Ellipse(0, 0.04f, 0.62f, 0.2f)), Rgba.Hex("8A5A3B"), Rgba.Hex("5E3A22"));
            return new DoodleArt(front ? "mole_hole_front" : "mole_hole", p);
        }

        /// <summary>Talpa (base 0,0): 0 arrabbiata, 1 con il cappellino da festa, 2 d'oro.</summary>
        public static DoodleArt Mole(int kind)
        {
            var p = new DoodlePainter(-0.55f, -0.1f, 0.55f, 1.35f, 130) { Ink = 0.04f };
            Rgba fur = kind == 2 ? Gold : Rgba.Hex("8A6A5A"), shade = kind == 2 ? GoldShade : Rgba.Hex("5E4638");
            Body(p, Sd.Box(0, 0.4f, 0.4f, 0.5f, 0.38f), fur, shade);
            Body(p, Sd.Ellipse(0, 0.5f, 0.17f, 0.12f), Rgba.Hex("FFB3C0"), Rgba.Hex("E88A9C"));
            p.Fill(Sd.Ellipse(0, 0.56f, 0.07f, 0.045f), Ink);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Circle(s * 0.16f, 0.7f, 0.045f), Ink);
                if (kind == 0) p.Fill(Sd.Polygon(s * 0.28f, 0.84f, s * 0.06f, 0.76f, s * 0.07f, 0.81f, s * 0.26f, 0.88f), Ink);   // sopracciglia arrabbiate
            }
            if (kind == 0) p.Shape(Sd.Box(0, 0.38f, 0.1f, 0.04f, 0.02f), Rgba.White, 0.015f);   // denti stretti
            else p.InkLine(0.02f, -0.08f, 0.38f, 0f, 0.33f, 0.08f, 0.38f);
            if (kind == 1)
            {
                p.Shape(Sd.Polygon(-0.2f, 0.86f, 0.2f, 0.86f, 0.05f, 1.3f), Rgba.Hex("8E5FD1"), 0.025f);
                p.Fill(Sd.Circle(0.05f, 1.3f, 0.06f), Rgba.Hex("FFE14A"));
                for (int i = 0; i < 3; i++) p.Fill(Sd.Circle(-0.08f + i * 0.08f, 0.95f + i * 0.1f, 0.025f), Rgba.Hex("FFE14A"));
            }
            if (kind == 2) for (int i = 0; i < 3; i++) p.Shape(Sd.Polygon(-0.2f + i * 0.2f, 0.88f, -0.14f + i * 0.2f, 1.1f, -0.08f + i * 0.2f, 0.88f), Gold, 0.02f);
            return new DoodleArt(kind == 0 ? "mole_angry" : kind == 1 ? "mole_party" : "mole_gold", p);
        }

        /// <summary>Martello di gomma (impugnatura in basso a destra, testa in alto a sinistra; perno 0,0).</summary>
        public static DoodleArt Mallet()
        {
            var p = new DoodlePainter(-0.6f, -0.1f, 0.5f, 0.9f, 130) { Ink = 0.04f };
            p.Line(Rgba.Hex("8A5A3B"), 0.07f, 0.45f, 0f, -0.15f, 0.6f);
            Body(p, Sd.Rotate(Sd.Box(-0.25f, 0.7f, 0.3f, 0.16f, 0.06f), -0.25f, 0.7f, 45f), Rgba.Hex("E8433F"), Rgba.Hex("B92E2A"));
            return new DoodleArt("mallet", p);
        }

        // ---------- 100: il trofeo ----------

        /// <summary>Coppa d'oro (base in 0,0, alta 1.3) e varianti: polvere grigia sopra, luccichio.</summary>
        public static DoodleArt Trophy(int layer)
        {
            var p = new DoodlePainter(-0.75f, -0.08f, 0.75f, 1.45f, 130) { Ink = layer == 0 ? 0.045f : 0f };
            Sdf cup = Sd.Union(Sd.Intersect(Sd.Circle(0, 1.05f, 0.42f), Sd.Box(0, 0.9f, 0.45f, 0.35f)), Sd.Box(0, 1.15f, 0.42f, 0.1f));
            Sdf handles = Sd.Union(Sd.Subtract(Sd.Circle(-0.45f, 1.0f, 0.2f), Sd.Circle(-0.45f, 1.0f, 0.11f)), Sd.Subtract(Sd.Circle(0.45f, 1.0f, 0.2f), Sd.Circle(0.45f, 1.0f, 0.11f)));
            Sdf stem = Sd.Union(Sd.Box(0, 0.45f, 0.07f, 0.2f), Sd.Box(0, 0.15f, 0.32f, 0.13f, 0.04f));
            Sdf all = Sd.Union(Sd.Union(cup, handles), stem);
            if (layer == 0)
            {
                Body(p, handles, Gold, GoldShade);
                Body(p, Sd.Union(cup, stem), Gold, GoldShade);
                p.Shape(Sd.Box(0, 0.15f, 0.22f, 0.07f, 0.02f), Rgba.Hex("2B2133"), 0.015f);
                p.Shape(Sd.Polygon(0, 1.22f, 0.06f, 1.08f, 0.2f, 1.08f, 0.09f, 0.99f, 0.13f, 0.85f, 0, 0.94f, -0.13f, 0.85f, -0.09f, 0.99f, -0.2f, 1.08f, -0.06f, 1.08f), Rgba.Hex("FFF6C2"), 0.015f);
            }
            else if (layer == 1)
                p.FillFunc(all, (x, y) => Rgba.Hex("8D8A80").WithAlpha(0.55f + Noise.Value(x * 14f, y * 14f) * 0.4f));
            else
            {
                p.FillFunc(all, (x, y) => Rgba.White.WithAlpha(Clamp(0.5f - Math.Abs(x + y * 0.6f - 0.75f) * 2.2f)));
                foreach (var s in new[] { (-0.5f, 1.35f), (0.55f, 0.6f), (0.2f, 1.4f) })
                    p.Fill(Sd.Polygon(s.Item1, s.Item2 + 0.12f, s.Item1 + 0.03f, s.Item2 + 0.03f, s.Item1 + 0.12f, s.Item2, s.Item1 + 0.03f, s.Item2 - 0.03f, s.Item1, s.Item2 - 0.12f, s.Item1 - 0.03f, s.Item2 - 0.03f, s.Item1 - 0.12f, s.Item2, s.Item1 - 0.03f, s.Item2 + 0.03f), Rgba.White);
            }
            return new DoodleArt(layer == 0 ? "trophy" : layer == 1 ? "trophy_dust" : "trophy_shine", p);
        }

        /// <summary>Cuscino morbido (centro 0,0, largo 1.8).</summary>
        public static DoodleArt Pillow()
        {
            var p = new DoodlePainter(-1.0f, -0.4f, 1.0f, 0.4f, 110) { Ink = 0.045f };
            Body(p, Sd.Box(0, 0, 0.88f, 0.28f, 0.22f), Rgba.Hex("FF8FB0"), Rgba.Hex("E2668C"));
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Circle(s * 0.88f, 0.22f * s, 0.06f), Rgba.Hex("FF8FB0"), 0.02f);
            return new DoodleArt("pillow", p);
        }
    }
}
