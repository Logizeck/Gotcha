using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.74 per il livello 273 "The Piggy Bank" (le monete sono "coin", l'orsetto "teddy_bear",
    /// il martello "v69_cp_hammer", il biglietto "sticky_note", le stelline "sparkle_star"): la cameretta (sfondo
    /// intero con la scrivania e la finestra sulla strada), il salvadanaio Mr. Oink (con la fessura piena di monete e
    /// l'adesivo con la freccia in giù), il tappo di gomma sotto la pancia e il buco che lascia, il bambino (preoccupato,
    /// "NO!" a braccia tese, che esulta), il coltello da burro, il mucchio di monete, il furgone dei gelati e lo
    /// scoppio del "NO!". Prefisso v74_a_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV74A() => new DoodleArt[]
        {
            V74A_RoomBg(), V74A_Piggy(), V74A_Plug(), V74A_PlugHole(), V74A_Kid(0), V74A_Kid(1), V74A_Kid(2),
            V74A_ButterKnife(), V74A_CoinPile(), V74A_IceCreamTruck(), V74A_NoBurst(),
        };

        static readonly Rgba V74APink = Rgba.Hex("F7A8C0"), V74APinkShade = Rgba.Hex("E07C9C"), V74APinkDeep = Rgba.Hex("C95F82");
        static readonly Rgba V74AGold = Rgba.Hex("F5C443"), V74AGoldShade = Rgba.Hex("D69B22");
        static readonly Rgba V74AWood = Rgba.Hex("D9A86C"), V74AWoodShade = Rgba.Hex("B5814A");
        static readonly Rgba V74ASkin = Rgba.Hex("F6C9A0"), V74ASkinShade = Rgba.Hex("DCA477");
        static readonly Rgba V74AHair = Rgba.Hex("7A4A2A");

        /// <summary>Piano della scrivania nel mondo (lo usa anche il builder): superficie in y -0.84.</summary>
        public const float V74ADeskTop = -1.0f;

        /// <summary>
        /// La cameretta: carta da parati color pesca a stelline, la finestra (centro 1.15, 1.5; vetro 1.6 x 1.3) con il
        /// cielo, le casette e la strada (y 0.92), le tendine, la scrivania di legno (piano da x -0.95 a 2.7, superficie
        /// in y -1.0, gambe fino al pavimento), il battiscopa (y -2.45), la moquette azzurra col tappeto tondo (centro
        /// y -3.6). Coordinate del mondo.
        /// </summary>
        public static DoodleArt V74A_RoomBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f, Wobble = 0.006f };
            Sdf wall = Sd.Box(0f, 2.4f, 5f, 4.7f);
            p.FillFunc(wall, (x, y) => Rgba.Lerp(Rgba.Hex("F6D9C2"), Rgba.Hex("FCEBDD"), Math.Max(0f, Math.Min(1f, (y + 2.2f) / 6f))));
            // stelline della carta da parati (a griglia sfalsata)
            for (int r = 0; r < 16; r++)
                for (int c = 0; c < 12; c++)
                {
                    float x = -4.4f + c * 0.8f + (r % 2) * 0.4f, y = -2.0f + r * 0.6f;
                    if (x > 0.1f && x < 2.2f && y > 0.55f && y < 2.45f) continue;   // dietro la finestra
                    p.Fill(V74AStar(x, y, 0.07f), Rgba.Hex("F2B98F", 0.75f));
                }
            // la finestra: cielo, casette, strada
            float wx = 1.15f, wy = 1.5f;
            p.Shape(Sd.Box(wx, wy, 0.92f, 0.77f, 0.05f), Rgba.Hex("FFFFFF"), 0.05f);
            Sdf glass = Sd.Box(wx, wy, 0.8f, 0.65f, 0.02f);
            p.FillFunc(glass, (x, y) => Rgba.Lerp(Rgba.Hex("BFE6FA"), Rgba.Hex("8ECDF0"), Math.Max(0f, Math.Min(1f, (y - 0.85f) / 1.3f))));
            p.Fill(Sd.Intersect(Sd.Circle(wx + 0.5f, wy + 0.42f, 0.14f), glass), Rgba.Hex("FFE47A"));
            p.Fill(Sd.Intersect(Sd.Union(Sd.Circle(wx - 0.35f, wy + 0.35f, 0.12f), Sd.Circle(wx - 0.2f, wy + 0.38f, 0.15f), Sd.Circle(wx - 0.05f, wy + 0.33f, 0.1f)), glass), Rgba.White.WithAlpha(0.9f));
            // casette lontane
            float[] hx = { -0.62f, -0.2f, 0.3f, 0.66f };
            string[] hc = { "F2C46B", "E98C7A", "9CC59A", "C8A6E0" };
            for (int i = 0; i < hx.Length; i++)
            {
                float h = 0.28f + (i % 2) * 0.1f;
                Sdf house = Sd.Union(Sd.Box(wx + hx[i], 1.12f + h * 0.5f, 0.17f, h * 0.5f), Sd.Polygon(wx + hx[i] - 0.2f, 1.12f + h, wx + hx[i] + 0.2f, 1.12f + h, wx + hx[i], 1.12f + h + 0.16f));
                p.Fill(Sd.Intersect(house, glass), Rgba.Hex(hc[i]));
                p.Fill(Sd.Intersect(Sd.Box(wx + hx[i], 1.12f + h * 0.45f, 0.05f, 0.05f), glass), Rgba.Hex("FFF7D0"));
            }
            // marciapiede e strada (il furgone dei gelati passa qui)
            p.Fill(Sd.Intersect(Sd.Box(wx, 1.07f, 1f, 0.05f), glass), Rgba.Hex("D9D4CC"));
            p.Fill(Sd.Intersect(Sd.Box(wx, 0.92f, 1f, 0.1f), glass), Rgba.Hex("7B7F8C"));
            for (int i = -3; i <= 3; i++) p.Fill(Sd.Intersect(Sd.Box(wx + i * 0.3f, 0.92f, 0.08f, 0.012f), glass), Rgba.Hex("F4F1E8"));
            p.Shape(glass, Rgba.Clear, 0.035f);
            p.Line(Rgba.Hex("2B2233"), 0.04f, wx, wy - 0.65f, wx, wy + 0.65f);
            p.Line(Rgba.Hex("2B2233"), 0.04f, wx - 0.8f, wy + 0.1f, wx + 0.8f, wy + 0.1f);
            p.Shape(Sd.Box(wx, wy - 0.83f, 1.02f, 0.06f, 0.02f), Rgba.Hex("FFFFFF"), 0.04f);
            // le tendine
            foreach (float s in new[] { -1f, 1f })
            {
                Sdf cur = Sd.Polygon(wx + s * 0.98f, wy + 0.92f, wx + s * 0.62f, wy + 0.92f, wx + s * 0.78f, wy + 0.1f, wx + s * 0.86f, wy - 0.75f, wx + s * 1.08f, wy - 0.75f);
                Body(p, cur, Rgba.Hex("8FD0C2"), Rgba.Hex("67B0A1"));
                p.Line(Rgba.Hex("67B0A1"), 0.02f, wx + s * 0.86f, wy + 0.85f, wx + s * 0.93f, wy - 0.7f);
            }
            p.Shape(Sd.Box(wx, wy + 0.95f, 1.2f, 0.035f, 0.03f), Rgba.Hex("B5814A"), 0.03f);
            // battiscopa e moquette
            p.Fill(Sd.Box(0f, -2.45f, 5f, 0.08f), Rgba.Hex("FFFFFF"), false);
            p.Line(Rgba.Hex("2B2233", 0.6f), 0.04f, -4.5f, -2.37f, 4.5f, -2.37f);
            Sdf floor = Sd.Box(0f, -4.95f, 5f, 2.42f);
            p.FillFunc(floor, (x, y) => Rgba.Lerp(Rgba.Hex("A9CFE3"), Rgba.Hex("C3E0EE"), Math.Max(0f, Math.Min(1f, (-2.53f - y) / 4.5f))));
            p.Line(Rgba.Hex("2B2233", 0.5f), 0.03f, -4.5f, -2.53f, 4.5f, -2.53f);
            // il tappeto tondo a righe
            Sdf rug = Sd.Ellipse(-0.2f, -3.6f, 2.3f, 0.75f);
            p.Shape(rug, Rgba.Hex("F7E39A"), 0.04f);
            p.Shape(Sd.Ellipse(-0.2f, -3.6f, 1.7f, 0.52f), Rgba.Hex("F4B3C2"), 0.03f);
            p.Shape(Sd.Ellipse(-0.2f, -3.6f, 1.05f, 0.3f), Rgba.Hex("BFE3B4"), 0.03f);
            // la scrivania: ombra, gambe, cassettiera, piano
            p.Fill(Sd.Box(0.95f, -2.57f, 1.75f, 0.05f), new Rgba(0f, 0f, 0f, 0.12f));
            foreach (float x in new[] { -0.8f, 2.55f })
                Body(p, Sd.Box(x, -1.83f, 0.07f, 0.74f, 0.02f), V74AWood, V74AWoodShade);
            Body(p, Sd.Box(0.85f, V74ADeskTop - 0.23f, 1.8f, 0.17f, 0.03f), V74AWood, V74AWoodShade);
            p.Shape(Sd.Box(0.85f, V74ADeskTop - 0.23f, 0.28f, 0.035f, 0.03f), Rgba.Hex("8A5A3B"), 0.02f);
            Body(p, Sd.Box(0.88f, V74ADeskTop - 0.07f, 1.86f, 0.075f, 0.03f), Rgba.Hex("E8BC82"), V74AWoodShade);
            p.Line(Rgba.White.WithAlpha(0.4f), 0.02f, -0.8f, V74ADeskTop - 0.03f, 1.6f, V74ADeskTop - 0.03f);
            return new DoodleArt("v74_a_room_bg", p);
        }

        /// <summary>Stellina a cinque punte (per la carta da parati).</summary>
        static Sdf V74AStar(float cx, float cy, float r)
        {
            var pts = new float[20];
            for (int i = 0; i < 10; i++)
            {
                double a = Math.PI / 2 + i * Math.PI / 5;
                float rr = i % 2 == 0 ? r : r * 0.45f;
                pts[i * 2] = cx + (float)Math.Cos(a) * rr;
                pts[i * 2 + 1] = cy + (float)Math.Sin(a) * rr;
            }
            return Sd.Polygon(pts);
        }

        /// <summary>
        /// Il salvadanaio Mr. Oink (centro 0,0, muso a destra): pancia di ceramica rosa (ellisse 0.8 x 0.56 in 0,0.05),
        /// quattro zampette fino a y -0.62, le orecchie, il codino arricciato a sinistra, la fessura in alto (0,0.6) con
        /// il bordo d'oro delle monete, l'adesivo bianco con la freccia in giù sul fianco (0.08,-0.22). Il tappo è un
        /// disegno a parte (v74_a_plug) attaccato sotto la pancia in (0,-0.5).
        /// </summary>
        public static DoodleArt V74A_Piggy()
        {
            var p = new DoodlePainter(-1.08f, -0.7f, 1.08f, 0.78f, 150) { Ink = 0.04f };
            // zampe di dietro (più scure) e davanti
            foreach (float x in new[] { -0.32f, 0.5f })
                p.Shape(Sd.Box(x + 0.1f, -0.47f, 0.11f, 0.14f, 0.05f), V74APinkDeep, 0.035f);
            // codino arricciato
            p.Line(V74APinkDeep, 0.075f, -0.74f, 0.12f, -0.9f, 0.16f, -0.96f, 0.28f, -0.88f, 0.36f, -0.8f, 0.3f, -0.86f, 0.22f, -0.98f, 0.18f);
            p.InkLine(0.03f, -0.74f, 0.08f, -0.76f, 0.08f);
            Sdf body = Sd.Ellipse(0f, 0.05f, 0.8f, 0.56f);
            Body(p, body, V74APink, V74APinkShade);
            foreach (float x in new[] { -0.42f, 0.4f })
                p.Shape(Sd.Box(x, -0.5f, 0.12f, 0.13f, 0.05f), V74APink, 0.035f);
            // orecchie
            Body(p, Sd.Polygon(0.3f, 0.5f, 0.46f, 0.82f - 0.1f, 0.58f, 0.44f), V74APink, V74APinkShade);
            p.Fill(Sd.Polygon(0.36f, 0.5f, 0.46f, 0.66f, 0.52f, 0.47f), V74APinkDeep.WithAlpha(0.7f));
            // muso
            Body(p, Sd.Ellipse(0.83f, 0.04f, 0.14f, 0.19f), Rgba.Hex("F9B9CD"), V74APinkShade);
            foreach (float y in new[] { 0.1f, -0.02f }) p.Fill(Sd.Ellipse(0.85f, y, 0.025f, 0.04f), V74APinkDeep);
            // occhio sorridente, guancia, sorriso
            p.InkLine(0.035f, 0.42f, 0.2f, 0.48f, 0.26f, 0.55f, 0.2f);
            p.Fill(Sd.Ellipse(0.55f, 0.04f, 0.09f, 0.05f), Rgba.Hex("FF7F9C", 0.55f));
            p.InkLine(0.025f, 0.62f, -0.12f, 0.68f, -0.16f, 0.74f, -0.13f);
            // la fessura con il bordo delle monete
            p.Shape(Sd.Box(-0.05f, 0.58f, 0.2f, 0.035f, 0.03f), Rgba.Hex("3A2A33"), 0.025f);
            p.Fill(Sd.Box(-0.08f, 0.6f, 0.12f, 0.014f, 0.01f), V74AGold);
            // lucido della ceramica
            p.Line(Rgba.White.WithAlpha(0.75f), 0.05f, -0.52f, 0.32f, -0.4f, 0.42f, -0.25f, 0.48f);
            p.Fill(Sd.Circle(-0.12f, 0.46f, 0.03f), Rgba.White.WithAlpha(0.8f));
            // l'adesivo con la freccia in giù (l'indizio)
            float sx = 0.08f, sy = -0.22f;
            p.Shape(Sd.Circle(sx, sy, 0.1f), Rgba.Hex("FFFDF2"), 0.02f);
            p.InkLine(0.025f, sx, sy + 0.06f, sx, sy - 0.04f);
            p.Fill(Sd.Polygon(sx - 0.045f, sy - 0.02f, sx + 0.045f, sy - 0.02f, sx, sy - 0.075f), DoodlePainter.DefaultInk);
            return new DoodleArt("v74_a_piggy", p);
        }

        /// <summary>
        /// Il tappo di gomma (attacco alla pancia in 0,0, sporge verso -y): il disco nero e la linguetta ad anello.
        /// Col maialino capovolto sporge verso l'alto.
        /// </summary>
        public static DoodleArt V74A_Plug()
        {
            var p = new DoodlePainter(-0.3f, -0.38f, 0.3f, 0.07f, 220) { Ink = 0.028f };
            Rgba rubber = Rgba.Hex("4A4452"), rubberShade = Rgba.Hex("2E2A36");
            p.Shape(Sd.Subtract(Sd.Circle(0f, -0.26f, 0.09f), Sd.Circle(0f, -0.26f, 0.045f)), rubber);
            Body(p, Sd.Box(0f, -0.14f, 0.065f, 0.05f, 0.02f), rubber, rubberShade);
            Body(p, Sd.Box(0f, -0.045f, 0.22f, 0.055f, 0.035f), rubber, rubberShade);
            p.Line(Rgba.White.WithAlpha(0.45f), 0.018f, -0.15f, -0.025f, 0.08f, -0.025f);
            return new DoodleArt("v74_a_plug", p);
        }

        /// <summary>Il buco tondo lasciato dal tappo (centro 0,0, stesso punto d'attacco), visto di sbieco.</summary>
        public static DoodleArt V74A_PlugHole()
        {
            var p = new DoodlePainter(-0.26f, -0.1f, 0.26f, 0.1f, 220) { Ink = 0.02f };
            p.Shape(Sd.Ellipse(0f, 0f, 0.19f, 0.06f), Rgba.Hex("2A1E26"));
            p.Fill(Sd.Ellipse(0.04f, -0.012f, 0.09f, 0.022f), V74AGold.WithAlpha(0.9f));
            return new DoodleArt("v74_a_plug_hole", p);
        }

        /// <summary>
        /// Il bambino in piedi (piedi in 0,0, alto circa 2.6; guarda verso destra, verso il salvadanaio).
        /// mood 0 = preoccupato (braccia giù, goccia di sudore), 1 = "NO!" (braccia alzate verso destra, a proteggere),
        /// 2 = esulta (braccia in su, occhi chiusi felici).
        /// </summary>
        public static DoodleArt V74A_Kid(int mood)
        {
            var p = new DoodlePainter(-0.85f, -0.08f, 1.15f, 2.62f, 110) { Ink = 0.04f };
            Rgba shirt = Rgba.Hex("F28C38"), shirtShade = Rgba.Hex("D46A1E");
            Rgba shorts = Rgba.Hex("4F7FD6"), shortsShade = Rgba.Hex("3A62B0");
            // scarpe e gambe
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Box(s * 0.16f, 0.36f, 0.075f, 0.3f, 0.04f), V74ASkin, 0.035f);
                p.Shape(Sd.Union(Sd.Ellipse(s * 0.16f + 0.06f, 0.08f, 0.17f, 0.09f), Sd.Box(s * 0.16f, 0.12f, 0.1f, 0.06f)), Rgba.Hex("E8433F"), 0.035f);
                p.Fill(Sd.Box(s * 0.16f + 0.04f, 0.04f, 0.15f, 0.02f), Rgba.White);
            }
            // braccia (dietro al busto quando sono giù)
            if (mood == 0)
                foreach (float s in new[] { -1f, 1f })
                {
                    p.Line(V74ASkin, 0.13f, s * 0.36f, 1.28f, s * 0.42f, 0.98f, s * 0.36f, 0.78f);
                    p.Shape(Sd.Circle(s * 0.35f, 0.76f, 0.075f), V74ASkin, 0.03f);
                }
            // pantaloncini e maglietta con la stella
            Body(p, Sd.Box(0f, 0.72f, 0.3f, 0.14f, 0.05f), shorts, shortsShade);
            Sdf torso = Sd.Union(Sd.Box(0f, 1.1f, 0.32f, 0.32f, 0.12f), Sd.Box(0f, 0.86f, 0.3f, 0.1f, 0.04f));
            Body(p, torso, shirt, shirtShade);
            p.Fill(V74AStar(0.02f, 1.1f, 0.13f), Rgba.Hex("FFE14A"));
            if (mood == 0)
                foreach (float s in new[] { -1f, 1f })
                    p.Shape(Sd.Rotate(Sd.Box(s * 0.36f, 1.22f, 0.09f, 0.14f, 0.05f), s * 0.36f, 1.22f, s * 12f), shirt, 0.035f);
            if (mood == 1)
            {
                // le braccia tese verso il salvadanaio, mani aperte: "NO!"
                // le braccia alzate verso il salvadanaio, mani aperte: "NO!"
                foreach (float dy in new[] { 0.2f, 0f })
                {
                    p.Line(V74ASkin, 0.13f, 0.12f, 1.22f + dy * 0.5f, 0.5f, 1.4f + dy * 0.8f, 0.86f, 1.55f + dy);
                    p.Shape(Sd.Rotate(Sd.Union(Sd.Ellipse(0.92f, 1.58f + dy, 0.07f, 0.1f), Sd.Box(0.98f, 1.6f + dy, 0.035f, 0.08f, 0.03f)), 0.92f, 1.58f + dy, 20f), V74ASkin, 0.03f);
                }
                p.Shape(Sd.Rotate(Sd.Box(0.25f, 1.3f, 0.13f, 0.1f, 0.05f), 0.25f, 1.3f, 25f), shirt, 0.035f);
            }
            if (mood == 2)
                foreach (float s in new[] { -1f, 1f })
                {
                    p.Line(V74ASkin, 0.13f, s * 0.3f, 1.3f, s * 0.48f, 1.6f, s * 0.58f, 1.95f);
                    p.Shape(Sd.Circle(s * 0.6f, 2.0f, 0.085f), V74ASkin, 0.03f);
                    p.Shape(Sd.Rotate(Sd.Box(s * 0.33f, 1.33f, 0.1f, 0.12f, 0.05f), s * 0.33f, 1.33f, s * -30f), shirt, 0.035f);
                }
            // testa
            float hx = 0.04f, hy = 1.78f;
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Circle(hx + s * 0.39f, hy - 0.02f, 0.09f), V74ASkin, V74ASkinShade);
            Body(p, Sd.Ellipse(hx, hy, 0.4f, 0.38f), V74ASkin, V74ASkinShade, null, -0.04f, 0.05f);
            Sdf hair = Sd.Union(Sd.Intersect(Sd.Ellipse(hx, hy + 0.08f, 0.43f, 0.38f), Sd.Box(hx, hy + 0.42f, 0.6f, 0.2f)),
                Sd.Polygon(hx - 0.3f, hy + 0.28f, hx - 0.16f, hy + 0.56f, hx - 0.02f, hy + 0.34f, hx + 0.1f, hy + 0.58f, hx + 0.2f, hy + 0.32f, hx + 0.38f, hy + 0.48f, hx + 0.36f, hy + 0.22f),
                Sd.Circle(hx - 0.34f, hy + 0.18f, 0.1f));
            Body(p, hair, V74AHair, Rgba.Hex("5A3A2A"));
            float ex = hx + 0.06f;
            if (mood == 2)
            {
                // occhi chiusi felici, boccone aperto, guance rosse
                foreach (float s in new[] { -1f, 1f }) p.InkLine(0.03f, ex + s * 0.14f - 0.07f, hy + 0.02f, ex + s * 0.14f, hy + 0.09f, ex + s * 0.14f + 0.07f, hy + 0.02f);
                Sdf mouth = Sd.Intersect(Sd.Ellipse(ex, hy - 0.16f, 0.15f, 0.13f), Sd.Box(ex, hy - 0.24f, 0.2f, 0.08f));
                p.Shape(mouth, Rgba.Hex("B8344A"), 0.025f);
                p.Fill(Sd.Ellipse(ex, hy - 0.27f, 0.07f, 0.03f), Rgba.Hex("FF8FA0"));
            }
            else
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    float x = ex + s * 0.14f;
                    p.Shape(Sd.Ellipse(x, hy + 0.03f, 0.075f, mood == 1 ? 0.1f : 0.085f), Rgba.White, 0.025f);
                    p.Fill(Sd.Circle(x + 0.025f, hy + 0.02f, 0.038f), DoodlePainter.DefaultInk);
                    // sopracciglia: all'insù (preoccupato) o aggrottate (NO!)
                    float inner = mood == 1 ? -0.02f : 0.05f;
                    p.InkLine(0.025f, x - s * 0.07f, hy + 0.15f + (s < 0 ? inner : 0f), x + s * 0.07f, hy + 0.15f + (s > 0 ? inner : 0f));
                }
                if (mood == 1)
                    p.Shape(Sd.Ellipse(ex + 0.02f, hy - 0.2f, 0.09f, 0.08f), Rgba.Hex("8A2433"), 0.025f);
                else
                    p.InkLine(0.025f, ex - 0.09f, hy - 0.2f, ex - 0.04f, hy - 0.17f, ex + 0.01f, hy - 0.2f, ex + 0.06f, hy - 0.17f, ex + 0.1f, hy - 0.2f);
            }
            p.Fill(Sd.Ellipse(hx - 0.2f, hy - 0.1f, 0.07f, 0.04f), Rgba.Hex("FF7F9C", 0.5f));
            if (mood == 0)
                p.Shape(Sd.Union(Sd.Circle(hx + 0.42f, hy + 0.12f, 0.045f), Sd.Polygon(hx + 0.38f, hy + 0.13f, hx + 0.46f, hy + 0.13f, hx + 0.42f, hy + 0.24f)), Rgba.Hex("BFE6FF"), 0.02f);
            string[] names = { "v74_a_kid_worried", "v74_a_kid_no", "v74_a_kid_cheer" };
            return new DoodleArt(names[mood], p);
        }

        /// <summary>Il coltello da burro sdraiato (centro 0,0, lungo 1): manico bianco a sinistra, lama arrotondata a destra.</summary>
        public static DoodleArt V74A_ButterKnife()
        {
            var p = new DoodlePainter(-0.56f, -0.14f, 0.56f, 0.14f, 170) { Ink = 0.03f };
            Body(p, Sd.Box(-0.3f, 0f, 0.22f, 0.06f, 0.05f), Rgba.Hex("F4F1EA"), Rgba.Hex("CFC8BA"));
            p.Fill(Sd.Circle(-0.4f, 0f, 0.015f), Rgba.Hex("9E9A92"));
            p.Fill(Sd.Circle(-0.2f, 0f, 0.015f), Rgba.Hex("9E9A92"));
            Sdf blade = Sd.Union(Sd.Box(0.18f, 0.005f, 0.28f, 0.05f, 0.01f), Sd.Circle(0.44f, 0.005f, 0.05f));
            Body(p, blade, Rgba.Hex("D5DAE2"), Rgba.Hex("A7AFBC"));
            p.Line(Rgba.White.WithAlpha(0.7f), 0.012f, 0.0f, 0.025f, 0.38f, 0.025f);
            return new DoodleArt("v74_a_butter_knife", p);
        }

        /// <summary>Il mucchio di monete sulla scrivania (base in 0,0, largo 1.4, alto 0.45).</summary>
        public static DoodleArt V74A_CoinPile()
        {
            var p = new DoodlePainter(-0.78f, -0.06f, 0.78f, 0.55f, 140) { Ink = 0.03f };
            Sdf heap = Sd.Intersect(Sd.Ellipse(0f, 0f, 0.72f, 0.45f), Sd.Box(0f, 0.25f, 0.8f, 0.25f));
            Body(p, heap, V74AGold, V74AGoldShade);
            var rnd = new Random(273);
            for (int i = 0; i < 26; i++)
            {
                float x = -0.6f + (float)rnd.NextDouble() * 1.2f;
                float top = 0.45f * (float)Math.Sqrt(Math.Max(0.0, 1.0 - (x / 0.72f) * (x / 0.72f)));
                float y = 0.04f + (float)rnd.NextDouble() * Math.Max(0.02f, top - 0.08f);
                p.Shape(Sd.Ellipse(x, y, 0.09f, 0.045f), i % 3 == 0 ? Rgba.Hex("FFE08A") : V74AGold, 0.018f);
            }
            p.Shape(Sd.Ellipse(0.3f, 0.36f, 0.09f, 0.045f), Rgba.Hex("D9DDE6"), 0.018f);
            p.Shape(Sd.Ellipse(-0.25f, 0.22f, 0.09f, 0.045f), Rgba.Hex("D9DDE6"), 0.018f);
            return new DoodleArt("v74_a_coin_pile", p);
        }

        /// <summary>Il furgoncino dei gelati (ruote in y 0, centro x 0, muso a sinistra; lungo 1.1): cono gigante sul tetto.</summary>
        public static DoodleArt V74A_IceCreamTruck()
        {
            var p = new DoodlePainter(-0.62f, -0.12f, 0.62f, 1.06f, 130) { Ink = 0.03f };
            Sdf body = Sd.Union(Sd.Box(0.1f, 0.33f, 0.45f, 0.26f, 0.06f), Sd.Box(-0.38f, 0.24f, 0.18f, 0.17f, 0.05f));
            Body(p, body, Rgba.Hex("FFFFFF"), Rgba.Hex("D8DCE6"));
            p.Fill(Sd.Box(0.1f, 0.2f, 0.44f, 0.04f), Rgba.Hex("F48FB1"));
            p.Shape(Sd.Box(-0.42f, 0.3f, 0.1f, 0.08f, 0.02f), Rgba.Hex("9FD8F5"), 0.02f);
            p.Shape(Sd.Box(0.14f, 0.4f, 0.22f, 0.1f, 0.02f), Rgba.Hex("FFE7A3"), 0.02f);
            foreach (float x in new[] { -0.32f, 0.3f })
            {
                p.Shape(Sd.Circle(x, 0.06f, 0.09f), Rgba.Hex("3A3442"), 0.025f);
                p.Fill(Sd.Circle(x, 0.06f, 0.035f), Rgba.Hex("C9CFD9"));
            }
            // il cono sul tetto
            p.Shape(Sd.Polygon(0.03f, 0.74f, 0.21f, 0.74f, 0.12f, 0.58f), Rgba.Hex("D9A86C"), 0.02f);
            p.Shape(Sd.Union(Sd.Circle(0.12f, 0.8f, 0.1f), Sd.Circle(0.12f, 0.9f, 0.065f)), Rgba.Hex("F7A8C0"), 0.02f);
            p.Shape(Sd.Circle(0.12f, 0.98f, 0.03f), Rgba.Hex("E8433F"), 0.015f);
            return new DoodleArt("v74_a_icecream_truck", p);
        }

        /// <summary>Lo scoppio a punte del "NO!" (centro 0,0, raggio 0.5; la scritta la mette il livello).</summary>
        public static DoodleArt V74A_NoBurst()
        {
            var p = new DoodlePainter(-0.62f, -0.5f, 0.62f, 0.5f, 120) { Ink = 0.04f };
            var pts = new float[32];
            for (int i = 0; i < 16; i++)
            {
                double a = i * Math.PI / 8;
                float r = i % 2 == 0 ? 0.56f : 0.38f;
                pts[i * 2] = (float)Math.Cos(a) * r;
                pts[i * 2 + 1] = (float)Math.Sin(a) * r * 0.8f;
            }
            p.Shape(Sd.Polygon(pts), Rgba.Hex("FFE14A"));
            p.Fill(Sd.Ellipse(0f, 0f, 0.3f, 0.22f), Rgba.Hex("FFF3A6"));
            return new DoodleArt("v74_a_no_burst", p);
        }
    }
}
