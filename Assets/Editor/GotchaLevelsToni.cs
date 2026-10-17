#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>I livelli di Toni, il carlino: uno ogni 25 (25, 50, 75, 100).</summary>
public static partial class GotchaProjectBuilder
{
    /// <summary>Mette un livello di Toni in ogni posizione multipla di 25; i livelli che erano lì scalano avanti.</summary>
    static void InsertToniLevels(List<LevelDefinition> levels)
    {
        var toni = new[]
        {
            Define("Toni1", "toni_button", "MOVE, TONI!",
                "Press the button. Toni is a very good boy... and very, very heavy. What does a dog love more than lying down?",
                BuildToniButtonLevel()),
            Define("Toni2", "toni_photo", "SAY CHEESE!",
                "Toni only looks at treats. A good pet photographer knows exactly where to hold one.",
                BuildToniPhotoLevel(), "toni_unicorn"),
            Define("Toni3", "toni_fridge", "WAKE UP, TONI!",
                "Alarm clocks don't work on pugs. Which sound makes EVERY dog jump out of bed?",
                BuildToniFridgeLevel(), "toni_toot"),
            Define("Toni4", "toni_cake", "TONI'S BIRTHDAY",
                "Toni has to blow out his candle... but pugs can't blow. Maybe he can do something else with his nose?",
                BuildToniCakeLevel()),
        };
        for (int i = 0; i < toni.Length; i++) levels.Insert(Mathf.Min(levels.Count, 24 + i * 25), toni[i]);
    }

