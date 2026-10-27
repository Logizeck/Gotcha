#if UNITY_EDITOR
using System.IO;
using System.Xml;
using UnityEditor.Android;

/// <summary>
/// Aggiunge il permesso VIBRATE al manifest Android a ogni build (APK e AAB). Unity lo mette da solo solo se trova
/// Handheld.Vibrate nel codice rimasto dopo lo stripping, e la vibrazione del gioco passa dal Vibrator di sistema:
/// senza permesso il telefono resta muto.
/// Dichiara anche come FACOLTATIVE alcune caratteristiche hardware che Google Play altrimenti dà per obbligatorie:
/// con l'orientamento bloccato in verticale (dalla 0.62) Play deduce "android.hardware.screen.portrait" obbligatorio
/// ed esclude migliaia di dispositivi solo orizzontali (TV, Chromebook, box); il permesso del microfono fa lo stesso
/// con "android.hardware.microphone" (i livelli col microfono hanno un'alternativa senza). Marca l'app come gioco
/// (android:appCategory="game").
/// </summary>
public class GotchaAndroidManifest : IPostGenerateGradleAndroidProject
{
    const string AndroidNs = "http://schemas.android.com/apk/res/android";
    static readonly string[] Permissions = { "android.permission.VIBRATE" };
    static readonly string[] OptionalFeatures = { "android.hardware.screen.portrait", "android.hardware.microphone" };

    public int callbackOrder => 1;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string manifest = Path.Combine(path, "src", "main", "AndroidManifest.xml");
        if (!File.Exists(manifest)) return;
        var doc = new XmlDocument();
        doc.Load(manifest);
        XmlElement root = doc.DocumentElement;
        if (root == null) return;
        bool changed = false;
        foreach (string permission in Permissions)
        {
            bool present = false;
            foreach (XmlNode n in root.SelectNodes("uses-permission"))
                if (n is XmlElement e && e.GetAttribute("name", AndroidNs) == permission) present = true;
            if (present) continue;
            XmlElement p = doc.CreateElement("uses-permission");
            p.SetAttribute("name", AndroidNs, permission);
            root.PrependChild(p);
            changed = true;
        }
        foreach (string feature in OptionalFeatures)
        {
            XmlElement found = null;
            foreach (XmlNode n in root.SelectNodes("uses-feature"))
                if (n is XmlElement e && e.GetAttribute("name", AndroidNs) == feature) found = e;
            if (found == null)
            {
                found = doc.CreateElement("uses-feature");
                found.SetAttribute("name", AndroidNs, feature);
                root.PrependChild(found);
            }
            if (found.GetAttribute("required", AndroidNs) != "false")
            {
                found.SetAttribute("required", AndroidNs, "false");
                changed = true;
            }
        }
        // Gotcha è un gioco: con targetSdk 36 (Android 16) i giochi sono esclusi dall'obbligo di ruotare e
        // ridimensionarsi sugli schermi grandi, quindi il verticale fisso resta valido anche su tablet e pieghevoli
        if (root.SelectSingleNode("application") is XmlElement app && app.GetAttribute("appCategory", AndroidNs) != "game")
        {
            app.SetAttribute("appCategory", AndroidNs, "game");
            changed = true;
        }
        if (changed) doc.Save(manifest);
    }
}
#endif
