using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.72 per il livello 270 "The Lost Remote" (il gatto che dorme è "cat", l'orologio è
    /// v63_clock_face con le lancette fatte di barre, presa e spina sono v67_socket / v67_plug, i ritrovamenti sotto i
    /// cuscini sono coin, sock, v40b_crumbs e mouse, la pianta è pot + gs_plant): il soggiorno (sfondo intero), il
    /// televisore a tubo con i due tastini sul fianco destro, il mobiletto, i programmi (la vernice che asciuga, la disco
    /// dei gatti col ballerino, l'effetto neve), il divano senza cuscini e il cuscino della seduta, il tavolino, il
    /// telecomando, la pila stilo, il libretto delle istruzioni chiuso e la sua pagina aperta, gli occhi aperti del gatto.
    /// Prefisso v72_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV72B() => new DoodleArt[]
        {
            V72B_LivingBg(), V72B_Tv(), V72B_Cabinet(), V72B_ShowPaint(), V72B_ShowDisco(), V72B_Dancer(), V72B_Static(),
            V72B_Sofa(), V72B_Cushion(), V72B_Table(), V72B_Remote(), V72B_Battery(), V72B_Manual(), V72B_ManualPage(),
            V72B_CatEyes(),
        };

        static readonly Rgba V72BTvBody = Rgba.Hex("B8875A"), V72BTvShade = Rgba.Hex("8E6440");
        static readonly Rgba V72BPlastic = Rgba.Hex("3A3442"), V72BPlasticShade = Rgba.Hex("26212D");
        static readonly Rgba V72BWood = Rgba.Hex("A8693F"), V72BWoodShade = Rgba.Hex("7E4B2B");
        static readonly Rgba V72BSofa = Rgba.Hex("4F9C8F"), V72BSofaShade = Rgba.Hex("3A7A6F");

        /// <summary>Lo schermo del televisore (coordinate locali del disegno V72B_Tv): centro e mezze misure.</summary>
        const float V72BScrX = -0.04f, V72BScrY = 0.08f, V72BScrHx = 1.05f, V72BScrHy = 0.72f;

        static Sdf V72B_ScreenShape(float grow = 0f) => Sd.Box(0f, 0f, V72BScrHx + grow, V72BScrHy + grow, 0.16f);

        /// <summary>
        /// Il soggiorno: carta da parati a righe e rombi fino al battiscopa (y = 0.15..0.35), parquet sotto, un tappeto
        /// tondo davanti al divano. Coordinate del mondo (come kitchen_bg).
        /// </summary>
        public static DoodleArt V72B_LivingBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f, Wobble = 0.006f };
            // parete: righe verticali verde salvia con i rombini
            Sdf wall = Sd.Box(0f, 3.7f, 5f, 3.45f);
            p.FillFunc(wall, (x, y) =>
            {
                float u = x / 0.5f - (float)Math.Floor(x / 0.5f);
                return u < 0.5f ? Rgba.Hex("DCE8D2") : Rgba.Hex("CFDFC4");
            });
            for (int i = -9; i <= 9; i++)
                for (int j = 0; j < 13; j++)
                {
                    float dx = i * 0.5f + 0.25f, dy = 0.75f + j * 0.55f + (i % 2 == 0 ? 0f : 0.27f);
                    p.Fill(Sd.Polygon(dx, dy + 0.07f, dx + 0.05f, dy, dx, dy - 0.07f, dx - 0.05f, dy), Rgba.Hex("A9C49B", 0.8f));
                }
            // battiscopa
            p.Fill(Sd.Box(0f, 0.25f, 5f, 0.11f), Rgba.Hex("F4EDE0"), false);
            p.Line(Rgba.Hex("2B2233", 0.7f), 0.04f, -4.5f, 0.36f, 4.5f, 0.36f);
            p.Line(Rgba.Hex("2B2233", 0.5f), 0.03f, -4.5f, 0.14f, 4.5f, 0.14f);
            // parquet a listoni (si schiarisce verso il basso)
            Sdf floor = Sd.Box(0f, -3.45f, 5f, 3.6f);
            p.FillFunc(floor, (x, y) =>
            {
                int row = (int)Math.Floor((0.14f - y) / 0.42f);
                float off = (row % 3) * 0.7f;
                float u = (x + off) / 2.1f - (float)Math.Floor((x + off) / 2.1f);
                Rgba a = row % 2 == 0 ? Rgba.Hex("D9A86C") : Rgba.Hex("D19E62");
                if (u < 0.02f) a = Rgba.Hex("B9864F");
                return Rgba.Lerp(a, Rgba.Hex("E3B97F"), Math.Max(0f, Math.Min(1f, (-y) / 7f)));
            });
            for (int r = 0; r < 17; r++)
                p.Line(Rgba.Hex("B9864F", 0.8f), 0.025f, -4.5f, 0.14f - r * 0.42f, 4.5f, 0.14f - r * 0.42f);
            // il tappeto tondo davanti al divano
            p.Fill(Sd.Ellipse(0.1f, -1.85f, 2.6f, 0.95f), new Rgba(0f, 0f, 0f, 0.08f));
            p.Shape(Sd.Ellipse(0f, -1.8f, 2.5f, 0.9f), Rgba.Hex("E38B6B"));
            p.Shape(Sd.Ellipse(0f, -1.8f, 2.05f, 0.68f), Rgba.Hex("F2C46B"), 0.035f);
            p.Shape(Sd.Ellipse(0f, -1.8f, 1.45f, 0.45f), Rgba.Hex("E38B6B"), 0.035f);
            return new DoodleArt("v72_b_living_bg", p);
        }

        /// <summary>
        /// Il televisore a tubo visto un po' di tre quarti (centro 0,0): cassa di legno 2.7 x 1.96, lo schermo spento
        /// (vetro scuro con il riflesso; i programmi sono disegni a parte sopra), la fascia sotto con la griglia
        /// dell'altoparlante, le antenne a orecchie di coniglio e il fianco destro (x 1.35..1.62) con i due tastini
        /// piccoli "+" (y 0.28) e "-" (y -0.12) sotto la scritta "CH" a righine.
        /// </summary>
        public static DoodleArt V72B_Tv()
        {
            var p = new DoodlePainter(-1.5f, -1.12f, 1.72f, 1.45f, 110) { Ink = 0.045f };
            // antenne
            p.InkLine(0.035f, -0.1f, 0.98f, -0.7f, 1.38f);
            p.InkLine(0.035f, 0.1f, 0.98f, 0.62f, 1.4f);
            p.Shape(Sd.Circle(-0.7f, 1.38f, 0.05f), Rgba.Hex("D9D4CB"), 0.025f);
            p.Shape(Sd.Circle(0.62f, 1.4f, 0.05f), Rgba.Hex("D9D4CB"), 0.025f);
            p.Shape(Sd.Ellipse(0f, 0.99f, 0.22f, 0.09f), V72BPlastic, 0.035f);
            // il fianco destro, in prospettiva
            Sdf side = Sd.Polygon(1.3f, 0.98f, 1.62f, 0.86f, 1.62f, -0.86f, 1.3f, -0.98f);
            p.Shape(side, V72BTvShade);
            p.Fill(Sd.Polygon(1.34f, 0.9f, 1.58f, 0.8f, 1.58f, 0.7f, 1.34f, 0.78f), Rgba.White.WithAlpha(0.12f));
            // la cassa
            Sdf body = Sd.Box(0f, 0f, 1.35f, 0.98f, 0.14f);
            Body(p, body, V72BTvBody, V72BTvShade);
            // venature
            for (int i = 0; i < 4; i++)
                p.Line(V72BTvShade.WithAlpha(0.5f), 0.015f, -1.25f, 0.9f - i * 0.03f, -1.12f, 0.92f - i * 0.03f);
            // la cornice e lo schermo spento (vetro bombato)
            Sdf bezel = Sd.Offset(V72B_ScreenShape(0.09f), V72BScrX, V72BScrY);
            p.Shape(bezel, Rgba.Hex("2E2A33"), 0.035f);
            Sdf glass = Sd.Offset(V72B_ScreenShape(), V72BScrX, V72BScrY);
            p.FillFunc(glass, (x, y) => Rgba.Lerp(Rgba.Hex("4A5A57"), Rgba.Hex("1F2826"), Math.Max(0f, Math.Min(1f, (V72BScrY + 0.72f - y) / 1.44f))));
            p.Fill(Sd.Intersect(glass, Sd.Rotate(Sd.Box(-0.45f, 0.42f, 0.55f, 0.08f), -0.45f, 0.42f, 20f)), Rgba.White.WithAlpha(0.18f));
            p.Shape(glass, Rgba.Clear, 0.03f);
            // fascia sotto: griglia dell'altoparlante e la manopola del volume (finta)
            for (int i = 0; i < 9; i++)
            {
                float x = -1.1f + i * 0.13f;
                p.Line(Rgba.Hex("5B4128"), 0.035f, x, -0.83f, x, -0.9f);
            }
            p.Shape(Sd.Circle(0.98f, -0.85f, 0.07f), Rgba.Hex("D9D4CB"), 0.025f);
            // i tastini sul fianco: piccoli ma visibili, con "CH" a righine sopra
            float bx = 1.47f;
            p.Line(Rgba.Hex("F4EDE0"), 0.02f, bx - 0.05f, 0.58f, bx - 0.09f, 0.55f, bx - 0.09f, 0.48f, bx - 0.05f, 0.45f);
            p.Line(Rgba.Hex("F4EDE0"), 0.02f, bx - 0.01f, 0.58f, bx - 0.01f, 0.45f);
            p.Line(Rgba.Hex("F4EDE0"), 0.02f, bx + 0.07f, 0.58f, bx + 0.07f, 0.45f);
            p.Line(Rgba.Hex("F4EDE0"), 0.02f, bx - 0.01f, 0.515f, bx + 0.07f, 0.515f);
            foreach (float by in new[] { 0.28f, -0.12f })
            {
                p.Shape(Sd.Ellipse(bx, by, 0.085f, 0.1f), Rgba.Hex("E9E3D6"), 0.025f);
                p.Fill(Sd.Ellipse(bx - 0.02f, by + 0.03f, 0.03f, 0.03f), Rgba.White.WithAlpha(0.8f));
            }
            p.Line(Rgba.Hex("2B2233"), 0.02f, bx - 0.04f, 0.28f, bx + 0.04f, 0.28f);
            p.Line(Rgba.Hex("2B2233"), 0.02f, bx, 0.24f, bx, 0.32f);
            p.Line(Rgba.Hex("2B2233"), 0.02f, bx - 0.04f, -0.12f, bx + 0.04f, -0.12f);
            return new DoodleArt("v72_b_tv", p);
        }

        /// <summary>Il mobiletto basso del televisore (centro 0,0; 3.1 x 0.6) con due cassetti e i piedini.</summary>
        public static DoodleArt V72B_Cabinet()
        {
            var p = new DoodlePainter(-1.65f, -0.42f, 1.65f, 0.36f, 90) { Ink = 0.045f };
            p.Fill(Sd.Box(0.05f, -0.33f, 1.55f, 0.05f), new Rgba(0f, 0f, 0f, 0.12f));
            foreach (float x in new[] { -1.35f, 1.35f }) p.Shape(Sd.Box(x, -0.33f, 0.07f, 0.06f, 0.02f), V72BWoodShade, 0.03f);
            Body(p, Sd.Box(0f, 0f, 1.55f, 0.28f, 0.05f), V72BWood, V72BWoodShade);
            p.Line(Rgba.Hex("2B2233", 0.6f), 0.03f, -1.52f, 0.2f, 1.52f, 0.2f);
            foreach (float x in new[] { -0.75f, 0.75f })
            {
                p.Shape(Sd.Box(x, -0.04f, 0.62f, 0.17f, 0.03f), Rgba.Hex("B87A4C"), 0.03f);
                p.Shape(Sd.Box(x, -0.04f, 0.12f, 0.03f, 0.03f), Rgba.Hex("E8C66A"), 0.02f);
            }
            return new DoodleArt("v72_b_cabinet", p);
        }

        /// <summary>
        /// Il programma orribile (centro 0,0 = centro dello schermo, 2.1 x 1.44): la fascia scura in alto per il titolo
        /// (lo scrive il livello), un tizio seduto sullo sgabello che fissa un muro verde con la vernice fresca
        /// (cartello WET PAINT a righine, una goccia), il barattolo col pennello, il bollino rosso "LIVE".
        /// </summary>
        public static DoodleArt V72B_ShowPaint()
        {
            var p = new DoodlePainter(-1.08f, -0.75f, 1.08f, 0.75f, 120) { Ink = 0.03f };
            Sdf scr = V72B_ScreenShape();
            // stanza beige e pavimento grigio
            p.FillFunc(scr, (x, y) => y > -0.38f ? Rgba.Hex("E9DCC2") : Rgba.Hex("A79F92"));
            // il muro dipinto (verde lucido) a destra, con le gocce
            Sdf paint = Sd.Intersect(scr, Sd.Box(0.48f, 0.04f, 0.5f, 0.42f));
            p.Fill(paint, Rgba.Hex("7DBE6A"));
            p.Fill(Sd.Intersect(scr, Sd.Box(0.48f, 0.3f, 0.5f, 0.06f)), Rgba.White.WithAlpha(0.25f));
            p.Fill(Sd.Capsule(0.2f, -0.38f, 0.2f, -0.46f, 0.025f), Rgba.Hex("7DBE6A"));
            p.Fill(Sd.Capsule(0.72f, -0.38f, 0.72f, -0.43f, 0.02f), Rgba.Hex("7DBE6A"));
            // cartello WET PAINT (righine al posto delle lettere)
            p.Shape(Sd.Box(0.5f, 0.05f, 0.24f, 0.11f, 0.02f), Rgba.Hex("FFF4B8"), 0.02f);
            p.Line(Rgba.Hex("2B2233", 0.8f), 0.02f, 0.33f, 0.09f, 0.66f, 0.09f);
            p.Line(Rgba.Hex("D9452B"), 0.02f, 0.33f, 0.01f, 0.6f, 0.01f);
            // il barattolo e il pennello per terra
            p.Shape(Sd.Box(0.05f, -0.5f, 0.1f, 0.1f, 0.02f), Rgba.Hex("C9CFD9"), 0.02f);
            p.Fill(Sd.Box(0.05f, -0.42f, 0.1f, 0.025f), Rgba.Hex("7DBE6A"));
            p.Line(Rgba.Hex("8E6440"), 0.03f, 0.1f, -0.38f, 0.22f, -0.22f);
            // il tizio sullo sgabello, di profilo, che guarda il muro
            p.InkLine(0.025f, -0.72f, -0.62f, -0.66f, -0.28f);
            p.InkLine(0.025f, -0.42f, -0.62f, -0.48f, -0.28f);
            p.Shape(Sd.Box(-0.57f, -0.27f, 0.2f, 0.035f, 0.02f), Rgba.Hex("B8875A"), 0.02f);
            Sdf torso = Sd.Box(-0.58f, -0.08f, 0.13f, 0.2f, 0.07f);
            p.Shape(torso, Rgba.Hex("6C8EBF"));
            p.Shape(Sd.Capsule(-0.5f, -0.24f, -0.3f, -0.28f, 0.05f), Rgba.Hex("4A5A7A"), 0.02f);
            p.Shape(Sd.Capsule(-0.3f, -0.28f, -0.3f, -0.55f, 0.045f), Rgba.Hex("4A5A7A"), 0.02f);
            p.Shape(Sd.Capsule(-0.52f, -0.02f, -0.4f, -0.16f, 0.035f), Rgba.Hex("6C8EBF"), 0.02f);
            p.Shape(Sd.Circle(-0.56f, 0.26f, 0.13f), Rgba.Hex("F2C9A0"));
            p.Fill(Sd.Intersect(Sd.Circle(-0.56f, 0.26f, 0.13f), Sd.Box(-0.6f, 0.36f, 0.14f, 0.06f)), Rgba.Hex("6B4A33"));
            p.Fill(Sd.Circle(-0.49f, 0.27f, 0.018f), Rgba.Hex("2B2233"));
            p.Line(Rgba.Hex("2B2233"), 0.012f, -0.52f, 0.2f, -0.46f, 0.2f);
            // la fascia del titolo e il bollino LIVE
            p.Fill(Sd.Intersect(scr, Sd.Box(0f, 0.62f, 1.1f, 0.13f)), Rgba.Hex("1A1229", 0.85f));
            p.Shape(Sd.Box(0.82f, -0.6f, 0.14f, 0.06f, 0.03f), Rgba.Hex("E0352B"), 0.015f);
            p.Fill(Sd.Circle(0.73f, -0.6f, 0.02f), Rgba.White);
            p.Line(Rgba.White, 0.015f, 0.78f, -0.58f, 0.92f, -0.58f);
            p.Line(Rgba.White, 0.015f, 0.78f, -0.62f, 0.9f, -0.62f);
            return new DoodleArt("v72_b_show_paint", p);
        }

        /// <summary>
        /// Il programma bello (centro 0,0, 2.1 x 1.44): la disco dei gatti. Sfondo viola con i fasci di luce colorati, la
        /// palla a specchi appesa, il pavimento a quadrotti luminosi e le stelline. Il ballerino è un disegno a parte
        /// (v72_b_dancer, così il livello lo fa ballare); la fascia del titolo in alto come nell'altro programma.
        /// </summary>
        public static DoodleArt V72B_ShowDisco()
        {
            var p = new DoodlePainter(-1.08f, -0.75f, 1.08f, 0.75f, 120) { Ink = 0.03f };
            Sdf scr = V72B_ScreenShape();
            p.FillFunc(scr, (x, y) => Rgba.Lerp(Rgba.Hex("3B1F6E"), Rgba.Hex("8A3FB8"), Math.Max(0f, Math.Min(1f, (0.7f - y) / 1.4f))));
            // fasci di luce
            Rgba[] beams = { Rgba.Hex("FF5FA2", 0.35f), Rgba.Hex("5FE3FF", 0.3f), Rgba.Hex("FFE45F", 0.3f) };
            for (int i = 0; i < 3; i++)
            {
                float x0 = -0.6f + i * 0.6f;
                p.Fill(Sd.Intersect(scr, Sd.Polygon(x0 - 0.04f, 0.45f, x0 + 0.04f, 0.45f, x0 + 0.5f - i * 0.35f, -0.7f, x0 + 0.1f - i * 0.35f, -0.7f)), beams[i]);
            }
            // il pavimento a quadrotti
            p.FillFunc(Sd.Intersect(scr, Sd.Box(0f, -0.58f, 1.1f, 0.17f)), (x, y) =>
            {
                int k = (int)Math.Floor(x / 0.22f) + (int)Math.Floor(y / 0.12f);
                return (k & 1) == 0 ? Rgba.Hex("FF5FA2") : Rgba.Hex("FFE45F");
            });
            p.Line(Rgba.Hex("2B2233", 0.6f), 0.02f, -1.05f, -0.41f, 1.05f, -0.41f);
            // la palla a specchi
            p.InkLine(0.015f, 0f, 0.5f, 0f, 0.38f);
            p.Shape(Sd.Circle(0f, 0.3f, 0.09f), Rgba.Hex("D9DEE8"), 0.02f);
            p.Line(Rgba.Hex("8E97A8"), 0.01f, -0.09f, 0.3f, 0.09f, 0.3f);
            p.Line(Rgba.Hex("8E97A8"), 0.01f, 0f, 0.21f, 0f, 0.39f);
            p.Fill(Sd.Circle(-0.03f, 0.33f, 0.02f), Rgba.White);
            // stelline
            var rnd = new Random(270);
            for (int i = 0; i < 14; i++)
            {
                float sx = -0.95f + (float)rnd.NextDouble() * 1.9f, sy = -0.3f + (float)rnd.NextDouble() * 0.75f;
                if (Math.Abs(sx) < 0.35f && sy < 0.2f) continue;
                p.Line(Rgba.Hex("FFF4B8"), 0.012f, sx - 0.035f, sy, sx + 0.035f, sy);
                p.Line(Rgba.Hex("FFF4B8"), 0.012f, sx, sy - 0.035f, sx, sy + 0.035f);
            }
            p.Fill(Sd.Intersect(scr, Sd.Box(0f, 0.62f, 1.1f, 0.13f)), Rgba.Hex("1A1229", 0.85f));
            return new DoodleArt("v72_b_show_disco", p);
        }

        /// <summary>Il gatto ballerino della disco (centro 0,0; alto 0.85): in piedi, braccia al cielo, occhiali da sole.</summary>
        public static DoodleArt V72B_Dancer()
        {
            var p = new DoodlePainter(-0.4f, -0.45f, 0.4f, 0.48f, 160) { Ink = 0.025f };
            // coda
            p.Shape(Sd.Polyline(0.03f, 0.12f, -0.2f, 0.28f, -0.15f, 0.32f, 0.02f), CatOrange);
            // gambe
            p.Shape(Sd.Capsule(-0.07f, -0.2f, -0.14f, -0.4f, 0.045f), CatOrange);
            p.Shape(Sd.Capsule(0.07f, -0.2f, 0.16f, -0.38f, 0.045f), CatOrange);
            // braccia al cielo
            p.Shape(Sd.Capsule(-0.1f, 0.05f, -0.28f, 0.3f, 0.04f), CatOrange);
            p.Shape(Sd.Capsule(0.1f, 0.05f, 0.27f, 0.33f, 0.04f), CatOrange);
            // corpo con la pancia chiara
            Sdf body = Sd.Ellipse(0f, -0.08f, 0.15f, 0.2f);
            Body(p, body, CatOrange, CatShade);
            p.Fill(Sd.Ellipse(0f, -0.12f, 0.08f, 0.12f), Rgba.Hex("FFE3C2"));
            // testa, orecchie, occhiali
            Sdf ears = Sd.Union(Sd.Polygon(-0.15f, 0.2f, -0.12f, 0.38f, -0.03f, 0.27f), Sd.Polygon(0.03f, 0.27f, 0.12f, 0.38f, 0.15f, 0.2f));
            p.Shape(ears, CatOrange);
            Sdf head = Sd.Ellipse(0f, 0.18f, 0.15f, 0.12f);
            Body(p, head, CatOrange, CatShade);
            p.Shape(Sd.Union(Sd.Box(-0.06f, 0.2f, 0.05f, 0.03f, 0.015f), Sd.Box(0.06f, 0.2f, 0.05f, 0.03f, 0.015f)), Rgba.Hex("2B2233"), 0.015f);
            p.Line(Rgba.White.WithAlpha(0.8f), 0.01f, -0.09f, 0.21f, -0.06f, 0.215f);
            p.InkLine(0.012f, -0.03f, 0.1f, 0f, 0.08f, 0.03f, 0.1f);
            return new DoodleArt("v72_b_dancer", p);
        }

        /// <summary>L'effetto neve del cambio canale (centro 0,0, 2.1 x 1.44): puntini grigi e righe orizzontali.</summary>
        public static DoodleArt V72B_Static()
        {
            var p = new DoodlePainter(-1.08f, -0.75f, 1.08f, 0.75f, 60) { Ink = 0.03f };
            Sdf scr = V72B_ScreenShape();
            p.FillFunc(scr, (x, y) =>
            {
                int ix = (int)Math.Floor(x * 60f), iy = (int)Math.Floor(y * 60f);
                uint h = (uint)(ix * 374761393 + iy * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                float v = (h & 0xFFFF) / 65535f;
                float band = (int)Math.Floor(y * 9f) % 3 == 0 ? 0.12f : 0f;
                v = Math.Min(1f, v * 0.85f + band);
                return new Rgba(v, v, v * 1.02f > 1f ? 1f : v * 1.02f, 1f);
            });
            return new DoodleArt("v72_b_static", p);
        }

        /// <summary>
        /// Il divano senza i cuscini della seduta (centro 0,0; largo 4.5): schienale con i tre cuscinoni, braccioli,
        /// la base con i piedini. La seduta (dove stanno i cuscini) è a y = -0.12.
        /// </summary>
        public static DoodleArt V72B_Sofa()
        {
            var p = new DoodlePainter(-2.32f, -0.92f, 2.32f, 0.95f, 80) { Ink = 0.05f };
            p.Fill(Sd.Box(0.05f, -0.82f, 2.15f, 0.08f), new Rgba(0f, 0f, 0f, 0.12f));
            foreach (float x in new[] { -1.85f, 1.85f }) p.Shape(Sd.Box(x, -0.78f, 0.07f, 0.09f, 0.02f), V72BWoodShade, 0.03f);
            // schienale
            Body(p, Sd.Box(0f, 0.38f, 1.98f, 0.5f, 0.22f), V72BSofa, V72BSofaShade);
            foreach (float x in new[] { -1.22f, 0f, 1.22f })
                p.Shape(Sd.Box(x, 0.36f, 0.58f, 0.38f, 0.16f), Rgba.Hex("5BAD9F"), 0.035f);
            // la seduta vuota (dietro i cuscini): più scura, si vede quando si alza un cuscino
            p.Shape(Sd.Box(0f, -0.12f, 1.9f, 0.14f, 0.06f), Rgba.Hex("2F655C"), 0.035f);
            // base
            Body(p, Sd.Box(0f, -0.48f, 1.98f, 0.26f, 0.08f), V72BSofa, V72BSofaShade);
            // braccioli
            foreach (float x in new[] { -2.02f, 2.02f })
            {
                Body(p, Sd.Box(x, -0.15f, 0.26f, 0.6f, 0.2f), V72BSofa, V72BSofaShade);
                p.Shape(Sd.Ellipse(x, 0.38f, 0.26f, 0.12f), Rgba.Hex("5BAD9F"), 0.035f);
            }
            return new DoodleArt("v72_b_sofa", p);
        }

        /// <summary>Un cuscino della seduta (centro 0,0; 1.18 x 0.42) con le cuciture.</summary>
        public static DoodleArt V72B_Cushion()
        {
            var p = new DoodlePainter(-0.66f, -0.27f, 0.66f, 0.27f, 110) { Ink = 0.04f };
            Sdf c = Sd.Box(0f, 0f, 0.59f, 0.21f, 0.12f);
            Body(p, c, Rgba.Hex("6BC0B1"), V72BSofa);
            p.Line(Rgba.Hex("3A7A6F", 0.7f), 0.015f, -0.5f, -0.02f, 0.5f, -0.02f);
            foreach (float x in new[] { -0.3f, 0.3f }) p.Fill(Sd.Circle(x, 0.04f, 0.025f), Rgba.Hex("3A7A6F"));
            p.Line(Rgba.White.WithAlpha(0.35f), 0.03f, -0.42f, 0.13f, 0.1f, 0.14f);
            return new DoodleArt("v72_b_cushion", p);
        }

        /// <summary>Il tavolino basso (centro 0,0; piano 2.1 x 0.4 a y 0.12, gambe fino a -0.42).</summary>
        public static DoodleArt V72B_Table()
        {
            var p = new DoodlePainter(-1.15f, -0.5f, 1.15f, 0.36f, 90) { Ink = 0.045f };
            p.Fill(Sd.Ellipse(0.05f, -0.42f, 1.0f, 0.08f), new Rgba(0f, 0f, 0f, 0.12f));
            foreach (float x in new[] { -0.85f, 0.85f })
                p.Shape(Sd.Box(x, -0.18f, 0.06f, 0.24f, 0.02f), V72BWoodShade, 0.035f);
            p.Shape(Sd.Box(0f, -0.06f, 1.0f, 0.05f, 0.02f), V72BWoodShade, 0.035f);
            Body(p, Sd.Box(0f, 0.12f, 1.05f, 0.18f, 0.08f), Rgba.Hex("C98A57"), V72BWood);
            p.Line(Rgba.White.WithAlpha(0.3f), 0.03f, -0.8f, 0.2f, -0.3f, 0.22f);
            return new DoodleArt("v72_b_table", p);
        }

        /// <summary>Il telecomando sdraiato (centro 0,0; 0.72 x 0.2, punta a destra): tasti colorati e lo sportellino.</summary>
        public static DoodleArt V72B_Remote()
        {
            var p = new DoodlePainter(-0.42f, -0.16f, 0.42f, 0.16f, 180) { Ink = 0.025f };
            Sdf r = Sd.Box(0f, 0f, 0.36f, 0.1f, 0.07f);
            Body(p, r, V72BPlastic, V72BPlasticShade);
            p.Fill(Sd.Circle(0.3f, 0f, 0.025f), Rgba.Hex("E0352B"));
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 2; j++)
                    p.Fill(Sd.Box(-0.2f + i * 0.1f, -0.035f + j * 0.07f, 0.03f, 0.022f, 0.01f), j == 0 ? Rgba.Hex("C9CFD9") : (i % 2 == 0 ? Rgba.Hex("5FB0E8") : Rgba.Hex("F2C14E")));
            p.Line(Rgba.Hex("6A6276"), 0.012f, -0.3f, -0.08f, -0.3f, 0.08f);
            return new DoodleArt("v72_b_remote", p);
        }

        /// <summary>Una pila stilo (centro 0,0; 0.3 x 0.1, polo + a destra).</summary>
        public static DoodleArt V72B_Battery()
        {
            var p = new DoodlePainter(-0.18f, -0.08f, 0.2f, 0.08f, 200) { Ink = 0.018f };
            p.Shape(Sd.Box(0.165f, 0f, 0.02f, 0.025f, 0.01f), Rgba.Hex("C9CFD9"));
            Sdf b = Sd.Box(0f, 0f, 0.15f, 0.055f, 0.02f);
            p.Fill(b, Rgba.Hex("2B2233"));
            p.Fill(Sd.Intersect(b, Sd.Box(0.1f, 0f, 0.06f, 0.06f)), Rgba.Hex("F2C14E"));
            p.Line(Rgba.White.WithAlpha(0.35f), 0.015f, -0.12f, 0.03f, 0.12f, 0.03f);
            p.Shape(b, Rgba.Clear);
            return new DoodleArt("v72_b_battery", p);
        }

        /// <summary>Il libretto delle istruzioni chiuso (centro 0,0; 0.7 x 0.5): copertina col televisorino.</summary>
        public static DoodleArt V72B_Manual()
        {
            var p = new DoodlePainter(-0.4f, -0.3f, 0.4f, 0.3f, 150) { Ink = 0.03f };
            p.Fill(Sd.Box(0.03f, -0.03f, 0.35f, 0.25f, 0.03f), new Rgba(0f, 0f, 0f, 0.15f));
            p.Shape(Sd.Box(0f, 0f, 0.35f, 0.25f, 0.03f), Rgba.Hex("F4EDE0"));
            p.Fill(Sd.Box(-0.31f, 0f, 0.03f, 0.24f), Rgba.Hex("E0352B"));
            p.Shape(Sd.Box(0.06f, -0.06f, 0.13f, 0.09f, 0.02f), Rgba.Hex("B8875A"), 0.02f);
            p.Fill(Sd.Box(0.05f, -0.055f, 0.09f, 0.06f, 0.02f), Rgba.Hex("4A5A57"));
            p.Line(Rgba.Hex("2B2233"), 0.012f, 0.0f, 0.03f, -0.04f, 0.08f);
            p.Line(Rgba.Hex("2B2233"), 0.012f, 0.1f, 0.03f, 0.14f, 0.08f);
            return new DoodleArt("v72_b_manual", p);
        }

        /// <summary>
        /// La pagina aperta del libretto (centro 0,0; 3.4 x 2.6): carta, il disegnino del televisore visto di tre quarti
        /// con i due tastini sul fianco cerchiati e una freccia rossa che li indica (la scritta "CH +/-" e il titolo li
        /// mette il livello), righe di testo finte.
        /// </summary>
        public static DoodleArt V72B_ManualPage()
        {
            var p = new DoodlePainter(-1.78f, -1.38f, 1.78f, 1.38f, 90) { Ink = 0.045f };
            p.Fill(Sd.Box(0.06f, -0.06f, 1.7f, 1.3f, 0.05f), new Rgba(0f, 0f, 0f, 0.25f));
            p.Shape(Sd.Box(0f, 0f, 1.7f, 1.3f, 0.05f), Rgba.Hex("FBF6EA"));
            p.Line(Rgba.Hex("D9CFBC"), 0.02f, 0f, -1.25f, 0f, 1.25f);
            // righe di testo finte a sinistra
            var rnd = new Random(72);
            for (int i = 0; i < 9; i++)
            {
                float y = 0.62f - i * 0.2f;
                float len = 0.9f + (float)rnd.NextDouble() * 0.45f;
                p.Line(Rgba.Hex("B9B1A0"), 0.03f, -1.5f, y, -1.5f + len, y);
            }
            // il televisorino (a destra)
            float cx = 0.75f, cy = -0.05f;
            p.Shape(Sd.Polygon(cx + 0.55f, cy + 0.4f, cx + 0.72f, cy + 0.33f, cx + 0.72f, cy - 0.33f, cx + 0.55f, cy - 0.4f), V72BTvShade, 0.03f);
            Body(p, Sd.Box(cx, cy, 0.56f, 0.4f, 0.06f), V72BTvBody, V72BTvShade);
            p.Shape(Sd.Box(cx - 0.02f, cy + 0.04f, 0.42f, 0.28f, 0.07f), Rgba.Hex("4A5A57"), 0.03f);
            p.InkLine(0.02f, cx - 0.05f, cy + 0.4f, cx - 0.3f, cy + 0.62f);
            p.InkLine(0.02f, cx + 0.05f, cy + 0.4f, cx + 0.28f, cy + 0.63f);
            foreach (float by in new[] { 0.12f, -0.06f })
                p.Shape(Sd.Circle(cx + 0.635f, cy + by, 0.045f), Rgba.Hex("E9E3D6"), 0.02f);
            // il cerchio rosso attorno ai tastini e la freccia che arriva da destra in basso
            p.Line(Rgba.Hex("E0352B"), 0.035f,
                cx + 0.635f, cy + 0.27f, cx + 0.74f, cy + 0.2f, cx + 0.77f, cy + 0.03f, cx + 0.74f, cy - 0.15f,
                cx + 0.635f, cy - 0.22f, cx + 0.53f, cy - 0.15f, cx + 0.5f, cy + 0.03f, cx + 0.53f, cy + 0.2f, cx + 0.64f, cy + 0.27f);
            p.Line(Rgba.Hex("E0352B"), 0.045f, cx + 0.55f, cy - 0.85f, cx + 0.78f, cy - 0.6f, cx + 0.72f, cy - 0.3f);
            p.Fill(Sd.Polygon(cx + 0.62f, cy - 0.33f, cx + 0.71f, cy - 0.2f, cx + 0.8f, cy - 0.33f), Rgba.Hex("E0352B"));
            // numero di pagina
            p.Line(Rgba.Hex("B9B1A0"), 0.025f, 1.45f, -1.12f, 1.55f, -1.12f);
            return new DoodleArt("v72_b_manual_page", p);
        }

        /// <summary>
        /// Gli occhi aperti del gatto che dorme ("cat", stesse coordinate: la testa è in -0.72, 0.33): due toppe arancioni
        /// che coprono gli occhi chiusi e gli occhi tondi con la pupilla rivolta a sinistra (verso il televisore).
        /// </summary>
        public static DoodleArt V72B_CatEyes()
        {
            var p = new DoodlePainter(-1.08f, 0.18f, -0.36f, 0.5f, 120) { Ink = 0.025f };
            foreach (float x in new[] { -0.855f, -0.575f })
            {
                p.Fill(Sd.Ellipse(x, 0.345f, 0.1f, 0.075f), CatOrange, false);
                p.Shape(Sd.Ellipse(x, 0.35f, 0.075f, 0.085f), Rgba.White);
                p.Fill(Sd.Ellipse(x - 0.025f, 0.345f, 0.04f, 0.06f), Rgba.Hex("2B2233"));
                p.Fill(Sd.Circle(x - 0.04f, 0.37f, 0.015f), Rgba.White);
            }
            return new DoodleArt("v72_b_cat_eyes", p);
        }
    }
}
