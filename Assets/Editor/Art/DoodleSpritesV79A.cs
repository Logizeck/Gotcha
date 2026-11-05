using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.79 per il livello 19 della roadmap "The Shy Monster" (i fumetti sono "bubble_box"): la tana
    /// del mostro (muro di pietra viola con due torce, pavimento di lastre), il mostro verde enorme (corpo con testa,
    /// corna, occhioni e bocca piena di denti; braccia a parte con gli artigli; sopracciglio a parte da ruotare; guance
    /// rosse a parte; braccia "timide" che coprono gli occhi; il simbolo della rabbia), la coccarda "BEST MONSTER" (la
    /// scritta è un TextMesh), la spadina e la fionda. Nessuna scritta disegnata. Prefisso v79_a_.
    /// Tutte le parti del mostro sono disegnate nelle coordinate locali del mostro (origine ai piedi, scala 1).
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV79A() => new DoodleArt[]
        {
            V79A_LairBg(), V79A_MonsterBody(), V79A_MonsterArm(), V79A_MonsterBrow(), V79A_MonsterBlush(), V79A_MonsterShy(),
            V79A_MonsterAnger(), V79A_Medal(), V79A_Sword(), V79A_Slingshot(),
        };

        // misure condivise col builder (coordinate locali del mostro: origine ai piedi, scala 1)
        /// <summary>Spalle del mostro (perno delle braccia; la sinistra è lo specchio della destra).</summary>
        public const float V79AShoulderX = 0.95f, V79AShoulderY = 2.2f;
        /// <summary>Sopracciglia (centro di ciascuna) sopra gli occhi.</summary>
        public const float V79ABrowX = 0.37f, V79ABrowY = 3.72f;
        /// <summary>Centro delle guance (tra le due guance rosse).</summary>
        public const float V79ACheekY = 2.98f;
        /// <summary>Centro della testa e punto in cui si appunta la coccarda (centro del petto).</summary>
        public const float V79AHeadY = 3.25f, V79APinY = 1.85f;
        /// <summary>Simbolo della rabbia accanto alla testa.</summary>
        public const float V79AAngerX = 0.86f, V79AAngerY = 3.92f;

        static readonly Rgba V79AGreen = Rgba.Hex("7CC56A"), V79AGreenShade = Rgba.Hex("4F9446"), V79ABelly = Rgba.Hex("DCEFAE");
        static readonly Rgba V79AHorn = Rgba.Hex("F3E4C2"), V79AHornShade = Rgba.Hex("CDB98F"), V79AMouth = Rgba.Hex("5A1F3A");
        static readonly Rgba V79AGold = Rgba.Hex("F2C14E"), V79AGoldShade = Rgba.Hex("C9922B"), V79ARibbon = Rgba.Hex("D8473F");
        static readonly Rgba V79ARibbon2 = Rgba.Hex("3F6FD8"), V79ACream = Rgba.Hex("FFF6DC");

        /// <summary>
        /// La tana, coordinate del mondo: muro di pietra viola scuro a blocchi irregolari con due torce accese (x ±2.15,
        /// y 0.4), qualche ragnatela negli angoli e il pavimento di lastre (sotto y -3.3), un po' più chiaro.
        /// </summary>
        public static DoodleArt V79A_LairBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.05f, Wobble = 0.006f };
            p.FillFunc(Sd.Box(0f, 1.9f, 5f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("3A3152"), Rgba.Hex("514573"), Math.Max(0f, Math.Min(1f, (y + 3f) / 9f))));
            // blocchi di pietra (file sfalsate)
            for (int row = 0; row < 16; row++)
            {
                float y = -3.0f + row * 0.48f;
                float shift = (row & 1) == 0 ? 0f : 0.42f;
                for (int col = -6; col <= 6; col++)
                {
                    float x = col * 0.84f + shift;
                    float w = 0.36f + 0.03f * (float)Math.Sin(row * 3.1f + col * 1.7f);
                    p.Shape(Sd.Box(x, y + 0.24f, w, 0.2f, 0.06f), Rgba.Hex("463C63", 0.9f), 0.025f);
                }
            }
            // luce calda delle torce
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Circle(s * 2.15f, 0.75f, 1.5f), Rgba.Hex("FFB347", 0.08f), false);
                p.Fill(Sd.Circle(s * 2.15f, 0.75f, 0.9f), Rgba.Hex("FFB347", 0.1f), false);
                // il supporto e la torcia
                p.Shape(Sd.Box(s * 2.15f, 0.05f, 0.16f, 0.06f, 0.02f), Rgba.Hex("6B6F80"), 0.03f);
                p.Shape(Sd.Polygon(s * 2.15f - 0.09f, 0.55f, s * 2.15f + 0.09f, 0.55f, s * 2.15f + 0.05f, -0.05f, s * 2.15f - 0.05f, -0.05f), Rgba.Hex("8A5A36"), 0.03f);
                p.Shape(Sd.Union(Sd.Ellipse(s * 2.15f, 0.78f, 0.17f, 0.24f), Sd.Polygon(s * 2.15f - 0.12f, 0.85f, s * 2.15f + 0.02f, 1.2f, s * 2.15f + 0.12f, 0.85f)), Rgba.Hex("FF8A2B"), 0.03f);
                p.Fill(Sd.Ellipse(s * 2.15f, 0.74f, 0.08f, 0.13f), Rgba.Hex("FFE27A"));
            }
            // ragnatele negli angoli alti
            foreach (float s in new[] { -1f, 1f })
            {
                float cx = s * 2.8f, cy = 3.0f;
                for (int i = 0; i < 4; i++)
                {
                    float a = (float)(Math.PI * (1.0 + i / 6.0));
                    float ex = cx - s * (float)Math.Abs(Math.Cos(a)) * 1.0f, ey = cy - (float)Math.Abs(Math.Sin(a)) * 1.0f;
                    p.Line(Rgba.Hex("C9C2DD", 0.5f), 0.015f, cx, cy, ex, ey);
                }
                foreach (float r in new[] { 0.35f, 0.65f, 0.9f })
                    p.Line(Rgba.Hex("C9C2DD", 0.45f), 0.012f, cx - s * r, cy, cx - s * r * 0.85f, cy - r * 0.45f, cx - s * r * 0.45f, cy - r * 0.85f, cx, cy - r);
            }
            // pavimento di lastre
            p.Shape(Sd.Box(0f, -3.3f, 5f, 0.07f), Rgba.Hex("2E2742"), 0.03f);
            p.FillFunc(Sd.Box(0f, -5.2f, 5f, 1.85f), (x, y) =>
            {
                float v = y + 3.35f;
                int rowI = (int)Math.Floor(-v / 0.5f);
                float u = x + rowI * 0.37f;
                bool seam = Math.Abs(u / 1.05f - (float)Math.Round(u / 1.05f)) * 1.05f < 0.02f || Math.Abs(-v / 0.5f - (float)Math.Round(-v / 0.5f)) * 0.5f < 0.015f;
                Rgba c = seam ? Rgba.Hex("4F4466") : Rgba.Lerp(Rgba.Hex("7A6C93"), Rgba.Hex("675A80"), ((rowI * 7 + (int)Math.Floor(u / 1.05f) * 3) & 3) / 3f);
                return Rgba.Lerp(c, Rgba.Hex("3B3352"), Math.Max(0f, Math.Min(0.55f, (-3.5f - y) / 3f)));
            });
            return new DoodleArt("v79_a_lair_bg", p);
        }

        /// <summary>
        /// Il corpo del mostro (origine ai piedi): gambe corte con gli artigli, pancione chiaro con le macchie, testa grande
        /// (centro y <see cref="V79AHeadY"/>, la faccia parte da y 2.45) con due corna ricurve, occhioni, naso a narici e un
        /// sorrisone pieno di denti aguzzi. Braccia, sopracciglia e guance rosse sono a parte.
        /// </summary>
        public static DoodleArt V79A_MonsterBody()
        {
            var p = new DoodlePainter(-1.55f, -0.15f, 1.55f, 4.75f, 100) { Ink = 0.06f };
            p.Fill(Sd.Ellipse(0f, 0.02f, 1.2f, 0.12f), new Rgba(0f, 0f, 0f, 0.25f));
            // gambe e piedi con gli artigli
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Box(s * 0.52f, 0.42f, 0.3f, 0.38f, 0.12f), V79AGreen, V79AGreenShade);
                Body(p, Sd.Ellipse(s * 0.58f, 0.1f, 0.42f, 0.14f), V79AGreen, V79AGreenShade);
                for (int i = -1; i <= 1; i++)
                    p.Shape(Sd.Polygon(s * 0.58f + i * 0.2f - 0.06f, 0.05f, s * 0.58f + i * 0.2f + 0.06f, 0.05f, s * 0.58f + i * 0.2f + s * 0.05f, -0.08f), V79AHorn, 0.025f);
            }
            // il corpo e il pancione
            Sdf torso = Sd.Ellipse(0f, 1.55f, 1.15f, 1.05f);
            Body(p, torso, V79AGreen, V79AGreenShade, Rgba.Hex("3E7A37", 0.5f));
            p.Shape(Sd.Ellipse(0f, 1.3f, 0.72f, 0.66f), V79ABelly, 0.04f);
            for (int i = 0; i < 3; i++) p.Line(Rgba.Hex("B9D58A"), 0.03f, -0.45f, 1.0f + i * 0.25f, 0.45f, 1.0f + i * 0.25f);
            foreach (float[] sp in new[] { new[] { -0.85f, 1.9f, 0.12f }, new[] { 0.9f, 1.5f, 0.1f }, new[] { -0.7f, 0.95f, 0.08f }, new[] { 0.75f, 2.15f, 0.07f } })
                p.Fill(Sd.Circle(sp[0], sp[1], sp[2]), V79AGreenShade.WithAlpha(0.8f));
            // le corna
            foreach (float s in new[] { -1f, 1f })
            {
                Sdf horn = Sd.Polygon(s * 0.42f, 3.85f, s * 0.78f, 3.7f, s * 1.08f, 4.05f, s * 1.18f, 4.62f, s * 0.95f, 4.2f, s * 0.62f, 4.02f);
                Body(p, horn, V79AHorn, V79AHornShade);
                p.Line(V79AHornShade, 0.025f, s * 0.75f, 3.85f, s * 0.85f, 3.98f);
                p.Line(V79AHornShade, 0.025f, s * 0.92f, 4.0f, s * 1.0f, 4.15f);
            }
            // la testa
            Sdf head = Sd.Union(Sd.Ellipse(0f, V79AHeadY, 1.0f, 0.8f), Sd.Ellipse(0f, 2.75f, 0.85f, 0.3f));
            Body(p, head, V79AGreen, V79AGreenShade);
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Ellipse(s * 1.0f, 3.35f, 0.16f, 0.22f), V79AGreen, 0.04f);   // orecchie
            // gli occhioni
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Ellipse(s * 0.37f, 3.42f, 0.24f, 0.22f), Rgba.White, 0.045f);
                p.Fill(Sd.Circle(s * 0.37f - s * 0.04f, 3.39f, 0.11f), Rgba.Hex("2B2233"));
                p.Fill(Sd.Circle(s * 0.37f - s * 0.07f, 3.43f, 0.035f), Rgba.White);
            }
            // narici
            foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Ellipse(s * 0.09f, 3.12f, 0.035f, 0.05f), Rgba.Hex("2E5A2A"));
            // il sorrisone con tanti denti
            Sdf mouth = Sd.Intersect(Sd.Ellipse(0f, 2.98f, 0.66f, 0.38f), Sd.Box(0f, 2.75f, 0.7f, 0.23f));
            p.Shape(mouth, V79AMouth, 0.045f);
            p.Fill(Sd.Intersect(mouth, Sd.Ellipse(0f, 2.55f, 0.32f, 0.12f)), Rgba.Hex("E2707A"));   // linguetta
            for (int i = 0; i < 7; i++)
            {
                float x = -0.48f + i * 0.16f;
                p.Shape(Sd.Intersect(mouth, Sd.Polygon(x - 0.07f, 2.99f, x + 0.07f, 2.99f, x, 2.82f)), Rgba.White, 0.018f);
            }
            for (int i = 0; i < 5; i++)
            {
                float x = -0.32f + i * 0.16f;
                p.Shape(Sd.Intersect(mouth, Sd.Polygon(x - 0.06f, 2.52f, x + 0.06f, 2.52f, x, 2.66f)), Rgba.White, 0.018f);
            }
            foreach (float s in new[] { -1f, 1f })   // zanne che spuntano sopra il labbro
                p.Shape(Sd.Polygon(s * 0.5f, 2.62f, s * 0.6f, 2.62f, s * 0.55f, 2.86f), Rgba.White, 0.025f);
            return new DoodleArt("v79_a_monster_body", p);
        }

        /// <summary>
        /// Il braccio DESTRO del mostro (origine sulla spalla, per il sinistro si specchia): sale verso l'esterno con la
        /// zampa aperta e tre artigli, posa "minacciosa".
        /// </summary>
        public static DoodleArt V79A_MonsterArm()
        {
            var p = new DoodlePainter(-0.2f, -0.25f, 0.85f, 1.05f, 100) { Ink = 0.055f };
            Body(p, Sd.Capsule(0f, 0f, 0.42f, 0.42f, 0.2f), V79AGreen, V79AGreenShade);
            Sdf paw = Sd.Ellipse(0.5f, 0.58f, 0.22f, 0.2f);
            Body(p, paw, V79AGreen, V79AGreenShade);
            for (int i = 0; i < 3; i++)
            {
                float a = (float)(Math.PI * (0.25 + i * 0.22));
                float bx = 0.5f + 0.18f * (float)Math.Cos(a), by = 0.58f + 0.17f * (float)Math.Sin(a);
                float tx = 0.5f + 0.38f * (float)Math.Cos(a), ty = 0.58f + 0.37f * (float)Math.Sin(a);
                p.Shape(Sd.Polygon(bx - 0.05f * (float)Math.Sin(a), by + 0.05f * (float)Math.Cos(a), bx + 0.05f * (float)Math.Sin(a), by - 0.05f * (float)Math.Cos(a), tx, ty), V79AHorn, 0.022f);
            }
            return new DoodleArt("v79_a_monster_arm", p);
        }

        /// <summary>Un sopracciglio folto (origine al centro, orizzontale: il livello lo ruota, arrabbiato o preoccupato).</summary>
        public static DoodleArt V79A_MonsterBrow()
        {
            var p = new DoodlePainter(-0.32f, -0.13f, 0.32f, 0.13f, 140) { Ink = 0.03f };
            Sdf brow = Sd.Union(Sd.Capsule(-0.22f, 0f, 0.22f, 0f, 0.07f), Sd.Circle(-0.12f, 0.03f, 0.07f), Sd.Circle(0.1f, 0.035f, 0.065f));
            p.Shape(brow, Rgba.Hex("2E4A2A"), 0.03f);
            for (int i = 0; i < 5; i++) p.Line(Rgba.Hex("1E331B"), 0.018f, -0.18f + i * 0.09f, -0.03f, -0.13f + i * 0.09f, 0.07f);
            return new DoodleArt("v79_a_monster_brow", p);
        }

        /// <summary>Le due guance rosse con le lineette del rossore (origine al centro, tra le guance a ±0.62).</summary>
        public static DoodleArt V79A_MonsterBlush()
        {
            var p = new DoodlePainter(-0.95f, -0.25f, 0.95f, 0.25f, 100) { Ink = 0f, Wobble = 0.004f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Ellipse(s * 0.62f, 0f, 0.27f, 0.17f), Rgba.Hex("FF4D5E", 0.55f));
                p.Fill(Sd.Ellipse(s * 0.62f, 0f, 0.19f, 0.11f), Rgba.Hex("FF2E45", 0.6f));
                for (int i = -1; i <= 1; i++) p.Line(Rgba.Hex("C8102E", 0.85f), 0.022f, s * 0.62f + i * 0.1f - 0.03f, -0.05f, s * 0.62f + i * 0.1f + 0.03f, 0.05f);
            }
            return new DoodleArt("v79_a_monster_blush", p);
        }

        /// <summary>
        /// Le braccia "timide" (coordinate locali del mostro, origine ai piedi): dalle spalle salgono piegate e le zampe
        /// coprono gli occhi. Si mostrano al posto delle braccia minacciose quando il mostro si vergogna.
        /// </summary>
        public static DoodleArt V79A_MonsterShy()
        {
            var p = new DoodlePainter(-1.45f, 1.9f, 1.45f, 3.8f, 100) { Ink = 0.055f };
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Polyline(0.18f, s * V79AShoulderX, V79AShoulderY, s * 1.18f, 2.72f, s * 0.62f, 3.3f), V79AGreen, V79AGreenShade);
                Sdf paw = Sd.Ellipse(s * 0.36f, 3.43f, 0.3f, 0.27f);
                Body(p, paw, V79AGreen, V79AGreenShade);
                for (int i = 0; i < 3; i++) p.InkLine(0.022f, s * (0.2f + i * 0.13f), 3.58f, s * (0.18f + i * 0.13f), 3.42f);
            }
            return new DoodleArt("v79_a_monster_shy", p);
        }

        /// <summary>Il simbolo della rabbia da fumetto (quattro archetti rossi), origine al centro.</summary>
        public static DoodleArt V79A_MonsterAnger()
        {
            var p = new DoodlePainter(-0.3f, -0.3f, 0.3f, 0.3f, 120) { Ink = 0f, Wobble = 0.003f };
            Rgba red = Rgba.Hex("E8283C");
            p.Line(red, 0.07f, -0.22f, 0.07f, -0.08f, 0.08f, -0.07f, 0.22f);
            p.Line(red, 0.07f, 0.07f, 0.22f, 0.08f, 0.08f, 0.22f, 0.07f);
            p.Line(red, 0.07f, 0.22f, -0.07f, 0.08f, -0.08f, 0.07f, -0.22f);
            p.Line(red, 0.07f, -0.07f, -0.22f, -0.08f, -0.08f, -0.22f, -0.07f);
            return new DoodleArt("v79_a_monster_anger", p);
        }

        /// <summary>
        /// La coccarda (origine al centro del disco): due nastri, rosso e blu, con la coda a V; disco dorato pieghettato e
        /// cerchio interno color crema (raggio 0.32) dove sta la scritta BEST MONSTER (TextMesh).
        /// </summary>
        public static DoodleArt V79A_Medal()
        {
            var p = new DoodlePainter(-0.55f, -1.0f, 0.55f, 0.55f, 120) { Ink = 0.04f };
            p.Shape(Sd.Polygon(-0.2f, -0.1f, 0.0f, -0.15f, -0.12f, -0.95f, -0.25f, -0.82f, -0.38f, -0.92f), V79ARibbon, 0.035f);
            p.Shape(Sd.Polygon(0.0f, -0.15f, 0.2f, -0.1f, 0.38f, -0.92f, 0.25f, -0.82f, 0.12f, -0.95f), V79ARibbon2, 0.035f);
            var petals = new List<Sdf>();
            for (int i = 0; i < 16; i++)
            {
                double a = i * Math.PI * 2 / 16;
                petals.Add(Sd.Circle(0.4f * (float)Math.Cos(a), 0.4f * (float)Math.Sin(a), 0.1f));
            }
            petals.Add(Sd.Circle(0f, 0f, 0.42f));
            Body(p, Sd.Union(petals.ToArray()), V79AGold, V79AGoldShade);
            p.Shape(Sd.Circle(0f, 0f, 0.33f), V79ACream, 0.03f);
            p.Shape(Sd.Circle(0f, 0f, 0.29f), V79ACream, 0.012f);
            return new DoodleArt("v79_a_medal", p);
        }

        /// <summary>La spadina giocattolo (origine al centro, orizzontale con la punta a destra; lunga 0.8).</summary>
        public static DoodleArt V79A_Sword()
        {
            var p = new DoodlePainter(-0.45f, -0.18f, 0.48f, 0.18f, 140) { Ink = 0.03f };
            p.Shape(Sd.Polygon(-0.12f, 0.05f, 0.34f, 0.05f, 0.44f, 0f, 0.34f, -0.05f, -0.12f, -0.05f), Rgba.Hex("D9DEE8"), 0.03f);
            p.Line(Rgba.White.WithAlpha(0.8f), 0.015f, -0.08f, 0.015f, 0.32f, 0.015f);
            p.Shape(Sd.Box(-0.15f, 0f, 0.035f, 0.14f, 0.02f), V79AGold, 0.025f);
            p.Shape(Sd.Box(-0.3f, 0f, 0.12f, 0.035f, 0.02f), Rgba.Hex("8A5A36"), 0.025f);
            p.Shape(Sd.Circle(-0.42f, 0f, 0.04f), V79AGold, 0.02f);
            return new DoodleArt("v79_a_sword", p);
        }

        /// <summary>La fionda di legno con l'elastico (origine al centro, alta 0.6).</summary>
        public static DoodleArt V79A_Slingshot()
        {
            var p = new DoodlePainter(-0.3f, -0.34f, 0.3f, 0.34f, 140) { Ink = 0.03f };
            Rgba wood = Rgba.Hex("A8744A"), woodShade = Rgba.Hex("7F5535");
            Body(p, Sd.Union(Sd.Capsule(0f, -0.28f, 0f, -0.02f, 0.045f), Sd.Capsule(0f, -0.02f, -0.18f, 0.24f, 0.04f), Sd.Capsule(0f, -0.02f, 0.18f, 0.24f, 0.04f)), wood, woodShade);
            p.Line(Rgba.Hex("D8473F"), 0.025f, -0.18f, 0.24f, 0f, 0.08f, 0.18f, 0.24f);
            p.Shape(Sd.Ellipse(0f, 0.08f, 0.05f, 0.035f), Rgba.Hex("6B4A34"), 0.02f);
            return new DoodleArt("v79_a_slingshot", p);
        }
    }
}
