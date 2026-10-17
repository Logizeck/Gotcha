using System;

namespace Gotcha.EditorArt
{
    /// <summary>Il carlino (livelli 3 e 19) e il suo osso.</summary>
    public static partial class DoodleSprites
    {
        static readonly Rgba PugFawn = Rgba.Hex("E9C590"), PugShade = Rgba.Hex("C99A60"), PugMask = Rgba.Hex("3B302C");
        public const float PugEyeLX = -0.9f, PugEyeRX = -0.54f, PugEyeY = 0.47f, PugEyeR = 0.105f;
        public const float PugMouthX = -0.72f, PugMouthY = 0.13f;

        /// <summary>Carlino che dorme acciambellato sul cuscino (stesso ingombro del gatto: testa a sinistra).</summary>
        public static DoodleArt Pug()
        {
            var p = new DoodlePainter(-1.6f, -0.95f, 1.6f, 1.15f, 130) { Ink = 0.05f };
            Sdf cushion = Sd.Box(0, -0.45f, 1.35f, 0.3f, 0.26f);
            foreach (float x in new[] { -1.32f, 1.32f })
            {
                p.InkLine(0.025f, x, -0.6f, x + Math.Sign(x) * 0.12f, -0.82f);
                p.Shape(Sd.Circle(x + Math.Sign(x) * 0.13f, -0.85f, 0.06f), Rgba.Hex("FFD25A"), 0.025f);
            }
            Body(p, cushion, Rgba.Hex("4FA3E0"), Rgba.Hex("2F7DB5"));
            p.Line(Rgba.Hex("A8D8FF"), 0.02f, -1.1f, -0.45f, 1.1f, -0.45f);
            foreach (float x in new[] { -0.6f, 0f, 0.6f }) p.Shape(Sd.Circle(x, -0.45f, 0.035f), Rgba.Hex("A8D8FF"), 0.02f);
            // corpo cicciotto con le pieghe
            Sdf body = Sd.Ellipse(0.15f, 0.08f, 0.95f, 0.47f);
            Body(p, body, PugFawn, PugShade, null, -0.08f, 0.1f);
            for (int i = 0; i < 3; i++) p.Line(PugShade, 0.03f, 0.25f + i * 0.22f, 0.5f, 0.3f + i * 0.22f, 0.3f);
            // zampe posteriori e anteriori
            p.Shape(Sd.Ellipse(0.75f, -0.2f, 0.22f, 0.12f), PugFawn, 0.035f);
            foreach (float x in new[] { -0.5f, -0.22f })
            {
                p.Shape(Sd.Ellipse(x, -0.24f, 0.14f, 0.08f), PugFawn, 0.035f);
                for (int k = -1; k <= 1; k++) p.InkLine(0.012f, x - 0.1f, -0.24f + k * 0.03f, x - 0.13f, -0.24f + k * 0.03f);
            }
            // testa tonda con le orecchie nere ripiegate
            foreach (float s in new[] { -1f, 1f })
                Body(p, Sd.Rotate(Sd.Ellipse(-0.72f + s * 0.4f, 0.68f, 0.14f, 0.1f), -0.72f + s * 0.4f, 0.68f, s * -35f), PugMask, Rgba.Hex("1E1816"));
            Sdf head = Sd.Ellipse(-0.72f, 0.38f, 0.46f, 0.42f);
            Body(p, head, PugFawn, PugShade, null, -0.05f, 0.06f);
            // rughe sulla fronte
            p.Line(PugShade, 0.03f, -0.84f, 0.68f, -0.76f, 0.72f, -0.66f, 0.68f, -0.58f, 0.72f);
            p.Line(PugShade, 0.025f, -0.8f, 0.6f, -0.72f, 0.63f, -0.64f, 0.6f);
            // muso nero (la maschera del carlino)
            p.Shape(Sd.Ellipse(PugMouthX, 0.2f, 0.25f, 0.17f), PugMask, 0.03f);
            p.Fill(Sd.Ellipse(PugMouthX, 0.27f, 0.1f, 0.06f), Rgba.Hex("120D0C"));               // naso
            p.Fill(Sd.Circle(PugMouthX - 0.03f, 0.29f, 0.018f), Rgba.White.WithAlpha(0.6f));
            p.Line(Rgba.Hex("1E1816"), 0.02f, PugMouthX, 0.21f, PugMouthX, 0.15f);
            p.Line(Rgba.Hex("1E1816"), 0.02f, PugMouthX - 0.12f, 0.12f, PugMouthX, 0.15f, PugMouthX + 0.12f, 0.12f);
            // occhi chiusi (si coprono con gli occhi aperti quando è sveglio)
            foreach (float ex in new[] { PugEyeLX, PugEyeRX })   // occhi chiusi beati: archetti all'ingiù
                p.InkLine(0.026f, ex - 0.09f, PugEyeY + 0.02f, ex - 0.05f, PugEyeY - 0.02f, ex, PugEyeY - 0.035f, ex + 0.05f, PugEyeY - 0.02f, ex + 0.09f, PugEyeY + 0.02f);
            return new DoodleArt("pug", p);
        }

