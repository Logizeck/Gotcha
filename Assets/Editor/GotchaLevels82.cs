#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli nuovi della roadmap 1-300 nella 0.82: 42 THE POLITE BOMB e 44 THE STUPID GENIUS 2 (posizione dalla roadmap,
/// GotchaLevelOrder.cs). Builder in GotchaLevels82A/B, runtime in Assets/Scripts/Levels/V82, logica pura in V64/Logic.
/// Festa di vittoria comune (LevelReward). Non si possono perdere (regole della roadmap).
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels82() => new List<LevelDefinition>
    {
        Define("V82PoliteBomb", "polite_bomb", "THE POLITE BOMB", "Bombs need power too.", BuildV82PoliteBomb()),
        Define("V82StupidGenius2", "stupid_genius_2", "THE STUPID GENIUS 2", "The question is wrong, not the answers.", BuildV82StupidGenius2()),
    };
}
#endif
