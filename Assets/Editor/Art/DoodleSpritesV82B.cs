using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.82 per il livello "The Stupid Genius 2" (roadmap n. 44): lo sfondo (aula con la lavagna
    /// grande murata, pavimento di legno e la console dei quiz su cui stanno i pulsanti), il genio arrogante a pezzi
    /// (corpo col camice e le mani sui fianchi, testone, capelli in ordine e capelli dritti, occhialoni, quattro facce:
    /// sufficienza, urlo, panico, stupore), il pulsante-risposta (tasto giallo, il numero è un testo del livello) e un
    /// frammento di gesso. L'equazione è scritta con dei testi (uno per simbolo): solo il 5 si trascina.
    /// Lo sbuffo è "puff", il bagliore "glow", la scintilla "sparkle_star". Prefisso v82_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV82B() => new DoodleArt[]
        {
            V82B_Bg(), V82B_Body(), V82B_Head(), V82B_Hair(false), V82B_Hair(true), V82B_Glasses(),
            V82B_Face(0), V82B_Face(1), V82B_Face(2), V82B_Face(3), V82B_Key(), V82B_ChalkBit(),
        };

        /// <summary>Centro della lavagna nel mondo e metà della parte verde (l'equazione la scrive il livello).</summary>
        public const float V82BBoardY = 1.55f, V82BBoardHalfW = 2.25f, V82BBoardHalfH = 1.22f;
        /// <summary>Altezza del collo sul corpo (origine della testa) e del centro degli occhi sulla testa.</summary>
        public const float V82BNeckY = 1.45f, V82BEyeY = 0.66f;

        static readonly Rgba V82BCoat = Rgba.Hex("F7F9FD"), V82BCoatShade = Rgba.Hex("CDD5E3");
        static readonly Rgba V82BHair = Rgba.Hex("5B3A29"), V82BHairShade = Rgba.Hex("3E2619");
        static readonly Rgba V82BChalk = Rgba.Hex("F2F2EA", 0.85f);

        static float V82B_Clamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        /// <summary>
        /// Lo sfondo (coordinate del mondo): parete a strisce color crema, la lavagna grande murata in alto (centro
        /// (0, <see cref="V82BBoardY"/>), parte verde ±<see cref="V82BBoardHalfW"/> × ±<see cref="V82BBoardHalfH"/>) con qualche
        /// scarabocchio di gesso cancellato negli angoli e la mensolina dei gessetti; pavimento di legno da y -2.65; la
        /// console blu dei quiz (centro (0, -3.55)) con le lucine, su cui il livello posa i tre pulsanti.
        /// </summary>
        public static DoodleArt V82B_Bg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            // parete
            p.FillFunc(Sd.Box(0f, 1.3f, 3.3f, 4.0f), (x, y) =>
            {
                Rgba c = (int)Math.Floor((x + 10f) * 2.2f) % 2 == 0 ? Rgba.Hex("EFE6F7") : Rgba.Hex("E6DBF1");
                return Rgba.Lerp(c, Rgba.Hex("D8CCE8"), V82B_Clamp((Math.Abs(x) - 2.2f) / 1.2f) * 0.6f);
            });
            // battiscopa e pavimento di legno
            p.FillFunc(Sd.Box(0f, -3.95f, 3.3f, 1.3f), (x, y) => (int)Math.Floor((y + 10f) * 3f) % 2 == 0 ? Wood : WoodShade);
            p.Shape(Sd.Box(0f, -2.6f, 3.3f, 0.07f), Rgba.Hex("8E6BB0"), 0.04f);
            // lavagna murata
            float by = V82BBoardY, hw = V82BBoardHalfW, hh = V82BBoardHalfH;
            Body(p, Sd.Box(0f, by, hw + 0.17f, hh + 0.17f, 0.06f), Rgba.Hex("A0663A"), Rgba.Hex("7A4A26"));
            Sdf board = Sd.Box(0f, by, hw, hh, 0.03f);
            p.FillFunc(board, (x, y) => Rgba.Lerp(Rgba.Hex("2F5A46"), Rgba.Hex("244636"), V82B_Clamp(Noise.Value(x * 2.5f, y * 2.5f))));
            p.Hatch(board, Rgba.Hex("FFFFFF", 0.045f), 0.17f, 0.07f, 20f);   // aloni di gesso cancellato
            // scarabocchi del genio negli angoli (formule illeggibili)
            Rgba faint = Rgba.Hex("F2F2EA", 0.28f);
            p.Line(faint, 0.025f, -2.0f, 2.55f, -1.85f, 2.65f, -1.7f, 2.5f, -1.55f, 2.62f);
            p.Line(faint, 0.025f, -1.45f, 2.58f, -1.25f, 2.58f);
            p.Line(faint, 0.025f, -1.35f, 2.5f, -1.35f, 2.66f);
            p.Line(faint, 0.025f, 1.3f, 2.6f, 1.5f, 2.5f, 1.65f, 2.66f, 1.85f, 2.52f, 2.0f, 2.62f);
            p.Line(faint, 0.02f, -2.0f, 0.52f, -1.6f, 0.52f);
            p.Line(faint, 0.02f, -1.95f, 0.45f, -1.7f, 0.62f);
            p.Line(faint, 0.025f, 1.45f, 0.5f, 1.6f, 0.62f, 1.75f, 0.48f, 1.9f, 0.6f, 2.02f, 0.5f);
            p.Shape(board, Rgba.Clear, 0.03f);
            // mensolina con i gessetti
            p.Shape(Sd.Box(0f, by - hh - 0.22f, 1.9f, 0.06f, 0.02f), Rgba.Hex("7A4A26"), 0.03f);
            p.Shape(Sd.Box(-1.25f, by - hh - 0.13f, 0.14f, 0.035f, 0.02f), Rgba.White, 0.018f);
            p.Shape(Sd.Box(1.4f, by - hh - 0.13f, 0.1f, 0.035f, 0.02f), Rgba.Hex("FFD3E6"), 0.018f);
            // console dei quiz
            Body(p, Sd.Box(0f, -3.55f, 2.5f, 0.78f, 0.18f), Rgba.Hex("3C4C86"), Rgba.Hex("2B376A"));
            p.Shape(Sd.Box(0f, -2.88f, 2.3f, 0.06f, 0.03f), Rgba.Hex("6A7BC0"), 0.03f);
            for (int i = 0; i < 9; i++)
            {
                float lx = -2.0f + i * 0.5f;
                p.Shape(Sd.Circle(lx, -4.18f, 0.05f), i % 3 == 0 ? Rgba.Hex("FFD45C") : i % 3 == 1 ? Rgba.Hex("FF7A8A") : Rgba.Hex("7FE0C0"), 0.02f);
            }
            return new DoodleArt("v82_b_bg", p);
        }

        /// <summary>
        /// Il corpo del genio (origine fra i piedi, a terra; collo in (0, <see cref="V82BNeckY"/>)): scarpe, pantaloni,
        /// camice bianco con le penne nel taschino e il farfallino, mani sui fianchi (aria di sufficienza).
        /// </summary>
        public static DoodleArt V82B_Body()
        {
            var p = new DoodlePainter(-0.85f, -0.08f, 0.85f, 1.56f, 110) { Ink = 0.045f };
            // scarpe e gambe
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Ellipse(s * 0.2f + s * 0.04f, 0.07f, 0.2f, 0.08f), Rgba.Hex("3A2E2A"));
                p.Shape(Sd.Box(s * 0.16f, 0.32f, 0.09f, 0.22f), Rgba.Hex("4A4E6E"));
            }
            // braccia ai fianchi (dietro il camice: gomiti in fuori)
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Polyline(0.1f, s * 0.34f, 1.33f, s * 0.68f, 1.0f, s * 0.42f, 0.84f), V82BCoat);
                p.Line(V82BCoatShade, 0.04f, s * 0.6f, 1.02f, s * 0.48f, 0.9f);
            }
            // camice
            Sdf coat = Sd.Polygon(-0.5f, 0.42f, 0.5f, 0.42f, 0.38f, 1.42f, -0.38f, 1.42f);
            Body(p, coat, V82BCoat, V82BCoatShade);
            // camicia e farfallino
            p.Shape(Sd.Polygon(-0.14f, 1.42f, 0.14f, 1.42f, 0f, 1.1f), Rgba.Hex("BFE3FF"), 0.025f);
            p.Shape(Sd.Union(Sd.Polygon(0f, 1.34f, -0.14f, 1.42f, -0.14f, 1.26f), Sd.Polygon(0f, 1.34f, 0.14f, 1.42f, 0.14f, 1.26f)), Rgba.Hex("E8433F"), 0.022f);
            p.Shape(Sd.Circle(0f, 1.34f, 0.035f), Rgba.Hex("B8302C"), 0.018f);
            // risvolti e bottoni
            p.InkLine(0.025f, -0.14f, 1.42f, -0.02f, 0.95f, 0f, 0.44f);
            p.InkLine(0.025f, 0.14f, 1.42f, 0.02f, 0.95f);
            foreach (float y in new[] { 0.85f, 0.65f }) p.Shape(Sd.Circle(0.07f, y, 0.025f), Rgba.Hex("D3DAE6"), 0.015f);
            // taschino con le penne
            p.Line(Rgba.Hex("E8433F"), 0.03f, 0.2f, 1.08f, 0.2f, 1.25f);
            p.Line(Rgba.Hex("3E8EF7"), 0.03f, 0.27f, 1.08f, 0.28f, 1.22f);
            p.Shape(Sd.Box(0.24f, 1.05f, 0.1f, 0.07f, 0.02f), V82BCoat, 0.022f);
            // mani sui fianchi
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Circle(s * 0.44f, 0.84f, 0.085f), Skin);
            return new DoodleArt("v82_b_body", p);
        }

        /// <summary>Il testone (origine al collo; centro della testa (0, 0.62), largo 1.2 con le orecchie).</summary>
        public static DoodleArt V82B_Head()
        {
            var p = new DoodlePainter(-0.76f, -0.06f, 0.76f, 1.24f, 110) { Ink = 0.045f };
            p.Shape(Sd.Box(0f, 0.04f, 0.13f, 0.1f), SkinShade);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Circle(s * 0.6f, 0.56f, 0.12f), Skin);
                p.Line(SkinShade, 0.025f, s * 0.6f, 0.62f, s * 0.64f, 0.52f);
            }
            Body(p, Sd.Ellipse(0f, 0.62f, 0.6f, 0.58f), Skin, SkinShade, null, -0.07f, 0.06f);
            // fronte lucida (il cervellone)
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.2f, 0.98f, 0.18f, 0.07f), -0.2f, 0.98f, 18f), Rgba.White.WithAlpha(0.55f));
            return new DoodleArt("v82_b_head", p);
        }

        /// <summary>
        /// I capelli (stessa origine della testa): in ordine, un ciuffo laccato all'indietro con le basette; oppure dritti
        /// come dopo una scossa (punte a raggiera).
        /// </summary>
        public static DoodleArt V82B_Hair(bool up)
        {
            if (!up)
            {
                var p = new DoodlePainter(-0.72f, 0.6f, 0.72f, 1.42f, 110) { Ink = 0.045f };
                Sdf top = Sd.Subtract(
                    Sd.Union(Sd.Ellipse(0f, 1.1f, 0.58f, 0.24f), Sd.Ellipse(0.22f, 1.2f, 0.36f, 0.18f), Sd.Circle(-0.42f, 1.02f, 0.18f)),
                    Sd.Ellipse(0f, 0.86f, 0.5f, 0.2f));
                Sdf burns = Sd.Union(Sd.Box(-0.55f, 0.82f, 0.06f, 0.14f, 0.04f), Sd.Box(0.55f, 0.82f, 0.06f, 0.14f, 0.04f));
                Body(p, Sd.Union(top, burns), V82BHair, V82BHairShade, null, 0.05f, 0.06f);
                p.Line(V82BHairShade, 0.025f, -0.3f, 1.12f, 0.05f, 1.26f, 0.38f, 1.24f);
                p.Line(V82BHairShade, 0.025f, -0.1f, 1.04f, 0.2f, 1.14f, 0.45f, 1.1f);
                p.Fill(Sd.Ellipse(0.1f, 1.27f, 0.16f, 0.04f), Rgba.White.WithAlpha(0.35f));
                return new DoodleArt("v82_b_hair", p);
            }
            var q = new DoodlePainter(-0.92f, 0.55f, 0.92f, 1.9f, 110) { Ink = 0.045f };
            var pts = new List<float>();
            int n = 11;
            for (int i = 0; i <= n; i++)
            {
                // da sinistra a destra: base sul cranio, punte che si allargano a raggiera
                float a = (float)Math.PI * (0.92f - 0.84f * i / n);
                float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
                pts.Add(c * 0.55f); pts.Add(0.62f + s * 0.5f);
                float am = (float)Math.PI * (0.92f - 0.84f * (i + 0.5f) / n);
                if (i < n) { pts.Add((float)Math.Cos(am) * 0.85f); pts.Add(0.62f + (float)Math.Sin(am) * 1.2f); }
            }
            Sdf spikes = Sd.Subtract(Sd.Polygon(pts.ToArray()), Sd.Ellipse(0f, 0.62f, 0.56f, 0.5f));
            Body(q, spikes, V82BHair, V82BHairShade, null, 0.05f, 0.06f);
            // scintille della scossa
            q.Line(Rgba.Hex("FFD45C"), 0.035f, -0.78f, 1.55f, -0.66f, 1.45f, -0.74f, 1.38f, -0.6f, 1.3f);
            q.Line(Rgba.Hex("FFD45C"), 0.035f, 0.8f, 1.5f, 0.68f, 1.42f, 0.76f, 1.34f, 0.62f, 1.26f);
            return new DoodleArt("v82_b_hair_up", q);
        }

        /// <summary>Gli occhialoni (centro 0,0 = fra le lenti; lenti in ±0.21 di raggio 0.18, stanghette fino a ±0.48).</summary>
        public static DoodleArt V82B_Glasses()
        {
            var p = new DoodlePainter(-0.52f, -0.26f, 0.52f, 0.26f, 140) { Ink = 0.04f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Line(Rgba.Hex("2B2233"), 0.035f, s * 0.38f, 0.04f, s * 0.48f, 0.08f);
                p.Shape(Sd.Subtract(Sd.Circle(s * 0.21f, 0f, 0.2f), Sd.Circle(s * 0.21f, 0f, 0.15f)), Rgba.Hex("2B2233"), 0.02f);
                p.Fill(Sd.Circle(s * 0.21f, 0f, 0.15f), Rgba.Hex("CFEFFF", 0.35f), false);
                p.Line(Rgba.White.WithAlpha(0.8f), 0.025f, s * 0.21f - 0.08f, 0.06f, s * 0.21f - 0.03f, 0.1f);
            }
            p.Line(Rgba.Hex("2B2233"), 0.04f, -0.05f, 0.03f, 0f, 0.06f, 0.05f, 0.03f);
            return new DoodleArt("v82_b_glasses", p);
        }

        /// <summary>
        /// Le facce (stessa origine della testa, occhi in (±0.21, <see cref="V82BEyeY"/>) dietro le lenti): 0 sufficienza
        /// (palpebre a metà, sguardo all'insù, sopracciglio alzato, ghigno), 1 urlo (sopracciglia a V, bocca spalancata),
        /// 2 panico (sopracciglia all'insù, pupille piccole, bocca a zig-zag, gocce di sudore), 3 stupore (occhi sgranati,
        /// bocca a O).
        /// </summary>
        public static DoodleArt V82B_Face(int mood)
        {
            var p = new DoodlePainter(-0.72f, 0.06f, 0.72f, 1.12f, 140) { Ink = 0.035f };
            float ey = V82BEyeY;
            Rgba pupil = Rgba.Hex("2B2233");
            foreach (float s in new[] { -1f, 1f })
            {
                float ex = s * 0.21f;
                float r = mood == 3 ? 0.13f : 0.11f;
                p.Shape(Sd.Ellipse(ex, ey, r, r * 1.08f), Rgba.White, 0.025f);
                float pr = mood >= 2 ? 0.03f : 0.05f;
                float lx = mood == 0 ? 0.03f : 0f, ly = mood == 0 ? 0.035f : 0f;
                p.Fill(Sd.Circle(ex + lx, ey + ly, pr), pupil);
                if (mood == 0)
                {
                    // palpebra a metà
                    p.Shape(Sd.Intersect(Sd.Ellipse(ex, ey, r, r * 1.08f), Sd.Box(ex, ey + 0.09f, 0.15f, 0.06f)), SkinShade, 0.025f);
                }
                // sopracciglia
                float b = ey + 0.25f;
                switch (mood)
                {
                    case 0: p.InkLine(0.04f, ex - 0.12f, b + (s < 0 ? 0.07f : 0f), ex + 0.12f, b + (s < 0 ? 0.1f : -0.01f)); break;
                    case 1: p.InkLine(0.05f, ex - s * 0.13f, b + 0.04f, ex + s * 0.11f, b - 0.07f); break;
                    case 2: p.InkLine(0.04f, ex - s * 0.12f, b - 0.04f, ex + s * 0.11f, b + 0.06f); break;
                    default: p.InkLine(0.04f, ex - 0.11f, b + 0.08f, ex, b + 0.13f, ex + 0.11f, b + 0.08f); break;
                }
            }
            // naso (all'insù per la sufficienza)
            p.Shape(Sd.Ellipse(0.02f, 0.5f, 0.07f, 0.06f), SkinShade, 0.025f);
            switch (mood)
            {
                case 0:
                    p.InkLine(0.035f, -0.18f, 0.32f, -0.04f, 0.29f, 0.1f, 0.3f, 0.2f, 0.37f);
                    p.InkLine(0.025f, 0.19f, 0.41f, 0.23f, 0.34f);
                    break;
                case 1:
                    {
                        Sdf mouth = Sd.Ellipse(0f, 0.28f, 0.2f, 0.14f);
                        p.Shape(mouth, Rgba.Hex("6A1E2A"), 0.035f);
                        p.Fill(Sd.Intersect(mouth, Sd.Box(0f, 0.38f, 0.2f, 0.04f)), Rgba.White);
                        p.Fill(Sd.Ellipse(0f, 0.2f, 0.11f, 0.05f), Rgba.Hex("F06A86"));
                        p.Shape(mouth, Rgba.Clear, 0.035f);
                        break;
                    }
                case 2:
                    p.InkLine(0.03f, -0.17f, 0.3f, -0.1f, 0.35f, -0.03f, 0.29f, 0.04f, 0.35f, 0.11f, 0.29f, 0.17f, 0.34f);
                    foreach (float[] d in new[] { new[] { -0.52f, 0.86f }, new[] { 0.5f, 0.92f }, new[] { 0.56f, 0.7f } })
                    {
                        Sdf drop = Sd.Union(Sd.Circle(d[0], d[1], 0.05f), Sd.Polygon(d[0] - 0.045f, d[1] + 0.02f, d[0] + 0.045f, d[1] + 0.02f, d[0], d[1] + 0.12f));
                        p.Shape(drop, Rgba.Hex("8FD3FF"), 0.02f);
                    }
                    break;
                default:
                    p.Shape(Sd.Ellipse(0f, 0.27f, 0.09f, 0.12f), Rgba.Hex("6A1E2A"), 0.035f);
                    break;
            }
            string[] names = { "v82_b_face_smug", "v82_b_face_shout", "v82_b_face_panic", "v82_b_face_shock" };
            return new DoodleArt(names[Math.Max(0, Math.Min(3, mood))], p);
        }

        /// <summary>Il pulsante-risposta (centro 0,0, largo 1.2): tasto giallo arrotondato sul suo zoccolo scuro. Il numero lo scrive il livello.</summary>
        public static DoodleArt V82B_Key()
        {
            var p = new DoodlePainter(-0.7f, -0.62f, 0.7f, 0.56f, 120) { Ink = 0.045f };
            p.Shape(Sd.Box(0f, -0.12f, 0.6f, 0.44f, 0.16f), Rgba.Hex("B98A16"));
            Body(p, Sd.Box(0f, 0.03f, 0.58f, 0.44f, 0.16f), Rgba.Hex("FFD45C"), Rgba.Hex("E9B62E"), null, -0.05f, 0.06f);
            p.Fill(Sd.Box(-0.3f, 0.36f, 0.16f, 0.035f, 0.03f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("v82_b_key", p);
        }

        /// <summary>Un frammento di gesso (centro 0,0, largo 0.18) per il 5 che si frantuma.</summary>
        public static DoodleArt V82B_ChalkBit()
        {
            var p = new DoodlePainter(-0.13f, -0.11f, 0.13f, 0.11f, 140) { Ink = 0.025f };
            p.Shape(Sd.Polygon(-0.09f, -0.05f, -0.02f, 0.07f, 0.09f, 0.04f, 0.06f, -0.07f), V82BChalk);
            return new DoodleArt("v82_b_chalk_bit", p);
        }
    }
}
