#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Levels;

/// <summary>Speciale Halloween: sei livelli 245-250 (vedi brief), inseriti prima di Almost There e dell'esame di Toni.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> LevelsHalloween() => new List<LevelDefinition>
    {
    };
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> VHalloween = new Dictionary<string, string[]>
    {
    };
}
#endif
