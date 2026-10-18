using System;

namespace Gotcha.EditorArt
{
    /// <summary>Effetti della trasformazione del robot (livello 21): scritta 合体！, luci, bagliori.</summary>
    public static partial class DoodleSprites
    {
        /// <summary>
        /// Sottotitolo giapponese di GATTAI!! — 合体！ disegnato a pennellate (il font del gioco non ha i caratteri
        /// giapponesi). Tre caselle larghe 1, centro in 0,0; contorno scuro e riempimento giallo come la scritta sopra.
        /// </summary>
        public static DoodleArt GattaiKanji()
        {
            var p = new DoodlePainter(-1.75f, -0.68f, 1.75f, 0.68f, 140) { Wobble = 0.006f };
            float[][] strokes =
            {
                // 合
                new[] { 0.5f, 1.0f, 0.02f, 0.55f }, new[] { 0.5f, 1.0f, 0.98f, 0.55f }, new[] { 0.3f, 0.62f, 0.7f, 0.62f },
                new[] { 0.22f, 0.42f, 0.22f, 0.0f }, new[] { 0.22f, 0.42f, 0.78f, 0.42f, 0.78f, 0.0f }, new[] { 0.22f, 0.04f, 0.78f, 0.04f },
                // 体
                new[] { 1.33f, 1.0f, 1.05f, 0.55f }, new[] { 1.2f, 0.75f, 1.2f, 0.0f },
                new[] { 1.38f, 0.74f, 1.98f, 0.74f }, new[] { 1.68f, 1.0f, 1.68f, 0.0f },
                new[] { 1.68f, 0.7f, 1.42f, 0.28f }, new[] { 1.68f, 0.7f, 1.96f, 0.3f }, new[] { 1.52f, 0.22f, 1.84f, 0.22f },
                // ！
                new[] { 2.5f, 1.0f, 2.5f, 0.32f },
            };
            // casella (x 0..3, y 0..1) → centrata in 0,0 e un po' più stretta
            float[] Map(float[] s)
            {
                var o = new float[s.Length];
                for (int i = 0; i < s.Length; i += 2) { o[i] = (s[i] - 1.5f) * 1.05f; o[i + 1] = (s[i + 1] - 0.5f) * 1.05f; }
                return o;
            }
            Rgba outline = Rgba.Hex("2B2133"), fill = Rgba.Hex("FFD94A"), hot = Rgba.Hex("FF8A3D");
            foreach (float[] s in strokes) p.Line(outline, 0.22f, Map(s));
            p.Fill(Sd.Circle((2.5f - 1.5f) * 1.05f, (0.06f - 0.5f) * 1.05f, 0.13f), outline);
            foreach (float[] s in strokes) p.Line(hot, 0.15f, Map(s));
            foreach (float[] s in strokes) p.Line(fill, 0.1f, Map(s));
            p.Fill(Sd.Circle((2.5f - 1.5f) * 1.05f, (0.06f - 0.5f) * 1.05f, 0.075f), fill);
            return new DoodleArt("gattai_jp", p);
        }

        /// <summary>Colonna di luce verticale (centro 0,0, alta 6): bianca al centro, sfuma ai lati e alle estremità.</summary>
        public static DoodleArt LightPillar()
        {
            var p = new DoodlePainter(-0.7f, -3f, 0.7f, 3f, 40) { Wobble = 0f };
            p.FillFunc(Sd.Box(0, 0, 0.7f, 3f), (x, y) =>
            {
                float side = (float)Math.Exp(-x * x * 9f), ends = Math.Min(1f, (3f - Math.Abs(y)) * 0.8f);
                return Rgba.Lerp(Rgba.Hex("7FE8FF"), Rgba.White, side).WithAlpha(side * ends);
            });
            return new DoodleArt("light_pillar", p);
        }

        /// <summary>Lampo orizzontale da obiettivo (anamorfico): striscia di luce sottile, centro 0,0.</summary>
        public static DoodleArt LightStreak()
        {
            var p = new DoodlePainter(-2.2f, -0.18f, 2.2f, 0.18f, 60) { Wobble = 0f };
            p.FillFunc(Sd.Box(0, 0, 2.2f, 0.18f), (x, y) =>
            {
                float a = (float)(Math.Exp(-y * y * 300f) * Math.Max(0, 1 - Math.Abs(x) / 2.2f));
                float core = (float)Math.Exp(-(x * x * 6f + y * y * 200f));
                return Rgba.Lerp(Rgba.Hex("9EEBFF"), Rgba.White, core).WithAlpha(Math.Min(1f, a + core));
            });
            return new DoodleArt("light_streak", p);
        }

        /// <summary>Stellina luccicante a quattro punte con alone (centro 0,0).</summary>
        public static DoodleArt SparkleStar()
        {
            var p = new DoodlePainter(-0.3f, -0.3f, 0.3f, 0.3f, 160) { Wobble = 0f };
            p.FillFunc(Sd.Circle(0, 0, 0.3f), (x, y) => Rgba.Hex("FFF6B0").WithAlpha((float)Math.Exp(-(x * x + y * y) * 40f) * 0.7f));
            p.Fill(Sd.Polygon(0f, 0.28f, 0.05f, 0.05f, 0.28f, 0f, 0.05f, -0.05f, 0f, -0.28f, -0.05f, -0.05f, -0.28f, 0f, -0.05f, 0.05f), Rgba.White);
            return new DoodleArt("sparkle_star", p);
        }
    }
}
