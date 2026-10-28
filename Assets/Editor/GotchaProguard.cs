#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Android;
using UnityEngine;

/// <summary>
/// Regole R8 del gioco, aggiunte a ogni build Android (dalla 0.67 il minify Release è acceso: GotchaBuildMenu).
/// Unity scrive già le sue regole (proguard-unity.txt: classi del player, GameActivity, JNI); qui si proteggono solo le
/// classi Java che il C# chiama per nome con AndroidJavaClass (com.gotcha.game.GotchaKeys, com.gotcha.video.GotchaVideo)
/// e il plugin delle notifiche. Tutto il resto (androidx, librerie) resta libero di essere ridotto e offuscato.
/// Le regole vanno in unityLibrary/proguard-gotcha.txt, aggiunto ai consumerProguardFiles del modulo.
/// </summary>
public class GotchaProguard : IPostGenerateGradleAndroidProject
{
    const string RulesFile = "proguard-gotcha.txt";
    const string Rules =
        "# Gotcha! — classi chiamate dal C# per nome (AndroidJavaClass): nomi e metodi pubblici devono restare\n" +
        "-keep class com.gotcha.game.GotchaKeys { public *; }\n" +
        "-keep class com.gotcha.video.GotchaVideo { public *; }\n" +
        "# notifiche Unity (com.unity.mobile.notifications): il plugin usa reflection e callback per nome\n" +
        "-keep class com.unity.androidnotifications.** { *; }\n" +
        "# stack trace leggibili in Play Console (con il mapping incluso nell'AAB)\n" +
        "-keepattributes SourceFile,LineNumberTable\n" +
        "-renamesourcefileattribute SourceFile\n";

    public int callbackOrder => 2;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        File.WriteAllText(Path.Combine(path, RulesFile), Rules);
        string gradle = Path.Combine(path, "build.gradle");
        if (!File.Exists(gradle)) { Debug.LogWarning("[Gotcha] build.gradle non trovato in " + path + ": regole R8 non aggiunte"); return; }
        string text = File.ReadAllText(gradle);
        if (text.Contains(RulesFile)) return;
        // consumerProguardFiles 'proguard-unity.txt'[, 'proguard-user.txt'] -> + 'proguard-gotcha.txt'
        var m = Regex.Match(text, @"consumerProguardFiles\s+[^\r\n]+");
        if (m.Success)
            text = text.Substring(0, m.Index) + m.Value.TrimEnd() + ", '" + RulesFile + "'" + text.Substring(m.Index + m.Length);
        else
        {
            var d = Regex.Match(text, @"defaultConfig\s*\{");
            if (!d.Success) { Debug.LogWarning("[Gotcha] defaultConfig non trovato in build.gradle: regole R8 non aggiunte"); return; }
            text = text.Insert(d.Index + d.Length, "\n        consumerProguardFiles '" + RulesFile + "'");
        }
        File.WriteAllText(gradle, text);
        Debug.Log("[Gotcha] Regole R8 aggiunte: " + RulesFile);
    }
}
#endif
