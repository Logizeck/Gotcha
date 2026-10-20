#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli della specifica 105-200 (in gioco dal 151), pacchetto E.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels41E() => new List<LevelDefinition>
    {
        Define("PixelRebellion", "pixel_rebellion", "PIXEL REBELLION", "The creature is smaller than it looks.", BuildS41EPixelRebellion(), "pixel_rebellion"),
        Define("ColdCoffee", "cold_coffee", "COLD COFFEE", "Coffee needs warmth.", BuildS41EColdCoffee()),
        Define("TheMissingSecond", "the_missing_second", "THE MISSING SECOND", "You only need one more second.", BuildS41EMissingSecond()),
        Define("MirrorTwins", "mirror_twins", "MIRROR TWINS", "Reflection can be useful.", BuildS41EMirrorTwins()),
        Define("TheHungryPrinter", "the_hungry_printer", "THE HUNGRY PRINTER", "The printer doesn't need special paper.", BuildS41EHungryPrinter()),
        Define("FingerMaze", "finger_maze", "FINGER MAZE", "Your finger is blocking the view.", BuildS41EFingerMaze()),
        Define("FalseFinish", "false_finish", "FALSE FINISH", "The race isn't really over.", BuildS41EFalseFinish()),
        Define("Heartbeat", "heartbeat", "HEARTBEAT", "Listen with your hands.", BuildS41EHeartbeat()),
    };

    // ---------- Aiuti comuni del pacchetto E ----------

    /// <summary>Riga dell'obiettivo in alto, su una fascia scura.</summary>
    static TextMesh S41E_Goal(Transform root, string text)
    {
        Transform h = Child(root, "Goal", new Vector2(0f, 3.25f)).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(5.5f, 0.5f), new Color(0.12f, 0.09f, 0.16f, 0.72f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, text, 0.05f);
        t.color = Color.white;
        Fit(t, 5.3f, 0.42f);
        return t;
    }

    /// <summary>Etichetta bianca su fascia scura (contatori).</summary>
    static TextMesh S41E_Info(Transform root, string name, Vector2 pos, float width)
    {
        Transform h = Child(root, name, pos).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(width, 0.4f), new Color(0.12f, 0.09f, 0.16f, 0.6f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, "", 0.04f);
        t.color = Color.white;
        Fit(t, width - 0.2f, 0.32f);
        return t;
    }

    static SmokeEmitter[] S41E_Confetti(Transform root)
    {
        var list = new List<SmokeEmitter>();
        foreach (Color c in new[] { new Color(1f, 0.36f, 0.54f), new Color(1f, 0.82f, 0.35f), new Color(0.43f, 0.78f, 1f), new Color(0.5f, 0.83f, 0.42f) })
        {
            SmokeEmitter e = Emitter(root, "Confetti", new Vector2(0f, 3.6f), "confetti", 22f, 2.4f, -1.6f, 2.8f, 1.4f, c, 80);
            e.sideDrift = 0.6f;
            e.growth = 0f;
            e.spinRange = 300f;
            list.Add(e);
        }
        return list.ToArray();
    }

    // ---------- 138 Pixel rebellion ----------

    static GameObject BuildS41EPixelRebellion()
    {
        var root = new GameObject("Level_PixelRebellion");
        PixelRebellionLevel level = root.AddComponent<PixelRebellionLevel>();
        Bar(root.transform, "Black", Vector2.zero, new Vector2(12f, 16f), new Color(0.02f, 0.02f, 0.05f), -30);
        Transform world = Child(root.transform, "World", Vector2.zero).transform;
        Art(world, "Space", "s41e_pixel_space", -20, false);
        Transform critter = Child(world, "Critter", new Vector2(1.35f, -2.2f)).transform;
        critter.localScale = Vector3.one * 0.2f;
        SpriteRenderer critterArt = Art(critter, "Art", "s41e_pixel_critter", 5, false).GetComponent<SpriteRenderer>();
        SpriteRenderer glow = Art(critter, "Glow", "glow", 3, false).GetComponent<SpriteRenderer>();
        glow.transform.localScale = Vector3.one * 1.6f;
        glow.color = new Color(0.45f, 1f, 0.6f, 0.7f);
        SpriteRenderer dot = Bar(critter, "Dot", Vector2.zero, new Vector2(0.32f, 0.32f), new Color(0.7f, 1f, 0.8f), 4);

        Transform leader = Child(root.transform, "Leader", new Vector2(0f, -3.2f)).transform;
        Art(leader, "Podium", "s41e_podium", 52, false, new Vector2(0.1f, -1.0f));
        Art(leader, "Pixel", "s41e_pixel_leader", 53, false, new Vector2(0f, 0.65f));
        TextMesh union = Fit(Label(leader, new Vector2(0f, -0.58f), 54, "UNION", 0.045f), 0.8f, 0.25f);
        union.color = new Color(0.55f, 0.15f, 0.1f);

        TextMesh goal = S41E_Goal(root.transform, "FIND THE CREATURE");
        SetField(level, "world", world);
        SetField(level, "critter", critter);
        SetField(level, "leader", leader);
        SetField(level, "critterArt", critterArt);
        SetField(level, "dot", dot);
        SetField(level, "dotGlow", glow);
        SetField(level, "madSprite", sprites["s41e_pixel_critter_mad"]);
        SetField(level, "pixelSprite", sprites["plain"]);
        SetField(level, "zoomText", S41E_Info(root.transform, "Zoom", new Vector2(-1.4f, 2.7f), 2.5f));
        SetField(level, "missText", S41E_Info(root.transform, "Misses", new Vector2(1.4f, 2.7f), 2.7f));
        SetField(level, "goalRoot", goal.transform.parent.gameObject);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0f, -0.9f)));
        SetArray(level, "confetti", S41E_Confetti(root.transform));
        return SaveLevel(root, "PixelRebellion");
    }

    // ---------- 139 Cold coffee ----------

    static GameObject BuildS41EColdCoffee()
    {
        var root = new GameObject("Level_ColdCoffee");
        ColdCoffeeLevel level = root.AddComponent<ColdCoffeeLevel>();
        Art(root.transform, "Office", "s41e_office_bg", -20, false);
        Transform worker = Child(root.transform, "Worker", new Vector2(-1.3f, -1.9f)).transform;
        SpriteRenderer workerArt = Art(worker, "Art", "s41e_worker_angry", 1, false).GetComponent<SpriteRenderer>();
        Art(root.transform, "Desk", "s41e_desk", 3, false, new Vector2(0f, -1.85f));
        Art(root.transform, "Lamp", "s41e_heat_lamp", 4, false, new Vector2(2.1f, -1.85f));
        Transform coneHolder = Child(root.transform, "Cone", new Vector2(1.3f, 0.15f)).transform;
        coneHolder.localScale = Vector3.one * 0.38f;
        SpriteRenderer cone = Art(coneHolder, "Art", "light_cone", 6, false).GetComponent<SpriteRenderer>();
        cone.color = new Color(1f, 0.82f, 0.35f, 0f);
        SpriteRenderer bulb = Art(root.transform, "Bulb", "glow", 7, false, new Vector2(1.3f, 0.15f)).GetComponent<SpriteRenderer>();
        bulb.transform.localScale = Vector3.one * 0.9f;
        bulb.color = new Color(1f, 0.75f, 0.3f, 0f);
        Transform spot = Child(root.transform, "LampSpot", new Vector2(1.3f, -1.85f)).transform;
        SpriteRenderer spill = Bar(root.transform, "Spill", new Vector2(1.3f, -1.95f), new Vector2(2.4f, 0.16f), new Color(0.42f, 0.23f, 0.12f), 9);

        Transform cup = Child(root.transform, "Cup", new Vector2(-0.3f, -1.85f)).transform;
        SpriteRenderer cupArt = Art(cup, "Art", "s41e_cup_frozen", 8, false).GetComponent<SpriteRenderer>();
        BoxCollider2D cupCol = cup.gameObject.AddComponent<BoxCollider2D>();
        cupCol.size = new Vector2(1.0f, 1.1f);
        cupCol.offset = new Vector2(0f, 0.45f);
        SmokeEmitter steam = Emitter(cup, "Steam", new Vector2(0f, 0.75f), "puff", 4f, 1.4f, 0.6f, 0.15f, 0.25f, new Color(1f, 1f, 1f, 0.7f), 9);

        Transform heat = Child(root.transform, "Heat", new Vector2(1.3f, -3.3f)).transform;
        Art(heat, "Art", "s41e_heat_btn", 10, false);
        TextMesh heatLabel = Fit(Label(root.transform, new Vector2(1.3f, -4.25f), 11, "HOLD TO HEAT", 0.045f), 2.2f, 0.3f);
        heatLabel.color = Color.white;
        heat.gameObject.AddComponent<CircleCollider2D>().radius = 0.75f;

        // termometro
        Transform thermo = Child(root.transform, "Thermo", new Vector2(-2.45f, 0.9f)).transform;
        thermo.localScale = Vector3.one * 0.75f;
        Bar(thermo, "Back", new Vector2(0f, 0.12f), new Vector2(0.26f, 3.16f), new Color(0.95f, 0.97f, 1f), 1);
        Bar(thermo, "Green", new Vector2(0f, 0.64f), new Vector2(0.26f, 0.54f), new Color(0.35f, 0.85f, 0.4f, 0.55f), 2);
        Transform fill = Child(thermo, "Fill", new Vector2(0f, -1.46f)).transform;
        Bar(fill, "Bar", new Vector2(0f, 1.58f), new Vector2(0.26f, 3.16f), new Color(1f, 0.3f, 0.3f), 3);
        Art(thermo, "Glass", "s41e_thermo", 4, false);
        Bar(thermo, "GreenMark", new Vector2(-0.33f, 0.64f), new Vector2(0.12f, 0.54f), new Color(0.3f, 0.8f, 0.35f), 5);
        TextMesh temp = Fit(Label(root.transform, new Vector2(-2.45f, 2.65f), 11, "", 0.05f), 0.9f, 0.35f);

        S41E_Goal(root.transform, "WARM UP THE FROZEN COFFEE");
        SetField(level, "cup", cup);
        SetField(level, "lampSpot", spot);
        SetField(level, "heatButton", heat);
        SetField(level, "thermoFill", fill);
        SetField(level, "worker", worker);
        SetField(level, "cupArea", cupCol);
        SetField(level, "heatArea", heat.GetComponent<Collider2D>());
        SetField(level, "cupArt", cupArt);
        SetField(level, "workerArt", workerArt);
        SetField(level, "cone", cone);
        SetField(level, "bulbGlow", bulb);
        SetField(level, "spill", spill);
        SetField(level, "hotCupSprite", sprites["s41e_cup_hot"]);
        SetField(level, "happyWorkerSprite", sprites["s41e_worker_happy"]);
        SetField(level, "steam", steam);
        SetField(level, "tempText", temp);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.5f, 0.55f)));
        SetArray(level, "confetti", S41E_Confetti(root.transform));
        return SaveLevel(root, "ColdCoffee");
    }

    // ---------- 140 The missing second ----------

    static GameObject BuildS41EMissingSecond()
    {
        var root = new GameObject("Level_TheMissingSecond");
        TheMissingSecondLevel level = root.AddComponent<TheMissingSecondLevel>();
        Art(root.transform, "Hall", "s41e_noon_bg", -20, false);
        Fit(Label(root.transform, new Vector2(1.3f, 0.55f), 2, "CHEESE BAR", 0.05f), 1.8f, 0.4f).color = new Color(0.55f, 0.3f, 0.1f);
        Transform shutter = Child(root.transform, "Shutter", new Vector2(1.3f, 0f)).transform;
        Art(shutter, "Art", "s41e_noon_shutter", 3, false);
        Fit(Label(shutter, new Vector2(0f, -1.15f), 4, "OPENS\nAT NOON", 0.04f), 1.1f, 0.5f);
        GameObject doorArea = Child(root.transform, "DoorArea", new Vector2(1.3f, -1.3f));
        doorArea.AddComponent<BoxCollider2D>().size = new Vector2(1.8f, 2.6f);
        Transform mouse = Child(root.transform, "Mouse", new Vector2(-0.3f, -2.1f)).transform;
        mouse.localScale = Vector3.one * 0.7f;
        Art(mouse, "Art", "mouse", 5, false);

        Transform panel = Child(root.transform, "Clock", new Vector2(0f, 1.6f)).transform;
        Art(panel, "Art", "s41e_clock_panel", 5, false);
        float[] xs = { -1.8f, -1.2f, -0.3f, 0.3f, 1.2f, 1.8f };
        Color lcd = new Color(0.45f, 1f, 0.55f);
        var digits = new List<TextMesh>();
        var ghosts = new List<TextMesh>();
        var areas = new List<Collider2D>();
        for (int i = 0; i < 6; i++)
        {
            TextMesh d = Fit(Label(panel, new Vector2(xs[i], 0f), 7, "0", 0.12f), 0.55f, 0.9f);
            d.color = lcd;
            digits.Add(d);
            TextMesh g = Fit(Label(panel, new Vector2(xs[i], 0f), 7, "", 0.12f), 0.55f, 0.9f);
            g.color = lcd;
            ghosts.Add(g);
            GameObject a = Child(panel, "Digit" + i, new Vector2(xs[i], 0f));
            a.AddComponent<BoxCollider2D>().size = new Vector2(0.58f, 1.6f);
            areas.Add(a.GetComponent<Collider2D>());
        }
        foreach (float x in new[] { -0.75f, 0.75f })
            Fit(Label(panel, new Vector2(x, 0.05f), 7, ":", 0.12f), 0.3f, 0.9f).color = lcd;

        S41E_Goal(root.transform, "OPEN THE DOOR AT NOON");
        SetArray(level, "digits", digits.ToArray());
        SetArray(level, "ghosts", ghosts.ToArray());
        SetArray(level, "digitAreas", areas.ToArray());
        SetField(level, "shutter", shutter);
        SetField(level, "mouse", mouse);
        SetField(level, "panel", panel);
        SetField(level, "doorArea", doorArea.GetComponent<Collider2D>());
        SetField(level, "wrongText", S41E_Info(root.transform, "Wrong", new Vector2(0f, 2.72f), 3.2f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.1f, -0.5f)));
        SetArray(level, "confetti", S41E_Confetti(root.transform));
        return SaveLevel(root, "TheMissingSecond");
    }

    // ---------- 141 Mirror twins ----------

    static GameObject BuildS41EMirrorTwins()
    {
        var root = new GameObject("Level_MirrorTwins");
        MirrorTwinsLevel level = root.AddComponent<MirrorTwinsLevel>();
        Art(root.transform, "Hall", "s41e_twins_bg", -20, false);
        Transform door = Child(root.transform, "Door", new Vector2(2.1f, -2.4f)).transform;
        SpriteRenderer doorArt = Art(door, "Art", "s41e_twin_door", 2, false).GetComponent<SpriteRenderer>();
        Transform lockPoint = Child(root.transform, "Lock", new Vector2(1.85f, -1.0f)).transform;
        SpriteRenderer glow = Art(root.transform, "LockGlow", "glow", 3, false, new Vector2(1.85f, -1.0f)).GetComponent<SpriteRenderer>();
        glow.transform.localScale = Vector3.one * 1.3f;
        glow.color = new Color(1f, 0.9f, 0.4f, 0f);
        Transform twinA = Child(root.transform, "TwinA", new Vector2(-2.0f, -2.4f)).transform;
        Art(twinA, "Art", "s41e_twin_hold", 4, false);
        Transform twinB = Child(root.transform, "TwinB", new Vector2(0.9f, -2.4f)).transform;
        Art(twinB, "Art", "s41e_twin_wave", 4, false);
        Transform key = Child(root.transform, "Key", new Vector2(-1.54f, -1.0f)).transform;
        key.localScale = Vector3.one * 0.5f;
        Art(key, "Art", "key", 5, false, new Vector2(0.6f, 0f));
        Transform reflection = Child(root.transform, "Reflection", Vector2.zero).transform;
        reflection.localScale = new Vector3(-0.5f, 0.5f, 1f);
        SpriteRenderer reflectionArt = Art(reflection, "Art", "key", 7, false, new Vector2(0.6f, 0f)).GetComponent<SpriteRenderer>();
        reflectionArt.color = new Color(0.6f, 0.85f, 1f, 0.6f);
        Transform mirror = Child(root.transform, "Mirror", new Vector2(-0.9f, 1.7f)).transform;
        Art(mirror, "Art", "s41e_stand_mirror", 6, false);
        mirror.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, 2.3f);
        Transform timer = GaugeBar(root.transform, "Timer", new Vector2(0f, 2.75f), new Vector2(4.2f, 0.16f), new Color(0.15f, 0.12f, 0.2f, 0.7f), new Color(1f, 0.85f, 0.3f), 30);

        S41E_Goal(root.transform, "OPEN THE DOOR FOR YOUR TWIN");
        SetField(level, "mirror", mirror);
        SetField(level, "key", key);
        SetField(level, "reflection", reflection);
        SetField(level, "lockPoint", lockPoint);
        SetField(level, "door", door);
        SetField(level, "twinA", twinA);
        SetField(level, "twinB", twinB);
        SetField(level, "timerFill", timer);
        SetField(level, "mirrorArea", mirror.GetComponent<Collider2D>());
        SetField(level, "reflectionArt", reflectionArt);
        SetField(level, "doorArt", doorArt);
        SetField(level, "glow", glow);
        SetField(level, "openDoorSprite", sprites["s41e_twin_door_open"]);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.3f, 0.2f)));
        SetArray(level, "confetti", S41E_Confetti(root.transform));
        return SaveLevel(root, "MirrorTwins");
    }

    // ---------- 142 The hungry printer ----------

    static GameObject BuildS41EHungryPrinter()
    {
        var root = new GameObject("Level_TheHungryPrinter");
        HungryPrinterLevel level = root.AddComponent<HungryPrinterLevel>();
        Art(root.transform, "Office", "s41e_office_bg", -20, false);
        Art(root.transform, "Shelf", "s41e_shelf", 2, false, Vector2.zero);
        Transform printer = Child(root.transform, "Printer", new Vector2(0f, -3.4f)).transform;
        Art(printer, "Art", "s41e_printer", 4, false);
        Transform feeder = Child(printer, "Feeder", new Vector2(0f, 1.7f)).transform;
        TextMesh display = Fit(Label(printer, new Vector2(0.6f, 1.12f), 5, "", 0.035f), 1.0f, 0.3f);
        display.color = new Color(0.1f, 0.3f, 0.15f);
        SmokeEmitter smoke = Emitter(printer, "Smoke", new Vector2(0f, 1.6f), "puff", 8f, 1.2f, 1.2f, 0.6f, 0.4f, new Color(0.45f, 0.45f, 0.5f, 0.9f), 9);

        var items = new List<Transform>();
        var areas = new List<Collider2D>();
        Transform sheet = Child(root.transform, "Instructions", new Vector2(0f, 2.1f)).transform;
        Art(sheet, "Art", "s41e_instr_sheet", 6, false);
        Fit(Label(sheet, new Vector2(0f, 0.3f), 7, "INSTRUCTIONS", 0.05f), 1.6f, 0.3f).color = new Color(0.75f, 0.15f, 0.2f);
        Fit(Label(sheet, new Vector2(-0.1f, -0.2f), 7, "Give paper to\nthe printer!", 0.035f), 1.5f, 0.5f);
        BoxCollider2D sc = sheet.gameObject.AddComponent<BoxCollider2D>();
        sc.size = new Vector2(1.9f, 1.25f);
        items.Add(sheet);
        areas.Add(sc);
        string[] stuff = { "sock", "fish", "cookie", "apple" };
        for (int i = 0; i < stuff.Length; i++)
        {
            Transform it = Child(root.transform, "Item" + i, new Vector2(-1.8f + i * 1.2f, 0.45f)).transform;
            Fitted(it, "Art", stuff[i], Vector2.zero, 0.8f, 6);
            CircleCollider2D c = it.gameObject.AddComponent<CircleCollider2D>();
            c.radius = 0.5f;
            items.Add(it);
            areas.Add(c);
        }
        var pages = new List<Transform>();
        for (int i = 0; i < 4; i++)
        {
            Transform pg = Child(root.transform, "Page" + i, Vector2.zero).transform;
            pg.localScale = Vector3.one * 0.6f;
            Art(pg, "Art", "s41e_blank_page", 5, false);
            pages.Add(pg);
        }
        Transform map = Child(root.transform, "Map", new Vector2(0f, 0.4f)).transform;
        map.localScale = Vector3.one * 2.4f;
        Art(map, "Art", "s41e_map_sheet", 20, false);

        S41E_Goal(root.transform, "PRINT THE TREASURE MAP");
        SetField(level, "printer", printer);
        SetField(level, "feeder", feeder);
        SetField(level, "mapSheet", map);
        SetArray(level, "items", items.ToArray());
        SetArray(level, "itemAreas", areas.ToArray());
        SetArray(level, "blankPages", pages.ToArray());
        SetField(level, "display", display);
        SetField(level, "jamText", S41E_Info(root.transform, "Jams", new Vector2(0f, -4.66f), 2.4f));
        SetField(level, "smoke", smoke);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.9f, -1.0f)));
        SetArray(level, "confetti", S41E_Confetti(root.transform));
        return SaveLevel(root, "TheHungryPrinter");
    }

    // ---------- 143 Finger maze ----------

    static GameObject BuildS41EFingerMaze()
    {
        var root = new GameObject("Level_FingerMaze");
        FingerMazeLevel level = root.AddComponent<FingerMazeLevel>();
        Art(root.transform, "Board", "s41e_fmaze_bg", -20, false);
        Transform wallsRoot = Child(root.transform, "Walls", Vector2.zero).transform;
        var walls = new List<Collider2D>();
        foreach (float[] b in Gotcha.EditorArt.DoodleSprites.S41E_MazeBoxes())
        {
            GameObject w = Child(wallsRoot, "Wall", new Vector2(b[0], b[1]));
            BoxCollider2D c = w.AddComponent<BoxCollider2D>();
            c.size = new Vector2(b[2] * 2f, b[3] * 2f);
            walls.Add(c);
        }
        Transform cheese = Child(root.transform, "Cheese", new Vector2(1.8f, 2.4f)).transform;
        cheese.localScale = Vector3.one * 0.7f;
        Art(cheese, "Art", "cheese_wedge", 11, false);
        Transform exit = Child(root.transform, "Exit", new Vector2(1.8f, 2.7f)).transform;
        Transform start = Child(root.transform, "Start", new Vector2(-1.8f, -3.6f)).transform;
        Transform mover = Child(root.transform, "Mouse", new Vector2(-1.8f, -3.6f)).transform;
        mover.localScale = Vector3.one * 0.35f;
        SpriteRenderer moverArt = Art(mover, "Art", "mouse", 11, false, new Vector2(0f, -0.15f)).GetComponent<SpriteRenderer>();
        SpriteRenderer print = Art(root.transform, "Print", "s41e_fingerprint", 10, false, new Vector2(0f, -0.9f)).GetComponent<SpriteRenderer>();
        GameObject printArea = Child(root.transform, "PrintArea", new Vector2(0f, -0.9f));
        printArea.AddComponent<BoxCollider2D>().size = new Vector2(5.4f, 7.2f);

        S41E_Goal(root.transform, "GUIDE THE MOUSE TO THE CHEESE");
        SetField(level, "mover", mover);
        SetField(level, "startPoint", start);
        SetField(level, "exitPoint", exit);
        SetArray(level, "walls", walls.ToArray());
        SetField(level, "printArea", printArea.GetComponent<Collider2D>());
        SetField(level, "printArt", print);
        SetField(level, "moverArt", moverArt);
        SetField(level, "bumpText", S41E_Info(root.transform, "Bumps", new Vector2(0f, -4.66f), 2.4f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.9f, 2.65f)));
        SetArray(level, "confetti", S41E_Confetti(root.transform));
        return SaveLevel(root, "FingerMaze");
    }

    // ---------- 144 False finish ----------

    static GameObject BuildS41EFalseFinish()
    {
        var root = new GameObject("Level_FalseFinish");
        FalseFinishLevel level = root.AddComponent<FalseFinishLevel>();
        Art(root.transform, "Stadium", "s41e_race_bg", -20, false);
        Fit(Label(root.transform, new Vector2(1.9f, 2.05f), 2, "FINISH", 0.035f), 0.8f, 0.3f);
        Transform hero = Child(root.transform, "Hero", new Vector2(-2.2f, 0.2f)).transform;
        Art(hero, "Art", "s41e_runner", 5, false);
        Transform mouse = Child(root.transform, "Mouse", new Vector2(-2.2f, -1.25f)).transform;
        mouse.localScale = Vector3.one * 0.55f;
        Art(mouse, "Art", "mouse", 6, false);
        CircleCollider2D mc = mouse.gameObject.AddComponent<CircleCollider2D>();
        mc.radius = 1.0f;
        mc.offset = new Vector2(0f, 0.15f);

        Transform banner = Child(root.transform, "Banner", new Vector2(0f, -1.2f)).transform;
        Bar(banner, "Ribbon", new Vector2(0f, 0.1f), new Vector2(6.2f, 1.5f), new Color(1f, 0.82f, 0.3f, 0.55f), 7);
        TextMesh win = Fit(Label(banner, new Vector2(0f, 0.1f), 9, "YOU WIN!", 0.2f), 5.4f, 1.4f);
        win.color = new Color(0.85f, 0.15f, 0.25f);
        Transform next = Child(banner, "Next", new Vector2(0f, -2.1f)).transform;
        Art(next, "Art", "s41e_next_btn", 10, false);
        Fit(Label(next, new Vector2(-0.15f, 0.02f), 11, "NEXT", 0.07f), 1.2f, 0.5f).color = Color.white;
        BoxCollider2D nc = next.gameObject.AddComponent<BoxCollider2D>();
        nc.size = new Vector2(1.9f, 0.9f);

        Transform trophy = Child(root.transform, "Trophy", new Vector2(2.3f, -0.75f)).transform;
        trophy.localScale = Vector3.one * 0.7f;
        Art(trophy, "Art", "s41e_trophy", 8, false);
        TextMesh realWin = Fit(Label(root.transform, new Vector2(0f, -3.3f), 12, "REAL WINNER: MOUSE!", 0.09f), 5.2f, 0.8f);
        realWin.color = new Color(0.85f, 0.15f, 0.25f);
        TextMesh countdown = Fit(Label(root.transform, new Vector2(0f, -0.6f), 20, "", 0.22f), 3f, 1.4f);
        countdown.color = new Color(1f, 0.95f, 0.5f);

        S41E_Goal(root.transform, "WIN THE RACE");
        SetField(level, "hero", hero);
        SetField(level, "mouse", mouse);
        SetField(level, "banner", banner);
        SetField(level, "nextButton", next);
        SetField(level, "trophy", trophy);
        SetField(level, "nextArea", nc);
        SetField(level, "mouseArea", mc);
        SetField(level, "countdown", countdown);
        SetField(level, "falseText", S41E_Info(root.transform, "Early", new Vector2(0f, 2.72f), 3.0f));
        SetField(level, "realWin", realWin);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.6f, 1.75f)));
        SetArray(level, "confetti", S41E_Confetti(root.transform));
        return SaveLevel(root, "FalseFinish");
    }

    // ---------- 145 Heartbeat ----------

    static GameObject BuildS41EHeartbeat()
    {
        var root = new GameObject("Level_Heartbeat");
        HeartbeatLevel level = root.AddComponent<HeartbeatLevel>();
        Art(root.transform, "Ward", "s41e_ward_bg", -20, false);
        Art(root.transform, "Monitor", "s41e_monitor", 2, false, new Vector2(0f, 1.6f));
        LineRenderer trace = Line(root.transform, "Trace", 0.05f, new Color(0.4f, 1f, 0.5f), 4);
        TextMesh monitorText = Fit(Label(root.transform, new Vector2(1.2f, 2.1f), 4, "", 0.035f), 1.4f, 0.3f);
        monitorText.color = new Color(0.4f, 1f, 0.5f);
        Transform monster = Child(root.transform, "Monster", new Vector2(0f, -1.6f)).transform;
        SpriteRenderer monsterArt = Art(monster, "Art", "s41e_monster_sleep", 3, false).GetComponent<SpriteRenderer>();
        Transform heart = Child(root.transform, "Heart", new Vector2(0f, -3.5f)).transform;
        Art(heart, "Art", "s41e_heart_btn", 6, false);
        heart.gameObject.AddComponent<CircleCollider2D>().radius = 0.8f;
        Transform sound = Child(root.transform, "Sound", new Vector2(2.1f, -3.6f)).transform;
        SpriteRenderer icon = Art(sound, "Art", "s41e_speaker_off", 6, false).GetComponent<SpriteRenderer>();
        Fit(Label(sound, new Vector2(0f, -0.55f), 7, "SOUND", 0.035f), 0.9f, 0.25f);
        sound.gameObject.AddComponent<CircleCollider2D>().radius = 0.55f;

        S41E_Goal(root.transform, "RESTART HIS HEARTBEAT");
        SetField(level, "heart", heart);
        SetField(level, "soundButton", sound);
        SetField(level, "monster", monster);
        SetField(level, "heartArea", heart.GetComponent<Collider2D>());
        SetField(level, "soundArea", sound.GetComponent<Collider2D>());
        SetField(level, "monsterArt", monsterArt);
        SetField(level, "soundIcon", icon);
        SetField(level, "awakeSprite", sprites["s41e_monster_awake"]);
        SetField(level, "soundOnSprite", sprites["s41e_speaker_on"]);
        SetField(level, "soundOffSprite", sprites["s41e_speaker_off"]);
        SetField(level, "trace", trace);
        SetField(level, "wrongText", S41E_Info(root.transform, "Wrong", new Vector2(0f, -4.66f), 3.0f));
        SetField(level, "monitorText", monitorText);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.2f, 0.3f)));
        SetArray(level, "confetti", S41E_Confetti(root.transform));
        return SaveLevel(root, "Heartbeat");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V41E = new Dictionary<string, string[]>
    {
        { "pixel_rebellion", new[] { "Zoom in.", "Enlarge the glowing pixel and tap the creature." } },
        { "cold_coffee", new[] { "Use the lamp.", "Put the cup beneath the lamp and stop heating in the green zone." } },
        { "the_missing_second", new[] { "The display can move.", "Swipe the seconds upward once." } },
        { "mirror_twins", new[] { "Align the reflected key.", "Move the mirror until the reflected key matches the lock." } },
        { "the_hungry_printer", new[] { "Look for something made of paper.", "Feed the instructions sheet into the printer." } },
        { "finger_maze", new[] { "Keep one finger on the print.", "Hold the fingerprint with one finger and guide the character with another." } },
        { "false_finish", new[] { "Someone is still running.", "Wait for the mouse and tap him at the finish line." } },
        { "heartbeat", new[] { "The vibration has a rhythm.", "Repeat two short beats followed by one long beat." } },
    };
}
#endif
