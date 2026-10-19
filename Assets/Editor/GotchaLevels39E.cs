#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli nuovi della versione 0.39 (gruppo E): colori, pluriball, chitarra, gru.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels39E() => new List<LevelDefinition>
    {
        Define("Paint", "paint_mixer", "PAINT MIXER", "Tap the tubes to drip paint into the bucket. Watch the MATCH meter.", BuildPaintMixerLevel()),
        Define("BubbleWrap", "bubble_wrap", "BUBBLE WRAP", "Pop every bubble. Every. Single. One.", BuildBubbleWrapLevel()),
        Define("Guitar", "out_of_tune", "OUT OF TUNE", "Pluck a string, play the tuning fork, turn the peg until they match.", BuildOutOfTuneLevel()),
        Define("Claw", "claw_machine", "CLAW MACHINE", "Move the claw, press DROP. Win the plush with the golden key!", BuildClawMachineLevel()),
    };

    /// <summary>Pulsante di testo a pillola (collider 1.3×0.7) con l'etichetta sopra.</summary>
    static TapButton E39TextButton(Transform parent, string name, string text, Vector2 pos, Color tint)
    {
        GameObject go = Child(parent, name, pos);
        Transform art = Art(go.transform, "Art", "e39_pill", 6, false);
        art.localScale = Vector3.one * 0.9f;
        art.GetComponent<SpriteRenderer>().color = tint;
        Fit(Label(art, new Vector2(0f, 0.02f), 7, text, 0.05f), 1.1f, 0.4f);
        go.AddComponent<BoxCollider2D>().size = new Vector2(1.35f, 0.75f);
        TapButton b = go.AddComponent<TapButton>();
        b.id = name;
        b.visual = art;
        return b;
    }

    /// <summary>Quattro getti di coriandoli dall'alto (spenti).</summary>
    static SmokeEmitter[] E39Confetti(Transform root)
    {
        var list = new List<SmokeEmitter>();
        foreach (Color c in new[] { new Color(1f, 0.36f, 0.54f), new Color(1f, 0.82f, 0.35f), new Color(0.43f, 0.78f, 1f), new Color(0.5f, 0.83f, 0.42f) })
        {
            SmokeEmitter e = Emitter(root, "Confetti", new Vector2(0f, 3.6f), "confetti", 22f, 2.4f, -1.6f, 2.8f, 1.4f, c, 50);
            e.sideDrift = 0.6f;
            e.growth = 0f;
            e.spinRange = 300f;
            list.Add(e);
        }
        return list.ToArray();
    }

    // ---------- Paint mixer ----------

    static GameObject BuildPaintMixerLevel()
    {
        var root = new GameObject("Level_Paint");
        PaintMixerLevel level = root.AddComponent<PaintMixerLevel>();
        Art(root.transform, "Studio", "mixer_studio_bg", -20, false);

        // cavalletto con il campione
        Transform easel = Child(root.transform, "Easel", new Vector2(1.2f, 1.0f)).transform;
        Art(easel, "Art", "mixer_easel", 2, false);
        SpriteRenderer swatch = Art(easel, "Swatch", "mixer_swatch", 3, false, new Vector2(0f, 0.12f)).GetComponent<SpriteRenderer>();
        SpriteRenderer painting = Art(easel, "Painting", "mixer_swatch", 4, false, new Vector2(0f, 0f)).GetComponent<SpriteRenderer>();
        painting.transform.localScale = new Vector3(1.4f, 1.15f, 1f);
        SpriteRenderer fish = Art(easel, "Fish", "mixer_fish_paint", 4, false, new Vector2(0f, 0.05f)).GetComponent<SpriteRenderer>();
        TextMesh canvasLabel = Fit(Label(easel, new Vector2(0f, -0.5f), 5, "", 0.035f), 1.55f, 1.2f);

        // il gatto pittore col braccio che pennella
        Transform cat = Child(root.transform, "Cat", new Vector2(-1.35f, 0.25f)).transform;
        cat.localScale = Vector3.one * 1.15f;
        Art(cat, "Art", "mixer_cat", 3, false);
        Transform arm = Child(cat, "Arm", new Vector2(0.3f, -0.15f)).transform;
        Art(arm, "Art", "mixer_cat_arm", 4, false);

        // misuratore
        Label(root.transform, new Vector2(-1.95f, 3.25f), 10, "MATCH", 0.045f);
        Transform meter = GaugeBar(root.transform, "Meter", new Vector2(0.15f, 3.25f), new Vector2(2.5f, 0.22f), new Color(0.17f, 0.13f, 0.2f, 0.7f), Color.white, 10);
        SpriteRenderer meterColor = meter.GetComponentInChildren<SpriteRenderer>();
        TextMesh matchText = Fit(Label(root.transform, new Vector2(2.1f, 3.25f), 10, "--", 0.05f), 0.8f, 0.4f);
        TextMesh roundText = Fit(Label(root.transform, new Vector2(1.2f, 2.05f), 10, "", 0.04f), 1.8f, 0.3f);

        // secchio
        Transform bucket = Child(root.transform, "Bucket", new Vector2(0f, -1.55f)).transform;
        Art(bucket, "Art", "mixer_bucket", 2, false);
        SpriteRenderer surface = Art(bucket, "Paint", "mixer_paint_surface", 3, false, new Vector2(0f, -0.14f)).GetComponent<SpriteRenderer>();
        surface.gameObject.SetActive(false);
        SpriteRenderer ripple = Art(bucket, "Ripple", "mixer_ripple", 4, false, new Vector2(0f, -0.14f)).GetComponent<SpriteRenderer>();
        SpriteRenderer drop = Art(root.transform, "Drop", "mixer_drop", 8, false).GetComponent<SpriteRenderer>();

        // tubetti
        string[] colors = { "red", "yellow", "blue", "white", "black" };
        var tubes = new List<TapButton>();
        for (int i = 0; i < colors.Length; i++)
        {
            GameObject t = Child(root.transform, "Tube_" + colors[i], new Vector2(-2.2f + i * 1.1f, -3.65f));
            Transform art = Art(t.transform, "Art", "mixer_tube_" + colors[i], 5, false);
            t.AddComponent<BoxCollider2D>().size = new Vector2(1.0f, 1.5f);
            TapButton b = t.AddComponent<TapButton>();
            b.id = colors[i];
            b.visual = art;
            tubes.Add(b);
        }
        TapButton empty = E39TextButton(root.transform, "Empty", "EMPTY", new Vector2(-1.95f, -1.75f), new Color(0.6f, 0.8f, 1f));
        TextMesh dropsText = Fit(Label(root.transform, new Vector2(1.95f, -1.75f), 10, "", 0.04f), 1.4f, 0.4f);
        dropsText.color = Color.white;

        SetArray(level, "tubes", tubes.ToArray());
        SetField(level, "emptyButton", empty);
        SetField(level, "surface", surface);
        SetField(level, "dropTemplate", drop);
        SetField(level, "rippleTemplate", ripple);
        SetField(level, "swatch", swatch);
        SetField(level, "painting", painting);
        SetField(level, "paintedFish", fish);
        SetField(level, "canvasLabel", canvasLabel);
        SetField(level, "matchText", matchText);
        SetField(level, "roundText", roundText);
        SetField(level, "dropsText", dropsText);
        SetField(level, "meterFill", meter);
        SetField(level, "meterColor", meterColor);
        SetField(level, "brushArm", arm);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.1f, 2.45f)));
        return SaveLevel(root, "Paint");
    }

    // ---------- Bubble wrap ----------

    static GameObject BuildBubbleWrapLevel()
    {
        var root = new GameObject("Level_BubbleWrap");
        BubbleWrapLevel level = root.AddComponent<BubbleWrapLevel>();
        Bar(root.transform, "Table", Vector2.zero, new Vector2(12f, 16f), new Color(0.86f, 0.7f, 0.5f), -20);
        Bar(root.transform, "Shadow", new Vector2(0.12f, -0.7f), new Vector2(4.8f, 5.6f), new Color(0f, 0f, 0f, 0.15f), -5);
        TextMesh counter = Fit(Label(root.transform, new Vector2(0f, 3.1f), 10, "", 0.07f), 5f, 0.6f);

        Transform sheet = Child(root.transform, "Sheet", new Vector2(0f, -0.55f)).transform;
        GameObject front = Child(sheet, "Front", Vector2.zero);
        Art(front.transform, "Art", "wrap_sheet_front", 2, false);
        var bubbles = new List<SpriteRenderer>();
        for (int r = 0; r < 7; r++)
            for (int c = 0; c < 6; c++)
                bubbles.Add(Art(front.transform, "Bubble", "wrap_bubble", 3, false, new Vector2((c - 2.5f) * 0.74f, (r - 3f) * 0.74f)).GetComponent<SpriteRenderer>());
        GameObject back = Child(sheet, "Back", Vector2.zero);
        Art(back.transform, "Art", "wrap_sheet_back", 2, false);
        SpriteRenderer backBubble = Art(back.transform, "Bubble", "wrap_bubble", 3, false, new Vector2(1.11f, -1.48f)).GetComponent<SpriteRenderer>();

        SetField(level, "sheet", sheet);
        SetField(level, "front", front);
        SetField(level, "back", back);
        SetArray(level, "bubbles", bubbles.ToArray());
        SetField(level, "backBubble", backBubble);
        SetField(level, "poppedSprite", sprites["wrap_bubble_popped"]);
        SetField(level, "counter", counter);
        SetArray(level, "confetti", E39Confetti(root.transform));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0f, 2.55f)));
        return SaveLevel(root, "BubbleWrap");
    }

    // ---------- Out of tune ----------

    static GameObject BuildOutOfTuneLevel()
    {
        var root = new GameObject("Level_Guitar");
        OutOfTuneLevel level = root.AddComponent<OutOfTuneLevel>();
        Art(root.transform, "Stage", "tune_stage_bg", -20, false);
        Art(root.transform, "Guitar", "tune_guitar", 0, false);

        float[] xs = { -0.75f, 0f, 0.75f };
        float[] postY = { 2.75f, 1.9f, 1.05f };
        var strings = new List<LineRenderer>();
        var glows = new List<LineRenderer>();
        var bridges = new List<Transform>();
        var nuts = new List<Transform>();
        var posts = new List<Transform>();
        var pegs = new List<Transform>();
        for (int i = 0; i < 3; i++)
        {
            strings.Add(Line(root.transform, "String" + i, 0.045f + (2 - i) * 0.012f, new Color(0.85f, 0.87f, 0.92f), 3));
            glows.Add(Line(root.transform, "Glow" + i, 0.18f, new Color(1f, 0.9f, 0.3f, 0.5f), 2));
            bridges.Add(Child(root.transform, "Bridge" + i, new Vector2(xs[i], -3.3f)).transform);
            nuts.Add(Child(root.transform, "Nut" + i, new Vector2(xs[i], 0.6f)).transform);
            posts.Add(Child(root.transform, "Post" + i, new Vector2(0.45f, postY[i])).transform);
            Transform peg = Child(root.transform, "Peg" + i, new Vector2(1.75f, postY[i])).transform;
            Art(peg, "Art", "tune_peg", 6, false);
            pegs.Add(peg);
        }

        // accordatore
        Art(root.transform, "Meter", "tune_meter", 5, false, new Vector2(-1.8f, 2.3f));
        Transform needle = Child(root.transform, "Needle", new Vector2(-1.8f, 2.3f)).transform;
        Bar(needle, "Hand", new Vector2(0f, 0.32f), new Vector2(0.04f, 0.64f), new Color(0.9f, 0.2f, 0.2f), 7);
        Bar(root.transform, "Pin", new Vector2(-1.8f, 2.3f), new Vector2(0.1f, 0.1f), new Color(0.17f, 0.13f, 0.2f), 8);
        TextMesh meterLabel = Fit(Label(root.transform, new Vector2(-1.8f, 1.75f), 8, "PLUCK!", 0.045f), 1.6f, 0.4f);
        meterLabel.color = Color.white;
        TextMesh tuned = Fit(Label(root.transform, new Vector2(-1.8f, 1.25f), 8, "TUNED 0/3", 0.05f), 1.6f, 0.4f);
        tuned.color = Color.white;

        // diapason
        GameObject forkGo = Child(root.transform, "Fork", new Vector2(2.0f, -3.25f));
        Transform forkArt = Art(forkGo.transform, "Art", "tune_fork", 6, false);
        SpriteRenderer beat = Art(forkGo.transform, "Beat", "glow", 5, false, new Vector2(0f, 0.15f)).GetComponent<SpriteRenderer>();
        forkGo.AddComponent<BoxCollider2D>().size = new Vector2(1.1f, 1.5f);
        TapButton fork = forkGo.AddComponent<TapButton>();
        fork.id = "fork";
        fork.visual = forkArt;
        TextMesh forkLabel = Fit(Label(root.transform, new Vector2(2.0f, -4.2f), 8, "", 0.04f), 1.3f, 0.35f);
        forkLabel.color = Color.white;

        Transform cat = Child(root.transform, "Cat", new Vector2(-1.95f, -3.4f)).transform;
        Art(cat, "Art", "tune_cat", 6, false);
        SpriteRenderer note = Art(root.transform, "Note", "note", 9, false).GetComponent<SpriteRenderer>();

        SetArray(level, "strings", strings.ToArray());
        SetArray(level, "glows", glows.ToArray());
        SetArray(level, "bridges", bridges.ToArray());
        SetArray(level, "nuts", nuts.ToArray());
        SetArray(level, "posts", posts.ToArray());
        SetArray(level, "pegs", pegs.ToArray());
        SetField(level, "fork", fork);
        SetField(level, "forkLabel", forkLabel);
        SetField(level, "beatRing", beat);
        SetField(level, "needle", needle);
        SetField(level, "meterLabel", meterLabel);
        SetField(level, "tunedText", tuned);
        SetField(level, "cat", cat);
        SetField(level, "noteTemplate", note);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.5f, -1.95f)));
        return SaveLevel(root, "Guitar");
    }

    // ---------- Claw machine ----------

    static GameObject BuildClawMachineLevel()
    {
        var root = new GameObject("Level_Claw");
        ClawMachineLevel level = root.AddComponent<ClawMachineLevel>();
        Bar(root.transform, "Back", Vector2.zero, new Vector2(12f, 16f), new Color(0.2f, 0.12f, 0.3f), -30);
        Art(root.transform, "Cabinet", "claw_cabinet", -10, false);
        Fit(Label(root.transform, new Vector2(0f, 3.4f), -8, "CLAW-O-MATIC", 0.075f), 4.4f, 0.6f);

        Transform carriage = Child(root.transform, "Carriage", new Vector2(0f, 2.72f)).transform;
        Bar(carriage, "Box", Vector2.zero, new Vector2(0.55f, 0.24f), new Color(0.35f, 0.37f, 0.45f), 3);
        LineRenderer cable = Line(root.transform, "Cable", 0.04f, new Color(0.17f, 0.13f, 0.2f), 2);
        Transform claw = Child(root.transform, "Claw", new Vector2(0f, 2.0f)).transform;
        SpriteRenderer clawArt = Art(claw, "Art", "claw_open", 6, false).GetComponent<SpriteRenderer>();

        // peluche: l'orsetto con la chiave sta SOTTO un pesce
        var toys = new List<Transform>();
        Transform Toy(string kind, Vector2 at)
        {
            Transform t = Child(root.transform, "Plush_" + kind, at).transform;
            Art(t, "Art", "claw_plush_" + kind, 4, false);
            toys.Add(t);
            return t;
        }
        Toy("mouse", new Vector2(-0.85f, -0.99f));
        Toy("cat", new Vector2(-0.05f, -0.99f));
        Transform key = Toy("key", new Vector2(0.75f, -0.99f));
        Toy("fish", new Vector2(0.75f, -0.37f));
        Toy("cat", new Vector2(1.6f, -0.99f));
        SpriteRenderer shine = Art(key, "Shine", "glow", 3, false).GetComponent<SpriteRenderer>();
        shine.color = new Color(1f, 0.85f, 0.3f, 0.6f);
        Art(root.transform, "Chute", "claw_chute", 8, false, new Vector2(-1.3f, -1.3f));

        Transform prize = Art(root.transform, "KeyPrize", "key", 12, false, new Vector2(-1.2f, -2.45f));
        prize.localScale = Vector3.one * 0.7f;

        // comandi
        Collider2D Arrow(string name, Vector2 at, bool left)
        {
            GameObject a = Child(root.transform, name, at);
            Transform art = Art(a.transform, "Art", "claw_arrow", 6, false);
            if (left) art.localScale = new Vector3(-1f, 1f, 1f);
            BoxCollider2D col = a.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.95f, 0.9f);
            return col;
        }
        Collider2D leftArrow = Arrow("Left", new Vector2(-1.6f, -3.55f), true);
        Collider2D rightArrow = Arrow("Right", new Vector2(-0.5f, -3.55f), false);
        TapButton drop = SwitchButton(root.transform, "Drop", "big_button", new Vector2(1.35f, -3.6f));
        drop.transform.localScale = Vector3.one * 0.75f;
        TextMesh dropLabel = Fit(Label(drop.transform, new Vector2(0f, 0.12f), 8, "DROP", 0.06f), 1.1f, 0.4f);
        dropLabel.color = Color.white;
        TextMesh coins = Fit(Label(root.transform, new Vector2(0.55f, -2.3f), 8, "", 0.05f), 1.6f, 0.4f);
        coins.color = Color.white;
        GameObject glass = Child(root.transform, "Glass", new Vector2(0f, 0.65f));
        glass.AddComponent<BoxCollider2D>().size = new Vector2(4.6f, 4.5f);

        SetField(level, "claw", claw);
        SetField(level, "clawArt", clawArt);
        SetField(level, "openSprite", sprites["claw_open"]);
        SetField(level, "closedSprite", sprites["claw_closed"]);
        SetField(level, "carriage", carriage);
        SetField(level, "cable", cable);
        SetArray(level, "toys", toys.ToArray());
        SetField(level, "keyToy", key);
        SetField(level, "keyPrize", prize);
        SetField(level, "dropButton", drop);
        SetField(level, "leftArrow", leftArrow);
        SetField(level, "rightArrow", rightArrow);
        SetField(level, "glass", glass.GetComponent<BoxCollider2D>());
        SetField(level, "coinsText", coins);
        SetArray(level, "confetti", E39Confetti(root.transform));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.3f, 1.0f)));
        return SaveLevel(root, "Claw");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V39E = new Dictionary<string, string[]>
    {
        { "paint_mixer", new[] { "Red+yellow = orange, yellow+blue = green, red+blue = purple. White makes it lighter.", "Salmon is a pinkish orange: 2 red + 1 yellow + 2 white." } },
        { "bubble_wrap", new[] { "The counter says one is left... but not on THIS side.", "Swipe sideways across the sheet to flip it over, then pop the bubble on the back." } },
        { "out_of_tune", new[] { "Drag a peg up to raise the pitch, down to lower it. The tuner shows LOW/HIGH while the string rings.", "For each string: pluck it, drag its peg until the needle is centred and the string glows. Tune all three." } },
        { "claw_machine", new[] { "The key bear is buried under a fish. Clear the fish out of the way first.", "Centre the claw on the fish and DROP, then centre it on the bear and DROP again." } },
    };
}
#endif
