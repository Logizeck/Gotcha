#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// AdMob senza passaggi a mano. In un assembly a parte (non dipende dal resto del gioco), così funziona anche quando
/// il gioco non compila:
/// - plugin "Google Mobile Ads" presente  -> aggiunge il define GOTCHA_ADMOB per Android (si attiva AdMobProvider);
///   plugin assente -> lo toglie (altrimenti il gioco non compilerebbe);
/// - copia l'App ID da Assets/Resources/AdMobIds.json (o quello di prova, se vuoto) nelle impostazioni del plugin
///   (Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset) all'apertura e prima di ogni build.
/// Menu: Gotcha > AdMob > Applica impostazioni.
/// </summary>
[InitializeOnLoad]
public class GotchaAdMobSetup : IPreprocessBuildWithReport
{
    const string Define = "GOTCHA_ADMOB";
    const string IdsPath = "Assets/Resources/AdMobIds.json";
    const string TestAppId = "ca-app-pub-3940256099942544~3347511713";
    const string SettingsPath = "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";

    [Serializable]
    class Ids { public string appId, interstitialId, rewardedId; }

    static GotchaAdMobSetup()
    {
        EditorApplication.delayCall += () => Apply(false);
    }

    public int callbackOrder => -100;   // prima del controllo del plugin, che vuole l'App ID già impostato

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.Android) Apply(true);
    }

    [MenuItem("Gotcha/AdMob/Applica impostazioni")]
    static void ApplyMenu()
    {
        string msg = Apply(true);
        EditorUtility.DisplayDialog("Gotcha! — AdMob", msg, "OK");
    }

    /// <summary>
    /// Il plugin di Google è nel progetto? Si cerca la classe GoogleMobileAds.Api.MobileAds in qualunque assembly
    /// caricato (anche quelli con nome diverso, a seconda della versione del plugin), oppure una DLL / un asmdef del
    /// plugin tra i file del progetto o dei pacchetti.
    /// </summary>
    public static bool PluginInstalled => FindPlugin(out _);

    static bool FindPlugin(out string where)
    {
        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type t = null;
            try { t = a.GetType("GoogleMobileAds.Api.MobileAds", false); } catch { }
            if (t == null)
            {
                try { t = a.GetTypes().FirstOrDefault(x => x.FullName == "GoogleMobileAds.Api.MobileAds"); } catch { }
            }
            if (t != null) { where = "assembly " + a.GetName().Name; return true; }
        }
        // ricerca tra i file: DLL o asmdef del plugin (cartella Assets/GoogleMobileAds o pacchetto com.google.ads.mobile)
        string[] guids = AssetDatabase.FindAssets("GoogleMobileAds");
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            if (path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase))
            {
                where = path;
                return true;
            }
        }
        where = null;
        return false;
    }

    /// <summary>In modalità provvisoria (errori di compilazione) i plugin non sono caricati: meglio non toccare nulla.</summary>
    static bool InSafeMode
    {
        get
        {
            PropertyInfo p = typeof(EditorUtility).GetProperty("isInSafeMode", BindingFlags.Static | BindingFlags.Public);
            try { return p != null && (bool)p.GetValue(null); } catch { return false; }
        }
    }

    static string Apply(bool verbose)
    {
        if (InSafeMode)
            return "Unity è in modalità provvisoria (Safe Mode): prima vanno risolti gli errori di compilazione, poi riprova.";
        bool plugin = FindPlugin(out string where);
        SetDefine(plugin);
        if (!plugin)
        {
            // per capire cosa c'è: assembly e file con "Google" nel nome
            string asm = string.Join(", ", AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name)
                .Where(n => n.IndexOf("Google", StringComparison.OrdinalIgnoreCase) >= 0).Take(12));
            string files = string.Join(", ", AssetDatabase.FindAssets("GoogleMobileAds").Select(AssetDatabase.GUIDToAssetPath).Take(8));
            return "Plugin Google Mobile Ads non trovato: AdMob resta spento (nessuna pubblicità nella build per lo store).\n" +
                   "Installalo come spiegato in PLAY_STORE.md, sezione 4.\n\nAssembly Google caricati: " +
                   (asm.Length > 0 ? asm : "nessuno") + "\nFile trovati: " + (files.Length > 0 ? files : "nessuno");
        }
        Ids ids = ReadIds();
        string appId = string.IsNullOrWhiteSpace(ids.appId) ? TestAppId : ids.appId.Trim();
        bool ok = SetPluginAppId(appId);
        bool test = string.IsNullOrWhiteSpace(ids.appId) || string.IsNullOrWhiteSpace(ids.interstitialId) || string.IsNullOrWhiteSpace(ids.rewardedId);
        string msg = "Plugin trovato (" + where + ").\n" + (ok ? "App ID AdMob impostato: " + appId : "Non riesco a impostare l'App ID nel plugin: mettilo a mano in Assets > Google Mobile Ads > Settings (" + appId + ").") +
                     (test ? "\nATTENZIONE: in " + IdsPath + " mancano gli ID reali: si useranno gli annunci di PROVA." : "\nID reali presenti: le build Release mostreranno annunci veri.");
        if (verbose || !ok) Debug.Log("[Gotcha] AdMob: " + msg.Replace('\n', ' '));
        return msg;
    }

    static Ids ReadIds()
    {
        try { return File.Exists(IdsPath) ? JsonUtility.FromJson<Ids>(File.ReadAllText(IdsPath)) ?? new Ids() : new Ids(); }
        catch { return new Ids(); }
    }

    static void SetDefine(bool on)
    {
#if UNITY_2021_2_OR_NEWER
        var target = NamedBuildTarget.Android;
        string defines = PlayerSettings.GetScriptingDefineSymbols(target);
#else
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
#endif
        var list = defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()).ToList();
        bool has = list.Contains(Define);
        if (has == on) return;
        if (on) list.Add(Define); else list.Remove(Define);
#if UNITY_2021_2_OR_NEWER
        PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", list));
#else
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, string.Join(";", list));
#endif
        Debug.Log("[Gotcha] AdMob: define " + Define + (on ? " aggiunto (plugin trovato)" : " tolto (plugin assente)"));
    }

    /// <summary>Scrive l'App ID nelle impostazioni del plugin (per riflessione: il plugin può non esserci).</summary>
    static bool SetPluginAppId(string appId)
    {
        try
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("GoogleMobileAds.Editor.GoogleMobileAdsSettings", false)).FirstOrDefault(t => t != null);
            if (type == null) return false;
            UnityEngine.Object settings = AssetDatabase.LoadAssetAtPath(SettingsPath, type);
            if (settings == null)
            {
                MethodInfo load = type.GetMethod("LoadInstance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                settings = load != null ? load.Invoke(null, null) as UnityEngine.Object : null;
            }
            if (settings == null) return false;
            PropertyInfo prop = type.GetProperty("GoogleMobileAdsAndroidAppId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop == null || !prop.CanWrite) return false;
            if ((prop.GetValue(settings) as string) == appId) return true;
            prop.SetValue(settings, appId);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Gotcha] AdMob: App ID non impostato (" + e.Message + ")");
            return false;
        }
    }
}
#endif
