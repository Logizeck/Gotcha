#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli della specifica 105-200 (in gioco dal 151), pacchetto A.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels41A() => new List<LevelDefinition>
    {
        Define("DontTouchMe", "dont_touch_me", "DON'T TOUCH ME!", "He's really sensitive.", BuildS41ADontTouchMeLevel()),
        Define("BalloonSurgery", "balloon_surgery", "BALLOON SURGERY", "Don't pop it.", BuildS41ABalloonSurgeryLevel()),
        Define("TheLazyElevator", "the_lazy_elevator", "THE LAZY ELEVATOR", "The fifth floor still exists.", BuildS41ALazyElevatorLevel()),
        Define("PhoneSandwich", "phone_sandwich", "PHONE SANDWICH", "Gravity is your chef.", BuildS41APhoneSandwichLevel()),
        Define("LeftBehind", "left_behind", "LEFT BEHIND", "The instruction might be hiding something.", BuildS41ALeftBehindLevel()),
        Define("SleepingGuard", "sleeping_guard", "SLEEPING GUARD", "He makes enough noise himself.", BuildS41ASleepingGuardLevel()),
        Define("TheBrokenCompass", "the_broken_compass", "THE BROKEN COMPASS", "Something is upside down.", BuildS41ABrokenCompassLevel()),
        Define("FakeUpdate", "fake_update", "FAKE UPDATE", "The update isn't real.", BuildS41AFakeUpdateLevel()),
    };

    /// <summary>Sfondo disegnato più un fondo pieno grande (per gli schermi più larghi del previsto).</summary>
    static void S41A_Backdrop(Transform root, string sprite, Color edge)
    {
        Bar(root, "Edge", Vector2.zero, new Vector2(14f, 18f), edge, -21);
        Art(root, "Backdrop", sprite, -20, false);
    }

    /// <summary>Pulsante arrotondato (pannello 9-slice colorato) con scritta e collider.</summary>
    static Transform S41A_Button(Transform parent, string name, Vector2 pos, Vector2 size, Color color, string text, int order, float textSize = 0.05f)
    {
        Transform b = Child(parent, name, pos).transform;
        SpriteRenderer sr = Art(b, "Panel", "ui_panel", order, false).GetComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
        sr.color = color;
        Fit(Label(b, Vector2.zero, order + 1, text, textSize), size.x - 0.2f, size.y - 0.1f);
        b.gameObject.AddComponent<BoxCollider2D>().size = size;
        return b;
    }

    static Transform S41A_Holder(Transform parent, string name, string sprite, Vector2 pos, float scale, int order)
    {
        Transform h = Child(parent, name, pos).transform;
        h.localScale = Vector3.one * scale;
        Art(h, "Art", sprite, order, false);
        return h;
    }

    // ---------- 105: Don't touch me ----------

    static GameObject BuildS41ADontTouchMeLevel()
    {
        var root = new GameObject("Level_DontTouchMe");
        DontTouchMeLevel level = root.AddComponent<DontTouchMeLevel>();
        S41A_Backdrop(root.transform, "room_bg", new Color(0.95f, 0.88f, 0.72f));
        Art(root.transform, "Sign", "s41a_sign", 2, false, new Vector2(0.1f, 0.95f));
        TextMesh sign = Fit(Label(root.transform, new Vector2(0.1f, 0.95f), 3, "DON'T TOUCH ME", 0.065f), 3.2f, 0.7f);
        sign.color = new Color(0.75f, 0.12f, 0.12f);
        Vector2 basePos = new Vector2(0f, -1.9f);
        Art(root.transform, "Base", "s41a_btn_base", 3, false, basePos);
        Transform dome = Child(root.transform, "Dome", basePos + new Vector2(0f, -0.04f)).transform;
        SpriteRenderer red = Art(dome, "Red", "s41a_btn_dome_red", 4, false).GetComponent<SpriteRenderer>();
        SpriteRenderer purple = Art(dome, "Purple", "s41a_btn_dome_purple", 5, false).GetComponent<SpriteRenderer>();
        SpriteRenderer flat = Art(dome, "Flat", "s41a_btn_dome_flat", 4, false).GetComponent<SpriteRenderer>();
        Transform area = Child(root.transform, "Area", basePos + new Vector2(0f, 0.45f)).transform;
        area.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(2.7f, 1.6f);
        Transform anger = S41A_Holder(root.transform, "Anger", "anger", basePos + new Vector2(0.95f, 1.25f), 1.6f, 8);
        var cracks = new List<Object>();
        Vector3[] cr = { new Vector3(-0.6f, 0.9f, 3.2f), new Vector3(1.3f, 2.4f, 2.4f), new Vector3(-1.7f, -1.6f, 2.6f), new Vector3(1.5f, -2.6f, 2.0f), new Vector3(-1.3f, 3.0f, 1.8f) };
        foreach (Vector3 c in cr)
        {
            Transform t = S41A_Holder(root.transform, "Crack", "glass_impact", new Vector2(c.x, c.y), c.z, 90);
            t.localRotation = Quaternion.Euler(0f, 0f, c.x * 40f);
            cracks.Add(t.GetComponentInChildren<SpriteRenderer>());
        }
        SpriteRenderer flash = Bar(root.transform, "Flash", Vector2.zero, new Vector2(14f, 18f), new Color(0.05f, 0.04f, 0.08f, 0.6f), 85);
        var stars = new List<Object>();
        foreach (Vector2 s in new[] { new Vector2(-1.6f, -0.8f), new Vector2(1.5f, -0.6f), new Vector2(-0.9f, 0.1f), new Vector2(1.0f, 0.3f) })
            stars.Add(S41A_Holder(root.transform, "Star", "sparkle_star", s, 0.9f, 9).GetComponentInChildren<SpriteRenderer>());
        TextMesh tip = Fit(Label(root.transform, new Vector2(0f, -3.6f), 4, "(it's just a button...)", 0.04f), 4.5f);
        tip.color = new Color(0.45f, 0.35f, 0.3f);

        SetField(level, "dome", dome);
        SetField(level, "domeRed", red);
        SetField(level, "domePurple", purple);
        SetField(level, "domeFlat", flat);
        SetField(level, "buttonArea", area.GetComponent<BoxCollider2D>());
        SetField(level, "anger", anger);
        SetArray(level, "cracks", cracks.ToArray());
        SetField(level, "flash", flash);
        SetArray(level, "stars", stars.ToArray());
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.1f, -0.25f)));
        return SaveLevel(root, "DontTouchMe");
    }

    // ---------- 106: Balloon surgery ----------

    static GameObject BuildS41ABalloonSurgeryLevel()
    {
        var root = new GameObject("Level_BalloonSurgery");
        BalloonSurgeryLevel level = root.AddComponent<BalloonSurgeryLevel>();
        S41A_Backdrop(root.transform, "sky_bg", new Color(0.55f, 0.78f, 0.95f));
        Bar(root.transform, "Pit", new Vector2(0f, -5.3f), new Vector2(14f, 2.2f), new Color(0.27f, 0.24f, 0.32f), 0);
        foreach (float x in new[] { -2.0f, 0f, 2.0f }) Art(root.transform, "Spikes", "s41a_spikes", 1, false, new Vector2(x, -4.25f));
        Art(root.transform, "Pillar", "s41a_ledge", 2, false, new Vector2(0f, -2.2f));
        Transform mouse = S41A_Holder(root.transform, "Mouse", "mouse", new Vector2(0.55f, -1.9f), 0.5f, 5);
        Transform balloon = Child(root.transform, "Balloon", new Vector2(0f, 1.6f)).transform;
        SpriteRenderer balloonArt = Art(balloon, "Art", "balloon", 6, false).GetComponent<SpriteRenderer>();
        Transform key = S41A_Holder(root.transform, "Key", "key", new Vector2(0f, 0.2f), 0.7f, 7);
        key.localRotation = Quaternion.Euler(0f, 0f, -90f);
        LineRenderer rope = Line(root.transform, "Rope", 0.03f, new Color(0.17f, 0.13f, 0.2f), 5);
        SpriteRenderer scrap = S41A_Holder(root.transform, "Scrap", "balloon_scrap", Vector2.zero, 1.6f, 8).GetComponentInChildren<SpriteRenderer>();
        var puffs = new List<Object>();
        for (int i = 0; i < 3; i++) puffs.Add(S41A_Holder(root.transform, "Puff", "puff", Vector2.zero, 1.1f, 9).GetComponentInChildren<SpriteRenderer>());
        TextMesh status = Fit(Label(root.transform, new Vector2(0f, -4.55f), 20, "GENTLY...", 0.055f), 5f, 0.5f);
        Bar(root.transform, "StatusBack", new Vector2(0f, -4.55f), new Vector2(3.6f, 0.5f), new Color(1f, 1f, 1f, 0.85f), 19);

        SetField(level, "balloon", balloon);
        SetField(level, "balloonArt", balloonArt);
        SetField(level, "key", key);
        SetField(level, "rope", rope);
        SetField(level, "mouse", mouse);
        SetField(level, "scrap", scrap);
        SetArray(level, "puffs", puffs.ToArray());
        SetField(level, "status", status);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.45f, -0.95f)));
        return SaveLevel(root, "BalloonSurgery");
    }

    // ---------- 107: The lazy elevator ----------

    static GameObject BuildS41ALazyElevatorLevel()
    {
        var root = new GameObject("Level_TheLazyElevator");
        LazyElevatorLevel level = root.AddComponent<LazyElevatorLevel>();
        S41A_Backdrop(root.transform, "s41a_tower_bg", new Color(0.62f, 0.78f, 0.92f));
        string[] names = { "G", "1", "2", "3", "4", "5" };
        for (int k = 0; k < 6; k++)
        {
            TextMesh n = Label(root.transform, new Vector2(-1.38f, -4.2f + k * 1.2f + 0.95f), 2, names[k], 0.035f);
            n.color = new Color(0.45f, 0.3f, 0.25f);
        }
        Transform cheese = S41A_Holder(root.transform, "Cheese", "cheese_wedge", new Vector2(-0.6f, 1.97f), 0.8f, 3);
        Transform car = Child(root.transform, "Car", new Vector2(-2.2f, -4.07f)).transform;
        car.localScale = Vector3.one * 0.82f;
        Art(car, "Art", "e40_car", 3, false);
        Transform mouse = S41A_Holder(car, "Mouse", "mouse", new Vector2(0f, 0.34f), 0.4f, 4);

        Vector2 panelAt = new Vector2(1.75f, -0.8f);
        Art(root.transform, "Panel", "s41a_lift_panel", 2, false, panelAt);
        TextMesh display = Fit(Label(root.transform, panelAt + new Vector2(0f, 2.15f), 4, "G", 0.05f), 1.2f, 0.45f);
        display.color = new Color(0.5f, 1f, 0.6f);
        var buttons = new List<Object>();
        for (int i = 0; i < 4; i++)
        {
            Transform b = Child(root.transform, "Button" + (i + 1), new Vector2(1.75f, -2.25f + i * 0.75f)).transform;
            Transform art = Art(b, "Art", "e40_floor_btn", 5, false);
            art.localScale = Vector3.one * 2.1f;
            art.localPosition *= 2.1f;
            TextMesh t = Label(b, new Vector2(0f, -0.02f), 6, (i + 1).ToString(), 0.06f);
            t.color = new Color(0.45f, 0.3f, 0.1f);
            buttons.Add(b);
        }
        Transform broken = S41A_Holder(root.transform, "Button5", "s41a_lift_btn_broken", new Vector2(1.75f, 0.75f), 1f, 5);
        broken.gameObject.AddComponent<CircleCollider2D>().radius = 0.5f;
        TextMesh five = Label(broken, new Vector2(0f, 0f), 6, "5", 0.05f);
        five.color = new Color(0.4f, 0.38f, 0.35f, 0.7f);
        var sparks = new List<Object>();
        foreach (Vector2 s in new[] { new Vector2(1.25f, 1.1f), new Vector2(2.25f, 1.05f), new Vector2(1.8f, 1.35f) })
        {
            SpriteRenderer sp = S41A_Holder(root.transform, "Spark", "sparkle_star", s, 0.6f, 8).GetComponentInChildren<SpriteRenderer>();
            sp.color = new Color(1f, 0.85f, 0.2f);
            sparks.Add(sp);
        }
        TextMesh plus = Label(root.transform, Vector2.zero, 7, "+", 0.07f);
        plus.color = new Color(1f, 0.8f, 0.2f);
        TextMesh strikes = Fit(Label(root.transform, new Vector2(1.6f, -3.85f), 6, "", 0.035f), 2.4f);
        strikes.color = Color.white;

        SetField(level, "car", car);
        SetField(level, "mouse", mouse);
        SetField(level, "cheese", cheese);
        SetArray(level, "buttons", buttons.ToArray());
        SetField(level, "brokenButton", broken.GetComponent<CircleCollider2D>());
        SetField(level, "display", display);
        SetField(level, "plusSign", plus);
        SetField(level, "strikesLabel", strikes);
        SetArray(level, "sparks", sparks.ToArray());
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.9f, 3.3f)));
        return SaveLevel(root, "TheLazyElevator");
    }

    // ---------- 108: Phone sandwich ----------

    static GameObject BuildS41APhoneSandwichLevel()
    {
        var root = new GameObject("Level_PhoneSandwich");
        PhoneSandwichLevel level = root.AddComponent<PhoneSandwichLevel>();
        S41A_Backdrop(root.transform, "s41a_galley_bg", new Color(0.72f, 0.77f, 0.87f));
        Art(root.transform, "Plate", "s41a_plate", 2, false, new Vector2(0f, -3.3f));
        Transform bread = S41A_Holder(root.transform, "BreadBottom", "s41a_bread_bottom", new Vector2(0f, -3.12f), 1f, 3);
        string[] fill = { "s41a_cheese", "s41a_lettuce", "s41a_tomato" };
        Vector2[] at = { new Vector2(-1.6f, 2.2f), new Vector2(0.3f, 1.2f), new Vector2(1.7f, 2.6f) };
        var items = new List<Object>();
        for (int i = 0; i < 3; i++) items.Add(S41A_Holder(root.transform, "Item" + i, fill[i], at[i], 1f, 5));
        Transform top = S41A_Holder(root.transform, "TopSlice", "s41a_bread_top", new Vector2(0f, 3.0f), 1f, 6);
        Transform mouse = S41A_Holder(root.transform, "Mouse", "mouse", new Vector2(1.8f, -1.7f), 0.6f, 7);
        Transform flipHint = S41A_Holder(root.transform, "FlipHint", "rotate_phone", new Vector2(0f, 1.0f), 1.2f, 20);
        Transform flipButton = S41A_Button(root.transform, "FlipButton", new Vector2(0f, -0.6f), new Vector2(1.8f, 0.8f), new Color(1f, 0.8f, 0.3f), "FLIP!", 20, 0.07f);
        TextMesh goal = Fit(Label(root.transform, new Vector2(0f, -4.0f), 20, "", 0.05f), 5.2f, 0.45f);
        TextMesh drops = Fit(Label(root.transform, new Vector2(0f, -4.42f), 20, "", 0.038f), 4f);

        SetArray(level, "items", items.ToArray());
        SetField(level, "breadBottom", bread);
        SetField(level, "topSlice", top);
        SetField(level, "mouse", mouse);
        SetField(level, "flipHint", flipHint);
        SetField(level, "flipButton", flipButton.GetComponent<BoxCollider2D>());
        SetField(level, "goal", goal);
        SetField(level, "dropsLabel", drops);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.1f, -0.75f)));
        return SaveLevel(root, "PhoneSandwich");
    }

    // ---------- 109: Left behind ----------

    static Transform S41A_Door(Transform root, string name, Vector2 pos, out Transform panel, out SpriteRenderer light)
    {
        Transform door = Child(root, name, pos).transform;
        door.localScale = Vector3.one * 0.42f;
        Art(door, "Frame", "door_frame", 2, false);
        light = Bar(door, "Light", Vector2.zero, new Vector2(2.3f, 3.7f), new Color(1f, 0.96f, 0.7f), 3);
        panel = Child(door, "Panel", new Vector2(-1.1f, 0f)).transform;
        Art(panel, "Art", "door_panel", 4, false);
        door.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(2.9f, 4.1f);
        return door;
    }

    static GameObject BuildS41ALeftBehindLevel()
    {
        var root = new GameObject("Level_LeftBehind");
        LeftBehindLevel level = root.AddComponent<LeftBehindLevel>();
        S41A_Backdrop(root.transform, "s41a_island_bg", new Color(0.8f, 0.6f, 0.65f));
        Art(root.transform, "Sign", "s41a_sign", 2, false, new Vector2(0f, 2.4f));
        Fit(Label(root.transform, new Vector2(-1.15f, 2.4f), 3, "GO", 0.08f), 1f, 0.75f);
        TextMesh left = Fit(Label(root.transform, new Vector2(0.55f, 2.4f), 3, "LEFT", 0.08f), 1.7f, 0.75f);
        left.color = new Color(0.2f, 0.55f, 0.25f);
        Transform tag = Child(root.transform, "RightTag", new Vector2(0.55f, 2.4f)).transform;
        Bar(tag, "Paper", Vector2.zero, new Vector2(1.95f, 0.85f), new Color(1f, 0.96f, 0.86f), 4);
        Bar(tag, "Tape", new Vector2(-0.85f, 0.35f), new Vector2(0.4f, 0.14f), new Color(0.85f, 0.85f, 0.75f, 0.9f), 6);
        TextMesh right = Fit(Label(tag, Vector2.zero, 5, "RIGHT", 0.08f), 1.75f, 0.75f);
        right.color = new Color(0.8f, 0.2f, 0.2f);
        tag.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.95f, 0.9f);
        Transform arrow = S41A_Holder(root.transform, "Arrow", "push_arrow", new Vector2(0f, 0.75f), 2.0f, 2);
        Transform leftDoor = S41A_Door(root.transform, "LeftDoor", new Vector2(-2.05f, -1.57f), out Transform leftPanel, out SpriteRenderer leftLight);
        Transform rightDoor = S41A_Door(root.transform, "RightDoor", new Vector2(2.05f, -1.57f), out Transform rightPanel, out SpriteRenderer rightLight);
        Transform padlock = S41A_Holder(root.transform, "Padlock", "e40_padlock", new Vector2(-1.85f, -1.55f), 0.35f, 6);
        Transform mouse = S41A_Holder(root.transform, "Mouse", "mouse", new Vector2(0f, -2.07f), 0.55f, 6);
        TextMesh tries = Fit(Label(root.transform, new Vector2(0f, -3.0f), 10, "", 0.045f), 4f);
        tries.color = Color.white;

        SetField(level, "rightTag", tag);
        SetField(level, "rightTagArea", tag.GetComponent<BoxCollider2D>());
        SetField(level, "leftWord", left);
        SetField(level, "arrow", arrow);
        SetField(level, "leftDoor", leftDoor.GetComponent<BoxCollider2D>());
        SetField(level, "rightDoor", rightDoor.GetComponent<BoxCollider2D>());
        SetField(level, "leftPanel", leftPanel);
        SetField(level, "rightPanel", rightPanel);
        SetField(level, "leftLight", leftLight);
        SetField(level, "rightLight", rightLight);
        SetField(level, "padlock", padlock);
        SetField(level, "mouse", mouse);
        SetField(level, "triesLabel", tries);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.1f, -0.95f)));
        return SaveLevel(root, "LeftBehind");
    }

    // ---------- 110: Sleeping guard ----------

    static GameObject BuildS41ASleepingGuardLevel()
    {
        var root = new GameObject("Level_SleepingGuard");
        SleepingGuardLevel level = root.AddComponent<SleepingGuardLevel>();
        S41A_Backdrop(root.transform, "s41a_jail_bg", new Color(0.5f, 0.47f, 0.56f));
        Transform guardHolder = Child(root.transform, "Guard", new Vector2(-1.5f, -3.4f)).transform;
        SpriteRenderer guard = Art(guardHolder, "Art", "s41a_guard_sleep", 3, false).GetComponent<SpriteRenderer>();
        // porta della cella (anta incernierata a sinistra) con il vano scuro dietro
        Bar(root.transform, "Doorway", new Vector2(1.9f, -2.2f), new Vector2(1.5f, 2.4f), new Color(0.08f, 0.07f, 0.12f), 1);
        Transform doorPanel = Child(root.transform, "DoorPanel", new Vector2(1.16f, -3.4f)).transform;
        Art(doorPanel, "Art", "s41a_cell_door", 2, false, new Vector2(0.74f, 0f));
        Transform keyhole = Child(root.transform, "Keyhole", new Vector2(2.35f, -2.3f)).transform;
        Transform key = S41A_Holder(root.transform, "Key", "key", new Vector2(-0.15f, -3.2f), 0.7f, 7);
        BoxCollider2D keyBox = key.gameObject.AddComponent<BoxCollider2D>();
        keyBox.size = new Vector2(2.1f, 1.3f);
        keyBox.offset = new Vector2(-0.65f, 0f);
        Transform mouse = S41A_Holder(root.transform, "Mouse", "mouse", new Vector2(0.75f, -3.1f), 0.5f, 5);
        Label(root.transform, new Vector2(-2.05f, 3.05f), 10, "NOISE", 0.04f).color = Color.white;
        Transform meter = GaugeBar(root.transform, "Meter", new Vector2(0.55f, 3.05f), new Vector2(3.2f, 0.32f), new Color(0.12f, 0.09f, 0.16f), Color.white, 10);
        Bar(root.transform, "Mark", new Vector2(0.55f - 1.6f + 3.2f * 0.55f, 3.05f), new Vector2(0.05f, 0.5f), new Color(1f, 0.9f, 0.3f), 12);
        TextMesh meterLabel = Fit(Label(root.transform, new Vector2(0.4f, 2.6f), 10, "", 0.042f), 4.5f);
        meterLabel.color = Color.white;
        TextMesh strikes = Fit(Label(root.transform, new Vector2(0f, -4.25f), 10, "", 0.042f), 4f);
        strikes.color = Color.white;
        var zzz = new List<Object>();
        foreach (Vector2 z in new[] { new Vector2(-1.15f, -1.0f), new Vector2(-1.0f, -0.8f), new Vector2(-1.2f, -0.9f) })
            zzz.Add(S41A_Holder(root.transform, "Zzz", "zzz", z, 1f, 8));

        SetField(level, "guard", guard);
        SetField(level, "sleepSprite", sprites["s41a_guard_sleep"]);
        SetField(level, "awakeSprite", sprites["s41a_guard_awake"]);
        SetField(level, "key", key);
        SetField(level, "keyArea", keyBox);
        SetField(level, "keyhole", keyhole);
        SetField(level, "doorPanel", doorPanel);
        SetField(level, "mouse", mouse);
        SetField(level, "meterFill", meter);
        SetField(level, "meterColor", meter.GetComponentInChildren<SpriteRenderer>());
        SetField(level, "meterLabel", meterLabel);
        SetField(level, "strikesLabel", strikes);
        SetArray(level, "zzz", zzz.ToArray());
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.3f, -0.4f)));
        return SaveLevel(root, "SleepingGuard");
    }

    // ---------- 111: The broken compass ----------

    static GameObject BuildS41ABrokenCompassLevel()
    {
        var root = new GameObject("Level_TheBrokenCompass");
        BrokenCompassLevel level = root.AddComponent<BrokenCompassLevel>();
        S41A_Backdrop(root.transform, "s41a_hedge_bg", new Color(0.24f, 0.56f, 0.29f));
        Vector2[] ends = { new Vector2(0f, 3.1f), new Vector2(2.35f, 1.4f), new Vector2(-2.35f, 1.4f) };
        float[] rot = { 90f, 0f, 180f };
        Vector2[] arrowsAt = { new Vector2(0f, 2.3f), new Vector2(0.95f, 1.4f), new Vector2(-0.95f, 1.4f) };
        var paths = new List<Object>();
        var endsT = new List<Object>();
        var fogs = new List<Object>();
        for (int i = 0; i < 3; i++)
        {
            endsT.Add(Child(root.transform, "End" + i, ends[i]).transform);
            SpriteRenderer fog = S41A_Holder(root.transform, "Fog" + i, "puff", ends[i], 2.8f, 6).GetComponentInChildren<SpriteRenderer>();
            fog.color = new Color(0.96f, 0.97f, 1f, 1f);
            fogs.Add(fog);
            Transform a = S41A_Holder(root.transform, "Path" + i, "ctrl_arrow", arrowsAt[i], 1.3f, 8);
            a.localRotation = Quaternion.Euler(0f, 0f, rot[i]);
            a.GetComponentInChildren<SpriteRenderer>().color = new Color(1f, 0.88f, 0.35f);
            a.gameObject.AddComponent<CircleCollider2D>().radius = 0.4f;
            paths.Add(a.GetComponent<CircleCollider2D>());
        }
        Transform cheese = S41A_Holder(root.transform, "Cheese", "cheese_wedge", ends[0], 0.9f, 5);
        Transform mouse = S41A_Holder(root.transform, "Mouse", "mouse", new Vector2(0f, 1.35f), 0.45f, 5);

        Transform compass = Child(root.transform, "Compass", new Vector2(0f, -2.75f)).transform;
        SpriteRenderer glow = S41A_Holder(compass, "Glow", "glow", Vector2.zero, 2.4f, 9).GetComponentInChildren<SpriteRenderer>();
        glow.color = new Color(1f, 0.9f, 0.4f, 0.8f);
        Art(compass, "Body", "s41a_compass_body", 10, false);
        Transform bezel = Child(compass, "Bezel", Vector2.zero).transform;
        Art(bezel, "Art", "s41a_compass_bezel", 11, false);
        string[] letters = { "N", "E", "S", "W" };
        Vector2[] lp = { new Vector2(0f, 0.89f), new Vector2(0.89f, 0f), new Vector2(0f, -0.89f), new Vector2(-0.89f, 0f) };
        for (int i = 0; i < 4; i++)
        {
            TextMesh l = Label(bezel, lp[i], 12, letters[i], 0.03f);
            l.color = i == 0 ? new Color(1f, 0.45f, 0.4f) : Color.white;
            l.transform.localRotation = Quaternion.Euler(0f, 0f, -90f * i);
        }
        Transform needle = Child(compass, "Needle", Vector2.zero).transform;
        Art(needle, "Art", "s41a_compass_needle", 13, false);
        TextMesh strikes = Fit(Label(root.transform, new Vector2(1.75f, -0.45f), 10, "", 0.04f), 2.1f);
        strikes.color = Color.white;

        SetField(level, "compass", compass);
        SetField(level, "bezel", bezel);
        SetField(level, "needle", needle);
        SetField(level, "glow", glow);
        SetArray(level, "paths", paths.ToArray());
        SetArray(level, "pathEnds", endsT.ToArray());
        SetArray(level, "fogs", fogs.ToArray());
        SetField(level, "cheese", cheese);
        SetField(level, "mouse", mouse);
        SetField(level, "strikesLabel", strikes);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.3f, 0.5f)));
        return SaveLevel(root, "TheBrokenCompass");
    }

    // ---------- 112: Fake update ----------

    static GameObject BuildS41AFakeUpdateLevel()
    {
        var root = new GameObject("Level_FakeUpdate");
        FakeUpdateLevel level = root.AddComponent<FakeUpdateLevel>();
        S41A_Backdrop(root.transform, "s41a_monitor_bg", new Color(0.95f, 0.9f, 0.81f));
        Color paper = new Color(1f, 0.957f, 0.839f);
        Art(root.transform, "Window", "s41a_upd_window", 2, false, new Vector2(0f, 0.65f));
        TextMesh title = Fit(Label(root.transform, new Vector2(-0.45f, 2.55f), 3, "GOTCHA UPDATER 9000", 0.04f), 3.2f, 0.4f);
        title.color = Color.white;
        Transform gear = S41A_Holder(root.transform, "Gear", "s41a_upd_gear", new Vector2(0f, 1.72f), 0.8f, 3);
        Vector2 numAt = new Vector2(-0.25f, 0.5f);
        Transform one = Child(root.transform, "One", numAt).transform;
        Label(one, Vector2.zero, 4, "1", 0.075f);
        one.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(0.7f, 0.85f);
        Transform nines = Child(root.transform, "Nines", numAt).transform;
        Bar(nines, "Patch", Vector2.zero, new Vector2(1.5f, 1.1f), paper, 5);
        TextMesh ninesText = Label(nines, Vector2.zero, 6, "99", 0.13f);
        nines.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.5f, 1.1f);
        TextMesh hundred = Label(root.transform, numAt, 6, "100", 0.13f);
        TextMesh percent = Label(root.transform, new Vector2(0.95f, 0.5f), 4, "%", 0.1f);
        Transform bar = GaugeBar(root.transform, "Bar", new Vector2(0f, -0.42f), new Vector2(3.6f, 0.36f), new Color(0.17f, 0.13f, 0.2f), new Color(0.36f, 0.8f, 0.42f), 4);
        TextMesh status = Fit(Label(root.transform, new Vector2(0f, -0.88f), 4, "", 0.038f), 4f, 0.35f);
        Transform speed = S41A_Button(root.transform, "SpeedUp", new Vector2(0f, -1.25f), new Vector2(1.9f, 0.42f), new Color(0.55f, 0.85f, 1f), "SPEED UP", 4, 0.035f);
        TextMesh warn = Fit(Label(root.transform, new Vector2(0f, -2.55f), 4, "Do not unplug your imagination.", 0.035f), 4.6f);
        warn.color = Color.white;

        // finta schermata "UPDATE FAILED" (da cartone, niente a che vedere con le finestre vere)
        GameObject failed = Child(root.transform, "Failed", Vector2.zero);
        Bar(failed.transform, "Shade", Vector2.zero, new Vector2(14f, 18f), new Color(0.1f, 0.05f, 0.15f, 0.7f), 100);
        Transform fw = Art(failed.transform, "Window", "s41a_upd_window", 101, false, new Vector2(0f, 0.6f));
        fw.GetComponent<SpriteRenderer>().color = new Color(1f, 0.82f, 0.9f);
        Transform sad = S41A_Holder(failed.transform, "Gear", "s41a_upd_gear", new Vector2(0f, 1.55f), 0.8f, 102);
        S41A_Holder(sad, "Dizzy", "dizzy_eyes", new Vector2(0f, 0.1f), 0.5f, 103);
        TextMesh f1 = Fit(Label(failed.transform, new Vector2(0f, 0.55f), 103, "UPDATE FAILED!", 0.075f), 4.0f);
        f1.color = new Color(0.85f, 0.15f, 0.25f);
        Fit(Label(failed.transform, new Vector2(0f, 0.05f), 103, "(not really, it's a joke)", 0.04f), 4.0f);
        Fit(Label(failed.transform, new Vector2(0f, -0.4f), 103, "Stuck? Try a HINT!", 0.05f), 4.0f);
        Transform ok = S41A_Button(failed.transform, "Ok", new Vector2(0f, -1.0f), new Vector2(1.5f, 0.55f), new Color(0.55f, 0.9f, 0.6f), "OK", 104, 0.05f);

        var confetti = new List<Object>();
        Color[] cc = { new Color(1f, 0.4f, 0.4f), new Color(1f, 0.85f, 0.3f), new Color(0.4f, 0.8f, 1f), new Color(0.5f, 0.9f, 0.5f), new Color(0.9f, 0.5f, 1f) };
        for (int i = 0; i < 24; i++)
        {
            SpriteRenderer c = S41A_Holder(root.transform, "Confetti", "confetti", new Vector2(-2.4f + (i * 0.73f) % 4.8f, 3.2f + (i % 4) * 0.2f), 2f, 30).GetComponentInChildren<SpriteRenderer>();
            c.color = cc[i % cc.Length];
            confetti.Add(c);
        }

        SetField(level, "nines", nines);
        SetField(level, "ninesArea", nines.GetComponent<BoxCollider2D>());
        SetField(level, "ninesText", ninesText);
        SetField(level, "one", one);
        SetField(level, "oneArea", one.GetComponent<BoxCollider2D>());
        SetField(level, "hundred", hundred);
        SetField(level, "percent", percent);
        SetField(level, "barFill", bar);
        SetField(level, "status", status);
        SetField(level, "speedButton", speed.GetComponent<BoxCollider2D>());
        SetField(level, "gear", gear);
        SetField(level, "failedScreen", failed);
        SetField(level, "okButton", ok.GetComponent<BoxCollider2D>());
        SetArray(level, "confetti", confetti.ToArray());
        return SaveLevel(root, "FakeUpdate");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V41A = new Dictionary<string, string[]>
    {
        { "dont_touch_me", new[] { "A quick touch only makes him angrier.", "Press and hold for five seconds." } },
        { "balloon_surgery", new[] { "Two fingers can control its size.", "Pinch slowly until the key lands safely." } },
        { "the_lazy_elevator", new[] { "The buttons can work together.", "Combine 4 and 1." } },
        { "phone_sandwich", new[] { "Tilt the phone to stack the ingredients.", "Flip the phone when everything is in place." } },
        { "left_behind", new[] { "Move the words.", "Drag RIGHT away to reveal LEFT." } },
        { "sleeping_guard", new[] { "Move when the snoring is loudest.", "Drag the key during the loud snoring." } },
        { "the_broken_compass", new[] { "Rotate your phone.", "Correct the compass after turning the device." } },
        { "fake_update", new[] { "Try moving the percentage.", "Drag 99 upwards, then move the hidden 1 into position." } },
    };
}
#endif