        /// <summary>Coda arricciata (attacco in 0,0): si scodinzola ruotandola.</summary>
        public static DoodleArt PugTail()
        {
            var p = new DoodlePainter(-0.1f, -0.1f, 0.45f, 0.45f, 160) { Ink = 0.035f };
            Sdf curl = Sd.Union(Sd.Subtract(Sd.Circle(0.2f, 0.24f, 0.15f), Sd.Circle(0.2f, 0.24f, 0.065f)), Sd.Capsule(0f, 0f, 0.08f, 0.15f, 0.07f));
            Body(p, curl, PugFawn, PugShade);
            p.Line(PugShade, 0.02f, 0.2f, 0.34f, 0.29f, 0.26f, 0.24f, 0.15f);
            return new DoodleArt("pug_tail", p);
        }

        /// <summary>Occhioni aperti del carlino (senza pupille: le pupille sono a parte e guardano l'osso).</summary>
        public static DoodleArt PugEyes(bool half)
        {
            var p = new DoodlePainter(-1.05f, 0.3f, -0.39f, 0.64f, 200) { Ink = 0.025f };
            foreach (float x in new[] { PugEyeLX, PugEyeRX })
            {
                p.Shape(Sd.Circle(x, PugEyeY, PugEyeR + 0.025f), Rgba.Hex("2B211E"), 0.015f);           // contorno scuro
                p.Fill(Sd.Circle(x, PugEyeY, PugEyeR), Rgba.Hex("FFFDF5"));
                if (half) p.Fill(Sd.Intersect(Sd.Circle(x, PugEyeY, PugEyeR + 0.03f), Sd.Box(x, PugEyeY + 0.08f, 0.15f, 0.08f)), PugFawn);   // palpebre a metà
                if (half) p.InkLine(0.02f, x - 0.11f, PugEyeY, x + 0.11f, PugEyeY);
            }
            return new DoodleArt(half ? "pug_eyes_half" : "pug_eyes", p);
        }

        /// <summary>Pupilla del carlino (centro 0,0): grande, nera, lucida.</summary>
        public static DoodleArt PugPupil()
        {
            var p = new DoodlePainter(-0.07f, -0.07f, 0.07f, 0.07f, 300) { Wobble = 0f };
            p.Fill(Sd.Circle(0, 0, 0.06f), Rgba.Hex("2A1A12"));
            p.Fill(Sd.Circle(0, 0, 0.035f), Rgba.Hex("120A06"));
            p.Fill(Sd.Circle(-0.02f, 0.022f, 0.016f), Rgba.White);
            return new DoodleArt("pug_pupil", p);
        }

        /// <summary>Bocca spalancata con la lingua (si sovrappone al muso).</summary>
        public static DoodleArt PugMouthOpen()
        {
            var p = new DoodlePainter(-1.0f, -0.12f, -0.44f, 0.3f, 220) { Ink = 0.025f };
            p.Shape(Sd.Ellipse(PugMouthX, 0.06f, 0.18f, 0.14f), Rgba.Hex("5A1424"), 0.025f);
            p.Fill(Sd.Ellipse(PugMouthX + 0.02f, 0.0f, 0.1f, 0.06f), Rgba.Hex("FF7A93"));
            for (int i = -1; i <= 1; i += 2) p.Shape(Sd.Polygon(PugMouthX + i * 0.11f, 0.17f, PugMouthX + i * 0.07f, 0.17f, PugMouthX + i * 0.09f, 0.1f), Rgba.White, 0.01f);   // dentini
            p.Fill(Sd.Ellipse(PugMouthX, 0.27f, 0.1f, 0.06f), Rgba.Hex("120D0C"));
            return new DoodleArt("pug_mouth_open", p);
        }

        /// <summary>Lingua che lecca i baffi.</summary>
        public static DoodleArt PugTongue()
        {
            var p = new DoodlePainter(-0.95f, -0.12f, -0.5f, 0.2f, 220) { Ink = 0.022f };
            p.Shape(Sd.Ellipse(PugMouthX + 0.08f, 0.02f, 0.08f, 0.1f), Rgba.Hex("FF7A93"), 0.02f);
            p.InkLine(0.01f, PugMouthX + 0.08f, 0.08f, PugMouthX + 0.08f, -0.04f);
            return new DoodleArt("pug_tongue", p);
        }

        /// <summary>Osso (centro 0,0, lungo 0.9).</summary>
        public static DoodleArt Bone()
        {
            var p = new DoodlePainter(-0.55f, -0.25f, 0.55f, 0.25f, 160) { Ink = 0.035f };
            Sdf bone = Sd.Union(Sd.Box(0, 0, 0.32f, 0.07f, 0.03f),
                Sd.Union(Sd.Union(Sd.Circle(-0.36f, 0.08f, 0.09f), Sd.Circle(-0.36f, -0.08f, 0.09f)), Sd.Union(Sd.Circle(0.36f, 0.08f, 0.09f), Sd.Circle(0.36f, -0.08f, 0.09f))));
            Body(p, bone, Rgba.Hex("FFF6E0"), Rgba.Hex("E2D2AE"));
            return new DoodleArt("bone", p);
        }
    }
}
