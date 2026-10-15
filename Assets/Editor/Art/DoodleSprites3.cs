using System;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli 21-28 (robot, zuppa, ghiaccio, falò, pulsante, piantina, scatole, cavi).</summary>
    public static partial class DoodleSprites
    {
        static readonly Rgba Steel = Rgba.Hex("B7BCC9"), SteelShade = Rgba.Hex("8D93A6"), SteelDark = Rgba.Hex("666B7D");

        // ---------- Livello 21: il robot scarico ----------

        /// <summary>Robottino (piedi in 0,0, testa a ~2.4). Spento: occhi a lineetta e antenna grigia; acceso: occhi luminosi.</summary>
        static DoodleArt Robot(string name, bool on, bool sad = false)
        {
            var p = new DoodlePainter(-1.1f, -0.1f, 1.1f, 2.95f, 120) { Ink = 0.05f };
            // gambe e piedi
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Box(s * 0.3f, 0.35f, 0.08f, 0.25f, 0.03f), SteelDark, 0.035f);
                Body(p, Sd.Box(s * 0.33f, 0.08f, 0.22f, 0.09f, 0.06f), Steel, SteelShade);
            }
            // braccia (spento: penzoloni; acceso: in alto a festeggiare)
            foreach (float s in new[] { -1f, 1f })
            {
                if (on) p.Shape(Sd.Polyline(0.07f, s * 0.6f, 1.25f, s * 0.85f, 1.6f, s * 0.9f, 2.0f), SteelDark, 0.03f);
                else p.Shape(Sd.Polyline(0.07f, s * 0.6f, 1.2f, s * 0.72f, 0.85f, s * 0.75f, 0.55f), SteelDark, 0.03f);
                float hx = on ? s * 0.9f : s * 0.75f, hy = on ? 2.05f : 0.5f;
                p.Shape(Sd.Circle(hx, hy, 0.11f), Steel, 0.03f);
            }
            // corpo con sportello della batteria (la batteria è un oggetto a parte)
            Sdf body = Sd.Box(0, 0.95f, 0.6f, 0.45f, 0.12f);
            Body(p, body, Rgba.Hex("6EC6FF"), Rgba.Hex("3E9BDB"));
            p.Shape(Sd.Box(0, 0.95f, 0.36f, 0.2f, 0.05f), Rgba.Hex("2A2C36"), 0.03f);
            // collo e testa
            p.Shape(Sd.Box(0, 1.47f, 0.12f, 0.08f, 0.02f), SteelDark, 0.03f);
            Sdf head = Sd.Box(0, 1.92f, 0.52f, 0.38f, 0.16f);
            Body(p, head, Steel, SteelShade);
            p.Shape(Sd.Box(0, 1.94f, 0.4f, 0.22f, 0.1f), Rgba.Hex("2A2C36"), 0.03f);   // visiera
            if (on)
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    p.FillFunc(Sd.Circle(s * 0.18f, 1.98f, 0.12f), (x, y) =>
                    {
                        float d = (float)Math.Sqrt((x - s * 0.18f) * (x - s * 0.18f) + (y - 1.98f) * (y - 1.98f)) / 0.12f;
                        return Rgba.Lerp(Rgba.White, NeonCyan, Clamp(d * 1.4f));
                    });
                }
                if (sad)
                {
                    p.Line(NeonCyan, 0.03f, -0.12f, 1.78f, 0f, 1.83f, 0.12f, 1.78f);   // bocca all'ingiù
                    foreach (float s2 in new[] { -1f, 1f }) p.Line(Rgba.Hex("2A2C36"), 0.05f, s2 * 0.3f, 2.08f, s2 * 0.08f, 2.12f);   // palpebre tristi
                    p.Fill(Sd.Ellipse(0.2f, 1.86f, 0.025f, 0.04f), Rgba.Hex("8FD8FF"));   // lacrimuccia
                }
                else p.Line(NeonCyan, 0.03f, -0.12f, 1.83f, 0f, 1.78f, 0.12f, 1.83f);   // sorriso
            }
            else
            {
                foreach (float s in new[] { -1f, 1f }) p.Line(Rgba.Hex("6B6F7E"), 0.04f, s * 0.26f, 1.93f, s * 0.1f, 1.9f);
                p.Line(Rgba.Hex("6B6F7E"), 0.025f, -0.1f, 1.8f, 0.1f, 1.8f);
            }
            // antenna
            p.InkLine(0.03f, 0, 2.3f, 0.05f, 2.62f);
            p.Shape(Sd.Circle(0.05f, 2.7f, 0.09f), on ? Rgba.Hex("FF4D5E") : Rgba.Hex("8D93A6"), 0.03f);
            // bulloni
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Circle(s * 0.56f, 1.92f, 0.06f), SteelDark, 0.02f);
            return new DoodleArt(name, p);
        }

        public static DoodleArt RobotOff() { return Robot("robot_off", false); }
        public static DoodleArt RobotOn() { return Robot("robot_on", true); }
        public static DoodleArt RobotSad() { return Robot("robot_sad", true, true); }

        /// <summary>Dinamo a manovella (centro della ruota in 0,0, raggio 0.75; la manopola è un disegno a parte).</summary>
        public static DoodleArt CrankWheel()
        {
            var p = new DoodlePainter(-0.95f, -1.35f, 0.95f, 0.9f, 120) { Ink = 0.05f };
            Body(p, Sd.Box(0, -1.05f, 0.7f, 0.22f, 0.08f), Rgba.Hex("FFC93C"), Rgba.Hex("E39B1B"));   // base del generatore
            p.Shape(Sd.Box(0, -0.6f, 0.12f, 0.3f, 0.03f), SteelDark, 0.035f);
            p.Shape(Sd.Subtract(Sd.Circle(0, 0, 0.78f), Sd.Circle(0, 0, 0.6f)), Rgba.Hex("E23B3B"), 0.04f);
            for (int i = 0; i < 6; i++)
            {
                double a = i * Math.PI / 3;
                p.Line(SteelShade, 0.06f, 0, 0, (float)Math.Cos(a) * 0.62f, (float)Math.Sin(a) * 0.62f);
            }
            p.Shape(Sd.Circle(0, 0, 0.13f), Steel, 0.03f);
            return new DoodleArt("crank_wheel", p);
        }

        /// <summary>Braccio della manovella con la manopola (perno in 0,0, manopola a x = 0.62).</summary>
        public static DoodleArt CrankArm()
        {
            var p = new DoodlePainter(-0.15f, -0.2f, 0.85f, 0.2f, 140) { Ink = 0.04f };
            p.Shape(Sd.Capsule(0, 0, 0.62f, 0, 0.06f), SteelDark, 0.03f);
            Body(p, Sd.Circle(0.62f, 0, 0.15f), Rgba.Hex("2A2C36"), Rgba.Hex("111219"));
            return new DoodleArt("crank_arm", p);
        }

        /// <summary>Contorno della batteria (orizzontale, 0.6 x 0.3; il livello di carica è una barra a parte).</summary>
        public static DoodleArt BatteryFrame()
        {
            var p = new DoodlePainter(-0.36f, -0.2f, 0.4f, 0.2f, 200) { Ink = 0.03f };
            p.Shape(Sd.Subtract(Sd.Box(0, 0, 0.3f, 0.15f, 0.04f), Sd.Box(0, 0, 0.255f, 0.105f, 0.02f)), Rgba.White);
            p.Shape(Sd.Box(0.335f, 0, 0.035f, 0.07f, 0.015f), Rgba.White);
            return new DoodleArt("battery", p);
        }

        /// <summary>Spina del caricatore con fulmine (si mostra quando il telefono è in carica).</summary>
        public static DoodleArt Bolt()
        {
            var p = new DoodlePainter(-0.25f, -0.35f, 0.25f, 0.35f, 160) { Ink = 0.035f };
            p.Shape(Sd.Polygon(0.06f, 0.32f, -0.18f, -0.03f, -0.02f, -0.03f, -0.08f, -0.32f, 0.18f, 0.06f, 0.02f, 0.06f), Rgba.Hex("FFE14A"));
            return new DoodleArt("bolt", p);
        }

        // ---------- Livello 22: la zuppa bollente ----------

        /// <summary>Scodella di zuppa (centro in 0,0; il bordo è a y = 0.12).</summary>
        public static DoodleArt SoupBowl()
        {
            var p = new DoodlePainter(-0.75f, -0.45f, 0.75f, 0.3f, 140) { Ink = 0.045f };
            Sdf bowl = Sd.Intersect(Sd.Ellipse(0, 0.1f, 0.68f, 0.5f), Sd.Box(0, -0.2f, 0.8f, 0.32f));
            p.Shape(Sd.Ellipse(0, 0.12f, 0.66f, 0.12f), Rgba.Hex("F28C38"), 0.035f);    // zuppa di zucca
            p.Fill(Sd.Ellipse(-0.2f, 0.14f, 0.12f, 0.035f), Rgba.Hex("FFC07A"));
            p.Fill(Sd.Circle(0.22f, 0.11f, 0.04f), Leaf);
            Body(p, bowl, Rgba.Hex("FF6B8A"), Rgba.Hex("D94467"));
            foreach (float x in new[] { -0.35f, 0f, 0.35f }) p.Shape(Sd.Circle(x, -0.12f, 0.05f), Rgba.White, 0.02f);
            p.Shape(Sd.Box(0, -0.4f, 0.28f, 0.04f, 0.02f), Rgba.Hex("D94467"), 0.03f);
            return new DoodleArt("soup", p);
        }

        /// <summary>Termometro verticale (bulbo in 0,0, tubo fino a y = 1.2): il mercurio è una barra a parte.</summary>
        public static DoodleArt Thermometer()
        {
            var p = new DoodlePainter(-0.25f, -0.22f, 0.25f, 1.35f, 140) { Ink = 0.035f };
            p.Shape(Sd.Union(Sd.Box(0, 0.65f, 0.08f, 0.6f, 0.08f), Sd.Circle(0, 0, 0.16f)), Rgba.White);
            for (int i = 0; i < 5; i++) p.InkLine(0.015f, 0.08f, 0.35f + i * 0.2f, 0.16f, 0.35f + i * 0.2f);
            return new DoodleArt("thermo", p);
        }

        // ---------- Livello 23: la chiave nel ghiaccio ----------

        /// <summary>Blocco di ghiaccio semitrasparente (centro 0,0, 1.9 x 1.2) con crepe e riflessi.</summary>
        public static DoodleArt IceBlock()
        {
            var p = new DoodlePainter(-1.05f, -0.7f, 1.05f, 0.7f, 120) { Ink = 0.04f };
            Sdf ice = Sd.Box(0, 0, 0.95f, 0.6f, 0.12f);
            p.FillFunc(ice, (x, y) => Rgba.Lerp(Rgba.Hex("E6F8FF", 0.62f), Rgba.Hex("9FDDF5", 0.72f), Clamp((0.6f - y) / 1.2f)));
            p.Fill(Sd.Rotate(Sd.Box(-0.55f, 0.32f, 0.25f, 0.04f, 0.04f), -0.55f, 0.32f, 12f), Rgba.White.WithAlpha(0.8f));
            p.Fill(Sd.Rotate(Sd.Box(0.6f, -0.3f, 0.15f, 0.03f, 0.03f), 0.6f, -0.3f, 12f), Rgba.White.WithAlpha(0.6f));
            p.Line(Rgba.Hex("FFFFFF", 0.7f), 0.02f, 0.2f, 0.6f, 0.3f, 0.3f, 0.15f, 0.1f, 0.35f, -0.15f);
            p.Line(Rgba.Hex("FFFFFF", 0.6f), 0.015f, -0.8f, -0.1f, -0.55f, -0.2f, -0.45f, -0.45f);
            p.Shape(ice, Rgba.Clear);
            return new DoodleArt("ice_block", p);
        }

        // ---------- Livello 24: il falò ----------

        /// <summary>Notte nel bosco: cielo stellato, pini scuri, tenda e prato (fondo intero).</summary>
        public static DoodleArt CampSite()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 5f, 7.5f), (x, y) => Rgba.Lerp(Rgba.Hex("3A3F7A"), Rgba.Hex("0E1230"), Clamp((y + 1f) / 6f)));
            var rnd = new Random(7);
            for (int i = 0; i < 60; i++)
            {
                float x = (float)rnd.NextDouble() * 9f - 4.5f, y = (float)rnd.NextDouble() * 6f + 0.5f;
                p.Fill(Sd.Circle(x, y, 0.02f + (float)rnd.NextDouble() * 0.03f), Rgba.White.WithAlpha(0.5f + (float)rnd.NextDouble() * 0.5f));
            }
            p.Shape(Sd.Subtract(Sd.Circle(2.6f, 4.6f, 0.45f), Sd.Circle(2.85f, 4.75f, 0.4f)), Rgba.Hex("FFF3B0"), 0.04f);   // luna
            foreach (float[] t in new[] { new[] { -3.6f, 0.2f, 1.1f }, new[] { -2.4f, -0.1f, 1.4f }, new[] { 2.2f, 0f, 1.3f }, new[] { 3.5f, 0.3f, 1.0f }, new[] { -0.4f, 0.4f, 0.8f } })
            {
                float x = t[0], y = t[1], s = t[2];
                for (int k = 0; k < 3; k++)
                {
                    float w = s * (0.75f - k * 0.18f), b = y - 1.5f * s + k * s * 0.85f;
                    p.Shape(Sd.Polygon(x - w, b, x + w, b, x, b + s * 1.2f), Rgba.Hex("1E4A3C"), 0.05f);
                }
            }
            Sdf ground = Sd.Box(0, -4.6f, 5f, 2.9f);
            p.Fill(ground, Rgba.Hex("2F5A3A"));
            p.Hatch(ground, Rgba.Hex("24472D"), 0.4f, 0.04f, 70f);
            p.Shape(Sd.Box(0, -1.75f, 5f, 0.06f), Rgba.Hex("24472D"), 0.05f);
            // tenda
            Sdf tent = Sd.Polygon(1.2f, -1.75f, 3.4f, -1.75f, 2.3f, 0.0f);
            Body(p, tent, Rgba.Hex("F2A33A"), Rgba.Hex("C97A1E"));
            p.Shape(Sd.Polygon(2.0f, -1.75f, 2.6f, -1.75f, 2.3f, -0.9f), Rgba.Hex("3A2A1E"), 0.04f);
            return new DoodleArt("camp_bg", p);
        }

        /// <summary>Catasta di legna con sassi intorno (centro della catasta in 0,0; la fiamma nasce a y = 0.15).</summary>
        public static DoodleArt CampLogs()
        {
            var p = new DoodlePainter(-1.35f, -0.55f, 1.35f, 0.55f, 120) { Ink = 0.05f };
            for (int i = 0; i < 7; i++)
            {
                float x = -1.1f + i * 0.37f;
                Body(p, Sd.Ellipse(x, -0.38f, 0.2f, 0.13f), Rgba.Hex("9A9EAD"), Rgba.Hex("6B6F7E"));
            }
            Rgba wood = Rgba.Hex("A0663A"), woodShade = Rgba.Hex("7A4A26");
            Body(p, Sd.Rotate(Sd.Box(0, -0.05f, 0.85f, 0.13f, 0.12f), 0, -0.05f, 22f), wood, woodShade);
            Body(p, Sd.Rotate(Sd.Box(0, -0.05f, 0.85f, 0.13f, 0.12f), 0, -0.05f, -22f), wood, woodShade);
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Ellipse(s * 0.78f, s * 0.27f - 0.05f, 0.08f, 0.12f), Rgba.Hex("E8C08A"), 0.03f);
            return new DoodleArt("logs", p);
        }

        /// <summary>Bastoncino per accendere il fuoco (verticale, punta in basso a 0,0).</summary>
        public static DoodleArt FireStick()
        {
            var p = new DoodlePainter(-0.12f, -0.05f, 0.12f, 1.3f, 140) { Ink = 0.04f };
            Body(p, Sd.Box(0, 0.62f, 0.055f, 0.62f, 0.05f), Rgba.Hex("C98B55"), Rgba.Hex("A0663A"));
            return new DoodleArt("fire_stick", p);
        }

        // ---------- Livello 25: il pulsante che scappa ----------

        /// <summary>Faccia spaventata del pulsante (occhi grandi e bocca a "o"), da sovrapporre alla cupola di big_button.</summary>
        public static DoodleArt ButtonFace()
        {
            var p = new DoodlePainter(-0.5f, -0.05f, 0.5f, 0.5f, 160) { Ink = 0.03f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Ellipse(s * 0.2f, 0.28f, 0.12f, 0.14f), Rgba.White, 0.025f);
                p.Fill(Sd.Circle(s * 0.2f + 0.02f, 0.26f, 0.05f), Ink);
                p.InkLine(0.025f, s * 0.32f, 0.47f, s * 0.12f, 0.43f);
            }
            p.Shape(Sd.Ellipse(0, 0.08f, 0.07f, 0.06f), Rgba.Hex("7A1F2B"), 0.02f);
            return new DoodleArt("button_face", p);
        }

        /// <summary>Gambette che corrono (sotto la base del pulsante; anca a y = 0).</summary>
        public static DoodleArt ButtonLegs()
        {
            var p = new DoodlePainter(-0.6f, -0.55f, 0.6f, 0.05f, 140) { Ink = 0.035f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.InkLine(0.045f, s * 0.35f, 0f, s * 0.42f, -0.3f, s * 0.3f, -0.42f);
                p.Shape(Sd.Ellipse(s * 0.28f + s * 0.07f, -0.45f, 0.14f, 0.07f), Rgba.Hex("FFD23A"), 0.025f);
            }
            return new DoodleArt("button_legs", p);
        }

        // ---------- Livello 26: annaffia la piantina ----------

        /// <summary>Goccia d'acqua (raggio ~0.09).</summary>
        public static DoodleArt Drop()
        {
            var p = new DoodlePainter(-0.12f, -0.12f, 0.12f, 0.12f, 200) { Ink = 0.018f, Wobble = 0f };
            p.Shape(Sd.Circle(0, 0, 0.085f), Rgba.Hex("4FB6FF"));
            p.Fill(Sd.Circle(-0.03f, 0.03f, 0.022f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("drop", p);
        }

        /// <summary>Vaso di terracotta (bordo superiore a y = 0, fondo a -0.9; apertura larga 1.1).</summary>
        public static DoodleArt Pot()
        {
            var p = new DoodlePainter(-0.75f, -1.0f, 0.75f, 0.15f, 120) { Ink = 0.05f };
            Sdf pot = Sd.Polygon(-0.55f, -0.08f, 0.55f, -0.08f, 0.42f, -0.92f, -0.42f, -0.92f);
            Body(p, pot, Rgba.Hex("E07A4F"), Rgba.Hex("B85B35"));
            Body(p, Sd.Box(0, -0.02f, 0.66f, 0.1f, 0.04f), Rgba.Hex("E98C61"), Rgba.Hex("C96A40"));
            p.Fill(Sd.Box(0, 0.07f, 0.56f, 0.03f), Rgba.Hex("5A3A22"));   // terra
            return new DoodleArt("pot", p);
        }

        /// <summary>Piantina nel vaso (base del gambo in 0,0): appassita o fiorita.</summary>
        static DoodleArt Plant(string name, bool bloom)
        {
            var p = new DoodlePainter(-0.9f, -0.05f, 0.9f, 1.9f, 120) { Ink = 0.045f };
            if (!bloom)
            {
                p.Shape(Sd.Polyline(0.05f, 0f, 0f, 0.05f, 0.5f, 0.25f, 0.85f, 0.5f, 0.8f), Rgba.Hex("9C9A4A"), 0.03f);
                p.Shape(Sd.Rotate(Sd.Ellipse(-0.2f, 0.35f, 0.2f, 0.08f), -0.2f, 0.35f, -30f), Rgba.Hex("B5A55A"), 0.03f);
                p.Shape(Sd.Rotate(Sd.Ellipse(0.62f, 0.62f, 0.16f, 0.12f), 0.62f, 0.62f, 20f), Rgba.Hex("C9906A"), 0.03f);   // fiore chiuso e afflosciato
            }
            else
            {
                p.Shape(Sd.Polyline(0.05f, 0f, 0f, 0.03f, 0.6f, 0f, 1.2f), Leaf, 0.03f);
                p.Shape(Sd.Rotate(Sd.Ellipse(-0.28f, 0.5f, 0.28f, 0.1f), -0.28f, 0.5f, 25f), Leaf, 0.03f);
                p.Shape(Sd.Rotate(Sd.Ellipse(0.3f, 0.75f, 0.28f, 0.1f), 0.3f, 0.75f, -25f), LeafShade, 0.03f);
                for (int i = 0; i < 8; i++)
                {
                    double a = i * Math.PI / 4;
                    float px = (float)Math.Cos(a) * 0.32f, py = 1.4f + (float)Math.Sin(a) * 0.32f;
                    p.Shape(Sd.Rotate(Sd.Ellipse(px, py, 0.2f, 0.11f), px, py, (float)(a * 180 / Math.PI)), Rgba.Hex("FFD23A"), 0.03f);
                }
                p.Shape(Sd.Circle(0, 1.4f, 0.2f), Rgba.Hex("8A5A2B"), 0.03f);
                foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Circle(s * 0.07f, 1.44f, 0.03f), Ink);
                p.InkLine(0.02f, -0.07f, 1.33f, 0f, 1.29f, 0.07f, 1.33f);
            }
            return new DoodleArt(name, p);
        }

        public static DoodleArt PlantDry() { return Plant("plant_dry", false); }
        public static DoodleArt PlantBloom() { return Plant("plant_bloom", true); }

        /// <summary>Bicchierone d'acqua aperto in alto (interno x ±0.45, fondo -0.7, bordo 0.7).</summary>
        public static DoodleArt Glass()
        {
            var p = new DoodlePainter(-0.6f, -0.85f, 0.6f, 0.85f, 120) { Ink = 0.04f };
            Sdf glass = Sd.Subtract(Sd.Box(0, 0, 0.52f, 0.76f, 0.08f), Sd.Box(0, 0.08f, 0.45f, 0.78f, 0.05f));
            p.Fill(Sd.Box(0, 0, 0.5f, 0.74f, 0.08f), Rgba.Hex("D6F1FF", 0.25f));
            p.Shape(glass, Rgba.Hex("BDEBFF", 0.85f));
            p.Fill(Sd.Box(-0.33f, 0.1f, 0.03f, 0.45f, 0.03f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("glass", p);
        }

        // ---------- Livello 27: il gioco delle tre scatole ----------

        /// <summary>Scatola di cartone capovolta (base in 0,0, alta 1.1).</summary>
        public static DoodleArt BoxCup()
        {
            var p = new DoodlePainter(-0.75f, -0.08f, 0.75f, 1.2f, 120) { Ink = 0.05f };
            Sdf box = Sd.Polygon(-0.66f, 0f, 0.66f, 0f, 0.56f, 1.1f, -0.56f, 1.1f);
            Body(p, box, Rgba.Hex("D9A866"), Rgba.Hex("B5843F"), Rgba.Hex("A0702F", 0.5f));
            p.Shape(Sd.Box(0, 0.62f, 0.62f, 0.07f), Rgba.Hex("C9964E"), 0.03f);   // nastro adesivo
            p.InkLine(0.02f, -0.2f, 0.35f, -0.05f, 0.25f);
            p.InkLine(0.02f, 0.1f, 0.32f, 0.25f, 0.4f);
            return new DoodleArt("box_cup", p);
        }

        // ---------- Livello 28: il cavo del colore del cielo ----------

        /// <summary>Foglietto giallo appuntato (centro 0,0; il testo lo scrive il livello).</summary>
        public static DoodleArt StickyNote()
        {
            var p = new DoodlePainter(-0.95f, -0.6f, 0.95f, 0.65f, 110) { Ink = 0.04f };
            p.Shape(Sd.Rotate(Sd.Box(0, 0, 0.88f, 0.52f, 0.03f), 0, 0, -3f), Rgba.Hex("FFF27A"));
            p.Shape(Sd.Circle(0, 0.48f, 0.08f), Rgba.Hex("FF4D5E"), 0.025f);   // puntina
            return new DoodleArt("sticky_note", p);
        }

        // ---------- Scioglimento del pupazzo (livelli 3 e 29) ----------

        /// <summary>Bocca stupita a "O" (centro 0,0).</summary>
        public static DoodleArt MouthO()
        {
            var p = new DoodlePainter(-0.2f, -0.22f, 0.2f, 0.22f, 200) { Ink = 0.03f };
            p.Shape(Sd.Ellipse(0, 0, 0.13f, 0.17f), Rgba.Hex("3A1F2B"));
            p.Fill(Sd.Intersect(Sd.Ellipse(0, -0.1f, 0.08f, 0.06f), Sd.Ellipse(0, 0, 0.11f, 0.15f)), Rgba.Hex("FF8FA3"));
            return new DoodleArt("mouth_o", p);
        }

        /// <summary>Sorriso di carbone (centro 0,0), per il pupazzo già fatto del livello 29.</summary>
        public static DoodleArt MouthSmile()
        {
            var p = new DoodlePainter(-0.5f, -0.25f, 0.5f, 0.15f, 160) { Ink = 0.02f };
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f, x = -0.38f + t * 0.76f, y = -0.15f * (float)Math.Sin(t * Math.PI);
                p.Shape(Sd.Circle(x, y, 0.055f), CoalColor);
            }
            return new DoodleArt("mouth_smile", p);
        }

        /// <summary>Sole che scotta, con occhiali da sole (centro 0,0, raggio 0.55 + raggi).</summary>
        public static DoodleArt Sun()
        {
            var p = new DoodlePainter(-1.0f, -1.0f, 1.0f, 1.0f, 120) { Ink = 0.045f };
            for (int i = 0; i < 12; i++)
            {
                double a = i * Math.PI / 6;
                float c = (float)Math.Cos(a), sn = (float)Math.Sin(a);
                p.Shape(Sd.Polygon(c * 0.6f - sn * 0.1f, sn * 0.6f + c * 0.1f, c * 0.6f + sn * 0.1f, sn * 0.6f - c * 0.1f, c * 0.92f, sn * 0.92f), Rgba.Hex("FF9F2E"), 0.03f);
            }
            Body(p, Sd.Circle(0, 0, 0.58f), Rgba.Hex("FFD23A"), Rgba.Hex("F2A33A"));
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Box(s * 0.2f, 0.1f, 0.16f, 0.1f, 0.05f), Rgba.Hex("2A2C36"), 0.025f);
            p.InkLine(0.03f, -0.05f, 0.12f, 0.05f, 0.12f);
            p.InkLine(0.03f, -0.2f, -0.2f, 0f, -0.28f, 0.2f, -0.2f);   // sorrisetto beffardo
            return new DoodleArt("sun", p);
        }

        /// <summary>Nuvola soffice (centro 0,0, larga 2.4).</summary>
        public static DoodleArt Cloud()
        {
            var p = new DoodlePainter(-1.3f, -0.7f, 1.3f, 0.8f, 110) { Ink = 0.05f };
            Sdf c = Sd.Union(Sd.Ellipse(0, -0.2f, 1.15f, 0.42f), Sd.Circle(-0.5f, 0.05f, 0.45f), Sd.Circle(0.15f, 0.25f, 0.55f), Sd.Circle(0.7f, 0.0f, 0.4f));
            Body(p, c, Rgba.White, Rgba.Hex("D6E4F5"), null, 0.05f, 0.08f);
            foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Circle(s * 0.2f - 0.05f, -0.05f, 0.05f), Ink);
            p.InkLine(0.025f, -0.2f, -0.22f, -0.05f, -0.28f, 0.1f, -0.22f);
            return new DoodleArt("cloud", p);
        }

        // ---------- Livello 31: pizza per quattro ----------

        static void PizzaTop(DoodlePainter p, Sdf clip)
        {
            Sdf crust = Sd.Intersect(Sd.Circle(0, 0, 1.1f), clip);
            Sdf sauce = Sd.Intersect(Sd.Circle(0, 0, 0.93f), clip);
            Body(p, crust, Rgba.Hex("E8B46A"), Rgba.Hex("C98B45"));
            p.Fill(sauce, Rgba.Hex("E2483C"));
            p.FillFunc(Sd.Intersect(Sd.Circle(0, 0, 0.86f), clip), (x, y) =>
                Noise.Value(x * 7f, y * 7f) > 0.42f ? Rgba.Hex("FFE08A") : Rgba.Hex("FFD15C"));
            foreach (float[] c in new[] { new[] { 0.45f, 0.25f }, new[] { -0.3f, 0.5f }, new[] { -0.5f, -0.2f }, new[] { 0.2f, -0.55f }, new[] { 0.55f, -0.35f }, new[] { -0.1f, 0.05f }, new[] { 0.15f, 0.62f }, new[] { -0.6f, 0.2f } })
                p.Shape(Sd.Intersect(Sd.Circle(c[0], c[1], 0.13f), clip), Rgba.Hex("C8323A"), 0.025f);
            foreach (float[] c in new[] { new[] { 0.25f, 0.2f }, new[] { -0.35f, -0.45f }, new[] { 0.6f, 0f }, new[] { -0.2f, 0.35f } })
                p.Fill(Sd.Intersect(Sd.Rotate(Sd.Ellipse(c[0], c[1], 0.07f, 0.035f), c[0], c[1], 30f), clip), Leaf);
            p.Shape(crust, Rgba.Clear);
        }

        /// <summary>Pizza intera (centro 0,0, raggio 1.1).</summary>
        public static DoodleArt Pizza()
        {
            var p = new DoodlePainter(-1.2f, -1.2f, 1.2f, 1.2f, 110) { Ink = 0.045f };
            PizzaTop(p, Sd.Circle(0, 0, 2f));
            return new DoodleArt("pizza", p);
        }

        /// <summary>Spicchio da un quarto: apice in (0,0), tra 0° e 90°.</summary>
        public static DoodleArt PizzaSlice()
        {
            var p = new DoodlePainter(-0.1f, -0.1f, 1.2f, 1.2f, 110) { Ink = 0.045f };
            PizzaTop(p, Sd.Box(0.6f, 0.6f, 0.6f, 0.6f));
            return new DoodleArt("pizza_slice", p);
        }

        // ---------- Livello 26: la fionda ----------

        /// <summary>Fionda di legno a Y (base in 0,0; punte dei rebbi a (±0.32, 1.3)).</summary>
        public static DoodleArt Slingshot()
        {
            var p = new DoodlePainter(-0.6f, -0.1f, 0.6f, 1.5f, 130) { Ink = 0.05f };
            Rgba wood = Rgba.Hex("A0663A"), woodShade = Rgba.Hex("7A4A26");
            Sdf fork = Sd.Union(
                Sd.Capsule(0, 0.05f, 0, 0.75f, 0.09f),
                Sd.Capsule(0, 0.72f, -0.32f, 1.3f, 0.075f),
                Sd.Capsule(0, 0.72f, 0.32f, 1.3f, 0.075f));
            Body(p, fork, wood, woodShade);
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Box(s * 0.3f, 1.2f, 0.09f, 0.05f, 0.02f), Rgba.Hex("E23B3B"), 0.025f);   // legature
            return new DoodleArt("slingshot", p);
        }
    }
}