    /// <summary>Toni completo: pose, occhi con le pupille, bocca, lingua, coda; fumetto ed effetti sotto "fx".</summary>
    static ToniPug ToniRig(Transform parent, Vector2 pos, float scale, Transform fx)
    {
        Transform t = Child(parent, "Toni", pos).transform;
        t.localScale = Vector3.one * scale;
        ToniPug pug = t.gameObject.AddComponent<ToniPug>();
        Transform tail = Child(t, "Tail", new Vector2(0.98f, 0.36f)).transform;
        Art(tail, "Art", "pug_tail", 1, false);
        Transform lie = Art(t, "Lie", "toni_lie", 2);
        var breathe = lie.gameObject.AddComponent<Pulse>();
        breathe.amount = 0.02f;
        breathe.speed = 2f;
        Transform stand = Art(t, "Stand", "toni_stand", 2);
        GameObject eyes = Art(t, "EyesOpen", "pug_eyes", 3, false).gameObject;
        var pupils = new List<Transform>();
        Vector2[] eyeCenters = { new Vector2(DoodleSprites.PugEyeLX, DoodleSprites.PugEyeY), new Vector2(DoodleSprites.PugEyeRX, DoodleSprites.PugEyeY) };
        foreach (Vector2 e in eyeCenters)
        {
            Transform pupil = Child(eyes.transform, "Pupil", Vector2.zero).transform;
            pupil.localPosition = e - (Vector2)eyes.transform.localPosition;
            Art(pupil, "Art", "pug_pupil", 4, false);
            pupils.Add(pupil);
        }
        GameObject half = Art(t, "EyesHalf", "pug_eyes_half", 3, false).gameObject;
        foreach (Vector2 e in eyeCenters)
        {
            Transform pupil = Art(half.transform, "Pupil", "pug_pupil", 4, false);
            pupil.localPosition = e + new Vector2(0f, -0.05f) - (Vector2)half.transform.localPosition;
        }
        half.SetActive(false);
        GameObject mouthOpen = Art(t, "MouthOpen", "pug_mouth_open", 4, false).gameObject;
        GameObject tongue = Art(t, "Tongue", "pug_tongue", 4, false).gameObject;
        GameObject alert = Art(t, "Alert", "alert", 5, true, new Vector2(-0.7f, 1.15f)).gameObject;

        SpeechBubble bubble = Bubble(fx, pos + new Vector2(0f, 1.6f));
        bubble.catVoice = false;
        bubble.transform.localScale = Vector3.one * 0.55f;
        SmokeEmitter zzz = Emitter(fx, "Zzz", pos, "zzz", 1.3f, 2.2f, 0.45f, 0.1f, 0.7f, Color.white, 30);
        zzz.spinRange = 12f;
        zzz.growth = 0.9f;
        zzz.sideDrift = 0.15f;
        SmokeEmitter hearts = Emitter(fx, "Hearts", pos, "heart", 5f, 1.2f, 0.9f, 0.5f, 0.8f, Color.white, 30);
        hearts.spinRange = 30f;
        hearts.growth = 0.4f;
        SmokeEmitter crumbs = Emitter(fx, "Crumbs", pos, "puff", 26f, 0.5f, -1.2f, 0.25f, 0.25f, new Color(1f, 0.95f, 0.8f), 12);

        SetField(pug, "lieArt", lie.gameObject);
        SetField(pug, "standArt", stand.gameObject);
        SetField(pug, "eyesOpen", eyes);
        SetField(pug, "eyesHalf", half);
        SetArray(pug, "pupils", pupils.ToArray());
        SetField(pug, "mouthOpen", mouthOpen);
        SetField(pug, "tongue", tongue);
        SetField(pug, "tail", tail);
        SetField(pug, "alert", alert);
        pug.bubble = bubble;
        pug.zzz = zzz;
        pug.hearts = hearts;
        pug.crumbs = crumbs;
        var so = new SerializedObject(pug);
        SerializedProperty ec = so.FindProperty("eyeCenters");
        ec.arraySize = 2;
        for (int i = 0; i < 2; i++) ec.GetArrayElementAtIndex(i).vector2Value = eyeCenters[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        return pug;
    }

    static TapButton RoundButton(Transform parent, string name, Vector2 pos, float scale, string text, int order)
    {
        GameObject go = Child(parent, name, pos);
        go.transform.localScale = Vector3.one * scale;
        Transform art = Art(go.transform, "Art", "big_button", order);
        go.AddComponent<BoxCollider2D>().size = new Vector2(1.9f, 1.05f);
        TapButton b = go.AddComponent<TapButton>();
        b.visual = art;
        b.id = name;
        if (!string.IsNullOrEmpty(text)) Label(parent, pos + new Vector2(0f, -0.62f * scale - 0.15f), order + 1, text, 0.045f).color = new Color(1f, 0.93f, 0.75f);
        return b;
    }

    // ---------- 25: MOVE, TONI! ----------

    static GameObject BuildToniButtonLevel()
    {
        var root = new GameObject("Level_Toni1");
        ToniButtonLevel level = root.AddComponent<ToniButtonLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);
        TapButton button = RoundButton(root.transform, "Button", new Vector2(-0.4f, -2.55f), 1.3f, "PRESS ME", 0);
        Bar(root.transform, "Rug", new Vector2(-0.3f, -2.75f), new Vector2(4.2f, 0.12f), new Color(0.85f, 0.45f, 0.4f, 0.5f), -5);
        ToniPug toni = ToniRig(root.transform, new Vector2(-0.3f, -1.75f), 1.25f, root.transform);
        Transform ball = Child(root.transform, "Ball", new Vector2(1.9f, -3.2f)).transform;
        Art(ball, "Art", "toni_ball", 8, false);

        SetField(level, "toni", toni);
        SetField(level, "button", button);
        SetField(level, "ball", ball);
        return SaveLevel(root, "Toni1");
    }

    // ---------- 50: SAY CHEESE! ----------

    static GameObject BuildToniPhotoLevel()
    {
        var root = new GameObject("Level_Toni2");
        ToniPhotoLevel level = root.AddComponent<ToniPhotoLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);
        Bar(root.transform, "Backdrop", new Vector2(-0.95f, 0.2f), new Vector2(3.6f, 4.6f), new Color(0.72f, 0.86f, 0.95f), -15);
        Bar(root.transform, "BackdropFloor", new Vector2(-0.95f, -2.3f), new Vector2(3.6f, 0.6f), new Color(0.66f, 0.8f, 0.9f), -14);
        Bar(root.transform, "Shelf", new Vector2(-1.9f, 1.25f), new Vector2(1.4f, 0.12f), new Color(0.55f, 0.36f, 0.22f), 1);
        Transform jar = Child(root.transform, "Jar", new Vector2(-1.9f, 1.85f)).transform;
        Art(jar, "Art", "treat_jar", 2);
        Label(jar, new Vector2(0f, 0.08f), 3, "TREATS", 0.03f);
        Art(root.transform, "Cushion", "pug_cushion", 1, true, new Vector2(-1.0f, -1.4f)).localScale = Vector3.one * 1.15f;
        ToniPug toni = ToniRig(root.transform, new Vector2(-1.0f, -1.4f), 1.15f, root.transform);

