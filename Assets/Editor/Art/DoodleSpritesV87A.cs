using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.87 per il livello 119 della roadmap "The Fireless Dragon": lo sfondo (prato al tramonto:
    /// cielo pesca e lilla, sole basso, montagne viola, colline verdi con i fiorellini), il corpo del draghetto colorato
    /// (verde, pancia gialla a strisce, ali lilla, crestina arancione; origine ai piedi), le cinque teste (normale che
    /// soffia bolle, guance gonfie, fiammata a bocca spalancata, tosse, felice; origine al collo, la bocca sempre in
    /// <see cref="V87AMouthX"/>, <see cref="V87AMouthY"/>) e la grande fiammata da cartone animato (origine alla bocca,
    /// verso destra). Riusati: "logs" (falò), "flame" (fiamme del falò e fiammella trascinabile), "glow", "soap_bubble"
    /// (bolle), "puff" (fumo grigio della tosse e della fiammella spenta), "sparkle_star". Prefisso v87_a_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV87A() => new DoodleArt[]
        {
            V87A_Bg(), V87A_DragonBody(),
            V87A_Head("v87_a_head_idle", 0), V87A_Head("v87_a_head_puff", 1), V87A_Head("v87_a_head_roar", 2),
            V87A_Head("v87_a_head_cough", 3), V87A_Head("v87_a_head_happy", 4),
            V87A_Burst(),
        };

        // misure condivise col builder (coordinate del mondo e locali)
        /// <summary>Piedi del drago nel mondo.</summary>
        public const float V87ADragonX = -1.0f, V87ADragonY = -2.75f;
        /// <summary>Attacco della testa (collo), locale al drago.</summary>
        public const float V87ANeckX = 0.35f, V87ANeckY = 2.05f;
        /// <summary>Bocca, locale alla testa (stessa in tutte le pose).</summary>
        public const float V87AMouthX = 0.98f, V87AMouthY = 0.22f;
        /// <summary>Falò: centro della catasta nel mondo e centro della fiammella trascinabile sospesa sopra.</summary>
        public const float V87AFireX = 1.5f, V87AFireY = -3.45f, V87AFlameX = 1.5f, V87AFlameY = -2.0f;

        static readonly Rgba V87AGreen = Rgba.Hex("6CCB5F"), V87AGreenShade = Rgba.Hex("4E9E45");
        static readonly Rgba V87ABelly = Rgba.Hex("FFE08A"), V87ABellyLine = Rgba.Hex("E8B85A");
        static readonly Rgba V87AWing = Rgba.Hex("C98BE8"), V87AWingShade = Rgba.Hex("9E63C4");
        static readonly Rgba V87ASpike = Rgba.Hex("FF9F43"), V87ASpikeShade = Rgba.Hex("E07A22");
        static readonly Rgba V87ABlush = Rgba.Hex("FF8FA8", 0.55f);

        static float V87A_Clamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        /// <summary>
        /// Lo sfondo (coordinate del mondo): cielo al tramonto (azzurro in alto, lilla, pesca vicino all'orizzonte), sole
        /// basso, due nuvolette, montagne viola lontane, colline verdi e il prato (da y -1.6 in giù) con fiorellini e ciuffi.
        /// </summary>
        public static DoodleArt V87A_Bg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0f, 2.0f, 3.3f, 3.3f), (x, y) =>
            {
                float k = V87A_Clamp((y + 1.3f) / 6.5f);
                return k < 0.45f ? Rgba.Lerp(Rgba.Hex("FFC39A"), Rgba.Hex("D9A6E0"), k / 0.45f) : Rgba.Lerp(Rgba.Hex("D9A6E0"), Rgba.Hex("7FB8F0"), (k - 0.45f) / 0.55f);
            });
            // sole basso con l'alone
            p.FillFunc(Sd.Circle(-1.6f, 0.2f, 1.1f), (x, y) => Rgba.Hex("FFF0B8").WithAlpha((float)Math.Exp(-Math.Sqrt((x + 1.6f) * (x + 1.6f) + (y - 0.2f) * (y - 0.2f)) * 2.8f) * 0.8f));
            p.Shape(Sd.Circle(-1.6f, 0.2f, 0.45f), Rgba.Hex("FFE27A"), 0.035f);
            // nuvolette
            foreach (float[] c in new[] { new[] { 1.4f, 2.2f, 1f }, new[] { -1.9f, 1.55f, 0.75f } })
            {
                float s = c[2];
                p.Shape(Sd.Union(Sd.Circle(c[0] - 0.35f * s, c[1], 0.25f * s), Sd.Circle(c[0], c[1] + 0.12f * s, 0.32f * s), Sd.Circle(c[0] + 0.38f * s, c[1], 0.24f * s),
                    Sd.Box(c[0], c[1] - 0.1f * s, 0.55f * s, 0.14f * s, 0.12f * s)), Rgba.Hex("FFF6F0"), 0.03f);
            }
            // montagne viola lontane
            p.Shape(Sd.Polygon(-3.3f, -1.0f, -2.4f, 0.5f, -1.7f, -0.2f, -0.9f, 0.9f, 0.1f, -0.3f, 0.9f, 0.6f, 1.8f, -0.4f, 2.6f, 0.35f, 3.3f, -0.3f, 3.3f, -1.2f, -3.3f, -1.2f),
                Rgba.Hex("9C83C9"), 0.035f);
            foreach (float[] t in new[] { new[] { -2.4f, 0.5f }, new[] { -0.9f, 0.9f }, new[] { 0.9f, 0.6f }, new[] { 2.6f, 0.35f } })
                p.Fill(Sd.Polygon(t[0] - 0.22f, t[1] - 0.3f, t[0], t[1], t[0] + 0.22f, t[1] - 0.3f, t[0], t[1] - 0.2f), Rgba.Hex("F4ECFF"));
            // colline verdi
            p.Shape(Sd.Union(Sd.Ellipse(-2.0f, -1.45f, 2.2f, 0.75f), Sd.Ellipse(1.9f, -1.35f, 2.0f, 0.8f)), Rgba.Hex("8FD06B"), 0.035f);
            // prato
            p.FillFunc(Sd.Box(0f, -3.6f, 3.3f, 2.0f), (x, y) => Rgba.Lerp(Rgba.Hex("7CC45A"), Rgba.Hex("5FA846"), V87A_Clamp((-1.6f - y) / 3.5f)));
            p.InkLine(0.035f, -3.3f, -1.62f, -1.5f, -1.58f, 0f, -1.66f, 1.5f, -1.58f, 3.3f, -1.62f);
            // ciuffi d'erba e fiorellini
            var rnd = new Random(87);
            for (int i = 0; i < 26; i++)
            {
                float x = (float)rnd.NextDouble() * 6.2f - 3.1f, y = -1.9f - (float)rnd.NextDouble() * 3.2f;
                p.Line(Rgba.Hex("4C8F3A"), 0.025f, x - 0.06f, y, x - 0.1f, y + 0.12f);
                p.Line(Rgba.Hex("4C8F3A"), 0.025f, x, y, x, y + 0.15f);
                p.Line(Rgba.Hex("4C8F3A"), 0.025f, x + 0.06f, y, x + 0.1f, y + 0.12f);
            }
            string[] petals = { "FFFFFF", "FFD84A", "FF8FB8", "B9A3FF" };
            for (int i = 0; i < 14; i++)
            {
                float x = (float)rnd.NextDouble() * 6.0f - 3.0f, y = -1.95f - (float)rnd.NextDouble() * 3.0f;
                p.Fill(Sd.Circle(x, y, 0.07f), Rgba.Hex(petals[i % petals.Length]));
                p.Fill(Sd.Circle(x, y, 0.03f), Rgba.Hex("FFB347"));
            }
            return new DoodleArt("v87_a_bg", p);
        }

        /// <summary>
        /// Il corpo del draghetto seduto (origine ai piedi, guarda a destra): coda a spirale con la punta a picca a sinistra,
        /// ala lilla dietro la schiena, pancione verde con la pancia gialla a strisce, zampette, braccino e collo fino
        /// all'attacco della testa (<see cref="V87ANeckX"/>, <see cref="V87ANeckY"/>), crestina arancione sulla schiena.
        /// </summary>
        public static DoodleArt V87A_DragonBody()
        {
            var p = new DoodlePainter(-1.75f, -0.15f, 1.15f, 2.55f, 110) { Ink = 0.045f };
            // ala dietro (prima di tutto)
            Sdf wing = Sd.Polygon(-0.35f, 1.55f, -0.85f, 2.45f, -1.05f, 2.0f, -1.5f, 2.1f, -1.25f, 1.65f, -1.6f, 1.55f, -0.5f, 1.15f);
            Body(p, wing, V87AWing, V87AWingShade);
            p.InkLine(0.025f, -0.45f, 1.45f, -0.95f, 2.05f);
            p.InkLine(0.025f, -0.48f, 1.35f, -1.3f, 1.7f);
            // coda a spirale con la punta a picca
            Sdf tail = Sd.Union(Sd.Polyline(0.17f, -0.5f, 0.35f, -1.0f, 0.22f, -1.35f, 0.35f, -1.45f, 0.65f),
                Sd.Polyline(0.1f, -1.45f, 0.65f, -1.38f, 0.88f));
            Body(p, tail, V87AGreen, V87AGreenShade);
            p.Shape(Sd.Polygon(-1.38f, 0.88f, -1.6f, 1.05f, -1.38f, 1.38f, -1.16f, 1.05f), V87ASpike, 0.035f);
            // crestina sulla schiena
            foreach (float[] s in new[] { new[] { -0.72f, 1.55f, 140f }, new[] { -0.82f, 1.15f, 160f }, new[] { -0.8f, 0.72f, 185f }, new[] { -0.5f, 1.9f, 120f } })
            {
                Sdf spike = Sd.Rotate(Sd.Polygon(-0.13f, 0f, 0.13f, 0f, 0f, 0.26f), 0f, 0f, s[2] - 90f);
                p.Shape(Sd.Offset(spike, s[0], s[1]), V87ASpike, 0.03f);
            }
            // collo e pancione
            Sdf neck = Sd.Capsule(0.1f, 1.55f, V87ANeckX, V87ANeckY - 0.1f, 0.33f);
            Sdf belly = Sd.Ellipse(0f, 0.95f, 0.85f, 0.95f);
            Body(p, Sd.Union(neck, belly), V87AGreen, V87AGreenShade);
            // pancia gialla a strisce
            Sdf front = Sd.Union(Sd.Ellipse(0.28f, 0.88f, 0.5f, 0.72f), Sd.Capsule(0.32f, 1.4f, V87ANeckX + 0.1f, V87ANeckY - 0.3f, 0.15f));
            p.Shape(front, V87ABelly, 0.035f);
            foreach (float y in new[] { 0.45f, 0.75f, 1.05f, 1.35f })
            {
                float hw = y < 1.4f ? 0.42f * (float)Math.Sqrt(Math.Max(0.0, 1.0 - Math.Pow((y - 0.88f) / 0.72f, 2))) : 0.15f;
                float cx = y < 1.4f ? 0.28f : 0.38f + (y - 1.4f) * 0.2f;
                p.Line(V87ABellyLine, 0.028f, cx - hw, y, cx + hw, y + 0.02f);
            }
            // zampette
            foreach (float s in new[] { -0.42f, 0.48f })
            {
                Body(p, Sd.Ellipse(s, 0.13f, 0.32f, 0.17f), V87AGreen, V87AGreenShade);
                foreach (float t in new[] { -0.15f, 0f, 0.15f }) p.Fill(Sd.Circle(s + 0.18f + t * 0.6f, 0.04f + Math.Abs(t) * 0.1f, 0.045f), Rgba.Hex("FFF4D6"));
            }
            // braccino
            Body(p, Sd.Rotate(Sd.Ellipse(0.62f, 1.1f, 0.12f, 0.26f), 0.62f, 1.1f, -35f), V87AGreen, V87AGreenShade);
            return new DoodleArt("v87_a_dragon_body", p);
        }

        /// <summary>
        /// Una testa del draghetto (origine al collo, guarda a destra): testa tonda, muso ovale con le narici, cornetti
        /// arancioni, guancia rosa e la bocca in (<see cref="V87AMouthX"/>, <see cref="V87AMouthY"/>).
        /// pose 0 normale (bocca a "o" che soffia bolle), 1 guance gonfie (occhio sgranato, labbra strette), 2 bocca
        /// spalancata della fiammata, 3 tosse (occhi strizzati, linguetta), 4 felice (occhio chiuso, sorriso).
        /// </summary>
        public static DoodleArt V87A_Head(string name, int pose)
        {
            var p = new DoodlePainter(-0.6f, -0.35f, 1.3f, 1.25f, 140) { Ink = 0.04f };
            float mx = V87AMouthX, my = V87AMouthY;
            // cornetti e orecchia
            foreach (float[] h in new[] { new[] { -0.12f, 0.88f, 20f }, new[] { 0.22f, 0.95f, -5f } })
                p.Shape(Sd.Rotate(Sd.Polygon(h[0] - 0.09f, h[1], h[0] + 0.09f, h[1], h[0] - 0.02f, h[1] + 0.3f), h[0], h[1], h[2]), V87ASpike, 0.03f);
            p.Shape(Sd.Rotate(Sd.Ellipse(-0.38f, 0.62f, 0.11f, 0.22f), -0.38f, 0.62f, 35f), V87AWing, 0.03f);
            // testa e muso
            float cheek = pose == 1 ? 0.12f : 0f;
            Sdf head = Sd.Circle(0.15f, 0.45f, 0.58f);
            Sdf snout = Sd.Ellipse(0.66f, 0.25f, 0.42f + cheek * 0.3f, 0.3f + cheek);
            Sdf skull = Sd.Union(head, snout);
            if (pose == 2) skull = Sd.Subtract(skull, Sd.Polygon(0.6f, 0.24f, 1.2f, 0.52f, 1.2f, 0.06f));   // mascella spalancata
            Body(p, skull, V87AGreen, V87AGreenShade);
            if (pose == 1) p.InkLine(0.025f, 0.32f, 0.02f, 0.45f, -0.04f, 0.6f, -0.02f);   // guancia gonfia
            p.Fill(Sd.Ellipse(0.35f, 0.18f, 0.13f, 0.08f), V87ABlush);
            // narici
            foreach (float d in new[] { 0f, 0.1f }) p.Fill(Sd.Ellipse(0.9f + d * 0.4f, 0.42f + d * 0.06f + cheek * 0.5f, 0.03f, 0.022f), DoodlePainter.DefaultInk);
            // occhio
            float ex = 0.28f, ey = 0.6f;
            switch (pose)
            {
                case 1: // sgranato
                    p.Shape(Sd.Circle(ex, ey, 0.21f), Rgba.White, 0.035f);
                    p.Fill(Sd.Circle(ex + 0.04f, ey, 0.08f), DoodlePainter.DefaultInk);
                    p.Fill(Sd.Circle(ex + 0.06f, ey + 0.03f, 0.025f), Rgba.White);
                    break;
                case 3: // strizzato (> <) con la lacrimuccia
                    p.InkLine(0.04f, ex - 0.13f, ey + 0.1f, ex + 0.05f, ey, ex - 0.13f, ey - 0.1f);
                    p.Fill(Sd.Union(Sd.Circle(ex - 0.08f, ey - 0.23f, 0.045f), Sd.Polygon(ex - 0.12f, ey - 0.22f, ex - 0.04f, ey - 0.22f, ex - 0.08f, ey - 0.14f)), Rgba.Hex("8FD0FF"));
                    break;
                case 4: // felice, occhio chiuso ad arco
                    p.InkLine(0.04f, ex - 0.14f, ey - 0.02f, ex - 0.05f, ey + 0.08f, ex + 0.05f, ey + 0.08f, ex + 0.14f, ey - 0.02f);
                    break;
                case 2: // determinato, mezzo chiuso
                    p.Shape(Sd.Subtract(Sd.Circle(ex, ey, 0.17f), Sd.Box(ex, ey + 0.17f, 0.2f, 0.09f)), Rgba.White, 0.035f);
                    p.Fill(Sd.Intersect(Sd.Circle(ex + 0.05f, ey - 0.02f, 0.085f), Sd.Box(ex, ey - 0.1f, 0.2f, 0.18f)), DoodlePainter.DefaultInk);
                    p.InkLine(0.04f, ex - 0.18f, ey + 0.12f, ex + 0.18f, ey + 0.04f);
                    break;
                default: // normale, grande e tenero
                    p.Shape(Sd.Circle(ex, ey, 0.17f), Rgba.White, 0.035f);
                    p.Fill(Sd.Circle(ex + 0.05f, ey - 0.01f, 0.09f), DoodlePainter.DefaultInk);
                    p.Fill(Sd.Circle(ex + 0.08f, ey + 0.03f, 0.03f), Rgba.White);
                    break;
            }
            // bocca
            switch (pose)
            {
                case 0: // "o" che soffia bolle
                    p.Shape(Sd.Circle(mx - 0.02f, my - 0.04f, 0.08f), Rgba.Hex("7A2E3A"), 0.03f);
                    p.InkLine(0.025f, mx - 0.35f, my - 0.12f, mx - 0.18f, my - 0.08f);
                    break;
                case 1: // labbra strette (tiene dentro la fiamma)
                    p.InkLine(0.035f, mx - 0.18f, my - 0.08f, mx - 0.06f, my - 0.03f, mx + 0.04f, my - 0.06f);
                    p.Fill(Sd.Circle(mx + 0.02f, my - 0.02f, 0.04f), Rgba.Hex("FFB23A", 0.85f));
                    break;
                case 2: // spalancata: palato scuro, dentini e lingua
                    p.Shape(Sd.Intersect(Sd.Polygon(0.64f, 0.24f, 1.14f, 0.48f, 1.14f, 0.08f), Sd.Grow(Sd.Ellipse(0.66f, 0.25f, 0.42f, 0.3f), 0.1f)), Rgba.Hex("7A2E3A"), 0.035f);
                    p.Fill(Sd.Ellipse(0.96f, 0.14f, 0.12f, 0.045f), Rgba.Hex("FF7A8A"));
                    foreach (float t in new[] { 0.85f, 1.0f })
                    {
                        p.Shape(Sd.Polygon(t - 0.04f, 0.24f + (t - 0.64f) * 0.48f, t + 0.04f, 0.24f + (t + 0.04f - 0.64f) * 0.48f, t + 0.0f, 0.15f + (t - 0.64f) * 0.48f), Rgba.White, 0.02f);
                    }
                    break;
                case 3: // tosse: bocca aperta storta con la linguetta
                    p.Shape(Sd.Ellipse(mx - 0.06f, my - 0.08f, 0.12f, 0.09f), Rgba.Hex("7A2E3A"), 0.03f);
                    p.Fill(Sd.Ellipse(mx - 0.04f, my - 0.14f, 0.07f, 0.04f), Rgba.Hex("FF7A8A"));
                    break;
                default: // sorriso
                    p.InkLine(0.035f, mx - 0.4f, my - 0.05f, mx - 0.25f, my - 0.15f, mx - 0.05f, my - 0.1f, mx + 0.05f, my - 0.02f);
                    break;
            }
            return new DoodleArt(name, p);
        }

        /// <summary>
        /// La grande fiammata da cartone animato (origine alla bocca, verso destra, lunga 2.45, alta ±0.7): lingue di fuoco
        /// rosse, arancioni e gialle con il cuore chiaro.
        /// </summary>
        public static DoodleArt V87A_Burst()
        {
            var p = new DoodlePainter(-0.12f, -0.82f, 2.55f, 0.82f, 90) { Ink = 0.045f, Wobble = 0.03f };
            Sdf outer = Sd.Union(
                Sd.Polygon(0f, 0.1f, 1.1f, 0.5f, 1.35f, 0.75f, 1.55f, 0.5f, 1.95f, 0.72f, 2.0f, 0.42f, 2.45f, 0.35f, 2.2f, 0.05f, 2.48f, -0.22f,
                    2.0f, -0.4f, 1.95f, -0.72f, 1.55f, -0.5f, 1.3f, -0.72f, 1.1f, -0.48f, 0f, -0.1f),
                Sd.Circle(1.75f, 0f, 0.5f));
            p.Shape(outer, Rgba.Hex("FF5A2E"));
            Sdf mid = Sd.Union(Sd.Polygon(0.1f, 0.06f, 1.2f, 0.36f, 1.55f, 0.48f, 1.8f, 0.3f, 2.2f, 0.12f, 2.05f, -0.1f, 2.2f, -0.18f, 1.8f, -0.32f, 1.5f, -0.48f, 1.2f, -0.34f, 0.1f, -0.06f),
                Sd.Circle(1.7f, 0f, 0.36f));
            p.Fill(mid, Rgba.Hex("FFA62E"));
            Sdf core = Sd.Union(Sd.Polygon(0.25f, 0.03f, 1.3f, 0.2f, 1.95f, 0.05f, 1.95f, -0.05f, 1.3f, -0.2f, 0.25f, -0.03f), Sd.Circle(1.62f, 0f, 0.22f));
            p.Fill(core, Rgba.Hex("FFE36B"));
            p.Fill(Sd.Union(Sd.Polygon(0.45f, 0.01f, 1.4f, 0.08f, 1.7f, 0f, 1.4f, -0.08f, 0.45f, -0.01f), Sd.Circle(1.58f, 0f, 0.1f)), Rgba.Hex("FFF8D6"));
            // scintille sparse
            foreach (float[] s in new[] { new[] { 2.3f, 0.6f }, new[] { 2.4f, -0.55f }, new[] { 0.9f, 0.62f }, new[] { 1.0f, -0.62f } })
                p.Shape(Sd.Circle(s[0], s[1], 0.06f), Rgba.Hex("FFD84A"), 0.025f);
            return new DoodleArt("v87_a_burst", p);
        }
    }
}
