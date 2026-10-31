#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli 271-272 (release 0.73), in coda dopo i livelli 0.72. Builder in GotchaLevels73A/B, runtime in
/// Assets/Scripts/Levels/V73, logica pura in Assets/Scripts/Levels/V64/Logic. Niente Easter egg e niente Toni.
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels73() => new List<LevelDefinition>
    {
        Define("V73StuckDrawer", "v73_stuck_drawer", "THE STUCK DRAWER", "Pulling harder won't help.", BuildV73StuckDrawer()),
        Define("V73MissingSock", "v73_missing_sock", "THE MISSING SOCK", "It didn't just vanish.", BuildV73MissingSock()),
    };
}
#endif
