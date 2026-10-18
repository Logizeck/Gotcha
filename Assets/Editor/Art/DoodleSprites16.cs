using System;

namespace Gotcha.EditorArt
{
    /// <summary>Oggetti dei livelli di Toni (25, 50, 75, 100).</summary>
    public static partial class DoodleSprites
    {
        /// <summary>Pallina di gomma di Toni (centro 0,0).</summary>
        public static DoodleArt ToniBall()
        {
            var p = new DoodlePainter(-0.27f, -0.27f, 0.27f, 0.27f, 200) { Ink = 0.03f };
            Sdf ball = Sd.Circle(0, 0, 0.22f);
            Body(p, ball, Rgba.Hex("FF4D5E"), Rgba.Hex("C9283B"));
            p.Fill(Sd.Intersect(ball, Sd.Rotate(Sd.Box(0, 0, 0.3f, 0.05f), 0, 0, 30f)), Rgba.Hex("FFD25A"));
            p.Shape(ball, Rgba.Clear);
            p.Fill(Sd.Ellipse(-0.08f, 0.1f, 0.05f, 0.03f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("toni_ball", p);
        }

        /// <summary>Filza di salsicce (centro 0,0).</summary>
        public static DoodleArt Sausages()
        {
            var p = new DoodlePainter(-0.65f, -0.22f, 0.65f, 0.22f, 150) { Ink = 0.03f };
            for (int i = -1; i <= 1; i++)
            {
                float x = i * 0.38f, y = i == 0 ? 0.02f : -0.02f;
                Body(p, Sd.Rotate(Sd.Capsule(x - 0.14f, y, x + 0.14f, y, 0.1f), x, y, i * 12f), Rgba.Hex("D9705E"), Rgba.Hex("A84A3C"));
                if (i < 1) p.InkLine(0.015f, x + 0.17f, y, x + 0.21f, y);
            }
            return new DoodleArt("sausages", p);
        }

        /// <summary>Piuma di pavone per il solletico: l'"occhio" colorato è la punta che fa il solletico (in 0,0), il gambo va verso destra.</summary>
        public static DoodleArt Feather()
        {
            var p = new DoodlePainter(-0.32f, -0.3f, 1.45f, 0.75f, 140) { Ink = 0.025f };
            // barbe sottili verdi lungo tutto il gambo, più lunghe verso l'occhio
            for (int i = 0; i < 26; i++)
            {
                float t = i / 25f, x = 0.15f + t * 1.15f, y = x * 0.42f, len = 0.24f * (1f - t * 0.75f);
                Rgba c = Rgba.Lerp(Rgba.Hex("2E9E5B"), Rgba.Hex("8FD14F"), t);
                p.Line(c, 0.018f, x, y, x - 0.1f, y + len);
                p.Line(c, 0.018f, x, y, x + 0.08f, y - len);
            }
            // l'occhio: ovali concentrici oro, verde, blu, blu notte
            Sdf eye = Sd.Rotate(Sd.Ellipse(0.06f, 0.02f, 0.3f, 0.24f), 0.06f, 0.02f, 23f);
            p.Shape(eye, Rgba.Hex("D9B23A"), 0.03f);
            p.Fill(Sd.Rotate(Sd.Ellipse(0.04f, 0.01f, 0.22f, 0.17f), 0.04f, 0.01f, 23f), Rgba.Hex("3FAE6A"));
            p.Fill(Sd.Rotate(Sd.Ellipse(0.02f, 0.0f, 0.15f, 0.12f), 0.02f, 0.0f, 23f), Rgba.Hex("2F7DE0"));
            p.Fill(Sd.Rotate(Sd.Ellipse(0.0f, -0.01f, 0.08f, 0.065f), 0.0f, -0.01f, 23f), Rgba.Hex("1A2A6C"));
            p.Fill(Sd.Circle(-0.03f, 0.02f, 0.02f), Rgba.White.WithAlpha(0.8f));
            p.InkLine(0.03f, 0.22f, 0.09f, 1.4f, 0.6f);   // il gambo
            p.Line(Rgba.Hex("F4EAD0"), 0.014f, 0.24f, 0.1f, 1.38f, 0.59f);
            return new DoodleArt("feather", p);
        }

        /// <summary>Torta per cani con l'osso sopra (base del piatto in y -0.55; la candelina va in 0.3, 0.12).</summary>
        public static DoodleArt DogCake()
        {
            var p = new DoodlePainter(-0.85f, -0.65f, 0.85f, 0.42f, 120) { Ink = 0.04f };
            p.Shape(Sd.Ellipse(0, -0.55f, 0.82f, 0.08f), Rgba.Hex("E8EEF5"));
            Body(p, Sd.Box(0, -0.25f, 0.62f, 0.28f, 0.08f), Rgba.Hex("C98B5A"), Rgba.Hex("9C6438"));
            var drips = new System.Collections.Generic.List<Sdf> { Sd.Box(0, 0.02f, 0.66f, 0.1f, 0.07f) };
            foreach (float x in new[] { -0.5f, -0.22f, 0.12f, 0.45f }) drips.Add(Sd.Capsule(x, 0f, x, -0.14f - Math.Abs(x) * 0.1f, 0.05f));
            p.Shape(Sd.Union(drips.ToArray()), Rgba.Hex("FF9FC4"), 0.035f);
            p.Shape(Sd.Union(Sd.Box(-0.15f, 0.17f, 0.2f, 0.045f), Sd.Circle(-0.35f, 0.21f, 0.06f), Sd.Circle(-0.35f, 0.13f, 0.06f), Sd.Circle(0.05f, 0.21f, 0.06f), Sd.Circle(0.05f, 0.13f, 0.06f)),
                Rgba.Hex("FFF6E0"), 0.025f);
            foreach (float x in new[] { -0.45f, 0f, 0.4f }) p.Fill(Sd.Circle(x, -0.3f, 0.04f), Rgba.Hex("FFD25A"));
            return new DoodleArt("dog_cake", p);
        }

        /// <summary>Sezione del muro vista di lato (casa a sinistra, giardino a destra) con il buco della gattaiola in y -1.92.</summary>
        public static DoodleArt WallSection()
        {
            var p = new DoodlePainter(-0.36f, -3.35f, 0.36f, 5.2f, 80) { Ink = 0.05f };
            Sdf hole = Sd.Box(0, -1.92f, 0.4f, 0.4f, 0.05f);
            Sdf wall = Sd.Subtract(Sd.Box(0, 0.9f, 0.27f, 4.25f), hole);
            Body(p, wall, Rgba.Hex("EADBC8"), Rgba.Hex("CDB89C"));
            for (float y = -3f; y < 5f; y += 0.42f)
                if (y < -2.4f || y > -1.4f) p.Line(Rgba.Hex("CDB89C"), 0.02f, -0.24f, y, 0.24f, y);
            // cornice della gattaiola
            foreach (float y in new[] { -1.52f, -2.32f }) p.Shape(Sd.Box(0, y, 0.3f, 0.05f, 0.02f), Rgba.Hex("8A5A3B"), 0.025f);
            return new DoodleArt("wall_section", p);
        }

        /// <summary>Lo sportello della gattaiola visto di taglio (cerniera in 0,0, pende verso il basso).</summary>
        public static DoodleArt DogFlap()
        {
            var p = new DoodlePainter(-0.1f, -0.9f, 0.1f, 0.06f, 120) { Ink = 0.03f };
            Body(p, Sd.Box(0, -0.42f, 0.05f, 0.44f, 0.03f), Rgba.Hex("B98552"), Rgba.Hex("8A5A3B"));
            p.Shape(Sd.Circle(0, 0, 0.045f), Rgba.Hex("B8C2CC"), 0.02f);
            return new DoodleArt("dog_flap", p);
        }

        /// <summary>Panetto di burro sul piattino (centro 0,0).</summary>
        public static DoodleArt Butter()
        {
            var p = new DoodlePainter(-0.5f, -0.22f, 0.5f, 0.3f, 150) { Ink = 0.03f };
            p.Shape(Sd.Ellipse(0, -0.13f, 0.47f, 0.07f), Rgba.Hex("E8EEF5"));
            Body(p, Sd.Box(0.02f, 0.03f, 0.3f, 0.13f, 0.04f), Rgba.Hex("FFE27A"), Rgba.Hex("E8C04A"));
            p.Fill(Sd.Box(-0.18f, 0.03f, 0.13f, 0.135f, 0.02f), Rgba.Hex("FFFFFF").WithAlpha(0.85f));   // la carta aperta
            p.Line(Rgba.Hex("5FB4FF"), 0.02f, -0.27f, 0.05f, -0.09f, 0.05f);
            p.Fill(Sd.Ellipse(0.12f, 0.1f, 0.06f, 0.025f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("butter", p);
        }

        /// <summary>Calzino a righe (il bordo in alto in 0, 0.25: da lì pende dalla bocca di Toni).</summary>
        public static DoodleArt Sock()
        {
            var p = new DoodlePainter(-0.3f, -0.62f, 0.42f, 0.32f, 150) { Ink = 0.03f };
            Sdf sock = Sd.Union(Sd.Capsule(0, 0.18f, 0, -0.35f, 0.13f), Sd.Capsule(0, -0.38f, 0.24f, -0.42f, 0.13f));
            p.Fill(sock, Rgba.White);
            p.Hatch(sock, Rgba.Hex("FF4D5E"), 0.14f, 0.06f, 0f);
            p.Shape(sock, Rgba.Clear);
            p.Shape(Sd.Box(0, 0.22f, 0.15f, 0.06f, 0.03f), Rgba.Hex("3E6FE0"), 0.025f);
            p.Shape(Sd.Circle(0.12f, -0.38f, 0.04f), Rgba.Hex("F2D9B8"), 0.015f);   // il buco sul ditone
            return new DoodleArt("sock", p);
        }

        /// <summary>Spiaccicata di panna sullo schermo.</summary>
        public static DoodleArt CreamSplat()
        {
            var p = new DoodlePainter(-1.7f, -1.7f, 1.7f, 1.7f, 80) { Ink = 0.05f };
            var rnd = new Random(100);
            var blobs = new System.Collections.Generic.List<Sdf> { Sd.Circle(0, 0, 0.95f) };
            for (int i = 0; i < 11; i++)
            {
                double a = i / 11.0 * Math.PI * 2 + rnd.NextDouble() * 0.3;
                float r = 0.95f + (float)rnd.NextDouble() * 0.45f;
                blobs.Add(Sd.Capsule(0, 0, (float)Math.Cos(a) * r, (float)Math.Sin(a) * r, 0.12f + (float)rnd.NextDouble() * 0.1f));
                blobs.Add(Sd.Circle((float)Math.Cos(a) * (r + 0.12f), (float)Math.Sin(a) * (r + 0.12f), 0.14f));
            }
            Sdf splat = Sd.Union(blobs.ToArray());
            Body(p, splat, Rgba.Hex("FFF8F0"), Rgba.Hex("F1DCCB"));
            foreach (var c in new[] { new[] { -0.4f, 0.3f }, new[] { 0.35f, -0.2f }, new[] { 0.1f, 0.5f }, new[] { -0.2f, -0.45f } })
                p.Shape(Sd.Circle(c[0], c[1], 0.12f), Rgba.Hex("FF9FC4"), 0.025f);
            p.Shape(Sd.Union(Sd.Box(0.3f, 0.25f, 0.2f, 0.045f), Sd.Circle(0.1f, 0.29f, 0.06f), Sd.Circle(0.1f, 0.21f, 0.06f), Sd.Circle(0.5f, 0.29f, 0.06f), Sd.Circle(0.5f, 0.21f, 0.06f)), Rgba.Hex("FFF6E0"), 0.025f);
            return new DoodleArt("cream_splat", p);
        }

        /// <summary>
        /// Il muso di Toni in primo piano, di fronte, schiacciato contro lo schermo (livello 100: lecca la panna).
        /// Centro 0,0; la bocca aperta (da cui esce la lingua) è in 0, -1.1.
        /// </summary>
        public static DoodleArt ToniFaceBig()
        {
            var p = new DoodlePainter(-2.4f, -2.2f, 2.4f, 2.1f, 70) { Ink = 0.09f };
            // orecchie ripiegate
            foreach (float sx in new[] { -1f, 1f })
                Body(p, Sd.Rotate(Sd.Ellipse(sx * 1.55f, 1.45f, 0.6f, 0.38f), sx * 1.55f, 1.45f, sx * -30f), PugShade, Rgba.Hex("141418"));
            Sdf head = Sd.Ellipse(0, 0, 2.05f, 1.75f);
            Body(p, head, PugFawn, PugShade, null, -0.25f, 0.3f);
            // rughe sulla fronte
            p.Line(PugLine, 0.07f, -0.7f, 1.2f, -0.35f, 1.35f, 0f, 1.25f, 0.35f, 1.35f, 0.7f, 1.2f);
            p.Line(PugLine, 0.06f, -0.45f, 0.95f, 0f, 1.05f, 0.45f, 0.95f);
            // occhioni lucidi che guardano la panna
            foreach (float sx in new[] { -1f, 1f })
            {
                p.Shape(Sd.Circle(sx * 0.82f, 0.38f, 0.5f), Rgba.Hex("2B211E"), 0.04f);
                p.Fill(Sd.Circle(sx * 0.82f, 0.38f, 0.43f), Rgba.Hex("FFFDF5"));
                p.Fill(Sd.Circle(sx * 0.78f, 0.5f, 0.27f), Rgba.Hex("2A1A12"));
                p.Fill(Sd.Circle(sx * 0.7f, 0.62f, 0.09f), Rgba.White);
            }
            // muso schiacciato col naso grande
            Body(p, Sd.Ellipse(0, -0.75f, 1.15f, 0.85f), PugMask, PugShade, null, -0.1f, 0.15f);
            p.Shape(Sd.Ellipse(0, -0.35f, 0.42f, 0.26f), Rgba.Hex("120D0C"), 0.04f);
            p.Fill(Sd.Ellipse(-0.12f, -0.27f, 0.12f, 0.06f), Rgba.White.WithAlpha(0.55f));
            p.Line(PugLine, 0.05f, 0f, -0.6f, 0f, -0.85f);
            // bocca aperta (la lingua esce da qui)
            p.Shape(Sd.Ellipse(0, -1.1f, 0.62f, 0.32f), Rgba.Hex("5A1424"), 0.05f);
            return new DoodleArt("toni_face_big", p);
        }

        /// <summary>Linguona di Toni, dritta verso l'alto (base in 0,0, lunga 2.2): si allunga e si gira per leccare.</summary>
        public static DoodleArt ToniTongueBig()
        {
            var p = new DoodlePainter(-0.55f, -0.1f, 0.55f, 2.3f, 90) { Ink = 0.06f };
            Sdf tongue = Sd.Union(Sd.Capsule(0, 0.2f, 0, 1.75f, 0.42f), Sd.Ellipse(0, 1.85f, 0.46f, 0.4f));
            Body(p, tongue, Rgba.Hex("FF7A93"), Rgba.Hex("E0566F"), null, -0.12f, 0.1f);
            p.Line(Rgba.Hex("D94A64"), 0.06f, 0f, 0.4f, 0f, 1.7f);   // il solco in mezzo
            p.Fill(Sd.Ellipse(-0.2f, 1.6f, 0.08f, 0.2f), Rgba.White.WithAlpha(0.5f));
            return new DoodleArt("toni_tongue_big", p);
        }

        /// <summary>Uno schizzo di panna (centro 0,0): tanti formano la torta spiaccicata sul vetro.</summary>
        public static DoodleArt CreamBlob()
        {
            var p = new DoodlePainter(-0.75f, -0.7f, 0.75f, 0.7f, 90) { Ink = 0.05f };
            var rnd = new Random(7);
            var parts = new System.Collections.Generic.List<Sdf> { Sd.Circle(0, 0, 0.42f) };
            for (int i = 0; i < 7; i++)
            {
                double a = i / 7.0 * Math.PI * 2 + rnd.NextDouble() * 0.4;
                float r = 0.3f + (float)rnd.NextDouble() * 0.2f;
                parts.Add(Sd.Circle((float)Math.Cos(a) * r, (float)Math.Sin(a) * r * 0.9f, 0.14f + (float)rnd.NextDouble() * 0.1f));
            }
            Sdf blob = Sd.Union(parts.ToArray());
            Body(p, blob, Rgba.Hex("FFF8F0"), Rgba.Hex("F1DCCB"));
            p.Shape(Sd.Circle(0.12f, 0.1f, 0.08f), Rgba.Hex("FF9FC4"), 0.02f);
            return new DoodleArt("cream_blob", p);
        }
    }
}
