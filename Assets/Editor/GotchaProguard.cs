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
        "# plugin Google Mobile Ads: il codice Java del plugin viene chiamato dal C# per nome\n" +
        "-keep class com.google.unity.ads.** { *; }\n" +
        "# il plugin contiene anche il ponte per il nuovo SDK \"next-gen\" di Google, che non usiamo e non è incluso:\n" +
        "# R8 si fermerebbe per le classi mancanti (Missing class com.google.android.libraries.ads.mobile.sdk...)\n" +
        "-dontwarn com.google.android.libraries.ads.mobile.sdk.**\n" +
        "-dontwarn com.google.unity.ads.nextgen.**\n";
    // Niente opzioni globali (-renamesourcefileattribute, -keepattributes...): proguard-unity.txt è un file "consumer"
    // del modulo unityLibrary e il Gradle plugin di Unity 6.3 le rifiuta lì ("Global keep option ... should not be
    // used in a consumer configuration file"). Il mapping per de-offuscare i crash finisce comunque nell'AAB.

    public int callbackOrder => 2;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string rulesFile = Path.Combine(path, "proguard-unity.txt");
        if (File.Exists(rulesFile))
        {
            // se il file è rimasto da una build precedente, le nostre righe vecchie vengono sostituite
            string text = File.ReadAllText(rulesFile);
            int old = text.IndexOf("\n" + Marker, System.StringComparison.Ordinal);
            if (old >= 0) text = text.Substring(0, old);
            File.WriteAllText(rulesFile, text + Rules);
        }
        else Debug.LogWarning("[Gotcha] proguard-unity.txt non trovato in " + path + ": regole R8 del gioco non aggiunte");

        // la prima versione (0.67) scriveva un file a parte, proguard-gotcha.txt, collegato al build.gradle: se il progetto
        // Gradle in Library/Bee è rimasto quello vecchio, il file e il suo riferimento vengono tolti
        string stale = Path.Combine(path, "proguard-gotcha.txt");
        if (File.Exists(stale)) File.Delete(stale);

        foreach (string module in new[] { path, Path.Combine(path, "..", "launcher") })
        {
            string gradle = Path.Combine(module, "build.gradle");
            if (!File.Exists(gradle)) continue;
            string g = File.ReadAllText(gradle);
            string fixedG = g.Replace("'proguard-android.txt'", "'proguard-android-optimize.txt'")
                             .Replace("\"proguard-android.txt\"", "\"proguard-android-optimize.txt\"")
                             .Replace(", 'proguard-gotcha.txt'", "")
                             .Replace("\n        consumerProguardFiles 'proguard-gotcha.txt'", "");
            if (fixedG != g) File.WriteAllText(gradle, fixedG);
        }
    }
}
#endif
