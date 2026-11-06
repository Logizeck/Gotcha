#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli nuovi della roadmap 1-300 nella 0.84: 63 THE TINY CROWN e 72 THE SHORT LADDER (posizione dalla roadmap,
/// GotchaLevelOrder.cs). Builder in GotchaLevels84A/B, runtime in Assets/Scripts/Levels/V84, logica pura in V64/Logic.
/// Festa di vittoria comune (LevelReward). Non si possono perdere (regole della roadmap).
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels84() => new List<LevelDefinition>
    {
        Define("V84TinyCrown", "tiny_crown", "THE TINY CROWN", "That crown would fit a mouse.", BuildV84TinyCrown()),
        Define("V84ShortLadder", "short_ladder", "THE SHORT LADDER", "The ladder is the problem, not the wall.", BuildV84ShortLadder()),
    };
}
#endif
