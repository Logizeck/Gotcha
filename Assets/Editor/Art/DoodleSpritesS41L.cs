using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli della specifica 105-200, pacchetto L (e i quattro livelli di Toni).</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV41L() => new DoodleArt[]
        {
            // The great escape
            S41L_PrisonBg(), S41L_Bar(), S41L_TileLoose(), S41L_Hole(), S41L_Spoon(), S41L_Bowl(),
            S41L_Jailer(false), S41L_Jailer(true), S41L_Desk(), S41L_Cone(), S41L_Megaphone(), S41L_Sign(), S41L_Contract(), S41L_DoorOpen(),
            // The impossible shot
            S41L_CourtBg(), S41L_BrickWall(), S41L_BasketTop(), S41L_Dot(),
            // The endless staircase
            S41L_TowerBg(), S41L_Step(), S41L_TopSign(), S41L_Climber(), S41L_Summit(),
            // Almost there
            S41L_ArcadeBg(), S41L_Scoreboard(), S41L_HiddenDot(),
            // Toni: X-ray
            S41L_XrayBones(), S41L_Scanner(), S41L_Fridge(), S41L_Padlock(),
            // Toni: beauty filter
            S41L_BfHead(), S41L_BfEar(), S41L_BfEye(), S41L_BfSnout(), S41L_BfPanel(), S41L_Antenna(),
            // Toni: personal trainer
            S41L_Hurdle(), S41L_Tire(), S41L_Finish(),
            // Toni: final exam
            S41L_ClassBg(), S41L_Professor(), S41L_ProfDesk(), S41L_Glasses(), S41L_Mortarboard(), S41L_Cup(),
        };

        static float S41L_Sin(float v) => (float)Math.Sin(v);
        static float S41L_Cos(float v) => (float)Math.Cos(v);
        static float S41L_Clamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
        static readonly Rgba S41L_Stone = Rgba.Hex("9EA3B5"), S41L_StoneShade = Rgba.Hex("7D8296"), S41L_Steel = Rgba.Hex("B9BFCE"), S41L_SteelShade = Rgba.Hex("7F8699");

        // ================= THE GREAT ESCAPE =================

        /// <summary>Prigione: cella di pietra a sinistra (pavimento a piastrelle), stanza della guardia a destra, finestrella col cielo.</summary>
        public static DoodleArt S41L_PrisonBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            // muro di pietra della cella
            p.FillFunc(Sd.Box(-1.2f, 1.6f, 2.1f, 3.7f), (x, y) =>
            {
                int row = (int)Math.Floor((y + 10f) / 0.5f);
                float bx = x + (row % 2) * 0.35f;
                float fx = bx / 0.7f - (float)Math.Floor(bx / 0.7f), fy = (y + 10f) / 0.5f - row;
                bool mortar = fx < 0.06f || fy < 0.1f;
                return mortar ? Rgba.Hex("6E7286") : Rgba.Lerp(S41L_Stone, S41L_StoneShade, 0.3f + 0.3f * S41L_Sin(bx * 3f + row));
            });
            // stanza della guardia: carta da parati calda
            p.FillFunc(Sd.Box(2.15f, 1.6f, 1.05f, 3.7f), (x, y) => (int)Math.Floor(x * 4f) % 2 == 0 ? Rgba.Hex("E8C690") : Rgba.Hex("DDB57C"));
            // pavimento a piastrelle (cella) e assi (guardia)
            p.FillFunc(Sd.Box(-1.2f, -3.95f, 2.1f, 1.95f), (x, y) =>
            {
                float fx = (x + 10f) / 0.8f, fy = (y + 10f) / 0.8f;
                bool joint = fx - (float)Math.Floor(fx) < 0.05f || fy - (float)Math.Floor(fy) < 0.05f;
                return joint ? Rgba.Hex("5E6274") : Rgba.Lerp(Rgba.Hex("B4B0A4"), Rgba.Hex("9C978B"), S41L_Clamp(0.5f + 0.4f * S41L_Sin(x * 2f + y * 3f)));
            });
            p.FillFunc(Sd.Box(2.15f, -3.95f, 1.05f, 1.95f), (x, y) => (int)Math.Floor((y + 10f) * 3f) % 2 == 0 ? Wood : WoodShade);
            p.Line(Ink, 0.05f, -3.3f, -2.0f, 3.3f, -2.0f);
            // finestrella col cielo notturno (le sbarre sono a parte)
            Sdf win = Sd.Box(-1.05f, 1.15f, 1.35f, 0.95f, 0.05f);
            p.FillFunc(win, (x, y) => Rgba.Lerp(Rgba.Hex("2A3166"), Rgba.Hex("121636"), S41L_Clamp((y - 0.2f) / 1.9f)));
            p.Shape(Sd.Subtract(Sd.Circle(-0.2f, 1.7f, 0.28f), Sd.Circle(-0.08f, 1.78f, 0.24f)), Rgba.Hex("FFF1A8"), 0.03f);
            for (int i = 0; i < 7; i++) p.Fill(Sd.Circle(-2.2f + i * 0.33f, 0.5f + (i * 0.71f) % 1.2f, 0.03f), Rgba.White.WithAlpha(0.8f));
            p.Shape(win, Rgba.Clear, 0.09f);
            p.Shape(Sd.Box(-1.05f, 0.15f, 1.45f, 0.08f, 0.03f), S41L_StoneShade);
            // tramezzo di sbarre fitte tra cella e guardia
            p.Shape(Sd.Box(0.92f, -0.3f, 0.06f, 3.6f), S41L_SteelShade, 0.03f);
            for (int i = 0; i < 2; i++) p.Shape(Sd.Box(0.92f, 3.0f - i * 5.0f, 0.14f, 0.06f), S41L_Steel, 0.03f);
            // porta principale nella stanza della guardia
            p.Shape(Sd.Box(2.55f, -0.55f, 0.4f, 1.45f, 0.08f), WoodShade);
            p.Shape(Sd.Box(2.55f, -0.55f, 0.3f, 1.33f, 0.05f), Wood, 0.03f);
            p.Shape(Sd.Circle(2.32f, -0.65f, 0.06f), Gold, 0.03f);
            // cartello EXIT sopra la tana nel muro esterno (in basso a destra)
            p.Shape(Sd.Box(2.35f, -2.85f, 0.35f, 0.15f, 0.04f), Rgba.Hex("4CBF5A"), 0.03f);
            p.Line(Rgba.White, 0.035f, 2.12f, -2.78f, 2.12f, -2.92f, 2.22f, -2.92f);
            p.Line(Rgba.White, 0.035f, 2.12f, -2.85f, 2.2f, -2.85f);
            p.Line(Rgba.White, 0.035f, 2.12f, -2.78f, 2.22f, -2.78f);
            p.Line(Rgba.White, 0.035f, 2.28f, -2.78f, 2.38f, -2.92f);
            p.Line(Rgba.White, 0.035f, 2.38f, -2.78f, 2.28f, -2.92f);
            p.Line(Rgba.White, 0.035f, 2.45f, -2.78f, 2.45f, -2.92f);
            p.Line(Rgba.White, 0.035f, 2.52f, -2.78f, 2.62f, -2.78f);
            p.Line(Rgba.White, 0.035f, 2.57f, -2.78f, 2.57f, -2.92f);
            // tane nei muri della cella
            foreach (float hx in new[] { -2.55f, -0.2f })
            {
                p.Shape(Sd.Union(Sd.Circle(hx, -1.75f, 0.22f), Sd.Box(hx, -1.9f, 0.22f, 0.12f)), Rgba.Hex("1E1826"), 0.04f);
            }
            p.Shape(Sd.Union(Sd.Circle(2.35f, -3.6f, 0.26f), Sd.Box(2.35f, -3.8f, 0.26f, 0.2f)), Rgba.Hex("1E1826"), 0.04f);
            return new DoodleArt("s41l_prison_bg", p);
        }

        /// <summary>Sbarra della finestrella (centro 0,0, alta 1.9).</summary>
        public static DoodleArt S41L_Bar()
        {
            var p = new DoodlePainter(-0.12f, -1.0f, 0.12f, 1.0f, 120) { Ink = 0.035f };
            Body(p, Sd.Box(0, 0, 0.065f, 0.93f, 0.05f), S41L_Steel, S41L_SteelShade);
            p.Fill(Sd.Box(-0.025f, 0.1f, 0.012f, 0.7f), Rgba.White.WithAlpha(0.6f));
            return new DoodleArt("s41l_bar", p);
        }

        /// <summary>Piastrella smossa (0.8 x 0.8, centro 0,0) con una crepa.</summary>
        public static DoodleArt S41L_TileLoose()
        {
            var p = new DoodlePainter(-0.46f, -0.46f, 0.46f, 0.46f, 120) { Ink = 0.04f };
            p.Shape(Sd.Box(0, 0, 0.38f, 0.38f, 0.03f), Rgba.Hex("B8B3A6"));
            p.Fill(Sd.Box(0.05f, -0.05f, 0.3f, 0.3f), Rgba.Hex("A8A396"));
            p.InkLine(0.025f, -0.3f, 0.25f, -0.1f, 0.08f, -0.18f, -0.05f, 0.05f, -0.25f);
            p.Fill(Sd.Box(0, -0.4f, 0.38f, 0.035f), Rgba.Hex("2B2233").WithAlpha(0.6f));
            return new DoodleArt("s41l_tile_loose", p);
        }

        /// <summary>Buco del tunnel sotto la piastrella (centro 0,0).</summary>
        public static DoodleArt S41L_Hole()
        {
            var p = new DoodlePainter(-0.46f, -0.46f, 0.46f, 0.46f, 120) { Ink = 0.04f };
            p.Shape(Sd.Box(0, 0, 0.38f, 0.38f, 0.03f), Rgba.Hex("6B4A2E"));
            p.Fill(Sd.Ellipse(0, -0.04f, 0.28f, 0.26f), Rgba.Hex("120C10"));
            p.Fill(Sd.Ellipse(0, 0.18f, 0.22f, 0.06f), Rgba.Hex("8A6340").WithAlpha(0.7f));
            return new DoodleArt("s41l_hole", p);
        }

        /// <summary>Cucchiaino (centro 0,0, coppa a destra).</summary>
        public static DoodleArt S41L_Spoon()
        {
            var p = new DoodlePainter(-0.55f, -0.2f, 0.55f, 0.2f, 160) { Ink = 0.035f };
            p.Shape(Sd.Capsule(-0.48f, 0, 0.15f, 0, 0.045f), S41L_Steel);
            Body(p, Sd.Ellipse(0.32f, 0, 0.2f, 0.13f), S41L_Steel, S41L_SteelShade);
            p.Fill(Sd.Ellipse(0.27f, 0.04f, 0.08f, 0.04f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("s41l_spoon", p);
        }

        /// <summary>Ciotola del cibo del topo con briciole di formaggio (base in 0,0).</summary>
        public static DoodleArt S41L_Bowl()
        {
            var p = new DoodlePainter(-0.48f, -0.08f, 0.48f, 0.4f, 140) { Ink = 0.04f };
            p.Shape(Sd.Ellipse(0, 0.25f, 0.36f, 0.08f), Rgba.Hex("FFD25A"));
            p.Shape(Sd.Box(-0.08f, 0.3f, 0.06f, 0.05f, 0.02f), Rgba.Hex("FFE48A"), 0.02f);
            p.Shape(Sd.Box(0.1f, 0.29f, 0.05f, 0.04f, 0.02f), Rgba.Hex("FFE48A"), 0.02f);
            Body(p, Sd.Intersect(Sd.Ellipse(0, 0.22f, 0.42f, 0.24f), Sd.Box(0, 0.05f, 0.5f, 0.17f)), Rgba.Hex("E8433F"), Rgba.Hex("B92E2A"));
            p.Line(Rgba.White, 0.03f, -0.2f, 0.12f, 0.2f, 0.12f);
            return new DoodleArt("s41l_bowl", p);
        }

        /// <summary>Gatto carceriere seduto col berretto da guardia (piedi in y 0); sveglio o addormentato.</summary>
        public static DoodleArt S41L_Jailer(bool asleep)
        {
            var p = new DoodlePainter(-0.85f, -0.08f, 0.85f, 2.05f, 110) { Ink = 0.05f };
            Rgba fur = Rgba.Hex("8C8F9E"), furShade = Rgba.Hex("6B6E7D");
            // coda
            p.Shape(Sd.Capsule(0.45f, 0.15f, 0.75f, 0.7f, 0.08f), fur);
            // corpo
            Body(p, Sd.Ellipse(0, 0.55f, 0.52f, 0.55f), fur, furShade);
            p.Shape(Sd.Ellipse(0, 0.5f, 0.28f, 0.35f), Rgba.Hex("D8DAE2"), 0.03f);
            // divisa: cravatta
            p.Shape(Sd.Polygon(-0.07f, 0.95f, 0.07f, 0.95f, 0.05f, 0.6f, 0, 0.55f, -0.05f, 0.6f), Rgba.Hex("2E4A8C"), 0.025f);
            // zampe
            p.Shape(Sd.Ellipse(-0.22f, 0.07f, 0.16f, 0.08f), fur, 0.035f);
            p.Shape(Sd.Ellipse(0.22f, 0.07f, 0.16f, 0.08f), fur, 0.035f);
            // orecchie
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Polygon(s * 0.42f, 1.35f, s * 0.12f, 1.55f, s * 0.38f, 1.78f), fur);
                p.Fill(Sd.Polygon(s * 0.37f, 1.42f, s * 0.2f, 1.55f, s * 0.35f, 1.68f), Rgba.Hex("FF9BB0"));
            }
            // testa
            Body(p, Sd.Ellipse(0, 1.3f, 0.48f, 0.38f), fur, furShade);
            // berretto da guardia
            p.Shape(Sd.Box(0, 1.68f, 0.36f, 0.14f, 0.06f), Rgba.Hex("2E4A8C"));
            p.Shape(Sd.Ellipse(0.08f, 1.55f, 0.4f, 0.06f), Rgba.Hex("1E2E5C"), 0.03f);
            p.Shape(Sd.Circle(0, 1.7f, 0.06f), Gold, 0.02f);
            // muso
            p.Fill(Sd.Polygon(-0.05f, 1.22f, 0.05f, 1.22f, 0, 1.16f), Rgba.Hex("FF7F9C"));
            p.InkLine(0.025f, -0.1f, 1.1f, 0, 1.15f, 0.1f, 1.1f);
            foreach (float s in new[] { -1f, 1f })
            {
                p.InkLine(0.015f, s * 0.15f, 1.18f, s * 0.48f, 1.22f);
                p.InkLine(0.015f, s * 0.15f, 1.14f, s * 0.46f, 1.1f);
                float ex = s * 0.18f;
                if (asleep) p.InkLine(0.03f, ex - 0.08f, 1.36f, ex, 1.32f, ex + 0.08f, 1.36f);
                else
                {
                    p.Shape(Sd.Ellipse(ex, 1.36f, 0.09f, 0.1f), Rgba.Hex("D8F05A"), 0.025f);
                    p.Fill(Sd.Ellipse(ex - 0.03f, 1.36f, 0.025f, 0.07f), Ink);
                    p.InkLine(0.03f, ex - 0.1f, 1.5f + s * 0.02f, ex + 0.1f, 1.48f - s * 0.02f);
                }
            }
            return new DoodleArt(asleep ? "s41l_jailer_sleep" : "s41l_jailer", p);
        }

        /// <summary>Scrivania della guardia (piano in y 0, larga 1.6) con la targhetta.</summary>
        public static DoodleArt S41L_Desk()
        {
            var p = new DoodlePainter(-0.88f, -1.02f, 0.88f, 0.1f, 100) { Ink = 0.05f };
            Body(p, Sd.Box(0, -0.5f, 0.78f, 0.47f, 0.04f), Wood, WoodShade);
            p.Shape(Sd.Box(0, 0, 0.85f, 0.07f, 0.03f), WoodShade);
            p.Shape(Sd.Box(0, -0.45f, 0.35f, 0.13f, 0.03f), Gold, 0.03f);
            p.Line(Ink, 0.025f, -0.22f, -0.45f, 0.22f, -0.45f);
            return new DoodleArt("s41l_desk", p);
        }

        /// <summary>Cono di vista del gatto (vertice in 0,0, si apre verso sinistra-basso), giallo trasparente.</summary>
        public static DoodleArt S41L_Cone()
        {
            var p = new DoodlePainter(-2.6f, -2.9f, 0.05f, 0.4f, 40) { Wobble = 0f };
            p.FillFunc(Sd.Polygon(0, 0, -2.5f, 0.3f, -2.5f, -2.8f, -0.6f, -2.8f), (x, y) => Rgba.Hex("FFF07A").WithAlpha(0.32f * S41L_Clamp(1.2f - (-x) / 3f)));
            return new DoodleArt("s41l_cone", p);
        }

        /// <summary>Megafono (bocca a destra, centro 0,0).</summary>
        public static DoodleArt S41L_Megaphone()
        {
            var p = new DoodlePainter(-0.45f, -0.3f, 0.45f, 0.3f, 150) { Ink = 0.035f };
            p.Shape(Sd.Box(-0.18f, -0.15f, 0.04f, 0.1f, 0.02f), Rgba.Hex("3A3550"));
            Body(p, Sd.Polygon(-0.35f, 0.08f, 0.35f, 0.24f, 0.35f, -0.24f, -0.35f, -0.08f), Rgba.Hex("FF8A1F"), Rgba.Hex("E0600B"));
            p.Shape(Sd.Ellipse(0.36f, 0, 0.06f, 0.24f), Rgba.White, 0.03f);
            return new DoodleArt("s41l_megaphone", p);
        }

        /// <summary>Cartello da protesta su un bastoncino (pannello 0.9 x 0.36 centrato in 0,0; il testo è a parte).</summary>
        public static DoodleArt S41L_Sign()
        {
            var p = new DoodlePainter(-0.5f, -0.75f, 0.5f, 0.24f, 120) { Ink = 0.035f };
            p.Shape(Sd.Box(0, -0.45f, 0.03f, 0.28f), Wood);
            p.Shape(Sd.Box(0, 0, 0.45f, 0.18f, 0.03f), Rgba.Hex("FFF8E6"));
            return new DoodleArt("s41l_sign", p);
        }

        /// <summary>Contratto con righe e spazio per la firma (centro 0,0, 1.4 x 1.8).</summary>
        public static DoodleArt S41L_Contract()
        {
            var p = new DoodlePainter(-0.78f, -0.98f, 0.78f, 0.98f, 100) { Ink = 0.04f };
            p.Shape(Sd.Box(0, 0, 0.7f, 0.9f, 0.04f), Rgba.Hex("FFFBEE"));
            for (int i = 0; i < 5; i++) p.Line(Rgba.Hex("B7BCC9"), 0.025f, -0.55f, -0.05f - i * 0.12f, 0.55f - (i % 2) * 0.2f, -0.05f - i * 0.12f);
            p.Line(Ink, 0.02f, 0.05f, -0.72f, 0.6f, -0.72f);
            p.Shape(Sd.Circle(-0.45f, -0.72f, 0.12f), Rgba.Hex("E8433F"), 0.03f);
            return new DoodleArt("s41l_contract", p);
        }

        /// <summary>Porta principale aperta (stesse misure di quella nello sfondo): vano luminoso.</summary>
        public static DoodleArt S41L_DoorOpen()
        {
            var p = new DoodlePainter(-0.47f, -1.52f, 0.47f, 1.52f, 100) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 0.4f, 1.45f, 0.08f), (x, y) => Rgba.Lerp(Rgba.Hex("FFF8D8"), Rgba.Hex("FFD86A"), S41L_Clamp(Math.Abs(x) * 2f)));
            p.Shape(Sd.Box(0, 0, 0.4f, 1.45f, 0.08f), Rgba.Clear, 0.08f);
            p.Shape(Sd.Polygon(-0.4f, 1.4f, -0.2f, 1.2f, -0.2f, -1.2f, -0.4f, -1.4f), Wood, 0.03f);
            return new DoodleArt("s41l_door_open", p);
        }

        // ================= THE IMPOSSIBLE SHOT =================

        /// <summary>Campetto visto dall'alto: parquet, bordi imbottiti ai lati (rimbalzo), righe del campo.</summary>
        public static DoodleArt S41L_CourtBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) =>
            {
                int plank = (int)Math.Floor((x + 10f) / 0.4f);
                float k = 0.5f + 0.25f * S41L_Sin(plank * 1.7f) + 0.1f * S41L_Sin(y * 2f + plank);
                return Rgba.Lerp(Rgba.Hex("E9B477"), Rgba.Hex("D69A5C"), S41L_Clamp(k));
            });
            for (int i = -8; i <= 8; i++) p.Line(Rgba.Hex("B9824A").WithAlpha(0.5f), 0.015f, i * 0.4f, -5.3f, i * 0.4f, 5.3f);
            p.Line(Rgba.White.WithAlpha(0.75f), 0.05f, -2.4f, -1.2f, 2.4f, -1.2f);
            p.Shape(Sd.Subtract(Sd.Circle(1.5f, -3.6f, 0.75f), Sd.Circle(1.5f, -3.6f, 0.7f)), Rgba.White.WithAlpha(0.7f), 0.0f);
            // sponde blu imbottite a sinistra e a destra, in alto e in basso
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Box(s * 2.75f, 0, 0.35f, 5.5f), Rgba.Hex("4F7BD8"));
                for (int i = -5; i <= 5; i++) p.Fill(Sd.Circle(s * 2.55f, i * 0.9f, 0.04f), Rgba.Hex("A8C4FF"));
            }
            p.Shape(Sd.Box(0, 3.25f, 3.3f, 0.35f), Rgba.Hex("4F7BD8"));
            p.Shape(Sd.Box(0, -4.55f, 3.3f, 0.35f), Rgba.Hex("4F7BD8"));
            return new DoodleArt("s41l_court_bg", p);
        }

        /// <summary>Muretto di mattoni 3.1 x 0.24 (centro 0,0).</summary>
        public static DoodleArt S41L_BrickWall()
        {
            var p = new DoodlePainter(-1.62f, -0.18f, 1.62f, 0.18f, 100) { Ink = 0.04f };
            Sdf wall = Sd.Box(0, 0, 1.55f, 0.12f, 0.03f);
            p.FillFunc(wall, (x, y) =>
            {
                int row = y > 0 ? 1 : 0;
                float bx = (x + 10f + row * 0.15f) / 0.3f;
                return bx - (float)Math.Floor(bx) < 0.08f || Math.Abs(y) < 0.012f ? Rgba.Hex("E8D8C8") : Rgba.Hex("C8553D");
            });
            p.Shape(wall, Rgba.Clear);
            return new DoodleArt("s41l_brick_wall", p);
        }

        /// <summary>Canestro (cesto di vimini) visto dall'alto, raggio 0.55.</summary>
        public static DoodleArt S41L_BasketTop()
        {
            var p = new DoodlePainter(-0.65f, -0.65f, 0.65f, 0.65f, 120) { Ink = 0.05f };
            p.FillFunc(Sd.Circle(0, 0, 0.55f), (x, y) =>
            {
                double a = Math.Atan2(y, x);
                float r = (float)Math.Sqrt(x * x + y * y);
                return Rgba.Lerp(Rgba.Hex("C98B4A"), Rgba.Hex("8A5A2E"), S41L_Clamp(0.5f + 0.5f * (float)Math.Sin(a * 14 + r * 10)));
            });
            p.Fill(Sd.Circle(0, 0, 0.36f), Rgba.Hex("3A2416"));
            p.Shape(Sd.Subtract(Sd.Circle(0, 0, 0.55f), Sd.Circle(0, 0, 0.36f)), Rgba.Clear, 0.04f);
            return new DoodleArt("s41l_basket_top", p);
        }

        /// <summary>Pallino bianco per la traiettoria (centro 0,0).</summary>
        public static DoodleArt S41L_Dot()
        {
            var p = new DoodlePainter(-0.08f, -0.08f, 0.08f, 0.08f, 200) { Ink = 0.02f, Wobble = 0f };
            p.Shape(Sd.Circle(0, 0, 0.05f), Rgba.White);
            return new DoodleArt("s41l_dot", p);
        }

        // ================= THE ENDLESS STAIRCASE =================

        /// <summary>Cielo con nuvole (la scala sale verso l'alto a destra).</summary>
        public static DoodleArt S41L_TowerBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("BFE6FF"), Rgba.Hex("7CC0F2"), S41L_Clamp((y + 5f) / 10f)));
            foreach (var c in new[] { (-1.8f, 2.2f, 0.6f), (1.6f, 0.4f, 0.5f), (-1.2f, -1.6f, 0.45f), (2.0f, -3.4f, 0.5f) })
                p.Shape(Sd.Union(Sd.Circle(c.Item1, c.Item2, c.Item3 * 0.55f), Sd.Circle(c.Item1 - c.Item3 * 0.6f, c.Item2 - 0.1f, c.Item3 * 0.4f), Sd.Circle(c.Item1 + c.Item3 * 0.6f, c.Item2 - 0.1f, c.Item3 * 0.42f)), Rgba.White, 0.04f);
            return new DoodleArt("s41l_tower_bg", p);
        }

        /// <summary>Gradino di pietra 0.6 x 0.4 (angolo in alto a sinistra in 0,0) con un pilastro sotto.</summary>
        public static DoodleArt S41L_Step()
        {
            var p = new DoodlePainter(-0.05f, -2.6f, 0.65f, 0.05f, 80) { Ink = 0.04f };
            p.Shape(Sd.Box(0.3f, -1.4f, 0.22f, 1.2f), Rgba.Hex("B7A890"));
            Body(p, Sd.Box(0.3f, -0.2f, 0.3f, 0.2f, 0.03f), Rgba.Hex("E8DCC4"), Rgba.Hex("C9B99A"));
            return new DoodleArt("s41l_step", p);
        }

        /// <summary>Cartello "TOP" con la bandierina (centro del pannello in 0,0; il testo è a parte).</summary>
        public static DoodleArt S41L_TopSign()
        {
            var p = new DoodlePainter(-0.6f, -0.35f, 0.6f, 0.75f, 120) { Ink = 0.04f };
            p.Shape(Sd.Box(0.4f, 0.4f, 0.025f, 0.35f), WoodDark, 0.025f);
            p.Shape(Sd.Polygon(0.42f, 0.72f, 0.42f, 0.42f, 0.0f, 0.57f), Rgba.Hex("E8433F"), 0.03f);
            p.Shape(Sd.Box(0, 0, 0.55f, 0.28f, 0.08f), Gold);
            return new DoodleArt("s41l_top_sign", p);
        }

        /// <summary>Omino scalatore con lo zaino (piedi in 0,0, guarda a destra).</summary>
        public static DoodleArt S41L_Climber()
        {
            var p = new DoodlePainter(-0.45f, -0.06f, 0.45f, 1.25f, 140) { Ink = 0.045f };
            p.Shape(Sd.Capsule(-0.08f, 0.08f, -0.05f, 0.42f, 0.06f), Rgba.Hex("3A5FB0"));
            p.Shape(Sd.Capsule(0.1f, 0.08f, 0.06f, 0.42f, 0.06f), Rgba.Hex("3A5FB0"));
            p.Shape(Sd.Ellipse(-0.06f, 0.05f, 0.11f, 0.05f), Rgba.Hex("5A3A2A"));
            p.Shape(Sd.Ellipse(0.14f, 0.05f, 0.11f, 0.05f), Rgba.Hex("5A3A2A"));
            p.Shape(Sd.Box(-0.22f, 0.65f, 0.12f, 0.2f, 0.06f), Rgba.Hex("6CBF4A"));
            Body(p, Sd.Box(0, 0.62f, 0.18f, 0.24f, 0.08f), Rgba.Hex("E8433F"), Rgba.Hex("B92E2A"));
            p.Shape(Sd.Capsule(0.1f, 0.72f, 0.3f, 0.55f, 0.05f), Skin);
            Body(p, Sd.Circle(0.02f, 1.0f, 0.2f), Skin, SkinShade);
            p.Shape(Sd.Intersect(Sd.Circle(0.02f, 1.02f, 0.22f), Sd.Box(0.02f, 1.18f, 0.25f, 0.1f)), Rgba.Hex("FFC83A"), 0.03f);
            p.Fill(Sd.Circle(0.1f, 1.0f, 0.03f), Ink);
            p.InkLine(0.025f, 0.05f, 0.9f, 0.12f, 0.88f, 0.17f, 0.92f);
            return new DoodleArt("s41l_climber", p);
        }

        /// <summary>Vetta: piattaforma con bandierona (piano in y 0).</summary>
        public static DoodleArt S41L_Summit()
        {
            var p = new DoodlePainter(-0.75f, -0.35f, 0.75f, 1.5f, 100) { Ink = 0.045f };
            p.Shape(Sd.Box(0.3f, 0.7f, 0.03f, 0.72f), WoodDark, 0.025f);
            p.Shape(Sd.Polygon(0.33f, 1.4f, 0.33f, 0.95f, 0.75f, 1.17f), Rgba.Hex("4CBF5A"), 0.03f);
            Body(p, Sd.Box(0, -0.15f, 0.7f, 0.15f, 0.05f), Rgba.Hex("E8DCC4"), Rgba.Hex("C9B99A"));
            return new DoodleArt("s41l_summit", p);
        }

        // ================= ALMOST THERE =================

        /// <summary>Sala giochi scura con luci al neon.</summary>
        public static DoodleArt S41L_ArcadeBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("2A1F4A"), Rgba.Hex("120C24"), S41L_Clamp((y + 5f) / 10f)));
            p.FillFunc(Sd.Box(0, -4.0f, 3.3f, 1.3f), (x, y) => ((int)Math.Floor((x + 10f) * 2f) + (int)Math.Floor((y + 10f) * 2f)) % 2 == 0 ? Rgba.Hex("3A2D60") : Rgba.Hex("241A40"));
            p.Line(NeonMagenta, 0.06f, -3.2f, 3.2f, 3.2f, 3.2f);
            p.Line(NeonCyan, 0.06f, -3.2f, -2.7f, 3.2f, -2.7f);
            for (int i = 0; i < 6; i++) p.Fill(Sd.Circle(-2.5f + i, 3.6f, 0.08f), (i % 2 == 0 ? NeonYellow : NeonCyan));
            return new DoodleArt("s41l_arcade_bg", p);
        }

        /// <summary>Tabellone del punteggio (4.8 x 2.2, centro 0,0; le cifre sono a parte).</summary>
        public static DoodleArt S41L_Scoreboard()
        {
            var p = new DoodlePainter(-2.55f, -1.25f, 2.55f, 1.25f, 80) { Ink = 0.06f };
            p.Shape(Sd.Box(0, 0, 2.45f, 1.15f, 0.2f), Rgba.Hex("3A3550"));
            p.Fill(Sd.Box(0, -0.05f, 2.2f, 0.9f, 0.12f), Rgba.Hex("0E0B18"));
            for (int i = 0; i < 12; i++)
            {
                float a = i / 12f * 6.283f;
                p.Fill(Sd.Circle(S41L_Cos(a) * 2.3f, S41L_Sin(a) * 1.02f, 0.06f), i % 2 == 0 ? NeonYellow : NeonMagenta);
            }
            return new DoodleArt("s41l_scoreboard", p);
        }

        /// <summary>Il puntino nascosto: un dischetto dorato luccicante (centro 0,0).</summary>
        public static DoodleArt S41L_HiddenDot()
        {
            var p = new DoodlePainter(-0.2f, -0.2f, 0.2f, 0.2f, 200) { Ink = 0.03f };
            p.Fill(Sd.Circle(0, 0, 0.18f), Rgba.Hex("FFF3B0").WithAlpha(0.4f));
            Body(p, Sd.Circle(0, 0, 0.1f), Gold, GoldShade);
            p.Fill(Sd.Circle(-0.03f, 0.03f, 0.025f), Rgba.White);
            return new DoodleArt("s41l_hidden_dot", p);
        }

        // ================= TONI: X-RAY =================

        /// <summary>Scheletro di Toni sdraiato visto ai raggi X (stesse coordinate di "toni_lie"), su fondo blu scuro.</summary>
        public static DoodleArt S41L_XrayBones()
        {
            var p = new DoodlePainter(-1.25f, -0.45f, 1.2f, 0.85f, 130) { Ink = 0.03f, InkColor = Rgba.Hex("6FB8FF") };
            Rgba bg = Rgba.Hex("0E2A4A"), bone = Rgba.Hex("E8F6FF");
            p.Fill(Sd.Union(Sd.Ellipse(0.15f, 0.08f, 0.95f, 0.47f), Sd.Ellipse(-0.72f, 0.38f, 0.46f, 0.42f)), bg);
            p.Line(Rgba.Hex("2E6A9A"), 0.02f, -0.6f, 0.1f, -0.2f, 0.05f);
            // cranio
            p.Shape(Sd.Subtract(Sd.Ellipse(-0.72f, 0.42f, 0.34f, 0.3f), Sd.Union(Sd.Circle(-0.86f, 0.46f, 0.08f), Sd.Circle(-0.56f, 0.46f, 0.08f))), bone);
            p.Shape(Sd.Box(-0.72f, 0.15f, 0.16f, 0.06f, 0.04f), bone);
            // colonna e costole
            for (int i = 0; i < 9; i++) p.Shape(Sd.Ellipse(-0.38f + i * 0.15f, 0.3f - i * 0.005f, 0.055f, 0.04f), bone, 0.02f);
            for (int i = 0; i < 5; i++)
            {
                float x = -0.15f + i * 0.17f;
                p.Line(bone, 0.035f, x, 0.28f, x + 0.04f, 0.1f, x, -0.12f);
            }
            // zampe
            p.Line(bone, 0.05f, -0.4f, -0.05f, -0.55f, -0.25f);
            p.Line(bone, 0.05f, 0.75f, 0.0f, 0.85f, -0.22f);
            p.Line(bone, 0.04f, 0.98f, 0.2f, 1.08f, 0.32f);
            // la gola (tubicino) verso la bocca
            p.Line(Rgba.Hex("6FB8FF").WithAlpha(0.6f), 0.02f, -0.62f, 0.2f, -0.35f, 0.12f, 0.0f, 0.02f);
            p.Line(Rgba.Hex("6FB8FF").WithAlpha(0.6f), 0.02f, -0.7f, 0.06f, -0.38f, -0.02f, -0.05f, -0.1f);
            return new DoodleArt("s41l_xray_bones", p);
        }

        /// <summary>Cornice dello scanner (finestra 1.5 x 1.2 centrata in 0,0) con la maniglia sotto.</summary>
        public static DoodleArt S41L_Scanner()
        {
            var p = new DoodlePainter(-0.95f, -1.05f, 0.95f, 0.8f, 110) { Ink = 0.05f };
            p.Shape(Sd.Box(0, -0.85f, 0.18f, 0.18f, 0.05f), Rgba.Hex("3A3550"));
            p.Shape(Sd.Subtract(Sd.Box(0, 0, 0.88f, 0.72f, 0.12f), Sd.Box(0, 0, 0.75f, 0.6f, 0.06f)), Rgba.Hex("5BC4E8"));
            p.Fill(Sd.Circle(0.68f, 0.62f, 0.04f), Rgba.Hex("FF4A5A"));
            p.Line(Rgba.White.WithAlpha(0.6f), 0.03f, -0.8f, 0.55f, -0.8f, -0.4f);
            return new DoodleArt("s41l_scanner", p);
        }

        /// <summary>Frigorifero con catena (base in y 0, alto 2.4).</summary>
        public static DoodleArt S41L_Fridge()
        {
            var p = new DoodlePainter(-0.7f, -0.05f, 0.7f, 2.5f, 90) { Ink = 0.05f };
            Body(p, Sd.Box(0, 1.2f, 0.62f, 1.18f, 0.12f), Rgba.Hex("F2F6FA"), Rgba.Hex("C9D4E0"));
            p.Line(Ink, 0.04f, -0.6f, 1.6f, 0.6f, 1.6f);
            p.Shape(Sd.Box(-0.45f, 2.0f, 0.04f, 0.2f, 0.03f), Rgba.Hex("9AA0AE"), 0.025f);
            p.Shape(Sd.Box(-0.45f, 1.1f, 0.04f, 0.3f, 0.03f), Rgba.Hex("9AA0AE"), 0.025f);
            // catena a X davanti
            for (int i = 0; i < 9; i++)
            {
                float k = i / 8f;
                p.Shape(Sd.Ellipse(-0.55f + k * 1.1f, 0.7f + k * 0.9f, 0.08f, 0.05f), Rgba.Hex("8D93A6"), 0.02f);
                p.Shape(Sd.Ellipse(-0.55f + k * 1.1f, 1.6f - k * 0.9f, 0.08f, 0.05f), Rgba.Hex("8D93A6"), 0.02f);
            }
            return new DoodleArt("s41l_fridge", p);
        }

        /// <summary>Lucchetto (centro 0,0).</summary>
        public static DoodleArt S41L_Padlock()
        {
            var p = new DoodlePainter(-0.3f, -0.3f, 0.3f, 0.36f, 150) { Ink = 0.035f };
            p.Shape(Sd.Subtract(Sd.Circle(0, 0.08f, 0.17f), Sd.Union(Sd.Circle(0, 0.08f, 0.1f), Sd.Box(0, -0.05f, 0.2f, 0.13f))), Rgba.Hex("B7BCC9"));
            Body(p, Sd.Box(0, -0.1f, 0.22f, 0.17f, 0.05f), Gold, GoldShade);
            p.Fill(Sd.Circle(0, -0.07f, 0.04f), Ink);
            return new DoodleArt("s41l_padlock", p);
        }

        // ================= TONI: BEAUTY FILTER =================

        /// <summary>Testona di Toni di fronte, senza occhi, orecchie e muso (si montano a parte). Centro 0,0, raggio ~1.</summary>
        public static DoodleArt S41L_BfHead()
        {
            var p = new DoodlePainter(-1.15f, -1.05f, 1.15f, 1.05f, 90) { Ink = 0.06f };
            Body(p, Sd.Ellipse(0, 0, 1.05f, 0.95f), PugFawn, PugShade, null, -0.1f, 0.1f);
            p.Line(PugLine, 0.05f, -0.35f, 0.6f, -0.15f, 0.68f, 0.05f, 0.6f, 0.25f, 0.68f, 0.4f, 0.6f);
            p.Line(PugLine, 0.04f, -0.25f, 0.45f, 0f, 0.5f, 0.25f, 0.45f);
            p.Shape(Sd.Capsule(-0.5f, -0.85f, 0.5f, -0.85f, 0.1f), Rgba.Hex("E8233F"), 0.04f);
            p.Shape(Sd.Circle(0, -0.95f, 0.12f), Rgba.Hex("FFC83A"), 0.03f);
            return new DoodleArt("s41l_bf_head", p);
        }

        /// <summary>Orecchio ripiegato (attacco in 0,0, punta verso il basso-esterno a sinistra).</summary>
        public static DoodleArt S41L_BfEar()
        {
            var p = new DoodlePainter(-0.55f, -0.45f, 0.15f, 0.2f, 120) { Ink = 0.05f };
            Body(p, Sd.Rotate(Sd.Ellipse(-0.2f, -0.1f, 0.3f, 0.2f), -0.2f, -0.1f, 30f), PugShade, Rgba.Hex("141418"));
            return new DoodleArt("s41l_bf_ear", p);
        }

        /// <summary>Occhio da carlino (centro 0,0): bianco, pupilla grande lucida.</summary>
        public static DoodleArt S41L_BfEye()
        {
            var p = new DoodlePainter(-0.28f, -0.28f, 0.28f, 0.28f, 160) { Ink = 0.04f, InkColor = Rgba.Hex("141418") };
            p.Shape(Sd.Circle(0, 0, 0.22f), Rgba.Hex("FFFDF5"));
            p.Fill(Sd.Circle(0.02f, -0.02f, 0.13f), Rgba.Hex("2A1A12"));
            p.Fill(Sd.Circle(-0.03f, 0.04f, 0.04f), Rgba.White);
            return new DoodleArt("s41l_bf_eye", p);
        }

        /// <summary>Muso schiacciato col naso e la boccuccia (centro 0,0).</summary>
        public static DoodleArt S41L_BfSnout()
        {
            var p = new DoodlePainter(-0.5f, -0.4f, 0.5f, 0.38f, 140) { Ink = 0.04f, InkColor = Rgba.Hex("141418") };
            p.Shape(Sd.Ellipse(0, 0, 0.45f, 0.33f), PugMask);
            p.Fill(Sd.Ellipse(0, 0.12f, 0.18f, 0.1f), Rgba.Hex("0E0A0A"));
            p.Fill(Sd.Circle(-0.06f, 0.16f, 0.03f), Rgba.White.WithAlpha(0.6f));
            p.Line(PugLine, 0.035f, 0, 0.02f, 0, -0.1f);
            p.Line(PugLine, 0.035f, -0.2f, -0.15f, 0, -0.1f, 0.2f, -0.15f);
            p.Fill(Sd.Ellipse(0.04f, -0.24f, 0.08f, 0.06f), Rgba.Hex("FF7A93"));
            return new DoodleArt("s41l_bf_snout", p);
        }

        /// <summary>Pannello degli effetti (3.6 x 1.6, centro 0,0): rosa con le scintille; i pulsanti sono a parte.</summary>
        public static DoodleArt S41L_BfPanel()
        {
            var p = new DoodlePainter(-1.9f, -0.88f, 1.9f, 0.88f, 80) { Ink = 0.06f };
            p.Shape(Sd.Box(0, 0, 1.8f, 0.8f, 0.2f), Rgba.Hex("FFB3D1"));
            p.Fill(Sd.Box(0, 0.6f, 1.7f, 0.1f, 0.06f), Rgba.Hex("FF8FBE"));
            for (int i = 0; i < 6; i++) p.Fill(Sd.Circle(-1.5f + i * 0.6f, 0.6f, 0.04f), Rgba.White);
            return new DoodleArt("s41l_bf_panel", p);
        }

        /// <summary>Antennina da alieno (base in 0,0).</summary>
        public static DoodleArt S41L_Antenna()
        {
            var p = new DoodlePainter(-0.2f, -0.05f, 0.2f, 0.75f, 140) { Ink = 0.035f };
            p.Shape(Sd.Capsule(0, 0, 0.03f, 0.55f, 0.03f), Rgba.Hex("7ED957"));
            Body(p, Sd.Circle(0.03f, 0.6f, 0.11f), Rgba.Hex("D6FF5A"), Rgba.Hex("9ED93A"));
            return new DoodleArt("s41l_antenna", p);
        }

        // ================= TONI: PERSONAL TRAINER =================

        /// <summary>Ostacolo a strisce (piedi in y 0, alto 0.55, largo 0.8).</summary>
        public static DoodleArt S41L_Hurdle()
        {
            var p = new DoodlePainter(-0.48f, -0.06f, 0.48f, 0.66f, 130) { Ink = 0.04f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Box(s * 0.36f, 0.27f, 0.035f, 0.27f), Rgba.Hex("9AA0AE"), 0.03f);
                p.Shape(Sd.Box(s * 0.36f, 0.02f, 0.1f, 0.03f), Rgba.Hex("6F7586"), 0.025f);
            }
            Sdf bar = Sd.Box(0, 0.5f, 0.42f, 0.07f, 0.03f);
            p.FillFunc(bar, (x, y) => (int)Math.Floor((x + 10f) / 0.14f) % 2 == 0 ? Rgba.Hex("E8433F") : Rgba.White);
            p.Shape(bar, Rgba.Clear, 0.035f);
            return new DoodleArt("s41l_hurdle", p);
        }

        /// <summary>Pneumatico in piedi (base in y 0, raggio 0.3).</summary>
        public static DoodleArt S41L_Tire()
        {
            var p = new DoodlePainter(-0.4f, -0.06f, 0.4f, 0.66f, 130) { Ink = 0.04f };
            p.Shape(Sd.Subtract(Sd.Ellipse(0, 0.3f, 0.34f, 0.3f), Sd.Ellipse(0, 0.3f, 0.15f, 0.13f)), Rgba.Hex("3A3540"));
            p.Line(Rgba.Hex("6E6878"), 0.025f, -0.22f, 0.45f, -0.12f, 0.53f);
            p.Line(Rgba.Hex("6E6878"), 0.025f, 0.22f, 0.15f, 0.12f, 0.07f);
            return new DoodleArt("s41l_tire", p);
        }

        /// <summary>Arco del traguardo a scacchi (base in y 0, largo 1).</summary>
        public static DoodleArt S41L_Finish()
        {
            var p = new DoodlePainter(-0.6f, -0.05f, 0.6f, 2.0f, 100) { Ink = 0.045f };
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Box(s * 0.48f, 0.9f, 0.05f, 0.9f), Rgba.Hex("9AA0AE"), 0.03f);
            Sdf banner = Sd.Box(0, 1.7f, 0.55f, 0.2f, 0.04f);
            p.FillFunc(banner, (x, y) => ((int)Math.Floor((x + 10f) / 0.1f) + (int)Math.Floor((y + 10f) / 0.1f)) % 2 == 0 ? Rgba.White : Rgba.Hex("2B2233"));
            p.Shape(banner, Rgba.Clear);
            return new DoodleArt("s41l_finish", p);
        }

        // ================= TONI: FINAL EXAM =================

        /// <summary>Aula: lavagna verde in alto, parete crema, pavimento di legno, chiodo per il diploma.</summary>
        public static DoodleArt S41L_ClassBg()
        {
            var p = new DoodlePainter(-3.2f, -5.2f, 3.2f, 5.2f, 50) { Ink = 0.05f };
            p.FillFunc(Sd.Box(0, 1f, 3.3f, 4.3f), (x, y) => (int)Math.Floor((x + 10f) * 2.5f) % 2 == 0 ? Rgba.Hex("FFF1D6") : Rgba.Hex("F7E6C4"));
            p.FillFunc(Sd.Box(0, -4.2f, 3.3f, 1.0f), (x, y) => (int)Math.Floor((y + 10f) * 3f) % 2 == 0 ? Wood : WoodShade);
            p.Line(Ink, 0.05f, -3.3f, -3.2f, 3.3f, -3.2f);
            // lavagna
            p.Shape(Sd.Box(0, 1.55f, 2.65f, 1.0f, 0.06f), WoodShade);
            p.FillFunc(Sd.Box(0, 1.55f, 2.5f, 0.86f, 0.04f), (x, y) => Rgba.Lerp(Rgba.Hex("2F5A46"), Rgba.Hex("264C3A"), S41L_Clamp(0.5f + 0.3f * S41L_Sin(x * 3f + y * 2f))));
            p.Shape(Sd.Box(0, 0.48f, 2.0f, 0.06f, 0.02f), WoodShade, 0.03f);
            p.Shape(Sd.Box(-1.2f, 0.56f, 0.12f, 0.04f, 0.02f), Rgba.White, 0.02f);
            // chiodo del diploma (in alto a destra, sopra la lavagna)
            p.Shape(Sd.Circle(2.0f, 3.0f, 0.06f), Rgba.Hex("9AA0AE"), 0.03f);
            return new DoodleArt("s41l_class_bg", p);
        }

        /// <summary>Professore anziano con barba bianca e camice (piedi in y 0, alto ~2).</summary>
        public static DoodleArt S41L_Professor()
        {
            var p = new DoodlePainter(-0.7f, -0.06f, 0.7f, 2.1f, 110) { Ink = 0.05f };
            p.Shape(Sd.Box(-0.15f, 0.35f, 0.1f, 0.35f, 0.04f), Rgba.Hex("3A3550"));
            p.Shape(Sd.Box(0.15f, 0.35f, 0.1f, 0.35f, 0.04f), Rgba.Hex("3A3550"));
            p.Shape(Sd.Ellipse(-0.18f, 0.04f, 0.16f, 0.06f), Ink);
            p.Shape(Sd.Ellipse(0.18f, 0.04f, 0.16f, 0.06f), Ink);
            Body(p, Sd.Box(0, 1.0f, 0.42f, 0.42f, 0.14f), Rgba.White, Rgba.Hex("D8DCE6"));
            p.Shape(Sd.Polygon(-0.08f, 1.38f, 0.08f, 1.38f, 0, 1.28f), Rgba.Hex("E8433F"), 0.025f);
            p.Shape(Sd.Capsule(-0.4f, 1.25f, -0.55f, 0.75f, 0.08f), Rgba.White);
            p.Shape(Sd.Capsule(0.4f, 1.25f, 0.55f, 0.8f, 0.08f), Rgba.White);
            p.Shape(Sd.Circle(-0.55f, 0.7f, 0.08f), Skin, 0.03f);
            p.Shape(Sd.Circle(0.55f, 0.75f, 0.08f), Skin, 0.03f);
            Body(p, Sd.Ellipse(0, 1.7f, 0.3f, 0.32f), Skin, SkinShade);
            // capelli e barba bianchi
            p.Shape(Sd.Circle(-0.3f, 1.82f, 0.12f), Rgba.White, 0.03f);
            p.Shape(Sd.Circle(0.3f, 1.82f, 0.12f), Rgba.White, 0.03f);
            p.Shape(Sd.Polygon(-0.25f, 1.6f, 0.25f, 1.6f, 0.12f, 1.3f, 0, 1.22f, -0.12f, 1.3f), Rgba.White, 0.03f);
            // occhialini e baffi
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Subtract(Sd.Circle(s * 0.12f, 1.75f, 0.08f), Sd.Circle(s * 0.12f, 1.75f, 0.05f)), Ink, 0f);
                p.Fill(Sd.Circle(s * 0.12f, 1.75f, 0.025f), Ink);
            }
            p.InkLine(0.025f, -0.04f, 1.75f, 0.04f, 1.75f);
            p.Shape(Sd.Ellipse(0, 1.6f, 0.14f, 0.04f), Rgba.Hex("EDEDF2"), 0.025f);
            return new DoodleArt("s41l_professor", p);
        }

        /// <summary>Cattedra del professore (piano in y 0, larga 3).</summary>
        public static DoodleArt S41L_ProfDesk()
        {
            var p = new DoodlePainter(-1.6f, -1.3f, 1.6f, 0.1f, 80) { Ink = 0.05f };
            Body(p, Sd.Box(0, -0.65f, 1.45f, 0.6f, 0.04f), Wood, WoodShade);
            p.Shape(Sd.Box(0, 0, 1.55f, 0.08f, 0.03f), WoodShade);
            p.Shape(Sd.Box(0, -0.6f, 0.5f, 0.25f, 0.04f), WoodDark.WithAlpha(0.5f), 0.03f);
            return new DoodleArt("s41l_prof_desk", p);
        }

        /// <summary>Occhialoni rotondi (centri delle lenti in -0.18 / +0.18 a distanza 0.36: si scalano su Toni).</summary>
        public static DoodleArt S41L_Glasses()
        {
            var p = new DoodlePainter(-0.45f, -0.25f, 0.45f, 0.25f, 200) { Ink = 0f, Wobble = 0f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Circle(s * 0.2f, 0, 0.17f), Rgba.Hex("BFE6FF").WithAlpha(0.35f));
                p.Fill(Sd.Subtract(Sd.Circle(s * 0.2f, 0, 0.19f), Sd.Circle(s * 0.2f, 0, 0.15f)), Rgba.Hex("2B2233"));
                p.Fill(Sd.Circle(s * 0.2f - 0.07f, 0.07f, 0.03f), Rgba.White.WithAlpha(0.8f));
            }
            p.Line(Rgba.Hex("2B2233"), 0.035f, -0.04f, 0.02f, 0, 0.05f, 0.04f, 0.02f);
            return new DoodleArt("s41l_glasses", p);
        }

        /// <summary>Tocco da laureato con la nappina (base in 0,0).</summary>
        public static DoodleArt S41L_Mortarboard()
        {
            var p = new DoodlePainter(-0.5f, -0.06f, 0.5f, 0.5f, 140) { Ink = 0.035f };
            p.Shape(Sd.Box(0, 0.12f, 0.2f, 0.12f, 0.04f), Rgba.Hex("2B2233"), 0.02f);
            p.Shape(Sd.Polygon(-0.45f, 0.3f, 0, 0.45f, 0.45f, 0.3f, 0, 0.17f), Rgba.Hex("3A3550"), 0.03f);
            p.Line(Gold, 0.03f, 0, 0.31f, 0.3f, 0.26f, 0.32f, 0.08f);
            p.Shape(Sd.Box(0.32f, 0.04f, 0.04f, 0.07f, 0.02f), Gold, 0.015f);
            return new DoodleArt("s41l_mortarboard", p);
        }

        /// <summary>Bicchierone capovolto per il test della memoria (base in y 0, alto 0.9).</summary>
        public static DoodleArt S41L_Cup()
        {
            var p = new DoodlePainter(-0.52f, -0.06f, 0.52f, 0.98f, 120) { Ink = 0.045f };
            Body(p, Sd.Polygon(-0.45f, 0, 0.45f, 0, 0.32f, 0.85f, -0.32f, 0.85f), Rgba.Hex("6FB8FF"), Rgba.Hex("3F8AD8"));
            p.Line(Rgba.White, 0.04f, -0.42f, 0.18f, 0.42f, 0.18f);
            p.Fill(Sd.Box(-0.2f, 0.5f, 0.04f, 0.25f), Rgba.White.WithAlpha(0.5f));
            return new DoodleArt("s41l_cup", p);
        }
    }
}
