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
/// salvato accanto all'AAB (Gotcha-x.y.aab.manifest.xml) e il riassunto in Gotcha-x.y.aab.report.txt. Il riassunto
/// riporta anche dimensione dell'AAB, dimensioni dei .dex e i file in BUNDLE-METADATA (mapping R8): serve per
/// confrontare due build (es. 0.66 senza R8 e 0.67 con R8).
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

        // DEX e metadati R8: dimensioni dei .dex (non compressi) e file in BUNDLE-METADATA (mapping, metadati R8)
        var entries = ZipEntries(File.ReadAllBytes(aab));
        var dex = entries.Where(e => e.name.EndsWith(".dex", StringComparison.Ordinal)).ToArray();
        long dexBytes = dex.Sum(e => e.size);
        sb.AppendLine("AAB: " + (new FileInfo(aab).Length / 1024) + " KB");
        sb.AppendLine("DEX: " + dex.Length + " file, " + (dexBytes / 1024) + " KB non compressi");
        foreach (var e in dex) sb.AppendLine("  - " + e.name + "  " + (e.size / 1024) + " KB");
        var meta = entries.Where(e => e.name.StartsWith("BUNDLE-METADATA/", StringComparison.Ordinal)).ToArray();
        sb.AppendLine("BUNDLE-METADATA (" + meta.Length + "):");
        foreach (var e in meta) sb.AppendLine("  - " + e.name + "  " + (e.size / 1024) + " KB");
        bool mapping = meta.Any(e => e.name.IndexOf("obfuscation", StringComparison.OrdinalIgnoreCase) >= 0 || e.name.EndsWith("proguard.map", StringComparison.Ordinal));
        sb.AppendLine(mapping ? "Mapping R8 incluso nell'AAB: SI" : "Mapping R8 incluso nell'AAB: NO (minify spento o non applicato)");
        return sb.ToString();
    }

    /// <summary>Elenco dei file di uno zip (nome e dimensione non compressa) letto dalla "central directory".</summary>
    static System.Collections.Generic.List<(string name, long size)> ZipEntries(byte[] b)
    {
        var list = new System.Collections.Generic.List<(string, long)>();
        for (int i = b.Length - 46; i >= 0; i--)
        {
            // fine della central directory: "PK\x05\x06"
            if (b[i] != 0x50 || b[i + 1] != 0x4b || b[i + 2] != 0x05 || b[i + 3] != 0x06) continue;
            int count = BitConverter.ToUInt16(b, i + 10);
            long pos = BitConverter.ToUInt32(b, i + 16);
            for (int k = 0; k < count && pos + 46 <= b.Length; k++)
            {
                if (BitConverter.ToUInt32(b, (int)pos) != 0x02014b50) break;
                long size = BitConverter.ToUInt32(b, (int)pos + 24);
                int nameLen = BitConverter.ToUInt16(b, (int)pos + 28);
                int extraLen = BitConverter.ToUInt16(b, (int)pos + 30);
                int commentLen = BitConverter.ToUInt16(b, (int)pos + 32);
                list.Add((Encoding.UTF8.GetString(b, (int)pos + 46, nameLen), size));
                pos += 46 + nameLen + extraLen + commentLen;
            }
            break;
        }
        return list;
    }
}
#endif
