#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Versione 0.69: livelli 265-266, in coda dopo i livelli 0.68. Builder in GotchaLevels69A/B, runtime in
/// Assets/Scripts/Levels/V69, logica pura in Assets/Scripts/Levels/V64/Logic. Niente Easter egg e niente Toni.
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels69() => new List<LevelDefinition>
    {
        Define("V69DontLetItSink", "v69_dont_let_it_sink", "DON'T LET IT SINK", "That's a strange ocean.", BuildV69DontLetItSink()),
        Define("V69CrookedPicture", "v69_crooked_picture", "THE CROOKED PICTURE", "Are you sure it's the picture that's crooked?", BuildV69CrookedPicture()),
    };
}
#endif
