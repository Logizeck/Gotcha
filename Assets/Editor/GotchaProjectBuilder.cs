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
using Gotcha.Levels;
using Gotcha.Sensors;
using Gotcha.UI;

/// <summary>
/// Genera da codice tutto ciò che non è script: sprite segnaposto, prefab riutilizzabili (Chiave, Porta),
/// prefab dei livelli, LevelDefinition, LevelCatalog, scena Main e impostazioni Android.
/// Parte da solo alla prima apertura (se la scena non esiste) ed è rilanciabile da Gotcha > Build Scene and Levels.
/// ATTENZIONE: rilanciarlo ricrea scena e prefab generati (le modifiche fatte a mano a quei file si perdono).
/// </summary>
[InitializeOnLoad]
public static class GotchaProjectBuilder
{
    const string ScenePath = "Assets/Scenes/Main.unity";
    const string SpriteDir = "Assets/Placeholder/Sprites";
    const string PrefabDir = "Assets/Prefabs";
    const string LevelDir = "Assets/Levels";

    static readonly Color Sky = new Color(0.62f, 0.82f, 0.96f);
    static readonly Color Snow = new Color(0.97f, 0.98f, 1f);
    static readonly Color SnowShade = new Color(0.86f, 0.91f, 0.97f);
    static readonly Color Coal = new Color(0.13f, 0.13f, 0.15f);
    static readonly Color CarrotOrange = new Color(1f, 0.5f, 0.1f);
    static readonly Color Gold = new Color(1f, 0.8f, 0.2f);
    static readonly Color GoldDark = new Color(0.8f, 0.58f, 0.1f);
    static readonly Color Wood = new Color(0.55f, 0.33f, 0.18f);
    static readonly Color WoodDark = new Color(0.38f, 0.22f, 0.12f);
    static readonly Color RoomWall = new Color(0.93f, 0.87f, 0.76f);
    static readonly Color MazeFloor = new Color(0.1f, 0.11f, 0.2f);
    static readonly Color Neon = new Color(0.2f, 0.95f, 1f);
    static readonly Color Pink = new Color(1f, 0.3f, 0.65f);

    static Sprite circle, square, triangle, rounded;

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
        EnsureFolder(PrefabDir);
        EnsureFolder(LevelDir);
        EnsureFolder("Assets/Scenes");

        circle = MakeSprite("circle", Circle(256), 256, Vector4.zero);
        square = MakeSprite("square", Square(64), 64, Vector4.zero);
        triangle = MakeSprite("triangle", Triangle(256), 256, Vector4.zero);
        rounded = MakeSprite("rounded", RoundedRect(128, 40), 100, new Vector4(48, 48, 48, 48));

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject keyPrefab = BuildKeyPrefab();
        GameObject doorPrefab = BuildDoorPrefab();

        var levels = new List<LevelDefinition>
        {
            Define("KeyDoor", "key_door", "PINCH & OPEN",
                "That key is far too big for the lock. Can you change its size?",
                BuildKeyDoorLevel(keyPrefab, doorPrefab)),
            Define("Maze", "tilt_maze", "TILT MAZE",
                "No buttons here: the phone itself moves the ball. Reach the glowing centre.",
                BuildMazeLevel()),
            Define("Snowman", "snowman", "SNOWMAN",
                "Give the snowman a face: two eyes, a nose and a mouth.",
                BuildSnowmanLevel()),
        };

        LevelCatalog catalog = LoadOrCreate<LevelCatalog>(LevelDir + "/LevelCatalog.asset");
        catalog.levels = levels;
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();

