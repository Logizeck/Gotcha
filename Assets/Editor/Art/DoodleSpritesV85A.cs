using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.85 per il livello 82 della roadmap "The Dishonest Hourglass": lo sfondo (studio al tramonto:
    /// parete prugna con la carta da parati a rombi, il calendario a strappo, la scrivania di legno dove sta la clessidra,
    /// il pavimento scuro dove sta il pulsante), la clessidra (vetro trasparente con i riflessi, i due tappi di legno e le
    /// colonnine; il vetro è disegnato DAVANTI alla sabbia), la sabbia del bulbo di sopra e quella del bulbo di sotto (piene:
    /// il livello le ritaglia con ClipArt/SpriteWindowClip, niente SpriteMask), il display digitale appeso alla parete (le
    /// cifre sono un testo) e il pulsante verde ADD TIME (la scritta è un testo). Il filo di sabbia è "plain" tinto, le
    /// scintille "sparkle_star", il bagliore "glow", la goccia di sudore del pulsante "sweat". Prefisso v85_a_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV85A() => new DoodleArt[]
        {
            V85A_Bg(), V85A_Glass(), V85A_Sand(true), V85A_Sand(false), V85A_Display(), V85A_Button(),
        };

        // misure condivise col builder (coordinate locali della clessidra: origine al collo, scala 1)
        /// <summary>Altezza interna di un bulbo (dal collo al tappo) e mezza altezza della clessidra fino al fondo del tappo.</summary>
        public const float V85ABulbH = 1.58f, V85AHalfH = 1.85f;
        /// <summary>Piano della scrivania nel mondo (dove poggia la clessidra).</summary>
        public const float V85ADeskY = -1.33f;

        static readonly Rgba V85ASand = Rgba.Hex("F2C46B"), V85ASandShade = Rgba.Hex("D69C45");
        static readonly Rgba V85AGlassInk = Rgba.Hex("3B4E6B");

        static float V85A_Clamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        /// <summary>Metà destra del bulbo di sopra, dal collo al tappo (x, y).</summary>
        static readonly float[] V85ABulbHalf =
        {
            0.07f, 0.0f, 0.12f, 0.08f, 0.32f, 0.28f, 0.55f, 0.55f, 0.7f, 0.85f, 0.75f, 1.12f, 0.73f, 1.36f, 0.64f, 1.52f, 0.56f, 1.6f,
        };

        /// <summary>Un bulbo (sopra: up=true) come poligono chiuso simmetrico.</summary>
        static Sdf V85A_Bulb(bool up)
        {
            int n = V85ABulbHalf.Length / 2;
            var pts = new List<float>();
            for (int i = 0; i < n; i++) { pts.Add(V85ABulbHalf[i * 2]); pts.Add((up ? 1f : -1f) * V85ABulbHalf[i * 2 + 1]); }
            for (int i = n - 1; i >= 0; i--) { pts.Add(-V85ABulbHalf[i * 2]); pts.Add((up ? 1f : -1f) * V85ABulbHalf[i * 2 + 1]); }
            return Sd.Polygon(pts.ToArray());
        }

        static Sdf V85A_GlassShape() => Sd.Union(V85A_Bulb(true), V85A_Bulb(false), Sd.Box(0f, 0f, 0.075f, 0.1f));

        /// <summary>
        /// Lo sfondo (coordinate del mondo): parete prugna a rombi fino al piano della scrivania, calendario a strappo a
        /// sinistra, la scrivania (piano da y -1.1 a -1.6 in leggera prospettiva, fronte fino a -2.45), pavimento scuro.
        /// </summary>
        public static DoodleArt V85A_Bg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0f, 1.8f, 3.3f, 3.5f), (x, y) =>
            {
                float u = (x + y) * 1.6f, v = (x - y) * 1.6f;
                bool alt = ((int)Math.Floor(u + 50f) + (int)Math.Floor(v + 50f)) % 2 == 0;
                Rgba c = alt ? Rgba.Hex("4E3B6B") : Rgba.Hex("47365F");
                return Rgba.Lerp(c, Rgba.Hex("2C2140"), V85A_Clamp((Math.Abs(x) - 1.4f) / 1.8f) * 0.7f + V85A_Clamp((1.5f - y) / 4f) * 0.25f);
            });
            // battiscopa della parete dietro la scrivania
            p.Shape(Sd.Box(0f, -1.0f, 3.3f, 0.06f), Rgba.Hex("6B567F"), 0.03f);
            // calendario a strappo (a sinistra, sotto il display)
            const float cx = -2.0f, cy = 0.75f;
            Body(p, Sd.Box(cx, cy, 0.42f, 0.5f, 0.04f), Rgba.Hex("F3EEE2"), Rgba.Hex("D9D0BE"));
            p.Shape(Sd.Box(cx, cy + 0.38f, 0.42f, 0.12f, 0.03f), Rgba.Hex("E8433F"), 0.03f);
            p.Shape(Sd.Circle(cx, cy + 0.56f, 0.04f), Rgba.Hex("C9C9D2"), 0.015f);
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 4; c++)
                    p.Fill(Sd.Box(cx - 0.27f + c * 0.18f, cy + 0.1f - r * 0.17f, 0.05f, 0.04f), Rgba.Hex("8B8296", 0.8f));
            p.Line(Rgba.Hex("E8433F"), 0.025f, cx - 0.33f, cy - 0.3f, cx - 0.17f, cy - 0.18f);   // un giorno cancellato
            p.Line(Rgba.Hex("E8433F"), 0.025f, cx - 0.33f, cy - 0.18f, cx - 0.17f, cy - 0.3f);
            // pianta in vaso a destra (lontana dalla clessidra)
            Body(p, Sd.Polygon(1.75f, -1.25f, 2.25f, -1.25f, 2.18f, -0.75f, 1.82f, -0.75f), Rgba.Hex("D97A4A"), Rgba.Hex("B35C33"));
            foreach (float a in new[] { -35f, 0f, 35f })
                p.Shape(Sd.Rotate(Sd.Ellipse(2.0f, -0.35f, 0.11f, 0.38f), 2.0f, -0.72f, a), Rgba.Hex("5BBF6A"), 0.025f);
            // la scrivania: piano in prospettiva
            Sdf top = Sd.Polygon(-2.85f, -1.6f, 2.85f, -1.6f, 2.6f, -1.1f, -2.6f, -1.1f);
            p.FillFunc(top, (x, y) => Rgba.Lerp(Rgba.Hex("C98348"), Rgba.Hex("A8652F"), V85A_Clamp((y + 1.6f) / 0.5f) * 0.6f + Noise.Value(x * 3f, y * 14f) * 0.15f));
            p.Shape(top, Rgba.Clear, 0.04f);
            // fronte e gambe
            Body(p, Sd.Box(0f, -2.0f, 2.95f, 0.4f, 0.03f), Rgba.Hex("B66E33"), WoodDark);
            p.Shape(Sd.Box(-1.2f, -2.0f, 0.55f, 0.2f, 0.03f), Rgba.Hex("A8622C"), 0.025f);
            p.Shape(Sd.Box(1.2f, -2.0f, 0.55f, 0.2f, 0.03f), Rgba.Hex("A8622C"), 0.025f);
            foreach (float s in new[] { -1.2f, 1.2f }) p.Shape(Sd.Circle(s, -2.0f, 0.04f), Rgba.Hex("E8C77A"), 0.015f);
            // pavimento scuro (il pulsante sta qui)
            p.FillFunc(Sd.Box(0f, -3.9f, 3.3f, 1.5f), (x, y) => (int)Math.Floor((x + 10f) * 1.25f) % 2 == 0 ? Rgba.Hex("2F2537") : Rgba.Hex("282030"));
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Box(s * 2.6f, -2.75f, 0.14f, 0.35f, 0.03f), WoodShade, WoodDark);
            p.Fill(Sd.Box(0f, -2.45f, 3.3f, 0.04f), Rgba.Hex("000000", 0.3f));
            return new DoodleArt("v85_a_bg", p);
        }

        /// <summary>
        /// La clessidra (origine al collo, alta 3.7): vetro trasparente (riempimento azzurro leggero, contorno, riflessi),
        /// tappi di legno a ±1.72 e tre colonnine. Simmetrica per mezzo giro (il livello la gira di 180°).
        /// </summary>
        public static DoodleArt V85A_Glass()
        {
            var p = new DoodlePainter(-1.05f, -1.95f, 1.05f, 1.95f, 110) { Ink = 0.045f };
            // colonnine dietro
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Box(s * 0.86f, 0f, 0.055f, 1.62f, 0.03f), Wood, WoodShade);
            Sdf glass = V85A_GlassShape();
            p.Fill(glass, Rgba.Hex("CFEAFF", 0.2f));
            p.Shape(glass, Rgba.Clear, 0.04f);
            p.InkColor = V85AGlassInk;
            // riflessi (uguali nei due bulbi: mezzo giro e la clessidra resta uguale)
            foreach (float s in new[] { 1f, -1f })
            {
                p.Fill(Sd.Rotate(Sd.Ellipse(-0.45f * s, 1.0f * s, 0.06f, 0.3f), -0.45f * s, 1.0f * s, -12f), Rgba.White.WithAlpha(0.55f));
                p.Fill(Sd.Circle(-0.5f * s, 1.38f * s, 0.035f), Rgba.White.WithAlpha(0.5f));
            }
            p.InkColor = DoodlePainter.DefaultInk;
            // tappi di legno
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Box(0f, s * 1.72f, 0.98f, 0.13f, 0.06f), Wood, WoodShade);
                p.Line(WoodDark, 0.02f, -0.85f, s * 1.72f, 0.85f, s * 1.72f);
                foreach (float k in new[] { -1f, 1f }) p.Shape(Sd.Circle(k * 0.86f, s * 1.72f, 0.05f), Rgba.Hex("E8C77A"), 0.015f);
            }
            return new DoodleArt("v85_a_glass", p);
        }

        /// <summary>
        /// La sabbia che riempie TUTTO un bulbo (sopra: top=true; coordinate locali della clessidra). Il livello la ritaglia
        /// con una finestra che sale dal fondo del bulbo (sotto) o dal collo (sopra).
        /// </summary>
        public static DoodleArt V85A_Sand(bool top)
        {
            float s = top ? 1f : -1f;
            var p = new DoodlePainter(-0.8f, top ? -0.02f : -1.64f, 0.8f, top ? 1.64f : 0.02f, 110) { Ink = 0.03f, Wobble = 0.006f };
            Sdf inside = Sd.Grow(V85A_Bulb(top), -0.045f);
            p.FillFunc(inside, (x, y) =>
            {
                float n = Noise.Value(x * 22f, y * 22f + (top ? 0f : 40f));
                Rgba c = Rgba.Lerp(V85ASand, V85ASandShade, V85A_Clamp(Math.Abs(x) / 0.7f) * 0.45f + n * 0.25f);
                return c;
            });
            // granelli
            var rnd = new Random(top ? 851 : 852);
            for (int i = 0; i < 70; i++)
            {
                float gx = (float)(rnd.NextDouble() * 1.4 - 0.7), gy = s * (float)(rnd.NextDouble() * 1.55);
                if (inside(gx, gy) < -0.03f) p.Fill(Sd.Circle(gx, gy, 0.012f), Rgba.Hex("B9822F", 0.7f), false);
            }
            return new DoodleArt(top ? "v85_a_sand_top" : "v85_a_sand_bot", p);
        }

        /// <summary>Il display digitale (centro 0,0, largo 2.9, alto 1.05): cornice di plastica scura, schermo nero lucido; le cifre sono un testo.</summary>
        public static DoodleArt V85A_Display()
        {
            var p = new DoodlePainter(-1.52f, -0.6f, 1.52f, 0.6f, 110) { Ink = 0.045f };
            p.InkLine(0.025f, -0.6f, 0.52f, 0f, 0.6f, 0.6f, 0.52f);   // filo del chiodo
            p.Shape(Sd.Circle(0f, 0.6f, 0.04f), Rgba.Hex("C9C9D2"), 0.015f);
            Body(p, Sd.Box(0f, -0.02f, 1.45f, 0.5f, 0.12f), Rgba.Hex("3A3346"), Rgba.Hex("241F2E"));
            p.FillFunc(Sd.Box(0f, -0.02f, 1.28f, 0.38f, 0.06f), (x, y) => Rgba.Lerp(Rgba.Hex("1A0E12"), Rgba.Hex("2A161C"), V85A_Clamp((y + 0.4f) / 0.8f)));
            p.Shape(Sd.Box(0f, -0.02f, 1.28f, 0.38f, 0.06f), Rgba.Clear, 0.025f);
            p.Fill(Sd.Rotate(Sd.Box(-0.75f, 0.2f, 0.35f, 0.04f), -0.75f, 0.2f, 8f), Rgba.White.WithAlpha(0.12f));
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Circle(s * 1.36f, -0.42f, 0.03f), Rgba.Hex("8B8296"), 0.012f);
            return new DoodleArt("v85_a_display", p);
        }

        /// <summary>Il pulsante ADD TIME (centro 0,0, largo 2.7, alto 1): tasto verde a pillola con l'orologino "+" a sinistra; la scritta è un testo.</summary>
        public static DoodleArt V85A_Button()
        {
            var p = new DoodlePainter(-1.42f, -0.6f, 1.42f, 0.56f, 110) { Ink = 0.05f };
            p.Shape(Sd.Box(0f, -0.14f, 1.34f, 0.42f, 0.4f), Rgba.Hex("1E6B3A"));
            Body(p, Sd.Box(0f, 0.04f, 1.3f, 0.42f, 0.4f), Rgba.Hex("49C46F"), Rgba.Hex("2E9A52"), null, -0.06f, 0.07f);
            p.Fill(Sd.Box(-0.6f, 0.32f, 0.38f, 0.045f, 0.04f), Rgba.White.WithAlpha(0.6f));
            // orologino col "+"
            p.Shape(Sd.Circle(-0.9f, 0.04f, 0.24f), Rgba.Hex("F7F3E8"), 0.03f);
            p.Line(Rgba.Hex("2E9A52"), 0.06f, -1.02f, 0.04f, -0.78f, 0.04f);
            p.Line(Rgba.Hex("2E9A52"), 0.06f, -0.9f, -0.08f, -0.9f, 0.16f);
            return new DoodleArt("v85_a_button", p);
        }
    }
}
