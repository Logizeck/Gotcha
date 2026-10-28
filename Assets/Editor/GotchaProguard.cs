#if UNITY_EDITOR
using System.IO;
using UnityEditor.Android;
using UnityEngine;

/// <summary>
/// Regole R8 del gioco, aggiunte a ogni build Android (dalla 0.67 il minify Release è acceso: GotchaBuildMenu).
/// Unity scrive già le sue regole in unityLibrary/proguard-unity.txt (classi del player, GameActivity, JNI) e quel file
/// è già collegato al progetto Gradle: le nostre righe vengono aggiunte in fondo, senza toccare i build.gradle.
/// Si proteggono solo le classi Java che il C# chiama per nome con AndroidJavaClass (com.gotcha.game.GotchaKeys,
/// com.gotcha.video.GotchaVideo) e il plugin delle notifiche; tutto il resto resta libero di essere ridotto e offuscato.
/// Inoltre, nel modulo launcher, "proguard-android.txt" (non più accettato dai Gradle plugin recenti perché contiene
/// -dontoptimize) diventa "proguard-android-optimize.txt".
/// </summary>
public class GotchaProguard : IPostGenerateGradleAndroidProject
{
    const string Marker = "# --- Gotcha! ---";
    const string Rules =
        "\n" + Marker + "\n" +
        "# classi chiamate dal C# per nome (AndroidJavaClass): nomi e metodi pubblici devono restare\n" +
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
        string rulesFile = Path.Combine(path, "proguard-unity.txt");
        if (File.Exists(rulesFile))
        {
            string text = File.ReadAllText(rulesFile);
            if (!text.Contains(Marker)) File.AppendAllText(rulesFile, Rules);
        }
        else Debug.LogWarning("[Gotcha] proguard-unity.txt non trovato in " + path + ": regole R8 del gioco non aggiunte");

        foreach (string module in new[] { path, Path.Combine(path, "..", "launcher") })
        {
            string gradle = Path.Combine(module, "build.gradle");
            if (!File.Exists(gradle)) continue;
            string g = File.ReadAllText(gradle);
            string fixedG = g.Replace("'proguard-android.txt'", "'proguard-android-optimize.txt'")
                             .Replace("\"proguard-android.txt\"", "\"proguard-android-optimize.txt\"");
            if (fixedG != g) File.WriteAllText(gradle, fixedG);
        }
    }
}
#endif
