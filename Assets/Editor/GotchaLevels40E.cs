#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>Livelli compositi della versione 0.40 (gruppo E).</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels40E() => new List<LevelDefinition>
    {
        Define("Elevator", "elevator_logic", "ELEVATOR LOGIC", "Read every clue: only one way works.", BuildElevatorLevel()),
        Define("Circuit", "circuit_board", "CIRCUIT BOARD", "Light all three. Gently.", BuildCircuitLevel()),
        Define("SnowGlobe", "snow_globe", "SNOW GLOBE", "Shake it and watch the snow settle.", BuildSnowGlobeLevel()),
        Define("Knight", "knight_quest", "KNIGHT'S QUEST", "Knights jump in an L. Sometimes backwards.", BuildKnightLevel()),
        Define("Gauntlet", "the_gauntlet", "THE GAUNTLET", "Everything you learned, all at once.", BuildGauntletLevel()),
    };

    // ---------- Aiuti comuni del gruppo E ----------

    /// <summary>Barra in alto: cuori a sinistra, pallini delle fasi a destra, riga dell'obiettivo sotto.</summary>
    static void V40E_Hud(Transform root, int stageCount, int heartCount, out SpriteRenderer[] dots, out SpriteRenderer[] hearts, out TextMesh goal)
    {
        var d = new List<SpriteRenderer>();
        for (int i = 0; i < stageCount; i++)
            d.Add(Art(root, "Dot" + i, "e40_dot", 32, false, new Vector2(1.45f + (i - (stageCount - 1) * 0.5f) * 0.4f, 3.35f)).GetComponent<SpriteRenderer>());
        dots = d.ToArray();
        var h = new List<SpriteRenderer>();
        for (int i = 0; i < heartCount; i++)
        {
            Transform t = Art(root, "Life" + i, "heart", 32, false, new Vector2(-2.35f + i * 0.42f, 3.35f));
            t.localScale = Vector3.one * 0.7f;
            h.Add(t.GetComponent<SpriteRenderer>());
        }
        hearts = h.ToArray();
        Bar(root, "GoalStrip", new Vector2(0f, 2.88f), new Vector2(5.7f, 0.5f), new Color(0.1f, 0.08f, 0.14f, 0.72f), 30);
        goal = Fit(Label(root, new Vector2(0f, 2.88f), 31, "", 0.045f), 5.4f, 0.42f);
        goal.color = Color.white;
    }

    static SpriteRenderer V40E_Veil(Transform root)
    {
        SpriteRenderer v = Bar(root, "Veil", Vector2.zero, new Vector2(12f, 16f), new Color(0.05f, 0.03f, 0.08f, 0f), 100);
        v.enabled = false;
        return v;
    }

    // ---------- Elevator logic ----------

    static GameObject BuildElevatorLevel()
    {
        var root = new GameObject("Level_Elevator");
        ElevatorLevel level = root.AddComponent<ElevatorLevel>();
        Art(root.transform, "Tower", "e40_tower_bg", -20, false);
        const float b0 = -4.25f, h = 0.95f, carX = -1.975f;
        var areas = new List<Collider2D>();
        var lights = new List<SpriteRenderer>();
        for (int k = 0; k <= 5; k++)
        {
            float y0 = b0 + k * h;
            GameObject area = Child(root.transform, "Floor" + k, new Vector2(0.62f, y0 + h * 0.5f));
            area.AddComponent<BoxCollider2D>().size = new Vector2(3.9f, 0.92f);
            areas.Add(area.GetComponent<Collider2D>());
            lights.Add(Bar(root.transform, "Light" + k, new Vector2(0.62f, y0 + 0.5f), new Vector2(3.86f, 0.8f), Color.clear, -10));
            Transform btn = Art(root.transform, "Button" + k, "e40_floor_btn", -8, false, new Vector2(-1.05f, y0 + 0.55f));
            btn.localScale = Vector3.one * 0.9f;
            Label(root.transform, new Vector2(-1.05f, y0 + 0.54f), -7, k == 0 ? "L" : k.ToString(), 0.04f);
        }
        Transform car = Child(root.transform, "Car", new Vector2(carX, b0 + 0.04f)).transform;
        Art(car, "Art", "e40_car", 2, false);

        string[] names = { "e40_cat", "e40_dog", "e40_parrot", "e40_bunny" };
        float[] slotX = { -0.75f, 0f, 0.75f, 1.5f };
        var animals = new List<Transform>();
        for (int i = 0; i < 4; i++)
        {
            Transform a = Child(root.transform, "Animal" + i, new Vector2(slotX[i], b0 + 0.1f)).transform;
            a.localScale = Vector3.one * 0.85f;
            Art(a, "Art", names[i], 5, false);
            animals.Add(a);
        }

        Art(root.transform, "Clues", "e40_clue_board", 1, false, new Vector2(0f, 2.02f));
        TextMesh clues = Label(root.transform, new Vector2(-2.2f, 2.0f), 2, "", 0.03f);
        clues.anchor = TextAnchor.MiddleLeft;
        clues.alignment = TextAlignment.Left;
        Fit(clues, 4.75f, 1.08f);

        V40E_Hud(root.transform, 4, 3, out SpriteRenderer[] dots, out SpriteRenderer[] hearts, out TextMesh goal);
        // i pallini qui sono i quattro inquilini sistemati: ci metto sopra le loro facce
        for (int i = 0; i < 4; i++)
        {
            Transform face = Art(dots[i].transform, "Face", names[i], 33, false, new Vector2(0f, -0.25f));
            face.localScale = Vector3.one * 0.45f;
        }

        SetField(level, "car", car);
        SetArray(level, "animals", animals.ToArray());
        SetArray(level, "floorAreas", areas.ToArray());
        SetArray(level, "floorLights", lights.ToArray());
        SetField(level, "clueText", clues);
        SetArray(level, "dots", dots);
        SetArray(level, "hearts", hearts);
        SetField(level, "goal", goal);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.7f, 0.95f)));
        return SaveLevel(root, "Elevator");
    }

    // ---------- Circuit board ----------

    static GameObject BuildCircuitLevel()
    {
        var root = new GameObject("Level_Circuit");
        CircuitLevel level = root.AddComponent<CircuitLevel>();
        Art(root.transform, "Workshop", "e40_workshop_bg", -20, false);
        Vector2 center = new Vector2(0f, -0.35f);
        Art(root.transform, "Inside", "e40_safe_inside", -15, false, center);
        Transform door = Child(root.transform, "Door", center + new Vector2(-2.75f, 0f)).transform;
        Vector2 hinge = new Vector2(2.75f, 0f);
        Art(door, "Art", "e40_safe_door", -12, false, hinge);
        Art(door, "Pcb", "e40_pcb", -11, false, hinge);
        Vector2 Local(Vector2Int c) => new Vector2(CircuitLevel.Origin.x + c.x * CircuitLevel.Cell, CircuitLevel.Origin.y + c.y * CircuitLevel.Cell) - center + hinge;

        foreach (Vector2Int s in CircuitLevel.Sockets) Art(door, "Socket", "e40_socket", -10, false, Local(s));
        Art(door, "LedSocket", "e40_socket", -10, false, Local(CircuitLevel.LedCell));
        Art(door, "Battery", "e40_battery", 11, false, Local(CircuitLevel.BatteryCell));
        TextMesh volts = Label(door, Local(CircuitLevel.BatteryCell) + new Vector2(0f, -0.12f), 12, "9V", 0.035f);
        volts.color = Color.white;
        SpriteRenderer cross = Art(door, "Cross", "e40_wire_cross", 11, false, Local(CircuitLevel.CrossCell)).GetComponent<SpriteRenderer>();
        SpriteRenderer wire = Art(door, "Wire", "e40_wire_straight", 11, false, Local(CircuitLevel.WireCell)).GetComponent<SpriteRenderer>();
        SpriteRenderer bulbA = Art(door, "BulbA", "e40_bulb_off", 11, false, Local(CircuitLevel.BulbACell)).GetComponent<SpriteRenderer>();
        Transform bHolder = Child(door, "BulbB", Local(CircuitLevel.BulbBCell)).transform;
        bHolder.localRotation = Quaternion.Euler(0, 0, -90f);   // attacco verso ovest
        SpriteRenderer bulbB = Art(bHolder, "Art", "e40_bulb_off", 11, false).GetComponent<SpriteRenderer>();
        TextMesh ledNote = Label(door, Local(new Vector2Int(3, 4)) + new Vector2(0.05f, 0f), 12, "LED\n2V MAX!", 0.03f);
        ledNote.color = new Color(1f, 0.85f, 0.85f);
        Fit(ledNote, 0.85f, 0.7f);

        Transform display = Art(door, "Display", "e40_display", -9, false, new Vector2(0f, 2.32f) - center + hinge);
        display.localScale = Vector3.one * 0.8f;
        var digits = new List<TextMesh>();
        for (int i = 0; i < 3; i++)
        {
            TextMesh dg = Label(door, new Vector2((i - 1) * 0.62f * 0.8f, 2.31f) - center + hinge, -8, "8", 0.07f);
            dg.color = new Color(1f, 0.3f, 0.3f, 0.15f);
            digits.Add(dg);
        }

        // pezzi nel vassoio
        Art(root.transform, "Tray", "e40_tray", 20, false, new Vector2(0f, CircuitLevel.TrayY));
        Transform Piece(string name, string sprite)
        {
            Transform t = Child(root.transform, name, new Vector2(0f, CircuitLevel.TrayY)).transform;
            Art(t, "Art", sprite, 22, false);
            return t;
        }
        var straights = new List<Transform>();
        for (int i = 0; i < 4; i++) straights.Add(Piece("Straight" + i, "e40_wire_straight"));
        var corners = new List<Transform>();
        for (int i = 0; i < 3; i++) corners.Add(Piece("Corner" + i, "e40_wire_corner"));
        Transform sw = Piece("Switch", "e40_switch_off");
        Transform res = Piece("Resistor", "e40_resistor");
        var leds = new List<Transform>();
        for (int i = 0; i < 3; i++) leds.Add(Piece("Led" + i, "e40_led_off"));
        var counts = new List<TextMesh>();
        for (int k = 0; k < 5; k++)
        {
            TextMesh c = Label(root.transform, new Vector2(CircuitLevel.TrayX[k] + 0.3f, CircuitLevel.TrayY - 0.36f), 30, "", 0.035f);
            c.color = Color.white;
            counts.Add(c);
        }

        V40E_Hud(root.transform, 3, 3, out SpriteRenderer[] dots, out SpriteRenderer[] hearts, out TextMesh goal);

        SetArray(level, "straights", straights.ToArray());
        SetArray(level, "corners", corners.ToArray());
        SetField(level, "switchPiece", sw);
        SetField(level, "resistorPiece", res);
        SetArray(level, "leds", leds.ToArray());
        SetField(level, "switchOff", sprites["e40_switch_off"]);
        SetField(level, "switchOn", sprites["e40_switch_on"]);
        SetField(level, "ledOff", sprites["e40_led_off"]);
        SetField(level, "ledOn", sprites["e40_led_on"]);
        SetField(level, "ledBurnt", sprites["e40_led_burnt"]);
        SetField(level, "bulbOff", sprites["e40_bulb_off"]);
        SetField(level, "bulbOn", sprites["e40_bulb_on"]);
        SetField(level, "bulbA", bulbA);
        SetField(level, "bulbB", bulbB);
        SetField(level, "crossArt", cross);
        SetField(level, "wireArt", wire);
        SetArray(level, "digits", digits.ToArray());
        SetArray(level, "counts", counts.ToArray());
        SetField(level, "door", door);
        SetArray(level, "dots", dots);
        SetArray(level, "hearts", hearts);
        SetField(level, "goal", goal);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.9f, 2.25f)));
        return SaveLevel(root, "Circuit");
    }

    // ---------- Snow globe ----------

    static GameObject BuildSnowGlobeLevel()
    {
        var root = new GameObject("Level_SnowGlobe");
        SnowGlobeLevel level = root.AddComponent<SnowGlobeLevel>();
        Art(root.transform, "Room", "e40_cozy_bg", -20, false);
        Art(root.transform, "Shelf", "e40_shelf", -5, false, new Vector2(0f, -3.6f));

        Transform globe = Child(root.transform, "Globe", new Vector2(0f, 0.95f)).transform;
        Art(globe, "Inside", "e40_globe_inside", 0, false);
        var flakes = new List<Transform>();
        for (int i = 0; i < 60; i++) flakes.Add(Art(globe, "Flake", "e40_flake", 3, false));
        SpriteRenderer dome = Art(globe, "Dome", "e40_globe_dome", 6, false).GetComponent<SpriteRenderer>();
        Art(root.transform, "Collar", "e40_globe_collar", 7, false, new Vector2(0f, -0.65f));

        Transform ringGroup = Child(root.transform, "Rings", Vector2.zero).transform;
        var rings = new List<Transform>();
        var glows = new List<SpriteRenderer>();
        var ringDigits = new List<TextMesh>();
        float[] ys = { -1.45f, -2.1f, -2.75f };
        for (int i = 0; i < 3; i++)
        {
            Transform r = Child(ringGroup, "Ring" + i, new Vector2(0f, ys[i])).transform;
            SpriteRenderer g = Bar(r, "Glow", Vector2.zero, new Vector2(3.85f, 0.74f), new Color(1f, 0.95f, 0.4f, 0.55f), 7);
            g.enabled = false;
            glows.Add(g);
            Art(r, "Band", "e40_ring_band", 8, false);
            for (int k = 0; k < 6; k++) ringDigits.Add(Label(r, Vector2.zero, 9, "0", 0.075f));
            rings.Add(r);
        }
        Bar(ringGroup, "Window", new Vector2(0f, -2.1f), new Vector2(0.5f, 2.0f), new Color(1f, 1f, 1f, 0.2f), 10);
        Art(ringGroup, "Arrow", "e40_ring_arrow", 11, false, new Vector2(0f, -1.06f));

        Transform box = Child(root.transform, "MusicBox", new Vector2(0f, -3.6f)).transform;
        SpriteRenderer boxArt = Art(box, "Art", "e40_music_box", 8, false).GetComponent<SpriteRenderer>();
        Transform keyhole = Child(box, "Keyhole", new Vector2(0f, 0.45f)).transform;

        Transform key = Child(root.transform, "Key", Vector2.zero).transform;
        Transform keyScale = Child(key, "Scale", Vector2.zero).transform;
        Art(keyScale, "Art", "key", 14, false);

        var notes = new List<Transform>();
        for (int i = 0; i < 5; i++) notes.Add(Art(root.transform, "Note", "note", 15, false));

        V40E_Hud(root.transform, 3, 3, out SpriteRenderer[] dots, out SpriteRenderer[] hearts, out TextMesh goal);

        SetField(level, "globe", globe);
        SetField(level, "dome", dome);
        SetArray(level, "flakes", flakes.ToArray());
        SetArray(level, "rings", rings.ToArray());
        SetArray(level, "ringDigits", ringDigits.ToArray());
        SetArray(level, "ringGlows", glows.ToArray());
        SetField(level, "ringGroup", ringGroup);
        SetField(level, "musicBox", box);
        SetField(level, "musicBoxArt", boxArt);
        SetField(level, "musicBoxOpen", sprites["e40_music_box_open"]);
        SetField(level, "keyhole", keyhole);
        SetField(level, "key", key);
        SetField(level, "keyArt", keyScale);
        SetArray(level, "notes", notes.ToArray());
        SetArray(level, "dots", dots);
        SetArray(level, "hearts", hearts);
        SetField(level, "goal", goal);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.3f, 2.2f)));
        return SaveLevel(root, "SnowGlobe");
    }

    // ---------- Knight's quest ----------

    static GameObject BuildKnightLevel()
    {
        var root = new GameObject("Level_Knight");
        KnightLevel level = root.AddComponent<KnightLevel>();
        Art(root.transform, "Hall", "e40_hall_bg", -20, false);
        Vector2 o = KnightLevel.Origin;
        const float c = KnightLevel.Cell;
        Art(root.transform, "Board", "e40_chessboard", -10, false, o + new Vector2(2f * c, 2f * c));
        for (int i = 0; i < 5; i++)
        {
            TextMesh col = Label(root.transform, new Vector2(o.x + i * c, o.y - 0.82f), -5, ((char)('a' + i)).ToString(), 0.035f);
            col.color = Color.white;
            TextMesh row = Label(root.transform, new Vector2(o.x - 0.8f, o.y + i * c), -5, (i + 1).ToString(), 0.035f);
            row.color = Color.white;
        }
        Vector2 P(Vector2Int cell) => o + new Vector2(cell.x * c, cell.y * c);
        Transform Piece(string name, string sprite, Vector2Int cell, int order)
        {
            Transform t = Child(root.transform, name, P(cell)).transform;
            Art(t, "Art", sprite, order, false);
            return t;
        }
        Transform knight = Piece("Knight", "e40_knight", Vector2Int.zero, 12);
        var mice = new List<Transform>();
        for (int i = 0; i < KnightLevel.MiceCells.Length; i++) mice.Add(Piece("Mouse" + i, "e40_mouse_pawn", KnightLevel.MiceCells[i], 10));
        Transform king = Piece("King", "e40_cat_king", KnightLevel.KingCell, 10);
        Transform rook = Piece("Rook", "e40_rook", KnightLevel.RookStart, 11);
        Transform yarn = Piece("Yarn", "e40_yarn", KnightLevel.YarnCell, 10);
        var moveDots = new List<Transform>();
        for (int i = 0; i < 16; i++) moveDots.Add(Piece("MoveDot", "e40_move_dot", Vector2Int.zero, 14));
        Transform ring = Piece("Select", "e40_select_ring", Vector2Int.zero, 9);
        ring.GetComponentInChildren<SpriteRenderer>().color = new Color(1f, 0.9f, 0.3f, 0.9f);

        GameObject undo = Child(root.transform, "Undo", new Vector2(-2.0f, -3.95f));
        Art(undo.transform, "Art", "e40_undo", 10, false);
        undo.AddComponent<CircleCollider2D>().radius = 0.45f;
        TextMesh undoLabel = Label(root.transform, new Vector2(-1.25f, -3.95f), 10, "UNDO", 0.04f);
        undoLabel.color = Color.white;
        TextMesh moves = Label(root.transform, new Vector2(1.3f, -3.95f), 10, "Moves: 0/6", 0.05f);
        moves.color = Color.white;

        TextMesh banner = Label(root.transform, new Vector2(0f, -0.4f), 41, "CHECKMATE!", 0.12f);
        banner.color = new Color(1f, 0.85f, 0.3f);
        Fit(banner, 5.0f, 1.0f);
        Bar(banner.transform, "Back", Vector2.zero, new Vector2(5.4f, 1.2f), new Color(0.17f, 0.13f, 0.2f, 0.8f), 40);

        V40E_Hud(root.transform, 2, 3, out SpriteRenderer[] dots, out SpriteRenderer[] hearts, out TextMesh goal);

        SetField(level, "knight", knight);
        SetField(level, "rook", rook);
        SetField(level, "king", king);
        SetField(level, "yarn", yarn);
        SetArray(level, "mice", mice.ToArray());
        SetArray(level, "moveDots", moveDots.ToArray());
        SetField(level, "selectRing", ring);
        SetField(level, "undoButton", undo.GetComponent<Collider2D>());
        SetField(level, "movesText", moves);
        SetField(level, "banner", banner);
        SetArray(level, "dots", dots);
        SetArray(level, "hearts", hearts);
        SetField(level, "veil", V40E_Veil(root.transform));
        SetField(level, "goal", goal);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.9f, 2.2f)));
        return SaveLevel(root, "Knight");
    }

    // ---------- The gauntlet ----------

    static GameObject BuildGauntletLevel()
    {
        var root = new GameObject("Level_Gauntlet");
        GauntletLevel level = root.AddComponent<GauntletLevel>();
        Art(root.transform, "Stage", "e40_arena_bg", -20, false);
        var stages = new List<GameObject>();
        for (int i = 0; i < 5; i++) stages.Add(Child(root.transform, "Trial" + i, Vector2.zero));

        // 1: chiave gigante e lucchetto
        Transform s0 = stages[0].transform;
        Transform padlock = Child(s0, "Padlock", new Vector2(0.6f, -1.6f)).transform;
        Art(padlock, "Art", "e40_padlock", 2, false);
        Transform keyhole = Child(padlock, "Keyhole", new Vector2(0f, -0.1f)).transform;
        Transform key = Child(s0, "Key", new Vector2(2.0f, 1.2f)).transform;
        Transform keyScale = Child(key, "Scale", Vector2.zero).transform;
        Art(keyScale, "Art", "key", 6, false);

        // 2: mini-labirinto da inclinare
        Transform s1 = stages[1].transform;
        Transform maze = Child(s1, "Maze", new Vector2(0f, -0.6f)).transform;
        Art(maze, "Tray", "e40_maze_tray", 0, false);
        float[] walls = DoodleSprites.V40E_MazeWalls;
        for (int i = 0; i + 3 < walls.Length; i += 4)
            Child(maze, "Wall", new Vector2(walls[i], walls[i + 1])).AddComponent<BoxCollider2D>().size = new Vector2(walls[i + 2] * 2f, walls[i + 3] * 2f);
        float[] hp = DoodleSprites.V40E_MazeHoles;
        var holes = new List<Transform>();
        for (int i = 0; i + 1 < hp.Length; i += 2)
        {
            Transform hole = Child(maze, "Hole", new Vector2(hp[i], hp[i + 1])).transform;
            Art(hole, "Art", "e40_maze_hole", 1, false);
            if (i + 2 >= hp.Length) Art(hole, "Star", "goal_star", 2, false).localScale = Vector3.one * 0.35f;
            holes.Add(hole);
        }
        GameObject ballGo = Child(maze, "Ball", new Vector2(-1.9f, 1.42f));
        Art(ballGo.transform, "Art", "e40_marble", 5, false);
        ballGo.AddComponent<CircleCollider2D>().radius = 0.17f;
        Rigidbody2D rb = ballGo.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
#if UNITY_6000_0_OR_NEWER
        rb.linearDamping = 0.8f;
#else
        rb.drag = 0.8f;
#endif
        TiltGravity tilt = stages[1].AddComponent<TiltGravity>();
        tilt.strength = 14f;
        tilt.enabled = false;

        // 3: torta con le candeline
        Transform s2 = stages[2].transform;
        Transform cake = Child(s2, "Cake", new Vector2(0f, -1.6f)).transform;
        cake.localScale = Vector3.one * 1.3f;
        Art(cake, "Art", "cake", 2, false);
        var flames = new List<Transform>();
        var smokes = new List<Transform>();
        foreach (float x in new[] { -0.45f, 0f, 0.45f })
        {
            Art(cake, "Candle", "candle", 3, false, new Vector2(x, 0.58f));
            Transform f = Child(cake, "Flame", new Vector2(x, 1.15f)).transform;
            Art(f, "Art", "flame", 5, false);
            flames.Add(f);
            Transform sm = Child(cake, "Smoke", new Vector2(x, 1.25f)).transform;
            sm.localScale = Vector3.one * 0.4f;
            Art(sm, "Art", "puff", 6, false);
            smokes.Add(sm);
        }
        GameObject cakeArea = Child(s2, "CakeArea", new Vector2(0f, -1.1f));
        cakeArea.AddComponent<BoxCollider2D>().size = new Vector2(3.6f, 2.6f);

        // 4: radio che urla
        Transform s3 = stages[3].transform;
        Transform radio = Child(s3, "Radio", new Vector2(0f, -1.2f)).transform;
        radio.localScale = Vector3.one * 1.1f;
        Art(radio, "Art", "radio", 2, false);
        GameObject knob = Child(radio, "Knob", new Vector2(0.85f, 0.32f));
        knob.AddComponent<CircleCollider2D>().radius = 0.42f;
        var bars = new List<SpriteRenderer>();
        for (int i = 0; i < 3; i++)
            bars.Add(Bar(s3, "VolumeBar" + i, new Vector2(-0.6f + i * 0.6f, 0.45f + i * 0.15f), new Vector2(0.4f, 0.4f + i * 0.3f), Color.white, 3));
        TextMesh vol = Label(s3, new Vector2(0f, 1.6f), 3, "VOLUME", 0.05f);
        vol.color = Color.white;
        AudioSource screamer = Child(s3, "Screamer", Vector2.zero).AddComponent<AudioSource>();
        screamer.playOnAwake = false;

        // 5: il bottone rosso
        Transform s4 = stages[4].transform;
        Transform button = Child(s4, "Button", new Vector2(0f, -1.3f)).transform;
        button.localScale = Vector3.one * 1.6f;
        Art(button, "Art", "big_button", 2, false);
        TextMesh press = Fit(Label(button, new Vector2(0f, 0.2f), 4, "PRESS ME", 0.04f), 1.05f, 0.4f);
        press.color = Color.white;
        TextMesh countdown = Label(s4, new Vector2(0f, 1.0f), 4, "5", 0.2f);
        countdown.color = Color.white;

        var bursts = new List<SpriteRenderer>();
        for (int i = 0; i < 6; i++) bursts.Add(Art(root.transform, "Firework", "e40_firework", 45, false).GetComponent<SpriteRenderer>());

        V40E_Hud(root.transform, 5, 3, out SpriteRenderer[] dots, out SpriteRenderer[] hearts, out TextMesh goal);

        SetArray(level, "stages", stages.ToArray());
        SetArray(level, "dots", dots);
        SetArray(level, "hearts", hearts);
        SetField(level, "veil", V40E_Veil(root.transform));
        SetField(level, "goal", goal);
        SetField(level, "key", key);
        SetField(level, "keyArt", keyScale);
        SetField(level, "padlock", padlock);
        SetField(level, "keyhole", keyhole);
        SetField(level, "ball", rb);
        SetArray(level, "holes", holes.ToArray());
        SetField(level, "tilt", tilt);
        SetArray(level, "flames", flames.ToArray());
        SetField(level, "cakeArea", cakeArea.GetComponent<Collider2D>());
        SetArray(level, "smokes", smokes.ToArray());
        SetField(level, "radio", radio);
        SetField(level, "knobArea", knob.GetComponent<Collider2D>());
        SetArray(level, "volumeBars", bars.ToArray());
        SetField(level, "screamer", screamer);
        SetField(level, "bigButton", button);
        SetField(level, "buttonText", press);
        SetField(level, "countdown", countdown);
        SetArray(level, "bursts", bursts.ToArray());
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.9f, 2.1f)));
        return SaveLevel(root, "Gauntlet");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V40E = new Dictionary<string, string[]>
    {
        { "elevator_logic", new[] { "Floors count from 1 (bottom) to 5 (top); one floor stays empty. Cat and dog never ride together.",
            "Start from the 'lowest/highest' and 'right above' clues, then use odd/even to pin the rest. Right animals stay put after a check." } },
        { "circuit_board", new[] { "Tap a placed piece to turn it. 9 volts is way too much for a 2V LED.",
            "Put the RESISTOR on the LED's branch (right of the middle cross, or just under the LED), then tap the switch." } },
        { "snow_globe", new[] { "The flakes spell 3 digits only while settling: shake again to re-read. Then tap a ring and tilt the phone.",
            "Set the rings to the snow code, then pinch the tiny key to about the keyhole's size and drag its tip into the music box." } },
        { "knight_quest", new[] { "Mice first: you may need to jump back to a square you already visited. Undo is free.",
            "Mice: c2, e3, back to c2, d4, b5. Mate: rook to a4, knight to d3 (either order)." } },
        { "the_gauntlet", new[] { "Pinch the key small, tilt the marble, blow on the candles, use the phone's volume buttons.",
            "Last trial: put the phone down and touch NOTHING for 5 seconds. The knob on the radio turns it UP." } },
    };
}
#endif
