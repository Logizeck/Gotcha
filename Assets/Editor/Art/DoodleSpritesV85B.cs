using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.85 per il livello "Mirror Liar" (roadmap n. 91): lo sfondo (una stanza con la carta da
    /// parati a righe e rombi, lo zoccolo e il pavimento di legno a doghe), lo specchio verticale in mezzo in tre pezzi
    /// (il vetro argentato con i riflessi, la cornice dorata coi piedini, le crepe che compaiono quando il falso si
    /// rivela), il personaggio (radice ai piedi, senza faccia: ciuffo e spilla a stella da una parte sola, così si vede
    /// che l'altro è il riflesso), quattro facce (normale, occhi chiusi, felice, preoccupata), le crepe sul personaggio
    /// falso e una scheggia di vetro. L'ombra è "ball_shadow", la scintilla "sparkle_star". Nessuna scritta disegnata.
    /// Prefisso v85_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV85B() => new DoodleArt[]
        {
            V85B_Bg(), V85B_MirrorGlass(), V85B_MirrorFrame(), V85B_MirrorCrack(),
            V85B_Guy(), V85B_FaceOpen(), V85B_FaceBlink(), V85B_FaceSmile(), V85B_FaceWorry(),
            V85B_GuyCrack(), V85B_Shard(),
        };

        // misure condivise col builder (le stesse della logica MirrorLiarLogic)
        /// <summary>Centro della testa del personaggio rispetto ai piedi, e raggio.</summary>
        public const float V85BHeadY = 1.58f, V85BHeadR = 0.34f;
        /// <summary>Lo specchio: mezza larghezza del vetro, base e cima (la cornice è appena fuori).</summary>
        public const float V85BGlassHx = 0.24f, V85BMirrorBottom = -3.75f, V85BMirrorTop = 1.95f;

        static readonly Rgba V85BSkin = Rgba.Hex("F5C9A0"), V85BSkinShade = Rgba.Hex("DDA67C");
        static readonly Rgba V85BHair = Rgba.Hex("3B2A4A"), V85BHairShine = Rgba.Hex("5C4870");
        static readonly Rgba V85BShirt = Rgba.Hex("3FB7A6"), V85BShirtShade = Rgba.Hex("2D8F82");
        static readonly Rgba V85BPants = Rgba.Hex("3E4A7A"), V85BPantsShade = Rgba.Hex("2E3860");
        static readonly Rgba V85BShoe = Rgba.Hex("2E2430"), V85BStar = Rgba.Hex("FFD54A");
        static readonly Rgba V85BGold = Rgba.Hex("E2B04A"), V85BGoldShade = Rgba.Hex("B98630");
        static readonly Rgba V85BCrackInk = Rgba.Hex("FFFFFF", 0.95f), V85BCrackDark = Rgba.Hex("5A7A96", 0.9f);

        static float V85B_Frac(float v) => v - (float)Math.Floor(v);

        /// <summary>
        /// Lo sfondo (coordinate del mondo): carta da parati color lavanda a righe verticali con piccoli rombi sopra
        /// y -1.55, zoccolo bianco, pavimento di legno a doghe orizzontali (più scure verso il basso) e un'ombra morbida
        /// ai bordi.
        /// </summary>
        public static DoodleArt V85B_Bg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            // carta da parati
            p.FillFunc(Sd.Box(0f, 1.9f, 3.3f, 3.45f), (x, y) =>
            {
                float fx = V85B_Frac((x + 10f) / 0.6f);
                Rgba c = fx < 0.5f ? Rgba.Hex("D9C9EE") : Rgba.Hex("CDBBE6");
                // rombi piccoli al centro delle righe chiare
                float cy = V85B_Frac((y + 10f) / 0.6f);
                if (fx < 0.5f && Math.Abs(fx - 0.25f) * 2.4f + Math.Abs(cy - 0.5f) < 0.16f) c = Rgba.Hex("B9A3DA");
                return Rgba.Lerp(c, Rgba.Hex("A893C9"), Math.Max(0f, (Math.Abs(x) - 1.8f) / 1.4f) * 0.5f);
            });
            // zoccolo
            p.Fill(Sd.Box(0f, -1.42f, 3.3f, 0.14f), Rgba.Hex("F4EEE6"));
            p.Fill(Sd.Box(0f, -1.53f, 3.3f, 0.03f), Rgba.Hex("D8CFC4"));
            p.InkLine(0.03f, -3.3f, -1.28f, 3.3f, -1.28f);
            // pavimento a doghe
            p.FillFunc(Sd.Box(0f, -3.4f, 3.3f, 1.84f), (x, y) =>
            {
                float row = (float)Math.Floor((y + 20f) / 0.42f);
                float fy = V85B_Frac((y + 20f) / 0.42f);
                float fx = V85B_Frac((x + 10f + row * 0.37f) / 1.6f);
                Rgba c = Rgba.Lerp(Rgba.Hex("D8A56A"), Rgba.Hex("C48E55"), Noise.Value(row * 2.3f, (float)Math.Floor((x + 10f + row * 0.37f) / 1.6f)));
                c = Rgba.Lerp(c, Rgba.Hex("9E6B3C"), Math.Max(0f, (-1.56f - y) / 3.6f) * 0.45f);
                if (fy < 0.07f || fx < 0.012f) c = Rgba.Lerp(c, Rgba.Hex("7A4F2A"), 0.6f);
                return c;
            });
            p.InkLine(0.035f, -3.3f, -1.56f, 3.3f, -1.56f);
            return new DoodleArt("v85_b_bg", p);
        }

        static Sdf V85B_GlassShape() => Sd.Union(
            Sd.Box(0f, (V85BMirrorBottom + V85BMirrorTop - 0.24f) * 0.5f, V85BGlassHx, (V85BMirrorTop - 0.24f - V85BMirrorBottom) * 0.5f),
            Sd.Ellipse(0f, V85BMirrorTop - 0.24f, V85BGlassHx, 0.24f));

        /// <summary>Il vetro dello specchio (coordinate del mondo, x 0): argento azzurrino con due riflessi obliqui.</summary>
        public static DoodleArt V85B_MirrorGlass()
        {
            var p = new DoodlePainter(-0.4f, V85BMirrorBottom - 0.1f, 0.4f, V85BMirrorTop + 0.1f, 80) { Ink = 0.04f };
            Sdf g = V85B_GlassShape();
            p.FillFunc(g, (x, y) =>
            {
                float t = (y - V85BMirrorBottom) / (V85BMirrorTop - V85BMirrorBottom);
                Rgba c = Rgba.Lerp(Rgba.Hex("9FB8CC"), Rgba.Hex("E3F0FA"), t);
                float band = V85B_Frac((y - x * 1.6f + 10f) / 1.7f);
                if (band < 0.08f || (band > 0.13f && band < 0.16f)) c = Rgba.Lerp(c, Rgba.White, 0.75f);
                return c;
            });
            return new DoodleArt("v85_b_mirror_glass", p);
        }

        /// <summary>La cornice dorata attorno al vetro (coordinate del mondo), con la punta decorata e due piedini.</summary>
        public static DoodleArt V85B_MirrorFrame()
        {
            var p = new DoodlePainter(-0.6f, V85BMirrorBottom - 0.3f, 0.6f, V85BMirrorTop + 0.35f, 80) { Ink = 0.045f };
            Sdf glass = V85B_GlassShape();
            Sdf ring = Sd.Subtract(Sd.Grow(glass, 0.09f), glass);
            Body(p, ring, V85BGold, V85BGoldShade);
            // la punta decorata e i piedini
            Body(p, Sd.Circle(0f, V85BMirrorTop + 0.14f, 0.1f), V85BGold, V85BGoldShade);
            foreach (float s in new[] { -1f, 1f })
                Body(p, Sd.Box(s * 0.26f, V85BMirrorBottom - 0.17f, 0.11f, 0.06f, 0.03f), V85BGold, V85BGoldShade);
            return new DoodleArt("v85_b_mirror_frame", p);
        }

        /// <summary>Le crepe sul vetro (coordinate del mondo): una stella di linee bianche dal punto d'impatto a y -1.0.</summary>
        public static DoodleArt V85B_MirrorCrack()
        {
            var p = new DoodlePainter(-0.3f, -2.8f, 0.3f, 0.8f, 80);
            const float cy = -1.0f;
            float[][] rays =
            {
                new[] { 0f, cy, 0.08f, cy + 0.5f, 0.02f, cy + 1.1f, 0.12f, cy + 1.7f },
                new[] { 0f, cy, -0.1f, cy + 0.4f, -0.16f, cy + 0.9f },
                new[] { 0f, cy, 0.2f, cy + 0.1f },
                new[] { 0f, cy, -0.21f, cy - 0.12f },
                new[] { 0f, cy, 0.1f, cy - 0.6f, 0.03f, cy - 1.2f, 0.15f, cy - 1.7f },
                new[] { 0f, cy, -0.08f, cy - 0.5f, -0.18f, cy - 0.85f },
            };
            foreach (float[] r in rays) { p.Line(V85BCrackDark, 0.035f, r); p.Line(V85BCrackInk, 0.016f, r); }
            p.Shape(Sd.Circle(0f, cy, 0.05f), Rgba.White, 0.015f);
            return new DoodleArt("v85_b_mirror_crack", p);
        }

        /// <summary>
        /// Il personaggio (origine ai piedi, alto circa 2): scarpe, pantaloni, maglia con la spilla a stella sul lato +x,
        /// braccia lungo i fianchi, testa senza faccia (centro <see cref="V85BHeadY"/>) col ciuffo che va verso +x.
        /// Quello a destra dello specchio è lo stesso disegno girato (scala x -1).
        /// </summary>
        public static DoodleArt V85B_Guy()
        {
            var p = new DoodlePainter(-0.55f, -0.06f, 0.6f, 2.12f, 110) { Ink = 0.045f };
            // scarpe e gambe
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Box(s * 0.15f, 0.42f, 0.11f, 0.36f, 0.04f), V85BPants, V85BPantsShade);
                p.Shape(Sd.Ellipse(s * 0.17f + 0.03f, 0.06f, 0.16f, 0.08f), V85BShoe);
            }
            // braccia
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Capsule(s * 0.3f, 1.12f, s * 0.38f, 0.64f, 0.08f), V85BShirt);
                p.Shape(Sd.Circle(s * 0.39f, 0.58f, 0.08f), V85BSkin);
            }
            // maglia
            Body(p, Sd.Box(0f, 0.93f, 0.3f, 0.3f, 0.1f), V85BShirt, V85BShirtShade);
            p.Line(V85BShirtShade, 0.03f, -0.28f, 0.7f, 0.28f, 0.7f);
            // spilla a stella sul lato +x (rivela il riflesso)
            p.Shape(Sd.Polygon(0.15f, 1.12f, 0.175f, 1.06f, 0.24f, 1.055f, 0.19f, 1.015f, 0.21f, 0.95f, 0.15f, 0.985f,
                0.09f, 0.95f, 0.11f, 1.015f, 0.06f, 1.055f, 0.125f, 1.06f), V85BStar, 0.015f);
            // collo, orecchie e testa
            p.Fill(Sd.Box(0f, 1.24f, 0.08f, 0.06f), V85BSkinShade);
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Circle(s * (V85BHeadR + 0.01f), V85BHeadY - 0.02f, 0.07f), V85BSkin);
            Body(p, Sd.Circle(0f, V85BHeadY, V85BHeadR), V85BSkin, V85BSkinShade);
            // capelli con il ciuffo verso +x
            Sdf hair = Sd.Union(
                Sd.Intersect(Sd.Circle(0f, V85BHeadY + 0.03f, V85BHeadR + 0.03f), Sd.Box(0f, V85BHeadY + 0.3f, 0.5f, 0.17f)),
                Sd.Polygon(-0.05f, V85BHeadY + 0.3f, 0.2f, V85BHeadY + 0.5f, 0.42f, V85BHeadY + 0.36f, 0.3f, V85BHeadY + 0.2f));
            Body(p, hair, V85BHair, V85BHairShine, null, 0.05f, -0.05f);
            return new DoodleArt("v85_b_guy", p);
        }

        static DoodlePainter V85B_FacePainter() => new DoodlePainter(-0.3f, V85BHeadY - 0.28f, 0.3f, V85BHeadY + 0.18f, 160) { Ink = 0.03f };

        /// <summary>La faccia normale (coordinate dei piedi): occhi aperti, sopracciglio furbo, sorrisetto.</summary>
        public static DoodleArt V85B_FaceOpen()
        {
            DoodlePainter p = V85B_FacePainter();
            const float ey = V85BHeadY + 0.02f;
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Ellipse(s * 0.12f, ey, 0.04f, 0.055f), DoodlePainter.DefaultInk);
                p.Fill(Sd.Circle(s * 0.12f - 0.012f, ey + 0.018f, 0.014f), Rgba.White);
                p.Fill(Sd.Ellipse(s * 0.2f, ey - 0.1f, 0.05f, 0.03f), Rgba.Hex("FF9AB0", 0.6f));
            }
            p.InkLine(0.022f, -0.17f, ey + 0.1f, -0.07f, ey + 0.12f);
            p.InkLine(0.022f, 0.07f, ey + 0.13f, 0.17f, ey + 0.1f);
            p.InkLine(0.022f, -0.1f, ey - 0.13f, -0.02f, ey - 0.17f, 0.08f, ey - 0.15f, 0.12f, ey - 0.11f);
            return new DoodleArt("v85_b_face_open", p);
        }

        /// <summary>Occhi chiusi (il vero sbatte le palpebre alla fine), sorriso.</summary>
        public static DoodleArt V85B_FaceBlink()
        {
            DoodlePainter p = V85B_FacePainter();
            const float ey = V85BHeadY + 0.02f;
            foreach (float s in new[] { -1f, 1f })
            {
                p.InkLine(0.024f, s * 0.12f - 0.05f, ey, s * 0.12f, ey - 0.025f, s * 0.12f + 0.05f, ey);
                p.Fill(Sd.Ellipse(s * 0.2f, ey - 0.1f, 0.05f, 0.03f), Rgba.Hex("FF9AB0", 0.6f));
            }
            p.InkLine(0.022f, -0.1f, ey - 0.12f, -0.02f, ey - 0.17f, 0.08f, ey - 0.15f, 0.12f, ey - 0.11f);
            return new DoodleArt("v85_b_face_blink", p);
        }

        /// <summary>Felice: occhi a mezzaluna e bocca aperta col sorriso grande.</summary>
        public static DoodleArt V85B_FaceSmile()
        {
            DoodlePainter p = V85B_FacePainter();
            const float ey = V85BHeadY + 0.02f;
            foreach (float s in new[] { -1f, 1f })
            {
                p.InkLine(0.026f, s * 0.12f - 0.05f, ey - 0.01f, s * 0.12f, ey + 0.035f, s * 0.12f + 0.05f, ey - 0.01f);
                p.Fill(Sd.Ellipse(s * 0.21f, ey - 0.09f, 0.055f, 0.035f), Rgba.Hex("FF8FA8", 0.75f));
            }
            Sdf mouth = Sd.Intersect(Sd.Circle(0f, ey - 0.07f, 0.13f), Sd.Box(0f, ey - 0.16f, 0.14f, 0.09f));
            p.Shape(mouth, Rgba.Hex("8A2E3E"), 0.022f);
            p.Fill(Sd.Intersect(mouth, Sd.Circle(0f, ey - 0.23f, 0.07f)), Rgba.Hex("FF7F95"));
            return new DoodleArt("v85_b_face_smile", p);
        }

        /// <summary>Preoccupata (il falso che non riesce più a copiare): occhi sgranati, bocca a zig-zag.</summary>
        public static DoodleArt V85B_FaceWorry()
        {
            DoodlePainter p = V85B_FacePainter();
            const float ey = V85BHeadY + 0.02f;
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Circle(s * 0.12f, ey, 0.065f), Rgba.White, 0.018f);
                p.Fill(Sd.Circle(s * 0.12f, ey - 0.005f, 0.026f), DoodlePainter.DefaultInk);
                p.InkLine(0.02f, s * 0.07f, ey + 0.1f, s * 0.17f, ey + 0.13f);
            }
            p.InkLine(0.02f, -0.1f, ey - 0.15f, -0.05f, ey - 0.12f, 0f, ey - 0.15f, 0.05f, ey - 0.12f, 0.1f, ey - 0.15f);
            return new DoodleArt("v85_b_face_worry", p);
        }

        /// <summary>Le crepe sul personaggio falso (coordinate dei piedi): linee bianche che attraversano testa e corpo.</summary>
        public static DoodleArt V85B_GuyCrack()
        {
            var p = new DoodlePainter(-0.5f, 0.1f, 0.5f, 2.0f, 100);
            float[][] lines =
            {
                new[] { 0.05f, 1.15f, -0.08f, 1.45f, 0.06f, 1.7f, -0.04f, 1.92f },
                new[] { 0.05f, 1.15f, 0.28f, 1.3f, 0.36f, 1.55f },
                new[] { 0.05f, 1.15f, -0.25f, 1.0f, -0.4f, 1.1f },
                new[] { 0.05f, 1.15f, 0.12f, 0.8f, -0.05f, 0.5f, 0.08f, 0.2f },
                new[] { 0.05f, 1.15f, 0.32f, 0.85f },
                new[] { -0.08f, 1.45f, -0.26f, 1.62f },
            };
            foreach (float[] l in lines) { p.Line(V85BCrackDark, 0.04f, l); p.Line(V85BCrackInk, 0.018f, l); }
            return new DoodleArt("v85_b_guy_crack", p);
        }

        /// <summary>Una scheggia di vetro (centrata nell'origine, lunga circa 0.3) con il filo bianco.</summary>
        public static DoodleArt V85B_Shard()
        {
            var p = new DoodlePainter(-0.2f, -0.2f, 0.2f, 0.2f, 120) { Ink = 0.02f };
            Sdf s = Sd.Polygon(-0.13f, -0.1f, 0.15f, -0.04f, -0.02f, 0.15f);
            p.Shape(s, Rgba.Hex("CFE9FA", 0.9f));
            p.Line(Rgba.White, 0.02f, -0.07f, -0.04f, -0.01f, 0.08f);
            return new DoodleArt("v85_b_shard", p);
        }
    }
}
