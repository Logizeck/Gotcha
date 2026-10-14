#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.EditorArt;
using Gotcha.Levels;
using Gotcha.Sensors;
using Gotcha.UI;

/// <summary>
/// Genera da codice tutto ciò che non è script: sprite "doodle" (da Editor/Art/DoodleSprites.cs), materiali,
/// prefab riutilizzabili (Chiave, Porta, Carota, Carbone), prefab dei livelli, LevelDefinition, LevelCatalog,
/// scena Main e impostazioni Android.
/// Parte da solo alla prima apertura (se la scena non esiste) ed è rilanciabile da Gotcha > Build Scene and Levels.
/// ATTENZIONE: rilanciarlo ricrea scena, sprite e prefab generati (le modifiche fatte a mano a quei file si perdono).
/// </summary>
[InitializeOnLoad]
public static class GotchaProjectBuilder
{
    const string ScenePath = "Assets/Scenes/Main.unity";
    const string SpriteDir = "Assets/Art/Generated";
    const string MaterialDir = "Assets/Materials";
    const string PrefabDir = "Assets/Prefabs";
    const string LevelDir = "Assets/Levels";
    const int BallLayer = 1;   // TransparentFX: la sfera non si riflette in se stessa

    static readonly Color Paper = new Color(1f, 0.97f, 0.9f);
    static readonly Color InkColor = new Color(0.17f, 0.13f, 0.2f);

    static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    static readonly Dictionary<string, Vector2> centers = new Dictionary<string, Vector2>();

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

        foreach (string dir in new[] { SpriteDir, MaterialDir, PrefabDir, LevelDir, "Assets/Scenes" }) EnsureFolder(dir);

        try
        {
            List<DoodleArt> arts = DoodleCatalog.All();
            for (int i = 0; i < arts.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Gotcha!", "Disegno " + arts[i].Name, i / (float)arts.Count);
                MakeSprite(arts[i]);
            }
        }
        finally { EditorUtility.ClearProgressBar(); }

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject keyPrefab = BuildKeyPrefab();
        GameObject doorPrefab = BuildDoorPrefab();
        GameObject carrotPrefab = BuildCarrotPrefab();
        GameObject coalPrefab = BuildCoalPrefab();

        var levels = new List<LevelDefinition>
        {
            Define("KeyDoor", "key_door", "PINCH & OPEN",
                "That key is far too big for the lock. Can you change its size?",
                false, BuildKeyDoorLevel(keyPrefab, doorPrefab)),
            Define("Maze", "tilt_maze", "TILT MAZE",
                "No buttons here: the phone itself moves the ball. Reach the glowing star.",
                true, BuildMazeLevel()),
            Define("Snowman", "snowman", "SNOWMAN",
                "Give the snowman a face. Put the pieces wherever you like... and the mouth? Draw it!",
                false, BuildSnowmanLevel(carrotPrefab, coalPrefab)),
        };

        LevelCatalog catalog = LoadOrCreate<LevelCatalog>(LevelDir + "/LevelCatalog.asset");
        catalog.levels = levels;
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();

