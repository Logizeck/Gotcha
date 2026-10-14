#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Levels;
using Gotcha.Sensors;
using Gotcha.UI;

/// <summary>
/// Genera da codice tutto ciò che non è script: sprite segnaposto, prefab del livello pupazzo di neve,
/// LevelDefinition, LevelCatalog, scena Main e impostazioni Android.
/// Parte da solo alla prima apertura (se la scena non esiste) ed è rilanciabile da Gotcha > Build Scene and Levels.
/// ATTENZIONE: rilanciarlo ricrea scena e prefab del pupazzo (le modifiche fatte a mano a quei file si perdono).
/// </summary>
[InitializeOnLoad]
public static class GotchaProjectBuilder
{
    const string ScenePath = "Assets/Scenes/Main.unity";
    const string SpriteDir = "Assets/Placeholder/Sprites";
    const string LevelDir = "Assets/Levels";
    const string SnowmanDir = "Assets/Levels/Snowman";

    static readonly Color Sky = new Color(0.62f, 0.82f, 0.96f);
    static readonly Color Snow = new Color(0.97f, 0.98f, 1f);
    static readonly Color SnowShade = new Color(0.86f, 0.91f, 0.97f);
    static readonly Color Coal = new Color(0.13f, 0.13f, 0.15f);
    static readonly Color CarrotOrange = new Color(1f, 0.5f, 0.1f);

