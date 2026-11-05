using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.79 per il livello 24 della roadmap "The Lazy Sun" (la luna è "s41k_moon", le stelle
    /// "sparkle_star", i "Zzz" sono "zzz", lo steccato "sheep_fence", il bagliore "glow", i fumetti "bubble_box"): il cielo
    /// di notte e quello dell'alba (sfuma sopra), la grande collina tonda (si trascina in giù), il prato davanti, il sole
    /// (corpo coi raggi, due facce a parte: addormentata e sveglia) col berretto da notte a parte, il gallo (addormentato,
    /// con un occhio aperto, che canta). Collina e prato sono disegnati coi colori del giorno: di notte il livello li tinge
    /// di blu. Nessuna scritta disegnata. Prefisso v79_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV79B() => new DoodleArt[]
        {
            V79B_SkyNight(), V79B_SkyDawn(), V79B_Hill(), V79B_Meadow(), V79B_SunBody(), V79B_SunFaceSleep(),
            V79B_SunFaceAwake(), V79B_Nightcap(), V79B_RoosterSleep(), V79B_RoosterPeek(), V79B_RoosterCrow(),
        };

        // misure condivise col builder e col livello (coordinate locali dei disegni)
        /// <summary>Mezza larghezza e altezza dell'ellisse della cima della collina (origine della collina = la cima).</summary>
        public const float V79BHillHalfW = 4.9f, V79BHillHeight = 4.6f;
        /// <summary>Raggio del sole coi raggi e raggio del solo disco.</summary>
        public const float V79BSunRadius = 1.05f, V79BSunDisc = 0.72f;
        /// <summary>Dove poggia il berretto da notte rispetto al centro del sole.</summary>
        public const float V79BCapX = 0.02f, V79BCapY = 0.5f;
        /// <summary>Altezza del bordo del prato davanti (coordinate del mondo, il disegno ondeggia di poco attorno).</summary>
        public const float V79BMeadowTop = -3.3f;

        static readonly Rgba V79BSunCore = Rgba.Hex("FFD23A"), V79BSunShade = Rgba.Hex("F2A33A"), V79BRay = Rgba.Hex("FF9F2E");
        static readonly Rgba V79BCheek = Rgba.Hex("FF8A7A", 0.55f), V79BMouth = Rgba.Hex("8A2E2E");
        static readonly Rgba V79BCap = Rgba.Hex("5B6FD6"), V79BCapShade = Rgba.Hex("3F4FA8"), V79BTrim = Rgba.Hex("FFFDF6");
        static readonly Rgba V79BHen = Rgba.Hex("FFFDF6"), V79BHenShade = Rgba.Hex("DAD3C4"), V79BComb = Rgba.Hex("E8443A"), V79BBeak = Rgba.Hex("F5A623");

        static float V79BClamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        /// <summary>Il cielo di notte, coordinate del mondo: blu notte in alto, viola-blu verso l'orizzonte, puntini di stelle.</summary>
        public static DoodleArt V79B_SkyNight()
        {
            var p = new DoodlePainter(-4.5f, -4f, 4.5f, 7f, 50) { Ink = 0f, Wobble = 0f };
            p.FillFunc(Sd.Box(0f, 1.5f, 4.6f, 5.6f), (x, y) => Rgba.Lerp(Rgba.Hex("3D3F82"), Rgba.Hex("121838"), V79BClamp((y + 1f) / 6.5f)));
            var rnd = new Random(2424);
            for (int i = 0; i < 70; i++)
            {
                float x = (float)rnd.NextDouble() * 9f - 4.5f, y = (float)rnd.NextDouble() * 7f - 0.2f;
                float r = 0.012f + (float)rnd.NextDouble() * 0.022f;
                p.Fill(Sd.Circle(x, y, r), Rgba.Hex("FFF6D8", 0.5f + (float)rnd.NextDouble() * 0.5f), false);
            }
            return new DoodleArt("v79_b_sky_night", p);
        }

        /// <summary>Il cielo dell'alba (stesse misure del cielo di notte, sfuma sopra): azzurro in alto, pesca e arancio in basso.</summary>
        public static DoodleArt V79B_SkyDawn()
        {
            var p = new DoodlePainter(-4.5f, -4f, 4.5f, 7f, 50) { Ink = 0f, Wobble = 0f };
            p.FillFunc(Sd.Box(0f, 1.5f, 4.6f, 5.6f), (x, y) =>
            {
                float t = V79BClamp((y + 2.5f) / 7f);
                Rgba low = Rgba.Lerp(Rgba.Hex("FF9E6E"), Rgba.Hex("FFD9A0"), V79BClamp(t * 2.2f));
                return Rgba.Lerp(low, Rgba.Hex("8CCBF2"), V79BClamp((t - 0.35f) / 0.65f));
            });
            // due nuvolette rosa
            foreach (float[] c in new[] { new[] { -1.9f, 2.6f }, new[] { 1.7f, 1.4f } })
                p.Fill(Sd.Union(Sd.Ellipse(c[0], c[1], 0.65f, 0.14f), Sd.Ellipse(c[0] + 0.2f, c[1] + 0.1f, 0.35f, 0.14f)), Rgba.Hex("FFC2B8", 0.8f));
            return new DoodleArt("v79_b_sky_dawn", p);
        }

        /// <summary>
        /// La grande collina tonda (origine sulla cima): ellisse di mezza larghezza <see cref="V79BHillHalfW"/> e altezza
        /// <see cref="V79BHillHeight"/>, piena fino in fondo al disegno; verde del giorno con ciuffi d'erba e cespugli
        /// (il livello la tinge di blu di notte). Larga quanto tutti gli schermi, così il sole dietro non si vede mai dai lati.
        /// </summary>
        public static DoodleArt V79B_Hill()
        {
            float a = V79BHillHalfW, b = V79BHillHeight;
            var p = new DoodlePainter(-4.5f, -4.6f, 4.5f, 0.12f, 50) { Ink = 0.07f, Wobble = 0.008f };
            Sdf hill = Sd.Union(Sd.Ellipse(0f, -b, a, b), Sd.Box(0f, -4.3f, 4.6f, 1.3f));
            p.FillFunc(hill, (x, y) => Rgba.Lerp(Rgba.Hex("4E9A50"), Rgba.Hex("86CC70"), V79BClamp((y + 3f) / 3f)));
            p.Fill(Sd.Subtract(hill, Sd.Offset(hill, -0.25f, -0.12f)), Rgba.Hex("A8E08E", 0.6f));   // luce sul bordo
            // ciuffi d'erba e cespugli
            var rnd = new Random(77);
            for (int i = 0; i < 26; i++)
            {
                float x = (float)rnd.NextDouble() * 8.4f - 4.2f;
                float top = -b + b * (float)Math.Sqrt(Math.Max(0.0, 1.0 - (x / a) * (x / a)));
                float y = top - 0.35f - (float)rnd.NextDouble() * 2.4f;
                p.Line(Rgba.Hex("3E7F44"), 0.03f, x - 0.08f, y + 0.12f, x, y, x + 0.03f, y + 0.15f, x + 0.06f, y, x + 0.14f, y + 0.1f);
            }
            p.Shape(hill, Rgba.Clear, 0.07f);
            foreach (float bx in new[] { -2.6f, 2.9f })
            {
                float top = -b + b * (float)Math.Sqrt(1.0 - (bx / a) * (bx / a));
                Sdf bush = Sd.Union(Sd.Circle(bx - 0.2f, top - 0.1f, 0.22f), Sd.Circle(bx + 0.1f, top - 0.04f, 0.26f), Sd.Circle(bx + 0.36f, top - 0.14f, 0.18f));
                p.Shape(bush, Rgba.Hex("3F8A46"), 0.05f);
            }
            return new DoodleArt("v79_b_hill", p);
        }

        /// <summary>Il prato davanti (coordinate del mondo): bordo ondulato attorno a y <see cref="V79BMeadowTop"/>, erba e fiorellini.</summary>
        public static DoodleArt V79B_Meadow()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, -2.95f, 50) { Ink = 0.07f, Wobble = 0.008f };
            Sdf meadow = (x, y) => y - (V79BMeadowTop + 0.09f * (float)Math.Sin(x * 2.1f) + 0.05f * (float)Math.Sin(x * 5.3f + 1f));
            Sdf area = Sd.Intersect(meadow, Sd.Box(0f, -5f, 4.6f, 2.2f));
            p.FillFunc(area, (x, y) => Rgba.Lerp(Rgba.Hex("3F7A3A"), Rgba.Hex("6DB259"), V79BClamp((y + 5.5f) / 2.2f)));
            var rnd = new Random(31);
            for (int i = 0; i < 40; i++)
            {
                float x = (float)rnd.NextDouble() * 8.6f - 4.3f, y = -3.55f - (float)rnd.NextDouble() * 3.1f;
                p.Line(Rgba.Hex("2F5F2C"), 0.028f, x - 0.07f, y + 0.1f, x, y, x + 0.02f, y + 0.13f, x + 0.05f, y, x + 0.12f, y + 0.09f);
            }
            for (int i = 0; i < 10; i++)
            {
                float x = (float)rnd.NextDouble() * 8.2f - 4.1f, y = -3.7f - (float)rnd.NextDouble() * 2.6f;
                Rgba petal = i % 2 == 0 ? Rgba.Hex("FFF3F0") : Rgba.Hex("FFE07A");
                foreach (float k in new[] { 0f, 1.57f, 3.14f, 4.71f })
                    p.Fill(Sd.Circle(x + 0.05f * (float)Math.Cos(k), y + 0.05f * (float)Math.Sin(k), 0.04f), petal);
                p.Fill(Sd.Circle(x, y, 0.025f), Rgba.Hex("F2A33A"));
            }
            p.Shape(area, Rgba.Clear, 0.07f);
            return new DoodleArt("v79_b_meadow", p);
        }

        /// <summary>Il sole senza faccia (origine al centro): dodici raggi fino a <see cref="V79BSunRadius"/>, disco di raggio <see cref="V79BSunDisc"/>.</summary>
        public static DoodleArt V79B_SunBody()
        {
            float r = V79BSunRadius, d = V79BSunDisc;
            var p = new DoodlePainter(-r - 0.06f, -r - 0.06f, r + 0.06f, r + 0.06f, 120) { Ink = 0.045f };
            for (int i = 0; i < 12; i++)
            {
                double ang = i * Math.PI / 6 + Math.PI / 12;
                float c = (float)Math.Cos(ang), s = (float)Math.Sin(ang);
                float w = 0.12f, rb = d - 0.05f;
                p.Shape(Sd.Polygon(c * rb - s * w, s * rb + c * w, c * rb + s * w, s * rb - c * w, c * (r - 0.03f), s * (r - 0.03f)), V79BRay, 0.035f);
            }
            Body(p, Sd.Circle(0f, 0f, d), V79BSunCore, V79BSunShade);
            return new DoodleArt("v79_b_sun_body", p);
        }

        /// <summary>La faccia del sole che dorme (origine al centro del sole): occhi chiusi a "u", guance rosse, bocca a "o" che russa, un filo di bava.</summary>
        public static DoodleArt V79B_SunFaceSleep()
        {
            var p = new DoodlePainter(-0.6f, -0.55f, 0.6f, 0.4f, 120) { Ink = 0.04f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.InkLine(0.035f, s * 0.34f, 0.1f, s * 0.24f, 0.03f, s * 0.12f, 0.08f);
                p.Fill(Sd.Ellipse(s * 0.38f, -0.1f, 0.11f, 0.07f), V79BCheek);
            }
            p.Shape(Sd.Ellipse(0f, -0.25f, 0.08f, 0.09f), V79BMouth, 0.035f);
            p.Shape(Sd.Union(Sd.Capsule(0.09f, -0.3f, 0.12f, -0.4f, 0.025f), Sd.Circle(0.12f, -0.42f, 0.035f)), Rgba.Hex("BFE6FF"), 0.02f);
            return new DoodleArt("v79_b_sun_face_sleep", p);
        }

        /// <summary>La faccia del sole sveglio (origine al centro del sole): occhi spalancati col riflesso, gran sorriso aperto, guance.</summary>
        public static DoodleArt V79B_SunFaceAwake()
        {
            var p = new DoodlePainter(-0.6f, -0.55f, 0.6f, 0.4f, 120) { Ink = 0.04f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Ellipse(s * 0.24f, 0.08f, 0.1f, 0.13f), Rgba.White, 0.035f);
                p.Fill(Sd.Circle(s * 0.24f + 0.02f, 0.06f, 0.055f), Ink);
                p.Fill(Sd.Circle(s * 0.24f, 0.09f, 0.02f), Rgba.White);
                p.InkLine(0.03f, s * 0.14f, 0.27f, s * 0.24f, 0.31f, s * 0.34f, 0.28f);
                p.Fill(Sd.Ellipse(s * 0.4f, -0.12f, 0.11f, 0.07f), V79BCheek);
            }
            Sdf mouth = Sd.Intersect(Sd.Ellipse(0f, -0.17f, 0.26f, 0.2f), Sd.Box(0f, -0.3f, 0.3f, 0.13f));
            p.Shape(mouth, V79BMouth, 0.035f);
            p.Fill(Sd.Intersect(mouth, Sd.Ellipse(0f, -0.38f, 0.14f, 0.08f)), Rgba.Hex("E8706A"));
            return new DoodleArt("v79_b_sun_face_awake", p);
        }

        /// <summary>
        /// Il berretto da notte (origine al centro della fascia, poggia sulla testa del sole in <see cref="V79BCapX"/>,
        /// <see cref="V79BCapY"/>): fascia bianca, cono blu a righe che ricade a destra, pon-pon.
        /// </summary>
        public static DoodleArt V79B_Nightcap()
        {
            var p = new DoodlePainter(-0.62f, -0.18f, 1.15f, 0.72f, 120) { Ink = 0.04f };
            Sdf cone = Sd.Polygon(-0.48f, 0.02f, -0.3f, 0.42f, 0.05f, 0.62f, 0.45f, 0.6f, 0.8f, 0.38f, 0.9f, 0.18f, 0.72f, 0.26f, 0.38f, 0.3f, 0.48f, 0.02f);
            p.Fill(cone, V79BCap);
            p.Fill(Sd.Crescent(cone, -0.05f, 0.06f), V79BCapShade);
            foreach (float k in new[] { -0.15f, 0.2f, 0.55f })
                p.Fill(Sd.Intersect(cone, Sd.Rotate(Sd.Box(k, 0.3f, 0.06f, 0.6f), k, 0.3f, -35f)), Rgba.Hex("8FA0F0"));
            p.Shape(cone, Rgba.Clear);
            Body(p, Sd.Box(0f, 0.02f, 0.55f, 0.1f, 0.08f), V79BTrim, Rgba.Hex("DAD3C4"));
            Body(p, Sd.Circle(0.95f, 0.14f, 0.13f), V79BTrim, Rgba.Hex("DAD3C4"));
            return new DoodleArt("v79_b_nightcap", p);
        }

        // ---------- Il gallo (di fronte, accovacciato sull'asse dello steccato; origine sotto, al centro) ----------

        /// <summary>Corpo del gallo, coda a ventaglio e ali; la testa (centro in hx, hy) la disegna chi chiama.</summary>
        static void V79B_RoosterBody(DoodlePainter p, float hx, float hy)
        {
            Rgba[] tail = { Rgba.Hex("2E8B57"), Rgba.Hex("D9822B"), Rgba.Hex("3A5BA0"), Rgba.Hex("D9822B"), Rgba.Hex("2E8B57") };
            for (int i = 0; i < tail.Length; i++)
            {
                float a = (float)((150 - i * 30) * Math.PI / 180.0);
                p.Shape(Sd.Capsule(0f, 0.3f, (float)Math.Cos(a) * 0.5f, 0.38f + (float)Math.Sin(a) * 0.36f, 0.08f), tail[i], 0.03f);
            }
            foreach (float s in new[] { -1f, 1f })
                Body(p, Sd.Rotate(Sd.Ellipse(s * 0.33f, 0.24f, 0.12f, 0.2f), s * 0.33f, 0.24f, s * 30f), V79BHen, V79BHenShade);
            Body(p, Sd.Ellipse(0f, 0.24f, 0.36f, 0.25f), V79BHen, V79BHenShade);
            Body(p, Sd.Ellipse(hx, hy, 0.21f, 0.22f), V79BHen, V79BHenShade);
            p.Shape(Sd.Union(Sd.Circle(hx - 0.1f, hy + 0.24f, 0.07f), Sd.Circle(hx, hy + 0.3f, 0.09f), Sd.Circle(hx + 0.11f, hy + 0.25f, 0.075f)), V79BComb, 0.03f);
        }

        static void V79B_RoosterBeak(DoodlePainter p, float hx, float hy, bool open)
        {
            if (!open)
            {
                p.Shape(Sd.Polygon(hx - 0.07f, hy - 0.02f, hx + 0.07f, hy - 0.02f, hx, hy - 0.12f), V79BBeak, 0.022f);
                p.Shape(Sd.Union(Sd.Ellipse(hx - 0.035f, hy - 0.17f, 0.035f, 0.055f), Sd.Ellipse(hx + 0.035f, hy - 0.17f, 0.035f, 0.055f)), V79BComb, 0.02f);
                return;
            }
            p.Shape(Sd.Polygon(hx - 0.08f, hy - 0.01f, hx + 0.08f, hy - 0.01f, hx, hy + 0.08f), V79BBeak, 0.022f);
            p.Shape(Sd.Polygon(hx - 0.07f, hy - 0.08f, hx + 0.07f, hy - 0.08f, hx, hy - 0.2f), V79BBeak, 0.022f);
            p.Fill(Sd.Polygon(hx - 0.06f, hy - 0.02f, hx + 0.06f, hy - 0.02f, hx + 0.05f, hy - 0.07f, hx - 0.05f, hy - 0.07f), V79BMouth);
            p.Shape(Sd.Union(Sd.Ellipse(hx - 0.035f, hy - 0.25f, 0.035f, 0.05f), Sd.Ellipse(hx + 0.035f, hy - 0.25f, 0.035f, 0.05f)), V79BComb, 0.02f);
        }

        /// <summary>Il gallo che dorme: testa un po' piegata, occhi chiusi, becco chiuso.</summary>
        public static DoodleArt V79B_RoosterSleep()
        {
            var p = new DoodlePainter(-0.62f, -0.05f, 0.62f, 1.12f, 140) { Ink = 0.04f };
            float hx = -0.06f, hy = 0.6f;
            V79B_RoosterBody(p, hx, hy);
            foreach (float s in new[] { -1f, 1f }) p.InkLine(0.025f, hx + s * 0.13f, hy + 0.06f, hx + s * 0.08f, hy + 0.03f, hx + s * 0.03f, hy + 0.06f);
            V79B_RoosterBeak(p, hx, hy, false);
            return new DoodleArt("v79_b_rooster_sleep", p);
        }

        /// <summary>Il gallo che apre un occhio solo (palpebra pesante, sopracciglio scocciato): "Too early."</summary>
        public static DoodleArt V79B_RoosterPeek()
        {
            var p = new DoodlePainter(-0.62f, -0.05f, 0.62f, 1.12f, 140) { Ink = 0.04f };
            float hx = -0.03f, hy = 0.62f;
            V79B_RoosterBody(p, hx, hy);
            p.InkLine(0.025f, hx - 0.13f, hy + 0.06f, hx - 0.08f, hy + 0.03f, hx - 0.03f, hy + 0.06f);
            p.Shape(Sd.Circle(hx + 0.09f, hy + 0.06f, 0.055f), Rgba.White, 0.022f);
            p.Fill(Sd.Circle(hx + 0.1f, hy + 0.05f, 0.03f), Ink);
            p.Fill(Sd.Intersect(Sd.Circle(hx + 0.09f, hy + 0.06f, 0.06f), Sd.Box(hx + 0.09f, hy + 0.12f, 0.08f, 0.04f)), V79BHenShade);   // palpebra pesante
            p.InkLine(0.025f, hx + 0.03f, hy + 0.09f, hx + 0.15f, hy + 0.08f);
            p.InkLine(0.022f, hx + 0.03f, hy + 0.16f, hx + 0.16f, hy + 0.15f);
            V79B_RoosterBeak(p, hx, hy, false);
            return new DoodleArt("v79_b_rooster_peek", p);
        }

        /// <summary>Il gallo che canta: testa alta, occhi chiusi stretti, becco spalancato.</summary>
        public static DoodleArt V79B_RoosterCrow()
        {
            var p = new DoodlePainter(-0.62f, -0.05f, 0.62f, 1.2f, 140) { Ink = 0.04f };
            float hx = 0f, hy = 0.7f;
            V79B_RoosterBody(p, hx, hy);
            foreach (float s in new[] { -1f, 1f }) p.InkLine(0.025f, hx + s * 0.14f, hy + 0.1f, hx + s * 0.07f, hy + 0.06f, hx + s * 0.13f, hy + 0.03f);
            V79B_RoosterBeak(p, hx, hy, true);
            return new DoodleArt("v79_b_rooster_crow", p);
        }
    }
}
