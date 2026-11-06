using System;
using System.Collections.Generic;

namespace Gotcha.EditorArt
{
    /// <summary>
    /// I disegni nuovi della 0.83 per "Pizza Chef": tre condimenti in più (fetta di prosciutto rosa con la striscia di
    /// grasso, rondella di salamino rossa coi puntini bianchi, quarto di carciofo verde), i loro disegni a gesso per la
    /// ricetta sulla lavagna e una lavagna "pulita" (senza i ghirigori in basso) su cui la ricetta sta su due righe.
    /// Pasta, sugo, mozzarella, ciotole e gli altri condimenti restano quelli del livello 32 (DoodleSprites4).
    /// Prefisso pz83_.
    /// </summary>
    public static partial class DoodleSprites
    {
        public static IEnumerable<DoodleArt> ArtPizza83() => new DoodleArt[]
        {
            Pz83_Topping("ham"), Pz83_Topping("salami"), Pz83_Topping("artichoke"),
            Pz83_Chalk("ham"), Pz83_Chalk("salami"), Pz83_Chalk("artichoke"),
            Pz83_Chalkboard(),
        };

        static readonly Rgba Pz83Ham = Rgba.Hex("F59AA6"), Pz83HamShade = Rgba.Hex("DB7484"), Pz83Fat = Rgba.Hex("FFF1EC");
        static readonly Rgba Pz83Salami = Rgba.Hex("C8323A"), Pz83SalamiShade = Rgba.Hex("9C2229"), Pz83SalamiFat = Rgba.Hex("FFE9E2");
        static readonly Rgba Pz83Artichoke = Rgba.Hex("8CC063"), Pz83ArtichokeShade = Rgba.Hex("5E9A42"), Pz83ArtichokeHeart = Rgba.Hex("E9EBA8");

        /// <summary>
        /// Un pezzo di condimento nuovo (centro 0,0, stessa scala di top_mushroom & co.): "ham" fetta ondulata di
        /// prosciutto con la striscia di grasso, "salami" rondella con i puntini di grasso, "artichoke" quarto di carciofo.
        /// </summary>
        public static DoodleArt Pz83_Topping(string kind)
        {
            var p = new DoodlePainter(-0.24f, -0.24f, 0.24f, 0.24f, 160) { Ink = 0.025f, Seed = kind.Length * 7 };
            switch (kind)
            {
                case "ham":
                {
                    // fetta un po' accartocciata: ellisse col bordo ondulato
                    Sdf slice = (x, y) => Sd.Ellipse(0f, 0f, 0.19f, 0.14f)(x, y) + 0.012f * (float)Math.Sin(Math.Atan2(y, x) * 7.0);
                    Body(p, slice, Pz83Ham, Pz83HamShade);
                    // striscia di grasso lungo un bordo (dentro la fetta)
                    Sdf fat = Sd.Intersect(Sd.Subtract(Sd.Ellipse(0f, 0f, 0.17f, 0.12f), Sd.Ellipse(-0.035f, 0.03f, 0.15f, 0.1f)), Sd.Box(0.05f, -0.07f, 0.16f, 0.08f));
                    p.Fill(fat, Pz83Fat);
                    p.Line(Rgba.Hex("E58796"), 0.012f, -0.1f, 0.04f, -0.03f, 0.07f, 0.05f, 0.05f);   // venatura
                    p.Shape(slice, Rgba.Clear);
                    break;
                }
                case "salami":
                {
                    Sdf round = Sd.Circle(0f, 0f, 0.13f);
                    Body(p, round, Pz83Salami, Pz83SalamiShade);
                    var rnd = new Random(5);
                    for (int i = 0; i < 9; i++)
                    {
                        double a = i * 2.39996, r = Math.Sqrt((i + 0.5) / 9.0) * 0.095;
                        float x = (float)(Math.Cos(a) * r), y = (float)(Math.Sin(a) * r);
                        float s = 0.014f + (float)rnd.NextDouble() * 0.009f;
                        p.Fill(Sd.Ellipse(x, y, s * 1.2f, s), Pz83SalamiFat, false);
                    }
                    p.Shape(Sd.Subtract(Sd.Circle(0f, 0f, 0.13f), Sd.Circle(0f, 0f, 0.112f)), Rgba.Hex("7E1A20"), 0.012f);   // pelle
                    p.Shape(round, Rgba.Clear);
                    break;
                }
                default:   // carciofo: spicchio a goccia rovesciata, foglie verdi fuori, cuore chiaro con le venature dentro
                {
                    Sdf outer = Sd.Union(
                        Sd.Ellipse(0f, 0.04f, 0.15f, 0.13f),
                        Sd.Polygon(-0.135f, 0.02f, 0.135f, 0.02f, 0.025f, -0.18f, -0.025f, -0.18f),
                        Sd.Polygon(-0.15f, 0.08f, -0.11f, 0.2f, -0.06f, 0.14f),   // punte delle foglie
                        Sd.Polygon(-0.05f, 0.15f, 0f, 0.22f, 0.05f, 0.15f),
                        Sd.Polygon(0.06f, 0.14f, 0.11f, 0.2f, 0.15f, 0.08f));
                    Body(p, outer, Pz83Artichoke, Pz83ArtichokeShade);
                    Sdf heart = Sd.Union(Sd.Ellipse(0f, 0.035f, 0.095f, 0.085f), Sd.Polygon(-0.085f, 0.02f, 0.085f, 0.02f, 0f, -0.13f));
                    p.Shape(heart, Pz83ArtichokeHeart, 0.014f);
                    foreach (float tx in new[] { -0.06f, -0.02f, 0.02f, 0.06f })   // venature a ventaglio dal fondo
                        p.Line(Rgba.Hex("C3C77A"), 0.01f, 0f, -0.1f, tx * 1.1f, 0.09f);
                    p.Line(Pz83ArtichokeShade, 0.012f, -0.13f, 0.0f, -0.095f, 0.1f);   // bordo della foglia esterna
                    p.Line(Pz83ArtichokeShade, 0.012f, 0.13f, 0.0f, 0.095f, 0.1f);
                    p.Shape(outer, Rgba.Clear);
                    break;
                }
            }
            return new DoodleArt("pz83_top_" + kind, p);
        }

