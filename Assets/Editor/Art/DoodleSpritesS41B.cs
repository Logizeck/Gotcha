using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli della specifica 105-200, pacchetto B.</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV41B() => new DoodleArt[]
        {
            // Glass floor
            S41B_GlassBg(), S41B_GlassTile(false), S41B_GlassTile(true), S41B_GlassShard(),
            // Tiny thief
            S41B_ThiefRoom(), S41B_Thief(), S41B_Crown(),
            // Silent alarm
            S41B_VaultBg(), S41B_VaultDoor(), S41B_AlarmScreen(), S41B_Dot(), S41B_AlarmButton(), S41B_SliderTrack(), S41B_SliderKnob(),
            S41B_Speaker(), S41B_SpeakerCross(), S41B_Siren(),
            // Black hole
            S41B_SpaceBg(), S41B_BlackHole(), S41B_Swirl(), S41B_Planet(0), S41B_Planet(1), S41B_Planet(2),
            // Gravity resigned
            S41B_GravityRoom(), S41B_LittleMan(), S41B_Cushion(), S41B_GravityWorker(), S41B_TimeClock(), S41B_Letter(), S41B_Sign(),
            // Cheap magician
            S41B_StageBg(), S41B_Magician(), S41B_Wand(), S41B_TopHat(), S41B_TopHatBack(), S41B_Rabbit(), S41B_Potato(), S41B_SpellButton(),
            // Don't wake the bomb
            S41B_BombBg(), S41B_Pillow(), S41B_Bomb(0), S41B_Bomb(1), S41B_Bomb(2), S41B_FuseTip(),
            // Upside-down king
            S41B_ThroneBg(), S41B_Throne(), S41B_King(), S41B_Ladder(), S41B_ArrowButton(),
        };

        static float S41B_Sin(float v) => (float)Math.Sin(v);
        static float S41B_Cos(float v) => (float)Math.Cos(v);

        /// <summary>Stelline sparse deterministiche.</summary>
        static void S41B_Stars(DoodlePainter p, float x0, float y0, float x1, float y1, int n, int seed, float rMax)
        {
            var rnd = new Random(seed);
            for (int i = 0; i < n; i++)
            {
                float x = x0 + (float)rnd.NextDouble() * (x1 - x0), y = y0 + (float)rnd.NextDouble() * (y1 - y0);
                float r = 0.012f + (float)rnd.NextDouble() * rMax;
                p.Fill(Sd.Circle(x, y, r), Rgba.White.WithAlpha(0.45f + (float)rnd.NextDouble() * 0.5f), false);
            }
        }

        /// <summary>Occhietti tondi con pupilla.</summary>
        static void S41B_Eyes(DoodlePainter p, float x0, float x1, float y, float r, float lookX = 0f, float lookY = 0f)
        {
            foreach (float x in new[] { x0, x1 })
            {
                p.Shape(Sd.Circle(x, y, r), Rgba.White, r * 0.35f);
                p.Fill(Sd.Circle(x + lookX * r * 0.4f, y + lookY * r * 0.4f, r * 0.5f), Ink);
                p.Fill(Sd.Circle(x + lookX * r * 0.4f - r * 0.18f, y + lookY * r * 0.4f + r * 0.18f, r * 0.16f), Rgba.White);
            }
        }

        // ---------- Glass floor ----------

        /// <summary>Burrone con il ponte di vetro (coordinate del livello): sponda di partenza sotto y -3.5, sponda d'arrivo sopra y 2.05, file di lastre in y -2.95 + 0.88 i.</summary>
        public static DoodleArt S41B_GlassBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 5f, 7.5f), (x, y) => Rgba.Lerp(Rgba.Hex("0E1230"), Rgba.Hex("3A4F8C"), Clamp((y + 3f) / 9f)));
            // pareti di roccia del burrone ai lati
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Polygon(s * 4.6f, -7f, s * 2.25f, -7f, s * 2.55f, -3f, s * 2.1f, 0f, s * 2.5f, 3f, s * 2.2f, 7f, s * 4.6f, 7f), Rgba.Hex("4B4466"));
                for (int i = 0; i < 6; i++) p.InkLine(0.03f, s * 2.6f, -4f + i * 1.6f, s * 3.2f, -3.6f + i * 1.6f);
            }
            // nebbia in fondo
            for (int i = 0; i < 5; i++) p.Fill(Sd.Ellipse(-1.6f + i * 0.8f, -1.2f + (i % 2) * 0.5f, 1.1f, 0.25f), Rgba.White.WithAlpha(0.06f));
            // travi del ponte
            foreach (float x in new[] { -1.42f, 1.42f }) p.Shape(Sd.Box(x, -0.7f, 0.07f, 2.9f), Rgba.Hex("8D95A8"), 0.04f);
            for (int i = 0; i < 6; i++)
            {
                float y = -2.95f + i * 0.88f;
                p.Shape(Sd.Box(0, y - 0.36f, 1.45f, 0.045f), Rgba.Hex("6F7790"), 0.03f);
                p.Fill(Sd.Circle(-1.42f, y - 0.36f, 0.05f), Rgba.Hex("C9CFDC"));
                p.Fill(Sd.Circle(1.42f, y - 0.36f, 0.05f), Rgba.Hex("C9CFDC"));
            }
            // sponda di partenza
            p.Shape(Sd.Polygon(-4.6f, -7f, 4.6f, -7f, 4.6f, -3.5f, 1.6f, -3.5f, 1.3f, -3.65f, -1.3f, -3.65f, -1.6f, -3.5f, -4.6f, -3.5f), Rgba.Hex("8B6B4E"));
            p.Fill(Sd.Box(0, -3.58f, 4.6f, 0.07f), Rgba.Hex("6CBF5A"));
            // sponda d'arrivo con l'uscita
            p.Shape(Sd.Polygon(-4.6f, 7f, 4.6f, 7f, 4.6f, 2.05f, 1.6f, 2.05f, 1.3f, 1.9f, -1.3f, 1.9f, -1.6f, 2.05f, -4.6f, 2.05f), Rgba.Hex("8B6B4E"));
            p.Fill(Sd.Box(0, 2.1f, 4.6f, 0.07f), Rgba.Hex("6CBF5A"));
            p.Shape(Sd.Box(-1.6f, 2.75f, 0.42f, 0.62f, 0.3f), Rgba.Hex("3A2A22"));
            p.Fill(Sd.Box(-1.6f, 2.75f, 0.3f, 0.5f, 0.24f), Rgba.Hex("120D10"));
            return new DoodleArt("s41b_glass_bg", p);
        }

        /// <summary>Lastra di vetro 1.2 x 0.66 (centro 0,0), intera o crepata.</summary>
        public static DoodleArt S41B_GlassTile(bool cracked)
        {
            var p = new DoodlePainter(-0.66f, -0.38f, 0.66f, 0.38f, 160) { Ink = 0.035f, Wobble = 0.004f };
            Sdf pane = Sd.Box(0, 0, 0.6f, 0.33f, 0.04f);
            p.FillFunc(pane, (x, y) => Rgba.Lerp(Rgba.Hex("BFEFFF", 0.55f), Rgba.Hex("7FD3F2", 0.45f), Clamp((y + 0.33f) / 0.66f)));
            p.Shape(pane, Rgba.Clear, 0.035f);
            p.Line(Rgba.White.WithAlpha(0.75f), 0.035f, -0.45f, 0.2f, -0.25f, 0.2f);
            p.Line(Rgba.White.WithAlpha(0.5f), 0.025f, -0.48f, 0.08f, -0.38f, 0.08f);
            p.Line(Rgba.White.WithAlpha(0.35f), 0.08f, 0.1f, -0.3f, 0.4f, 0.3f);
            foreach (float x in new[] { -0.52f, 0.52f }) foreach (float y in new[] { -0.25f, 0.25f })
                p.Shape(Sd.Circle(x, y, 0.035f), Rgba.Hex("C9CFDC"), 0.015f);
            if (cracked)
            {
                Rgba c = Rgba.Hex("FFFFFF", 0.95f);
                p.Line(c, 0.02f, 0.05f, 0.02f, -0.2f, 0.25f, -0.35f, 0.28f);
                p.Line(c, 0.02f, 0.05f, 0.02f, 0.3f, 0.2f, 0.45f, 0.3f);
                p.Line(c, 0.02f, 0.05f, 0.02f, -0.15f, -0.2f, -0.3f, -0.3f);
                p.Line(c, 0.02f, 0.05f, 0.02f, 0.25f, -0.15f, 0.5f, -0.25f);
                p.Line(c, 0.018f, 0.05f, 0.02f, 0.02f, -0.3f);
                p.InkLine(0.012f, 0.05f, 0.02f, -0.2f, 0.25f);
                p.InkLine(0.012f, 0.05f, 0.02f, 0.25f, -0.15f);
            }
            return new DoodleArt(cracked ? "s41b_glass_cracked" : "s41b_glass_tile", p);
        }

        /// <summary>Scheggia di vetro.</summary>
        public static DoodleArt S41B_GlassShard()
        {
            var p = new DoodlePainter(-0.14f, -0.14f, 0.14f, 0.14f, 200) { Ink = 0.02f, Wobble = 0.003f };
            p.Shape(Sd.Polygon(-0.1f, -0.08f, 0.11f, -0.04f, -0.02f, 0.11f), Rgba.Hex("BFEFFF", 0.8f));
            return new DoodleArt("s41b_glass_shard", p);
        }

        // ---------- Tiny thief ----------

        /// <summary>
        /// Salotto arredato (coordinate del contenitore a zoom 1): muro sopra y -0.5, finestra x -0.9..0.5 y 0.85..2.45,
        /// libreria a sinistra, divano, tana nel battiscopa in x 1.45, tavolino col cuscino (corona in 0.2, -1.78), pianta.
        /// </summary>
        public static DoodleArt S41B_ThiefRoom()
        {
            var p = new DoodlePainter(-3.4f, -6f, 3.4f, 3.4f, 100) { Ink = 0.035f, Wobble = 0.006f };
            // muro a righe
            p.Fill(Sd.Box(0, 1.5f, 3.5f, 2.1f), Rgba.Hex("E9D7B8"), false);
            for (float x = -3.3f; x < 3.4f; x += 0.4f) p.Fill(Sd.Box(x, 1.5f, 0.08f, 2.1f), Rgba.Hex("DCC6A2"), false);
            // pavimento di legno
            p.Fill(Sd.Box(0, -3.3f, 3.5f, 2.8f), Rgba.Hex("B98552"), false);
            for (float y = -0.8f; y > -6f; y -= 0.45f) p.Line(Rgba.Hex("9A6B3E"), 0.025f, -3.4f, y, 3.4f, y);
            p.Shape(Sd.Box(0, -0.5f, 3.5f, 0.09f), Rgba.Hex("F4EEE2"), 0.025f);   // battiscopa
            // tana del topo nel battiscopa
            p.Fill(Sd.Union(Sd.Box(1.45f, -0.48f, 0.15f, 0.1f), Sd.Circle(1.45f, -0.4f, 0.15f)), Rgba.Hex("1C1418"));
            // finestra aperta sulla notte
            p.Shape(Sd.Box(-0.2f, 1.65f, 0.72f, 0.82f, 0.04f), Rgba.Hex("6B4A2E"));
            p.FillFunc(Sd.Box(-0.2f, 1.65f, 0.6f, 0.7f), (x, y) => Rgba.Lerp(Rgba.Hex("1A2350"), Rgba.Hex("3C4E8E"), Clamp((y - 1f) / 1.4f)));
            S41B_Stars(p, -0.75f, 1.0f, 0.35f, 2.3f, 14, 5, 0.012f);
            p.Shape(Sd.Subtract(Sd.Circle(0.15f, 2.0f, 0.18f), Sd.Circle(0.22f, 2.05f, 0.15f)), Rgba.Hex("FFF3B0"), 0.02f);
            p.Shape(Sd.Box(-0.2f, 0.82f, 0.82f, 0.06f, 0.02f), Rgba.Hex("8A6240"), 0.025f);   // davanzale
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Polygon(-0.2f + s * 0.66f, 2.5f, -0.2f + s * 0.95f, 2.5f, -0.2f + s * 0.85f, 0.9f, -0.2f + s * 0.68f, 0.9f), Rgba.Hex("C9584E"));
            // quadro
            p.Shape(Sd.Box(1.95f, 1.6f, 0.62f, 0.5f, 0.02f), Rgba.Hex("C99A3A"));
            p.Fill(Sd.Box(1.95f, 1.6f, 0.5f, 0.38f), Rgba.Hex("9FD3E6"));
            p.Fill(Sd.Polygon(1.45f, 1.22f, 1.85f, 1.75f, 2.1f, 1.45f, 2.45f, 1.9f, 2.45f, 1.22f), Rgba.Hex("6FA35A"));
            // libreria
            p.Shape(Sd.Box(-2.45f, 0.35f, 0.58f, 1.6f, 0.02f), Rgba.Hex("7A4E2D"));
            Rgba[] books = { Rgba.Hex("D35B5B"), Rgba.Hex("5B8ED3"), Rgba.Hex("E8C45A"), Rgba.Hex("6DBF7A"), Rgba.Hex("A77BD6") };
            for (int r = 0; r < 4; r++)
            {
                float y = -0.95f + r * 0.78f;
                p.Fill(Sd.Box(-2.45f, y + 0.33f, 0.5f, 0.36f), Rgba.Hex("4A2E1A"));
                for (int b = 0; b < 6; b++)
                {
                    float h = 0.22f + ((b * 7 + r * 3) % 5) * 0.025f;
                    p.Shape(Sd.Box(-2.85f + b * 0.15f, y + h, 0.055f, h, 0.01f), books[(b + r) % 5], 0.015f);
                }
                p.Shape(Sd.Box(-2.45f, y - 0.03f, 0.56f, 0.04f), Rgba.Hex("8F5E36"), 0.015f);
            }
            // divano
            p.Shape(Sd.Box(-0.15f, -0.25f, 1.15f, 0.55f, 0.18f), Rgba.Hex("5F8FB8"));
            p.Shape(Sd.Box(-0.15f, -0.72f, 1.25f, 0.28f, 0.12f), Rgba.Hex("4F7CA3"));
            foreach (float x in new[] { -1.35f, 1.05f }) p.Shape(Sd.Box(x, -0.5f, 0.17f, 0.45f, 0.12f), Rgba.Hex("6B9CC4"));
            foreach (float x in new[] { -0.7f, 0.4f }) p.Shape(Sd.Box(x, -0.45f, 0.5f, 0.12f, 0.06f), Rgba.Hex("6B9CC4"), 0.02f);
            // tappeto
            p.Shape(Sd.Ellipse(0.1f, -3.1f, 2.3f, 0.75f), Rgba.Hex("C45B6B"));
            p.Shape(Sd.Ellipse(0.1f, -3.1f, 1.8f, 0.5f), Rgba.Hex("E08C8C"), 0.02f);
            // tavolino col cuscino
            foreach (float x in new[] { -0.25f, 0.65f }) p.Shape(Sd.Box(x, -2.45f, 0.06f, 0.45f), Rgba.Hex("6E4426"), 0.02f);
            p.Shape(Sd.Box(0.2f, -2.0f, 0.75f, 0.08f, 0.03f), Rgba.Hex("8F5A31"));
            p.Shape(Sd.Ellipse(0.2f, -1.85f, 0.38f, 0.1f), Rgba.Hex("B03A5A"), 0.025f);
            p.Fill(Sd.Circle(-0.15f, -1.92f, 0.035f), Gold);
            p.Fill(Sd.Circle(0.55f, -1.92f, 0.035f), Gold);
            // pianta
            p.Shape(Sd.Polygon(1.95f, -3.6f, 2.65f, -3.6f, 2.55f, -4.2f, 2.05f, -4.2f), Rgba.Hex("C4673B"));
            foreach (float a in new[] { -0.9f, -0.4f, 0f, 0.4f, 0.9f })
                p.Shape(Sd.Rotate(Sd.Ellipse(2.3f, -2.95f, 0.13f, 0.6f), 2.3f, -3.55f, a * 50f), Rgba.Hex("5AA65A"), 0.02f);
            // tavolinetto con la lampada a sinistra
            p.Shape(Sd.Box(-2.4f, -3.4f, 0.5f, 0.06f), Rgba.Hex("8F5A31"), 0.025f);
            foreach (float x in new[] { -2.8f, -2.0f }) p.Shape(Sd.Box(x, -3.85f, 0.05f, 0.4f), Rgba.Hex("6E4426"), 0.02f);
            p.Shape(Sd.Box(-2.4f, -3.1f, 0.05f, 0.25f), Rgba.Hex("C9CFDC"), 0.02f);
            p.Shape(Sd.Polygon(-2.75f, -2.85f, -2.05f, -2.85f, -2.2f, -2.4f, -2.6f, -2.4f), Rgba.Hex("F2D27A"));
            // ombre dei mobili (dove il ladro si confonde)
            p.Fill(Sd.Ellipse(0.2f, -2.92f, 0.85f, 0.12f), Ink.WithAlpha(0.25f));
            p.Fill(Sd.Ellipse(-0.15f, -1.0f, 1.3f, 0.12f), Ink.WithAlpha(0.25f));
            return new DoodleArt("s41b_thief_room", p);
        }

        /// <summary>Ladruncolo minuscolo con mascherina e sacco (alto 0.16, piedi in y -0.08).</summary>
        public static DoodleArt S41B_Thief()
        {
            var p = new DoodlePainter(-0.1f, -0.09f, 0.1f, 0.1f, 900) { Ink = 0.006f, Wobble = 0.0006f };
            p.Shape(Sd.Polyline(0.008f, -0.025f, -0.08f, -0.02f, -0.04f), Rgba.Hex("2A2734"), 0.004f);
            p.Shape(Sd.Polyline(0.008f, 0.02f, -0.08f, 0.015f, -0.04f), Rgba.Hex("2A2734"), 0.004f);
            p.Shape(Sd.Ellipse(-0.04f, 0.015f, 0.045f, 0.04f), Rgba.Hex("D9B23A"), 0.005f);   // sacco con la corona
            p.Fill(Sd.Polygon(-0.07f, 0.045f, -0.06f, 0.075f, -0.045f, 0.055f, -0.035f, 0.08f, -0.025f, 0.055f, -0.01f, 0.075f, -0.01f, 0.045f), Gold);
            p.Shape(Sd.Box(0.005f, -0.005f, 0.035f, 0.045f, 0.015f), Rgba.Hex("3A3550"), 0.006f);
            for (int i = 0; i < 3; i++) p.Fill(Sd.Box(0.005f, -0.03f + i * 0.025f, 0.035f, 0.006f), Rgba.Hex("E8E8F0"));
            p.Shape(Sd.Circle(0.01f, 0.06f, 0.032f), Rgba.Hex("F2C9A0"), 0.006f);
            p.Fill(Sd.Box(0.01f, 0.065f, 0.033f, 0.01f), Ink);
            p.Fill(Sd.Circle(0.0f, 0.065f, 0.005f), Rgba.White);
            p.Fill(Sd.Circle(0.022f, 0.065f, 0.005f), Rgba.White);
            p.Shape(Sd.Box(0.01f, 0.09f, 0.032f, 0.012f, 0.008f), Rgba.Hex("2A2734"), 0.004f);
            return new DoodleArt("s41b_thief", p);
        }

        /// <summary>Corona d'oro (larga 0.3, base in y -0.08).</summary>
        public static DoodleArt S41B_Crown()
        {
            var p = new DoodlePainter(-0.19f, -0.12f, 0.19f, 0.16f, 500) { Ink = 0.014f, Wobble = 0.002f };
            Sdf c = Sd.Polygon(-0.15f, -0.08f, 0.15f, -0.08f, 0.16f, 0.1f, 0.08f, 0.02f, 0f, 0.12f, -0.08f, 0.02f, -0.16f, 0.1f);
            Body(p, c, Gold, GoldShade, null, -0.02f, 0.02f);
            p.Shape(Sd.Box(0, -0.06f, 0.15f, 0.025f, 0.01f), GoldShade, 0.01f);
            foreach (var g in new[] { (-0.16f, 0.11f), (0f, 0.13f), (0.16f, 0.11f) }) p.Shape(Sd.Circle(g.Item1, g.Item2, 0.022f), Rgba.Hex("FF5C7A"), 0.008f);
            p.Shape(Sd.Circle(0, -0.06f, 0.02f), Rgba.Hex("5BC0EB"), 0.008f);
            return new DoodleArt("s41b_crown", p);
        }

        // ---------- Silent alarm ----------

        /// <summary>Caveau: muro d'acciaio, strisce di sicurezza, pavimento sotto y -3.9.</summary>
        public static DoodleArt S41B_VaultBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 5f, 7.5f), (x, y) => Rgba.Lerp(Rgba.Hex("3E4A5E"), Rgba.Hex("67768F"), Clamp((y + 4f) / 9f)));
            for (float x = -4.2f; x < 4.5f; x += 1.2f) p.Line(Rgba.Hex("2E3747", 0.6f), 0.04f, x, -3.9f, x, 7f);
            for (float y = -3f; y < 7f; y += 1.5f) for (float x = -4.2f; x < 4.5f; x += 1.2f) p.Fill(Sd.Circle(x + 0.15f, y, 0.04f), Rgba.Hex("9AA6BA"));
            p.Fill(Sd.Box(0, -5.5f, 5f, 1.6f), Rgba.Hex("2A303C"), false);
            Sdf stripe = Sd.Box(0, -4.05f, 5f, 0.16f);
            p.Fill(stripe, Rgba.Hex("F2C230"), false);
            for (float x = -4.6f; x < 4.6f; x += 0.5f) p.Fill(Sd.Intersect(stripe, Sd.Polygon(x, -4.21f, x + 0.25f, -4.21f, x + 0.45f, -3.89f, x + 0.2f, -3.89f)), Ink, false);
            // pannello dei pulsanti
            p.Shape(Sd.Box(-0.35f, -1.35f, 1.95f, 1.8f, 0.12f), Rgba.Hex("2B3240"));
            p.Fill(Sd.Box(-0.35f, -1.35f, 1.8f, 1.65f, 0.08f), Rgba.Hex("394254"));
            return new DoodleArt("s41b_vault_bg", p);
        }

        /// <summary>Porta tonda del caveau (raggio 1.3, centro 0,0).</summary>
        public static DoodleArt S41B_VaultDoor()
        {
            var p = new DoodlePainter(-1.42f, -1.42f, 1.42f, 1.42f, 110) { Ink = 0.05f };
            Body(p, Sd.Circle(0, 0, 1.32f), Rgba.Hex("A9B4C6"), Rgba.Hex("7D889C"));
            p.Shape(Sd.Circle(0, 0, 1.12f), Rgba.Hex("BCC6D6"), 0.035f);
            for (int i = 0; i < 12; i++)
            {
                float a = i / 12f * 6.2832f;
                p.Shape(Sd.Circle(S41B_Cos(a) * 1.22f, S41B_Sin(a) * 1.22f, 0.05f), Rgba.Hex("5E687A"), 0.02f);
            }
            // maniglia a ruota in basso
            p.Shape(Sd.Subtract(Sd.Circle(0, -0.72f, 0.3f), Sd.Circle(0, -0.72f, 0.24f)), Rgba.Hex("6B7486"), 0.025f);
            for (int i = 0; i < 3; i++)
            {
                float a = i / 3f * 3.1416f;
                p.Line(Rgba.Hex("6B7486"), 0.06f, S41B_Cos(a) * -0.28f, -0.72f + S41B_Sin(a) * -0.28f, S41B_Cos(a) * 0.28f, -0.72f + S41B_Sin(a) * 0.28f);
            }
            p.Shape(Sd.Circle(0, -0.72f, 0.07f), Rgba.Hex("D7DEE8"), 0.02f);
            return new DoodleArt("s41b_vault_door", p);
        }

        /// <summary>Monitor del codice (2.3 x 0.9, centro 0,0).</summary>
        public static DoodleArt S41B_AlarmScreen()
        {
            var p = new DoodlePainter(-1.25f, -0.5f, 1.25f, 0.5f, 140) { Ink = 0.04f };
            p.Shape(Sd.Box(0, 0, 1.15f, 0.43f, 0.08f), Rgba.Hex("2B3240"));
            p.FillFunc(Sd.Box(0, 0, 1.03f, 0.33f, 0.04f), (x, y) => Rgba.Lerp(Rgba.Hex("0B1A1E"), Rgba.Hex("12343A"), Clamp((y + 0.3f) / 0.6f)));
            for (float y = -0.3f; y < 0.33f; y += 0.06f) p.Line(Rgba.White.WithAlpha(0.04f), 0.015f, -1f, y, 1f, y);
            return new DoodleArt("s41b_alarm_screen", p);
        }

        /// <summary>Pallino bianco (si tinge; raggio 0.15).</summary>
        public static DoodleArt S41B_Dot()
        {
            var p = new DoodlePainter(-0.19f, -0.19f, 0.19f, 0.19f, 250) { Ink = 0.03f, Wobble = 0.003f };
            p.Shape(Sd.Circle(0, 0, 0.15f), Rgba.White);
            p.Fill(Sd.Circle(-0.05f, 0.05f, 0.04f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("s41b_dot", p);
        }

        /// <summary>Pulsante tondo da sala giochi, bianco da tingere (raggio 0.62).</summary>
        public static DoodleArt S41B_AlarmButton()
        {
            var p = new DoodlePainter(-0.72f, -0.72f, 0.72f, 0.72f, 140) { Ink = 0.045f };
            p.Shape(Sd.Circle(0, -0.04f, 0.66f), Rgba.Hex("2B2B33"));
            Body(p, Sd.Circle(0, 0.04f, 0.54f), Rgba.Hex("F4F4F4"), Rgba.Hex("B5B5BE"));
            p.Fill(Sd.Ellipse(-0.18f, 0.25f, 0.18f, 0.1f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("s41b_alarm_button", p);
        }

        /// <summary>Guida verticale del volume (alta 3.3, centro 0,0).</summary>
        public static DoodleArt S41B_SliderTrack()
        {
            var p = new DoodlePainter(-0.3f, -1.8f, 0.3f, 1.8f, 120) { Ink = 0.035f };
            p.Shape(Sd.Box(0, 0, 0.22f, 1.72f, 0.18f), Rgba.Hex("2B3240"));
            p.Fill(Sd.Box(0, 0, 0.06f, 1.5f, 0.05f), Rgba.Hex("12161E"));
            for (int i = 0; i <= 5; i++) p.Line(Rgba.Hex("9AA6BA"), 0.02f, 0.11f, -1.45f + i * 0.58f, 0.18f, -1.45f + i * 0.58f);
            return new DoodleArt("s41b_slider_track", p);
        }

        /// <summary>Manopola del volume (0.7 x 0.42).</summary>
        public static DoodleArt S41B_SliderKnob()
        {
            var p = new DoodlePainter(-0.4f, -0.26f, 0.4f, 0.26f, 160) { Ink = 0.035f };
            Body(p, Sd.Box(0, 0, 0.34f, 0.19f, 0.08f), Rgba.Hex("E0E6EE"), Rgba.Hex("A9B2C0"));
            for (int i = -1; i <= 1; i++) p.InkLine(0.02f, -0.18f, i * 0.07f, 0.18f, i * 0.07f);
            return new DoodleArt("s41b_slider_knob", p);
        }

        /// <summary>Altoparlante con onde (0.6 x 0.5).</summary>
        public static DoodleArt S41B_Speaker()
        {
            var p = new DoodlePainter(-0.34f, -0.3f, 0.36f, 0.3f, 180) { Ink = 0.03f };
            p.Shape(Sd.Polygon(-0.28f, -0.09f, -0.14f, -0.09f, 0.02f, -0.22f, 0.02f, 0.22f, -0.14f, 0.09f, -0.28f, 0.09f), Rgba.White);
            p.Shape(Sd.Intersect(Sd.Subtract(Sd.Circle(0.06f, 0, 0.16f), Sd.Circle(0.06f, 0, 0.12f)), Sd.Box(0.2f, 0, 0.14f, 0.3f)), Rgba.White, 0.015f);
            p.Shape(Sd.Intersect(Sd.Subtract(Sd.Circle(0.06f, 0, 0.27f), Sd.Circle(0.06f, 0, 0.23f)), Sd.Box(0.25f, 0, 0.19f, 0.3f)), Rgba.White, 0.015f);
            return new DoodleArt("s41b_speaker", p);
        }

        /// <summary>X rossa sull'altoparlante.</summary>
        public static DoodleArt S41B_SpeakerCross()
        {
            var p = new DoodlePainter(-0.24f, -0.24f, 0.24f, 0.24f, 180) { Ink = 0.03f };
            p.Shape(Sd.Union(Sd.Capsule(-0.16f, -0.16f, 0.16f, 0.16f, 0.05f), Sd.Capsule(-0.16f, 0.16f, 0.16f, -0.16f, 0.05f)), Rgba.Hex("FF4D4D"));
            return new DoodleArt("s41b_speaker_x", p);
        }

        /// <summary>Lampeggiante rosso (raggio 0.3, ruota).</summary>
        public static DoodleArt S41B_Siren()
        {
            var p = new DoodlePainter(-0.4f, -0.4f, 0.4f, 0.4f, 160) { Ink = 0.035f };
            Body(p, Sd.Circle(0, 0, 0.3f), Rgba.Hex("FF5A4E"), Rgba.Hex("C7372E"));
            p.Fill(Sd.Polygon(0, 0, 0.36f, 0.12f, 0.36f, -0.12f), Rgba.Hex("FFE08A", 0.8f));
            p.Fill(Sd.Polygon(0, 0, -0.36f, 0.12f, -0.36f, -0.12f), Rgba.Hex("FFE08A", 0.8f));
            p.Fill(Sd.Circle(-0.08f, 0.09f, 0.06f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("s41b_siren", p);
        }

        // ---------- Black hole ----------

        public static DoodleArt S41B_SpaceBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 5f, 7.5f), (x, y) => Rgba.Lerp(Rgba.Hex("0B0820"), Rgba.Hex("231A4A"), Clamp((y + 7f) / 14f)));
            p.Fill(Sd.Ellipse(-1.8f, 2.2f, 2.2f, 0.9f), Rgba.Hex("C266D6", 0.12f));
            p.Fill(Sd.Ellipse(2f, -2.6f, 2f, 1.1f), Rgba.Hex("4FA3E0", 0.12f));
            S41B_Stars(p, -4.5f, -7f, 4.5f, 7f, 160, 11, 0.03f);
            return new DoodleArt("s41b_space_bg", p);
        }

        /// <summary>Buco nero di raggio 1 con il disco di luce attorno (fino a 1.35).</summary>
        public static DoodleArt S41B_BlackHole()
        {
            var p = new DoodlePainter(-1.4f, -1.4f, 1.4f, 1.4f, 160) { Ink = 0f, Wobble = 0f };
            p.FillFunc(Sd.Circle(0, 0, 1.38f), (x, y) =>
            {
                float r = (float)Math.Sqrt(x * x + y * y);
                float k = Clamp((r - 1f) / 0.38f);
                return Rgba.Lerp(Rgba.Hex("FFB347", 0.95f), Rgba.Hex("FF5E8A", 0f), k);
            });
            p.Fill(Sd.Circle(0, 0, 1.02f), Rgba.Hex("000000"), false);
            return new DoodleArt("s41b_black_hole", p);
        }

        /// <summary>Bracci a spirale semitrasparenti (ruotano sopra il buco).</summary>
        public static DoodleArt S41B_Swirl()
        {
            var p = new DoodlePainter(-1.35f, -1.35f, 1.35f, 1.35f, 120) { Ink = 0f, Wobble = 0f };
            for (int arm = 0; arm < 3; arm++)
            {
                var pts = new List<float>();
                for (int i = 0; i <= 20; i++)
                {
                    float t = i / 20f, a = arm * 2.094f + t * 3.4f, r = 1.3f - t * 0.3f;
                    pts.Add(S41B_Cos(a) * r); pts.Add(S41B_Sin(a) * r);
                }
                p.Line(Rgba.Hex("FFD9A0", 0.45f), 0.06f, pts.ToArray());
            }
            return new DoodleArt("s41b_swirl", p);
        }

        /// <summary>Pianetini (raggio 0.28): 0 terra, 1 con gli anelli, 2 di ghiaccio.</summary>
        public static DoodleArt S41B_Planet(int kind)
        {
            var p = new DoodlePainter(-0.5f, -0.36f, 0.5f, 0.36f, 220) { Ink = 0.03f };
            Sdf ball = Sd.Circle(0, 0, 0.27f);
            if (kind == 1) p.Shape(Sd.Subtract(Sd.Ellipse(0, 0, 0.46f, 0.11f), Sd.Ellipse(0, 0.02f, 0.34f, 0.06f)), Rgba.Hex("F2D27A"), 0.02f);
            Rgba fill = kind == 0 ? Rgba.Hex("5BA8E0") : kind == 1 ? Rgba.Hex("E8A15A") : Rgba.Hex("C9F0FF");
            Rgba shade = kind == 0 ? Rgba.Hex("3E7FB8") : kind == 1 ? Rgba.Hex("C27A3A") : Rgba.Hex("8FC9E6");
            Body(p, ball, fill, shade, null, -0.05f, 0.05f);
            if (kind == 0)
            {
                p.Fill(Sd.Intersect(ball, Sd.Ellipse(-0.08f, 0.06f, 0.12f, 0.09f)), Rgba.Hex("6CBF5A"));
                p.Fill(Sd.Intersect(ball, Sd.Ellipse(0.12f, -0.1f, 0.1f, 0.07f)), Rgba.Hex("6CBF5A"));
            }
            if (kind == 1)
            {
                p.Fill(Sd.Intersect(ball, Sd.Box(0, 0.08f, 0.3f, 0.025f)), Rgba.Hex("C27A3A"));
                p.Shape(Sd.Intersect(Sd.Subtract(Sd.Ellipse(0, 0, 0.46f, 0.11f), Sd.Ellipse(0, 0.02f, 0.34f, 0.06f)), Sd.Box(0, -0.1f, 0.5f, 0.1f)), Rgba.Hex("F2D27A"), 0.02f);
            }
            if (kind == 2) p.Fill(Sd.Intersect(ball, Sd.Circle(0.06f, -0.06f, 0.06f)), Rgba.White.WithAlpha(0.7f));
            // facce preoccupate
            foreach (float x in new[] { -0.08f, 0.08f }) { p.Fill(Sd.Circle(x, 0.03f, 0.04f), Rgba.White); p.Fill(Sd.Circle(x, 0.03f, 0.022f), Ink); }
            p.Shape(Sd.Ellipse(0, -0.09f, 0.035f, 0.04f), Rgba.Hex("7A2E3A"), 0.012f);
            return new DoodleArt("s41b_planet" + kind, p);
        }
        // ---------- Gravity resigned ----------

        /// <summary>Stanza: soffitto sotto y 2.5, muri laterali oltre x ±2.5, pavimento in y -3.2.</summary>
        public static DoodleArt S41B_GravityRoom()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.Fill(Sd.Box(0, 0, 5f, 7.5f), Rgba.Hex("CFE6D8"), false);
            for (float x = -2.2f; x < 2.5f; x += 0.55f) p.Line(Rgba.Hex("BBD8C6"), 0.12f, x, -3.2f, x, 2.5f);
            // soffitto
            p.Shape(Sd.Box(0, 4.75f, 5f, 2.25f), Rgba.Hex("E9E2D0"));
            p.Fill(Sd.Box(0, 2.56f, 5f, 0.06f), Rgba.Hex("BDB39A"));
            // lampadario spento
            p.InkLine(0.03f, 0.9f, 2.5f, 0.9f, 2.1f);
            p.Shape(Sd.Polygon(0.65f, 2.1f, 1.15f, 2.1f, 1.05f, 1.9f, 0.75f, 1.9f), Rgba.Hex("F2D27A"), 0.03f);
            // muri laterali
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Box(s * 3.6f, 0, 1.1f, 7.5f), Rgba.Hex("A88F74"));
                for (float y = -6.5f; y < 7f; y += 0.5f) p.Line(Rgba.Hex("8E7660"), 0.025f, s * 2.5f, y, s * 4.7f, y);
            }
            // quadro storto
            p.Shape(Sd.Rotate(Sd.Box(-1.3f, 0.9f, 0.5f, 0.38f, 0.02f), -1.3f, 0.9f, 8f), Rgba.Hex("C99A3A"));
            p.Fill(Sd.Rotate(Sd.Box(-1.3f, 0.9f, 0.4f, 0.28f), -1.3f, 0.9f, 8f), Rgba.Hex("F6E7A8"));
            p.Fill(Sd.Circle(-1.3f, 0.88f, 0.12f), Rgba.Hex("E2735A"));
            // pavimento
            p.Shape(Sd.Box(0, -5.1f, 5f, 1.9f), Rgba.Hex("9A6B47"));
            for (float x = -2.4f; x < 2.5f; x += 0.8f) p.Line(Rgba.Hex("7E5536"), 0.025f, x, -3.25f, x - 0.2f, -7f);
            return new DoodleArt("s41b_gr_room", p);
        }

        /// <summary>Omino dritto (alto 0.9, piedi in y -0.45, centro 0,0).</summary>
        public static DoodleArt S41B_LittleMan()
        {
            var p = new DoodlePainter(-0.42f, -0.5f, 0.42f, 0.5f, 220) { Ink = 0.03f };
            foreach (float x in new[] { -0.09f, 0.09f })
            {
                p.Shape(Sd.Capsule(x, -0.12f, x, -0.38f, 0.055f), Rgba.Hex("3E5A8C"), 0.025f);
                p.Shape(Sd.Ellipse(x + 0.02f, -0.42f, 0.08f, 0.04f), Rgba.Hex("5A3A2A"), 0.02f);
            }
            p.Shape(Sd.Capsule(-0.17f, 0.06f, -0.33f, 0.24f, 0.045f), Rgba.Hex("F2C14E"), 0.025f);
            p.Shape(Sd.Capsule(0.17f, 0.06f, 0.33f, 0.24f, 0.045f), Rgba.Hex("F2C14E"), 0.025f);
            Body(p, Sd.Box(0, -0.02f, 0.17f, 0.17f, 0.08f), Rgba.Hex("F2C14E"), Rgba.Hex("D19A2A"));
            Sdf head = Sd.Circle(0, 0.28f, 0.16f);
            Body(p, head, Rgba.Hex("FFD7B5"), Rgba.Hex("E8B48C"), null, -0.03f, 0.03f);
            p.Fill(Sd.Intersect(head, Sd.Box(0, 0.42f, 0.2f, 0.06f)), Rgba.Hex("6B3E26"));
            S41B_Eyes(p, -0.06f, 0.06f, 0.29f, 0.04f, 0f, -0.5f);
            p.Shape(Sd.Ellipse(0, 0.19f, 0.035f, 0.03f), Rgba.Hex("7A2E3A"), 0.012f);
            return new DoodleArt("s41b_gr_man", p);
        }

        /// <summary>Materasso sul pavimento (2.6 x 0.35, sommità in y 0.15).</summary>
        public static DoodleArt S41B_Cushion()
        {
            var p = new DoodlePainter(-1.4f, -0.25f, 1.4f, 0.25f, 110) { Ink = 0.04f };
            Body(p, Sd.Box(0, -0.02f, 1.3f, 0.17f, 0.12f), Rgba.Hex("F28BA8"), Rgba.Hex("D46A88"));
            for (int i = -2; i <= 2; i++) p.Fill(Sd.Circle(i * 0.5f, -0.02f, 0.03f), Rgba.Hex("B04E6C"));
            return new DoodleArt("s41b_gr_cushion", p);
        }

        /// <summary>La Gravità in persona: impiegato scocciato con cravatta, cartellino "G" e valigetta (alto 1.3, piedi in -0.65).</summary>
        public static DoodleArt S41B_GravityWorker()
        {
            var p = new DoodlePainter(-0.6f, -0.7f, 0.55f, 0.7f, 180) { Ink = 0.035f };
            foreach (float x in new[] { -0.1f, 0.1f })
            {
                p.Shape(Sd.Box(x, -0.4f, 0.07f, 0.22f, 0.03f), Rgba.Hex("3A3F52"), 0.025f);
                p.Shape(Sd.Ellipse(x - 0.03f, -0.63f, 0.1f, 0.045f), Rgba.Hex("1E1E26"), 0.02f);
            }
            // valigetta
            p.Shape(Sd.Subtract(Sd.Box(-0.4f, -0.2f, 0.07f, 0.05f, 0.03f), Sd.Box(-0.4f, -0.2f, 0.04f, 0.025f)), Rgba.Hex("5A3A22"), 0.02f);
            p.Shape(Sd.Box(-0.4f, -0.38f, 0.17f, 0.13f, 0.03f), Rgba.Hex("8A5A32"));
            p.Fill(Sd.Box(-0.4f, -0.33f, 0.03f, 0.025f), Gold);
            p.Shape(Sd.Capsule(-0.2f, 0.2f, -0.38f, -0.18f, 0.05f), Rgba.Hex("4A5068"), 0.025f);
            p.Shape(Sd.Capsule(0.2f, 0.2f, 0.3f, -0.12f, 0.05f), Rgba.Hex("4A5068"), 0.025f);
            Body(p, Sd.Box(0, 0.02f, 0.22f, 0.26f, 0.08f), Rgba.Hex("4A5068"), Rgba.Hex("363B50"));
            p.Fill(Sd.Polygon(-0.08f, 0.28f, 0.08f, 0.28f, 0f, 0.15f), Rgba.White);
            p.Shape(Sd.Polygon(-0.03f, 0.24f, 0.03f, 0.24f, 0.05f, -0.08f, 0f, -0.14f, -0.05f, -0.08f), Rgba.Hex("D6453B"), 0.018f);
            p.Shape(Sd.Box(0.13f, 0.12f, 0.06f, 0.05f, 0.01f), Rgba.White, 0.015f);
            p.InkLine(0.014f, 0.11f, 0.1f, 0.11f, 0.15f, 0.15f, 0.15f);
            Sdf head = Sd.Circle(0, 0.47f, 0.2f);
            Body(p, head, Rgba.Hex("BFE3FF"), Rgba.Hex("8FC0E6"), null, -0.04f, 0.04f);
            p.Fill(Sd.Intersect(head, Sd.Box(0, 0.65f, 0.25f, 0.06f)), Rgba.Hex("6E7590"));
            // occhi annoiati (palpebre a metà) e bocca storta
            foreach (float x in new[] { -0.08f, 0.08f })
            {
                p.Shape(Sd.Circle(x, 0.5f, 0.05f), Rgba.White, 0.015f);
                p.Fill(Sd.Circle(x, 0.48f, 0.025f), Ink);
                p.Fill(Sd.Box(x, 0.53f, 0.06f, 0.025f), Rgba.Hex("8FC0E6"));
                p.InkLine(0.016f, x - 0.055f, 0.505f, x + 0.055f, 0.505f);
            }
            p.InkLine(0.02f, -0.07f, 0.37f, 0.07f, 0.39f);
            return new DoodleArt("s41b_gr_worker", p);
        }

        /// <summary>Orologio marcatempo da muro (0.6 x 0.9).</summary>
        public static DoodleArt S41B_TimeClock()
        {
            var p = new DoodlePainter(-0.36f, -0.5f, 0.36f, 0.5f, 180) { Ink = 0.035f };
            Body(p, Sd.Box(0, 0, 0.3f, 0.44f, 0.06f), Rgba.Hex("D9C7A0"), Rgba.Hex("B5A07A"));
            p.Shape(Sd.Circle(0, 0.18f, 0.18f), Rgba.White, 0.025f);
            p.InkLine(0.02f, 0, 0.18f, 0, 0.3f);
            p.InkLine(0.02f, 0, 0.18f, 0.09f, 0.14f);
            p.Shape(Sd.Box(0, -0.2f, 0.18f, 0.03f), Ink.WithAlpha(0.8f), 0.01f);
            p.Shape(Sd.Box(0.05f, -0.12f, 0.08f, 0.13f, 0.01f), Rgba.Hex("FFF6D8"), 0.015f);
            return new DoodleArt("s41b_gr_clock", p);
        }

        /// <summary>Lettera di dimissioni (1.6 x 2.0; il testo è un'etichetta).</summary>
        public static DoodleArt S41B_Letter()
        {
            var p = new DoodlePainter(-0.88f, -1.08f, 0.88f, 1.08f, 110) { Ink = 0.04f };
            p.Shape(Sd.Polygon(-0.8f, -1f, 0.8f, -1f, 0.8f, 0.75f, 0.55f, 1f, -0.8f, 1f), Rgba.Hex("FFFDF4"));
            p.Shape(Sd.Polygon(0.55f, 1f, 0.55f, 0.75f, 0.8f, 0.75f), Rgba.Hex("E8E2CC"), 0.025f);
            for (int i = 0; i < 5; i++) p.Line(Rgba.Hex("B9C9E8"), 0.02f, -0.65f, -0.85f + i * 0.13f, 0.65f, -0.85f + i * 0.13f);
            p.Line(Rgba.Hex("E2735A", 0.6f), 0.02f, -0.6f, 1f, -0.6f, -1f);
            return new DoodleArt("s41b_gr_letter", p);
        }

        /// <summary>Cartello su paletto (2.5 x 0.9 la tavola, centro della tavola in 0,0).</summary>
        public static DoodleArt S41B_Sign()
        {
            var p = new DoodlePainter(-1.35f, -1.05f, 1.35f, 0.55f, 110) { Ink = 0.045f };
            p.Shape(Sd.Box(0, -0.7f, 0.06f, 0.35f), Rgba.Hex("8A5A32"));
            Body(p, Sd.Box(0, 0, 1.25f, 0.45f, 0.06f), Rgba.Hex("F6E7C1"), Rgba.Hex("DCC79B"));
            return new DoodleArt("s41b_gr_sign", p);
        }

        // ---------- Cheap magician ----------

        /// <summary>Palcoscenico: sipario rosso ai lati e in alto, assi del palco sotto y -1.0.</summary>
        public static DoodleArt S41B_StageBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 5f, 7.5f), (x, y) => Rgba.Lerp(Rgba.Hex("2A1B3D"), Rgba.Hex("4A2F63"), Clamp((y + 1f) / 6f)));
            // luce del faro
            p.Fill(Sd.Polygon(-0.4f, 5f, 0.4f, 5f, 2.2f, -2.4f, -2.2f, -2.4f), Rgba.Hex("FFF3C4", 0.12f));
            // palco
            p.Shape(Sd.Box(0, -4.2f, 5f, 3.2f), Rgba.Hex("B07A45"));
            for (float y = -1.4f; y > -7f; y -= 0.45f) p.Line(Rgba.Hex("8E5E33"), 0.03f, -4.5f, y, 4.5f, y);
            p.Fill(Sd.Ellipse(0, -2.2f, 2.3f, 0.6f), Rgba.Hex("FFF3C4", 0.18f));
            // sipari
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Polygon(s * 4.6f, 7f, s * 2.1f, 7f, s * 2.35f, 1.5f, s * 2.6f, -1.1f, s * 4.6f, -1.1f), Rgba.Hex("C02A3A"));
                for (int i = 0; i < 4; i++) p.Line(Rgba.Hex("8E1A28"), 0.05f, s * (2.4f + i * 0.5f), 7f, s * (2.6f + i * 0.5f), -1f);
            }
            p.Shape(Sd.Box(0, 3.4f, 5f, 0.35f), Rgba.Hex("A82232"));
            for (float x = -4.2f; x < 4.5f; x += 0.6f) p.Fill(Sd.Circle(x, 3.05f, 0.2f), Rgba.Hex("A82232"));
            p.Fill(Sd.Box(0, 3.0f, 5f, 0.04f), Gold);
            return new DoodleArt("s41b_stage_bg", p);
        }

        /// <summary>Mago da quattro soldi con mantello rattoppato (alto 2.4, piedi in -1.2; la mano destra in 0.42, 0.15).</summary>
        public static DoodleArt S41B_Magician()
        {
            var p = new DoodlePainter(-0.8f, -1.25f, 0.8f, 1.25f, 110) { Ink = 0.045f };
            p.Shape(Sd.Polygon(-0.55f, -1.1f, 0.5f, -1.1f, 0.3f, 0.45f, -0.35f, 0.45f), Rgba.Hex("3B2A5C"));
            p.Shape(Sd.Box(-0.25f, -0.5f, 0.12f, 0.1f, 0.02f), Rgba.Hex("7F6BAE"), 0.025f);   // toppa
            p.InkLine(0.015f, -0.33f, -0.5f, -0.17f, -0.5f);
            foreach (float x in new[] { -0.15f, 0.12f })
            {
                p.Shape(Sd.Box(x, -1.0f, 0.07f, 0.15f), Rgba.Hex("1E1A26"), 0.025f);
                p.Shape(Sd.Ellipse(x + 0.04f, -1.15f, 0.13f, 0.06f), Rgba.Hex("1E1A26"), 0.02f);
            }
            Body(p, Sd.Box(0, -0.25f, 0.24f, 0.55f, 0.1f), Rgba.Hex("2B2B3A"), Rgba.Hex("1E1E2A"));
            p.Fill(Sd.Polygon(-0.1f, 0.3f, 0.1f, 0.3f, 0f, -0.1f), Rgba.White);
            p.Shape(Sd.Polygon(-0.12f, 0.27f, 0f, 0.22f, 0.12f, 0.27f, 0.12f, 0.17f, 0f, 0.22f, -0.12f, 0.17f), Rgba.Hex("D6453B"), 0.015f);
            p.Shape(Sd.Capsule(0.2f, 0.2f, 0.42f, 0.15f, 0.06f), Rgba.Hex("2B2B3A"), 0.025f);
            p.Shape(Sd.Circle(0.45f, 0.15f, 0.07f), Rgba.White, 0.02f);
            p.Shape(Sd.Capsule(-0.2f, 0.2f, -0.35f, -0.15f, 0.06f), Rgba.Hex("2B2B3A"), 0.025f);
            p.Shape(Sd.Circle(-0.36f, -0.18f, 0.07f), Rgba.White, 0.02f);
            Sdf head = Sd.Ellipse(0, 0.55f, 0.2f, 0.24f);
            Body(p, head, Rgba.Hex("FFD7B5"), Rgba.Hex("E8B48C"), null, -0.04f, 0.04f);
            S41B_Eyes(p, -0.07f, 0.08f, 0.6f, 0.045f, 0.4f, 0f);
            p.Shape(Sd.Union(Sd.Ellipse(-0.08f, 0.47f, 0.09f, 0.035f), Sd.Ellipse(0.08f, 0.47f, 0.09f, 0.035f)), Rgba.Hex("3A2A22"), 0.015f);   // baffi
            p.Shape(Sd.Ellipse(0.03f, 0.53f, 0.04f, 0.035f), Rgba.Hex("F2A08A"), 0.015f);
            // cilindro un po' ammaccato
            p.Shape(Sd.Box(0, 0.78f, 0.28f, 0.035f, 0.02f), Rgba.Hex("1E1A26"));
            p.Shape(Sd.Rotate(Sd.Box(0.02f, 1.0f, 0.17f, 0.2f, 0.03f), 0f, 0.8f, -6f), Rgba.Hex("1E1A26"));
            p.Fill(Sd.Rotate(Sd.Box(0.02f, 0.87f, 0.17f, 0.04f), 0f, 0.8f, -6f), Rgba.Hex("C02A3A"));
            return new DoodleArt("s41b_magician", p);
        }

        /// <summary>Bacchetta (perno all'impugnatura in 0,0, punta verso l'alto a destra).</summary>
        public static DoodleArt S41B_Wand()
        {
            var p = new DoodlePainter(-0.08f, -0.08f, 0.5f, 0.5f, 200) { Ink = 0.025f };
            p.Shape(Sd.Capsule(0, 0, 0.38f, 0.38f, 0.035f), Rgba.Hex("1E1A26"));
            p.Shape(Sd.Capsule(0.3f, 0.3f, 0.4f, 0.4f, 0.036f), Rgba.White, 0.02f);
            return new DoodleArt("s41b_wand", p);
        }

        /// <summary>Cilindro rovesciato con l'apertura in alto (alto 0.9, centro 0,0: tesa in -0.45, bocca in 0.42).</summary>
        public static DoodleArt S41B_TopHat()
        {
            var p = new DoodlePainter(-0.62f, -0.55f, 0.62f, 0.55f, 160) { Ink = 0.04f };
            p.Shape(Sd.Ellipse(0, -0.42f, 0.55f, 0.1f), Rgba.Hex("1E1A26"));
            Body(p, Sd.Box(0, 0f, 0.38f, 0.42f, 0.03f), Rgba.Hex("2E2838"), Rgba.Hex("1E1A26"));
            p.Fill(Sd.Box(0, -0.25f, 0.38f, 0.07f), Rgba.Hex("C02A3A"));
            // bordo anteriore della bocca
            p.Shape(Sd.Intersect(Sd.Subtract(Sd.Ellipse(0, 0.42f, 0.4f, 0.09f), Sd.Ellipse(0, 0.44f, 0.34f, 0.06f)), Sd.Box(0, 0.36f, 0.5f, 0.07f)), Rgba.Hex("3D3548"), 0.02f);
            p.Line(Rgba.White.WithAlpha(0.25f), 0.04f, -0.25f, 0.3f, -0.25f, -0.1f);
            return new DoodleArt("s41b_tophat", p);
        }

        /// <summary>Interno scuro della bocca del cilindro (va dietro al coniglio).</summary>
        public static DoodleArt S41B_TopHatBack()
        {
            var p = new DoodlePainter(-0.62f, -0.55f, 0.62f, 0.55f, 160) { Ink = 0.04f };
            p.Shape(Sd.Ellipse(0, 0.42f, 0.4f, 0.09f), Rgba.Hex("0E0B12"));
            return new DoodleArt("s41b_tophat_back", p);
        }

        /// <summary>Coniglio bianco (alto 1.0, centro 0,0).</summary>
        public static DoodleArt S41B_Rabbit()
        {
            var p = new DoodlePainter(-0.42f, -0.52f, 0.42f, 0.55f, 170) { Ink = 0.035f };
            foreach (float s in new[] { -1f, 1f })
            {
                Sdf ear = Sd.Rotate(Sd.Ellipse(s * 0.1f, 0.32f, 0.07f, 0.2f), s * 0.1f, 0.15f, -s * 12f);
                p.Shape(ear, Rgba.White);
                p.Fill(Sd.Rotate(Sd.Ellipse(s * 0.1f, 0.32f, 0.035f, 0.14f), s * 0.1f, 0.15f, -s * 12f), Rgba.Hex("FFB3C4"));
            }
            Body(p, Sd.Ellipse(0, -0.28f, 0.27f, 0.22f), Rgba.White, Rgba.Hex("D8DCE8"));
            Body(p, Sd.Circle(0, 0.05f, 0.2f), Rgba.White, Rgba.Hex("D8DCE8"), null, -0.04f, 0.04f);
            S41B_Eyes(p, -0.08f, 0.08f, 0.09f, 0.045f, 0f, 0.3f);
            p.Shape(Sd.Ellipse(0, -0.01f, 0.035f, 0.025f), Rgba.Hex("FF8FA3"), 0.012f);
            p.Shape(Sd.Box(0, -0.07f, 0.03f, 0.025f), Rgba.White, 0.012f);
            foreach (float x in new[] { -0.12f, 0.12f }) p.Shape(Sd.Ellipse(x, -0.48f, 0.09f, 0.045f), Rgba.White, 0.02f);
            return new DoodleArt("s41b_rabbit", p);
        }

        /// <summary>Patata con gli occhietti (larga 0.75).</summary>
        public static DoodleArt S41B_Potato()
        {
            var p = new DoodlePainter(-0.42f, -0.52f, 0.42f, 0.55f, 170) { Ink = 0.035f };
            Sdf body = Sd.Rotate(Sd.Ellipse(0, -0.15f, 0.36f, 0.27f), 0, -0.15f, 12f);
            Body(p, body, Rgba.Hex("D9A866"), Rgba.Hex("B5854A"));
            foreach (var d in new[] { (-0.2f, -0.05f), (0.18f, -0.28f), (0.05f, 0.0f), (-0.1f, -0.32f) }) p.Fill(Sd.Circle(d.Item1, d.Item2, 0.025f), Rgba.Hex("8E6232"));
            S41B_Eyes(p, -0.07f, 0.09f, -0.1f, 0.04f, 0f, 0f);
            p.InkLine(0.015f, -0.03f, -0.22f, 0.05f, -0.2f);
            return new DoodleArt("s41b_potato", p);
        }

        /// <summary>Pulsante viola delle formule (1.75 x 0.7; il testo è un'etichetta).</summary>
        public static DoodleArt S41B_SpellButton()
        {
            var p = new DoodlePainter(-0.95f, -0.42f, 0.95f, 0.42f, 130) { Ink = 0.04f };
            Body(p, Sd.Box(0, 0, 0.86f, 0.33f, 0.14f), Rgba.Hex("8A5BD6"), Rgba.Hex("6B3FB5"));
            foreach (var s in new[] { (-0.72f, 0.18f), (0.74f, -0.17f) }) p.Fill(Sd.Circle(s.Item1, s.Item2, 0.04f), Rgba.Hex("FFE58A"));
            return new DoodleArt("s41b_spell_btn", p);
        }

        // ---------- Don't wake the bomb ----------

        /// <summary>Cameretta di notte (pavimento sotto y -3.2).</summary>
        public static DoodleArt S41B_BombBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.Fill(Sd.Box(0, 0, 5f, 7.5f), Rgba.Hex("2E3561"), false);
            for (float x = -4.2f; x < 4.5f; x += 0.7f) for (float y = -3f; y < 7f; y += 0.7f) p.Fill(Sd.Circle(x + (y * 1.3f % 0.35f), y, 0.04f), Rgba.Hex("3E4678"));
            // finestra con la luna
            p.Shape(Sd.Box(1.5f, 1.6f, 0.75f, 0.7f, 0.05f), Rgba.Hex("6B4A2E"));
            p.Fill(Sd.Box(1.5f, 1.6f, 0.63f, 0.58f), Rgba.Hex("141A3A"));
            p.Shape(Sd.Subtract(Sd.Circle(1.75f, 1.85f, 0.22f), Sd.Circle(1.85f, 1.92f, 0.19f)), Rgba.Hex("FFF3B0"), 0.025f);
            S41B_Stars(p, 0.95f, 1.1f, 2.05f, 2.1f, 8, 3, 0.012f);
            p.InkLine(0.04f, 1.5f, 1.0f, 1.5f, 2.2f);
            p.InkLine(0.04f, 0.9f, 1.6f, 2.1f, 1.6f);
            // pavimento e tappeto
            p.Shape(Sd.Box(0, -5.1f, 5f, 1.9f), Rgba.Hex("5A4060"));
            p.Fill(Sd.Ellipse(0, -3.7f, 2.4f, 0.45f), Rgba.Hex("7A5A8A"));
            // lucina da notte
            p.Fill(Sd.Circle(-2f, 1.2f, 0.9f), Rgba.Hex("FFE08A", 0.08f));
            p.Shape(Sd.Circle(-2f, 1.2f, 0.18f), Rgba.Hex("FFE08A"), 0.03f);
            return new DoodleArt("s41b_bomb_bg", p);
        }

        /// <summary>Cuscino (2.8 x 0.9, centro 0,0).</summary>
        public static DoodleArt S41B_Pillow()
        {
            var p = new DoodlePainter(-1.5f, -0.52f, 1.5f, 0.52f, 110) { Ink = 0.045f };
            Sdf pillow = Sd.Union(Sd.Box(0, 0, 1.25f, 0.36f, 0.3f), Sd.Circle(-1.25f, 0.3f, 0.12f), Sd.Circle(1.25f, 0.3f, 0.12f), Sd.Circle(-1.25f, -0.3f, 0.12f), Sd.Circle(1.25f, -0.3f, 0.12f));
            Body(p, pillow, Rgba.Hex("BFD6FF"), Rgba.Hex("93B3E6"));
            for (int i = -3; i <= 3; i++) p.Fill(Sd.Circle(i * 0.35f, 0.05f + (i % 2) * 0.12f, 0.035f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("s41b_pillow", p);
        }

        /// <summary>Bomba con la faccia (raggio 1, centro 0,0; foro della miccia in 0.42, 1.08): 0 dorme, 1 assonnata, 2 sveglia.</summary>
        public static DoodleArt S41B_Bomb(int mood)
        {
            var p = new DoodlePainter(-1.1f, -1.1f, 1.1f, 1.25f, 120) { Ink = 0.05f };
            Sdf cap = Sd.Rotate(Sd.Box(0.36f, 0.98f, 0.17f, 0.13f, 0.03f), 0.36f, 0.98f, -25f);
            p.Shape(cap, Rgba.Hex("8A8FA0"));
            Body(p, Sd.Circle(0, 0, 0.95f), Rgba.Hex("3A3F52"), Rgba.Hex("262A38"));
            p.Fill(Sd.Ellipse(-0.42f, 0.42f, 0.2f, 0.12f), Rgba.White.WithAlpha(0.35f));
            p.Fill(Sd.Circle(-0.6f, -0.15f, 0.12f), Rgba.Hex("FF8FA3", 0.5f));
            p.Fill(Sd.Circle(0.6f, -0.15f, 0.12f), Rgba.Hex("FF8FA3", 0.5f));
            Rgba lid = Rgba.Hex("E8E8F0");
            foreach (float x in new[] { -0.3f, 0.3f })
            {
                if (mood == 0) p.Line(lid, 0.05f, x - 0.15f, 0.05f, x, -0.02f, x + 0.15f, 0.05f);
                else if (mood == 1)
                {
                    p.Shape(Sd.Intersect(Sd.Circle(x, 0.02f, 0.13f), Sd.Box(x, -0.05f, 0.2f, 0.07f)), Rgba.White, 0.025f);
                    p.Fill(Sd.Circle(x, -0.06f, 0.05f), Ink);
                    p.Line(lid, 0.04f, x - 0.15f, 0.02f, x + 0.15f, 0.02f);
                }
                else
                {
                    p.Shape(Sd.Circle(x, 0.05f, 0.17f), Rgba.White, 0.03f);
                    p.Fill(Sd.Circle(x, 0.03f, 0.07f), Ink);
                    p.Line(Rgba.Hex("FF6B5A"), 0.05f, x - 0.18f * Math.Sign(x), 0.32f, x + 0.12f * Math.Sign(x), 0.24f);
                }
            }
            if (mood == 0) p.Shape(Sd.Ellipse(0, -0.35f, 0.07f, 0.05f), Rgba.Hex("7A2E3A"), 0.02f);
            else if (mood == 1) p.Line(lid, 0.035f, -0.12f, -0.35f, 0.12f, -0.33f);
            else p.Shape(Sd.Ellipse(0, -0.38f, 0.18f, 0.12f), Rgba.Hex("7A2E3A"), 0.03f);
            return new DoodleArt(mood == 0 ? "s41b_bomb_sleep" : mood == 1 ? "s41b_bomb_drowsy" : "s41b_bomb_awake", p);
        }

        /// <summary>Punta della miccia sfilacciata (raggio 0.18).</summary>
        public static DoodleArt S41B_FuseTip()
        {
            var p = new DoodlePainter(-0.24f, -0.24f, 0.24f, 0.24f, 200) { Ink = 0.025f };
            p.Fill(Sd.Circle(0, 0, 0.22f), Rgba.Hex("FFE08A", 0.3f));
            p.Shape(Sd.Circle(0, 0, 0.09f), Rgba.Hex("C9A46A"));
            for (int i = 0; i < 5; i++)
            {
                float a = i / 5f * 6.28f;
                p.InkLine(0.018f, S41B_Cos(a) * 0.08f, S41B_Sin(a) * 0.08f, S41B_Cos(a) * 0.16f, S41B_Sin(a) * 0.16f);
            }
            return new DoodleArt("s41b_fuse_tip", p);
        }

        // ---------- Upside-down king ----------

        /// <summary>Sala del trono: soffitto a travi sopra y 2.5, pavimento sotto y -2.9, muri oltre x ±2.55.</summary>
        public static DoodleArt S41B_ThroneBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.Fill(Sd.Box(0, 0, 5f, 7.5f), Rgba.Hex("B9AFC4"), false);
            for (float y = -2.9f; y < 2.5f; y += 0.5f)
                for (float x = -2.6f + ((int)((y + 3f) / 0.5f) % 2) * 0.35f; x < 2.6f; x += 0.7f)
                    p.Shape(Sd.Box(x, y + 0.25f, 0.33f, 0.22f, 0.04f), Rgba.Hex("A79DB5"), 0.02f);
            // stendardi
            foreach (float x in new[] { -1.1f, 0.4f })
            {
                p.Shape(Sd.Polygon(x - 0.3f, 2.4f, x + 0.3f, 2.4f, x + 0.3f, 0.6f, x, 0.85f, x - 0.3f, 0.6f), Rgba.Hex("C02A3A"));
                p.Shape(Sd.Circle(x, 1.7f, 0.12f), Gold, 0.02f);
            }
            // soffitto di legno
            p.Shape(Sd.Box(0, 4.75f, 5f, 2.25f), Rgba.Hex("7A4E2D"));
            for (float x = -4.2f; x < 4.5f; x += 0.9f) p.Shape(Sd.Box(x, 2.6f, 0.12f, 0.12f), Rgba.Hex("5A3A1E"), 0.03f);
            // muri laterali
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Box(s * 3.65f, 0, 1.1f, 7.5f), Rgba.Hex("8A7F99"));
            // pavimento e tappeto
            p.Shape(Sd.Box(0, -5f, 5f, 2.1f), Rgba.Hex("8F6B4A"));
            p.Fill(Sd.Box(0, -3.3f, 2.5f, 0.25f), Rgba.Hex("C02A3A"));
            p.Fill(Sd.Box(0, -3.3f, 2.5f, 0.04f), Gold);
            return new DoodleArt("s41b_throne_bg", p);
        }

        /// <summary>Trono (largo 1.3, alto 2; sedile in 0, -0.2).</summary>
        public static DoodleArt S41B_Throne()
        {
            var p = new DoodlePainter(-0.75f, -1.05f, 0.75f, 1.1f, 110) { Ink = 0.045f };
            Body(p, Sd.Box(0, 0.35f, 0.55f, 0.7f, 0.2f), Rgba.Hex("D6453B"), Rgba.Hex("A8302A"));
            p.Shape(Sd.Box(0, 0.35f, 0.4f, 0.55f, 0.15f), Rgba.Hex("E8635A"), 0.03f);
            foreach (float x in new[] { -0.55f, 0f, 0.55f }) p.Shape(Sd.Circle(x, 1.0f, 0.08f), Gold, 0.025f);
            p.Shape(Sd.Box(0, -0.3f, 0.65f, 0.14f, 0.05f), Gold);
            foreach (float x in new[] { -0.55f, 0.55f }) p.Shape(Sd.Box(x, -0.7f, 0.08f, 0.3f), GoldShade);
            return new DoodleArt("s41b_throne", p);
        }

        /// <summary>Re pelato senza corona (alto 1.3, centro 0,0; cima della testa in 0.62).</summary>
        public static DoodleArt S41B_King()
        {
            var p = new DoodlePainter(-0.6f, -0.7f, 0.6f, 0.7f, 150) { Ink = 0.04f };
            p.Shape(Sd.Polygon(-0.5f, -0.6f, 0.5f, -0.6f, 0.3f, 0.2f, -0.3f, 0.2f), Rgba.Hex("7A3FB5"));
            p.Fill(Sd.Box(0, -0.58f, 0.5f, 0.05f), Rgba.White);
            for (float x = -0.4f; x < 0.45f; x += 0.16f) p.Fill(Sd.Circle(x, -0.57f, 0.02f), Ink);
            Body(p, Sd.Box(0, -0.1f, 0.26f, 0.32f, 0.12f), Rgba.Hex("F2C14E"), Rgba.Hex("D19A2A"));
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Capsule(s * 0.22f, 0.05f, s * 0.4f, 0.3f, 0.06f), Rgba.Hex("F2C14E"), 0.025f);
            Sdf head = Sd.Circle(0, 0.38f, 0.24f);
            Body(p, head, Rgba.Hex("FFD7B5"), Rgba.Hex("E8B48C"), null, -0.05f, 0.05f);
            // barba bianca
            p.Shape(Sd.Union(Sd.Circle(-0.12f, 0.22f, 0.1f), Sd.Circle(0.12f, 0.22f, 0.1f), Sd.Circle(0, 0.16f, 0.12f)), Rgba.White, 0.025f);
            S41B_Eyes(p, -0.09f, 0.09f, 0.42f, 0.05f, 0.3f, 0.8f);
            p.Shape(Sd.Circle(0, 0.33f, 0.045f), Rgba.Hex("F2A08A"), 0.015f);
            p.InkLine(0.02f, -0.02f, 0.62f, 0.02f, 0.68f, 0.06f, 0.63f);   // ciuffo
            return new DoodleArt("s41b_king", p);
        }

        /// <summary>Scala di legno (larga 0.9, base in y 0, cima in 3.9).</summary>
        public static DoodleArt S41B_Ladder()
        {
            var p = new DoodlePainter(-0.52f, -0.06f, 0.52f, 3.98f, 100) { Ink = 0.04f };
            foreach (float x in new[] { -0.38f, 0.38f }) p.Shape(Sd.Box(x, 1.95f, 0.07f, 1.95f, 0.03f), Rgba.Hex("C98A4A"));
            for (int i = 0; i < 9; i++) p.Shape(Sd.Box(0, 0.3f + i * 0.42f, 0.32f, 0.05f, 0.02f), Rgba.Hex("E0A866"), 0.03f);
            return new DoodleArt("s41b_ladder", p);
        }

        /// <summary>Freccia tonda (verso destra; si specchia per la sinistra).</summary>
        public static DoodleArt S41B_ArrowButton()
        {
            var p = new DoodlePainter(-0.48f, -0.48f, 0.48f, 0.48f, 140) { Ink = 0.04f };
            Body(p, Sd.Circle(0, 0, 0.42f), Rgba.Hex("FFD25A"), Rgba.Hex("E3A92A"));
            p.Shape(Sd.Polygon(-0.15f, -0.2f, 0.22f, 0f, -0.15f, 0.2f), Rgba.White, 0.03f);
            return new DoodleArt("s41b_arrow_btn", p);
        }
    }
}
