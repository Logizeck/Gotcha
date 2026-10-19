using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli della specifica 105-200, pacchetto C.</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV41C() => new DoodleArt[]
        {
            // One too many
            S41C_FarmBackdrop(), S41C_Coop(), S41C_Puddle(), S41C_PuddleReflection(), S41C_Feather(),
            // Reverse parking
            S41C_LotBackdrop(), S41C_Car(), S41C_Wheel(), S41C_Pedal(),
            // The stubborn door
            S41C_HallBackdrop(), S41C_BigDoor(), S41C_Padlock(false), S41C_Padlock(true), S41C_Crack(),
            // Zero gravity dinner
            S41C_StationBackdrop(), S41C_Bowl(), S41C_BowlBack(), S41C_Meatball(),
            // The forgotten password
            S41C_PcBackdrop(), S41C_Monitor(), S41C_KeyCap(),
            // The shrinking room
            S41C_RoomFloor(), S41C_BrickWall(true), S41C_BrickWall(false), S41C_ExitDoor(),
            // Paperwork nightmare
            S41C_OfficeBackdrop(), S41C_Desk(), S41C_Sheet(), S41C_FancyBin(), S41C_Worker(),
            // Battery vampire
            S41C_VampBackdrop(), S41C_BatteryShell(), S41C_Symbol(true), S41C_Symbol(false), S41C_Vampire(false), S41C_Vampire(true),
        };

        static readonly Rgba S41C_Ink = DoodlePainter.DefaultInk;
        static float S41C_Sin(float v) => (float)Math.Sin(v);
        static float S41C_Cos(float v) => (float)Math.Cos(v);

        /// <summary>Nuvoletta bianca dello sfondo.</summary>
        static void S41C_Cloud(DoodlePainter p, float x, float y, float s)
        {
            p.Fill(Sd.Union(Sd.Circle(x, y, 0.32f * s), Sd.Circle(x + 0.34f * s, y + 0.08f * s, 0.27f * s), Sd.Circle(x - 0.34f * s, y - 0.04f * s, 0.24f * s),
                            Sd.Box(x, y - 0.12f * s, 0.55f * s, 0.14f * s, 0.12f * s)), Rgba.White.WithAlpha(0.9f));
        }

        // ---------- One too many ----------

        /// <summary>Aia: cielo, colline, staccionata e prato (coordinate del livello, orizzonte in y 0.35).</summary>
        public static DoodleArt S41C_FarmBackdrop()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 4f, 6f), (x, y) => Rgba.Lerp(Rgba.Hex("BFE8FF"), Rgba.Hex("7CC4F0"), Clamp((y - 0.3f) / 4.5f)));
            S41C_Cloud(p, -1.8f, 3.9f, 1.1f);
            S41C_Cloud(p, 1.9f, 4.4f, 0.8f);
            p.Shape(Sd.Union(Sd.Circle(-2.4f, -0.6f, 1.9f), Sd.Circle(1.2f, -1.2f, 2.4f)), Rgba.Hex("9ED36B"));
            p.FillFunc(Sd.Box(0, -3.0f, 4f, 3.35f), (x, y) => Rgba.Lerp(Rgba.Hex("8CCB5A"), Rgba.Hex("6DB143"), Clamp(-y / 5f)));
            p.Line(S41C_Ink.WithAlpha(0.6f), 0.05f, -3.3f, 0.35f, 3.3f, 0.35f);
            // staccionata
            for (float x = -3.0f; x <= 3.1f; x += 0.55f)
                p.Shape(Sd.Polygon(x - 0.1f, 0.15f, x + 0.1f, 0.15f, x + 0.1f, 0.95f, x, 1.08f, x - 0.1f, 0.95f), Rgba.Hex("F6E7C8"), 0.035f);
            p.Shape(Sd.Box(0, 0.75f, 3.4f, 0.06f), Rgba.Hex("E9D5AE"), 0.03f);
            p.Shape(Sd.Box(0, 0.42f, 3.4f, 0.06f), Rgba.Hex("E9D5AE"), 0.03f);
            // ciuffi d'erba e terra battuta
            p.Fill(Sd.Ellipse(-0.2f, -2.6f, 2.5f, 1.2f), Rgba.Hex("C9A46B", 0.55f));
            var rnd = new Random(41);
            for (int i = 0; i < 40; i++)
            {
                float x = (float)rnd.NextDouble() * 6f - 3f, y = -(float)rnd.NextDouble() * 4.8f;
                p.Line(Rgba.Hex("4E9433"), 0.03f, x - 0.06f, y + 0.12f, x, y, x + 0.06f, y + 0.14f);
            }
            return new DoodleArt("s41c_farm_bg", p);
        }

        /// <summary>Pollaio rosso: cinque finestrelle-nido in alto (x -0.9..0.9 ogni 0.45, y 0.25), porta in (0,-0.55), base in y -1.05.</summary>
        public static DoodleArt S41C_Coop()
        {
            var p = new DoodlePainter(-1.45f, -1.35f, 1.45f, 1.75f, 110) { Ink = 0.05f };
            Rgba red = Rgba.Hex("D9534A"), redShade = Rgba.Hex("B23A33");
            p.Shape(Sd.Box(0, -0.15f, 1.18f, 0.9f, 0.04f), red);
            for (float y = -0.9f; y < 0.7f; y += 0.22f) p.Line(redShade, 0.025f, -1.1f, y, 1.1f, y);
            p.Shape(Sd.Polygon(-1.4f, 0.68f, 1.4f, 0.68f, 0f, 1.65f), Rgba.Hex("7A4A2E"));
            p.Shape(Sd.Polygon(-1.0f, 0.82f, 1.0f, 0.82f, 0f, 1.42f), Rgba.Hex("F2D59A"), 0.03f);
            for (int i = 0; i < 5; i++)
                p.Shape(Sd.Circle(-0.9f + i * 0.45f, 0.25f, 0.18f), Rgba.Hex("3B2A28"), 0.04f);
            p.Shape(Sd.Union(Sd.Box(0, -0.7f, 0.3f, 0.32f), Sd.Circle(0, -0.38f, 0.3f)), Rgba.Hex("3B2A28"));
            // rampa
            p.Shape(Sd.Polygon(-0.3f, -1.02f, 0.3f, -1.02f, 0.42f, -1.3f, -0.42f, -1.3f), Rgba.Hex("C98A4B"), 0.035f);
            foreach (float y in new[] { -1.1f, -1.2f }) p.InkLine(0.02f, -0.33f, y, 0.33f, y);
            // cartello bianco sopra la porta (il testo è un'etichetta)
            p.Shape(Sd.Box(0, -0.12f, 0.62f, 0.12f, 0.03f), Rgba.Hex("FFF8E8"), 0.03f);
            return new DoodleArt("s41c_coop", p);
        }

        /// <summary>Pozzanghera (ellisse 1.25 x 0.55, centro 0,0).</summary>
        public static DoodleArt S41C_Puddle()
        {
            var p = new DoodlePainter(-1.35f, -0.65f, 1.35f, 0.65f, 120) { Ink = 0.04f };
            Sdf e = Sd.Ellipse(0, 0, 1.25f, 0.55f);
            p.FillFunc(e, (x, y) => Rgba.Lerp(Rgba.Hex("9AD3F5"), Rgba.Hex("5FA8DB"), Clamp((0.4f - y) / 0.8f)));
            p.Shape(e, Rgba.Clear);
            p.Line(Rgba.White.WithAlpha(0.7f), 0.035f, -1.0f, 0.1f, -0.75f, 0.22f);
            p.Line(Rgba.White.WithAlpha(0.6f), 0.03f, 0.7f, -0.3f, 1.0f, -0.16f);
            return new DoodleArt("s41c_puddle", p);
        }

        /// <summary>Riflesso capovolto della gallina dentro la pozzanghera (stessa ellisse; i piedi della gallina in y 0.48).</summary>
        public static DoodleArt S41C_PuddleReflection()
        {
            DoodlePainter hen = MotherHen().Painter;
            var p = new DoodlePainter(-1.35f, -0.65f, 1.35f, 0.65f, 120) { Ink = 0.04f };
            Sdf inner = Sd.Ellipse(0, 0, 1.19f, 0.5f);
            const float sx = 0.95f, sy = 0.7f, feet = 0.48f, srcFeet = -0.6f;
            Rgba water = Rgba.Hex("6FB6E6");
            p.FillFunc(inner, (x, y) =>
            {
                float hx = x / sx, hy = srcFeet + (feet - y) / sy;
                int i = (int)Math.Floor((hx - hen.XMin) * hen.Ppu), j = (int)Math.Floor((hy - hen.YMin) * hen.Ppu);
                if (i < 0 || j < 0 || i >= hen.Width || j >= hen.Height) return Rgba.Clear;
                Rgba c = hen.Pixels[j * hen.Width + i];
                if (c.a <= 0f) return Rgba.Clear;
                // increspatura: righine più chiare
                float ripple = (float)Math.Abs(Math.Sin(y * 40f)) < 0.12f ? 0.25f : 0f;
                Rgba m = Rgba.Lerp(c, water, 0.22f + ripple);
                return m.WithAlpha(c.a * 0.85f);
            });
            return new DoodleArt("s41c_puddle_reflection", p);
        }

        /// <summary>Piumetta bianca (centro 0,0, lunga 0.4).</summary>
        public static DoodleArt S41C_Feather()
        {
            var p = new DoodlePainter(-0.25f, -0.25f, 0.25f, 0.25f, 160) { Ink = 0.025f };
            Sdf f = Sd.Rotate(Sd.Ellipse(0, 0, 0.08f, 0.2f), 0, 0, 30f);
            p.Shape(f, Rgba.Hex("FFFDF6"));
            p.Line(Rgba.Hex("DAD3C4"), 0.02f, 0.1f, -0.17f, -0.1f, 0.17f);
            return new DoodleArt("s41c_feather", p);
        }

        // ---------- Reverse parking ----------

        /// <summary>
        /// Parcheggio visto dall'alto (coordinate del livello): strada x -1.6..1.05, corsia di sosta x 1.05..2.45,
        /// marciapiede oltre; posto libero x 1.05..2.45, y -0.95..0.95.
        /// </summary>
        public static DoodleArt S41C_LotBackdrop()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.Fill(Sd.Box(0, 0, 4f, 6f), Rgba.Hex("8CCB5A"), false);
            for (int i = 0; i < 9; i++) p.Fill(Sd.Circle(-2.45f + (i % 2) * 0.25f, -4.5f + i * 1.15f, 0.28f), Rgba.Hex("6DB143"));
            // marciapiedi
            p.Fill(Sd.Box(-1.85f, 0, 0.25f, 6f), Rgba.Hex("C9C4BC"), false);
            p.Fill(Sd.Box(2.85f, 0, 0.4f, 6f), Rgba.Hex("C9C4BC"), false);
            for (float y = -5f; y < 5.3f; y += 0.6f) { p.Line(Rgba.Hex("A8A29A"), 0.025f, -2.1f, y, -1.6f, y); p.Line(Rgba.Hex("A8A29A"), 0.025f, 2.45f, y, 3.25f, y); }
            // asfalto
            p.FillFunc(Sd.Box(0.425f, 0, 2.025f, 6f), (x, y) => Rgba.Lerp(Rgba.Hex("5E5B66"), Rgba.Hex("54515C"), Noise.Value(x * 3f, y * 3f)));
            p.Line(Rgba.Hex("3E3C45"), 0.06f, -1.6f, -6f, -1.6f, 6f);
            p.Line(Rgba.Hex("3E3C45"), 0.06f, 2.45f, -6f, 2.45f, 6f);
            // mezzeria tratteggiata
            for (float y = -5f; y < 5.3f; y += 0.9f) p.Fill(Sd.Box(-0.3f, y, 0.04f, 0.25f), Rgba.Hex("FFF1B5"), false);
            // corsia di sosta: righe bianche
            p.Line(Rgba.White.WithAlpha(0.9f), 0.04f, 1.05f, -6f, 1.05f, 6f);
            foreach (float y in new[] { -3.2f, -0.95f, 0.95f, 3.2f }) p.Line(Rgba.White.WithAlpha(0.9f), 0.04f, 1.05f, y, 2.45f, y);
            // posto libero evidenziato
            p.Fill(Sd.Box(1.75f, 0, 0.62f, 0.88f), Rgba.Hex("FFD54A", 0.18f), false);
            for (float y = -0.85f; y < 0.9f; y += 0.28f) p.Fill(Sd.Box(1.12f, y, 0.025f, 0.08f), Rgba.Hex("FFD54A"), false);
            p.Line(Rgba.Hex("FFD54A"), 0.05f, 2.38f, -0.88f, 2.38f, 0.88f);
            p.Line(Rgba.Hex("FFD54A"), 0.05f, 1.12f, 0.88f, 2.38f, 0.88f);
            p.Line(Rgba.Hex("FFD54A"), 0.05f, 1.12f, -0.88f, 2.38f, -0.88f);
            return new DoodleArt("s41c_lot_bg", p);
        }

        /// <summary>Auto vista dall'alto, muso in su (lunga 1.2, larga 0.62, centro 0,0). Carrozzeria bianca: si tinge.</summary>
        public static DoodleArt S41C_Car()
        {
            var p = new DoodlePainter(-0.42f, -0.68f, 0.42f, 0.68f, 160) { Ink = 0.035f };
            foreach (float sx in new[] { -1f, 1f })
                foreach (float y in new[] { -0.36f, 0.36f })
                    p.Shape(Sd.Box(sx * 0.31f, y, 0.06f, 0.13f, 0.03f), Rgba.Hex("2E2B35"), 0.02f);
            Sdf body = Sd.Box(0, 0, 0.3f, 0.6f, 0.16f);
            Body(p, body, Rgba.White, Rgba.Hex("D8D8DE"));
            p.Shape(Sd.Polygon(-0.24f, 0.3f, 0.24f, 0.3f, 0.2f, 0.08f, -0.2f, 0.08f), Rgba.Hex("BFE6FF"), 0.025f);
            p.Shape(Sd.Polygon(-0.2f, -0.26f, 0.2f, -0.26f, 0.23f, -0.42f, -0.23f, -0.42f), Rgba.Hex("BFE6FF"), 0.025f);
            p.Shape(Sd.Box(0, -0.09f, 0.21f, 0.16f, 0.05f), Rgba.Hex("EEEEF2"), 0.025f);
            foreach (float sx in new[] { -1f, 1f })
            {
                p.Shape(Sd.Ellipse(sx * 0.18f, 0.56f, 0.07f, 0.035f), Rgba.Hex("FFF3B0"), 0.02f);
                p.Shape(Sd.Box(sx * 0.19f, -0.575f, 0.07f, 0.025f, 0.01f), Rgba.Hex("E8443A"), 0.02f);
            }
            return new DoodleArt("s41c_car", p);
        }

        /// <summary>Volante (raggio 0.7, centro 0,0).</summary>
        public static DoodleArt S41C_Wheel()
        {
            var p = new DoodlePainter(-0.78f, -0.78f, 0.78f, 0.78f, 130) { Ink = 0.045f };
            Sdf ring = Sd.Subtract(Sd.Circle(0, 0, 0.7f), Sd.Circle(0, 0, 0.52f));
            Body(p, ring, Rgba.Hex("4A4656"), Rgba.Hex("2E2B35"));
            p.Shape(Sd.Union(Sd.Box(0, 0.02f, 0.56f, 0.07f), Sd.Box(0, -0.3f, 0.07f, 0.3f)), Rgba.Hex("5A5666"), 0.035f);
            p.Shape(Sd.Circle(0, 0, 0.2f), Rgba.Hex("6E6A7C"), 0.035f);
            p.Fill(Sd.Box(0, 0.61f, 0.06f, 0.09f), Rgba.Hex("FFC93C"));
            p.Fill(Sd.Circle(-0.05f, 0.06f, 0.06f), Rgba.White.WithAlpha(0.4f));
            return new DoodleArt("s41c_wheel", p);
        }

        /// <summary>Pedale di metallo (0.8 x 1.0, centro 0,0).</summary>
        public static DoodleArt S41C_Pedal()
        {
            var p = new DoodlePainter(-0.48f, -0.58f, 0.48f, 0.58f, 130) { Ink = 0.045f };
            Sdf plate = Sd.Box(0, 0, 0.4f, 0.5f, 0.12f);
            Body(p, plate, Rgba.Hex("D9DCE3"), Rgba.Hex("AEB2BC"));
            for (float y = -0.36f; y < 0.4f; y += 0.12f) p.Line(Rgba.Hex("8E929C"), 0.03f, -0.28f, y, 0.28f, y);
            return new DoodleArt("s41c_pedal", p);
        }

        // ---------- The stubborn door ----------

        /// <summary>Ingresso: parete a righe, pavimento a scacchi, vano scuro della porta (x -1.15..1.15, y -3.4..1.75).</summary>
        public static DoodleArt S41C_HallBackdrop()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.Fill(Sd.Box(0, 1f, 4f, 5f), Rgba.Hex("F6E3C4"), false);
            for (float x = -3.1f; x < 3.2f; x += 0.4f) p.Fill(Sd.Box(x, 1f, 0.09f, 5f), Rgba.Hex("EFD3AA"), false);
            p.FillFunc(Sd.Box(0, -4.4f, 4f, 1.0f), (x, y) => ((int)Math.Floor(x / 0.5f) + (int)Math.Floor(y / 0.5f)) % 2 == 0 ? Rgba.Hex("D9C3A5") : Rgba.Hex("B89C7A"));
            p.Shape(Sd.Box(0, -3.42f, 3.4f, 0.08f), Rgba.Hex("8E5A35"), 0.04f);
            // stipite e vano
            p.Shape(Sd.Box(0, -0.8f, 1.38f, 2.68f, 0.05f), Rgba.Hex("8E5A35"));
            p.Fill(Sd.Box(0, -0.83f, 1.16f, 2.56f), Rgba.Hex("2D2438"), false);
            // quadretto
            p.Shape(Sd.Box(-2.25f, 0.9f, 0.45f, 0.55f, 0.03f), Rgba.Hex("C98A4B"));
            p.Fill(Sd.Box(-2.25f, 0.9f, 0.33f, 0.43f), Rgba.Hex("BFE6FF"), false);
            p.Fill(Sd.Polygon(-2.58f, 0.47f, -2.3f, 0.95f, -2.12f, 0.7f, -1.92f, 0.47f), Rgba.Hex("6DB143"));
            p.Fill(Sd.Circle(-2.05f, 1.15f, 0.09f), Rgba.Hex("FFC93C"));
            // appendiabiti
            p.Shape(Sd.Box(2.25f, 0.9f, 0.4f, 0.06f, 0.03f), Rgba.Hex("8E5A35"), 0.03f);
            foreach (float x in new[] { 2.0f, 2.5f }) p.InkLine(0.04f, x, 0.85f, x, 0.7f, x + 0.08f, 0.65f);
            return new DoodleArt("s41c_hall_bg", p);
        }

        /// <summary>Portone di legno (x -1.15..1.15, y -2.55..2.55, centro 0,0) con tre staffe a destra (x 0.62; y 1.2, 0, -1.2).</summary>
        public static DoodleArt S41C_BigDoor()
        {
            var p = new DoodlePainter(-1.22f, -2.62f, 1.22f, 2.62f, 90) { Ink = 0.05f };
            Sdf door = Sd.Box(0, 0, 1.14f, 2.54f, 0.04f);
            p.Fill(door, Wood);
            for (float x = -0.86f; x < 1.0f; x += 0.57f) p.Line(WoodShade, 0.04f, x, -2.45f, x, 2.45f);
            p.Shape(door, Rgba.Clear);
            foreach (float y in new[] { 1.85f, -1.85f }) p.Shape(Sd.Box(0, y, 1.0f, 0.12f, 0.03f), Rgba.Hex("7E8290"), 0.035f);
            foreach (float y in new[] { 1.2f, 0f, -1.2f })
            {
                p.Shape(Sd.Box(0.62f, y + 0.22f, 0.36f, 0.07f, 0.03f), Rgba.Hex("9A9EAA"), 0.03f);
                p.Shape(Sd.Box(0.62f, y + 0.22f, 0.07f, 0.12f, 0.03f), Rgba.Hex("BFC3CC"), 0.03f);
            }
            p.Shape(Sd.Circle(-0.85f, 0f, 0.1f), Gold, 0.03f);
            return new DoodleArt("s41c_big_door", p);
        }

        /// <summary>Lucchetto (corpo 0.6 x 0.5 centrato in 0,0, buco della serratura in 0,-0.05; arco sopra). Aperto: arco sollevato.</summary>
        public static DoodleArt S41C_Padlock(bool open)
        {
            var p = new DoodlePainter(-0.42f, -0.36f, 0.62f, open ? 0.9f : 0.62f, 170) { Ink = 0.04f };
            float lift = open ? 0.24f : 0f;
            Sdf shackle = Sd.Subtract(Sd.Union(Sd.Circle(0, 0.25f + lift, 0.21f), Sd.Box(0, 0.12f + lift * 0.5f, 0.21f, 0.13f + lift * 0.5f)),
                                      Sd.Union(Sd.Circle(0, 0.25f + lift, 0.12f), Sd.Box(0, 0.12f + lift * 0.5f, 0.12f, 0.2f + lift * 0.5f)));
            if (open) shackle = Sd.Rotate(shackle, -0.165f, 0.12f, -35f);
            Body(p, shackle, Rgba.Hex("D2D6DE"), Rgba.Hex("9EA3AE"));
            Sdf body = Sd.Box(0, -0.02f, 0.3f, 0.25f, 0.07f);
            Body(p, body, open ? Rgba.Hex("8EE08E") : Gold, open ? Rgba.Hex("5BBF4A") : GoldShade);
            p.Fill(Sd.Union(Sd.Circle(0, 0.0f, 0.06f), Sd.Polygon(-0.035f, -0.02f, 0.035f, -0.02f, 0.05f, -0.15f, -0.05f, -0.15f)), S41C_Ink);
            return new DoodleArt(open ? "s41c_padlock_open" : "s41c_padlock", p);
        }

        /// <summary>Crepa a zig-zag (lunga 0.5, centro 0,0).</summary>
        public static DoodleArt S41C_Crack()
        {
            var p = new DoodlePainter(-0.3f, -0.18f, 0.3f, 0.18f, 200) { Ink = 0.02f };
            p.Line(S41C_Ink, 0.035f, -0.04f, 0.15f, 0.03f, 0.05f, -0.05f, -0.01f, 0.04f, -0.08f, -0.02f, -0.15f);
            p.Line(Rgba.White.WithAlpha(0.8f), 0.015f, -0.02f, 0.14f, 0.05f, 0.05f, -0.03f, -0.01f, 0.06f, -0.08f);
            return new DoodleArt("s41c_crack", p);
        }

        // ---------- Zero gravity dinner ----------

        /// <summary>
        /// Cucina della stazione spaziale (coordinate del livello): arena x -2.75..2.75, y -4.2..2.55; boccaporti aperti
        /// a sinistra (y 0.5..1.5) e a destra (y -2.4..-1.4); erogatore di polpette in alto al centro (0, 2.35).
        /// </summary>
        public static DoodleArt S41C_StationBackdrop()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 4f, 6f), (x, y) => Rgba.Lerp(Rgba.Hex("DDE3EE"), Rgba.Hex("C3CCDC"), Clamp((y + 4f) / 8f)));
            for (float y = -4.2f; y < 3f; y += 1.2f) p.Line(Rgba.Hex("AEB8CA"), 0.035f, -2.75f, y, 2.75f, y);
            for (float x = -2.2f; x < 2.5f; x += 1.1f) p.Line(Rgba.Hex("AEB8CA"), 0.035f, x, -4.2f, x, 2.6f);
            for (float y = -3.6f; y < 2.6f; y += 1.2f)
                for (float x = -2.0f; x < 2.5f; x += 1.1f) p.Fill(Sd.Circle(x, y, 0.035f), Rgba.Hex("9AA4B8"));
            // pareti spesse
            Rgba wall = Rgba.Hex("7E889C");
            p.Fill(Sd.Box(-3.0f, 0, 0.25f, 6f), wall, false);
            p.Fill(Sd.Box(3.0f, 0, 0.25f, 6f), wall, false);
            p.Fill(Sd.Box(0, -4.7f, 4f, 0.5f), wall, false);
            p.Line(S41C_Ink, 0.05f, -2.75f, -4.2f, -2.75f, 5.2f);
            p.Line(S41C_Ink, 0.05f, 2.75f, -4.2f, 2.75f, 5.2f);
            p.Line(S41C_Ink, 0.05f, -2.75f, -4.2f, 2.75f, -4.2f);
            // boccaporti aperti sullo spazio, con strisce di pericolo
            foreach (var h in new[] { (-3.0f, 1.0f), (3.0f, -1.9f) })
            {
                Sdf hole = Sd.Box(h.Item1, h.Item2, 0.3f, 0.5f);
                p.FillFunc(hole, (x, y) => Noise.Value(x * 40f, y * 40f) > 0.93f ? Rgba.White : Rgba.Hex("141432"));
                for (int k = -1; k <= 1; k += 2)
                {
                    Sdf band = Sd.Box(h.Item1, h.Item2 + k * 0.58f, 0.3f, 0.08f);
                    p.FillFunc(band, (x, y) => ((int)Math.Floor((x + y) * 8f) & 1) == 0 ? Rgba.Hex("FFC93C") : S41C_Ink);
                }
            }
            // oblò con la Terra
            p.Shape(Sd.Circle(-1.4f, -1.0f, 0.5f), Rgba.Hex("9AA4B8"));
            p.Fill(Sd.Circle(-1.4f, -1.0f, 0.38f), Rgba.Hex("141432"));
            p.Fill(Sd.Intersect(Sd.Circle(-1.25f, -1.15f, 0.22f), Sd.Circle(-1.4f, -1.0f, 0.38f)), Rgba.Hex("4FA3E0"));
            p.Fill(Sd.Intersect(Sd.Circle(-1.2f, -1.1f, 0.1f), Sd.Circle(-1.25f, -1.15f, 0.22f)), Rgba.Hex("5BBF4A"));
            p.Shape(Sd.Circle(-1.4f, -1.0f, 0.38f), Rgba.Clear, 0.035f);
            // erogatore di polpette
            p.Shape(Sd.Box(0, 2.62f, 0.55f, 0.3f, 0.08f), Rgba.Hex("E8443A"));
            p.Shape(Sd.Box(0, 2.28f, 0.26f, 0.1f, 0.04f), Rgba.Hex("5A5666"));
            p.Fill(Sd.Circle(-0.3f, 2.66f, 0.06f), Rgba.Hex("FFF3B0"));
            return new DoodleArt("s41c_station_bg", p);
        }

        /// <summary>Ciotola vista di fronte (bocca ellittica in y 0.15, larga 1.3; fondo in y -0.45).</summary>
        public static DoodleArt S41C_Bowl()
        {
            var p = new DoodlePainter(-0.78f, -0.55f, 0.78f, 0.32f, 140) { Ink = 0.045f };
            Sdf body = Sd.Subtract(Sd.Intersect(Sd.Ellipse(0, 0.15f, 0.68f, 0.6f), Sd.Box(0, -0.2f, 0.8f, 0.35f)), Sd.Ellipse(0, 0.15f, 0.62f, 0.12f));
            Body(p, body, Rgba.Hex("FF8FA3"), Rgba.Hex("E0607A"));
            p.Shape(Sd.Box(0, -0.48f, 0.26f, 0.05f, 0.03f), Rgba.Hex("E0607A"), 0.035f);
            p.Shape(Sd.Subtract(Sd.Ellipse(0, 0.15f, 0.68f, 0.17f), Sd.Ellipse(0, 0.15f, 0.62f, 0.12f)), Rgba.Hex("FFC2CE"), 0.03f);
            for (int i = 0; i < 3; i++) p.Fill(Sd.Circle(-0.3f + i * 0.3f, -0.15f, 0.05f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("s41c_bowl", p);
        }

        /// <summary>Interno scuro della ciotola (va dietro le polpette).</summary>
        public static DoodleArt S41C_BowlBack()
        {
            var p = new DoodlePainter(-0.7f, -0.05f, 0.7f, 0.35f, 140) { Ink = 0.03f };
            p.Fill(Sd.Ellipse(0, 0.15f, 0.64f, 0.14f), Rgba.Hex("B8466A"));
            return new DoodleArt("s41c_bowl_back", p);
        }

        /// <summary>Polpetta al sugo (raggio 0.2).</summary>
        public static DoodleArt S41C_Meatball()
        {
            var p = new DoodlePainter(-0.26f, -0.26f, 0.26f, 0.26f, 200) { Ink = 0.035f };
            Body(p, Sd.Circle(0, 0, 0.2f), Rgba.Hex("A0603A"), Rgba.Hex("7A4024"));
            p.Fill(Sd.Union(Sd.Ellipse(0.02f, 0.11f, 0.13f, 0.07f), Sd.Circle(0.12f, 0.05f, 0.05f)), Rgba.Hex("E8443A"));
            p.Fill(Sd.Circle(-0.07f, -0.05f, 0.025f), Rgba.Hex("5BBF4A"));
            p.Fill(Sd.Circle(0.06f, -0.09f, 0.02f), Rgba.Hex("5BBF4A"));
            p.Fill(Sd.Circle(-0.06f, 0.12f, 0.03f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("s41c_meatball", p);
        }

        // ---------- The forgotten password ----------

        /// <summary>Ufficio: parete azzurra con finestra, scrivania in basso (piano in y -1.0).</summary>
        public static DoodleArt S41C_PcBackdrop()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.Fill(Sd.Box(0, 1f, 4f, 5f), Rgba.Hex("CFE3F2"), false);
            for (float y = -1f; y < 5.3f; y += 0.5f) p.Line(Rgba.Hex("BDD4E8"), 0.03f, -3.2f, y, 3.2f, y);
            p.Shape(Sd.Box(-2.3f, 3.3f, 0.6f, 0.55f, 0.03f), Rgba.Hex("BFE6FF"));
            p.InkLine(0.04f, -2.3f, 2.75f, -2.3f, 3.85f);
            p.Shape(Sd.Box(0, -1.15f, 3.4f, 0.18f, 0.03f), Rgba.Hex("C98A4B"));
            p.Fill(Sd.Box(0, -3.3f, 4f, 2.0f), Rgba.Hex("A8743F"), false);
            for (float x = -2.8f; x < 3f; x += 1.4f) p.Line(Rgba.Hex("8E5A35"), 0.04f, x, -5.2f, x, -1.33f);
            // tazza
            p.Shape(Sd.Box(2.45f, -0.72f, 0.2f, 0.25f, 0.04f), Rgba.White);
            p.Shape(Sd.Subtract(Sd.Circle(2.7f, -0.7f, 0.12f), Sd.Circle(2.7f, -0.7f, 0.06f)), Rgba.White, 0.03f);
            return new DoodleArt("s41c_pc_bg", p);
        }

        /// <summary>Monitor a tubo color crema (schermo x -1.2..1.2, y -0.7..0.95; base in y -1.5).</summary>
        public static DoodleArt S41C_Monitor()
        {
            var p = new DoodlePainter(-1.6f, -1.6f, 1.6f, 1.4f, 90) { Ink = 0.05f };
            p.Shape(Sd.Polygon(-0.55f, -1.25f, 0.55f, -1.25f, 0.8f, -1.52f, -0.8f, -1.52f), Rgba.Hex("E6DCC4"));
            Body(p, Sd.Box(0, 0.1f, 1.5f, 1.25f, 0.18f), Rgba.Hex("F2E9D3"), Rgba.Hex("D8CBA9"));
            p.FillFunc(Sd.Box(0, 0.12f, 1.22f, 0.85f, 0.12f), (x, y) => Rgba.Lerp(Rgba.Hex("2E5FA8"), Rgba.Hex("1C3F7A"), Clamp((0.9f - y) / 1.8f)));
            p.Shape(Sd.Box(0, 0.12f, 1.22f, 0.85f, 0.12f), Rgba.Clear, 0.04f);
            p.Fill(Sd.Polygon(-1.1f, 0.9f, -0.6f, 0.9f, -1.1f, 0.3f), Rgba.White.WithAlpha(0.12f));
            p.Fill(Sd.Circle(1.25f, -0.95f, 0.06f), Rgba.Hex("5BBF4A"));
            return new DoodleArt("s41c_monitor", p);
        }

        /// <summary>Tasto quadrato (0.62 x 0.56, centro 0,0).</summary>
        public static DoodleArt S41C_KeyCap()
        {
            var p = new DoodlePainter(-0.36f, -0.34f, 0.36f, 0.34f, 150) { Ink = 0.035f };
            p.Shape(Sd.Box(0, -0.04f, 0.31f, 0.26f, 0.08f), Rgba.Hex("A8A090"));
            p.Shape(Sd.Box(0, 0.02f, 0.28f, 0.22f, 0.07f), Rgba.Hex("F2E9D3"));
            return new DoodleArt("s41c_keycap", p);
        }

        // ---------- The shrinking room ----------

        /// <summary>Pavimento di parquet della stanza (x -4.6..4.6, y -6.2..6.2, centro 0,0).</summary>
        public static DoodleArt S41C_RoomFloor()
        {
            var p = new DoodlePainter(-4.6f, -6.2f, 4.6f, 6.2f, 36) { Ink = 0.06f, Wobble = 0f };
            p.FillFunc(Sd.Box(0, 0, 4.6f, 6.2f), (x, y) =>
            {
                int row = (int)Math.Floor(y / 0.45f);
                float off = (row % 3) * 0.6f;
                int plank = (int)Math.Floor((x + off) / 1.6f);
                float shade = Noise.Value(plank * 3.1f, row * 1.7f) * 0.18f;
                return Rgba.Lerp(Rgba.Hex("E2B07A"), Rgba.Hex("B8814A"), shade + 0.2f);
            });
            for (float y = -6.2f; y < 6.3f; y += 0.45f) p.Line(Rgba.Hex("9A6A3A", 0.6f), 0.025f, -4.6f, y, 4.6f, y);
            // tappeto tondo al centro
            p.Shape(Sd.Ellipse(0, 0, 1.2f, 0.9f), Rgba.Hex("E87D9B"));
            p.Shape(Sd.Ellipse(0, 0, 0.85f, 0.6f), Rgba.Hex("FFC2CE"), 0.03f);
            return new DoodleArt("s41c_room_floor", p);
        }

        /// <summary>Muro di mattoni spesso 0.5: verticale lungo 13 o orizzontale lungo 10 (centro 0,0).</summary>
        public static DoodleArt S41C_BrickWall(bool vertical)
        {
            float hx = vertical ? 0.25f : 5f, hy = vertical ? 6.5f : 0.25f;
            var p = new DoodlePainter(-hx - 0.05f, -hy - 0.05f, hx + 0.05f, hy + 0.05f, 40) { Ink = 0.05f, Wobble = 0.004f };
            Sdf wall = Sd.Box(0, 0, hx, hy);
            p.FillFunc(wall, (x, y) =>
            {
                float u = vertical ? y : x, v = vertical ? x : y;
                int row = (int)Math.Floor((v + 0.25f) / 0.25f);
                float bu = u + (row % 2) * 0.2f;
                float fu = bu / 0.4f - (float)Math.Floor(bu / 0.4f), fv = (v + 0.25f) / 0.25f - row;
                bool mortar = fu < 0.08f || fv < 0.12f;
                return mortar ? Rgba.Hex("D9CFC0") : Rgba.Lerp(Rgba.Hex("C9573F"), Rgba.Hex("A9432F"), Noise.Value((float)Math.Floor(bu / 0.4f) * 2.3f, row * 1.9f));
            });
            p.Shape(wall, Rgba.Clear);
            return new DoodleArt(vertical ? "s41c_wall_v" : "s41c_wall_h", p);
        }

        /// <summary>Uscita vista dall'alto: soglia verde con freccia verso destra (0.6 x 1.2, centro 0,0).</summary>
        public static DoodleArt S41C_ExitDoor()
        {
            var p = new DoodlePainter(-0.36f, -0.68f, 0.36f, 0.68f, 120) { Ink = 0.04f };
            p.Shape(Sd.Box(0, 0, 0.3f, 0.62f, 0.05f), Rgba.Hex("5BBF4A"));
            p.Fill(Sd.Box(0, 0, 0.22f, 0.54f, 0.03f), Rgba.Hex("8EE08E"));
            p.Shape(Sd.Polygon(-0.15f, 0.08f, 0.02f, 0.08f, 0.02f, 0.2f, 0.2f, 0f, 0.02f, -0.2f, 0.02f, -0.08f, -0.15f, -0.08f), Rgba.White, 0.03f);
            return new DoodleArt("s41c_exit_door", p);
        }

        // ---------- Paperwork nightmare ----------

        /// <summary>Ufficio grigio-verde con finestra e orologio (pavimento sotto y -2.6).</summary>
        public static DoodleArt S41C_OfficeBackdrop()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.Fill(Sd.Box(0, 1f, 4f, 5f), Rgba.Hex("DCE8D5"), false);
            for (float x = -3.1f; x < 3.2f; x += 0.6f) p.Line(Rgba.Hex("C9DABF"), 0.04f, x, -2.6f, x, 5.2f);
            p.Fill(Sd.Box(0, -4.0f, 4f, 1.4f), Rgba.Hex("8A8C9A"), false);
            p.Shape(Sd.Box(0, -2.6f, 3.4f, 0.07f), Rgba.Hex("6B6D7A"), 0.035f);
            p.Shape(Sd.Box(1.7f, 2.0f, 0.75f, 0.55f, 0.03f), Rgba.Hex("BFE6FF"));
            p.InkLine(0.04f, 1.7f, 1.45f, 1.7f, 2.55f);
            p.InkLine(0.04f, 0.95f, 2.0f, 2.45f, 2.0f);
            p.Shape(Sd.Circle(-0.2f, 2.1f, 0.35f), Rgba.White);
            p.InkLine(0.04f, -0.2f, 2.1f, -0.2f, 2.35f);
            p.InkLine(0.04f, -0.2f, 2.1f, -0.02f, 2.0f);
            // pianta in vaso a destra
            p.Shape(Sd.Polygon(2.3f, -2.6f, 2.8f, -2.6f, 2.9f, -1.9f, 2.2f, -1.9f), Rgba.Hex("C98A4B"));
            foreach (float a in new[] { -30f, 0f, 30f })
                p.Shape(Sd.Rotate(Sd.Ellipse(2.55f, -1.45f, 0.12f, 0.42f), 2.55f, -1.85f, a), Leaf, 0.035f);
            return new DoodleArt("s41c_office_bg", p);
        }

        /// <summary>
        /// Scrivania (piano x -2.75..2.75, y -1.85..0.55 visto un po' dall'alto, centro 0,0); sul piano c'è il contratto
        /// stampato (rettangolo in 0.35,-0.65, 1.1 x 1.4) che sta sotto la pila di fogli.
        /// </summary>
        public static DoodleArt S41C_Desk()
        {
            var p = new DoodlePainter(-2.9f, -3.3f, 2.9f, 0.7f, 70) { Ink = 0.05f };
            foreach (float x in new[] { -2.4f, 2.4f }) p.Shape(Sd.Box(x, -2.5f, 0.18f, 0.75f), WoodShade);
            p.Shape(Sd.Box(0, -2.0f, 2.8f, 0.18f, 0.03f), WoodDark);
            p.Shape(Sd.Polygon(-2.6f, 0.55f, 2.6f, 0.55f, 2.8f, -1.85f, -2.8f, -1.85f), Wood);
            for (float y = -1.5f; y < 0.5f; y += 0.45f) p.Line(WoodShade.WithAlpha(0.6f), 0.025f, -2.6f, y, 2.6f, y);
            // contratto stampato sul piano
            float cx = 0.35f, cy = -0.65f;
            p.Shape(Sd.Box(cx, cy, 0.55f, 0.7f, 0.02f), Rgba.Hex("FFF8E8"), 0.035f);
            for (int i = 0; i < 6; i++) p.Line(Rgba.Hex("9A9AA8"), 0.03f, cx - 0.4f, cy + 0.15f - i * 0.12f, cx + (i == 5 ? 0.05f : 0.4f), cy + 0.15f - i * 0.12f);
            p.Shape(Sd.Circle(cx + 0.28f, cy - 0.45f, 0.14f), Rgba.Hex("E8443A", 0.85f), 0.025f);
            p.Line(Rgba.Hex("2E5FA8"), 0.025f, cx - 0.4f, cy - 0.5f, cx - 0.25f, cy - 0.42f, cx - 0.1f, cy - 0.52f, cx + 0.05f, cy - 0.44f);
            return new DoodleArt("s41c_desk", p);
        }

        /// <summary>Foglio con righe di testo (0.9 x 1.15, centro 0,0). Bianco: si tinge.</summary>
        public static DoodleArt S41C_Sheet()
        {
            var p = new DoodlePainter(-0.5f, -0.62f, 0.5f, 0.62f, 120) { Ink = 0.03f };
            p.Shape(Sd.Box(0, 0, 0.44f, 0.56f, 0.02f), Rgba.White);
            p.Shape(Sd.Polygon(0.24f, 0.56f, 0.44f, 0.56f, 0.44f, 0.36f), Rgba.Hex("E6E6EE"), 0.02f);
            for (int i = 0; i < 7; i++) p.Line(Rgba.Hex("B0B0C0"), 0.03f, -0.32f, 0.36f - i * 0.12f, i % 3 == 2 ? 0.05f : 0.3f, 0.36f - i * 0.12f);
            return new DoodleArt("s41c_sheet", p);
        }

        /// <summary>Cestino "decorativo" a fiori con fiocco (bocca in y 0.5, larga 1.1; base in y -0.55).</summary>
        public static DoodleArt S41C_FancyBin()
        {
            var p = new DoodlePainter(-0.7f, -0.65f, 0.7f, 0.75f, 120) { Ink = 0.045f };
            Sdf body = Sd.Polygon(-0.55f, 0.5f, 0.55f, 0.5f, 0.4f, -0.55f, -0.4f, -0.55f);
            Body(p, body, Rgba.Hex("B9E3F5"), Rgba.Hex("8CC4E0"));
            foreach (var f in new[] { (-0.22f, 0.05f, "FF8FA3"), (0.2f, -0.15f, "FFC93C"), (0.05f, 0.28f, "C49BF2") })
            {
                for (int k = 0; k < 5; k++)
                {
                    float a = k / 5f * 6.283f;
                    p.Fill(Sd.Circle(f.Item1 + S41C_Cos(a) * 0.08f, f.Item2 + S41C_Sin(a) * 0.08f, 0.06f), Rgba.Hex(f.Item3));
                }
                p.Fill(Sd.Circle(f.Item1, f.Item2, 0.04f), Rgba.White);
            }
            p.Shape(Sd.Ellipse(0, 0.5f, 0.58f, 0.12f), Rgba.Hex("6FA9C8"), 0.04f);
            // fiocco
            p.Shape(Sd.Union(Sd.Polygon(-0.05f, -0.3f, -0.3f, -0.18f, -0.3f, -0.42f), Sd.Polygon(0.05f, -0.3f, 0.3f, -0.18f, 0.3f, -0.42f)), Rgba.Hex("E8443A"), 0.03f);
            p.Shape(Sd.Circle(0, -0.3f, 0.06f), Rgba.Hex("E8443A"), 0.03f);
            return new DoodleArt("s41c_fancy_bin", p);
        }

        /// <summary>Impiegato disperato con cravatta (busto; centro della testa in 0,0.45; spalle in y -0.4).</summary>
        public static DoodleArt S41C_Worker()
        {
            var p = new DoodlePainter(-0.75f, -0.85f, 0.75f, 1.05f, 130) { Ink = 0.045f };
            Body(p, Sd.Box(0, -0.45f, 0.62f, 0.42f, 0.25f), Rgba.Hex("F4F4FA"), Rgba.Hex("D2D2DE"));
            p.Shape(Sd.Polygon(-0.07f, -0.08f, 0.07f, -0.08f, 0.1f, -0.6f, 0f, -0.75f, -0.1f, -0.6f), Rgba.Hex("E8443A"), 0.03f);
            p.Shape(Sd.Polygon(-0.2f, -0.02f, 0f, -0.12f, 0.2f, -0.02f, 0.14f, 0.05f, -0.14f, 0.05f), Rgba.White, 0.03f);
            Body(p, Sd.Ellipse(0, 0.45f, 0.38f, 0.42f), Skin, SkinShade);
            p.Shape(Sd.Union(Sd.Circle(-0.22f, 0.8f, 0.14f), Sd.Circle(0.0f, 0.86f, 0.15f), Sd.Circle(0.22f, 0.8f, 0.14f)), Rgba.Hex("6B4A2E"), 0.035f);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Circle(s * 0.14f, 0.5f, 0.1f), Rgba.White, 0.03f);
                p.Fill(Sd.Circle(s * 0.14f, 0.48f, 0.04f), S41C_Ink);
                p.InkLine(0.03f, s * 0.05f, 0.66f, s * 0.24f, 0.64f);
                p.Shape(Sd.Circle(s * 0.14f, 0.5f, 0.13f), Rgba.Clear, 0.025f);
            }
            p.InkLine(0.03f, -0.04f, 0.5f, 0.04f, 0.5f);
            p.Shape(Sd.Ellipse(0, 0.22f, 0.1f, 0.07f), Rgba.Hex("8A3A3A"), 0.03f);
            // goccia di sudore
            p.Shape(Sd.Union(Sd.Circle(0.36f, 0.62f, 0.05f), Sd.Polygon(0.32f, 0.64f, 0.4f, 0.64f, 0.36f, 0.76f)), Rgba.Hex("9AD3F5"), 0.02f);
            return new DoodleArt("s41c_worker", p);
        }

        // ---------- Battery vampire ----------

        /// <summary>Laboratorio di notte: parete viola, finestra con la luna, pavimento in y -3.0.</summary>
        public static DoodleArt S41C_VampBackdrop()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 1f, 4f, 5f), (x, y) => Rgba.Lerp(Rgba.Hex("4B3A6E"), Rgba.Hex("2E2448"), Clamp((y + 2f) / 7f)));
            for (float x = -3.0f; x < 3.2f; x += 0.75f) p.Line(Rgba.Hex("40325F"), 0.05f, x, -3f, x, 5.2f);
            p.Fill(Sd.Box(0, -4.2f, 4f, 1.2f), Rgba.Hex("3A3046"), false);
            p.Shape(Sd.Box(0, -3.0f, 3.4f, 0.07f), Rgba.Hex("2A2234"), 0.035f);
            p.Shape(Sd.Box(-1.6f, 2.0f, 0.75f, 0.65f, 0.35f), Rgba.Hex("1C1838"));
            p.Fill(Sd.Subtract(Sd.Circle(-1.45f, 2.15f, 0.3f), Sd.Circle(-1.3f, 2.25f, 0.26f)), Rgba.Hex("FFF3B0"));
            for (int i = 0; i < 5; i++) p.Fill(Sd.Circle(-2.1f + i * 0.25f, 1.6f + (i % 2) * 0.35f, 0.025f), Rgba.White);
            return new DoodleArt("s41c_vamp_bg", p);
        }

        /// <summary>
        /// Guscio della batteria orizzontale (interno trasparente x -1.65..1.65, y -0.6..0.6; polo in x 1.95).
        /// La carica è una barra dietro.
        /// </summary>
        public static DoodleArt S41C_BatteryShell()
        {
            var p = new DoodlePainter(-2.0f, -0.9f, 2.25f, 0.9f, 90) { Ink = 0.05f };
            Sdf outer = Sd.Box(0, 0, 1.85f, 0.8f, 0.18f), inner = Sd.Box(0, 0, 1.65f, 0.6f, 0.08f);
            Body(p, Sd.Subtract(outer, inner), Rgba.Hex("E2E5EC"), Rgba.Hex("B8BDC8"));
            p.Shape(inner, Rgba.Clear, 0.04f);
            p.Shape(Sd.Box(1.98f, 0, 0.15f, 0.35f, 0.06f), Rgba.Hex("B8BDC8"));
            p.Fill(Sd.Box(-0.6f, 0.45f, 0.9f, 0.05f, 0.04f), Rgba.White.WithAlpha(0.35f));
            return new DoodleArt("s41c_battery_shell", p);
        }

        /// <summary>Medaglione del polo: + rosso o − blu (raggio 0.4).</summary>
        public static DoodleArt S41C_Symbol(bool plus)
        {
            var p = new DoodlePainter(-0.46f, -0.46f, 0.46f, 0.46f, 160) { Ink = 0.04f };
            Body(p, Sd.Circle(0, 0, 0.38f), plus ? Rgba.Hex("E8443A") : Rgba.Hex("3C7FD9"), plus ? Rgba.Hex("B92E2A") : Rgba.Hex("2A5DA8"));
            Sdf sign = plus ? Sd.Union(Sd.Box(0, 0, 0.22f, 0.065f, 0.03f), Sd.Box(0, 0, 0.065f, 0.22f, 0.03f)) : Sd.Box(0, 0, 0.22f, 0.065f, 0.03f);
            p.Shape(sign, Rgba.White, 0.03f);
            return new DoodleArt(plus ? "s41c_sym_plus" : "s41c_sym_minus", p);
        }

        /// <summary>Vampiretto elettronico (corpo tondo in 0,0 raggio 0.42, mantello; denti-spina in basso). Fulminato: capelli dritti.</summary>
        public static DoodleArt S41C_Vampire(bool shocked)
        {
            var p = new DoodlePainter(-0.95f, -0.75f, 0.95f, 0.95f, 140) { Ink = 0.045f };
            // mantello
            p.Shape(Sd.Polygon(-0.3f, 0.1f, -0.9f, -0.2f, -0.75f, -0.35f, -0.55f, -0.3f, -0.45f, -0.5f, 0f, -0.3f, 0.45f, -0.5f, 0.55f, -0.3f, 0.75f, -0.35f, 0.9f, -0.2f, 0.3f, 0.1f),
                    Rgba.Hex("2A2234"));
            p.Fill(Sd.Polygon(-0.25f, 0.05f, -0.7f, -0.2f, 0f, -0.25f, 0.7f, -0.2f, 0.25f, 0.05f), Rgba.Hex("C4304A"));
            // orecchie da pipistrello
            foreach (float s in new[] { -1f, 1f })
                p.Shape(Sd.Polygon(s * 0.18f, 0.32f, s * 0.42f, 0.72f, s * 0.38f, 0.22f), Rgba.Hex("8A7AB8"));
            Body(p, Sd.Circle(0, 0.05f, 0.42f), shocked ? Rgba.Hex("D9D0FF") : Rgba.Hex("A898D8"), Rgba.Hex("7A6AA8"));
            if (shocked)
            {
                for (int i = 0; i < 7; i++)
                {
                    float a = 0.5f + i * 0.35f, c = S41C_Cos(a), sn = S41C_Sin(a);
                    p.Line(Rgba.Hex("FFE14D"), 0.05f, c * 0.42f, 0.05f + sn * 0.42f, c * 0.6f + 0.05f, 0.05f + sn * 0.62f, c * 0.78f, 0.05f + sn * 0.8f);
                }
                foreach (float s in new[] { -1f, 1f })
                {
                    p.Shape(Sd.Circle(s * 0.15f, 0.12f, 0.12f), Rgba.White, 0.03f);
                    p.Fill(Sd.Circle(s * 0.15f, 0.12f, 0.03f), S41C_Ink);
                }
                p.Shape(Sd.Ellipse(0, -0.17f, 0.14f, 0.1f), Rgba.Hex("3A1A2A"), 0.03f);
            }
            else
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    p.Shape(Sd.Box(s * 0.15f, 0.12f, 0.1f, 0.06f, 0.03f), Rgba.Hex("FF3B4E"), 0.03f);
                    p.InkLine(0.035f, s * 0.04f, 0.24f, s * 0.26f, 0.3f);
                }
                p.InkLine(0.035f, -0.16f, -0.12f, 0f, -0.08f, 0.16f, -0.12f);
            }
            // zanne a spina
            foreach (float s in new[] { -1f, 1f })
                p.Shape(Sd.Box(s * 0.07f, -0.27f, 0.03f, 0.08f, 0.01f), Rgba.Hex("D2D6DE"), 0.025f);
            return new DoodleArt(shocked ? "s41c_vampire_zap" : "s41c_vampire", p);
        }
    }
}
