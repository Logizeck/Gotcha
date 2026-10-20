#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli della specifica 105-200 (in gioco dal 151), pacchetto L.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels41L() => new List<LevelDefinition>
    {
        Define("TheGreatEscape", "the_great_escape", "THE GREAT ESCAPE", "There might be a way under the floor.", BuildS41LGreatEscape(), "mouse_union"),
        Define("TheImpossibleShot", "the_impossible_shot", "THE IMPOSSIBLE SHOT", "A straight shot won't work.", BuildS41LImpossibleShot()),
        Define("TheEndlessStaircase", "the_endless_staircase", "THE ENDLESS STAIRCASE", "The top keeps moving away.", BuildS41LEndlessStaircase()),
        Define("AlmostThere", "almost_there", "ALMOST THERE", "Only one point is missing.", BuildS41LAlmostThere()),
    };

    /// <summary>I 4 livelli di Toni della specifica (X-RAY, BEAUTY FILTER, PERSONAL TRAINER, FINAL EXAM), in ordine.</summary>
    static List<LevelDefinition> Toni41() => new List<LevelDefinition>
    {
        Define("Toni7", "toni_xray", "TONI'S X-RAY", "Toni has eaten something important.", BuildS41LToniXRay()),
        Define("Toni8", "toni_beauty_filter", "TONI'S BEAUTY FILTER", "Toni was already beautiful.", BuildS41LToniBeauty()),
        Define("Toni9", "toni_personal_trainer", "TONI'S PERSONAL TRAINER", "Toni needs motivation.", BuildS41LToniTrainer()),
        Define("Toni10", "toni_final_exam", "TONI'S FINAL EXAM", "Toni isn't a normal student.", BuildS41LToniExam()),
    };

    // ---------- Aiuti comuni del pacchetto L ----------

    /// <summary>Riga dell'obiettivo in alto, su una fascia scura.</summary>
    static TextMesh S41L_Goal(Transform root, string text)
    {
        Transform h = Child(root, "Goal", new Vector2(0f, 3.25f)).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(5.5f, 0.5f), new Color(0.12f, 0.09f, 0.16f, 0.72f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, text, 0.05f);
        t.color = Color.white;
        Fit(t, 5.3f, 0.42f);
        return t;
    }

    /// <summary>Etichetta bianca su fascia scura (contatori).</summary>
    static TextMesh S41L_Info(Transform root, string name, Vector2 pos, float width, string text = "")
    {
        Transform h = Child(root, name, pos).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(width, 0.4f), new Color(0.12f, 0.09f, 0.16f, 0.62f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, text, 0.04f);
        t.color = Color.white;
        Fit(t, width - 0.2f, 0.32f);
        return t;
    }

    static SmokeEmitter[] S41L_Confetti(Transform root)
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
    static Transform S41L_Button(Transform root, string name, Vector2 pos, Vector2 size, Color color, string text, int order)
    {
        Transform b = Child(root, name, pos).transform;
        Bar(b, "Edge", Vector2.zero, size + new Vector2(0.1f, 0.1f), new Color(0.17f, 0.13f, 0.2f), order);
        Bar(b, "Face", Vector2.zero, size, color, order + 1);
        TextMesh t = Label(b, Vector2.zero, order + 2, text, 0.05f);
        t.color = Color.white;
        Fit(t, size.x - 0.2f, size.y - 0.15f);
        b.gameObject.AddComponent<BoxCollider2D>().size = size + new Vector2(0.15f, 0.15f);
        return b;
    }

    /// <summary>Contenitore in pos con scala, e dentro lo sprite con le sue coordinate di disegno.</summary>
    static Transform S41L_ArtAt(Transform parent, string name, string sprite, int order, Vector2 pos, float scale)
    {
        Transform t = Child(parent, name, pos).transform;
        t.localScale = Vector3.one * scale;
        Art(t, "Art", sprite, order, false);
        return t;
    }

    static void S41L_Masked(Transform t)
    {
        foreach (SpriteRenderer sr in t.GetComponentsInChildren<SpriteRenderer>(true)) sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    }

    // ---------- 196 The great escape ----------

    static GameObject BuildS41LGreatEscape()
    {
        var root = new GameObject("Level_TheGreatEscape");
        GreatEscapeLevel level = root.AddComponent<GreatEscapeLevel>();
        Transform r = root.transform;
        Art(r, "Prison", "s41l_prison_bg", -20, false);
        SpriteRenderer doorOpen = Art(r, "DoorOpen", "s41l_door_open", -15, false, new Vector2(2.55f, -0.55f)).GetComponent<SpriteRenderer>();

        // sbarre della finestrella (si toccano: suonano)
        var bars = new List<Collider2D>();
        for (int i = 0; i < 6; i++)
        {
            Transform b = Child(r, "Bar" + (i + 1), new Vector2(-2.1f + i * 0.42f, 1.15f)).transform;
            Art(b, "Art", "s41l_bar", 2, false);
            b.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(0.38f, 1.9f);
            bars.Add(b.GetComponent<Collider2D>());
        }

        SpriteRenderer hole = Art(r, "Hole", "s41l_hole", 2, false, new Vector2(0f, -3.2f)).GetComponent<SpriteRenderer>();
        Transform tile = S41L_ArtAt(r, "LooseTile", "s41l_tile_loose", 3, new Vector2(0f, -3.2f), 1f);
        LineRenderer tunnel = Line(r, "Tunnel", 0.2f, new Color(0.3f, 0.2f, 0.12f), 1);
        Transform exit = Child(r, "ExitPoint", new Vector2(2.35f, -3.65f)).transform;

        // il gatto carceriere alla scrivania, col cono di vista
        Transform cat = Child(r, "Cat", new Vector2(1.5f, -1.95f)).transform;
        cat.localScale = Vector3.one * 0.85f;
        SpriteRenderer catArt = Art(cat, "Art", "s41l_jailer_sleep", 4, false).GetComponent<SpriteRenderer>();
        S41L_ArtAt(r, "Desk", "s41l_desk", 6, new Vector2(1.5f, -1.35f), 0.75f);
        SpriteRenderer cone = Art(r, "Cone", "s41l_cone", 3, false, new Vector2(1.5f, -0.8f)).GetComponent<SpriteRenderer>();
        GameObject alert = Art(r, "Alert", "alert", 9, true, new Vector2(1.05f, 0.05f)).gameObject;
        SmokeEmitter zzz = Emitter(r, "Zzz", new Vector2(1.8f, -0.35f), "zzz", 1.3f, 2.2f, 0.45f, 0.1f, 0.6f, Color.white, 30);
        zzz.spinRange = 12f;
        zzz.growth = 0.9f;

        // il topo, il cucchiaino, la ciotola
        Transform mouse = S41L_ArtAt(r, "Mouse", "mouse", 8, new Vector2(-1.7f, -2.55f), 0.55f);
        Transform spoon = S41L_ArtAt(r, "Spoon", "s41l_spoon", 9, new Vector2(-2.05f, -3.75f), 1f);
        spoon.localRotation = Quaternion.Euler(0, 0, 10f);
        BoxCollider2D spoonBox = spoon.gameObject.AddComponent<BoxCollider2D>();
        spoonBox.size = new Vector2(1.2f, 0.6f);
        Transform bowl = S41L_ArtAt(r, "Bowl", "s41l_bowl", 7, new Vector2(-1.0f, -4.0f), 1.1f);
        BoxCollider2D bowlBox = bowl.gameObject.AddComponent<BoxCollider2D>();
        bowlBox.size = new Vector2(1.0f, 0.8f);
        bowlBox.offset = new Vector2(0f, 0.2f);
        Transform holdBar = Child(r, "HoldBar", new Vector2(-1.0f, -3.35f)).transform;
        Transform holdPivot = GaugeBar(holdBar, "Hold", Vector2.zero, new Vector2(1.0f, 0.12f), new Color(0.12f, 0.09f, 0.16f, 0.8f), new Color(1f, 0.82f, 0.3f), 30);

        // Easter egg: topini manifestanti (modelli), megafono, contratto
        string[] slogans = { "MORE CHEESE", "LESS CATS", "PAID NAPS" };
        var templates = new List<GameObject>();
        for (int i = 0; i < 3; i++)
        {
            Transform p = Child(r, "Protester" + i, new Vector2(0f, -3f)).transform;
            S41L_ArtAt(p, "Mouse", "mouse", 20, Vector2.zero, 0.3f);
            Transform sign = S41L_ArtAt(p, "Sign", "s41l_sign", 21, new Vector2(0.18f, 0.6f), 0.55f);
            Fit(Label(sign, Vector2.zero, 22, slogans[i], 0.03f), 0.8f, 0.3f).color = new Color(0.85f, 0.2f, 0.25f);
            templates.Add(p.gameObject);
        }
        var holes = new List<Transform>();
        foreach (Vector2 h in new[] { new Vector2(-2.55f, -1.85f), new Vector2(-0.2f, -1.85f), new Vector2(2.35f, -3.65f) })
            holes.Add(Child(r, "MouseHole", h).transform);
        Transform megaphone = S41L_ArtAt(r, "Megaphone", "s41l_megaphone", 24, Vector2.zero, 0.9f);
        Transform contract = S41L_ArtAt(r, "Contract", "s41l_contract", 40, new Vector2(-0.6f, 0.6f), 1.2f);
        Fit(Label(contract, new Vector2(0f, 0.45f), 41, "CONTRACT:\n23 HOURS OF SLEEP\nA DAY", 0.03f), 1.2f, 0.75f);
        TextMesh banner = Fit(Label(r, new Vector2(-0.9f, -1.25f), 45, "", 0.045f), 3.7f, 0.4f);
        banner.color = new Color(1f, 0.95f, 0.5f);

        SmokeEmitter dust = Emitter(r, "Dust", Vector2.zero, "puff", 25f, 0.6f, 0.5f, 0.4f, 0.4f, new Color(0.75f, 0.65f, 0.55f, 0.9f), 15);
        S41L_Goal(r, "HELP THE MOUSE ESCAPE");

        SetField(level, "spoon", spoon);
        SetField(level, "looseTile", tile);
        SetField(level, "mouse", mouse);
        SetField(level, "cat", cat);
        SetField(level, "exitPoint", exit);
        SetField(level, "bowl", bowl);
        SetField(level, "spoonArea", spoonBox);
        SetField(level, "bowlArea", bowlBox);
        SetArray(level, "bars", bars.ToArray());
        SetField(level, "hole", hole);
        SetField(level, "catArt", catArt);
        SetField(level, "cone", cone);
        SetField(level, "doorOpen", doorOpen);
        SetField(level, "holdPivot", holdPivot);
        SetField(level, "holdBar", holdBar.gameObject);
        SetField(level, "awakeSprite", sprites["s41l_jailer"]);
        SetField(level, "sleepSprite", sprites["s41l_jailer_sleep"]);
        SetField(level, "alertMark", alert);
        SetField(level, "zzz", zzz);
        SetField(level, "dust", dust);
        SetField(level, "tunnel", tunnel);
        SetField(level, "strikeText", S41L_Info(r, "Strikes", new Vector2(0f, 2.65f), 2.8f));
        SetField(level, "banner", banner);
        SetField(level, "catBubble", SmallBubble(r, new Vector2(1.1f, 0.55f)));
        SetField(level, "mouseBubble", SmallBubble(r, new Vector2(-1.4f, -0.85f)));
        SetArray(level, "confetti", S41L_Confetti(r));
        SetArray(level, "protesterTemplates", templates.ToArray());
        SetArray(level, "spawnHoles", holes.ToArray());
        SetField(level, "megaphone", megaphone);
        SetField(level, "contract", contract);
        return SaveLevel(root, "TheGreatEscape");
    }

    // ---------- 197 The impossible shot ----------

    static GameObject BuildS41LImpossibleShot()
    {
        var root = new GameObject("Level_TheImpossibleShot");
        ImpossibleShotLevel level = root.AddComponent<ImpossibleShotLevel>();
        Transform r = root.transform;
        Art(r, "Court", "s41l_court_bg", -20, false);
        Art(r, "WallLow", "s41l_brick_wall", 2, false, new Vector2(0.85f, -1.2f));
        Art(r, "WallHigh", "s41l_brick_wall", 2, false, new Vector2(-0.85f, 0.9f));
        Transform basket = S41L_ArtAt(r, "Basket", "s41l_basket_top", 1, new Vector2(-1.5f, 2.15f), 1f);
        S41L_ArtAt(r, "Slingshot", "slingshot", 3, new Vector2(1.5f, -4.25f), 0.55f);
        Transform forkL = Child(r, "ForkL", new Vector2(1.32f, -3.55f)).transform;
        Transform forkR = Child(r, "ForkR", new Vector2(1.68f, -3.55f)).transform;
        LineRenderer band = Line(r, "Band", 0.05f, new Color(0.55f, 0.15f, 0.12f), 6);
        Transform ball = S41L_ArtAt(r, "Ball", "dunk_ball", 7, new Vector2(1.5f, -3.6f), 0.45f);
        var dots = new List<SpriteRenderer>();
        for (int i = 0; i < 24; i++) dots.Add(Art(r, "Dot", "s41l_dot", 8, false).GetComponent<SpriteRenderer>());
        TextMesh bounce = Fit(Label(r, Vector2.zero, 30, "", 0.05f), 3f, 0.4f);
        bounce.color = Color.white;
        S41L_Goal(r, "BANK IT INTO THE BASKET");

        SetField(level, "ball", ball);
        SetField(level, "basket", basket);
        SetField(level, "forkLeft", forkL);
        SetField(level, "forkRight", forkR);
        SetField(level, "band", band);
        SetArray(level, "dots", dots.ToArray());
        SetField(level, "missText", S41L_Info(r, "Misses", new Vector2(0.95f, 2.5f), 2.6f));
        SetField(level, "bounceText", bounce);
        SetField(level, "bubble", SmallBubble(r, new Vector2(-0.9f, -2.5f)));
        SetArray(level, "confetti", S41L_Confetti(r));
        return SaveLevel(root, "TheImpossibleShot");
    }

    // ---------- 198 The endless staircase ----------

    static GameObject BuildS41LEndlessStaircase()
    {
        var root = new GameObject("Level_TheEndlessStaircase");
        EndlessStaircaseLevel level = root.AddComponent<EndlessStaircaseLevel>();
        Transform r = root.transform;
        Art(r, "Sky", "s41l_tower_bg", -20, false);
        var steps = new List<Transform>();
        for (int i = 0; i < 20; i++)
        {
            Transform s = Child(r, "Step" + i, new Vector2(-1.25f + (i - 8) * 0.5f, -1.55f + (i - 8) * 0.55f)).transform;
            Art(s, "Art", "s41l_step", 2, false);
            steps.Add(s);
        }
        Transform climber = S41L_ArtAt(r, "Climber", "s41l_climber", 6, new Vector2(-1.0f, -1.55f), 1f);
        Transform summit = S41L_ArtAt(r, "Summit", "s41l_summit", 4, Vector2.zero, 1f);
        Transform top = Child(r, "Top", new Vector2(1.5f, 2.55f)).transform;
        Art(top, "Art", "s41l_top_sign", 10, false);
        TextMesh topText = Fit(Label(top, Vector2.zero, 11, "TOP", 0.07f), 1f, 0.45f);
        topText.color = new Color(0.45f, 0.25f, 0.05f);
        BoxCollider2D topBox = top.gameObject.AddComponent<BoxCollider2D>();
        topBox.size = new Vector2(1.4f, 1.0f);
        topBox.offset = new Vector2(0f, 0.15f);

        Transform stamina = GaugeBar(r, "Energy", new Vector2(-1.2f, -4.15f), new Vector2(1.6f, 0.2f), new Color(0.12f, 0.09f, 0.16f, 0.8f), new Color(0.4f, 0.85f, 0.35f), 30);
        Fit(Label(r, new Vector2(-1.2f, -3.85f), 31, "ENERGY", 0.035f), 1.4f, 0.25f).color = Color.white;
        S41L_Goal(r, "REACH THE TOP");

        SetArray(level, "steps", steps.ToArray());
        SetField(level, "climber", climber);
        SetField(level, "topSign", top);
        SetField(level, "summit", summit);
        SetField(level, "staminaPivot", stamina);
        SetField(level, "topArea", topBox);
        SetField(level, "stepsText", S41L_Info(r, "StepsLeft", new Vector2(-1.15f, 2.65f), 2.9f));
        SetField(level, "attemptsText", S41L_Info(r, "Attempts", new Vector2(1.25f, -4.1f), 2.2f));
        SetField(level, "bubble", SmallBubble(r, new Vector2(-1.4f, -0.2f)));
        SetArray(level, "confetti", S41L_Confetti(r));
        return SaveLevel(root, "TheEndlessStaircase");
    }

    // ---------- 199 Almost there ----------

    static GameObject BuildS41LAlmostThere()
    {
        var root = new GameObject("Level_AlmostThere");
        AlmostThereLevel level = root.AddComponent<AlmostThereLevel>();
        Transform r = root.transform;
        Art(r, "Arcade", "s41l_arcade_bg", -20, false);
        Transform board = S41L_ArtAt(r, "Board", "s41l_scoreboard", 1, new Vector2(0f, 0.6f), 1f);
        Fit(Label(board, new Vector2(0f, 0.72f), 3, "SCORE", 0.04f), 1.6f, 0.25f).color = new Color(0.24f, 0.9f, 1f);

        TextMesh score = Fit(Label(r, new Vector2(-1.3f, 0.5f), 6, "99", 0.17f), 1.7f, 1.2f);
        score.color = Color.white;
        TextMesh max = Fit(Label(r, new Vector2(1.35f, 0.5f), 6, "100", 0.17f), 1.9f, 1.2f);
        max.color = Color.white;
        Transform dot = S41L_ArtAt(r, "Dot", "s41l_hidden_dot", 3, new Vector2(0f, 0.45f), 1f);
        CircleCollider2D dotArea = dot.gameObject.AddComponent<CircleCollider2D>();
        dotArea.radius = 0.45f;
        Transform slash = Child(r, "Slash", new Vector2(0f, 0.5f)).transform;
        Bar(slash, "Cover", Vector2.zero, new Vector2(0.55f, 1.1f), new Color(0.055f, 0.043f, 0.094f), 4);
        Fit(Label(slash, Vector2.zero, 6, "/", 0.17f), 0.8f, 1.2f).color = Color.white;
        BoxCollider2D slashArea = slash.gameObject.AddComponent<BoxCollider2D>();
        slashArea.size = new Vector2(0.8f, 1.4f);
        BoxCollider2D scoreArea = Child(r, "ScoreArea", new Vector2(-1.3f, 0.5f)).AddComponent<BoxCollider2D>();
        scoreArea.size = new Vector2(1.5f, 1.3f);
        BoxCollider2D maxArea = Child(r, "MaxArea", new Vector2(1.35f, 0.5f)).AddComponent<BoxCollider2D>();
        maxArea.size = new Vector2(1.6f, 1.3f);
        Fit(Label(r, new Vector2(0f, -3.4f), 5, "(nothing to collect around here)", 0.035f), 4.5f, 0.3f).color = new Color(1f, 1f, 1f, 0.5f);
        S41L_Goal(r, "SCORE 100 POINTS");

        SetField(level, "scoreText", score);
        SetField(level, "maxText", max);
        SetField(level, "strikeText", S41L_Info(r, "Penalties", new Vector2(0f, -1.05f), 2.6f));
        SetField(level, "scoreArea", scoreArea);
        SetField(level, "maxArea", maxArea);
        SetField(level, "slashArea", slashArea);
        SetField(level, "dotArea", dotArea);
        SetField(level, "slash", slash);
        SetField(level, "dot", dot);
        SetField(level, "board", board);
        SetField(level, "bubble", SmallBubble(r, new Vector2(0f, -2.2f)));
        SetArray(level, "confetti", S41L_Confetti(r));
        return SaveLevel(root, "AlmostThere");
    }

    // ---------- Toni 7: X-ray ----------

    static GameObject BuildS41LToniXRay()
    {
        var root = new GameObject("Level_Toni7");
        ToniXRayLevel level = root.AddComponent<ToniXRayLevel>();
        Transform r = root.transform;
        Art(r, "Room", "room_bg", -20, false);
        Transform fridge = S41L_ArtAt(r, "Fridge", "s41l_fridge", -5, new Vector2(-2.05f, -3.35f), 0.7f);
        Transform padlock = S41L_ArtAt(r, "Padlock", "s41l_padlock", 1, new Vector2(-2.05f, -2.55f), 0.9f);
        ToniPug toni = ToniRig(r, new Vector2(0.55f, -2.7f), 1.35f, r);

        // l'interno della pancia ai raggi X (visibile solo dentro lo scanner)
        Transform xray = Child(toni.transform, "Xray", Vector2.zero).transform;
        Art(xray, "Bones", "s41l_xray_bones", 10, false);
        S41L_ArtAt(xray, "Sock", "sock", 11, new Vector2(0.05f, 0.0f), 0.3f).GetComponentInChildren<SpriteRenderer>().color = new Color(0.75f, 0.92f, 1f);
        S41L_ArtAt(xray, "Duck", "pond_duck", 11, new Vector2(0.62f, -0.07f), 0.28f).GetComponentInChildren<SpriteRenderer>().color = new Color(0.8f, 0.95f, 1f);
        Transform xKey = S41L_ArtAt(r, "XrayKey", "key", 12, Vector2.zero, 0.26f);
        xKey.GetComponentInChildren<SpriteRenderer>().color = new Color(1f, 1f, 0.75f);
        S41L_Masked(xray);
        S41L_Masked(xKey);

        Transform scanner = Child(r, "Scanner", new Vector2(-1.0f, 0.9f)).transform;
        Art(scanner, "Frame", "s41l_scanner", 20, false);
        Transform mask = Child(scanner, "Mask", Vector2.zero).transform;
        mask.localScale = new Vector3(1.5f, 1.2f, 1f);
        mask.gameObject.AddComponent<SpriteMask>().sprite = sprites["plain"];
        Fit(Label(scanner, new Vector2(0f, -0.85f), 21, "X-RAY", 0.03f), 0.34f, 0.2f).color = Color.white;
        BoxCollider2D scanBox = scanner.gameObject.AddComponent<BoxCollider2D>();
        scanBox.size = new Vector2(1.9f, 1.85f);
        scanBox.offset = new Vector2(0f, -0.12f);

        Transform realKey = S41L_ArtAt(r, "Key", "key", 30, Vector2.zero, 0.4f);
        SmokeEmitter sneeze = Emitter(r, "Sneeze", Vector2.zero, "puff", 40f, 0.5f, 0.2f, 0.5f, 0.35f, new Color(0.9f, 1f, 0.95f, 0.9f), 25);
        sneeze.sideDrift = -2.5f;
        S41L_Goal(r, "GET THE FRIDGE KEY BACK");
        Fit(Label(r, new Vector2(0f, 2.65f), 30, "SCAN HIS BELLY - TILT - TAP HIS NOSE", 0.035f), 5f, 0.3f).color = new Color(0.25f, 0.2f, 0.35f);

        SetField(level, "toni", toni);
        SetField(level, "scanner", scanner);
        SetField(level, "xKey", xKey);
        SetField(level, "realKey", realKey);
        SetField(level, "padlock", padlock);
        SetField(level, "fridge", fridge);
        SetField(level, "scannerArea", scanBox);
        SetField(level, "sneezeCloud", sneeze);
        SetField(level, "strikeText", S41L_Info(r, "Sneezes", new Vector2(1.2f, -4.2f), 2.2f));
        SetArray(level, "confetti", S41L_Confetti(r));
        return SaveLevel(root, "Toni7");
    }

    // ---------- Toni 8: beauty filter ----------

    static GameObject BuildS41LToniBeauty()
    {
        var root = new GameObject("Level_Toni8");
        ToniBeautyFilterLevel level = root.AddComponent<ToniBeautyFilterLevel>();
        Transform r = root.transform;
        Art(r, "Party", "party_bg", -20, false);
        // la polaroid
        Bar(r, "PhotoEdge", new Vector2(0f, 0.95f), new Vector2(3.75f, 4.05f), new Color(0.17f, 0.13f, 0.2f), -6);
        Bar(r, "Photo", new Vector2(0f, 0.95f), new Vector2(3.6f, 3.9f), Color.white, -5);
        Bar(r, "PhotoBack", new Vector2(0f, 1.2f), new Vector2(3.2f, 3.1f), new Color(0.75f, 0.9f, 1f), -4);

        Transform face = Child(r, "Face", new Vector2(0f, 1.15f)).transform;
        face.localScale = Vector3.one * 1.2f;
        var antennae = new List<GameObject>();
        foreach (float s in new[] { -1f, 1f })
        {
            Transform a = S41L_ArtAt(face, "Antenna", "s41l_antenna", 0, new Vector2(s * 0.4f, 0.8f), 1f);
            a.localRotation = Quaternion.Euler(0, 0, -s * 20f);
            antennae.Add(a.gameObject);
        }
        Transform earL = S41L_ArtAt(face, "EarL", "s41l_bf_ear", 1, new Vector2(-0.72f, 0.62f), 1f);
        Transform earR = S41L_ArtAt(face, "EarR", "s41l_bf_ear", 1, new Vector2(0.72f, 0.62f), 1f);
        earR.localScale = new Vector3(-1f, 1f, 1f);
        SpriteRenderer head = Art(face, "Head", "s41l_bf_head", 2, false).GetComponent<SpriteRenderer>();
        Transform eyeL = S41L_ArtAt(face, "EyeL", "s41l_bf_eye", 3, new Vector2(-0.38f, 0.12f), 1f);
        Transform eyeR = S41L_ArtAt(face, "EyeR", "s41l_bf_eye", 3, new Vector2(0.38f, 0.12f), 1f);
        Transform snout = S41L_ArtAt(face, "Snout", "s41l_bf_snout", 4, new Vector2(0f, -0.33f), 1f);
        var fur = new List<Object> { head, earL.GetComponentInChildren<SpriteRenderer>(), earR.GetComponentInChildren<SpriteRenderer>(), snout.GetComponentInChildren<SpriteRenderer>() };
        TextMesh caption = Fit(Label(r, new Vector2(0f, -0.72f), 6, "", 0.06f), 3.2f, 0.4f);
        SmokeEmitter hearts = Emitter(r, "Hearts", new Vector2(0f, 2.3f), "heart", 6f, 1.4f, 0.9f, 1.2f, 0.8f, Color.white, 30);
        hearts.spinRange = 30f;
        SmokeEmitter sparkle = Emitter(r, "Sparkle", new Vector2(0f, 1.2f), "sparkle_star", 25f, 0.7f, 0.3f, 1.3f, 0.6f, Color.white, 35);

        // il pulsantino nascosto e, sopra, il pannello dei filtri (si trascina)
        Transform noFilter = S41L_Button(r, "NoFilter", new Vector2(0.9f, -3.3f), new Vector2(1.3f, 0.45f), new Color(0.35f, 0.35f, 0.42f), "NO FILTER", 4);
        Transform panel = Child(r, "Panel", new Vector2(0f, -3.0f)).transform;
        Art(panel, "Art", "s41l_bf_panel", 10, false);
        Fit(Label(panel, new Vector2(0f, 0.6f), 11, "BEAUTY FILTERS", 0.035f), 3f, 0.18f).color = Color.white;
        BoxCollider2D panelBox = panel.gameObject.AddComponent<BoxCollider2D>();
        panelBox.size = new Vector2(3.7f, 1.7f);
        var buttons = new List<Collider2D>();
        string[] names = { "BIGGER EYES", "CUTER EARS", "TINY SNOUT" };
        Color[] cols = { new Color(0.45f, 0.55f, 0.95f), new Color(0.95f, 0.5f, 0.7f), new Color(0.95f, 0.65f, 0.3f) };
        for (int i = 0; i < 3; i++) buttons.Add(S41L_Button(panel, "Filter" + i, new Vector2(-1.17f + i * 1.17f, -0.12f), new Vector2(1.05f, 0.8f), cols[i], names[i], 11).GetComponent<Collider2D>());
        SpriteRenderer flash = Bar(r, "Flash", Vector2.zero, new Vector2(8f, 12f), Color.white, 90);
        S41L_Goal(r, "THE PERFECT PROFILE PIC");

        SetField(level, "face", face);
        SetField(level, "earL", earL);
        SetField(level, "earR", earR);
        SetField(level, "eyeL", eyeL);
        SetField(level, "eyeR", eyeR);
        SetField(level, "snout", snout);
        SetField(level, "panel", panel);
        SetArray(level, "furParts", fur.ToArray());
        SetArray(level, "antennae", antennae.ToArray());
        SetField(level, "panelArea", panelBox);
        SetField(level, "noFilterArea", noFilter.GetComponent<Collider2D>());
        SetArray(level, "filterButtons", buttons.ToArray());
        SetField(level, "flash", flash);
        SetField(level, "retouchText", S41L_Info(r, "Retouches", new Vector2(0f, -1.3f), 2.6f));
        SetField(level, "caption", caption);
        SetField(level, "bubble", SmallBubble(r, new Vector2(0f, -1.85f)));
        SetField(level, "hearts", hearts);
        SetField(level, "sparkle", sparkle);
        return SaveLevel(root, "Toni8");
    }

    // ---------- Toni 9: personal trainer ----------

    static GameObject BuildS41LToniTrainer()
    {
        var root = new GameObject("Level_Toni9");
        ToniTrainerLevel level = root.AddComponent<ToniTrainerLevel>();
        Transform r = root.transform;
        Art(r, "Park", "park_bg", -20, false);
        Bar(r, "Track", new Vector2(0f, -3.75f), new Vector2(6.4f, 0.8f), new Color(0.85f, 0.5f, 0.35f), -10);
        Bar(r, "TrackLine", new Vector2(0f, -3.37f), new Vector2(6.4f, 0.05f), Color.white, -9);
        var obstacles = new List<Transform>
        {
            S41L_ArtAt(r, "Hurdle1", "s41l_hurdle", 2, new Vector2(-0.7f, -3.35f), 0.9f),
            S41L_ArtAt(r, "Tire", "s41l_tire", 2, new Vector2(0.2f, -3.35f), 1f),
            S41L_ArtAt(r, "Hurdle2", "s41l_hurdle", 2, new Vector2(1.1f, -3.35f), 0.9f),
        };
        S41L_ArtAt(r, "Arch", "s41l_finish", 1, new Vector2(2.25f, -3.35f), 1f);
        Transform finish = Child(r, "FinishLine", new Vector2(1.95f, -3.35f)).transform;
        ToniPug toni = ToniRig(r, new Vector2(-1.9f, -3.05f), 0.65f, r);
        // il biscotto, sul suo piattino
        Bar(r, "Plate", new Vector2(-1.9f, 1.35f), new Vector2(1.0f, 0.1f), new Color(0.95f, 0.95f, 1f), 1);
        Transform biscuit = S41L_ArtAt(r, "Biscuit", "cookie", 15, new Vector2(-1.9f, 1.62f), 1.3f);
        S41L_Goal(r, "GET TONI TO THE FINISH");

        SetField(level, "toni", toni);
        SetField(level, "biscuit", biscuit);
        SetField(level, "finish", finish);
        SetArray(level, "obstacles", obstacles.ToArray());
        SetField(level, "eatenText", S41L_Info(r, "Eaten", new Vector2(0f, 2.65f), 3.2f));
        SetArray(level, "confetti", S41L_Confetti(r));
        return SaveLevel(root, "Toni9");
    }

    // ---------- Toni 10: final exam ----------

    static GameObject BuildS41LToniExam()
    {
        var root = new GameObject("Level_Toni10");
        ToniFinalExamLevel level = root.AddComponent<ToniFinalExamLevel>();
        Transform r = root.transform;
        Art(r, "Class", "s41l_class_bg", -20, false);
        TextMesh board = Fit(Label(r, new Vector2(0f, 1.55f), 2, "", 0.06f), 4.6f, 1.5f);
        board.color = new Color(0.95f, 0.97f, 0.92f);
        Transform professor = S41L_ArtAt(r, "Professor", "s41l_professor", 4, new Vector2(-2.2f, -3.3f), 0.85f);
        S41L_ArtAt(r, "Desk", "s41l_prof_desk", 3, new Vector2(0.45f, -2.0f), 1f);
        ToniPug toni = ToniRig(r, new Vector2(0.55f, -1.45f), 0.95f, r);
        S41L_ArtAt(toni.transform, "Glasses", "s41l_glasses", 6, new Vector2(-0.72f, 0.47f), 0.95f);
        Transform mortar = S41L_ArtAt(toni.transform, "Mortarboard", "s41l_mortarboard", 7, new Vector2(-0.75f, 0.72f), 1.3f);

        // diploma appeso in alto
        Transform diploma = Child(r, "Diploma", new Vector2(2.0f, 2.45f)).transform;
        S41L_ArtAt(diploma, "Scroll", "scroll", 10, Vector2.zero, 0.45f);
        Fit(Label(diploma, Vector2.zero, 11, "DIPLOMA", 0.03f), 0.5f, 0.2f).color = new Color(0.4f, 0.25f, 0.1f);
        BoxCollider2D dipBox = diploma.gameObject.AddComponent<BoxCollider2D>();
        dipBox.size = new Vector2(1.1f, 0.8f);

        // risposte e bicchieri della memoria
        var answers = new List<Collider2D>();
        var labels = new List<TextMesh>();
        float[] xs = { -0.9f, 0.5f, 1.9f };
        for (int i = 0; i < 3; i++)
        {
            Transform b = S41L_Button(r, "Answer" + i, new Vector2(xs[i], -3.9f), new Vector2(1.25f, 0.6f), new Color(0.3f, 0.55f, 0.85f), "?", 20);
            answers.Add(b.GetComponent<Collider2D>());
            labels.Add(b.GetComponentInChildren<TextMesh>());
        }
        var items = new List<Transform>
        {
            S41L_ArtAt(r, "MemBone", "bone", 12, Vector2.zero, 0.6f),
            S41L_ArtAt(r, "MemBall", "toni_ball", 12, Vector2.zero, 0.8f),
            S41L_ArtAt(r, "MemBiscuit", "cookie", 12, Vector2.zero, 1.2f),
        };
        var cups = new List<Transform>();
        var cupAreas = new List<Collider2D>();
        for (int i = 0; i < 3; i++)
        {
            Transform c = S41L_ArtAt(r, "Cup" + i, "s41l_cup", 14, new Vector2(xs[i], -4.25f), 1f);
            BoxCollider2D box = c.gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1.1f, 1.1f);
            box.offset = new Vector2(0f, 0.45f);
            cups.Add(c);
            cupAreas.Add(box);
        }

        // finale: il muso di Toni in primo piano col tocco, e il RUTTO
        Transform bigFace = Child(r, "BigFace", new Vector2(0f, 0f)).transform;
        bigFace.localScale = Vector3.one * 0.9f;
        Art(bigFace, "Face", "toni_face_big", 85, false);
        S41L_ArtAt(bigFace, "Mortar", "s41l_mortarboard", 86, new Vector2(0f, 1.5f), 2.4f);
        TextMesh burp = Fit(Label(r, new Vector2(0f, -2.6f), 92, "", 0.14f), 5f, 1f);
        burp.color = new Color(0.55f, 0.9f, 0.3f);
        S41L_Goal(r, "PASS THE FINAL EXAM");

        SetField(level, "toni", toni);
        SetField(level, "boardText", board);
        SetField(level, "mistakesText", S41L_Info(r, "Mistakes", new Vector2(-1.1f, 2.7f), 2.4f));
        SetField(level, "burpText", burp);
        SetArray(level, "answerButtons", answers.ToArray());
        SetArray(level, "answerLabels", labels.ToArray());
        SetArray(level, "memItems", items.ToArray());
        SetArray(level, "cups", cups.ToArray());
        SetArray(level, "cupAreas", cupAreas.ToArray());
        SetField(level, "diploma", diploma);
        SetField(level, "diplomaArea", dipBox);
        SetField(level, "mortarboard", mortar);
        SetField(level, "professor", professor);
        SetField(level, "bigFace", bigFace);
        SetField(level, "profBubble", SmallBubble(r, new Vector2(-1.4f, -0.35f)));
        SetArray(level, "confetti", S41L_Confetti(r));
        return SaveLevel(root, "Toni10");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V41L = new Dictionary<string, string[]>
    {
        { "the_great_escape", new[] { "Look for a loose tile.", "Use the spoon to lift the tile and open a tunnel." } },
        { "the_impossible_shot", new[] { "Use the walls.", "Aim for two rebounds before the basket." } },
        { "the_endless_staircase", new[] { "Maybe the goal can move closer.", "Drag the word TOP down to the character." } },
        { "almost_there", new[] { "Look between the numbers.", "Move the slash and tap the hidden dot." } },
        { "toni_xray", new[] { "Look inside his belly.", "Move the key toward his mouth and make him sneeze." } },
        { "toni_beauty_filter", new[] { "More filters aren't helping.", "Disable every filter." } },
        { "toni_personal_trainer", new[] { "Food works better than commands.", "Lead him with the biscuit, but keep it away from his mouth." } },
        { "toni_final_exam", new[] { "Food is his answer to almost everything.", "Choose BISCUIT, remember the biscuit and move the diploma toward Toni." } },
    };
}
#endif
