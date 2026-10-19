#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli nuovi della versione 0.39 (gruppo C): Sokoban, serpente, flipper, air hockey, basket.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels39C() => new List<LevelDefinition>
    {
        Define("Sokoban", "box_pusher", "BOX PUSHER", "Swipe to push the cheese crates onto the red marks.", BuildBoxPusherLevel()),
        Define("Snake", "hungry_snake", "HUNGRY SNAKE", "Swipe to steer the snake. Eat 8 apples!", BuildHungrySnakeLevel()),
        Define("Pinball", "pinball", "PINBALL", "Tap the left or right half of the screen to flip. Light all 3 bumpers!", BuildPinballLevel()),
        Define("AirHockey", "air_hockey", "AIR HOCKEY", "Drag your mallet and score 3 goals before the cat does.", BuildAirHockeyLevel()),
        Define("Basket", "slam_dunk", "SLAM DUNK", "Flick the ball up toward the hoop.", BuildSlamDunkLevel()),
    };

    // ---------- aiuti del gruppo C ----------

    /// <summary>Pulsante a pillola con scritta (collider 1.6×0.9).</summary>
    static TapButton PillButtonC39(Transform parent, string name, string text, Vector2 pos)
    {
        GameObject go = Child(parent, name, pos);
        Transform art = Child(go.transform, "Visual", Vector2.zero).transform;
        Art(art, "Art", "sok_button", 20, false);
        Fit(Label(art, new Vector2(0f, 0.02f), 21, text, 0.055f), 1.3f, 0.5f);
        go.AddComponent<BoxCollider2D>().size = new Vector2(1.6f, 0.9f);
        TapButton b = go.AddComponent<TapButton>();
        b.id = name;
        b.visual = art;
        return b;
    }

    /// <summary>Muro fisico (EdgeCollider2D) con il suo tratto disegnato (LineRenderer locale).</summary>
    static EdgeCollider2D WallC39(Transform parent, string name, Vector2[] pts, float width, Color color, int order)
    {
        GameObject go = Child(parent, name, Vector2.zero);
        EdgeCollider2D edge = go.AddComponent<EdgeCollider2D>();
        edge.points = pts;
        edge.edgeRadius = 0.03f;
        if (width > 0f)
        {
            LineRenderer lr = Line(go.transform, "Line", width, color, order);
            lr.useWorldSpace = false;
            lr.positionCount = pts.Length;
            for (int i = 0; i < pts.Length; i++) lr.SetPosition(i, pts[i]);
            lr.numCornerVertices = 4;
        }
        return edge;
    }

    // ---------- Box Pusher (Sokoban) ----------

    static GameObject BuildBoxPusherLevel()
    {
        var root = new GameObject("Level_Sokoban");
        BoxPusherLevel level = root.AddComponent<BoxPusherLevel>();
        Bar(root.transform, "Back", Vector2.zero, new Vector2(12f, 16f), new Color(0.36f, 0.3f, 0.27f), -30);
        Art(root.transform, "Floor", "sok_floor", -20, false, new Vector2(0f, BoxPusherLevel.BoardY));
        var crates = new List<Transform>();
        var glows = new List<GameObject>();
        Transform mouse = null;
        string[] map = BoxPusherLevel.Layout;
        for (int r = 0; r < map.Length; r++)
            for (int c = 0; c < map[r].Length; c++)
            {
                Vector2 at = BoxPusherLevel.CellPos(c, r);
                char ch = map[r][c];
                if (ch == '#') Art(root.transform, "Wall", "sok_wall", 2, false, at);
                if (ch == '.') Art(root.transform, "Mark", "sok_mark", -10, false, at);
                if (ch == '$')
                {
                    Transform crate = Child(root.transform, "Crate", at).transform;
                    GameObject glow = Art(crate, "Glow", "sok_glow", 3, false).gameObject;
                    glow.SetActive(false);
                    glows.Add(glow);
                    Art(crate, "Art", "sok_crate", 4, false);
                    crates.Add(crate);
                }
                if (ch == '@') mouse = Fitted(root.transform, "Mouse", "mouse", at, 0.64f, 6);
            }
        Vector2 pc = BoxPusherLevel.CellPos(BoxPusherLevel.PawCell.x, BoxPusherLevel.PawCell.y);
        SpriteRenderer shadow = Art(root.transform, "PawShadow", "sok_paw_shadow", 1, false, pc).GetComponent<SpriteRenderer>();
        Transform paw = Child(root.transform, "Paw", pc + new Vector2(3f, 0f)).transform;
        paw.localRotation = Quaternion.Euler(0, 0, -90f);   // il braccio entra da destra
        paw.localScale = Vector3.one * 0.85f;
        Art(paw, "Art", "paw", 9, false);
        TapButton undo = PillButtonC39(root.transform, "Undo", "UNDO", new Vector2(-1.25f, -3.9f));
        TapButton reset = PillButtonC39(root.transform, "Reset", "RESET", new Vector2(1.25f, -3.9f));
        TextMesh info = Label(root.transform, new Vector2(0f, 2.9f), 20, "", 0.055f);
        info.color = Color.white;

        SetField(level, "mouse", mouse);
        SetArray(level, "crates", crates.ToArray());
        SetArray(level, "glows", glows.ToArray());
        SetField(level, "paw", paw);
        SetField(level, "pawShadow", shadow);
        SetField(level, "undoButton", undo);
        SetField(level, "resetButton", reset);
        SetField(level, "info", info);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.3f, 3.35f)));
        return SaveLevel(root, "Sokoban");
    }

    // ---------- Hungry Snake ----------

    static GameObject BuildHungrySnakeLevel()
    {
        var root = new GameObject("Level_Snake");
        HungrySnakeLevel level = root.AddComponent<HungrySnakeLevel>();
        Bar(root.transform, "Back", Vector2.zero, new Vector2(12f, 16f), new Color(0.32f, 0.55f, 0.3f), -30);
        Art(root.transform, "Board", "snake_board", -20, false, new Vector2(0f, HungrySnakeLevel.BoardY));
        Transform apple = Fitted(root.transform, "Apple", "apple", HungrySnakeLevel.CellPos(4, 8), 0.5f, 4);
        Transform body = Child(root.transform, "Body", Vector2.zero).transform;
        Art(body, "Art", "snake_body", 5, false);
        Transform tail = Child(root.transform, "Tail", Vector2.zero).transform;
        Art(tail, "Art", "snake_tail", 5, false);
        Transform head = Child(root.transform, "Head", Vector2.zero).transform;
        Art(head, "Art", "snake_head", 8, false);
        Transform tongue = Child(head, "Tongue", new Vector2(0.3f, 0f)).transform;
        Art(tongue, "Art", "snake_tongue", 7, false);
        var hearts = new List<GameObject>();
        for (int i = 0; i < 3; i++) hearts.Add(Fitted(root.transform, "Heart", "heart", new Vector2(1.45f + i * 0.45f, 3.4f), 0.38f, 20).gameObject);
        TextMesh counter = Label(root.transform, new Vector2(-1.2f, 3.4f), 20, "", 0.055f);
        counter.color = Color.white;
        TextMesh prompt = Label(root.transform, new Vector2(0f, -1.2f), 25, "", 0.07f);
        prompt.color = Color.white;

        SetField(level, "head", head);
        SetField(level, "tongue", tongue);
        SetField(level, "bodyTemplate", body);
        SetField(level, "tail", tail);
        SetField(level, "apple", apple);
        SetArray(level, "hearts", hearts.ToArray());
        SetField(level, "counter", counter);
        SetField(level, "prompt", prompt);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0f, 2.5f)));
        return SaveLevel(root, "Snake");
    }

    // ---------- Pinball ----------

    static GameObject BuildPinballLevel()
    {
        var root = new GameObject("Level_Pinball");
        PinballLevel level = root.AddComponent<PinballLevel>();
        Art(root.transform, "Table", "pin_bg", -20, false);
        Color neon = new Color(0.45f, 0.95f, 1f);
        WallC39(root.transform, "Wall", PinballLevel.OuterWall(), 0.1f, neon, 2);
        WallC39(root.transform, "RightSlope", PinballLevel.RightSlope, 0.1f, neon, 2);
        Collider2D gate = WallC39(root.transform, "Gate", PinballLevel.Gate, 0.05f, new Color(1f, 0.6f, 0.85f, 0.7f), 2);

        var bumpers = new List<Transform>();
        var lit = new List<GameObject>();
        foreach (Vector2 at in PinballLevel.Bumpers)
        {
            Transform b = Child(root.transform, "Bumper", at).transform;
            b.gameObject.AddComponent<CircleCollider2D>().radius = PinballLevel.BumperRadius;
            Art(b, "Art", "pin_bumper", 4, false);
            lit.Add(Art(b, "Lit", "pin_bumper_lit", 5, false).gameObject);
            bumpers.Add(b);
        }
        Transform cat = Child(root.transform, "Cat", PinballLevel.CatPos).transform;
        cat.localScale = Vector3.one * PinballLevel.CatScale;
        cat.gameObject.AddComponent<CircleCollider2D>().radius = PinballLevel.CatRadius / PinballLevel.CatScale;
        GameObject closed = Art(cat, "Closed", "pin_cat_closed", 4, false).gameObject;
        GameObject open = Art(cat, "Open", "pin_cat_open", 4, false).gameObject;

        Rigidbody2D Flipper(string name, Vector2 pivot, bool mirrored)
        {
            GameObject f = Child(root.transform, name, pivot);
            f.transform.localRotation = Quaternion.Euler(0, 0, mirrored ? 180f - PinballLevel.RestAngle : PinballLevel.RestAngle);
            Rigidbody2D rb = f.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            CapsuleCollider2D cap = f.AddComponent<CapsuleCollider2D>();
            cap.direction = CapsuleDirection2D.Horizontal;
            cap.offset = new Vector2(0.42f, 0f);
            cap.size = new Vector2(1.06f, 0.24f);
            Art(f.transform, "Art", "pin_flipper", 6, false);
            return rb;
        }
        Rigidbody2D left = Flipper("LeftFlipper", PinballLevel.LeftPivot, false);
        Rigidbody2D right = Flipper("RightFlipper", PinballLevel.RightPivot, true);

        Transform plunger = Art(root.transform, "Plunger", "pin_plunger", 3, false, new Vector2(PinballLevel.LaunchPos.x, -4.32f));
        GameObject ballGo = Child(root.transform, "Ball", PinballLevel.LaunchPos);
        Rigidbody2D ball = ballGo.AddComponent<Rigidbody2D>();
        ball.gravityScale = 0f;
        ballGo.AddComponent<CircleCollider2D>().radius = PinballLevel.BallRadius;
        Fitted(ballGo.transform, "Art", "chrome_ball", Vector2.zero, PinballLevel.BallRadius * 2f, 8);

        TextMesh balls = Label(root.transform, new Vector2(-1.7f, 3.4f), 20, "", 0.05f);
        balls.color = Color.white;
        TextMesh prompt = Label(root.transform, new Vector2(-0.2f, -1.55f), 20, "", 0.045f);
        prompt.color = new Color(1f, 0.95f, 0.6f);

        SetField(level, "ball", ball);
        SetField(level, "leftFlipper", left);
        SetField(level, "rightFlipper", right);
        SetArray(level, "bumpers", bumpers.ToArray());
        SetArray(level, "bumperLit", lit.ToArray());
        SetField(level, "catClosed", closed);
        SetField(level, "catOpen", open);
        SetField(level, "gate", gate);
        SetField(level, "plunger", plunger);
        SetField(level, "ballsLabel", balls);
        SetField(level, "prompt", prompt);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.9f, 3.3f)));
        return SaveLevel(root, "Pinball");
    }

    // ---------- Air Hockey ----------

    static GameObject BuildAirHockeyLevel()
    {
        var root = new GameObject("Level_AirHockey");
        AirHockeyLevel level = root.AddComponent<AirHockeyLevel>();
        Bar(root.transform, "Back", Vector2.zero, new Vector2(12f, 16f), new Color(0.22f, 0.2f, 0.36f), -30);
        float w = AirHockeyLevel.HalfW, top = AirHockeyLevel.Top, bot = AirHockeyLevel.Bottom, g = AirHockeyLevel.GoalHalf;
        float cg = AirHockeyLevel.CatGoalHalf;   // la porta del gatto è più larga
        Art(root.transform, "Table", "hockey_table_wide", -20, false, new Vector2(0f, (top + bot) * 0.5f));
        foreach (float s in new[] { -1f, 1f })
        {
            WallC39(root.transform, "Wall", new[] { new Vector2(s * cg, top), new Vector2(s * w, top), new Vector2(s * w, bot), new Vector2(s * g, bot) }, 0f, Color.clear, 0);
            WallC39(root.transform, "PostTop", new[] { new Vector2(s * cg, top), new Vector2(s * cg, top + 0.5f) }, 0f, Color.clear, 0);
            WallC39(root.transform, "PostBottom", new[] { new Vector2(s * g, bot), new Vector2(s * g, bot - 0.5f) }, 0f, Color.clear, 0);
        }
        GameObject puckGo = Child(root.transform, "Puck", new Vector2(0f, -2f));
        Rigidbody2D puck = puckGo.AddComponent<Rigidbody2D>();
        puck.gravityScale = 0f;
        puckGo.AddComponent<CircleCollider2D>().radius = AirHockeyLevel.PuckR;
        Art(puckGo.transform, "Art", "hockey_puck", 6, false);
        Transform player = Child(root.transform, "PlayerMallet", AirHockeyLevel.PlayerHome).transform;
        Art(player, "Art", "hockey_mallet", 7, false).localScale = Vector3.one * 1.15f;
        Transform catMallet = Child(root.transform, "CatMallet", AirHockeyLevel.CatHome).transform;
        Art(catMallet, "Art", "hockey_paw_mallet", 7, false);

        Transform bigPaw = Child(root.transform, "BigPaw", new Vector2(0f, 5.2f)).transform;
        bigPaw.localScale = new Vector3(1.3f, 1.1f, 1f);   // copre tutta la porta larga
        Art(bigPaw, "Art", "hockey_big_paw", 9, false);
        Rigidbody2D pawRb = bigPaw.gameObject.AddComponent<Rigidbody2D>();
        pawRb.bodyType = RigidbodyType2D.Kinematic;
        BoxCollider2D block = bigPaw.gameObject.AddComponent<BoxCollider2D>();
        block.offset = new Vector2(0f, -0.3f);
        block.size = new Vector2(1.9f, 0.5f);

        Transform face = Child(root.transform, "CatFace", new Vector2(-1.85f, 3.3f)).transform;
        face.localScale = Vector3.one * 0.28f;
        Art(face, "Face", "cat_big_face", 3, false);
        var pupils = new List<Transform>();
        foreach (float sx in new[] { -1f, 1f })
        {
            Transform pupil = Child(face, "PupilPivot", new Vector2(sx * 0.55f, 0.25f)).transform;
            Art(pupil, "Art", "cat_pupil", 4, false);
            pupils.Add(pupil);
        }
        GameObject anger = Art(face, "Anger", "anger", 5, false, new Vector2(1.2f, 1.2f)).gameObject;
        anger.transform.localScale = Vector3.one * 3f;
        TextMesh score = Label(root.transform, new Vector2(0.9f, 3.35f), 20, "", 0.055f);
        score.color = Color.white;

        SetField(level, "puck", puck);
        SetField(level, "playerMallet", player);
        SetField(level, "catMallet", catMallet);
        SetField(level, "bigPaw", bigPaw);
        SetField(level, "bigPawBlock", block);
        SetField(level, "catFace", face);
        SetArray(level, "pupils", pupils.ToArray());
        SetField(level, "anger", anger);
        SetField(level, "score", score);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.4f, 2.35f)));
        return SaveLevel(root, "AirHockey");
    }

    // ---------- Slam Dunk ----------

    static GameObject BuildSlamDunkLevel()
    {
        var root = new GameObject("Level_Basket");
        SlamDunkLevel level = root.AddComponent<SlamDunkLevel>();
        Art(root.transform, "Gym", "dunk_gym", -20, false);
        Transform hoop = Child(root.transform, "Hoop", SlamDunkLevel.HoopSpots[0]).transform;
        Art(hoop, "Back", "dunk_hoop_back", 3, false);
        Transform net = Child(hoop, "Net", Vector2.zero).transform;
        Art(net, "Front", "dunk_hoop_front", 7, false);
        Transform ball = Child(root.transform, "Ball", SlamDunkLevel.BallStart).transform;
        SpriteRenderer ballArt = Art(ball, "Art", "dunk_ball", 12, false).GetComponent<SpriteRenderer>();
        TextMesh swish = Label(root.transform, new Vector2(0f, 3f), 30, "SWISH!", 0.1f);
        swish.color = new Color(1f, 0.55f, 0.15f);
        TextMesh counter = Label(root.transform, new Vector2(0f, -4.45f), 20, "", 0.05f);
        TextMesh prompt = Label(root.transform, new Vector2(0f, -2.3f), 20, "", 0.06f);
        prompt.color = Color.white;

        SetField(level, "ball", ball);
        SetField(level, "ballArt", ballArt);
        SetField(level, "hoop", hoop);
        SetField(level, "net", net);
        SetField(level, "swishText", swish);
        SetField(level, "counter", counter);
        SetField(level, "prompt", prompt);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-1.0f, -0.3f)));
        return SaveLevel(root, "Basket");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V39C = new Dictionary<string, string[]>
    {
        { "box_pusher", new[] { "A crate pushed into a corner is stuck forever: plan the order (UNDO is free). Watch the shadow on the right, a paw is coming!",
                                "Left crate: push it up, then left into the corner. Middle crate: push it right twice, then up twice from below. Bottom crate: push it right twice, then up once." } },
        { "hungry_snake", new[] { "You can't turn back on yourself. Plan wide turns and stay away from the walls as you speed up.",
                                  "Swipe toward each apple, circle around your body. Leave space: the faster you go, the earlier you must turn." } },
        { "pinball", new[] { "Every bumper you hit stays lit. When all 3 are lit, the cat opens its mouth.",
                             "Flip when the ball reaches the middle of the flipper. With all 3 lit, aim straight up the center into the cat's mouth." } },
        { "air_hockey", new[] { "The cat is slow and gets distracted: aim for the corners of its wide goal when it looks away.",
                                "At 2-0 the cat blocks its goal with a big paw: tap near the paw to boop it away, then shoot quickly while it sulks!" } },
        { "slam_dunk", new[] { "Swipe straight toward the hoop. A medium-fast flick is better than a super-fast one.",
                               "Flick from the ball toward the rim with a quick, medium swipe. For the moving hoop, swipe where it WILL be." } },
    };
}
#endif
