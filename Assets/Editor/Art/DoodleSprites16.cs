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

        /// <summary>Macchina fotografica sul treppiede, obiettivo verso sinistra (centro della lente in -0.74, 0.15).</summary>
        public static DoodleArt PhotoCamera()
        {
            var p = new DoodlePainter(-0.95f, -1.75f, 0.75f, 0.8f, 120) { Ink = 0.045f };
            foreach (float x in new[] { -0.45f, 0.1f, 0.65f }) p.InkLine(0.05f, 0.1f, -0.2f, x, -1.65f);
            foreach (float x in new[] { -0.45f, 0.1f, 0.65f }) p.Line(Rgba.Hex("8A5A3B"), 0.03f, 0.1f, -0.2f, x, -1.65f);
            p.Shape(Sd.Box(0.15f, 0.55f, 0.2f, 0.1f, 0.03f), Rgba.Hex("2A2E36"));
            p.Shape(Sd.Box(0.48f, 0.56f, 0.1f, 0.07f, 0.02f), Rgba.Hex("E8EEF5"));
            Body(p, Sd.Box(0.1f, 0.15f, 0.5f, 0.35f, 0.08f), Rgba.Hex("3A3F4A"), Rgba.Hex("23262D"));
            p.Fill(Sd.Box(0.1f, 0.15f, 0.5f, 0.06f), Rgba.Hex("8A5A3B"));
            Body(p, Sd.Box(-0.55f, 0.15f, 0.17f, 0.25f, 0.04f), Rgba.Hex("6B7280"), Rgba.Hex("4B5260"));
            p.Shape(Sd.Ellipse(-0.74f, 0.15f, 0.07f, 0.26f), Rgba.Hex("5FB4FF"), 0.03f);
            p.Fill(Sd.Ellipse(-0.75f, 0.24f, 0.025f, 0.07f), Rgba.White.WithAlpha(0.8f));
            p.Shape(Sd.Circle(0.45f, 0.38f, 0.045f), Rgba.Hex("FF4D5E"), 0.02f);
            return new DoodleArt("photo_camera", p);
        }

        /// <summary>Polaroid: cornice bianca, foto azzurra in alto (centro 0, 0.15), spazio per la didascalia in basso.</summary>
        public static DoodleArt Polaroid()
        {
            var p = new DoodlePainter(-0.92f, -1.12f, 0.92f, 1.08f, 110) { Ink = 0.04f };
            Body(p, Sd.Box(0, -0.03f, 0.85f, 1.02f, 0.03f), Rgba.Hex("FFFDF7"), Rgba.Hex("E6E0D2"));
            p.Shape(Sd.Box(0, 0.2f, 0.72f, 0.7f), Rgba.Hex("BFE3FF"), 0.025f);
            return new DoodleArt("polaroid", p);
        }

        /// <summary>Barattolo dei biscottini (centro 0,0).</summary>
        public static DoodleArt TreatJar()
        {
            var p = new DoodlePainter(-0.5f, -0.62f, 0.5f, 0.66f, 130) { Ink = 0.04f };
            Sdf jar = Sd.Box(0, -0.05f, 0.38f, 0.5f, 0.14f);
            p.Fill(jar, Rgba.Hex("DFF3FF").WithAlpha(0.7f));
            var rnd = new Random(25);
            for (int i = 0; i < 9; i++)
            {
                float x = -0.24f + (i % 3) * 0.24f + (float)(rnd.NextDouble() - 0.5) * 0.06f, y = -0.42f + (i / 3) * 0.16f;
                p.Shape(Sd.Rotate(Sd.Union(Sd.Box(x, y, 0.08f, 0.025f), Sd.Circle(x - 0.08f, y, 0.04f), Sd.Circle(x + 0.08f, y, 0.04f)), x, y, (float)(rnd.NextDouble() - 0.5) * 60f),
                    Rgba.Hex("E9B97A"), 0.015f);
            }
            p.Shape(jar, Rgba.Clear);
            p.Line(Rgba.White.WithAlpha(0.8f), 0.04f, -0.27f, 0.25f, -0.27f, -0.2f);
            Body(p, Sd.Box(0, 0.52f, 0.32f, 0.09f, 0.04f), Rgba.Hex("FF4D5E"), Rgba.Hex("C9283B"));
            p.Shape(Sd.Box(0, 0.08f, 0.27f, 0.12f, 0.03f), Rgba.Hex("FFF6E0"), 0.025f);
            return new DoodleArt("treat_jar", p);
        }

        /// <summary>Frigorifero aperto (l'interno illuminato): la porta è a parte, incernierata sul bordo destro (x 0.8).</summary>
        public static DoodleArt FridgeBody()
        {
            var p = new DoodlePainter(-0.88f, -1.85f, 0.88f, 1.8f, 90) { Ink = 0.05f };
            foreach (float x in new[] { -0.6f, 0.6f }) p.Shape(Sd.Box(x, -1.74f, 0.08f, 0.06f), Rgba.Hex("6B7280"), 0.03f);
            Body(p, Sd.Box(0, 0, 0.8f, 1.7f, 0.12f), Rgba.Hex("EEF3F7"), Rgba.Hex("C9D6E0"));
            p.Shape(Sd.Box(0, 0.02f, 0.66f, 1.55f, 0.06f), Rgba.Hex("FFFBE6"), 0.03f);
            p.FillFunc(Sd.Box(0, 0.02f, 0.66f, 1.55f, 0.06f), (x, y) => Rgba.Hex("FFF27A").WithAlpha((float)Math.Exp(-((x * x) + (y - 1.3) * (y - 1.3)) * 1.2) * 0.45f));
            foreach (float y in new[] { 0.75f, 0.0f, -0.75f }) p.Line(Rgba.Hex("A9C4D8"), 0.035f, -0.64f, y, 0.64f, y);
            // latte, formaggio, barattolo, uova
            p.Shape(Sd.Union(Sd.Box(-0.38f, 1.08f, 0.16f, 0.32f, 0.02f), Sd.Polygon(-0.54f, 1.4f, -0.22f, 1.4f, -0.38f, 1.52f)), Rgba.White);
            p.Fill(Sd.Box(-0.38f, 1.05f, 0.16f, 0.08f), Rgba.Hex("5FB4FF"));
            p.Shape(Sd.Polygon(0.05f, 0.78f, 0.5f, 0.78f, 0.5f, 1.02f), Rgba.Hex("FFD25A"));
            foreach (float x in new[] { -0.45f, -0.25f, -0.05f }) p.Shape(Sd.Ellipse(x, -0.62f, 0.08f, 0.11f), Rgba.Hex("FFF6E0"), 0.025f);
            p.Shape(Sd.Box(0.35f, -0.52f, 0.15f, 0.2f, 0.05f), Rgba.Hex("7ED36B"), 0.03f);
            p.Shape(Sd.Box(0f, -1.25f, 0.55f, 0.3f, 0.06f), Rgba.Hex("DDEBF5").WithAlpha(0.8f), 0.03f);
            return new DoodleArt("fridge_body", p);
        }

        /// <summary>Porta del frigo (cerniera in 0,0 a destra, maniglia a sinistra in -1.38).</summary>
        public static DoodleArt FridgeDoor()
        {
            var p = new DoodlePainter(-1.68f, -1.82f, 0.1f, 1.82f, 90) { Ink = 0.05f };
            Body(p, Sd.Box(-0.8f, 0, 0.8f, 1.7f, 0.12f), Rgba.Hex("F4F8FB"), Rgba.Hex("C9D6E0"));
            p.Shape(Sd.Capsule(-1.4f, 0.1f, -1.4f, 0.95f, 0.06f), Rgba.Hex("B8C2CC"), 0.03f);
            p.Shape(Sd.Rotate(Sd.Box(-0.7f, 0.85f, 0.3f, 0.3f), -0.7f, 0.85f, -6f), Rgba.Hex("FFF27A"), 0.025f);   // il disegno di Toni attaccato
            p.Shape(Sd.Ellipse(-0.7f, 0.82f, 0.16f, 0.1f), Rgba.Hex("E9C590"), 0.02f);
            p.Fill(Sd.Ellipse(-0.82f, 0.82f, 0.07f, 0.05f), Rgba.Hex("3B302C"));
            p.Shape(Sd.Union(Sd.Box(-0.6f, -0.2f, 0.14f, 0.04f), Sd.Circle(-0.74f, -0.17f, 0.05f), Sd.Circle(-0.74f, -0.23f, 0.05f), Sd.Circle(-0.46f, -0.17f, 0.05f), Sd.Circle(-0.46f, -0.23f, 0.05f)), Rgba.Hex("FF8FC7"), 0.02f);
            return new DoodleArt("fridge_door", p);
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

        /// <summary>Piuma per il solletico (la punta che fa il solletico è in 0,0).</summary>
        public static DoodleArt Feather()
        {
            var p = new DoodlePainter(-0.12f, -0.15f, 1.25f, 0.68f, 140) { Ink = 0.03f };
            Sdf vane = Sd.Rotate(Sd.Ellipse(0.62f, 0.28f, 0.55f, 0.15f), 0.62f, 0.28f, 25f);
            Body(p, vane, Rgba.Hex("FF8FC7"), Rgba.Hex("E05FA3"));
            for (int i = 1; i < 9; i++)
            {
                float t = i / 9f, x = 0.1f + t * 1.0f, y = x * 0.47f;
                p.Line(Rgba.Hex("E05FA3"), 0.012f, x, y, x - 0.05f, y + 0.12f);
                p.Line(Rgba.Hex("E05FA3"), 0.012f, x, y, x + 0.06f, y - 0.11f);
            }
            p.InkLine(0.022f, 0f, 0f, 1.15f, 0.54f);
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
    }
}
