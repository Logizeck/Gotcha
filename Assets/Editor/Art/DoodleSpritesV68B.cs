using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.68 per il livello 264 "The Camera-Shy Bird" (il parco, l'albero, il cespuglio, le nuvole, il
    /// sole, il ramo, l'uccellino che canta e l'omino sono quelli esistenti): le pose dell'uccellino timido (sbircia, fissa
    /// l'obiettivo, si copre con l'ala, vola, fa la V col fotobomba), gli occhialini da sole, gli occhi chiusi dell'omino
    /// (s41b_gr_man) per la foto finale, la panchina, il lampione e le icone dell'app della fotocamera (scatto, giro della
    /// fotocamera, flash, riquadro della messa a fuoco).
    /// Le pose dell'uccellino sono nelle coordinate di "s41h_bird" (rivolto a destra, centro 0,0, zampe in y -0.42, occhio
    /// in 0.26,0.25): si scambiano sullo stesso oggetto e gli occhialini si appoggiano sopra con la stessa scala.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV68B() => new DoodleArt[]
        {
            V68_BirdPeek(), V68_BirdStare(), V68_BirdHide(), V68_BirdFly(), V68_BirdPeace(), V68_BirdShades(),
            V68_Blink(), V68_Bench(), V68_LampPost(), V68_ShutterButton(), V68_FlipIcon(), V68_FlashIcon(), V68_Focus(),
        };

        static readonly Rgba V68Beak = Rgba.Hex("FFB43C");

        /// <summary>Coda, zampe, corpo e pancia dell'uccellino (come s41h_bird, senza becco né occhio).</summary>
        static void V68_BirdBase(DoodlePainter p, bool legs = true)
        {
            p.Shape(Sd.Polygon(-0.35f, -0.05f, -0.62f, 0.12f, -0.6f, -0.12f), BirdShade);
            if (legs)
            {
                p.InkLine(0.03f, -0.05f, -0.3f, -0.07f, -0.44f);
                p.InkLine(0.03f, 0.1f, -0.3f, 0.12f, -0.44f);
            }
            Sdf body = Sd.Union(Sd.Ellipse(-0.02f, -0.05f, 0.38f, 0.3f), Sd.Circle(0.2f, 0.17f, 0.22f));
            Body(p, body, BirdBlue, BirdShade);
            p.Fill(Sd.Ellipse(0.05f, -0.15f, 0.22f, 0.15f), BirdBelly);
        }

        static void V68_ClosedBeak(DoodlePainter p)
        {
            p.Shape(Sd.Polygon(0.38f, 0.23f, 0.6f, 0.17f, 0.38f, 0.11f), V68Beak, 0.025f);
            p.InkLine(0.015f, 0.4f, 0.17f, 0.56f, 0.17f);
        }

        static void V68_Crest(DoodlePainter p, bool puffed)
        {
            if (!puffed) { p.Shape(Sd.Polygon(0.12f, 0.36f, 0.06f, 0.5f, 0.18f, 0.4f), BirdBlue, 0.02f); return; }
            p.Shape(Sd.Polygon(0.08f, 0.35f, -0.02f, 0.55f, 0.14f, 0.39f), BirdBlue, 0.02f);
            p.Shape(Sd.Polygon(0.15f, 0.38f, 0.16f, 0.6f, 0.22f, 0.39f), BirdBlue, 0.02f);
            p.Shape(Sd.Polygon(0.24f, 0.38f, 0.34f, 0.56f, 0.3f, 0.35f), BirdBlue, 0.02f);
        }

        /// <summary>Sbircia curioso: becco chiuso, occhio che guarda avanti (verso l'obiettivo), guancia rosa.</summary>
        public static DoodleArt V68_BirdPeek()
        {
            var p = new DoodlePainter(-0.62f, -0.5f, 0.66f, 0.56f, 150) { Ink = 0.045f };
            V68_BirdBase(p);
            p.Shape(Sd.Ellipse(-0.12f, -0.02f, 0.2f, 0.12f), BirdShade, 0.03f);
            V68_ClosedBeak(p);
            p.Shape(Sd.Circle(0.26f, 0.25f, 0.065f), Rgba.White, 0.02f);
            p.Fill(Sd.Circle(0.29f, 0.25f, 0.04f), Ink);
            p.Fill(Sd.Circle(0.28f, 0.27f, 0.014f), Rgba.White);
            p.Fill(Sd.Circle(0.3f, 0.1f, 0.04f), BlushColor.WithAlpha(0.6f));
            V68_Crest(p, false);
            return new DoodleArt("v68_bird_peek", p);
        }

        /// <summary>Fissa l'obiettivo: occhio enorme con la pupilla piccola, piume ritte, becco serrato.</summary>
        public static DoodleArt V68_BirdStare()
        {
            var p = new DoodlePainter(-0.62f, -0.5f, 0.66f, 0.66f, 150) { Ink = 0.045f };
            V68_BirdBase(p);
            p.Shape(Sd.Ellipse(-0.12f, -0.02f, 0.2f, 0.12f), BirdShade, 0.03f);
            V68_ClosedBeak(p);
            p.Shape(Sd.Circle(0.26f, 0.25f, 0.1f), Rgba.White, 0.025f);
            p.Fill(Sd.Circle(0.3f, 0.25f, 0.028f), Ink);
            V68_Crest(p, true);
            return new DoodleArt("v68_bird_stare", p);
        }

        /// <summary>"No paparazzi.": l'ala alzata copre tutta la faccia (si vede solo il becco che spunta).</summary>
        public static DoodleArt V68_BirdHide()
        {
            var p = new DoodlePainter(-0.62f, -0.5f, 0.7f, 0.62f, 150) { Ink = 0.045f };
            V68_BirdBase(p);
            V68_ClosedBeak(p);
            V68_Crest(p, false);
            // l'ala parte dalla spalla e sale davanti alla testa
            Sdf wing = Sd.Rotate(Sd.Ellipse(0.24f, 0.2f, 0.15f, 0.31f), 0.24f, 0.2f, -24f);
            Body(p, wing, BirdShade, Rgba.Hex("24689A"));
            foreach (float k in new[] { -0.06f, 0.04f })
                p.Line(Rgba.Hex("24689A"), 0.018f, 0.25f + k, 0.38f, 0.26f + k * 0.6f, 0.06f);
            return new DoodleArt("v68_bird_hide", p);
        }

        /// <summary>In volo: ali alzate, zampe raccolte, becco chiuso.</summary>
        public static DoodleArt V68_BirdFly()
        {
            var p = new DoodlePainter(-0.66f, -0.42f, 0.66f, 0.8f, 150) { Ink = 0.045f };
            // l'ala dietro (più scura)
            Sdf back = Sd.Rotate(Sd.Ellipse(-0.18f, 0.38f, 0.12f, 0.3f), -0.18f, 0.38f, 32f);
            p.Shape(back, Rgba.Hex("24689A"));
            V68_BirdBase(p, false);
            p.InkLine(0.03f, -0.02f, -0.32f, -0.12f, -0.36f);
            V68_ClosedBeak(p);
            p.Fill(Sd.Circle(0.27f, 0.25f, 0.05f), Ink);
            p.Fill(Sd.Circle(0.26f, 0.27f, 0.018f), Rgba.White);
            V68_Crest(p, false);
            Sdf front = Sd.Rotate(Sd.Ellipse(-0.02f, 0.38f, 0.14f, 0.34f), -0.02f, 0.38f, 18f);
            Body(p, front, BirdBlue, BirdShade);
            p.Line(BirdShade, 0.018f, -0.06f, 0.62f, -0.02f, 0.2f);
            return new DoodleArt("v68_bird_fly", p);
        }

        /// <summary>Il fotobomba: occhio felice a "^", linguaccia, l'ala alzata che fa la V con due piume.</summary>
        public static DoodleArt V68_BirdPeace()
        {
            var p = new DoodlePainter(-0.62f, -0.5f, 0.7f, 0.95f, 150) { Ink = 0.045f };
            V68_BirdBase(p);
            // becco aperto con la linguaccia
            p.Shape(Sd.Polygon(0.38f, 0.22f, 0.62f, 0.27f, 0.4f, 0.16f), V68Beak, 0.025f);
            p.Shape(Sd.Polygon(0.38f, 0.12f, 0.58f, 0.04f, 0.4f, 0.16f), V68Beak, 0.025f);
            p.Shape(Sd.Capsule(0.46f, 0.13f, 0.54f, 0.02f, 0.04f), Rgba.Hex("FF6F8A"), 0.02f);
            p.InkLine(0.025f, 0.21f, 0.24f, 0.26f, 0.29f, 0.31f, 0.24f);   // occhio felice
            p.Fill(Sd.Circle(0.3f, 0.1f, 0.045f), BlushColor.WithAlpha(0.7f));
            V68_Crest(p, true);
            // l'ala sale dal fianco e finisce con due piume aperte a V
            Sdf arm = Sd.Capsule(-0.12f, 0.0f, -0.2f, 0.5f, 0.09f);
            Sdf f1 = Sd.Capsule(-0.2f, 0.5f, -0.3f, 0.86f, 0.045f);
            Sdf f2 = Sd.Capsule(-0.2f, 0.5f, -0.07f, 0.85f, 0.045f);
            Body(p, Sd.Union(arm, f1, f2), BirdBlue, BirdShade);
            return new DoodleArt("v68_bird_peace", p);
        }

        /// <summary>Occhialini da sole sull'occhio dell'uccellino (coordinate di s41h_bird), con la stanghetta verso la nuca.</summary>
        public static DoodleArt V68_BirdShades()
        {
            var p = new DoodlePainter(0.08f, 0.13f, 0.42f, 0.36f, 300) { Ink = 0.016f, Wobble = 0.002f };
            p.Line(Ink, 0.022f, 0.14f, 0.3f, 0.22f, 0.27f);
            p.Shape(Sd.Box(0.28f, 0.25f, 0.085f, 0.06f, 0.035f), Rgba.Hex("1E1A26"));
            p.Line(Rgba.White.WithAlpha(0.75f), 0.016f, 0.24f, 0.28f, 0.27f, 0.22f);
            p.Line(Ink, 0.016f, 0.36f, 0.27f, 0.39f, 0.29f);
            return new DoodleArt("v68_bird_shades", p);
        }

        /// <summary>Occhi chiusi dell'omino s41b_gr_man (stesse coordinate): pelle sopra gli occhi e due archetti.</summary>
        public static DoodleArt V68_Blink()
        {
            var p = new DoodlePainter(-0.13f, 0.22f, 0.13f, 0.36f, 600) { Ink = 0.008f, Wobble = 0.001f };
            foreach (float x in new[] { -0.06f, 0.06f })
            {
                p.Fill(Sd.Circle(x, 0.29f, 0.057f), Rgba.Hex("FFD7B5"), false);
                p.InkLine(0.011f, x - 0.035f, 0.295f, x - 0.015f, 0.28f, x + 0.015f, 0.28f, x + 0.035f, 0.295f);
            }
            return new DoodleArt("v68_blink", p);
        }

        /// <summary>Panchina del parco (centro 0,0; piedi in y -0.5; larga 1.8).</summary>
        public static DoodleArt V68_Bench()
        {
            var p = new DoodlePainter(-0.98f, -0.56f, 0.98f, 0.56f, 110) { Ink = 0.04f };
            Rgba wood = Rgba.Hex("C9884E"), woodShade = Rgba.Hex("9E6434"), iron = Rgba.Hex("3D3A48");
            foreach (float x in new[] { -0.72f, 0.72f })
            {
                p.Shape(Sd.Box(x, -0.3f, 0.05f, 0.2f), iron);
                p.Shape(Sd.Box(x, 0.12f, 0.04f, 0.32f), iron);
            }
            foreach (float y in new[] { 0.36f, 0.16f })
            {
                p.Shape(Sd.Box(0f, y, 0.9f, 0.07f, 0.03f), wood);
                p.Fill(Sd.Box(0f, y - 0.04f, 0.88f, 0.02f), woodShade);
            }
            p.Shape(Sd.Box(0f, -0.08f, 0.92f, 0.08f, 0.03f), wood);
            p.Fill(Sd.Box(0f, -0.13f, 0.9f, 0.025f), woodShade);
            return new DoodleArt("v68_bench", p);
        }

        /// <summary>Lampione del parco (base in 0,0; lanterna in cima a y 2.9).</summary>
        public static DoodleArt V68_LampPost()
        {
            var p = new DoodlePainter(-0.36f, -0.06f, 0.36f, 3.25f, 90) { Ink = 0.04f };
            Rgba iron = Rgba.Hex("3D3A48");
            p.Shape(Sd.Box(0f, 0.08f, 0.17f, 0.08f, 0.03f), iron);
            p.Shape(Sd.Box(0f, 1.35f, 0.05f, 1.25f), iron);
            p.Shape(Sd.Polygon(-0.2f, 2.6f, 0.2f, 2.6f, 0.26f, 3.0f, -0.26f, 3.0f), Rgba.Hex("FFF1B0"));
            p.Shape(Sd.Polygon(-0.3f, 3.0f, 0.3f, 3.0f, 0f, 3.18f), iron);
            p.Shape(Sd.Box(0f, 2.57f, 0.22f, 0.04f), iron);
            return new DoodleArt("v68_lamp_post", p);
        }

        /// <summary>Pulsante di scatto: anello bianco e disco bianco (centro 0,0, raggio 0.5).</summary>
        public static DoodleArt V68_ShutterButton()
        {
            var p = new DoodlePainter(-0.56f, -0.56f, 0.56f, 0.56f, 140) { Ink = 0.03f, Wobble = 0.003f };
            p.Shape(Sd.Subtract(Sd.Circle(0f, 0f, 0.5f), Sd.Circle(0f, 0f, 0.43f)), Rgba.White);
            Body(p, Sd.Circle(0f, 0f, 0.37f), Rgba.White, Rgba.Hex("DADDE6"));
            return new DoodleArt("v68_shutter_btn", p);
        }

        /// <summary>Icona "gira la fotocamera": due frecce che girano in tondo dentro un disco scuro (centro 0,0, raggio 0.42).</summary>
        public static DoodleArt V68_FlipIcon()
        {
            var p = new DoodlePainter(-0.48f, -0.48f, 0.48f, 0.48f, 150) { Ink = 0.025f, Wobble = 0.003f };
            p.Shape(Sd.Circle(0f, 0f, 0.42f), Rgba.Hex("3A3644", 0.9f));
            Sdf ring = Sd.Subtract(Sd.Circle(0f, 0f, 0.25f), Sd.Circle(0f, 0f, 0.18f));
            // due archi (sopra e sotto) separati da due tagli obliqui
            Sdf cuts = Sd.Union(Sd.Rotate(Sd.Box(0.22f, 0f, 0.09f, 0.035f), 0f, 0f, 20f), Sd.Rotate(Sd.Box(-0.22f, 0f, 0.09f, 0.035f), 0f, 0f, 20f));
            p.Fill(Sd.Subtract(ring, cuts), Rgba.White, false);
            // punte delle frecce (una per arco, versi opposti: girano)
            p.Fill(Sd.Polygon(0.13f, 0.02f, 0.33f, 0.02f, 0.23f, -0.12f), Rgba.White, false);
            p.Fill(Sd.Polygon(-0.13f, -0.02f, -0.33f, -0.02f, -0.23f, 0.12f), Rgba.White, false);
            return new DoodleArt("v68_flip_icon", p);
        }

        /// <summary>Icona del flash: fulmine bianco dentro un disco scuro (centro 0,0, raggio 0.36).</summary>
        public static DoodleArt V68_FlashIcon()
        {
            var p = new DoodlePainter(-0.42f, -0.42f, 0.42f, 0.42f, 150) { Ink = 0.025f, Wobble = 0.003f };
            p.Shape(Sd.Circle(0f, 0f, 0.36f), Rgba.Hex("3A3644", 0.75f));
            p.Fill(Sd.Polygon(0.05f, 0.24f, -0.13f, -0.02f, -0.01f, -0.02f, -0.06f, -0.24f, 0.13f, 0.04f, 0.01f, 0.04f), Rgba.White, false);
            return new DoodleArt("v68_flash_icon", p);
        }

        /// <summary>Riquadro gialla della messa a fuoco: quattro angoli (centro 0,0, lato 1).</summary>
        public static DoodleArt V68_Focus()
        {
            var p = new DoodlePainter(-0.55f, -0.55f, 0.55f, 0.55f, 120) { Ink = 0f, Wobble = 0.002f };
            Rgba y = Rgba.Hex("FFD84A");
            foreach (float sx in new[] { -1f, 1f })
                foreach (float sy in new[] { -1f, 1f })
                    p.Line(y, 0.05f, sx * 0.5f, sy * 0.24f, sx * 0.5f, sy * 0.5f, sx * 0.24f, sy * 0.5f);
            p.Fill(Sd.Circle(0f, 0f, 0.035f), y.WithAlpha(0.8f), false);
            return new DoodleArt("v68_focus", p);
        }
    }
}