        Transform cam = Child(root.transform, "Camera", new Vector2(1.75f, -0.9f)).transform;
        Art(cam, "Art", "photo_camera", 6);
        Transform lens = Child(cam, "Lens", new Vector2(-0.74f, 0.15f)).transform;

        var boneGo = Child(root.transform, "Bone", new Vector2(-1.9f, 2.4f));
        boneGo.transform.localScale = Vector3.one * 0.8f;
        boneGo.AddComponent<CircleCollider2D>().radius = 0.45f;
        Draggable2D bone = boneGo.AddComponent<Draggable2D>();
        bone.itemId = "bone";
        bone.returnOnMiss = false;
        Art(boneGo.transform, "Art", "bone", 9);

        TapButton shutter = RoundButton(root.transform, "Shutter", new Vector2(1.2f, -3.6f), 0.75f, "SNAP!", 10);
        TextMesh film = Label(root.transform, new Vector2(-0.4f, 3.35f), 12, "FILM: 4", 0.05f);
        SpriteRenderer flash = Bar(root.transform, "Flash", Vector2.zero, new Vector2(12f, 16f), new Color(1f, 1f, 1f, 0f), 95);

        Transform polaroid = Child(root.transform, "Polaroid", new Vector2(0f, 0.7f)).transform;
        polaroid.localScale = Vector3.one * 1.5f;
        Art(polaroid, "Art", "polaroid", 75, false);
        Transform slot = Child(polaroid, "Photo", new Vector2(0.1f, 0.12f)).transform;
        TextMesh caption = Label(polaroid, new Vector2(0f, -0.78f), 77, "", 0.035f);

