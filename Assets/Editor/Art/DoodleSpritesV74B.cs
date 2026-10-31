using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.74 per il livello 274 "The Crying Onion" (il coltello è hw_knife, la candela hw_candle con
    /// flame, le pale e la griglia del ventilatore v67_fan_blades/v67_fan_grill, la bocca spalancata mouth_o, le lacrime
    /// sweat, il vento wind_lines, la festa glow e sparkle_star): la cucina (parete con finestra, mensola e gancio), il
    /// bancone davanti al cuoco, il frigo col suo sportello chiuso e aperto e la brina, il biglietto, il cuoco (corpo e
    /// testa senza occhi né bocca) con occhi e bocche a parte, gli occhialini da nuoto (lenti, cinghia, appannatura), la
    /// base del ventilatore da tavolo, il tagliere, la cipolla (intera, mezza, brinata, a cubetti), i fumi e il pane.
    /// Prefisso v74_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV74B() => new DoodleArt[]
        {
            V74B_KitchenBg(), V74B_Counter(), V74B_Fridge(), V74B_FridgeDoor(), V74B_FridgeDoorOpen(), V74B_FridgeFrost(),
            V74B_Note(), V74B_CookBody(), V74B_CookHead(),
            V74B_EyesWatery(), V74B_EyesDry(), V74B_EyesCry(), V74B_EyesHappy(),
            V74B_MouthWobble(), V74B_MouthWail(), V74B_MouthPuff(), V74B_MouthSmile(),
            V74B_Goggles(), V74B_GogglesStrap(), V74B_GogglesFog(), V74B_FanStand(), V74B_Board(),
            V74B_Onion(), V74B_OnionHalf(), V74B_OnionFrost(), V74B_OnionDiced(), V74B_Fume(), V74B_Bread(),
        };

        static readonly Rgba V74BFridge = Rgba.Hex("BFE4F2"), V74BFridgeShade = Rgba.Hex("8EC2D8");
        static readonly Rgba V74BOnion = Rgba.Hex("E7A95C"), V74BOnionShade = Rgba.Hex("C27D3A"), V74BOnionLine = Rgba.Hex("A8642B");
        static readonly Rgba V74BOnionFlesh = Rgba.Hex("FBF3E6"), V74BOnionRing = Rgba.Hex("E9D8F0");
        static readonly Rgba V74BChef = Rgba.Hex("FFFFFF"), V74BChefShade = Rgba.Hex("D9DEE8");
        static readonly Rgba V74BFrost = Rgba.Hex("EAF7FF"), V74BIce = Rgba.Hex("A9DBF5");
        static readonly Rgba V74BWood = Rgba.Hex("E2B47A"), V74BWoodShade = Rgba.Hex("C48F55");

        /// <summary>Contorno dell'onda di una cipolla intera (centro 0,0: bulbo r ~0.4, punta in alto a 0.62).</summary>
        static Sdf V74B_OnionShape() => Sd.Union(
            Sd.Ellipse(0f, -0.02f, 0.42f, 0.38f),
            Sd.Polygon(-0.2f, 0.25f, 0.2f, 0.25f, 0.035f, 0.58f, -0.035f, 0.58f),
            Sd.Ellipse(0f, -0.33f, 0.16f, 0.07f));

        /// <summary>
        /// La cucina, coordinate del mondo (come kitchen_bg): parete color crema, piastrelle bianche dietro il bancone
        /// (da y -1.3 a 0.9), la finestra in alto al centro, la mensola coi barattoli a destra e il gancio di ottone in
        /// (1.95, 1.45) dove sono appesi gli occhialini. Il bancone è un disegno a parte (davanti al cuoco).
        /// </summary>
        public static DoodleArt V74B_KitchenBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f, Wobble = 0.006f };
            p.Fill(Sd.Box(0f, 0f, 5f, 7.5f), Rgba.Hex("FBEFD6"), false);
            // carta da parati a righine
            for (int i = -9; i <= 9; i++) p.Line(Rgba.Hex("F3E2C0"), 0.12f, i * 0.5f, 0.9f, i * 0.5f, 7f);
            // piastrelle dietro il bancone
            Sdf tiles = Sd.Box(0f, -0.2f, 5f, 1.1f);
            p.FillFunc(tiles, (x, y) =>
            {
                int j = (int)Math.Floor((y + 1.3f) / 0.32f);
                int i = (int)Math.Floor((x + (j % 2 == 0 ? 0f : 0.22f)) / 0.44f);
                return (i + j) % 3 == 0 ? Rgba.Hex("EAF4F1") : Rgba.Hex("F7FBFA");
            });
            for (int j = 0; j <= 7; j++)
            {
                float y = -1.3f + j * 0.32f;
                p.Line(Rgba.Hex("C9DDD8"), 0.022f, -4.5f, y, 4.5f, y);
                for (int i = -11; i <= 11; i++)
                {
                    float x = i * 0.44f + (j % 2 == 0 ? 0f : 0.22f);
                    if (j < 7) p.Line(Rgba.Hex("C9DDD8"), 0.022f, x, y, x, y + 0.32f);
                }
            }
            p.Fill(Sd.Box(0f, 0.93f, 5f, 0.05f), Rgba.Hex("7CC3B4"), false);
            p.Line(Rgba.Hex("2B2233", 0.45f), 0.03f, -4.5f, 0.9f, 4.5f, 0.9f);
            // la finestra
            p.Fill(Sd.Box(0.45f, 2.05f, 0.95f, 0.72f, 0.06f), new Rgba(0f, 0f, 0f, 0.07f));
            p.Shape(Sd.Box(0.42f, 2.1f, 0.92f, 0.7f, 0.05f), Rgba.Hex("FFFFFF"));
            p.FillFunc(Sd.Box(0.42f, 2.1f, 0.8f, 0.58f), (x, y) => Rgba.Lerp(Rgba.Hex("A8DDF5"), Rgba.Hex("DDF3FC"), Math.Max(0f, Math.Min(1f, (2.68f - y) / 1.16f))));
            p.Fill(Sd.Union(Sd.Circle(-0.05f, 2.25f, 0.13f), Sd.Circle(0.1f, 2.3f, 0.16f), Sd.Circle(0.26f, 2.24f, 0.12f), Sd.Box(0.1f, 2.18f, 0.2f, 0.07f, 0.05f)), Rgba.White.WithAlpha(0.95f));
            p.Fill(Sd.Union(Sd.Ellipse(0.3f, 1.66f, 0.55f, 0.16f), Sd.Ellipse(0.85f, 1.62f, 0.5f, 0.14f)), Rgba.Hex("9CCB86"));
            p.Shape(Sd.Box(0.42f, 2.1f, 0.8f, 0.58f), Rgba.Clear, 0.04f);
            p.Shape(Sd.Box(0.42f, 2.1f, 0.03f, 0.58f), Rgba.White, 0.025f);
            p.Shape(Sd.Box(0.42f, 2.1f, 0.8f, 0.03f), Rgba.White, 0.025f);
            p.Shape(Sd.Box(0.42f, 1.36f, 1.05f, 0.06f, 0.03f), Rgba.Hex("FFFFFF"), 0.035f);
            // la mensola coi barattoli (a destra, in alto)
            p.Fill(Sd.Box(2.05f, 2.15f, 0.62f, 0.06f), new Rgba(0f, 0f, 0f, 0.08f));
            foreach (var j in new[] { (1.65f, 0.17f, 0.22f, "F2A65A"), (2.0f, 0.14f, 0.18f, "E86A5C"), (2.35f, 0.16f, 0.26f, "9BCB6B") })
            {
                float x = j.Item1, w = j.Item2, h = j.Item3;
                p.Shape(Sd.Box(x, 2.22f + h, w, h, 0.04f), Rgba.Hex("EAF6F8", 0.95f), 0.035f);
                p.Fill(Sd.Box(x, 2.22f + h * 0.8f, w - 0.03f, h * 0.75f, 0.03f), Rgba.Hex(j.Item4));
                p.Shape(Sd.Box(x, 2.25f + h * 2f, w * 0.85f, 0.05f, 0.02f), Rgba.Hex("B98552"), 0.03f);
            }
            p.Shape(Sd.Box(2.05f, 2.18f, 0.66f, 0.05f, 0.02f), Rgba.Hex("B98552"), 0.035f);
            foreach (float x in new[] { 1.6f, 2.5f }) p.Shape(Sd.Polygon(x - 0.03f, 2.13f, x + 0.03f, 2.13f, x + 0.03f, 1.98f, x - 0.12f, 2.13f), Rgba.Hex("8E6440"), 0.025f);
            // il gancio di ottone degli occhialini
            p.Fill(Sd.Ellipse(1.97f, 1.43f, 0.11f, 0.05f), new Rgba(0f, 0f, 0f, 0.08f));
            p.Shape(Sd.Circle(1.95f, 1.47f, 0.08f), Rgba.Hex("E2B04A"), 0.03f);
            p.Shape(Sd.Capsule(1.95f, 1.47f, 1.95f, 1.36f, 0.035f), Rgba.Hex("E2B04A"), 0.025f);
            p.Shape(Sd.Circle(1.95f, 1.35f, 0.045f), Rgba.Hex("F2C866"), 0.02f);
            // ombra del frigo sul muro
            p.Fill(Sd.Box(-0.68f, -1.7f, 0.1f, 3.6f), new Rgba(0f, 0f, 0f, 0.07f));
            return new DoodleArt("v74_b_kitchen_bg", p);
        }

        /// <summary>
        /// Il bancone, coordinate del mondo (sta davanti al cuoco): il piano di legno dal bordo dietro (y -1.3) al bordo
        /// davanti (y -2.25), il bordino, i cassetti e le ante verde salvia fino in fondo allo schermo.
        /// </summary>
        public static DoodleArt V74B_Counter()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, -1.2f, 50) { Ink = 0.06f, Wobble = 0.006f };
            // ante e cassetti
            p.Fill(Sd.Box(0f, -4.7f, 5f, 2.35f), Rgba.Hex("9CC9B2"), false);
            p.Fill(Sd.Box(0f, -2.42f, 5f, 0.08f), Rgba.Hex("7FAF97"), false);
            for (int i = -2; i <= 2; i++)
            {
                float x = i * 1.5f + 0.35f;
                p.Shape(Sd.Box(x, -2.85f, 0.66f, 0.27f, 0.06f), Rgba.Hex("AED6C1"), 0.045f);
                p.Shape(Sd.Capsule(x - 0.2f, -2.82f, x + 0.2f, -2.82f, 0.04f), Rgba.Hex("D9C38A"), 0.03f);
                p.Shape(Sd.Box(x, -4.55f, 0.66f, 1.25f, 0.08f), Rgba.Hex("AED6C1"), 0.045f);
                p.Shape(Sd.Box(x, -4.55f, 0.5f, 1.08f, 0.05f), Rgba.Hex("A3CFB8"), 0.03f);
                p.Shape(Sd.Circle(x + (i % 2 == 0 ? 0.5f : -0.5f), -3.9f, 0.06f), Rgba.Hex("D9C38A"), 0.03f);
            }
            p.Fill(Sd.Box(0f, -6.6f, 5f, 0.5f), Rgba.Hex("5C7F70"), false);
            // il piano di legno
            p.Fill(Sd.Box(0f, -2.28f, 5f, 0.12f), Rgba.Hex("B98552"), false);
            p.FillFunc(Sd.Box(0f, -1.77f, 5f, 0.47f), (x, y) =>
            {
                float k = Math.Max(0f, Math.Min(1f, (-1.3f - y) / 0.95f));
                Rgba a = Rgba.Lerp(Rgba.Hex("D7A56B"), V74BWood, k);
                int s = (int)Math.Floor((x + 4.5f) / 0.55f);
                return s % 2 == 0 ? a : Rgba.Lerp(a, Rgba.Hex("EBC48E"), 0.35f);
            });
            for (int s = -9; s <= 9; s++) p.Line(Rgba.Hex("B98552", 0.45f), 0.02f, s * 0.55f, -1.3f, s * 0.55f, -2.24f);
            p.Line(Rgba.Hex("2B2233", 0.75f), 0.045f, -4.5f, -1.3f, 4.5f, -1.3f);
            p.Line(Rgba.Hex("2B2233", 0.75f), 0.045f, -4.5f, -2.18f, 4.5f, -2.18f);
            p.Line(Rgba.Hex("2B2233", 0.55f), 0.035f, -4.5f, -2.42f, 4.5f, -2.42f);
            return new DoodleArt("v74_b_counter", p);
        }

        /// <summary>
        /// Il frigo (origine al centro dello sportello grande; mondo: oggetto in (-1.55, -1.6)): la cassa azzurra
        /// (x ±0.8, y -3.7..3.5), il freezer chiuso in alto (y 2.05..3.4) e l'interno illuminato dello scomparto grande
        /// (x ±0.64, y -3.4..1.85) con tre ripiani di vetro (y 0.75, -0.55, -1.85) e un po' di spesa; il posto vuoto al
        /// centro del ripiano di mezzo è per la cipolla. Lo sportello è un disegno a parte (chiuso o aperto).
        /// </summary>
        public static DoodleArt V74B_Fridge()
        {
            var p = new DoodlePainter(-0.9f, -3.8f, 0.9f, 3.6f, 70) { Ink = 0.05f };
            Body(p, Sd.Box(0f, -0.1f, 0.8f, 3.6f, 0.16f), V74BFridge, V74BFridgeShade);
            // freezer
            p.Shape(Sd.Box(0f, 2.72f, 0.74f, 0.67f, 0.1f), Rgba.Hex("CDEBF6"), 0.04f);
            p.Shape(Sd.Box(0.3f, 2.18f, 0.28f, 0.05f, 0.04f), Rgba.Hex("E9F0F4"), 0.03f);
            p.Fill(Sd.Ellipse(-0.45f, 3.05f, 0.12f, 0.22f), Rgba.White.WithAlpha(0.5f));
            // interno
            Sdf inside = Sd.Box(0f, -0.78f, 0.66f, 2.64f, 0.06f);
            p.FillFunc(inside, (x, y) => Rgba.Lerp(Rgba.Hex("FDFEFF"), Rgba.Hex("CFE7F2"), Math.Max(0f, Math.Min(1f, (1.85f - y) / 5.2f + Math.Abs(x) * 0.4f))));
            p.FillFunc(Sd.Ellipse(0f, 1.7f, 0.6f, 0.5f), (x, y) => Rgba.Hex("FFF9D6").WithAlpha(0.6f * (float)Math.Exp(-((x * x) + (y - 1.85f) * (y - 1.85f)) * 3f)));
            p.Shape(Sd.Box(0f, 1.78f, 0.16f, 0.05f, 0.03f), Rgba.Hex("FFF4B0"), 0.02f);   // la lucina
            // spesa sul ripiano alto: cartone del latte e barattolo di marmellata
            p.Shape(Sd.Union(Sd.Box(-0.32f, 1.15f, 0.17f, 0.38f, 0.02f), Sd.Polygon(-0.49f, 1.53f, -0.15f, 1.53f, -0.32f, 1.68f)), Rgba.White, 0.035f);
            p.Fill(Sd.Box(-0.32f, 1.1f, 0.16f, 0.13f), Rgba.Hex("5FA8E8"));
            p.Fill(Sd.Circle(-0.32f, 1.1f, 0.06f), Rgba.White);
            p.Shape(Sd.Box(0.3f, 0.98f, 0.19f, 0.2f, 0.06f), Rgba.Hex("E2504A", 0.9f), 0.035f);
            p.Shape(Sd.Box(0.3f, 1.22f, 0.21f, 0.05f, 0.02f), Rgba.Hex("F2D27A"), 0.03f);
            p.Fill(Sd.Ellipse(0.24f, 1.02f, 0.04f, 0.09f), Rgba.White.WithAlpha(0.45f));
            // ripiano di mezzo: solo una bottiglietta a destra (il posto al centro è libero)
            p.Shape(Sd.Union(Sd.Box(0.5f, -0.25f, 0.08f, 0.28f, 0.04f), Sd.Box(0.5f, 0.1f, 0.035f, 0.1f)), Rgba.Hex("8FD18A", 0.9f), 0.03f);
            // ripiano basso: formaggio e uova
            p.Shape(Sd.Polygon(-0.55f, -1.83f, 0.0f, -1.83f, 0.0f, -1.5f), Rgba.Hex("FFD45A"), 0.035f);
            foreach (float x in new[] { -0.4f, -0.2f }) p.Fill(Sd.Circle(x, -1.75f, 0.035f), Rgba.Hex("E8B43A"));
            foreach (float x in new[] { 0.18f, 0.36f, 0.54f }) p.Shape(Sd.Ellipse(x, -1.72f, 0.08f, 0.11f), Rgba.Hex("FFF3E0"), 0.03f);
            // cassetti delle verdure
            foreach (float x in new[] { -0.33f, 0.33f })
            {
                p.Shape(Sd.Box(x, -2.72f, 0.3f, 0.6f, 0.05f), Rgba.Hex("DDF2EA", 0.9f), 0.035f);
                p.Fill(Sd.Box(x, -2.9f, 0.26f, 0.35f, 0.04f), Rgba.Hex("9BD38A", 0.55f));
                p.Shape(Sd.Box(x, -2.2f, 0.12f, 0.035f, 0.02f), Rgba.Hex("B8D8E4"), 0.02f);
            }
            // ripiani di vetro (davanti a tutto)
            foreach (float y in new[] { 0.75f, -0.55f, -1.85f })
            {
                p.Fill(Sd.Box(0f, y - 0.02f, 0.64f, 0.035f), Rgba.Hex("A9D6EA", 0.75f));
                p.Line(Rgba.Hex("2B2233", 0.55f), 0.025f, -0.64f, y + 0.02f, 0.64f, y + 0.02f);
            }
            p.Shape(inside, Rgba.Clear, 0.045f);
            // piedini
            foreach (float x in new[] { -0.6f, 0.6f }) p.Shape(Sd.Box(x, -3.72f, 0.08f, 0.04f, 0.02f), Rgba.Hex("6B6677"), 0.02f);
            return new DoodleArt("v74_b_fridge", p);
        }

        /// <summary>Lo sportello chiuso del frigo (stessa origine del frigo; x ±0.8, y -3.55..1.95): maniglia a destra.</summary>
        public static DoodleArt V74B_FridgeDoor()
        {
            var p = new DoodlePainter(-0.9f, -3.65f, 0.9f, 2.05f, 70) { Ink = 0.05f };
            Sdf door = Sd.Box(0f, -0.8f, 0.8f, 2.75f, 0.1f);
            Body(p, door, Rgba.Hex("C8E9F5"), V74BFridgeShade, null, -0.05f, 0.05f);
            p.Fill(Sd.Ellipse(-0.48f, 0.6f, 0.1f, 0.9f), Rgba.White.WithAlpha(0.45f));
            p.Fill(Sd.Ellipse(-0.5f, -1.4f, 0.06f, 0.35f), Rgba.White.WithAlpha(0.35f));
            // la maniglia
            p.Fill(Sd.Box(0.63f, 0.42f, 0.07f, 0.8f, 0.05f), new Rgba(0f, 0f, 0f, 0.1f));
            p.Shape(Sd.Box(0.6f, 0.45f, 0.07f, 0.8f, 0.05f), Rgba.Hex("E9F0F4"), 0.035f);
            // la targhetta
            p.Shape(Sd.Box(0f, -3.2f, 0.28f, 0.08f, 0.04f), Rgba.Hex("E9F0F4"), 0.03f);
            p.InkLine(0.02f, -0.15f, -3.2f, 0.15f, -3.2f);
            return new DoodleArt("v74_b_fridge_door", p);
        }

        /// <summary>
        /// Lo sportello aperto (stessa origine del frigo): girato sul cardine di sinistra verso chi guarda, si vede di
        /// sbieco a sinistra della cassa (x -1.42..-0.8) con le mensoline interne e due bottiglie.
        /// </summary>
        public static DoodleArt V74B_FridgeDoorOpen()
        {
            var p = new DoodlePainter(-1.5f, -3.8f, -0.7f, 2.1f, 70) { Ink = 0.05f };
            Sdf panel = Sd.Polygon(-0.8f, 1.95f, -1.42f, 1.75f, -1.42f, -3.4f, -0.8f, -3.55f);
            Body(p, panel, Rgba.Hex("DDF1F8"), V74BFridgeShade, null, 0.05f, 0.04f);
            Sdf inner = Sd.Polygon(-0.86f, 1.8f, -1.34f, 1.62f, -1.34f, -3.25f, -0.86f, -3.38f);
            p.Fill(inner, Rgba.Hex("F4FAFD"));
            foreach (float y in new[] { 0.9f, -0.6f, -2.1f })
            {
                p.Shape(Sd.Polygon(-0.86f, y, -1.34f, y + 0.07f * (y + 3.4f) / 5f - 0.05f, -1.34f, y - 0.2f, -0.86f, y - 0.25f), Rgba.Hex("BFE4F2", 0.9f), 0.03f);
            }
            p.Shape(Sd.Union(Sd.Box(-1.05f, -0.2f, 0.09f, 0.36f, 0.04f), Sd.Box(-1.05f, 0.24f, 0.04f, 0.1f)), Rgba.Hex("F2A65A", 0.9f), 0.03f);
            p.Shape(Sd.Union(Sd.Box(-1.18f, -1.72f, 0.08f, 0.32f, 0.04f), Sd.Box(-1.18f, -1.33f, 0.035f, 0.09f)), Rgba.Hex("E86A5C", 0.9f), 0.03f);
            p.Shape(panel, Rgba.Clear, 0.05f);
            foreach (float y in new[] { 1.4f, -3.0f }) p.Shape(Sd.Box(-0.78f, y, 0.05f, 0.12f, 0.02f), Rgba.Hex("A9B1BC"), 0.025f);
            return new DoodleArt("v74_b_fridge_door_open", p);
        }

        /// <summary>La brina sullo sportello chiuso (stessa origine): bordi gelati, ghiaccioli in alto, cristalli agli angoli.</summary>
        public static DoodleArt V74B_FridgeFrost()
        {
            var p = new DoodlePainter(-0.9f, -3.65f, 0.9f, 2.05f, 70) { Ink = 0.03f, Wobble = 0.01f };
            Sdf door = Sd.Box(0f, -0.8f, 0.8f, 2.75f, 0.1f);
            Sdf rim = Sd.Subtract(door, Sd.Box(0f, -0.8f, 0.58f, 2.5f, 0.3f));
            p.FillFunc(rim, (x, y) => V74BFrost.WithAlpha(0.85f));
            p.Hatch(rim, V74BIce.WithAlpha(0.8f), 0.09f, 0.02f, 60f);
            // ghiaccioli dal bordo in alto
            for (int i = 0; i < 7; i++)
            {
                float x = -0.66f + i * 0.22f, h = 0.18f + 0.12f * ((i * 37) % 5) / 4f;
                p.Shape(Sd.Polygon(x - 0.06f, 1.92f, x + 0.06f, 1.92f, x, 1.92f - h - 0.1f), V74BFrost, 0.02f);
            }
            // cristalli
            foreach (var c in new[] { (-0.55f, 1.55f, 0.13f), (0.5f, -3.15f, 0.15f), (-0.6f, -2.6f, 0.1f), (0.55f, 1.3f, 0.09f) })
                for (int k = 0; k < 3; k++)
                {
                    double a = k * Math.PI / 3.0;
                    float dx = c.Item3 * (float)Math.Cos(a), dy = c.Item3 * (float)Math.Sin(a);
                    p.Line(Rgba.White, 0.03f, c.Item1 - dx, c.Item2 - dy, c.Item1 + dx, c.Item2 + dy);
                }
            p.Shape(door, Rgba.Clear, 0.0f);
            return new DoodleArt("v74_b_fridge_frost", p);
        }

        /// <summary>Il biglietto attaccato al frigo con la calamita (centro 0,0; 0.62 x 0.5; la scritta è un testo a parte).</summary>
        public static DoodleArt V74B_Note()
        {
            var p = new DoodlePainter(-0.38f, -0.32f, 0.38f, 0.36f, 160) { Ink = 0.025f };
            Sdf paper = Sd.Rotate(Sd.Box(0f, -0.02f, 0.31f, 0.25f, 0.01f), 0f, 0f, -4f);
            p.Fill(Sd.Offset(paper, 0.02f, -0.02f), new Rgba(0f, 0f, 0f, 0.12f));
            p.Shape(paper, Rgba.Hex("FFF7C2"));
            p.Fill(Sd.Rotate(Sd.Box(0f, 0.15f, 0.3f, 0.015f), 0f, 0f, -4f), Rgba.Hex("F2B8C6", 0.6f));
            // la calamita a forma di cipollina
            p.Shape(Sd.Union(Sd.Circle(0f, 0.22f, 0.085f), Sd.Polygon(-0.04f, 0.27f, 0.04f, 0.27f, 0f, 0.35f)), Rgba.Hex("B98AD6"), 0.022f);
            p.Fill(Sd.Circle(-0.03f, 0.24f, 0.022f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("v74_b_note", p);
        }

        /// <summary>
        /// Il corpo del cuoco (origine = base del collo; mondo: oggetto in (0.35, -0.6)): giacca bianca a doppio petto
        /// con fazzoletto rosso al collo, spalle larghe ±0.78, finisce in basso a y -1.25 (coperto dal bancone).
        /// </summary>
        public static DoodleArt V74B_CookBody()
        {
            var p = new DoodlePainter(-0.92f, -1.3f, 0.92f, 0.3f, 110) { Ink = 0.045f };
            Sdf torso = Sd.Union(Sd.Box(0f, -0.7f, 0.74f, 0.62f, 0.3f), Sd.Box(0f, -1.05f, 0.7f, 0.25f));
            Body(p, torso, V74BChef, V74BChefShade);
            p.Line(Rgba.Hex("2B2233", 0.5f), 0.025f, 0.12f, -0.18f, 0.12f, -1.25f);
            foreach (float y in new[] { -0.42f, -0.7f, -0.98f })
                foreach (float x in new[] { -0.08f, 0.32f })
                    p.Shape(Sd.Circle(x, y, 0.045f), Rgba.Hex("C9D1DB"), 0.02f);
            // il collo e il fazzoletto
            p.Shape(Sd.Box(0f, 0.04f, 0.17f, 0.14f, 0.06f), Skin, 0.035f);
            p.Shape(Sd.Union(Sd.Ellipse(0f, -0.12f, 0.3f, 0.1f), Sd.Polygon(-0.05f, -0.15f, 0.1f, -0.15f, 0.2f, -0.42f, 0.02f, -0.38f)), Rgba.Hex("E2504A"), 0.035f);
            // il colletto
            p.Shape(Sd.Union(Sd.Polygon(-0.36f, -0.04f, -0.1f, -0.2f, -0.2f, 0.04f), Sd.Polygon(0.36f, -0.04f, 0.1f, -0.2f, 0.2f, 0.04f)), V74BChef, 0.03f);
            return new DoodleArt("v74_b_cook_body", p);
        }

        /// <summary>
        /// La testa del cuoco senza occhi né bocca (stessa origine del corpo): faccia tonda in (0, 0.6) r 0.5, orecchie,
        /// basette, naso a patata in (0, 0.53), baffoni sopra la bocca (bocca in (0, 0.3)), occhi in (±0.19, 0.7) e il
        /// cappello da cuoco alto fino a y 1.9. Si tinge di blu quando trattiene il fiato.
        /// </summary>
        public static DoodleArt V74B_CookHead()
        {
            var p = new DoodlePainter(-0.7f, 0.02f, 0.7f, 1.95f, 110) { Ink = 0.045f };
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Ellipse(s * 0.5f, 0.6f, 0.1f, 0.14f), Skin, 0.035f);
            Sdf face = Sd.Ellipse(0f, 0.6f, 0.5f, 0.52f);
            Body(p, face, Skin, SkinShade, null, -0.05f, 0.05f);
            // basette
            foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Intersect(face, Sd.Box(s * 0.44f, 0.82f, 0.08f, 0.18f)), Rgba.Hex("5A3A26"));
            p.Shape(face, Rgba.Clear);
            // guance
            foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Ellipse(s * 0.3f, 0.47f, 0.09f, 0.06f), Rgba.Hex("FF9A8A", 0.45f));
            // il naso
            p.Shape(Sd.Ellipse(0f, 0.53f, 0.1f, 0.085f), Rgba.Hex("F7B98E"), 0.03f);
            p.Fill(Sd.Circle(-0.03f, 0.56f, 0.025f), Rgba.White.WithAlpha(0.6f));
            // i baffoni
            Sdf stache = Sd.Union(Sd.Ellipse(-0.13f, 0.42f, 0.15f, 0.06f), Sd.Ellipse(0.13f, 0.42f, 0.15f, 0.06f),
                Sd.Circle(-0.27f, 0.45f, 0.045f), Sd.Circle(0.27f, 0.45f, 0.045f));
            p.Shape(stache, Rgba.Hex("5A3A26"), 0.03f);
            // il cappello
            Sdf puff = Sd.Union(Sd.Circle(-0.28f, 1.5f, 0.26f), Sd.Circle(0.0f, 1.62f, 0.3f), Sd.Circle(0.28f, 1.5f, 0.26f), Sd.Box(0f, 1.3f, 0.4f, 0.18f, 0.05f));
            Body(p, puff, V74BChef, V74BChefShade);
            foreach (float x in new[] { -0.14f, 0.14f }) p.Line(V74BChefShade, 0.025f, x, 1.2f, x * 1.2f, 1.55f);
            p.Shape(Sd.Box(0f, 1.13f, 0.43f, 0.11f, 0.04f), V74BChef, 0.04f);
            return new DoodleArt("v74_b_cook_head", p);
        }

        static void V74B_Brows(DoodlePainter p, float tilt, float y)
        {
            foreach (float s in new[] { -1f, 1f })
                p.Shape(Sd.Capsule(s * 0.1f, y + tilt, s * 0.29f, y - tilt, 0.025f), Rgba.Hex("5A3A26"), 0.015f);
        }

        /// <summary>Occhi lucidi (origine = punto in mezzo agli occhi): grandi, con l'acqua che sale dal basso e sopracciglia a tetto.</summary>
        public static DoodleArt V74B_EyesWatery()
        {
            var p = new DoodlePainter(-0.36f, -0.2f, 0.36f, 0.24f, 160) { Ink = 0.03f };
            foreach (float s in new[] { -1f, 1f })
            {
                float x = s * 0.19f;
                Sdf eye = Sd.Ellipse(x, 0f, 0.115f, 0.13f);
                p.Fill(eye, Rgba.White);
                p.Fill(Sd.Circle(x, 0.0f, 0.06f), Ink);
                p.Fill(Sd.Circle(x - 0.025f, 0.03f, 0.024f), Rgba.White);
                p.Fill(Sd.Circle(x + 0.03f, -0.025f, 0.012f), Rgba.White);
                // l'acqua che sale
                p.Fill(Sd.Intersect(eye, Sd.Box(x, -0.1f, 0.2f, 0.055f)), Rgba.Hex("7FC8F5", 0.75f));
                p.Line(Rgba.White.WithAlpha(0.8f), 0.012f, x - 0.07f, -0.05f, x + 0.04f, -0.05f);
                p.Shape(eye, Rgba.Clear);
                // goccia sul bordo
                p.Shape(Sd.Union(Sd.Circle(x + s * 0.09f, -0.13f, 0.03f), Sd.Polygon(x + s * 0.09f - 0.026f, -0.12f, x + s * 0.09f + 0.026f, -0.12f, x + s * 0.09f, -0.08f)), Rgba.Hex("9AD3F5"), 0.012f);
            }
            V74B_Brows(p, 0.035f, 0.18f);
            return new DoodleArt("v74_b_eyes_watery", p);
        }

        /// <summary>Occhi asciutti e tranquilli (origine come gli altri occhi).</summary>
        public static DoodleArt V74B_EyesDry()
        {
            var p = new DoodlePainter(-0.36f, -0.2f, 0.36f, 0.24f, 160) { Ink = 0.03f };
            foreach (float s in new[] { -1f, 1f })
            {
                float x = s * 0.19f;
                Sdf eye = Sd.Ellipse(x, 0f, 0.1f, 0.12f);
                p.Fill(eye, Rgba.White);
                p.Fill(Sd.Circle(x, -0.01f, 0.055f), Ink);
                p.Fill(Sd.Circle(x - 0.02f, 0.02f, 0.02f), Rgba.White);
                p.Shape(eye, Rgba.Clear);
            }
            V74B_Brows(p, -0.012f, 0.19f);
            return new DoodleArt("v74_b_eyes_dry", p);
        }

        /// <summary>Occhi strizzati che piangono a dirotto (origine come gli altri occhi): due "&gt; &lt;" e i rivoli.</summary>
        public static DoodleArt V74B_EyesCry()
        {
            var p = new DoodlePainter(-0.36f, -0.42f, 0.36f, 0.24f, 160) { Ink = 0.03f };
            foreach (float s in new[] { -1f, 1f })
            {
                float x = s * 0.19f;
                // i rivoli che scendono sulle guance
                p.Shape(Sd.Union(Sd.Capsule(x + s * 0.05f, -0.03f, x + s * 0.07f, -0.33f, 0.04f), Sd.Circle(x + s * 0.07f, -0.35f, 0.05f)), Rgba.Hex("8ECDF5", 0.92f), 0.015f);
                p.Line(Rgba.White.WithAlpha(0.7f), 0.012f, x + s * 0.04f, -0.06f, x + s * 0.05f, -0.25f);
                p.InkLine(0.035f, x - s * 0.1f, 0.06f, x + s * 0.06f, 0f, x - s * 0.1f, -0.06f);
            }
            V74B_Brows(p, 0.05f, 0.17f);
            return new DoodleArt("v74_b_eyes_cry", p);
        }

        /// <summary>Occhi felici a mezzaluna (origine come gli altri occhi).</summary>
        public static DoodleArt V74B_EyesHappy()
        {
            var p = new DoodlePainter(-0.36f, -0.2f, 0.36f, 0.24f, 160) { Ink = 0.03f };
            foreach (float s in new[] { -1f, 1f })
            {
                float x = s * 0.19f;
                p.Shape(Sd.Subtract(Sd.Circle(x, -0.04f, 0.1f), Sd.Circle(x, -0.1f, 0.1f)), Ink, 0.01f);
            }
            V74B_Brows(p, -0.025f, 0.19f);
            return new DoodleArt("v74_b_eyes_happy", p);
        }

        /// <summary>Bocca tremolante (origine = centro della bocca).</summary>
        public static DoodleArt V74B_MouthWobble()
        {
            var p = new DoodlePainter(-0.2f, -0.08f, 0.2f, 0.08f, 160) { Ink = 0.03f };
            p.InkLine(0.03f, -0.13f, -0.02f, -0.07f, 0.02f, 0f, -0.02f, 0.07f, 0.02f, 0.13f, -0.02f);
            return new DoodleArt("v74_b_mouth_wobble", p);
        }

        /// <summary>Bocca spalancata del pianto a dirotto (origine = centro della bocca).</summary>
        public static DoodleArt V74B_MouthWail()
        {
            var p = new DoodlePainter(-0.24f, -0.2f, 0.24f, 0.1f, 160) { Ink = 0.03f };
            Sdf m = Sd.Union(Sd.Ellipse(0f, -0.05f, 0.18f, 0.12f), Sd.Box(0f, 0.0f, 0.18f, 0.05f, 0.04f));
            p.Shape(m, Rgba.Hex("6E2433"));
            p.Fill(Sd.Intersect(m, Sd.Ellipse(0f, -0.15f, 0.11f, 0.06f)), Rgba.Hex("FF8FA3"));
            p.Fill(Sd.Intersect(m, Sd.Box(0f, 0.04f, 0.14f, 0.025f)), Rgba.White);
            p.Shape(m, Rgba.Clear);
            return new DoodleArt("v74_b_mouth_wail", p);
        }

        /// <summary>Fiato trattenuto (origine = centro della bocca): labbra strette e guance gonfie ai lati.</summary>
        public static DoodleArt V74B_MouthPuff()
        {
            var p = new DoodlePainter(-0.5f, -0.2f, 0.5f, 0.2f, 160) { Ink = 0.03f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Circle(s * 0.3f, 0.02f, 0.15f), Skin, 0.03f);
                p.Fill(Sd.Circle(s * 0.3f, 0.0f, 0.08f), Rgba.Hex("FF9A8A", 0.5f));
                p.Fill(Sd.Circle(s * 0.3f - 0.05f, 0.08f, 0.03f), Rgba.White.WithAlpha(0.6f));
            }
            p.Shape(Sd.Ellipse(0f, -0.01f, 0.05f, 0.035f), Rgba.Hex("C9506A"), 0.025f);
            return new DoodleArt("v74_b_mouth_puff", p);
        }

        /// <summary>Gran sorriso (origine = centro della bocca).</summary>
        public static DoodleArt V74B_MouthSmile()
        {
            var p = new DoodlePainter(-0.24f, -0.2f, 0.24f, 0.08f, 160) { Ink = 0.03f };
            Sdf m = Sd.Intersect(Sd.Ellipse(0f, 0.0f, 0.19f, 0.15f), Sd.Box(0f, -0.1f, 0.2f, 0.1f));
            p.Shape(m, Rgba.Hex("6E2433"));
            p.Fill(Sd.Intersect(m, Sd.Box(0f, -0.02f, 0.2f, 0.03f)), Rgba.White);
            p.Fill(Sd.Intersect(m, Sd.Ellipse(0f, -0.14f, 0.1f, 0.05f)), Rgba.Hex("FF8FA3"));
            p.Shape(m, Rgba.Clear);
            return new DoodleArt("v74_b_mouth_smile", p);
        }

        /// <summary>Occhialini da nuoto (origine = in mezzo alle lenti; lenti in ±0.19, r 0.15) con i mozziconi della cinghia.</summary>
        public static DoodleArt V74B_Goggles()
        {
            var p = new DoodlePainter(-0.5f, -0.22f, 0.5f, 0.22f, 160) { Ink = 0.03f };
            foreach (float s in new[] { -1f, 1f })
                p.Shape(Sd.Box(s * 0.4f, 0f, 0.08f, 0.04f, 0.02f), Rgba.Hex("2E8FD6"), 0.025f);
            p.Shape(Sd.Capsule(-0.06f, 0.01f, 0.06f, 0.01f, 0.03f), Rgba.Hex("2E8FD6"), 0.025f);
            foreach (float s in new[] { -1f, 1f })
            {
                float x = s * 0.19f;
                p.Shape(Sd.Ellipse(x, 0f, 0.17f, 0.15f), Rgba.Hex("2E8FD6"), 0.03f);
                p.Fill(Sd.Ellipse(x, 0f, 0.12f, 0.1f), Rgba.Hex("BFE8FF", 0.55f));
                p.Fill(Sd.Rotate(Sd.Ellipse(x - 0.04f, 0.04f, 0.05f, 0.02f), x - 0.04f, 0.04f, 30f), Rgba.White.WithAlpha(0.8f));
                p.Shape(Sd.Ellipse(x, 0f, 0.12f, 0.1f), Rgba.Clear, 0.02f);
            }
            return new DoodleArt("v74_b_goggles", p);
        }

        /// <summary>La cinghia degli occhialini appesi (stessa origine): sale dai lati fino al gancio, 0.5 più in alto.</summary>
        public static DoodleArt V74B_GogglesStrap()
        {
            var p = new DoodlePainter(-0.52f, -0.08f, 0.52f, 0.58f, 160) { Ink = 0.025f, Wobble = 0.004f };
            Sdf strap = Sd.Union(Sd.Polyline(0.03f, -0.44f, 0.0f, -0.4f, 0.22f, -0.2f, 0.42f, 0f, 0.5f),
                                 Sd.Polyline(0.03f, 0.44f, 0.0f, 0.4f, 0.22f, 0.2f, 0.42f, 0f, 0.5f));
            p.Shape(strap, Rgba.Hex("1F6FB0"), 0.02f);
            return new DoodleArt("v74_b_goggles_strap", p);
        }

        /// <summary>L'appannatura delle lenti (stessa origine degli occhialini): bianco latte con goccioline.</summary>
        public static DoodleArt V74B_GogglesFog()
        {
            var p = new DoodlePainter(-0.36f, -0.16f, 0.36f, 0.16f, 160) { Ink = 0.02f };
            foreach (float s in new[] { -1f, 1f })
            {
                float x = s * 0.19f;
                Sdf lens = Sd.Ellipse(x, 0f, 0.125f, 0.105f);
                p.Fill(lens, Rgba.Hex("F4F8FA", 0.95f));
                for (int k = 0; k < 4; k++) p.Fill(Sd.Circle(x - 0.07f + k * 0.045f, -0.04f + (k % 2) * 0.05f, 0.012f), Rgba.Hex("BFD8E6"));
                p.Shape(lens, Rgba.Clear, 0.015f);
            }
            return new DoodleArt("v74_b_goggles_fog", p);
        }

        /// <summary>Ventilatore da tavolo senza pale né griglia (base in 0,0; centro della testa in 0, 0.85, r 0.36).</summary>
        public static DoodleArt V74B_FanStand()
        {
            var p = new DoodlePainter(-0.45f, -0.08f, 0.45f, 1.27f, 110) { Ink = 0.04f };
            p.Fill(Sd.Ellipse(0.04f, 0.02f, 0.4f, 0.07f), new Rgba(0f, 0f, 0f, 0.12f));
            Body(p, Sd.Ellipse(0f, 0.08f, 0.32f, 0.1f), Rgba.Hex("7ED3B2"), Rgba.Hex("4FA88A"));
            p.Shape(Sd.Circle(0.17f, 0.08f, 0.03f), Rgba.Hex("FF8FA3"), 0.015f);
            Body(p, Sd.Capsule(0f, 0.12f, 0f, 0.6f, 0.05f), Rgba.Hex("D9DEE8"), Rgba.Hex("A9B0C0"));
            Body(p, Sd.Ellipse(0f, 0.62f, 0.12f, 0.08f), Rgba.Hex("7ED3B2"), Rgba.Hex("4FA88A"));
            p.Shape(Sd.Circle(0f, 0.85f, 0.38f), Rgba.Hex("E8F6FF", 0.55f), 0.035f);
            return new DoodleArt("v74_b_fan_stand", p);
        }

        /// <summary>Il tagliere di legno (centro 0,0; ±0.88 x ±0.3, con il manico a destra e lo spessore sotto).</summary>
        public static DoodleArt V74B_Board()
        {
            var p = new DoodlePainter(-0.98f, -0.42f, 1.2f, 0.36f, 100) { Ink = 0.04f };
            p.Fill(Sd.Box(0.06f, -0.12f, 0.95f, 0.25f, 0.2f), new Rgba(0f, 0f, 0f, 0.12f));
            Sdf side = Sd.Union(Sd.Box(0f, -0.07f, 0.88f, 0.26f, 0.22f), Sd.Box(0.95f, -0.07f, 0.18f, 0.1f, 0.08f));
            p.Shape(side, V74BWoodShade);
            Sdf top = Sd.Union(Sd.Box(0f, 0f, 0.88f, 0.26f, 0.22f), Sd.Box(0.95f, 0f, 0.18f, 0.1f, 0.08f));
            p.Shape(top, Rgba.Hex("EFC58C"));
            foreach (float y in new[] { -0.12f, 0.02f, 0.14f }) p.Line(Rgba.Hex("D9A86A", 0.7f), 0.018f, -0.7f, y, 0.75f, y + 0.01f);
            p.Shape(Sd.Circle(1.02f, 0f, 0.045f), V74BWoodShade, 0.025f);
            return new DoodleArt("v74_b_board", p);
        }

        /// <summary>La cipolla intera (centro 0,0; bulbo ±0.42, punta in alto a 0.6, radichette in basso a -0.45).</summary>
        public static DoodleArt V74B_Onion()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.5f, 0.66f, 150) { Ink = 0.035f };
            // radichette
            foreach (float x in new[] { -0.08f, -0.03f, 0.02f, 0.07f })
                p.Line(Rgba.Hex("C9A27A"), 0.02f, x * 0.6f, -0.36f, x, -0.42f, x * 1.3f, -0.46f);
            Sdf o = V74B_OnionShape();
            Body(p, o, V74BOnion, V74BOnionShade, null, -0.07f, 0.06f);
            // le venature della buccia
            foreach (float k in new[] { -0.6f, -0.25f, 0.1f, 0.45f })
                p.Line(V74BOnionLine.WithAlpha(0.7f), 0.018f, k * 0.5f, -0.33f, k * 0.62f + 0.02f * k, -0.05f, k * 0.35f, 0.25f, k * 0.06f, 0.55f);
            p.Fill(Sd.Ellipse(-0.2f, 0.06f, 0.06f, 0.16f), Rgba.White.WithAlpha(0.35f));
            // la buccia secca che si stacca sulla punta
            p.Shape(Sd.Polygon(0.02f, 0.5f, 0.12f, 0.64f, 0.07f, 0.5f), Rgba.Hex("D9B07A"), 0.015f);
            p.Shape(o, Rgba.Clear);
            return new DoodleArt("v74_b_onion", p);
        }

        /// <summary>
        /// La mezza cipolla di sinistra (stessa origine della cipolla intera: affiancata alla sua gemella specchiata la
        /// ricompone): buccia a sinistra, taglio a destra con gli anelli bianco-lilla.
        /// </summary>
        public static DoodleArt V74B_OnionHalf()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.06f, 0.66f, 150) { Ink = 0.035f };
            Sdf half = Sd.Intersect(V74B_OnionShape(), Sd.Box(-0.5f, 0.08f, 0.5f, 0.7f));
            Body(p, half, V74BOnion, V74BOnionShade, null, -0.07f, 0.06f);
            // il taglio: anelli concentrici visti di taglio
            Sdf cut = Sd.Intersect(Sd.Ellipse(0f, -0.02f, 0.12f, 0.37f), Sd.Box(-0.06f, -0.02f, 0.06f, 0.5f));
            p.Fill(cut, V74BOnionFlesh);
            for (int i = 1; i <= 3; i++)
                p.Fill(Sd.Intersect(Sd.Subtract(Sd.Ellipse(0f, -0.02f, 0.12f * i / 4f + 0.01f, 0.37f * i / 4f + 0.01f), Sd.Ellipse(0f, -0.02f, 0.12f * i / 4f - 0.008f, 0.37f * i / 4f - 0.01f)), Sd.Box(-0.06f, 0f, 0.06f, 0.5f)), V74BOnionRing);
            p.Shape(half, Rgba.Clear);
            return new DoodleArt("v74_b_onion_half", p);
        }

        /// <summary>La brina sopra la cipolla fredda (stessa origine): crosticina bianca in alto, riflessi azzurri, cristalli.</summary>
        public static DoodleArt V74B_OnionFrost()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.5f, 0.7f, 150) { Ink = 0.02f, Wobble = 0.008f };
            Sdf o = V74B_OnionShape();
            p.Fill(o, Rgba.Hex("BFE6FF", 0.35f));
            Sdf cap = Sd.Intersect(o, Sd.Union(Sd.Ellipse(0f, 0.32f, 0.5f, 0.2f), Sd.Box(0f, 0.5f, 0.2f, 0.2f)));
            p.Fill(cap, V74BFrost.WithAlpha(0.95f));
            for (int i = 0; i < 6; i++)
            {
                float x = -0.3f + i * 0.12f;
                p.Fill(Sd.Intersect(o, Sd.Circle(x, 0.2f - 0.03f * (i % 2), 0.05f)), V74BFrost.WithAlpha(0.95f));
            }
            foreach (var c in new[] { (-0.22f, -0.12f), (0.18f, 0.0f), (0.05f, -0.24f), (-0.05f, 0.12f) })
                for (int k = 0; k < 3; k++)
                {
                    double a = k * Math.PI / 3.0;
                    float dx = 0.045f * (float)Math.Cos(a), dy = 0.045f * (float)Math.Sin(a);
                    p.Line(Rgba.White, 0.016f, c.Item1 - dx, c.Item2 - dy, c.Item1 + dx, c.Item2 + dy);
                }
            p.Fill(Sd.Ellipse(-0.24f, 0.08f, 0.05f, 0.14f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("v74_b_onion_frost", p);
        }

        /// <summary>La cipolla tagliata a cubetti, in un mucchietto ordinato (centro 0,0, base in y -0.3).</summary>
        public static DoodleArt V74B_OnionDiced()
        {
            var p = new DoodlePainter(-0.62f, -0.4f, 0.62f, 0.28f, 150) { Ink = 0.025f };
            p.Fill(Sd.Ellipse(0f, -0.3f, 0.55f, 0.08f), new Rgba(0f, 0f, 0f, 0.1f));
            var rnd = new Random(274);
            for (int row = 0; row < 4; row++)
            {
                int n = 7 - row * 2 + (row == 3 ? 1 : 0);
                for (int i = 0; i < n; i++)
                {
                    float x = (i - (n - 1) * 0.5f) * 0.13f + (float)(rnd.NextDouble() - 0.5) * 0.03f;
                    float y = -0.24f + row * 0.11f + (float)(rnd.NextDouble() - 0.5) * 0.02f;
                    float a = (float)(rnd.NextDouble() - 0.5) * 40f;
                    Sdf cube = Sd.Rotate(Sd.Box(x, y, 0.06f, 0.05f, 0.015f), x, y, a);
                    p.Shape(cube, (i + row) % 3 == 0 ? V74BOnionRing : V74BOnionFlesh, 0.018f);
                    p.Fill(Sd.Rotate(Sd.Box(x - 0.01f, y + 0.02f, 0.035f, 0.008f), x, y, a), Rgba.Hex("D9C3E6", 0.8f));
                }
            }
            return new DoodleArt("v74_b_onion_diced", p);
        }

        /// <summary>Un fumo della cipolla (base in 0,0, sale fino a 0.6): nastro ondulato verde-giallino col ricciolo in cima.</summary>
        public static DoodleArt V74B_Fume()
        {
            var p = new DoodlePainter(-0.2f, -0.05f, 0.2f, 0.66f, 150) { Ink = 0.02f, Wobble = 0.004f };
            Sdf ribbon = Sd.Polyline(0.03f, 0f, 0f, 0.07f, 0.1f, 0.0f, 0.22f, -0.07f, 0.32f, 0.0f, 0.43f, 0.07f, 0.5f, 0.04f, 0.58f, -0.03f, 0.57f, -0.03f, 0.51f);
            p.Shape(ribbon, Rgba.Hex("A9D84A", 0.9f), 0.018f);
            p.Fill(Sd.Polyline(0.01f, 0.0f, 0.03f, 0.05f, 0.1f, -0.01f, 0.2f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("v74_b_fume", p);
        }

        /// <summary>Una fetta di pancarré (centro 0,0; ±0.24 x ±0.24) con la crosta.</summary>
        public static DoodleArt V74B_Bread()
        {
            var p = new DoodlePainter(-0.3f, -0.3f, 0.3f, 0.32f, 150) { Ink = 0.03f };
            Sdf slice = Sd.Union(Sd.Box(0f, -0.06f, 0.21f, 0.18f, 0.04f), Sd.Circle(-0.11f, 0.12f, 0.12f), Sd.Circle(0.11f, 0.12f, 0.12f));
            p.Shape(slice, Rgba.Hex("C98A45"));
            Sdf crumb = Sd.Union(Sd.Box(0f, -0.06f, 0.16f, 0.14f, 0.03f), Sd.Circle(-0.1f, 0.11f, 0.085f), Sd.Circle(0.1f, 0.11f, 0.085f));
            p.Fill(crumb, Rgba.Hex("FBE7B8"));
            foreach (var d in new[] { (-0.06f, 0.02f), (0.07f, -0.08f), (0.02f, 0.1f), (-0.09f, -0.12f), (0.1f, 0.05f) })
                p.Fill(Sd.Ellipse(d.Item1, d.Item2, 0.015f, 0.01f), Rgba.Hex("E2C489"));
            return new DoodleArt("v74_b_bread", p);
        }
    }
}
