#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livello nuovo della roadmap 1-300 nella 0.88: 138 THE BROKEN APPLAUSE (posizione dalla roadmap, GotchaLevelOrder.cs). Builder in GotchaLevels88A/B, runtime in
/// Assets/Scripts/Levels/V88, logica pura in V64/Logic. Festa di vittoria comune (LevelReward).
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels88() => new List<LevelDefinition>
    {
        Define("V88BrokenApplause", "broken_applause", "THE BROKEN APPLAUSE", "One hand can't clap.", BuildV88BrokenApplause()),
    };
}
#endif
