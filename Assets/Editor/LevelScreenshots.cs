#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Gotcha.Levels;

/// <summary>
/// Menu Gotcha ▸ Screenshot di tutti i livelli: fotografa la scena iniziale di ogni livello del catalogo
/// (720×1280, come un telefono) e la salva in Screenshots/level_001.png, ... nella cartella del progetto.
/// Serve per l'elenco dei livelli in Excel (Tools/levels_xlsx.py usa queste immagini se ci sono).
/// </summary>
public static class LevelScreenshots
{
    const int Width = 720, Height = 1280;

    [MenuItem("Gotcha/Screenshot di tutti i livelli")]
    public static void CaptureAll()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>("Assets/Levels/LevelCatalog.asset");
        if (catalog == null) { Debug.LogError("[Gotcha] LevelCatalog non trovato: lancia prima il builder."); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        string dir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Screenshots");
        Directory.CreateDirectory(dir);

        var scene = EditorSceneManager.NewPreviewScene();
        var camGo = new GameObject("ShotCamera");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camGo, scene);
        Camera cam = camGo.AddComponent<Camera>();
        cam.scene = scene;
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(1f, 0.97f, 0.9f);
        cam.transform.position = new Vector3(0, 0, -10);
        var rt = new RenderTexture(Width, Height, 24);
        cam.targetTexture = rt;
        cam.aspect = Width / (float)Height;
        var tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        try
        {
            for (int i = 0; i < catalog.levels.Count; i++)
            {
                LevelDefinition def = catalog.levels[i];
                if (def == null || def.prefab == null) continue;
                EditorUtility.DisplayProgressBar("Gotcha!", "Screenshot " + (i + 1) + " " + def.title, i / (float)catalog.levels.Count);
                var go = (GameObject)PrefabUtility.InstantiatePrefab(def.prefab, scene);
                cam.Render();
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                tex.Apply();
                RenderTexture.active = null;
                File.WriteAllBytes(Path.Combine(dir, "level_" + (i + 1).ToString("000") + ".png"), tex.EncodeToPNG());
                Object.DestroyImmediate(go);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            EditorSceneManager.ClosePreviewScene(scene);
        }
        Debug.Log("[Gotcha] Screenshot salvati in " + dir);
        EditorUtility.RevealInFinder(dir);
    }
}
#endif
