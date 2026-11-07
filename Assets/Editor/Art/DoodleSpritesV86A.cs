using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.86 per il livello 101 della roadmap "The Upside-Down Rain": lo sfondo (strada sotto la
    /// pioggia: cielo grigio-blu, palazzi lontani con le finestre accese, lampione, marciapiede e asfalto bagnato), la
    /// nuvola nera imbronciata, l'ombrello aperto PIENO DI BUCHI (i buchi sono trasparenti: dietro si vede lo sfondo;
    /// origine alla mano che lo tiene, così inclinarlo lo fa ruotare attorno alla mano), l'ombrello chiuso (stessa
    /// origine), l'omino (origine ai piedi, braccio destro alzato che tiene il manico; la faccia è a parte), le tre facce
    /// (triste, che batte i denti, felice; origine al centro della testa) e le gocce che gli colano addosso (il livello ne
    /// regola la trasparenza col bagnato). Riusati: "s41d_raindrop" (gocce), "splash", "puddle", "sun", "glow".
    /// Prefisso v86_a_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV86A() => new DoodleArt[]
        {
            V86A_Bg(), V86A_Cloud(), V86A_Umbrella(), V86A_UmbrellaFolded(), V86A_Man(),
            V86A_Face(0), V86A_Face(1), V86A_Face(2), V86A_Drips(),
        };

        // misure condivise col builder (coordinate del mondo e locali)
        /// <summary>Quota del marciapiede (piedi dell'omino) e posizione dei suoi piedi.</summary>
        public const float V86AGroundY = -2.7f, V86AManX = -0.45f;
        /// <summary>Centro della testa (locale all'omino, origine ai piedi) e raggio.</summary>
        public const float V86AHeadY = 1.55f, V86AHeadR = 0.38f;
        /// <summary>Mano che tiene il manico (locale all'omino): è l'origine dell'ombrello.</summary>
        public const float V86AHandX = 0.45f, V86AHandY = 1.37f;
        /// <summary>Tela dell'ombrello (locale all'ombrello): bordo in basso, mezza larghezza, altezza della cupola.</summary>
        public const float V86ARimY = 1.45f, V86ACanopyHx = 1.35f, V86ACanopyH = 0.9f;
        /// <summary>Centro della nuvola nel mondo e quota della sua pancia (da dove parte la pioggia).</summary>
        public const float V86ACloudX = 0f, V86ACloudY = 2.35f, V86ACloudBottom = 1.8f;

        /// <summary>I buchi dell'ombrello (x, y, raggio; locali all'ombrello). I primi tre stanno sopra la testa.</summary>
        public static readonly float[] V86AHoles =
        {
            -0.5f, 1.75f, 0.11f,
            -0.2f, 2.05f, 0.1f,
            -0.88f, 1.66f, 0.1f,
            0.35f, 1.72f, 0.12f,
            0.78f, 1.95f, 0.09f,
            1.05f, 1.6f, 0.1f,
            0.1f, 2.22f, 0.08f,
            -0.62f, 2.08f, 0.08f,
        };

        static readonly Rgba V86ASkin = Rgba.Hex("F2C49B"), V86ASkinShade = Rgba.Hex("D9A57B");
        static readonly Rgba V86ACoat = Rgba.Hex("E8B23A"), V86ACoatShade = Rgba.Hex("C28F22");
        static readonly Rgba V86APants = Rgba.Hex("4A5A7A"), V86APantsShade = Rgba.Hex("37445E");
        static readonly Rgba V86ACanopyA = Rgba.Hex("E8433F"), V86ACanopyB = Rgba.Hex("F2EAD8"), V86ACanopyShade = Rgba.Hex("B9302E");
        static readonly Rgba V86AWater = Rgba.Hex("8FD0FF");

        static float V86A_Clamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        /// <summary>
        /// Lo sfondo (coordinate del mondo): cielo grigio-blu (più scuro in alto), palazzi lontani con qualche finestra
        /// accesa, il lampione a destra, il marciapiede (fino a y -2.7) col bordo e la strada bagnata con i riflessi.
        /// </summary>
        public static DoodleArt V86A_Bg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0f, 1.3f, 3.3f, 4.0f), (x, y) => Rgba.Lerp(Rgba.Hex("7D8BA3"), Rgba.Hex("3E4A63"), V86A_Clamp((y + 2.5f) / 7.5f)));
            // palazzi lontani
            float[] bx = { -2.75f, -1.95f, -1.05f, -0.2f, 0.75f, 1.65f, 2.55f };
            float[] bh = { 0.9f, 1.6f, 0.6f, 1.25f, 0.8f, 1.5f, 1.0f };
            for (int i = 0; i < bx.Length; i++)
            {
                float top = -1.6f + bh[i];
                p.Shape(Sd.Box(bx[i], (top - 2.7f) * 0.5f, 0.42f, (top + 2.7f) * 0.5f), Rgba.Hex(i % 2 == 0 ? "5A6680" : "526079"), 0.03f);
                for (float wy = top - 0.3f; wy > -2.45f; wy -= 0.42f)
                    for (int c = -1; c <= 1; c += 2)
                    {
                        bool lit = Noise.Value(bx[i] * 7f + c, wy * 5f) > 0.62f;
                        p.Fill(Sd.Box(bx[i] + c * 0.17f, wy, 0.08f, 0.1f), lit ? Rgba.Hex("F7D774", 0.85f) : Rgba.Hex("3E4A63", 0.8f), false);
                    }
            }
            // velo di pioggia lontana (righe oblique leggere)
            p.Hatch(Sd.Box(0f, 1.0f, 3.3f, 3.7f), Rgba.White.WithAlpha(0.07f), 0.35f, 0.012f, 70f);
            // lampione a destra
            Body(p, Sd.Box(2.35f, -1.1f, 0.06f, 1.6f, 0.03f), Rgba.Hex("3B4252"), Rgba.Hex("262B36"));
            p.Shape(Sd.Capsule(2.35f, 0.5f, 1.95f, 0.62f, 0.045f), Rgba.Hex("3B4252"), 0.03f);
            p.Shape(Sd.Polygon(1.72f, 0.58f, 2.08f, 0.58f, 2.0f, 0.42f, 1.8f, 0.42f), Rgba.Hex("F7D774"), 0.03f);
            p.Fill(Sd.Polygon(1.8f, 0.42f, 2.0f, 0.42f, 2.35f, -2.7f, 1.45f, -2.7f), Rgba.Hex("F7D774", 0.08f), false);
            // marciapiede
            p.FillFunc(Sd.Box(0f, -2.95f, 3.3f, 0.25f), (x, y) => (int)Math.Floor((x + 10f) / 0.8f) % 2 == 0 ? Rgba.Hex("9AA0AC") : Rgba.Hex("8D94A1"));
            p.Shape(Sd.Box(0f, -3.22f, 3.3f, 0.06f), Rgba.Hex("B8BEC8"), 0.03f);
            // strada bagnata con i riflessi
            p.FillFunc(Sd.Box(0f, -4.3f, 3.3f, 1.05f), (x, y) => Rgba.Lerp(Rgba.Hex("3A4150"), Rgba.Hex("2B303C"), V86A_Clamp((-3.3f - y) / 1.9f)));
            foreach (float s in new[] { -1.6f, 0f, 1.6f }) p.Fill(Sd.Box(s, -4.0f, 0.38f, 0.05f, 0.04f), Rgba.Hex("E8E2C8", 0.6f), false);
            p.Fill(Sd.Ellipse(1.9f, -3.75f, 0.35f, 0.06f), Rgba.Hex("F7D774", 0.25f), false);
            p.Fill(Sd.Ellipse(-1.9f, -4.6f, 0.5f, 0.07f), Rgba.Hex("8FD0FF", 0.18f), false);
            return new DoodleArt("v86_a_bg", p);
        }

        /// <summary>La nuvola nera imbronciata (centro 0,0, larga 3.9, alta 1.25; la pancia piatta a y -0.5).</summary>
        public static DoodleArt V86A_Cloud()
        {
            var p = new DoodlePainter(-2.05f, -0.68f, 2.05f, 0.72f, 100) { Ink = 0.05f };
            Sdf cloud = Sd.Union(
                Sd.Circle(-1.3f, -0.1f, 0.42f), Sd.Circle(-0.6f, 0.2f, 0.5f), Sd.Circle(0.2f, 0.25f, 0.42f),
                Sd.Circle(0.95f, 0.12f, 0.48f), Sd.Circle(1.55f, -0.12f, 0.36f), Sd.Box(0f, -0.3f, 1.6f, 0.22f, 0.2f));
            Body(p, cloud, Rgba.Hex("5B6175"), Rgba.Hex("434859"), null, -0.08f, 0.1f);
            p.Fill(Sd.Ellipse(-0.75f, 0.42f, 0.28f, 0.08f), Rgba.White.WithAlpha(0.18f));
            // faccia imbronciata
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Ellipse(s * 0.32f, 0.02f, 0.06f, 0.08f), DoodlePainter.DefaultInk);
                p.InkLine(0.04f, s * 0.48f, 0.2f, s * 0.2f, 0.12f);   // sopracciglia aggrottate
            }
            p.InkLine(0.035f, -0.18f, -0.25f, -0.06f, -0.2f, 0.06f, -0.2f, 0.18f, -0.25f);
            return new DoodleArt("v86_a_cloud", p);
        }

        static Sdf V86A_CanopyShape()
        {
            // cupola: mezza ellisse sopra il bordo, con il bordo a festoni
            Sdf dome = Sd.Intersect(Sd.Ellipse(0f, V86ARimY, V86ACanopyHx, V86ACanopyH), Sd.Box(0f, V86ARimY + 0.5f, 1.5f, 0.5f));
            Sdf rim = dome;
            for (int i = 0; i < 6; i++)
            {
                float cx = -V86ACanopyHx + 0.225f + i * 0.45f;
                rim = Sd.Subtract(rim, Sd.Circle(cx, V86ARimY - 0.02f, 0.16f));
            }
            return Sd.Union(rim, Sd.Box(0f, V86ARimY + 0.08f, 0.04f, 0.1f));
        }

        static Sdf V86A_Holes()
        {
            var list = new List<Sdf>();
            for (int i = 0; i < V86AHoles.Length / 3; i++)
            {
                float hx = V86AHoles[i * 3], hy = V86AHoles[i * 3 + 1], r = V86AHoles[i * 3 + 2];
                // buco un po' sfrangiato: cerchio più un'ellisse storta
                list.Add(Sd.Union(Sd.Circle(hx, hy, r), Sd.Rotate(Sd.Ellipse(hx + r * 0.3f, hy - r * 0.2f, r * 0.8f, r * 0.5f), hx, hy, 30f * i)));
            }
            return Sd.Union(list.ToArray());
        }

        /// <summary>
        /// L'ombrello aperto bucato (origine alla mano): manico a uncino sotto la mano, asta dritta, cupola a spicchi rossi e
        /// panna fra y 1.45 e 2.35 con i buchi <see cref="V86AHoles"/> trasparenti e il puntale in cima.
        /// </summary>
        public static DoodleArt V86A_Umbrella()
        {
            var p = new DoodlePainter(-1.45f, -0.45f, 1.45f, 2.55f, 100) { Ink = 0.045f };
            // manico e asta
            p.Shape(Sd.Polyline(0.045f, 0f, 1.5f, 0f, -0.15f), Rgba.Hex("6B4129"), 0.03f);
            p.Shape(Sd.Subtract(Sd.Circle(-0.13f, -0.17f, 0.17f), Sd.Union(Sd.Circle(-0.13f, -0.17f, 0.08f), Sd.Box(-0.13f, 0.0f, 0.2f, 0.17f))), Rgba.Hex("6B4129"), 0.03f);
            Sdf canopy = Sd.Subtract(V86A_CanopyShape(), V86A_Holes());
            p.FillFunc(canopy, (x, y) =>
            {
                double a = Math.Atan2(y - (V86ARimY - 0.6f), x);
                int k = (int)Math.Floor(a / (Math.PI / 7.0));
                Rgba c = k % 2 == 0 ? V86ACanopyA : V86ACanopyB;
                return Rgba.Lerp(c, V86ACanopyShade, V86A_Clamp((x - 0.3f) / 1.2f) * 0.35f);
            });
            // stecche
            for (int i = 1; i < 7; i++)
            {
                double a = i * Math.PI / 7.0;
                float ex = (float)Math.Cos(a) * V86ACanopyHx * 0.98f, ey = V86ARimY + (float)Math.Sin(a) * 0.12f;
                p.Line(Rgba.Hex("2B2233", 0.35f), 0.018f, 0f, V86ARimY + V86ACanopyH - 0.02f, ex, ey);
            }
            p.Shape(canopy, Rgba.Clear);
            // bordi strappati dei buchi (un filo d'inchiostro in più)
            p.Shape(V86A_Holes(), Rgba.Clear, 0.03f);
            p.Shape(Sd.Circle(0f, V86ARimY + V86ACanopyH + 0.06f, 0.06f), Rgba.Hex("F2D24B"), 0.025f);
            return new DoodleArt("v86_a_umbrella", p);
        }

        /// <summary>L'ombrello chiuso (origine alla mano): stesso manico, la tela arrotolata stretta lungo l'asta, il laccetto.</summary>
        public static DoodleArt V86A_UmbrellaFolded()
        {
            var p = new DoodlePainter(-0.4f, -0.45f, 0.4f, 2.45f, 100) { Ink = 0.045f };
            p.Shape(Sd.Polyline(0.045f, 0f, 2.3f, 0f, -0.15f), Rgba.Hex("6B4129"), 0.03f);
            p.Shape(Sd.Subtract(Sd.Circle(-0.13f, -0.17f, 0.17f), Sd.Union(Sd.Circle(-0.13f, -0.17f, 0.08f), Sd.Box(-0.13f, 0.0f, 0.2f, 0.17f))), Rgba.Hex("6B4129"), 0.03f);
            Sdf roll = Sd.Polygon(-0.05f, 0.55f, 0.05f, 0.55f, 0.24f, 1.25f, 0.14f, 2.05f, 0f, 2.22f, -0.14f, 2.05f, -0.24f, 1.25f);
            Body(p, roll, V86ACanopyA, V86ACanopyShade);
            p.Line(V86ACanopyB, 0.04f, -0.12f, 0.95f, 0.06f, 1.6f, 0.02f, 2.0f);
            p.Shape(Sd.Box(0f, 1.3f, 0.25f, 0.05f, 0.03f), Rgba.Hex("F2EAD8"), 0.025f);
            p.Shape(Sd.Circle(0f, 2.3f, 0.06f), Rgba.Hex("F2D24B"), 0.025f);
            return new DoodleArt("v86_a_umbrella_folded", p);
        }

        /// <summary>
        /// L'omino (origine ai piedi): impermeabile giallo, pantaloni blu, scarpe, braccio sinistro lungo il fianco e
        /// braccio destro alzato fino alla mano che stringe il manico (<see cref="V86AHandX"/>, <see cref="V86AHandY"/>);
        /// la testa senza faccia (le facce sono a parte), capelli bagnati appiccicati.
        /// </summary>
        public static DoodleArt V86A_Man()
        {
            var p = new DoodlePainter(-0.6f, -0.08f, 0.62f, 2.0f, 110) { Ink = 0.045f };
            // gambe e scarpe
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Box(s * 0.13f, 0.3f, 0.1f, 0.26f, 0.04f), V86APants, V86APantsShade);
                p.Shape(Sd.Box(s * 0.16f, 0.04f, 0.15f, 0.06f, 0.05f), Rgba.Hex("3A2B2B"));
            }
            // braccio sinistro (a destra di chi guarda è quello alzato)
            p.Shape(Sd.Capsule(-0.3f, 1.05f, -0.4f, 0.62f, 0.075f), V86ACoat);
            p.Shape(Sd.Circle(-0.41f, 0.56f, 0.07f), V86ASkin);
            // impermeabile
            Sdf coat = Sd.Polygon(-0.32f, 1.12f, 0.32f, 1.12f, 0.38f, 0.5f, -0.38f, 0.5f);
            Body(p, Sd.Grow(coat, 0.03f), V86ACoat, V86ACoatShade);
            p.InkLine(0.02f, 0f, 1.1f, 0f, 0.55f);
            foreach (float by in new[] { 0.95f, 0.78f, 0.62f }) p.Shape(Sd.Circle(0.06f, by, 0.025f), Rgba.Hex("6B4129"), 0.012f);
            p.Shape(Sd.Polygon(-0.16f, 1.14f, 0f, 1.0f, 0.16f, 1.14f), Rgba.Hex("F2E5B5"), 0.02f);
            // braccio destro alzato fino alla mano sul manico
            p.Shape(Sd.Capsule(0.28f, 1.05f, V86AHandX, V86AHandY - 0.02f, 0.075f), V86ACoat);
            p.Shape(Sd.Circle(V86AHandX, V86AHandY, 0.08f), V86ASkin);
            // testa (senza faccia)
            Body(p, Sd.Circle(0f, V86AHeadY, V86AHeadR), V86ASkin, V86ASkinShade);
            // capelli bagnati appiccicati
            Sdf hair = Sd.Intersect(Sd.Circle(0f, V86AHeadY + 0.02f, V86AHeadR + 0.02f), Sd.Box(0f, V86AHeadY + 0.33f, 0.45f, 0.14f));
            p.Shape(hair, Rgba.Hex("6B4129"), 0.03f);
            foreach (float s in new[] { -0.2f, -0.05f, 0.12f, 0.26f })
                p.Shape(Sd.Polygon(s - 0.06f, V86AHeadY + 0.22f, s + 0.06f, V86AHeadY + 0.22f, s + 0.01f, V86AHeadY + 0.1f), Rgba.Hex("6B4129"), 0.02f);
            return new DoodleArt("v86_a_man", p);
        }

        /// <summary>Le facce (origine al centro della testa): 0 triste, 1 che batte i denti (labbra blu), 2 felice.</summary>
        public static DoodleArt V86A_Face(int kind)
        {
            var p = new DoodlePainter(-0.36f, -0.32f, 0.36f, 0.22f, 120) { Ink = 0.035f };
            Rgba ink = DoodlePainter.DefaultInk;
            foreach (float s in new[] { -1f, 1f })
            {
                if (kind == 2)
                {
                    p.InkLine(0.035f, s * 0.2f, 0.0f, s * 0.13f, 0.06f, s * 0.06f, 0.0f);   // occhi a mezzaluna felici
                    p.Fill(Sd.Ellipse(s * 0.22f, -0.1f, 0.06f, 0.035f), Rgba.Hex("FF9AB0", 0.75f));
                }
                else
                {
                    p.Fill(Sd.Ellipse(s * 0.13f, 0.0f, 0.04f, 0.055f), ink);
                    p.Fill(Sd.Circle(s * 0.13f - 0.012f, 0.018f, 0.013f), Rgba.White);
                    // sopracciglia tristi (all'insù verso il centro)
                    p.InkLine(0.025f, s * 0.22f, 0.08f, s * 0.07f, 0.14f);
                }
            }
            if (kind == 0)
            {
                p.InkLine(0.03f, -0.1f, -0.18f, -0.04f, -0.13f, 0.04f, -0.13f, 0.1f, -0.18f);   // bocca all'ingiù
                p.Fill(Sd.Ellipse(0.2f, -0.06f, 0.025f, 0.04f), V86AWater);                    // lacrimuccia
            }
            else if (kind == 1)
            {
                p.Shape(Sd.Box(0f, -0.16f, 0.13f, 0.055f, 0.03f), Rgba.White, 0.025f);         // denti che battono
                for (int i = -1; i <= 1; i++) p.InkLine(0.012f, i * 0.06f, -0.2f, i * 0.06f, -0.12f);
                p.InkLine(0.012f, -0.12f, -0.16f, 0.12f, -0.16f);
                p.Fill(Sd.Ellipse(-0.22f, -0.08f, 0.05f, 0.03f), Rgba.Hex("8FB8FF", 0.7f));
                p.Fill(Sd.Ellipse(0.22f, -0.08f, 0.05f, 0.03f), Rgba.Hex("8FB8FF", 0.7f));
            }
            else
            {
                p.Shape(Sd.Intersect(Sd.Circle(0f, -0.1f, 0.13f), Sd.Box(0f, -0.2f, 0.15f, 0.1f)), Rgba.Hex("B9302E"), 0.03f);   // sorriso aperto
            }
            return new DoodleArt(kind == 0 ? "v86_a_face_sad" : kind == 1 ? "v86_a_face_shiver" : "v86_a_face_happy", p);
        }

        /// <summary>Le gocce che colano addosso all'omino (origine ai piedi): rivoli sulla testa, macchie sull'impermeabile, gocce che cadono dal mento e dalle maniche.</summary>
        public static DoodleArt V86A_Drips()
        {
            var p = new DoodlePainter(-0.6f, -0.08f, 0.62f, 2.0f, 110) { Ink = 0.025f, Wobble = 0.006f };
            Rgba w = V86AWater.WithAlpha(0.85f);
            // rivoli sulla testa
            foreach (float s in new[] { -0.28f, 0.3f })
                p.Shape(Sd.Union(Sd.Box(s, V86AHeadY + 0.05f, 0.025f, 0.12f, 0.02f), Sd.Circle(s, V86AHeadY - 0.08f, 0.045f)), w, 0.02f);
            // macchie bagnate sull'impermeabile
            p.Fill(Sd.Ellipse(-0.15f, 0.9f, 0.1f, 0.07f), Rgba.Hex("B08420", 0.6f));
            p.Fill(Sd.Ellipse(0.2f, 0.7f, 0.12f, 0.08f), Rgba.Hex("B08420", 0.6f));
            p.Fill(Sd.Ellipse(-0.25f, 0.6f, 0.08f, 0.06f), Rgba.Hex("B08420", 0.6f));
            // gocce che cadono
            foreach (float[] d in new[] { new[] { -0.45f, 0.4f }, new[] { 0.42f, 0.42f }, new[] { 0f, 1.08f }, new[] { -0.2f, 0.12f } })
                p.Shape(Sd.Union(Sd.Circle(d[0], d[1], 0.04f), Sd.Polygon(d[0] - 0.03f, d[1] + 0.02f, d[0] + 0.03f, d[1] + 0.02f, d[0], d[1] + 0.1f)), w, 0.02f);
            return new DoodleArt("v86_a_drips", p);
        }
    }
}
