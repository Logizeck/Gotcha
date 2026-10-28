#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Versione 0.68: livelli 263-264, in coda dopo i livelli 0.67. Builder in GotchaLevels68A/B, runtime in
/// Assets/Scripts/Levels/V68, logica pura in Assets/Scripts/Levels/V64/Logic. Niente Easter egg e niente Toni.
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels68() => new List<LevelDefinition>
    {
        Define("V68StuckSnack", "v68_stuck_snack", "THE STUCK SNACK", "Kicking is not a payment method.", BuildV68StuckSnack()),
        Define("V68CameraShyBird", "v68_camera_shy_bird", "THE CAMERA-SHY BIRD", "This bird hates being the subject.", BuildV68CameraShyBird()),
    };
}
#endif
