#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli della specifica 105-200 (in gioco dal 151), pacchetto I.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels41I() => new List<LevelDefinition>
    {
        Define("TheStupidGenius", "the_stupid_genius", "THE STUPID GENIUS", "The machine might be unnecessary.", BuildS41IStupidGenius()),
        Define("TheEscapingCursor", "the_escaping_cursor", "THE ESCAPING CURSOR", "The cursor is faster than you.", BuildS41IEscapingCursor()),
        Define("UpsideDownCode", "upside_down_code", "UPSIDE-DOWN CODE", "You're looking at the code from the wrong angle.", BuildS41IUpsideDownCode()),
        Define("TheHumanCharger", "the_human_charger", "THE HUMAN CHARGER", "There is energy on the screen.", BuildS41IHumanCharger()),
        Define("Adpocalypse", "adpocalypse", "ADPOCALYPSE", "The close button isn't helping.", BuildS41IAdpocalypse(), "adpocalypse"),
        Define("TheGlassCage", "the_glass_cage", "THE GLASS CAGE", "The glass has an opening.", BuildS41IGlassCage()),
        Define("TheInvisibleInk", "the_invisible_ink", "THE INVISIBLE INK", "The paper isn't empty.", BuildS41IInvisibleInk()),
        Define("TheLastDrop", "the_last_drop", "THE LAST DROP", "The bottle isn't completely empty.", BuildS41ILastDrop()),
    };

    // ---------- Aiuti comuni del pacchetto I ----------

    /// <summary>Riga dell'obiettivo in alto, su una fascia scura.</summary>
    static TextMesh S41I_Goal(Transform root, string text)
    {
        Transform h = Child(root, "Goal", new Vector2(0f, 3.25f)).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(5.5f, 0.5f), new Color(0.12f, 0.09f, 0.16f, 0.72f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, text, 0.05f);
        t.color = Color.white;
        Fit(t, 5.3f, 0.42f);
        return t;
    }

    /// <summary>Etichetta bianca su fascia scura (contatori).</summary>
    static TextMesh S41I_Info(Transform root, string name, Vector2 pos, float width)
    {
        Transform h = Child(root, name, pos).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(width, 0.4f), new Color(0.12f, 0.09f, 0.16f, 0.6f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, "", 0.04f);
        t.color = Color.white;
        Fit(t, width - 0.2f, 0.32f);
        return t;
    }

    static SmokeEmitter[] S41I_Confetti(Transform root)
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

    static void S41I_Common(Component level, Transform root, Vector2 bubbleAt)
    {
        SetField(level, "bubble", SmallBubble(root, bubbleAt));
        SetArray(level, "confetti", S41I_Confetti(root));
    }

    // ---------- 171 The stupid genius ----------

    static GameObject BuildS41IStupidGenius()
    {
        var root = new GameObject("Level_TheStupidGenius");
        TheStupidGeniusLevel level = root.AddComponent<TheStupidGeniusLevel>();
        Art(root.transform, "Lab", "e40_workshop_bg", -20, false);
        Transform machine = Child(root.transform, "Machine", new Vector2(-0.75f, 0.3f)).transform;
        machine.localScale = Vector3.one * 1.2f;
        Art(machine, "Art", "s41i_machine", 2, false);
        Transform gear = Child(machine, "Gear", new Vector2(-0.35f, 0.55f)).transform;
        Art(gear, "Art", "s41i_gear", 3, false);
        BoxCollider2D mcol = machine.gameObject.AddComponent<BoxCollider2D>();
        mcol.size = new Vector2(2.9f, 3.3f);
        Transform glove = Child(root.transform, "Glove", new Vector2(1.08f, -1.2f)).transform;
        Art(glove, "Art", "s41i_glove", 4, false);

        Transform shelf = Child(root.transform, "Shelf", new Vector2(2.0f, -1.75f)).transform;
        Bar(shelf, "Top", Vector2.zero, new Vector2(1.1f, 0.12f), new Color(0.55f, 0.36f, 0.2f), 3);
        Bar(shelf, "Leg", new Vector2(0f, -1.3f), new Vector2(0.12f, 2.5f), new Color(0.45f, 0.28f, 0.15f), 2);
        Transform target = Child(root.transform, "Target", new Vector2(2.0f, -1.4f)).transform;
        target.localScale = Vector3.one * 1.4f;
        Art(target, "Art", "s41i_target_btn", 5, false);
        CircleCollider2D tcol = target.gameObject.AddComponent<CircleCollider2D>();
        tcol.radius = 0.38f;

        Transform start = Child(root.transform, "Start", new Vector2(-1.6f, -2.7f)).transform;
        start.localScale = Vector3.one * 1.3f;
        Art(start, "Art", "s41i_start_btn", 5, false);
        TextMesh st = Fit(Label(start, new Vector2(0f, -0.2f), 6, "START", 0.05f), 0.8f, 0.2f);
        st.color = Color.white;
        BoxCollider2D scol = start.gameObject.AddComponent<BoxCollider2D>();
        scol.size = new Vector2(1.1f, 0.8f);

        Transform sci = Child(root.transform, "Scientist", new Vector2(0.45f, -4.4f)).transform;
        sci.localScale = Vector3.one * 0.95f;
        SpriteRenderer sciArt = Art(sci, "Art", "s41i_scientist", 6, false).GetComponent<SpriteRenderer>();
        Transform boom = Child(root.transform, "Boom", new Vector2(-0.75f, 0.2f)).transform;
        boom.localScale = Vector3.one * 2.2f;
        Art(boom, "Art", "boom", 30, false);
        SmokeEmitter smoke = Emitter(root.transform, "Smoke", new Vector2(-1.6f, 2.2f), "puff", 6f, 1.6f, 1.0f, 0.4f, 0.6f, new Color(0.8f, 0.8f, 0.85f, 0.9f), 20);

        S41I_Goal(root.transform, "PRESS THE RED BUTTON");
        SetField(level, "machine", machine);
        SetField(level, "gear", gear);
        SetField(level, "glove", glove);
        SetField(level, "startButton", start);
        SetField(level, "target", target);
        SetField(level, "scientist", sci);
        SetField(level, "boom", boom);
        SetField(level, "startArea", scol);
        SetField(level, "targetArea", tcol);
        SetField(level, "machineArea", mcol);
        SetField(level, "smoke", smoke);
        SetField(level, "scientistArt", sciArt);
        SetField(level, "triesText", S41I_Info(root.transform, "Tries", new Vector2(0f, 2.75f), 3.6f));
        S41I_Common(level, root.transform, new Vector2(-0.6f, -1.75f));
        return SaveLevel(root, "TheStupidGenius");
    }

    // ---------- 172 The escaping cursor ----------

    static GameObject BuildS41IEscapingCursor()
    {
        var root = new GameObject("Level_TheEscapingCursor");
        TheEscapingCursorLevel level = root.AddComponent<TheEscapingCursorLevel>();
        Art(root.transform, "Desktop", "s41i_desktop_bg", -20, false);
        Transform target = Child(root.transform, "Target", new Vector2(-1.2f, -2.6f)).transform;
        Art(target, "Art", "s41i_target", 5, false);
        CircleCollider2D tcol = target.gameObject.AddComponent<CircleCollider2D>();
        tcol.radius = 0.55f;
        Transform cursor = Child(root.transform, "Cursor", new Vector2(1.0f, 0.8f)).transform;
        cursor.localScale = Vector3.one * 1.2f;
        Art(cursor, "Art", "s41i_cursor", 8, false);

        S41I_Goal(root.transform, "CLICK THE TARGET");
        SetField(level, "cursor", cursor);
        SetField(level, "target", target);
        SetField(level, "targetArea", tcol);
        SetField(level, "triesText", S41I_Info(root.transform, "Tries", new Vector2(0f, 2.75f), 3.6f));
        S41I_Common(level, root.transform, new Vector2(0.6f, -0.6f));
        return SaveLevel(root, "TheEscapingCursor");
    }

    // ---------- 173 Upside-down code ----------

    static GameObject BuildS41IUpsideDownCode()
    {
        var root = new GameObject("Level_UpsideDownCode");
        UpsideDownCodeLevel level = root.AddComponent<UpsideDownCodeLevel>();
        Art(root.transform, "Vault", "heist_vault_bg", -20, false);
        Transform safe = Child(root.transform, "Safe", new Vector2(0f, 1.0f)).transform;
        safe.localScale = Vector3.one * 1.25f;
        SpriteRenderer safeArt = Art(safe, "Art", "safe", 1, false).GetComponent<SpriteRenderer>();
        Transform plate = Child(root.transform, "Plate", new Vector2(0f, 1.15f)).transform;
        plate.localScale = Vector3.one * 0.8f;
        Art(plate, "Art", "s41i_code_plate", 4, false);
        SpriteRenderer glow = Art(plate, "Glow", "glow", 5, false).GetComponent<SpriteRenderer>();
        glow.transform.localScale = new Vector3(2.6f, 0.8f, 1f);
        glow.color = new Color(0.5f, 1f, 0.6f, 0f);
        TextMesh code = Label(plate, Vector2.zero, 6, "0000", 0.1f);
        code.transform.localRotation = Quaternion.Euler(0, 0, 180f);   // stampato capovolto
        Fit(code, 2.1f, 0.6f);

        Transform panel = Child(root.transform, "Keypad", new Vector2(0f, -2.55f)).transform;
        Bar(panel, "Back", new Vector2(0f, 0f), new Vector2(3.4f, 3.7f), new Color(0.16f, 0.17f, 0.22f, 0.9f), 2);
        Bar(panel, "Screen", new Vector2(0f, 1.45f), new Vector2(2.9f, 0.55f), new Color(0.08f, 0.12f, 0.1f), 3);
        TextMesh display = Fit(Label(panel, new Vector2(0f, 1.45f), 4, "____", 0.08f), 2.6f, 0.45f);
        display.color = new Color(0.55f, 1f, 0.6f);
        var keys = new Collider2D[12];
        string[] names = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "CLR", "OK" };
        for (int i = 0; i < 12; i++)
        {
            int slot = i == 0 ? 10 : i == 10 ? 9 : i == 11 ? 11 : i - 1;   // 1..9, poi CLR 0 OK
            Vector2 at = new Vector2(-1.0f + (slot % 3) * 1.0f, 0.65f - (slot / 3) * 0.75f);
            Transform k = Child(panel, "Key" + names[i], at).transform;
            k.localScale = Vector3.one * 1.15f;
            Art(k, "Art", "s41i_key_btn", 4, false);
            TextMesh t = Fit(Label(k, new Vector2(0f, 0.02f), 5, names[i], 0.06f), 0.6f, 0.4f);
            if (i >= 10) t.color = i == 10 ? new Color(0.85f, 0.25f, 0.3f) : new Color(0.2f, 0.6f, 0.3f);
            BoxCollider2D c = k.gameObject.AddComponent<BoxCollider2D>();
            c.size = new Vector2(0.82f, 0.64f);
            keys[i] = c;
        }

        Transform flip = Child(root.transform, "FlipButton", new Vector2(1.95f, -0.25f)).transform;
        Bar(flip, "Back", Vector2.zero, new Vector2(1.5f, 0.55f), new Color(0.3f, 0.5f, 0.9f), 8);
        TextMesh ft = Fit(Label(flip, Vector2.zero, 9, "HOLD: FLIP VIEW", 0.04f), 1.4f, 0.4f);
        ft.color = Color.white;
        BoxCollider2D fcol = flip.gameObject.AddComponent<BoxCollider2D>();
        fcol.size = new Vector2(1.5f, 0.8f);

        S41I_Goal(root.transform, "OPEN THE SAFE");
        SetField(level, "plate", plate);
        SetField(level, "safe", safe);
        SetField(level, "codeText", code);
        SetField(level, "display", display);
        SetArray(level, "keys", keys);
        SetField(level, "safeArt", safeArt);
        SetField(level, "glow", glow);
        SetField(level, "safeOpen", sprites["safe_open"]);
        SetField(level, "flipButton", flip.gameObject);
        SetField(level, "flipArea", fcol);
        SetField(level, "triesText", S41I_Info(root.transform, "Tries", new Vector2(0f, 2.75f), 3.6f));
        S41I_Common(level, root.transform, new Vector2(-1.3f, -0.3f));
        return SaveLevel(root, "UpsideDownCode");
    }

    // ---------- 174 The human charger ----------

    static GameObject BuildS41IHumanCharger()
    {
        var root = new GameObject("Level_TheHumanCharger");
        TheHumanChargerLevel level = root.AddComponent<TheHumanChargerLevel>();
        Art(root.transform, "Factory", "rf_bg", -20, false);
        Transform big = Child(root.transform, "BigBattery", new Vector2(0f, 1.55f)).transform;
        big.localScale = Vector3.one * 4f;
        Bar(big, "Fill", new Vector2(-0.02f, 0f), new Vector2(0.56f, 0.24f), new Color(0.4f, 0.9f, 0.45f), 1);
        Art(big, "Art", "battery", 2, false);
        BoxCollider2D bcol = big.gameObject.AddComponent<BoxCollider2D>();
        bcol.size = new Vector2(0.78f, 0.42f);
        Fit(Label(root.transform, new Vector2(0f, 0.6f), 3, "NO CABLE INCLUDED", 0.04f), 3f, 0.3f).color = Color.white;

        Transform warn = Child(root.transform, "LowPower", new Vector2(0f, -0.25f)).transform;
        Bar(warn, "Back", Vector2.zero, new Vector2(4.4f, 0.8f), new Color(0.85f, 0.2f, 0.25f, 0.9f), 3);
        TextMesh low = Fit(Label(warn, new Vector2(-1.25f, 0f), 4, "LOW", 0.09f), 1.4f, 0.6f);
        low.color = Color.white;
        TextMesh power = Fit(Label(warn, new Vector2(1.15f, 0f), 4, "POWER", 0.09f), 1.8f, 0.6f);
        power.color = Color.white;
        Transform bolt = Child(warn, "Bolt", new Vector2(-0.25f, 0f)).transform;
        bolt.localScale = Vector3.one * 1.1f;
        Art(bolt, "Art", "bolt", 6, false);
        BoxCollider2D boltCol = bolt.gameObject.AddComponent<BoxCollider2D>();
        boltCol.size = new Vector2(0.8f, 0.9f);

        Transform robot = Child(root.transform, "Robot", new Vector2(0f, -4.45f)).transform;
        robot.localScale = Vector3.one * 0.8f;
        SpriteRenderer robotArt = Art(robot, "Art", "robot_off", 2, false).GetComponent<SpriteRenderer>();
        BoxCollider2D rcol = robot.gameObject.AddComponent<BoxCollider2D>();
        rcol.size = new Vector2(2.6f, 3.4f);
        rcol.offset = new Vector2(0f, 1.5f);
        TextMesh percent = Fit(Label(root.transform, new Vector2(1.75f, -2.0f), 5, "1%", 0.08f), 1.2f, 0.5f);
        Transform charge = GaugeBar(root.transform, "Charge", new Vector2(1.75f, -2.55f), new Vector2(1.1f, 0.16f), new Color(0.12f, 0.09f, 0.16f, 0.8f), new Color(0.4f, 0.9f, 0.45f), 5);
        Fit(Label(root.transform, new Vector2(-1.1f, 2.75f), 72, "SHUTDOWN IN", 0.035f), 1.6f, 0.3f).color = Color.white;
        Bar(root.transform, "TimerBack", new Vector2(0f, 2.75f), new Vector2(5.4f, 0.4f), new Color(0.12f, 0.09f, 0.16f, 0.6f), 70);
        Transform timer = GaugeBar(root.transform, "Timer", new Vector2(0.9f, 2.75f), new Vector2(2.6f, 0.16f), new Color(0.2f, 0.2f, 0.25f), new Color(1f, 0.75f, 0.3f), 71);

        S41I_Goal(root.transform, "CHARGE THE ROBOT!");
        SetField(level, "robot", robot);
        SetField(level, "bolt", bolt);
        SetField(level, "timerFill", timer);
        SetField(level, "chargeFill", charge);
        SetField(level, "boltArea", boltCol);
        SetField(level, "robotArea", rcol);
        SetField(level, "bigBatteryArea", bcol);
        SetField(level, "robotArt", robotArt);
        SetField(level, "robotOnSprite", sprites["robot_on"]);
        SetField(level, "percentText", percent);
        S41I_Common(level, root.transform, new Vector2(-1.2f, -1.2f));
        return SaveLevel(root, "TheHumanCharger");
    }

    // ---------- 176 Adpocalypse ----------

    static GameObject BuildS41IAdpocalypse()
    {
        var root = new GameObject("Level_Adpocalypse");
        AdpocalypseLevel level = root.AddComponent<AdpocalypseLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);
        Transform cheese = Child(root.transform, "Cheese", new Vector2(1.7f, -3.95f)).transform;
        Art(cheese, "Art", "cheese_wedge", 2, false);
        Transform mouse = Child(root.transform, "Mouse", new Vector2(0f, -1.4f)).transform;
        mouse.localScale = Vector3.one * 0.9f;
        Art(mouse, "Art", "mouse", 3, false);

        Transform banner = Child(root.transform, "Banner", new Vector2(0f, -0.7f)).transform;
        Art(banner, "Art", "s41i_ad_banner", 10, false);
        TextMesh head = Fit(Label(banner, new Vector2(0f, 1.35f), 11, "MEGA CHEESE 3000!!", 0.08f), 4.3f, 0.55f);
        head.color = Color.white;
        Fit(Label(banner, new Vector2(-0.6f, 0.45f), 11, "BUY NOW! ONLY 999 COINS!", 0.06f), 3.4f, 0.5f);
        Fit(Label(banner, new Vector2(1.55f, -0.95f), 12, "SALE", 0.06f), 0.75f, 0.35f).color = Color.white;
        Fit(Label(banner, new Vector2(0f, -1.62f), 11, "(this is a fake ad)", 0.035f), 2.4f, 0.2f);
        BoxCollider2D bcol = banner.gameObject.AddComponent<BoxCollider2D>();
        bcol.size = new Vector2(4.8f, 3.6f);
        Transform closeX = Child(banner, "CloseX", new Vector2(2.05f, 1.5f)).transform;
        closeX.localScale = Vector3.one * 1.1f;
        Art(closeX, "Art", "s41i_ad_x", 14, false);

        // pubblicità parodia dell'Easter egg
        string[] titles = { "UNDERPANTS FOR FISH!", "GHOST INSURANCE", "PIGEON PRO COURSE", "PREMIUM BREATHING" };
        string[] subs = { "Finally! Fish deserve it.", "Covers haunting & boo-boos", "Learn to coo like a boss", "Now only 9.99/breath!" };
        string[] icons = { "s41i_icon_pants", "s41d_ghost", "s41i_icon_pigeon", "s41i_icon_lungs" };
        Color[] tints = { new Color(1f, 0.85f, 0.9f), new Color(0.85f, 0.92f, 1f), new Color(0.9f, 1f, 0.85f), new Color(1f, 0.95f, 0.8f) };
        Vector2[] spots =
        {
            new Vector2(-1.5f, 2.1f), new Vector2(1.4f, 1.9f), new Vector2(-1.3f, 0.6f), new Vector2(1.5f, 0.3f),
            new Vector2(0f, -0.7f), new Vector2(-1.5f, -1.4f), new Vector2(1.5f, -1.7f), new Vector2(0.1f, 1.2f),
            new Vector2(-1.4f, -2.7f), new Vector2(1.3f, -2.9f), new Vector2(0f, -2.1f), new Vector2(0f, 2.3f),
        };
        var ads = new List<Transform>();
        var xs = new List<Transform>();
        for (int i = 0; i < spots.Length; i++)
        {
            int kind = i % 4;
            Transform ad = Child(root.transform, "Ad" + i, spots[i]).transform;
            ad.localRotation = Quaternion.Euler(0, 0, ((i * 37) % 17) - 8f);
            int o = 20 + i * 3;
            Art(ad, "Art", "s41i_ad_small", o, false).GetComponent<SpriteRenderer>().color = tints[kind];
            Fit(Label(ad, new Vector2(-0.1f, 0.58f), o + 1, titles[kind], 0.04f), 1.6f, 0.2f);
            Transform icon = Art(ad, "Icon", icons[kind], o + 1, false, new Vector2(-0.62f, -0.15f));
            icon.localScale = Vector3.one * (kind == 1 ? 0.45f : 0.9f);
            if (kind == 0) Art(ad, "Fish", "fish", o + 2, false, new Vector2(-0.45f, -0.4f)).localScale = Vector3.one * 0.5f;
            Fit(Label(ad, new Vector2(0.35f, -0.15f), o + 1, subs[kind], 0.035f), 1.25f, 0.6f);
            Transform x = Child(ad, "X", new Vector2(0.88f, 0.58f)).transform;
            x.localScale = Vector3.one * 0.38f;
            Art(x, "Art", "s41i_ad_x", o + 2, false);
            ads.Add(ad);
            xs.Add(x);
        }

        Transform exec = Child(root.transform, "Executive", new Vector2(-0.6f, -4.3f)).transform;
        exec.localScale = Vector3.one * 1.1f;
        Art(exec, "Art", "s41i_exec", 60, false);
        var strikers = new List<Transform>();
        for (int i = 0; i < 6; i++)
        {
            Transform s = Child(root.transform, "Striker" + i, new Vector2(-2.3f + i * 0.55f + (i >= 2 ? 0.9f : 0f), -3.9f)).transform;
            Art(s, "Art", "s41i_ad_x_alive", 61, false);
            if (i % 2 == 0)
            {
                Transform sign = Art(s, "Sign", "s41i_picket", 60, false, new Vector2(0.25f, 0.55f));
                sign.localScale = Vector3.one * 0.8f;
                Fit(Label(sign, new Vector2(0f, 0.2f), 62, "STRIKE!", 0.05f), 0.8f, 0.35f).color = new Color(0.85f, 0.15f, 0.25f);
            }
            strikers.Add(s);
        }
        Transform giant = Child(root.transform, "GiantX", new Vector2(0f, -0.3f)).transform;
        giant.localScale = Vector3.one * 9f;
        Art(giant, "Art", "s41i_ad_x", 70, false);

        S41I_Goal(root.transform, "FREE THE MOUSE FROM THE AD");
        SetField(level, "banner", banner);
        SetField(level, "closeX", closeX);
        SetField(level, "mouse", mouse);
        SetField(level, "cheese", cheese);
        SetField(level, "executive", exec);
        SetField(level, "giantX", giant);
        SetField(level, "bannerArea", bcol);
        SetArray(level, "ads", ads.ToArray());
        SetArray(level, "adXs", xs.ToArray());
        SetArray(level, "strikers", strikers.ToArray());
        SetField(level, "pressText", S41I_Info(root.transform, "Presses", new Vector2(0f, 2.75f), 3.2f));
        S41I_Common(level, root.transform, new Vector2(-1.2f, 1.7f));
        return SaveLevel(root, "Adpocalypse");
    }

    // ---------- 177 The glass cage ----------

    static GameObject BuildS41IGlassCage()
    {
        var root = new GameObject("Level_TheGlassCage");
        TheGlassCageLevel level = root.AddComponent<TheGlassCageLevel>();
        Art(root.transform, "Table", "table_bg", -20, false);
        Transform cheese = Child(root.transform, "Cheese", new Vector2(1.6f, -3.6f)).transform;
        Art(cheese, "Art", "cheese_wedge", 2, false);
        Transform mouse = Child(root.transform, "Mouse", new Vector2(0f, -0.4f)).transform;
        mouse.localScale = Vector3.one * 0.55f;
        Art(mouse, "Art", "mouse", 4, false);
        Transform alert = Child(root.transform, "Alert", Vector2.zero).transform;
        alert.localScale = Vector3.one * 0.6f;
        Art(alert, "Art", "alert", 9, false);
        Transform dome = Child(root.transform, "Dome", new Vector2(0f, -0.4f)).transform;
        dome.localScale = Vector3.one * 1.2f;
        Art(dome, "Art", "s41i_dome", 6, false);
        dome.gameObject.AddComponent<CircleCollider2D>().radius = 1.4f;

        S41I_Goal(root.transform, "FREE THE MOUSE");
        SetField(level, "dome", dome);
        SetField(level, "mouse", mouse);
        SetField(level, "cheese", cheese);
        SetField(level, "alert", alert);
        SetField(level, "domeArea", dome.GetComponent<Collider2D>());
        SetField(level, "triesText", S41I_Info(root.transform, "Tries", new Vector2(0f, 2.75f), 3.2f));
        S41I_Common(level, root.transform, new Vector2(-1.1f, 1.9f));
        return SaveLevel(root, "TheGlassCage");
    }

    // ---------- 178 The invisible ink ----------

    static GameObject BuildS41IInvisibleInk()
    {
        var root = new GameObject("Level_TheInvisibleInk");
        TheInvisibleInkLevel level = root.AddComponent<TheInvisibleInkLevel>();
        Art(root.transform, "Desk", "table_bg", -20, false);
        Transform paper = Child(root.transform, "Map", new Vector2(0f, -0.4f)).transform;
        Art(paper, "Art", "s41i_blank_map", 1, false);

        // sentiero d'inchiostro simpatico (coordinate mondo)
        var dots = new List<SpriteRenderer>();
        Vector2[] path = { new Vector2(-1.7f, -2.5f), new Vector2(-1.1f, -1.9f), new Vector2(-1.6f, -1.0f), new Vector2(-0.6f, -0.5f),
                           new Vector2(0.6f, -1.3f), new Vector2(1.5f, -0.6f), new Vector2(1.0f, 0.4f), new Vector2(-0.4f, 0.6f),
                           new Vector2(-1.2f, 1.3f), new Vector2(0.2f, 1.6f), new Vector2(1.3f, 1.4f) };
        Color ink = new Color(0.36f, 0.17f, 0.45f);
        for (int s = 0; s < path.Length - 1; s++)
        {
            Vector2 a = path[s], b = path[s + 1];
            int n = Mathf.Max(1, Mathf.RoundToInt(Vector2.Distance(a, b) / 0.27f));
            for (int k = 0; k < n; k++)
            {
                Vector2 p = Vector2.Lerp(a, b, k / (float)n);
                SpriteRenderer d = Art(root.transform, "Dot", "ink_dot", 3, false, p).GetComponent<SpriteRenderer>();
                d.transform.localScale = Vector3.one * 0.7f;
                d.color = ink;
                dots.Add(d);
            }
        }
        SpriteRenderer x = Art(root.transform, "X", "s41i_ink_x", 3, false, path[path.Length - 1] + new Vector2(0.15f, 0.1f)).GetComponent<SpriteRenderer>();
        dots.Add(x);
        TextMesh here = Fit(Label(root.transform, new Vector2(1.35f, 1.85f), 3, "HERE!", 0.06f), 1.2f, 0.4f);
        here.color = ink;

        var scorches = new List<SpriteRenderer>();
        Vector2 gridMin = new Vector2(-2f, -2.65f);
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 9; c++)
                scorches.Add(Art(root.transform, "Scorch", "s41i_scorch", 2, false, gridMin + new Vector2(c * 0.5f, r * 0.5f)).GetComponent<SpriteRenderer>());

        Transform token = Child(root.transform, "Token", Vector2.zero).transform;
        token.localScale = Vector3.one * 0.4f;
        Art(token, "Art", "mouse", 6, false);
        Transform treasure = Child(root.transform, "Treasure", new Vector2(1.45f, 1.5f)).transform;
        Art(treasure, "Art", "cheese_wedge", 7, false);

        Transform dryer = Child(root.transform, "Dryer", new Vector2(1.6f, -4.3f)).transform;
        Art(dryer, "Art", "s41i_dryer", 10, false);
        BoxCollider2D dcol = dryer.gameObject.AddComponent<BoxCollider2D>();
        dcol.size = new Vector2(1.5f, 1.5f);
        dcol.offset = new Vector2(0.15f, 0.75f);
        SmokeEmitter hot = Emitter(dryer, "HotAir", new Vector2(0f, -0.1f), "puff", 10f, 0.5f, -1.2f, 0.25f, 0.35f, new Color(1f, 0.85f, 0.7f, 0.6f), 9);
        SmokeEmitter fire = Emitter(root.transform, "Fire", Vector2.zero, "puff", 12f, 1.4f, 1.4f, 0.6f, 0.8f, new Color(0.35f, 0.3f, 0.3f, 0.9f), 20);

        S41I_Goal(root.transform, "FIND THE HIDDEN PATH");
        SetField(level, "dryer", dryer);
        SetField(level, "paper", paper);
        SetField(level, "token", token);
        SetField(level, "treasure", treasure);
        SetField(level, "dryerArea", dcol);
        SetArray(level, "dots", dots.ToArray());
        SetArray(level, "scorches", scorches.ToArray());
        SetField(level, "hereText", here);
        SetField(level, "hotAir", hot);
        SetField(level, "fireSmoke", fire);
        S41I_Common(level, root.transform, new Vector2(-0.8f, 2.55f));
        return SaveLevel(root, "TheInvisibleInk");
    }

    // ---------- 179 The last drop ----------

    static GameObject BuildS41ILastDrop()
    {
        var root = new GameObject("Level_TheLastDrop");
        TheLastDropLevel level = root.AddComponent<TheLastDropLevel>();
        Art(root.transform, "Garden", "garden_bg", -20, false);
        Bar(root.transform, "Beam", new Vector2(0f, 2.95f), new Vector2(3.0f, 0.18f), new Color(0.55f, 0.36f, 0.2f), 1);
        Bar(root.transform, "String", new Vector2(0f, 2.8f), new Vector2(0.04f, 0.3f), new Color(0.25f, 0.2f, 0.2f), 1);
        Transform pivot = Child(root.transform, "Pivot", new Vector2(0f, 2.68f)).transform;
        Art(pivot, "Bottle", "s41i_bottle", 3, false);
        Transform drop = Child(pivot, "Drop", new Vector2(-0.26f, -2.58f)).transform;
        drop.localScale = Vector3.one * 0.55f;
        Art(drop, "Art", "drop", 4, false);
        SpriteRenderer glint = Art(pivot, "Glint", "glow", 5, false, new Vector2(0f, -2.62f)).GetComponent<SpriteRenderer>();
        glint.transform.localScale = Vector3.one * 0.6f;
        glint.color = new Color(0.6f, 0.9f, 1f, 0f);
        Transform falling = Child(root.transform, "FallingDrop", Vector2.zero).transform;
        falling.localScale = Vector3.one * 0.7f;
        Art(falling, "Art", "drop", 6, false);

        Transform pot = Child(root.transform, "Pot", new Vector2(0f, -3.75f)).transform;
        SpriteRenderer plant = Art(pot, "Plant", "plant_dry", 1, false, new Vector2(0f, 0.05f)).GetComponent<SpriteRenderer>();
        Art(pot, "Art", "pot", 2, false);

        Transform buttons = Child(root.transform, "TiltButtons", Vector2.zero).transform;
        var areas = new List<Collider2D>();
        foreach (float s in new[] { -1f, 1f })
        {
            Transform b = Child(buttons, s < 0 ? "Left" : "Right", new Vector2(s * 2.0f, -2.3f)).transform;
            Bar(b, "Back", Vector2.zero, new Vector2(1.1f, 0.8f), new Color(0.3f, 0.5f, 0.9f, 0.9f), 8);
            Fit(Label(b, Vector2.zero, 9, s < 0 ? "< TILT" : "TILT >", 0.05f), 1f, 0.5f).color = Color.white;
            BoxCollider2D c = b.gameObject.AddComponent<BoxCollider2D>();
            c.size = new Vector2(1.2f, 0.9f);
            areas.Add(c);
        }

        S41I_Goal(root.transform, "GIVE THE PLANT ONE LAST DROP");
        SetField(level, "pivot", pivot);
        SetField(level, "drop", drop);
        SetField(level, "fallingDrop", falling);
        SetField(level, "pot", pot);
        SetField(level, "plantArt", plant);
        SetField(level, "readyGlint", glint);
        SetField(level, "bloomSprite", sprites["plant_bloom"]);
        SetField(level, "tiltButtons", buttons.gameObject);
        SetField(level, "leftArea", areas[0]);
        SetField(level, "rightArea", areas[1]);
        SetField(level, "triesText", S41I_Info(root.transform, "Tries", new Vector2(0f, -4.66f), 3.2f));
        S41I_Common(level, root.transform, new Vector2(1.3f, -1.0f));
        return SaveLevel(root, "TheLastDrop");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V41I = new Dictionary<string, string[]>
    {
        { "the_stupid_genius", new[] { "What is the machine actually doing?", "Tap the target button directly." } },
        { "the_escaping_cursor", new[] { "Move the target instead.", "Drag the target onto the cursor." } },
        { "upside_down_code", new[] { "Turn the device.", "Rotate the phone and read the numbers." } },
        { "the_human_charger", new[] { "Look for the lightning symbol.", "Drag the lightning bolt to the robot four times." } },
        { "adpocalypse", new[] { "You can move the entire ad.", "Drag the banner out of the screen." } },
        { "the_glass_cage", new[] { "Look underneath it.", "Rotate the dome until the exit faces the mouse." } },
        { "the_invisible_ink", new[] { "Heat can reveal hidden ink.", "Move the hair dryer across the sheet without stopping." } },
        { "the_last_drop", new[] { "Check the cap.", "Tilt the phone to free the last drop." } },
    };
}
#endif
