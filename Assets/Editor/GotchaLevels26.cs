#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli della versione 0.26: meccaniche nuove al posto delle ripetizioni.</summary>
public static partial class GotchaProjectBuilder
{
    static GameObject Child(Transform parent, string name, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        return go;
    }

    static SpeechBubble SmallBubble(Transform root, Vector2 pos)
    {
        SpeechBubble b = Bubble(root, pos);
        b.transform.localScale = Vector3.one * 0.5f;
        return b;
    }

    static GameObject SaveLevel(GameObject root, string folder) => SavePrefab(root, LevelDir + "/" + folder + "/Level_" + folder + ".prefab");

    /// <summary>Barra orizzontale che si accorcia verso sinistra (scala x del Transform restituito).</summary>
    static Transform GaugeBar(Transform parent, string name, Vector2 pos, Vector2 size, Color back, Color fill, int order)
    {
        Bar(parent, name + "Back", pos, size + new Vector2(0.08f, 0.08f), back, order);
        Transform pivot = Child(parent, name, pos - new Vector2(size.x * 0.5f, 0f)).transform;
        Bar(pivot, "Fill", new Vector2(size.x * 0.5f, 0f), size, fill, order + 1);
        return pivot;
    }

    // ---------- 60: la torcia ----------

    static GameObject BuildFlashlightLevel()
    {
        var root = new GameObject("Level_Flashlight");
        FlashlightLevel level = root.AddComponent<FlashlightLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);
        var spots = new List<Transform>();
        foreach (Vector2 s in new[] { new Vector2(-1.9f, 2.6f), new Vector2(0.1f, 3.0f), new Vector2(2.0f, 2.3f), new Vector2(-2.1f, 0.4f),
                     new Vector2(1.9f, 0.1f), new Vector2(-0.6f, 1.2f), new Vector2(0.9f, -1.6f), new Vector2(-1.6f, -2.0f), new Vector2(0.4f, -3.0f) })
            spots.Add(Child(root.transform, "Spot", s).transform);
        var bats = new List<Transform>();
        for (int i = 0; i < 4; i++)
        {
            Transform bat = Child(root.transform, "Bat" + i, Vector2.zero).transform;
            Art(bat, "Sleep", "bat_sleep", 6);
            Art(bat, "Awake", "bat_awake", 6).gameObject.SetActive(false);
            bats.Add(bat);
        }
        GameObject sw = Child(root.transform, "Switch", Vector2.zero);
        SpriteRenderer swArt = Art(sw.transform, "Art", "switch_off", 5, false).GetComponent<SpriteRenderer>();
        sw.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, 1.1f);
        Transform torch = Art(root.transform, "Torch", "darkness", 50, false);
        SpriteRenderer blackout = Bar(root.transform, "Blackout", Vector2.zero, new Vector2(12f, 16f), Color.black, 51);
        SpeechBubble bubble = Bubble(root.transform, new Vector2(0, 3.6f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetField(level, "torch", torch);
        SetField(level, "blackout", blackout);
        SetArray(level, "bats", bats.ToArray());
        SetArray(level, "spots", spots.ToArray());
        SetField(level, "lightSwitch", sw.GetComponent<BoxCollider2D>());
        SetField(level, "switchArt", swArt);
        SetField(level, "switchOn", sprites["switch_on"]);
        SetField(level, "bubble", bubble);
        return SaveLevel(root, "Flashlight");
    }

    // ---------- 67: lo specchio appannato ----------

    static GameObject BuildMirrorLevel()
    {
        var root = new GameObject("Level_Mirror");
        MirrorLevel level = root.AddComponent<MirrorLevel>();
        Art(root.transform, "Bathroom", "bathroom_bg", -20, false);
        Transform mirror = Child(root.transform, "Mirror", new Vector2(-0.75f, 1.5f)).transform;
        mirror.localScale = Vector3.one * 0.9f;
        Art(mirror, "Art", "mirror", 2, false);
        SpriteRenderer fog = Art(mirror, "Fog", "mirror_fog", 3, false).GetComponent<SpriteRenderer>();
        TextMesh code = Label(mirror, new Vector2(0.05f, -0.05f), 4, "0000", 0.17f);
        code.transform.localRotation = Quaternion.Euler(0, 0, 8f);

        GameObject cabinet = Child(root.transform, "Cabinet", new Vector2(1.75f, 0.9f));
        cabinet.transform.localScale = Vector3.one * 0.85f;
        GameObject closed = Art(cabinet.transform, "Closed", "cabinet", 2, false).gameObject;
        GameObject open = Art(cabinet.transform, "Open", "cabinet_open", 2, false).gameObject;
        BoxCollider2D area = cabinet.AddComponent<BoxCollider2D>();
        area.size = new Vector2(1.6f, 1.5f);
        area.offset = new Vector2(0, 0.7f);
        SmokeEmitter steam = Emitter(root.transform, "Breath", new Vector2(-0.75f, -1.2f), "puff", 18f, 1.4f, 1.6f, 0.6f, 0.9f, new Color(1f, 1f, 1f, 0.45f), 6);
        GameObject keypad = KeypadOverlay(root.transform, out BoxCollider2D panelCol, out TextMesh display, out TapButton[] keys);
        SpeechBubble bubble = Bubble(root.transform, new Vector2(0.4f, -2.6f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetField(level, "fog", fog);
        SetField(level, "code", code);
        SetField(level, "cabinetArea", area);
        SetField(level, "cabinetClosed", closed);
        SetField(level, "cabinetOpen", open);
        SetField(level, "keypad", keypad);
        SetField(level, "keypadPanel", panelCol);
        SetArray(level, "keys", keys);
        SetField(level, "display", display);
        SetField(level, "steam", steam);
        SetField(level, "bubble", bubble);
        return SaveLevel(root, "Mirror");
    }

    // ---------- 81: il funambolo ----------

    static GameObject BuildTightropeLevel()
    {
        var root = new GameObject("Level_Tightrope");
        TightropeLevel level = root.AddComponent<TightropeLevel>();
        Art(root.transform, "Canyon", "canyon_bg", -20, false);
        Color wood = new Color(0.55f, 0.36f, 0.22f);
        Bar(root.transform, "PoleL", new Vector2(-2.45f, 0.1f), new Vector2(0.14f, 1.9f), wood, 1);
        Bar(root.transform, "PoleR", new Vector2(2.45f, 0.4f), new Vector2(0.14f, 1.3f), wood, 1);
        var rope = new List<Vector3>();
        for (int i = 0; i <= 12; i++) { float t = i / 12f; rope.Add(new Vector3(Mathf.Lerp(-2.45f, 2.45f, t), 1.0f - Mathf.Sin(t * Mathf.PI) * 0.06f, 0f)); }
        StaticLine(root.transform, "Rope", rope.ToArray(), 0.05f, InkColor, 2);
        Transform start = Child(root.transform, "Start", new Vector2(-2.2f, 1.0f)).transform;
        Transform end = Child(root.transform, "End", new Vector2(2.2f, 1.0f)).transform;
        Transform acrobat = Child(root.transform, "Acrobat", new Vector2(-2.2f, 1.0f)).transform;
        Transform art = Art(acrobat, "Art", "acrobat", 5);
        art.localScale = Vector3.one * 0.75f;
        art.localPosition *= 0.75f;
        SmokeEmitter splash = Emitter(root.transform, "Splash", new Vector2(0, -3.4f), "puff", 40f, 0.7f, 2.2f, 0.7f, 0.8f, new Color(0.6f, 0.85f, 1f), 7);
        SpeechBubble bubble = Bubble(root.transform, new Vector2(0, 3.2f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetField(level, "acrobat", acrobat);
        SetField(level, "startPoint", start);
        SetField(level, "endPoint", end);
        SetField(level, "splash", splash);
        SetField(level, "bubble", bubble);
        return SaveLevel(root, "Tightrope");
    }

    // ---------- 86: l'aeroplanino di carta ----------

    static GameObject BuildPaperPlaneLevel()
    {
        var root = new GameObject("Level_PaperPlane");
        PaperPlaneLevel level = root.AddComponent<PaperPlaneLevel>();
        Art(root.transform, "Sky", "sky_bg", -20, false);
        var rings = new List<Transform>();
        foreach (Vector2 r in new[] { new Vector2(0.3f, 2.2f), new Vector2(1.9f, 0.5f), new Vector2(-1.2f, -0.9f), new Vector2(1.0f, -2.4f) })
        {
            Transform ring = Art(root.transform, "Hoop", "hoop", 4, true, r);
            ring.localScale = Vector3.one * 1.2f;
            rings.Add(ring);
        }
        Transform plane = Child(root.transform, "Plane", new Vector2(-2.4f, 1.2f)).transform;
        Art(plane, "Art", "paper_plane", 10);
        Transform breath = GaugeBar(root.transform, "Breath", new Vector2(0, 3.9f), new Vector2(2.2f, 0.18f), new Color(0.17f, 0.13f, 0.2f, 0.6f), new Color(0.55f, 0.85f, 1f), 20);
        Label(root.transform, new Vector2(-1.55f, 3.9f), 21, "AIR", 0.035f);
        SpeechBubble bubble = Bubble(root.transform, new Vector2(0, 3.1f));
        bubble.transform.localScale = Vector3.one * 0.5f;

        SetField(level, "plane", plane);
        SetArray(level, "rings", rings.ToArray());
        SetField(level, "breathBar", breath);
        SetField(level, "doneSprite", sprites["hoop_done"]);
        SetField(level, "bubble", bubble);
        return SaveLevel(root, "PaperPlane");
    }

    // ---------- 87: lo scassinatore ----------

    static GameObject BuildSafeCrackLevel()
    {
        var root = new GameObject("Level_SafeCrack");
        SafeCrackLevel level = root.AddComponent<SafeCrackLevel>();
        Bar(root.transform, "Wall", Vector2.zero, new Vector2(12f, 16f), new Color(0.2f, 0.17f, 0.26f), -20);
        Transform safe = Child(root.transform, "Safe", new Vector2(0, 0.2f)).transform;
        safe.localScale = Vector3.one * 1.55f;
        GameObject closed = Art(safe, "Closed", "safe", 1, false).gameObject;
        GameObject open = Art(safe, "Open", "safe_open", 1, false).gameObject;
        Transform dial = Child(root.transform, "Dial", new Vector2(0, 0.1f)).transform;
        dial.localScale = Vector3.one * 1.1f;
        Art(dial, "Art", "safe_dial", 4, false);
        for (int n = 0; n < 40; n += 5)
        {
            float a = n * 9f * Mathf.Deg2Rad;
            TextMesh t = Label(dial, new Vector2(-Mathf.Sin(a) * 0.48f, Mathf.Cos(a) * 0.48f), 5, n.ToString(), 0.032f);
            t.color = Color.white;
            t.transform.localRotation = Quaternion.Euler(0, 0, n * 9f);
        }
        CircleCollider2D area = dial.gameObject.AddComponent<CircleCollider2D>();
        area.radius = 1.0f;
        Bar(root.transform, "Pointer", new Vector2(0, 1.32f), new Vector2(0.08f, 0.25f), new Color(1f, 0.3f, 0.3f), 6);
        var leds = new List<SpriteRenderer>();
        for (int i = 0; i < 3; i++) leds.Add(Bar(root.transform, "Led" + i, new Vector2(-0.6f + i * 0.6f, 2.35f), new Vector2(0.35f, 0.18f), Color.gray, 6));
        TextMesh readout = Label(root.transform, new Vector2(0, -1.75f), 6, "0", 0.09f);
        readout.color = new Color(0.36f, 1f, 0.6f);
        TextMesh hint = Label(root.transform, new Vector2(0, -2.6f), 6, "", 0.06f);
        hint.color = Color.white;
        Transform guard = GaugeBar(root.transform, "Guard", new Vector2(0.3f, 3.9f), new Vector2(2.2f, 0.18f), new Color(0f, 0f, 0f, 0.6f), new Color(1f, 0.85f, 0.3f), 20);
        Label(root.transform, new Vector2(-1.35f, 3.9f), 21, "GUARD", 0.035f).color = Color.white;

        SetField(level, "dial", dial);
        SetField(level, "dialArea", area);
        SetArray(level, "leds", leds.ToArray());
        SetField(level, "readout", readout);
        SetField(level, "hint", hint);
        SetField(level, "safeClosed", closed);
        SetField(level, "safeOpen", open);
        SetField(level, "guardBar", guard);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0, 3.15f)));
        return SaveLevel(root, "SafeCrack");
    }

    // ---------- 90: il domino ----------

    static GameObject BuildDominoLevel()
    {
        var root = new GameObject("Level_Domino");
        DominoLevel level = root.AddComponent<DominoLevel>();
        Bar(root.transform, "Wall", Vector2.zero, new Vector2(12f, 16f), new Color(0.98f, 0.93f, 0.82f), -20);
        Bar(root.transform, "Table", new Vector2(0, -2.6f), new Vector2(7f, 2.2f), new Color(0.72f, 0.5f, 0.32f), -19);
        Bar(root.transform, "TableEdge", new Vector2(0, -1.52f), new Vector2(7f, 0.06f), InkColor, -18);
        // mensola bassa sopra il primo buco
        Bar(root.transform, "Shelf", new Vector2(-1.0f, -0.34f), new Vector2(0.9f, 0.12f), new Color(0.55f, 0.36f, 0.22f), 6);
        Bar(root.transform, "ShelfLeg", new Vector2(-1.0f, 0.4f), new Vector2(0.08f, 1.4f), new Color(0.45f, 0.3f, 0.18f), 5);
        var row = new List<Transform>();
        foreach (float x in new[] { -2.5f, -2.0f, -1.5f, -0.5f, 0.0f, 2.0f })
        {
            Transform t = Child(root.transform, "Tile", new Vector2(x, -1.5f)).transform;
            Art(t, "Art", "domino", 5, false);
            row.Add(t);
        }
        BoxCollider2D first = row[0].gameObject.AddComponent<BoxCollider2D>();
        first.size = new Vector2(0.6f, 1.2f);
        first.offset = new Vector2(0, 0.5f);
        var gaps = new List<Transform>();
        foreach (float x in new[] { -1.0f, 0.6f })
        {
            Transform g = Child(root.transform, "Gap", new Vector2(x, -1.5f)).transform;
            Bar(g, "Ghost", new Vector2(0, 0.5f), new Vector2(0.22f, 1f), new Color(0.17f, 0.13f, 0.2f, 0.12f), 4);
            gaps.Add(g);
        }
        Transform bell = Child(root.transform, "Bell", new Vector2(2.6f, -1.5f)).transform;
        Art(bell, "Art", "bell", 5, false);
        Bar(root.transform, "Tray", new Vector2(0, -3.55f), new Vector2(3.8f, 0.12f), new Color(0.45f, 0.3f, 0.18f), 5);
        var pieces = new List<Draggable2D>();
        float[] hs = { 0.45f, 1.0f, 1.5f };
        for (int i = 0; i < 3; i++)
        {
            GameObject p = Child(root.transform, "Piece" + i, new Vector2(-1.2f + i * 1.2f, -3.5f));
            p.transform.localScale = new Vector3(1f, hs[i], 1f);
            Art(p.transform, "Art", "domino", 7, false);
            BoxCollider2D c = p.AddComponent<BoxCollider2D>();
            c.size = new Vector2(0.6f, 1f);
            c.offset = new Vector2(0, 0.5f);
            Draggable2D d = p.AddComponent<Draggable2D>();
            d.itemId = hs[i].ToString(System.Globalization.CultureInfo.InvariantCulture);
            pieces.Add(d);
        }

        SetArray(level, "row", row.ToArray());
        SetArray(level, "gaps", gaps.ToArray());
        Tune(level, "gapClearance", new[] { 1.1f, 0f });
        SetArray(level, "pieces", pieces.ToArray());
        SetField(level, "firstTile", first);
        SetField(level, "bell", bell);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0, 2.4f)));
        return SaveLevel(root, "Domino");
    }

    // ---------- 91: il sentiero allo specchio ----------

    static GameObject BuildMirrorMazeLevel()
    {
        var root = new GameObject("Level_MirrorMaze");
        MirrorMazeLevel level = root.AddComponent<MirrorMazeLevel>();
        Art(root.transform, "Garden", "garden_bg", -20, false);
        Vector2[] pts = { new Vector2(-2.2f, -3.3f), new Vector2(-2.2f, -1.6f), new Vector2(0.2f, -1.6f), new Vector2(0.2f, -0.1f),
            new Vector2(2.2f, -0.1f), new Vector2(2.2f, 1.4f), new Vector2(-0.6f, 1.4f), new Vector2(-0.6f, 2.9f), new Vector2(1.9f, 2.9f) };
        var path = new List<Transform>();
        var v3 = new Vector3[pts.Length];
        for (int i = 0; i < pts.Length; i++) { path.Add(Child(root.transform, "P" + i, pts[i]).transform); v3[i] = pts[i]; }
        StaticLine(root.transform, "Fence", v3, 1.04f, new Color(1f, 0.85f, 0.2f), 1);
        StaticLine(root.transform, "Edge", v3, 0.96f, InkColor, 2);
        StaticLine(root.transform, "Path", v3, 0.9f, new Color(0.93f, 0.84f, 0.66f), 3);
        Transform mouse = Child(root.transform, "Mouse", pts[0]).transform;
        mouse.localScale = Vector3.one * 0.35f;
        Art(mouse, "Art", "mouse", 10);
        Art(root.transform, "Cheese", "cheese_wedge", 8, true, pts[pts.Length - 1]).localScale = Vector3.one * 0.6f;
        TextMesh sign = Label(root.transform, new Vector2(-2.0f, 3.5f), 12, "<  >", 0.07f);

        SetField(level, "mouse", mouse);
        SetArray(level, "path", path.ToArray());
        SetField(level, "sign", sign);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.6f, 3.6f)));
        return SaveLevel(root, "MirrorMaze");
    }

    // ---------- 92: pesca con la calamita ----------

    static GameObject BuildMagnetLevel()
    {
        var root = new GameObject("Level_Magnet");
        MagnetLevel level = root.AddComponent<MagnetLevel>();
        Art(root.transform, "Pond", "pond_bg", -20, false);
        Transform anchor = Child(root.transform, "RodTip", new Vector2(-0.4f, 3.0f)).transform;
        Art(anchor, "Rod", "fishing_rod", 3);
        Transform magnet = Child(root.transform, "Magnet", new Vector2(-0.4f, 1.8f)).transform;
        Art(magnet, "Art", "magnet", 8);
        LineRenderer wire = Line(root.transform, "Wire", 0.025f, InkColor, 7);
        Transform key = Child(root.transform, "Key", new Vector2(1.6f, -3.35f)).transform;
        key.localScale = Vector3.one * 0.45f;
        Art(key, "Art", "key", 6, true, new Vector2(0.6f, 0f));
        var junk = new List<Transform>();
        foreach (var j in new[] { ("tin_can", new Vector2(-1.8f, -3.3f)), ("horseshoe", new Vector2(0.4f, -3.35f)), ("tin_can", new Vector2(2.4f, -3.3f)), ("horseshoe", new Vector2(-0.7f, -3.3f)) })
        {
            Transform t = Child(root.transform, j.Item1, j.Item2).transform;
            Art(t, "Art", j.Item1, 6);
            junk.Add(t);
        }
        Transform piranha = Child(root.transform, "Piranha", new Vector2(0f, -1f)).transform;
        piranha.localScale = Vector3.one * 0.8f;
        Art(piranha, "Art", "piranha", 7);

        SetField(level, "anchor", anchor);
        SetField(level, "magnet", magnet);
        SetField(level, "wire", wire);
        SetField(level, "key", key);
        SetArray(level, "junk", junk.ToArray());
        SetField(level, "piranha", piranha);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.9f, 3.5f)));
        return SaveLevel(root, "Magnet");
    }

    // ---------- 93: l'orologio a cucù ----------

    static GameObject BuildCuckooLevel()
    {
        var root = new GameObject("Level_Cuckoo");
        CuckooLevel level = root.AddComponent<CuckooLevel>();
        Bar(root.transform, "Wall", Vector2.zero, new Vector2(12f, 16f), new Color(0.95f, 0.86f, 0.7f), -20);
        Transform clock = Child(root.transform, "Clock", new Vector2(0, 0.6f)).transform;
        clock.localScale = Vector3.one * 1.1f;
        Art(clock, "Art", "cuckoo_clock", 2);
        CircleCollider2D face = clock.gameObject.AddComponent<CircleCollider2D>();
        face.radius = 1.05f;
        Transform hour = Child(clock, "Hour", Vector2.zero).transform;
        Bar(hour, "Hand", new Vector2(0, 0.27f), new Vector2(0.1f, 0.55f), InkColor, 4);
        Transform minute = Child(clock, "Minute", Vector2.zero).transform;
        Bar(minute, "Hand", new Vector2(0, 0.4f), new Vector2(0.05f, 0.82f), new Color(0.6f, 0.15f, 0.15f), 5);
        Art(clock, "Pin", "led", 6, false).localScale = Vector3.one * 0.3f;
        Transform cuckoo = Child(clock, "Cuckoo", new Vector2(0, 1.45f)).transform;
        cuckoo.localScale = Vector3.one * 0.5f;
        Art(cuckoo, "Art", "bird_up", 7);
        Bar(root.transform, "Note", new Vector2(0, -2.35f), new Vector2(4.8f, 1.25f), new Color(1f, 0.96f, 0.6f), 3);
        TextMesh riddle = Label(root.transform, new Vector2(0, -2.35f), 4, "", 0.05f);
        TapButton set = SwitchButton(root.transform, "SetButton", "big_button", new Vector2(0, -3.75f));
        set.transform.localScale = Vector3.one * 0.6f;

        SetField(level, "hourHand", hour);
        SetField(level, "minuteHand", minute);
        SetField(level, "face", face);
        SetField(level, "setButton", set);
        SetField(level, "riddle", riddle);
        SetField(level, "cuckoo", cuckoo);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.2f, 3.4f)));
        return SaveLevel(root, "Cuckoo");
    }

    // ---------- 94: le lucciole ----------

    static GameObject BuildFirefliesLevel()
    {
        var root = new GameObject("Level_Fireflies");
        FirefliesLevel level = root.AddComponent<FirefliesLevel>();
        Art(root.transform, "Night", "camp_bg", -20, false);
        Bar(root.transform, "Dim", Vector2.zero, new Vector2(12f, 16f), new Color(0f, 0f, 0.08f, 0.45f), -15);
        var eyes = new List<Transform>();
        foreach (Vector2 b in new[] { new Vector2(-1.8f, -2.9f), new Vector2(0.4f, -3.2f), new Vector2(2.1f, -2.7f) })
        {
            Art(root.transform, "Bush", "bush", 3, true, b);
            Transform e = Child(root.transform, "Eyes", b + new Vector2(0, 0.55f)).transform;
            Art(e, "Open", "wolf_eyes", 4, false).localScale = Vector3.one * 0.8f;
            eyes.Add(e);
        }
        var flies = new List<Transform>();
        for (int i = 0; i < 12; i++)
        {
            Transform f = Child(root.transform, "Firefly" + i, Vector2.zero).transform;
            Art(f, "Bug", "firefly", 6, false);
            Transform glow = Art(f, "Glow", "glow", 5, false);
            glow.localScale = Vector3.one * 0.8f;
            glow.GetComponent<SpriteRenderer>().color = new Color(1f, 0.95f, 0.4f);
            flies.Add(f);
        }
        Transform jar = Child(root.transform, "Jar", new Vector2(-2.0f, 2.9f)).transform;
        jar.localScale = Vector3.one * 0.5f;
        Art(jar, "Art", "jar", 8);
        SpriteRenderer jarGlow = Art(jar, "Glow", "glow", 9, false).GetComponent<SpriteRenderer>();
        jarGlow.transform.localScale = Vector3.one * 1.6f;
        TextMesh counter = Label(root.transform, new Vector2(-1.2f, 2.9f), 12, "0 / 6", 0.06f);
        counter.color = Color.white;
        GameObject wolf = Art(root.transform, "Wolf", "wolf", 20).gameObject;
        TextMesh wanted = Label(root.transform, new Vector2(0.9f, 2.9f), 12, "CATCH: YELLOW", 0.05f);

        SetArray(level, "flies", flies.ToArray());
        SetArray(level, "eyes", eyes.ToArray());
        SetField(level, "jar", jar);
        SetField(level, "jarGlow", jarGlow);
        SetField(level, "counter", counter);
        SetField(level, "wolf", wolf);
        SetField(level, "wantedLabel", wanted);
        Tune(level, "areaMin", new Vector2(-2.4f, -2.6f));
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.8f, 3.6f)));
        return SaveLevel(root, "Fireflies");
    }

    // ---------- 96: le bolle numerate ----------

    static GameObject BuildBubbleNumbersLevel()
    {
        var root = new GameObject("Level_BubbleNumbers");
        BubbleNumbersLevel level = root.AddComponent<BubbleNumbersLevel>();
        Art(root.transform, "Bath", "bathroom_bg", -20, false);
        var labels = new[] { ("1", 1, 0f, 0.09f), ("II", 2, 0f, 0.08f), ("three", 3, 0f, 0.045f), ("2+2", 4, 0f, 0.06f), ("V", 5, 0f, 0.09f),
            ("6\n_", 6, 45f, 0.07f), ("SEVEN", 7, 0f, 0.04f), ("8", 8, 35f, 0.09f), ("9\n_", 9, -40f, 0.07f) };
        var bubbles = new List<Transform>();
        var values = new List<int>();
        var spins = new List<float>();
        foreach (var l in labels)
        {
            Transform b = Child(root.transform, "Bubble" + l.Item2, Vector2.zero).transform;
            b.localScale = Vector3.one * 1.15f;
            Art(b, "Art", "soap_bubble", 5, false);
            Transform label = Child(b, "Label", Vector2.zero).transform;
            label.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            Label(label, l.Item1.Contains("\n") ? new Vector2(0, -0.08f) : Vector2.zero, 6, l.Item1, l.Item4);
            bubbles.Add(b);
            values.Add(l.Item2);
            spins.Add(l.Item3);
        }
        TextMesh next = Label(root.transform, new Vector2(1.6f, 3.6f), 12, "NEXT: 1", 0.055f);

        SetArray(level, "bubbles", bubbles.ToArray());
        Tune(level, "values", values.ToArray());
        Tune(level, "spin", spins.ToArray());
        SetField(level, "nextLabel", next);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.8f, 3.4f)));
        return SaveLevel(root, "BubbleNumbers");
    }

    // ---------- 97: il topo furtivo ----------

    static GameObject BuildSneakyLevel()
    {
        var root = new GameObject("Level_Sneaky");
        SneakyCatLevel level = root.AddComponent<SneakyCatLevel>();
        Art(root.transform, "Floor", "night_floor_bg", -20, false);
        Transform dog = Child(root.transform, "Dog", new Vector2(0, 2.9f)).transform;
        Art(dog, "Art", "guard_dog", 8);
        Transform cone = Child(root.transform, "Cone", new Vector2(0, 2.72f)).transform;
        Transform beam = Art(cone, "Beam", "light_cone", 6, false);
        cone.localScale = Vector3.one * 1.35f;
        beam.GetComponent<SpriteRenderer>().color = new Color(1f, 0.95f, 0.6f);
        var furniture = new List<Collider2D>();
        foreach (var f in new[] { ("sofa", new Vector2(-1.0f, 0.7f), new Vector2(2.3f, 1.0f)), ("box", new Vector2(1.5f, -0.6f), new Vector2(1f, 0.9f)),
                     ("box", new Vector2(-1.7f, -1.7f), new Vector2(1f, 0.9f)), ("plant_top", new Vector2(0.6f, -2.3f), new Vector2(0.7f, 0.7f)) })
        {
            GameObject g = Child(root.transform, f.Item1, f.Item2);
            Art(g.transform, "Art", f.Item1, 7);
            g.AddComponent<BoxCollider2D>().size = f.Item3;
            furniture.Add(g.GetComponent<BoxCollider2D>());
        }
        Transform mouse = Child(root.transform, "Mouse", new Vector2(-2.2f, -3.4f)).transform;
        mouse.localScale = Vector3.one * 0.4f;
        Art(mouse, "Art", "mouse", 10);
        SpriteRenderer noise = Art(mouse, "Noise", "ring_waves", 11, false, new Vector2(0, 1.4f)).GetComponent<SpriteRenderer>();
        noise.color = new Color(1f, 1f, 1f, 0f);
        Transform cheese = Art(root.transform, "Cheese", "cheese_wedge", 8, true, new Vector2(2.2f, 1.5f));
        cheese.localScale = Vector3.one * 0.55f;

        SetField(level, "cat", mouse);
        SetField(level, "dog", dog);
        SetField(level, "cone", cone);
        SetField(level, "fish", cheese);
        SetArray(level, "furniture", furniture.ToArray());
        SetField(level, "noiseIcon", noise);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.2f, 3.5f)));
        return SaveLevel(root, "Sneaky");
    }

    // ---------- 99: acchiappa la talpa ----------

    static GameObject BuildWhackLevel()
    {
        var root = new GameObject("Level_Whack");
        WhackLevel level = root.AddComponent<WhackLevel>();
        Art(root.transform, "Garden", "garden_bg", -20, false);
        var holes = new List<Transform>();
        for (int r = 0; r < 2; r++)
            for (int c = 0; c < 3; c++)
            {
                Transform h = Child(root.transform, "Hole", new Vector2(-1.7f + c * 1.7f, -0.5f - r * 1.9f)).transform;
                Art(h, "Back", "mole_hole", 2 + r * 10, false);
                Transform pop = Child(h, "Pop", new Vector2(0, -0.9f)).transform;
                foreach (var k in new[] { ("Angry", "mole_angry"), ("Party", "mole_party"), ("Gold", "mole_gold"), ("Cat", "cat") })
                {
                    Transform a = Art(pop, k.Item1, k.Item2, 3 + r * 10, false);
                    if (k.Item1 == "Cat") { a.localScale = Vector3.one * 0.4f; a.localPosition = a.localPosition * 0.4f + new Vector3(0, 0.45f, 0); }
                    a.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                }
                Art(h, "Front", "mole_hole_front", 4 + r * 10, false);
                GameObject mask = Child(h, "Mask", new Vector2(0, 0.78f));
                mask.transform.localScale = new Vector3(1.5f, 1.6f, 1f);
                mask.AddComponent<SpriteMask>().sprite = sprites["plain"];
                holes.Add(h);
            }
        TextMesh counter = Label(root.transform, new Vector2(1.9f, 3.6f), 30, "0 / 12", 0.06f);
        Transform time = GaugeBar(root.transform, "Time", new Vector2(-0.6f, 3.6f), new Vector2(2.2f, 0.18f), new Color(0.17f, 0.13f, 0.2f, 0.6f), new Color(0.4f, 0.85f, 0.4f), 30);
        Transform hammer = Art(root.transform, "Hammer", "mallet", 40, false);

        SetArray(level, "holes", holes.ToArray());
        SetField(level, "counter", counter);
        SetField(level, "timeBar", time);
        SetField(level, "hammer", hammer);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0, 2.4f)));
        return SaveLevel(root, "Whack");
    }

}
#endif
