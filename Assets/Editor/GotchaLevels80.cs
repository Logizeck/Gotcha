#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli nuovi della roadmap 1-300 nella 0.80: 27 THE SAD BALLOON e 30 THE INVISIBLE TREASURE (posizione dalla
/// roadmap, GotchaLevelOrder.cs). Builder in GotchaLevels80A/B, runtime in Assets/Scripts/Levels/V80, logica pura in
/// V64/Logic. Festa di vittoria comune (LevelReward). Non si possono perdere (regole della roadmap).
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels80() => new List<LevelDefinition>
    {
        Define("V80SadBalloon", "sad_balloon", "THE SAD BALLOON", "It's not the balloon's fault.", BuildV80SadBalloon()),
        Define("V80InvisibleTreasure", "invisible_treasure", "THE INVISIBLE TREASURE", "Maps can be wrong. Or rewritten.", BuildV80InvisibleTreasure()),
    };
}
#endif