        BuildScene(catalog);
        ConfigurePlayer();
        Debug.Log("[Gotcha] Scena e livelli generati. Apri " + ScenePath + " e premi Play.");
    }

    static LevelDefinition Define(string folder, string id, string title, string hint, bool keepAwake, GameObject prefab)
    {
        LevelDefinition def = LoadOrCreate<LevelDefinition>(LevelDir + "/" + folder + "/" + folder + ".asset");
        def.levelId = id;
        def.title = title;
        def.hint = hint;
        def.keepScreenAwake = keepAwake;
        def.prefab = prefab;
        EditorUtility.SetDirty(def);
        return def;
    }

    // ---------- Scena ----------

    static void BuildScene(LevelCatalog catalog)
    {
        Scene scene = SceneManager.GetActiveScene();

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Paper;
        camGo.transform.position = new Vector3(0, 0, -10);
        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<CameraFit>();

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
        SetField(ui, "roundedSprite", sprites["ui_panel"]);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
    }

    // ---------- Oggetti riutilizzabili ----------

    /// <summary>Chiave con il pivot sulla punta: è la punta che deve entrare nella serratura.</summary>
    static GameObject BuildKeyPrefab()
    {
        var root = new GameObject("Key");
        BoxCollider2D grab = root.AddComponent<BoxCollider2D>();
        grab.offset = new Vector2(-0.6f, 0);
        grab.size = new Vector2(1.5f, 0.85f);   // area di presa generosa, per le dita
        Draggable2D drag = root.AddComponent<Draggable2D>();
        drag.itemId = "key";
        drag.returnOnMiss = false;              // resta dove la lasci
        PinchScalable pinch = root.AddComponent<PinchScalable>();
        pinch.minScale = 0.3f;
        pinch.maxScale = 1.9f;
        Art(root.transform, "Art", "key", 20);
        return SavePrefab(root, PrefabDir + "/Key.prefab");
    }

    /// <summary>Porta con anta (cardini a sinistra) e serratura che accetta solo chiavi piccole.</summary>
    static GameObject BuildDoorPrefab()
    {
        var root = new GameObject("Door");
        Art(root.transform, "Frame", "door_frame", 1);

        var panel = new GameObject("Panel").transform;
        panel.SetParent(root.transform, false);
        panel.localPosition = new Vector3(-1.1f, 0, 0);
        Art(panel, "Art", "door_panel", 3);

        var keyhole = new GameObject("Keyhole");
        keyhole.transform.SetParent(panel, false);
        keyhole.transform.localPosition = new Vector3(1.85f, -0.15f, 0);
        Art(keyhole.transform, "Art", "keyhole", 5);
        CircleCollider2D hole = keyhole.AddComponent<CircleCollider2D>();
        hole.isTrigger = true;
        hole.radius = 0.3f;
        DropSlot slot = keyhole.AddComponent<DropSlot>();
        slot.acceptedId = "key";
        slot.maxItemScale = 0.6f;

        LockedDoor door = root.AddComponent<LockedDoor>();
        SetField(door, "keyhole", slot);
        SetField(door, "panel", panel);
        return SavePrefab(root, PrefabDir + "/Door.prefab");
    }

    static GameObject BuildCarrotPrefab()
    {
        var root = new GameObject("Carrot");
        CircleCollider2D c = root.AddComponent<CircleCollider2D>();
        c.radius = 0.38f;
        Draggable2D d = root.AddComponent<Draggable2D>();
        d.itemId = "carrot";
        d.returnOnMiss = false;
        Art(root.transform, "Art", "carrot", 11);
        return SavePrefab(root, PrefabDir + "/Carrot.prefab");
    }

    static GameObject BuildCoalPrefab()
    {
        var root = new GameObject("Coal");
        CircleCollider2D c = root.AddComponent<CircleCollider2D>();
        c.radius = 0.32f;
        Draggable2D d = root.AddComponent<Draggable2D>();
        d.itemId = "eye";
        d.returnOnMiss = false;
        Art(root.transform, "Art", "coal", 10);
        return SavePrefab(root, PrefabDir + "/Coal.prefab");
    }

    // ---------- Livello 1: Pizzica e apri ----------

    static GameObject BuildKeyDoorLevel(GameObject keyPrefab, GameObject doorPrefab)
    {
        var root = new GameObject("Level_KeyDoor");
        root.AddComponent<KeyDoorLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);

        GameObject door = Instance(doorPrefab, root.transform, new Vector2(0, -0.75f));
        door.name = "Door";

        // Chiave enorme sul pavimento: va rimpicciolita prima di entrare nella serratura
        GameObject key = Instance(keyPrefab, root.transform, new Vector2(1.5f, -3.6f));
        key.transform.localScale = Vector3.one * 1.7f;

        return SavePrefab(root, LevelDir + "/KeyDoor/Level_KeyDoor.prefab");
    }

    // ---------- Livello 2: labirinto a inclinazione ----------

    const float WallT = 0.22f;        // spessore delle pareti (= nucleo dello sprite maze_wall)
    const float WallMargin = 0.12f;   // alone al neon attorno al nucleo, nello sprite

    static GameObject BuildMazeLevel()
    {
        var root = new GameObject("Level_Maze");
        MazeLevel level = root.AddComponent<MazeLevel>();
        TiltGravity tilt = root.AddComponent<TiltGravity>();
        tilt.strength = 32f;
        tilt.response = 0.65f;
        Art(root.transform, "Night", "maze_bg", -20, false);

        var maze = new GameObject("Maze").transform;
        maze.SetParent(root.transform, false);
        maze.localPosition = new Vector3(0, -0.4f, 0);

        // Recinto esterno: la sfera non può uscire dallo schermo
        Rect(maze, "Arena", 2.8f, 3.5f, null);
        // Anelli dall'esterno all'interno, ognuno con un varco su un lato diverso
        Rect(maze, "Ring1", 2.1f, 2.1f, "top");
        Rect(maze, "Ring2", 1.4f, 1.4f, "bottom");
        Rect(maze, "Ring3", 0.7f, 0.7f, "top");
        // Sbarramenti nei corridoi: obbligano a fare il giro lungo
        HWall(maze, "Block1", 1.4f, 2.1f, 0f);
        HWall(maze, "Block2", -1.4f, -0.7f, 0f);

        // Stella luminosa al centro
        var goal = new GameObject("Goal");
        goal.transform.SetParent(maze, false);
        Transform star = Art(goal.transform, "Art", "goal_star", 2);
        star.gameObject.AddComponent<Pulse>();
        CircleCollider2D goalArea = goal.AddComponent<CircleCollider2D>();
        goalArea.isTrigger = true;
        goalArea.radius = 0.22f;
        GoalZone zone = goal.AddComponent<GoalZone>();
        zone.requiredTag = "Player";
        zone.holdSeconds = 0.25f;

        // Sfera cromata, fuori dal labirinto (in basso a sinistra)
        var ball = new GameObject("Ball");
        ball.tag = "Player";
        ball.layer = BallLayer;
        ball.transform.SetParent(maze, false);
        ball.transform.localPosition = new Vector3(-2.35f, -3.0f, 0);
        ball.transform.localScale = Vector3.one * 0.34f;
        SpriteRenderer sr = ball.AddComponent<SpriteRenderer>();
        sr.sprite = sprites["chrome_ball"];
        sr.sharedMaterial = ChromeMaterial();
        sr.sortingOrder = 10;
        ball.AddComponent<CircleCollider2D>().radius = 0.5f;
        Rigidbody2D rb = ball.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.freezeRotation = true;                           // il riflesso non deve girare
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;     // la gravità cambia di continuo
        rb.sharedMaterial = BallPhysics();
        ChromeBall chrome = ball.AddComponent<ChromeBall>();
        chrome.background = new Color(0.04f, 0.055f, 0.11f);

        SetField(level, "ball", rb);
        return SavePrefab(root, LevelDir + "/Maze/Level_Maze.prefab");
    }

    static PhysicsMaterial2D BallPhysics()
    {
        string path = LevelDir + "/Maze/Ball.physicsMaterial2D";
        EnsureFolder(LevelDir + "/Maze");
        PhysicsMaterial2D mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
        if (mat == null)
        {
            mat = new PhysicsMaterial2D("Ball");
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.friction = 0.05f;
        mat.bounciness = 0.25f;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Material ChromeMaterial()
    {
        string path = MaterialDir + "/ChromeBall.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Gotcha/ChromeBall");
        if (shader == null)
        {
            Debug.LogError("[Gotcha] Shader Gotcha/ChromeBall non trovato: la sfera userà lo shader standard degli sprite.");
            shader = Shader.Find("Sprites/Default");
        }
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.shader = shader;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Material StrokeMaterial()
    {
        string path = MaterialDir + "/Stroke.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    /// <summary>Rettangolo di muri (semi-larghezza hx, semi-altezza hy) con un varco opzionale al centro di un lato.</summary>
    static void Rect(Transform parent, string name, float hx, float hy, string gapSide)
    {
        const float gap = 0.35f;   // metà larghezza del varco
        var g = new GameObject(name).transform;
        g.SetParent(parent, false);

        if (gapSide == "top") { HWall(g, "TopL", -hx, -gap, hy); HWall(g, "TopR", gap, hx, hy); }
        else HWall(g, "Top", -hx, hx, hy);
        if (gapSide == "bottom") { HWall(g, "BottomL", -hx, -gap, -hy); HWall(g, "BottomR", gap, hx, -hy); }
        else HWall(g, "Bottom", -hx, hx, -hy);
        VWall(g, "Left", -hx, -hy, hy);
        VWall(g, "Right", hx, -hy, hy);
    }

    static void HWall(Transform parent, string name, float x1, float x2, float y)
    {
        Wall(parent, name, new Vector2((x1 + x2) / 2f, y), new Vector2(x2 - x1 + WallT, WallT));
    }

    static void VWall(Transform parent, string name, float x, float y1, float y2)
    {
        Wall(parent, name, new Vector2(x, (y1 + y2) / 2f), new Vector2(WallT, y2 - y1 + WallT));
    }

    /// <summary>Parete al neon: sprite 9-slice allungato, collider grande quanto il nucleo.</summary>
    static void Wall(Transform parent, string name, Vector2 pos, Vector2 core)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprites["maze_wall"];
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = core + Vector2.one * (2f * WallMargin);
        sr.sortingOrder = 5;
        go.AddComponent<BoxCollider2D>().size = core;
    }

    // ---------- Livello 3: pupazzo di neve ----------

    static GameObject BuildSnowmanLevel(GameObject carrotPrefab, GameObject coalPrefab)
    {
        var root = new GameObject("Level_Snowman");
        SnowmanLevel level = root.AddComponent<SnowmanLevel>();
        Art(root.transform, "Landscape", "snow_bg", -20, false);

        // Pupazzo: pivot ai piedi, così si scioglie verso il basso
        var snowman = new GameObject("Snowman").transform;
        snowman.SetParent(root.transform, false);
        snowman.localPosition = new Vector3(0, -3.0f, 0);
        SpriteRenderer body = Art(snowman, "Body", "snowman", 2).GetComponent<SpriteRenderer>();
        SpriteRenderer blush = Art(snowman, "Blush", "blush", 4, true, new Vector2(0, 3.93f)).GetComponent<SpriteRenderer>();

        // Bocca da disegnare col dito: l'area è la faccia
        var pen = new GameObject("MouthPen");
        pen.transform.SetParent(snowman, false);
        pen.transform.localPosition = new Vector3(0, 4.0f, 0);
        CircleCollider2D face = pen.AddComponent<CircleCollider2D>();
        face.isTrigger = true;
        face.radius = 0.52f;
        StrokeDrawer drawer = pen.AddComponent<StrokeDrawer>();
        drawer.width = 0.07f;
        drawer.color = InkColor;
        drawer.minLength = 0.25f;
        drawer.sortingOrder = 12;
        drawer.material = StrokeMaterial();

        // Easter egg: la carota vicino al sedere
        var butt = new GameObject("Butt");
        butt.transform.SetParent(snowman, false);
        butt.transform.localPosition = new Vector3(0, 0.35f, 0);
        CircleCollider2D buttArea = butt.AddComponent<CircleCollider2D>();
        buttArea.isTrigger = true;
        buttArea.radius = 0.55f;
        EasterEggTrigger egg = butt.AddComponent<EasterEggTrigger>();
        egg.eggId = "snowman_butt";
        egg.triggerItemId = "carrot";

        var smokeGo = new GameObject("Smoke");
        smokeGo.transform.SetParent(snowman, false);
        smokeGo.transform.localPosition = new Vector3(0, 3.7f, 0);
        SmokeEmitter smoke = smokeGo.AddComponent<SmokeEmitter>();
        smoke.puffSprite = sprites["puff"];
        smoke.spread = 0.6f;

        var puddle = new GameObject("Puddle").transform;
        puddle.SetParent(root.transform, false);
        puddle.localPosition = new Vector3(0, -2.95f, 0);
        Art(puddle, "Art", "puddle", 3);

        // Fumetto con testo
        var bubbleGo = new GameObject("Bubble");
        bubbleGo.transform.SetParent(root.transform, false);
        bubbleGo.transform.localPosition = new Vector3(0.95f, 2.45f, 0);
        Art(bubbleGo.transform, "Art", "bubble", 60);
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(bubbleGo.transform, false);
        textGo.transform.localPosition = new Vector3(0, 0.07f, -0.01f);
        TextMesh text = textGo.AddComponent<TextMesh>();
        Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/LuckiestGuy-Regular.ttf");
        if (font != null)
        {
            text.font = font;
            textGo.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }
        text.fontSize = 80;
        text.characterSize = 0.05f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = InkColor;
        textGo.GetComponent<MeshRenderer>().sortingOrder = 61;
        SpeechBubble bubble = bubbleGo.AddComponent<SpeechBubble>();
        bubble.text = text;
        bubble.maxTextWidth = 2.5f;

        // Pezzi sparsi nella neve: nessuna posizione suggerita sul pupazzo
        Instance(coalPrefab, root.transform, new Vector2(-2.0f, -3.95f)).name = "Eye1";
        Instance(coalPrefab, root.transform, new Vector2(-1.35f, -4.35f)).name = "Eye2";
        Instance(carrotPrefab, root.transform, new Vector2(1.55f, -4.1f)).name = "Carrot";

        SetField(level, "snowman", snowman);
        SetField(level, "body", body);
        SetField(level, "blush", blush);
        SetField(level, "puddle", puddle);
        SetField(level, "bubble", bubble);
        SetField(level, "smoke", smoke);
        SetField(level, "mouthPen", drawer);
        return SavePrefab(root, LevelDir + "/Snowman/Level_Snowman.prefab");
    }

    // ---------- Aiuti per la costruzione ----------

    /// <summary>Figlio con uno sprite doodle, posizionato in modo che le coordinate del disegno coincidano con quelle locali.</summary>
    static Transform Art(Transform parent, string name, string sprite, int order, bool wobble = true, Vector2 offset = default)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = centers[sprite] + offset;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprites[sprite];
        sr.sortingOrder = order;
        if (wobble) go.AddComponent<DoodleWobble>();
        return go.transform;
    }

    static GameObject Instance(GameObject prefab, Transform parent, Vector2 pos)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        return go;
    }

    static GameObject SavePrefab(GameObject root, string path)
    {
        EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
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
        // 64 bit obbligatorio: molti telefoni recenti rifiutano gli APK solo a 32 bit
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        AssetDatabase.SaveAssets();
        EnsureLegacyInput();
    }

    /// <summary>
    /// Il gioco legge tocchi e sensori con il vecchio Input Manager. Unity 6 di default usa solo il nuovo
    /// Input System: in quel caso si passa a "Both" (richiede il riavvio dell'Editor).
    /// </summary>
    static void EnsureLegacyInput()
    {
        Object[] all = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
        if (all == null || all.Length == 0) return;
        var so = new SerializedObject(all[0]);
        SerializedProperty p = so.FindProperty("activeInputHandler");
        if (p == null || p.intValue != 1) return;   // 0 = Old, 1 = New, 2 = Both

        p.intValue = 2;
        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
        if (EditorUtility.DisplayDialog("Gotcha!",
                "Active Input Handling impostato su \"Both\": serve per tocchi e sensori. Unity deve riavviarsi.",
                "Riavvia ora", "Più tardi"))
            EditorApplication.OpenProject(Directory.GetCurrentDirectory());
    }

    // ---------- Sprite doodle ----------

    static void MakeSprite(DoodleArt art)
    {
        DoodlePainter p = art.Painter;
        string path = SpriteDir + "/" + art.Name + ".png";

        var tex = new Texture2D(p.Width, p.Height, TextureFormat.RGBA32, false);
        var colors = new Color[p.Pixels.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            Rgba c = p.Pixels[i];
            colors[i] = new Color(c.r, c.g, c.b, c.a);
        }
        tex.SetPixels(colors);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = p.Ppu;
        importer.spriteBorder = Vector4.one * art.Border;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;   // linee nette, niente artefatti
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;   // necessario per il 9-slice e per lo shader della sfera
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();

        sprites[art.Name] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        centers[art.Name] = new Vector2(p.CenterX, p.CenterY);
    }

    // ---------- Utilità ----------

    static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;
        EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
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
