using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.76 per il livello "The Shy Button" (roadmap n. 2): lo sfondo semplice (parete pastello a
    /// pois con qualche ditata sbiadita), il pulsantone verde START (cupola e basamento, ombra compresa, la scritta è un
    /// testo a parte) e quattro facce da sovrapporre alla cupola (nervosa, nel panico, bloccata, occhi spalancati).
    /// La goccia di sudore è la solita "sweat", lo sbuffo della fuga è "puff". Prefisso v76_a_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV76A() => new DoodleArt[]
        {
            V76A_Bg(), V76A_Button(), V76A_Face(0), V76A_Face(1), V76A_Face(2), V76A_Face(3),
        };

        static readonly Rgba V76AGreen = Rgba.Hex("5BCF5A"), V76AGreenShade = Rgba.Hex("3E9A35"), V76AEyeWhite = Rgba.Hex("FFFFFF");

        /// <summary>Altezza (coordinate locali del pulsante) del centro della scritta START sul basamento.</summary>
        public const float V76ALabelY = -0.5f;
        /// <summary>Punto (locale) dove compare la goccia di sudore, sul fianco destro della cupola.</summary>
        public const float V76ASweatX = 0.8f, V76ASweatY = 0.36f;

        /// <summary>
        /// Lo sfondo: parete pastello (azzurro in alto, rosa in basso) a pois tenui e qualche ditata sbiadita (indizio
        /// scherzoso: qui si tocca parecchio). Niente pavimento: il pulsante scappa dappertutto. Coordinate del mondo.
        /// </summary>
        public static DoodleArt V76A_Bg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 40) { Ink = 0.05f, Wobble = 0.006f };
            Sdf all = Sd.Box(0f, 0f, 4.6f, 7.1f);
            p.FillFunc(all, (x, y) =>
            {
                float t = Math.Max(0f, Math.Min(1f, (3.5f - y) / 8f));
                Rgba c = Rgba.Lerp(Rgba.Hex("CDEBF6"), Rgba.Hex("F7E3F1"), t);
                // vignetta leggerissima ai bordi
                float v = Math.Max(0f, Math.Min(1f, (Math.Abs(x) - 2.2f) / 2.3f));
                return Rgba.Lerp(c, Rgba.Hex("B9D3E6"), v * 0.35f);
            });
            for (int r = 0; r < 24; r++)
                for (int c = 0; c < 12; c++)
                {
                    float x = -4.2f + c * 0.8f + (r % 2) * 0.4f, y = -6.8f + r * 0.6f;
                    p.Fill(Sd.Circle(x, y, 0.06f), Rgba.White.WithAlpha(0.55f), false);
                }
            // ditate sbiadite (spirali di linee concentriche, inclinate)
            float[][] prints = { new[] { -1.9f, 2.2f, 20f }, new[] { 1.85f, -1.2f, -25f }, new[] { -1.6f, -3.9f, 40f }, new[] { 1.5f, 3.6f, -10f } };
            foreach (float[] f in prints)
            {
                for (int k = 0; k < 5; k++)
                {
                    float rx = 0.1f + k * 0.055f, ry = 0.14f + k * 0.07f;
                    Sdf ring = Sd.Subtract(Sd.Ellipse(f[0], f[1], rx, ry), Sd.Ellipse(f[0], f[1], rx - 0.018f, ry - 0.018f));
                    p.Fill(Sd.Rotate(ring, f[0], f[1], f[2]), Rgba.Hex("9FB4C8", 0.28f), false);
                }
            }
            return new DoodleArt("v76_a_bg", p);
        }

        /// <summary>
        /// Il pulsantone START (centro 0,0, largo 2.3): basamento grigio scuro (y -0.7..-0.3, la scritta START ci va
        /// sopra in y <see cref="V76ALabelY"/>), cupola verde lucida fino a y 0.72, ombra morbida sotto (y -0.8).
        /// </summary>
        public static DoodleArt V76A_Button()
        {
            var p = new DoodlePainter(-1.22f, -0.95f, 1.22f, 0.82f, 120) { Ink = 0.045f };
            p.Fill(Sd.Ellipse(0f, -0.78f, 1.12f, 0.13f), Rgba.Hex("2B2233", 0.22f), false);
            Sdf baseS = Sd.Box(0f, -0.5f, 1.08f, 0.21f, 0.1f);
            Body(p, baseS, Rgba.Hex("5E6475"), Rgba.Hex("444A5A"));
            Sdf dome = Sd.Intersect(Sd.Ellipse(0f, -0.3f, 0.92f, 1.02f), Sd.Box(0f, 0.25f, 1f, 0.58f));
            Body(p, dome, V76AGreen, V76AGreenShade);
            // collarino scuro tra cupola e basamento
            p.Shape(Sd.Box(0f, -0.3f, 0.95f, 0.035f, 0.03f), Rgba.Hex("2F7A2A"), 0.03f);
            // riflessi
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.45f, 0.42f, 0.17f, 0.07f), -0.45f, 0.42f, 35f), Rgba.White.WithAlpha(0.65f));
            p.Fill(Sd.Circle(-0.22f, 0.6f, 0.04f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("v76_a_button", p);
        }

        /// <summary>
        /// Faccia sulla cupola (coordinate locali del pulsante, occhi in ±0.3, 0.3): 0 nervosa (sopracciglia
        /// preoccupate, bocca tremolante), 1 nel panico (pupille piccole, sopracciglia alte, bocca a zigzag), 2 bloccata
        /// (occhi in giù verso il dito, denti stretti), 3 occhi spalancati (la vittoria, bocca a "O").
        /// </summary>
        public static DoodleArt V76A_Face(int mood)
        {
            var p = new DoodlePainter(-0.6f, -0.06f, 0.6f, 0.72f, 140) { Ink = 0.035f };
            float eyeY = 0.3f;
            float rx = mood == 3 ? 0.19f : mood == 1 ? 0.16f : 0.14f;
            float ry = mood == 3 ? 0.23f : mood == 1 ? 0.19f : 0.17f;
            foreach (float s in new[] { -1f, 1f })
            {
                float ex = s * 0.3f;
                p.Shape(Sd.Ellipse(ex, eyeY, rx, ry), V76AEyeWhite);
                // pupille: guardano di lato (0), minuscole e tremanti (1), in giù verso il dito (2), al centro (3)
                float px = mood == 0 ? ex + s * 0.03f - 0.04f : ex;
                float py = mood == 2 ? eyeY - 0.08f : mood == 0 ? eyeY - 0.03f : eyeY;
                float pr = mood == 1 ? 0.04f : mood == 3 ? 0.075f : 0.065f;
                p.Fill(Sd.Circle(px, py, pr), Rgba.Hex("2B2233"));
                p.Fill(Sd.Circle(px - pr * 0.35f, py + pr * 0.4f, pr * 0.35f), Rgba.White);
                if (mood == 3) p.Fill(Sd.Circle(px + 0.05f, py - 0.05f, 0.018f), Rgba.White);
                // sopracciglia: interno più alto = preoccupazione
                float by = eyeY + ry + (mood == 3 ? 0.07f : 0.05f);
                float inner = mood == 0 ? 0.08f : mood == 1 ? 0.13f : mood == 2 ? 0.11f : 0.03f;
                p.InkLine(0.035f, s * 0.42f, by, s * 0.14f, by + inner);
            }
            switch (mood)
            {
                case 0:
                    // bocchina tremolante
                    p.InkLine(0.03f, -0.13f, 0.06f, -0.07f, 0.09f, -0.02f, 0.05f, 0.03f, 0.09f, 0.08f, 0.05f, 0.13f, 0.08f);
                    break;
                case 1:
                    // bocca a zigzag, più larga
                    p.Shape(Sd.Box(0f, 0.06f, 0.17f, 0.05f, 0.04f), Rgba.Hex("5A1E2A"), 0.03f);
                    p.Line(Rgba.White, 0.022f, -0.14f, 0.04f, -0.09f, 0.08f, -0.04f, 0.04f, 0.01f, 0.08f, 0.06f, 0.04f, 0.11f, 0.08f, 0.15f, 0.05f);
                    break;
                case 2:
                    // denti stretti
                    Sdf teeth = Sd.Box(0f, 0.05f, 0.18f, 0.055f, 0.03f);
                    p.Shape(teeth, Rgba.White, 0.03f);
                    p.InkLine(0.016f, -0.17f, 0.05f, 0.17f, 0.05f);
                    foreach (float x in new[] { -0.09f, 0f, 0.09f }) p.InkLine(0.014f, x, 0.0f, x, 0.1f);
                    break;
                default:
                    // "O" di stupore
                    p.Shape(Sd.Ellipse(0f, 0.04f, 0.08f, 0.1f), Rgba.Hex("5A1E2A"), 0.03f);
                    p.Fill(Sd.Ellipse(0f, 0.0f, 0.045f, 0.035f), Rgba.Hex("F27A8A"));
                    break;
            }
            string[] names = { "v76_a_face_nervous", "v76_a_face_panic", "v76_a_face_pinned", "v76_a_face_wide" };
            return new DoodleArt(names[Math.Max(0, Math.Min(3, mood))], p);
        }
    }
}
