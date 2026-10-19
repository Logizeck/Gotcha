#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli nuovi della versione 0.39 (gruppo B).</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels39B() => new List<LevelDefinition>
    {
        Define("Crane", "sky_crane", "SKY CRANE", "Tap to drop the crate. Only what lands on the stack stays.", BuildCraneLevel()),
        Define("Fishing", "gone_fishing", "GONE FISHING", "Not every wobble is a fish.", BuildFishingLevel()),
        Define("Sheep", "count_sheep", "COUNT THE SHEEP", "Count only the real sheep that END UP on the right.", BuildSheepLevel()),
        Define("LockPattern", "lock_screen", "LOCK SCREEN", "Look closely at the glass. Fingers leave traces.", BuildLockPatternLevel()),
        Define("Giraffe", "tall_guest", "TALL GUEST", "The screen is too short. Is it, though?", BuildGiraffeLevel()),
    };

    /// <summary>Contenitore vuoto con uno sprite figlio (così il codice può muovere/scalare il contenitore).</summary>
    static Transform Holder39B(Transform parent, string name, string sprite, Vector2 pos, int order, float scale = 1f)
    {
        Transform h = Child(parent, name, pos).transform;
        h.localScale = Vector3.one * scale;
        Art(h, "Art", sprite, order, false);
        return h;
    }

    static Transform[] Hearts39B(Transform parent, Vector2 first, float step)
    {
        var list = new Transform[3];
        for (int i = 0; i < 3; i++) list[i] = Holder39B(parent, "Heart" + i, "heart", first + new Vector2(step * i, 0f), 40, 1.1f);
        return list;
    }

    // ---------- Sky Crane ----------

    static GameObject BuildCraneLevel()
    {
        var root = new GameObject("Level_Crane");
        CraneLevel level = root.AddComponent<CraneLevel>();
        Art(root.transform, "Sky", "crane_bg", -20, false);
        Bar(root.transform, "Base", new Vector2(-0.5f, -3.68f), new Vector2(1.5f, 0.16f), new Color(0.55f, 0.57f, 0.64f), 2);
        Transform stack = Child(root.transform, "Stack", Vector2.zero).transform;
        SpriteRenderer crate = Art(root.transform, "CrateTemplate", "crane_crate", 6, false).GetComponent<SpriteRenderer>();
        crate.drawMode = SpriteDrawMode.Sliced;
        crate.size = new Vector2(1.5f, 0.72f);
        Transform hook = Holder39B(root.transform, "Hook", "crane_hook", new Vector2(-0.5f, 2.66f), 7);
        LineRenderer rope = Line(root.transform, "Rope", 0.035f, new Color(0.17f, 0.13f, 0.2f), 5);
        Transform cat = Holder39B(root.transform, "Cat", "roof_cat", new Vector2(1.6f, 0.87f), 9, 0.85f);
        TextMesh perfect = Label(root.transform, Vector2.zero, 30, "PERFECT!", 0.07f);
        perfect.color = new Color(1f, 0.85f, 0.2f);
        TextMesh counter = Label(root.transform, new Vector2(1.65f, 2.45f), 21, "", 0.06f);

        SetField(level, "crateTemplate", crate);
        SetField(level, "hook", hook);
        SetField(level, "rope", rope);
        SetField(level, "stack", stack);
        SetField(level, "cat", cat);
        SetField(level, "perfectLabel", perfect);
        SetField(level, "counter", counter);
        SetArray(level, "hearts", Hearts39B(root.transform, new Vector2(1.2f, 2.95f), 0.45f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.9f, 1.95f)));
        return SaveLevel(root, "Crane");
    }

    // ---------- Gone Fishing ----------

    static GameObject BuildFishingLevel()
    {
        var root = new GameObject("Level_Fishing");
        FishingLevel level = root.AddComponent<FishingLevel>();
        Art(root.transform, "Lake", "lake_bg", -20, false);
        Vector2 rest = new Vector2(0.5f, 0.4f);
        Transform rod = Child(root.transform, "Rod", new Vector2(-2.3f, -4.4f)).transform;
        Art(rod, "Art", "rod_long", 8, false);
        Transform tip = Child(rod, "Tip", new Vector2(2.3f, 3.1f)).transform;
        LineRenderer line = Line(root.transform, "Line", 0.02f, new Color(1f, 1f, 1f, 0.85f), 7);
        Transform leaf = Holder39B(root.transform, "Leaf", "pond_leaf", rest, 3);
        Transform ripple = Holder39B(root.transform, "Ripple", "ripple", rest, 4);
        Transform bobber = Holder39B(root.transform, "Bobber", "bobber", rest, 5);
        Transform splash = Holder39B(root.transform, "Splash", "splash", rest, 6);
        Transform alert = Holder39B(root.transform, "Alert", "alert", rest + new Vector2(0f, 1.0f), 9, 0.8f);
        Transform frog = Holder39B(root.transform, "Frog", "pond_frog", new Vector2(-1.9f, -1.0f), 5, 0.9f);
        Transform duck = Holder39B(root.transform, "Duck", "pond_duck", rest + new Vector2(3.3f, 0f), 5, 0.9f);
        Transform fish = Holder39B(root.transform, "CaughtFish", "fish", rest, 12, 2.2f);
        Transform boot = Holder39B(root.transform, "CaughtBoot", "fish_boot", rest, 12, 1.5f);
        var icons = new List<SpriteRenderer>();
        for (int i = 0; i < 3; i++)
        {
            Transform ic = Child(root.transform, "Catch" + i, new Vector2(-2.2f + i * 0.75f, 3.15f)).transform;
            ic.localScale = Vector3.one * 1.4f;
            icons.Add(Art(ic, "Art", "fish", 20, false).GetComponent<SpriteRenderer>());
        }

        SetField(level, "bobber", bobber);
        SetField(level, "rod", rod);
        SetField(level, "rodTip", tip);
        SetField(level, "line", line);
        SetField(level, "ripple", ripple);
        SetField(level, "splash", splash);
        SetField(level, "alertMark", alert);
        SetField(level, "frog", frog);
        SetField(level, "duck", duck);
        SetField(level, "leaf", leaf);
        SetField(level, "caughtFish", fish);
        SetField(level, "caughtBoot", boot);
        SetArray(level, "catchIcons", icons.ToArray());
        SetArray(level, "hearts", Hearts39B(root.transform, new Vector2(1.3f, 3.15f), 0.45f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0f, 2.45f)));
        return SaveLevel(root, "Fishing");
    }

    // ---------- Count the Sheep ----------

    static GameObject BuildSheepLevel()
    {
        var root = new GameObject("Level_Sheep");
        SheepLevel level = root.AddComponent<SheepLevel>();
        Art(root.transform, "Night", "sheep_night_bg", -20, false);
        Transform kid = Child(root.transform, "Kid", new Vector2(-0.2f, 2.25f)).transform;
        GameObject awake = Art(kid, "Awake", "kid_bed", 3, false).gameObject;
        GameObject asleep = Art(kid, "Asleep", "kid_bed_sleep", 3, false).gameObject;
        Transform zzz = Holder39B(kid, "Zzz", "zzz", new Vector2(-0.4f, 0.75f), 6);
        Holder39B(root.transform, "Fence", "sheep_fence", new Vector2(0f, -2.35f), 4);
        Transform sheep = Holder39B(root.transform, "SheepTemplate", "sheep", new Vector2(-5f, -2.35f), 6);
        Transform wolf = Holder39B(root.transform, "WolfTemplate", "sheep_wolf", new Vector2(-5f, -2.35f), 6);
        Transform actors = Child(root.transform, "Actors", Vector2.zero).transform;

        // tastierino
        Transform pad = Child(root.transform, "Pad", Vector2.zero).transform;
        Bar(pad, "Dim", new Vector2(0f, -1.75f), new Vector2(5.2f, 4.5f), new Color(0.1f, 0.1f, 0.25f, 0.88f), 30);
        Bar(pad, "Screen", new Vector2(0f, -0.05f), new Vector2(2.0f, 0.62f), new Color(0.06f, 0.08f, 0.2f), 31);
        TextMesh display = Fit(Label(pad, new Vector2(0f, -0.05f), 33, "_", 0.1f), 1.8f, 0.55f);
        display.color = Color.white;
        var keys = new TapButton[10];
        TapButton Key(string name, string text, Vector2 pos, Color tint)
        {
            Transform k = Child(pad, name, pos).transform;
            Transform art = Art(k, "Art", "keypad_key", 33, false);
            art.localScale = Vector3.one * 2.6f;
            art.GetComponent<SpriteRenderer>().color = tint;
            Fit(Label(art, new Vector2(0f, 0.01f), 34, text, 0.05f), 0.3f, 0.25f);
            k.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.25f, 0.85f);
            TapButton tb = k.gameObject.AddComponent<TapButton>();
            tb.id = name;
            tb.visual = art;
            return tb;
        }
        for (int n = 1; n <= 9; n++) keys[n] = Key("Key" + n, n.ToString(), new Vector2(-1.4f + ((n - 1) % 3) * 1.4f, -0.85f - ((n - 1) / 3) * 0.9f), Color.white);
        keys[0] = Key("Key0", "0", new Vector2(0f, -3.55f), Color.white);
        TapButton del = Key("Del", "DEL", new Vector2(-1.4f, -3.55f), new Color(1f, 0.75f, 0.7f));
        TapButton ok = Key("Ok", "OK", new Vector2(1.4f, -3.55f), new Color(0.7f, 1f, 0.7f));

        SetField(level, "sheepTemplate", sheep);
        SetField(level, "wolfTemplate", wolf);
        SetField(level, "actors", actors);
        SetField(level, "kidAwake", awake);
        SetField(level, "kidAsleep", asleep);
        SetField(level, "zzzTemplate", zzz);
        SetField(level, "pad", pad.gameObject);
        SetArray(level, "keys", keys);
        SetField(level, "delKey", del);
        SetField(level, "okKey", ok);
        SetField(level, "display", display);
        SetArray(level, "hearts", Hearts39B(root.transform, new Vector2(1.75f, 3.3f), 0.42f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0f, 0.85f)));
        return SaveLevel(root, "Sheep");
    }

    // ---------- Lock Screen ----------

    static GameObject BuildLockPatternLevel()
    {
        var root = new GameObject("Level_LockPattern");
        LockPatternLevel level = root.AddComponent<LockPatternLevel>();
        Art(root.transform, "Table", "table_bg", -20, false);
        Fit(Label(root.transform, new Vector2(0f, 3.35f), 20, "Mom's phone. Unlock it to order pizza!", 0.045f), 5.4f);
        Transform phone = Child(root.transform, "Phone", new Vector2(0f, -0.5f)).transform;
        Art(phone, "Body", "phone_lock", 2, false);
        Transform lockScreen = Child(phone, "LockScreen", Vector2.zero).transform;
        GameObject closed = Holder39B(lockScreen, "Padlock", "padlock", new Vector2(0f, 2.55f), 4).gameObject;
        GameObject open = Holder39B(lockScreen, "PadlockOpen", "padlock_open", new Vector2(0f, 2.55f), 4).gameObject;
        TextMesh clock = Label(lockScreen, new Vector2(0f, 1.75f), 4, "9:41", 0.14f);
        clock.color = Color.white;
        TextMesh date = Fit(Label(lockScreen, new Vector2(0f, 1.15f), 4, "Friday - pizza night?", 0.04f), 2.8f);
        date.color = new Color(1f, 1f, 1f, 0.8f);
        TextMesh status = Fit(Label(phone, new Vector2(0f, 0.45f), 5, "", 0.045f), 2.8f);
        status.color = Color.white;
        var dots = new List<Transform>();
        var rings = new List<SpriteRenderer>();
        for (int i = 0; i < 9; i++)
        {
            Transform d = Child(phone, "Dot" + i, new Vector2((i % 3 - 1) * 0.95f, -0.4f - (i / 3) * 0.95f)).transform;
            Art(d, "Art", "lock_dot", 5, false);
            rings.Add(Art(d, "Ring", "lock_ring", 6, false).GetComponent<SpriteRenderer>());
            dots.Add(d);
        }
        Transform smudge = Holder39B(phone, "Smudge", "lock_smudge", Vector2.zero, 7);
        Transform print = Holder39B(phone, "Print", "lock_print", Vector2.zero, 8, 1.3f);
        LineRenderer trace = Line(root.transform, "Trace", 0.09f, Color.white, 9);
        Transform app = Holder39B(phone, "PizzaApp", "pizza_app", Vector2.zero, 10);
        Fit(Label(app, new Vector2(0f, 2.62f), 11, "PIZZA NOW", 0.08f), 2.6f).color = Color.white;
        Fit(Label(app, new Vector2(0f, -0.85f), 11, "1x Large Pepperoni", 0.045f), 2.7f);
        Transform order = Child(app, "Order", new Vector2(0f, -1.9f)).transform;
        Fit(Label(order, Vector2.zero, 12, "ORDER", 0.08f), 2.0f).color = Color.white;

        SetField(level, "phone", phone);
        SetArray(level, "dots", dots.ToArray());
        SetArray(level, "rings", rings.ToArray());
        SetField(level, "trace", trace);
        SetField(level, "smudgeTemplate", smudge);
        SetField(level, "fingerprint", print);
        SetField(level, "lockScreen", lockScreen.gameObject);
        SetField(level, "padlockClosed", closed);
        SetField(level, "padlockOpen", open);
        SetField(level, "pizzaApp", app);
        SetField(level, "orderButton", order);
        SetField(level, "status", status);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0f, -4.35f)));
        return SaveLevel(root, "LockPattern");
    }

    // ---------- Tall Guest ----------

    static GameObject BuildGiraffeLevel()
    {
        var root = new GameObject("Level_Giraffe");
        GiraffeLevel level = root.AddComponent<GiraffeLevel>();
        Transform world = Child(root.transform, "World", Vector2.zero).transform;

        // telefono in verticale: la testa esce dallo schermo
        Transform portrait = Child(world, "Portrait", Vector2.zero).transform;
        Art(portrait, "Hall", "giraffe_hall_bg", -20, false);
        Transform g = Child(portrait, "Giraffe", new Vector2(-0.9f, -3.3f)).transform;
        Transform neck = Child(g, "Neck", new Vector2(0.75f, 1.85f)).transform;
        neck.localScale = new Vector3(1f, 1.6f, 1f);
        Art(neck, "Art", "giraffe_neck", 3, false);
        Art(g, "Body", "giraffe_body", 4, false);
        BoxCollider2D hit = g.gameObject.AddComponent<BoxCollider2D>();
        hit.size = new Vector2(3.0f, 2.4f);
        hit.offset = new Vector2(0f, 1.15f);
        SpeechBubble bubble = SmallBubble(portrait, new Vector2(1.3f, 0.9f));

        // telefono di lato: scena larga
        Transform landscape = Child(world, "Landscape", Vector2.zero).transform;
        Art(landscape, "Hall", "party_wide_bg", -19, false);
        Art(landscape, "WallPiece", "party_wall_piece", 10, false);
        Transform gw = Child(landscape, "Giraffe", new Vector2(-2.6f, -2.35f)).transform;
        gw.localScale = Vector3.one * 0.75f;
        Transform neckW = Child(gw, "Neck", new Vector2(0.75f, 1.85f)).transform;
        Art(neckW, "Art", "giraffe_neck", 3, false);
        Transform headW = Child(neckW, "Head", new Vector2(0f, 5.5f)).transform;
        Art(headW, "Art", "giraffe_head", 5, false);
        Art(gw, "Body", "giraffe_body", 4, false);
        Transform door = Child(landscape, "DoorPoint", new Vector2(3.3f, -0.6f)).transform;
        Transform confetti = Holder39B(landscape, "Confetti", "confetti", new Vector2(3.3f, -0.6f), 20, 2f);
        SpeechBubble bubbleWide = SmallBubble(landscape, new Vector2(-0.6f, 2.1f));

        Transform hint = Holder39B(root.transform, "RotateHint", "rotate_phone", new Vector2(1.7f, 2.4f), 45, 1.2f);

        SetField(level, "world", world);
        SetField(level, "portrait", portrait.gameObject);
        SetField(level, "landscape", landscape.gameObject);
        SetField(level, "giraffeHit", hit);
        SetField(level, "giraffeWide", gw);
        SetField(level, "neckWide", neckW);
        SetField(level, "headWide", headW);
        SetField(level, "rotateHint", hint);
        SetField(level, "confettiTemplate", confetti);
        SetField(level, "doorPoint", door);
        SetField(level, "bubble", bubble);
        SetField(level, "bubbleWide", bubbleWide);
        return SaveLevel(root, "Giraffe");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V39B = new Dictionary<string, string[]>
    {
        { "sky_crane", new[] { "Wait until the crate is right above the tower, then tap.", "Tap when the swinging crate lines up with the top crate: perfect drops don't shrink." } },
        { "gone_fishing", new[] { "Twitches, frogs, leaves and ducks are fake-outs.", "Tap only when the bobber goes fully under with a splash and a \"!\". The boot is free." } },
        { "count_sheep", new[] { "The wolf doesn't count, the one that turns back doesn't count. Watch the bottom of the fence!", "Count sheep that end on the right side, plus the tiny lamb running UNDER the fence." } },
        { "lock_screen", new[] { "The greasy smudges on the screen draw the path.", "Start at the big fingerprint and follow the smudges from dot to dot without lifting." } },
        { "tall_guest", new[] { "You can't zoom. But you can turn something.", "Turn your phone sideways (landscape) and hold it there." } },
    };
}
#endif
