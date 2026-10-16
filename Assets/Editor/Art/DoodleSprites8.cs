using System;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni della versione 0.24 (scarafaggio, spray, ombrello, uccello, porte, rune...).</summary>
    public static partial class DoodleSprites
    {
        static readonly Rgba RoachBrown = Rgba.Hex("7A4A2B"), RoachShade = Rgba.Hex("54301A"), RoachShine = Rgba.Hex("A8693F");

        /// <summary>Corpo dello scarafaggio visto dall'alto, testa verso +y (centro 0,0). Le zampe sono a parte.</summary>
        static void RoachBody(DoodlePainter p, bool dead)
        {
            // antenne lunghe
            p.InkLine(0.022f, -0.07f, 0.62f, -0.2f, 0.85f, -0.42f, 1.0f, -0.6f, 1.02f);
            p.InkLine(0.022f, 0.07f, 0.62f, 0.2f, 0.85f, 0.42f, 1.0f, 0.6f, 1.02f);
            Sdf abdomen = Sd.Ellipse(0, -0.12f, 0.3f, 0.48f);
            Body(p, abdomen, RoachBrown, RoachShade, RoachShade.WithAlpha(0.35f), -0.08f, 0.06f);
            // elitre: due metà con la riga in mezzo e un riflesso lucido
            p.InkLine(0.02f, 0f, 0.28f, 0f, -0.55f);
            p.Shape(Sd.Rotate(Sd.Ellipse(-0.13f, -0.05f, 0.05f, 0.22f), -0.13f, -0.05f, -8f), RoachShine.WithAlpha(0.8f), 0f);
            Sdf thorax = Sd.Ellipse(0, 0.35f, 0.25f, 0.13f);
            Body(p, thorax, Rgba.Hex("8C5733"), RoachShade);
            Sdf head = Sd.Circle(0, 0.55f, 0.14f);
            Body(p, head, Rgba.Hex("5E3820"), RoachShade);
            if (dead)
            {
                p.InkLine(0.025f, -0.1f, 0.62f, -0.03f, 0.55f);
                p.InkLine(0.025f, -0.1f, 0.55f, -0.03f, 0.62f);
                p.InkLine(0.025f, 0.03f, 0.62f, 0.1f, 0.55f);
                p.InkLine(0.025f, 0.03f, 0.55f, 0.1f, 0.62f);
                return;
            }
            // occhi cattivi con le sopracciglia aggrottate
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Circle(s * 0.065f, 0.58f, 0.05f), Rgba.White, 0.018f);
                p.Fill(Sd.Circle(s * 0.07f, 0.6f, 0.022f), Rgba.Hex("D11F2E"));
                p.InkLine(0.025f, s * 0.12f, 0.67f, s * 0.02f, 0.62f);
            }
        }

        static void RoachLegs(DoodlePainter p, float phase)
        {
            for (int i = 0; i < 3; i++)
            {
                float y = 0.3f - i * 0.28f;
                float swing = (i % 2 == 0 ? phase : -phase) * 0.12f;
                foreach (float s in new[] { -1f, 1f })
                {
                    float sw = s < 0 ? swing : -swing;
                    p.InkLine(0.03f, s * 0.2f, y, s * 0.45f, y + 0.06f + sw, s * 0.58f, y - 0.12f + sw * 1.5f);
                }
            }
        }

        public static DoodleArt Roach()
        {
            var p = new DoodlePainter(-0.7f, -0.7f, 0.7f, 1.1f, 150) { Ink = 0.035f };
            RoachBody(p, false);
            return new DoodleArt("roach", p);
        }

        public static DoodleArt RoachLegs(int frame)
        {
            var p = new DoodlePainter(-0.7f, -0.4f, 0.7f, 0.5f, 150) { Ink = 0.03f };
            RoachLegs(p, frame == 0 ? 1f : -1f);
            return new DoodleArt(frame == 0 ? "roach_legs_a" : "roach_legs_b", p);
        }

        /// <summary>Scarafaggio stecchito: a pancia in su, zampe all'aria, occhi a X.</summary>
        public static DoodleArt RoachDead()
        {
            var p = new DoodlePainter(-0.75f, -0.7f, 0.75f, 1.1f, 150) { Ink = 0.035f };
            for (int i = 0; i < 3; i++)
                foreach (float s in new[] { -1f, 1f })
                {
                    float y = 0.25f - i * 0.25f;
                    p.InkLine(0.03f, s * 0.2f, y, s * 0.5f, y + 0.18f, s * 0.42f, y + 0.36f);
                }
            Sdf belly = Sd.Ellipse(0, -0.12f, 0.3f, 0.48f);
            Body(p, belly, Rgba.Hex("B98A5E"), Rgba.Hex("8E6440"));
            for (int i = 0; i < 4; i++) p.Line(Rgba.Hex("8E6440"), 0.02f, -0.22f, -0.35f + i * 0.16f, 0.22f, -0.35f + i * 0.16f);
            Body(p, Sd.Circle(0, 0.55f, 0.14f), Rgba.Hex("5E3820"), RoachShade);
            p.InkLine(0.025f, -0.1f, 0.62f, -0.03f, 0.53f);
            p.InkLine(0.025f, -0.1f, 0.53f, -0.03f, 0.62f);
            p.InkLine(0.025f, 0.03f, 0.62f, 0.1f, 0.53f);
            p.InkLine(0.025f, 0.03f, 0.53f, 0.1f, 0.62f);
            p.Shape(Sd.Ellipse(0.0f, 0.44f, 0.04f, 0.035f), Rgba.Hex("7BD13F"), 0.015f);   // linguaccia verde
            return new DoodleArt("roach_dead", p);
        }

        /// <summary>Bomboletta spray bianca col teschio del veleno; ugello in alto in (0, 0.1), fondo a y = -1.3.</summary>
        public static DoodleArt SprayCan()
        {
            var p = new DoodlePainter(-0.45f, -1.38f, 0.45f, 0.2f, 150) { Ink = 0.035f };
            Sdf can = Sd.Box(0, -0.72f, 0.32f, 0.58f, 0.07f);
            Body(p, can, Rgba.Hex("F4F6F8"), Rgba.Hex("C9CFD6"), null, -0.12f, 0f);
            Body(p, Sd.Ellipse(0, -0.14f, 0.32f, 0.11f), Rgba.Hex("E2E6EA"), Rgba.Hex("B9C0C8"));
            Body(p, Sd.Box(0, 0f, 0.12f, 0.09f, 0.03f), Rgba.Hex("E8433F"), Rgba.Hex("B92E2A"));   // tappo erogatore
            p.Fill(Sd.Circle(0, 0.1f, 0.03f), Ink);                                              // ugello
            // etichetta: rombo arancione col teschio e le tibie incrociate
            Sdf diamond = Sd.Polygon(0, -0.38f, 0.24f, -0.68f, 0, -0.98f, -0.24f, -0.68f);
            p.Shape(diamond, Rgba.Hex("FF8A1F"), 0.025f);
            p.InkLine(0.04f, -0.13f, -0.86f, 0.13f, -0.6f);
            p.InkLine(0.04f, 0.13f, -0.86f, -0.13f, -0.6f);
            p.Shape(Sd.Union(Sd.Circle(0, -0.66f, 0.1f), Sd.Box(0, -0.75f, 0.055f, 0.04f, 0.02f)), Rgba.White, 0.02f);
            p.Fill(Sd.Circle(-0.04f, -0.66f, 0.026f), Ink);
            p.Fill(Sd.Circle(0.04f, -0.66f, 0.026f), Ink);
            p.Line(Rgba.Hex("2B2133"), 0.04f, -0.24f, -1.12f, 0.24f, -1.12f);
            return new DoodleArt("spray_can", p);
        }
        /// <summary>Biscotto spezzato: due metà frastagliate un po' separate e briciole (centro 0,0).</summary>
        public static DoodleArt CookieBroken()
        {
            var p = new DoodlePainter(-0.45f, -0.3f, 0.45f, 0.3f, 160) { Ink = 0.035f };
            Sdf crack = Sd.Polygon(-0.02f, 0.3f, 0.04f, 0.12f, -0.03f, 0.02f, 0.05f, -0.1f, -0.01f, -0.3f, 0.4f, -0.3f, 0.4f, 0.3f);
            Sdf whole = Sd.Circle(0, 0, 0.22f);
            Sdf right = Sd.Offset(Sd.Rotate(Sd.Intersect(whole, crack), 0, 0, -12f), 0.08f, -0.03f);
            Sdf left = Sd.Offset(Sd.Rotate(Sd.Subtract(whole, crack), 0, 0, 10f), -0.08f, -0.02f);
            foreach (Sdf half in new[] { left, right })
                Body(p, half, Rgba.Hex("D99A55"), Rgba.Hex("B8773A"), null, -0.03f, 0.04f);
            foreach (var c in new[] { new[] { -0.16f, 0.06f }, new[] { -0.12f, -0.1f }, new[] { 0.14f, 0.08f }, new[] { 0.2f, -0.08f } })
                p.Fill(Sd.Circle(c[0], c[1], 0.032f), Rgba.Hex("4A2A1A"));
            foreach (var c in new[] { new[] { 0.36f, -0.2f }, new[] { -0.36f, -0.22f }, new[] { 0.3f, -0.25f }, new[] { -0.28f, -0.26f }, new[] { 0.0f, -0.25f } })
                p.Shape(Sd.Circle(c[0], c[1], 0.025f), Rgba.Hex("D99A55"), 0.012f);
            return new DoodleArt("cookie_broken", p);
        }
        /// <summary>Raggiera "da anime" (centro 0,0): spicchi chiari alternati che sfumano verso l'esterno.</summary>
        public static DoodleArt BurstRays()
        {
            var p = new DoodlePainter(-3f, -3f, 3f, 3f, 40) { Wobble = 0f };
            p.FillFunc(Sd.Circle(0, 0, 3f), (x, y) =>
            {
                double a = Math.Atan2(y, x);
                float r = (float)Math.Sqrt(x * x + y * y);
                bool on = Math.Sin(a * 14.0) > 0.15;
                float fade = Clamp(1f - r / 3f) * Clamp(r / 0.4f);
                return on ? Rgba.Hex("FFF6C2").WithAlpha(0.85f * fade) : Rgba.Hex("FFC93C").WithAlpha(0.25f * fade);
            });
            return new DoodleArt("burst_rays", p);
        }

        /// <summary>Anello dell'onda d'urto (raggio 1).</summary>
        public static DoodleArt ShockRing()
        {
            var p = new DoodlePainter(-1.15f, -1.15f, 1.15f, 1.15f, 80) { Wobble = 0f };
            p.FillFunc(Sd.Circle(0, 0, 1.15f), (x, y) =>
            {
                float r = (float)Math.Sqrt(x * x + y * y);
                float k = Clamp(1f - Math.Abs(r - 1f) / 0.12f);
                return Rgba.White.WithAlpha(k * k);
            });
            return new DoodleArt("shock_ring", p);
        }

        /// <summary>Pezzi dell'armatura che arrivano in volo durante la trasformazione (centro 0,0).</summary>
        public static DoodleArt ArmorPiece(int kind)
        {
            var p = new DoodlePainter(-0.6f, -0.6f, 0.6f, 0.6f, 120) { Ink = 0.04f };
            Rgba blue = Rgba.Hex("2F6FE0"), blueShade = Rgba.Hex("1F4FAE");
            Rgba red = Rgba.Hex("E23B3B"), redShade = Rgba.Hex("B22A2A");
            switch (kind)
            {
                case 0:   // corazza
                    Body(p, Sd.Polygon(-0.45f, 0.35f, 0.45f, 0.35f, 0.32f, -0.3f, 0f, -0.45f, -0.32f, -0.3f), blue, blueShade);
                    p.Shape(Sd.Polygon(-0.12f, 0.1f, 0.12f, 0.1f, 0f, -0.15f), Gold, 0.025f);
                    break;
                case 1:   // spallaccio
                    Body(p, Sd.Intersect(Sd.Ellipse(0, 0, 0.45f, 0.35f), Sd.Box(0, 0.12f, 0.5f, 0.3f)), red, redShade);
                    p.Line(GoldShade, 0.04f, -0.35f, 0.02f, 0.35f, 0.02f);
                    break;
                case 2:   // cresta a V
                    p.Shape(Sd.Union(Sd.Polygon(0f, -0.1f, -0.5f, 0.45f, -0.38f, 0.48f, 0.02f, 0.08f), Sd.Polygon(0f, -0.1f, 0.5f, 0.45f, 0.38f, 0.48f, -0.02f, 0.08f)), Gold, 0.03f);
                    p.Shape(Sd.Circle(0, -0.12f, 0.1f), red, 0.025f);
                    break;
                default:  // spada laser
                    p.FillFunc(Sd.Capsule(-0.5f, -0.5f, 0.45f, 0.45f, 0.12f), (x, y) =>
                    {
                        float d = DistToSeg(x, y, -0.5f, -0.5f, 0.45f, 0.45f);
                        return Rgba.Lerp(Rgba.White, Rgba.Hex("FF4FD8"), Clamp(d / 0.06f)).WithAlpha(Clamp(1.2f - d / 0.12f));
                    });
                    Body(p, Sd.Rotate(Sd.Box(-0.48f, -0.48f, 0.06f, 0.14f, 0.02f), -0.48f, -0.48f, 45f), Rgba.Hex("8D93A6"), Rgba.Hex("666B7D"));
                    break;
            }
            return new DoodleArt("armor_" + kind, p);
        }
        static readonly Rgba BirdBlue = Rgba.Hex("4AA3DF"), BirdShade = Rgba.Hex("2F7DB5"), BirdBelly = Rgba.Hex("FFF1D6");

        /// <summary>Uccellino in volo visto di lato, becco verso destra (centro 0,0). Frame: 0 ali su, 1 ali giù, 2 arrabbiato.</summary>
        public static DoodleArt Bird(int frame)
        {
            var p = new DoodlePainter(-0.6f, -0.5f, 0.6f, 0.55f, 150) { Ink = 0.035f };
            // ala dietro
            Sdf backWing = frame == 1 ? Sd.Rotate(Sd.Ellipse(-0.05f, -0.22f, 0.12f, 0.24f), -0.05f, 0f, -25f) : Sd.Rotate(Sd.Ellipse(-0.05f, 0.25f, 0.12f, 0.26f), -0.05f, 0f, 20f);
            Body(p, backWing, BirdShade, Rgba.Hex("235F8A"));
            // coda
            p.Shape(Sd.Polygon(-0.3f, 0.02f, -0.55f, 0.16f, -0.52f, -0.12f), BirdShade, 0.03f);
            Body(p, Sd.Ellipse(0f, 0f, 0.32f, 0.22f), BirdBlue, BirdShade);
            p.Shape(Sd.Intersect(Sd.Ellipse(0.05f, -0.08f, 0.24f, 0.13f), Sd.Box(0.05f, -0.15f, 0.3f, 0.1f)), BirdBelly, 0f);
            Body(p, Sd.Circle(0.3f, 0.1f, 0.15f), BirdBlue, BirdShade);
            p.Shape(Sd.Polygon(0.42f, 0.13f, 0.58f, 0.07f, 0.42f, 0.03f), Rgba.Hex("FFB02E"), 0.025f);   // becco
            if (frame == 2)
            {
                p.Shape(Sd.Circle(0.33f, 0.14f, 0.05f), Rgba.White, 0.018f);
                p.Fill(Sd.Circle(0.35f, 0.13f, 0.022f), Rgba.Hex("D11F2E"));
                p.InkLine(0.03f, 0.24f, 0.24f, 0.4f, 0.18f);                                              // sopracciglio furioso
                p.Shape(Sd.Polygon(0.42f, 0.06f, 0.56f, 0.0f, 0.42f, 0.0f), Rgba.Hex("FFB02E"), 0.02f);   // becco spalancato
            }
            else
            {
                p.Shape(Sd.Circle(0.33f, 0.14f, 0.045f), Rgba.White, 0.018f);
                p.Fill(Sd.Circle(0.345f, 0.14f, 0.022f), Ink);
            }
            // ala davanti
            Sdf wing = frame == 1 ? Sd.Rotate(Sd.Ellipse(0.02f, -0.25f, 0.13f, 0.26f), 0.02f, 0f, -15f) : Sd.Rotate(Sd.Ellipse(0.02f, 0.27f, 0.13f, 0.28f), 0.02f, 0f, 12f);
            Body(p, wing, Rgba.Hex("5DB4EE"), BirdShade);
            return new DoodleArt(frame == 0 ? "bird_up" : frame == 1 ? "bird_down" : "bird_angry", p);
        }

        /// <summary>Manica a vento a strisce (attacco al palo in 0,0, si allunga verso destra).</summary>
        public static DoodleArt Windsock()
        {
            var p = new DoodlePainter(-0.08f, -0.3f, 1.25f, 0.3f, 130) { Ink = 0.03f };
            for (int i = 0; i < 4; i++)
            {
                float x0 = i * 0.28f, x1 = x0 + 0.28f;
                float h0 = 0.22f - x0 * 0.08f, h1 = 0.22f - x1 * 0.08f;
                p.Shape(Sd.Polygon(x0, h0, x1, h1, x1, -h1, x0, -h0), i % 2 == 0 ? Rgba.Hex("FF5A4E") : Rgba.White, 0.025f);
            }
            p.Shape(Sd.Ellipse(0f, 0f, 0.05f, 0.23f), Rgba.Hex("C9CFD6"), 0.02f);
            return new DoodleArt("windsock", p);
        }

        /// <summary>Tasca di cuoio della fionda (centro 0,0).</summary>
        public static DoodleArt Pouch()
        {
            var p = new DoodlePainter(-0.22f, -0.16f, 0.22f, 0.16f, 160) { Ink = 0.03f };
            Body(p, Sd.Ellipse(0, 0, 0.18f, 0.11f), Rgba.Hex("8A5A3B"), Rgba.Hex("6B4129"));
            p.Line(Rgba.Hex("C9955F"), 0.012f, -0.1f, 0.03f, 0.1f, 0.03f);
            return new DoodleArt("pouch", p);
        }
        /// <summary>Ombrello aperto: bordo della cupola in y = 0 (larga 1.8), manico col gancio fino a y = -1.05.</summary>
        public static DoodleArt Umbrella(bool burnt)
        {
            var p = new DoodlePainter(-1.0f, -1.15f, 1.0f, 0.62f, 120) { Ink = 0.045f };
            p.InkLine(0.05f, 0f, 0.5f, 0f, -0.9f);
            p.Line(burnt ? Rgba.Hex("2B2133") : Rgba.Hex("8A5A3B"), 0.07f, 0f, -0.9f, 0f, -1.0f, -0.1f, -1.08f, -0.2f, -1.0f);
            Sdf dome = Sd.Intersect(Sd.Ellipse(0, 0, 0.9f, 0.5f), Sd.Box(0, 0.26f, 0.95f, 0.26f));
            // bordo smerlato: piccoli archi tra le stecche
            for (int i = 0; i < 4; i++) dome = Sd.Subtract(dome, Sd.Ellipse(-0.675f + i * 0.45f, -0.02f, 0.2f, 0.07f));
            if (!burnt)
            {
                p.FillFunc(dome, (x, y) =>
                {
                    int seg = (int)Math.Floor((Math.Atan2(y, x) / Math.PI) * 8.0);
                    Rgba c = seg % 2 == 0 ? Rgba.Hex("E8433F") : Rgba.Hex("FFF3E8");
                    return Rgba.Lerp(c, Rgba.Hex("B92E2A"), Clamp(0.25f - y * 0.4f) * 0.4f);
                });
            }
            else
            {
                // bruciacchiato: nero fumo con buchi e bordi strappati
                Sdf holes = Sd.Union(Sd.Circle(-0.35f, 0.22f, 0.13f), Sd.Circle(0.3f, 0.12f, 0.1f), Sd.Circle(0.05f, 0.36f, 0.07f), Sd.Circle(0.62f, 0.05f, 0.09f));
                dome = Sd.Subtract(dome, holes);
                p.FillFunc(dome, (x, y) => Rgba.Lerp(Rgba.Hex("3A3540"), Rgba.Hex("1E1A22"), Clamp(Noise.Value(x * 8f, y * 8f))));
            }
            p.Shape(dome, Rgba.Clear, 0.035f);
            for (int i = -2; i <= 2; i++) p.Line(burnt ? Rgba.Hex("111111", 0.6f) : Rgba.Hex("B92E2A", 0.6f), 0.015f, 0f, 0.48f, i * 0.42f, 0.02f);
            p.Shape(Sd.Circle(0, 0.52f, 0.045f), burnt ? Rgba.Hex("2B2133") : Gold, 0.02f);
            return new DoodleArt(burnt ? "umbrella_burnt" : "umbrella", p);
        }
        /// <summary>Scogliere del ponte: cielo, due rupi erbose (sinistra fino a x -1.25 con cima a y -0.6, destra da x 1.35
        /// con cima a y -0.2) e il fiume in fondo al burrone (sotto y -3.2). Coordinate del livello.</summary>
        public static DoodleArt CanyonBackground()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 4.6f, 7.1f), (x, y) => Rgba.Lerp(Rgba.Hex("FFE9C7"), Rgba.Hex("8ED4FF"), Clamp((y + 1f) / 5f)));
            Body(p, Sd.Circle(-2.4f, 3.3f, 0.5f), Rgba.Hex("FFE14A"), Rgba.Hex("FFC23A"));
            Body(p, Sd.Union(Sd.Ellipse(-1.5f, 0.8f, 2.6f, 1.2f), Sd.Ellipse(2.4f, 0.9f, 2.4f, 1.4f)), Rgba.Hex("B9DFA3"), Rgba.Hex("9CC98A"));   // colline lontane
            // fiume in fondo
            Sdf river = Sd.Box(0, -5.6f, 4.6f, 2.4f);
            p.FillFunc(river, (x, y) => Rgba.Lerp(Rgba.Hex("6FC3F2"), Rgba.Hex("2F7DB5"), Clamp((-3.2f - y) / 2f)));
            for (int i = 0; i < 6; i++) p.Line(Rgba.Hex("D6F1FF", 0.8f), 0.03f, -1.0f + i * 0.4f, -3.5f - (i % 2) * 0.3f, -0.8f + i * 0.4f, -3.45f - (i % 2) * 0.3f, -0.6f + i * 0.4f, -3.5f - (i % 2) * 0.3f);
            Rgba rock = Rgba.Hex("B98A5E"), rockShade = Rgba.Hex("8E6440");
            Sdf left = Sd.Polygon(-4.6f, -0.6f, -1.25f, -0.6f, -1.35f, -1.6f, -1.2f, -2.6f, -1.45f, -7.1f, -4.6f, -7.1f);
            Sdf right = Sd.Polygon(1.35f, -0.2f, 4.6f, -0.2f, 4.6f, -7.1f, 1.5f, -7.1f, 1.3f, -2.4f, 1.45f, -1.3f);
            foreach (Sdf cliff in new[] { left, right })
            {
                Body(p, cliff, rock, rockShade, rockShade.WithAlpha(0.35f), -0.1f, 0.05f);
                p.Hatch(cliff, Rgba.Hex("7A5236", 0.25f), 0.35f, 0.03f, -25f);
            }
            // erba sulle cime
            Body(p, Sd.Intersect(left, Sd.Box(-2.9f, -0.72f, 1.8f, 0.14f)), Rgba.Hex("7ED36B"), Rgba.Hex("5BB04B"));
            Body(p, Sd.Intersect(right, Sd.Box(2.9f, -0.32f, 1.8f, 0.14f)), Rgba.Hex("7ED36B"), Rgba.Hex("5BB04B"));
            return new DoodleArt("canyon_bg", p);
        }

        /// <summary>Spicchio di formaggio coi buchi (appoggiato in y = 0).</summary>
        public static DoodleArt CheeseWedge()
        {
            var p = new DoodlePainter(-0.45f, -0.05f, 0.45f, 0.45f, 150) { Ink = 0.035f };
            Sdf wedge = Sd.Polygon(-0.4f, 0f, 0.4f, 0f, 0.4f, 0.36f, -0.4f, 0.12f);
            Body(p, wedge, Rgba.Hex("FFD54A"), Rgba.Hex("E8B42A"));
            foreach (var h in new[] { new[] { -0.15f, 0.08f, 0.05f }, new[] { 0.12f, 0.2f, 0.06f }, new[] { 0.28f, 0.08f, 0.04f } })
                p.Fill(Sd.Circle(h[0], h[1], h[2]), Rgba.Hex("D9A21F"));
            return new DoodleArt("cheese_wedge", p);
        }
        /// <summary>Pergamena srotolata (centro 0,0, larga 2.6): la runa la disegna il livello sopra.</summary>
        public static DoodleArt Scroll()
        {
            var p = new DoodlePainter(-1.45f, -0.9f, 1.45f, 0.9f, 110) { Ink = 0.045f };
            Sdf sheet = Sd.Box(0, 0, 1.2f, 0.72f, 0.04f);
            p.FillFunc(sheet, (x, y) => Rgba.Lerp(Rgba.Hex("F6E7C1"), Rgba.Hex("E2C98F"), Clamp(Noise.Value(x * 3f, y * 3f) * 0.6f + Math.Abs(x) * 0.25f)));
            p.Shape(sheet, Rgba.Clear, 0.04f);
            foreach (float sx in new[] { -1f, 1f })
                Body(p, Sd.Box(sx * 1.25f, 0, 0.12f, 0.82f, 0.1f), Rgba.Hex("D9BC7E"), Rgba.Hex("B8955A"));
            return new DoodleArt("scroll", p);
        }

        /// <summary>Cerchio magico dove si disegna la runa (raggio 1.75).</summary>
        public static DoodleArt MagicCircle()
        {
            var p = new DoodlePainter(-1.9f, -1.9f, 1.9f, 1.9f, 90) { Wobble = 0f };
            p.FillFunc(Sd.Circle(0, 0, 1.9f), (x, y) =>
            {
                float r = (float)Math.Sqrt(x * x + y * y);
                float ring = Clamp(1f - Math.Abs(r - 1.75f) / 0.05f) + Clamp(1f - Math.Abs(r - 1.55f) / 0.03f) * 0.7f;
                float glow = Clamp(1f - Math.Abs(r - 1.75f) / 0.3f) * 0.25f;
                float inside = r < 1.55f ? 0.18f : 0f;
                Rgba c = Rgba.Hex("B98BFF");
                return c.WithAlpha(Math.Min(1f, ring + glow + inside));
            });
            for (int i = 0; i < 12; i++)
            {
                double a = i * Math.PI * 2 / 12;
                float cx = (float)Math.Cos(a) * 1.65f, cy = (float)Math.Sin(a) * 1.65f;
                p.Line(Rgba.Hex("E9DBFF"), 0.02f, cx - 0.04f, cy - 0.04f, cx + 0.04f, cy + 0.04f);
            }
            return new DoodleArt("magic_circle", p);
        }

        /// <summary>Forziere del tesoro (base in 0,0, largo 1.6): chiuso o aperto con le monete che brillano.</summary>
        public static DoodleArt Chest(bool open)
        {
            var p = new DoodlePainter(-0.95f, -0.1f, 0.95f, 1.75f, 120) { Ink = 0.05f };
            Rgba wood = Rgba.Hex("A0663A"), woodShade = Rgba.Hex("7A4A26");
            if (open)
            {
                Body(p, Sd.Polygon(-0.8f, 0.85f, 0.8f, 0.85f, 0.7f, 1.6f, -0.7f, 1.6f), woodShade, Rgba.Hex("5C3820"));   // coperchio alzato
                p.FillFunc(Sd.Ellipse(0, 0.86f, 0.75f, 0.16f), (x, y) => Rgba.Hex("FFE36B"));
                for (int i = 0; i < 7; i++) p.Shape(Sd.Circle(-0.5f + i * 0.17f, 0.9f + (i % 2) * 0.06f, 0.08f), Gold, 0.02f);
            }
            Body(p, Sd.Box(0, 0.42f, 0.8f, 0.42f, 0.06f), wood, woodShade);
            if (!open) Body(p, Sd.Intersect(Sd.Ellipse(0, 0.84f, 0.8f, 0.45f), Sd.Box(0, 1.06f, 0.82f, 0.23f)), wood, woodShade);
            foreach (float x in new[] { -0.55f, 0.55f }) p.Shape(Sd.Box(x, open ? 0.42f : 0.62f, 0.07f, open ? 0.42f : 0.62f), Gold, 0.025f);
            p.Shape(Sd.Box(0, 0.8f, 0.13f, 0.14f, 0.03f), Gold, 0.025f);
            p.Fill(Sd.Circle(0, 0.82f, 0.035f), Ink);
            return new DoodleArt(open ? "chest_open" : "chest", p);
        }
        // ---------- Livello 74: due porte, due chiavi ----------

        static readonly Rgba TileA = Rgba.Hex("E9D3B0"), TileB = Rgba.Hex("C9895F"), TileLine = Rgba.Hex("A86E48");

        /// <summary>Colore del pavimento a rombi in un punto (lo stesso schema per lo sfondo e per la chiave mimetizzata).</summary>
        static Rgba FloorTile(float x, float y)
        {
            float u = (x + y) / 0.62f, v = (x - y) / 0.62f;
            bool a = ((int)Math.Floor(u) + (int)Math.Floor(v)) % 2 == 0;
            float fu = u - (float)Math.Floor(u), fv = v - (float)Math.Floor(v);
            float edge = Math.Min(Math.Min(fu, 1f - fu), Math.Min(fv, 1f - fv));
            Rgba c = a ? TileA : TileB;
            // fiorellino al centro dei rombi chiari
            if (a && Math.Abs(fu - 0.5f) < 0.12f && Math.Abs(fv - 0.5f) < 0.12f) c = TileB;
            return edge < 0.035f ? TileLine : c;
        }

        /// <summary>Sala con carta da parati e pavimento a rombi (sotto y = -1.55). Coordinate del livello.</summary>
        public static DoodleArt HallBackground()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 2.7f, 4.6f, 4.3f), (x, y) =>
            {
                bool stripe = Math.Abs((x * 2.5f) - Math.Round(x * 2.5f)) < 0.18f;
                return stripe ? Rgba.Hex("E7C996") : Rgba.Hex("F3DBB0");
            });
            Body(p, Sd.Box(0, -1.3f, 4.6f, 0.26f), Rgba.Hex("8A5A3B"), Rgba.Hex("6B4129"));
            p.FillFunc(Sd.Box(0, -4.3f, 4.6f, 2.75f), (x, y) => FloorTile(x, y));
            return new DoodleArt("hall_bg", p);
        }

        /// <summary>Chiave rossa con l'anello tondo o blu con l'anello quadrato (punta in 0,0 rivolta a destra).</summary>
        public static DoodleArt ShapedKey(bool round)
        {
            var p = new DoodlePainter(-1.35f, -0.45f, 0.12f, 0.45f, 160) { Ink = 0.045f };
            Sdf bow = round ? Sd.Subtract(Sd.Circle(-0.95f, 0, 0.32f), Sd.Circle(-0.95f, 0, 0.12f))
                            : Sd.Subtract(Sd.Box(-0.95f, 0, 0.3f, 0.3f, 0.04f), Sd.Box(-0.95f, 0, 0.11f, 0.11f));
            Sdf key = Sd.Union(bow, Sd.Box(-0.33f, 0, 0.33f, 0.075f, 0.07f),
                Sd.Box(-0.12f, -0.13f, 0.05f, 0.08f, 0.02f), Sd.Box(-0.3f, -0.12f, 0.05f, 0.065f, 0.02f));
            Rgba c = round ? Rgba.Hex("E8433F") : Rgba.Hex("3E6FE0"), shade = round ? Rgba.Hex("B92E2A") : Rgba.Hex("2A4FAE");
            p.Fill(key, c);
            p.Fill(Sd.Crescent(key, -0.03f, 0.04f), shade);
            p.Shape(key, Rgba.Clear);
            return new DoodleArt(round ? "key_round" : "key_square", p);
        }

        /// <summary>La chiave rossa "dipinta" nel pavimento: stessi rombi, appena più scuri, nessun bordo.</summary>
        public static DoodleArt FloorKeyCamo()
        {
            var p = new DoodlePainter(-1.35f, -0.45f, 0.12f, 0.45f, 160) { Wobble = 0f };
            Sdf bow = Sd.Subtract(Sd.Circle(-0.95f, 0, 0.32f), Sd.Circle(-0.95f, 0, 0.12f));
            Sdf key = Sd.Union(bow, Sd.Box(-0.33f, 0, 0.33f, 0.075f, 0.07f), Sd.Box(-0.12f, -0.13f, 0.05f, 0.08f, 0.02f), Sd.Box(-0.3f, -0.12f, 0.05f, 0.065f, 0.02f));
            // coordinate del pavimento: la chiave sta (con scala 0.5) a x 1.55, y -3.05
            p.FillFunc(key, (x, y) =>
            {
                Rgba c = FloorTile(1.55f + x * 0.5f, -3.05f + y * 0.5f);
                return Rgba.Lerp(c, Rgba.Hex("9C5A3A"), 0.22f);
            });
            return new DoodleArt("floor_key_camo", p);
        }

        /// <summary>Porta a riquadri: vano scuro con cornice (base in 0,0, largo 1.5, alto 2.6).</summary>
        public static DoodleArt Doorway()
        {
            var p = new DoodlePainter(-0.9f, -0.08f, 0.9f, 2.82f, 100) { Ink = 0.05f };
            Sdf opening = Sd.Box(0, 1.3f, 0.66f, 1.3f, 0.03f);
            p.FillFunc(opening, (x, y) => Rgba.Lerp(Rgba.Hex("2A1E2E"), Rgba.Hex("120C16"), Clamp(1f - y / 2.6f)));
            Body(p, Sd.Subtract(Sd.Box(0, 1.36f, 0.8f, 1.4f, 0.06f), opening), Rgba.Hex("6B4129"), Rgba.Hex("4A2A18"));
            return new DoodleArt("doorway", p);
        }

        /// <summary>Anta colorata con la serratura tonda (rossa) o quadrata (blu): cardini in x = 0, larga 1.32.</summary>
        public static DoodleArt ColorDoor(bool red)
        {
            var p = new DoodlePainter(-0.08f, -0.05f, 1.42f, 2.65f, 100) { Ink = 0.05f };
            Rgba c = red ? Rgba.Hex("C9483C") : Rgba.Hex("3E6FC9"), shade = red ? Rgba.Hex("9C2F28") : Rgba.Hex("2A4F9C");
            Body(p, Sd.Box(0.66f, 1.3f, 0.66f, 1.3f, 0.05f), c, shade, null, -0.04f, 0.04f);
            foreach (float y in new[] { 1.85f, 0.65f })
                p.Shape(Sd.Box(0.66f, y, 0.45f, 0.42f, 0.05f), Rgba.Lerp(c, Rgba.White, 0.12f), 0.03f);
            Body(p, Sd.Circle(1.12f, 1.25f, 0.08f), Gold, GoldShade);
            // placca e serratura (centro in 1.12, 1.0)
            p.Shape(Sd.Box(1.12f, 1.0f, 0.1f, 0.15f, 0.04f), Gold, 0.02f);
            if (red) p.Fill(Sd.Circle(1.12f, 1.0f, 0.05f), Ink);
            else p.Fill(Sd.Box(1.12f, 1.0f, 0.045f, 0.045f), Ink);
            return new DoodleArt(red ? "door_red" : "door_blue", p);
        }

        /// <summary>Ragazza bellissima che saluta (base in 0,0, alta circa 2.1).</summary>
        public static DoodleArt Lady()
        {
            var p = new DoodlePainter(-0.8f, -0.05f, 0.8f, 2.25f, 120) { Ink = 0.04f };
            Rgba skin = Rgba.Hex("FFD9C2"), skinShade = Rgba.Hex("F2B79C"), hair = Rgba.Hex("6B3A1E"), hairShade = Rgba.Hex("4A2512");
            Body(p, Sd.Union(Sd.Ellipse(0, 1.6f, 0.42f, 0.52f), Sd.Box(0, 1.05f, 0.45f, 0.45f, 0.2f)), hair, hairShade);   // capelli lunghi
            Body(p, Sd.Polygon(-0.55f, 0f, 0.55f, 0f, 0.3f, 1.0f, -0.3f, 1.0f), Rgba.Hex("E8433F"), Rgba.Hex("B92E2A"));   // vestito
            Body(p, Sd.Box(0, 1.05f, 0.08f, 0.1f), skin, skinShade);
            Body(p, Sd.Ellipse(0, 1.48f, 0.3f, 0.36f), skin, skinShade);
            Body(p, Sd.Intersect(Sd.Ellipse(0.02f, 1.78f, 0.34f, 0.2f), Sd.Box(0, 1.85f, 0.4f, 0.16f)), hair, hairShade);  // frangia
            foreach (float sx in new[] { -1f, 1f })
            {
                p.Shape(Sd.Ellipse(sx * 0.11f, 1.5f, 0.06f, 0.08f), Rgba.White, 0.018f);
                p.Fill(Sd.Circle(sx * 0.11f, 1.49f, 0.04f), Rgba.Hex("3B7A57"));
                p.InkLine(0.018f, sx * 0.05f, 1.58f, sx * 0.11f, 1.6f, sx * 0.18f, 1.58f, sx * 0.21f, 1.61f);   // ciglia
                p.Fill(Sd.Ellipse(sx * 0.17f, 1.38f, 0.06f, 0.035f), Rgba.Hex("FF9BB0", 0.7f));                  // guance
            }
            p.Shape(Sd.Intersect(Sd.Ellipse(0, 1.3f, 0.09f, 0.06f), Sd.Box(0, 1.27f, 0.1f, 0.04f)), Rgba.Hex("E23B5A"), 0.015f);   // sorriso
            // braccio che saluta
            p.Shape(Sd.Capsule(0.32f, 0.85f, 0.62f, 1.45f, 0.07f), skin, 0.03f);
            p.Shape(Sd.Circle(0.64f, 1.5f, 0.09f), skin, 0.025f);
            return new DoodleArt("lady", p);
        }

        /// <summary>Mostro peloso viola con corna e zanne (base in 0,0, alto circa 2.3).</summary>
        public static DoodleArt Monster()
        {
            var p = new DoodlePainter(-0.85f, -0.05f, 0.85f, 2.4f, 120) { Ink = 0.05f };
            Rgba fur = Rgba.Hex("7B4FC9"), furShade = Rgba.Hex("5A3399");
            foreach (float sx in new[] { -1f, 1f })
                p.Shape(Sd.Polygon(sx * 0.3f, 1.95f, sx * 0.55f, 2.35f, sx * 0.45f, 1.9f), Rgba.Hex("F2E6C9"), 0.03f);   // corna
            Body(p, Sd.Union(Sd.Ellipse(0, 1.2f, 0.7f, 0.95f), Sd.Box(0, 0.3f, 0.65f, 0.3f, 0.15f)), fur, furShade, furShade.WithAlpha(0.4f));
            for (int i = 0; i < 14; i++)
            {
                double a = i * Math.PI * 2 / 14;
                float x = (float)Math.Cos(a) * 0.7f, y = 1.2f + (float)Math.Sin(a) * 0.95f;
                p.Shape(Sd.Polygon(x, y, x * 1.12f + 0.04f, y + 0.08f, x * 1.05f - 0.04f, y + 0.02f), fur, 0.02f);   // ciuffi di pelo
            }
            p.Shape(Sd.Circle(0, 1.55f, 0.24f), Rgba.Hex("FFF36B"), 0.03f);   // occhio unico giallo
            p.Fill(Sd.Ellipse(0, 1.55f, 0.06f, 0.16f), Ink);
            p.InkLine(0.05f, -0.3f, 1.86f, 0f, 1.76f, 0.3f, 1.86f);
            Sdf mouth = Sd.Ellipse(0, 0.95f, 0.42f, 0.24f);
            p.Shape(mouth, Rgba.Hex("3A0F1E"), 0.04f);
            for (int i = 0; i < 5; i++)
            {
                float x = -0.3f + i * 0.15f;
                p.Shape(Sd.Polygon(x - 0.06f, 1.15f, x + 0.06f, 1.15f, x, 0.98f), Rgba.White, 0.015f);
                p.Shape(Sd.Polygon(x - 0.05f, 0.73f, x + 0.05f, 0.73f, x, 0.88f), Rgba.White, 0.015f);
            }
            foreach (float sx in new[] { -1f, 1f }) Body(p, Sd.Capsule(sx * 0.62f, 1.05f, sx * 0.8f, 0.45f, 0.13f), fur, furShade);
            return new DoodleArt("monster", p);
        }

        /// <summary>Quadro con paesaggio (centro 0,0): la chiave blu è nascosta nel recinto, dipinta con gli stessi toni.</summary>
        public static DoodleArt HallPainting(bool withKey)
        {
            var p = new DoodlePainter(-1.0f, -0.75f, 1.0f, 0.75f, 120) { Ink = 0.045f };
            Body(p, Sd.Box(0, 0, 0.95f, 0.7f, 0.05f), Gold, GoldShade);
            Sdf canvas = Sd.Box(0, 0, 0.8f, 0.55f);
            p.FillFunc(canvas, (x, y) => Rgba.Lerp(Rgba.Hex("BFE6FF"), Rgba.Hex("7EC3EE"), Clamp(y / 0.5f)));
            Body(p, Sd.Intersect(canvas, Sd.Ellipse(-0.3f, -0.55f, 0.9f, 0.45f)), Rgba.Hex("7ED36B"), Rgba.Hex("5BB04B"));
            Body(p, Sd.Intersect(canvas, Sd.Ellipse(0.5f, -0.6f, 0.7f, 0.4f)), Rgba.Hex("8FDA7B"), Rgba.Hex("5BB04B"));
            Body(p, Sd.Circle(0.55f, 0.32f, 0.12f), Rgba.Hex("FFE14A"), Rgba.Hex("FFC23A"));
            Body(p, Sd.Box(-0.45f, -0.05f, 0.04f, 0.2f), Rgba.Hex("8A5A3B"), Rgba.Hex("6B4129"));
            Body(p, Sd.Circle(-0.45f, 0.18f, 0.2f), Rgba.Hex("5BB04B"), Rgba.Hex("3E9A35"));
            // recinto di legno blu-grigio: la chiave (anello quadrato) è uno dei suoi pezzi
            Rgba fence = Rgba.Hex("5E78A8");
            for (int i = 0; i < 5; i++) p.Shape(Sd.Box(0.02f + i * 0.13f, -0.3f, 0.025f, 0.09f), fence, 0.012f);
            p.Shape(Sd.Box(0.28f, -0.3f, 0.3f, 0.018f), fence, 0.01f);
            if (withKey)
            {
                Sdf key = Sd.Union(Sd.Subtract(Sd.Box(-0.05f, -0.18f, 0.06f, 0.06f, 0.01f), Sd.Box(-0.05f, -0.18f, 0.022f, 0.022f)),
                                   Sd.Box(0.07f, -0.18f, 0.07f, 0.016f, 0.01f), Sd.Box(0.12f, -0.205f, 0.012f, 0.016f));
                p.Fill(key, Rgba.Hex("5470A3"));
            }
            p.Shape(canvas, Rgba.Clear, 0.03f);
            return new DoodleArt(withKey ? "hall_painting" : "hall_painting_empty", p);
        }
    }
}
