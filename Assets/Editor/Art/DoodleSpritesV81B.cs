using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.81 per il livello 38 della roadmap "The Impossible Hug" (lo sfondo è il parco "park_bg", i
    /// cuoricini che volano sono "heart", il bagliore del cuore riunito è "glow", i fumetti "bubble_box"): i due amici
    /// tondi (Bo, blu col ciuffo, e Pip, rosa col fiocco: solo il corpo coi piedini), le braccia conserte e le braccia
    /// dell'abbraccio di ciascuno (a parte: si scambiano), tre facce comuni ai due (arrabbiata che guarda altrove col
    /// rossore, timida che guarda l'altro, felice a occhi chiusi), le due metà del cuore spezzato (stessa origine: messe
    /// nello stesso punto combaciano) e il segno di rabbia "a vena". Tutti disegnati rivolti a DESTRA (verso l'amico): il
    /// livello rovescia in x quello di destra. Nessuna scritta disegnata. Prefisso v81_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV81B() => new DoodleArt[]
        {
            V81B_Body("v81_b_body_blue", V81BBlue, V81BBlueShade, false), V81B_Body("v81_b_body_pink", V81BPink, V81BPinkShade, true),
            V81B_ArmsCrossed("v81_b_arms_cross_blue", V81BBlue, V81BBlueShade), V81B_ArmsCrossed("v81_b_arms_cross_pink", V81BPink, V81BPinkShade),
            V81B_ArmsHug("v81_b_arms_hug_blue", V81BBlue, V81BBlueShade), V81B_ArmsHug("v81_b_arms_hug_pink", V81BPink, V81BPinkShade),
            V81B_FaceAngry(), V81B_FaceShy(), V81B_FaceHug(),
            V81B_HeartHalf("v81_b_heart_left", true), V81B_HeartHalf("v81_b_heart_right", false),
            V81B_Anger(),
        };

        // misure condivise col builder
        /// <summary>Raggio del corpo tondo degli amici (origine al centro del corpo).</summary>
        public const float V81BBodyR = 0.55f;

        static readonly Rgba V81BBlue = Rgba.Hex("62B9F2"), V81BBlueShade = Rgba.Hex("3B8DCB");
        static readonly Rgba V81BPink = Rgba.Hex("FF93BE"), V81BPinkShade = Rgba.Hex("E2608F");
        static readonly Rgba V81BEye = Rgba.Hex("2B2133"), V81BAngryRed = Rgba.Hex("FF2A2A"), V81BBlush = Rgba.Hex("FF7F9C");
        static readonly Rgba V81BHeart = Rgba.Hex("E8333F"), V81BHeartShade = Rgba.Hex("B21E2C");

        /// <summary>
        /// Il corpo tondo (raggio <see cref="V81BBodyR"/>, origine al centro) con due piedini sotto; Bo ha il ciuffo, Pip il
        /// fiocco. Faccia e braccia sono a parte.
        /// </summary>
        public static DoodleArt V81B_Body(string name, Rgba fill, Rgba shade, bool bow)
        {
            var p = new DoodlePainter(-0.66f, -0.74f, 0.66f, 0.8f, 150) { Ink = 0.045f };
            p.Fill(Sd.Ellipse(0f, -0.7f, 0.5f, 0.06f), new Rgba(0f, 0f, 0f, 0.2f));   // ombra a terra
            foreach (float s in new[] { -1f, 1f })
                Body(p, Sd.Ellipse(s * 0.24f, -0.58f, 0.17f, 0.1f), shade, shade);   // piedini
            Body(p, Sd.Circle(0f, 0f, V81BBodyR), fill, shade, null, -0.07f, 0.08f);
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.27f, 0.3f, 0.1f, 0.06f), -0.27f, 0.3f, 40f), Rgba.White.WithAlpha(0.7f));
            if (bow)
            {
                // fiocco giallo in cima, un po' di lato
                Rgba ribbon = Rgba.Hex("FFD25A"), ribbonShade = Rgba.Hex("E0A92A");
                Body(p, Sd.Polygon(0.1f, 0.56f, -0.16f, 0.72f, -0.18f, 0.44f), ribbon, ribbonShade);
                Body(p, Sd.Polygon(0.1f, 0.56f, 0.36f, 0.72f, 0.38f, 0.44f), ribbon, ribbonShade);
                p.Shape(Sd.Circle(0.1f, 0.57f, 0.06f), ribbon, 0.03f);
            }
            else
            {
                // ciuffo di tre ricciolini
                p.Line(V81BEye, 0.035f, -0.04f, 0.53f, -0.1f, 0.66f, -0.02f, 0.74f);
                p.Line(V81BEye, 0.035f, 0.04f, 0.54f, 0.06f, 0.7f, 0.16f, 0.76f);
                p.Line(V81BEye, 0.035f, 0.12f, 0.52f, 0.22f, 0.62f, 0.3f, 0.6f);
            }
            return new DoodleArt(name, p);
        }

        /// <summary>Le braccia conserte davanti alla pancia (origine al centro del corpo): due salsicciotti incrociati con le manine.</summary>
        public static DoodleArt V81B_ArmsCrossed(string name, Rgba fill, Rgba shade)
        {
            var p = new DoodlePainter(-0.56f, -0.46f, 0.56f, 0.02f, 150) { Ink = 0.035f };
            Body(p, Sd.Union(Sd.Capsule(-0.46f, -0.12f, 0.2f, -0.3f, 0.095f), Sd.Circle(0.24f, -0.3f, 0.1f)), fill, shade);
            Body(p, Sd.Union(Sd.Capsule(0.46f, -0.12f, -0.2f, -0.3f, 0.095f), Sd.Circle(-0.24f, -0.3f, 0.1f)), fill, shade);
            return new DoodleArt(name, p);
        }

        /// <summary>Le braccia aperte dell'abbraccio, tese verso destra (origine al centro del corpo): due braccia corte e basse con le manine.</summary>
        public static DoodleArt V81B_ArmsHug(string name, Rgba fill, Rgba shade)
        {
            // basse e corte: nell'abbraccio passano sotto la faccia dell'altro (non la coprono)
            var p = new DoodlePainter(-0.1f, -0.52f, 0.92f, 0.1f, 150) { Ink = 0.035f };
            Body(p, Sd.Union(Sd.Capsule(0.34f, -0.1f, 0.6f, -0.08f, 0.09f), Sd.Circle(0.64f, -0.07f, 0.1f)), fill, shade);
            Body(p, Sd.Union(Sd.Capsule(0.3f, -0.32f, 0.76f, -0.36f, 0.09f), Sd.Circle(0.8f, -0.34f, 0.11f)), fill, shade);
            return new DoodleArt(name, p);
        }

        /// <summary>
        /// Faccia arrabbiata (origine al centro del corpo): rossore rosso sulla fronte, sopracciglia a "V", occhi che
        /// guardano a SINISTRA (altrove, lontano dall'amico), bocca imbronciata.
        /// </summary>
        public static DoodleArt V81B_FaceAngry()
        {
            var p = new DoodlePainter(-0.5f, -0.2f, 0.5f, 0.5f, 150) { Ink = 0f, Wobble = 0.004f };
            p.FillFunc(Sd.Ellipse(0f, 0.2f, 0.44f, 0.26f), (x, y) => V81BAngryRed.WithAlpha(0.42f * Math.Max(0f, Math.Min(1f, (y + 0.05f) / 0.3f))));
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Ellipse(s * 0.19f, 0.15f, 0.09f, 0.075f), Rgba.White);
                p.Fill(Sd.Circle(s * 0.19f - 0.045f, 0.14f, 0.045f), V81BEye);   // pupille a sinistra
                p.Line(V81BEye, 0.045f, s * 0.07f, 0.24f, s * 0.31f, 0.33f);       // sopracciglia a V (basse al centro)
            }
            var mouth = new List<float>();
            for (int i = 0; i <= 8; i++)
            {
                double a = Math.PI * (0.25 + 0.5 * i / 8.0);
                mouth.Add(-0.03f + 0.13f * (float)Math.Cos(a));
                mouth.Add(-0.1f + 0.06f * (float)Math.Sin(a));
            }
            p.Line(V81BEye, 0.04f, mouth.ToArray());
            return new DoodleArt("v81_b_face_angry", p);
        }

        /// <summary>Faccia timida (origine al centro del corpo): occhi che guardano a DESTRA (l'amico), guance rosa, boccuccia ondulata.</summary>
        public static DoodleArt V81B_FaceShy()
        {
            var p = new DoodlePainter(-0.5f, -0.2f, 0.5f, 0.5f, 150) { Ink = 0f, Wobble = 0.004f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Ellipse(s * 0.19f, 0.15f, 0.085f, 0.095f), Rgba.White);
                p.Fill(Sd.Circle(s * 0.19f + 0.04f, 0.14f, 0.045f), V81BEye);
                p.Line(V81BEye, 0.035f, s * 0.11f, 0.3f, s * 0.27f, 0.33f);   // sopracciglia alzate
                p.Fill(Sd.Ellipse(s * 0.33f, 0.0f, 0.09f, 0.055f), V81BBlush.WithAlpha(0.8f));
            }
            p.Line(V81BEye, 0.032f, -0.1f, -0.06f, -0.05f, -0.03f, 0f, -0.07f, 0.05f, -0.03f, 0.1f, -0.06f);
            return new DoodleArt("v81_b_face_shy", p);
        }

        /// <summary>Faccia dell'abbraccio (origine al centro del corpo): occhi chiusi felici, guance rosa, sorrisone.</summary>
        public static DoodleArt V81B_FaceHug()
        {
            var p = new DoodlePainter(-0.5f, -0.24f, 0.5f, 0.5f, 150) { Ink = 0f, Wobble = 0.004f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Line(V81BEye, 0.045f, s * 0.28f, 0.12f, s * 0.19f, 0.2f, s * 0.1f, 0.12f);   // occhi chiusi "^"
                p.Fill(Sd.Ellipse(s * 0.33f, 0.0f, 0.1f, 0.06f), V81BBlush.WithAlpha(0.85f));
            }
            Sdf smile = Sd.Intersect(Sd.Ellipse(0f, -0.02f, 0.19f, 0.17f), Sd.Box(0f, -0.12f, 0.22f, 0.1f));
            p.Shape(smile, Rgba.Hex("5A1F3A"), 0.03f);
            p.Fill(Sd.Intersect(smile, Sd.Ellipse(0f, -0.18f, 0.1f, 0.05f)), Rgba.Hex("FF7A8A"));
            return new DoodleArt("v81_b_face_hug", p);
        }

        /// <summary>
        /// Una metà del cuore spezzato (origine al centro del cuore intero, 1.2 x 1.1): la crepa a zig-zag è la stessa per le
        /// due metà, quindi messe nello stesso punto il cuore torna intero.
        /// </summary>
        public static DoodleArt V81B_HeartHalf(string name, bool left)
        {
            var p = new DoodlePainter(-0.66f, -0.68f, 0.66f, 0.52f, 140) { Ink = 0.04f };
            Sdf heart = Sd.Union(Sd.Circle(-0.27f, 0.13f, 0.31f), Sd.Circle(0.27f, 0.13f, 0.31f),
                Sd.Polygon(-0.56f, 0.03f, 0.56f, 0.03f, 0f, -0.62f));
            float[] crack = { 0f, 0.8f, 0f, 0.27f, -0.09f, 0.12f, 0.07f, -0.04f, -0.06f, -0.2f, 0.05f, -0.36f, 0f, -0.8f };
            var poly = new List<float>(crack);
            float side = left ? -1f : 1f;
            poly.Add(side * 1f); poly.Add(-0.8f);
            poly.Add(side * 1f); poly.Add(0.8f);
            Sdf half = Sd.Intersect(heart, Sd.Polygon(poly.ToArray()));
            Body(p, half, V81BHeart, V81BHeartShade, null, -0.05f, 0.06f);
            if (left) p.Fill(Sd.Rotate(Sd.Ellipse(-0.3f, 0.22f, 0.1f, 0.06f), -0.3f, 0.22f, 35f), Rgba.White.WithAlpha(0.75f));
            return new DoodleArt(name, p);
        }

        /// <summary>Il segno di rabbia "a vena" (origine al centro, circa 0.36): quattro archetti rossi.</summary>
        public static DoodleArt V81B_Anger()
        {
            var p = new DoodlePainter(-0.22f, -0.22f, 0.22f, 0.22f, 160) { Ink = 0f, Wobble = 0.003f };
            foreach (float sx in new[] { -1f, 1f })
                foreach (float sy in new[] { -1f, 1f })
                    p.Line(V81BAngryRed, 0.05f, sx * 0.04f, sy * 0.17f, sx * 0.07f, sy * 0.07f, sx * 0.17f, sy * 0.04f);
            return new DoodleArt("v81_b_anger", p);
        }
    }
}
