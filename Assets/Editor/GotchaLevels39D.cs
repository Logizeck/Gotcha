#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli nuovi della versione 0.39 (gruppo D).</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels39D() => new List<LevelDefinition>
    {
        Define("Knock", "knock_knock", "KNOCK KNOCK", "Who's there? Not your fingertip.", BuildKnockLevel()),
        Define("FaceDown", "screen_time", "SCREEN TIME", "He's staring at something bright...", BuildFaceDownLevel()),
        Define("Vase", "broken_vase", "BROKEN VASE", "Quick, before Mom sees!", BuildVaseLevel()),
        Define("Words", "word_salad", "WORD SALAD", "Tap the letters to answer the riddle.", BuildWordsLevel()),
        Define("Chicken", "why_chicken", "WHY THE CHICKEN", "Mom is waiting on the other side.", BuildChickenLevel()),
    };

    // ---------- Knock knock: si bussa sul retro del telefono ----------

    static GameObject BuildKnockLevel()
    {
        var root = new GameObject("Level_Knock");
        KnockLevel level = root.AddComponent<KnockLevel>();
        Art(root.transform, "Alley", "knock_bg", -20, false);
        const float s = 1.15f, frameY = -2.9f + 1.8f * s;
        Transform frame = Art(root.transform, "Frame", "door_frame", -8, false, new Vector2(0f, frameY));
        frame.localScale = Vector3.one * s;
        frame.localPosition = new Vector3(0f, frameY, 0f) + (Vector3)(centers["door_frame"] * s);
        Transform guest = Child(root.transform, "Guest", new Vector2(0.15f, -2.9f)).transform;
        guest.localScale = Vector3.one * 1.0f;
        Art(guest, "Art", "monster", -6, false);

        Transform door = Child(root.transform, "Door", new Vector2(-1.1f * s, frameY)).transform;
        Transform panel = Child(door, "Panel", Vector2.zero).transform;
        panel.localScale = Vector3.one * s;
        Art(panel, "Art", "secret_door", 2, false);
        Transform eye = Child(panel, "Eye", new Vector2(1.1f, 1.05f)).transform;
        Art(eye, "White", "peep_eye", 5, false);
        Transform pupil = Child(eye, "Pupil", Vector2.zero).transform;
        Art(pupil, "Art", "peep_pupil", 6, false);
        Transform sign = Child(panel, "Sign", new Vector2(1.1f, 0.05f)).transform;
        Art(sign, "Art", "knock_sign", 4, false);
        Fit(Label(sign, new Vector2(0f, 0.02f), 5, "SECRET KNOCK ONLY\nknock...  knock-knock", 0.035f), 1.6f, 0.6f);
        var dots = new List<SpriteRenderer>();
        foreach (float x in new[] { 0.75f, 1.3f, 1.55f }) dots.Add(Bar(panel, "Dot", new Vector2(x, -0.55f), new Vector2(0.14f, 0.14f), Color.white, 5));
        var texts = new List<TextMesh>();
        for (int i = 0; i < 3; i++)
        {
            TextMesh t = Label(root.transform, Vector2.zero, 40, "KNOCK!", 0.09f);
            t.color = new Color(1f, 0.9f, 0.4f);
            texts.Add(t);
        }

        SetField(level, "door", door);
        SetField(level, "eye", eye);
        SetField(level, "pupil", pupil);
        SetField(level, "guest", guest);
        SetArray(level, "knockTexts", texts.ToArray());
        SetArray(level, "dots", dots.ToArray());
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.4f, 2.6f)));
        return SaveLevel(root, "Knock");
    }

    // ---------- Screen time: telefono a faccia in giù ----------

    static GameObject BuildFaceDownLevel()
    {
        var root = new GameObject("Level_FaceDown");
        FaceDownLevel level = root.AddComponent<FaceDownLevel>();
        Art(root.transform, "Room", "bedroom_bg", -20, false);
        Art(root.transform, "Blanket", "kid_blanket", -5, false, new Vector2(0f, -1.25f));
        SpriteRenderer dark = Bar(root.transform, "Dark", Vector2.zero, new Vector2(12f, 16f), new Color(0f, 0f, 0.05f, 0.55f), 1);
        dark.enabled = false;
        Transform kidHolder = Child(root.transform, "Kid", new Vector2(0f, -0.45f)).transform;
        SpriteRenderer glow = Art(kidHolder, "Glow", "glow", 2, false).GetComponent<SpriteRenderer>();
        glow.transform.localScale = Vector3.one * 2.4f;
        glow.color = new Color(0.55f, 0.8f, 1f, 0.6f);
        SpriteRenderer kid = Art(kidHolder, "Art", "kid_awake", 3, false).GetComponent<SpriteRenderer>();
        kidHolder.gameObject.AddComponent<CircleCollider2D>().radius = 0.8f;

        GameObject sw = Child(root.transform, "Switch", new Vector2(2.0f, 0.5f));
        SpriteRenderer swArt = Art(sw.transform, "Art", "switch_on", 0, false).GetComponent<SpriteRenderer>();
        sw.AddComponent<BoxCollider2D>().size = new Vector2(1.0f, 1.2f);

        var zzz = new List<Transform>();
        foreach (Vector2 p in new[] { new Vector2(0.55f, 0.2f), new Vector2(0.7f, 0.35f), new Vector2(0.85f, 0.5f) })
            zzz.Add(Art(root.transform, "Zzz", "zzz", 8, false, p));

        SetField(level, "kid", kid);
        SetField(level, "awakeSprite", sprites["kid_awake"]);
        SetField(level, "drowsySprite", sprites["kid_drowsy"]);
        SetField(level, "asleepSprite", sprites["kid_asleep"]);
        SetField(level, "screenGlow", glow);
        SetField(level, "darkness", dark);
        SetField(level, "kidArea", kidHolder.GetComponent<Collider2D>());
        SetField(level, "switchArea", sw.GetComponent<Collider2D>());
        SetField(level, "switchArt", swArt);
        SetField(level, "switchOn", sprites["switch_on"]);
        SetField(level, "switchOff", sprites["switch_off"]);
        SetArray(level, "zzz", zzz.ToArray());
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.9f, 1.25f)));
        return SaveLevel(root, "FaceDown");
    }

    // ---------- Broken vase: puzzle con rotazione ----------

    static GameObject BuildVaseLevel()
    {
        var root = new GameObject("Level_Vase");
        VaseLevel level = root.AddComponent<VaseLevel>();
        Art(root.transform, "Room", "vase_room_bg", -20, false);
        const float s = 0.9f;
        Vector2 shelfAt = new Vector2(-1.4f, 1.4f);
        Art(root.transform, "Shelf", "wall_shelf", 1, false, shelfAt);
        Transform vase = Child(root.transform, "Vase", shelfAt + new Vector2(0f, 0.03f + 1.06f * s)).transform;
        vase.localScale = Vector3.one * s;
        Art(vase, "Art", "fancy_vase", 3, false);

        Vector2 outlineAt = new Vector2(0.75f, 0.75f);
        Transform outlineT = Child(root.transform, "Outline", outlineAt).transform;
        outlineT.localScale = Vector3.one * s;
        SpriteRenderer outline = Art(outlineT, "Art", "vase_outline", 1, false).GetComponent<SpriteRenderer>();
        SpriteRenderer glue = Art(outlineT, "Glue", "vase_glue", 7, false).GetComponent<SpriteRenderer>();

        Vector2[] landings = { new Vector2(-2.0f, -2.95f), new Vector2(-0.55f, -3.3f), new Vector2(1.35f, -3.4f), new Vector2(-1.7f, -4.15f), new Vector2(0.45f, -4.2f) };
        var shards = new List<Transform>();
        var targets = new List<Transform>();
        var lands = new List<Transform>();
        for (int i = 0; i < 5; i++)
        {
            string name = "vase_shard" + i;
            Vector2 c = centers[name];
            targets.Add(Child(root.transform, "Target" + i, outlineAt + c * s).transform);
            lands.Add(Child(root.transform, "Landing" + i, landings[i]).transform);
            Transform h = Child(root.transform, "Shard" + i, landings[i]).transform;
            h.localScale = Vector3.one * s;
            Art(h, "Art", name, 6, false, -c);
            Vector3 b = sprites[name].bounds.size;
            h.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(Mathf.Max(b.x, 0.8f / s), Mathf.Max(b.y, 0.8f / s));
            shards.Add(h);
        }

        Transform cat = Child(root.transform, "Cat", new Vector2(1.6f, -3.2f)).transform;
        cat.localScale = Vector3.one * 0.6f;
        Art(cat, "Art", "cat", 8);
        cat.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(2.8f, 1.9f);

        Transform mom = Child(root.transform, "Mom", new Vector2(4.2f, -2.7f)).transform;
        mom.localScale = Vector3.one * 1.15f;
        Art(mom, "Art", "lady", 9, false);

        SetField(level, "vase", vase);
        SetArray(level, "shards", shards.ToArray());
        SetArray(level, "targets", targets.ToArray());
        SetArray(level, "landings", lands.ToArray());
        SetField(level, "cat", cat);
        SetField(level, "catArea", cat.GetComponent<Collider2D>());
        SetField(level, "impact", Child(root.transform, "Impact", new Vector2(-1.4f, -2.9f)).transform);
        SetField(level, "outline", outline);
        SetField(level, "glue", glue);
        SetField(level, "mom", mom);
        SetField(level, "momStop", Child(root.transform, "MomStop", new Vector2(2.05f, -2.7f)).transform);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0.3f, 3.0f)));
        return SaveLevel(root, "Vase");
    }

    // ---------- Word salad: indovinelli con le tessere ----------

    static GameObject BuildWordsLevel()
    {
        var root = new GameObject("Level_Words");
        WordsLevel level = root.AddComponent<WordsLevel>();
        Art(root.transform, "Study", "words_bg", -20, false);
        Art(root.transform, "Board", "riddle_board", 1, false, new Vector2(0f, 2.3f));
        TextMesh riddle = Fit(Label(root.transform, new Vector2(0.1f, 2.3f), 2, "", 0.06f), 4.3f, 1.5f);
        TextMesh progress = Label(root.transform, new Vector2(-1.5f, 1.0f), 2, "", 0.045f);
        progress.color = Color.white;

        var slots = new List<Transform>();
        for (int i = 0; i < 6; i++)
        {
            Transform sl = Child(root.transform, "Slot" + i, new Vector2(i * 0.8f, -0.35f)).transform;
            Art(sl, "Art", "letter_slot", 1, false);
            slots.Add(sl);
        }
        var tiles = new List<Transform>();
        var letters = new List<TextMesh>();
        for (int i = 0; i < 9; i++)
        {
            Transform t = Child(root.transform, "Tile" + i, new Vector2(0f, -2.1f)).transform;
            Art(t, "Art", "letter_tile", 5, false);
            letters.Add(Fit(Label(t, new Vector2(0f, 0.03f), 6, "A", 0.11f), 0.5f, 0.5f));
            t.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(0.8f, 0.8f);
            tiles.Add(t);
        }

        SetField(level, "riddleText", riddle);
        SetField(level, "progress", progress);
        SetArray(level, "slots", slots.ToArray());
        SetArray(level, "tiles", tiles.ToArray());
        SetArray(level, "tileLetters", letters.ToArray());
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.1f, 0.75f)));
        return SaveLevel(root, "Words");
    }

    // ---------- Why the chicken: attraversa la strada ----------

    static GameObject BuildChickenLevel()
    {
        var root = new GameObject("Level_Chicken");
        ChickenLevel level = root.AddComponent<ChickenLevel>();
        Art(root.transform, "Road", "road_bg", -20, false);
        float[] laneY = { -2.5f, -1.4f, 0.8f, 1.9f };
        float[] speeds = { 1.5f, -2.2f, 2.4f, -1.7f };
        string[][] traffic =
        {
            new[] { "car_red", "car_blue", "car_red" }, new[] { "truck", "car_blue" },
            new[] { "car_blue", "car_red", "car_blue" }, new[] { "truck", "car_red" },
        };
        var rnd = new System.Random(39);
        var lanes = new List<Transform>();
        for (int l = 0; l < laneY.Length; l++)
        {
            Transform lane = Child(root.transform, "Lane" + l, new Vector2(0f, laneY[l])).transform;
            int n = traffic[l].Length;
            float span = 10.6f / n;
            for (int j = 0; j < n; j++)
            {
                float x = -5.3f + span * (j + 0.5f) + (float)(rnd.NextDouble() - 0.5) * 0.6f;
                Transform v = Child(lane, "Vehicle", new Vector2(x, 0.05f)).transform;
                if (speeds[l] < 0f) v.localScale = new Vector3(-1f, 1f, 1f);
                Art(v, "Art", traffic[l][j], 5, false);
            }
            lanes.Add(lane);
        }
        Transform mom = Child(root.transform, "Mom", new Vector2(0f, 3.0f)).transform;
        mom.localScale = Vector3.one * 0.85f;
        Art(mom, "Art", "mother_hen", 8, false);
        Transform chick = Child(root.transform, "Chick", new Vector2(0f, -3.6f)).transform;
        SpriteRenderer chickArt = Art(chick, "Art", "chick", 10, false).GetComponent<SpriteRenderer>();
        var feathers = new List<Transform>();
        for (int i = 0; i < 7; i++) feathers.Add(Art(root.transform, "Feather", "chick_feather", 12, false));
        var lives = new List<SpriteRenderer>();
        for (int i = 0; i < 3; i++)
        {
            Transform h = Art(root.transform, "Life", "heart", 20, false, new Vector2(-2.35f + i * 0.42f, 3.35f));
            h.localScale = Vector3.one * 0.7f;
            lives.Add(h.GetComponent<SpriteRenderer>());
        }
        Transform hearts = Child(root.transform, "Hearts", Vector2.zero).transform;
        foreach (Vector2 p in new[] { new Vector2(-0.25f, 0f), new Vector2(0.2f, 0.25f), new Vector2(0.05f, -0.2f) })
            Art(hearts, "Heart", "heart", 14, false, p).localScale = Vector3.one * 0.6f;
        TextMesh banner = Label(root.transform, new Vector2(0f, -0.3f), 31, "WHY? To get to\nthe other side!", 0.075f);
        banner.color = Color.white;
        Fit(banner, 5.2f, 1.4f);
        Bar(banner.transform, "Back", Vector2.zero, new Vector2(5.6f, 1.6f), new Color(0.17f, 0.13f, 0.2f, 0.75f), 30);

        SetField(level, "chick", chick);
        SetField(level, "chickArt", chickArt);
        SetField(level, "chickSprite", sprites["chick"]);
        SetField(level, "flatSprite", sprites["chick_flat"]);
        SetField(level, "mom", mom);
        SetArray(level, "lanes", lanes.ToArray());
        var so = new UnityEditor.SerializedObject(level);
        UnityEditor.SerializedProperty sp = so.FindProperty("laneSpeeds");
        sp.arraySize = speeds.Length;
        for (int i = 0; i < speeds.Length; i++) sp.GetArrayElementAtIndex(i).floatValue = speeds[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        SetArray(level, "feathers", feathers.ToArray());
        SetArray(level, "lives", lives.ToArray());
        SetField(level, "banner", banner);
        SetField(level, "hearts", hearts);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(1.3f, -4.2f)));
        return SaveLevel(root, "Chicken");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V39D = new Dictionary<string, string[]>
    {
        { "knock_knock", new[] { "Screen taps don't count: the whole phone is the door.", "Knock with your knuckles on the BACK of the phone: knock... (pause) knock-knock." } },
        { "screen_time", new[] { "He won't sleep while the screen is on. And YOU are the screen.", "Put the phone face DOWN for 3 seconds (lullaby, then snoring), then turn it back up." } },
        { "broken_vase", new[] { "Tap a shard to turn it: it only fits the right way up.", "One shard slid under the sleeping cat: tap the cat to make it roll away." } },
        { "word_salad", new[] { "Tap a letter in a box to send it back. Some letters are decoys.", "The last answer is the word itself: W-R-O-N-G." } },
        { "why_chicken", new[] { "Swipe down to hop back, left or right to step aside. The middle grass is safe.", "Tap to hop only when there's a gap, rest on the middle grass, then cross the second road." } },
    };
}
#endif
