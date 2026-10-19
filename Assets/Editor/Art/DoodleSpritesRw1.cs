using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli rifatti nella versione 0.40 (gruppo 1).</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtRw1() => new DoodleArt[]
        {
            TrapMazeRwNeon(), GlassShard("glass_shard_a", 0), GlassShard("glass_shard_b", 1), GlassShard("glass_shard_c", 2), GlassImpact(),
        };

        // ---------- Livello 16: labirinto circolare con trappole (rifatto) ----------

        /// <summary>
        /// Tre cerchi concentrici: bordo (r 2.7), anello (r 1.75) aperto in alto, anello (r 0.8) aperto in basso, due
        /// muri radiali che obbligano al giro lungo. Corridoi larghi uguali (0.95): le buche ci stanno dentro davvero.
        /// </summary>
        public static MazeSeg[] CircleTrapMazeRw()
        {
            var l = new List<MazeSeg>();
            Arc(l, 2.7f, 0f, 360f);
            Arc(l, 1.75f, 104f, 436f);   // varco in alto (76°-104°)
            Arc(l, 0.8f, -60f, 240f);    // varco in basso (240°-300°)
            Radial(l, 300f, 1.75f, 2.7f);   // corridoio esterno: si va a sinistra (in senso orario)
            Radial(l, 135f, 0.8f, 1.75f);   // corridoio di mezzo: si va a destra
            return l.ToArray();
        }

        /// <summary>
        /// Buche lungo il percorso, una ogni 45°: alternate contro la parete esterna/interna (si passa dall'altro lato)
        /// e una sulla bocca di ciascun varco (si entra di sbieco). Percorso verificato con raggio di caduta 0.127.
        /// </summary>
        public static readonly float[] CircleTrapPitsRw = Polar(225f, 2.0f, 135f, 2.45f, 90f, 2.225f, 45f, 1.05f, 315f, 1.5f, 270f, 1.275f);
        /// <summary>Laser radiali da parete a parete: a sinistra nel corridoio esterno, a destra in quello di mezzo.</summary>
        public static readonly float[] CircleTrapLasersRw = { -2.62f, 0f, -1.83f, 0f, 0.88f, 0f, 1.67f, 0f };

        public static DoodleArt TrapMazeRwNeon() { return NeonMaze("maze_trap_rw", CircleTrapMazeRw()); }

        // ---------- Livello 17: labirinto al buio con trappole (coordinate prima della deformazione) ----------

        /// <summary>Buche: corridoio in basso a sinistra (contro il muro sotto), incrocio a destra (contro il bordo),
        /// angolo in alto a sinistra (contro il soffitto: chi sale troppo ci cade).</summary>
        public static readonly float[] DarkPitsRw = { -1.1f, -1.25f, 2.43f, 0f, -2.2f, 3.25f };
        /// <summary>Laser nel corridoio verticale che porta alla chiave (da muro a muro).</summary>
        public static readonly float[] DarkLasersRw = { 1.73f, -2.0f, 2.67f, -2.0f };

        // ---------- Livello 55: lo schermo in frantumi ----------

        /// <summary>Scheggia di vetro (tre forme): azzurrina trasparente con riflesso bianco.</summary>
        public static DoodleArt GlassShard(string name, int shape)
        {
            var p = new DoodlePainter(-0.36f, -0.36f, 0.36f, 0.36f, 160) { Ink = 0.025f, Wobble = 0f };
            float[] pts;
            switch (shape)
            {
                case 0: pts = new[] { -0.28f, -0.22f, 0.3f, -0.1f, -0.05f, 0.3f }; break;
                case 1: pts = new[] { -0.3f, -0.05f, -0.02f, -0.28f, 0.26f, -0.12f, 0.12f, 0.27f }; break;
                default: pts = new[] { -0.12f, -0.3f, 0.1f, -0.26f, 0.06f, 0.3f, -0.08f, 0.12f }; break;
            }
            Sdf s = Sd.Polygon(pts);
            p.InkColor = Rgba.Hex("2B2133", 0.75f);
            p.Shape(s, Rgba.Hex("BFEFFF", 0.55f));
            // riflesso: una riga bianca lungo il primo lato
            float mx = (pts[0] + pts[2]) * 0.5f, my = (pts[1] + pts[3]) * 0.5f;
            float cx = 0f, cy = 0f;
            for (int i = 0; i < pts.Length; i += 2) { cx += pts[i]; cy += pts[i + 1]; }
            cx /= pts.Length / 2; cy /= pts.Length / 2;
            float ax = pts[0] + (cx - pts[0]) * 0.3f, ay = pts[1] + (cy - pts[1]) * 0.3f;
            float bx = mx + (cx - mx) * 0.3f, by = my + (cy - my) * 0.3f;
            p.Line(Rgba.Hex("FFFFFF", 0.9f), 0.035f, ax, ay, bx, by);
            p.Fill(Sd.Circle(cx + (pts[2] - cx) * 0.35f, cy + (pts[3] - cy) * 0.35f, 0.025f), Rgba.Hex("FFFFFF", 0.85f), false);
            return new DoodleArt(name, p);
        }

        /// <summary>Punto d'impatto: raggiera di schegge bianche con un buco scuro al centro (centro 0,0, raggio ~0.6).</summary>
        public static DoodleArt GlassImpact()
        {
            var p = new DoodlePainter(-0.7f, -0.7f, 0.7f, 0.7f, 150) { Wobble = 0f };
            p.Fill(Sd.Circle(0, 0, 0.62f), Rgba.Hex("FFFFFF", 0.18f), false);
            var rnd = new Random(55);
            for (int i = 0; i < 14; i++)
            {
                double a = i / 14.0 * Math.PI * 2 + rnd.NextDouble() * 0.3;
                float r = 0.35f + (float)rnd.NextDouble() * 0.25f;
                float x1 = (float)Math.Cos(a) * 0.08f, y1 = (float)Math.Sin(a) * 0.08f;
                float x2 = (float)Math.Cos(a + 0.12) * r * 0.55f, y2 = (float)Math.Sin(a + 0.12) * r * 0.55f;
                float x3 = (float)Math.Cos(a - 0.05) * r, y3 = (float)Math.Sin(a - 0.05) * r;
                p.Line(Rgba.Hex("2B2133", 0.55f), 0.045f, x1, y1, x2, y2, x3, y3);
                p.Line(Rgba.Hex("FFFFFF"), 0.022f, x1, y1, x2, y2, x3, y3);
            }
            // anello di crepe concentrico
            for (int i = 0; i < 10; i++)
            {
                double a0 = i / 10.0 * Math.PI * 2, a1 = a0 + Math.PI * 2 / 10 * 0.8;
                float r = 0.26f + (i % 3) * 0.04f;
                p.Line(Rgba.Hex("FFFFFF", 0.85f), 0.018f, (float)Math.Cos(a0) * r, (float)Math.Sin(a0) * r,
                    (float)Math.Cos((a0 + a1) / 2) * (r + 0.03f), (float)Math.Sin((a0 + a1) / 2) * (r + 0.03f), (float)Math.Cos(a1) * r, (float)Math.Sin(a1) * r);
            }
            p.Shape(Sd.Circle(0, 0, 0.09f), Rgba.Hex("3A3F55"), 0.03f);
            p.Fill(Sd.Circle(-0.025f, 0.025f, 0.03f), Rgba.Hex("FFFFFF", 0.7f), false);
            return new DoodleArt("glass_impact", p);
        }
    }
}
