using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli della specifica 105-200, pacchetto H.</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV41H() => new DoodleArt[]
        {
            // The silent disco
            S41H_Dancer(),
            // The door that lies
            S41H_Door(false), S41H_Door(true), S41H_Doorway(true), S41H_Doorway(false),
            S41H_DoorFace(0), S41H_DoorFace(1), S41H_DoorFace(2), S41H_Plaque(), S41H_Glove(),
            // Invisible button
            S41H_HiddenButton(false), S41H_HiddenButton(true),
            // The second key
            S41H_Dresser(), S41H_Chest(false), S41H_Chest(true), S41H_Key(),
            // Shadow thief
            S41H_StreetBg(), S41H_Lamppost(), S41H_Bulb(), S41H_LampLight(), S41H_ThiefShadow(),
            // The broken ruler
            S41H_GorgeBg(), S41H_Ruler(0), S41H_Ruler(1), S41H_Ruler(2), S41H_Plank(), S41H_Handle(), S41H_Stamp(),
            // The backward song
            S41H_Bird(), S41H_Branch(), S41H_Pad(),
            // Don't blink
            S41H_Statue(false), S41H_Statue(true), S41H_EyeButton(true), S41H_EyeButton(false),
        };

        static float S41H_Sin(float v) => (float)Math.Sin(v);
        static float S41H_Clamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        // ================= THE SILENT DISCO =================

        /// <summary>Ballerino bianco (si tinge) con cuffie grandi, braccia in alto; piedi in y 0, alto 1.5.</summary>
        public static DoodleArt S41H_Dancer()
        {
            var p = new DoodlePainter(-0.62f, -0.06f, 0.62f, 1.62f, 140) { Ink = 0.045f };
            // gambe
            p.Shape(Sd.Capsule(-0.16f, 0.1f, -0.12f, 0.45f, 0.07f), Rgba.Hex("3A3550"));
            p.Shape(Sd.Capsule(0.16f, 0.1f, 0.12f, 0.45f, 0.07f), Rgba.Hex("3A3550"));
            p.Shape(Sd.Ellipse(-0.2f, 0.06f, 0.13f, 0.06f), Rgba.Hex("F2F2F6"));
            p.Shape(Sd.Ellipse(0.2f, 0.06f, 0.13f, 0.06f), Rgba.Hex("F2F2F6"));
            // braccia
            p.Shape(Sd.Capsule(-0.25f, 0.75f, -0.5f, 1.15f, 0.065f), Rgba.White);
            p.Shape(Sd.Capsule(0.25f, 0.75f, 0.52f, 0.95f, 0.065f), Rgba.White);
            p.Shape(Sd.Circle(-0.52f, 1.2f, 0.08f), Rgba.White);
            p.Shape(Sd.Circle(0.55f, 0.97f, 0.08f), Rgba.White);
            // busto
            Body(p, Sd.Box(0, 0.65f, 0.27f, 0.26f, 0.13f), Rgba.White, Rgba.Hex("D8D8E2"));
            // testa
            Body(p, Sd.Circle(0, 1.17f, 0.28f), Rgba.White, Rgba.Hex("E2E2EA"));
            p.InkLine(0.03f, -0.13f, 1.2f, -0.07f, 1.24f, -0.02f, 1.2f);
            p.InkLine(0.03f, 0.02f, 1.2f, 0.07f, 1.24f, 0.13f, 1.2f);
            p.Shape(Sd.Ellipse(0, 1.06f, 0.08f, 0.05f), Rgba.Hex("5A2A3A"), 0.025f);
            p.Fill(Sd.Circle(-0.17f, 1.1f, 0.05f), BlushColor.WithAlpha(0.6f));
            p.Fill(Sd.Circle(0.17f, 1.1f, 0.05f), BlushColor.WithAlpha(0.6f));
            // cuffie
            p.Shape(Sd.Subtract(Sd.Circle(0, 1.2f, 0.36f), Sd.Union(Sd.Circle(0, 1.2f, 0.29f), Sd.Box(0, 0.95f, 0.5f, 0.3f))), Rgba.Hex("3A3550"), 0.03f);
            p.Shape(Sd.Box(-0.33f, 1.13f, 0.08f, 0.13f, 0.05f), Rgba.Hex("3A3550"), 0.03f);
            p.Shape(Sd.Box(0.33f, 1.13f, 0.08f, 0.13f, 0.05f), Rgba.Hex("3A3550"), 0.03f);
            p.Fill(Sd.Circle(-0.33f, 1.13f, 0.035f), Rgba.Hex("6CF0FF"));
            p.Fill(Sd.Circle(0.33f, 1.13f, 0.035f), Rgba.Hex("6CF0FF"));
            return new DoodleArt("s41h_dancer", p);
        }

        // ================= THE DOOR THAT LIES =================

        /// <summary>Porta di legno 1.3 x 2.3, base in y 0. Con serratura (lock) ha una grande placca con buco della chiave.</summary>
        public static DoodleArt S41H_Door(bool withLock)
        {
            var p = new DoodlePainter(-0.7f, -0.05f, 0.7f, 2.35f, 100) { Ink = 0.05f };
            Sdf door = Sd.Box(0, 1.15f, 0.62f, 1.13f, 0.08f);
            p.FillFunc(door, (x, y) => Rgba.Lerp(WoodShade, Wood, 0.55f + 0.45f * S41H_Sin(x * 9f + S41H_Sin(y * 3f))));
            p.Shape(Sd.Box(0, 1.75f, 0.45f, 0.38f, 0.06f), Wood.WithAlpha(0.6f), 0.03f);
            p.Shape(Sd.Box(0, 0.6f, 0.45f, 0.38f, 0.06f), Wood.WithAlpha(0.6f), 0.03f);
            p.Shape(Sd.Circle(0.47f, 1.1f, 0.07f), Gold, 0.03f);
            p.Shape(door, Rgba.Clear);
            if (withLock)
            {
                p.Shape(Sd.Box(0, 1.15f, 0.26f, 0.36f, 0.08f), Rgba.Hex("C9CDD8"));
                p.Fill(Sd.Union(Sd.Circle(0, 1.22f, 0.09f), Sd.Polygon(-0.06f, 1.2f, 0.06f, 1.2f, 0.1f, 0.95f, -0.1f, 0.95f)), Ink);
            }
            return new DoodleArt(withLock ? "s41h_lockdoor" : "s41h_door", p);
        }

        /// <summary>Vano della porta aperta (stesse misure): luce dorata (vera uscita) o buio.</summary>
        public static DoodleArt S41H_Doorway(bool lit)
        {
            var p = new DoodlePainter(-0.7f, -0.05f, 0.7f, 2.35f, 100) { Ink = 0.05f };
            Sdf hole = Sd.Box(0, 1.15f, 0.62f, 1.13f, 0.08f);
            if (lit)
            {
                p.FillFunc(hole, (x, y) => Rgba.Lerp(Rgba.Hex("FFF8D8"), Rgba.Hex("FFD86A"), S41H_Clamp(Math.Abs(x) * 1.3f + (2.3f - y) * 0.1f)));
                for (int i = -2; i <= 2; i++) p.Line(Rgba.White.WithAlpha(0.6f), 0.05f, i * 0.12f, 2.1f, i * 0.25f, 0.1f);
            }
            else
            {
                p.FillFunc(hole, (x, y) => Rgba.Lerp(Rgba.Hex("120E1C"), Rgba.Hex("2E2440"), S41H_Clamp(y / 2.3f)));
                p.Fill(Sd.Circle(-0.2f, 1.6f, 0.05f), Rgba.Hex("FFE45A"));
                p.Fill(Sd.Circle(0.12f, 1.6f, 0.05f), Rgba.Hex("FFE45A"));
            }
            p.Shape(hole, Rgba.Clear, 0.07f);
            return new DoodleArt(lit ? "s41h_doorway_lit" : "s41h_doorway_dark", p);
        }

        /// <summary>Faccine disegnate sulla porta (centro 0,0): 0 sorriso, 1 terrore, 2 sorrisetto furbo.</summary>
        public static DoodleArt S41H_DoorFace(int mood)
        {
            var p = new DoodlePainter(-0.48f, -0.42f, 0.48f, 0.38f, 160) { Ink = 0.04f };
            foreach (float s in new[] { -1f, 1f })
            {
                float x = s * 0.2f;
                if (mood == 1)
                {
                    p.Shape(Sd.Circle(x, 0.12f, 0.14f), Rgba.White, 0.035f);
                    p.Fill(Sd.Circle(x, 0.12f, 0.035f), Ink);
                    p.InkLine(0.035f, x - s * 0.02f, 0.33f, x + s * 0.14f, 0.28f);
                }
                else if (mood == 2)
                {
                    p.Shape(Sd.Ellipse(x, 0.1f, 0.12f, 0.09f), Rgba.White, 0.035f);
                    p.Fill(Sd.Circle(x + 0.04f, 0.07f, 0.05f), Ink);
                    p.Fill(Sd.Box(x, 0.16f, 0.15f, 0.06f), Wood);
                    p.InkLine(0.04f, x - 0.13f, 0.12f, x + 0.13f, 0.12f);
                }
                else
                {
                    p.InkLine(0.045f, x - 0.1f, 0.08f, x, 0.17f, x + 0.1f, 0.08f);
                }
                if (mood != 1) p.Fill(Sd.Circle(s * 0.33f, -0.07f, 0.07f), BlushColor.WithAlpha(0.6f));
            }
            if (mood == 0)
            {
                Sdf m = Sd.Intersect(Sd.Circle(0, -0.07f, 0.22f), Sd.Box(0, -0.2f, 0.3f, 0.13f));
                p.Shape(m, Rgba.Hex("7A2A3A"), 0.035f);
                p.Fill(Sd.Ellipse(0, -0.24f, 0.09f, 0.04f), Rgba.Hex("FF8FA0"));
            }
            else if (mood == 1)
            {
                p.Shape(Sd.Ellipse(0, -0.22f, 0.12f, 0.15f), Rgba.Hex("3A1A2A"), 0.035f);
                p.Fill(Sd.Box(0, -0.12f, 0.08f, 0.03f), Rgba.White);
            }
            else
            {
                p.InkLine(0.045f, -0.18f, -0.17f, 0f, -0.2f, 0.16f, -0.12f, 0.22f, -0.05f);
            }
            return new DoodleArt(mood == 0 ? "s41h_dface_smile" : mood == 1 ? "s41h_dface_scared" : "s41h_dface_smug", p);
        }

        /// <summary>Targa di legno chiaro 1.3 x 0.44 con due chiodi (il testo è a parte).</summary>
        public static DoodleArt S41H_Plaque()
        {
            var p = new DoodlePainter(-0.7f, -0.26f, 0.7f, 0.26f, 120) { Ink = 0.045f };
            p.Shape(Sd.Box(0, 0, 0.65f, 0.22f, 0.07f), Rgba.Hex("FFF1CF"));
            p.Fill(Sd.Box(0, -0.15f, 0.6f, 0.04f), Rgba.Hex("EBD3A3"));
            p.Shape(Sd.Circle(-0.55f, 0.11f, 0.03f), Rgba.Hex("9AA0AE"), 0.015f);
            p.Shape(Sd.Circle(0.55f, 0.11f, 0.03f), Rgba.Hex("9AA0AE"), 0.015f);
            return new DoodleArt("s41h_plaque", p);
        }

        /// <summary>Guantone da boxe a molla (centro 0,0, punta in avanti verso chi guarda).</summary>
        public static DoodleArt S41H_Glove()
        {
            var p = new DoodlePainter(-0.55f, -0.55f, 0.55f, 0.5f, 140) { Ink = 0.045f };
            for (int i = 0; i < 4; i++) p.Shape(Sd.Ellipse(0, -0.45f + i * 0.06f, 0.14f, 0.04f), Rgba.Hex("B7BCC9"), 0.02f);
            Sdf glove = Sd.Union(Sd.Ellipse(0, 0.05f, 0.42f, 0.38f), Sd.Circle(-0.36f, -0.05f, 0.17f));
            Body(p, glove, Rgba.Hex("E8433F"), Rgba.Hex("B92E2A"));
            p.Shape(Sd.Box(0, -0.3f, 0.22f, 0.07f, 0.03f), Rgba.White, 0.03f);
            p.Fill(Sd.Ellipse(-0.12f, 0.22f, 0.12f, 0.06f), Rgba.White.WithAlpha(0.45f));
            p.InkLine(0.03f, -0.22f, -0.02f, -0.1f, 0.05f);
            return new DoodleArt("s41h_glove", p);
        }

        // ================= INVISIBLE BUTTON =================

        /// <summary>Pulsante tondo a muro (raggio 0.42): rosso spento o verde premuto e acceso.</summary>
        public static DoodleArt S41H_HiddenButton(bool on)
        {
            var p = new DoodlePainter(-0.55f, -0.55f, 0.55f, 0.55f, 140) { Ink = 0.045f };
            p.Shape(Sd.Box(0, 0, 0.48f, 0.48f, 0.12f), Rgba.Hex("C9CDD8"));
            p.Fill(Sd.Box(0.03f, -0.03f, 0.4f, 0.4f, 0.1f), Rgba.Hex("AEB3C2"));
            foreach (var c in new[] { (-0.38f, 0.38f), (0.38f, 0.38f), (-0.38f, -0.38f), (0.38f, -0.38f) })
                p.Shape(Sd.Circle(c.Item1, c.Item2, 0.035f), Rgba.Hex("8D93A6"), 0.015f);
            Rgba fill = on ? Rgba.Hex("6EE06A") : Rgba.Hex("E8433F"), shade = on ? Rgba.Hex("3EA83A") : Rgba.Hex("B92E2A");
            Body(p, Sd.Circle(0, on ? -0.02f : 0.02f, on ? 0.27f : 0.3f), fill, shade);
            p.Fill(Sd.Ellipse(-0.09f, 0.12f, 0.09f, 0.05f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt(on ? "s41h_hidden_button_on" : "s41h_hidden_button", p);
        }

        // ================= THE SECOND KEY =================

        /// <summary>Comò di legno: piano in y 0, largo 1.7, alto 1.2 (origine al centro del piano).</summary>
        public static DoodleArt S41H_Dresser()
        {
            var p = new DoodlePainter(-0.92f, -1.28f, 0.92f, 0.12f, 100) { Ink = 0.05f };
            p.Shape(Sd.Box(-0.65f, -1.15f, 0.07f, 0.1f), WoodDark);
            p.Shape(Sd.Box(0.65f, -1.15f, 0.07f, 0.1f), WoodDark);
            Body(p, Sd.Box(0, -0.55f, 0.8f, 0.5f, 0.05f), Wood, WoodShade);
            p.Shape(Sd.Box(0, 0, 0.88f, 0.07f, 0.03f), WoodShade);
            for (int i = 0; i < 2; i++)
            {
                float y = -0.33f - i * 0.45f;
                p.Shape(Sd.Box(0, y, 0.68f, 0.17f, 0.04f), Rgba.Hex("E39A55"), 0.035f);
                p.Shape(Sd.Circle(0, y, 0.05f), Gold, 0.02f);
            }
            return new DoodleArt("s41h_dresser", p);
        }

        /// <summary>Baule con due serrature (centro 0,0; 1.7 x 1.2). Fori in (±0.38, -0.05). Aperto: coperchio su e oro.</summary>
        public static DoodleArt S41H_Chest(bool open)
        {
            var p = new DoodlePainter(-0.92f, -0.66f, 0.92f, 1.25f, 110) { Ink = 0.05f };
            if (open)
            {
                p.Shape(Sd.Box(0, 0.75f, 0.82f, 0.42f, 0.12f), WoodShade);
                p.Fill(Sd.Box(0, 0.72f, 0.7f, 0.3f, 0.08f), WoodDark);
                p.Shape(Sd.Union(Sd.Ellipse(0, 0.2f, 0.72f, 0.22f), Sd.Circle(-0.3f, 0.3f, 0.18f), Sd.Circle(0.25f, 0.33f, 0.2f)), Gold);
                foreach (var c in new[] { (-0.45f, 0.25f), (-0.1f, 0.38f), (0.3f, 0.3f), (0.5f, 0.2f), (0.05f, 0.18f) })
                    p.Shape(Sd.Circle(c.Item1, c.Item2, 0.08f), GoldLight, 0.02f);
                p.Shape(Sd.Polygon(0.1f, 0.5f, 0.2f, 0.62f, 0.3f, 0.5f, 0.2f, 0.4f), Rgba.Hex("6CE0FF"), 0.02f);
            }
            else
            {
                Sdf lid = Sd.Intersect(Sd.Ellipse(0, 0.12f, 0.85f, 0.55f), Sd.Box(0, 0.45f, 0.9f, 0.33f));
                Body(p, lid, Wood, WoodShade);
            }
            Body(p, Sd.Box(0, -0.25f, 0.85f, 0.38f, 0.06f), Wood, WoodShade);
            p.Shape(Sd.Box(-0.62f, open ? -0.25f : 0f, 0.07f, open ? 0.38f : 0.65f), Gold, 0.03f);
            p.Shape(Sd.Box(0.62f, open ? -0.25f : 0f, 0.07f, open ? 0.38f : 0.65f), Gold, 0.03f);
            foreach (float x in new[] { -0.38f, 0.38f })
            {
                p.Shape(Sd.Box(x, -0.08f, 0.15f, 0.19f, 0.05f), Gold, 0.035f);
                p.Fill(Sd.Union(Sd.Circle(x, -0.02f, 0.05f), Sd.Polygon(x - 0.035f, -0.03f, x + 0.035f, -0.03f, x + 0.055f, -0.17f, x - 0.055f, -0.17f)), Ink);
            }
            return new DoodleArt(open ? "s41h_chest_open" : "s41h_chest", p);
        }

        /// <summary>Chiave dorata centrata (lunga 0.9, anello a sinistra).</summary>
        public static DoodleArt S41H_Key()
        {
            var p = new DoodlePainter(-0.5f, -0.25f, 0.5f, 0.25f, 180) { Ink = 0.04f };
            Sdf key = Sd.Union(Sd.Subtract(Sd.Circle(-0.28f, 0, 0.19f), Sd.Circle(-0.28f, 0, 0.07f)),
                Sd.Box(0.12f, 0, 0.3f, 0.05f, 0.03f), Sd.Box(0.36f, -0.09f, 0.04f, 0.07f, 0.01f), Sd.Box(0.22f, -0.08f, 0.035f, 0.06f, 0.01f));
            Body(p, key, Gold, GoldShade);
            p.Fill(Sd.Circle(-0.36f, 0.08f, 0.04f), GoldLight);
            return new DoodleArt("s41h_key", p);
        }

        // ================= SHADOW THIEF =================

        /// <summary>Strada di notte: luna in alto a sinistra, palazzi, marciapiede con il bordo in y -2.7.</summary>
        public static DoodleArt S41H_StreetBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("2A2F5C"), Rgba.Hex("0E1230"), S41H_Clamp((y + 1f) / 6f)));
            for (int i = 0; i < 40; i++)
            {
                float x = -3f + (i * 1.37f) % 6f, y = 0.5f + (i * 2.71f) % 4.5f;
                p.Fill(Sd.Circle(x, y, 0.025f + (i % 3) * 0.01f), Rgba.White.WithAlpha(0.6f));
            }
            p.Fill(Sd.Circle(-2.0f, 3.4f, 0.75f), Rgba.Hex("FFF5C8", 0.15f));
            p.Shape(Sd.Circle(-2.0f, 3.4f, 0.48f), Rgba.Hex("FFF3C0"));
            p.Fill(Sd.Circle(-2.15f, 3.5f, 0.09f), Rgba.Hex("E8DCA0"));
            p.Fill(Sd.Circle(-1.85f, 3.25f, 0.06f), Rgba.Hex("E8DCA0"));
            // palazzi
            float[] bx = { -3.2f, -2.1f, -0.9f, 0.3f, 1.4f, 2.4f };
            float[] bh = { 1.4f, 2.4f, 1.0f, 2.0f, 1.2f, 2.6f };
            for (int i = 0; i < bx.Length; i++)
            {
                float w = 1.1f, top = -0.4f + bh[i];
                Sdf b = Sd.Box(bx[i] + w / 2f, (top - 2.7f) / 2f, w / 2f, (top + 2.7f) / 2f);
                p.Shape(b, Rgba.Hex(i % 2 == 0 ? "1B2046" : "232858"), 0.035f);
                for (float wy = top - 0.4f; wy > -2.2f; wy -= 0.5f)
                    for (float wx = bx[i] + 0.25f; wx < bx[i] + w - 0.15f; wx += 0.35f)
                        p.Fill(Sd.Box(wx, wy, 0.07f, 0.1f), ((int)(wx * 7 + wy * 3) % 4 == 0) ? Rgba.Hex("FFD86A", 0.8f) : Rgba.Hex("0E1230", 0.7f));
            }
            // marciapiede e strada
            p.FillFunc(Sd.Box(0, -3.0f, 3.3f, 0.3f), (x, y) => Rgba.Hex("5A5E78"));
            p.Line(Rgba.Hex("3E4258"), 0.03f, -3.2f, -2.7f, 3.2f, -2.7f);
            for (float x = -3f; x < 3.2f; x += 0.8f) p.Line(Rgba.Hex("3E4258"), 0.025f, x, -2.7f, x - 0.1f, -3.3f);
            p.Fill(Sd.Box(0, -4.3f, 3.3f, 1.0f), Rgba.Hex("2A2C3C"));
            p.Line(Rgba.Hex("6A6E86"), 0.05f, -3.2f, -3.3f, 3.2f, -3.3f);
            for (float x = -2.8f; x < 3.2f; x += 1.3f) p.Fill(Sd.Box(x, -4.3f, 0.3f, 0.05f), Rgba.Hex("D8D27A", 0.8f));
            return new DoodleArt("s41h_street_bg", p);
        }

        /// <summary>Lampione: base in (0,0), testa in y 3.0 (la lampadina va in y 2.85).</summary>
        public static DoodleArt S41H_Lamppost()
        {
            var p = new DoodlePainter(-0.36f, -0.08f, 0.36f, 3.32f, 100) { Ink = 0.045f };
            p.Shape(Sd.Box(0, 0.08f, 0.2f, 0.08f, 0.03f), Rgba.Hex("3A3F5A"));
            p.Shape(Sd.Box(0, 1.45f, 0.055f, 1.35f), Rgba.Hex("4A5070"));
            p.Shape(Sd.Polygon(-0.3f, 2.92f, 0.3f, 2.92f, 0.16f, 3.18f, -0.16f, 3.18f), Rgba.Hex("4A5070"));
            p.Shape(Sd.Circle(0, 3.25f, 0.06f), Rgba.Hex("4A5070"), 0.03f);
            return new DoodleArt("s41h_lamppost", p);
        }

        /// <summary>Lampadina bianca (si tinge), raggio 0.12.</summary>
        public static DoodleArt S41H_Bulb()
        {
            var p = new DoodlePainter(-0.16f, -0.16f, 0.16f, 0.16f, 160) { Ink = 0.03f };
            p.Shape(Sd.Circle(0, 0, 0.12f), Rgba.White);
            return new DoodleArt("s41h_bulb", p);
        }

        /// <summary>Cono di luce del lampione: parte da (0,0) e scende fino a y -2.95 con una pozza di luce.</summary>
        public static DoodleArt S41H_LampLight()
        {
            var p = new DoodlePainter(-0.75f, -3.15f, 0.75f, 0.05f, 50) { Ink = 0f };
            p.FillFunc(Sd.Polygon(-0.16f, 0f, 0.16f, 0f, 0.62f, -2.95f, -0.62f, -2.95f), (x, y) => Rgba.Hex("FFE98A", 0.25f + 0.35f * S41H_Clamp(1f + y / 3f)));
            p.Fill(Sd.Ellipse(0, -2.95f, 0.72f, 0.16f), Rgba.Hex("FFF1B0", 0.6f), false);
            return new DoodleArt("s41h_lamp_light", p);
        }

        /// <summary>Ombra lunga del ladro distesa a terra verso destra (piedi in 0,0).</summary>
        public static DoodleArt S41H_ThiefShadow()
        {
            var p = new DoodlePainter(-0.2f, -0.22f, 1.75f, 0.22f, 80) { Ink = 0f };
            Rgba sh = Rgba.Hex("05060F", 0.7f);
            Sdf body = Sd.Union(Sd.Polygon(0f, -0.1f, 0f, 0.1f, 1.0f, 0.13f, 1.05f, -0.13f),
                Sd.Circle(1.25f, 0f, 0.17f), Sd.Polygon(1.1f, -0.12f, 1.55f, -0.05f, 1.5f, 0.1f, 1.15f, 0.12f),
                Sd.Capsule(0.65f, 0.1f, 1.0f, 0.2f, 0.04f), Sd.Ellipse(0.75f, -0.15f, 0.22f, 0.08f));
            p.Fill(body, sh);
            return new DoodleArt("s41h_thief_shadow", p);
        }

        // ================= THE BROKEN RULER =================

        /// <summary>Burrone: cielo, montagne, scogliera sinistra fino a x -1.2 e destra da x 1.1 (bordo in y -1.2), fiume in basso.</summary>
        public static DoodleArt S41H_GorgeBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("FFE7C2"), Rgba.Hex("8ECDF2"), S41H_Clamp((y + 1f) / 4f)));
            p.Shape(Sd.Circle(1.9f, 3.3f, 0.45f), Rgba.Hex("FFD45A"), 0.04f);
            p.Shape(Sd.Polygon(-3.3f, -1.2f, -2.0f, 1.0f, -0.8f, -0.2f, 0.4f, 1.4f, 1.8f, -0.1f, 3.3f, 0.9f, 3.3f, -1.2f), Rgba.Hex("B9A6D8"), 0.04f);
            p.Fill(Sd.Polygon(-2.0f, 1.0f, -2.3f, 0.6f, -1.8f, 0.6f), Rgba.White);
            p.Fill(Sd.Polygon(0.4f, 1.4f, 0.1f, 1.0f, 0.65f, 1.0f), Rgba.White);
            // fiume
            p.FillFunc(Sd.Box(0, -4.6f, 3.3f, 1.0f), (x, y) => Rgba.Lerp(Rgba.Hex("3E86C9"), Rgba.Hex("6FB8E8"), S41H_Clamp((y + 5.2f) / 1.4f)));
            for (int i = 0; i < 6; i++) p.Line(Rgba.White.WithAlpha(0.6f), 0.03f, -0.9f + (i % 3) * 0.6f, -3.9f - (i / 3) * 0.5f, -0.6f + (i % 3) * 0.6f, -3.85f - (i / 3) * 0.5f);
            // scogliere
            foreach (bool left in new[] { true, false })
            {
                float edge = left ? -1.2f : 1.1f, outer = left ? -3.3f : 3.3f;
                Sdf cliff = Sd.Polygon(outer, -1.2f, edge, -1.2f, edge + (left ? -0.25f : 0.25f), -3.0f, edge + (left ? 0.05f : -0.05f), -5.3f, outer, -5.3f);
                p.FillFunc(cliff, (x, y) => Rgba.Lerp(Rgba.Hex("A8693F"), Rgba.Hex("C98A55"), 0.5f + 0.5f * S41H_Sin(y * 4f + x * 2f)));
                p.Hatch(cliff, Rgba.Hex("7A4A2B", 0.5f), 0.22f, 0.02f, 60f);
                p.Shape(cliff, Rgba.Clear);
                p.Shape(Sd.Box((edge + outer) / 2f, -1.15f, Math.Abs(outer - edge) / 2f, 0.08f, 0.04f), Rgba.Hex("6BC25A"), 0.035f);
            }
            return new DoodleArt("s41h_gorge_bg", p);
        }

        static float S41H_Zig(int kind, float y)
        {
            if (kind == 1) { float f = y / 0.12f - (float)Math.Floor(y / 0.12f); return 0.07f * (Math.Abs(f - 0.5f) * 4f - 1f); }
            return 0.07f * S41H_Sin(y * 38f + 1f);
        }

        /// <summary>Pezzo di righello (0: 0-3, 1: 3-5, 2: 5-8 unità; 1 unità = 0.5): origine all'estremo sinistro, rotture a zigzag.</summary>
        public static DoodleArt S41H_Ruler(int piece)
        {
            int[] units = { 3, 2, 3 };
            float w = units[piece] * 0.5f;
            var p = new DoodlePainter(-0.12f, -0.22f, w + 0.12f, 0.22f, 140) { Ink = 0.035f };
            int leftKind = piece == 0 ? 0 : piece, rightKind = piece == 2 ? 0 : piece + 1;
            Sdf box = Sd.Box(w / 2f, 0, w / 2f + 0.1f, 0.17f, 0.02f);
            Sdf shape = (x, y) =>
            {
                float d = box(x, y);
                if (leftKind != 0) d = Math.Max(d, S41H_Zig(leftKind, y) - x);
                else d = Math.Max(d, -x);
                if (rightKind != 0) d = Math.Max(d, x - (w + S41H_Zig(rightKind, y)));
                else d = Math.Max(d, x - w);
                return d;
            };
            p.Fill(shape, Rgba.Hex("FFE28A"));
            p.Fill(Sd.Intersect(shape, Sd.Box(w / 2f, -0.14f, w + 1f, 0.04f)), Rgba.Hex("F0C45A"));
            for (int t = 0; t <= units[piece] * 2; t++)
            {
                float x = t * 0.25f;
                if ((t == 0 && leftKind != 0) || (t == units[piece] * 2 && rightKind != 0)) continue;
                bool major = t % 2 == 0;
                p.InkLine(0.022f, x, 0.17f, x, major ? 0.02f : 0.09f);
            }
            p.Shape(shape, Rgba.Clear);
            return new DoodleArt("s41h_ruler_" + piece, p);
        }

        /// <summary>Asse del ponte lunga 1 (origine all'estremo sinistro): si allunga scalando x.</summary>
        public static DoodleArt S41H_Plank()
        {
            var p = new DoodlePainter(-0.02f, -0.14f, 1.02f, 0.14f, 120) { Ink = 0.03f };
            p.Shape(Sd.Box(0.5f, 0, 0.5f, 0.1f), Wood);
            p.Fill(Sd.Box(0.5f, -0.06f, 0.5f, 0.03f), WoodShade);
            p.Line(WoodDark.WithAlpha(0.6f), 0.015f, 0f, 0.03f, 1f, 0.03f);
            return new DoodleArt("s41h_plank", p);
        }

        /// <summary>Maniglia per allungare il ponte (cerchio con frecce).</summary>
        public static DoodleArt S41H_Handle()
        {
            var p = new DoodlePainter(-0.36f, -0.36f, 0.36f, 0.36f, 140) { Ink = 0.04f };
            Body(p, Sd.Circle(0, 0, 0.27f), Rgba.Hex("FF9A3C"), Rgba.Hex("E0700B"));
            p.Fill(Sd.Polygon(-0.2f, 0f, -0.07f, 0.1f, -0.07f, -0.1f), Rgba.White);
            p.Fill(Sd.Polygon(0.2f, 0f, 0.07f, 0.1f, 0.07f, -0.1f), Rgba.White);
            p.Fill(Sd.Box(0, 0, 0.08f, 0.03f), Rgba.White);
            return new DoodleArt("s41h_handle", p);
        }

        /// <summary>Timbro verde "approvato" con la spunta (centro 0,0, raggio 0.55).</summary>
        public static DoodleArt S41H_Stamp()
        {
            var p = new DoodlePainter(-0.62f, -0.62f, 0.62f, 0.62f, 120) { Ink = 0.045f };
            p.Shape(Sd.Circle(0, 0, 0.55f), Rgba.Hex("6EE06A"));
            p.Fill(Sd.Subtract(Sd.Circle(0, 0, 0.47f), Sd.Circle(0, 0, 0.42f)), Rgba.White.WithAlpha(0.8f));
            p.Line(Rgba.White, 0.12f, -0.22f, 0f, -0.05f, -0.18f, 0.25f, 0.2f);
            return new DoodleArt("s41h_stamp", p);
        }

        // ================= THE BACKWARD SONG =================

        /// <summary>Uccellino azzurro che canta, rivolto a destra (centro 0,0, largo 1.1; zampe in y -0.42).</summary>
        public static DoodleArt S41H_Bird()
        {
            var p = new DoodlePainter(-0.62f, -0.5f, 0.66f, 0.5f, 150) { Ink = 0.045f };
            p.Shape(Sd.Polygon(-0.35f, -0.05f, -0.62f, 0.12f, -0.6f, -0.12f), BirdShade);
            p.InkLine(0.03f, -0.05f, -0.3f, -0.07f, -0.44f);
            p.InkLine(0.03f, 0.1f, -0.3f, 0.12f, -0.44f);
            Sdf body = Sd.Union(Sd.Ellipse(-0.02f, -0.05f, 0.38f, 0.3f), Sd.Circle(0.2f, 0.17f, 0.22f));
            Body(p, body, BirdBlue, BirdShade);
            p.Fill(Sd.Ellipse(0.05f, -0.15f, 0.22f, 0.15f), BirdBelly);
            p.Shape(Sd.Ellipse(-0.12f, -0.02f, 0.2f, 0.12f), BirdShade, 0.03f);
            // becco aperto
            p.Shape(Sd.Polygon(0.38f, 0.22f, 0.62f, 0.27f, 0.4f, 0.16f), Rgba.Hex("FFB43C"), 0.025f);
            p.Shape(Sd.Polygon(0.38f, 0.12f, 0.58f, 0.04f, 0.4f, 0.16f), Rgba.Hex("FFB43C"), 0.025f);
            p.Fill(Sd.Circle(0.26f, 0.25f, 0.05f), Ink);
            p.Fill(Sd.Circle(0.25f, 0.27f, 0.018f), Rgba.White);
            p.Fill(Sd.Circle(0.3f, 0.12f, 0.04f), BlushColor.WithAlpha(0.6f));
            p.Shape(Sd.Polygon(0.12f, 0.36f, 0.06f, 0.5f, 0.18f, 0.4f), BirdBlue, 0.02f);
            return new DoodleArt("s41h_bird", p);
        }

        /// <summary>Ramo orizzontale con foglioline (centro 0,0, lungo 2.4).</summary>
        public static DoodleArt S41H_Branch()
        {
            var p = new DoodlePainter(-1.25f, -0.3f, 1.3f, 0.3f, 100) { Ink = 0.04f };
            p.Shape(Sd.Capsule(-1.2f, -0.05f, 1.2f, 0.05f, 0.08f), Twig);
            p.Shape(Sd.Capsule(0.6f, 0.03f, 0.9f, 0.2f, 0.04f), Twig);
            foreach (var l in new[] { (-0.9f, 0.12f), (0.95f, 0.22f), (-0.4f, -0.15f), (1.15f, -0.08f) })
                p.Shape(Sd.Ellipse(l.Item1, l.Item2, 0.13f, 0.07f), Leaf, 0.025f);
            return new DoodleArt("s41h_branch", p);
        }

        /// <summary>Tasto quadrato bianco (si tinge) 1.0 x 1.0 con una notina disegnata.</summary>
        public static DoodleArt S41H_Pad()
        {
            var p = new DoodlePainter(-0.56f, -0.56f, 0.56f, 0.56f, 120) { Ink = 0.05f };
            p.Shape(Sd.Box(0, 0, 0.48f, 0.48f, 0.14f), Rgba.White);
            p.Fill(Sd.Box(0, -0.38f, 0.42f, 0.06f, 0.05f), Rgba.Hex("D8D8E2"));
            p.Fill(Sd.Ellipse(-0.07f, -0.12f, 0.12f, 0.09f), Ink.WithAlpha(0.55f));
            p.Line(Ink.WithAlpha(0.55f), 0.045f, 0.04f, -0.1f, 0.04f, 0.22f, 0.16f, 0.12f);
            return new DoodleArt("s41h_pad", p);
        }

        // ================= DON'T BLINK =================

        /// <summary>Statua di pietra con le ali (piedi in y 0, alta 2.2): timida (mani sugli occhi) o che sbircia.</summary>
        public static DoodleArt S41H_Statue(bool peek)
        {
            var p = new DoodlePainter(-0.95f, -0.06f, 0.95f, 2.32f, 100) { Ink = 0.05f };
            Rgba stone = Rgba.Hex("C9CBD6"), shade = Rgba.Hex("9A9DAE");
            // ali
            foreach (float s in new[] { -1f, 1f })
            {
                Sdf wing = Sd.Polygon(s * 0.2f, 1.5f, s * 0.9f, 2.15f, s * 0.85f, 1.3f, s * 0.6f, 0.75f, s * 0.25f, 1.0f);
                p.Shape(wing, Rgba.Hex("B8BACA"));
                for (int k = 0; k < 3; k++) p.Line(shade, 0.025f, s * 0.35f, 1.3f - k * 0.1f, s * (0.85f - k * 0.08f), 1.9f - k * 0.35f);
            }
            // veste
            Body(p, Sd.Polygon(-0.32f, 1.45f, 0.32f, 1.45f, 0.55f, 0.05f, -0.55f, 0.05f), stone, shade);
            for (int k = -1; k <= 1; k++) p.Line(shade, 0.025f, k * 0.12f, 1.2f, k * 0.25f, 0.1f);
            // testa
            Body(p, Sd.Circle(0, 1.7f, 0.28f), stone, shade);
            p.Shape(Sd.Subtract(Sd.Circle(0, 1.8f, 0.3f), Sd.Circle(0, 1.72f, 0.27f)), Rgba.Hex("B0B3C4"), 0.03f);
            if (peek)
            {
                p.Shape(Sd.Circle(-0.1f, 1.72f, 0.065f), Rgba.Hex("2A2234"), 0.025f);
                p.Shape(Sd.Circle(0.1f, 1.72f, 0.065f), Rgba.Hex("2A2234"), 0.025f);
                p.Fill(Sd.Circle(-0.1f, 1.72f, 0.02f), Rgba.Hex("FF4A5A"));
                p.Fill(Sd.Circle(0.1f, 1.72f, 0.02f), Rgba.Hex("FF4A5A"));
                p.InkLine(0.03f, -0.12f, 1.56f, -0.04f, 1.52f, 0.04f, 1.52f, 0.12f, 1.56f);
                p.Shape(Sd.Capsule(-0.3f, 1.3f, -0.42f, 0.8f, 0.07f), stone);
                p.Shape(Sd.Capsule(0.3f, 1.3f, 0.42f, 0.8f, 0.07f), stone);
            }
            else
            {
                p.Shape(Sd.Capsule(-0.28f, 1.3f, -0.12f, 1.66f, 0.08f), stone);
                p.Shape(Sd.Capsule(0.28f, 1.3f, 0.12f, 1.66f, 0.08f), stone);
                p.Shape(Sd.Ellipse(-0.1f, 1.72f, 0.12f, 0.09f), stone, 0.03f);
                p.Shape(Sd.Ellipse(0.1f, 1.72f, 0.12f, 0.09f), stone, 0.03f);
                p.Shape(Sd.Ellipse(0, 1.52f, 0.04f, 0.03f), Rgba.Hex("6A6D7E"), 0.02f);
            }
            p.Shape(Sd.Box(0, 0.06f, 0.6f, 0.07f, 0.03f), shade);
            return new DoodleArt(peek ? "s41h_statue_peek" : "s41h_statue_shy", p);
        }

        /// <summary>Pulsante tondo "guarda" (raggio 0.72): occhio aperto o chiuso.</summary>
        public static DoodleArt S41H_EyeButton(bool open)
        {
            var p = new DoodlePainter(-0.8f, -0.8f, 0.8f, 0.8f, 120) { Ink = 0.055f };
            Body(p, Sd.Circle(0, 0, 0.72f), open ? Rgba.Hex("7FD6FF") : Rgba.Hex("A8AEC4"), open ? Rgba.Hex("4AA3DF") : Rgba.Hex("858BA3"));
            Sdf eye = Sd.Intersect(Sd.Circle(0, -0.35f, 0.62f), Sd.Circle(0, 0.35f, 0.62f));
            if (open)
            {
                p.Shape(eye, Rgba.White, 0.045f);
                p.Fill(Sd.Circle(0, 0, 0.19f), Rgba.Hex("5A3A8A"));
                p.Fill(Sd.Circle(0, 0, 0.09f), Ink);
                p.Fill(Sd.Circle(-0.06f, 0.07f, 0.045f), Rgba.White);
            }
            else
            {
                p.InkLine(0.06f, -0.45f, 0.02f, -0.2f, -0.15f, 0.2f, -0.15f, 0.45f, 0.02f);
                for (int k = -1; k <= 1; k++) p.InkLine(0.045f, k * 0.2f, -0.17f, k * 0.25f, -0.32f);
            }
            return new DoodleArt(open ? "s41h_eye_open" : "s41h_eye_closed", p);
        }
    }
}
