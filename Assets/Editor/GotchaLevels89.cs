#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livello nuovo 299 nella 0.89 (idea del proprietario): THE IMPOSSIBLE PRINT; la soluzione è l'Easter egg "You Rock!" (posizione dalla roadmap, GotchaLevelOrder.cs). Builder in GotchaLevels89A/B, runtime in
/// Assets/Scripts/Levels/V89, logica pura in V64/Logic. Festa di vittoria comune (LevelReward).
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels89() => new List<LevelDefinition>
    {
        Define("V89LostSheet", "lost_sheet", "THE IMPOSSIBLE PRINT", "This level has no paper. Another level has too much.", BuildV89LostSheet(), "you_rock"),
    };
}
#endif
