#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli nuovi della roadmap 1-300 nella 0.79: 19 THE SHY MONSTER e 24 THE LAZY SUN (posizione dalla roadmap,
/// GotchaLevelOrder.cs). Builder in GotchaLevels79A/B, runtime in Assets/Scripts/Levels/V79, logica pura in V64/Logic.
/// Festa di vittoria comune (LevelReward). Non si possono perdere (regole della roadmap).
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels79() => new List<LevelDefinition>
    {
        Define("V79ShyMonster", "shy_monster", "THE SHY MONSTER", "Fighting only makes it stronger.", BuildV79ShyMonster()),
        Define("V79LazySun", "lazy_sun", "THE LAZY SUN", "The sun is not lost. It's just lazy.", BuildV79LazySun()),
    };
}
#endif
