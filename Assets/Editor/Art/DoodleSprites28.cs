using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli compositi della versione 0.40 (gruppo E).</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV40E() => new DoodleArt[]
        {
            V40E_Dot(), V40E_FireworkBurst(),
            // Elevator logic
            V40E_TowerBackdrop(), V40E_ElevatorCar(), V40E_FloorButton(), V40E_ClueBoard(),
            V40E_Animal("e40_cat"), V40E_Animal("e40_dog"), V40E_Animal("e40_parrot"), V40E_Animal("e40_bunny"),
            // Circuit board
            V40E_WorkshopBackdrop(), V40E_SafeDoor(), V40E_Pcb(), V40E_Socket(), V40E_Tray(), V40E_Display(),
            V40E_Wire(0), V40E_Wire(1), V40E_Wire(2), V40E_Switch(false), V40E_Switch(true), V40E_Resistor(),
            V40E_Battery(), V40E_Bulb(false), V40E_Bulb(true), V40E_Led(0), V40E_Led(1), V40E_Led(2), V40E_SafeInside(),
            // Snow globe
            V40E_CozyBackdrop(), V40E_GlobeInside(), V40E_GlobeDome(), V40E_GlobeCollar(), V40E_RingBand(), V40E_RingArrow(),
            V40E_Flake(), V40E_MusicBox(false), V40E_MusicBox(true), V40E_Shelf(),
            // Knight's quest
            V40E_HallBackdrop(), V40E_Chessboard(), V40E_Knight(), V40E_MousePawn(), V40E_CatKing(), V40E_Rook(), V40E_Yarn(),
            V40E_MoveDot(), V40E_SelectRing(), V40E_UndoButton(),
            // The gauntlet
            V40E_ArenaBackdrop(), V40E_Padlock(), V40E_MazeTray(), V40E_Marble(), V40E_MazeHole(),
        };

        static readonly Rgba V40E_Ink = DoodlePainter.DefaultInk;
        static float V40E_Sin(float v) => (float)Math.Sin(v);
        static float V40E_Cos(float v) => (float)Math.Cos(v);

        // ---------- Comuni ----------

        /// <summary>Pallino bianco del progresso delle fasi (si tinge; raggio 0.11).</summary>
        public static DoodleArt V40E_Dot()
        {
            var p = new DoodlePainter(-0.14f, -0.14f, 0.14f, 0.14f, 300) { Ink = 0.03f, Wobble = 0.004f };
            p.Shape(Sd.Circle(0, 0, 0.1f), Rgba.White);
            p.Fill(Sd.Circle(-0.035f, 0.035f, 0.025f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("e40_dot", p);
        }

        /// <summary>Scoppio di fuoco d'artificio bianco (si tinge; raggio 1).</summary>
        public static DoodleArt V40E_FireworkBurst()
        {
            var p = new DoodlePainter(-1.05f, -1.05f, 1.05f, 1.05f, 90) { Wobble = 0f };
            for (int i = 0; i < 14; i++)
            {
                float a = i / 14f * 6.2832f;
                float c = V40E_Cos(a), s = V40E_Sin(a);
                p.Line(Rgba.White.WithAlpha(0.9f), 0.05f, c * 0.3f, s * 0.3f, c * 0.85f, s * 0.85f);
                p.Fill(Sd.Circle(c * 0.95f, s * 0.95f, 0.06f), Rgba.White, false);
            }
            p.Fill(Sd.Circle(0, 0, 0.16f), Rgba.White.WithAlpha(0.8f), false);
            return new DoodleArt("e40_firework", p);
        }

        // ---------- Elevator logic ----------

        /// <summary>
        /// Palazzo in sezione (coordinate del livello): vano dell'ascensore a sinistra (x -2.6..-1.35), atrio con base in
        /// y -4.25 e cinque piani alti 0.95 (base del piano k = -4.25 + 0.95 k), tetto in 1.45.
        /// </summary>
        public static DoodleArt V40E_TowerBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 5f, 7.5f), (x, y) => Rgba.Lerp(Rgba.Hex("9FD8F5"), Rgba.Hex("5BA8E0"), Clamp((y + 1f) / 6f)));
            foreach (var c in new[] { (-2.0f, 3.1f), (1.7f, 2.4f), (3.6f, 3.4f) })
            {
                p.Fill(Sd.Union(Sd.Circle(c.Item1, c.Item2, 0.35f), Sd.Circle(c.Item1 + 0.35f, c.Item2 + 0.1f, 0.3f), Sd.Circle(c.Item1 - 0.35f, c.Item2 - 0.05f, 0.25f)), Rgba.White.WithAlpha(0.85f));
            }
            // marciapiede
            p.Fill(Sd.Box(0, -5.6f, 5f, 1.35f), Rgba.Hex("8B8698"), false);
            p.Shape(Sd.Box(0, -4.3f, 5f, 0.06f), Rgba.Hex("6B6677"));
            // muri del palazzo
            const float b0 = -4.25f, h = 0.95f;
            p.Shape(Sd.Box(0, -1.4f, 2.72f, 2.92f, 0.04f), Rgba.Hex("C9775A"));
            string[] walls = { "F3E3C2", "FFD8C9", "D9F0D3", "D7E4FA", "F7E0F2", "FFF1B5" };
            for (int k = 0; k <= 5; k++)
            {
                float y0 = b0 + k * h;
                Sdf room = Sd.Box(0.62f, y0 + h * 0.5f + 0.02f, 1.93f, h * 0.5f - 0.07f);
                p.Fill(room, Rgba.Hex(walls[k]), false);
                // carta da parati a righine
                for (float x = -1.2f; x < 2.55f; x += 0.3f) p.Line(V40E_Ink.WithAlpha(0.06f), 0.03f, x, y0 + 0.12f, x, y0 + h - 0.1f);
                // pavimento
                p.Fill(Sd.Box(0.62f, y0 + 0.06f, 1.93f, 0.05f), Rgba.Hex("A0663E"), false);
                // finestra a destra
                if (k > 0)
                {
                    p.Shape(Sd.Box(2.15f, y0 + 0.52f, 0.26f, 0.24f, 0.03f), Rgba.Hex("BFE6FF"), 0.035f);
                    p.InkLine(0.025f, 2.15f, y0 + 0.28f, 2.15f, y0 + 0.76f);
                }
                // solaio
                p.Shape(Sd.Box(0, y0 + h, 2.72f, 0.045f), Rgba.Hex("8E4B36"), 0.03f);
            }
            // vano dell'ascensore
            p.FillFunc(Sd.Box(-1.975f, -1.4f, 0.62f, 2.85f), (x, y) => Rgba.Lerp(Rgba.Hex("3D3A4A"), Rgba.Hex("555066"), Clamp((x + 2.6f) / 1.3f)));
            foreach (float x in new[] { -2.45f, -1.5f }) p.Line(Rgba.Hex("2A2734"), 0.05f, x, -4.25f, x, 1.45f);
            p.Line(Rgba.Hex("9C98AA"), 0.025f, -1.975f, 1.45f, -1.975f, -4.0f);
            p.Shape(Sd.Box(-1.33f, -1.4f, 0.04f, 2.85f), Rgba.Hex("8E4B36"), 0.03f);
            // tetto con insegna
            p.Shape(Sd.Polygon(-2.85f, 1.45f, 2.85f, 1.45f, 2.6f, 1.62f, -2.6f, 1.62f), Rgba.Hex("7A3F2C"));
            // porta dell'atrio
            p.Shape(Sd.Box(1.85f, -3.85f, 0.32f, 0.45f, 0.05f), Rgba.Hex("6E3A1A"));
            p.Fill(Sd.Circle(1.62f, -3.85f, 0.04f), Gold);
            // tappeto rosso dell'atrio
            p.Fill(Sd.Box(0.3f, -4.17f, 1.4f, 0.04f), Rgba.Hex("D84A4A"), false);
            return new DoodleArt("e40_tower_bg", p);
        }

        /// <summary>Cabina dell'ascensore aperta (base in y 0, larga 1.16, alta 0.82).</summary>
        public static DoodleArt V40E_ElevatorCar()
        {
            var p = new DoodlePainter(-0.64f, -0.06f, 0.64f, 0.92f, 140) { Ink = 0.045f };
            Sdf frame = Sd.Box(0, 0.42f, 0.58f, 0.42f, 0.04f);
            p.Shape(frame, Rgba.Hex("D9A441"));
            p.FillFunc(Sd.Box(0, 0.4f, 0.5f, 0.34f, 0.02f), (x, y) => Rgba.Lerp(Rgba.Hex("7A5A9A"), Rgba.Hex("9C7CC0"), Clamp((y - 0.06f) / 0.7f)));
            p.Shape(Sd.Box(0, 0.4f, 0.5f, 0.34f, 0.02f), Rgba.Clear, 0.03f);
            // lucina in alto e pavimento
            p.Shape(Sd.Box(0, 0.8f, 0.14f, 0.03f, 0.02f), Rgba.Hex("FFF3B0"), 0.02f);
            p.Shape(Sd.Box(0, 0.05f, 0.56f, 0.05f, 0.02f), Rgba.Hex("8A6A2A"), 0.03f);
            // cavo
            p.Line(Rgba.Hex("2A2734"), 0.04f, 0, 0.84f, 0, 0.92f);
            return new DoodleArt("e40_car", p);
        }

        /// <summary>Pulsante di piano in ottone (raggio 0.22; il numero è un'etichetta).</summary>
        public static DoodleArt V40E_FloorButton()
        {
            var p = new DoodlePainter(-0.27f, -0.27f, 0.27f, 0.27f, 220) { Ink = 0.035f };
            Body(p, Sd.Circle(0, 0, 0.23f), Gold, GoldShade);
            p.Shape(Sd.Circle(0, 0, 0.17f), Rgba.Hex("FFF6D8"), 0.025f);
            return new DoodleArt("e40_floor_btn", p);
        }

        /// <summary>Foglio degli indizi appuntato (5.3 x 1.2, centro 0,0).</summary>
        public static DoodleArt V40E_ClueBoard()
        {
            var p = new DoodlePainter(-2.75f, -0.68f, 2.75f, 0.68f, 90) { Ink = 0.045f };
            p.Fill(Sd.Box(0.05f, -0.06f, 2.62f, 0.6f, 0.06f), V40E_Ink.WithAlpha(0.25f));
            p.Shape(Sd.Box(0, 0, 2.62f, 0.6f, 0.06f), Rgba.Hex("FFFBEA"));
            for (float y = -0.45f; y < 0.5f; y += 0.19f) p.Line(Rgba.Hex("9CC3EA").WithAlpha(0.6f), 0.015f, -2.5f, y, 2.5f, y);
            p.Line(Rgba.Hex("E87A7A").WithAlpha(0.7f), 0.02f, -2.35f, -0.56f, -2.35f, 0.56f);
            foreach (float x in new[] { -2.3f, 2.3f }) p.Shape(Sd.Circle(x, 0.5f, 0.07f), Rgba.Hex("E8433F"), 0.025f);
            return new DoodleArt("e40_clue_board", p);
        }

        /// <summary>Animaletti seduti degli inquilini (base in y 0, larghi ~0.6).</summary>
        public static DoodleArt V40E_Animal(string name)
        {
            var p = new DoodlePainter(-0.36f, -0.04f, 0.36f, 0.95f, 160) { Ink = 0.035f };
            Rgba body, shade;
            switch (name)
            {
                case "e40_cat":
                    body = CatOrange; shade = CatShade;
                    p.Shape(Sd.Capsule(0.18f, 0.08f, 0.3f, 0.4f, 0.045f), body);
                    Body(p, Sd.Ellipse(0, 0.2f, 0.22f, 0.2f), body, shade);
                    Sdf ears = Sd.Union(Sd.Polygon(-0.2f, 0.52f, -0.17f, 0.74f, -0.04f, 0.6f), Sd.Polygon(0.2f, 0.52f, 0.17f, 0.74f, 0.04f, 0.6f));
                    p.Shape(ears, body);
                    Body(p, Sd.Circle(0, 0.5f, 0.19f), body, shade);
                    p.Fill(Sd.Polygon(-0.16f, 0.6f, -0.15f, 0.69f, -0.09f, 0.62f), CatPink);
                    p.Fill(Sd.Polygon(0.16f, 0.6f, 0.15f, 0.69f, 0.09f, 0.62f), CatPink);
                    V40E_Face(p, 0.5f, CatPink);
                    p.InkLine(0.012f, 0.08f, 0.45f, 0.25f, 0.47f);
                    p.InkLine(0.012f, -0.08f, 0.45f, -0.25f, 0.47f);
                    break;
                case "e40_dog":
                    body = Rgba.Hex("C98B55"); shade = Rgba.Hex("9C6436");
                    Body(p, Sd.Ellipse(0, 0.2f, 0.23f, 0.2f), body, shade);
                    p.Fill(Sd.Ellipse(0, 0.15f, 0.12f, 0.12f), Rgba.Hex("F2D7B5"));
                    Body(p, Sd.Circle(0, 0.5f, 0.2f), body, shade);
                    p.Shape(Sd.Rotate(Sd.Ellipse(-0.2f, 0.47f, 0.07f, 0.16f), -0.2f, 0.47f, -15f), Rgba.Hex("6E4325"));
                    p.Shape(Sd.Rotate(Sd.Ellipse(0.2f, 0.47f, 0.07f, 0.16f), 0.2f, 0.47f, 15f), Rgba.Hex("6E4325"));
                    p.Shape(Sd.Ellipse(0, 0.42f, 0.1f, 0.075f), Rgba.Hex("F2D7B5"), 0.025f);
                    p.Fill(Sd.Ellipse(0, 0.46f, 0.045f, 0.03f), V40E_Ink);
                    p.Fill(Sd.Circle(-0.075f, 0.56f, 0.03f), V40E_Ink);
                    p.Fill(Sd.Circle(0.075f, 0.56f, 0.03f), V40E_Ink);
                    p.Fill(Sd.Circle(-0.085f, 0.57f, 0.01f), Rgba.White);
                    p.Fill(Sd.Circle(0.065f, 0.57f, 0.01f), Rgba.White);
                    p.Shape(Sd.Capsule(0, 0.37f, 0.01f, 0.34f, 0.025f), Rgba.Hex("FF7F9C"), 0.015f);
                    break;
                case "e40_parrot":
                    body = Rgba.Hex("4CC25A"); shade = Rgba.Hex("2E8E3E");
                    p.Shape(Sd.Polygon(-0.05f, 0.1f, 0.05f, 0.1f, 0.02f, -0.02f), Rgba.Hex("3B7DE0"));
                    Body(p, Sd.Ellipse(0, 0.3f, 0.19f, 0.27f), body, shade);
                    p.Shape(Sd.Rotate(Sd.Ellipse(0.14f, 0.26f, 0.08f, 0.18f), 0.14f, 0.26f, -12f), Rgba.Hex("E8433F"));
                    Body(p, Sd.Circle(0, 0.62f, 0.17f), body, shade);
                    p.Shape(Sd.Union(Sd.Circle(0, 0.81f, 0.05f), Sd.Circle(-0.06f, 0.79f, 0.04f)), Rgba.Hex("FFC83D"), 0.02f);
                    p.Fill(Sd.Circle(0.06f, 0.67f, 0.06f), Rgba.White);
                    p.Fill(Sd.Circle(0.07f, 0.67f, 0.028f), V40E_Ink);
                    p.Shape(Sd.Polygon(0.12f, 0.62f, 0.27f, 0.56f, 0.16f, 0.48f), Rgba.Hex("FFC83D"), 0.025f);
                    p.Line(Rgba.Hex("E39B1B"), 0.035f, -0.1f, 0.02f, 0.1f, 0.02f);
                    break;
                default: // coniglietto
                    body = Rgba.Hex("F4F0F6"); shade = Rgba.Hex("CFC6DA");
                    Body(p, Sd.Ellipse(0, 0.2f, 0.22f, 0.2f), body, shade);
                    p.Shape(Sd.Circle(0.21f, 0.12f, 0.06f), Rgba.White);
                    foreach (float sx in new[] { -1f, 1f })
                    {
                        Sdf ear = Sd.Rotate(Sd.Ellipse(sx * 0.08f, 0.76f, 0.055f, 0.17f), sx * 0.08f, 0.62f, -sx * 8f);
                        p.Shape(ear, body);
                        p.Fill(Sd.Rotate(Sd.Ellipse(sx * 0.08f, 0.76f, 0.025f, 0.12f), sx * 0.08f, 0.62f, -sx * 8f), Rgba.Hex("FFB6C8"));
                    }
                    Body(p, Sd.Circle(0, 0.46f, 0.18f), body, shade);
                    V40E_Face(p, 0.46f, Rgba.Hex("FF8FB0"));
                    p.Shape(Sd.Box(0, 0.36f, 0.035f, 0.025f, 0.01f), Rgba.White, 0.012f);
                    break;
            }
            return new DoodleArt(name, p);
        }

        static void V40E_Face(DoodlePainter p, float cy, Rgba nose)
        {
            p.Fill(Sd.Circle(-0.07f, cy + 0.03f, 0.028f), V40E_Ink);
            p.Fill(Sd.Circle(0.07f, cy + 0.03f, 0.028f), V40E_Ink);
            p.Fill(Sd.Circle(-0.08f, cy + 0.04f, 0.01f), Rgba.White);
            p.Fill(Sd.Circle(0.06f, cy + 0.04f, 0.01f), Rgba.White);
            p.Fill(Sd.Polygon(-0.025f, cy - 0.02f, 0.025f, cy - 0.02f, 0f, cy - 0.05f), nose);
            p.InkLine(0.012f, -0.04f, cy - 0.08f, 0f, cy - 0.06f, 0.04f, cy - 0.08f);
            p.Fill(Sd.Circle(-0.12f, cy - 0.04f, 0.03f), BlushColor.WithAlpha(0.5f));
            p.Fill(Sd.Circle(0.12f, cy - 0.04f, 0.03f), BlushColor.WithAlpha(0.5f));
        }

        // ---------- Circuit board ----------

        /// <summary>Laboratorio con parete di piastrelle e banco (coordinate del livello).</summary>
        public static DoodleArt V40E_WorkshopBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 5f, 7.5f), (x, y) => Rgba.Lerp(Rgba.Hex("4B5D73"), Rgba.Hex("6C819A"), Clamp((y + 3f) / 9f)));
            for (float y = -3.2f; y < 7f; y += 0.6f)
                for (float x = -4.5f; x < 4.6f; x += 0.6f)
                    p.Shape(Sd.Box(x + 0.3f, y + 0.3f, 0.27f, 0.27f, 0.04f), Rgba.Hex("7990AA").WithAlpha(0.45f), 0.012f);
            // banco
            p.Fill(Sd.Box(0, -5.6f, 5f, 2.0f), Rgba.Hex("8A5A3B"), false);
            p.Shape(Sd.Box(0, -3.45f, 5f, 0.12f), Rgba.Hex("A8744C"));
            return new DoodleArt("e40_workshop_bg", p);
        }

        /// <summary>Porta della cassaforte d'acciaio attorno alla scheda (centro 0,0; 5.5 x 5.9; cardine a sinistra).</summary>
        public static DoodleArt V40E_SafeDoor()
        {
            var p = new DoodlePainter(-2.85f, -3.05f, 2.85f, 3.05f, 60) { Ink = 0.06f };
            Sdf door = Sd.Box(0, 0, 2.75f, 2.95f, 0.18f);
            Body(p, door, Rgba.Hex("A7B1C2"), Rgba.Hex("7F8A9D"));
            p.Shape(Sd.Box(0, 0, 2.55f, 2.75f, 0.12f), Rgba.Hex("BAC3D2"), 0.035f);
            foreach (float y in new[] { 2.0f, -2.0f }) p.Shape(Sd.Box(-2.75f, y, 0.12f, 0.35f, 0.05f), Rgba.Hex("5E6778"), 0.035f);
            foreach (var c in new[] { (-2.45f, 2.65f), (2.45f, 2.65f), (-2.45f, -2.65f), (2.45f, -2.65f) })
                p.Shape(Sd.Circle(c.Item1, c.Item2, 0.07f), Rgba.Hex("D9DEE7"), 0.025f);
            return new DoodleArt("e40_safe_door", p);
        }

        /// <summary>Interno della cassaforte aperta (centro 0,0; 5.5 x 5.9) con il formaggio d'oro.</summary>
        public static DoodleArt V40E_SafeInside()
        {
            var p = new DoodlePainter(-2.85f, -3.05f, 2.85f, 3.05f, 50) { Ink = 0.06f };
            p.Shape(Sd.Box(0, 0, 2.75f, 2.95f, 0.18f), Rgba.Hex("2B2F3A"));
            p.Fill(Sd.Box(0, -0.6f, 2.5f, 0.06f), Rgba.Hex("5E6778"));
            // forma di formaggio d'oro
            Sdf cheese = Sd.Polygon(-1.0f, -0.55f, 1.0f, -0.55f, 1.0f, 0.35f, -1.0f, -0.15f);
            Body(p, cheese, Gold, GoldShade);
            foreach (var c in new[] { (-0.4f, -0.3f, 0.12f), (0.3f, -0.1f, 0.15f), (0.7f, 0.15f, 0.08f) })
                p.Shape(Sd.Circle(c.Item1, c.Item2, c.Item3), GoldShade, 0.025f);
            p.FillFunc(Sd.Circle(0, -0.1f, 2.2f), (x, y) => GoldLight.WithAlpha((float)Math.Exp(-Math.Sqrt(x * x + (y + 0.1f) * (y + 0.1f)) * 1.4f) * 0.4f));
            return new DoodleArt("e40_safe_inside", p);
        }

        /// <summary>Scheda elettronica verde 5 x 5 celle da 0.95 (centro 0,0).</summary>
        public static DoodleArt V40E_Pcb()
        {
            var p = new DoodlePainter(-2.48f, -2.48f, 2.48f, 2.48f, 60) { Ink = 0.05f };
            p.Shape(Sd.Box(0, 0, 2.42f, 2.42f, 0.1f), Rgba.Hex("2F8A5A"));
            for (int i = -2; i <= 3; i++)
            {
                float v = (i - 0.5f) * 0.95f;
                p.Line(Rgba.Hex("53B07E").WithAlpha(0.5f), 0.015f, v, -2.38f, v, 2.38f);
                p.Line(Rgba.Hex("53B07E").WithAlpha(0.5f), 0.015f, -2.38f, v, 2.38f, v);
            }
            for (int i = 0; i < 40; i++)
            {
                float x = -2.2f + Noise.Value(i * 3.3f, 1.7f) * 4.4f, y = -2.2f + Noise.Value(2.1f, i * 2.9f) * 4.4f;
                p.Fill(Sd.Circle(x, y, 0.03f), Rgba.Hex("CDE8D8").WithAlpha(0.5f));
            }
            return new DoodleArt("e40_pcb", p);
        }

        /// <summary>Zoccolo tratteggiato dove va un componente (cella 0.95, centro 0,0).</summary>
        public static DoodleArt V40E_Socket()
        {
            var p = new DoodlePainter(-0.48f, -0.48f, 0.48f, 0.48f, 140) { Wobble = 0f };
            p.Fill(Sd.Box(0, 0, 0.42f, 0.42f, 0.06f), Rgba.Hex("1E5E3C").WithAlpha(0.8f));
            Sdf ring = Sd.Subtract(Sd.Box(0, 0, 0.42f, 0.42f, 0.06f), Sd.Box(0, 0, 0.385f, 0.385f, 0.04f));
            p.FillFunc(ring, (x, y) => ((int)Math.Floor((x + y + 2f) / 0.12f)) % 2 == 0 ? Rgba.Hex("BFF2D4") : Rgba.Clear);
            return new DoodleArt("e40_socket", p);
        }

        /// <summary>Vassoio dei componenti (5.5 x 1.1, centro 0,0).</summary>
        public static DoodleArt V40E_Tray()
        {
            var p = new DoodlePainter(-2.82f, -0.62f, 2.82f, 0.62f, 80) { Ink = 0.05f };
            Body(p, Sd.Box(0, 0, 2.75f, 0.55f, 0.12f), Rgba.Hex("3A4150"), Rgba.Hex("272C37"));
            for (int i = 0; i < 5; i++) p.Shape(Sd.Box(-2.1f + i * 1.05f, 0.02f, 0.47f, 0.45f, 0.08f), Rgba.Hex("4D566A"), 0.03f);
            return new DoodleArt("e40_tray", p);
        }

        /// <summary>Display a tre cifre (2.0 x 0.6, centro 0,0).</summary>
        public static DoodleArt V40E_Display()
        {
            var p = new DoodlePainter(-1.08f, -0.36f, 1.08f, 0.36f, 120) { Ink = 0.045f };
            p.Shape(Sd.Box(0, 0, 1.0f, 0.3f, 0.06f), Rgba.Hex("4A4655"));
            for (int i = -1; i <= 1; i++) p.Shape(Sd.Box(i * 0.62f, 0, 0.27f, 0.22f, 0.04f), Rgba.Hex("140F18"), 0.025f);
            return new DoodleArt("e40_display", p);
        }

        /// <summary>Pista di rame (si tinge): 0 dritta W-E, 1 curva E-N, 2 incrocio a quattro vie (cella 0.95).</summary>
        public static DoodleArt V40E_Wire(int kind)
        {
            var p = new DoodlePainter(-0.48f, -0.48f, 0.48f, 0.48f, 140) { Ink = 0.03f, Wobble = 0.004f };
            Rgba cu = Rgba.Hex("F4F1EA");
            Sdf trace = kind == 0 ? Sd.Capsule(-0.47f, 0, 0.47f, 0, 0.07f)
                : kind == 1 ? Sd.Union(Sd.Capsule(0.47f, 0, 0, 0, 0.07f), Sd.Capsule(0, 0, 0, 0.47f, 0.07f))
                : Sd.Union(Sd.Capsule(-0.47f, 0, 0.47f, 0, 0.07f), Sd.Capsule(0, -0.47f, 0, 0.47f, 0.07f));
            if (kind == 0 || kind == 1)
                p.Fill(Sd.Box(0, 0, 0.36f, 0.36f, 0.06f), Rgba.Hex("163F2A").WithAlpha(0.75f));
            p.Shape(trace, cu);
            p.Fill(Sd.Capsule(-0.3f, 0.025f, 0.3f, 0.025f, 0.015f), Rgba.White.WithAlpha(kind == 1 ? 0f : 0.7f));
            p.Shape(Sd.Circle(0, 0, kind == 2 ? 0.13f : 0.1f), Rgba.Hex("D9DEE7"), 0.025f);
            string n = kind == 0 ? "e40_wire_straight" : kind == 1 ? "e40_wire_corner" : "e40_wire_cross";
            return new DoodleArt(n, p);
        }

        /// <summary>Interruttore a leva sulla pista W-E (cella 0.95): aperto (leva su) o chiuso.</summary>
        public static DoodleArt V40E_Switch(bool on)
        {
            var p = new DoodlePainter(-0.48f, -0.48f, 0.48f, 0.48f, 140) { Ink = 0.03f, Wobble = 0.004f };
            p.Fill(Sd.Box(0, 0, 0.4f, 0.4f, 0.08f), Rgba.Hex("2A2D38"));
            p.Shape(Sd.Capsule(-0.47f, 0, -0.22f, 0, 0.07f), Rgba.Hex("F4F1EA"));
            p.Shape(Sd.Capsule(0.22f, 0, 0.47f, 0, 0.07f), Rgba.Hex("F4F1EA"));
            p.Shape(Sd.Circle(-0.22f, 0, 0.07f), Rgba.Hex("D9DEE7"), 0.025f);
            p.Shape(Sd.Circle(0.22f, 0, 0.07f), Rgba.Hex("D9DEE7"), 0.025f);
            float ex = on ? 0.25f : 0.08f, ey = on ? 0.02f : 0.3f;
            p.Shape(Sd.Capsule(-0.22f, 0, ex, ey, 0.04f), Rgba.Hex("C9CED8"));
            p.Shape(Sd.Circle(ex, ey, 0.075f), on ? Rgba.Hex("4CC25A") : Rgba.Hex("E8433F"), 0.025f);
            return new DoodleArt(on ? "e40_switch_on" : "e40_switch_off", p);
        }

        /// <summary>Resistenza a strisce colorate sulla pista W-E (cella 0.95).</summary>
        public static DoodleArt V40E_Resistor()
        {
            var p = new DoodlePainter(-0.48f, -0.48f, 0.48f, 0.48f, 140) { Ink = 0.03f, Wobble = 0.004f };
            p.Fill(Sd.Box(0, 0, 0.36f, 0.36f, 0.06f), Rgba.Hex("163F2A").WithAlpha(0.75f));
            p.Shape(Sd.Capsule(-0.47f, 0, 0.47f, 0, 0.04f), Rgba.Hex("D9DEE7"));
            Sdf body = Sd.Capsule(-0.22f, 0, 0.22f, 0, 0.12f);
            Body(p, body, Rgba.Hex("E8C99A"), Rgba.Hex("C9A46E"));
            string[] bands = { "8A4B2A", "1E1A24", "E8433F", "FFC93C" };
            for (int i = 0; i < 4; i++) p.Fill(Sd.Intersect(body, Sd.Box(-0.16f + i * 0.1f + (i == 3 ? 0.04f : 0f), 0, 0.025f, 0.2f)), Rgba.Hex(bands[i]));
            p.Shape(body, Rgba.Clear, 0.03f);
            return new DoodleArt("e40_resistor", p);
        }

        /// <summary>Pila da 9V con l'uscita verso nord (cella 0.95).</summary>
        public static DoodleArt V40E_Battery()
        {
            var p = new DoodlePainter(-0.48f, -0.48f, 0.48f, 0.48f, 140) { Ink = 0.035f, Wobble = 0.004f };
            p.Shape(Sd.Capsule(0, 0.2f, 0, 0.47f, 0.07f), Rgba.Hex("FFE680"));
            Sdf body = Sd.Box(0, -0.08f, 0.3f, 0.3f, 0.06f);
            Body(p, body, Rgba.Hex("3B3A46"), Rgba.Hex("26252E"));
            p.Fill(Sd.Box(0, 0.08f, 0.28f, 0.11f), Rgba.Hex("FFC93C"));
            p.Shape(body, Rgba.Clear);
            p.Shape(Sd.Box(-0.12f, 0.27f, 0.06f, 0.05f, 0.02f), Rgba.Hex("D9DEE7"), 0.02f);
            p.Shape(Sd.Box(0.12f, 0.27f, 0.07f, 0.05f, 0.02f), Rgba.Hex("D9DEE7"), 0.02f);
            return new DoodleArt("e40_battery", p);
        }

        /// <summary>Lampadina con l'attacco verso sud (cella 0.95), spenta o accesa.</summary>
        public static DoodleArt V40E_Bulb(bool on)
        {
            var p = new DoodlePainter(-0.48f, -0.48f, 0.48f, 0.48f, 140) { Ink = 0.035f, Wobble = 0.004f };
            if (on) p.FillFunc(Sd.Circle(0, 0.12f, 0.48f), (x, y) => Rgba.Hex("FFE680").WithAlpha((float)Math.Max(0, 1 - Math.Sqrt(x * x + (y - 0.12f) * (y - 0.12f)) / 0.48f) * 0.8f));
            p.Shape(Sd.Capsule(0, -0.47f, 0, -0.3f, 0.07f), Rgba.Hex("F4F1EA"));
            Sdf screw = Sd.Box(0, -0.22f, 0.11f, 0.09f, 0.03f);
            p.Shape(screw, Rgba.Hex("B7BDC9"));
            p.InkLine(0.015f, -0.1f, -0.2f, 0.1f, -0.18f);
            p.InkLine(0.015f, -0.1f, -0.26f, 0.1f, -0.24f);
            Sdf glass = Sd.Union(Sd.Circle(0, 0.12f, 0.23f), Sd.Box(0, -0.09f, 0.1f, 0.06f));
            p.Shape(glass, on ? Rgba.Hex("FFF27A") : Rgba.Hex("E4ECF2"));
            p.InkLine(0.015f, -0.05f, -0.08f, -0.06f, 0.1f, 0f, 0.16f, 0.06f, 0.1f, 0.05f, -0.08f);
            p.Fill(Sd.Capsule(-0.12f, 0.2f, -0.06f, 0.28f, 0.03f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt(on ? "e40_bulb_on" : "e40_bulb_off", p);
        }

        /// <summary>LED rosso con l'attacco verso sud (cella 0.95): 0 spento, 1 acceso, 2 bruciato.</summary>
        public static DoodleArt V40E_Led(int state)
        {
            var p = new DoodlePainter(-0.48f, -0.48f, 0.48f, 0.48f, 140) { Ink = 0.035f, Wobble = 0.004f };
            if (state == 1) p.FillFunc(Sd.Circle(0, 0.08f, 0.46f), (x, y) => Rgba.Hex("FF4D5E").WithAlpha((float)Math.Max(0, 1 - Math.Sqrt(x * x + (y - 0.08f) * (y - 0.08f)) / 0.46f) * 0.75f));
            p.Shape(Sd.Capsule(-0.07f, -0.47f, -0.07f, -0.15f, 0.025f), Rgba.Hex("D9DEE7"), 0.02f);
            p.Shape(Sd.Capsule(0.07f, -0.47f, 0.07f, -0.2f, 0.025f), Rgba.Hex("D9DEE7"), 0.02f);
            Sdf dome = Sd.Union(Sd.Circle(0, 0.12f, 0.17f), Sd.Box(0, -0.02f, 0.17f, 0.12f));
            Rgba c = state == 0 ? Rgba.Hex("C23A48") : state == 1 ? Rgba.Hex("FF6B78") : Rgba.Hex("3A3540");
            p.Shape(dome, c);
            p.Shape(Sd.Box(0, -0.15f, 0.21f, 0.04f, 0.015f), state == 2 ? Rgba.Hex("26252E") : Rgba.Hex("A82E3A"), 0.025f);
            if (state != 2) p.Fill(Sd.Capsule(-0.08f, 0.0f, -0.08f, 0.15f, 0.03f), Rgba.White.WithAlpha(0.6f));
            else
            {
                p.InkLine(0.02f, -0.1f, 0.2f, 0f, 0.05f, 0.08f, 0.18f);
                p.Fill(Sd.Union(Sd.Circle(0.1f, 0.36f, 0.08f), Sd.Circle(0.2f, 0.42f, 0.06f), Sd.Circle(0.02f, 0.42f, 0.05f)), Rgba.Hex("6B6677").WithAlpha(0.7f));
            }
            string n = state == 0 ? "e40_led_off" : state == 1 ? "e40_led_on" : "e40_led_burnt";
            return new DoodleArt(n, p);
        }

        // ---------- Snow globe ----------

        /// <summary>Salotto accogliente di sera (coordinate del livello; mensola in y -3.65).</summary>
        public static DoodleArt V40E_CozyBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 5f, 7.5f), (x, y) => Rgba.Lerp(Rgba.Hex("5A3A5E"), Rgba.Hex("7E5279"), Clamp((y + 4f) / 10f)));
            for (float x = -4.4f; x < 4.6f; x += 0.55f)
                p.Line(Rgba.Hex("8E6390").WithAlpha(0.5f), 0.12f, x, -4f, x, 7f);
            // finestra con la neve
            p.Shape(Sd.Box(-2.2f, 1.6f, 0.9f, 1.3f, 0.08f), Rgba.Hex("8A5A3B"));
            p.FillFunc(Sd.Box(-2.2f, 1.6f, 0.75f, 1.15f, 0.04f), (x, y) => Rgba.Lerp(Rgba.Hex("1B2350"), Rgba.Hex("31407E"), Clamp((y - 0.4f) / 2.3f)));
            for (int i = 0; i < 26; i++)
                p.Fill(Sd.Circle(-2.9f + Noise.Value(i * 1.9f, 0.3f) * 1.4f, 0.5f + Noise.Value(0.7f, i * 2.3f) * 2.2f, 0.03f), Rgba.White.WithAlpha(0.8f));
            p.InkLine(0.04f, -2.2f, 0.45f, -2.2f, 2.75f);
            p.InkLine(0.04f, -2.95f, 1.6f, -1.45f, 1.6f);
            // luce calda della lampada
            p.FillFunc(Sd.Circle(2.0f, 1.5f, 2.6f), (x, y) => Rgba.Hex("FFD27A").WithAlpha((float)Math.Exp(-Math.Sqrt((x - 2f) * (x - 2f) + (y - 1.5f) * (y - 1.5f)) * 1.2f) * 0.45f));
            return new DoodleArt("e40_cozy_bg", p);
        }

        /// <summary>Mensola di legno (5.6 x 0.3, piano in y 0).</summary>
        public static DoodleArt V40E_Shelf()
        {
            var p = new DoodlePainter(-2.95f, -0.42f, 2.95f, 0.08f, 80) { Ink = 0.05f };
            Body(p, Sd.Box(0, -0.15f, 2.85f, 0.17f, 0.04f), Wood, WoodShade);
            return new DoodleArt("e40_shelf", p);
        }

        /// <summary>Paesaggio dentro la sfera (centro della sfera 0,0, raggio 1.45): neve, casetta e abete.</summary>
        public static DoodleArt V40E_GlobeInside()
        {
            var p = new DoodlePainter(-1.5f, -1.5f, 1.5f, 1.5f, 100) { Ink = 0.04f };
            Sdf sphere = Sd.Circle(0, 0, 1.42f);
            p.FillFunc(sphere, (x, y) => Rgba.Lerp(Rgba.Hex("8FC6EE"), Rgba.Hex("3E6FB8"), Clamp((y + 1f) / 2.4f)));
            // abete
            Sdf tree = Sd.Union(Sd.Polygon(0.45f, -0.55f, 1.15f, -0.55f, 0.8f, 0.0f), Sd.Polygon(0.52f, -0.2f, 1.08f, -0.2f, 0.8f, 0.35f));
            p.Shape(Sd.Box(0.8f, -0.66f, 0.06f, 0.1f), Twig);
            Body(p, tree, Leaf, LeafShade);
            // casetta
            Sdf house = Sd.Box(-0.45f, -0.5f, 0.42f, 0.32f, 0.02f);
            Body(p, house, Rgba.Hex("E8433F"), Rgba.Hex("B92E2A"));
            p.Shape(Sd.Polygon(-0.95f, -0.2f, 0.05f, -0.2f, -0.45f, 0.22f), Rgba.White);
            p.Shape(Sd.Box(-0.6f, -0.6f, 0.1f, 0.15f, 0.02f), Rgba.Hex("6E3A1A"), 0.03f);
            p.Shape(Sd.Box(-0.25f, -0.42f, 0.1f, 0.09f, 0.02f), Rgba.Hex("FFE680"), 0.03f);
            // neve per terra
            Sdf ground = Sd.Intersect(sphere, Sd.Ellipse(0, -1.4f, 1.8f, 0.72f));
            p.Shape(ground, Snow);
            p.Fill(Sd.Intersect(ground, Sd.Ellipse(0.4f, -1.5f, 1.0f, 0.4f)), SnowShade);
            return new DoodleArt("e40_globe_inside", p);
        }

        /// <summary>Cupola di vetro della sfera (centro 0,0, raggio 1.45): trasparente con riflessi.</summary>
        public static DoodleArt V40E_GlobeDome()
        {
            var p = new DoodlePainter(-1.55f, -1.55f, 1.55f, 1.55f, 100) { Ink = 0.06f };
            Sdf sphere = Sd.Circle(0, 0, 1.45f);
            p.Fill(Sd.Subtract(sphere, Sd.Circle(0.12f, -0.1f, 1.38f)), Rgba.White.WithAlpha(0.35f));
            p.Fill(Sd.Capsule(-0.95f, 0.55f, -0.55f, 1.0f, 0.08f), Rgba.White.WithAlpha(0.7f));
            p.Fill(Sd.Circle(-1.0f, 0.3f, 0.06f), Rgba.White.WithAlpha(0.7f));
            p.Shape(sphere, Rgba.Clear);
            return new DoodleArt("e40_globe_dome", p);
        }

        /// <summary>Collare di legno sotto la cupola (larga 2.6, centro 0,0, alta 0.45).</summary>
        public static DoodleArt V40E_GlobeCollar()
        {
            var p = new DoodlePainter(-1.45f, -0.3f, 1.45f, 0.3f, 100) { Ink = 0.05f };
            Sdf c = Sd.Polygon(-1.05f, 0.22f, 1.05f, 0.22f, 1.35f, -0.22f, -1.35f, -0.22f);
            Body(p, c, Wood, WoodShade);
            p.Line(GoldShade, 0.04f, -1.15f, -0.05f, 1.15f, -0.05f);
            return new DoodleArt("e40_globe_collar", p);
        }

        /// <summary>Anello d'ottone della base (3.7 x 0.66, centro 0,0) su cui scorrono le cifre.</summary>
        public static DoodleArt V40E_RingBand()
        {
            var p = new DoodlePainter(-1.95f, -0.4f, 1.95f, 0.4f, 100) { Ink = 0.045f };
            Sdf band = Sd.Box(0, 0, 1.85f, 0.32f, 0.12f);
            p.FillFunc(band, (x, y) =>
            {
                float k = Clamp(1f - Math.Abs(x) / 1.9f);
                return Rgba.Lerp(Rgba.Hex("8A5A1E"), Rgba.Hex("F7D47A"), (float)Math.Sqrt(k)) ;
            });
            p.Fill(Sd.Box(0, 0.2f, 1.75f, 0.04f), Rgba.White.WithAlpha(0.3f));
            p.Shape(band, Rgba.Clear);
            return new DoodleArt("e40_ring_band", p);
        }

        /// <summary>Freccia rossa che indica la colonna giusta (punta in basso in 0,0).</summary>
        public static DoodleArt V40E_RingArrow()
        {
            var p = new DoodlePainter(-0.22f, -0.04f, 0.22f, 0.36f, 200) { Ink = 0.035f };
            p.Shape(Sd.Polygon(-0.17f, 0.3f, 0.17f, 0.3f, 0f, 0.02f), Rgba.Hex("E8433F"));
            return new DoodleArt("e40_ring_arrow", p);
        }

        /// <summary>Fiocco di neve (raggio 0.06).</summary>
        public static DoodleArt V40E_Flake()
        {
            var p = new DoodlePainter(-0.08f, -0.08f, 0.08f, 0.08f, 300) { Wobble = 0f };
            p.Fill(Sd.Circle(0, 0, 0.065f), Rgba.Hex("B8D4EE"));
            p.Fill(Sd.Circle(0, 0, 0.05f), Rgba.White);
            return new DoodleArt("e40_flake", p);
        }

        /// <summary>Carillon (2.2 x 1.2, base in y 0; serratura in 0, 0.45): chiuso o aperto con la gattina ballerina.</summary>
        public static DoodleArt V40E_MusicBox(bool open)
        {
            var p = new DoodlePainter(-1.25f, -0.06f, 1.25f, open ? 2.3f : 1.45f, 110) { Ink = 0.05f };
            Rgba box = Rgba.Hex("3E8EF7"), boxShade = Rgba.Hex("2563C9");
            if (open)
            {
                // coperchio aperto dietro
                Sdf lid = Sd.Polygon(-1.0f, 1.0f, 1.0f, 1.0f, 0.85f, 2.1f, -0.85f, 2.1f);
                Body(p, lid, boxShade, Rgba.Hex("1D4F9E"));
                p.Shape(Sd.Box(0, 1.55f, 0.55f, 0.38f, 0.05f), Rgba.Hex("D7E4FA"), 0.03f);
                // gattina ballerina sul perno
                p.Line(Rgba.Hex("B7BDC9"), 0.05f, 0, 1.0f, 0, 1.25f);
                p.Shape(Sd.Polygon(-0.3f, 1.25f, 0.3f, 1.25f, 0.12f, 1.48f, -0.12f, 1.48f), Rgba.Hex("FF9BB0"));
                Body(p, Sd.Ellipse(0, 1.6f, 0.13f, 0.17f), CatOrange, CatShade);
                p.Shape(Sd.Union(Sd.Polygon(-0.16f, 1.8f, -0.15f, 1.98f, -0.04f, 1.86f), Sd.Polygon(0.16f, 1.8f, 0.15f, 1.98f, 0.04f, 1.86f)), CatOrange);
                Body(p, Sd.Circle(0, 1.82f, 0.15f), CatOrange, CatShade);
                p.InkLine(0.015f, -0.09f, 1.84f, -0.04f, 1.82f);
                p.InkLine(0.015f, 0.04f, 1.82f, 0.09f, 1.84f);
                p.Fill(Sd.Polygon(-0.02f, 1.77f, 0.02f, 1.77f, 0f, 1.75f), CatPink);
                p.Line(V40E_Ink, 0.035f, -0.12f, 1.66f, -0.3f, 1.9f);
                p.Line(V40E_Ink, 0.035f, 0.12f, 1.66f, 0.3f, 1.9f);
            }
            Sdf body = Sd.Box(0, 0.5f, 1.1f, 0.48f, 0.08f);
            Body(p, body, box, boxShade);
            p.Line(GoldShade, 0.05f, -1.05f, 0.85f, 1.05f, 0.85f);
            p.Line(GoldShade, 0.05f, -1.05f, 0.15f, 1.05f, 0.15f);
            if (!open)
            {
                Sdf lid = Sd.Box(0, 1.12f, 1.16f, 0.16f, 0.07f);
                Body(p, lid, box, boxShade);
                p.Shape(Sd.Box(0, 1.3f, 0.2f, 0.05f, 0.03f), Gold, 0.025f);
            }
            // serratura
            p.Shape(Sd.Circle(0, 0.45f, 0.2f), Gold);
            Sdf hole = Sd.Union(Sd.Circle(0, 0.5f, 0.06f), Sd.Polygon(-0.04f, 0.48f, 0.04f, 0.48f, 0.06f, 0.33f, -0.06f, 0.33f));
            p.Fill(hole, V40E_Ink);
            return new DoodleArt(open ? "e40_music_box_open" : "e40_music_box", p);
        }

        // ---------- Knight's quest ----------

        /// <summary>Sala del castello con stendardi (coordinate del livello).</summary>
        public static DoodleArt V40E_HallBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 5f, 7.5f), (x, y) => Rgba.Lerp(Rgba.Hex("3F3A52"), Rgba.Hex("5D5677"), Clamp((y + 4f) / 10f)));
            for (int row = 0; row < 30; row++)
            {
                float y0 = -7f + row * 0.5f, shift = row % 2 == 0 ? 0f : 0.45f;
                for (float x = -4.6f + shift; x < 4.6f; x += 0.9f)
                    p.Shape(Sd.Box(x + 0.45f, y0 + 0.25f, 0.41f, 0.21f, 0.05f), Rgba.Lerp(Rgba.Hex("625B7E"), Rgba.Hex("6E6790"), Noise.Value(x * 2.1f, y0 * 1.7f)).WithAlpha(0.7f), 0.015f);
            }
            foreach (float x in new[] { -2.3f, 2.3f })
            {
                Sdf banner = Sd.Polygon(x - 0.35f, 3.6f, x + 0.35f, 3.6f, x + 0.35f, 2.3f, x, 2.55f, x - 0.35f, 2.3f);
                Body(p, banner, Rgba.Hex("C23A48"), Rgba.Hex("8E2532"));
                p.Shape(Sd.Circle(x, 3.1f, 0.14f), Gold, 0.03f);
            }
            p.Fill(Sd.Box(0, -5.5f, 5f, 1.6f), Rgba.Hex("3A2E3E"), false);
            return new DoodleArt("e40_hall_bg", p);
        }

        /// <summary>Scacchiera 5 x 5 con celle da 0.95 (centro 0,0) e cornice.</summary>
        public static DoodleArt V40E_Chessboard()
        {
            var p = new DoodlePainter(-2.68f, -2.68f, 2.68f, 2.68f, 60) { Ink = 0.06f };
            Body(p, Sd.Box(0, 0, 2.6f, 2.6f, 0.12f), Wood, WoodShade);
            for (int c = 0; c < 5; c++)
                for (int r = 0; r < 5; r++)
                {
                    bool dark = (c + r) % 2 == 0;
                    p.Fill(Sd.Box((c - 2) * 0.95f, (r - 2) * 0.95f, 0.475f, 0.475f), dark ? Rgba.Hex("A86B4A") : Rgba.Hex("F4DFC0"), false);
                }
            p.Shape(Sd.Box(0, 0, 2.375f, 2.375f), Rgba.Clear, 0.04f);
            return new DoodleArt("e40_chessboard", p);
        }

        static void V40E_PieceBase(DoodlePainter p, Rgba fill, Rgba shade)
        {
            Body(p, Sd.Box(0, -0.33f, 0.28f, 0.06f, 0.04f), fill, shade);
            Body(p, Sd.Polygon(-0.2f, -0.27f, 0.2f, -0.27f, 0.14f, -0.12f, -0.14f, -0.12f), fill, shade);
        }

        /// <summary>Cavallo degli scacchi cavalcato da un gatto (centro della cella 0,0).</summary>
        public static DoodleArt V40E_Knight()
        {
            var p = new DoodlePainter(-0.42f, -0.42f, 0.42f, 0.5f, 160) { Ink = 0.035f };
            Rgba horse = Rgba.Hex("F4F0F6"), hShade = Rgba.Hex("CFC6DA");
            V40E_PieceBase(p, horse, hShade);
            Sdf head = Sd.Polygon(-0.16f, -0.13f, 0.14f, -0.13f, 0.12f, 0.08f, 0.3f, 0.02f, 0.33f, 0.12f, 0.12f, 0.3f, -0.02f, 0.32f, -0.16f, 0.12f);
            Body(p, head, horse, hShade);
            p.Line(Rgba.Hex("6E4325"), 0.06f, -0.16f, 0.0f, -0.12f, 0.2f, -0.02f, 0.3f);
            p.Fill(Sd.Circle(0.14f, 0.16f, 0.025f), V40E_Ink);
            // gatto in sella
            Body(p, Sd.Ellipse(-0.08f, 0.32f, 0.11f, 0.08f), CatOrange, CatShade);
            p.Shape(Sd.Union(Sd.Polygon(-0.18f, 0.42f, -0.17f, 0.52f, -0.1f, 0.46f), Sd.Polygon(-0.02f, 0.42f, -0.03f, 0.52f, -0.08f, 0.46f)), CatOrange, 0.02f);
            Body(p, Sd.Circle(-0.1f, 0.38f, 0.09f), CatOrange, CatShade);
            p.Fill(Sd.Circle(-0.13f, 0.39f, 0.015f), V40E_Ink);
            p.Fill(Sd.Circle(-0.07f, 0.39f, 0.015f), V40E_Ink);
            p.Shape(Sd.Polygon(0.0f, 0.38f, 0.14f, 0.43f, 0.0f, 0.44f), Rgba.Hex("3E8EF7"), 0.015f);
            return new DoodleArt("e40_knight", p);
        }

        /// <summary>Topolino-pedone grigio (centro della cella 0,0).</summary>
        public static DoodleArt V40E_MousePawn()
        {
            var p = new DoodlePainter(-0.38f, -0.42f, 0.38f, 0.38f, 160) { Ink = 0.035f };
            Rgba fur = Rgba.Hex("B7B4C4"), shade = Rgba.Hex("8E8AA0");
            V40E_PieceBase(p, Rgba.Hex("6B6677"), Rgba.Hex("4D4858"));
            p.Line(Rgba.Hex("FF9BB0"), 0.025f, 0.12f, -0.2f, 0.28f, -0.15f, 0.3f, 0.0f);
            Body(p, Sd.Ellipse(0, -0.02f, 0.14f, 0.13f), fur, shade);
            foreach (float sx in new[] { -1f, 1f })
            {
                p.Shape(Sd.Circle(sx * 0.13f, 0.26f, 0.08f), fur);
                p.Fill(Sd.Circle(sx * 0.13f, 0.26f, 0.045f), Rgba.Hex("FFB6C8"));
            }
            Body(p, Sd.Circle(0, 0.15f, 0.12f), fur, shade);
            p.Fill(Sd.Circle(-0.045f, 0.17f, 0.018f), V40E_Ink);
            p.Fill(Sd.Circle(0.045f, 0.17f, 0.018f), V40E_Ink);
            p.Fill(Sd.Circle(0, 0.11f, 0.022f), Rgba.Hex("FF7F9C"));
            return new DoodleArt("e40_mouse_pawn", p);
        }

        /// <summary>Gatto-re con corona che fa il pisolino (centro della cella 0,0).</summary>
        public static DoodleArt V40E_CatKing()
        {
            var p = new DoodlePainter(-0.42f, -0.42f, 0.42f, 0.5f, 160) { Ink = 0.035f };
            V40E_PieceBase(p, Rgba.Hex("7A5A9A"), Rgba.Hex("5A3E78"));
            Body(p, Sd.Ellipse(0, -0.02f, 0.2f, 0.14f), Rgba.Hex("7A5A9A"), Rgba.Hex("5A3E78"));
            p.Shape(Sd.Union(Sd.Polygon(-0.2f, 0.12f, -0.2f, 0.32f, -0.07f, 0.2f), Sd.Polygon(0.2f, 0.12f, 0.2f, 0.32f, 0.07f, 0.2f)), Rgba.Hex("9A9AA8"));
            Body(p, Sd.Circle(0, 0.13f, 0.17f), Rgba.Hex("9A9AA8"), Rgba.Hex("6E6E80"));
            p.InkLine(0.015f, -0.1f, 0.14f, -0.04f, 0.12f);
            p.InkLine(0.015f, 0.04f, 0.12f, 0.1f, 0.14f);
            p.Fill(Sd.Polygon(-0.02f, 0.08f, 0.02f, 0.08f, 0f, 0.06f), CatPink);
            Sdf crown = Sd.Polygon(-0.13f, 0.27f, 0.13f, 0.27f, 0.15f, 0.43f, 0.07f, 0.34f, 0f, 0.45f, -0.07f, 0.34f, -0.15f, 0.43f);
            Body(p, crown, Gold, GoldShade);
            p.Fill(Sd.Circle(0, 0.32f, 0.025f), Rgba.Hex("E8433F"));
            return new DoodleArt("e40_cat_king", p);
        }

        /// <summary>Torre degli scacchi color crema (centro della cella 0,0).</summary>
        public static DoodleArt V40E_Rook()
        {
            var p = new DoodlePainter(-0.38f, -0.42f, 0.38f, 0.42f, 160) { Ink = 0.035f };
            Rgba fill = Rgba.Hex("F4F0F6"), shade = Rgba.Hex("CFC6DA");
            V40E_PieceBase(p, fill, shade);
            Body(p, Sd.Box(0, 0.05f, 0.15f, 0.2f, 0.02f), fill, shade);
            Sdf top = Sd.Subtract(Sd.Box(0, 0.3f, 0.22f, 0.08f, 0.02f), Sd.Union(Sd.Box(-0.08f, 0.37f, 0.035f, 0.05f), Sd.Box(0.08f, 0.37f, 0.035f, 0.05f)));
            Body(p, top, fill, shade);
            p.Shape(Sd.Box(0, 0.05f, 0.04f, 0.07f, 0.03f), Rgba.Hex("3B3A46"), 0.02f);
            return new DoodleArt("e40_rook", p);
        }

        /// <summary>Gomitolo di lana del re (centro della cella 0,0).</summary>
        public static DoodleArt V40E_Yarn()
        {
            var p = new DoodlePainter(-0.36f, -0.36f, 0.36f, 0.3f, 160) { Ink = 0.035f };
            Sdf ball = Sd.Circle(0, -0.04f, 0.26f);
            Body(p, ball, Rgba.Hex("FF7F9C"), Rgba.Hex("D9567A"));
            foreach (float a in new[] { -40f, 0f, 40f })
                p.Fill(Sd.Intersect(ball, Sd.Rotate(Sd.Subtract(Sd.Ellipse(0, -0.04f, 0.3f, 0.12f), Sd.Ellipse(0, -0.04f, 0.27f, 0.09f)), 0, -0.04f, a)), Rgba.Hex("B83D60"));
            p.Line(Rgba.Hex("D9567A"), 0.025f, 0.18f, -0.25f, 0.3f, -0.32f, 0.34f, -0.25f);
            return new DoodleArt("e40_yarn", p);
        }

        /// <summary>Pallino della mossa possibile (si tinge; raggio 0.16).</summary>
        public static DoodleArt V40E_MoveDot()
        {
            var p = new DoodlePainter(-0.2f, -0.2f, 0.2f, 0.2f, 200) { Wobble = 0f };
            p.Fill(Sd.Circle(0, 0, 0.17f), Rgba.White.WithAlpha(0.5f));
            p.Fill(Sd.Circle(0, 0, 0.11f), Rgba.White);
            return new DoodleArt("e40_move_dot", p);
        }

        /// <summary>Anello di selezione della cella (si tinge; cella 0.95).</summary>
        public static DoodleArt V40E_SelectRing()
        {
            var p = new DoodlePainter(-0.48f, -0.48f, 0.48f, 0.48f, 140) { Wobble = 0f };
            p.Fill(Sd.Subtract(Sd.Box(0, 0, 0.45f, 0.45f, 0.08f), Sd.Box(0, 0, 0.37f, 0.37f, 0.05f)), Rgba.White);
            return new DoodleArt("e40_select_ring", p);
        }

        /// <summary>Pulsante tondo "annulla" con la freccia curva (raggio 0.38).</summary>
        public static DoodleArt V40E_UndoButton()
        {
            var p = new DoodlePainter(-0.44f, -0.44f, 0.44f, 0.44f, 160) { Ink = 0.045f };
            Body(p, Sd.Circle(0, 0, 0.38f), Rgba.Hex("FFE680"), Rgba.Hex("E8C93A"));
            Sdf arc = Sd.Intersect(Sd.Subtract(Sd.Circle(0.02f, -0.03f, 0.2f), Sd.Circle(0.02f, -0.03f, 0.12f)), Sd.Box(0.05f, -0.05f, 0.25f, 0.25f));
            p.Fill(Sd.Subtract(arc, Sd.Box(-0.15f, -0.15f, 0.12f, 0.12f)), V40E_Ink);
            p.Fill(Sd.Polygon(-0.26f, 0.0f, -0.06f, 0.0f, -0.16f, -0.16f), V40E_Ink);
            return new DoodleArt("e40_undo", p);
        }

        // ---------- The gauntlet ----------

        /// <summary>Palco con sipario e riflettore (coordinate del livello).</summary>
        public static DoodleArt V40E_ArenaBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 5f, 7.5f), (x, y) => Rgba.Lerp(Rgba.Hex("2A1E3A"), Rgba.Hex("44305C"), Clamp((y + 3f) / 9f)));
            p.FillFunc(Sd.Polygon(-0.6f, 7f, 0.6f, 7f, 2.8f, -3.6f, -2.8f, -3.6f), (x, y) => Rgba.Hex("FFF1B0").WithAlpha(0.08f + 0.06f * Clamp((y + 3.6f) / 10f)));
            foreach (float sx in new[] { -1f, 1f })
            {
                Sdf curtain = Sd.Polygon(sx * 4.6f, 7f, sx * 2.5f, 7f, sx * 2.65f, 2f, sx * 2.9f, -3.6f, sx * 4.6f, -3.6f);
                p.Shape(curtain, Rgba.Hex("B8283A"));
                for (float k = 0.25f; k < 1.8f; k += 0.4f) p.Line(Rgba.Hex("8E1E2E"), 0.06f, sx * (2.7f + k), 7f, sx * (2.75f + k), -3.6f);
            }
            p.Shape(Sd.Box(0, 6.6f, 5f, 0.6f), Rgba.Hex("B8283A"));
            p.Fill(Sd.Box(0, -5.4f, 5f, 1.8f), Rgba.Hex("7A4A2E"), false);
            p.Shape(Sd.Box(0, -3.6f, 5f, 0.08f), Rgba.Hex("A8744C"));
            for (float x = -4.2f; x < 4.5f; x += 0.9f) p.Line(V40E_Ink.WithAlpha(0.3f), 0.03f, x, -3.7f, x - 0.4f, -7f);
            return new DoodleArt("e40_arena_bg", p);
        }

        /// <summary>Lucchetto grande (corpo 1.5 x 1.3 centrato in 0,0; buco della serratura in 0,-0.1).</summary>
        public static DoodleArt V40E_Padlock()
        {
            var p = new DoodlePainter(-0.85f, -0.78f, 0.85f, 1.45f, 110) { Ink = 0.055f };
            Sdf shackle = Sd.Subtract(Sd.Union(Sd.Circle(0, 0.75f, 0.55f), Sd.Box(0, 0.45f, 0.55f, 0.3f)), Sd.Union(Sd.Circle(0, 0.75f, 0.38f), Sd.Box(0, 0.45f, 0.38f, 0.32f)));
            Body(p, Sd.Intersect(shackle, Sd.Box(0, 0.9f, 0.6f, 0.6f)), Rgba.Hex("C9CED8"), Rgba.Hex("8E95A4"));
            Sdf body = Sd.Box(0, 0, 0.75f, 0.65f, 0.14f);
            Body(p, body, Gold, GoldShade);
            p.Shape(Sd.Circle(0, -0.1f, 0.26f), Rgba.Hex("FFF3B0"), 0.04f);
            Sdf hole = Sd.Union(Sd.Circle(0, -0.04f, 0.09f), Sd.Polygon(-0.06f, -0.07f, 0.06f, -0.07f, 0.09f, -0.28f, -0.09f, -0.28f));
            p.Fill(hole, V40E_Ink);
            return new DoodleArt("e40_padlock", p);
        }

        /// <summary>
        /// Pareti del mini-labirinto della sfida (rettangoli cx, cy, hx, hy in coordinate del vassoio): le usano sia il
        /// disegno sia i collider.
        /// </summary>
        public static readonly float[] V40E_MazeWalls =
        {
            0f, 1.9f, 2.3f, 0.08f,     0f, -1.9f, 2.3f, 0.08f,     -2.3f, 0f, 0.08f, 1.98f,     2.3f, 0f, 0.08f, 1.98f,
            -0.75f, 0.95f, 1.55f, 0.07f,     0.75f, -0.15f, 1.55f, 0.07f,     -0.75f, -1.05f, 1.55f, 0.07f,
        };
        /// <summary>Buche del labirinto (cx, cy): l'ultima è il traguardo.</summary>
        public static readonly float[] V40E_MazeHoles = { 0.6f, 0.42f, -0.9f, -0.6f, -0.3f, -1.66f, -1.85f, -1.45f };

        /// <summary>Vassoio di legno del mini-labirinto (4.8 x 4, centro 0,0).</summary>
        public static DoodleArt V40E_MazeTray()
        {
            var p = new DoodlePainter(-2.5f, -2.1f, 2.5f, 2.1f, 70) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 2.38f, 1.98f, 0.1f), (x, y) => Rgba.Lerp(Rgba.Hex("E6C08A"), Rgba.Hex("F2D29C"), Noise.Value(x * 1.3f, y * 9f)));
            for (int i = 0; i + 3 < V40E_MazeWalls.Length; i += 4)
            {
                Sdf w = Sd.Box(V40E_MazeWalls[i], V40E_MazeWalls[i + 1], V40E_MazeWalls[i + 2], V40E_MazeWalls[i + 3], 0.05f);
                Body(p, w, Wood, WoodShade);
            }
            // freccia di partenza
            p.Line(V40E_Ink.WithAlpha(0.4f), 0.04f, -1.9f, 1.42f, -1.5f, 1.42f);
            p.Fill(Sd.Polygon(-1.5f, 1.52f, -1.5f, 1.32f, -1.38f, 1.42f), V40E_Ink.WithAlpha(0.4f));
            return new DoodleArt("e40_maze_tray", p);
        }

        /// <summary>Buca del vassoio (raggio 0.24, centro 0,0).</summary>
        public static DoodleArt V40E_MazeHole()
        {
            var p = new DoodlePainter(-0.28f, -0.28f, 0.28f, 0.28f, 200) { Ink = 0.035f };
            p.FillFunc(Sd.Circle(0, 0, 0.24f), (x, y) => Rgba.Lerp(Rgba.Hex("120D16"), Rgba.Hex("3A2E3E"), Clamp((y + 0.24f) / 0.48f)));
            p.Shape(Sd.Circle(0, 0, 0.24f), Rgba.Clear);
            return new DoodleArt("e40_maze_hole", p);
        }

        /// <summary>Biglia di vetro azzurra (raggio 0.17).</summary>
        public static DoodleArt V40E_Marble()
        {
            var p = new DoodlePainter(-0.2f, -0.2f, 0.2f, 0.2f, 300) { Ink = 0.025f, Wobble = 0.003f };
            Sdf m = Sd.Circle(0, 0, 0.17f);
            p.FillFunc(m, (x, y) => Rgba.Lerp(Rgba.Hex("2563C9"), Rgba.Hex("7FD8FF"), Clamp((x * -0.5f + y + 0.17f) / 0.34f)));
            p.Fill(Sd.Rotate(Sd.Ellipse(0.02f, 0, 0.05f, 0.13f), 0.02f, 0, 30f), Rgba.Hex("FFE680").WithAlpha(0.8f));
            p.Fill(Sd.Circle(-0.06f, 0.07f, 0.035f), Rgba.White.WithAlpha(0.9f));
            p.Shape(m, Rgba.Clear);
            return new DoodleArt("e40_marble", p);
        }
    }
}
