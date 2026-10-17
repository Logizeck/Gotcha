using System;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// Disegni della versione 0.30 (livello 101): il carlino nero, a pezzi separati per animarlo
    /// (corpo, testa, orecchie, occhi, bocca, lingua, bolla al naso), più cuscino, pepiera e granelli di pepe.
    /// Tutti i pezzi del carlino usano le stesse coordinate: origine tra le zampe a terra, testa centrata in (0, 1.5).
    /// </summary>
    public static partial class DoodleSprites
    {
        static readonly Rgba PugFur = Rgba.Hex("3D3A4A"), PugShade = Rgba.Hex("2A2735"), PugDeep = Rgba.Hex("1C1A24");
        static readonly Rgba PugLight = Rgba.Hex("6A6582"), PugInk = Rgba.Hex("120F18");
        static readonly Rgba PugTongue = Rgba.Hex("FF8FAB"), PugTongueLine = Rgba.Hex("D9567A");
        static readonly Rgba PugRed = Rgba.Hex("E8433F"), PugRedShade = Rgba.Hex("B92E2A");

        // posizione degli occhi (il sinistro è in -PugEyeX)
        public const float PugEyeX = 0.34f, PugEyeY = 1.61f, PugEyeR = 0.165f;

        static DoodlePainter PugPainter(float xMin, float yMin, float xMax, float yMax, float ink = 0.045f)
        {
            return new DoodlePainter(xMin, yMin, xMax, yMax, 160) { Ink = ink, InkColor = PugInk };
        }

        /// <summary>Arco "a mano" (curva quadratica da a a b con controllo c).</summary>
        static void PugArc(DoodlePainter p, Rgba color, float width, float ax, float ay, float cx, float cy, float bx, float by)
        {
            var xy = new float[18];
            for (int i = 0; i <= 8; i++)
            {
                float t = i / 8f, u = 1f - t;
                xy[i * 2] = u * u * ax + 2f * u * t * cx + t * t * bx;
                xy[i * 2 + 1] = u * u * ay + 2f * u * t * cy + t * t * by;
            }
            p.Line(color, width, xy);
        }

        /// <summary>Ruga del carlino: piega scura con il bordo chiaro appena sopra.</summary>
        static void PugFold(DoodlePainter p, float width, float ax, float ay, float cx, float cy, float bx, float by)
        {
            PugArc(p, PugInk, width, ax, ay, cx, cy, bx, by);
            PugArc(p, PugLight, width * 0.6f, ax, ay + width * 0.9f, cx, cy + width * 0.9f, bx, by + width * 0.9f);
        }

        static Sdf PugJowls(float dy = 0f)
        {
            return Sd.Union(Sd.Ellipse(-0.19f, 1.19f + dy, 0.225f, 0.16f), Sd.Ellipse(0.19f, 1.19f + dy, 0.225f, 0.16f));
        }

        static void PugMuzzle(DoodlePainter p, float dy)
        {
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Ellipse(s * 0.19f, 1.19f + dy, 0.225f, 0.16f), PugFur, PugShade, null, -0.04f, 0.05f);
            foreach (float s in new[] { -1f, 1f })
            {
                foreach (var d in new[] { (0.12f, 1.22f), (0.2f, 1.17f), (0.28f, 1.21f), (0.17f, 1.1f), (0.26f, 1.12f) })
                    p.Fill(Sd.Circle(s * d.Item1, d.Item2 + dy, 0.011f), PugLight);
                p.Line(PugLight.WithAlpha(0.6f), 0.01f, s * 0.3f, 1.17f + dy, s * 0.5f, 1.2f + dy);
                p.Line(PugLight.WithAlpha(0.6f), 0.01f, s * 0.28f, 1.11f + dy, s * 0.46f, 1.06f + dy);
            }
            p.InkLine(0.03f, 0f, 1.29f + dy, 0f, 1.2f + dy);
        }

        static void PugNose(DoodlePainter p)
        {
            p.Shape(Sd.Ellipse(0, 1.37f, 0.16f, 0.085f), Rgba.Hex("17141D"), 0.035f);
            foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Rotate(Sd.Ellipse(s * 0.07f, 1.355f, 0.036f, 0.021f), s * 0.07f, 1.355f, s * 20f), Rgba.Hex("050407"));
            p.Fill(Sd.Ellipse(-0.035f, 1.415f, 0.065f, 0.018f), Rgba.White.WithAlpha(0.45f));
        }

        /// <summary>Corpo seduto: un'unica sagoma con zampe, cosce e piedi, e il collare con l'anello (la coda è a parte).</summary>
        public static DoodleArt PugBody()
        {
            var p = PugPainter(-0.95f, -0.12f, 0.95f, 1.35f);
            Sdf body = Sd.Union(
                Sd.Ellipse(0, 0.55f, 0.66f, 0.52f), Sd.Ellipse(0, 1.0f, 0.5f, 0.3f),             // busto e collo
                Sd.Ellipse(-0.55f, 0.3f, 0.25f, 0.3f), Sd.Ellipse(0.55f, 0.3f, 0.25f, 0.3f),     // cosce
                Sd.Capsule(-0.3f, 0.6f, -0.3f, 0.1f, 0.155f), Sd.Capsule(0.3f, 0.6f, 0.3f, 0.1f, 0.155f),   // zampe davanti
                Sd.Ellipse(-0.31f, 0.07f, 0.19f, 0.085f), Sd.Ellipse(0.31f, 0.07f, 0.19f, 0.085f),          // piedi davanti
                Sd.Ellipse(-0.66f, 0.06f, 0.17f, 0.07f), Sd.Ellipse(0.66f, 0.06f, 0.17f, 0.07f));           // piedi dietro
            Body(p, body, PugFur, PugShade, null, -0.08f, 0.08f);

            // le zampe si separano solo in basso: il resto è un tutt'uno con il petto
            p.Fill(Sd.Polygon(-0.12f, -0.01f, 0f, 0.4f, 0.12f, -0.01f), PugDeep);
            p.InkLine(0.032f, -0.13f, 0.0f, -0.05f, 0.3f, 0f, 0.4f, 0.05f, 0.3f, 0.13f, 0.0f);
            foreach (float s in new[] { -1f, 1f })
            {
                p.InkLine(0.03f, s * 0.43f, 0.58f, s * 0.47f, 0.36f, s * 0.48f, 0.14f);   // coscia dietro la zampa
                p.Line(PugLight.WithAlpha(0.7f), 0.022f, s * 0.6f, 0.52f, s * 0.7f, 0.42f, s * 0.75f, 0.28f);   // luce sulla coscia
                p.Line(PugLight.WithAlpha(0.55f), 0.02f, s * 0.24f, 0.5f, s * 0.23f, 0.25f);   // luce sulla zampa
                foreach (float dx in new[] { -0.06f, 0.06f }) p.InkLine(0.022f, s * 0.31f + dx, -0.01f, s * 0.31f + dx, 0.07f);
                p.InkLine(0.022f, s * 0.66f, -0.0f, s * 0.66f, 0.06f);
                foreach (float dx in new[] { -0.11f, 0f, 0.11f }) p.Fill(Sd.Ellipse(s * 0.31f + dx, -0.005f, 0.022f, 0.012f), Rgba.Hex("B9BFCC"));   // unghiette
            }
            // ciuffi di pelo sul petto
            foreach (var t in new[] { (-0.14f, 0.66f), (0.02f, 0.72f), (0.15f, 0.62f), (-0.04f, 0.56f) })
                p.Line(PugLight.WithAlpha(0.6f), 0.016f, t.Item1 - 0.03f, t.Item2 + 0.04f, t.Item1, t.Item2, t.Item1 + 0.03f, t.Item2 + 0.04f);

            // collare rosso con borchie e anello (ci si appende la medaglietta o, nel livello 101, la chiave)
            Sdf collar = Sd.Intersect(Sd.Subtract(Sd.Ellipse(0, 0.98f, 0.56f, 0.27f), Sd.Ellipse(0, 1.07f, 0.6f, 0.27f)), Sd.Box(0, 0.9f, 0.6f, 0.3f));
            Body(p, collar, PugRed, PugRedShade, null, -0.03f, 0.04f);
            foreach (float x in new[] { -0.38f, -0.2f, 0.2f, 0.38f })
            {
                float y = 0.98f - 0.27f * (float)Math.Sqrt(Math.Max(0f, 1f - x * x / (0.56f * 0.56f))) + 0.04f;
                p.Shape(Sd.Circle(x, y, 0.022f), Gold, 0.015f);
            }
            p.Shape(Sd.Subtract(Sd.Circle(0, 0.69f, 0.055f), Sd.Circle(0, 0.69f, 0.028f)), Gold, 0.02f);
            return new DoodleArt("pug_body", p);
        }

        /// <summary>Coda arricciata sul fianco destro (va dietro al corpo; attaccatura in 0.5, 0.82).</summary>
        public static DoodleArt PugTail()
        {
            var p = PugPainter(0.38f, 0.62f, 0.86f, 1.1f);
            Body(p, Sd.Subtract(Sd.Circle(0.62f, 0.86f, 0.17f), Sd.Circle(0.6f, 0.87f, 0.065f)), PugFur, PugShade);
            p.Line(PugLight, 0.022f, 0.52f, 0.97f, 0.62f, 1.0f, 0.72f, 0.95f);
            return new DoodleArt("pug_tail", p);
        }

        /// <summary>Testa senza occhi né orecchie: muso piatto, piega sul naso, rughe, guance cadenti.</summary>
        public static DoodleArt PugHead()
        {
            var p = PugPainter(-0.85f, 0.85f, 0.85f, 2.2f);
            Sdf head = Sd.Union(Sd.Ellipse(0, 1.58f, 0.66f, 0.55f), Sd.Ellipse(0, 1.32f, 0.62f, 0.42f));
            Body(p, head, PugFur, PugShade, null, -0.08f, 0.08f);
            p.Fill(Sd.Ellipse(-0.08f, 1.93f, 0.3f, 0.1f), PugLight.WithAlpha(0.35f));   // lucido sulla fronte

            // rughe a rombo sulla fronte e pieghe delle guance
            PugFold(p, 0.03f, -0.24f, 2.0f, 0f, 2.075f, 0.24f, 2.0f);
            PugFold(p, 0.028f, -0.15f, 1.9f, 0f, 1.96f, 0.15f, 1.9f);
            PugFold(p, 0.025f, -0.075f, 1.81f, 0f, 1.85f, 0.075f, 1.81f);
            p.InkLine(0.025f, 0f, 1.785f, 0f, 1.72f);
            foreach (float s in new[] { -1f, 1f })
            {
                PugFold(p, 0.03f, s * 0.53f, 1.8f, s * 0.35f, 1.93f, s * 0.15f, 1.85f);
                PugFold(p, 0.026f, s * 0.56f, 1.45f, s * 0.54f, 1.31f, s * 0.45f, 1.2f);
                p.Fill(Sd.Ellipse(s * PugEyeX, PugEyeY - 0.01f, 0.2f, 0.19f), PugDeep);   // orbite (gli occhi sono a parte)
            }

            // mento, guance e piega grossa sopra il naso
            Body(p, Sd.Ellipse(0, 1.02f, 0.14f, 0.07f), PugFur, PugShade);
            PugMuzzle(p, 0f);
            Sdf roll = Sd.Subtract(Sd.Ellipse(0, 1.36f, 0.31f, 0.19f), Sd.Ellipse(0, 1.3f, 0.34f, 0.16f));
            Body(p, roll, Rgba.Hex("4A4658"), PugShade, null, 0f, -0.03f);
            PugArc(p, PugLight, 0.022f, -0.24f, 1.47f, 0f, 1.6f, 0.24f, 1.47f);
            PugNose(p);
            return new DoodleArt("pug_head", p);
        }

        /// <summary>Orecchio "a bottone": attaccato in alto, ripiegato in avanti con la punta verso l'occhio. side -1 = sinistro.</summary>
        public static DoodleArt PugEar(int side)
        {
            float s = side < 0 ? 1f : -1f;   // i punti sono scritti per l'orecchio sinistro
            var p = side < 0 ? PugPainter(-0.88f, 1.58f, -0.12f, 2.28f, 0.04f) : PugPainter(0.12f, 1.58f, 0.88f, 2.28f, 0.04f);
            float[] pts = { -0.2f, 2.13f, -0.48f, 2.2f, -0.7f, 2.12f, -0.8f, 1.99f, -0.79f, 1.8f, -0.72f, 1.67f, -0.62f, 1.72f, -0.5f, 1.85f, -0.34f, 1.98f };
            for (int i = 0; i < pts.Length; i += 2) pts[i] *= s;
            p.Fill(Sd.Offset(Sd.Polygon(pts), 0.02f * s, -0.04f), PugInk.WithAlpha(0.45f));   // ombra sulla testa
            Body(p, Sd.Grow(Sd.Polygon(pts), 0.012f), Rgba.Hex("312E3D"), PugDeep, null, 0.05f * s, 0.06f);
            p.Line(PugLight, 0.032f, -0.24f * s, 2.15f, -0.48f * s, 2.2f, -0.68f * s, 2.13f);   // piega illuminata
            p.InkLine(0.024f, -0.36f * s, 2.05f, -0.56f * s, 1.94f, -0.68f * s, 1.76f);
            return new DoodleArt(side < 0 ? "pug_ear_l" : "pug_ear_r", p);
        }

        /// <summary>Occhio aperto, grande e lucido (side -1 = sinistro).</summary>
        public static DoodleArt PugEye(int side)
        {
            float cx = side * PugEyeX, cy = PugEyeY, r = PugEyeR, ix = cx - side * 0.012f;
            var p = PugPainter(cx - 0.24f, cy - 0.24f, cx + 0.24f, cy + 0.24f, 0.035f);
            Sdf ball = Sd.Circle(cx, cy, r);
            p.Fill(ball, Rgba.Hex("F4F1EA"));
            p.FillFunc(Sd.Circle(ix, cy - 0.005f, 0.135f), (x, y) => Rgba.Lerp(Rgba.Hex("3B1D0B"), Rgba.Hex("9A5A26"), Clamp((cy + 0.1f - y) / 0.2f)));
            p.Fill(Sd.Circle(ix, cy - 0.005f, 0.075f), Ink);
            p.Fill(Sd.Ellipse(ix - 0.05f, cy + 0.05f, 0.045f, 0.032f), Rgba.White);
            p.Fill(Sd.Circle(ix + 0.05f, cy - 0.05f, 0.018f), Rgba.White.WithAlpha(0.85f));
            // palpebra superiore un po' cadente: lo sguardo tipico del carlino
            p.Fill(Sd.Intersect(ball, Sd.Ellipse(cx, cy + 0.19f, 0.24f, 0.1f)), PugFur);
            p.InkLine(0.03f, cx - 0.155f, cy + 0.06f, cx - 0.07f, cy + 0.1f, cx + 0.07f, cy + 0.1f, cx + 0.155f, cy + 0.06f);
            p.Shape(ball, Rgba.Clear);
            return new DoodleArt(side < 0 ? "pug_eye_l" : "pug_eye_r", p);
        }

        /// <summary>Tutti e due gli occhi chiusi (palpebre con ciglia).</summary>
        public static DoodleArt PugEyesClosed()
        {
            var p = PugPainter(-0.6f, 1.36f, 0.6f, 1.86f, 0.035f);
            foreach (float s in new[] { -1f, 1f })
            {
                float cx = s * PugEyeX, cy = PugEyeY;
                p.Fill(Sd.Circle(cx, cy, PugEyeR + 0.035f), PugFur);   // palpebre chiuse: niente contorno, solo la fessura
                PugArc(p, PugLight, 0.018f, cx - 0.12f, cy + 0.06f, cx, cy + 0.12f, cx + 0.12f, cy + 0.06f);
                PugArc(p, PugInk, 0.04f, cx - 0.13f, cy - 0.01f, cx, cy - 0.1f, cx + 0.13f, cy - 0.01f);
                foreach (float dx in new[] { -0.08f, 0f, 0.08f }) p.InkLine(0.022f, cx + dx, cy - 0.06f + Math.Abs(dx) * 0.4f, cx + dx * 1.3f, cy - 0.12f + Math.Abs(dx) * 0.4f);
            }
            return new DoodleArt("pug_eyes_closed", p);
        }

        /// <summary>Bocca spalancata (abbaio): gola, lingua, dentini dell'enognatismo, guance sollevate e naso sopra.</summary>
        public static DoodleArt PugMouthOpen()
        {
            var p = PugPainter(-0.5f, 0.72f, 0.5f, 1.48f);
            Sdf mouth = Sd.Ellipse(0, 1.0f, 0.17f, 0.21f);
            p.Shape(mouth, Rgba.Hex("5A1424"));
            p.Fill(Sd.Intersect(Sd.Ellipse(0, 0.86f, 0.13f, 0.09f), mouth), PugTongue);
            p.Line(PugTongueLine, 0.016f, 0f, 0.84f, 0f, 0.9f);
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Polygon(s * 0.1f, 0.9f, s * 0.075f, 0.98f, s * 0.05f, 0.9f), Rgba.White, 0.012f);
            p.Shape(mouth, Rgba.Clear);
            PugMuzzle(p, 0.035f);
            PugNose(p);
            return new DoodleArt("pug_mouth_open", p);
        }

        /// <summary>Lingua che pende di lato, dall'angolo destro della bocca (attaccatura in 0.2, 1.08).</summary>
        public static DoodleArt PugTongueSide()
        {
            var p = PugPainter(0.08f, 0.68f, 0.52f, 1.14f, 0.03f);
            Sdf t = Sd.Rotate(Sd.Ellipse(0.27f, 0.94f, 0.08f, 0.15f), 0.27f, 0.94f, 30f);
            p.Shape(t, PugTongue);
            p.Line(PugTongueLine, 0.016f, 0.23f, 1.04f, 0.27f, 0.93f, 0.32f, 0.84f);
            p.Fill(Sd.Ellipse(0.23f, 0.97f, 0.022f, 0.012f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("pug_tongue", p);
        }

        /// <summary>Punta della lingua che spunta mentre dorme.</summary>
        public static DoodleArt PugBlep()
        {
            var p = PugPainter(-0.08f, 0.94f, 0.08f, 1.1f, 0.022f);
            p.Shape(Sd.Ellipse(0, 1.02f, 0.045f, 0.055f), PugTongue);
            p.Line(PugTongueLine, 0.01f, 0f, 1.04f, 0f, 0.99f);
            return new DoodleArt("pug_blep", p);
        }

        /// <summary>Bolla di moccio (centro in 0.1, 0: il punto 0,0 va sulla narice).</summary>
        public static DoodleArt PugSnot()
        {
            var p = new DoodlePainter(-0.02f, -0.12f, 0.22f, 0.12f, 220) { Wobble = 0f };
            p.FillFunc(Sd.Circle(0.1f, 0, 0.1f), (x, y) =>
            {
                float r = (float)Math.Sqrt((x - 0.1f) * (x - 0.1f) + y * y) / 0.1f;
                return Rgba.Hex("BFE8FF").WithAlpha(0.25f + 0.5f * r * r * r);
            });
            p.Line(Rgba.White.WithAlpha(0.9f), 0.014f, 0.05f, 0.03f, 0.08f, 0.06f);
            p.Shape(Sd.Circle(0.1f, 0, 0.1f), Rgba.Clear, 0.01f);
            return new DoodleArt("pug_snot", p);
        }

        /// <summary>Cuscino a cuscinotto con nappine (centro 0,0).</summary>
        public static DoodleArt PugCushion()
        {
            var p = new DoodlePainter(-1.35f, -0.42f, 1.35f, 0.32f, 110) { Ink = 0.05f };
            foreach (float x in new[] { -1.15f, 1.15f })
            {
                p.InkLine(0.025f, x, -0.1f, x + Math.Sign(x) * 0.1f, -0.3f);
                p.Shape(Sd.Circle(x + Math.Sign(x) * 0.11f, -0.33f, 0.055f), Gold, 0.025f);
            }
            Body(p, Sd.Box(0, 0, 1.18f, 0.24f, 0.22f), Rgba.Hex("4FB3A9"), Rgba.Hex("2F8C83"));
            p.Line(Rgba.Hex("A5E3DC"), 0.02f, -0.95f, 0.02f, 0.95f, 0.02f);
            foreach (float x in new[] { -0.55f, 0f, 0.55f }) p.Shape(Sd.Circle(x, 0.02f, 0.035f), Rgba.Hex("A5E3DC"), 0.02f);
            return new DoodleArt("pug_cushion", p);
        }

        /// <summary>Pepiera di vetro col tappo forato (tappo in alto, buchi in 0, 0.43).</summary>
        public static DoodleArt PepperShaker()
        {
            var p = new DoodlePainter(-0.3f, -0.42f, 0.3f, 0.5f, 140) { Ink = 0.04f };
            Sdf glass = Sd.Box(0, -0.06f, 0.19f, 0.3f, 0.09f);
            p.Fill(glass, Rgba.Hex("EEF6F8"));
            p.FillFunc(Sd.Intersect(glass, Sd.Box(0, -0.24f, 0.3f, 0.14f)), (x, y) => Noise.Value(x * 60f, y * 60f) > 0.55f ? Rgba.Hex("2B2233") : Rgba.Hex("6E6672"));
            p.Line(Rgba.White.WithAlpha(0.9f), 0.03f, -0.11f, 0.15f, -0.11f, -0.2f);
            p.Shape(glass, Rgba.Clear);
            Body(p, Sd.Box(0, 0.32f, 0.21f, 0.11f, 0.08f), Rgba.Hex("C9CFD6"), Rgba.Hex("8D93A6"));
            foreach (float x in new[] { -0.09f, 0f, 0.09f }) p.Fill(Sd.Circle(x, 0.37f, 0.018f), Ink);
            p.Fill(Sd.Circle(-0.045f, 0.3f, 0.018f), Ink);
            p.Fill(Sd.Circle(0.045f, 0.3f, 0.018f), Ink);
            return new DoodleArt("pepper_shaker", p);
        }

        /// <summary>Granello di pepe.</summary>
        public static DoodleArt PepperGrain()
        {
            var p = new DoodlePainter(-0.04f, -0.04f, 0.04f, 0.04f, 200) { Wobble = 0f };
            p.Fill(Sd.Circle(0, 0, 0.028f), Rgba.Hex("2B2233"));
            return new DoodleArt("pepper_grain", p);
        }
    }
}
