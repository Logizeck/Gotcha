using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.84 per il livello 63 della roadmap "The Tiny Crown" (lo sfondo è la sala del trono
    /// "s41b_throne_bg", il trono è "s41b_throne", la corona minuscola è "s41b_crown", le scintille sono "sparkle_star",
    /// il bagliore sulla testa è "glow", il fumetto è "bubble_box"): la testona del re (pelata, orecchie, nasone, baffi e
    /// barbone bianco, senza occhi), gli occhi preoccupati e gli occhi felici a parte (si scambiano), il corpo seduto e il
    /// corpo in ginocchio (mantello viola col bordo di ermellino), il cuscino di velluto rosso con le nappe e il piedistallo
    /// d'oro. Nessuna scritta disegnata. Prefisso v84_a_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV84A() => new DoodleArt[]
        {
            V84A_Head(), V84A_Eyes(), V84A_EyesHappy(), V84A_BodySit(), V84A_BodyKneel(), V84A_Cushion(), V84A_Stand(),
        };

        // misure condivise col builder
        /// <summary>Cima della testa (coordinate locali della testa, origine al centro della testa).</summary>
        public const float V84AHeadTopY = 1.05f;
        /// <summary>Raggi della testona (ellisse).</summary>
        public const float V84AHeadRx = 1.0f, V84AHeadRy = 1.05f;
        /// <summary>Centro della testa rispetto al collo (origine dei corpi) e scala della testa nella scena (ancora più testona).</summary>
        public const float V84AHeadOnNeckY = 1.0f, V84AHeadScale = 1.12f;
        /// <summary>Altezza del cuscino sopra il piano del piedistallo e della corona (centro) sopra il cuscino.</summary>
        public const float V84ACushionY = 0.2f, V84ACrownOnCushionY = 0.27f;

        static readonly Rgba V84ASkin = Rgba.Hex("FFD3AE"), V84ASkinShade = Rgba.Hex("E9A984"), V84ABeard = Rgba.Hex("F7F7FA");
        static readonly Rgba V84ABeardShade = Rgba.Hex("C9CCDA"), V84ARobe = Rgba.Hex("7A3FB5"), V84ARobeShade = Rgba.Hex("57288A");
        static readonly Rgba V84AVelvet = Rgba.Hex("D3304A"), V84AVelvetShade = Rgba.Hex("9E1F36"), V84AEyeInk = Rgba.Hex("2B2133");

        /// <summary>
        /// La testona (origine al centro, ellisse 1.0 x 1.05, cima in <see cref="V84AHeadTopY"/>): pelata lucida, ciuffi
        /// bianchi ai lati, orecchie, guance rosa, nasone tondo, baffi a manubrio e un barbone bianco che scende fin sotto
        /// il mento (copre il petto). Gli occhi sono a parte.
        /// </summary>
        public static DoodleArt V84A_Head()
        {
            var p = new DoodlePainter(-1.25f, -1.6f, 1.25f, 1.15f, 100) { Ink = 0.045f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Ellipse(s * 0.98f, 0.0f, 0.2f, 0.27f), V84ASkin);                     // orecchie
                p.Fill(Sd.Ellipse(s * 1.0f, 0.0f, 0.09f, 0.15f), V84ASkinShade);
            }
            Body(p, Sd.Ellipse(0f, 0f, V84AHeadRx, V84AHeadRy), V84ASkin, V84ASkinShade, null, -0.1f, 0.12f);
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.38f, 0.72f, 0.22f, 0.1f), -0.38f, 0.72f, 25f), Rgba.White.WithAlpha(0.6f));   // pelata lucida
            p.Fill(Sd.Circle(-0.1f, 0.86f, 0.045f), Rgba.White.WithAlpha(0.55f));
            foreach (float s in new[] { -1f, 1f })
            {
                // ciuffi bianchi sopra le orecchie
                p.Shape(Sd.Union(Sd.Circle(s * 0.86f, 0.36f, 0.14f), Sd.Circle(s * 0.95f, 0.2f, 0.13f), Sd.Circle(s * 0.8f, 0.5f, 0.1f)), V84ABeard, 0.03f);
                p.Fill(Sd.Ellipse(s * 0.55f, -0.08f, 0.16f, 0.1f), Rgba.Hex("FF9AB0", 0.8f));   // guance
            }
            // barbone: tante nuvolette bianche
            Sdf beard = Sd.Union(
                Sd.Ellipse(0f, -0.62f, 0.86f, 0.5f),
                Sd.Circle(-0.62f, -0.45f, 0.3f), Sd.Circle(0.62f, -0.45f, 0.3f),
                Sd.Circle(-0.45f, -0.95f, 0.34f), Sd.Circle(0.45f, -0.95f, 0.34f),
                Sd.Circle(0f, -1.15f, 0.36f), Sd.Circle(-0.2f, -1.32f, 0.2f), Sd.Circle(0.22f, -1.3f, 0.2f));
            Body(p, Sd.Subtract(beard, Sd.Ellipse(0f, -0.28f, 0.34f, 0.2f)), V84ABeard, V84ABeardShade, null, -0.06f, 0.06f);
            p.Line(V84ABeardShade, 0.03f, -0.3f, -0.8f, -0.24f, -1.05f);
            p.Line(V84ABeardShade, 0.03f, 0.28f, -0.78f, 0.22f, -1.0f);
            p.Line(V84ABeardShade, 0.03f, 0f, -0.9f, 0.02f, -1.2f);
            // bocca piccola (si vede appena tra i baffi e la barba)
            p.Shape(Sd.Ellipse(0f, -0.36f, 0.12f, 0.06f), Rgba.Hex("8A3040"), 0.025f);
            // baffi a manubrio
            foreach (float s in new[] { -1f, 1f })
                p.Shape(Sd.Union(Sd.Ellipse(s * 0.2f, -0.24f, 0.24f, 0.1f), Sd.Circle(s * 0.44f, -0.17f, 0.07f)), V84ABeard, 0.03f);
            // nasone
            Body(p, Sd.Ellipse(0f, -0.08f, 0.17f, 0.15f), Rgba.Hex("FFB596"), Rgba.Hex("E08A70"), null, -0.04f, 0.04f);
            return new DoodleArt("v84_a_head", p);
        }

        /// <summary>Occhi aperti un po' preoccupati (origine al centro della testa): bianchi grandi, pupille, sopracciglia bianche cespugliose.</summary>
        public static DoodleArt V84A_Eyes()
        {
            var p = new DoodlePainter(-0.65f, 0.0f, 0.65f, 0.7f, 120) { Ink = 0.03f, Wobble = 0.003f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Ellipse(s * 0.3f, 0.26f, 0.14f, 0.16f), Rgba.White);
                p.Fill(Sd.Circle(s * 0.3f + 0.02f, 0.24f, 0.065f), V84AEyeInk);
                p.Fill(Sd.Circle(s * 0.3f, 0.27f, 0.022f), Rgba.White);
                // sopracciglio cespuglioso, alzato verso il centro (perplesso)
                p.Shape(Sd.Rotate(Sd.Ellipse(s * 0.32f, 0.5f, 0.2f, 0.065f), s * 0.32f, 0.5f, s * -14f), V84ABeard, 0.025f);
            }
            return new DoodleArt("v84_a_eyes", p);
        }

        /// <summary>Occhi felici chiusi a "^" con le sopracciglia rilassate (origine al centro della testa).</summary>
        public static DoodleArt V84A_EyesHappy()
        {
            var p = new DoodlePainter(-0.65f, 0.0f, 0.65f, 0.7f, 120) { Ink = 0.03f, Wobble = 0.003f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Line(V84AEyeInk, 0.05f, s * 0.42f, 0.2f, s * 0.3f, 0.33f, s * 0.18f, 0.2f);
                p.Shape(Sd.Rotate(Sd.Ellipse(s * 0.31f, 0.5f, 0.2f, 0.065f), s * 0.31f, 0.5f, s * 8f), V84ABeard, 0.025f);
            }
            return new DoodleArt("v84_a_eyes_happy", p);
        }

        /// <summary>
        /// Il corpo seduto (origine al collo, la testa ha il centro <see cref="V84AHeadOnNeckY"/> più su): mantello viola
        /// largo col bordo di ermellino, manine sulle ginocchia, gambette e scarpe d'oro a punta (piedi in y -1.9).
        /// </summary>
        public static DoodleArt V84A_BodySit()
        {
            var p = new DoodlePainter(-1.05f, -2.05f, 1.05f, 0.2f, 100) { Ink = 0.045f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Box(s * 0.3f, -1.62f, 0.1f, 0.22f, 0.04f), Rgba.Hex("F4F0FA"));   // calze
                p.Shape(Sd.Polygon(s * 0.18f, -1.78f, s * 0.42f, -1.78f, s * 0.62f, -1.9f, s * 0.42f, -1.96f, s * 0.18f, -1.96f), Gold);   // scarpe
            }
            Body(p, Sd.Polygon(-0.55f, 0.05f, 0.55f, 0.05f, 0.92f, -1.45f, -0.92f, -1.45f), V84ARobe, V84ARobeShade);
            p.Shape(Sd.Box(0f, -1.45f, 0.92f, 0.08f, 0.04f), Rgba.White, 0.035f);   // ermellino in fondo
            for (float x = -0.75f; x < 0.8f; x += 0.25f) p.Fill(Sd.Ellipse(x, -1.45f, 0.025f, 0.04f), V84AEyeInk);
            p.Shape(Sd.Box(0f, -0.6f, 0.06f, 0.55f), Gold, 0.02f);   // bottoni d'oro
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Capsule(s * 0.5f, -0.2f, s * 0.62f, -1.0f, 0.13f), V84ARobeShade, 0.035f);   // braccia
                p.Shape(Sd.Circle(s * 0.6f, -1.08f, 0.12f), V84ASkin, 0.03f);                          // mani sulle ginocchia
            }
            p.Shape(Sd.Ellipse(0f, 0.02f, 0.7f, 0.16f), Rgba.White, 0.035f);   // collo di ermellino
            for (float x = -0.5f; x < 0.55f; x += 0.25f) p.Fill(Sd.Ellipse(x, 0.0f, 0.025f, 0.04f), V84AEyeInk);
            return new DoodleArt("v84_a_body_sit", p);
        }

        /// <summary>
        /// Il corpo in ginocchio (origine al collo): il mantello si allarga sul pavimento a campana, le manine giunte
        /// davanti, una scarpa d'oro che spunta dietro (fondo del mantello in y -1.25: poggia a terra).
        /// </summary>
        public static DoodleArt V84A_BodyKneel()
        {
            var p = new DoodlePainter(-1.35f, -1.4f, 1.35f, 0.2f, 100) { Ink = 0.045f };
            p.Shape(Sd.Polygon(0.85f, -1.12f, 1.25f, -1.08f, 1.28f, -1.25f, 0.85f, -1.27f), Gold);   // scarpa dietro
            Body(p, Sd.Union(Sd.Polygon(-0.5f, 0.05f, 0.5f, 0.05f, 1.15f, -1.2f, -1.15f, -1.2f), Sd.Ellipse(0f, -1.15f, 1.18f, 0.12f)), V84ARobe, V84ARobeShade);
            p.Shape(Sd.Box(0f, -1.2f, 1.15f, 0.08f, 0.05f), Rgba.White, 0.035f);
            for (float x = -1f; x < 1.05f; x += 0.25f) p.Fill(Sd.Ellipse(x, -1.2f, 0.025f, 0.04f), V84AEyeInk);
            foreach (float s in new[] { -1f, 1f })
                p.Shape(Sd.Capsule(s * 0.48f, -0.18f, s * 0.12f, -0.62f, 0.13f), V84ARobeShade, 0.035f);   // braccia verso il centro
            p.Shape(Sd.Union(Sd.Circle(-0.07f, -0.66f, 0.12f), Sd.Circle(0.07f, -0.66f, 0.12f)), V84ASkin, 0.03f);   // mani giunte
            p.Shape(Sd.Ellipse(0f, 0.02f, 0.68f, 0.16f), Rgba.White, 0.035f);
            for (float x = -0.5f; x < 0.55f; x += 0.25f) p.Fill(Sd.Ellipse(x, 0.0f, 0.025f, 0.04f), V84AEyeInk);
            return new DoodleArt("v84_a_body_kneel", p);
        }

        /// <summary>Il cuscino di velluto rosso (origine al centro, 1.1 x 0.36) coi bottoncini e le nappe d'oro agli angoli.</summary>
        public static DoodleArt V84A_Cushion()
        {
            var p = new DoodlePainter(-0.72f, -0.32f, 0.72f, 0.26f, 130) { Ink = 0.04f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Line(Gold, 0.03f, s * 0.5f, -0.08f, s * 0.6f, -0.18f);
                p.Shape(Sd.Polygon(s * 0.56f, -0.16f, s * 0.66f, -0.16f, s * 0.7f, -0.3f, s * 0.52f, -0.3f), Gold, 0.02f);   // nappe
            }
            Body(p, Sd.Box(0f, 0f, 0.55f, 0.16f, 0.14f), V84AVelvet, V84AVelvetShade, null, -0.04f, 0.04f);
            foreach (float x in new[] { -0.28f, 0f, 0.28f }) p.Shape(Sd.Circle(x, 0.01f, 0.035f), Gold, 0.015f);
            return new DoodleArt("v84_a_cushion", p);
        }

        /// <summary>Il piedistallo d'oro (origine al centro del piano in cima, colonnina fino a y -1.1 sul pavimento).</summary>
        public static DoodleArt V84A_Stand()
        {
            var p = new DoodlePainter(-0.5f, -1.2f, 0.5f, 0.1f, 110) { Ink = 0.04f };
            p.Shape(Sd.Box(0f, -1.04f, 0.36f, 0.07f, 0.03f), GoldShade);
            Body(p, Sd.Box(0f, -0.52f, 0.11f, 0.48f, 0.03f), Gold, GoldShade);
            p.Shape(Sd.Box(0f, -0.03f, 0.42f, 0.06f, 0.03f), Gold);
            return new DoodleArt("v84_a_stand", p);
        }
    }
}
