using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.80 per il livello 27 della roadmap "The Sad Balloon" (lo sfondo è il parco "park_bg", i
    /// cuoricini della scia sono "heart", il fumetto è "bubble_box"): il palloncino rosso (corpo col nodino in fondo,
    /// senza faccia), la faccia triste e la faccia felice a parte (si scambiano), il sassolino grigio, il nodo dello
    /// spago avvolto intorno al sasso (a parte: sparisce quando si scioglie) e il ricciolo dello spago libero (all'estremo
    /// dello spago sciolto). Lo spago vero è una LineRenderer del livello. Nessuna scritta disegnata. Prefisso v80_a_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV80A() => new DoodleArt[]
        {
            V80A_Balloon(), V80A_FaceSad(), V80A_FaceHappy(), V80A_Stone(), V80A_Knot(), V80A_LooseEnd(),
        };

        // misure condivise col builder
        /// <summary>Punto del palloncino (sotto il nodino) da cui parte lo spago, coordinate locali del palloncino.</summary>
        public const float V80ATieY = -1.08f;
        /// <summary>Raggi del palloncino (ellisse).</summary>
        public const float V80ABalloonRx = 0.78f, V80ABalloonRy = 0.95f;
        /// <summary>Punto del sasso (in cima al nodo) dove arriva lo spago, coordinate locali del sasso.</summary>
        public const float V80AKnotY = 0.27f;

        static readonly Rgba V80ARed = Rgba.Hex("E8333F"), V80ARedShade = Rgba.Hex("B21E2C"), V80AString = Rgba.Hex("C9A46E");
        static readonly Rgba V80AStone = Rgba.Hex("9EA3AD"), V80AStoneShade = Rgba.Hex("767B86"), V80AEye = Rgba.Hex("2B2133");

        /// <summary>
        /// Il palloncino (origine al centro dell'ellisse 0.78 x 0.95): rosso lucido con il riflesso, il nodino a triangolo
        /// in fondo (punta dello spago in y <see cref="V80ATieY"/>). La faccia è a parte.
        /// </summary>
        public static DoodleArt V80A_Balloon()
        {
            var p = new DoodlePainter(-0.9f, -1.16f, 0.9f, 1.04f, 110) { Ink = 0.05f };
            p.Shape(Sd.Polygon(0f, -0.9f, 0.13f, -1.07f, -0.13f, -1.07f), V80ARedShade, 0.035f);   // nodino
            Body(p, Sd.Union(Sd.Ellipse(0f, 0f, V80ABalloonRx, V80ABalloonRy), Sd.Ellipse(0f, -0.62f, 0.42f, 0.32f)), V80ARed, V80ARedShade, null, -0.08f, 0.08f);
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.36f, 0.48f, 0.13f, 0.24f), -0.36f, 0.48f, -28f), Rgba.White.WithAlpha(0.75f));
            p.Fill(Sd.Circle(-0.18f, 0.74f, 0.05f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("v80_a_balloon", p);
        }

        /// <summary>La faccia triste (origine al centro del palloncino): sopracciglia a "tetto", occhi lucidi, bocca all'ingiù, lacrimuccia.</summary>
        public static DoodleArt V80A_FaceSad()
        {
            var p = new DoodlePainter(-0.6f, -0.55f, 0.6f, 0.55f, 120) { Ink = 0f, Wobble = 0.004f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Ellipse(s * 0.26f, 0.12f, 0.085f, 0.11f), V80AEye);
                p.Fill(Sd.Circle(s * 0.26f - 0.03f, 0.16f, 0.03f), Rgba.White);
                p.Line(V80AEye, 0.04f, s * 0.14f, 0.36f, s * 0.36f, 0.27f);   // sopracciglio triste (alto verso il centro)
            }
            // bocca all'ingiù
            var mouth = new List<float>();
            for (int i = 0; i <= 10; i++)
            {
                double a = Math.PI * (0.15 + 0.7 * i / 10.0);
                mouth.Add(0.2f * (float)Math.Cos(a));
                mouth.Add(-0.36f + 0.12f * (float)Math.Sin(a));
            }
            p.Line(V80AEye, 0.045f, mouth.ToArray());
            // lacrimuccia sotto l'occhio destro
            p.Shape(Sd.Union(Sd.Circle(0.36f, -0.08f, 0.055f), Sd.Polygon(0.31f, -0.06f, 0.41f, -0.06f, 0.36f, 0.05f)), Rgba.Hex("8FD3FF"), 0.02f);
            return new DoodleArt("v80_a_face_sad", p);
        }

        /// <summary>La faccia felice (origine al centro del palloncino): occhi chiusi a "^", guance rosa, sorrisone aperto.</summary>
        public static DoodleArt V80A_FaceHappy()
        {
            var p = new DoodlePainter(-0.6f, -0.55f, 0.6f, 0.55f, 120) { Ink = 0f, Wobble = 0.004f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Line(V80AEye, 0.05f, s * 0.36f, 0.08f, s * 0.26f, 0.2f, s * 0.16f, 0.08f);   // occhi felici
                p.Fill(Sd.Ellipse(s * 0.42f, -0.12f, 0.1f, 0.065f), Rgba.Hex("FF9AB0", 0.85f));
            }
            Sdf smile = Sd.Intersect(Sd.Ellipse(0f, -0.14f, 0.27f, 0.25f), Sd.Box(0f, -0.3f, 0.3f, 0.16f));
            p.Shape(smile, Rgba.Hex("5A1F3A"), 0.035f);
            p.Fill(Sd.Intersect(smile, Sd.Ellipse(0f, -0.4f, 0.14f, 0.07f)), Rgba.Hex("FF7A8A"));
            return new DoodleArt("v80_a_face_happy", p);
        }

        /// <summary>Il sassolino grigio (origine al centro, 0.64 x 0.44) con qualche puntino e un'ombra sotto.</summary>
        public static DoodleArt V80A_Stone()
        {
            var p = new DoodlePainter(-0.42f, -0.3f, 0.42f, 0.28f, 130) { Ink = 0.04f };
            p.Fill(Sd.Ellipse(0f, -0.2f, 0.36f, 0.07f), new Rgba(0f, 0f, 0f, 0.22f));
            Body(p, Sd.Union(Sd.Ellipse(0f, 0f, 0.32f, 0.21f), Sd.Ellipse(0.08f, 0.06f, 0.2f, 0.16f)), V80AStone, V80AStoneShade);
            foreach (float[] d in new[] { new[] { -0.15f, -0.05f }, new[] { 0.12f, -0.09f }, new[] { 0.2f, 0.04f } })
                p.Fill(Sd.Circle(d[0], d[1], 0.022f), V80AStoneShade);
            return new DoodleArt("v80_a_stone", p);
        }

        /// <summary>
        /// Il nodo dello spago intorno al sasso (origine al centro del sasso): due giri di spago chiaro che fasciano il sasso
        /// e il nodo in cima, con i due codini; lo spago parte da y <see cref="V80AKnotY"/>.
        /// </summary>
        public static DoodleArt V80A_Knot()
        {
            var p = new DoodlePainter(-0.42f, -0.3f, 0.42f, 0.36f, 130) { Ink = 0.025f, Wobble = 0.003f };
            p.Shape(Sd.Polyline(0.022f, -0.3f, -0.02f, -0.1f, -0.06f, 0.1f, -0.06f, 0.3f, 0.0f), V80AString, 0.02f);
            p.Shape(Sd.Polyline(0.022f, -0.24f, 0.12f, -0.02f, 0.2f, 0.22f, 0.12f), V80AString, 0.02f);
            p.Shape(Sd.Polyline(0.022f, -0.06f, 0.2f, -0.12f, -0.1f, -0.06f, -0.2f), V80AString, 0.02f);
            p.Shape(Sd.Circle(0f, 0.2f, 0.07f), V80AString, 0.025f);   // il nodo
            p.Shape(Sd.Polyline(0.018f, 0.04f, 0.17f, 0.16f, 0.12f, 0.2f, 0.02f), V80AString, 0.018f);   // codino
            p.Shape(Sd.Polyline(0.018f, -0.04f, 0.17f, -0.15f, 0.1f, -0.2f, 0.0f), V80AString, 0.018f);
            return new DoodleArt("v80_a_knot", p);
        }

        /// <summary>Il ricciolo dello spago sciolto (origine all'estremo dello spago, il ricciolo pende sotto).</summary>
        public static DoodleArt V80A_LooseEnd()
        {
            var p = new DoodlePainter(-0.2f, -0.26f, 0.2f, 0.06f, 130) { Ink = 0.02f, Wobble = 0.003f };
            var curl = new List<float>();
            for (int i = 0; i <= 18; i++)
            {
                double a = Math.PI * 0.5 + i / 18.0 * Math.PI * 1.7;
                float r = 0.11f - 0.04f * i / 18f;
                curl.Add(r * (float)Math.Cos(a));
                curl.Add(-0.11f + r * (float)Math.Sin(a));
            }
            p.Shape(Sd.Polyline(0.02f, curl.ToArray()), V80AString, 0.02f);
            return new DoodleArt("v80_a_loose_end", p);
        }
    }
}
