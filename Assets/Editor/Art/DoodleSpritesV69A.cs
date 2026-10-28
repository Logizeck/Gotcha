using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.69 per il livello 265 "Don't Let It Sink" (il resto riusa gli sprite esistenti: il topo
    /// capitano, la paperella, il secchio, le bolle di sapone, le gocce, lo spruzzo, il vortice, la stellina):
    /// la vasca vista di lato con le piastrelle azzurre "cielo" e il bordo di smalto "orizzonte" (disegnata già nelle
    /// coordinate del mondo), l'acqua (alta un'unità, la si stira), la superficie con la cresta di schiuma, le onde
    /// davanti, la barchetta col buco, l'indicatore dell'acqua, il cappello da capitano, il dito, il rotolo di nastro, la
    /// toppa, la catenella (anello, maglia) col tappo, la schiuma, il getto che entra dal buco.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV69A() => new DoodleArt[]
        {
            V69A_BathBg(), V69A_Water(), V69A_Surface(), V69A_WavesFront(), V69A_Boat(), V69A_Gauge(), V69A_Cap(), V69A_Finger(),
            V69A_Tape(), V69A_Patch(), V69A_Link(), V69A_ChainEnd(), V69A_Plug(), V69A_Foam(), V69A_Spurt(),
        };

        // misure della vasca (unità del mondo; il costruttore del livello usa gli stessi numeri)
        const float V69A_RimBottom = 0.92f, V69A_RimTop = 1.24f, V69A_Floor = -4.72f, V69A_DrainX = 0.9f;

        /// <summary>
        /// Lo sfondo (coordinate del mondo): in alto le piastrelle azzurre che sembrano cielo (con le fughe bianche), il
        /// bordo bianco della vasca che fa da orizzonte, l'interno di smalto con gli angoli arrotondati, il fondo e lo
        /// scarico (il tappo è un oggetto a parte).
        /// </summary>
        public static DoodleArt V69A_BathBg()
        {
            var p = new DoodlePainter(-3.0f, -5.2f, 3.0f, 5.2f, 50) { Ink = 0.05f };
            Rgba ink = Rgba.Hex("2B2233");
            // le piastrelle: azzurro che schiarisce verso l'orizzonte, fughe bianche
            Sdf wall = Sd.Box(0f, 3.3f, 3.1f, 2.2f);
            p.FillFunc(wall, (x, y) => Rgba.Lerp(Rgba.Hex("D9F0FA"), Rgba.Hex("A9D6EE"), Clamp01V69A((y - 1.1f) / 4f)));
            for (float gy = 1.1f + 0.62f; gy < 5.3f; gy += 0.62f) p.Line(new Rgba(1f, 1f, 1f, 0.75f), 0.035f, -3.0f, gy, 3.0f, gy);
            for (float gx = -2.79f; gx < 3.0f; gx += 0.62f) p.Line(new Rgba(1f, 1f, 1f, 0.75f), 0.035f, gx, 1.1f, gx, 5.2f);
            // un riflesso su qualche piastrella
            foreach (var t in new[] { (-2.17f, 2.65f), (0.31f, 3.9f), (1.55f, 2.03f), (-0.93f, 4.52f) })
                p.Line(new Rgba(1f, 1f, 1f, 0.55f), 0.04f, t.Item1 - 0.18f, t.Item2 + 0.12f, t.Item1 - 0.05f, t.Item2 + 0.22f);
            // il corpo della vasca (smalto bianco) e l'interno (un po' più in ombra), angoli in basso arrotondati
            p.Fill(Sd.Box(0f, -2.0f, 3.1f, 3.2f), Rgba.Hex("F4F7F9"));
            // (la parte alta arrotondata resterebbe sopra il bordo: la si taglia sotto il bordo)
            Sdf inside = Sd.Intersect(Sd.Box(0f, -1.6f, 2.92f, 3.12f, 0.95f), Sd.Box(0f, -2.0f, 3.2f, 2.97f));
            p.FillFunc(inside, (x, y) => Rgba.Lerp(Rgba.Hex("D3DEE6"), Rgba.Hex("EEF3F6"), Clamp01V69A((y - V69A_Floor) / 5.5f)));
            // ombra morbida ai lati e luce in mezzo
            p.Fill(Sd.Intersect(inside, Sd.Box(-2.6f, -1.6f, 0.4f, 3.2f)), new Rgba(0.6f, 0.68f, 0.76f, 0.18f));
            p.Fill(Sd.Intersect(inside, Sd.Box(2.6f, -1.6f, 0.4f, 3.2f)), new Rgba(0.6f, 0.68f, 0.76f, 0.18f));
            p.Line(new Rgba(1f, 1f, 1f, 0.6f), 0.08f, -1.6f, 0.6f, -1.6f, -3.6f);
            p.Line(new Rgba(1f, 1f, 1f, 0.45f), 0.05f, -1.35f, 0.4f, -1.35f, -2.8f);
            p.Shape(inside, Rgba.Clear, 0.045f);
            // lo scarico sul fondo
            p.Shape(Sd.Ellipse(V69A_DrainX, V69A_Floor + 0.06f, 0.3f, 0.08f), Rgba.Hex("A7B1BA"), 0.03f);
            p.Fill(Sd.Ellipse(V69A_DrainX, V69A_Floor + 0.06f, 0.2f, 0.045f), Rgba.Hex("3A3F47"));
            // il bordo della vasca: l'"orizzonte" bianco e lucido
            Sdf rim = Sd.Box(0f, (V69A_RimBottom + V69A_RimTop) * 0.5f, 3.3f, (V69A_RimTop - V69A_RimBottom) * 0.5f, 0.14f);
            Body(p, rim, Rgba.Hex("FCFDFE"), Rgba.Hex("DCE4EA"), null, 0f, 0.06f);
            p.Line(Rgba.White, 0.05f, -3.0f, V69A_RimTop - 0.07f, 3.0f, V69A_RimTop - 0.07f);
            p.Line(ink.WithAlpha(0.25f), 0.02f, -3.0f, V69A_RimBottom - 0.04f, 3.0f, V69A_RimBottom - 0.04f);
            return new DoodleArt("v69_bath_bg", p);
        }

        static float Clamp01V69A(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        /// <summary>L'acqua: un'unità di altezza (da 0 in basso a 1 in alto), larga 6; il livello la stira in verticale.</summary>
        public static DoodleArt V69A_Water()
        {
            var p = new DoodlePainter(-3.0f, 0f, 3.0f, 1f, 40) { Ink = 0f };
            p.FillFunc(Sd.Box(0f, 0.5f, 3.1f, 0.6f), (x, y) => Rgba.Lerp(Rgba.Hex("1C5A96", 0.97f), Rgba.Hex("4DB3E6", 0.86f), Clamp01V69A(y)));
            return new DoodleArt("v69_bath_water", p);
        }

        /// <summary>La superficie dell'acqua (y = 0 è il pelo dell'acqua): onda azzurra con la cresta bianca e qualche fiocco di schiuma.</summary>
        public static DoodleArt V69A_Surface()
        {
            var p = new DoodlePainter(-3.0f, -0.16f, 3.0f, 0.12f, 60) { Ink = 0f };
            Sdf band = (x, y) => Math.Max(y - (0.02f + 0.035f * (float)Math.Sin(x * 5f)), -0.14f - y);
            p.Fill(band, Rgba.Hex("86D3F3", 0.95f), false);
            var crest = new List<float>();
            for (float x = -3.0f; x <= 3.01f; x += 0.1f) { crest.Add(x); crest.Add(0.02f + 0.035f * (float)Math.Sin(x * 5f)); }
            p.Line(Rgba.Hex("F4FBFE"), 0.04f, crest.ToArray());
            foreach (float fx in new[] { -2.4f, -1.1f, 0.6f, 1.9f })
                p.Fill(Sd.Circle(fx, 0.02f + 0.035f * (float)Math.Sin(fx * 5f), 0.035f), Rgba.White);
            return new DoodleArt("v69_bath_surface", p);
        }

        /// <summary>Le onde davanti alla barchetta (y = 0 è il pelo dell'acqua): coprono a metà la parte immersa dello scafo.</summary>
        public static DoodleArt V69A_WavesFront()
        {
            var p = new DoodlePainter(-3.0f, -0.42f, 3.0f, 0.06f, 50) { Ink = 0f };
            Func<float, float> top = x => -0.03f + 0.04f * (float)Math.Sin(x * 3.6f + 1f);
            Sdf band = (x, y) => Math.Max(y - top(x), -0.4f - y);
            p.FillFunc(band, (x, y) => Rgba.Hex("4FB7E8", 0.25f + 0.4f * Clamp01V69A((y + 0.4f) / 0.4f)));
            var crest = new List<float>();
            for (float x = -3.0f; x <= 3.01f; x += 0.1f) { crest.Add(x); crest.Add(top(x)); }
            p.Line(Rgba.Hex("C9EEFB", 0.9f), 0.035f, crest.ToArray());
            return new DoodleArt("v69_bath_waves", p);
        }

        /// <summary>
        /// La barchetta vista di lato (0,0 = linea di galleggiamento al centro): scafo rosso con la riga bianca, la
        /// falchetta di legno, l'albero con la vela e la bandierina, il buco scheggiato vicino alla prua (in 0.58,-0.08).
        /// </summary>
        public static DoodleArt V69A_Boat()
        {
            var p = new DoodlePainter(-1.18f, -0.5f, 1.2f, 1.58f, 100) { Ink = 0.045f };
            // albero, vela e bandierina (dietro lo scafo)
            Body(p, Sd.Box(-0.05f, 0.86f, 0.035f, 0.64f, 0.02f), Rgba.Hex("9A6A45"), Rgba.Hex("7A4F31"));
            Body(p, Sd.Polygon(0.01f, 1.4f, 0.01f, 0.36f, 0.7f, 0.38f), Rgba.Hex("F8F4EA"), Rgba.Hex("DDD5C3"));
            p.Line(Rgba.Hex("DDD5C3"), 0.02f, 0.08f, 1.1f, 0.4f, 0.48f);
            p.Shape(Sd.Polygon(-0.04f, 1.5f, -0.04f, 1.32f, -0.36f, 1.41f), Rgba.Hex("5B8DEF"), 0.025f);
            // lo scafo
            Sdf hull = Sd.Polygon(-1.06f, 0.24f, 1.12f, 0.24f, 0.82f, -0.4f, -0.86f, -0.4f);
            Body(p, hull, Rgba.Hex("D9573F"), Rgba.Hex("B3412D"), null, -0.04f, 0.06f);
            p.Fill(Sd.Intersect(hull, Sd.Box(0f, 0.06f, 1.2f, 0.045f)), Rgba.Hex("F4EFE6"));
            p.Shape(hull, Rgba.Clear);
            p.Shape(Sd.Box(0.03f, 0.25f, 1.12f, 0.045f, 0.03f), Rgba.Hex("9A6A45"), 0.035f);
            // il buco: scheggiato, scuro
            Sdf hole = Sd.Polygon(0.47f, -0.07f, 0.52f, -0.01f, 0.56f, -0.04f, 0.6f, 0.01f, 0.64f, -0.05f, 0.69f, -0.08f,
                0.64f, -0.12f, 0.6f, -0.16f, 0.55f, -0.12f, 0.5f, -0.15f);
            p.Shape(hole, Rgba.Hex("1E1B26"), 0.025f);
            return new DoodleArt("v69_toy_boat", p);
        }

        /// <summary>L'indicatore dell'acqua (centro 0,0): tubo di vetro alto 0.9 con le tacche e la zona rossa in cima.</summary>
        public static DoodleArt V69A_Gauge()
        {
            var p = new DoodlePainter(-0.17f, -0.52f, 0.17f, 0.56f, 160) { Ink = 0.03f };
            p.Fill(Sd.Box(0f, 0f, 0.085f, 0.45f, 0.05f), new Rgba(0.92f, 0.96f, 1f, 0.3f));
            p.Fill(Sd.Box(0f, 0.36f, 0.08f, 0.07f, 0.03f), Rgba.Hex("E2483D", 0.35f));
            for (int i = 1; i < 6; i++)
            {
                float y = -0.45f + i * 0.15f;
                p.Line(Rgba.Hex("2B2233", 0.7f), 0.015f, -0.085f, y, -0.03f, y);
            }
            p.Line(new Rgba(1f, 1f, 1f, 0.7f), 0.02f, 0.05f, 0.35f, 0.05f, -0.35f);
            p.Shape(Sd.Box(0f, 0f, 0.085f, 0.45f, 0.05f), Rgba.Clear);
            p.Shape(Sd.Box(0f, 0.48f, 0.11f, 0.04f, 0.02f), Rgba.Hex("A7B1BA"), 0.025f);
            p.Shape(Sd.Box(0f, -0.48f, 0.11f, 0.04f, 0.02f), Rgba.Hex("A7B1BA"), 0.025f);
            return new DoodleArt("v69_boat_gauge", p);
        }

        /// <summary>Cappello da capitano (centro 0,0): calotta bianca, visiera nera, fascia blu e ancoretta dorata.</summary>
        public static DoodleArt V69A_Cap()
        {
            var p = new DoodlePainter(-0.32f, -0.14f, 0.34f, 0.26f, 180) { Ink = 0.03f };
            Body(p, Sd.Ellipse(0f, 0.08f, 0.27f, 0.13f), Rgba.Hex("FBFBF8"), Rgba.Hex("DADDE2"));
            p.Shape(Sd.Box(0f, -0.03f, 0.2f, 0.05f, 0.02f), Rgba.Hex("2F4C8F"), 0.025f);
            p.Shape(Sd.Rotate(Sd.Ellipse(0.12f, -0.09f, 0.17f, 0.04f), 0.12f, -0.09f, -8f), Rgba.Hex("2B2233"), 0.02f);
            p.Fill(Sd.Circle(0f, 0.09f, 0.04f), Rgba.Hex("F2C14E"));
            p.Line(Rgba.Hex("B88A1E"), 0.012f, 0f, 0.12f, 0f, 0.06f);
            return new DoodleArt("v69_captain_cap", p);
        }

        /// <summary>Il dito che tappa il buco: mano che arriva da destra, punta dell'indice in 0,0.</summary>
        public static DoodleArt V69A_Finger()
        {
            var p = new DoodlePainter(-0.1f, -0.3f, 1.8f, 0.24f, 120) { Ink = 0.035f };
            Rgba skin = Rgba.Hex("F2C6A0"), shade = Rgba.Hex("D9A47F");
            Body(p, Sd.Box(1.35f, -0.07f, 0.45f, 0.19f, 0.05f), Rgba.Hex("5B8DEF"), Rgba.Hex("3F6CC4"));
            Body(p, Sd.Box(0.88f, -0.07f, 0.06f, 0.21f, 0.03f), Rgba.Hex("EDEAE3"), Rgba.Hex("C9C3B6"));
            Sdf hand = Sd.Union(Sd.Box(0.6f, -0.08f, 0.24f, 0.16f, 0.08f), Sd.Capsule(0.02f, 0f, 0.5f, 0.02f, 0.065f),
                Sd.Capsule(0.38f, -0.17f, 0.5f, -0.2f, 0.07f));
            Body(p, hand, skin, shade);
            p.Line(shade, 0.018f, 0.42f, -0.06f, 0.5f, -0.07f);
            p.Line(shade, 0.018f, 0.09f, 0.03f, 0.13f, -0.02f);
            return new DoodleArt("v69_plug_finger", p);
        }

        /// <summary>Rotolo di nastro adesivo grigio (centro 0,0) con il lembo che pende.</summary>
        public static DoodleArt V69A_Tape()
        {
            var p = new DoodlePainter(-0.34f, -0.48f, 0.34f, 0.34f, 140) { Ink = 0.035f };
            p.Shape(Sd.Polygon(0.12f, -0.24f, 0.26f, -0.2f, 0.27f, -0.42f, 0.2f, -0.38f, 0.14f, -0.44f), Rgba.Hex("AEB6C0"), 0.025f);
            Sdf ring = Sd.Subtract(Sd.Circle(0f, 0f, 0.29f), Sd.Circle(0f, 0f, 0.13f));
            Body(p, ring, Rgba.Hex("B9C0C9"), Rgba.Hex("8F98A4"));
            p.Shape(Sd.Subtract(Sd.Circle(0f, 0f, 0.13f), Sd.Circle(0f, 0f, 0.1f)), Rgba.Hex("C8A27A"), 0.02f);
            p.Line(new Rgba(1f, 1f, 1f, 0.6f), 0.025f, -0.18f, 0.12f, -0.08f, 0.2f);
            return new DoodleArt("v69_duct_tape", p);
        }

        /// <summary>La toppa: due strisce di nastro adesivo incrociate (centro 0,0).</summary>
        public static DoodleArt V69A_Patch()
        {
            var p = new DoodlePainter(-0.26f, -0.2f, 0.26f, 0.2f, 180) { Ink = 0.02f };
            foreach (float a in new[] { 28f, -28f })
            {
                Sdf strip = Sd.Rotate(Sd.Box(0f, 0f, 0.22f, 0.05f, 0.01f), 0f, 0f, a);
                p.Shape(strip, Rgba.Hex("B9C0C9"));
                p.Hatch(strip, Rgba.Hex("8F98A4", 0.6f), 0.04f, 0.008f, a + 90f);
            }
            return new DoodleArt("v69_tape_patch", p);
        }

        /// <summary>Una maglia della catenella (centro 0,0, lunga 0.16, orizzontale).</summary>
        public static DoodleArt V69A_Link()
        {
            var p = new DoodlePainter(-0.1f, -0.065f, 0.1f, 0.065f, 220) { Ink = 0.018f };
            p.Shape(Sd.Subtract(Sd.Ellipse(0f, 0f, 0.08f, 0.048f), Sd.Ellipse(0f, 0f, 0.048f, 0.02f)), Rgba.Hex("C7CED8"));
            return new DoodleArt("v69_chain_link", p);
        }

        /// <summary>L'anello in fondo alla catenella (centro 0,0): da prendere col dito.</summary>
        public static DoodleArt V69A_ChainEnd()
        {
            var p = new DoodlePainter(-0.22f, -0.22f, 0.22f, 0.24f, 160) { Ink = 0.03f };
            Body(p, Sd.Subtract(Sd.Circle(0f, 0f, 0.17f), Sd.Circle(0f, 0f, 0.1f)), Rgba.Hex("D6DCE4"), Rgba.Hex("9AA3B0"));
            p.Shape(Sd.Circle(0f, 0.19f, 0.04f), Rgba.Hex("C7CED8"), 0.02f);
            p.Line(new Rgba(1f, 1f, 1f, 0.8f), 0.02f, -0.12f, 0.06f, -0.06f, 0.13f);
            return new DoodleArt("v69_chain_end", p);
        }

        /// <summary>Il tappo di gomma (0,0 = centro della base) con l'occhiello in cima per la catenella.</summary>
        public static DoodleArt V69A_Plug()
        {
            var p = new DoodlePainter(-0.3f, -0.08f, 0.3f, 0.32f, 160) { Ink = 0.03f };
            p.Shape(Sd.Subtract(Sd.Circle(0f, 0.22f, 0.06f), Sd.Circle(0f, 0.22f, 0.03f)), Rgba.Hex("C7CED8"), 0.02f);
            Body(p, Sd.Polygon(-0.25f, 0.16f, 0.25f, 0.16f, 0.18f, -0.04f, -0.18f, -0.04f), Rgba.Hex("454B57"), Rgba.Hex("2E323B"));
            p.Shape(Sd.Box(0f, 0.17f, 0.27f, 0.03f, 0.02f), Rgba.Hex("A7B1BA"), 0.02f);
            return new DoodleArt("v69_bath_plug", p);
        }

        /// <summary>Un ciuffo di schiuma (0,0 = pelo dell'acqua): bolle bianche ammucchiate, la base piatta.</summary>
        public static DoodleArt V69A_Foam()
        {
            var p = new DoodlePainter(-0.55f, -0.08f, 0.55f, 0.3f, 120) { Ink = 0.025f };
            Sdf foam = Sd.Union(Sd.Circle(-0.3f, 0.06f, 0.13f), Sd.Circle(-0.1f, 0.12f, 0.17f), Sd.Circle(0.14f, 0.1f, 0.15f),
                Sd.Circle(0.34f, 0.05f, 0.11f), Sd.Box(0f, 0f, 0.42f, 0.05f, 0.04f));
            Body(p, Sd.Intersect(foam, Sd.Box(0f, 0.12f, 0.6f, 0.16f)), Rgba.Hex("FFFFFF"), Rgba.Hex("D8EEF8"), null, 0.03f, 0.05f);
            foreach (var c in new[] { (-0.18f, 0.18f, 0.035f), (0.1f, 0.16f, 0.03f), (0.3f, 0.09f, 0.025f) })
                p.Shape(Sd.Circle(c.Item1, c.Item2, c.Item3), Rgba.Hex("EAF7FD"), 0.015f);
            return new DoodleArt("v69_bath_foam", p);
        }

        /// <summary>Il getto d'acqua che entra nel buco (il buco è in 0,0; l'acqua arriva da destra, in basso).</summary>
        public static DoodleArt V69A_Spurt()
        {
            var p = new DoodlePainter(-0.06f, -0.3f, 0.5f, 0.1f, 160) { Ink = 0.02f };
            Sdf jet = Sd.Union(Sd.Capsule(0.02f, -0.02f, 0.18f, -0.12f, 0.035f), Sd.Capsule(0.18f, -0.12f, 0.38f, -0.2f, 0.05f));
            p.Shape(jet, Rgba.Hex("7FD0F2"));
            p.Line(Rgba.White, 0.015f, 0.08f, -0.04f, 0.3f, -0.14f);
            p.Shape(Sd.Circle(0.44f, -0.08f, 0.03f), Rgba.Hex("7FD0F2"), 0.015f);
            p.Shape(Sd.Circle(0.3f, -0.27f, 0.025f), Rgba.Hex("7FD0F2"), 0.015f);
            return new DoodleArt("v69_leak_spurt", p);
        }
    }
}
