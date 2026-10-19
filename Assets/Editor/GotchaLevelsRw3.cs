#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli rifatti da zero nella revisione (gruppo 3).</summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Fold It (cartella Cookie2): la mappa da piegare ----------

    static GameObject BuildPaperFoldLevel()
    {
        var root = new GameObject("Level_Cookie2");
        PaperFoldLevel level = root.AddComponent<PaperFoldLevel>();
        Bar(root.transform, "Table", Vector2.zero, new Vector2(12f, 16f), new Color(0.55f, 0.37f, 0.24f), -20);
        for (int i = 0; i < 9; i++)
            Bar(root.transform, "Plank", new Vector2(0f, -4.6f + i * 1.15f), new Vector2(12f, 0.03f), new Color(0.42f, 0.27f, 0.17f), -19);
        Vector2 center = new Vector2(0f, -0.75f);
        const float c = 1.25f;
        // ombra del foglio
        Bar(root.transform, "Shadow", center + new Vector2(0.08f, -0.1f), new Vector2(5.0f, 2.5f), new Color(0f, 0f, 0f, 0.25f), -2);
        Bar(root.transform, "ShadowTop", center + new Vector2(0.08f, 1.85f), new Vector2(2.5f, 1.25f), new Color(0f, 0f, 0f, 0.25f), -2);
        Art(root.transform, "Paper", "fold_paper_center", 1, false, center);

        string[] fronts = { "fold_flap_left", "fold_flap_right", "fold_flap_top" };
        string[] backs = { "fold_back_left", "fold_back_right", "fold_back_top" };
        Vector2[] hinges = { new Vector2(-c, 0f), new Vector2(c, 0f), new Vector2(0f, c) };   // nel sistema del pannello centrale
        var pivots = new Transform[3];
        var frontGos = new GameObject[3];
        var backGos = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            Transform pivot = Child(root.transform, "Flap" + i, center + hinges[i]).transform;
            frontGos[i] = Art(pivot, "Front", fronts[i], 5, false, -hinges[i]).gameObject;
            // il retro è disegnato già "atterrato": lo specchio del contenitore annulla quello della piega
            Transform holder = Child(pivot, "Back", Vector2.zero).transform;
            holder.localScale = i < 2 ? new Vector3(-1f, 1f, 1f) : new Vector3(1f, -1f, 1f);
            Art(holder, "Art", backs[i], 5, false, -hinges[i]);
            backGos[i] = holder.gameObject;
            pivots[i] = pivot;
        }

        // frecce: "striscia qui"
        var hints = new List<Transform>();
        foreach (var h in new[] { (new Vector2(-1.75f, -0.75f), 0f), (new Vector2(1.75f, -0.75f), 180f), (new Vector2(0f, 1.25f), -90f) })
        {
            Transform t = Child(root.transform, "SwipeHint", h.Item1).transform;
            t.localRotation = Quaternion.Euler(0, 0, h.Item2);
            Art(t, "Art", "ctrl_arrow", 40, false).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.85f);
            hints.Add(t);
        }

        // il sentiero completo (soluzione): dalla bandierina alla X
        var path = new List<Transform>();
        foreach (Vector2 p in new[] { new Vector2(-0.75f, -0.95f), new Vector2(-0.45f, -0.9f), new Vector2(-0.15f, -0.63f), new Vector2(0.25f, -0.62f),
                     new Vector2(0.5f, -0.5f), new Vector2(0.62f, -0.3f), new Vector2(0.625f, 0f), new Vector2(0.58f, 0.2f), new Vector2(0.62f, 0.45f), new Vector2(0.65f, 0.72f) })
            path.Add(Child(root.transform, "Walk", center + p).transform);
        Transform walker = Child(root.transform, "Walker", path[0].localPosition).transform;
        walker.localScale = Vector3.one * 0.28f;
        Art(walker, "Art", "mouse", 62);
        Transform glow = Child(root.transform, "XGlow", center + new Vector2(0.65f, 0.72f)).transform;
        Art(glow, "Art", "fold_x_glow", 60, false);

        // pulsante UNFOLD
        GameObject btn = Child(root.transform, "Unfold", new Vector2(0f, -3.1f));
        Transform visual = Child(btn.transform, "Visual", Vector2.zero).transform;
        Art(visual, "Art", "fold_button", 20, false);
        Fit(Label(visual, new Vector2(0f, 0.02f), 21, "UNFOLD", 0.06f), 1.7f, 0.5f);
        btn.AddComponent<BoxCollider2D>().size = new Vector2(2.3f, 1.0f);
        TapButton unfold = btn.AddComponent<TapButton>();
        unfold.id = "unfold";
        unfold.visual = visual;

        SetArray(level, "pivots", pivots);
        SetArray(level, "fronts", frontGos);
        SetArray(level, "backs", backGos);
        SetArray(level, "swipeHints", hints.ToArray());
        SetField(level, "unfoldButton", unfold);
        SetField(level, "xGlow", glow);
        SetField(level, "walker", walker);
        SetArray(level, "walkPath", path.ToArray());
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0f, 2.75f)));
        return SaveLevel(root, "Cookie2");
    }
}
#endif
