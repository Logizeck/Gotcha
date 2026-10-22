using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dello speciale Halloween (livelli 245-250).</summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtHalloween() => new DoodleArt[]
        {
            // comuni
            HW_Button(),
            // 245 Toni the were-pug
            HW_NightBg(), HW_Moon(), HW_Werepug(), HW_BiscuitBox(false), HW_BiscuitBox(true), HW_Biscuit(),
            // 246 The perfect pumpkin
            HW_RoomBg(), HW_CeilingLamp(), HW_LampLight(), HW_Table(), HW_Pumpkin(), HW_Knife(), HW_Candle(), HW_Lighter(),
            // 247 The ghost who can't scare
            HW_BedroomBg(), HW_Bed(), HW_ManHead(false), HW_ManHead(true), HW_ManGhost(), HW_Ghost(false), HW_Ghost(true),
            HW_Sheet(), HW_Mirror(), HW_Nightstand(),
            // 248 The witch and the impossible potion
            HW_WitchBg(), HW_Witch(false), HW_Witch(true), HW_WitchToad(), HW_WitchApple(), HW_WitchEyes(), HW_Cauldron(), HW_Brew(),
            HW_Toad(), HW_Eyeball(), HW_Mushroom(), HW_HeartIngredient(), HW_Phone(),
            // 249 The vegetarian zombie
            HW_GraveyardBg(), HW_ZombieBody(), HW_ZombieHead(false), HW_ZombieHead(true), HW_ZombieArm(), HW_Brain(), HW_Burger(),
            HW_Broccoli(), HW_Balloon(), HW_BalloonTail(), HW_Tile(), HW_Undo(),
            // 250 Trick or treat!
            HW_HouseBg(), HW_Door(), HW_Doorway(), HW_Lady(0), HW_Lady(1), HW_Lady(2), HW_Doorbell(), HW_Bucket(), HW_Candy(),
            HW_Chocolate(), HW_PumpkinKid(), HW_BigSheet(), HW_WitchHat(), HW_Crown(),
        };

        // ---------- colori e aiuti comuni ----------

        static readonly Rgba HW_Orange = Rgba.Hex("FF8A1F"), HW_OrangeDark = Rgba.Hex("D9620B"), HW_Purple = Rgba.Hex("6B3FA0"),
            HW_Green = Rgba.Hex("8CCB5E"), HW_GreenDark = Rgba.Hex("5E9A3A"), HW_Linen = Rgba.Hex("F6F5FF"), HW_LinenShade = Rgba.Hex("C9CBEA"),
            HW_SkinW = Rgba.Hex("9BD36B"), HW_SkinWS = Rgba.Hex("6FAE45"), HW_Zombie = Rgba.Hex("A8CF7E"), HW_ZombieShade = Rgba.Hex("7BA552");

        static float HW_Clamp(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
        static float HW_Sin(float v) => (float)Math.Sin(v);
        static float HW_Cos(float v) => (float)Math.Cos(v);

        /// <summary>Occhio tondo con pupilla e riflesso.</summary>
        static void HW_Eye(DoodlePainter p, float x, float y, float r, float lookX = 0f, float lookY = 0f, Rgba? white = null)
        {
            p.Shape(Sd.Circle(x, y, r), white ?? Rgba.White, Math.Max(0.018f, r * 0.28f));
            p.Fill(Sd.Circle(x + lookX * r * 0.35f, y + lookY * r * 0.35f, r * 0.5f), Ink);
            p.Fill(Sd.Circle(x + lookX * r * 0.35f - r * 0.17f, y + lookY * r * 0.35f + r * 0.18f, r * 0.16f), Rgba.White);
        }

        /// <summary>Cielo notturno sfumato con stelline (sfondi 6.4 x 13.2).</summary>
        static void HW_Sky(DoodlePainter p, string top, string bottom, int seed)
        {
            p.FillFunc(Sd.Box(0, 0, 3.3f, 6.7f), (x, y) => Rgba.Lerp(Rgba.Hex(bottom), Rgba.Hex(top), HW_Clamp((y + 2f) / 7f)));
            var rnd = new Random(seed);
            for (int i = 0; i < 40; i++)
            {
                float x = (float)rnd.NextDouble() * 6.2f - 3.1f, y = (float)rnd.NextDouble() * 6f + 0.2f, r = 0.015f + (float)rnd.NextDouble() * 0.03f;
                p.Fill(Sd.Circle(x, y, r), Rgba.Hex("FFF6C8", 0.5f + (float)rnd.NextDouble() * 0.5f), false);
            }
        }

        /// <summary>Cappello a punta da strega (base del cappello in cx, cy).</summary>
        static void HW_HatAt(DoodlePainter p, float cx, float cy, float s, Rgba band, float tilt = 12f)
        {
            Sdf cone = Sd.Rotate(Sd.Polygon(cx - 0.42f * s, cy, cx + 0.42f * s, cy, cx + 0.12f * s, cy + 0.75f * s, cx - 0.18f * s, cy + 1.05f * s), cx, cy, -tilt);
            Body(p, cone, Rgba.Hex("3A2B4A"), Rgba.Hex("241A30"));
            p.Shape(Sd.Rotate(Sd.Box(cx, cy + 0.1f * s, 0.4f * s, 0.07f * s, 0.02f), cx, cy, -tilt), band, 0.025f);
            p.Shape(Sd.Ellipse(cx, cy - 0.02f * s, 0.72f * s, 0.11f * s), Rgba.Hex("3A2B4A"));
        }

        // ================= COMUNI =================

        /// <summary>Pulsante a pillola chiaro (si tinge): 1.5 x 0.6, centro 0,0.</summary>
        public static DoodleArt HW_Button()
        {
            var p = new DoodlePainter(-0.82f, -0.38f, 0.82f, 0.36f, 120) { Ink = 0.045f };
            p.Shape(Sd.Box(0, -0.05f, 0.74f, 0.27f, 0.27f), Rgba.Hex("9C97A8"));
            p.Shape(Sd.Box(0, 0.02f, 0.74f, 0.27f, 0.27f), Rgba.Hex("F4F2F8"));
            p.Fill(Sd.Box(-0.2f, 0.16f, 0.4f, 0.045f, 0.045f), Rgba.White.WithAlpha(0.9f));
            return new DoodleArt("hw_button", p);
        }

        // ================= 245 TONI THE WERE-PUG =================

        /// <summary>Notte di Halloween: colline, alberi spogli, croci e zucche (coordinate del livello, terreno sotto y -2.9).</summary>
        public static DoodleArt HW_NightBg()
        {
            var p = new DoodlePainter(-3.2f, -6.6f, 3.2f, 6.6f, 40) { Ink = 0.06f };
            HW_Sky(p, "1B1240", "4A2A6E", 245);
            // colline lontane
            p.Fill(Sd.Union(Sd.Ellipse(-2.2f, -2.4f, 2.6f, 1.2f), Sd.Ellipse(1.9f, -2.2f, 2.4f, 1.3f)), Rgba.Hex("2E1F4F"));
            // albero spoglio a sinistra
            Rgba bark = Rgba.Hex("1C1430");
            p.Line(bark, 0.22f, -2.4f, -2.6f, -2.35f, -0.6f, -2.0f, 0.6f);
            p.Line(bark, 0.12f, -2.35f, -0.9f, -2.9f, 0.1f);
            p.Line(bark, 0.1f, -2.1f, 0.2f, -1.6f, 0.9f, -1.3f, 1.0f);
            p.Line(bark, 0.08f, -2.0f, 0.6f, -2.2f, 1.2f);
            // terreno
            p.FillFunc(Sd.Union(Sd.Box(0, -5.0f, 3.3f, 2.1f), Sd.Ellipse(0, -2.95f, 3.6f, 0.35f)), (x, y) => Rgba.Lerp(Rgba.Hex("241A3A"), Rgba.Hex("3D2C5C"), HW_Clamp((y + 4.5f) / 1.8f)));
            p.InkLine(0.05f, -3.3f, -2.95f, -1.5f, -2.75f, 0.2f, -2.95f, 1.8f, -2.75f, 3.3f, -2.95f);
            // lapidi e una zucca
            foreach (float x in new[] { -1.5f, 2.3f })
            {
                Body(p, Sd.Union(Sd.Box(x, -2.85f, 0.26f, 0.32f, 0.04f), Sd.Circle(x, -2.55f, 0.26f)), Rgba.Hex("6F6A86"), Rgba.Hex("524D69"));
                p.Line(Rgba.Hex("47425C"), 0.04f, x - 0.12f, -2.6f, x + 0.12f, -2.6f);
            }
            Body(p, Sd.Ellipse(-0.6f, -3.5f, 0.32f, 0.25f), HW_Orange, HW_OrangeDark);
            p.Line(HW_GreenDark, 0.06f, -0.6f, -3.27f, -0.55f, -3.14f);
            p.Fill(Sd.Polygon(-0.72f, -3.45f, -0.62f, -3.45f, -0.67f, -3.36f), Rgba.Hex("FFD84D"));
            p.Fill(Sd.Polygon(-0.58f, -3.45f, -0.48f, -3.45f, -0.53f, -3.36f), Rgba.Hex("FFD84D"));
            // pipistrelli
            foreach (var (bx, by) in new[] { (-0.9f, 3.0f), (0.1f, 2.3f), (2.4f, 3.5f) })
                p.Fill(Sd.Polygon(bx - 0.25f, by + 0.05f, bx - 0.12f, by - 0.02f, bx - 0.05f, by + 0.06f, bx, by - 0.05f, bx + 0.05f, by + 0.06f, bx + 0.12f, by - 0.02f, bx + 0.25f, by + 0.05f, bx, by - 0.12f), Rgba.Hex("120C24"));
            return new DoodleArt("hw_night_bg", p);
        }

        /// <summary>Luna piena gigante (centro 0,0, raggio 0.8).</summary>
        public static DoodleArt HW_Moon()
        {
            var p = new DoodlePainter(-0.92f, -0.92f, 0.92f, 0.92f, 120) { Ink = 0.05f };
            Body(p, Sd.Circle(0, 0, 0.8f), Rgba.Hex("FFF4C2"), Rgba.Hex("F0D98A"), null, -0.1f, 0.1f);
            foreach (var (x, y, r) in new[] { (-0.3f, 0.25f, 0.16f), (0.28f, -0.2f, 0.2f), (0.1f, 0.42f, 0.09f), (-0.25f, -0.38f, 0.1f) })
                p.Shape(Sd.Circle(x, y, r), Rgba.Hex("EBD28A"), 0.025f);
            return new DoodleArt("hw_moon", p);
        }

        /// <summary>
        /// Toni lupo-carlino visto di fronte (piedi in 0,0, alto 2.7): resta un carlino (muso schiacciato, orecchie
        /// ripiegate, collare rosso) ma più grosso, col pelo arruffato, occhi gialli luminosi e due dentini.
        /// Bocca in (0, 1.58).
        /// </summary>
        public static DoodleArt HW_Werepug()
        {
            var p = new DoodlePainter(-1.35f, -0.08f, 1.35f, 2.8f, 110) { Ink = 0.055f };
            Rgba fur = Rgba.Hex("3E3A4C"), furShade = Rgba.Hex("27242F"), furLine = Rgba.Hex("75708A"), belly = Rgba.Hex("57536A");
            // codina arricciata dietro
            p.Shape(Sd.Subtract(Sd.Circle(0.95f, 0.75f, 0.22f), Sd.Circle(0.95f, 0.75f, 0.1f)), fur);
            // zampe posteriori con artigli
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Ellipse(s * 0.45f, 0.22f, 0.3f, 0.22f), fur, furShade);
                for (int k = -1; k <= 1; k++) p.Shape(Sd.Polygon(s * 0.45f + k * 0.12f - 0.04f, 0.04f, s * 0.45f + k * 0.12f + 0.04f, 0.04f, s * 0.45f + k * 0.12f, -0.06f), Rgba.Hex("F1EDE0"), 0.015f);
            }
            // corpo con ciuffi di pelo spettinato
            Sdf body = Sd.Ellipse(0, 0.95f, 0.82f, 0.72f);
            var tufts = new List<Sdf> { body };
            for (int i = 0; i < 9; i++)
            {
                float a = (float)(Math.PI * (0.1 + i * 0.1));
                float cx = HW_Cos(a) * 0.8f, cy = 0.95f + HW_Sin(a) * 0.66f;
                float ox = HW_Cos(a) * 0.2f, oy = HW_Sin(a) * 0.2f;
                tufts.Add(Sd.Polygon(cx - oy * 0.6f, cy + ox * 0.6f, cx + ox, cy + oy, cx + oy * 0.6f, cy - ox * 0.6f));
            }
            Body(p, Sd.Union(tufts.ToArray()), fur, furShade);
            p.Shape(Sd.Ellipse(0, 0.8f, 0.45f, 0.42f), belly, 0.03f);
            for (int i = 0; i < 3; i++) p.Line(furLine, 0.025f, -0.15f + i * 0.15f, 0.6f, -0.12f + i * 0.15f, 0.75f);
            // braccia muscolose con artigli
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Capsule(s * 0.72f, 1.3f, s * 0.98f, 0.62f, 0.17f), fur, furShade);
                for (int k = -1; k <= 1; k++) p.Shape(Sd.Polygon(s * 0.98f + k * 0.08f - 0.03f, 0.5f, s * 0.98f + k * 0.08f + 0.03f, 0.5f, s * (0.98f + k * 0.08f), 0.4f), Rgba.Hex("F1EDE0"), 0.012f);
            }
            // collare rosso con la medaglietta (è sempre Toni!)
            p.Shape(Sd.Box(0, 1.47f, 0.5f, 0.08f, 0.06f), Rgba.Hex("E8233F"), 0.03f);
            p.Shape(Sd.Circle(0, 1.33f, 0.09f), Rgba.Hex("FFC83A"), 0.025f);
            // testa da carlino con orecchie ripiegate e ciuffi
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Rotate(Sd.Ellipse(s * 0.6f, 2.4f, 0.22f, 0.14f), s * 0.6f, 2.4f, s * -35f), furShade, Rgba.Hex("141418"));
                p.Shape(Sd.Polygon(s * 0.66f, 1.7f, s * 0.92f, 1.82f, s * 0.7f, 1.95f), fur, 0.03f);
            }
            Sdf head = Sd.Ellipse(0, 1.95f, 0.7f, 0.58f);
            Body(p, Sd.Union(head, Sd.Polygon(-0.15f, 2.45f, 0f, 2.68f, 0.12f, 2.47f)), fur, furShade, null, -0.06f, 0.08f);
            p.Line(furLine, 0.035f, -0.25f, 2.3f, -0.1f, 2.36f, 0.05f, 2.3f, 0.2f, 2.36f);
            p.Line(furLine, 0.03f, -0.18f, 2.22f, 0f, 2.27f, 0.18f, 2.22f);
            // occhi gialli luminosi
            foreach (float s in new[] { -1f, 1f })
            {
                p.FillFunc(Sd.Circle(s * 0.3f, 2.03f, 0.3f), (x, y) => Rgba.Hex("FFE14D", 0.45f * (float)Math.Exp(-Math.Sqrt((x - s * 0.3f) * (x - s * 0.3f) + (y - 2.03f) * (y - 2.03f)) * 9f)));
                p.Shape(Sd.Circle(s * 0.3f, 2.03f, 0.17f), Rgba.Hex("FFE14D"), 0.03f);
                p.Fill(Sd.Ellipse(s * 0.3f, 2.03f, 0.04f, 0.12f), Rgba.Hex("C2241F"));
                p.Fill(Sd.Circle(s * 0.3f - 0.06f, 2.09f, 0.035f), Rgba.White);
                // sopracciglia arrabbiate
                p.InkLine(0.05f, s * 0.12f, 2.18f, s * 0.45f, 2.27f);
            }
            // muso nero schiacciato, naso, bocca e dentini
            p.Shape(Sd.Ellipse(0, 1.68f, 0.34f, 0.24f), Rgba.Hex("24222C"), 0.03f);
            p.Fill(Sd.Ellipse(0, 1.78f, 0.13f, 0.07f), Rgba.Hex("0E0C10"));
            p.Fill(Sd.Circle(-0.04f, 1.8f, 0.025f), Rgba.White.WithAlpha(0.6f));
            p.Line(furLine, 0.025f, 0f, 1.71f, 0f, 1.62f);
            p.Line(furLine, 0.025f, -0.17f, 1.58f, 0f, 1.62f, 0.17f, 1.58f);
            foreach (float s in new[] { -1f, 1f })
                p.Shape(Sd.Polygon(s * 0.06f, 1.6f, s * 0.14f, 1.59f, s * 0.1f, 1.48f), Rgba.White, 0.012f);
            return new DoodleArt("hw_werepug", p);
        }

        /// <summary>Scatola di biscotti per cani (centro 0,0, 1.2 x 0.9); aperta: i lembi su e i biscotti dentro.</summary>
        public static DoodleArt HW_BiscuitBox(bool open)
        {
            var p = new DoodlePainter(-0.75f, -0.5f, 0.75f, open ? 0.85f : 0.55f, 130) { Ink = 0.045f };
            if (open)
            {
                foreach (float s in new[] { -1f, 1f })
                    p.Shape(Sd.Polygon(s * 0.58f, 0.38f, s * 0.12f, 0.38f, s * 0.2f, 0.75f, s * 0.68f, 0.7f), Rgba.Hex("E9B94F"));
                p.Fill(Sd.Box(0, 0.4f, 0.55f, 0.06f), Rgba.Hex("6B4A22"));
                foreach (float x in new[] { -0.25f, 0.05f, 0.3f }) p.Shape(Sd.Circle(x, 0.45f, 0.13f), Rgba.Hex("D9A05B"), 0.025f);
            }
            Sdf box = Sd.Box(0, -0.05f, 0.6f, 0.42f, 0.05f);
            Body(p, box, Rgba.Hex("E4483E"), Rgba.Hex("B52F2A"));
            if (!open) p.Shape(Sd.Box(0, 0.4f, 0.64f, 0.07f, 0.03f), Rgba.Hex("C93A33"));
            // etichetta con l'osso
            p.Shape(Sd.Box(0, -0.06f, 0.4f, 0.24f, 0.08f), Rgba.Hex("FFF3D6"), 0.03f);
            Sdf bone = Sd.Union(Sd.Capsule(-0.18f, -0.06f, 0.18f, -0.06f, 0.05f), Sd.Circle(-0.2f, -0.01f, 0.05f), Sd.Circle(-0.2f, -0.11f, 0.05f),
                Sd.Circle(0.2f, -0.01f, 0.05f), Sd.Circle(0.2f, -0.11f, 0.05f));
            p.Shape(bone, Rgba.Hex("F2E3C4"), 0.02f);
            return new DoodleArt(open ? "hw_biscuit_box_open" : "hw_biscuit_box", p);
        }

        /// <summary>Biscotto tondo (centro 0,0, raggio 0.3): alto risoluzione perché si ingrandisce fino alla luna.</summary>
        public static DoodleArt HW_Biscuit()
        {
            var p = new DoodlePainter(-0.34f, -0.34f, 0.34f, 0.34f, 320) { Ink = 0.025f };
            Body(p, Sd.Circle(0, 0, 0.3f), Rgba.Hex("E2A95E"), Rgba.Hex("C1833D"), null, -0.03f, 0.03f);
            p.Line(Rgba.Hex("C98A44"), 0.012f, -0.22f, 0f, 0.22f, 0f);
            foreach (var (x, y) in new[] { (-0.14f, 0.12f), (0.12f, 0.15f), (0f, -0.12f), (-0.15f, -0.1f), (0.16f, -0.08f), (0.02f, 0.06f) })
                p.Fill(Sd.Circle(x, y, 0.022f), Rgba.Hex("8F5A24"));
            return new DoodleArt("hw_biscuit", p);
        }

        // ================= 246 THE PERFECT PUMPKIN =================

        /// <summary>Stanza: carta da parati viola a righe, finestra con la luna, pavimento di legno (coordinate del livello).</summary>
        public static DoodleArt HW_RoomBg()
        {
            var p = new DoodlePainter(-3.2f, -6.6f, 3.2f, 6.6f, 40) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 6.7f), (x, y) => ((int)Math.Floor((x + 4f) / 0.45f) % 2 == 0) ? Rgba.Hex("7A4FA8") : Rgba.Hex("6E4599"));
            // finestra con la notte
            Sdf win = Sd.Box(-1.6f, 1.5f, 0.85f, 0.95f, 0.08f);
            p.FillFunc(win, (x, y) => Rgba.Lerp(Rgba.Hex("2A1B55"), Rgba.Hex("1A1236"), HW_Clamp((y - 0.6f) / 1.8f)));
            p.Shape(Sd.Circle(-1.2f, 2.0f, 0.25f), Rgba.Hex("FFF4C2"), 0.03f);
            p.Shape(win, Rgba.Clear, 0.1f);
            p.Line(Rgba.Hex("3B2A1A"), 0.08f, -1.6f, 0.55f, -1.6f, 2.45f);
            p.Line(Rgba.Hex("3B2A1A"), 0.08f, -2.45f, 1.5f, -0.75f, 1.5f);
            // ragnatela nell'angolo
            for (int i = 0; i < 5; i++) p.Line(Rgba.White.WithAlpha(0.35f), 0.015f, 3.2f, 3.9f, 3.2f - HW_Cos(i * 0.39f) * 1.1f, 3.9f - HW_Sin(i * 0.39f) * 1.1f);
            for (int k = 1; k <= 3; k++) p.Line(Rgba.White.WithAlpha(0.3f), 0.015f, 3.2f - k * 0.33f, 3.9f, 3.2f - k * 0.3f, 3.9f - k * 0.15f, 3.2f, 3.9f - k * 0.33f);
            // battiscopa e pavimento
            p.Fill(Sd.Box(0, -2.7f, 3.3f, 0.12f), Rgba.Hex("4A2F1E"));
            p.FillFunc(Sd.Box(0, -4.8f, 3.3f, 2.0f), (x, y) => Rgba.Lerp(Rgba.Hex("8A5A34"), Rgba.Hex("6E4426"), HW_Clamp((-y - 2.8f) / 3f)));
            for (int i = -3; i <= 3; i++) p.Line(Rgba.Hex("5C3820"), 0.03f, i * 0.9f, -2.82f, i * 1.4f, -6.7f);
            return new DoodleArt("hw_room_bg", p);
        }

        /// <summary>Lampada appesa al soffitto (paralume in 0,0; filo verso l'alto).</summary>
        public static DoodleArt HW_CeilingLamp()
        {
            var p = new DoodlePainter(-0.62f, -0.35f, 0.62f, 1.4f, 120) { Ink = 0.045f };
            p.InkLine(0.04f, 0f, 1.4f, 0f, 0.35f);
            p.Shape(Sd.Circle(0, -0.08f, 0.14f), Rgba.Hex("FFF3A0"), 0.03f);
            Body(p, Sd.Polygon(-0.55f, -0.02f, 0.55f, -0.02f, 0.25f, 0.4f, -0.25f, 0.4f), Rgba.Hex("F2C14E"), Rgba.Hex("D19A2B"));
            p.Line(Rgba.Hex("B87F1E"), 0.03f, -0.5f, 0.05f, 0.5f, 0.05f);
            return new DoodleArt("hw_ceiling_lamp", p);
        }

        /// <summary>Cono di luce della lampada (punta in 0,0, verso il basso), semitrasparente.</summary>
        public static DoodleArt HW_LampLight()
        {
            var p = new DoodlePainter(-1.8f, -3.4f, 1.8f, 0.05f, 30) { Wobble = 0f };
            p.FillFunc(Sd.Polygon(-0.4f, 0f, 0.4f, 0f, 1.7f, -3.3f, -1.7f, -3.3f), (x, y) => Rgba.Hex("FFF1A8", 0.32f * HW_Clamp(1f + y / 3.6f)));
            return new DoodleArt("hw_lamp_light", p);
        }

        /// <summary>Tavolo di legno (piano in y 0, largo 6, gambe fino a -2.8).</summary>
        public static DoodleArt HW_Table()
        {
            var p = new DoodlePainter(-3.05f, -2.9f, 3.05f, 0.18f, 50) { Ink = 0.06f };
            foreach (float x in new[] { -2.5f, 2.5f }) Body(p, Sd.Box(x, -1.5f, 0.14f, 1.35f, 0.04f), Rgba.Hex("7A4A28"), Rgba.Hex("5C3519"));
            Body(p, Sd.Box(0, -0.12f, 2.95f, 0.22f, 0.06f), Rgba.Hex("A8693A"), Rgba.Hex("83502A"));
            p.Line(Rgba.Hex("C98A55"), 0.04f, -2.8f, 0.02f, 2.8f, 0.02f);
            p.Line(Rgba.Hex("83502A"), 0.025f, -2.0f, -0.15f, -0.6f, -0.18f);
            p.Line(Rgba.Hex("83502A"), 0.025f, 0.8f, -0.2f, 2.2f, -0.16f);
            return new DoodleArt("hw_table", p);
        }

        /// <summary>Zucca intera gigante (centro 0,0, 2.6 x 2.2) col picciolo: la faccia si intaglia col dito.</summary>
        public static DoodleArt HW_Pumpkin()
        {
            var p = new DoodlePainter(-1.42f, -1.18f, 1.42f, 1.55f, 100) { Ink = 0.06f };
            p.Shape(Sd.Union(Sd.Capsule(0, 0.95f, 0.1f, 1.4f, 0.11f)), Rgba.Hex("6E8B3D"));
            p.Line(Rgba.Hex("4F6A2A"), 0.03f, 0.03f, 1.0f, 0.1f, 1.35f);
            Sdf[] lobes = { Sd.Ellipse(-0.72f, 0f, 0.62f, 1.0f), Sd.Ellipse(0.72f, 0f, 0.62f, 1.0f), Sd.Ellipse(-0.35f, 0f, 0.62f, 1.08f), Sd.Ellipse(0.35f, 0f, 0.62f, 1.08f), Sd.Ellipse(0, 0f, 0.55f, 1.1f) };
            Sdf all = Sd.Union(lobes);
            Body(p, all, HW_Orange, HW_OrangeDark, null, -0.1f, 0.08f);
            foreach (float x in new[] { -0.75f, -0.35f, 0.35f, 0.75f })
                p.Line(HW_OrangeDark, 0.035f, x * 0.9f, 0.95f, x * 1.12f, 0f, x * 0.9f, -0.98f);
            p.Fill(Sd.Ellipse(-0.55f, 0.55f, 0.14f, 0.28f), Rgba.Hex("FFB766", 0.8f));
            p.Shape(all, Rgba.Clear);
            return new DoodleArt("hw_pumpkin", p);
        }

        /// <summary>Coltello da intaglio (centro 0,0, lama a destra).</summary>
        public static DoodleArt HW_Knife()
        {
            var p = new DoodlePainter(-0.62f, -0.14f, 0.62f, 0.14f, 160) { Ink = 0.03f };
            Body(p, Sd.Box(-0.33f, 0, 0.24f, 0.075f, 0.05f), Rgba.Hex("6B3A1E"), Rgba.Hex("4D2812"));
            foreach (float x in new[] { -0.45f, -0.22f }) p.Fill(Sd.Circle(x, 0, 0.02f), Rgba.Hex("D8D2C2"));
            p.Shape(Sd.Polygon(-0.08f, 0.07f, 0.58f, 0.05f, 0.6f, 0.0f, -0.08f, -0.07f), Rgba.Hex("D9DEE8"));
            p.Line(Rgba.White, 0.015f, 0f, 0.035f, 0.5f, 0.03f);
            return new DoodleArt("hw_knife", p);
        }

        /// <summary>Candela bianca a colonna con colature (base in 0,0, stoppino in 0, 0.6).</summary>
        public static DoodleArt HW_Candle()
        {
            var p = new DoodlePainter(-0.22f, -0.05f, 0.22f, 0.66f, 200) { Ink = 0.028f };
            Sdf c = Sd.Union(Sd.Box(0, 0.26f, 0.15f, 0.26f, 0.03f), Sd.Capsule(-0.08f, 0.5f, -0.08f, 0.36f, 0.035f), Sd.Capsule(0.07f, 0.5f, 0.07f, 0.42f, 0.03f));
            Body(p, c, Rgba.Hex("FFF6E2"), Rgba.Hex("E5D7B8"), null, -0.04f, 0.02f);
            p.InkLine(0.018f, 0, 0.52f, 0.01f, 0.6f);
            return new DoodleArt("hw_candle", p);
        }

        /// <summary>Accendino rosso (centro 0,0; la fiamma esce dalla punta in 0, 0.33).</summary>
        public static DoodleArt HW_Lighter()
        {
            var p = new DoodlePainter(-0.2f, -0.34f, 0.2f, 0.36f, 200) { Ink = 0.028f };
            Body(p, Sd.Box(0, -0.08f, 0.14f, 0.24f, 0.05f), Rgba.Hex("E4483E"), Rgba.Hex("B52F2A"));
            p.Shape(Sd.Box(0, 0.22f, 0.12f, 0.08f, 0.02f), Rgba.Hex("C9CED8"));
            p.Shape(Sd.Circle(0.06f, 0.24f, 0.045f), Rgba.Hex("8E95A3"), 0.015f);
            p.Fill(Sd.Box(-0.06f, -0.08f, 0.03f, 0.16f, 0.02f), Rgba.White.WithAlpha(0.45f));
            return new DoodleArt("hw_lighter", p);
        }

        // ================= 247 THE GHOST WHO CAN'T SCARE =================

        /// <summary>Camera da letto di notte: parete blu, finestra con le tende e la luna, pavimento (coordinate del livello).</summary>
        public static DoodleArt HW_BedroomBg()
        {
            var p = new DoodlePainter(-3.2f, -6.6f, 3.2f, 6.6f, 40) { Ink = 0.06f };
            p.FillFunc(Sd.Box(0, 0, 3.3f, 6.7f), (x, y) => Rgba.Lerp(Rgba.Hex("2E3A6E"), Rgba.Hex("3E4C86"), HW_Clamp((y + 2f) / 6f)));
            for (int i = -3; i <= 3; i++)
                for (int j = -2; j <= 6; j++)
                    p.Fill(Sd.Circle(i * 0.9f + (j % 2) * 0.45f, j * 0.8f, 0.04f), Rgba.Hex("5262A0"), false);
            // finestra con le tende
            Sdf win = Sd.Box(0.2f, 2.0f, 0.75f, 0.85f, 0.06f);
            p.FillFunc(win, (x, y) => Rgba.Lerp(Rgba.Hex("141033"), Rgba.Hex("2A1F5C"), HW_Clamp((y - 1.2f) / 1.6f)));
            p.Shape(Sd.Circle(0.45f, 2.35f, 0.22f), Rgba.Hex("FFF4C2"), 0.03f);
            p.Shape(win, Rgba.Clear, 0.1f);
            p.Line(Rgba.Hex("3B2A1A"), 0.07f, 0.2f, 1.15f, 0.2f, 2.85f);
            foreach (float s in new[] { -1f, 1f })
                Body(p, Sd.Polygon(0.2f + s * 0.55f, 3.0f, 0.2f + s * 1.05f, 3.0f, 0.2f + s * 1.15f, 1.0f, 0.2f + s * 0.8f, 1.1f), Rgba.Hex("B04A6C"), Rgba.Hex("8A3352"));
            p.InkLine(0.06f, -1.1f, 3.05f, 1.5f, 3.05f);
            // pavimento con il tappeto
            p.Fill(Sd.Box(0, -2.05f, 3.3f, 0.1f), Rgba.Hex("1E2650"));
            p.FillFunc(Sd.Box(0, -4.4f, 3.3f, 2.3f), (x, y) => Rgba.Lerp(Rgba.Hex("5C3D2C"), Rgba.Hex("7A5238"), HW_Clamp((y + 4.5f) / 2.4f)));
            for (int i = -3; i <= 3; i++) p.Line(Rgba.Hex("4A2F21"), 0.03f, i * 0.9f, -2.15f, i * 1.3f, -6.7f);
            return new DoodleArt("hw_bedroom_bg", p);
        }

        /// <summary>Letto con testiera a sinistra, cuscino in (-1.55, 0.25) e coperta a toppe (piano del materasso in y 0).</summary>
        public static DoodleArt HW_Bed()
        {
            var p = new DoodlePainter(-2.45f, -1.35f, 2.45f, 1.2f, 70) { Ink = 0.055f };
            Body(p, Sd.Union(Sd.Box(-2.2f, -0.1f, 0.16f, 1.2f, 0.06f), Sd.Circle(-2.2f, 1.05f, 0.18f)), Rgba.Hex("8A5A34"), Rgba.Hex("6B4224"));
            Body(p, Sd.Box(2.25f, -0.55f, 0.12f, 0.75f, 0.04f), Rgba.Hex("8A5A34"), Rgba.Hex("6B4224"));
            Body(p, Sd.Box(0.05f, -0.75f, 2.2f, 0.3f, 0.05f), Rgba.Hex("8A5A34"), Rgba.Hex("6B4224"));
            Body(p, Sd.Ellipse(-1.55f, 0.18f, 0.55f, 0.28f), Rgba.Hex("F4F1FF"), Rgba.Hex("CFCBEA"));
            // coperta con il "bozzo" delle gambe
            Sdf quilt = Sd.Union(Sd.Box(0.4f, -0.25f, 1.85f, 0.32f, 0.12f), Sd.Ellipse(0.4f, 0.0f, 1.6f, 0.38f));
            p.Fill(quilt, Rgba.Hex("7E5BC6"));
            p.Hatch(Sd.Intersect(quilt, Sd.Box(-0.4f, 0, 0.45f, 1f)), Rgba.Hex("6B48B0"), 0.12f, 0.03f, 45f);
            p.Hatch(Sd.Intersect(quilt, Sd.Box(1.3f, 0, 0.45f, 1f)), Rgba.Hex("6B48B0"), 0.12f, 0.03f, -45f);
            p.Shape(quilt, Rgba.Clear);
            p.Line(Rgba.Hex("B9A4EE"), 0.03f, -1.2f, 0.1f, 2.1f, 0.1f);
            return new DoodleArt("hw_bed", p);
        }

        /// <summary>Testa dell'uomo sul cuscino, con la cuffia da notte (centro 0,0): addormentato o sveglio e scocciato.</summary>
        public static DoodleArt HW_ManHead(bool awake)
        {
            var p = new DoodlePainter(-0.62f, -0.5f, 0.78f, 0.82f, 140) { Ink = 0.04f };
            Body(p, Sd.Circle(0, 0, 0.4f), Rgba.Hex("F7C9A0"), Rgba.Hex("E0A57A"));
            p.Shape(Sd.Ellipse(-0.38f, 0.0f, 0.08f, 0.12f), Rgba.Hex("F7C9A0"), 0.03f);
            // cuffia a righe col pompon
            Sdf cap = Sd.Polygon(-0.42f, 0.12f, 0.4f, 0.22f, 0.65f, 0.55f, 0.1f, 0.62f, -0.3f, 0.45f);
            p.Fill(cap, Rgba.Hex("4FA3E0"));
            p.Hatch(cap, Rgba.White, 0.14f, 0.05f, 70f);
            p.Shape(cap, Rgba.Clear);
            p.Shape(Sd.Circle(0.66f, 0.6f, 0.1f), Rgba.White, 0.03f);
            // baffoni
            p.Shape(Sd.Union(Sd.Ellipse(-0.1f, -0.15f, 0.14f, 0.06f), Sd.Ellipse(0.12f, -0.15f, 0.14f, 0.06f)), Rgba.Hex("6B4A2E"), 0.025f);
            p.Shape(Sd.Ellipse(0.0f, -0.04f, 0.08f, 0.07f), Rgba.Hex("F2A889"), 0.025f);
            if (awake)
            {
                foreach (float x in new[] { -0.16f, 0.16f })
                {
                    p.Shape(Sd.Circle(x, 0.1f, 0.075f), Rgba.White, 0.02f);
                    p.Fill(Sd.Circle(x, 0.09f, 0.035f), Ink);
                    p.Fill(Sd.Box(x, 0.15f, 0.09f, 0.035f), Rgba.Hex("E0A57A"));   // palpebre a metà: scocciato
                    p.InkLine(0.025f, x - 0.09f, 0.13f, x + 0.09f, 0.13f);
                }
                p.InkLine(0.03f, -0.1f, -0.27f, 0.1f, -0.27f);
            }
            else
            {
                foreach (float x in new[] { -0.16f, 0.16f }) p.InkLine(0.03f, x - 0.08f, 0.1f, x, 0.07f, x + 0.08f, 0.1f);
                p.Shape(Sd.Ellipse(0.02f, -0.28f, 0.05f, 0.06f), Rgba.Hex("7A2A3A"), 0.02f);
            }
            return new DoodleArt(awake ? "hw_man_awake" : "hw_man_sleep", p);
        }

        /// <summary>L'uomo seduto nel letto col lenzuolo addosso: sembra un fantasma (centro della testa in 0,0).</summary>
        public static DoodleArt HW_ManGhost()
        {
            var p = new DoodlePainter(-0.85f, -0.75f, 0.85f, 0.75f, 120) { Ink = 0.05f };
            var parts = new List<Sdf> { Sd.Circle(0, 0.15f, 0.5f), Sd.Box(0, -0.3f, 0.62f, 0.4f, 0.1f) };
            for (int i = 0; i < 5; i++) parts.Add(Sd.Circle(-0.5f + i * 0.25f, -0.66f, 0.13f));
            Sdf sheet = Sd.Union(parts.ToArray());
            Body(p, sheet, HW_Linen, HW_LinenShade);
            foreach (float x in new[] { -0.17f, 0.17f })
            {
                p.Fill(Sd.Ellipse(x, 0.2f, 0.09f, 0.13f), Rgba.Hex("2B2233"));
                p.Fill(Sd.Circle(x, 0.22f, 0.035f), Rgba.White);
            }
            p.Fill(Sd.Ellipse(0, -0.12f, 0.07f, 0.09f), Rgba.Hex("2B2233"));
            p.Shape(Sd.Circle(0.38f, 0.58f, 0.09f), Rgba.White, 0.03f);   // il pompon della cuffia che spunta
            return new DoodleArt("hw_man_ghost", p);
        }

        /// <summary>Fantasma col lenzuolo (centro 0,0, alto 1.8) oppure invisibile: solo un contorno tenue e gli occhietti.</summary>
        public static DoodleArt HW_Ghost(bool invisible)
        {
            var p = new DoodlePainter(-0.9f, -0.98f, 0.9f, 0.98f, 120) { Ink = 0.05f };
            var parts = new List<Sdf> { Sd.Circle(0, 0.25f, 0.68f), Sd.Box(0, -0.35f, 0.68f, 0.45f, 0.05f) };
            for (int i = 0; i < 5; i++) parts.Add(Sd.Circle(-0.54f + i * 0.27f, -0.8f, 0.14f));
            Sdf ghost = Sd.Subtract(Sd.Union(parts.ToArray()), Sd.Union(Sd.Circle(-0.405f, -0.95f, 0.07f), Sd.Circle(-0.135f, -0.95f, 0.07f), Sd.Circle(0.135f, -0.95f, 0.07f), Sd.Circle(0.405f, -0.95f, 0.07f)));
            if (invisible)
            {
                p.Fill(Sd.Subtract(Sd.Grow(ghost, 0.025f), Sd.Grow(ghost, -0.025f)), Rgba.Hex("DDE6FF", 0.45f));
                foreach (float x in new[] { -0.22f, 0.22f })
                {
                    p.FillFunc(Sd.Circle(x, 0.3f, 0.2f), (xx, yy) => Rgba.Hex("9EF0FF", 0.5f * (float)Math.Exp(-Math.Sqrt((xx - x) * (xx - x) + (yy - 0.3f) * (yy - 0.3f)) * 12f)));
                    p.Fill(Sd.Ellipse(x, 0.3f, 0.07f, 0.1f), Rgba.Hex("BDF6FF"));
                }
                return new DoodleArt("hw_ghost_invisible", p);
            }
            Body(p, ghost, HW_Linen, HW_LinenShade);
            p.Line(HW_LinenShade, 0.03f, -0.35f, -0.2f, -0.3f, -0.75f);
            p.Line(HW_LinenShade, 0.03f, 0.3f, -0.1f, 0.38f, -0.7f);
            foreach (float x in new[] { -0.22f, 0.22f }) p.Fill(Sd.Ellipse(x, 0.3f, 0.1f, 0.15f), Rgba.Hex("2B2233"));
            p.Fill(Sd.Ellipse(0, -0.05f, 0.1f, 0.12f), Rgba.Hex("2B2233"));
            p.Fill(Sd.Circle(-0.35f, 0.62f, 0.09f), Rgba.White);
            return new DoodleArt("hw_ghost", p);
        }

        /// <summary>Lenzuolo stropicciato (centro 0,0, 1.4 x 0.75).</summary>
        public static DoodleArt HW_Sheet()
        {
            var p = new DoodlePainter(-0.8f, -0.45f, 0.8f, 0.5f, 120) { Ink = 0.045f };
            Sdf s = Sd.Union(Sd.Ellipse(-0.3f, 0f, 0.45f, 0.3f), Sd.Ellipse(0.3f, -0.05f, 0.42f, 0.32f), Sd.Ellipse(0f, 0.15f, 0.35f, 0.25f));
            Body(p, s, HW_Linen, HW_LinenShade);
            p.Line(HW_LinenShade, 0.03f, -0.5f, 0.05f, -0.1f, -0.1f, 0.2f, 0.1f);
            p.Line(HW_LinenShade, 0.03f, 0.1f, -0.25f, 0.5f, -0.1f);
            foreach (float x in new[] { -0.12f, 0.12f }) p.Fill(Sd.Ellipse(x, 0.18f, 0.05f, 0.07f), Rgba.Hex("2B2233"));
            return new DoodleArt("hw_sheet", p);
        }

        /// <summary>Specchio ovale sul cavalletto (centro del vetro in 0,0; piedi in y -1.6).</summary>
        public static DoodleArt HW_Mirror()
        {
            var p = new DoodlePainter(-0.9f, -1.7f, 0.9f, 1.15f, 100) { Ink = 0.05f };
            Rgba wood = Rgba.Hex("8A5A34"), woodS = Rgba.Hex("6B4224");
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Capsule(s * 0.45f, -0.6f, s * 0.6f, -1.6f, 0.06f), wood, woodS);
            Body(p, Sd.Ellipse(0, 0, 0.72f, 1.02f), wood, woodS);
            p.FillFunc(Sd.Ellipse(0, 0, 0.58f, 0.88f), (x, y) => Rgba.Lerp(Rgba.Hex("9FC6E8"), Rgba.Hex("D8ECFA"), HW_Clamp((y + x + 1f) / 2f)));
            p.Line(Rgba.White.WithAlpha(0.7f), 0.06f, -0.3f, 0.45f, 0f, 0.7f);
            p.Line(Rgba.White.WithAlpha(0.5f), 0.04f, -0.32f, 0.2f, 0.1f, 0.62f);
            p.Shape(Sd.Ellipse(0, 0, 0.58f, 0.88f), Rgba.Clear, 0.035f);
            return new DoodleArt("hw_mirror", p);
        }

        /// <summary>Comodino col cassetto (piano in y 0).</summary>
        public static DoodleArt HW_Nightstand()
        {
            var p = new DoodlePainter(-0.6f, -1.35f, 0.6f, 0.1f, 100) { Ink = 0.05f };
            Body(p, Sd.Box(0, -0.62f, 0.5f, 0.62f, 0.05f), Rgba.Hex("A8693A"), Rgba.Hex("83502A"));
            p.Shape(Sd.Box(0, -0.4f, 0.38f, 0.2f, 0.04f), Rgba.Hex("BC7C48"), 0.03f);
            p.Shape(Sd.Circle(0, -0.4f, 0.05f), Rgba.Hex("FFD15A"), 0.02f);
            return new DoodleArt("hw_nightstand", p);
        }

        // ================= 248 THE WITCH AND THE IMPOSSIBLE POTION =================

        /// <summary>Capanna della strega: muro di pietra, mensola (piano in y 1.35), finestrella con la luna, pavimento.</summary>
        public static DoodleArt HW_WitchBg()
        {
            var p = new DoodlePainter(-3.2f, -6.6f, 3.2f, 6.6f, 40) { Ink = 0.06f };
            p.Fill(Sd.Box(0, 0, 3.3f, 6.7f), Rgba.Hex("4B3F63"));
            var rnd = new Random(248);
            for (int j = -8; j <= 15; j++)
                for (int i = -4; i <= 4; i++)
                {
                    float x = i * 0.8f + (j % 2 == 0 ? 0f : 0.4f) + (float)rnd.NextDouble() * 0.06f, y = j * 0.45f;
                    p.Shape(Sd.Box(x, y, 0.36f, 0.19f, 0.07f), Rgba.Lerp(Rgba.Hex("5A4D75"), Rgba.Hex("665A82"), (float)rnd.NextDouble()), 0.03f);
                }
            // finestrella tonda con la luna
            p.FillFunc(Sd.Circle(1.9f, 0.1f, 0.55f), (x, y) => Rgba.Lerp(Rgba.Hex("141033"), Rgba.Hex("2E2160"), HW_Clamp(y + 0.4f)));
            p.Shape(Sd.Circle(2.05f, 0.25f, 0.16f), Rgba.Hex("FFF4C2"), 0.025f);
            p.Shape(Sd.Subtract(Sd.Circle(1.9f, 0.1f, 0.66f), Sd.Circle(1.9f, 0.1f, 0.55f)), Rgba.Hex("6B4224"));
            // mensola degli ingredienti
            Body(p, Sd.Box(0, 1.25f, 3.0f, 0.1f, 0.03f), Rgba.Hex("8A5A34"), Rgba.Hex("6B4224"));
            foreach (float x in new[] { -2.6f, 2.6f }) p.Shape(Sd.Polygon(x - 0.08f, 1.15f, x + 0.08f, 1.15f, x, 0.8f), Rgba.Hex("6B4224"), 0.03f);
            // ragnatele e aglio appeso
            p.InkLine(0.02f, -2.2f, 3.6f, -2.2f, 3.0f);
            p.Shape(Sd.Union(Sd.Circle(-2.2f, 2.85f, 0.16f), Sd.Polygon(-2.28f, 2.95f, -2.12f, 2.95f, -2.2f, 3.1f)), Rgba.Hex("F2EBD9"), 0.03f);
            // pavimento
            p.FillFunc(Sd.Box(0, -5.0f, 3.3f, 1.65f), (x, y) => Rgba.Lerp(Rgba.Hex("3A2A1E"), Rgba.Hex("5A3F2A"), HW_Clamp((y + 5f) / 1.8f)));
            p.InkLine(0.06f, -3.3f, -3.35f, 3.3f, -3.35f);
            return new DoodleArt("hw_witch_bg", p);
        }

        /// <summary>La strega (piedi in 0,0, alta 3.4), guarda a destra con un NASO ENORME. Glam: capelli, rossetto, abito rosa.</summary>
        public static DoodleArt HW_Witch(bool glam)
        {
            var p = new DoodlePainter(-1.15f, -0.08f, 1.45f, 3.55f, 100) { Ink = 0.055f };
            Rgba dress = glam ? Rgba.Hex("FF6FB2") : Rgba.Hex("6B3FA0"), dressS = glam ? Rgba.Hex("E04C93") : Rgba.Hex("4E2C7A");
            Rgba hair = glam ? Rgba.Hex("FFD15A") : Rgba.Hex("2B2233");
            // capelli dietro
            if (glam) Body(p, Sd.Union(Sd.Circle(-0.35f, 2.05f, 0.5f), Sd.Circle(-0.55f, 1.6f, 0.38f), Sd.Circle(0.2f, 2.35f, 0.4f)), hair, Rgba.Hex("E8B23A"));
            else foreach (float x in new[] { -0.55f, -0.42f, -0.3f }) p.Line(hair, 0.07f, x, 2.4f, x - 0.15f, 1.55f);
            // vestito a campana con le scarpe a punta
            foreach (float x in new[] { -0.25f, 0.25f }) p.Shape(Sd.Polygon(x - 0.15f, 0.0f, x + 0.3f, 0.0f, x + 0.38f, 0.1f, x - 0.12f, 0.15f), Rgba.Hex("2B2233"), 0.03f);
            Sdf gown = Sd.Polygon(-0.75f, 0.12f, 0.75f, 0.12f, 0.32f, 1.75f, -0.32f, 1.75f);
            Body(p, gown, dress, dressS);
            if (glam) foreach (var (x, y) in new[] { (-0.3f, 0.5f), (0.2f, 0.9f), (-0.05f, 1.35f), (0.4f, 0.4f) }) p.Fill(Sd.Circle(x, y, 0.04f), Rgba.White);
            else p.Shape(Sd.Box(0, 1.0f, 0.45f, 0.06f, 0.02f), Rgba.Hex("2B2233"), 0.02f);
            // braccio verso il calderone
            Body(p, Sd.Capsule(0.15f, 1.55f, 0.75f, 1.2f, 0.11f), dress, dressS);
            p.Shape(Sd.Circle(0.82f, 1.17f, 0.11f), HW_SkinW, 0.03f);
            // testa verde col NASONE e il porro
            Sdf head = Sd.Ellipse(0, 2.2f, 0.42f, 0.46f);
            Body(p, head, HW_SkinW, HW_SkinWS);
            Sdf nose = Sd.Polygon(0.2f, 2.35f, 0.38f, 2.3f, 1.35f, 1.95f, 1.2f, 1.88f, 0.3f, 2.02f);
            Body(p, nose, HW_SkinW, HW_SkinWS);
            p.Shape(Sd.Circle(0.85f, 2.12f, 0.06f), Rgba.Hex("6FAE45"), 0.02f);
            // occhi
            if (glam)
            {
                HW_Eye(p, 0.1f, 2.38f, 0.1f, 0.6f, 0f);
                for (int k = 0; k < 3; k++) p.InkLine(0.025f, 0.05f + k * 0.06f, 2.47f, 0.02f + k * 0.08f, 2.56f);
                p.Shape(Sd.Ellipse(0.12f, 1.92f, 0.13f, 0.06f), Rgba.Hex("E8233F"), 0.02f);   // rossetto
                p.Fill(Sd.Circle(-0.15f, 2.1f, 0.08f), Rgba.Hex("FF8FB8", 0.7f));
            }
            else
            {
                HW_Eye(p, 0.1f, 2.38f, 0.1f, 0.6f, 0f, Rgba.Hex("FFF6B0"));
                p.InkLine(0.035f, -0.02f, 2.52f, 0.25f, 2.48f);
                p.InkLine(0.03f, 0.0f, 1.9f, 0.2f, 1.86f, 0.32f, 1.9f);
                p.Fill(Sd.Box(0.15f, 1.85f, 0.03f, 0.03f), Rgba.White);   // dentino solitario
            }
            // cappello a punta
            HW_HatAt(p, -0.05f, 2.55f, 1.15f, glam ? Rgba.Hex("FF6FB2") : Rgba.Hex("9B6FD6"));
            if (glam) p.Shape(Sd.Circle(0.25f, 2.75f, 0.11f), Rgba.Hex("FF9FC4"), 0.025f);
            return new DoodleArt(glam ? "hw_witch_glam" : "hw_witch", p);
        }

        /// <summary>La strega trasformata in rospo (stesse coordinate della strega): cappello e nasone compresi.</summary>
        public static DoodleArt HW_WitchToad()
        {
            var p = new DoodlePainter(-1.15f, -0.08f, 1.45f, 3.55f, 100) { Ink = 0.055f };
            Rgba toad = Rgba.Hex("7FBF4D"), toadS = Rgba.Hex("5C9A33");
            foreach (float s in new[] { -1f, 1f }) Body(p, Sd.Ellipse(s * 0.55f, 0.18f, 0.4f, 0.2f), toad, toadS);
            Body(p, Sd.Ellipse(0, 0.8f, 0.85f, 0.7f), toad, toadS);
            p.Shape(Sd.Ellipse(0, 0.65f, 0.55f, 0.45f), Rgba.Hex("D8EAA0"), 0.03f);
            foreach (var (x, y) in new[] { (-0.5f, 1.1f), (0.45f, 1.2f), (-0.2f, 1.35f) }) p.Fill(Sd.Circle(x, y, 0.06f), toadS);
            foreach (float s in new[] { -1f, 1f }) { p.Shape(Sd.Circle(s * 0.35f, 1.55f, 0.22f), toad); HW_Eye(p, s * 0.35f, 1.58f, 0.13f, 0.3f, 0f); }
            p.InkLine(0.04f, -0.4f, 1.1f, 0f, 1.0f, 0.4f, 1.1f);
            Body(p, Sd.Polygon(0.05f, 1.3f, 0.2f, 1.25f, 1.1f, 0.95f, 0.98f, 0.88f, 0.1f, 1.05f), HW_SkinW, HW_SkinWS);
            HW_HatAt(p, 0f, 1.72f, 0.9f, Rgba.Hex("9B6FD6"));
            return new DoodleArt("hw_witch_toad", p);
        }

        /// <summary>La strega trasformata in mela (stesse coordinate): faccina, nasone e cappello.</summary>
        public static DoodleArt HW_WitchApple()
        {
            var p = new DoodlePainter(-1.15f, -0.08f, 1.45f, 3.55f, 100) { Ink = 0.055f };
            Sdf apple = Sd.Union(Sd.Circle(-0.35f, 0.95f, 0.7f), Sd.Circle(0.35f, 0.95f, 0.7f), Sd.Ellipse(0, 0.7f, 0.85f, 0.65f));
            Body(p, apple, Rgba.Hex("E4483E"), Rgba.Hex("B52F2A"), null, -0.08f, 0.08f);
            p.Fill(Sd.Ellipse(-0.45f, 1.25f, 0.12f, 0.22f), Rgba.White.WithAlpha(0.5f));
            p.Shape(Sd.Rotate(Sd.Ellipse(0.35f, 1.75f, 0.25f, 0.1f), 0.35f, 1.75f, 25f), HW_Green, 0.03f);
            foreach (float s in new[] { -1f, 1f }) HW_Eye(p, s * 0.25f, 1.1f, 0.12f, 0.4f, 0f);
            Body(p, Sd.Polygon(0.05f, 0.95f, 0.2f, 0.9f, 1.2f, 0.6f, 1.08f, 0.52f, 0.1f, 0.7f), HW_SkinW, HW_SkinWS);
            p.InkLine(0.035f, -0.2f, 0.5f, 0f, 0.42f, 0.15f, 0.48f);
            HW_HatAt(p, -0.05f, 1.62f, 0.95f, Rgba.Hex("9B6FD6"));
            return new DoodleArt("hw_witch_apple", p);
        }

        /// <summary>Tanti occhi spalancati sparsi sulla strega (sovrapposizione, stesse coordinate).</summary>
        public static DoodleArt HW_WitchEyes()
        {
            var p = new DoodlePainter(-1.15f, -0.08f, 1.45f, 3.55f, 100) { Ink = 0.04f };
            var rnd = new Random(7);
            foreach (var (x, y, r) in new[] { (-0.35f, 0.5f, 0.14f), (0.3f, 0.8f, 0.12f), (-0.1f, 1.25f, 0.16f), (0.35f, 0.3f, 0.1f), (-0.45f, 1.0f, 0.1f),
                (0.55f, 1.25f, 0.09f), (-0.2f, 2.0f, 0.1f), (0.3f, 2.25f, 0.13f), (0.0f, 2.9f, 0.12f), (0.95f, 2.05f, 0.08f) })
                HW_Eye(p, x, y, r, (float)rnd.NextDouble() * 2f - 1f, (float)rnd.NextDouble() * 2f - 1f);
            return new DoodleArt("hw_witch_eyes", p);
        }

        /// <summary>Calderone nero sul fuoco (centro della pancia in 0,0; bordo in y 0.55).</summary>
        public static DoodleArt HW_Cauldron()
        {
            var p = new DoodlePainter(-1.25f, -1.05f, 1.25f, 0.75f, 100) { Ink = 0.06f };
            foreach (float x in new[] { -0.5f, 0f, 0.5f })
                p.Shape(Sd.Union(Sd.Circle(x, -0.85f, 0.14f), Sd.Polygon(x - 0.13f, -0.85f, x + 0.13f, -0.85f, x, -0.5f)), Rgba.Hex("FF8A1F"), 0.025f);
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Capsule(s * 0.6f, -0.5f, s * 0.75f, -0.9f, 0.07f), Rgba.Hex("2B2233"));
            Body(p, Sd.Ellipse(0, -0.05f, 1.05f, 0.75f), Rgba.Hex("3B3445"), Rgba.Hex("24202B"), null, -0.1f, 0.08f);
            p.Shape(Sd.Ellipse(0, 0.52f, 1.08f, 0.2f), Rgba.Hex("4D4558"));
            p.Fill(Sd.Ellipse(-0.5f, 0.1f, 0.12f, 0.25f), Rgba.White.WithAlpha(0.15f));
            return new DoodleArt("hw_cauldron", p);
        }

        /// <summary>Superficie della pozione (bianca, si tinge: verde e poi rosa), in coordinate del calderone.</summary>
        public static DoodleArt HW_Brew()
        {
            var p = new DoodlePainter(-1.0f, 0.3f, 1.0f, 0.85f, 100) { Ink = 0.03f };
            p.Fill(Sd.Ellipse(0, 0.55f, 0.92f, 0.15f), Rgba.Hex("E8E8E8"));
            foreach (var (x, y, r) in new[] { (-0.4f, 0.6f, 0.1f), (0.15f, 0.65f, 0.13f), (0.55f, 0.58f, 0.08f), (-0.1f, 0.5f, 0.06f) })
                p.Shape(Sd.Circle(x, y, r), Rgba.White, 0.02f);
            return new DoodleArt("hw_brew", p);
        }

        /// <summary>Rospo (ingrediente, centro 0,0).</summary>
        public static DoodleArt HW_Toad()
        {
            var p = new DoodlePainter(-0.45f, -0.32f, 0.45f, 0.38f, 140) { Ink = 0.035f };
            Rgba toad = Rgba.Hex("7FBF4D"), toadS = Rgba.Hex("5C9A33");
            Body(p, Sd.Ellipse(0, -0.05f, 0.4f, 0.25f), toad, toadS);
            foreach (float s in new[] { -1f, 1f }) { p.Shape(Sd.Circle(s * 0.17f, 0.18f, 0.12f), toad, 0.03f); HW_Eye(p, s * 0.17f, 0.2f, 0.07f); }
            p.InkLine(0.025f, -0.2f, -0.05f, 0f, -0.1f, 0.2f, -0.05f);
            foreach (var (x, y) in new[] { (-0.25f, -0.15f), (0.22f, -0.12f), (0.05f, -0.22f) }) p.Fill(Sd.Circle(x, y, 0.035f), toadS);
            return new DoodleArt("hw_toad", p);
        }

        /// <summary>Occhio (ingrediente, centro 0,0) con le venuzze.</summary>
        public static DoodleArt HW_Eyeball()
        {
            var p = new DoodlePainter(-0.32f, -0.32f, 0.32f, 0.32f, 160) { Ink = 0.035f };
            Body(p, Sd.Circle(0, 0, 0.27f), Rgba.White, Rgba.Hex("E6E0EA"));
            foreach (float a in new[] { 0.3f, 2.2f, 3.6f, 5.0f }) p.Line(Rgba.Hex("E8233F"), 0.012f, HW_Cos(a) * 0.26f, HW_Sin(a) * 0.26f, HW_Cos(a + 0.2f) * 0.17f, HW_Sin(a + 0.2f) * 0.17f);
            p.Shape(Sd.Circle(0.06f, 0.04f, 0.12f), Rgba.Hex("3EA0D8"), 0.025f);
            p.Fill(Sd.Circle(0.07f, 0.04f, 0.06f), Ink);
            p.Fill(Sd.Circle(0.03f, 0.08f, 0.025f), Rgba.White);
            return new DoodleArt("hw_eye", p);
        }

        /// <summary>Fungo rosso a pois (ingrediente, centro 0,0).</summary>
        public static DoodleArt HW_Mushroom()
        {
            var p = new DoodlePainter(-0.36f, -0.32f, 0.36f, 0.34f, 150) { Ink = 0.035f };
            Body(p, Sd.Box(0, -0.15f, 0.1f, 0.15f, 0.05f), Rgba.Hex("FFF3D6"), Rgba.Hex("E8D7B0"));
            Sdf cap = Sd.Intersect(Sd.Ellipse(0, 0.0f, 0.32f, 0.3f), Sd.Box(0, 0.16f, 0.4f, 0.16f));
            Body(p, cap, Rgba.Hex("E4483E"), Rgba.Hex("B52F2A"));
            foreach (var (x, y, r) in new[] { (-0.15f, 0.12f, 0.045f), (0.1f, 0.2f, 0.05f), (0.2f, 0.06f, 0.035f), (-0.02f, 0.06f, 0.03f) }) p.Fill(Sd.Circle(x, y, r), Rgba.White);
            return new DoodleArt("hw_mushroom", p);
        }

        /// <summary>Cuore da pozione con le cuciture (ingrediente, centro 0,0).</summary>
        public static DoodleArt HW_HeartIngredient()
        {
            var p = new DoodlePainter(-0.34f, -0.34f, 0.34f, 0.3f, 150) { Ink = 0.035f };
            Sdf heart = Sd.Union(Sd.Circle(-0.13f, 0.08f, 0.15f), Sd.Circle(0.13f, 0.08f, 0.15f), Sd.Polygon(-0.27f, 0.03f, 0.27f, 0.03f, 0f, -0.3f));
            Body(p, heart, Rgba.Hex("D93D6B"), Rgba.Hex("A82A50"));
            p.Line(Rgba.Hex("FFD1DC"), 0.015f, -0.05f, 0.18f, 0.05f, -0.15f);
            for (int k = 0; k < 4; k++) { float t = k / 3f; float x = -0.05f + 0.1f * t, y = 0.18f - 0.33f * t; p.Line(Rgba.Hex("FFD1DC"), 0.012f, x - 0.04f, y - 0.01f, x + 0.04f, y + 0.01f); }
            p.Fill(Sd.Circle(-0.15f, 0.13f, 0.035f), Rgba.White.WithAlpha(0.7f));
            return new DoodleArt("hw_heart", p);
        }

        /// <summary>Smartphone rosa per il selfie (centro 0,0).</summary>
        public static DoodleArt HW_Phone()
        {
            var p = new DoodlePainter(-0.22f, -0.36f, 0.22f, 0.36f, 180) { Ink = 0.03f };
            p.Shape(Sd.Box(0, 0, 0.17f, 0.31f, 0.05f), Rgba.Hex("FF6FB2"));
            p.Fill(Sd.Box(0, 0.01f, 0.13f, 0.25f, 0.02f), Rgba.Hex("9FD6FF"));
            p.Fill(Sd.Circle(0.07f, 0.2f, 0.03f), Rgba.White);
            return new DoodleArt("hw_phone", p);
        }

        // ================= 249 THE VEGETARIAN ZOMBIE =================

        /// <summary>Cimitero nella nebbia: lapidi, cancellata, albero storto (coordinate del livello, terreno sotto y -3.0).</summary>
        public static DoodleArt HW_GraveyardBg()
        {
            var p = new DoodlePainter(-3.2f, -6.6f, 3.2f, 6.6f, 40) { Ink = 0.06f };
            HW_Sky(p, "102A2E", "2F5A4E", 249);
            p.Shape(Sd.Circle(-2.0f, 2.6f, 0.45f), Rgba.Hex("E9F7D0"), 0.04f);
            // cancellata
            for (int i = -6; i <= 6; i++) p.Line(Rgba.Hex("14201E"), 0.06f, i * 0.5f, -3.0f, i * 0.5f, -1.55f);
            for (int i = -6; i <= 6; i++) p.Fill(Sd.Polygon(i * 0.5f - 0.07f, -1.55f, i * 0.5f + 0.07f, -1.55f, i * 0.5f, -1.38f), Rgba.Hex("14201E"));
            p.Line(Rgba.Hex("14201E"), 0.06f, -3.3f, -1.75f, 3.3f, -1.75f);
            // terreno
            p.FillFunc(Sd.Union(Sd.Box(0, -5.0f, 3.3f, 2.0f), Sd.Ellipse(0, -3.0f, 3.6f, 0.3f)), (x, y) => Rgba.Lerp(Rgba.Hex("1E3A2E"), Rgba.Hex("2F5A3E"), HW_Clamp((y + 4.6f) / 1.8f)));
            // lapidi
            foreach (var (x, y, s) in new[] { (-2.5f, -2.6f, 0.9f), (2.55f, -2.75f, 1.0f), (0.6f, -2.5f, 0.75f) })
            {
                Body(p, Sd.Union(Sd.Box(x, y, 0.3f * s, 0.32f * s, 0.04f), Sd.Circle(x, y + 0.3f * s, 0.3f * s)), Rgba.Hex("8A8AA0"), Rgba.Hex("6A6A82"));
                p.Line(Rgba.Hex("55556C"), 0.04f, x - 0.15f * s, y + 0.25f * s, x + 0.15f * s, y + 0.25f * s);
                p.Line(Rgba.Hex("55556C"), 0.04f, x - 0.12f * s, y + 0.05f * s, x + 0.12f * s, y + 0.05f * s);
            }
            // nebbiolina
            p.FillFunc(Sd.Box(0, -3.2f, 3.3f, 0.5f), (x, y) => Rgba.Hex("DFF2E8", 0.18f * (0.6f + 0.4f * HW_Sin(x * 2.3f)) * HW_Clamp(1f - Math.Abs(y + 3.2f) / 0.5f)));
            return new DoodleArt("hw_graveyard_bg", p);
        }

        /// <summary>
        /// Corpo dello zombie visto di fronte (piedi in 0,0): pantaloni strappati, camicia a brandelli, braccio sinistro
        /// teso. La testa e il braccio destro sono disegni a parte (spalla destra in 0.42, 1.72).
        /// </summary>
        public static DoodleArt HW_ZombieBody()
        {
            var p = new DoodlePainter(-1.25f, -0.08f, 0.75f, 2.05f, 100) { Ink = 0.055f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Ellipse(s * 0.25f, 0.08f, 0.22f, 0.1f), Rgba.Hex("4A3A2E"), 0.03f);
                Body(p, Sd.Polygon(s * 0.08f, 1.0f, s * 0.42f, 1.0f, s * 0.38f, 0.12f, s * 0.12f, 0.12f), Rgba.Hex("4F6A9A"), Rgba.Hex("3A5180"));
            }
            p.Shape(Sd.Polygon(0.12f, 0.35f, 0.38f, 0.35f, 0.3f, 0.2f, 0.2f, 0.28f), HW_Zombie, 0.02f);   // strappo con la pelle
            // camicia a brandelli
            Sdf shirt = Sd.Polygon(-0.48f, 1.8f, 0.48f, 1.8f, 0.5f, 0.92f, 0.32f, 1.0f, 0.18f, 0.88f, 0f, 0.98f, -0.2f, 0.86f, -0.35f, 0.98f, -0.5f, 0.92f);
            Body(p, shirt, Rgba.Hex("A0705A"), Rgba.Hex("7E5242"));
            p.Line(Rgba.Hex("6B4235"), 0.03f, 0f, 1.75f, 0f, 1.0f);
            p.Shape(Sd.Polygon(-0.25f, 1.45f, -0.05f, 1.4f, -0.15f, 1.25f), HW_Zombie, 0.02f);
            // collo
            p.Shape(Sd.Box(0, 1.85f, 0.14f, 0.1f, 0.03f), HW_Zombie, 0.035f);
            // braccio sinistro teso in avanti
            Body(p, Sd.Capsule(-0.42f, 1.72f, -0.92f, 1.38f, 0.13f), Rgba.Hex("A0705A"), Rgba.Hex("7E5242"));
            Body(p, Sd.Union(Sd.Circle(-1.02f, 1.32f, 0.14f), Sd.Capsule(-1.05f, 1.3f, -1.18f, 1.18f, 0.04f), Sd.Capsule(-1.0f, 1.24f, -1.1f, 1.08f, 0.04f)), HW_Zombie, HW_ZombieShade);
            return new DoodleArt("hw_zombie_body", p);
        }

        /// <summary>Testa dello zombie (coordinate del corpo: centro 0, 2.3): affamata o felice con le guance rosa.</summary>
        public static DoodleArt HW_ZombieHead(bool happy)
        {
            var p = new DoodlePainter(-0.6f, 1.78f, 0.6f, 2.9f, 120) { Ink = 0.05f };
            Body(p, Sd.Ellipse(0, 2.3f, 0.44f, 0.46f), HW_Zombie, HW_ZombieShade);
            // capelli spettinati
            foreach (float x in new[] { -0.25f, -0.1f, 0.05f, 0.2f }) p.Line(Rgba.Hex("3A2B2E"), 0.06f, x, 2.7f, x + 0.08f, 2.84f);
            // cicatrice cucita
            p.Line(Rgba.Hex("4A5A3A"), 0.025f, -0.35f, 2.55f, -0.1f, 2.62f);
            for (int k = 0; k < 3; k++) p.Line(Rgba.Hex("4A5A3A"), 0.018f, -0.3f + k * 0.09f, 2.52f, -0.28f + k * 0.09f, 2.66f);
            if (happy)
            {
                foreach (float x in new[] { -0.16f, 0.16f }) p.InkLine(0.04f, x - 0.09f, 2.33f, x, 2.4f, x + 0.09f, 2.33f);
                foreach (float x in new[] { -0.28f, 0.28f }) p.Fill(Sd.Ellipse(x, 2.18f, 0.08f, 0.05f), Rgba.Hex("FF8FB8", 0.8f));
                p.Shape(Sd.Intersect(Sd.Ellipse(0, 2.12f, 0.2f, 0.14f), Sd.Box(0, 2.04f, 0.3f, 0.08f)), Rgba.Hex("7A2A3A"), 0.03f);
            }
            else
            {
                HW_Eye(p, -0.16f, 2.35f, 0.11f, 0.2f, -0.3f, Rgba.Hex("F4F7D8"));
                HW_Eye(p, 0.17f, 2.37f, 0.08f, -0.4f, 0.4f, Rgba.Hex("F4F7D8"));
                p.Shape(Sd.Ellipse(0.02f, 2.08f, 0.14f, 0.1f), Rgba.Hex("3A1E2A"), 0.03f);
                p.Fill(Sd.Box(-0.04f, 2.15f, 0.03f, 0.03f), Rgba.White);
            }
            return new DoodleArt(happy ? "hw_zombie_head_happy" : "hw_zombie_head", p);
        }

        /// <summary>Braccio destro dello zombie (attacco alla spalla in 0,0), teso in avanti; si può staccare.</summary>
        public static DoodleArt HW_ZombieArm()
        {
            var p = new DoodlePainter(-0.18f, -0.62f, 0.82f, 0.18f, 130) { Ink = 0.05f };
            Body(p, Sd.Capsule(0f, 0f, 0.5f, -0.34f, 0.13f), Rgba.Hex("A0705A"), Rgba.Hex("7E5242"));
            Body(p, Sd.Union(Sd.Circle(0.6f, -0.4f, 0.14f), Sd.Capsule(0.62f, -0.38f, 0.76f, -0.5f, 0.04f), Sd.Capsule(0.58f, -0.45f, 0.66f, -0.58f, 0.04f),
                Sd.Capsule(0.66f, -0.3f, 0.78f, -0.36f, 0.04f)), HW_Zombie, HW_ZombieShade);
            p.Line(Rgba.Hex("7E5242"), 0.025f, 0.38f, -0.2f, 0.42f, -0.4f);
            return new DoodleArt("hw_zombie_arm", p);
        }

        /// <summary>Cervello rosa (centro 0,0).</summary>
        public static DoodleArt HW_Brain()
        {
            var p = new DoodlePainter(-0.44f, -0.32f, 0.44f, 0.32f, 140) { Ink = 0.035f };
            Sdf brain = Sd.Union(Sd.Ellipse(-0.17f, 0.02f, 0.25f, 0.24f), Sd.Ellipse(0.17f, 0.02f, 0.25f, 0.24f), Sd.Ellipse(0, -0.1f, 0.35f, 0.16f));
            Body(p, brain, Rgba.Hex("FFA6C1"), Rgba.Hex("E57C9E"));
            p.Line(Rgba.Hex("D9638A"), 0.025f, 0f, 0.24f, 0f, -0.2f);
            p.Line(Rgba.Hex("D9638A"), 0.02f, -0.32f, 0.05f, -0.22f, 0.12f, -0.12f, 0.02f, -0.2f, -0.08f);
            p.Line(Rgba.Hex("D9638A"), 0.02f, 0.32f, 0.05f, 0.22f, 0.14f, 0.12f, 0.04f, 0.2f, -0.06f);
            return new DoodleArt("hw_brain", p);
        }

        /// <summary>Hamburger (centro 0,0).</summary>
        public static DoodleArt HW_Burger()
        {
            var p = new DoodlePainter(-0.42f, -0.32f, 0.42f, 0.34f, 140) { Ink = 0.035f };
            p.Shape(Sd.Box(0, -0.2f, 0.34f, 0.07f, 0.06f), Rgba.Hex("E2A95E"));
            p.Shape(Sd.Box(0, -0.08f, 0.37f, 0.07f, 0.06f), Rgba.Hex("6B3A1E"));
            p.Shape(Sd.Polygon(-0.38f, 0f, 0.38f, 0f, 0.3f, 0.06f, 0.2f, -0.02f, 0.1f, 0.06f, 0f, -0.02f, -0.1f, 0.06f, -0.2f, -0.02f, -0.3f, 0.06f), Rgba.Hex("7FBF4D"), 0.02f);
            Body(p, Sd.Intersect(Sd.Ellipse(0, 0.04f, 0.36f, 0.26f), Sd.Box(0, 0.18f, 0.4f, 0.15f)), Rgba.Hex("F0B562"), Rgba.Hex("D1913D"));
            foreach (var (x, y) in new[] { (-0.15f, 0.18f), (0.05f, 0.24f), (0.18f, 0.15f) }) p.Fill(Sd.Ellipse(x, y, 0.03f, 0.018f), Rgba.Hex("FFF3D6"));
            return new DoodleArt("hw_burger", p);
        }

        /// <summary>Broccolo (centro 0,0): visto da vicino somiglia un po' a un cervello verde.</summary>
        public static DoodleArt HW_Broccoli()
        {
            var p = new DoodlePainter(-0.4f, -0.42f, 0.4f, 0.4f, 140) { Ink = 0.035f };
            Body(p, Sd.Polygon(-0.1f, 0f, 0.1f, 0f, 0.07f, -0.38f, -0.07f, -0.38f), Rgba.Hex("B5D98A"), Rgba.Hex("8CB863"));
            Sdf crown = Sd.Union(Sd.Circle(-0.18f, 0.08f, 0.17f), Sd.Circle(0.18f, 0.08f, 0.17f), Sd.Circle(0, 0.2f, 0.18f), Sd.Circle(-0.05f, 0.02f, 0.15f), Sd.Circle(0.1f, 0.0f, 0.14f));
            Body(p, crown, HW_Green, HW_GreenDark);
            foreach (var (x, y) in new[] { (-0.2f, 0.12f), (0.15f, 0.15f), (0f, 0.25f), (0.05f, 0.05f), (-0.1f, 0.0f) }) p.Shape(Sd.Circle(x, y, 0.05f), Rgba.Hex("A3D873"), 0.012f);
            return new DoodleArt("hw_broccoli", p);
        }

        /// <summary>Fumetto rettangolare 9-slice (bianco, contorno a inchiostro): si allarga con le lettere.</summary>
        public static DoodleArt HW_Balloon()
        {
            var p = new DoodlePainter(-0.6f, -0.6f, 0.6f, 0.6f, 100) { Ink = 0.06f, Wobble = 0.006f };
            p.Shape(Sd.Box(0, 0, 0.52f, 0.52f, 0.26f), Rgba.White);
            return new DoodleArt("hw_balloon", p, 42);
        }

        /// <summary>Codina del fumetto (va dietro al fumetto: il bordo in alto resta coperto). Punta in (-0.35, -0.55).</summary>
        public static DoodleArt HW_BalloonTail()
        {
            var p = new DoodlePainter(-0.45f, -0.62f, 0.3f, 0.15f, 120) { Ink = 0.06f };
            p.Shape(Sd.Polygon(-0.2f, 0.1f, 0.2f, 0.1f, -0.35f, -0.55f), Rgba.White);
            return new DoodleArt("hw_balloon_tail", p);
        }

        /// <summary>Tessera di una lettera (0.6 x 0.6, centro 0,0).</summary>
        public static DoodleArt HW_Tile()
        {
            var p = new DoodlePainter(-0.35f, -0.36f, 0.35f, 0.34f, 150) { Ink = 0.035f };
            p.Shape(Sd.Box(0, -0.03f, 0.29f, 0.29f, 0.08f), Rgba.Hex("D9C9A0"));
            p.Shape(Sd.Box(0, 0f, 0.29f, 0.29f, 0.08f), Rgba.Hex("FFF3D6"));
            return new DoodleArt("hw_tile", p);
        }

        /// <summary>Pulsante tondo "annulla" con la freccia che torna indietro (centro 0,0).</summary>
        public static DoodleArt HW_Undo()
        {
            var p = new DoodlePainter(-0.32f, -0.32f, 0.32f, 0.32f, 160) { Ink = 0.035f };
            Body(p, Sd.Circle(0, 0, 0.27f), Rgba.Hex("FFB23F"), Rgba.Hex("E0901F"));
            var pts = new List<float>();
            for (int i = 0; i <= 12; i++) { double a = Math.PI * (0.9 - 1.5 * i / 12.0); pts.Add((float)Math.Cos(a) * 0.14f); pts.Add((float)Math.Sin(a) * 0.14f); }
            p.Line(Rgba.White, 0.05f, pts.ToArray());
            p.Fill(Sd.Polygon(-0.22f, 0.02f, -0.04f, 0.04f, -0.14f, 0.16f), Rgba.White);
            return new DoodleArt("hw_undo", p);
        }

        // ================= 250 TRICK OR TREAT! =================

        /// <summary>Facciata della casa di notte: parete di legno, finestra illuminata, vialetto (la porta è a parte, in 1.45, -0.95).</summary>
        public static DoodleArt HW_HouseBg()
        {
            var p = new DoodlePainter(-3.2f, -6.6f, 3.2f, 6.6f, 40) { Ink = 0.06f };
            HW_Sky(p, "160F38", "3A2466", 250);
            // casa: parete a doghe con il tetto
            Sdf wall = Sd.Box(0.6f, 0.4f, 3.0f, 2.9f);
            p.FillFunc(wall, (x, y) => ((int)Math.Floor((y + 10f) / 0.32f) % 2 == 0) ? Rgba.Hex("5E4B7A") : Rgba.Hex("54426E"));
            p.Shape(wall, Rgba.Clear);
            p.Shape(Sd.Polygon(-2.7f, 3.3f, 3.6f, 3.3f, 3.6f, 3.9f, -2.2f, 3.9f), Rgba.Hex("3A2B4A"));
            // finestra illuminata a sinistra
            Sdf win = Sd.Box(-1.2f, 1.6f, 0.62f, 0.55f, 0.05f);
            p.Fill(win, Rgba.Hex("FFD87A"));
            p.Shape(win, Rgba.Clear, 0.09f);
            p.Line(Rgba.Hex("3B2A1A"), 0.06f, -1.2f, 1.05f, -1.2f, 2.15f);
            p.Line(Rgba.Hex("3B2A1A"), 0.06f, -1.82f, 1.6f, -0.58f, 1.6f);
            p.Shape(Sd.Union(Sd.Circle(-1.35f, 1.45f, 0.14f), Sd.Box(-1.35f, 1.3f, 0.05f, 0.12f)), Rgba.Hex("2B2233"), 0.0f);   // sagoma di un gatto alla finestra
            // cornice della porta e gradino
            p.Shape(Sd.Box(1.45f, -0.9f, 0.86f, 1.48f, 0.04f), Rgba.Hex("3A2B4A"));
            Body(p, Sd.Box(1.45f, -2.55f, 1.1f, 0.18f, 0.05f), Rgba.Hex("8A8AA0"), Rgba.Hex("6A6A82"));
            // vialetto e prato
            p.FillFunc(Sd.Box(0, -4.7f, 3.3f, 2.0f), (x, y) => Rgba.Lerp(Rgba.Hex("243A2E"), Rgba.Hex("2F5038"), HW_Clamp((y + 4.6f) / 1.8f)));
            p.Fill(Sd.Polygon(0.6f, -2.72f, 2.3f, -2.72f, 2.9f, -6.7f, 0.0f, -6.7f), Rgba.Hex("7A6E8A"));
            for (int i = 0; i < 6; i++) p.Line(Rgba.Hex("5E5470"), 0.03f, 0.55f - i * 0.1f, -3.2f - i * 0.6f, 2.35f + i * 0.1f, -3.2f - i * 0.6f);
            // zucche ai lati della porta
            foreach (float x in new[] { 0.2f, 2.75f })
            {
                Body(p, Sd.Ellipse(x, -2.45f, 0.26f, 0.2f), HW_Orange, HW_OrangeDark);
                p.Fill(Sd.Polygon(x - 0.12f, -2.4f, x - 0.03f, -2.4f, x - 0.075f, -2.32f), Rgba.Hex("FFD84D"));
                p.Fill(Sd.Polygon(x + 0.03f, -2.4f, x + 0.12f, -2.4f, x + 0.075f, -2.32f), Rgba.Hex("FFD84D"));
                p.Line(Rgba.Hex("FFD84D"), 0.03f, x - 0.1f, -2.52f, x, -2.56f, x + 0.1f, -2.52f);
            }
            return new DoodleArt("hw_house_bg", p);
        }

        /// <summary>Porta chiusa (centro 0,0, 1.4 x 2.7) col batacchio a pipistrello.</summary>
        public static DoodleArt HW_Door()
        {
            var p = new DoodlePainter(-0.78f, -1.42f, 0.78f, 1.42f, 100) { Ink = 0.055f };
            Body(p, Sd.Box(0, 0, 0.7f, 1.35f, 0.05f), Rgba.Hex("C46A2C"), Rgba.Hex("9C4E1C"));
            foreach (float y in new[] { 0.6f, -0.55f }) p.Shape(Sd.Box(0, y, 0.48f, 0.42f, 0.05f), Rgba.Hex("B65F25"), 0.035f);
            p.Shape(Sd.Circle(0.5f, -0.05f, 0.07f), Rgba.Hex("FFD15A"), 0.025f);
            p.Fill(Sd.Polygon(-0.2f, 1.12f, -0.07f, 1.05f, 0f, 1.12f, 0.07f, 1.05f, 0.2f, 1.12f, 0f, 0.98f), Rgba.Hex("2B2233"));
            return new DoodleArt("hw_door", p);
        }

        /// <summary>Porta aperta: l'ingresso illuminato e il battente aperto (stesse misure della porta).</summary>
        public static DoodleArt HW_Doorway()
        {
            var p = new DoodlePainter(-0.78f, -1.42f, 0.78f, 1.42f, 100) { Ink = 0.055f };
            p.FillFunc(Sd.Box(0, 0, 0.7f, 1.35f, 0.05f), (x, y) => Rgba.Lerp(Rgba.Hex("E8A64A"), Rgba.Hex("FFE2A0"), HW_Clamp((y + 1.3f) / 2.6f)));
            p.Shape(Sd.Box(0, 0, 0.7f, 1.35f, 0.05f), Rgba.Clear);
            Body(p, Sd.Polygon(-0.7f, 1.35f, -0.45f, 1.2f, -0.45f, -1.2f, -0.7f, -1.35f), Rgba.Hex("C46A2C"), Rgba.Hex("9C4E1C"));
            return new DoodleArt("hw_doorway", p);
        }

        /// <summary>La vecchietta sulla porta (busto, centro 0,0): 0 sorride, 1 terrorizzata, 2 innamorata del carlino.</summary>
        public static DoodleArt HW_Lady(int mood)
        {
            var p = new DoodlePainter(-0.66f, -1.0f, 0.66f, 1.05f, 120) { Ink = 0.045f };
            // golfino viola e collana di perle
            Body(p, Sd.Union(Sd.Ellipse(0, -0.65f, 0.6f, 0.45f), Sd.Box(0, -0.85f, 0.6f, 0.15f)), Rgba.Hex("8E5BC6"), Rgba.Hex("6B48B0"));
            for (int i = -3; i <= 3; i++) p.Shape(Sd.Circle(i * 0.07f, -0.33f - Math.Abs(i) * -0.01f - (3 - Math.Abs(i)) * 0.015f, 0.03f), Rgba.White, 0.012f);
            // crocchia e testa
            Rgba hair = Rgba.Hex("D9D6E4");
            if (mood == 1)
                for (int i = -3; i <= 3; i++) p.Line(hair, 0.06f, i * 0.09f, 0.55f, i * 0.16f, 0.95f);   // capelli dritti dallo spavento
            else p.Shape(Sd.Circle(0, 0.75f, 0.18f), hair);
            Body(p, Sd.Ellipse(0, 0.2f, 0.38f, 0.44f), Rgba.Hex("F7C9A0"), Rgba.Hex("E0A57A"));
            p.Shape(Sd.Intersect(Sd.Ellipse(0, 0.3f, 0.42f, 0.42f), Sd.Box(0, 0.62f, 0.5f, 0.22f)), hair);
            // occhiali e occhi
            foreach (float x in new[] { -0.15f, 0.15f })
            {
                if (mood == 2)
                {
                    Sdf h = Sd.Union(Sd.Circle(x - 0.04f, 0.27f, 0.05f), Sd.Circle(x + 0.04f, 0.27f, 0.05f), Sd.Polygon(x - 0.09f, 0.25f, x + 0.09f, 0.25f, x, 0.14f));
                    p.Shape(h, Rgba.Hex("FF5C8A"), 0.015f);
                }
                else if (mood == 1) { p.Shape(Sd.Circle(x, 0.24f, 0.1f), Rgba.White, 0.02f); p.Fill(Sd.Circle(x, 0.24f, 0.03f), Ink); }
                else { p.Fill(Sd.Circle(x, 0.23f, 0.035f), Ink); p.Fill(Sd.Circle(x - 0.01f, 0.24f, 0.012f), Rgba.White); }
                p.Shape(Sd.Subtract(Sd.Circle(x, 0.23f, 0.13f), Sd.Circle(x, 0.23f, 0.1f)), Rgba.Hex("B8860B"), 0.0f);
            }
            p.Line(Rgba.Hex("B8860B"), 0.025f, -0.03f, 0.25f, 0.03f, 0.25f);
            p.Shape(Sd.Ellipse(0, 0.08f, 0.05f, 0.06f), Rgba.Hex("F2A889"), 0.02f);
            if (mood == 1) p.Shape(Sd.Ellipse(0, -0.1f, 0.11f, 0.13f), Rgba.Hex("5A1424"), 0.025f);
            else
            {
                p.Shape(Sd.Intersect(Sd.Ellipse(0, -0.04f, 0.14f, 0.1f), Sd.Box(0, -0.1f, 0.2f, 0.06f)), Rgba.Hex("C2415E"), 0.02f);
                foreach (float x in new[] { -0.24f, 0.24f }) p.Fill(Sd.Ellipse(x, 0.02f, 0.07f, 0.04f), Rgba.Hex("FF8FB8", mood == 2 ? 0.9f : 0.5f));
            }
            return new DoodleArt(mood == 0 ? "hw_lady" : mood == 1 ? "hw_lady_scared" : "hw_lady_love", p);
        }

        /// <summary>Campanello (centro 0,0) col pulsante luminoso.</summary>
        public static DoodleArt HW_Doorbell()
        {
            var p = new DoodlePainter(-0.24f, -0.32f, 0.24f, 0.32f, 160) { Ink = 0.035f };
            Body(p, Sd.Box(0, 0, 0.17f, 0.26f, 0.06f), Rgba.Hex("D8C3A0"), Rgba.Hex("B8A07A"));
            p.FillFunc(Sd.Circle(0, 0.03f, 0.2f), (x, y) => Rgba.Hex("FFE36B", 0.5f * (float)Math.Exp(-Math.Sqrt(x * x + (y - 0.03f) * (y - 0.03f)) * 12f)));
            p.Shape(Sd.Circle(0, 0.03f, 0.08f), Rgba.Hex("FFD84D"), 0.025f);
            return new DoodleArt("hw_doorbell", p);
        }

        /// <summary>Secchiello-zucca delle caramelle (centro 0,0) col manico.</summary>
        public static DoodleArt HW_Bucket()
        {
            var p = new DoodlePainter(-0.42f, -0.36f, 0.42f, 0.5f, 140) { Ink = 0.035f };
            p.Shape(Sd.Subtract(Sd.Ellipse(0, 0.15f, 0.34f, 0.3f), Sd.Ellipse(0, 0.15f, 0.29f, 0.25f)), Rgba.Hex("2B2233"), 0.0f);
            Body(p, Sd.Intersect(Sd.Ellipse(0, -0.05f, 0.36f, 0.3f), Sd.Box(0, -0.1f, 0.4f, 0.26f)), HW_Orange, HW_OrangeDark);
            p.Fill(Sd.Polygon(-0.18f, 0.0f, -0.06f, 0.0f, -0.12f, 0.1f), Rgba.Hex("2B2233"));
            p.Fill(Sd.Polygon(0.06f, 0.0f, 0.18f, 0.0f, 0.12f, 0.1f), Rgba.Hex("2B2233"));
            p.Line(Rgba.Hex("2B2233"), 0.03f, -0.16f, -0.12f, -0.05f, -0.17f, 0.05f, -0.12f, 0.16f, -0.17f);
            foreach (var (x, c) in new[] { (-0.15f, "FF5C8A"), (0.05f, "4FA3E0"), (0.2f, "FFD84D") }) p.Shape(Sd.Circle(x, 0.15f, 0.08f), Rgba.Hex(c), 0.02f);
            return new DoodleArt("hw_bucket", p);
        }

        /// <summary>Caramella incartata (chiara: si tinge di colori diversi), centro 0,0.</summary>
        public static DoodleArt HW_Candy()
        {
            var p = new DoodlePainter(-0.24f, -0.12f, 0.24f, 0.12f, 200) { Ink = 0.022f };
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Polygon(s * 0.08f, 0f, s * 0.2f, 0.08f, s * 0.2f, -0.08f), Rgba.Hex("F4F4F4"));
            p.Shape(Sd.Ellipse(0, 0, 0.1f, 0.075f), Rgba.White);
            p.Hatch(Sd.Ellipse(0, 0, 0.09f, 0.065f), Rgba.Hex("D8D8D8"), 0.06f, 0.02f, 50f);
            return new DoodleArt("hw_candy", p);
        }

        /// <summary>Tavoletta di cioccolato incartata (centro 0,0).</summary>
        public static DoodleArt HW_Chocolate()
        {
            var p = new DoodlePainter(-0.24f, -0.16f, 0.24f, 0.16f, 200) { Ink = 0.025f };
            p.Shape(Sd.Box(0, 0, 0.2f, 0.12f, 0.02f), Rgba.Hex("6B3A1E"));
            p.Shape(Sd.Box(0.06f, 0, 0.14f, 0.125f, 0.02f), Rgba.Hex("E8233F"));
            p.Fill(Sd.Box(0.06f, 0, 0.14f, 0.03f), Rgba.Hex("FFD15A"));
            return new DoodleArt("hw_chocolate", p);
        }

        /// <summary>Zucca vivente con le gambette (piedi in 0,0; centro della zucca in 0, 0.62).</summary>
        public static DoodleArt HW_PumpkinKid()
        {
            var p = new DoodlePainter(-0.75f, -0.06f, 0.75f, 1.38f, 120) { Ink = 0.045f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Shape(Sd.Capsule(s * 0.2f, 0.25f, s * 0.24f, 0.06f, 0.06f), Rgba.Hex("2B2233"));
                p.Shape(Sd.Ellipse(s * 0.28f, 0.05f, 0.12f, 0.06f), Rgba.Hex("2B2233"));
            }
            p.Shape(Sd.Capsule(0, 1.05f, 0.06f, 1.3f, 0.07f), Rgba.Hex("6E8B3D"));
            Sdf body = Sd.Union(Sd.Ellipse(-0.25f, 0.62f, 0.38f, 0.48f), Sd.Ellipse(0.25f, 0.62f, 0.38f, 0.48f), Sd.Ellipse(0, 0.62f, 0.4f, 0.52f));
            Body(p, body, HW_Orange, HW_OrangeDark);
            foreach (float x in new[] { -0.3f, 0.3f }) p.Line(HW_OrangeDark, 0.025f, x * 0.8f, 1.05f, x * 1.1f, 0.62f, x * 0.8f, 0.17f);
            // faccia da jack-o'-lantern simpatica
            foreach (float s in new[] { -1f, 1f }) p.Shape(Sd.Polygon(s * 0.08f, 0.72f, s * 0.3f, 0.72f, s * 0.19f, 0.9f), Rgba.Hex("FFD84D"), 0.02f);
            p.Shape(Sd.Polygon(-0.3f, 0.5f, 0.3f, 0.5f, 0.2f, 0.36f, 0.1f, 0.42f, 0f, 0.34f, -0.1f, 0.42f, -0.2f, 0.36f), Rgba.Hex("FFD84D"), 0.02f);
            return new DoodleArt("hw_pumpkin_kid", p);
        }

        /// <summary>
        /// Il lenzuolo gigante del fantasma che avvolge il mostro (coordinate dello zombie: piedi in 0,0): due buchi per
        /// gli occhi della zucca che brillano.
        /// </summary>
        public static DoodleArt HW_BigSheet()
        {
            var p = new DoodlePainter(-1.25f, 0.25f, 1.25f, 3.25f, 90) { Ink = 0.06f };
            var parts = new List<Sdf> { Sd.Ellipse(0, 2.3f, 0.75f, 0.85f), Sd.Polygon(-0.7f, 2.2f, 0.7f, 2.2f, 1.05f, 0.55f, -1.05f, 0.55f) };
            for (int i = 0; i < 6; i++) parts.Add(Sd.Circle(-0.88f + i * 0.35f, 0.52f, 0.17f));
            Sdf sheet = Sd.Union(parts.ToArray());
            Body(p, sheet, HW_Linen, HW_LinenShade);
            p.Line(HW_LinenShade, 0.04f, -0.5f, 1.8f, -0.7f, 0.7f);
            p.Line(HW_LinenShade, 0.04f, 0.45f, 1.7f, 0.65f, 0.75f);
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Ellipse(s * 0.25f, 2.42f, 0.13f, 0.16f), Rgba.Hex("2B2233"));
                p.Fill(Sd.Polygon(s * 0.17f, 2.36f, s * 0.33f, 2.36f, s * 0.25f, 2.5f), Rgba.Hex("FFD84D"));
            }
            p.Fill(Sd.Ellipse(0, 2.05f, 0.25f, 0.1f), Rgba.Hex("2B2233"));
            p.Fill(Sd.Box(0, 2.08f, 0.18f, 0.025f), Rgba.Hex("FFD84D"));
            return new DoodleArt("hw_big_sheet", p);
        }

        /// <summary>Cappello da strega (base in 0,0).</summary>
        public static DoodleArt HW_WitchHat()
        {
            var p = new DoodlePainter(-0.85f, -0.18f, 0.85f, 1.3f, 110) { Ink = 0.05f };
            HW_HatAt(p, 0f, 0f, 1.15f, Rgba.Hex("9B6FD6"));
            return new DoodleArt("hw_witch_hat", p);
        }

        /// <summary>Corona d'oro (base in 0,0).</summary>
        public static DoodleArt HW_Crown()
        {
            var p = new DoodlePainter(-0.42f, -0.08f, 0.42f, 0.48f, 160) { Ink = 0.035f };
            Sdf crown = Sd.Polygon(-0.34f, 0f, 0.34f, 0f, 0.36f, 0.38f, 0.18f, 0.2f, 0f, 0.42f, -0.18f, 0.2f, -0.36f, 0.38f);
            Body(p, crown, Rgba.Hex("FFD15A"), Rgba.Hex("E0A82A"));
            foreach (var (x, c) in new[] { (-0.18f, "E8233F"), (0f, "4FA3E0"), (0.18f, "7FBF4D") }) p.Shape(Sd.Circle(x, 0.1f, 0.045f), Rgba.Hex(c), 0.015f);
            return new DoodleArt("hw_crown", p);
        }
    }
}
