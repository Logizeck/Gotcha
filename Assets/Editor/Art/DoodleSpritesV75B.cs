using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.75 per il livello 276 "The Locked Bike" (la festa riusa glow, sparkle_star e puff): la
    /// strada (facciata di mattoni con vetrina e portone, marciapiede, cordolo, asfalto), il palo mozzo, il cartello caduto
    /// col pezzo di palo spezzato, la bici (telaio) e la sua ruota, la catena (metà dietro al palo e metà davanti), il
    /// lucchetto a combinazione con 4 rotelle e la toppa, il mazzo di chiavi (con la chiave di casa e l'apribottiglie), la
    /// tronchese (dritta e piegata), il piccione (seduto e in volo). Prefisso v75_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV75B() => new DoodleArt[]
        {
            V75B_StreetBg(), V75B_Pole(), V75B_SignFallen(), V75B_Bike(), V75B_Wheel(), V75B_ChainBack(), V75B_ChainFront(),
            V75B_Padlock(), V75B_Keychain(), V75B_Cutters(), V75B_CuttersBent(), V75B_Pigeon(), V75B_PigeonFly(),
        };

        static readonly Rgba V75BSteel = Rgba.Hex("A7B1BE"), V75BSteelShade = Rgba.Hex("7D8796"), V75BSteelLight = Rgba.Hex("D5DCE5");
        static readonly Rgba V75BFrame = Rgba.Hex("E2574C"), V75BFrameShade = Rgba.Hex("B83E36");
        static readonly Rgba V75BTire = Rgba.Hex("2F2A36"), V75BGold = Rgba.Hex("E8B84A"), V75BGoldShade = Rgba.Hex("C3922E");
        static readonly Rgba V75BPigeon = Rgba.Hex("9AA3B5"), V75BPigeonShade = Rgba.Hex("737C90"), V75BPigeonNeck = Rgba.Hex("6FAE9A");

        /// <summary>Centro dell'anello della catena (coordinate della bici) e i suoi semiassi: gira attorno al palo e al tubo sella.</summary>
        const float V75BChainCx = -0.32f, V75BChainCy = 0.85f, V75BChainRx = 0.3f, V75BChainRy = 0.1f;

        /// <summary>
        /// La strada, coordinate del mondo: cielo in alto, facciata di mattoni (vetrina con tenda a sinistra, portone e
        /// finestre a destra) fino a y -1.7, il marciapiede di lastre (y -1.7..-3.45), il cordolo e l'asfalto con la riga
        /// tratteggiata. La bici sta sul marciapiede (ruote a terra in y -2.4).
        /// </summary>
        public static DoodleArt V75B_StreetBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f, Wobble = 0.006f };
            p.FillFunc(Sd.Box(0f, 4.5f, 5f, 3f), (x, y) => Rgba.Lerp(Rgba.Hex("BFE6F7"), Rgba.Hex("8FCBEA"), Math.Max(0f, Math.Min(1f, (y - 2.6f) / 3f))));
            p.Fill(Sd.Union(Sd.Circle(-1.6f, 4.1f, 0.25f), Sd.Circle(-1.3f, 4.2f, 0.32f), Sd.Circle(-0.95f, 4.08f, 0.22f)), Rgba.White.WithAlpha(0.9f));
            // la facciata di mattoni
            Sdf wall = Sd.Box(0f, 0.95f, 5f, 2.65f);
            p.FillFunc(wall, (x, y) =>
            {
                int j = (int)Math.Floor((y + 1.7f) / 0.2f);
                int i = (int)Math.Floor((x + (j % 2 == 0 ? 0f : 0.2f)) / 0.4f);
                int h = (i * 7 + j * 13) & 7;
                return h == 0 ? Rgba.Hex("D9826A") : h == 3 ? Rgba.Hex("C96F58") : Rgba.Hex("D27760");
            });
            for (int j = 0; j <= 26; j++)
            {
                float y = -1.7f + j * 0.2f;
                p.Line(Rgba.Hex("B35F4B", 0.55f), 0.018f, -4.5f, y, 4.5f, y);
                for (int i = -12; i <= 12; i++)
                {
                    float x = i * 0.4f + (j % 2 == 0 ? 0f : 0.2f);
                    p.Line(Rgba.Hex("B35F4B", 0.55f), 0.018f, x, y, x, y + 0.2f);
                }
            }
            p.Fill(Sd.Box(0f, 3.62f, 5f, 0.06f), Rgba.Hex("8E4A3B"));
            // la vetrina con la tenda a righe (a sinistra)
            p.Shape(Sd.Box(-1.35f, -0.35f, 1.15f, 0.95f, 0.03f), Rgba.Hex("6E5A4E"), 0.05f);
            p.FillFunc(Sd.Box(-1.35f, -0.3f, 1.02f, 0.82f), (x, y) => Rgba.Lerp(Rgba.Hex("CFEAF3"), Rgba.Hex("9CC8DA"), Math.Max(0f, Math.Min(1f, (0.5f - y) / 1.6f))));
            p.Fill(Sd.Polygon(-2.2f, 0.5f, -1.75f, 0.5f, -2.3f, -0.7f, -2.37f, -0.7f), Rgba.White.WithAlpha(0.45f));
            p.Fill(Sd.Polygon(-1.4f, 0.5f, -1.2f, 0.5f, -1.75f, -1.1f, -1.95f, -1.1f), Rgba.White.WithAlpha(0.35f));
            p.Shape(Sd.Box(-1.35f, -0.3f, 1.02f, 0.82f), Rgba.Clear, 0.04f);
            p.Shape(Sd.Box(-1.35f, -1.22f, 1.2f, 0.08f, 0.02f), Rgba.Hex("B9A89A"), 0.04f);
            Sdf awning = Sd.Polygon(-2.65f, 1.0f, -0.05f, 1.0f, 0.1f, 0.62f, -2.8f, 0.62f);
            p.FillFunc(awning, (x, y) => ((int)Math.Floor((x + 4f) / 0.29f)) % 2 == 0 ? Rgba.Hex("3E9C8B") : Rgba.Hex("F3EEDF"));
            p.Shape(awning, Rgba.Clear, 0.045f);
            for (int k = 0; k < 10; k++)
            {
                float x = -2.8f + k * 0.29f + 0.145f;
                p.Shape(Sd.Circle(x, 0.62f, 0.145f), ((int)Math.Floor((x + 4f) / 0.29f)) % 2 == 0 ? Rgba.Hex("3E9C8B") : Rgba.Hex("F3EEDF"), 0.035f);
            }
            p.Fill(Sd.Box(-1.35f, 0.83f, 1.3f, 0.2f), Rgba.Clear);
            // il portone e le finestre (a destra)
            p.Shape(Sd.Box(1.55f, -0.55f, 0.55f, 1.18f, 0.06f), Rgba.Hex("4F7C9E"), 0.05f);
            p.Shape(Sd.Box(1.55f, -0.25f, 0.4f, 0.72f, 0.04f), Rgba.Hex("5E8CAE"), 0.035f);
            p.Shape(Sd.Circle(1.88f, -0.6f, 0.05f), V75BGold, 0.025f);
            p.Shape(Sd.Box(1.55f, -1.66f, 0.68f, 0.06f, 0.02f), Rgba.Hex("B9A89A"), 0.035f);
            foreach (float x in new[] { -1.9f, -0.75f, 0.45f, 1.6f })
            {
                p.Shape(Sd.Box(x, 2.25f, 0.34f, 0.48f, 0.03f), Rgba.Hex("F3EEDF"), 0.045f);
                p.FillFunc(Sd.Box(x, 2.25f, 0.27f, 0.41f), (xx, yy) => Rgba.Lerp(Rgba.Hex("BFE3F2"), Rgba.Hex("7FB3CC"), Math.Max(0f, Math.Min(1f, (2.66f - yy) / 0.82f))));
                p.Line(Rgba.Hex("F3EEDF"), 0.04f, x, 1.84f, x, 2.66f);
                p.Line(Rgba.Hex("F3EEDF"), 0.04f, x - 0.27f, 2.3f, x + 0.27f, 2.3f);
                p.Shape(Sd.Box(x, 1.74f, 0.4f, 0.05f, 0.02f), Rgba.Hex("B9A89A"), 0.03f);
            }
            // il marciapiede (lastre in prospettiva, più chiaro verso il fondo)
            Sdf walk = Sd.Box(0f, -2.58f, 5f, 0.88f);
            p.FillFunc(walk, (x, y) =>
            {
                float k = Math.Max(0f, Math.Min(1f, (-1.7f - y) / 1.75f));
                return Rgba.Lerp(Rgba.Hex("D8D6D2"), Rgba.Hex("C3C0BA"), k);
            });
            p.Fill(Sd.Box(0f, -1.76f, 5f, 0.06f), new Rgba(0f, 0f, 0f, 0.12f));
            foreach (float y in new[] { -2.05f, -2.55f, -3.1f }) p.Line(Rgba.Hex("A8A49C", 0.8f), 0.025f, -4.5f, y, 4.5f, y);
            for (int i = -6; i <= 6; i++) p.Line(Rgba.Hex("A8A49C", 0.8f), 0.025f, i * 0.75f, -1.7f, i * 1.0f, -3.45f);
            p.Fill(Sd.Ellipse(1.9f, -3.3f, 0.12f, 0.04f), Rgba.Hex("A8A49C", 0.6f));
            p.Fill(Sd.Ellipse(-2.1f, -2.2f, 0.1f, 0.03f), Rgba.Hex("A8A49C", 0.6f));
            // cordolo e strada
            p.Fill(Sd.Box(0f, -3.53f, 5f, 0.09f), Rgba.Hex("E9E7E2"), false);
            p.Fill(Sd.Box(0f, -3.66f, 5f, 0.05f), Rgba.Hex("9C988F"), false);
            p.Line(Rgba.Hex("2B2233", 0.7f), 0.04f, -4.5f, -3.45f, 4.5f, -3.45f);
            p.FillFunc(Sd.Box(0f, -5.4f, 5f, 1.7f), (x, y) => Rgba.Lerp(Rgba.Hex("57535E"), Rgba.Hex("47434E"), Math.Max(0f, Math.Min(1f, (-3.7f - y) / 2f))));
            for (int i = -6; i <= 6; i++) p.Fill(Sd.Box(i * 0.9f, -4.75f, 0.25f, 0.04f, 0.02f), Rgba.Hex("F2E6B0", 0.9f));
            return new DoodleArt("v75_b_street_bg", p);
        }

        /// <summary>
        /// Il palo mozzo (origine alla base; cima piatta in y 1.78): tubo d'acciaio con la piastra a terra, la fascetta
        /// rimasta dove c'era il cartello e due bulloni tranciati.
        /// </summary>
        public static DoodleArt V75B_Pole()
        {
            var p = new DoodlePainter(-0.32f, -0.12f, 0.32f, 1.9f, 80) { Ink = 0.04f };
            p.Fill(Sd.Ellipse(0.04f, 0f, 0.28f, 0.06f), new Rgba(0f, 0f, 0f, 0.18f));
            p.Shape(Sd.Box(0f, 0.03f, 0.2f, 0.04f, 0.02f), V75BSteelShade, 0.03f);
            Sdf tube = Sd.Box(0f, 0.9f, 0.085f, 0.88f, 0.01f);
            p.FillFunc(tube, (x, y) => Rgba.Lerp(V75BSteelLight, V75BSteelShade, Math.Max(0f, Math.Min(1f, (x + 0.085f) / 0.17f))));
            p.Fill(Sd.Box(-0.035f, 0.9f, 0.018f, 0.82f), Rgba.White.WithAlpha(0.5f));
            p.Shape(tube, Rgba.Clear);
            // la cima tagliata, vista un po' dall'alto (si vede che è vuota)
            p.Shape(Sd.Ellipse(0f, 1.78f, 0.085f, 0.03f), Rgba.Hex("4A4F5C"), 0.03f);
            // la fascetta del vecchio cartello e i bulloni tranciati
            p.Shape(Sd.Box(0f, 1.52f, 0.1f, 0.045f, 0.015f), V75BSteel, 0.03f);
            foreach (float y in new[] { 1.5f, 1.55f }) p.Shape(Sd.Box(0.14f, y, 0.045f, 0.012f), V75BSteelShade, 0.02f);
            p.Fill(Sd.Box(0f, 1.25f, 0.085f, 0.02f), new Rgba(0.55f, 0.32f, 0.18f, 0.35f));
            return new DoodleArt("v75_b_pole", p);
        }

        /// <summary>
        /// Il cartello caduto (origine al centro; disegnato in piedi, nel livello è girato e sdraiato per terra): il cartello
        /// blu "P" col simbolo della bici e sotto il pezzo di palo spezzato, con la sua fascetta.
        /// </summary>
        public static DoodleArt V75B_SignFallen()
        {
            var p = new DoodlePainter(-0.45f, -0.72f, 0.45f, 0.8f, 80) { Ink = 0.04f };
            // il pezzo di palo spezzato (estremità frastagliata in basso)
            Sdf stub = Sd.Polygon(-0.085f, 0.0f, 0.085f, 0.0f, 0.085f, -0.5f, 0.04f, -0.56f, 0.0f, -0.5f, -0.05f, -0.6f, -0.085f, -0.53f);
            p.FillFunc(stub, (x, y) => Rgba.Lerp(V75BSteelLight, V75BSteelShade, Math.Max(0f, Math.Min(1f, (x + 0.085f) / 0.17f))));
            p.Shape(stub, Rgba.Clear);
            p.Shape(Sd.Box(0f, -0.02f, 0.1f, 0.045f, 0.015f), V75BSteel, 0.03f);
            // il cartello
            Sdf plate = Sd.Box(0f, 0.38f, 0.36f, 0.36f, 0.06f);
            p.Shape(plate, Rgba.White, 0.045f);
            p.Shape(Sd.Box(0f, 0.38f, 0.31f, 0.31f, 0.04f), Rgba.Hex("3C7DD9"), 0.02f);
            // la P
            Rgba w = Rgba.White;
            p.Fill(Sd.Capsule(-0.15f, 0.2f, -0.15f, 0.6f, 0.045f), w);
            p.Fill(Sd.Subtract(Sd.Intersect(Sd.Ellipse(-0.1f, 0.5f, 0.16f, 0.11f), Sd.Box(0.1f, 0.5f, 0.2f, 0.2f)), Sd.Ellipse(-0.1f, 0.5f, 0.07f, 0.04f)), w);
            p.Fill(Sd.Box(-0.1f, 0.61f, 0.06f, 0.0f), w);
            p.Fill(Sd.Capsule(-0.15f, 0.61f, -0.08f, 0.61f, 0.045f), w);
            p.Fill(Sd.Capsule(-0.15f, 0.39f, -0.08f, 0.39f, 0.045f), w);
            // la bicicletta piccola
            foreach (float x in new[] { 0.08f, 0.24f }) p.Fill(Sd.Subtract(Sd.Circle(x, 0.2f, 0.065f), Sd.Circle(x, 0.2f, 0.04f)), w);
            p.Line(w, 0.025f, 0.08f, 0.2f, 0.14f, 0.3f, 0.21f, 0.3f, 0.24f, 0.2f);
            p.Line(w, 0.025f, 0.14f, 0.3f, 0.16f, 0.2f, 0.21f, 0.3f);
            p.Line(w, 0.025f, 0.12f, 0.34f, 0.16f, 0.34f);
            // un graffio e un'ammaccatura: è caduto
            p.Line(Rgba.Hex("2B2233", 0.35f), 0.015f, 0.12f, 0.62f, 0.25f, 0.52f);
            p.Fill(Sd.Ellipse(-0.24f, 0.13f, 0.05f, 0.03f), new Rgba(0f, 0f, 0f, 0.15f));
            return new DoodleArt("v75_b_sign_fallen", p);
        }

        /// <summary>
        /// La bici senza ruote (origine a terra, a metà fra le ruote; assi delle ruote in (±0.85, 0.5)): telaio rosso con
        /// tubo sella da (0, 0.45) a (-0.22, 1.12), sella, manubrio in (0.6, 1.45) col campanello, corona e pedale.
        /// </summary>
        public static DoodleArt V75B_Bike()
        {
            var p = new DoodlePainter(-1.45f, 0.15f, 1.0f, 1.62f, 80) { Ink = 0.035f };
            float bbx = 0f, bby = 0.45f, sx = -0.22f, sy = 1.12f, hx = 0.62f, hy = 1.15f, hbx = 0.68f, hby = 0.92f;
            float rx = -0.85f, fx = 0.85f, ay = 0.5f;
            const float r = 0.042f;
            Sdf frame = Sd.Union(
                Sd.Capsule(rx, ay, bbx, bby, r * 0.85f),
                Sd.Capsule(rx, ay, sx + 0.02f, sy - 0.05f, r * 0.8f),
                Sd.Capsule(bbx, bby, sx, sy, r),
                Sd.Capsule(sx + 0.01f, sy - 0.03f, hx, hy, r),
                Sd.Capsule(bbx, bby, hbx, hby, r * 1.1f),
                Sd.Capsule(hx - 0.01f, hy + 0.04f, hbx, hby - 0.04f, r * 1.25f),
                Sd.Capsule(hbx, hby, fx, ay, r * 0.8f));
            // il parafango dietro (sopra la ruota)
            p.Shape(Sd.Subtract(Sd.Intersect(Sd.Circle(rx, ay, 0.58f), Sd.Box(rx - 0.05f, ay + 0.35f, 0.5f, 0.3f)), Sd.Circle(rx, ay, 0.53f)), Rgba.Hex("D9D2C6"), 0.025f);
            // corona, pedivella e pedale
            p.Shape(Sd.Circle(bbx, bby, 0.13f), V75BSteel, 0.03f);
            p.Fill(Sd.Circle(bbx, bby, 0.06f), V75BSteelShade);
            p.Line(Rgba.Hex("4A4F5C", 0.85f), 0.02f, rx, ay + 0.05f, bbx, bby + 0.13f);
            p.Line(Rgba.Hex("4A4F5C", 0.85f), 0.02f, rx, ay - 0.05f, bbx, bby - 0.13f);
            p.Fill(frame, V75BFrame);
            p.Fill(Sd.Intersect(frame, (x, y) => -(y - (bby + (x - bbx) * 0.35f)) - 0.015f), V75BFrameShade.WithAlpha(0.0f));
            p.Shape(frame, Rgba.Clear);
            p.Fill(Sd.Capsule(sx + 0.05f, sy - 0.08f, hx - 0.08f, hy - 0.02f, 0.012f), Rgba.White.WithAlpha(0.45f));
            p.Shape(Sd.Capsule(bbx + 0.02f, bby, bbx + 0.14f, bby - 0.22f, 0.025f), Rgba.Hex("4A4F5C"), 0.02f);
            p.Shape(Sd.Box(bbx + 0.15f, bby - 0.24f, 0.08f, 0.025f, 0.01f), Rgba.Hex("2F2A36"), 0.02f);
            // reggisella e sella
            p.Shape(Sd.Capsule(sx, sy, sx - 0.03f, sy + 0.12f, 0.022f), V75BSteel, 0.02f);
            p.Shape(Sd.Union(Sd.Ellipse(sx - 0.05f, sy + 0.15f, 0.17f, 0.055f), Sd.Ellipse(sx + 0.08f, sy + 0.16f, 0.1f, 0.035f)), Rgba.Hex("3B3340"), 0.03f);
            // attacco e manubrio (curvo, manopole scure) col campanello giallo
            p.Shape(Sd.Capsule(hx, hy, hx - 0.03f, hy + 0.28f, 0.025f), V75BSteel, 0.02f);
            p.Shape(Sd.Polyline(0.022f, hx - 0.03f, hy + 0.28f, hx - 0.2f, hy + 0.32f, hx - 0.28f, hy + 0.26f), V75BSteel, 0.02f);
            p.Shape(Sd.Capsule(hx - 0.28f, hy + 0.26f, hx - 0.36f, hy + 0.2f, 0.032f), Rgba.Hex("3B3340"), 0.02f);
            p.Shape(Sd.Circle(hx + 0.04f, hy + 0.25f, 0.045f), Rgba.Hex("F2C94C"), 0.02f);
            // il catarifrangente
            p.Shape(Sd.Box(rx + 0.05f, ay + 0.6f, 0.05f, 0.025f, 0.01f), Rgba.Hex("E2504A"), 0.015f);
            return new DoodleArt("v75_b_bike", p);
        }

        /// <summary>Una ruota (origine all'asse, raggio 0.5): copertone scuro, cerchio d'acciaio, raggi e mozzo.</summary>
        public static DoodleArt V75B_Wheel()
        {
            var p = new DoodlePainter(-0.56f, -0.56f, 0.56f, 0.56f, 80) { Ink = 0.03f };
            p.Shape(Sd.Subtract(Sd.Circle(0f, 0f, 0.5f), Sd.Circle(0f, 0f, 0.42f)), V75BTire, 0.03f);
            p.Shape(Sd.Subtract(Sd.Circle(0f, 0f, 0.425f), Sd.Circle(0f, 0f, 0.39f)), V75BSteelLight, 0.02f);
            for (int i = 0; i < 12; i++)
            {
                double a = i * Math.PI / 6.0;
                double b = a + (i % 2 == 0 ? 0.35 : -0.35);
                p.Line(Rgba.Hex("6E7685"), 0.012f, (float)(Math.Cos(b) * 0.05), (float)(Math.Sin(b) * 0.05), (float)(Math.Cos(a) * 0.39), (float)(Math.Sin(a) * 0.39));
            }
            p.Shape(Sd.Circle(0f, 0f, 0.06f), V75BSteel, 0.02f);
            // il battistrada: qualche tacca (si vede girare)
            for (int i = 0; i < 6; i++)
            {
                double a = i * Math.PI / 3.0;
                p.Line(Rgba.Hex("57505F"), 0.02f, (float)(Math.Cos(a) * 0.44), (float)(Math.Sin(a) * 0.44), (float)(Math.Cos(a + 0.18) * 0.47), (float)(Math.Sin(a + 0.18) * 0.47));
            }
            return new DoodleArt("v75_b_wheel", p);
        }

        /// <summary>Le maglie della catena da t0 a t1 (radianti) lungo l'anello attorno al palo e al tubo sella.</summary>
        static void V75B_ChainLinks(DoodlePainter p, double t0, double t1)
        {
            const int n = 18;
            for (int i = 0; i < n; i++)
            {
                double t = i * 2.0 * Math.PI / n + 0.07;
                double tt = t % (2.0 * Math.PI);
                if (tt < t0 || tt > t1) continue;
                float x = V75BChainCx + V75BChainRx * (float)Math.Cos(tt), y = V75BChainCy + V75BChainRy * (float)Math.Sin(tt);
                float dx = -V75BChainRx * (float)Math.Sin(tt), dy = V75BChainRy * (float)Math.Cos(tt);
                float deg = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
                bool flat = i % 2 == 0;
                Sdf link = Sd.Rotate(Sd.Subtract(Sd.Ellipse(x, y, 0.075f, flat ? 0.045f : 0.026f), Sd.Ellipse(x, y, 0.04f, flat ? 0.016f : 0.006f)), x, y, deg);
                p.Shape(link, flat ? V75BSteel : V75BSteelShade, 0.022f);
            }
        }

        /// <summary>La metà di dietro della catena (coordinate della bici; sta dietro al palo).</summary>
        public static DoodleArt V75B_ChainBack()
        {
            var p = new DoodlePainter(-0.72f, 0.66f, 0.08f, 1.04f, 90) { Ink = 0.025f, Wobble = 0.004f };
            V75B_ChainLinks(p, 0.0, Math.PI);
            return new DoodleArt("v75_b_chain_back", p);
        }

        /// <summary>La metà davanti della catena (coordinate della bici; sta davanti al palo e al telaio).</summary>
        public static DoodleArt V75B_ChainFront()
        {
            var p = new DoodlePainter(-0.72f, 0.66f, 0.08f, 1.04f, 90) { Ink = 0.025f, Wobble = 0.004f };
            V75B_ChainLinks(p, Math.PI, 2.0 * Math.PI);
            return new DoodleArt("v75_b_chain_front", p);
        }

        /// <summary>
        /// Il lucchetto a combinazione (origine in cima all'archetto, da cui pende dalla catena): archetto d'acciaio, corpo
        /// grosso blu notte, la finestrella con le 4 rotelle (centri x -0.165, -0.055, 0.055, 0.165; y -0.36; le cifre sono
        /// testi del livello) e la toppa dorata in basso.
        /// </summary>
        public static DoodleArt V75B_Padlock()
        {
            var p = new DoodlePainter(-0.34f, -0.7f, 0.34f, 0.04f, 110) { Ink = 0.035f };
            p.Shape(Sd.Subtract(Sd.Union(Sd.Box(0f, -0.14f, 0.17f, 0.12f), Sd.Circle(0f, -0.13f, 0.17f)), Sd.Union(Sd.Box(0f, -0.16f, 0.1f, 0.12f), Sd.Circle(0f, -0.13f, 0.1f))), V75BSteel, 0.03f);
            Sdf body = Sd.Box(0f, -0.43f, 0.29f, 0.24f, 0.07f);
            Body(p, body, Rgba.Hex("2F3E62"), Rgba.Hex("22304D"), null, -0.04f, 0.05f);
            p.Fill(Sd.Box(-0.18f, -0.25f, 0.08f, 0.02f, 0.02f), Rgba.White.WithAlpha(0.3f));
            // la finestrella delle rotelle
            p.Shape(Sd.Box(0f, -0.36f, 0.235f, 0.075f, 0.02f), Rgba.Hex("1A2238"), 0.025f);
            foreach (float x in new[] { -0.165f, -0.055f, 0.055f, 0.165f })
            {
                p.FillFunc(Sd.Box(x, -0.36f, 0.045f, 0.065f, 0.012f), (xx, yy) => Rgba.Lerp(Rgba.Hex("FFF6DE"), Rgba.Hex("C9B98F"), Math.Min(1f, Math.Abs(yy + 0.36f) / 0.065f)));
                for (int k = -2; k <= 2; k++) p.Line(Rgba.Hex("2B2233", 0.25f), 0.006f, x - 0.045f, -0.36f + k * 0.028f, x - 0.035f, -0.36f + k * 0.028f);
            }
            // la toppa
            p.Shape(Sd.Circle(0f, -0.54f, 0.06f), V75BGold, 0.025f);
            p.Fill(Sd.Union(Sd.Circle(0f, -0.53f, 0.018f), Sd.Box(0f, -0.56f, 0.008f, 0.025f)), Rgba.Hex("2B2233"));
            return new DoodleArt("v75_b_padlock", p);
        }

        /// <summary>Una chiave che pende dall'anello (testa in (hx, hy), punta verso (tx, ty)), colore a scelta.</summary>
        static void V75B_Key(DoodlePainter p, float hx, float hy, float tx, float ty, Rgba c, Rgba shade)
        {
            float dx = tx - hx, dy = ty - hy, len = (float)Math.Sqrt(dx * dx + dy * dy);
            float ux = dx / len, uy = dy / len, nx = -uy, ny = ux;
            float bx = hx + ux * 0.1f, by = hy + uy * 0.1f;
            Sdf blade = Sd.Capsule(bx, by, tx, ty, 0.028f);
            var teeth = new List<Sdf> { blade };
            for (int k = 0; k < 3; k++)
            {
                float t = 0.45f + k * 0.17f;
                float cx = hx + dx * t + nx * 0.035f, cy = hy + dy * t + ny * 0.035f;
                teeth.Add(Sd.Box(cx, cy, 0.025f, 0.025f));
            }
            p.Shape(Sd.Union(teeth.ToArray()), c, 0.022f);
            p.Shape(Sd.Subtract(Sd.Circle(hx, hy, 0.085f), Sd.Circle(hx - ux * 0.03f, hy - uy * 0.03f, 0.028f)), c, 0.025f);
            p.Fill(Sd.Crescent(Sd.Circle(hx, hy, 0.07f), -0.025f, 0.025f), shade.WithAlpha(0.7f));
        }

        /// <summary>
        /// Il mazzo di chiavi (origine al centro dell'anello): quattro chiavi (oro, argento, bronzo, una col cappuccio blu),
        /// la chiave di casa con la targhetta a casetta e l'apribottiglie.
        /// </summary>
        public static DoodleArt V75B_Keychain()
        {
            var p = new DoodlePainter(-0.62f, -0.78f, 0.62f, 0.2f, 100) { Ink = 0.03f };
            Sdf ring = Sd.Subtract(Sd.Circle(0f, 0f, 0.13f), Sd.Circle(0f, 0f, 0.1f));
            // l'apribottiglie (a sinistra)
            Sdf opener = Sd.Subtract(Sd.Union(Sd.Capsule(-0.14f, -0.1f, -0.42f, -0.48f, 0.05f), Sd.Circle(-0.44f, -0.52f, 0.11f)), Sd.Union(Sd.Circle(-0.44f, -0.52f, 0.055f), Sd.Box(-0.52f, -0.6f, 0.05f, 0.03f)));
            p.Shape(opener, Rgba.Hex("D9DEE6"), 0.025f);
            V75B_Key(p, -0.12f, -0.2f, -0.24f, -0.66f, V75BSteelLight, V75BSteelShade);
            V75B_Key(p, -0.02f, -0.22f, -0.04f, -0.72f, V75BGold, V75BGoldShade);
            V75B_Key(p, 0.08f, -0.2f, 0.18f, -0.68f, Rgba.Hex("D19A6A"), Rgba.Hex("A8744A"));
            V75B_Key(p, 0.15f, -0.15f, 0.42f, -0.52f, V75BSteelLight, V75BSteelShade);
            p.Shape(Sd.Circle(0.15f, -0.15f, 0.07f), Rgba.Hex("4C8DDC"), 0.025f);
            // la chiave di casa con la targhetta a casetta
            V75B_Key(p, 0.12f, -0.05f, 0.5f, -0.2f, V75BGold, V75BGoldShade);
            p.Shape(Sd.Union(Sd.Box(0.33f, 0.04f, 0.09f, 0.07f, 0.01f), Sd.Polygon(0.22f, 0.1f, 0.44f, 0.1f, 0.33f, 0.2f)), Rgba.Hex("E2574C"), 0.022f);
            p.Fill(Sd.Box(0.33f, 0.01f, 0.025f, 0.04f), Rgba.Hex("FFF3E0"));
            p.Line(Rgba.Hex("2B2233"), 0.015f, 0.2f, 0.02f, 0.25f, 0.04f);
            p.Shape(ring, V75BSteel, 0.022f);
            return new DoodleArt("v75_b_keychain", p);
        }

        /// <summary>Le ganasce e i manici (origine al perno): ganasce a sinistra fino a x -0.42, manici rossi fino a x 1.1.</summary>
        static void V75B_CutterJaws(DoodlePainter p)
        {
            p.Shape(Sd.Polygon(0.05f, 0.05f, -0.42f, 0.03f, -0.3f, 0.1f, -0.05f, 0.14f), V75BSteel, 0.025f);
            p.Shape(Sd.Polygon(0.05f, -0.05f, -0.42f, -0.03f, -0.3f, -0.1f, -0.05f, -0.14f), V75BSteelShade, 0.025f);
            p.Shape(Sd.Circle(0f, 0f, 0.07f), V75BSteelLight, 0.025f);
            p.Fill(Sd.Circle(0f, 0f, 0.025f), Rgba.Hex("4A4F5C"));
        }

        /// <summary>La tronchese, sdraiata (origine al perno delle ganasce).</summary>
        public static DoodleArt V75B_Cutters()
        {
            var p = new DoodlePainter(-0.48f, -0.3f, 1.18f, 0.3f, 90) { Ink = 0.03f };
            p.Shape(Sd.Capsule(0.05f, 0.04f, 0.5f, 0.12f, 0.03f), V75BSteel, 0.022f);
            p.Shape(Sd.Capsule(0.05f, -0.04f, 0.5f, -0.12f, 0.03f), V75BSteel, 0.022f);
            p.Shape(Sd.Capsule(0.5f, 0.12f, 1.08f, 0.2f, 0.055f), Rgba.Hex("D9473B"), 0.025f);
            p.Shape(Sd.Capsule(0.5f, -0.12f, 1.08f, -0.2f, 0.055f), Rgba.Hex("D9473B"), 0.025f);
            p.Fill(Sd.Capsule(0.55f, 0.15f, 1.0f, 0.21f, 0.012f), Rgba.White.WithAlpha(0.4f));
            V75B_CutterJaws(p);
            return new DoodleArt("v75_b_cutters", p);
        }

        /// <summary>La tronchese piegata (stessa origine): i manici storti a zig-zag, le ganasce intatte.</summary>
        public static DoodleArt V75B_CuttersBent()
        {
            var p = new DoodlePainter(-0.48f, -0.5f, 1.05f, 0.5f, 90) { Ink = 0.03f };
            p.Shape(Sd.Polyline(0.03f, 0.05f, 0.04f, 0.32f, 0.16f, 0.48f, 0.32f), V75BSteel, 0.022f);
            p.Shape(Sd.Polyline(0.03f, 0.05f, -0.04f, 0.3f, -0.18f, 0.45f, -0.36f), V75BSteel, 0.022f);
            p.Shape(Sd.Polyline(0.055f, 0.48f, 0.32f, 0.72f, 0.36f, 0.95f, 0.28f), Rgba.Hex("D9473B"), 0.025f);
            p.Shape(Sd.Polyline(0.055f, 0.45f, -0.36f, 0.7f, -0.42f, 0.92f, -0.3f), Rgba.Hex("D9473B"), 0.025f);
            V75B_CutterJaws(p);
            return new DoodleArt("v75_b_cutters_bent", p);
        }

        /// <summary>Corpo, testa e collo del piccione (origine ai piedi; guarda a sinistra).</summary>
        static void V75B_PigeonBody(DoodlePainter p, bool flying)
        {
            Sdf tail = Sd.Polygon(0.12f, 0.16f, 0.32f, 0.1f, 0.33f, 0.2f, 0.14f, 0.24f);
            p.Shape(tail, V75BPigeonShade, 0.025f);
            Sdf body = Sd.Ellipse(0.02f, 0.2f, 0.19f, 0.13f);
            Body(p, body, V75BPigeon, V75BPigeonShade, null, -0.03f, 0.04f);
            // collo iridescente e testa
            p.Shape(Sd.Ellipse(-0.1f, 0.31f, 0.09f, 0.1f), V75BPigeonNeck, 0.025f);
            p.Fill(Sd.Ellipse(-0.08f, 0.27f, 0.05f, 0.04f), Rgba.Hex("A57FC0", 0.7f));
            p.Shape(Sd.Circle(-0.14f, 0.4f, 0.075f), V75BPigeon, 0.025f);
            p.Shape(Sd.Polygon(-0.2f, 0.42f, -0.28f, 0.39f, -0.2f, 0.38f), Rgba.Hex("3B3340"), 0.015f);
            p.Fill(Sd.Circle(-0.205f, 0.425f, 0.012f), Rgba.White);
            p.Shape(Sd.Circle(-0.15f, 0.42f, 0.022f), Rgba.Hex("F28A2E"), 0.012f);
            p.Fill(Sd.Circle(-0.153f, 0.422f, 0.009f), Rgba.Hex("2B2233"));
            if (!flying)
            {
                // l'ala chiusa con le due bande scure
                p.Shape(Sd.Ellipse(0.06f, 0.2f, 0.13f, 0.075f), Rgba.Hex("A9B2C2"), 0.022f);
                p.Line(Rgba.Hex("4A4F5C", 0.8f), 0.02f, 0.05f, 0.18f, 0.12f, 0.16f);
                p.Line(Rgba.Hex("4A4F5C", 0.8f), 0.02f, 0.08f, 0.23f, 0.15f, 0.21f);
                // le zampette rosa
                foreach (float x in new[] { -0.04f, 0.05f }) p.Line(Rgba.Hex("E57F8A"), 0.02f, x, 0.08f, x, 0.005f, x - 0.04f, 0.0f);
            }
            else
            {
                foreach (float x in new[] { -0.02f, 0.06f }) p.Line(Rgba.Hex("E57F8A"), 0.02f, x, 0.09f, x + 0.04f, 0.04f);
            }
        }

        /// <summary>Il piccione seduto (origine ai piedi, sulla cima del palo).</summary>
        public static DoodleArt V75B_Pigeon()
        {
            var p = new DoodlePainter(-0.32f, -0.03f, 0.36f, 0.5f, 110) { Ink = 0.03f };
            V75B_PigeonBody(p, false);
            return new DoodleArt("v75_b_pigeon", p);
        }

        /// <summary>Il piccione in volo (stessa origine): le due ali alzate.</summary>
        public static DoodleArt V75B_PigeonFly()
        {
            var p = new DoodlePainter(-0.36f, -0.03f, 0.42f, 0.78f, 110) { Ink = 0.03f };
            p.Shape(Sd.Polygon(0.0f, 0.24f, 0.18f, 0.72f, 0.3f, 0.66f, 0.14f, 0.22f), V75BPigeonShade, 0.025f);
            V75B_PigeonBody(p, true);
            Sdf wing = Sd.Polygon(-0.04f, 0.24f, -0.02f, 0.74f, 0.1f, 0.7f, 0.13f, 0.24f);
            p.Shape(wing, Rgba.Hex("A9B2C2"), 0.025f);
            p.Line(Rgba.Hex("4A4F5C", 0.8f), 0.02f, 0.0f, 0.5f, 0.11f, 0.48f);
            p.Line(Rgba.Hex("4A4F5C", 0.8f), 0.02f, 0.0f, 0.6f, 0.1f, 0.58f);
            return new DoodleArt("v75_b_pigeon_fly", p);
        }
    }
}
