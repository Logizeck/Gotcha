using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.87 per il livello "The Fake Trophy" (roadmap n. 129): lo sfondo (una sala delle premiazioni
    /// spenta e grigia, pavimento di assi), la grande insegna luminosa SENZA scritta (la parola WIN è un testo a parte, un
    /// oggetto indipendente), le due serie di lampadine accese che si alternano, l'alone impolverato che resta sull'insegna
    /// quando WIN si stacca, il piedistallo con la targhetta vuota, la coppa grigia anonima e la stessa coppa d'oro, l'omino
    /// deluso (in tuta da atleta) e lo stesso a braccia alzate che solleva la coppa, il puntino dell'anello della pressione
    /// lunga e lo "clink" metallico. Coriandoli, bagliore e stelline sono quelli comuni. Nessuna scritta disegnata.
    /// Prefisso v87_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV87B() => new DoodleArt[]
        {
            V87B_Bg(), V87B_Sign(), V87B_Bulbs(0), V87B_Bulbs(1), V87B_WordGhost(), V87B_Pedestal(),
            V87B_Trophy(false), V87B_Trophy(true), V87B_GuySad(), V87B_GuyLift(), V87B_RingDot(), V87B_Clink(),
        };

        // misure condivise col builder (le stesse della logica FakeTrophyLogic)
        /// <summary>Pavimento, centro dell'insegna, piedistallo (centro x, piano y) e mezze misure dell'insegna.</summary>
        public const float V87BFloorY = -2.9f, V87BSignX = 0f, V87BSignY = 1.85f, V87BPedestalX = 1.4f, V87BPedestalTop = -1.7f;
        public const float V87BSignHalfW = 1.8f, V87BSignHalfH = 0.75f;
        /// <summary>Altezza della coppa (origine alla base) e centro della coppa vera e propria (dove si posa WIN).</summary>
        public const float V87BTrophyH = 1.35f, V87BBowlY = 0.92f;
        /// <summary>Dove l'omino a braccia alzate tiene la base della coppa (coordinate dei piedi).</summary>
        public const float V87BLiftHandsY = 2.22f;

        static readonly Rgba V87BWall = Rgba.Hex("A7A9B4"), V87BWallDark = Rgba.Hex("8E909C");
        static readonly Rgba V87BBoard = Rgba.Hex("9C8E80"), V87BBoardDark = Rgba.Hex("857869");
        static readonly Rgba V87BFrame = Rgba.Hex("E8384F"), V87BFrameDark = Rgba.Hex("B8243A"), V87BPanel = Rgba.Hex("2A1B4A");
        static readonly Rgba V87BBulbOff = Rgba.Hex("BDB394"), V87BBulbOn = Rgba.Hex("FFE45C");
        static readonly Rgba V87BGrey = Rgba.Hex("B9BCC4"), V87BGreyDark = Rgba.Hex("8F939D");
        static readonly Rgba V87BGold = Rgba.Hex("FFC83D"), V87BGoldDark = Rgba.Hex("D9921E"), V87BGoldLight = Rgba.Hex("FFF1A6");
        static readonly Rgba V87BSkin = Rgba.Hex("F2C49B"), V87BSkinShade = Rgba.Hex("D9A57B");
        static readonly Rgba V87BSuit = Rgba.Hex("3E7BD9"), V87BSuitShade = Rgba.Hex("2D5DA8"), V87BStripe = Rgba.Hex("F2F2F2");
        static readonly Rgba V87BHair = Rgba.Hex("6B4129"), V87BShoe = Rgba.Hex("E9E4DA");

        /// <summary>Le lampadine attorno all'insegna (coordinate dell'insegna): file sopra e sotto, tre per lato.</summary>
        public static List<(float x, float y)> V87BBulbSpots()
        {
            var l = new List<(float, float)>();
            for (int i = 0; i < 9; i++) l.Add((-1.6f + i * 0.4f, 0.6f));
            for (int i = 0; i < 3; i++) l.Add((1.62f, 0.3f - i * 0.3f));
            for (int i = 0; i < 9; i++) l.Add((1.6f - i * 0.4f, -0.6f));
            for (int i = 0; i < 3; i++) l.Add((-1.62f, -0.3f + i * 0.3f));
            return l;
        }

        /// <summary>
        /// Lo sfondo (coordinate del mondo): parete grigia spenta con righe verticali appena accennate e una mensola vuota
        /// polverosa, zoccolo, pavimento di assi dal piano y <see cref="V87BFloorY"/> in giù. Tutto smorto: l'unica cosa
        /// colorata della scena è l'insegna.
        /// </summary>
        public static DoodleArt V87B_Bg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            float fy = V87BFloorY;
            p.FillFunc(Sd.Box(0f, 1.2f, 3.3f, 4.1f + 0f), (x, y) =>
            {
                float stripe = (float)Math.Sin(x * 9f);
                Rgba c = stripe > 0.8f ? V87BWallDark : V87BWall;
                return Rgba.Lerp(c, Rgba.Hex("C3C5CE"), Math.Max(0f, Math.Min(1f, (y - fy) / 7f)) * 0.5f);
            });
            // mensola vuota con un'ombra di polvere dove c'era qualcosa
            p.Shape(Sd.Box(-2.1f, 0.25f, 0.75f, 0.05f, 0.01f), Rgba.Hex("9A8A7A"), 0.03f);
            p.Fill(Sd.Ellipse(-2.1f, 0.42f, 0.22f, 0.12f), Rgba.Hex("9EA0AB", 0.6f));
            p.InkLine(0.025f, -2.65f, 0.2f, -2.6f, 0.05f);
            p.InkLine(0.025f, -1.55f, 0.2f, -1.6f, 0.05f);
            // ragnatela nell'angolo in alto a destra
            for (int i = 0; i < 4; i++) p.Line(Rgba.Hex("FFFFFF", 0.55f), 0.012f, 3.2f, 3.05f, 3.2f - 0.6f + i * 0.13f, 3.05f - 0.08f - i * 0.12f);
            p.Line(Rgba.Hex("FFFFFF", 0.55f), 0.012f, 2.75f, 2.95f, 2.85f, 2.72f, 3.0f, 2.6f);
            // zoccolo e pavimento
            p.Shape(Sd.Box(0f, fy + 0.08f, 3.3f, 0.08f), Rgba.Hex("7D7F8A"), 0.03f);
            p.FillFunc(Sd.Box(0f, fy - 1.25f, 3.3f, 1.25f), (x, y) =>
            {
                int row = (int)Math.Floor((fy - y) / 0.42f);
                return row % 2 == 0 ? V87BBoard : V87BBoardDark;
            });
            for (int r = 1; r < 6; r++) p.Line(Rgba.Hex("6E6257", 0.8f), 0.02f, -3.3f, fy - r * 0.42f, 3.3f, fy - r * 0.42f);
            for (int r = 0; r < 6; r++)
                for (int k = 0; k < 4; k++)
                {
                    float jx = -2.6f + k * 1.6f + (r % 2) * 0.8f, jy = fy - r * 0.42f;
                    p.Line(Rgba.Hex("6E6257", 0.8f), 0.02f, jx, jy, jx, jy - 0.42f);
                }
            return new DoodleArt("v87_b_bg", p);
        }

        /// <summary>
        /// La grande insegna (coordinate dell'insegna, origine al centro): cornice rossa arrotondata con le lampadine spente,
        /// pannello viola scuro VUOTO (la parola WIN è un testo a parte), due cavi che salgono verso l'alto, due stelline ai
        /// lati. Mezze misure <see cref="V87BSignHalfW"/> x <see cref="V87BSignHalfH"/>.
        /// </summary>
        public static DoodleArt V87B_Sign()
        {
            float hw = V87BSignHalfW, hh = V87BSignHalfH;
            var p = new DoodlePainter(-hw - 0.15f, -hh - 0.12f, hw + 0.15f, 1.2f, 70) { Ink = 0.05f, Wobble = 0.006f };
            // i cavi
            foreach (float s in new[] { -1f, 1f }) p.InkLine(0.035f, s * 1.2f, hh - 0.05f, s * 1.3f, 1.2f);
            Body(p, Sd.Box(0f, 0f, hw, hh, 0.22f), V87BFrame, V87BFrameDark);
            p.Shape(Sd.Box(0f, 0f, hw - 0.3f, hh - 0.27f, 0.12f), V87BPanel, 0.04f);
            p.Fill(Sd.Box(0f, hh - 0.36f, hw - 0.42f, 0.04f, 0.04f), Rgba.Hex("3D2C66"));   // riflesso sul pannello
            foreach (var b in V87BBulbSpots())
            {
                p.Shape(Sd.Circle(b.x, b.y, 0.085f), V87BBulbOff, 0.025f);
                p.Fill(Sd.Circle(b.x - 0.025f, b.y + 0.025f, 0.025f), Rgba.Hex("FFFFFF", 0.6f));
            }
            return new DoodleArt("v87_b_sign", p);
        }

        /// <summary>Una delle due serie di lampadine accese (pari o dispari), con l'alone: il livello le alterna.</summary>
        public static DoodleArt V87B_Bulbs(int set)
        {
            float hw = V87BSignHalfW, hh = V87BSignHalfH;
            var p = new DoodlePainter(-hw - 0.15f, -hh - 0.12f, hw + 0.15f, hh + 0.12f, 70) { Ink = 0.025f, Wobble = 0f };
            List<(float x, float y)> spots = V87BBulbSpots();
            for (int i = set; i < spots.Count; i += 2)
                p.Fill(Sd.Circle(spots[i].x, spots[i].y, 0.17f), Rgba.Hex("FFE45C", 0.35f), false);
            for (int i = set; i < spots.Count; i += 2)
            {
                p.Shape(Sd.Circle(spots[i].x, spots[i].y, 0.085f), V87BBulbOn, 0.025f);
                p.Fill(Sd.Circle(spots[i].x - 0.025f, spots[i].y + 0.025f, 0.03f), Rgba.White);
            }
            return new DoodleArt(set == 0 ? "v87_b_bulbs_a" : "v87_b_bulbs_b", p);
        }

        /// <summary>L'alone che resta sul pannello quando WIN si stacca (coordinate dell'insegna): un rettangolo più chiaro tratteggiato.</summary>
        public static DoodleArt V87B_WordGhost()
        {
            var p = new DoodlePainter(-1.35f, -0.5f, 1.35f, 0.5f, 70) { Ink = 0.03f, Wobble = 0f };
            p.Fill(Sd.Box(0f, 0f, 1.2f, 0.36f, 0.08f), Rgba.Hex("4A3A78", 0.85f), false);
            for (int i = 0; i < 12; i++)
            {
                float x0 = -1.2f + i * 0.2f;
                p.Line(Rgba.Hex("8A7AC0", 0.9f), 0.025f, x0, 0.36f, x0 + 0.1f, 0.36f);
                p.Line(Rgba.Hex("8A7AC0", 0.9f), 0.025f, x0, -0.36f, x0 + 0.1f, -0.36f);
            }
            return new DoodleArt("v87_b_word_ghost", p);
        }

        /// <summary>Il piedistallo (coordinate del mondo): blocco grigio dal pavimento al piano y <see cref="V87BPedestalTop"/>, cornice, targhetta vuota.</summary>
        public static DoodleArt V87B_Pedestal()
        {
            float x = V87BPedestalX, top = V87BPedestalTop, fy = V87BFloorY;
            var p = new DoodlePainter(x - 0.8f, fy - 0.1f, x + 0.8f, top + 0.1f, 70) { Ink = 0.045f };
            float midY = (top + fy) * 0.5f, half = (top - fy) * 0.5f;
            Body(p, Sd.Box(x, midY, 0.55f, half - 0.06f, 0.02f), Rgba.Hex("C9C6C0"), Rgba.Hex("A9A59E"));
            p.Shape(Sd.Box(x, top - 0.05f, 0.65f, 0.06f, 0.02f), Rgba.Hex("D8D5CF"), 0.035f);
            p.Shape(Sd.Box(x, fy + 0.06f, 0.65f, 0.07f, 0.02f), Rgba.Hex("B5B1AA"), 0.035f);
            p.Shape(Sd.Box(x, midY + 0.05f, 0.32f, 0.14f, 0.03f), Rgba.Hex("A8A49C"), 0.03f);   // targhetta vuota
            p.Line(Rgba.Hex("8F8B84"), 0.02f, x - 0.2f, midY + 0.05f, x + 0.2f, midY + 0.05f);
            return new DoodleArt("v87_b_pedestal", p);
        }

        /// <summary>
        /// La coppa (origine al centro della base, alta <see cref="V87BTrophyH"/>): base a gradini, stelo, coppa con due manici.
        /// Grigia: piatta, opaca, un'ammaccatura e un po' di polvere. D'oro: sfumata, con riflesso, stella sulla base e scintille.
        /// </summary>
        public static DoodleArt V87B_Trophy(bool gold)
        {
            var p = new DoodlePainter(-0.85f, -0.05f, 0.85f, V87BTrophyH + 0.2f, 110) { Ink = 0.045f };
            Rgba c = gold ? V87BGold : V87BGrey, d = gold ? V87BGoldDark : V87BGreyDark;
            // manici
            foreach (float s in new[] { -1f, 1f })
            {
                Sdf handle = Sd.Subtract(Sd.Ellipse(s * 0.5f, 0.98f, 0.24f, 0.22f), Sd.Ellipse(s * 0.5f, 0.98f, 0.13f, 0.12f));
                p.Shape(Sd.Intersect(handle, Sd.Box(s * 0.62f, 0.98f, 0.2f, 0.3f)), c, 0.035f);
            }
            // base e stelo
            Body(p, Sd.Box(0f, 0.09f, 0.4f, 0.09f, 0.03f), d, Rgba.Lerp(d, Rgba.Hex("000000"), 0.15f));
            Body(p, Sd.Box(0f, 0.25f, 0.3f, 0.08f, 0.03f), c, d);
            Body(p, Sd.Polygon(-0.08f, 0.32f, 0.08f, 0.32f, 0.12f, 0.55f, -0.12f, 0.55f), c, d);
            // la coppa
            Sdf bowl = Sd.Union(Sd.Intersect(Sd.Ellipse(0f, 1.25f, 0.52f, 0.72f), Sd.Box(0f, 0.9f, 0.6f, 0.4f)), Sd.Box(0f, 1.27f, 0.52f, 0.05f, 0.02f));
            if (gold)
            {
                p.FillFunc(bowl, (x, y) => Rgba.Lerp(V87BGoldLight, V87BGold, Math.Max(0f, Math.Min(1f, (x + 0.25f) / 0.5f))));
                p.Fill(Sd.Crescent(bowl, -0.1f, 0.05f), V87BGoldDark);
                p.Shape(bowl, Rgba.Clear);
                p.Fill(Sd.Capsule(-0.3f, 1.15f, -0.22f, 0.75f, 0.05f), Rgba.Hex("FFFFFF", 0.8f));
                // stella sulla base
                p.Shape(V87B_Star(0f, 0.25f, 0.07f), V87BGoldLight, 0.015f);
                foreach (var sp in new[] { (-0.68f, 1.42f, 0.07f), (0.7f, 1.3f, 0.055f), (0.6f, 0.45f, 0.05f) })
                    p.Shape(V87B_Star(sp.Item1, sp.Item2, sp.Item3), Rgba.Hex("FFF7C2"), 0.015f);
            }
            else
            {
                Body(p, bowl, c, d);
                // ammaccatura e polvere
                p.InkLine(0.02f, 0.18f, 1.05f, 0.26f, 1.0f, 0.3f, 1.08f);
                p.Fill(Sd.Ellipse(-0.2f, 1.26f, 0.25f, 0.03f), Rgba.Hex("9EA1A9", 0.8f));
            }
            p.Fill(Sd.Ellipse(0f, 1.27f, 0.42f, 0.03f), gold ? V87BGoldDark : Rgba.Hex("7D818B"));   // l'interno vuoto
            return new DoodleArt(gold ? "v87_b_trophy_gold" : "v87_b_trophy_grey", p);
        }

        static Sdf V87B_Star(float cx, float cy, float r)
        {
            var pts = new float[20];
            for (int i = 0; i < 10; i++)
            {
                double a = Math.PI / 2 + i * Math.PI / 5;
                float rr = i % 2 == 0 ? r : r * 0.45f;
                pts[i * 2] = cx + (float)Math.Cos(a) * rr;
                pts[i * 2 + 1] = cy + (float)Math.Sin(a) * rr;
            }
            return Sd.Polygon(pts);
        }

        /// <summary>Gambe, scarpe e pantaloni della tuta comuni dell'omino (origine ai piedi).</summary>
        static void V87B_Legs(DoodlePainter p)
        {
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Box(s * 0.13f, 0.42f, 0.11f, 0.36f, 0.05f), V87BSuit, V87BSuitShade);
                p.Line(V87BStripe, 0.03f, s * 0.22f, 0.75f, s * 0.22f, 0.12f);
                p.Shape(Sd.Box(s * 0.15f, 0.05f, 0.15f, 0.07f, 0.05f), V87BShoe, 0.03f);
            }
        }

        static void V87B_Torso(DoodlePainter p, float slump)
        {
            Body(p, Sd.Box(0f, 1.05f - slump, 0.27f, 0.3f, 0.1f), V87BSuit, V87BSuitShade);
            p.InkLine(0.025f, 0f, 1.33f - slump, 0f, 0.8f - slump);   // la zip
            p.Shape(Sd.Box(0f, 1.35f - slump, 0.12f, 0.04f, 0.03f), V87BStripe, 0.02f);   // il colletto
        }

        static void V87B_Head(DoodlePainter p, float hx, float hy)
        {
            Body(p, Sd.Circle(hx, hy, 0.28f), V87BSkin, V87BSkinShade);
            p.Shape(Sd.Intersect(Sd.Circle(hx, hy + 0.04f, 0.3f), Sd.Box(hx, hy + 0.27f, 0.32f, 0.12f)), V87BHair, 0.03f);
            p.Shape(Sd.Box(hx, hy + 0.13f, 0.29f, 0.04f, 0.02f), Rgba.Hex("F2504A"), 0.025f);   // la fascia da atleta
        }

        /// <summary>L'omino deluso (origine ai piedi, alto circa 2): spalle cadenti, braccia penzoloni, sopracciglia tristi, bocca all'ingiù.</summary>
        public static DoodleArt V87B_GuySad()
        {
            var p = new DoodlePainter(-0.6f, -0.05f, 0.6f, 2.1f, 110) { Ink = 0.045f };
            const float slump = 0.06f;
            V87B_Legs(p);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Capsule(s * 0.27f, 1.24f - slump, s * 0.36f, 0.72f, 0.075f), V87BSuit);
                p.Shape(Sd.Circle(s * 0.37f, 0.66f, 0.08f), V87BSkin);
            }
            V87B_Torso(p, slump);
            float hx = 0.03f, hy = 1.66f - slump;
            V87B_Head(p, hx, hy);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Ellipse(hx + s * 0.1f, hy - 0.04f, 0.03f, 0.04f), DoodlePainter.DefaultInk);
                p.InkLine(0.022f, hx + s * 0.05f, hy + 0.06f, hx + s * 0.16f, hy + 0.02f);   // sopracciglia tristi
            }
            p.InkLine(0.022f, hx - 0.08f, hy - 0.17f, hx, hy - 0.13f, hx + 0.08f, hy - 0.17f);
            return new DoodleArt("v87_b_guy_sad", p);
        }

        /// <summary>L'omino felice a braccia alzate (origine ai piedi): le mani in alto in y <see cref="V87BLiftHandsY"/> (lì il livello mette la base della coppa), occhi a "^", sorrisone.</summary>
        public static DoodleArt V87B_GuyLift()
        {
            var p = new DoodlePainter(-0.6f, -0.05f, 0.6f, 2.4f, 110) { Ink = 0.045f };
            V87B_Legs(p);
            float hands = V87BLiftHandsY - 0.05f;
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Capsule(s * 0.27f, 1.26f, s * 0.33f, hands - 0.05f, 0.075f), V87BSuit);
                p.Shape(Sd.Circle(s * 0.32f, hands, 0.085f), V87BSkin);
            }
            V87B_Torso(p, 0f);
            float hx = 0f, hy = 1.68f;
            V87B_Head(p, hx, hy);
            foreach (float s in new[] { -1f, 1f })
            {
                p.InkLine(0.026f, hx + s * 0.15f, hy - 0.03f, hx + s * 0.1f, hy + 0.02f, hx + s * 0.05f, hy - 0.03f);
                p.Fill(Sd.Ellipse(hx + s * 0.17f, hy - 0.1f, 0.05f, 0.03f), Rgba.Hex("FF9AB0", 0.85f));
            }
            Sdf smile = Sd.Intersect(Sd.Ellipse(hx, hy - 0.1f, 0.12f, 0.1f), Sd.Box(hx, hy - 0.16f, 0.13f, 0.06f));
            p.Shape(smile, Rgba.Hex("5A1F3A"), 0.022f);
            return new DoodleArt("v87_b_guy_lift", p);
        }

        /// <summary>Un puntino dell'anello della pressione lunga (bianco, il livello lo tinge: spento o pieno).</summary>
        public static DoodleArt V87B_RingDot()
        {
            var p = new DoodlePainter(-0.08f, -0.08f, 0.08f, 0.08f, 160) { Ink = 0.02f, Wobble = 0f };
            p.Shape(Sd.Circle(0f, 0f, 0.055f), Rgba.White);
            return new DoodleArt("v87_b_ring_dot", p);
        }

        /// <summary>Lo "clink" metallico (origine al centro): tre trattini a raggiera e un piccolo lampo.</summary>
        public static DoodleArt V87B_Clink()
        {
            var p = new DoodlePainter(-0.4f, -0.4f, 0.4f, 0.4f, 120) { Ink = 0.03f, Wobble = 0f };
            foreach (float a in new[] { 60f, 100f, 140f, 20f })
            {
                double r = a * Math.PI / 180;
                float c = (float)Math.Cos(r), s = (float)Math.Sin(r);
                p.Line(Rgba.Hex("E8EEF5"), 0.06f, c * 0.14f, s * 0.14f, c * 0.34f, s * 0.34f);
                p.InkLine(0.02f, c * 0.14f, s * 0.14f, c * 0.34f, s * 0.34f);
            }
            p.Shape(V87B_Star(0f, 0f, 0.1f), Rgba.Hex("F4F7FA"), 0.02f);
            return new DoodleArt("v87_b_clink", p);
        }
    }
}
