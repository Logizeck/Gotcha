#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli compositi della versione 0.40 (gruppo B).</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels40B() => new List<LevelDefinition>
    {
        Define("KitchenEscape", "kitchen_escape", "KITCHEN ESCAPE", "Tap things to pick them up. Then get crafty.", BuildKitchenEscapeLevel()),
        Define("Station", "space_station", "SPACE STATION", "Power first, then signal, then the launch code.", BuildSpaceStationLevel()),
        Define("TreasureMap", "treasure_map", "TREASURE MAP", "Every detail on the map matters. EVERY detail.", BuildTreasureMapLevel()),
        Define("Detective", "detective", "DETECTIVE", "Listen carefully, look closely, then accuse.", BuildDetectiveLevel()),
    };

    static readonly Color V40B_Dark = new Color(0.1f, 0.08f, 0.14f, 0.6f);

    /// <summary>Barra delle fasi: pallini in alto a sinistra, cuori in alto a destra, obiettivo e scritta grande.</summary>
    static StageHud40B V40B_Hud(Transform root, int stages, int tries)
    {
        Transform h = Child(root, "Hud", Vector2.zero).transform;
        StageHud40B hud = h.gameObject.AddComponent<StageHud40B>();
        var dots = new List<SpriteRenderer>();
        for (int i = 0; i < stages; i++) dots.Add(Art(h, "Dot", "v40b_dot", 47, false, new Vector2(-2.3f + i * 0.42f, 3.5f)).GetComponent<SpriteRenderer>());
        var hearts = new List<SpriteRenderer>();
        for (int i = 0; i < tries; i++)
        {
            Transform t = Art(h, "Heart", "heart", 47, false, new Vector2(2.35f - i * 0.45f, 3.5f));
            t.localScale = Vector3.one * 0.75f;
            hearts.Add(t.GetComponent<SpriteRenderer>());
        }
        Bar(h, "GoalBack", new Vector2(0f, 3.05f), new Vector2(5.8f, 0.42f), V40B_Dark, 45);
        TextMesh goal = Fit(Label(h, new Vector2(0f, 3.05f), 46, "", 0.04f), 5.5f, 0.36f);
        goal.color = Color.white;
        SpriteRenderer back = Bar(h, "BannerBack", new Vector2(0f, 0.6f), new Vector2(6.2f, 1.3f), new Color(0.1f, 0.08f, 0.14f, 0.82f), 48);
        TextMesh banner = Fit(Label(h, new Vector2(0f, 0.6f), 49, "", 0.09f), 5.4f, 1.0f);
        banner.color = new Color(1f, 0.85f, 0.3f);
        SetArray(hud, "dots", dots.ToArray());
        SetArray(hud, "hearts", hearts.ToArray());
        SetField(hud, "goal", goal);
        SetField(hud, "banner", banner);
        SetField(hud, "bannerBack", back);
        return hud;
    }

    static T V40B_Collider<T>(Transform t, Vector2 size) where T : Collider2D
    {
        T c = t.gameObject.AddComponent<T>();
        if (c is BoxCollider2D b) b.size = size;
        if (c is CircleCollider2D cc) cc.radius = size.x * 0.5f;
        return c;
    }

    // ---------- Kitchen escape: inventario, combinazioni, pesca, serratura e pizzico ----------

    static GameObject BuildKitchenEscapeLevel()
    {
        var root = new GameObject("Level_KitchenEscape");
        KitchenEscapeLevel level = root.AddComponent<KitchenEscapeLevel>();
        Transform r = root.transform;
        Art(r, "Kitchen", "v40b_kitchen_bg", -20, false);

        Vector2 cupAt = new Vector2(1.35f, 1.55f);
        SpriteRenderer cup = Art(r, "Cupboard", "v40b_cupboard", 1, false, cupAt).GetComponent<SpriteRenderer>();
        Transform cupArea = Child(r, "CupboardArea", cupAt).transform;
        V40B_Collider<BoxCollider2D>(cupArea, new Vector2(2.4f, 1.8f));
        Transform cheese = Fitted(r, "Cheese", "cheese_wedge", cupAt + new Vector2(0f, -0.02f), 0.9f, 3);
        SpriteRenderer latch = Art(r, "Latch", "v40b_latch", 4, false, cupAt + new Vector2(0f, 0.1f)).GetComponent<SpriteRenderer>();
        Transform latchArea = Child(r, "LatchArea", cupAt + new Vector2(0f, 0.1f)).transform;
        V40B_Collider<BoxCollider2D>(latchArea, new Vector2(1.1f, 0.7f));
        Art(r, "KeyPlate", "v40b_keyplate", 4, false, cupAt + new Vector2(0f, -0.45f));
        Transform lockArea = Child(r, "LockArea", cupAt + new Vector2(0f, -0.45f)).transform;
        V40B_Collider<CircleCollider2D>(lockArea, new Vector2(0.8f, 0.8f));

        Vector2 drawerAt = new Vector2(0.15f, -1.3f);
        SpriteRenderer drawer = Art(r, "Drawer", "v40b_drawer", 2, false, drawerAt).GetComponent<SpriteRenderer>();
        Transform drawerArea = Child(r, "DrawerArea", drawerAt).transform;
        V40B_Collider<BoxCollider2D>(drawerArea, new Vector2(1.5f, 0.7f));

        Transform drain = Child(r, "Drain", new Vector2(1.6f, -0.7f)).transform;
        V40B_Collider<CircleCollider2D>(drain, new Vector2(1.0f, 1.0f));
        LineRenderer fishLine = Line(r, "FishLine", 0.035f, new Color(0.89f, 0.76f, 0.54f), 30);

        Transform mouse = Fitted(r, "Mouse", "mouse", new Vector2(-0.4f, -2.9f), 0.95f, 5);
        BoxCollider2D mouseCol = V40B_Collider<BoxCollider2D>(mouse, Vector2.one * (1.0f / mouse.localScale.x));

        Art(r, "Tray", "v40b_tray", 20, false, new Vector2(0f, -3.95f));
        var slots = new List<Transform>();
        for (int i = 0; i < 4; i++) slots.Add(Child(r, "Slot" + i, new Vector2(-1.8f + i * 1.2f, -3.95f)).transform);

        // oggetti: calamita, forchetta, spago, calamita+spago, chiave
        Transform[] items =
        {
            Fitted(r, "Magnet", "magnet", new Vector2(-1.55f, 0.2f), 0.7f, 22),
            Fitted(r, "Fork", "v40b_fork", new Vector2(1.6f, -2.95f), 0.85f, 22),
            Fitted(r, "String", "v40b_string", drawerAt + new Vector2(0f, 0.42f), 0.62f, 22),
            Fitted(r, "MagnetString", "v40b_magnet_string", Vector2.zero, 0.85f, 22),
            Fitted(r, "Key", "key", Vector2.zero, 0.85f, 22),
        };
        foreach (Transform t in items) V40B_Collider<BoxCollider2D>(t, Vector2.one * (0.9f / t.localScale.x));

        SetField(level, "hud", V40B_Hud(r, 4, 3));
        SetField(level, "bubble", SmallBubble(r, new Vector2(-0.9f, -1.75f)));
        SetField(level, "mouse", mouse);
        SetField(level, "mouseArea", mouseCol);
        SetArray(level, "items", items);
        SetArray(level, "slots", slots.ToArray());
        SetField(level, "drawerArea", drawerArea.GetComponent<Collider2D>());
        SetField(level, "drawerArt", drawer);
        SetField(level, "drawerOpenSprite", sprites["v40b_drawer_open"]);
        SetField(level, "drainArea", drain.GetComponent<Collider2D>());
        SetField(level, "drainPoint", drain);
        SetField(level, "fishLine", fishLine);
        SetField(level, "cupboardArea", cupArea.GetComponent<Collider2D>());
        SetField(level, "cupboardArt", cup);
        SetField(level, "cupboardOpenSprite", sprites["v40b_cupboard_open"]);
        SetField(level, "lockArea", lockArea.GetComponent<Collider2D>());
        SetField(level, "latchArea", latchArea.GetComponent<Collider2D>());
        SetField(level, "latchArt", latch);
        SetField(level, "latchOpenSprite", sprites["v40b_latch_open"]);
        SetField(level, "cheese", cheese);
        SetField(level, "cheeseLanding", Child(r, "CheeseLanding", new Vector2(0.45f, -2.95f)).transform);
        return SaveLevel(root, "KitchenEscape");
    }

    // ---------- Space station: cavi, parabola inclinando, codice luminoso ----------

    static GameObject BuildSpaceStationLevel()
    {
        var root = new GameObject("Level_Station");
        SpaceStationLevel level = root.AddComponent<SpaceStationLevel>();
        Transform r = root.transform;
        Art(r, "Station", "v40b_station_bg", -20, false);
        SpriteRenderer dark = Bar(r, "Darkness", Vector2.zero, new Vector2(12f, 16f), new Color(0f, 0f, 0.05f, 0.55f), -15);
        Color dim = new Color(0.75f, 0.8f, 1f);

        // fase 1: griglia 3x3 (riga 0 in alto), celle da 1
        Transform grid = Child(r, "GridStage", Vector2.zero).transform;
        const float gy = 0.4f;
        Vector2 CellPos(int cell) => new Vector2(cell % 3 - 1f, gy + 1f - cell / 3);
        Bar(grid, "Panel", new Vector2(0f, gy), new Vector2(3.3f, 3.3f), new Color(0.08f, 0.09f, 0.15f, 0.9f), 0);
        for (int c = 0; c < 9; c++) Art(grid, "Tile", "v40b_tile", 1, false, CellPos(c));
        var tiles = new List<Transform>();
        var glows = new List<SpriteRenderer>();
        for (int i = 0; i < SpaceStationLevel.Cells.Length; i++)
        {
            string kind = SpaceStationLevel.Corner[i] ? "corner" : "straight";
            Transform t = Child(grid, "Cable" + i, CellPos(SpaceStationLevel.Cells[i])).transform;
            Art(t, "Cable", "v40b_cable_" + kind, 3, false);
            SpriteRenderer g = Art(t, "Energy", "v40b_energy_" + kind, 4, false).GetComponent<SpriteRenderer>();
            g.color = new Color(1f, 1f, 1f, 0f);
            V40B_Collider<BoxCollider2D>(t, new Vector2(1f, 1f));
            tiles.Add(t);
            glows.Add(g);
        }
        Vector2 reactorAt = new Vector2(-2.2f, gy);
        SpriteRenderer reactorGlow = Art(grid, "ReactorGlow", "glow", 1, false, reactorAt).GetComponent<SpriteRenderer>();
        reactorGlow.transform.localScale = Vector3.one * 1.6f;
        Art(grid, "Reactor", "v40b_reactor", 2, false, reactorAt);
        Bar(grid, "Plug", new Vector2(-1.62f, gy), new Vector2(0.2f, 0.12f), new Color(0.72f, 0.74f, 0.79f), 2);
        TextMesh rl = Label(grid, reactorAt + new Vector2(0f, -0.95f), 3, "REACTOR", 0.03f);
        rl.color = dim;
        Vector2 dishAt = new Vector2(2.15f, gy - 1f);
        SpriteRenderer dishLight = Art(grid, "DishLight", "glow", 1, false, dishAt + new Vector2(0f, 0.25f)).GetComponent<SpriteRenderer>();
        dishLight.transform.localScale = Vector3.one * 1.3f;
        dishLight.color = new Color(0.4f, 0.4f, 0.5f, 0.3f);
        Fitted(grid, "DishIcon", "v40b_dish", dishAt + new Vector2(0f, 0.2f), 0.75f, 2);
        Bar(grid, "Plug2", new Vector2(1.62f, gy - 1f), new Vector2(0.2f, 0.12f), new Color(0.72f, 0.74f, 0.79f), 2);
        TextMesh dl = Label(grid, dishAt + new Vector2(0f, -0.55f), 3, "DISH", 0.03f);
        dl.color = dim;

        // fase 2: la parabola
        Transform dishStage = Child(r, "DishStage", Vector2.zero).transform;
        Art(dishStage, "Mount", "v40b_dish_mount", 3, false, new Vector2(0f, -1.1f));
        Transform dish = Child(dishStage, "Dish", new Vector2(0f, -1.1f)).transform;
        dish.localScale = Vector3.one * 1.15f;
        Art(dish, "Art", "v40b_dish", 2, false);
        Transform sat = Fitted(dishStage, "Satellite", "v40b_satellite", new Vector2(0f, 2f), 0.9f, 5);
        var bars = new List<SpriteRenderer>();
        for (int i = 0; i < 5; i++)
        {
            float h = 0.2f + 0.13f * i;
            bars.Add(Bar(dishStage, "Signal" + i, new Vector2(-0.6f + i * 0.38f, -3.15f + h * 0.5f), new Vector2(0.28f, h), Color.white, 6));
        }
        TextMesh sigLabel = Label(dishStage, new Vector2(-1.85f, -2.85f), 6, "SIGNAL", 0.035f);
        sigLabel.color = dim;
        TextMesh noise = Fit(Label(dishStage, new Vector2(0f, -2.35f), 6, "KSSSSHHHHH", 0.045f), 5f, 0.4f);
        noise.color = new Color(0.6f, 1f, 1f);
        Transform hold = GaugeBar(dishStage, "Hold", new Vector2(0.3f, -3.55f), new Vector2(3.2f, 0.18f), new Color(0f, 0f, 0f, 0.5f), new Color(0.5f, 1f, 0.4f), 6);
        hold.localScale = new Vector3(0f, 1f, 1f);
        TextMesh lockLabel = Label(dishStage, new Vector2(-1.85f, -3.55f), 6, "LOCK", 0.035f);
        lockLabel.color = dim;

        // fase 3: lampada, nastro, tabella e tastierino
        Transform codeStage = Child(r, "CodeStage", Vector2.zero).transform;
        Art(codeStage, "Lamp", "v40b_lamp_signal", 3, false, new Vector2(0f, 2.15f));
        SpriteRenderer lampGlow = Art(codeStage, "LampGlow", "glow", 4, false, new Vector2(0f, 2.15f)).GetComponent<SpriteRenderer>();
        lampGlow.transform.localScale = Vector3.one * 1.8f;
        lampGlow.color = new Color(1f, 0.9f, 0.3f, 0.95f);
        Bar(codeStage, "Tape", new Vector2(0f, 1.15f), new Vector2(5.2f, 0.42f), new Color(0.05f, 0.06f, 0.1f, 0.9f), 5);
        var marks = new List<SpriteRenderer>();
        for (int i = 0; i < 12; i++) marks.Add(Bar(codeStage, "Mark", new Vector2(0f, 1.15f), new Vector2(0.14f, 0.14f), new Color(1f, 0.9f, 0.3f), 6));
        Bar(codeStage, "TablePanel", new Vector2(-1.3f, -1.65f), new Vector2(2.6f, 3.35f), new Color(0.06f, 0.07f, 0.12f, 0.92f), 5);
        TextMesh tt = Label(codeStage, new Vector2(-1.3f, -0.2f), 6, "CODE TABLE", 0.035f);
        tt.color = new Color(1f, 0.9f, 0.3f);
        int[] order = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
        for (int n = 0; n < 10; n++)
        {
            int digit = order[n];
            float x0 = n < 5 ? -2.42f : -1.2f, y = -0.65f + -(n % 5) * 0.58f;
            TextMesh dt = Label(codeStage, new Vector2(x0, y), 6, digit.ToString(), 0.05f);
            dt.color = Color.white;
            float x = x0 + 0.22f;
            foreach (char ch in SpaceStationLevel.Codes[digit])
            {
                float w = ch == 'L' ? 0.24f : 0.08f;
                Bar(codeStage, "Sym", new Vector2(x + w * 0.5f, y), new Vector2(w, 0.09f), new Color(1f, 0.9f, 0.3f), 6);
                x += w + 0.06f;
            }
        }
        Bar(codeStage, "DisplayBack", new Vector2(1.25f, -0.15f), new Vector2(2.0f, 0.55f), new Color(0.02f, 0.05f, 0.03f, 0.95f), 5);
        TextMesh display = Fit(Label(codeStage, new Vector2(1.25f, -0.15f), 6, "___", 0.07f), 1.8f, 0.45f);
        display.color = new Color(0.4f, 1f, 0.5f);
        string[] keyNames = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "C", "0", "OK" };
        var keys = new List<Collider2D>();
        for (int i = 0; i < 12; i++)
        {
            Vector2 at = new Vector2(0.55f + (i % 3) * 0.7f, -0.85f - (i / 3) * 0.66f);
            Transform k = Fitted(codeStage, "Key" + keyNames[i], "keypad_key", at, 0.62f, 6);
            Fit(Label(k, Vector2.zero, 7, keyNames[i], 0.06f / k.localScale.x), 0.5f / k.localScale.x, 0.4f / k.localScale.x);
            keys.Add(V40B_Collider<BoxCollider2D>(k, Vector2.one * (0.66f / k.localScale.x)));
        }

        // il razzo finale
        Transform rocket = Child(r, "Rocket", new Vector2(0f, -7f)).transform;
        Fitted(rocket, "Art", "rocket", Vector2.zero, 2.6f, 10);
        Transform flame = Child(rocket, "Flame", new Vector2(0f, -1.3f)).transform;
        Transform fl = Fitted(flame, "Art", "flame", new Vector2(0f, -0.3f), 0.8f, 9);
        fl.localRotation = Quaternion.Euler(0, 0, 180f);

        SetField(level, "hud", V40B_Hud(r, 3, 3));
        SetField(level, "bubble", SmallBubble(r, new Vector2(0f, -4.15f)));
        SetField(level, "darkness", dark);
        SetField(level, "gridGroup", grid);
        SetField(level, "dishGroup", dishStage);
        SetField(level, "codeGroup", codeStage);
        SetArray(level, "tiles", tiles.ToArray());
        SetArray(level, "glows", glows.ToArray());
        SetField(level, "reactorGlow", reactorGlow);
        SetField(level, "dishLight", dishLight);
        SetField(level, "dish", dish);
        SetField(level, "satellite", sat);
        SetArray(level, "signalBars", bars.ToArray());
        SetField(level, "holdBar", hold);
        SetField(level, "noiseText", noise);
        SetField(level, "lampGlow", lampGlow);
        SetArray(level, "tapeMarks", marks.ToArray());
        SetArray(level, "keys", keys.ToArray());
        SetField(level, "display", display);
        SetField(level, "rocket", rocket);
        SetField(level, "flame", flame);
        return SaveLevel(root, "Station");
    }

    // ---------- Treasure map: mappa strappata, camminata con la bussola, scavo, lucchetto ----------

    static GameObject BuildTreasureMapLevel()
    {
        var root = new GameObject("Level_TreasureMap");
        TreasureMapLevel level = root.AddComponent<TreasureMapLevel>();
        Transform r = root.transform;
        Color ink = new Color(0.54f, 0.35f, 0.23f);

        // fase 1: spiaggia, sagoma e pezzi
        Transform beach = Child(r, "BeachStage", Vector2.zero).transform;
        Art(beach, "Beach", "v40b_beach_bg", -20, false);
        Vector2 mapAt = new Vector2(0f, 0.45f);
        SpriteRenderer outline = Art(r, "Outline", "v40b_map_outline", 1, false, mapAt).GetComponent<SpriteRenderer>();
        Transform mapHolder = Child(r, "Map", mapAt).transform;
        Vector2[] landings = { new Vector2(-1.5f, -2.35f), new Vector2(1.55f, -2.4f), new Vector2(-0.7f, -3.55f), new Vector2(1.1f, -3.6f) };
        var pieces = new List<Transform>();
        var targets = new List<Transform>();
        var syms = new List<SpriteRenderer>();
        var compasses = new List<SpriteRenderer>();
        var nums = new List<TextMesh>();
        for (int q = 0; q < 4; q++)
        {
            string name = "v40b_map_piece" + q;
            Vector2 c = centers[name];
            targets.Add(Child(mapHolder, "Target" + q, c).transform);
            Transform h = Child(r, "Piece" + q, landings[q]).transform;
            h.localScale = Vector3.one * 0.9f;
            Art(h, "Art", name, 30, false, -c);
            V40B_Collider<BoxCollider2D>(h, new Vector2(1.75f, 1.75f));
            float sx = q % 2 == 0 ? -1f : 1f, sy = q < 2 ? 1f : -1f;
            Vector2 corner = new Vector2(sx * 1.08f, sy * 1.1f) - c;
            syms.Add(Fitted(h, "Symbol", "v40b_sym_skull", corner, 0.55f, 31).GetComponentInChildren<SpriteRenderer>());
            compasses.Add(Fitted(h, "Compass", "v40b_compass", corner, 0.62f, 31).GetComponentInChildren<SpriteRenderer>());
            TextMesh num = Label(h, new Vector2(sx * 0.62f, sy * 1.12f) - c, 32, "1", 0.08f);
            num.color = new Color(0.75f, 0.22f, 0.17f);
            nums.Add(num);
            pieces.Add(h);
        }
        TextMesh mapText = Fit(Label(mapHolder, new Vector2(0f, -0.85f), 33, "", 0.05f), 2.4f, 0.32f);
        mapText.color = ink;
        Transform mapIcon = Child(r, "MapIcon", new Vector2(-2.15f, 2.5f)).transform;
        mapIcon.localScale = Vector3.one * 0.22f;
        Transform mapButton = Child(r, "MapButton", new Vector2(-2.15f, 2.5f)).transform;
        V40B_Collider<BoxCollider2D>(mapButton, new Vector2(1.0f, 1.0f));
        SpriteRenderer mapDim = Bar(r, "MapDim", Vector2.zero, new Vector2(12f, 16f), new Color(0f, 0f, 0f, 0.6f), 28);

        // fase 2-3: l'isola
        Transform island = Child(r, "IslandStage", Vector2.zero).transform;
        Art(island, "Island", "v40b_island_bg", -20, false);
        Transform compass = Fitted(island, "Compass", "v40b_compass", new Vector2(-1.95f, -4.0f), 0.95f, 5);
        Bar(island, "Scroll", new Vector2(0.25f, 2.45f), new Vector2(4.4f, 0.55f), new Color(0.98f, 0.91f, 0.71f), 8);
        TextMesh dirs = Fit(Label(island, new Vector2(0.25f, 2.45f), 9, "", 0.06f), 4.2f, 0.45f);
        dirs.color = ink;
        Transform gridOrigin = Child(island, "GridOrigin", new Vector2(-1.7f, -3.025f)).transform;
        Fitted(island, "Boat", "v40b_boat", new Vector2(0f, -4.05f), 0.85f, 4);
        Transform x = Fitted(island, "X", "v40b_x_mark", Vector2.zero, 0.7f, 6);
        Transform hole = Child(island, "Hole", Vector2.zero).transform;
        Fitted(hole, "Art", "v40b_sand_hole", Vector2.zero, 1.2f, 7);
        Transform smallChest = Fitted(island, "SmallChest", "chest", Vector2.zero, 0.85f, 13);
        Transform cat = Child(island, "Cat", Vector2.zero).transform;
        cat.localScale = Vector3.one * 0.6f;
        Art(cat, "Art", "v40b_pirate_cat", 12, false, new Vector2(-0.05f, -0.55f));
        var grains = new List<Transform>();
        for (int i = 0; i < 8; i++)
        {
            Transform g = Art(island, "Grain", "puff", 14, false);
            g.GetComponent<SpriteRenderer>().color = new Color(0.95f, 0.8f, 0.55f);
            grains.Add(g);
        }
        Transform digGauge = Child(island, "DigGauge", Vector2.zero).transform;
        Transform digBar = GaugeBar(digGauge, "Dig", new Vector2(0.6f, -4.15f), new Vector2(3.0f, 0.2f), new Color(0f, 0f, 0f, 0.5f), new Color(1f, 0.8f, 0.3f), 9);
        digGauge.gameObject.SetActive(false);

        // fase 4: il baule
        Transform chestStage = Child(r, "ChestStage", Vector2.zero).transform;
        Art(chestStage, "Beach", "v40b_beach_bg", -20, false);
        Transform chest = Fitted(chestStage, "Chest", "chest", new Vector2(0f, 0.4f), 2.9f, 5);
        var dials = new List<Collider2D>();
        var dialSyms = new List<SpriteRenderer>();
        for (int i = 0; i < 3; i++)
        {
            Vector2 at = new Vector2(-1.15f + i * 1.15f, -2.0f);
            Transform d = Fitted(chestStage, "Dial" + i, "v40b_dial", at, 1.1f, 6);
            dials.Add(V40B_Collider<BoxCollider2D>(d, new Vector2(1.0f, 1.2f) / d.localScale.x));
            dialSyms.Add(Fitted(chestStage, "DialSymbol" + i, "v40b_sym_skull", at, 0.62f, 7).GetComponentInChildren<SpriteRenderer>());
            TextMesh n = Label(chestStage, at + new Vector2(0f, 0.8f), 7, (i + 1).ToString(), 0.07f);
            n.color = Color.white;
        }
        Transform open = Child(chestStage, "Open", new Vector2(0f, -3.55f)).transform;
        Bar(open, "Back", Vector2.zero, new Vector2(2.2f, 0.75f), new Color(1f, 0.79f, 0.24f), 6);
        Label(open, new Vector2(0f, 0.02f), 7, "OPEN", 0.07f);
        V40B_Collider<BoxCollider2D>(open, new Vector2(2.3f, 0.85f));
        var coins = new List<Transform>();
        for (int i = 0; i < 6; i++) coins.Add(Fitted(chestStage, "Coin", "coin", Vector2.zero, 0.4f, 8));

        SetField(level, "hud", V40B_Hud(r, 4, 3));
        SetField(level, "bubble", SmallBubble(r, new Vector2(0.4f, -1.5f)));
        SetArray(level, "pieces", pieces.ToArray());
        SetArray(level, "targets", targets.ToArray());
        SetField(level, "mapHolder", mapHolder);
        SetField(level, "outline", outline);
        SetArray(level, "cornerSymbols", syms.ToArray());
        SetArray(level, "cornerCompasses", compasses.ToArray());
        SetArray(level, "cornerNumbers", nums.ToArray());
        SetArray(level, "symbolSprites", new Object[] { sprites["v40b_sym_skull"], sprites["v40b_sym_parrot"], sprites["v40b_sym_anchor"], sprites["v40b_sym_palm"] });
        SetField(level, "mapText", mapText);
        SetField(level, "mapIcon", mapIcon);
        SetField(level, "mapButton", mapButton.GetComponent<Collider2D>());
        SetField(level, "mapDim", mapDim);
        SetField(level, "beachGroup", beach);
        SetField(level, "islandGroup", island);
        SetField(level, "chestGroup", chestStage);
        SetField(level, "cat", cat);
        SetField(level, "islandCompass", compass);
        SetField(level, "directions", dirs);
        SetField(level, "gridOrigin", gridOrigin);
        SetField(level, "xMark", x);
        SetField(level, "hole", hole);
        SetArray(level, "sand", grains.ToArray());
        SetField(level, "digBar", digBar);
        SetField(level, "smallChest", smallChest);
        SetField(level, "chestArt", chest.GetComponentInChildren<SpriteRenderer>());
        SetField(level, "chestOpenSprite", sprites["chest_open"]);
        SetArray(level, "dials", dials.ToArray());
        SetArray(level, "dialSymbols", dialSyms.ToArray());
        SetField(level, "openButton", open.GetComponent<Collider2D>());
        SetArray(level, "coins", coins.ToArray());
        return SaveLevel(root, "TreasureMap");
    }

    // ---------- Detective: alibi, lente, manette ----------

    static GameObject BuildDetectiveLevel()
    {
        var root = new GameObject("Level_Detective");
        DetectiveLevel level = root.AddComponent<DetectiveLevel>();
        Transform r = root.transform;

        Transform lineup = Child(r, "LineupStage", Vector2.zero).transform;
        Art(lineup, "Room", "v40b_lineup_bg", -20, false);
        // gatto
        Transform cat = Child(lineup, "Cat", new Vector2(-1.7f, -2.2f)).transform;
        cat.localScale = Vector3.one * 1.45f;
        Art(cat, "Art", "roof_cat", 5, false);
        BoxCollider2D cc = cat.gameObject.AddComponent<BoxCollider2D>();
        cc.size = new Vector2(0.9f, 1.25f);
        cc.offset = new Vector2(0.05f, 0.6f);
        GameObject wet = Child(cat, "Wet", Vector2.zero);
        foreach (Vector2 p in new[] { new Vector2(-0.3f, 0.95f), new Vector2(0.32f, 0.55f), new Vector2(-0.2f, 0.35f), new Vector2(0.2f, 1.05f) })
            Art(wet.transform, "Drop", "drop", 6, false, p).localScale = Vector3.one * 0.55f;
        GameObject crumbs = Art(cat, "Crumbs", "v40b_crumbs", 6, false, new Vector2(0f, 0.64f)).gameObject;
        // cane
        Transform dog = Child(lineup, "Dog", new Vector2(0f, -2.2f)).transform;
        dog.localScale = Vector3.one * 1.25f;
        Art(dog, "Art", "v40b_dog", 5, false);
        BoxCollider2D dc = dog.gameObject.AddComponent<BoxCollider2D>();
        dc.size = new Vector2(1.1f, 1.6f);
        dc.offset = new Vector2(0f, 0.8f);
        GameObject mud = Child(dog, "Mud", Vector2.zero);
        foreach (float s in new[] { -1f, 1f }) Art(mud.transform, "Mud", "v40b_mud", 6, false, new Vector2(s * 0.17f, 0.07f));
        Art(mud.transform, "Mud", "v40b_mud", 6, false, new Vector2(0.25f, 0.35f)).localScale = Vector3.one * 0.7f;
        // pappagallo
        Transform parrot = Child(lineup, "Parrot", new Vector2(1.7f, -2.2f)).transform;
        parrot.localScale = Vector3.one * 1.2f;
        Art(parrot, "Art", "v40b_parrot", 5, false);
        Transform wing = Child(parrot, "Wing", new Vector2(-0.08f, 0.88f)).transform;
        Art(wing, "Art", "v40b_parrot_wing", 6, false);
        BoxCollider2D pc = parrot.gameObject.AddComponent<BoxCollider2D>();
        pc.size = new Vector2(0.95f, 1.6f);
        pc.offset = new Vector2(0f, 0.78f);
        var ticks = new List<SpriteRenderer>();
        foreach (float sx in new[] { -1.7f, 0f, 1.7f })
            ticks.Add(Art(lineup, "Tick", "v40b_check", 7, false, new Vector2(sx + 0.55f, -2.5f)).GetComponent<SpriteRenderer>());
        Transform cuffs = Fitted(r, "Cuffs", "v40b_handcuffs", new Vector2(0f, -3.6f), 1.15f, 20);
        TextMesh cuffHint = Label(cuffs, new Vector2(0f, -0.45f) / cuffs.localScale.x, 20, "DRAG ME", 0.035f / cuffs.localScale.x);
        cuffHint.color = Color.white;

        // la scena del crimine
        Transform scene = Child(r, "SceneStage", Vector2.zero).transform;
        Bar(scene, "Back", Vector2.zero, new Vector2(12f, 16f), new Color(0.12f, 0.1f, 0.16f), -19);
        Transform frame = Child(scene, "Frame", new Vector2(0f, -0.85f)).transform;
        frame.localScale = Vector3.one * 0.85f;
        Transform photo = Child(frame, "Photo", Vector2.zero).transform;
        Art(photo, "Art", "v40b_crime_scene", 2, false);
        Vector2[] spots =
        {
            new Vector2(-0.6f, -1.45f), new Vector2(0.75f, -2.75f),    // impronte
            new Vector2(1.55f, 2.42f), new Vector2(-1.75f, -0.25f),    // piuma
            new Vector2(1.05f, 0.64f), new Vector2(0.2f, -0.85f),      // briciole
        };
        var spotList = new List<Transform>();
        for (int i = 0; i < spots.Length; i++) spotList.Add(Child(photo, "Spot" + i, spots[i]).transform);
        string[] clueSprites = { "v40b_prints", "feather", "v40b_crumbs" };
        float[] clueSizes = { 0.36f, 0.3f, 0.2f };
        var clues = new List<Transform>();
        var marks = new List<SpriteRenderer>();
        var icons = new List<SpriteRenderer>();
        for (int i = 0; i < 3; i++)
        {
            Transform c = Fitted(photo, "Clue" + i, clueSprites[i], spots[i * 2], clueSizes[i], 5);
            Transform m = Art(c, "Mark", "v40b_check", 6, false, new Vector2(0.12f, 0.12f) / c.localScale.x);
            m.localScale = Vector3.one * (0.35f / c.localScale.x);
            clues.Add(c);
            marks.Add(m.GetComponent<SpriteRenderer>());
            Transform ic = Fitted(r, "ClueIcon" + i, clueSprites[i], new Vector2(-0.6f + i * 0.6f, 1.55f), 0.42f, 41);
            icons.Add(ic.GetComponentInChildren<SpriteRenderer>());
        }
        Bar(r, "ClueBar", new Vector2(0f, 1.55f), new Vector2(2.0f, 0.55f), new Color(1f, 1f, 1f, 0.75f), 40);
        Fitted(scene, "Lens", "v40b_magnifier", new Vector2(-2.1f, -4.05f), 0.7f, 40);
        TextMesh zoomText = Label(scene, new Vector2(1.6f, -4.05f), 40, "ZOOM x1.0", 0.04f);
        zoomText.color = Color.white;

        Bar(r, "CaseBack", new Vector2(0f, 2.35f), new Vector2(5.7f, 0.95f), new Color(0.1f, 0.08f, 0.14f, 0.8f), 40);
        TextMesh caseText = Fit(Label(r, new Vector2(0f, 2.35f), 41, "", 0.05f), 5.4f, 0.8f);
        caseText.color = Color.white;

        SpeechBubble bubble = Bubble(r, new Vector2(0f, 0.95f));
        bubble.transform.localScale = Vector3.one * 0.72f;

        SetField(level, "hud", V40B_Hud(r, 3, 3));
        SetField(level, "bubble", bubble);
        SetField(level, "lineupGroup", lineup);
        SetField(level, "sceneGroup", scene);
        SetArray(level, "suspects", new Object[] { cat, dog, parrot });
        SetArray(level, "ticks", ticks.ToArray());
        SetField(level, "catWet", wet);
        SetField(level, "catCrumbs", crumbs);
        SetField(level, "dogMud", mud);
        SetField(level, "parrotWing", wing);
        SetField(level, "caseText", caseText);
        SetField(level, "photo", photo);
        SetArray(level, "clues", clues.ToArray());
        SetArray(level, "clueSpots", spotList.ToArray());
        SetArray(level, "clueMarks", marks.ToArray());
        SetArray(level, "clueIcons", icons.ToArray());
        SetField(level, "zoomText", zoomText);
        SetField(level, "cuffs", cuffs);
        return SaveLevel(root, "Detective");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V40B = new Dictionary<string, string[]>
    {
        { "kitchen_escape", new[] { "The drawer opens. In the tray, drop one item onto another to combine them.", "String on magnet, then fish in the drain. Key on the lock, then pinch the white child-lock with two fingers." } },
        { "space_station", new[] { "Energy must flow from the reactor (left) to the dish (bottom right). Then tilt slowly and HOLD still.", "Decode each blink group with the table: short = small dash, long = long dash. Gaps separate the 3 digits." } },
        { "treasure_map", new[] { "The island compass is turned: N is where its red arrow points, not up. Wrong steps send you back.", "Rub the X fast. For the lock, tap the little map (top left): corner symbols 1, 2, 3 in order." } },
        { "detective", new[] { "Compare each alibi with the case fact and with how the suspect LOOKS.", "Dry dog in the rain, crumbs on the cookie-hater, or a 'flightless' parrot that's flying: cuff the liar." } },
    };
}
#endif
