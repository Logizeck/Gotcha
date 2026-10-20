#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli della specifica 105-200 (in gioco dal 151), pacchetto F.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels41F() => new List<LevelDefinition>
    {
        Define("TheWrongShadow", "the_wrong_shadow", "THE WRONG SHADOW", "The monster isn't real.", BuildS41FWrongShadow()),
        Define("TheImpossibleScale", "the_impossible_scale", "THE IMPOSSIBLE SCALE", "Size matters here.", BuildS41FImpossibleScale()),
        Define("RewindReality", "rewind_reality", "REWIND REALITY", "The accident already happened.", BuildS41FRewindReality()),
        Define("TheStickyFinger", "the_sticky_finger", "THE STICKY FINGER", "The coin won't leave your finger.", BuildS41FStickyFinger()),
        Define("TheGuiltyButton", "the_guilty_button", "THE GUILTY BUTTON", "Someone is hiding something.", BuildS41FGuiltyButton()),
        Define("TheInvisibleStairs", "the_invisible_stairs", "THE INVISIBLE STAIRS", "The steps are there.", BuildS41FInvisibleStairs()),
        Define("LieDetector", "lie_detector", "LIE DETECTOR", "The robot may not be trustworthy.", BuildS41FLieDetector()),
        Define("TooBright", "too_bright", "TOO BRIGHT!", "He hates the light.", BuildS41FTooBright()),
    };

    // ---------- Aiuti comuni del pacchetto F ----------

    /// <summary>Riga dell'obiettivo in alto, su una fascia scura.</summary>
    static TextMesh S41F_Goal(Transform root, string text)
    {
        Transform h = Child(root, "Goal", new Vector2(0f, 3.25f)).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(5.5f, 0.5f), new Color(0.12f, 0.09f, 0.16f, 0.72f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, text, 0.05f);
        t.color = Color.white;
        Fit(t, 5.3f, 0.42f);
        return t;
    }

    /// <summary>Etichetta bianca su fascia scura (contatori).</summary>
    static TextMesh S41F_Info(Transform parent, string name, Vector2 pos, float width, int order = 70)
    {
        Transform h = Child(parent, name, pos).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(width, 0.4f), new Color(0.12f, 0.09f, 0.16f, 0.65f), order);
        TextMesh t = Label(h, Vector2.zero, order + 1, "", 0.04f);
        t.color = Color.white;
        Fit(t, width - 0.2f, 0.32f);
        return t;
    }

    static SmokeEmitter[] S41F_Confetti(Transform root)
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

    /// <summary>Contenitore scalato: i disegni messi dentro restano nelle coordinate del disegno (scalate).</summary>
    static Transform S41F_Holder(Transform parent, string name, Vector2 pos, float scale)
    {
        Transform t = Child(parent, name, pos).transform;
        t.localScale = Vector3.one * scale;
        return t;
    }

    /// <summary>Pulsante a pillola con scritta e collider.</summary>
    static Transform S41F_Pill(Transform parent, string name, Vector2 pos, string text, Color tint, int order, out TextMesh label, out Collider2D area)
    {
        Transform b = Child(parent, name, pos).transform;
        Art(b, "Art", "s41f_pill_btn", order, false).GetComponent<SpriteRenderer>().color = tint;
        label = Fit(Label(b, new Vector2(0f, 0.02f), order + 1, text, 0.06f), 1.3f, 0.4f);
        BoxCollider2D c = b.gameObject.AddComponent<BoxCollider2D>();
        c.size = new Vector2(1.8f, 0.85f);
        area = c;
        return b;
    }

    static void S41F_Common(Component level, Transform root, Vector2 bubbleAt)
    {
        SetField(level, "bubble", SmallBubble(root, bubbleAt));
        SetArray(level, "confetti", S41F_Confetti(root));
    }

    // ---------- 146 The wrong shadow ----------

    static GameObject BuildS41FWrongShadow()
    {
        var root = new GameObject("Level_TheWrongShadow");
        TheWrongShadowLevel level = root.AddComponent<TheWrongShadowLevel>();
        Art(root.transform, "Room", "s41f_shadow_bg", -20, false);
        Transform feet = Child(root.transform, "CatFeet", new Vector2(0f, -1.5f)).transform;
        Transform shadow = Child(root.transform, "Shadow", new Vector2(0f, -1.5f)).transform;
        SpriteRenderer shadowArt = Art(shadow, "Art", "s41f_monster_shadow", -5, false).GetComponent<SpriteRenderer>();
        shadowArt.color = new Color(1f, 1f, 1f, 0.6f);
        Transform cat = S41F_Holder(root.transform, "Cat", new Vector2(0f, -1.5f), 0.75f);
        Art(cat, "Art", "cat", 2, false, new Vector2(0f, 0.95f));

        Transform lamp = Child(root.transform, "Lamp", new Vector2(0.3f, -0.75f)).transform;
        Transform glow = Art(lamp, "Glow", "glow", -4, false, new Vector2(0f, 1.3f));
        glow.localScale = Vector3.one * 3f;
        glow.GetComponent<SpriteRenderer>().color = new Color(1f, 0.9f, 0.55f, 0.45f);
        SpriteRenderer lampArt = Art(lamp, "Art", "s41f_floor_lamp", 1, false).GetComponent<SpriteRenderer>();
        BoxCollider2D lc = lamp.gameObject.AddComponent<BoxCollider2D>();
        lc.size = new Vector2(1.1f, 2.0f);
        lc.offset = new Vector2(0f, 0.9f);

        Transform mouse = Child(root.transform, "Mouse", new Vector2(2.2f, -3.95f)).transform;
        Transform body = S41F_Holder(mouse, "Body", Vector2.zero, 0.7f);
        Art(body, "Art", "mouse", 6, false, new Vector2(0f, 0.56f));
        BoxCollider2D mc = mouse.gameObject.AddComponent<BoxCollider2D>();
        mc.size = new Vector2(1.2f, 1.2f);
        mc.offset = new Vector2(0f, 0.5f);

        S41F_Goal(root.transform, "HELP THE MOUSE REACH THE CHEESE");
        SetField(level, "lamp", lamp);
        SetField(level, "lampArea", lc);
        SetField(level, "lampArt", lampArt);
        SetField(level, "catFeet", feet);
        SetField(level, "shadow", shadow);
        SetField(level, "shadowArt", shadowArt);
        SetField(level, "mouse", mouse);
        SetField(level, "mouseArea", mc);
        SetField(level, "triesText", S41F_Info(root.transform, "Tries", new Vector2(0f, 2.6f), 2.8f));
        S41F_Common(level, root.transform, new Vector2(1.3f, -2.4f));
        return SaveLevel(root, "TheWrongShadow");
    }

    // ---------- 147 The impossible scale ----------

    static GameObject BuildS41FImpossibleScale()
    {
        var root = new GameObject("Level_TheImpossibleScale");
        TheImpossibleScaleLevel level = root.AddComponent<TheImpossibleScaleLevel>();
        Art(root.transform, "Museum", "mu_bg", -20, false);
        Vector2 pivot = new Vector2(0f, 0.6f);
        Art(root.transform, "Stand", "s41f_scale_stand", 1, false, pivot);
        Transform light = Art(root.transform, "BalanceLight", "glow", 2, false, pivot);
        light.localScale = Vector3.one * 0.9f;
        Transform beam = Child(root.transform, "Beam", pivot).transform;
        Art(beam, "Art", "s41f_scale_beam", 3, false);
        Transform panL = Child(root.transform, "PanL", pivot + new Vector2(-1.75f, 0f)).transform;
        Art(panL, "Art", "s41f_scale_pan", 2, false);
        Transform panR = Child(root.transform, "PanR", pivot + new Vector2(1.75f, 0f)).transform;
        Art(panR, "Art", "s41f_scale_pan", 2, false);
        Transform elephant = Child(panL, "Elephant", new Vector2(0f, -1.25f)).transform;
        Art(elephant, "Art", "s41f_elephant", 4, false);
        BoxCollider2D ec = elephant.gameObject.AddComponent<BoxCollider2D>();
        ec.size = new Vector2(1.6f, 1.5f);
        ec.offset = new Vector2(0f, 0.75f);
        Transform feather = Child(panR, "Feather", new Vector2(0f, -1.25f)).transform;
        Art(feather, "Art", "s41f_big_feather", 4, false);
        BoxCollider2D fc = feather.gameObject.AddComponent<BoxCollider2D>();
        fc.size = new Vector2(0.9f, 1.1f);
        fc.offset = new Vector2(0f, 0.5f);

        S41F_Goal(root.transform, "BALANCE THE SCALE");
        SetField(level, "beam", beam);
        SetField(level, "leftPan", panL);
        SetField(level, "rightPan", panR);
        SetField(level, "feather", feather);
        SetField(level, "elephant", elephant);
        SetField(level, "featherArea", fc);
        SetField(level, "elephantArea", ec);
        SetField(level, "featherWeight", S41F_Info(panR, "Weight", new Vector2(0f, -1.85f), 1.6f, 10));
        SetField(level, "elephantWeight", S41F_Info(panL, "Weight", new Vector2(0f, -1.85f), 1.6f, 10));
        SetField(level, "triesText", S41F_Info(root.transform, "Tries", new Vector2(0f, -4.2f), 2.6f));
        SetField(level, "balanceLight", light.GetComponent<SpriteRenderer>());
        S41F_Common(level, root.transform, new Vector2(-1.0f, 2.3f));
        return SaveLevel(root, "TheImpossibleScale");
    }

    // ---------- 148 Rewind reality ----------

    static GameObject BuildS41FRewindReality()
    {
        var root = new GameObject("Level_RewindReality");
        RewindRealityLevel level = root.AddComponent<RewindRealityLevel>();
        Art(root.transform, "Room", "s41f_rewind_bg", -20, false);
        Art(root.transform, "Overlay", "s41f_cam_overlay", 30, false);
        TextMesh rec = Fit(Label(root.transform, new Vector2(-1.7f, 2.45f), 31, "REC  CAM 1", 0.045f), 1.8f, 0.3f);
        rec.color = new Color(1f, 0.3f, 0.35f);

        Transform cat = Child(root.transform, "Cat", new Vector2(-0.6f, 0.02f)).transform;
        cat.localScale = Vector3.one * 0.8f;
        SpriteRenderer catArt = Art(cat, "Art", "s41f_walk_cat", 4, false).GetComponent<SpriteRenderer>();
        Transform vase = Child(root.transform, "Vase", new Vector2(1.95f, 0.02f)).transform;
        Art(vase, "Art", "vase", 5, false, new Vector2(0f, 0.45f));
        BoxCollider2D vc = vase.gameObject.AddComponent<BoxCollider2D>();
        vc.size = new Vector2(1.0f, 1.1f);
        vc.offset = new Vector2(0f, 0.45f);
        GameObject shards = Art(root.transform, "Shards", "s41f_shards", 4, false, new Vector2(2.5f, -2.3f)).gameObject;

        // posti sicuri: mensola e cuscino
        var zones = new List<Collider2D>();
        var spots = new List<Transform>();
        var marks = new List<SpriteRenderer>();
        Vector2[] spotAt = { new Vector2(-1.4f, 1.29f), new Vector2(-1.75f, -2.15f) };
        Vector2[] zoneAt = { new Vector2(-1.6f, 1.75f), new Vector2(-1.75f, -1.9f) };
        for (int i = 0; i < 2; i++)
        {
            spots.Add(Child(root.transform, "SafeSpot" + i, spotAt[i]).transform);
            BoxCollider2D z = Child(root.transform, "SafeZone" + i, zoneAt[i]).AddComponent<BoxCollider2D>();
            z.size = new Vector2(1.9f, 1.3f);
            zones.Add(z);
            SpriteRenderer m = Art(root.transform, "Mark" + i, "glow", 3, false, spotAt[i] + new Vector2(0f, 0.4f)).GetComponent<SpriteRenderer>();
            m.transform.localScale = Vector3.one * 1.6f;
            m.color = new Color(0.5f, 1f, 0.55f, 0.7f);
            marks.Add(m);
        }

        Bar(root.transform, "Deck", new Vector2(0f, -3.95f), new Vector2(6.2f, 2.2f), new Color(0.1f, 0.08f, 0.14f, 0.88f), 31);
        Bar(root.transform, "Track", new Vector2(0f, -3.3f), new Vector2(4.5f, 0.14f), new Color(0.75f, 0.75f, 0.85f), 32);
        BoxCollider2D track = Child(root.transform, "TrackArea", new Vector2(0f, -3.3f)).AddComponent<BoxCollider2D>();
        track.size = new Vector2(5.0f, 0.9f);
        Transform handle = Child(root.transform, "Handle", new Vector2(2.2f, -3.3f)).transform;
        Art(handle, "Art", "s41f_tape_handle", 34, false);
        Transform play = S41F_Pill(root.transform, "Play", new Vector2(0f, -4.15f), "PAUSE", new Color(1f, 0.85f, 0.45f), 33, out TextMesh playLabel, out Collider2D playArea);
        TextMesh clockText = Fit(Label(root.transform, new Vector2(-1.75f, -4.15f), 33, "00:05", 0.06f), 1.3f, 0.4f);
        clockText.color = Color.white;

        S41F_Goal(root.transform, "SAVE THE VASE");
        SetField(level, "cat", cat);
        SetField(level, "catArt", catArt);
        SetField(level, "catWalk", sprites["s41f_walk_cat"]);
        SetField(level, "catSwat", sprites["s41f_walk_cat_swat"]);
        SetField(level, "vase", vase);
        SetField(level, "vaseArea", vc);
        SetField(level, "shards", shards);
        SetField(level, "handle", handle);
        SetField(level, "trackArea", track);
        SetField(level, "playArea", playArea);
        SetField(level, "playLabel", playLabel);
        SetField(level, "playButton", play);
        SetField(level, "clockText", clockText);
        SetField(level, "fallsText", S41F_Info(root.transform, "Falls", new Vector2(1.75f, -4.15f), 1.7f, 33));
        SetArray(level, "safeZones", zones.ToArray());
        SetArray(level, "safeSpots", spots.ToArray());
        SetArray(level, "safeMarks", marks.ToArray());
        S41F_Common(level, root.transform, new Vector2(0.5f, 1.7f));
        return SaveLevel(root, "RewindReality");
    }

    // ---------- 149 The sticky finger ----------

    static GameObject BuildS41FStickyFinger()
    {
        var root = new GameObject("Level_TheStickyFinger");
        TheStickyFingerLevel level = root.AddComponent<TheStickyFingerLevel>();
        Art(root.transform, "Bank", "s41f_bank_bg", -20, false);
        Bar(root.transform, "Hook", new Vector2(1.3f, 2.88f), new Vector2(0.06f, 0.3f), new Color(0.17f, 0.13f, 0.2f), 5);
        Transform gum = Child(root.transform, "Gum", new Vector2(1.3f, 2.7f)).transform;
        Art(gum, "Art", "s41f_gum_wad", 6, false);
        SpriteRenderer strand = Bar(root.transform, "Strand", new Vector2(1.3f, 2.0f), Vector2.one, new Color(1f, 0.56f, 0.75f), 5);
        Transform coin = Child(root.transform, "Coin", new Vector2(1.3f, 1.35f)).transform;
        Art(coin, "Art", "s41f_sticky_coin", 7, false);
        coin.gameObject.AddComponent<CircleCollider2D>().radius = 0.55f;
        Transform safe = Child(root.transform, "Safe", new Vector2(-1.8f, -3.4f)).transform;
        SpriteRenderer safeArt = Art(safe, "Art", "s41f_coin_safe", 4, false).GetComponent<SpriteRenderer>();
        BoxCollider2D sc = safe.gameObject.AddComponent<BoxCollider2D>();
        sc.size = new Vector2(1.6f, 1.4f);
        sc.offset = new Vector2(0f, 0.6f);

        S41F_Goal(root.transform, "PUT THE COIN IN THE SAFE");
        SetField(level, "anchor", gum);
        SetField(level, "coin", coin);
        SetField(level, "coinArea", coin.GetComponent<Collider2D>());
        SetField(level, "strand", strand.transform);
        SetField(level, "safe", safe);
        SetField(level, "safeArea", sc);
        SetField(level, "safeArt", safeArt);
        SetField(level, "safeHappy", sprites["s41f_coin_safe_happy"]);
        SetField(level, "triesText", S41F_Info(root.transform, "Tries", new Vector2(0f, -4.2f), 2.6f));
        S41F_Common(level, root.transform, new Vector2(-0.9f, 1.3f));
        return SaveLevel(root, "TheStickyFinger");
    }

    // ---------- 151 The guilty button ----------

    static GameObject BuildS41FGuiltyButton()
    {
        var root = new GameObject("Level_TheGuiltyButton");
        TheGuiltyButtonLevel level = root.AddComponent<TheGuiltyButtonLevel>();
        Art(root.transform, "Room", "s41f_interro_bg", -20, false);
        Color[] tints = { new Color(1f, 0.62f, 0.62f), new Color(1f, 0.9f, 0.5f), new Color(0.6f, 0.82f, 1f) };
        var suspects = new List<Transform>();
        var arts = new List<SpriteRenderer>();
        var areas = new List<Collider2D>();
        var drops = new List<SpriteRenderer>();
        for (int i = 0; i < 3; i++)
        {
            Transform s = Child(root.transform, "Suspect" + i, new Vector2(-1.6f + i * 1.6f, -1.6f)).transform;
            SpriteRenderer a = Art(s, "Art", "s41f_suspect", 3, false).GetComponent<SpriteRenderer>();
            a.color = tints[i];
            BoxCollider2D c = s.gameObject.AddComponent<BoxCollider2D>();
            c.size = new Vector2(1.4f, 1.6f);
            c.offset = new Vector2(0f, 0.7f);
            suspects.Add(s);
            arts.Add(a);
            areas.Add(c);
            drops.Add(Art(root.transform, "Drop" + i, "s41f_drop_water", 6, false).GetComponent<SpriteRenderer>());
        }
        SpriteRenderer sparkle = Art(root.transform, "Sparkle", "sparkle_star", 7, false).GetComponent<SpriteRenderer>();
        Transform key = S41F_Holder(root.transform, "Key", new Vector2(0f, 0f), 0.6f);
        Art(key, "Art", "key", 9, false);
        Transform detective = S41F_Holder(root.transform, "Detective", new Vector2(1.85f, -4.4f), 1.15f);
        Art(detective, "Art", "s41f_detective", 8, false);
        TextMesh help = Fit(Label(root.transform, new Vector2(-0.7f, -2.35f), 8, "HOLD = PRESSURE    TAP = ACCUSE", 0.04f), 3.6f, 0.3f);
        help.color = new Color(1f, 0.95f, 0.75f);

        S41F_Goal(root.transform, "WHO HID THE KEY?");
        SetArray(level, "suspects", suspects.ToArray());
        SetArray(level, "suspectArts", arts.ToArray());
        SetArray(level, "suspectAreas", areas.ToArray());
        SetArray(level, "drops", drops.ToArray());
        SetField(level, "idleSprite", sprites["s41f_suspect"]);
        SetField(level, "pressedSprite", sprites["s41f_suspect_pressed"]);
        SetField(level, "waterDrop", sprites["s41f_drop_water"]);
        SetField(level, "metalDrop", sprites["s41f_drop_metal"]);
        SetField(level, "sparkle", sparkle);
        SetField(level, "key", key);
        SetField(level, "detective", detective);
        SetField(level, "triesText", S41F_Info(root.transform, "Tries", new Vector2(0f, 2.6f), 4.2f));
        S41F_Common(level, root.transform, new Vector2(0f, 0.9f));
        return SaveLevel(root, "TheGuiltyButton");
    }

    // ---------- 152 The invisible stairs ----------

    static GameObject BuildS41FInvisibleStairs()
    {
        var root = new GameObject("Level_TheInvisibleStairs");
        TheInvisibleStairsLevel level = root.AddComponent<TheInvisibleStairsLevel>();
        Art(root.transform, "Sky", "s41f_stairs_bg", -20, false);
        Transform door = Child(root.transform, "Door", new Vector2(1.9f, 1.4f)).transform;
        SpriteRenderer doorArt = Art(door, "Art", "s41f_cloud_door", 3, false).GetComponent<SpriteRenderer>();
        var steps = new List<Transform>();
        var stepArts = new List<SpriteRenderer>();
        for (int i = 0; i < 4; i++)
        {
            Transform s = Child(root.transform, "Step" + i, Vector2.zero).transform;
            stepArts.Add(Art(s, "Art", "s41f_glass_step", 4, false).GetComponent<SpriteRenderer>());
            steps.Add(s);
        }
        Transform climber = Child(root.transform, "Climber", new Vector2(-2.0f, -3.4f)).transform;
        Art(climber, "Art", "s41f_climber", 6, false);
        BoxCollider2D cc = climber.gameObject.AddComponent<BoxCollider2D>();
        cc.size = new Vector2(0.95f, 1.25f);
        cc.offset = new Vector2(0f, 0.5f);
        SpriteRenderer ring = Art(root.transform, "Ring", "shock_ring", 8, false).GetComponent<SpriteRenderer>();

        S41F_Goal(root.transform, "REACH THE FLOATING DOOR");
        SetField(level, "climber", climber);
        SetField(level, "climberArea", cc);
        SetArray(level, "steps", steps.ToArray());
        SetArray(level, "stepArts", stepArts.ToArray());
        SetField(level, "ring", ring);
        SetField(level, "door", door);
        SetField(level, "doorArt", doorArt);
        SetField(level, "doorOpen", sprites["s41f_cloud_door_open"]);
        SetField(level, "triesText", S41F_Info(root.transform, "Tries", new Vector2(0f, -4.2f), 4.4f));
        S41F_Common(level, root.transform, new Vector2(-1.2f, -1.9f));
        return SaveLevel(root, "TheInvisibleStairs");
    }

    // ---------- 153 Lie detector ----------

    static GameObject BuildS41FLieDetector()
    {
        var root = new GameObject("Level_LieDetector");
        LieDetectorLevel level = root.AddComponent<LieDetectorLevel>();
        Art(root.transform, "Room", "s41f_interro_bg", -20, false);
        Transform robot = S41F_Holder(root.transform, "Robot", new Vector2(0f, -1.55f), 1.2f);
        Art(robot, "Art", "s41f_robot", 4, false);
        SpriteRenderer face = Art(robot, "Face", "s41f_robot_face", 5, false, new Vector2(0f, 1.58f)).GetComponent<SpriteRenderer>();
        BoxCollider2D rc = robot.gameObject.AddComponent<BoxCollider2D>();
        rc.size = new Vector2(2.0f, 2.4f);
        rc.offset = new Vector2(0f, 1.2f);
        SmokeEmitter smoke = Emitter(root.transform, "Smoke", new Vector2(0f, 1.0f), "puff", 9f, 1.2f, 0.9f, 0.5f, 0.5f, new Color(0.55f, 0.55f, 0.6f, 0.85f), 12);

        Transform q = Child(root.transform, "Question", new Vector2(0f, 2.15f)).transform;
        Bar(q, "Back", Vector2.zero, new Vector2(5.2f, 0.75f), new Color(0.08f, 0.12f, 0.18f, 0.85f), 20);
        TextMesh question = Fit(Label(q, Vector2.zero, 21, "", 0.07f), 4.9f, 0.55f);
        question.color = new Color(0.5f, 1f, 0.7f);
        TextMesh verdict = Fit(Label(root.transform, new Vector2(-1.85f, 0.9f), 22, "", 0.11f), 1.8f, 0.7f);
        verdict.color = new Color(1f, 0.3f, 0.3f);

        Transform det = Child(root.transform, "Detector", new Vector2(1.6f, -3.1f)).transform;
        Art(det, "Art", "s41f_polygraph", 8, false);
        Transform needle = Child(det, "Needle", new Vector2(0.35f, -0.2f)).transform;
        Art(needle, "Art", "s41f_poly_needle", 9, false);
        Transform lamp = Art(det, "Lamp", "glow", 9, false, new Vector2(0.55f, 0.5f));
        lamp.localScale = Vector3.one * 0.6f;
        det.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.7f, 1.3f);
        Transform clamp = Child(root.transform, "ClampSpot", new Vector2(0f, -0.6f)).transform;

        Transform yes = S41F_Pill(root.transform, "Yes", new Vector2(-1.8f, -3.1f), "YES", new Color(0.6f, 1f, 0.65f), 10, out _, out Collider2D yesArea);
        Transform no = S41F_Pill(root.transform, "No", new Vector2(-0.15f, -3.1f), "NO", new Color(1f, 0.62f, 0.62f), 10, out _, out Collider2D noArea);

        S41F_Goal(root.transform, "SURVIVE THE INTERROGATION");
        SetField(level, "robot", robot);
        SetField(level, "robotArea", rc);
        SetField(level, "faceArt", face);
        SetField(level, "panicFace", sprites["s41f_robot_face_panic"]);
        SetField(level, "detector", det);
        SetField(level, "detectorArea", det.GetComponent<Collider2D>());
        SetField(level, "needle", needle);
        SetField(level, "lamp", lamp.GetComponent<SpriteRenderer>());
        SetField(level, "yesButton", yes);
        SetField(level, "noButton", no);
        SetField(level, "yesArea", yesArea);
        SetField(level, "noArea", noArea);
        SetField(level, "questionText", question);
        SetField(level, "verdictText", verdict);
        SetField(level, "triesText", S41F_Info(root.transform, "Tries", new Vector2(0f, -4.2f), 2.6f));
        SetField(level, "smoke", smoke);
        SetField(level, "clampSpot", clamp);
        S41F_Common(level, root.transform, new Vector2(1.3f, 1.0f));
        return SaveLevel(root, "LieDetector");
    }

    // ---------- 154 Too bright ----------

    static GameObject BuildS41FTooBright()
    {
        var root = new GameObject("Level_TooBright");
        TooBrightLevel level = root.AddComponent<TooBrightLevel>();
        Art(root.transform, "Room", "s41f_bright_bg", -20, false);
        var curtains = new List<Transform>();
        var windows = new List<Collider2D>();
        var beams = new List<SpriteRenderer>();
        foreach (float cx in new[] { -1.25f, 1.2f })
        {
            beams.Add(Art(root.transform, "Beam", "s41f_sunbeam", 1, false, new Vector2(cx, 0.45f)).GetComponent<SpriteRenderer>());
            Transform c = Child(root.transform, "Curtain", new Vector2(cx - 0.82f, 2.46f)).transform;
            Art(c, "Art", "s41f_curtain", 3, false);
            curtains.Add(c);
            BoxCollider2D w = Child(root.transform, "Window", new Vector2(cx, 1.45f)).AddComponent<BoxCollider2D>();
            w.size = new Vector2(1.9f, 2.3f);
            windows.Add(w);
        }
        Transform coffin = Child(root.transform, "Coffin", new Vector2(2.3f, -3.4f)).transform;
        Art(coffin, "Art", "s41f_coffin", 4, false);
        Transform vamp = Child(root.transform, "Vampire", new Vector2(-2.3f, -3.4f)).transform;
        SpriteRenderer vampArt = Art(vamp, "Art", "s41f_vampire", 6, false).GetComponent<SpriteRenderer>();
        BoxCollider2D vc = vamp.gameObject.AddComponent<BoxCollider2D>();
        vc.size = new Vector2(1.3f, 1.5f);
        vc.offset = new Vector2(0f, 0.6f);
        SmokeEmitter smoke = Emitter(vamp, "Smoke", new Vector2(0f, 0.7f), "puff", 10f, 0.9f, 1.0f, 0.35f, 0.35f, new Color(0.8f, 0.8f, 0.85f, 0.85f), 7);
        Transform meter = GaugeBar(root.transform, "Sun", new Vector2(0.35f, 2.85f), new Vector2(3.6f, 0.18f), new Color(0.15f, 0.12f, 0.2f, 0.7f), new Color(0.65f, 0.45f, 1f), 30);
        TextMesh sunLabel = Fit(Label(root.transform, new Vector2(-2.15f, 2.85f), 31, "LIFE", 0.04f), 0.8f, 0.3f);
        sunLabel.color = new Color(0.2f, 0.1f, 0.3f);

        S41F_Goal(root.transform, "GET THE VAMPIRE TO HIS COFFIN");
        SetArray(level, "curtains", curtains.ToArray());
        SetArray(level, "windowAreas", windows.ToArray());
        SetArray(level, "beams", beams.ToArray());
        SetField(level, "vampire", vamp);
        SetField(level, "vampireArea", vc);
        SetField(level, "vampireArt", vampArt);
        SetField(level, "idleSprite", sprites["s41f_vampire"]);
        SetField(level, "burntSprite", sprites["s41f_vampire_burnt"]);
        SetField(level, "smoke", smoke);
        SetField(level, "sunMeter", meter);
        SetField(level, "coffin", coffin);
        S41F_Common(level, root.transform, new Vector2(-1.3f, -1.6f));
        return SaveLevel(root, "TooBright");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V41F = new Dictionary<string, string[]>
    {
        { "the_wrong_shadow", new[] { "Light changes shadows.", "Move the lamp to put the shadow behind the cat." } },
        { "the_impossible_scale", new[] { "The feather can change.", "Enlarge the feather until the scale balances." } },
        { "rewind_reality", new[] { "Time can move backwards.", "Rewind before the fall and move the vase." } },
        { "the_sticky_finger", new[] { "Move the destination instead.", "Hold the coin and drag the safe beneath it." } },
        { "the_guilty_button", new[] { "Watch how they react under pressure.", "Hold each button and identify the metallic tear." } },
        { "the_invisible_stairs", new[] { "Touch the empty space.", "Locate the four invisible platforms and follow them." } },
        { "lie_detector", new[] { "Test the interrogator.", "Drag the detector onto the robot." } },
        { "too_bright", new[] { "Make the room darker.", "Close the curtains over the windows, then lead the vampire to the coffin." } },
    };
}
#endif
