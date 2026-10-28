#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Versione 0.67: livelli 261-262, in coda dopo l'esame finale di Toni (260). Da qui in poi ogni release aggiunge
/// uno o due livelli in fondo. Builder in GotchaLevels67A/B, runtime in Assets/Scripts/Levels/V67, logica pura in
/// Assets/Scripts/Levels/V64/Logic. Niente Easter egg e niente Toni.
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels67() => new List<LevelDefinition>
    {
        Define("V67DarkRoom", "v67_dark_room", "THE DARK ROOM", "Electricity is overrated.", BuildV67DarkRoom()),
        Define("V67ImmortalCandle", "v67_immortal_candle", "THE IMMORTAL CANDLE", "The wind doesn't seem to work.", BuildV67ImmortalCandle()),
    };
}
#endif
