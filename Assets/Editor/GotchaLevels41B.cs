#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli della specifica 105-200 (in gioco dal 151), pacchetto B.</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels41B() => new List<LevelDefinition>
    {
        Define("GlassFloor", "glass_floor", "GLASS FLOOR", "Not all glass sounds the same.", BuildS41BGlassFloorLevel()),
        Define("TinyThief", "tiny_thief", "TINY THIEF", "He's much smaller than you think.", BuildS41BTinyThiefLevel()),
        Define("TheSilentAlarm", "the_silent_alarm", "THE SILENT ALARM", "Silence matters.", BuildS41BSilentAlarmLevel()),
        Define("BlackHole", "black_hole", "BLACK HOLE", "It keeps growing... what if it went the other way?", BuildS41BBlackHoleLevel()),
        Define("GravityResigned", "gravity_resigned", "GRAVITY RESIGNED", "Turn his world around.", BuildS41BGravityResignedLevel(), "gravity_resigned"),
        Define("TheCheapMagician", "the_cheap_magician", "THE CHEAP MAGICIAN", "Maybe he isn't a real magician.", BuildS41BCheapMagicianLevel()),
        Define("DontWakeTheBomb", "dont_wake_the_bomb", "DON'T WAKE THE BOMB", "It's a light sleeper.", BuildS41BDontWakeTheBombLevel()),
        Define("TheUpsideDownKing", "the_upside_down_king", "THE UPSIDE-DOWN KING", "He needs a little help reaching the ceiling.", BuildS41BUpsideDownKingLevel()),
    };

    static readonly Color S41B_Dark = new Color(0.1f, 0.08f, 0.14f, 0.6f);

    /// <summary>Riga dell'obiettivo in alto (sotto la barra del titolo).</summary>
    static TextMesh S41B_Goal(Transform root, string text)
    {
        Bar(root, "GoalBack", new Vector2(0f, 3.05f), new Vector2(5.8f, 0.42f), S41B_Dark, 45);
        TextMesh goal = Fit(Label(root, new Vector2(0f, 3.05f), 46, text, 0.04f), 5.5f, 0.36f);
        goal.color = Color.white;
        return goal;
    }

    /// <summary>Cuori dei tentativi in alto a destra.</summary>
    static SpriteRenderer[] S41B_Hearts(Transform root, int n)
    {
        var hearts = new List<SpriteRenderer>();
        for (int i = 0; i < n; i++)
        {
            Transform t = Art(root, "Heart", "heart", 47, false, new Vector2(2.35f - i * 0.45f, 3.5f));
            t.localScale = Vector3.one * 0.75f;
            hearts.Add(t.GetComponent<SpriteRenderer>());
        }
        return hearts.ToArray();
    }

    static BoxCollider2D S41B_Box(Transform t, Vector2 size)
    {
        BoxCollider2D b = t.gameObject.AddComponent<BoxCollider2D>();
        b.size = size;
        return b;
    }

    static CircleCollider2D S41B_Circle(Transform t, float radius)
    {
        CircleCollider2D c = t.gameObject.AddComponent<CircleCollider2D>();
        c.radius = radius;
        return c;
    }

    static TextMesh S41B_Text(Transform parent, Vector2 pos, int order, string text, float size, float w, float h, Color color)
    {
        TextMesh t = Fit(Label(parent, pos, order, text, size), w, h);
        t.color = color;
        return t;
    }

    // ---------- 113: il pavimento di vetro ----------

    static GameObject BuildS41BGlassFloorLevel()
    {
        var root = new GameObject("Level_GlassFloor");
        GlassFloorLevel level = root.AddComponent<GlassFloorLevel>();
        Transform r = root.transform;
        Art(r, "Chasm", "s41b_glass_bg", -20, false);
        var tiles = new List<SpriteRenderer>();
        for (int row = 0; row < GlassFloorLevel.Rows; row++)
            for (int c = 0; c < 2; c++)
            {
                Transform t = Art(r, "Tile" + row + c, "s41b_glass_tile", 2, false, new Vector2(c == 0 ? -0.72f : 0.72f, -2.95f + 0.88f * row));
                S41B_Box(t, new Vector2(1.2f, 0.7f));
                tiles.Add(t.GetComponent<SpriteRenderer>());
            }
        Transform start = Child(r, "Start", new Vector2(0f, -3.25f)).transform;
        Transform goal = Child(r, "Goal", new Vector2(-0.2f, 2.38f)).transform;
        Transform cheese = Fitted(r, "Cheese", "cheese_wedge", new Vector2(0.75f, 2.33f), 0.6f, 9);
        Transform mouse = Fitted(r, "Mouse", "mouse", new Vector2(0f, -3.25f), 0.75f, 10);

        S41B_Goal(r, "CROSS THE GLASS BRIDGE");
        SetArray(level, "hearts", S41B_Hearts(r, 2));
        SetField(level, "bubble", SmallBubble(r, new Vector2(-1.2f, -4.2f)));
        SetField(level, "mouse", mouse);
        SetArray(level, "tiles", tiles.ToArray());
        SetField(level, "crackedSprite", sprites["s41b_glass_cracked"]);
        SetField(level, "shardSprite", sprites["s41b_glass_shard"]);
        SetField(level, "noteSprite", sprites["note"]);
        SetField(level, "startSpot", start);
        SetField(level, "goalSpot", goal);
        SetField(level, "cheese", cheese);
        return SaveLevel(root, "GlassFloor");
    }

    // ---------- 114: il ladro minuscolo ----------

    static GameObject BuildS41BTinyThiefLevel()
    {
        var root = new GameObject("Level_TinyThief");
        TinyThiefLevel level = root.AddComponent<TinyThiefLevel>();
        Transform r = root.transform;
        Transform room = Child(r, "Room", Vector2.zero).transform;
        Art(room, "Bg", "s41b_thief_room", -20, false);
        SpriteRenderer crown = Art(room, "Crown", "s41b_crown", -6, false, new Vector2(0.2f, -1.74f)).GetComponent<SpriteRenderer>();

        Vector2[] hole = { new Vector2(0.2f, -1.82f), new Vector2(0.6f, -1.9f), new Vector2(0.62f, -2.85f), new Vector2(1.25f, -2.6f), new Vector2(1.3f, -1.3f), new Vector2(1.45f, -0.62f), new Vector2(1.45f, -0.42f) };
        Vector2[] window = { new Vector2(0.2f, -1.82f), new Vector2(-0.2f, -1.9f), new Vector2(-0.22f, -2.85f), new Vector2(-1.0f, -2.5f), new Vector2(-1.3f, -1.15f), new Vector2(-0.9f, -0.95f), new Vector2(-0.75f, -0.1f), new Vector2(-0.55f, 0.35f), new Vector2(-0.45f, 0.85f), new Vector2(-0.2f, 1.1f) };
        Transform routes = Child(room, "Routes", Vector2.zero).transform;
        var routeHole = new List<Transform>();
        var routeWindow = new List<Transform>();
        for (int i = 0; i < hole.Length; i++) routeHole.Add(Child(routes, "Hole" + i, hole[i]).transform);
        for (int i = 0; i < window.Length; i++) routeWindow.Add(Child(routes, "Window" + i, window[i]).transform);

        Transform thief = Child(room, "Thief", hole[0]).transform;
        SpriteRenderer thiefArt = Art(thief, "Art", "s41b_thief", -4, false).GetComponent<SpriteRenderer>();
        Transform glint = Art(thief, "Glint", "glow", -3, false, new Vector2(-0.04f, 0.06f));
        glint.localScale = Vector3.one * 0.07f;

        Bar(r, "Tray", new Vector2(2.1f, -4.55f), new Vector2(1.4f, 0.16f), new Color(0.35f, 0.22f, 0.12f), 19);
        Transform basket = Fitted(r, "Basket", "basket", new Vector2(2.1f, -4.0f), 1.0f, 20);
        Transform time = GaugeBar(r, "Time", new Vector2(0.3f, 2.6f), new Vector2(4.4f, 0.14f), new Color(0f, 0f, 0f, 0.5f), new Color(1f, 0.75f, 0.3f), 46);
        S41B_Text(r, new Vector2(-2.35f, 2.6f), 46, "TIME", 0.03f, 0.6f, 0.2f, Color.white);

        S41B_Goal(r, "CATCH THE TINY THIEF");
        SetField(level, "bubble", SmallBubble(r, new Vector2(0f, 1.6f)));
        SetField(level, "room", room);
        SetField(level, "thief", thief);
        SetField(level, "thiefArt", thiefArt);
        SetField(level, "glint", glint.GetComponent<SpriteRenderer>());
        SetArray(level, "routeHole", routeHole.ToArray());
        SetArray(level, "routeWindow", routeWindow.ToArray());
        SetField(level, "basket", basket);
        SetField(level, "crownOnCushion", crown);
        SetField(level, "timeBar", time);
        return SaveLevel(root, "TinyThief");
    }

    // ---------- 115: l'allarme silenzioso ----------

    static GameObject BuildS41BSilentAlarmLevel()
    {
        var root = new GameObject("Level_TheSilentAlarm");
        SilentAlarmLevel level = root.AddComponent<SilentAlarmLevel>();
        Transform r = root.transform;
        Art(r, "Vault", "s41b_vault_bg", -20, false);

        Transform door = Child(r, "Door", new Vector2(-0.35f, 1.45f)).transform;
        Art(door, "Art", "s41b_vault_door", 1, false);
        Art(door, "Screen", "s41b_alarm_screen", 2, false, new Vector2(0f, 0.32f));
        var lamps = new List<SpriteRenderer>();
        for (int i = 0; i < 4; i++)
        {
            Transform l = Art(door, "Lamp" + i, "s41b_dot", 3, false, new Vector2(-0.6f + 0.4f * i, 0.45f));
            l.localScale = Vector3.one * 0.85f;
            lamps.Add(l.GetComponent<SpriteRenderer>());
        }
        TextMesh screen = S41B_Text(door, new Vector2(0f, 0.15f), 3, "ENTER CODE", 0.028f, 1.9f, 0.18f, Color.white);
        var dots = new List<SpriteRenderer>();
        for (int i = 0; i < 4; i++)
        {
            Transform d = Art(door, "Progress" + i, "s41b_dot", 3, false, new Vector2(-0.45f + 0.3f * i, -0.2f));
            d.localScale = Vector3.one * 0.6f;
            dots.Add(d.GetComponent<SpriteRenderer>());
        }

        Color[] colors = { new Color(1f, 0.3f, 0.3f), new Color(0.35f, 0.9f, 0.4f), new Color(0.35f, 0.6f, 1f), new Color(1f, 0.85f, 0.25f) };
        Vector2[] at = { new Vector2(-1.25f, -0.5f), new Vector2(0.55f, -0.5f), new Vector2(-1.25f, -2.2f), new Vector2(0.55f, -2.2f) };
        var buttons = new List<SpriteRenderer>();
        for (int i = 0; i < 4; i++)
        {
            Transform b = Art(r, "Button" + i, "s41b_alarm_button", 3, false, at[i]);
            b.GetComponent<SpriteRenderer>().color = colors[i];
            S41B_Circle(b, 0.66f);
            buttons.Add(b.GetComponent<SpriteRenderer>());
        }

        Art(r, "Track", "s41b_slider_track", 2, false, new Vector2(2.3f, -2.05f));
        Transform knob = Child(r, "Knob", new Vector2(2.3f, -0.6f)).transform;
        Art(knob, "Art", "s41b_slider_knob", 4, false);
        Art(r, "Speaker", "s41b_speaker", 3, false, new Vector2(2.25f, 0.05f));
        SpriteRenderer cross = Art(r, "Muted", "s41b_speaker_x", 4, false, new Vector2(2.45f, 0.05f)).GetComponent<SpriteRenderer>();
        var bars = new List<SpriteRenderer>();
        for (int i = 0; i < 5; i++)
        {
            float h = 0.1f + 0.07f * i;
            bars.Add(Bar(r, "Vol" + i, new Vector2(1.98f + 0.14f * i, 0.42f + h * 0.5f), new Vector2(0.1f, h), Color.white, 3));
        }
        S41B_Text(r, new Vector2(2.3f, -3.98f), 3, "VOLUME", 0.026f, 0.9f, 0.2f, Color.white);
        Transform siren = Art(r, "Siren", "s41b_siren", 5, false, new Vector2(1.95f, 2.1f));
        SpriteRenderer flash = Bar(r, "Flash", Vector2.zero, new Vector2(12f, 16f), new Color(1f, 0f, 0f, 0f), 40);

        S41B_Goal(r, "DO NOT MAKE A SOUND");
        SetArray(level, "hearts", S41B_Hearts(r, 3));
        SetField(level, "bubble", SmallBubble(r, new Vector2(1.3f, 0.9f)));
        SetArray(level, "buttons", buttons.ToArray());
        SetArray(level, "screenLamps", lamps.ToArray());
        SetArray(level, "progressDots", dots.ToArray());
        SetField(level, "screenText", screen);
        SetField(level, "knob", knob);
        SetArray(level, "volumeBars", bars.ToArray());
        SetField(level, "speakerCross", cross);
        SetField(level, "alarmFlash", flash);
        SetField(level, "siren", siren);
        SetField(level, "vaultDoor", door);
        return SaveLevel(root, "TheSilentAlarm");
    }

    // ---------- 116: il buco nero ----------

    static GameObject BuildS41BBlackHoleLevel()
    {
        var root = new GameObject("Level_BlackHole");
        BlackHoleLevel level = root.AddComponent<BlackHoleLevel>();
        Transform r = root.transform;
        Art(r, "Space", "s41b_space_bg", -20, false);
        Transform hole = Child(r, "Hole", new Vector2(0f, 0.1f)).transform;
        Art(hole, "Art", "s41b_black_hole", 5, false);
        Transform swirl = Art(hole, "Swirl", "s41b_swirl", 6, false);
        var planets = new List<Transform>();
        for (int i = 0; i < 3; i++)
        {
            Transform p = Child(r, "Planet" + i, new Vector2(1.5f + i * 0.5f, 0.1f)).transform;
            Art(p, "Art", "s41b_planet" + i, 4, false);
            planets.Add(p);
        }
        SpriteRenderer white = Bar(r, "Whiteout", Vector2.zero, new Vector2(12f, 16f), new Color(1f, 1f, 1f, 0f), 60);
        TextMesh escaped = S41B_Text(r, new Vector2(0f, 0.3f), 61, "YOU ESCAPED", 0.09f, 5.2f, 1.2f, new Color(0.17f, 0.13f, 0.2f));

        S41B_Goal(r, "STOP THE BLACK HOLE");
        SetArray(level, "hearts", S41B_Hearts(r, 3));
        SetField(level, "bubble", SmallBubble(r, new Vector2(0f, -3.3f)));
        SetField(level, "hole", hole);
        SetField(level, "swirl", swirl);
        SetArray(level, "planets", planets.ToArray());
        SetField(level, "whiteout", white);
        SetField(level, "escapedText", escaped);
        return SaveLevel(root, "BlackHole");
    }

    // ---------- 117: la gravità si è licenziata ----------

    static GameObject BuildS41BGravityResignedLevel()
    {
        var root = new GameObject("Level_GravityResigned");
        GravityResignedLevel level = root.AddComponent<GravityResignedLevel>();
        Transform r = root.transform;
        Art(r, "Room", "s41b_gr_room", -20, false);
        Transform cushion = Art(r, "Cushion", "s41b_gr_cushion", 2, false, new Vector2(0f, -3.35f));
        Transform clock = Art(r, "TimeClock", "s41b_gr_clock", 3, false, new Vector2(2.05f, -1.2f));
        Transform man = Child(r, "Man", new Vector2(0f, 2.05f)).transform;
        Art(man, "Art", "s41b_gr_man", 6, false);

        Transform hold = GaugeBar(r, "Hold", new Vector2(0.4f, -4.15f), new Vector2(3.4f, 0.16f), new Color(0f, 0f, 0f, 0.45f), new Color(0.5f, 0.85f, 1f), 6);
        S41B_Text(r, new Vector2(-1.85f, -4.15f), 6, "FLIP", 0.03f, 0.7f, 0.22f, Color.white);

        Transform flip = Child(r, "FlipButton", new Vector2(-1.9f, -1.7f)).transform;
        Transform fa = Art(flip, "Art", "s41b_arrow_btn", 7, false);
        fa.localRotation = Quaternion.Euler(0f, 0f, 90f);
        S41B_Text(flip, new Vector2(0f, -0.6f), 7, "HOLD TO FLIP", 0.026f, 1.4f, 0.2f, new Color(0.17f, 0.13f, 0.2f));
        CircleCollider2D flipArea = S41B_Circle(flip, 0.5f);

        Transform worker = Child(r, "Gravity", new Vector2(1.3f, -2.55f)).transform;
        Art(worker, "Art", "s41b_gr_worker", 8, false);
        Transform letter = Child(r, "Letter", new Vector2(0f, 0.2f)).transform;
        Art(letter, "Art", "s41b_gr_letter", 50, false);
        S41B_Text(letter, new Vector2(0.05f, 0.25f), 51, "I RESIGN.\nEFFECTIVE\nIMMEDIATELY.\n\n- G.", 0.035f, 1.3f, 1.3f, new Color(0.17f, 0.13f, 0.2f));
        Transform sign = Child(r, "Sign", new Vector2(0f, 1.55f)).transform;
        Art(sign, "Art", "s41b_gr_sign", 9, false);
        S41B_Text(sign, new Vector2(0f, 0.02f), 10, "WELL, THAT'S\nAWKWARD", 0.045f, 2.25f, 0.75f, new Color(0.17f, 0.13f, 0.2f));

        TextMesh goal = S41B_Goal(r, "GET HIM DOWN");
        SetField(level, "goal", goal);
        SetArray(level, "hearts", S41B_Hearts(r, 3));
        SetField(level, "bubble", SmallBubble(r, new Vector2(-0.9f, 0.9f)));
        SetField(level, "man", man);
        SetField(level, "holdRing", hold);
        SetField(level, "flipButton", flip.gameObject);
        SetField(level, "flipArea", flipArea);
        SetField(level, "worker", worker);
        SetField(level, "timeClock", clock);
        SetField(level, "letter", letter);
        SetField(level, "sign", sign);
        SetField(level, "cushion", cushion);
        return SaveLevel(root, "GravityResigned");
    }

    // ---------- 118: il mago da quattro soldi ----------

    static GameObject BuildS41BCheapMagicianLevel()
    {
        var root = new GameObject("Level_TheCheapMagician");
        CheapMagicianLevel level = root.AddComponent<CheapMagicianLevel>();
        Transform r = root.transform;
        Art(r, "Stage", "s41b_stage_bg", -20, false);
        Transform magician = Child(r, "Magician", new Vector2(-1.6f, -0.95f)).transform;
        Art(magician, "Art", "s41b_magician", 4, false);
        Transform wand = Child(magician, "Wand", new Vector2(0.45f, 0.15f)).transform;
        Art(wand, "Art", "s41b_wand", 5, false);

        Vector2 hatAt = new Vector2(1.0f, -1.9f);
        Transform hatBack = Child(r, "HatBack", hatAt).transform;
        Art(hatBack, "Art", "s41b_tophat_back", 5, false);
        Transform rabbit = Child(r, "Rabbit", hatAt + new Vector2(0f, 0.55f)).transform;
        SpriteRenderer rabbitArt = Art(rabbit, "Art", "s41b_rabbit", 6, false).GetComponent<SpriteRenderer>();
        Transform hat = Child(r, "Hat", hatAt).transform;
        Art(hat, "Art", "s41b_tophat", 7, false);

        string[] spells = { "ABRACADABRA", "HOCUS POCUS", "SIM SALA BIM" };
        var buttons = new List<Collider2D>();
        for (int i = 0; i < 3; i++)
        {
            Transform b = Art(r, "Spell" + i, "s41b_spell_btn", 10, false, new Vector2(-1.9f + 1.9f * i, -3.75f));
            S41B_Text(b, Vector2.zero, 11, spells[i], 0.032f, 1.5f, 0.4f, Color.white);
            buttons.Add(S41B_Box(b, new Vector2(1.75f, 0.7f)));
        }
        SpriteRenderer puff = Art(r, "Puff", "puff", 12, false).GetComponent<SpriteRenderer>();
        var sparkles = new List<SpriteRenderer>();
        Vector2[] sp = { new Vector2(0.4f, -1.0f), new Vector2(1.6f, -0.9f), new Vector2(1.0f, -0.5f), new Vector2(0.2f, -1.7f), new Vector2(1.8f, -1.6f) };
        foreach (Vector2 s in sp) sparkles.Add(Art(r, "Sparkle", "sparkle_star", 12, false, s).GetComponent<SpriteRenderer>());

        S41B_Goal(r, "MAKE THE RABBIT VANISH");
        SetArray(level, "hearts", S41B_Hearts(r, 3));
        SetField(level, "bubble", SmallBubble(r, new Vector2(-0.6f, 0.6f)));
        SetField(level, "rabbit", rabbit);
        SetField(level, "rabbitArt", rabbitArt);
        SetField(level, "rabbitSprite", sprites["s41b_rabbit"]);
        SetField(level, "potatoSprite", sprites["s41b_potato"]);
        SetField(level, "hat", hat);
        SetField(level, "hatBack", hatBack);
        SetField(level, "magician", magician);
        SetField(level, "wand", wand);
        SetArray(level, "spellButtons", buttons.ToArray());
        SetField(level, "puff", puff);
        SetArray(level, "sparkles", sparkles.ToArray());
        return SaveLevel(root, "TheCheapMagician");
    }

    // ---------- 119: non svegliare la bomba ----------

    static GameObject BuildS41BDontWakeTheBombLevel()
    {
        var root = new GameObject("Level_DontWakeTheBomb");
        DontWakeTheBombLevel level = root.AddComponent<DontWakeTheBombLevel>();
        Transform r = root.transform;
        Art(r, "Bedroom", "s41b_bomb_bg", -20, false);
        Art(r, "Pillow", "s41b_pillow", 1, false, new Vector2(0f, -2.45f));
        Transform bomb = Child(r, "Bomb", new Vector2(0f, -1.15f)).transform;
        SpriteRenderer bombArt = Art(bomb, "Art", "s41b_bomb_sleep", 3, false).GetComponent<SpriteRenderer>();
        CircleCollider2D area = S41B_Circle(bomb, 0.95f);
        Transform socket = Child(bomb, "Socket", new Vector2(0.42f, 1.08f)).transform;
        var zzz = new List<Transform>();
        for (int i = 0; i < 3; i++) zzz.Add(Art(bomb, "Z" + i, "zzz", 4, false, new Vector2(1f, 0.3f + i * 0.4f)));
        LineRenderer fuse = Line(r, "Fuse", 0.07f, new Color(0.79f, 0.64f, 0.42f), 5);
        Transform tip = Child(r, "Tip", new Vector2(0.62f, 0.25f)).transform;
        Art(tip, "Art", "s41b_fuse_tip", 6, false);

        Transform wake = GaugeBar(r, "Wake", new Vector2(0.4f, 2.55f), new Vector2(3.6f, 0.2f), new Color(0f, 0f, 0f, 0.5f), Color.white, 46);
        S41B_Text(r, new Vector2(-1.85f, 2.55f), 46, "WAKE", 0.032f, 0.75f, 0.22f, Color.white);
        Transform pull = GaugeBar(r, "Pull", new Vector2(0.4f, -4.1f), new Vector2(3.6f, 0.2f), new Color(0f, 0f, 0f, 0.5f), new Color(0.5f, 0.9f, 0.45f), 6);
        S41B_Text(r, new Vector2(-1.85f, -4.1f), 6, "FUSE", 0.032f, 0.75f, 0.22f, Color.white);

        SpriteRenderer boom = Art(r, "Boom", "boom", 20, false, new Vector2(0f, -1.15f)).GetComponent<SpriteRenderer>();
        var confetti = new List<SpriteRenderer>();
        for (int i = 0; i < 16; i++)
        {
            Transform c = Art(r, "Confetto" + i, "confetti", 21, false, new Vector2(0f, -1.15f));
            c.localScale = Vector3.one * 1.6f;
            confetti.Add(c.GetComponent<SpriteRenderer>());
        }

        S41B_Goal(r, "DON'T WAKE THE BOMB");
        SetField(level, "bubble", SmallBubble(r, new Vector2(-1.1f, 0.9f)));
        SetField(level, "bomb", bomb);
        SetField(level, "bombArt", bombArt);
        SetField(level, "asleepSprite", sprites["s41b_bomb_sleep"]);
        SetField(level, "drowsySprite", sprites["s41b_bomb_drowsy"]);
        SetField(level, "awakeSprite", sprites["s41b_bomb_awake"]);
        SetField(level, "socket", socket);
        SetField(level, "tip", tip);
        SetField(level, "fuse", fuse);
        SetField(level, "bombArea", area);
        SetField(level, "wakeBar", wake);
        SetField(level, "wakeFill", wake.GetChild(0).GetComponent<SpriteRenderer>());
        SetField(level, "pullBar", pull);
        SetArray(level, "zzz", zzz.ToArray());
        SetField(level, "boom", boom);
        SetArray(level, "confetti", confetti.ToArray());
        return SaveLevel(root, "DontWakeTheBomb");
    }

    // ---------- 120: il re sottosopra ----------

    static GameObject BuildS41BUpsideDownKingLevel()
    {
        var root = new GameObject("Level_TheUpsideDownKing");
        UpsideDownKingLevel level = root.AddComponent<UpsideDownKingLevel>();
        Transform r = root.transform;
        Art(r, "Hall", "s41b_throne_bg", -20, false);
        Art(r, "Throne", "s41b_throne", 2, false, new Vector2(-1.75f, -1.9f));
        Transform seat = Child(r, "Seat", new Vector2(-1.75f, -1.75f)).transform;
        Transform ladder = Child(r, "Ladder", new Vector2(-0.35f, -2.9f)).transform;
        Art(ladder, "Art", "s41b_ladder", 4, false);
        Transform king = Child(r, "King", new Vector2(-1.75f, -1.75f)).transform;
        Art(king, "Art", "s41b_king", 6, false);
        Transform head = Child(king, "Head", new Vector2(0f, 0.72f)).transform;
        Transform crown = Fitted(r, "Crown", "s41b_crown", new Vector2(1.45f, 2.36f), 0.75f, 7);
        crown.localRotation = Quaternion.Euler(0f, 0f, 180f);
        SpriteRenderer dust = Art(r, "Dust", "puff", 8, false).GetComponent<SpriteRenderer>();

        Transform arrows = Child(r, "Arrows", Vector2.zero).transform;
        Transform left = Child(arrows, "Left", new Vector2(-2.1f, -4.15f)).transform;
        Art(left, "Art", "s41b_arrow_btn", 30, false).localScale = new Vector3(-1f, 1f, 1f);
        Transform right = Child(arrows, "Right", new Vector2(2.1f, -4.15f)).transform;
        Art(right, "Art", "s41b_arrow_btn", 30, false);

        S41B_Goal(r, "GET THE CROWN BACK");
        SetArray(level, "hearts", S41B_Hearts(r, 3));
        SetField(level, "bubble", SmallBubble(r, new Vector2(-0.8f, 0.4f)));
        SetField(level, "ladder", ladder);
        SetField(level, "king", king);
        SetField(level, "crown", crown);
        SetField(level, "throneSeat", seat);
        SetField(level, "kingHead", head);
        SetField(level, "arrows", arrows.gameObject);
        SetField(level, "leftArrow", S41B_Circle(left, 0.5f));
        SetField(level, "rightArrow", S41B_Circle(right, 0.5f));
        SetField(level, "dust", dust);
        return SaveLevel(root, "TheUpsideDownKing");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V41B = new Dictionary<string, string[]>
    {
        { "glass_floor", new[] { "Listen before stepping.", "The low-pitched tiles are safe." } },
        { "tiny_thief", new[] { "Zoom in and look around the furniture.", "Trap him with the basket near the exit." } },
        { "the_silent_alarm", new[] { "The device volume is part of the puzzle.", "Set media volume to zero before entering the sequence." } },
        { "black_hole", new[] { "Make the black hole smaller.", "Pinch two fingers together on it until it collapses into a dot." } },
        { "gravity_resigned", new[] { "Your phone can change which way is down.", "Rotate the device upside down and hold it." } },
        { "the_cheap_magician", new[] { "You don't need magic.", "Hide the rabbit under the hat." } },
        { "dont_wake_the_bomb", new[] { "Slow down your fingers.", "Pull the fuse very slowly." } },
        { "the_upside_down_king", new[] { "Move the ladder by tilting.", "Position the ladder and drag the king onto it." } },
    };
}
#endif
