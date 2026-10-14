#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Crea l'APK Android senza passare dalla finestra Build Settings. Compilando da quella finestra Unity a volte
/// scrive in Console "GUI Error: Invalid GUILayout state in BuildPlayerWindow view" e "ArgumentException:
/// Getting control 1's position..." (bug noto e innocuo dell'Editor: la build gira dentro il disegno della
/// finestra). Da questo menu la build parte fuori dalla finestra e quegli errori non compaiono.
/// </summary>
public static class GotchaBuildMenu
{
    const string LastPathKey = "Gotcha.LastApkPath";

    [MenuItem("Gotcha/Build Android APK")]
    public static void BuildApk() { EditorApplication.delayCall += () => Build(false); }

    [MenuItem("Gotcha/Build And Run on Phone")]
    public static void BuildAndRun() { EditorApplication.delayCall += () => Build(true); }

    static void Build(bool run)
    {
        string last = EditorPrefs.GetString(LastPathKey, Path.Combine(Directory.GetCurrentDirectory(), "Builds", "Gotcha.apk"));
        string path = EditorUtility.SaveFilePanel("Salva APK", Path.GetDirectoryName(last), Path.GetFileName(last), "apk");
        if (string.IsNullOrEmpty(path)) return;
        EditorPrefs.SetString(LastPathKey, path);

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android &&
            !EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
        {
            EditorUtility.DisplayDialog("Gotcha!", "Impossibile passare alla piattaforma Android. È installato il modulo Android Build Support?", "OK");
            return;
        }
        EditorUserBuildSettings.buildAppBundle = false;   // APK installabile direttamente, non .aab

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Main.unity" },
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
}
#endif
