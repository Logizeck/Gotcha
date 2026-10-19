#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli della specifica 105-200 (in gioco dal 151), pacchetto D.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels41D() => new List<LevelDefinition>
    {
        Define("TheFourthWall", "the_fourth_wall", "THE FOURTH WALL", "The walls aren't permanent.", BuildS41DFourthWall()),
        Define("HotPotato", "hot_potato", "HOT POTATO", "Watch the timing.", BuildS41DHotPotato()),
        Define("ShakeToUndo", "shake_to_undo", "SHAKE TO UNDO", "The undo button isn't the only option.", BuildS41DShakeToUndo()),
        Define("TheLonelyCloud", "the_lonely_cloud", "THE LONELY CLOUD", "The cloud is too small.", BuildS41DLonelyCloud()),
        Define("GhostSelfie", "ghost_selfie", "GHOST SELFIE", "He hates sudden photos.", BuildS41DGhostSelfie()),
        Define("MagnetMayhem", "magnet_mayhem", "MAGNET MAYHEM", "Magnets move metal.", BuildS41DMagnetMayhem()),
        Define("TheFakeCrash", "the_fake_crash", "THE FAKE CRASH", "It's not a real system error.", BuildS41DFakeCrash()),
        Define("RainbowThief", "rainbow_thief", "RAINBOW THIEF", "Some colors can be mixed.", BuildS41DRainbowThief()),
    };

    // ---------- Aiuti comuni del pacchetto D ----------

    /// <summary>Riga dell'obiettivo in alto, su una fascia scura.</summary>
    static TextMesh S41D_Goal(Transform root, string text)
    {
        Transform h = Child(root, "Goal", new Vector2(0f, 3.25f)).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(5.5f, 0.5f), new Color(0.12f, 0.09f, 0.16f, 0.72f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, text, 0.05f);
        t.color = Color.white;
        Fit(t, 5.3f, 0.42f);
        return t;
    }

    /// <summary>Etichetta bianca su fascia scura (contatori).</summary>
    static TextMesh S41D_Info(Transform root, string name, Vector2 pos, float width)
    {
        Transform h = Child(root, name, pos).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(width, 0.4f), new Color(0.12f, 0.09f, 0.16f, 0.6f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, "", 0.04f);
        t.color = Color.white;
        Fit(t, width - 0.2f, 0.32f);
        return t;
    }

    static SmokeEmitter[] S41D_Confetti(Transform root)
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

    // ---------- 130 The fourth wall ----------

    static GameObject BuildS41DFourthWall()
    {
        var root = new GameObject("Level_TheFourthWall");
        TheFourthWallLevel level = root.AddComponent<TheFourthWallLevel>();
        Art(root.transform, "Stage", "s41d_stage_bg", -20, false);
        Transform frame = Child(root.transform, "Frame", new Vector2(0f, 0.6f)).transform;
        Art(frame, "Canvas", "s41d_frame_canvas", 1, false);
        Art(frame, "Top", "s41d_frame_h", 4, false, new Vector2(0f, 1.5f));
        Art(frame, "Bottom", "s41d_frame_h", 4, false, new Vector2(0f, -1.5f));
        Transform left = Child(frame, "WallLeft", new Vector2(-1.7f, 0f)).transform;
        Art(left, "Art", "s41d_frame_v", 4, false);
        Transform right = Child(frame, "WallRight", new Vector2(1.7f, 0f)).transform;
        Art(right, "Art", "s41d_frame_v", 4, false);
        frame.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(3.8f, 3.4f);
        Transform actor = Child(frame, "Actor", new Vector2(-0.3f, -1.2f)).transform;
        Art(actor, "Art", "s41d_actor", 6, false);
        BoxCollider2D ac = actor.gameObject.AddComponent<BoxCollider2D>();
        ac.size = new Vector2(1.0f, 1.3f);
        ac.offset = new Vector2(0f, 0.55f);

        S41D_Goal(root.transform, "HELP THE ACTOR OUT OF THE PAINTING");
        SetField(level, "frame", frame);
        SetField(level, "actor", actor);
        SetField(level, "actorArea", ac);
        SetField(level, "frameArea", frame.GetComponent<Collider2D>());
        SetField(level, "wallLeft", left);
        SetField(level, "wallRight", right);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.9f, 2.45f)));
        SetArray(level, "confetti", S41D_Confetti(root.transform));
        return SaveLevel(root, "TheFourthWall");
    }

    // ---------- 131 Hot potato ----------

    static GameObject BuildS41DHotPotato()
    {
        var root = new GameObject("Level_HotPotato");
        HotPotatoLevel level = root.AddComponent<HotPotatoLevel>();
        Art(root.transform, "Park", "park_bg", -20, false);
        Vector2[] at = { new Vector2(-1.5f, 0.3f), new Vector2(1.5f, 0.3f), new Vector2(1.5f, -3.0f), new Vector2(-1.5f, -3.0f) };
        Color[] tints = { new Color(1f, 0.7f, 0.8f), new Color(1f, 0.9f, 0.5f), new Color(0.6f, 0.9f, 1f), new Color(0.7f, 1f, 0.6f) };
        var buddies = new List<Transform>();
        var signals = new List<SpriteRenderer>();
        for (int i = 0; i < 4; i++)
        {
            Transform b = Child(root.transform, "Buddy" + i, at[i]).transform;
            Art(b, "Art", "s41d_buddy", 2, false).GetComponent<SpriteRenderer>().color = tints[i];
            signals.Add(Art(b, "Signal", "s41d_signal", 3, false, new Vector2(0f, 1.75f)).GetComponent<SpriteRenderer>());
            BoxCollider2D c = b.gameObject.AddComponent<BoxCollider2D>();
            c.size = new Vector2(1.6f, 2.0f);
            c.offset = new Vector2(0f, 0.8f);
            buddies.Add(b);
        }
        Transform potato = Child(root.transform, "Potato", Vector2.zero).transform;
        Art(potato, "Art", "s41d_potato", 8, false);
        Transform mash = Child(root.transform, "Mash", Vector2.zero).transform;
        mash.localScale = Vector3.one * 1.3f;
        Art(mash, "Art", "s41d_mash", 9, false);

        S41D_Goal(root.transform, "CATCH THE HOT POTATO 5 TIMES!");
        SetArray(level, "buddies", buddies.ToArray());
        SetArray(level, "signals", signals.ToArray());
        SetField(level, "potato", potato);
        SetField(level, "mash", mash);
        SetField(level, "counter", S41D_Info(root.transform, "Counter", new Vector2(0f, 2.7f), 4.6f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0f, -1.0f)));
        SetArray(level, "confetti", S41D_Confetti(root.transform));
        return SaveLevel(root, "HotPotato");
    }

    // ---------- 132 Shake to undo ----------

    static GameObject BuildS41DShakeToUndo()
    {
        var root = new GameObject("Level_ShakeToUndo");
        ShakeToUndoLevel level = root.AddComponent<ShakeToUndoLevel>();
        Art(root.transform, "Studio", "mixer_studio_bg", -20, false);
        Vector2 easel = new Vector2(0.6f, -0.4f);           // il centro della tela è easel + (0, 0.4)
        Art(root.transform, "Masterpiece", "s41d_masterpiece", 2, false, easel);
        SpriteRenderer blank = Bar(root.transform, "Blank", easel + new Vector2(0f, 0.4f), new Vector2(2.45f, 1.85f), new Color(0.98f, 0.97f, 0.94f), 3);
        var strokes = new List<SpriteRenderer>();
        int[] orders = { 6, 4, 5 };   // contorno sopra, riempimento sotto, cuoricini in mezzo
        for (int i = 0; i < 3; i++) strokes.Add(Art(root.transform, "Stroke" + i, "s41d_pants_" + i, orders[i], false, easel).GetComponent<SpriteRenderer>());
        Transform painter = Child(root.transform, "Painter", new Vector2(-1.75f, -2.75f)).transform;
        Art(painter, "Art", "s41d_painter", 7, false);

        Transform undo = Child(root.transform, "Undo", new Vector2(1.7f, -3.3f)).transform;
        undo.localScale = Vector3.one * 1.2f;
        Art(undo, "Art", "s41d_undo_btn", 8, false);
        Fit(Label(undo, new Vector2(0f, -0.5f), 9, "UNDO", 0.05f), 1f, 0.3f);
        BoxCollider2D undoCol = undo.gameObject.AddComponent<BoxCollider2D>();
        undoCol.size = new Vector2(1.2f, 1.0f);
        undoCol.offset = new Vector2(0f, -0.1f);

        Transform shake = Child(root.transform, "ShakeButton", new Vector2(0f, -4.25f)).transform;
        Art(shake, "Art", "s41d_crash_btn", 10, false).GetComponent<SpriteRenderer>().color = new Color(1f, 0.75f, 0.4f);
        Fit(Label(shake, new Vector2(0f, 0.02f), 11, "SHAKE", 0.06f), 1.3f, 0.4f);
        BoxCollider2D shakeCol = shake.gameObject.AddComponent<BoxCollider2D>();
        shakeCol.size = new Vector2(1.7f, 0.8f);

        S41D_Goal(root.transform, "ERASE THE UNDERPANTS!");
        SetArray(level, "strokes", strokes.ToArray());
        SetField(level, "blank", blank);
        SetField(level, "painter", painter);
        SetField(level, "undoButton", undo);
        SetField(level, "undoArea", undoCol);
        SetField(level, "shakeButtonArea", shakeCol);
        SetField(level, "shakeButton", shake.gameObject);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.2f, 2.3f)));
        SetArray(level, "confetti", S41D_Confetti(root.transform));
        return SaveLevel(root, "ShakeToUndo");
    }

    // ---------- 133 The lonely cloud ----------

    static GameObject BuildS41DLonelyCloud()
    {
        var root = new GameObject("Level_TheLonelyCloud");
        TheLonelyCloudLevel level = root.AddComponent<TheLonelyCloudLevel>();
        Art(root.transform, "Meadow", "s41d_meadow_bg", -20, false);
        Transform cloud = Child(root.transform, "Cloud", new Vector2(-2.5f, 1.7f)).transform;
        var puffs = new List<Transform>();
        for (int i = 0; i < 12; i++)
        {
            Transform p = Child(cloud, "Puff" + i, Vector2.zero).transform;
            Art(p, "Ink", "s41d_puff_ink", 1, false);
            Art(p, "Fill", "s41d_puff", 2, false);
            puffs.Add(p);
        }
        Transform face = Child(cloud, "Face", Vector2.zero).transform;
        SpriteRenderer faceArt = Art(face, "Art", "s41d_cloud_sad", 3, false).GetComponent<SpriteRenderer>();
        BoxCollider2D area = cloud.gameObject.AddComponent<BoxCollider2D>();
        Transform flower = Child(root.transform, "Flower", new Vector2(1.6f, -3.4f)).transform;
        SpriteRenderer flowerArt = Art(flower, "Art", "s41d_flower_sad", 4, false).GetComponent<SpriteRenderer>();
        Transform timer = GaugeBar(root.transform, "Timer", new Vector2(0f, 2.75f), new Vector2(4.2f, 0.16f), new Color(0.15f, 0.12f, 0.2f, 0.7f), new Color(0.4f, 0.8f, 1f), 30);

        S41D_Goal(root.transform, "WATER THE THIRSTY FLOWER");
        SetField(level, "cloud", cloud);
        SetArray(level, "puffs", puffs.ToArray());
        SetField(level, "face", face);
        SetField(level, "faceArt", faceArt);
        SetField(level, "flowerArt", flowerArt);
        SetField(level, "happyFace", sprites["s41d_cloud_happy"]);
        SetField(level, "happyFlower", sprites["s41d_flower_happy"]);
        SetField(level, "dropSprite", sprites["s41d_raindrop"]);
        SetField(level, "flower", flower);
        SetField(level, "timerFill", timer);
        SetField(level, "cloudArea", area);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.9f, 0.55f)));
        SetArray(level, "confetti", S41D_Confetti(root.transform));
        return SaveLevel(root, "TheLonelyCloud");
    }

    // ---------- 134 Ghost selfie ----------

    static GameObject BuildS41DGhostSelfie()
    {
        var root = new GameObject("Level_GhostSelfie");
        GhostSelfieLevel level = root.AddComponent<GhostSelfieLevel>();
        Art(root.transform, "Hall", "haunt_hall_bg", -20, false);
        Transform center = Child(root.transform, "FrameCenter", new Vector2(0f, 0.4f)).transform;
        Art(root.transform, "Viewfinder", "s41d_viewfinder", 5, false, new Vector2(0f, 0.4f));
        Transform ghost = Child(root.transform, "Ghost", new Vector2(1.4f, 2.3f)).transform;
        SpriteRenderer ghostArt = Art(ghost, "Art", "s41d_ghost", 6, false).GetComponent<SpriteRenderer>();
        Transform shutter = Child(root.transform, "Shutter", new Vector2(0f, -3.2f)).transform;
        Art(shutter, "Art", "s41d_shutter", 8, false);
        shutter.gameObject.AddComponent<CircleCollider2D>().radius = 0.75f;
        TextMesh countdown = Label(root.transform, new Vector2(0f, 0.4f), 20, "", 0.22f);
        countdown.color = new Color(1f, 0.95f, 0.6f);
        Fit(countdown, 3f, 1.4f);
        SpriteRenderer flash = Bar(root.transform, "Flash", Vector2.zero, new Vector2(12f, 16f), Color.white, 50);
        Transform polaroid = Child(root.transform, "Polaroid", new Vector2(0f, -0.9f)).transform;
        polaroid.localRotation = Quaternion.Euler(0, 0, -6f);
        Art(polaroid, "Art", "s41d_polaroid", 30, false);
        Transform mini = Child(polaroid, "Ghost", new Vector2(0f, 0.2f)).transform;
        mini.localScale = Vector3.one * 0.6f;
        Art(mini, "Art", "s41d_ghost_pose", 31, false);
        TextMesh caption = Fit(Label(polaroid, new Vector2(0f, -0.75f), 32, "", 0.045f), 1.6f, 0.3f);

        S41D_Goal(root.transform, "TAKE A SELFIE WITH THE GHOST");
        SetField(level, "ghost", ghost);
        SetField(level, "frameCenter", center);
        SetField(level, "shutter", shutter);
        SetField(level, "polaroid", polaroid);
        SetField(level, "polaroidGhost", mini);
        SetField(level, "ghostArt", ghostArt);
        SetField(level, "flash", flash);
        SetField(level, "idleSprite", sprites["s41d_ghost"]);
        SetField(level, "poseSprite", sprites["s41d_ghost_pose"]);
        SetField(level, "shutterArea", shutter.GetComponent<Collider2D>());
        SetField(level, "countdown", countdown);
        SetField(level, "photoCaption", caption);
        SetField(level, "triesText", S41D_Info(root.transform, "Tries", new Vector2(0f, 2.7f), 3.4f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.1f, 1.95f)));
        SetArray(level, "confetti", S41D_Confetti(root.transform));
        return SaveLevel(root, "GhostSelfie");
    }

    // ---------- 135 Magnet mayhem ----------

    static GameObject BuildS41DMagnetMayhem()
    {
        var root = new GameObject("Level_MagnetMayhem");
        MagnetMayhemLevel level = root.AddComponent<MagnetMayhemLevel>();
        Art(root.transform, "Board", "s41d_maze_bg", -20, false);
        // muri (devono coincidere col disegno s41d_maze_bg)
        var walls = new List<Collider2D>();
        Vector4[] boxes =
        {
            new Vector4(0f, 2.75f, 2.65f, 0.1f), new Vector4(0f, -4.25f, 2.65f, 0.1f), new Vector4(-2.55f, -0.75f, 0.1f, 3.6f), new Vector4(2.55f, -0.75f, 0.1f, 3.6f),
            new Vector4(-0.675f, -2.4f, 1.875f, 0.1f), new Vector4(0.675f, -0.6f, 1.875f, 0.1f), new Vector4(-0.675f, 1.2f, 1.875f, 0.1f),
        };
        foreach (Vector4 b in boxes)
        {
            GameObject w = Child(root.transform, "Wall", new Vector2(b.x, b.y));
            BoxCollider2D c = w.AddComponent<BoxCollider2D>();
            c.size = new Vector2(b.z * 2f, b.w * 2f);
            walls.Add(c);
        }
        var traps = new List<Collider2D>();
        var trapArts = new List<SpriteRenderer>();
        foreach (Vector2 p in new[] { new Vector2(0f, -2.68f), new Vector2(0.2f, -0.88f), new Vector2(-0.2f, -0.32f), new Vector2(0f, 2.47f) })
        {
            Transform t = Child(root.transform, "Trap", p).transform;
            trapArts.Add(Art(t, "Art", "s41d_zap", 3, false).GetComponent<SpriteRenderer>());
            BoxCollider2D c = t.gameObject.AddComponent<BoxCollider2D>();
            c.size = new Vector2(1.0f, 0.32f);
            traps.Add(c);
        }
        Transform goal = Child(root.transform, "Lock", new Vector2(-1.9f, 1.95f)).transform;
        SpriteRenderer lockArt = Art(goal, "Art", "s41d_lock", 3, false).GetComponent<SpriteRenderer>();
        Transform start = Child(root.transform, "KeyStart", new Vector2(-1.9f, -3.35f)).transform;
        Transform key = Child(root.transform, "Key", new Vector2(-1.9f, -3.35f)).transform;
        Art(key, "Art", "s41d_mkey", 4, false);

        var magnets = new List<Transform>();
        var fields = new List<SpriteRenderer>();
        Vector2[] mAt = { new Vector2(1.9f, 2.0f), new Vector2(2.0f, -3.4f), new Vector2(0.6f, 0.3f) };
        for (int i = 0; i < 3; i++)
        {
            Transform m = Child(root.transform, "Magnet" + i, mAt[i]).transform;
            SpriteRenderer f = Art(m, "Field", "glow", 2, false).GetComponent<SpriteRenderer>();
            f.transform.localScale = Vector3.one * 1.4f;
            fields.Add(f);
            Art(m, "Art", i < 2 ? "s41d_magnet_red" : "s41d_magnet_blue", 5, false);
            m.gameObject.AddComponent<CircleCollider2D>().radius = 0.5f;
            magnets.Add(m);
        }

        S41D_Goal(root.transform, "GUIDE THE KEY TO THE LOCK");
        SetField(level, "key", key);
        SetField(level, "keyStart", start);
        SetArray(level, "magnets", magnets.ToArray());
        SetArray(level, "fields", fields.ToArray());
        SetArray(level, "walls", walls.ToArray());
        SetArray(level, "traps", traps.ToArray());
        SetArray(level, "trapArts", trapArts.ToArray());
        SetField(level, "goal", goal);
        SetField(level, "lockArt", lockArt);
        SetField(level, "lockOpen", sprites["s41d_lock_open"]);
        SetField(level, "livesText", S41D_Info(root.transform, "Lives", new Vector2(0f, -4.66f), 2.8f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.6f, 0.35f)));
        SetArray(level, "confetti", S41D_Confetti(root.transform));
        return SaveLevel(root, "MagnetMayhem");
    }

    // ---------- 136 The fake crash ----------

    static GameObject BuildS41DFakeCrash()
    {
        var root = new GameObject("Level_TheFakeCrash");
        TheFakeCrashLevel level = root.AddComponent<TheFakeCrashLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);
        Transform cat = Child(root.transform, "FrozenCat", new Vector2(-1.1f, -2.9f)).transform;
        cat.localScale = Vector3.one * 0.6f;
        cat.localRotation = Quaternion.Euler(0, 0, 18f);
        Art(cat, "Art", "cat", 1, false);
        SpriteRenderer dim = Bar(root.transform, "Dim", Vector2.zero, new Vector2(12f, 16f), new Color(0.1f, 0.05f, 0.2f, 0.55f), 10);
        SpriteRenderer glitch = Bar(root.transform, "Glitch", Vector2.zero, new Vector2(12f, 16f), new Color(0.55f, 1f, 0.6f, 0f), 60);

        Transform window = Child(root.transform, "Window", new Vector2(0f, 0.6f)).transform;
        Art(window, "Art", "s41d_crash_window", 20, false);
        TextMesh title = Fit(Label(window, new Vector2(-0.9f, 1.42f), 21, "oopsie.exe", 0.05f), 2.2f, 0.4f);
        title.color = Color.white;
        Art(window, "Face", "s41d_crash_face", 21, false, new Vector2(-1.65f, 0.15f));
        Fit(Label(window, new Vector2(0.5f, 0.55f), 22, "GOTCHA HAS STOPPED", 0.07f), 3.1f, 0.5f);
        Transform working = Child(window, "Working", new Vector2(0.5f, -0.05f)).transform;
        TextMesh wt = Fit(Label(working, Vector2.zero, 23, "WORKING", 0.1f), 2.2f, 0.6f);
        wt.color = new Color(0.85f, 0.15f, 0.3f);
        BoxCollider2D wcol = working.gameObject.AddComponent<BoxCollider2D>();
        wcol.size = new Vector2(2.2f, 0.75f);
        TextMesh fix = Fit(Label(window, new Vector2(0.5f, -0.05f), 22, "", 0.1f), 2.6f, 0.6f);
        fix.color = new Color(0.2f, 0.6f, 0.3f);
        TextMesh status = Fit(Label(window, new Vector2(0f, -0.62f), 22, "", 0.045f), 4.2f, 0.3f);

        Transform close = Child(window, "Close", new Vector2(-1.0f, -1.2f)).transform;
        Art(close, "Art", "s41d_crash_btn", 21, false).GetComponent<SpriteRenderer>().color = new Color(1f, 0.6f, 0.65f);
        Fit(Label(close, new Vector2(0f, 0.02f), 22, "CLOSE", 0.06f), 1.2f, 0.4f);
        BoxCollider2D ccol = close.gameObject.AddComponent<BoxCollider2D>();
        ccol.size = new Vector2(1.6f, 0.7f);
        Transform wait = Child(window, "Wait", new Vector2(1.0f, -1.2f)).transform;
        Art(wait, "Art", "s41d_crash_btn", 21, false).GetComponent<SpriteRenderer>().color = new Color(0.6f, 0.88f, 1f);
        Fit(Label(wait, new Vector2(0f, 0.02f), 22, "WAIT", 0.06f), 1.2f, 0.4f);
        BoxCollider2D wacol = wait.gameObject.AddComponent<BoxCollider2D>();
        wacol.size = new Vector2(1.6f, 0.7f);

        SetField(level, "window", window);
        SetField(level, "working", working);
        SetField(level, "closeButton", close);
        SetField(level, "waitButton", wait);
        SetField(level, "frozenCat", cat);
        SetField(level, "workingArea", wcol);
        SetField(level, "closeArea", ccol);
        SetField(level, "waitArea", wacol);
        SetField(level, "errorWord", fix);
        SetField(level, "statusText", status);
        SetField(level, "dim", dim);
        SetField(level, "glitch", glitch);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.4f, -1.75f)));
        SetArray(level, "confetti", S41D_Confetti(root.transform));
        return SaveLevel(root, "TheFakeCrash");
    }

    // ---------- 137 Rainbow thief ----------

    static GameObject BuildS41DRainbowThief()
    {
        var root = new GameObject("Level_RainbowThief");
        RainbowThiefLevel level = root.AddComponent<RainbowThiefLevel>();
        Art(root.transform, "Sky", "s41d_rainbow_bg", -20, false);
        var bands = new List<SpriteRenderer>();
        for (int k = 0; k < 7; k++) bands.Add(Art(root.transform, "Band" + k, "s41d_band_" + k, 1, false, new Vector2(0f, 0.6f)).GetComponent<SpriteRenderer>());
        foreach (float x in new[] { -2.15f, 2.15f })
        {
            Transform c = Art(root.transform, "Cloud", "cloud", 3, false, new Vector2(x, 0.65f));
            c.localScale = Vector3.one * 1.1f;
        }
        Transform thief = Child(root.transform, "Thief", new Vector2(1.6f, -1.0f)).transform;
        thief.localScale = Vector3.one * 0.9f;
        Art(thief, "Art", "mu_burglar", 4, false);
        Art(root.transform, "Example", "s41d_venn", 4, false, new Vector2(-1.75f, -0.3f));
        Fit(Label(root.transform, new Vector2(-1.75f, -1.0f), 5, "RED + GREEN LIGHT", 0.04f), 1.9f, 0.25f);

        var crates = new List<Transform>();
        var glows = new List<SpriteRenderer>();
        string[] tags = { "YELLOW", "CYAN", "MAGENTA" };
        Color[] tagCols = { new Color(1f, 0.92f, 0.2f), new Color(0.3f, 0.92f, 1f), new Color(1f, 0.35f, 0.92f) };
        for (int i = 0; i < 3; i++)
        {
            Transform c = Child(root.transform, "Crate" + i, new Vector2(-1.6f + i * 1.6f, -2.0f)).transform;
            SpriteRenderer g = Art(c, "Glow", "glow", 5, false).GetComponent<SpriteRenderer>();
            g.transform.localScale = Vector3.one * 0.9f;
            glows.Add(g);
            Art(c, "Art", "s41d_crate", 6, false);
            c.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.2f, 1.2f);
            Bar(root.transform, "TagBack", new Vector2(-1.6f + i * 1.6f, -2.85f), new Vector2(1.4f, 0.36f), new Color(0.12f, 0.09f, 0.16f, 0.75f), 6);
            TextMesh t = Fit(Label(root.transform, new Vector2(-1.6f + i * 1.6f, -2.85f), 7, tags[i], 0.045f), 1.3f, 0.3f);
            t.color = tagCols[i];
            crates.Add(c);
        }
        Bar(root.transform, "Tray", new Vector2(0f, -3.7f), new Vector2(5.2f, 0.8f), new Color(0.12f, 0.09f, 0.16f, 0.45f), 7);
        Color[] orbCols = { new Color(1f, 0.25f, 0.25f), new Color(0.3f, 0.92f, 0.35f), new Color(0.3f, 0.45f, 1f) };
        var orbs = new List<Transform>();
        for (int i = 0; i < 6; i++)
        {
            Transform o = Child(root.transform, "Orb" + i, new Vector2(-2.0f + i * 0.8f, -3.7f)).transform;
            Art(o, "Art", "s41d_orb", 8, false).GetComponent<SpriteRenderer>().color = orbCols[i / 2];
            o.gameObject.AddComponent<CircleCollider2D>().radius = 0.4f;
            orbs.Add(o);
        }
        Fit(Label(root.transform, new Vector2(-1.1f, -4.55f), 8, "TIME", 0.045f), 1f, 0.3f);
        var segs = new List<SpriteRenderer>();
        for (int i = 0; i < 4; i++) segs.Add(Bar(root.transform, "Time" + i, new Vector2(-0.4f + i * 0.42f, -4.55f), new Vector2(0.34f, 0.24f), new Color(1f, 0.8f, 0.3f), 8));

        S41D_Goal(root.transform, "GIVE THE RAINBOW ITS COLORS BACK");
        SetArray(level, "bands", bands.ToArray());
        SetArray(level, "orbs", orbs.ToArray());
        SetArray(level, "crates", crates.ToArray());
        SetArray(level, "crateGlows", glows.ToArray());
        SetArray(level, "timeSegments", segs.ToArray());
        SetField(level, "thief", thief);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.6f, -0.2f)));
        SetArray(level, "confetti", S41D_Confetti(root.transform));
        return SaveLevel(root, "RainbowThief");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V41D = new Dictionary<string, string[]>
    {
        { "the_fourth_wall", new[] { "Move the frame, not the actor.", "Drag the frame partly offscreen." } },
        { "hot_potato", new[] { "Tap just before the potato arrives.", "Use the green flash as your signal." } },
        { "shake_to_undo", new[] { "Try shaking something.", "Shake your phone three times, then stop." } },
        { "the_lonely_cloud", new[] { "Stretch it.", "Spread the cloud with two fingers until the rain reaches the flower." } },
        { "ghost_selfie", new[] { "Hold the camera button.", "Hold for three seconds and release when he's in frame." } },
        { "magnet_mayhem", new[] { "Opposite poles attract.", "Position the magnets to guide the key without touching it." } },
        { "the_fake_crash", new[] { "The message is interactive.", "Drag the word WORKING away." } },
        { "rainbow_thief", new[] { "Start with the three basic light colors.", "Combine the primary lights to restore the missing colors." } },
    };
}
#endif
