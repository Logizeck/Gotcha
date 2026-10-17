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
    /// <summary>I livelli di Toni, nell'ordine in cui compaiono (uno ogni 25). Quelli nuovi si aggiungono in fondo.</summary>
    static List<LevelDefinition> ToniLevels() => new List<LevelDefinition>
    {
        Define("Toni1", "toni_stuck", "STUCK!",
            "Toni tried the cat flap. Bad idea. Pushing alone won't do... something slippery from the kitchen, maybe?",
            BuildToniStuckLevel(), "toni_toot"),
        Define("Toni2", "toni_tug", "TUG OF WAR",
            "The house key is missing and Toni looks VERY innocent. Pull the sock out... gently! Yank it and he pulls back.",
            BuildToniTugLevel(), "toni_tug"),
        Define("Toni3", "toni_hide", "WHERE'S TONI?",
            "Toni is hiding... and snoring. Move your finger around the room and listen (and feel): the closer, the louder.",
            BuildToniHideLevel()),
        Define("Toni4", "toni_cake", "TONI'S BIRTHDAY",
            "Toni has to blow out his candle... but pugs can't blow. That peacock feather looks ticklish... and his nose is RIGHT there.",
            BuildToniCakeLevel()),
    };

    /// <summary>
    /// Intreccia i livelli normali con quelli di Toni: Toni va in ogni posizione multipla di 25 (25, 50, 75, ...), i
    /// normali riempiono il resto nel loro ordine. Se servisse un livello di Toni che non c'è ancora, il builder lo
    /// segnala e prosegue con i livelli normali.
    /// </summary>
    static List<LevelDefinition> PlaceToniLevels(List<LevelDefinition> regular, List<LevelDefinition> toni)
    {
        var all = new List<LevelDefinition>();
        int r = 0, t = 0;
        while (r < regular.Count)
        {
            if ((all.Count + 1) % 25 == 0)
            {
                if (t < toni.Count) { all.Add(toni[t++]); continue; }
                Debug.LogWarning("[Gotcha] Manca un livello di Toni per la posizione " + (all.Count + 1) + ": aggiungilo in ToniLevels().");
            }
            all.Add(regular[r++]);
        }
        // un livello di Toni pronto per la prossima casella resta in fondo solo se cade proprio lì
        if (t < toni.Count && (all.Count + 1) % 25 == 0) all.Add(toni[t++]);
        return all;
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

    static Draggable2D Prop(Transform parent, string name, string sprite, Vector2 pos, float scale, Vector2 colliderSize, Vector2 colliderOffset, int order, float rot = 0f)
    {
        GameObject go = Child(parent, name, pos);
        go.transform.localScale = Vector3.one * scale;
        go.transform.localRotation = Quaternion.Euler(0, 0, rot);
        var box = go.AddComponent<BoxCollider2D>();
        box.size = colliderSize;
        box.offset = colliderOffset;
        Draggable2D d = go.AddComponent<Draggable2D>();
        d.itemId = name.ToLowerInvariant();
        d.returnOnMiss = true;
        Art(go.transform, "Art", sprite, order);
        return d;
    }

    // ---------- 25: STUCK! ----------

    static GameObject BuildToniStuckLevel()
    {
        var root = new GameObject("Level_Toni1");
        ToniStuckLevel level = root.AddComponent<ToniStuckLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);
        // fuori, a destra del muro: cielo, sole, prato e un cespuglio
        Bar(root.transform, "Sky", new Vector2(1.8f, 1.5f), new Vector2(2.8f, 13f), new Color(0.62f, 0.82f, 0.98f), -18);
        Bar(root.transform, "Grass", new Vector2(1.8f, -3.7f), new Vector2(2.8f, 3.4f), new Color(0.5f, 0.8f, 0.4f), -17);
        Art(root.transform, "Sun", "sun", -16, true, new Vector2(2.1f, 3.0f)).localScale = Vector3.one * 0.6f;
        Art(root.transform, "Bush", "bush", -16, true, new Vector2(2.2f, -2.2f));
        Transform wall = Art(root.transform, "Wall", "wall_section", 8, false, new Vector2(0.4f, 0f));
        ToniPug toni = ToniRig(root.transform, new Vector2(0.15f, -2.59f), 1.2f, root.transform);
        Transform flap = Child(root.transform, "Flap", new Vector2(0.64f, -1.52f)).transform;
        flap.localRotation = Quaternion.Euler(0, 0, 55f);
        Art(flap, "Art", "dog_flap", 9, false);

        // il tavolino con le "soluzioni"
        Bar(root.transform, "Table", new Vector2(-1.6f, -0.65f), new Vector2(2.0f, 0.12f), new Color(0.55f, 0.36f, 0.22f), 2);
        foreach (float x in new[] { -2.45f, -0.75f }) Bar(root.transform, "Leg", new Vector2(x, -1.15f), new Vector2(0.1f, 0.9f), new Color(0.45f, 0.29f, 0.18f), 1);
        Draggable2D butter = Prop(root.transform, "Butter", "butter", new Vector2(-2.15f, -0.45f), 0.9f, new Vector2(1f, 0.5f), new Vector2(0f, 0.04f), 10);
        Draggable2D bone = Prop(root.transform, "Bone", "bone", new Vector2(-1.5f, -0.48f), 0.55f, new Vector2(1.1f, 0.5f), Vector2.zero, 10);
        Draggable2D feather = Prop(root.transform, "Feather", "feather", new Vector2(-1.15f, -0.45f), 0.55f, new Vector2(1.75f, 0.75f), new Vector2(0.55f, 0.22f), 10, 15f);

        SmokeEmitter shine = Emitter(root.transform, "Shine", Vector2.zero, "puff", 6f, 0.7f, 0.4f, 0.4f, 0.18f, new Color(1f, 0.95f, 0.5f, 0.9f), 12);
        SmokeEmitter toot = Emitter(root.transform, "Toot", Vector2.zero, "puff", 30f, 1.4f, 0.2f, 0.4f, 0.6f, new Color(0.6f, 0.85f, 0.35f, 0.85f), 12);
        toot.sideDrift = -1.5f;

        SetField(level, "toni", toni);
        SetField(level, "wall", wall);
        SetField(level, "flap", flap);
        SetField(level, "butter", butter);
        SetField(level, "bone", bone);
        SetField(level, "feather", feather);
        SetField(level, "shine", shine);
        SetField(level, "toot", toot);
        return SaveLevel(root, "Toni1");
    }

    // ---------- 50: TUG OF WAR ----------

    static GameObject BuildToniTugLevel()
    {
        var root = new GameObject("Level_Toni2");
        ToniTugLevel level = root.AddComponent<ToniTugLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);
        Label(root.transform, new Vector2(0f, 3.2f), 12, "WHERE ARE MY KEYS?!", 0.05f);
        Art(root.transform, "Cushion", "pug_cushion", 1, true, new Vector2(0.9f, -2.0f)).localScale = Vector3.one * 1.3f;
        ToniPug toni = ToniRig(root.transform, new Vector2(0.9f, -2.0f), 1.3f, root.transform);
        var items = new List<Transform>();
        foreach (var it in new[] { ("sock", 1.0f), ("fish", 1.1f), ("toni_ball", 1.1f), ("coin", 1.2f), ("bell", 0.7f), ("sausages", 0.75f), ("cheese_wedge", 1.0f), ("key", 0.6f) })
        {
            Transform t = Child(root.transform, "Item_" + it.Item1, new Vector2(0f, -2.3f)).transform;
            t.localScale = Vector3.one * it.Item2;
            Art(t, "Art", it.Item1, 6);
            items.Add(t);
        }
        LineRenderer rope = Line(root.transform, "Rope", 0.035f, new Color(0.96f, 0.95f, 0.9f), 5);

        SetField(level, "toni", toni);
        SetArray(level, "items", items.ToArray());
        SetField(level, "rope", rope);
        return SaveLevel(root, "Toni2");
    }

    // ---------- 75: WHERE'S TONI? ----------

    static GameObject BuildToniHideLevel()
    {
        var root = new GameObject("Level_Toni3");
        ToniHideLevel level = root.AddComponent<ToniHideLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);
        Art(root.transform, "Window", "window_frame", -10, false, new Vector2(1.35f, 1.1f)).localScale = Vector3.one * 0.8f;

        var spots = new List<TapButton>();
        var arts = new List<Transform>();
        var gags = new List<GameObject>();
        var toniAt = new List<Vector2>();
        void Spot(string name, string sprite, Vector2 pos, Vector2 scale, Vector2 size, Vector2 offset, Vector2 toniPos, System.Action<Transform> gag)
        {
            GameObject go = Child(root.transform, name, pos);
            Transform art = Art(go.transform, "Art", sprite, 6, false);
            art.localScale = new Vector3(scale.x, scale.y, 1f);
            art.localPosition = Vector3.Scale(art.localPosition, new Vector3(scale.x, scale.y, 1f));
            var box = go.AddComponent<BoxCollider2D>();
            box.size = size;
            box.offset = offset;
            TapButton tb = go.AddComponent<TapButton>();
            tb.id = name;
            tb.visual = Child(go.transform, "PressDummy", Vector2.zero).transform;   // si alza con la sua animazione
            GameObject g = Child(root.transform, name + "Gag", pos + new Vector2(0f, 0.1f));
            gag(g.transform);
            spots.Add(tb);
            arts.Add(art);
            gags.Add(g);
            toniAt.Add(toniPos);
        }
        Spot("Sofa", "sofa", new Vector2(-1.1f, -2.2f), new Vector2(1.1f, 1.1f), new Vector2(2.7f, 1.3f), Vector2.zero, new Vector2(-1.1f, -1.55f), g =>
        {
            for (int i = 0; i < 3; i++) Art(g, "Sock", "sock", 7, true, new Vector2(-0.5f + i * 0.45f, 0.1f)).localRotation = Quaternion.Euler(0, 0, -30f + i * 30f);
        });
        Spot("Box", "box", new Vector2(1.25f, -3.1f), Vector2.one, new Vector2(1.2f, 1.1f), Vector2.zero, new Vector2(1.25f, -2.6f), g =>
            Art(g, "Mouse", "mouse", 7, true).localScale = Vector3.one * 0.5f);
        Spot("Basket", "basket", new Vector2(-1.2f, -4.1f), new Vector2(0.85f, 0.85f), new Vector2(1.6f, 1.2f), new Vector2(0f, 0.45f), new Vector2(-1.2f, -3.4f), g =>
            Art(g, "Cat", "cat", 7, true, new Vector2(0f, 0.4f)).localScale = Vector3.one * 0.45f);
        Spot("Curtain", "curtain", new Vector2(2.1f, 0.6f), new Vector2(1f, 1.3f), new Vector2(1.1f, 3f), new Vector2(0f, -1.35f), new Vector2(1.5f, -1.95f), g =>
            Art(g, "Wasp", "wasp", 7, true, new Vector2(-0.3f, -0.6f)).localScale = Vector3.one * 0.6f);

        ToniPug toni = ToniRig(root.transform, Vector2.zero, 0.85f, root.transform);
        foreach (SpriteRenderer sr in toni.GetComponentsInChildren<SpriteRenderer>(true)) sr.sortingOrder += 6;   // davanti al nascondiglio

        SetField(level, "toni", toni);
        SetArray(level, "spots", spots.ToArray());
        SetArray(level, "spotArt", arts.ToArray());
        SetArray(level, "gags", gags.ToArray());
        var so = new SerializedObject(level);
        SerializedProperty ta = so.FindProperty("toniAt");
        ta.arraySize = toniAt.Count;
        for (int i = 0; i < toniAt.Count; i++) ta.GetArrayElementAtIndex(i).vector2Value = toniAt[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        SpeechBubble bubble = Bubble(root.transform, new Vector2(0f, 3.1f));
        bubble.transform.localScale = Vector3.one * 0.5f;
        SetField(level, "bubble", bubble);
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

        Transform vase = Art(root.transform, "Vase", "vase", 10, true, new Vector2(2.45f, -1.0f));
        vase.localScale = Vector3.one * 0.8f;
        // piuma di pavone nel vaso: l'occhio colorato in alto (è la punta che fa il solletico), il gambo nel vaso
        var featherGo = Child(root.transform, "Feather", new Vector2(1.75f, 0.35f));
        featherGo.transform.localRotation = Quaternion.Euler(0, 0, -83f);
        var box = featherGo.AddComponent<BoxCollider2D>();
        box.offset = new Vector2(0.55f, 0.22f);
        box.size = new Vector2(1.75f, 0.75f);
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
