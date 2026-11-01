using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.76 per il livello 5 della roadmap "The Cowardly Lion" (la coppa che cade è s41e_trophy, le
    /// onde del ruggito sono ring_waves): la savana, il costume da leone (corpo seduto con la cerniera sul petto, la coda
    /// col ciuffo e l'apertura del collo), la criniera esagerata col suo elastico sotto il mento, il muso feroce (con la
    /// bocca chiusa e coi denti aguzzi, e quello che ruggisce), l'etichetta "COSTUME" (la scritta è un TextMesh), il bottone
    /// ROAR (la scritta è un TextMesh), il gattino (la testolina nascosta nel costume, spaventato, in fuga). Prefisso v76_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV76B() => new DoodleArt[]
        {
            V76B_SavannaBg(), V76B_LionBody(), V76B_Mane(), V76B_Face(false), V76B_Face(true), V76B_Tag(), V76B_RoarButton(),
            V76B_KittenHead(), V76B_KittenScared(), V76B_KittenRun(),
        };

        static readonly Rgba V76BFur = Rgba.Hex("EBAE52"), V76BFurShade = Rgba.Hex("C98A32"), V76BFurLight = Rgba.Hex("F7D08A");
        static readonly Rgba V76BMane = Rgba.Hex("B9561F"), V76BManeShade = Rgba.Hex("8C3F17"), V76BManeInner = Rgba.Hex("D9762C");
        static readonly Rgba V76BMuzzle = Rgba.Hex("FBE6BC"), V76BMouth = Rgba.Hex("5A1622"), V76BNose = Rgba.Hex("4A2A2E");
        static readonly Rgba V76BKitten = Rgba.Hex("CFCAD6"), V76BKittenShade = Rgba.Hex("A7A1B2"), V76BKittenStripe = Rgba.Hex("8D879A");
        static readonly Rgba V76BPink = Rgba.Hex("F49AB0");

        /// <summary>
        /// La savana al tramonto, coordinate del mondo: cielo caldo, sole grande, colline lontane con due acacie, il prato
        /// (orizzonte in y -1.1) con ciuffi d'erba. Il leone siede in y -2.45, il bottone ROAR sta in y -3.75.
        /// </summary>
        public static DoodleArt V76B_SavannaBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f, Wobble = 0.006f };
            p.FillFunc(Sd.Box(0f, 3f, 5f, 4.5f), (x, y) => Rgba.Lerp(Rgba.Hex("FFC77A"), Rgba.Hex("FFE7A8"), Math.Max(0f, Math.Min(1f, (y + 1f) / 5f))));
            p.Fill(Sd.Circle(1.55f, 1.55f, 1.05f), Rgba.Hex("FFF3C4", 0.45f));
            p.Shape(Sd.Circle(1.55f, 1.55f, 0.72f), Rgba.Hex("FFE08A"), 0.04f);
            p.Fill(Sd.Union(Sd.Ellipse(-1.7f, 2.5f, 0.55f, 0.13f), Sd.Ellipse(-1.25f, 2.6f, 0.4f, 0.16f)), Rgba.White.WithAlpha(0.6f));
            p.Fill(Sd.Ellipse(0.4f, 2.95f, 0.5f, 0.1f), Rgba.White.WithAlpha(0.5f));
            // colline lontane
            p.Fill(Sd.Union(Sd.Ellipse(-2.6f, -1.15f, 2.4f, 0.75f), Sd.Ellipse(1.2f, -1.2f, 2.8f, 0.55f), Sd.Ellipse(4f, -1.1f, 1.6f, 0.8f)), Rgba.Hex("E2A764"));
            // due acacie (silhouette)
            foreach (var (tx, ty, s) in new[] { (-2.05f, -0.75f, 1f), (2.2f, -0.85f, 0.75f) })
            {
                p.Line(Rgba.Hex("7A4A30"), 0.07f * s, tx, ty - 0.2f * s, tx + 0.05f * s, ty + 0.55f * s, tx - 0.25f * s, ty + 0.85f * s);
                p.Line(Rgba.Hex("7A4A30"), 0.05f * s, tx + 0.05f * s, ty + 0.55f * s, tx + 0.35f * s, ty + 0.85f * s);
                p.Fill(Sd.Union(Sd.Ellipse(tx - 0.05f * s, ty + 0.95f * s, 0.7f * s, 0.16f * s), Sd.Ellipse(tx + 0.3f * s, ty + 1.02f * s, 0.4f * s, 0.12f * s)), Rgba.Hex("6F7B3A"));
            }
            // il prato
            p.FillFunc(Sd.Box(0f, -4.1f, 5f, 2.95f), (x, y) => Rgba.Lerp(Rgba.Hex("D9C26A"), Rgba.Hex("B49C48"), Math.Max(0f, Math.Min(1f, (-1.2f - y) / 3.5f))));
            p.Line(Rgba.Hex("2B2233", 0.45f), 0.03f, -4.5f, -1.15f, 4.5f, -1.15f);
            var rnd = new Random(76);
            for (int i = 0; i < 46; i++)
            {
                float x = (float)rnd.NextDouble() * 6f - 3f, y = -1.4f - (float)rnd.NextDouble() * 3.4f;
                if (Math.Abs(x) < 1.4f && y > -2.8f && y < -1.3f) continue;   // dietro al leone non serve
                float h = 0.08f + 0.06f * (float)rnd.NextDouble();
                Rgba c = Rgba.Hex("8E7A32", 0.75f);
                p.Line(c, 0.022f, x - 0.05f, y, x - 0.08f, y + h);
                p.Line(c, 0.022f, x, y, x, y + h * 1.3f);
                p.Line(c, 0.022f, x + 0.05f, y, x + 0.09f, y + h);
            }
            p.Fill(Sd.Ellipse(0f, -2.47f, 1.6f, 0.16f), Rgba.Hex("8E7A32", 0.25f));
            return new DoodleArt("v76_b_savanna_bg", p);
        }

        /// <summary>
        /// Il costume da leone seduto (origine a terra al centro): le zampe posteriori, il petto con la cerniera, le zampe
        /// anteriori con gli artigli finti, la coda col ciuffo e, in cima, l'apertura del collo (in y 2.0) da cui spunta la
        /// testolina del gattino. La criniera e il muso sono oggetti a parte.
        /// </summary>
        public static DoodleArt V76B_LionBody()
        {
            var p = new DoodlePainter(-1.4f, -0.2f, 1.75f, 2.2f, 100) { Ink = 0.05f };
            p.Fill(Sd.Ellipse(0.1f, 0.02f, 1.35f, 0.13f), new Rgba(0f, 0f, 0f, 0.18f));
            // la coda
            p.Line(V76BFurShade, 0.16f, 0.85f, 0.3f, 1.3f, 0.45f, 1.5f, 0.85f, 1.42f, 1.25f);
            p.InkLine(0.035f, 0.85f, 0.38f, 1.27f, 0.54f, 1.42f, 0.86f, 1.36f, 1.2f);
            Sdf tuft = Sd.Union(Sd.Circle(1.4f, 1.4f, 0.16f), Sd.Circle(1.5f, 1.5f, 0.12f), Sd.Circle(1.3f, 1.52f, 0.11f));
            p.Shape(tuft, V76BManeShade, 0.035f);
            // le zampe posteriori (cosce)
            Sdf hips = Sd.Union(Sd.Ellipse(-0.62f, 0.5f, 0.55f, 0.48f), Sd.Ellipse(0.62f, 0.5f, 0.55f, 0.48f));
            Body(p, hips, V76BFur, V76BFurShade);
            p.Shape(Sd.Ellipse(-0.9f, 0.08f, 0.35f, 0.12f), V76BFur, 0.04f);
            p.Shape(Sd.Ellipse(0.9f, 0.08f, 0.35f, 0.12f), V76BFur, 0.04f);
            // il petto
            Sdf chest = Sd.Union(Sd.Ellipse(0f, 1.2f, 0.72f, 0.88f), Sd.Box(0f, 0.6f, 0.62f, 0.4f, 0.2f));
            Body(p, chest, V76BFur, V76BFurShade);
            p.Fill(Sd.Ellipse(0f, 1.15f, 0.42f, 0.62f), V76BFurLight);
            // la cerniera (indizio: è un costume)
            p.Line(Rgba.Hex("6B5A4A"), 0.05f, 0f, 1.95f, 0f, 0.75f);
            for (int i = 0; i < 12; i++)
            {
                float y = 1.9f - i * 0.1f;
                p.Line(Rgba.Hex("C9C2B4"), 0.025f, -0.05f, y, 0.05f, y - 0.03f);
            }
            p.Shape(Sd.Box(0.07f, 1.62f, 0.05f, 0.09f, 0.03f), Rgba.Hex("C9C2B4"), 0.025f);
            p.Shape(Sd.Circle(0.07f, 1.52f, 0.03f), Rgba.Hex("8F877A"), 0.015f);
            // le zampe anteriori con le zampone e gli artigli finti (di plastica)
            foreach (float s in new[] { -1f, 1f })
            {
                Sdf leg = Sd.Box(s * 0.36f, 0.48f, 0.19f, 0.42f, 0.14f);
                Body(p, leg, V76BFur, V76BFurShade);
                Sdf paw = Sd.Ellipse(s * 0.4f, 0.12f, 0.3f, 0.15f);
                Body(p, paw, V76BFurLight, V76BFurShade);
                for (int k = -1; k <= 1; k++)
                {
                    float cx = s * 0.4f + k * 0.12f;
                    p.InkLine(0.025f, cx, 0.2f, cx, 0.06f);
                    p.Shape(Sd.Polygon(cx - 0.035f, 0.0f, cx + 0.035f, 0.0f, cx, -0.07f), Rgba.White, 0.02f);
                }
            }
            // l'apertura del collo (si vede solo quando la criniera non c'è)
            p.Shape(Sd.Ellipse(0f, 2.0f, 0.45f, 0.12f), Rgba.Hex("6E3E1C"), 0.04f);
            return new DoodleArt("v76_b_lion_body", p);
        }

        /// <summary>
        /// La criniera esagerata (origine al centro della testa): due giri di ciocche a punta attorno a un disco, e
        /// l'elastico nero che la tiene sotto il mento (indizio). Il muso è un disegno a parte, sopra.
        /// </summary>
        public static DoodleArt V76B_Mane()
        {
            var p = new DoodlePainter(-1.45f, -1.45f, 1.45f, 1.45f, 100) { Ink = 0.05f };
            var tufts = new List<Sdf> { Sd.Circle(0f, 0f, 1.02f) };
            for (int k = 0; k < 16; k++)
            {
                double a = k * Math.PI * 2 / 16 + 0.1;
                float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
                float tx = c * 1.32f, ty = s * 1.32f, bx = -s * 0.24f, by = c * 0.24f;
                tufts.Add(Sd.Polygon(c * 0.85f + bx, s * 0.85f + by, tx, ty, c * 0.85f - bx, s * 0.85f - by));
            }
            Sdf outer = Sd.Union(tufts.ToArray());
            Body(p, outer, V76BMane, V76BManeShade);
            var inner = new List<Sdf> { Sd.Circle(0f, 0f, 0.8f) };
            for (int k = 0; k < 13; k++)
            {
                double a = k * Math.PI * 2 / 13 + 0.35;
                float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
                float bx = -s * 0.2f, by = c * 0.2f;
                inner.Add(Sd.Polygon(c * 0.7f + bx, s * 0.7f + by, c * 1.04f, s * 1.04f, c * 0.7f - bx, s * 0.7f - by));
            }
            Sdf innerS = Sd.Union(inner.ToArray());
            p.Fill(innerS, V76BManeInner);
            p.Shape(innerS, Rgba.Clear, 0.035f);
            // ciocche disegnate
            for (int k = 0; k < 10; k++)
            {
                double a = k * Math.PI * 2 / 10 + 0.25;
                float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
                p.Line(V76BManeShade, 0.03f, c * 0.86f, s * 0.86f, c * 1.12f, s * 1.12f);
            }
            // l'elastico sotto il mento
            p.Line(Rgba.Hex("2E2633"), 0.07f, -0.8f, -0.55f, -0.4f, -0.82f, 0.4f, -0.82f, 0.8f, -0.55f);
            p.Line(Rgba.White.WithAlpha(0.35f), 0.018f, -0.7f, -0.6f, -0.38f, -0.8f, 0.38f, -0.8f);
            return new DoodleArt("v76_b_mane", p);
        }

        /// <summary>
        /// Il muso feroce (origine al centro): sopracciglia arrabbiate, occhi gialli a fessura, il nasone, i baffi, e la
        /// bocca coi denti aguzzi (chiusa in un ghigno, oppure spalancata nel ruggito).
        /// </summary>
        public static DoodleArt V76B_Face(bool roar)
        {
            var p = new DoodlePainter(-0.85f, -0.95f, 0.85f, 0.85f, 120) { Ink = 0.045f };
            // orecchie
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Circle(s * 0.52f, 0.52f, 0.19f), V76BFur, 0.04f);
                p.Fill(Sd.Circle(s * 0.52f, 0.52f, 0.1f), Rgba.Hex("C96A5A"));
            }
            Sdf head = Sd.Ellipse(0f, 0f, 0.68f, 0.66f);
            Body(p, head, V76BFur, V76BFurShade);
            // sopracciglia arrabbiate
            float browTilt = roar ? 0.2f : 0.14f;
            foreach (float s in new[] { -1f, 1f })
                p.Shape(Sd.Polygon(s * 0.52f, 0.36f + browTilt * 0.4f, s * 0.08f, 0.2f - browTilt * 0.3f, s * 0.1f, 0.12f - browTilt * 0.3f, s * 0.5f, 0.26f + browTilt * 0.3f), V76BManeShade, 0.03f);
            // occhi gialli a fessura
            float eh = roar ? 0.1f : 0.075f;
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Ellipse(s * 0.27f, 0.08f, 0.13f, eh), Rgba.Hex("FFD42E"), 0.03f);
                p.Fill(Sd.Ellipse(s * 0.25f, 0.08f, 0.025f, eh * 0.9f), Rgba.Hex("231A22"));
            }
            // il muso
            float my = roar ? -0.12f : -0.24f;
            p.Shape(Sd.Union(Sd.Ellipse(-0.15f, my, 0.22f, 0.16f), Sd.Ellipse(0.15f, my, 0.22f, 0.16f)), V76BMuzzle, 0.035f);
            foreach (float s in new[] { -1f, 1f })
            {
                for (int k = 0; k < 3; k++) p.Fill(Sd.Circle(s * (0.1f + k * 0.08f), my - 0.02f + (k % 2) * 0.05f, 0.017f), Rgba.Hex("8A6A4A"));
                p.InkLine(0.018f, s * 0.3f, my + 0.02f, s * 0.72f, my + 0.1f);
                p.InkLine(0.018f, s * 0.3f, my - 0.04f, s * 0.74f, my - 0.08f);
            }
            p.Shape(Sd.Polygon(-0.13f, my + 0.18f, 0.13f, my + 0.18f, 0f, my + 0.04f), V76BNose, 0.03f);
            if (!roar)
            {
                // ghigno chiuso coi denti aguzzi
                float top = -0.42f, bot = -0.54f;
                Sdf mouth = Sd.Polygon(-0.4f, top + 0.03f, 0.4f, top + 0.03f, 0.28f, bot, -0.28f, bot);
                p.Shape(mouth, V76BMouth, 0.035f);
                foreach (float x in new[] { -0.3f, -0.1f, 0.1f, 0.3f })
                {
                    float big = Math.Abs(x) > 0.2f ? 0.14f : 0.08f;
                    p.Shape(Sd.Polygon(x - 0.055f, top + 0.03f, x + 0.055f, top + 0.03f, x, top - big), Rgba.White, 0.022f);
                }
                foreach (float x in new[] { -0.18f, 0f, 0.18f })
                    p.Shape(Sd.Polygon(x - 0.045f, bot, x + 0.045f, bot, x, bot + 0.07f), Rgba.White, 0.02f);
            }
            else
            {
                // bocca spalancata: lingua, zanne in alto e in basso
                Sdf mouth = Sd.Ellipse(0f, -0.5f, 0.4f, 0.32f);
                p.Shape(mouth, V76BMouth, 0.04f);
                p.Fill(Sd.Intersect(Sd.Ellipse(0f, -0.72f, 0.26f, 0.14f), mouth), Rgba.Hex("E46A7C"));
                foreach (float x in new[] { -0.24f, 0.24f })
                {
                    p.Shape(Sd.Polygon(x - 0.07f, -0.24f, x + 0.07f, -0.24f, x, -0.46f), Rgba.White, 0.022f);
                    p.Shape(Sd.Polygon(x - 0.06f, -0.77f, x + 0.06f, -0.77f, x, -0.6f), Rgba.White, 0.022f);
                }
                foreach (float x in new[] { -0.09f, 0.09f })
                    p.Shape(Sd.Polygon(x - 0.045f, -0.2f, x + 0.045f, -0.2f, x, -0.31f), Rgba.White, 0.02f);
            }
            return new DoodleArt(roar ? "v76_b_face_roar" : "v76_b_face", p);
        }

        /// <summary>L'etichetta di carta appesa a un filo (origine dove il filo è attaccato; la scritta è un TextMesh in 0.2,-0.4).</summary>
        public static DoodleArt V76B_Tag()
        {
            var p = new DoodlePainter(-0.12f, -0.62f, 0.55f, 0.08f, 150) { Ink = 0.03f };
            p.InkLine(0.018f, 0f, 0f, 0.06f, -0.2f, 0.09f, -0.29f);
            Sdf tag = Sd.Rotate(Sd.Polygon(-0.04f, -0.4f, 0.06f, -0.32f, 0.44f, -0.32f, 0.44f, -0.48f, 0.06f, -0.48f), 0.2f, -0.4f, -18f);
            p.Shape(tag, Rgba.Hex("FFF4D6"), 0.03f);
            p.Shape(Sd.Circle(0.09f, -0.31f, 0.022f), Rgba.Hex("C9B98F"), 0.015f);
            return new DoodleArt("v76_b_tag", p);
        }

        /// <summary>Il bottone ROAR (origine al centro): tasto rosso bombato sulla sua base (la scritta è un TextMesh).</summary>
        public static DoodleArt V76B_RoarButton()
        {
            var p = new DoodlePainter(-0.95f, -0.5f, 0.95f, 0.45f, 120) { Ink = 0.05f };
            p.Fill(Sd.Box(0.03f, -0.13f, 0.86f, 0.32f, 0.3f), new Rgba(0f, 0f, 0f, 0.2f));
            p.Shape(Sd.Box(0f, -0.1f, 0.85f, 0.32f, 0.3f), Rgba.Hex("9E2420"), 0.045f);
            Sdf top = Sd.Box(0f, 0.02f, 0.82f, 0.3f, 0.28f);
            Body(p, top, Rgba.Hex("E8433F"), Rgba.Hex("C02E2B"));
            p.Fill(Sd.Box(-0.35f, 0.18f, 0.3f, 0.04f, 0.04f), Rgba.White.WithAlpha(0.45f));
            return new DoodleArt("v76_b_roar_btn", p);
        }

        static void V76B_KittenFace(DoodlePainter p, float cx, float cy, float r, bool scared)
        {
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Polygon(cx + s * r * 0.95f, cy + r * 0.25f, cx + s * r * 0.85f, cy + r * 1.35f, cx + s * r * 0.25f, cy + r * 0.85f), V76BKitten, 0.03f);
                p.Fill(Sd.Polygon(cx + s * r * 0.8f, cy + r * 0.45f, cx + s * r * 0.78f, cy + r * 1.1f, cx + s * r * 0.4f, cy + r * 0.8f), V76BPink);
            }
            Sdf head = Sd.Ellipse(cx, cy, r, r * 0.9f);
            Body(p, head, V76BKitten, V76BKittenShade, null, -0.03f, 0.04f);
            foreach (float dx in new[] { -0.2f, 0f, 0.2f }) p.Line(V76BKittenStripe, 0.03f, cx + dx * r, cy + r * 0.85f, cx + dx * r * 0.8f, cy + r * 0.55f);
            float er = scared ? r * 0.3f : r * 0.26f, ey = cy + r * 0.1f;
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Circle(cx + s * r * 0.4f, ey, er), Rgba.White, 0.025f);
                p.Fill(Sd.Circle(cx + s * r * 0.4f, ey, scared ? er * 0.25f : er * 0.6f), Rgba.Hex("231A22"));
                if (!scared) p.Fill(Sd.Circle(cx + s * r * 0.4f - er * 0.2f, ey + er * 0.25f, er * 0.2f), Rgba.White);
                p.InkLine(0.014f, cx + s * r * 0.45f, cy - r * 0.3f, cx + s * r * 1.25f, cy - r * 0.2f);
                p.InkLine(0.014f, cx + s * r * 0.45f, cy - r * 0.4f, cx + s * r * 1.25f, cy - r * 0.5f);
            }
            p.Shape(Sd.Polygon(cx - r * 0.12f, cy - r * 0.18f, cx + r * 0.12f, cy - r * 0.18f, cx, cy - r * 0.32f), V76BPink, 0.02f);
            if (scared) p.Shape(Sd.Ellipse(cx, cy - r * 0.55f, r * 0.13f, r * 0.17f), V76BMouth, 0.02f);
            else p.InkLine(0.018f, cx - r * 0.2f, cy - r * 0.45f, cx - r * 0.1f, cy - r * 0.52f, cx, cy - r * 0.42f, cx + r * 0.1f, cy - r * 0.52f, cx + r * 0.2f, cy - r * 0.45f);
        }

        /// <summary>La testolina del gattino nascosta nel costume (origine al centro della testa, raggio 0.3).</summary>
        public static DoodleArt V76B_KittenHead()
        {
            var p = new DoodlePainter(-0.45f, -0.35f, 0.45f, 0.48f, 150) { Ink = 0.035f };
            V76B_KittenFace(p, 0f, 0f, 0.3f, false);
            return new DoodleArt("v76_b_kitten_head", p);
        }

        /// <summary>Il gattino spaventato dal proprio miagolio (origine ai piedi): pelo gonfio a punte, coda dritta, occhi sgranati.</summary>
        public static DoodleArt V76B_KittenScared()
        {
            var p = new DoodlePainter(-0.75f, -0.08f, 0.75f, 1.25f, 140) { Ink = 0.035f };
            // la coda dritta e arruffata
            var tail = new List<Sdf> { Sd.Capsule(-0.32f, 0.5f, -0.45f, 1.08f, 0.06f) };
            for (int k = 0; k < 6; k++)
            {
                float t = k / 5f, x = -0.32f - 0.13f * t, y = 0.55f + 0.5f * t;
                tail.Add(Sd.Polygon(x - 0.06f, y, x - 0.16f, y + 0.06f, x - 0.02f, y + 0.06f));
                tail.Add(Sd.Polygon(x + 0.04f, y, x + 0.14f, y + 0.08f, x + 0.03f, y + 0.07f));
            }
            Body(p, Sd.Union(tail.ToArray()), V76BKitten, V76BKittenShade);
            // le zampe rigide
            foreach (float x in new[] { -0.28f, -0.12f, 0.12f, 0.28f })
                Body(p, Sd.Box(x, 0.2f, 0.045f, 0.2f, 0.04f), V76BKitten, V76BKittenShade);
            // il corpo gonfio, a punte
            var fur = new List<Sdf> { Sd.Ellipse(0f, 0.48f, 0.38f, 0.24f) };
            for (int k = 0; k < 18; k++)
            {
                double a = k * Math.PI * 2 / 18;
                float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
                float ox = c * 0.38f, oy = 0.48f + s * 0.24f;
                fur.Add(Sd.Polygon(ox - s * 0.06f, oy + c * 0.04f, ox + c * 0.13f, oy + s * 0.13f, ox + s * 0.06f, oy - c * 0.04f));
            }
            Body(p, Sd.Union(fur.ToArray()), V76BKitten, V76BKittenShade);
            V76B_KittenFace(p, 0.3f, 0.82f, 0.24f, true);
            // il "!" di spavento
            p.Shape(Sd.Box(0.62f, 1.08f, 0.035f, 0.1f, 0.03f), Rgba.Hex("E8433F"), 0.02f);
            p.Shape(Sd.Circle(0.62f, 0.9f, 0.035f), Rgba.Hex("E8433F"), 0.02f);
            return new DoodleArt("v76_b_kitten_scared", p);
        }

        /// <summary>Il gattino che scappa verso destra (origine ai piedi): corpo allungato, zampe tese, coda dietro, goccia di sudore.</summary>
        public static DoodleArt V76B_KittenRun()
        {
            var p = new DoodlePainter(-0.95f, -0.08f, 0.75f, 0.8f, 140) { Ink = 0.035f };
            // linee di velocità
            foreach (float y in new[] { 0.2f, 0.36f, 0.52f }) p.Line(Rgba.Hex("2B2233", 0.5f), 0.025f, -0.92f, y, -0.62f, y);
            Body(p, Sd.Capsule(-0.4f, 0.4f, -0.8f, 0.5f, 0.05f), V76BKitten, V76BKittenShade);
            Body(p, Sd.Capsule(-0.25f, 0.28f, -0.55f, 0.05f, 0.045f), V76BKitten, V76BKittenShade);
            Body(p, Sd.Capsule(0.22f, 0.28f, 0.5f, 0.06f, 0.045f), V76BKitten, V76BKittenShade);
            Body(p, Sd.Ellipse(-0.05f, 0.34f, 0.38f, 0.16f), V76BKitten, V76BKittenShade);
            foreach (float dx in new[] { -0.2f, -0.05f, 0.1f }) p.Line(V76BKittenStripe, 0.028f, dx, 0.49f, dx - 0.03f, 0.4f);
            V76B_KittenFace(p, 0.4f, 0.48f, 0.2f, true);
            p.Shape(Sd.Union(Sd.Circle(0.12f, 0.66f, 0.04f), Sd.Polygon(0.085f, 0.67f, 0.155f, 0.67f, 0.12f, 0.76f)), Rgba.Hex("8FD3F5"), 0.018f);
            return new DoodleArt("v76_b_kitten_run", p);
        }
    }
}
