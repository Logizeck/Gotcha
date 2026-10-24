using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I pochi disegni nuovi della 0.62 (livelli 251-255, "vecchi oggetti, azioni nuove"): tutto il resto riusa gli
    /// sprite esistenti. Solo quello che mancava davvero: l'amo, il rubinetto senza manopola, l'ombrello chiuso,
    /// l'acqua dentro l'ombrello rovesciato e il tetto su cui aspetta il gatto.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV62() => new DoodleArt[]
        {
            V62_Hook(), V62_FaucetBare(), V62_UmbrellaClosed(), V62_Water(), V62_Roof(),
        };

        /// <summary>Amo da pesca (occhiello in 0,0; la curva scende fino a y = -0.3).</summary>
        public static DoodleArt V62_Hook()
        {
            var p = new DoodlePainter(-0.14f, -0.36f, 0.14f, 0.08f, 200) { Ink = 0.02f, Wobble = 0.004f };
            Rgba steel = Rgba.Hex("9AA1B4");
            p.Shape(Sd.Subtract(Sd.Circle(0f, 0f, 0.045f), Sd.Circle(0f, 0f, 0.022f)), steel, 0.015f);
            p.Shape(Sd.Polyline(0.018f, 0f, -0.04f, 0f, -0.24f, -0.02f, -0.29f, -0.06f, -0.31f, -0.1f, -0.29f, -0.11f, -0.24f, -0.09f, -0.2f), steel, 0.015f);
            p.Fill(Sd.Polygon(-0.11f, -0.17f, -0.07f, -0.21f, -0.115f, -0.22f), Ink);
            return new DoodleArt("v62_hook", p);
        }

        /// <summary>Rubinetto a muro che esce verso destra (bocca in 0,0) a cui manca la manopola: c'è solo il perno.</summary>
        public static DoodleArt V62_FaucetBare()
        {
            var p = new DoodlePainter(-0.7f, -0.35f, 0.1f, 0.5f, 130) { Ink = 0.04f };
            Body(p, Sd.Box(-0.3f, 0, 0.32f, 0.14f, 0.05f), Rgba.Hex("C9CFD6"), Rgba.Hex("8D93A6"));
            Body(p, Sd.Box(-0.35f, 0.2f, 0.04f, 0.07f), Rgba.Hex("8D93A6"), Rgba.Hex("5E6275"));
            p.Shape(Sd.Box(-0.35f, 0.29f, 0.025f, 0.03f), Rgba.Hex("5E6275"), 0.02f);   // il perno spoglio
            p.Fill(Sd.Box(-0.06f, -0.05f, 0.05f, 0.03f), Rgba.Hex("2B2133", 0.6f));    // bocca asciutta
            return new DoodleArt("v62_faucet_bare", p);
        }

        /// <summary>
        /// Ombrello chiuso, coi colori di "gs_umbrella" e lo stesso riquadro di disegno (centro della cupola in 0,0,
        /// il manico arriva a y = -1.15): si scambia con l'ombrello aperto senza spostarlo.
        /// </summary>
        public static DoodleArt V62_UmbrellaClosed()
        {
            var p = new DoodlePainter(-0.95f, -1.25f, 0.95f, 0.62f, 110) { Ink = 0.045f };
            p.Shape(Sd.Polyline(0.045f, 0f, 0.5f, 0f, -1.05f, 0.1f, -1.15f, 0.2f, -1.08f), Rgba.Hex("D8C7A8"));
            Sdf furl = Sd.Polygon(0f, 0.5f, 0.17f, -0.3f, 0.11f, -0.6f, -0.11f, -0.6f, -0.17f, -0.25f);
            p.FillFunc(furl, (x, y) => ((int)Math.Floor((x * 3f + y * 1.2f + 3f) * 2.2f)) % 2 == 0 ? Rgba.Hex("E8433F") : Rgba.Hex("FFF4DC"));
            p.Fill(Sd.Crescent(furl, -0.05f, 0.02f), Rgba.Hex("000000", 0.12f));
            p.Shape(furl, Rgba.Clear);
            p.Shape(Sd.Box(0f, -0.22f, 0.17f, 0.04f, 0.02f), Rgba.Hex("E8433F"), 0.025f);   // la fascetta
            p.Shape(Sd.Circle(0f, 0.53f, 0.045f), Rgba.Hex("FFD24A"), 0.025f);
            return new DoodleArt("v62_umbrella_closed", p);
        }

        /// <summary>Acqua dentro la cupola dell'ombrello rovesciato (fondo in 0,0, pelo dell'acqua in y = 0.6).</summary>
        public static DoodleArt V62_Water()
        {
            var p = new DoodlePainter(-0.9f, -0.06f, 0.9f, 0.68f, 110) { Ink = 0.035f, Wobble = 0.006f };
            Sdf bowl = Sd.Intersect(Sd.Ellipse(0f, 0.62f, 0.8f, 0.62f), Sd.Box(0f, 0.31f, 0.85f, 0.31f));
            p.FillFunc(bowl, (x, y) => Rgba.Lerp(Rgba.Hex("2F7FD8", 0.92f), Rgba.Hex("7CC4F5", 0.92f), Clamp(y / 0.6f)));
            p.Line(Rgba.Hex("D9F1FF"), 0.04f, -0.72f, 0.59f, 0.72f, 0.59f);
            p.Line(Rgba.White.WithAlpha(0.7f), 0.03f, -0.4f, 0.4f, -0.2f, 0.42f);
            return new DoodleArt("v62_water", p);
        }

        /// <summary>
        /// Casetta vista di fronte con il tetto di tegole (il colmo piatto è in y = 0, largo da -1.1 a 1.3; il muro
        /// scende fino a y = -2.4). Va in alto a destra: il gatto si siede sul colmo.
        /// </summary>
        public static DoodleArt V62_Roof()
        {
            var p = new DoodlePainter(-1.75f, -2.45f, 1.95f, 0.75f, 70) { Ink = 0.05f };
            // muro con la finestra
            Body(p, Sd.Box(0.1f, -1.65f, 1.35f, 0.8f), Rgba.Hex("F4E3C3"), Rgba.Hex("D9C29A"));
            p.Shape(Sd.Box(-0.4f, -1.45f, 0.38f, 0.33f, 0.04f), Rgba.Hex("BFE3F7"), 0.04f);
            p.InkLine(0.03f, -0.4f, -1.78f, -0.4f, -1.12f);
            p.InkLine(0.03f, -0.78f, -1.45f, -0.02f, -1.45f);
            // camino
            Body(p, Sd.Box(0.85f, 0.25f, 0.18f, 0.35f), Rgba.Hex("B5533C"), Rgba.Hex("8A3A28"));
            p.Shape(Sd.Box(0.85f, 0.6f, 0.24f, 0.06f, 0.02f), Rgba.Hex("8A3A28"), 0.035f);
            // tetto a trapezio con le file di tegole
            Sdf roof = Sd.Polygon(-1.7f, -0.85f, 1.9f, -0.85f, 1.3f, 0f, -1.1f, 0f);
            p.FillFunc(roof, (x, y) =>
            {
                float row = -y / 0.28f;
                int ri = (int)Math.Floor(row);
                float col = (x + 1.6f - (ri % 2) * 0.16f) / 0.32f;
                bool seam = row - ri < 0.1f || col - (float)Math.Floor(col) < 0.08f;
                return seam ? Rgba.Hex("A33A28") : Rgba.Hex("D9573F");
            });
            p.Fill(Sd.Crescent(roof, 0.05f, 0.08f), Rgba.Hex("000000", 0.1f));
            p.Shape(roof, Rgba.Clear);
            p.Shape(Sd.Box(0.1f, 0.02f, 1.24f, 0.05f, 0.02f), Rgba.Hex("A33A28"), 0.035f);   // il colmo
            return new DoodleArt("v62_roof", p);
        }
    }
}
