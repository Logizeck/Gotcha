using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.83 per il livello "The Wrong Prize" (roadmap n. 47): lo sfondo (stadio con il cielo, le
    /// tribune col pubblico e la pista), i tre gradini del podio (stesso colore per tutti: il numero è un testo del
    /// livello, scritto sulla targa bianca del davanti, e si può spostare), il corridore del giocatore a pezzi (corpo
    /// pronto, corpo a braccia alzate, testa con la fascia, tre facce: allegra, delusa, felicissima; la scritta "YOU"
    /// della pettorina è un testo del livello), il rivale tronfio (due pose: sufficienza e sbigottito), l'arbitro (fermo e
    /// col fischietto in bocca) e il bullone della coppa. La coppa è "s41e_trophy", la medaglia "v79_a_medal", lo sbuffo
    /// "puff", il bagliore "glow", la scintilla "sparkle_star". Prefisso v83_a_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV83A() => new DoodleArt[]
        {
            V83A_Bg(), V83A_Step("v83_a_step_high", V83AStepHigh), V83A_Step("v83_a_step_mid", V83AStepMid),
            V83A_Step("v83_a_step_low", V83AStepLow), V83A_RunnerBody(false), V83A_RunnerBody(true), V83A_RunnerHead(),
            V83A_RunnerFace(0), V83A_RunnerFace(1), V83A_RunnerFace(2), V83A_Rival(false), V83A_Rival(true),
            V83A_Referee(false), V83A_Referee(true), V83A_Bolt(),
        };

        /// <summary>La base del podio (y del mondo) e le altezze dei gradini (alto = centro, medio = sinistra, basso = destra).</summary>
        public const float V83AGroundY = -1.6f, V83AStepHigh = 1.9f, V83AStepMid = 1.4f, V83AStepLow = 0.9f;
        /// <summary>Metà larghezza di un gradino e distanza della targa (centro del numero) dalla cima del gradino.</summary>
        public const float V83AStepHalfW = 0.68f, V83APlaqueDown = 0.45f;
        /// <summary>Corridore (origine fra i piedi): collo, centro della pettorina, mani alzate.</summary>
        public const float V83ANeckY = 0.95f, V83ABibY = 0.72f, V83AHandsUpY = 1.62f;

        static readonly Rgba V83AStep = Rgba.Hex("4F86E8"), V83AStepShade = Rgba.Hex("3466C2"), V83AStepTop = Rgba.Hex("8DB6FF");
        static readonly Rgba V83AShirt = Rgba.Hex("FF8A2A"), V83AShirtShade = Rgba.Hex("E06A12");
        static readonly Rgba V83AShorts = Rgba.Hex("2E3F8F"), V83AShoe = Rgba.Hex("F2F2F2"), V83AShoeSole = Rgba.Hex("E8433F");
        static readonly Rgba V83ARivalShirt = Rgba.Hex("9A5BE0"), V83ARivalShade = Rgba.Hex("7440B8");
        static readonly Rgba V83AHairCol = Rgba.Hex("6B3E22");

        static float V83A_Clamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        /// <summary>
        /// Lo sfondo (coordinate del mondo): cielo azzurro sfumato, due nuvolette, le tribune col pubblico (teste color
        /// pastello, poco contrasto: davanti ci stanno il podio e i corridori), la ringhiera e la pista rossa con le corsie
        /// bianche da y <see cref="V83AGroundY"/> in giù, l'ombra del podio.
        /// </summary>
        public static DoodleArt V83A_Bg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            // cielo
            p.FillFunc(Sd.Box(0f, 2.6f, 3.3f, 2.7f), (x, y) => Rgba.Lerp(Rgba.Hex("BFE6FF"), Rgba.Hex("7CC4F5"), V83A_Clamp((y - 0.8f) / 4.2f)));
            foreach (float[] c in new[] { new[] { -1.8f, 2.4f }, new[] { 1.6f, 2.0f } })
                p.Shape(Sd.Union(Sd.Ellipse(c[0], c[1], 0.45f, 0.18f), Sd.Circle(c[0] - 0.15f, c[1] + 0.12f, 0.18f), Sd.Circle(c[0] + 0.16f, c[1] + 0.1f, 0.15f)), Rgba.White.WithAlpha(0.9f), 0.03f);
            // tribune: gradoni e pubblico
            Sdf stands = Sd.Box(0f, -0.25f, 3.3f, 1.5f);
            p.FillFunc(stands, (x, y) => (int)Math.Floor((y + 10f) * 2.5f) % 2 == 0 ? Rgba.Hex("D9D2E8") : Rgba.Hex("CDC4DF"));
            p.Shape(Sd.Box(0f, 1.27f, 3.3f, 0.05f), Rgba.Hex("A99BC8"), 0.03f);
            Rgba[] crowd = { Rgba.Hex("F7B7A3"), Rgba.Hex("F3D9A4"), Rgba.Hex("B7D7F0"), Rgba.Hex("C9E7B9"), Rgba.Hex("E9C2E6"), Rgba.Hex("F4E3B5") };
            for (int row = 0; row < 6; row++)
            {
                float y = 1.0f - row * 0.4f;
                for (int i = 0; i < 14; i++)
                {
                    float x = -3.05f + i * 0.47f + (row % 2) * 0.23f;
                    Rgba c = crowd[(i * 7 + row * 3) % crowd.Length];
                    p.Shape(Sd.Union(Sd.Circle(x, y, 0.11f), Sd.Box(x, y - 0.17f, 0.13f, 0.07f, 0.05f)), c.WithAlpha(0.85f), 0.02f);
                }
            }
            // ringhiera
            p.Shape(Sd.Box(0f, -1.48f, 3.3f, 0.12f), Rgba.Hex("6B5A90"), 0.035f);
            // pista con le corsie
            p.FillFunc(Sd.Box(0f, -3.6f, 3.3f, 2.0f), (x, y) => Rgba.Lerp(Rgba.Hex("D9573E"), Rgba.Hex("C2452F"), V83A_Clamp((-1.6f - y) / 3.5f)));
            foreach (float ly in new[] { -2.25f, -3.05f, -3.95f }) p.Line(Rgba.White.WithAlpha(0.85f), 0.04f, -3.3f, ly, 3.3f, ly);
            p.Line(Rgba.White.WithAlpha(0.8f), 0.05f, -3.3f, -1.62f, 3.3f, -1.62f);
            // ombra del podio
            p.Fill(Sd.Ellipse(0f, -1.68f, 2.4f, 0.14f), Rgba.Hex("2B2133", 0.25f), false);
            return new DoodleArt("v83_a_bg", p);
        }

        /// <summary>
        /// Un gradino (origine al centro della base; largo 2 × <see cref="V83AStepHalfW"/>, alto <paramref name="h"/>): blocco
        /// azzurro con la cima più chiara e la targa bianca rotonda sul davanti (centro a <see cref="V83APlaqueDown"/> sotto
        /// la cima) dove il livello scrive il numero.
        /// </summary>
        public static DoodleArt V83A_Step(string name, float h)
        {
            float hw = V83AStepHalfW;
            var p = new DoodlePainter(-hw - 0.08f, -0.06f, hw + 0.08f, h + 0.08f, 90) { Ink = 0.045f };
            Body(p, Sd.Box(0f, h / 2f, hw, h / 2f, 0.04f), V83AStep, V83AStepShade);
            p.Shape(Sd.Box(0f, h - 0.07f, hw, 0.07f, 0.03f), V83AStepTop, 0.035f);
            // targa per il numero
            float py = h - V83APlaqueDown;
            p.Shape(Sd.Circle(0f, py, 0.36f), Rgba.Hex("FFFDF4"), 0.035f);
            p.Shape(Sd.Circle(0f, py, 0.31f), Rgba.Hex("FFFDF4"), 0.012f);
            // righine decorative sul fondo
            p.Line(V83AStepShade, 0.03f, -hw + 0.12f, 0.16f, hw - 0.12f, 0.16f);
            return new DoodleArt(name, p);
        }

        /// <summary>
        /// Il corpo del corridore (origine fra i piedi; collo in (0, <see cref="V83ANeckY"/>)): scarpe da corsa, gambe,
        /// pantaloncini blu, maglietta arancione con la pettorina bianca (centro (0, <see cref="V83ABibY"/>), la scritta "YOU"
        /// la mette il livello). Pronto: braccia piegate pronte a scattare; esultanza: braccia alzate (mani in
        /// (±0.28, <see cref="V83AHandsUpY"/>), lì il livello posa la coppa).
        /// </summary>
        public static DoodleArt V83A_RunnerBody(bool cheer)
        {
            var p = new DoodlePainter(-0.62f, -0.06f, 0.62f, cheer ? 1.78f : 1.08f, 110) { Ink = 0.045f };
            Rgba skin = Skin;
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Capsule(s * 0.13f, 0.12f, s * 0.15f, 0.42f, 0.06f), skin);
                p.Shape(Sd.Ellipse(s * 0.17f + s * 0.04f, 0.07f, 0.15f, 0.07f), V83AShoe);
                p.Line(V83AShoeSole, 0.035f, s * 0.07f, 0.02f, s * 0.34f, 0.02f);
            }
            // braccia (dietro la maglietta)
            foreach (float s in new[] { -1f, 1f })
            {
                if (cheer)
                {
                    p.Shape(Sd.Polyline(0.07f, s * 0.24f, 0.86f, s * 0.36f, 1.25f, s * 0.28f, V83AHandsUpY - 0.06f), skin);
                    p.Shape(Sd.Circle(s * 0.28f, V83AHandsUpY, 0.08f), skin);
                }
                else
                {
                    p.Shape(Sd.Polyline(0.07f, s * 0.24f, 0.88f, s * 0.42f, 0.66f, s * 0.3f, 0.5f), skin);
                    p.Shape(Sd.Circle(s * 0.3f, 0.48f, 0.075f), skin);
                }
            }
            // pantaloncini
            p.Shape(Sd.Polygon(-0.27f, 0.56f, 0.27f, 0.56f, 0.3f, 0.36f, 0.03f, 0.36f, 0f, 0.44f, -0.03f, 0.36f, -0.3f, 0.36f), V83AShorts);
            // maglietta con la pettorina
            Sdf shirt = Sd.Box(0f, 0.76f, 0.26f, 0.22f, 0.08f);
            Body(p, shirt, V83AShirt, V83AShirtShade);
            p.Shape(Sd.Box(0f, V83ABibY, 0.19f, 0.12f, 0.03f), Rgba.White, 0.03f);
            foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Circle(s * 0.15f, V83ABibY + 0.09f, 0.018f), Rgba.Hex("B0B0B8"), false);
            // collo
            p.Shape(Sd.Box(0f, 0.97f, 0.07f, 0.05f), SkinShade, 0.03f);
            return new DoodleArt(cheer ? "v83_a_runner_cheer" : "v83_a_runner_body", p);
        }

        /// <summary>La testa del corridore (origine al collo; centro (0, 0.3), raggio 0.28): capelli a spazzola e fascia rossa.</summary>
        public static DoodleArt V83A_RunnerHead()
        {
            var p = new DoodlePainter(-0.4f, -0.06f, 0.4f, 0.68f, 120) { Ink = 0.045f };
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Circle(s * 0.28f, 0.28f, 0.07f), Skin);
            Body(p, Sd.Circle(0f, 0.3f, 0.28f), Skin, SkinShade);
            Sdf hair = Sd.Intersect(Sd.Circle(0f, 0.32f, 0.29f), Sd.Box(0f, 0.55f, 0.32f, 0.12f));
            p.Shape(Sd.Union(hair, Sd.Polygon(-0.1f, 0.6f, -0.02f, 0.66f, 0.02f, 0.58f, 0.12f, 0.64f, 0.14f, 0.55f)), V83AHairCol, 0.035f);
            p.Shape(Sd.Box(0f, 0.45f, 0.29f, 0.045f, 0.02f), Rgba.Hex("E8433F"), 0.03f);
            p.Shape(Sd.Polygon(0.26f, 0.46f, 0.38f, 0.5f, 0.36f, 0.38f), Rgba.Hex("E8433F"), 0.025f);
            return new DoodleArt("v83_a_runner_head", p);
        }

        /// <summary>Le facce del corridore (stessa origine della testa): 0 allegra, 1 delusa (sopracciglia all'insù, bocca all'ingiù), 2 felicissima (occhi chiusi a mezzaluna, bocca spalancata).</summary>
        public static DoodleArt V83A_RunnerFace(int mood)
        {
            var p = new DoodlePainter(-0.3f, 0.02f, 0.3f, 0.44f, 140) { Ink = 0.03f };
            foreach (float s in new[] { -1f, 1f })
            {
                float ex = s * 0.1f, ey = 0.3f;
                if (mood == 2) p.InkLine(0.03f, ex - 0.05f, ey - 0.01f, ex, ey + 0.04f, ex + 0.05f, ey - 0.01f);
                else
                {
                    p.Shape(Sd.Ellipse(ex, ey, 0.045f, 0.055f), Rgba.White, 0.02f);
                    p.Fill(Sd.Circle(ex, ey - 0.01f, 0.022f), Rgba.Hex("2B2233"));
                }
                if (mood == 1) p.InkLine(0.025f, ex - s * 0.06f, ey + 0.06f, ex + s * 0.05f, ey + 0.1f);
                p.Fill(Sd.Ellipse(s * 0.18f, 0.2f, 0.05f, 0.03f), Rgba.Hex("FF9AA2", 0.6f), false);
            }
            switch (mood)
            {
                case 0: p.InkLine(0.03f, -0.09f, 0.18f, 0f, 0.13f, 0.09f, 0.18f); break;
                case 1: p.InkLine(0.03f, -0.07f, 0.12f, 0f, 0.16f, 0.07f, 0.12f); break;
                default:
                    {
                        Sdf mouth = Sd.Intersect(Sd.Ellipse(0f, 0.17f, 0.11f, 0.09f), Sd.Box(0f, 0.12f, 0.12f, 0.06f));
                        p.Shape(mouth, Rgba.Hex("6A1E2A"), 0.03f);
                        p.Fill(Sd.Intersect(mouth, Sd.Ellipse(0f, 0.08f, 0.07f, 0.04f)), Rgba.Hex("F06A86"));
                        break;
                    }
            }
            string[] names = { "v83_a_runner_face_happy", "v83_a_runner_face_sad", "v83_a_runner_face_joy" };
            return new DoodleArt(names[Math.Max(0, Math.Min(2, mood))], p);
        }

        /// <summary>
        /// Il rivale tronfio (origine fra i piedi, alto 1.5): maglietta viola, braccia conserte, occhiali da sole sulla
        /// fronte, sorrisetto e naso all'insù; sbigottito: occhi sgranati, bocca a O, occhiali storti, braccia aperte.
        /// </summary>
        public static DoodleArt V83A_Rival(bool shock)
        {
            var p = new DoodlePainter(-0.66f, -0.06f, 0.66f, 1.6f, 110) { Ink = 0.045f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Capsule(s * 0.13f, 0.12f, s * 0.14f, 0.42f, 0.06f), Skin);
                p.Shape(Sd.Ellipse(s * 0.21f, 0.07f, 0.15f, 0.07f), Rgba.Hex("3A2E2A"));
            }
            if (shock)
                foreach (float s in new[] { -1f, 1f })
                {
                    p.Shape(Sd.Polyline(0.07f, s * 0.24f, 0.88f, s * 0.48f, 0.9f, s * 0.58f, 1.1f), Skin);
                    p.Shape(Sd.Circle(s * 0.58f, 1.14f, 0.075f), Skin);
                }
            p.Shape(Sd.Box(0f, 0.46f, 0.28f, 0.1f, 0.03f), Rgba.Hex("2B2B3A"));
            Body(p, Sd.Box(0f, 0.76f, 0.27f, 0.23f, 0.08f), V83ARivalShirt, V83ARivalShade);
            if (!shock)
            {
                // braccia conserte
                p.Shape(Sd.Capsule(-0.25f, 0.74f, 0.25f, 0.7f, 0.075f), Skin);
                p.Line(SkinShade, 0.03f, -0.05f, 0.76f, 0.1f, 0.67f);
            }
            p.Shape(Sd.Box(0f, 0.98f, 0.07f, 0.05f), SkinShade, 0.03f);
            // testa
            float hy = 1.27f;
            Body(p, Sd.Circle(0f, hy, 0.28f), Skin, SkinShade);
            p.Shape(Sd.Intersect(Sd.Circle(0f, hy + 0.03f, 0.29f), Sd.Box(0.04f, hy + 0.24f, 0.32f, 0.1f)), Rgba.Hex("2B2233"), 0.035f);
            // occhiali da sole sulla fronte
            float gr = shock ? -14f : 0f;
            Sdf shades = Sd.Rotate(Sd.Union(Sd.Box(-0.11f, hy + 0.16f, 0.09f, 0.045f, 0.03f), Sd.Box(0.11f, hy + 0.16f, 0.09f, 0.045f, 0.03f)), 0f, hy + 0.16f, gr);
            p.Shape(shades, Rgba.Hex("1E1E2A"), 0.025f);
            foreach (float s in new[] { -1f, 1f })
            {
                float ex = s * 0.1f, ey = hy + 0.01f;
                if (shock)
                {
                    p.Shape(Sd.Circle(ex, ey, 0.06f), Rgba.White, 0.02f);
                    p.Fill(Sd.Circle(ex, ey, 0.018f), Rgba.Hex("2B2233"));
                }
                else p.InkLine(0.03f, ex - 0.05f, ey, ex + 0.05f, ey + 0.01f);
            }
            if (shock) p.Shape(Sd.Ellipse(0f, hy - 0.14f, 0.05f, 0.07f), Rgba.Hex("6A1E2A"), 0.03f);
            else p.InkLine(0.03f, -0.08f, hy - 0.12f, 0.02f, hy - 0.14f, 0.1f, hy - 0.09f);
            return new DoodleArt(shock ? "v83_a_rival_shock" : "v83_a_rival_smug", p);
        }

        /// <summary>
        /// L'arbitro (origine fra i piedi, alto 1.5): maglietta a righe bianche e nere, berretto nero, fischietto al collo;
        /// col fischio: guance gonfie, fischietto in bocca e braccio teso che indica.
        /// </summary>
        public static DoodleArt V83A_Referee(bool blow)
        {
            var p = new DoodlePainter(-0.62f, -0.06f, blow ? 0.95f : 0.62f, 1.62f, 110) { Ink = 0.045f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Box(s * 0.13f, 0.27f, 0.07f, 0.2f), Rgba.Hex("1E1E2A"));
                p.Shape(Sd.Ellipse(s * 0.19f, 0.06f, 0.14f, 0.06f), Rgba.Hex("1E1E2A"));
            }
            // braccia
            p.Shape(Sd.Polyline(0.075f, -0.25f, 0.9f, -0.38f, 0.66f, -0.3f, 0.5f), Skin);
            if (blow)
            {
                p.Shape(Sd.Polyline(0.075f, 0.25f, 0.92f, 0.85f, 1.0f), Skin);
                p.Shape(Sd.Circle(0.86f, 1.0f, 0.07f), Skin);
            }
            else p.Shape(Sd.Polyline(0.075f, 0.25f, 0.9f, 0.38f, 0.66f, 0.3f, 0.5f), Skin);
            // maglietta a righe
            Sdf shirt = Sd.Box(0f, 0.74f, 0.27f, 0.27f, 0.07f);
            p.FillFunc(shirt, (x, y) => (int)Math.Floor((x + 10f) * 9f) % 2 == 0 ? Rgba.White : Rgba.Hex("2B2B3A"));
            p.Shape(shirt, Rgba.Clear);
            p.Shape(Sd.Box(0f, 1.02f, 0.07f, 0.05f), SkinShade, 0.03f);
            float hy = 1.28f;
            Body(p, Sd.Circle(0f, hy, 0.27f), Skin, SkinShade);
            // berretto
            p.Shape(Sd.Intersect(Sd.Circle(0f, hy + 0.04f, 0.28f), Sd.Box(0f, hy + 0.22f, 0.3f, 0.1f)), Rgba.Hex("1E1E2A"), 0.035f);
            p.Shape(Sd.Box(0.22f, hy + 0.13f, 0.16f, 0.03f, 0.02f), Rgba.Hex("1E1E2A"), 0.025f);
            // baffi e occhi
            foreach (float s in new[] { -1f, 1f })
            {
                if (blow) p.InkLine(0.03f, s * 0.14f, hy + 0.06f, s * 0.05f, hy + 0.03f);
                else p.Fill(Sd.Circle(s * 0.1f, hy + 0.03f, 0.028f), Rgba.Hex("2B2233"));
                p.InkLine(0.025f, s * 0.15f, hy + 0.12f, s * 0.04f, hy + 0.11f);
            }
            p.Shape(Sd.Union(Sd.Ellipse(-0.07f, hy - 0.09f, 0.08f, 0.035f), Sd.Ellipse(0.07f, hy - 0.09f, 0.08f, 0.035f)), Rgba.Hex("4A3020"), 0.02f);
            Rgba whistle = Rgba.Hex("C8CCD8");
            if (blow)
            {
                // guance gonfie e fischietto in bocca
                foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Circle(s * 0.17f, hy - 0.1f, 0.09f), Rgba.Hex("FFB59A"), 0.02f);
                p.Shape(Sd.Box(0.08f, hy - 0.16f, 0.09f, 0.04f, 0.02f), whistle, 0.025f);
                p.Shape(Sd.Circle(0.18f, hy - 0.18f, 0.06f), whistle, 0.025f);
            }
            else
            {
                p.Line(Rgba.Hex("E8433F"), 0.02f, -0.07f, 1.04f, 0f, 0.86f, 0.07f, 1.04f);
                p.Shape(Sd.Circle(0f, 0.82f, 0.06f), whistle, 0.025f);
                p.InkLine(0.025f, -0.06f, hy - 0.15f, 0.06f, hy - 0.15f);
            }
            return new DoodleArt(blow ? "v83_a_referee_blow" : "v83_a_referee", p);
        }

        /// <summary>Il bullone a testa esagonale che tiene la coppa avvitata (centro 0,0, raggio 0.08).</summary>
        public static DoodleArt V83A_Bolt()
        {
            var p = new DoodlePainter(-0.11f, -0.11f, 0.11f, 0.11f, 160) { Ink = 0.025f };
            var pts = new List<float>();
            for (int i = 0; i < 6; i++)
            {
                double a = Math.PI / 6 + i * Math.PI / 3;
                pts.Add(0.08f * (float)Math.Cos(a));
                pts.Add(0.08f * (float)Math.Sin(a));
            }
            p.Shape(Sd.Polygon(pts.ToArray()), Rgba.Hex("B8BEC9"));
            p.InkLine(0.02f, -0.04f, 0f, 0.04f, 0f);
            return new DoodleArt("v83_a_bolt", p);
        }
    }
}
