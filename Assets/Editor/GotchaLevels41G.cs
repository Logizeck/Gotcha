#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli della specifica 105-200 (in gioco dal 151), pacchetto G.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels41G() => new List<LevelDefinition>
    {
        Define("UnfairRace", "unfair_race", "UNFAIR RACE", "You can't make the snail faster.", BuildS41GUnfairRace()),
        Define("TheBottomlessCup", "the_bottomless_cup", "THE BOTTOMLESS CUP", "Something is leaking.", BuildS41GBottomlessCup()),
        Define("TheDirtySecret", "the_dirty_secret", "THE DIRTY SECRET", "The screen needs cleaning.", BuildS41GDirtySecret(), "dirty_secret"),
        Define("TheFakeVirus", "the_fake_virus", "THE FAKE VIRUS", "Only one is the real virus.", BuildS41GFakeVirus()),
        Define("SlowMotion", "slow_motion", "SLOW MOTION", "Everything is moving too fast.", BuildS41GSlowMotion()),
        Define("TheOverworkedRobot", "the_overworked_robot", "THE OVERWORKED ROBOT", "Two hands aren't enough.", BuildS41GOverworkedRobot()),
        Define("TheMissingLetter", "the_missing_letter", "THE MISSING LETTER", "The sign is incomplete.", BuildS41GMissingLetter()),
        Define("AntiGravityTaxi", "anti_gravity_taxi", "ANTI-GRAVITY TAXI", "The taxi needs to go up.", BuildS41GAntiGravityTaxi()),
    };

    // ---------- Aiuti comuni del pacchetto G ----------

    /// <summary>Riga dell'obiettivo in alto, su una fascia scura.</summary>
    static TextMesh S41G_Goal(Transform root, string text)
    {
        Transform h = Child(root, "Goal", new Vector2(0f, 3.25f)).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(5.5f, 0.5f), new Color(0.12f, 0.09f, 0.16f, 0.72f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, text, 0.05f);
        t.color = Color.white;
        Fit(t, 5.3f, 0.42f);
        return t;
    }

    /// <summary>Etichetta bianca su fascia scura (contatori).</summary>
    static TextMesh S41G_Info(Transform root, string name, Vector2 pos, float width)
    {
        Transform h = Child(root, name, pos).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(width, 0.4f), new Color(0.12f, 0.09f, 0.16f, 0.6f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, "", 0.04f);
        t.color = Color.white;
        Fit(t, width - 0.2f, 0.32f);
        return t;
    }

    /// <summary>Pulsante a pillola colorato con scritta e collider.</summary>
    static Transform S41G_Button(Transform root, string name, Vector2 pos, string text, Color tint, float scale, int order = 20)
    {
        Transform b = Child(root, name, pos).transform;
        b.localScale = Vector3.one * scale;
        Art(b, "Art", "s41g_button", order, false).GetComponent<SpriteRenderer>().color = tint;
        Fit(Label(b, new Vector2(0f, 0.02f), order + 1, text, 0.05f), 1.35f, 0.4f);
        b.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.7f, 0.8f);
        return b;
    }

    static SmokeEmitter[] S41G_Confetti(Transform root)
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

    // ---------- 155 Unfair race ----------

    static GameObject BuildS41GUnfairRace()
    {
        var root = new GameObject("Level_UnfairRace");
        UnfairRaceLevel level = root.AddComponent<UnfairRaceLevel>();
        Art(root.transform, "Track", "s41g_race_bg", -20, false);
        Transform hare = Child(root.transform, "Hare", new Vector2(-2.35f, -0.25f)).transform;
        hare.localScale = Vector3.one * 0.9f;
        Art(hare, "Art", "s41g_hare", 6, false);
        Transform snail = Child(root.transform, "Snail", new Vector2(-2.45f, -2.0f)).transform;
        Art(snail, "Art", "s41g_snail", 6, false);
        Transform finish = Child(root.transform, "Finish", new Vector2(2.2f, -0.7f)).transform;
        Art(finish, "Art", "s41g_finish", 5, false);
        Bar(finish, "TagBack", new Vector2(0f, 1.5f), new Vector2(0.95f, 0.32f), new Color(0.9f, 0.27f, 0.23f), 7);
        TextMesh tag = Fit(Label(finish, new Vector2(0f, 1.5f), 8, "FINISH", 0.04f), 0.85f, 0.26f);
        tag.color = Color.white;
        BoxCollider2D fc = finish.gameObject.AddComponent<BoxCollider2D>();
        fc.size = new Vector2(1.0f, 2.9f);
        fc.offset = new Vector2(0f, 0.15f);
        Transform go = S41G_Button(root.transform, "Start", new Vector2(0f, -3.55f), "START RACE", new Color(0.55f, 0.92f, 0.55f), 1.25f);
        TextMesh countdown = Label(root.transform, new Vector2(0f, -0.7f), 30, "", 0.2f);
        countdown.color = new Color(1f, 0.85f, 0.2f);
        Fit(countdown, 4.5f, 1.2f);

        S41G_Goal(root.transform, "HELP THE SNAIL WIN THE RACE");
        SetField(level, "hare", hare);
        SetField(level, "snail", snail);
        SetField(level, "finish", finish);
        SetField(level, "goButton", go);
        SetField(level, "finishArea", fc);
        SetField(level, "goArea", go.GetComponent<Collider2D>());
        SetField(level, "countdown", countdown);
        SetField(level, "triesText", S41G_Info(root.transform, "Tries", new Vector2(0f, 2.65f), 3.6f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.1f, 1.75f)));
        SetArray(level, "confetti", S41G_Confetti(root.transform));
        return SaveLevel(root, "UnfairRace");
    }

    // ---------- 156 The bottomless cup ----------

    static GameObject BuildS41GBottomlessCup()
    {
        var root = new GameObject("Level_TheBottomlessCup");
        BottomlessCupLevel level = root.AddComponent<BottomlessCupLevel>();
        Art(root.transform, "Kitchen", "s41g_kitchen_bg", -20, false);
        Vector2 glassAt = new Vector2(-0.3f, -2.86f);
        Transform water = Child(root.transform, "Water", glassAt + new Vector2(0f, 0.12f)).transform;
        Bar(water, "Fill", new Vector2(0f, 0.99f), new Vector2(1.44f, 1.98f), new Color(0.35f, 0.7f, 1f, 0.8f), 3);
        Art(root.transform, "Glass", "s41g_glass", 4, false, glassAt);
        Transform stream = Child(root.transform, "Stream", new Vector2(-0.3f, 0.9f)).transform;
        Bar(stream, "Fill", new Vector2(0f, -0.5f), new Vector2(0.14f, 1f), new Color(0.45f, 0.78f, 1f, 0.9f), 2);
        Art(root.transform, "Tap", "s41g_tap", 5, false, new Vector2(-0.3f, 0.9f));
        Transform handle = Child(root.transform, "Handle", new Vector2(0.75f, 2.35f)).transform;
        handle.localScale = Vector3.one * 1.2f;
        Art(handle, "Art", "s41g_tap_handle", 6, false);
        handle.gameObject.AddComponent<CircleCollider2D>().radius = 0.55f;
        Bar(root.transform, "HoldBack", new Vector2(1.95f, 2.35f), new Vector2(1.1f, 0.34f), new Color(0.12f, 0.09f, 0.16f, 0.6f), 6);
        TextMesh hold = Fit(Label(root.transform, new Vector2(1.95f, 2.35f), 7, "HOLD", 0.04f), 1.0f, 0.28f);
        hold.color = Color.white;
        Transform hole = Child(root.transform, "Hole", glassAt + new Vector2(0f, 0.07f)).transform;
        hole.gameObject.AddComponent<CircleCollider2D>().radius = 0.55f;
        Transform cork = Child(root.transform, "Cork", glassAt + new Vector2(0f, 0.05f)).transform;
        Art(cork, "Art", "s41g_cork", 6, false);
        Transform mouse = Child(root.transform, "Mouse", new Vector2(1.75f, -2.3f)).transform;
        mouse.localScale = Vector3.one * 0.9f;
        Art(mouse, "Art", "mouse", 5, false);

        S41G_Goal(root.transform, "FILL THE GLASS FOR THE THIRSTY MOUSE");
        SetField(level, "holeArea", hole.GetComponent<Collider2D>());
        SetField(level, "handleArea", handle.GetComponent<Collider2D>());
        SetField(level, "handle", handle);
        SetField(level, "water", water);
        SetField(level, "stream", stream);
        SetField(level, "hole", hole);
        SetField(level, "cork", cork);
        SetField(level, "mouse", mouse);
        SetField(level, "dropSprite", sprites["drop"]);
        SetField(level, "triesText", S41G_Info(root.transform, "Tries", new Vector2(0f, -4.55f), 4.2f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.45f, -0.95f)));
        SetArray(level, "confetti", S41G_Confetti(root.transform));
        return SaveLevel(root, "TheBottomlessCup");
    }

    // ---------- 157 The dirty secret ----------

    static GameObject BuildS41GDirtySecret()
    {
        var root = new GameObject("Level_TheDirtySecret");
        DirtySecretLevel level = root.AddComponent<DirtySecretLevel>();
        Art(root.transform, "Beach", "s41g_beach_bg", -20, false);
        Transform exit = Child(root.transform, "Exit", new Vector2(0.4f, -1.2f)).transform;
        Art(exit, "Art", "s41g_exit_btn", 5, false);
        Fit(Label(exit, new Vector2(0.22f, 0.03f), 6, "EXIT", 0.07f), 1.0f, 0.5f).color = Color.white;
        exit.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.7f, 1.0f);
        Vector2 corner = new Vector2(-2.2f, -3.9f);
        Transform hatch = Child(root.transform, "HatchHole", corner).transform;
        Art(hatch, "Art", "s41g_hatch_hole", 11, false);
        Transform smudge = Child(root.transform, "Smudge", corner).transform;
        Art(smudge, "Art", "s41g_round_smudge", 12, false);
        Transform cleaner = Child(root.transform, "Cleaner", new Vector2(-1.3f, -3.4f)).transform;
        cleaner.localScale = Vector3.one * 1.3f;
        SpriteRenderer cleanerArt = Art(cleaner, "Art", "s41g_cleaner_0", 14, false).GetComponent<SpriteRenderer>();
        Transform rag = Child(cleaner, "Rag", new Vector2(0.6f, 1.36f)).transform;
        Art(rag, "Art", "s41g_rag", 15, false);
        Transform holder = Child(cleaner, "BubbleAt", new Vector2(0.9f, 2.3f)).transform;
        TextMesh alert = Label(cleaner, new Vector2(0f, 2.0f), 16, "", 0.12f);
        alert.color = new Color(0.9f, 0.2f, 0.2f);

        S41G_Goal(root.transform, "CLEAN THE SCREEN");
        SetArray(level, "dirtSprites", new Object[] { sprites["s41g_fprint"], sprites["s41g_grease"] });
        SetArray(level, "cleanerPoses", new Object[] { sprites["s41g_cleaner_0"], sprites["s41g_cleaner_1"], sprites["s41g_cleaner_2"], sprites["s41g_cleaner_3"] });
        SetField(level, "exitButton", exit);
        SetField(level, "smudge", smudge);
        SetField(level, "hatchHole", hatch);
        SetField(level, "cleaner", cleaner);
        SetField(level, "rag", rag);
        SetField(level, "bubbleHolder", holder);
        SetField(level, "exitArea", exit.GetComponent<Collider2D>());
        SetField(level, "cleanerArt", cleanerArt);
        SetField(level, "percentText", S41G_Info(root.transform, "Percent", new Vector2(0f, 2.75f), 3.4f));
        SetField(level, "alertText", alert);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.7f, 1.9f)));
        SetArray(level, "confetti", S41G_Confetti(root.transform));
        return SaveLevel(root, "TheDirtySecret");
    }

    // ---------- 158 The fake virus ----------

    static GameObject BuildS41GFakeVirus()
    {
        var root = new GameObject("Level_TheFakeVirus");
        FakeVirusLevel level = root.AddComponent<FakeVirusLevel>();
        Art(root.transform, "Desktop", "s41g_desktop_bg", -20, false);
        string[] names = { "HOMEWORK", "CAT PICS", "TOP SECRET" };
        for (int i = 0; i < 3; i++)
        {
            Vector2 at = new Vector2(-2.05f, 1.9f - i * 1.0f);
            Art(root.transform, "Folder", "s41g_folder", 1, false, at);
            TextMesh t = Fit(Label(root.transform, at + new Vector2(0f, -0.42f), 2, names[i], 0.035f), 0.95f, 0.22f);
            t.color = Color.white;
        }
        Transform bin = Child(root.transform, "Bin", new Vector2(2.0f, -2.15f)).transform;
        Art(bin, "Art", "s41g_bin", 5, false);
        TextMesh binLabel = Fit(Label(bin, new Vector2(0f, -0.72f), 6, "BIN", 0.04f), 0.9f, 0.22f);
        binLabel.color = Color.white;
        BoxCollider2D binCol = bin.gameObject.AddComponent<BoxCollider2D>();
        binCol.size = new Vector2(1.3f, 1.4f);
        Transform spawn = Child(root.transform, "Spawn", new Vector2(-0.4f, 0.4f)).transform;
        TextMesh achoo = Label(root.transform, Vector2.zero, 40, "", 0.05f);
        achoo.color = new Color(1f, 0.95f, 0.4f);

        S41G_Goal(root.transform, "DRAG THE REAL VIRUS INTO THE BIN");
        SetField(level, "idleSprite", sprites["s41g_virus"]);
        SetField(level, "sneezeSprite", sprites["s41g_virus_sneeze"]);
        SetField(level, "puffSprite", sprites["puff"]);
        SetField(level, "bin", bin);
        SetField(level, "spawnPoint", spawn);
        SetField(level, "binArea", binCol);
        SetField(level, "statusText", S41G_Info(root.transform, "Status", new Vector2(0f, 2.45f), 3.6f));
        SetField(level, "triesText", S41G_Info(root.transform, "Tries", new Vector2(0f, -3.72f), 3.8f));
        SetField(level, "achoo", achoo);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.6f, 1.55f)));
        SetArray(level, "confetti", S41G_Confetti(root.transform));
        return SaveLevel(root, "TheFakeVirus");
    }

    // ---------- 159 Slow motion ----------

    static GameObject BuildS41GSlowMotion()
    {
        var root = new GameObject("Level_SlowMotion");
        SlowMotionLevel level = root.AddComponent<SlowMotionLevel>();
        Art(root.transform, "Arcade", "s41g_lanes_bg", -20, false);
        Transform cannon = Child(root.transform, "Cannon", new Vector2(-2.25f, 1.2f)).transform;
        Art(cannon, "Art", "s41g_cannon", 6, false);
        Transform ball = Child(root.transform, "Ball", new Vector2(-1.7f, 1.2f)).transform;
        Art(ball, "Art", "s41g_ball", 7, false);
        Transform bumper = Child(root.transform, "Bumper", new Vector2(1.9f, -1.2f)).transform;
        Art(bumper, "Art", "s41g_bumper", 6, false);
        bumper.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.1f, 1.4f);
        SpriteRenderer tint = Bar(root.transform, "Tint", Vector2.zero, new Vector2(12f, 16f), new Color(0.3f, 0.55f, 1f, 0f), 50);
        TextMesh slowLabel = Label(root.transform, new Vector2(0f, 2.45f), 60, "", 0.09f);
        slowLabel.color = new Color(0.55f, 0.95f, 1f);
        Fit(slowLabel, 3.0f, 0.6f);
        Transform slow = S41G_Button(root.transform, "Slow", new Vector2(0.25f, -4.35f), "HOLD: SLOW", new Color(0.7f, 0.85f, 1f), 1.3f, 55);
        Art(slow, "Icon", "s41g_hourglass", 58, false, new Vector2(-0.95f, 0f));

        S41G_Goal(root.transform, "CATCH THE FASTBALL WITH THE BUMPER");
        SetField(level, "cannon", cannon);
        SetField(level, "ball", ball);
        SetField(level, "bumper", bumper);
        SetField(level, "slowButton", slow);
        SetField(level, "bumperArea", bumper.GetComponent<Collider2D>());
        SetField(level, "slowArea", slow.GetComponent<Collider2D>());
        SetField(level, "tint", tint);
        SetField(level, "slowLabel", slowLabel);
        SetField(level, "missesText", S41G_Info(root.transform, "Misses", new Vector2(0f, -3.55f), 3.4f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.5f, 2.35f)));
        SetArray(level, "confetti", S41G_Confetti(root.transform));
        return SaveLevel(root, "SlowMotion");
    }

    // ---------- 160 The overworked robot ----------

    static GameObject BuildS41GOverworkedRobot()
    {
        var root = new GameObject("Level_TheOverworkedRobot");
        OverworkedRobotLevel level = root.AddComponent<OverworkedRobotLevel>();
        Art(root.transform, "Lab", "s41g_lab_bg", -20, false);
        Transform robot = Child(root.transform, "Robot", new Vector2(0f, 1.0f)).transform;
        SpriteRenderer robotArt = Art(robot, "Art", "s41g_robot", 5, false).GetComponent<SpriteRenderer>();
        Transform shL = Child(robot, "ShoulderL", new Vector2(-0.62f, 0.35f)).transform;
        Transform shR = Child(robot, "ShoulderR", new Vector2(0.62f, 0.35f)).transform;
        Color metal = new Color(0.55f, 0.6f, 0.68f);
        LineRenderer armL = Line(root.transform, "ArmL", 0.16f, metal, 4);
        LineRenderer armR = Line(root.transform, "ArmR", 0.16f, metal, 4);
        Transform handL = Child(root.transform, "HandL", new Vector2(-1.25f, -0.55f)).transform;
        Art(handL, "Art", "s41g_robot_hand", 8, false);
        Transform handR = Child(root.transform, "HandR", new Vector2(1.25f, -0.55f)).transform;
        Art(handR, "Art", "s41g_robot_hand", 8, false);
        foreach (float x in new[] { -1.25f, 1.25f }) Art(root.transform, "Pad", "s41g_pad", 2, false, new Vector2(x, -1.95f));
        var slots = new List<Transform>();
        foreach (float x in new[] { -1.65f, -0.85f, 0.85f, 1.65f }) slots.Add(Child(root.transform, "Slot", new Vector2(x, -1.95f)).transform);
        var switches = new List<Transform>();
        var arts = new List<SpriteRenderer>();
        Vector2[] at = { new Vector2(-2.1f, -2.95f), new Vector2(-0.7f, -3.45f), new Vector2(0.7f, -2.95f), new Vector2(2.1f, -3.45f) };
        foreach (Vector2 p in at)
        {
            Transform s = Child(root.transform, "Switch", p).transform;
            arts.Add(Art(s, "Art", "s41g_switch_off", 6, false).GetComponent<SpriteRenderer>());
            s.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(0.8f, 0.9f);
            switches.Add(s);
        }
        Transform press = S41G_Button(root.transform, "Press", new Vector2(0f, -4.35f), "PRESS!", new Color(1f, 0.55f, 0.5f), 1.25f);

        S41G_Goal(root.transform, "HELP THE ROBOT PRESS ALL 4 SWITCHES");
        SetField(level, "robotArt", robotArt);
        SetField(level, "happySprite", sprites["s41g_robot_happy"]);
        SetField(level, "onSprite", sprites["s41g_switch_on"]);
        SetField(level, "offSprite", sprites["s41g_switch_off"]);
        SetField(level, "robot", robot);
        SetField(level, "handL", handL);
        SetField(level, "handR", handR);
        SetField(level, "shoulderL", shL);
        SetField(level, "shoulderR", shR);
        SetField(level, "pressButton", press);
        SetField(level, "armL", armL);
        SetField(level, "armR", armR);
        SetArray(level, "switches", switches.ToArray());
        SetArray(level, "slots", slots.ToArray());
        SetArray(level, "switchArts", arts.ToArray());
        SetField(level, "pressArea", press.GetComponent<Collider2D>());
        SetField(level, "triesText", S41G_Info(root.transform, "Tries", new Vector2(0f, 2.7f), 3.8f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.55f, 1.15f)));
        SetArray(level, "confetti", S41G_Confetti(root.transform));
        return SaveLevel(root, "TheOverworkedRobot");
    }

    // ---------- 161 The missing letter ----------

    static GameObject BuildS41GMissingLetter()
    {
        var root = new GameObject("Level_TheMissingLetter");
        MissingLetterLevel level = root.AddComponent<MissingLetterLevel>();
        Art(root.transform, "Field", "s41g_gate_bg", -20, false);
        Art(root.transform, "Cloud", "s41g_cloud", 2, false, new Vector2(-1.3f, 2.3f));
        Art(root.transform, "Cloud2", "s41g_cloud", 2, false, new Vector2(1.6f, 2.55f)).localScale = Vector3.one * 0.6f;
        Transform gateL = Child(root.transform, "GateL", new Vector2(-1.15f, -3.25f)).transform;
        Art(gateL, "Art", "s41g_gate_half", 3, false);
        Transform gateR = Child(root.transform, "GateR", new Vector2(1.15f, -3.25f)).transform;
        gateR.localScale = new Vector3(-1f, 1f, 1f);
        Art(gateR, "Art", "s41g_gate_half", 3, false);
        Transform sign = Child(root.transform, "Sign", new Vector2(0f, 0.95f)).transform;
        Art(sign, "Board", "s41g_sign_board", 4, false);
        Fit(Label(sign, new Vector2(0f, 0.3f), 5, "OPEN THE", 0.075f), 3.0f, 0.5f);
        Fit(Label(sign, new Vector2(-0.9f, -0.28f), 5, "G", 0.09f), 0.6f, 0.6f);
        Fit(Label(sign, new Vector2(0.3f, -0.28f), 5, "T", 0.09f), 0.6f, 0.6f);
        Fit(Label(sign, new Vector2(0.9f, -0.28f), 5, "E", 0.09f), 0.6f, 0.6f);
        SpriteRenderer slotMark = Bar(sign, "Slot", new Vector2(-0.3f, -0.28f), new Vector2(0.5f, 0.6f), new Color(1f, 1f, 1f, 0.6f), 5);
        Transform slot = Child(root.transform, "SlotPoint", new Vector2(-0.3f, 0.67f)).transform;

        // lettere nascoste come decorazioni: A (giusta) nella nuvola, O nel tronco, E nella staccionata, U sul pilastro
        string[] chars = { "A", "O", "E", "U" };
        Vector2[] pos = { new Vector2(-1.25f, 2.3f), new Vector2(2.45f, -0.9f), new Vector2(-2.3f, -2.6f), new Vector2(1.35f, -1.35f) };
        float[] rot = { 10f, 0f, -90f, 180f };
        float[] scl = { 0.9f, 0.75f, 1f, 0.8f };
        Color[] col = { new Color(0.8f, 0.87f, 0.97f), new Color(0.45f, 0.28f, 0.15f), new Color(0.96f, 0.94f, 0.88f), new Color(0.55f, 0.52f, 0.6f) };
        var letters = new List<Transform>();
        var texts = new List<TextMesh>();
        for (int i = 0; i < chars.Length; i++)
        {
            Transform l = Child(root.transform, "Letter" + chars[i], pos[i]).transform;
            l.localRotation = Quaternion.Euler(0, 0, rot[i]);
            l.localScale = Vector3.one * scl[i];
            TextMesh t = Fit(Label(l, Vector2.zero, 6, chars[i], 0.09f), 0.6f, 0.6f);
            t.color = col[i];
            l.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(0.8f, 0.85f);
            letters.Add(l);
            texts.Add(t);
        }
        Transform mouse = Child(root.transform, "Mouse", new Vector2(-1.9f, -3.3f)).transform;
        mouse.localScale = Vector3.one * 0.8f;
        Art(mouse, "Art", "mouse", 7, false);

        S41G_Goal(root.transform, "OPEN THE GATE");
        SetArray(level, "letters", letters.ToArray());
        SetArray(level, "letterTexts", texts.ToArray());
        SetField(level, "slot", slot);
        SetField(level, "gateL", gateL);
        SetField(level, "gateR", gateR);
        SetField(level, "mouse", mouse);
        SetField(level, "sign", sign);
        SetField(level, "slotMark", slotMark);
        SetField(level, "triesText", S41G_Info(root.transform, "Tries", new Vector2(0f, -4.6f), 3.8f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.1f, -1.85f)));
        SetArray(level, "confetti", S41G_Confetti(root.transform));
        return SaveLevel(root, "TheMissingLetter");
    }

    // ---------- 162 Anti-gravity taxi ----------

    static GameObject BuildS41GAntiGravityTaxi()
    {
        var root = new GameObject("Level_AntiGravityTaxi");
        AntiGravityTaxiLevel level = root.AddComponent<AntiGravityTaxiLevel>();
        Art(root.transform, "City", "s41g_city_bg", -20, false);
        Transform platform = Child(root.transform, "Platform", new Vector2(0.3f, 1.6f)).transform;
        Art(platform, "Art", "s41g_platform", 4, false);
        Transform client = Child(root.transform, "Client", new Vector2(0.65f, 1.75f)).transform;
        Art(client, "Art", "s41g_client", 5, false);
        Transform taxi = Child(root.transform, "Taxi", new Vector2(0.3f, -3.6f)).transform;
        Art(taxi, "Art", "s41g_taxi", 6, false);
        Transform badge = Child(root.transform, "Badge", new Vector2(-1.45f, -0.3f)).transform;
        Art(badge, "Art", "s41g_grav_badge", 7, false);
        badge.gameObject.AddComponent<CircleCollider2D>().radius = 0.85f;
        Transform arrow = Child(root.transform, "Arrow", new Vector2(-1.45f, -0.3f)).transform;
        Art(arrow, "Art", "s41g_grav_arrow", 8, false);
        Bar(root.transform, "GravBack", new Vector2(-1.45f, -1.25f), new Vector2(1.3f, 0.32f), new Color(0.12f, 0.09f, 0.16f, 0.6f), 7);
        TextMesh gl = Fit(Label(root.transform, new Vector2(-1.45f, -1.25f), 8, "GRAVITY", 0.04f), 1.2f, 0.26f);
        gl.color = Color.white;
        Transform jump = S41G_Button(root.transform, "Jump", new Vector2(-1.3f, -4.45f), "JUMP", new Color(1f, 0.85f, 0.4f), 1.1f);

        S41G_Goal(root.transform, "PICK UP THE CLIENT");
        SetField(level, "taxi", taxi);
        SetField(level, "arrow", arrow);
        SetField(level, "badge", badge);
        SetField(level, "platform", platform);
        SetField(level, "client", client);
        SetField(level, "jumpButton", jump);
        SetField(level, "arrowArea", badge.GetComponent<Collider2D>());
        SetField(level, "jumpArea", jump.GetComponent<Collider2D>());
        SetField(level, "triesText", S41G_Info(root.transform, "Tries", new Vector2(1.2f, -4.45f), 2.4f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.6f, 2.45f)));
        SetArray(level, "confetti", S41G_Confetti(root.transform));
        return SaveLevel(root, "AntiGravityTaxi");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V41G = new Dictionary<string, string[]>
    {
        { "unfair_race", new[] { "You can change the race.", "Move the finish line next to the snail." } },
        { "the_bottomless_cup", new[] { "Cover the hole.", "Hold the hole with one finger while filling with the other." } },
        { "the_dirty_secret", new[] { "Follow the dirt with your finger.", "Rub away enough dirt to reveal the exit." } },
        { "the_fake_virus", new[] { "Watch their behavior.", "Catch the one that sneezes." } },
        { "slow_motion", new[] { "Try holding two fingers down.", "Slow time with two fingers, then move the barrier." } },
        { "the_overworked_robot", new[] { "Move the switches closer together.", "Group the switches so each hand presses two." } },
        { "the_missing_letter", new[] { "Look for the missing letter.", "Take the A from the cloud and place it into G_TE." } },
        { "anti_gravity_taxi", new[] { "Gravity doesn't have to point down.", "Rotate the gravity arrow upward." } },
    };
}
#endif
