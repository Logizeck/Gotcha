#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.63: tre livelli nuovi (256-258) subito dopo i cinque della 0.62; Almost There e l'esame finale di Toni
/// scalano a 259-260. Senza Easter egg e senza Toni (che compare solo nei livelli multipli di 25). Stessi aiuti dei
/// livelli 0.62 (V62Level, V62_Wire, HW_*); i pochi disegni nuovi (prefisso v63_) sono in DoodleSpritesV63.
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels63() => new List<LevelDefinition>
    {
        Define("V63ChainReaction", "v63_chain_reaction", "CHAIN REACTION", "Every piece pushes the next one. Where does it all start?",
            BuildV63ChainReaction()),
        Define("V63LyingClocks", "v63_lying_clocks", "THE LYING CLOCKS", "Every clock lies. But one of them tells you HOW it lies.",
            BuildV63LyingClocks()),
        Define("V63ShadowPuppet", "v63_shadow_puppet", "SHADOW PUPPET", "Dogs are scared of one thing: a really, REALLY big cat.",
            BuildV63ShadowPuppet()),
    };

    // ---------- Aiuti ----------

    /// <summary>Tratteggio da a verso b (quadratini) con la punta di freccia in fondo: un gruppo che il livello colora.</summary>
    static Transform V63_Dashes(Transform parent, string name, Vector2 a, Vector2 b, int n, int order)
    {
        Transform g = Child(parent, name, Vector2.zero).transform;
        Color c = new Color(1f, 1f, 1f, 0.75f);
        for (int i = 0; i < n; i++)
        {
            Vector2 p = Vector2.Lerp(a, b, (i + 0.5f) / n);
            Bar(g, "Dash" + i, p, new Vector2(0.08f, 0.08f), c, order);
        }
        // la punta della freccia: due barrette a V
        Vector2 dir = (b - a).normalized;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        foreach (float s in new[] { 1f, -1f })
        {
            Vector2 off = (Vector2)(Quaternion.Euler(0f, 0f, ang + s * 140f) * Vector3.right) * 0.09f;
            SpriteRenderer bar = Bar(g, "Head", b + off, new Vector2(0.2f, 0.06f), c, order);
            bar.transform.localRotation = Quaternion.Euler(0f, 0f, ang + s * 140f);
        }
        return g;
    }

    /// <summary>Array di float in un campo serializzato.</summary>
    static void V63_SetFloats(Object target, string field, float[] values)
    {
        var so = new UnityEditor.SerializedObject(target);
        UnityEditor.SerializedProperty p = so.FindProperty(field);
        if (p == null || !p.isArray) { Debug.LogError("[Gotcha] Array non trovato: " + target.GetType().Name + "." + field); return; }
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).floatValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Oggetto col disegno spostato in modo che il punto <paramref name="pivot"/> del disegno sia l'origine.</summary>
    static Transform V63_Item(Transform parent, string name, Vector2 pos, string sprite, int order, float scale, Vector2 pivot)
    {
        Transform t = Child(parent, name, pos).transform;
        t.localScale = Vector3.one * scale;
        Art(t, "Art", sprite, order, false, -pivot);
        return t;
    }

    // ---------- 256 Chain reaction ----------

    static GameObject BuildV63ChainReaction()
    {
        var root = new GameObject("Level_V63ChainReaction");
        V63ChainReactionLevel level = root.AddComponent<V63ChainReactionLevel>();
        Art(root.transform, "Workshop", "workshop_bg", -20, false);
        Transform r = root.transform;

        // la mensola in alto a sinistra, il tavolo, il campanello appeso in alto a destra
        HW_Obj(r, "Shelf", new Vector2(-1.9f, 0.4f), "e40_shelf", 2, 0.22f);
        HW_Obj(r, "Table", new Vector2(1.05f, -1.25f), "hw_table", 1, 0.5f);
        Bar(r, "BellRod", new Vector2(1.6f, 2.95f), new Vector2(0.06f, 0.6f), new Color(0.25f, 0.25f, 0.28f), 2);
        Transform bell = HW_Obj(r, "Bell", new Vector2(1.6f, 2.05f), "bell", 4, 0.7f);
        BoxCollider2D bellCol = HW_Box(bell, new Vector2(1.3f, 1.3f), new Vector2(0f, 0.4f));
        SmokeEmitter bellWaves = Emitter(r, "BellWaves", new Vector2(1.6f, 2.4f), "ring_waves", 8f, 0.6f, 0.2f, 0.05f, 0.4f, Color.white, 20);
        bellWaves.growth = 0.9f;
        bellWaves.spinRange = 0f;

        // il palloncino legato al tavolo, sotto il campanello
        Bar(r, "Peg", new Vector2(1.6f, -1.12f), new Vector2(0.14f, 0.08f), new Color(0.35f, 0.24f, 0.16f), 5);
        Transform peg = Child(r, "PegPoint", new Vector2(1.6f, -1.1f)).transform;
        Transform balloon = Child(r, "Balloon", new Vector2(1.6f, -0.15f)).transform;
        Transform balloonArt = Art(balloon, "Art", "balloon", 6, false);
        balloonArt.localScale = Vector3.one * 0.5f;
        balloonArt.localPosition = centers["balloon"] * 0.5f;
        BoxCollider2D balloonCol = HW_Box(balloon, new Vector2(0.9f, 1.2f), new Vector2(0f, 0.55f));
        LineRenderer str = Line(r, "String", 0.02f, new Color(0.9f, 0.9f, 0.95f, 0.9f), 5);
        Transform bellHit = Child(r, "BellHit", new Vector2(1.6f, 0.95f)).transform;
        Transform stringPoint = Child(r, "StringPoint", new Vector2(1.6f, -0.35f)).transform;
        SmokeEmitter burn = Emitter(r, "Burn", new Vector2(1.6f, -0.35f), "puff", 20f, 0.6f, 0.5f, 0.2f, 0.35f, new Color(0.35f, 0.32f, 0.35f, 0.9f), 30);

        // i quattro slot tratteggiati e il percorso
        Vector2[] ringAt = { new Vector2(-1.95f, 0.74f), new Vector2(-0.85f, -0.6f), new Vector2(0.58f, -0.78f), new Vector2(1.25f, -0.65f) };
        float[] ringSize = { 1.2f, 1.5f, 0.8f, 0.7f };
        var rings = new SpriteRenderer[4];
        for (int i = 0; i < 4; i++)
        {
            Transform ring = HW_Obj(r, "Slot" + (i + 1), ringAt[i], "v63_slot_ring", 0, ringSize[i]);
            rings[i] = ring.Find("Art").GetComponent<SpriteRenderer>();
        }
        Transform[] slotPoints =
        {
            Child(r, "Seat1", new Vector2(-1.95f, 0.42f)).transform,
            Child(r, "Seat2", new Vector2(-0.85f, -0.6f)).transform,
            Child(r, "Seat3", new Vector2(0.6f, -1.15f)).transform,
            Child(r, "Seat4", new Vector2(1.2f, -1.15f)).transform,
        };
        Transform[] paths =
        {
            V63_Dashes(r, "Path1", new Vector2(-1.58f, 0.74f), new Vector2(-1.32f, 0.74f), 2, 1),
            V63_Dashes(r, "Path2", new Vector2(-1.3f, 0.25f), new Vector2(-0.4f, -0.65f), 5, 1),
            V63_Dashes(r, "Path3", new Vector2(0.3f, -0.25f), new Vector2(0.85f, -0.25f), 3, 1),
            V63_Dashes(r, "Path4", new Vector2(1.0f, 0.12f), new Vector2(1.42f, 0.12f), 2, 1),
            V63_Dashes(r, "Path5", new Vector2(1.6f, 1.15f), new Vector2(1.6f, 1.75f), 3, 1),
        };
        // il libro-rampa (centro dello slot 2, inclinato di 45°): i due capi della superficie
        Transform shelfEdge = Child(r, "ShelfEdge", new Vector2(-1.3f, 0.42f)).transform;
        Transform rampTop = Child(r, "RampTop", new Vector2(-1.19f, 0.02f)).transform;
        Transform rampBottom = Child(r, "RampBottom", new Vector2(-0.23f, -0.94f)).transform;
        Transform floorEnd = Child(r, "FloorEnd", new Vector2(2.5f, -1.15f)).transform;

        // il ripiano in basso con i cinque oggetti (perni: palla, libro e calamita al centro; domino e candela alla base)
        Bar(r, "Tray", new Vector2(0f, -3.62f), new Vector2(5.2f, 0.16f), new Color(0.55f, 0.37f, 0.24f), 2);
        const float top = -3.54f;
        Transform ball = HW_Obj(r, "Ball", new Vector2(-1.95f, top + 0.324f), "chrome_ball", 7, 0.6f);
        CircleCollider2D ballCol = ball.gameObject.AddComponent<CircleCollider2D>();
        ballCol.radius = 0.7f;
        Transform book = HW_Obj(r, "Book", new Vector2(0.62f, top + 0.27f), "pt_book", 6, 0.27f);
        BoxCollider2D bookCol = HW_Box(book, new Vector2(5.2f, 2.6f));
        Transform dom = Child(r, "Dominoes", new Vector2(-1.1f, top)).transform;
        var dominoes = new Transform[4];
        for (int i = 0; i < 4; i++)
        {
            dominoes[i] = Child(dom, "D" + i, new Vector2(-0.255f + i * 0.17f, 0f)).transform;
            dominoes[i].localScale = Vector3.one * 0.45f;
            Art(dominoes[i], "Art", "domino", 7, false);
        }
        BoxCollider2D domCol = HW_Box(dom, new Vector2(0.9f, 0.75f), new Vector2(0f, 0.3f));
        Transform candle = Child(r, "Candle", new Vector2(-0.3f, top)).transform;
        Transform candleBody = HW_Obj(candle, "Body", Vector2.zero, "candle", 7, 1.2f);
        Transform flame = Art(candleBody, "Flame", "flame", 8, false, new Vector2(0f, 0.58f));
        Transform cglow = Art(candleBody, "Glow", "glow", 6, false, new Vector2(0f, 0.68f));
        cglow.localScale = Vector3.one * 0.7f;
        cglow.GetComponent<SpriteRenderer>().color = new Color(1f, 0.8f, 0.35f, 0.6f);
        BoxCollider2D candleCol = HW_Box(candle, new Vector2(0.6f, 1.2f), new Vector2(0f, 0.5f));
        GameObject flameGroup = Child(candleBody, "FlameGroup", Vector2.zero);
        flame.SetParent(flameGroup.transform, true);
        cglow.SetParent(flameGroup.transform, true);
        Transform magnet = HW_Obj(r, "Magnet", new Vector2(1.8f, top + 0.3f), "magnet", 7, 0.9f);
        BoxCollider2D magnetCol = HW_Box(magnet, new Vector2(0.95f, 0.9f));

        // GO!
        Transform go = HW_Obj(r, "Go", new Vector2(-0.35f, -4.35f), "s41b_arrow_btn", 8, 0.8f);
        TextMesh goText = Label(r, new Vector2(0.55f, -4.35f), 9, "GO!", 0.07f);
        goText.color = new Color(1f, 0.86f, 0.3f);
        BoxCollider2D goCol = HW_Area(r, "GoArea", new Vector2(2.1f, 0.95f), new Vector2(0.1f, -4.35f));

        HW_Goal(r, "RING THE BELL ON THE OTHER SIDE OF THE ROOM!");
        SetArray(level, "items", new[] { ball, book, dom, candle, magnet });
        SetArray(level, "itemAreas", new Collider2D[] { ballCol, bookCol, domCol, candleCol, magnetCol });
        V63_SetFloats(level, "halfHeights", new[] { 0.324f, 0.27f, 0f, 0f, 0.3f });
        SetField(level, "ballArt", ball.Find("Art"));
        SetArray(level, "dominoes", dominoes);
        SetField(level, "candleFlame", flameGroup);
        SetArray(level, "slotPoints", slotPoints);
        SetArray(level, "slotRings", rings);
        SetArray(level, "linkPaths", paths);
        SetField(level, "shelfEdge", shelfEdge);
        SetField(level, "rampTop", rampTop);
        SetField(level, "rampBottom", rampBottom);
        SetField(level, "floorEnd", floorEnd);
        SetField(level, "stringPoint", stringPoint);
        SetField(level, "balloon", balloon);
        SetField(level, "balloonArea", balloonCol);
        SetField(level, "balloonPeg", peg);
        SetField(level, "balloonString", str);
        SetField(level, "bellHit", bellHit);
        SetField(level, "bell", bell);
        SetField(level, "bellArea", bellCol);
        SetField(level, "bellWaves", bellWaves);
        SetField(level, "burnPuff", burn);
        SetField(level, "goButton", go);
        SetField(level, "goArea", goCol);
        V62_Wire(level, r, new Vector2(0f, -2.2f));
        return SaveLevel(root, "V63ChainReaction");
    }

    // ---------- 257 The lying clocks ----------

    /// <summary>Lancette (ora e minuti) attorno a un perno: barre che il livello fa girare.</summary>
    static void V63_Hands(Transform clock, Vector2 center, float len, float width, int order, out Transform hour, out Transform minute)
    {
        hour = Child(clock, "Hour", center).transform;
        Bar(hour, "Hand", new Vector2(0f, len * 0.3f), new Vector2(width, len * 0.62f), InkColor, order);
        minute = Child(clock, "Minute", center).transform;
        Bar(minute, "Hand", new Vector2(0f, len * 0.45f), new Vector2(width * 0.55f, len * 0.92f), new Color(0.6f, 0.15f, 0.15f), order + 1);
        Bar(clock, "Pin", center, new Vector2(width * 1.2f, width * 1.2f), InkColor, order + 2);
    }

    static GameObject BuildV63LyingClocks()
    {
        var root = new GameObject("Level_V63LyingClocks");
        V63LyingClocksLevel level = root.AddComponent<V63LyingClocksLevel>();
        Art(root.transform, "Room", "s41j_clock_bg", -20, false);
        Transform r = root.transform;

        // i tre orologi sulla parete: cucù (indietro), sveglia (ferma), orologio da timbro (corre il doppio)
        Transform cuckoo = HW_Obj(r, "Cuckoo", new Vector2(-1.5f, 1.05f), "cuckoo_clock", 2, 0.5f);
        V63_Hands(cuckoo, Vector2.zero, 0.9f, 0.1f, 4, out Transform cH, out Transform cM);
        BoxCollider2D cuckooCol = HW_Box(cuckoo, new Vector2(3.0f, 3.8f), new Vector2(0f, 0.5f));
        Transform bird = Child(cuckoo, "Bird", new Vector2(0f, 1.5f)).transform;
        bird.localScale = Vector3.one * 0.55f;
        Art(bird, "Art", "bird_up", 3, false);

        Transform alarm = HW_Obj(r, "Alarm", new Vector2(0f, 0.55f), "alarm_clock", 2, 0.85f);
        Art(alarm, "Face", "v63_clock_face", 3, false, new Vector2(0f, 0.72f)).localScale = Vector3.one * 0.96f;
        V63_Hands(alarm, new Vector2(0f, 0.72f), 0.42f, 0.06f, 4, out Transform aH, out Transform aM);
        BoxCollider2D alarmCol = HW_Box(alarm, new Vector2(2.0f, 1.7f), new Vector2(0f, 0.8f));

        Transform fast = HW_Obj(r, "TimeClock", new Vector2(1.55f, 1.15f), "s41b_gr_clock", 2, 1.9f);
        Art(fast, "Face", "v63_clock_face", 3, false, new Vector2(0f, 0.18f)).localScale = Vector3.one * 0.4f;
        V63_Hands(fast, new Vector2(0f, 0.18f), 0.17f, 0.03f, 4, out Transform fH, out Transform fM);
        BoxCollider2D fastCol = HW_Box(fast, new Vector2(0.75f, 1.0f));

        // la porta a destra, con le tre spie degli errori
        HW_Obj(r, "DoorFrame", new Vector2(1.55f, -1.6f), "door_frame", 0, 0.7f);
        Transform panel = Child(r, "DoorPanel", new Vector2(1.55f - 1.1f * 0.7f, -1.6f)).transform;
        panel.localScale = Vector3.one * 0.7f;
        Art(panel, "Art", "door_panel", 1, false);
        BoxCollider2D doorCol = HW_Box(panel, new Vector2(2.2f, 3.6f), new Vector2(1.1f, 0f));
        var lights = new SpriteRenderer[3];
        for (int i = 0; i < 3; i++)
            lights[i] = Bar(r, "MistakeLight" + i, new Vector2(1.25f + i * 0.3f, -1.05f), new Vector2(0.18f, 0.18f), new Color(0.35f, 0.35f, 0.4f), 3);

        // il tastierino: display HH:MM, frecce su/giù per ore e minuti, OK
        Bar(r, "Keypad", new Vector2(-1.0f, -1.5f), new Vector2(2.5f, 2.7f), new Color(0.22f, 0.2f, 0.26f, 0.95f), 1);
        HW_Obj(r, "Display", new Vector2(-1.0f, -1.0f), "e40_display", 2, 0.95f);
        TextMesh display = Label(r, new Vector2(-1.0f, -1.0f), 4, "12:00", 0.075f);
        display.color = new Color(0.45f, 1f, 0.55f);
        Fit(display, 1.9f, 0.6f);
        var arrows = new Collider2D[4];
        Vector2[] arrowAt = { new Vector2(-1.6f, -0.45f), new Vector2(-1.6f, -1.55f), new Vector2(-0.4f, -0.45f), new Vector2(-0.4f, -1.55f) };
        for (int i = 0; i < 4; i++)
        {
            Transform a = HW_Obj(r, "Arrow" + i, arrowAt[i], "s41b_arrow_btn", 3, 0.55f);
            a.localRotation = Quaternion.Euler(0f, 0f, i % 2 == 0 ? 90f : -90f);
            CircleCollider2D c = a.gameObject.AddComponent<CircleCollider2D>();
            c.radius = 0.62f;
            arrows[i] = c;
        }
        Transform ok = HW_Obj(r, "Ok", new Vector2(-1.0f, -2.3f), "keypad_key", 3, 2.0f);
        BoxCollider2D okCol = HW_Box(ok, new Vector2(0.55f, 0.55f));
        TextMesh okText = Label(ok, Vector2.zero, 4, "OK", 0.04f);
        Fit(okText, 0.32f, 0.25f);

        // la radio per terra
        Transform radio = HW_Obj(r, "Radio", new Vector2(-1.45f, -3.95f), "radio", 3, 0.5f);
        BoxCollider2D radioCol = HW_Box(radio, new Vector2(3.0f, 2.4f), new Vector2(0f, 0.1f));

        HW_Goal(r, "THE DOOR OPENS ONLY AT THE RIGHT TIME. WHAT TIME IS IT?");
        SetField(level, "cuckooHour", cH);
        SetField(level, "cuckooMinute", cM);
        SetField(level, "stoppedHour", aH);
        SetField(level, "stoppedMinute", aM);
        SetField(level, "fastHour", fH);
        SetField(level, "fastMinute", fM);
        SetField(level, "cuckooBird", bird);
        SetField(level, "cuckooClock", cuckoo);
        SetField(level, "cuckooArea", cuckooCol);
        SetField(level, "stoppedClock", alarm);
        SetField(level, "stoppedArea", alarmCol);
        SetField(level, "fastClock", fast);
        SetField(level, "fastArea", fastCol);
        SetField(level, "radio", radio);
        SetField(level, "radioArea", radioCol);
        SetField(level, "display", display);
        SetField(level, "hourUp", arrows[0]);
        SetField(level, "hourDown", arrows[1]);
        SetField(level, "minuteUp", arrows[2]);
        SetField(level, "minuteDown", arrows[3]);
        SetField(level, "okArea", okCol);
        SetField(level, "okButton", ok);
        SetArray(level, "mistakeLights", lights);
        SetField(level, "doorPanel", panel);
        SetField(level, "doorArea", doorCol);
        SetField(level, "cuckooBubble", SmallBubble(r, new Vector2(-0.55f, 2.35f)));
        SetField(level, "doorBubble", SmallBubble(r, new Vector2(1.25f, 0.0f)));
        SetField(level, "radioBubble", SmallBubble(r, new Vector2(-0.35f, -3.55f)));
        V62_Wire(level, r, new Vector2(0f, -3.05f));
        return SaveLevel(root, "V63LyingClocks");
    }

    // ---------- 258 Shadow puppet ----------

    static GameObject BuildV63ShadowPuppet()
    {
        var root = new GameObject("Level_V63ShadowPuppet");
        V63ShadowPuppetLevel level = root.AddComponent<V63ShadowPuppetLevel>();
        Art(root.transform, "Hall", "hall_bg", -20, false);
        Transform r = root.transform;

        // il muro dove cade l'ombra: alone della lampada e le quattro ombre (nascoste finché la lampada è spenta)
        SpriteRenderer wallGlow = Art(r, "WallGlow", "glow", -9, false, new Vector2(-0.4f, 1.4f)).GetComponent<SpriteRenderer>();
        wallGlow.transform.localScale = Vector3.one * 3.4f;
        wallGlow.color = new Color(1f, 0.93f, 0.6f, 0f);
        Transform shadowRoot = Child(r, "Shadow", new Vector2(-0.6f, 1.25f)).transform;
        var shadowZones = new Transform[4];
        var shadowArts = new SpriteRenderer[4];
        for (int i = 0; i < 4; i++)
        {
            shadowZones[i] = Child(shadowRoot, "Zone" + i, Vector2.zero).transform;
            var go = new GameObject("Art");
            go.transform.SetParent(shadowZones[i], false);
            shadowArts[i] = go.AddComponent<SpriteRenderer>();
            shadowArts[i].sortingOrder = -6;
        }

        // la porta e il cane da guardia davanti
        HW_Obj(r, "DoorFrame", new Vector2(1.55f, -0.75f), "door_frame", 0, 0.6f);
        Transform panel = Child(r, "DoorPanel", new Vector2(1.55f - 1.1f * 0.6f, -0.75f)).transform;
        panel.localScale = Vector3.one * 0.6f;
        Art(panel, "Art", "door_panel", 1, false);
        BoxCollider2D doorCol = HW_Box(panel, new Vector2(2.2f, 3.6f), new Vector2(1.1f, 0f));
        Transform dog = HW_Obj(r, "GuardDog", new Vector2(1.55f, -2.75f), "guard_dog", 5, 0.95f);
        BoxCollider2D dogCol = HW_Box(dog, new Vector2(1.6f, 1.5f), new Vector2(0f, 0.55f));

        // la lampada (spenta) e il suo cono di luce verso il muro
        Transform lamp = HW_Obj(r, "Lamp", new Vector2(-1.95f, -2.95f), "bedside_lamp", 6, 0.75f);
        BoxCollider2D lampCol = HW_Box(lamp, new Vector2(1.5f, 2.0f), new Vector2(0.25f, 0.85f));
        SpriteRenderer cone = Art(r, "LightCone", "light_cone", 2, false).GetComponent<SpriteRenderer>();
        Transform coneT = cone.transform;
        // il cono parte dal paralume e punta all'alone sul muro (il disegno punta in basso, con la punta in 0,0)
        Transform conePivot = Child(r, "ConePivot", new Vector2(-1.95f, -2.0f)).transform;
        conePivot.localRotation = Quaternion.Euler(0f, 0f, 155.5f);
        conePivot.localScale = new Vector3(0.75f, 0.72f, 1f);
        coneT.SetParent(conePivot, false);
        coneT.localPosition = centers["light_cone"];
        cone.color = new Color(1f, 0.95f, 0.7f, 0f);

        // il supporto che scorre (origine alla base) con le quattro zone: testa, orecchie, coda
        float[] notch = { 0.0f, -0.5f, -1.05f };   // Far, Mid, Near (verso la lampada)
        Transform stand = Child(r, "Stand", new Vector2(notch[0], -2.95f)).transform;
        Color wood = new Color(0.43f, 0.29f, 0.2f);
        Bar(stand, "Base", Vector2.zero, new Vector2(0.9f, 0.1f), wood, 3);
        Bar(stand, "Pole", new Vector2(0f, 0.68f), new Vector2(0.07f, 1.3f), wood, 3);
        Bar(stand, "Arm", new Vector2(0.15f, 1.33f), new Vector2(1.0f, 0.06f), wood, 3);
        BoxCollider2D standCol = HW_Box(stand, new Vector2(1.1f, 1.1f), new Vector2(0f, 0.45f));
        Vector2[] zoneAt = { new Vector2(0f, 1.7f), new Vector2(-0.22f, 1.98f), new Vector2(0.22f, 1.98f), new Vector2(0.45f, 1.5f) };
        float[] zoneSize = { 0.65f, 0.45f, 0.45f, 0.6f };
        string[] zoneName = { "Head", "EarLeft", "EarRight", "Tail" };
        var zones = new Transform[4];
        var rings = new SpriteRenderer[4];
        for (int i = 0; i < 4; i++)
        {
            zones[i] = Child(stand, zoneName[i], zoneAt[i]).transform;
            Transform ring = HW_Obj(stand, zoneName[i] + "Ring", zoneAt[i], "v63_slot_ring", 4, zoneSize[i]);
            rings[i] = ring.Find("Art").GetComponent<SpriteRenderer>();
            rings[i].color = new Color(0.35f, 0.24f, 0.18f, 0.7f);
        }
        // le tacche sul pavimento: lontano, a metà, vicino alla lampada
        foreach (float x in notch) Bar(r, "Notch", new Vector2(x, -3.06f), new Vector2(0.05f, 0.14f), new Color(0.3f, 0.2f, 0.14f), 2);

        // il ripiano con gli oggetti (perno = centro del disegno)
        Bar(r, "Tray", new Vector2(0f, -4.32f), new Vector2(5.2f, 0.14f), new Color(0.55f, 0.37f, 0.24f), 2);
        const float top = -4.25f;
        Transform yarn = V63_Item(r, "Yarn", new Vector2(-2.1f, top + 0.3f), "e40_yarn", 7, 0.8f, new Vector2(0f, -0.03f));
        Transform pizzaA = V63_Item(r, "PizzaA", new Vector2(-1.5f, top + 0.27f), "pizza_slice", 7, 0.34f, new Vector2(0.55f, 0.55f));
        Transform pizzaB = V63_Item(r, "PizzaB", new Vector2(-0.95f, top + 0.27f), "pizza_slice", 7, 0.34f, new Vector2(0.55f, 0.55f));
        Transform feather = V63_Item(r, "Feather", new Vector2(-0.25f, top + 0.25f), "feather", 7, 0.45f, new Vector2(0.57f, 0.22f));
        Transform apple = V63_Item(r, "Apple", new Vector2(0.5f, top + 0.3f), "apple", 7, 0.3f, new Vector2(0f, 0.17f));
        Transform boot = V63_Item(r, "Boot", new Vector2(1.15f, top + 0.28f), "fish_boot", 7, 0.6f, new Vector2(0.04f, 0.02f));
        Transform paw = V63_Item(r, "Paw", new Vector2(1.8f, top + 0.15f), "paw", 7, 0.28f, new Vector2(0f, 0.95f));
        paw.localRotation = Quaternion.Euler(0f, 0f, 90f);
        Transform[] items = { yarn, pizzaA, pizzaB, feather, apple, boot, paw };
        Vector2[] grab = { new Vector2(1.0f, 1.0f), new Vector2(1.6f, 1.6f), new Vector2(1.6f, 1.6f), new Vector2(2.0f, 1.3f),
            new Vector2(2.2f, 2.3f), new Vector2(1.1f, 1.1f), new Vector2(1.9f, 2.9f) };
        var areas = new Collider2D[items.Length];
        var arts = new SpriteRenderer[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            areas[i] = HW_Box(items[i], grab[i]);
            arts[i] = items[i].Find("Art").GetComponent<SpriteRenderer>();
        }

        HW_Goal(r, "GET PAST THE GUARD DOG!");
        SetArray(level, "items", items);
        SetArray(level, "itemAreas", areas);
        SetArray(level, "itemArts", arts);
        SetField(level, "stand", stand);
        SetField(level, "standArea", standCol);
        V63_SetFloats(level, "notchX", notch);
        SetArray(level, "zones", zones);
        SetArray(level, "zoneRings", rings);
        SetField(level, "shadowRoot", shadowRoot);
        SetArray(level, "shadowZones", shadowZones);
        SetArray(level, "shadowArts", shadowArts);
        SetField(level, "lightCone", cone);
        SetField(level, "wallGlow", wallGlow);
        SetField(level, "lamp", lamp);
        SetField(level, "lampArea", lampCol);
        SetField(level, "dog", dog);
        SetField(level, "dogArea", dogCol);
        SetField(level, "doorPanel", panel);
        SetField(level, "doorArea", doorCol);
        SetField(level, "dogBubble", SmallBubble(r, new Vector2(0.75f, -1.0f)));
        V62_Wire(level, r, new Vector2(0f, -3.55f));
        return SaveLevel(root, "V63ShadowPuppet");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V63 = new Dictionary<string, string[]>
    {
        { "v63_chain_reaction", new[] { "Something round starts it, something flat gets it down, something in a row passes it on... and something hot finishes it.",
            "Ball in the circle on the shelf, book as the ramp, dominoes on the table, candle right before the balloon string. The magnet is a trap. Then GO!" } },
        { "v63_lying_clocks", new[] { "The cuckoo clock is slow, but its bird sings exactly on the REAL hour. Watch where its hands are when it sings.",
            "When the bird sings it's really HH:00 (count the cuckoos). If the cuckoo clock shows, say, 3:45, it's 15 minutes slow: add 15 to what it shows and type that time, then OK." } },
        { "v63_shadow_puppet", new[] { "Build a cat on the stand: a round head, two pointy ears, a tail. Then switch on the lamp. Size matters!",
            "Yarn in the head circle, one pizza slice in each ear circle, the feather as the tail. Slide the stand all the way to the lamp, then tap the lamp." } },
    };
}
#endif
