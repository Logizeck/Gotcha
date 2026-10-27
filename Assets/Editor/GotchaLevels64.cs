#if UNITY_EDITOR
using System.Collections.Generic;
using Gotcha.Levels;

/// <summary>
/// Versione 0.64: i livelli 251-258 rifatti secondo la specifica del proprietario (il 252 "Wake up, Kitty!" resta
/// quello della 0.63). Builder in GotchaLevels64A/B/C, runtime e logica in Assets/Scripts/Levels/V64.
/// Niente Easter egg e niente Toni. Almost There e l'esame finale di Toni restano 259-260.
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels64()
    {
        var list = new List<LevelDefinition>
        {
            Define("V64FingerprintThief", "v64_fingerprint_thief", "THE FINGERPRINT THIEF", "Maybe the phone knows someone you don't.", BuildV64FingerprintThief()),
        };
        list.AddRange(Levels62());   // 252 Wake up, Kitty!
        list.AddRange(new[]
        {
            Define("V64PlasticFish", "v64_plastic_fish", "THE FISH THAT IS AFRAID OF WATER", "Something smells fishy. And plasticky.", BuildV64PlasticFish()),
            Define("V64NeighboursAlarm", "v64_neighbours_alarm", "THE NEIGHBOUR'S ALARM", "Maybe you're blaming the wrong clock.", BuildV64NeighboursAlarm()),
            Define("V64InvisiblePassword", "v64_invisible_password", "THE INVISIBLE PASSWORD", "Maybe the secret isn't behind the door.", BuildV64InvisiblePassword()),
            Define("V64IncompetentMagician", "v64_incompetent_magician", "THE INCOMPETENT MAGICIAN", "Magic works better without an audience of one.", BuildV64IncompetentMagician()),
            Define("V64PerfectLiar", "v64_perfect_liar", "THE PERFECT LIAR", "Perhaps the machine is lying too.", BuildV64PerfectLiar()),
            Define("V64RiggedCards", "v64_rigged_cards", "THE RIGGED THREE-CARD GAME", "The dealer is very good at cheating.", BuildV64RiggedCards()),
        });
        return list;
    }
}
#endif
