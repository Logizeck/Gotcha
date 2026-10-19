using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli compositi della versione 0.40 (gruppo B).</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV40B() => new DoodleArt[]
        {
            V40B_Dot(),
            // fuga dalla cucina
            V40B_KitchenBackdrop(), V40B_Cupboard(false), V40B_Cupboard(true), V40B_KeyPlate(), V40B_Latch(false), V40B_Latch(true),
            V40B_Drawer(false), V40B_Drawer(true), V40B_String(), V40B_Fork(), V40B_MagnetString(), V40B_Tray(),
            // stazione spaziale
            V40B_StationBackdrop(), V40B_Tile(), V40B_Cable(false, false), V40B_Cable(true, false), V40B_Cable(false, true), V40B_Cable(true, true),
            V40B_Reactor(), V40B_Dish(), V40B_DishMount(), V40B_SignalLamp(), V40B_Satellite(),
            // mappa del tesoro
            V40B_BeachBackdrop(), V40B_MapPiece(0), V40B_MapPiece(1), V40B_MapPiece(2), V40B_MapPiece(3), V40B_MapOutline(),
            V40B_Symbol(0), V40B_Symbol(1), V40B_Symbol(2), V40B_Symbol(3), V40B_Compass(), V40B_IslandBackdrop(),
            V40B_PirateCat(), V40B_Boat(), V40B_XMark(), V40B_SandHole(), V40B_Dial(),
            // detective
            V40B_LineupBackdrop(), V40B_Dog(), V40B_Parrot(), V40B_ParrotWing(), V40B_Mud(), V40B_Crumbs(), V40B_CrimeScene(),
            V40B_Prints(), V40B_Magnifier(), V40B_Handcuffs(), V40B_Check(),
        };

        static readonly Rgba V40BInk = DoodlePainter.DefaultInk;
        static readonly Rgba V40BMetal = Rgba.Hex("B7BCC9"), V40BMetalShade = Rgba.Hex("8D93A6");

        /// <summary>Pallino bianco dei progressi (si tinge; centro 0,0, raggio 0.13).</summary>
        public static DoodleArt V40B_Dot()
        {
            var p = new DoodlePainter(-0.17f, -0.17f, 0.17f, 0.17f, 200) { Ink = 0.04f, Wobble = 0.004f };
            p.Shape(Sd.Circle(0, 0, 0.13f), Rgba.White);
            p.Fill(Sd.Circle(-0.04f, 0.04f, 0.035f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("v40b_dot", p);
        }

        // ---------- Fuga dalla cucina ----------

        /// <summary>
        /// Cucina in coordinate del livello: frigo a sinistra (x -2.9..-0.95, y -2.4..2.2), bancone a destra con il piano
        /// in y -0.95..-0.45 e il lavello con lo scarico in 1.6, -0.7; pavimento sotto y = -2.4.
        /// </summary>
        public static DoodleArt V40B_KitchenBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            // muro a piastrelle
            p.Fill(Sd.Box(0, 2.3f, 5f, 4.8f), Rgba.Hex("FFF4D6"), false);
            for (float y = -2.4f; y < 7f; y += 0.5f) p.Line(Rgba.Hex("E8D7AE"), 0.03f, -4.5f, y, 4.5f, y);
            for (int row = 0; row < 20; row++)
                for (float x = -4.5f + (row % 2) * 0.25f; x < 4.5f; x += 0.5f)
                    p.Line(Rgba.Hex("E8D7AE"), 0.03f, x, -2.4f + row * 0.5f, x, -1.9f + row * 0.5f);
            // pavimento a scacchi
            p.Fill(Sd.Box(0, -4.8f, 5f, 2.4f), Rgba.Hex("F2E3C6"), false);
            for (int i = -9; i < 10; i++)
                for (int j = 0; j < 9; j++)
                    if ((i + j) % 2 == 0) p.Fill(Sd.Box(i * 0.5f + 0.25f, -2.65f - j * 0.5f, 0.25f, 0.25f), Rgba.Hex("8FB6D9"), false);
            p.Shape(Sd.Box(0, -2.4f, 5f, 0.06f), Rgba.Hex("C98B5A"));
            // frigorifero
            Sdf fridge = Sd.Box(-1.95f, -0.1f, 0.98f, 2.32f, 0.18f);
            Body(p, fridge, Rgba.Hex("D8F1EC"), Rgba.Hex("A9D6CE"));
            p.InkLine(0.05f, -2.9f, 1.0f, -0.98f, 1.0f);
            p.Shape(Sd.Box(-1.12f, 1.55f, 0.06f, 0.35f, 0.05f), V40BMetal, 0.035f);
            p.Shape(Sd.Box(-1.12f, 0.3f, 0.06f, 0.55f, 0.05f), V40BMetal, 0.035f);
            p.Fill(Sd.Capsule(-2.6f, 1.9f, -2.6f, 1.3f, 0.05f), Rgba.White.WithAlpha(0.8f));
            p.Fill(Sd.Capsule(-2.6f, 0.7f, -2.6f, -1.6f, 0.05f), Rgba.White.WithAlpha(0.8f));
            // disegno del bambino attaccato al frigo
            p.Shape(Sd.Rotate(Sd.Box(-2.25f, -1.05f, 0.42f, 0.5f, 0.02f), -2.25f, -1.05f, 6f), Rgba.White, 0.03f);
            p.Fill(Sd.Circle(-2.2f, -0.82f, 0.12f), Rgba.Hex("FFC93C"));
            p.Line(Rgba.Hex("4E9A55"), 0.04f, -2.55f, -1.4f, -2.3f, -1.25f, -2.0f, -1.42f);
            p.Line(Rgba.Hex("E8433F"), 0.035f, -2.45f, -1.1f, -2.35f, -0.95f, -2.25f, -1.1f);
            p.Shape(Sd.Circle(-2.27f, -0.6f, 0.05f), Rgba.Hex("E8433F"), 0.02f);
            p.Shape(Sd.Box(-1.95f, -2.38f, 0.9f, 0.05f, 0.02f), Rgba.Hex("6B6677"));
            // mobile basso del bancone
            Sdf cab = Sd.Box(2.0f, -1.65f, 2.7f, 0.72f, 0.04f);
            Body(p, cab, Rgba.Hex("F2B880"), Rgba.Hex("D6935A"));
            p.Shape(Sd.Box(1.85f, -1.95f, 0.5f, 0.35f, 0.04f), Rgba.Hex("F7C99A"), 0.035f);
            p.Shape(Sd.Circle(1.45f, -1.95f, 0.05f), Gold, 0.025f);
            p.Shape(Sd.Box(3.0f, -1.65f, 0.4f, 0.62f, 0.04f), Rgba.Hex("F7C99A"), 0.035f);
            p.Fill(Sd.Box(2.0f, -2.42f, 2.7f, 0.04f), V40BInk.WithAlpha(0.6f));
            // piano di lavoro (in prospettiva) con il lavello
            Sdf top = Sd.Polygon(-0.75f, -0.95f, 4.6f, -0.95f, 4.6f, -0.45f, -0.6f, -0.45f);
            p.Shape(top, Rgba.Hex("EDEFF5"));
            p.Shape(Sd.Box(1.95f, -0.98f, 2.7f, 0.06f, 0.02f), Rgba.Hex("9AA0B2"), 0.035f);
            Sdf basin = Sd.Ellipse(1.6f, -0.7f, 0.7f, 0.2f);
            p.Shape(basin, Rgba.Hex("9AA0B2"), 0.045f);
            p.Fill(Sd.Intersect(basin, Sd.Ellipse(1.6f, -0.62f, 0.7f, 0.2f)), Rgba.Hex("7C8296"));
            p.Shape(Sd.Ellipse(1.6f, -0.72f, 0.13f, 0.06f), Rgba.Hex("2F2C38"), 0.03f);
            p.Line(Rgba.Hex("6B6677"), 0.015f, 1.5f, -0.72f, 1.7f, -0.72f);
            // rubinetto
            p.Shape(Sd.Box(1.6f, -0.33f, 0.12f, 0.1f, 0.03f), V40BMetal, 0.035f);
            p.Shape(Sd.Polyline(0.055f, 1.6f, -0.25f, 1.6f, 0.15f, 1.85f, 0.2f, 1.95f, 0.05f), V40BMetal, 0.035f);
            p.Shape(Sd.Circle(1.32f, -0.3f, 0.07f), Rgba.Hex("E8433F"), 0.03f);
            p.Shape(Sd.Circle(1.88f, -0.3f, 0.07f), Rgba.Hex("5EC4D9"), 0.03f);
            // presine e mensolina sopra il frigo
            p.InkLine(0.04f, -2.7f, 2.55f, -1.2f, 2.55f);
            p.Shape(Sd.Box(-2.4f, 2.75f, 0.13f, 0.2f, 0.03f), Rgba.Hex("E8433F"), 0.03f);
            p.Shape(Sd.Box(-2.0f, 2.8f, 0.11f, 0.25f, 0.03f), Rgba.Hex("FFC93C"), 0.03f);
            p.Shape(Sd.Box(-1.62f, 2.72f, 0.15f, 0.17f, 0.03f), Rgba.Hex("5EC4D9"), 0.03f);
            return new DoodleArt("v40b_kitchen_bg", p);
        }

        /// <summary>Pensile a due ante (centro 0,0; 2.4 x 1.8); aperto mostra il ripiano vuoto (ante spalancate ai lati).</summary>
        public static DoodleArt V40B_Cupboard(bool open)
        {
            var p = new DoodlePainter(open ? -1.9f : -1.3f, -1.0f, open ? 1.9f : 1.3f, 1.0f, 100) { Ink = 0.05f };
            Rgba wood = Rgba.Hex("F2B880"), shade = Rgba.Hex("D6935A"), panel = Rgba.Hex("F7C99A");
            if (!open)
            {
                Body(p, Sd.Box(0, 0, 1.2f, 0.9f, 0.05f), wood, shade);
                foreach (float s in new[] { -1f, 1f })
                    p.Shape(Sd.Box(s * 0.6f, 0, 0.45f, 0.72f, 0.05f), panel, 0.035f);
                p.InkLine(0.05f, 0, -0.88f, 0, 0.88f);
                foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Circle(s * 0.15f, 0.1f, 0.06f), Gold, 0.03f);
            }
            else
            {
                p.Shape(Sd.Box(0, 0, 1.2f, 0.9f, 0.05f), Rgba.Hex("8C5A3A"));
                p.Fill(Sd.Box(0, 0.75f, 1.12f, 0.1f), Rgba.Hex("6E4228"));
                p.Shape(Sd.Box(0, -0.25f, 1.14f, 0.05f, 0.01f), Rgba.Hex("D6935A"), 0.03f);
                p.Fill(Sd.Box(0, -0.6f, 1.12f, 0.3f), Rgba.Hex("7A4A2E"));
                p.Shape(Sd.Box(0, 0, 1.2f, 0.9f, 0.05f), Rgba.Clear);
                foreach (float s in new[] { -1f, 1f })
                {
                    Sdf door = Sd.Polygon(s * 1.2f, 0.9f, s * 1.85f, 0.75f, s * 1.85f, -0.75f, s * 1.2f, -0.9f);
                    Body(p, door, wood, shade);
                    p.Shape(Sd.Circle(s * 1.72f, 0.1f, 0.05f), Gold, 0.025f);
                }
            }
            return new DoodleArt(open ? "v40b_cupboard_open" : "v40b_cupboard", p);
        }

        /// <summary>Piastra d'ottone con la serratura (centro 0,0, 0.5 x 0.5).</summary>
        public static DoodleArt V40B_KeyPlate()
        {
            var p = new DoodlePainter(-0.3f, -0.3f, 0.3f, 0.3f, 160) { Ink = 0.04f };
            Body(p, Sd.Box(0, 0, 0.22f, 0.24f, 0.07f), Gold, GoldShade);
            p.Fill(Sd.Union(Sd.Circle(0, 0.05f, 0.06f), Sd.Polygon(-0.04f, 0.03f, 0.04f, 0.03f, 0.06f, -0.13f, -0.06f, -0.13f)), V40BInk);
            foreach (float y in new[] { 0.18f, -0.18f }) p.Fill(Sd.Circle(0, y, 0.02f), GoldShade);
            return new DoodleArt("v40b_keyplate", p);
        }

        /// <summary>Blocco "a prova di bambino": fermaglio di plastica fra i due pomelli (centro 0,0, largo 0.9).</summary>
        public static DoodleArt V40B_Latch(bool open)
        {
            var p = new DoodlePainter(-0.6f, -0.3f, 0.6f, 0.3f, 160) { Ink = 0.04f };
            Rgba plastic = Rgba.Hex("FFFFFF"), shade = Rgba.Hex("C9D3E6"), button = Rgba.Hex("FF6B8A");
            if (!open)
            {
                Body(p, Sd.Box(0, 0, 0.45f, 0.09f, 0.08f), plastic, shade);
                foreach (float s in new[] { -1f, 1f })
                {
                    p.Shape(Sd.Box(s * 0.4f, 0, 0.09f, 0.17f, 0.05f), plastic);
                    p.Shape(Sd.Box(s * 0.4f, 0, 0.045f, 0.1f, 0.03f), button, 0.025f);
                }
                p.Fill(Sd.Polygon(-0.12f, 0.02f, -0.04f, 0.02f, -0.08f, 0.06f), V40BInk.WithAlpha(0.4f));
                p.Fill(Sd.Polygon(0.12f, 0.02f, 0.04f, 0.02f, 0.08f, 0.06f), V40BInk.WithAlpha(0.4f));
            }
            else
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    Sdf half = Sd.Rotate(Sd.Box(s * 0.32f, 0, 0.2f, 0.09f, 0.08f), s * 0.45f, 0, s * -35f);
                    Body(p, half, plastic, shade);
                    p.Shape(Sd.Box(s * 0.45f, 0, 0.09f, 0.17f, 0.05f), plastic);
                    p.Shape(Sd.Box(s * 0.45f, 0, 0.045f, 0.1f, 0.03f), Rgba.Hex("7ED957"), 0.025f);
                }
            }
            return new DoodleArt(open ? "v40b_latch_open" : "v40b_latch", p);
        }

        /// <summary>Cassetto (frontale centrato in 0,0, 1.4 x 0.5; disegno da y -0.35 a 0.75); aperto si vede l'interno sopra il frontale.</summary>
        public static DoodleArt V40B_Drawer(bool open)
        {
            var p = new DoodlePainter(-0.8f, -0.35f, 0.8f, 0.75f, 110) { Ink = 0.045f };   // stessi bordi aperto/chiuso
            if (open)
            {
                Sdf box = Sd.Polygon(-0.7f, 0.15f, 0.7f, 0.15f, 0.6f, 0.68f, -0.6f, 0.68f);
                p.Shape(box, Rgba.Hex("8C5A3A"));
                p.Fill(Sd.Polygon(-0.6f, 0.62f, 0.6f, 0.62f, 0.55f, 0.66f, -0.55f, 0.66f), Rgba.Hex("6E4228"));
                p.Shape(Sd.Polygon(-0.7f, 0.15f, -0.6f, 0.68f, -0.55f, 0.68f, -0.63f, 0.15f), Rgba.Hex("D6935A"), 0.03f);
                p.Shape(Sd.Polygon(0.7f, 0.15f, 0.6f, 0.68f, 0.55f, 0.68f, 0.63f, 0.15f), Rgba.Hex("D6935A"), 0.03f);
            }
            float fy = open ? -0.07f : 0f;
            Body(p, Sd.Box(0, fy, 0.72f, 0.26f, 0.04f), Rgba.Hex("F7C99A"), Rgba.Hex("D6935A"));
            p.Shape(Sd.Box(0, fy, 0.22f, 0.05f, 0.04f), V40BMetal, 0.03f);
            return new DoodleArt(open ? "v40b_drawer_open" : "v40b_drawer", p);
        }

        /// <summary>Gomitolo di spago (centro 0,0, largo circa 0.6).</summary>
        public static DoodleArt V40B_String()
        {
            var p = new DoodlePainter(-0.42f, -0.3f, 0.42f, 0.3f, 160) { Ink = 0.035f };
            Rgba twine = Rgba.Hex("E3C38A"), shade = Rgba.Hex("BF9A5C");
            Body(p, Sd.Circle(-0.05f, 0, 0.24f), twine, shade);
            for (int i = 0; i < 5; i++)
            {
                float a = -0.18f + i * 0.09f;
                p.Line(shade, 0.018f, -0.25f, a, -0.05f, a + 0.12f, 0.15f, a);
            }
            p.Shape(Sd.Circle(-0.05f, 0, 0.24f), Rgba.Clear);
            p.Shape(Sd.Polyline(0.022f, 0.16f, -0.12f, 0.27f, -0.2f, 0.36f, -0.08f, 0.32f, 0.05f), twine, 0.03f);
            return new DoodleArt("v40b_string", p);
        }

        /// <summary>Forchetta in diagonale (centro 0,0, lunga circa 1).</summary>
        public static DoodleArt V40B_Fork()
        {
            var p = new DoodlePainter(-0.5f, -0.22f, 0.55f, 0.22f, 160) { Ink = 0.035f };
            Sdf fork = Sd.Union(Sd.Capsule(-0.45f, 0, 0.05f, 0, 0.045f), Sd.Box(0.14f, 0, 0.1f, 0.11f, 0.04f),
                Sd.Box(0.36f, 0.09f, 0.14f, 0.02f), Sd.Box(0.36f, 0.03f, 0.14f, 0.02f), Sd.Box(0.36f, -0.03f, 0.14f, 0.02f), Sd.Box(0.36f, -0.09f, 0.14f, 0.02f));
            Sdf rot = Sd.Rotate(fork, 0, 0, 12f);
            p.Shape(rot, V40BMetal);
            p.Fill(Sd.Rotate(Sd.Capsule(-0.4f, 0.015f, 0f, 0.015f, 0.012f), 0, 0, 12f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("v40b_fork", p);
        }

        /// <summary>Calamita legata allo spago (calamita in 0,-0.35, spago che sale fino a 0,0.45).</summary>
        public static DoodleArt V40B_MagnetString()
        {
            var p = new DoodlePainter(-0.4f, -0.68f, 0.4f, 0.58f, 160) { Ink = 0.035f };
            p.Shape(Sd.Polyline(0.02f, 0f, -0.12f, 0.08f, 0.05f, -0.06f, 0.22f, 0.05f, 0.38f, 0f, 0.5f), Rgba.Hex("E3C38A"), 0.025f);
            Sdf u = Sd.Subtract(Sd.Union(Sd.Circle(0, -0.35f, 0.27f), Sd.Box(0, -0.48f, 0.27f, 0.14f)), Sd.Union(Sd.Circle(0, -0.35f, 0.12f), Sd.Box(0, -0.5f, 0.12f, 0.16f)));
            Sdf uu = Sd.Rotate(u, 0, -0.35f, 180f);
            p.Shape(uu, Rgba.Hex("E8433F"));
            p.Fill(Sd.Intersect(uu, Sd.Box(0, -0.13f, 0.4f, 0.1f)), Rgba.Hex("E8E9EE"));
            p.Shape(uu, Rgba.Clear);
            p.Shape(Sd.Circle(0, -0.1f, 0.045f), Rgba.Hex("E3C38A"), 0.025f);
            return new DoodleArt("v40b_magnet_string", p);
        }

        /// <summary>Vassoio dell'inventario (centro 0,0; 4 posti larghi 0.95 in x -1.8, -0.6, 0.6, 1.8).</summary>
        public static DoodleArt V40B_Tray()
        {
            var p = new DoodlePainter(-2.65f, -0.62f, 2.65f, 0.62f, 80) { Ink = 0.05f };
            Body(p, Sd.Box(0, 0, 2.55f, 0.52f, 0.2f), Rgba.Hex("7A4A2E"), Rgba.Hex("5C3520"));
            for (int i = 0; i < 4; i++)
            {
                float x = -1.8f + i * 1.2f;
                p.Shape(Sd.Box(x, 0, 0.47f, 0.42f, 0.12f), Rgba.Hex("F7E7CE"), 0.035f);
                p.Fill(Sd.Box(x, 0.33f, 0.43f, 0.06f, 0.05f), Rgba.Hex("E2CCA8"));
            }
            return new DoodleArt("v40b_tray", p);
        }

        // ---------- Stazione spaziale ----------

        /// <summary>Interno della stazione (coordinate del livello): pareti a pannelli, oblò con stelle in alto, grata in basso.</summary>
        public static DoodleArt V40B_StationBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 4.6f, 7.1f), (x, y) => Rgba.Lerp(Rgba.Hex("2A3048"), Rgba.Hex("3D4766"), Clamp((y + 4f) / 9f)));
            for (float x = -4.2f; x < 4.5f; x += 1.4f)
                for (float y = -3.4f; y < 7f; y += 1.7f)
                {
                    p.Shape(Sd.Box(x, y, 0.64f, 0.79f, 0.08f), Rgba.Hex("47527A"), 0.04f);
                    foreach (float sx in new[] { -0.52f, 0.52f }) foreach (float sy in new[] { -0.67f, 0.67f })
                            p.Fill(Sd.Circle(x + sx, y + sy, 0.04f), Rgba.Hex("6B7AA8"));
                }
            // tubi
            p.Shape(Sd.Box(0, 3.15f, 5f, 0.09f), Rgba.Hex("6B7AA8"), 0.04f);
            p.Shape(Sd.Box(-2.75f, 0, 0.09f, 5f), Rgba.Hex("6B7AA8"), 0.04f);
            p.Shape(Sd.Box(2.75f, 0, 0.09f, 5f), Rgba.Hex("6B7AA8"), 0.04f);
            // oblò in alto
            Sdf glass = Sd.Circle(0, 4.6f, 1.1f);
            p.Shape(Sd.Circle(0, 4.6f, 1.3f), V40BMetal, 0.06f);
            p.Fill(glass, Rgba.Hex("0B0F24"));
            var rnd = new Random(40);
            for (int i = 0; i < 40; i++)
            {
                float a = (float)(rnd.NextDouble() * Math.PI * 2), r = (float)Math.Sqrt(rnd.NextDouble()) * 1.05f;
                p.Fill(Sd.Intersect(Sd.Circle((float)Math.Cos(a) * r, 4.6f + (float)Math.Sin(a) * r, 0.025f + (float)rnd.NextDouble() * 0.025f), glass), Rgba.White.WithAlpha(0.85f));
            }
            p.Fill(Sd.Intersect(Sd.Circle(0.6f, 4.0f, 0.55f), glass), Rgba.Hex("5EC4D9"));
            p.Fill(Sd.Intersect(Sd.Intersect(Sd.Circle(0.6f, 4.0f, 0.55f), Sd.Circle(0.85f, 4.25f, 0.35f)), glass), Rgba.Hex("7ED957"));
            p.Fill(Sd.Intersect(Sd.Crescent(Sd.Circle(0.6f, 4.0f, 0.55f), 0.12f, -0.1f), glass), Rgba.Hex("0B0F24").WithAlpha(0.5f));
            p.Shape(glass, Rgba.Clear, 0.05f);
            p.Fill(Sd.Capsule(-0.6f, 5.1f, -0.25f, 5.4f, 0.06f), Rgba.White.WithAlpha(0.35f));
            // grata del pavimento
            p.Fill(Sd.Box(0, -5.6f, 5f, 2.1f), Rgba.Hex("1D2133"), false);
            for (float x = -4.5f; x < 4.5f; x += 0.3f) p.Line(Rgba.Hex("3D4766"), 0.05f, x, -3.5f, x, -7f);
            p.Shape(Sd.Box(0, -3.5f, 5f, 0.08f), Rgba.Hex("6B7AA8"));
            return new DoodleArt("v40b_station_bg", p);
        }

        /// <summary>Piastrella del pannello dei cavi (centro 0,0, lato 1).</summary>
        public static DoodleArt V40B_Tile()
        {
            var p = new DoodlePainter(-0.53f, -0.53f, 0.53f, 0.53f, 120) { Ink = 0.04f };
            Body(p, Sd.Box(0, 0, 0.47f, 0.47f, 0.08f), Rgba.Hex("2F3650"), Rgba.Hex("232842"));
            foreach (float sx in new[] { -0.36f, 0.36f }) foreach (float sy in new[] { -0.36f, 0.36f })
                    p.Shape(Sd.Circle(sx, sy, 0.035f), V40BMetal, 0.02f);
            return new DoodleArt("v40b_tile", p);
        }

        /// <summary>
        /// Cavo dritto (da ovest a est) o ad angolo (da ovest a nord) sulla piastrella (centro 0,0, lato 1).
        /// Con energy = true solo il filo d'energia luminoso da sovrapporre al cavo.
        /// </summary>
        public static DoodleArt V40B_Cable(bool corner, bool energy)
        {
            var p = new DoodlePainter(-0.53f, -0.53f, 0.53f, 0.53f, 120) { Ink = 0.04f, Wobble = energy ? 0f : 0.006f };
            float[] path = corner
                ? new[] { -0.55f, 0f, -0.3f, 0f, -0.12f, -0.03f, -0.03f, -0.12f, 0f, -0.3f, 0f, -0.55f }
                : new[] { -0.55f, 0f, 0.55f, 0f };
            if (corner) path = new[] { -0.55f, 0f, -0.3f, 0f, -0.12f, 0.03f, -0.03f, 0.12f, 0f, 0.3f, 0f, 0.55f };
            Sdf clip = Sd.Box(0, 0, 0.5f, 0.5f);
            if (!energy)
            {
                p.Shape(Sd.Intersect(Sd.Polyline(0.13f, path), clip), Rgba.Hex("4A4655"));
                p.Fill(Sd.Intersect(Sd.Polyline(0.05f, path), clip), Rgba.Hex("1A1C2A"));
                // connettori ai bordi
                p.Shape(Sd.Box(-0.44f, 0, 0.05f, 0.16f, 0.02f), V40BMetal, 0.025f);
                if (corner) p.Shape(Sd.Box(0, 0.44f, 0.16f, 0.05f, 0.02f), V40BMetal, 0.025f);
                else p.Shape(Sd.Box(0.44f, 0, 0.05f, 0.16f, 0.02f), V40BMetal, 0.025f);
            }
            else
            {
                p.FillFunc(Sd.Intersect(Sd.Polyline(0.16f, path), clip), (x, y) => Rgba.Hex("3CE6FF", 0.35f));
                p.Fill(Sd.Intersect(Sd.Polyline(0.06f, path), clip), Rgba.Hex("9EF3FF"), false);
                p.Fill(Sd.Intersect(Sd.Polyline(0.025f, path), clip), Rgba.White, false);
            }
            return new DoodleArt("v40b_" + (energy ? "energy_" : "cable_") + (corner ? "corner" : "straight"), p);
        }

        /// <summary>Reattore cilindrico con nucleo luminoso (centro 0,0; uscita del cavo a destra in 0.45, 0).</summary>
        public static DoodleArt V40B_Reactor()
        {
            var p = new DoodlePainter(-0.5f, -0.75f, 0.55f, 0.75f, 120) { Ink = 0.045f };
            Body(p, Sd.Box(0, 0, 0.36f, 0.65f, 0.12f), Rgba.Hex("8D93A6"), Rgba.Hex("666B7D"));
            p.Shape(Sd.Box(0, 0, 0.22f, 0.45f, 0.1f), Rgba.Hex("1A1C2A"));
            p.FillFunc(Sd.Box(0, 0, 0.18f, 0.41f, 0.08f), (x, y) => Rgba.Lerp(Rgba.Hex("7ED957"), Rgba.Hex("E9FFB0"), (float)Math.Exp(-x * x * 40f)));
            foreach (float y in new[] { -0.2f, 0.05f, 0.3f }) p.Line(Rgba.Hex("3E9A35", 0.6f), 0.02f, -0.18f, y, 0.18f, y);
            p.Shape(Sd.Box(0.42f, 0, 0.08f, 0.13f, 0.03f), V40BMetal, 0.03f);
            // simbolo di pericolo
            p.Shape(Sd.Polygon(-0.12f, 0.53f, 0.12f, 0.53f, 0f, 0.7f), Rgba.Hex("FFC93C"), 0.02f);
            return new DoodleArt("v40b_reactor", p);
        }

        /// <summary>Parabola vista di fronte, perno in 0,0 (la coppa sopra, fino a y 1.5).</summary>
        public static DoodleArt V40B_Dish()
        {
            var p = new DoodlePainter(-1.15f, -0.2f, 1.15f, 1.7f, 100) { Ink = 0.05f };
            p.Shape(Sd.Box(0, 0.25f, 0.1f, 0.3f, 0.03f), V40BMetalShade);
            Sdf cup = Sd.Intersect(Sd.Circle(0, 1.75f, 1.2f), Sd.Box(0, 0.95f, 1.2f, 0.5f));
            Body(p, cup, Rgba.Hex("EDEFF5"), Rgba.Hex("B7BCC9"));
            p.Shape(Sd.Ellipse(0, 1.43f, 1.05f, 0.14f), Rgba.Hex("C9CEDB"), 0.04f);
            p.InkLine(0.04f, -0.7f, 1.4f, 0f, 2.3f);
            p.InkLine(0.04f, 0.7f, 1.4f, 0f, 2.3f);
            p.Shape(Sd.Circle(0, 0.62f, 0.06f), Rgba.Hex("E8433F"), 0.03f);
            return new DoodleArt("v40b_dish", p);
        }

        /// <summary>Base della parabola: piedistallo con il perno in 0,0.</summary>
        public static DoodleArt V40B_DishMount()
        {
            var p = new DoodlePainter(-0.75f, -1.25f, 0.75f, 0.2f, 100) { Ink = 0.05f };
            Body(p, Sd.Polygon(-0.25f, 0f, 0.25f, 0f, 0.4f, -1.0f, -0.4f, -1.0f), V40BMetal, V40BMetalShade);
            Body(p, Sd.Box(0, -1.07f, 0.65f, 0.1f, 0.04f), Rgba.Hex("6B7AA8"), Rgba.Hex("47527A"));
            p.Shape(Sd.Circle(0, 0, 0.14f), Rgba.Hex("4A4655"));
            p.Fill(Sd.Circle(0, 0, 0.05f), V40BMetal);
            return new DoodleArt("v40b_dish_mount", p);
        }

        /// <summary>Lampada di segnalazione spenta (vetro in 0,0, raggio 0.4; supporto sotto).</summary>
        public static DoodleArt V40B_SignalLamp()
        {
            var p = new DoodlePainter(-0.6f, -0.75f, 0.6f, 0.55f, 120) { Ink = 0.045f };
            Body(p, Sd.Box(0, -0.55f, 0.5f, 0.15f, 0.05f), Rgba.Hex("6B7AA8"), Rgba.Hex("47527A"));
            p.Shape(Sd.Circle(0, 0, 0.44f), V40BMetalShade);
            p.Shape(Sd.Circle(0, 0, 0.36f), Rgba.Hex("6B5A2E"));
            p.Fill(Sd.Capsule(-0.18f, 0.12f, -0.06f, 0.22f, 0.05f), Rgba.White.WithAlpha(0.45f));
            foreach (float a in new[] { 0f, 90f, 180f, 270f })
            {
                double r = a * Math.PI / 180;
                p.Shape(Sd.Circle((float)Math.Cos(r) * 0.4f, (float)Math.Sin(r) * 0.4f, 0.035f), V40BMetal, 0.02f);
            }
            return new DoodleArt("v40b_lamp_signal", p);
        }

        /// <summary>Satellitino che passa nell'oblò (centro 0,0, largo 0.9).</summary>
        public static DoodleArt V40B_Satellite()
        {
            var p = new DoodlePainter(-0.5f, -0.25f, 0.5f, 0.25f, 140) { Ink = 0.03f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Box(s * 0.3f, 0, 0.16f, 0.1f, 0.01f), Rgba.Hex("3D6FD9"), 0.025f);
                p.Line(Rgba.Hex("9CC3EA"), 0.01f, s * 0.3f, -0.09f, s * 0.3f, 0.09f);
            }
            p.Shape(Sd.Box(0, 0, 0.11f, 0.13f, 0.03f), Gold, 0.03f);
            p.InkLine(0.015f, 0, 0.13f, 0.05f, 0.22f);
            return new DoodleArt("v40b_satellite", p);
        }

        // ---------- Mappa del tesoro ----------

        /// <summary>Spiaggia con un baule di legno come tavolo (coordinate del livello).</summary>
        public static DoodleArt V40B_BeachBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 4.5f, 4.6f, 2.6f), (x, y) => Rgba.Lerp(Rgba.Hex("FFE6B8"), Rgba.Hex("8CCFF2"), Clamp((y - 2.4f) / 2.5f)));
            p.FillFunc(Sd.Box(0, 2.4f, 4.6f, 0.6f), (x, y) => Rgba.Lerp(Rgba.Hex("2E9DC2"), Rgba.Hex("5EC4D9"), Clamp((y - 1.8f) / 1.2f)));
            for (int i = 0; i < 9; i++) p.Line(Rgba.White.WithAlpha(0.6f), 0.04f, -4.2f + i * 1.0f, 2.2f + (i % 3) * 0.2f, -3.9f + i * 1.0f, 2.25f + (i % 3) * 0.2f);
            Sdf sand = Sd.Box(0, -2.6f, 4.6f, 4.6f);
            p.FillFunc(sand, (x, y) => Rgba.Lerp(Rgba.Hex("F2C982"), Rgba.Hex("F7DCA4"), Noise.Value(x * 3f, y * 3f) * 0.5f + Clamp((y + 4f) / 6f) * 0.5f));
            p.Line(Rgba.White.WithAlpha(0.85f), 0.08f, -4.6f, 1.85f, -2.5f, 1.75f, -0.5f, 1.9f, 1.5f, 1.75f, 4.6f, 1.85f);
            var rnd = new Random(41);
            for (int i = 0; i < 70; i++)
                p.Fill(Sd.Circle(-4.4f + (float)rnd.NextDouble() * 8.8f, -6.9f + (float)rnd.NextDouble() * 8.5f, 0.025f), Rgba.Hex("D9A866").WithAlpha(0.7f));
            // conchiglie e stella marina
            p.Shape(Sd.Ellipse(-2.3f, -4.1f, 0.18f, 0.13f), Rgba.Hex("FFB3C1"), 0.03f);
            p.Shape(Sd.Polygon(2.3f, -1.5f, 2.38f, -1.7f, 2.58f, -1.72f, 2.42f, -1.85f, 2.48f, -2.05f, 2.3f, -1.93f, 2.12f, -2.05f, 2.18f, -1.85f, 2.02f, -1.72f, 2.22f, -1.7f), Rgba.Hex("FF8A3D"), 0.03f);
            return new DoodleArt("v40b_beach_bg", p);
        }

        // margini strappati della mappa: cuciture fra i quattro pezzi
        static float V40B_SeamV(float y) => 0.07f * (float)Math.Sin(y * 7f) + 0.04f * (float)Math.Sin(y * 19f + 1f);
        static float V40B_SeamH(float x) => 0.07f * (float)Math.Sin(x * 6f + 0.5f) + 0.04f * (float)Math.Sin(x * 17f);

        /// <summary>Zona del pezzo q della mappa: 0 alto-sinistra, 1 alto-destra, 2 basso-sinistra, 3 basso-destra.</summary>
        static Sdf V40B_MapZone(int q)
        {
            bool left = q == 0 || q == 2, upper = q < 2;
            return (x, y) => Math.Max(left ? x - V40B_SeamV(y) : V40B_SeamV(y) - x, upper ? V40B_SeamH(x) - y : y - V40B_SeamH(x));
        }

        /// <summary>Disegna la mappa (quadrato di lato 3.2 centrato in 0,0) ritagliata dentro clip.</summary>
        static void V40B_PaintMap(DoodlePainter p, Sdf clip)
        {
            Sdf paper = Sd.Intersect(Sd.Box(0, 0, 1.55f, 1.55f, 0.06f), clip);
            Rgba brown = Rgba.Hex("8A5A3B");
            p.FillFunc(paper, (x, y) => Rgba.Lerp(Rgba.Hex("FBE7B5"), Rgba.Hex("E9C98A"), Noise.Value(x * 2.2f, y * 2.2f) * 0.6f + Clamp((Math.Abs(x) + Math.Abs(y) - 1.6f) / 1.4f) * 0.5f));
            // macchie
            p.Fill(Sd.Intersect(Sd.Circle(0.9f, -0.5f, 0.22f), paper), Rgba.Hex("D9B070", 0.5f));
            p.Fill(Sd.Intersect(Sd.Circle(-0.7f, 0.95f, 0.14f), paper), Rgba.Hex("D9B070", 0.5f));
            // onde del mare
            for (int i = 0; i < 6; i++)
            {
                float x = -1.1f + (i % 3) * 1.0f, y = i < 3 ? 1.1f : -1.15f;
                p.Fill(Sd.Intersect(Sd.Polyline(0.018f, x, y, x + 0.12f, y + 0.07f, x + 0.24f, y, x + 0.36f, y + 0.07f), paper), brown.WithAlpha(0.6f));
            }
            // isola
            Sdf island = Sd.Union(Sd.Ellipse(0, 0.15f, 0.95f, 0.62f), Sd.Ellipse(0.55f, -0.2f, 0.45f, 0.35f), Sd.Ellipse(-0.6f, 0.4f, 0.4f, 0.3f));
            p.Fill(Sd.Intersect(Sd.Grow(island, 0.08f), paper), Rgba.Hex("BFE3E6"));
            p.Fill(Sd.Intersect(island, paper), Rgba.Hex("F2C982"));
            p.Fill(Sd.Intersect(Sd.Ellipse(-0.2f, 0.3f, 0.45f, 0.28f), paper), Rgba.Hex("9CCB6A"));
            p.Fill(Sd.Intersect(Sd.Subtract(Sd.Grow(island, 0.02f), Sd.Grow(island, -0.02f)), paper), brown);
            // palme e montagnola
            foreach (float[] t in new[] { new[] { -0.45f, 0.35f }, new[] { 0.1f, 0.5f }, new[] { 0.6f, 0.0f } })
            {
                p.Fill(Sd.Intersect(Sd.Polyline(0.02f, t[0], t[1] - 0.15f, t[0] + 0.02f, t[1] + 0.05f), paper), brown);
                p.Fill(Sd.Intersect(Sd.Union(Sd.Ellipse(t[0] - 0.08f, t[1] + 0.07f, 0.1f, 0.035f), Sd.Ellipse(t[0] + 0.1f, t[1] + 0.07f, 0.1f, 0.035f)), paper), Rgba.Hex("4E9A55"));
            }
            // sentiero tratteggiato
            for (int i = 0; i < 7; i++)
            {
                float x = -0.7f + i * 0.2f, y = -0.25f + (float)Math.Sin(i * 0.9f) * 0.12f;
                p.Fill(Sd.Intersect(Sd.Capsule(x, y, x + 0.08f, y + 0.02f, 0.02f), paper), Rgba.Hex("C0392B"));
            }
            p.Fill(Sd.Intersect(Sd.Polyline(0.035f, 0.68f, -0.1f, 0.85f, -0.27f), paper), Rgba.Hex("C0392B"));
            p.Fill(Sd.Intersect(Sd.Polyline(0.035f, 0.85f, -0.1f, 0.68f, -0.27f), paper), Rgba.Hex("C0392B"));
            // spazio per le istruzioni (in basso) e cornice
            p.Fill(Sd.Intersect(Sd.Box(0, -0.85f, 1.25f, 0.2f, 0.05f), paper), Rgba.Hex("FFF6DA", 0.75f));
            p.Fill(Sd.Intersect(Sd.Subtract(Sd.Box(0, 0, 1.42f, 1.42f, 0.04f), Sd.Box(0, 0, 1.38f, 1.38f, 0.04f)), paper), brown.WithAlpha(0.7f));
            p.Shape(paper, Rgba.Clear);
        }

        /// <summary>Pezzo q della mappa strappata, nelle coordinate della mappa (il centro del disegno è il centro del pezzo).</summary>
        public static DoodleArt V40B_MapPiece(int q)
        {
            bool left = q == 0 || q == 2, upper = q < 2;
            float x0 = left ? -1.68f : -0.16f, x1 = left ? 0.16f : 1.68f, y0 = upper ? -0.16f : -1.68f, y1 = upper ? 1.68f : 0.16f;
            var p = new DoodlePainter(x0, y0, x1, y1, 100) { Ink = 0.045f, Seed = 3 };
            V40B_PaintMap(p, V40B_MapZone(q));
            return new DoodleArt("v40b_map_piece" + q, p);
        }

        /// <summary>Sagoma tratteggiata dove ricomporre la mappa (centro 0,0, lato 3.2).</summary>
        public static DoodleArt V40B_MapOutline()
        {
            var p = new DoodlePainter(-1.7f, -1.7f, 1.7f, 1.7f, 80) { Wobble = 0f };
            p.Fill(Sd.Box(0, 0, 1.55f, 1.55f, 0.06f), Rgba.Hex("2B2133", 0.18f));
            for (float t = -1.5f; t < 1.5f; t += 0.3f)
                foreach (float s in new[] { -1.55f, 1.55f })
                {
                    p.Line(Rgba.White.WithAlpha(0.8f), 0.04f, t, s, t + 0.16f, s);
                    p.Line(Rgba.White.WithAlpha(0.8f), 0.04f, s, t, s, t + 0.16f);
                }
            return new DoodleArt("v40b_map_outline", p);
        }

        /// <summary>Simbolo pirata (centro 0,0, circa 0.7): 0 teschio, 1 pappagallo, 2 ancora, 3 palma.</summary>
        public static DoodleArt V40B_Symbol(int kind)
        {
            var p = new DoodlePainter(-0.4f, -0.4f, 0.4f, 0.4f, 160) { Ink = 0.035f };
            string[] names = { "skull", "parrot", "anchor", "palm" };
            switch (kind)
            {
                case 0:
                    {
                        Sdf skull = Sd.Union(Sd.Circle(0, 0.06f, 0.25f), Sd.Box(0, -0.2f, 0.13f, 0.1f, 0.04f));
                        Body(p, skull, Rgba.White, Rgba.Hex("D9DCE6"));
                        foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Ellipse(s * 0.1f, 0.05f, 0.07f, 0.08f), V40BInk);
                        p.Fill(Sd.Polygon(-0.03f, -0.07f, 0.03f, -0.07f, 0f, -0.02f), V40BInk);
                        foreach (float x in new[] { -0.06f, 0f, 0.06f }) p.InkLine(0.015f, x, -0.15f, x, -0.27f);
                        break;
                    }
                case 1:
                    {
                        Body(p, Sd.Union(Sd.Circle(-0.03f, 0.08f, 0.22f), Sd.Ellipse(0.02f, -0.15f, 0.17f, 0.22f)), Rgba.Hex("4CC35A"), Rgba.Hex("2E9A3F"));
                        p.Fill(Sd.Ellipse(-0.06f, 0.12f, 0.1f, 0.09f), Rgba.White);
                        p.Fill(Sd.Circle(-0.05f, 0.12f, 0.04f), V40BInk);
                        Sdf beak = Sd.Polygon(0.13f, 0.17f, 0.32f, 0.08f, 0.26f, -0.08f, 0.15f, 0.0f);
                        p.Shape(beak, Rgba.Hex("FFC93C"), 0.03f);
                        p.Shape(Sd.Polygon(-0.05f, -0.3f, 0.05f, -0.3f, 0.12f, -0.42f, -0.1f, -0.42f), Rgba.Hex("E8433F"), 0.03f);
                        p.Fill(Sd.Ellipse(-0.08f, 0.31f, 0.06f, 0.04f), Rgba.Hex("E8433F"));
                        break;
                    }
                case 2:
                    {
                        Rgba m = Rgba.Hex("6B7AA8");
                        Sdf anchor = Sd.Union(Sd.Box(0, 0, 0.04f, 0.27f), Sd.Box(0, 0.16f, 0.14f, 0.035f),
                            Sd.Subtract(Sd.Circle(0, 0.29f, 0.08f), Sd.Circle(0, 0.29f, 0.035f)),
                            Sd.Subtract(Sd.Circle(0, -0.08f, 0.26f), Sd.Union(Sd.Circle(0, -0.08f, 0.19f), Sd.Box(0, 0.12f, 0.3f, 0.2f))),
                            Sd.Polygon(-0.33f, -0.03f, -0.2f, -0.02f, -0.27f, -0.13f), Sd.Polygon(0.33f, -0.03f, 0.2f, -0.02f, 0.27f, -0.13f));
                        p.Shape(anchor, m);
                        break;
                    }
                default:
                    {
                        p.Shape(Sd.Polyline(0.04f, -0.02f, -0.36f, 0.0f, -0.1f, 0.05f, 0.15f), Rgba.Hex("B8682E"), 0.03f);
                        foreach (float a in new[] { 20f, 70f, 115f, 160f, 200f })
                        {
                            double r = a * Math.PI / 180;
                            float ex = 0.05f + (float)Math.Cos(r) * 0.3f, ey = 0.15f + (float)Math.Sin(r) * 0.18f;
                            p.Shape(Sd.Capsule(0.05f, 0.17f, ex, ey, 0.05f), Rgba.Hex("4CC35A"), 0.03f);
                        }
                        p.Shape(Sd.Circle(0.0f, 0.1f, 0.05f), Rgba.Hex("8A5A3B"), 0.02f);
                        break;
                    }
            }
            return new DoodleArt("v40b_sym_" + names[kind], p);
        }

        /// <summary>Rosa dei venti con la N in alto (centro 0,0, raggio 0.5).</summary>
        public static DoodleArt V40B_Compass()
        {
            var p = new DoodlePainter(-0.55f, -0.55f, 0.55f, 0.75f, 140) { Ink = 0.035f };
            p.Shape(Sd.Circle(0, 0, 0.42f), Rgba.Hex("FFF6DA"));
            p.Fill(Sd.Subtract(Sd.Circle(0, 0, 0.36f), Sd.Circle(0, 0, 0.33f)), Rgba.Hex("8A5A3B"));
            p.Shape(Sd.Polygon(0, 0.06f, 0.33f, 0, 0, -0.06f, -0.33f, 0), Rgba.Hex("B7BCC9"), 0.025f);
            p.Shape(Sd.Polygon(-0.08f, 0, 0, 0.4f, 0.08f, 0), Rgba.Hex("E8433F"), 0.03f);
            p.Shape(Sd.Polygon(-0.08f, 0, 0, -0.4f, 0.08f, 0), Rgba.White, 0.03f);
            p.Shape(Sd.Circle(0, 0, 0.05f), Gold, 0.02f);
            // la lettera N sopra la freccia
            p.Line(Rgba.Hex("C0392B"), 0.06f, -0.09f, 0.48f, -0.09f, 0.68f, 0.09f, 0.48f, 0.09f, 0.68f);
            return new DoodleArt("v40b_compass", p);
        }

        /// <summary>
        /// Isola vista dall'alto (coordinate del livello): mare tutt'intorno e la sabbia sulla griglia 5 x 6
        /// di caselle da 0.85 centrata in 0,-0.9 (x -2.125..2.125, y -3.45..1.65), con i puntini della griglia.
        /// </summary>
        public static DoodleArt V40B_IslandBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 4.6f, 7.1f), (x, y) => Rgba.Lerp(Rgba.Hex("2E9DC2"), Rgba.Hex("5EC4D9"), Noise.Value(x * 0.8f, y * 0.8f)));
            var rnd = new Random(42);
            for (int i = 0; i < 40; i++)
            {
                float x = -4.3f + (float)rnd.NextDouble() * 8.6f, y = -6.8f + (float)rnd.NextDouble() * 13.6f;
                p.Line(Rgba.White.WithAlpha(0.45f), 0.04f, x, y, x + 0.15f, y + 0.07f, x + 0.3f, y);
            }
            Sdf island = Sd.Union(Sd.Box(0, -0.9f, 2.35f, 2.8f, 0.6f), Sd.Ellipse(-1.9f, 1.4f, 0.8f, 0.6f), Sd.Ellipse(2.1f, -3.4f, 0.7f, 0.6f));
            p.Fill(Sd.Grow(island, 0.25f), Rgba.Hex("9EE3EE"));
            p.FillFunc(island, (x, y) => Rgba.Lerp(Rgba.Hex("F2C982"), Rgba.Hex("F7DCA4"), Noise.Value(x * 3f, y * 3f)));
            p.Shape(island, Rgba.Clear);
            for (int i = 0; i < 80; i++)
            {
                float x = -2.2f + (float)rnd.NextDouble() * 4.4f, y = -3.6f + (float)rnd.NextDouble() * 5.4f;
                p.Fill(Sd.Circle(x, y, 0.022f), Rgba.Hex("D9A866").WithAlpha(0.6f));
            }
            // puntini agli incroci della griglia
            for (int c = 0; c <= 5; c++)
                for (int r = 0; r <= 6; r++)
                    p.Fill(Sd.Circle(-2.125f + c * 0.85f, -3.45f + r * 0.85f, 0.035f), Rgba.Hex("B8864E", 0.7f));
            // palme ai bordi (fuori dalle caselle)
            foreach (float[] t in new[] { new[] { -2.35f, 1.75f }, new[] { 2.3f, -3.6f }, new[] { -2.4f, -3.55f } })
            {
                p.Shape(Sd.Circle(t[0], t[1], 0.12f), Rgba.Hex("B8682E"), 0.03f);
                foreach (float a in new[] { 0f, 72f, 144f, 216f, 288f })
                {
                    double r = a * Math.PI / 180;
                    p.Shape(Sd.Capsule(t[0], t[1], t[0] + (float)Math.Cos(r) * 0.42f, t[1] + (float)Math.Sin(r) * 0.42f, 0.09f), Rgba.Hex("4CC35A"), 0.03f);
                }
                p.Shape(Sd.Circle(t[0], t[1], 0.08f), Rgba.Hex("8A5A3B"), 0.02f);
            }
            return new DoodleArt("v40b_island_bg", p);
        }

        /// <summary>Gatto pirata seduto (come roof_cat: piedi in y 0, testa in 0,0.8) con cappello e benda.</summary>
        public static DoodleArt V40B_PirateCat()
        {
            var p = new DoodlePainter(-0.48f, -0.05f, 0.6f, 1.38f, 140) { Ink = 0.04f };
            p.Shape(Sd.Polyline(0.06f, 0.25f, 0.08f, 0.45f, 0.2f, 0.48f, 0.45f), CatOrange);
            Body(p, Sd.Ellipse(0, 0.33f, 0.31f, 0.33f), CatOrange, CatShade);
            p.Fill(Sd.Ellipse(0, 0.26f, 0.15f, 0.2f), Rgba.Hex("FFE3C2"));
            // fascia a righe
            p.Fill(Sd.Intersect(Sd.Ellipse(0, 0.33f, 0.31f, 0.33f), Sd.Box(0, 0.5f, 0.4f, 0.06f)), Rgba.Hex("E8433F"));
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Ellipse(s * 0.13f, 0.05f, 0.1f, 0.06f), CatOrange, 0.03f);
            Body(p, Sd.Ellipse(0, 0.8f, 0.3f, 0.25f), CatOrange, CatShade);
            // occhio buono e benda
            p.Shape(Sd.Ellipse(0.11f, 0.83f, 0.065f, 0.08f), Rgba.White, 0.02f);
            p.Fill(Sd.Circle(0.11f, 0.82f, 0.035f), Ink);
            p.Fill(Sd.Circle(0.098f, 0.84f, 0.012f), Rgba.White);
            p.InkLine(0.025f, -0.29f, 0.95f, 0.2f, 0.72f);
            p.Shape(Sd.Ellipse(-0.11f, 0.83f, 0.08f, 0.075f), Ink, 0.02f);
            foreach (float s in new[] { -1f, 1f })
                for (int i = 0; i < 2; i++) p.InkLine(0.015f, s * 0.15f, 0.72f - i * 0.04f, s * 0.36f, 0.74f - i * 0.07f);
            p.Fill(Sd.Polygon(-0.04f, 0.74f, 0.04f, 0.74f, 0f, 0.7f), CatPink);
            p.InkLine(0.018f, -0.06f, 0.65f, 0f, 0.69f, 0.06f, 0.65f);
            // tricorno
            Sdf hat = Sd.Union(Sd.Ellipse(0, 1.03f, 0.42f, 0.09f), Sd.Polygon(-0.3f, 1.03f, -0.2f, 1.3f, 0f, 1.24f, 0.2f, 1.3f, 0.3f, 1.03f));
            p.Shape(hat, Rgba.Hex("2F2C38"));
            p.Fill(Sd.Box(0, 1.08f, 0.3f, 0.02f), Gold);
            p.Fill(Sd.Circle(0, 1.17f, 0.045f), Rgba.White);
            p.Fill(Sd.Circle(-0.015f, 1.18f, 0.01f), Ink);
            p.Fill(Sd.Circle(0.015f, 1.18f, 0.01f), Ink);
            return new DoodleArt("v40b_pirate_cat", p);
        }

        /// <summary>Barchetta a remi vista dall'alto (centro 0,0, lunga 0.9).</summary>
        public static DoodleArt V40B_Boat()
        {
            var p = new DoodlePainter(-0.35f, -0.55f, 0.35f, 0.55f, 120) { Ink = 0.04f };
            Sdf hull = Sd.Union(Sd.Ellipse(0, 0, 0.25f, 0.48f), Sd.Box(0, -0.2f, 0.25f, 0.25f, 0.1f));
            Body(p, hull, Rgba.Hex("B8682E"), Rgba.Hex("8A4A1E"));
            p.Shape(Sd.Grow(hull, -0.07f), Rgba.Hex("D2843F"), 0.03f);
            foreach (float y in new[] { 0.1f, -0.2f }) p.Shape(Sd.Box(0, y, 0.2f, 0.04f), Rgba.Hex("8A4A1E"), 0.02f);
            return new DoodleArt("v40b_boat", p);
        }

        /// <summary>X rossa sulla sabbia (centro 0,0, larga 0.7).</summary>
        public static DoodleArt V40B_XMark()
        {
            var p = new DoodlePainter(-0.4f, -0.4f, 0.4f, 0.4f, 140) { Ink = 0.04f };
            Sdf x = Sd.Union(Sd.Capsule(-0.28f, -0.28f, 0.28f, 0.28f, 0.07f), Sd.Capsule(-0.28f, 0.28f, 0.28f, -0.28f, 0.07f));
            p.Shape(x, Rgba.Hex("E8433F"));
            return new DoodleArt("v40b_x_mark", p);
        }

        /// <summary>Buca nella sabbia (centro 0,0, larga 1.2).</summary>
        public static DoodleArt V40B_SandHole()
        {
            var p = new DoodlePainter(-0.7f, -0.5f, 0.7f, 0.5f, 120) { Ink = 0.04f };
            p.Shape(Sd.Ellipse(0, 0, 0.62f, 0.42f), Rgba.Hex("E2B26E"));
            p.FillFunc(Sd.Ellipse(0, -0.03f, 0.48f, 0.3f), (x, y) => Rgba.Lerp(Rgba.Hex("6E4A2A"), Rgba.Hex("A87A48"), Clamp((-y + 0.3f) / 0.6f)));
            foreach (float a in new[] { 30f, 140f, 220f, 320f })
            {
                double r = a * Math.PI / 180;
                p.Fill(Sd.Circle((float)Math.Cos(r) * 0.55f, (float)Math.Sin(r) * 0.36f, 0.06f), Rgba.Hex("F7DCA4"));
            }
            return new DoodleArt("v40b_sand_hole", p);
        }

        /// <summary>Rotella del lucchetto con la finestrella per il simbolo (centro 0,0, larga 0.95).</summary>
        public static DoodleArt V40B_Dial()
        {
            var p = new DoodlePainter(-0.52f, -0.62f, 0.52f, 0.62f, 140) { Ink = 0.045f };
            Body(p, Sd.Box(0, 0, 0.44f, 0.54f, 0.12f), Gold, GoldShade);
            p.Shape(Sd.Box(0, 0, 0.36f, 0.4f, 0.08f), Rgba.Hex("FFF6DA"), 0.035f);
            foreach (float y in new[] { 0.48f, -0.48f }) for (float x = -0.3f; x <= 0.31f; x += 0.1f) p.InkLine(0.015f, x, y - 0.03f, x, y + 0.03f);
            return new DoodleArt("v40b_dial", p);
        }

        // ---------- Detective ----------

        /// <summary>Stanza del confronto all'americana con le righe dell'altezza (pavimento sotto y = -2.2).</summary>
        public static DoodleArt V40B_LineupBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.Fill(Sd.Box(0, 2.3f, 5f, 4.6f), Rgba.Hex("C9D3E6"), false);
            for (int i = 0; i < 8; i++)
            {
                float y = -2.2f + i * 0.5f;
                p.Line(V40BInk.WithAlpha(i % 2 == 0 ? 0.6f : 0.3f), i % 2 == 0 ? 0.04f : 0.025f, -4.5f, y, 4.5f, y);
            }
            foreach (float x in new[] { -3.2f, 3.2f })
                for (int i = 0; i < 8; i += 2)
                {
                    float y = -2.2f + i * 0.5f;
                    p.Line(V40BInk.WithAlpha(0.6f), 0.05f, x - 0.15f, y + 0.12f, x - 0.15f, y + 0.32f);
                }
            // finestra con la pioggia (in alto a sinistra) e lampada
            p.Fill(Sd.Box(0, 4.5f, 5f, 1.2f), Rgba.Hex("9AA0B2"), false);
            p.Shape(Sd.Box(0, 3.35f, 5f, 0.08f), Rgba.Hex("6B6677"));
            // pavimento
            p.Fill(Sd.Box(0, -4.6f, 5f, 2.4f), Rgba.Hex("6B6677"), false);
            for (float y = -2.7f; y > -7f; y -= 0.6f) p.Line(V40BInk.WithAlpha(0.35f), 0.03f, -4.5f, y, 4.5f, y);
            p.Shape(Sd.Box(0, -2.2f, 5f, 0.07f), Rgba.Hex("4A4655"));
            // numeri dei posti (righe a terra)
            foreach (float x in new[] { -1.7f, 0f, 1.7f }) p.Fill(Sd.Ellipse(x, -2.45f, 0.65f, 0.14f), V40BInk.WithAlpha(0.25f));
            return new DoodleArt("v40b_lineup_bg", p);
        }

        /// <summary>Cane seduto (piedi in y 0, centro x 0; zampe anteriori in ±0.17, 0.06; alto circa 1.55).</summary>
        public static DoodleArt V40B_Dog()
        {
            var p = new DoodlePainter(-0.65f, -0.08f, 0.75f, 1.62f, 120) { Ink = 0.045f };
            Rgba fur = Rgba.Hex("D9A066"), shade = Rgba.Hex("B87D45"), light = Rgba.Hex("FFE3C2"), ear = Rgba.Hex("8A5A3B");
            p.Shape(Sd.Polyline(0.07f, 0.3f, 0.2f, 0.55f, 0.3f, 0.65f, 0.55f), fur);
            Body(p, Sd.Ellipse(0, 0.45f, 0.4f, 0.45f), fur, shade);
            p.Fill(Sd.Ellipse(0, 0.42f, 0.2f, 0.3f), light);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Capsule(s * 0.17f, 0.45f, s * 0.17f, 0.08f, 0.1f), fur);
                p.Shape(Sd.Ellipse(s * 0.17f, 0.06f, 0.13f, 0.08f), light, 0.035f);
            }
            // collare
            p.Shape(Sd.Box(0, 0.83f, 0.27f, 0.05f, 0.03f), Rgba.Hex("3D6FD9"), 0.03f);
            p.Shape(Sd.Circle(0, 0.75f, 0.06f), Gold, 0.025f);
            // testa
            Body(p, Sd.Ellipse(0, 1.15f, 0.38f, 0.33f), fur, shade);
            foreach (float s in new[] { -1f, 1f })
                Body(p, Sd.Rotate(Sd.Ellipse(s * 0.38f, 1.08f, 0.11f, 0.26f), s * 0.38f, 1.3f, s * 18f), ear, Rgba.Hex("6E4228"));
            p.Shape(Sd.Ellipse(0, 1.02f, 0.2f, 0.14f), light, 0.03f);
            p.Shape(Sd.Ellipse(0, 1.1f, 0.07f, 0.05f), Ink, 0.02f);
            p.Fill(Sd.Circle(-0.02f, 1.12f, 0.015f), Rgba.White);
            p.InkLine(0.02f, 0, 1.05f, 0, 0.98f);
            p.InkLine(0.02f, -0.08f, 0.96f, 0f, 0.98f, 0.08f, 0.96f);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Ellipse(s * 0.13f, 1.26f, 0.06f, 0.07f), Rgba.White, 0.02f);
                p.Fill(Sd.Circle(s * 0.13f, 1.25f, 0.035f), Ink);
                p.Fill(Sd.Circle(s * 0.13f - 0.01f, 1.27f, 0.012f), Rgba.White);
            }
            return new DoodleArt("v40b_dog", p);
        }

        /// <summary>Pappagallo in piedi (piedi in y 0; ala in 0.05, 0.75 a parte in v40b_parrot_wing; alto circa 1.5).</summary>
        public static DoodleArt V40B_Parrot()
        {
            var p = new DoodlePainter(-0.5f, -0.08f, 0.55f, 1.55f, 120) { Ink = 0.045f };
            Rgba green = Rgba.Hex("4CC35A"), shade = Rgba.Hex("2E9A3F");
            // coda
            p.Shape(Sd.Polygon(-0.05f, 0.35f, 0.08f, 0.35f, 0.12f, -0.05f, 0.0f, -0.05f), Rgba.Hex("3D6FD9"));
            p.Shape(Sd.Polygon(0.02f, 0.35f, 0.12f, 0.35f, 0.24f, 0.0f, 0.12f, -0.02f), Rgba.Hex("E8433F"));
            Body(p, Sd.Ellipse(0, 0.65f, 0.3f, 0.42f), green, shade);
            p.Fill(Sd.Ellipse(0.05f, 0.55f, 0.16f, 0.28f), Rgba.Hex("B6EBA0"));
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Polyline(0.03f, s * 0.08f, 0.25f, s * 0.08f, 0.04f, s * 0.15f, 0.0f), Rgba.Hex("8D93A6"), 0.025f);
            // testa
            Body(p, Sd.Circle(0.02f, 1.17f, 0.27f), green, shade);
            p.Fill(Sd.Ellipse(-0.02f, 1.43f, 0.08f, 0.06f), Rgba.Hex("E8433F"));
            p.Shape(Sd.Ellipse(0.06f, 1.22f, 0.1f, 0.1f), Rgba.White, 0.02f);
            p.Fill(Sd.Circle(0.07f, 1.22f, 0.045f), Ink);
            p.Fill(Sd.Circle(0.055f, 1.24f, 0.015f), Rgba.White);
            Sdf beak = Sd.Union(Sd.Polygon(0.2f, 1.25f, 0.44f, 1.12f, 0.36f, 0.95f, 0.22f, 1.05f));
            p.Shape(beak, Rgba.Hex("FFC93C"));
            p.InkLine(0.015f, 0.23f, 1.08f, 0.38f, 1.04f);
            return new DoodleArt("v40b_parrot", p);
        }

        /// <summary>Ala del pappagallo (attaccatura in 0,0, punta verso il basso).</summary>
        public static DoodleArt V40B_ParrotWing()
        {
            var p = new DoodlePainter(-0.25f, -0.62f, 0.18f, 0.1f, 140) { Ink = 0.04f };
            Sdf wing = Sd.Polygon(-0.13f, 0.05f, 0.12f, 0.05f, 0.08f, -0.3f, -0.05f, -0.58f, -0.18f, -0.25f);
            Body(p, wing, Rgba.Hex("2E9A3F"), Rgba.Hex("1F7A30"));
            p.Fill(Sd.Intersect(wing, Sd.Box(0, -0.42f, 0.3f, 0.1f)), Rgba.Hex("3D6FD9"));
            p.Fill(Sd.Intersect(wing, Sd.Box(0, -0.55f, 0.3f, 0.05f)), Rgba.Hex("E8433F"));
            p.Shape(wing, Rgba.Clear);
            return new DoodleArt("v40b_parrot_wing", p);
        }

        /// <summary>Fango su una zampa (centro 0,0, largo 0.3).</summary>
        public static DoodleArt V40B_Mud()
        {
            var p = new DoodlePainter(-0.2f, -0.14f, 0.2f, 0.14f, 160) { Wobble = 0.01f };
            Rgba mud = Rgba.Hex("6E4A2A");
            p.Fill(Sd.Union(Sd.Ellipse(0, -0.02f, 0.14f, 0.08f), Sd.Circle(-0.09f, 0.04f, 0.05f), Sd.Circle(0.06f, 0.05f, 0.06f)), mud);
            p.Fill(Sd.Circle(0.14f, -0.08f, 0.025f), mud);
            p.Fill(Sd.Circle(-0.04f, 0.02f, 0.02f), Rgba.Hex("A87A48"));
            return new DoodleArt("v40b_mud", p);
        }

        /// <summary>Briciole di biscotto (centro 0,0, larghe 0.3).</summary>
        public static DoodleArt V40B_Crumbs()
        {
            var p = new DoodlePainter(-0.18f, -0.12f, 0.18f, 0.12f, 200) { Ink = 0.02f };
            float[][] c = { new[] { -0.1f, 0.02f, 0.035f }, new[] { 0.0f, -0.04f, 0.045f }, new[] { 0.1f, 0.04f, 0.03f }, new[] { 0.05f, 0.06f, 0.022f }, new[] { -0.03f, 0.07f, 0.02f }, new[] { 0.13f, -0.05f, 0.025f } };
            foreach (float[] k in c) p.Shape(Sd.Rotate(Sd.Box(k[0], k[1], k[2], k[2] * 0.8f, 0.01f), k[0], k[1], k[0] * 300f), Rgba.Hex("D9A066"));
            p.Fill(Sd.Circle(0.0f, -0.04f, 0.012f), Rgba.Hex("6E4228"));
            return new DoodleArt("v40b_crumbs", p);
        }

        /// <summary>
        /// Foto della scena del crimine (centro 0,0; 5.2 x 6.6): cucina di notte, tavolo con il piatto vuoto, finestra.
        /// Gli indizi sono sprite a parte, minuscoli.
        /// </summary>
        public static DoodleArt V40B_CrimeScene()
        {
            var p = new DoodlePainter(-2.7f, -3.4f, 2.7f, 3.4f, 60) { Ink = 0.05f };
            Sdf photo = Sd.Box(0, 0, 2.6f, 3.3f, 0.05f);
            p.Shape(photo, Rgba.White, 0.06f);
            Sdf pic = Sd.Box(0, 0.1f, 2.45f, 3.05f);
            p.Fill(Sd.Intersect(pic, Sd.Box(0, 1.6f, 2.5f, 1.6f)), Rgba.Hex("F5E1B8"), false);
            p.Fill(Sd.Intersect(pic, Sd.Box(0, -1.5f, 2.5f, 1.6f)), Rgba.Hex("C98B5A"), false);
            for (float y = -0.4f; y > -3f; y -= 0.45f) p.Fill(Sd.Intersect(pic, Sd.Box(0, y, 2.5f, 0.015f)), V40BInk.WithAlpha(0.3f), false);
            p.Fill(Sd.Box(0, 0.0f, 2.45f, 0.05f), Rgba.Hex("8C4A22"));
            // finestra con la pioggia
            p.Shape(Sd.Box(-1.2f, 2.0f, 0.75f, 0.65f, 0.04f), Rgba.Hex("3A4A6E"));
            for (int i = 0; i < 9; i++) p.Line(Rgba.Hex("9CC3EA", 0.7f), 0.02f, -1.75f + i * 0.13f, 2.5f - (i % 3) * 0.2f, -1.8f + i * 0.13f, 2.3f - (i % 3) * 0.2f);
            p.InkLine(0.04f, -1.2f, 1.35f, -1.2f, 2.65f);
            p.InkLine(0.04f, -1.95f, 2.0f, -0.45f, 2.0f);
            // mensola alta con barattoli
            p.Shape(Sd.Box(1.3f, 2.3f, 0.9f, 0.05f), Rgba.Hex("8C4A22"), 0.03f);
            p.Shape(Sd.Box(0.85f, 2.55f, 0.15f, 0.2f, 0.04f), Rgba.Hex("BFE3E6"), 0.03f);
            p.Shape(Sd.Box(1.3f, 2.5f, 0.13f, 0.15f, 0.04f), Rgba.Hex("FFC93C"), 0.03f);
            p.Shape(Sd.Ellipse(1.8f, 2.4f, 0.25f, 0.06f), Rgba.White, 0.03f);
            // tavolo con il piatto vuoto
            Body(p, Sd.Box(0.4f, 0.45f, 1.6f, 0.12f, 0.03f), Rgba.Hex("D2843F"), Rgba.Hex("A85E26"));
            foreach (float x in new[] { -1.0f, 1.8f }) p.Shape(Sd.Box(x, -0.55f, 0.08f, 0.9f), Rgba.Hex("A85E26"), 0.035f);
            p.Shape(Sd.Ellipse(0.5f, 0.66f, 0.6f, 0.11f), Rgba.White, 0.035f);
            p.Shape(Sd.Ellipse(0.5f, 0.68f, 0.4f, 0.06f), Rgba.Hex("EDEFF5"), 0.025f);
            p.Line(Rgba.Hex("6E4228", 0.8f), 0.03f, 0.3f, 0.7f, 0.42f, 0.68f);
            // sedia e ciotola
            p.Shape(Sd.Box(-1.8f, -0.4f, 0.4f, 0.07f), Rgba.Hex("E8433F"), 0.035f);
            p.Shape(Sd.Box(-2.13f, 0.3f, 0.07f, 0.7f), Rgba.Hex("E8433F"), 0.035f);
            foreach (float x in new[] { -2.13f, -1.5f }) p.Shape(Sd.Box(x, -1.05f, 0.05f, 0.6f), Rgba.Hex("E8433F"), 0.03f);
            p.Shape(Sd.Ellipse(1.4f, -2.3f, 0.35f, 0.12f), Rgba.Hex("5EC4D9"), 0.035f);
            // tappeto
            p.Shape(Sd.Ellipse(-0.3f, -2.0f, 1.2f, 0.3f), Rgba.Hex("9C6FD9"), 0.035f);
            p.Shape(Sd.Ellipse(-0.3f, -2.0f, 0.9f, 0.18f), Rgba.Hex("C3A6F2"), 0.025f);
            p.Shape(pic, Rgba.Clear, 0.035f);
            return new DoodleArt("v40b_crime_scene", p);
        }

        /// <summary>Scia di impronte infangate (centro 0,0, lunga 0.6).</summary>
        public static DoodleArt V40B_Prints()
        {
            var p = new DoodlePainter(-0.35f, -0.2f, 0.35f, 0.2f, 200) { Wobble = 0.006f };
            Rgba mud = Rgba.Hex("6E4A2A");
            for (int i = 0; i < 4; i++)
            {
                float x = -0.25f + i * 0.17f, y = (i % 2 == 0 ? -0.06f : 0.06f);
                p.Fill(Sd.Ellipse(x, y, 0.045f, 0.038f), mud);
                for (int k = 0; k < 3; k++) p.Fill(Sd.Circle(x + 0.045f, y + (k - 1) * 0.035f, 0.016f), mud);
            }
            return new DoodleArt("v40b_prints", p);
        }

        /// <summary>Lente d'ingrandimento (lente in 0,0, raggio 0.3; manico in basso a destra).</summary>
        public static DoodleArt V40B_Magnifier()
        {
            var p = new DoodlePainter(-0.42f, -0.75f, 0.75f, 0.42f, 140) { Ink = 0.045f };
            p.Shape(Sd.Capsule(0.25f, -0.25f, 0.6f, -0.6f, 0.08f), Rgba.Hex("8A4A1E"));
            p.Shape(Sd.Circle(0, 0, 0.36f), Gold);
            p.Shape(Sd.Circle(0, 0, 0.28f), Rgba.Hex("D8F1FF", 0.85f));
            p.Fill(Sd.Capsule(-0.14f, 0.08f, -0.05f, 0.16f, 0.04f), Rgba.White.WithAlpha(0.9f));
            return new DoodleArt("v40b_magnifier", p);
        }

        /// <summary>Manette (centro 0,0, larghe 1).</summary>
        public static DoodleArt V40B_Handcuffs()
        {
            var p = new DoodlePainter(-0.6f, -0.35f, 0.6f, 0.35f, 140) { Ink = 0.04f };
            foreach (float s in new[] { -1f, 1f })
            {
                Sdf ring = Sd.Subtract(Sd.Circle(s * 0.32f, -0.02f, 0.24f), Sd.Circle(s * 0.32f, -0.02f, 0.15f));
                Body(p, ring, V40BMetal, V40BMetalShade);
                p.Shape(Sd.Box(s * 0.14f, 0.12f, 0.06f, 0.07f, 0.02f), V40BMetal, 0.03f);
            }
            for (int i = 0; i < 3; i++) p.Shape(Sd.Ellipse(-0.06f + i * 0.06f, 0.15f, 0.04f, 0.025f), V40BMetal, 0.02f);
            return new DoodleArt("v40b_handcuffs", p);
        }

        /// <summary>Spunta verde (centro 0,0).</summary>
        public static DoodleArt V40B_Check()
        {
            var p = new DoodlePainter(-0.25f, -0.22f, 0.25f, 0.22f, 160) { Ink = 0.04f };
            p.Shape(Sd.Polyline(0.05f, -0.16f, 0f, -0.05f, -0.12f, 0.17f, 0.14f), Rgba.Hex("7ED957"));
            return new DoodleArt("v40b_check", p);
        }
    }
}
