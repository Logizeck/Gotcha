#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli della specifica 105-200 (in gioco dal 151), pacchetto J.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels41J() => new List<LevelDefinition>
    {
        Define("TheAngryKeyboard", "the_angry_keyboard", "THE ANGRY KEYBOARD", "Typing isn't working.", BuildS41JAngryKeyboard()),
        Define("TheCowardlyDragon", "the_cowardly_dragon", "THE COWARDLY DRAGON", "He needs a scare.", BuildS41JCowardlyDragon()),
        Define("HiddenFrequency", "hidden_frequency", "HIDDEN FREQUENCY", "The lock reacts to sound.", BuildS41JHiddenFrequency()),
        Define("TheCrookedBridge", "the_crooked_bridge", "THE CROOKED BRIDGE", "The bridge is badly aligned.", BuildS41JCrookedBridge()),
        Define("TheFinalWrongAnswer", "the_final_wrong_answer", "THE FINAL WRONG ANSWER", "The correct answer may not be a door.", BuildS41JFinalWrongAnswer()),
        Define("TheImpatientClock", "the_impatient_clock", "THE IMPATIENT CLOCK", "The hands won't stay still.", BuildS41JImpatientClock()),
        Define("TheFrozenWaterfall", "the_frozen_waterfall", "THE FROZEN WATERFALL", "Sunlight can melt ice.", BuildS41JFrozenWaterfall()),
        Define("TheFakeCheckpoint", "the_fake_checkpoint", "THE FAKE CHECKPOINT", "The checkpoint isn't where it should be.", BuildS41JFakeCheckpoint()),
    };

    // ---------- Aiuti comuni del pacchetto J ----------

    /// <summary>Riga dell'obiettivo in alto, su una fascia scura.</summary>
    static TextMesh S41J_Goal(Transform root, string text)
    {
        Transform h = Child(root, "Goal", new Vector2(0f, 3.25f)).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(5.5f, 0.5f), new Color(0.12f, 0.09f, 0.16f, 0.72f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, text, 0.05f);
        t.color = Color.white;
        Fit(t, 5.3f, 0.42f);
        return t;
    }

    /// <summary>Etichetta bianca su fascia scura (contatori).</summary>
    static TextMesh S41J_Info(Transform parent, string name, Vector2 pos, float width, int order = 70)
    {
        Transform h = Child(parent, name, pos).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(width, 0.4f), new Color(0.12f, 0.09f, 0.16f, 0.65f), order);
        TextMesh t = Label(h, Vector2.zero, order + 1, "", 0.04f);
        t.color = Color.white;
        Fit(t, width - 0.2f, 0.32f);
        return t;
    }

    static SmokeEmitter[] S41J_Confetti(Transform root)
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

    /// <summary>Contenitore scalato.</summary>
    static Transform S41J_Holder(Transform parent, string name, Vector2 pos, float scale)
    {
        Transform t = Child(parent, name, pos).transform;
        t.localScale = Vector3.one * scale;
        return t;
    }

    /// <summary>Topolino (piedi nell'origine del Transform restituito).</summary>
    static Transform S41J_Mouse(Transform parent, Vector2 pos, float scale, int order)
    {
        Transform m = Child(parent, "Mouse", pos).transform;
        Transform body = S41J_Holder(m, "Body", Vector2.zero, scale);
        Art(body, "Art", "mouse", order, false, new Vector2(0f, 0.56f));
        return m;
    }

    /// <summary>Pulsante a pillola con scritta e collider.</summary>
    static Transform S41J_Button(Transform parent, string name, Vector2 pos, string text, Color tint, int order, out Collider2D area)
    {
        Transform b = Child(parent, name, pos).transform;
        Art(b, "Art", "s41j_pill", order, false).GetComponent<SpriteRenderer>().color = tint;
        Fit(Label(b, new Vector2(0f, 0.02f), order + 1, text, 0.06f), 1.4f, 0.42f);
        BoxCollider2D c = b.gameObject.AddComponent<BoxCollider2D>();
        c.size = new Vector2(1.9f, 0.9f);
        area = c;
        return b;
    }

    static void S41J_Common(Component level, Transform root, Vector2 bubbleAt)
    {
        SetField(level, "bubble", SmallBubble(root, bubbleAt));
        SetArray(level, "confetti", S41J_Confetti(root));
    }

    // ---------- 180 The angry keyboard ----------

    static GameObject BuildS41JAngryKeyboard()
    {
        var root = new GameObject("Level_TheAngryKeyboard");
        TheAngryKeyboardLevel level = root.AddComponent<TheAngryKeyboardLevel>();
        Art(root.transform, "Office", "s41j_kbd_bg", -20, false);
        Art(root.transform, "Monitor", "s41j_kbd_monitor", 1, false, new Vector2(0f, 1.1f));
        SpriteRenderer glow = Bar(root.transform, "ScreenGlow", new Vector2(0f, 1.12f), new Vector2(4.4f, 2.1f), new Color(0f, 0f, 0f, 0f), 2);
        TextMesh screen = Fit(Label(root.transform, new Vector2(0f, 1.75f), 4, "PASSWORD?", 0.07f), 4.0f, 0.5f);
        screen.color = new Color(0.5f, 1f, 0.6f);
        var slots = new List<Transform>();
        for (int i = 0; i < 4; i++)
        {
            Transform s = Child(root.transform, "Slot" + i, new Vector2(-1.35f + 0.9f * i, 0.72f)).transform;
            Art(s, "Art", "s41j_kbd_slot", 3, false);
            slots.Add(s);
        }
        BoxCollider2D answer = Child(root.transform, "AnswerBox", new Vector2(0f, 0.72f)).AddComponent<BoxCollider2D>();
        answer.size = new Vector2(4.2f, 1.5f);

        Transform kb = Child(root.transform, "Keyboard", new Vector2(0f, -2.8f)).transform;
        Art(kb, "Base", "s41j_kbd_base", 5, false);
        SpriteRenderer face = Art(kb, "Face", "s41j_kbd_face_angry", 6, false, new Vector2(0f, 1.7f)).GetComponent<SpriteRenderer>();
        string[] rows = { "QWERTY", "UIOPAS", "DFGHJK", "LZXCVN" };
        var keys = new List<Transform>();
        var letters = new List<TextMesh>();
        for (int r = 0; r < rows.Length; r++)
            for (int c = 0; c < rows[r].Length; c++)
            {
                Transform k = Child(kb, "Key" + rows[r][c], new Vector2(-2.15f + 0.86f * c, 1.05f - 0.85f * r)).transform;
                Art(k, "Art", "s41j_kbd_key", 20, false);
                k.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(0.84f, 0.84f);
                TextMesh l = Label(k, new Vector2(0f, 0.04f), 22, rows[r][c].ToString(), 0.08f);
                keys.Add(k);
                letters.Add(l);
            }
        SmokeEmitter boom = Emitter(root.transform, "Boom", new Vector2(0f, -2.6f), "puff", 30f, 1.0f, 0.8f, 1.8f, 0.9f, new Color(0.75f, 0.7f, 0.8f, 0.9f), 50);

        S41J_Goal(root.transform, "TYPE OPEN");
        SetArray(level, "keys", keys.ToArray());
        SetArray(level, "keyLetters", letters.ToArray());
        SetField(level, "keyboard", kb);
        SetField(level, "faceArt", face);
        SetField(level, "defeatedFace", sprites["s41j_kbd_face_defeated"]);
        SetField(level, "answerArea", answer);
        SetArray(level, "slots", slots.ToArray());
        SetField(level, "screenText", screen);
        SetField(level, "pressText", S41J_Info(root.transform, "Presses", new Vector2(0f, 2.72f), 3.0f));
        SetField(level, "screenGlow", glow);
        SetField(level, "boomSmoke", boom);
        S41J_Common(level, root.transform, new Vector2(1.4f, -0.35f));
        return SaveLevel(root, "TheAngryKeyboard");
    }

    // ---------- 181 The cowardly dragon ----------

    static GameObject BuildS41JCowardlyDragon()
    {
        var root = new GameObject("Level_TheCowardlyDragon");
        TheCowardlyDragonLevel level = root.AddComponent<TheCowardlyDragonLevel>();
        Art(root.transform, "Castle", "s41j_dragon_bg", -20, false);
        Transform shadow = Child(root.transform, "Shadow", new Vector2(-1.5f, -2.5f)).transform;
        SpriteRenderer shadowArt = Art(shadow, "Art", "s41j_dragon_shadow", -10, false).GetComponent<SpriteRenderer>();
        shadowArt.color = new Color(1f, 1f, 1f, 0.6f);
        Transform sun = Child(root.transform, "Sun", new Vector2(1.8f, 2.35f)).transform;
        Transform sunGlow = Art(sun, "Glow", "glow", 2, false);
        sunGlow.localScale = Vector3.one * 2.6f;
        sunGlow.GetComponent<SpriteRenderer>().color = new Color(1f, 0.9f, 0.5f, 0.5f);
        Art(sun, "Art", "sun", 3, false);
        sun.gameObject.AddComponent<CircleCollider2D>().radius = 0.9f;
        Transform barricade = Child(root.transform, "Barricade", new Vector2(1.9f, -2.6f)).transform;
        SpriteRenderer barArt = Art(barricade, "Art", "s41j_barricade", 2, false).GetComponent<SpriteRenderer>();
        SmokeEmitter flames = Emitter(root.transform, "Flames", new Vector2(1.9f, -1.8f), "flame", 18f, 0.8f, 1.3f, 0.8f, 2.4f, new Color(1f, 0.75f, 0.4f, 1f), 9);

        Transform dragon = Child(root.transform, "Dragon", new Vector2(-0.4f, -2.6f)).transform;
        SpriteRenderer dragonArt = Art(dragon, "Art", "s41j_dragon", 5, false).GetComponent<SpriteRenderer>();
        BoxCollider2D dc = dragon.gameObject.AddComponent<BoxCollider2D>();
        dc.size = new Vector2(2.0f, 2.0f);
        dc.offset = new Vector2(0f, 1.0f);
        Transform mouth = Child(dragon, "Mouth", new Vector2(0.95f, 1.35f)).transform;
        Transform fire = Child(root.transform, "Fire", Vector2.zero).transform;
        Art(fire, "Art", "s41j_fire_breath", 8, false);
        SmokeEmitter cough = Emitter(root.transform, "Cough", new Vector2(0.55f, -1.25f), "puff", 10f, 0.8f, 0.6f, 0.2f, 0.4f, new Color(0.6f, 0.6f, 0.65f, 0.9f), 9);
        S41J_Button(root.transform, "Blow", new Vector2(0f, -4.1f), "BLOW FIRE!", new Color(1f, 0.7f, 0.45f), 20, out Collider2D blowArea);

        S41J_Goal(root.transform, "BURN THE BARRICADE");
        SetField(level, "dragon", dragon);
        SetField(level, "dragonArt", dragonArt);
        SetField(level, "idleSprite", sprites["s41j_dragon"]);
        SetField(level, "scaredSprite", sprites["s41j_dragon_scared"]);
        SetField(level, "dragonArea", dc);
        SetField(level, "sun", sun);
        SetField(level, "sunArea", sun.GetComponent<Collider2D>());
        SetField(level, "shadow", shadow);
        SetField(level, "shadowArt", shadowArt);
        SetField(level, "barricade", barricade);
        SetField(level, "barricadeArt", barArt);
        SetField(level, "fire", fire);
        SetField(level, "mouth", mouth);
        SetField(level, "flames", flames);
        SetField(level, "cough", cough);
        SetField(level, "blowArea", blowArea);
        SetField(level, "triesText", S41J_Info(root.transform, "Tries", new Vector2(0f, -3.35f), 3.4f));
        S41J_Common(level, root.transform, new Vector2(-1.3f, 0.2f));
        return SaveLevel(root, "TheCowardlyDragon");
    }

    // ---------- 182 Hidden frequency ----------

    static GameObject BuildS41JHiddenFrequency()
    {
        var root = new GameObject("Level_HiddenFrequency");
        HiddenFrequencyLevel level = root.AddComponent<HiddenFrequencyLevel>();
        Art(root.transform, "Vault", "s41j_freq_bg", -20, false);
        SpriteRenderer safe = Art(root.transform, "Safe", "s41j_freq_safe", 1, false, new Vector2(0f, -0.3f)).GetComponent<SpriteRenderer>();
        var lights = new List<SpriteRenderer>();
        foreach (float x in new[] { -1.1f, 0f, 1.1f })
            lights.Add(Art(root.transform, "Light", "s41j_freq_light", 3, false, new Vector2(x, 1.55f)).GetComponent<SpriteRenderer>());
        GameObject door = Child(root.transform, "DoorParts", Vector2.zero);
        SpriteRenderer wig = Art(door.transform, "Wiggles", "s41j_freq_wiggle", 3, false, new Vector2(0f, 0.25f)).GetComponent<SpriteRenderer>();
        Transform plate = Child(door.transform, "Lock", new Vector2(0f, 0.25f)).transform;
        Art(plate, "Art", "s41j_freq_lock", 4, false);
        Transform gauge = GaugeBar(door.transform, "Vibration", new Vector2(0f, -0.35f), new Vector2(2.6f, 0.16f), new Color(0.15f, 0.15f, 0.22f, 0.9f), new Color(1f, 0.85f, 0.3f), 4);
        Transform dial = Child(door.transform, "Dial", new Vector2(0f, -1.55f)).transform;
        Art(dial, "Art", "s41j_freq_dial", 4, false);
        dial.gameObject.AddComponent<CircleCollider2D>().radius = 1.0f;
        S41J_Button(root.transform, "Open", new Vector2(0f, -3.85f), "OPEN", new Color(0.6f, 1f, 0.65f), 20, out Collider2D openArea);

        S41J_Goal(root.transform, "OPEN THE SAFE");
        SetField(level, "dial", dial);
        SetField(level, "dialArea", dial.GetComponent<Collider2D>());
        SetArray(level, "lights", lights.ToArray());
        SetField(level, "lockPlate", plate);
        SetField(level, "wiggles", wig);
        SetField(level, "vibrationGauge", gauge);
        SetField(level, "openArea", openArea);
        SetField(level, "safeArt", safe);
        SetField(level, "safeOpen", sprites["s41j_freq_safe_open"]);
        SetField(level, "doorParts", door);
        SetField(level, "timeText", S41J_Info(root.transform, "Time", new Vector2(-1.4f, 2.6f), 2.6f));
        SetField(level, "triesText", S41J_Info(root.transform, "Tries", new Vector2(1.4f, 2.6f), 2.6f));
        S41J_Common(level, root.transform, new Vector2(-1.5f, 0.95f));
        return SaveLevel(root, "HiddenFrequency");
    }

    // ---------- 183 The crooked bridge ----------

    static GameObject BuildS41JCrookedBridge()
    {
        var root = new GameObject("Level_TheCrookedBridge");
        TheCrookedBridgeLevel level = root.AddComponent<TheCrookedBridgeLevel>();
        Art(root.transform, "Chasm", "s41j_chasm_bg", -20, false);
        Transform cheese = Child(root.transform, "Cheese", new Vector2(2.35f, -1.0f)).transform;
        Art(cheese, "Art", "cheese_wedge", 3, false);
        Transform plank = Child(root.transform, "Plank", Vector2.zero).transform;
        SpriteRenderer plankArt = Art(plank, "Art", "s41j_bridge_plank", 4, false).GetComponent<SpriteRenderer>();
        Transform left = Child(root.transform, "LeftEnd", Vector2.zero).transform;
        Art(left, "Art", "s41j_bridge_knob", 6, false);
        Transform right = Child(root.transform, "RightEnd", Vector2.zero).transform;
        Art(right, "Art", "s41j_bridge_knob", 6, false);
        Transform mouse = S41J_Mouse(root.transform, new Vector2(-2.3f, -1.0f), 0.6f, 7);
        BoxCollider2D mc = mouse.gameObject.AddComponent<BoxCollider2D>();
        mc.size = new Vector2(1.0f, 1.0f);
        mc.offset = new Vector2(0f, 0.4f);
        TextMesh help = Fit(Label(root.transform, new Vector2(0f, -4.3f), 8, "DRAG THE BRIDGE ENDS - TAP THE MOUSE TO CROSS", 0.04f), 5.2f, 0.3f);
        help.color = new Color(1f, 0.95f, 0.8f);

        S41J_Goal(root.transform, "GET THE MOUSE ACROSS");
        SetField(level, "plank", plank);
        SetField(level, "plankArt", plankArt);
        SetField(level, "leftKnob", left);
        SetField(level, "rightKnob", right);
        SetField(level, "mouse", mouse);
        SetField(level, "mouseArea", mc);
        SetField(level, "cheese", cheese);
        SetField(level, "fallsText", S41J_Info(root.transform, "Falls", new Vector2(0f, 2.6f), 2.6f));
        S41J_Common(level, root.transform, new Vector2(-1.4f, 0.6f));
        return SaveLevel(root, "TheCrookedBridge");
    }

    // ---------- 184 The final wrong answer ----------

    static GameObject BuildS41JFinalWrongAnswer()
    {
        var root = new GameObject("Level_TheFinalWrongAnswer");
        TheFinalWrongAnswerLevel level = root.AddComponent<TheFinalWrongAnswerLevel>();
        Art(root.transform, "Studio", "s41j_quiz_bg", -20, false);
        Transform hole = Child(root.transform, "Hole", new Vector2(0f, 1.75f)).transform;
        Art(hole, "Art", "s41j_wall_hole", 1, false);
        Transform q = Child(root.transform, "Question", new Vector2(0f, 1.75f)).transform;
        Art(q, "Art", "s41j_quiz_sign", 5, false);
        TextMesh qt = Fit(Label(q, Vector2.zero, 6, "WHAT IS 2 + 2?", 0.08f), 3.6f, 0.7f);
        qt.color = new Color(1f, 0.9f, 0.35f);
        BoxCollider2D qc = q.gameObject.AddComponent<BoxCollider2D>();
        qc.size = new Vector2(4.2f, 1.3f);

        string[] answers = { "3", "4", "FISH" };
        var doors = new List<Transform>();
        var arts = new List<SpriteRenderer>();
        var areas = new List<Collider2D>();
        var surprises = new List<Object>();
        for (int i = 0; i < 3; i++)
        {
            Transform d = Child(root.transform, "Door" + i, new Vector2(-1.75f + 1.75f * i, -2.4f)).transform;
            arts.Add(Art(d, "Art", "s41j_quiz_door", 2, false).GetComponent<SpriteRenderer>());
            Fit(Label(d, new Vector2(0f, 1.6f), 3, answers[i], 0.08f), 0.7f, 0.55f);
            BoxCollider2D c = d.gameObject.AddComponent<BoxCollider2D>();
            c.size = new Vector2(1.3f, 2.3f);
            c.offset = new Vector2(0f, 1.15f);
            string sprite = i == 0 ? "s41j_quiz_x" : i == 1 ? "zzz" : "fish";
            Transform s = Art(d, "Surprise", sprite, 3, false, new Vector2(0f, 0.9f));
            if (i == 1) s.localScale = Vector3.one * 1.8f;
            doors.Add(d);
            areas.Add(c);
            surprises.Add(s.gameObject);
        }
        Transform mouse = S41J_Mouse(root.transform, new Vector2(-2.2f, -3.7f), 0.7f, 8);
        SpriteRenderer buzzer = Bar(root.transform, "Buzzer", Vector2.zero, new Vector2(6.4f, 10.4f), new Color(1f, 1f, 1f, 0f), 40);

        S41J_Goal(root.transform, "ANSWER THE FINAL QUESTION");
        SetField(level, "question", q);
        SetField(level, "questionArea", qc);
        SetField(level, "hole", hole);
        SetArray(level, "doors", doors.ToArray());
        SetArray(level, "doorArts", arts.ToArray());
        SetArray(level, "doorAreas", areas.ToArray());
        SetField(level, "doorClosed", sprites["s41j_quiz_door"]);
        SetField(level, "doorOpen", sprites["s41j_quiz_door_open"]);
        SetArray(level, "doorSurprises", surprises.ToArray());
        SetField(level, "mouse", mouse);
        SetField(level, "triesText", S41J_Info(root.transform, "Tries", new Vector2(0.6f, -4.35f), 3.2f));
        SetField(level, "buzzerLight", buzzer);
        S41J_Common(level, root.transform, new Vector2(-1.2f, 0.45f));
        return SaveLevel(root, "TheFinalWrongAnswer");
    }

    // ---------- 185 The impatient clock ----------

    static GameObject BuildS41JImpatientClock()
    {
        var root = new GameObject("Level_TheImpatientClock");
        TheImpatientClockLevel level = root.AddComponent<TheImpatientClockLevel>();
        Art(root.transform, "Wall", "s41j_clock_bg", -20, false);
        Transform body = Child(root.transform, "Clock", new Vector2(0f, 0.2f)).transform;
        Art(body, "Art", "s41j_clock_body", 1, false);
        SpriteRenderer face = Art(body, "Face", "s41j_clock_face_angry", 2, false).GetComponent<SpriteRenderer>();
        Transform glow = Art(body, "TwelveGlow", "glow", 2, false, new Vector2(0f, 1.55f));
        glow.localScale = Vector3.one * 1.1f;
        SpriteRenderer glowArt = glow.GetComponent<SpriteRenderer>();
        glowArt.color = new Color(0.4f, 1f, 0.45f, 0f);
        TextMesh twelve = Fit(Label(body, new Vector2(0f, 1.3f), 3, "12", 0.07f), 0.6f, 0.4f);
        twelve.color = new Color(0.85f, 0.2f, 0.2f);
        Transform center = Child(body, "Center", Vector2.zero).transform;
        Transform hour = Child(body, "HourHand", Vector2.zero).transform;
        Art(hour, "Art", "s41j_clock_hour", 5, false);
        Transform minute = Child(body, "MinuteHand", Vector2.zero).transform;
        Art(minute, "Art", "s41j_clock_minute", 6, false);
        TextMesh help = Fit(Label(root.transform, new Vector2(0f, -3.0f), 8, "DRAG THE HANDS", 0.045f), 3.0f, 0.3f);
        help.color = new Color(0.35f, 0.2f, 0.15f);

        S41J_Goal(root.transform, "SET THE CLOCK TO 12:00");
        SetField(level, "center", center);
        SetField(level, "hourHand", hour);
        SetField(level, "minuteHand", minute);
        SetField(level, "faceArt", face);
        SetField(level, "happyFace", sprites["s41j_clock_face_happy"]);
        SetField(level, "clockBody", body);
        SetField(level, "twelveGlow", glowArt);
        SetField(level, "triesText", S41J_Info(root.transform, "Tries", new Vector2(0f, -3.9f), 3.0f));
        S41J_Common(level, root.transform, new Vector2(1.3f, -2.6f));
        return SaveLevel(root, "TheImpatientClock");
    }

    // ---------- 186 The frozen waterfall ----------

    static GameObject BuildS41JFrozenWaterfall()
    {
        var root = new GameObject("Level_TheFrozenWaterfall");
        TheFrozenWaterfallLevel level = root.AddComponent<TheFrozenWaterfallLevel>();
        Art(root.transform, "Mountain", "s41j_falls_bg", -20, false);
        SpriteRenderer water = Art(root.transform, "Water", "s41j_water_fall", 1, false, new Vector2(1.85f, -0.6f)).GetComponent<SpriteRenderer>();
        SpriteRenderer ice = Art(root.transform, "Ice", "s41j_ice_fall", 2, false, new Vector2(1.85f, -0.6f)).GetComponent<SpriteRenderer>();
        BoxCollider2D iceArea = Child(root.transform, "IceArea", new Vector2(1.85f, -0.6f)).AddComponent<BoxCollider2D>();
        iceArea.size = new Vector2(1.3f, 4.9f);
        Transform key = S41J_Holder(root.transform, "Key", new Vector2(2.15f, -1.6f), 0.45f);
        Art(key, "Art", "key", 3, false);

        Transform sun = Child(root.transform, "Sun", new Vector2(-1.9f, 2.35f)).transform;
        Transform sg = Art(sun, "Glow", "glow", 1, false);
        sg.localScale = Vector3.one * 3.6f;
        sg.GetComponent<SpriteRenderer>().color = new Color(1f, 0.9f, 0.5f, 0.45f);
        Art(sun, "Art", "sun", 2, false);

        LineRenderer sunlight = Line(root.transform, "Sunlight", 0.35f, new Color(1f, 0.95f, 0.6f, 0.35f), 3);
        LineRenderer beam = Line(root.transform, "Beam", 0.08f, new Color(1f, 0.8f, 0.3f, 0.95f), 5);
        SpriteRenderer hot = Art(root.transform, "HotSpot", "glow", 5, false).GetComponent<SpriteRenderer>();
        hot.transform.localScale = Vector3.one * 0.8f;
        hot.color = new Color(1f, 0.6f, 0.2f, 0f);
        SmokeEmitter steam = Emitter(root.transform, "Steam", Vector2.zero, "puff", 12f, 0.9f, 0.8f, 0.25f, 0.35f, new Color(1f, 1f, 1f, 0.85f), 8);

        Transform lens = Child(root.transform, "Lens", new Vector2(0.2f, -2.5f)).transform;
        Art(lens, "Art", "s41j_lens", 6, false);
        lens.gameObject.AddComponent<CircleCollider2D>().radius = 0.5f;
        lens.localRotation = Quaternion.Euler(0f, 0f, 120f);
        Transform aim = Child(lens, "Aim", new Vector2(0.85f, 0f)).transform;
        Art(aim, "Art", "s41j_lens_aim", 7, false);
        aim.gameObject.AddComponent<CircleCollider2D>().radius = 0.4f;

        Transform mouse = S41J_Mouse(root.transform, new Vector2(-1.9f, -3.3f), 0.65f, 6);
        Transform gauge = GaugeBar(root.transform, "Melt", new Vector2(0.4f, -4.1f), new Vector2(3.0f, 0.18f), new Color(0.15f, 0.15f, 0.22f, 0.8f), new Color(1f, 0.6f, 0.25f), 20);
        TextMesh ml = Fit(Label(root.transform, new Vector2(-1.75f, -4.1f), 21, "MELT", 0.045f), 0.9f, 0.3f);
        ml.color = new Color(0.2f, 0.25f, 0.4f);

        S41J_Goal(root.transform, "FREE THE KEY FROM THE ICE");
        SetField(level, "sun", sun);
        SetField(level, "lens", lens);
        SetField(level, "lensArea", lens.GetComponent<Collider2D>());
        SetField(level, "aimArea", aim.GetComponent<Collider2D>());
        SetField(level, "sunlight", sunlight);
        SetField(level, "beam", beam);
        SetField(level, "hotSpot", hot);
        SetField(level, "iceArea", iceArea);
        SetField(level, "iceArt", ice);
        SetField(level, "waterArt", water);
        SetField(level, "steam", steam);
        SetField(level, "meltGauge", gauge);
        SetField(level, "key", key);
        SetField(level, "mouse", mouse);
        SetField(level, "triesText", S41J_Info(root.transform, "Tries", new Vector2(1.0f, 2.6f), 2.6f));
        S41J_Common(level, root.transform, new Vector2(-1.0f, -1.5f));
        return SaveLevel(root, "TheFrozenWaterfall");
    }

    // ---------- 187 The fake checkpoint ----------

    static GameObject BuildS41JFakeCheckpoint()
    {
        var root = new GameObject("Level_TheFakeCheckpoint");
        TheFakeCheckpointLevel level = root.AddComponent<TheFakeCheckpointLevel>();
        Art(root.transform, "World", "s41j_cp_bg", -20, false);
        Art(root.transform, "Pole", "s41j_cp_pole", 2, false, new Vector2(0f, -2.05f));
        Transform home = Child(root.transform, "FlagHome", new Vector2(0f, -0.25f)).transform;
        Transform door = Child(root.transform, "Door", new Vector2(2.2f, -2.05f)).transform;
        SpriteRenderer doorArt = Art(door, "Art", "s41j_cp_door", 2, false).GetComponent<SpriteRenderer>();
        Transform spot = Child(root.transform, "DoorSpot", new Vector2(1.75f, 0.35f)).transform;
        Transform flag = Child(root.transform, "Flag", new Vector2(0f, -0.25f)).transform;
        Art(flag, "Art", "s41j_cp_flag", 4, false);
        BoxCollider2D fc = flag.gameObject.AddComponent<BoxCollider2D>();
        fc.size = new Vector2(1.2f, 1.0f);
        fc.offset = new Vector2(0.45f, -0.3f);
        Transform hero = Child(root.transform, "Hero", new Vector2(-2.2f, -2.05f)).transform;
        Art(hero, "Art", "s41j_cp_hero", 6, false);
        BoxCollider2D hc = hero.gameObject.AddComponent<BoxCollider2D>();
        hc.size = new Vector2(1.0f, 1.2f);
        hc.offset = new Vector2(0f, 0.5f);
        S41J_Button(root.transform, "Go", new Vector2(0f, -3.6f), "GO!", new Color(0.6f, 1f, 0.6f), 20, out Collider2D goArea);
        SmokeEmitter poof = Emitter(root.transform, "Poof", Vector2.zero, "puff", 25f, 0.6f, 0.4f, 0.5f, 0.6f, new Color(1f, 1f, 1f, 0.9f), 9);
        SpriteRenderer sparkle = Art(root.transform, "Sparkle", "sparkle_star", 8, false).GetComponent<SpriteRenderer>();
        sparkle.transform.localScale = Vector3.one * 2f;
        TextMesh banner = Fit(Label(root.transform, new Vector2(0f, 1.2f), 30, "", 0.12f), 5.0f, 0.8f);
        banner.color = new Color(1f, 0.85f, 0.2f);

        S41J_Goal(root.transform, "REACH THE DOOR");
        SetField(level, "hero", hero);
        SetField(level, "heroArea", hc);
        SetField(level, "goArea", goArea);
        SetField(level, "flag", flag);
        SetField(level, "flagArea", fc);
        SetField(level, "flagHome", home);
        SetField(level, "doorSpot", spot);
        SetField(level, "doorArt", doorArt);
        SetField(level, "doorOpen", sprites["s41j_cp_door_open"]);
        SetField(level, "poof", poof);
        SetField(level, "sparkle", sparkle);
        SetField(level, "banner", banner);
        SetField(level, "triesText", S41J_Info(root.transform, "Tries", new Vector2(0f, 2.6f), 2.6f));
        S41J_Common(level, root.transform, new Vector2(-1.2f, 0.1f));
        return SaveLevel(root, "TheFakeCheckpoint");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V41J = new Dictionary<string, string[]>
    {
        { "the_angry_keyboard", new[] { "The keys themselves can move.", "Drag O, P, E and N into the answer box." } },
        { "the_cowardly_dragon", new[] { "His shadow can frighten him.", "Move the sun to enlarge the dragon's shadow." } },
        { "hidden_frequency", new[] { "Watch the vibration indicator.", "Tune until each of the three lights turns green." } },
        { "the_crooked_bridge", new[] { "Both ends can move.", "Adjust the two ends at the same time." } },
        { "the_final_wrong_answer", new[] { "The question is covering something.", "Move the question itself." } },
        { "the_impatient_clock", new[] { "Hold one while moving the other.", "Use two fingers, then release them together." } },
        { "the_frozen_waterfall", new[] { "The lens can focus light.", "Align the lens and hold the beam on the frozen waterfall." } },
        { "the_fake_checkpoint", new[] { "Try moving the flag.", "Put the checkpoint flag at the exit." } },
    };
}
#endif
