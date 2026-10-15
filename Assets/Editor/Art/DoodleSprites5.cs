using System;

namespace Gotcha.EditorArt
{
    /// <summary>Disegni dei livelli 36-40 (labirinto con la vespa, mele, biglia, tasti colorati).</summary>
    public static partial class DoodleSprites
    {
        /// <summary>Labirinto aperto della vespa: quattro corridoi larghi e qualche pilastro dietro cui ripararsi.</summary>
        public static readonly MazeSeg[] WaspMaze =
        {
            new MazeSeg(-2.75f, -3.5f, 2.75f, -3.5f, NeonCyan),
            new MazeSeg(-2.75f, 3.5f, 2.75f, 3.5f, NeonCyan),
            new MazeSeg(-2.75f, -3.5f, -2.75f, 3.5f, NeonCyan),
            new MazeSeg(2.75f, -3.5f, 2.75f, 3.5f, NeonCyan),
            new MazeSeg(-2.75f, -1.75f, 1.55f, -1.75f, NeonMagenta),
            new MazeSeg(-1.55f, 0.05f, 2.75f, 0.05f, NeonYellow),
            new MazeSeg(-2.75f, 1.85f, 1.55f, 1.85f, NeonMagenta),
            new MazeSeg(0.0f, -3.5f, 0.0f, -2.75f, NeonYellow),       // pilastri: la vespa ci sbatte contro
            new MazeSeg(0.35f, -1.75f, 0.35f, -0.95f, NeonCyan),
            new MazeSeg(-0.35f, 0.05f, -0.35f, 0.85f, NeonYellow),
            new MazeSeg(0.4f, 1.85f, 0.4f, 2.65f, NeonCyan),
        };

        public static DoodleArt WaspMazeNeon() { return NeonMaze("maze_wasp", WaspMaze); }

        /// <summary>Melo (centro della chioma in 0,0; tronco fino a y -3.6).</summary>
        public static DoodleArt AppleTree()
        {
            var p = new DoodlePainter(-2.6f, -3.7f, 2.6f, 1.75f, 60) { Ink = 0.07f };
            Body(p, Sd.Polygon(-0.3f, -3.6f, 0.3f, -3.6f, 0.22f, -0.6f, -0.22f, -0.6f), Rgba.Hex("8A5A3B"), Rgba.Hex("6B4129"));
            p.Shape(Sd.Polyline(0.1f, 0f, -1.2f, -0.7f, -0.6f), Rgba.Hex("8A5A3B"), 0.05f);
            Sdf crown = Sd.Union(Sd.Ellipse(0, 0.1f, 2.3f, 1.35f), Sd.Circle(-1.4f, -0.3f, 0.9f), Sd.Circle(1.4f, -0.25f, 0.95f), Sd.Circle(0.2f, 0.9f, 0.85f));
            Body(p, crown, Leaf, LeafShade, LeafShade.WithAlpha(0.4f), -0.15f, 0.2f);
            return new DoodleArt("apple_tree", p);
        }

        /// <summary>Cesto di vimini (fondo in 0,0; bordo a y 0.75, largo 1.5).</summary>
        public static DoodleArt Basket()
        {
            var p = new DoodlePainter(-0.95f, -0.1f, 0.95f, 1.25f, 120) { Ink = 0.05f };
            p.Shape(Sd.Subtract(Sd.Ellipse(0, 0.75f, 0.62f, 0.45f), Sd.Ellipse(0, 0.75f, 0.52f, 0.37f)), Rgba.Hex("A0663A"), 0.035f);   // manico
            Sdf body = Sd.Polygon(-0.78f, 0.72f, 0.78f, 0.72f, 0.58f, 0f, -0.58f, 0f);
            Body(p, body, Rgba.Hex("D9A866"), Rgba.Hex("B5843F"));
            p.Hatch(body, Rgba.Hex("A0702F", 0.6f), 0.14f, 0.03f, 60f);
            p.Hatch(body, Rgba.Hex("A0702F", 0.6f), 0.14f, 0.03f, -60f);
            p.Shape(body, Rgba.Clear);
            Body(p, Sd.Box(0, 0.72f, 0.82f, 0.07f, 0.05f), Rgba.Hex("C9964E"), Rgba.Hex("A0702F"));
            return new DoodleArt("basket", p);
        }

        /// <summary>Tazza bersaglio della biglia (fondo in 0,0; apertura a y 0.8, larga 1.0).</summary>
        public static DoodleArt Cup()
        {
            var p = new DoodlePainter(-0.85f, -0.1f, 0.85f, 1.0f, 120) { Ink = 0.05f };
            p.Shape(Sd.Subtract(Sd.Ellipse(0.62f, 0.45f, 0.2f, 0.22f), Sd.Ellipse(0.62f, 0.45f, 0.1f, 0.12f)), Rgba.Hex("2F6FE0"), 0.03f);   // manico
            Sdf cup = Sd.Polygon(-0.55f, 0.8f, 0.55f, 0.8f, 0.42f, 0f, -0.42f, 0f);
            Body(p, cup, Rgba.Hex("6EC6FF"), Rgba.Hex("3E9BDB"));
            p.Fill(Sd.Box(0, 0.5f, 0.45f, 0.06f), Rgba.White.WithAlpha(0.7f));
            p.Shape(cup, Rgba.Clear);
            return new DoodleArt("cup", p);
        }

        /// <summary>Tasto quadrato bianco arrotondato (si tinge): per il gioco "copia la sequenza".</summary>
        public static DoodleArt Pad()
        {
            var p = new DoodlePainter(-0.8f, -0.8f, 0.8f, 0.8f, 100) { Ink = 0.06f };
            Body(p, Sd.Box(0, 0, 0.7f, 0.7f, 0.22f), Rgba.White, Rgba.Hex("D6D6DE"));
            p.Fill(Sd.Rotate(Sd.Ellipse(-0.35f, 0.38f, 0.18f, 0.07f), -0.35f, 0.38f, 20f), Rgba.White.WithAlpha(0.8f));
            return new DoodleArt("pad", p);
        }
    }
}
