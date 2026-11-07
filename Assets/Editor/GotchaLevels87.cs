#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli nuovi della roadmap 1-300 nella 0.87: 119 THE FIRELESS DRAGON e 129 THE FAKE TROPHY (posizione dalla roadmap, GotchaLevelOrder.cs). Builder in GotchaLevels87A/B, runtime in
/// Assets/Scripts/Levels/V87, logica pura in V64/Logic. Festa di vittoria comune (LevelReward).
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels87() => new List<LevelDefinition>
    {
        Define("V87FirelessDragon", "fireless_dragon", "THE FIRELESS DRAGON", "Bubbles are not fire.", BuildV87FirelessDragon()),
        Define("V87FakeTrophy", "fake_trophy", "THE FAKE TROPHY", "That trophy is missing something.", BuildV87FakeTrophy()),
    };
}
#endif
