using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli della specifica 105-200, pacchetto F.</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV41F() => new DoodleArt[]
        {
            // The wrong shadow
            S41F_ShadowRoomBg(), S41F_MonsterShadow(), S41F_FloorLamp(),
            // The impossible scale
            S41F_ScaleStand(), S41F_ScaleBeam(), S41F_ScalePan(), S41F_Elephant(), S41F_BigFeather(),
            // Rewind reality
            S41F_RewindBg(), S41F_WalkCat(false), S41F_WalkCat(true), S41F_Shards(), S41F_CamOverlay(), S41F_TapeHandle(), S41F_PillButton(),
            // The sticky finger
            S41F_BankBg(), S41F_GumWad(), S41F_StickyCoin(), S41F_CoinSafe(false), S41F_CoinSafe(true),
            // The guilty button / Lie detector
            S41F_InterroBg(), S41F_Suspect(false), S41F_Suspect(true), S41F_SweatDrop(false), S41F_SweatDrop(true), S41F_Detective(),
            S41F_Robot(), S41F_RobotFace(false), S41F_RobotFace(true), S41F_Polygraph(), S41F_PolyNeedle(),
            // The invisible stairs
            S41F_StairsBg(), S41F_CloudDoor(false), S41F_CloudDoor(true), S41F_Climber(), S41F_GlassStep(),
            // Too bright
            S41F_BrightBg(), S41F_Sunbeam(), S41F_CurtainPanel(), S41F_Vampire(false), S41F_Vampire(true), S41F_Coffin(),
        };

        static float S41F_Clamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
        static float S41F_Sin(float v) => (float)Math.Sin(v);

        static void S41F_Eyes(DoodlePainter p, float cx, float cy, float gap, float r, float lookX = 0f, float lookY = 0f)
        {
            foreach (float s in new[] { -1f, 1f })
            {
                float x = cx + s * gap;
                p.Shape(Sd.Circle(x, cy, r), Rgba.White, r * 0.35f);
                p.Fill(Sd.Circle(x + lookX * r * 0.35f, cy + lookY * r * 0.35f, r * 0.5f), Ink);
                p.Fill(Sd.Circle(x + lookX * r * 0.35f - r * 0.18f, cy + lookY * r * 0.35f + r * 0.2f, r * 0.17f), Rgba.White);
            }
        }

        // ================= THE WRONG SHADOW =================

        /// <summary>Stanza di sera: parete lilla fino a y -0.6, pavimento di legno, formaggio a sinistra in basso.</summary>
        public static DoodleArt S41F_ShadowRoomBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 2.3f, 3.3f, 2.95f), (x, y) => Rgba.Lerp(Rgba.Hex("6B5A8E"), Rgba.Hex("47396A"), S41F_Clamp((y + 0.6f) / 5.5f)));
            for (float x = -3f; x < 3.2f; x += 0.6f) p.Line(Rgba.Hex("5C4C80", 0.6f), 0.05f, x, -0.6f, x, 5.2f);
            // finestra con la luna
            Sdf win = Sd.Box(-1.6f, 2.0f, 0.7f, 0.8f, 0.05f);
            p.Shape(win, Rgba.Hex("1E2448"), 0.06f);
            p.Fill(Sd.Subtract(Sd.Circle(-1.4f, 2.3f, 0.25f), Sd.Circle(-1.3f, 2.38f, 0.22f)), Rgba.Hex("FFF1A8"));
            p.InkLine(0.05f, -1.6f, 1.2f, -1.6f, 2.8f);
            p.InkLine(0.05f, -2.3f, 2.0f, -0.9f, 2.0f);
            // quadro
            p.Shape(Sd.Box(1.5f, 2.2f, 0.5f, 0.4f, 0.03f), Rgba.Hex("E9C27A"), 0.05f);
            p.Fill(Sd.Box(1.5f, 2.2f, 0.38f, 0.28f), Rgba.Hex("8FBF7A"));
            p.Fill(Sd.Circle(1.62f, 2.32f, 0.08f), Rgba.Hex("FFE07A"));
            // battiscopa e pavimento
            p.FillFunc(Sd.Box(0, -2.9f, 3.3f, 2.35f), (x, y) => Rgba.Lerp(Rgba.Hex("7A4A2E"), Rgba.Hex("A8693F"), S41F_Clamp((y + 5.2f) / 4.6f)));
            for (float y = -1.1f; y > -5.3f; y -= 0.55f) p.Line(Rgba.Hex("6A3E25", 0.7f), 0.03f, -3.3f, y, 3.3f, y);
            p.Shape(Sd.Box(0, -0.6f, 3.3f, 0.08f), Rgba.Hex("3E2C55"), 0.04f);
            // tappeto sotto il gatto
            p.Shape(Sd.Ellipse(0f, -1.55f, 1.4f, 0.35f), Rgba.Hex("B65A7A"), 0.04f);
            p.Fill(Sd.Ellipse(0f, -1.55f, 1.1f, 0.22f), Rgba.Hex("D47C98"));
            // corsia del topo (striscia più chiara)
            p.Fill(Sd.Box(0, -3.95f, 3.3f, 0.35f), Rgba.Hex("C98A55", 0.35f));
            // piattino col formaggio a sinistra
            p.Shape(Sd.Ellipse(-2.15f, -4.05f, 0.5f, 0.12f), Rgba.White, 0.035f);
            p.Shape(Sd.Polygon(-2.5f, -4.0f, -1.8f, -4.0f, -1.8f, -3.7f), Rgba.Hex("FFD54A"), 0.035f);
            p.Fill(Sd.Circle(-2.05f, -3.88f, 0.05f), Rgba.Hex("E0A82A"));
            p.Fill(Sd.Circle(-2.3f, -3.94f, 0.04f), Rgba.Hex("E0A82A"));
            return new DoodleArt("s41f_shadow_bg", p);
        }

        /// <summary>Ombra del "mostro" (base in 0,0, punta verso l'alto, alta 3): orecchie a corna, artigli, denti.</summary>
        public static DoodleArt S41F_MonsterShadow()
        {
            var p = new DoodlePainter(-1.35f, -0.1f, 1.35f, 3.2f, 60) { Ink = 0f, Wobble = 0.03f };
            Rgba c = Rgba.Hex("1A1030");
            Sdf body = Sd.Union(
                Sd.Polygon(-0.5f, 0f, 0.5f, 0f, 0.8f, 1.6f, 0.7f, 2.2f, -0.7f, 2.2f, -0.8f, 1.6f),
                Sd.Circle(0f, 2.2f, 0.62f),
                Sd.Polygon(-0.45f, 2.55f, -0.95f, 3.15f, -0.2f, 2.75f),
                Sd.Polygon(0.45f, 2.55f, 0.95f, 3.15f, 0.2f, 2.75f),
                Sd.Capsule(-0.7f, 1.6f, -1.15f, 2.25f, 0.13f),
                Sd.Capsule(0.7f, 1.6f, 1.15f, 2.25f, 0.13f),
                Sd.Polygon(-1.25f, 2.25f, -1.3f, 2.6f, -1.1f, 2.35f), Sd.Polygon(-1.12f, 2.3f, -1.0f, 2.62f, -0.98f, 2.32f),
                Sd.Polygon(1.25f, 2.25f, 1.3f, 2.6f, 1.1f, 2.35f), Sd.Polygon(1.12f, 2.3f, 1.0f, 2.62f, 0.98f, 2.32f));
            p.Fill(body, c);
            // occhi e bocca "bucati" (luce che passa)
            p.Fill(Sd.Polygon(-0.38f, 2.35f, -0.08f, 2.2f, -0.35f, 2.15f), Rgba.Hex("FFE36A", 0.9f));
            p.Fill(Sd.Polygon(0.38f, 2.35f, 0.08f, 2.2f, 0.35f, 2.15f), Rgba.Hex("FFE36A", 0.9f));
            p.Fill(Sd.Polygon(-0.32f, 1.95f, 0.32f, 1.95f, 0.22f, 1.82f, 0.12f, 1.92f, 0f, 1.8f, -0.12f, 1.92f, -0.22f, 1.82f), Rgba.Hex("FFE36A", 0.6f));
            return new DoodleArt("s41f_monster_shadow", p);
        }

        /// <summary>Lampada da terra (base in 0,0; lampadina in 0,1.35).</summary>
        public static DoodleArt S41F_FloorLamp()
        {
            var p = new DoodlePainter(-0.55f, -0.1f, 0.55f, 1.85f, 140) { Ink = 0.04f };
            p.Shape(Sd.Ellipse(0, 0.04f, 0.32f, 0.08f), Rgba.Hex("4A3A5C"));
            p.Shape(Sd.Box(0, 0.65f, 0.035f, 0.6f), Rgba.Hex("6E5A84"), 0.03f);
            p.Fill(Sd.Circle(0, 1.3f, 0.2f), Rgba.Hex("FFF6C8"));
            Sdf shade = Sd.Polygon(-0.45f, 1.25f, 0.45f, 1.25f, 0.25f, 1.75f, -0.25f, 1.75f);
            p.FillFunc(shade, (x, y) => Rgba.Lerp(Rgba.Hex("FFB84A"), Rgba.Hex("FFE08A"), S41F_Clamp((y - 1.25f) / 0.5f)));
            for (float x = -0.3f; x < 0.35f; x += 0.15f) p.Line(Rgba.Hex("E39B1B", 0.6f), 0.025f, x * 1.3f, 1.27f, x * 0.8f, 1.73f);
            p.Shape(shade, Rgba.Clear);
            p.Shape(Sd.Ellipse(0, 1.22f, 0.12f, 0.07f), Rgba.Hex("FFF6C8"), 0.025f);
            return new DoodleArt("s41f_floor_lamp", p);
        }

        // ================= THE IMPOSSIBLE SCALE =================

        /// <summary>Colonna della bilancia: perno in 0,0, base in y -3.4.</summary>
        public static DoodleArt S41F_ScaleStand()
        {
            var p = new DoodlePainter(-1.0f, -3.55f, 1.0f, 0.35f, 90) { Ink = 0.05f };
            p.Shape(Sd.Box(0, -3.3f, 0.9f, 0.15f, 0.06f), GoldShade);
            p.Shape(Sd.Polygon(-0.55f, -3.15f, 0.55f, -3.15f, 0.3f, -2.85f, -0.3f, -2.85f), Gold);
            Sdf post = Sd.Box(0, -1.45f, 0.11f, 1.45f);
            p.FillFunc(post, (x, y) => Rgba.Lerp(GoldShade, Gold, S41F_Clamp((x + 0.11f) / 0.15f)));
            p.Shape(post, Rgba.Clear);
            p.Shape(Sd.Circle(0, 0, 0.22f), Gold);
            p.Fill(Sd.Circle(-0.06f, 0.06f, 0.07f), GoldLight);
            return new DoodleArt("s41f_scale_stand", p);
        }

        /// <summary>Asta della bilancia (centro 0,0, ganci a x ±1.75).</summary>
        public static DoodleArt S41F_ScaleBeam()
        {
            var p = new DoodlePainter(-1.95f, -0.2f, 1.95f, 0.25f, 110) { Ink = 0.04f };
            Sdf beam = Sd.Union(Sd.Box(0, 0, 1.75f, 0.07f, 0.04f), Sd.Polygon(-0.4f, 0.07f, 0.4f, 0.07f, 0f, 0.18f));
            p.FillFunc(beam, (x, y) => Rgba.Lerp(GoldShade, GoldLight, S41F_Clamp((y + 0.07f) / 0.2f)));
            p.Shape(beam, Rgba.Clear);
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Circle(s * 1.75f, 0f, 0.1f), Gold, 0.035f);
            p.Shape(Sd.Circle(0, 0, 0.1f), GoldShade, 0.035f);
            return new DoodleArt("s41f_scale_beam", p);
        }

        /// <summary>Piatto appeso (gancio in 0,0, superficie del piatto in y -1.25).</summary>
        public static DoodleArt S41F_ScalePan()
        {
            var p = new DoodlePainter(-0.9f, -1.6f, 0.9f, 0.12f, 110) { Ink = 0.04f };
            p.Line(GoldShade, 0.035f, 0f, 0f, -0.7f, -1.25f);
            p.Line(GoldShade, 0.035f, 0f, 0f, 0.7f, -1.25f);
            Sdf dish = Sd.Intersect(Sd.Ellipse(0, -1.25f, 0.8f, 0.28f), Sd.Box(0, -1.45f, 0.9f, 0.2f));
            p.FillFunc(dish, (x, y) => Rgba.Lerp(GoldShade, Gold, S41F_Clamp((y + 1.53f) / 0.28f)));
            p.Shape(dish, Rgba.Clear);
            p.Shape(Sd.Ellipse(0, -1.25f, 0.8f, 0.05f), GoldLight, 0.03f);
            p.Shape(Sd.Circle(0, 0, 0.07f), Gold, 0.03f);
            return new DoodleArt("s41f_scale_pan", p);
        }

        /// <summary>Elefantino seduto (piedi in y 0, largo 1.6).</summary>
        public static DoodleArt S41F_Elephant()
        {
            var p = new DoodlePainter(-0.9f, -0.08f, 0.9f, 1.6f, 120) { Ink = 0.045f };
            Rgba g = Rgba.Hex("A9B3C9"), gs = Rgba.Hex("8590AA");
            p.Shape(Sd.Capsule(0.62f, 0.5f, 0.8f, 0.75f, 0.03f), gs, 0.025f);    // coda
            foreach (float x in new[] { -0.45f, -0.12f, 0.22f, 0.5f }) p.Shape(Sd.Box(x, 0.2f, 0.12f, 0.2f, 0.05f), g);
            Body(p, Sd.Ellipse(0.1f, 0.55f, 0.65f, 0.42f), g, gs);
            foreach (float x in new[] { -0.45f, -0.12f, 0.22f, 0.5f }) p.Fill(Sd.Ellipse(x, 0.04f, 0.09f, 0.03f), Rgba.White.WithAlpha(0.8f));
            // orecchio, testa, proboscide
            p.Shape(Sd.Ellipse(-0.25f, 1.0f, 0.36f, 0.4f), g);
            p.Fill(Sd.Ellipse(-0.25f, 1.0f, 0.24f, 0.28f), Rgba.Hex("F2B6C6"));
            Body(p, Sd.Circle(-0.55f, 1.0f, 0.36f), g, gs, null, 0.05f, 0.06f);
            p.Shape(Sd.Polyline(0.1f, -0.75f, 0.8f, -0.85f, 0.5f, -0.8f, 0.3f, -0.68f, 0.3f), g);
            p.Shape(Sd.Circle(-0.6f, 1.1f, 0.075f), Rgba.White, 0.025f);
            p.Fill(Sd.Circle(-0.62f, 1.11f, 0.04f), Ink);
            p.Fill(Sd.Circle(-0.42f, 0.9f, 0.06f), BlushColor.WithAlpha(0.6f));
            p.Shape(Sd.Polygon(-0.5f, 0.78f, -0.4f, 0.8f, -0.42f, 0.62f), Rgba.White, 0.02f);   // zanna
            // "5 TONS" sulla pancia? solo un fiocco
            p.Shape(Sd.Polygon(-0.1f, 1.36f, -0.3f, 1.48f, -0.3f, 1.26f), Rgba.Hex("FF6F91"), 0.02f);
            p.Shape(Sd.Polygon(-0.1f, 1.36f, 0.1f, 1.48f, 0.1f, 1.26f), Rgba.Hex("FF6F91"), 0.02f);
            return new DoodleArt("s41f_elephant", p);
        }

        /// <summary>Piuma dritta (base in 0,0, alta 1).</summary>
        public static DoodleArt S41F_BigFeather()
        {
            var p = new DoodlePainter(-0.32f, -0.05f, 0.32f, 1.05f, 220) { Ink = 0.02f };
            Sdf vane = Sd.Rotate(Sd.Ellipse(0.02f, 0.58f, 0.2f, 0.44f), 0.02f, 0.58f, -8f);
            p.FillFunc(vane, (x, y) => Rgba.Lerp(Rgba.Hex("FF8FB8"), Rgba.Hex("FFE1EC"), S41F_Clamp((y - 0.15f) / 0.85f)));
            for (float y = 0.25f; y < 0.98f; y += 0.07f)
            {
                p.Line(Rgba.Hex("E66A98", 0.55f), 0.01f, 0.0f + (y - 0.1f) * 0.06f, y, -0.18f, y + 0.08f);
                p.Line(Rgba.Hex("E66A98", 0.55f), 0.01f, 0.0f + (y - 0.1f) * 0.06f, y, 0.2f, y + 0.07f);
            }
            p.Shape(vane, Rgba.Clear);
            p.InkLine(0.025f, 0f, 0f, 0.03f, 0.5f, 0.07f, 0.98f);
            return new DoodleArt("s41f_big_feather", p);
        }

        // ================= REWIND REALITY =================

        /// <summary>Soggiorno: tavolo (piano in y 0, da x -0.9 a 2.3), mensola in alto a sinistra (y 1.3), cuscino a terra.</summary>
        public static DoodleArt S41F_RewindBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 1.9f, 3.3f, 3.4f), (x, y) => Rgba.Lerp(Rgba.Hex("F1E1C0"), Rgba.Hex("E2CDA4"), S41F_Clamp((y + 1.5f) / 6f)));
            for (float x = -3f; x < 3.2f; x += 0.45f) p.Line(Rgba.Hex("D8C096", 0.5f), 0.04f, x, -1.5f, x, 5.3f);
            p.FillFunc(Sd.Box(0, -3.4f, 3.3f, 1.9f), (x, y) => Rgba.Lerp(Rgba.Hex("9A6238"), Rgba.Hex("C78A52"), S41F_Clamp((y + 5.2f) / 3.7f)));
            for (float y = -1.9f; y > -5.3f; y -= 0.5f) p.Line(Rgba.Hex("7E4D2A", 0.6f), 0.03f, -3.3f, y, 3.3f, y);
            p.Shape(Sd.Box(0, -1.5f, 3.3f, 0.07f), Rgba.Hex("B98552"), 0.04f);
            // mensola
            p.Shape(Sd.Box(-1.75f, 1.2f, 0.85f, 0.09f, 0.03f), Wood);
            p.Shape(Sd.Polygon(-2.3f, 1.11f, -2.2f, 1.11f, -2.25f, 0.8f), WoodShade, 0.03f);
            p.Shape(Sd.Polygon(-1.3f, 1.11f, -1.2f, 1.11f, -1.25f, 0.8f), WoodShade, 0.03f);
            p.Shape(Sd.Box(-2.2f, 1.55f, 0.18f, 0.26f, 0.03f), Rgba.Hex("7FB2E5"), 0.035f);   // libro
            p.Shape(Sd.Box(-1.95f, 1.5f, 0.1f, 0.21f, 0.02f), Rgba.Hex("E57F7F"), 0.035f);
            // tavolo
            p.Shape(Sd.Box(0.7f, -0.08f, 1.65f, 0.1f, 0.03f), Wood);
            p.Shape(Sd.Box(-0.7f, -0.9f, 0.08f, 0.75f), WoodShade, 0.04f);
            p.Shape(Sd.Box(2.1f, -0.9f, 0.08f, 0.75f), WoodShade, 0.04f);
            // cuscino a terra (posto morbido)
            p.Shape(Sd.Box(-1.75f, -2.35f, 0.75f, 0.22f, 0.2f), Rgba.Hex("8FD0A8"));
            p.Fill(Sd.Box(-1.75f, -2.28f, 0.6f, 0.08f, 0.08f), Rgba.Hex("B8E6C8"));
            for (float x = -2.25f; x < -1.2f; x += 0.25f) p.Fill(Sd.Circle(x, -2.35f, 0.03f), Rgba.Hex("5FA07A"));
            // pianta
            p.Shape(Sd.Polygon(2.45f, -2.7f, 2.85f, -2.7f, 2.78f, -2.2f, 2.52f, -2.2f), Rgba.Hex("D9784A"));
            foreach (float a in new[] { -0.4f, 0f, 0.4f }) p.Shape(Sd.Rotate(Sd.Ellipse(2.65f, -1.85f, 0.1f, 0.35f), 2.65f, -2.15f, a * 50f), Leaf, 0.03f);
            return new DoodleArt("s41f_rewind_bg", p);
        }

        /// <summary>Gatto che cammina verso destra (piedi in y 0); swat = zampa alzata.</summary>
        public static DoodleArt S41F_WalkCat(bool swat)
        {
            var p = new DoodlePainter(-0.85f, -0.06f, 1.15f, 1.15f, 130) { Ink = 0.045f };
            p.Shape(Sd.Polyline(0.07f, -0.5f, 0.45f, -0.75f, 0.75f, -0.72f, 1.0f), CatOrange);
            foreach (float x in new[] { -0.35f, -0.15f }) p.Shape(Sd.Capsule(x, 0.08f, x, 0.35f, 0.08f), CatOrange);
            p.Shape(Sd.Capsule(0.18f, 0.08f, 0.18f, 0.35f, 0.08f), CatOrange);
            if (!swat) p.Shape(Sd.Capsule(0.36f, 0.08f, 0.36f, 0.35f, 0.08f), CatOrange);
            Body(p, Sd.Ellipse(-0.05f, 0.45f, 0.52f, 0.27f), CatOrange, CatShade);
            for (float x = -0.4f; x < 0.2f; x += 0.17f) p.Line(CatShade, 0.04f, x, 0.7f, x + 0.04f, 0.55f);
            Sdf head = Sd.Circle(0.45f, 0.72f, 0.26f);
            p.Shape(Sd.Polygon(0.25f, 0.85f, 0.3f, 1.08f, 0.42f, 0.92f), CatOrange);
            p.Shape(Sd.Polygon(0.52f, 0.95f, 0.66f, 1.08f, 0.67f, 0.86f), CatOrange);
            Body(p, head, CatOrange, CatShade, null, -0.04f, 0.05f);
            p.Shape(Sd.Circle(0.5f, 0.76f, 0.065f), Rgba.White, 0.02f);
            p.Fill(Sd.Circle(0.53f, 0.76f, 0.035f), Ink);
            p.Fill(Sd.Circle(0.66f, 0.66f, 0.03f), CatPink);
            p.InkLine(0.018f, 0.55f, 0.6f, 0.62f, 0.62f, 0.68f, 0.58f);
            p.InkLine(0.012f, 0.62f, 0.64f, 0.82f, 0.68f);
            p.InkLine(0.012f, 0.62f, 0.62f, 0.82f, 0.58f);
            if (swat)
            {
                p.Shape(Sd.Capsule(0.25f, 0.4f, 0.82f, 0.3f, 0.08f), CatOrange);
                p.Shape(Sd.Circle(0.86f, 0.3f, 0.1f), CatOrange);
                p.Line(Rgba.Hex("2B2233", 0.6f), 0.02f, 1.0f, 0.42f, 1.08f, 0.48f);
                p.Line(Rgba.Hex("2B2233", 0.6f), 0.02f, 1.02f, 0.3f, 1.12f, 0.3f);
            }
            return new DoodleArt(swat ? "s41f_walk_cat_swat" : "s41f_walk_cat", p);
        }

        /// <summary>Cocci del vaso a terra (centro 0,0).</summary>
        public static DoodleArt S41F_Shards()
        {
            var p = new DoodlePainter(-0.7f, -0.15f, 0.7f, 0.35f, 140) { Ink = 0.03f };
            Rgba v = Rgba.Hex("4FC1D8"), vs = Rgba.Hex("2E93AE");
            p.Shape(Sd.Polygon(-0.6f, -0.05f, -0.3f, 0.05f, -0.4f, 0.2f), v);
            p.Shape(Sd.Polygon(-0.25f, -0.08f, 0.15f, -0.05f, 0.05f, 0.25f, -0.15f, 0.18f), vs);
            p.Fill(Sd.Box(-0.05f, 0.06f, 0.12f, 0.02f), Gold);
            p.Shape(Sd.Polygon(0.2f, -0.05f, 0.55f, 0.0f, 0.4f, 0.15f), v);
            p.Shape(Sd.Polygon(0.45f, 0.18f, 0.6f, 0.25f, 0.52f, 0.3f), vs);
            p.Shape(Sd.Polygon(-0.48f, 0.22f, -0.35f, 0.3f, -0.42f, 0.33f), vs);
            return new DoodleArt("s41f_shards", p);
        }

        /// <summary>Sovrapposizione telecamera: angoli, righe di scansione (x ±2.8, y -2.9..3.0).</summary>
        public static DoodleArt S41F_CamOverlay()
        {
            var p = new DoodlePainter(-2.85f, -2.95f, 2.85f, 3.05f, 40) { Ink = 0f, Wobble = 0f };
            for (float y = -2.9f; y < 3.0f; y += 0.12f) p.Fill(Sd.Box(0, y, 2.85f, 0.015f), Rgba.Hex("203040", 0.08f), false);
            Rgba w = Rgba.White.WithAlpha(0.85f);
            float L = 0.45f, X = 2.6f, Yt = 2.8f, Yb = -2.7f;
            foreach (float sx in new[] { -1f, 1f })
                foreach (float yy in new[] { Yt, Yb })
                {
                    float sy = yy > 0 ? -1f : 1f;
                    p.Line(w, 0.06f, sx * X, yy + sy * L, sx * X, yy, sx * (X - L), yy);
                }
            return new DoodleArt("s41f_cam_overlay", p);
        }

        /// <summary>Cursore della timeline (centro 0,0).</summary>
        public static DoodleArt S41F_TapeHandle()
        {
            var p = new DoodlePainter(-0.32f, -0.4f, 0.32f, 0.4f, 160) { Ink = 0.04f };
            p.Shape(Sd.Box(0, 0, 0.22f, 0.32f, 0.1f), Rgba.Hex("FF5A6E"));
            p.Fill(Sd.Box(-0.07f, 0.08f, 0.06f, 0.14f, 0.04f), Rgba.White.WithAlpha(0.45f));
            foreach (float x in new[] { -0.07f, 0f, 0.07f }) p.InkLine(0.025f, x, -0.15f, x, 0.15f);
            return new DoodleArt("s41f_tape_handle", p);
        }

        /// <summary>Pulsante a pillola bianco (si tinge), 1.6 x 0.7.</summary>
        public static DoodleArt S41F_PillButton()
        {
            var p = new DoodlePainter(-0.88f, -0.42f, 0.88f, 0.42f, 120) { Ink = 0.05f };
            Sdf b = Sd.Box(0, 0, 0.8f, 0.33f, 0.3f);
            p.Fill(Sd.Box(0, -0.06f, 0.8f, 0.33f, 0.3f), Rgba.Hex("2B2233", 0.5f));
            p.Fill(b, Rgba.White);
            p.Fill(Sd.Box(0, -0.18f, 0.7f, 0.1f, 0.1f), Rgba.Hex("DADCE8"));
            p.Fill(Sd.Box(-0.35f, 0.17f, 0.25f, 0.05f, 0.05f), Rgba.White.WithAlpha(0.9f));
            p.Shape(b, Rgba.Clear);
            return new DoodleArt("s41f_pill_btn", p);
        }

        // ================= THE STICKY FINGER =================

        /// <summary>Banca: parete verde, soffitto con la gomma, pavimento in y -3.4.</summary>
        public static DoodleArt S41F_BankBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 1.0f, 3.3f, 4.4f), (x, y) => Rgba.Lerp(Rgba.Hex("BFE3C9"), Rgba.Hex("9CCFB0"), S41F_Clamp((y + 3.4f) / 8f)));
            for (float x = -2.9f; x < 3.2f; x += 1.15f)
            {
                p.Shape(Sd.Box(x, 0.4f, 0.12f, 2.6f), Rgba.Hex("E8F5EC"), 0.04f);
                p.Shape(Sd.Box(x, 3.05f, 0.2f, 0.08f), Rgba.Hex("E8F5EC"), 0.04f);
            }
            p.Shape(Sd.Box(0, 3.3f, 3.3f, 0.18f), Rgba.Hex("6FAE88"), 0.05f);
            p.FillFunc(Sd.Box(0, -4.3f, 3.3f, 0.9f), (x, y) => ((int)Math.Floor(x * 1.6f) + (int)Math.Floor(y * 1.6f)) % 2 == 0 ? Rgba.Hex("EDE6D8") : Rgba.Hex("C9BBA2"));
            p.Shape(Sd.Box(0, -3.4f, 3.3f, 0.06f), Rgba.Hex("6FAE88"), 0.04f);
            // insegna
            p.Shape(Sd.Box(-1.6f, 2.35f, 0.75f, 0.32f, 0.08f), Rgba.Hex("FFE08A"), 0.05f);
            p.Shape(Sd.Circle(-1.6f, 2.35f, 0.2f), Gold, 0.04f);
            p.InkLine(0.04f, -1.6f, 2.22f, -1.6f, 2.48f);
            p.InkLine(0.035f, -1.67f, 2.42f, -1.53f, 2.42f, -1.67f, 2.3f, -1.53f, 2.28f);
            return new DoodleArt("s41f_bank_bg", p);
        }

        /// <summary>Grumo di gomma da masticare appiccicato al soffitto (attacco in 0,0).</summary>
        public static DoodleArt S41F_GumWad()
        {
            var p = new DoodlePainter(-0.45f, -0.3f, 0.45f, 0.25f, 150) { Ink = 0.035f };
            Sdf g = Sd.Union(Sd.Ellipse(0, 0.05f, 0.38f, 0.17f), Sd.Circle(-0.12f, -0.08f, 0.1f), Sd.Circle(0.1f, -0.12f, 0.08f));
            Body(p, g, Rgba.Hex("FF8FC0"), Rgba.Hex("E5609E"));
            p.Fill(Sd.Ellipse(-0.15f, 0.12f, 0.1f, 0.04f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("s41f_gum_wad", p);
        }

        /// <summary>Moneta d'oro con gocce di gomma rosa (centro 0,0, raggio 0.38).</summary>
        public static DoodleArt S41F_StickyCoin()
        {
            var p = new DoodlePainter(-0.48f, -0.6f, 0.48f, 0.48f, 170) { Ink = 0.04f };
            Sdf c = Sd.Circle(0, 0, 0.38f);
            p.FillFunc(c, (x, y) => Rgba.Lerp(GoldShade, Gold, S41F_Clamp(0.5f + (y - x) * 0.9f)));
            p.Shape(c, Rgba.Clear);
            p.Shape(Sd.Circle(0, 0, 0.27f), Rgba.Clear, 0.025f);
            p.Line(GoldShade, 0.07f, 0.0f, -0.16f, 0.0f, 0.16f);
            p.Fill(Sd.Ellipse(-0.14f, 0.16f, 0.08f, 0.04f), GoldLight);
            // gomma che cola
            Sdf gum = Sd.Union(Sd.Ellipse(0.1f, -0.33f, 0.2f, 0.09f), Sd.Capsule(0.18f, -0.35f, 0.2f, -0.5f, 0.045f), Sd.Capsule(-0.02f, -0.36f, -0.03f, -0.44f, 0.035f));
            p.Shape(gum, Rgba.Hex("FF8FC0"), 0.03f);
            return new DoodleArt("s41f_sticky_coin", p);
        }

        /// <summary>Salvadanaio-cassaforte (base in y 0, fessura in alto in y 1.15).</summary>
        public static DoodleArt S41F_CoinSafe(bool happy)
        {
            var p = new DoodlePainter(-0.75f, -0.06f, 0.75f, 1.3f, 120) { Ink = 0.045f };
            Sdf box = Sd.Box(0, 0.6f, 0.65f, 0.55f, 0.1f);
            Body(p, box, Steel, SteelShade);
            p.Shape(Sd.Box(0, 1.15f, 0.3f, 0.06f, 0.04f), Rgba.Hex("2B2233"), 0.02f);
            p.Shape(Sd.Box(0, 0.5f, 0.5f, 0.38f, 0.06f), Rgba.Hex("C9CED9"), 0.03f);
            S41F_Eyes(p, 0, 0.62f, 0.2f, 0.1f, 0f, 0.6f);
            if (happy) p.Shape(Sd.Intersect(Sd.Circle(0, 0.38f, 0.14f), Sd.Box(0, 0.3f, 0.2f, 0.08f)), Rgba.Hex("D8434F"), 0.03f);
            else p.Shape(Sd.Ellipse(0, 0.33f, 0.08f, 0.06f), Rgba.Hex("5A2A3A"), 0.03f);
            p.Shape(Sd.Box(-0.45f, 0.04f, 0.1f, 0.06f), SteelDark, 0.03f);
            p.Shape(Sd.Box(0.45f, 0.04f, 0.1f, 0.06f), SteelDark, 0.03f);
            return new DoodleArt(happy ? "s41f_coin_safe_happy" : "s41f_coin_safe", p);
        }

        // ================= INTERROGATORIO (Guilty button, Lie detector) =================

        /// <summary>Stanza degli interrogatori: muro di mattoni scuri, lampada che pende, tavolo in y -1.6.</summary>
        public static DoodleArt S41F_InterroBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 1.8f, 3.3f, 3.5f), (x, y) =>
            {
                int row = (int)Math.Floor((y + 10f) / 0.42f);
                float off = row % 2 == 0 ? 0f : 0.35f;
                float bx = (x + 10f + off) % 0.7f, by = (y + 10f) % 0.42f;
                bool mortar = bx < 0.04f || by < 0.04f;
                Rgba b = Rgba.Lerp(Rgba.Hex("4A4458"), Rgba.Hex("5C556C"), (row * 37 % 7) / 7f);
                return mortar ? Rgba.Hex("332E40") : b;
            });
            // cono di luce
            p.Fill(Sd.Polygon(-0.25f, 3.3f, 0.25f, 3.3f, 2.7f, -1.6f, -2.7f, -1.6f), Rgba.Hex("FFF2B0", 0.13f), false);
            p.Line(Rgba.Hex("2B2233"), 0.04f, 0f, 5.3f, 0f, 3.55f);
            p.Shape(Sd.Polygon(-0.45f, 3.2f, 0.45f, 3.2f, 0.2f, 3.6f, -0.2f, 3.6f), Rgba.Hex("3E7A66"), 0.05f);
            p.Fill(Sd.Ellipse(0, 3.2f, 0.25f, 0.08f), Rgba.Hex("FFF6C8"));
            // tavolo
            p.FillFunc(Sd.Box(0, -3.45f, 3.3f, 1.85f), (x, y) => Rgba.Lerp(Rgba.Hex("3A2C24"), Rgba.Hex("6A4A36"), S41F_Clamp((y + 5.2f) / 3.6f)));
            p.Shape(Sd.Box(0, -1.65f, 3.3f, 0.1f), Rgba.Hex("8A6246"), 0.05f);
            p.Fill(Sd.Ellipse(0, -2.6f, 2.4f, 0.5f), Rgba.Hex("FFF2B0", 0.08f));
            return new DoodleArt("s41f_interro_bg", p);
        }

        /// <summary>Pulsante sospettato (si tinge): base scura, cupola chiara con faccia (base in y 0, alto 1.35).</summary>
        public static DoodleArt S41F_Suspect(bool pressed)
        {
            var p = new DoodlePainter(-0.72f, -0.06f, 0.72f, 1.45f, 130) { Ink = 0.045f };
            p.Shape(Sd.Box(0, 0.2f, 0.62f, 0.2f, 0.08f), Rgba.Hex("6E6A80"));
            p.Fill(Sd.Box(0, 0.12f, 0.55f, 0.06f, 0.04f), Rgba.Hex("57536A"));
            float h = pressed ? 0.6f : 0.9f;
            Sdf dome = Sd.Intersect(Sd.Ellipse(0, 0.4f, 0.5f, h), Sd.Box(0, 0.4f + h * 0.5f, 0.6f, h * 0.5f));
            Body(p, dome, Rgba.White, Rgba.Hex("D8D8E2"), null, 0.08f, 0.08f);
            p.Fill(Sd.Ellipse(-0.22f, 0.4f + h * 0.7f, 0.08f, 0.13f), Rgba.White.WithAlpha(0.9f));
            float ey = 0.4f + h * 0.45f;
            S41F_Eyes(p, 0, ey, 0.17f, 0.09f, pressed ? 0.6f : -0.5f, pressed ? -0.4f : 0.3f);
            // sopracciglia preoccupate
            p.InkLine(0.025f, -0.28f, ey + 0.17f, -0.08f, ey + 0.13f);
            p.InkLine(0.025f, 0.28f, ey + 0.17f, 0.08f, ey + 0.13f);
            if (pressed) p.Shape(Sd.Ellipse(0, ey - 0.2f, 0.07f, 0.05f), Rgba.Hex("5A2A3A"), 0.02f);
            else p.InkLine(0.025f, -0.12f, ey - 0.2f, -0.04f, ey - 0.17f, 0.04f, ey - 0.2f, 0.12f, ey - 0.17f);
            return new DoodleArt(pressed ? "s41f_suspect_pressed" : "s41f_suspect", p);
        }

        /// <summary>Goccia di sudore: acqua (azzurra) o metallica (argento lucido). Centro 0,0.</summary>
        public static DoodleArt S41F_SweatDrop(bool metal)
        {
            var p = new DoodlePainter(-0.18f, -0.18f, 0.18f, 0.24f, 200) { Ink = 0.025f };
            Sdf d = Sd.Union(Sd.Circle(0, -0.04f, 0.12f), Sd.Polygon(-0.1f, 0.02f, 0.1f, 0.02f, 0f, 0.2f));
            if (metal)
            {
                p.FillFunc(d, (x, y) => Rgba.Lerp(Rgba.Hex("6D7385"), Rgba.Hex("F2F4FA"), S41F_Clamp(0.5f + 0.5f * S41F_Sin((x + y) * 24f))));
                p.Shape(d, Rgba.Clear);
                p.Fill(Sd.Circle(-0.04f, 0.0f, 0.03f), Rgba.White);
            }
            else
            {
                p.Shape(d, Rgba.Hex("8FD3FF"));
                p.Fill(Sd.Circle(-0.04f, -0.02f, 0.03f), Rgba.White.WithAlpha(0.9f));
            }
            return new DoodleArt(metal ? "s41f_drop_metal" : "s41f_drop_water", p);
        }

        /// <summary>Detective con impermeabile, cappello e lente (piedi in y 0, alto 1.7).</summary>
        public static DoodleArt S41F_Detective()
        {
            var p = new DoodlePainter(-0.65f, -0.06f, 0.75f, 1.8f, 120) { Ink = 0.045f };
            Rgba coat = Rgba.Hex("C9A26A"), coatS = Rgba.Hex("A57F4A");
            p.Shape(Sd.Ellipse(-0.15f, 0.05f, 0.14f, 0.06f), Rgba.Hex("3A2C24"));
            p.Shape(Sd.Ellipse(0.17f, 0.05f, 0.14f, 0.06f), Rgba.Hex("3A2C24"));
            Body(p, Sd.Polygon(-0.4f, 0.1f, 0.42f, 0.1f, 0.3f, 1.05f, -0.3f, 1.05f), coat, coatS);
            p.InkLine(0.025f, 0f, 0.15f, 0f, 1.0f);
            p.Shape(Sd.Polygon(-0.18f, 1.05f, 0.18f, 1.05f, 0f, 0.8f), Rgba.White, 0.02f);
            p.Shape(Sd.Polygon(-0.04f, 0.98f, 0.04f, 0.98f, 0.06f, 0.75f, 0f, 0.7f, -0.06f, 0.75f), Rgba.Hex("D8434F"), 0.015f);
            // braccio con lente
            p.Shape(Sd.Capsule(0.3f, 0.8f, 0.5f, 0.95f, 0.07f), coat);
            p.Shape(Sd.Capsule(0.5f, 0.95f, 0.55f, 1.05f, 0.03f), WoodDark, 0.02f);
            p.Shape(Sd.Circle(0.58f, 1.2f, 0.15f), Rgba.Hex("CDEBFF", 0.6f), 0.04f);
            p.Fill(Sd.Circle(0.54f, 1.25f, 0.04f), Rgba.White);
            // testa
            p.Shape(Sd.Circle(0, 1.25f, 0.25f), Skin);
            p.Fill(Sd.Circle(-0.13f, 1.18f, 0.05f), BlushColor.WithAlpha(0.6f));
            p.InkLine(0.03f, -0.12f, 1.27f, -0.03f, 1.27f);
            p.Fill(Sd.Circle(0.08f, 1.27f, 0.035f), Ink);
            p.InkLine(0.022f, 0.02f, 1.36f, 0.14f, 1.33f);
            p.InkLine(0.02f, -0.07f, 1.13f, 0.06f, 1.12f);
            // cappello
            p.Shape(Sd.Ellipse(0, 1.43f, 0.38f, 0.07f), Rgba.Hex("7A5A3A"));
            p.Shape(Sd.Box(0, 1.56f, 0.22f, 0.13f, 0.06f), Rgba.Hex("8A6A48"));
            p.Fill(Sd.Box(0, 1.48f, 0.22f, 0.04f), Rgba.Hex("3A2C24"));
            return new DoodleArt("s41f_detective", p);
        }

        /// <summary>Robot interrogatore (piedi in y 0; schermo-faccia centrato in 0,1.55, 0.9 x 0.6).</summary>
        public static DoodleArt S41F_Robot()
        {
            var p = new DoodlePainter(-1.05f, -0.06f, 1.05f, 2.45f, 110) { Ink = 0.05f };
            p.Shape(Sd.Box(-0.3f, 0.25f, 0.12f, 0.25f), SteelShade);
            p.Shape(Sd.Box(0.3f, 0.25f, 0.12f, 0.25f), SteelShade);
            p.Shape(Sd.Box(-0.3f, 0.05f, 0.2f, 0.07f, 0.04f), SteelDark);
            p.Shape(Sd.Box(0.3f, 0.05f, 0.2f, 0.07f, 0.04f), SteelDark);
            p.Shape(Sd.Capsule(-0.6f, 0.95f, -0.92f, 0.55f, 0.09f), Steel);
            p.Shape(Sd.Capsule(0.6f, 0.95f, 0.92f, 0.55f, 0.09f), Steel);
            p.Shape(Sd.Circle(-0.95f, 0.5f, 0.12f), SteelDark);
            p.Shape(Sd.Circle(0.95f, 0.5f, 0.12f), SteelDark);
            Body(p, Sd.Box(0, 0.85f, 0.58f, 0.4f, 0.12f), Rgba.Hex("8FB8E8"), Rgba.Hex("6A93C8"));
            p.Shape(Sd.Box(0, 0.85f, 0.3f, 0.18f, 0.05f), Rgba.Hex("DDEBFA"), 0.03f);
            p.Fill(Sd.Circle(-0.15f, 0.85f, 0.05f), Rgba.Hex("FF5A6E"));
            p.Fill(Sd.Circle(0f, 0.85f, 0.05f), Rgba.Hex("FFD54A"));
            p.Fill(Sd.Circle(0.15f, 0.85f, 0.05f), Rgba.Hex("6ED38A"));
            p.Shape(Sd.Box(0, 1.27f, 0.12f, 0.05f), SteelShade, 0.03f);
            Body(p, Sd.Box(0, 1.6f, 0.6f, 0.42f, 0.14f), Steel, SteelShade);
            p.Shape(Sd.Box(0, 1.58f, 0.46f, 0.3f, 0.08f), Rgba.Hex("1E2A36"), 0.035f);
            p.InkLine(0.04f, 0f, 2.02f, 0f, 2.25f);
            p.Shape(Sd.Circle(0, 2.3f, 0.08f), Rgba.Hex("FF5A6E"), 0.03f);
            p.Shape(Sd.Box(-0.64f, 1.6f, 0.06f, 0.15f, 0.03f), SteelDark, 0.03f);
            p.Shape(Sd.Box(0.64f, 1.6f, 0.06f, 0.15f, 0.03f), SteelDark, 0.03f);
            return new DoodleArt("s41f_robot", p);
        }

        /// <summary>Faccia sullo schermo del robot (centro 0,0): calma o in panico.</summary>
        public static DoodleArt S41F_RobotFace(bool panic)
        {
            var p = new DoodlePainter(-0.44f, -0.28f, 0.44f, 0.28f, 150) { Ink = 0f, Wobble = 0.004f };
            Rgba g = panic ? Rgba.Hex("FF6A6A") : Rgba.Hex("7CF0B0");
            if (panic)
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    p.Line(g, 0.05f, s * 0.24f - 0.07f, 0.13f, s * 0.24f + 0.07f, -0.01f);
                    p.Line(g, 0.05f, s * 0.24f - 0.07f, -0.01f, s * 0.24f + 0.07f, 0.13f);
                }
                p.Line(g, 0.04f, -0.2f, -0.15f, -0.1f, -0.1f, 0f, -0.17f, 0.1f, -0.1f, 0.2f, -0.15f);
            }
            else
            {
                p.Fill(Sd.Box(-0.22f, 0.06f, 0.08f, 0.05f, 0.02f), g);
                p.Fill(Sd.Box(0.22f, 0.06f, 0.08f, 0.05f, 0.02f), g);
                p.Fill(Sd.Box(0f, -0.14f, 0.18f, 0.025f), g);
            }
            return new DoodleArt(panic ? "s41f_robot_face_panic" : "s41f_robot_face", p);
        }

        /// <summary>Macchina della verità (centro 0,0, 1.5 x 1.1): carta millimetrata e lampadina; perno ago in 0,-0.1.</summary>
        public static DoodleArt S41F_Polygraph()
        {
            var p = new DoodlePainter(-0.82f, -0.62f, 0.82f, 0.72f, 120) { Ink = 0.045f };
            Body(p, Sd.Box(0, -0.05f, 0.75f, 0.52f, 0.1f), Rgba.Hex("D86A4A"), Rgba.Hex("B04E34"));
            Sdf paper = Sd.Box(0, 0.08f, 0.6f, 0.3f, 0.03f);
            p.Fill(paper, Rgba.Hex("FFFBEA"));
            for (float x = -0.55f; x < 0.6f; x += 0.1f) p.Line(Rgba.Hex("F2B8B8", 0.8f), 0.01f, x, -0.21f, x, 0.37f);
            p.Line(Rgba.Hex("2B2233", 0.7f), 0.02f, -0.58f, 0.1f, -0.4f, 0.12f, -0.3f, 0.06f, -0.2f, 0.14f, -0.1f, 0.08f, 0f, 0.1f);
            p.Shape(paper, Rgba.Clear, 0.03f);
            p.Shape(Sd.Circle(-0.5f, -0.4f, 0.08f), Rgba.Hex("FFE36A"), 0.03f);
            p.Shape(Sd.Circle(-0.25f, -0.4f, 0.08f), Rgba.Hex("6ED38A"), 0.03f);
            p.Fill(Sd.Box(0.3f, -0.4f, 0.3f, 0.05f, 0.03f), Rgba.Hex("2B2233"));
            p.Shape(Sd.Box(0, 0.6f, 0.08f, 0.08f, 0.02f), SteelShade, 0.03f);
            return new DoodleArt("s41f_polygraph", p);
        }

        /// <summary>Ago del poligrafo (perno in 0,0, punta in alto).</summary>
        public static DoodleArt S41F_PolyNeedle()
        {
            var p = new DoodlePainter(-0.08f, -0.08f, 0.08f, 0.42f, 200) { Ink = 0.02f };
            p.Shape(Sd.Polygon(-0.025f, 0f, 0.025f, 0f, 0f, 0.38f), Rgba.Hex("2B2233"));
            p.Shape(Sd.Circle(0, 0, 0.05f), SteelDark);
            return new DoodleArt("s41f_poly_needle", p);
        }

        // ================= THE INVISIBLE STAIRS =================

        /// <summary>Cielo al tramonto con stelle, prato in basso (terreno in y -3.4).</summary>
        public static DoodleArt S41F_StairsBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) =>
            {
                float t = S41F_Clamp((y + 3.4f) / 8.5f);
                return t < 0.5f ? Rgba.Lerp(Rgba.Hex("FFB38A"), Rgba.Hex("B98AD8"), t * 2f) : Rgba.Lerp(Rgba.Hex("B98AD8"), Rgba.Hex("4A4A9A"), (t - 0.5f) * 2f);
            });
            var r = new Random(41);
            for (int i = 0; i < 26; i++)
            {
                float x = (float)r.NextDouble() * 6.2f - 3.1f, y = (float)r.NextDouble() * 3.0f + 1.8f;
                p.Fill(Sd.Circle(x, y, 0.03f + (float)r.NextDouble() * 0.03f), Rgba.White.WithAlpha(0.8f));
            }
            p.Shape(Sd.Union(Sd.Circle(-2.3f, 0.2f, 0.35f), Sd.Circle(-1.9f, 0.3f, 0.45f), Sd.Circle(-1.5f, 0.15f, 0.3f)), Rgba.White.WithAlpha(0.55f), 0f);
            p.Shape(Sd.Union(Sd.Circle(2.4f, -1.4f, 0.3f), Sd.Circle(2.0f, -1.3f, 0.38f)), Rgba.White.WithAlpha(0.45f), 0f);
            // colline e prato
            p.Shape(Sd.Union(Sd.Circle(-2.2f, -4.6f, 1.6f), Sd.Circle(1.8f, -4.9f, 1.9f)), Rgba.Hex("7FB868"), 0.05f);
            p.FillFunc(Sd.Box(0, -4.4f, 3.3f, 1.0f), (x, y) => Rgba.Lerp(Rgba.Hex("5E9E4E"), Rgba.Hex("78BE5E"), S41F_Clamp((y + 5.2f) / 1.8f)));
            p.Line(Rgba.Hex("3E7A36"), 0.05f, -3.3f, -3.4f, 3.3f, -3.4f);
            for (float x = -3f; x < 3.2f; x += 0.35f) p.Line(Rgba.Hex("4E8E3E"), 0.03f, x, -3.42f, x + 0.06f, -3.3f);
            return new DoodleArt("s41f_stairs_bg", p);
        }

        /// <summary>Porta su una nuvoletta (base della porta in 0,0; nuvola sotto, alta 1.4).</summary>
        public static DoodleArt S41F_CloudDoor(bool open)
        {
            var p = new DoodlePainter(-0.95f, -0.45f, 0.95f, 1.55f, 110) { Ink = 0.045f };
            Sdf frame = Sd.Box(0, 0.7f, 0.5f, 0.72f, 0.12f);
            p.Shape(frame, Rgba.Hex("8A5A3A"));
            if (open)
            {
                p.FillFunc(Sd.Box(0, 0.67f, 0.4f, 0.64f, 0.08f), (x, y) => Rgba.Lerp(Rgba.Hex("FFF6C8"), Rgba.Hex("FFD27A"), S41F_Clamp(y / 1.3f)));
                p.Shape(Sd.Polygon(-0.4f, 0.03f, -0.62f, 0.15f, -0.62f, 1.2f, -0.4f, 1.31f), Rgba.Hex("D97A4A"), 0.035f);
            }
            else
            {
                p.Shape(Sd.Box(0, 0.67f, 0.4f, 0.64f, 0.08f), Rgba.Hex("D97A4A"), 0.035f);
                p.Shape(Sd.Box(0, 0.95f, 0.28f, 0.22f, 0.04f), Rgba.Hex("E99A6A"), 0.025f);
                p.Shape(Sd.Box(0, 0.38f, 0.28f, 0.22f, 0.04f), Rgba.Hex("E99A6A"), 0.025f);
                p.Shape(Sd.Circle(0.28f, 0.65f, 0.06f), Gold, 0.025f);
            }
            Sdf cloud = Sd.Union(Sd.Ellipse(0, -0.15f, 0.85f, 0.2f), Sd.Circle(-0.45f, -0.08f, 0.25f), Sd.Circle(0.05f, -0.02f, 0.28f), Sd.Circle(0.5f, -0.1f, 0.24f));
            Body(p, cloud, Rgba.White, Rgba.Hex("DDE4F5"), null, 0.05f, 0.06f);
            return new DoodleArt(open ? "s41f_cloud_door_open" : "s41f_cloud_door", p);
        }

        /// <summary>Omino scalatore con zainetto (piedi in y 0, alto 0.95).</summary>
        public static DoodleArt S41F_Climber()
        {
            var p = new DoodlePainter(-0.42f, -0.05f, 0.42f, 1.05f, 160) { Ink = 0.04f };
            p.Shape(Sd.Capsule(-0.1f, 0.06f, -0.1f, 0.3f, 0.06f), Rgba.Hex("3E5A8A"));
            p.Shape(Sd.Capsule(0.1f, 0.06f, 0.1f, 0.3f, 0.06f), Rgba.Hex("3E5A8A"));
            p.Shape(Sd.Ellipse(-0.13f, 0.04f, 0.09f, 0.045f), Rgba.Hex("7A3F1D"));
            p.Shape(Sd.Ellipse(0.13f, 0.04f, 0.09f, 0.045f), Rgba.Hex("7A3F1D"));
            p.Shape(Sd.Box(-0.25f, 0.5f, 0.1f, 0.15f, 0.05f), Rgba.Hex("E9C25A"));
            Body(p, Sd.Box(0, 0.48f, 0.19f, 0.2f, 0.08f), Rgba.Hex("6ED3A0"), Rgba.Hex("4AAA7E"));
            p.Shape(Sd.Capsule(0.17f, 0.55f, 0.32f, 0.38f, 0.045f), Rgba.Hex("6ED3A0"));
            p.Shape(Sd.Circle(0, 0.82f, 0.19f), Skin);
            S41F_Eyes(p, 0.03f, 0.84f, 0.07f, 0.045f, 0.4f, 0.4f);
            p.InkLine(0.018f, -0.02f, 0.74f, 0.04f, 0.72f, 0.09f, 0.75f);
            p.Shape(Sd.Intersect(Sd.Circle(0, 0.86f, 0.21f), Sd.Box(0, 0.98f, 0.25f, 0.1f)), Rgba.Hex("E8433F"));
            return new DoodleArt("s41f_climber", p);
        }

        /// <summary>Gradino di vetro rivelato (piano in y 0, largo 1.0).</summary>
        public static DoodleArt S41F_GlassStep()
        {
            var p = new DoodlePainter(-0.6f, -0.3f, 0.6f, 0.1f, 140) { Ink = 0.035f };
            p.InkColor = Rgba.Hex("3E7AB8");
            Sdf s = Sd.Box(0, -0.11f, 0.5f, 0.11f, 0.04f);
            p.Shape(s, Rgba.Hex("BFE6FF", 0.6f));
            p.Line(Rgba.White.WithAlpha(0.9f), 0.03f, -0.4f, -0.04f, -0.1f, -0.04f);
            p.Line(Rgba.White.WithAlpha(0.7f), 0.02f, 0.15f, -0.17f, 0.35f, -0.04f);
            return new DoodleArt("s41f_glass_step", p);
        }

        // ================= TOO BRIGHT =================

        /// <summary>Stanza con due finestre (centri x -1.25 e 1.2, y da 0.5 a 2.3), angolo buio a sinistra, pavimento in y -3.4.</summary>
        public static DoodleArt S41F_BrightBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 1.0f, 3.3f, 4.4f), (x, y) => Rgba.Lerp(Rgba.Hex("E9D8F0"), Rgba.Hex("CDB8DA"), S41F_Clamp((y + 3.4f) / 8f)));
            p.FillFunc(Sd.Box(0, -4.3f, 3.3f, 0.9f), (x, y) => Rgba.Lerp(Rgba.Hex("6A4A6A"), Rgba.Hex("8A6A7E"), S41F_Clamp((y + 5.2f) / 1.8f)));
            p.Shape(Sd.Box(0, -3.4f, 3.3f, 0.07f), Rgba.Hex("5A3A5A"), 0.04f);
            // angolo buio a sinistra (ombra)
            p.FillFunc(Sd.Box(-2.6f, -1.0f, 0.75f, 4.3f), (x, y) => Rgba.Hex("2B1A3A", 0.55f * S41F_Clamp((-1.9f - x) / 0.8f + 0.25f)));
            foreach (float cx in new[] { -1.25f, 1.2f })
            {
                Sdf frame = Sd.Box(cx, 1.4f, 0.82f, 0.98f, 0.06f);
                p.Shape(frame, Rgba.White, 0.06f);
                p.FillFunc(Sd.Box(cx, 1.4f, 0.72f, 0.88f), (x, y) => Rgba.Lerp(Rgba.Hex("FFF3B0"), Rgba.Hex("8FD3FF"), S41F_Clamp((y - 0.5f) / 1.8f)));
                p.Shape(Sd.Circle(cx + 0.3f, 1.95f, 0.22f), Rgba.Hex("FFE36A"), 0.035f);
                p.Line(Rgba.White, 0.07f, cx, 0.52f, cx, 2.28f);
                p.Line(Rgba.White, 0.07f, cx - 0.72f, 1.4f, cx + 0.72f, 1.4f);
                p.Shape(Sd.Box(cx, 0.42f, 0.92f, 0.06f), Rgba.White, 0.04f);
                // bastone della tenda
                p.Shape(Sd.Box(cx, 2.48f, 0.95f, 0.04f), WoodDark, 0.03f);
                p.Shape(Sd.Circle(cx - 0.97f, 2.48f, 0.07f), Gold, 0.025f);
                p.Shape(Sd.Circle(cx + 0.97f, 2.48f, 0.07f), Gold, 0.025f);
            }
            // quadro col pipistrello
            p.Shape(Sd.Box(0f, -0.6f, 0.35f, 0.3f, 0.03f), Gold, 0.04f);
            p.Fill(Sd.Box(0f, -0.6f, 0.26f, 0.21f), Rgba.Hex("3A2A5A"));
            p.Fill(Sd.Polygon(-0.18f, -0.55f, 0f, -0.65f, 0.18f, -0.55f, 0.08f, -0.6f, 0f, -0.52f, -0.08f, -0.6f), Rgba.Hex("9A8ACA"));
            return new DoodleArt("s41f_bright_bg", p);
        }

        /// <summary>Raggio di sole (in alto largo 1.4 in y 0, a terra largo 1.9 in y -3.85, inclinato a destra).</summary>
        public static DoodleArt S41F_Sunbeam()
        {
            var p = new DoodlePainter(-0.8f, -3.9f, 1.6f, 0.05f, 40) { Ink = 0f, Wobble = 0f };
            Sdf b = Sd.Polygon(-0.7f, 0f, 0.7f, 0f, 1.5f, -3.85f, -0.4f, -3.85f);
            p.FillFunc(b, (x, y) => Rgba.Hex("FFF0A0", 0.45f + 0.3f * S41F_Clamp(-y / 3.85f)));
            for (int i = 0; i < 4; i++)
            {
                float u = -0.5f + i * 0.35f;
                p.Line(Rgba.Hex("FFFBE0", 0.3f), 0.08f, u, 0f, u + 0.6f + i * 0.08f, -3.8f);
            }
            return new DoodleArt("s41f_sunbeam", p);
        }

        /// <summary>Tenda di velluto (bordo sinistro in alto in 0,0; larga 1.64, alta 1.95).</summary>
        public static DoodleArt S41F_CurtainPanel()
        {
            var p = new DoodlePainter(-0.05f, -2.0f, 1.7f, 0.08f, 90) { Ink = 0.045f };
            Sdf c = Sd.Box(0.82f, -0.98f, 0.82f, 0.98f, 0.04f);
            p.FillFunc(c, (x, y) => Rgba.Lerp(Rgba.Hex("5A1E5E"), Rgba.Hex("8A3A8E"), 0.5f + 0.5f * S41F_Sin(x * 16f)));
            p.Shape(c, Rgba.Clear);
            p.Fill(Sd.Box(0.82f, -1.85f, 0.82f, 0.05f), Gold);
            for (float x = 0.1f; x < 1.7f; x += 0.2f) p.Shape(Sd.Circle(x, -0.02f, 0.05f), Gold, 0.02f);
            return new DoodleArt("s41f_curtain", p);
        }

        /// <summary>Vampirello col mantello (piedi in y 0, alto 1.15); burnt = fumante e arrossato.</summary>
        public static DoodleArt S41F_Vampire(bool burnt)
        {
            var p = new DoodlePainter(-0.6f, -0.06f, 0.6f, 1.25f, 140) { Ink = 0.045f };
            Rgba skin = burnt ? Rgba.Hex("FF9A8A") : Rgba.Hex("C8E6D8");
            Sdf cape = Sd.Polygon(-0.5f, 0.05f, 0.5f, 0.05f, 0.35f, 0.78f, -0.35f, 0.78f);
            p.Shape(cape, Rgba.Hex("2E2240"));
            p.Fill(Sd.Polygon(-0.25f, 0.1f, 0.25f, 0.1f, 0.15f, 0.7f, -0.15f, 0.7f), Rgba.Hex("D8434F"));
            p.Shape(Sd.Polygon(-0.42f, 0.8f, -0.12f, 0.68f, -0.5f, 1.05f), Rgba.Hex("D8434F"), 0.03f);
            p.Shape(Sd.Polygon(0.42f, 0.8f, 0.12f, 0.68f, 0.5f, 1.05f), Rgba.Hex("D8434F"), 0.03f);
            Sdf head = Sd.Circle(0, 0.88f, 0.26f);
            p.Shape(head, skin);
            p.Fill(Sd.Polygon(-0.24f, 1.0f, 0.24f, 1.0f, 0.2f, 1.13f, 0f, 1.0f, -0.2f, 1.13f), Rgba.Hex("2E2240"));
            if (burnt)
            {
                p.InkLine(0.025f, -0.15f, 0.92f, -0.05f, 0.87f);
                p.InkLine(0.025f, 0.15f, 0.92f, 0.05f, 0.87f);
                p.Shape(Sd.Ellipse(0, 0.75f, 0.08f, 0.06f), Rgba.Hex("5A2A3A"), 0.02f);
            }
            else
            {
                S41F_Eyes(p, 0, 0.9f, 0.1f, 0.06f, 0.5f, 0f);
                p.InkLine(0.02f, -0.1f, 0.76f, 0f, 0.73f, 0.1f, 0.76f);
                p.Fill(Sd.Polygon(-0.07f, 0.745f, -0.03f, 0.745f, -0.05f, 0.69f), Rgba.White);
                p.Fill(Sd.Polygon(0.03f, 0.745f, 0.07f, 0.745f, 0.05f, 0.69f), Rgba.White);
            }
            p.Fill(Sd.Circle(-0.16f, 0.8f, 0.04f), BlushColor.WithAlpha(burnt ? 0.9f : 0.4f));
            p.Fill(Sd.Circle(0.16f, 0.8f, 0.04f), BlushColor.WithAlpha(burnt ? 0.9f : 0.4f));
            return new DoodleArt(burnt ? "s41f_vampire_burnt" : "s41f_vampire", p);
        }

        /// <summary>Bara in piedi aperta (base in y 0, alta 1.9).</summary>
        public static DoodleArt S41F_Coffin()
        {
            var p = new DoodlePainter(-0.6f, -0.06f, 0.6f, 1.98f, 110) { Ink = 0.05f };
            Sdf c = Sd.Polygon(-0.3f, 0f, 0.3f, 0f, 0.5f, 1.35f, 0.3f, 1.9f, -0.3f, 1.9f, -0.5f, 1.35f);
            p.Shape(c, Rgba.Hex("6A3E2A"));
            Sdf inner = Sd.Polygon(-0.22f, 0.1f, 0.22f, 0.1f, 0.4f, 1.33f, 0.24f, 1.8f, -0.24f, 1.8f, -0.4f, 1.33f);
            p.FillFunc(inner, (x, y) => Rgba.Lerp(Rgba.Hex("6A2A7A"), Rgba.Hex("A85AB8"), S41F_Clamp(y / 1.8f)));
            p.Fill(Sd.Ellipse(0, 1.55f, 0.18f, 0.1f), Rgba.Hex("F2D8F2"));
            p.Shape(inner, Rgba.Clear, 0.03f);
            p.Line(Rgba.Hex("FFE36A"), 0.04f, 0f, 0.6f, 0f, 1.0f);
            p.Line(Rgba.Hex("FFE36A"), 0.04f, -0.13f, 0.88f, 0.13f, 0.88f);
            return new DoodleArt("s41f_coffin", p);
        }
    }
}