        /// <summary>Disegno a gesso di un ingrediente nuovo (stesso riquadro di chalk_tomato & co.).</summary>
        public static DoodleArt Pz83_Chalk(string id)
        {
            var p = new DoodlePainter(-0.32f, -0.3f, 0.32f, 0.32f, 160) { Wobble = 0.012f };
            switch (id)
            {
                case "ham":
                    ChalkOval(p, 0f, 0f, 0.24f, 0.17f);
                    p.Line(Chalk, 0.02f, -0.19f, -0.05f, -0.1f, -0.1f, 0f, -0.115f, 0.1f, -0.1f, 0.19f, -0.05f);   // la striscia di grasso
                    p.Line(Chalk, 0.016f, -0.09f, 0.05f, -0.02f, 0.08f, 0.06f, 0.06f);   // venatura
                    break;
                case "salami":
                    ChalkCircle(p, 0f, 0f, 0.21f);
                    foreach (float[] d in new[] { new[] { -0.08f, 0.07f }, new[] { 0.07f, 0.09f }, new[] { 0.0f, -0.02f }, new[] { -0.09f, -0.09f }, new[] { 0.09f, -0.08f } })
                        ChalkCircle(p, d[0], d[1], 0.022f);
                    break;
                default:   // carciofo: goccia rovesciata con le punte delle foglie, cuore e venature
                    p.Line(Chalk, 0.024f, 0f, -0.27f, -0.2f, -0.02f, -0.21f, 0.12f, -0.15f, 0.27f, -0.08f, 0.18f, 0f, 0.29f, 0.08f, 0.18f, 0.15f, 0.27f, 0.21f, 0.12f, 0.2f, -0.02f, 0f, -0.27f);
                    p.Line(Chalk, 0.018f, 0f, -0.18f, -0.12f, 0.0f, -0.1f, 0.12f, 0f, 0.15f, 0.1f, 0.12f, 0.12f, 0.0f, 0f, -0.18f);
                    p.Line(Chalk, 0.014f, 0f, -0.14f, -0.04f, 0.1f);
                    p.Line(Chalk, 0.014f, 0f, -0.14f, 0.04f, 0.1f);
                    break;
            }
            return new DoodleArt("pz83_chalk_" + id, p);
        }

        /// <summary>
        /// Come "chalkboard" (stesse misure e cornice, titolo con la riga), ma senza cuoricino e stelline in basso: la
        /// ricetta della 0.83 ha 4-5 ingredienti e va su due righe, fino quasi al bordo.
        /// </summary>
        public static DoodleArt Pz83_Chalkboard()
        {
            var p = new DoodlePainter(-1.35f, -0.9f, 1.35f, 0.9f, 110) { Ink = 0.05f };
            Body(p, Sd.Box(0, 0, 1.28f, 0.84f, 0.05f), Rgba.Hex("A0663A"), Rgba.Hex("7A4A26"));
            Sdf board = Sd.Box(0, 0.02f, 1.13f, 0.7f);
            p.FillFunc(board, (x, y) => Rgba.Lerp(Rgba.Hex("2F4A3E"), Rgba.Hex("26392F"), Clamp(Noise.Value(x * 4f, y * 4f))));
            p.Hatch(board, Rgba.Hex("FFFFFF", 0.04f), 0.15f, 0.06f, 20f);
            p.Line(Chalk, 0.02f, -0.95f, 0.55f, 0.95f, 0.55f);
            p.Shape(board, Rgba.Clear, 0.03f);
            p.Shape(Sd.Box(0.75f, -0.78f, 0.18f, 0.04f, 0.02f), Rgba.White, 0.015f);   // gessetto
            return new DoodleArt("pz83_chalkboard", p);
        }
    }
}
