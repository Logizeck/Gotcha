#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.67, livello 261 (gruppo A): la stanza buia. Riusa il soggiorno (vase_room_bg), la finestra, le tende,
/// il comodino e la sua lampada, la radio, l'orologio, la candela, i fiammiferi, il topo, l'interruttore, il tavolino,
/// il sole, l'uccellino, il fascio di sole, il buio col foro morbido e gli aiuti di Halloween/0.62 (HW_Goal, HW_Obj,
/// HW_Box, V62_Wire); i pochi disegni nuovi (prefisso v67_) sono in DoodleSpritesV67A. Niente Easter egg e niente Toni.
/// L'elenco dei livelli (Define) lo aggiunge il capo progetto in GotchaProjectBuilder.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 261 The Dark Room ----------

    static GameObject BuildV67DarkRoom()
    {
        var root = new GameObject("Level_V67DarkRoom");
        V67DarkRoomLevel level = root.AddComponent<V67DarkRoomLevel>();
        Transform r = root.transform;
        Art(r, "Room", "vase_room_bg", -20, false);

        // ---- la finestra (di giorno!) con le tende lunghe; la cucitura tra le due metà è in x = SeamX ----
        const float SeamX = 1.15f, RodY = 2.78f;
        Transform window = HW_Obj(r, "Window", new Vector2(SeamX, 1.35f), "window_frame", 0, 1f);
        Transform sun = HW_Obj(r, "Sun", new Vector2(SeamX + 0.45f, 1.95f), "sun", 1, 0.3f);
        Transform bird = HW_Obj(r, "Bird", new Vector2(SeamX - 0.5f, 0.38f), "s41h_bird", 9, 0.32f);
        Bar(r, "Rod", new Vector2(SeamX, RodY), new Vector2(2.75f, 0.09f), new Color(0.42f, 0.27f, 0.16f), 6);
        var panels = new Transform[2];
        var panelArts = new Transform[2];
        var panelAreas = new Collider2D[2];
        for (int i = 0; i < 2; i++)
        {
            float x = SeamX + (i == 0 ? -0.625f : 0.625f);
            Transform panel = Child(r, i == 0 ? "CurtainLeft" : "CurtainRight", new Vector2(x, RodY)).transform;
            Transform scaler = Child(panel, "Scaler", Vector2.zero).transform;
            scaler.localScale = new Vector3(0.86f, 1.8f, 1f);           // 1.38 x 5.2: dal bastone fino al pavimento (le due metà si sovrappongono un po')
            Art(scaler, "Art", "morning_curtain", 4 + i, false);
            panelAreas[i] = HW_Box(panel, new Vector2(1.45f, 5.2f), new Vector2(0f, -2.55f));
            panels[i] = panel;
            panelArts[i] = scaler;
        }
        // il cordino con la nappina (pende dal bastone, davanti alla tenda destra)
        Vector2 cordTop = new Vector2(SeamX + 0.95f, RodY - 0.05f), cordEnd = new Vector2(SeamX + 0.95f, 0.15f);
        LineRenderer cordLine = Line(r, "Cord", 0.035f, new Color(0.85f, 0.65f, 0.25f), 7);
        cordLine.SetPositions(new[] { (Vector3)cordTop, (Vector3)cordEnd });
        Transform tassel = HW_Obj(r, "Tassel", cordEnd, "v67_tassel", 8, 1.3f);
        BoxCollider2D cordCol = HW_Box(tassel, new Vector2(0.7f, 1.1f), new Vector2(0f, 0.1f));

        // ---- la lampada del soffitto (senza lampadina): oscilla attorno al punto in alto ----
        Transform lampPivot = Child(r, "CeilingLamp", new Vector2(-0.35f, 3.6f)).transform;
        SpriteRenderer lampArt = Art(lampPivot, "Art", "v67_lamp_empty", 3, false, new Vector2(0f, -1.5f)).GetComponent<SpriteRenderer>();
        BoxCollider2D lampCol = HW_Area(lampPivot, "Area", new Vector2(1.4f, 0.9f), new Vector2(0f, -1.4f));

        // ---- l'orologio a muro: le 2, col sole e PM ----
        Transform clock = HW_Obj(r, "Clock", new Vector2(-1.55f, 1.85f), "v63_clock_face", 3, 0.95f);
        Transform hour = Child(clock, "Hour", Vector2.zero).transform;
        Bar(hour, "Bar", new Vector2(0f, 0.12f), new Vector2(0.07f, 0.26f), InkColor, 5);
        hour.localRotation = Quaternion.Euler(0f, 0f, -60f);
        Transform minute = Child(clock, "Minute", Vector2.zero).transform;
        Bar(minute, "Bar", new Vector2(0f, 0.18f), new Vector2(0.045f, 0.38f), InkColor, 5);
        HW_Obj(clock, "SunIcon", new Vector2(-0.14f, -0.24f), "sun", 4, 0.07f);
        Fit(Label(clock, new Vector2(0.12f, -0.24f), 4, "PM", 0.022f), 0.2f, 0.11f);
        CircleCollider2D clockCol = clock.gameObject.AddComponent<CircleCollider2D>();
        clockCol.radius = 0.75f;

        // ---- l'interruttore sul muro ----
        Transform sw = HW_Obj(r, "Switch", new Vector2(-0.8f, 0.6f), "switch_off", 3, 0.9f);
        SpriteRenderer swArt = sw.GetComponentInChildren<SpriteRenderer>();
        BoxCollider2D swCol = HW_Box(sw, new Vector2(1.2f, 1.4f));

        // ---- il comodino con la lampada staccata, la presa, il filo rosicchiato, la spina e il topo ----
        HW_Obj(r, "Nightstand", new Vector2(-1.55f, -1.1f), "hw_nightstand", 3, 1f);
        Transform tableLamp = HW_Obj(r, "TableLamp", new Vector2(-1.55f, -1.07f), "bedside_lamp", 4, 0.6f);
        BoxCollider2D tableLampCol = HW_Box(tableLamp, new Vector2(2.0f, 2.3f), new Vector2(0f, 0.85f));
        Transform socket = HW_Obj(r, "Socket", new Vector2(-0.64f, -1.95f), "v67_socket", 2, 1.1f);
        LineRenderer lampCable = Line(r, "LampCable", 0.045f, new Color(0.23f, 0.2f, 0.25f), 2);
        lampCable.positionCount = 4;
        lampCable.SetPositions(new[] { new Vector3(-1.32f, -1.02f, 0f), new Vector3(-1.02f, -1.25f, 0f), new Vector3(-1.25f, -2.6f, 0f), new Vector3(-1.17f, -2.77f, 0f) });
        Transform chew = HW_Obj(r, "Chew", new Vector2(-1.0f, -2.77f), "v67_frayed", 6, 1f);
        LineRenderer plugCable = Line(r, "PlugCable", 0.045f, new Color(0.23f, 0.2f, 0.25f), 5);
        plugCable.positionCount = 3;
        Transform plug = HW_Obj(r, "Plug", new Vector2(-0.5f, -2.88f), "v67_plug", 7, 1.2f);
        plug.localRotation = Quaternion.Euler(0f, 0f, 12f);
        BoxCollider2D plugCol = HW_Box(plug, new Vector2(0.9f, 0.75f));
        plugCable.SetPositions(new[] { new Vector3(-0.83f, -2.77f, 0f), new Vector3(-0.75f, -2.9f, 0f), new Vector3(-0.83f, -2.95f, 0f) });
        SmokeEmitter sparks = Emitter(r, "Sparks", new Vector2(-1.0f, -2.77f), "s41k_spark", 26f, 0.35f, 0.9f, 0.25f, 0.35f, new Color(1f, 0.9f, 0.4f), 57);
        sparks.growth = 0.2f;
        sparks.sideDrift = 0.6f;
        Transform mouse = HW_Obj(r, "Mouse", new Vector2(-1.65f, -2.62f), "mouse", 7, 0.42f);
        CircleCollider2D mouseCol = mouse.gameObject.AddComponent<CircleCollider2D>();
        mouseCol.radius = 1.3f;

        // ---- il tavolino con la candela e i fiammiferi ----
        HW_Obj(r, "CoffeeTable", new Vector2(-0.55f, -3.25f), "hw_table", 2, 0.42f);
        Transform candle = HW_Obj(r, "Candle", new Vector2(-1.3f, -3.2f), "hw_candle", 3, 1.05f);
        BoxCollider2D candleCol = HW_Box(candle, new Vector2(0.8f, 1.1f), new Vector2(0f, 0.35f));
        Transform matchbox = HW_Obj(r, "Matchbox", new Vector2(-0.3f, -3.07f), "haunt_matchbox", 4, 0.32f);
        BoxCollider2D matchCol = HW_Box(matchbox, new Vector2(2.9f, 2.2f));
        SmokeEmitter fizzle = Emitter(r, "Fizzle", new Vector2(-1.3f, -2.5f), "puff", 14f, 0.8f, 0.6f, 0.08f, 0.22f, new Color(0.8f, 0.8f, 0.85f, 0.85f), 57);
        fizzle.growth = 1.2f;

        // ---- la radio per terra ----
        Transform radio = HW_Obj(r, "Radio", new Vector2(1.55f, -3.35f), "radio", 3, 0.42f);
        BoxCollider2D radioCol = HW_Box(radio, new Vector2(3.6f, 3.0f), new Vector2(0f, 0.1f));
        SmokeEmitter radioWaves = Emitter(r, "RadioWaves", new Vector2(1.55f, -2.75f), "ring_waves", 3f, 0.7f, 0.5f, 0.05f, 0.3f, new Color(1f, 1f, 1f, 0.8f), 57);
        radioWaves.growth = 0.8f;
        radioWaves.spinRange = 0f;
        radioWaves.sideDrift = 0f;

        // ---- il sole che entra (alla fine) ----
        Transform beamT = Child(r, "Sunbeam", new Vector2(SeamX + 0.4f, 1.3f)).transform;
        beamT.localScale = new Vector3(-1.5f, 1.4f, 1f);                // specchiato: dalla finestra verso il pavimento a sinistra
        SpriteRenderer beam = Art(beamT, "Art", "s41f_sunbeam", 40, false).GetComponent<SpriteRenderer>();

        // ---- il buio: velo scuro fuori dalla maschera del dito, alone morbido dentro ----
        SpriteRenderer dark = Bar(r, "Darkness", Vector2.zero, new Vector2(12f, 16f), new Color(0.02f, 0.02f, 0.06f, 0.93f), 50);
        dark.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
        Transform torch = Child(r, "Torch", Vector2.zero).transform;
        GameObject maskGo = Child(torch, "Mask", Vector2.zero);
        maskGo.transform.localScale = Vector3.one * 3.5f;              // "glow" con soglia 0.05: raggio 0.5 x 3.5 = 1.75
        SpriteMask mask = maskGo.AddComponent<SpriteMask>();
        mask.sprite = sprites["glow"];
        mask.alphaCutoff = 0.05f;
        SpriteRenderer ring = Art(torch, "Ring", "darkness", 51, false).GetComponent<SpriteRenderer>();
        ring.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        ring.color = new Color(1f, 1f, 1f, 0.93f);

        // ---- l'indizio: la riga di luce sul bordo delle tende (sopra il buio, si vede anche senza dito) ----
        SpriteRenderer edgeGlow = Art(r, "EdgeGlow", "glow", 54, false, new Vector2(SeamX, 1.4f)).GetComponent<SpriteRenderer>();
        edgeGlow.transform.localScale = new Vector3(0.9f, 2.4f, 1f);
        edgeGlow.color = new Color(1f, 0.93f, 0.65f, 0.45f);
        SpriteRenderer edgeLine = Bar(r, "EdgeLine", new Vector2(SeamX, 1.4f), new Vector2(0.035f, 2.6f), new Color(1f, 0.96f, 0.78f, 0.95f), 55);
        SpriteRenderer floorSpill = Bar(r, "FloorSpill", new Vector2(SeamX, -2.42f), new Vector2(0.9f, 0.05f), new Color(1f, 0.93f, 0.65f, 0.5f), 54);

        // il notiziario della radio (fumetto sopra il buio)
        SpeechBubble radioBubble = SmallBubble(r, new Vector2(0.75f, -2.0f));

        HW_Goal(r, "TURN ON THE LIGHT.");
        SetField(level, "darkness", dark);
        SetField(level, "torch", torch);
        SetField(level, "torchRing", ring);
        SetField(level, "edgeLine", edgeLine);
        SetField(level, "edgeGlow", edgeGlow);
        SetField(level, "floorSpill", floorSpill);
        SetField(level, "curtainLeft", panels[0]);
        SetField(level, "curtainRight", panels[1]);
        SetField(level, "curtainLeftArt", panelArts[0]);
        SetField(level, "curtainRightArt", panelArts[1]);
        SetField(level, "curtainLeftArea", panelAreas[0]);
        SetField(level, "curtainRightArea", panelAreas[1]);
        SetField(level, "cordTassel", tassel);
        SetField(level, "cordArea", cordCol);
        SetField(level, "cordLine", cordLine);
        SetField(level, "switchArea", swCol);
        SetField(level, "switchArt", swArt);
        SetField(level, "switchOffSprite", sprites["switch_off"]);
        SetField(level, "switchOnSprite", sprites["switch_on"]);
        SetField(level, "ceilingLampPivot", lampPivot);
        SetField(level, "ceilingLampArt", lampArt);
        SetField(level, "ceilingLampArea", lampCol);
        SetField(level, "tableLamp", tableLamp);
        SetField(level, "tableLampArea", tableLampCol);
        SetField(level, "plug", plug);
        SetField(level, "plugArea", plugCol);
        SetField(level, "socket", socket);
        SetField(level, "chew", chew);
        SetField(level, "plugCable", plugCable);
        SetField(level, "sparks", sparks);
        SetField(level, "mouse", mouse);
        SetField(level, "mouseArea", mouseCol);
        SetField(level, "matchbox", matchbox);
        SetField(level, "matchboxArea", matchCol);
        SetField(level, "candle", candle);
        SetField(level, "candleArea", candleCol);
        SetField(level, "fizzle", fizzle);
        SetField(level, "radio", radio);
        SetField(level, "radioArea", radioCol);
        SetField(level, "radioWaves", radioWaves);
        SetField(level, "radioBubble", radioBubble);
        SetField(level, "clock", clock);
        SetField(level, "clockArea", clockCol);
        SetField(level, "sunbeam", beam);
        SetField(level, "sun", sun);
        SetField(level, "bird", bird);
        V62_Wire(level, r, new Vector2(0f, -0.55f));
        return SaveLevel(root, "V67DarkRoom");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V67A = new Dictionary<string, string[]>
    {
        { "v67_dark_room", new[] { "What time is it, actually?",
            "Light doesn't always come from a bulb. Check the window." } },
    };
}
#endif
