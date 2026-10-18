#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli della versione 0.29 (80, 85, 88, 98).</summary>
public static partial class GotchaProjectBuilder
{
    /// <summary>Sprite scalato in modo che il lato più lungo misuri "size" unità.</summary>
    static Transform Fitted(Transform parent, string name, string sprite, Vector2 pos, float size, int order)
    {
        Transform h = Child(parent, name, pos).transform;
        Vector3 b = sprites[sprite].bounds.size;
        h.localScale = Vector3.one * (size / Mathf.Max(b.x, b.y));
        Transform art = Art(h, "Art", sprite, order, false);
        art.localPosition = -(Vector3)(Vector2)sprites[sprite].bounds.center;   // il disegno centrato sul suo riquadro
        return h;
    }

    // cappellino in testa al topolino (coordinate di Fitted: testa del disegno "mouse" in -0.08, 0.5 → -0.08, 0.35;
    // il cappellino largo 0.75 ha la base 0.23 sotto il suo centro)
    static readonly Vector2 MouseHatAt = new Vector2(-0.08f, 0.58f);

    // ---------- 80: dov'è il topolino? ----------

    static GameObject BuildWhereMouseLevel()
    {
        var root = new GameObject("Level_WhereMouse");
        WhereMouseLevel level = root.AddComponent<WhereMouseLevel>();
        Bar(root.transform, "Back", Vector2.zero, new Vector2(12f, 16f), new Color(0.15f, 0.3f, 0.2f), -30);
        Transform crowd = Child(root.transform, "Crowd", new Vector2(0, -0.3f)).transform;
        Bar(crowd, "Grass", Vector2.zero, new Vector2(5.4f, 7.2f), new Color(0.55f, 0.82f, 0.45f), -20);
        string[] critters = { "cat", "fish", "apple", "bird_up", "wasp", "roach", "sheep", "cheese_wedge", "carrot", "heart", "snow_buddy", "mole_party", "bell", "cherry", "piranha", "bat_sleep" };
        var rnd = new System.Random(80);
        var decoys = new List<Transform>();
        Transform target = null;
        int n = 0;
        for (float y = -3.4f; y <= 3.4f; y += 0.42f)
            for (float x = -2.5f; x <= 2.5f; x += 0.42f)
            {
                Vector2 at = new Vector2(x + (float)(rnd.NextDouble() - 0.5) * 0.18f, y + (float)(rnd.NextDouble() - 0.5) * 0.18f);
                int kind = rnd.Next(100);
                n++;
                if (target == null && n == 77)
                {
                    target = Fitted(crowd, "Target", "mouse", at, 0.34f, 5);
                    Fitted(target, "Hat", "tiny_hat_red", MouseHatAt, 0.75f, 6);
                    continue;
                }
                if (kind < 14)
                {
                    Transform m = Fitted(crowd, "Mouse", "mouse", at, 0.34f, 5);
                    if (kind < 8) Fitted(m, "Hat", "tiny_hat_blue", MouseHatAt, 0.75f, 6);
                    decoys.Add(m);
                    continue;
                }
                string s = critters[rnd.Next(critters.Length)];
                Transform c = Fitted(crowd, s, s, at, 0.32f + (float)rnd.NextDouble() * 0.08f, 3 + rnd.Next(2));
                c.localRotation = Quaternion.Euler(0, 0, (float)(rnd.NextDouble() - 0.5) * 30f);
            }
        Transform time = GaugeBar(root.transform, "Time", new Vector2(-0.8f, 3.75f), new Vector2(2.2f, 0.16f), new Color(0f, 0f, 0f, 0.5f), new Color(1f, 0.85f, 0.3f), 40);
        TextMesh info = Label(root.transform, new Vector2(1.6f, 3.75f), 40, "", 0.04f);
        info.color = Color.white;

        SetField(level, "crowd", crowd);
        SetField(level, "target", target);
        SetArray(level, "decoys", decoys.ToArray());
        SetField(level, "timeBar", time);
        SetField(level, "info", info);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0, 3.2f)));
        return SaveLevel(root, "WhereMouse");
    }

    // ---------- 85: il castello di carte ----------

    static GameObject BuildCardHouseLevel()
    {
        var root = new GameObject("Level_CardHouse");
        CardHouseLevel level = root.AddComponent<CardHouseLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);
        Transform window = Art(root.transform, "Window", "window_frame", -10, false, new Vector2(1.55f, 1.7f));
        Transform curtain = Art(root.transform, "Curtain", "curtain", -9, true, new Vector2(1.95f, 2.85f));
        Bar(root.transform, "Table", new Vector2(-0.3f, -2.75f), new Vector2(3.6f, 0.18f), new Color(0.55f, 0.36f, 0.22f), 2);
        Bar(root.transform, "Leg", new Vector2(-0.3f, -3.6f), new Vector2(0.25f, 1.6f), new Color(0.45f, 0.3f, 0.18f), 1);
        Transform tower = Child(root.transform, "Tower", new Vector2(-0.3f, -2.66f)).transform;
        var cards = new List<Transform>();
        void Card(Vector2 at, float rot)
        {
            Transform c = Art(tower, "Card", "playing_card", 5 + cards.Count, false, at);
            c.localRotation = Quaternion.Euler(0, 0, rot);
            cards.Add(c);
        }
        const float lean = 18f;
        Card(new Vector2(-0.62f, 0.36f), -lean); Card(new Vector2(-0.38f, 0.36f), lean);
        Card(new Vector2(0.38f, 0.36f), -lean); Card(new Vector2(0.62f, 0.36f), lean);
        Card(new Vector2(0f, 0.74f), 90f);
        Card(new Vector2(-0.12f, 1.12f), -lean); Card(new Vector2(0.12f, 1.12f), lean);
        Card(new Vector2(0f, 1.5f), 90f);
        Card(new Vector2(0f, 1.88f), 0f);
        Transform sway = GaugeBar(root.transform, "Sway", new Vector2(0.2f, 3.75f), new Vector2(2.2f, 0.16f), new Color(0.17f, 0.13f, 0.2f, 0.6f), new Color(1f, 0.45f, 0.3f), 20);
        Label(root.transform, new Vector2(-1.35f, 3.75f), 21, "WOBBLE", 0.035f);
        TextMesh counter = Label(root.transform, new Vector2(-1.9f, 3.1f), 21, "", 0.05f);

        SetField(level, "tower", tower);
        SetArray(level, "cards", cards.ToArray());
        SetField(level, "curtain", curtain);
        SetField(level, "swayBar", sway);
        SetField(level, "counter", counter);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.6f, 1.6f)));
        return SaveLevel(root, "CardHouse");
    }

    // ---------- 88: la giostra ----------

    static GameObject BuildCarouselLevel()
    {
        var root = new GameObject("Level_Carousel");
        CarouselLevel level = root.AddComponent<CarouselLevel>();
        Art(root.transform, "Party", "party_bg", -20, false);
        Transform disk = Child(root.transform, "Disk", new Vector2(0, -0.8f)).transform;
        disk.localScale = Vector3.one * 1.25f;
        Art(disk, "Art", "carousel_top", 2, false);
        Transform cat = Child(disk, "Cat", new Vector2(0f, 1.35f)).transform;
        cat.localScale = Vector3.one * 0.3f;
        Art(cat, "Face", "cat_big_face", 4, false);
        foreach (float sx in new[] { -1f, 1f }) Art(cat, "Pupil", "cat_pupil", 5, false, new Vector2(sx * 0.55f, 0.25f));
        GameObject dizzy = Art(cat, "Dizzy", "dizzy_eyes", 6, false, new Vector2(0f, 0.25f)).gameObject;
        dizzy.transform.localScale = Vector3.one * 1.6f;
        TextMesh counter = Label(root.transform, new Vector2(0, 2.4f), 12, "", 0.08f);
        TextMesh prompt = Label(root.transform, new Vector2(0, -3.6f), 12, "", 0.07f);

        SetField(level, "disk", disk);
        SetField(level, "cat", cat);
        SetField(level, "counter", counter);
        SetField(level, "prompt", prompt);
        SetField(level, "dizzyEyes", dizzy);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0, 3.3f)));
        return SaveLevel(root, "Carousel");
    }

    // ---------- 98: il cronometro cieco ----------

    static GameObject BuildStopwatchLevel()
    {
        var root = new GameObject("Level_Stopwatch");
        StopwatchLevel level = root.AddComponent<StopwatchLevel>();
        Bar(root.transform, "Wall", Vector2.zero, new Vector2(12f, 16f), new Color(0.3f, 0.6f, 0.75f), -20);
        Transform watch = Child(root.transform, "Watch", new Vector2(0, 0.6f)).transform;
        watch.localScale = Vector3.one * 1.2f;
        Art(watch, "Art", "stopwatch", 2, false);
        Transform hand = Child(watch, "Hand", Vector2.zero).transform;
        Bar(hand, "Needle", new Vector2(0, 0.5f), new Vector2(0.05f, 1.0f), new Color(0.9f, 0.2f, 0.2f), 4);
        Art(watch, "Pin", "led", 5, false).localScale = Vector3.one * 0.3f;
        TextMesh display = Fit(Label(watch, new Vector2(0, -0.55f), 6, "0.00", 0.09f), 1.5f, 0.6f);
        TextMesh goal = Label(root.transform, new Vector2(0, 3.2f), 12, "", 0.07f);
        goal.color = Color.white;
        TextMesh tries = Label(root.transform, new Vector2(0, -1.6f), 12, "", 0.05f);
        tries.color = Color.white;
        TapButton button = SwitchButton(root.transform, "Button", "big_button", new Vector2(0, -2.9f));
        button.transform.localScale = Vector3.one * 0.8f;

        SetField(level, "button", button);
        SetField(level, "display", display);
        SetField(level, "goal", goal);
        SetField(level, "tries", tries);
        SetField(level, "hand", hand);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0, 2.4f)));
        return SaveLevel(root, "Stopwatch");
    }
}
#endif
