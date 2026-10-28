using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I pochi disegni nuovi della 0.67 per il livello 261 "The Dark Room" (tutto il resto riusa gli sprite esistenti:
    /// il soggiorno, la finestra, le tende, la lampada da comodino, la radio, l'orologio, la candela, i fiammiferi, il
    /// topo, l'interruttore, il sole e l'uccellino): la lampada del soffitto SENZA lampadina, la presa nel muro, la spina,
    /// il pezzo di filo rosicchiato e la nappina del cordino delle tende.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV67A() => new DoodleArt[]
        {
            V67_LampEmpty(), V67_WallSocket(), V67_Plug(), V67_Frayed(), V67_Tassel(),
        };

        /// <summary>Lampada appesa al soffitto con il portalampada vuoto (paralume in 0,0; filo verso l'alto fino a 1.6).</summary>
        public static DoodleArt V67_LampEmpty()
        {
            var p = new DoodlePainter(-0.62f, -0.3f, 0.62f, 1.6f, 120) { Ink = 0.045f };
            p.InkLine(0.04f, 0f, 1.6f, 0f, 0.35f);
            // portalampada vuoto: un cilindretto con il buco scuro (niente lampadina!)
            p.Shape(Sd.Box(0f, -0.06f, 0.085f, 0.1f, 0.02f), Rgba.Hex("B9BCC6"), 0.03f);
            p.Fill(Sd.Ellipse(0f, -0.15f, 0.06f, 0.025f), Rgba.Hex("2B2233"));
            p.Line(Rgba.Hex("8D93A6"), 0.015f, -0.07f, -0.02f, 0.07f, -0.04f);
            p.Line(Rgba.Hex("8D93A6"), 0.015f, -0.07f, -0.08f, 0.07f, -0.1f);
            Body(p, Sd.Polygon(-0.55f, 0.02f, 0.55f, 0.02f, 0.25f, 0.42f, -0.25f, 0.42f), Rgba.Hex("F2C14E"), Rgba.Hex("D19A2B"));
            p.Line(Rgba.Hex("B87F1E"), 0.03f, -0.5f, 0.08f, 0.5f, 0.08f);
            return new DoodleArt("v67_lamp_empty", p);
        }

        /// <summary>Presa di corrente nel muro (centro 0,0; placca 0.5 x 0.5 con due fori).</summary>
        public static DoodleArt V67_WallSocket()
        {
            var p = new DoodlePainter(-0.3f, -0.3f, 0.3f, 0.3f, 160) { Ink = 0.035f };
            p.Shape(Sd.Box(0f, 0f, 0.24f, 0.24f, 0.06f), Rgba.Hex("F4F1EA"));
            p.Shape(Sd.Circle(0f, 0f, 0.15f), Rgba.Hex("E2DDD2"), 0.025f);
            p.Fill(Sd.Circle(-0.055f, 0f, 0.025f), Rgba.Hex("2B2233"));
            p.Fill(Sd.Circle(0.055f, 0f, 0.025f), Rgba.Hex("2B2233"));
            return new DoodleArt("v67_socket", p);
        }

        /// <summary>Spina (centro 0,0): il corpo, i due spinotti verso destra (+x) e il filo che esce a sinistra.</summary>
        public static DoodleArt V67_Plug()
        {
            var p = new DoodlePainter(-0.3f, -0.18f, 0.34f, 0.18f, 160) { Ink = 0.03f };
            p.Line(Rgba.Hex("3A3340"), 0.06f, -0.3f, -0.02f, -0.12f, 0f);
            foreach (float y in new[] { -0.055f, 0.055f })
                p.Shape(Sd.Box(0.22f, y, 0.09f, 0.022f, 0.01f), Rgba.Hex("D9B44A"), 0.018f);
            Body(p, Sd.Box(0f, 0f, 0.14f, 0.13f, 0.05f), Rgba.Hex("EDEAE3"), Rgba.Hex("C9C3B6"));
            p.Line(Rgba.Hex("B5AE9F"), 0.015f, -0.05f, -0.07f, -0.05f, 0.07f);
            return new DoodleArt("v67_plug", p);
        }

        /// <summary>Pezzo di filo rosicchiato (centro 0,0): la guaina col morso e i fili di rame che spuntano.</summary>
        public static DoodleArt V67_Frayed()
        {
            var p = new DoodlePainter(-0.26f, -0.16f, 0.26f, 0.16f, 160) { Ink = 0.025f };
            Rgba copper = Rgba.Hex("E08A3C");
            // i fili di rame si sfrangiano a ventaglio dalle due estremità del morso
            foreach (float a in new[] { -0.08f, -0.03f, 0.02f, 0.07f })
            {
                p.Line(copper, 0.014f, -0.08f, a * 0.15f, -0.015f, a);
                p.Line(copper, 0.014f, 0.08f, -a * 0.15f, 0.015f, -a * 0.9f);
            }
            Sdf sheath = Sd.Subtract(Sd.Union(Sd.Box(-0.17f, 0f, 0.09f, 0.03f, 0.02f), Sd.Box(0.17f, 0f, 0.09f, 0.03f, 0.02f)),
                Sd.Union(Sd.Circle(-0.09f, 0.035f, 0.025f), Sd.Circle(0.1f, -0.035f, 0.025f)));
            p.Shape(sheath, Rgba.Hex("3A3340"), 0.02f);
            return new DoodleArt("v67_frayed", p);
        }

        /// <summary>Nappina in fondo al cordino delle tende (attacco in 0,0, pende verso il basso fino a -0.4).</summary>
        public static DoodleArt V67_Tassel()
        {
            var p = new DoodlePainter(-0.16f, -0.42f, 0.16f, 0.04f, 160) { Ink = 0.03f };
            Rgba gold = Rgba.Hex("F2C14E"), shade = Rgba.Hex("C9932B");
            Body(p, Sd.Polygon(-0.05f, -0.1f, 0.05f, -0.1f, 0.12f, -0.38f, -0.12f, -0.38f), gold, shade);
            for (int i = -2; i <= 2; i++) p.Line(shade, 0.012f, i * 0.035f, -0.14f, i * 0.05f, -0.36f);
            Body(p, Sd.Circle(0f, -0.06f, 0.055f), gold, shade);
            return new DoodleArt("v67_tassel", p);
        }
    }
}
