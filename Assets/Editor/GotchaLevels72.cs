#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli 269-270 (release 0.72), in coda dopo i livelli 0.71. Builder in GotchaLevels72A/B, runtime in
/// Assets/Scripts/Levels/V72, logica pura in Assets/Scripts/Levels/V64/Logic. Niente Easter egg e niente Toni.
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels72() => new List<LevelDefinition>
    {
        Define("V72CatBurglar", "v72_cat_burglar", "THE CAT BURGLAR", "Some things in this house can climb better than you.", BuildV72CatBurglar()),
        Define("V72LostRemote", "v72_lost_remote", "THE LOST REMOTE", "Remotes are overrated.", BuildV72LostRemote()),
    };
}
#endif
