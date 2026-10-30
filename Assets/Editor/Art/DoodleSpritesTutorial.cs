using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I pezzi delle icone animate del mini tutorial dei primi 8 livelli (scheda dei consigli di GameUI). Ogni icona è
    /// fatta di pezzi separati che GameUI muove, ruota, scala e sfuma da codice (niente Animator): la mano col dito,
    /// l'anello del tocco, la chiave, il telefono coi tasti laterali, il sole e la freccia, la lente, le scintille,
    /// gli oggetti-indizio, la scia dello swipe, l'orecchio e le onde, la sveglia con le lancette, l'altoparlante e i
    /// pulsanti tondi. Regola: ogni disegno è centrato sul suo punto di aggancio (l'origine, attorno a cui ruota e
    /// scala), quindi i bordi del pittore sono simmetrici rispetto a 0,0 e il pivot centrale dello sprite coincide.
    /// Nell'icona 1 unità mondo = circa 85 px dell'interfaccia (l'icona occupa circa -1..1).
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtTutorial() => new DoodleArt[]
        {
            Tut_Finger(), Tut_Ring(), Tut_Key(), Tut_Phone(), Tut_SideKey(), Tut_Sun(), Tut_Arrow(), Tut_Magnifier(),
            Tut_Sparkle(), Tut_Clues(), Tut_Trail(), Tut_Ear(), Tut_Wave("tut_wave_s", 0.2f), Tut_Wave("tut_wave_m", 0.32f),
            Tut_Wave("tut_wave_l", 0.44f), Tut_Clock(), Tut_Hand("tut_hand_long", 0.32f, 0.045f), Tut_Hand("tut_hand_short", 0.2f, 0.06f),
            Tut_Speaker(), Tut_Button("tut_button_red", Rgba.Hex("FF5C6B"), Rgba.Hex("D9404F")),
            Tut_Button("tut_button_blue", Rgba.Hex("3E8EF7"), Rgba.Hex("2563C9")),
        };

        const float TutPpu = 128f;
        static readonly Rgba TutSkin = Rgba.Hex("FFD6B3"), TutSkinShade = Rgba.Hex("EDB28C");
        static readonly Rgba TutGold = Rgba.Hex("F2C230"), TutGoldShade = Rgba.Hex("D19D1A");
        static readonly Rgba TutTeal = Rgba.Hex("2BBFB5"), TutCoral = Rgba.Hex("FF5C6B");

        /// <summary>Pittore simmetrico attorno all'origine (il punto di aggancio).</summary>
        static DoodlePainter TutPainter(float hx, float hy, float ink = 0.035f)
        {
            return new DoodlePainter(-hx, -hy, hx, hy, TutPpu) { Ink = ink, Wobble = 0.006f };
        }

        /// <summary>Mano che indica verso l'alto: la punta del dito è nell'origine (il punto che tocca lo schermo).</summary>
        public static DoodleArt Tut_Finger()
        {
            var p = TutPainter(0.4f, 1.0f);
            Sdf finger = Sd.Capsule(0f, -0.1f, 0f, -0.55f, 0.1f);
            Sdf palm = Sd.Box(0.07f, -0.7f, 0.2f, 0.19f, 0.1f);
            Sdf thumb = Sd.Capsule(-0.12f, -0.66f, -0.2f, -0.5f, 0.065f);
            Body(p, Sd.Union(finger, palm, thumb), TutSkin, TutSkinShade, null, -0.04f, 0.05f);
            // nocche delle dita piegate e unghia
            foreach (float y in new[] { -0.6f, -0.71f }) p.InkLine(0.022f, 0.13f, y, 0.26f, y);
            p.Shape(Sd.Box(0f, -0.1f, 0.05f, 0.05f, 0.04f), Rgba.Hex("FFEBDD"), 0.02f);
            // polsino corallo
            p.Shape(Sd.Box(0.07f, -0.93f, 0.23f, 0.06f, 0.03f), TutCoral, 0.03f);
            return new DoodleArt("tut_finger", p);
        }

        /// <summary>Anello del tocco (si allarga e svanisce): raggio 0.3.</summary>
        public static DoodleArt Tut_Ring()
        {
            var p = TutPainter(0.37f, 0.37f, 0.025f);
            p.Shape(Sd.Subtract(Sd.Circle(0f, 0f, 0.31f), Sd.Circle(0f, 0f, 0.24f)), Rgba.White);
            return new DoodleArt("tut_ring", p);
        }

        /// <summary>Chiave d'oro orizzontale, centrata.</summary>
        public static DoodleArt Tut_Key()
        {
            var p = TutPainter(0.64f, 0.3f);
            Sdf bow = Sd.Subtract(Sd.Circle(-0.38f, 0f, 0.21f), Sd.Circle(-0.38f, 0f, 0.085f));
            Sdf shaft = Sd.Box(0.12f, 0f, 0.36f, 0.06f, 0.03f);
            Sdf teeth = Sd.Union(Sd.Box(0.33f, -0.12f, 0.05f, 0.08f, 0.02f), Sd.Box(0.46f, -0.1f, 0.04f, 0.06f, 0.02f));
            Body(p, Sd.Union(bow, shaft, teeth), TutGold, TutGoldShade);
            p.Line(Rgba.White.WithAlpha(0.7f), 0.03f, -0.48f, 0.1f, -0.42f, 0.15f);
            return new DoodleArt("tut_key", p);
        }

        /// <summary>Telefono visto di fronte (in verticale): schermo azzurro con la biglia e la stella del labirinto.</summary>
        public static DoodleArt Tut_Phone()
        {
            var p = TutPainter(0.42f, 0.72f, 0.04f);
            Body(p, Sd.Box(0f, 0f, 0.36f, 0.66f, 0.1f), Rgba.Hex("4A4363"), Rgba.Hex("36304A"));
            p.Shape(Sd.Box(0f, 0.02f, 0.29f, 0.53f, 0.04f), Rgba.Hex("9EDCF5"), 0.025f);
            p.Fill(Sd.Polygon(-0.29f, 0.3f, -0.29f, 0.55f, -0.05f, 0.55f), Rgba.White.WithAlpha(0.35f));
            p.Fill(Sd.Box(0f, 0.6f, 0.07f, 0.014f, 0.014f), Rgba.Hex("2B2233"));
            p.Shape(Sd.Circle(-0.12f, -0.3f, 0.07f), Rgba.Hex("D8DCE6"), 0.02f);
            p.Shape(Star(0.12f, 0.25f, 0.1f, 0.045f), TutGold, 0.02f);
            return new DoodleArt("tut_phone", p);
        }

        /// <summary>Tasto laterale del telefono (volume), verticale.</summary>
        public static DoodleArt Tut_SideKey()
        {
            var p = TutPainter(0.07f, 0.14f, 0.025f);
            p.Shape(Sd.Box(0f, 0f, 0.04f, 0.11f, 0.03f), Rgba.Hex("6E6788"));
            return new DoodleArt("tut_side_key", p);
        }

        /// <summary>Sole (il cielo verso cui guarda lo schermo).</summary>
        public static DoodleArt Tut_Sun()
        {
            var p = TutPainter(0.38f, 0.38f);
            for (int i = 0; i < 8; i++)
            {
                double a = i * Math.PI / 4;
                float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
                p.Line(Rgba.Hex("FFB627"), 0.05f, c * 0.24f, s * 0.24f, c * 0.33f, s * 0.33f);
            }
            Body(p, Sd.Circle(0f, 0f, 0.18f), Rgba.Hex("FFD84D"), Rgba.Hex("F5B731"));
            return new DoodleArt("tut_sun", p);
        }

        /// <summary>Freccia verso l'alto (dallo schermo al cielo), centrata.</summary>
        public static DoodleArt Tut_Arrow()
        {
            var p = TutPainter(0.2f, 0.38f);
            Sdf arrow = Sd.Union(Sd.Box(0f, -0.1f, 0.05f, 0.22f, 0.03f), Sd.Polygon(-0.16f, 0.1f, 0.16f, 0.1f, 0f, 0.33f));
            Body(p, arrow, TutCoral, Rgba.Hex("D9404F"), null, -0.03f, 0.03f);
            return new DoodleArt("tut_arrow", p);
        }

        /// <summary>Lente d'ingrandimento: il centro del vetro è nell'origine, il manico scende a destra.</summary>
        public static DoodleArt Tut_Magnifier()
        {
            var p = TutPainter(0.62f, 0.62f, 0.035f);
            Body(p, Sd.Capsule(0.26f, -0.26f, 0.5f, -0.5f, 0.07f), Rgba.Hex("C98B5A"), Rgba.Hex("A86E48"));
            p.Fill(Sd.Circle(0f, 0f, 0.24f), Rgba.Hex("D5ECF7", 0.45f));
            p.Shape(Sd.Subtract(Sd.Circle(0f, 0f, 0.29f), Sd.Circle(0f, 0f, 0.23f)), Rgba.Hex("A9B0BF"));
            p.Line(Rgba.White.WithAlpha(0.85f), 0.035f, -0.14f, 0.06f, -0.08f, 0.14f);
            return new DoodleArt("tut_magnifier", p);
        }

        /// <summary>Scintilla a quattro punte.</summary>
        public static DoodleArt Tut_Sparkle()
        {
            var p = TutPainter(0.18f, 0.18f, 0.022f);
            p.Shape(Star4(0f, 0f, 0.16f, 0.045f), Rgba.Hex("FFE066"));
            return new DoodleArt("tut_sparkle", p);
        }

        /// <summary>Tre piccoli "indizi" in fila (biscotto in x -0.6, pulsante rosso in 0, foglietto giallo in 0.6).</summary>
        public static DoodleArt Tut_Clues()
        {
            var p = TutPainter(0.86f, 0.26f, 0.03f);
            // biscotto
            Body(p, Sd.Circle(-0.6f, 0f, 0.18f), Rgba.Hex("E0A458"), Rgba.Hex("C4863E"));
            foreach (float[] c in new[] { new[] { -0.66f, 0.06f }, new[] { -0.53f, 0.04f }, new[] { -0.6f, -0.08f } })
                p.Fill(Sd.Circle(c[0], c[1], 0.03f), Rgba.Hex("5B3A29"));
            // pulsante rosso
            p.Shape(Sd.Ellipse(0f, -0.08f, 0.21f, 0.09f), Rgba.Hex("8E95A6"));
            Body(p, Sd.Intersect(Sd.Ellipse(0f, -0.04f, 0.16f, 0.17f), Sd.Box(0f, 0.06f, 0.2f, 0.12f)), TutCoral, Rgba.Hex("D9404F"));
            // foglietto
            p.Shape(Sd.Rotate(Sd.Box(0.6f, 0f, 0.17f, 0.17f, 0.02f), 0.6f, 0f, 8f), Rgba.Hex("FFE680"));
            p.InkLine(0.02f, 0.5f, 0.05f, 0.56f, 0.08f, 0.62f, 0.04f, 0.7f, 0.08f);
            p.InkLine(0.02f, 0.5f, -0.05f, 0.58f, -0.03f, 0.66f, -0.06f);
            return new DoodleArt("tut_clues", p);
        }

        /// <summary>Scia dello swipe: sottile a sinistra, piena a destra (dove c'è il dito), centrata.</summary>
        public static DoodleArt Tut_Trail()
        {
            var p = TutPainter(0.56f, 0.14f, 0.0f);
            p.Fill(Sd.Polygon(-0.52f, 0.0f, 0.4f, 0.075f, 0.5f, 0.0f, 0.4f, -0.075f), Rgba.White.WithAlpha(0.9f));
            p.Fill(Sd.Polygon(-0.3f, 0.0f, 0.4f, 0.035f, 0.45f, 0.0f, 0.4f, -0.035f), TutTeal.WithAlpha(0.7f));
            p.Line(Rgba.White.WithAlpha(0.7f), 0.02f, -0.2f, 0.11f, 0.25f, 0.11f);
            p.Line(Rgba.White.WithAlpha(0.7f), 0.02f, -0.1f, -0.11f, 0.3f, -0.11f);
            return new DoodleArt("tut_trail", p);
        }

        /// <summary>Orecchio (visto di lato, apertura a destra), centrato.</summary>
        public static DoodleArt Tut_Ear()
        {
            var p = TutPainter(0.34f, 0.44f);
            Sdf ear = Sd.Union(Sd.Ellipse(0f, 0.07f, 0.25f, 0.33f), Sd.Ellipse(0.02f, -0.24f, 0.13f, 0.13f));
            Body(p, ear, TutSkin, TutSkinShade, null, 0.05f, 0.05f);
            // conca e piega dell'elice
            p.Fill(Sd.Ellipse(0.05f, -0.02f, 0.1f, 0.14f), TutSkinShade);
            p.InkLine(0.03f, 0.13f, 0.25f, 0.03f, 0.3f, -0.1f, 0.24f, -0.15f, 0.08f, -0.1f, -0.06f, -0.03f, -0.13f);
            p.InkLine(0.026f, 0.1f, 0.06f, 0.03f, 0.1f, -0.03f, 0.03f);
            return new DoodleArt("tut_ear", p);
        }

        /// <summary>Onda sonora: arco ")" di raggio r attorno all'origine (lato destro, ±50°).</summary>
        public static DoodleArt Tut_Wave(string name, float r)
        {
            float h = r + 0.06f;
            var p = TutPainter(h, h, 0.0f);
            var pts = new List<float>();
            for (int i = 0; i <= 12; i++)
            {
                double a = (-50 + i * 100 / 12.0) * Math.PI / 180;
                pts.Add((float)Math.Cos(a) * r);
                pts.Add((float)Math.Sin(a) * r);
            }
            p.Line(DoodlePainter.DefaultInk, 0.075f, pts.ToArray());
            p.Line(TutTeal, 0.04f, pts.ToArray());
            return new DoodleArt(name, p);
        }

        /// <summary>Sveglia senza lancette (le lancette sono pezzi a parte): il centro del quadrante è nell'origine.</summary>
        public static DoodleArt Tut_Clock()
        {
            var p = TutPainter(0.6f, 0.6f, 0.035f);
            // campanelle e martelletto, piedini
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Intersect(Sd.Circle(s * 0.28f, 0.38f, 0.14f), Sd.Rotate(Sd.Box(s * 0.28f, 0.45f, 0.2f, 0.12f), s * 0.28f, 0.38f, -s * 35f)), TutGold, TutGoldShade);
                p.InkLine(0.05f, s * 0.24f, -0.36f, s * 0.33f, -0.5f);
            }
            p.InkLine(0.04f, 0f, 0.42f, 0f, 0.52f);
            Body(p, Sd.Circle(0f, 0f, 0.42f), TutCoral, Rgba.Hex("D9404F"));
            p.Shape(Sd.Circle(0f, 0f, 0.33f), Rgba.Hex("FFF8E7"), 0.03f);
            for (int i = 0; i < 12; i++)
            {
                double a = i * Math.PI / 6;
                float c = (float)Math.Cos(a), s = (float)Math.Sin(a), r0 = i % 3 == 0 ? 0.23f : 0.27f;
                p.InkLine(i % 3 == 0 ? 0.035f : 0.022f, c * r0, s * r0, c * 0.3f, s * 0.3f);
            }
            return new DoodleArt("tut_clock", p);
        }

        /// <summary>Lancetta: dall'origine (il perno) verso l'alto, lunga len.</summary>
        public static DoodleArt Tut_Hand(string name, float len, float width)
        {
            float h = len + 0.05f;
            var p = TutPainter(h, h, 0.0f);
            p.InkLine(width, 0f, -0.04f, 0f, len);
            p.Fill(Sd.Circle(0f, 0f, width * 0.9f), DoodlePainter.DefaultInk);
            return new DoodleArt(name, p);
        }

        /// <summary>Altoparlante (il volume), centrato; le onde sono pezzi a parte.</summary>
        public static DoodleArt Tut_Speaker()
        {
            var p = TutPainter(0.24f, 0.24f, 0.03f);
            Sdf spk = Sd.Union(Sd.Box(-0.12f, 0f, 0.07f, 0.08f, 0.02f), Sd.Polygon(-0.08f, 0.08f, 0.1f, 0.2f, 0.1f, -0.2f, -0.08f, -0.08f));
            p.Shape(spk, Rgba.White);
            return new DoodleArt("tut_speaker", p);
        }

        /// <summary>Pulsante tondo visto dall'alto di tre quarti (base grigia e cupola colorata), centrato.</summary>
        public static DoodleArt Tut_Button(string name, Rgba dome, Rgba shade)
        {
            var p = TutPainter(0.32f, 0.32f, 0.032f);
            Body(p, Sd.Ellipse(0f, -0.08f, 0.29f, 0.17f), Rgba.Hex("A9B0BF"), Rgba.Hex("7C8496"));
            Body(p, Sd.Union(Sd.Ellipse(0f, 0.06f, 0.2f, 0.12f), Sd.Box(0f, -0.02f, 0.2f, 0.08f)), dome, shade);
            p.Fill(Sd.Ellipse(-0.07f, 0.1f, 0.07f, 0.03f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt(name, p);
        }

        /// <summary>Stella a cinque punte.</summary>
        static Sdf Star(float cx, float cy, float rOut, float rIn)
        {
            var pts = new float[20];
            for (int i = 0; i < 10; i++)
            {
                double a = Math.PI / 2 + i * Math.PI / 5;
                float r = i % 2 == 0 ? rOut : rIn;
                pts[i * 2] = cx + (float)Math.Cos(a) * r;
                pts[i * 2 + 1] = cy + (float)Math.Sin(a) * r;
            }
            return Sd.Polygon(pts);
        }

        /// <summary>Stella a quattro punte (scintilla).</summary>
        static Sdf Star4(float cx, float cy, float rOut, float rIn)
        {
            var pts = new float[16];
            for (int i = 0; i < 8; i++)
            {
                double a = Math.PI / 2 + i * Math.PI / 4;
                float r = i % 2 == 0 ? rOut : rIn;
                pts[i * 2] = cx + (float)Math.Cos(a) * r;
                pts[i * 2 + 1] = cy + (float)Math.Sin(a) * r;
            }
            return Sd.Polygon(pts);
        }
    }
}
