using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.72 per il livello 269 "The Cat Burglar" (il resto riusa gli sprite esistenti: la stanza
    /// col tappeto, il gatto seduto, il puntatore laser, il cuscino, la nuvoletta dei pensieri, i biscotti, la stellina):
    /// la "scala" dei mobili (sedia, tavolino, credenza alta, mensola), il barattolo dei biscotti e i suoi cocci, la
    /// scaletta corta, i tre libri, il puntino rosso col suo raggio, gli occhi mobili del gatto e la zampa che spinge.
    /// Prefisso v72_a_. Le origini sono scritte sopra ogni disegno (il livello usa gli stessi numeri).
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV72A() => new DoodleArt[]
        {
            V72A_Chair(), V72A_Table(), V72A_Cupboard(), V72A_Shelf(), V72A_CookieJar(), V72A_JarShards(), V72A_Stepladder(),
            V72A_Book("v72_a_book_red", "E8574F", "B73A35"), V72A_Book("v72_a_book_blue", "5B8DEF", "3F68C2"),
            V72A_Book("v72_a_book_green", "62C26A", "43974B"), V72A_RedDot(), V72A_Beam(), V72A_CatEyes(), V72A_CatPupil(), V72A_Paw(),
        };

        static readonly Rgba V72A_Wood = Rgba.Hex("C98B5A"), V72A_WoodShade = Rgba.Hex("A86E48"), V72A_WoodLight = Rgba.Hex("E2AE7C");

        /// <summary>La sedia di legno (piedi in 0,0; il sedile è a y 0.85, lo schienale dietro arriva a 1.7).</summary>
        public static DoodleArt V72A_Chair()
        {
            var p = new DoodlePainter(-0.48f, -0.05f, 0.48f, 1.78f, 110) { Ink = 0.04f };
            // schienale: due montanti e due traversi
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Box(s * 0.31f, 1.25f, 0.045f, 0.47f, 0.02f), V72A_WoodShade, Rgba.Hex("8C5636"));
            Body(p, Sd.Box(0f, 1.6f, 0.36f, 0.08f, 0.04f), V72A_Wood, V72A_WoodShade);
            Body(p, Sd.Box(0f, 1.3f, 0.31f, 0.045f, 0.02f), V72A_Wood, V72A_WoodShade);
            // gambe e traversino
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Box(s * 0.33f, 0.4f, 0.045f, 0.4f, 0.02f), V72A_Wood, V72A_WoodShade);
            p.Shape(Sd.Box(0f, 0.25f, 0.3f, 0.025f), V72A_WoodShade, 0.025f);
            // il sedile (con un cuscinetto a quadri)
            Body(p, Sd.Box(0f, 0.8f, 0.42f, 0.06f, 0.03f), V72A_WoodLight, V72A_Wood);
            p.Shape(Sd.Box(0f, 0.88f, 0.34f, 0.035f, 0.03f), Rgba.Hex("E58A9B"), 0.025f);
            return new DoodleArt("v72_a_chair", p);
        }

        /// <summary>Il tavolino (piano in y 0, largo 1.4; le gambe arrivano a -1.75).</summary>
        public static DoodleArt V72A_Table()
        {
            var p = new DoodlePainter(-0.76f, -1.8f, 0.76f, 0.06f, 100) { Ink = 0.045f };
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Box(s * 0.58f, -0.92f, 0.055f, 0.83f, 0.02f), V72A_Wood, V72A_WoodShade);
            Body(p, Sd.Box(0f, -0.2f, 0.62f, 0.07f, 0.02f), V72A_WoodShade, Rgba.Hex("8C5636"));
            // il cassetto, col pomello
            p.Shape(Sd.Box(0f, -0.2f, 0.2f, 0.045f, 0.02f), V72A_WoodLight, 0.02f);
            p.Shape(Sd.Circle(0f, -0.2f, 0.022f), Rgba.Hex("F2C45A"), 0.015f);
            Body(p, Sd.Box(0f, -0.06f, 0.7f, 0.065f, 0.03f), V72A_WoodLight, V72A_Wood);
            return new DoodleArt("v72_a_table", p);
        }

        /// <summary>La credenza alta (sopra in y 0, larga 1.45; il fondo è a -2.95), due ante coi pomelli e la cornice in cima.</summary>
        public static DoodleArt V72A_Cupboard()
        {
            var p = new DoodlePainter(-0.8f, -3.0f, 0.8f, 0.06f, 90) { Ink = 0.045f };
            Sdf body = Sd.Box(0f, -1.5f, 0.68f, 1.42f, 0.03f);
            Body(p, body, Rgba.Hex("F4E3C8"), Rgba.Hex("DCC6A4"));
            // zoccolo e cornice
            Body(p, Sd.Box(0f, -2.88f, 0.72f, 0.07f, 0.02f), V72A_Wood, V72A_WoodShade);
            Body(p, Sd.Box(0f, -0.06f, 0.75f, 0.07f, 0.03f), V72A_Wood, V72A_WoodShade);
            // le due ante (sopra con i vetrini, sotto piene)
            foreach (float s in new[] { -1f, 1f })
            {
                Sdf top = Sd.Box(s * 0.33f, -0.85f, 0.29f, 0.62f, 0.03f);
                p.Shape(top, Rgba.Hex("FFF6E6"), 0.03f);
                p.Fill(Sd.Box(s * 0.33f, -0.85f, 0.2f, 0.52f, 0.02f), Rgba.Hex("CFE7F2"));
                p.Fill(Sd.Capsule(s * 0.33f - 0.12f, -1.2f, s * 0.33f + 0.05f, -0.5f, 0.025f), new Rgba(1f, 1f, 1f, 0.6f));
                p.Shape(Sd.Box(s * 0.33f, -0.85f, 0.2f, 0.52f, 0.02f), Rgba.Clear, 0.025f);
                Sdf low = Sd.Box(s * 0.33f, -2.12f, 0.29f, 0.62f, 0.03f);
                p.Shape(low, Rgba.Hex("FFF6E6"), 0.03f);
                p.Shape(Sd.Box(s * 0.33f, -2.12f, 0.19f, 0.48f, 0.02f), Rgba.Hex("F1DDBE"), 0.02f);
                p.Shape(Sd.Circle(s * 0.08f, -1.5f, 0.035f), Rgba.Hex("F2C45A"), 0.018f);
            }
            // dentro i vetrini: piatti e una tazza
            p.Shape(Sd.Ellipse(-0.33f, -0.88f, 0.14f, 0.04f), Rgba.White, 0.02f);
            p.Shape(Sd.Ellipse(-0.33f, -0.95f, 0.14f, 0.04f), Rgba.White, 0.02f);
            p.Shape(Sd.Box(0.33f, -0.98f, 0.08f, 0.07f, 0.02f), Rgba.Hex("FF9BB0"), 0.02f);
            return new DoodleArt("v72_a_cupboard", p);
        }

        /// <summary>La mensola a muro (piano in y 0, larga 1.5) con le due mensoline sotto.</summary>
        public static DoodleArt V72A_Shelf()
        {
            var p = new DoodlePainter(-0.8f, -0.42f, 0.8f, 0.04f, 110) { Ink = 0.04f };
            foreach (float s in new[] { -1f, 1f })
                Body(p, Sd.Polygon(s * 0.48f, -0.08f, s * 0.48f, -0.38f, s * 0.48f + s * 0.06f, -0.38f, s * 0.48f + s * 0.26f, -0.08f), Rgba.Hex("8D93A6"), Rgba.Hex("6C7286"));
            Body(p, Sd.Box(0f, -0.045f, 0.75f, 0.05f, 0.02f), V72A_WoodLight, V72A_Wood);
            return new DoodleArt("v72_a_shelf", p);
        }

        /// <summary>Il barattolo dei biscotti (fondo in 0,0): vetro panciuto pieno di biscotti, coperchio rosso col pomello, cuoricino.</summary>
        public static DoodleArt V72A_CookieJar()
        {
            var p = new DoodlePainter(-0.32f, -0.04f, 0.32f, 0.72f, 180) { Ink = 0.03f };
            Sdf glass = Sd.Union(Sd.Box(0f, 0.25f, 0.26f, 0.24f, 0.1f), Sd.Box(0f, 0.5f, 0.19f, 0.05f, 0.02f));
            p.Fill(glass, Rgba.Hex("E6F4FB"));
            // i biscotti dentro
            foreach (var c in new[] { (-0.12f, 0.12f), (0.1f, 0.1f), (0f, 0.24f), (-0.13f, 0.33f), (0.13f, 0.32f) })
            {
                p.Shape(Sd.Circle(c.Item1, c.Item2, 0.095f), Rgba.Hex("D9A06A"), 0.018f);
                foreach (var ch in new[] { (-0.03f, 0.02f), (0.03f, -0.03f), (0.035f, 0.035f) })
                    p.Fill(Sd.Circle(c.Item1 + ch.Item1, c.Item2 + ch.Item2, 0.015f), Rgba.Hex("6B4129"));
            }
            // riflesso e contorno del vetro
            p.Fill(Sd.Capsule(-0.18f, 0.12f, -0.17f, 0.38f, 0.025f), new Rgba(1f, 1f, 1f, 0.75f));
            p.Fill(glass, new Rgba(0.85f, 0.95f, 1f, 0.18f));
            p.Shape(glass, Rgba.Clear, 0.035f);
            // il coperchio
            Body(p, Sd.Box(0f, 0.58f, 0.23f, 0.045f, 0.03f), Rgba.Hex("E8574F"), Rgba.Hex("B73A35"));
            Body(p, Sd.Circle(0f, 0.66f, 0.045f), Rgba.Hex("E8574F"), Rgba.Hex("B73A35"));
            // l'etichetta col cuore
            p.Shape(Sd.Box(0f, 0.2f, 0.1f, 0.06f, 0.02f), Rgba.Hex("FFF6E6"), 0.018f);
            p.Fill(Sd.Union(Sd.Circle(-0.018f, 0.215f, 0.02f), Sd.Circle(0.018f, 0.215f, 0.02f), Sd.Polygon(-0.037f, 0.207f, 0.037f, 0.207f, 0f, 0.17f)), Rgba.Hex("E8574F"));
            return new DoodleArt("v72_a_cookie_jar", p);
        }

        /// <summary>Il barattolo in frantumi sul pavimento (origine sul pavimento, al centro): cocci, coperchio di lato, biscotti sparsi.</summary>
        public static DoodleArt V72A_JarShards()
        {
            var p = new DoodlePainter(-0.85f, -0.12f, 0.85f, 0.3f, 140) { Ink = 0.025f };
            var rnd = new Random(72);
            for (int i = 0; i < 9; i++)
            {
                float x = (float)(rnd.NextDouble() * 1.4 - 0.7), y = (float)(rnd.NextDouble() * 0.1);
                float a = (float)(rnd.NextDouble() * 6.28), r = 0.05f + (float)rnd.NextDouble() * 0.06f;
                Sdf shard = Sd.Polygon(x + r * (float)Math.Cos(a), y + r * (float)Math.Sin(a), x + r * (float)Math.Cos(a + 2.2), y + r * 0.6f * (float)Math.Sin(a + 2.2),
                    x + r * 0.7f * (float)Math.Cos(a + 4.1), y + r * (float)Math.Sin(a + 4.1));
                p.Shape(shard, Rgba.Hex("D4ECF7"), 0.018f);
            }
            // il coperchio rovesciato
            Body(p, Sd.Rotate(Sd.Box(0.42f, 0.05f, 0.18f, 0.04f, 0.03f), 0.42f, 0.05f, 25f), Rgba.Hex("E8574F"), Rgba.Hex("B73A35"));
            // biscotti (uno spezzato)
            foreach (var c in new[] { (-0.5f, 0.07f), (-0.1f, 0.06f), (0.2f, 0.12f), (0.66f, 0.06f) })
            {
                p.Shape(Sd.Circle(c.Item1, c.Item2, 0.085f), Rgba.Hex("D9A06A"), 0.018f);
                p.Fill(Sd.Circle(c.Item1 - 0.02f, c.Item2 + 0.02f, 0.014f), Rgba.Hex("6B4129"));
                p.Fill(Sd.Circle(c.Item1 + 0.03f, c.Item2 - 0.02f, 0.014f), Rgba.Hex("6B4129"));
            }
            p.Shape(Sd.Subtract(Sd.Circle(-0.3f, 0.18f, 0.07f), Sd.Box(-0.22f, 0.18f, 0.04f, 0.1f)), Rgba.Hex("D9A06A"), 0.018f);
            foreach (var b in new[] { (-0.72f, 0f), (0.05f, -0.05f), (0.45f, -0.06f), (0.8f, 0.02f) })
                p.Fill(Sd.Circle(b.Item1, b.Item2, 0.02f), Rgba.Hex("C98B5A"));
            return new DoodleArt("v72_a_jar_shards", p);
        }

        /// <summary>La scaletta corta a due gradini (piedi in 0,0, alta 0.95).</summary>
        public static DoodleArt V72A_Stepladder()
        {
            var p = new DoodlePainter(-0.42f, -0.05f, 0.42f, 1.0f, 120) { Ink = 0.04f };
            Rgba al = Rgba.Hex("C9D3E0"), alShade = Rgba.Hex("9AA7B8");
            // la gamba dietro
            Body(p, Sd.Capsule(0.12f, 0.92f, 0.34f, 0.02f, 0.035f), alShade, Rgba.Hex("7D8999"));
            // i montanti davanti e i gradini
            Body(p, Sd.Capsule(-0.12f, 0.92f, -0.34f, 0.02f, 0.04f), al, alShade);
            Body(p, Sd.Capsule(0.06f, 0.92f, 0.08f, 0.02f, 0.04f), al, alShade);
            foreach (float y in new[] { 0.32f, 0.62f })
            {
                float t = y / 0.9f;
                float x0 = -0.34f + 0.22f * t, x1 = 0.08f - 0.02f * t;
                Body(p, Sd.Box((x0 + x1) * 0.5f, y, (x1 - x0) * 0.5f + 0.02f, 0.035f, 0.015f), Rgba.Hex("FFC93C"), Rgba.Hex("E39B1B"));
            }
            Body(p, Sd.Box(0f, 0.93f, 0.18f, 0.045f, 0.02f), Rgba.Hex("FFC93C"), Rgba.Hex("E39B1B"));
            return new DoodleArt("v72_a_stepladder", p);
        }

        /// <summary>Un libro chiuso visto dal dorso (centro 0,0; largo 0.74, alto 0.16): copertina colorata, fasce dorate, il titolo.</summary>
        public static DoodleArt V72A_Book(string name, string cover, string shade)
        {
            var p = new DoodlePainter(-0.4f, -0.11f, 0.4f, 0.11f, 160) { Ink = 0.03f };
            Sdf spine = Sd.Box(0f, 0f, 0.37f, 0.075f, 0.025f);
            Body(p, spine, Rgba.Hex(cover), Rgba.Hex(shade));
            foreach (float x in new[] { -0.27f, 0.27f }) p.Fill(Sd.Box(x, 0f, 0.022f, 0.075f), Rgba.Hex("FFD166"));
            p.Line(Rgba.Hex("FFF6E6", 0.85f), 0.018f, -0.15f, 0f, 0.12f, 0f);
            p.Shape(spine, Rgba.Clear);
            return new DoodleArt(name, p);
        }

        /// <summary>Il puntino rosso del laser (centro 0,0) con un alone.</summary>
        public static DoodleArt V72A_RedDot()
        {
            var p = new DoodlePainter(-0.2f, -0.2f, 0.2f, 0.2f, 200);
            p.FillFunc(Sd.Circle(0f, 0f, 0.19f), (x, y) =>
            {
                float d = (float)Math.Sqrt(x * x + y * y) / 0.19f;
                return new Rgba(1f, 0.15f, 0.2f, 0.45f * (1f - d) * (1f - d));
            });
            p.Fill(Sd.Circle(0f, 0f, 0.065f), Rgba.Hex("FF2A3A"), false);
            p.Fill(Sd.Circle(-0.015f, 0.015f, 0.025f), Rgba.Hex("FFC2C8"), false);
            return new DoodleArt("v72_a_red_dot", p);
        }

        /// <summary>Il raggio del laser: un filo rosso lungo 1 (da x 0 a 1) da stirare fra la punta e il puntino.</summary>
        public static DoodleArt V72A_Beam()
        {
            var p = new DoodlePainter(0f, -0.04f, 1f, 0.04f, 120);
            p.FillFunc(Sd.Box(0.5f, 0f, 0.5f, 0.035f), (x, y) => new Rgba(1f, 0.2f, 0.25f, 0.6f * (1f - Math.Abs(y) / 0.035f)));
            return new DoodleArt("v72_a_beam", p);
        }

        /// <summary>Il bianco degli occhi del gatto seduto (coordinate di roof_cat): copre gli occhi disegnati, le pupille si muovono sopra.</summary>
        public static DoodleArt V72A_CatEyes()
        {
            var p = new DoodlePainter(-0.22f, 0.71f, 0.22f, 0.95f, 140) { Ink = 0.02f };
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Ellipse(s * 0.11f, 0.83f, 0.07f, 0.085f), Rgba.White, 0.022f);
            return new DoodleArt("v72_a_cat_eyes", p);
        }

        /// <summary>Una pupilla del gatto (centro 0,0) col riflesso.</summary>
        public static DoodleArt V72A_CatPupil()
        {
            var p = new DoodlePainter(-0.05f, -0.05f, 0.05f, 0.05f, 280);
            p.Fill(Sd.Circle(0f, 0f, 0.037f), Ink, false);
            p.Fill(Sd.Circle(-0.012f, 0.014f, 0.012f), Rgba.White, false);
            return new DoodleArt("v72_a_cat_pupil", p);
        }

        /// <summary>La zampa che spinge (attacco in 0,0, punta a destra, lunga 0.32).</summary>
        public static DoodleArt V72A_Paw()
        {
            var p = new DoodlePainter(-0.06f, -0.08f, 0.4f, 0.08f, 200) { Ink = 0.025f };
            Body(p, Sd.Capsule(0f, 0f, 0.3f, 0.01f, 0.055f), CatOrange, CatShade);
            p.Fill(Sd.Ellipse(0.33f, 0.01f, 0.025f, 0.035f), CatPink);
            foreach (float y in new[] { -0.025f, 0.01f, 0.045f }) p.Line(Ink, 0.012f, 0.3f, y, 0.345f, y);
            return new DoodleArt("v72_a_paw", p);
        }
    }
}
