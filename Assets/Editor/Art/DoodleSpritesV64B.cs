using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// Disegni nuovi della 0.64 per i livelli 255 (The Invisible Password) e 256 (The Incompetent Magician).
    /// Il 256 riusa mago, coniglio, cilindro, bacchetta e palco di "The cheap magician" (s41b_*): qui solo le aggiunte
    /// (occhi chiusi, guanto, sopracciglia arrabbiate, zampa che indica, botola, scatole magiche, cartello, pubblico).
    /// Il 255 ha la porta blindata (davanti e retro), i cardini coi perni, lo sgabuzzino e il riflesso dello specchio.
    ///
    /// Le scritte del retro della porta sono disegnate apposta (non un semplice capovolgimento): sul retro c'è "0000"
    /// in vernice opaca nera (con un velo lucido quasi invisibile dentro il primo e l'ultimo zero, l'indizio);
    /// lo specchio prende la luce di sbieco e mostra solo l'inchiostro riflettente: "8008" luminoso. 8 e 0 sono
    /// simmetrici e 8008 è palindromo, quindi il riflesso si legge dritto, senza cifre rovesciate.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV64B() => new DoodleArt[]
        {
            V64_PwBackground(), V64_PwDoorFront(), V64_PwDoorBack(), V64_PwMirrorReflect(), V64_PwHinge(), V64_PwPin(),
            V64_PwCloset(), V64_PwMop(), V64_PwConsole(), V64_PwButton(), V64_PwSign(), V64_PwLamp(),
            V64_MgEyesClosed(), V64_MgGlove(), V64_MgRabbitBrows(), V64_MgRabbitPaw(), V64_MgTrapdoor(), V64_MgTrapHole(),
            V64_MgTrapGlow(), V64_MgBox(), V64_MgSign(), V64_MgCurtain(), V64_MgAudience(), V64_MgMicOff(),
        };

        // ---------- Aiuti per le cifre (simmetriche: si leggono uguali allo specchio) ----------

        /// <summary>Zero: anello ovale pieno (niente ponticelli: a porta inclinata sembrerebbero parentesi).</summary>
        static Sdf V64_Zero(float cx, float cy, float hx, float hy, float stroke)
        {
            return Sd.Subtract(Sd.Ellipse(cx, cy, hx, hy), Sd.Ellipse(cx, cy, hx - stroke, hy - stroke));
        }

        /// <summary>Otto: due anelli impilati (quello sopra un po' più piccolo), stesso ingombro dello zero.</summary>
        static Sdf V64_Eight(float cx, float cy, float hx, float hy, float stroke)
        {
            float uy = cy + hy * 0.47f, ury = hy * 0.53f, urx = hx * 0.84f;
            float ly = cy - hy * 0.43f, lry = hy * 0.57f, lrx = hx;
            Sdf up = Sd.Subtract(Sd.Ellipse(cx, uy, urx, ury), Sd.Ellipse(cx, uy, urx - stroke, ury - stroke));
            Sdf lo = Sd.Subtract(Sd.Ellipse(cx, ly, lrx, lry), Sd.Ellipse(cx, ly, lrx - stroke, lry - stroke));
            return Sd.Union(up, lo);
        }

        static Rgba V64_Hazard(float x, float y) =>
            ((int)Math.Floor((x + y) * 6f + 100f)) % 2 == 0 ? Rgba.Hex("F2C230") : Rgba.Hex("2B2233");

        // =====================================================================
        // 255 THE INVISIBLE PASSWORD
        // =====================================================================

        /// <summary>
        /// Corridoio di cemento (coordinate del livello): parete, pavimento sotto y -2.7, telaio d'acciaio della porta
        /// (vano x -1.55..0.55, y -2.7..1.0) e striscia di pericolo davanti alla soglia.
        /// </summary>
        public static DoodleArt V64_PwBackground()
        {
            var p = new DoodlePainter(-3.2f, -5.4f, 3.2f, 5.4f, 40) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.5f), (x, y) => Rgba.Lerp(Rgba.Hex("566173"), Rgba.Hex("3B4454"), Clamp((y + 2.7f) / 7f)));
            for (float x = -2.9f; x < 3.2f; x += 1.15f) p.Line(Rgba.Hex("000000", 0.12f), 0.03f, x, -2.7f, x, 5.4f);
            p.Line(Rgba.Hex("000000", 0.14f), 0.035f, -3.2f, 1.95f, 3.2f, 1.95f);
            for (int i = 0; i < 26; i++)   // macchioline del cemento
            {
                float x = -3f + (i * 1.37f) % 6f, y = -2.4f + (i * 2.71f) % 7.6f;
                p.Fill(Sd.Circle(x, y, 0.03f + (i % 3) * 0.015f), Rgba.Hex("000000", 0.1f));
            }
            // pavimento
            p.FillFunc(Sd.Box(0, -4.05f, 3.3f, 1.35f), (x, y) => Rgba.Lerp(Rgba.Hex("2A2F3A"), Rgba.Hex("3A404D"), Clamp((y + 5.4f) / 2.7f)));
            p.Line(Rgba.Hex("1C2028"), 0.05f, -3.2f, -2.7f, 3.2f, -2.7f);
            for (float x = -3.2f; x < 3.3f; x += 0.8f) p.Line(Rgba.Hex("000000", 0.18f), 0.025f, x * 0.55f - 0.2f, -2.7f, x * 1.2f - 0.3f, -5.4f);
            // telaio della porta
            Sdf frame = Sd.Subtract(Sd.Box(-0.5f, -0.77f, 1.23f, 1.93f, 0.03f), Sd.Box(-0.5f, -0.85f, 1.05f, 1.86f));
            Body(p, frame, Rgba.Hex("7C8799"), Rgba.Hex("5E6878"));
            for (float y = -2.4f; y < 1.1f; y += 0.55f)
            {
                p.Shape(Sd.Circle(-1.64f, y, 0.035f), Rgba.Hex("B7C0CC"), 0.015f);
                p.Shape(Sd.Circle(0.64f, y, 0.035f), Rgba.Hex("B7C0CC"), 0.015f);
            }
            // striscia di pericolo sulla soglia
            Sdf tape = Sd.Box(-0.5f, -2.88f, 1.2f, 0.11f);
            p.FillFunc(tape, V64_Hazard);
            p.Shape(tape, Rgba.Clear, 0.03f);
            return new DoodleArt("v64_pw_bg", p);
        }

        /// <summary>Porta blindata vista davanti (centro 0,0, larga 2.1, alta 3.7): niente maniglia, piastra della serratura a destra.</summary>
        public static DoodleArt V64_PwDoorFront()
        {
            var p = new DoodlePainter(-1.12f, -1.9f, 1.12f, 1.9f, 90) { Ink = 0.05f };
            Body(p, Sd.Box(0, 0, 1.05f, 1.85f, 0.03f), Rgba.Hex("9AA5B6"), Rgba.Hex("7C8799"), Rgba.Hex("6E7889"));
            p.Shape(Sd.Box(0, 0.9f, 0.78f, 0.6f, 0.05f), Rgba.Hex("8D98AA"), 0.03f);
            p.Shape(Sd.Box(0, -0.78f, 0.78f, 0.72f, 0.05f), Rgba.Hex("8D98AA"), 0.03f);
            p.Fill(Sd.Box(-0.05f, 0.95f, 0.66f, 0.04f), Rgba.White.WithAlpha(0.18f));
            p.Shape(Sd.Box(0, 0.08f, 1.0f, 0.07f), Rgba.Hex("6E7889"), 0.03f);
            foreach (float y in new[] { 1.72f, 0.08f, -1.5f })
                for (float x = -0.9f; x <= 0.91f; x += 0.3f)
                    p.Shape(Sd.Circle(x, y, 0.028f), Rgba.Hex("C9D1DC"), 0.012f);
            Sdf band = Sd.Box(0, -1.7f, 0.98f, 0.09f);
            p.FillFunc(band, V64_Hazard);
            p.Shape(band, Rgba.Clear, 0.025f);
            // piastra della serratura (lato destro)
            p.Shape(Sd.Box(0.93f, -0.15f, 0.08f, 0.32f, 0.02f), Rgba.Hex("5E6878"), 0.03f);
            p.Fill(Sd.Box(0.93f, -0.15f, 0.025f, 0.18f), Rgba.Hex("2B2233"));
            return new DoodleArt("v64_pw_door_front", p);
        }

        /// <summary>
        /// Retro della porta (centro 0,0; si vede quando la porta ruota oltre i 90° attorno alla serratura, che qui è a
        /// sinistra). Sulla targa, in vernice opaca: "0000". Dentro il primo e l'ultimo zero c'è un velo lucido appena
        /// visibile (la "vita" di un 8): è l'inchiostro riflettente che si legge solo nello specchio.
        /// </summary>
        public static DoodleArt V64_PwDoorBack()
        {
            var p = new DoodlePainter(-1.12f, -1.9f, 1.12f, 1.9f, 90) { Ink = 0.05f };
            Body(p, Sd.Box(0, 0, 1.05f, 1.85f, 0.03f), Rgba.Hex("B4BDC9"), Rgba.Hex("939DAD"));
            // telaio di rinforzo: traversi e diagonale in basso
            foreach (float y in new[] { 1.55f, -0.25f, -1.6f }) p.Shape(Sd.Box(0, y, 0.95f, 0.08f, 0.02f), Rgba.Hex("8792A3"), 0.03f);
            p.Shape(Sd.Intersect(Sd.Rotate(Sd.Box(0, -0.92f, 0.08f, 1.2f), 0, -0.92f, -52f), Sd.Box(0, -0.92f, 0.95f, 0.6f)), Rgba.Hex("8792A3"), 0.03f);
            // serratura a sinistra (la porta è vista da dietro), segni dei cardini strappati a destra
            p.Shape(Sd.Box(-0.97f, -0.15f, 0.06f, 0.26f, 0.02f), Rgba.Hex("6E7889"), 0.025f);
            foreach (float y in new[] { 1.3f, -1.3f })
            {
                p.Fill(Sd.Box(0.96f, y, 0.07f, 0.2f), Rgba.Hex("6B7383"));
                p.Shape(Sd.Circle(0.93f, y + 0.1f, 0.025f), Rgba.Hex("3B4252"), 0.01f);
                p.Shape(Sd.Circle(0.93f, y - 0.1f, 0.025f), Rgba.Hex("3B4252"), 0.01f);
            }
            // targa con il codice "0000"
            p.Shape(Sd.Box(0, 0.62f, 0.92f, 0.45f, 0.04f), Rgba.Hex("D3D9E1"), 0.03f);
            float[] xs = { -0.63f, -0.21f, 0.21f, 0.63f };
            const float hx = 0.18f, hy = 0.3f, st = 0.08f, cy = 0.62f;
            for (int i = 0; i < 4; i++)
            {
                p.Fill(V64_Zero(xs[i], cy, hx, hy, st), Rgba.Hex("26222C"));
                if (i == 0 || i == 3)
                {
                    // il velo lucido: solo la parte dell'8 che sta DENTRO lo zero, quasi invisibile
                    Sdf inside = Sd.Ellipse(xs[i], cy, hx - st, hy - st);
                    p.Fill(Sd.Intersect(V64_Eight(xs[i], cy, hx, hy, st * 0.8f), inside), Rgba.Hex("FFFFFF", 0.22f));
                }
            }
            p.Fill(Sd.Box(0.62f, 0.2f, 0.18f, 0.012f), Rgba.Hex("26222C", 0.5f));   // sbavatura di vernice
            return new DoodleArt("v64_pw_door_back", p);
        }

        /// <summary>
        /// Il riflesso nello specchio ovale "mirror" (stesse coordinate del vetro: ellisse 1.08 x 1.38, centro 0,0).
        /// Lo specchio vede il retro della porta di sbieco, in ombra: la vernice opaca non si vede, l'inchiostro
        /// riflettente sì: "8008" luminoso, cifre simmetriche (si leggono dritte anche riflesse).
        /// </summary>
        public static DoodleArt V64_PwMirrorReflect()
        {
            var p = new DoodlePainter(-1.1f, -1.4f, 1.1f, 1.4f, 110) { Ink = 0.04f };
            Sdf glass = Sd.Ellipse(0, 0, 1.08f, 1.38f);
            p.FillFunc(glass, (x, y) => Rgba.Lerp(Rgba.Hex("243145"), Rgba.Hex("44567A"), Clamp((y + 1.3f) / 2.6f)));
            // il retro della porta in prospettiva (riflesso: la serratura ora è a destra)
            Sdf door = Sd.Intersect(Sd.Polygon(-1.1f, 0.95f, 1.1f, 1.12f, 1.1f, -1.05f, -1.1f, -0.85f), glass);
            p.Fill(door, Rgba.Hex("5E6D86"));
            p.Fill(Sd.Intersect(Sd.Polygon(-1.1f, -0.55f, 1.1f, -0.68f, 1.1f, -0.82f, -1.1f, -0.68f), glass), Rgba.Hex("4A5870"));
            p.Fill(Sd.Intersect(Sd.Box(0.96f, -0.1f, 0.06f, 0.24f), glass), Rgba.Hex("3B4660"));
            Sdf plate = Sd.Box(0, 0.12f, 0.98f, 0.55f, 0.05f);
            p.Fill(plate, Rgba.Hex("6F7F9B"));
            float[] xs = { -0.72f, -0.24f, 0.24f, 0.72f };
            const float hx = 0.2f, hy = 0.38f, st = 0.085f, cy = 0.12f;
            Sdf code = Sd.Union(V64_Eight(xs[0], cy, hx, hy, st), V64_Zero(xs[1], cy, hx, hy, st), V64_Zero(xs[2], cy, hx, hy, st), V64_Eight(xs[3], cy, hx, hy, st));
            p.Fill(Sd.Grow(code, 0.07f), Rgba.Hex("8FEFFF", 0.35f), false);   // alone dell'inchiostro riflettente
            p.Shape(code, Rgba.Hex("E8FDFF"), 0.022f);
            // luccichii
            foreach (var (x, y, r) in new[] { (-0.85f, 0.55f, 0.07f), (0.62f, -0.35f, 0.06f), (0.05f, 0.62f, 0.05f) })
            {
                p.Fill(Sd.Union(Sd.Box(x, y, r, r * 0.18f), Sd.Box(x, y, r * 0.18f, r)), Rgba.White.WithAlpha(0.9f), false);
            }
            // riflessi del vetro (come lo specchio originale)
            p.Line(Rgba.White.WithAlpha(0.45f), 0.06f, -0.6f, 0.9f, -0.2f, 1.2f);
            p.Line(Rgba.White.WithAlpha(0.3f), 0.04f, -0.75f, 0.55f, -0.45f, 0.8f);
            p.Shape(glass, Rgba.Clear, 0.03f);
            return new DoodleArt("v64_pw_mirror_reflect", p);
        }

        /// <summary>Cardine (centro 0,0 = asse del perno sul bordo della porta): due ali avvitate e il cilindro.</summary>
        public static DoodleArt V64_PwHinge()
        {
            var p = new DoodlePainter(-0.26f, -0.32f, 0.26f, 0.32f, 200) { Ink = 0.03f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Box(s * 0.12f, 0, 0.1f, 0.2f, 0.02f), Rgba.Hex("858B96"), 0.025f);
                p.Shape(Sd.Circle(s * 0.14f, 0.11f, 0.022f), Rgba.Hex("C9CFD8"), 0.01f);
                p.Shape(Sd.Circle(s * 0.14f, -0.11f, 0.022f), Rgba.Hex("C9CFD8"), 0.01f);
            }
            Body(p, Sd.Box(0, 0, 0.055f, 0.25f, 0.03f), Rgba.Hex("AEB4BE"), Rgba.Hex("858B96"));
            p.Line(Rgba.Hex("2B2233", 0.6f), 0.015f, -0.05f, 0.0f, 0.05f, 0.0f);
            return new DoodleArt("v64_pw_hinge", p);
        }

        /// <summary>Perno del cardine (asse in 0,0; testa in alto a y 0.27): si sfila verso l'alto (sotto: capovolto).</summary>
        public static DoodleArt V64_PwPin()
        {
            var p = new DoodlePainter(-0.1f, -0.27f, 0.1f, 0.33f, 220) { Ink = 0.022f };
            p.Shape(Sd.Box(0, 0, 0.028f, 0.24f, 0.01f), Rgba.Hex("D7DCE3"));
            p.Shape(Sd.Box(0, 0.27f, 0.065f, 0.035f, 0.015f), Rgba.Hex("E9C46A"));
            return new DoodleArt("v64_pw_pin", p);
        }

        /// <summary>Sgabuzzino dietro la porta (centro 0,0, stesso riquadro della porta): mensole, scopa, secchio, lampadina.</summary>
        public static DoodleArt V64_PwCloset()
        {
            var p = new DoodlePainter(-1.06f, -1.86f, 1.06f, 1.86f, 80) { Ink = 0.04f };
            p.FillFunc(Sd.Box(0, 0, 1.05f, 1.85f), (x, y) => Rgba.Lerp(Rgba.Hex("6B4F3A"), Rgba.Hex("8A6A4C"), Clamp((y + 1.85f) / 3.7f)));
            for (float x = -0.75f; x < 1f; x += 0.35f) p.Line(Rgba.Hex("000000", 0.15f), 0.02f, x, -1.85f, x, 1.85f);
            // lampadina
            p.Line(Ink, 0.02f, 0.1f, 1.85f, 0.1f, 1.5f);
            p.Shape(Sd.Circle(0.1f, 1.4f, 0.11f), Rgba.Hex("FFF3B0"), 0.02f);
            // mensole con detersivi e rotoli
            foreach (float y in new[] { 0.95f, 0.2f }) p.Shape(Sd.Box(0.1f, y, 0.92f, 0.045f), Rgba.Hex("B08A5E"), 0.025f);
            p.Shape(Sd.Box(-0.45f, 1.18f, 0.09f, 0.18f, 0.04f), Rgba.Hex("5BC0EB"), 0.02f);
            p.Shape(Sd.Box(-0.45f, 1.4f, 0.04f, 0.05f), Rgba.Hex("F25F5C"), 0.015f);
            p.Shape(Sd.Box(-0.15f, 1.15f, 0.08f, 0.15f, 0.04f), Rgba.Hex("9BC53D"), 0.02f);
            p.Shape(Sd.Box(0.3f, 1.12f, 0.12f, 0.12f, 0.03f), Rgba.White, 0.02f);
            p.Shape(Sd.Box(0.58f, 1.12f, 0.12f, 0.12f, 0.03f), Rgba.White, 0.02f);
            p.Shape(Sd.Box(-0.3f, 0.36f, 0.18f, 0.12f, 0.03f), Rgba.Hex("FFE066"), 0.02f);   // spugna
            p.Shape(Sd.Box(0.35f, 0.42f, 0.1f, 0.18f, 0.04f), Rgba.Hex("C77DFF"), 0.02f);
            // scopa appoggiata a sinistra
            p.Shape(Sd.Capsule(-0.78f, -1.2f, -0.45f, 0.6f, 0.035f), Rgba.Hex("C08A4A"), 0.02f);
            p.Shape(Sd.Polygon(-0.98f, -1.82f, -0.58f, -1.82f, -0.7f, -1.25f, -0.86f, -1.25f), Rgba.Hex("E9C46A"), 0.025f);
            // secchio a destra
            Body(p, Sd.Polygon(0.35f, -1.15f, 0.9f, -1.15f, 0.82f, -1.82f, 0.43f, -1.82f), Rgba.Hex("4FA3E0"), Rgba.Hex("2F7FBF"));
            p.Line(Ink, 0.025f, 0.38f, -1.15f, 0.62f, -0.88f, 0.87f, -1.15f);
            p.Fill(Sd.Box(0, -1.83f, 1.05f, 0.03f), Rgba.Hex("000000", 0.3f));
            return new DoodleArt("v64_pw_closet", p);
        }

        /// <summary>Mocio (perno alla base della testa in 0,0, manico verso l'alto fino a y 2.5).</summary>
        public static DoodleArt V64_PwMop()
        {
            var p = new DoodlePainter(-0.38f, -0.06f, 0.38f, 2.6f, 100) { Ink = 0.035f };
            p.Shape(Sd.Capsule(0, 0.3f, 0, 2.5f, 0.04f), Rgba.Hex("C9A06A"));
            Sdf head = Sd.Polygon(-0.1f, 0.42f, 0.1f, 0.42f, 0.32f, 0.02f, -0.32f, 0.02f);
            p.Shape(head, Rgba.Hex("ECE6D4"));
            for (float x = -0.24f; x <= 0.25f; x += 0.08f) p.Line(Rgba.Hex("B9B19A"), 0.018f, x * 0.3f, 0.38f, x, 0.05f);
            p.Shape(Sd.Box(0, 0.42f, 0.1f, 0.04f, 0.01f), Rgba.Hex("4FA3E0"), 0.02f);
            return new DoodleArt("v64_pw_mop", p);
        }

        /// <summary>Console della tastiera in primo piano (coordinate del livello): display in alto a sinistra, tasti sotto.</summary>
        public static DoodleArt V64_PwConsole()
        {
            var p = new DoodlePainter(-2.72f, -4.95f, 2.72f, -2.85f, 60) { Ink = 0.05f };
            Body(p, Sd.Box(0, -3.9f, 2.62f, 1.0f, 0.12f), Rgba.Hex("4A5263"), Rgba.Hex("3A4150"));
            p.Fill(Sd.Box(0, -2.98f, 2.45f, 0.03f), Rgba.White.WithAlpha(0.15f));
            Sdf screen = Sd.Box(-0.9f, -3.27f, 1.5f, 0.24f, 0.05f);
            p.FillFunc(screen, (x, y) => Rgba.Lerp(Rgba.Hex("0C2A1E"), Rgba.Hex("174532"), Clamp((y + 3.5f) / 0.5f)));
            p.Shape(screen, Rgba.Clear, 0.035f);
            foreach (float x in new[] { -2.45f, 2.45f })
                foreach (float y in new[] { -3.05f, -4.75f })
                    p.Shape(Sd.Circle(x, y, 0.04f), Rgba.Hex("8D96A6"), 0.015f);
            return new DoodleArt("v64_pw_console", p);
        }

        /// <summary>Pulsante rettangolare bianco (centro 0,0, 1.6 x 0.48): si tinge col colore del renderer.</summary>
        public static DoodleArt V64_PwButton()
        {
            var p = new DoodlePainter(-0.84f, -0.28f, 0.84f, 0.28f, 120) { Ink = 0.035f };
            Body(p, Sd.Box(0, 0, 0.8f, 0.24f, 0.08f), Rgba.Hex("F3F5FA"), Rgba.Hex("C9CED9"));
            return new DoodleArt("v64_pw_button", p);
        }

        /// <summary>Cartello di pericolo giallo col bordo a strisce (centro 0,0, 1.9 x 0.62); la scritta è un testo sopra.</summary>
        public static DoodleArt V64_PwSign()
        {
            var p = new DoodlePainter(-1.0f, -0.36f, 1.0f, 0.36f, 110) { Ink = 0.035f };
            Sdf outer = Sd.Box(0, 0, 0.95f, 0.31f, 0.04f);
            p.FillFunc(outer, V64_Hazard);
            p.Shape(Sd.Box(0, 0, 0.84f, 0.21f, 0.02f), Rgba.Hex("F7D54A"), 0.02f);
            p.Shape(outer, Rgba.Clear);
            foreach (float x in new[] { -0.9f, 0.9f }) p.Shape(Sd.Circle(x, 0.26f, 0.025f), Rgba.Hex("C9CFD8"), 0.01f);
            return new DoodleArt("v64_pw_sign", p);
        }

        /// <summary>Spia di stato a muro (centro 0,0): ghiera scura e cupola bianca (la tinge il livello).</summary>
        public static DoodleArt V64_PwLamp()
        {
            var p = new DoodlePainter(-0.2f, -0.2f, 0.2f, 0.2f, 200) { Ink = 0.03f };
            p.Shape(Sd.Circle(0, 0, 0.17f), Rgba.Hex("3B4252"));
            p.Shape(Sd.Circle(0, 0, 0.11f), Rgba.White, 0.02f);
            p.Fill(Sd.Circle(-0.035f, 0.04f, 0.03f), Rgba.White.WithAlpha(0.9f));
            return new DoodleArt("v64_pw_lamp", p);
        }

        // =====================================================================
        // 256 THE INCOMPETENT MAGICIAN (aggiunte agli sprite s41b_*)
        // =====================================================================

        /// <summary>Occhi chiusi del mago (coordinate di "s41b_magician": occhi in -0.07 / 0.08, y 0.6).</summary>
        public static DoodleArt V64_MgEyesClosed()
        {
            var p = new DoodlePainter(-0.15f, 0.52f, 0.16f, 0.68f, 330) { Ink = 0.016f };
            foreach (float x in new[] { -0.07f, 0.08f })
            {
                p.Fill(Sd.Circle(x, 0.6f, 0.058f), Rgba.Hex("FFD7B5"), false);
                p.Line(Ink, 0.016f, x - 0.042f, 0.605f, x - 0.02f, 0.588f, x + 0.02f, 0.588f, x + 0.042f, 0.605f);
                p.Line(Ink, 0.01f, x - 0.01f, 0.588f, x - 0.016f, 0.57f);
                p.Line(Ink, 0.01f, x + 0.012f, 0.588f, x + 0.018f, 0.57f);
            }
            return new DoodleArt("v64_mg_eyes_closed", p);
        }

        /// <summary>Guanto bianco del mago che si stropiccia gli occhi (centro 0,0).</summary>
        public static DoodleArt V64_MgGlove()
        {
            var p = new DoodlePainter(-0.14f, -0.12f, 0.14f, 0.14f, 300) { Ink = 0.016f };
            Sdf glove = Sd.Union(Sd.Ellipse(0, 0, 0.09f, 0.075f), Sd.Rotate(Sd.Ellipse(-0.08f, 0.05f, 0.035f, 0.02f), -0.08f, 0.05f, 35f),
                Sd.Box(0.06f, -0.05f, 0.04f, 0.04f, 0.015f));
            Body(p, glove, Rgba.White, Rgba.Hex("D8DCE8"));
            p.Line(Ink, 0.008f, -0.02f, 0.03f, 0.04f, 0.03f);
            p.Line(Ink, 0.008f, -0.02f, -0.005f, 0.045f, -0.005f);
            return new DoodleArt("v64_mg_glove", p);
        }

        /// <summary>Sopracciglia arrabbiate e palpebre a mezz'asta del coniglio (coordinate di "s41b_rabbit").</summary>
        public static DoodleArt V64_MgRabbitBrows()
        {
            var p = new DoodlePainter(-0.17f, 0.03f, 0.17f, 0.21f, 330) { Ink = 0.014f };
            foreach (float s in new[] { -1f, 1f })
            {
                float x = s * 0.08f;
                p.Fill(Sd.Intersect(Sd.Circle(x, 0.09f, 0.05f), Sd.Box(x, 0.13f, 0.06f, 0.035f)), Rgba.Hex("E4E7F0"), false);
                p.Line(Ink, 0.012f, x - 0.045f, 0.1f, x + 0.045f, 0.1f);
                p.Line(Ink, 0.026f, s * 0.15f, 0.175f, s * 0.035f, 0.145f);
            }
            return new DoodleArt("v64_mg_rabbit_brows", p);
        }

        /// <summary>Braccino del coniglio che indica (spalla in 0,0, punta verso +x).</summary>
        public static DoodleArt V64_MgRabbitPaw()
        {
            var p = new DoodlePainter(-0.06f, -0.08f, 0.42f, 0.08f, 300) { Ink = 0.02f };
            p.Shape(Sd.Capsule(0, 0, 0.3f, 0, 0.045f), Rgba.White);
            p.Shape(Sd.Union(Sd.Circle(0.32f, 0, 0.05f), Sd.Capsule(0.33f, 0.01f, 0.4f, 0.01f, 0.018f)), Rgba.White);
            return new DoodleArt("v64_mg_rabbit_paw", p);
        }

        /// <summary>Coperchio della botola (centro 0,0; 1.0 x 0.38): assi del palco e fessure appena visibili.</summary>
        public static DoodleArt V64_MgTrapdoor()
        {
            var p = new DoodlePainter(-0.54f, -0.22f, 0.54f, 0.22f, 120) { Ink = 0.014f, InkColor = Rgba.Hex("8E5E33") };
            p.Shape(Sd.Box(0, 0, 0.5f, 0.19f), Rgba.Hex("B07A45"));
            p.Line(Rgba.Hex("8E5E33", 0.7f), 0.012f, -0.17f, -0.19f, -0.17f, 0.19f);
            p.Line(Rgba.Hex("8E5E33", 0.7f), 0.012f, 0.17f, -0.19f, 0.17f, 0.19f);
            p.Shape(Sd.Subtract(Sd.Circle(0.36f, 0, 0.035f), Sd.Circle(0.36f, 0, 0.018f)), Rgba.Hex("7A4F2A"), 0.008f);   // anellino
            return new DoodleArt("v64_mg_trapdoor", p);
        }

        /// <summary>La botola aperta: buco scuro (stesso riquadro del coperchio).</summary>
        public static DoodleArt V64_MgTrapHole()
        {
            var p = new DoodlePainter(-0.54f, -0.22f, 0.54f, 0.22f, 120) { Ink = 0.03f };
            Sdf hole = Sd.Box(0, 0, 0.5f, 0.19f, 0.01f);
            p.FillFunc(hole, (x, y) => Rgba.Lerp(Rgba.Hex("0B0710"), Rgba.Hex("2A1B3D"), Clamp((y + 0.19f) / 0.5f)));
            p.Shape(hole, Rgba.Clear);
            p.Fill(Sd.Box(0, 0.15f, 0.48f, 0.03f), Rgba.Hex("5A3A1E"));
            return new DoodleArt("v64_mg_trap_hole", p);
        }

        /// <summary>Contorno luminoso della botola scoperta (bianco, lo tinge il livello).</summary>
        public static DoodleArt V64_MgTrapGlow()
        {
            var p = new DoodlePainter(-0.66f, -0.34f, 0.66f, 0.34f, 100) { Wobble = 0f };
            p.FillFunc(Sd.Subtract(Sd.Box(0, 0, 0.62f, 0.3f, 0.12f), Sd.Box(0, 0, 0.51f, 0.2f)), (x, y) =>
            {
                float d = Math.Max(Math.Abs(x) - 0.51f, Math.Abs(y) - 0.2f);
                return Rgba.White.WithAlpha(0.95f * Clamp(1f - d / 0.11f));
            });
            return new DoodleArt("v64_mg_trap_glow", p);
        }

        /// <summary>Scatola magica chiara con stelle (base in 0,-0.45; alta 0.9): si tinge col colore del renderer.</summary>
        public static DoodleArt V64_MgBox()
        {
            var p = new DoodlePainter(-0.5f, -0.5f, 0.56f, 0.6f, 120) { Ink = 0.035f };
            p.Shape(Sd.Polygon(-0.4f, 0.35f, 0.4f, 0.35f, 0.5f, 0.5f, -0.3f, 0.5f), Rgba.Hex("FFFFFF"));
            p.Shape(Sd.Polygon(0.4f, 0.35f, 0.5f, 0.5f, 0.5f, -0.3f, 0.4f, -0.45f), Rgba.Hex("BFC3CF"));
            Body(p, Sd.Box(0, -0.05f, 0.4f, 0.4f, 0.02f), Rgba.Hex("EEF0F6"), Rgba.Hex("D2D6E2"));
            foreach (var (x, y, r) in new[] { (-0.18f, 0.12f, 0.09f), (0.15f, -0.18f, 0.07f), (0.2f, 0.2f, 0.05f) })
            {
                Sdf star = Sd.Polygon(x, y + r, x + r * 0.3f, y + r * 0.3f, x + r, y + r * 0.2f, x + r * 0.4f, y - r * 0.2f,
                    x + r * 0.6f, y - r, x, y - r * 0.45f, x - r * 0.6f, y - r, x - r * 0.4f, y - r * 0.2f, x - r, y + r * 0.2f, x - r * 0.3f, y + r * 0.3f);
                p.Shape(star, Rgba.Hex("FFE58A"), 0.015f);
            }
            p.Line(Ink, 0.02f, -0.4f, -0.3f, 0.4f, -0.3f);
            return new DoodleArt("v64_mg_box", p);
        }

        /// <summary>Cartello su bastone del coniglio (centro del cartello 0,0; bastone fino a y -0.62); la scritta è un testo.</summary>
        public static DoodleArt V64_MgSign()
        {
            var p = new DoodlePainter(-0.6f, -0.66f, 0.6f, 0.32f, 140) { Ink = 0.03f };
            p.Shape(Sd.Box(0, -0.4f, 0.035f, 0.25f), Rgba.Hex("C08A4A"));
            Body(p, Sd.Box(0, 0, 0.55f, 0.27f, 0.03f), Rgba.Hex("FFF4DC"), Rgba.Hex("E9D8B4"));
            return new DoodleArt("v64_mg_sign", p);
        }

        /// <summary>
        /// Sipario destro in primo piano (coordinate del livello, identico a quello di "s41b_stage_bg"): il coniglio
        /// col cartello può sbucare da dietro.
        /// </summary>
        public static DoodleArt V64_MgCurtain()
        {
            var p = new DoodlePainter(1.95f, -1.25f, 3.3f, 4.2f, 50) { Ink = 0.06f };
            p.Shape(Sd.Polygon(4.6f, 7f, 2.1f, 7f, 2.35f, 1.5f, 2.6f, -1.1f, 4.6f, -1.1f), Rgba.Hex("C02A3A"));
            for (int i = 0; i < 2; i++) p.Line(Rgba.Hex("8E1A28"), 0.05f, 2.4f + i * 0.5f, 7f, 2.6f + i * 0.5f, -1f);
            return new DoodleArt("v64_mg_curtain", p);
        }

        /// <summary>Teste del pubblico in controluce (coordinate del livello, in basso: y -5.2..-3.9).</summary>
        public static DoodleArt V64_MgAudience()
        {
            var p = new DoodlePainter(-3.0f, -5.3f, 3.0f, -3.85f, 50) { Ink = 0.04f, InkColor = Rgba.Hex("0B0712") };
            float[] xs = { -2.6f, -1.75f, -0.9f, 0f, 0.85f, 1.7f, 2.55f };
            for (int i = 0; i < xs.Length; i++)
            {
                float x = xs[i], y = -4.45f + (i % 2) * 0.12f;
                Rgba c = Rgba.Hex(i % 2 == 0 ? "1C1428" : "241A33");
                p.Shape(Sd.Ellipse(x, -5.1f, 0.42f, 0.4f), c);
                Sdf head = Sd.Circle(x, y, 0.3f);
                if (i == 2) head = Sd.Union(head, Sd.Box(x, y + 0.38f, 0.18f, 0.14f, 0.02f), Sd.Box(x, y + 0.25f, 0.3f, 0.03f));   // cappello
                if (i == 5) head = Sd.Union(head, Sd.Circle(x - 0.2f, y + 0.25f, 0.13f), Sd.Circle(x + 0.2f, y + 0.25f, 0.13f));   // codini
                p.Shape(head, c);
                p.Fill(Sd.Crescent(Sd.Circle(x, y, 0.28f), 0.05f, -0.05f), Rgba.Hex("6B4A9A", 0.35f));
            }
            return new DoodleArt("v64_mg_audience", p);
        }

        /// <summary>Distintivo "microfono spento" per l'alternativa senza microfono (centro 0,0).</summary>
        public static DoodleArt V64_MgMicOff()
        {
            var p = new DoodlePainter(-0.3f, -0.3f, 0.3f, 0.3f, 200) { Ink = 0.025f };
            p.Shape(Sd.Circle(0, 0, 0.27f), Rgba.Hex("FFF4DC"));
            p.Shape(Sd.Box(0, 0.05f, 0.06f, 0.11f, 0.06f), Rgba.Hex("6B5A8E"), 0.02f);
            p.Line(Ink, 0.02f, -0.1f, 0.0f, -0.1f, -0.05f, 0f, -0.12f, 0.1f, -0.05f, 0.1f, 0.0f);
            p.Line(Ink, 0.02f, 0f, -0.12f, 0f, -0.18f);
            p.Line(Rgba.Hex("E8433F"), 0.04f, -0.17f, 0.17f, 0.17f, -0.17f);
            return new DoodleArt("v64_mg_mic_off", p);
        }
    }
}
