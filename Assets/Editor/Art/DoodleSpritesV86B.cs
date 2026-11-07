using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.86 per il livello "The Imaginary Bridge" (roadmap n. 110): lo sfondo (cielo del tramonto
    /// con il sole e le nuvole, le mesas lontane, l'interno del canyon col fiumiciattolo in fondo), le due rocce in primo
    /// piano (un disegno solo, separato dallo sfondo: l'asse che cade nel canyon passa DIETRO le pareti, niente SpriteMask),
    /// la bandierina sulla piattaforma di destra, l'asse a pezzi (il tratto centrale con le venature solo orizzontali, che il
    /// livello allunga in x senza deformarlo, e il capo arrotondato col chiodo, specchiato per l'altro lato) e l'esploratore
    /// (in piedi, spaventato con le braccia in su e la goccia di sudore, felice a braccia alzate). L'ombra è "ball_shadow",
    /// la scintilla "sparkle_star". Nessuna scritta disegnata. Prefisso v86_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV86B() => new DoodleArt[]
        {
            V86B_Bg(), V86B_Cliffs(), V86B_Flag(), V86B_PlankMid(), V86B_PlankEnd(),
            V86B_Man(), V86B_ManScared(), V86B_ManCheer(),
        };

        // misure condivise col builder (le stesse della logica ImaginaryBridgeLogic)
        /// <summary>Piano delle due piattaforme e bordi del canyon.</summary>
        public const float V86BCliffY = -1.0f, V86BLeftEdgeX = -0.8f, V86BRightEdgeX = 0.8f;
        /// <summary>Spessore dell'asse e larghezza di un capo (il tratto centrale è largo 1 e si allunga).</summary>
        public const float V86BPlankH = 0.2f, V86BPlankCap = 0.16f;

        static readonly Rgba V86BWood = Rgba.Hex("D9A05B"), V86BWoodShade = Rgba.Hex("B07A3C"), V86BWoodGrain = Rgba.Hex("A86F33");
        static readonly Rgba V86BRock = Rgba.Hex("C8744A"), V86BRockDark = Rgba.Hex("9C5536"), V86BRockLight = Rgba.Hex("E39A68");
        static readonly Rgba V86BGrass = Rgba.Hex("7CC24E"), V86BGrassDark = Rgba.Hex("5A9A36");
        static readonly Rgba V86BSkin = Rgba.Hex("F2C49B"), V86BSkinShade = Rgba.Hex("D9A57B");
        static readonly Rgba V86BShirt = Rgba.Hex("4FA88A"), V86BShirtShade = Rgba.Hex("3A8570");
        static readonly Rgba V86BShorts = Rgba.Hex("D8C08A"), V86BShortsShade = Rgba.Hex("B89E66");
        static readonly Rgba V86BHat = Rgba.Hex("E6D3A0"), V86BHatShade = Rgba.Hex("C2AC74"), V86BBand = Rgba.Hex("8A5A3A");
        static readonly Rgba V86BBoot = Rgba.Hex("6B4129"), V86BPack = Rgba.Hex("D9773F");

        /// <summary>
        /// Lo sfondo (coordinate del mondo): cielo dal blu al pesca, sole a sinistra, nuvole, mesas lontane viola-rosa sul
        /// piano y -1, sotto l'interno scuro del canyon (le rocce in primo piano coprono i lati) col fiumiciattolo in fondo.
        /// </summary>
        public static DoodleArt V86B_Bg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0f, 2f, 3.3f, 3.3f), (x, y) => Rgba.Lerp(Rgba.Hex("FFD9A8"), Rgba.Hex("6FB6F2"), Math.Max(0f, Math.Min(1f, (y + 0.6f) / 5f))));
            // sole
            p.Fill(Sd.Circle(-1.75f, 2.35f, 0.75f), Rgba.Hex("FFF1B0", 0.35f));
            p.Shape(Sd.Circle(-1.75f, 2.35f, 0.5f), Rgba.Hex("FFE066"), 0.04f);
            // nuvole
            foreach (var c in new[] { (0.9f, 2.6f, 1f), (2.1f, 1.7f, 0.75f), (-0.4f, 1.3f, 0.6f) })
            {
                float cx = c.Item1, cy = c.Item2, k = c.Item3;
                Sdf cloud = Sd.Union(Sd.Ellipse(cx, cy, 0.55f * k, 0.2f * k), Sd.Circle(cx - 0.2f * k, cy + 0.12f * k, 0.22f * k), Sd.Circle(cx + 0.18f * k, cy + 0.16f * k, 0.27f * k));
                p.Shape(cloud, Rgba.Hex("FFFFFF", 0.92f), 0.03f);
            }
            // mesas lontane
            Sdf far = Sd.Polygon(-3.3f, -1.2f, -3.3f, -0.3f, -2.6f, -0.3f, -2.4f, 0.2f, -1.7f, 0.2f, -1.5f, -0.4f, -0.6f, -0.4f,
                -0.45f, 0.45f, 0.35f, 0.45f, 0.5f, -0.2f, 1.4f, -0.2f, 1.6f, 0.05f, 2.4f, 0.05f, 2.55f, -0.5f, 3.3f, -0.5f, 3.3f, -1.2f);
            p.FillFunc(far, (x, y) => Rgba.Lerp(Rgba.Hex("C98A9E"), Rgba.Hex("E2A9A0"), Math.Max(0f, Math.Min(1f, (y + 1.2f) / 1.6f))));
            p.Line(Rgba.Hex("B07A8E"), 0.025f, -0.45f, 0.45f, 0.35f, 0.45f);
            // l'interno del canyon: parete di fondo che scurisce verso il basso
            p.FillFunc(Sd.Box(0f, -3.15f, 3.3f, 2.15f), (x, y) =>
            {
                float k = Math.Max(0f, Math.Min(1f, (-1f - y) / 3.6f));
                Rgba c = Rgba.Lerp(Rgba.Hex("A35A45"), Rgba.Hex("3E2236"), k);
                float band = (float)Math.Sin(y * 7f + Math.Sin(x * 2.3f) * 0.6f);
                return band > 0.75f ? Rgba.Lerp(c, Rgba.Hex("2E1A2A"), 0.25f) : c;
            });
            // il fiumiciattolo in fondo
            p.FillFunc(Sd.Box(0f, -4.95f, 3.3f, 0.25f), (x, y) => Rgba.Lerp(Rgba.Hex("4FA3D9"), Rgba.Hex("2E6FA3"), (y + 4.7f) / -0.5f));
            for (int i = 0; i < 6; i++) p.Line(Rgba.Hex("BFE6FF", 0.7f), 0.025f, -0.5f + i * 0.2f, -4.85f - (i % 2) * 0.12f, -0.38f + i * 0.2f, -4.85f - (i % 2) * 0.12f);
            return new DoodleArt("v86_b_bg", p);
        }

        /// <summary>Profilo di una parete del canyon (lato = -1 sinistra, +1 destra), dal piano y -1 fino in fondo.</summary>
        static Sdf V86B_CliffShape(float side)
        {
            float e = side < 0 ? V86BLeftEdgeX : V86BRightEdgeX, o = side * 3.4f, top = V86BCliffY;
            return Sd.Polygon(o, top, e, top, e - side * 0.04f, top - 0.35f, e + side * 0.08f, top - 0.9f, e - side * 0.06f, top - 1.6f,
                e + side * 0.1f, top - 2.4f, e - side * 0.02f, top - 3.2f, e + side * 0.14f, top - 4.4f, o, top - 4.4f);
        }

        /// <summary>
        /// Le due rocce in primo piano (coordinate del mondo): piano esattamente in y <see cref="V86BCliffY"/>, bordi in x
        /// ±0.8, strati di roccia, crepe, un ciuffo d'erba sul bordo di ogni piano e qualche sasso.
        /// </summary>
        public static DoodleArt V86B_Cliffs()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, -0.75f, 60) { Ink = 0.05f, Wobble = 0.006f };
            foreach (float side in new[] { -1f, 1f })
            {
                Sdf rock = V86B_CliffShape(side);
                p.FillFunc(rock, (x, y) =>
                {
                    float strata = (float)Math.Sin((y + Noise.Value(x * 1.3f, 0.5f) * 0.25f) * 9f);
                    Rgba c = strata > 0.55f ? V86BRockDark : strata < -0.7f ? V86BRockLight : V86BRock;
                    float faceD = Math.Abs(x) - 0.8f;   // più scura verso l'interno del canyon
                    return Rgba.Lerp(c, Rgba.Hex("6E3A2A"), Math.Max(0f, 0.35f - faceD * 0.5f) + Math.Max(0f, (-2f - y) * 0.12f));
                });
                p.Shape(rock, Rgba.Clear);
                // crepe
                float e = side < 0 ? V86BLeftEdgeX : V86BRightEdgeX;
                p.InkLine(0.025f, e - side * 0.35f, -1.6f, e - side * 0.5f, -2.0f, e - side * 0.42f, -2.4f);
                p.InkLine(0.025f, e - side * 1.4f, -2.8f, e - side * 1.2f, -3.3f);
                // l'erba sul piano (sottile: i piedi restano sul piano y -1)
                p.Shape(Sd.Box(side * 2.1f, V86BCliffY - 0.03f, 1.33f, 0.045f, 0.02f), V86BGrass, 0.025f);
                for (int i = 0; i < 9; i++)
                {
                    float gx = e - side * (0.12f + i * 0.28f);
                    p.Line(V86BGrassDark, 0.025f, gx, V86BCliffY, gx - 0.04f, V86BCliffY + 0.09f);
                    p.Line(V86BGrassDark, 0.025f, gx + 0.05f, V86BCliffY, gx + 0.08f, V86BCliffY + 0.07f);
                }
                // sassi
                p.Shape(Sd.Ellipse(side * 2.5f, -1.45f, 0.14f, 0.09f), V86BRockLight, 0.025f);
                p.Shape(Sd.Ellipse(side * 1.6f, -2.6f, 0.1f, 0.07f), V86BRockLight, 0.025f);
            }
            return new DoodleArt("v86_b_cliffs", p);
        }

        /// <summary>La bandierina (origine alla base dell'asta): asta di legno alta 0.95, gagliardetto rosso a punta.</summary>
        public static DoodleArt V86B_Flag()
        {
            var p = new DoodlePainter(-0.12f, -0.05f, 0.62f, 1.05f, 110) { Ink = 0.035f };
            p.Shape(Sd.Box(0f, 0.47f, 0.025f, 0.48f, 0.01f), V86BBoot);
            Body(p, Sd.Polygon(0.03f, 0.95f, 0.58f, 0.8f, 0.03f, 0.62f), Rgba.Hex("F2504A"), Rgba.Hex("C23A35"));
            p.Shape(Sd.Circle(0.22f, 0.79f, 0.045f), Rgba.Hex("FFE066"), 0.02f);
            p.Shape(Sd.Ellipse(0f, 0f, 0.1f, 0.035f), V86BRockDark, 0.02f);
            return new DoodleArt("v86_b_flag", p);
        }

        /// <summary>
        /// Il tratto centrale dell'asse (origine in basso al centro, largo 1, alto <see cref="V86BPlankH"/>): legno con
        /// venature SOLO orizzontali (allungato in x non si deforma), contorno solo sopra e sotto (si attacca ai capi).
        /// </summary>
        public static DoodleArt V86B_PlankMid()
        {
            float h = V86BPlankH;
            var p = new DoodlePainter(-0.5f, -0.03f, 0.5f, h + 0.03f, 120) { Ink = 0.035f, Wobble = 0f };
            p.Fill(Sd.Box(0f, h * 0.5f, 0.52f, h * 0.5f), V86BWood, false);
            p.Fill(Sd.Box(0f, 0.035f, 0.52f, 0.035f), V86BWoodShade, false);
            foreach (float gy in new[] { 0.085f, 0.125f, 0.16f })
                p.Line(V86BWoodGrain, 0.012f, -0.52f, gy, 0.52f, gy);
            p.Fill(Sd.Box(0f, h - 0.025f, 0.52f, 0.012f), Rgba.Hex("F0C487"), false);
            p.InkLine(0.035f, -0.52f, h, 0.52f, h);
            p.InkLine(0.035f, -0.52f, 0f, 0.52f, 0f);
            return new DoodleArt("v86_b_plank_mid", p);
        }

        /// <summary>Il capo sinistro dell'asse (origine sulla punta, in basso; va verso +x per <see cref="V86BPlankCap"/>), arrotondato, col chiodo. Specchiato fa il capo destro.</summary>
        public static DoodleArt V86B_PlankEnd()
        {
            float h = V86BPlankH, w = V86BPlankCap;
            var p = new DoodlePainter(-0.04f, -0.03f, w + 0.02f, h + 0.03f, 120) { Ink = 0.035f, Wobble = 0f };
            Sdf end = Sd.Union(Sd.Box(w * 0.5f + 0.04f, h * 0.5f, w * 0.5f - 0.02f, h * 0.5f), Sd.Box(w * 0.5f, h * 0.5f, w * 0.5f, h * 0.5f, 0.06f));
            p.Fill(end, V86BWood, false);
            p.Fill(Sd.Intersect(end, Sd.Box(w * 0.5f, 0.035f, w, 0.035f)), V86BWoodShade, false);
            foreach (float gy in new[] { 0.085f, 0.125f, 0.16f })
                p.Line(V86BWoodGrain, 0.012f, 0.07f, gy, w + 0.02f, gy);
            // contorno: la punta arrotondata, sopra e sotto (niente linea dove si attacca al tratto centrale)
            Sdf outline = Sd.Subtract(Sd.Box(w * 0.5f, h * 0.5f, w * 0.5f, h * 0.5f, 0.06f), Sd.Box(w * 0.5f, h * 0.5f, w * 0.5f - 0.0175f, h * 0.5f - 0.0175f, 0.045f));
            p.Fill(Sd.Subtract(outline, Sd.Box(w + 0.01f, h * 0.5f, 0.06f, h * 0.5f - 0.0175f)), DoodlePainter.DefaultInk, false);
            p.InkLine(0.035f, 0.06f, h, w + 0.02f, h);
            p.InkLine(0.035f, 0.06f, 0f, w + 0.02f, 0f);
            // il chiodo
            p.Shape(Sd.Circle(0.075f, h * 0.55f, 0.022f), Rgba.Hex("9AA0AA"), 0.015f);
            return new DoodleArt("v86_b_plank_end", p);
        }

        /// <summary>Gambe, scarponi, pantaloncini e zaino comuni dell'esploratore (origine ai piedi).</summary>
        static void V86B_Legs(DoodlePainter p)
        {
            Body(p, Sd.Box(-0.2f, 0.62f, 0.09f, 0.17f, 0.05f), V86BPack, Rgba.Hex("B35A2A"));   // zaino dietro
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Box(s * 0.09f, 0.2f, 0.05f, 0.14f, 0.02f), V86BSkin);
                p.Shape(Sd.Box(s * 0.1f, 0.05f, 0.1f, 0.055f, 0.04f), V86BBoot);
            }
            Body(p, Sd.Box(0f, 0.36f, 0.17f, 0.1f, 0.03f), V86BShorts, V86BShortsShade);
        }

        /// <summary>Il cappello da esploratore (sulla testa di centro cx, cy, raggio r).</summary>
        static void V86B_Hat(DoodlePainter p, float cx, float cy, float r)
        {
            p.Shape(Sd.Ellipse(cx, cy + r * 0.55f, r + 0.12f, 0.05f), V86BHat, 0.03f);
            Body(p, Sd.Intersect(Sd.Circle(cx, cy + r * 0.5f, r * 0.85f), Sd.Box(cx, cy + r * 1.0f, r, r * 0.48f)), V86BHat, V86BHatShade);
            p.Fill(Sd.Box(cx, cy + r * 0.64f, r * 0.82f, 0.025f), V86BBand);
        }

        static void V86B_Torso(DoodlePainter p)
        {
            Body(p, Sd.Box(0f, 0.6f, 0.19f, 0.18f, 0.07f), V86BShirt, V86BShirtShade);
            p.Line(V86BBand, 0.03f, -0.15f, 0.76f, 0.12f, 0.46f);   // cinghia dello zaino
        }

        /// <summary>L'esploratore in piedi, di fronte (origine ai piedi, alto circa 1.2): faccia decisa.</summary>
        public static DoodleArt V86B_Man()
        {
            var p = new DoodlePainter(-0.45f, -0.04f, 0.45f, 1.32f, 120) { Ink = 0.04f };
            V86B_Legs(p);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Capsule(s * 0.2f, 0.72f, s * 0.27f, 0.47f, 0.05f), V86BShirt);
                p.Shape(Sd.Circle(s * 0.28f, 0.43f, 0.055f), V86BSkin);
            }
            V86B_Torso(p);
            const float hy = 0.98f, r = 0.2f;
            Body(p, Sd.Circle(0f, hy, r), V86BSkin, V86BSkinShade);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Ellipse(s * 0.07f, hy - 0.02f, 0.026f, 0.036f), DoodlePainter.DefaultInk);
                p.Fill(Sd.Circle(s * 0.07f - 0.01f, hy - 0.008f, 0.009f), Rgba.White);
                p.Fill(Sd.Ellipse(s * 0.125f, hy - 0.07f, 0.035f, 0.022f), Rgba.Hex("FF9AB0", 0.7f));
            }
            p.InkLine(0.016f, -0.05f, hy - 0.1f, 0.0f, hy - 0.115f, 0.05f, hy - 0.1f);
            V86B_Hat(p, 0f, hy, r);
            return new DoodleArt("v86_b_man", p);
        }

        /// <summary>L'esploratore spaventato (origine ai piedi): braccia in su che si agitano, occhi sbarrati, bocca a "O", goccia di sudore.</summary>
        public static DoodleArt V86B_ManScared()
        {
            var p = new DoodlePainter(-0.5f, -0.04f, 0.5f, 1.4f, 120) { Ink = 0.04f };
            V86B_Legs(p);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Capsule(s * 0.2f, 0.72f, s * 0.38f, 1.08f, 0.05f), V86BShirt);
                p.Shape(Sd.Circle(s * 0.4f, 1.13f, 0.06f), V86BSkin);
            }
            V86B_Torso(p);
            const float hy = 0.98f, r = 0.2f;
            Body(p, Sd.Circle(0f, hy, r), V86BSkin, V86BSkinShade);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Circle(s * 0.075f, hy, 0.045f), Rgba.White, 0.016f);
                p.Fill(Sd.Circle(s * 0.075f, hy - 0.005f, 0.015f), DoodlePainter.DefaultInk);
            }
            p.Shape(Sd.Ellipse(0f, hy - 0.11f, 0.035f, 0.045f), Rgba.Hex("5A1F3A"), 0.016f);
            V86B_Hat(p, 0f, hy + 0.05f, r);
            // goccia di sudore
            p.Shape(Sd.Union(Sd.Circle(0.25f, hy + 0.02f, 0.04f), Sd.Polygon(0.215f, hy + 0.035f, 0.285f, hy + 0.035f, 0.25f, hy + 0.12f)), Rgba.Hex("8FD0FF"), 0.015f);
            return new DoodleArt("v86_b_man_scared", p);
        }

        /// <summary>L'esploratore felice (origine ai piedi): braccia alzate, occhi chiusi a "^", sorrisone.</summary>
        public static DoodleArt V86B_ManCheer()
        {
            var p = new DoodlePainter(-0.5f, -0.04f, 0.5f, 1.45f, 120) { Ink = 0.04f };
            V86B_Legs(p);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Capsule(s * 0.2f, 0.74f, s * 0.33f, 1.18f, 0.05f), V86BShirt);
                p.Shape(Sd.Circle(s * 0.34f, 1.24f, 0.065f), V86BSkin);
            }
            V86B_Torso(p);
            const float hy = 0.98f, r = 0.2f;
            Body(p, Sd.Circle(0f, hy, r), V86BSkin, V86BSkinShade);
            foreach (float s in new[] { -1f, 1f })
            {
                p.InkLine(0.022f, s * 0.11f, hy - 0.03f, s * 0.07f, hy + 0.015f, s * 0.03f, hy - 0.03f);
                p.Fill(Sd.Ellipse(s * 0.13f, hy - 0.07f, 0.04f, 0.025f), Rgba.Hex("FF9AB0", 0.85f));
            }
            Sdf smile = Sd.Intersect(Sd.Ellipse(0f, hy - 0.07f, 0.09f, 0.08f), Sd.Box(0f, hy - 0.12f, 0.1f, 0.05f));
            p.Shape(smile, Rgba.Hex("5A1F3A"), 0.02f);
            V86B_Hat(p, 0f, hy, r);
            return new DoodleArt("v86_b_man_cheer", p);
        }
    }
}
