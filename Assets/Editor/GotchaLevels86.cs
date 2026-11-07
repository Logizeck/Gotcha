#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli nuovi della roadmap 1-300 nella 0.86: 101 THE UPSIDE-DOWN RAIN e 110 THE IMAGINARY BRIDGE (posizione dalla
/// roadmap, GotchaLevelOrder.cs). Builder in GotchaLevels86A/B, runtime in Assets/Scripts/Levels/V86, logica pura in
/// V64/Logic. Festa di vittoria comune (LevelReward). Non si possono perdere (regole della roadmap).
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels86() => new List<LevelDefinition>
    {
        Define("V86UpsideDownRain", "upside_down_rain", "THE UPSIDE-DOWN RAIN", "That umbrella is hopeless.", BuildV86UpsideDownRain()),
        Define("V86ImaginaryBridge", "imaginary_bridge", "THE IMAGINARY BRIDGE", "That plank is too short. For now.", BuildV86ImaginaryBridge()),
    };
}
#endif
