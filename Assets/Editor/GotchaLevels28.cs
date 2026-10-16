#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli della versione 0.28 (54, 71, 77, 78, 79, 80, 82, 89, 95, 98, 100).</summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 54: il puntatore laser ----------

    static GameObject BuildLaserLevel()
    {
        var root = new GameObject("Level_Laser");
        LaserLevel level = root.AddComponent<LaserLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);
        Bar(root.transform, "Dim", Vector2.zero, new Vector2(12f, 16f), new Color(0.05f, 0.03f, 0.1f, 0.55f), -15);
        Vector2 origin = new Vector2(-1.8f, -2.6f);
        const float cell = 0.9f;
        Vector2 C(int c, int r) => origin + new Vector2(c * cell, r * cell);
        for (int r = 0; r < 6; r++)
            for (int c = 0; c < 5; c++)
                Bar(root.transform, "Tile", C(c, r), new Vector2(cell - 0.06f, cell - 0.06f), new Color(1f, 1f, 1f, (c + r) % 2 == 0 ? 0.08f : 0.13f), -10);
        var mirrors = new List<Transform>();
        foreach (Vector2Int m in new[] { new Vector2Int(1, 1), new Vector2Int(1, 4), new Vector2Int(3, 4), new Vector2Int(3, 2), new Vector2Int(4, 2),
                     new Vector2Int(0, 4), new Vector2Int(2, 2), new Vector2Int(2, 5), new Vector2Int(4, 4) })
        {
            Transform t = Art(root.transform, "Mirror", "mirror_tile", 5, false, C(m.x, m.y));
            t.localScale = Vector3.one * cell;
            mirrors.Add(t);
        }
        var vases = new List<Transform>();
        foreach (Vector2Int v in new[] { new Vector2Int(3, 1), new Vector2Int(0, 3), new Vector2Int(4, 5) })
            vases.Add(Art(root.transform, "Vase", "vase", 5, true, C(v.x, v.y)));
        Transform cat = Child(root.transform, "Cat", C(4, 0)).transform;
        cat.localScale = Vector3.one * 0.32f;
        Art(cat, "Art", "cat", 6);
        GameObject eyes = Art(cat, "Eyes", "cat_eyes", 7, false).gameObject;
        Transform emitter = Child(root.transform, "Emitter", C(-1, 1) + new Vector2(0.3f, 0f)).transform;
        Art(emitter, "Pen", "laser_pen", 6, false);
        LineRenderer beam = Line(root.transform, "Beam", 0.06f, new Color(1f, 0.15f, 0.2f), 8);
        Transform dot = Art(root.transform, "Dot", "glow", 9, false);
        dot.localScale = Vector3.one * 0.5f;
        dot.GetComponent<SpriteRenderer>().color = new Color(1f, 0.2f, 0.25f);
        TextMesh battery = Label(root.transform, new Vector2(0, 2.75f), 12, "BATTERY", 0.05f);
        battery.color = new Color(1f, 0.85f, 0.4f);

        SetArray(level, "mirrors", mirrors.ToArray());
        SetArray(level, "vases", vases.ToArray());
        SetField(level, "cat", cat);
        SetField(level, "catEyes", eyes);
        SetField(level, "emitter", emitter);
        SetField(level, "beam", beam);
        SetField(level, "dot", dot);
        SetField(level, "battery", battery);
        Tune(level, "origin", origin);
        Tune(level, "cell", cell);
        Tune(level, "solution", new[] { 0, 0, 1, 1, 1, -1, -1, -1, -1 });
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.4f, 3.5f)));
        return SaveLevel(root, "Laser");
    }

    // ---------- 71: il topo DJ ----------

    static GameObject BuildRhythmLevel()
    {
        var root = new GameObject("Level_Rhythm");
        RhythmLevel level = root.AddComponent<RhythmLevel>();
        Art(root.transform, "Disco", "disco_bg", -20, false);
        AudioSource music = root.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.clip = Music("Animals");
        music.volume = 0.8f;
        Bar(root.transform, "Lane", new Vector2(0.2f, -0.6f), new Vector2(6f, 0.8f), new Color(0f, 0f, 0f, 0.45f), -5);
        Transform ring = Art(root.transform, "Ring", "beat_ring", 6, false, new Vector2(-1.9f, -0.6f));
        SpriteRenderer note = Art(root.transform, "Note", "beat_note", 7, false).GetComponent<SpriteRenderer>();
        note.gameObject.SetActive(false);
        SpriteRenderer ghost = Art(root.transform, "Ghost", "beat_ghost", 7, false).GetComponent<SpriteRenderer>();
        ghost.gameObject.SetActive(false);
        Transform dancer = Child(root.transform, "Dancer", new Vector2(0.4f, -3.2f)).transform;
        dancer.localScale = Vector3.one * 0.7f;
        Art(dancer, "Mouse", "mouse", 10, true, new Vector2(0f, 0.55f));
        TextMesh score = Label(root.transform, new Vector2(0, 2.6f), 12, "", 0.045f);
        score.color = Color.white;

        SetField(level, "music", music);
        SetField(level, "noteTemplate", note);
        SetField(level, "ghostTemplate", ghost);
        SetField(level, "hitRing", ring);
        SetField(level, "dancer", dancer);
        SetField(level, "score", score);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.4f, 3.5f)));
        return SaveLevel(root, "Rhythm");
    }

    // ---------- 77: l'idraulico ----------

    static GameObject BuildPipesLevel()
    {
        var root = new GameObject("Level_Pipes");
        PipesLevel level = root.AddComponent<PipesLevel>();
        Art(root.transform, "Garden", "garden_bg", -20, false);
        Vector2 origin = new Vector2(-1.65f, -2.4f);
        const float cell = 1.1f;
        Bar(root.transform, "Board", origin + new Vector2(1.5f * cell, 2f * cell), new Vector2(4 * cell + 0.15f, 5 * cell + 0.15f), new Color(0.45f, 0.32f, 0.22f), -5);
        var tiles = new List<SpriteRenderer>();
        for (int r = 0; r < 5; r++)
            for (int c = 0; c < 4; c++)
                tiles.Add(Art(root.transform, "Pipe", "pipe_straight", 3, false, origin + new Vector2(c * cell, r * cell)).GetComponent<SpriteRenderer>());
        Art(root.transform, "Faucet", "faucet", 4, false, origin + new Vector2(-0.55f, 1 * cell));
        Vector2 potAt = origin + new Vector2(3 * cell + 0.85f, 2 * cell - 0.45f);
        Transform pot = Child(root.transform, "Pot", potAt).transform;
        pot.localScale = Vector3.one * 0.45f;
        Art(pot, "Pot", "pot", 4);
        GameObject dry = Art(pot, "Dry", "plant_dry", 3).gameObject;
        GameObject bloom = Art(pot, "Bloom", "plant_bloom", 3).gameObject;
        Transform wilt = GaugeBar(root.transform, "Wilt", new Vector2(0.3f, 3.9f), new Vector2(2.2f, 0.18f), new Color(0.17f, 0.13f, 0.2f, 0.6f), new Color(0.4f, 0.7f, 1f), 20);
        Label(root.transform, new Vector2(-1.35f, 3.9f), 21, "WATER", 0.035f);

        SetArray(level, "tiles", tiles.ToArray());
        SetField(level, "straight", sprites["pipe_straight"]);
        SetField(level, "corner", sprites["pipe_corner"]);
        SetField(level, "tee", sprites["pipe_tee"]);
        Tune(level, "origin", origin);
        Tune(level, "cell", cell);
        SetField(level, "plantDry", dry);
        SetField(level, "plantBloom", bloom);
        SetField(level, "wiltBar", wilt);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.4f, 3.3f)));
        return SaveLevel(root, "Pipes");
    }

    // ---------- 78: la moneta falsa ----------

    static GameObject BuildBalanceLevel()
    {
        var root = new GameObject("Level_Balance");
        BalanceLevel level = root.AddComponent<BalanceLevel>();
        Bar(root.transform, "Wall", Vector2.zero, new Vector2(12f, 16f), new Color(0.93f, 0.88f, 0.78f), -20);
        Bar(root.transform, "Table", new Vector2(0, -3.6f), new Vector2(7f, 2.2f), new Color(0.6f, 0.4f, 0.26f), -19);
        Bar(root.transform, "Base", new Vector2(0, -1.75f), new Vector2(1.4f, 0.2f), new Color(0.55f, 0.36f, 0.22f), 1);
        Bar(root.transform, "Pole", new Vector2(0, -0.2f), new Vector2(0.14f, 3.1f), new Color(0.55f, 0.36f, 0.22f), 1);
        Transform beam = Child(root.transform, "Beam", new Vector2(0, 1.35f)).transform;
        Bar(beam, "Bar", Vector2.zero, new Vector2(3.3f, 0.12f), Gold(), 3);
        Transform left = Child(root.transform, "LeftPan", new Vector2(-1.5f, 0.45f)).transform;
        Bar(left, "Plate", Vector2.zero, new Vector2(1.45f, 0.1f), Gold(), 2);
        Transform right = Child(root.transform, "RightPan", new Vector2(1.5f, 0.45f)).transform;
        Bar(right, "Plate", Vector2.zero, new Vector2(1.45f, 0.1f), Gold(), 2);
        var coins = new List<Transform>();
        for (int i = 0; i < 9; i++)
        {
            Transform c = Child(root.transform, "Coin" + (i + 1), new Vector2(-2.4f + i * 0.6f, -2.65f)).transform;
            Art(c, "Art", "coin", 5, false).localScale = Vector3.one * 1.3f;
            Label(c, new Vector2(0, -0.01f), 6, (i + 1).ToString(), 0.035f);
            coins.Add(c);
        }
        TapButton weigh = SwitchButton(root.transform, "Weigh", "big_button", new Vector2(0, -3.75f));
        weigh.transform.localScale = Vector3.one * 0.55f;
        TextMesh weighLabel = Label(root.transform, new Vector2(1.2f, -3.75f), 12, "WEIGH", 0.05f);
        TextMesh info = Label(root.transform, new Vector2(0, 2.55f), 12, "", 0.05f);

        SetArray(level, "coins", coins.ToArray());
        SetField(level, "beam", beam);
        SetField(level, "leftPan", left);
        SetField(level, "rightPan", right);
        SetField(level, "weighButton", weigh);
        SetField(level, "weighLabel", weighLabel);
        SetField(level, "info", info);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0, 3.4f)));
        return SaveLevel(root, "Balance");
    }

    static Color Gold() => new Color(1f, 0.79f, 0.24f);

    // ---------- 79: trova le differenze ----------

    static GameObject BuildSpotLevel()
    {
        var root = new GameObject("Level_Spot");
        SpotLevel level = root.AddComponent<SpotLevel>();
        Bar(root.transform, "Wall", Vector2.zero, new Vector2(12f, 16f), new Color(0.3f, 0.25f, 0.4f), -20);
        var items = new (string sprite, Vector2 at, float scale)[]
        {
            ("apple", new Vector2(-2.0f, 0.8f), 0.45f), ("cup", new Vector2(-0.9f, 0.9f), 0.6f), ("heart", new Vector2(0.2f, 0.85f), 1.3f),
            ("bell", new Vector2(1.4f, 0.75f), 0.55f), ("cherry", new Vector2(2.2f, 0.9f), 1.1f), ("carrot", new Vector2(-1.8f, -0.6f), 1.0f),
            ("cheese_wedge", new Vector2(-0.4f, -0.5f), 0.9f), ("fish", new Vector2(0.9f, -0.6f), 1.0f), ("balloon", new Vector2(2.1f, -0.4f), 0.3f),
            ("note", new Vector2(-1.1f, 0.0f), 1.1f), ("coin", new Vector2(0.4f, 0.1f), 1.8f),
        };
        Transform[] MakeFrame(string name, float y)
        {
            Transform f = Child(root.transform, name, new Vector2(0, y)).transform;
            Bar(f, "Edge", Vector2.zero, new Vector2(5.5f, 3.25f), InkColor, -12);
            Bar(f, "Paper", Vector2.zero, new Vector2(5.35f, 3.1f), new Color(1f, 0.97f, 0.88f), -11);
            var list = new List<Transform>();
            foreach (var it in items)
            {
                Transform h = Child(f, it.sprite, it.at).transform;
                h.localScale = Vector3.one * it.scale;
                Art(h, "Art", it.sprite, 3, false);
                list.Add(h);
            }
            return list.ToArray();
        }
        Transform[] top = MakeFrame("Top", 1.75f), bottom = MakeFrame("Bottom", -1.85f);
        SpriteRenderer marker = Art(root.transform, "Marker", "beat_ring", 30, false).GetComponent<SpriteRenderer>();
        marker.color = new Color(1f, 0.25f, 0.3f);
        marker.transform.localScale = Vector3.one * 1.3f;
        marker.gameObject.SetActive(false);
        TextMesh counter = Label(root.transform, new Vector2(0, 3.65f), 12, "", 0.045f);
        counter.color = Color.white;
        Transform time = GaugeBar(root.transform, "Time", new Vector2(0, -3.75f), new Vector2(3f, 0.16f), new Color(0f, 0f, 0f, 0.5f), new Color(1f, 0.85f, 0.3f), 20);

        SetArray(level, "top", top);
        SetArray(level, "bottom", bottom);
        SetField(level, "markerTemplate", marker);
        SetField(level, "counter", counter);
        SetField(level, "timeBar", time);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0, 0.0f)));
        return SaveLevel(root, "Spot");
    }

    // ---------- 80: conta le pecore ----------

    static GameObject BuildSheepLevel()
    {
        var root = new GameObject("Level_Sheep");
        SheepLevel level = root.AddComponent<SheepLevel>();
        Art(root.transform, "Night", "camp_bg", -20, false);
        Bar(root.transform, "Dim", Vector2.zero, new Vector2(12f, 16f), new Color(0f, 0f, 0.1f, 0.35f), -15);
        Transform fence = Child(root.transform, "Fence", new Vector2(0, -3.0f)).transform;
        Color wood = new Color(0.62f, 0.42f, 0.26f);
        Bar(fence, "Post", new Vector2(0, 0.45f), new Vector2(0.16f, 0.95f), wood, 4);
        Bar(fence, "Rail1", new Vector2(0, 0.35f), new Vector2(0.9f, 0.1f), wood, 4);
        Bar(fence, "Rail2", new Vector2(0, 0.7f), new Vector2(0.9f, 0.1f), wood, 4);
        Label(root.transform, new Vector2(1.7f, -1.4f), 5, "PEN →", 0.05f).color = Color.white;
        Transform sheep = Child(root.transform, "Sheep", new Vector2(-5f, -3f)).transform;
        sheep.localScale = Vector3.one * 0.75f;
        Art(sheep, "Art", "sheep", 6);
        sheep.gameObject.SetActive(false);
        GameObject keypad = KeypadOverlay(root.transform, out BoxCollider2D panelCol, out TextMesh display, out TapButton[] keys);
        TextMesh question = Label(root.transform, new Vector2(0, -2.7f), 40, "How many sheep in the PEN?", 0.055f);
        question.color = Color.white;
        GameObject sleep = Bar(root.transform, "Sleep", Vector2.zero, new Vector2(12f, 16f), new Color(0f, 0f, 0f, 0.88f), 70).gameObject;
        Label(sleep.transform, Vector2.zero, 71, "Z z z . . .", 0.02f).color = Color.white;

        SetField(level, "sheepTemplate", sheep);
        SetField(level, "fence", fence);
        SetField(level, "keypad", keypad);
        SetArray(level, "keys", keys);
        SetField(level, "display", display);
        SetField(level, "question", question);
        SetField(level, "sleepOverlay", sleep);
        Tune(level, "groundY", -3.0f);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0, 3.3f)));
        return SaveLevel(root, "Sheep");
    }

    // ---------- 82: le ombre cinesi ----------

    static GameObject BuildShadowLevel()
    {
        var root = new GameObject("Level_Shadow");
        ShadowLevel level = root.AddComponent<ShadowLevel>();
        Bar(root.transform, "Wall", Vector2.zero, new Vector2(12f, 16f), new Color(0.98f, 0.9f, 0.72f), -20);
        Bar(root.transform, "Floor", new Vector2(0, -5.2f), new Vector2(12f, 5f), new Color(0.55f, 0.38f, 0.26f), -19);
        Art(root.transform, "Lamp", "lamp", -5, false, new Vector2(0, 3.6f)).localScale = Vector3.one * 0.6f;
        SpriteRenderer shadow = Child(root.transform, "Shadow", new Vector2(0, 1.6f)).AddComponent<SpriteRenderer>();
        shadow.sortingOrder = 2;
        shadow.color = new Color(0.08f, 0.06f, 0.12f, 0.75f);
        SpriteRenderer item = Child(root.transform, "Item", new Vector2(0, -2.2f)).AddComponent<SpriteRenderer>();
        item.sortingOrder = 6;
        TextMesh counter = Label(root.transform, new Vector2(1.9f, 3.7f), 12, "", 0.05f);
        Transform time = GaugeBar(root.transform, "Time", new Vector2(-0.6f, 3.7f), new Vector2(2.2f, 0.16f), new Color(0.17f, 0.13f, 0.2f, 0.5f), new Color(1f, 0.85f, 0.3f), 20);

        SetField(level, "shadow", shadow);
        SetField(level, "item", item);
        SetArray(level, "objects", new Object[] { sprites["fish"], sprites["carrot"], sprites["key"] });
        SetField(level, "counter", counter);
        SetField(level, "timeBar", time);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0, -0.6f)));
        return SaveLevel(root, "Shadow");
    }

    // ---------- 89: il puzzle scorrevole ----------

    static GameObject BuildSlideLevel()
    {
        var root = new GameObject("Level_Slide");
        SlideLevel level = root.AddComponent<SlideLevel>();
        Bar(root.transform, "Wall", Vector2.zero, new Vector2(12f, 16f), new Color(0.55f, 0.75f, 0.85f), -20);
        Bar(root.transform, "Frame", new Vector2(0, 0.2f), new Vector2(4.6f, 4.6f), new Color(0.55f, 0.36f, 0.22f), -5);
        Bar(root.transform, "Slot", new Vector2(0, 0.2f), new Vector2(4.25f, 4.25f), new Color(0.2f, 0.15f, 0.12f), -4);
        Transform pic = Art(root.transform, "Picture", "puzzle_cat", 3, false, new Vector2(0, 0.2f));
        pic.localScale = Vector3.one * (4.2f / 3.6f);
        GameObject tile = Child(root.transform, "Tile", Vector2.zero);
        SpriteRenderer tileSr = tile.AddComponent<SpriteRenderer>();
        tileSr.sortingOrder = 4;
        Label(tile.transform, new Vector2(-0.52f, 0.5f), 5, "1", 0.03f).color = Color.white;
        tile.SetActive(false);
        TextMesh moves = Label(root.transform, new Vector2(0, -2.7f), 12, "", 0.05f);
        Transform time = GaugeBar(root.transform, "Time", new Vector2(0, 3.6f), new Vector2(3f, 0.16f), new Color(0.17f, 0.13f, 0.2f, 0.5f), new Color(1f, 0.6f, 0.3f), 20);

        SetField(level, "picture", pic.GetComponent<SpriteRenderer>());
        SetField(level, "tileTemplate", tileSr);
        SetField(level, "moves", moves);
        SetField(level, "timeBar", time);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0, -3.4f)));
        return SaveLevel(root, "Slide");
    }

    // ---------- 95: memory ----------

    static GameObject BuildMemoryLevel()
    {
        var root = new GameObject("Level_Memory");
        MemoryLevel level = root.AddComponent<MemoryLevel>();
        Bar(root.transform, "Table", Vector2.zero, new Vector2(12f, 16f), new Color(0.2f, 0.5f, 0.35f), -20);
        var cards = new List<Transform>();
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
            {
                Transform card = Child(root.transform, "Card", new Vector2(-1.65f + c * 1.1f, 1.7f - r * 1.35f)).transform;
                Art(card, "Back", "card_back", 4, false);
                Transform face = Art(card, "Face", "card_face", 4, false);
                var icon = new GameObject("Icon");
                icon.transform.SetParent(face, false);
                icon.AddComponent<SpriteRenderer>().sortingOrder = 5;
                cards.Add(card);
            }
        Transform paw = Child(root.transform, "Paw", new Vector2(0, 5f)).transform;
        Art(paw, "Art", "paw", 30);
        TextMesh info = Label(root.transform, new Vector2(0, 3.0f), 12, "", 0.042f);
        info.color = Color.white;

        SetArray(level, "cards", cards.ToArray());
        SetArray(level, "icons", new Object[] { sprites["apple"], sprites["fish"], sprites["cherry"], sprites["carrot"], sprites["heart"], sprites["bell"], sprites["cheese_wedge"], sprites["coin"] });
        SetField(level, "paw", paw);
        SetField(level, "info", info);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0, 3.6f)));
        return SaveLevel(root, "Memory");
    }

    // ---------- 98: l'ingorgo ----------

    static GameObject BuildTrafficLevel()
    {
        var root = new GameObject("Level_Traffic");
        TrafficLevel level = root.AddComponent<TrafficLevel>();
        Bar(root.transform, "Ground", Vector2.zero, new Vector2(12f, 16f), new Color(0.35f, 0.38f, 0.42f), -20);
        const float cell = 0.85f;
        Vector2 center = new Vector2(0, -0.2f);
        Vector2 origin = center + new Vector2(-2.5f * cell, 2.5f * cell);
        Bar(root.transform, "Edge", center, new Vector2(6 * cell + 0.2f, 6 * cell + 0.2f), new Color(0.95f, 0.85f, 0.3f), -12);
        Bar(root.transform, "Lot", center, new Vector2(6 * cell, 6 * cell), new Color(0.25f, 0.27f, 0.3f), -11);
        Bar(root.transform, "Exit", origin + new Vector2(5.5f * cell + 0.1f, -2 * cell), new Vector2(0.25f, cell), new Color(0.25f, 0.27f, 0.3f), -10);
        Label(root.transform, origin + new Vector2(5.5f * cell + 0.25f, -2 * cell + 0.6f), 5, "EXIT", 0.035f).color = Color.white;
        for (int i = 1; i < 6; i++) Bar(root.transform, "Line", origin + new Vector2((i - 0.5f) * cell, -2.5f * cell), new Vector2(0.03f, 6 * cell), new Color(1f, 1f, 1f, 0.15f), -10);
        // furgone rosso + 10 auto (puzzle verificato: si risolve in 22 mosse)
        var spec = new (int c, int r, int len, bool h, string sprite)[]
        {
            (0, 2, 2, true, "car_red"), (3, 4, 2, true, "car_blue"), (0, 0, 2, true, "car_green"), (2, 0, 3, false, "truck_purple"),
            (3, 0, 2, false, "car_yellow"), (3, 5, 3, true, "truck_orange"), (0, 3, 2, true, "car_yellow"), (0, 4, 2, false, "car_blue"),
            (4, 0, 2, true, "car_blue"), (3, 2, 2, false, "car_green"), (5, 1, 3, false, "truck_purple"),
        };
        var cars = new List<Transform>();
        foreach (var s in spec)
        {
            Transform car = Child(root.transform, "Car", Vector2.zero).transform;
            Transform art = Art(car, "Art", s.sprite, 5, false);
            art.localRotation = Quaternion.Euler(0, 0, s.h ? 0f : 90f);
            art.localScale = Vector3.one * (cell / 0.9f);
            cars.Add(car);
        }
        TextMesh moves = Label(root.transform, new Vector2(1.6f, 3.3f), 12, "", 0.045f);
        moves.color = Color.white;
        Transform time = GaugeBar(root.transform, "Time", new Vector2(-0.9f, 3.3f), new Vector2(2.2f, 0.16f), new Color(0f, 0f, 0f, 0.5f), new Color(1f, 0.85f, 0.3f), 20);

        SetArray(level, "cars", cars.ToArray());
        Tune(level, "col", System.Array.ConvertAll(spec, s => s.c));
        Tune(level, "row", System.Array.ConvertAll(spec, s => s.r));
        Tune(level, "length", System.Array.ConvertAll(spec, s => s.len));
        Tune(level, "horizontal", System.Array.ConvertAll(spec, s => s.h));
        Tune(level, "origin", origin);
        Tune(level, "cell", cell);
        SetField(level, "timeBar", time);
        SetField(level, "moves", moves);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0, -3.4f)));
        return SaveLevel(root, "Traffic");
    }

    // ---------- 100: THE END? ----------

    static GameObject BuildCreditsLevel()
    {
        var root = new GameObject("Level_Credits");
        CreditsLevel level = root.AddComponent<CreditsLevel>();
        Bar(root.transform, "Black", Vector2.zero, new Vector2(12f, 16f), new Color(0.06f, 0.05f, 0.1f), -20);
        Transform roll = Child(root.transform, "Roll", new Vector2(0, -4.8f)).transform;
        string[] text =
        {
            "GOTCHA!", "", "GAME DESIGN", "Andrea Zecchin", "", "CHIEF CAT OFFICER", "The Cat", "", "WASP WRANGLERS", "Wasp #1   Wasp #2", "",
            "SNOWMAN MELTING CONSULTANT", "Mr. Snowman (retired)", "", "PIZZA TESTERS", "Four hungry mice", "", "COCKROACH STUNTS", "Himself", "",
            "LIGHTING", "A very tired lamp", "", "CHEESE SUPPLIER", "Nobody knows", "", "MOUSE", "(he did NOT sign a contract)", "",
            "SPECIAL THANKS", "You, for 100 levels", "", "", "THE END",
        };
        var lines = new List<TextMesh>();
        for (int i = 0; i < text.Length; i++)
        {
            bool head = text[i] == text[i].ToUpperInvariant() && text[i].Length > 0 && i > 0 && text[i] != "THE END";
            TextMesh t = Label(roll, new Vector2(0, -i * 0.62f), 5, text[i], head ? 0.04f : 0.055f);
            t.color = head ? new Color(1f, 0.8f, 0.3f) : Color.white;
            lines.Add(t);
        }
        Transform mouse = Child(root.transform, "Mouse", Vector2.zero).transform;
        mouse.localScale = Vector3.one * 0.3f;
        Art(mouse, "Art", "mouse", 8);
        GameObject skipGo = Child(root.transform, "Skip", new Vector2(2.1f, -3.8f));
        Label(skipGo.transform, Vector2.zero, 12, "SKIP >>", 0.045f).color = new Color(0.8f, 0.8f, 0.9f);
        skipGo.AddComponent<BoxCollider2D>().size = new Vector2(1.3f, 0.6f);
        TapButton skip = skipGo.AddComponent<TapButton>();
        skip.id = "skip";
        GameObject finale = Child(root.transform, "Finale", new Vector2(0, 0.6f));
        Bar(finale.transform, "Dim", new Vector2(0, -0.6f), new Vector2(12f, 16f), new Color(0.06f, 0.05f, 0.1f, 0.9f), 20);
        Label(finale.transform, new Vector2(0, 1.4f), 22, "THE REAL END", 0.13f).color = new Color(1f, 0.8f, 0.2f);
        Art(finale.transform, "Trophy", "trophy", 21, true, new Vector2(0, -1.2f));
        SmokeEmitter confetti = Emitter(root.transform, "Confetti", new Vector2(0, 3.0f), "confetti", 40f, 2.2f, -1.2f, 2.6f, 1.1f, new Color(1f, 0.85f, 0.35f), 45);
        TextMesh counter = Label(root.transform, new Vector2(-2.0f, 3.7f), 12, "", 0.05f);
        counter.color = Color.white;

        SetField(level, "roll", roll);
        SetArray(level, "lines", lines.ToArray());
        SetField(level, "mouse", mouse);
        SetField(level, "skip", skip);
        SetField(level, "finale", finale);
        SetField(level, "confetti", confetti);
        SetField(level, "counter", counter);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.4f, 3.3f)));
        return SaveLevel(root, "Credits");
    }
}
#endif