    static GotchaProjectBuilder()
    {
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(ScenePath)) BuildAll();
        };
    }

    [MenuItem("Gotcha/Build Scene and Levels")]
    public static void BuildAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EnsureFolder(SpriteDir);
        EnsureFolder(SnowmanDir);
        EnsureFolder("Assets/Scenes");

        Sprite circle = MakeSprite("circle", Circle(256), 256, Vector4.zero);
        Sprite square = MakeSprite("square", Square(64), 64, Vector4.zero);
        Sprite triangle = MakeSprite("triangle", Triangle(256), 256, Vector4.zero);
        Sprite rounded = MakeSprite("rounded", RoundedRect(128, 40), 100, new Vector4(48, 48, 48, 48));

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject snowmanPrefab = BuildSnowmanPrefab(circle, square, triangle);

        LevelDefinition snowman = LoadOrCreate<LevelDefinition>(SnowmanDir + "/Snowman.asset");
        snowman.levelId = "snowman";
        snowman.title = "SNOWMAN";
        snowman.hint = "Give the snowman a face: two eyes, a nose and a mouth.";
        snowman.prefab = snowmanPrefab;
        EditorUtility.SetDirty(snowman);

        LevelCatalog catalog = LoadOrCreate<LevelCatalog>(LevelDir + "/LevelCatalog.asset");
        if (!catalog.levels.Contains(snowman)) catalog.levels.Insert(0, snowman);
        catalog.levels.RemoveAll(l => l == null);
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();

        BuildScene(catalog, rounded);
        ConfigurePlayer();
        Debug.Log("[Gotcha] Scena e livelli generati. Apri " + ScenePath + " e premi Play.");
    }

    // ---------- Scena ----------

    static void BuildScene(LevelCatalog catalog, Sprite rounded)
    {
        Scene scene = SceneManager.GetActiveScene();

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Sky;
        camGo.transform.position = new Vector3(0, 0, -10);
        camGo.AddComponent<AudioListener>();

        new GameObject("SensorHub").AddComponent<SensorHub>();
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var flowGo = new GameObject("GameFlow");
        GameFlow flow = flowGo.AddComponent<GameFlow>();
        var host = new GameObject("LevelHost").transform;
        host.SetParent(flowGo.transform, false);
        SetField(flow, "catalog", catalog);
        SetField(flow, "levelHost", host);

        var uiGo = new GameObject("GameUI");
        GameUI ui = uiGo.AddComponent<GameUI>();
        SetField(ui, "flow", flow);
        SetField(ui, "roundedSprite", rounded);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
    }

    // ---------- Livello 1: pupazzo di neve ----------

    static GameObject BuildSnowmanPrefab(Sprite circle, Sprite square, Sprite triangle)
    {
        var root = new GameObject("Level_Snowman");
        SnowmanLevel level = root.AddComponent<SnowmanLevel>();

        // Terreno innevato
        Visual(root.transform, "Ground", square, Snow, new Vector2(0, -4.6f), new Vector2(30, 3.2f), 0);
        Visual(root.transform, "GroundEdge", square, SnowShade, new Vector2(0, -3.05f), new Vector2(30, 0.1f), 1);

        // Pupazzo: tre palle e i punti dove agganciare la faccia
        var snowman = new GameObject("Snowman").transform;
        snowman.SetParent(root.transform, false);
        Visual(snowman, "Bottom", circle, Snow, new Vector2(0, -2.1f), Vector2.one * 2.4f, 2);
        Visual(snowman, "Middle", circle, Snow, new Vector2(0, -0.25f), Vector2.one * 1.8f, 3);
        Visual(snowman, "Head", circle, Snow, new Vector2(0, 1.2f), Vector2.one * 1.35f, 4);
        Visual(snowman, "Button1", circle, Coal, new Vector2(0, -0.05f), Vector2.one * 0.16f, 5);
        Visual(snowman, "Button2", circle, Coal, new Vector2(0, -0.45f), Vector2.one * 0.16f, 5);

        Slot(snowman, "Slot_EyeL", "eye", new Vector2(-0.26f, 1.4f), circle);
        Slot(snowman, "Slot_EyeR", "eye", new Vector2(0.26f, 1.4f), circle);
        Slot(snowman, "Slot_Nose", "carrot", new Vector2(0.18f, 1.15f), circle);
        Slot(snowman, "Slot_Mouth", "mouth", new Vector2(0, 0.83f), circle);

        // Pezzi da trascinare, appoggiati sulla neve
        Item(root.transform, "Eye1", "eye", new Vector2(-2.1f, -4.0f), circle, Coal, Vector2.one * 0.24f);
        Item(root.transform, "Eye2", "eye", new Vector2(-1.4f, -4.3f), circle, Coal, Vector2.one * 0.24f);
        Item(root.transform, "Carrot", "carrot", new Vector2(1.1f, -4.1f), triangle, CarrotOrange, new Vector2(0.6f, 0.2f));
        Item(root.transform, "Mouth", "mouth", new Vector2(-0.2f, -4.4f), square, Coal, new Vector2(0.5f, 0.1f));

        // Coniglio affamato (Easter egg: gli si porta la carota)
        var bunny = new GameObject("Bunny").transform;
        bunny.SetParent(root.transform, false);
        bunny.localPosition = new Vector3(2.0f, -2.55f, 0);
        Color fur = new Color(0.93f, 0.93f, 0.95f);
        Visual(bunny, "EarL", circle, fur, new Vector2(-0.14f, 0.62f), new Vector2(0.18f, 0.6f), 6);
        Visual(bunny, "EarR", circle, fur, new Vector2(0.14f, 0.62f), new Vector2(0.18f, 0.6f), 6);
        Visual(bunny, "InnerL", circle, new Color(1f, 0.72f, 0.8f), new Vector2(-0.14f, 0.6f), new Vector2(0.08f, 0.4f), 7);
        Visual(bunny, "InnerR", circle, new Color(1f, 0.72f, 0.8f), new Vector2(0.14f, 0.6f), new Vector2(0.08f, 0.4f), 7);
        Visual(bunny, "Body", circle, fur, new Vector2(0, -0.2f), new Vector2(0.8f, 0.7f), 6);
        Visual(bunny, "Head", circle, fur, new Vector2(0, 0.22f), Vector2.one * 0.55f, 7);
        Visual(bunny, "EyeL", circle, Coal, new Vector2(-0.1f, 0.27f), Vector2.one * 0.07f, 8);
        Visual(bunny, "EyeR", circle, Coal, new Vector2(0.1f, 0.27f), Vector2.one * 0.07f, 8);
        Visual(bunny, "Nose", circle, new Color(1f, 0.55f, 0.65f), new Vector2(0, 0.17f), Vector2.one * 0.08f, 8);
        CircleCollider2D eggArea = bunny.gameObject.AddComponent<CircleCollider2D>();
        eggArea.isTrigger = true;
        eggArea.radius = 0.55f;
        EasterEggTrigger egg = bunny.gameObject.AddComponent<EasterEggTrigger>();
        egg.eggId = SnowmanEggId;
        egg.triggerItemId = "carrot";

        SetField(level, "snowman", snowman);
        SetField(level, "bunny", bunny);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, SnowmanDir + "/Level_Snowman.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    const string SnowmanEggId = "snowman_bunny";

    static void Slot(Transform parent, string name, string acceptedId, Vector2 pos, Sprite circle)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        CircleCollider2D c = go.AddComponent<CircleCollider2D>();
        c.isTrigger = true;
        c.radius = 0.22f;
        DropSlot slot = go.AddComponent<DropSlot>();
        slot.acceptedId = acceptedId;
        // Segno appena visibile, così si capisce dove vanno i pezzi
        Visual(go.transform, "Mark", circle, new Color(0.75f, 0.82f, 0.92f, 0.6f), Vector2.zero, Vector2.one * 0.12f, 5);
    }

    static void Item(Transform parent, string name, string itemId, Vector2 pos, Sprite sprite, Color color, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        CircleCollider2D c = go.AddComponent<CircleCollider2D>();
        c.radius = 0.4f;   // area di presa più grande del disegno, per le dita
        Draggable2D d = go.AddComponent<Draggable2D>();
        d.itemId = itemId;
        Visual(go.transform, "Visual", sprite, color, Vector2.zero, size, 10);
    }

    static Transform Visual(Transform parent, string name, Sprite sprite, Color color, Vector2 pos, Vector2 size, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        return go.transform;
    }

    // ---------- Impostazioni Android ----------

    static void ConfigurePlayer()
    {
        PlayerSettings.productName = "Gotcha!";
        PlayerSettings.companyName = "Gotcha";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.gotcha.game");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        AssetDatabase.SaveAssets();
    }

    // ---------- Sprite segnaposto (generati, nessuna grafica esterna) ----------

    static Sprite MakeSprite(string name, Texture2D tex, float pixelsPerUnit, Vector4 border)
    {
        string path = SpriteDir + "/" + name + ".png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.spriteBorder = border;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Texture2D NewTexture(int w, int h)
    {
        return new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
    }

    static Texture2D Circle(int size)
    {
        Texture2D t = NewTexture(size, size);
        float r = size / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                t.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(r - d)));
            }
        t.Apply();
        return t;
    }

    static Texture2D Square(int size)
    {
        Texture2D t = NewTexture(size, size);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                t.SetPixel(x, y, Color.white);
        t.Apply();
        return t;
    }

    /// <summary>Triangolo con la punta verso destra (la carota).</summary>
    static Texture2D Triangle(int size)
    {
        Texture2D t = NewTexture(size, size);
        float half = size / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float allowed = half * (1f - (x + 0.5f) / size);   // semi-altezza che si restringe verso la punta
                float a = Mathf.Clamp01(allowed - Mathf.Abs(y + 0.5f - half) + 0.5f);
                t.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        t.Apply();
        return t;
    }

    static Texture2D RoundedRect(int size, int radius)
    {
        Texture2D t = NewTexture(size, size);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float cx = Mathf.Clamp(x + 0.5f, radius, size - radius);
                float cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                t.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(radius - d + 0.5f)));
            }
        t.Apply();
        return t;
    }

    // ---------- Utilità ----------

    static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;
        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    static void SetField(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null) { Debug.LogError("[Gotcha] Campo non trovato: " + target.GetType().Name + "." + field); return; }
        p.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
#endif
