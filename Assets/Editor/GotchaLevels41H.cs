#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli della specifica 105-200 (in gioco dal 151), pacchetto H.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels41H() => new List<LevelDefinition>
    {
        Define("TheSilentDisco", "the_silent_disco", "THE SILENT DISCO", "One dancer knows the rhythm.", BuildS41HSilentDisco()),
        Define("TheDoorThatLies", "the_door_that_lies", "THE DOOR THAT LIES", "The doors have feelings.", BuildS41HDoorThatLies()),
        Define("InvisibleButton", "invisible_button", "INVISIBLE BUTTON", "You can't see everything.", BuildS41HInvisibleButton()),
        Define("TheSecondKey", "the_second_key", "THE SECOND KEY", "You need a copy.", BuildS41HSecondKey()),
        Define("ShadowThief", "shadow_thief", "SHADOW THIEF", "Even invisible people cast shadows here.", BuildS41HShadowThief()),
        Define("TheBrokenRuler", "the_broken_ruler", "THE BROKEN RULER", "The ruler can still be useful.", BuildS41HBrokenRuler()),
        Define("TheBackwardSong", "the_backward_song", "THE BACKWARD SONG", "Something sounds reversed.", BuildS41HBackwardSong()),
        Define("DontBlink", "dont_blink", "DON'T BLINK!", "She moves when nobody is watching.", BuildS41HDontBlink()),
    };

    // ---------- Aiuti comuni del pacchetto H ----------

    /// <summary>Riga dell'obiettivo in alto, su una fascia scura.</summary>
    static TextMesh S41H_Goal(Transform root, string text)
    {
        Transform h = Child(root, "Goal", new Vector2(0f, 3.25f)).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(5.5f, 0.5f), new Color(0.12f, 0.09f, 0.16f, 0.72f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, text, 0.05f);
        t.color = Color.white;
        Fit(t, 5.3f, 0.42f);
        return t;
    }

    /// <summary>Etichetta bianca su fascia scura (contatori).</summary>
    static TextMesh S41H_Info(Transform root, string name, Vector2 pos, float width, string text = "")
    {
        Transform h = Child(root, name, pos).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(width, 0.4f), new Color(0.12f, 0.09f, 0.16f, 0.62f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, text, 0.04f);
        t.color = Color.white;
        Fit(t, width - 0.2f, 0.32f);
        return t;
    }

    static SmokeEmitter[] S41H_Confetti(Transform root)
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

    /// <summary>Pulsante rettangolare colorato con scritta e collider.</summary>
    static Transform S41H_Button(Transform root, string name, Vector2 pos, Vector2 size, Color color, string text, int order)
    {
        Transform b = Child(root, name, pos).transform;
        Bar(b, "Edge", Vector2.zero, size + new Vector2(0.1f, 0.1f), new Color(0.17f, 0.13f, 0.2f), order);
        Bar(b, "Face", Vector2.zero, size, color, order + 1);
        TextMesh t = Label(b, Vector2.zero, order + 2, text, 0.05f);
        t.color = Color.white;
        Fit(t, size.x - 0.2f, size.y - 0.15f);
        b.gameObject.AddComponent<BoxCollider2D>().size = size + new Vector2(0.2f, 0.2f);
        return b;
    }

    // ---------- 163 The silent disco ----------

    static GameObject BuildS41HSilentDisco()
    {
        var root = new GameObject("Level_TheSilentDisco");
        TheSilentDiscoLevel level = root.AddComponent<TheSilentDiscoLevel>();
        Art(root.transform, "Disco", "disco_bg", -20, false);
        var lights = new List<SpriteRenderer>();
        Color[] lc = { new Color(1f, 0.3f, 0.85f), new Color(0.3f, 0.9f, 1f), new Color(1f, 0.9f, 0.3f), new Color(0.5f, 1f, 0.5f) };
        Vector2[] lp = { new Vector2(-2.2f, 1.6f), new Vector2(2.2f, 1.6f), new Vector2(-1.0f, -3.4f), new Vector2(1.0f, -3.4f) };
        for (int i = 0; i < 4; i++)
        {
            SpriteRenderer l = Art(root.transform, "Light" + i, "glow", -10, false, lp[i]).GetComponent<SpriteRenderer>();
            l.transform.localScale = Vector3.one * 3f;
            l.color = lc[i];
            lights.Add(l);
        }
        Color[] tints = { new Color(1f, 0.5f, 0.5f), new Color(1f, 0.88f, 0.4f), new Color(0.45f, 0.65f, 1f), new Color(0.55f, 0.9f, 0.5f) };
        var dancers = new List<Transform>();
        for (int i = 0; i < 4; i++)
        {
            Transform d = Child(root.transform, "Dancer" + i, new Vector2(-2.0f + i * 1.333f, -2.3f)).transform;
            Art(d, "Art", "s41h_dancer", 3, false).GetComponent<SpriteRenderer>().color = tints[i];
            dancers.Add(d);
        }
        Transform floor = Child(root.transform, "Floor", new Vector2(0f, -3.65f)).transform;
        floor.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(6f, 2.5f);
        Fit(Label(root.transform, new Vector2(0f, -4.5f), 6, "TAP THE FLOOR", 0.045f), 2.6f, 0.3f).color = new Color(1f, 1f, 1f, 0.8f);
        SpriteRenderer ring = Art(root.transform, "Ring", "beat_ring", 8, false).GetComponent<SpriteRenderer>();
        var dots = new List<SpriteRenderer>();
        for (int i = 0; i < 3; i++)
        {
            SpriteRenderer d = Art(root.transform, "Dot" + i, "s41h_bulb", 72, false, new Vector2(-0.5f + i * 0.5f, 2.2f)).GetComponent<SpriteRenderer>();
            d.transform.localScale = Vector3.one * 1.3f;
            dots.Add(d);
        }

        S41H_Goal(root.transform, "TAP THE FLOOR TO THE RIGHT BEAT");
        SetArray(level, "dancers", dancers.ToArray());
        SetField(level, "floorArea", floor.GetComponent<Collider2D>());
        SetField(level, "ring", ring);
        SetArray(level, "tapDots", dots.ToArray());
        SetArray(level, "lights", lights.ToArray());
        SetField(level, "missText", S41H_Info(root.transform, "Misses", new Vector2(0f, 2.7f), 3.4f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0f, 0.9f)));
        SetArray(level, "confetti", S41H_Confetti(root.transform));
        return SaveLevel(root, "TheSilentDisco");
    }

    // ---------- 164 The door that lies ----------

    static GameObject BuildS41HDoorThatLies()
    {
        var root = new GameObject("Level_TheDoorThatLies");
        TheDoorThatLiesLevel level = root.AddComponent<TheDoorThatLiesLevel>();
        Art(root.transform, "Hall", "hall_bg", -20, false);
        var doors = new List<Transform>();
        var panels = new List<Transform>();
        var doorAreas = new List<Collider2D>();
        var faces = new List<SpriteRenderer>();
        var sweats = new List<SpriteRenderer>();
        var ways = new List<SpriteRenderer>();
        for (int i = 0; i < 3; i++)
        {
            Transform d = Child(root.transform, "Door" + i, new Vector2(-1.8f + i * 1.8f, -1.4f)).transform;
            ways.Add(Art(d, "Doorway", "s41h_doorway_dark", 1, false).GetComponent<SpriteRenderer>());
            Transform panel = Child(d, "Panel", new Vector2(-0.65f, 0f)).transform;
            Art(panel, "Art", "s41h_door", 2, false, new Vector2(0.65f, 0f));
            panels.Add(panel);
            faces.Add(Art(d, "Face", "s41h_dface_smile", 3, false, new Vector2(0f, 1.5f)).GetComponent<SpriteRenderer>());
            sweats.Add(Art(d, "Sweat", "sweat", 4, false, new Vector2(0.48f, 1.85f)).GetComponent<SpriteRenderer>());
            BoxCollider2D c = d.gameObject.AddComponent<BoxCollider2D>();
            c.size = new Vector2(1.35f, 2.3f);
            c.offset = new Vector2(0f, 1.15f);
            doors.Add(d);
            doorAreas.Add(c);
        }
        string[] names = { "SAFE", "DANGER", "EXIT" };
        Color[] cols = { new Color(0.2f, 0.6f, 0.3f), new Color(0.85f, 0.2f, 0.25f), new Color(0.2f, 0.4f, 0.85f) };
        var plaques = new List<Transform>();
        var plaqueAreas = new List<Collider2D>();
        for (int i = 0; i < 3; i++)
        {
            Transform p = Child(root.transform, "Plaque" + names[i], new Vector2(-1.8f + i * 1.8f, 1.15f)).transform;
            Art(p, "Art", "s41h_plaque", 6, false);
            TextMesh t = Fit(Label(p, new Vector2(0f, 0.01f), 7, names[i], 0.055f), 1.1f, 0.32f);
            t.color = cols[i];
            BoxCollider2D c = p.gameObject.AddComponent<BoxCollider2D>();
            c.size = new Vector2(1.4f, 0.6f);
            plaques.Add(p);
            plaqueAreas.Add(c);
        }
        Transform glove = Art(root.transform, "Glove", "s41h_glove", 9, false);
        Transform mouse = Child(root.transform, "Mouse", new Vector2(0f, -3.4f)).transform;
        mouse.localScale = Vector3.one * 0.6f;
        Art(mouse, "Art", "mouse", 10, false);

        S41H_Goal(root.transform, "OPEN THE REAL EXIT");
        SetArray(level, "doors", doors.ToArray());
        SetArray(level, "panels", panels.ToArray());
        SetArray(level, "plaques", plaques.ToArray());
        SetArray(level, "doorAreas", doorAreas.ToArray());
        SetArray(level, "plaqueAreas", plaqueAreas.ToArray());
        SetArray(level, "faces", faces.ToArray());
        SetArray(level, "sweats", sweats.ToArray());
        SetArray(level, "doorways", ways.ToArray());
        SetField(level, "smileFace", sprites["s41h_dface_smile"]);
        SetField(level, "scaredFace", sprites["s41h_dface_scared"]);
        SetField(level, "smugFace", sprites["s41h_dface_smug"]);
        SetField(level, "litDoorway", sprites["s41h_doorway_lit"]);
        SetField(level, "darkDoorway", sprites["s41h_doorway_dark"]);
        SetField(level, "glove", glove);
        SetField(level, "mouse", mouse);
        SetField(level, "missText", S41H_Info(root.transform, "Misses", new Vector2(0f, -4.5f), 3.2f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0f, 2.3f)));
        SetArray(level, "confetti", S41H_Confetti(root.transform));
        return SaveLevel(root, "TheDoorThatLies");
    }

    // ---------- 165 Invisible button ----------

    static GameObject BuildS41HInvisibleButton()
    {
        var root = new GameObject("Level_InvisibleButton");
        InvisibleButtonLevel level = root.AddComponent<InvisibleButtonLevel>();
        Art(root.transform, "Room", "night_bg", -20, false);
        Transform button = Child(root.transform, "HiddenButton", Vector2.zero).transform;
        SpriteRenderer art = Art(button, "Art", "s41h_hidden_button", 2, false).GetComponent<SpriteRenderer>();
        SpriteRenderer dark = Bar(root.transform, "Dark", Vector2.zero, new Vector2(12f, 16f), new Color(0.02f, 0.02f, 0.05f, 0.97f), 10);
        SpriteRenderer probe = Art(root.transform, "Probe", "glow", 12, false).GetComponent<SpriteRenderer>();
        Transform toggle = S41H_Button(root.transform, "SoundToggle", new Vector2(1.45f, -4.4f), new Vector2(2.2f, 0.55f), new Color(0.35f, 0.3f, 0.55f), "SOUND: OFF", 20);

        S41H_Goal(root.transform, "FIND THE INVISIBLE BUTTON");
        S41H_Info(root.transform, "HowTo", new Vector2(0f, 2.7f), 4.4f, "SLIDE TO FEEL - LIFT, THEN TAP TO PRESS");
        SetField(level, "dark", dark);
        SetField(level, "probe", probe);
        SetField(level, "buttonArt", art);
        SetField(level, "pressedSprite", sprites["s41h_hidden_button_on"]);
        SetField(level, "hiddenButton", button);
        SetField(level, "soundToggle", toggle.GetComponent<Collider2D>());
        SetField(level, "soundText", toggle.GetComponentInChildren<TextMesh>());
        SetField(level, "missText", S41H_Info(root.transform, "Misses", new Vector2(-1.3f, -4.4f), 2.6f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0f, 2.0f)));
        SetArray(level, "confetti", S41H_Confetti(root.transform));
        return SaveLevel(root, "InvisibleButton");
    }

    // ---------- 166 The second key ----------

    static GameObject BuildS41HSecondKey()
    {
        var root = new GameObject("Level_TheSecondKey");
        TheSecondKeyLevel level = root.AddComponent<TheSecondKeyLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);
        Transform mirror = Art(root.transform, "Mirror", "mirror", 2, false, new Vector2(1.2f, 1.05f));
        mirror.localScale = Vector3.one * 0.6f;
        Art(root.transform, "Dresser", "s41h_dresser", 2, false, new Vector2(1.2f, -0.15f));
        Transform spot = Child(root.transform, "DresserSpot", new Vector2(1.2f, 0.02f)).transform;
        Transform refl = Child(root.transform, "Reflection", new Vector2(1.2f, 1.0f)).transform;
        SpriteRenderer reflArt = Art(refl, "Art", "s41h_key", 4, false).GetComponent<SpriteRenderer>();
        reflArt.flipX = true;
        reflArt.color = new Color(0.75f, 0.9f, 1f, 0.65f);
        refl.gameObject.AddComponent<CircleCollider2D>().radius = 0.55f;
        SpriteRenderer sparkle = Art(root.transform, "Sparkle", "sparkle_star", 9, false).GetComponent<SpriteRenderer>();

        Transform chest = Child(root.transform, "Chest", new Vector2(-0.9f, -2.6f)).transform;
        chest.localScale = Vector3.one * 1.2f;
        SpriteRenderer glow = Art(chest, "Glow", "glow", 1, false, new Vector2(0f, 0.3f)).GetComponent<SpriteRenderer>();
        glow.transform.localScale = Vector3.one * 2.2f;
        glow.color = new Color(1f, 0.9f, 0.4f);
        SpriteRenderer chestArt = Art(chest, "Art", "s41h_chest", 3, false).GetComponent<SpriteRenderer>();
        var holes = new List<Transform>();
        foreach (float x in new[] { -0.38f, 0.38f }) holes.Add(Child(chest, "Hole", new Vector2(x, -0.05f)).transform);

        var keys = new List<Transform>();
        var keyAreas = new List<Collider2D>();
        foreach (Vector2 at in new[] { new Vector2(0.1f, -1.5f), new Vector2(1.9f, -1.9f) })
        {
            Transform k = Child(root.transform, "Key", at).transform;
            Art(k, "Art", "s41h_key", 7, false);
            BoxCollider2D c = k.gameObject.AddComponent<BoxCollider2D>();
            c.size = new Vector2(1.1f, 0.7f);
            keys.Add(k);
            keyAreas.Add(c);
        }

        S41H_Goal(root.transform, "OPEN THE CHEST");
        SetArray(level, "keys", keys.ToArray());
        SetArray(level, "holes", holes.ToArray());
        SetArray(level, "keyAreas", keyAreas.ToArray());
        SetField(level, "dresserSpot", spot);
        SetField(level, "chest", chest);
        SetField(level, "reflection", reflArt);
        SetField(level, "chestArt", chestArt);
        SetField(level, "treasureGlow", glow);
        SetField(level, "sparkle", sparkle);
        SetField(level, "reflectionArea", refl.GetComponent<Collider2D>());
        SetField(level, "chestOpenSprite", sprites["s41h_chest_open"]);
        SetField(level, "missText", S41H_Info(root.transform, "Misses", new Vector2(0f, -4.5f), 3.2f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.1f, 1.4f)));
        SetArray(level, "confetti", S41H_Confetti(root.transform));
        return SaveLevel(root, "TheSecondKey");
    }

    // ---------- 167 Shadow thief ----------

    static GameObject BuildS41HShadowThief()
    {
        var root = new GameObject("Level_ShadowThief");
        ShadowThiefLevel level = root.AddComponent<ShadowThiefLevel>();
        Art(root.transform, "Street", "s41h_street_bg", -20, false);
        var lamps = new List<Transform>();
        var areas = new List<Collider2D>();
        var cones = new List<SpriteRenderer>();
        var bulbs = new List<SpriteRenderer>();
        for (int i = 0; i < 5; i++)
        {
            Transform l = Child(root.transform, "Lamp" + i, new Vector2(-2.2f + i * 1.1f, -2.7f)).transform;
            cones.Add(Art(l, "Light", "s41h_lamp_light", 1, false, new Vector2(0f, 2.85f)).GetComponent<SpriteRenderer>());
            Art(l, "Post", "s41h_lamppost", 2, false);
            bulbs.Add(Art(l, "Bulb", "s41h_bulb", 3, false, new Vector2(0f, 2.86f)).GetComponent<SpriteRenderer>());
            BoxCollider2D c = l.gameObject.AddComponent<BoxCollider2D>();
            c.size = new Vector2(1.05f, 3.6f);
            c.offset = new Vector2(0f, 1.6f);
            lamps.Add(l);
            areas.Add(c);
        }
        Transform thief = Child(root.transform, "Thief", new Vector2(-3.6f, -2.85f)).transform;
        Transform shadow = Child(thief, "Shadow", Vector2.zero).transform;
        Art(shadow, "Art", "s41h_thief_shadow", 4, false);
        Transform body = Child(thief, "Body", Vector2.zero).transform;
        body.localScale = Vector3.one * 0.85f;
        SpriteRenderer thiefArt = Art(body, "Art", "mu_burglar", 6, false).GetComponent<SpriteRenderer>();
        SpriteRenderer alert = Art(thief, "Alert", "alert", 7, false, new Vector2(0f, 1.6f)).GetComponent<SpriteRenderer>();

        S41H_Goal(root.transform, "CATCH THE INVISIBLE THIEF");
        S41H_Info(root.transform, "HowTo", new Vector2(0f, 2.7f), 3.6f, "TAP A LAMP TO SWITCH IT ON");
        SetArray(level, "lamps", lamps.ToArray());
        SetArray(level, "lampAreas", areas.ToArray());
        SetArray(level, "cones", cones.ToArray());
        SetArray(level, "bulbs", bulbs.ToArray());
        SetField(level, "thief", thief);
        SetField(level, "shadow", shadow);
        SetField(level, "thiefArt", thiefArt);
        SetField(level, "exclaim", alert);
        SetField(level, "missText", S41H_Info(root.transform, "Misses", new Vector2(0f, -4.5f), 3.2f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.6f, 1.7f)));
        SetArray(level, "confetti", S41H_Confetti(root.transform));
        return SaveLevel(root, "ShadowThief");
    }

    // ---------- 168 The broken ruler ----------

    static GameObject BuildS41HBrokenRuler()
    {
        var root = new GameObject("Level_TheBrokenRuler");
        TheBrokenRulerLevel level = root.AddComponent<TheBrokenRulerLevel>();
        Art(root.transform, "Gorge", "s41h_gorge_bg", -20, false);
        Transform plank = Child(root.transform, "Plank", new Vector2(-2.0f, -1.2f)).transform;
        Art(plank, "Art", "s41h_plank", 3, false);
        Transform handle = Child(root.transform, "Handle", new Vector2(0.5f, -1.2f)).transform;
        Art(handle, "Art", "s41h_handle", 6, false);
        handle.gameObject.AddComponent<CircleCollider2D>().radius = 0.45f;
        Transform inspector = Child(root.transform, "Inspector", new Vector2(-2.6f, -1.1f)).transform;
        inspector.localScale = Vector3.one * 0.5f;
        Art(inspector, "Art", "mouse", 7, false, new Vector2(0f, 0.6f));

        float[] widths = { 1.5f, 1.0f, 1.5f };
        Vector2[] at = { new Vector2(0.6f, 2.1f), new Vector2(-0.4f, 0.8f), new Vector2(-2.4f, 1.6f) };
        string[][] marks = { new[] { "0", "2" }, new[] { "4" }, new[] { "6", "8" } };
        float[][] markX = { new[] { 0.07f, 1.0f }, new[] { 0.5f }, new[] { 0.5f, 1.42f } };
        var pieces = new List<Transform>();
        var pieceAreas = new List<Collider2D>();
        for (int i = 0; i < 3; i++)
        {
            Transform r = Child(root.transform, "Ruler" + i, at[i]).transform;
            Art(r, "Art", "s41h_ruler_" + i, 8 + i, false);
            for (int m = 0; m < marks[i].Length; m++)
                Label(r, new Vector2(markX[i][m], -0.08f), 12, marks[i][m], 0.03f);
            BoxCollider2D c = r.gameObject.AddComponent<BoxCollider2D>();
            c.size = new Vector2(widths[i], 0.55f);
            c.offset = new Vector2(widths[i] / 2f, 0f);
            pieces.Add(r);
            pieceAreas.Add(c);
        }
        Transform test = S41H_Button(root.transform, "Test", new Vector2(0f, -3.1f), new Vector2(2.3f, 0.65f), new Color(0.95f, 0.5f, 0.2f), "TEST BRIDGE", 14);
        SpriteRenderer stamp = Art(root.transform, "Stamp", "s41h_stamp", 20, false, new Vector2(1.9f, 0.1f)).GetComponent<SpriteRenderer>();

        S41H_Goal(root.transform, "BUILD A BRIDGE EXACTLY 7 UNITS LONG");
        S41H_Info(root.transform, "HowTo", new Vector2(0f, 2.7f), 3.8f, "DRAG THE ORANGE KNOB TO STRETCH");
        SetArray(level, "pieces", pieces.ToArray());
        SetArray(level, "pieceAreas", pieceAreas.ToArray());
        SetField(level, "plank", plank);
        SetField(level, "handle", handle);
        SetField(level, "inspector", inspector);
        SetField(level, "testButton", test);
        SetField(level, "handleArea", handle.GetComponent<Collider2D>());
        SetField(level, "testArea", test.GetComponent<Collider2D>());
        SetField(level, "stamp", stamp);
        SetField(level, "missText", S41H_Info(root.transform, "Misses", new Vector2(0f, -4.5f), 3.2f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.3f, -0.2f)));
        SetArray(level, "confetti", S41H_Confetti(root.transform));
        return SaveLevel(root, "TheBrokenRuler");
    }

    // ---------- 169 The backward song ----------

    static GameObject BuildS41HBackwardSong()
    {
        var root = new GameObject("Level_TheBackwardSong");
        TheBackwardSongLevel level = root.AddComponent<TheBackwardSongLevel>();
        Art(root.transform, "Park", "park_bg", -20, false);
        Art(root.transform, "Branch", "s41h_branch", 2, false, new Vector2(-0.6f, 1.05f));
        Transform bird = Child(root.transform, "Bird", new Vector2(-0.6f, 1.55f)).transform;
        Art(bird, "Art", "s41h_bird", 3, false);
        bird.gameObject.AddComponent<CircleCollider2D>().radius = 0.65f;
        Color[] padCol = { new Color(1f, 0.42f, 0.42f), new Color(1f, 0.85f, 0.3f), new Color(0.45f, 0.85f, 0.4f), new Color(0.4f, 0.65f, 1f) };
        var pads = new List<Transform>();
        var areas = new List<Collider2D>();
        var glows = new List<SpriteRenderer>();
        for (int i = 0; i < 4; i++)
        {
            Vector2 at = new Vector2(-1.95f + i * 1.3f, -2.9f);
            SpriteRenderer g = Art(root.transform, "Glow" + i, "glow", 4, false, at).GetComponent<SpriteRenderer>();
            g.transform.localScale = Vector3.one * 1.8f;
            g.color = padCol[i];
            glows.Add(g);
            Transform p = Child(root.transform, "Pad" + i, at).transform;
            p.localScale = Vector3.one;
            Transform a = Art(p, "Art", "s41h_pad", 5, false);
            a.localScale = Vector3.one * 1.1f;
            a.GetComponent<SpriteRenderer>().color = padCol[i];
            p.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.2f, 1.2f);
            pads.Add(p);
            areas.Add(p.GetComponent<Collider2D>());
        }
        Transform note = Child(root.transform, "NoteFx", Vector2.zero).transform;
        note.localScale = Vector3.one * 1.8f;
        SpriteRenderer noteArt = Art(note, "Art", "note", 8, false).GetComponent<SpriteRenderer>();
        var dots = new List<SpriteRenderer>();
        for (int i = 0; i < 4; i++)
        {
            SpriteRenderer d = Art(root.transform, "Dot" + i, "s41h_bulb", 6, false, new Vector2(-0.75f + i * 0.5f, -4.0f)).GetComponent<SpriteRenderer>();
            d.transform.localScale = Vector3.one * 1.3f;
            dots.Add(d);
        }

        S41H_Goal(root.transform, "HELP THE BIRD SING ITS SONG");
        SetField(level, "bird", bird);
        SetField(level, "noteFx", note);
        SetField(level, "birdArea", bird.GetComponent<Collider2D>());
        SetArray(level, "pads", pads.ToArray());
        SetArray(level, "padAreas", areas.ToArray());
        SetArray(level, "padGlows", glows.ToArray());
        SetArray(level, "inputDots", dots.ToArray());
        SetField(level, "noteArt", noteArt);
        SetField(level, "missText", S41H_Info(root.transform, "Misses", new Vector2(0f, -4.6f), 3.2f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.0f, 2.25f)));
        SetArray(level, "confetti", S41H_Confetti(root.transform));
        return SaveLevel(root, "TheBackwardSong");
    }

    // ---------- 170 Don't blink ----------

    static GameObject BuildS41HDontBlink()
    {
        var root = new GameObject("Level_DontBlink");
        DontBlinkLevel level = root.AddComponent<DontBlinkLevel>();
        Art(root.transform, "Hall", "haunt_hall_bg", -20, false);
        Transform statue = Child(root.transform, "Statue", new Vector2(-0.2f, -0.3f)).transform;
        SpriteRenderer statueArt = Art(statue, "Art", "s41h_statue_shy", 3, false).GetComponent<SpriteRenderer>();
        Transform key = Child(root.transform, "Key", new Vector2(0.75f, -0.15f)).transform;
        Art(key, "Art", "s41h_key", 8, false);
        BoxCollider2D keyCol = key.gameObject.AddComponent<BoxCollider2D>();
        keyCol.size = new Vector2(1.1f, 0.75f);

        Transform door = Child(root.transform, "Door", new Vector2(1.75f, -4.3f)).transform;
        door.localScale = Vector3.one * 0.75f;
        SpriteRenderer doorway = Art(door, "Doorway", "s41h_doorway_lit", 1, false).GetComponent<SpriteRenderer>();
        SpriteRenderer panel = Art(door, "Panel", "s41h_lockdoor", 2, false).GetComponent<SpriteRenderer>();
        Transform lockSpot = Child(door, "Lock", new Vector2(0f, 1.15f)).transform;

        Transform eye = Child(root.transform, "Eye", new Vector2(-1.6f, -3.45f)).transform;
        SpriteRenderer eyeArt = Art(eye, "Art", "s41h_eye_closed", 10, false).GetComponent<SpriteRenderer>();
        eye.gameObject.AddComponent<CircleCollider2D>().radius = 0.85f;
        S41H_Info(root.transform, "EyeLabel", new Vector2(-1.6f, -4.5f), 2.2f, "HOLD TO WATCH");
        SpriteRenderer blackout = Bar(root.transform, "Blackout", Vector2.zero, new Vector2(12f, 16f), Color.black, 60);

        S41H_Goal(root.transform, "TAKE THE KEY TO THE DOOR");
        SetField(level, "statue", statue);
        SetField(level, "key", key);
        SetField(level, "eyeButton", eye);
        SetField(level, "lockSpot", lockSpot);
        SetField(level, "statueArt", statueArt);
        SetField(level, "eyeArt", eyeArt);
        SetField(level, "blackout", blackout);
        SetField(level, "doorway", doorway);
        SetField(level, "doorPanel", panel);
        SetField(level, "shySprite", sprites["s41h_statue_shy"]);
        SetField(level, "peekSprite", sprites["s41h_statue_peek"]);
        SetField(level, "eyeOpen", sprites["s41h_eye_open"]);
        SetField(level, "eyeClosed", sprites["s41h_eye_closed"]);
        SetField(level, "eyeArea", eye.GetComponent<Collider2D>());
        SetField(level, "keyArea", keyCol);
        SetField(level, "stepText", S41H_Info(root.transform, "Steps", new Vector2(0f, 2.7f), 3.0f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.9f, 1.7f)));
        SetArray(level, "confetti", S41H_Confetti(root.transform));
        return SaveLevel(root, "DontBlink");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V41H = new Dictionary<string, string[]>
    {
        { "the_silent_disco", new[] { "Follow his timing.", "Tap the floor to match the dancer in blue." } },
        { "the_door_that_lies", new[] { "Their reactions reveal something.", "Move the labels and watch which door reacts differently." } },
        { "invisible_button", new[] { "Feel the vibrations.", "Slide your finger until vibration intensity peaks, then tap." } },
        { "the_second_key", new[] { "A mirror can make two.", "Reflect the key, then tap its reflection." } },
        { "shadow_thief", new[] { "Follow the moving shadow.", "Illuminate the lamp the shadow is approaching." } },
        { "the_broken_ruler", new[] { "Match the broken markings.", "Reassemble it, then set the bridge to seven units." } },
        { "the_backward_song", new[] { "Start from the last note.", "Play the four notes in reverse order." } },
        { "dont_blink", new[] { "Keep watching while taking the key.", "Maintain eye contact or hold the watch button while dragging the key." } },
    };
}
#endif
