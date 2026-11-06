#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli nuovi della roadmap 1-300 nella 0.83: 47 THE WRONG PRIZE e 54 LUCK HACK (posizione dalla roadmap,
/// GotchaLevelOrder.cs). Builder in GotchaLevels83A/B, runtime in Assets/Scripts/Levels/V83, logica pura in V64/Logic.
/// Festa di vittoria comune (LevelReward). Non si possono perdere (regole della roadmap).
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels83() => new List<LevelDefinition>
    {
        Define("V83WrongPrize", "wrong_prize", "THE WRONG PRIZE", "You can't move to first place. Can first place move to you?", BuildV83WrongPrize()),
        Define("V83LuckHack", "luck_hack", "LUCK HACK", "Luck needs a little help.", BuildV83LuckHack()),
    };
}
#endif
