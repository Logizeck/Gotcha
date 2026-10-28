using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.69 per il livello 266 "The Crooked Picture" (la stanza, il quadro col gatto nobile, il
    /// tavolo, la mensola, la finestra, la lampada e la biglia sono quelli esistenti): il chiodo e il filo del quadro,
    /// l'occhiolino del gatto del quadro (coordinate di "heist_painting"), il bicchiere vuoto in due strati con la sua
    /// maschera (l'acqua è un rettangolo mascherato che resta orizzontale), la livella a bolla con la bolla, il rotolo
    /// di nastro adesivo, il pezzo di nastro e il martello.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV69B() => new DoodleArt[]
        {
            V69B_Nail(), V69B_PictureWire(), V69B_Wink(), V69B_GlassBack(), V69B_GlassFront(), V69B_GlassMask(),
            V69B_SpiritLevel(), V69B_LevelBubble(), V69B_TapeRoll(), V69B_TapeStrip(), V69B_Hammer(),
        };

        static readonly Rgba V69BMetal = Rgba.Hex("A9B0BF"), V69BMetalShade = Rgba.Hex("7C8496");

        /// <summary>Chiodo piantato nel muro (testa tonda, centro 0,0).</summary>
        public static DoodleArt V69B_Nail()
        {
            var p = new DoodlePainter(-0.12f, -0.12f, 0.12f, 0.12f, 200) { Ink = 0.025f };
            Body(p, Sd.Circle(0f, 0f, 0.075f), V69BMetal, V69BMetalShade);
            p.Fill(Sd.Circle(-0.025f, 0.025f, 0.02f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("v69_cp_nail", p);
        }

        /// <summary>Filo del quadro, nelle coordinate di "heist_painting": dagli angoli in alto fino al chiodo in (0, 1.85).</summary>
        public static DoodleArt V69B_PictureWire()
        {
            var p = new DoodlePainter(-1.35f, 1.0f, 1.35f, 1.95f, 100) { Ink = 0.03f };
            p.InkLine(0.035f, -1.15f, 1.15f, 0f, 1.85f, 1.15f, 1.15f);
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Circle(s * 1.15f, 1.15f, 0.06f), V69BMetal, 0.025f);
            return new DoodleArt("v69_cp_picture_wire", p);
        }

        /// <summary>Occhiolino del gatto nobile di "heist_painting": la palpebra chiusa sull'occhio di sinistra (senza monocolo).</summary>
        public static DoodleArt V69B_Wink()
        {
            var p = new DoodlePainter(-0.38f, 0.04f, -0.06f, 0.3f, 200) { Ink = 0.02f, Wobble = 0.002f };
            p.Fill(Sd.Ellipse(-0.22f, 0.17f, 0.125f, 0.095f), Rgba.Hex("9AA3B5"), false);
            p.InkLine(0.028f, -0.32f, 0.15f, -0.22f, 0.2f, -0.12f, 0.15f);
            p.InkLine(0.018f, -0.3f, 0.12f, -0.34f, 0.09f);
            return new DoodleArt("v69_cp_wink", p);
        }

        // il bicchiere: fondo in y 0, interno largo 0.36 in basso e 0.46 in alto (y 0.06 .. 0.78)
        static Sdf V69BGlassInside() => Sd.Polygon(-0.18f, 0.06f, 0.18f, 0.06f, 0.23f, 0.78f, -0.23f, 0.78f);
        static Sdf V69BGlassOutside() => Sd.Polygon(-0.22f, 0f, 0.22f, 0f, 0.28f, 0.82f, -0.28f, 0.82f);

        /// <summary>Il bicchiere vuoto (parte dietro: vetro azzurrino trasparente, sotto l'acqua).</summary>
        public static DoodleArt V69B_GlassBack()
        {
            var p = new DoodlePainter(-0.32f, -0.04f, 0.32f, 0.86f, 200) { Ink = 0.03f };
            p.Fill(V69BGlassOutside(), Rgba.Hex("E6F4FB", 0.55f));
            return new DoodleArt("v69_cp_glass_back", p);
        }

        /// <summary>Il bicchiere (parte davanti: contorno, fondo spesso e riflessi, sopra l'acqua).</summary>
        public static DoodleArt V69B_GlassFront()
        {
            var p = new DoodlePainter(-0.32f, -0.04f, 0.32f, 0.86f, 200) { Ink = 0.03f };
            p.Shape(Sd.Subtract(V69BGlassOutside(), V69BGlassInside()), Rgba.Hex("D5ECF7", 0.85f));
            p.Shape(V69BGlassOutside(), Rgba.Clear, 0.032f);
            p.Line(Rgba.White.WithAlpha(0.85f), 0.03f, -0.15f, 0.14f, -0.19f, 0.68f);
            p.Line(Rgba.White.WithAlpha(0.6f), 0.02f, -0.09f, 0.14f, -0.11f, 0.4f);
            return new DoodleArt("v69_cp_glass_front", p);
        }

        /// <summary>Maschera dell'interno del bicchiere (per l'acqua).</summary>
        public static DoodleArt V69B_GlassMask()
        {
            var p = new DoodlePainter(-0.32f, -0.04f, 0.32f, 0.86f, 200) { Ink = 0.03f };
            p.Fill(V69BGlassInside(), Rgba.White, false);
            return new DoodleArt("v69_cp_glass_mask", p);
        }

        /// <summary>Livella a bolla appoggiata (fondo in y 0, lunga 1.2): fiala verde al centro con le due tacche.</summary>
        public static DoodleArt V69B_SpiritLevel()
        {
            var p = new DoodlePainter(-0.64f, -0.04f, 0.64f, 0.27f, 160) { Ink = 0.035f };
            Body(p, Sd.Box(0f, 0.115f, 0.6f, 0.11f, 0.03f), Rgba.Hex("F2C230"), Rgba.Hex("D19D1A"));
            foreach (float x in new[] { -0.46f, 0.46f }) p.Shape(Sd.Box(x, 0.115f, 0.07f, 0.045f, 0.02f), Rgba.Hex("D19D1A"), 0.02f);
            p.Shape(Sd.Box(0f, 0.115f, 0.24f, 0.06f, 0.05f), Rgba.Hex("BFEA8C"), 0.025f);
            p.Fill(Sd.Box(0f, 0.15f, 0.2f, 0.015f, 0.01f), Rgba.White.WithAlpha(0.45f));
            foreach (float x in new[] { -0.07f, 0.07f }) p.InkLine(0.018f, x, 0.07f, x, 0.16f);
            return new DoodleArt("v69_cp_spirit_level", p);
        }

        /// <summary>La bolla d'aria della livella (centro 0,0).</summary>
        public static DoodleArt V69B_LevelBubble()
        {
            var p = new DoodlePainter(-0.07f, -0.05f, 0.07f, 0.05f, 300) { Ink = 0.012f, Wobble = 0.001f };
            p.Shape(Sd.Ellipse(0f, 0f, 0.052f, 0.032f), Rgba.Hex("F4FFE8"), 0.012f);
            p.Fill(Sd.Ellipse(-0.015f, 0.01f, 0.018f, 0.009f), Rgba.White);
            return new DoodleArt("v69_cp_level_bubble", p);
        }

        /// <summary>Rotolo di nastro adesivo appoggiato in piedi (fondo in y 0).</summary>
        public static DoodleArt V69B_TapeRoll()
        {
            var p = new DoodlePainter(-0.3f, -0.04f, 0.42f, 0.56f, 160) { Ink = 0.035f };
            Sdf ring = Sd.Subtract(Sd.Circle(0f, 0.26f, 0.25f), Sd.Circle(0f, 0.26f, 0.12f));
            Body(p, ring, Rgba.Hex("F3E2A6"), Rgba.Hex("D8C27D"));
            p.Line(Rgba.Hex("C9B06A"), 0.015f, 0f, 0.42f, 0.13f, 0.4f);
            // il lembo che pende
            p.Shape(Sd.Polygon(0.2f, 0.16f, 0.36f, 0.06f, 0.38f, 0.1f, 0.24f, 0.22f), Rgba.Hex("F7EBC0", 0.9f), 0.02f);
            return new DoodleArt("v69_cp_tape_roll", p);
        }

        /// <summary>Pezzo di nastro adesivo (centro 0,0, lungo 0.5) con i bordi seghettati.</summary>
        public static DoodleArt V69B_TapeStrip()
        {
            var p = new DoodlePainter(-0.28f, -0.1f, 0.28f, 0.1f, 200) { Ink = 0.015f };
            Sdf strip = Sd.Polygon(-0.25f, -0.07f, -0.23f, -0.03f, -0.26f, 0.01f, -0.24f, 0.07f, 0.25f, 0.07f, 0.23f, 0.03f, 0.26f, -0.01f, 0.24f, -0.07f);
            p.Shape(strip, Rgba.Hex("F3E2A6", 0.85f), 0.015f);
            p.Fill(Sd.Box(0f, 0.035f, 0.2f, 0.012f), Rgba.White.WithAlpha(0.5f));
            return new DoodleArt("v69_cp_tape_strip", p);
        }

        /// <summary>Martello sdraiato (manico verso sinistra, testa a destra; centro del manico in 0,0).</summary>
        public static DoodleArt V69B_Hammer()
        {
            var p = new DoodlePainter(-0.5f, -0.2f, 0.56f, 0.22f, 160) { Ink = 0.035f };
            Body(p, Sd.Box(-0.06f, 0f, 0.42f, 0.055f, 0.04f), Rgba.Hex("C98B5A"), Rgba.Hex("A86E48"));
            p.Shape(Sd.Box(-0.34f, 0f, 0.12f, 0.065f, 0.04f), Rgba.Hex("D9463F"), 0.03f);
            Sdf head = Sd.Union(Sd.Box(0.4f, 0.02f, 0.07f, 0.17f, 0.03f), Sd.Polygon(0.4f, -0.12f, 0.53f, -0.17f, 0.47f, -0.08f));
            Body(p, head, V69BMetal, V69BMetalShade);
            return new DoodleArt("v69_cp_hammer", p);
        }
    }
}
