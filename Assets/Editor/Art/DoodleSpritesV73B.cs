using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.73 per il livello 272 "The Missing Sock" (il gatto che dorme è "cat" con gli occhi aperti
    /// v72_b_cat_eyes, le stelline della festa sono sparkle_star e glow): la lavanderia (sfondo intero), la lavatrice a
    /// oblò con la guarnizione di gomma, il suo sportello chiuso e aperto, la piega della guarnizione tirata, il
    /// pezzetto di calzino che spunta, l'asciugatrice col suo sportello, il coniglietto di pelucchi, lo stendino con la
    /// molletta, la cesta di plastica (dietro e davanti), il detersivo e i calzini (stelline, pois e sette spaiati).
    /// Prefisso v73_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV73B() => new DoodleArt[]
        {
            V73B_LaundryBg(), V73B_Washer(), V73B_WasherDoor(), V73B_DoorOpen(), V73B_SealPeel(), V73B_SockPeek(),
            V73B_Dryer(), V73B_DryerDoor(), V73B_LintBunny(), V73B_Rack(), V73B_Peg(), V73B_BasketBack(),
            V73B_BasketFront(), V73B_Detergent(),
            V73B_SockStars(), V73B_SockDots(),
            V73B_SockStripes(), V73B_SockArgyle(), V73B_SockHearts(), V73B_SockZigzag(), V73B_SockRainbow(),
            V73B_SockChecks(), V73B_SockHole(),
        };

        static readonly Rgba V73BMetal = Rgba.Hex("D7DCE3"), V73BMetalShade = Rgba.Hex("A9B1BC");
        static readonly Rgba V73BRubber = Rgba.Hex("8A8F99"), V73BRubberDark = Rgba.Hex("5F646E");
        static readonly Rgba V73BStarGreen = Rgba.Hex("4FB357"), V73BStarYellow = Rgba.Hex("FFD84A");

        /// <summary>Stella a cinque punte (centro cx, cy, raggio r, ruotata di deg gradi).</summary>
        static Sdf V73B_Star(float cx, float cy, float r, float deg = 0f)
        {
            var xy = new float[20];
            for (int i = 0; i < 10; i++)
            {
                double a = (90.0 + deg + i * 36.0) * Math.PI / 180.0;
                float rr = i % 2 == 0 ? r : r * 0.45f;
                xy[i * 2] = cx + rr * (float)Math.Cos(a);
                xy[i * 2 + 1] = cy + rr * (float)Math.Sin(a);
            }
            return Sd.Polygon(xy);
        }

        static Sdf V73B_Heart(float cx, float cy, float s) => Sd.Union(
            Sd.Circle(cx - s * 0.5f, cy + s * 0.25f, s * 0.55f), Sd.Circle(cx + s * 0.5f, cy + s * 0.25f, s * 0.55f),
            Sd.Polygon(cx - s * 1.0f, cy + s * 0.05f, cx + s * 1.0f, cy + s * 0.05f, cx, cy - s * 1.05f));

        /// <summary>
        /// La lavanderia: piastrelle chiare fino al battiscopa (y = -0.55), pavimento a scacchi grigio-azzurri sotto, il
        /// tappetino davanti allo stendino e in alto il bastone con le grucce vuote. Coordinate del mondo (come kitchen_bg).
        /// </summary>
        public static DoodleArt V73B_LaundryBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f, Wobble = 0.006f };
            // parete a piastrelle
            Sdf wall = Sd.Box(0f, 3.2f, 5f, 3.85f);
            p.FillFunc(wall, (x, y) =>
            {
                int i = (int)Math.Floor(x / 0.6f), j = (int)Math.Floor(y / 0.6f);
                return (i + j) % 2 == 0 ? Rgba.Hex("E3F1F4") : Rgba.Hex("D6EAEF");
            });
            for (int i = -8; i <= 8; i++) p.Line(Rgba.Hex("B9D3DA"), 0.025f, i * 0.6f, -0.55f, i * 0.6f, 7f);
            for (int j = 0; j < 13; j++) p.Line(Rgba.Hex("B9D3DA"), 0.025f, -4.5f, -0.6f + j * 0.6f, 4.5f, -0.6f + j * 0.6f);
            // fascia di piastrelle colorate a metà parete
            p.Fill(Sd.Box(0f, 2.15f, 5f, 0.09f), Rgba.Hex("7CC3C9"), false);
            // battiscopa
            p.Fill(Sd.Box(0f, -0.66f, 5f, 0.11f), Rgba.Hex("F4EDE0"), false);
            p.Line(Rgba.Hex("2B2233", 0.7f), 0.04f, -4.5f, -0.55f, 4.5f, -0.55f);
            p.Line(Rgba.Hex("2B2233", 0.5f), 0.03f, -4.5f, -0.77f, 4.5f, -0.77f);
            // pavimento a scacchi (si schiarisce verso il basso)
            Sdf floor = Sd.Box(0f, -3.9f, 5f, 3.13f);
            p.FillFunc(floor, (x, y) =>
            {
                int i = (int)Math.Floor((x + y * 0.35f) / 0.7f), j = (int)Math.Floor(y / 0.55f);
                Rgba a = (i + j) % 2 == 0 ? Rgba.Hex("B7C4D1") : Rgba.Hex("D3DCE6");
                return Rgba.Lerp(a, Rgba.Hex("E2E8EF"), Math.Max(0f, Math.Min(1f, (-0.8f - y) / 7f)));
            });
            // tappetino tondo davanti allo stendino
            p.Fill(Sd.Ellipse(0.45f, -2.75f, 1.8f, 0.42f), new Rgba(0f, 0f, 0f, 0.06f));
            p.Shape(Sd.Ellipse(0.4f, -2.7f, 1.7f, 0.38f), Rgba.Hex("F2C46B"));
            p.Shape(Sd.Ellipse(0.4f, -2.7f, 1.2f, 0.24f), Rgba.Hex("E9A55B"), 0.03f);
            // il bastone con le grucce vuote (in alto)
            p.Shape(Sd.Capsule(-2.1f, 2.75f, 2.1f, 2.75f, 0.04f), Rgba.Hex("B8875A"), 0.03f);
            foreach (float hx in new[] { -1.5f, -0.95f, 1.1f, 1.6f })
            {
                p.InkLine(0.02f, hx, 2.75f, hx, 2.62f);
                p.Line(Rgba.Hex("2B2233", 0.85f), 0.025f, hx - 0.3f, 2.38f, hx, 2.62f, hx + 0.3f, 2.38f, hx - 0.3f, 2.38f);
            }
            return new DoodleArt("v73_b_laundry_bg", p);
        }

        /// <summary>Corpo di lavatrice/asciugatrice (centro 0,0; 1.9 x 2.16), con i piedini e l'ombra.</summary>
        static void V73B_MachineBody(DoodlePainter p, Rgba body, Rgba shade, Rgba panel)
        {
            p.Fill(Sd.Ellipse(0.05f, -1.1f, 1.0f, 0.08f), new Rgba(0f, 0f, 0f, 0.12f));
            foreach (float x in new[] { -0.72f, 0.72f }) p.Shape(Sd.Box(x, -1.08f, 0.08f, 0.05f, 0.02f), Rgba.Hex("6B6677"), 0.025f);
            Body(p, Sd.Box(0f, 0f, 0.95f, 1.08f, 0.1f), body, shade);
            p.Shape(Sd.Box(0f, 0.86f, 0.89f, 0.17f, 0.05f), panel, 0.03f);
        }

        /// <summary>
        /// La lavatrice (centro 0,0; 1.9 x 2.16): il pannello coi comandi in alto (cassettino del detersivo, display,
        /// manopola), l'oblò centrato in (0, -0.12): guarnizione di gomma (raggio 0.56..0.72) e il cestello dentro.
        /// Lo sportello è un disegno a parte (chiuso o aperto).
        /// </summary>
        public static DoodleArt V73B_Washer()
        {
            var p = new DoodlePainter(-1.02f, -1.2f, 1.02f, 1.13f, 100) { Ink = 0.045f };
            V73B_MachineBody(p, Rgba.Hex("F3F5F8"), Rgba.Hex("C9D1DB"), Rgba.Hex("E3E8EE"));
            p.Shape(Sd.Box(-0.55f, 0.86f, 0.25f, 0.09f, 0.03f), Rgba.Hex("F8FAFC"), 0.025f);
            p.InkLine(0.02f, -0.68f, 0.86f, -0.42f, 0.86f);
            p.Shape(Sd.Box(0.08f, 0.86f, 0.2f, 0.08f, 0.02f), Rgba.Hex("2F3A3A"), 0.025f);
            p.Line(Rgba.Hex("7CE38A"), 0.03f, -0.04f, 0.86f, 0.18f, 0.86f);
            p.Shape(Sd.Circle(0.62f, 0.86f, 0.12f), Rgba.Hex("C9D1DB"), 0.03f);
            p.InkLine(0.025f, 0.62f, 0.86f, 0.68f, 0.95f);
            // la guarnizione di gomma e il cestello
            const float cy = -0.12f;
            p.Shape(Sd.Circle(0f, cy, 0.74f), V73BRubber, 0.035f);
            p.Fill(Sd.Subtract(Sd.Circle(0f, cy, 0.66f), Sd.Circle(0f, cy, 0.6f)), V73BRubberDark.WithAlpha(0.6f));
            p.FillFunc(Sd.Circle(0f, cy, 0.56f), (x, y) => Rgba.Lerp(Rgba.Hex("8D96A3"), Rgba.Hex("4A525E"), Math.Max(0f, Math.Min(1f, (float)Math.Sqrt(x * x + (y - cy) * (y - cy)) / 0.56f))));
            for (int i = -3; i <= 3; i++)
                for (int j = -3; j <= 3; j++)
                {
                    float hx = i * 0.14f + (j % 2 == 0 ? 0f : 0.07f), hy = cy + j * 0.13f;
                    if (hx * hx + (hy - cy) * (hy - cy) < 0.42f * 0.42f) p.Fill(Sd.Circle(hx, hy, 0.022f), Rgba.Hex("2E343D", 0.7f));
                }
            p.Shape(Sd.Circle(0f, cy, 0.56f), Rgba.Clear, 0.03f);
            p.Fill(Sd.Box(0f, cy - 0.5f, 0.12f, 0.04f, 0.02f), Rgba.Hex("6B7380"));   // la nervatura del cestello
            return new DoodleArt("v73_b_washer", p);
        }

        /// <summary>Lo sportello chiuso della lavatrice (centro = centro dell'oblò): anello cromato r 0.66, vetro azzurro.</summary>
        public static DoodleArt V73B_WasherDoor() => V73B_Door("v73_b_washer_door", Rgba.Hex("9FC7E0", 0.88f));

        /// <summary>Lo sportello chiuso dell'asciugatrice (stesso disegno, vetro fumé).</summary>
        public static DoodleArt V73B_DryerDoor() => V73B_Door("v73_b_dryer_door", Rgba.Hex("5A5F6E", 0.92f));

        static DoodleArt V73B_Door(string name, Rgba glass)
        {
            var p = new DoodlePainter(-0.72f, -0.72f, 0.82f, 0.72f, 110) { Ink = 0.04f };
            Body(p, Sd.Circle(0f, 0f, 0.66f), V73BMetal, V73BMetalShade);
            p.Shape(Sd.Circle(0f, 0f, 0.5f), glass, 0.03f);
            p.Fill(Sd.Intersect(Sd.Circle(0f, 0f, 0.5f), Sd.Rotate(Sd.Box(-0.14f, 0.18f, 0.4f, 0.07f), -0.14f, 0.18f, 35f)), Rgba.White.WithAlpha(0.35f));
            p.Fill(Sd.Intersect(Sd.Circle(0f, 0f, 0.5f), Sd.Rotate(Sd.Box(0.06f, 0.0f, 0.4f, 0.025f), 0.06f, 0f, 35f)), Rgba.White.WithAlpha(0.25f));
            p.Shape(Sd.Box(0.66f, 0f, 0.08f, 0.17f, 0.04f), Rgba.Hex("B9C0C9"), 0.03f);   // la maniglia
            return new DoodleArt(name, p);
        }

        /// <summary>
        /// Lo sportello aperto (centro = centro dell'oblò): girato sul cardine di sinistra verso chi guarda, si vede di
        /// taglio come un'ellisse stretta in (-0.78, 0), con i due cardini.
        /// </summary>
        public static DoodleArt V73B_DoorOpen()
        {
            var p = new DoodlePainter(-1.08f, -0.75f, -0.45f, 0.75f, 110) { Ink = 0.04f };
            foreach (float y in new[] { -0.38f, 0.38f }) p.Shape(Sd.Box(-0.58f, y, 0.08f, 0.05f, 0.02f), V73BMetalShade, 0.025f);
            Body(p, Sd.Ellipse(-0.78f, 0f, 0.25f, 0.68f), V73BMetal, V73BMetalShade);
            p.Shape(Sd.Ellipse(-0.79f, 0f, 0.16f, 0.5f), Rgba.Hex("B7D6E8", 0.9f), 0.03f);
            p.Fill(Sd.Ellipse(-0.83f, 0.18f, 0.04f, 0.2f), Rgba.White.WithAlpha(0.45f));
            return new DoodleArt("v73_b_door_open", p);
        }

        /// <summary>
        /// La guarnizione tirata (centro = centro dell'oblò): il labbro di gomma in basso a destra ripiegato all'infuori,
        /// con la piega buia dove si nascondeva il calzino.
        /// </summary>
        public static DoodleArt V73B_SealPeel()
        {
            var p = new DoodlePainter(-0.2f, -0.98f, 0.95f, 0.1f, 110) { Ink = 0.035f };
            Sdf sector = Sd.Polygon(0f, 0f, 0.0f, -1.2f, 1.2f, -0.9f, 1.2f, 0.05f);
            // la piega buia
            p.Fill(Sd.Intersect(Sd.Subtract(Sd.Circle(0f, 0f, 0.74f), Sd.Circle(0f, 0f, 0.58f)), sector), Rgba.Hex("23262D"));
            // il labbro ripiegato in fuori
            Sdf lip = Sd.Intersect(Sd.Subtract(Sd.Circle(0f, 0f, 0.88f), Sd.Circle(0f, 0f, 0.72f)), sector);
            Body(p, lip, V73BRubber, V73BRubberDark);
            p.Line(V73BRubberDark, 0.02f, 0.05f, -0.8f, 0.4f, -0.7f, 0.66f, -0.45f, 0.78f, -0.1f);
            return new DoodleArt("v73_b_seal_peel", p);
        }

        /// <summary>Il pezzetto di calzino che spunta dalla guarnizione (centro 0,0): una punta verde con una stellina.</summary>
        public static DoodleArt V73B_SockPeek()
        {
            var p = new DoodlePainter(-0.2f, -0.15f, 0.2f, 0.15f, 160) { Ink = 0.025f };
            Sdf tip = Sd.Union(Sd.Ellipse(0f, 0f, 0.15f, 0.09f), Sd.Circle(0.07f, -0.03f, 0.07f));
            p.Fill(tip, V73BStarGreen);
            p.Fill(V73B_Star(0.02f, 0.0f, 0.065f, 12f), V73BStarYellow);
            p.Shape(tip, Rgba.Clear);
            return new DoodleArt("v73_b_sock_peek", p);
        }

        /// <summary>
        /// L'asciugatrice (centro 0,0; 1.9 x 2.16, verde menta): due manopole, la fessura del filtro dei pelucchi, l'oblò in
        /// (0, -0.12) con il cestello d'acciaio a costole (lo sportello è un disegno a parte).
        /// </summary>
        public static DoodleArt V73B_Dryer()
        {
            var p = new DoodlePainter(-1.02f, -1.2f, 1.02f, 1.13f, 100) { Ink = 0.045f };
            V73B_MachineBody(p, Rgba.Hex("DDEFE6"), Rgba.Hex("B5D3C4"), Rgba.Hex("CBE4D8"));
            foreach (float x in new[] { 0.3f, 0.65f })
            {
                p.Shape(Sd.Circle(x, 0.86f, 0.11f), Rgba.Hex("F3F5F8"), 0.03f);
                p.InkLine(0.022f, x, 0.86f, x, 0.95f);
            }
            p.Shape(Sd.Box(-0.45f, 0.86f, 0.35f, 0.05f, 0.03f), Rgba.Hex("8FAE9F"), 0.025f);
            for (int i = 0; i < 6; i++) p.Line(Rgba.Hex("2B2233", 0.5f), 0.015f, -0.72f + i * 0.11f, 0.84f, -0.72f + i * 0.11f, 0.88f);
            const float cy = -0.12f;
            p.Shape(Sd.Circle(0f, cy, 0.72f), Rgba.Hex("C9D1DB"), 0.035f);
            p.FillFunc(Sd.Circle(0f, cy, 0.6f), (x, y) => Rgba.Lerp(Rgba.Hex("C9D0D8"), Rgba.Hex("7F8893"), Math.Max(0f, Math.Min(1f, (float)Math.Sqrt(x * x + (y - cy) * (y - cy)) / 0.6f))));
            foreach (float a in new[] { 30f, 150f, 270f })
            {
                double r = a * Math.PI / 180.0;
                float ex = 0.58f * (float)Math.Cos(r), ey = cy + 0.58f * (float)Math.Sin(r);
                p.Shape(Sd.Capsule(ex * 0.72f, cy + (ey - cy) * 0.72f, ex, ey, 0.045f), Rgba.Hex("E6EAEF"), 0.025f);
            }
            p.Shape(Sd.Circle(0f, cy, 0.6f), Rgba.Clear, 0.03f);
            return new DoodleArt("v73_b_dryer", p);
        }

        /// <summary>Il coniglietto di pelucchi (centro 0,0; circa 0.5 x 0.4): batuffolo grigio con orecchie e occhietti.</summary>
        public static DoodleArt V73B_LintBunny()
        {
            var p = new DoodlePainter(-0.3f, -0.22f, 0.3f, 0.32f, 150) { Ink = 0.03f, Wobble = 0.02f };
            Rgba fluff = Rgba.Hex("C8C3CF"), fluffShade = Rgba.Hex("A59FAE");
            foreach (float s in new[] { -1f, 1f })
                p.Shape(Sd.Rotate(Sd.Ellipse(s * 0.08f, 0.17f, 0.045f, 0.11f), s * 0.08f, 0.1f, -s * 15f), fluff);
            Sdf blob = Sd.Union(Sd.Circle(0f, -0.02f, 0.17f), Sd.Circle(-0.13f, -0.06f, 0.1f), Sd.Circle(0.13f, -0.07f, 0.1f), Sd.Circle(0.05f, 0.07f, 0.11f));
            Body(p, blob, fluff, fluffShade);
            for (int i = 0; i < 8; i++)
            {
                double a = i * Math.PI / 4.0;
                float x = 0.2f * (float)Math.Cos(a), y = -0.03f + 0.15f * (float)Math.Sin(a);
                p.Line(fluffShade, 0.012f, x * 0.85f, y * 0.85f, x * 1.1f, y * 1.1f);
            }
            foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Circle(s * 0.055f, 0.0f, 0.022f), Ink);
            p.Fill(Sd.Circle(0f, -0.05f, 0.015f), Rgba.Hex("FF8FA3"));
            return new DoodleArt("v73_b_lint_bunny", p);
        }

        /// <summary>
        /// Lo stendino pieghevole (origine = centro della barra in alto, y 0; barra da -1.2 a 1.2): la seconda barra più
        /// in basso, le gambe a cavalletto fino a y -1.55 e il traverso.
        /// </summary>
        public static DoodleArt V73B_Rack()
        {
            var p = new DoodlePainter(-1.42f, -1.68f, 1.42f, 0.12f, 90) { Ink = 0.035f };
            Rgba bar = Rgba.Hex("EEF1F5");
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Polyline(0.035f, s * 1.15f, 0f, s * 1.32f, -1.55f), bar);
                p.Shape(Sd.Polyline(0.035f, s * 0.95f, -0.3f, s * 0.7f, -1.55f), bar);
                p.Shape(Sd.Box(s * 1.32f, -1.58f, 0.07f, 0.035f, 0.02f), Rgba.Hex("6B6677"), 0.02f);
                p.Shape(Sd.Box(s * 0.7f, -1.58f, 0.07f, 0.035f, 0.02f), Rgba.Hex("6B6677"), 0.02f);
            }
            p.Shape(Sd.Capsule(-1.26f, -1.0f, 1.26f, -1.0f, 0.03f), bar);
            p.Shape(Sd.Capsule(-1.0f, -0.3f, 1.0f, -0.3f, 0.035f), bar);
            p.Shape(Sd.Capsule(-1.22f, 0f, 1.22f, 0f, 0.045f), bar);
            return new DoodleArt("v73_b_rack", p);
        }

        /// <summary>La molletta di legno (centro 0,0; alta 0.24, la bocca in basso).</summary>
        public static DoodleArt V73B_Peg()
        {
            var p = new DoodlePainter(-0.08f, -0.14f, 0.08f, 0.14f, 200) { Ink = 0.02f };
            foreach (float s in new[] { -1f, 1f })
                p.Shape(Sd.Polygon(s * 0.015f, 0.12f, s * 0.055f, 0.12f, s * 0.045f, -0.12f, s * 0.008f, -0.12f), Rgba.Hex("E2B07A"));
            p.Shape(Sd.Box(0f, 0.02f, 0.06f, 0.025f, 0.01f), Rgba.Hex("B9C0C9"), 0.015f);
            return new DoodleArt("v73_b_peg", p);
        }

        /// <summary>Il bordo dietro della cesta di plastica (centro 0,0 = centro della bocca): l'interno in ombra.</summary>
        public static DoodleArt V73B_BasketBack()
        {
            var p = new DoodlePainter(-0.95f, -0.25f, 0.95f, 0.25f, 100) { Ink = 0.04f };
            p.Shape(Sd.Ellipse(0f, 0f, 0.88f, 0.2f), Rgba.Hex("3F86C2"));
            p.Fill(Sd.Ellipse(0f, -0.03f, 0.8f, 0.14f), Rgba.Hex("2D6699"));
            return new DoodleArt("v73_b_basket_back", p);
        }

        /// <summary>
        /// Il davanti della cesta di plastica azzurra (origine = centro della bocca, come v73_b_basket_back): il bordo
        /// arrotondato in y 0, le pareti fino a y -0.85 con i buchi ovali e le due maniglie.
        /// </summary>
        public static DoodleArt V73B_BasketFront()
        {
            var p = new DoodlePainter(-1.05f, -0.98f, 1.05f, 0.12f, 100) { Ink = 0.045f };
            p.Fill(Sd.Ellipse(0.05f, -0.9f, 0.85f, 0.08f), new Rgba(0f, 0f, 0f, 0.12f));
            Sdf body = Sd.Polygon(-0.88f, 0f, 0.88f, 0f, 0.7f, -0.86f, -0.7f, -0.86f);
            Body(p, body, Rgba.Hex("5CA8E0"), Rgba.Hex("3F86C2"));
            for (int r = 0; r < 3; r++)
                for (int c = -3; c <= 3; c++)
                {
                    float y = -0.28f - r * 0.2f, x = c * 0.2f * (1f - r * 0.05f);
                    p.Fill(Sd.Ellipse(x, y, 0.06f, 0.035f), Rgba.Hex("2D6699"));
                }
            p.Shape(Sd.Box(0f, -0.04f, 0.93f, 0.07f, 0.06f), Rgba.Hex("7BBCEB"));
            foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Ellipse(s * 0.66f, -0.04f, 0.13f, 0.03f), Rgba.Hex("2D6699"));
            return new DoodleArt("v73_b_basket_front", p);
        }

        /// <summary>La scatola del detersivo (origine = centro del fondo; 0.56 x 0.66): arancione con l'onda blu e le bolle.</summary>
        public static DoodleArt V73B_Detergent()
        {
            var p = new DoodlePainter(-0.36f, -0.06f, 0.36f, 0.74f, 140) { Ink = 0.035f };
            Sdf box = Sd.Box(0f, 0.33f, 0.28f, 0.33f, 0.03f);
            Body(p, box, Rgba.Hex("FF9A3C"), Rgba.Hex("E07A1F"));
            p.Fill(Sd.Intersect(box, Sd.Polygon(-0.3f, 0.12f, 0.3f, 0.3f, 0.3f, 0.0f, -0.3f, 0.0f)), Rgba.Hex("3E6FE0"));
            p.Shape(box, Rgba.Clear);
            p.Shape(Sd.Box(0f, 0.5f, 0.2f, 0.09f, 0.03f), Rgba.White, 0.02f);   // l'etichetta (la scritta la mette il livello)
            foreach (float[] b in new[] { new[] { -0.12f, 0.24f, 0.05f }, new[] { 0.1f, 0.2f, 0.035f }, new[] { 0.16f, 0.32f, 0.025f } })
                p.Shape(Sd.Circle(b[0], b[1], b[2]), Rgba.Hex("DDF3FF"), 0.015f);
            p.Shape(Sd.Box(-0.06f, 0.68f, 0.12f, 0.03f, 0.02f), Rgba.Hex("E07A1F"), 0.02f);
            return new DoodleArt("v73_b_detergent", p);
        }

        // ---------- I calzini ----------

        /// <summary>La sagoma del calzino: origine al centro del bordo in alto (y 0), gamba giù fino a y -0.6, piede a destra.</summary>
        static Sdf V73B_SockShape() => Sd.Union(Sd.Capsule(0f, -0.1f, 0f, -0.56f, 0.14f), Sd.Capsule(0f, -0.6f, 0.3f, -0.62f, 0.13f), Sd.Box(0f, -0.06f, 0.14f, 0.06f));

        static DoodleArt V73B_Sock(string name, Func<float, float, Rgba> pattern, Rgba cuff, Rgba toe, Action<DoodlePainter, Sdf> extra = null)
        {
            var p = new DoodlePainter(-0.2f, -0.8f, 0.48f, 0.06f, 150) { Ink = 0.03f };
            Sdf sock = V73B_SockShape();
            p.FillFunc(sock, pattern);
            extra?.Invoke(p, sock);
            // tallone e punta di un altro colore
            p.Fill(Sd.Intersect(sock, Sd.Circle(-0.1f, -0.66f, 0.13f)), toe);
            p.Fill(Sd.Intersect(sock, Sd.Circle(0.42f, -0.62f, 0.13f)), toe);
            p.Shape(sock, Rgba.Clear);
            Sdf band = Sd.Box(0f, -0.05f, 0.155f, 0.06f, 0.03f);
            p.Shape(band, cuff, 0.025f);
            for (int i = -2; i <= 2; i++) p.Line(Rgba.Hex("2B2233", 0.25f), 0.012f, i * 0.05f, -0.01f, i * 0.05f, -0.09f);
            return new DoodleArt(name, p);
        }

        /// <summary>Il calzino da accoppiare (e il suo gemello): verde con le stelline gialle.</summary>
        public static DoodleArt V73B_SockStars() => V73B_Sock("v73_b_sock_stars", (x, y) => V73BStarGreen, Rgba.Hex("2F8A3A"), Rgba.Hex("2F8A3A"), (p, s) =>
        {
            float[][] st = { new[] { -0.04f, -0.2f, 10f }, new[] { 0.05f, -0.42f, -12f }, new[] { -0.06f, -0.6f, 20f }, new[] { 0.22f, -0.6f, 0f } };
            foreach (float[] a in st) p.Fill(Sd.Intersect(s, V73B_Star(a[0], a[1], 0.08f, a[2])), V73BStarYellow);
        });

        /// <summary>Il calzino del gatto: stessi colori, ma a pois (quasi giusto).</summary>
        public static DoodleArt V73B_SockDots() => V73B_Sock("v73_b_sock_dots", (x, y) =>
        {
            float u = x / 0.12f - (float)Math.Floor(x / 0.12f) - 0.5f, v = y / 0.12f - (float)Math.Floor(y / 0.12f) - 0.5f;
            return u * u + v * v < 0.07f ? V73BStarYellow : V73BStarGreen;
        }, Rgba.Hex("2F8A3A"), Rgba.Hex("2F8A3A"));

        public static DoodleArt V73B_SockStripes() => V73B_Sock("v73_b_sock_stripes", (x, y) =>
            (int)Math.Floor(-y / 0.1f) % 2 == 0 ? Rgba.Hex("3E6FE0") : Rgba.White, Rgba.Hex("E8433F"), Rgba.Hex("E8433F"));

        public static DoodleArt V73B_SockArgyle() => V73B_Sock("v73_b_sock_argyle", (x, y) =>
        {
            float u = Math.Abs((x / 0.18f) - (float)Math.Floor(x / 0.18f) - 0.5f) + Math.Abs((y / 0.24f) - (float)Math.Floor(y / 0.24f) - 0.5f);
            return u < 0.42f ? Rgba.Hex("F28B30") : Rgba.Hex("7A4A2E");
        }, Rgba.Hex("F2D9B8"), Rgba.Hex("F2D9B8"));

        public static DoodleArt V73B_SockHearts() => V73B_Sock("v73_b_sock_hearts", (x, y) => Rgba.Hex("FFB3C7"), Rgba.Hex("E8433F"), Rgba.Hex("E8433F"), (p, s) =>
        {
            foreach (float[] a in new[] { new[] { -0.03f, -0.2f }, new[] { 0.05f, -0.45f }, new[] { 0.25f, -0.6f } })
                p.Fill(Sd.Intersect(s, V73B_Heart(a[0], a[1], 0.06f)), Rgba.Hex("E8433F"));
        });

        public static DoodleArt V73B_SockZigzag() => V73B_Sock("v73_b_sock_zigzag", (x, y) =>
        {
            float z = y + 0.06f * Math.Abs((x / 0.12f) - (float)Math.Floor(x / 0.12f + 0.5f)) * 2f;
            return Math.Abs(z / 0.16f - (float)Math.Floor(z / 0.16f) - 0.5f) < 0.18f ? Rgba.Hex("B6E34A") : Rgba.Hex("7B4FC9");
        }, Rgba.Hex("B6E34A"), Rgba.Hex("B6E34A"));

        public static DoodleArt V73B_SockRainbow() => V73B_Sock("v73_b_sock_rainbow", (x, y) =>
        {
            string[] c = { "E8433F", "FF9A3C", "FFD84A", "5BBF4A", "3E8EF7", "8E5BD9" };
            int i = Math.Max(0, (int)Math.Floor((-y + x * 0.3f) / 0.12f)) % c.Length;
            return Rgba.Hex(c[i]);
        }, Rgba.White, Rgba.White);

        public static DoodleArt V73B_SockChecks() => V73B_Sock("v73_b_sock_checks", (x, y) =>
            ((int)Math.Floor(x / 0.1f + 10f) + (int)Math.Floor(-y / 0.1f)) % 2 == 0 ? Rgba.Hex("2B2233") : Rgba.White, Rgba.Hex("E8433F"), Rgba.Hex("2B2233"));

        public static DoodleArt V73B_SockHole() => V73B_Sock("v73_b_sock_hole", (x, y) => Rgba.Hex("F2C46B"), Rgba.Hex("C98A2E"), Rgba.Hex("C98A2E"), (p, s) =>
            p.Shape(Sd.Circle(0.3f, -0.6f, 0.05f), Rgba.Hex("F2D9B8"), 0.015f));
    }
}
