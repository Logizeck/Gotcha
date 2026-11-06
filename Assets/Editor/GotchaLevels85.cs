#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli nuovi della roadmap 1-300 nella 0.85: 82 THE DISHONEST HOURGLASS e 91 MIRROR LIAR (posizione dalla roadmap,
/// GotchaLevelOrder.cs). Builder in GotchaLevels85A/B, runtime in Assets/Scripts/Levels/V85, logica pura in V64/Logic.
/// Festa di vittoria comune (LevelReward). Non si possono perdere (regole della roadmap).
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels85() => new List<LevelDefinition>
    {
        Define("V85DishonestHourglass", "dishonest_hourglass", "THE DISHONEST HOURGLASS", "That button is a liar.", BuildV85DishonestHourglass()),
        Define("V85MirrorLiar", "mirror_liar", "MIRROR LIAR", "One of them is just copying.", BuildV85MirrorLiar()),
    };
}
#endif
