using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli nuovi della versione 0.39 (gruppo E): colori, pluriball, chitarra, gru.</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV39E() => new DoodleArt[]
        {
            E39MixerStudio(), E39MixerEasel(), E39MixerSwatch(), E39MixerFish(), E39MixerCat(), E39MixerArm(),
            E39MixerTube("red", Rgba.Hex("E8433F")), E39MixerTube("yellow", Rgba.Hex("FFD23C")), E39MixerTube("blue", Rgba.Hex("3E7BE0")),
            E39MixerTube("white", Rgba.Hex("FFFFFF")), E39MixerTube("black", Rgba.Hex("2E2A33")),
            E39MixerBucket(), E39MixerSurface(), E39MixerDrop(), E39MixerRipple(), E39Pill(),
            E39WrapSheet(false), E39WrapSheet(true), E39WrapBubble(), E39WrapPopped(),
            E39TuneStage(), E39TuneGuitar(), E39TunePeg(), E39TuneFork(), E39TuneMeter(), E39TuneCat(),
            E39ClawCabinet(), E39Claw(false), E39Claw(true), E39ClawChute(), E39ClawArrow(),
            E39Plush("cat"), E39Plush("fish"), E39Plush("mouse"), E39Plush("key"),
        };

        static readonly Rgba E39Outline = Rgba.Hex("2B2133");

        // ---------- Paint mixer ----------

        /// <summary>Studio del pittore: parete color crema con boiserie, pavimento in legno, finestrella (6×10.4, centro 0,0).</summary>
        public static DoodleArt E39MixerStudio()
        {
            var p = new DoodlePainter(-3.1f, -5.2f, 3.1f, 5.2f, 60) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.2f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("FFE9C9"), Rgba.Hex("F7D7B0"), Clamp01E((3.5f - y) / 8f)));
            // carta da parati a righine
            for (float x = -3f; x < 3.1f; x += 0.5f) p.Line(Rgba.Hex("F2CFA3").WithAlpha(0.7f), 0.06f, x, -1.0f, x, 5.2f);
            // quadretto appeso (un gatto stilizzato), sotto il misuratore
            Sdf frame = Sd.Box(-0.05f, 2.1f, 0.42f, 0.36f, 0.04f);
            p.Shape(frame, Rgba.Hex("C98F5E"), 0.05f);
            p.Fill(Sd.Box(-0.05f, 2.1f, 0.32f, 0.26f), Rgba.Hex("BFE6FF"));
            p.Fill(Sd.Circle(-0.05f, 2.02f, 0.15f), Rgba.Hex("F6A04D"));
            p.Fill(Sd.Polygon(-0.2f, 2.08f, -0.17f, 2.28f, -0.08f, 2.14f), Rgba.Hex("F6A04D"));
            p.Fill(Sd.Polygon(0.1f, 2.08f, 0.07f, 2.28f, -0.02f, 2.14f), Rgba.Hex("F6A04D"));
            p.InkLine(0.03f, -0.05f, 2.46f, -0.05f, 2.62f);
            // pavimento e battiscopa
            p.FillFunc(Sd.Box(0, -3.15f, 3.3f, 2.15f), (x, y) => Rgba.Lerp(Rgba.Hex("C98A55"), Rgba.Hex("A86B3C"), Clamp01E((-1f - y) / 4f)));
            for (float y = -1.45f; y > -5.3f; y -= 0.6f) p.Line(Rgba.Hex("8E5A32"), 0.03f, -3.2f, y, 3.2f, y);
            p.Shape(Sd.Box(0, -0.95f, 3.3f, 0.07f), Rgba.Hex("E7B98A"), 0.04f);
            // macchie di colore sul pavimento
            p.Fill(Sd.Ellipse(-2.2f, -4.6f, 0.3f, 0.1f), Rgba.Hex("3E7BE0").WithAlpha(0.6f));
            p.Fill(Sd.Ellipse(2.3f, -2.6f, 0.22f, 0.08f), Rgba.Hex("E8433F").WithAlpha(0.6f));
            p.Fill(Sd.Ellipse(1.5f, -4.9f, 0.25f, 0.08f), Rgba.Hex("FFD23C").WithAlpha(0.7f));
            return new DoodleArt("mixer_studio_bg", p);
        }

        /// <summary>Cavalletto con tela bianca: centro della tela in (0,0), mezza larghezza 0.85, mezza altezza 0.68; piedi in y -2.</summary>
        public static DoodleArt E39MixerEasel()
        {
            var p = new DoodlePainter(-1.15f, -2.1f, 1.15f, 1.05f, 110) { Ink = 0.045f };
            // gambe
            p.Shape(Sd.Capsule(-0.7f, -2.0f, -0.15f, 0.95f, 0.06f), Wood);
            p.Shape(Sd.Capsule(0.7f, -2.0f, 0.15f, 0.95f, 0.06f), Wood);
            p.Shape(Sd.Capsule(0f, -1.8f, 0f, 0.9f, 0.055f), WoodShade);
            // mensola sotto la tela
            Body(p, Sd.Box(0, -0.78f, 1.0f, 0.07f, 0.03f), Wood, WoodShade);
            // tela
            Sdf canvas = Sd.Box(0, 0, 0.85f, 0.68f, 0.03f);
            p.Fill(canvas, Rgba.Hex("FFFDF6"));
            p.Fill(Sd.Crescent(canvas, -0.04f, 0.05f), Rgba.Hex("EDE6D6"));
            p.Shape(canvas, Rgba.Clear);
            p.Shape(Sd.Box(0, 0.74f, 0.25f, 0.06f, 0.02f), WoodShade, 0.03f);
            return new DoodleArt("mixer_easel", p);
        }

        /// <summary>Macchia di colore bianca (si tinge), centro 0,0, larga circa 1.</summary>
        public static DoodleArt E39MixerSwatch()
        {
            var p = new DoodlePainter(-0.6f, -0.55f, 0.6f, 0.55f, 130) { Ink = 0.035f, Wobble = 0.03f, WobbleFreq = 4f };
            Sdf blob = Sd.Union(Sd.Ellipse(0, 0, 0.48f, 0.4f), Sd.Circle(0.38f, -0.25f, 0.11f), Sd.Circle(-0.42f, 0.22f, 0.09f), Sd.Circle(0.2f, 0.35f, 0.12f));
            p.Shape(blob, Rgba.White);
            p.Fill(Sd.Ellipse(-0.18f, 0.16f, 0.12f, 0.06f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("mixer_swatch", p);
        }

        /// <summary>Sagoma di pesce (salmone) bianca da tingere, centro 0,0, guarda a destra.</summary>
        public static DoodleArt E39MixerFish()
        {
            var p = new DoodlePainter(-0.75f, -0.42f, 0.75f, 0.42f, 130) { Ink = 0.035f };
            Sdf body = Sd.Union(Sd.Ellipse(0.08f, 0, 0.5f, 0.27f), Sd.Polygon(-0.35f, 0, -0.68f, 0.3f, -0.62f, 0, -0.68f, -0.3f));
            p.Shape(Sd.Polygon(-0.1f, 0.2f, 0.2f, 0.38f, 0.28f, 0.18f), Rgba.White);
            p.Shape(body, Rgba.White);
            p.InkLine(0.025f, 0.28f, 0.2f, 0.22f, 0f, 0.28f, -0.2f);
            p.Fill(Sd.Circle(0.4f, 0.07f, 0.05f), E39Outline);
            p.Fill(Sd.Circle(0.39f, 0.09f, 0.017f), Rgba.White);
            p.Fill(Sd.Ellipse(-0.05f, -0.12f, 0.25f, 0.06f), Rgba.White.WithAlpha(0.5f));
            return new DoodleArt("mixer_fish_paint", p);
        }

        /// <summary>Gatto pittore in piedi con basco e grembiule: piedi in y -1.1, testa in (0, 0.55). La spalla destra in (0.3,-0.15).</summary>
        public static DoodleArt E39MixerCat()
        {
            var p = new DoodlePainter(-0.9f, -1.2f, 0.9f, 1.45f, 120) { Ink = 0.045f };
            // coda
            p.Shape(Sd.Polyline(0.08f, -0.35f, -0.85f, -0.7f, -0.7f, -0.78f, -0.3f, -0.62f, -0.05f), CatOrange);
            // zampe
            foreach (float x in new[] { -0.22f, 0.22f }) p.Shape(Sd.Ellipse(x, -1.02f, 0.18f, 0.09f), Rgba.Hex("FFE3C2"));
            // corpo e grembiule
            Sdf body = Sd.Ellipse(0, -0.45f, 0.48f, 0.58f);
            Body(p, body, CatOrange, CatShade);
            Sdf apron = Sd.Intersect(body, Sd.Box(0, -0.6f, 0.34f, 0.5f, 0.1f));
            p.Shape(apron, Rgba.Hex("EAF2FF"), 0.03f);
            p.Fill(Sd.Circle(-0.12f, -0.55f, 0.06f), Rgba.Hex("E8433F"));
            p.Fill(Sd.Circle(0.1f, -0.8f, 0.05f), Rgba.Hex("3E7BE0"));
            p.Fill(Sd.Circle(0.15f, -0.4f, 0.045f), Rgba.Hex("FFD23C"));
            // braccio sinistro (dietro, con la tavolozza)
            p.Shape(Sd.Capsule(-0.4f, -0.2f, -0.62f, -0.55f, 0.09f), CatOrange);
            Sdf palette = Sd.Subtract(Sd.Ellipse(-0.68f, -0.62f, 0.24f, 0.16f), Sd.Circle(-0.6f, -0.68f, 0.04f));
            p.Shape(palette, Rgba.Hex("E3B47A"), 0.03f);
            p.Fill(Sd.Circle(-0.8f, -0.58f, 0.04f), Rgba.Hex("E8433F"));
            p.Fill(Sd.Circle(-0.7f, -0.52f, 0.04f), Rgba.Hex("FFD23C"));
            p.Fill(Sd.Circle(-0.58f, -0.54f, 0.04f), Rgba.Hex("3E7BE0"));
            // testa
            Sdf ears = Sd.Union(Sd.Polygon(-0.42f, 0.6f, -0.36f, 1.05f, -0.1f, 0.85f), Sd.Polygon(0.42f, 0.6f, 0.36f, 1.05f, 0.1f, 0.85f));
            p.Shape(ears, CatOrange);
            p.Fill(Sd.Grow(ears, -0.06f), CatPink);
            Sdf head = Sd.Ellipse(0, 0.52f, 0.46f, 0.38f);
            Body(p, head, CatOrange, CatShade);
            p.Fill(Sd.Ellipse(0, 0.38f, 0.2f, 0.12f), Rgba.Hex("FFE3C2"));
            foreach (float x in new[] { -0.17f, 0.17f })
            {
                p.Shape(Sd.Ellipse(x, 0.58f, 0.08f, 0.1f), Rgba.Hex("C8F07A"), 0.025f);
                p.Fill(Sd.Ellipse(x + 0.01f, 0.57f, 0.025f, 0.07f), E39Outline);
                p.Fill(Sd.Circle(x - 0.025f, 0.61f, 0.017f), Rgba.White);
            }
            p.Shape(Sd.Polygon(-0.05f, 0.45f, 0.05f, 0.45f, 0, 0.4f), CatPink, 0.02f);
            p.InkLine(0.018f, 0, 0.4f, -0.05f, 0.34f, -0.1f, 0.36f);
            p.InkLine(0.018f, 0, 0.4f, 0.05f, 0.34f, 0.1f, 0.36f);
            foreach (float dy in new[] { 0f, 0.06f })
            {
                p.InkLine(0.012f, -0.2f, 0.42f + dy, -0.5f, 0.45f + dy * 1.6f);
                p.InkLine(0.012f, 0.2f, 0.42f + dy, 0.5f, 0.45f + dy * 1.6f);
            }
            // basco rosso
            Sdf beret = Sd.Rotate(Sd.Ellipse(-0.05f, 0.88f, 0.36f, 0.13f), -0.05f, 0.88f, 12f);
            Body(p, beret, Rgba.Hex("D93A4E"), Rgba.Hex("A82638"));
            p.InkLine(0.03f, -0.06f, 1.0f, -0.02f, 1.1f);
            // baffi da artista
            p.InkLine(0.03f, -0.02f, 0.31f, -0.14f, 0.3f, -0.22f, 0.36f);
            p.InkLine(0.03f, 0.02f, 0.31f, 0.14f, 0.3f, 0.22f, 0.36f);
            return new DoodleArt("mixer_cat", p);
        }

        /// <summary>Braccio destro del pittore col pennello: spalla in (0,0), punta del pennello verso destra (circa 0.85, 0.25).</summary>
        public static DoodleArt E39MixerArm()
        {
            var p = new DoodlePainter(-0.12f, -0.2f, 1.0f, 0.42f, 140) { Ink = 0.035f };
            p.Shape(Sd.Capsule(0, 0, 0.36f, 0.02f, 0.09f), CatOrange);
            p.Shape(Sd.Circle(0.4f, 0.03f, 0.1f), Rgba.Hex("FFE3C2"));
            p.Shape(Sd.Capsule(0.32f, -0.08f, 0.72f, 0.2f, 0.03f), Rgba.Hex("B5733C"), 0.025f);
            p.Shape(Sd.Box(0.75f, 0.22f, 0.04f, 0.03f), Rgba.Hex("B8C2CC"), 0.02f);
            Sdf tip = Sd.Rotate(Sd.Ellipse(0.85f, 0.29f, 0.09f, 0.045f), 0.85f, 0.29f, 35f);
            p.Shape(tip, Rgba.Hex("FF8C66"), 0.025f);
            return new DoodleArt("mixer_cat_arm", p);
        }

        /// <summary>Tubetto di colore in piedi, tappo in alto (beccuccio in 0, 0.62), fondo in y -0.65.</summary>
        public static DoodleArt E39MixerTube(string name, Rgba color)
        {
            var p = new DoodlePainter(-0.42f, -0.75f, 0.42f, 0.78f, 140) { Ink = 0.04f };
            Rgba shade = Rgba.Lerp(color, Rgba.Hex("2B2133"), 0.25f);
            Sdf tube = Sd.Polygon(-0.28f, 0.32f, 0.28f, 0.32f, 0.34f, -0.5f, -0.34f, -0.5f);
            Body(p, tube, Rgba.Hex("E9EDF3"), Rgba.Hex("C5CCD8"));
            // fascia colorata con goccia
            Sdf band = Sd.Intersect(tube, Sd.Box(0, -0.12f, 0.4f, 0.26f));
            p.Fill(band, color);
            p.Fill(Sd.Intersect(band, Sd.Crescent(tube, -0.06f, 0.0f)), shade);
            p.Shape(band, Rgba.Clear, 0.03f);
            p.Shape(Sd.Union(Sd.Circle(0, -0.1f, 0.1f), Sd.Polygon(-0.085f, -0.06f, 0.085f, -0.06f, 0, 0.1f)), Rgba.White.WithAlpha(name == "white" ? 0f : 0.9f), 0.02f);
            // piega in fondo e spalla
            p.Shape(Sd.Box(0, -0.56f, 0.37f, 0.08f, 0.02f), Rgba.Hex("C5CCD8"), 0.035f);
            for (float x = -0.25f; x < 0.3f; x += 0.1f) p.InkLine(0.012f, x, -0.6f, x, -0.52f);
            p.Shape(Sd.Polygon(-0.28f, 0.32f, 0.28f, 0.32f, 0.12f, 0.45f, -0.12f, 0.45f), Rgba.Hex("D8DEE8"), 0.035f);
            // tappo colorato
            Sdf cap = Sd.Box(0, 0.57f, 0.12f, 0.13f, 0.04f);
            Body(p, cap, color, shade);
            for (float x = -0.06f; x < 0.08f; x += 0.06f) p.Line(shade, 0.015f, x, 0.48f, x, 0.66f);
            p.Fill(Sd.Capsule(-0.2f, 0.2f, -0.24f, -0.35f, 0.025f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("mixer_tube_" + name, p);
        }

        /// <summary>Secchio di latta: bordo della bocca in y 0 (ellisse 0.72×0.2), fondo in y -1.05.</summary>
        public static DoodleArt E39MixerBucket()
        {
            var p = new DoodlePainter(-0.95f, -1.15f, 0.95f, 0.75f, 120) { Ink = 0.045f };
            // manico
            p.InkLine(0.05f, -0.72f, -0.15f, -0.6f, 0.45f, 0f, 0.65f, 0.6f, 0.45f, 0.72f, -0.15f);
            p.Line(Rgba.Hex("B8C2CC"), 0.025f, -0.72f, -0.15f, -0.6f, 0.45f, 0f, 0.65f, 0.6f, 0.45f, 0.72f, -0.15f);
            Sdf body = Sd.Union(Sd.Polygon(-0.72f, 0, 0.72f, 0, 0.56f, -1.0f, -0.56f, -1.0f), Sd.Ellipse(0, -1.0f, 0.56f, 0.08f));
            Body(p, body, Rgba.Hex("C9D3DE"), Rgba.Hex("9AA7B7"));
            p.Line(Rgba.Hex("9AA7B7"), 0.04f, -0.66f, -0.35f, 0.66f, -0.35f);
            p.Line(Rgba.Hex("9AA7B7"), 0.04f, -0.6f, -0.75f, 0.6f, -0.75f);
            p.Fill(Sd.Capsule(-0.45f, -0.12f, -0.38f, -0.85f, 0.04f), Rgba.White.WithAlpha(0.6f));
            // colatura di vernice sul bordo
            p.Shape(Sd.Union(Sd.Capsule(0.32f, -0.04f, 0.32f, -0.35f, 0.05f), Sd.Capsule(-0.1f, -0.04f, -0.1f, -0.2f, 0.04f)), Rgba.Hex("8E6CEF"), 0.025f);
            // bocca vuota (l'interno), poi l'orlo
            p.Fill(Sd.Ellipse(0, 0, 0.72f, 0.2f), Rgba.Hex("6D7889"));
            p.Fill(Sd.Ellipse(0, -0.06f, 0.62f, 0.13f), Rgba.Hex("57616F"));
            p.Shape(Sd.Subtract(Sd.Ellipse(0, 0, 0.76f, 0.23f), Sd.Ellipse(0, 0, 0.68f, 0.17f)), Rgba.Hex("E3E9F0"), 0.03f);
            p.Shape(Sd.Circle(-0.72f, -0.15f, 0.05f), Rgba.Hex("8D93A6"), 0.02f);
            p.Shape(Sd.Circle(0.72f, -0.15f, 0.05f), Rgba.Hex("8D93A6"), 0.02f);
            return new DoodleArt("mixer_bucket", p);
        }

        /// <summary>Superficie della vernice nel secchio (bianca, si tinge), centro 0,0.</summary>
        public static DoodleArt E39MixerSurface()
        {
            var p = new DoodlePainter(-0.72f, -0.2f, 0.72f, 0.2f, 140) { Ink = 0.02f, Wobble = 0.004f };
            p.Fill(Sd.Ellipse(0, 0, 0.67f, 0.16f), Rgba.White);
            p.Fill(Sd.Ellipse(-0.22f, 0.05f, 0.2f, 0.04f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("mixer_paint_surface", p);
        }

        /// <summary>Goccia di colore bianca (si tinge), punta in alto, centro 0,0.</summary>
        public static DoodleArt E39MixerDrop()
        {
            var p = new DoodlePainter(-0.17f, -0.17f, 0.17f, 0.27f, 160) { Ink = 0.03f };
            Sdf d = Sd.Union(Sd.Circle(0, -0.04f, 0.12f), Sd.Polygon(-0.105f, 0, 0.105f, 0, 0, 0.24f));
            p.Shape(d, Rgba.White);
            p.Fill(Sd.Ellipse(-0.04f, 0f, 0.03f, 0.05f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("mixer_drop", p);
        }

        /// <summary>Anello d'onda (ellisse bianca, si tinge e si allarga).</summary>
        public static DoodleArt E39MixerRipple()
        {
            var p = new DoodlePainter(-0.5f, -0.16f, 0.5f, 0.16f, 140) { Wobble = 0.004f };
            p.Fill(Sd.Subtract(Sd.Ellipse(0, 0, 0.46f, 0.13f), Sd.Ellipse(0, 0, 0.39f, 0.09f)), Rgba.White);
            return new DoodleArt("mixer_ripple", p);
        }

        /// <summary>Pulsante a pillola bianco (si tinge), 1.4×0.6, centro 0,0.</summary>
        public static DoodleArt E39Pill()
        {
            var p = new DoodlePainter(-0.78f, -0.38f, 0.78f, 0.38f, 140) { Ink = 0.045f };
            Sdf pill = Sd.Box(0, 0, 0.7f, 0.3f, 0.28f);
            p.Fill(pill, Rgba.White);
            p.Fill(Sd.Intersect(pill, Sd.Box(0, -0.22f, 0.8f, 0.1f)), Rgba.Hex("D5D9E2"));
            p.Shape(pill, Rgba.Clear);
            p.Fill(Sd.Capsule(-0.42f, 0.16f, -0.1f, 0.16f, 0.03f), Rgba.White.WithAlpha(0.9f));
            return new DoodleArt("e39_pill", p);
        }

        // ---------- Bubble wrap ----------

        /// <summary>Foglio di pluriball (4.8×5.6, centro 0,0): fronte con l'angolo arricciato in basso a destra, retro più opaco.</summary>
        public static DoodleArt E39WrapSheet(bool back)
        {
            var p = new DoodlePainter(-2.55f, -2.95f, 2.55f, 2.95f, 80) { Ink = 0.05f, Wobble = 0.01f };
            Sdf sheet = back
                ? Sd.Polygon(-2.4f, 2.8f, 2.4f, 2.8f, 2.4f, -2.8f, -1.95f, -2.8f, -2.4f, -2.35f)
                : Sd.Polygon(-2.4f, 2.8f, 2.4f, 2.8f, 2.4f, -2.35f, 1.95f, -2.8f, -2.4f, -2.8f);
            Rgba top = back ? Rgba.Hex("D7E4EE") : Rgba.Hex("E6F4FF");
            Rgba bottom = back ? Rgba.Hex("B9CBDB") : Rgba.Hex("C9E3F7");
            p.FillFunc(sheet, (x, y) => Rgba.Lerp(top, bottom, Clamp01E((2.8f - y) / 5.6f)).WithAlpha(0.95f));
            // riflessi diagonali sulla plastica
            p.Fill(Sd.Intersect(sheet, Sd.Rotate(Sd.Box(-0.8f, 0.8f, 3.5f, 0.12f), -0.8f, 0.8f, 35f)), Rgba.White.WithAlpha(0.45f));
            p.Fill(Sd.Intersect(sheet, Sd.Rotate(Sd.Box(0.9f, -1.0f, 3.5f, 0.06f), 0.9f, -1.0f, 35f)), Rgba.White.WithAlpha(0.4f));
            // tracce delle bolle (il "nido" di ogni bolla)
            for (int c = 0; c < 6; c++)
                for (int r = 0; r < 7; r++)
                {
                    float x = (c - 2.5f) * 0.74f, y = (r - 3f) * 0.74f;
                    p.Fill(Sd.Subtract(Sd.Circle(x, y, 0.33f), Sd.Circle(x, y, 0.29f)), Rgba.Hex("9CB8CF").WithAlpha(back ? 0.35f : 0.5f));
                }
            if (back)
            {
                p.Fill(Sd.Box(-0.9f, 2.35f, 1.3f, 0.22f, 0.08f), Rgba.Hex("9CB8CF").WithAlpha(0.45f));
                p.Line(Rgba.Hex("7F9AB2").WithAlpha(0.8f), 0.04f, -2.0f, 2.35f, 0.2f, 2.35f);
            }
            p.Shape(sheet, Rgba.Clear);
            // angolo arricciato
            float[] curl = back ? new[] { -2.4f, -2.35f, -1.95f, -2.8f, -1.95f, -2.35f } : new[] { 2.4f, -2.35f, 1.95f, -2.8f, 1.95f, -2.35f };
            p.Shape(Sd.Polygon(curl), back ? Rgba.Hex("E6F4FF") : Rgba.Hex("B9CBDB"), 0.035f);
            return new DoodleArt(back ? "wrap_sheet_back" : "wrap_sheet_front", p);
        }

        /// <summary>Bolla intera: cupola trasparente lucida, raggio 0.3, centro 0,0.</summary>
        public static DoodleArt E39WrapBubble()
        {
            var p = new DoodlePainter(-0.36f, -0.36f, 0.36f, 0.36f, 160) { Ink = 0.03f, Wobble = 0.006f };
            Sdf b = Sd.Circle(0, 0, 0.3f);
            p.FillFunc(b, (x, y) =>
            {
                float d = (float)Math.Sqrt(x * x + y * y) / 0.3f;
                return Rgba.Lerp(Rgba.Hex("F4FBFF"), Rgba.Hex("A9CBE6"), d * d).WithAlpha(0.92f);
            });
            p.Fill(Sd.Crescent(b, -0.05f, 0.06f), Rgba.Hex("8FB4D3").WithAlpha(0.6f));
            p.Shape(b, Rgba.Clear);
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.1f, 0.12f, 0.1f, 0.05f), -0.1f, 0.12f, 35f), Rgba.White);
            p.Fill(Sd.Circle(0.12f, -0.1f, 0.025f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("wrap_bubble", p);
        }

        /// <summary>Bolla scoppiata: plastica afflosciata e grinzosa, centro 0,0.</summary>
        public static DoodleArt E39WrapPopped()
        {
            var p = new DoodlePainter(-0.36f, -0.36f, 0.36f, 0.36f, 160) { Ink = 0.022f, Wobble = 0.03f, WobbleFreq = 9f };
            Sdf b = Sd.Circle(0, 0, 0.27f);
            p.Shape(b, Rgba.Hex("C3D8E8").WithAlpha(0.9f));
            p.InkLine(0.018f, -0.15f, 0.08f, -0.02f, 0.02f, 0.06f, 0.12f, 0.16f, 0.04f);
            p.InkLine(0.018f, -0.12f, -0.1f, 0.0f, -0.05f, 0.1f, -0.14f);
            p.Line(Rgba.White.WithAlpha(0.8f), 0.02f, -0.08f, 0.16f, 0.04f, 0.18f);
            return new DoodleArt("wrap_bubble_popped", p);
        }

        // ---------- Out of tune ----------

        /// <summary>Palco rock: fondale scuro viola con fari e pedana (6.2×10.4, centro 0,0).</summary>
        public static DoodleArt E39TuneStage()
        {
            var p = new DoodlePainter(-3.1f, -5.2f, 3.1f, 5.2f, 60) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.2f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("3A2456"), Rgba.Hex("170F26"), Clamp01E((y + 5f) / 10f)));
            // fasci di luce dai fari
            foreach (float sx in new[] { -2.2f, 0.2f, 2.4f })
            {
                float tx = sx * 0.3f;
                p.FillFunc(Sd.Polygon(sx - 0.15f, 5.1f, sx + 0.15f, 5.1f, tx + 1.3f, -4.0f, tx - 1.3f, -4.0f), (x, y) =>
                    Rgba.Hex("FFE8A8").WithAlpha(0.12f * Clamp01E((y + 4.5f) / 9f)));
                p.Shape(Sd.Box(sx, 4.95f, 0.22f, 0.18f, 0.05f), Rgba.Hex("4A4E5C"), 0.04f);
            }
            // casse ai lati
            foreach (float sx in new[] { -2.6f, 2.6f })
            {
                Body(p, Sd.Box(sx, -3.4f, 0.55f, 1.0f, 0.05f), Rgba.Hex("2E2A33"), Rgba.Hex("1C1A21"));
                p.Shape(Sd.Circle(sx, -3.0f, 0.3f), Rgba.Hex("4A4E5C"), 0.04f);
                p.Shape(Sd.Circle(sx, -3.9f, 0.2f), Rgba.Hex("4A4E5C"), 0.04f);
            }
            // pedana
            p.Shape(Sd.Box(0, -4.85f, 3.3f, 0.45f), Rgba.Hex("5B3A2A"), 0.05f);
            for (float x = -2.8f; x < 3f; x += 0.7f) p.Line(Rgba.Hex("40281C"), 0.03f, x, -5.3f, x, -4.4f);
            // stelline
            var rnd = new Random(39);
            for (int i = 0; i < 26; i++)
                p.Fill(Sd.Circle((float)rnd.NextDouble() * 6f - 3f, (float)rnd.NextDouble() * 6f - 1.5f, 0.02f + (float)rnd.NextDouble() * 0.025f), Rgba.White.WithAlpha(0.5f));
            return new DoodleArt("tune_stage_bg", p);
        }

        /// <summary>
        /// Chitarra vista da vicino, in verticale: corde (non disegnate) in x -0.75, 0, 0.75 dal ponte (y -3.3) al
        /// capotasto (y 0.6); paletta da 0.6 a 3.2 con i perni delle corde in (0.45, 2.75 / 1.9 / 1.05) e le chiavette a destra.
        /// </summary>
        public static DoodleArt E39TuneGuitar()
        {
            var p = new DoodlePainter(-2.35f, -4.95f, 2.35f, 3.35f, 70) { Ink = 0.055f };
            // cassa (solo la parte alta si vede)
            Sdf body = Sd.Union(Sd.Ellipse(0, -4.9f, 2.25f, 1.55f), Sd.Box(0, -4.6f, 1.6f, 0.3f));
            Body(p, body, Rgba.Hex("E8433F"), Rgba.Hex("B82B33"), null, -0.1f, 0.12f);
            p.Shape(Sd.Ellipse(-1.5f, -4.0f, 0.3f, 0.18f), Rgba.Hex("FF8A7A").WithAlpha(0.6f), 0.0f);
            // battipenna e ponte
            p.Shape(Sd.Box(0, -3.3f, 1.25f, 0.13f, 0.05f), Rgba.Hex("2E2A33"));
            foreach (float x in new[] { -0.75f, 0f, 0.75f }) p.Shape(Sd.Circle(x, -3.3f, 0.06f), Rgba.Hex("E3E9F0"), 0.02f);
            // manico
            Sdf neck = Sd.Box(0, -1.35f, 1.15f, 2.0f);
            Body(p, neck, Rgba.Hex("8A5A3B"), Rgba.Hex("6B4129"));
            for (int i = 0; i < 6; i++)
            {
                float y = 0.6f - 0.3f - i * 0.62f + i * i * 0.025f;
                p.Shape(Sd.Box(0, y, 1.12f, 0.025f), Rgba.Hex("D9DEE6"), 0.02f);
                if (i == 1 || i == 3 || i == 5) p.Shape(Sd.Circle(0.38f, y + 0.3f, 0.07f), Rgba.Hex("FFF6E0"), 0.02f);
            }
            // capotasto
            p.Shape(Sd.Box(0, 0.6f, 1.15f, 0.07f), Rgba.Hex("FFF6E0"), 0.035f);
            // paletta
            Sdf head = Sd.Polygon(-1.05f, 0.67f, 1.05f, 0.67f, 0.95f, 3.15f, 0.2f, 3.25f, -0.9f, 2.95f);
            Body(p, head, Rgba.Hex("2E2A33"), Rgba.Hex("1C1A21"));
            p.Fill(Sd.Capsule(-0.6f, 1.0f, -0.55f, 2.7f, 0.04f), Rgba.White.WithAlpha(0.2f));
            p.Fill(Sd.Box(-0.4f, 2.0f, 0.12f, 0.6f, 0.05f), Gold.WithAlpha(0.85f));
            foreach (float y in new[] { 2.75f, 1.9f, 1.05f })
            {
                // asta della chiavetta verso destra e perno
                p.Shape(Sd.Box(1.2f, y, 0.4f, 0.07f, 0.03f), Rgba.Hex("D9DEE6"), 0.03f);
                p.Shape(Sd.Circle(0.45f, y, 0.12f), Rgba.Hex("E3E9F0"), 0.03f);
                p.Fill(Sd.Circle(0.45f, y, 0.045f), Rgba.Hex("8D93A6"));
            }
            return new DoodleArt("tune_guitar", p);
        }

        /// <summary>Chiavetta d'accordatura a farfalla vista da sopra (centro 0,0); ruota quando la giri.</summary>
        public static DoodleArt E39TunePeg()
        {
            var p = new DoodlePainter(-0.42f, -0.42f, 0.42f, 0.42f, 160) { Ink = 0.04f };
            Sdf wings = Sd.Union(Sd.Ellipse(0, 0.2f, 0.2f, 0.2f), Sd.Ellipse(0, -0.2f, 0.2f, 0.2f), Sd.Box(0, 0, 0.12f, 0.2f));
            Body(p, wings, Rgba.Hex("FFF6E0"), Rgba.Hex("D9C9A3"));
            p.Shape(Sd.Circle(0, 0, 0.08f), Rgba.Hex("D9DEE6"), 0.03f);
            p.InkLine(0.025f, -0.08f, 0.28f, 0.08f, 0.28f);
            return new DoodleArt("tune_peg", p);
        }

        /// <summary>Diapason argentato su base tonda (punte in alto, base in y -0.6).</summary>
        public static DoodleArt E39TuneFork()
        {
            var p = new DoodlePainter(-0.5f, -0.72f, 0.5f, 0.72f, 140) { Ink = 0.04f };
            p.Shape(Sd.Ellipse(0, -0.55f, 0.42f, 0.13f), Rgba.Hex("8A5A3B"));
            p.Shape(Sd.Box(0, -0.42f, 0.06f, 0.12f), Rgba.Hex("B8C2CC"), 0.03f);
            Sdf fork = Sd.Union(Sd.Subtract(Sd.Box(0, 0.1f, 0.22f, 0.45f, 0.18f), Sd.Box(0, 0.25f, 0.11f, 0.45f, 0.1f)), Sd.Box(0, -0.3f, 0.05f, 0.1f));
            Body(p, fork, Rgba.Hex("E3E9F0"), Rgba.Hex("9AA7B7"));
            p.Line(Rgba.White, 0.025f, -0.17f, 0.0f, -0.17f, 0.45f);
            return new DoodleArt("tune_fork", p);
        }

        /// <summary>Accordatore a lancetta: semicerchio con perno in (0,0), raggio 0.75; zona verde al centro, ♭ a sinistra, ♯ a destra.</summary>
        public static DoodleArt E39TuneMeter()
        {
            var p = new DoodlePainter(-0.92f, -0.3f, 0.92f, 1.0f, 140) { Ink = 0.045f };
            Sdf box = Sd.Box(0, 0.32f, 0.85f, 0.56f, 0.12f);
            Body(p, box, Rgba.Hex("2E2A33"), Rgba.Hex("1C1A21"));
            Sdf face = Sd.Intersect(Sd.Circle(0, 0, 0.72f), Sd.Box(0, 0.4f, 0.8f, 0.4f));
            p.Fill(face, Rgba.Hex("FFF6E0"));
            // spicchi: rosso ai lati, verde al centro
            for (int i = -6; i <= 6; i++)
            {
                double a = Math.PI / 2 - i * (Math.PI / 2 * 0.85 / 6);
                float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
                Rgba col = i == 0 ? Rgba.Hex("2FB35A") : Math.Abs(i) >= 5 ? Rgba.Hex("E8433F") : E39Outline;
                p.Line(col, i == 0 ? 0.05f : 0.025f, c * 0.5f, s * 0.5f, c * 0.66f, s * 0.66f);
            }
            p.Fill(Sd.Intersect(Sd.Subtract(Sd.Circle(0, 0, 0.7f), Sd.Circle(0, 0, 0.6f)), Sd.Rotate(Sd.Box(0, 0.6f, 0.07f, 0.2f), 0, 0, 0)), Rgba.Hex("2FB35A").WithAlpha(0.6f));
            p.Shape(face, Rgba.Clear, 0.03f);
            // "-" (calante) a sinistra e "+" (crescente) a destra
            p.Line(Rgba.Hex("FFF6E0"), 0.05f, -0.8f, 0.82f, -0.62f, 0.82f);
            p.Line(Rgba.Hex("FFF6E0"), 0.05f, 0.62f, 0.82f, 0.8f, 0.82f);
            p.Line(Rgba.Hex("FFF6E0"), 0.05f, 0.71f, 0.73f, 0.71f, 0.91f);
            return new DoodleArt("tune_meter", p);
        }

        /// <summary>Gatto rockstar con occhiali da sole e cresta (busto, centro della testa in 0,0).</summary>
        public static DoodleArt E39TuneCat()
        {
            var p = new DoodlePainter(-0.85f, -0.95f, 0.85f, 0.95f, 120) { Ink = 0.045f };
            // giubbotto
            Sdf jacket = Sd.Ellipse(0, -0.85f, 0.7f, 0.45f);
            Body(p, jacket, Rgba.Hex("2E2A33"), Rgba.Hex("1C1A21"));
            p.Fill(Sd.Polygon(-0.12f, -0.42f, 0.12f, -0.42f, 0, -0.75f), Rgba.Hex("E8433F"));
            p.Shape(Sd.Circle(0.38f, -0.65f, 0.06f), Rgba.Hex("D9DEE6"), 0.02f);
            // cresta
            p.Shape(Sd.Polygon(-0.25f, 0.3f, -0.18f, 0.75f, -0.05f, 0.42f, 0.05f, 0.88f, 0.12f, 0.42f, 0.28f, 0.75f, 0.28f, 0.3f), Rgba.Hex("FF4DB8"));
            Sdf ears = Sd.Union(Sd.Polygon(-0.48f, 0.12f, -0.44f, 0.6f, -0.15f, 0.36f), Sd.Polygon(0.48f, 0.12f, 0.44f, 0.6f, 0.15f, 0.36f));
            p.Shape(ears, CatOrange);
            p.Fill(Sd.Grow(ears, -0.06f), CatPink);
            Sdf head = Sd.Ellipse(0, 0, 0.5f, 0.42f);
            Body(p, head, CatOrange, CatShade);
            p.Fill(Sd.Ellipse(0, -0.18f, 0.22f, 0.13f), Rgba.Hex("FFE3C2"));
            // occhiali
            foreach (float x in new[] { -0.2f, 0.2f })
            {
                p.Shape(Sd.Box(x, 0.05f, 0.15f, 0.1f, 0.06f), Rgba.Hex("2E2A33"), 0.03f);
                p.Fill(Sd.Capsule(x - 0.08f, 0.09f, x - 0.02f, 0.11f, 0.018f), Rgba.White.WithAlpha(0.7f));
            }
            p.InkLine(0.03f, -0.05f, 0.07f, 0.05f, 0.07f);
            p.Shape(Sd.Polygon(-0.05f, -0.1f, 0.05f, -0.1f, 0, -0.15f), CatPink, 0.02f);
            // ghigno
            p.InkLine(0.02f, -0.12f, -0.22f, 0, -0.27f, 0.12f, -0.22f);
            p.Fill(Sd.Polygon(0.04f, -0.25f, 0.08f, -0.24f, 0.06f, -0.3f), Rgba.White);
            foreach (float dy in new[] { 0f, 0.06f })
            {
                p.InkLine(0.012f, -0.22f, -0.15f + dy, -0.55f, -0.12f + dy * 1.6f);
                p.InkLine(0.012f, 0.22f, -0.15f + dy, 0.55f, -0.12f + dy * 1.6f);
            }
            return new DoodleArt("tune_cat", p);
        }

        // ---------- Claw machine ----------

        /// <summary>
        /// Cabinato della gru (5.4×9.4, centro 0,0): vetrina da x -2.3 a 2.3, y -1.6 a 2.9; scivolo a sinistra
        /// (buco nel pavimento da x -2.3 a -1.3); sportello premi in (-1.8, -2.45); pannello comandi sotto.
        /// </summary>
        public static DoodleArt E39ClawCabinet()
        {
            var p = new DoodlePainter(-2.8f, -4.85f, 2.8f, 4.0f, 70) { Ink = 0.06f };
            Rgba pink = Rgba.Hex("FF5FA2"), pinkShade = Rgba.Hex("D93A80");
            // corpo
            Sdf cab = Sd.Box(0, -0.45f, 2.65f, 4.3f, 0.2f);
            Body(p, cab, pink, pinkShade);
            // insegna in alto
            Body(p, Sd.Box(0, 3.4f, 2.5f, 0.45f, 0.18f), Rgba.Hex("FFD23C"), Rgba.Hex("E3A51B"));
            for (int i = 0; i < 9; i++) p.Shape(Sd.Circle(-2.2f + i * 0.55f, 3.82f, 0.06f), Rgba.Hex("FFF6E0"), 0.02f);
            // vetrina: sfondo, binario e pavimento
            Sdf glass = Sd.Box(0, 0.65f, 2.3f, 2.25f, 0.05f);
            p.FillFunc(glass, (x, y) => Rgba.Lerp(Rgba.Hex("BFE6FF"), Rgba.Hex("7EB8E6"), Clamp01E((2.9f - y) / 4.5f)));
            p.Fill(Sd.Box(0, 2.72f, 2.3f, 0.1f), Rgba.Hex("8D93A6"));
            p.Line(Rgba.Hex("5B6070"), 0.03f, -2.3f, 2.66f, 2.3f, 2.66f);
            p.Fill(Sd.Box(0.5f, -1.45f, 1.8f, 0.15f), Rgba.Hex("6B4CC9"));
            // scivolo (buco scuro)
            p.Fill(Sd.Box(-1.8f, -1.45f, 0.5f, 0.15f), Rgba.Hex("1C1A21"));
            p.Shape(glass, Rgba.Clear, 0.07f);
            // riflessi del vetro
            p.Fill(Sd.Intersect(glass, Sd.Rotate(Sd.Box(-1.2f, 1.5f, 0.12f, 3f), -1.2f, 1.5f, -25f)), Rgba.White.WithAlpha(0.35f));
            p.Fill(Sd.Intersect(glass, Sd.Rotate(Sd.Box(-0.7f, 1.5f, 0.05f, 3f), -0.7f, 1.5f, -25f)), Rgba.White.WithAlpha(0.3f));
            // pannello comandi
            Body(p, Sd.Box(0, -3.25f, 2.45f, 1.35f, 0.15f), Rgba.Hex("6B4CC9"), Rgba.Hex("4E35A0"));
            // sportello premi
            Sdf door = Sd.Box(-1.8f, -2.45f, 0.5f, 0.42f, 0.08f);
            p.Shape(door, Rgba.Hex("1C1A21"));
            p.Shape(Sd.Box(-1.8f, -2.3f, 0.44f, 0.22f, 0.05f), Rgba.Hex("3A3540"), 0.03f);
            p.Shape(Sd.Box(-1.8f, -2.75f, 0.3f, 0.05f, 0.03f), Rgba.Hex("B8C2CC"), 0.025f);
            // fessura delle monete
            p.Shape(Sd.Box(1.85f, -2.3f, 0.35f, 0.35f, 0.06f), Rgba.Hex("B8C2CC"), 0.035f);
            p.Fill(Sd.Box(1.85f, -2.3f, 0.04f, 0.2f, 0.02f), E39Outline);
            // base
            p.Shape(Sd.Box(0, -4.65f, 2.75f, 0.18f, 0.06f), Rgba.Hex("2E2A33"));
            return new DoodleArt("claw_cabinet", p);
        }

        /// <summary>Artiglio della gru (aperto o chiuso): presa in (0,0), il cavo esce in alto in (0, 0.55).</summary>
        public static DoodleArt E39Claw(bool closed)
        {
            var p = new DoodlePainter(-0.65f, -0.4f, 0.65f, 0.65f, 140) { Ink = 0.04f };
            Rgba metal = Rgba.Hex("D9DEE6"), metalShade = Rgba.Hex("9AA7B7");
            foreach (int s in new[] { -1, 1 })
            {
                float[] finger = closed
                    ? new[] { s * 0.15f, 0.28f, s * 0.35f, 0.05f, s * 0.15f, -0.22f }
                    : new[] { s * 0.18f, 0.28f, s * 0.5f, 0.1f, s * 0.48f, -0.25f };
                p.Shape(Sd.Polyline(0.055f, finger), metal);
                p.Line(metalShade, 0.03f, finger[2], finger[3], finger[4], finger[5]);
            }
            // dito centrale (davanti)
            p.Shape(Sd.Polyline(0.05f, 0f, 0.25f, 0f, closed ? -0.22f : -0.18f), metal);
            Sdf hub = Sd.Ellipse(0, 0.33f, 0.3f, 0.15f);
            Body(p, hub, Rgba.Hex("FFD23C"), Rgba.Hex("E3A51B"));
            p.Shape(Sd.Box(0, 0.5f, 0.07f, 0.08f, 0.02f), metalShade, 0.03f);
            return new DoodleArt(closed ? "claw_closed" : "claw_open", p);
        }

        /// <summary>Parete di plexiglas dello scivolo (sta davanti ai peluche): bordo destro in x 0, da y 0 a 0.75.</summary>
        public static DoodleArt E39ClawChute()
        {
            var p = new DoodlePainter(-1.1f, -0.1f, 0.12f, 0.95f, 100) { Ink = 0.04f };
            p.Shape(Sd.Box(-0.5f, 0.38f, 0.5f, 0.38f, 0.04f), Rgba.Hex("FFFFFF").WithAlpha(0.3f));
            p.Fill(Sd.Capsule(-0.8f, 0.1f, -0.6f, 0.65f, 0.03f), Rgba.White.WithAlpha(0.6f));
            p.Fill(Sd.Box(-0.5f, 0.52f, 0.36f, 0.09f, 0.03f), Rgba.Hex("FFD23C"));
            p.InkLine(0.022f, -0.78f, 0.52f, -0.22f, 0.52f);
            p.InkLine(0.022f, -0.38f, 0.6f, -0.22f, 0.52f, -0.38f, 0.44f);
            return new DoodleArt("claw_chute", p);
        }

        /// <summary>Pulsante con freccia verso destra (si specchia per la sinistra), centro 0,0, 0.9×0.8.</summary>
        public static DoodleArt E39ClawArrow()
        {
            var p = new DoodlePainter(-0.5f, -0.45f, 0.5f, 0.45f, 140) { Ink = 0.045f };
            Sdf pad = Sd.Box(0, 0, 0.42f, 0.38f, 0.12f);
            Body(p, pad, Rgba.Hex("3CE6FF"), Rgba.Hex("1FA8C9"));
            p.Shape(Sd.Union(Sd.Polygon(0.25f, 0, -0.02f, 0.22f, -0.02f, -0.22f), Sd.Box(-0.12f, 0, 0.13f, 0.08f)), Rgba.White, 0.03f);
            return new DoodleArt("claw_arrow", p);
        }

        /// <summary>Peluche (gatto, pesce, topo o orsetto con la chiave d'oro): centro 0,0, circa 0.8×0.7.</summary>
        public static DoodleArt E39Plush(string kind)
        {
            var p = new DoodlePainter(-0.5f, -0.45f, 0.5f, 0.5f, 140) { Ink = 0.04f, Wobble = 0.015f };
            void Eyes(float y, float dx)
            {
                foreach (float x in new[] { -dx, dx }) { p.Fill(Sd.Circle(x, y, 0.045f), E39Outline); p.Fill(Sd.Circle(x - 0.012f, y + 0.015f, 0.014f), Rgba.White); }
            }
            void Stitch(float x0, float y0, float x1, float y1)
            {
                for (int i = 0; i < 4; i++)
                {
                    float t0 = i / 4f, t1 = t0 + 0.12f;
                    p.Line(Rgba.White.WithAlpha(0.7f), 0.012f, x0 + (x1 - x0) * t0, y0 + (y1 - y0) * t0, x0 + (x1 - x0) * t1, y0 + (y1 - y0) * t1);
                }
            }
            switch (kind)
            {
                case "cat":
                {
                    Rgba c = Rgba.Hex("B9A3FF"), sh = Rgba.Hex("8E6CEF");
                    Sdf ears = Sd.Union(Sd.Polygon(-0.36f, 0.1f, -0.3f, 0.45f, -0.08f, 0.28f), Sd.Polygon(0.36f, 0.1f, 0.3f, 0.45f, 0.08f, 0.28f));
                    p.Shape(ears, c);
                    p.Fill(Sd.Grow(ears, -0.05f), CatPink);
                    Sdf body = Sd.Union(Sd.Ellipse(0, 0.02f, 0.4f, 0.3f), Sd.Ellipse(0, -0.25f, 0.3f, 0.17f));
                    Body(p, body, c, sh);
                    Eyes(0.06f, 0.14f);
                    p.Shape(Sd.Polygon(-0.04f, -0.03f, 0.04f, -0.03f, 0, -0.07f), CatPink, 0.015f);
                    p.InkLine(0.015f, 0, -0.07f, -0.05f, -0.11f);
                    p.InkLine(0.015f, 0, -0.07f, 0.05f, -0.11f);
                    foreach (float x in new[] { -0.15f, 0.15f }) p.Shape(Sd.Ellipse(x, -0.36f, 0.1f, 0.06f), Rgba.Hex("FFF6E0"), 0.025f);
                    Stitch(-0.25f, 0.2f, 0.25f, 0.2f);
                    return new DoodleArt("claw_plush_cat", p);
                }
                case "fish":
                {
                    Rgba c = Rgba.Hex("5FD1C5"), sh = Rgba.Hex("2FA59A");
                    Sdf body = Sd.Union(Sd.Ellipse(0.06f, 0, 0.36f, 0.26f), Sd.Polygon(-0.25f, 0, -0.47f, 0.24f, -0.42f, 0, -0.47f, -0.24f));
                    Body(p, body, c, sh);
                    p.Fill(Sd.Intersect(Sd.Ellipse(0.06f, 0, 0.36f, 0.26f), Sd.Box(0.06f, -0.15f, 0.4f, 0.1f)), Rgba.Hex("FFF6E0").WithAlpha(0.6f));
                    p.Shape(Sd.Polygon(-0.05f, 0.22f, 0.12f, 0.36f, 0.18f, 0.2f), Rgba.Hex("FFD23C"), 0.025f);
                    p.Fill(Sd.Circle(0.22f, 0.05f, 0.06f), Rgba.White);
                    p.Fill(Sd.Circle(0.23f, 0.05f, 0.035f), E39Outline);
                    p.InkLine(0.018f, 0.32f, -0.06f, 0.36f, -0.03f);
                    Stitch(-0.15f, -0.2f, 0.2f, -0.2f);
                    return new DoodleArt("claw_plush_fish", p);
                }
                case "mouse":
                {
                    Rgba c = Rgba.Hex("C9CED8"), sh = Rgba.Hex("9AA0AE");
                    p.Line(CatPink, 0.04f, 0.25f, -0.25f, 0.42f, -0.2f, 0.45f, 0.0f);
                    foreach (float x in new[] { -0.26f, 0.26f })
                    {
                        p.Shape(Sd.Circle(x, 0.24f, 0.15f), c);
                        p.Fill(Sd.Circle(x, 0.24f, 0.09f), CatPink);
                    }
                    Sdf body = Sd.Ellipse(0, -0.05f, 0.34f, 0.32f);
                    Body(p, body, c, sh);
                    Eyes(0.04f, 0.12f);
                    p.Shape(Sd.Circle(0, -0.06f, 0.045f), CatPink, 0.015f);
                    p.Fill(Sd.Ellipse(0, -0.25f, 0.16f, 0.08f), Rgba.Hex("E9EDF3"));
                    return new DoodleArt("claw_plush_mouse", p);
                }
                default:
                {
                    // orsetto con la chiave d'oro in braccio
                    Rgba c = Rgba.Hex("D2A06A"), sh = Rgba.Hex("A8743F");
                    foreach (float x in new[] { -0.26f, 0.26f })
                    {
                        p.Shape(Sd.Circle(x, 0.28f, 0.12f), c);
                        p.Fill(Sd.Circle(x, 0.28f, 0.06f), Rgba.Hex("F2D9B8"));
                    }
                    Sdf body = Sd.Union(Sd.Ellipse(0, 0.06f, 0.36f, 0.28f), Sd.Ellipse(0, -0.25f, 0.3f, 0.18f));
                    Body(p, body, c, sh);
                    p.Fill(Sd.Ellipse(0, -0.02f, 0.13f, 0.09f), Rgba.Hex("F2D9B8"));
                    Eyes(0.1f, 0.13f);
                    p.Fill(Sd.Ellipse(0, 0.01f, 0.04f, 0.03f), E39Outline);
                    // la chiave d'oro tenuta in diagonale
                    Sdf bow = Sd.Subtract(Sd.Circle(-0.2f, -0.24f, 0.11f), Sd.Circle(-0.2f, -0.24f, 0.045f));
                    Sdf key = Sd.Union(bow, Sd.Rotate(Sd.Box(0.08f, -0.24f, 0.2f, 0.035f), -0.2f, -0.24f, 12f),
                        Sd.Rotate(Sd.Box(0.22f, -0.28f, 0.03f, 0.05f), -0.2f, -0.24f, 12f));
                    p.Shape(key, Gold, 0.03f);
                    p.Fill(Sd.Circle(-0.24f, -0.2f, 0.02f), GoldLight);
                    foreach (float x in new[] { -0.08f, 0.2f }) p.Shape(Sd.Ellipse(x, -0.27f, 0.08f, 0.06f), c, 0.025f);
                    return new DoodleArt("claw_plush_key", p);
                }
            }
        }

        static float Clamp01E(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
