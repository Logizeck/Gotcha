#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli nuovi della roadmap 1-300 nella 0.76: 2 THE SHY BUTTON e 5 THE COWARDLY LION. La posizione non dipende da
/// questo elenco ma dalla roadmap (GotchaLevelOrder.cs). Builder in GotchaLevels76A/B, runtime in Assets/Scripts/Levels/V76,
/// logica pura in V64/Logic. Festa di vittoria comune (LevelReward). Non si possono perdere (regole della roadmap).
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels76() => new List<LevelDefinition>
    {
        Define("V76ShyButton", "shy_button", "THE SHY BUTTON", "It runs away from one finger.", BuildV76ShyButton()),
        Define("V76CowardlyLion", "cowardly_lion", "THE COWARDLY LION", "Not everything about this lion is real.", BuildV76CowardlyLion()),
    };
}
#endif
