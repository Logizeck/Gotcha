using System;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni della versione 0.28 (livelli 54, 71, 77, 78, 79, 80, 82, 89, 95, 98, 100).</summary>
    public static partial class DoodleSprites
    {
        // ---------- 54: il puntatore laser ----------

        /// <summary>Penna laser che punta a destra (punta in 0,0).</summary>
        public static DoodleArt LaserPen()
        {
            var p = new DoodlePainter(-1.0f, -0.2f, 0.1f, 0.2f, 140) { Ink = 0.035f };
            Body(p, Sd.Box(-0.5f, 0, 0.45f, 0.11f, 0.08f), Rgba.Hex("3E6FE0"), Rgba.Hex("2A4FAE"));
            p.Shape(Sd.Box(-0.02f, 0, 0.06f, 0.08f, 0.02f), Rgba.Hex("C9CFD6"), 0.02f);
            p.Shape(Sd.Box(-0.55f, 0.11f, 0.08f, 0.03f, 0.02f), Rgba.Hex("E8433F"), 0.015f);
            return new DoodleArt("laser_pen", p);
        }

        /// <summary>Specchietto girevole su piedistallo, inclinato a "/" (centro 0,0, cella 1).</summary>
        public static DoodleArt MirrorTile()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.5f, 0.5f, 140) { Ink = 0.03f };
            p.Shape(Sd.Circle(0, 0, 0.16f), Rgba.Hex("8D93A6"), 0.025f);
            Sdf glass = Sd.Rotate(Sd.Box(0, 0, 0.42f, 0.06f, 0.03f), 0, 0, 45f);
            Body(p, glass, Rgba.Hex("D9EEF7"), Rgba.Hex("9CC3D6"));
            p.Line(Rgba.White, 0.025f, -0.22f, -0.16f, 0.16f, 0.22f);
            return new DoodleArt("mirror_tile", p);
        }

        public static DoodleArt Vase()
        {
            var p = new DoodlePainter(-0.4f, -0.45f, 0.4f, 0.45f, 140) { Ink = 0.035f };
            Body(p, Sd.Union(Sd.Ellipse(0, -0.12f, 0.3f, 0.3f), Sd.Box(0, 0.22f, 0.12f, 0.18f, 0.03f)), Rgba.Hex("6FC3F2"), Rgba.Hex("2F7DB5"));
            p.Shape(Sd.Box(0, 0.38f, 0.17f, 0.04f, 0.02f), Rgba.Hex("6FC3F2"), 0.02f);
            p.Line(Rgba.White.WithAlpha(0.8f), 0.03f, -0.25f, -0.1f, -0.12f, 0.08f, 0.15f, -0.1f, 0.25f, 0.06f);
            return new DoodleArt("vase", p);
        }

        // ---------- 77: l'idraulico ----------

        /// <summary>Tessere dei tubi (centro 0,0, cella 1.1): 0 dritto ovest-est, 1 curva nord-est, 2 a T ovest-nord-est.</summary>
        public static DoodleArt PipeTile(int kind)
        {
            var p = new DoodlePainter(-0.55f, -0.55f, 0.55f, 0.55f, 120) { Ink = 0.035f };
            p.Shape(Sd.Box(0, 0, 0.52f, 0.52f, 0.06f), Rgba.Hex("E9E4D6"), 0.02f);
            Sdf pipe = kind == 0 ? Sd.Box(0, 0, 0.56f, 0.15f)
                : kind == 1 ? Sd.Union(Sd.Box(0, 0.28f, 0.15f, 0.29f), Sd.Box(0.28f, 0, 0.29f, 0.15f))
                : Sd.Union(Sd.Box(0, 0, 0.56f, 0.15f), Sd.Box(0, 0.28f, 0.15f, 0.29f));
            if (kind == 1) pipe = Sd.Union(pipe, Sd.Circle(0, 0, 0.15f));
            Body(p, Sd.Intersect(pipe, Sd.Box(0, 0, 0.52f, 0.52f)), Rgba.Hex("D9DEE6"), Rgba.Hex("9AA1B4"));
            return new DoodleArt(kind == 0 ? "pipe_straight" : kind == 1 ? "pipe_corner" : "pipe_tee", p);
        }

        /// <summary>Rubinetto a muro che esce verso destra (bocca in 0,0).</summary>
        public static DoodleArt Faucet()
        {
            var p = new DoodlePainter(-0.7f, -0.35f, 0.1f, 0.5f, 130) { Ink = 0.04f };
            Body(p, Sd.Box(-0.3f, 0, 0.32f, 0.14f, 0.05f), Rgba.Hex("C9CFD6"), Rgba.Hex("8D93A6"));
            Body(p, Sd.Box(-0.35f, 0.25f, 0.04f, 0.12f), Rgba.Hex("8D93A6"), Rgba.Hex("5E6275"));
            Body(p, Sd.Box(-0.35f, 0.38f, 0.18f, 0.05f, 0.03f), Rgba.Hex("E8433F"), Rgba.Hex("B92E2A"));
            return new DoodleArt("faucet", p);
        }

        // ---------- 78: la moneta falsa ----------

        public static DoodleArt Coin()
        {
            var p = new DoodlePainter(-0.24f, -0.24f, 0.24f, 0.24f, 160) { Ink = 0.03f };
            Body(p, Sd.Circle(0, 0, 0.2f), Gold, GoldShade);
            p.Shape(Sd.Circle(0, 0, 0.14f), GoldLight, 0.015f);
            return new DoodleArt("coin", p);
        }

        // ---------- 80: le pecore ----------

        /// <summary>Pecora di lato che guarda a destra (zampe in y 0).</summary>
        public static DoodleArt Sheep()
        {
            var p = new DoodlePainter(-0.7f, -0.05f, 0.75f, 0.95f, 130) { Ink = 0.04f };
            foreach (float x in new[] { -0.35f, -0.15f, 0.2f, 0.38f }) p.InkLine(0.06f, x, 0.35f, x, 0.02f);
            Sdf wool = Sd.Union(Sd.Union(Sd.Circle(-0.35f, 0.55f, 0.25f), Sd.Circle(0f, 0.62f, 0.3f)), Sd.Union(Sd.Circle(0.3f, 0.55f, 0.25f), Sd.Ellipse(0, 0.48f, 0.55f, 0.22f)));
            Body(p, wool, Rgba.White, Rgba.Hex("D9DEE6"));
            Body(p, Sd.Ellipse(0.55f, 0.6f, 0.17f, 0.13f), Rgba.Hex("3A2E4A"), Rgba.Hex("1E1830"));
            p.Shape(Sd.Ellipse(0.45f, 0.72f, 0.08f, 0.04f), Rgba.Hex("3A2E4A"), 0.015f);
            p.Fill(Sd.Circle(0.6f, 0.64f, 0.03f), Rgba.White);
            return new DoodleArt("sheep", p);
        }

        // ---------- 89: il puzzle ----------

        // ---------- 95: memory ----------

        public static DoodleArt Card(bool face)
        {
            var p = new DoodlePainter(-0.48f, -0.62f, 0.48f, 0.62f, 120) { Ink = 0.04f };
            if (face) p.Shape(Sd.Box(0, 0, 0.42f, 0.56f, 0.08f), Rgba.White, 0.04f);
            else
            {
                p.FillFunc(Sd.Box(0, 0, 0.42f, 0.56f, 0.08f), (x, y) => ((int)Math.Floor((x + y) * 6f) & 1) == 0 ? Rgba.Hex("8E5FD1") : Rgba.Hex("7A4FC0"));
                p.Shape(Sd.Box(0, 0, 0.42f, 0.56f, 0.08f), Rgba.Clear, 0.04f);
                p.Shape(Sd.Circle(0, 0, 0.16f), Rgba.Hex("FFE14A"), 0.025f);
                p.Fill(Sd.Polygon(-0.03f, 0.08f, 0.06f, 0.08f, 0.02f, -0.1f), Ink);
            }
            return new DoodleArt(face ? "card_face" : "card_back", p);
        }

        /// <summary>Zampa del gatto che scende dall'alto (polpastrelli in basso, in 0,0).</summary>
        public static DoodleArt Paw()
        {
            var p = new DoodlePainter(-0.4f, -0.3f, 0.4f, 2.2f, 110) { Ink = 0.045f };
            Body(p, Sd.Union(Sd.Box(0, 1.2f, 0.28f, 1.0f), Sd.Circle(0, 0.1f, 0.33f)), Rgba.Hex("FF9A3D"), Rgba.Hex("E2731E"));
            p.Shape(Sd.Ellipse(0, 0.0f, 0.13f, 0.1f), Rgba.Hex("FFB3C0"), 0.015f);
            for (int i = -1; i <= 1; i++) p.Shape(Sd.Circle(i * 0.13f, -0.15f, 0.055f), Rgba.Hex("FFB3C0"), 0.012f);
            for (int i = 0; i < 3; i++) p.InkLine(0.025f, -0.2f, 0.7f + i * 0.4f, 0.2f, 0.85f + i * 0.4f);
            return new DoodleArt("paw", p);
        }

        // ---------- 98: l'ingorgo ----------


        // ---------- 71: il topo DJ ----------

        public static DoodleArt BeatNote(bool ghost)
        {
            var p = new DoodlePainter(-0.32f, -0.32f, 0.32f, 0.32f, 150) { Ink = 0.035f };
            Body(p, Sd.Circle(0, 0, 0.27f), ghost ? Rgba.Hex("8D93A6") : Rgba.Hex("FF5AA8"), ghost ? Rgba.Hex("5E6275") : Rgba.Hex("C93A80"));
            if (ghost) { p.Line(Rgba.White, 0.05f, -0.12f, -0.12f, 0.12f, 0.12f); p.Line(Rgba.White, 0.05f, -0.12f, 0.12f, 0.12f, -0.12f); }
            else { p.Fill(Sd.Ellipse(-0.05f, -0.07f, 0.08f, 0.06f), Rgba.White); p.Line(Rgba.White, 0.035f, 0.02f, -0.07f, 0.02f, 0.15f, 0.1f, 0.1f); }
            return new DoodleArt(ghost ? "beat_ghost" : "beat_note", p);
        }

        public static DoodleArt BeatRing()
        {
            var p = new DoodlePainter(-0.45f, -0.45f, 0.45f, 0.45f, 150) { Wobble = 0f };
            p.Shape(Sd.Subtract(Sd.Circle(0, 0, 0.4f), Sd.Circle(0, 0, 0.32f)), Rgba.Hex("FFE14A"), 0.03f);
            return new DoodleArt("beat_ring", p);
        }

        /// <summary>Palco della discoteca: pavimento a scacchi luminosi e faretti (coordinate del livello).</summary>
        public static DoodleArt DiscoBackground()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 4.6f, 7.1f), (x, y) => Rgba.Lerp(Rgba.Hex("1E1830"), Rgba.Hex("3A2E6A"), Clamp((y + 3f) / 9f)));
            p.FillFunc(Sd.Box(0, -5f, 4.6f, 2.6f), (x, y) =>
            {
                int k = ((int)Math.Floor(x / 0.6f) + (int)Math.Floor(y / 0.6f)) & 3;
                return k == 0 ? Rgba.Hex("FF5AA8") : k == 1 ? Rgba.Hex("5DE07A") : k == 2 ? Rgba.Hex("6FC3F2") : Rgba.Hex("FFE14A");
            });
            foreach (var b in new[] { (-3.2f, Rgba.Hex("FF5AA8")), (3.2f, Rgba.Hex("6FC3F2")) })
                p.FillFunc(Sd.Polygon(b.Item1, 5f, b.Item1 * 0.2f - 1f, -2.6f, b.Item1 * 0.2f + 1f, -2.6f), (x, y) => b.Item2.WithAlpha(0.18f));
            Body(p, Sd.Circle(0, 4.6f, 0.5f), Rgba.Hex("D9DEE6"), Rgba.Hex("9AA1B4"));   // palla da discoteca
            for (int i = 0; i < 12; i++) p.Fill(Sd.Box(-0.3f + (i % 4) * 0.2f, 4.35f + (i / 4) * 0.22f, 0.05f, 0.05f), Rgba.White);
            return new DoodleArt("disco_bg", p);
        }
    }
}
