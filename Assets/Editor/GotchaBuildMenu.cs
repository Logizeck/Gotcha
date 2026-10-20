#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Gotcha.Core;
using Gotcha.UI;

/// <summary>
/// Build Android senza passare dalla finestra Build Settings. Compilando da quella finestra Unity a volte
/// scrive in Console "GUI Error: Invalid GUILayout state in BuildPlayerWindow view" e "ArgumentException:
/// Getting control 1's position..." (bug noto e innocuo dell'Editor: la build gira dentro il disegno della
/// finestra). Da questo menu la build parte fuori dalla finestra e quegli errori non compaiono.
/// - "Build Android APK" / "Build And Run on Phone": build di prova (tutti i livelli sbloccati, pubblicità simulata).
/// - "Build Google Play AAB": build per lo store (.aab firmato, define GOTCHA_RELEASE). Vedi PLAY_STORE.md.
/// </summary>
public static class GotchaBuildMenu
{
    const string LastPathKey = "Gotcha.LastApkPath";
    const string LastAabKey = "Gotcha.LastAabPath";
    const string SigningFile = "ReleaseSigning.json";   // nella cartella del progetto, fuori da git (.gitignore)
    const string ScenePath = "Assets/Scenes/Main.unity";

    [MenuItem("Gotcha/Build Android APK")]
    public static void BuildApk() { EditorApplication.delayCall += () => Build(false); }

    [MenuItem("Gotcha/Build And Run on Phone")]
    public static void BuildAndRun() { EditorApplication.delayCall += () => Build(true); }

    [MenuItem("Gotcha/Build Google Play AAB")]
    public static void BuildAab() { EditorApplication.delayCall += BuildRelease; }

    // ---------- Versione e impostazioni per Google Play ----------

    /// <summary>
    /// versionCode di Google Play (intero che deve crescere a ogni caricamento) ricavato da GameUI.Version:
    /// major*10000 + minor*100 + patch. "0.39" → 3900, "0.39.1" → 3901, "1.0" → 10000 (più grande di "0.99").
    /// Minor e patch devono stare tra 0 e 99.
    /// </summary>
    public static int VersionCode(string version)
    {
        string[] parts = version.Split('.');
        int[] n = new int[3];
        for (int i = 0; i < n.Length && i < parts.Length; i++)
            if (!int.TryParse(parts[i], out n[i]) || n[i] < 0 || (i > 0 && n[i] > 99))
                throw new ArgumentException("Versione non valida per il versionCode: " + version + " (formato major.minor[.patch], minor e patch 0-99)");
        return n[0] * 10000 + n[1] * 100 + n[2];
    }

    /// <summary>Versione dell'app = quella mostrata nel gioco (GameUI.Version).</summary>
    public static void ApplyVersion()
    {
        PlayerSettings.bundleVersion = GameUI.Version;
        PlayerSettings.Android.bundleVersionCode = VersionCode(GameUI.Version);
    }

    /// <summary>Impostazioni richieste da Google Play (le usa anche il builder in ConfigurePlayer).</summary>
    public static void ApplyStoreSettings()
    {
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, InviteService.PackageName);
        ApplyVersion();
        // API 24 (Android 7.0): minimo richiesto dalla protezione automatica di Play (Play Integrity)
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        // API 36 (Android 16): obbligatoria per le app nuove dal 31/8/2026. Il cast compila anche su Unity 2022.3,
        // dove il valore non è nell'elenco (serve la piattaforma 36 installata nell'Android SDK)
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
        // 64 bit obbligatorio su Google Play
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        // minify (R8) resta spento: toglierebbe le classi Java chiamate per nome (GotchaKeys, GotchaVideo)
    }

    // ---------- Build di prova (APK) ----------

    static void Build(bool run)
    {
        string last = EditorPrefs.GetString(LastPathKey, Path.Combine(Directory.GetCurrentDirectory(), "Builds", "Gotcha.apk"));
        string path = EditorUtility.SaveFilePanel("Salva APK", Path.GetDirectoryName(last), Path.GetFileName(last), "apk");
        if (string.IsNullOrEmpty(path)) return;
        EditorPrefs.SetString(LastPathKey, path);

        if (!SwitchToAndroid()) return;
        EditorUserBuildSettings.buildAppBundle = false;   // APK installabile direttamente, non .aab
        ApplyVersion();

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = path,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = run ? BuildOptions.AutoRunPlayer : BuildOptions.None,
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("[Gotcha] APK creato: " + path + " (" + (summary.totalSize / (1024f * 1024f)).ToString("0.0") + " MB)");
            if (!run) EditorUtility.RevealInFinder(path);
        }
        else
        {
            Debug.LogError("[Gotcha] Build non riuscita (" + summary.result + "): " + summary.totalErrors + " errori. Vedi la Console.");
        }
    }

    static bool SwitchToAndroid()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ||
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            return true;
        EditorUtility.DisplayDialog("Gotcha!", "Impossibile passare alla piattaforma Android. È installato il modulo Android Build Support?", "OK");
        return false;
    }

    // ---------- Build per Google Play (AAB firmato) ----------

    [Serializable]
    class ReleaseSigning
    {
        public string keystorePath, keystorePass, keyAlias, keyAliasPass;
    }

    static ReleaseSigning LoadSigning(out string problem)
    {
        problem = null;
        if (!File.Exists(SigningFile)) { problem = "Manca il file " + SigningFile + " nella cartella del progetto."; return null; }
        ReleaseSigning s;
        try { s = JsonUtility.FromJson<ReleaseSigning>(File.ReadAllText(SigningFile)); }
        catch (Exception e) { problem = SigningFile + " non è un JSON valido: " + e.Message; return null; }
        if (s == null || string.IsNullOrEmpty(s.keystorePath) || string.IsNullOrEmpty(s.keystorePass) ||
            string.IsNullOrEmpty(s.keyAlias) || string.IsNullOrEmpty(s.keyAliasPass))
        { problem = SigningFile + " è incompleto: servono keystorePath, keystorePass, keyAlias e keyAliasPass."; return null; }
        s.keystorePath = Path.GetFullPath(s.keystorePath);   // relativo alla cartella del progetto o assoluto
        if (!File.Exists(s.keystorePath)) { problem = "Keystore non trovato: " + s.keystorePath; return null; }
        return s;
    }

    static void BuildRelease()
    {
        ReleaseSigning signing = LoadSigning(out string problem);
        if (signing == null)
        {
            EditorUtility.DisplayDialog("Gotcha! – firma mancante", problem + "\n\n" +
                "1) Crea una volta sola la chiave di caricamento (upload key), fuori dal progetto, e conservane una copia:\n" +
                "keytool -genkeypair -v -keystore gotcha-upload.keystore -alias gotcha -keyalg RSA -keysize 2048 -validity 10000\n" +
                "(oppure Player Settings > Publishing Settings > Keystore Manager)\n\n" +
                "2) Crea " + SigningFile + " nella cartella del progetto:\n" +
                "{\"keystorePath\":\"C:/percorso/gotcha-upload.keystore\",\"keystorePass\":\"...\",\"keyAlias\":\"gotcha\",\"keyAliasPass\":\"...\"}\n\n" +
                "Il file è in .gitignore: non va mai condiviso. Dettagli in PLAY_STORE.md.", "OK");
            return;
        }

#if UNITY_2021_2_OR_NEWER
        string defines = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Android);
