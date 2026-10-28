using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I pochi disegni nuovi della 0.67 per il livello 262 "The Immortal Candle" (torta, candelina, fiammella, sfondo
    /// della festa, sbuffi, gocce e linee del vento sono quelli esistenti): il ventilatore a colonna (corpo, pale e
    /// griglia separati, così le pale girano dietro la griglia), il bicchiere d'acqua e le due facce della fiammella
    /// (ridacchia quando la si tocca con un dito; la linguaccia finale). Le facce sono nelle coordinate dello sprite
    /// "flame" (base in 0,0, alta 0.26): si appoggiano sopra la fiammella con la stessa scala.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV67B() => new DoodleArt[]
        {
            V67_FanBody(), V67_FanBlades(), V67_FanGrill(), V67_Glass(), V67_FlameGiggle(), V67_FlameCheeky(),
        };

        /// <summary>Centro della griglia del ventilatore rispetto alla base (coordinate dello sprite v67_fan_body).</summary>
        public const float V67FanHeadY = 2.6f;

        static readonly Rgba V67Fan = Rgba.Hex("7ED3B2"), V67FanShade = Rgba.Hex("4FA88A");

        /// <summary>Ventilatore a colonna: base tonda, asta lunga, motore e fondo della gabbia (base in 0,0; testa a y 2.6).</summary>
        public static DoodleArt V67_FanBody()
        {
            var p = new DoodlePainter(-0.66f, -0.08f, 0.66f, 3.2f, 100) { Ink = 0.045f };
            float hy = V67FanHeadY;
            Body(p, Sd.Ellipse(0f, 0.1f, 0.48f, 0.13f), V67Fan, V67FanShade);
            p.Fill(Sd.Ellipse(-0.12f, 0.14f, 0.18f, 0.04f), Rgba.White.WithAlpha(0.45f));
            Body(p, Sd.Capsule(0f, 0.16f, 0f, hy - 0.45f, 0.055f), Rgba.Hex("D9DEE8"), Rgba.Hex("A9B0C0"));
            // pulsantini sulla base
            p.Shape(Sd.Circle(0.22f, 0.1f, 0.035f), Rgba.Hex("FF8FA3"), 0.02f);
            p.Shape(Sd.Circle(0.33f, 0.08f, 0.035f), Rgba.Hex("FFD25A"), 0.02f);
            // motore dietro la gabbia e fondo della gabbia (azzurrino trasparente)
            Body(p, Sd.Ellipse(0f, hy - 0.42f, 0.16f, 0.12f), V67Fan, V67FanShade);
            p.Shape(Sd.Circle(0f, hy, 0.56f), Rgba.Hex("E8F6FF", 0.55f), 0.04f);
            return new DoodleArt("v67_fan_body", p);
        }

        /// <summary>Tre pale col mozzo (centro 0,0, raggio 0.5): ruotano attorno allo zero.</summary>
        public static DoodleArt V67_FanBlades()
        {
            var p = new DoodlePainter(-0.52f, -0.52f, 0.52f, 0.52f, 140) { Ink = 0.03f, Wobble = 0.004f };
            var blades = new List<Sdf>();
            for (int i = 0; i < 3; i++)
                blades.Add(Sd.Rotate(Sd.Ellipse(0f, 0.27f, 0.15f, 0.22f), 0f, 0f, i * 120f + 15f));
            Body(p, Sd.Union(blades.ToArray()), Rgba.Hex("BDEBFF", 0.9f), Rgba.Hex("7CC4E6", 0.9f));
            Body(p, Sd.Circle(0f, 0f, 0.1f), V67Fan, V67FanShade);
            return new DoodleArt("v67_fan_blades", p);
        }

        /// <summary>Griglia davanti alle pale: cerchio esterno, raggi sottili e bottone centrale (centro 0,0).</summary>
        public static DoodleArt V67_FanGrill()
        {
            var p = new DoodlePainter(-0.62f, -0.62f, 0.62f, 0.62f, 140) { Ink = 0.03f, Wobble = 0.004f };
            Rgba wire = Rgba.Hex("8A93A8");
            for (int i = 0; i < 12; i++)
            {
                double a = i * Math.PI / 6.0;
                p.Line(wire, 0.014f, 0.11f * (float)Math.Cos(a), 0.11f * (float)Math.Sin(a), 0.55f * (float)Math.Cos(a), 0.55f * (float)Math.Sin(a));
            }
            p.Shape(Sd.Subtract(Sd.Circle(0f, 0f, 0.36f), Sd.Circle(0f, 0f, 0.34f)), wire, 0.008f);
            p.Shape(Sd.Subtract(Sd.Circle(0f, 0f, 0.58f), Sd.Circle(0f, 0f, 0.52f)), Rgba.Hex("D9DEE8"));
            Body(p, Sd.Circle(0f, 0f, 0.1f), Rgba.Hex("FF8FA3"), Rgba.Hex("D9607A"));
            return new DoodleArt("v67_fan_grill", p);
        }

        /// <summary>Bicchiere d'acqua (base in 0,0, alto 0.95, bordo largo ±0.34; acqua fino a 0.72).</summary>
        public static DoodleArt V67_Glass()
        {
            var p = new DoodlePainter(-0.44f, -0.08f, 0.44f, 1.03f, 140) { Ink = 0.04f };
            Sdf outer = Sd.Polygon(-0.27f, 0f, 0.27f, 0f, 0.34f, 0.95f, -0.34f, 0.95f);
            Sdf inner = Sd.Polygon(-0.22f, 0.07f, 0.22f, 0.07f, 0.29f, 0.95f, -0.29f, 0.95f);
            p.Fill(outer, Rgba.Hex("DFF3FF", 0.6f));
            p.Fill(Sd.Intersect(inner, Sd.Box(0f, 0.36f, 0.4f, 0.36f)), Rgba.Hex("6EC6FF", 0.75f));
            p.Shape(Sd.Intersect(inner, Sd.Box(0f, 0.72f, 0.4f, 0.012f)), Rgba.Hex("BFE8FF"), 0.012f);   // pelo dell'acqua
            p.Line(Rgba.White.WithAlpha(0.85f), 0.045f, -0.18f, 0.84f, -0.15f, 0.18f);
            p.Line(Rgba.White.WithAlpha(0.6f), 0.025f, -0.07f, 0.84f, -0.06f, 0.6f);
            p.Shape(outer, Rgba.Clear);
            p.Shape(Sd.Ellipse(0f, 0.95f, 0.34f, 0.04f), Rgba.Hex("EAF7FF", 0.7f), 0.025f);
            return new DoodleArt("v67_glass", p);
        }

        /// <summary>Faccina che ridacchia (occhi a "^", bocca aperta che ride, guance), coordinate dello sprite "flame".</summary>
        public static DoodleArt V67_FlameGiggle()
        {
            var p = new DoodlePainter(-0.07f, -0.01f, 0.07f, 0.15f, 700) { Ink = 0.006f, Wobble = 0.0008f };
            p.InkLine(0.007f, -0.036f, 0.086f, -0.024f, 0.1f, -0.012f, 0.086f);
            p.InkLine(0.007f, 0.012f, 0.086f, 0.024f, 0.1f, 0.036f, 0.086f);
            p.Shape(Sd.Intersect(Sd.Circle(0f, 0.062f, 0.022f), Sd.Box(0f, 0.045f, 0.03f, 0.017f)), Rgba.Hex("B8323C"));
            p.Fill(Sd.Ellipse(-0.045f, 0.064f, 0.012f, 0.007f), Rgba.Hex("FF6F5A", 0.75f));
            p.Fill(Sd.Ellipse(0.045f, 0.064f, 0.012f, 0.007f), Rgba.Hex("FF6F5A", 0.75f));
            return new DoodleArt("v67_flame_giggle", p);
        }

        /// <summary>Linguaccia finale: un occhio aperto, uno che fa l'occhiolino, lingua rosa di fuori (coordinate di "flame").</summary>
        public static DoodleArt V67_FlameCheeky()
        {
            var p = new DoodlePainter(-0.07f, -0.04f, 0.07f, 0.15f, 700) { Ink = 0.006f, Wobble = 0.0008f };
            p.Shape(Sd.Ellipse(-0.024f, 0.098f, 0.011f, 0.015f), Rgba.White);
            p.Fill(Sd.Circle(-0.021f, 0.095f, 0.006f), Rgba.Hex("2B2233"));
            p.InkLine(0.007f, 0.012f, 0.096f, 0.024f, 0.104f, 0.038f, 0.1f);                    // occhiolino
            p.InkLine(0.006f, -0.03f, 0.066f, -0.01f, 0.058f, 0.012f, 0.058f, 0.03f, 0.064f);   // bocca storta
            Sdf tongue = Sd.Union(Sd.Box(0.004f, 0.04f, 0.017f, 0.02f, 0.008f), Sd.Circle(0.004f, 0.021f, 0.018f));
            p.Shape(tongue, Rgba.Hex("FF6F8A"));
            p.Line(Rgba.Hex("D9465F"), 0.004f, 0.004f, 0.054f, 0.004f, 0.02f);
            return new DoodleArt("v67_flame_cheeky", p);
        }
    }
}
