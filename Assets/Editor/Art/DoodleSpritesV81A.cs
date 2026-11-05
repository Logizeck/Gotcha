using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.81 per il livello "Too Easy!" (roadmap n. 34): lo sfondo quasi vuoto (foglio a quadretti
    /// chiarissimi), due facce da sovrapporre alla cupola del pulsantone verde (riusato: "v76_a_button", scritta WIN come
    /// testo sul basamento): sorrisetto furbo e linguaccia con occhiolino; il cervellone rosa che applaude (corpo con
    /// faccia felice a occhi chiusi) e la sua manina col braccio (origine sulla spalla, la sinistra è la stessa
    /// specchiata). La faccia stupita della fine è "v76_a_face_wide", lo sbuffo "puff", le scintille "sparkle_star".
    /// Prefisso v81_a_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV81A() => new DoodleArt[]
        {
            V81A_Bg(), V81A_Face(0), V81A_Face(1), V81A_Brain(), V81A_Hand(),
        };

        static readonly Rgba V81APink = Rgba.Hex("F7A6C4"), V81APinkShade = Rgba.Hex("E27BA4"), V81APinkDeep = Rgba.Hex("C95A88");

        /// <summary>Spalle delle manine (coordinate locali del cervello): la destra in +x, la sinistra specchiata.</summary>
        public const float V81AShoulderX = 0.62f, V81AShoulderY = -0.95f;
        /// <summary>Distanza spalla-centro della mano (per far battere le mani davanti al cervello).</summary>
        public const float V81AArmLength = 0.62f;

        /// <summary>
        /// Lo sfondo: foglio bianco caldo a quadretti chiarissimi, con un leggero alone ai bordi. Quasi vuoto: c'è solo il
        /// pulsante (e la frase dell'obiettivo). Coordinate del mondo.
        /// </summary>
        public static DoodleArt V81A_Bg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 40) { Ink = 0.05f, Wobble = 0.004f };
            Sdf all = Sd.Box(0f, 0f, 4.6f, 7.1f);
            p.FillFunc(all, (x, y) =>
            {
                Rgba c = Rgba.Lerp(Rgba.Hex("FBF7EC"), Rgba.Hex("F3EEE0"), Math.Max(0f, Math.Min(1f, (3f - y) / 9f)));
                float v = Math.Max(0f, Math.Min(1f, (Math.Abs(x) - 2.0f) / 2.5f));
                return Rgba.Lerp(c, Rgba.Hex("E2DBC8"), v * 0.45f);
            });
            // quadretti
            Rgba grid = Rgba.Hex("A9C4DE", 0.28f);
            for (float x = -4.4f; x <= 4.4f; x += 0.4f) p.Fill(Sd.Box(x, 0f, 0.008f, 7.1f), grid, false);
            for (float y = -6.8f; y <= 6.8f; y += 0.4f) p.Fill(Sd.Box(0f, y, 4.6f, 0.008f), grid, false);
            // margine rosso del quaderno a sinistra
            p.Fill(Sd.Box(-2.5f, 0f, 0.012f, 7.1f), Rgba.Hex("E58A8A", 0.35f), false);
            return new DoodleArt("v81_a_bg", p);
        }

        /// <summary>
        /// Faccia sulla cupola del pulsante (stesse coordinate delle facce di "The Shy Button": occhi in ±0.3, 0.3):
        /// 0 sorrisetto furbo (palpebre a metà, sopracciglio alzato, ghigno storto), 1 linguaccia con occhiolino.
        /// </summary>
        public static DoodleArt V81A_Face(int mood)
        {
            var p = new DoodlePainter(-0.6f, -0.32f, 0.6f, 0.72f, 140) { Ink = 0.035f };
            float eyeY = 0.3f;
            Rgba lid = Rgba.Hex("3E9A35");
            foreach (float s in new[] { -1f, 1f })
            {
                float ex = s * 0.3f;
                bool wink = mood == 1 && s > 0f;
                if (wink)
                {
                    // occhiolino: un arco chiuso
                    p.InkLine(0.04f, ex - 0.13f, eyeY - 0.01f, ex - 0.06f, eyeY + 0.05f, ex + 0.06f, eyeY + 0.05f, ex + 0.13f, eyeY - 0.01f);
                    p.InkLine(0.035f, ex - 0.12f, eyeY + 0.19f, ex + 0.12f, eyeY + 0.15f);
                    continue;
                }
                p.Shape(Sd.Ellipse(ex, eyeY, 0.14f, 0.17f), Rgba.White);
                float px = ex + 0.04f, py = eyeY - 0.02f;
                p.Fill(Sd.Circle(px, py, 0.065f), Rgba.Hex("2B2233"));
                p.Fill(Sd.Circle(px - 0.02f, py + 0.025f, 0.022f), Rgba.White);
                // palpebra a metà (aria di sufficienza)
                Sdf lidS = Sd.Intersect(Sd.Ellipse(ex, eyeY, 0.14f, 0.17f), Sd.Box(ex, eyeY + 0.13f, 0.2f, 0.1f));
                p.Shape(lidS, lid, 0.03f);
                // sopracciglia: una alzata (la sinistra), l'altra piatta
                float by = eyeY + 0.25f + (s < 0f ? 0.06f : 0f);
                p.InkLine(0.035f, ex - 0.13f, by - (s < 0f ? 0.03f : 0f), ex + 0.13f, by + (s < 0f ? 0.03f : -0.01f));
            }
            if (mood == 0)
            {
                // ghigno storto verso destra
                p.InkLine(0.035f, -0.16f, 0.08f, -0.05f, 0.04f, 0.08f, 0.05f, 0.18f, 0.12f);
                p.InkLine(0.025f, 0.16f, 0.15f, 0.2f, 0.09f);
            }
            else
            {
                // bocca aperta e linguaccia che penzola fin sul collarino
                p.Shape(Sd.Box(0f, 0.07f, 0.17f, 0.06f, 0.05f), Rgba.Hex("5A1E2A"), 0.03f);
                Sdf tongue = Sd.Union(Sd.Box(0.02f, -0.06f, 0.1f, 0.12f, 0.06f), Sd.Ellipse(0.02f, -0.17f, 0.1f, 0.08f));
                p.Shape(tongue, Rgba.Hex("F06A86"), 0.03f);
                p.InkLine(0.02f, 0.02f, 0.02f, 0.02f, -0.15f);
                p.Fill(Sd.Ellipse(-0.03f, -0.12f, 0.025f, 0.04f), Rgba.White.WithAlpha(0.5f));
            }
            return new DoodleArt(mood == 1 ? "v81_a_face_tongue" : "v81_a_face_smug", p);
        }

        /// <summary>
        /// Il cervellone rosa (centro 0,0, largo circa 2.5, alto circa 1.9): due emisferi a lobi con le circonvoluzioni,
        /// la fessura in mezzo, il midollino sotto; faccia felice a occhi chiusi, guance rosse e bocca aperta in un
        /// "bravo!". Le manine (<see cref="V81A_Hand"/>) si attaccano in (±<see cref="V81AShoulderX"/>, <see cref="V81AShoulderY"/>).
        /// </summary>
        public static DoodleArt V81A_Brain()
        {
            var p = new DoodlePainter(-1.42f, -1.22f, 1.42f, 1.12f, 110) { Ink = 0.05f, Wobble = 0.01f };
            // ombra a terra
            p.Fill(Sd.Ellipse(0f, -1.08f, 0.95f, 0.1f), Rgba.Hex("2B2233", 0.18f), false);
            // midollino
            p.Shape(Sd.Box(0f, -0.88f, 0.16f, 0.22f, 0.1f), V81APinkShade);
            // emisferi a lobi
            Sdf lobes = Sd.Union(
                Sd.Ellipse(0f, 0.05f, 1.25f, 0.88f),
                Sd.Circle(-0.75f, 0.5f, 0.5f), Sd.Circle(0.75f, 0.5f, 0.5f),
                Sd.Circle(-0.25f, 0.68f, 0.42f), Sd.Circle(0.25f, 0.68f, 0.42f),
                Sd.Circle(-1.0f, -0.05f, 0.42f), Sd.Circle(1.0f, -0.05f, 0.42f),
                Sd.Circle(-0.55f, -0.5f, 0.42f), Sd.Circle(0.55f, -0.5f, 0.42f));
            Body(p, lobes, V81APink, V81APinkShade, null, -0.08f, 0.08f);
            // fessura centrale (solo in alto, sopra la faccia)
            p.InkLine(0.04f, 0f, 1.05f, 0.03f, 0.85f, -0.02f, 0.68f, 0.01f, 0.55f);
            // circonvoluzioni (ghirigori)
            Rgba g = V81APinkDeep;
            p.Line(g, 0.035f, -1.05f, 0.35f, -0.9f, 0.5f, -0.75f, 0.38f, -0.6f, 0.55f, -0.42f, 0.42f);
            p.Line(g, 0.035f, -0.62f, 0.85f, -0.45f, 0.72f, -0.3f, 0.88f, -0.15f, 0.75f);
            p.Line(g, 0.035f, -1.15f, -0.05f, -1.0f, 0.1f, -0.88f, -0.02f);
            p.Line(g, 0.035f, -0.85f, -0.55f, -0.7f, -0.42f, -0.6f, -0.6f, -0.45f, -0.5f);
            p.Line(g, 0.035f, 1.05f, 0.35f, 0.9f, 0.5f, 0.75f, 0.38f, 0.6f, 0.55f, 0.42f, 0.42f);
            p.Line(g, 0.035f, 0.62f, 0.85f, 0.45f, 0.72f, 0.3f, 0.88f, 0.15f, 0.75f);
            p.Line(g, 0.035f, 1.15f, -0.05f, 1.0f, 0.1f, 0.88f, -0.02f);
            p.Line(g, 0.035f, 0.85f, -0.55f, 0.7f, -0.42f, 0.6f, -0.6f, 0.45f, -0.5f);
            // luce
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.55f, 0.82f, 0.22f, 0.08f), -0.55f, 0.82f, 25f), Rgba.White.WithAlpha(0.6f));
            // faccia: occhi chiusi felici (archi), guance, bocca aperta
            foreach (float s in new[] { -1f, 1f })
            {
                float ex = s * 0.33f, ey = 0.12f;
                p.InkLine(0.05f, ex - 0.15f, ey - 0.03f, ex - 0.08f, ey + 0.06f, ex + 0.08f, ey + 0.06f, ex + 0.15f, ey - 0.03f);
                p.Fill(Sd.Ellipse(s * 0.6f, -0.08f, 0.13f, 0.08f), Rgba.Hex("FF6F91", 0.55f));
            }
            Sdf mouth = Sd.Intersect(Sd.Ellipse(0f, -0.12f, 0.24f, 0.22f), Sd.Box(0f, -0.25f, 0.3f, 0.13f));
            p.Shape(mouth, Rgba.Hex("7A2440"), 0.04f);
            p.Fill(Sd.Ellipse(0f, -0.32f, 0.12f, 0.05f), Rgba.Hex("F06A86"));
            return new DoodleArt("v81_a_brain", p);
        }

        /// <summary>
        /// La manina destra col braccio (origine sulla spalla, braccio verso l'alto): braccino rosa e mano a guanto
        /// bianco col pollice verso il centro del cervello (−x), centro del palmo a (0, <see cref="V81AArmLength"/>).
        /// La sinistra è la stessa con scala x = −1.
        /// </summary>
        public static DoodleArt V81A_Hand()
        {
            var p = new DoodlePainter(-0.32f, -0.1f, 0.3f, 0.95f, 140) { Ink = 0.04f, Wobble = 0.006f };
            float L = V81AArmLength;
            p.Shape(Sd.Capsule(0f, 0f, 0f, L - 0.15f, 0.065f), V81APinkShade);
            // guanto: palmo, quattro dita in alto, pollice verso -x
            Sdf palm = Sd.Union(
                Sd.Box(0f, L, 0.17f, 0.15f, 0.08f),
                Sd.Capsule(-0.11f, L + 0.1f, -0.12f, L + 0.24f, 0.045f),
                Sd.Capsule(-0.035f, L + 0.12f, -0.035f, L + 0.29f, 0.045f),
                Sd.Capsule(0.04f, L + 0.12f, 0.045f, L + 0.28f, 0.045f),
                Sd.Capsule(0.11f, L + 0.1f, 0.13f, L + 0.22f, 0.042f),
                Sd.Capsule(-0.14f, L - 0.03f, -0.25f, L + 0.07f, 0.045f));
            p.Shape(palm, Rgba.White);
            p.InkLine(0.022f, -0.08f, L - 0.06f, 0.08f, L - 0.06f);
            // polsino del guanto
            p.Shape(Sd.Box(0f, L - 0.16f, 0.13f, 0.045f, 0.03f), Rgba.Hex("F2F2F2"), 0.03f);
            return new DoodleArt("v81_a_hand", p);
        }
    }
}
