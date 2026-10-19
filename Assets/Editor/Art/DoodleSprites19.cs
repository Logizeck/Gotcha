using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli nuovi della versione 0.39 (gruppo B).</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV39B() => new DoodleArt[]
        {
            CraneBackground(), CraneCrate(), CraneHook(), RoofCat(),
            LakeBackground(), LongRod(), Bobber(), Ripple(), Splash(), PondFrog(), PondDuck(), PondLeaf(), FishBoot(),
            SheepNight(), KidBed(false), KidBed(true), SheepFence(), SheepWolf(),
            TableTop(), LockPhone(), LockDot(), LockRing(), LockSmudge(), LockPrint(), Padlock(false), Padlock(true), PizzaApp(),
            HallBackground39(), PartyWide(), PartyWallPiece(), GiraffeBody(), GiraffeNeck(), GiraffeHead(), RotatePhone(),
        };

        static void Cloud39(DoodlePainter p, float x, float y, float s, float alpha = 0.95f)
        {
            Sdf c = Sd.Union(Sd.Ellipse(x, y, 0.75f * s, 0.3f * s),
                Sd.Union(Sd.Circle(x - 0.3f * s, y + 0.15f * s, 0.3f * s), Sd.Circle(x + 0.25f * s, y + 0.2f * s, 0.36f * s)));
            p.Shape(c, Rgba.White.WithAlpha(alpha), 0.03f);
        }

        // ---------- la gru ----------

        /// <summary>Cielo, città lontana, palazzo a destra (tetto a y 0.72, da x 1.0) e gru gialla a sinistra (braccio a y 3.45). Suolo a y -3.6.</summary>
        public static DoodleArt CraneBackground()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 4.6f, 7.1f), (x, y) => Rgba.Lerp(Rgba.Hex("FFE6C7"), Rgba.Hex("7CC4F0"), Clamp((y + 3f) / 7f)));
            Cloud39(p, -1.2f, 2.3f, 0.7f); Cloud39(p, 0.4f, -0.6f, 0.55f); Cloud39(p, -2.2f, 0.4f, 0.45f); Cloud39(p, 2.2f, 2.6f, 0.6f);
            var rnd = new Random(39);
            for (float x = -4.6f; x < 4.6f;)
            {
                float w = 0.5f + (float)rnd.NextDouble() * 0.6f, top = -2.4f + (float)rnd.NextDouble() * 1.6f;
                p.Fill(Sd.Box(x + w * 0.5f, (top - 3.6f) * 0.5f, w * 0.5f - 0.03f, (top + 3.6f) * 0.5f), Rgba.Hex("A9C4E4"), false);
                for (float wy = top - 0.35f; wy > -3.4f; wy -= 0.4f) p.Fill(Sd.Box(x + w * 0.5f, wy, w * 0.25f, 0.06f), Rgba.Hex("C8DAF0"), false);
                x += w;
            }
            // suolo
            Body(p, Sd.Box(0, -5.35f, 4.7f, 1.75f), Rgba.Hex("8C8F9E"), Rgba.Hex("6D7080"));
            p.Line(Rgba.Hex("FFE14A"), 0.08f, -4.4f, -4.6f, -3.6f, -4.6f);
            p.Line(Rgba.Hex("FFE14A"), 0.08f, -2.6f, -4.6f, -1.8f, -4.6f);
            p.Line(Rgba.Hex("FFE14A"), 0.08f, -0.8f, -4.6f, 0f, -4.6f);
            p.Line(Rgba.Hex("FFE14A"), 0.08f, 1.0f, -4.6f, 1.8f, -4.6f);
            // palazzo
            Body(p, Sd.Box(3.0f, -1.45f, 2.0f, 2.17f), Rgba.Hex("D9775A"), Rgba.Hex("B85A40"));
            for (float wy = -2.9f; wy < 0.3f; wy += 0.85f)
                for (float wx = 1.55f; wx < 4.5f; wx += 0.85f)
                    p.Shape(Sd.Box(wx, wy, 0.22f, 0.28f, 0.03f), Rgba.Hex("BFE6FA"), 0.04f);
            Body(p, Sd.Box(3.0f, 0.78f, 2.1f, 0.09f, 0.03f), Rgba.Hex("B7BCC9"), Rgba.Hex("8D93A6"));
            p.InkLine(0.04f, 3.4f, 0.85f, 3.4f, 1.7f);
            p.Fill(Sd.Circle(3.4f, 1.72f, 0.06f), Rgba.Hex("FF4D5E"));
            // gru: torre a traliccio e braccio
            Rgba yel = Rgba.Hex("FFC83D"), yelS = Rgba.Hex("E09A16");
            Sdf tower = Sd.Box(-2.75f, 0f, 0.22f, 3.6f);
            p.Fill(tower, yel);
            for (float y = -3.6f; y < 3.3f; y += 0.44f) p.InkLine(0.035f, -2.95f, y, -2.55f, y + 0.44f);
            p.Shape(tower, Rgba.Clear);
            Sdf jib = Sd.Box(-0.6f, 3.45f, 2.7f, 0.12f);
            p.Fill(jib, yel);
            for (float x = -3.3f; x < 2.0f; x += 0.3f) p.InkLine(0.03f, x, 3.33f, x + 0.15f, 3.57f, x + 0.3f, 3.33f);
            p.Shape(jib, Rgba.Clear);
            Body(p, Sd.Box(-2.75f, 2.95f, 0.36f, 0.3f, 0.05f), yel, yelS);
            p.Shape(Sd.Box(-2.65f, 3.0f, 0.18f, 0.15f, 0.03f), Rgba.Hex("BFE6FA"), 0.03f);
            Body(p, Sd.Box(-2.75f, -3.55f, 0.45f, 0.1f), Rgba.Hex("B7BCC9"), Rgba.Hex("8D93A6"));
            return new DoodleArt("crane_bg", p);
        }

        /// <summary>Cassa di legno 1x1 (centro 0,0) a 9-slice: si allarga/stringe con SpriteRenderer.size.</summary>
        public static DoodleArt CraneCrate()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.5f, 0.5f, 100) { Ink = 0.045f, Wobble = 0.006f };
            Sdf box = Sd.Box(0, 0, 0.46f, 0.46f, 0.05f);
            p.Fill(box, Rgba.Hex("E2AC6B"));
            foreach (float y in new[] { -0.12f, 0.12f }) p.Fill(Sd.Box(0, y, 0.46f, 0.012f), Rgba.Hex("B97A3E"), false);
            p.Shape(Sd.Subtract(box, Sd.Box(0, 0, 0.31f, 0.31f)), Rgba.Hex("C88A4C"), 0.035f);
            foreach (float sx in new[] { -1f, 1f }) foreach (float sy in new[] { -1f, 1f }) p.Fill(Sd.Circle(sx * 0.385f, sy * 0.385f, 0.028f), Ink);
            p.Shape(box, Rgba.Clear);
            return new DoodleArt("crane_crate", p, 22);
        }

        /// <summary>Gancio della gru: la corda arriva in (0, 0.55), la cassa è appesa sotto y 0.</summary>
        public static DoodleArt CraneHook()
        {
            var p = new DoodlePainter(-0.36f, -0.05f, 0.36f, 0.62f, 140) { Ink = 0.035f };
            p.InkLine(0.03f, -0.3f, 0.0f, 0f, 0.22f, 0.3f, 0.0f);
            Body(p, Sd.Box(0, 0.44f, 0.13f, 0.1f, 0.03f), Rgba.Hex("FFC83D"), Rgba.Hex("E09A16"));
            p.Shape(Sd.Subtract(Sd.Circle(0, 0.25f, 0.09f), Sd.Union(Sd.Circle(0, 0.25f, 0.05f), Sd.Box(0.06f, 0.3f, 0.05f, 0.06f))), Rgba.Hex("8D93A6"), 0.03f);
            return new DoodleArt("crane_hook", p);
        }

        /// <summary>Gatto seduto visto di fronte (zampe in 0,0).</summary>
        public static DoodleArt RoofCat()
        {
            var p = new DoodlePainter(-0.45f, -0.05f, 0.6f, 1.15f, 140) { Ink = 0.04f };
            p.Shape(Sd.Polyline(0.06f, 0.25f, 0.08f, 0.45f, 0.2f, 0.48f, 0.45f), CatOrange);
            Body(p, Sd.Ellipse(0, 0.33f, 0.31f, 0.33f), CatOrange, CatShade);
            p.Fill(Sd.Ellipse(0, 0.26f, 0.15f, 0.2f), Rgba.Hex("FFE3C2"));
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Ellipse(s * 0.13f, 0.05f, 0.1f, 0.06f), CatOrange, 0.03f);
            Sdf ears = Sd.Union(Sd.Polygon(-0.27f, 0.86f, -0.24f, 1.1f, -0.06f, 0.98f), Sd.Polygon(0.27f, 0.86f, 0.24f, 1.1f, 0.06f, 0.98f));
            p.Shape(ears, CatOrange);
            p.Fill(Sd.Grow(ears, -0.05f), CatPink);
            Body(p, Sd.Ellipse(0, 0.8f, 0.3f, 0.25f), CatOrange, CatShade);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Ellipse(s * 0.11f, 0.83f, 0.065f, 0.08f), Rgba.White, 0.02f);
                p.Fill(Sd.Circle(s * 0.11f, 0.82f, 0.035f), Ink);
                p.Fill(Sd.Circle(s * 0.11f - 0.012f, 0.84f, 0.012f), Rgba.White);
                for (int i = 0; i < 2; i++) p.InkLine(0.015f, s * 0.15f, 0.72f - i * 0.04f, s * 0.36f, 0.74f - i * 0.07f);
            }
            p.Fill(Sd.Polygon(-0.04f, 0.74f, 0.04f, 0.74f, 0f, 0.7f), CatPink);
            p.InkLine(0.018f, -0.06f, 0.65f, 0f, 0.69f, 0.06f, 0.65f);
            return new DoodleArt("roof_cat", p);
        }

        // ---------- la pesca ----------

        /// <summary>Lago visto dalla riva: cielo, colline e alberi (orizzonte a y 1.8), acqua, riva erbosa sotto y -4.1.</summary>
        public static DoodleArt LakeBackground()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 4.6f, 7.1f), (x, y) => Rgba.Lerp(Rgba.Hex("FFF1D6"), Rgba.Hex("8CCFF2"), Clamp((y - 1.8f) / 3f)));
            Cloud39(p, -1.6f, 3.0f, 0.6f); Cloud39(p, 1.8f, 2.6f, 0.45f);
            Body(p, Sd.Union(Sd.Ellipse(-2.6f, 1.8f, 2.6f, 0.7f), Sd.Ellipse(2.0f, 1.8f, 3.0f, 0.55f)), Rgba.Hex("8FC97A"), Rgba.Hex("6FAE5C"));
            var rnd = new Random(7);
            for (int i = 0; i < 9; i++)
            {
                float x = -4.2f + i * 1.05f + (float)rnd.NextDouble() * 0.3f, h = 0.35f + (float)rnd.NextDouble() * 0.3f;
                p.InkLine(0.05f, x, 1.8f, x, 2.0f + h * 0.4f);
                Body(p, Sd.Ellipse(x, 2.05f + h, 0.28f, h), Rgba.Hex("4E9A55"), Rgba.Hex("3B7A43"));
            }
            Sdf water = Sd.Box(0, -1.4f, 4.6f, 3.2f);
            p.FillFunc(water, (x, y) => Rgba.Lerp(Rgba.Hex("2E7DB2"), Rgba.Hex("86CBE6"), Clamp((y + 4f) / 5.8f)));
            p.Line(Rgba.White.WithAlpha(0.8f), 0.05f, -4.6f, 1.8f, 4.6f, 1.8f);
            for (int i = 0; i < 26; i++)
            {
                float x = -4.3f + (float)rnd.NextDouble() * 8.6f, y = -3.8f + (float)rnd.NextDouble() * 5.4f, w = 0.15f + (y + 4f) * -0.02f + 0.2f;
                p.Line(Rgba.White.WithAlpha(0.35f), 0.03f, x - w, y, x + w, y);
            }
            // ninfee
            foreach (var l in new[] { (-1.9f, -1.0f, 0.5f), (2.1f, 0.9f, 0.3f), (-2.4f, 1.0f, 0.25f), (1.7f, -2.6f, 0.45f) })
            {
                Sdf pad = Sd.Subtract(Sd.Ellipse(l.Item1, l.Item2, l.Item3, l.Item3 * 0.38f), Sd.Polygon(l.Item1, l.Item2, l.Item1 + l.Item3 * 1.2f, l.Item2 + l.Item3 * 0.15f, l.Item1 + l.Item3 * 1.2f, l.Item2 - l.Item3 * 0.3f));
                Body(p, pad, Rgba.Hex("6CC24A"), Rgba.Hex("4E9A35"));
            }
            p.Shape(Sd.Circle(-2.05f, -0.92f, 0.08f), Rgba.Hex("FF9BB0"), 0.03f);
            // canne a destra
            for (int i = 0; i < 6; i++)
            {
                float x = 2.2f + i * 0.13f;
                p.Line(Rgba.Hex("4E9A35"), 0.05f, x, -4.0f, x + 0.05f * (i % 2 == 0 ? 1 : -1), -2.6f + (i % 3) * 0.3f);
                if (i % 2 == 0) p.Shape(Sd.Capsule(x + 0.05f, -2.7f + (i % 3) * 0.3f, x + 0.05f, -2.45f + (i % 3) * 0.3f, 0.05f), Rgba.Hex("8A5A3B"), 0.025f);
            }
            // riva
            Body(p, Sd.Union(Sd.Box(0, -5.7f, 4.7f, 1.6f), Sd.Ellipse(-1.5f, -4.15f, 3.2f, 0.25f)), Rgba.Hex("7ED36B"), Rgba.Hex("5FB04F"));
            for (int i = 0; i < 12; i++) { float x = -4.2f + i * 0.75f; p.Line(Rgba.Hex("4E9A35"), 0.04f, x, -4.5f, x - 0.06f, -4.25f, x + 0.04f, -4.3f); }
            return new DoodleArt("lake_bg", p);
        }

        /// <summary>Canna da pesca lunga: impugnatura in 0,0, punta in (2.3, 3.1).</summary>
        public static DoodleArt LongRod()
        {
            var p = new DoodlePainter(-0.3f, -0.3f, 2.45f, 3.25f, 100) { Ink = 0.04f };
            p.InkLine(0.085f, 0.4f, 0.54f, 2.3f, 3.1f);
            p.Line(Rgba.Hex("3E6FE0"), 0.04f, 0.4f, 0.54f, 2.3f, 3.1f);
            p.Shape(Sd.Capsule(0f, 0f, 0.45f, 0.6f, 0.08f), Rgba.Hex("C98A50"));
            p.Shape(Sd.Circle(0.36f, 0.24f, 0.16f), Rgba.Hex("B7BCC9"), 0.035f);
            p.Fill(Sd.Circle(0.36f, 0.24f, 0.05f), Ink);
            p.InkLine(0.03f, 0.36f, 0.24f, 0.5f, 0.12f);
            foreach (float t in new[] { 0.45f, 0.75f }) p.Shape(Sd.Circle(0.4f + 1.9f * t, 0.54f + 2.56f * t, 0.035f), Rgba.Hex("B7BCC9"), 0.02f);
            return new DoodleArt("rod_long", p);
        }

        /// <summary>Galleggiante rosso e bianco; y 0 = pelo dell'acqua.</summary>
        public static DoodleArt Bobber()
        {
            var p = new DoodlePainter(-0.28f, -0.08f, 0.28f, 0.45f, 160) { Ink = 0.03f };
            p.Fill(Sd.Subtract(Sd.Ellipse(0, 0, 0.26f, 0.06f), Sd.Ellipse(0, 0, 0.19f, 0.035f)), Rgba.White.WithAlpha(0.75f));
            p.InkLine(0.03f, 0, 0.26f, 0, 0.42f);
            Sdf ball = Sd.Intersect(Sd.Circle(0, 0.1f, 0.17f), Sd.Box(0, 0.25f, 0.3f, 0.25f));
            p.Fill(ball, Rgba.White);
            p.Fill(Sd.Intersect(ball, Sd.Box(0, 0.3f, 0.3f, 0.2f)), Rgba.Hex("E8433F"));
            p.Shape(ball, Rgba.Clear);
            p.Fill(Sd.Ellipse(-0.06f, 0.18f, 0.035f, 0.05f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("bobber", p);
        }

        /// <summary>Cerchio d'onda sull'acqua (centro 0,0).</summary>
        public static DoodleArt Ripple()
        {
            var p = new DoodlePainter(-0.55f, -0.2f, 0.55f, 0.2f, 120) { Wobble = 0.004f };
            p.Fill(Sd.Subtract(Sd.Ellipse(0, 0, 0.5f, 0.16f), Sd.Ellipse(0, 0, 0.43f, 0.12f)), Rgba.White.WithAlpha(0.85f));
            return new DoodleArt("ripple", p);
        }

        /// <summary>Spruzzo a corona (base sull'acqua in 0,0).</summary>
        public static DoodleArt Splash()
        {
            var p = new DoodlePainter(-0.6f, -0.08f, 0.6f, 0.8f, 130) { Ink = 0.03f };
            Rgba w = Rgba.Hex("D9F3FF");
            p.Shape(Sd.Polygon(-0.45f, 0f, -0.32f, 0.32f, -0.18f, 0.12f, 0f, 0.45f, 0.18f, 0.12f, 0.32f, 0.32f, 0.45f, 0f), w);
            foreach (var d in new[] { (-0.42f, 0.55f, 0.06f), (-0.15f, 0.66f, 0.05f), (0.12f, 0.7f, 0.06f), (0.4f, 0.52f, 0.05f), (0f, 0.58f, 0.035f) })
                p.Shape(Sd.Circle(d.Item1, d.Item2, d.Item3), w, 0.025f);
            return new DoodleArt("splash", p);
        }

        /// <summary>Rana seduta vista di fronte (zampe in 0,0).</summary>
        public static DoodleArt PondFrog()
        {
            var p = new DoodlePainter(-0.45f, -0.05f, 0.45f, 0.65f, 150) { Ink = 0.035f };
            Rgba g = Rgba.Hex("6CC24A"), gs = Rgba.Hex("4E9A35");
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Ellipse(s * 0.26f, 0.1f, 0.15f, 0.09f), g, gs);
            Body(p, Sd.Ellipse(0, 0.25f, 0.28f, 0.21f), g, gs);
            p.Fill(Sd.Ellipse(0, 0.2f, 0.15f, 0.1f), Rgba.Hex("D4F2A8"));
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Circle(s * 0.15f, 0.48f, 0.1f), g);
                p.Fill(Sd.Circle(s * 0.15f, 0.49f, 0.065f), Rgba.White);
                p.Fill(Sd.Circle(s * 0.15f, 0.48f, 0.035f), Ink);
            }
            p.InkLine(0.025f, -0.14f, 0.3f, 0f, 0.25f, 0.14f, 0.3f);
            p.Fill(Sd.Circle(-0.2f, 0.3f, 0.035f), BlushColor.WithAlpha(0.7f));
            p.Fill(Sd.Circle(0.2f, 0.3f, 0.035f), BlushColor.WithAlpha(0.7f));
            return new DoodleArt("pond_frog", p);
        }

        /// <summary>Paperella che guarda a sinistra; y 0 = pelo dell'acqua.</summary>
        public static DoodleArt PondDuck()
        {
            var p = new DoodlePainter(-0.62f, -0.06f, 0.6f, 0.8f, 140) { Ink = 0.04f };
            Rgba y = Rgba.Hex("FFD84A"), ys = Rgba.Hex("E8B52A");
            Sdf above = Sd.Box(0, 0.5f, 1f, 0.5f);
            Body(p, Sd.Intersect(Sd.Union(Sd.Ellipse(0.1f, 0.12f, 0.42f, 0.26f), Sd.Polygon(0.35f, 0.1f, 0.58f, 0.42f, 0.45f, 0.1f)), above), y, ys);
            p.Shape(Sd.Rotate(Sd.Ellipse(0.18f, 0.2f, 0.2f, 0.08f), 0.18f, 0.2f, -12f), Rgba.Hex("F2C23A"), 0.03f);
            Body(p, Sd.Circle(-0.22f, 0.52f, 0.19f), y, ys);
            p.Shape(Sd.Polygon(-0.36f, 0.55f, -0.6f, 0.49f, -0.37f, 0.44f), Rgba.Hex("FF9A3D"), 0.03f);
            p.Fill(Sd.Circle(-0.27f, 0.58f, 0.035f), Ink);
            p.Fill(Sd.Circle(-0.28f, 0.59f, 0.012f), Rgba.White);
            p.Fill(Sd.Subtract(Sd.Ellipse(0.05f, 0f, 0.55f, 0.06f), Sd.Ellipse(0.05f, 0f, 0.45f, 0.035f)), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("pond_duck", p);
        }

        /// <summary>Foglia secca che galleggia (centro 0,0).</summary>
        public static DoodleArt PondLeaf()
        {
            var p = new DoodlePainter(-0.34f, -0.16f, 0.34f, 0.16f, 160) { Ink = 0.03f };
            Sdf leaf = Sd.Intersect(Sd.Circle(0, -0.22f, 0.32f), Sd.Circle(0, 0.22f, 0.32f));
            Body(p, leaf, Rgba.Hex("F29A3A"), Rgba.Hex("D2702A"));
            p.Line(Rgba.Hex("A8501E"), 0.02f, -0.2f, 0f, 0.18f, 0f);
            for (int i = -1; i <= 1; i++) { p.Line(Rgba.Hex("A8501E"), 0.015f, i * 0.1f, 0f, i * 0.1f + 0.06f, 0.06f); p.Line(Rgba.Hex("A8501E"), 0.015f, i * 0.1f, 0f, i * 0.1f + 0.06f, -0.06f); }
            p.InkLine(0.025f, -0.24f, 0f, -0.33f, 0.03f);
            return new DoodleArt("pond_leaf", p);
        }

        /// <summary>Vecchio scarpone gocciolante con un'alga (centro 0,0).</summary>
        public static DoodleArt FishBoot()
        {
            var p = new DoodlePainter(-0.42f, -0.42f, 0.5f, 0.45f, 140) { Ink = 0.04f };
            Sdf boot = Sd.Union(Sd.Box(-0.12f, 0.08f, 0.18f, 0.3f, 0.04f), Sd.Box(0.08f, -0.2f, 0.36f, 0.13f, 0.1f));
            Body(p, boot, Rgba.Hex("8A6A4A"), Rgba.Hex("664A30"));
            p.Shape(Sd.Box(0.08f, -0.32f, 0.36f, 0.04f, 0.03f), Rgba.Hex("3A2E2A"), 0.03f);
            for (int i = 0; i < 3; i++) p.InkLine(0.02f, -0.02f, 0.18f - i * 0.12f, 0.08f, 0.22f - i * 0.12f);
            p.Line(Rgba.Hex("4E9A35"), 0.04f, -0.25f, 0.38f, -0.32f, 0.15f, -0.24f, -0.05f, -0.34f, -0.25f);
            foreach (var d in new[] { (0.3f, -0.4f), (-0.05f, -0.4f) }) p.Shape(Sd.Circle(d.Item1, d.Item2, 0.025f), Rgba.Hex("8FD3FF"), 0.015f);
            return new DoodleArt("fish_boot", p);
        }

        // ---------- conta le pecore ----------

        /// <summary>Notte stellata e prato del sogno: prato sotto y -2.2 (le pecore corrono a y -2.35).</summary>
        public static DoodleArt SheepNight()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 4.6f, 7.1f), (x, y) => Rgba.Lerp(Rgba.Hex("4A4F9A"), Rgba.Hex("1B1F4A"), Clamp((y + 2f) / 6f)));
            var rnd = new Random(11);
            for (int i = 0; i < 60; i++)
            {
                float x = -4.4f + (float)rnd.NextDouble() * 8.8f, y = -1.5f + (float)rnd.NextDouble() * 8f, r = 0.02f + (float)rnd.NextDouble() * 0.03f;
                p.Fill(Sd.Circle(x, y, r), Rgba.Hex("FFF6C8", 0.5f + (float)rnd.NextDouble() * 0.5f), false);
            }
            p.Shape(Sd.Subtract(Sd.Circle(-2.0f, 0.3f, 0.42f), Sd.Circle(-1.8f, 0.45f, 0.36f)), Rgba.Hex("FFF1A8"));
            // nuvola del sogno intorno al letto
            Sdf dream = Sd.Union(Sd.Ellipse(0, 2.3f, 2.5f, 1.15f), Sd.Union(Sd.Circle(-1.5f, 2.9f, 0.8f), Sd.Circle(1.4f, 2.8f, 0.85f)));
            p.Fill(dream, Rgba.Hex("8E86D8", 0.35f));
            foreach (var c in new[] { (1.5f, 0.75f, 0.16f), (1.85f, 0.25f, 0.12f), (2.1f, -0.15f, 0.08f) }) p.Fill(Sd.Circle(c.Item1, c.Item2, c.Item3), Rgba.Hex("B9B2F0", 0.5f));
            Body(p, Sd.Ellipse(1.5f, -2.3f, 3.6f, 1.0f), Rgba.Hex("2F6A4A"), Rgba.Hex("24563B"));
            Body(p, Sd.Box(0, -4.75f, 4.7f, 2.55f), Rgba.Hex("3F8A4F"), Rgba.Hex("2F6E3D"));
            for (int i = 0; i < 16; i++)
            {
                float x = -4.3f + (float)rnd.NextDouble() * 8.6f, y = -2.8f - (float)rnd.NextDouble() * 1.8f;
                p.Line(Rgba.Hex("5FB04F"), 0.035f, x - 0.08f, y, x, y + 0.15f, x + 0.08f, y);
                if (i % 4 == 0) p.Fill(Sd.Circle(x, y + 0.18f, 0.05f), Rgba.Hex("FFE14A"));
            }
            return new DoodleArt("sheep_night_bg", p);
        }

        /// <summary>Bambino a letto (centro 0,0): sveglio con gli occhi sbarrati, o addormentato.</summary>
        public static DoodleArt KidBed(bool asleep)
        {
            var p = new DoodlePainter(-1.5f, -0.8f, 1.5f, 1.0f, 110) { Ink = 0.045f };
            Rgba wood = Rgba.Hex("B98A5A"), woodS = Rgba.Hex("8A5A3B");
            foreach (float x in new[] { -1.25f, 1.25f }) p.Shape(Sd.Box(x, -0.62f, 0.06f, 0.13f), woodS, 0.03f);
            Body(p, Sd.Box(-1.25f, 0.05f, 0.14f, 0.75f, 0.12f), wood, woodS);
            Body(p, Sd.Box(1.25f, -0.12f, 0.12f, 0.48f, 0.1f), wood, woodS);
            Body(p, Sd.Box(0, -0.35f, 1.18f, 0.17f, 0.06f), Rgba.White, Rgba.Hex("D9DEE6"));
            Body(p, Sd.Ellipse(-0.75f, 0.02f, 0.36f, 0.18f), Rgba.White, Rgba.Hex("D9DEE6"));
            Sdf head = Sd.Circle(-0.7f, 0.28f, 0.27f);
            Body(p, head, Skin, SkinShade);
            p.Shape(Sd.Intersect(Sd.Circle(-0.72f, 0.32f, 0.29f), Sd.Box(-0.72f, 0.56f, 0.4f, 0.15f)), Rgba.Hex("7A4A2A"), 0.035f);
            if (asleep)
            {
                foreach (float s in new[] { -1f, 1f }) p.InkLine(0.03f, -0.7f + s * 0.11f - 0.06f, 0.3f, -0.7f + s * 0.11f, 0.26f, -0.7f + s * 0.11f + 0.06f, 0.3f);
                p.InkLine(0.025f, -0.76f, 0.16f, -0.7f, 0.13f, -0.64f, 0.16f);
                p.Fill(Sd.Circle(-0.9f, 0.2f, 0.04f), BlushColor.WithAlpha(0.7f));
                p.Fill(Sd.Circle(-0.5f, 0.2f, 0.04f), BlushColor.WithAlpha(0.7f));
            }
            else
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    p.Shape(Sd.Ellipse(-0.7f + s * 0.11f, 0.3f, 0.075f, 0.095f), Rgba.White, 0.02f);
                    p.Fill(Sd.Circle(-0.7f + s * 0.11f + 0.025f, 0.27f, 0.035f), Ink);
                    p.InkLine(0.02f, -0.7f + s * 0.11f - 0.06f, 0.41f + (s < 0 ? 0.02f : 0f), -0.7f + s * 0.11f + 0.06f, 0.43f - (s < 0 ? 0f : 0.02f));
                }
                p.InkLine(0.025f, -0.77f, 0.14f, -0.7f, 0.17f, -0.63f, 0.14f);
                // borse sotto gli occhi
                foreach (float s in new[] { -1f, 1f }) p.Line(Rgba.Hex("B98AC8"), 0.015f, -0.7f + s * 0.11f - 0.05f, 0.2f, -0.7f + s * 0.11f + 0.05f, 0.2f);
            }
            Sdf blanket = Sd.Box(0.3f, -0.2f, 0.92f, 0.22f, 0.12f);
            Body(p, blanket, Rgba.Hex("5B8DEF"), Rgba.Hex("3F6CC8"));
            for (int i = 0; i < 6; i++) p.Fill(Sd.Circle(-0.4f + i * 0.28f, -0.25f + (i % 2) * 0.13f, 0.035f), Rgba.Hex("FFE14A"));
            return new DoodleArt(asleep ? "kid_bed_sleep" : "kid_bed", p);
        }

        /// <summary>Staccionata (base in 0,0): pali alti 0.97, traversa bassa da 0.45 a 0.55, alta da 0.77 a 0.87.</summary>
        public static DoodleArt SheepFence()
        {
            var p = new DoodlePainter(-0.8f, -0.05f, 0.8f, 1.05f, 120) { Ink = 0.04f };
            Rgba w = Rgba.Hex("D9A066"), ws = Rgba.Hex("B07840");
            foreach (float x in new[] { -0.62f, 0f, 0.62f }) Body(p, Sd.Union(Sd.Box(x, 0.43f, 0.075f, 0.43f), Sd.Polygon(x - 0.075f, 0.85f, x + 0.075f, 0.85f, x, 0.97f)), w, ws);
            Body(p, Sd.Box(0, 0.5f, 0.75f, 0.05f, 0.02f), w, ws);
            Body(p, Sd.Box(0, 0.82f, 0.75f, 0.05f, 0.02f), w, ws);
            return new DoodleArt("sheep_fence", p);
        }

        /// <summary>Lupo travestito da pecora: stesso vello, ma orecchie a punta, muso grigio e coda folta (zampe in y 0).</summary>
        public static DoodleArt SheepWolf()
        {
            var p = new DoodlePainter(-1.0f, -0.05f, 0.95f, 1.05f, 130) { Ink = 0.04f };
            Rgba grey = Rgba.Hex("8D93A6"), greyS = Rgba.Hex("5E6275");
            Body(p, Sd.Rotate(Sd.Ellipse(-0.72f, 0.42f, 0.25f, 0.11f), -0.72f, 0.42f, 25f), grey, greyS);
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.9f, 0.34f, 0.08f, 0.06f), -0.9f, 0.34f, 25f), Rgba.White);
            foreach (float x in new[] { -0.35f, -0.15f, 0.2f, 0.38f }) p.Line(greyS, 0.07f, x, 0.35f, x, 0.03f);
            Sdf wool = Sd.Union(Sd.Union(Sd.Circle(-0.35f, 0.55f, 0.25f), Sd.Circle(0f, 0.62f, 0.3f)), Sd.Union(Sd.Circle(0.3f, 0.55f, 0.25f), Sd.Ellipse(0, 0.48f, 0.55f, 0.22f)));
            Body(p, wool, Rgba.White, Rgba.Hex("D9DEE6"));
            foreach (float s in new[] { 0f, 0.14f }) Body(p, Sd.Polygon(0.42f + s, 0.7f, 0.46f + s, 1.0f, 0.56f + s, 0.72f), grey, greyS);
            Body(p, Sd.Union(Sd.Ellipse(0.56f, 0.6f, 0.18f, 0.14f), Sd.Polygon(0.6f, 0.68f, 0.92f, 0.56f, 0.88f, 0.5f, 0.6f, 0.48f)), grey, greyS);
            p.Fill(Sd.Circle(0.9f, 0.55f, 0.04f), Ink);
            p.Shape(Sd.Polygon(0.72f, 0.5f, 0.76f, 0.44f, 0.8f, 0.5f), Rgba.White, 0.012f);
            p.Shape(Sd.Ellipse(0.6f, 0.65f, 0.045f, 0.035f), Rgba.Hex("FFE14A"), 0.012f);
            p.Fill(Sd.Ellipse(0.6f, 0.65f, 0.012f, 0.03f), Ink);
            p.InkLine(0.02f, 0.52f, 0.72f, 0.66f, 0.69f);
            return new DoodleArt("sheep_wolf", p);
        }

        // ---------- il telefono della mamma ----------

        /// <summary>Tavolo di legno visto dall'alto, con tazza e penna.</summary>
        public static DoodleArt TableTop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            Rgba[] planks = { Rgba.Hex("D9A066"), Rgba.Hex("CF955A"), Rgba.Hex("D69C5F") };
            p.FillFunc(Sd.Box(0, 0, 4.6f, 7.1f), (x, y) =>
            {
                int k = (int)Math.Floor((x + 4.6f) / 1.15f);
                float grain = (float)Math.Sin(y * 3f + k * 2.1f + Math.Sin(y * 0.7f + k) * 2f) * 0.5f + 0.5f;
                return Rgba.Lerp(planks[((k % 3) + 3) % 3], Rgba.Hex("B97A3E"), grain * 0.18f);
            });
            for (float x = -4.6f + 1.15f; x < 4.6f; x += 1.15f) p.Line(Rgba.Hex("8A5A3B"), 0.04f, x, -7f, x, 7f);
            // tazza (dall'alto)
            p.Shape(Sd.Circle(2.45f, 2.9f, 0.62f), Rgba.White);
            p.Shape(Sd.Circle(2.45f, 2.9f, 0.48f), Rgba.Hex("6B3A1E"), 0.03f);
            p.Shape(Sd.Box(1.75f, 2.9f, 0.2f, 0.1f, 0.06f), Rgba.White);
            p.Fill(Sd.Subtract(Sd.Circle(-2.2f, -4.2f, 0.55f), Sd.Circle(-2.2f, -4.2f, 0.47f)), Rgba.Hex("8A5A3B", 0.35f));
            // penna
            p.Shape(Sd.Rotate(Sd.Box(-2.3f, 2.4f, 0.75f, 0.07f, 0.05f), -2.3f, 2.4f, -30f), Rgba.Hex("3E6FE0"), 0.035f);
            return new DoodleArt("table_bg", p);
        }

        /// <summary>Telefono (centro 0,0), schermo spento da x ±1.52, y ±3.15.</summary>
        public static DoodleArt LockPhone()
        {
            var p = new DoodlePainter(-1.85f, -3.5f, 1.85f, 3.5f, 80) { Ink = 0.05f };
            Body(p, Sd.Box(0, 0, 1.7f, 3.38f, 0.32f), Rgba.Hex("2E2E3A"), Rgba.Hex("1E1E28"));
            foreach (float y in new[] { 1.8f, 1.1f }) p.Shape(Sd.Box(1.74f, y, 0.04f, 0.25f, 0.02f), Rgba.Hex("2E2E3A"), 0.03f);
            Sdf screen = Sd.Box(0, 0, 1.52f, 3.15f, 0.2f);
            p.FillFunc(screen, (x, y) => Rgba.Lerp(Rgba.Hex("1C2648"), Rgba.Hex("4A3484"), Clamp((y + 3f) / 6.2f)));
            p.Fill(Sd.Intersect(screen, Sd.Circle(-1.2f, 2.0f, 1.3f)), Rgba.Hex("7A5CC8", 0.25f));
            p.Fill(Sd.Intersect(screen, Sd.Circle(1.4f, -0.4f, 1.1f)), Rgba.Hex("3C8AD8", 0.18f));
            p.Fill(Sd.Circle(0, 3.0f, 0.06f), Rgba.Hex("0A0A12"));
            p.Fill(Sd.Box(0, -2.98f, 0.45f, 0.03f, 0.03f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("phone_lock", p);
        }

        public static DoodleArt LockDot()
        {
            var p = new DoodlePainter(-0.13f, -0.13f, 0.13f, 0.13f, 200) { Wobble = 0f };
            p.Fill(Sd.Circle(0, 0, 0.09f), Rgba.White);
            return new DoodleArt("lock_dot", p);
        }

        public static DoodleArt LockRing()
        {
            var p = new DoodlePainter(-0.32f, -0.32f, 0.32f, 0.32f, 160) { Wobble = 0f };
            p.Fill(Sd.Circle(0, 0, 0.28f), Rgba.White.WithAlpha(0.25f));
            p.Fill(Sd.Subtract(Sd.Circle(0, 0, 0.28f), Sd.Circle(0, 0, 0.23f)), Rgba.White);
            return new DoodleArt("lock_ring", p);
        }

        /// <summary>Strisciata d'unto lunga 0.84 tra i centri (asse x), semitrasparente e striata.</summary>
        public static DoodleArt LockSmudge()
        {
            var p = new DoodlePainter(-0.58f, -0.18f, 0.58f, 0.18f, 120) { Wobble = 0f };
            Sdf core = Sd.Capsule(-0.42f, 0, 0.42f, 0, 0.11f);
            p.FillFunc(Sd.Grow(core, 0.03f), (x, y) =>
            {
                float d = core(x, y);
                float edge = Clamp((0.03f - d) / 0.1f);
                float streak = 0.65f + 0.35f * (float)Math.Sin(y * 95f + Math.Sin(x * 9f) * 1.5f);
                return Rgba.Hex("F4EEDC").WithAlpha(0.4f * edge * streak);
            });
            return new DoodleArt("lock_smudge", p);
        }

        /// <summary>Ditata (impronta) semitrasparente, centro 0,0.</summary>
        public static DoodleArt LockPrint()
        {
            var p = new DoodlePainter(-0.32f, -0.36f, 0.32f, 0.36f, 140) { Wobble = 0f };
            p.FillFunc(Sd.Ellipse(0, 0, 0.27f, 0.32f), (x, y) =>
            {
                float r = (float)Math.Sqrt((x / 0.27f) * (x / 0.27f) + (y / 0.32f) * (y / 0.32f));
                float ridge = 0.5f + 0.5f * (float)Math.Cos(r * 42f + x * 6f);
                return Rgba.Hex("F4EEDC").WithAlpha((0.2f + 0.35f * ridge) * Clamp((1f - r) * 3f));
            });
            return new DoodleArt("lock_print", p);
        }

        /// <summary>Lucchetto bianco (centro 0,0), chiuso o aperto.</summary>
        public static DoodleArt Padlock(bool open)
        {
            var p = new DoodlePainter(-0.3f, -0.3f, 0.3f, 0.5f, 160) { Ink = 0.03f };
            float lift = open ? 0.14f : 0f, shift = open ? 0.11f : 0f;
            Sdf shackle = Sd.Union(Sd.Intersect(Sd.Subtract(Sd.Circle(shift, 0.12f + lift, 0.13f), Sd.Circle(shift, 0.12f + lift, 0.075f)), Sd.Box(shift, 0.25f + lift, 0.2f, 0.13f)),
                Sd.Union(Sd.Box(shift - 0.1f, 0.06f + lift * 0.5f, 0.025f, 0.08f + lift * 0.5f), Sd.Box(shift + 0.1f, 0.08f + lift, 0.025f, 0.06f)));
            p.Shape(shackle, Rgba.Hex("E9ECF5"), 0.025f);
            Body(p, Sd.Box(0, -0.1f, 0.2f, 0.16f, 0.04f), Rgba.White, Rgba.Hex("C3C8D6"));
            p.Fill(Sd.Circle(0, -0.07f, 0.035f), Ink);
            p.Fill(Sd.Box(0, -0.13f, 0.012f, 0.05f), Ink);
            return new DoodleArt(open ? "padlock_open" : "padlock", p);
        }

        /// <summary>Schermata dell'app della pizza (stesse misure dello schermo del telefono): testata rossa, pizza, pulsante verde a y -1.9.</summary>
        public static DoodleArt PizzaApp()
        {
            var p = new DoodlePainter(-1.55f, -3.18f, 1.55f, 3.18f, 80) { Ink = 0.05f };
            Sdf screen = Sd.Box(0, 0, 1.52f, 3.15f, 0.2f);
            p.Fill(screen, Rgba.Hex("FFF4E0"));
            p.Fill(Sd.Intersect(screen, Sd.Box(0, 2.6f, 1.6f, 0.58f)), Rgba.Hex("E8433F"));
            Body(p, Sd.Circle(0, 0.5f, 1.05f), Rgba.Hex("E9B26B"), Rgba.Hex("C98A40"));
            p.Shape(Sd.Circle(0, 0.5f, 0.88f), Rgba.Hex("E8533F"), 0.03f);
            var rnd = new Random(5);
            for (int i = 0; i < 9; i++)
            {
                double a = i * 2.4; float r = 0.25f + (float)rnd.NextDouble() * 0.5f;
                p.Shape(Sd.Ellipse((float)Math.Cos(a) * r, 0.5f + (float)Math.Sin(a) * r, 0.16f, 0.11f), Rgba.Hex("FFE08A"), 0.02f);
            }
            for (int i = 0; i < 6; i++)
            {
                double a = i * 1.05 + 0.4; float r = i % 2 == 0 ? 0.55f : 0.3f;
                p.Shape(Sd.Circle((float)Math.Cos(a) * r, 0.5f + (float)Math.Sin(a) * r, 0.11f), Rgba.Hex("B92E2A"), 0.025f);
            }
            Body(p, Sd.Box(0, -1.9f, 1.1f, 0.4f, 0.2f), Rgba.Hex("5DBB4A"), Rgba.Hex("3E9A35"));
            p.Shape(screen, Rgba.Clear, 0.03f);
            return new DoodleArt("pizza_app", p);
        }

        // ---------- la giraffa ----------

        static readonly Rgba Giraffe = Rgba.Hex("F6C453"), GiraffeShade = Rgba.Hex("E0A43A"), GiraffeSpot = Rgba.Hex("B8682E");

        /// <summary>Corridoio: parete calda, zoccolo, pavimento sotto floorY; porta della festa (vano cx, da floorY a top, semilarghezza hw).</summary>
        static void Hallway(DoodlePainter p, float floorY, float doorX, float doorHw, float doorTop, bool withDoorInside)
        {
            p.FillFunc(Sd.Box(0, 0, 9f, 9f), (x, y) =>
            {
                if (y < floorY) return ((int)Math.Floor(x / 0.9f + 50) % 2 == 0) ? Rgba.Hex("B97A45") : Rgba.Hex("AD6F3C");
                if (y < floorY + 1.1f) return ((int)Math.Floor(x / 0.45f + 100) % 2 == 0) ? Rgba.Hex("9FC7A0") : Rgba.Hex("94BD95");
                return ((int)Math.Floor(x / 0.6f + 100) % 2 == 0) ? Rgba.Hex("FFE9C7") : Rgba.Hex("FBE0B8");
            });
            p.Fill(Sd.Box(0, floorY + 1.1f, 9f, 0.04f), Rgba.Hex("6E8F70"), false);
            p.Fill(Sd.Box(0, floorY, 9f, 0.05f), Rgba.Hex("6B3A1E"), false);
            float hh = (doorTop - floorY) * 0.5f, cy = floorY + hh;
            if (withDoorInside)
            {
                Sdf opening = Sd.Box(doorX, cy, doorHw, hh);
                p.Fill(opening, Rgba.Hex("3A2A5E"), false);
                var rnd = new Random(21);
                Rgba[] party = { Rgba.Hex("FF5C8A"), Rgba.Hex("FFD25A"), Rgba.Hex("6EC6FF"), Rgba.Hex("7ED36B"), Rgba.Hex("B9A3FF") };
                for (int i = 0; i < 40; i++)
                {
                    float x = doorX - doorHw + (float)rnd.NextDouble() * doorHw * 2f, y = floorY + (float)rnd.NextDouble() * hh * 2f;
                    p.Fill(Sd.Intersect(opening, Sd.Circle(x, y, 0.05f + (float)rnd.NextDouble() * 0.06f)), party[i % party.Length].WithAlpha(0.85f), false);
                }
                p.Fill(Sd.Intersect(opening, Sd.Box(doorX, floorY + 0.15f, doorHw, 0.15f)), Rgba.Hex("5A3A7E"), false);
            }
            Sdf frame = Sd.Subtract(Sd.Box(doorX, cy + 0.07f, doorHw + 0.16f, hh + 0.07f), Sd.Box(doorX, cy - 0.1f, doorHw, hh + 0.1f));
            p.Seed = 500;
            Body(p, frame, Rgba.Hex("C98A50"), Rgba.Hex("A06A38"));
            // festoni sopra la porta
            Rgba[] flags = { Rgba.Hex("FF5C8A"), Rgba.Hex("FFD25A"), Rgba.Hex("6EC6FF"), Rgba.Hex("7ED36B") };
            float fy = doorTop + 0.45f;
            p.InkLine(0.025f, doorX - doorHw - 0.4f, fy, doorX, fy - 0.2f, doorX + doorHw + 0.4f, fy);
            for (int i = 0; i < 6; i++)
            {
                float t = (i + 0.5f) / 6f, x = doorX - doorHw - 0.4f + t * (doorHw * 2f + 0.8f), y = fy - 0.2f * (1f - Math.Abs(t * 2f - 1f));
                p.Shape(Sd.Polygon(x - 0.14f, y, x + 0.14f, y, x, y - 0.28f), flags[i % flags.Length], 0.025f);
            }
        }

        /// <summary>Corridoio verticale: pavimento a y -3.2, porta della festa in x 1.75 (vano largo 1.4, alto fino a -0.6).</summary>
        public static DoodleArt HallBackground39()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            Hallway(p, -3.2f, 1.75f, 0.7f, -0.6f, true);
            foreach (var f in new[] { (-1.9f, 1.6f), (1.6f, 2.6f) })
            {
                p.Shape(Sd.Box(f.Item1, f.Item2, 0.45f, 0.35f, 0.04f), Rgba.Hex("8A5A3B"));
                p.Fill(Sd.Box(f.Item1, f.Item2, 0.33f, 0.24f), Rgba.Hex("BFE6FA"));
                p.Fill(Sd.Ellipse(f.Item1, f.Item2 - 0.12f, 0.33f, 0.12f), Rgba.Hex("7ED36B"));
            }
            return new DoodleArt("giraffe_hall_bg", p);
        }

        /// <summary>Corridoio orizzontale (per il telefono girato): pavimento a y -2.3, porta in x 3.3 (vano largo 1.6, alto fino a 0.6).</summary>
        public static DoodleArt PartyWide()
        {
            var p = new DoodlePainter(-6.6f, -3.9f, 6.6f, 3.9f, 50) { Ink = 0.06f };
            Hallway(p, -2.3f, 3.3f, 0.8f, 0.6f, true);
            foreach (var f in new[] { (-2.6f, 1.7f), (0.4f, 1.9f) })
            {
                p.Shape(Sd.Box(f.Item1, f.Item2, 0.45f, 0.35f, 0.04f), Rgba.Hex("8A5A3B"));
                p.Fill(Sd.Box(f.Item1, f.Item2, 0.33f, 0.24f), Rgba.Hex("FFD1DF"));
                p.Fill(Sd.Circle(f.Item1, f.Item2, 0.12f), Rgba.Hex("FF5C8A"));
            }
            return new DoodleArt("party_wide_bg", p);
        }

        /// <summary>Pezzo di parete a destra del vano (stesse coordinate di party_wide_bg): copre la giraffa che entra.</summary>
        public static DoodleArt PartyWallPiece()
        {
            var p = new DoodlePainter(4.1f, -3.9f, 6.6f, 3.9f, 50) { Ink = 0.06f };
            Hallway(p, -2.3f, 3.3f, 0.8f, 0.6f, false);
            return new DoodleArt("party_wall_piece", p);
        }

        /// <summary>Corpo della giraffa che guarda a destra (zoccoli in y 0); il collo si attacca in (0.75, 1.85).</summary>
        public static DoodleArt GiraffeBody()
        {
            var p = new DoodlePainter(-1.55f, -0.05f, 1.45f, 2.3f, 100) { Ink = 0.05f };
            p.InkLine(0.04f, -0.95f, 1.75f, -1.3f, 1.0f);
            p.Shape(Sd.Ellipse(-1.32f, 0.92f, 0.08f, 0.14f), Rgba.Hex("6B3A1E"), 0.03f);
            foreach (float x in new[] { -0.85f, -0.5f, 0.42f, 0.78f })
            {
                Body(p, Sd.Union(Sd.Box(x, 0.7f, 0.09f, 0.65f, 0.05f), Sd.Circle(x, 0.68f, 0.11f)), Giraffe, GiraffeShade);
                p.Shape(Sd.Box(x, 0.06f, 0.1f, 0.06f, 0.02f), Rgba.Hex("4A2A1A"), 0.03f);
            }
            Sdf body = Sd.Union(Sd.Ellipse(-0.05f, 1.5f, 1.05f, 0.5f), Sd.Ellipse(0.55f, 1.65f, 0.5f, 0.45f));
            p.Fill(body, Giraffe);
            var rnd = new Random(4);
            for (int i = 0; i < 9; i++)
            {
                float x = -0.9f + (i % 5) * 0.42f + (float)rnd.NextDouble() * 0.1f, y = 1.3f + (i / 5) * 0.38f + (float)rnd.NextDouble() * 0.08f;
                p.Fill(Sd.Intersect(body, Sd.Rotate(Sd.Box(x, y, 0.14f, 0.11f, 0.05f), x, y, i * 27f)), GiraffeSpot);
            }
            p.Fill(Sd.Crescent(body, -0.06f, 0.08f), GiraffeShade.WithAlpha(0.7f));
            p.Shape(body, Rgba.Clear);
            return new DoodleArt("giraffe_body", p);
        }

        /// <summary>Collo lunghissimo della giraffa: base in 0,0, punta (dove va la testa) in (0, 5.5). Criniera a sinistra.</summary>
        public static DoodleArt GiraffeNeck()
        {
            var p = new DoodlePainter(-0.42f, -0.25f, 0.36f, 5.7f, 60) { Ink = 0.05f };
            Sdf neck = Sd.Polygon(-0.3f, -0.2f, 0.3f, -0.2f, 0.17f, 5.55f, -0.17f, 5.55f);
            float[] mane = new float[30];
            for (int i = 0; i < 15; i++) { float y = i * 5.4f / 14f; mane[i * 2] = -0.29f + y * 0.023f - (i % 2) * 0.09f; mane[i * 2 + 1] = y; }
            p.Line(Rgba.Hex("A0582A"), 0.07f, mane);
            p.Fill(neck, Giraffe);
            for (int i = 0; i < 9; i++)
            {
                float y = 0.25f + i * 0.6f, x = (i % 2 == 0 ? -0.06f : 0.07f);
                p.Fill(Sd.Intersect(neck, Sd.Rotate(Sd.Box(x, y, 0.1f, 0.13f, 0.05f), x, y, i * 31f)), GiraffeSpot);
            }
            p.Fill(Sd.Crescent(neck, -0.07f, 0f), GiraffeShade.WithAlpha(0.7f));
            p.Shape(neck, Rgba.Clear);
            return new DoodleArt("giraffe_neck", p);
        }

        /// <summary>Testa della giraffa che guarda a destra; si attacca al collo in 0,0.</summary>
        public static DoodleArt GiraffeHead()
        {
            var p = new DoodlePainter(-0.45f, -0.35f, 1.0f, 0.9f, 120) { Ink = 0.045f };
            foreach (float x in new[] { 0.02f, 0.2f })
            {
                p.InkLine(0.07f, x, 0.3f, x - 0.04f, 0.68f);
                p.Fill(Sd.Circle(x - 0.04f, 0.7f, 0.075f), Rgba.Hex("6B3A1E"));
            }
            p.Shape(Sd.Rotate(Sd.Ellipse(-0.2f, 0.32f, 0.2f, 0.08f), -0.2f, 0.32f, 25f), Giraffe);
            Sdf head = Sd.Union(Sd.Ellipse(0.2f, 0.2f, 0.36f, 0.27f), Sd.Ellipse(0.62f, 0.06f, 0.3f, 0.2f));
            Body(p, head, Giraffe, GiraffeShade);
            p.Shape(Sd.Ellipse(0.7f, 0.03f, 0.24f, 0.15f), Rgba.Hex("F8DCA0"), 0.03f);
            p.Fill(Sd.Ellipse(0.82f, 0.08f, 0.03f, 0.02f), Ink);
            p.InkLine(0.025f, 0.55f, -0.05f, 0.7f, -0.08f, 0.85f, -0.03f);
            p.Shape(Sd.Ellipse(0.3f, 0.27f, 0.07f, 0.08f), Rgba.White, 0.02f);
            p.Fill(Sd.Circle(0.32f, 0.26f, 0.04f), Ink);
            p.Fill(Sd.Circle(0.31f, 0.28f, 0.013f), Rgba.White);
            p.InkLine(0.02f, 0.24f, 0.35f, 0.2f, 0.4f); p.InkLine(0.02f, 0.3f, 0.36f, 0.3f, 0.42f); p.InkLine(0.02f, 0.36f, 0.35f, 0.4f, 0.4f);
            p.Fill(Sd.Circle(0.45f, 0.1f, 0.045f), BlushColor.WithAlpha(0.6f));
            return new DoodleArt("giraffe_head", p);
        }

        /// <summary>Icona "gira il telefono": telefono inclinato e freccia curva (centro 0,0).</summary>
        public static DoodleArt RotatePhone()
        {
            var p = new DoodlePainter(-0.65f, -0.65f, 0.65f, 0.65f, 140) { Ink = 0.04f };
            p.Fill(Sd.Circle(0, 0, 0.62f), Rgba.Hex("2B2133", 0.55f), false);
            Sdf phone = Sd.Rotate(Sd.Box(0, 0, 0.17f, 0.3f, 0.05f), 0, 0, -35f);
            p.Shape(phone, Rgba.White);
            p.Fill(Sd.Rotate(Sd.Box(0, 0, 0.11f, 0.22f, 0.02f), 0, 0, -35f), Rgba.Hex("6EC6FF"));
            float[] arc = new float[22];
            for (int i = 0; i < 11; i++) { double a = Math.PI * (0.15 + 0.6 * i / 10.0); arc[i * 2] = (float)Math.Cos(a) * 0.46f; arc[i * 2 + 1] = (float)Math.Sin(a) * 0.46f; }
            p.Line(Rgba.Hex("FFD25A"), 0.07f, arc);
            float ex = arc[20], ey = arc[21];
            p.Fill(Sd.Polygon(ex - 0.12f, ey + 0.06f, ex + 0.08f, ey + 0.1f, ex - 0.02f, ey - 0.12f), Rgba.Hex("FFD25A"));
            return new DoodleArt("rotate_phone", p);
        }
    }
}
