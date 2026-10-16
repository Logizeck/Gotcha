using System;

namespace Gotcha.EditorArt
{
    /// <summary>Labirinti delle varianti (livelli 41-60), generati una volta (griglia 5x7, percorso verificato) e fissati qui.</summary>
    public static partial class DoodleSprites
    {
        /// <summary>Labirinto 2 (griglia 5x7): dalla cella in basso a sinistra al centro.</summary>
        public static readonly MazeSeg[] Maze2 =
        {
            new MazeSeg(-2.75f, -3.50f, 2.75f, -3.50f, NeonCyan),
            new MazeSeg(-2.75f, 3.50f, 2.75f, 3.50f, NeonCyan),
            new MazeSeg(-2.75f, -3.50f, -2.75f, 3.50f, NeonCyan),
            new MazeSeg(2.75f, -3.50f, 2.75f, 3.50f, NeonCyan),
            new MazeSeg(-2.75f, -2.50f, -1.65f, -2.50f, NeonMagenta),
            new MazeSeg(-0.55f, -2.50f, 1.65f, -2.50f, NeonYellow),
            new MazeSeg(0.55f, -1.50f, 2.75f, -1.50f, NeonMagenta),
            new MazeSeg(-1.65f, -0.50f, -0.55f, -0.50f, NeonYellow),
            new MazeSeg(-2.75f, 0.50f, -1.65f, 0.50f, NeonMagenta),
            new MazeSeg(-0.55f, 0.50f, 1.65f, 0.50f, NeonYellow),
            new MazeSeg(0.55f, 1.50f, 1.65f, 1.50f, NeonMagenta),
            new MazeSeg(-1.65f, 2.50f, 0.55f, 2.50f, NeonYellow),
            new MazeSeg(-1.65f, -2.50f, -1.65f, -1.50f, NeonMagenta),
            new MazeSeg(-1.65f, 0.50f, -1.65f, 2.50f, NeonYellow),
            new MazeSeg(-0.55f, -3.50f, -0.55f, 1.50f, NeonMagenta),
            new MazeSeg(0.55f, -1.50f, 0.55f, -0.50f, NeonYellow),
            new MazeSeg(0.55f, 1.50f, 0.55f, 2.50f, NeonMagenta),
            new MazeSeg(1.65f, -0.50f, 1.65f, 0.50f, NeonYellow),
            new MazeSeg(1.65f, 1.50f, 1.65f, 2.50f, NeonMagenta),
        };
        /// <summary>Trappole 2: dalla cella in basso a sinistra a quella in alto a destra.</summary>
        public static readonly MazeSeg[] TrapMaze2 =
        {
            new MazeSeg(-2.75f, -3.50f, 2.75f, -3.50f, NeonCyan),
            new MazeSeg(-2.75f, 3.50f, 2.75f, 3.50f, NeonCyan),
            new MazeSeg(-2.75f, -3.50f, -2.75f, 3.50f, NeonCyan),
            new MazeSeg(2.75f, -3.50f, 2.75f, 3.50f, NeonCyan),
            new MazeSeg(-2.75f, -2.50f, -0.55f, -2.50f, NeonMagenta),
            new MazeSeg(-1.65f, -0.50f, 1.65f, -0.50f, NeonYellow),
            new MazeSeg(-1.65f, 0.50f, -0.55f, 0.50f, NeonMagenta),
            new MazeSeg(-2.75f, 1.50f, 1.65f, 1.50f, NeonYellow),
            new MazeSeg(-1.65f, 2.50f, 0.55f, 2.50f, NeonMagenta),
            new MazeSeg(1.65f, 2.50f, 2.75f, 2.50f, NeonYellow),
            new MazeSeg(-1.65f, -1.50f, -1.65f, 0.50f, NeonMagenta),
            new MazeSeg(-0.55f, -2.50f, -0.55f, -1.50f, NeonYellow),
            new MazeSeg(0.55f, -3.50f, 0.55f, -0.50f, NeonMagenta),
            new MazeSeg(0.55f, 0.50f, 0.55f, 2.50f, NeonYellow),
            new MazeSeg(1.65f, -2.50f, 1.65f, 0.50f, NeonMagenta),
        };
        public static readonly float[] TrapPits2 = { 0.26f, -3.22f, -1.36f, -0.78f, -2.48f, -1.0f, -1.1f, 1.24f, 1.36f, -0.22f, 2.46f, 2.22f };
        public static readonly float[] TrapLasers2 = { -1.65f, 0.58f, 1.42f };
        /// <summary>Buio 2: uscita in alto a sinistra dietro il cancello, chiave in un vicolo cieco.</summary>
        public static readonly MazeSeg[] DarkMaze2 =
        {
            new MazeSeg(-2.75f, -3.50f, 2.75f, -3.50f, NeonCyan),
            new MazeSeg(-2.75f, 3.50f, 2.75f, 3.50f, NeonCyan),
            new MazeSeg(-2.75f, -3.50f, -2.75f, 3.50f, NeonCyan),
            new MazeSeg(2.75f, -3.50f, 2.75f, 3.50f, NeonCyan),
            new MazeSeg(-1.65f, -2.50f, -0.55f, -2.50f, NeonMagenta),
            new MazeSeg(0.55f, -2.50f, 2.75f, -2.50f, NeonYellow),
            new MazeSeg(-2.75f, -1.50f, -1.65f, -1.50f, NeonMagenta),
            new MazeSeg(0.55f, -1.50f, 1.65f, -1.50f, NeonYellow),
            new MazeSeg(-0.55f, -0.50f, 0.55f, -0.50f, NeonMagenta),
            new MazeSeg(-1.65f, 0.50f, 1.65f, 0.50f, NeonYellow),
            new MazeSeg(-1.65f, 1.50f, 0.55f, 1.50f, NeonMagenta),
            new MazeSeg(-2.75f, 2.50f, -0.55f, 2.50f, NeonYellow),
            new MazeSeg(0.55f, 2.50f, 2.75f, 2.50f, NeonMagenta),
            new MazeSeg(-1.65f, -3.50f, -1.65f, -2.50f, NeonYellow),
            new MazeSeg(-1.65f, -1.50f, -1.65f, 0.50f, NeonMagenta),
            new MazeSeg(-0.55f, -2.50f, -0.55f, -0.50f, NeonYellow),
            new MazeSeg(0.55f, -1.50f, 0.55f, -0.50f, NeonMagenta),
            new MazeSeg(0.55f, 1.50f, 0.55f, 2.50f, NeonYellow),
            new MazeSeg(1.65f, -0.50f, 1.65f, 1.50f, NeonMagenta),
        };
        public static readonly MazeSeg DarkGate2 = new MazeSeg(-1.65f, 2.50f, -1.65f, 3.50f, Rgba.Hex("FF3B4E"));
        public const float DarkKey2X = 2.20f, DarkKey2Y = 3.00f, DarkExit2X = -2.20f, DarkExit2Y = 3.00f;
        /// <summary>Labirinto aperto per l'inseguimento: più strade (anelli) e qualche vicolo cieco.</summary>
        public static readonly MazeSeg[] WaspMaze2 =
        {
            new MazeSeg(-2.75f, -3.50f, 2.75f, -3.50f, NeonCyan),
            new MazeSeg(-2.75f, 3.50f, 2.75f, 3.50f, NeonCyan),
            new MazeSeg(-2.75f, -3.50f, -2.75f, 3.50f, NeonCyan),
            new MazeSeg(2.75f, -3.50f, 2.75f, 3.50f, NeonCyan),
            new MazeSeg(-2.75f, -2.50f, -1.65f, -2.50f, NeonMagenta),
            new MazeSeg(-0.55f, -2.50f, 0.55f, -2.50f, NeonYellow),
            new MazeSeg(-1.65f, -1.50f, -0.55f, -1.50f, NeonMagenta),
            new MazeSeg(0.55f, -1.50f, 1.65f, -1.50f, NeonYellow),
            new MazeSeg(-0.55f, -0.50f, 0.55f, -0.50f, NeonMagenta),
            new MazeSeg(-2.75f, 0.50f, -1.65f, 0.50f, NeonYellow),
            new MazeSeg(0.55f, 0.50f, 1.65f, 0.50f, NeonMagenta),
            new MazeSeg(-1.65f, 1.50f, 0.55f, 1.50f, NeonYellow),
            new MazeSeg(-1.65f, -0.50f, -1.65f, 0.50f, NeonMagenta),
            new MazeSeg(-1.65f, 1.50f, -1.65f, 2.50f, NeonYellow),
            new MazeSeg(-0.55f, -1.50f, -0.55f, 0.50f, NeonMagenta),
            new MazeSeg(-0.55f, 2.50f, -0.55f, 3.50f, NeonYellow),
            new MazeSeg(0.55f, -2.50f, 0.55f, -1.50f, NeonMagenta),
            new MazeSeg(0.55f, 1.50f, 0.55f, 2.50f, NeonYellow),
            new MazeSeg(1.65f, -3.50f, 1.65f, -2.50f, NeonMagenta),
            new MazeSeg(1.65f, -1.50f, 1.65f, -0.50f, NeonYellow),
            new MazeSeg(1.65f, 1.50f, 1.65f, 2.50f, NeonMagenta),
        };

        public static DoodleArt Maze2Neon() { return NeonMaze("maze_2", Maze2); }
        public static DoodleArt TrapMaze2Neon() { return NeonMaze("maze_trap2", TrapMaze2); }
        public static DoodleArt DarkMaze2Neon() { return NeonMaze("maze_dark2", DarkMaze2); }
        public static DoodleArt WaspMaze2Neon() { return NeonMaze("maze_wasp2", WaspMaze2); }
    }
}
