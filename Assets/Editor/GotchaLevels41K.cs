#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli della specifica 105-200 (in gioco dal 151), pacchetto K.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels41K() => new List<LevelDefinition>
    {
        Define("TheTwoFingerPianist", "the_two_finger_pianist", "THE TWO-FINGER PIANIST", "These notes must sound together.", BuildS41KTwoFingerPianist()),
        Define("TheMoonThief", "the_moon_thief", "THE MOON THIEF", "Hide what he wants.", BuildS41KMoonThief()),
        Define("BackToTheFuture", "back_to_the_future", "BACK TO THE FUTURE", "The door hasn't been built yet.", BuildS41KBackToTheFuture()),
        Define("TheThreeIdiots", "the_three_idiots", "THE THREE IDIOTS", "They aren't working together.", BuildS41KThreeIdiots()),
        Define("TheShyStar", "the_shy_star", "THE SHY STAR", "She's shy, but also vain.", BuildS41KShyStar()),
        Define("TheBrokenInternet", "the_broken_internet", "THE BROKEN INTERNET", "The connection is physically broken.", BuildS41KBrokenInternet()),
        Define("TheLaughingLock", "the_laughing_lock", "THE LAUGHING LOCK", "The lock won't stop laughing.", BuildS41KLaughingLock()),
        Define("TheFakeBoss", "the_fake_boss", "THE FAKE BOSS", "The boss isn't what he seems.", BuildS41KFakeBoss()),
    };

    // ---------- Aiuti comuni del pacchetto K ----------

    /// <summary>Riga dell'obiettivo in alto, su una fascia scura.</summary>
    static TextMesh S41K_Goal(Transform root, string text)
    {
        Transform h = Child(root, "Goal", new Vector2(0f, 3.25f)).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(5.5f, 0.5f), new Color(0.12f, 0.09f, 0.16f, 0.72f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, text, 0.05f);
        t.color = Color.white;
        Fit(t, 5.3f, 0.42f);
        return t;
    }

    /// <summary>Etichetta bianca su fascia scura (contatori).</summary>
    static TextMesh S41K_Info(Transform root, string name, Vector2 pos, float width)
    {
        Transform h = Child(root, name, pos).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(width, 0.4f), new Color(0.12f, 0.09f, 0.16f, 0.6f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, "", 0.04f);
        t.color = Color.white;
        Fit(t, width - 0.2f, 0.32f);
        return t;
    }

    /// <summary>Pulsante a pillola colorato con scritta e collider.</summary>
    static Transform S41K_Button(Transform root, string name, Vector2 pos, string text, Color tint, float scale, int order = 20)
    {
        Transform b = Child(root, name, pos).transform;
        b.localScale = Vector3.one * scale;
        Art(b, "Art", "s41k_button", order, false).GetComponent<SpriteRenderer>().color = tint;
        Fit(Label(b, new Vector2(0f, 0.02f), order + 1, text, 0.05f), 1.35f, 0.4f);
        b.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.7f, 0.8f);
        return b;
    }

    static SmokeEmitter[] S41K_Confetti(Transform root)
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

    // ---------- 188 The two-finger pianist ----------

    static GameObject BuildS41KTwoFingerPianist()
    {
        var root = new GameObject("Level_TheTwoFingerPianist");
        TwoFingerPianistLevel level = root.AddComponent<TwoFingerPianistLevel>();
        Art(root.transform, "Stage", "s41k_concert_bg", -20, false);
        Art(root.transform, "Piano", "s41k_piano", 1, false);
        string[] names = { "C", "D", "E", "F", "G", "A", "B" };
        var keys = new List<Transform>();
        var arts = new List<SpriteRenderer>();
        var areas = new List<Collider2D>();
        for (int i = 0; i < 7; i++)
        {
            Transform k = Child(root.transform, "Key" + names[i], new Vector2(-2.16f + i * 0.72f, -2.5f)).transform;
            arts.Add(Art(k, "Art", "s41k_key", 3, false).GetComponent<SpriteRenderer>());
            Fit(Label(k, new Vector2(0f, -0.95f), 4, names[i], 0.07f), 0.5f, 0.4f).color = new Color(0.45f, 0.4f, 0.55f);
            BoxCollider2D c = k.gameObject.AddComponent<BoxCollider2D>();
            c.size = new Vector2(0.7f, 2.56f);
            keys.Add(k);
            areas.Add(c);
        }
        Transform stand = Child(root.transform, "Stand", new Vector2(1.25f, 1.3f)).transform;
        stand.localScale = Vector3.one * 0.8f;
        Art(stand, "Art", "s41k_stand", 3, false);
        TextMesh chord = Fit(Label(stand, new Vector2(0.1f, 0f), 4, "C + E", 0.13f), 2.0f, 0.8f);
        chord.color = new Color(0.25f, 0.18f, 0.35f);
        var dots = new List<SpriteRenderer>();
        for (int i = 0; i < 3; i++) dots.Add(Art(root.transform, "Dot" + i, "s41k_dot", 5, false, new Vector2(-0.4f + i * 0.4f, -0.42f)).GetComponent<SpriteRenderer>());
        Transform pianist = Child(root.transform, "Pianist", new Vector2(-2.0f, 1.15f)).transform;
        Art(pianist, "Art", "mouse", 4, false);
        SmokeEmitter notes = Emitter(root.transform, "Notes", new Vector2(0f, 0.4f), "note", 5f, 1.6f, 1.2f, 2.6f, 0.6f, Color.white, 30);

        S41K_Goal(root.transform, "PLAY THE THREE CHORDS");
        SetArray(level, "keys", keys.ToArray());
        SetArray(level, "keyArts", arts.ToArray());
        SetArray(level, "keyAreas", areas.ToArray());
        SetArray(level, "progressDots", dots.ToArray());
        SetField(level, "pianist", pianist);
        SetField(level, "chordText", chord);
        SetField(level, "notes", notes);
        SetField(level, "triesText", S41K_Info(root.transform, "Tries", new Vector2(0f, 2.65f), 3.6f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.05f, 2.0f)));
        SetArray(level, "confetti", S41K_Confetti(root.transform));
        return SaveLevel(root, "TheTwoFingerPianist");
    }

    // ---------- 189 The moon thief ----------

    static GameObject BuildS41KMoonThief()
    {
        var root = new GameObject("Level_TheMoonThief");
        MoonThiefLevel level = root.AddComponent<MoonThiefLevel>();
        Art(root.transform, "Night", "s41k_night_bg", -20, false);
        Transform moon = Child(root.transform, "Moon", new Vector2(1.3f, 1.8f)).transform;
        SpriteRenderer moonArt = Art(moon, "Art", "s41k_moon", 3, false).GetComponent<SpriteRenderer>();
        var clouds = new List<Transform>();
        foreach (Vector2 at in new[] { new Vector2(-1.3f, 1.5f), new Vector2(0.9f, -0.2f) })
        {
            Transform c = Child(root.transform, "Cloud", at).transform;
            Art(c, "Art", "s41k_cloud", 6, false);
            clouds.Add(c);
        }
        Transform thief = Child(root.transform, "Thief", new Vector2(-1.4f, -2.3f)).transform;
        Transform net = Child(thief, "Net", new Vector2(0.42f, 0.85f)).transform;
        Art(net, "Art", "s41k_net", 4, false);
        SpriteRenderer thiefArt = Art(thief, "Art", "s41k_thief", 5, false).GetComponent<SpriteRenderer>();
        BoxCollider2D tc = thief.gameObject.AddComponent<BoxCollider2D>();
        tc.size = new Vector2(1.1f, 1.9f);
        tc.offset = new Vector2(0f, 0.9f);

        S41K_Goal(root.transform, "STOP THE MOON THIEF");
        SetField(level, "moon", moon);
        SetField(level, "thief", thief);
        SetField(level, "net", net);
        SetArray(level, "clouds", clouds.ToArray());
        SetField(level, "moonArt", moonArt);
        SetField(level, "thiefArt", thiefArt);
        SetField(level, "sadSprite", sprites["s41k_thief_sad"]);
        SetField(level, "thiefArea", tc);
        SetField(level, "triesText", S41K_Info(root.transform, "Tries", new Vector2(0f, 2.65f), 3.6f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.6f, -0.6f)));
        SetArray(level, "confetti", S41K_Confetti(root.transform));
        return SaveLevel(root, "TheMoonThief");
    }

    // ---------- 190 Back to the future ----------

    static GameObject BuildS41KBackToTheFuture()
    {
        var root = new GameObject("Level_BackToTheFuture");
        BackToTheFutureLevel level = root.AddComponent<BackToTheFutureLevel>();
        Art(root.transform, "Lab", "s41k_lab_bg", -20, false);
        Transform cal = Child(root.transform, "Calendar", new Vector2(-1.0f, 0.6f)).transform;
        Art(cal, "Art", "s41k_calendar", 3, false);
        BoxCollider2D calCol = cal.gameObject.AddComponent<BoxCollider2D>();
        calCol.size = new Vector2(2.5f, 2.7f);
        calCol.offset = new Vector2(0f, 0.05f);
        Transform holder = Child(cal, "PageHolder", new Vector2(0f, -0.15f)).transform;
        Transform page = Child(holder, "Page", Vector2.zero).transform;
        Art(page, "Art", "s41k_page", 5, false);
        TextMesh week = Fit(Label(page, new Vector2(0f, 0.72f), 6, "OCTOBER", 0.04f), 1.55f, 0.26f);
        week.color = Color.white;
        TextMesh day = Fit(Label(page, new Vector2(0f, -0.12f), 6, "20", 0.3f), 1.4f, 1.1f);
        day.color = new Color(0.25f, 0.18f, 0.35f);
        Transform fly = Child(holder, "FlyPage", Vector2.zero).transform;
        Art(fly, "Art", "s41k_page", 7, false);
        TextMesh flyDay = Fit(Label(fly, new Vector2(0f, -0.12f), 8, "20", 0.3f), 1.4f, 1.1f);
        flyDay.color = new Color(0.25f, 0.18f, 0.35f);
        TextMesh tm = Fit(Label(cal, new Vector2(0f, 1.2f), 4, "TIME-O-MATIC", 0.035f), 1.9f, 0.16f);
        tm.color = new Color(0.25f, 0.18f, 0.35f);
        SmokeEmitter smoke = Emitter(root.transform, "Smoke", new Vector2(-1.0f, 1.6f), "puff", 10f, 1.4f, 1.2f, 1.2f, 0.8f, new Color(0.45f, 0.45f, 0.5f, 0.9f), 20);

        Transform door = Child(root.transform, "Door", new Vector2(1.55f, -3.4f)).transform;
        SpriteRenderer doorArt = Art(door, "Art", "s41k_door", 3, false).GetComponent<SpriteRenderer>();
        SpriteRenderer ghost = Art(door, "Ghost", "s41k_door_ghost", 4, false).GetComponent<SpriteRenderer>();
        Transform sign = Child(root.transform, "Sign", new Vector2(1.55f, 1.05f)).transform;
        Art(sign, "Art", "s41k_sign", 3, false);
        Fit(Label(sign, new Vector2(0f, 0.17f), 4, "DOOR BUILT ON", 0.04f), 1.7f, 0.26f).color = new Color(0.25f, 0.18f, 0.35f);
        Fit(Label(sign, new Vector2(0f, -0.17f), 4, "OCTOBER 29", 0.05f), 1.7f, 0.32f).color = new Color(0.85f, 0.2f, 0.2f);
        Transform scientist = Child(root.transform, "Scientist", new Vector2(-1.7f, -3.4f)).transform;
        Art(scientist, "Art", "s41k_scientist", 5, false);

        S41K_Goal(root.transform, "OPEN THE DOOR FROM THE FUTURE");
        SetField(level, "calendar", cal);
        SetField(level, "page", page);
        SetField(level, "flyPage", fly);
        SetField(level, "door", door);
        SetField(level, "scientist", scientist);
        SetField(level, "dayText", day);
        SetField(level, "weekText", week);
        SetField(level, "flyDayText", flyDay);
        SetField(level, "doorArt", doorArt);
        SetField(level, "doorGhost", ghost);
        SetField(level, "calendarArea", calCol);
        SetField(level, "smoke", smoke);
        SetField(level, "triesText", S41K_Info(root.transform, "Tries", new Vector2(0f, 2.65f), 3.6f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.6f, -1.3f)));
        SetArray(level, "confetti", S41K_Confetti(root.transform));
        return SaveLevel(root, "BackToTheFuture");
    }

    // ---------- 191 The three idiots ----------

    static GameObject BuildS41KThreeIdiots()
    {
        var root = new GameObject("Level_TheThreeIdiots");
        ThreeIdiotsLevel level = root.AddComponent<ThreeIdiotsLevel>();
        Art(root.transform, "Field", "s41k_field_bg", -20, false);
        Transform pit = Child(root.transform, "Pit", new Vector2(2.05f, -0.3f)).transform;
        Transform rock = Child(root.transform, "Rock", new Vector2(-0.25f, -0.3f)).transform;
        Art(rock, "Art", "s41k_rock", 4, false);
        var guys = new List<Transform>();
        Vector2[] at = { new Vector2(-1.5f, 0.75f), new Vector2(-1.8f, -0.3f), new Vector2(-1.5f, -1.35f) };
        for (int i = 0; i < 3; i++)
        {
            Transform g = Child(root.transform, "Guy" + i, at[i]).transform;
            Art(g, "Art", "s41k_idiot_" + i, 5, false);
            guys.Add(g);
        }
        Transform whistle = Child(root.transform, "Whistle", new Vector2(0f, -3.3f)).transform;
        Art(whistle, "Art", "s41k_whistle", 6, false);
        whistle.gameObject.AddComponent<CircleCollider2D>().radius = 0.7f;
        Fit(Label(root.transform, new Vector2(0f, -4.15f), 7, "BLOW THE WHISTLE", 0.045f), 3f, 0.3f).color = Color.white;
        SmokeEmitter dust = Emitter(root.transform, "Dust", new Vector2(-0.25f, -0.3f), "puff", 9f, 1.0f, 0.3f, 1.6f, 0.6f, new Color(0.85f, 0.75f, 0.6f, 0.85f), 8);

        S41K_Goal(root.transform, "PUSH THE ROCK INTO THE HOLE");
        SetArray(level, "guys", guys.ToArray());
        SetField(level, "rock", rock);
        SetField(level, "pit", pit);
        SetField(level, "whistle", whistle);
        SetField(level, "whistleArea", whistle.GetComponent<Collider2D>());
        SetField(level, "dust", dust);
        SetField(level, "triesText", S41K_Info(root.transform, "Tries", new Vector2(0f, 2.65f), 3.6f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.3f, 1.9f)));
        SetArray(level, "confetti", S41K_Confetti(root.transform));
        return SaveLevel(root, "TheThreeIdiots");
    }

    // ---------- 192 The shy star ----------

    static GameObject BuildS41KShyStar()
    {
        var root = new GameObject("Level_TheShyStar");
        ShyStarLevel level = root.AddComponent<ShyStarLevel>();
        Art(root.transform, "Sky", "s41k_sky_bg", -20, false);
        var clouds = new List<Transform>();
        foreach (Vector2 at in new[] { new Vector2(-1.4f, 1.5f), new Vector2(1.3f, 1.1f), new Vector2(-0.9f, -0.5f) })
        {
            Transform c = Child(root.transform, "Cloud", at).transform;
            Art(c, "Art", "s41k_cloud", 6, false);
            clouds.Add(c);
        }
        Transform star = Child(root.transform, "Star", new Vector2(-0.85f, 1.92f)).transform;
        Transform glow = Child(star, "Glow", Vector2.zero).transform;
        Art(glow, "Art", "s41k_glow", 3, false);
        SpriteRenderer starArt = Art(star, "Art", "s41k_star", 4, false).GetComponent<SpriteRenderer>();
        SmokeEmitter sparkles = Emitter(star, "Sparkles", Vector2.zero, "sparkle_star", 8f, 1.2f, 0.4f, 1.6f, 0.8f, new Color(1f, 0.95f, 0.6f), 9);
        Transform mirror = Child(root.transform, "Mirror", new Vector2(1.5f, -3.0f)).transform;
        Art(mirror, "Art", "s41k_mirror", 8, false);
        BoxCollider2D mc = mirror.gameObject.AddComponent<BoxCollider2D>();
        mc.size = new Vector2(1.4f, 2.1f);
        mc.offset = new Vector2(0f, -0.2f);
        Transform refl = Art(mirror, "Reflection", "s41k_star_happy", 9, false, new Vector2(0f, 0.05f));
        refl.localScale = Vector3.one * 0.6f;

        S41K_Goal(root.transform, "MAKE THE STAR SHINE");
        SetField(level, "star", star);
        SetField(level, "mirror", mirror);
        SetField(level, "glow", glow);
        SetArray(level, "clouds", clouds.ToArray());
        SetField(level, "starArt", starArt);
        SetField(level, "reflection", refl.GetComponent<SpriteRenderer>());
        SetField(level, "happySprite", sprites["s41k_star_happy"]);
        SetField(level, "mirrorArea", mc);
        SetField(level, "sparkles", sparkles);
        SetField(level, "triesText", S41K_Info(root.transform, "Tries", new Vector2(0f, -4.55f), 3.6f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.1f, -0.6f)));
        SetArray(level, "confetti", S41K_Confetti(root.transform));
        return SaveLevel(root, "TheShyStar");
    }

    // ---------- 193 The broken internet ----------

    static GameObject BuildS41KBrokenInternet()
    {
        var root = new GameObject("Level_TheBrokenInternet");
        BrokenInternetLevel level = root.AddComponent<BrokenInternetLevel>();
        Art(root.transform, "Desk", "s41k_desk_bg", -20, false);
        Color wire = new Color(0.43f, 0.45f, 0.5f);
        Bar(root.transform, "WireL", new Vector2(-2.35f, -0.45f), new Vector2(0.1f, 1.5f), wire, 2);
        Bar(root.transform, "WireR", new Vector2(2.35f, -0.4f), new Vector2(0.1f, 1.6f), wire, 2);
        Transform computer = Child(root.transform, "Computer", new Vector2(-1.4f, 1.45f)).transform;
        SpriteRenderer compArt = Art(computer, "Art", "s41k_computer", 3, false).GetComponent<SpriteRenderer>();
        TextMesh screen = Fit(Label(computer, new Vector2(0f, 0.05f), 4, "NO\nINTERNET", 0.06f), 1.55f, 1.0f);
        screen.color = Color.white;
        Art(root.transform, "Router", "s41k_router", 3, false, new Vector2(1.9f, 0.75f));
        Art(root.transform, "SocketL", "s41k_socket", 3, false, new Vector2(-2.25f, -1.2f));
        Art(root.transform, "SocketR", "s41k_socket", 3, false, new Vector2(2.25f, -1.2f));
        Art(root.transform, "PortL", "s41k_plug_0", 4, false, new Vector2(-2.25f, -1.2f));
        Art(root.transform, "PortR", "s41k_plug_3", 4, false, new Vector2(2.25f, -1.2f));
        var slots = new List<Transform>();
        for (int i = 0; i < 3; i++)
        {
            Transform s = Child(root.transform, "Slot" + i, new Vector2(-1.35f + i * 1.35f, -1.2f)).transform;
            Bar(s, "Mark", Vector2.zero, new Vector2(1.2f, 0.5f), new Color(1f, 1f, 1f, 0.3f), 2);
            slots.Add(s);
        }
        int[] left = { 0, 1, 2 }, right = { 1, 2, 3 };
        Vector2[] start = { new Vector2(1.2f, -3.0f), new Vector2(-0.2f, -3.55f), new Vector2(-1.4f, -2.55f) };
        var segs = new List<Transform>();
        var cables = new List<SpriteRenderer>();
        for (int i = 0; i < 3; i++)
        {
            Transform s = Child(root.transform, "Cable" + i, start[i]).transform;
            cables.Add(Art(s, "Art", "s41k_cable", 5, false).GetComponent<SpriteRenderer>());
            Art(s, "PlugL", "s41k_plug_" + left[i], 6, false, new Vector2(-0.55f, 0f));
            Art(s, "PlugR", "s41k_plug_" + right[i], 6, false, new Vector2(0.55f, 0f));
            segs.Add(s);
        }
        Transform connect = S41K_Button(root.transform, "Connect", new Vector2(0f, -4.4f), "CONNECT", new Color(0.55f, 0.92f, 0.55f), 1.25f);
        Transform spark = Child(root.transform, "Spark", new Vector2(0f, -1.2f)).transform;
        Art(spark, "Art", "s41k_spark", 9, false);

        S41K_Goal(root.transform, "FIX THE INTERNET");
        SetArray(level, "segments", segs.ToArray());
        SetArray(level, "slots", slots.ToArray());
        SetArray(level, "cableArts", cables.ToArray());
        SetField(level, "computer", computer);
        SetField(level, "connectButton", connect);
        SetField(level, "spark", spark);
        SetField(level, "computerArt", compArt);
        SetField(level, "onlineSprite", sprites["s41k_computer_ok"]);
        SetField(level, "screenText", screen);
        SetField(level, "connectArea", connect.GetComponent<Collider2D>());
        SetField(level, "triesText", S41K_Info(root.transform, "Tries", new Vector2(0f, 2.65f), 3.6f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.25f, 2.0f)));
        SetArray(level, "confetti", S41K_Confetti(root.transform));
        return SaveLevel(root, "TheBrokenInternet");
    }

    // ---------- 194 The laughing lock ----------

    static GameObject BuildS41KLaughingLock()
    {
        var root = new GameObject("Level_TheLaughingLock");
        LaughingLockLevel level = root.AddComponent<LaughingLockLevel>();
        Art(root.transform, "Room", "s41k_lockroom_bg", -20, false);
        Transform door = Child(root.transform, "Door", new Vector2(-1.45f, -3.9f)).transform;
        Art(door, "Art", "s41k_wood_door", 2, false);
        Transform handle = Child(door, "Handle", new Vector2(2.4f, 2.6f)).transform;
        Art(handle, "Art", "s41k_handle", 4, false);
        CircleCollider2D hc = handle.gameObject.AddComponent<CircleCollider2D>();
        hc.radius = 0.6f;
        hc.offset = new Vector2(0.3f, 0f);
        Transform lockBody = Child(root.transform, "Lock", new Vector2(0f, 0.4f)).transform;
        SpriteRenderer lockArt = Art(lockBody, "Art", "s41k_lock_laugh", 5, false).GetComponent<SpriteRenderer>();
        Transform hole = Child(root.transform, "KeyHole", new Vector2(0f, 0.2f)).transform;
        Transform key = Child(root.transform, "Key", new Vector2(-0.2f, -4.2f)).transform;
        Art(key, "Art", "key", 7, false);
        var cracks = new List<Object>();
        foreach (Vector2 c in new[] { new Vector2(-0.4f, 0f), new Vector2(-0.95f, 0.05f) })
        {
            Transform cr = Art(key, "Crack", "s41k_crack", 8, false, c);
            cracks.Add(cr.gameObject);
        }
        // misuratore della stanchezza
        Bar(root.transform, "TiredBack", new Vector2(0f, 2.62f), new Vector2(2.7f, 0.36f), new Color(0.12f, 0.09f, 0.16f, 0.7f), 20);
        Transform fill = Child(root.transform, "TiredFill", new Vector2(-1.27f, 2.62f)).transform;
        Bar(fill, "Bar", new Vector2(1.27f, 0f), new Vector2(2.54f, 0.24f), new Color(0.55f, 0.6f, 1f), 21);
        Fit(Label(root.transform, new Vector2(0f, 2.62f), 22, "TIREDNESS", 0.04f), 2.4f, 0.24f).color = Color.white;
        TextMesh ha = Fit(Label(root.transform, new Vector2(-1.75f, 1.45f), 30, "HA HA HA!", 0.07f), 1.4f, 0.5f);
        ha.color = new Color(0.95f, 0.25f, 0.3f);

        S41K_Goal(root.transform, "OPEN THE LAUGHING LOCK");
        SetField(level, "lockBody", lockBody);
        SetField(level, "handle", handle);
        SetField(level, "key", key);
        SetField(level, "keyHole", hole);
        SetField(level, "tiredFill", fill);
        SetField(level, "door", door);
        SetField(level, "lockArt", lockArt);
        SetField(level, "laughSprite", sprites["s41k_lock_laugh"]);
        SetField(level, "tiredSprite", sprites["s41k_lock_tired"]);
        SetField(level, "yawnSprite", sprites["s41k_lock_yawn"]);
        SetField(level, "openSprite", sprites["s41k_lock_open"]);
        SetField(level, "handleArea", hc);
        SetArray(level, "cracks", cracks.ToArray());
        SetField(level, "haText", ha);
        SetField(level, "triesText", S41K_Info(root.transform, "Tries", new Vector2(0f, -4.85f), 3.4f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.55f, 1.55f)));
        SetArray(level, "confetti", S41K_Confetti(root.transform));
        return SaveLevel(root, "TheLaughingLock");
    }

    // ---------- 195 The fake boss ----------

    static GameObject BuildS41KFakeBoss()
    {
        var root = new GameObject("Level_TheFakeBoss");
        FakeBossLevel level = root.AddComponent<FakeBossLevel>();
        Art(root.transform, "Arena", "s41k_arena_bg", -20, false);
        var lanes = new List<SpriteRenderer>();
        foreach (float x in new[] { -1.7f, 0f, 1.7f })
            lanes.Add(Bar(root.transform, "Lane", new Vector2(x, -2.65f), new Vector2(1.6f, 3.6f), new Color(1f, 0.15f, 0.15f, 0f), 2));
        Transform boss = Child(root.transform, "Boss", new Vector2(0f, 0.75f)).transform;
        SpriteRenderer bossArt = Art(boss, "Art", "s41k_boss", 3, false).GetComponent<SpriteRenderer>();
        Transform bar = Child(root.transform, "HealthBar", new Vector2(0f, 2.6f)).transform;
        Bar(bar, "Fill", new Vector2(0.18f, 0f), new Vector2(3.88f, 0.34f), new Color(0.9f, 0.2f, 0.25f), 19);
        Art(bar, "Frame", "s41k_hpbar", 20, false);
        Fit(Label(bar, new Vector2(0.18f, 0f), 21, "MEGA DOOM  9999 HP", 0.045f), 3.6f, 0.28f).color = Color.white;
        Transform hero = Child(root.transform, "Hero", new Vector2(0f, -3.4f)).transform;
        SpriteRenderer heroArt = Art(hero, "Art", "s41k_hero", 6, false).GetComponent<SpriteRenderer>();
        SpriteRenderer flash = Bar(root.transform, "Flash", Vector2.zero, new Vector2(12f, 16f), new Color(1f, 0.2f, 0.2f, 0f), 50);
        TextMesh warn = Fit(Label(root.transform, new Vector2(0f, -1.25f), 30, "", 0.1f), 3.5f, 0.6f);
        warn.color = new Color(1f, 0.85f, 0.25f);
        SmokeEmitter dust = Emitter(root.transform, "Dust", new Vector2(0f, -1.0f), "puff", 14f, 1.0f, 0.5f, 4.5f, 0.8f, new Color(0.75f, 0.7f, 0.8f, 0.85f), 8);
        Fit(Label(root.transform, new Vector2(0f, -4.15f), 7, "DRAG TO DODGE", 0.04f), 2.6f, 0.26f).color = new Color(1f, 1f, 1f, 0.75f);

        S41K_Goal(root.transform, "DEFEAT THE BOSS");
        SetField(level, "boss", boss);
        SetField(level, "healthBar", bar);
        SetField(level, "hero", hero);
        SetField(level, "bossArt", bossArt);
        SetField(level, "heroArt", heroArt);
        SetField(level, "flash", flash);
        SetField(level, "puppetSprite", sprites["s41k_puppet"]);
        SetArray(level, "lanes", lanes.ToArray());
        SetField(level, "warnText", warn);
        SetField(level, "dust", dust);
        SetField(level, "triesText", S41K_Info(root.transform, "Tries", new Vector2(0f, -4.65f), 3.6f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.5f, 1.95f)));
        SetArray(level, "confetti", S41K_Confetti(root.transform));
        return SaveLevel(root, "TheFakeBoss");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V41K = new Dictionary<string, string[]>
    {
        { "the_two_finger_pianist", new[] { "Use more than one finger.", "Hold the first key and press the second simultaneously." } },
        { "the_moon_thief", new[] { "The clouds can conceal the moon.", "Move the moon behind a cloud and take the cloud away." } },
        { "back_to_the_future", new[] { "Time can move forward.", "Swipe the calendar to the exact date on the sign." } },
        { "the_three_idiots", new[] { "One of them is facing the wrong way.", "Rotate the wrong character and blow the virtual whistle." } },
        { "the_shy_star", new[] { "Let her see herself.", "Place the mirror where the star can see her reflection." } },
        { "the_broken_internet", new[] { "Follow the cable.", "Join the three cable segments in the correct order." } },
        { "the_laughing_lock", new[] { "Make it tired.", "Hold the handle until it yawns, then insert the key." } },
        { "the_fake_boss", new[] { "His strength is displayed somewhere.", "Remove the health bar." } },
    };
}
#endif
