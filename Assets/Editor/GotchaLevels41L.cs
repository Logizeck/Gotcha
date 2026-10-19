#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli della specifica 105-200 (in gioco dal 151), pacchetto L.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels41L() => new List<LevelDefinition>
    {
    };

    /// <summary>I 4 livelli di Toni della specifica (X-RAY, BEAUTY FILTER, PERSONAL TRAINER, FINAL EXAM), in ordine.</summary>
    static List<LevelDefinition> Toni41() => new List<LevelDefinition>
    {
    };
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V41L = new Dictionary<string, string[]>
    {
    };
}
#endif
