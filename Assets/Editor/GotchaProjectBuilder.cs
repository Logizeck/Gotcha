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

    // Da aumentare a ogni cambio di grafica o livelli: all'apertura del progetto il builder si rilancia da solo.
    const int BuilderVersion = 7;
    const string VersionFile = "ProjectSettings/GotchaBuilderVersion.txt";

    static readonly Color Paper = new Color(1f, 0.97f, 0.9f);
    static readonly Color InkColor = new Color(0.17f, 0.13f, 0.2f);

    static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    static readonly Dictionary<string, Vector2> centers = new Dictionary<string, Vector2>();

    static GotchaProjectBuilder()
    {
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(ScenePath) || InstalledVersion() < BuilderVersion) BuildAll();
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
                BuildKeyDoorLevel(keyPrefab, doorPrefab)),
            Define("Maze", "tilt_maze", "TILT MAZE",
                "No buttons here: the phone itself moves the ball. Reach the glowing star.",
                BuildMazeLevel()),
            Define("Snowman", "snowman", "SNOWMAN",
                "Give the snowman a happy face. Eyes and nose go on the head... and the smile? Draw it in the snow!",
                BuildSnowmanLevel(carrotPrefab, coalPrefab)),
            Define("Idle", "do_nothing", "BIG RED BUTTON",
                "A hungry cat never sleeps. Is there anything to eat around here?",
                BuildIdleLevel()),
            Define("Apple", "slice_apple", "SLICE IT!",
                "No knife? No problem. A quick finger is sharp enough.",
                BuildAppleLevel()),
        };

        LevelCatalog catalog = LoadOrCreate<LevelCatalog>(LevelDir + "/LevelCatalog.asset");
        catalog.levels = levels;
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();

        BuildScene(catalog);
        ConfigurePlayer();
        File.WriteAllText(VersionFile, BuilderVersion.ToString());
        Debug.Log("[Gotcha] Scena e livelli generati. Apri " + ScenePath + " e premi Play.");
    }

    static int InstalledVersion()
    {
        return File.Exists(VersionFile) && int.TryParse(File.ReadAllText(VersionFile).Trim(), out int v) ? v : 0;
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

        var musicGo = new GameObject("Music", typeof(AudioSource));
        MusicPlayer music = musicGo.AddComponent<MusicPlayer>();
        music.soundtrack = Soundtrack();
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

    const float WallT = 0.16f;        // spessore delle pareti (= diametro dei tubi al neon)
    const float WallMargin = 0.15f;   // alone attorno al tubo, nello sprite

    static GameObject BuildMazeLevel()
    {
        var root = new GameObject("Level_Maze");
        MazeLevel level = root.AddComponent<MazeLevel>();
        TiltGravity tilt = root.AddComponent<TiltGravity>();
        tilt.strength = 41.4f;   // risposta rapida ma un po' più dolce (-10% rispetto a 46)
        tilt.response = 0.5f;
        Art(root.transform, "Night", "maze_bg", -20, false);

        var maze = new GameObject("Maze").transform;
        maze.SetParent(root.transform, false);
        maze.localPosition = new Vector3(0, -0.4f, 0);

        // Recinto esterno: la sfera non può uscire dallo schermo
        Rect(maze, "Arena", 2.8f, 3.5f, null, "tube_cyan");
        // Anelli dall'esterno all'interno, ognuno con un varco su un lato diverso
        Rect(maze, "Ring1", 2.1f, 2.1f, "top", "tube_magenta");
        Rect(maze, "Ring2", 1.4f, 1.4f, "bottom", "tube_yellow");
        Rect(maze, "Ring3", 0.7f, 0.7f, "top", "tube_cyan");
        // Sbarramenti nei corridoi: obbligano a fare il giro lungo
        HWall(maze, "Block1", 1.4f, 2.1f, 0f, "tube_magenta");
        HWall(maze, "Block2", -1.4f, -0.7f, 0f, "tube_yellow");

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
        ball.transform.localScale = Vector3.one * 0.4f;
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
        chrome.captureRadius = 1.1f;
        // ombra morbida sotto la biglia (stesso layer: non compare nel riflesso)
        Transform shadow = Art(ball.transform, "Shadow", "ball_shadow", 8, false, new Vector2(0.15f, -0.22f));
        shadow.gameObject.layer = BallLayer;
        // scia luminosa dietro la biglia
        TrailRenderer trail = ball.AddComponent<TrailRenderer>();
        trail.time = 0.12f;
        trail.widthMultiplier = 0.3f;
        trail.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        trail.minVertexDistance = 0.02f;
        trail.sharedMaterial = StrokeMaterial();
        trail.sortingOrder = 9;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(new Color(0.75f, 0.95f, 1f), 0f), new GradientColorKey(new Color(0.24f, 0.9f, 1f), 1f) },
            new[] { new GradientAlphaKey(0.55f, 0f), new GradientAlphaKey(0f, 1f) });
        trail.colorGradient = gradient;

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
        mat.friction = 0.02f;
        mat.bounciness = 0.3f;
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
        if (mat.HasProperty("_Reflectivity")) mat.SetFloat("_Reflectivity", 0.8f);
        if (mat.HasProperty("_Radius")) mat.SetFloat("_Radius", 0.463f);   // raggio 0.5 su uno sprite di 1.08 unità
        EditorUtility.SetDirty(mat);
        return mat;
    }

    /// <summary>Colonna sonora: importata compressa (Vorbis) per non appesantire l'APK.</summary>
    static AudioClip Soundtrack()
    {
        const string path = "Assets/Audio/Soundtrack.wav";
        var importer = AssetImporter.GetAtPath(path) as AudioImporter;
        if (importer == null) { Debug.LogWarning("[Gotcha] Colonna sonora non trovata in " + path); return null; }
        AudioImporterSampleSettings settings = importer.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.CompressedInMemory;
        settings.compressionFormat = AudioCompressionFormat.Vorbis;
        settings.quality = 0.5f;
        importer.defaultSampleSettings = settings;
        importer.loadInBackground = true;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
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

    /// <summary>Rettangolo di tubi (semi-larghezza hx, semi-altezza hy) con un varco opzionale al centro di un lato.</summary>
    static void Rect(Transform parent, string name, float hx, float hy, string gapSide, string tube)
    {
        const float gap = 0.35f;   // metà larghezza del varco
        var g = new GameObject(name).transform;
        g.SetParent(parent, false);

        if (gapSide == "top") { HWall(g, "TopL", -hx, -gap, hy, tube); HWall(g, "TopR", gap, hx, hy, tube); }
        else HWall(g, "Top", -hx, hx, hy, tube);
        if (gapSide == "bottom") { HWall(g, "BottomL", -hx, -gap, -hy, tube); HWall(g, "BottomR", gap, hx, -hy, tube); }
        else HWall(g, "Bottom", -hx, hx, -hy, tube);
        VWall(g, "Left", -hx, -hy, hy, tube);
        VWall(g, "Right", hx, -hy, hy, tube);
    }

    static void HWall(Transform parent, string name, float x1, float x2, float y, string tube)
    {
        Wall(parent, name, new Vector2((x1 + x2) / 2f, y), new Vector2(x2 - x1 + WallT, WallT), tube);
    }

    static void VWall(Transform parent, string name, float x, float y1, float y2, string tube)
    {
        Wall(parent, name, new Vector2(x, (y1 + y2) / 2f), new Vector2(WallT, y2 - y1 + WallT), tube);
    }

    /// <summary>Tubo al neon: sprite 9-slice allungato (estremità tonde), collider a capsula grande quanto il tubo.</summary>
    static void Wall(Transform parent, string name, Vector2 pos, Vector2 core, string tube)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprites[tube];
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = core + Vector2.one * (2f * WallMargin);
        sr.sortingOrder = 5;
        CapsuleCollider2D col = go.AddComponent<CapsuleCollider2D>();   // tondo anche per la fisica: la biglia scivola sugli spigoli
        col.size = core;
        col.direction = core.x >= core.y ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
    }

    // ---------- Livello 3: pupazzo di neve ----------

    static GameObject BuildSnowmanLevel(GameObject carrotPrefab, GameObject coalPrefab)
    {
        var root = new GameObject("Level_Snowman");
        SnowmanLevel level = root.AddComponent<SnowmanLevel>();
        Art(root.transform, "Landscape", "snow_bg", -20, false);

        // Pupazzo in parti, ognuna col pivot sul suo punto d'appoggio: si scioglie dalla testa in giù
        var snowman = new GameObject("Snowman").transform;
        snowman.SetParent(root.transform, false);
        snowman.localPosition = new Vector3(0, -3.0f, 0);
        Transform basePart = Part(snowman, "Base", 0f);
        Transform middlePart = Part(snowman, "Middle", DoodleSprites.SnowMiddlePivot);
        Transform armsPart = Part(snowman, "Arms", 2.85f);
        Transform headPart = Part(snowman, "Head", DoodleSprites.SnowHeadPivot);
        Transform hatPart = Part(snowman, "Hat", DoodleSprites.SnowHatPivot);
        var bodies = new List<SpriteRenderer>
        {
            Art(basePart, "Art", "sm_base", 2, true, Vector2.zero).GetComponent<SpriteRenderer>(),
            Art(middlePart, "Art", "sm_middle", 3, true, new Vector2(0, -DoodleSprites.SnowMiddlePivot)).GetComponent<SpriteRenderer>(),
            Art(headPart, "Art", "sm_head", 4, true, new Vector2(0, -DoodleSprites.SnowHeadPivot)).GetComponent<SpriteRenderer>(),
        };
        Art(armsPart, "Art", "sm_arms", 1, true, new Vector2(0, -2.85f));
        Art(hatPart, "Art", "sm_hat", 7, true, new Vector2(0, -DoodleSprites.SnowHatPivot));
        float headLocal = DoodleSprites.SnowHeadCenterY - DoodleSprites.SnowHeadPivot;   // centro della testa nel pivot della testa
        float headX = DoodleSprites.SnowHeadCenterX;
        SpriteRenderer blush = Art(headPart, "Blush", "blush", 5, true, new Vector2(headX - 0.06f, headLocal - 0.08f)).GetComponent<SpriteRenderer>();

        // Bocca: si disegna col dito sulla testa e resta come un solco nella neve
        var pen = new GameObject("MouthPen");
        pen.transform.SetParent(headPart, false);
        pen.transform.localPosition = new Vector3(headX, headLocal, 0);
        CircleCollider2D face = pen.AddComponent<CircleCollider2D>();
        face.isTrigger = true;
        face.radius = DoodleSprites.SnowHeadRadius;
        StrokeDrawer drawer = pen.AddComponent<StrokeDrawer>();
        drawer.minLength = 0.2f;
        drawer.sortingOrder = 6;
        drawer.material = StrokeMaterial();
        drawer.layers = new[]
        {
            new StrokeDrawer.Layer { color = Color.white, width = 0.085f, offset = new Vector2(0, -0.022f) },               // bordo illuminato
            new StrokeDrawer.Layer { color = new Color(0.61f, 0.73f, 0.84f), width = 0.08f, offset = new Vector2(0, 0.006f) }, // pareti del solco
            new StrokeDrawer.Layer { color = new Color(0.42f, 0.55f, 0.71f), width = 0.04f, offset = new Vector2(0, 0.012f) }, // fondo in ombra
        };

        var smokeGo = new GameObject("Smoke");
        smokeGo.transform.SetParent(headPart, false);
        smokeGo.transform.localPosition = new Vector3(headX, headLocal + 0.2f, 0);
        SmokeEmitter smoke = smokeGo.AddComponent<SmokeEmitter>();
        smoke.puffSprite = sprites["puff"];
        smoke.spread = 0.55f;

        var puddle = new GameObject("Puddle").transform;
        puddle.SetParent(root.transform, false);
        puddle.localPosition = new Vector3(0, -2.95f, 0);
        Art(puddle, "Art", "puddle", 8);

        SpeechBubble bubble = Bubble(root.transform, new Vector2(0.95f, 2.45f));

        // Pezzi sparsi nella neve: nessuna posizione è suggerita sul pupazzo
        Instance(coalPrefab, root.transform, new Vector2(-2.0f, -3.95f)).name = "Eye1";
        Instance(coalPrefab, root.transform, new Vector2(-1.35f, -4.35f)).name = "Eye2";
        Instance(carrotPrefab, root.transform, new Vector2(1.55f, -4.1f)).name = "Carrot";

        SetField(level, "snowman", snowman);
        SetField(level, "basePart", basePart);
        SetField(level, "middlePart", middlePart);
        SetField(level, "armsPart", armsPart);
        SetField(level, "headPart", headPart);
        SetField(level, "hatPart", hatPart);
        SetArray(level, "bodyRenderers", bodies.ToArray());
        SetField(level, "blush", blush);
        SetField(level, "puddle", puddle);
        SetField(level, "bubble", bubble);
        SetField(level, "smoke", smoke);
        SetField(level, "mouthPen", drawer);
        return SavePrefab(root, LevelDir + "/Snowman/Level_Snowman.prefab");
    }

    static Transform Part(Transform parent, string name, float pivotY)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        t.localPosition = new Vector3(0, pivotY, 0);
        return t;
    }

    /// <summary>Fumetto con testo (TextMesh col font dei titoli).</summary>
    static SpeechBubble Bubble(Transform parent, Vector2 pos)
    {
        var bubbleGo = new GameObject("Bubble");
        bubbleGo.transform.SetParent(parent, false);
        bubbleGo.transform.localPosition = pos;
        Art(bubbleGo.transform, "Art", "bubble", 60);
        TextMesh text = Label(bubbleGo.transform, new Vector2(0, 0.07f), 61);
        SpeechBubble bubble = bubbleGo.AddComponent<SpeechBubble>();
        bubble.text = text;
        bubble.maxTextWidth = 2.5f;
        return bubble;
    }

    static TextMesh Label(Transform parent, Vector2 pos, int order, string value = "", float characterSize = 0.05f)
    {
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(parent, false);
        textGo.transform.localPosition = new Vector3(pos.x, pos.y, -0.01f);
        TextMesh text = textGo.AddComponent<TextMesh>();
        Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/LuckiestGuy-Regular.ttf");
        if (font != null)
        {
            text.font = font;
            textGo.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }
        text.text = value;
        text.fontSize = 80;
        text.characterSize = characterSize;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = InkColor;
        textGo.GetComponent<MeshRenderer>().sortingOrder = order;
        return text;
    }

    // ---------- Livello 4: non fare niente ----------

    static GameObject BuildIdleLevel()
    {
        var root = new GameObject("Level_Idle");
        IdleLevel level = root.AddComponent<IdleLevel>();
        Art(root.transform, "Room", "night_bg", -20, false);

        IdleTimer timer = root.AddComponent<IdleTimer>();
        timer.seconds = 5f;
        timer.resetOnMotion = true;
        timer.motionThreshold = 1.2f;

        var cat = new GameObject("Cat").transform;
        cat.SetParent(root.transform, false);
        cat.localPosition = new Vector3(0, -1.5f, 0);
        cat.localScale = Vector3.one * 1.35f;
        Transform catArt = Art(cat, "Art", "cat", 2);
        var breathe = catArt.gameObject.AddComponent<Pulse>();   // respira piano
        breathe.amount = 0.025f;
        breathe.speed = 2.2f;
        GameObject eyes = Art(cat, "EyesOpen", "cat_eyes", 3, false).gameObject;
        GameObject alert = Art(cat, "Alert", "alert", 4, true, new Vector2(-0.7f, 1.05f)).gameObject;

        var zzzGo = new GameObject("Zzz");
        zzzGo.transform.SetParent(cat, false);
        zzzGo.transform.localPosition = new Vector3(-0.35f, 0.75f, 0);
        SmokeEmitter zzz = zzzGo.AddComponent<SmokeEmitter>();
        zzz.puffSprite = sprites["zzz"];
        zzz.puffsPerSecond = 1.3f;
        zzz.life = 2.2f;
        zzz.riseSpeed = 0.45f;
        zzz.startScale = 0.7f;
        zzz.spread = 0.1f;
        zzz.spinRange = 12f;
        zzz.growth = 0.9f;
        zzz.sideDrift = 0.15f;
        zzz.tint = Color.white;

        var heartsGo = new GameObject("Hearts");
        heartsGo.transform.SetParent(cat, false);
        heartsGo.transform.localPosition = new Vector3(-0.4f, 0.6f, 0);
        SmokeEmitter hearts = heartsGo.AddComponent<SmokeEmitter>();
        hearts.puffSprite = sprites["heart"];
        hearts.puffsPerSecond = 5f;
        hearts.life = 1.2f;
        hearts.riseSpeed = 0.9f;
        hearts.startScale = 0.8f;
        hearts.spread = 0.5f;
        hearts.spinRange = 30f;
        hearts.growth = 0.4f;
        hearts.tint = Color.white;

        // Il pesce del quadro: va dato al gatto
        var fishGo = new GameObject("Fish");
        fishGo.transform.SetParent(root.transform, false);
        fishGo.transform.localPosition = new Vector3(-1.6f, 2.25f, 0);
        fishGo.AddComponent<CircleCollider2D>().radius = 0.35f;
        Draggable2D fish = fishGo.AddComponent<Draggable2D>();
        fish.itemId = "fish";
        fish.returnOnMiss = false;
        Art(fishGo.transform, "Art", "fish", 5);

        SpeechBubble meow = Bubble(root.transform, new Vector2(-0.1f, 0.3f));
        meow.transform.localScale = Vector3.one * 0.55f;

        // Il pulsantone: un'esca, toccarlo sveglia il gatto
        var button = new GameObject("BigButton").transform;
        button.SetParent(root.transform, false);
        button.localPosition = new Vector3(0, -3.75f, 0);
        Art(button, "Art", "big_button", 6);
        Label(root.transform, new Vector2(0, -4.45f), 7, "PRESS ME", 0.045f).color = new Color(1f, 0.93f, 0.75f);

        SetField(level, "cat", cat);
        SetField(level, "fish", fish);
        SetField(level, "meow", meow);
        SetField(level, "timer", timer);
        SetField(level, "eyesOpen", eyes);
        SetField(level, "alert", alert);
        SetField(level, "zzz", zzz);
        SetField(level, "hearts", hearts);
        SetField(level, "button", button);
        return SavePrefab(root, LevelDir + "/Idle/Level_Idle.prefab");
    }

    // ---------- Livello 5: taglia la mela ----------

    static GameObject BuildAppleLevel()
    {
        var root = new GameObject("Level_Apple");
        AppleLevel level = root.AddComponent<AppleLevel>();
        Art(root.transform, "Kitchen", "kitchen_bg", -20, false);

        var board = new GameObject("Board");
        board.transform.SetParent(root.transform, false);
        board.transform.localPosition = new Vector3(0, -1.85f, 0);
        Art(board.transform, "Art", "board", 1);
        board.AddComponent<BoxCollider2D>().size = new Vector2(8f, 0.56f);   // le metà si appoggiano qui
        foreach (float x in new[] { -3.3f, 3.3f })                          // pareti invisibili ai lati
        {
            var wall = new GameObject("Wall");
            wall.transform.SetParent(root.transform, false);
            wall.transform.localPosition = new Vector3(x, 0, 0);
            wall.AddComponent<BoxCollider2D>().size = new Vector2(0.4f, 12f);
        }

        // Mela intera: il cerchio da tagliare è centrato sul frutto
        var apple = new GameObject("Apple");
        apple.transform.SetParent(root.transform, false);
        apple.transform.localPosition = new Vector3(0, -0.85f, 0);
        var whole = new GameObject("Whole");
        whole.transform.SetParent(apple.transform, false);
        Art(whole.transform, "Art", "apple", 5);
        SwipeCutter cutter = apple.AddComponent<SwipeCutter>();
        cutter.radius = DoodleSprites.AppleRadius - 0.03f;
        cutter.minChord = 1.1f;
        cutter.trailMaterial = StrokeMaterial();

        // Le due metà (nascoste finché non si taglia)
        var halves = new GameObject("Halves").transform;
        halves.SetParent(apple.transform, false);
        Rigidbody2D left = Half(halves, "LeftHalf", "apple_left", -1);
        Rigidbody2D right = Half(halves, "RightHalf", "apple_right", 1);

        var juiceGo = new GameObject("Juice");
        juiceGo.transform.SetParent(apple.transform, false);
        SmokeEmitter juice = juiceGo.AddComponent<SmokeEmitter>();
        juice.puffSprite = sprites["juice"];
        juice.puffsPerSecond = 30f;
        juice.life = 0.7f;
        juice.riseSpeed = -1.4f;   // le gocce cadono
        juice.sideDrift = 1.6f;
        juice.spread = 0.3f;
        juice.startScale = 0.9f;
        juice.growth = -0.4f;
        juice.spinRange = 0f;
        juice.tint = Color.white;
        juice.sortingOrder = 45;

        SetField(level, "cutter", cutter);
        SetField(level, "wholeApple", whole);
        SetField(level, "halves", halves);
        SetField(level, "leftHalf", left);
        SetField(level, "rightHalf", right);
        SetField(level, "juice", juice);
        return SavePrefab(root, LevelDir + "/Apple/Level_Apple.prefab");
    }

    static Rigidbody2D Half(Transform parent, string name, string sprite, int side)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Art(go.transform, "Art", sprite, 5);
        CircleCollider2D c = go.AddComponent<CircleCollider2D>();
        c.offset = new Vector2(side * 0.3f, -0.05f);
        c.radius = 0.36f;
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        return rb;
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

    static void SetArray(Object target, string field, Object[] values)
    {
        var so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null || !p.isArray) { Debug.LogError("[Gotcha] Array non trovato: " + target.GetType().Name + "." + field); return; }
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
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
