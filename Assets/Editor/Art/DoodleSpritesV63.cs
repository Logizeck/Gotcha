using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I pochi disegni nuovi della 0.63 (252 rifatto e livelli 256-258): il resto riusa gli sprite esistenti.
    /// Solo quello che mancava: il cuscino del gatto senza gatto (quando si alza), la coda che copre gli occhi,
    /// il cerchio tratteggiato degli "slot" (bianco, si colora dal livello) e il quadrante d'orologio senza lancette
    /// (le lancette sono barre che il livello fa girare).
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV63() => new DoodleArt[]
        {
            V63_CatCushion(), V63_CatTail(), V63_SlotRing(), V63_ClockFace(),
        };

        /// <summary>Il cuscino viola del gatto che dorme ("cat"), da solo e nella stessa posizione del disegno.</summary>
        public static DoodleArt V63_CatCushion()
        {
            var p = new DoodlePainter(-1.6f, -0.95f, 1.6f, 1.15f, 120) { Ink = 0.05f };
            Sdf cushion = Sd.Box(0, -0.45f, 1.35f, 0.3f, 0.26f);
            foreach (float x in new[] { -1.32f, 1.32f })
            {
                p.InkLine(0.025f, x, -0.6f, x + Math.Sign(x) * 0.12f, -0.82f);
                p.Shape(Sd.Circle(x + Math.Sign(x) * 0.13f, -0.85f, 0.06f), Rgba.Hex("FFD25A"), 0.025f);
            }
            Body(p, cushion, Rgba.Hex("8E6CEF"), Rgba.Hex("6B4CC9"));
            p.Line(Rgba.Hex("B9A3FF"), 0.02f, -1.1f, -0.45f, 1.1f, -0.45f);
            foreach (float x in new[] { -0.6f, 0f, 0.6f }) p.Shape(Sd.Circle(x, -0.45f, 0.035f), Rgba.Hex("B9A3FF"), 0.02f);
            return new DoodleArt("v63_cat_cushion", p);
        }

        /// <summary>
        /// La coda del gatto che dorme portata sugli occhi (stesse coordinate di "cat": la testa è in -0.72, 0.33).
        /// </summary>
        public static DoodleArt V63_CatTail()
        {
            var p = new DoodlePainter(-1.3f, 0.0f, 0.2f, 0.7f, 120) { Ink = 0.045f };
            Sdf tail = Sd.Polyline(0.1f, 0.05f, 0.25f, -0.3f, 0.42f, -0.72f, 0.4f, -1.08f, 0.3f);
            p.Shape(tail, CatOrange);
            for (int i = 0; i < 3; i++)
            {
                float x = -0.25f - i * 0.25f;
                p.Line(CatShade, 0.04f, x, 0.48f - i * 0.03f, x - 0.04f, 0.34f - i * 0.02f);
            }
            p.Shape(tail, Rgba.Clear);
            return new DoodleArt("v63_cat_tail", p);
        }

        /// <summary>Cerchio tratteggiato bianco (raggio 0.5, centro 0,0): il livello lo colora (vuoto, giusto, rotto).</summary>
        public static DoodleArt V63_SlotRing()
        {
            var p = new DoodlePainter(-0.58f, -0.58f, 0.58f, 0.58f, 120) { Ink = 0.03f, Wobble = 0.004f };
            p.Fill(Sd.Circle(0f, 0f, 0.5f), Rgba.White.WithAlpha(0.18f));
            const int dashes = 14;
            for (int i = 0; i < dashes; i++)
            {
                double a0 = (i + 0.15) * 2 * Math.PI / dashes, a1 = (i + 0.85) * 2 * Math.PI / dashes, am = (a0 + a1) / 2;
                p.Line(Rgba.White, 0.06f,
                    (float)Math.Cos(a0) * 0.47f, (float)Math.Sin(a0) * 0.47f,
                    (float)Math.Cos(am) * 0.47f, (float)Math.Sin(am) * 0.47f,
                    (float)Math.Cos(a1) * 0.47f, (float)Math.Sin(a1) * 0.47f);
            }
            return new DoodleArt("v63_slot_ring", p);
        }

        /// <summary>Quadrante d'orologio senza lancette (raggio 0.5, centro 0,0) con le 12 tacche.</summary>
        public static DoodleArt V63_ClockFace()
        {
            var p = new DoodlePainter(-0.56f, -0.56f, 0.56f, 0.56f, 160) { Ink = 0.035f };
            p.Shape(Sd.Circle(0f, 0f, 0.5f), Rgba.Hex("FFF8E8"));
            for (int i = 0; i < 12; i++)
            {
                double a = i * Math.PI / 6;
                float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
                p.InkLine(i % 3 == 0 ? 0.035f : 0.018f, c * 0.36f, s * 0.36f, c * 0.44f, s * 0.44f);
            }
            return new DoodleArt("v63_clock_face", p);
        }
    }
}
