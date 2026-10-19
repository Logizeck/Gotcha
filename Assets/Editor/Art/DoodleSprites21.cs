using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli nuovi della versione 0.39 (gruppo D).</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV39D() => new DoodleArt[]
        {
            KnockBackdrop(), SecretDoor(), KnockSign(), PeepEye(), PeepPupil(),
            BedroomBackdrop(), KidHead(0), KidHead(1), KidHead(2), KidBlanket(),
            VaseRoomBackdrop(), WallShelf(), FancyVase(), VaseShard(0), VaseShard(1), VaseShard(2), VaseShard(3), VaseShard(4),
            VaseOutline(), VaseGlue(),
            WordsBackdrop(), RiddleBoard(), LetterTile(), LetterSlot(),
            RoadBackdrop(), Car(false), Car(true), Truck(), Chick(), ChickFlat(), MotherHen(), ChickFeather(),
        };

        static readonly Rgba D21Ink = DoodlePainter.DefaultInk;

        // ---------- Knock knock ----------

        /// <summary>Vicolo di notte con muro di mattoni (coordinate del livello; marciapiede sotto y = -2.9).</summary>
        public static DoodleArt KnockBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            Sdf wall = Sd.Box(0, 2.1f, 5f, 5f);
            p.FillFunc(wall, (x, y) => Rgba.Lerp(Rgba.Hex("5E3A3F"), Rgba.Hex("7A4A45"), Clamp((y + 2.9f) / 8f)));
            // mattoni sfalsati
            for (int row = 0; row < 26; row++)
            {
                float y0 = -2.9f + row * 0.36f;
                float shift = row % 2 == 0 ? 0f : 0.35f;
                for (float x = -4.6f + shift; x < 4.6f; x += 0.7f)
                {
                    float n = Noise.Value(x * 3.1f, y0 * 2.3f);
                    Rgba c = Rgba.Lerp(Rgba.Hex("8E5048"), Rgba.Hex("A35E4F"), n);
                    p.Fill(Sd.Box(x + 0.35f, y0 + 0.18f, 0.31f, 0.14f, 0.03f), c.WithAlpha(0.9f));
                }
            }
            // alone del lampione
            p.FillFunc(Sd.Circle(-2.1f, 1.9f, 2.2f), (x, y) =>
                Rgba.Hex("FFD27A").WithAlpha((float)Math.Exp(-Math.Sqrt((x + 2.1f) * (x + 2.1f) + (y - 1.9f) * (y - 1.9f)) * 1.6f) * 0.55f));
            // lampada a muro
            p.InkLine(0.05f, -2.1f, 2.7f, -2.1f, 2.35f);
            Body(p, Sd.Polygon(-2.4f, 2.35f, -1.8f, 2.35f, -1.95f, 2.65f, -2.25f, 2.65f), Rgba.Hex("3B3A46"), Rgba.Hex("26252E"));
            p.Shape(Sd.Circle(-2.1f, 2.22f, 0.14f), Rgba.Hex("FFF1B0"), 0.04f);
            // marciapiede
            Sdf ground = Sd.Box(0, -5f, 5f, 2.1f);
            p.Fill(ground, Rgba.Hex("4A4655"), false);
            foreach (float y in new[] { -3.5f, -4.3f, -5.3f }) p.Line(D21Ink.WithAlpha(0.45f), 0.03f, -4.5f, y, 4.5f, y + 0.02f);
            for (float x = -4f; x < 4.5f; x += 1.3f) p.Line(D21Ink.WithAlpha(0.35f), 0.03f, x, -2.95f, x - 0.3f, -3.5f);
            p.Shape(Sd.Box(0, -2.92f, 5f, 0.07f), Rgba.Hex("6B6677"));
            // gradino davanti alla porta
            Body(p, Sd.Box(0, -3.05f, 1.6f, 0.13f, 0.03f), Rgba.Hex("8B8698"), Rgba.Hex("6B6677"));
            return new DoodleArt("knock_bg", p);
        }

        /// <summary>Porta segreta di legno scuro con spioncino (cardini in x = 0, larga 2.2, alta 3.6; spioncino in 1.1, 1.05).</summary>
        public static DoodleArt SecretDoor()
        {
            var p = new DoodlePainter(-0.08f, -1.87f, 2.28f, 1.87f, 100) { Ink = 0.055f };
            Rgba wood = Rgba.Hex("9A5A2E"), shade = Rgba.Hex("6E3A1A");
            Sdf board = Sd.Box(1.1f, 0, 1.1f, 1.8f, 0.07f);
            Body(p, board, wood, shade, null, -0.05f, 0.05f);
            foreach (float x in new[] { 0.44f, 0.88f, 1.32f, 1.76f }) p.Line(D21Ink.WithAlpha(0.75f), 0.025f, x, -1.75f, x + 0.01f, 1.75f);
            p.Line(shade, 0.02f, 0.2f, 0.5f, 0.25f, 0.1f, 0.22f, -0.3f);
            p.Line(shade, 0.02f, 1.0f, -0.5f, 0.97f, -0.9f);
            p.Line(shade, 0.02f, 1.55f, 0.4f, 1.6f, 0.0f);
            p.Line(shade, 0.02f, 2.0f, -1.0f, 1.96f, -1.4f);
            // fasce di ferro con borchie
            foreach (float y in new[] { 1.45f, -1.35f })
            {
                Sdf strap = Sd.Box(1.1f, y, 1.06f, 0.1f, 0.02f);
                Body(p, strap, Rgba.Hex("4A4655"), Rgba.Hex("2F2C38"));
                for (float x = 0.2f; x < 2.1f; x += 0.38f) p.Shape(Sd.Circle(x, y, 0.035f), Rgba.Hex("9C98AA"), 0.018f);
            }
            // spioncino d'ottone (chiuso, scuro)
            p.Shape(Sd.Circle(1.1f, 1.05f, 0.21f), Gold);
            p.Fill(Sd.Crescent(Sd.Circle(1.1f, 1.05f, 0.21f), -0.03f, 0.03f), GoldShade);
            p.Shape(Sd.Circle(1.1f, 1.05f, 0.13f), Rgba.Hex("1E1A24"), 0.03f);
            p.Fill(Sd.Circle(1.06f, 1.09f, 0.03f), Rgba.White.WithAlpha(0.35f));
            // battiporta e pomello
            p.Shape(Sd.Circle(1.9f, 0.05f, 0.13f), Gold);
            p.Fill(Sd.Circle(1.86f, 0.09f, 0.035f), GoldLight);
            return new DoodleArt("secret_door", p);
        }

        /// <summary>Cartello di legno appeso a due cordini (centro della tavola in 0,0, 1.8 x 0.76).</summary>
        public static DoodleArt KnockSign()
        {
            var p = new DoodlePainter(-0.98f, -0.45f, 0.98f, 0.78f, 120) { Ink = 0.04f };
            p.InkLine(0.025f, -0.65f, 0.3f, 0f, 0.68f, 0.65f, 0.3f);
            p.Shape(Sd.Circle(0, 0.68f, 0.05f), Rgba.Hex("9C98AA"), 0.02f);
            Sdf plank = Sd.Box(0, 0, 0.9f, 0.38f, 0.06f);
            Body(p, plank, Rgba.Hex("F2D29C"), Rgba.Hex("D6AE6E"), null, -0.04f, 0.05f);
            p.Line(Rgba.Hex("D6AE6E"), 0.015f, -0.75f, -0.25f, -0.4f, -0.27f);
            p.Line(Rgba.Hex("D6AE6E"), 0.015f, 0.45f, 0.28f, 0.75f, 0.26f);
            foreach (float x in new[] { -0.65f, 0.65f }) p.Shape(Sd.Circle(x, 0.28f, 0.03f), Rgba.Hex("9C98AA"), 0.015f);
            return new DoodleArt("knock_sign", p);
        }

        /// <summary>Occhio giallo (del mostro) che guarda dallo spioncino aperto (centro 0,0, raggio 0.14).</summary>
        public static DoodleArt PeepEye()
        {
            var p = new DoodlePainter(-0.16f, -0.16f, 0.16f, 0.16f, 300) { Ink = 0.025f, Wobble = 0.004f };
            p.Shape(Sd.Circle(0, 0, 0.13f), Rgba.Hex("FFF27A"));
            p.Fill(Sd.Crescent(Sd.Circle(0, 0, 0.13f), -0.02f, 0.02f), Rgba.Hex("E8C93A"));
            p.Shape(Sd.Circle(0, 0, 0.13f), Rgba.Clear);
            p.Line(Rgba.Hex("D85A3A", 0.6f), 0.008f, -0.11f, 0.02f, -0.07f, 0.0f);
            p.Line(Rgba.Hex("D85A3A", 0.6f), 0.008f, 0.11f, -0.03f, 0.07f, -0.02f);
            return new DoodleArt("peep_eye", p);
        }

        /// <summary>Pupilla dell'occhio dello spioncino (centro 0,0).</summary>
        public static DoodleArt PeepPupil()
        {
            var p = new DoodlePainter(-0.06f, -0.06f, 0.06f, 0.06f, 300) { Wobble = 0f };
            p.Fill(Sd.Circle(0, 0, 0.05f), D21Ink);
            p.Fill(Sd.Circle(-0.015f, 0.02f, 0.015f), Rgba.White);
            return new DoodleArt("peep_pupil", p);
        }

        // ---------- Screen time ----------

        /// <summary>Cameretta di notte col letto (coordinate del livello; il cuscino in 0,-0.75, pavimento sotto y = -3.2).</summary>
        public static DoodleArt BedroomBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            Sdf wall = Sd.Box(0, 2f, 5f, 5.2f);
            p.FillFunc(wall, (x, y) => Rgba.Lerp(Rgba.Hex("1E2350"), Rgba.Hex("2E3770"), Clamp((y + 3f) / 8f)));
            // carta da parati a stelline
            for (float y = -2.8f; y < 7f; y += 0.7f)
                for (float x = -4.4f + (((int)((y + 3f) / 0.7f)) % 2) * 0.35f; x < 4.5f; x += 0.7f)
                    p.Fill(Sd.Circle(x, y, 0.035f), Rgba.Hex("4A5596"), false);
            // finestra con la luna
            Sdf glass = Sd.Box(-1.6f, 2.0f, 0.65f, 0.8f, 0.05f);
            p.Shape(Sd.Box(-1.6f, 2.0f, 0.8f, 0.95f, 0.08f), Rgba.Hex("3C4687"));
            p.FillFunc(glass, (x, y) => Rgba.Lerp(Rgba.Hex("0C1130"), Rgba.Hex("1E2A66"), Clamp((y - 1.2f) / 1.6f)));
            p.Shape(Sd.Subtract(Sd.Circle(-1.4f, 2.35f, 0.26f), Sd.Circle(-1.27f, 2.45f, 0.22f)), Rgba.Hex("FFE680"), 0.035f);
            p.Fill(Sd.Circle(-1.95f, 2.5f, 0.03f), Rgba.Hex("FFF4B8"));
            p.Fill(Sd.Circle(-1.85f, 1.55f, 0.025f), Rgba.Hex("FFF4B8"));
            p.Shape(glass, Rgba.Clear, 0.04f);
            p.InkLine(0.04f, -1.6f, 1.2f, -1.6f, 2.8f);
            p.InkLine(0.04f, -2.25f, 2.0f, -0.95f, 2.0f);
            // poster con la stellina
            Body(p, Sd.Box(1.7f, 2.2f, 0.5f, 0.62f, 0.03f), Rgba.Hex("3E9BDB"), Rgba.Hex("2C78B0"));
            p.Shape(Sd.Polygon(1.7f, 2.55f, 1.8f, 2.3f, 2.05f, 2.28f, 1.86f, 2.1f, 1.92f, 1.85f, 1.7f, 2.0f, 1.48f, 1.85f, 1.54f, 2.1f, 1.35f, 2.28f, 1.6f, 2.3f), Rgba.Hex("FFD24A"), 0.03f);
            // pavimento
            Sdf floor = Sd.Box(0, -5.1f, 5f, 1.9f);
            p.Fill(floor, Rgba.Hex("3A2C3E"), false);
            foreach (float y in new[] { -3.7f, -4.4f, -5.3f }) p.Line(D21Ink.WithAlpha(0.5f), 0.03f, -4.5f, y, 4.5f, y + 0.02f);
            p.Shape(Sd.Box(0, -3.2f, 5f, 0.08f), Rgba.Hex("2A2030"));
            // testiera del letto
            Sdf head = Sd.Union(Sd.Box(0, -1.1f, 1.95f, 1.15f, 0.1f), Sd.Ellipse(0, -0.05f, 1.95f, 0.45f));
            Body(p, head, Rgba.Hex("8A5A3B"), Rgba.Hex("6A422A"));
            foreach (float x in new[] { -1.2f, -0.4f, 0.4f, 1.2f }) p.Line(D21Ink.WithAlpha(0.5f), 0.03f, x, 0.3f, x, -1.8f);
            // materasso e cuscino
            Body(p, Sd.Box(0, -2.55f, 2.2f, 0.65f, 0.15f), Rgba.Hex("D8DEF5"), Rgba.Hex("A9B2D8"));
            Body(p, Sd.Ellipse(0, -0.85f, 1.05f, 0.38f), Rgba.Hex("E9ECFA"), Rgba.Hex("B9C0E0"));
            // gambe del letto
            foreach (float x in new[] { -2.0f, 2.0f }) p.Shape(Sd.Box(x, -3.35f, 0.12f, 0.2f, 0.03f), Rgba.Hex("6A422A"));
            return new DoodleArt("bedroom_bg", p);
        }

        /// <summary>Testa del bambino a letto (centro della faccia 0,0). 0 = sveglio illuminato dallo schermo, 1 = assonnato, 2 = addormentato.</summary>
        public static DoodleArt KidHead(int state)
        {
            var p = new DoodlePainter(-0.72f, -0.62f, 0.72f, 0.82f, 150) { Ink = 0.04f };
            bool lit = state < 2;
            Rgba skin = lit ? Rgba.Hex("C9E2FF") : Rgba.Hex("D8A988"), skinShade = lit ? Rgba.Hex("8FB3E6") : Rgba.Hex("B88566");
            Rgba hair = lit ? Rgba.Hex("5A4A7A") : Rgba.Hex("5A3A2A");
            // orecchie
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Circle(s * 0.5f, -0.02f, 0.12f), skin, skinShade);
            Sdf face = Sd.Ellipse(0, -0.02f, 0.5f, 0.52f);
            Body(p, face, skin, skinShade, null, -0.05f, 0.06f);
            // capelli spettinati
            Sdf hairS = Sd.Union(Sd.Intersect(Sd.Ellipse(0, 0.1f, 0.54f, 0.5f), Sd.Box(0, 0.6f, 1f, 0.35f)),
                                 Sd.Polygon(-0.35f, 0.4f, -0.2f, 0.78f, -0.05f, 0.45f, 0.08f, 0.8f, 0.2f, 0.45f, 0.42f, 0.7f, 0.4f, 0.35f),
                                 Sd.Circle(-0.38f, 0.32f, 0.15f), Sd.Circle(0.4f, 0.3f, 0.14f));
            Body(p, hairS, hair, Rgba.Lerp(hair, D21Ink, 0.4f));
            if (state == 0)
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    float x = s * 0.18f;
                    p.Shape(Sd.Ellipse(x, 0.02f, 0.14f, 0.16f), Rgba.White, 0.03f);
                    p.Fill(Sd.Circle(x, 0.0f, 0.075f), D21Ink);
                    p.Fill(Sd.Box(x - 0.03f, 0.04f, 0.022f, 0.03f), Rgba.Hex("8FE3FF"));   // riflesso dello schermo
                    p.Fill(Sd.Box(x + 0.025f, 0.04f, 0.018f, 0.03f), Rgba.Hex("8FE3FF"));
                    p.Line(skinShade, 0.025f, x - 0.1f, -0.18f, x, -0.21f, x + 0.1f, -0.18f);   // occhiaie
                }
                Sdf mouth = Sd.Intersect(Sd.Ellipse(0, -0.28f, 0.14f, 0.1f), Sd.Box(0, -0.33f, 0.2f, 0.06f));
                p.Shape(mouth, Rgba.Hex("7A2E3A"), 0.025f);
            }
            else if (state == 1)
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    float x = s * 0.18f;
                    Sdf eye = Sd.Intersect(Sd.Ellipse(x, 0.02f, 0.14f, 0.16f), Sd.Box(x, -0.08f, 0.2f, 0.08f));
                    p.Shape(eye, Rgba.White, 0.03f);
                    p.Fill(Sd.Intersect(Sd.Circle(x, -0.04f, 0.07f), Sd.Grow(eye, -0.01f)), D21Ink);
                    p.InkLine(0.035f, x - 0.15f, 0.0f, x + 0.15f, 0.0f);
                    p.Line(skinShade, 0.025f, x - 0.1f, -0.2f, x, -0.23f, x + 0.1f, -0.2f);
                }
                p.InkLine(0.03f, -0.1f, -0.3f, 0.1f, -0.3f);
            }
            else
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    float x = s * 0.18f;
                    p.InkLine(0.035f, x - 0.12f, 0.02f, x - 0.06f, -0.03f, x, -0.04f, x + 0.06f, -0.03f, x + 0.12f, 0.02f);
                    p.Fill(Sd.Ellipse(x * 1.3f, -0.15f, 0.08f, 0.045f), Rgba.Hex("FF7F9C", 0.5f));
                }
                p.Shape(Sd.Ellipse(0.02f, -0.3f, 0.06f, 0.07f), Rgba.Hex("7A2E3A"), 0.025f);
            }
            return new DoodleArt(state == 0 ? "kid_awake" : state == 1 ? "kid_drowsy" : "kid_asleep", p);
        }

        /// <summary>Coperta a stelline (bordo superiore intorno a y = 0.25, centro x = 0).</summary>
        public static DoodleArt KidBlanket()
        {
            var p = new DoodlePainter(-2.15f, -1.75f, 2.15f, 0.55f, 100) { Ink = 0.05f };
            Sdf blanket = Sd.Union(Sd.Box(0, -0.8f, 2.08f, 0.9f, 0.12f),
                                   Sd.Ellipse(-0.75f, 0.0f, 0.75f, 0.32f), Sd.Ellipse(0.75f, 0.0f, 0.75f, 0.32f), Sd.Ellipse(0, -0.05f, 0.6f, 0.3f));
            Body(p, blanket, Rgba.Hex("5C7BE0"), Rgba.Hex("4560B8"));
            p.Shape(Sd.Box(0, 0.08f, 2.0f, 0.12f, 0.1f), Rgba.Hex("E9ECFA"), 0.035f);   // risvolto del lenzuolo
            var rnd = new Random(39);
            for (int i = 0; i < 14; i++)
            {
                float x = (float)rnd.NextDouble() * 3.8f - 1.9f, y = -0.25f - (float)rnd.NextDouble() * 1.35f;
                p.Fill(Sd.Polygon(x, y + 0.08f, x + 0.025f, y + 0.025f, x + 0.08f, y, x + 0.025f, y - 0.025f, x, y - 0.08f, x - 0.025f, y - 0.025f, x - 0.08f, y, x - 0.025f, y + 0.025f), Rgba.Hex("FFE680", 0.85f));
            }
            p.Line(D21Ink.WithAlpha(0.4f), 0.025f, -0.6f, -0.3f, -0.7f, -1.5f);
            p.Line(D21Ink.WithAlpha(0.4f), 0.025f, 0.7f, -0.3f, 0.8f, -1.5f);
            return new DoodleArt("kid_blanket", p);
        }

        // ---------- Il vaso rotto ----------

        /// <summary>Soggiorno: carta da parati e pavimento di legno (coordinate del livello; pavimento sotto y = -2.4).</summary>
        public static DoodleArt VaseRoomBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            Sdf wall = Sd.Box(0, 2.3f, 5f, 4.8f);
            p.Fill(wall, Rgba.Hex("E9F2DC"), false);
            for (float x = -4.4f; x < 4.5f; x += 0.6f) p.Fill(Sd.Box(x, 2.3f, 0.12f, 4.8f), Rgba.Hex("D6E6C4"), false);
            for (float y = -2f; y < 7f; y += 0.9f)
                for (float x = -4.1f; x < 4.5f; x += 1.2f)
                    p.Fill(Sd.Circle(x + ((int)((y + 2f) / 0.9f) % 2) * 0.6f, y, 0.06f), Rgba.Hex("B9D3A1"), false);
            // zoccolo di legno
            Body(p, Sd.Box(0, -1.95f, 5f, 0.45f), Rgba.Hex("C98B5A"), Rgba.Hex("A86E48"));
            p.Line(D21Ink.WithAlpha(0.5f), 0.03f, -4.5f, -1.62f, 4.5f, -1.62f);
            Sdf floor = Sd.Box(0, -4.8f, 5f, 2.4f);
            p.Fill(floor, Rgba.Hex("D9A06A"), false);
            foreach (float y in new[] { -2.95f, -3.6f, -4.4f, -5.4f }) p.Line(D21Ink.WithAlpha(0.4f), 0.03f, -4.5f, y, 4.5f, y + 0.03f);
            float[] seams = { -3.1f, -0.7f, 1.6f, -2.0f, 0.4f, 2.6f };
            for (int i = 0; i < seams.Length; i++)
            {
                float y0 = i < 3 ? -2.4f : -2.95f, y1 = i < 3 ? -2.95f : -3.6f;
                p.Line(D21Ink.WithAlpha(0.4f), 0.03f, seams[i], y0, seams[i], y1);
            }
            p.Shape(Sd.Box(0, -2.4f, 5f, 0.08f), Rgba.Hex("8C4A22"));
            // tappeto
            Body(p, Sd.Ellipse(-0.3f, -3.55f, 2.3f, 0.75f), Rgba.Hex("E58A9B"), Rgba.Hex("C76A7C"));
            p.Shape(Sd.Ellipse(-0.3f, -3.55f, 1.8f, 0.5f), Rgba.Hex("F7C6CF"), 0.035f);
            return new DoodleArt("vase_room_bg", p);
        }

        /// <summary>Mensola a muro con due staffe (piano in y = 0, larga 1.8, centro x = 0).</summary>
        public static DoodleArt WallShelf()
        {
            var p = new DoodlePainter(-1f, -0.55f, 1f, 0.12f, 110) { Ink = 0.045f };
            foreach (float x in new[] { -0.6f, 0.6f })
                Body(p, Sd.Polygon(x - 0.06f, -0.04f, x + 0.06f, -0.04f, x + 0.06f, -0.45f), Rgba.Hex("8D93A6"), Rgba.Hex("666B7D"));
            Body(p, Sd.Box(0, -0.04f, 0.9f, 0.07f, 0.02f), Rgba.Hex("C98B5A"), Rgba.Hex("A86E48"));
            return new DoodleArt("wall_shelf", p);
        }

        // geometria del vaso (centro 0,0; alto 2.15, largo 1.44) e dei tagli dei cocci
        static Sdf D21VaseShape() => Sd.Union(Sd.Ellipse(0, -0.3f, 0.72f, 0.68f), Sd.Box(0, 0.58f, 0.3f, 0.42f, 0.05f),
                                              Sd.Box(0, 0.98f, 0.44f, 0.09f, 0.05f), Sd.Box(0, -0.98f, 0.38f, 0.08f, 0.03f));

        static float D21Pw(float[] xs, float[] ys, float x)
        {
            if (x <= xs[0]) return ys[0];
            for (int i = 1; i < xs.Length; i++)
                if (x <= xs[i]) return ys[i - 1] + (ys[i] - ys[i - 1]) * (x - xs[i - 1]) / (xs[i] - xs[i - 1]);
            return ys[ys.Length - 1];
        }

        static readonly float[] D21H1x = { -1f, -0.3f, -0.05f, 0.25f, 1f }, D21H1y = { 0.36f, 0.48f, 0.3f, 0.44f, 0.34f };
        static readonly float[] D21H2x = { -1f, -0.35f, 0.05f, 0.4f, 1f }, D21H2y = { -0.5f, -0.32f, -0.58f, -0.4f, -0.55f };
        static readonly float[] D21V1y = { -1f, -0.4f, -0.1f, 0.15f, 0.6f }, D21V1x = { 0.05f, 0.05f, 0.2f, -0.1f, 0.1f };
        static readonly float[] D21V2y = { -1.2f, -0.85f, -0.6f, 0f }, D21V2x = { -0.15f, 0.12f, -0.05f, -0.05f };
        static float D21H1(float x) => D21Pw(D21H1x, D21H1y, x);
        static float D21H2(float x) => D21Pw(D21H2x, D21H2y, x);
        static float D21V1(float y) => D21Pw(D21V1y, D21V1x, y);
        static float D21V2(float y) => D21Pw(D21V2y, D21V2x, y);

        /// <summary>Zona del coccio i: 0 collo, 1 pancia sinistra, 2 pancia destra, 3 fondo sinistro, 4 fondo destro.</summary>
        static Sdf D21ShardZone(int i)
        {
            switch (i)
            {
                case 0: return (x, y) => D21H1(x) - y;
                case 1: return (x, y) => Math.Max(Math.Max(y - D21H1(x), D21H2(x) - y), x - D21V1(y));
                case 2: return (x, y) => Math.Max(Math.Max(y - D21H1(x), D21H2(x) - y), D21V1(y) - x);
                case 3: return (x, y) => Math.Max(y - D21H2(x), x - D21V2(y));
                default: return (x, y) => Math.Max(y - D21H2(x), D21V2(y) - x);
            }
        }

        /// <summary>Dipinge il vaso decorato, ritagliato dentro clip.</summary>
        static void D21PaintVase(DoodlePainter p, Sdf clip)
        {
            Sdf vase = D21VaseShape();
            Sdf v = Sd.Intersect(vase, clip);
            Rgba body = Rgba.Hex("5EC4D9"), shade = Rgba.Hex("3592AE");
            p.Fill(v, body);
            p.Fill(Sd.Intersect(Sd.Crescent(vase, -0.1f, 0.1f), clip), shade);
            p.Fill(Sd.Intersect(v, Sd.Box(0, -0.12f, 1f, 0.07f)), Gold);
            p.Fill(Sd.Intersect(v, Sd.Box(0, 0.7f, 1f, 0.05f)), Gold);
            p.Fill(Sd.Intersect(v, Sd.Box(0, -0.88f, 1f, 0.04f)), Gold);
            // fiorellini bianchi sulla pancia
            foreach (float[] f in new[] { new[] { -0.36f, -0.5f }, new[] { 0.32f, -0.42f }, new[] { -0.02f, -0.7f } })
            {
                for (int k = 0; k < 5; k++)
                {
                    double a = k * Math.PI * 2 / 5;
                    p.Fill(Sd.Intersect(Sd.Circle(f[0] + (float)Math.Cos(a) * 0.08f, f[1] + (float)Math.Sin(a) * 0.08f, 0.06f), v), Rgba.Hex("FFFFFF", 0.95f));
                }
                p.Fill(Sd.Intersect(Sd.Circle(f[0], f[1], 0.045f), v), Rgba.Hex("FFC93C"));
            }
            // puntini sul collo
            foreach (float y in new[] { 0.3f, 0.48f }) foreach (float x in new[] { -0.15f, 0f, 0.15f })
                    p.Fill(Sd.Intersect(Sd.Circle(x + (y > 0.4f ? 0.07f : 0f), y, 0.03f), v), Rgba.Hex("FFFFFF", 0.9f));
            // riflesso
            p.Fill(Sd.Intersect(Sd.Capsule(-0.5f, -0.05f, -0.35f, 0.12f, 0.035f), v), Rgba.White.WithAlpha(0.8f));
            p.Shape(v, Rgba.Clear);
        }

        /// <summary>Vaso decorato intero (centro 0,0; alto circa 2.15).</summary>
        public static DoodleArt FancyVase()
        {
            var p = new DoodlePainter(-0.8f, -1.12f, 0.8f, 1.12f, 120) { Ink = 0.045f };
            D21PaintVase(p, (x, y) => -1f);
            return new DoodleArt("fancy_vase", p);
        }

        /// <summary>Coccio i del vaso, nelle coordinate del vaso (il centro del disegno è il centro del coccio).</summary>
        public static DoodleArt VaseShard(int i)
        {
            Sdf s = Sd.Intersect(D21VaseShape(), D21ShardZone(i));
            float x0 = 9, y0 = 9, x1 = -9, y1 = -9;
            for (float y = -1.15f; y <= 1.15f; y += 0.01f)
                for (float x = -0.8f; x <= 0.8f; x += 0.01f)
                    if (s(x, y) < 0f) { x0 = Math.Min(x0, x); x1 = Math.Max(x1, x); y0 = Math.Min(y0, y); y1 = Math.Max(y1, y); }
            var p = new DoodlePainter(x0 - 0.06f, y0 - 0.06f, x1 + 0.06f, y1 + 0.06f, 120) { Ink = 0.045f };
            D21PaintVase(p, D21ShardZone(i));
            return new DoodleArt("vase_shard" + i, p);
        }

        /// <summary>Sagoma tratteggiata del vaso con le linee dei tagli appena accennate (coordinate del vaso).</summary>
        public static DoodleArt VaseOutline()
        {
            var p = new DoodlePainter(-0.8f, -1.12f, 0.8f, 1.12f, 120) { Wobble = 0f };
            Sdf vase = D21VaseShape();
            p.Fill(vase, Rgba.White.WithAlpha(0.18f), false);
            Sdf inside = Sd.Grow(vase, -0.03f);
            Rgba faint = D21Ink.WithAlpha(0.22f);
            p.Fill(Sd.Intersect((x, y) => Math.Abs(y - D21H1(x)) - 0.012f, inside), faint, false);
            p.Fill(Sd.Intersect((x, y) => Math.Abs(y - D21H2(x)) - 0.012f, inside), faint, false);
            p.Fill(Sd.Intersect((x, y) => Math.Max(Math.Abs(x - D21V1(y)) - 0.012f, Math.Max(y - D21H1(x), D21H2(x) - y)), inside), faint, false);
            p.Fill(Sd.Intersect((x, y) => Math.Max(Math.Abs(x - D21V2(y)) - 0.012f, y - D21H2(x)), inside), faint, false);
            p.FillFunc((x, y) => Math.Abs(vase(x, y)) - 0.025f, (x, y) =>
            {
                double a = Math.Atan2(y + 0.1f, x);
                return Math.Sin(a * 34) > -0.2 ? D21Ink.WithAlpha(0.75f) : Rgba.Clear;
            });
            return new DoodleArt("vase_outline", p);
        }

        /// <summary>Crepe incollate d'oro (kintsugi) lungo i tagli dei cocci (coordinate del vaso).</summary>
        public static DoodleArt VaseGlue()
        {
            var p = new DoodlePainter(-0.8f, -1.12f, 0.8f, 1.12f, 120) { Wobble = 0f };
            Sdf inside = Sd.Grow(D21VaseShape(), 0.01f);
            Sdf cracks = Sd.Union((x, y) => Math.Abs(y - D21H1(x)), (x, y) => Math.Abs(y - D21H2(x)),
                                  (x, y) => Math.Max(Math.Abs(x - D21V1(y)), Math.Max(y - D21H1(x), D21H2(x) - y)),
                                  (x, y) => Math.Max(Math.Abs(x - D21V2(y)), y - D21H2(x)));
            p.FillFunc(Sd.Intersect(Sd.Grow(cracks, 0.07f), inside), (x, y) =>
                Rgba.Hex("FFE680").WithAlpha((float)Math.Exp(-cracks(x, y) * 30f) * 0.7f));
            p.Fill(Sd.Intersect(Sd.Grow(cracks, 0.022f), inside), Rgba.Hex("FFC93C"), false);
            p.Fill(Sd.Intersect(Sd.Grow(cracks, 0.008f), inside), Rgba.Hex("FFF6C7"), false);
            return new DoodleArt("vase_glue", p);
        }

        // ---------- Word salad ----------

        /// <summary>Studiolo: carta da parati e scrivania (coordinate del livello; piano della scrivania sotto y = -1.1).</summary>
        public static DoodleArt WordsBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            Sdf wall = Sd.Box(0, 3f, 5f, 4.2f);
            p.Fill(wall, Rgba.Hex("2F6E68"), false);
            for (float y = -1f; y < 7.5f; y += 0.8f)
                for (float x = -4.4f; x < 4.6f; x += 0.8f)
                {
                    float cx = x + ((int)((y + 1f) / 0.8f) % 2) * 0.4f;
                    p.Fill(Sd.Polygon(cx, y + 0.14f, cx + 0.1f, y, cx, y - 0.14f, cx - 0.1f, y), Rgba.Hex("3A827B"), false);
                }
            Sdf desk = Sd.Box(0, -4.2f, 5f, 3.1f);
            p.Fill(desk, Rgba.Hex("B9773F"), false);
            for (int i = 0; i < 9; i++)
            {
                float y = -1.5f - i * 0.45f;
                p.Line(Rgba.Hex("9A5E2E", 0.6f), 0.03f, -4.5f, y, -2f, y + 0.06f, 1f, y - 0.04f, 4.5f, y + 0.03f);
            }
            p.Shape(Sd.Box(0, -1.1f, 5f, 0.1f), Rgba.Hex("D18C50"));
            return new DoodleArt("words_bg", p);
        }

        /// <summary>Foglio dell'indovinello appeso con lo scotch (centro 0,0; 5 x 1.9).</summary>
        public static DoodleArt RiddleBoard()
        {
            var p = new DoodlePainter(-2.65f, -1.08f, 2.65f, 1.1f, 90) { Ink = 0.05f };
            Sdf paper = Sd.Box(0, 0, 2.5f, 0.95f, 0.08f);
            Body(p, paper, Rgba.Hex("FFF8E6"), Rgba.Hex("EBDDBA"), null, -0.05f, 0.06f);
            for (float y = -0.6f; y < 0.8f; y += 0.3f) p.Line(Rgba.Hex("BFD7F0", 0.7f), 0.02f, -2.35f, y, 2.35f, y);
            p.Line(Rgba.Hex("F2A0A0", 0.7f), 0.02f, -2.1f, -0.88f, -2.1f, 0.88f);
            foreach (float s in new[] { -1f, 1f })
                p.Fill(Sd.Rotate(Sd.Box(s * 2.3f, 0.85f, 0.32f, 0.1f), s * 2.3f, 0.85f, -s * 30f), Rgba.Hex("F7E9A8", 0.85f));
            return new DoodleArt("riddle_board", p);
        }

        /// <summary>Tessera di legno per le lettere (centro 0,0; 0.7 x 0.7).</summary>
        public static DoodleArt LetterTile()
        {
            var p = new DoodlePainter(-0.4f, -0.42f, 0.4f, 0.4f, 160) { Ink = 0.035f };
            p.Shape(Sd.Box(0, -0.04f, 0.34f, 0.34f, 0.08f), Rgba.Hex("C9955A"));
            Body(p, Sd.Box(0, 0.02f, 0.34f, 0.34f, 0.08f), Rgba.Hex("F7E2B5"), Rgba.Hex("E2C38B"), null, -0.04f, 0.04f);
            p.Line(Rgba.Hex("E2C38B"), 0.012f, -0.22f, -0.24f, 0.05f, -0.26f);
            return new DoodleArt("letter_tile", p);
        }

        /// <summary>Casella vuota tratteggiata per una lettera (centro 0,0; 0.72 x 0.72).</summary>
        public static DoodleArt LetterSlot()
        {
            var p = new DoodlePainter(-0.4f, -0.4f, 0.4f, 0.4f, 160) { Wobble = 0f };
            Sdf box = Sd.Box(0, 0, 0.35f, 0.35f, 0.08f);
            p.Fill(box, Rgba.Hex("FFFFFF", 0.22f), false);
            p.FillFunc((x, y) => Math.Abs(box(x, y)) - 0.018f, (x, y) =>
            {
                float t = Math.Abs(x) > Math.Abs(y) ? y : x;
                return Math.Sin(t * 42f) > -0.3 ? Rgba.Hex("FFF8E6", 0.9f) : Rgba.Clear;
            });
            return new DoodleArt("letter_slot", p);
        }

        // ---------- Why the chicken ----------

        /// <summary>
        /// Strada a quattro corsie vista dall'alto (coordinate del livello): prato di partenza sotto y = -3.05, corsie
        /// centrate in -2.5, -1.4, aiuola di mezzo -0.85..0.25, corsie 0.8, 1.9, prato d'arrivo sopra 2.45.
        /// </summary>
        public static DoodleArt RoadBackdrop()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.Fill(Sd.Box(0, 0, 5f, 7f), Rgba.Hex("7CCB5E"), false);
            var rnd = new Random(5);
            for (int i = 0; i < 90; i++)
            {
                float x = (float)rnd.NextDouble() * 9f - 4.5f, y = (float)rnd.NextDouble() * 14f - 7f;
                p.Line(Rgba.Hex("5DAE45"), 0.03f, x - 0.06f, y, x, y + 0.1f, x + 0.06f, y);
            }
            foreach (float cy in new[] { -1.95f, 1.35f })
            {
                Sdf road = Sd.Box(0, cy, 5f, 1.1f);
                p.Fill(road, Rgba.Hex("55525F"), false);
                p.Hatch(road, Rgba.Hex("4B4855", 0.6f), 0.35f, 0.06f, 20f);
                for (float x = -4.4f; x < 4.6f; x += 0.9f) p.Fill(Sd.Box(x, cy, 0.25f, 0.04f), Rgba.Hex("FFE680"), false);
                foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Box(0, cy + s * 1.12f, 5f, 0.06f), Rgba.Hex("D9D6E0"), 0.04f);
            }
            // fiorellini nei prati
            foreach (float[] f in new[] { new[] { -2.2f, -3.8f }, new[] { 1.9f, -4.1f }, new[] { -1.1f, -0.2f }, new[] { 2.3f, -0.5f }, new[] { -2.4f, 3.2f }, new[] { 1.2f, 3.6f } })
            {
                for (int k = 0; k < 5; k++)
                {
                    double a = k * Math.PI * 2 / 5;
                    p.Fill(Sd.Circle(f[0] + (float)Math.Cos(a) * 0.07f, f[1] + (float)Math.Sin(a) * 0.07f, 0.055f), Rgba.White);
                }
                p.Fill(Sd.Circle(f[0], f[1], 0.04f), Rgba.Hex("FFC93C"));
            }
            return new DoodleArt("road_bg", p);
        }

        /// <summary>Automobilina vista di lato, muso verso destra (centro 0,0; lunga 1.2).</summary>
        public static DoodleArt Car(bool blue)
        {
            var p = new DoodlePainter(-0.66f, -0.38f, 0.66f, 0.42f, 120) { Ink = 0.04f };
            Rgba c = blue ? Rgba.Hex("4AA3DF") : Rgba.Hex("E8443A"), cs = blue ? Rgba.Hex("2F7DB5") : Rgba.Hex("B92E2A");
            Sdf cabin = Sd.Polygon(-0.38f, 0.05f, -0.25f, 0.34f, 0.18f, 0.34f, 0.36f, 0.05f);
            Body(p, cabin, c, cs);
            p.Shape(Sd.Polygon(-0.27f, 0.08f, -0.19f, 0.27f, -0.05f, 0.27f, -0.05f, 0.08f), Rgba.Hex("BFE8FF"), 0.025f);
            p.Shape(Sd.Polygon(0.02f, 0.08f, 0.02f, 0.27f, 0.14f, 0.27f, 0.27f, 0.08f), Rgba.Hex("BFE8FF"), 0.025f);
            Sdf body = Sd.Box(0, -0.06f, 0.6f, 0.15f, 0.09f);
            Body(p, body, c, cs);
            p.Shape(Sd.Box(0.54f, -0.02f, 0.05f, 0.04f, 0.02f), Rgba.Hex("FFF27A"), 0.02f);
            p.Shape(Sd.Box(-0.57f, -0.02f, 0.03f, 0.035f, 0.015f), Rgba.Hex("FF8A7A"), 0.02f);
            p.InkLine(0.02f, -0.02f, 0.06f, -0.02f, -0.17f);
            foreach (float x in new[] { -0.33f, 0.33f })
            {
                p.Shape(Sd.Circle(x, -0.22f, 0.13f), Rgba.Hex("3A3540"));
                p.Fill(Sd.Circle(x, -0.22f, 0.05f), Rgba.Hex("B7BCC9"));
            }
            return new DoodleArt(blue ? "car_blue" : "car_red", p);
        }

        /// <summary>Camion con rimorchio, muso verso destra (centro 0,0; lungo 2).</summary>
        public static DoodleArt Truck()
        {
            var p = new DoodlePainter(-1.05f, -0.45f, 1.05f, 0.55f, 110) { Ink = 0.045f };
            Sdf box = Sd.Box(-0.28f, 0.12f, 0.72f, 0.36f, 0.04f);
            Body(p, box, Rgba.Hex("F4F4F7"), Rgba.Hex("C9CAD3"));
            p.Fill(Sd.Box(-0.28f, 0.12f, 0.6f, 0.08f), Rgba.Hex("FF8A1F"));
            Sdf cab = Sd.Polygon(0.5f, -0.24f, 1.0f, -0.24f, 1.0f, 0.08f, 0.88f, 0.3f, 0.5f, 0.3f);
            Body(p, cab, Rgba.Hex("FFC83D"), Rgba.Hex("E09A16"));
            p.Shape(Sd.Polygon(0.66f, 0.04f, 0.92f, 0.04f, 0.84f, 0.22f, 0.66f, 0.22f), Rgba.Hex("BFE8FF"), 0.025f);
            p.Shape(Sd.Box(0.98f, -0.15f, 0.03f, 0.04f, 0.015f), Rgba.Hex("FFF27A"), 0.02f);
            foreach (float x in new[] { -0.75f, -0.45f, 0.75f })
            {
                p.Shape(Sd.Circle(x, -0.3f, 0.13f), Rgba.Hex("3A3540"));
                p.Fill(Sd.Circle(x, -0.3f, 0.05f), Rgba.Hex("B7BCC9"));
            }
            return new DoodleArt("truck", p);
        }

        static readonly Rgba D21ChickYellow = Rgba.Hex("FFE04A"), D21ChickShade = Rgba.Hex("F2B21E"), D21Beak = Rgba.Hex("FF8A1F");

        /// <summary>Pulcino visto di fronte (centro del corpo 0,0; largo circa 0.6).</summary>
        public static DoodleArt Chick()
        {
            var p = new DoodlePainter(-0.36f, -0.4f, 0.36f, 0.4f, 180) { Ink = 0.035f };
            foreach (float s in new[] { -1f, 1f })
                p.Line(D21Beak, 0.035f, s * 0.1f, -0.25f, s * 0.1f, -0.33f, s * 0.04f, -0.36f, s * 0.1f, -0.33f, s * 0.17f, -0.36f);
            foreach (float s in new[] { -1f, 1f })
                Body(p, Sd.Rotate(Sd.Ellipse(s * 0.25f, -0.03f, 0.08f, 0.14f), s * 0.25f, -0.03f, s * 25f), D21ChickYellow, D21ChickShade);
            Sdf body = Sd.Ellipse(0, -0.02f, 0.26f, 0.27f);
            Body(p, body, D21ChickYellow, D21ChickShade);
            p.Line(D21ChickYellow, 0.04f, 0f, 0.24f, -0.03f, 0.33f);
            p.InkLine(0.025f, -0.02f, 0.25f, -0.05f, 0.34f);
            p.InkLine(0.025f, 0.02f, 0.25f, 0.06f, 0.33f);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Circle(s * 0.09f, 0.06f, 0.04f), D21Ink);
                p.Fill(Sd.Circle(s * 0.09f - 0.012f, 0.075f, 0.013f), Rgba.White);
                p.Fill(Sd.Ellipse(s * 0.16f, -0.02f, 0.045f, 0.025f), Rgba.Hex("FF9BB0", 0.7f));
            }
            p.Shape(Sd.Polygon(-0.05f, 0.0f, 0.05f, 0.0f, 0f, -0.07f), D21Beak, 0.02f);
            return new DoodleArt("chick", p);
        }

        /// <summary>Pulcino spiaccicato (frittella con gli occhi a X; centro 0,0).</summary>
        public static DoodleArt ChickFlat()
        {
            var p = new DoodlePainter(-0.5f, -0.2f, 0.5f, 0.2f, 180) { Ink = 0.035f };
            foreach (float s in new[] { -1f, 1f }) p.Line(D21Beak, 0.035f, s * 0.12f, -0.08f, s * 0.2f, -0.17f);
            Sdf flat = Sd.Ellipse(0, 0, 0.45f, 0.11f);
            Body(p, flat, D21ChickYellow, D21ChickShade);
            foreach (float s in new[] { -1f, 1f })
            {
                float x = s * 0.12f;
                p.InkLine(0.022f, x - 0.035f, 0.05f, x + 0.035f, -0.01f);
                p.InkLine(0.022f, x - 0.035f, -0.01f, x + 0.035f, 0.05f);
            }
            p.Shape(Sd.Polygon(-0.05f, -0.02f, 0.05f, -0.02f, 0f, -0.07f), D21Beak, 0.02f);
            return new DoodleArt("chick_flat", p);
        }

        /// <summary>Mamma chioccia bianca vista di fronte (base in 0,-0.55; alta circa 1.2).</summary>
        public static DoodleArt MotherHen()
        {
            var p = new DoodlePainter(-0.65f, -0.62f, 0.65f, 0.72f, 150) { Ink = 0.04f };
            Rgba white = Rgba.Hex("FFFDF6"), shade = Rgba.Hex("DAD3C4"), red = Rgba.Hex("E8443A");
            foreach (float s in new[] { -1f, 1f })
                p.Line(D21Beak, 0.045f, s * 0.15f, -0.4f, s * 0.15f, -0.53f, s * 0.07f, -0.58f, s * 0.15f, -0.53f, s * 0.25f, -0.58f);
            foreach (float s in new[] { -1f, 1f })
                Body(p, Sd.Rotate(Sd.Ellipse(s * 0.45f, -0.12f, 0.14f, 0.26f), s * 0.45f, -0.12f, s * 20f), white, shade);
            Sdf body = Sd.Ellipse(0, -0.12f, 0.45f, 0.32f);
            Body(p, body, white, shade);
            p.Shape(Sd.Union(Sd.Circle(-0.08f, 0.6f, 0.07f), Sd.Circle(0.02f, 0.64f, 0.08f), Sd.Circle(0.11f, 0.58f, 0.065f)), red, 0.03f);
            Sdf head = Sd.Ellipse(0, 0.33f, 0.26f, 0.27f);
            Body(p, head, white, shade);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Circle(s * 0.1f, 0.38f, 0.045f), D21Ink);
                p.Fill(Sd.Circle(s * 0.1f - 0.014f, 0.395f, 0.014f), Rgba.White);
                p.InkLine(0.02f, s * 0.05f, 0.47f, s * 0.15f, 0.46f);
                p.Fill(Sd.Ellipse(s * 0.17f, 0.28f, 0.05f, 0.03f), Rgba.Hex("FF9BB0", 0.7f));
            }
            p.Shape(Sd.Ellipse(0, 0.2f, 0.045f, 0.06f), red, 0.02f);
            p.Shape(Sd.Polygon(-0.07f, 0.3f, 0.07f, 0.3f, 0f, 0.2f), D21Beak, 0.025f);
            p.Line(shade, 0.025f, -0.2f, -0.15f, -0.1f, -0.2f, 0f, -0.15f, 0.1f, -0.2f, 0.2f, -0.15f);
            return new DoodleArt("mother_hen", p);
        }

        /// <summary>Piumetta gialla (centro 0,0).</summary>
        public static DoodleArt ChickFeather()
        {
            var p = new DoodlePainter(-0.14f, -0.08f, 0.14f, 0.08f, 220) { Ink = 0.02f };
            p.Shape(Sd.Rotate(Sd.Ellipse(0.01f, 0, 0.11f, 0.045f), 0, 0, 10f), D21ChickYellow);
            p.InkLine(0.012f, -0.13f, -0.03f, 0.1f, 0.02f);
            return new DoodleArt("chick_feather", p);
        }
    }
}
