using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli della specifica 105-200, pacchetto A.</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV41A() => new DoodleArt[]
        {
            // Don't touch me
            S41A_ButtonBase(), S41A_Dome("s41a_btn_dome_red", "E8413C", "B82A2A", false), S41A_Dome("s41a_btn_dome_purple", "9B4FD0", "6E2FA0", true),
            S41A_DomeFlat(), S41A_Sign(),
            // Balloon surgery
            S41A_Spikes(), S41A_Ledge(),
            // The lazy elevator
            S41A_TowerBackdrop(), S41A_LiftPanel(), S41A_BrokenButton(),
            // Phone sandwich
            S41A_GalleyBackdrop(), S41A_Plate(), S41A_BreadBottom(), S41A_BreadTop(), S41A_Cheese(), S41A_Lettuce(), S41A_Tomato(),
            // Left behind
            S41A_IslandBackdrop(),
            // Sleeping guard
            S41A_JailBackdrop(), S41A_Guard(false), S41A_Guard(true), S41A_CellDoor(),
            // The broken compass
            S41A_HedgeBackdrop(), S41A_CompassBody(), S41A_CompassBezel(), S41A_CompassNeedle(),
            // Fake update
            S41A_MonitorBackdrop(), S41A_UpdateWindow(), S41A_UpdateGear(),
        };

        static readonly Rgba S41A_Ink = DoodlePainter.DefaultInk;
        static float S41A_Sin(float v) => (float)Math.Sin(v);
        static float S41A_Cos(float v) => (float)Math.Cos(v);

        /// <summary>Occhio bianco con pupilla (dx, dy = spostamento della pupilla).</summary>
        static void S41A_Eye(DoodlePainter p, float x, float y, float rx, float ry, float dx = 0f, float dy = 0f)
        {
            p.Shape(Sd.Ellipse(x, y, rx, ry), Rgba.White, 0.03f);
            p.Fill(Sd.Circle(x + dx, y + dy, Math.Min(rx, ry) * 0.5f), S41A_Ink);
            p.Fill(Sd.Circle(x + dx - 0.02f, y + dy + 0.025f, Math.Min(rx, ry) * 0.16f), Rgba.White, false);
        }

        // ---------- Don't touch me ----------

        /// <summary>Basamento grigio del pulsante (largo 2.7, la cima in y 0).</summary>
        public static DoodleArt S41A_ButtonBase()
        {
            var p = new DoodlePainter(-1.45f, -0.7f, 1.45f, 0.1f, 110) { Ink = 0.05f };
            Body(p, Sd.Box(0, -0.32f, 1.35f, 0.3f, 0.12f), Rgba.Hex("8E92A3"), Rgba.Hex("6B6F80"));
            p.Shape(Sd.Box(0, -0.02f, 1.18f, 0.06f, 0.04f), Rgba.Hex("4E5160"), 0.035f);
            foreach (float x in new[] { -1.05f, 1.05f }) p.Shape(Sd.Circle(x, -0.36f, 0.06f), Rgba.Hex("C9CCD6"), 0.025f);
            return new DoodleArt("s41a_btn_base", p);
        }

        /// <summary>Cupola del pulsante con occhi arrabbiati (base in y 0, larga 2.1, alta 1).</summary>
        public static DoodleArt S41A_Dome(string name, string fill, string shade, bool sweating)
        {
            var p = new DoodlePainter(-1.15f, -0.08f, 1.15f, 1.12f, 120) { Ink = 0.05f };
            Sdf dome = Sd.Intersect(Sd.Ellipse(0, 0, 1.02f, 0.98f), Sd.Box(0, 0.6f, 1.2f, 0.6f));
            Body(p, dome, Rgba.Hex(fill), Rgba.Hex(shade), null, -0.08f, 0.08f);
            p.Fill(Sd.Ellipse(-0.5f, 0.68f, 0.2f, 0.11f), Rgba.White.WithAlpha(0.55f));
            S41A_Eye(p, -0.34f, 0.47f, 0.17f, 0.2f, 0.04f, -0.04f);
            S41A_Eye(p, 0.34f, 0.47f, 0.17f, 0.2f, -0.04f, -0.04f);
            p.InkLine(0.09f, -0.6f, 0.8f, -0.17f, 0.66f);
            p.InkLine(0.09f, 0.6f, 0.8f, 0.17f, 0.66f);
            p.InkLine(0.055f, -0.22f, 0.17f, 0f, 0.26f, 0.22f, 0.17f);
            if (sweating)
                foreach (var d in new[] { (-0.8f, 0.55f), (0.78f, 0.62f) })
                {
                    p.Shape(Sd.Union(Sd.Circle(d.Item1, d.Item2, 0.07f), Sd.Polygon(d.Item1 - 0.065f, d.Item2 + 0.02f, d.Item1 + 0.065f, d.Item2 + 0.02f, d.Item1, d.Item2 + 0.16f)), Rgba.Hex("9FDBFF"), 0.025f);
                }
            return new DoodleArt(name, p);
        }

        /// <summary>Cupola sgonfia e arresa (occhi a X, lingua fuori; base in y 0).</summary>
        public static DoodleArt S41A_DomeFlat()
        {
            var p = new DoodlePainter(-1.3f, -0.08f, 1.3f, 0.6f, 120) { Ink = 0.05f };
            Sdf blob = Sd.Intersect(Sd.Union(Sd.Ellipse(0, 0, 1.2f, 0.3f), Sd.Ellipse(-0.25f, 0.05f, 0.6f, 0.38f), Sd.Ellipse(0.35f, 0f, 0.5f, 0.33f)), Sd.Box(0, 0.4f, 1.3f, 0.4f));
            Body(p, blob, Rgba.Hex("B884E0"), Rgba.Hex("8A58B8"));
            foreach (float x in new[] { -0.3f, 0.25f })
            {
                p.InkLine(0.05f, x - 0.09f, 0.29f, x + 0.09f, 0.13f);
                p.InkLine(0.05f, x - 0.09f, 0.13f, x + 0.09f, 0.29f);
            }
            p.Shape(Sd.Capsule(0.02f, 0.06f, 0.1f, -0.02f, 0.06f), Rgba.Hex("FF8FA8"), 0.025f);
            p.InkLine(0.035f, -0.85f, 0.1f, -0.7f, 0.16f);
            p.InkLine(0.035f, 0.75f, 0.12f, 0.92f, 0.08f);
            return new DoodleArt("s41a_btn_dome_flat", p);
        }

        /// <summary>Cartello di legno (3.6 x 1, centro 0,0; il testo è un'etichetta).</summary>
        public static DoodleArt S41A_Sign()
        {
            var p = new DoodlePainter(-1.9f, -0.6f, 1.9f, 0.6f, 100) { Ink = 0.05f };
            Sdf board = Sd.Box(0, 0, 1.8f, 0.5f, 0.1f);
            Body(p, board, Rgba.Hex("E2AE72"), Rgba.Hex("BC8450"));
            p.Line(S41A_Ink.WithAlpha(0.25f), 0.025f, -1.7f, 0.02f, 1.7f, -0.02f);
            foreach (var n in new[] { (-1.6f, 0.32f), (1.6f, 0.32f), (-1.6f, -0.32f), (1.6f, -0.32f) })
                p.Shape(Sd.Circle(n.Item1, n.Item2, 0.05f), Rgba.Hex("C9CCD6"), 0.02f);
            return new DoodleArt("s41a_sign", p);
        }

        // ---------- Balloon surgery ----------

        /// <summary>Striscia di punte metalliche (larga 2, base in y 0, punte alte 0.5).</summary>
        public static DoodleArt S41A_Spikes()
        {
            var p = new DoodlePainter(-1.05f, -0.12f, 1.05f, 0.62f, 110) { Ink = 0.035f };
            for (int i = 0; i < 6; i++)
            {
                float x = -0.835f + i * 0.334f;
                Sdf s = Sd.Polygon(x - 0.16f, 0.08f, x + 0.16f, 0.08f, x, 0.56f);
                Body(p, s, Rgba.Hex("D4DAE3"), Rgba.Hex("8E97A6"), null, 0.05f, 0.02f);
            }
            p.Shape(Sd.Box(0, 0.02f, 1.0f, 0.08f, 0.03f), Rgba.Hex("5E6475"));
            return new DoodleArt("s41a_spikes", p);
        }

        /// <summary>Colonna di pietra con l'erba in cima (piano in y 0, larga 1.5, base in y -2.2).</summary>
        public static DoodleArt S41A_Ledge()
        {
            var p = new DoodlePainter(-0.85f, -2.25f, 0.85f, 0.15f, 100) { Ink = 0.05f };
            Sdf col = Sd.Box(0, -1.15f, 0.48f, 1.05f, 0.04f);
            Body(p, col, Rgba.Hex("B3ADC2"), Rgba.Hex("8A839C"));
            for (int r = 0; r < 6; r++)
            {
                float y = -2.1f + r * 0.33f;
                p.Line(S41A_Ink.WithAlpha(0.35f), 0.025f, -0.45f, y, 0.45f, y);
                float x = (r % 2 == 0) ? -0.1f : 0.18f;
                p.Line(S41A_Ink.WithAlpha(0.35f), 0.025f, x, y, x, y + 0.33f);
            }
            Sdf cap = Sd.Box(0, -0.12f, 0.75f, 0.13f, 0.06f);
            Body(p, cap, Rgba.Hex("C9C3D6"), Rgba.Hex("9C95AE"));
            p.Shape(Sd.Union(Sd.Box(0, 0.0f, 0.76f, 0.05f, 0.04f), Sd.Circle(-0.5f, -0.05f, 0.08f), Sd.Circle(0.4f, -0.06f, 0.07f)), Rgba.Hex("7BC96F"), 0.035f);
            return new DoodleArt("s41a_ledge", p);
        }

        // ---------- The lazy elevator ----------

        /// <summary>
        /// Palazzo in sezione (coordinate del livello): vano dell'ascensore x -2.75..-1.65 (centro -2.2), stanze fino a
        /// x 0.3, piano k (0 = terra) con base in y -4.2 + 1.2 k, tetto in 3.0. A destra cielo e palazzi lontani.
        /// </summary>
        public static DoodleArt S41A_TowerBackdrop()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("BFE6FA"), Rgba.Hex("6FB4E6"), Clamp((y + 1f) / 6f)));
            // palazzi lontani a destra
            foreach (var b in new[] { (1.0f, -1.2f, 0.5f), (2.1f, -0.2f, 0.6f), (3.0f, -1.8f, 0.5f) })
            {
                p.Fill(Sd.Box(b.Item1, -4.5f + (b.Item2 + 4.5f) * 0.5f, b.Item3, (b.Item2 + 4.5f) * 0.5f), Rgba.Hex("9DB3D6"), false);
                for (float wy = -4f; wy < b.Item2 - 0.3f; wy += 0.5f)
                    p.Fill(Sd.Box(b.Item1, wy, b.Item3 * 0.6f, 0.08f), Rgba.Hex("C9D8EE"), false);
            }
            p.Fill(Sd.Union(Sd.Circle(1.6f, 3.6f, 0.35f), Sd.Circle(1.95f, 3.7f, 0.3f), Sd.Circle(1.3f, 3.55f, 0.25f)), Rgba.White.WithAlpha(0.9f));
            // marciapiede
            p.Fill(Sd.Box(0, -4.85f, 3.3f, 0.65f), Rgba.Hex("8B8698"), false);
            p.Shape(Sd.Box(0, -4.25f, 3.3f, 0.05f), Rgba.Hex("6B6677"));
            // facciata
            p.Shape(Sd.Box(-1.25f, -0.6f, 1.65f, 3.62f, 0.04f), Rgba.Hex("C9775A"));
            string[] walls = { "F3E3C2", "FFD8C9", "D9F0D3", "D7E4FA", "F7E0F2", "FFF1B5" };
            for (int k = 0; k <= 5; k++)
            {
                float y0 = -4.2f + k * 1.2f;
                p.Fill(Sd.Box(-0.65f, y0 + 0.6f, 0.92f, 0.52f), Rgba.Hex(walls[k]), false);
                for (float x = -1.45f; x < 0.25f; x += 0.3f) p.Line(S41A_Ink.WithAlpha(0.06f), 0.03f, x, y0 + 0.14f, x, y0 + 1.06f);
                p.Fill(Sd.Box(-0.65f, y0 + 0.07f, 0.92f, 0.06f), Rgba.Hex("A0663E"), false);
                if (k > 0)
                {
                    p.Shape(Sd.Box(-0.15f, y0 + 0.66f, 0.26f, 0.25f, 0.03f), Rgba.Hex("BFE6FF"), 0.035f);
                    p.InkLine(0.025f, -0.15f, y0 + 0.41f, -0.15f, y0 + 0.91f);
                }
                p.Shape(Sd.Box(-1.25f, y0 + 1.2f, 1.65f, 0.05f), Rgba.Hex("8E4B36"), 0.03f);
            }
            // vano
            p.FillFunc(Sd.Box(-2.2f, -0.6f, 0.55f, 3.58f), (x, y) => Rgba.Lerp(Rgba.Hex("3D3A4A"), Rgba.Hex("555066"), Clamp((x + 2.75f) / 1.1f)));
            foreach (float x in new[] { -2.7f, -1.7f }) p.Line(Rgba.Hex("2A2734"), 0.05f, x, -4.2f, x, 3.0f);
            p.Shape(Sd.Box(-1.6f, -0.6f, 0.05f, 3.6f), Rgba.Hex("8E4B36"), 0.03f);
            // tetto
            p.Shape(Sd.Polygon(-2.98f, 3.0f, 0.48f, 3.0f, 0.3f, 3.2f, -2.8f, 3.2f), Rgba.Hex("7A3F2C"));
            p.Shape(Sd.Box(-0.4f, 3.45f, 0.06f, 0.25f), Rgba.Hex("9C98AA"), 0.03f);
            return new DoodleArt("s41a_tower_bg", p);
        }

        /// <summary>Pulsantiera d'ottone (1.7 x 5.3, centro 0,0) col display in alto (centro y 2.15).</summary>
        public static DoodleArt S41A_LiftPanel()
        {
            var p = new DoodlePainter(-0.95f, -2.75f, 0.95f, 2.75f, 100) { Ink = 0.05f };
            Body(p, Sd.Box(0, 0, 0.85f, 2.65f, 0.18f), Gold, GoldShade);
            p.Shape(Sd.Box(0, -0.25f, 0.68f, 2.15f, 0.12f), Rgba.Hex("4A4560"), 0.04f);
            p.Shape(Sd.Box(0, 2.15f, 0.66f, 0.3f, 0.07f), Rgba.Hex("1E2A22"), 0.04f);
            p.Fill(Sd.Box(-0.35f, 2.3f, 0.2f, 0.05f), Rgba.White.WithAlpha(0.12f));
            foreach (var n in new[] { (-0.72f, 2.52f), (0.72f, 2.52f), (-0.72f, -2.52f), (0.72f, -2.52f) })
            {
                p.Shape(Sd.Circle(n.Item1, n.Item2, 0.06f), GoldLight, 0.02f);
                p.InkLine(0.02f, n.Item1 - 0.04f, n.Item2, n.Item1 + 0.04f, n.Item2);
            }
            return new DoodleArt("s41a_lift_panel", p);
        }

        /// <summary>Pulsante rotto e incerottato (raggio 0.5).</summary>
        public static DoodleArt S41A_BrokenButton()
        {
            var p = new DoodlePainter(-0.58f, -0.58f, 0.58f, 0.58f, 160) { Ink = 0.045f };
            Body(p, Sd.Circle(0, 0, 0.5f), Rgba.Hex("B8B2A6"), Rgba.Hex("8E887C"));
            p.Shape(Sd.Circle(0, 0, 0.37f), Rgba.Hex("D8D2C6"), 0.03f);
            p.InkLine(0.035f, -0.28f, 0.3f, -0.05f, 0.05f, -0.15f, -0.08f, 0.12f, -0.33f);
            p.InkLine(0.03f, -0.05f, 0.05f, 0.2f, 0.12f);
            foreach (float a in new[] { 40f, -40f })
                p.Shape(Sd.Rotate(Sd.Box(0, 0, 0.52f, 0.1f, 0.02f), 0, 0, a), Rgba.Hex("F5E3B8").WithAlpha(0.95f), 0.025f);
            return new DoodleArt("s41a_lift_btn_broken", p);
        }

        // ---------- Phone sandwich ----------

        /// <summary>Cucina di una stazione spaziale (oblò in alto a destra, niente pavimento).</summary>
        public static DoodleArt S41A_GalleyBackdrop()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("A9B6D3"), Rgba.Hex("DCE3F0"), Clamp((y + 5f) / 10f)));
            for (float y = -4.6f; y < 5f; y += 1.6f) p.Line(S41A_Ink.WithAlpha(0.15f), 0.04f, -3.2f, y, 3.2f, y);
            foreach (float x in new[] { -1.6f, 0f, 1.6f })
                for (float y = -4.6f; y < 5f; y += 1.6f) p.Line(S41A_Ink.WithAlpha(0.12f), 0.035f, x, y, x, y + 1.6f);
            for (float y = -4.5f; y < 5f; y += 0.8f)
                foreach (float x in new[] { -2.9f, 2.9f }) p.Fill(Sd.Circle(x, y, 0.05f), S41A_Ink.WithAlpha(0.25f));
            // oblò con le stelle e la Terra
            p.Shape(Sd.Circle(1.55f, 2.6f, 0.95f), Rgba.Hex("C9CCD6"), 0.06f);
            p.FillFunc(Sd.Circle(1.55f, 2.6f, 0.75f), (x, y) => Rgba.Lerp(Rgba.Hex("0C1030"), Rgba.Hex("232A66"), Clamp((y - 1.8f) / 1.6f)));
            p.Fill(Sd.Intersect(Sd.Circle(1.25f, 1.6f, 0.75f), Sd.Circle(1.55f, 2.6f, 0.75f)), Rgba.Hex("3E8EDE"));
            p.Fill(Sd.Intersect(Sd.Union(Sd.Ellipse(1.1f, 2.1f, 0.25f, 0.12f), Sd.Ellipse(1.45f, 2.0f, 0.2f, 0.1f)), Sd.Circle(1.25f, 1.6f, 0.75f)), Rgba.Hex("6CC56A"));
            foreach (var s in new[] { (1.9f, 3.0f), (1.4f, 3.15f), (2.05f, 2.5f), (1.65f, 2.75f), (1.15f, 2.85f) })
                p.Fill(Sd.Circle(s.Item1, s.Item2, 0.03f), Rgba.White);
            p.Shape(Sd.Subtract(Sd.Circle(1.55f, 2.6f, 0.82f), Sd.Circle(1.55f, 2.6f, 0.75f)), Rgba.Hex("8E92A3"), 0.03f);
            // corrimano e strisce gialle
            p.Shape(Sd.Capsule(-2.6f, 0.9f, -0.6f, 0.9f, 0.06f), Rgba.Hex("E39B1B"), 0.03f);
            p.Shape(Sd.Box(-1.7f, 3.0f, 0.9f, 0.32f, 0.06f), Rgba.Hex("F2F4F8"), 0.04f);
            for (float x = -2.45f; x < -0.9f; x += 0.3f) p.Fill(Sd.Intersect(Sd.Rotate(Sd.Box(x, 3.0f, 0.07f, 0.4f), x, 3.0f, 35f), Sd.Box(-1.7f, 3.0f, 0.85f, 0.27f)), Rgba.Hex("FFC93C"));
            // ventola e spia
            p.Shape(Sd.Circle(-2.2f, -1.6f, 0.4f), Rgba.Hex("8E92A3"), 0.04f);
            for (int i = 0; i < 4; i++) p.Fill(Sd.Rotate(Sd.Ellipse(-2.2f, -1.4f, 0.08f, 0.17f), -2.2f, -1.6f, i * 90f + 20f), Rgba.Hex("5E6475"));
            p.Shape(Sd.Circle(2.4f, -1.2f, 0.12f), Rgba.Hex("7BEA8A"), 0.03f);
            return new DoodleArt("s41a_galley_bg", p);
        }

        /// <summary>Piatto visto di lato (largo 2.4, bordo superiore in y 0).</summary>
        public static DoodleArt S41A_Plate()
        {
            var p = new DoodlePainter(-1.3f, -0.4f, 1.3f, 0.12f, 120) { Ink = 0.045f };
            p.Shape(Sd.Ellipse(0, -0.12f, 1.2f, 0.2f), Rgba.Hex("F4F4F8"));
            p.Shape(Sd.Ellipse(0, -0.06f, 0.85f, 0.1f), Rgba.Hex("DDE6F2"), 0.03f);
            p.Shape(Sd.Box(0, -0.3f, 0.45f, 0.06f, 0.04f), Rgba.Hex("C9D4E6"), 0.035f);
            return new DoodleArt("s41a_plate", p);
        }

        /// <summary>Fetta di pane di sotto (1.8 x 0.32, centro 0,0).</summary>
        public static DoodleArt S41A_BreadBottom()
        {
            var p = new DoodlePainter(-1.0f, -0.24f, 1.0f, 0.24f, 140) { Ink = 0.045f };
            p.Shape(Sd.Box(0, 0, 0.9f, 0.16f, 0.1f), Rgba.Hex("C98A4B"));
            p.Fill(Sd.Box(0, 0.01f, 0.8f, 0.09f, 0.06f), Rgba.Hex("F5DCA6"));
            foreach (float x in new[] { -0.5f, -0.1f, 0.35f, 0.6f }) p.Fill(Sd.Ellipse(x, 0.02f, 0.04f, 0.02f), Rgba.Hex("D9B77A"));
            return new DoodleArt("s41a_bread_bottom", p);
        }

        /// <summary>Fetta di pane di sopra, bombata coi semini (1.8 x 0.4, centro 0,0).</summary>
        public static DoodleArt S41A_BreadTop()
        {
            var p = new DoodlePainter(-1.0f, -0.26f, 1.0f, 0.3f, 140) { Ink = 0.045f };
            Sdf top = Sd.Union(Sd.Box(0, -0.1f, 0.9f, 0.1f, 0.08f), Sd.Intersect(Sd.Ellipse(0, -0.08f, 0.9f, 0.3f), Sd.Box(0, 0.1f, 1f, 0.2f)));
            Body(p, top, Rgba.Hex("D9964F"), Rgba.Hex("B0703A"));
            p.Fill(Sd.Box(0, -0.13f, 0.8f, 0.04f, 0.03f), Rgba.Hex("F5DCA6"));
            foreach (var s in new[] { (-0.5f, 0.05f), (-0.15f, 0.14f), (0.2f, 0.12f), (0.5f, 0.03f), (0f, 0.0f) })
                p.Fill(Sd.Rotate(Sd.Ellipse(s.Item1, s.Item2, 0.04f, 0.022f), s.Item1, s.Item2, 20f), Rgba.Hex("FFF6DE"));
            return new DoodleArt("s41a_bread_top", p);
        }

        /// <summary>Fetta di formaggio che cola (1.7 x 0.16, centro 0,0).</summary>
        public static DoodleArt S41A_Cheese()
        {
            var p = new DoodlePainter(-0.95f, -0.3f, 0.95f, 0.14f, 140) { Ink = 0.04f };
            Sdf c = Sd.Union(Sd.Box(0, 0, 0.85f, 0.08f, 0.03f), Sd.Capsule(-0.5f, 0f, -0.5f, -0.18f, 0.06f), Sd.Capsule(0.35f, 0f, 0.35f, -0.12f, 0.05f), Sd.Capsule(0.78f, 0f, 0.82f, -0.2f, 0.05f));
            p.Shape(c, Rgba.Hex("FFD24A"));
            foreach (float x in new[] { -0.7f, -0.15f, 0.55f }) p.Fill(Sd.Ellipse(x, 0.01f, 0.06f, 0.035f), Rgba.Hex("E8AE2A"));
            return new DoodleArt("s41a_cheese", p);
        }

        /// <summary>Foglia d'insalata ondulata (1.9 x 0.22, centro 0,0).</summary>
        public static DoodleArt S41A_Lettuce()
        {
            var p = new DoodlePainter(-1.05f, -0.2f, 1.05f, 0.2f, 140) { Ink = 0.04f };
            var parts = new List<Sdf> { Sd.Box(0, 0, 0.85f, 0.06f, 0.03f) };
            for (int i = 0; i < 13; i++) { float x = -0.9f + i * 0.15f; parts.Add(Sd.Circle(x, (i % 2 == 0) ? 0.03f : -0.03f, 0.1f)); }
            Sdf l = Sd.Union(parts.ToArray());
            Body(p, l, Rgba.Hex("8ED465"), Rgba.Hex("5EA845"));
            p.Line(Rgba.Hex("D7F5B5"), 0.03f, -0.85f, 0f, 0.85f, 0f);
            return new DoodleArt("s41a_lettuce", p);
        }

        /// <summary>Due fette di pomodoro (1.7 x 0.16, centro 0,0).</summary>
        public static DoodleArt S41A_Tomato()
        {
            var p = new DoodlePainter(-0.95f, -0.15f, 0.95f, 0.15f, 140) { Ink = 0.04f };
            foreach (float x in new[] { -0.43f, 0.43f })
            {
                p.Shape(Sd.Box(x, 0, 0.41f, 0.08f, 0.07f), Rgba.Hex("E84A3A"));
                p.Fill(Sd.Box(x, 0.0f, 0.33f, 0.035f, 0.03f), Rgba.Hex("FF8A70"));
                foreach (float s in new[] { -0.2f, 0f, 0.2f }) p.Fill(Sd.Ellipse(x + s, 0f, 0.035f, 0.02f), Rgba.Hex("FFE6A8"));
            }
            return new DoodleArt("s41a_tomato", p);
        }

        // ---------- Left behind ----------

        /// <summary>Isolotto di roccia nel cielo del tramonto (coordinate del livello: piano in y -2.4).</summary>
        public static DoodleArt S41A_IslandBackdrop()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("FFB38A"), Rgba.Hex("6B5BA8"), Clamp((y + 3f) / 8f)));
            foreach (var s in new[] { (-2.2f, 4.2f), (-0.6f, 3.6f), (1.2f, 4.4f), (2.5f, 3.3f), (0.4f, 2.6f), (-1.9f, 2.4f) })
                p.Fill(Sd.Circle(s.Item1, s.Item2, 0.04f), Rgba.White.WithAlpha(0.85f));
            p.Fill(Sd.Union(Sd.Ellipse(-2.0f, -0.6f, 0.9f, 0.18f), Sd.Ellipse(-1.5f, -0.5f, 0.5f, 0.14f)), Rgba.White.WithAlpha(0.5f));
            p.Fill(Sd.Union(Sd.Ellipse(2.1f, 0.4f, 0.8f, 0.16f), Sd.Ellipse(2.5f, 0.5f, 0.4f, 0.12f)), Rgba.White.WithAlpha(0.5f));
            p.Fill(Sd.Union(Sd.Ellipse(-1.2f, -4.6f, 1.6f, 0.3f), Sd.Ellipse(1.6f, -4.8f, 1.4f, 0.3f)), Rgba.White.WithAlpha(0.6f));
            // isola
            Sdf rock = Sd.Polygon(-3.0f, -2.4f, 3.0f, -2.4f, 2.6f, -3.0f, 1.8f, -3.3f, 1.0f, -3.9f, 0.2f, -4.3f, -0.6f, -4.0f, -1.5f, -3.5f, -2.4f, -3.1f);
            Body(p, rock, Rgba.Hex("9A8A7A"), Rgba.Hex("6E5F52"));
            p.Shape(Sd.Box(0, -2.42f, 3.05f, 0.1f, 0.05f), Rgba.Hex("7BC96F"), 0.04f);
            foreach (var c in new[] { (-1.6f, -3.0f), (0.6f, -3.4f), (1.5f, -2.9f), (-0.3f, -3.7f) })
                p.InkLine(0.03f, c.Item1, c.Item2, c.Item1 + 0.25f, c.Item2 - 0.12f, c.Item1 + 0.4f, c.Item2 - 0.08f);
            return new DoodleArt("s41a_island_bg", p);
        }

        // ---------- Sleeping guard ----------

        /// <summary>Prigione: muro di pietra, finestrella con le sbarre, pavimento da y -3.4.</summary>
        public static DoodleArt S41A_JailBackdrop()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.Fill(Sd.Box(0, 1f, 3.3f, 4.5f), Rgba.Hex("8F88A0"), false);
            for (int r = 0; r < 17; r++)
            {
                float y = -3.4f + r * 0.5f;
                p.Line(S41A_Ink.WithAlpha(0.22f), 0.035f, -3.2f, y, 3.2f, y);
                for (float x = -3.2f + (r % 2) * 0.45f; x < 3.3f; x += 0.9f) p.Line(S41A_Ink.WithAlpha(0.22f), 0.035f, x, y, x, y + 0.5f);
            }
            p.Fill(Sd.Box(0, -4.3f, 3.3f, 0.9f), Rgba.Hex("5E566B"), false);
            p.Shape(Sd.Box(0, -3.4f, 3.3f, 0.05f), Rgba.Hex("3E384A"));
            // finestrella con la luna
            p.Shape(Sd.Box(-1.4f, 2.6f, 0.55f, 0.45f, 0.06f), Rgba.Hex("1E2448"));
            p.Fill(Sd.Subtract(Sd.Circle(-1.2f, 2.75f, 0.2f), Sd.Circle(-1.12f, 2.82f, 0.17f)), Rgba.Hex("FFF1B5"));
            foreach (float x in new[] { -1.7f, -1.4f, -1.1f }) p.Shape(Sd.Box(x, 2.6f, 0.035f, 0.45f), Rgba.Hex("5A5566"), 0.025f);
            // muffa e chiodo per le chiavi
            p.Fill(Sd.Ellipse(2.4f, -2.9f, 0.4f, 0.2f), Rgba.Hex("6E9E6A").WithAlpha(0.35f));
            return new DoodleArt("s41a_jail_bg", p);
        }

        /// <summary>Guardia seduta sullo sgabello (base in y 0, alta 2.7): addormentata o sveglia e arrabbiata.</summary>
        public static DoodleArt S41A_Guard(bool awake)
        {
            var p = new DoodlePainter(-1.15f, -0.08f, 1.15f, 2.75f, 110) { Ink = 0.05f };
            // sgabello
            foreach (float x in new[] { -0.5f, 0.5f }) p.Shape(Sd.Box(x, 0.3f, 0.06f, 0.3f), Rgba.Hex("8A5A3A"), 0.035f);
            p.Shape(Sd.Box(0, 0.62f, 0.7f, 0.08f, 0.04f), Rgba.Hex("A8744A"));
            // gambe e stivali
            foreach (float x in new[] { -0.35f, 0.35f })
            {
                p.Shape(Sd.Box(x, 0.45f, 0.15f, 0.3f, 0.06f), Rgba.Hex("2E3F78"), 0.04f);
                p.Shape(Sd.Box(x + 0.05f, 0.1f, 0.22f, 0.1f, 0.06f), Rgba.Hex("3A2A22"), 0.04f);
            }
            // pancia
            Body(p, Sd.Ellipse(0, 1.15f, 0.78f, 0.62f), Rgba.Hex("3E5BA8"), Rgba.Hex("2E4588"));
            p.Shape(Sd.Box(0, 0.8f, 0.6f, 0.07f, 0.03f), Rgba.Hex("3A2A22"), 0.03f);
            p.Shape(Sd.Box(0, 0.8f, 0.09f, 0.08f, 0.02f), Gold, 0.025f);
            foreach (float y in new[] { 1.05f, 1.3f, 1.55f }) p.Shape(Sd.Circle(0, y, 0.04f), Gold, 0.02f);
            p.Shape(Sd.Union(Sd.Circle(-0.35f, 1.5f, 0.11f), Sd.Polygon(-0.45f, 1.5f, -0.25f, 1.5f, -0.35f, 1.33f)), Gold, 0.025f);
            // braccia
            if (awake)
            {
                p.Shape(Sd.Capsule(-0.7f, 1.3f, -1.0f, 2.0f, 0.13f), Rgba.Hex("3E5BA8"), 0.04f);
                p.Shape(Sd.Circle(-1.0f, 2.08f, 0.13f), Rgba.Hex("F2C29B"), 0.035f);
                p.Shape(Sd.Capsule(0.7f, 1.3f, 0.95f, 1.0f, 0.13f), Rgba.Hex("3E5BA8"), 0.04f);
            }
            else
            {
                p.Shape(Sd.Capsule(-0.65f, 1.35f, -0.45f, 0.95f, 0.13f), Rgba.Hex("3E5BA8"), 0.04f);
                p.Shape(Sd.Capsule(0.65f, 1.35f, 0.45f, 0.95f, 0.13f), Rgba.Hex("3E5BA8"), 0.04f);
            }
            // testa
            float hx = 0f, hy = awake ? 2.12f : 2.02f;
            float tilt = awake ? 0f : -12f;
            Sdf head = Sd.Rotate(Sd.Ellipse(hx, hy, 0.42f, 0.4f), hx, hy, tilt);
            Body(p, head, Rgba.Hex("F2C29B"), Rgba.Hex("D99E78"));
            p.Shape(Sd.Circle(hx + 0.02f, hy - 0.05f, 0.08f), Rgba.Hex("E8A07E"), 0.03f);
            // baffi
            p.Shape(Sd.Union(Sd.Ellipse(hx - 0.13f, hy - 0.15f, 0.15f, 0.06f), Sd.Ellipse(hx + 0.15f, hy - 0.15f, 0.15f, 0.06f)), Rgba.Hex("5A3A2A"), 0.03f);
            if (awake)
            {
                S41A_Eye(p, hx - 0.17f, hy + 0.1f, 0.1f, 0.12f);
                S41A_Eye(p, hx + 0.17f, hy + 0.1f, 0.1f, 0.12f);
                p.InkLine(0.05f, hx - 0.3f, hy + 0.3f, hx - 0.08f, hy + 0.22f);
                p.InkLine(0.05f, hx + 0.3f, hy + 0.3f, hx + 0.08f, hy + 0.22f);
                p.Shape(Sd.Ellipse(hx, hy - 0.28f, 0.1f, 0.07f), Rgba.Hex("7A2A2A"), 0.03f);
            }
            else
            {
                p.InkLine(0.04f, hx - 0.27f, hy + 0.1f, hx - 0.17f, hy + 0.05f, hx - 0.07f, hy + 0.1f);
                p.InkLine(0.04f, hx + 0.07f, hy + 0.06f, hx + 0.17f, hy + 0.01f, hx + 0.27f, hy + 0.06f);
                p.Shape(Sd.Ellipse(hx + 0.02f, hy - 0.28f, 0.07f, 0.09f), Rgba.Hex("7A2A2A"), 0.03f);
            }
            // berretto
            float cx = hx - (awake ? 0f : 0.06f), cy = hy + 0.32f;
            Sdf cap = Sd.Rotate(Sd.Union(Sd.Box(cx, cy + 0.1f, 0.38f, 0.15f, 0.08f), Sd.Ellipse(cx + 0.12f, cy - 0.04f, 0.42f, 0.06f)), cx, cy, tilt);
            p.Shape(cap, Rgba.Hex("24356A"));
            p.Shape(Sd.Circle(cx, cy + 0.12f, 0.06f), Gold, 0.025f);
            return new DoodleArt(awake ? "s41a_guard_awake" : "s41a_guard_sleep", p);
        }

        /// <summary>Porta di ferro della cella (larga 1.5, base in y 0, alta 2.4; serratura in 0.45, 1.1).</summary>
        public static DoodleArt S41A_CellDoor()
        {
            var p = new DoodlePainter(-0.82f, -0.06f, 0.82f, 2.5f, 110) { Ink = 0.05f };
            Body(p, Sd.Box(0, 1.2f, 0.74f, 1.2f, 0.06f), Rgba.Hex("7A7488"), Rgba.Hex("5A5566"));
            p.Shape(Sd.Box(0, 1.85f, 0.4f, 0.28f, 0.04f), Rgba.Hex("1E2030"), 0.04f);
            foreach (float x in new[] { -0.2f, 0f, 0.2f }) p.Shape(Sd.Box(x, 1.85f, 0.03f, 0.28f), Rgba.Hex("9C98AA"), 0.02f);
            for (float y = 0.25f; y < 2.3f; y += 0.5f)
                foreach (float x in new[] { -0.62f, 0.62f }) p.Shape(Sd.Circle(x, y, 0.045f), Rgba.Hex("A9A4B8"), 0.02f);
            p.Shape(Sd.Box(0.45f, 1.1f, 0.18f, 0.26f, 0.05f), Gold, 0.035f);
            p.Fill(Sd.Union(Sd.Circle(0.45f, 1.15f, 0.06f), Sd.Polygon(0.42f, 1.13f, 0.48f, 1.13f, 0.5f, 0.98f, 0.4f, 0.98f)), S41A_Ink);
            return new DoodleArt("s41a_cell_door", p);
        }

        // ---------- The broken compass ----------

        /// <summary>Labirinto di siepi visto dall'alto: incrocio in (0, 1.4), corridoi larghi 1.1; radura per la bussola in basso.</summary>
        public static DoodleArt S41A_HedgeBackdrop()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.Fill(Sd.Box(0, 0, 3.3f, 5.3f), Rgba.Hex("3E8E4A"), false);
            for (int i = 0; i < 160; i++)
            {
                float x = -3.2f + (i * 0.739f % 6.4f), y = -5.2f + (i * 1.371f % 10.4f);
                p.Fill(Sd.Circle(x, y, 0.22f + (i % 3) * 0.05f), Rgba.Hex(i % 2 == 0 ? "4FA35A" : "347A40"));
            }
            Sdf paths = Sd.Union(Sd.Box(0, 1.4f, 3.4f, 0.55f, 0.1f), Sd.Box(0, 2.2f, 0.55f, 2.3f, 0.1f), Sd.Box(0, 0.2f, 0.55f, 1.0f, 0.1f));
            p.Shape(paths, Rgba.Hex("E8D3A2"));
            for (int i = 0; i < 40; i++)
            {
                float x = -2.9f + (i * 0.613f % 5.8f), y = 1.4f + ((i * 0.37f) % 1.0f) - 0.5f;
                p.Fill(Sd.Circle(x, y, 0.03f), Rgba.Hex("C9B07A"));
                p.Fill(Sd.Circle(((i * 0.29f) % 1.0f) - 0.5f, -0.7f + (i * 0.731f % 4.8f), 0.03f), Rgba.Hex("C9B07A"));
            }
            // radura per la bussola
            p.Shape(Sd.Ellipse(0, -2.75f, 2.2f, 1.75f), Rgba.Hex("8FCF7A"));
            foreach (var f in new[] { (-1.6f, -2.0f), (1.5f, -3.6f), (1.7f, -1.7f), (-1.3f, -3.9f) })
            {
                for (int k = 0; k < 5; k++) p.Fill(Sd.Circle(f.Item1 + S41A_Cos(k * 1.2566f) * 0.07f, f.Item2 + S41A_Sin(k * 1.2566f) * 0.07f, 0.05f), Rgba.White);
                p.Fill(Sd.Circle(f.Item1, f.Item2, 0.04f), Gold);
            }
            return new DoodleArt("s41a_hedge_bg", p);
        }

        /// <summary>Cassa d'ottone della bussola col vetro (raggio 1.2, anello in alto).</summary>
        public static DoodleArt S41A_CompassBody()
        {
            var p = new DoodlePainter(-1.32f, -1.32f, 1.32f, 1.58f, 110) { Ink = 0.05f };
            p.Shape(Sd.Subtract(Sd.Circle(0, 1.33f, 0.2f), Sd.Circle(0, 1.33f, 0.1f)), Gold, 0.04f);
            Body(p, Sd.Circle(0, 0, 1.22f), Gold, GoldShade);
            p.Shape(Sd.Circle(0, 0, 1.04f), Rgba.Hex("FFF6E0"), 0.04f);
            for (int i = 0; i < 8; i++)
            {
                float a = i * 0.785f;
                p.Line(S41A_Ink.WithAlpha(0.18f), 0.03f, S41A_Cos(a) * 0.15f, S41A_Sin(a) * 0.15f, S41A_Cos(a) * 0.72f, S41A_Sin(a) * 0.72f);
            }
            return new DoodleArt("s41a_compass_body", p);
        }

        /// <summary>Ghiera girevole (anello 0.8..1.0) con tacche e triangolo rosso del Nord; le lettere sono etichette.</summary>
        public static DoodleArt S41A_CompassBezel()
        {
            var p = new DoodlePainter(-1.08f, -1.08f, 1.08f, 1.08f, 130) { Ink = 0.035f };
            p.Shape(Sd.Subtract(Sd.Circle(0, 0, 1.0f), Sd.Circle(0, 0, 0.78f)), Rgba.Hex("2E4A7A"));
            for (int i = 0; i < 24; i++)
            {
                if (i % 6 == 0) continue;
                float a = i * 0.2618f, r0 = (i % 3 == 0) ? 0.8f : 0.84f;
                p.Line(Rgba.White.WithAlpha(0.7f), 0.022f, S41A_Cos(a) * r0, S41A_Sin(a) * r0, S41A_Cos(a) * 0.95f, S41A_Sin(a) * 0.95f);
            }
            p.Shape(Sd.Polygon(-0.12f, 0.62f, 0.12f, 0.62f, 0f, 0.77f), Rgba.Hex("E8413C"), 0.025f);
            return new DoodleArt("s41a_compass_bezel", p);
        }

        /// <summary>Ago della bussola verso l'alto (metà rossa = Nord, lungo 1.4).</summary>
        public static DoodleArt S41A_CompassNeedle()
        {
            var p = new DoodlePainter(-0.2f, -0.78f, 0.2f, 0.78f, 160) { Ink = 0.03f };
            p.Shape(Sd.Polygon(0f, 0.72f, 0.12f, 0f, -0.12f, 0f), Rgba.Hex("E8413C"));
            p.Shape(Sd.Polygon(0f, -0.72f, 0.12f, 0f, -0.12f, 0f), Rgba.Hex("F4F4F8"));
            p.Shape(Sd.Circle(0, 0, 0.07f), Gold, 0.025f);
            return new DoodleArt("s41a_compass_needle", p);
        }

        // ---------- Fake update ----------

        /// <summary>Scrivania con un grande monitor (schermo x -2.55..2.55, y -2.9..3.6).</summary>
        public static DoodleArt S41A_MonitorBackdrop()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.Fill(Sd.Box(0, 0, 3.3f, 5.3f), Rgba.Hex("F3E6CF"), false);
            for (float x = -3f; x < 3.3f; x += 0.5f) p.Line(Rgba.Hex("E8D6B8"), 0.08f, x, -5f, x, 5.2f);
            p.Shape(Sd.Box(0, -4.6f, 3.4f, 0.7f, 0.04f), Rgba.Hex("B07A45"));
            p.Shape(Sd.Box(0, -3.55f, 0.35f, 0.4f), Rgba.Hex("5A5566"));
            p.Shape(Sd.Box(0, -3.95f, 1.0f, 0.08f, 0.06f), Rgba.Hex("5A5566"));
            p.Shape(Sd.Box(0, 0.35f, 2.8f, 3.55f, 0.25f), Rgba.Hex("3A3647"));
            p.FillFunc(Sd.Box(0, 0.35f, 2.55f, 3.25f, 0.1f), (x, y) => Rgba.Lerp(Rgba.Hex("7FD0F2"), Rgba.Hex("4A8FD6"), Clamp((y + 2.9f) / 6.5f)));
            foreach (var c in new[] { (-1.6f, 2.8f), (1.4f, 3.1f), (-0.2f, -2.2f), (1.8f, -1.6f) })
                p.Fill(Sd.Union(Sd.Circle(c.Item1, c.Item2, 0.28f), Sd.Circle(c.Item1 + 0.3f, c.Item2 + 0.06f, 0.22f), Sd.Circle(c.Item1 - 0.28f, c.Item2 - 0.04f, 0.18f)), Rgba.White.WithAlpha(0.55f));
            p.Shape(Sd.Circle(0, -3.07f, 0.05f), Rgba.Hex("7BEA8A"), 0.02f);
            // tazza e paperella
            p.Shape(Sd.Box(-2.2f, -4.05f, 0.25f, 0.25f, 0.06f), Rgba.Hex("E8413C"));
            p.Shape(Sd.Subtract(Sd.Circle(-1.92f, -4.05f, 0.14f), Sd.Circle(-1.92f, -4.05f, 0.07f)), Rgba.Hex("E8413C"), 0.03f);
            p.Shape(Sd.Union(Sd.Ellipse(2.2f, -4.1f, 0.3f, 0.2f), Sd.Circle(2.35f, -3.8f, 0.15f)), Rgba.Hex("FFD24A"));
            p.Shape(Sd.Polygon(2.48f, -3.82f, 2.65f, -3.86f, 2.48f, -3.9f), Rgba.Hex("FF9A3C"), 0.02f);
            p.Fill(Sd.Circle(2.38f, -3.75f, 0.03f), S41A_Ink);
            return new DoodleArt("s41a_monitor_bg", p);
        }

        /// <summary>Finestra a fumetto dell'aggiornamento (4.5 x 4.4, centro 0,0; barra del titolo in y 1.85).</summary>
        public static DoodleArt S41A_UpdateWindow()
        {
            var p = new DoodlePainter(-2.35f, -2.3f, 2.35f, 2.35f, 90) { Ink = 0.06f };
            p.Fill(Sd.Box(0.08f, -0.1f, 2.25f, 2.2f, 0.32f), S41A_Ink.WithAlpha(0.3f));
            Sdf body = Sd.Box(0, 0, 2.25f, 2.2f, 0.32f);
            p.Shape(body, Rgba.Hex("FFF4D6"));
            p.Fill(Sd.Intersect(body, Sd.Box(0, 1.92f, 2.4f, 0.33f)), Rgba.Hex("FF8C5A"));
            p.Line(S41A_Ink, 0.05f, -2.22f, 1.58f, 2.22f, 1.58f);
            string[] candy = { "FFE06A", "8EE6C0", "FF9AC8" };
            for (int i = 0; i < 3; i++) p.Shape(Sd.Circle(1.85f - i * 0.36f, 1.9f, 0.12f), Rgba.Hex(candy[i]), 0.035f);
            for (int i = 0; i < 8; i++) p.Fill(Sd.Circle(-2.0f + i * 0.22f, -2.0f, 0.04f), Rgba.Hex("F2D9A6"));
            p.Shape(Sd.Box(0, 0, 2.25f, 2.2f, 0.32f), Rgba.Clear);
            return new DoodleArt("s41a_upd_window", p);
        }

        /// <summary>Ingranaggio mascotte con la faccia sudata (raggio 0.62).</summary>
        public static DoodleArt S41A_UpdateGear()
        {
            var p = new DoodlePainter(-0.7f, -0.7f, 0.7f, 0.7f, 150) { Ink = 0.04f };
            var parts = new List<Sdf> { Sd.Circle(0, 0, 0.48f) };
            for (int i = 0; i < 8; i++) parts.Add(Sd.Rotate(Sd.Box(0, 0.52f, 0.1f, 0.12f, 0.03f), 0, 0, i * 45f));
            Body(p, Sd.Union(parts.ToArray()), Rgba.Hex("9AA6C0"), Rgba.Hex("6E7A96"));
            p.Shape(Sd.Circle(0, 0, 0.36f), Rgba.Hex("B9C3D8"), 0.03f);
            S41A_Eye(p, -0.13f, 0.08f, 0.08f, 0.1f, 0.02f, 0.02f);
            S41A_Eye(p, 0.13f, 0.08f, 0.08f, 0.1f, 0.02f, 0.02f);
            p.InkLine(0.035f, -0.12f, -0.14f, 0f, -0.2f, 0.12f, -0.14f);
            p.Shape(Sd.Union(Sd.Circle(0.33f, 0.18f, 0.05f), Sd.Polygon(0.285f, 0.2f, 0.375f, 0.2f, 0.33f, 0.3f)), Rgba.Hex("9FDBFF"), 0.02f);
            return new DoodleArt("s41a_upd_gear", p);
        }
    }
}
