#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli 275-276 (release 0.75), in coda dopo i livelli 0.74. Il 275 è un livello di Toni (posizione multipla di 25).
/// Builder in GotchaLevels75A/B, runtime in Assets/Scripts/Levels/V75, logica pura in Assets/Scripts/Levels/V64/Logic.
/// Dalla 0.75 ogni livello nuovo si può anche perdere (Fail). Niente Easter egg.
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels75() => new List<LevelDefinition>
    {
        Define("V75ToniArmchair", "v75_toni_armchair", "TONI'S ARMCHAIR", "Toni can't be moved. Toni can be tricked.", BuildV75ToniArmchair()),
        Define("V75LockedBike", "v75_locked_bike", "THE LOCKED BIKE", "The lock is unbreakable. The pole isn't that tall.", BuildV75LockedBike()),
    };
}
#endif
