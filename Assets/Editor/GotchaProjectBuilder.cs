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
    const int BuilderVersion = 20;
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
            Define("Idle", "do_nothing", "BIG RED BUTTON",
                "A hungry cat never sleeps. Is there anything to eat around here?",
                BuildIdleLevel()),
            Define("Apple", "slice_apple", "SLICE IT!",
                "No knife? No problem. A quick finger is sharp enough.",
                BuildAppleLevel()),
            Define("Morse", "beep_code", "BEEP BEEP",
                "Listen carefully. Short and long... Tap the radio to hear it again.",
                BuildMorseLevel()),
            Define("Bomb", "defuse", "TICK TOCK",
                "Follow the wires. Only one of them really goes to the timer.",
                BuildBombLevel()),
            Define("Volume", "shush", "SHHH!",
                "That music is way too loud for a cat. Maybe your phone can do something about it.",
                BuildVolumeLevel()),
            Define("Hold", "hold_both", "HOLD ON",
                "One finger is not enough. Keep both pressed until the timer runs out.",
                BuildHoldLevel()),
            Define("Wasp", "angry_wasp", "BUZZ OFF",
                "Bare hands won't do. Is there something in the grass you could swat it with?",
                BuildWaspLevel()),
            Define("ArtTheft", "art_theft", "ART THEFT",
                "Locked again? Some paintings are more realistic than they look.",
                BuildKeyPaintingLevel(keyPrefab, doorPrefab)),
            Define("Snowman", "snowman", "SNOWMAN",
                "Give the snowman a happy face. Eyes and nose go on the head... and the smile? Draw it in the snow!",
                BuildSnowmanLevel(carrotPrefab, coalPrefab), "snowman_butt"),
            Define("Defend", "defend_cat", "BODYGUARD",
                "Draw walls with your finger. The wasp hates walls.",
                BuildDefendLevel()),
            Define("Lamps", "you_won", "LIGHTS ON",
                "Red ink under red light is hard to read... what if both lights were on?",
                BuildLampsLevel()),
            Define("Cookie", "cookie_jar", "COOKIE JAR",
                "That cookie won't come out by itself. Maybe turn the world upside down?",
                BuildCookieLevel()),
            Define("Birthday", "birthday", "MAKE A WISH",
                "Candles don't like wind. Your phone has a microphone...",
                BuildBirthdayLevel()),
            Define("TrapMaze", "trap_maze", "TRAP MAZE",
                "Same old maze? Mind the holes... and the lasers keep a rhythm.",
                BuildTrapMazeLevel()),
            Define("DarkMaze", "dark_maze", "IN THE DARK",
                "Who turned off the lights? Every locked gate has a key somewhere.",
                BuildDarkMazeLevel()),
            Define("Safe", "hidden_safe", "HIDDEN SAFE",
                "That painting is stuck to the wall. Maybe a good shake would help?",
                BuildSafeLevel(keyPrefab, doorPrefab)),
            Define("Bedtime", "bedtime", "BEDTIME",
                "A sleepy cat needs darkness, silence... and nobody poking it.",
                BuildBedtimeLevel()),
            Define("Rocket", "lift_off", "LIFT OFF",
                "Rockets need fuel. And launch buttons are always protected.",
                BuildRocketLevel()),
            Define("Battery", "low_battery", "LOW BATTERY",
                "Robots run on electricity. So does your phone...",
                BuildBatteryLevel(), "mecha_robot"),
            Define("Soup", "hot_soup", "HOT SOUP",
                "Careful, it's boiling! How do you cool soup down?",
                BuildSoupLevel(), "hot_soup"),
            Define("FrozenKey", "frozen_key", "FROZEN KEY",
                "Ice melts with a little friction. And that key looks huge...",
                BuildFrozenKeyLevel(keyPrefab, doorPrefab)),
            Define("Campfire", "campfire", "CAMPFIRE",
                "Rub the stick to make a spark, then blow... gently!",
                BuildCampfireLevel()),
            Define("Runaway", "alarm_clock", "WAKE-UP CALL",
                "The OFF button ran away! Draw fences to corner it, then press it.",
                BuildRunawayLevel()),
            Define("Plant", "thirsty_plant", "THIRSTY PLANT",
                "Pull the drop back, aim... and let go!",
                BuildPlantLevel()),
            Define("Shell", "shell_game", "SHELL GAME",
                "Keep your eyes on the box with the mouse.",
                BuildShellLevel()),
            Define("SkyWire", "sky_wire", "SKY WIRE",
                "Read the note. Then look out of the window.",
                BuildSkyWireLevel()),
            Define("Sunny", "sunny_day", "SUNNY DAY",
                "Snowmen hate the sun. Could a cloud help?",
                BuildSunnyLevel(), "sun_melt"),
            Define("Rope", "cut_rope", "CUT THE ROPE",
                "Cut one rope, wait for the swing, then cut the other.",
                BuildRopeLevel()),
            Define("Pizza", "pizza_party", "PIZZA PARTY",
                "Four friends, four slices. Cut right through the middle!",
                BuildPizzaLevel()),
            Define("PizzaChef", "pizza_chef", "PIZZA CHEF",
                "Grandma never wrote recipes down. She just... drew them.",
                BuildPizzaChefLevel(), "spicy_pizza"),
            Define("Balloon", "balloon", "BLOW IT UP",
                "Fill the dotted line. Not a puff more!",
                BuildBalloonLevel()),
            Define("CatEyes", "cat_eyes", "CAT EYES",
                "The cat's eyes follow your phone. Stare at the mouse!",
                BuildCatEyesLevel()),
            Define("Pancake", "pancake", "PANCAKE FLIP",
                "A quick flick of the wrist... not too gentle, not too wild.",
                BuildPancakeLevel()),
            Define("WaspMaze", "sting_run", "STING RUN",
                "Tilt to the star. The wasp can't fly through walls... use them.",
                BuildWaspMazeLevel()),
            Define("LampSafe", "secret_code", "SECRET CODE",
                "Red light hides red ink. Blue light hides blue ink. Remember!",
                BuildLampSafeLevel()),
            Define("AppleCatch", "apple_catch", "APPLE CATCH",
                "Shake the tree, tilt the basket. Five apples for a pie.",
                BuildAppleCatchLevel()),
            Define("MarbleRun", "marble_run", "MARBLE RUN",
                "Draw up to three ramps, then press GO. Over the wall, into the cup!",
                BuildMarbleRunLevel()),
            Define("Copycat", "copycat", "COPYCAT",
                "Watch the cat, listen, then play it back.",
                BuildCopycatLevel()),
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

    static LevelDefinition Define(string folder, string id, string title, string hint, GameObject prefab, string easterEgg = "")
    {
        LevelDefinition def = LoadOrCreate<LevelDefinition>(LevelDir + "/" + folder + "/" + folder + ".asset");
        def.levelId = id;
        def.title = title;
        def.hint = hint;
        def.prefab = prefab;
        def.easterEggId = easterEgg;
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

        ClipRecorder recorder = new GameObject("ClipRecorder").AddComponent<ClipRecorder>();
        recorder.watermark = sprites["app_icon"].texture;   // il logo nell'angolo dei video condivisi

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
        SetField(ui, "heartSprite", sprites["heart"]);

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

    static GameObject BuildMazeLevel()
    {
        var root = new GameObject("Level_Maze");
        MazeLevel level = root.AddComponent<MazeLevel>();
        TiltGravity tilt = root.AddComponent<TiltGravity>();
        tilt.strength = 37.3f;   // sensibilità ridotta due volte del 10% (46 → 41.4 → 37.3)
        tilt.response = 0.5f;
        Art(root.transform, "Night", "maze_bg", -20, false);

        var maze = new GameObject("Maze").transform;
        maze.SetParent(root.transform, false);
        maze.localPosition = new Vector3(0, -0.4f, 0);

        // Recinto esterno: la sfera non può uscire dallo schermo
        // Grafica: tutte le pareti in un unico disegno al neon (curve e tratti dritti identici)
        Art(maze, "Neon", "maze_neon", 5, false);
        // Fisica: pareti invisibili che ricalcano esattamente il disegno
        for (int i = 0; i < DoodleSprites.MazeRings.Length; i++)
        {
            DoodleSprites.MazeRing ring = DoodleSprites.MazeRings[i];
            Rect(maze, "Ring" + i, ring.hx, ring.hy, ring.gap > 0 ? "top" : ring.gap < 0 ? "bottom" : null);
        }
        for (int i = 0; i < DoodleSprites.MazeBlocks.Length; i++)
        {
            DoodleSprites.MazeBlock b = DoodleSprites.MazeBlocks[i];
            HWall(maze, "Block" + i, b.x1, b.x2, b.y);
        }

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
        Rigidbody2D rb = MazeBall(maze, new Vector2(-2.35f, -3.0f));

        SetField(level, "ball", rb);
        return SavePrefab(root, LevelDir + "/Maze/Level_Maze.prefab");
    }

    /// <summary>Sfera cromata dei labirinti (riflessi, ombra, scia), figlia del labirinto.</summary>
    static Rigidbody2D MazeBall(Transform maze, Vector2 pos)
    {
        var ball = new GameObject("Ball");
        ball.tag = "Player";
        ball.layer = BallLayer;
        ball.transform.SetParent(maze, false);
        ball.transform.localPosition = pos;
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
        return rb;
    }

    /// <summary>Biscotto: attrito vero (rotola invece di scivolare) e quasi nessun rimbalzo.</summary>
    static PhysicsMaterial2D CookiePhysics()
    {
        string path = LevelDir + "/Cookie/Cookie.physicsMaterial2D";
        EnsureFolder(LevelDir + "/Cookie");
        PhysicsMaterial2D mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
        if (mat == null)
        {
            mat = new PhysicsMaterial2D("Cookie");
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.friction = 0.6f;
        mat.bounciness = 0.12f;
        EditorUtility.SetDirty(mat);
        return mat;
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
        if (mat.HasProperty("_Reflectivity")) mat.SetFloat("_Reflectivity", 1.3f);
        if (mat.HasProperty("_Studio")) mat.SetFloat("_Studio", 0.62f);
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

    /// <summary>Pareti fisiche di un anello: tratti dritti a capsula e angoli a quarto di cerchio, varco opzionale.</summary>
    static void Rect(Transform parent, string name, float hx, float hy, string gapSide)
    {
        float gap = DoodleSprites.MazeGapHalf, r = DoodleSprites.MazeCornerRadius;
        var g = new GameObject(name).transform;
        g.SetParent(parent, false);

        if (gapSide == "top") { HWall(g, "TopL", -hx + r, -gap, hy); HWall(g, "TopR", gap, hx - r, hy); }
        else HWall(g, "Top", -hx + r, hx - r, hy);
        if (gapSide == "bottom") { HWall(g, "BottomL", -hx + r, -gap, -hy); HWall(g, "BottomR", gap, hx - r, -hy); }
        else HWall(g, "Bottom", -hx + r, hx - r, -hy);
        VWall(g, "Left", -hx, -hy + r, hy - r);
        VWall(g, "Right", hx, -hy + r, hy - r);

        Corner(g, "CornerTR", new Vector2(hx - r, hy - r), 0f);
        Corner(g, "CornerTL", new Vector2(-hx + r, hy - r), 90f);
        Corner(g, "CornerBL", new Vector2(-hx + r, -hy + r), 180f);
        Corner(g, "CornerBR", new Vector2(hx - r, -hy + r), 270f);
    }

    /// <summary>Angolo fisico: arco di raggio MazeCornerRadius con bordo arrotondato (spessore del tubo).</summary>
    static void Corner(Transform parent, string name, Vector2 center, float angle)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = center;
        go.transform.localRotation = Quaternion.Euler(0, 0, angle);
        EdgeCollider2D edge = go.AddComponent<EdgeCollider2D>();
        var pts = new Vector2[9];
        for (int i = 0; i < pts.Length; i++)
        {
            float a = i / (float)(pts.Length - 1) * Mathf.PI * 0.5f;
            pts[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * DoodleSprites.MazeCornerRadius;
        }
        edge.points = pts;
        edge.edgeRadius = WallT * 0.5f;
    }

    static void HWall(Transform parent, string name, float x1, float x2, float y)
    {
        Wall(parent, name, new Vector2((x1 + x2) / 2f, y), new Vector2(x2 - x1 + WallT, WallT));
    }

    static void VWall(Transform parent, string name, float x, float y1, float y2)
    {
        Wall(parent, name, new Vector2(x, (y1 + y2) / 2f), new Vector2(WallT, y2 - y1 + WallT));
    }

    /// <summary>Tratto dritto fisico: collider a capsula grande quanto il tubo disegnato.</summary>
    static void Wall(Transform parent, string name, Vector2 pos, Vector2 core)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        CapsuleCollider2D col = go.AddComponent<CapsuleCollider2D>();
        col.size = core;
        col.direction = core.x >= core.y ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
    }

    // ---------- Livello 3: pupazzo di neve ----------

    /// <summary>
    /// Scioglimento del pupazzo (livelli 3 e 29): la neve intera con lo shader Gotcha/SnowMelt (si affloscia, cola,
    /// si allarga), gocce, vapore e pozzanghera ai piedi. Le parti di neve separate si nascondono quando parte.
    /// </summary>
    static SnowmanMelter AddMelter(Transform snowman, SpriteRenderer[] parts)
    {
        SnowmanMelter melter = snowman.gameObject.AddComponent<SnowmanMelter>();
        melter.snowman = snowman;
        melter.parts = parts;
        melter.bodyXMin = DoodleSprites.SnowBodyXMin;
        melter.bodyXMax = DoodleSprites.SnowBodyXMax;
        melter.bodyYMin = DoodleSprites.SnowBodyYMin;
        melter.bodyYMax = DoodleSprites.SnowBodyYMax;
        melter.topY = DoodleSprites.SnowHeadCenterY + DoodleSprites.SnowHeadRadius;
        melter.seed = 1.7f;

        SpriteRenderer body = Art(snowman, "MeltingSnow", "sm_body", 3, true).GetComponent<SpriteRenderer>();
        body.sharedMaterial = SnowMeltMaterial();
        body.gameObject.SetActive(false);
        melter.body = body;

        var puddle = new GameObject("Puddle").transform;
        puddle.SetParent(snowman, false);
        puddle.localPosition = new Vector3(0, 0.05f, 0);
        puddle.localScale = Vector3.zero;
        Art(puddle, "Art", "puddle", 0, true);
        melter.puddle = puddle;

        var dripsGo = new GameObject("Drips");
        dripsGo.transform.SetParent(snowman, false);
        SmokeEmitter drips = dripsGo.AddComponent<SmokeEmitter>();
        drips.puffSprite = sprites["sweat"];
        drips.puffsPerSecond = 14f;
        drips.life = 0.7f;
        drips.riseSpeed = -2.6f;   // gocce che cadono
        drips.spread = 1.0f;
        drips.sideDrift = 0.25f;
        drips.startScale = 1.2f;
        drips.growth = -0.4f;
        drips.spinRange = 0f;
        drips.tint = new Color(0.75f, 0.9f, 1f, 0.95f);
        drips.sortingOrder = 9;
        melter.drips = drips;

        var steamGo = new GameObject("Steam");
        steamGo.transform.SetParent(snowman, false);
        SmokeEmitter steam = steamGo.AddComponent<SmokeEmitter>();
        steam.puffSprite = sprites["puff"];
        steam.puffsPerSecond = 6f;
        steam.life = 1.5f;
        steam.riseSpeed = 0.9f;
        steam.spread = 0.6f;
        steam.startScale = 0.45f;
        steam.growth = 1.8f;
        steam.sideDrift = 0.35f;
        steam.tint = new Color(1f, 1f, 1f, 0.5f);
        steam.sortingOrder = 8;
        melter.steam = steam;
        return melter;
    }

    /// <summary>La "tragedia" del pupazzo (urlo, occhi, cappello, braccia, scioglimento), uguale nei livelli 11 e 29.</summary>
    static SnowmanMeltdown AddMeltdown(GameObject root, Transform snowman, SnowmanMelter melter, Transform hat, Transform armLeft,
        Transform armRight, Transform scarf, SpriteRenderer mouthO, SpriteRenderer blush, SpeechBubble bubble, SmokeEmitter smoke)
    {
        SnowmanMeltdown m = root.AddComponent<SnowmanMeltdown>();
        m.snowman = snowman;
        m.melter = melter;
        m.hat = hat;
        m.armLeft = armLeft;
        m.armRight = armRight;
        m.scarf = scarf;
        m.mouthO = mouthO;
        m.blush = blush;
        m.bubble = bubble;
        m.smoke = smoke;
        AudioSource scream = root.AddComponent<AudioSource>();
        scream.playOnAwake = false;
        m.screamSource = scream;
        m.screamClip = ScreamClip();   // null: il gioco lo sintetizza
        return m;
    }

    static Material SnowMeltMaterial()
    {
        string path = MaterialDir + "/SnowMelt.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Gotcha/SnowMelt");
        if (shader == null) { Debug.LogError("[Gotcha] Shader Gotcha/SnowMelt non trovato."); shader = Shader.Find("Sprites/Default"); }
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.shader = shader;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    /// <summary>Urlo vero se c'è Assets/Audio/Scream.wav, altrimenti null (il livello lo sintetizza).</summary>
    static AudioClip ScreamClip()
    {
        return AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Scream.wav");
    }

    /// <summary>Pupazzo di neve in parti (base, busto, braccia, testa, cappello), piedi in <paramref name="pos"/>.</summary>
    static Transform SnowmanFigure(Transform parent, Vector2 pos, out Transform basePart, out Transform armLeft,
        out Transform armRight, out Transform headPart, out Transform hatPart, out Transform scarf, out List<SpriteRenderer> bodies)
    {
        var snowman = new GameObject("Snowman").transform;
        snowman.SetParent(parent, false);
        snowman.localPosition = pos;
        basePart = Part(snowman, "Base", 0f);
        Transform middlePart = Part(snowman, "Middle", DoodleSprites.SnowMiddlePivot);
        armLeft = Part(snowman, "ArmLeft", DoodleSprites.SnowShoulderY);
        armRight = Part(snowman, "ArmRight", DoodleSprites.SnowShoulderY);
        armLeft.localPosition = new Vector3(-0.6f, DoodleSprites.SnowShoulderY, 0);   // pivot sulla spalla: cadono ruotando
        armRight.localPosition = new Vector3(0.5f, DoodleSprites.SnowShoulderY, 0);
        headPart = Part(snowman, "Head", DoodleSprites.SnowHeadPivot);
        hatPart = Part(snowman, "Hat", DoodleSprites.SnowHatPivot);
        bodies = new List<SpriteRenderer>
        {
            Art(basePart, "Art", "sm_base", 2, true, Vector2.zero).GetComponent<SpriteRenderer>(),
            Art(middlePart, "Art", "sm_middle", 3, true, new Vector2(0, -DoodleSprites.SnowMiddlePivot)).GetComponent<SpriteRenderer>(),
            Art(headPart, "Art", "sm_head", 4, true, new Vector2(0, -DoodleSprites.SnowHeadPivot)).GetComponent<SpriteRenderer>(),
        };
        Art(armLeft, "Art", "sm_arm_l", 1, true, new Vector2(0.6f, -DoodleSprites.SnowShoulderY));
        Art(armRight, "Art", "sm_arm_r", 1, true, new Vector2(-0.5f, -DoodleSprites.SnowShoulderY));
        Art(hatPart, "Art", "sm_hat", 7, true, new Vector2(0, -DoodleSprites.SnowHatPivot));
        scarf = Art(headPart, "Scarf", "sm_scarf", 5, true, new Vector2(0, -DoodleSprites.SnowHeadPivot));   // non è neve
        return snowman;
    }

    static GameObject BuildSnowmanLevel(GameObject carrotPrefab, GameObject coalPrefab)
    {
        var root = new GameObject("Level_Snowman");
        SnowmanLevel level = root.AddComponent<SnowmanLevel>();
        Art(root.transform, "Landscape", "snow_bg", -20, false);

        // Pupazzo in parti, ognuna col pivot sul suo punto d'appoggio
        Transform snowman = SnowmanFigure(root.transform, new Vector2(0, -3.0f), out Transform basePart, out Transform armLeft,
            out Transform armRight, out Transform headPart, out Transform hatPart, out Transform scarf, out List<SpriteRenderer> bodies);
        float headLocal = DoodleSprites.SnowHeadCenterY - DoodleSprites.SnowHeadPivot;   // centro della testa nel pivot della testa
        float headX = DoodleSprites.SnowHeadCenterX;
        Transform blushT = Art(headPart, "Blush", "blush", 5, true, new Vector2(headX - 0.06f, headLocal - 0.12f));
        blushT.localScale = Vector3.one * 1.6f;   // guance grandi per la faccia grande
        SpriteRenderer blush = blushT.GetComponent<SpriteRenderer>();

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
            new StrokeDrawer.Layer { color = Color.white, width = 0.11f, offset = new Vector2(0, -0.028f) },               // bordo illuminato
            new StrokeDrawer.Layer { color = new Color(0.61f, 0.73f, 0.84f), width = 0.105f, offset = new Vector2(0, 0.008f) }, // pareti del solco
            new StrokeDrawer.Layer { color = new Color(0.42f, 0.55f, 0.71f), width = 0.055f, offset = new Vector2(0, 0.015f) }, // fondo in ombra
        };

        var smokeGo = new GameObject("Smoke");
        smokeGo.transform.SetParent(headPart, false);
        smokeGo.transform.localPosition = new Vector3(headX, headLocal + 0.2f, 0);
        SmokeEmitter smoke = smokeGo.AddComponent<SmokeEmitter>();
        smoke.puffSprite = sprites["puff"];
        smoke.spread = 0.55f;

        // bocca stupita che prende il posto del solco quando parte l'Easter egg
        SpriteRenderer mouthO = Art(headPart, "MouthO", "mouth_o", 6, false, new Vector2(headX - 0.25f, headLocal - 0.45f)).GetComponent<SpriteRenderer>();
        mouthO.transform.localScale = Vector3.one * 1.3f;
        mouthO.gameObject.SetActive(false);
        SnowmanMelter melter = AddMelter(snowman, bodies.ToArray());

        SpeechBubble bubble = Bubble(root.transform, new Vector2(0.95f, 2.45f));

        // Pezzi sparsi nella neve: nessuna posizione è suggerita sul pupazzo
        // pezzi più grandi del normale: la faccia è enorme e buffa
        GameObject eye1 = Instance(coalPrefab, root.transform, new Vector2(-2.0f, -3.95f));
        GameObject eye2 = Instance(coalPrefab, root.transform, new Vector2(-1.3f, -4.35f));
        GameObject carrot = Instance(carrotPrefab, root.transform, new Vector2(1.5f, -4.1f));
        eye1.name = "Eye1"; eye2.name = "Eye2"; carrot.name = "Carrot";
        eye1.transform.localScale = eye2.transform.localScale = Vector3.one * 1.4f;
        carrot.transform.localScale = Vector3.one * 1.3f;

        SetField(level, "snowman", snowman);
        SetField(level, "basePart", basePart);
        SetField(level, "headPart", headPart);
        SetArray(level, "bodyRenderers", bodies.ToArray());
        SetField(level, "blush", blush);
        SetField(level, "bubble", bubble);
        SetField(level, "meltdown", AddMeltdown(root, snowman, melter, hatPart, armLeft, armRight, scarf, mouthO, blush, bubble, smoke));
        SetField(level, "mouthO", mouthO);
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
        meow.catVoice = true;   // il gatto miagola quando parla
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

    // ---------- Livello 6: ascolta i segnali ----------

    static GameObject BuildMorseLevel()
    {
        var root = new GameObject("Level_Morse");
        MorseLevel level = root.AddComponent<MorseLevel>();
        AudioSource source = root.AddComponent<AudioSource>();
        source.playOnAwake = false;
        Art(root.transform, "Room", "radio_bg", -20, false);

        var stage = new GameObject("Stage").transform;   // tutto ciò che trema quando sbagli
        stage.SetParent(root.transform, false);

        var radioGo = new GameObject("Radio");
        radioGo.transform.SetParent(stage, false);
        radioGo.transform.localPosition = new Vector3(0, 1.3f, 0);
        Transform radioArt = Art(radioGo.transform, "Art", "radio", 2);
        radioGo.AddComponent<BoxCollider2D>().size = new Vector2(2.7f, 1.9f);
        TapButton radio = radioGo.AddComponent<TapButton>();
        radio.id = "radio";
        radio.visual = radioArt;
        radio.pressedScaleY = 0.95f;
        GameObject lamp = Art(radioGo.transform, "Lamp", "radio_lamp", 3, false, new Vector2(0.45f, 0.72f)).gameObject;

        var keys = new TapButton[MorseLevel.Codes.Length];
        Vector2[] slots = { new Vector2(-1.75f, -1.55f), new Vector2(0f, -1.55f), new Vector2(1.75f, -1.55f), new Vector2(-0.9f, -2.95f), new Vector2(0.9f, -2.95f) };
        for (int i = 0; i < keys.Length; i++)
        {
            var key = new GameObject("Key " + MorseLevel.Codes[i]);
            key.transform.SetParent(stage, false);
            key.transform.localPosition = slots[i];
            Transform art = Art(key.transform, "Art", "morse_" + i, 4);
            key.AddComponent<BoxCollider2D>().size = new Vector2(1.5f, 1.0f);
            TapButton b = key.AddComponent<TapButton>();
            b.id = MorseLevel.Codes[i];
            b.visual = art;
            keys[i] = b;
        }

        SetField(level, "source", source);
        SetArray(level, "keys", keys);
        SetField(level, "radio", radio);
        SetField(level, "lamp", lamp);
        SetField(level, "shakeTarget", stage);
        return SavePrefab(root, LevelDir + "/Morse/Level_Morse.prefab");
    }

    // ---------- Livello 7: disinnesca la bomba ----------

    static GameObject BuildBombLevel()
    {
        var root = new GameObject("Level_Bomb");
        BombLevel level = root.AddComponent<BombLevel>();
        Art(root.transform, "Workshop", "workshop_bg", -20, false);

        var bomb = new GameObject("Bomb");
        bomb.transform.SetParent(root.transform, false);
        bomb.transform.localPosition = new Vector3(0, 1.45f, 0);
        Art(bomb.transform, "Dynamite", "dynamite", 2);
        var timer = new GameObject("Timer");
        timer.transform.SetParent(bomb.transform, false);
        timer.transform.localPosition = new Vector3(0, 1.35f, 0);
        Art(timer.transform, "Art", "timer_box", 5);
        TextMesh display = Label(timer.transform, new Vector2(0, 0.04f), 6, "10.0", 0.06f);
        display.color = new Color(1f, 0.2f, 0.2f);
        var terminal = new GameObject("Terminal").transform;
        terminal.SetParent(timer.transform, false);
        terminal.localPosition = new Vector3(-0.35f, -0.55f, 0);

        TapButton blue = BombButton(root.transform, "BlueButton", "blue_button", new Vector2(-1.3f, -3.6f), out Transform blueStart);
        TapButton red = BombButton(root.transform, "RedButton", "big_button", new Vector2(1.3f, -3.6f), out Transform redStart);

        var wireEnd = new GameObject("WireEnd").transform;
        wireEnd.SetParent(root.transform, false);
        Art(wireEnd, "Art", "wire_end", 5, false);

        var boom = new GameObject("Boom");
        boom.transform.SetParent(root.transform, false);
        boom.transform.localPosition = new Vector3(0, 1.6f, 0);
        Art(boom.transform, "Art", "boom", 70);
        Label(boom.transform, new Vector2(0, 0.05f), 71, "BOOM!", 0.09f);

        var smokeGo = new GameObject("Smoke");
        smokeGo.transform.SetParent(root.transform, false);
        smokeGo.transform.localPosition = new Vector3(0, 1.6f, 0);
        SmokeEmitter smoke = smokeGo.AddComponent<SmokeEmitter>();
        smoke.puffSprite = sprites["puff"];
        smoke.puffsPerSecond = 14f;
        smoke.spread = 1.2f;
        smoke.startScale = 1.2f;
        smoke.tint = new Color(0.45f, 0.45f, 0.5f, 0.95f);
        smoke.sortingOrder = 72;

        SpeechBubble bubble = Bubble(root.transform, new Vector2(-1.1f, 3.25f));
        bubble.transform.localScale = Vector3.one * 0.6f;

        SetField(level, "blueButton", blue);
        SetField(level, "redButton", red);
        SetField(level, "blueStart", blueStart);
        SetField(level, "redStart", redStart);
        SetField(level, "timerTerminal", terminal);
        SetField(level, "wireEnd", wireEnd);
        SetField(level, "display", display);
        SetField(level, "bomb", bomb);
        SetField(level, "boom", boom);
        SetField(level, "smoke", smoke);
        SetField(level, "bubble", bubble);
        SetField(level, "wireMaterial", StrokeMaterial());

        // Il tizio nel panico che guarda il giocatore, trema e suda
        var guy = new GameObject("Guy");
        guy.transform.SetParent(root.transform, false);
        guy.transform.localPosition = new Vector3(1.95f, 2.3f, 0);   // accanto al timer, ben visibile
        guy.transform.localScale = Vector3.one * 1.05f;
        Tremble tremble = guy.AddComponent<Tremble>();
        GameObject panic = Art(guy.transform, "Panic", "guy_panic", 8).gameObject;
        GameObject relieved = Art(guy.transform, "Relieved", "guy_relieved", 8).gameObject;
        relieved.SetActive(false);
        var sweatGo = new GameObject("Sweat");
        sweatGo.transform.SetParent(guy.transform, false);
        sweatGo.transform.localPosition = new Vector3(0, 0.6f, 0);
        SmokeEmitter sweat = sweatGo.AddComponent<SmokeEmitter>();
        sweat.puffSprite = sprites["sweat"];
        sweat.puffsPerSecond = 2f;
        sweat.life = 0.8f;
        sweat.riseSpeed = -0.9f;
        sweat.sideDrift = 0.7f;
        sweat.spread = 0.45f;
        sweat.startScale = 1f;
        sweat.growth = 0f;
        sweat.spinRange = 0f;
        sweat.tint = Color.white;
        sweat.sortingOrder = 9;
        SetField(level, "guyPanic", panic);
        SetField(level, "guyRelieved", relieved);
        SetField(level, "guyTremble", tremble);
        SetField(level, "sweat", sweat);
        return SavePrefab(root, LevelDir + "/Bomb/Level_Bomb.prefab");
    }

    // ---------- Livello 8: abbassa il volume ----------

    static GameObject BuildVolumeLevel()
    {
        var root = new GameObject("Level_Volume");
        VolumeLevel level = root.AddComponent<VolumeLevel>();
        AudioSource stereo = root.AddComponent<AudioSource>();
        stereo.playOnAwake = false;
        stereo.loop = true;
        stereo.clip = Soundtrack();
        Art(root.transform, "Room", "night_bg", -20, false);

        var hifi = new GameObject("HiFi").transform;
        hifi.SetParent(root.transform, false);
        hifi.localPosition = new Vector3(0, 0.75f, 0);
        Art(hifi, "Unit", "hifi", 4);
        Leds(hifi, 5);   // barre del volume del telefono, dal verde al rosso
        var speakers = new List<Pulse>();
        foreach (float x in new[] { -1.55f, 1.55f })
        {
            Transform sp = Art(hifi, "Speaker", "speaker", 4, false, new Vector2(x, 0.1f));
            Pulse pulse = sp.gameObject.AddComponent<Pulse>();
            pulse.speed = 14f;
            pulse.amount = 0.06f;
            speakers.Add(pulse);
        }
        var notesGo = new GameObject("Notes");
        notesGo.transform.SetParent(hifi, false);
        notesGo.transform.localPosition = new Vector3(0, 0.6f, 0);
        SmokeEmitter notes = notesGo.AddComponent<SmokeEmitter>();
        notes.puffSprite = sprites["note"];
        notes.puffsPerSecond = 5f;
        notes.life = 1.8f;
        notes.riseSpeed = 0.7f;
        notes.spread = 1.8f;
        notes.sideDrift = 0.4f;
        notes.startScale = 1f;
        notes.growth = 0.3f;
        notes.spinRange = 25f;
        notes.tint = Color.white;

        // Lo stesso gatto del livello 4, sveglio e arrabbiato
        var cat = new GameObject("Cat").transform;
        cat.SetParent(root.transform, false);
        cat.localPosition = new Vector3(0, -1.75f, 0);
        cat.localScale = Vector3.one * 1.35f;
        Transform catArt = Art(cat, "Art", "cat", 2);
        Pulse breathe = catArt.gameObject.AddComponent<Pulse>();
        breathe.amount = 0.025f;
        breathe.speed = 2.2f;
        var faces = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            faces[i] = Art(cat, "Face" + i, "cat_face_" + i, 3, false).gameObject;
            faces[i].SetActive(i == 0);
        }
        GameObject anger = Art(cat, "Anger", "anger", 4, true, new Vector2(-0.35f, 0.78f)).gameObject;

        var steamGo = new GameObject("Steam");
        steamGo.transform.SetParent(cat, false);
        steamGo.transform.localPosition = new Vector3(-0.72f, 0.85f, 0);
        SmokeEmitter steam = steamGo.AddComponent<SmokeEmitter>();
        steam.puffSprite = sprites["puff"];
        steam.puffsPerSecond = 6f;
        steam.life = 0.9f;
        steam.riseSpeed = 1.1f;
        steam.spread = 0.3f;
        steam.startScale = 0.35f;
        steam.tint = new Color(1f, 1f, 1f, 0.85f);

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

        SpeechBubble bubble = Bubble(root.transform, new Vector2(0.4f, -0.55f));
        bubble.catVoice = true;   // il gatto miagola quando parla
        bubble.transform.localScale = Vector3.one * 0.55f;

        SetField(level, "stereo", stereo);
        SetArray(level, "faces", faces);
        SetField(level, "anger", anger);
        SetField(level, "steam", steam);
        SetField(level, "notes", notes);
        SetField(level, "zzz", zzz);
        SetArray(level, "speakers", speakers.ToArray());
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/Volume/Level_Volume.prefab");
    }

    // ---------- Livello 9: tieni premuti entrambi ----------

    static GameObject BuildHoldLevel()
    {
        var root = new GameObject("Level_Hold");
        HoldLevel level = root.AddComponent<HoldLevel>();
        Art(root.transform, "Workshop", "workshop_bg", -20, false);

        var box = new GameObject("Timer").transform;
        box.SetParent(root.transform, false);
        box.localPosition = new Vector3(0, 1.5f, 0);
        box.localScale = Vector3.one * 1.4f;
        Art(box, "Art", "timer_box", 5);
        TextMesh display = Label(box, new Vector2(0, 0.04f), 6, "5.0", 0.06f);
        display.color = new Color(1f, 0.2f, 0.2f);

        Transform left = HoldButton(root.transform, "BlueButton", "blue_button", new Vector2(-1.5f, -3.3f), out Collider2D leftCol);
        Transform right = HoldButton(root.transform, "RedButton", "big_button", new Vector2(1.5f, -3.3f), out Collider2D rightCol);

        // cavi dai pulsanti ai due morsetti del timer
        var wires = new List<LineRenderer>();
        foreach (float side in new[] { -1f, 1f })
        {
            Vector3 a = new Vector3(side * 1.5f, -3.15f, 0), b = new Vector3(side * 0.49f, 0.8f, 0);
            Vector3 c = new Vector3(side * 2.4f, -1.0f, 0);
            var pts = new Vector3[24];
            for (int i = 0; i < pts.Length; i++)
            {
                float t = i / (float)(pts.Length - 1);
                pts[i] = (1 - t) * (1 - t) * a + 2 * (1 - t) * t * c + t * t * b;
            }
            StaticLine(root.transform, "WireOutline", pts, 0.15f, InkColor, 3);
            wires.Add(StaticLine(root.transform, "Wire", pts, 0.09f, new Color(0.75f, 0.62f, 0.15f), 4));
        }

        SetField(level, "leftButton", leftCol);
        SetField(level, "rightButton", rightCol);
        SetField(level, "leftVisual", left);
        SetField(level, "rightVisual", right);
        SetField(level, "display", display);
        SetArray(level, "wires", wires.ToArray());
        return SavePrefab(root, LevelDir + "/Hold/Level_Hold.prefab");
    }

    static Transform HoldButton(Transform parent, string name, string sprite, Vector2 pos, out Collider2D col)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        Transform art = Art(go.transform, "Art", sprite, 6);
        BoxCollider2D box = go.AddComponent<BoxCollider2D>();
        box.size = new Vector2(1.9f, 1.3f);
        col = box;
        return art;
    }

    static LineRenderer StaticLine(Transform parent, string name, Vector3[] pts, float width, Color color, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.widthMultiplier = width;
        lr.numCapVertices = 5;
        lr.numCornerVertices = 4;
        lr.sharedMaterial = StrokeMaterial();
        lr.startColor = lr.endColor = color;
        lr.sortingOrder = order;
        lr.positionCount = pts.Length;
        lr.SetPositions(pts);
        return lr;
    }

    // ---------- Livello 10: la vespa furiosa ----------

    static GameObject BuildWaspLevel()
    {
        var root = new GameObject("Level_Wasp");
        WaspLevel level = root.AddComponent<WaspLevel>();
        AudioSource buzz = root.AddComponent<AudioSource>();
        buzz.playOnAwake = false;
        Art(root.transform, "Garden", "garden_bg", -20, false);

        var wasp = new GameObject("Wasp").transform;
        wasp.SetParent(root.transform, false);
        wasp.localPosition = new Vector3(0.5f, 1.0f, 0);
        var alive = new GameObject("Alive").transform;
        alive.SetParent(wasp, false);
        SpriteRenderer body = Art(alive, "Body", "wasp", 10).GetComponent<SpriteRenderer>();
        foreach (var w in new[] { new { x = -0.02f, order = 9, rot = 15f }, new { x = 0.08f, order = 11, rot = -10f } })
        {
            Transform wing = Art(alive, "Wing", "wasp_wing", w.order, false, new Vector2(w.x, 0.12f));
            wing.localRotation = Quaternion.Euler(0, 0, w.rot);
            wing.gameObject.AddComponent<WingFlap>();
        }
        GameObject dead = Art(wasp, "Dead", "wasp_dead", 10).gameObject;
        dead.SetActive(false);

        // Paletta nell'erba: pivot sull'impugnatura, rete 1.9 più in là lungo "su"
        var swatter = new GameObject("Swatter").transform;
        swatter.SetParent(root.transform, false);
        swatter.localPosition = new Vector3(-1.6f, -3.5f, 0);
        swatter.localRotation = Quaternion.Euler(0, 0, -80f);
        Art(swatter, "Art", "swatter", 30, true, new Vector2(0, 1.9f));

        // Puntura: bagliore rosso ai bordi dello schermo + fumetto
        SpriteRenderer flash = Art(root.transform, "StingFlash", "sting_flash", 80, false).GetComponent<SpriteRenderer>();
        flash.transform.localScale = new Vector3(4.5f, 7.5f, 1f);
        flash.color = new Color(1f, 1f, 1f, 0f);
        SpeechBubble bubble = Bubble(root.transform, new Vector2(0.9f, 2.9f));
        bubble.transform.localScale = Vector3.one * 0.6f;

        SetField(level, "wasp", wasp);
        SetField(level, "alive", alive.gameObject);
        SetField(level, "dead", dead);
        SetField(level, "body", body);
        SetField(level, "buzz", buzz);
        SetField(level, "swatter", swatter);
        SetField(level, "stingFlash", flash);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/Wasp/Level_Wasp.prefab");
    }

    // ---------- Livello 11: la chiave nel quadro ----------

    static GameObject BuildKeyPaintingLevel(GameObject keyPrefab, GameObject doorPrefab)
    {
        var root = new GameObject("Level_ArtTheft");
        KeyDoorLevel level = root.AddComponent<KeyDoorLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);
        GameObject door = Instance(doorPrefab, root.transform, new Vector2(0.75f, -0.85f));
        door.name = "Door";
        // natura morta: la chiave è dipinta sul tavolo, tra la frutta e la candela
        Vector2 at = new Vector2(-1.45f, 2.35f);
        const float size = 1.05f;   // copre anche il quadretto disegnato nello sfondo
        var frame = new GameObject("Painting").transform;
        frame.SetParent(root.transform, false);
        frame.localPosition = at;
        frame.localScale = Vector3.one * size;
        SpriteRenderer painting = Art(frame, "Art", "still_life", 2).GetComponent<SpriteRenderer>();
        GameObject key = Instance(keyPrefab, root.transform, at + new Vector2(DoodleSprites.StillLifeKeyX, DoodleSprites.StillLifeKeyY) * size);
        key.transform.localScale = Vector3.one * DoodleSprites.StillLifeKeyScale * size;
        foreach (SpriteRenderer r in key.GetComponentsInChildren<SpriteRenderer>(true)) r.enabled = false;   // è solo dipinta... finché non la tocchi
        SmokeEmitter sparkle = Sparks(root.transform, "Sparkle", new Color(1f, 0.85f, 0.35f));
        SetField(level, "painting", painting);
        SetField(level, "paintingWithoutKey", sprites["still_life_empty"]);
        SetField(level, "sparkle", sparkle);
        return SavePrefab(root, LevelDir + "/ArtTheft/Level_ArtTheft.prefab");
    }

    static GameObject BuildDefendLevel()
    {
        var root = new GameObject("Level_Defend");
        DefendLevel level = root.AddComponent<DefendLevel>();
        AudioSource buzz = root.AddComponent<AudioSource>();
        buzz.playOnAwake = false;
        Art(root.transform, "Room", "night_bg", -20, false);

        var cat = new GameObject("Cat").transform;
        cat.SetParent(root.transform, false);
        cat.localPosition = new Vector3(0.3f, -2.75f, 0);
        cat.localScale = Vector3.one * 1.2f;
        Transform catArt = Art(cat, "Art", "cat", 2);
        Pulse breathe = catArt.gameObject.AddComponent<Pulse>();
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

        var wasp = new GameObject("Wasp").transform;
        wasp.SetParent(root.transform, false);
        wasp.localPosition = new Vector3(-0.9f, 2.6f, 0);
        wasp.localScale = Vector3.one;
        SpriteRenderer body = Art(wasp, "Body", "wasp_furious", 12).GetComponent<SpriteRenderer>();
        GameObject waspAnger = Art(wasp, "Anger", "anger", 14, false, new Vector2(-0.12f, 0.36f)).gameObject;
        foreach (var w in new[] { new { x = -0.02f, order = 11, rot = 15f }, new { x = 0.08f, order = 13, rot = -10f } })
        {
            Transform wing = Art(wasp, "Wing", "wasp_wing", w.order, false, new Vector2(w.x, 0.12f));
            wing.localRotation = Quaternion.Euler(0, 0, w.rot);
            wing.gameObject.AddComponent<WingFlap>();
        }

        // area dove si disegnano le barriere (tutto lo schermo di gioco)
        var penGo = new GameObject("Pen");
        penGo.transform.SetParent(root.transform, false);
        BoxCollider2D area = penGo.AddComponent<BoxCollider2D>();
        area.isTrigger = true;
        area.size = new Vector2(6f, 7.4f);
        area.offset = new Vector2(0, -0.2f);
        StrokeDrawer pen = penGo.AddComponent<StrokeDrawer>();
        pen.singleStroke = false;
        pen.maxStrokes = 3;
        pen.minLength = 0.3f;
        pen.addCollider = true;
        pen.colliderRadius = 0.07f;
        pen.sortingOrder = 8;
        pen.material = StrokeMaterial();
        pen.layers = new[]
        {
            new StrokeDrawer.Layer { color = InkColor, width = 0.15f, offset = Vector2.zero },
            new StrokeDrawer.Layer { color = new Color(1f, 0.85f, 0.25f), width = 0.07f, offset = Vector2.zero },
        };

        TextMesh countdown = Label(root.transform, new Vector2(0, 3.3f), 30, "10", 0.11f);
        countdown.color = new Color(1f, 0.93f, 0.75f);
        SpeechBubble bubble = Bubble(root.transform, new Vector2(0.9f, -0.9f));
        bubble.catVoice = true;   // il gatto miagola quando parla
        bubble.transform.localScale = Vector3.one * 0.55f;

        SetField(level, "wasp", wasp);
        SetField(level, "waspBody", body);
        SetField(level, "cat", cat);
        SetField(level, "catEyesOpen", eyes);
        SetField(level, "catAlert", alert);
        SetField(level, "zzz", zzz);
        SetField(level, "pen", pen);
        SetField(level, "countdown", countdown);
        SetField(level, "bubble", bubble);
        SetField(level, "buzz", buzz);
        SetField(level, "waspAnger", waspAnger);
        return SavePrefab(root, LevelDir + "/Defend/Level_Defend.prefab");
    }

    // ---------- Livello 13: le due lampade ----------

    static GameObject BuildLampsLevel()
    {
        var root = new GameObject("Level_Lamps");
        LampsLevel level = root.AddComponent<LampsLevel>();
        SpriteRenderer wall = Art(root.transform, "Wall", "plain", -20, false).GetComponent<SpriteRenderer>();
        wall.transform.localScale = new Vector3(12f, 16f, 1f);
        SpriteRenderer floor = Art(root.transform, "Floor", "plain", -19, false, new Vector2(0, -5.0f)).GetComponent<SpriteRenderer>();
        floor.transform.localScale = new Vector3(12f, 4.6f, 1f);
        floor.color = new Color(0.09f, 0.07f, 0.1f);

        GameObject red = Lamp(root.transform, "RedLamp", new Vector2(-1.35f, 2.9f), new Color(1f, 0.25f, 0.25f));
        GameObject blue = Lamp(root.transform, "BlueLamp", new Vector2(1.35f, 2.9f), new Color(0.3f, 0.45f, 1f));

        TextMesh you = Label(root.transform, new Vector2(0, 0.95f), 3, "YOU", 0.16f);
        TextMesh won = Label(root.transform, new Vector2(0, -0.55f), 3, "WON", 0.16f);

        Transform redButton = HoldButton(root.transform, "RedButton", "big_button", new Vector2(-1.4f, -3.4f), out Collider2D redCol);
        Transform blueButton = HoldButton(root.transform, "BlueButton", "blue_button", new Vector2(1.4f, -3.4f), out Collider2D blueCol);

        SetField(level, "redButton", redCol);
        SetField(level, "blueButton", blueCol);
        SetField(level, "redVisual", redButton);
        SetField(level, "blueVisual", blueButton);
        SetField(level, "wall", wall);
        SetField(level, "you", you);
        SetField(level, "won", won);
        SetField(level, "redLight", red);
        SetField(level, "blueLight", blue);
        return SavePrefab(root, LevelDir + "/Lamps/Level_Lamps.prefab");
    }

    /// <summary>Lampada appesa con la sua luce (cono + alone) che si accende e spegne; restituisce la luce.</summary>
    static GameObject Lamp(Transform parent, string name, Vector2 bulb, Color color)
    {
        var lamp = new GameObject(name).transform;
        lamp.SetParent(parent, false);
        lamp.localPosition = bulb;
        Art(lamp, "Art", "lamp", 6);
        var light = new GameObject("Light");
        light.transform.SetParent(lamp, false);
        SpriteRenderer cone = Art(light.transform, "Cone", "light_cone", 5, false).GetComponent<SpriteRenderer>();
        // sopra le scritte: schiarisce muro e inchiostro allo stesso modo, così la parola "invisibile" resta invisibile
        cone.color = new Color(1f, 1f, 1f, 0.35f);
        SpriteRenderer glow = Art(light.transform, "Glow", "glow", 7, false, new Vector2(0, -0.06f)).GetComponent<SpriteRenderer>();
        glow.color = color;
        glow.transform.localScale = Vector3.one * 1.3f;
        light.SetActive(false);
        return light;
    }

    // ---------- Livello 14: il biscotto nel barattolo ----------

    static GameObject BuildCookieLevel()
    {
        var root = new GameObject("Level_Cookie");
        CookieLevel level = root.AddComponent<CookieLevel>();
        TiltGravity tilt = root.AddComponent<TiltGravity>();
        tilt.strength = 16f;     // gravità proporzionale all'inclinazione (niente scatti)
        tilt.response = 1f;
        tilt.smoothing = 0.12f;  // filtra il tremolio dell'accelerometro
        tilt.deadZone = 0.02f;
        Art(root.transform, "Kitchen", "kitchen_bg", -20, false);

        // bordi della stanza (il biscotto non esce dallo schermo); il piano di lavoro è a y = -2
        var bounds = new GameObject("Bounds");
        bounds.transform.SetParent(root.transform, false);
        EdgeCollider2D edge = bounds.AddComponent<EdgeCollider2D>();
        edge.points = new[] { new Vector2(-2.8f, -1.94f), new Vector2(2.8f, -1.94f), new Vector2(2.8f, 3.6f), new Vector2(-2.8f, 3.6f), new Vector2(-2.8f, -1.94f) };

        // mensola e barattolo aperto in alto
        var shelf = new GameObject("Shelf");
        shelf.transform.SetParent(root.transform, false);
        shelf.transform.localPosition = new Vector3(-1.45f, -0.78f, 0);
        SpriteRenderer plank = Art(shelf.transform, "Art", "plain", 3, false).GetComponent<SpriteRenderer>();
        plank.transform.localScale = new Vector3(1.9f, 0.16f, 1f);
        plank.color = new Color(0.66f, 0.42f, 0.25f);
        shelf.AddComponent<BoxCollider2D>().size = new Vector2(1.9f, 0.16f);

        var jar = new GameObject("Jar");
        jar.transform.SetParent(root.transform, false);
        jar.transform.localPosition = new Vector3(-1.45f, 0.0f, 0);
        Art(jar.transform, "Art", "jar", 6);
        EdgeCollider2D glass = jar.AddComponent<EdgeCollider2D>();
        glass.points = new[] { new Vector2(-0.56f, 0.62f), new Vector2(-0.56f, -0.66f), new Vector2(0.56f, -0.66f), new Vector2(0.56f, 0.62f) };
        glass.edgeRadius = 0.06f;

        var cookieGo = new GameObject("Cookie");
        cookieGo.transform.SetParent(root.transform, false);
        cookieGo.transform.localPosition = new Vector3(-1.45f, -0.3f, 0);
        Art(cookieGo.transform, "Art", "cookie", 5);
        cookieGo.AddComponent<CircleCollider2D>().radius = 0.22f;
        Rigidbody2D cookie = cookieGo.AddComponent<Rigidbody2D>();
        cookie.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        cookie.interpolation = RigidbodyInterpolation2D.Interpolate;
        cookie.sleepMode = RigidbodySleepMode2D.NeverSleep;
        cookie.sharedMaterial = CookiePhysics();
        cookie.mass = 0.3f;
        SmokeEmitter crumbs = Sparks(root.transform, "Crumbs", new Color(0.78f, 0.55f, 0.3f));
        crumbs.riseSpeed = -0.6f;
        crumbs.startScale = 0.8f;
        crumbs.life = 0.6f;
        crumbs.puffsPerSecond = 35f;
        ImpactCrumbs impact = cookieGo.AddComponent<ImpactCrumbs>();
        impact.crumbs = crumbs;
        impact.minImpact = 2.2f;

        var mouse = new GameObject("Mouse").transform;
        mouse.SetParent(root.transform, false);
        mouse.localPosition = new Vector3(1.7f, -1.5f, 0);
        Art(mouse, "Art", "mouse", 4);
        var heartsGo = new GameObject("Hearts");
        heartsGo.transform.SetParent(mouse, false);
        heartsGo.transform.localPosition = new Vector3(0, 0.6f, 0);
        SmokeEmitter hearts = heartsGo.AddComponent<SmokeEmitter>();
        hearts.puffSprite = sprites["heart"];
        hearts.puffsPerSecond = 5f;
        hearts.life = 1.2f;
        hearts.riseSpeed = 0.9f;
        hearts.startScale = 0.7f;
        hearts.spread = 0.4f;
        hearts.spinRange = 30f;
        hearts.growth = 0.4f;
        hearts.tint = Color.white;
        SpeechBubble bubble = Bubble(root.transform, new Vector2(1.75f, -0.15f));
        bubble.transform.localScale = Vector3.one * 0.45f;

        SetField(level, "cookie", cookie);
        SetField(level, "mouse", mouse);
        SetField(level, "hearts", hearts);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/Cookie/Level_Cookie.prefab");
    }

    // ---------- Livello 15: spegni le candeline soffiando ----------

    static GameObject BuildBirthdayLevel()
    {
        var root = new GameObject("Level_Birthday");
        BirthdayLevel level = root.AddComponent<BirthdayLevel>();
        Art(root.transform, "Party", "party_bg", -20, false);

        var cake = new GameObject("Cake").transform;
        cake.SetParent(root.transform, false);
        cake.localPosition = new Vector3(0, -1.4f, 0);
        Art(cake, "Art", "cake", 3);
        var flames = new List<Transform>();
        foreach (float x in new[] { -0.56f, -0.28f, 0f, 0.28f, 0.56f })
        {
            Art(cake, "Candle", "candle", 4, true, new Vector2(x, 0.6f));
            var flame = new GameObject("Flame").transform;
            flame.SetParent(cake, false);
            flame.localPosition = new Vector3(x, 1.18f, 0);
            Art(flame, "Art", "flame", 5, false);
            flames.Add(flame);
        }

        var smokeGo = new GameObject("Smoke");
        smokeGo.transform.SetParent(root.transform, false);
        SmokeEmitter smoke = smokeGo.AddComponent<SmokeEmitter>();
        smoke.puffSprite = sprites["puff"];
        smoke.puffsPerSecond = 16f;
        smoke.life = 1.2f;
        smoke.riseSpeed = 0.8f;
        smoke.spread = 0.05f;
        smoke.startScale = 0.3f;
        smoke.sideDrift = 0.1f;
        smoke.tint = new Color(0.75f, 0.75f, 0.8f, 0.85f);

        var confetti = new List<SmokeEmitter>();
        foreach (Color c in new[] { new Color(1f, 0.36f, 0.54f), new Color(1f, 0.82f, 0.35f), new Color(0.43f, 0.78f, 1f), new Color(0.5f, 0.83f, 0.42f) })
        {
            var go = new GameObject("Confetti");
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = new Vector3(0, 3.6f, 0);
            SmokeEmitter e = go.AddComponent<SmokeEmitter>();
            e.puffSprite = sprites["confetti"];
            e.puffsPerSecond = 22f;
            e.life = 2.4f;
            e.riseSpeed = -1.6f;
            e.spread = 2.8f;
            e.sideDrift = 0.6f;
            e.startScale = 1.4f;
            e.growth = 0f;
            e.spinRange = 300f;
            e.tint = c;
            e.sortingOrder = 50;
            confetti.Add(e);
        }
        SpeechBubble bubble = Bubble(root.transform, new Vector2(0.6f, 1.4f));
        bubble.transform.localScale = Vector3.one * 0.6f;

        SetArray(level, "flames", flames.ToArray());
        SetField(level, "smoke", smoke);
        SetArray(level, "confetti", confetti.ToArray());
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/Birthday/Level_Birthday.prefab");
    }

    // ---------- Livelli 16-17: labirinti con trappole e al buio ----------

    /// <summary>Pareti fisiche invisibili che ricalcano esattamente i tubi al neon di un labirinto a segmenti.</summary>
    static void SegWalls(Transform maze, DoodleSprites.MazeSeg[] segs)
    {
        var walls = new GameObject("Walls").transform;
        walls.SetParent(maze, false);
        for (int i = 0; i < segs.Length; i++)
        {
            DoodleSprites.MazeSeg g = segs[i];
            if (Mathf.Approximately(g.y1, g.y2)) HWall(walls, "Wall" + i, Mathf.Min(g.x1, g.x2), Mathf.Max(g.x1, g.x2), g.y1);
            else VWall(walls, "Wall" + i, g.x1, Mathf.Min(g.y1, g.y2), Mathf.Max(g.y1, g.y2));
        }
    }

    static GoalZone MazeGoal(Transform maze, Vector2 pos)
    {
        var goal = new GameObject("Goal");
        goal.transform.SetParent(maze, false);
        goal.transform.localPosition = pos;
        Transform star = Art(goal.transform, "Art", "goal_star", 2);
        star.gameObject.AddComponent<Pulse>();
        CircleCollider2D area = goal.AddComponent<CircleCollider2D>();
        area.isTrigger = true;
        area.radius = 0.22f;
        GoalZone zone = goal.AddComponent<GoalZone>();
        zone.requiredTag = "Player";
        zone.holdSeconds = 0.25f;
        return zone;
    }

    static SmokeEmitter Sparks(Transform parent, string name, Color tint)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        SmokeEmitter e = go.AddComponent<SmokeEmitter>();
        e.puffSprite = sprites["confetti"];
        e.puffsPerSecond = 40f;
        e.life = 0.45f;
        e.riseSpeed = 0.6f;
        e.spread = 0.35f;
        e.startScale = 1.1f;
        e.growth = 0f;
        e.spinRange = 400f;
        e.sideDrift = 1.2f;
        e.tint = tint;
        e.sortingOrder = 45;
        return e;
    }

    static GameObject BuildTrapMazeLevel()
    {
        var root = new GameObject("Level_TrapMaze");
        TrapMazeLevel level = root.AddComponent<TrapMazeLevel>();
        TiltGravity tilt = root.AddComponent<TiltGravity>();
        tilt.strength = 32f;     // un filo più lento del livello 2: qui un errore costa caro
        tilt.response = 0.55f;
        Art(root.transform, "Night", "maze_bg", -20, false);

        var maze = new GameObject("Maze").transform;
        maze.SetParent(root.transform, false);
        maze.localPosition = new Vector3(0, -0.4f, 0);
        Art(maze, "Neon", "maze_trap", 5, false);
        SegWalls(maze, DoodleSprites.TrapMaze);

        var pits = new List<Transform>();
        float[] pp = DoodleSprites.TrapPits;
        for (int i = 0; i < pp.Length; i += 2)
            pits.Add(Art(maze, "Pit", "pit", 3, false, new Vector2(pp[i], pp[i + 1])));

        var lasers = new List<LaserBeam>();
        float[] lz = DoodleSprites.TrapLasers;
        for (int i = 0; i < lz.Length; i += 3)
        {
            float len = lz[i + 2] - lz[i + 1];
            var go = new GameObject("Laser");
            go.transform.SetParent(maze, false);
            go.transform.localPosition = new Vector3(lz[i], (lz[i + 1] + lz[i + 2]) * 0.5f, 0);
            LaserBeam beam = go.AddComponent<LaserBeam>();
            Transform b = Art(go.transform, "Beam", "laser", 7, false);
            b.localScale = new Vector3(1f, len, 1f);
            beam.beam = b.GetComponent<SpriteRenderer>();
            Transform top = Art(go.transform, "NodeTop", "laser_node", 8, false, new Vector2(0, len * 0.5f - 0.02f));
            Transform bottom = Art(go.transform, "NodeBottom", "laser_node", 8, false, new Vector2(0, -len * 0.5f + 0.02f));
            bottom.localRotation = Quaternion.Euler(0, 0, 180f);
            beam.lenses = new[] { top.GetComponent<SpriteRenderer>(), bottom.GetComponent<SpriteRenderer>() };
            beam.length = len;
            beam.onTime = 1.4f;
            beam.offTime = 1.7f;
            beam.phase = i == 0 ? 0f : 1.55f;
            lasers.Add(beam);
        }

        MazeGoal(maze, new Vector2(-2.2f, 2.6f));
        Rigidbody2D ball = MazeBall(maze, new Vector2(-2.2f, -3.0f));
        SmokeEmitter sparks = Sparks(root.transform, "Sparks", new Color(1f, 0.45f, 0.3f));
        SpeechBubble bubble = Bubble(root.transform, new Vector2(1.5f, -4.45f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetField(level, "ball", ball);
        SetArray(level, "pits", pits.ToArray());
        SetArray(level, "lasers", lasers.ToArray());
        SetField(level, "sparks", sparks);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/TrapMaze/Level_TrapMaze.prefab");
    }

    static GameObject BuildDarkMazeLevel()
    {
        var root = new GameObject("Level_DarkMaze");
        DarkMazeLevel level = root.AddComponent<DarkMazeLevel>();
        TiltGravity tilt = root.AddComponent<TiltGravity>();
        tilt.strength = 34f;
        tilt.response = 0.5f;
        Art(root.transform, "Night", "maze_bg", -20, false);

        var maze = new GameObject("Maze").transform;
        maze.SetParent(root.transform, false);
        maze.localPosition = new Vector3(0, -0.4f, 0);
        Art(maze, "Neon", "maze_dark", 5, false);
        SegWalls(maze, DoodleSprites.DarkMaze);

        // cancello rosso con lucchetto davanti all'uscita
        DoodleSprites.MazeSeg g = DoodleSprites.DarkGate;
        var gate = new GameObject("Gate");
        gate.transform.SetParent(maze, false);
        gate.transform.localPosition = new Vector3((g.x1 + g.x2) * 0.5f, (g.y1 + g.y2) * 0.5f, 0);
        Transform bar = Art(gate.transform, "Art", "gate_bar", 6, false);
        bool vertical = Mathf.Approximately(g.x1, g.x2);
        if (vertical) bar.localRotation = Quaternion.Euler(0, 0, 90f);
        CapsuleCollider2D gateCol = gate.AddComponent<CapsuleCollider2D>();
        gateCol.size = vertical ? new Vector2(WallT, 1f + WallT) : new Vector2(1f + WallT, WallT);
        gateCol.direction = vertical ? CapsuleDirection2D.Vertical : CapsuleDirection2D.Horizontal;

        // chiave luminosa in un vicolo cieco
        var key = new GameObject("Key").transform;
        key.SetParent(maze, false);
        key.localPosition = new Vector3(DoodleSprites.DarkKeyX, DoodleSprites.DarkKeyY, 0);
        key.localScale = Vector3.one * 0.32f;
        SpriteRenderer halo = Art(key, "Glow", "glow", 4, false).GetComponent<SpriteRenderer>();
        halo.color = new Color(1f, 0.85f, 0.3f, 0.9f);
        halo.transform.localScale = Vector3.one * 2.4f;
        Art(key, "Art", "key", 11, false, new Vector2(0.6f, 0f));

        MazeGoal(maze, new Vector2(DoodleSprites.DarkExitX, DoodleSprites.DarkExitY));
        Rigidbody2D ball = MazeBall(maze, new Vector2(-2.2f, -3.0f));
        Transform darkness = Art(root.transform, "Darkness", "darkness", 20, false);
        SmokeEmitter sparkle = Sparks(root.transform, "Sparkle", new Color(1f, 0.85f, 0.3f));
        SpeechBubble bubble = Bubble(root.transform, new Vector2(1.5f, -4.45f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetField(level, "ball", ball);
        SetField(level, "darkness", darkness);
        SetField(level, "key", key);
        SetField(level, "gate", gate);
        SetField(level, "sparkle", sparkle);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/DarkMaze/Level_DarkMaze.prefab");
    }

    // ---------- Livello 18: la cassaforte nascosta ----------

    static GameObject BuildSafeLevel(GameObject keyPrefab, GameObject doorPrefab)
    {
        var root = new GameObject("Level_Safe");
        SafeLevel level = root.AddComponent<SafeLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);
        GameObject door = Instance(doorPrefab, root.transform, new Vector2(1.3f, -0.9f));
        door.name = "Door";

        // cassaforte a muro (coperta dal quadro)
        var safe = new GameObject("Safe").transform;
        safe.SetParent(root.transform, false);
        safe.localPosition = new Vector3(-1.65f, 1.75f, 0);
        safe.localScale = Vector3.one * 0.7f;
        GameObject closed = Art(safe, "Closed", "safe", 1).gameObject;
        GameObject open = Art(safe, "Open", "safe_open", 1).gameObject;
        TextMesh safeDisplay = Label(closed.transform, new Vector2(0, 0.68f), 2, "_ _ _", 0.05f);
        BoxCollider2D safeArea = safe.gameObject.AddComponent<BoxCollider2D>();
        safeArea.size = new Vector2(1.9f, 2.1f);

        GameObject key = Instance(keyPrefab, root.transform, new Vector2(-1.3f, 1.62f));
        key.transform.localScale = Vector3.one * 0.45f;

        var portrait = new GameObject("Portrait").transform;
        portrait.SetParent(root.transform, false);
        portrait.localPosition = new Vector3(-1.65f, 1.75f, 0);
        portrait.localScale = Vector3.one * 0.78f;
        Art(portrait, "Art", "portrait", 3);
        portrait.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.9f, 2.2f);

        var back = new GameObject("PaintingBack").transform;
        back.SetParent(root.transform, false);
        back.localPosition = new Vector3(-1.35f, -3.05f, 0);
        back.localRotation = Quaternion.Euler(0, 0, -6f);
        back.localScale = Vector3.one * 0.72f;
        Art(back, "Art", "painting_back", 3);
        TextMesh note = Label(back, new Vector2(0, -0.17f), 4, "000", 0.08f);
        note.transform.localRotation = Quaternion.Euler(0, 0, -4f);

        var dustGo = new GameObject("Dust");
        dustGo.transform.SetParent(root.transform, false);
        SmokeEmitter dust = dustGo.AddComponent<SmokeEmitter>();
        dust.puffSprite = sprites["puff"];
        dust.puffsPerSecond = 30f;
        dust.life = 0.9f;
        dust.riseSpeed = 0.4f;
        dust.spread = 0.8f;
        dust.sideDrift = 1.0f;
        dust.startScale = 0.5f;
        dust.tint = new Color(0.9f, 0.85f, 0.75f, 0.9f);

        // tastiera ingrandita (compare toccando la cassaforte)
        var keypad = new GameObject("Keypad").transform;
        keypad.SetParent(root.transform, false);
        SpriteRenderer dim = Art(keypad, "Dim", "plain", 30, false).GetComponent<SpriteRenderer>();
        dim.transform.localScale = new Vector3(12f, 16f, 1f);
        dim.color = new Color(0f, 0f, 0f, 0.45f);
        SpriteRenderer panel = Art(keypad, "Panel", "plain", 31, false, new Vector2(0, 0.95f)).GetComponent<SpriteRenderer>();
        panel.transform.localScale = new Vector3(4.3f, 5.6f, 1f);
        panel.color = new Color(0.55f, 0.58f, 0.66f);
        BoxCollider2D panelCol = keypad.gameObject.AddComponent<BoxCollider2D>();
        panelCol.size = new Vector2(4.3f, 5.6f);
        panelCol.offset = new Vector2(0, 0.95f);
        SpriteRenderer screen = Art(keypad, "Screen", "plain", 32, false, new Vector2(0, 3.05f)).GetComponent<SpriteRenderer>();
        screen.transform.localScale = new Vector3(3.4f, 0.85f, 1f);
        screen.color = new Color(0.06f, 0.23f, 0.16f);
        TextMesh display = Label(keypad, new Vector2(0, 3.05f), 33, "_ _ _", 0.13f);
        var digits = new List<TapButton>();
        for (int n = 0; n <= 9; n++)
        {
            Vector2 pos = n == 0 ? new Vector2(0, -1.4f) : new Vector2(-1.2f + ((n - 1) % 3) * 1.2f, 1.95f - ((n - 1) / 3) * 1.12f);
            var k = new GameObject("Key" + n);
            k.transform.SetParent(keypad, false);
            k.transform.localPosition = pos;
            Transform art = Art(k.transform, "Art", "keypad_key", 33, false);
            art.localScale = Vector3.one * 2.7f;
            Label(art, new Vector2(0, 0.01f), 34, n.ToString(), 0.05f);
            k.AddComponent<BoxCollider2D>().size = new Vector2(1.05f, 1.0f);
            TapButton tb = k.AddComponent<TapButton>();
            tb.id = n.ToString();
            tb.visual = art;
            digits.Add(tb);
        }
        keypad.gameObject.SetActive(false);

        SpeechBubble bubble = Bubble(root.transform, new Vector2(0.55f, 2.9f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetField(level, "portrait", portrait);
        SetField(level, "paintingBack", back.gameObject);
        SetField(level, "codeNote", note);
        SetField(level, "safeArea", safeArea);
        SetField(level, "safeClosed", closed);
        SetField(level, "safeOpen", open);
        SetField(level, "keypad", keypad.gameObject);
        SetField(level, "keypadPanel", panelCol);
        SetArray(level, "digits", digits.ToArray());
        SetField(level, "display", display);
        SetField(level, "safeDisplay", safeDisplay);
        SetField(level, "key", key.GetComponent<Draggable2D>());
        SetField(level, "dust", dust);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/Safe/Level_Safe.prefab");
    }

    // ---------- Livello 19: a nanna ----------

    /// <summary>Barre LED del volume sul display dello stereo (dal verde al rosso).</summary>
    static VolumeLeds Leds(Transform hifi, int order)
    {
        var go = new GameObject("Leds");
        go.transform.SetParent(hifi, false);
        var bars = new List<SpriteRenderer>();
        for (int i = 0; i < 10; i++)
        {
            Transform t = Art(go.transform, "Led" + i, "led", order, false, new Vector2(-0.405f + i * 0.09f, 0.22f));
            t.localScale = new Vector3(0.85f, 1.15f, 1f);
            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            sr.color = i < 6 ? new Color(0.35f, 1f, 0.45f) : i < 8 ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.25f, 0.25f);
            bars.Add(sr);
        }
        VolumeLeds leds = go.AddComponent<VolumeLeds>();
        leds.bars = bars.ToArray();
        return leds;
    }

    static GameObject BuildBedtimeLevel()
    {
        var root = new GameObject("Level_Bedtime");
        BedtimeLevel level = root.AddComponent<BedtimeLevel>();
        AudioSource stereo = root.AddComponent<AudioSource>();
        stereo.playOnAwake = false;
        stereo.loop = true;
        stereo.clip = Soundtrack();
        Art(root.transform, "Room", "night_bg", -20, false);

        // stereo sul mobile
        var hifi = new GameObject("HiFi").transform;
        hifi.SetParent(root.transform, false);
        hifi.localPosition = new Vector3(-1.3f, -0.55f, 0);
        hifi.localScale = Vector3.one * 0.85f;
        Art(hifi, "Unit", "hifi", 4);
        Leds(hifi, 5);
        Transform sp = Art(hifi, "Speaker", "speaker", 4, false, new Vector2(1.25f, 0.1f));
        Pulse pulse = sp.gameObject.AddComponent<Pulse>();
        pulse.speed = 14f;
        pulse.amount = 0.05f;
        var notesGo = new GameObject("Notes");
        notesGo.transform.SetParent(hifi, false);
        notesGo.transform.localPosition = new Vector3(0, 0.6f, 0);
        SmokeEmitter notes = notesGo.AddComponent<SmokeEmitter>();
        notes.puffSprite = sprites["note"];
        notes.puffsPerSecond = 4f;
        notes.life = 1.6f;
        notes.riseSpeed = 0.7f;
        notes.spread = 1.2f;
        notes.sideDrift = 0.4f;
        notes.startScale = 0.9f;
        notes.growth = 0.3f;
        notes.spinRange = 25f;
        notes.tint = Color.white;
        SpriteRenderer shelf = Art(root.transform, "Shelf", "plain", 2, false, new Vector2(-1.0f, -1.1f)).GetComponent<SpriteRenderer>();
        shelf.transform.localScale = new Vector3(2.9f, 0.14f, 1f);
        shelf.color = new Color(0.55f, 0.36f, 0.24f);

        // abat-jour sul comodino
        SpriteRenderer stand = Art(root.transform, "Nightstand", "plain", 1, false, new Vector2(1.95f, -2.15f)).GetComponent<SpriteRenderer>();
        stand.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
        stand.color = new Color(0.62f, 0.42f, 0.28f);
        var lamp = new GameObject("Lamp").transform;
        lamp.SetParent(root.transform, false);
        lamp.localPosition = new Vector3(1.95f, -1.5f, 0);
        Transform lampArt = Art(lamp, "Art", "bedside_lamp", 6);
        BoxCollider2D lampCol = lamp.gameObject.AddComponent<BoxCollider2D>();
        lampCol.size = new Vector2(1.6f, 1.9f);
        lampCol.offset = new Vector2(0, 0.85f);
        TapButton lampSwitch = lamp.gameObject.AddComponent<TapButton>();
        lampSwitch.visual = lampArt;
        lampSwitch.pressedScaleY = 0.92f;
        var lightGo = new GameObject("Light");
        lightGo.transform.SetParent(lamp, false);
        SpriteRenderer glow = Art(lightGo.transform, "Glow", "glow", 0, false, new Vector2(0, 1.12f)).GetComponent<SpriteRenderer>();
        glow.color = new Color(1f, 0.9f, 0.55f, 0.75f);
        glow.transform.localScale = Vector3.one * 5f;
        SpriteRenderer bulb = Art(lightGo.transform, "Bulb", "glow", 7, false, new Vector2(0, 1.12f)).GetComponent<SpriteRenderer>();
        bulb.color = new Color(1f, 0.95f, 0.6f, 1f);
        bulb.transform.localScale = Vector3.one * 0.7f;

        // gatto assonnato sul cuscino
        var cat = new GameObject("Cat").transform;
        cat.SetParent(root.transform, false);
        cat.localPosition = new Vector3(-0.55f, -2.85f, 0);
        cat.localScale = Vector3.one * 1.1f;
        Transform catArt = Art(cat, "Art", "cat", 2);
        Pulse breathe = catArt.gameObject.AddComponent<Pulse>();
        breathe.amount = 0.02f;
        breathe.speed = 1.8f;
        GameObject eyesOpen = Art(cat, "EyesOpen", "cat_eyes", 3, false).gameObject;
        GameObject eyesHalf = Art(cat, "EyesHalf", "cat_face_3", 3, false).gameObject;
        eyesHalf.SetActive(false);
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

        SpriteRenderer dark = Art(root.transform, "Dark", "plain", 15, false).GetComponent<SpriteRenderer>();
        dark.transform.localScale = new Vector3(12f, 16f, 1f);
        dark.color = new Color(0.02f, 0.02f, 0.08f, 0f);

        SpeechBubble bubble = Bubble(root.transform, new Vector2(0.3f, -0.9f));
        bubble.catVoice = true;   // il gatto miagola quando parla
        bubble.transform.localScale = Vector3.one * 0.55f;

        SetField(level, "lampSwitch", lampSwitch);
        SetField(level, "lampLight", lightGo);
        SetField(level, "roomDark", dark);
        SetField(level, "stereo", stereo);
        SetField(level, "notes", notes);
        SetField(level, "catEyesOpen", eyesOpen);
        SetField(level, "catEyesHalf", eyesHalf);
        SetField(level, "zzz", zzz);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/Bedtime/Level_Bedtime.prefab");
    }

    // ---------- Livello 20: il razzo ----------

    static GameObject BuildRocketLevel()
    {
        var root = new GameObject("Level_Rocket");
        RocketLevel level = root.AddComponent<RocketLevel>();
        Art(root.transform, "Site", "launch_bg", -20, false);

        var rocket = new GameObject("Rocket").transform;
        rocket.SetParent(root.transform, false);
        rocket.localPosition = new Vector3(0.3f, -2.63f, 0);
        Art(rocket, "Art", "rocket", 5);
        var flame = new GameObject("Flame").transform;
        flame.SetParent(rocket, false);
        flame.localPosition = new Vector3(0, 0.02f, 0);
        Transform fl = Art(flame, "Art", "flame", 4, false);
        fl.localRotation = Quaternion.Euler(0, 0, 180f);
        fl.localPosition = Vector3.zero;
        fl.localScale = Vector3.one * 3.2f;
        var cap = new GameObject("FuelCap");
        cap.transform.SetParent(rocket, false);
        cap.transform.localPosition = new Vector3(0, 1.0f, 0);
        cap.AddComponent<CircleCollider2D>().radius = 0.55f;
        DropSlot slot = cap.AddComponent<DropSlot>();
        slot.acceptedId = "fuel";

        var can = new GameObject("FuelCan");
        can.transform.SetParent(root.transform, false);
        can.transform.localPosition = new Vector3(1.95f, -3.7f, 0);
        Art(can.transform, "Art", "fuel_can", 12);
        can.AddComponent<BoxCollider2D>().size = new Vector2(1.0f, 1.2f);
        Draggable2D drag = can.AddComponent<Draggable2D>();
        drag.itemId = "fuel";

        var smokeGo = new GameObject("Smoke");
        smokeGo.transform.SetParent(root.transform, false);
        smokeGo.transform.localPosition = new Vector3(0.3f, -2.7f, 0);
        SmokeEmitter smoke = smokeGo.AddComponent<SmokeEmitter>();
        smoke.puffSprite = sprites["puff"];
        smoke.puffsPerSecond = 12f;
        smoke.life = 1.4f;
        smoke.riseSpeed = 0.25f;
        smoke.spread = 0.6f;
        smoke.sideDrift = 1.4f;
        smoke.startScale = 0.9f;
        smoke.growth = 1.6f;
        smoke.tint = new Color(0.95f, 0.95f, 0.97f, 0.95f);
        smoke.sortingOrder = 9;

        // pannello di lancio: carburante, coperchio di sicurezza, pulsante
        TextMesh fuelLabel = Label(root.transform, new Vector2(-2.45f, -3.25f), 8, "FUEL", 0.035f);
        fuelLabel.color = Color.white;
        SpriteRenderer gaugeBg = Art(root.transform, "GaugeBg", "plain", 7, false, new Vector2(-1.3f, -3.25f)).GetComponent<SpriteRenderer>();
        gaugeBg.transform.localScale = new Vector3(1.7f, 0.26f, 1f);
        gaugeBg.color = new Color(0.17f, 0.13f, 0.2f);
        var fill = new GameObject("FuelFill").transform;
        fill.SetParent(root.transform, false);
        fill.localPosition = new Vector3(-2.11f, -3.25f, 0);
        SpriteRenderer fillArt = Art(fill, "Bar", "plain", 8, false, new Vector2(0.81f, 0f)).GetComponent<SpriteRenderer>();
        fillArt.transform.localScale = new Vector3(1.62f, 0.18f, 1f);
        fillArt.color = new Color(0.36f, 1f, 0.45f);

        Transform launchVisual = HoldButton(root.transform, "LaunchButton", "big_button", new Vector2(-1.3f, -4.2f), out Collider2D launchCol);
        var cover = new GameObject("SafetyCover");
        cover.transform.SetParent(root.transform, false);
        cover.transform.localPosition = new Vector3(-1.3f, -3.55f, 0);
        Transform coverArt = Art(cover.transform, "Art", "safety_cover", 9, false);
        BoxCollider2D coverCol = cover.AddComponent<BoxCollider2D>();
        coverCol.size = new Vector2(1.6f, 0.9f);
        coverCol.offset = new Vector2(0, -0.4f);
        TapButton coverButton = cover.AddComponent<TapButton>();
        coverButton.visual = coverArt;
        coverButton.pressedScaleY = 0.95f;

        TextMesh countdown = Label(root.transform, new Vector2(0, 3.1f), 30, "", 0.13f);
        SpeechBubble bubble = Bubble(root.transform, new Vector2(1.75f, 0.9f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetField(level, "rocket", rocket);
        SetField(level, "fuelCap", slot);
        SetField(level, "fuelFill", fill);
        SetField(level, "cover", coverButton);
        SetField(level, "coverHinge", cover.transform);
        SetField(level, "launchButton", launchCol);
        SetField(level, "launchVisual", launchVisual);
        SetField(level, "countdown", countdown);
        SetField(level, "flame", flame);
        SetField(level, "smoke", smoke);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/Rocket/Level_Rocket.prefab");
    }

    // ---------- Livelli 21-31 ----------

    static SmokeEmitter Emitter(Transform parent, string name, Vector2 pos, string sprite, float rate, float life, float rise, float spread, float scale, Color tint, int order = 40)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        SmokeEmitter e = go.AddComponent<SmokeEmitter>();
        e.puffSprite = sprites[sprite];
        e.puffsPerSecond = rate;
        e.life = life;
        e.riseSpeed = rise;
        e.spread = spread;
        e.startScale = scale;
        e.tint = tint;
        e.sortingOrder = order;
        return e;
    }

    static SpriteRenderer Bar(Transform parent, string name, Vector2 pos, Vector2 size, Color color, int order)
    {
        SpriteRenderer sr = Art(parent, name, "plain", order, false, pos).GetComponent<SpriteRenderer>();
        sr.transform.localScale = new Vector3(size.x, size.y, 1f);
        sr.color = color;
        return sr;
    }

    static LineRenderer Line(Transform parent, string name, float width, Color color, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.widthMultiplier = width;
        lr.numCapVertices = 4;
        lr.sharedMaterial = StrokeMaterial();
        lr.startColor = lr.endColor = color;
        lr.sortingOrder = order;
        return lr;
    }

    static GameObject BuildBatteryLevel()
    {
        var root = new GameObject("Level_Battery");
        BatteryLevel level = root.AddComponent<BatteryLevel>();
        Art(root.transform, "Workshop", "workshop_bg", -20, false);

        var robot = new GameObject("Robot").transform;
        robot.SetParent(root.transform, false);
        robot.localPosition = new Vector3(-0.85f, -3.3f, 0);
        robot.localScale = Vector3.one * 1.3f;
        GameObject off = Art(robot, "Off", "robot_off", 5).gameObject;
        GameObject on = Art(robot, "On", "robot_on", 5).gameObject;
        GameObject sad = Art(robot, "Sad", "robot_sad", 5).gameObject;
        GameObject mecha = Art(robot, "Mecha", "robot_mecha", 5).gameObject;   // Easter egg: l'evoluzione
        SpriteRenderer aura = Art(robot, "Aura", "glow", 3, false, new Vector2(0, 1.5f)).GetComponent<SpriteRenderer>();
        BoxCollider2D body = robot.gameObject.AddComponent<BoxCollider2D>();
        body.size = new Vector2(2f, 2.9f);
        body.offset = new Vector2(0, 1.45f);
        SmokeEmitter notes = Emitter(robot, "Notes", new Vector2(0, 2.6f), "note", 5f, 1.6f, 0.8f, 1.2f, 0.9f, Color.white);

        // indicatore della batteria, grande, sopra il robot
        var battery = new GameObject("Battery").transform;
        battery.SetParent(root.transform, false);
        battery.localPosition = new Vector3(0, 1.5f, 0);
        battery.localScale = Vector3.one * 2.6f;
        SpriteRenderer frame = Art(battery, "Frame", "battery", 6, false).GetComponent<SpriteRenderer>();
        var fill = new GameObject("Fill").transform;
        fill.SetParent(battery, false);
        fill.localPosition = new Vector3(-0.25f, 0, 0);
        SpriteRenderer bar = Bar(fill, "Bar", new Vector2(0.25f, 0f), new Vector2(0.5f, 0.2f), new Color(1f, 0.25f, 0.25f), 5);
        GameObject bolt = Art(battery, "Bolt", "bolt", 7, true, new Vector2(0.62f, 0f)).gameObject;

        SpeechBubble bubble = Bubble(root.transform, new Vector2(1.0f, -0.15f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        // niente caricatore? un pulsante fa comparire la dinamo a manovella
        var skip = new GameObject("NoCharger");
        skip.transform.SetParent(root.transform, false);
        skip.transform.localPosition = new Vector3(1.55f, -4.0f, 0);
        Transform skipArt = new GameObject("Visual").transform;
        skipArt.SetParent(skip.transform, false);
        Bar(skipArt, "Back", Vector2.zero, new Vector2(2.3f, 0.62f), new Color(0.17f, 0.75f, 0.71f), 20);
        TextMesh skipText = Label(skipArt, Vector2.zero, 21, "NO CHARGER?", 0.045f);
        skipText.color = Color.white;
        skip.AddComponent<BoxCollider2D>().size = new Vector2(2.3f, 0.7f);
        TapButton skipButton = skip.AddComponent<TapButton>();
        skipButton.visual = skipArt;

        var crank = new GameObject("Crank");
        crank.transform.SetParent(root.transform, false);
        crank.transform.localPosition = new Vector3(1.55f, -1.95f, 0);
        Art(crank.transform, "Wheel", "crank_wheel", 6);
        var arm = new GameObject("Arm").transform;
        arm.SetParent(crank.transform, false);
        Art(arm, "Art", "crank_arm", 7, false);
        LineRenderer cable = Line(crank.transform, "Cable", 0.06f, new Color(0.17f, 0.13f, 0.2f), 4);
        cable.useWorldSpace = false;
        cable.positionCount = 3;
        cable.SetPositions(new[] { new Vector3(0f, -1.05f, 0f), new Vector3(-0.9f, -1.45f, 0f), new Vector3(-1.75f, -0.4f, 0f) });
        TextMesh crankText = Label(crank.transform, new Vector2(0, 1.1f), 21, "SPIN ME!", 0.05f);
        crankText.color = Color.white;
        crank.SetActive(false);

        SpriteRenderer flash = Bar(root.transform, "Flash", Vector2.zero, new Vector2(12f, 16f), new Color(1f, 1f, 1f, 0f), 55);
        SmokeEmitter sparks = Sparks(robot, "Sparks", new Color(0.55f, 0.95f, 1f));
        sparks.transform.localPosition = new Vector3(0, 1.4f, 0);
        sparks.spread = 1.2f;
        SetField(level, "robotMecha", mecha);
        SetField(level, "batteryUi", battery.gameObject);
        SetField(level, "aura", aura);
        SetField(level, "flash", flash);
        SetField(level, "sparks", sparks);
        SetField(level, "skipButton", skipButton);
        SetField(level, "crank", crank);
        SetField(level, "crankArm", arm);
        SetField(level, "robotSad", sad);
        SetField(level, "robotOff", off);
        SetField(level, "robotOn", on);
        SetField(level, "robot", robot);
        SetField(level, "batteryFill", fill);
        SetField(level, "batteryBar", bar);
        SetField(level, "batteryFrame", frame);
        SetField(level, "bolt", bolt);
        SetField(level, "notes", notes);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/Battery/Level_Battery.prefab");
    }

    static GameObject BuildSoupLevel()
    {
        var root = new GameObject("Level_Soup");
        SoupLevel level = root.AddComponent<SoupLevel>();
        Art(root.transform, "Kitchen", "kitchen_bg", -20, false);

        var bowl = new GameObject("Bowl");
        bowl.transform.SetParent(root.transform, false);
        bowl.transform.localPosition = new Vector3(-1.3f, -1.72f, 0);
        Art(bowl.transform, "Art", "soup", 8);
        bowl.AddComponent<BoxCollider2D>().size = new Vector2(1.5f, 0.8f);
        Draggable2D drag = bowl.AddComponent<Draggable2D>();
        drag.itemId = "soup";
        drag.returnOnMiss = false;
        SmokeEmitter steam = Emitter(bowl.transform, "Steam", new Vector2(0, 0.25f), "puff", 9f, 1.4f, 0.9f, 0.45f, 0.45f, new Color(1f, 1f, 1f, 0.7f));
        steam.growth = 1.6f;
        steam.sideDrift = 0.3f;

        // termometro: il mercurio scende soffiando
        var thermo = new GameObject("Thermometer").transform;
        thermo.SetParent(root.transform, false);
        thermo.localPosition = new Vector3(-2.35f, -1.25f, 0);
        thermo.localScale = Vector3.one * 1.2f;
        Art(thermo, "Glass", "thermo", 7);
        SpriteRenderer bulb = Art(thermo, "Bulb", "drop", 8, false).GetComponent<SpriteRenderer>();
        bulb.transform.localScale = Vector3.one * 1.5f;
        bulb.color = new Color(1f, 0.2f, 0.2f);
        var mercury = new GameObject("Mercury").transform;
        mercury.SetParent(thermo, false);
        SpriteRenderer mbar = Bar(mercury, "Bar", new Vector2(0, 0.57f), new Vector2(0.08f, 1.14f), new Color(1f, 0.2f, 0.2f), 8);

        var mouse = new GameObject("Mouse").transform;
        mouse.SetParent(root.transform, false);
        mouse.localPosition = new Vector3(1.7f, -1.5f, 0);
        SpriteRenderer mouseArt = Art(mouse, "Art", "mouse", 4).GetComponent<SpriteRenderer>();
        GameObject skeleton = Art(mouse, "Skeleton", "mouse_skeleton", 4).gameObject;   // Easter egg: zuppa bollente
        SpriteRenderer burnGlow = Art(mouse, "BurnGlow", "glow", 3, false, new Vector2(0, 0.15f)).GetComponent<SpriteRenderer>();
        burnGlow.transform.localScale = Vector3.one * 4f;
        SmokeEmitter fire = Emitter(mouse, "Fire", new Vector2(0, -0.1f), "flame", 26f, 0.7f, 1.6f, 0.45f, 2.6f, Color.white, 45);
        fire.growth = -0.3f;
        fire.spinRange = 15f;
        SmokeEmitter blackSmoke = Emitter(mouse, "BlackSmoke", new Vector2(0, 0.5f), "puff", 6f, 1.8f, 0.7f, 0.3f, 0.5f, new Color(0.22f, 0.2f, 0.24f, 0.85f), 46);
        blackSmoke.growth = 1.8f;
        SmokeEmitter hearts = Emitter(mouse, "Hearts", new Vector2(0, 0.6f), "heart", 5f, 1.2f, 0.9f, 0.4f, 0.7f, Color.white);
        SpeechBubble bubble = Bubble(root.transform, new Vector2(1.75f, -0.15f));
        bubble.transform.localScale = Vector3.one * 0.45f;

        SetField(level, "bowl", drag);
        SetField(level, "steam", steam);
        SetField(level, "mercury", mercury);
        SetField(level, "mercuryBar", mbar);
        SetField(level, "mouse", mouse);
        SetField(level, "mouseArt", mouseArt);
        SetField(level, "skeleton", skeleton);
        SetField(level, "fire", fire);
        SetField(level, "blackSmoke", blackSmoke);
        SetField(level, "burnGlow", burnGlow);
        SetField(level, "hearts", hearts);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/Soup/Level_Soup.prefab");
    }

    static GameObject BuildFrozenKeyLevel(GameObject keyPrefab, GameObject doorPrefab)
    {
        var root = new GameObject("Level_FrozenKey");
        FrozenKeyLevel level = root.AddComponent<FrozenKeyLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);
        GameObject door = Instance(doorPrefab, root.transform, new Vector2(0.9f, -0.9f));
        door.name = "Door";

        GameObject key = Instance(keyPrefab, root.transform, new Vector2(-0.6f, -2.95f));
        key.transform.localScale = Vector3.one * 1.15f;   // troppo grande per la serratura: va rimpicciolita

        var puddle = new GameObject("Puddle").transform;
        puddle.SetParent(root.transform, false);
        puddle.localPosition = new Vector3(-1.3f, -3.5f, 0);
        Art(puddle, "Art", "puddle", 1);

        var ice = new GameObject("Ice").transform;
        ice.SetParent(root.transform, false);
        ice.localPosition = new Vector3(-1.3f, -2.95f, 0);
        SpriteRenderer iceArt = Art(ice, "Art", "ice_block", 25).GetComponent<SpriteRenderer>();
        BoxCollider2D iceArea = ice.gameObject.AddComponent<BoxCollider2D>();
        iceArea.size = new Vector2(2.1f, 1.4f);

        SmokeEmitter drips = Emitter(root.transform, "Drips", new Vector2(-1.3f, -3.2f), "sweat", 10f, 0.6f, -1.6f, 0.8f, 1.1f, new Color(0.75f, 0.9f, 1f, 0.95f), 26);
        drips.spinRange = 0f;
        SmokeEmitter steam = Emitter(root.transform, "Steam", new Vector2(-1.3f, -2.4f), "puff", 6f, 1f, 0.8f, 0.7f, 0.35f, new Color(1f, 1f, 1f, 0.6f), 26);
        SpeechBubble bubble = Bubble(root.transform, new Vector2(-1.0f, -1.2f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetField(level, "ice", ice);
        SetField(level, "iceArt", iceArt);
        SetField(level, "iceArea", iceArea);
        SetField(level, "key", key.GetComponent<Draggable2D>());
        SetField(level, "puddle", puddle);
        SetField(level, "drips", drips);
        SetField(level, "steam", steam);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/FrozenKey/Level_FrozenKey.prefab");
    }

    static GameObject BuildCampfireLevel()
    {
        var root = new GameObject("Level_Campfire");
        CampfireLevel level = root.AddComponent<CampfireLevel>();
        Art(root.transform, "Camp", "camp_bg", -20, false);

        SpriteRenderer glow = Art(root.transform, "Glow", "glow", -5, false, new Vector2(-0.2f, -2.3f)).GetComponent<SpriteRenderer>();
        glow.transform.localScale = Vector3.one * 7f;
        var logs = new GameObject("Logs");
        logs.transform.SetParent(root.transform, false);
        logs.transform.localPosition = new Vector3(-0.2f, -2.95f, 0);
        Art(logs.transform, "Art", "logs", 5);
        BoxCollider2D area = logs.AddComponent<BoxCollider2D>();
        area.size = new Vector2(3.0f, 1.6f);
        area.offset = new Vector2(0, 0.3f);

        var flame = new GameObject("Flame").transform;
        flame.SetParent(root.transform, false);
        flame.localPosition = new Vector3(-0.2f, -2.85f, 0);
        Art(flame, "Art", "flame", 6, false);
        var stick = new GameObject("Stick").transform;
        stick.SetParent(root.transform, false);
        stick.localPosition = new Vector3(0.2f, -2.75f, 0);
        Transform stickArt = Art(stick, "Art", "fire_stick", 7, false);
        stickArt.localRotation = Quaternion.Euler(0, 0, -20f);

        SmokeEmitter smoke = Emitter(root.transform, "Smoke", new Vector2(-0.2f, -2.6f), "puff", 8f, 1.4f, 0.8f, 0.4f, 0.4f, new Color(0.8f, 0.8f, 0.85f, 0.8f));
        smoke.growth = 1.6f;
        SmokeEmitter sparks = Sparks(root.transform, "Sparks", new Color(1f, 0.7f, 0.25f));

        // un topolino che trema dal freddo
        var mouse = new GameObject("Mouse").transform;
        mouse.SetParent(root.transform, false);
        mouse.localPosition = new Vector3(1.75f, -2.9f, 0);
        mouse.localScale = Vector3.one * 0.85f;
        Art(mouse, "Art", "mouse", 4);
        mouse.gameObject.AddComponent<Tremble>();
        SpeechBubble bubble = Bubble(root.transform, new Vector2(1.3f, -1.15f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetField(level, "logsArea", area);
        SetField(level, "stick", stick);
        SetField(level, "flame", flame);
        SetField(level, "glow", glow);
        SetField(level, "smoke", smoke);
        SetField(level, "sparks", sparks);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/Campfire/Level_Campfire.prefab");
    }

    static GameObject BuildRunawayLevel()
    {
        var root = new GameObject("Level_Runaway");
        RunawayLevel level = root.AddComponent<RunawayLevel>();
        AudioSource alarm = root.AddComponent<AudioSource>();
        alarm.playOnAwake = false;
        Art(root.transform, "Room", "room_bg", -20, false);

        // sveglia sul comodino
        Bar(root.transform, "Nightstand", new Vector2(1.55f, -2.45f), new Vector2(1.5f, 1.2f), new Color(0.62f, 0.42f, 0.28f), 1);
        var clock = new GameObject("AlarmClock").transform;
        clock.SetParent(root.transform, false);
        clock.localPosition = new Vector3(1.55f, -1.85f, 0);
        clock.localScale = Vector3.one * 0.9f;
        Art(clock, "Art", "alarm_clock", 4);
        GameObject waves = Art(clock, "Waves", "ring_waves", 4, true, new Vector2(0, 1.1f)).gameObject;
        waves.AddComponent<Pulse>();
        var socket = new GameObject("Socket").transform;
        socket.SetParent(clock, false);
        socket.localPosition = new Vector3(0, 1.62f, 0);

        // il pulsante OFF scappato, con le gambette
        var button = new GameObject("Button").transform;
        button.SetParent(root.transform, false);
        button.localPosition = new Vector3(0.3f, -2.9f, 0);
        button.localScale = Vector3.one * 0.6f;
        var visual = new GameObject("Visual").transform;
        visual.SetParent(button, false);
        Art(visual, "Art", "big_button", 6);
        Art(visual, "Face", "button_face", 7, true, new Vector2(0, 0.02f));
        TextMesh off = Label(visual, new Vector2(0, -0.2f), 7, "OFF", 0.07f);
        off.color = Color.white;
        var legs = new GameObject("Legs").transform;
        legs.SetParent(button, false);
        legs.localPosition = new Vector3(0, -0.42f, 0);
        Art(legs, "Art", "button_legs", 5, false);

        // staccionate disegnate col dito
        var penGo = new GameObject("Fences");
        penGo.transform.SetParent(root.transform, false);
        BoxCollider2D area = penGo.AddComponent<BoxCollider2D>();
        area.isTrigger = true;
        area.size = new Vector2(6f, 7.2f);
        area.offset = new Vector2(0, -0.6f);
        StrokeDrawer pen = penGo.AddComponent<StrokeDrawer>();
        pen.singleStroke = false;
        pen.maxStrokes = 4;
        pen.minLength = 0.4f;
        pen.addCollider = true;
        pen.colliderRadius = 0.07f;
        pen.sortingOrder = 8;
        pen.material = StrokeMaterial();
        pen.layers = new[]
        {
            new StrokeDrawer.Layer { color = InkColor, width = 0.16f, offset = Vector2.zero },
            new StrokeDrawer.Layer { color = new Color(0.95f, 0.75f, 0.45f), width = 0.08f, offset = Vector2.zero },
        };

        SpeechBubble bubble = Bubble(root.transform, new Vector2(0.15f, 0.2f));
        bubble.transform.localScale = Vector3.one * 0.55f;

        SetField(level, "button", button);
        SetField(level, "visual", visual);
        SetField(level, "legs", legs);
        SetField(level, "pen", pen);
        SetField(level, "clock", clock);
        SetField(level, "clockSocket", socket);
        SetField(level, "waves", waves);
        SetField(level, "alarm", alarm);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/Runaway/Level_Runaway.prefab");
    }

    static PhysicsMaterial2D WaterPhysics()
    {
        string path = LevelDir + "/Plant/Water.physicsMaterial2D";
        EnsureFolder(LevelDir + "/Plant");
        PhysicsMaterial2D mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
        if (mat == null)
        {
            mat = new PhysicsMaterial2D("Water");
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.friction = 0f;
        mat.bounciness = 0.05f;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static GameObject BuildPlantLevel()
    {
        var root = new GameObject("Level_Plant");
        PlantLevel level = root.AddComponent<PlantLevel>();
        Art(root.transform, "Garden", "garden_bg", -20, false);

        // fionda a sinistra: la goccia sta nella tasca tra i due rebbi
        Vector2 sling = new Vector2(-1.9f, -2.75f);
        Art(root.transform, "Slingshot", "slingshot", 6, true, sling);
        var pocket = new GameObject("Pocket").transform;
        pocket.SetParent(root.transform, false);
        pocket.localPosition = sling + new Vector2(0f, 1.18f);
        var back = new GameObject("BandBack").transform;
        back.SetParent(root.transform, false);
        back.localPosition = sling + new Vector2(-0.32f, 1.25f);
        var front = new GameObject("BandFront").transform;
        front.SetParent(root.transform, false);
        front.localPosition = sling + new Vector2(0.32f, 1.25f);
        LineRenderer bands = Line(root.transform, "Bands", 0.07f, new Color(0.55f, 0.2f, 0.18f), 9);

        var drops = new List<Rigidbody2D>();
        PhysicsMaterial2D water = WaterPhysics();
        for (int i = 0; i < 10; i++)
        {
            var d = new GameObject("Drop" + i);
            d.transform.SetParent(root.transform, false);
            d.transform.localPosition = pocket.localPosition;
            d.transform.localScale = Vector3.one * 1.6f;
            Art(d.transform, "Art", "drop", 8, false);
            d.AddComponent<CircleCollider2D>().radius = 0.085f;
            Rigidbody2D rb = d.AddComponent<Rigidbody2D>();
            rb.mass = 0.1f;
            rb.sharedMaterial = water;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.bodyType = RigidbodyType2D.Kinematic;
            drops.Add(rb);
        }
        var dots = new List<Transform>();
        for (int i = 0; i < 12; i++)
        {
            Transform dot = Art(root.transform, "Aim" + i, "drop", 7, false);
            dot.localScale = Vector3.one * Mathf.Lerp(0.8f, 0.45f, i / 11f);
            dot.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.75f);
            dot.gameObject.SetActive(false);
            dots.Add(dot);
        }

        // vaso con la piantina appassita, a destra
        var pot = new GameObject("Pot");
        pot.transform.SetParent(root.transform, false);
        pot.transform.localPosition = new Vector3(1.6f, -2.55f, 0);
        Art(pot.transform, "Art", "pot", 6);
        EdgeCollider2D pc = pot.AddComponent<EdgeCollider2D>();
        pc.points = new[] { new Vector2(-0.66f, 0.06f), new Vector2(-0.44f, -0.9f), new Vector2(0.44f, -0.9f), new Vector2(0.66f, 0.06f) };
        pc.edgeRadius = 0.04f;
        var zone = new GameObject("WaterZone");
        zone.transform.SetParent(pot.transform, false);
        BoxCollider2D zc = zone.AddComponent<BoxCollider2D>();
        zc.isTrigger = true;
        zc.size = new Vector2(1.05f, 0.5f);
        zc.offset = new Vector2(0, -0.2f);
        var dry = new GameObject("PlantDry").transform;
        dry.SetParent(pot.transform, false);
        dry.localPosition = new Vector3(0, 0.07f, 0);
        Art(dry, "Art", "plant_dry", 5);
        GameObject bloom = Art(pot.transform, "PlantBloom", "plant_bloom", 5, true, new Vector2(0, 0.07f)).gameObject;

        SmokeEmitter sparkle = Sparks(pot.transform, "Sparkle", new Color(1f, 0.9f, 0.35f));
        sparkle.transform.localPosition = new Vector3(0, 0.4f, 0);
        SmokeEmitter splash = Sparks(root.transform, "Splash", new Color(0.4f, 0.75f, 1f));
        TextMesh counter = Label(root.transform, new Vector2(1.6f, -3.95f), 30, "0 / 5", 0.08f);
        TextMesh ammo = Label(root.transform, new Vector2(-1.75f, -3.95f), 30, "DROPS LEFT: 10", 0.05f);
        SpeechBubble bubble = Bubble(root.transform, new Vector2(0.75f, -0.75f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetArray(level, "drops", drops.ToArray());
        SetField(level, "pocket", pocket);
        SetField(level, "bandBack", back);
        SetField(level, "bandFront", front);
        SetField(level, "bands", bands);
        SetArray(level, "aimDots", dots.ToArray());
        SetField(level, "potZone", zc);
        SetField(level, "plantDry", dry);
        SetField(level, "plantBloom", bloom);
        SetField(level, "sparkle", sparkle);
        SetField(level, "splash", splash);
        SetField(level, "counter", counter);
        SetField(level, "ammo", ammo);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/Plant/Level_Plant.prefab");
    }

    static GameObject BuildShellLevel()
    {
        var root = new GameObject("Level_Shell");
        ShellLevel level = root.AddComponent<ShellLevel>();
        Art(root.transform, "Party", "party_bg", -20, false);

        var mouse = new GameObject("Mouse").transform;
        mouse.SetParent(root.transform, false);
        mouse.localPosition = new Vector3(0, -2.2f + 0.6f * 0.65f, 0);
        mouse.localScale = Vector3.one * 0.65f;
        Art(mouse, "Art", "mouse", 4);
        var boxes = new List<Transform>();
        foreach (float x in new[] { -1.65f, 0f, 1.65f })
        {
            var b = new GameObject("Box").transform;
            b.SetParent(root.transform, false);
            b.localPosition = new Vector3(x, -2.2f, 0);
            Art(b, "Art", "box_cup", 6);
            BoxCollider2D c = b.gameObject.AddComponent<BoxCollider2D>();
            c.size = new Vector2(1.45f, 1.3f);
            c.offset = new Vector2(0, 0.55f);
            boxes.Add(b);
        }
        var tail = new GameObject("Tail").transform;
        tail.SetParent(root.transform, false);
        tail.localScale = Vector3.one * 1.5f;
        Art(tail, "Art", "mouse_tail", 7, false);
        SmokeEmitter hearts = Emitter(root.transform, "Hearts", Vector2.zero, "heart", 5f, 1.2f, 0.9f, 0.4f, 0.7f, Color.white);
        SpeechBubble bubble = Bubble(root.transform, new Vector2(0, 0.6f));
        bubble.transform.localScale = Vector3.one * 0.55f;

        SetArray(level, "boxes", boxes.ToArray());
        SetField(level, "mouse", mouse);
        SetField(level, "tail", tail);
        SetField(level, "hearts", hearts);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/Shell/Level_Shell.prefab");
    }

    static GameObject BuildSkyWireLevel()
    {
        var root = new GameObject("Level_SkyWire");
        SkyWireLevel level = root.AddComponent<SkyWireLevel>();
        Art(root.transform, "Room", "night_bg", -20, false);

        var bomb = new GameObject("Bomb").transform;
        bomb.SetParent(root.transform, false);
        Transform dyn = Art(bomb, "Dynamite", "dynamite", 5, true, new Vector2(0, -3.0f));
        dyn.localScale = Vector3.one * 0.9f;
        Art(bomb, "Timer", "timer_box", 6, true, new Vector2(0, 0.6f));
        TextMesh display = Label(bomb, new Vector2(0, 0.62f), 7, "00:15", 0.09f);

        var cutters = new List<SwipeCutter>();
        var tops = new List<Transform>();
        var bottoms = new List<Transform>();
        var topArt = new List<SpriteRenderer>();
        var bottomArt = new List<SpriteRenderer>();
        foreach (float x in new[] { -0.75f, 0f, 0.75f })
        {
            var top = new GameObject("WireTop").transform;
            top.SetParent(bomb, false);
            top.localPosition = new Vector3(x, 0.08f, 0);
            topArt.Add(Bar(top, "Art", new Vector2(0, -0.52f), new Vector2(0.13f, 1.04f), Color.white, 4));
            var bottom = new GameObject("WireBottom").transform;
            bottom.SetParent(bomb, false);
            bottom.localPosition = new Vector3(x, -2.05f, 0);
            bottomArt.Add(Bar(bottom, "Art", new Vector2(0, 0.55f), new Vector2(0.13f, 1.1f), Color.white, 4));
            tops.Add(top);
            bottoms.Add(bottom);
            var cut = new GameObject("Cutter");
            cut.transform.SetParent(root.transform, false);
            cut.transform.localPosition = new Vector3(x, -0.95f, 0);
            SwipeCutter c = cut.AddComponent<SwipeCutter>();
            c.radius = 0.3f;
            c.minChord = 1.1f;
            c.trailMaterial = StrokeMaterial();
            cutters.Add(c);
        }

        var note = new GameObject("Note").transform;
        note.SetParent(root.transform, false);
        note.localPosition = new Vector3(-1.45f, 2.45f, 0);
        note.localRotation = Quaternion.Euler(0, 0, 4f);
        Art(note, "Art", "sticky_note", 3);
        TextMesh noteText = Label(note, new Vector2(0, -0.05f), 4, "CUT THE WIRE\nTHE COLOR OF\nTHE SKY!", 0.036f);
        noteText.lineSpacing = 0.85f;

        GameObject boom = Art(root.transform, "Boom", "boom", 50, true, new Vector2(0, -1.4f)).gameObject;
        SmokeEmitter sparks = Sparks(root.transform, "Sparks", new Color(1f, 0.85f, 0.3f));
        SpeechBubble bubble = Bubble(root.transform, new Vector2(-1.2f, 0.9f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetArray(level, "cutters", cutters.ToArray());
        SetArray(level, "tops", tops.ToArray());
        SetArray(level, "bottoms", bottoms.ToArray());
        SetArray(level, "topArt", topArt.ToArray());
        SetArray(level, "bottomArt", bottomArt.ToArray());
        SetField(level, "display", display);
        SetField(level, "bomb", bomb.gameObject);
        SetField(level, "boom", boom);
        SetField(level, "sparks", sparks);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/SkyWire/Level_SkyWire.prefab");
    }

    static GameObject BuildSunnyLevel()
    {
        var root = new GameObject("Level_Sunny");
        SunnyLevel level = root.AddComponent<SunnyLevel>();
        Art(root.transform, "Landscape", "snow_bg", -20, false);

        // il pupazzo già fatto (stesso del livello 3), un po' più piccolo
        Transform snowman = SnowmanFigure(root.transform, new Vector2(-0.35f, -3.65f), out Transform basePart, out Transform armLeft,
            out Transform armRight, out Transform headPart, out Transform hatPart, out Transform scarf, out List<SpriteRenderer> bodies);
        snowman.localScale = Vector3.one * 0.74f;
        float hp = DoodleSprites.SnowHeadPivot;
        Transform eyeL = Art(headPart, "EyeL", "coal", 8, true, new Vector2(-0.62f, 4.1f - hp));
        Transform eyeR = Art(headPart, "EyeR", "coal", 8, true, new Vector2(0.22f, 4.1f - hp));
        eyeL.localScale = eyeR.localScale = Vector3.one * 1.4f;
        Transform nose = Art(headPart, "Nose", "carrot", 9, true, new Vector2(-0.45f, 3.62f - hp));
        nose.localScale = Vector3.one * 1.3f;
        SpriteRenderer smile = Art(headPart, "Smile", "mouth_smile", 8, false, new Vector2(-0.3f, 3.27f - hp)).GetComponent<SpriteRenderer>();
        SpriteRenderer mouthO = Art(headPart, "MouthO", "mouth_o", 8, false, new Vector2(-0.3f, 3.22f - hp)).GetComponent<SpriteRenderer>();
        mouthO.transform.localScale = Vector3.one * 1.4f;
        SnowmanMelter melter = AddMelter(snowman, bodies.ToArray());
        SmokeEmitter sweat = Emitter(headPart, "Sweat", new Vector2(-0.1f, 4.4f - hp), "sweat", 4f, 0.8f, -1.2f, 0.8f, 1.2f, new Color(0.75f, 0.9f, 1f), 12);
        SmokeEmitter smoke = Emitter(headPart, "Smoke", new Vector2(-0.1f, 3.9f - hp), "puff", 7f, 1.6f, 0.9f, 0.55f, 0.6f, new Color(0.85f, 0.86f, 0.9f, 0.95f));

        // terra innevata in primo piano: sole e luna passano DIETRO le colline
        Art(root.transform, "Foreground", "snow_fg", -1, false);
        var sun = new GameObject("Sun").transform;
        sun.SetParent(root.transform, false);
        sun.localPosition = new Vector3(1.6f, 2.85f, 0);
        Transform sunArt = Art(sun, "Art", "sun", -2);
        var moon = new GameObject("Moon").transform;
        moon.SetParent(root.transform, false);
        moon.localPosition = new Vector3(-0.4f, 2.9f, 0);
        Art(moon, "Art", "moon", -2);
        sunArt.gameObject.AddComponent<Pulse>();
        Vector2 head = new Vector2(-0.35f - 0.1f * 0.74f, -3.65f + 3.7f * 0.74f);
        SpriteRenderer beam = Art(sun, "Beam", "light_cone", 9, false).GetComponent<SpriteRenderer>();
        beam.color = new Color(1f, 0.85f, 0.4f, 0.35f);
        Vector2 dir = head - (Vector2)sun.localPosition;
        beam.transform.localRotation = Quaternion.FromToRotation(Vector3.down, dir);
        beam.transform.localScale = new Vector3(0.45f, dir.magnitude / 5.2f, 1f);

        var cloud = new GameObject("Cloud");
        cloud.transform.SetParent(root.transform, false);
        cloud.transform.localPosition = new Vector3(-1.7f, 2.4f, 0);
        Art(cloud.transform, "Art", "cloud", 12);
        cloud.AddComponent<BoxCollider2D>().size = new Vector2(2.5f, 1.4f);
        Draggable2D drag = cloud.AddComponent<Draggable2D>();
        drag.itemId = "cloud";
        drag.returnOnMiss = false;

        SpriteRenderer sky = Bar(root.transform, "Dusk", Vector2.zero, new Vector2(12f, 16f), new Color(0.1f, 0.1f, 0.3f, 0f), 15);   // tramonto, poi notte
        TextMesh countdown = Label(root.transform, new Vector2(0, 3.3f), 30, "", 0.06f);
        SpeechBubble bubble = Bubble(root.transform, new Vector2(1.1f, 0.4f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetField(level, "sun", sun);
        SetField(level, "cloud", drag);
        SetField(level, "sunbeam", beam);
        SetField(level, "skyTint", sky);
        SetField(level, "melter", melter);
        SetField(level, "moon", moon);
        SetField(level, "meltdown", AddMeltdown(root, snowman, melter, hatPart, armLeft, armRight, scarf, mouthO, null, bubble, smoke));
        SetArray(level, "eyes", new Object[] { eyeL, eyeR });
        SetArray(level, "attached", new Object[] { nose });
        SetField(level, "mouthSmile", smile);
        SetField(level, "mouthO", mouthO);
        SetField(level, "sweat", sweat);
        SetField(level, "countdown", countdown);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/Sunny/Level_Sunny.prefab");
    }

    static GameObject BuildRopeLevel()
    {
        var root = new GameObject("Level_Rope");
        RopeLevel level = root.AddComponent<RopeLevel>();
        Art(root.transform, "Kitchen", "kitchen_bg", -20, false);
        var floor = new GameObject("Counter");
        floor.transform.SetParent(root.transform, false);
        floor.AddComponent<EdgeCollider2D>().points = new[] { new Vector2(-3f, -1.94f), new Vector2(3f, -1.94f) };

        Vector2 start = new Vector2(-0.4f, 0.9f);
        var cookieGo = new GameObject("Cookie");
        cookieGo.transform.SetParent(root.transform, false);
        cookieGo.transform.localPosition = start;
        Art(cookieGo.transform, "Art", "cookie", 6);
        cookieGo.AddComponent<CircleCollider2D>().radius = 0.22f;
        Rigidbody2D cookie = cookieGo.AddComponent<Rigidbody2D>();
        cookie.interpolation = RigidbodyInterpolation2D.Interpolate;
        cookie.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        cookie.sharedMaterial = CookiePhysics();

        var anchors = new List<Transform>();
        var ropes = new List<LineRenderer>();
        foreach (Vector2 a in new[] { new Vector2(-2.2f, 3.0f), new Vector2(1.1f, 3.3f) })
        {
            var pin = new GameObject("Pin").transform;
            pin.SetParent(root.transform, false);
            pin.localPosition = a;
            SpriteRenderer head = Art(pin, "Art", "drop", 7, false).GetComponent<SpriteRenderer>();
            head.color = new Color(0.55f, 0.58f, 0.66f);
            head.transform.localScale = Vector3.one * 1.6f;
            anchors.Add(pin);
            DistanceJoint2D j = cookieGo.AddComponent<DistanceJoint2D>();
            j.autoConfigureConnectedAnchor = false;
            j.autoConfigureDistance = false;
            j.connectedAnchor = a;
            j.distance = Vector2.Distance(a, start);
            j.maxDistanceOnly = true;
            LineRenderer rope = Line(root.transform, "Rope", 0.05f, new Color(0.55f, 0.36f, 0.22f), 5);
            rope.SetPosition(0, a);
            rope.SetPosition(1, start);
            ropes.Add(rope);
        }

        var mouse = new GameObject("Mouse").transform;
        mouse.SetParent(root.transform, false);
        mouse.localPosition = new Vector3(2.0f, -1.5f, 0);
        Art(mouse, "Art", "mouse", 4);
        SmokeEmitter hearts = Emitter(mouse, "Hearts", new Vector2(0, 0.6f), "heart", 5f, 1.2f, 0.9f, 0.4f, 0.7f, Color.white);
        SmokeEmitter crumbs = Sparks(root.transform, "Crumbs", new Color(0.78f, 0.55f, 0.3f));
        crumbs.riseSpeed = -0.5f;
        LineRenderer trail = Line(root.transform, "Trail", 0.1f, new Color(1f, 1f, 1f, 0.9f), 50);
        trail.positionCount = 0;
        SpeechBubble bubble = Bubble(root.transform, new Vector2(1.75f, -0.15f));
        bubble.transform.localScale = Vector3.one * 0.45f;

        SetField(level, "cookie", cookie);
        SetArray(level, "anchors", anchors.ToArray());
        SetArray(level, "ropes", ropes.ToArray());
        SetField(level, "mouse", mouse);
        SetField(level, "hearts", hearts);
        SetField(level, "crumbs", crumbs);
        SetField(level, "bubble", bubble);
        SetField(level, "trail", trail);
        return SavePrefab(root, LevelDir + "/Rope/Level_Rope.prefab");
    }

    static GameObject BuildPizzaLevel()
    {
        var root = new GameObject("Level_Pizza");
        PizzaLevel level = root.AddComponent<PizzaLevel>();
        Art(root.transform, "Party", "party_bg", -20, false);

        var pizza = new GameObject("Pizza").transform;
        pizza.SetParent(root.transform, false);
        pizza.localPosition = new Vector3(0, -0.2f, 0);
        GameObject whole = Art(pizza, "Whole", "pizza", 6).gameObject;
        LineRenderer cutMark = Line(root.transform, "CutMark", 0.05f, new Color(0.45f, 0.18f, 0.12f), 7);

        var slices = new List<Transform>();
        float c = 4f * 1.1f / (3f * Mathf.PI) / Mathf.Sqrt(2f);   // baricentro dello spicchio, componenti x e y
        for (int i = 0; i < 4; i++)
        {
            var s = new GameObject("Slice" + i);
            s.transform.SetParent(root.transform, false);
            Art(s.transform, "Art", "pizza_slice", 8 + i, false, new Vector2(-c, -c));
            s.AddComponent<CircleCollider2D>().radius = 0.5f;
            Draggable2D d = s.AddComponent<Draggable2D>();
            d.itemId = "slice";
            s.SetActive(false);   // compaiono dopo i tagli (e il punto di ritorno si fissa lì)
            slices.Add(s.transform);
        }

        var plates = new List<DropSlot>();
        foreach (Vector2 m in new[] { new Vector2(-2.0f, 2.0f), new Vector2(2.0f, 2.0f), new Vector2(-2.0f, -2.85f), new Vector2(2.0f, -2.85f) })
        {
            var mouse = new GameObject("Mouse").transform;
            mouse.SetParent(root.transform, false);
            mouse.localPosition = m;
            mouse.localScale = Vector3.one * 0.8f;
            Art(mouse, "Art", "mouse", 4);
            var plate = new GameObject("Plate");
            plate.transform.SetParent(root.transform, false);
            plate.transform.localPosition = m + new Vector2(0, -0.35f);
            plate.AddComponent<CircleCollider2D>().radius = 0.75f;
            DropSlot slot = plate.AddComponent<DropSlot>();
            slot.acceptedId = "slice";
            plates.Add(slot);
        }
        SmokeEmitter hearts = Emitter(root.transform, "Hearts", new Vector2(0, 0.5f), "heart", 8f, 1.4f, 1f, 2.2f, 0.8f, Color.white);
        LineRenderer trail = Line(root.transform, "Trail", 0.1f, new Color(1f, 1f, 1f, 0.9f), 50);
        trail.positionCount = 0;
        SpeechBubble bubble = Bubble(root.transform, new Vector2(0, 1.85f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetField(level, "pizza", pizza);
        SetField(level, "whole", whole);
        SetField(level, "cutMark", cutMark);
        SetArray(level, "slices", slices.ToArray());
        SetArray(level, "plates", plates.ToArray());
        SetField(level, "hearts", hearts);
        SetField(level, "bubble", bubble);
        SetField(level, "trail", trail);
        return SavePrefab(root, LevelDir + "/Pizza/Level_Pizza.prefab");
    }

    // ---------- Livelli 32-35 ----------

    static GameObject BuildPizzaChefLevel()
    {
        var root = new GameObject("Level_PizzaChef");
        PizzaChefLevel level = root.AddComponent<PizzaChefLevel>();
        Art(root.transform, "Kitchen", "kitchen_bg", -20, false);
        Transform board = Art(root.transform, "Chalkboard", "chalkboard", -5, true, new Vector2(-1.35f, 2.4f));
        board.localScale = Vector3.one * 0.95f;

        // forno su una mensola
        Bar(root.transform, "OvenShelf", new Vector2(1.5f, 1.0f), new Vector2(2.6f, 0.16f), new Color(0.55f, 0.36f, 0.24f), -4);
        var oven = new GameObject("Oven").transform;
        oven.SetParent(root.transform, false);
        oven.localPosition = new Vector3(1.5f, 1.05f, 0);
        oven.localScale = Vector3.one * 0.95f;
        Art(oven, "Art", "oven", -3);
        SpriteRenderer ovenGlow = Art(oven, "Glow", "glow", -2, false, new Vector2(0, 0.7f)).GetComponent<SpriteRenderer>();
        ovenGlow.transform.localScale = Vector3.one * 2.2f;
        var mouth = new GameObject("Mouth");
        mouth.transform.SetParent(oven, false);
        mouth.transform.localPosition = new Vector3(0, 0.75f, 0);
        BoxCollider2D mouthCol = mouth.AddComponent<BoxCollider2D>();
        mouthCol.isTrigger = true;
        mouthCol.size = new Vector2(2.3f, 2.1f);   // tutto il forno: basta avvicinarci la pizza
        DropSlot ovenSlot = mouth.AddComponent<DropSlot>();
        ovenSlot.acceptedId = "pizza";
        ovenSlot.snap = false;
        TapButton ovenButton = mouth.AddComponent<TapButton>();
        ovenButton.visual = oven;
        ovenButton.pressedScaleY = 0.95f;
        SmokeEmitter ovenSmoke = Emitter(oven, "Smoke", new Vector2(0.62f, 1.9f), "puff", 6f, 1.6f, 0.9f, 0.15f, 0.4f, new Color(0.8f, 0.8f, 0.85f, 0.8f));

        // pizza: pasta + strati (accesi man mano che si aggiungono gli ingredienti)
        string[] ids = { "tomato", "mozzarella", "mushroom", "olive", "chili", "pineapple" };
        var pizza = new GameObject("Pizza").transform;
        pizza.SetParent(root.transform, false);
        pizza.localPosition = new Vector3(0, -0.4f, 0);
        SpriteRenderer dough = Art(pizza, "Dough", "dough", 3).GetComponent<SpriteRenderer>();
        pizza.gameObject.AddComponent<CircleCollider2D>().radius = 1.1f;
        Draggable2D pizzaDrag = pizza.gameObject.AddComponent<Draggable2D>();
        pizzaDrag.itemId = "pizza";
        var wholeLayers = new List<GameObject>();
        SpriteRenderer cheeseWhole = null;
        float[][] bitsWhole = { new[] { 0.45f, 0.3f }, new[] { -0.4f, 0.45f }, new[] { -0.55f, -0.25f }, new[] { 0.2f, -0.55f }, new[] { 0.6f, -0.2f }, new[] { -0.05f, 0.05f }, new[] { 0.05f, 0.65f } };
        for (int k = 0; k < ids.Length; k++)
        {
            var layer = new GameObject("Layer_" + ids[k]).transform;
            layer.SetParent(pizza, false);
            if (ids[k] == "tomato") Art(layer, "Sauce", "sauce", 4, false);
            else if (ids[k] == "mozzarella") cheeseWhole = Art(layer, "Cheese", "cheese", 5, false).GetComponent<SpriteRenderer>();
            else
                for (int b = 0; b < bitsWhole.Length; b++)
                {
                    float[] q = bitsWhole[(b + k * 2) % bitsWhole.Length];
                    Transform bit = Art(layer, "Bit", "top_" + ids[k], 6 + k, false, new Vector2(q[0] + k * 0.04f, q[1] - k * 0.05f));
                    bit.localRotation = Quaternion.Euler(0, 0, b * 47f + k * 13f);
                }
            wholeLayers.Add(layer.gameObject);
        }

        // i quattro spicchi: stessi strati ritagliati a quarto (pivot nel baricentro, per afferrarli bene)
        float c = 4f * 1.1f / (3f * Mathf.PI) / Mathf.Sqrt(2f);
        var slices = new List<Transform>();
        for (int i = 0; i < 4; i++)
        {
            var sl = new GameObject("Slice" + i);
            sl.transform.SetParent(root.transform, false);
            int o = 12 + i * 8;
            Art(sl.transform, "Dough", "dough_q", o, false, new Vector2(-c, -c));
            for (int k = 0; k < ids.Length; k++)
            {
                var layer = new GameObject("Layer_" + ids[k]).transform;
                layer.SetParent(sl.transform, false);
                if (ids[k] == "tomato") Art(layer, "Sauce", "sauce_q", o + 1, false, new Vector2(-c, -c));
                else if (ids[k] == "mozzarella") Art(layer, "Cheese", "cheese_q", o + 2, false, new Vector2(-c, -c));
                else
                    foreach (float[] q in new[] { new[] { 25f + k * 6f, 0.45f }, new[] { 62f - k * 5f, 0.72f } })
                    {
                        float a = q[0] * Mathf.Deg2Rad;
                        Art(layer, "Bit", "top_" + ids[k], o + 3, false, new Vector2(Mathf.Cos(a) * q[1] - c, Mathf.Sin(a) * q[1] - c));
                    }
                layer.gameObject.SetActive(false);
            }
            sl.AddComponent<CircleCollider2D>().radius = 0.5f;
            Draggable2D d = sl.AddComponent<Draggable2D>();
            d.itemId = "slice";
            sl.SetActive(false);
            slices.Add(sl.transform);
        }

        // ciotole degli ingredienti
        var bowls = new List<Draggable2D>();
        for (int k = 0; k < ids.Length; k++)
        {
            var bowl = new GameObject("Bowl_" + ids[k]);
            bowl.transform.SetParent(root.transform, false);
            bowl.transform.localPosition = new Vector3(-2.3f + k * 0.92f, -3.25f, 0);
            Art(bowl.transform, "Art", "ing_bowl", 8);
            // contenuto bello pieno, che si riconosce a colpo d'occhio, e il nome sotto
            if (ids[k] == "tomato") Art(bowl.transform, "Content", "sauce", 9, false, new Vector2(0, 0.08f)).localScale = new Vector3(0.4f, 0.12f, 1f);
            else if (ids[k] == "mozzarella") Art(bowl.transform, "Content", "cheese", 9, false, new Vector2(0, 0.14f)).localScale = Vector3.one * 0.42f;
            else
                foreach (float[] q in new[] { new[] { -0.18f, 0.1f }, new[] { 0.16f, 0.12f }, new[] { -0.01f, 0.22f } })
                    Art(bowl.transform, "Content", "top_" + ids[k], 9, false, new Vector2(q[0], q[1])).localScale = Vector3.one * 1.35f;
            string[] names = { "TOMATO", "MOZZARELLA", "MUSHROOMS", "OLIVES", "CHILI", "PINEAPPLE" };
            TextMesh name = Label(bowl.transform, new Vector2(0, -0.45f), 9, names[k], 0.03f);
            name.gameObject.AddComponent<TextMeshFit>().maxWidth = 0.86f;
            bowl.AddComponent<BoxCollider2D>().size = new Vector2(0.88f, 0.7f);
            Draggable2D d = bowl.AddComponent<Draggable2D>();
            d.itemId = ids[k];
            bowls.Add(d);
        }

        // i quattro topolini (arrivano a pizza cotta) con la faccia "piccante" pronta
        var mice = new GameObject("Mice").transform;
        mice.SetParent(root.transform, false);
        var plates = new List<DropSlot>();
        var miceArt = new List<SpriteRenderer>();
        var spicy = new List<GameObject>();
        foreach (Vector2 m in new[] { new Vector2(-2.15f, 0.55f), new Vector2(2.15f, -0.5f), new Vector2(-2.15f, -2.45f), new Vector2(2.15f, -2.45f) })
        {
            var mouse = new GameObject("Mouse").transform;
            mouse.SetParent(mice, false);
            mouse.localPosition = m;
            mouse.localScale = Vector3.one * 0.75f;
            miceArt.Add(Art(mouse, "Art", "mouse", 20).GetComponent<SpriteRenderer>());
            var face = new GameObject("Spicy").transform;
            face.SetParent(mouse, false);
            foreach (float x in new[] { -0.2f, 0.02f })
            {
                Transform f = Art(face, "FireEye", "flame", 22, false, new Vector2(x, 0.3f));
                f.localScale = Vector3.one * 1.5f;
                f.gameObject.AddComponent<Pulse>();
            }
            Art(face, "Tongue", "tongue", 22, false, new Vector2(-0.17f, 0.12f)).localScale = Vector3.one * 1.3f;
            Art(face, "Anger", "anger", 22, true, new Vector2(0.3f, 0.65f));
            spicy.Add(face.gameObject);
            var plate = new GameObject("Plate");
            plate.transform.SetParent(mice, false);
            plate.transform.localPosition = m + new Vector2(0, -0.25f);
            plate.AddComponent<CircleCollider2D>().radius = 0.75f;
            DropSlot slot = plate.AddComponent<DropSlot>();
            slot.acceptedId = "slice";
            plates.Add(slot);
        }

        LineRenderer cutMark = Line(root.transform, "CutMark", 0.05f, new Color(0.45f, 0.18f, 0.12f), 11);
        LineRenderer trail = Line(root.transform, "Trail", 0.1f, new Color(1f, 1f, 1f, 0.9f), 50);
        trail.positionCount = 0;
        SmokeEmitter hearts = Emitter(root.transform, "Hearts", new Vector2(0, 0.2f), "heart", 8f, 1.4f, 1f, 2.2f, 0.8f, Color.white);
        SmokeEmitter steam = Emitter(root.transform, "Steam", new Vector2(0, -0.4f), "puff", 6f, 1.3f, 0.8f, 0.6f, 0.4f, new Color(1f, 1f, 1f, 0.6f));
        SpeechBubble bubble = Bubble(root.transform, new Vector2(0.2f, 1.0f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetArray(level, "bowls", bowls.ToArray());
        var idField = new SerializedObject(level);
        SerializedProperty idsProp = idField.FindProperty("layerIds");
        idsProp.arraySize = ids.Length;
        for (int k = 0; k < ids.Length; k++) idsProp.GetArrayElementAtIndex(k).stringValue = ids[k];
        idField.ApplyModifiedPropertiesWithoutUndo();
        SetArray(level, "wholeLayers", wholeLayers.ToArray());
        SetArray(level, "bakeTint", new Object[] { dough, cheeseWhole });
        SetField(level, "pizza", pizza);
        SetField(level, "ovenMouth", mouthCol);
        SetField(level, "ovenSlot", ovenSlot);
        SetField(level, "ovenButton", ovenButton);
        SetField(level, "ovenGlow", ovenGlow);
        SetField(level, "ovenSmoke", ovenSmoke);
        SetField(level, "cutMark", cutMark);
        SetField(level, "trail", trail);
        SetArray(level, "slices", slices.ToArray());
        SetArray(level, "plates", plates.ToArray());
        SetField(level, "mice", mice.gameObject);
        SetArray(level, "miceArt", miceArt.ToArray());
        SetArray(level, "spicyFaces", spicy.ToArray());
        SetField(level, "hearts", hearts);
        SetField(level, "steam", steam);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/PizzaChef/Level_PizzaChef.prefab");
    }

    static GameObject BuildBalloonLevel()
    {
        var root = new GameObject("Level_Balloon");
        BalloonLevel level = root.AddComponent<BalloonLevel>();
        Art(root.transform, "Party", "party_bg", -20, false);

        var holder = new GameObject("BalloonSpot").transform;
        holder.SetParent(root.transform, false);
        holder.localPosition = new Vector3(-0.5f, -2.0f, 0);
        holder.localScale = Vector3.one * 1.5f;
        SpriteRenderer outline = Art(holder, "Outline", "balloon_outline", 2, false).GetComponent<SpriteRenderer>();
        var balloon = new GameObject("Balloon").transform;
        balloon.SetParent(holder, false);
        Art(balloon, "Art", "balloon", 5);

        var pump = new GameObject("Pump");
        pump.transform.SetParent(root.transform, false);
        pump.transform.localPosition = new Vector3(1.75f, -3.45f, 0);
        Art(pump.transform, "Art", "pump", 6);
        var handle = new GameObject("Handle").transform;
        handle.SetParent(pump.transform, false);
        handle.localPosition = new Vector3(0, 1.55f, 0);
        Art(handle, "Art", "pump_handle", 6, false);
        BoxCollider2D pc = pump.AddComponent<BoxCollider2D>();
        pc.size = new Vector2(1.3f, 2.2f);
        pc.offset = new Vector2(0, 0.9f);
        TapButton pumpButton = pump.AddComponent<TapButton>();
        pumpButton.pressedScaleY = 1f;
        LineRenderer hose = Line(root.transform, "Hose", 0.06f, new Color(0.17f, 0.13f, 0.2f), 4);
        hose.positionCount = 3;
        hose.SetPositions(new[] { new Vector3(1.75f, -3.35f, 0), new Vector3(0.6f, -3.7f, 0), new Vector3(-0.5f, -2.05f, 0) });
        TextMesh counter = Label(root.transform, new Vector2(1.75f, -0.8f), 30, "", 0.12f);
        counter.color = new Color(0.17f, 0.13f, 0.2f);

        GameObject boom = Art(root.transform, "Boom", "boom", 50, true, new Vector2(-0.5f, 0.0f)).gameObject;
        SmokeEmitter scraps = Emitter(root.transform, "Scraps", Vector2.zero, "balloon_scrap", 40f, 0.9f, -1.2f, 1.2f, 1.6f, Color.white, 45);
        scraps.spinRange = 500f;
        scraps.sideDrift = 2f;
        scraps.growth = 0f;
        SpeechBubble bubble = Bubble(root.transform, new Vector2(1.2f, 1.9f));
        bubble.transform.localScale = Vector3.one * 0.55f;

        SetField(level, "balloon", balloon);
        SetField(level, "outline", outline);
        SetField(level, "pump", pumpButton);
        SetField(level, "pumpHandle", handle);
        SetField(level, "scraps", scraps);
        SetField(level, "boom", boom);
        SetField(level, "counter", counter);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/Balloon/Level_Balloon.prefab");
    }

    static GameObject BuildCatEyesLevel()
    {
        var root = new GameObject("Level_CatEyes");
        CatEyesLevel level = root.AddComponent<CatEyesLevel>();
        // parete liscia con le tane (niente quadri o lampade a confondere dove guardare)
        Bar(root.transform, "Wall", new Vector2(0, 1f), new Vector2(12f, 14f), new Color(1f, 0.93f, 0.8f), -20);
        Bar(root.transform, "Skirting", new Vector2(0, -3.18f), new Vector2(12f, 0.12f), new Color(0.55f, 0.36f, 0.24f), -19);
        Bar(root.transform, "Floor", new Vector2(0, -5.3f), new Vector2(12f, 4.2f), new Color(0.78f, 0.55f, 0.36f), -19);
        var face = new GameObject("CatFace").transform;
        face.SetParent(root.transform, false);
        face.localPosition = new Vector3(0, 0.15f, 0);
        Art(face, "Art", "cat_big_face", 2);
        var pupils = new List<Transform>();
        foreach (float x in new[] { -0.58f, 0.58f })
        {
            var p = new GameObject("Pupil").transform;
            p.SetParent(face, false);
            p.localPosition = new Vector3(x, 0.25f, 0);
            Art(p, "Art", "cat_pupil", 3, false);
            pupils.Add(p);
        }
        var spots = new List<Transform>();
        foreach (Vector2 v in new[] { new Vector2(-2.1f, 2.75f), new Vector2(2.1f, 2.75f), new Vector2(-2.35f, 0.45f), new Vector2(2.35f, 0.45f), new Vector2(-1.8f, -2.8f), new Vector2(1.8f, -2.8f) })
        {
            Transform hole = Art(root.transform, "Hole", "mouse_hole", -6, false, v);
            hole.localScale = Vector3.one * 0.85f;
            spots.Add(hole);
        }
        var mouse = new GameObject("Mouse").transform;
        mouse.SetParent(root.transform, false);
        mouse.localScale = Vector3.one * 0.5f;
        Art(mouse, "Art", "mouse", -4);

        Bar(root.transform, "BarBack", new Vector2(0, -3.75f), new Vector2(3.2f, 0.28f), new Color(0.17f, 0.13f, 0.2f), 20);
        var fill = new GameObject("Progress").transform;
        fill.SetParent(root.transform, false);
        fill.localPosition = new Vector3(-1.55f, -3.75f, 0);
        Bar(fill, "Bar", new Vector2(1.55f, 0f), new Vector2(3.1f, 0.2f), new Color(0.36f, 1f, 0.45f), 21);

        SpeechBubble catBubble = Bubble(root.transform, new Vector2(1.3f, 2.1f));
        catBubble.transform.localScale = Vector3.one * 0.5f;
        catBubble.catVoice = true;   // il gatto miagola quando parla
        SpeechBubble mouseBubble = Bubble(root.transform, Vector2.zero);
        mouseBubble.transform.localScale = Vector3.one * 0.4f;

        SetArray(level, "pupils", pupils.ToArray());
        SetField(level, "face", face);
        SetField(level, "mouse", mouse);
        SetArray(level, "spots", spots.ToArray());
        SetField(level, "progress", fill);
        SetField(level, "catBubble", catBubble);
        SetField(level, "mouseBubble", mouseBubble);
        return SavePrefab(root, LevelDir + "/CatEyes/Level_CatEyes.prefab");
    }

    static GameObject BuildPancakeLevel()
    {
        var root = new GameObject("Level_Pancake");
        PancakeLevel level = root.AddComponent<PancakeLevel>();
        Art(root.transform, "Kitchen", "kitchen_bg", -20, false);
        var pan = new GameObject("Pan").transform;
        pan.SetParent(root.transform, false);
        pan.localPosition = new Vector3(0, -2.0f, 0);
        Art(pan, "Art", "pan", 4);
        var pancake = new GameObject("Pancake").transform;
        pancake.SetParent(root.transform, false);
        pancake.localPosition = new Vector3(0, -1.8f, 0);
        GameObject raw = Art(pancake, "Raw", "pancake_raw", 5, false).gameObject;
        GameObject cooked = Art(pancake, "Cooked", "pancake_cooked", 5, false).gameObject;
        SmokeEmitter steam = Emitter(pancake, "Steam", new Vector2(0, 0.15f), "puff", 4f, 1.3f, 0.8f, 0.5f, 0.35f, new Color(1f, 1f, 1f, 0.6f));
        TextMesh counter = Label(root.transform, new Vector2(0, 2.95f), 30, "0 / 3", 0.1f);
        SpeechBubble bubble = Bubble(root.transform, new Vector2(1.0f, 0.6f));
        bubble.transform.localScale = Vector3.one * 0.55f;

        SetField(level, "pan", pan);
        SetField(level, "pancake", pancake);
        SetField(level, "rawUp", raw);
        SetField(level, "cookedUp", cooked);
        SetField(level, "steam", steam);
        SetField(level, "counter", counter);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/Pancake/Level_Pancake.prefab");
    }

    // ---------- Livelli 36-40 ----------

    static GameObject BuildWaspMazeLevel()
    {
        var root = new GameObject("Level_WaspMaze");
        WaspMazeLevel level = root.AddComponent<WaspMazeLevel>();
        TiltGravity tilt = root.AddComponent<TiltGravity>();
        tilt.strength = 34f;
        tilt.response = 0.5f;
        AudioSource buzz = root.AddComponent<AudioSource>();
        buzz.playOnAwake = false;
        Art(root.transform, "Night", "maze_bg", -20, false);

        var maze = new GameObject("Maze").transform;
        maze.SetParent(root.transform, false);
        maze.localPosition = new Vector3(0, -0.4f, 0);
        Art(maze, "Neon", "maze_wasp", 5, false);
        SegWalls(maze, DoodleSprites.WaspMaze);
        MazeGoal(maze, new Vector2(-2.2f, 2.7f));
        Rigidbody2D ball = MazeBall(maze, new Vector2(-1.0f, -3.0f));

        // la vespa parte dall'angolo alle spalle della sfera
        var wasp = new GameObject("Wasp").transform;
        wasp.SetParent(maze, false);
        wasp.localPosition = new Vector3(-2.3f, -2.25f, 0);
        wasp.localScale = Vector3.one * 0.6f;
        SpriteRenderer body = Art(wasp, "Body", "wasp_furious", 12).GetComponent<SpriteRenderer>();
        foreach (var w in new[] { new { x = -0.02f, order = 11, rot = 15f }, new { x = 0.08f, order = 13, rot = -10f } })
        {
            Transform wing = Art(wasp, "Wing", "wasp_wing", w.order, false, new Vector2(w.x, 0.12f));
            wing.localRotation = Quaternion.Euler(0, 0, w.rot);
            wing.gameObject.AddComponent<WingFlap>();
        }

        SpeechBubble bubble = Bubble(root.transform, new Vector2(1.5f, -4.45f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetField(level, "ball", ball);
        SetField(level, "wasp", wasp);
        SetField(level, "waspBody", body);
        SetField(level, "buzz", buzz);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/WaspMaze/Level_WaspMaze.prefab");
    }

    static GameObject BuildLampSafeLevel()
    {
        var root = new GameObject("Level_LampSafe");
        LampSafeLevel level = root.AddComponent<LampSafeLevel>();
        SpriteRenderer wall = Art(root.transform, "Wall", "plain", -20, false).GetComponent<SpriteRenderer>();
        wall.transform.localScale = new Vector3(12f, 16f, 1f);
        SpriteRenderer floor = Art(root.transform, "Floor", "plain", -19, false, new Vector2(0, -5.0f)).GetComponent<SpriteRenderer>();
        floor.transform.localScale = new Vector3(12f, 4.6f, 1f);
        floor.color = new Color(0.09f, 0.07f, 0.1f);

        GameObject red = Lamp(root.transform, "RedLamp", new Vector2(-1.35f, 2.9f), new Color(1f, 0.25f, 0.25f));
        GameObject blue = Lamp(root.transform, "BlueLamp", new Vector2(1.35f, 2.9f), new Color(0.3f, 0.45f, 1f));

        var digits = new List<TextMesh>();
        for (int i = 0; i < 4; i++) digits.Add(Label(root.transform, new Vector2(-1.65f + i * 1.1f, 0.7f), 3, "0", 0.17f));

        // cassaforte sotto le cifre
        var safe = new GameObject("Safe").transform;
        safe.SetParent(root.transform, false);
        safe.localPosition = new Vector3(0, -1.55f, 0);
        safe.localScale = Vector3.one * 0.7f;
        GameObject closed = Art(safe, "Closed", "safe", 1).gameObject;
        GameObject open = Art(safe, "Open", "safe_open", 1).gameObject;
        BoxCollider2D safeArea = safe.gameObject.AddComponent<BoxCollider2D>();
        safeArea.size = new Vector2(1.9f, 2.1f);
        SmokeEmitter confetti = Emitter(root.transform, "Confetti", new Vector2(0, -1.2f), "confetti", 30f, 1.3f, 1.4f, 0.8f, 1.1f, new Color(1f, 0.85f, 0.35f), 45);

        TapButton redSwitch = SwitchButton(root.transform, "RedSwitch", "big_button", new Vector2(-1.55f, -3.65f));
        TapButton blueSwitch = SwitchButton(root.transform, "BlueSwitch", "blue_button", new Vector2(1.55f, -3.65f));

        GameObject keypad = KeypadOverlay(root.transform, out BoxCollider2D panelCol, out TextMesh display, out TapButton[] keys);
        SpeechBubble bubble = Bubble(root.transform, new Vector2(0.9f, 2.2f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetField(level, "redSwitch", redSwitch);
        SetField(level, "blueSwitch", blueSwitch);
        SetField(level, "redLight", red);
        SetField(level, "blueLight", blue);
        SetField(level, "wall", wall);
        SetArray(level, "digits", digits.ToArray());
        SetField(level, "safeArea", safeArea);
        SetField(level, "safeClosed", closed);
        SetField(level, "safeOpen", open);
        SetField(level, "keypad", keypad);
        SetField(level, "keypadPanel", panelCol);
        SetArray(level, "keys", keys);
        SetField(level, "display", display);
        SetField(level, "confetti", confetti);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/LampSafe/Level_LampSafe.prefab");
    }

    static TapButton SwitchButton(Transform parent, string name, string sprite, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        Transform art = Art(go.transform, "Art", sprite, 6);
        go.AddComponent<BoxCollider2D>().size = new Vector2(1.8f, 1.1f);
        TapButton b = go.AddComponent<TapButton>();
        b.id = name;
        b.visual = art;
        return b;
    }

    /// <summary>Tastiera ingrandita della cassaforte (come al livello 18), inizialmente nascosta.</summary>
    static GameObject KeypadOverlay(Transform parent, out BoxCollider2D panelCol, out TextMesh display, out TapButton[] keys)
    {
        var keypad = new GameObject("Keypad").transform;
        keypad.SetParent(parent, false);
        SpriteRenderer dim = Art(keypad, "Dim", "plain", 30, false).GetComponent<SpriteRenderer>();
        dim.transform.localScale = new Vector3(12f, 16f, 1f);
        dim.color = new Color(0f, 0f, 0f, 0.45f);
        SpriteRenderer panel = Art(keypad, "Panel", "plain", 31, false, new Vector2(0, 0.95f)).GetComponent<SpriteRenderer>();
        panel.transform.localScale = new Vector3(4.3f, 5.6f, 1f);
        panel.color = new Color(0.55f, 0.58f, 0.66f);
        panelCol = keypad.gameObject.AddComponent<BoxCollider2D>();
        panelCol.size = new Vector2(4.3f, 5.6f);
        panelCol.offset = new Vector2(0, 0.95f);
        SpriteRenderer screen = Art(keypad, "Screen", "plain", 32, false, new Vector2(0, 3.05f)).GetComponent<SpriteRenderer>();
        screen.transform.localScale = new Vector3(3.4f, 0.85f, 1f);
        screen.color = new Color(0.06f, 0.23f, 0.16f);
        display = Label(keypad, new Vector2(0, 3.05f), 33, "_ _ _ _", 0.13f);
        var list = new List<TapButton>();
        for (int n = 0; n <= 9; n++)
        {
            Vector2 pos = n == 0 ? new Vector2(0, -1.4f) : new Vector2(-1.2f + ((n - 1) % 3) * 1.2f, 1.95f - ((n - 1) / 3) * 1.12f);
            var k = new GameObject("Key" + n);
            k.transform.SetParent(keypad, false);
            k.transform.localPosition = pos;
            Transform art = Art(k.transform, "Art", "keypad_key", 33, false);
            art.localScale = Vector3.one * 2.7f;
            Label(art, new Vector2(0, 0.01f), 34, n.ToString(), 0.05f);
            k.AddComponent<BoxCollider2D>().size = new Vector2(1.05f, 1.0f);
            TapButton tb = k.AddComponent<TapButton>();
            tb.id = n.ToString();
            tb.visual = art;
            list.Add(tb);
        }
        keypad.gameObject.SetActive(false);
        keys = list.ToArray();
        return keypad.gameObject;
    }

    static GameObject BuildAppleCatchLevel()
    {
        var root = new GameObject("Level_AppleCatch");
        AppleCatchLevel level = root.AddComponent<AppleCatchLevel>();
        Art(root.transform, "Garden", "garden_bg", -20, false);
        var tree = new GameObject("Tree").transform;
        tree.SetParent(root.transform, false);
        tree.localPosition = new Vector3(0, 1.3f, 0);
        Art(tree, "Art", "apple_tree", 2, false);
        var apples = new List<Transform>();
        foreach (Vector2 v in new[] { new Vector2(-1.6f, 1.1f), new Vector2(-0.9f, 1.85f), new Vector2(0f, 2.35f), new Vector2(0.9f, 1.9f), new Vector2(1.6f, 1.2f),
                                      new Vector2(-1.15f, 0.55f), new Vector2(-0.4f, 1.25f), new Vector2(0.5f, 1.05f), new Vector2(1.2f, 0.5f), new Vector2(0.1f, 0.4f) })
        {
            var a = new GameObject("Apple").transform;
            a.SetParent(root.transform, false);
            a.localPosition = v;
            a.localScale = Vector3.one * 0.3f;
            a.localRotation = Quaternion.Euler(0, 0, Random.Range(-20f, 20f));
            Art(a, "Art", "apple", 4, false);
            apples.Add(a);
        }
        var basket = new GameObject("Basket").transform;
        basket.SetParent(root.transform, false);
        basket.localPosition = new Vector3(0, -3.3f, 0);
        Art(basket, "Art", "basket", 6);
        TextMesh counter = Label(root.transform, new Vector2(0, -4.4f), 30, "0 / 5", 0.1f);
        SpeechBubble bubble = Bubble(root.transform, new Vector2(1.7f, -1.5f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetArray(level, "apples", apples.ToArray());
        SetField(level, "tree", tree);
        SetField(level, "basket", basket);
        SetField(level, "counter", counter);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/AppleCatch/Level_AppleCatch.prefab");
    }

    static GameObject BuildMarbleRunLevel()
    {
        var root = new GameObject("Level_MarbleRun");
        MarbleRunLevel level = root.AddComponent<MarbleRunLevel>();
        Art(root.transform, "Workshop", "workshop_bg", -20, false);
        Color wood = new Color(0.66f, 0.42f, 0.25f);

        // mensola di partenza in alto a sinistra
        Bar(root.transform, "Ledge", new Vector2(-2.05f, 2.2f), new Vector2(1.4f, 0.16f), wood, 3);
        var ledge = new GameObject("LedgeCollider");
        ledge.transform.SetParent(root.transform, false);
        ledge.transform.localPosition = new Vector3(-2.05f, 2.2f, 0);
        ledge.AddComponent<BoxCollider2D>().size = new Vector2(1.4f, 0.16f);

        // muretto in mezzo: la biglia deve passarci sopra
        Bar(root.transform, "Wall", new Vector2(0.1f, -1.25f), new Vector2(0.28f, 4.3f), wood, 3);
        var wallGo = new GameObject("WallCollider");
        wallGo.transform.SetParent(root.transform, false);
        wallGo.transform.localPosition = new Vector3(0.1f, -1.25f, 0);
        wallGo.AddComponent<BoxCollider2D>().size = new Vector2(0.28f, 4.3f);

        // tazza in basso a destra (pareti a U + zona di arrivo)
        var cup = new GameObject("Cup").transform;
        cup.SetParent(root.transform, false);
        cup.localPosition = new Vector3(1.9f, -3.3f, 0);
        cup.localScale = Vector3.one * 1.2f;
        Art(cup, "Art", "cup", 6);
        EdgeCollider2D cupWalls = cup.gameObject.AddComponent<EdgeCollider2D>();
        cupWalls.points = new[] { new Vector2(-0.56f, 0.8f), new Vector2(-0.48f, 0.04f), new Vector2(0.48f, 0.04f), new Vector2(0.56f, 0.8f) };
        cupWalls.edgeRadius = 0.04f;
        var zoneGo = new GameObject("CupZone");
        zoneGo.transform.SetParent(cup, false);
        BoxCollider2D zone = zoneGo.AddComponent<BoxCollider2D>();
        zone.isTrigger = true;
        zone.size = new Vector2(0.9f, 0.7f);
        zone.offset = new Vector2(0, 0.4f);

        Rigidbody2D marble = MazeBall(root.transform, new Vector2(-2.45f, 2.48f));
        marble.bodyType = RigidbodyType2D.Kinematic;

        // area di disegno (sopra il pulsante GO)
        var penGo = new GameObject("Pen");
        penGo.transform.SetParent(root.transform, false);
        BoxCollider2D area = penGo.AddComponent<BoxCollider2D>();
        area.isTrigger = true;
        area.size = new Vector2(5.8f, 6.4f);
        area.offset = new Vector2(0, 0.2f);
        StrokeDrawer pen = penGo.AddComponent<StrokeDrawer>();
        pen.singleStroke = false;
        pen.maxStrokes = 3;
        pen.minLength = 0.3f;
        pen.addCollider = true;
        pen.colliderRadius = 0.07f;
        pen.sortingOrder = 8;
        pen.material = StrokeMaterial();
        pen.layers = new[]
        {
            new StrokeDrawer.Layer { color = InkColor, width = 0.15f, offset = Vector2.zero },
            new StrokeDrawer.Layer { color = new Color(1f, 0.6f, 0.3f), width = 0.07f, offset = Vector2.zero },
        };

        var goGo = new GameObject("Go");
        goGo.transform.SetParent(root.transform, false);
        goGo.transform.localPosition = new Vector3(-1.6f, -3.95f, 0);
        Transform goArt = Art(goGo.transform, "Art", "big_button", 30);
        goArt.localScale = Vector3.one * 0.8f;
        TextMesh goLabel = Label(goGo.transform, new Vector2(0, 0.08f), 31, "GO", 0.09f);
        goLabel.color = Color.white;
        goGo.AddComponent<BoxCollider2D>().size = new Vector2(1.6f, 1.0f);
        TapButton go = goGo.AddComponent<TapButton>();
        go.id = "go";
        go.visual = goArt;

        SmokeEmitter confetti = Emitter(root.transform, "Confetti", new Vector2(1.9f, -2.4f), "confetti", 30f, 1.2f, 1.6f, 0.6f, 1.1f, new Color(1f, 0.85f, 0.35f), 45);
        SpeechBubble bubble = Bubble(root.transform, new Vector2(0.9f, 2.9f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetField(level, "marble", marble);
        SetField(level, "go", go);
        SetField(level, "cupZone", zone);
        SetField(level, "confetti", confetti);
        SetField(level, "bubble", bubble);
        return SavePrefab(root, LevelDir + "/MarbleRun/Level_MarbleRun.prefab");
    }

    static GameObject BuildCopycatLevel()
    {
        var root = new GameObject("Level_Copycat");
        CopycatLevel level = root.AddComponent<CopycatLevel>();
        Bar(root.transform, "Wall", new Vector2(0, 1f), new Vector2(12f, 14f), new Color(0.98f, 0.9f, 0.82f), -20);
        var face = new GameObject("CatFace").transform;
        face.SetParent(root.transform, false);
        face.localPosition = new Vector3(0, 2.0f, 0);
        face.localScale = Vector3.one * 0.55f;
        Art(face, "Art", "cat_big_face", 2);
        foreach (float x in new[] { -0.58f, 0.58f }) Art(face, "Pupil", "cat_pupil", 3, false, new Vector2(x, 0.1f));

        TextMesh roundLabel = Label(root.transform, new Vector2(0, 0.45f), 30, "ROUND 1 / 3", 0.08f);
        roundLabel.color = InkColor;
        Color[] colors = { new Color(1f, 0.33f, 0.36f), new Color(0.36f, 0.82f, 0.42f), new Color(0.3f, 0.52f, 1f), new Color(1f, 0.83f, 0.25f) };
        Vector2[] spots = { new Vector2(-0.82f, -0.85f), new Vector2(0.82f, -0.85f), new Vector2(-0.82f, -2.5f), new Vector2(0.82f, -2.5f) };
        var pads = new List<TapButton>();
        var arts = new List<SpriteRenderer>();
        var glows = new List<SpriteRenderer>();
        for (int i = 0; i < 4; i++)
        {
            var go = new GameObject("Pad" + i);
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = spots[i];
            SpriteRenderer glow = Art(go.transform, "Glow", "glow", 4, false).GetComponent<SpriteRenderer>();
            glow.transform.localScale = Vector3.one * 2.2f;
            glow.color = colors[i];
            glow.enabled = false;
            Transform art = Art(go.transform, "Art", "pad", 5, false);
            SpriteRenderer sr = art.GetComponent<SpriteRenderer>();
            sr.color = colors[i];
            go.AddComponent<BoxCollider2D>().size = new Vector2(1.45f, 1.45f);
            TapButton b = go.AddComponent<TapButton>();
            b.id = i.ToString();
            b.visual = art;
            pads.Add(b);
            arts.Add(sr);
            glows.Add(glow);
        }
        SpeechBubble bubble = Bubble(root.transform, new Vector2(1.45f, 3.1f));
        bubble.transform.localScale = Vector3.one * 0.5f;
        bubble.catVoice = true;   // il gatto miagola quando parla

        SetArray(level, "pads", pads.ToArray());
        SetArray(level, "padArt", arts.ToArray());
        SetArray(level, "padGlow", glows.ToArray());
        SetField(level, "catBubble", bubble);
        SetField(level, "roundLabel", roundLabel);
        return SavePrefab(root, LevelDir + "/Copycat/Level_Copycat.prefab");
    }

    static TapButton BombButton(Transform parent, string name, string sprite, Vector2 pos, out Transform wireStart)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        Transform art = Art(go.transform, "Art", sprite, 6);
        go.AddComponent<BoxCollider2D>().size = new Vector2(1.8f, 1.1f);
        TapButton b = go.AddComponent<TapButton>();
        b.id = name;
        b.visual = art;
        wireStart = new GameObject("WireStart").transform;
        wireStart.SetParent(go.transform, false);
        wireStart.localPosition = new Vector3(0, 0.15f, 0);
        return b;
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

    const string CustomIconPath = "Assets/Art/Icon/AppIcon.png";

    static void ConfigurePlayer()
    {
        // Icona: se c'è il logo originale in Assets/Art/Icon/AppIcon.png si usa quello, altrimenti la versione disegnata
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(CustomIconPath);
        if (icon == null && sprites.TryGetValue("app_icon", out Sprite drawn)) icon = drawn.texture;
        if (icon != null) PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { icon });

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

    static void SetFloat(Object target, string field, float value)
    {
        var so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null) { Debug.LogError("[Gotcha] Campo non trovato: " + target.GetType().Name + "." + field); return; }
        p.floatValue = value;
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
