using System;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni della versione 0.26 (livelli con meccaniche nuove).</summary>
    public static partial class DoodleSprites
    {
        // ---------- 60: la torcia ----------

        /// <summary>Pipistrello appeso a testa in giù che dorme (attacco in alto in 0,0) o sveglio con le ali aperte.</summary>
        public static DoodleArt Bat(bool awake)
        {
            var p = new DoodlePainter(-0.75f, -0.75f, 0.75f, 0.1f, 140) { Ink = 0.035f };
            Rgba fur = Rgba.Hex("4A3C5C"), shade = Rgba.Hex("2E2440");
            p.InkLine(0.03f, -0.06f, 0f, -0.06f, -0.12f);
            p.InkLine(0.03f, 0.06f, 0f, 0.06f, -0.12f);
            if (awake)
            {
                foreach (float s in new[] { -1f, 1f })
                    Body(p, Sd.Polygon(s * 0.12f, -0.25f, s * 0.7f, -0.05f, s * 0.6f, -0.35f, s * 0.68f, -0.6f, s * 0.12f, -0.45f), shade, Rgba.Hex("1E1830"));
            }
            else
                foreach (float s in new[] { -1f, 1f })
                    Body(p, Sd.Ellipse(s * 0.16f, -0.35f, 0.12f, 0.25f), shade, Rgba.Hex("1E1830"));
            Body(p, Sd.Ellipse(0, -0.33f, 0.17f, 0.24f), fur, shade);
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Polygon(s * 0.06f, -0.55f, s * 0.15f, -0.72f, s * 0.14f, -0.52f), fur, 0.02f);   // orecchie (in basso: è capovolto)
            if (awake)
            {
                foreach (float s in new[] { -1f, 1f }) { p.Shape(Sd.Circle(s * 0.07f, -0.43f, 0.045f), Rgba.Hex("FFE14A"), 0.015f); p.Fill(Sd.Circle(s * 0.07f, -0.43f, 0.018f), Rgba.Hex("D11F2E")); }
                p.Shape(Sd.Ellipse(0, -0.3f, 0.06f, 0.04f), Rgba.Hex("3A0F1E"), 0.015f);
            }
            else
                foreach (float s in new[] { -1f, 1f }) p.InkLine(0.018f, s * 0.03f, -0.44f, s * 0.11f, -0.42f);
            return new DoodleArt(awake ? "bat_awake" : "bat_sleep", p);
        }

        /// <summary>Interruttore della luce a levetta (centro 0,0).</summary>
        public static DoodleArt LightSwitch(bool on)
        {
            var p = new DoodlePainter(-0.3f, -0.4f, 0.3f, 0.4f, 150) { Ink = 0.03f };
            Body(p, Sd.Box(0, 0, 0.22f, 0.32f, 0.05f), Rgba.Hex("F4F1E8"), Rgba.Hex("CFC8B6"));
            p.Shape(Sd.Box(0, 0, 0.07f, 0.16f, 0.03f), Rgba.Hex("D9D2C0"), 0.02f);
            p.Shape(Sd.Box(0, on ? 0.08f : -0.08f, 0.055f, 0.07f, 0.03f), Rgba.Hex("FFFFFF"), 0.02f);
            return new DoodleArt(on ? "switch_on" : "switch_off", p);
        }

        // ---------- 67: lo specchio appannato ----------

        /// <summary>Bagno: piastrelle azzurre, mensola, lavandino (coordinate del livello).</summary>
        public static DoodleArt Bathroom()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 4.6f, 7.1f), (x, y) =>
            {
                float fx = x / 0.6f - (float)Math.Floor(x / 0.6f), fy = y / 0.6f - (float)Math.Floor(y / 0.6f);
                bool grout = fx < 0.05f || fy < 0.05f;
                return grout ? Rgba.Hex("BFD9E6") : Rgba.Hex("E3F2F8");
            });
            Body(p, Sd.Box(0, -5f, 4.6f, 2.2f), Rgba.Hex("D8C3A5"), Rgba.Hex("BFA582"));
            Body(p, Sd.Box(0, -1.9f, 1.5f, 0.2f, 0.08f), Rgba.White, Rgba.Hex("D6E0E8"));   // lavandino
            Body(p, Sd.Box(0, -2.6f, 0.25f, 0.7f, 0.05f), Rgba.White, Rgba.Hex("D6E0E8"));
            Body(p, Sd.Box(0.1f, -1.55f, 0.06f, 0.18f, 0.03f), Rgba.Hex("C9CFD6"), Rgba.Hex("8D93A6"));   // rubinetto
            Body(p, Sd.Box(0.2f, -1.42f, 0.14f, 0.05f, 0.02f), Rgba.Hex("C9CFD6"), Rgba.Hex("8D93A6"));
            return new DoodleArt("bathroom_bg", p);
        }

        /// <summary>Specchio ovale con la cornice (centro 0,0, largo 2.4, alto 3).</summary>
        public static DoodleArt Mirror()
        {
            var p = new DoodlePainter(-1.35f, -1.65f, 1.35f, 1.65f, 100) { Ink = 0.05f };
            Body(p, Sd.Ellipse(0, 0, 1.25f, 1.55f), Rgba.Hex("C9A36A"), Rgba.Hex("9C7A45"));
            Sdf glass = Sd.Ellipse(0, 0, 1.08f, 1.38f);
            p.FillFunc(glass, (x, y) => Rgba.Lerp(Rgba.Hex("D9EEF7"), Rgba.Hex("9CC3D6"), Clamp((y + 1.3f) / 2.6f)));
            p.Line(Rgba.White.WithAlpha(0.7f), 0.06f, -0.6f, 0.9f, -0.2f, 1.2f);
            p.Line(Rgba.White.WithAlpha(0.5f), 0.04f, -0.75f, 0.55f, -0.45f, 0.8f);
            p.Shape(glass, Rgba.Clear, 0.03f);
            return new DoodleArt("mirror", p);
        }

        /// <summary>Velo di condensa sullo specchio (ovale bianco sfumato, stesso centro).</summary>
        public static DoodleArt MirrorFog()
        {
            var p = new DoodlePainter(-1.1f, -1.4f, 1.1f, 1.4f, 80) { Wobble = 0f };
            p.FillFunc(Sd.Ellipse(0, 0, 1.08f, 1.38f), (x, y) =>
                Rgba.Hex("FFFFFF").WithAlpha(0.75f + Noise.Value(x * 5f, y * 5f) * 0.2f));
            return new DoodleArt("mirror_fog", p);
        }

        /// <summary>Armadietto del bagno con la tastiera (base in 0,0).</summary>
        public static DoodleArt Cabinet(bool open)
        {
            var p = new DoodlePainter(-0.85f, -0.08f, 0.85f, 1.5f, 110) { Ink = 0.045f };
            Body(p, Sd.Box(0, 0.7f, 0.75f, 0.7f, 0.05f), Rgba.Hex("F4F1E8"), Rgba.Hex("CFC8B6"));
            if (open)
            {
                p.Fill(Sd.Box(0, 0.7f, 0.62f, 0.58f), Rgba.Hex("6B5A4A"));
                Body(p, Sd.Box(-0.3f, 0.5f, 0.08f, 0.3f, 0.04f), Rgba.Hex("6FC3F2"), Rgba.Hex("2F7DB5"));     // dentifricio
                Body(p, Sd.Box(0.1f, 0.55f, 0.04f, 0.32f, 0.02f), Rgba.Hex("FF5A4E"), Rgba.Hex("B92E2A"));    // spazzolino
                Body(p, Sd.Circle(0.4f, 0.4f, 0.14f), Rgba.Hex("FFD54A"), Rgba.Hex("E8B42A"));                 // paperella
            }
            else
            {
                p.Shape(Sd.Box(0, 0.7f, 0.62f, 0.58f, 0.04f), Rgba.Hex("E9E4D6"), 0.03f);
                p.Shape(Sd.Box(0.45f, 0.7f, 0.12f, 0.2f, 0.03f), Rgba.Hex("2B2133"), 0.02f);   // tastierino
                for (int i = 0; i < 6; i++) p.Fill(Sd.Circle(0.41f + (i % 2) * 0.08f, 0.6f + (i / 2) * 0.09f, 0.025f), Rgba.Hex("5DF08A"));
            }
            return new DoodleArt(open ? "cabinet_open" : "cabinet", p);
        }

        // ---------- 81: il funambolo ----------

        /// <summary>Funambolo con l'asta (piedi in 0,0 sul filo, testa verso l'alto). Il livello lo fa oscillare dai piedi.</summary>
        public static DoodleArt Acrobat()
        {
            var p = new DoodlePainter(-1.3f, -0.08f, 1.3f, 1.9f, 120) { Ink = 0.04f };
            p.Line(Rgba.Hex("8A5A3B"), 0.05f, -1.25f, 1.05f, 1.25f, 1.05f);   // asta
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Circle(s * 1.25f, 1.05f, 0.07f), Rgba.Hex("E8433F"), 0.02f);
            p.InkLine(0.06f, -0.12f, 0f, -0.05f, 0.55f);
            p.InkLine(0.06f, 0.12f, 0f, 0.05f, 0.55f);
            Body(p, Sd.Box(0, 0.85f, 0.2f, 0.32f, 0.08f), Rgba.Hex("3E6FE0"), Rgba.Hex("2A4FAE"));   // tutina
            for (int i = 0; i < 3; i++) p.Fill(Sd.Circle(0, 0.7f + i * 0.13f, 0.03f), Rgba.Hex("FFE14A"));
            p.InkLine(0.05f, -0.18f, 1.05f, -0.45f, 1.05f);
            p.InkLine(0.05f, 0.18f, 1.05f, 0.45f, 1.05f);
            Body(p, Sd.Circle(0, 1.38f, 0.2f), Rgba.Hex("FFD9C2"), Rgba.Hex("F2B79C"));
            Body(p, Sd.Intersect(Sd.Circle(0, 1.42f, 0.21f), Sd.Box(0, 1.55f, 0.25f, 0.1f)), Rgba.Hex("6B3A1E"), Rgba.Hex("4A2512"));
            foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Circle(s * 0.07f, 1.38f, 0.025f), Ink);
            p.InkLine(0.02f, -0.06f, 1.28f, 0f, 1.25f, 0.06f, 1.28f);
            return new DoodleArt("acrobat", p);
        }

        // ---------- 86: l'aeroplanino di carta ----------

        /// <summary>Cielo di giorno con nuvole e colline (coordinate del livello).</summary>
        public static DoodleArt SkyBackground()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 4.6f, 7.1f), (x, y) => Rgba.Lerp(Rgba.Hex("BFE6FA"), Rgba.Hex("6FC0EE"), Clamp((y + 4f) / 10f)));
            foreach (var c in new[] { (-2.3f, 4.6f, 1f), (1.9f, 3.2f, 0.8f), (-0.6f, 1.4f, 0.6f), (2.6f, 5.9f, 0.7f) })
            {
                Sdf cloud = Sd.Union(Sd.Ellipse(c.Item1, c.Item2, 0.75f * c.Item3, 0.32f * c.Item3),
                    Sd.Union(Sd.Circle(c.Item1 - 0.3f * c.Item3, c.Item2 + 0.15f * c.Item3, 0.32f * c.Item3), Sd.Circle(c.Item1 + 0.25f * c.Item3, c.Item2 + 0.2f * c.Item3, 0.38f * c.Item3)));
                p.Shape(cloud, Rgba.White.WithAlpha(0.95f), 0.03f);
            }
            Body(p, Sd.Union(Sd.Ellipse(-2.5f, -4.6f, 3.4f, 1.3f), Sd.Ellipse(2.6f, -4.9f, 3.2f, 1.2f)), Rgba.Hex("8FD16B"), Rgba.Hex("5FA846"));
            Body(p, Sd.Box(0, -6f, 4.6f, 1.6f), Leaf, LeafShade);
            return new DoodleArt("sky_bg", p);
        }

        /// <summary>Anello da attraversare (centro 0,0, diametro 0.9) visto di taglio.</summary>
        public static DoodleArt Hoop() { return Hoop(false); }

        /// <summary>Anello: arancione da attraversare, verde quando è stato attraversato.</summary>
        public static DoodleArt Hoop(bool done)
        {
            var p = new DoodlePainter(-0.35f, -0.55f, 0.35f, 0.55f, 150) { Ink = 0.035f };
            p.Shape(Sd.Subtract(Sd.Ellipse(0, 0, 0.27f, 0.47f), Sd.Ellipse(0, 0, 0.15f, 0.36f)), done ? Rgba.Hex("5DE07A") : Rgba.Hex("FF7A3D"), 0.03f);
            p.Line(Rgba.White.WithAlpha(0.6f), 0.03f, -0.18f, 0.25f, -0.12f, 0.35f);
            return new DoodleArt(done ? "hoop_done" : "hoop", p);
        }


        public static DoodleArt PaperPlane()
        {
            var p = new DoodlePainter(-0.55f, -0.3f, 0.6f, 0.3f, 150) { Ink = 0.03f };
            p.Shape(Sd.Polygon(-0.5f, 0.05f, 0.55f, 0f, -0.35f, 0.25f), Rgba.White, 0.025f);
            p.Shape(Sd.Polygon(-0.5f, 0.05f, 0.55f, 0f, -0.3f, -0.12f), Rgba.Hex("DCE6F2"), 0.025f);
            p.Shape(Sd.Polygon(-0.3f, -0.12f, 0.2f, -0.03f, -0.2f, -0.25f), Rgba.Hex("C3D2E6"), 0.02f);
            return new DoodleArt("paper_plane", p);
        }
    }
}
