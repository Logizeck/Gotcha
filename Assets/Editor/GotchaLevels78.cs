#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli nuovi della roadmap 1-300 nella 0.78: 13 THE ARISTOCRATIC CAT e 15 THE HONEST THIEF (posizione dalla roadmap,
/// GotchaLevelOrder.cs). Builder in GotchaLevels78A/B, runtime in Assets/Scripts/Levels/V78, logica pura in V64/Logic.
/// Festa di vittoria comune (LevelReward). Non si possono perdere (regole della roadmap).
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels78() => new List<LevelDefinition>
    {
        Define("V78AristocraticCat", "aristocratic_cat", "THE ARISTOCRATIC CAT", "This cat won't come down for anything you offer.", BuildV78AristocraticCat()),
        Define("V78HonestThief", "honest_thief", "THE HONEST THIEF", "You can't steal it. But nobody said you have to steal it.", BuildV78HonestThief()),
    };
}
#endif
