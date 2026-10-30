using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.70 per il livello 268 "The Stubborn Ketchup" (il coltello è hw_knife, lo sbuffo è puff):
    /// il tavolo della tavola calda (sfondo intero), il piatto di patatine, il flacone di ketchup senza marca (corpo,
    /// tappo con beccuccio, sigillo di alluminio con la linguetta, sigillo strappato, adesivo "NEW!"), il tovagliolo, la
    /// goccia di ketchup (le gocce in fila disegnano lo smile) e il filo di ketchup che scende. Prefisso v70_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV70B() => new DoodleArt[]
        {
            V70B_DinerBg(), V70B_Fries(), V70B_Bottle(), V70B_Cap(), V70B_Foil(), V70B_FoilScrap(), V70B_Sticker(),
            V70B_Napkin(), V70B_Blob(), V70B_Stream(),
        };

        static readonly Rgba V70BRed = Rgba.Hex("E0352B"), V70BRedShade = Rgba.Hex("A8231C");
        static readonly Rgba V70BFoil = Rgba.Hex("C9CFD9"), V70BFoilShade = Rgba.Hex("8E97A8");
        static readonly Rgba V70BFry = Rgba.Hex("F6C445"), V70BFryShade = Rgba.Hex("D99A2B");

        /// <summary>
        /// La tavola calda vista da seduti: in alto la parete a scacchi bianchi e neri con la fascia rossa e lo schienale
        /// del divanetto in finta pelle rossa; dal bordo cromato (y = 0.95) in giù il piano del tavolo in formica
        /// turchese con i "boomerang" anni '50. Coordinate del mondo (come kitchen_bg).
        /// </summary>
        public static DoodleArt V70B_DinerBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f, Wobble = 0.006f };
            // parete
            Sdf wall = Sd.Box(0f, 4.5f, 5f, 3.6f);
            p.Fill(wall, Rgba.Hex("FBF3E1"), false);
            Sdf band = Sd.Box(0f, 3.95f, 5f, 0.32f);
            p.FillFunc(band, (x, y) => ((int)Math.Floor(x / 0.32f) + (int)Math.Floor((y - 3.63f) / 0.32f)) % 2 == 0 ? Rgba.Hex("2B2233") : Rgba.Hex("FBF8EF"));
            p.Fill(Sd.Box(0f, 3.53f, 5f, 0.08f), Rgba.Hex("E0352B"), false);
            p.Fill(Sd.Box(0f, 4.37f, 5f, 0.08f), Rgba.Hex("E0352B"), false);
            // lo schienale del divanetto (bottoni capitonné)
            Sdf seat = Sd.Box(0f, 2.15f, 4.8f, 1.25f, 0.35f);
            Body(p, seat, Rgba.Hex("D6453C"), Rgba.Hex("A92F28"));
            for (int r = 0; r < 2; r++)
                for (int i = -6; i <= 6; i++)
                {
                    float bx = i * 0.7f + (r == 0 ? 0f : 0.35f), by = 2.55f - r * 0.75f;
                    p.Fill(Sd.Circle(bx, by, 0.05f), Rgba.Hex("7E211C"));
                    p.Line(Rgba.Hex("B9362E", 0.7f), 0.025f, bx - 0.3f, by + 0.25f, bx, by, bx + 0.3f, by + 0.25f);
                }
            p.Line(Rgba.Hex("F28B80"), 0.05f, -4.4f, 3.15f, 4.4f, 3.15f);
            // il piano del tavolo
            Sdf top = Sd.Box(0f, -3.05f, 5f, 4.0f);
            p.FillFunc(top, (x, y) => Rgba.Lerp(Rgba.Hex("9FDCD3"), Rgba.Hex("7CC9BE"), Math.Max(0f, Math.Min(1f, (0.9f - y) / 7.5f))));
            // boomerang e puntini (motivo formica)
            var rnd = new Random(70);
            for (int i = 0; i < 46; i++)
            {
                float bx = -4.2f + (float)rnd.NextDouble() * 8.4f, by = -6.6f + (float)rnd.NextDouble() * 7.2f;
                float a = (float)rnd.NextDouble() * 360f;
                Rgba c = i % 3 == 0 ? Rgba.Hex("F2C14E", 0.55f) : i % 3 == 1 ? Rgba.Hex("E0352B", 0.35f) : Rgba.Hex("2B2233", 0.25f);
                p.Fill(Sd.Rotate(Sd.Polyline(0.03f, bx - 0.2f, by - 0.05f, bx, by + 0.08f, bx + 0.2f, by - 0.05f), bx, by, a), c);
                p.Fill(Sd.Circle(bx + 0.35f, by + 0.3f, 0.025f), Rgba.Hex("FBF8EF", 0.7f));
            }
            // riflesso della luce sul piano
            p.Fill(Sd.Rotate(Sd.Ellipse(-1.6f, -0.3f, 1.6f, 0.35f), -1.6f, -0.3f, -8f), Rgba.White.WithAlpha(0.13f));
            // il bordo cromato
            Sdf chrome = Sd.Box(0f, 0.95f, 5f, 0.13f);
            p.FillFunc(chrome, (x, y) => Rgba.Lerp(Rgba.Hex("8E97A8"), Rgba.Hex("F2F4F8"), Math.Max(0f, Math.Min(1f, (y - 0.82f) / 0.26f))));
            p.Line(Rgba.Hex("2B2233", 0.7f), 0.04f, -4.5f, 0.82f, 4.5f, 0.82f);
            p.Line(Rgba.Hex("2B2233", 0.7f), 0.04f, -4.5f, 1.08f, 4.5f, 1.08f);
            return new DoodleArt("v70_b_diner_bg", p);
        }

        /// <summary>Piatto ovale con un mucchio di patatine (centro del piatto 0,0; largo 2.4).</summary>
        public static DoodleArt V70B_Fries()
        {
            var p = new DoodlePainter(-1.3f, -0.68f, 1.3f, 0.95f, 110) { Ink = 0.04f };
            p.Fill(Sd.Ellipse(0.05f, -0.1f, 1.22f, 0.55f), new Rgba(0f, 0f, 0f, 0.15f));
            p.Shape(Sd.Ellipse(0f, 0f, 1.2f, 0.56f), Rgba.Hex("FBFBF7"));
            p.Shape(Sd.Ellipse(0f, 0.02f, 0.92f, 0.4f), Rgba.Hex("EEF0EC"), 0.025f);
            p.Line(Rgba.Hex("6CC3E0"), 0.03f, -1.05f, 0.18f, -0.75f, 0.42f, -0.2f, 0.53f);
            // le patatine: bastoncini incrociati, quelli dietro più in alto
            var rnd = new Random(268);
            for (int i = 0; i < 38; i++)
            {
                float row = i / 38f;
                float cx = -0.72f + (float)rnd.NextDouble() * 1.44f;
                float cy = 0.42f - row * 0.62f + (float)rnd.NextDouble() * 0.08f;
                cx *= 1f - Math.Abs(cy - 0.1f) * 0.6f;
                float len = 0.32f + (float)rnd.NextDouble() * 0.18f;
                float ang = -65f + (float)rnd.NextDouble() * 130f;
                Sdf fry = Sd.Rotate(Sd.Box(cx, cy, 0.055f, len * 0.5f, 0.02f), cx, cy, ang);
                Body(p, fry, i % 4 == 0 ? Rgba.Hex("F9D466") : V70BFry, V70BFryShade);
            }
            return new DoodleArt("v70_b_fries", p);
        }

        /// <summary>
        /// Il flacone di ketchup da spremere, senza marca e senza tappo (centro 0,0: fondo a y = -1.0, bocca a y = 0.78):
        /// corpo rosso con la luce, l'etichetta bianca con il pomodoro, la spalla e il collo con il filetto.
        /// </summary>
        public static DoodleArt V70B_Bottle()
        {
            var p = new DoodlePainter(-0.5f, -1.12f, 0.5f, 0.86f, 140) { Ink = 0.04f };
            Sdf body = Sd.Union(
                Sd.Box(0f, -0.25f, 0.37f, 0.75f, 0.16f),
                Sd.Polygon(-0.33f, 0.36f, 0.33f, 0.36f, 0.19f, 0.6f, -0.19f, 0.6f),
                Sd.Box(0f, 0.65f, 0.17f, 0.13f, 0.03f));
            Body(p, body, V70BRed, V70BRedShade);
            // il fondo un po' più scuro
            p.Fill(Sd.Intersect(body, Sd.Box(0f, -0.94f, 0.4f, 0.07f)), V70BRedShade.WithAlpha(0.8f));
            p.Line(Rgba.Hex("2B2233", 0.6f), 0.025f, -0.35f, -0.87f, 0.35f, -0.87f);
            // il filetto del collo
            for (int i = 0; i < 3; i++) p.Line(Rgba.Hex("7E1A15"), 0.02f, -0.16f, 0.57f + i * 0.065f, 0.16f, 0.61f + i * 0.065f);
            // la luce
            p.Fill(Sd.Capsule(-0.23f, -0.75f, -0.23f, 0.3f, 0.045f), Rgba.White.WithAlpha(0.35f));
            // l'etichetta con il pomodoro (la scritta la mette il livello)
            p.Shape(Sd.Box(0f, -0.38f, 0.33f, 0.3f, 0.05f), Rgba.Hex("FBF8EF"), 0.03f);
            p.Shape(Sd.Circle(0f, -0.3f, 0.12f), V70BRed, 0.025f);
            p.Fill(Sd.Polygon(-0.07f, -0.17f, 0f, -0.21f, 0.07f, -0.17f, 0.03f, -0.15f, 0f, -0.12f, -0.03f, -0.15f), Rgba.Hex("5BAE4A"));
            p.Fill(Sd.Circle(-0.04f, -0.27f, 0.03f), Rgba.White.WithAlpha(0.6f));
            p.Shape(body, Rgba.Clear);
            return new DoodleArt("v70_b_bottle", p);
        }

        /// <summary>Il tappo a vite con il beccuccio conico (centro 0,0: base da y = -0.2, punta a y = 0.25).</summary>
        public static DoodleArt V70B_Cap()
        {
            var p = new DoodlePainter(-0.28f, -0.25f, 0.28f, 0.31f, 160) { Ink = 0.035f };
            Sdf cap = Sd.Union(Sd.Box(0f, -0.08f, 0.22f, 0.12f, 0.04f), Sd.Polygon(-0.17f, 0.03f, 0.17f, 0.03f, 0.04f, 0.22f, -0.04f, 0.22f));
            Body(p, cap, Rgba.Hex("F6F3EA"), Rgba.Hex("CFC8B8"));
            for (int i = 0; i < 7; i++)
            {
                float x = -0.18f + i * 0.06f;
                p.Line(Rgba.Hex("B9B1A0"), 0.015f, x, -0.18f, x, 0.02f);
            }
            p.Shape(Sd.Box(0f, 0.235f, 0.045f, 0.03f, 0.015f), V70BRed, 0.025f);
            p.Shape(cap, Rgba.Clear);
            return new DoodleArt("v70_b_cap", p);
        }

        /// <summary>
        /// Il sigillo di alluminio sulla bocca del flacone (centro 0,0 = bocca), visto quasi di taglio, con la linguetta che
        /// sporge a destra: con il tappo avvitato si vede solo la punta della linguetta.
        /// </summary>
        public static DoodleArt V70B_Foil()
        {
            var p = new DoodlePainter(-0.24f, -0.17f, 0.5f, 0.1f, 200) { Ink = 0.025f };
            Sdf tab = Sd.Union(Sd.Polygon(0.12f, 0.02f, 0.3f, -0.02f, 0.3f, -0.11f, 0.12f, -0.05f), Sd.Box(0.37f, -0.065f, 0.09f, 0.065f, 0.045f));
            Body(p, tab, V70BFoil, V70BFoilShade);
            p.Line(Rgba.White.WithAlpha(0.9f), 0.015f, 0.31f, -0.04f, 0.4f, -0.04f);
            Sdf disc = Sd.Ellipse(0f, 0f, 0.19f, 0.055f);
            Body(p, disc, V70BFoil, V70BFoilShade);
            p.Line(Rgba.White.WithAlpha(0.8f), 0.015f, -0.1f, 0.015f, 0.05f, 0.025f);
            return new DoodleArt("v70_b_foil", p);
        }

        /// <summary>Il sigillo strappato: un disco d'alluminio stropicciato con la linguetta (centro 0,0).</summary>
        public static DoodleArt V70B_FoilScrap()
        {
            var p = new DoodlePainter(-0.3f, -0.26f, 0.42f, 0.26f, 180) { Ink = 0.03f };
            Sdf disc = Sd.Polygon(-0.2f, 0.02f, -0.14f, 0.17f, -0.02f, 0.2f, 0.06f, 0.15f, 0.18f, 0.16f, 0.21f, 0.02f, 0.16f, -0.12f, 0.04f, -0.2f, -0.1f, -0.17f, -0.18f, -0.1f);
            Sdf tab = Sd.Box(0.28f, -0.05f, 0.1f, 0.06f, 0.04f);
            Body(p, Sd.Union(disc, tab), V70BFoil, V70BFoilShade);
            p.Line(V70BFoilShade, 0.012f, -0.12f, 0.08f, 0.0f, -0.02f, 0.1f, 0.1f);
            p.Line(V70BFoilShade, 0.012f, -0.05f, -0.12f, 0.08f, -0.04f);
            p.Line(Rgba.White.WithAlpha(0.9f), 0.015f, -0.1f, 0.12f, 0.02f, 0.15f);
            return new DoodleArt("v70_b_foil_scrap", p);
        }

        /// <summary>L'adesivo giallo a stella (la scritta "NEW!" la mette il livello; centro 0,0, raggio 0.2).</summary>
        public static DoodleArt V70B_Sticker()
        {
            var p = new DoodlePainter(-0.24f, -0.24f, 0.24f, 0.24f, 200) { Ink = 0.025f };
            var pts = new List<float>();
            for (int i = 0; i < 24; i++)
            {
                double a = Math.PI * 2 * i / 24 + 0.1;
                float r = i % 2 == 0 ? 0.2f : 0.155f;
                pts.Add((float)Math.Cos(a) * r);
                pts.Add((float)Math.Sin(a) * r);
            }
            Body(p, Sd.Polygon(pts.ToArray()), Rgba.Hex("FFE04A"), Rgba.Hex("F2B632"));
            return new DoodleArt("v70_b_sticker", p);
        }

        /// <summary>Il tovagliolo di carta piegato (centro 0,0; 1.3 x 0.9).</summary>
        public static DoodleArt V70B_Napkin()
        {
            var p = new DoodlePainter(-0.75f, -0.55f, 0.75f, 0.55f, 110) { Ink = 0.035f };
            p.Fill(Sd.Box(0.05f, -0.05f, 0.65f, 0.45f, 0.04f), new Rgba(0f, 0f, 0f, 0.13f));
            Sdf n = Sd.Box(0f, 0f, 0.65f, 0.45f, 0.04f);
            p.Shape(n, Rgba.Hex("FFFFFF"));
            p.Fill(Sd.Intersect(n, Sd.Polygon(-0.65f, -0.45f, 0.65f, 0.45f, 0.65f, -0.45f)), Rgba.Hex("EEF0F4"));
            p.Line(Rgba.Hex("C9CFD9"), 0.02f, -0.6f, -0.4f, 0.6f, 0.4f);
            p.Shape(Sd.Box(0f, 0f, 0.56f, 0.36f, 0.03f), Rgba.Clear, 0.012f);
            return new DoodleArt("v70_b_napkin", p);
        }

        /// <summary>Una goccia di ketchup vista dall'alto (centro 0,0, raggio 0.08): in fila disegnano lo smile.</summary>
        public static DoodleArt V70B_Blob()
        {
            var p = new DoodlePainter(-0.11f, -0.11f, 0.11f, 0.11f, 200) { Ink = 0.018f, InkColor = Rgba.Hex("7E1A15") };
            Body(p, Sd.Circle(0f, 0f, 0.08f), V70BRed, V70BRedShade);
            p.Fill(Sd.Circle(-0.025f, 0.03f, 0.02f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("v70_b_blob", p);
        }

        /// <summary>Il filo di ketchup che scende dal beccuccio (in alto a y = 0, lungo 0.5).</summary>
        public static DoodleArt V70B_Stream()
        {
            var p = new DoodlePainter(-0.07f, -0.56f, 0.07f, 0.02f, 200) { Ink = 0.015f, InkColor = Rgba.Hex("7E1A15") };
            Body(p, Sd.Capsule(0f, -0.02f, 0f, -0.5f, 0.035f), V70BRed, V70BRedShade);
            return new DoodleArt("v70_b_stream", p);
        }
    }
}
