#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli nuovi della versione 0.39 (gruppo A).</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels39A() => new List<LevelDefinition>
    {
        Define("BulbGrid", "bulb_grid", "BULB GRID", "Each bulb also flips its neighbours.", BuildBulbGridLevel()),
        Define("ColorTubes", "color_tubes", "SORT IT OUT", "One colour per tube. Is that grey one really a ball?", BuildColorTubesLevel()),
        Define("WaterJugs", "four_liters", "FOUR LITERS", "Drag a jug under the tap to fill it. A 5 and a 3 can make a 4: pour back and forth.", BuildFourLitersLevel()),
        Define("Untangle", "untangle", "YARN KNOT", "Drag the buttons until no strings cross.", BuildUntangleLevel()),
        Define("DotToDot", "dot_to_dot", "CONNECT THE DOTS", "Draw from 1 to 12... and back to 1.", BuildDotToDotLevel()),
    };

    /// <summary>Pulsante tondo RESET (cerchio largo ~0.85) con la scritta accanto.</summary>
    static Transform A39Reset(Transform root, Vector2 pos, Vector2 labelPos)
    {
        Transform b = Child(root, "Reset", pos).transform;
        Art(b, "Art", "reset_btn", 30, false);
        b.gameObject.AddComponent<CircleCollider2D>().radius = 0.45f;
        TextMesh t = Label(root, labelPos, 30, "RESET", 0.03f);
        t.color = Color.white;
        return b;
    }

    // ---------- Bulb grid ----------

    static GameObject BuildBulbGridLevel()
    {
        var root = new GameObject("Level_BulbGrid");
        BulbGridLevel level = root.AddComponent<BulbGridLevel>();
        Art(root.transform, "Attic", "attic_bg", -20, false);
        Vector2 center = new Vector2(0f, 0.6f);
        const float cell = 1.4f;
        Art(root.transform, "Board", "bulb_board", 1, false, center);
        var bulbs = new List<Object>();
        var glows = new List<Object>();
        for (int r = 0; r < 3; r++)
            for (int k = 0; k < 3; k++)
            {
                Transform h = Child(root.transform, "Bulb" + (r * 3 + k), center + new Vector2((k - 1) * cell, (1 - r) * cell)).transform;
                h.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.2f, 1.2f);
                SpriteRenderer g = Art(h, "Glow", "glow", 4, false, new Vector2(0f, 0.12f)).GetComponent<SpriteRenderer>();
                g.transform.localScale = Vector3.one * 2.2f;
                g.color = new Color(1f, 0.9f, 0.45f, 0.85f);
                glows.Add(g);
                bulbs.Add(Art(h, "Bulb", "bulb_off", 5, false).GetComponent<SpriteRenderer>());
            }
        SpriteRenderer dark = Bar(root.transform, "Darkness", Vector2.zero, new Vector2(12f, 16f), new Color(0.02f, 0.02f, 0.06f, 0.82f), 3);
        Transform mouse = Child(root.transform, "Mouse", new Vector2(-1.6f, -3.3f)).transform;
        mouse.localScale = Vector3.one * 0.9f;
        Art(mouse, "Art", "mouse", 6);
        Transform map = Child(root.transform, "Map", new Vector2(-0.3f, -3.45f)).transform;
        Art(map, "Art", "attic_map", 7, false);
        TextMesh moves = Label(root.transform, new Vector2(0f, 3.2f), 30, "MOVES: 0", 0.05f);
        moves.color = Color.white;
        Transform reset = A39Reset(root.transform, new Vector2(2.0f, -3.5f), new Vector2(2.0f, -4.08f));

        SetArray(level, "bulbs", bulbs.ToArray());
        SetArray(level, "glows", glows.ToArray());
        SetField(level, "bulbOn", sprites["bulb_on"]);
        SetField(level, "bulbOff", sprites["bulb_off"]);
        SetField(level, "darkness", dark);
        SetField(level, "resetButton", reset);
        SetField(level, "moves", moves);
        SetField(level, "map", map);
        SetField(level, "mouse", mouse);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.6f, -1.95f)));
        return SaveLevel(root, "BulbGrid");
    }

    // ---------- Color tubes ----------

    static GameObject BuildColorTubesLevel()
    {
        var root = new GameObject("Level_ColorTubes");
        ColorTubesLevel level = root.AddComponent<ColorTubesLevel>();
        Art(root.transform, "Room", "toyshelf_bg", -20, false);
        var tubes = new List<Object>();
        foreach (float x in new[] { -2.0f, -0.67f, 0.67f, 2.0f })
        {
            Transform t = Child(root.transform, "Tube", new Vector2(x, -2.6f)).transform;
            Art(t, "Glass", "glass_tube", 2, false);
            Art(t, "Shine", "tube_shine", 6, false);
            t.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.2f, 3.6f);
            t.GetComponent<BoxCollider2D>().offset = new Vector2(0f, 1.5f);
            tubes.Add(t);
        }
        string[] colors = { "tube_ball_red", "tube_ball_blue", "tube_ball_yellow" };
        var balls = new List<Object>();
        for (int i = 0; i < 12; i++) balls.Add(Art(root.transform, "Ball" + i, colors[i / 4], 4, false).GetComponent<SpriteRenderer>());
        SpriteRenderer hamster = Art(root.transform, "Hamster", "hamster_ball", 5, false).GetComponent<SpriteRenderer>();
        Transform reset = A39Reset(root.transform, new Vector2(-2.05f, -3.85f), new Vector2(-1.2f, -3.85f));

        SetArray(level, "tubes", tubes.ToArray());
        SetArray(level, "balls", balls.ToArray());
        SetField(level, "hamster", hamster);
        SetField(level, "hamsterRun", sprites["mouse"]);
        SetFloat(level, "hamsterRunScale", 0.5f);
        SetField(level, "resetButton", reset);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0f, 2.3f)));
        return SaveLevel(root, "ColorTubes");
    }

    // ---------- Four liters ----------

    static GameObject BuildFourLitersLevel()
    {
        var root = new GameObject("Level_WaterJugs");
        FourLitersLevel level = root.AddComponent<FourLitersLevel>();
        // cucina semplice: piastrelle, battiscopa e pavimento (lo sfondo della stanza ha la pianta proprio dove sta il gatto)
        Bar(root.transform, "Wall", new Vector2(0f, 2.0f), new Vector2(12f, 10f), new Color(0.84f, 0.93f, 0.9f), -20);
        for (float y = -2.4f; y < 5f; y += 0.6f) Bar(root.transform, "Grout", new Vector2(0f, y), new Vector2(12f, 0.03f), new Color(0.7f, 0.82f, 0.8f), -19);
        Bar(root.transform, "Floor", new Vector2(0f, -5.0f), new Vector2(12f, 4.2f), new Color(0.87f, 0.62f, 0.37f), -18);
        Bar(root.transform, "Skirting", new Vector2(0f, -2.9f), new Vector2(12f, 0.12f), new Color(0.55f, 0.29f, 0.13f), -17);
        Color steel = new Color(0.79f, 0.81f, 0.84f);
        // tavolo a destra
        Bar(root.transform, "Table", new Vector2(1.0f, -1.88f), new Vector2(3.8f, 0.18f), new Color(0.55f, 0.36f, 0.22f), 1);
        foreach (float x in new[] { -0.6f, 2.6f }) Bar(root.transform, "Leg", new Vector2(x, -2.45f), new Vector2(0.18f, 1.0f), new Color(0.45f, 0.3f, 0.18f), 0);
        // rubinetto con vaschetta (riempire)
        Bar(root.transform, "Pipe", new Vector2(0.35f, 3.1f), new Vector2(0.6f, 0.16f), steel, 5);
        Transform mouth = Child(root.transform, "Faucet", new Vector2(1.45f, 3.1f)).transform;
        mouth.localScale = Vector3.one * 1.3f;
        Art(mouth, "Art", "faucet", 6, false);
        Transform tapSpot = Child(root.transform, "TapSpot", new Vector2(1.45f, 0.75f)).transform;
        Bar(root.transform, "Tray", new Vector2(1.45f, 0.62f), new Vector2(1.5f, 0.1f), steel, 1);
        // istruzione ben visibile: dove si riempiono le caraffe (zona tratteggiata sotto il rubinetto)
        TextMesh fill = Fit(Label(root.transform, new Vector2(1.45f, 2.05f), 4, "FILL THE JUG HERE", 0.06f), 2.4f, 0.4f);
        for (int d = 0; d < 6; d++)
        {
            Bar(root.transform, "Dash", new Vector2(0.75f + d * 0.28f, 1.75f), new Vector2(0.16f, 0.04f), new Color(0.3f, 0.3f, 0.4f), 2);
            Bar(root.transform, "Dash", new Vector2(0.75f + d * 0.28f, 0.68f), new Vector2(0.16f, 0.04f), new Color(0.3f, 0.3f, 0.4f), 2);
        }
        for (int d = 0; d < 4; d++)
            foreach (float x in new[] { 0.62f, 2.28f })
                Bar(root.transform, "Dash", new Vector2(x, 0.82f + d * 0.27f), new Vector2(0.04f, 0.16f), new Color(0.3f, 0.3f, 0.4f), 2);
        // lavandino (svuotare)
        Transform sink = Child(root.transform, "Sink", new Vector2(-1.6f, 1.3f)).transform;
        Art(sink, "Art", "drain_sink", 3, false);
        Fit(Label(root.transform, new Vector2(-1.6f, 0.45f), 2, "EMPTY HERE", 0.05f), 1.8f, 0.35f);
        // gatto e ciotola
        Transform cat = Child(root.transform, "Cat", new Vector2(-1.85f, -2.3f)).transform;
        cat.localScale = Vector3.one * 0.36f;
        Art(cat, "Face", "cat_big_face", 2, false);
        var pupils = new List<Object>();
        foreach (float sx in new[] { -1f, 1f }) pupils.Add(Art(cat, "Pupil", "cat_pupil", 3, false, new Vector2(sx * 0.58f, 0.25f)));
        Transform bowl = Child(root.transform, "Bowl", new Vector2(-1.85f, -3.45f)).transform;
        Art(bowl, "Art", "cat_bowl", 5, false);
        Transform bowlWater = Child(bowl, "Water", new Vector2(0f, 0.22f)).transform;
        Art(bowlWater, "Art", "bowl_water", 6, false);
        // caraffe
        var jugs = new List<Object>();
        var waters = new List<Object>();
        var labels = new List<Object>();
        int[] caps = { 5, 3 };
        Vector2[] homes = { new Vector2(1.6f, -1.7f), new Vector2(-0.15f, -1.7f) };
        for (int j = 0; j < 2; j++)
        {
            float h = caps[j] * 0.36f;
            Transform jug = Child(root.transform, "Jug" + caps[j], homes[j]).transform;
            Transform water = Child(jug, "Water", Vector2.zero).transform;
            Bar(water, "Fill", new Vector2(0f, 0.5f), new Vector2(1.0f, 1.0f), new Color(0.3f, 0.65f, 0.95f, 0.85f), 7);
            Art(jug, "Glass", "jug_" + caps[j] + "l", 8, false);
            BoxCollider2D box = jug.gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1.6f, h + 0.6f);
            box.offset = new Vector2(0.1f, h * 0.5f);
            TextMesh t = Fit(Label(jug, new Vector2(0f, h * 0.5f), 9, "0 / " + caps[j] + " L", 0.045f), 0.95f);
            jugs.Add(jug);
            waters.Add(water);
            labels.Add(t);
        }
        SpriteRenderer stream = Bar(root.transform, "Stream", Vector2.zero, new Vector2(0.1f, 1f), new Color(0.45f, 0.75f, 1f, 0.85f), 9);
        fill.color = new Color(0.3f, 0.3f, 0.4f);

        SetArray(level, "jugs", jugs.ToArray());
        SetArray(level, "waters", waters.ToArray());
        SetArray(level, "labels", labels.ToArray());
        SetField(level, "tapSpot", tapSpot);
        SetField(level, "tapMouth", mouth);
        SetField(level, "sink", sink);
        SetField(level, "bowl", bowl);
        SetField(level, "bowlWater", bowlWater);
        SetField(level, "stream", stream);
        SetArray(level, "pupils", pupils.ToArray());
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.5f, -0.75f)));
        return SaveLevel(root, "WaterJugs");
    }

    // ---------- Untangle ----------

    static GameObject BuildUntangleLevel()
    {
        var root = new GameObject("Level_Untangle");
        UntangleLevel level = root.AddComponent<UntangleLevel>();
        Art(root.transform, "Rug", "yarn_rug_bg", -20, false);
        Transform cat = Child(root.transform, "Cat", new Vector2(0f, -3.7f)).transform;
        cat.localScale = Vector3.one * 0.6f;
        Art(cat, "Art", "cat", 2);
        Color[] tints = { new Color(1f, 0.75f, 0.8f), new Color(0.75f, 0.85f, 1f), new Color(1f, 0.92f, 0.6f), new Color(0.8f, 1f, 0.8f),
                          new Color(0.9f, 0.8f, 1f), new Color(1f, 0.82f, 0.65f), new Color(0.7f, 0.95f, 0.95f) };
        var nodes = new List<Object>();
        for (int i = 0; i < 7; i++)
        {
            Transform n = Child(root.transform, "Button" + i, Vector2.zero).transform;
            n.localScale = Vector3.one * 1.35f;
            Art(n, "Art", "yarn_button", 6, false).GetComponent<SpriteRenderer>().color = tints[i];
            n.gameObject.AddComponent<CircleCollider2D>().radius = 0.35f;
            nodes.Add(n);
        }
        var strings = new List<Object>();
        for (int e = 0; e < 10; e++) strings.Add(Line(root.transform, "String" + e, 0.08f, new Color(0.45f, 0.78f, 0.45f), 3));

        SetArray(level, "nodes", nodes.ToArray());
        SetArray(level, "strings", strings.ToArray());
        SetField(level, "cat", cat);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.0f, -2.75f)));
        return SaveLevel(root, "Untangle");
    }

    // ---------- Dot to dot ----------

    // contorno della chiave (coordinate del foglio, prima della scala 1.1): anello a sinistra, denti in basso a destra
    static readonly Vector2[] A39KeyDots =
    {
        new Vector2(-0.5f, 0.35f), new Vector2(1.9f, 0.35f), new Vector2(1.9f, -0.35f), new Vector2(1.4f, -0.35f),
        new Vector2(1.4f, -1.05f), new Vector2(0.8f, -1.05f), new Vector2(0.8f, -0.35f), new Vector2(-0.5f, -0.35f),
        new Vector2(-1.05f, -1.0f), new Vector2(-1.95f, -0.55f), new Vector2(-1.95f, 0.55f), new Vector2(-1.05f, 1.0f),
    };

    static GameObject BuildDotToDotLevel()
    {
        var root = new GameObject("Level_DotToDot");
        DotToDotLevel level = root.AddComponent<DotToDotLevel>();
        Bar(root.transform, "Wall", new Vector2(0f, 4.5f), new Vector2(12f, 8f), new Color(1f, 0.945f, 0.84f), -20);
        Bar(root.transform, "Floor", new Vector2(0f, -3.0f), new Vector2(12f, 7f), new Color(0.87f, 0.62f, 0.37f), -19);
        Bar(root.transform, "Skirting", new Vector2(0f, 0.5f), new Vector2(12f, 0.12f), new Color(0.55f, 0.29f, 0.13f), -18);
        // porta chiusa a chiave
        Transform door = Child(root.transform, "Door", new Vector2(0f, 2.0f)).transform;
        door.localScale = Vector3.one * 0.7f;
        Art(door, "Frame", "door_frame", 1, false);
        Bar(door, "Light", Vector2.zero, new Vector2(2.3f, 3.7f), new Color(1f, 0.96f, 0.78f), 2);
        Transform panel = Child(door, "Panel", new Vector2(-1.1f, 0f)).transform;
        Art(panel, "Art", "door_panel", 3, false);
        Transform keyhole = Child(panel, "Keyhole", new Vector2(1.85f, -0.15f)).transform;
        Art(keyhole, "Art", "keyhole", 5, false);
        Transform mouse = Child(root.transform, "Mouse", new Vector2(-1.85f, 0.95f)).transform;
        mouse.localScale = Vector3.one * 0.7f;
        Art(mouse, "Art", "mouse", 6);
        // foglio coi puntini
        Transform paper = Child(root.transform, "Paper", new Vector2(0f, -2.05f)).transform;
        Art(paper, "Art", "paper_sheet", 1, false);
        var dots = new List<Object>();
        for (int i = 0; i < A39KeyDots.Length; i++)
        {
            Vector2 at = A39KeyDots[i] * 1.1f;
            Transform d = Child(paper, "Dot" + (i + 1), at).transform;
            Art(d, "Art", "ink_dot", 3, false);
            Vector2 outward = at.sqrMagnitude > 0.01f ? at.normalized : Vector2.up;
            TextMesh n = Label(paper, at + outward * 0.26f + new Vector2(0f, 0.02f), 4, (i + 1).ToString(), 0.035f);
            n.color = new Color(0.35f, 0.3f, 0.45f);
            dots.Add(d);
        }
        Transform fly = Child(paper, "Fly", A39KeyDots[8] * 1.1f + new Vector2(0.03f, 0.03f)).transform;
        fly.localScale = Vector3.one * 1.1f;
        Art(fly, "Art", "house_fly", 7, false);
        fly.gameObject.AddComponent<CircleCollider2D>().radius = 0.45f;
        LineRenderer line = Line(root.transform, "Line", 0.07f, new Color(0.17f, 0.13f, 0.2f), 5);
        Transform key = Child(root.transform, "Key", Vector2.zero).transform;
        Art(key, "Art", "key", 10, false);
        key.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.5f, 0.9f);
        key.GetComponent<BoxCollider2D>().offset = new Vector2(-0.6f, 0f);

        SetArray(level, "dots", dots.ToArray());
        SetField(level, "line", line);
        SetField(level, "fly", fly);
        SetField(level, "paper", paper);
        SetField(level, "key", key);
        SetField(level, "keyhole", keyhole);
        SetField(level, "doorPanel", panel);
        SetField(level, "mouse", mouse);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.45f, 2.0f)));
        return SaveLevel(root, "DotToDot");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V39A = new Dictionary<string, string[]>
    {
        { "bulb_grid", new[] { "Tapping the same bulb twice undoes it. The scramble was only 3 or 4 taps.", "A dark plus shape (+) means: tap its centre bulb. Two overlapping pluses? Tap both centres. Lost? Press RESET." } },
        { "color_tubes", new[] { "The grey 'ball' is a curled-up hamster: lift it and it runs away, freeing space.", "Dig out the hamster first, then use the empty space to stack each colour in its own tube." } },
        { "four_liters", new[] { "Fill the 5 L jug and pour it into the 3 L one: 2 L stay in the big jug.", "Fill 5, pour into 3, empty 3, pour the 2 L into 3, fill 5, top up the 3: 4 L stay in the 5 L jug. Pour it in the bowl." } },
        { "untangle", new[] { "One button is tied to four others: put it in the middle.", "Put the busiest button in the centre, then arrange the other six around it in a ring, following the strings." } },
        { "dot_to_dot", new[] { "Something is sitting on dot 9. Tap it!", "Shoo the fly off dot 9, draw 1→12→1, then drag the key into the lock." } },
    };
}
#endif
