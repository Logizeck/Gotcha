#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Speciale Halloween: sei livelli 245-250 (vedi brief), inseriti prima di Almost There e dell'esame di Toni.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> LevelsHalloween() => new List<LevelDefinition>
    {
        Define("HwWerepug", "hw_werepug", "TONI THE WERE-PUG", "Maybe the problem isn't Toni...", BuildHwWerepug()),
        Define("HwPumpkin", "hw_pumpkin", "THE PERFECT PUMPKIN", "A carved pumpkin isn't enough for Halloween.", BuildHwPumpkin()),
        Define("HwGhost", "hw_ghost", "THE GHOST WHO CAN'T SCARE", "The ghost doesn't look very convincing.", BuildHwGhost()),
        Define("HwWitch", "hw_witch", "THE WITCH AND THE IMPOSSIBLE POTION", "All the ingredients seem wrong.", BuildHwWitch()),
        Define("HwZombie", "hw_zombie", "THE VEGETARIAN ZOMBIE", "Maybe the problem isn't what he eats.", BuildHwZombie()),
        Define("HwTrickOrTreat", "hw_trick_or_treat", "TRICK OR TREAT!", "The old lady recognises every visitor.", BuildHwTrickOrTreat(), "true_monster_pug"),
    };

    // ---------- Aiuti comuni dello speciale ----------

    /// <summary>Riga dell'obiettivo in alto, su una fascia scura.</summary>
    static TextMesh HW_Goal(Transform root, string text)
    {
        Transform h = Child(root, "Goal", new Vector2(0f, 3.25f)).transform;
        Bar(h, "Back", Vector2.zero, new Vector2(5.6f, 0.52f), new Color(0.1f, 0.06f, 0.16f, 0.78f), 70);
        TextMesh t = Label(h, Vector2.zero, 71, text, 0.05f);
        t.color = new Color(1f, 0.86f, 0.45f);
        Fit(t, 5.3f, 0.42f);
        return t;
    }

    /// <summary>Oggetto con il suo disegno (origine del disegno = origine dell'oggetto).</summary>
    static Transform HW_Obj(Transform parent, string name, Vector2 pos, string sprite, int order, float scale = 1f, bool wobble = false)
    {
        Transform t = Child(parent, name, pos).transform;
        t.localScale = Vector3.one * scale;
        Art(t, "Art", sprite, order, wobble);
        return t;
    }

    static BoxCollider2D HW_Box(Transform t, Vector2 size, Vector2 offset = default)
    {
        BoxCollider2D c = t.gameObject.AddComponent<BoxCollider2D>();
        c.size = size;
        c.offset = offset;
        return c;
    }

    /// <summary>Collider su un figlio dedicato (per avere più aree diverse sullo stesso oggetto).</summary>
    static BoxCollider2D HW_Area(Transform parent, string name, Vector2 size, Vector2 offset)
    {
        return HW_Box(Child(parent, name, Vector2.zero).transform, size, offset);
    }

    static SmokeEmitter[] HW_Confetti(Transform root)
    {
        var list = new List<SmokeEmitter>();
        foreach (Color c in new[] { new Color(1f, 0.55f, 0.12f), new Color(0.62f, 0.4f, 0.95f), new Color(0.55f, 0.9f, 0.35f), new Color(1f, 0.9f, 0.4f) })
        {
            SmokeEmitter e = Emitter(root, "Confetti", new Vector2(0f, 3.6f), "confetti", 22f, 2.4f, -1.6f, 2.8f, 1.4f, c, 80);
            e.sideDrift = 0.6f;
            e.growth = 0f;
            e.spinRange = 300f;
            list.Add(e);
        }
        return list.ToArray();
    }

    /// <summary>Scritta di commento su fascia scura (nascosta all'avvio, la mostra HwToast).</summary>
    static TextMesh HW_Toast(Transform root, Vector2 pos, out Transform holder)
    {
        holder = Child(root, "Toast", pos).transform;
        Bar(holder, "Back", Vector2.zero, new Vector2(4.8f, 0.5f), new Color(0.1f, 0.06f, 0.16f, 0.7f), 72);
        TextMesh t = Label(holder, Vector2.zero, 73, "", 0.042f);
        t.color = Color.white;
        Fit(t, 4.6f, 0.4f);
        return t;
    }

    // ---------- 245 Toni the were-pug ----------

    static GameObject BuildHwWerepug()
    {
        var root = new GameObject("Level_HwWerepug");
        HwWerepugLevel level = root.AddComponent<HwWerepugLevel>();
        Art(root.transform, "Night", "hw_night_bg", -20, false);
        SpriteRenderer glow = Art(root.transform, "MoonGlow", "glow", -16, false, new Vector2(1.15f, 2.05f)).GetComponent<SpriteRenderer>();
        glow.transform.localScale = Vector3.one * 4.2f;
        glow.color = new Color(1f, 0.95f, 0.7f, 0.65f);
        Transform moon = HW_Obj(root.transform, "Moon", new Vector2(1.15f, 2.05f), "hw_moon", -15);
        CircleCollider2D moonCol = moon.gameObject.AddComponent<CircleCollider2D>();
        moonCol.radius = 0.85f;

        Transform pug = HW_Obj(root.transform, "Werepug", new Vector2(-0.75f, -3.25f), "hw_werepug", 5, 1.15f);
        var breathe = pug.Find("Art").gameObject.AddComponent<Pulse>();
        breathe.amount = 0.015f;
        breathe.speed = 2.5f;
        BoxCollider2D pugCol = HW_Box(pug, new Vector2(2.3f, 2.8f), new Vector2(0f, 1.4f));

        // Toni normale (di lato, guarda verso la luna): corpo, occhi aperti con le pupille, codina
        Transform toni = Child(root.transform, "ToniNormal", new Vector2(-0.8f, -2.0f)).transform;
        toni.localScale = new Vector3(-1.3f, 1.3f, 1f);
        Transform tail = Child(toni, "Tail", new Vector2(0.98f, 0.36f)).transform;
        Art(tail, "Art", "pug_tail", 4, false);
        Art(toni, "Body", "toni_stand", 5);
        Art(toni, "Eyes", "pug_eyes", 6, false);
        Art(toni, "PupilL", "pug_pupil", 7, false, new Vector2(Gotcha.EditorArt.DoodleSprites.PugEyeLX + 0.03f, Gotcha.EditorArt.DoodleSprites.PugEyeY + 0.02f));
        Art(toni, "PupilR", "pug_pupil", 7, false, new Vector2(Gotcha.EditorArt.DoodleSprites.PugEyeRX + 0.03f, Gotcha.EditorArt.DoodleSprites.PugEyeY + 0.02f));

        Transform box = HW_Obj(root.transform, "Box", new Vector2(1.55f, -3.55f), "hw_biscuit_box", 6);
        BoxCollider2D boxCol = HW_Box(box, new Vector2(1.4f, 1.1f), new Vector2(0f, 0.05f));
        Transform spot = Child(root.transform, "BiscuitSpot", new Vector2(0.35f, -3.85f)).transform;
        Transform biscuit = HW_Obj(root.transform, "Biscuit", new Vector2(1.55f, -3.55f), "hw_biscuit", 20);
        CircleCollider2D biscuitCol = biscuit.gameObject.AddComponent<CircleCollider2D>();
        biscuitCol.radius = 0.45f;
        SpriteRenderer tint = Bar(root.transform, "NightTint", Vector2.zero, new Vector2(8f, 16f), new Color(0.05f, 0.03f, 0.12f, 0f), 30);

        SmokeEmitter smoke = Emitter(root.transform, "Smoke", new Vector2(-0.75f, -2.0f), "puff", 16f, 1.2f, 0.8f, 0.9f, 0.9f, new Color(0.6f, 0.55f, 0.75f, 0.9f), 40);
        SmokeEmitter hearts = Emitter(root.transform, "Hearts", new Vector2(-1.0f, -1.0f), "heart", 4f, 1.2f, 0.8f, 0.4f, 0.8f, Color.white, 35);
        HW_Goal(root.transform, "HELP TONI TURN BACK TO NORMAL!");

        SetField(level, "moon", moon);
        SetField(level, "moonGlow", glow);
        SetField(level, "nightTint", tint);
        SetField(level, "werepug", pug);
        SetField(level, "werepugArea", pugCol);
        SetField(level, "toniNormal", toni.gameObject);
        SetField(level, "toniTail", tail);
        SetField(level, "box", box);
        SetField(level, "boxArt", box.Find("Art").GetComponent<SpriteRenderer>());
        SetField(level, "boxOpenSprite", sprites["hw_biscuit_box_open"]);
        SetField(level, "boxArea", boxCol);
        SetField(level, "moonArea", moonCol);
        SetField(level, "biscuit", biscuit);
        SetField(level, "biscuitArea", biscuitCol);
        SetField(level, "biscuitSpot", spot);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.2f, -0.1f)));
        SetField(level, "smoke", smoke);
        SetField(level, "hearts", hearts);
        SetArray(level, "confetti", HW_Confetti(root.transform));
        return SaveLevel(root, "HwWerepug");
    }

    // ---------- 246 The perfect pumpkin ----------

    static GameObject BuildHwPumpkin()
    {
        var root = new GameObject("Level_HwPumpkin");
        HwPumpkinLevel level = root.AddComponent<HwPumpkinLevel>();
        Art(root.transform, "Room", "hw_room_bg", -20, false);
        Transform lamp = HW_Obj(root.transform, "Lamp", new Vector2(-0.9f, 2.75f), "hw_ceiling_lamp", 2);
        SpriteRenderer lampLight = Art(lamp, "Light", "hw_lamp_light", 1, false, new Vector2(0f, -0.05f)).GetComponent<SpriteRenderer>();
        BoxCollider2D lampCol = HW_Box(lamp, new Vector2(1.3f, 1.0f), new Vector2(0f, 0.15f));
        Art(root.transform, "Table", "hw_table", 0, false, new Vector2(0f, -1.75f));

        Transform pumpkin = HW_Obj(root.transform, "Pumpkin", new Vector2(0f, -0.55f), "hw_pumpkin", 10);
        Transform face = Child(pumpkin, "Face", new Vector2(0f, -0.05f)).transform;
        Transform glowT = Art(pumpkin, "Glow", "glow", 60, false);
        glowT.localScale = new Vector3(3.4f, 2.8f, 1f);
        SpriteRenderer pumpkinGlow = glowT.GetComponent<SpriteRenderer>();
        pumpkinGlow.color = new Color(1f, 0.62f, 0.15f, 0f);
        Transform inside = Child(pumpkin, "CandleSpot", new Vector2(0f, -0.75f)).transform;

        Transform knife = HW_Obj(root.transform, "Knife", new Vector2(-2.05f, -1.62f), "hw_knife", 14, 1.3f);
        BoxCollider2D knifeCol = HW_Box(knife, new Vector2(1.3f, 0.6f));
        Transform candle = HW_Obj(root.transform, "Candle", new Vector2(1.5f, -1.75f), "hw_candle", 15, 1.3f);
        BoxCollider2D candleCol = HW_Box(candle, new Vector2(0.7f, 0.95f), new Vector2(0f, 0.35f));
        Transform wick = Child(candle, "Wick", new Vector2(0f, 0.6f)).transform;
        Transform flame = Art(candle, "Flame", "flame", 63, false, new Vector2(0f, 0.58f));
        Transform cglow = Art(candle, "Glow", "glow", 62, false, new Vector2(0f, 0.7f));
        cglow.localScale = Vector3.one * 1.6f;
        cglow.GetComponent<SpriteRenderer>().color = new Color(1f, 0.75f, 0.3f, 0.85f);
        Transform lighter = HW_Obj(root.transform, "Lighter", new Vector2(2.3f, -1.45f), "hw_lighter", 16, 1.3f);
        BoxCollider2D lighterCol = HW_Box(lighter, new Vector2(0.65f, 0.9f));
        Transform tip = Child(lighter, "Tip", new Vector2(0f, 0.34f)).transform;
        SmokeEmitter spark = Emitter(lighter, "Spark", new Vector2(0f, 0.4f), "sparkle_star", 20f, 0.3f, 0.4f, 0.1f, 0.25f, new Color(1f, 0.85f, 0.3f), 64);

        Transform sw = HW_Obj(root.transform, "Switch", new Vector2(2.25f, 1.0f), "switch_on", 55, 1.5f);
        BoxCollider2D swCol = HW_Box(sw, new Vector2(0.8f, 0.95f));
        SpriteRenderer dark = Bar(root.transform, "Darkness", Vector2.zero, new Vector2(8f, 16f), new Color(0.03f, 0.02f, 0.08f, 0f), 50);

        Transform clear = Child(root.transform, "NewFace", new Vector2(-1.75f, -3.35f)).transform;
        Art(clear, "Art", "hw_button", 20, false).GetComponent<SpriteRenderer>().color = new Color(1f, 0.75f, 0.45f);
        Fit(Label(clear, new Vector2(0f, 0.02f), 21, "NEW FACE", 0.045f), 1.3f, 0.4f);
        BoxCollider2D clearCol = HW_Box(clear, new Vector2(1.7f, 0.85f));

        SmokeEmitter smoke = Emitter(root.transform, "Smoke", new Vector2(0f, 0.75f), "puff", 14f, 1.4f, 0.9f, 0.5f, 0.7f, new Color(0.85f, 0.82f, 0.9f, 0.85f), 65);
        TextMesh info = HW_Toast(root.transform, new Vector2(0f, 2.45f), out Transform infoHolder);
        HW_Goal(root.transform, "CARVE THE PERFECT HALLOWEEN PUMPKIN!");

        SetField(level, "face", face);
        SetField(level, "pumpkin", pumpkin);
        SetField(level, "cutMaterial", StrokeMaterial());
        SetField(level, "knife", knife);
        SetField(level, "knifeArea", knifeCol);
        SetField(level, "candle", candle);
        SetField(level, "candleArea", candleCol);
        SetField(level, "candleArt", candle.Find("Art").GetComponent<SpriteRenderer>());
        SetField(level, "candleWick", wick);
        SetField(level, "flame", flame.gameObject);
        SetField(level, "candleGlow", cglow.GetComponent<SpriteRenderer>());
        SetField(level, "candleInsideSpot", inside);
        SetField(level, "lighter", lighter);
        SetField(level, "lighterArea", lighterCol);
        SetField(level, "lighterTip", tip);
        SetField(level, "lighterSpark", spark);
        SetField(level, "lampArea", lampCol);
        SetField(level, "lamp", lamp);
        SetField(level, "lampLight", lampLight);
        SetField(level, "switchArea", swCol);
        SetField(level, "switchArt", sw.Find("Art").GetComponent<SpriteRenderer>());
        SetField(level, "switchOn", sprites["switch_on"]);
        SetField(level, "switchOff", sprites["switch_off"]);
        SetField(level, "darkness", dark);
        SetField(level, "pumpkinGlow", pumpkinGlow);
        SetField(level, "clearArea", clearCol);
        SetField(level, "clearButton", clear);
        SetField(level, "smoke", smoke);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.9f, 1.3f)));
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        return SaveLevel(root, "HwPumpkin");
    }

    // ---------- 247 The ghost who can't scare ----------

    static GameObject BuildHwGhost()
    {
        var root = new GameObject("Level_HwGhost");
        HwGhostLevel level = root.AddComponent<HwGhostLevel>();
        Art(root.transform, "Bedroom", "hw_bedroom_bg", -20, false);

        HW_Obj(root.transform, "Bed", new Vector2(0.6f, -2.9f), "hw_bed", 2, 0.85f);
        Transform man = HW_Obj(root.transform, "Man", new Vector2(-0.7f, -2.45f), "hw_man_sleep", 3, 0.9f);
        BoxCollider2D manCol = HW_Area(root.transform, "ManArea", new Vector2(4.0f, 2.1f), new Vector2(0.55f, -2.9f));
        SmokeEmitter zzz = Emitter(root.transform, "Zzz", new Vector2(-0.45f, -2.1f), "zzz", 1.3f, 2.2f, 0.45f, 0.1f, 0.7f, Color.white, 30);
        zzz.spinRange = 12f;
        zzz.growth = 0.9f;
        zzz.sideDrift = 0.15f;

        // comodino con la lampada, sveglia per terra, specchio a destra
        Art(root.transform, "Nightstand", "hw_nightstand", 3, false, new Vector2(-2.35f, -2.3f));
        HW_Obj(root.transform, "Lamp", new Vector2(-2.35f, -2.3f), "bedside_lamp", 4, 0.55f);
        SpriteRenderer lampGlow = Art(root.transform, "LampGlow", "glow", 1, false, new Vector2(-2.35f, -1.55f)).GetComponent<SpriteRenderer>();
        lampGlow.transform.localScale = Vector3.one * 3.2f;
        lampGlow.color = new Color(1f, 0.9f, 0.5f, 0.6f);
        BoxCollider2D lampCol = HW_Area(root.transform, "LampArea", new Vector2(1.0f, 1.2f), new Vector2(-2.35f, -1.75f));
        Transform alarm = HW_Obj(root.transform, "Alarm", new Vector2(-2.2f, -4.4f), "alarm_clock", 4, 0.42f);
        BoxCollider2D alarmCol = HW_Box(alarm, new Vector2(2.2f, 2.0f), new Vector2(0f, 0.8f));
        SmokeEmitter waves = Emitter(root.transform, "AlarmWaves", new Vector2(-2.2f, -3.7f), "ring_waves", 6f, 0.5f, 0.3f, 0.05f, 0.35f, Color.white, 20);
        waves.growth = 0.8f;
        waves.spinRange = 0f;
        Transform mirror = HW_Obj(root.transform, "Mirror", new Vector2(2.05f, -0.15f), "hw_mirror", 4, 0.9f);
        Transform refl = Art(mirror, "Reflection", "hw_man_ghost", 5, false, new Vector2(0f, -0.1f));
        refl.localScale = new Vector3(-0.75f, 0.75f, 1f);
        SpriteRenderer roomDark = Bar(root.transform, "RoomDark", Vector2.zero, new Vector2(8f, 16f), new Color(0.02f, 0.02f, 0.08f, 0f), 40);

        // il fantasma (col lenzuolo o invisibile) e il lenzuolo trascinabile
        Transform ghost = Child(root.transform, "Ghost", new Vector2(-0.55f, 0.95f)).transform;
        GameObject ghostSheet = Art(ghost, "Sheet", "hw_ghost", 10, false).gameObject;
        SpriteRenderer ghostInv = Art(ghost, "Invisible", "hw_ghost_invisible", 10, false).GetComponent<SpriteRenderer>();
        BoxCollider2D ghostCol = HW_Box(ghost, new Vector2(1.5f, 1.8f));
        Transform sheet = HW_Obj(root.transform, "Sheet", new Vector2(-0.55f, 0.95f), "hw_sheet", 45);
        BoxCollider2D sheetCol = HW_Box(sheet, new Vector2(1.6f, 1.8f));
        Transform floorSpot = Child(root.transform, "SheetFloorSpot", new Vector2(0.2f, -4.4f)).transform;

        HW_Goal(root.transform, "HELP THE GHOST SCARE THE MAN!");
        SetField(level, "ghost", ghost);
        SetField(level, "ghostSheetArt", ghostSheet);
        SetField(level, "ghostInvisible", ghostInv);
        SetField(level, "ghostArea", ghostCol);
        SetField(level, "sheet", sheet);
        SetField(level, "sheetArea", sheetCol);
        SetField(level, "sheetArt", sheet.Find("Art").GetComponent<SpriteRenderer>());
        SetField(level, "sheetFloorSpot", floorSpot);
        SetField(level, "man", man);
        SetField(level, "manArt", man.Find("Art").GetComponent<SpriteRenderer>());
        SetField(level, "manSleep", sprites["hw_man_sleep"]);
        SetField(level, "manAwake", sprites["hw_man_awake"]);
        SetField(level, "manGhost", sprites["hw_man_ghost"]);
        SetField(level, "manArea", manCol);
        SetField(level, "zzz", zzz);
        SetField(level, "mirrorReflection", refl.GetComponent<SpriteRenderer>());
        SetField(level, "alarm", alarm);
        SetField(level, "alarmArea", alarmCol);
        SetField(level, "alarmWaves", waves);
        SetField(level, "lampArea", lampCol);
        SetField(level, "lampGlow", lampGlow);
        SetField(level, "roomDark", roomDark);
        SetField(level, "ghostBubble", SmallBubble(root.transform, new Vector2(-0.15f, 2.05f)));
        SetField(level, "manBubble", SmallBubble(root.transform, new Vector2(-0.25f, -1.65f)));
        SetArray(level, "confetti", HW_Confetti(root.transform));
        return SaveLevel(root, "HwGhost");
    }

    // ---------- 248 The witch and the impossible potion ----------

    static GameObject BuildHwWitch()
    {
        var root = new GameObject("Level_HwWitch");
        HwWitchLevel level = root.AddComponent<HwWitchLevel>();
        Art(root.transform, "Hut", "hw_witch_bg", -20, false);

        // la frase in alto, fatta di pezzi affiancati: la parola BEAUTIFUL è un pezzo a sé
        Bar(root.transform, "SentenceBack", new Vector2(0f, 3.0f), new Vector2(5.7f, 1.3f), new Color(0.1f, 0.06f, 0.16f, 0.8f), 69);
        TextMesh line1 = Label(root.transform, new Vector2(0f, 3.42f), 71, "THE WITCH WANTS", 0.045f);
        line1.color = Color.white;
        Fit(line1, 5.4f, 0.4f);
        var row = new List<TextMesh>();
        foreach (string piece in new[] { "TO BE", "BEAUTIFUL", "." })
        {
            bool word = piece == "BEAUTIFUL";
            TextMesh t = Label(root.transform, new Vector2(0f, 2.98f), word ? 75 : 71, piece, word ? 0.062f : 0.045f, false);
            t.gameObject.name = word ? "Word" : "Piece";
            t.color = word ? new Color(1f, 0.55f, 0.8f) : Color.white;
            row.Add(t);
        }
        BoxCollider2D wordCol = row[1].gameObject.AddComponent<BoxCollider2D>();
        wordCol.size = new Vector2(2.6f, 0.8f);
        SpriteRenderer gap = Bar(root.transform, "Gap", new Vector2(0f, 2.78f), new Vector2(1f, 0.06f), new Color(1f, 1f, 1f, 0.7f), 71);
        TextMesh line3 = Label(root.transform, new Vector2(0f, 2.56f), 71, "BREW THE RIGHT POTION!", 0.04f);
        line3.color = new Color(1f, 0.86f, 0.45f);
        Fit(line3, 5.4f, 0.36f);

        // la strega e le sue trasformazioni
        Transform witch = HW_Obj(root.transform, "Witch", new Vector2(-1.5f, -3.35f), "hw_witch", 5, 0.95f);
        SpriteRenderer witchArt = witch.Find("Art").GetComponent<SpriteRenderer>();
        GameObject eyes = Art(witch, "Eyes", "hw_witch_eyes", 6, false).gameObject;
        Transform phone = Art(witch, "Phone", "hw_phone", 8, false, new Vector2(0.88f, 1.45f));
        phone.localScale = Vector3.one * 1.3f;
        SmokeEmitter hearts = Emitter(root.transform, "Hearts", new Vector2(-1.4f, -0.6f), "heart", 5f, 1.2f, 0.9f, 0.5f, 0.8f, Color.white, 30);
        SmokeEmitter puff = Emitter(root.transform, "WitchPuff", new Vector2(-1.5f, -1.9f), "puff", 18f, 1.0f, 0.6f, 0.8f, 0.9f, new Color(0.75f, 0.6f, 0.95f, 0.9f), 40);

        // calderone
        Transform cauldron = HW_Obj(root.transform, "Cauldron", new Vector2(1.2f, -2.5f), "hw_cauldron", 3);
        SpriteRenderer brew = Art(cauldron, "Brew", "hw_brew", 4, false).GetComponent<SpriteRenderer>();
        BoxCollider2D mouth = HW_Area(cauldron, "Mouth", new Vector2(2.5f, 2.2f), new Vector2(0f, 0.45f));
        SmokeEmitter bubbles = Emitter(root.transform, "Bubbles", new Vector2(1.2f, -1.9f), "puff", 3f, 1.4f, 0.5f, 0.6f, 0.25f, Color.white, 6);
        SmokeEmitter splash = Emitter(root.transform, "Splash", new Vector2(1.2f, -1.9f), "puff", 25f, 0.6f, 1.0f, 0.6f, 0.4f, new Color(0.55f, 0.9f, 0.4f, 0.9f), 7);

        // ingredienti sulla mensola (ordine di WitchIngredient: rospo, occhio, fungo, cuore, mela)
        var items = new List<Transform>();
        var areas = new List<Collider2D>();
        string[] art = { "hw_toad", "hw_eye", "hw_mushroom", "hw_heart", "apple" };
        float[] scale = { 1.2f, 1.2f, 1.2f, 1.25f, 0.4f };
        float[] y = { 1.58f, 1.6f, 1.62f, 1.62f, 1.68f };
        for (int i = 0; i < art.Length; i++)
        {
            Transform it = HW_Obj(root.transform, "Ingredient" + i, new Vector2(-2.2f + i * 1.1f, y[i]), art[i], 10, scale[i], true);
            items.Add(it);
            areas.Add(HW_Box(it, Vector2.one * (0.95f / scale[i]), i == 4 ? new Vector2(0f, 0.15f / scale[i]) : Vector2.zero));
        }

        SpriteRenderer flash = Bar(root.transform, "Flash", Vector2.zero, new Vector2(8f, 16f), new Color(1f, 1f, 1f, 0f), 90);

        SetField(level, "witch", witch);
        SetField(level, "witchArt", witchArt);
        SetField(level, "witchNormal", sprites["hw_witch"]);
        SetField(level, "witchGlam", sprites["hw_witch_glam"]);
        SetField(level, "witchToad", sprites["hw_witch_toad"]);
        SetField(level, "witchApple", sprites["hw_witch_apple"]);
        SetField(level, "eyesOverlay", eyes);
        SetField(level, "hearts", hearts);
        SetField(level, "witchPuff", puff);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.95f, 0.3f)));
        SetField(level, "cauldron", cauldron);
        SetField(level, "cauldronArea", mouth);
        SetField(level, "brew", brew);
        SetField(level, "brewBubbles", bubbles);
        SetField(level, "splash", splash);
        SetArray(level, "ingredients", items.ToArray());
        SetArray(level, "ingredientAreas", areas.ToArray());
        SetArray(level, "rowPieces", row.ToArray());
        SetField(level, "wordArea", wordCol);
        SetField(level, "gapMark", gap);
        SetField(level, "phone", phone.gameObject);
        SetField(level, "flash", flash);
        SetArray(level, "confetti", HW_Confetti(root.transform));
        return SaveLevel(root, "HwWitch");
    }

    // ---------- 249 The vegetarian zombie ----------

    static GameObject BuildHwZombie()
    {
        var root = new GameObject("Level_HwZombie");
        HwZombieLevel level = root.AddComponent<HwZombieLevel>();
        Art(root.transform, "Graveyard", "hw_graveyard_bg", -20, false);

        Transform zombie = Child(root.transform, "Zombie", new Vector2(-1.3f, -3.8f)).transform;
        Art(zombie, "Body", "hw_zombie_body", 5);
        SpriteRenderer head = Art(zombie, "Head", "hw_zombie_head", 6).GetComponent<SpriteRenderer>();
        Art(zombie, "Arm", "hw_zombie_arm", 6, true, new Vector2(0.42f, 1.72f));
        BoxCollider2D zombieCol = HW_Box(zombie, new Vector2(1.9f, 2.9f), new Vector2(0f, 1.4f));
        Transform mouthT = Child(zombie, "Mouth", new Vector2(0.02f, 2.08f)).transform;

        // il fumetto fatto di tessere-lettera
        Transform balloon = Child(root.transform, "Balloon", new Vector2(0f, 1.5f)).transform;
        SpriteRenderer frame = Art(balloon, "Frame", "hw_balloon", 40, false).GetComponent<SpriteRenderer>();
        frame.drawMode = SpriteDrawMode.Sliced;
        frame.size = new Vector2(5.57f, 1.1f);
        Transform tail = Art(balloon, "Tail", "hw_balloon_tail", 39, false, new Vector2(-1.3f, -0.5f));
        tail.localScale = new Vector3(1f, 2.1f, 1f);
        SpriteRenderer glow = Art(balloon, "Glow", "plain", 38, false).GetComponent<SpriteRenderer>();
        glow.color = new Color(1f, 0.95f, 0.4f, 0f);
        BoxCollider2D balloonCol = HW_Box(balloon, new Vector2(5.57f, 1.1f));
        var tiles = new List<Transform>();
        var tileCols = new List<Collider2D>();
        string word = Gotcha.Levels.Halloween.ZombieWord.DefaultOriginal;
        for (int i = 0; i < word.Length; i++)
        {
            Transform t = Child(balloon, "Tile" + i, new Vector2((i - (word.Length - 1) * 0.5f) * 0.64f, 0f)).transform;
            Art(t, "Art", "hw_tile", 41, false);
            Label(t, new Vector2(0f, 0.01f), 42, word[i].ToString(), 0.075f, false).color = new Color(0.45f, 0.12f, 0.2f);
            tiles.Add(t);
            tileCols.Add(HW_Box(t, new Vector2(0.62f, 0.66f)));
        }
        Transform undo = HW_Obj(balloon, "Undo", new Vector2(2.25f, 0.65f), "hw_undo", 43, 1.3f);
        CircleCollider2D undoCol = undo.gameObject.AddComponent<CircleCollider2D>();
        undoCol.radius = 0.36f;

        // il cibo: cervello, hamburger e tre broccoli
        Transform brain = HW_Obj(root.transform, "Brain", new Vector2(-0.15f, -4.05f), "hw_brain", 12, 1.15f, true);
        BoxCollider2D brainCol = HW_Box(brain, new Vector2(0.85f, 0.75f));
        Transform burger = HW_Obj(root.transform, "Burger", new Vector2(0.8f, -4.05f), "hw_burger", 12, 1.15f, true);
        BoxCollider2D burgerCol = HW_Box(burger, new Vector2(0.8f, 0.75f));
        var brocs = new List<Transform>();
        var brocCols = new List<Collider2D>();
        foreach (Vector2 at in new[] { new Vector2(1.6f, -4.1f), new Vector2(2.3f, -4.1f), new Vector2(1.95f, -3.5f) })
        {
            Transform b = HW_Obj(root.transform, "Broccoli", at, "hw_broccoli", at.y > -3.8f ? 11 : 12, 1.15f, true);
            brocs.Add(b);
            brocCols.Add(HW_Box(b, new Vector2(0.75f, 0.75f)));
        }

        SmokeEmitter hearts = Emitter(root.transform, "Hearts", new Vector2(-1.3f, -0.85f), "heart", 4f, 1.3f, 0.8f, 0.4f, 0.8f, Color.white, 35);
        SmokeEmitter burp = Emitter(root.transform, "Burp", new Vector2(-1.1f, -1.7f), "puff", 18f, 1.2f, 0.7f, 0.4f, 0.8f, new Color(0.55f, 0.85f, 0.3f, 0.9f), 35);
        burp.sideDrift = 0.6f;
        SmokeEmitter crumbs = Emitter(root.transform, "Crumbs", new Vector2(-1.25f, -1.75f), "puff", 26f, 0.5f, -1.0f, 0.3f, 0.25f, new Color(0.55f, 0.8f, 0.35f), 30);
        HW_Goal(root.transform, "THE ZOMBIE IS STARVING. GIVE HIM SOMETHING HE LIKES!");

        SetField(level, "zombie", zombie);
        SetField(level, "zombieHead", head);
        SetField(level, "headNormal", sprites["hw_zombie_head"]);
        SetField(level, "headHappy", sprites["hw_zombie_head_happy"]);
        SetField(level, "zombieArea", zombieCol);
        SetField(level, "mouth", mouthT);
        SetField(level, "balloon", balloon);
        SetField(level, "balloonFrame", frame);
        SetField(level, "balloonArea", balloonCol);
        SetField(level, "balloonGlow", glow);
        SetArray(level, "tiles", tiles.ToArray());
        SetArray(level, "tileAreas", tileCols.ToArray());
        SetField(level, "undoButton", undo);
        SetField(level, "undoArea", undoCol);
        SetField(level, "brain", brain);
        SetField(level, "burger", burger);
        SetField(level, "brainArea", brainCol);
        SetField(level, "burgerArea", burgerCol);
        SetArray(level, "broccoli", brocs.ToArray());
        SetArray(level, "broccoliAreas", brocCols.ToArray());
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.05f, -0.95f)));
        SetField(level, "hearts", hearts);
        SetField(level, "burp", burp);
        SetField(level, "crumbs", crumbs);
        return SaveLevel(root, "HwZombie");
    }

    // ---------- 250 Trick or treat! ----------

    static GameObject BuildHwTrickOrTreat()
    {
        var root = new GameObject("Level_HwTrickOrTreat");
        HwTrickOrTreatLevel level = root.AddComponent<HwTrickOrTreatLevel>();
        Art(root.transform, "House", "hw_house_bg", -20, false);

        // porta, signora, cartello, campanello
        GameObject doorClosed = Art(root.transform, "DoorClosed", "hw_door", 2, false, new Vector2(1.45f, -0.95f)).gameObject;
        Transform doorOpen = Child(root.transform, "DoorOpen", new Vector2(1.45f, -0.95f)).transform;
        Art(doorOpen, "Art", "hw_doorway", 2, false);
        SpriteRenderer lady = Art(doorOpen, "Lady", "hw_lady", 3, false, new Vector2(0f, -0.2f)).GetComponent<SpriteRenderer>();
        Transform sign = Child(root.transform, "Sign", new Vector2(1.45f, 1.05f)).transform;
        Bar(sign, "Board", Vector2.zero, new Vector2(2.75f, 0.6f), new Color(0.55f, 0.36f, 0.22f), 3);
        TextMesh signText = Label(sign, Vector2.zero, 4, "ONE TREAT PER CHILD!", 0.04f);
        signText.color = new Color(1f, 0.93f, 0.75f);
        Fit(signText, 2.55f, 0.45f);
        Transform bell = HW_Obj(root.transform, "Bell", new Vector2(2.5f, -0.75f), "hw_doorbell", 3, 1.3f);
        CircleCollider2D bellCol = bell.gameObject.AddComponent<CircleCollider2D>();
        bellCol.radius = 0.4f;
        SmokeEmitter bellWaves = Emitter(root.transform, "BellWaves", new Vector2(2.5f, -0.75f), "ring_waves", 8f, 0.5f, 0.2f, 0.05f, 0.3f, Color.white, 20);
        bellWaves.growth = 0.8f;
        bellWaves.spinRange = 0f;
        Transform doorSpot = Child(root.transform, "DoorSpot", new Vector2(0.3f, -3.05f)).transform;
        Transform toniSpot = Child(root.transform, "ToniDoorSpot", new Vector2(1.5f, -3.05f)).transform;
        Transform armGround = Child(root.transform, "ArmGround", new Vector2(0f, -3.3f)).transform;

        // lo zombie (corpo, testa, punti d'attacco) con i pezzi del costume
        Transform zombie = Child(root.transform, "Zombie", new Vector2(-0.85f, -3.4f)).transform;
        zombie.localScale = Vector3.one * 0.72f;
        Art(zombie, "Body", "hw_zombie_body", 10);
        GameObject zHead = Art(zombie, "Head", "hw_zombie_head", 11).gameObject;
        Transform headSpot = Child(zombie, "HeadSpot", new Vector2(0f, 1.62f)).transform;
        Transform anchor = Child(zombie, "ArmAnchor", new Vector2(0.42f, 1.72f)).transform;
        BoxCollider2D zPick = HW_Area(zombie, "PickArea", new Vector2(1.4f, 2.6f), new Vector2(-0.1f, 1.3f));
        BoxCollider2D zDrop = HW_Area(zombie, "DropArea", new Vector2(2.4f, 3.8f), new Vector2(0f, 1.7f));
        GameObject bigSheet = Art(zombie, "BigSheet", "hw_big_sheet", 14, false).gameObject;
        GameObject hat = Art(zombie, "Hat", "hw_witch_hat", 15, false, new Vector2(0f, 3.0f)).gameObject;
        Transform tailPeek = HW_Obj(zombie, "TailPeek", new Vector2(0.4f, 0.7f), "pug_tail", 16, 1.8f);
        SmokeEmitter spell = Emitter(root.transform, "Spell", new Vector2(0.3f, -1.6f), "sparkle_star", 14f, 1.0f, 0.4f, 1.0f, 0.5f, new Color(1f, 0.9f, 0.4f), 30);
        spell.spinRange = 200f;
        Transform arm = HW_Obj(root.transform, "ZombieArm", new Vector2(-0.85f + 0.42f * 0.72f, -3.4f + 1.72f * 0.72f), "hw_zombie_arm", 12, 0.72f);
        BoxCollider2D armCol = HW_Box(arm, new Vector2(1.25f, 1.1f), new Vector2(0.45f, -0.3f));

        // Toni lupo-carlino
        Transform toni = HW_Obj(root.transform, "Toni", new Vector2(-2.1f, -4.45f), "hw_werepug", 20, 0.5f, true);
        BoxCollider2D toniCol = HW_Box(toni, new Vector2(2.5f, 2.9f), new Vector2(0f, 1.35f));
        Transform toniMouth = Child(toni, "Mouth", new Vector2(0.1f, 1.55f)).transform;
        Transform crown = HW_Obj(toni, "Crown", new Vector2(0f, 2.55f), "hw_crown", 22, 1.7f);
        SmokeEmitter toniHearts = Emitter(toni, "Hearts", new Vector2(0f, 2.8f), "heart", 5f, 1.2f, 0.8f, 0.4f, 0.7f, Color.white, 35);

        // zucca, fantasma, strega
        Transform kid = HW_Obj(root.transform, "PumpkinKid", new Vector2(0.25f, -4.5f), "hw_pumpkin_kid", 18, 0.8f, true);
        BoxCollider2D kidCol = HW_Box(kid, new Vector2(1.5f, 1.6f), new Vector2(0f, 0.65f));
        Transform ghost = Child(root.transform, "Ghost", new Vector2(-1.05f, 0.45f)).transform;
        ghost.localScale = Vector3.one * 0.7f;
        Transform ghostBob = Child(ghost, "Bob", Vector2.zero).transform;
        Art(ghostBob, "Art", "hw_ghost", 8, false);
        BoxCollider2D ghostCol = HW_Box(ghost, new Vector2(1.8f, 2.0f));
        Transform witch = HW_Obj(root.transform, "Witch", new Vector2(-2.15f, -2.75f), "hw_witch", 9, 0.55f, true);
        BoxCollider2D witchCol = HW_Box(witch, new Vector2(1.7f, 3.5f), new Vector2(0.1f, 1.7f));

        // caramelle, cioccolatino, secchiello
        Transform choc = HW_Obj(root.transform, "Chocolate", new Vector2(1.45f, -1.0f), "hw_chocolate", 80, 1.5f);
        Transform bucket = HW_Obj(root.transform, "Bucket", new Vector2(1.45f, -1.0f), "hw_bucket", 30, 1.2f);
        var candies = new List<Transform>();
        Color[] colors = { new Color(1f, 0.4f, 0.55f), new Color(0.35f, 0.65f, 1f), new Color(1f, 0.85f, 0.3f), new Color(0.5f, 0.85f, 0.4f), new Color(0.75f, 0.5f, 1f), new Color(1f, 0.6f, 0.25f) };
        for (int i = 0; i < 12; i++)
        {
            Transform c = HW_Obj(root.transform, "Candy" + i, new Vector2(1.45f, -1.0f), "hw_candy", 31, 1.5f);
            c.Find("Art").GetComponent<SpriteRenderer>().color = colors[i % colors.Length];
            candies.Add(c);
        }
        Transform counter = Child(root.transform, "CandyCounter", new Vector2(-1.55f, 2.5f)).transform;
        Bar(counter, "Back", Vector2.zero, new Vector2(2.5f, 0.5f), new Color(0.1f, 0.06f, 0.16f, 0.7f), 70);
        TextMesh candyText = Label(counter, Vector2.zero, 71, "CANDY: 0 / 5", 0.045f);
        candyText.color = new Color(1f, 0.86f, 0.45f);
        Fit(candyText, 2.3f, 0.4f);

        // il messaggio dell'Easter egg
        Transform banner = Child(root.transform, "EggBanner", new Vector2(0f, 1.85f)).transform;
        Bar(banner, "Back", Vector2.zero, new Vector2(5.6f, 1.2f), new Color(0.32f, 0.12f, 0.45f, 0.95f), 85);
        TextMesh b1 = Label(banner, new Vector2(0f, 0.25f), 86, "EASTER EGG UNLOCKED", 0.06f);
        b1.color = new Color(1f, 0.86f, 0.3f);
        Fit(b1, 5.3f, 0.5f);
        TextMesh b2 = Label(banner, new Vector2(0f, -0.27f), 86, "THE REAL MONSTER IS THE PUG!", 0.05f);
        b2.color = Color.white;
        Fit(b2, 5.3f, 0.42f);

        HW_Goal(root.transform, "TRICK OR TREAT? GET ENOUGH CANDY FOR EVERYONE!");
        SetField(level, "zombie", zombie);
        SetField(level, "zombieArea", zDrop);
        SetField(level, "zombieBodyArea", zPick);
        SetField(level, "zombieHead", zHead);
        SetField(level, "zombieArm", arm);
        SetField(level, "armAnchor", anchor);
        SetField(level, "armArea", armCol);
        SetField(level, "headSpot", headSpot);
        SetField(level, "bigSheet", bigSheet);
        SetField(level, "witchHat", hat);
        SetField(level, "tailPeek", tailPeek.gameObject);
        SetField(level, "spell", spell);
        SetField(level, "toni", toni);
        SetField(level, "toniArea", toniCol);
        SetField(level, "toniMouth", toniMouth);
        SetField(level, "crown", crown.gameObject);
        SetField(level, "toniHearts", toniHearts);
        SetField(level, "pumpkinKid", kid);
        SetField(level, "pumpkinArea", kidCol);
        SetField(level, "ghost", ghost);
        SetField(level, "ghostArt", ghostBob);
        SetField(level, "ghostArea", ghostCol);
        SetField(level, "witch", witch);
        SetField(level, "witchArea", witchCol);
        SetField(level, "doorClosed", doorClosed);
        SetField(level, "doorOpen", doorOpen.gameObject);
        SetField(level, "lady", lady);
        SetField(level, "ladyNormal", sprites["hw_lady"]);
        SetField(level, "ladyScared", sprites["hw_lady_scared"]);
        SetField(level, "ladyLove", sprites["hw_lady_love"]);
        SetField(level, "bell", bell);
        SetField(level, "bellArea", bellCol);
        SetField(level, "bellWaves", bellWaves);
        SetField(level, "doorSpot", doorSpot);
        SetField(level, "toniDoorSpot", toniSpot);
        SetField(level, "armGroundSpot", armGround);
        SetField(level, "chocolate", choc);
        SetField(level, "bucket", bucket);
        SetArray(level, "candies", candies.ToArray());
        SetField(level, "candyText", candyText);
        SetField(level, "gangBubble", SmallBubble(root.transform, new Vector2(-1.55f, 1.55f)));
        SetField(level, "ladyBubble", SmallBubble(root.transform, new Vector2(1.25f, 0.0f)));
        SetField(level, "eggBanner", banner.gameObject);
        SetArray(level, "confetti", HW_Confetti(root.transform));
        return SaveLevel(root, "HwTrickOrTreat");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> VHalloween = new Dictionary<string, string[]>
    {
        { "hw_werepug", new[] { "The full moon causes the transformation.", "Cover the moon completely with the biscuit." } },
        { "hw_pumpkin", new[] { "It needs something to light it from the inside.", "Light the candle, put it in the pumpkin and switch off the room light." } },
        { "hw_ghost", new[] { "Maybe the sheet could be useful to someone else.", "Take the sheet off the ghost and put it on the man." } },
        { "hw_witch", new[] { "The answer might already be right in front of your eyes.", "Drag the word 'beautiful' into the cauldron." } },
        { "hw_zombie", new[] { "Read carefully what he's saying.", "Turn BRAAAINS into BRAIN and drag the balloon onto the broccoli." } },
        { "hw_trick_or_treat", new[] { "Maybe the five monsters could show up as one.", "Combine the five characters into one costume and ring the bell." } },
    };
}
#endif