        BuildScene(catalog);
        ConfigurePlayer();
        Debug.Log("[Gotcha] Scena e livelli generati. Apri " + ScenePath + " e premi Play.");
    }

    static LevelDefinition Define(string folder, string id, string title, string hint, GameObject prefab)
    {
        LevelDefinition def = LoadOrCreate<LevelDefinition>(LevelDir + "/" + folder + "/" + folder + ".asset");
        def.levelId = id;
        def.title = title;
        def.hint = hint;
        def.prefab = prefab;
        EditorUtility.SetDirty(def);
        return def;
    }

    static GameObject SavePrefab(GameObject root, string path)
    {
        EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
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
        cam.backgroundColor = Sky;
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
        SetField(ui, "roundedSprite", rounded);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
    }

    // ---------- Oggetti riutilizzabili: Chiave e Porta con serratura ----------

    /// <summary>Chiave orizzontale con il pivot sulla punta: è la punta che deve entrare nella serratura.</summary>
    static GameObject BuildKeyPrefab()
    {
        var root = new GameObject("Key");
        BoxCollider2D grab = root.AddComponent<BoxCollider2D>();
        grab.offset = new Vector2(-0.8f, 0);
        grab.size = new Vector2(1.8f, 0.8f);   // area di presa generosa, per le dita
        Draggable2D drag = root.AddComponent<Draggable2D>();
        drag.itemId = "key";
        PinchScalable pinch = root.AddComponent<PinchScalable>();
        pinch.minScale = 0.3f;
        pinch.maxScale = 1.6f;

        Visual(root.transform, "Bow", circle, Gold, new Vector2(-1.4f, 0), Vector2.one * 0.6f, 20);
        Visual(root.transform, "BowHole", circle, GoldDark, new Vector2(-1.4f, 0), Vector2.one * 0.24f, 21);
        Visual(root.transform, "Shaft", square, Gold, new Vector2(-0.6f, 0), new Vector2(1.2f, 0.14f), 20);
        Visual(root.transform, "Tooth1", square, Gold, new Vector2(-0.18f, -0.14f), new Vector2(0.1f, 0.2f), 20);
        Visual(root.transform, "Tooth2", square, Gold, new Vector2(-0.4f, -0.12f), new Vector2(0.1f, 0.16f), 20);
        return SavePrefab(root, PrefabDir + "/Key.prefab");
    }

    /// <summary>Porta con anta (pivot sui cardini, a sinistra) e serratura che accetta solo chiavi piccole.</summary>
    static GameObject BuildDoorPrefab()
    {
        var root = new GameObject("Door");
        const float w = 2.2f, h = 3.6f;

        Visual(root.transform, "Frame", square, WoodDark, Vector2.zero, new Vector2(w + 0.3f, h + 0.3f), 1);
        Visual(root.transform, "Light", square, new Color(1f, 0.97f, 0.75f), Vector2.zero, new Vector2(w, h), 2);

        var panel = new GameObject("Panel").transform;
        panel.SetParent(root.transform, false);
        panel.localPosition = new Vector3(-w / 2f, 0, 0);
        Visual(panel, "Board", square, Wood, new Vector2(w / 2f, 0), new Vector2(w, h), 3);
        Visual(panel, "InsetTop", square, WoodDark, new Vector2(w / 2f, 0.85f), new Vector2(w * 0.7f, 1.2f), 4);
        Visual(panel, "InsetBottom", square, WoodDark, new Vector2(w / 2f, -0.95f), new Vector2(w * 0.7f, 1.2f), 4);
        Visual(panel, "Knob", circle, Gold, new Vector2(w - 0.35f, 0.25f), Vector2.one * 0.22f, 6);

        var keyhole = new GameObject("Keyhole");
        keyhole.transform.SetParent(panel, false);
        keyhole.transform.localPosition = new Vector3(w - 0.35f, -0.15f, 0);
        Visual(keyhole.transform, "Plate", rounded, GoldDark, Vector2.zero, new Vector2(0.3f, 0.5f), 5);
        Visual(keyhole.transform, "Hole", circle, Coal, new Vector2(0, 0.06f), Vector2.one * 0.11f, 6);
        Transform slit = Visual(keyhole.transform, "Slit", triangle, Coal, new Vector2(0, -0.06f), new Vector2(0.18f, 0.12f), 6);
        slit.localRotation = Quaternion.Euler(0, 0, 90);
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

    // ---------- Livello 1: Pizzica e apri ----------

    static GameObject BuildKeyDoorLevel(GameObject keyPrefab, GameObject doorPrefab)
    {
        var root = new GameObject("Level_KeyDoor");
        root.AddComponent<KeyDoorLevel>();

        Visual(root.transform, "Wall", square, RoomWall, new Vector2(0, 1), new Vector2(30, 14), -10);
        Visual(root.transform, "Floor", square, new Color(0.62f, 0.45f, 0.3f), new Vector2(0, -4.6f), new Vector2(30, 4), -9);
        Visual(root.transform, "Skirting", square, WoodDark, new Vector2(0, -2.6f), new Vector2(30, 0.12f), -8);

        var door = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab);
        door.transform.SetParent(root.transform, false);
        door.transform.localPosition = new Vector3(0, -0.8f, 0);   // poggia sul battiscopa

        // Chiave enorme, appoggiata sul pavimento: va rimpicciolita prima di entrare nella serratura
        var key = (GameObject)PrefabUtility.InstantiatePrefab(keyPrefab);
        key.transform.SetParent(root.transform, false);
        key.transform.localPosition = new Vector3(1.2f, -3.6f, 0);
        key.transform.localScale = Vector3.one * 1.4f;

        return SavePrefab(root, LevelDir + "/KeyDoor/Level_KeyDoor.prefab");
    }

    // ---------- Livello 2: labirinto a inclinazione ----------

    const float WallT = 0.15f;

    static GameObject BuildMazeLevel()
    {
        var root = new GameObject("Level_Maze");
        MazeLevel level = root.AddComponent<MazeLevel>();
        root.AddComponent<TiltGravity>().strength = 12f;

        var maze = new GameObject("Maze").transform;
        maze.SetParent(root.transform, false);
        maze.localPosition = new Vector3(0, -0.4f, 0);

        // Coordinate relative al centro del labirinto
        Visual(maze, "Floor", square, MazeFloor, Vector2.zero, new Vector2(5.6f, 7f), -5);

        // Recinto esterno: la sfera non può uscire dallo schermo
        Rect(maze, "Arena", 2.8f, 3.5f, null);
        // Anelli dall'esterno all'interno, ognuno con un varco su un lato diverso
        Rect(maze, "Ring1", 2.1f, 2.1f, "top");
        Rect(maze, "Ring2", 1.4f, 1.4f, "bottom");
        Rect(maze, "Ring3", 0.7f, 0.7f, "top");
        // Sbarramenti nei corridoi: obbligano a fare il giro lungo
        HWall(maze, "Block1", 1.4f, 2.1f, 0f);
        HWall(maze, "Block2", -1.4f, -0.7f, 0f);

        // Centro luminoso
        var goal = new GameObject("Goal");
        goal.transform.SetParent(maze, false);
        Visual(goal.transform, "Glow", circle, new Color(1f, 0.9f, 0.3f, 0.35f), Vector2.zero, Vector2.one * 0.9f, 1);
        Visual(goal.transform, "Core", circle, new Color(1f, 0.95f, 0.5f), Vector2.zero, Vector2.one * 0.4f, 2);
        CircleCollider2D goalArea = goal.AddComponent<CircleCollider2D>();
        goalArea.isTrigger = true;
        goalArea.radius = 0.25f;
        GoalZone zone = goal.AddComponent<GoalZone>();
        zone.requiredTag = "Player";
        zone.holdSeconds = 0.3f;

        // Sfera, fuori dal labirinto (in basso a sinistra)
        var ball = new GameObject("Ball");
        ball.tag = "Player";
        ball.transform.SetParent(maze, false);
        ball.transform.localPosition = new Vector3(-2.35f, -3.0f, 0);
        ball.transform.localScale = Vector3.one * 0.36f;
        SpriteRenderer sr = ball.AddComponent<SpriteRenderer>();
        sr.sprite = circle;
        sr.color = Pink;
        sr.sortingOrder = 10;
        Visual(ball.transform, "Shine", circle, new Color(1f, 1f, 1f, 0.7f), new Vector2(-0.18f, 0.18f), Vector2.one * 0.3f, 11);
        ball.AddComponent<CircleCollider2D>().radius = 0.5f;
        Rigidbody2D rb = ball.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;   // la gravità cambia di continuo: non deve addormentarsi
        rb.sharedMaterial = BallMaterial();

        SetField(level, "ball", rb);
        return SavePrefab(root, LevelDir + "/Maze/Level_Maze.prefab");
    }

    static PhysicsMaterial2D BallMaterial()
    {
        string path = LevelDir + "/Maze/Ball.physicsMaterial2D";
        EnsureFolder(LevelDir + "/Maze");
        PhysicsMaterial2D mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
        if (mat == null)
        {
            mat = new PhysicsMaterial2D("Ball");
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.friction = 0.2f;
        mat.bounciness = 0.15f;
        EditorUtility.SetDirty(mat);
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

    static void Wall(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        Transform t = Visual(parent, name, square, Neon, pos, size, 5);
        t.gameObject.AddComponent<BoxCollider2D>();   // dimensione 1, scalata con il muro
    }

    // ---------- Livello 3: pupazzo di neve ----------

    const string SnowmanEggId = "snowman_bunny";

    static GameObject BuildSnowmanLevel()
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

        Slot(snowman, "Slot_EyeL", "eye", new Vector2(-0.26f, 1.4f));
        Slot(snowman, "Slot_EyeR", "eye", new Vector2(0.26f, 1.4f));
        Slot(snowman, "Slot_Nose", "carrot", new Vector2(0.18f, 1.15f));
        Slot(snowman, "Slot_Mouth", "mouth", new Vector2(0, 0.83f));

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
        return SavePrefab(root, LevelDir + "/Snowman/Level_Snowman.prefab");
    }

    static void Slot(Transform parent, string name, string acceptedId, Vector2 pos)
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
