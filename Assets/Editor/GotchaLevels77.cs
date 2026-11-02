#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli nuovi della roadmap 1-300 nella 0.77: 8 LOVE THE BUG e 10 HALF FULL (posizione dalla roadmap,
/// GotchaLevelOrder.cs). Builder in GotchaLevels77A/B, runtime in Assets/Scripts/Levels/V77, logica pura in V64/Logic.
/// Festa di vittoria comune (LevelReward). Non si possono perdere (regole della roadmap).
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels77() => new List<LevelDefinition>
    {
        Define("V77LoveTheBug", "love_the_bug", "LOVE THE BUG", "Squashing makes it worse.", BuildV77LoveTheBug()),
        Define("V77HalfFull", "half_full", "HALF FULL", "Water won't help. Words might.", BuildV77HalfFull()),
    };
}
#endif
