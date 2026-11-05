using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.78 per il livello 15 della roadmap "The Honest Thief" (le stelline del diamante sono
    /// "sparkle_star", il lampo dell'allarme è "plain" tinto, i fumetti "bubble_box"): la sala del museo, il piedistallo
    /// con la targhetta d'ottone e il cuscino, la campana di vetro, il diamante, la sirena (base e cupola da tingere, i
    /// fasci di luce), il ladro (corpo con la mano tesa, cappello a parte per salutare), la guardia (corpo e braccio a
    /// parte che ruota sulla spalla), il tavolino col cestino dei volantini gratis e il cartellino appeso (la scritta
    /// FREE è un TextMesh). Nessuna scritta disegnata. Prefisso v78_b_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtV78B() => new DoodleArt[]
        {
            V78B_RoomBg(), V78B_Pedestal(), V78B_CaseGlass(), V78B_Diamond(), V78B_SirenBase(), V78B_SirenDome(),
            V78B_SirenBeams(), V78B_ThiefBody(), V78B_ThiefHat(), V78B_GuardBody(), V78B_GuardArm(), V78B_FreebieStand(),
            V78B_Card(),
        };

        // misure condivise col builder (coordinate locali dei disegni)
        /// <summary>Altezza del piano del piedistallo sopra i piedi (lì poggiano il cuscino e la campana di vetro).</summary>
        public const float V78BPedestalTop = 1.95f;
        /// <summary>Centro della targhetta d'ottone sul piedistallo (sopra i piedi).</summary>
        public const float V78BPlaqueY = 1.1f;
        /// <summary>Altezza interna della campana di vetro (la sirena sta in cima).</summary>
        public const float V78BGlassHeight = 1.75f;
        /// <summary>Mezza larghezza della campana di vetro.</summary>
        public const float V78BGlassHalfW = 0.68f;
        /// <summary>Gancio del cartellino sul bordo del cestino (sopra i piedi del tavolino).</summary>
        public const float V78BHookY = 1.5f;
        /// <summary>Mano tesa del ladro (sopra i piedi; il ladro guarda a sinistra).</summary>
        public const float V78BHandX = -0.62f, V78BHandY = 1.22f;
        /// <summary>Spalla della guardia (perno del braccio) e cima della testa per il cappello del ladro.</summary>
        public const float V78BGuardShoulderX = 0.24f, V78BGuardShoulderY = 1.68f, V78BThiefHeadTop = 2.22f;

        static readonly Rgba V78BSkin = Rgba.Hex("F2C9A0"), V78BSkinShade = Rgba.Hex("D9A57A");
        static readonly Rgba V78BInk = Rgba.Hex("2B2233"), V78BStripe = Rgba.Hex("F4F1EA"), V78BDark = Rgba.Hex("2E2A36");
        static readonly Rgba V78BNavy = Rgba.Hex("2F4B8C"), V78BNavyShade = Rgba.Hex("223768"), V78BGold = Rgba.Hex("E8B84A");
        static readonly Rgba V78BMarble = Rgba.Hex("ECE7E0"), V78BMarbleShade = Rgba.Hex("C9C1B6"), V78BVelvet = Rgba.Hex("B8323F");
        static readonly Rgba V78BGlass = Rgba.Hex("D8F1FA");

        /// <summary>
        /// La sala del museo, coordinate del mondo: parete verde scuro con una cornice a stucco, un quadro in alto a sinistra,
        /// un faretto che illumina la teca al centro, lo zoccolo di legno (y -3.45) e il pavimento a scacchi.
        /// </summary>
        public static DoodleArt V78B_RoomBg()
        {
            var p = new DoodlePainter(-4.5f, -7f, 4.5f, 7f, 50) { Ink = 0.06f, Wobble = 0.006f };
            p.FillFunc(Sd.Box(0f, 1.8f, 5f, 5.3f), (x, y) => Rgba.Lerp(Rgba.Hex("2F5A55"), Rgba.Hex("3E726B"), Math.Max(0f, Math.Min(1f, (y + 3f) / 8f))));
            // pannelli a stucco
            foreach (float cx in new[] { -2.1f, 0f, 2.1f })
                p.Shape(Sd.Box(cx, -2.5f, 0.9f, 0.7f, 0.05f), Rgba.Hex("356660"), 0.03f);
            p.Line(Rgba.Hex("C9A95E", 0.8f), 0.06f, -4.5f, -1.55f, 4.5f, -1.55f);
            // il faretto sulla teca
            p.Fill(Sd.Polygon(-0.25f, 4.6f, 0.25f, 4.6f, 1.25f, -3.4f, -1.25f, -3.4f), Rgba.Hex("FFF6C8", 0.12f), false);
            p.Fill(Sd.Polygon(-0.12f, 4.6f, 0.12f, 4.6f, 0.75f, -3.4f, -0.75f, -3.4f), Rgba.Hex("FFF6C8", 0.1f), false);
            // il quadro (paesaggio con la cornice dorata)
            p.Shape(Sd.Box(-1.25f, 1.65f, 0.95f, 0.72f, 0.04f), V78BGold, 0.05f);
            p.FillFunc(Sd.Box(-1.25f, 1.65f, 0.78f, 0.56f), (x, y) => Rgba.Lerp(Rgba.Hex("9FD4E8"), Rgba.Hex("E9F4F6"), Math.Max(0f, Math.Min(1f, (y - 1.1f) / 1.1f))));
            p.Fill(Sd.Intersect(Sd.Box(-1.25f, 1.65f, 0.78f, 0.56f), Sd.Union(Sd.Ellipse(-1.6f, 1.15f, 0.7f, 0.42f), Sd.Ellipse(-0.75f, 1.1f, 0.6f, 0.3f))), Rgba.Hex("7DBA6A"));
            p.Fill(Sd.Circle(-0.8f, 1.95f, 0.13f), Rgba.Hex("FFD45A"));
            p.Shape(Sd.Box(-1.25f, 1.65f, 0.78f, 0.56f), Rgba.Clear, 0.035f);
            p.Shape(Sd.Box(-1.25f, 0.82f, 0.28f, 0.07f, 0.02f), V78BGold, 0.025f);
            // zoccolo e pavimento a scacchi
            p.Shape(Sd.Box(0f, -3.4f, 5f, 0.12f), Rgba.Hex("6B4A34"), 0.04f);
            p.FillFunc(Sd.Box(0f, -5.3f, 5f, 1.78f), (x, y) =>
            {
                float u = x + (y + 3.52f) * 0.55f, v = y + 3.52f;
                int cell = ((int)Math.Floor(u / 0.6f) + (int)Math.Floor(v / 0.42f)) & 1;
                Rgba c = cell == 0 ? Rgba.Hex("E9E2D3") : Rgba.Hex("B9AE9A");
                return Rgba.Lerp(c, Rgba.Hex("8F8574"), Math.Max(0f, Math.Min(0.5f, (-3.6f - y) / 3f)));
            });
            return new DoodleArt("v78_b_room_bg", p);
        }

        /// <summary>
        /// Il piedistallo di marmo (origine ai piedi, al centro): base larga, colonna, piano in y <see cref="V78BPedestalTop"/>
        /// col cuscino di velluto rosso; la targhetta d'ottone vuota in y <see cref="V78BPlaqueY"/> (la scritta è un TextMesh).
        /// </summary>
        public static DoodleArt V78B_Pedestal()
        {
            var p = new DoodlePainter(-0.85f, -0.12f, 0.85f, 2.2f, 100) { Ink = 0.05f };
            p.Fill(Sd.Ellipse(0f, 0f, 0.8f, 0.09f), new Rgba(0f, 0f, 0f, 0.2f));
            Body(p, Sd.Box(0f, 0.1f, 0.75f, 0.1f, 0.03f), V78BMarble, V78BMarbleShade);
            Body(p, Sd.Box(0f, 1.0f, 0.6f, 0.82f, 0.03f), V78BMarble, V78BMarbleShade);
            p.Line(V78BMarbleShade, 0.02f, -0.35f, 0.3f, -0.2f, 0.75f, -0.32f, 1.6f);
            p.Line(V78BMarbleShade, 0.015f, 0.3f, 1.75f, 0.42f, 1.4f);
            Body(p, Sd.Box(0f, 1.88f, 0.72f, 0.08f, 0.03f), V78BMarble, V78BMarbleShade);
            // il cuscino
            p.Shape(Sd.Ellipse(0f, V78BPedestalTop + 0.06f, 0.42f, 0.09f), V78BVelvet, 0.035f);
            p.Line(Rgba.Hex("E2707A", 0.8f), 0.025f, -0.25f, V78BPedestalTop + 0.1f, 0.15f, V78BPedestalTop + 0.11f);
            // la targhetta d'ottone (vuota)
            p.Shape(Sd.Box(0f, V78BPlaqueY, 0.48f, 0.19f, 0.04f), Rgba.Hex("D9AE52"), 0.035f);
            p.Shape(Sd.Box(0f, V78BPlaqueY, 0.41f, 0.13f, 0.02f), Rgba.Hex("F0CF7A"), 0.012f);
            foreach (float s in new[] { -1f, 1f }) p.Fill(Sd.Circle(s * 0.44f, V78BPlaqueY, 0.02f), Rgba.Hex("8A6A2A"));
            return new DoodleArt("v78_b_pedestal", p);
        }

        /// <summary>
        /// La campana di vetro (origine al centro della base): cornice di metallo sopra e sotto, vetro azzurrino quasi
        /// trasparente con riflessi. Alta <see cref="V78BGlassHeight"/>, mezza larghezza <see cref="V78BGlassHalfW"/>.
        /// </summary>
        public static DoodleArt V78B_CaseGlass()
        {
            float h = V78BGlassHeight, w = V78BGlassHalfW;
            var p = new DoodlePainter(-w - 0.1f, -0.08f, w + 0.1f, h + 0.1f, 100) { Ink = 0.045f, Wobble = 0.004f };
            Sdf pane = Sd.Box(0f, h * 0.5f, w, h * 0.5f, 0.03f);
            p.Fill(pane, V78BGlass.WithAlpha(0.22f));
            p.Shape(pane, Rgba.Clear, 0.04f);
            p.Line(Rgba.White.WithAlpha(0.8f), 0.05f, -w + 0.16f, h - 0.25f, -w + 0.16f, 0.45f);
            p.Line(Rgba.White.WithAlpha(0.6f), 0.03f, -w + 0.3f, h - 0.3f, -w + 0.3f, h - 0.75f);
            p.Line(Rgba.White.WithAlpha(0.5f), 0.03f, w - 0.18f, 0.6f, w - 0.18f, 0.35f);
            p.Line(V78BGlass.WithAlpha(0.7f), 0.02f, w * 0.55f, 0.02f, w * 0.55f, h - 0.02f);
            // cornici
            Body(p, Sd.Box(0f, h, w + 0.05f, 0.06f, 0.02f), Rgba.Hex("B8BCC8"), Rgba.Hex("8D92A3"));
            Body(p, Sd.Box(0f, 0f, w + 0.05f, 0.05f, 0.02f), Rgba.Hex("B8BCC8"), Rgba.Hex("8D92A3"));
            return new DoodleArt("v78_b_case_glass", p);
        }

        /// <summary>Il diamante (taglio a brillante, origine al centro): corona a trapezio, padiglione a punta, faccette.</summary>
        public static DoodleArt V78B_Diamond()
        {
            var p = new DoodlePainter(-0.4f, -0.36f, 0.4f, 0.3f, 160) { Ink = 0.035f };
            Sdf crown = Sd.Polygon(-0.32f, 0.08f, -0.17f, 0.24f, 0.17f, 0.24f, 0.32f, 0.08f);
            Sdf pav = Sd.Polygon(-0.32f, 0.08f, 0.32f, 0.08f, 0f, -0.32f);
            p.FillFunc(Sd.Union(crown, pav), (x, y) => Rgba.Lerp(Rgba.Hex("7FD3F5"), Rgba.Hex("E6FAFF"), Math.Max(0f, Math.Min(1f, (y + 0.32f) / 0.56f + x * 0.4f))));
            p.Fill(Sd.Polygon(-0.17f, 0.24f, -0.06f, 0.08f, -0.32f, 0.08f), Rgba.White.WithAlpha(0.55f));
            p.Fill(Sd.Polygon(0f, 0.08f, 0.15f, 0.08f, 0f, -0.32f), Rgba.Hex("4FB4E0", 0.55f));
            p.InkLine(0.018f, -0.32f, 0.08f, 0.32f, 0.08f);
            p.InkLine(0.015f, -0.17f, 0.24f, -0.06f, 0.08f, 0.06f, 0.24f);
            p.InkLine(0.015f, 0.17f, 0.24f, 0.06f, 0.08f);
            p.InkLine(0.015f, -0.06f, 0.08f, 0f, -0.32f, 0.06f, 0.08f);
            p.InkLine(0.015f, -0.2f, 0.08f, 0f, -0.32f);
            p.InkLine(0.015f, 0.2f, 0.08f, 0f, -0.32f);
            p.Shape(Sd.Union(crown, pav), Rgba.Clear, 0.035f);
            p.Fill(Sd.Circle(-0.12f, 0.17f, 0.03f), Rgba.White);
            return new DoodleArt("v78_b_diamond", p);
        }

        /// <summary>La base della sirena (origine al centro della base, poggia sulla cornice della campana).</summary>
        public static DoodleArt V78B_SirenBase()
        {
            var p = new DoodlePainter(-0.3f, -0.05f, 0.3f, 0.14f, 120) { Ink = 0.035f };
            Body(p, Sd.Box(0f, 0.05f, 0.25f, 0.05f, 0.02f), Rgba.Hex("5A5E6E"), Rgba.Hex("3E4150"));
            return new DoodleArt("v78_b_siren_base", p);
        }

        /// <summary>La cupola della sirena (origine alla base della cupola): chiara, il livello la tinge (rosso spento, rosso acceso, verde).</summary>
        public static DoodleArt V78B_SirenDome()
        {
            var p = new DoodlePainter(-0.26f, -0.04f, 0.26f, 0.32f, 120) { Ink = 0.035f };
            Sdf dome = Sd.Intersect(Sd.Ellipse(0f, 0.02f, 0.2f, 0.27f), Sd.Box(0f, 0.16f, 0.25f, 0.15f));
            Body(p, dome, Rgba.Hex("F4F4F4"), Rgba.Hex("C8C8C8"));
            p.Fill(Sd.Ellipse(-0.07f, 0.17f, 0.04f, 0.06f), Rgba.White.WithAlpha(0.9f));
            return new DoodleArt("v78_b_siren_dome", p);
        }

        /// <summary>I fasci di luce della sirena (origine al centro della cupola): due coni opposti, morbidi, da tingere e ruotare.</summary>
        public static DoodleArt V78B_SirenBeams()
        {
            var p = new DoodlePainter(-1.6f, -0.6f, 1.6f, 0.6f, 50) { Ink = 0f, Wobble = 0f };
            foreach (float s in new[] { -1f, 1f })
            {
                p.Fill(Sd.Polygon(0f, 0.04f, s * 1.55f, 0.5f, s * 1.55f, -0.5f, 0f, -0.04f), Rgba.White.WithAlpha(0.35f), false);
                p.Fill(Sd.Polygon(0f, 0.02f, s * 1.3f, 0.25f, s * 1.3f, -0.25f, 0f, -0.02f), Rgba.White.WithAlpha(0.35f), false);
            }
            return new DoodleArt("v78_b_siren_beams", p);
        }

        /// <summary>
        /// Il ladro (origine ai piedi, guarda a SINISTRA verso la teca): pantaloni neri, maglia a righe bianche e nere,
        /// mascherina nera, sorrisetto e barba di un giorno; il braccio sinistro teso col palmo in su (mano in
        /// <see cref="V78BHandX"/>, <see cref="V78BHandY"/>), l'altro sul fianco. Il cappello è a parte.
        /// </summary>
        public static DoodleArt V78B_ThiefBody()
        {
            var p = new DoodlePainter(-0.85f, -0.12f, 0.7f, 2.32f, 100) { Ink = 0.05f };
            p.Fill(Sd.Ellipse(0f, 0f, 0.5f, 0.08f), new Rgba(0f, 0f, 0f, 0.2f));
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Box(s * 0.15f, 0.45f, 0.12f, 0.4f, 0.05f), V78BDark, Rgba.Hex("1E1B24"));
                p.Shape(Sd.Ellipse(s * 0.15f - 0.07f, 0.06f, 0.18f, 0.08f), Rgba.Hex("3A2F2A"), 0.04f);
            }
            // il braccio dietro (sul fianco)
            Body(p, Sd.Capsule(0.36f, 1.5f, 0.5f, 1.05f, 0.1f), V78BStripe, Rgba.Hex("D6D1C6"));
            p.Shape(Sd.Circle(0.42f, 0.98f, 0.09f), V78BSkin, 0.035f);
            // la maglia a righe
            Sdf torso = Sd.Box(0f, 1.25f, 0.36f, 0.42f, 0.15f);
            p.Fill(torso, V78BStripe);
            for (int i = 0; i < 5; i++)
            {
                float y = 0.92f + i * 0.17f;
                p.Fill(Sd.Intersect(torso, Sd.Box(0f, y, 0.5f, 0.045f)), V78BDark);
            }
            p.Fill(Sd.Crescent(torso, -0.06f, 0.06f), new Rgba(0f, 0f, 0f, 0.15f));
            p.Shape(torso, Rgba.Clear);
            // il braccio teso col palmo in su
            Sdf arm = Sd.Capsule(-0.28f, 1.52f, V78BHandX + 0.12f, V78BHandY + 0.02f, 0.1f);
            p.Fill(arm, V78BStripe);
            p.Fill(Sd.Intersect(arm, Sd.Box(-0.38f, 1.35f, 0.04f, 0.3f)), V78BDark);
            p.Shape(arm, Rgba.Clear, 0.045f);
            p.Shape(Sd.Union(Sd.Ellipse(V78BHandX, V78BHandY, 0.13f, 0.06f), Sd.Circle(V78BHandX - 0.11f, V78BHandY + 0.05f, 0.04f)), V78BSkin, 0.035f);
            // la testa
            p.Shape(Sd.Box(0f, 1.69f, 0.08f, 0.06f, 0.03f), V78BSkin, 0.035f);
            Sdf head = Sd.Ellipse(0f, 1.93f, 0.3f, 0.3f);
            Body(p, head, V78BSkin, V78BSkinShade);
            p.Fill(Sd.Intersect(head, Sd.Ellipse(0.02f, 1.77f, 0.26f, 0.13f)), Rgba.Hex("8A7A70", 0.35f));   // barba di un giorno
            // la mascherina con i buchi degli occhi (guarda a sinistra)
            Sdf mask = Sd.Intersect(Sd.Grow(head, 0.02f), Sd.Box(0f, 1.98f, 0.4f, 0.075f));
            p.Shape(mask, V78BDark, 0.02f);
            foreach (float ex in new[] { -0.15f, 0.06f })
            {
                p.Fill(Sd.Ellipse(ex, 1.98f, 0.055f, 0.04f), Rgba.White);
                p.Fill(Sd.Circle(ex - 0.025f, 1.98f, 0.022f), V78BInk);
            }
            p.Line(V78BDark, 0.025f, 0.28f, 2.0f, 0.36f, 2.07f, 0.4f, 1.98f);   // nodo della mascherina
            // sorrisetto furbo
            p.InkLine(0.03f, -0.17f, 1.8f, -0.08f, 1.76f, 0.02f, 1.79f);
            p.Fill(Sd.Ellipse(-0.27f, 1.9f, 0.04f, 0.04f), V78BSkinShade);   // naso di profilo
            return new DoodleArt("v78_b_thief_body", p);
        }

        /// <summary>Il cappello del ladro (coppola nera con la visiera a sinistra; origine al centro del bordo, sulla testa).</summary>
        public static DoodleArt V78B_ThiefHat()
        {
            var p = new DoodlePainter(-0.48f, -0.08f, 0.38f, 0.32f, 120) { Ink = 0.04f };
            Body(p, Sd.Union(Sd.Intersect(Sd.Ellipse(0f, 0.02f, 0.31f, 0.26f), Sd.Box(0f, 0.15f, 0.35f, 0.14f)), Sd.Box(0f, 0.03f, 0.3f, 0.04f, 0.02f)), V78BDark, Rgba.Hex("1E1B24"));
            p.Shape(Sd.Ellipse(-0.3f, 0.02f, 0.15f, 0.04f), Rgba.Hex("1E1B24"), 0.03f);
            p.Line(Rgba.Hex("55505E"), 0.02f, -0.15f, 0.2f, 0.1f, 0.24f);
            return new DoodleArt("v78_b_thief_hat", p);
        }

        /// <summary>
        /// La guardia (origine ai piedi, guarda a DESTRA verso la teca): divisa blu, berretto col distintivo, baffoni,
        /// cinturone; il braccio davanti è a parte (perno sulla spalla in <see cref="V78BGuardShoulderX"/>,
        /// <see cref="V78BGuardShoulderY"/>), quello dietro è disegnato lungo il fianco.
        /// </summary>
        public static DoodleArt V78B_GuardBody()
        {
            var p = new DoodlePainter(-0.6f, -0.12f, 0.6f, 2.62f, 100) { Ink = 0.05f };
            p.Fill(Sd.Ellipse(0f, 0f, 0.5f, 0.08f), new Rgba(0f, 0f, 0f, 0.2f));
            foreach (float s in new[] { -1f, 1f })
            {
                Body(p, Sd.Box(s * 0.15f, 0.45f, 0.13f, 0.42f, 0.05f), V78BNavyShade, Rgba.Hex("19284D"));
                p.Shape(Sd.Ellipse(s * 0.15f + 0.06f, 0.06f, 0.18f, 0.08f), Rgba.Hex("1E1B24"), 0.04f);
            }
            Body(p, Sd.Capsule(-0.3f, 1.55f, -0.36f, 0.95f, 0.11f), V78BNavyShade, Rgba.Hex("19284D"));
            Sdf torso = Sd.Box(0f, 1.28f, 0.4f, 0.45f, 0.16f);
            Body(p, torso, V78BNavy, V78BNavyShade);
            p.Shape(Sd.Box(0f, 0.93f, 0.4f, 0.06f, 0.02f), Rgba.Hex("1E1B24"), 0.03f);
            p.Shape(Sd.Box(0.06f, 0.93f, 0.07f, 0.05f, 0.01f), V78BGold, 0.02f);
            p.Shape(Sd.Polygon(0.12f, 1.5f, 0.24f, 1.5f, 0.24f, 1.36f, 0.18f, 1.3f, 0.12f, 1.36f), V78BGold, 0.02f);   // distintivo
            foreach (float y in new[] { 1.2f, 1.05f }) p.Fill(Sd.Circle(-0.02f, y, 0.025f), V78BGold);
            // la testa
            p.Shape(Sd.Box(0f, 1.76f, 0.09f, 0.06f, 0.03f), V78BSkin, 0.035f);
            Sdf head = Sd.Ellipse(0.02f, 2.0f, 0.29f, 0.3f);
            Body(p, head, V78BSkin, V78BSkinShade);
            p.Fill(Sd.Circle(0.12f, 2.04f, 0.035f), V78BInk);
            p.InkLine(0.03f, 0.05f, 2.14f, 0.2f, 2.12f);   // sopracciglio severo
            p.Fill(Sd.Ellipse(0.3f, 1.97f, 0.05f, 0.05f), V78BSkinShade);
            p.Shape(Sd.Union(Sd.Ellipse(0.15f, 1.86f, 0.15f, 0.05f), Sd.Ellipse(0.27f, 1.84f, 0.06f, 0.04f)), Rgba.Hex("5A4636"), 0.025f);   // baffoni
            // il berretto
            p.Shape(Sd.Ellipse(0.24f, 2.2f, 0.2f, 0.045f), Rgba.Hex("1E1B24"), 0.03f);
            Body(p, Sd.Box(0f, 2.33f, 0.31f, 0.13f, 0.06f), V78BNavy, V78BNavyShade);
            p.Shape(Sd.Box(0f, 2.23f, 0.31f, 0.035f, 0.01f), Rgba.Hex("1E1B24"), 0.02f);
            p.Shape(Sd.Circle(0.12f, 2.34f, 0.05f), V78BGold, 0.02f);
            return new DoodleArt("v78_b_guard_body", p);
        }

        /// <summary>Il braccio davanti della guardia (origine sulla spalla, pende verso il basso; la mano aperta in fondo).</summary>
        public static DoodleArt V78B_GuardArm()
        {
            var p = new DoodlePainter(-0.2f, -0.82f, 0.2f, 0.14f, 120) { Ink = 0.045f };
            Body(p, Sd.Capsule(0f, 0f, 0f, -0.55f, 0.1f), V78BNavy, V78BNavyShade);
            p.Shape(Sd.Union(Sd.Ellipse(0f, -0.66f, 0.09f, 0.12f), Sd.Ellipse(0.07f, -0.6f, 0.035f, 0.06f)), V78BSkin, 0.035f);
            p.InkLine(0.012f, -0.03f, -0.7f, -0.03f, -0.76f);
            p.InkLine(0.012f, 0.02f, -0.7f, 0.02f, -0.77f);
            return new DoodleArt("v78_b_guard_arm", p);
        }

        /// <summary>
        /// Il tavolino tondo col cestino dei volantini gratis e qualche penna (origine ai piedi). Il gancio del cartellino è
        /// sul bordo davanti del cestino, in y <see cref="V78BHookY"/>.
        /// </summary>
        public static DoodleArt V78B_FreebieStand()
        {
            var p = new DoodlePainter(-0.5f, -0.1f, 0.5f, 1.95f, 100) { Ink = 0.045f };
            p.Fill(Sd.Ellipse(0f, 0f, 0.36f, 0.06f), new Rgba(0f, 0f, 0f, 0.2f));
            Rgba wood = Rgba.Hex("A8744A"), woodShade = Rgba.Hex("7F5535");
            Body(p, Sd.Box(0f, 0.6f, 0.05f, 0.55f, 0.02f), wood, woodShade);
            Body(p, Sd.Ellipse(0f, 0.05f, 0.28f, 0.06f), wood, woodShade);
            Body(p, Sd.Box(0f, 1.15f, 0.42f, 0.05f, 0.02f), wood, woodShade);
            // volantini e penne che spuntano dal cestino
            p.Shape(Sd.Rotate(Sd.Box(-0.14f, 1.62f, 0.13f, 0.18f, 0.01f), -0.14f, 1.62f, 12f), Rgba.Hex("F6E7A8"), 0.025f);
            p.Shape(Sd.Rotate(Sd.Box(0.06f, 1.66f, 0.13f, 0.18f, 0.01f), 0.06f, 1.66f, -6f), Rgba.Hex("BFE3F2"), 0.025f);
            p.Line(Rgba.Hex("2B2233", 0.5f), 0.015f, 0.0f, 1.72f, 0.12f, 1.73f);
            p.Line(Rgba.Hex("2B2233", 0.5f), 0.015f, 0.0f, 1.66f, 0.1f, 1.67f);
            p.Shape(Sd.Rotate(Sd.Box(0.22f, 1.68f, 0.025f, 0.2f, 0.02f), 0.22f, 1.68f, -18f), Rgba.Hex("D8473F"), 0.02f);
            p.Shape(Sd.Rotate(Sd.Box(0.3f, 1.62f, 0.025f, 0.17f, 0.02f), 0.3f, 1.62f, -30f), Rgba.Hex("3F7BD8"), 0.02f);
            // il cestino di vimini
            Sdf basket = Sd.Polygon(-0.36f, 1.56f, 0.36f, 1.56f, 0.28f, 1.2f, -0.28f, 1.2f);
            p.Fill(basket, Rgba.Hex("D7A66A"));
            p.Hatch(basket, Rgba.Hex("A87A44"), 0.08f, 0.02f, 45f);
            p.Hatch(basket, Rgba.Hex("A87A44"), 0.08f, 0.02f, -45f);
            p.Shape(basket, Rgba.Clear);
            p.Shape(Sd.Box(0f, 1.56f, 0.38f, 0.035f, 0.02f), Rgba.Hex("C08A50"), 0.03f);
            // il gancetto
            p.Shape(Sd.Circle(0f, V78BHookY, 0.03f), Rgba.Hex("9A9AA6"), 0.02f);
            return new DoodleArt("v78_b_freebie_stand", p);
        }

        /// <summary>
        /// Il cartellino di cartoncino (origine al centro): bordo a inchiostro, foro in alto e lo spago a triangolo che sale
        /// fino al gancio (y 0.4). Volutamente sobrio: la scritta FREE (TextMesh) è dello stesso inchiostro del resto.
        /// </summary>
        public static DoodleArt V78B_Card()
        {
            var p = new DoodlePainter(-0.48f, -0.3f, 0.48f, 0.44f, 120) { Ink = 0.035f };
            p.Line(Rgba.Hex("7A5A3E"), 0.02f, -0.12f, 0.2f, 0f, 0.4f, 0.12f, 0.2f);
            p.Shape(Sd.Box(0f, 0f, 0.42f, 0.24f, 0.05f), Rgba.Hex("F7F0DE"), 0.035f);
            p.Shape(Sd.Box(0f, 0f, 0.36f, 0.18f, 0.03f), Rgba.Hex("F7F0DE"), 0.012f);
            p.Shape(Sd.Circle(0f, 0.2f, 0.025f), Rgba.Hex("8F8574"), 0.012f);
            return new DoodleArt("v78_b_card", p);
        }
    }
}
