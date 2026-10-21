using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli rifatti nella versione 0.40 (gruppo 2).</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtRw2() => new DoodleArt[]
        {
            DustLayer(), DustClumps(), DustMote(),
            FuelCanLetter(true), FuelCanLetter(false), FuelGauge3(),
            MotionTrail(), SpeedLines(), HeroCape(), WindLines(),
            ButterGloss(), ButterDrip(), PushArrow(),
        };

        static float Rw2Clamp(float v) { return v < 0f ? 0f : v > 1f ? 1f : v; }

        // ---------- Livello 18: la cassaforte nascosta (polvere sul retro del quadro) ----------

        /// <summary>Strato di polvere spesso e uniforme sul retro del quadro (stesso ingombro di "painting_back", centro 0,0).</summary>
        public static DoodleArt DustLayer()
        {
            var p = new DoodlePainter(-1.05f, -1.2f, 1.05f, 1.2f, 100) { Ink = 0f };
            Sdf area = Sd.Box(0, 0, 0.93f, 1.08f, 0.08f);
            p.FillFunc(area, (x, y) =>
            {
                float n = Noise.Value(x * 6f, y * 6f) * 0.6f + Noise.Value(x * 19f, y * 19f) * 0.4f;
                Rgba c = Rgba.Lerp(Rgba.Hex("B9B2A4"), Rgba.Hex("D8D2C4"), n);
                float edge = Rw2Clamp(-area(x, y) / 0.12f);   // bordo sfumato e "peloso"
                float a = Rw2Clamp(edge * (0.85f + n * 0.4f) - (Noise.Value(x * 23f, y * 23f) > 0.8f ? 0.25f : 0f));
                return c.WithAlpha(a);
            });
            // puntini scuri e pelucchi
            var rnd = new Random(18);
            for (int i = 0; i < 70; i++)
            {
                float x = (float)rnd.NextDouble() * 1.7f - 0.85f, y = (float)rnd.NextDouble() * 2.0f - 1.0f;
                p.Fill(Sd.Circle(x, y, 0.012f + (float)rnd.NextDouble() * 0.02f), Rgba.Hex("8A8377", 0.7f));
            }
            for (int i = 0; i < 9; i++)
            {
                float x = (float)rnd.NextDouble() * 1.5f - 0.75f, y = (float)rnd.NextDouble() * 1.8f - 0.9f;
                p.Line(Rgba.Hex("9C9587", 0.8f), 0.012f, x, y, x + 0.08f, y + 0.05f, x + 0.12f, y - 0.02f, x + 0.2f, y + 0.03f);
            }
            return new DoodleArt("dust_layer", p);
        }

        /// <summary>Grumi di polvere sparsi (restano per ultimi quando si soffia), stesso ingombro di "dust_layer".</summary>
        public static DoodleArt DustClumps()
        {
            var p = new DoodlePainter(-1.05f, -1.2f, 1.05f, 1.2f, 100) { Ink = 0f };
            Sdf area = Sd.Box(0, 0, 0.9f, 1.05f, 0.08f);
            // macchie morbide (niente contorno: è polvere), più fitte al centro dove c'è il codice
            Sdf clumps = (x, y) => Math.Max(area(x, y), (0.56f - Noise.Value(x * 3.2f + 7f, y * 3.2f + 3f) - 0.25f * Rw2Clamp(1f - (x * x + (y + 0.15f) * (y + 0.15f)) * 2.5f)) * 0.5f);
            p.FillFunc(Sd.Grow(clumps, 0.05f), (x, y) =>
            {
                float d = clumps(x, y);
                float n = Noise.Value(x * 14f, y * 14f);
                float a = Rw2Clamp(0.5f - d / 0.1f) * (0.8f + 0.2f * n);
                return Rgba.Lerp(Rgba.Hex("A39B8C"), Rgba.Hex("CBC4B5"), n).WithAlpha(a);
            });
            // batuffoli di polvere
            foreach (var (cx, cy, r) in new[] { (-0.5f, 0.6f, 0.13f), (0.52f, -0.7f, 0.15f), (0.45f, 0.35f, 0.1f), (-0.4f, -0.55f, 0.12f) })
            {
                Sdf ball = (x, y) => Sd.Circle(cx, cy, r)(x, y) - (Noise.Value(x * 30f, y * 30f) - 0.5f) * 0.07f;
                p.Fill(ball, Rgba.Hex("BDB6A8"));
                p.Fill(Sd.Circle(cx - r * 0.3f, cy + r * 0.3f, r * 0.35f), Rgba.Hex("D8D2C4"));
            }
            return new DoodleArt("dust_clumps", p);
        }

        /// <summary>Fiocco di polvere che vola via quando si soffia (particella).</summary>
        public static DoodleArt DustMote()
        {
            var p = new DoodlePainter(-0.16f, -0.16f, 0.16f, 0.16f, 160) { Ink = 0.02f, InkColor = Rgba.Hex("7D7669") };
            Sdf fluff = (x, y) => Sd.Circle(0, 0, 0.1f)(x, y) - (Noise.Value(x * 45f + 3f, y * 45f) - 0.5f) * 0.06f;
            p.Shape(fluff, Rgba.Hex("D3CCBE"));
            p.Line(Rgba.Hex("8A8377"), 0.01f, -0.05f, 0f, 0f, 0.03f, 0.05f, -0.01f);
            return new DoodleArt("dust_mote", p);
        }

        // ---------- Livello 20: si parte! (H + H + O) ----------

        /// <summary>Tanica con la lettera grande: "fuel_can_h" azzurra, "fuel_can_o" arancione.</summary>
        public static DoodleArt FuelCanLetter(bool hydrogen)
        {
            var p = new DoodlePainter(-0.48f, -0.55f, 0.48f, 0.62f, 140) { Ink = 0.04f };
            Rgba body = hydrogen ? Rgba.Hex("3FA9F5") : Rgba.Hex("FF7A3D");
            Rgba shade = hydrogen ? Rgba.Hex("2A7FC2") : Rgba.Hex("D2552A");
            Sdf can = Sd.Box(0, -0.08f, 0.38f, 0.44f, 0.08f);
            Body(p, can, body, shade);
            p.Shape(Sd.Subtract(Sd.Box(-0.12f, 0.42f, 0.2f, 0.11f, 0.06f), Sd.Box(-0.12f, 0.42f, 0.11f, 0.04f, 0.03f)), shade, 0.025f);   // maniglia
            p.Shape(Sd.Box(0.24f, 0.44f, 0.07f, 0.1f, 0.02f), Rgba.Hex("FFC93C"), 0.025f);                                         // tappo
            p.Shape(Sd.Circle(0, -0.1f, 0.27f), Rgba.Hex("FFF8E8"), 0.03f);                                                        // bollino bianco
            Sdf letter = hydrogen
                ? Sd.Union(Sd.Capsule(-0.1f, -0.25f, -0.1f, 0.05f, 0.045f), Sd.Capsule(0.1f, -0.25f, 0.1f, 0.05f, 0.045f), Sd.Capsule(-0.1f, -0.1f, 0.1f, -0.1f, 0.04f))
                : Sd.Subtract(Sd.Ellipse(0, -0.1f, 0.15f, 0.17f), Sd.Ellipse(0, -0.1f, 0.065f, 0.085f));
            p.Shape(letter, body, 0.03f);
            p.Fill(Sd.Box(-0.3f, 0.12f, 0.03f, 0.14f, 0.02f), Rgba.White.WithAlpha(0.35f));                                        // riflesso
            return new DoodleArt(hydrogen ? "fuel_can_h" : "fuel_can_o", p);
        }

        /// <summary>Pannello del carburante a 3 finestrelle (H, H, O) con le finestre trasparenti (centro 0,0; celle a x = -0.6, 0, 0.6).</summary>
        public static DoodleArt FuelGauge3()
        {
            var p = new DoodlePainter(-1.05f, -0.4f, 1.05f, 0.4f, 120) { Ink = 0.045f };
            Sdf windows = Sd.Union(Sd.Box(-0.6f, 0f, 0.22f, 0.24f, 0.05f), Sd.Box(0f, 0f, 0.22f, 0.24f, 0.05f), Sd.Box(0.6f, 0f, 0.22f, 0.24f, 0.05f));
            Sdf panel = Sd.Subtract(Sd.Box(0, 0, 0.98f, 0.34f, 0.1f), windows);
            Body(p, panel, Rgba.Hex("8D93A6"), Rgba.Hex("666B7D"));
            p.Shape(windows, Rgba.Clear, 0.035f);
            foreach (float x in new[] { -0.6f, 0f, 0.6f })
                p.Fill(Sd.Box(x - 0.13f, 0.12f, 0.03f, 0.07f, 0.02f), Rgba.White.WithAlpha(0.35f));   // riflesso del vetro
            foreach (float x in new[] { -0.92f, 0.92f }) p.Shape(Sd.Circle(x, 0.26f, 0.03f), Rgba.Hex("C3C8D6"), 0.015f);
            return new DoodleArt("fuel_gauge3", p);
        }

        // ---------- Livello 21: batteria scarica (trasformazione epica) ----------

        /// <summary>Scia di movimento bianca, rastremata: parte da (0,0) e si allunga verso +x sfumando (si tinge e si ruota). Ingombro simmetrico: il perno è in (0,0).</summary>
        public static DoodleArt MotionTrail()
        {
            var p = new DoodlePainter(-2.3f, -0.3f, 2.3f, 0.3f, 80) { Ink = 0f };
            Sdf trail = Sd.Polygon(0f, 0.24f, 2.2f, 0.02f, 2.2f, -0.02f, 0f, -0.24f);
            p.FillFunc(Sd.Union(trail, Sd.Circle(0, 0, 0.24f)), (x, y) =>
            {
                float t = Rw2Clamp(x / 2.2f);
                float core = Rw2Clamp(1f - Math.Abs(y) / (0.24f * (1f - t) + 0.02f));
                return Rgba.White.WithAlpha((1f - t) * (0.45f + 0.55f * core));
            });
            return new DoodleArt("motion_trail", p);
        }

        /// <summary>Linee cinetiche da manga attorno al centro (vuoto al centro), bianche da tingere; copre tutto lo schermo.</summary>
        public static DoodleArt SpeedLines()
        {
            var p = new DoodlePainter(-4.5f, -6.5f, 4.5f, 6.5f, 40) { Ink = 0f };
            var rnd = new Random(21);
            int n = 64;
            var wedges = new List<Sdf>();
            for (int i = 0; i < n; i++)
            {
                double a = (i + rnd.NextDouble() * 0.6) * Math.PI * 2 / n;
                float r0 = 2.0f + (float)rnd.NextDouble() * 1.4f, w = 0.05f + (float)rnd.NextDouble() * 0.09f;
                float ca = (float)Math.Cos(a), sa = (float)Math.Sin(a);
                float r1 = 9f;
                // cuneo sottile: punta verso il centro, largo fuori
                wedges.Add(Sd.Polygon(ca * r0, sa * r0, ca * r1 - sa * w * 3.5f, sa * r1 + ca * w * 3.5f, ca * r1 + sa * w * 3.5f, sa * r1 - ca * w * 3.5f));
            }
            p.Fill(Sd.Union(wedges.ToArray()), Rgba.White, false);
            return new DoodleArt("speed_lines", p);
        }

        /// <summary>Mantello rosso da eroe, attaccato alle spalle in (0,0), che sventola verso destra. Ingombro simmetrico: il perno è in (0,0).</summary>
        public static DoodleArt HeroCape()
        {
            var p = new DoodlePainter(-1.9f, -2.3f, 1.9f, 2.3f, 80) { Ink = 0.05f };
            Sdf cape = (x, y) =>
            {
                // trapezio che si allarga in basso e piega a destra, orlo ondulato
                float t = Rw2Clamp(-y / 2.0f);
                float cx = 0.15f + t * t * 0.9f;
                float half = 0.42f + t * 0.45f;
                float hem = -2.0f + (float)Math.Sin((x - cx) * 7f) * 0.1f;
                return Math.Max(Math.Max(Math.Abs(x - cx) - half, y - 0.1f), hem - y);
            };
            p.FillFunc(cape, (x, y) =>
            {
                float t = Rw2Clamp(-y / 2.0f);
                float fold = (float)Math.Sin((x - (0.15f + t * t * 0.9f)) * 6f + 1f) * 0.5f + 0.5f;
                return Rgba.Lerp(Rgba.Hex("A8192C"), Rgba.Hex("E8233F"), fold);
            });
            p.Shape(cape, Rgba.Clear);
            for (int i = 0; i < 3; i++)
            {
                float x0 = -0.15f + i * 0.3f;
                p.Line(Rgba.Hex("7E1020", 0.7f), 0.025f, x0, -0.15f, x0 + 0.3f, -1.0f, x0 + 0.75f + i * 0.08f, -1.75f);   // pieghe
            }
            p.Shape(Sd.Box(0.15f, 0.08f, 0.45f, 0.08f, 0.04f), Rgba.Hex("FFC93C"), 0.03f);   // fermaglio dorato
            return new DoodleArt("hero_cape", p);
        }

        /// <summary>Tre linee di vento a ricciolo (bianche), per la posa eroica.</summary>
        public static DoodleArt WindLines()
        {
            var p = new DoodlePainter(-1.2f, -0.6f, 1.2f, 0.6f, 100) { Ink = 0f };
            Rgba c = Rgba.White;
            p.Line(c, 0.045f, -1.1f, 0.35f, -0.3f, 0.38f, 0.4f, 0.33f, 0.7f, 0.45f, 0.62f, 0.58f, 0.5f, 0.5f);
            p.Line(c, 0.045f, -0.8f, 0f, 0.1f, 0.02f, 0.85f, -0.02f, 1.08f, 0.1f, 1.0f, 0.24f, 0.86f, 0.16f);
            p.Line(c, 0.04f, -1.0f, -0.4f, -0.2f, -0.36f, 0.35f, -0.42f);
            return new DoodleArt("wind_lines", p);
        }

        // ---------- Livello 25: STUCK! (Toni imburrato) ----------

        /// <summary>
        /// Velo lucido di burro sul sedere di Toni: origine = punto "Butt" di ToniStuckLevel (0.75, 0.15 nelle coordinate di Toni),
        /// segue la sagoma del corpo di "toni_stand". Riflessi bianchi a mezzaluna e una patina giallina.
        /// </summary>
        public static DoodleArt ButterGloss()
        {
            var p = new DoodlePainter(-1.0f, -0.65f, 0.55f, 0.55f, 130) { Ink = 0f };
            // corpo di Toni (ellisse 0.15,0.08 r 0.95x0.47) spostato nell'origine del sedere
            Sdf body = Sd.Ellipse(0.15f - 0.75f, 0.08f - 0.15f, 0.95f, 0.47f);
            Sdf rear = Sd.Intersect(body, Sd.Box(0.05f, 0f, 0.6f, 0.7f));
            p.FillFunc(rear, (x, y) => Rgba.Hex("FFE27A").WithAlpha(0.22f + 0.18f * Rw2Clamp((y + 0.3f) / 0.6f)));
            // riflessi lucidi: fascia curva lungo la schiena e due lampi più piccoli
            Sdf band = Sd.Intersect(Sd.Subtract(Sd.Ellipse(-0.6f, -0.07f, 0.85f, 0.38f), Sd.Ellipse(-0.6f, -0.13f, 0.83f, 0.33f)), Sd.Box(0.0f, 0.2f, 0.45f, 0.3f));
            p.Fill(band, Rgba.White.WithAlpha(0.85f));
            p.Fill(Sd.Rotate(Sd.Ellipse(0.12f, 0.05f, 0.05f, 0.14f), 0.12f, 0.05f, -35f), Rgba.White.WithAlpha(0.8f));
            p.Fill(Sd.Circle(0.2f, -0.12f, 0.035f), Rgba.White.WithAlpha(0.8f));
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.3f, -0.28f, 0.16f, 0.035f), -0.3f, -0.28f, 8f), Rgba.White.WithAlpha(0.45f));
            // gocce di burro che colano
            foreach (var (x, y) in new[] { (0.28f, -0.2f), (-0.1f, -0.45f) })
            {
                p.Shape(Sd.Union(Sd.Circle(x, y - 0.05f, 0.04f), Sd.Polygon(x - 0.035f, y - 0.05f, x + 0.035f, y - 0.05f, x, y + 0.04f)), Rgba.Hex("FFE27A"), 0.018f);
                p.Fill(Sd.Circle(x - 0.012f, y - 0.04f, 0.012f), Rgba.White.WithAlpha(0.9f));
            }
            return new DoodleArt("butter_gloss", p);
        }

        /// <summary>Goccia di burro fuso (particella che cola).</summary>
        public static DoodleArt ButterDrip()
        {
            var p = new DoodlePainter(-0.1f, -0.13f, 0.1f, 0.13f, 160) { Ink = 0.02f };
            p.Shape(Sd.Union(Sd.Circle(0, -0.04f, 0.06f), Sd.Polygon(-0.052f, -0.03f, 0.052f, -0.03f, 0f, 0.1f)), Rgba.Hex("FFE27A"));
            p.Fill(Sd.Circle(-0.02f, -0.03f, 0.018f), Rgba.White.WithAlpha(0.9f));
            return new DoodleArt("butter_drip", p);
        }

        /// <summary>Freccione "spingi di qua" che punta a destra (punta in 0.75,0).</summary>
        public static DoodleArt PushArrow()
        {
            var p = new DoodlePainter(-0.85f, -0.5f, 0.85f, 0.5f, 120) { Ink = 0.05f };
            Sdf arrow = Sd.Union(Sd.Box(-0.3f, 0f, 0.45f, 0.15f, 0.05f), Sd.Polygon(0.05f, 0.4f, 0.75f, 0f, 0.05f, -0.4f));
            Body(p, arrow, Rgba.Hex("FFC93C"), Rgba.Hex("E39B1B"));
            p.Fill(Sd.Box(-0.35f, 0.07f, 0.3f, 0.03f, 0.02f), Rgba.White.WithAlpha(0.55f));
            return new DoodleArt("push_arrow", p);
        }
    }
}
