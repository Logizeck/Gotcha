#if UNITY_EDITOR
using System.IO;
using System.Xml;
using UnityEditor.Android;

/// <summary>
/// Aggiunge il permesso VIBRATE al manifest Android a ogni build (APK e AAB). Unity lo mette da solo solo se trova
/// Handheld.Vibrate nel codice rimasto dopo lo stripping, e la vibrazione del gioco passa dal Vibrator di sistema:
/// senza permesso il telefono resta muto.
/// </summary>
public class GotchaAndroidManifest : IPostGenerateGradleAndroidProject
{
    const string AndroidNs = "http://schemas.android.com/apk/res/android";
    static readonly string[] Permissions = { "android.permission.VIBRATE" };

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
        if (changed) doc.Save(manifest);
    }
}
#endif