        SetField(level, "toni", toni);
        SetField(level, "bone", bone);
        SetField(level, "jar", jar);
        SetField(level, "lens", lens);
        SetField(level, "shutter", shutter);
        SetField(level, "flash", flash);
        SetField(level, "polaroid", polaroid);
        SetField(level, "photoSlot", slot);
        SetField(level, "caption", caption);
        SetField(level, "filmLabel", film);
        return SaveLevel(root, "Toni2");
    }

    // ---------- 75: WAKE UP, TONI! ----------

    static GameObject BuildToniFridgeLevel()
    {
        var root = new GameObject("Level_Toni3");
        ToniFridgeLevel level = root.AddComponent<ToniFridgeLevel>();
        Art(root.transform, "Kitchen", "kitchen_bg", -20, false);
        Art(root.transform, "Cushion", "pug_cushion", 1, true, new Vector2(-1.2f, -2.4f)).localScale = Vector3.one * 1.1f;
        ToniPug toni = ToniRig(root.transform, new Vector2(-1.2f, -2.4f), 1.1f, root.transform);

        Transform fridge = Child(root.transform, "Fridge", new Vector2(1.5f, -0.9f)).transform;
        Art(fridge, "Body", "fridge_body", 3, false);
        Transform glow = Art(fridge, "Light", "glow", 4, false, new Vector2(0f, 0.8f));
        glow.localScale = Vector3.one * 2.2f;
        Transform sausages = Art(fridge, "Sausages", "sausages", 5, true, new Vector2(0.1f, 0.15f));
        Transform hinge = Child(fridge, "Hinge", new Vector2(0.8f, 0f)).transform;
        Art(hinge, "Door", "fridge_door", 7, false);

        GameObject alarmGo = Child(root.transform, "Alarm", new Vector2(0.45f, -3.4f));
        alarmGo.transform.localScale = Vector3.one * 0.75f;
        Transform alarmArt = Art(alarmGo.transform, "Art", "alarm_clock", 8);
        alarmGo.AddComponent<CircleCollider2D>().radius = 0.6f;
        TapButton alarm = alarmGo.AddComponent<TapButton>();
        alarm.visual = alarmArt;
        alarm.id = "alarm";
        Label(root.transform, new Vector2(-1.9f, 2.9f), 12, "7:00 - WALK TIME!", 0.04f);

        SmokeEmitter toot = Emitter(root.transform, "Toot", Vector2.zero, "puff", 14f, 1.6f, 0.35f, 0.45f, 0.6f, new Color(0.6f, 0.85f, 0.35f, 0.8f), 30);

        SetField(level, "toni", toni);
        SetField(level, "fridge", fridge);
        SetField(level, "hinge", hinge);
        SetField(level, "fridgeLight", glow.GetComponent<SpriteRenderer>());
        SetField(level, "sausages", sausages);
        SetField(level, "alarm", alarm);
        SetField(level, "alarmArt", alarmArt);
        SetField(level, "toot", toot);
        return SaveLevel(root, "Toni3");
    }

    // ---------- 100: TONI'S BIRTHDAY ----------

    static GameObject BuildToniCakeLevel()
    {
        var root = new GameObject("Level_Toni4");
        ToniCakeLevel level = root.AddComponent<ToniCakeLevel>();
        Art(root.transform, "Party", "party_bg", -20, false);
        Bar(root.transform, "Table", new Vector2(-1.3f, -2.28f), new Vector2(2.5f, 0.16f), new Color(0.55f, 0.36f, 0.22f), 2);
        foreach (float x in new[] { -2.3f, -0.3f }) Bar(root.transform, "Leg", new Vector2(x, -2.8f), new Vector2(0.14f, 0.95f), new Color(0.45f, 0.29f, 0.18f), 1);
        Transform cake = Child(root.transform, "Cake", new Vector2(-1.3f, -1.6f)).transform;
        Art(cake, "Art", "dog_cake", 5);
        Art(cake, "Candle", "candle", 6, true, new Vector2(0.3f, 0.12f));
        Transform flame = Child(cake, "Flame", new Vector2(0.3f, 0.7f)).transform;
        Art(flame, "Art", "flame", 7, false);
        Label(root.transform, new Vector2(0f, 3.0f), 12, "HAPPY BIRTHDAY TONI!", 0.05f);

        ToniPug toni = ToniRig(root.transform, new Vector2(1.1f, -2.35f), 1.25f, root.transform);
        Transform hat = Art(toni.transform, "Hat", "tiny_hat_red", 6, false, new Vector2(-0.55f, 0.72f));
        hat.localScale = Vector3.one * 1.4f;
        hat.localRotation = Quaternion.Euler(0, 0, 18f);

        Transform vase = Art(root.transform, "Vase", "vase", 3, true, new Vector2(2.15f, -0.2f));
        vase.localScale = Vector3.one * 0.6f;
        var featherGo = Child(root.transform, "Feather", new Vector2(1.75f, 0.35f));
        featherGo.transform.localRotation = Quaternion.Euler(0, 0, 40f);
        var box = featherGo.AddComponent<BoxCollider2D>();
        box.offset = new Vector2(0.55f, 0.26f);
        box.size = new Vector2(1.3f, 0.5f);
        Draggable2D feather = featherGo.AddComponent<Draggable2D>();
        feather.itemId = "feather";
        feather.returnOnMiss = true;
        Art(featherGo.transform, "Art", "feather", 9);

        SmokeEmitter candleSmoke = Emitter(root.transform, "CandleSmoke", Vector2.zero, "puff", 8f, 1.2f, 0.8f, 0.05f, 0.25f, new Color(0.7f, 0.7f, 0.75f, 0.8f), 8);
        SmokeEmitter sneeze = Emitter(root.transform, "Sneeze", Vector2.zero, "puff", 40f, 0.5f, 0.2f, 0.5f, 0.35f, new Color(0.9f, 1f, 0.95f, 0.9f), 20);
        sneeze.sideDrift = -2.5f;
        SmokeEmitter confetti = Emitter(root.transform, "Confetti", new Vector2(0f, 3.2f), "confetti", 40f, 2.2f, -1.6f, 2.6f, 1f, Color.white, 50);
        confetti.spinRange = 360f;
        SpriteRenderer splat = Art(root.transform, "Splat", "cream_splat", 90, true, new Vector2(0f, 0.3f)).GetComponent<SpriteRenderer>();
        splat.transform.localScale = Vector3.one * 2.2f;
        Transform tongue = Fitted(root.transform, "BigTongue", "pug_tongue", new Vector2(0f, -7.5f), 4f, 91);

        SetField(level, "toni", toni);
        SetField(level, "feather", feather);
        SetField(level, "cake", cake);
        SetField(level, "flame", flame);
        SetField(level, "candleSmoke", candleSmoke);
        SetField(level, "sneezeCloud", sneeze);
        SetField(level, "confetti", confetti);
        SetField(level, "splat", splat);
        SetField(level, "bigTongue", tongue);
        SetField(level, "candleBubble", SmallBubble(root.transform, new Vector2(-1.0f, 0.3f)));
        return SaveLevel(root, "Toni4");
    }
}
#endif
