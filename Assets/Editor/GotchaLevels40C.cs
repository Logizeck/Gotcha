#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli compositi della versione 0.40 (gruppo C).</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels40C() => new List<LevelDefinition>
    {
        Define("BombSquad", "bomb_squad", "BOMB SQUAD", "The manual knows. Swipe its pages.", BuildBombSquadLevel()),
        Define("Potion", "potion_lab", "POTION LAB", "Solve the riddle in the spell book.", BuildPotionLevel()),
        Define("RobotFactory", "robot_factory", "ROBOT FACTORY", "Look at what each robot is missing.", BuildRobotFactoryLevel()),
        Define("Trains", "train_switches", "TRAIN SWITCHES", "Tap the switches before the train gets there.", BuildTrainsLevel()),
    };

    // ---------- Pezzi comuni (gruppo C) ----------

    /// <summary>Riga dell'obiettivo in alto (testo bianco su striscia scura).</summary>
    static TextMesh V40C_Goal(Transform root)
    {
        Bar(root, "GoalBack", new Vector2(0f, 3.28f), new Vector2(6.2f, 0.46f), new Color(0.08f, 0.06f, 0.12f, 0.55f), 28);
        TextMesh t = Fit(Label(root, new Vector2(0f, 3.28f), 29, "", 0.045f), 5.4f, 0.36f);
        t.color = Color.white;
        return t;
    }

    /// <summary>Pallini di avanzamento in alto.</summary>
    static SpriteRenderer[] V40C_Dots(Transform root, int count, float gap = 0.4f, float scale = 1f)
    {
        var list = new List<SpriteRenderer>();
        for (int i = 0; i < count; i++)
        {
            Transform d = Art(root, "Dot" + i, "v40c_dot", 30, false, new Vector2((i - (count - 1) * 0.5f) * gap, 3.68f));
            d.localScale = Vector3.one * scale;
            list.Add(d.GetComponent<SpriteRenderer>());
        }
        return list.ToArray();
    }

    // ---------- Bomb squad ----------

    static GameObject BuildBombSquadLevel()
    {
        var root = new GameObject("Level_BombSquad");
        BombSquadLevel level = root.AddComponent<BombSquadLevel>();
        Art(root.transform, "Workshop", "workshop_bg", -20, false);
        Art(root.transform, "Case", "bs_case", 0, false);

        // manuale
        Transform page = Child(root.transform, "Page", new Vector2(-1.475f, 0f)).transform;
        Art(page, "Art", "bs_manual", 1, false, new Vector2(1.475f, 0f));
        BoxCollider2D pageCol = page.gameObject.AddComponent<BoxCollider2D>();
        pageCol.size = new Vector2(2.65f, 7.4f);
        pageCol.offset = new Vector2(0f, -0.7f);
        TextMesh pageText = Fit(Label(page, new Vector2(0f, -0.55f), 3, "", 0.032f), 2.3f, 6.4f);
        TextMesh pageNumber = Label(page, new Vector2(0f, -4.12f), 3, "1 / 3", 0.04f);
        var symbols = new List<SpriteRenderer>();
        for (int c = 0; c < 3; c++)
        {
            float x = (c - 1) * 0.68f;
            if (c > 0) Bar(page, "ColLine", new Vector2(x - 0.34f, -1.0f), new Vector2(0.025f, 3.7f), new Color(0.17f, 0.13f, 0.2f, 0.35f), 2);
            for (int r = 0; r < 6; r++)
            {
                Transform s = Art(page, "Sym", "bs_sym" + ((c * 6 + r) % 11), 3, false, new Vector2(x, 0.55f - r * 0.6f));
                s.localScale = Vector3.one * 0.85f;
                symbols.Add(s.GetComponent<SpriteRenderer>());
            }
        }
        var arrows = new List<Collider2D>();
        foreach (var a in new[] { (-2.35f, "<"), (-0.6f, ">") })
        {
            GameObject g = Child(root.transform, a.Item2 == "<" ? "Prev" : "Next", new Vector2(a.Item1, -4.12f));
            Label(g.transform, Vector2.zero, 4, a.Item2, 0.08f);
            g.AddComponent<BoxCollider2D>().size = new Vector2(0.85f, 0.7f);
            arrows.Add(g.GetComponent<Collider2D>());
        }

        // timer ed errori
        TextMesh timer = Fit(Label(root.transform, new Vector2(1.0f, 2.6f), 3, "1:30", 0.075f), 1.5f, 0.5f);
        timer.color = new Color(1f, 0.3f, 0.25f);
        TextMesh strikes = Fit(Label(root.transform, new Vector2(2.27f, 2.6f), 3, "---", 0.06f), 0.66f, 0.45f);
        strikes.color = new Color(1f, 0.3f, 0.25f);
        var leds = new List<SpriteRenderer>();
        foreach (float y in new[] { 1.98f, 0.12f, -1.82f })
        {
            Transform l = Art(root.transform, "Led", "led", 3, false, new Vector2(2.6f, y));
            l.localScale = Vector3.one * 1.1f;
            leds.Add(l.GetComponent<SpriteRenderer>());
        }

        // fili
        var wires = new List<Transform>();
        var wireArt = new List<SpriteRenderer>();
        for (int i = 0; i < 5; i++)
        {
            Transform w = Child(root.transform, "Wire" + i, new Vector2(0.4f + i * 0.5f, 1.33f)).transform;
            wireArt.Add(Art(w, "Art", "bs_wire", 3, false).GetComponent<SpriteRenderer>());
            w.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(0.5f, 1.4f);
            wires.Add(w);
        }

        // pulsante
        Transform button = Child(root.transform, "Button", new Vector2(0.85f, -0.6f)).transform;
        SpriteRenderer buttonArt = Art(button, "Art", "bs_button", 3, false).GetComponent<SpriteRenderer>();
        TextMesh buttonLabel = Fit(Label(button, new Vector2(0f, 0.02f), 4, "PRESS", 0.04f), 0.85f, 0.35f);
        button.gameObject.AddComponent<CircleCollider2D>().radius = 0.62f;
        SpriteRenderer strip = Bar(root.transform, "Strip", new Vector2(2.2f, -0.6f), new Vector2(0.28f, 1.28f), Color.black, 2);

        // tastierino
        var keys = new List<Transform>();
        var keyArt = new List<SpriteRenderer>();
        var keySyms = new List<SpriteRenderer>();
        foreach (Vector2 p in new[] { new Vector2(0.82f, -2.4f), new Vector2(1.98f, -2.4f), new Vector2(0.82f, -3.6f), new Vector2(1.98f, -3.6f) })
        {
            Transform k = Child(root.transform, "Key", p).transform;
            keyArt.Add(Art(k, "Art", "bs_key", 3, false).GetComponent<SpriteRenderer>());
            Transform s = Art(k, "Sym", "bs_sym0", 4, false);
            s.localScale = Vector3.one * 1.35f;
            keySyms.Add(s.GetComponent<SpriteRenderer>());
            k.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.08f, 1.08f);
            keys.Add(k);
        }
        var symSprites = new List<Sprite>();
        for (int i = 0; i < 11; i++) symSprites.Add(sprites["bs_sym" + i]);

        // esiti
        Transform boom = Child(root.transform, "Boom", new Vector2(0.4f, -0.5f)).transform;
        Art(boom, "Art", "boom", 80, false);
        Transform flag = Child(root.transform, "Flag", new Vector2(0.35f, -1.6f)).transform;
        flag.localScale = Vector3.one * 1.5f;
        Art(flag, "Art", "bs_flag", 70, false);
        TextMesh bang = Fit(Label(flag, new Vector2(0.66f, 1.08f), 71, "BANG!", 0.06f), 1.0f, 0.42f);
        bang.color = new Color(0.9f, 0.27f, 0.24f);
        SpriteRenderer flash = Bar(root.transform, "Flash", Vector2.zero, new Vector2(12f, 16f), new Color(1f, 1f, 1f, 0f), 90);

        SetField(level, "goalText", V40C_Goal(root.transform));
        SetArray(level, "dots", V40C_Dots(root.transform, 3));
        SetArray(level, "leds", leds.ToArray());
        SetField(level, "timerText", timer);
        SetField(level, "strikesText", strikes);
        SetField(level, "page", page);
        SetField(level, "pageText", pageText);
        SetField(level, "pageNumber", pageNumber);
        SetField(level, "manualArea", pageCol);
        SetField(level, "prevArrow", arrows[0]);
        SetField(level, "nextArrow", arrows[1]);
        SetArray(level, "manualSymbols", symbols.ToArray());
        SetArray(level, "wires", wires.ToArray());
        SetArray(level, "wireArt", wireArt.ToArray());
        SetField(level, "wireSprite", sprites["bs_wire"]);
        SetField(level, "wireCutSprite", sprites["bs_wire_cut"]);
        SetField(level, "button", button);
        SetField(level, "buttonArt", buttonArt);
        SetField(level, "buttonLabel", buttonLabel);
        SetField(level, "buttonArea", button.GetComponent<Collider2D>());
        SetField(level, "strip", strip);
        SetArray(level, "keys", keys.ToArray());
        SetArray(level, "keyArt", keyArt.ToArray());
        SetArray(level, "keySymbols", keySyms.ToArray());
        SetArray(level, "symbolSprites", symSprites.ToArray());
        SetField(level, "boom", boom);
        SetField(level, "flag", flag);
        SetField(level, "flash", flash);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.4f, 0.45f)));
        return SaveLevel(root, "BombSquad");
    }

    // ---------- Potion lab ----------

    static GameObject BuildPotionLevel()
    {
        var root = new GameObject("Level_Potion");
        PotionLabLevel level = root.AddComponent<PotionLabLevel>();
        Art(root.transform, "Lab", "pt_bg", -20, false);
        Art(root.transform, "Book", "pt_book", 1, false, new Vector2(0f, 2.0f));
        TextMesh left = Fit(Label(root.transform, new Vector2(-1.17f, 2.0f), 3, "", 0.035f), 1.85f, 1.35f);
        TextMesh right = Fit(Label(root.transform, new Vector2(1.17f, 2.0f), 3, "", 0.035f), 1.85f, 1.35f);

        // fuoco, calderone, termometro
        Transform fire = Child(root.transform, "Fire", new Vector2(-0.6f, -1.98f)).transform;
        Transform flames = Child(fire, "Flames", Vector2.zero).transform;
        flames.localScale = Vector3.one * 2.4f;
        foreach (float x in new[] { -0.11f, 0f, 0.11f }) Art(flames, "Flame", "flame", 1, false, new Vector2(x, 0f));
        Art(root.transform, "Cauldron", "pt_cauldron", 2, false, new Vector2(-0.6f, -0.6f));
        Transform logs = Child(root.transform, "Logs", new Vector2(-0.6f, -2.05f)).transform;
        logs.localScale = Vector3.one * 0.5f;
        Art(logs, "Art", "logs", 3, false);
        Transform mouth = Child(root.transform, "Mouth", new Vector2(-0.6f, -0.08f)).transform;
        SpriteRenderer liquid = Art(root.transform, "Liquid", "pt_liquid", 3, false, new Vector2(-0.6f, -0.08f)).GetComponent<SpriteRenderer>();
        Art(root.transform, "Thermo", "pt_thermo", 3, false, new Vector2(2.2f, -0.45f));
        Transform mercury = Child(root.transform, "Mercury", new Vector2(2.2f, -1.35f)).transform;
        Bar(mercury, "Fill", new Vector2(0f, 1.025f), new Vector2(0.09f, 2.05f), new Color(0.89f, 0.28f, 0.24f), 4);
        Transform ladle = Child(root.transform, "Ladle", new Vector2(-0.6f, -0.2f)).transform;
        Art(ladle, "Art", "pt_ladle", 4, false);
        TextMesh counter = Label(root.transform, new Vector2(-0.6f, 0.75f), 10, "", 0.06f);
        counter.color = Color.white;
        SmokeEmitter burp = Emitter(root.transform, "Burp", new Vector2(-0.6f, 0.1f), "puff", 14f, 1.2f, 1.1f, 0.5f, 0.6f, new Color(0.5f, 0.85f, 0.3f, 0.9f), 30);
        SmokeEmitter steam = Emitter(root.transform, "Steam", new Vector2(-0.6f, 0.1f), "puff", 9f, 1.4f, 0.9f, 0.5f, 0.45f, new Color(1f, 1f, 1f, 0.7f), 30);

        // gatto, rana, scintilla
        Transform cat = Child(root.transform, "Cat", new Vector2(1.55f, -2.55f)).transform;
        cat.localScale = Vector3.one * 0.5f;
        Art(cat, "Art", "cat", 2, false);
        Transform frog = Child(root.transform, "Frog", new Vector2(1.55f, -3.05f)).transform;
        frog.localScale = Vector3.one * 1.4f;
        Art(frog, "Art", "pond_frog", 2, false);
        Transform zap = Child(root.transform, "Zap", Vector2.zero).transform;
        zap.localScale = Vector3.one * 1.5f;
        Art(zap, "Art", "sparkle_star", 40, false);

        // ingredienti (nell'ordine degli indovinelli)
        string[] ing = { "pt_water", "pt_milk", "pt_egg", "pt_honey", "pt_mushroom", "feather", "cheese_wedge", "fish" };
        var items = new List<Transform>();
        foreach (string s in ing)
        {
            Transform h = Fitted(root.transform, "Ing_" + s, s, new Vector2(0f, -3.75f), 0.78f, 6);
            float k = h.localScale.x;
            h.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(0.86f / k, 0.9f / k);
            items.Add(h);
        }
        var hearts = new List<SpriteRenderer>();
        for (int i = 0; i < 3; i++)
        {
            Transform h = Art(root.transform, "Try", "heart", 30, false, new Vector2(1.75f + i * 0.4f, 3.68f));
            h.localScale = Vector3.one * 0.65f;
            hearts.Add(h.GetComponent<SpriteRenderer>());
        }

        SetField(level, "goalText", V40C_Goal(root.transform));
        SetArray(level, "dots", V40C_Dots(root.transform, 3));
        SetArray(level, "hearts", hearts.ToArray());
        SetField(level, "leftPage", left);
        SetField(level, "rightPage", right);
        SetArray(level, "ingredients", items.ToArray());
        SetField(level, "cauldronMouth", mouth);
        SetField(level, "liquid", liquid);
        SetField(level, "burp", burp);
        SetField(level, "steam", steam);
        SetField(level, "ladle", ladle);
        SetField(level, "counter", counter);
        SetField(level, "fire", fire);
        SetField(level, "mercury", mercury);
        SetField(level, "cat", cat);
        SetField(level, "frog", frog);
        SetField(level, "zap", zap);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.95f, 0.6f)));
        return SaveLevel(root, "Potion");
    }

    // ---------- Robot factory ----------

    static GameObject BuildRobotFactoryLevel()
    {
        var root = new GameObject("Level_RobotFactory");
        RobotFactoryLevel level = root.AddComponent<RobotFactoryLevel>();
        Art(root.transform, "Factory", "rf_bg", -20, false);
        string[] partNames = { "rf_head", "rf_arm", "rf_legs" };
        string[] partLabels = { "HEAD", "ARM", "LEGS" };

        // fase 1: il nastro
        Transform s1 = Child(root.transform, "Stage1", Vector2.zero).transform;
        Art(s1, "Blueprint", "rf_blueprint", 1, false, new Vector2(-1.35f, 2.2f));
        var icons = new List<GameObject>();
        foreach (string p in partNames)
        {
            Transform ic = Fitted(s1, "Icon_" + p, p, new Vector2(-1.35f, 2.4f), 0.75f, 2);
            ic.GetComponentInChildren<SpriteRenderer>().color = new Color(0.85f, 0.93f, 1f);
            icons.Add(ic.gameObject);
        }
        TextMesh bpLabel = Fit(Label(s1, new Vector2(-1.35f, 1.75f), 3, "", 0.04f), 2.1f, 0.3f);
        bpLabel.color = Color.white;
        TextMesh counter = Fit(Label(s1, new Vector2(1.35f, 2.2f), 3, "", 0.06f), 2.3f, 0.5f);
        counter.color = Color.white;
        Art(s1, "Belt", "rf_belt", 1, false, new Vector2(0f, -0.4f));
        var cleats = new List<Transform>();
        for (int i = 0; i < 10; i++)
            cleats.Add(Bar(s1, "Cleat", new Vector2(-3.2f + i * 0.64f, -0.22f), new Vector2(0.1f, 0.07f), new Color(0.55f, 0.57f, 0.62f), 2).transform);
        Transform body = Child(s1, "Body", new Vector2(-3.9f, 0.85f)).transform;
        BoxCollider2D bodyCol = body.gameObject.AddComponent<BoxCollider2D>();
        bodyCol.size = new Vector2(1.5f, 2.1f);
        var parts = new List<SpriteRenderer>
        {
            Art(body, "Torso", "rf_torso", 5, false).GetComponent<SpriteRenderer>(),
            Art(body, "Head", "rf_head", 4, false, new Vector2(0f, 0.42f)).GetComponent<SpriteRenderer>(),
            Art(body, "ArmL", "rf_arm", 4, false, new Vector2(-0.43f, 0.25f)).GetComponent<SpriteRenderer>(),
        };
        Transform armR = Child(body, "ArmR", new Vector2(0.43f, 0.25f)).transform;
        armR.localScale = new Vector3(-1f, 1f, 1f);
        parts.Add(Art(armR, "Art", "rf_arm", 4, false).GetComponent<SpriteRenderer>());
        parts.Add(Art(body, "Legs", "rf_legs", 4, false, new Vector2(0f, -0.43f)).GetComponent<SpriteRenderer>());
        var bins = new List<Collider2D>();
        var partSprites = new List<Sprite>();
        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * 1.75f;
            GameObject bin = Child(s1, "Bin" + i, new Vector2(x, -2.4f));
            Fitted(bin.transform, "Icon", partNames[i], new Vector2(0f, 0.2f), 0.7f, 3);
            Art(bin.transform, "Art", "rf_bin", 4, false, new Vector2(0f, -0.25f));
            TextMesh l = Label(bin.transform, new Vector2(0f, -0.92f), 4, partLabels[i], 0.045f);
            l.color = Color.white;
            bin.AddComponent<BoxCollider2D>().size = new Vector2(1.45f, 1.4f);
            bins.Add(bin.GetComponent<Collider2D>());
            partSprites.Add(sprites[partNames[i]]);
        }
        TextMesh hint1 = Fit(Label(s1, new Vector2(0f, -3.95f), 4, "Drag a part onto the robot as it passes!", 0.04f), 5.2f, 0.3f);
        hint1.color = Color.white;
        Transform held = Child(root.transform, "Held", Vector2.zero).transform;
        SpriteRenderer heldArt = Art(held, "Art", "rf_head", 40, false).GetComponent<SpriteRenderer>();

        // fase 2: la griglia
        Transform s2 = Child(root.transform, "Stage2", Vector2.zero).transform;
        const float cell = 0.76f;
        Vector2 gc = new Vector2(0f, 1.15f);
        Bar(s2, "GridBack", gc, new Vector2(cell * 5f + 0.2f, cell * 5f + 0.2f), new Color(0.17f, 0.2f, 0.27f), 0);
        for (int c = 0; c < 5; c++)
            for (int r = 0; r < 5; r++)
            {
                Transform t = Art(s2, "Tile", "rf_tile", 1, false, gc + new Vector2((c - 2) * cell, (r - 2) * cell));
                t.localScale = Vector3.one * 0.98f;
            }
        var crates = new List<Transform>();
        for (int i = 0; i < 7; i++)
        {
            Transform cr = Child(s2, "Crate" + i, gc).transform;
            Art(cr, "Art", "rf_crate", 3, false);
            crates.Add(cr);
        }
        Transform battery = Child(s2, "Battery", gc).transform;
        Art(battery, "Art", "rf_cell", 3, false);
        Transform bot = Child(s2, "Bot", gc).transform;
        Art(bot, "Art", "rf_minibot", 5, false);
        var slots = new List<Collider2D>();
        var slotCards = new List<SpriteRenderer>();
        for (int i = 0; i < 6; i++)
        {
            GameObject sl = Child(s2, "Slot" + i, new Vector2((i - 2.5f) * 0.88f, -1.3f));
            Art(sl.transform, "Art", "rf_slot", 2, false);
            SpriteRenderer card = Art(sl.transform, "Card", "rf_card0", 3, false).GetComponent<SpriteRenderer>();
            card.transform.localScale = Vector3.one * 0.95f;
            slotCards.Add(card);
            sl.AddComponent<BoxCollider2D>().size = new Vector2(0.86f, 0.9f);
            slots.Add(sl.GetComponent<Collider2D>());
        }
        string[] cardLabels = { "FORWARD", "TURN LEFT", "TURN RIGHT" };
        var palette = new List<Collider2D>();
        var cardSprites = new List<Sprite>();
        for (int i = 0; i < 3; i++)
        {
            GameObject pc = Child(s2, "Card" + i, new Vector2(-2.0f + i * 1.15f, -2.65f));
            Art(pc.transform, "Art", "rf_card" + i, 3, false);
            TextMesh l = Fit(Label(pc.transform, new Vector2(0f, -0.62f), 4, cardLabels[i], 0.035f), 1.05f, 0.25f);
            l.color = Color.white;
            pc.AddComponent<BoxCollider2D>().size = new Vector2(0.95f, 0.95f);
            palette.Add(pc.GetComponent<Collider2D>());
            cardSprites.Add(sprites["rf_card" + i]);
        }
        GameObject run = Child(s2, "Run", new Vector2(1.75f, -2.75f));
        Art(run.transform, "Art", "rf_run", 3, false);
        TextMesh runLabel = Label(run.transform, new Vector2(0f, 0.02f), 4, "RUN", 0.06f);
        runLabel.color = Color.white;
        run.AddComponent<CircleCollider2D>().radius = 0.6f;
        TextMesh hint2 = Fit(Label(s2, new Vector2(0f, -3.85f), 4, "Drag cards into the slots, tap a slot to clear it.", 0.04f), 5.2f, 0.3f);
        hint2.color = Color.white;
        Transform heldCard = Child(root.transform, "HeldCard", Vector2.zero).transform;
        Art(heldCard, "Art", "rf_card0", 40, false);
        SmokeEmitter sparks = Sparks(s2, "Sparks", new Color(1f, 0.85f, 0.3f));

        SetField(level, "goalText", V40C_Goal(root.transform));
        SetArray(level, "dots", V40C_Dots(root.transform, 2));
        SetField(level, "stage1", s1.gameObject);
        SetField(level, "stage2", s2.gameObject);
        SetField(level, "counter", counter);
        SetArray(level, "blueprintIcons", icons.ToArray());
        SetField(level, "blueprintLabel", bpLabel);
        SetArray(level, "cleats", cleats.ToArray());
        SetField(level, "body", body);
        SetField(level, "bodyArea", bodyCol);
        SetArray(level, "parts", parts.ToArray());
        SetArray(level, "bins", bins.ToArray());
        SetArray(level, "partSprites", partSprites.ToArray());
        SetField(level, "held", held);
        SetField(level, "heldArt", heldArt);
        SetArray(level, "crates", crates.ToArray());
        SetField(level, "battery", battery);
        SetField(level, "bot", bot);
        SetArray(level, "slots", slots.ToArray());
        SetArray(level, "slotCards", slotCards.ToArray());
        SetArray(level, "palette", palette.ToArray());
        SetArray(level, "cardSprites", cardSprites.ToArray());
        SetField(level, "heldCard", heldCard);
        SetField(level, "runButton", run.GetComponent<Collider2D>());
        SetField(level, "sparks", sparks);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.2f, 1.3f)));
        return SaveLevel(root, "RobotFactory");
    }

    // ---------- Train switches ----------

    static GameObject BuildTrainsLevel()
    {
        var root = new GameObject("Level_Trains");
        TrainSwitchesLevel level = root.AddComponent<TrainSwitchesLevel>();
        Art(root.transform, "Meadow", "tr_bg", -20, false);
        var beds = new List<LineRenderer>();
        var rails = new List<LineRenderer>();
        for (int i = 0; i < TrainSwitchesLevel.Segs.Length; i++)
        {
            Vector2[] seg = TrainSwitchesLevel.Segs[i];
            var pts = new Vector3[seg.Length];
            for (int k = 0; k < seg.Length; k++) pts[k] = seg[k];
            beds.Add(StaticLine(root.transform, "Bed" + i, pts, 0.32f, new Color(0.55f, 0.38f, 0.25f), 1));
            rails.Add(StaticLine(root.transform, "Rail" + i, pts, 0.1f, new Color(0.93f, 0.93f, 0.96f), 2));
        }
        Art(root.transform, "Tunnel", "tr_tunnel", 8, false, new Vector2(0f, 2.85f));
        Transform signal = Art(root.transform, "Signal", "v40c_dot", 9, false, new Vector2(0.95f, 2.75f));
        signal.localScale = Vector3.one * 1.8f;
        Bar(root.transform, "SignalPole", new Vector2(0.95f, 2.45f), new Vector2(0.06f, 0.5f), new Color(0.3f, 0.3f, 0.35f), 8);

        var switches = new List<Collider2D>();
        var arrows = new List<Transform>();
        for (int s = 0; s < 3; s++)
        {
            GameObject sw = Child(root.transform, "Switch" + s, TrainSwitchesLevel.SwitchAt[s]);
            Transform b = Art(sw.transform, "Art", "tr_switch", 4, false);
            b.localScale = Vector3.one * 0.9f;
            Transform arrow = Child(sw.transform, "Arrow", Vector2.zero).transform;
            Art(arrow, "Art", "tr_arrow", 5, false);
            sw.AddComponent<CircleCollider2D>().radius = 0.62f;
            switches.Add(sw.GetComponent<Collider2D>());
            arrows.Add(arrow);
        }
        var stations = new List<Transform>();
        foreach (float x in new[] { -2.0f, 0f, 2.0f })
        {
            Transform st = Child(root.transform, "Station", new Vector2(x, -3.5f)).transform;
            Art(st, "Art", "tr_station", 3, false);
            stations.Add(st);
        }
        var trains = new List<Transform>();
        for (int i = 0; i < 4; i++)
        {
            Transform t = Child(root.transform, "Train" + i, new Vector2(0f, 2.75f)).transform;
            Art(t, "Art", "tr_loco", 6, false);
            trains.Add(t);
        }
        TextMesh score = Fit(Label(root.transform, new Vector2(0f, -4.35f), 10, "", 0.05f), 4f, 0.4f);

        SetField(level, "goalText", V40C_Goal(root.transform));
        SetField(level, "scoreText", score);
        SetArray(level, "trainDots", V40C_Dots(root.transform, 8, 0.32f, 0.8f));
        SetArray(level, "beds", beds.ToArray());
        SetArray(level, "rails", rails.ToArray());
        SetArray(level, "switches", switches.ToArray());
        SetArray(level, "arrows", arrows.ToArray());
        SetArray(level, "stations", stations.ToArray());
        SetArray(level, "trains", trains.ToArray());
        SetField(level, "signal", signal.GetComponent<SpriteRenderer>());
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.3f, 1.9f)));
        return SaveLevel(root, "Trains");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V40C = new Dictionary<string, string[]>
    {
        { "bomb_squad", new[] { "Wires: count them first, then use the first rule that fits. Button: when holding, check the strip colour.",
            "Cut the wire the page-1 rules point to; tap or hold the button as page 2 says (blue strip = let go on a 4); press the 4 symbols in the order of the one column that has all of them." } },
        { "potion_lab", new[] { "Tears of the sky = water, the cow = milk, the mouse's dream = cheese... Then stir CLOCKWISE.",
            "Drop the 4 ingredients in the riddle's order, draw 5 clockwise circles in the cauldron, then blow gently: stop when the thermometer hits green and keep it there 2 seconds." } },
        { "robot_factory", new[] { "The grey ghost on the robot (and the blueprint) shows the missing part. Then plan the route on the grid.",
            "Drag the blueprint's part from its bin onto 3 passing robots. Then use all 6 slots: straight ahead, ONE turn, straight to the battery (e.g. F, F, F, RIGHT, F, F or F, F, LEFT, F, F, F) and press RUN." } },
        { "train_switches", new[] { "The light next to the tunnel shows the colour of the next train.",
            "Top switch: left for red, right for yellow. Lower switches: outer track = red/yellow, inner track = blue. Set them while the train is still on the way." } },
    };
}
#endif
