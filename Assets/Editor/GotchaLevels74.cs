#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Livelli 273-274 (release 0.74), in coda dopo i livelli 0.73. Builder in GotchaLevels74A/B, runtime in
/// Assets/Scripts/Levels/V74, logica pura in Assets/Scripts/Levels/V64/Logic. Niente Easter egg e niente Toni.
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels74() => new List<LevelDefinition>
    {
        Define("V74PiggyBank", "v74_piggy_bank", "THE PIGGY BANK", "Violence is not a savings plan.", BuildV74PiggyBank()),
        Define("V74CryingOnion", "v74_crying_onion", "THE CRYING ONION", "Goggles won't save you.", BuildV74CryingOnion()),
    };
}
#endif
