#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli compositi della versione 0.40 (gruppo A).</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels40A() => new List<LevelDefinition>
    {
        Define("Morning", "morning_routine", "MORNING ROUTINE", "The cat must get ready. Step one: wake him up!", BuildMorningLevel()),
        Define("Heist", "bank_heist", "BANK HEIST", "Some ink can only be seen in the right light.", BuildHeistLevel()),
        Define("Submarine", "submarine", "SUBMARINE", "Dive to the wreck. Keep an eye on your air!", BuildSubmarineLevel()),
        Define("Haunted", "haunted_house", "HAUNTED HOUSE", "Find the candles in the dark. Your finger is the flashlight.", BuildHauntedLevel()),
    };

    // ---------- Aiuti comuni del gruppo A ----------

    /// <summary>Barra delle fasi: riga dell'obiettivo, pallini delle fasi, cuori dei tentativi, scritta e dissolvenza.</summary>
    static StageHud40A V40A_Hud(Transform root, int stages, int tries)
    {
        GameObject go = Child(root, "StageHud", Vector2.zero);
        StageHud40A hud = go.AddComponent<StageHud40A>();
        Transform goalHolder = Child(go.transform, "Goal", new Vector2(0f, 2.98f)).transform;
        Bar(goalHolder, "Back", Vector2.zero, new Vector2(5.5f, 0.5f), new Color(0.12f, 0.09f, 0.16f, 0.72f), 70);
        TextMesh goal = Label(goalHolder, Vector2.zero, 71, "", 0.045f);
        goal.color = Color.white;
        Fit(goal, 5.3f, 0.42f);
        var dots = new List<SpriteRenderer>();
        for (int i = 0; i < stages; i++)
            dots.Add(Art(go.transform, "Dot" + i, "v40a_dot", 71, false, new Vector2((i - (stages - 1) * 0.5f) * 0.42f, 3.42f)).GetComponent<SpriteRenderer>());
        var hearts = new List<SpriteRenderer>();
        for (int i = 0; i < tries; i++)
        {
            Transform h = Art(go.transform, "Try" + i, "heart", 71, false, new Vector2(2.45f - (tries - 1 - i) * 0.4f, 3.42f));
            h.localScale = Vector3.one * 0.65f;
            hearts.Add(h.GetComponent<SpriteRenderer>());
        }
        Transform bannerHolder = Child(go.transform, "Banner", new Vector2(0f, 0.8f)).transform;
        Bar(bannerHolder, "Back", Vector2.zero, new Vector2(5.4f, 1.0f), new Color(0.12f, 0.09f, 0.16f, 0.8f), 91);
        TextMesh banner = Label(bannerHolder, Vector2.zero, 92, "", 0.1f);
        banner.color = new Color(1f, 0.85f, 0.3f);
        Fit(banner, 5.0f, 0.85f);
        SpriteRenderer fade = Bar(go.transform, "Fade", Vector2.zero, new Vector2(12f, 16f), Color.black, 95);

        SetField(hud, "goal", goal);
        SetArray(hud, "dots", dots.ToArray());
        SetArray(hud, "tries", hearts.ToArray());
        SetField(hud, "banner", banner);
        SetField(hud, "fade", fade);
        return hud;
    }

    static void V40A_Floats(Object target, string field, float[] values)
    {
        var so = new UnityEditor.SerializedObject(target);
        UnityEditor.SerializedProperty p = so.FindProperty(field);
        if (p == null) { Debug.LogError("[Gotcha] Campo non trovato: " + field); return; }
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).floatValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void V40A_Masked(Transform t)
    {
        foreach (SpriteRenderer sr in t.GetComponentsInChildren<SpriteRenderer>(true)) sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    }

    static SpriteMask V40A_Mask(Transform parent, string name, string sprite, Vector2 pos, float scale)
    {
        GameObject m = Child(parent, name, pos + centers[sprite] * scale);
        m.transform.localScale = Vector3.one * scale;
        SpriteMask mask = m.AddComponent<SpriteMask>();
        mask.sprite = sprites[sprite];
        return mask;
    }

    // ---------- Morning routine ----------

    static GameObject BuildMorningLevel()
    {
        var root = new GameObject("Level_Morning");
        MorningLevel level = root.AddComponent<MorningLevel>();

        // camera da letto
        Transform bedroom = Child(root.transform, "Bedroom", Vector2.zero).transform;
        Art(bedroom, "Room", "morning_bedroom_bg", -20, false);
        SpriteRenderer sun = Art(bedroom, "Sun", "glow", -4, false, new Vector2(0f, 1.6f)).GetComponent<SpriteRenderer>();
        sun.transform.localScale = Vector3.one * 5f;
        sun.color = new Color(1f, 0.95f, 0.7f, 0f);
        var curtains = new List<Transform>();
        foreach (float s in new[] { -1f, 1f })
        {
            Transform c = Child(bedroom, s < 0 ? "CurtainLeft" : "CurtainRight", new Vector2(s * 0.68f, 2.85f)).transform;
            Art(c, "Art", "morning_curtain", 5, false);
            BoxCollider2D col = c.gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.5f, 2.7f);
            col.offset = new Vector2(0f, -1.35f);
            curtains.Add(c);
        }
        Transform cat = Child(bedroom, "Cat", new Vector2(-1.3f, -0.62f)).transform;
        cat.localScale = Vector3.one * 0.75f;
        Art(cat, "Art", "cat", 3);
        GameObject eyes = Art(cat, "EyesOpen", "cat_eyes", 4, false).gameObject;
        cat.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(3.2f, 2.1f);
        Transform zzz = Art(bedroom, "Zzz", "zzz", 8, false, new Vector2(-1.5f, 0.15f));
        Transform clock = Child(bedroom, "Clock", new Vector2(2.2f, -1.66f)).transform;
        clock.localScale = Vector3.one * 0.55f;
        Art(clock, "Art", "alarm_clock", 4);
        GameObject waves = Art(clock, "Waves", "ring_waves", 4, true, new Vector2(0f, 1.1f)).gameObject;
        waves.AddComponent<Pulse>();
        BoxCollider2D clockCol = clock.gameObject.AddComponent<BoxCollider2D>();
        clockCol.size = new Vector2(2.2f, 2.2f);
        clockCol.offset = new Vector2(0f, 0.9f);
        SpriteRenderer dark = Bar(bedroom, "Dark", Vector2.zero, new Vector2(12f, 16f), new Color(0.02f, 0.02f, 0.08f, 0.72f), 20);

        // cucina
        Transform kitchen = Child(root.transform, "Kitchen", Vector2.zero).transform;
        Art(kitchen, "Room", "morning_kitchen_bg", -20, false);
        Transform kcat = Child(kitchen, "Cat", new Vector2(-1.95f, -0.72f)).transform;
        kcat.localScale = Vector3.one * 0.75f;
        SpriteRenderer kcatArt = Art(kcat, "Art", "morning_cat_hungry", 1, false).GetComponent<SpriteRenderer>();
        Transform bowl = Child(kitchen, "Bowl", new Vector2(0.25f, -0.62f)).transform;
        Art(bowl, "Back", "morning_bowl_back", 3, false);
        V40A_Mask(bowl, "Mask", "morning_bowl_mask", Vector2.zero, 1f);
        Transform cereal = Child(bowl, "Cereal", new Vector2(0f, -0.6f)).transform;
        Art(cereal, "Art", "morning_cereal_fill", 4, false);
        V40A_Masked(cereal);
        SpriteRenderer milk = Bar(bowl, "Milk", new Vector2(0f, -1.4f), new Vector2(2.6f, 1.6f), new Color(1f, 0.99f, 0.95f, 0.82f), 5);
        milk.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        SpriteRenderer zone = Bar(bowl, "Zone", new Vector2(0f, 0.35f), new Vector2(2.6f, 0.23f), new Color(0.35f, 0.9f, 0.4f, 0.4f), 6);
        zone.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        Art(bowl, "Front", "morning_bowl_front", 7, false);
        GameObject bowlArea = Child(kitchen, "BowlArea", new Vector2(0.25f, 0.2f));
        bowlArea.AddComponent<BoxCollider2D>().size = new Vector2(2.9f, 2.8f);
        var flakes = new List<Transform>();
        for (int i = 0; i < 18; i++) flakes.Add(Art(kitchen, "Flake", "morning_flake", 6, false));
        Transform box = Child(kitchen, "CerealBox", new Vector2(1.6f, 0.9f)).transform;
        Art(box, "Art", "morning_cereal_box", 8, false);
        Transform mouth = Child(box, "Mouth", new Vector2(-0.3f, 0.75f)).transform;
        Transform carton = Child(kitchen, "Milk", new Vector2(2.1f, -0.52f)).transform;
        Art(carton, "Art", "morning_milk", 8, false);
        Transform spout = Child(carton, "Spout", new Vector2(-0.38f, 0.84f)).transform;
        BoxCollider2D cartonCol = carton.gameObject.AddComponent<BoxCollider2D>();
        cartonCol.size = new Vector2(1.1f, 1.9f);
        cartonCol.offset = new Vector2(0f, 0.05f);
        SpriteRenderer stream = Bar(kitchen, "Stream", Vector2.zero, new Vector2(0.09f, 1f), new Color(1f, 0.99f, 0.95f), 6);

        StageHud40A hud = V40A_Hud(root.transform, 4, 3);
        SetField(level, "hud", hud);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.9f, 1.0f)));
        SetField(level, "bedroom", bedroom.gameObject);
        SetField(level, "kitchen", kitchen.gameObject);
        SetField(level, "clock", clock);
        SetField(level, "clockWaves", waves);
        SetField(level, "clockArea", clockCol);
        SetField(level, "sleepyCat", cat);
        SetField(level, "catEyes", eyes);
        SetField(level, "catArea", cat.GetComponent<Collider2D>());
        SetField(level, "zzz", zzz);
        SetField(level, "curtainLeft", curtains[0]);
        SetField(level, "curtainRight", curtains[1]);
        SetField(level, "darkness", dark);
        SetField(level, "sunGlow", sun);
        SetField(level, "cerealBox", box);
        SetField(level, "boxMouth", mouth);
        SetArray(level, "flakes", flakes.ToArray());
        SetField(level, "bowl", bowl);
        SetField(level, "cerealFill", cereal);
        SetField(level, "milkCarton", carton);
        SetField(level, "milkSpout", spout);
        SetField(level, "milkArea", cartonCol);
        SetField(level, "bowlArea", bowlArea.GetComponent<Collider2D>());
        SetField(level, "milkFill", milk.transform);
        SetField(level, "milkZone", zone);
        SetField(level, "stream", stream);
        SetField(level, "kitchenCat", kcatArt);
        SetField(level, "catHappy", sprites["morning_cat_happy"]);
        return SaveLevel(root, "Morning");
    }

    // ---------- Bank heist ----------

    static GameObject BuildHeistLevel()
    {
        var root = new GameObject("Level_Heist");
        HeistLevel level = root.AddComponent<HeistLevel>();
        Art(root.transform, "Vault", "heist_vault_bg", -20, false);

        // parete col quadro
        Transform wall = Child(root.transform, "WallScene", Vector2.zero).transform;
        Transform painting = Child(wall, "Painting", new Vector2(0f, 1.0f)).transform;
        Art(painting, "Art", "heist_painting", 2, false);
        var spots = new List<Transform>();
        foreach (Vector2 p in new[] { new Vector2(-0.6f, 0.72f), new Vector2(0.6f, 0.72f), new Vector2(-0.6f, -0.7f), new Vector2(0.6f, -0.7f), new Vector2(0f, 0.8f) })
            spots.Add(Child(painting, "InkSpot", p).transform);
        var ink = new List<SpriteRenderer>();
        for (int i = 0; i < 4; i++)
        {
            SpriteRenderer d = Art(painting, "Ink" + i, "heist_ink_0", 6, false).GetComponent<SpriteRenderer>();
            d.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            ink.Add(d);
        }
        Transform plaque = Child(wall, "Plaque", new Vector2(0f, -0.55f)).transform;
        Bar(plaque, "Back", Vector2.zero, new Vector2(1.3f, 0.34f), new Color(0.89f, 0.69f, 0.24f), 2);
        Fit(Label(plaque, Vector2.zero, 3, "EST. 1887", 0.045f), 1.2f, 0.28f);
        Transform torch = Child(wall, "Torch", new Vector2(0.6f, -2.5f)).transform;
        SpriteRenderer uv = Art(torch, "Light", "glow", 5, false).GetComponent<SpriteRenderer>();
        uv.transform.localScale = Vector3.one * 1.25f;
        uv.color = new Color(0.62f, 0.35f, 1f, 0.6f);
        V40A_Mask(torch, "Mask", "v40a_disc", Vector2.zero, 1.05f);
        Art(torch, "Body", "heist_uv_torch", 7, false, new Vector2(0f, -0.55f));
        Transform note = Child(root.transform, "Note", new Vector2(-1.9f, -3.6f)).transform;
        note.localScale = Vector3.one * 0.65f;
        Art(note, "Art", "sticky_note", 10, false);
        Fit(Label(note, new Vector2(0f, 0.32f), 11, "CODE", 0.05f), 1.5f, 0.3f);
        TextMesh noteText = Fit(Label(note, new Vector2(0f, -0.12f), 11, "_ _ _ _", 0.1f), 1.6f, 0.6f);

        // portellone, tastierino e allarme
        Transform door = Child(root.transform, "DoorScene", Vector2.zero).transform;
        Transform inside = Child(door, "Inside", new Vector2(0f, 1.45f)).transform;
        inside.localScale = Vector3.one * 0.8f;
        Art(inside, "Art", "heist_vault_inside", 1, false);
        Transform vaultDoor = Child(door, "VaultDoor", new Vector2(0f, 1.45f)).transform;
        vaultDoor.localScale = Vector3.one * 0.8f;
        Art(vaultDoor, "Art", "heist_vault_door", 2, false);
        Transform wheel = Child(vaultDoor, "Wheel", Vector2.zero).transform;
        Art(wheel, "Art", "heist_vault_wheel", 3, false);

        Transform keypad = Child(door, "Keypad", new Vector2(0.5f, -2.05f)).transform;
        Art(keypad, "Art", "heist_keypad", 5, false);
        TextMesh display = Fit(Label(keypad, new Vector2(0f, 1.58f), 7, "----", 0.12f), 2.3f, 0.5f);
        display.color = new Color(0.55f, 1f, 0.7f);
        var keys = new Collider2D[12];
        string[] labels = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "C", "0", "OK" };
        int[] ids = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 11 };
        for (int i = 0; i < 12; i++)
        {
            Vector2 at = new Vector2((i % 3 - 1) * 0.9f, 0.75f - (i / 3) * 0.82f);
            Transform k = Child(keypad, "Key" + labels[i], at).transform;
            Transform art = Art(k, "Art", "keypad_key", 6, false);
            art.localScale = Vector3.one * 1.85f;
            art.localPosition = Vector3.zero;
            if (ids[i] == 10) art.GetComponent<SpriteRenderer>().color = new Color(1f, 0.75f, 0.75f);
            if (ids[i] == 11) art.GetComponent<SpriteRenderer>().color = new Color(0.7f, 1f, 0.7f);
            Fit(Label(k, new Vector2(0f, 0.02f), 7, labels[i], 0.1f), 0.6f, 0.5f);
            BoxCollider2D col = k.gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.85f, 0.8f);
            keys[ids[i]] = col;
        }

        Transform alarm = Child(door, "Alarm", new Vector2(0f, -2.2f)).transform;
        Art(alarm, "Art", "heist_alarm_box", 5, false);
        SpriteRenderer alarmLight = Art(alarm, "Light", "glow", 7, false, new Vector2(0f, 1.33f)).GetComponent<SpriteRenderer>();
        alarmLight.color = new Color(1f, 0.25f, 0.25f, 0.6f);
        var wires = new List<Transform>();
        foreach (float x in new[] { -1.2f, -0.4f, 0.4f, 1.2f })
        {
            Transform w = Child(alarm, "Wire", new Vector2(x, 0f)).transform;
            Art(w, "Top", "heist_wire_top", 6, false);
            Art(w, "Bot", "heist_wire_bot", 6, false);
            BoxCollider2D col = w.gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.7f, 1.85f);
            col.offset = new Vector2(0f, -0.15f);
            wires.Add(w);
        }
        Transform riddle = Child(door, "Riddle", new Vector2(0f, -0.05f)).transform;
        riddle.localScale = Vector3.one * 0.75f;
        Art(riddle, "Art", "heist_note", 9, false);
        TextMesh riddleText = Fit(Label(riddle, new Vector2(0.1f, -0.02f), 10, "", 0.06f), 3.5f, 1.3f);

        SpriteRenderer red = Bar(root.transform, "RedFlash", Vector2.zero, new Vector2(12f, 16f), new Color(1f, 0.1f, 0.1f, 0.3f), 85);
        var coins = new List<Transform>();
        for (int i = 0; i < 18; i++)
        {
            Transform c = Art(root.transform, "Coin", "coin", 80, false);
            c.localScale = Vector3.one * 1.3f;
            coins.Add(c);
        }

        var inkSprites = new Object[10];
        for (int d = 0; d < 10; d++) inkSprites[d] = sprites["heist_ink_" + d];
        StageHud40A hud = V40A_Hud(root.transform, 3, 3);
        SetField(level, "hud", hud);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.35f, -1.3f)));
        SetField(level, "wallScene", wall);
        SetField(level, "doorScene", door);
        SetArray(level, "inkDigits", ink.ToArray());
        SetArray(level, "inkSprites", inkSprites);
        SetArray(level, "inkSpots", spots.ToArray());
        SetField(level, "torch", torch);
        SetField(level, "noteText", noteText);
        SetField(level, "keypad", keypad);
        SetArray(level, "keys", keys);
        SetField(level, "display", display);
        SetField(level, "wheel", wheel);
        SetField(level, "alarmBox", alarm);
        SetArray(level, "wires", wires.ToArray());
        SetField(level, "alarmLight", alarmLight);
        SetField(level, "riddleText", riddleText);
        SetField(level, "riddleNote", riddle);
        SetField(level, "redFlash", red);
        SetField(level, "vaultDoor", vaultDoor);
        SetArray(level, "coins", coins.ToArray());
        return SaveLevel(root, "Heist");
    }

    // ---------- Submarine ----------

    static GameObject BuildSubmarineLevel()
    {
        var root = new GameObject("Level_Submarine");
        SubmarineLevel level = root.AddComponent<SubmarineLevel>();
        Art(root.transform, "Sea", "sub_sea_bg", -20, false);

        // la grotta
        Transform cave = Child(root.transform, "Cave", Vector2.zero).transform;
        Transform track = Child(cave, "Track", Vector2.zero).transform;
        Art(track, "Rock", "sub_tunnel", -10, false);
        Fit(Label(cave, new Vector2(-2.45f, 2.5f), 71, "O2", 0.045f), 0.4f, 0.3f).color = Color.white;
        Transform o2 = GaugeBar(cave, "Oxygen", new Vector2(0.2f, 2.5f), new Vector2(4.3f, 0.2f), new Color(0.1f, 0.12f, 0.2f, 0.75f), new Color(0.45f, 0.85f, 1f), 70);

        Transform sub = Child(root.transform, "Sub", new Vector2(0f, 1.6f)).transform;
        SpriteRenderer subArt = Art(sub, "Art", "sub_sub", 20, false).GetComponent<SpriteRenderer>();
        SmokeEmitter air = Emitter(sub, "Air", new Vector2(-0.55f, 0.15f), "sub_bubble", 10f, 1.4f, 1.0f, 0.15f, 1f, Color.white, 19);

        // il fondale col relitto
        Transform floor = Child(root.transform, "Floor", Vector2.zero).transform;
        Art(floor, "Bg", "sub_seafloor_bg", -19, false);
        Art(floor, "Wreck", "sub_wreck", 2, false, new Vector2(-0.4f, -0.6f));
        Transform flag = Child(floor, "Flag", new Vector2(0.3f, 2.0f)).transform;
        flag.localScale = Vector3.one * 0.85f;
        Art(flag, "Art", "sub_flag", 33, false);
        var flagSyms = new List<SpriteRenderer>();
        foreach (float x in new[] { 0.32f, 0.86f, 1.38f })
        {
            Transform s = Art(flag, "Symbol", "sub_sym_fish", 34, false, new Vector2(x, -0.45f));
            s.localScale = Vector3.one * 0.85f;
            flagSyms.Add(s.GetComponent<SpriteRenderer>());
        }
        Transform parking = Child(floor, "Parking", new Vector2(-1.7f, 2.0f)).transform;
        Transform ping = Child(floor, "Ping", new Vector2(2.05f, 0.35f)).transform;
        Art(ping, "Art", "sub_ping_btn", 5, false);
        ping.gameObject.AddComponent<CircleCollider2D>().radius = 0.6f;
        TextMesh pings = Fit(Label(floor, new Vector2(2.05f, 1.05f), 6, "PINGS: 3", 0.045f), 1.3f, 0.3f);
        pings.color = Color.white;
        Transform ring = Child(floor, "Ring", Vector2.zero).transform;
        Art(ring, "Art", "sub_ring", 8, false);
        SpriteRenderer blip = Art(floor, "Blip", "glow", 9, false).GetComponent<SpriteRenderer>();
        var rocks = new List<Transform>();
        float[] rx = { -2.2f, -1.1f, 0f, 1.1f, 2.2f }, ry = { -3.3f, -3.7f, -3.3f, -3.7f, -3.3f };
        for (int i = 0; i < 5; i++)
        {
            Transform r = Child(floor, "Rock" + i, new Vector2(rx[i], ry[i])).transform;
            Art(r, "Art", "sub_rock_" + (i % 2), 5 + i % 2, false);
            BoxCollider2D col = r.gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.05f, 1.0f);
            col.offset = new Vector2(0f, 0.42f);
            rocks.Add(r);
        }
        Transform eel = Child(floor, "Eel", Vector2.zero).transform;
        Art(eel, "Art", "sub_eel", 4, false);
        Transform smallChest = Child(floor, "SmallChest", Vector2.zero).transform;
        smallChest.localScale = Vector3.one * 0.45f;
        Art(smallChest, "Art", "chest", 4, false);

        // la serratura del forziere
        Transform lockScene = Child(root.transform, "Lock", Vector2.zero).transform;
        SpriteRenderer dim = Bar(lockScene, "Dim", Vector2.zero, new Vector2(12f, 16f), new Color(0f, 0.02f, 0.08f, 0f), 30);
        Transform chest = Child(lockScene, "Chest", new Vector2(0f, -1.75f)).transform;
        chest.localScale = Vector3.one * 1.25f;
        SpriteRenderer chestArt = Art(chest, "Art", "chest", 40, false).GetComponent<SpriteRenderer>();
        Art(lockScene, "Plate", "sub_lock_plate", 42, false, new Vector2(0f, -2.9f));
        var dials = new List<Collider2D>();
        var dialSyms = new List<SpriteRenderer>();
        foreach (float x in new[] { -1.2f, 0f, 1.2f })
        {
            Transform d = Child(lockScene, "Dial", new Vector2(x, -2.9f)).transform;
            Art(d, "Art", "sub_dial", 43, false);
            Transform s = Art(d, "Symbol", "sub_sym_fish", 44, false);
            s.localScale = Vector3.one * 0.95f;
            dialSyms.Add(s.GetComponent<SpriteRenderer>());
            CircleCollider2D col = d.gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.58f;
            dials.Add(col);
        }
        SmokeEmitter gold = Emitter(lockScene, "Treasure", new Vector2(0f, -1.0f), "sub_bubble", 14f, 2.0f, 1.3f, 0.7f, 1.6f, Color.white, 45);

        StageHud40A hud = V40A_Hud(root.transform, 3, 3);
        SetField(level, "hud", hud);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.7f, 0.4f)));
        SetField(level, "caveScene", cave.gameObject);
        SetField(level, "track", track);
        V40A_Floats(level, "path", Gotcha.EditorArt.DoodleSprites.V40A_TunnelPath);
        SetField(level, "sub", sub);
        SetField(level, "subArt", subArt);
        SetField(level, "oxygenBar", o2);
        SetField(level, "oxygenFill", o2.GetComponentInChildren<SpriteRenderer>());
        SetField(level, "airBubbles", air);
        SetField(level, "floorScene", floor.gameObject);
        SetField(level, "subParking", parking);
        SetArray(level, "rocks", rocks.ToArray());
        SetField(level, "eel", eel);
        SetField(level, "pingButton", ping.GetComponent<Collider2D>());
        SetField(level, "ring", ring);
        SetField(level, "blip", blip);
        SetField(level, "pingsLabel", pings);
        SetField(level, "smallChest", smallChest);
        SetArray(level, "flagSymbols", flagSyms.ToArray());
        SetArray(level, "symbolSprites", new Object[] { sprites["sub_sym_fish"], sprites["sub_sym_anchor"], sprites["sub_sym_star"], sprites["sub_sym_shell"] });
        SetField(level, "lockScene", lockScene.gameObject);
        SetField(level, "dim", dim);
        SetField(level, "bigChest", chestArt);
        SetField(level, "chestOpen", sprites["chest_open"]);
        SetArray(level, "dials", dials.ToArray());
        SetArray(level, "dialSymbols", dialSyms.ToArray());
        SetField(level, "treasureBubbles", gold);
        return SaveLevel(root, "Submarine");
    }

    // ---------- Haunted house ----------

    static GameObject BuildHauntedLevel()
    {
        var root = new GameObject("Level_Haunted");
        HauntedLevel level = root.AddComponent<HauntedLevel>();
        Art(root.transform, "Hall", "haunt_hall_bg", -20, false);

        var doors = new List<Collider2D>();
        var doorLabels = new List<TextMesh>();
        var doorArts = new List<SpriteRenderer>();
        foreach (float x in new[] { -1.75f, 0f, 1.75f })
        {
            Transform d = Child(root.transform, "Door", new Vector2(x, -0.62f)).transform;
            doorArts.Add(Art(d, "Art", "haunt_door", 2, false).GetComponent<SpriteRenderer>());
            doorLabels.Add(Fit(Label(d, new Vector2(0f, 1.65f), 3, "", 0.07f), 0.85f, 0.3f));
            BoxCollider2D col = d.gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.35f, 2.2f);
            col.offset = new Vector2(0f, 1.05f);
            doors.Add(col);
        }

        var spots = new List<Transform>();
        foreach (Vector2 p in new[] { new Vector2(-2.35f, 1.85f), new Vector2(2.4f, 1.7f), new Vector2(-2.3f, -1.4f), new Vector2(2.3f, -1.3f),
                     new Vector2(-1.4f, -2.4f), new Vector2(1.35f, -2.3f), new Vector2(-2.35f, -3.15f), new Vector2(2.4f, -3.0f) })
            spots.Add(Child(root.transform, "Spot", p).transform);
        var candles = new List<Transform>();
        for (int i = 0; i < 3; i++)
        {
            Transform c = Child(root.transform, "Candle" + i, Vector2.zero).transform;
            Art(c, "Art", "haunt_candle", 10, false);
            Transform flame = Child(c, "Flame", new Vector2(0f, 0.72f)).transform;
            SpriteRenderer g = Art(flame, "Glow", "glow", 9, false).GetComponent<SpriteRenderer>();
            g.transform.localScale = Vector3.one * 1.8f;
            g.color = new Color(1f, 0.8f, 0.4f, 0.5f);
            Art(flame, "Fire", "flame", 12).localScale = Vector3.one * 1.4f;
            SpriteRenderer sp = Art(c, "Sparkle", "glow", 8, false, new Vector2(0f, 0.4f)).GetComponent<SpriteRenderer>();
            sp.transform.localScale = Vector3.one * 1.4f;
            sp.color = new Color(1f, 0.95f, 0.5f, 0.6f);
            candles.Add(c);
        }
        Transform eyes = Child(root.transform, "Eyes", Vector2.zero).transform;
        Art(eyes, "Art", "haunt_eyes", 10, false);

        Transform torch = Art(root.transform, "Torch", "darkness", 50, false);
        SpriteRenderer blackout = Bar(root.transform, "Blackout", Vector2.zero, new Vector2(12f, 16f), new Color(0.01f, 0.01f, 0.03f, 1f), 51);
        SpriteRenderer dim = Bar(root.transform, "Dim", Vector2.zero, new Vector2(12f, 16f), new Color(0.02f, 0.01f, 0.05f, 0.82f), 50);
        TextMesh found = Fit(Label(root.transform, new Vector2(2.2f, 2.45f), 55, "", 0.06f), 1.2f, 0.4f);
        found.color = Color.white;

        // fiammiferi
        Transform kit = Child(root.transform, "MatchKit", Vector2.zero).transform;
        Art(kit, "Box", "haunt_matchbox", 68, false, new Vector2(-1.0f, -3.95f));
        GameObject strike = Child(kit, "Strike", new Vector2(-1.0f, -3.65f));
        strike.AddComponent<BoxCollider2D>().size = new Vector2(1.9f, 0.6f);
        Transform match = Child(kit, "Match", new Vector2(1.3f, -3.1f)).transform;
        SpriteRenderer matchArt = Art(match, "Art", "haunt_match", 72, false).GetComponent<SpriteRenderer>();
        Transform mflame = Child(match, "Flame", new Vector2(0f, 0.04f)).transform;
        SpriteRenderer mg = Art(mflame, "Glow", "glow", 71, false).GetComponent<SpriteRenderer>();
        mg.transform.localScale = Vector3.one * 1.4f;
        mg.color = new Color(1f, 0.8f, 0.4f, 0.55f);
        Art(mflame, "Fire", "flame", 73).localScale = Vector3.one * 1.2f;
        Transform burnHolder = Child(match, "Burn", new Vector2(0f, 0.6f)).transform;
        Transform burn = GaugeBar(burnHolder, "Bar", Vector2.zero, new Vector2(0.7f, 0.09f), new Color(0.1f, 0.08f, 0.12f, 0.8f), new Color(1f, 0.6f, 0.2f), 74);
        TextMesh matches = Fit(Label(kit, new Vector2(1.8f, -4.35f), 75, "MATCHES: 3", 0.045f), 1.6f, 0.3f);
        matches.color = Color.white;

        // il fantasma
        Transform ghost = Child(root.transform, "Ghost", new Vector2(0f, -2.15f)).transform;
        ghost.localScale = Vector3.one * 0.95f;
        SpriteRenderer ghostArt = Art(ghost, "Art", "haunt_ghost", 75, false).GetComponent<SpriteRenderer>();
        ghost.gameObject.AddComponent<CircleCollider2D>().radius = 1.0f;
        Transform whisperHolder = Child(root.transform, "Whisper", new Vector2(0f, -0.85f)).transform;
        TextMesh whisper = Fit(Label(whisperHolder, Vector2.zero, 80, "", 0.07f), 5.2f, 0.5f);

        StageHud40A hud = V40A_Hud(root.transform, 3, 3);
        SetField(level, "hud", hud);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.9f, 0.3f)));
        SetField(level, "torch", torch);
        SetField(level, "blackout", blackout);
        SetField(level, "dim", dim);
        SetArray(level, "candles", candles.ToArray());
        SetArray(level, "candleSpots", spots.ToArray());
        SetField(level, "eyes", eyes);
        SetField(level, "foundLabel", found);
        SetField(level, "matchKit", kit.gameObject);
        SetField(level, "strikeArea", strike.GetComponent<Collider2D>());
        SetField(level, "match", match);
        SetField(level, "matchArt", matchArt);
        SetField(level, "matchSprite", sprites["haunt_match"]);
        SetField(level, "burntSprite", sprites["haunt_match_burnt"]);
        SetField(level, "matchFlame", mflame.gameObject);
        SetField(level, "burnBar", burn);
        SetField(level, "matchesLabel", matches);
        SetField(level, "ghost", ghost);
        SetField(level, "ghostArt", ghostArt);
        SetField(level, "ghostCalm", sprites["haunt_ghost"]);
        SetField(level, "ghostWave", sprites["haunt_ghost_wave"]);
        SetField(level, "ghostBoo", sprites["haunt_ghost_boo"]);
        SetField(level, "whisper", whisper);
        SetArray(level, "doors", doors.ToArray());
        SetArray(level, "doorLabels", doorLabels.ToArray());
        SetArray(level, "doorArts", doorArts.ToArray());
        SetField(level, "doorOpen", sprites["haunt_door_open"]);
        return SaveLevel(root, "Haunted");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V40A = new Dictionary<string, string[]>
    {
        { "morning_routine", new[] { "Tapping won't wake him: shake his whole world. Later, tilt to pour and stop at the lines.", "SHAKE the phone, drag both curtains aside, tilt LEFT gently and straighten up at the orange line, then tap the milk, hold on the bowl and let go in the green band." } },
        { "bank_heist", new[] { "Sweep the UV light over the whole painting. The year on the plaque is a decoy.", "Read the 4 glowing digits, type them and press OK. Then work out the riddle's wire from the colours and cut only that one." } },
        { "submarine", new[] { "Tilt to steer. When the air runs low, your own breath is the pump.", "Tilt left/right in the tunnel and BLOW into the mic to refill the oxygen; PING and tap the rock that echoes; set the dials to the 3 symbols on the flag." } },
        { "haunted_house", new[] { "The ghost whispers too quietly... what makes things louder on a phone?", "Find the candles with your finger, rub the match fast on the brown strip and light them, then press VOLUME UP a few times and tap the door with the whispered word." } },
    };
}
#endif
