#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli nuovi della roadmap 1-300 nella 0.81: 34 TOO EASY! e 38 THE IMPOSSIBLE HUG (posizione dalla roadmap,
/// GotchaLevelOrder.cs). Builder in GotchaLevels81A/B, runtime in Assets/Scripts/Levels/V81, logica pura in V64/Logic.
/// Festa di vittoria comune (LevelReward). Non si possono perdere (regole della roadmap).
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels81() => new List<LevelDefinition>
    {
        Define("V81TooEasy", "too_easy", "TOO EASY!", "The button is lying. Read the objective.", BuildV81TooEasy()),
        Define("V81ImpossibleHug", "impossible_hug", "THE IMPOSSIBLE HUG", "Neither one wants to go first.", BuildV81ImpossibleHug()),
    };
}
#endif
