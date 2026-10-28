#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor;

/// <summary>
/// Menu "Gotcha > Controlla manifest AAB": legge il manifest vero da un file .aab (con il bundletool incluso in Unity
/// e il JDK di Unity) e riassume ciò che Google Play usa per scegliere i dispositivi compatibili: versioni Android,
/// caratteristiche hardware obbligatorie, librerie obbligatorie, schermi e ABI nativi. Il manifest completo viene
/// salvato accanto all'AAB (Gotcha-x.y.aab.manifest.xml) e il riassunto in Gotcha-x.y.aab.report.txt.
/// </summary>
public static class GotchaManifestCheck
{
    const string AndroidNs = "http://schemas.android.com/apk/res/android";

    [MenuItem("Gotcha/Controlla manifest AAB")]
    static void Check()
    {
        string last = EditorPrefs.GetString("Gotcha.LastAabPath", Path.Combine(Directory.GetCurrentDirectory(), "Builds"));
        string aab = EditorUtility.OpenFilePanel("Scegli l'AAB da controllare", File.Exists(last) ? Path.GetDirectoryName(last) : last, "aab");
        if (string.IsNullOrEmpty(aab)) return;
        try
        {
            string xml = DumpManifest(aab, out string error);
            if (xml == null)
            {
                EditorUtility.DisplayDialog("Gotcha!", "Non riesco a leggere il manifest:\n" + error, "OK");
                return;
            }
            File.WriteAllText(aab + ".manifest.xml", xml);
            string report = Summarize(xml, aab);
            File.WriteAllText(aab + ".report.txt", report);
            UnityEngine.Debug.Log("[Gotcha] Manifest dell'AAB:\n" + report);
            EditorUtility.DisplayDialog("Gotcha! — manifest AAB", report.Length > 1800 ? report.Substring(0, 1800) + "\n..." : report, "OK");
            EditorUtility.RevealInFinder(aab + ".report.txt");
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Gotcha!", "Errore: " + e.Message, "OK");
        }
    }

    static string DumpManifest(string aab, out string error)
    {
        error = null;
        string engine = BuildPipeline.GetPlaybackEngineDirectory(BuildTarget.Android, BuildOptions.None);
        string jar = Directory.Exists(engine)
            ? Directory.GetFiles(engine, "bundletool*.jar", SearchOption.AllDirectories).FirstOrDefault()
            : null;
        if (jar == null) { error = "bundletool non trovato in " + engine; return null; }
        string java = FindJava(engine);
        var psi = new ProcessStartInfo(java, "-jar \"" + jar + "\" dump manifest --bundle \"" + aab + "\"")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        using (Process p = Process.Start(psi))
        {
            string output = p.StandardOutput.ReadToEnd();
            string err = p.StandardError.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode != 0 || string.IsNullOrWhiteSpace(output)) { error = java + "\n" + err; return null; }
            return output;
        }
    }

    /// <summary>Il java del JDK di Unity (Preferences > External Tools), altrimenti JAVA_HOME, altrimenti "java".</summary>
    static string FindJava(string engine)
    {
        string exe = Path.DirectorySeparatorChar == '\\' ? "java.exe" : "java";
        Type settings = Type.GetType("UnityEditor.Android.AndroidExternalToolsSettings, UnityEditor.Android.Extensions");
        string jdk = settings?.GetProperty("jdkRootPath")?.GetValue(null) as string;
        foreach (string root in new[] { jdk, Path.Combine(engine, "OpenJDK"), Environment.GetEnvironmentVariable("JAVA_HOME") })
            if (!string.IsNullOrEmpty(root) && File.Exists(Path.Combine(root, "bin", exe))) return Path.Combine(root, "bin", exe);
        return exe;
    }

    static string Summarize(string xml, string aab)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        XmlElement root = doc.DocumentElement;
        var sb = new StringBuilder();
        sb.AppendLine("AAB: " + Path.GetFileName(aab));
        sb.AppendLine("package " + root.GetAttribute("package") + "  versionCode " + root.GetAttribute("versionCode", AndroidNs));
        foreach (XmlElement e in root.SelectNodes("uses-sdk"))
            sb.AppendLine("SDK: min " + e.GetAttribute("minSdkVersion", AndroidNs) + "  target " + e.GetAttribute("targetSdkVersion", AndroidNs)
                + (e.HasAttribute("maxSdkVersion", AndroidNs) ? "  MAX " + e.GetAttribute("maxSdkVersion", AndroidNs) + "  <-- limite massimo!" : ""));
        sb.AppendLine("Caratteristiche OBBLIGATORIE (filtrano i dispositivi):");
        int n = 0;
        foreach (XmlElement e in root.SelectNodes("uses-feature"))
        {
            if (e.GetAttribute("required", AndroidNs) == "false") continue;
            string name = e.GetAttribute("name", AndroidNs);
            if (string.IsNullOrEmpty(name)) name = "glEsVersion " + e.GetAttribute("glEsVersion", AndroidNs);
            sb.AppendLine("  - " + name);
            n++;
        }
        if (n == 0) sb.AppendLine("  (nessuna)");
        sb.AppendLine("Caratteristiche facoltative:");
        foreach (XmlElement e in root.SelectNodes("uses-feature"))
            if (e.GetAttribute("required", AndroidNs) == "false") sb.AppendLine("  - " + e.GetAttribute("name", AndroidNs));
        foreach (XmlElement e in root.SelectNodes("application/uses-library"))
            if (e.GetAttribute("required", AndroidNs) != "false") sb.AppendLine("Libreria OBBLIGATORIA: " + e.GetAttribute("name", AndroidNs));
        foreach (XmlElement e in root.SelectNodes("application/uses-native-library"))
            if (e.GetAttribute("required", AndroidNs) != "false") sb.AppendLine("Libreria nativa OBBLIGATORIA: " + e.GetAttribute("name", AndroidNs));
        foreach (string tag in new[] { "supports-screens", "compatible-screens", "supports-gl-texture" })
            foreach (XmlElement e in root.SelectNodes(tag))
                sb.AppendLine(tag + ": " + e.OuterXml);
        foreach (XmlElement e in root.SelectNodes("application/meta-data"))
        {
            string name = e.GetAttribute("name", AndroidNs);
            if (name.IndexOf("vr", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("xr", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("ar", StringComparison.OrdinalIgnoreCase) >= 0 && name.IndexOf("core", StringComparison.OrdinalIgnoreCase) >= 0)
                sb.AppendLine("meta-data sospetto: " + name + " = " + e.GetAttribute("value", AndroidNs));
        }
        // ABI: i nomi dei file nello zip sono in chiaro (base/lib/<abi>/...); così non serve System.IO.Compression
        string raw = Encoding.ASCII.GetString(File.ReadAllBytes(aab));
        var abis = System.Text.RegularExpressions.Regex.Matches(raw, @"base/lib/([a-z0-9_\-]+)/")
            .Cast<System.Text.RegularExpressions.Match>().Select(m => m.Groups[1].Value).Distinct().ToArray();
        sb.AppendLine("ABI nativi: " + (abis.Length > 0 ? string.Join(", ", abis) : "nessuno"));
        return sb.ToString();
    }
}
#endif
