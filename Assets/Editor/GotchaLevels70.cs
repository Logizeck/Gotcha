#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli 267-268 (release 0.71), in coda dopo i livelli 0.69. Builder in GotchaLevels70A/B, runtime in
/// Assets/Scripts/Levels/V70, logica pura in Assets/Scripts/Levels/V64/Logic. Niente Easter egg e niente Toni.
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels70() => new List<LevelDefinition>
    {
        Define("V70UnswattableFly", "v70_unswattable_fly", "THE UNSWATTABLE FLY", "Violence isn't working.", BuildV70UnswattableFly()),
        Define("V70StubbornKetchup", "v70_stubborn_ketchup", "THE STUBBORN KETCHUP", "Shaking harder won't help.", BuildV70StubbornKetchup()),
    };
}
#endif
