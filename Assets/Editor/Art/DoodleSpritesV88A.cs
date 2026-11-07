using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.88 per il livello 138 della roadmap "The Broken Applause" (lo sfondo è il palco vuoto
    /// "s41b_stage_bg", il bagliore del battito è "glow", i fumetti "bubble_box", i coriandoli quelli della festa comune):
    /// la grande mano a guanto da cartone animato (disegnata come mano SINISTRA: palmo e pollice rivolti a DESTRA, verso
    /// l'altra mano; il livello rovescia in x quella di destra), la stella dell'impatto del battito, gli spettatori (quattro
    /// varianti di testa e spalle, origine al centro della testa), quattro facce comuni (annoiata a palpebre basse, che
    /// sbadiglia, curiosa che guarda in su, felice), le manine che applaudono (alzate davanti al petto) e la fila di
    /// poltroncine rosse davanti a ogni fila di spettatori. Nessuna scritta disegnata. Prefisso v88_a_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV88A() => new DoodleArt[]
        {
            V88A_Hand(), V88A_Impact(),
            V88A_Fan(0), V88A_Fan(1), V88A_Fan(2), V88A_Fan(3),
            V88A_FaceBored(), V88A_FaceYawn(), V88A_FaceLook(), V88A_FaceHappy(),
            V88A_FanClap(), V88A_Seats(),
        };

        // misure condivise col builder
        /// <summary>Raggio della testa degli spettatori (origine al centro della testa).</summary>
        public const float V88AHeadR = 0.3f;
        /// <summary>Metà larghezza del palmo della mano (dal centro del palmo al bordo verso l'altra mano, pollice escluso).</summary>
        public const float V88APalmHx = 0.42f;

        static readonly Rgba V88AGlove = Rgba.Hex("FFFFFF"), V88AGloveShade = Rgba.Hex("D8D4E6"), V88AGloveCuff = Rgba.Hex("F3EFFA");
        static readonly Rgba V88AEye = Rgba.Hex("2B2133"), V88AMouth = Rgba.Hex("5A1F3A"), V88ATongue = Rgba.Hex("FF7A8A");
        static readonly Rgba V88ASeat = Rgba.Hex("B8283A"), V88ASeatShade = Rgba.Hex("8A1A2A");

        /// <summary>
        /// La grande mano a guanto (origine al centro del palmo): quattro dita in su, il pollice che punta a destra (verso
        /// l'altra mano), le tre cuciture sul dorso e il polsino arrotolato sotto.
        /// </summary>
        public static DoodleArt V88A_Hand()
        {
            var p = new DoodlePainter(-0.62f, -1.02f, 0.72f, 1.08f, 130) { Ink = 0.05f };
            // polsino
            Body(p, Sd.Box(-0.02f, -0.8f, 0.4f, 0.17f, 0.08f), V88AGloveCuff, V88AGloveShade);
            p.Line(V88AGloveShade, 0.03f, -0.36f, -0.75f, 0.32f, -0.75f);
            p.Line(V88AGloveShade, 0.03f, -0.36f, -0.86f, 0.32f, -0.86f);
            // dita (dietro il palmo) e pollice
            float[] fx = { -0.3f, -0.1f, 0.1f, 0.28f }, top = { 0.8f, 0.95f, 0.9f, 0.74f };
            for (int i = 0; i < 4; i++) Body(p, Sd.Capsule(fx[i], 0.2f, fx[i], top[i], 0.115f), V88AGlove, V88AGloveShade);
            Body(p, Sd.Capsule(0.28f, -0.2f, 0.56f, 0.2f, 0.13f), V88AGlove, V88AGloveShade);
            // palmo
            Sdf palm = Sd.Union(Sd.Ellipse(0f, -0.1f, V88APalmHx, 0.5f), Sd.Box(-0.01f, -0.55f, 0.33f, 0.12f, 0.06f));
            Body(p, palm, V88AGlove, V88AGloveShade);
            // le cuciture del guanto e le pieghe delle dita
            foreach (float x in new[] { -0.16f, 0f, 0.16f }) p.Line(V88AEye, 0.03f, x, -0.38f, x * 1.1f, 0.02f);
            for (int i = 0; i < 4; i++) p.Line(V88AGloveShade, 0.025f, fx[i] - 0.06f, 0.42f, fx[i] + 0.06f, 0.42f);
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.22f, 0.12f, 0.08f, 0.05f), -0.22f, 0.12f, 40f), Rgba.White.WithAlpha(0.9f));
            return new DoodleArt("v88_a_hand", p);
        }

        /// <summary>La stella gialla dell'impatto del battito (origine al centro, raggio 0.55) con un cuore chiaro.</summary>
        public static DoodleArt V88A_Impact()
        {
            var p = new DoodlePainter(-0.62f, -0.62f, 0.62f, 0.62f, 150) { Ink = 0.04f };
            var pts = new List<float>();
            for (int i = 0; i < 20; i++)
            {
                double a = Math.PI * 2 * i / 20.0 + 0.1;
                float r = i % 2 == 0 ? 0.56f : 0.3f;
                pts.Add(r * (float)Math.Cos(a));
                pts.Add(r * (float)Math.Sin(a));
            }
            p.Shape(Sd.Polygon(pts.ToArray()), Rgba.Hex("FFD84A"));
            p.Fill(Sd.Circle(0f, 0f, 0.2f), Rgba.Hex("FFF6C8"));
            return new DoodleArt("v88_a_impact", p);
        }

        /// <summary>
        /// Uno spettatore (origine al centro della testa, raggio <see cref="V88AHeadR"/>): spalle con la maglia colorata e
        /// la testa; quattro varianti di pelle, capelli e maglia (0 calvo col ciuffo, 1 chignon, 2 codini, 3 berretto).
        /// La faccia è a parte.
        /// </summary>
        public static DoodleArt V88A_Fan(int v)
        {
            string[] skins = { "F2C29B", "C98A5E", "FFDDC2", "8D5A3B" };
            string[] shirts = { "4FA3E0", "6CC56A", "F28C3A", "9B6ADB" };
            string[] shirtShades = { "357FB8", "4C9A4A", "C96A20", "7348B0" };
            string[] hairs = { "4A3428", "1F1A24", "E0A030", "3A2A1E" };
            Rgba skin = Rgba.Hex(skins[v]), skinShade = Rgba.Lerp(skin, Rgba.Hex("6B3A2A"), 0.25f);
            Rgba shirt = Rgba.Hex(shirts[v]), shirtShade = Rgba.Hex(shirtShades[v]), hair = Rgba.Hex(hairs[v]);
            var p = new DoodlePainter(-0.5f, -1.0f, 0.5f, 0.56f, 140) { Ink = 0.04f };
            Body(p, Sd.Union(Sd.Ellipse(0f, -0.68f, 0.42f, 0.32f), Sd.Box(0f, -0.88f, 0.42f, 0.12f)), shirt, shirtShade);
            p.Shape(Sd.Box(0f, -0.38f, 0.08f, 0.08f), skinShade, 0.03f);   // collo
            if (v == 1) Body(p, Sd.Circle(0f, 0.36f, 0.13f), hair, hair);   // chignon
            if (v == 2) foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Ellipse(s * 0.33f, 0.08f, 0.1f, 0.15f), hair, hair);   // codini
            Body(p, Sd.Circle(0f, 0f, V88AHeadR), skin, skinShade);
            switch (v)
            {
                case 0:   // calvo con un ciuffo
                    p.Line(hair, 0.035f, -0.04f, 0.28f, -0.1f, 0.4f, -0.02f, 0.47f);
                    p.Line(hair, 0.035f, 0.04f, 0.29f, 0.08f, 0.42f);
                    break;
                case 1:
                case 2:   // frangetta
                    p.Shape(Sd.Intersect(Sd.Circle(0f, 0f, V88AHeadR + 0.01f), Sd.Box(0f, 0.25f, 0.4f, 0.1f)), hair, 0.03f);
                    break;
                case 3:   // berretto con la visiera
                    p.Shape(Sd.Intersect(Sd.Circle(0f, 0.02f, V88AHeadR + 0.03f), Sd.Box(0f, 0.25f, 0.4f, 0.12f)), Rgba.Hex("E8433F"), 0.035f);
                    p.Shape(Sd.Box(0.22f, 0.14f, 0.16f, 0.04f, 0.03f), Rgba.Hex("B82E2B"), 0.03f);
                    break;
            }
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Ellipse(s * 0.3f, 0f, 0.06f, 0.08f), skin, 0.03f);   // orecchie
            return new DoodleArt("v88_a_fan_" + v, p);
        }

        /// <summary>Faccia annoiata (origine al centro della testa): palpebre a metà, occhiaie, bocca piatta storta.</summary>
        public static DoodleArt V88A_FaceBored()
        {
            var p = new DoodlePainter(-0.26f, -0.22f, 0.26f, 0.2f, 150) { Ink = 0f, Wobble = 0.004f };
            foreach (float s in new[] { -1f, 1f })
            {
                // solo la metà bassa dell'occhio: la palpebra è calata
                Sdf eye = Sd.Intersect(Sd.Ellipse(s * 0.11f, 0.04f, 0.065f, 0.06f), Sd.Box(s * 0.11f, 0.0f, 0.08f, 0.05f));
                p.Fill(eye, Rgba.White);
                p.Fill(Sd.Intersect(Sd.Circle(s * 0.11f, 0.01f, 0.032f), eye), V88AEye);
                p.Line(V88AEye, 0.022f, s * 0.05f, 0.05f, s * 0.17f, 0.05f);
                p.Line(Rgba.Hex("8A6A9A", 0.6f), 0.018f, s * 0.06f, -0.04f, s * 0.15f, -0.035f);   // occhiaie
            }
            p.Line(V88AEye, 0.026f, -0.08f, -0.12f, 0.08f, -0.1f);
            return new DoodleArt("v88_a_face_bored", p);
        }

        /// <summary>Faccia che sbadiglia (origine al centro della testa): occhi chiusi strizzati, boccone ovale aperto con la lingua.</summary>
        public static DoodleArt V88A_FaceYawn()
        {
            var p = new DoodlePainter(-0.26f, -0.26f, 0.26f, 0.2f, 150) { Ink = 0f, Wobble = 0.004f };
            foreach (float s in new[] { -1f, 1f }) p.Line(V88AEye, 0.026f, s * 0.05f, 0.06f, s * 0.11f, 0.03f, s * 0.17f, 0.06f);
            Sdf mouth = Sd.Ellipse(0f, -0.1f, 0.085f, 0.12f);
            p.Shape(mouth, V88AMouth, 0.022f);
            p.Fill(Sd.Intersect(mouth, Sd.Ellipse(0f, -0.2f, 0.07f, 0.05f)), V88ATongue);
            return new DoodleArt("v88_a_face_yawn", p);
        }

        /// <summary>Faccia curiosa (origine al centro della testa): occhi tondi che guardano in su, sopracciglia alzate, boccuccia "o".</summary>
        public static DoodleArt V88A_FaceLook()
        {
            var p = new DoodlePainter(-0.26f, -0.22f, 0.26f, 0.22f, 150) { Ink = 0f, Wobble = 0.004f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Ellipse(s * 0.11f, 0.04f, 0.065f, 0.075f), Rgba.White);
                p.Fill(Sd.Circle(s * 0.11f, 0.07f, 0.033f), V88AEye);
                p.Line(V88AEye, 0.022f, s * 0.06f, 0.15f, s * 0.16f, 0.17f);
            }
            p.Shape(Sd.Ellipse(0f, -0.11f, 0.035f, 0.04f), V88AMouth, 0.018f);
            return new DoodleArt("v88_a_face_look", p);
        }

        /// <summary>Faccia felice (origine al centro della testa): occhi chiusi "^", guance rosa, sorrisone aperto.</summary>
        public static DoodleArt V88A_FaceHappy()
        {
            var p = new DoodlePainter(-0.26f, -0.24f, 0.26f, 0.2f, 150) { Ink = 0f, Wobble = 0.004f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Line(V88AEye, 0.026f, s * 0.17f, 0.03f, s * 0.11f, 0.08f, s * 0.05f, 0.03f);
                p.Fill(Sd.Ellipse(s * 0.19f, -0.05f, 0.055f, 0.035f), BlushColor.WithAlpha(0.85f));
            }
            Sdf smile = Sd.Intersect(Sd.Ellipse(0f, -0.06f, 0.12f, 0.11f), Sd.Box(0f, -0.13f, 0.14f, 0.07f));
            p.Shape(smile, V88AMouth, 0.02f);
            p.Fill(Sd.Intersect(smile, Sd.Ellipse(0f, -0.18f, 0.07f, 0.035f)), V88ATongue);
            return new DoodleArt("v88_a_face_happy", p);
        }

        /// <summary>Le manine che applaudono (origine al centro della testa): due manine guantate giunte davanti al petto, sotto il mento.</summary>
        public static DoodleArt V88A_FanClap()
        {
            var p = new DoodlePainter(-0.42f, -0.82f, 0.42f, -0.18f, 150) { Ink = 0.03f };
            Rgba skin = V88AGlove, shade = V88AGloveShade;   // guantini bianchi come le grandi mani
            foreach (float s in new[] { -1f, 1f })
                Body(p, Sd.Capsule(s * 0.3f, -0.75f, s * 0.1f, -0.5f, 0.06f), Rgba.Hex("6B5A8E"), Rgba.Hex("4E3F6E"));   // maniche
            foreach (float s in new[] { -1f, 1f })
                Body(p, Sd.Rotate(Sd.Ellipse(s * 0.05f, -0.4f, 0.06f, 0.12f), s * 0.05f, -0.4f, -s * 12f), skin, shade);
            foreach (float s in new[] { -1f, 1f }) p.Line(V88AEye, 0.02f, s * 0.16f, -0.32f, s * 0.22f, -0.27f);   // trattini del battito
            return new DoodleArt("v88_a_fan_clap", p);
        }

        /// <summary>
        /// Una fila di poltroncine rosse da teatro (origine al centro, larga 6, alta 0.62): schienali tondi ogni 1.0,
        /// a coprire le spalle degli spettatori della fila.
        /// </summary>
        public static DoodleArt V88A_Seats()
        {
            var p = new DoodlePainter(-3.05f, -0.36f, 3.05f, 0.36f, 80) { Ink = 0.045f };
            p.Shape(Sd.Box(0f, -0.12f, 3.05f, 0.2f), V88ASeatShade);
            for (float x = -3f; x <= 3.01f; x += 0.5f)
                Body(p, Sd.Box(x, 0.04f, 0.22f, 0.27f, 0.12f), V88ASeat, V88ASeatShade);
            p.Line(Rgba.Hex("FFC93C"), 0.03f, -3.05f, -0.24f, 3.05f, -0.24f);
            return new DoodleArt("v88_a_seats", p);
        }
    }
}
