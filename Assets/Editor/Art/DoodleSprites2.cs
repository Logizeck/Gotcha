using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli 16-20 (labirinti con trappole e al buio, cassaforte, nanna, razzo).</summary>
    public static partial class DoodleSprites
    {
        // ---------- Labirinti a segmenti (livelli 16 e 17) ----------

        /// <summary>Tratto di parete al neon da (x1,y1) a (x2,y2), in coordinate locali del labirinto.</summary>
        public struct MazeSeg
        {
            public float x1, y1, x2, y2; public Rgba color;
            public MazeSeg(float x1, float y1, float x2, float y2, Rgba color) { this.x1 = x1; this.y1 = y1; this.x2 = x2; this.y2 = y2; this.color = color; }
        }

        /// <summary>Labirinto con trappole: serpentina su 4 corridoi (si entra in basso a sinistra, la stella è in alto a sinistra).</summary>
        public static readonly MazeSeg[] TrapMaze =
        {
            new MazeSeg(-2.75f, -3.5f, 2.75f, -3.5f, NeonCyan),
            new MazeSeg(-2.75f, 3.5f, 2.75f, 3.5f, NeonCyan),
            new MazeSeg(-2.75f, -3.5f, -2.75f, 3.5f, NeonCyan),
            new MazeSeg(2.75f, -3.5f, 2.75f, 3.5f, NeonCyan),
            new MazeSeg(-2.75f, -1.75f, 1.65f, -1.75f, NeonMagenta),
            new MazeSeg(-1.65f, 0f, 2.75f, 0f, NeonYellow),
            new MazeSeg(-2.75f, 1.75f, 1.65f, 1.75f, NeonMagenta),
        };
        /// <summary>Buche del labirinto con trappole (centro; raggio TrapPitRadius).</summary>
        public static readonly float[] TrapPits = { -0.9f, -2.35f, 0.5f, -2.95f, 0.9f, -0.6f, -0.5f, -1.15f, -1.0f, 0.9f, 1.3f, 0.5f, 0.8f, 2.9f, -1.7f, 2.2f };
        public const float TrapPitRadius = 0.3f;
        /// <summary>Laser verticali: x, y1, y2.</summary>
        public static readonly float[] TrapLasers = { 0.2f, 0.08f, 1.67f, -0.9f, 1.83f, 3.42f };

        /// <summary>Labirinto al buio (griglia 5x7, generato una volta e fissato qui): la chiave è in un vicolo cieco.</summary>
        public static readonly MazeSeg[] DarkMaze =
        {
            new MazeSeg(-2.75f, -3.50f, 2.75f, -3.50f, NeonCyan),
            new MazeSeg(-2.75f, -2.50f, -1.65f, -2.50f, NeonYellow),
            new MazeSeg(0.55f, -2.50f, 1.65f, -2.50f, NeonMagenta),
            new MazeSeg(-1.65f, -1.50f, 1.65f, -1.50f, NeonYellow),
            new MazeSeg(-2.75f, -0.50f, -0.55f, -0.50f, NeonMagenta),
            new MazeSeg(1.65f, -0.50f, 2.75f, -0.50f, NeonYellow),
            new MazeSeg(0.55f, 0.50f, 1.65f, 0.50f, NeonMagenta),
            new MazeSeg(-1.65f, 1.50f, 1.65f, 1.50f, NeonYellow),
            new MazeSeg(-1.65f, 2.50f, 2.75f, 2.50f, NeonMagenta),
            new MazeSeg(-2.75f, 3.50f, 2.75f, 3.50f, NeonCyan),
            new MazeSeg(-2.75f, -3.50f, -2.75f, 3.50f, NeonCyan),
            new MazeSeg(-1.65f, 0.50f, -1.65f, 1.50f, NeonYellow),
            new MazeSeg(-0.55f, -3.50f, -0.55f, -1.50f, NeonMagenta),
            new MazeSeg(-0.55f, -0.50f, -0.55f, 1.50f, NeonYellow),
            new MazeSeg(0.55f, -1.50f, 0.55f, 0.50f, NeonMagenta),
            new MazeSeg(1.65f, -2.50f, 1.65f, -1.50f, NeonYellow),
            new MazeSeg(2.75f, -3.50f, 2.75f, 3.50f, NeonCyan),
        };
        /// <summary>Cancello rosso che chiude la cella d'uscita finché non si prende la chiave.</summary>
        public static readonly MazeSeg DarkGate = new MazeSeg(1.65f, 2.5f, 1.65f, 3.5f, Rgba.Hex("FF3B4E"));
        public const float DarkKeyX = 1.1f, DarkKeyY = -2.0f, DarkExitX = 2.2f, DarkExitY = 3.0f;

        static float SegDistance(MazeSeg s, float x, float y)
        {
            float dx = s.x2 - s.x1, dy = s.y2 - s.y1;
            float len2 = dx * dx + dy * dy;
            float t = len2 > 0f ? Math.Max(0f, Math.Min(1f, ((x - s.x1) * dx + (y - s.y1) * dy) / len2)) : 0f;
            float px = s.x1 + dx * t - x, py = s.y1 + dy * t - y;
            return (float)Math.Sqrt(px * px + py * py);
        }

        /// <summary>
        /// Distanza dal tubo più vicino e colore "mescolato": vicino a una giunzione tra tubi di colori diversi i colori
        /// sfumano l'uno nell'altro (peso esponenziale sulla differenza di distanza) invece di tagliarsi di netto.
        /// </summary>
        static Rgba BlendedTube(IList<MazeSeg> segs, float x, float y, out float dmin)
        {
            const float blend = 0.2f;   // unità: lunghezza della sfumatura attorno alla giunzione
            dmin = float.MaxValue;
            int n = segs.Count;
            var d = new float[n];
            for (int i = 0; i < n; i++) { d[i] = SegDistance(segs[i], x, y); if (d[i] < dmin) dmin = d[i]; }
            float r = 0f, g = 0f, b = 0f, wsum = 0f;
            for (int i = 0; i < n; i++)
            {
                float w = (float)Math.Exp(-(d[i] - dmin) / blend * 1.6f);
                if (w < 0.002f) continue;
                Rgba c = segs[i].color;
                r += c.r * w; g += c.g * w; b += c.b * w; wsum += w;
            }
            return new Rgba(r / wsum, g / wsum, b / wsum, 1f);
        }

        /// <summary>
        /// Colore arcobaleno delle pareti dei labirinti: la tinta scorre in diagonale sul labirinto (dal basso a sinistra
        /// all'alto a destra fa un giro completo dello spettro), così ogni corridoio ha un colore diverso e sfumato.
        /// </summary>
        public static Rgba Rainbow(float x, float y)
        {
            float h = (x * 0.55f + y * 0.8f) / 7.2f + 0.5f;
            h -= (float)Math.Floor(h);
            float k(float n) { float t = (n + h * 6f) % 6f; return 1f - Math.Max(0f, Math.Min(Math.Min(t, 4f - t), 1f)); }
            const float sat = 0.82f;   // pastello acceso: resta "neon" senza bruciare
            return new Rgba(1f - sat * (1f - k(5f)), 1f - sat * (1f - k(3f)), 1f - sat * (1f - k(1f)), 1f);
        }

        /// <summary>Stessi tubi al neon del livello 2 (nucleo chiaro, corpo colorato, alone) per una lista di segmenti.</summary>
        static void NeonTubes(DoodlePainter p, Sdf area, IList<MazeSeg> segs, bool rainbow = false)
        {
            const float glowReach = 0.17f;
            // alone: un solo strato, col colore mescolato (alle giunzioni non si sommano due aloni di colori diversi)
            p.FillFunc(area, (x, y) =>
            {
                Rgba c = BlendedTube(segs, x, y, out float dmin);
                if (rainbow) c = Rainbow(x, y);
                float d = dmin - MazeTubeRadius;
                return d > glowReach ? Rgba.Clear : c.WithAlpha((float)Math.Exp(-Math.Max(0f, d) * 20f) * 0.6f);
            });
            p.FillFunc((x, y) =>
            {
                float dmin = float.MaxValue;
                foreach (MazeSeg sg in segs) dmin = Math.Min(dmin, SegDistance(sg, x, y));
                return dmin - MazeTubeRadius;
            }, (x, y) =>
            {
                Rgba c = BlendedTube(segs, x, y, out float dmin);
                if (rainbow) c = Rainbow(x, y);
                float t = Clamp(1f - dmin / MazeTubeRadius);
                Rgba core = Rgba.Lerp(Rgba.Lerp(c, Rgba.White, 0.15f), Rgba.White, Clamp((t - 0.45f) / 0.45f));
                return Rgba.Lerp(Rgba.Lerp(c, Ink, 0.15f), core, Clamp(t * 4f));
            });
        }

        static DoodleArt NeonMaze(string name, IList<MazeSeg> segs)
        {
            var p = new DoodlePainter(-3.0f, -3.75f, 3.0f, 3.75f, 150) { Wobble = 0f };
            NeonTubes(p, Sd.Box(0, 0, 3.05f, 3.8f), segs, true);   // pareti arcobaleno
            return new DoodleArt(name, p);
        }

        public static DoodleArt TrapMazeNeon() { return NeonMaze("maze_trap", CircleTrapMaze()); }   // livello 16: cerchi concentrici
        public static DoodleArt DarkMazeNeon() { return NeonMaze("maze_dark", Warped(DarkMaze, DarkWarp)); }

        /// <summary>Cancello al neon rosso (orizzontale, lungo 1, centro 0,0: si ruota e si scala nel builder).</summary>
        public static DoodleArt GateBar()
        {
            var p = new DoodlePainter(-0.75f, -0.3f, 0.75f, 0.3f, 150) { Wobble = 0f };
            NeonTubes(p, Sd.Box(0, 0, 0.8f, 0.35f), new[] { new MazeSeg(-0.5f, 0f, 0.5f, 0f, DarkGate.color) });
            p.Shape(Sd.Box(0, -0.02f, 0.11f, 0.09f, 0.03f), Gold, 0.025f);   // lucchetto
            p.Shape(Sd.Subtract(Sd.Circle(0, 0.08f, 0.08f), Sd.Circle(0, 0.08f, 0.045f)), Rgba.Hex("B7BCC9"), 0.02f);
            p.Fill(Sd.Circle(0, -0.02f, 0.025f), Ink);
            return new DoodleArt("gate_bar", p);
        }

        /// <summary>Buca nel pavimento del labirinto (raggio 0.3) con bordo a strisce di pericolo.</summary>
        public static DoodleArt Pit()
        {
            var p = new DoodlePainter(-0.42f, -0.42f, 0.42f, 0.42f, 160) { Ink = 0.03f, Wobble = 0f };
            Sdf rim = Sd.Circle(0, 0, 0.36f);
            p.FillFunc(rim, (x, y) =>
            {
                double a = Math.Atan2(y, x) * 180.0 / Math.PI;
                return ((int)Math.Floor((a + 360.0) / 30.0)) % 2 == 0 ? Rgba.Hex("FFD23A") : Ink;
            });
            p.FillFunc(Sd.Circle(0, 0, 0.29f), (x, y) =>
            {
                float d = (float)Math.Sqrt(x * x + (y - 0.05f) * (y - 0.05f)) / 0.29f;
                return Rgba.Lerp(Rgba.Hex("000000"), Rgba.Hex("2A1E3A"), Clamp(d * d));
            });
            p.Shape(rim, Rgba.Clear);
            return new DoodleArt("pit", p);
        }

        /// <summary>Raggio laser verticale lungo 1 (da -0.5 a 0.5), rosso con nucleo bianco: si scala in altezza.</summary>
        public static DoodleArt LaserBeam()
        {
            var p = new DoodlePainter(-0.2f, -0.5f, 0.2f, 0.5f, 120) { Wobble = 0f };
            p.FillFunc(Sd.Box(0, 0, 0.2f, 0.5f), (x, y) =>
            {
                float d = Math.Abs(x);
                Rgba red = Rgba.Hex("FF2A3D");
                if (d < 0.03f) return Rgba.Lerp(Rgba.White, red, d / 0.03f);
                return red.WithAlpha((float)Math.Exp(-(d - 0.03f) * 22f) * 0.85f);
            });
            return new DoodleArt("laser", p);
        }

        /// <summary>Emettitore del laser, appoggiato alla parete (la lente guarda in basso, verso -y).</summary>
        public static DoodleArt LaserNode()
        {
            var p = new DoodlePainter(-0.2f, -0.16f, 0.2f, 0.16f, 160) { Ink = 0.03f };
            Body(p, Sd.Box(0, 0.02f, 0.15f, 0.1f, 0.04f), Rgba.Hex("4A4E5C"), Rgba.Hex("2E313B"));
            p.Shape(Sd.Circle(0, -0.07f, 0.055f), Rgba.Hex("FF4D5E"), 0.02f);
            return new DoodleArt("laser_node", p);
        }

        /// <summary>Buio con un foro di luce morbido in (0,0) (raggio ~1.4): segue la sfera.</summary>
        public static DoodleArt Darkness()
        {
            var p = new DoodlePainter(-9f, -9f, 9f, 9f, 20) { Wobble = 0f };
            Rgba night = Rgba.Hex("03040A");
            p.FillFunc(Sd.Box(0, 0, 9.2f, 9.2f), (x, y) =>
            {
                float d = (float)Math.Sqrt(x * x + y * y);
                float k = Clamp((d - 0.95f) / 0.95f);
                return night.WithAlpha(k * k * (3f - 2f * k));
            });
            return new DoodleArt("darkness", p);
        }

        // ---------- Vespa furiosa (livello 12) ----------

        /// <summary>Vespa fuori di sé: sopracciglio a V spesso, occhio iniettato, denti stretti, guance rosse.</summary>
        public static DoodleArt WaspFurious()
        {
            var p = new DoodlePainter(-0.62f, -0.45f, 0.6f, 0.45f, 160) { Ink = 0.035f };
            WaspBody(p, false);
            p.Fill(Sd.Ellipse(-0.28f, -0.03f, 0.07f, 0.045f), Rgba.Hex("FF5A4E", 0.55f));    // guancia rossa
            p.Shape(Sd.Ellipse(-0.37f, 0.05f, 0.065f, 0.065f), Rgba.Hex("FFE1DC"), 0.02f);
            p.Line(Rgba.Hex("E23B3B", 0.8f), 0.008f, -0.42f, 0.07f, -0.39f, 0.05f);           // venuzze
            p.Line(Rgba.Hex("E23B3B", 0.8f), 0.008f, -0.33f, 0.09f, -0.36f, 0.06f);
            p.Fill(Sd.Circle(-0.39f, 0.04f, 0.02f), Ink);
            p.Fill(Sd.Polygon(-0.5f, 0.17f, -0.28f, 0.09f, -0.29f, 0.14f, -0.47f, 0.21f), Ink);  // sopracciglio a V
            Sdf mouth = Sd.Box(-0.38f, -0.09f, 0.075f, 0.035f, 0.02f);
            p.Shape(mouth, Rgba.White, 0.018f);                                                 // denti stretti
            p.InkLine(0.01f, -0.45f, -0.09f, -0.31f, -0.09f);
            foreach (float x in new[] { -0.41f, -0.38f, -0.35f }) p.InkLine(0.008f, x, -0.12f, x, -0.06f);
            return new DoodleArt("wasp_furious", p);
        }

        // ---------- Barre LED (stereo) ----------

        /// <summary>Barretta LED bianca arrotondata (si tinge dal verde al rosso).</summary>
        public static DoodleArt Led()
        {
            var p = new DoodlePainter(-0.06f, -0.1f, 0.06f, 0.1f, 200) { Wobble = 0f, Ink = 0.012f };
            p.Shape(Sd.Box(0, 0, 0.045f, 0.085f, 0.02f), Rgba.White);
            p.Fill(Sd.Box(-0.015f, 0.035f, 0.01f, 0.035f, 0.01f), Rgba.White.WithAlpha(0.5f));
            return new DoodleArt("led", p);
        }

        // ---------- Livello 18: la cassaforte nascosta ----------

        /// <summary>Ritratto "Mona Cat" in cornice dorata (centro 0,0, 1.9 x 2.2).</summary>
        public static DoodleArt Portrait()
        {
            var p = new DoodlePainter(-1.0f, -1.15f, 1.0f, 1.4f, 110) { Ink = 0.05f };
            p.InkLine(0.025f, -0.55f, 1.0f, 0f, 1.35f, 0.55f, 1.0f);
            p.Shape(Sd.Circle(0, 1.35f, 0.045f), Rgba.Hex("B7BCC9"), 0.02f);
            Body(p, Sd.Box(0, 0, 0.95f, 1.1f, 0.06f), Gold, GoldShade);
            Sdf canvas = Sd.Box(0, 0, 0.76f, 0.9f);
            p.FillFunc(canvas, (x, y) => Rgba.Lerp(Rgba.Hex("7FB685"), Rgba.Hex("3E6B57"), Clamp((0.9f - y) / 1.8f)));
            p.Fill(Sd.Ellipse(0, -0.85f, 0.7f, 0.5f), Rgba.Hex("5A3E5D"));                        // spalle
            Sdf head = Sd.Circle(0, 0.05f, 0.42f);
            Sdf ears = Sd.Union(Sd.Polygon(-0.4f, 0.2f, -0.32f, 0.62f, -0.08f, 0.42f), Sd.Polygon(0.4f, 0.2f, 0.32f, 0.62f, 0.08f, 0.42f));
            p.Shape(ears, CatOrange, 0.03f);
            Body(p, head, CatOrange, CatShade, null, -0.06f, 0.06f);
            foreach (float s in new[] { -1f, 1f })
            {
                p.InkLine(0.03f, s * 0.22f, 0.12f, s * 0.12f, 0.1f);                          // occhi socchiusi e sornioni
                p.InkLine(0.015f, s * 0.12f, -0.12f, s * 0.4f, -0.08f);                       // baffi
                p.InkLine(0.015f, s * 0.12f, -0.16f, s * 0.4f, -0.2f);
            }
            p.Shape(Sd.Polygon(-0.05f, -0.02f, 0.05f, -0.02f, 0f, -0.08f), CatPink, 0.02f);
            p.InkLine(0.022f, -0.1f, -0.17f, 0f, -0.13f, 0.1f, -0.17f);                         // sorrisetto enigmatico
            p.Shape(canvas, Rgba.Clear, 0.035f);
            return new DoodleArt("portrait", p);
        }

        /// <summary>Retro del quadro caduto: cartone, fil di ferro e un post-it (il codice lo scrive il livello).</summary>
        public static DoodleArt PaintingBack()
        {
            var p = new DoodlePainter(-1.0f, -1.15f, 1.0f, 1.15f, 100) { Ink = 0.05f };
            Body(p, Sd.Box(0, 0, 0.95f, 1.1f, 0.06f), Rgba.Hex("C9A36A"), Rgba.Hex("A57E47"));
            Sdf board = Sd.Box(0, 0, 0.8f, 0.94f);
            p.Fill(board, Rgba.Hex("D9B98A"));
            p.Hatch(board, Rgba.Hex("C49E68", 0.6f), 0.12f, 0.02f, 90f);
            p.InkLine(0.02f, -0.6f, 0.75f, 0f, 0.55f, 0.6f, 0.75f);
            p.Shape(Sd.Rotate(Sd.Box(0, -0.15f, 0.5f, 0.38f, 0.03f), 0, -0.15f, -4f), Rgba.Hex("FFF27A"), 0.03f);   // post-it
            return new DoodleArt("painting_back", p);
        }

        /// <summary>Cassaforte a muro chiusa (centro 0,0): sportello con volantino, display in alto, spazio tastiera a destra.</summary>
        public static DoodleArt Safe()
        {
            var p = new DoodlePainter(-1.0f, -1.1f, 1.0f, 1.1f, 110) { Ink = 0.05f };
            Body(p, Sd.Box(0, 0, 0.95f, 1.05f, 0.08f), Rgba.Hex("8D93A6"), Rgba.Hex("666B7D"), Rgba.Hex("5A5F70", 0.4f));
            Sdf door = Sd.Box(0, -0.05f, 0.82f, 0.9f, 0.06f);
            Body(p, door, Rgba.Hex("B7BCC9"), Rgba.Hex("8D93A6"));
            p.Shape(Sd.Box(0, 0.68f, 0.5f, 0.14f, 0.04f), Rgba.Hex("103B2A"), 0.03f);     // display
            p.Shape(Sd.Circle(-0.42f, -0.2f, 0.26f), Rgba.Hex("D6DAE4"), 0.035f);          // volantino
            for (int i = 0; i < 3; i++)
            {
                double a = i * Math.PI * 2 / 3;
                p.InkLine(0.04f, -0.42f, -0.2f, -0.42f + (float)Math.Cos(a) * 0.36f, -0.2f + (float)Math.Sin(a) * 0.36f);
            }
            p.Shape(Sd.Circle(-0.42f, -0.2f, 0.07f), Rgba.Hex("666B7D"), 0.025f);
            foreach (float y in new[] { 0.55f, -0.65f }) p.Shape(Sd.Box(0.88f, y, 0.05f, 0.12f, 0.02f), Rgba.Hex("666B7D"), 0.02f);   // cerniere
            return new DoodleArt("safe", p);
        }

        /// <summary>Interno della cassaforte aperta (stesso ingombro) con lo sportello spalancato a destra.</summary>
        public static DoodleArt SafeOpen()
        {
            var p = new DoodlePainter(-1.0f, -1.1f, 1.45f, 1.1f, 110) { Ink = 0.05f };
            Body(p, Sd.Box(0, 0, 0.95f, 1.05f, 0.08f), Rgba.Hex("8D93A6"), Rgba.Hex("666B7D"));
            Sdf hole = Sd.Box(0, -0.05f, 0.8f, 0.88f, 0.05f);
            p.FillFunc(hole, (x, y) => Rgba.Lerp(Rgba.Hex("1E2029"), Rgba.Hex("3A3D49"), Clamp((y + 0.9f) / 1.8f)));
            p.Shape(Sd.Box(0, -0.35f, 0.8f, 0.04f), Rgba.Hex("6B6F7E"), 0.025f);         // ripiano
            p.Shape(hole, Rgba.Clear, 0.04f);
            Sdf door = Sd.Polygon(0.92f, 0.85f, 1.4f, 0.65f, 1.4f, -0.75f, 0.92f, -0.95f);  // sportello spalancato
            Body(p, door, Rgba.Hex("B7BCC9"), Rgba.Hex("8D93A6"));
            p.Shape(Sd.Ellipse(1.18f, -0.15f, 0.1f, 0.22f), Rgba.Hex("D6DAE4"), 0.03f);
            return new DoodleArt("safe_open", p);
        }

        /// <summary>Tasto della tastiera numerica (il numero è un testo sopra).</summary>
        public static DoodleArt KeypadKey()
        {
            var p = new DoodlePainter(-0.2f, -0.2f, 0.2f, 0.2f, 160) { Ink = 0.03f };
            Body(p, Sd.Box(0, 0, 0.17f, 0.17f, 0.05f), Rgba.Hex("EDEFF5"), Rgba.Hex("C3C8D6"));
            return new DoodleArt("keypad_key", p);
        }

        // ---------- Livello 19: a nanna ----------

        /// <summary>Abat-jour da comodino con interruttore a cordicella (base in 0,0; lampadina a 0,1.15).</summary>
        public static DoodleArt BedsideLamp()
        {
            var p = new DoodlePainter(-0.8f, -0.1f, 0.8f, 1.75f, 110) { Ink = 0.05f };
            Body(p, Sd.Ellipse(0, 0.08f, 0.42f, 0.1f), Rgba.Hex("7A5BD6"), Rgba.Hex("5A3FB5"));
            p.Shape(Sd.Box(0, 0.5f, 0.05f, 0.42f, 0.02f), Rgba.Hex("B7BCC9"), 0.03f);
            p.Shape(Sd.Circle(0, 1.12f, 0.15f), Rgba.Hex("FFF6C9"), 0.03f);
            Sdf shade = Sd.Polygon(-0.7f, 0.95f, 0.7f, 0.95f, 0.38f, 1.7f, -0.38f, 1.7f);
            Body(p, shade, Rgba.Hex("FFB4C7"), Rgba.Hex("F08AA6"), Rgba.Hex("E06C8C", 0.4f));
            p.InkLine(0.018f, 0.45f, 0.97f, 0.47f, 0.55f);
            p.Shape(Sd.Circle(0.47f, 0.5f, 0.05f), Gold, 0.02f);
            return new DoodleArt("bedside_lamp", p);
        }

        // ---------- Livello 20: il razzo ----------

        /// <summary>Cielo, colline e piattaforma di lancio (fondo intero).</summary>
        public static DoodleArt LaunchSite()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 5f, 7.5f), (x, y) => Rgba.Lerp(Rgba.Hex("FFD8A8"), Rgba.Hex("6FB7F2"), Clamp((y + 2f) / 7f)));
            foreach (float[] c in new[] { new[] { -2.2f, 3.4f, 0.9f }, new[] { 1.8f, 2.4f, 0.7f }, new[] { -0.4f, 1.3f, 0.5f } })
            {
                Sdf cloud = Sd.Union(Sd.Ellipse(c[0], c[1], c[2], c[2] * 0.4f), Sd.Circle(c[0] - c[2] * 0.3f, c[1] + c[2] * 0.2f, c[2] * 0.4f), Sd.Circle(c[0] + c[2] * 0.25f, c[1] + c[2] * 0.25f, c[2] * 0.33f));
                p.Shape(cloud, Rgba.White.WithAlpha(0.9f), 0.04f);
            }
            p.Shape(Sd.Union(Sd.Ellipse(-2.8f, -2.4f, 2.6f, 1.1f), Sd.Ellipse(2.6f, -2.5f, 2.6f, 1.2f)), Rgba.Hex("8FD27A"), 0.06f);
            Sdf ground = Sd.Box(0, -5.1f, 5f, 2.5f);
            p.Fill(ground, Leaf);
            p.Hatch(ground, LeafShade.WithAlpha(0.5f), 0.35f, 0.04f, 70f);
            p.Shape(Sd.Box(0, -2.7f, 5f, 0.1f), LeafShade, 0.05f);
            Body(p, Sd.Box(0, -2.85f, 1.5f, 0.22f, 0.06f), Rgba.Hex("B7BCC9"), Rgba.Hex("8D93A6"), Rgba.Hex("6B6F7E", 0.4f));   // piattaforma
            // torre di servizio
            p.Shape(Sd.Union(Sd.Box(-1.25f, -0.4f, 0.06f, 2.3f), Sd.Box(-0.75f, -0.4f, 0.06f, 2.3f)), Rgba.Hex("E23B3B"), 0.04f);
            for (int i = 0; i < 6; i++)
            {
                float y = -2.5f + i * 0.75f;
                p.Line(Rgba.Hex("B22A2A"), 0.05f, -1.25f, y, -0.75f, y + 0.75f);
                p.Line(Rgba.Hex("B22A2A"), 0.05f, -0.75f, y, -1.25f, y + 0.75f);
            }
            return new DoodleArt("launch_bg", p);
        }

        /// <summary>Razzo a fumetti (ugello in basso a y=0, punta a y=3).</summary>
        public static DoodleArt Rocket()
        {
            var p = new DoodlePainter(-0.95f, -0.1f, 0.95f, 3.1f, 110) { Ink = 0.055f };
            Sdf finL = Sd.Polygon(-0.42f, 1.1f, -0.85f, 0.25f, -0.85f, 0.05f, -0.42f, 0.4f);
            Sdf finR = Sd.Polygon(0.42f, 1.1f, 0.85f, 0.25f, 0.85f, 0.05f, 0.42f, 0.4f);
            Body(p, Sd.Union(finL, finR), Rgba.Hex("FF4D5E"), Rgba.Hex("D12F43"));
            p.Shape(Sd.Polygon(-0.3f, 0.32f, 0.3f, 0.32f, 0.38f, 0f, -0.38f, 0f), Rgba.Hex("666B7D"), 0.04f);   // ugello
            Sdf body = Sd.Union(Sd.Box(0, 1.3f, 0.45f, 1.0f, 0.2f), Sd.Ellipse(0, 2.2f, 0.45f, 0.85f));
            Body(p, body, Rgba.Hex("F4F5FA"), Rgba.Hex("C3C8D6"));
            p.Fill(Sd.Intersect(body, Sd.Box(0, 2.62f, 0.6f, 0.42f)), Rgba.Hex("FF4D5E"));                     // punta rossa
            p.Fill(Sd.Intersect(body, Sd.Box(0, 0.48f, 0.6f, 0.12f)), Rgba.Hex("FF4D5E"));
            p.Shape(body, Rgba.Clear);
            p.Shape(Sd.Circle(0, 1.75f, 0.24f), Rgba.Hex("B7BCC9"), 0.05f);                                       // oblò
            p.FillFunc(Sd.Circle(0, 1.75f, 0.17f), (x, y) => Rgba.Lerp(Rgba.Hex("BDEBFF"), Rgba.Hex("3E9BDB"), Clamp((1.9f - y) / 0.35f)));
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.06f, 1.82f, 0.06f, 0.03f), -0.06f, 1.82f, 30f), Rgba.White.WithAlpha(0.8f));
            p.Shape(Sd.Box(0, 1.0f, 0.06f, 0.32f, 0.03f), Rgba.Hex("FFC93C"), 0.025f);                            // bocchettone carburante
            return new DoodleArt("rocket", p);
        }

        /// <summary>Tanica del carburante rossa (centro 0,0).</summary>
        public static DoodleArt FuelCan()
        {
            var p = new DoodlePainter(-0.48f, -0.55f, 0.48f, 0.62f, 140) { Ink = 0.04f };
            Sdf can = Sd.Box(0, -0.08f, 0.38f, 0.44f, 0.08f);
            Body(p, can, Rgba.Hex("E23B3B"), Rgba.Hex("B22A2A"));
            p.Shape(Sd.Subtract(Sd.Box(-0.12f, 0.42f, 0.2f, 0.11f, 0.06f), Sd.Box(-0.12f, 0.42f, 0.11f, 0.04f, 0.03f)), Rgba.Hex("B22A2A"), 0.025f);   // maniglia
            p.Shape(Sd.Box(0.24f, 0.44f, 0.07f, 0.1f, 0.02f), Rgba.Hex("FFC93C"), 0.025f);
            p.InkLine(0.03f, -0.22f, 0.1f, 0.22f, -0.3f);
            p.InkLine(0.03f, -0.22f, -0.3f, 0.22f, 0.1f);
            return new DoodleArt("fuel_can", p);
        }

        /// <summary>Coperchio di sicurezza trasparente del pulsante di lancio, incernierato in alto (0,0 = cerniera).</summary>
        public static DoodleArt SafetyCover()
        {
            var p = new DoodlePainter(-0.8f, -0.75f, 0.8f, 0.1f, 120) { Ink = 0.04f };
            Sdf cover = Sd.Box(0, -0.36f, 0.72f, 0.33f, 0.08f);
            p.Fill(cover, Rgba.Hex("FFD23A", 0.55f));
            p.Hatch(cover, Rgba.Hex("2A2C36", 0.55f), 0.22f, 0.07f, 45f);
            p.Fill(Sd.Box(-0.4f, -0.2f, 0.18f, 0.04f, 0.03f), Rgba.White.WithAlpha(0.6f));
            p.Shape(cover, Rgba.Clear);
            p.Shape(Sd.Box(0, 0f, 0.3f, 0.05f, 0.03f), Rgba.Hex("8D93A6"), 0.025f);
            return new DoodleArt("safety_cover", p);
        }
    }
}
