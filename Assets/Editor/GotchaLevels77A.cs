#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.77, livello 8 della roadmap (gruppo A): Love the Bug. Lo sfondo (v77_a_bg), la finestra della app di
/// fantasia (v77_a_window, titolo "MY APP v0.1" e scritta di stato come testi), i blocchi colorati dei difetti (barre
/// "plain" tinte), la barra di caricamento (barra "plain" dentro la vaschetta disegnata), la faccina rotta e quella
/// riparata, otto insetti già pronti (v77_a_bug + guance v77_a_bug_cheeks), il cuore-misuratore (due "heart"), gli
/// sbuffi dello sdoppiamento ("puff"), l'insetto-cuore e i cuoricini del finale, la riga dell'obiettivo e la scritta di
/// commento (aiuti di Halloween: HW_Goal, HW_Obj, HW_Toast) e la festa comune (RewardRig). Niente Easter egg e niente
/// Toni. L'elenco dei livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 8: Love the Bug ----------

    static GameObject BuildV77LoveTheBug()
    {
        var root = new GameObject("Level_V77LoveTheBug");
        V77LoveTheBugLevel level = root.AddComponent<V77LoveTheBugLevel>();
        Transform r = root.transform;
        Art(r, "Wall", "v77_a_bg", -20, false);
        Art(r, "Window", "v77_a_window", -10, false);

        // ---- titolo e stato della app (testi a parte: si traducono e cambiano) ----
        TextMesh title = Label(r, new Vector2(-0.3f, DoodleSprites.V77ATitleY), -8, "MY APP v0.1", 0.045f);
        title.color = Color.white;
        Fit(title, 2.6f, 0.36f);
        TextMesh status = Label(r, new Vector2(0f, DoodleSprites.V77ABarY - 0.42f), -8, "LOADING... 42%   BUGS: 1", 0.032f);
        status.color = new Color(0.33f, 0.36f, 0.48f);
        Fit(status, 3.9f, 0.3f);

        // ---- faccina-icona della app ----
        Transform smileyBroken = HW_Obj(r, "SmileyBroken", new Vector2(0f, 0.15f), "v77_a_smiley_broken", -6, 1.15f);
        Transform smileyHappy = HW_Obj(r, "SmileyHappy", new Vector2(0f, 0.15f), "v77_a_smiley_happy", -6, 1.15f);
        smileyHappy.gameObject.SetActive(false);

        // ---- blocchi dei difetti (si accendono man mano che gli insetti aumentano) ----
        Color magenta = new Color(1f, 0.3f, 0.8f), cyan = new Color(0.2f, 0.88f, 1f), yellow = new Color(1f, 0.88f, 0.2f),
              ink = new Color(0.2f, 0.15f, 0.3f), lime = new Color(0.45f, 1f, 0.4f);
        float[][] blocks =
        {
            new[] { -1.2f, 1.3f, 0.9f, 0.16f }, new[] { 1.1f, -1.6f, 0.7f, 0.22f }, new[] { 0.9f, 1.75f, 1.2f, 0.1f },
            new[] { -1.4f, -0.7f, 0.5f, 0.3f }, new[] { 0.6f, 0.9f, 0.35f, 0.35f }, new[] { -0.3f, -2.3f, 1.6f, 0.12f },
            new[] { 1.5f, 0.3f, 0.45f, 0.18f }, new[] { -0.9f, 0.55f, 0.8f, 0.09f }, new[] { 0.2f, -1.05f, 1.0f, 0.2f },
            new[] { -1.6f, 1.85f, 0.4f, 0.4f },
        };
        Color[] blockColors = { magenta, cyan, yellow, ink, lime, magenta, cyan, ink, yellow, lime };
        Transform glitchRoot = Child(r, "Glitches", Vector2.zero).transform;
        var glitches = new Object[blocks.Length];
        for (int i = 0; i < blocks.Length; i++)
        {
            float[] b = blocks[i];
            Color c = blockColors[i];
            c.a = 0.75f;
            SpriteRenderer g = Bar(glitchRoot, "Glitch" + i, new Vector2(b[0], b[1]), new Vector2(b[2], b[3]), c, -4);
            g.gameObject.SetActive(i < 2);
            glitches[i] = g;
        }

        // ---- barra di caricamento (storta: ruota il contenitore) ----
        Transform barHolder = Child(r, "LoadingBar", new Vector2(0f, DoodleSprites.V77ABarY)).transform;
        float barW = DoodleSprites.V77ABarHalfW * 2f;
        SpriteRenderer barFill = Bar(barHolder, "Fill", Vector2.zero, new Vector2(barW * 0.42f, DoodleSprites.V77ABarHalfH * 2f), new Color(0.36f, 0.82f, 0.45f), -7);
        barFill.transform.localPosition = new Vector3(-barW * 0.5f + barW * 0.21f, 0f, 0f);

        // ---- gli otto insetti (solo il primo acceso) ----
        int n = 8;   // = LoveTheBugLogic.MaxBugs
        var bugs = new Object[n];
        var flips = new Object[n];
        var arts = new Object[n];
        var cheeks = new Object[n];
        var smallHearts = new Object[n];
        Transform bugRoot = Child(r, "Bugs", Vector2.zero).transform;
        for (int i = 0; i < n; i++)
        {
            Transform bug = Child(bugRoot, "Bug" + i, new Vector2(-0.95f, -1.35f)).transform;
            Transform flip = Child(bug, "Flip", Vector2.zero).transform;
            SpriteRenderer art = Art(flip, "Art", "v77_a_bug", 10 + i * 2, false).GetComponent<SpriteRenderer>();
            SpriteRenderer cheek = Art(flip, "Cheeks", "v77_a_bug_cheeks", 11 + i * 2, false).GetComponent<SpriteRenderer>();
            cheek.gameObject.SetActive(false);
            bug.gameObject.SetActive(i == 0);
            bugs[i] = bug;
            flips[i] = flip;
            arts[i] = art;
            cheeks[i] = cheek;
            Transform h = HW_Obj(r, "SmallHeart" + i, Vector2.zero, "heart", 42 + i, 1f);
            h.gameObject.SetActive(false);
            smallHearts[i] = h;
        }

        // ---- sbuffi dello sdoppiamento ----
        var pops = new Object[4];
        for (int i = 0; i < pops.Length; i++)
        {
            SpriteRenderer p = Art(r, "Pop" + i, "puff", 30, false).GetComponent<SpriteRenderer>();
            p.gameObject.SetActive(false);
            pops[i] = p;
        }

        // ---- cuore-misuratore: cuore scuro dietro, cuore rosa che cresce dentro ----
        Transform meter = Child(r, "Meter", new Vector2(0f, 0f)).transform;
        Transform meterBack = HW_Obj(meter, "Back", Vector2.zero, "heart", 34, 1.25f);
        meterBack.GetComponentInChildren<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.35f);   // cuore "fantasma": quello pieno cresce dentro
        Transform meterFill = HW_Obj(meter, "Fill", Vector2.zero, "heart", 35, 1f);
        meterFill.localScale = Vector3.one * 0.001f;
        meter.localScale = Vector3.one * 1.5f;
        meter.gameObject.SetActive(false);

        // ---- l'insetto diventato cuore ----
        Transform bigHeart = HW_Obj(r, "BigHeart", Vector2.zero, "v77_a_bug_heart", 45, 1.1f);
        bigHeart.gameObject.SetActive(false);

        HW_Goal(r, "GET RID OF THE BUG.");
        TextMesh info = HW_Toast(r, new Vector2(0f, -4.5f), out Transform infoHolder);
        SetArray(level, "bugs", bugs);
        SetArray(level, "bugFlips", flips);
        SetArray(level, "bugArts", arts);
        SetArray(level, "bugCheeks", cheeks);
        SetArray(level, "pops", pops);
        SetField(level, "meter", meter);
        SetField(level, "meterFill", meterFill);
        SetArray(level, "glitches", glitches);
        SetField(level, "barFill", barFill.transform);
        SetField(level, "barFillArt", barFill);
        SetField(level, "smileyBroken", smileyBroken);
        SetField(level, "smileyHappy", smileyHappy);
        SetField(level, "titleText", title);
        SetField(level, "statusText", status);
        SetField(level, "bigHeart", bigHeart);
        SetArray(level, "smallHearts", smallHearts);
        SetFloat(level, "barWidth", barW);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(root.transform));
        return SaveLevel(root, "V77LoveTheBug");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V77A = new Dictionary<string, string[]>
    {
        { "love_the_bug", new[] { "Be gentle. Be patient.",
            "Press and hold a bug for 2 seconds." } },
    };
}
#endif
