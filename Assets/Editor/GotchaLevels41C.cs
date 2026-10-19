#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli della specifica 105-200 (in gioco dal 151), pacchetto C.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels41C() => new List<LevelDefinition>
    {
        Define("OneTooMany", "one_too_many", "ONE TOO MANY", "Are there really six?", BuildOneTooManyLevel()),
        Define("ReverseParking", "reverse_parking", "REVERSE PARKING", "One finger isn't enough.", BuildReverseParkingLevel()),
        Define("StubbornDoor", "the_stubborn_door", "THE STUBBORN DOOR", "The key can only open one lock.", BuildStubbornDoorLevel()),
        Define("ZeroGravityDinner", "zero_gravity_dinner", "ZERO GRAVITY DINNER", "Don't chase every meatball.", BuildZeroGravityDinnerLevel()),
        Define("ForgottenPassword", "the_forgotten_password", "THE FORGOTTEN PASSWORD", "The answer is already visible.", BuildForgottenPasswordLevel()),
        Define("ShrinkingRoom", "the_shrinking_room", "THE SHRINKING ROOM", "Think bigger than the mouse.", BuildShrinkingRoomLevel()),
        Define("PaperworkNightmare", "paperwork_nightmare", "PAPERWORK NIGHTMARE", "The papers are hiding something.", BuildPaperworkNightmareLevel()),
        Define("BatteryVampire", "battery_vampire", "BATTERY VAMPIRE", "He loves electricity.", BuildBatteryVampireLevel()),
    };

    // ---------- Aiuti comuni del pacchetto 41C ----------

    /// <summary>Barra in alto: cuori a sinistra (se ce ne sono) e riga dell'obiettivo sotto.</summary>
    static void S41C_Hud(Transform root, int heartCount, out SpriteRenderer[] hearts, out TextMesh goal)
    {
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

    static void S41C_Wire(Object level, SpriteRenderer[] hearts, TextMesh goal, SpeechBubble bubble)
    {
        if (hearts != null) SetArray(level, "hearts", hearts);
        SetField(level, "goal", goal);
        SetField(level, "bubble", bubble);
    }

    // ---------- One too many ----------

    static GameObject BuildOneTooManyLevel()
    {
        var root = new GameObject("Level_OneTooMany");
        OneTooManyLevel level = root.AddComponent<OneTooManyLevel>();
        Art(root.transform, "Farm", "s41c_farm_bg", -20, false);
        // pollaio con cinque finestrelle
        Transform coop = Child(root.transform, "Coop", new Vector2(1.3f, 0.95f)).transform;
        coop.localScale = Vector3.one * 0.9f;
        Art(coop, "Art", "s41c_coop", 2, false);
        Fit(Label(coop, new Vector2(0f, -0.12f), 3, "5 NESTS", 0.035f), 1.1f, 0.2f);
        Transform door = Child(coop, "Door", new Vector2(0f, -0.6f)).transform;
        var heads = new List<Object>();
        for (int i = 0; i < 5; i++)
        {
            Transform n = Child(coop, "Nest" + i, new Vector2(-0.9f + i * 0.45f, 0.2f)).transform;
            n.localScale = Vector3.one * 0.27f;
            heads.Add(Art(n, "Hen", "mother_hen", 4, false).GetComponent<SpriteRenderer>());
        }
        // pozzanghera col riflesso
        Transform puddle = Child(root.transform, "Puddle", new Vector2(0.5f, -3.4f)).transform;
        Art(puddle, "Water", "s41c_puddle", 3, false);
        SpriteRenderer reflection = Art(puddle, "Reflection", "s41c_puddle_reflection", 4, false).GetComponent<SpriteRenderer>();
        // galline (piedi nell'origine)
        Vector2[] spots = { new Vector2(-2.0f, -1.35f), new Vector2(-0.6f, -0.85f), new Vector2(-2.0f, -3.5f), new Vector2(2.0f, -2.2f), new Vector2(0.5f, -2.92f) };
        Color[] tints = { new Color(0.95f, 0.75f, 0.55f), Color.white, new Color(1f, 0.9f, 0.7f), new Color(0.85f, 0.65f, 0.5f), Color.white };
        var hens = new List<Object>();
        for (int i = 0; i < spots.Length; i++)
        {
            Transform h = Child(root.transform, "Hen" + i, spots[i]).transform;
            h.localScale = Vector3.one * 0.9f;
            Art(h, "Art", "mother_hen", 6 + i, true, new Vector2(0f, 0.6f)).GetComponent<SpriteRenderer>().color = tints[i];
            hens.Add(h);
        }
        var feathers = new List<Object>();
        for (int i = 0; i < 7; i++) feathers.Add(Art(root.transform, "Feather" + i, "s41c_feather", 20, false).GetComponent<SpriteRenderer>());
        TextMesh counter = Fit(Label(root.transform, new Vector2(-1.25f, 2.1f), 5, "HENS: 6   NESTS: 5", 0.04f), 2.6f, 0.35f);
        S41C_Hud(root.transform, 3, out SpriteRenderer[] hearts, out TextMesh goal);

        SetArray(level, "hens", hens.ToArray());
        SetField(level, "puddle", puddle);
        SetField(level, "reflection", reflection);
        SetField(level, "coopDoor", door);
        SetArray(level, "nestHeads", heads.ToArray());
        SetArray(level, "feathers", feathers.ToArray());
        SetField(level, "counter", counter);
        S41C_Wire(level, hearts, goal, SmallBubble(root.transform, new Vector2(0.4f, -1.55f)));
        return SaveLevel(root, "OneTooMany");
    }

    // ---------- Reverse parking ----------

    static GameObject BuildReverseParkingLevel()
    {
        var root = new GameObject("Level_ReverseParking");
        ReverseParkingLevel level = root.AddComponent<ReverseParkingLevel>();
        Art(root.transform, "Lot", "s41c_lot_bg", -20, false);
        var parked = new List<Object>();
        Color[] tints = { new Color(0.55f, 0.75f, 1f), new Color(0.6f, 0.95f, 0.6f) };
        float[] ys = { 1.52f, -1.52f };
        for (int i = 0; i < 2; i++)
        {
            Transform c = Child(root.transform, "Parked" + i, new Vector2(1.75f, ys[i])).transform;
            Art(c, "Art", "s41c_car", 4, false).GetComponent<SpriteRenderer>().color = tints[i];
            parked.Add(c);
        }
        Transform car = Child(root.transform, "Car", new Vector2(0.4f, 1.9f)).transform;
        Art(car, "Art", "s41c_car", 6, false).GetComponent<SpriteRenderer>().color = new Color(1f, 0.55f, 0.4f);
        var dots = new List<Object>();
        for (int i = 0; i < 12; i++)
        {
            Transform d = Art(root.transform, "PathDot" + i, "e40_dot", 5, false);
            d.localScale = Vector3.one * 0.8f;
            dots.Add(d.GetComponent<SpriteRenderer>());
        }
        // cruscotto: volante a sinistra, pedali a destra
        Bar(root.transform, "Dashboard", new Vector2(0f, -4.15f), new Vector2(6.4f, 2.5f), new Color(0.18f, 0.16f, 0.22f), 10);
        Bar(root.transform, "DashEdge", new Vector2(0f, -2.92f), new Vector2(6.4f, 0.06f), new Color(0.08f, 0.07f, 0.1f), 11);
        Transform wheel = Child(root.transform, "Wheel", new Vector2(-1.55f, -3.8f)).transform;
        Art(wheel, "Art", "s41c_wheel", 12, false);
        GameObject wheelArea = Child(root.transform, "WheelArea", new Vector2(-1.55f, -3.8f));
        wheelArea.AddComponent<CircleCollider2D>().radius = 0.95f;
        Collider2D Pedal(string name, Vector2 pos)
        {
            Transform p = Child(root.transform, name, pos).transform;
            Art(p, "Art", "s41c_pedal", 12, false);
            Fit(Label(p, new Vector2(0f, 0f), 13, name, 0.045f), 0.7f, 0.3f);
            BoxCollider2D b = p.gameObject.AddComponent<BoxCollider2D>();
            b.size = new Vector2(1.05f, 1.25f);
            return b;
        }
        Collider2D fwd = Pedal("FWD", new Vector2(0.8f, -3.8f));
        Collider2D rev = Pedal("REV", new Vector2(2.0f, -3.8f));
        S41C_Hud(root.transform, 3, out SpriteRenderer[] hearts, out TextMesh goal);

        SetField(level, "car", car);
        SetArray(level, "parked", parked.ToArray());
        SetField(level, "wheel", wheel);
        SetField(level, "wheelArea", wheelArea.GetComponent<Collider2D>());
        SetField(level, "fwdPedal", fwd);
        SetField(level, "revPedal", rev);
        SetArray(level, "dots", dots.ToArray());
        S41C_Wire(level, hearts, goal, SmallBubble(root.transform, new Vector2(-0.6f, 1.7f)));
        return SaveLevel(root, "ReverseParking");
    }

    // ---------- The stubborn door ----------

    static GameObject BuildStubbornDoorLevel()
    {
        var root = new GameObject("Level_StubbornDoor");
        StubbornDoorLevel level = root.AddComponent<StubbornDoorLevel>();
        Art(root.transform, "Hall", "s41c_hall_bg", -20, false);
        SpriteRenderer light = Bar(root.transform, "DoorLight", new Vector2(0f, -0.83f), new Vector2(2.3f, 5.1f), new Color(1f, 0.95f, 0.7f), -15);
        Transform hinge = Child(root.transform, "Hinge", new Vector2(-1.14f, -0.83f)).transform;
        Art(hinge, "Door", "s41c_big_door", -10, false, new Vector2(1.14f, 0f));
        var locks = new List<Object>();
        var arts = new List<Object>();
        foreach (float y in new[] { 1.1f, -0.1f, -1.3f })
        {
            Transform l = Child(root.transform, "Lock", new Vector2(0.62f, -0.83f + y)).transform;
            arts.Add(Art(l, "Art", "s41c_padlock", 8, false).GetComponent<SpriteRenderer>());
            locks.Add(l);
        }
        Transform key = Child(root.transform, "Key", new Vector2(-0.35f, -4.0f)).transform;
        Art(key, "Art", "key", 10, false);
        var cracks = new List<Object>();
        foreach (float x in new[] { -0.5f, -0.33f, -0.18f })
            cracks.Add(Art(key, "Crack", "s41c_crack", 11, false, new Vector2(x, 0f)).GetComponent<SpriteRenderer>());
        Transform mouse = Child(root.transform, "Mouse", new Vector2(2.0f, -3.45f)).transform;
        mouse.localScale = Vector3.one * 0.75f;
        Art(mouse, "Art", "mouse", 9, true, new Vector2(0f, 0.55f));
        S41C_Hud(root.transform, 3, out SpriteRenderer[] hearts, out TextMesh goal);

        SetField(level, "key", key);
        SetArray(level, "cracks", cracks.ToArray());
        SetArray(level, "locks", locks.ToArray());
        SetArray(level, "lockArts", arts.ToArray());
        SetField(level, "openSprite", sprites["s41c_padlock_open"]);
        SetField(level, "hinge", hinge);
        SetField(level, "doorLight", light);
        SetField(level, "mouse", mouse);
        S41C_Wire(level, hearts, goal, SmallBubble(root.transform, new Vector2(1.55f, -2.0f)));
        return SaveLevel(root, "StubbornDoor");
    }

    // ---------- Zero gravity dinner ----------

    static GameObject BuildZeroGravityDinnerLevel()
    {
        var root = new GameObject("Level_ZeroGravityDinner");
        ZeroGravityDinnerLevel level = root.AddComponent<ZeroGravityDinnerLevel>();
        Art(root.transform, "Station", "s41c_station_bg", -20, false);
        Transform bowl = Child(root.transform, "Bowl", new Vector2(0f, -2.8f)).transform;
        Art(bowl, "Back", "s41c_bowl_back", 4, false);
        Art(bowl, "Front", "s41c_bowl", 7, false);
        TextMesh hold = Label(bowl, new Vector2(0f, 1.0f), 20, "", 0.09f);
        var balls = new List<Object>();
        for (int i = 0; i < 5; i++)
        {
            Transform b = Child(root.transform, "Meatball" + i, Vector2.zero).transform;
            Art(b, "Art", "s41c_meatball", 6, false);
            balls.Add(b);
        }
        S41C_Hud(root.transform, 3, out SpriteRenderer[] hearts, out TextMesh goal);

        SetField(level, "bowl", bowl);
        SetArray(level, "balls", balls.ToArray());
        SetField(level, "holdText", hold);
        S41C_Wire(level, hearts, goal, SmallBubble(root.transform, new Vector2(-1.2f, 1.95f)));
        return SaveLevel(root, "ZeroGravityDinner");
    }

    // ---------- The forgotten password ----------

    static GameObject BuildForgottenPasswordLevel()
    {
        var root = new GameObject("Level_ForgottenPassword");
        ForgottenPasswordLevel level = root.AddComponent<ForgottenPasswordLevel>();
        Art(root.transform, "Office", "s41c_pc_bg", -20, false);
        Transform monitor = Child(root.transform, "Monitor", new Vector2(0f, 0.9f)).transform;
        Art(monitor, "Art", "s41c_monitor", 2, false);
        TextMesh title = Fit(Label(root.transform, new Vector2(0f, 1.65f), 5, "WELCOME BACK!", 0.045f), 2.2f, 0.3f);
        title.color = Color.white;
        Bar(root.transform, "Field", new Vector2(0f, 1.0f), new Vector2(2.0f, 0.44f), new Color(0.96f, 0.97f, 1f), 4);
        TextMesh field = Fit(Label(root.transform, new Vector2(0f, 1.0f), 5, "_ _ _ _", 0.05f), 1.85f, 0.36f);
        TextMesh msg = Fit(Label(root.transform, new Vector2(0f, 0.45f), 5, "", 0.035f), 2.3f, 0.3f);
        SpriteRenderer tint = Bar(root.transform, "ScreenTint", new Vector2(0f, 1.02f), new Vector2(2.44f, 1.7f), Color.clear, 6);
        GameObject box = Child(root.transform, "PasswordBox", new Vector2(0f, 1.0f));
        box.AddComponent<BoxCollider2D>().size = new Vector2(2.4f, 1.1f);
        // post-it attaccato al monitor
        Transform note = Child(root.transform, "Note", new Vector2(1.5f, 2.0f)).transform;
        note.localScale = Vector3.one * 0.62f;
        note.localRotation = Quaternion.Euler(0f, 0f, -8f);
        Art(note, "Art", "sticky_note", 12, false);
        Fit(Label(note, new Vector2(0f, -0.05f), 13, "I FORGOT IT", 0.075f), 1.6f, 0.6f);
        // tastierino finto
        Bar(root.transform, "Keypad", new Vector2(0f, -2.57f), new Vector2(2.7f, 2.85f), new Color(0.32f, 0.3f, 0.36f), 3);
        string[] labels = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "DEL", "0", "OK" };
        int[] codes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 11 };
        var keys = new Object[12];
        for (int i = 0; i < 12; i++)
        {
            Transform k = Child(root.transform, "Key" + labels[i], new Vector2(-0.82f + (i % 3) * 0.82f, -1.6f - (i / 3) * 0.65f)).transform;
            k.localScale = Vector3.one * 1.15f;
            Art(k, "Art", "s41c_keycap", 4, false);
            Fit(Label(k, new Vector2(0f, 0.02f), 5, labels[i], 0.04f), 0.5f, 0.3f);
            BoxCollider2D c = k.gameObject.AddComponent<BoxCollider2D>();
            c.size = new Vector2(0.68f, 0.56f);
            keys[codes[i]] = c;
        }
        S41C_Hud(root.transform, 3, out SpriteRenderer[] hearts, out TextMesh goal);

        SetField(level, "note", note);
        SetField(level, "passwordBox", box.GetComponent<Collider2D>());
        SetField(level, "field", field);
        SetField(level, "screenMsg", msg);
        SetField(level, "screenTint", tint);
        SetArray(level, "keys", keys);
        S41C_Wire(level, hearts, goal, SmallBubble(root.transform, new Vector2(-1.65f, -0.75f)));
        return SaveLevel(root, "ForgottenPassword");
    }

    // ---------- The shrinking room ----------

    static GameObject BuildShrinkingRoomLevel()
    {
        var root = new GameObject("Level_ShrinkingRoom");
        ShrinkingRoomLevel level = root.AddComponent<ShrinkingRoomLevel>();
        Bar(root.transform, "Void", Vector2.zero, new Vector2(12f, 16f), new Color(0.12f, 0.1f, 0.16f), -30);
        Transform room = Child(root.transform, "Room", new Vector2(0f, -0.8f)).transform;
        Art(room, "Floor", "s41c_room_floor", -20, false);
        Transform Wall(string name, string sprite)
        {
            Transform w = Child(room, name, Vector2.zero).transform;
            Art(w, "Art", sprite, -10, false);
            return w;
        }
        Transform wl = Wall("WallLeft", "s41c_wall_v"), wr = Wall("WallRight", "s41c_wall_v");
        Transform wt = Wall("WallTop", "s41c_wall_h"), wb = Wall("WallBottom", "s41c_wall_h");
        Transform exit = Child(wr, "Exit", new Vector2(-0.15f, 0f)).transform;
        Art(exit, "Art", "s41c_exit_door", -9, false);
        Transform mouse = Child(room, "Mouse", Vector2.zero).transform;
        mouse.localScale = Vector3.one * 0.6f;
        Art(mouse, "Art", "mouse", 5, true, new Vector2(0f, -0.15f));
        S41C_Hud(root.transform, 0, out SpriteRenderer[] _, out TextMesh goal);

        SetField(level, "room", room);
        SetField(level, "wallLeft", wl);
        SetField(level, "wallRight", wr);
        SetField(level, "wallTop", wt);
        SetField(level, "wallBottom", wb);
        SetField(level, "exitDoor", exit);
        SetField(level, "mouse", mouse);
        S41C_Wire(level, null, goal, SmallBubble(root.transform, new Vector2(0.9f, 0.4f)));
        return SaveLevel(root, "ShrinkingRoom");
    }

    // ---------- Paperwork nightmare ----------

    static GameObject BuildPaperworkNightmareLevel()
    {
        var root = new GameObject("Level_PaperworkNightmare");
        PaperworkNightmareLevel level = root.AddComponent<PaperworkNightmareLevel>();
        Art(root.transform, "Office", "s41c_office_bg", -20, false);
        Transform worker = Child(root.transform, "Worker", new Vector2(-1.55f, 0.2f)).transform;
        Art(worker, "Art", "s41c_worker", 1, true);
        Art(root.transform, "Desk", "s41c_desk", 2, false, new Vector2(0f, -1.0f));
        Vector2 doc = new Vector2(0.35f, -1.65f);
        GameObject area = Child(root.transform, "Contract", doc);
        area.AddComponent<BoxCollider2D>().size = new Vector2(1.2f, 1.5f);
        Transform glow = Art(root.transform, "ContractGlow", "glow", 3, false, doc);
        glow.localScale = new Vector3(1.4f, 1.7f, 1f);
        SpriteRenderer glowSr = glow.GetComponent<SpriteRenderer>();
        glowSr.color = new Color(1f, 0.95f, 0.4f, 0.7f);
        var rnd = new System.Random(128);
        Color[] tints = { Color.white, new Color(1f, 0.98f, 0.85f), new Color(0.92f, 0.96f, 1f), new Color(1f, 0.92f, 0.94f) };
        var sheets = new List<Object>();
        var arts = new List<Object>();
        for (int i = 0; i < 14; i++)
        {
            Vector2 at = doc + new Vector2((float)rnd.NextDouble() * 1.4f - 0.7f, (float)rnd.NextDouble() * 1.1f - 0.55f);
            Transform s = Child(root.transform, "Sheet" + i, at).transform;
            s.localRotation = Quaternion.Euler(0f, 0f, (float)rnd.NextDouble() * 60f - 30f);
            SpriteRenderer sr = Art(s, "Art", "s41c_sheet", 10 + i, false).GetComponent<SpriteRenderer>();
            sr.color = tints[i % tints.Length];
            sheets.Add(s);
            arts.Add(sr);
        }
        Transform bin = Child(root.transform, "Bin", new Vector2(2.2f, -3.55f)).transform;
        Art(bin, "Art", "s41c_fancy_bin", 30, false);
        Transform mouth = Child(bin, "Mouth", new Vector2(0f, 0.5f)).transform;
        Bar(root.transform, "TimerBack", new Vector2(2.1f, 3.35f), new Vector2(1.1f, 0.45f), new Color(0.1f, 0.08f, 0.14f, 0.72f), 31);
        TextMesh timer = Fit(Label(root.transform, new Vector2(2.1f, 3.35f), 32, "0:40", 0.045f), 1.0f, 0.38f);
        timer.color = Color.white;
        S41C_Hud(root.transform, 0, out SpriteRenderer[] _, out TextMesh goal);

        SetArray(level, "sheets", sheets.ToArray());
        SetArray(level, "sheetArts", arts.ToArray());
        SetField(level, "binMouth", mouth);
        SetField(level, "bin", bin);
        SetField(level, "docArea", area.GetComponent<Collider2D>());
        SetField(level, "docGlow", glowSr);
        SetField(level, "timerText", timer);
        S41C_Wire(level, null, goal, SmallBubble(root.transform, new Vector2(-0.45f, 1.75f)));
        return SaveLevel(root, "PaperworkNightmare");
    }

    // ---------- Battery vampire ----------

    static GameObject BuildBatteryVampireLevel()
    {
        var root = new GameObject("Level_BatteryVampire");
        BatteryVampireLevel level = root.AddComponent<BatteryVampireLevel>();
        Art(root.transform, "Lab", "s41c_vamp_bg", -20, false);
        Transform battery = Child(root.transform, "Battery", new Vector2(0f, -0.6f)).transform;
        Bar(battery, "Back", Vector2.zero, new Vector2(3.32f, 1.22f), new Color(0.12f, 0.1f, 0.16f), 2);
        SpriteRenderer fill = Bar(battery, "Fill", Vector2.zero, new Vector2(3.24f, 1.14f), new Color(0.4f, 0.9f, 0.4f), 3);
        Art(battery, "Shell", "s41c_battery_shell", 5, false);
        Transform terminal = Child(battery, "Terminal", new Vector2(2.1f, 0f)).transform;
        TextMesh percent = Label(root.transform, new Vector2(0f, -2.05f), 6, "100%", 0.09f);
        percent.color = Color.white;
        Transform plus = Child(root.transform, "Plus", new Vector2(1.05f, -0.6f)).transform;
        Art(plus, "Art", "s41c_sym_plus", 8, false);
        Transform minus = Child(root.transform, "Minus", new Vector2(-1.05f, -0.6f)).transform;
        Art(minus, "Art", "s41c_sym_minus", 8, false);
        LineRenderer cable = Line(root.transform, "Cable", 0.06f, new Color(0.12f, 0.1f, 0.16f), 7);
        Transform vamp = Child(root.transform, "Vampire", new Vector2(1.75f, 0.95f)).transform;
        vamp.localScale = Vector3.one * 1.0f;
        SpriteRenderer vampArt = Art(vamp, "Art", "s41c_vampire", 9, true).GetComponent<SpriteRenderer>();
        var bolts = new List<Object>();
        for (int i = 0; i < 4; i++)
        {
            Transform b = Art(root.transform, "Bolt" + i, "bolt", 12, false);
            bolts.Add(b.GetComponent<SpriteRenderer>());
        }
        S41C_Hud(root.transform, 0, out SpriteRenderer[] _, out TextMesh goal);

        SetField(level, "fill", fill.transform);
        SetField(level, "fillArt", fill);
        SetField(level, "percent", percent);
        SetField(level, "plus", plus);
        SetField(level, "minus", minus);
        SetField(level, "vampire", vamp);
        SetField(level, "vampireArt", vampArt);
        SetField(level, "zapSprite", sprites["s41c_vampire_zap"]);
        SetField(level, "cable", cable);
        SetField(level, "terminal", terminal);
        SetArray(level, "bolts", bolts.ToArray());
        S41C_Wire(level, null, goal, SmallBubble(root.transform, new Vector2(-0.9f, 1.4f)));
        return SaveLevel(root, "BatteryVampire");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V41C = new Dictionary<string, string[]>
    {
        { "one_too_many", new[] { "Look at the reflection.", "Move the puddle away." } },
        { "reverse_parking", new[] { "Steer and move at the same time.", "Use two fingers and reverse into the space." } },
        { "the_stubborn_door", new[] { "Try moving the locks.", "Bring the remaining locks to the inserted key." } },
        { "zero_gravity_dinner", new[] { "Position the bowl along their paths.", "Catch all five, then hold still." } },
        { "the_forgotten_password", new[] { "Don't type.", "Drag the sticky note into the password field." } },
        { "the_shrinking_room", new[] { "The room itself can change.", "Pinch the entire room smaller." } },
        { "paperwork_nightmare", new[] { "Clear the desk.", "Throw the papers away and tap the desk." } },
        { "battery_vampire", new[] { "Maybe the polarity matters.", "Swap the plus and minus symbols." } },
    };
}
#endif
