using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.73 per il livello 271 "The Stuck Drawer" (il foglietto è "sticky_note", la lampada è
    /// "bedside_lamp" con "glow", l'asciugacapelli è "s41i_dryer", il calzino è "sock", le stelline "sparkle_star"):
    /// la camera moderna (sfondo intero), il cassettone bianco a frontali lisci, il frontale di un cassetto, l'interno
    /// del cassetto aperto, la maniglietta decorativa e i suoi due buchini, le chiavi della macchina, lo spray
    /// lubrificante e la sua nuvoletta, il luccichio del lubrificante, il cacciavite (dritto e piegato), l'aria calda
    /// dell'asciugacapelli, il tappeto. Prefisso v73_a_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV73A() => new DoodleArt[]
        {
            V73A_BedroomBg(), V73A_Dresser(), V73A_DrawerFront(), V73A_DrawerBox(), V73A_Handle(), V73A_HandleHoles(),
            V73A_CarKeys(), V73A_LubeCan(), V73A_Mist(), V73A_Shine(), V73A_Screwdriver(), V73A_ScrewdriverBent(),
            V73A_HotAir(), V73A_Rug(),
        };

        static readonly Rgba V73AWhite = Rgba.Hex("F4F2EE"), V73AWhiteShade = Rgba.Hex("D9D5CE");
        static readonly Rgba V73AOak = Rgba.Hex("D8B386"), V73AOakShade = Rgba.Hex("B88D5E");
        static readonly Rgba V73ABrass = Rgba.Hex("E2B85A"), V73ABrassShade = Rgba.Hex("B88A2E");

        /// <summary>Mezze misure del frontale di un cassetto (coordinate locali del frontale, centro 0,0).</summary>
        const float V73AFrontHx = 1.75f, V73AFrontHy = 0.26f;

        /// <summary>
        /// La camera moderna: parete grigio caldo liscia con una stampa astratta incorniciata (in 0.95, 1.45) e una
        /// mensolina con due vasetti a destra in alto, il battiscopa (y -1.95..-1.75), il parquet chiaro a listoni sotto.
        /// Coordinate del mondo.
        /// </summary>
        public static DoodleArt V73A_BedroomBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f, Wobble = 0.006f };
            Sdf wall = Sd.Box(0f, 2.6f, 5f, 4.45f);
            p.FillFunc(wall, (x, y) => Rgba.Lerp(Rgba.Hex("E4E0DA"), Rgba.Hex("EFECE7"), Math.Max(0f, Math.Min(1f, (y + 1.8f) / 6f))));
            // pannelli verticali appena accennati
            for (int i = -8; i <= 8; i++)
                p.Line(Rgba.Hex("D6D1C9", 0.6f), 0.02f, i * 1.1f, -1.75f, i * 1.1f, 7f);
            // la stampa astratta (cerchio senape, arco terracotta, linea blu)
            float fx = 0.95f, fy = 1.45f;
            p.Fill(Sd.Box(fx + 0.06f, fy - 0.06f, 0.62f, 0.78f), new Rgba(0f, 0f, 0f, 0.1f));
            p.Shape(Sd.Box(fx, fy, 0.62f, 0.78f, 0.02f), Rgba.Hex("2E2A33"), 0.03f);
            p.Fill(Sd.Box(fx, fy, 0.52f, 0.68f), Rgba.Hex("FBF8F2"), false);
            p.Fill(Sd.Circle(fx - 0.12f, fy + 0.22f, 0.24f), Rgba.Hex("E6B34A"));
            p.Fill(Sd.Intersect(Sd.Subtract(Sd.Circle(fx + 0.1f, fy - 0.35f, 0.34f), Sd.Circle(fx + 0.1f, fy - 0.35f, 0.2f)), Sd.Box(fx + 0.1f, fy - 0.15f, 0.4f, 0.2f)), Rgba.Hex("D97A55"));
            p.Line(Rgba.Hex("3F6E9E"), 0.05f, fx - 0.4f, fy - 0.05f, fx + 0.4f, fy + 0.12f);
            // la mensolina con due vasetti
            p.Shape(Sd.Box(-1.25f, 2.35f, 0.75f, 0.05f, 0.02f), V73AOak, 0.03f);
            p.Shape(Sd.Box(-1.6f, 2.55f, 0.13f, 0.15f, 0.05f), Rgba.Hex("9DB59A"), 0.03f);
            p.Shape(Sd.Ellipse(-1.6f, 2.8f, 0.2f, 0.12f), Rgba.Hex("6FA06A"), 0.03f);
            p.Shape(Sd.Box(-1.0f, 2.52f, 0.1f, 0.12f, 0.04f), Rgba.Hex("E8C9B8"), 0.03f);
            // battiscopa
            p.Fill(Sd.Box(0f, -1.85f, 5f, 0.1f), Rgba.Hex("F7F5F1"), false);
            p.Line(Rgba.Hex("2B2233", 0.6f), 0.04f, -4.5f, -1.75f, 4.5f, -1.75f);
            p.Line(Rgba.Hex("2B2233", 0.5f), 0.03f, -4.5f, -1.95f, 4.5f, -1.95f);
            // parquet chiaro
            Sdf floor = Sd.Box(0f, -4.5f, 5f, 2.55f);
            p.FillFunc(floor, (x, y) =>
            {
                int row = (int)Math.Floor((-1.95f - y) / 0.38f);
                float off = (row % 3) * 0.8f;
                float u = (x + off) / 2.4f - (float)Math.Floor((x + off) / 2.4f);
                Rgba a = row % 2 == 0 ? Rgba.Hex("E2C9A4") : Rgba.Hex("DCC29C");
                if (u < 0.015f) a = Rgba.Hex("C7AB82");
                return Rgba.Lerp(a, Rgba.Hex("EAD6B6"), Math.Max(0f, Math.Min(1f, (-1.95f - y) / 5f)));
            });
            for (int r = 1; r < 14; r++)
                p.Line(Rgba.Hex("C7AB82", 0.7f), 0.02f, -4.5f, -1.95f - r * 0.38f, 4.5f, -1.95f - r * 0.38f);
            return new DoodleArt("v73_a_bedroom_bg", p);
        }

        /// <summary>
        /// Il cassettone moderno (centro 0,0): corpo bianco opaco x ±1.9, y -0.95..0.84, piano di rovere 0.84..0.98,
        /// gambette fino a -1.2. Le tre sedi dei cassetti (scure, si vedono solo nelle fessure e quando un cassetto
        /// esce) sono centrate in y 0.56, 0, -0.56; i frontali sono un disegno a parte (v73_a_drawer_front).
        /// </summary>
        public static DoodleArt V73A_Dresser()
        {
            var p = new DoodlePainter(-2.02f, -1.28f, 2.02f, 1.08f, 90) { Ink = 0.045f };
            p.Fill(Sd.Box(0.06f, -1.2f, 1.85f, 0.06f), new Rgba(0f, 0f, 0f, 0.12f));
            foreach (float x in new[] { -1.65f, 1.65f })
                p.Shape(Sd.Polygon(x - 0.08f, -0.9f, x + 0.08f, -0.9f, x + 0.04f, -1.2f, x - 0.02f, -1.2f), Rgba.Hex("4A4048"), 0.03f);
            Body(p, Sd.Box(0f, -0.06f, 1.9f, 0.9f, 0.04f), V73AWhite, V73AWhiteShade);
            // il piano di rovere
            Body(p, Sd.Box(0f, 0.91f, 1.98f, 0.075f, 0.03f), V73AOak, V73AOakShade);
            p.Line(Rgba.White.WithAlpha(0.35f), 0.02f, -1.7f, 0.95f, 0.6f, 0.95f);
            // le sedi dei cassetti
            foreach (float y in new[] { 0.56f, 0f, -0.56f })
                p.Fill(Sd.Box(0f, y, V73AFrontHx + 0.03f, V73AFrontHy + 0.03f, 0.02f), Rgba.Hex("3A3442"), false);
            return new DoodleArt("v73_a_dresser", p);
        }

        /// <summary>Il frontale liscio di un cassetto (centro 0,0; 3.5 x 0.52), senza maniglia: push-to-open.</summary>
        public static DoodleArt V73A_DrawerFront()
        {
            var p = new DoodlePainter(-1.82f, -0.33f, 1.82f, 0.33f, 90) { Ink = 0.035f };
            Sdf f = Sd.Box(0f, 0f, V73AFrontHx, V73AFrontHy, 0.03f);
            p.FillFunc(f, (x, y) => Rgba.Lerp(Rgba.Hex("E9E6E0"), Rgba.Hex("FAF9F6"), Math.Max(0f, Math.Min(1f, (y + 0.26f) / 0.52f))));
            p.Line(Rgba.White.WithAlpha(0.8f), 0.02f, -1.7f, 0.22f, 1.7f, 0.22f);
            p.Line(V73AWhiteShade, 0.02f, -1.7f, -0.22f, 1.7f, -0.22f);
            p.Shape(f, Rgba.Clear);
            return new DoodleArt("v73_a_drawer_front", p);
        }

        /// <summary>
        /// L'interno del cassetto aperto visto un po' dall'alto (bordo basso in y 0, largo come il frontale): il fondo di
        /// rovere chiaro che si stringe verso il fondo (y 0.5), i fianchi bianchi, l'ombra in fondo.
        /// </summary>
        public static DoodleArt V73A_DrawerBox()
        {
            var p = new DoodlePainter(-1.82f, -0.08f, 1.82f, 0.6f, 90) { Ink = 0.035f };
            Sdf outer = Sd.Polygon(-1.75f, 0f, 1.75f, 0f, 1.6f, 0.52f, -1.6f, 0.52f);
            p.Shape(outer, V73AWhite);
            Sdf floor = Sd.Polygon(-1.66f, 0.02f, 1.66f, 0.02f, 1.52f, 0.44f, -1.52f, 0.44f);
            p.FillFunc(floor, (x, y) => Rgba.Lerp(Rgba.Hex("E7CDA6"), Rgba.Hex("B89366"), Math.Max(0f, Math.Min(1f, y / 0.44f))));
            p.Fill(Sd.Polygon(-1.52f, 0.36f, 1.52f, 0.36f, 1.52f, 0.44f, -1.52f, 0.44f), new Rgba(0f, 0f, 0f, 0.18f));
            p.Shape(outer, Rgba.Clear);
            return new DoodleArt("v73_a_drawer_box", p);
        }

        /// <summary>La maniglietta decorativa (centro 0,0): una barretta d'ottone 0.42 x 0.08 su due piedini.</summary>
        public static DoodleArt V73A_Handle()
        {
            var p = new DoodlePainter(-0.28f, -0.1f, 0.28f, 0.1f, 200) { Ink = 0.025f };
            foreach (float x in new[] { -0.15f, 0.15f }) p.Shape(Sd.Box(x, -0.02f, 0.03f, 0.04f, 0.01f), V73ABrassShade, 0.018f);
            Body(p, Sd.Box(0f, 0.025f, 0.22f, 0.035f, 0.03f), V73ABrass, V73ABrassShade);
            p.Line(Rgba.White.WithAlpha(0.7f), 0.012f, -0.15f, 0.04f, 0.1f, 0.04f);
            return new DoodleArt("v73_a_handle", p);
        }

        /// <summary>I due buchini lasciati dalla maniglia staccata (centro 0,0, stessi punti dei piedini).</summary>
        public static DoodleArt V73A_HandleHoles()
        {
            var p = new DoodlePainter(-0.22f, -0.08f, 0.22f, 0.06f, 200) { Ink = 0.015f };
            foreach (float x in new[] { -0.15f, 0.15f }) p.Shape(Sd.Circle(x, -0.02f, 0.025f), Rgba.Hex("3A3442"));
            return new DoodleArt("v73_a_handle_holes", p);
        }

        /// <summary>Le chiavi della macchina (centro 0,0; ~0.8 x 0.45): anello, telecomandino nero coi tasti, chiave.</summary>
        public static DoodleArt V73A_CarKeys()
        {
            var p = new DoodlePainter(-0.45f, -0.28f, 0.48f, 0.28f, 180) { Ink = 0.025f };
            p.Shape(Sd.Subtract(Sd.Circle(-0.02f, 0.06f, 0.11f), Sd.Circle(-0.02f, 0.06f, 0.075f)), Rgba.Hex("C9CFD9"));
            // il telecomandino
            Sdf fob = Sd.Rotate(Sd.Box(-0.24f, -0.05f, 0.16f, 0.1f, 0.08f), -0.24f, -0.05f, 20f);
            Body(p, fob, Rgba.Hex("3A3442"), Rgba.Hex("26212D"));
            p.Fill(Sd.Circle(-0.29f, -0.07f, 0.03f), Rgba.Hex("E0352B"));
            p.Fill(Sd.Circle(-0.2f, -0.04f, 0.03f), Rgba.Hex("C9CFD9"));
            // la chiave
            p.Shape(Sd.Circle(0.12f, -0.02f, 0.07f), Rgba.Hex("D9DEE8"), 0.02f);
            p.Shape(Sd.Polygon(0.17f, -0.05f, 0.42f, -0.12f, 0.43f, -0.07f, 0.19f, 0.01f), Rgba.Hex("D9DEE8"), 0.02f);
            p.Line(Rgba.Hex("8E97A8"), 0.012f, 0.25f, -0.06f, 0.4f, -0.1f);
            p.Line(Rgba.White.WithAlpha(0.8f), 0.012f, 0.09f, 0.0f, 0.12f, 0.02f);
            return new DoodleArt("v73_a_car_keys", p);
        }

        /// <summary>Lo spray lubrificante (base in 0,0; alto 0.9): bomboletta blu e gialla con la cannuccia rossa.</summary>
        public static DoodleArt V73A_LubeCan()
        {
            var p = new DoodlePainter(-0.3f, -0.06f, 0.42f, 1.0f, 140) { Ink = 0.035f };
            p.Fill(Sd.Ellipse(0.03f, 0f, 0.22f, 0.04f), new Rgba(0f, 0f, 0f, 0.15f));
            p.Shape(Sd.Box(0f, 0.86f, 0.06f, 0.05f, 0.02f), Rgba.Hex("C9CFD9"), 0.025f);
            p.Line(Rgba.Hex("E0352B"), 0.03f, 0.05f, 0.88f, 0.36f, 0.93f);
            p.Shape(Sd.Ellipse(0f, 0.78f, 0.17f, 0.06f), Rgba.Hex("C9CFD9"), 0.025f);
            Sdf can = Sd.Box(0f, 0.4f, 0.19f, 0.38f, 0.05f);
            Body(p, can, Rgba.Hex("3F6E9E"), Rgba.Hex("2D527A"));
            p.Fill(Sd.Intersect(can, Sd.Box(0f, 0.42f, 0.2f, 0.14f)), Rgba.Hex("F2C14E"));
            p.Line(Rgba.Hex("2B2233"), 0.03f, -0.12f, 0.45f, 0.12f, 0.45f);
            p.Line(Rgba.Hex("2B2233", 0.7f), 0.02f, -0.12f, 0.37f, 0.06f, 0.37f);
            p.Line(Rgba.White.WithAlpha(0.4f), 0.03f, -0.12f, 0.1f, -0.12f, 0.7f);
            p.Shape(can, Rgba.Clear);
            return new DoodleArt("v73_a_lube_can", p);
        }

        /// <summary>La nuvoletta dello spray (centro 0,0; ~0.9 x 0.6): sbuffi bianchi trasparenti.</summary>
        public static DoodleArt V73A_Mist()
        {
            var p = new DoodlePainter(-0.5f, -0.35f, 0.5f, 0.35f, 100) { Ink = 0.02f };
            p.Fill(Sd.Union(Sd.Circle(-0.2f, 0f, 0.2f), Sd.Circle(0.05f, 0.08f, 0.22f), Sd.Circle(0.25f, -0.05f, 0.18f), Sd.Circle(0f, -0.12f, 0.17f)), Rgba.Hex("E8F1FA", 0.75f));
            foreach (var d in new[] { (-0.38f, 0.2f), (0.4f, 0.18f), (0.1f, 0.28f), (-0.3f, -0.25f) })
                p.Fill(Sd.Circle(d.Item1, d.Item2, 0.04f), Rgba.Hex("E8F1FA", 0.8f));
            return new DoodleArt("v73_a_mist", p);
        }

        /// <summary>Il luccichio del lubrificante su un frontale (centro 0,0; 3.5 x 0.52): strisce oblique bianche.</summary>
        public static DoodleArt V73A_Shine()
        {
            var p = new DoodlePainter(-1.8f, -0.3f, 1.8f, 0.3f, 80) { Ink = 0.02f };
            Sdf f = Sd.Box(0f, 0f, V73AFrontHx - 0.03f, V73AFrontHy - 0.03f, 0.02f);
            foreach (float x in new[] { -1.2f, -1.0f, 0.3f, 0.45f, 1.3f })
            {
                float w = x == -1.0f || x == 0.45f ? 0.03f : 0.07f;
                p.Fill(Sd.Intersect(f, Sd.Rotate(Sd.Box(x, 0f, w, 0.5f), x, 0f, -30f)), Rgba.White.WithAlpha(0.75f));
            }
            p.Fill(Sd.Intersect(f, Sd.Box(0f, 0.17f, 1.8f, 0.04f)), Rgba.White.WithAlpha(0.35f));
            return new DoodleArt("v73_a_shine", p);
        }

        /// <summary>Il cacciavite a taglio (centro 0,0; lungo 1.2, punta a destra): manico arancione, stelo d'acciaio.</summary>
        public static DoodleArt V73A_Screwdriver()
        {
            var p = new DoodlePainter(-0.65f, -0.16f, 0.65f, 0.16f, 150) { Ink = 0.03f };
            p.Shape(Sd.Box(0.22f, 0f, 0.36f, 0.03f, 0.01f), Rgba.Hex("C9CFD9"));
            p.Shape(Sd.Polygon(0.56f, 0.03f, 0.62f, 0.015f, 0.62f, -0.015f, 0.56f, -0.03f), Rgba.Hex("C9CFD9"), 0.02f);
            V73A_ScrewdriverHandle(p);
            return new DoodleArt("v73_a_screwdriver", p);
        }

        /// <summary>Lo stesso cacciavite con lo stelo piegato a zig-zag (dopo la fessura troppo sottile).</summary>
        public static DoodleArt V73A_ScrewdriverBent()
        {
            var p = new DoodlePainter(-0.65f, -0.3f, 0.65f, 0.3f, 150) { Ink = 0.03f };
            p.Shape(Sd.Polyline(0.03f, -0.15f, 0f, 0.08f, 0f, 0.2f, 0.18f, 0.32f, -0.12f, 0.46f, 0.12f), Rgba.Hex("C9CFD9"));
            p.Shape(Sd.Circle(0.47f, 0.13f, 0.035f), Rgba.Hex("C9CFD9"), 0.02f);
            V73A_ScrewdriverHandle(p);
            return new DoodleArt("v73_a_screwdriver_bent", p);
        }

        static void V73A_ScrewdriverHandle(DoodlePainter p)
        {
            Sdf h = Sd.Box(-0.38f, 0f, 0.24f, 0.1f, 0.08f);
            Body(p, h, Rgba.Hex("F28C38"), Rgba.Hex("C9651B"));
            foreach (float x in new[] { -0.5f, -0.38f, -0.26f }) p.Line(Rgba.Hex("2B2233", 0.5f), 0.015f, x, -0.06f, x, 0.06f);
            p.Shape(Sd.Box(-0.1f, 0f, 0.05f, 0.06f, 0.02f), Rgba.Hex("2B2233"), 0.02f);
        }

        /// <summary>L'aria calda dell'asciugacapelli (parte da 0,0 e va in giù, ~0.6 x 1.0): tre righe ondulate.</summary>
        public static DoodleArt V73A_HotAir()
        {
            var p = new DoodlePainter(-0.4f, -1.05f, 0.4f, 0.05f, 100) { Ink = 0.02f };
            for (int i = -1; i <= 1; i++)
            {
                var pts = new List<float>();
                for (int k = 0; k <= 12; k++)
                {
                    float t = k / 12f;
                    pts.Add(i * (0.08f + 0.18f * t) + 0.05f * (float)Math.Sin(t * 12f + i));
                    pts.Add(-0.05f - t * 0.95f);
                }
                p.Line(Rgba.Hex("FF9A5A", 0.85f), 0.035f, pts.ToArray());
            }
            return new DoodleArt("v73_a_hot_air", p);
        }

        /// <summary>Il tappeto moderno (centro 0,0; 4.4 x 1.3 in prospettiva): rettangolo a righe grigio e senape.</summary>
        public static DoodleArt V73A_Rug()
        {
            var p = new DoodlePainter(-2.35f, -0.75f, 2.35f, 0.75f, 70) { Ink = 0.04f };
            Sdf rug = Sd.Polygon(-1.9f, 0.62f, 1.9f, 0.62f, 2.2f, -0.62f, -2.2f, -0.62f);
            p.Fill(Sd.Offset(rug, 0.05f, -0.05f), new Rgba(0f, 0f, 0f, 0.08f));
            p.FillFunc(rug, (x, y) =>
            {
                int band = (int)Math.Floor((y + 0.62f) / 0.2f);
                return band == 2 || band == 3 ? Rgba.Hex("E6B34A") : band % 2 == 0 ? Rgba.Hex("A8A6B0") : Rgba.Hex("BDBBC4");
            });
            p.Shape(rug, Rgba.Clear);
            for (int i = 0; i < 18; i++)
            {
                float x = -2.1f + i * 0.247f;
                p.Line(Rgba.Hex("8E8C97"), 0.02f, x, -0.62f, x, -0.7f);
            }
            return new DoodleArt("v73_a_rug", p);
        }
    }
}