#else
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
#endif
        bool adMob = Array.IndexOf(defines.Split(';'), "GOTCHA_ADMOB") >= 0;
        if (!adMob && !EditorUtility.DisplayDialog("Gotcha!",
                "AdMob non è attivo (manca il define GOTCHA_ADMOB): la build per lo store non avrà pubblicità.\nContinuare?",
                "Continua senza pubblicità", "Annulla"))
            return;
        const string adMobFile = "Assets/Scripts/Ads/AdMobProvider.cs";
        if (adMob && File.Exists(adMobFile) && File.ReadAllText(adMobFile).Contains("ca-app-pub-3940256099942544/") &&
            !EditorUtility.DisplayDialog("Gotcha!",
                "AdMobProvider usa ancora gli ID di PROVA di Google: in questa build gli annunci saranno finti e non pagati.\nContinuare?",
                "Continua (solo per test)", "Annulla"))
            return;

        string name = "Gotcha-" + GameUI.Version + ".aab";
        string last = EditorPrefs.GetString(LastAabKey, Path.Combine(Directory.GetCurrentDirectory(), "Builds", name));
        string path = EditorUtility.SaveFilePanel("Salva AAB per Google Play", Path.GetDirectoryName(last), name, "aab");
        if (string.IsNullOrEmpty(path)) return;
        EditorPrefs.SetString(LastAabKey, path);

        if (!SwitchToAndroid()) return;
        ApplyStoreSettings();

        // la firma vale solo per questa build: dopo si torna alle impostazioni di prima (le APK di prova restano come sono)
        bool prevCustom = PlayerSettings.Android.useCustomKeystore;
        string prevKeystore = PlayerSettings.Android.keystoreName, prevKsPass = PlayerSettings.Android.keystorePass;
        string prevAlias = PlayerSettings.Android.keyaliasName, prevAliasPass = PlayerSettings.Android.keyaliasPass;
        bool prevBundle = EditorUserBuildSettings.buildAppBundle;
        BuildReport report;
        try
        {
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = signing.keystorePath;
            PlayerSettings.Android.keystorePass = signing.keystorePass;
            PlayerSettings.Android.keyaliasName = signing.keyAlias;
            PlayerSettings.Android.keyaliasPass = signing.keyAliasPass;
            EditorUserBuildSettings.buildAppBundle = true;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = path,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None,
                // GOTCHA_RELEASE solo per questa build: i define del progetto non cambiano
                extraScriptingDefines = new[] { "GOTCHA_RELEASE" },
            };
            report = BuildPipeline.BuildPlayer(options);
        }
        finally
        {
            PlayerSettings.Android.useCustomKeystore = prevCustom;
            PlayerSettings.Android.keystoreName = prevKeystore;
            PlayerSettings.Android.keystorePass = prevKsPass;
            PlayerSettings.Android.keyaliasName = prevAlias;
            PlayerSettings.Android.keyaliasPass = prevAliasPass;
            EditorUserBuildSettings.buildAppBundle = prevBundle;
        }

        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
        {
            float mb = summary.totalSize / (1024f * 1024f);
            string info = "AAB per Google Play creato:\n" + path + "\n\nVersione " + PlayerSettings.bundleVersion +
                " (versionCode " + PlayerSettings.Android.bundleVersionCode + "), " + mb.ToString("0.0") + " MB" +
                (adMob ? "" : "\nSenza pubblicità (AdMob non attivo).");
            Debug.Log("[Gotcha] " + info.Replace('\n', ' '));
            EditorUtility.DisplayDialog("Gotcha!", info + "\n\nRicorda: ogni caricamento su Play richiede un versionCode più alto " +
                "(aumenta GameUI.Version).", "OK");
            EditorUtility.RevealInFinder(path);
        }
        else
        {
            Debug.LogError("[Gotcha] Build AAB non riuscita (" + summary.result + "): " + summary.totalErrors + " errori. Vedi la Console.");
        }
    }
}
#endif
