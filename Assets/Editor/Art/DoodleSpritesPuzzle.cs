using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>L'immagine del puzzle degli Easter egg: Toni in palestra (50 pezzi, 5 x 10).</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtPuzzle() => new[] { PuzzleToniGym() };

        /// <summary>
        /// Toni solleva un bilanciere piegato dal peso, con una ciambella e una forma di formaggio al posto dei dischi:
        /// fascia rossa in testa, occhi strizzati, lingua di fuori, gocce di sudore e gambette che tremano. 5 x 10 unità.
        /// </summary>
        static DoodleArt PuzzleToniGym()
        {
            var p = new DoodlePainter(-2.5f, -5f, 2.5f, 5f, 100) { Ink = 0.07f, Wobble = 0.01f };
            // palestra: muro con le strisce, specchio, pavimento di legno
            p.FillFunc(Sd.Box(0, 0, 2.6f, 5.1f), (x, y) => Rgba.Lerp(Rgba.Hex("FFB867"), Rgba.Hex("B86BD6"), Math.Max(0f, Math.Min(1f, (y + 3f) / 8f))));
            foreach (float y in new[] { -2.2f, -1.6f }) p.Fill(Sd.Box(0, y, 2.6f, 0.12f), Rgba.Hex("FFFFFF").WithAlpha(0.35f));
            p.Shape(Sd.Box(-1.6f, 3.9f, 0.7f, 0.55f, 0.08f), Rgba.Hex("D6F1FF"), 0.05f);   // quadretto con il cuore
            p.Shape(Sd.Union(Sd.Circle(-1.72f, 3.97f, 0.16f), Sd.Circle(-1.48f, 3.97f, 0.16f), Sd.Polygon(-1.88f, 3.9f, -1.32f, 3.9f, -1.6f, 3.58f)), Rgba.Hex("FF5D7A"), 0.04f);
            p.FillFunc(Sd.Box(0, -4.3f, 2.6f, 0.8f), (x, y) => (int)Math.Floor((x + 3f) / 0.7f) % 2 == 0 ? Rgba.Hex("C98A55") : Rgba.Hex("B9794A"));
            p.Line(Rgba.Hex("2B2133"), 0.06f, -2.6f, -3.5f, 2.6f, -3.5f);

            // gambette tremanti (con le righe del tremolio)
            foreach (float sx in new[] { -1f, 1f })
            {
                Body(p, Sd.Ellipse(sx * 0.55f, -3.1f, 0.34f, 0.5f), PugFawn, PugShade);
                Body(p, Sd.Ellipse(sx * 0.62f, -3.55f, 0.42f, 0.18f), PugShade, Rgba.Hex("141418"));
                p.Line(Rgba.Hex("2B2133"), 0.04f, sx * 1.05f, -2.85f, sx * 1.25f, -2.75f);
                p.Line(Rgba.Hex("2B2133"), 0.04f, sx * 1.05f, -3.15f, sx * 1.3f, -3.15f);
            }
            // corpo a pera, pancetta più chiara
            Body(p, Sd.Ellipse(0, -1.7f, 1.2f, 1.45f), PugFawn, PugShade, null, -0.2f, 0.25f);
            p.Fill(Sd.Ellipse(0, -2.0f, 0.68f, 0.85f), Rgba.Hex("55555F"));
            // canotta da palestra gialla
            p.Shape(Sd.Intersect(Sd.Ellipse(0, -1.55f, 1.18f, 1.3f), Sd.Box(0, -1.15f, 1.3f, 0.75f)), Rgba.Hex("FFD23F"), 0.05f);
            p.Line(Rgba.Hex("E2A800"), 0.06f, -0.35f, -0.55f, 0f, -0.9f, 0.35f, -0.55f);

            // braccia alzate che reggono il bilanciere
            foreach (float sx in new[] { -1f, 1f })
            {
                Body(p, Sd.Capsule(sx * 0.95f, -0.75f, sx * 1.62f, 2.15f, 0.3f), PugFawn, PugShade);
                Body(p, Sd.Circle(sx * 1.66f, 2.32f, 0.34f), PugShade, Rgba.Hex("141418"));
                // righe dello sforzo
                p.Line(Rgba.Hex("FFFFFF"), 0.05f, sx * 2.15f, 0.6f, sx * 2.4f, 0.75f);
                p.Line(Rgba.Hex("FFFFFF"), 0.05f, sx * 2.15f, 1.0f, sx * 2.42f, 1.05f);
            }

            // bilanciere piegato dal peso, dischi: una ciambella e una forma di formaggio
            p.Line(Rgba.Hex("2B2133"), 0.2f, -2.2f, 2.08f, -1.6f, 2.42f, 0f, 2.6f, 1.6f, 2.42f, 2.2f, 2.08f);
            p.Line(Rgba.Hex("C7CCD6"), 0.1f, -2.2f, 2.08f, -1.6f, 2.42f, 0f, 2.6f, 1.6f, 2.42f, 2.2f, 2.08f);
            Sdf donut = Sd.Subtract(Sd.Circle(-1.82f, 2.2f, 0.62f), Sd.Circle(-1.82f, 2.2f, 0.18f));
            Body(p, donut, Rgba.Hex("E9B27A"), Rgba.Hex("C98A55"));
            p.Fill(Sd.Subtract(Sd.Circle(-1.82f, 2.26f, 0.52f), Sd.Circle(-1.82f, 2.2f, 0.25f)), Rgba.Hex("FF8FB4"));
            var rnd = new Random(11);
            Rgba[] sprinkle = { Rgba.Hex("FFFFFF"), Rgba.Hex("6AD3FF"), Rgba.Hex("FFE066"), Rgba.Hex("7BE07B") };
            for (int i = 0; i < 16; i++)
            {
                double a = rnd.NextDouble() * Math.PI * 2, r = 0.31 + rnd.NextDouble() * 0.18;
                float cx = -1.82f + (float)(Math.Cos(a) * r), cy = 2.26f + (float)(Math.Sin(a) * r);
                p.Line(sprinkle[i % sprinkle.Length], 0.05f, cx - 0.05f, cy - 0.03f, cx + 0.05f, cy + 0.03f);
            }
            Body(p, Sd.Box(1.92f, 2.2f, 0.36f, 0.66f, 0.12f), Rgba.Hex("FFD54A"), Rgba.Hex("E6B323"));
            foreach (var h in new[] { (1.8f, 2.55f, 0.1f), (2.05f, 2.05f, 0.12f), (1.82f, 1.72f, 0.08f), (2.1f, 2.7f, 0.06f) })
                p.Shape(Sd.Circle(h.Item1, h.Item2, h.Item3), Rgba.Hex("E6B323"), 0.03f);

            // testa: orecchie, fascia rossa, occhi strizzati, guance gonfie, lingua di fuori
            foreach (float sx in new[] { -1f, 1f })
                Body(p, Sd.Rotate(Sd.Ellipse(sx * 0.95f, 1.25f, 0.42f, 0.27f), sx * 0.95f, 1.25f, sx * -30f), PugShade, Rgba.Hex("141418"));
            Body(p, Sd.Ellipse(0, 0.55f, 1.2f, 1.0f), PugFawn, PugShade, null, -0.15f, 0.2f);
            p.Shape(Sd.Intersect(Sd.Ellipse(0, 0.55f, 1.21f, 1.01f), Sd.Box(0, 1.05f, 1.3f, 0.16f)), Rgba.Hex("E8233F"), 0.04f);
            p.Line(Rgba.Hex("FFFFFF"), 0.05f, -0.9f, 1.05f, 0.9f, 1.05f);
            foreach (float sx in new[] { -1f, 1f })   // occhi strizzati a ">" "<"
                p.Line(Rgba.Hex("141418"), 0.09f, sx * 0.72f, 0.75f, sx * 0.4f, 0.6f, sx * 0.72f, 0.45f);
            Body(p, Sd.Ellipse(0, 0.05f, 0.75f, 0.5f), PugMask, PugShade);
            p.Shape(Sd.Ellipse(0, 0.25f, 0.27f, 0.17f), Rgba.Hex("120D0C"), 0.04f);
            Body(p, Sd.Rotate(Sd.Capsule(0.15f, -0.2f, 0.45f, -0.75f, 0.17f), 0.3f, -0.45f, 10f), Rgba.Hex("FF7A93"), Rgba.Hex("E0566F"));
            foreach (float sx in new[] { -1f, 1f }) p.Fill(Sd.Circle(sx * 0.82f, 0.15f, 0.18f), Rgba.Hex("FF8FA3").WithAlpha(0.55f));

            // gocce di sudore e stelline
            foreach (var d in new[] { (-1.45f, 1.0f), (1.4f, 0.95f), (-1.25f, -0.1f), (1.55f, 0.1f), (0.3f, 1.55f) })
                p.Shape(Sd.Union(Sd.Circle(d.Item1, d.Item2, 0.1f), Sd.Polygon(d.Item1 - 0.09f, d.Item2 + 0.03f, d.Item1 + 0.09f, d.Item2 + 0.03f, d.Item1, d.Item2 + 0.25f)), Rgba.Hex("8FD8FF"), 0.03f);
            foreach (var s in new[] { (-0.2f, 3.5f), (0.9f, 3.25f), (-0.95f, 3.2f) })
                p.Shape(StarShape(s.Item1, s.Item2, 0.16f, 0.07f), Rgba.Hex("FFF27A"), 0.03f);
            return new DoodleArt("puzzle_toni_gym", p);
        }

        /// <summary>Stellina a 5 punte (poligono).</summary>
        static Sdf StarShape(float cx, float cy, float outer, float inner)
        {
            var xy = new float[20];
            for (int i = 0; i < 10; i++)
            {
                double a = Math.PI / 2 + i * Math.PI / 5;
                float r = i % 2 == 0 ? outer : inner;
                xy[i * 2] = cx + (float)(Math.Cos(a) * r);
                xy[i * 2 + 1] = cy + (float)(Math.Sin(a) * r);
            }
            return Sd.Polygon(xy);
        }
    }
}
