using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I pochi disegni nuovi della 0.64 per i livelli 251, 253 e 254 (tutto il resto riusa gli sprite esistenti: il
    /// telefono, i tasti, l'impronta, il pesce, il secchio, la pompa, la camera da letto, la sveglia, la biglia...):
    /// la cornice della foto e la mano alzata del proprietario (il resto della foto è "guy_relieved"), la boccia tonda col
    /// collo (dietro, davanti e maschera per l'acqua), la valvolina chiusa e aperta, il gallo (la chioccia rivisitata),
    /// lo spioncino sul piano di sotto e il pulsante del microfono per chi non può soffiare.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV64A() => new DoodleArt[]
        {
            V64_Photo(), V64_PhotoHand(), V64_BowlBack(), V64_BowlFront(), V64_BowlMask(), V64_Valve(false), V64_Valve(true),
            V64_Rooster(), V64_Peephole(), V64_MicButton(),
        };

        // ---------- 251: la foto del proprietario ----------

        /// <summary>Punta dell'indice della mano nella foto (coordinate della foto: la foto è centrata in 0,0).</summary>
        public const float V64PhotoTipX = 0.31f, V64PhotoTipY = 0.6f;

        /// <summary>Foto istantanea con la cornice bianca (centro 0,0; 1.5 x 1.8). La figura va sopra, nel riquadro.</summary>
        public static DoodleArt V64_Photo()
        {
            var p = new DoodlePainter(-0.82f, -0.97f, 0.82f, 0.97f, 120) { Ink = 0.04f, Wobble = 0.006f };
            p.Shape(Sd.Box(0f, 0f, 0.75f, 0.9f, 0.04f), Rgba.Hex("FFFDF6"));
            Sdf pic = Sd.Box(0f, 0.12f, 0.64f, 0.68f, 0.02f);
            p.FillFunc(pic, (x, y) => y > -0.25f ? Rgba.Lerp(Rgba.Hex("BFE3F7"), Rgba.Hex("8FCBF0"), Clamp((y + 0.25f) / 1f)) : Rgba.Hex("9BD07A"));
            p.Fill(Sd.Circle(-0.42f, 0.6f, 0.11f), Rgba.Hex("FFD24A"));
            p.Fill(Sd.Intersect(pic, Sd.Ellipse(0.1f, -0.25f, 0.8f, 0.08f)), Rgba.Hex("86BF67"));
            p.Shape(pic, Rgba.Clear, 0.025f);
            // un pezzetto di nastro adesivo in alto
            p.Fill(Sd.Rotate(Sd.Box(0f, 0.9f, 0.22f, 0.07f), 0f, 0.9f, -6f), Rgba.Hex("F4E9B8", 0.85f));
            return new DoodleArt("v64_photo", p);
        }

        /// <summary>
        /// La mano alzata del proprietario, nella foto (coordinate della foto): avambraccio con la manica blu di
        /// "guy_relieved", palmo e cinque dita ben separate; sul polpastrello dell'indice si vedono le creste.
        /// </summary>
        public static DoodleArt V64_PhotoHand()
        {
            var p = new DoodlePainter(0.0f, -0.62f, 0.66f, 0.72f, 160) { Ink = 0.03f, Wobble = 0.004f };
            Rgba skin = Rgba.Hex("FFD3A8"), skinShade = Rgba.Hex("EDB386"), sleeve = Rgba.Hex("7FA8F0"), sleeveShade = Rgba.Hex("5F86D6");
            Body(p, Sd.Capsule(0.28f, -0.6f, 0.36f, -0.05f, 0.09f), sleeve, sleeveShade);
            Sdf fingers = Sd.Union(
                Sd.Capsule(0.25f, 0.24f, 0.2f, 0.38f, 0.045f),                    // pollice
                Sd.Capsule(0.31f, 0.3f, V64PhotoTipX, V64PhotoTipY - 0.03f, 0.042f), // indice
                Sd.Capsule(0.38f, 0.3f, 0.395f, 0.62f, 0.042f),                   // medio
                Sd.Capsule(0.44f, 0.29f, 0.47f, 0.57f, 0.04f),                    // anulare
                Sd.Capsule(0.49f, 0.25f, 0.54f, 0.46f, 0.036f));                  // mignolo
            Sdf palm = Sd.Union(Sd.Ellipse(0.38f, 0.17f, 0.14f, 0.15f), Sd.Box(0.37f, 0.02f, 0.08f, 0.08f));
            Body(p, Sd.Union(fingers, palm), skin, skinShade);
            p.InkLine(0.012f, 0.33f, 0.22f, 0.36f, 0.27f);
            // creste dell'impronta sul polpastrello dell'indice
            for (int i = 1; i <= 3; i++)
                p.Shape(Sd.Ellipse(V64PhotoTipX, V64PhotoTipY - 0.02f, 0.011f * i, 0.014f * i), Rgba.Clear, 0.006f);
            return new DoodleArt("v64_photo_hand", p);
        }

        // ---------- 253: la boccia ----------

        const float V64BowlR = 1.5f, V64NeckHalf = 0.55f, V64NeckTop = 1.78f;

        static Sdf V64BowlInside() => Sd.Union(Sd.Circle(0f, 0f, V64BowlR), Sd.Box(0f, 1.45f, V64NeckHalf, V64NeckTop - 1.45f));

        /// <summary>Fondo della boccia (vetro azzurrino trasparente e base), centro della sfera in 0,0, raggio interno 1.5.</summary>
        public static DoodleArt V64_BowlBack()
        {
            var p = new DoodlePainter(-1.7f, -1.82f, 1.7f, 1.95f, 80) { Ink = 0.05f };
            p.Shape(Sd.Box(0f, -1.6f, 0.75f, 0.14f, 0.07f), Rgba.Hex("B7DCEB"));
            p.Fill(V64BowlInside(), Rgba.Hex("D8F1FB", 0.35f));
            return new DoodleArt("v64_bowl_back", p);
        }

        /// <summary>Vetro della boccia davanti all'acqua: contorno, labbro del collo e riflessi (dentro è trasparente).</summary>
        public static DoodleArt V64_BowlFront()
        {
            var p = new DoodlePainter(-1.7f, -1.82f, 1.7f, 1.95f, 80) { Ink = 0.05f };
            Sdf glass = Sd.Subtract(Sd.Grow(V64BowlInside(), 0.06f), Sd.Union(V64BowlInside(), Sd.Box(0f, 1.9f, V64NeckHalf, 0.3f)));
            p.Fill(glass, Rgba.Hex("CFEAF5", 0.75f));
            p.Shape(Sd.Grow(V64BowlInside(), 0.06f), Rgba.Clear);
            p.Shape(Sd.Box(0f, V64NeckTop, V64NeckHalf + 0.12f, 0.07f, 0.06f), Rgba.Hex("E4F5FB", 0.9f), 0.035f);
            // riflessi
            p.Line(Rgba.White.WithAlpha(0.8f), 0.07f, -1.15f, 0.55f, -1.25f, 0.2f, -1.25f, -0.15f);
            p.Line(Rgba.White.WithAlpha(0.6f), 0.05f, -1.0f, 0.85f, -0.85f, 1.02f);
            p.Line(Rgba.White.WithAlpha(0.45f), 0.05f, 1.15f, -0.6f, 1.05f, -0.85f);
            return new DoodleArt("v64_bowl_front", p);
        }

        /// <summary>Maschera dell'interno della boccia (sfera più collo), per l'acqua e le bolle.</summary>
        public static DoodleArt V64_BowlMask()
        {
            var p = new DoodlePainter(-1.7f, -1.82f, 1.7f, 1.95f, 80) { Ink = 0.05f };
            p.Fill(V64BowlInside(), Rgba.White, false);
            return new DoodleArt("v64_bowl_mask", p);
        }

        /// <summary>Valvolina da gonfiabile (base in 0,0, alta 0.12): chiusa col tappo, oppure aperta col buco.</summary>
        public static DoodleArt V64_Valve(bool open)
        {
            var p = new DoodlePainter(-0.1f, -0.03f, 0.1f, 0.16f, 400) { Ink = 0.014f, Wobble = 0.002f };
            Rgba plastic = Rgba.Hex("FFF3C9"), shade = Rgba.Hex("E8CF8A");
            Body(p, Sd.Box(0f, 0.035f, 0.03f, 0.035f, 0.01f), plastic, shade);
            if (open)
            {
                Body(p, Sd.Ellipse(0f, 0.085f, 0.055f, 0.03f), plastic, shade);
                p.Fill(Sd.Ellipse(0f, 0.09f, 0.03f, 0.016f), Rgba.Hex("3A2B2B"));
            }
            else
            {
                Body(p, Sd.Box(0f, 0.09f, 0.042f, 0.022f, 0.012f), Rgba.Hex("FF9F43"), Rgba.Hex("E07B22"));
            }
            return new DoodleArt(open ? "v64_valve_open" : "v64_valve", p);
        }

        // ---------- 254: il gallo, lo spioncino, il microfono ----------

        /// <summary>Gallo di fronte, a becco spalancato (la chioccia di "mother_hen" con cresta, bargigli e coda colorata).</summary>
        public static DoodleArt V64_Rooster()
        {
            var p = new DoodlePainter(-0.85f, -0.62f, 0.85f, 0.95f, 150) { Ink = 0.04f };
            Rgba white = Rgba.Hex("FFFDF6"), shade = Rgba.Hex("DAD3C4"), red = Rgba.Hex("E8443A"), beak = Rgba.Hex("F5A623");
            // coda a ventaglio dietro
            Rgba[] tail = { Rgba.Hex("2E8B57"), Rgba.Hex("D9822B"), Rgba.Hex("3A5BA0"), Rgba.Hex("D9822B"), Rgba.Hex("2E8B57") };
            for (int i = 0; i < tail.Length; i++)
            {
                float a = (float)((150 - i * 30) * Math.PI / 180.0);
                float tx = (float)Math.Cos(a) * 0.62f, ty = 0.05f + (float)Math.Sin(a) * 0.5f;
                p.Shape(Sd.Capsule(0f, -0.1f, tx, ty, 0.09f), tail[i], 0.03f);
            }
            foreach (float s in new[] { -1f, 1f })
                p.Line(beak, 0.045f, s * 0.15f, -0.4f, s * 0.15f, -0.53f, s * 0.07f, -0.58f, s * 0.15f, -0.53f, s * 0.25f, -0.58f);
            foreach (float s in new[] { -1f, 1f })
                Body(p, Sd.Rotate(Sd.Ellipse(s * 0.45f, -0.12f, 0.14f, 0.26f), s * 0.45f, -0.12f, s * 35f), white, shade);
            Body(p, Sd.Ellipse(0, -0.12f, 0.45f, 0.32f), white, shade);
            // cresta grande
            p.Shape(Sd.Union(Sd.Circle(-0.14f, 0.62f, 0.09f), Sd.Circle(-0.02f, 0.7f, 0.11f), Sd.Circle(0.11f, 0.66f, 0.1f), Sd.Circle(0.2f, 0.56f, 0.08f)), red, 0.03f);
            Sdf head = Sd.Ellipse(0, 0.33f, 0.26f, 0.27f);
            Body(p, head, white, shade);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Circle(s * 0.1f, 0.4f, 0.05f), Ink);
                p.Fill(Sd.Circle(s * 0.1f - 0.015f, 0.415f, 0.016f), Rgba.White);
                p.InkLine(0.022f, s * 0.04f, 0.5f, s * 0.17f, 0.47f);   // sopracciglia arrabbiate
            }
            // becco spalancato e bargigli
            p.Shape(Sd.Polygon(-0.08f, 0.3f, 0.08f, 0.3f, 0f, 0.22f), beak, 0.022f);
            p.Shape(Sd.Polygon(-0.07f, 0.2f, 0.07f, 0.2f, 0f, 0.1f), beak, 0.022f);
            p.Fill(Sd.Polygon(-0.06f, 0.215f, 0.06f, 0.215f, 0f, 0.17f), Rgba.Hex("8A1F1F"));
            p.Shape(Sd.Union(Sd.Ellipse(-0.04f, 0.04f, 0.04f, 0.07f), Sd.Ellipse(0.04f, 0.04f, 0.04f, 0.07f)), red, 0.02f);
            return new DoodleArt("v64_rooster", p);
        }

        /// <summary>
        /// Spioncino sul piano di sotto (centro 0,0, raggio 1): il bordo frastagliato delle assi del pavimento e, dentro,
        /// la penombra della camera del vicino con il piano del suo comodino in basso (y -0.45).
        /// </summary>
        public static DoodleArt V64_Peephole()
        {
            var p = new DoodlePainter(-1.12f, -1.12f, 1.12f, 1.12f, 90) { Ink = 0.05f };
            Sdf hole = Sd.Circle(0f, 0f, 0.95f);
            p.FillFunc(hole, (x, y) => Rgba.Lerp(Rgba.Hex("1B1630"), Rgba.Hex("3A3160"), Clamp((y + 0.9f) / 1.8f)));
            p.Fill(Sd.Intersect(hole, Sd.Box(0f, -0.7f, 1f, 0.25f)), Rgba.Hex("6B4A34"));
            p.Fill(Sd.Intersect(hole, Sd.Box(0f, -0.47f, 1f, 0.03f)), Rgba.Hex("8A6244"));
            Sdf rim = Sd.Subtract(Sd.Circle(0f, 0f, 1.08f), hole);
            p.FillFunc(rim, (x, y) => Rgba.Lerp(Rgba.Hex("5C3D2C"), Rgba.Hex("7A5238"), Clamp(0.5f + Noise.Value(x * 6f, y * 6f) * 0.5f)));
            for (int i = 0; i < 10; i++)
            {
                float a = (float)(i * Math.PI / 5.0);
                float cx = (float)Math.Cos(a), sy = (float)Math.Sin(a);
                p.InkLine(0.025f, cx * 0.95f, sy * 0.95f, cx * 1.06f, sy * 1.06f);
            }
            p.Shape(Sd.Circle(0f, 0f, 1.08f), Rgba.Clear);
            p.Shape(hole, Rgba.Clear, 0.035f);
            return new DoodleArt("v64_peephole", p);
        }

        /// <summary>Pulsante tondo col microfono (centro 0,0, raggio 0.42): l'alternativa per chi non può usare il microfono.</summary>
        public static DoodleArt V64_MicButton()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.5f, 0.5f, 160) { Ink = 0.045f };
            Body(p, Sd.Circle(0f, 0f, 0.42f), Rgba.Hex("FFD24A"), Rgba.Hex("E8B32E"));
            p.Shape(Sd.Capsule(0f, 0.02f, 0f, 0.16f, 0.09f), Rgba.Hex("4A4458"), 0.03f);
            p.InkLine(0.035f, -0.15f, 0.06f, -0.14f, -0.04f, -0.06f, -0.11f, 0.06f, -0.11f, 0.14f, -0.04f, 0.15f, 0.06f);
            p.InkLine(0.035f, 0f, -0.11f, 0f, -0.2f);
            p.InkLine(0.035f, -0.09f, -0.21f, 0.09f, -0.21f);
            return new DoodleArt("v64_mic_btn", p);
        }
    }
}
