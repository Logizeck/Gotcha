#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;
using Gotcha.Levels.V64;

/// <summary>
/// Versione 0.85, livello 82 della roadmap (gruppo A): The Dishonest Hourglass. Lo sfondo (v85_a_bg: studio al tramonto
/// con la scrivania), la clessidra sulla scrivania (radice al collo con il collider generoso; dentro, il perno che gira
/// con il vetro davanti, la sabbia di sopra e di sotto ritagliate con ClipArt/SpriteWindowClip dalle due finestre figlie
/// del perno — niente SpriteMask — e il filo di sabbia "plain" tinto), il display digitale appeso alla parete (cifre come
/// testo, bagliore verde e quattro scintille nascoste), l'etichetta della velocità a destra della clessidra (nascosta),
/// il pulsante verde ADD TIME (scritta come testo, goccia di sudore nascosta, collider generoso) con il suo fumetto
/// (Bubble/SpeechBubble, si adatta al testo), la riga dell'obiettivo e la scritta di commento (aiuti di Halloween) e la
/// festa comune (RewardRig, sopra la clessidra). Ogni TextMesh ha il suo Fit. Display e scritte non sono figli del perno:
/// non ruotano mai. Niente Easter egg e niente Toni. L'elenco dei livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 82: The Dishonest Hourglass ----------

    static readonly Color V85ASandColor = new Color(0.95f, 0.77f, 0.42f);
    static readonly Color V85ALed = new Color(1f, 0.3f, 0.22f);

    static GameObject BuildV85DishonestHourglass()
    {
        var root = new GameObject("Level_V85DishonestHourglass");
        V85DishonestHourglassLevel level = root.AddComponent<V85DishonestHourglassLevel>();
        Transform r = root.transform;
        Art(r, "Room", "v85_a_bg", -20, false);

        // ---- la clessidra (collo in y 0.15, scala 0.8: il tappo di sotto poggia sulla scrivania) ----
        const float hs = 0.8f;
        float neckY = DoodleSprites.V85ADeskY + DoodleSprites.V85AHalfH * hs;
        Transform glass = Child(r, "Hourglass", new Vector2(0f, neckY)).transform;
        glass.localScale = Vector3.one * hs;
        Transform spin = Child(glass, "Spin", Vector2.zero).transform;
        float bulbH = DoodleSprites.V85ABulbH;
        float top01 = DishonestHourglassLogic.StartSeconds / DishonestHourglassLogic.Capacity;
        Transform topWin = Child(spin, "TopWindow", Vector2.zero).transform;
        topWin.localScale = new Vector3(1f, bulbH * Mathf.Pow(top01, 0.75f), 1f);
        Transform botWin = Child(spin, "BottomWindow", new Vector2(0f, -bulbH)).transform;
        botWin.localScale = new Vector3(1f, bulbH * 0.97f * Mathf.Pow(1f - top01, 1.1f), 1f);
        ClipArt(spin, "SandTop", "v85_a_sand_top", 10, topWin, new Vector2(0f, 0.5f), new Vector2(2f, 1f), Color.white);
        ClipArt(spin, "SandBottom", "v85_a_sand_bot", 10, botWin, new Vector2(0f, 0.5f), new Vector2(2f, 1f), Color.white);
        Transform stream = Child(spin, "Stream", Vector2.zero).transform;
        SpriteRenderer streamArt = Bar(stream, "Art", new Vector2(0f, -0.5f), new Vector2(0.05f, 1f), V85ASandColor, 10);
        stream.localScale = new Vector3(1f, bulbH - botWin.localScale.y, 1f);
        Art(spin, "Glass", "v85_a_glass", 11, false);
        BoxCollider2D glassArea = HW_Box(glass, new Vector2(2.5f, 4.0f));   // clessidra larga 2.1 x 3.9: area generosa

        // ---- il display appeso alla parete ----
        const float dy = 2.3f;
        SpriteRenderer dGlow = Art(r, "DisplayGlow", "glow", 19, false, new Vector2(0f, dy)).GetComponent<SpriteRenderer>();
        dGlow.transform.localScale = Vector3.one * 3.4f;
        dGlow.color = new Color(0.4f, 1f, 0.45f, 0f);
        Transform display = Child(r, "Display", new Vector2(0f, dy)).transform;
        Art(display, "Panel", "v85_a_display", 20, false);
        TextMesh digits = Label(display, new Vector2(0f, -0.02f), 22, "00:08", 0.12f);
        digits.color = V85ALed;
        Fit(digits, 2.3f, 0.62f);
        BoxCollider2D displayArea = HW_Box(display, new Vector2(3.1f, 1.25f));
        var sparkles = new List<Object>();
        Vector2[] sp = { new Vector2(-1.55f, 0.45f), new Vector2(1.55f, 0.4f), new Vector2(-1.35f, -0.5f), new Vector2(1.4f, -0.45f) };
        foreach (Vector2 p in sp)
        {
            Transform s = HW_Obj(display, "Sparkle", p, "sparkle_star", 24, 0.3f);
            s.gameObject.SetActive(false);
            sparkles.Add(s);
        }

        // ---- l'etichetta della velocità (sulla parete a destra della clessidra, nascosta) ----
        Transform speed = Child(r, "Speed", new Vector2(1.72f, 1.05f)).transform;
        Bar(speed, "Back", Vector2.zero, new Vector2(1.45f, 0.42f), new Color(0.1f, 0.06f, 0.16f, 0.8f), 25);
        TextMesh speedText = Label(speed, Vector2.zero, 26, "SPEED x1.5", 0.035f);
        speedText.color = new Color(1f, 0.55f, 0.3f);
        Fit(speedText, 1.3f, 0.32f);
        speed.gameObject.SetActive(false);

        // ---- il pulsante ADD TIME (sul pavimento) ----
        Transform button = Child(r, "AddButton", new Vector2(0f, -3.45f)).transform;
        Transform body = Child(button, "Body", Vector2.zero).transform;
        Art(body, "Key", "v85_a_button", 30, false);
        TextMesh addLabel = Label(body, new Vector2(0.3f, 0.07f), 31, "ADD TIME", 0.075f);
        addLabel.color = Color.white;
        Fit(addLabel, 1.6f, 0.5f);
        BoxCollider2D addArea = HW_Box(button, new Vector2(3.3f, 1.5f));   // tasto largo 2.7: area generosa
        Transform sweat = HW_Obj(button, "Sweat", new Vector2(1.2f, 0.55f), "sweat", 32, 1.6f);
        sweat.gameObject.SetActive(false);
        SpeechBubble bubble = Bubble(r, new Vector2(-0.35f, -2.8f));   // la codina punta al pulsante, cresce in su e a destra

        HW_Goal(r, "GET 10 MORE SECONDS.");
        TextMesh info = HW_Toast(r, new Vector2(0f, -4.55f), out Transform infoHolder);
        SetField(level, "hourglass", glass);
        SetField(level, "hourSpin", spin);
        SetField(level, "glassArea", glassArea);
        SetField(level, "topWindow", topWin);
        SetField(level, "bottomWindow", botWin);
        SetField(level, "stream", stream);
        SetField(level, "streamArt", streamArt);
        SetFloat(level, "bulbH", bulbH);
        SetField(level, "display", display);
        SetField(level, "displayText", digits);
        SetField(level, "speedText", speedText);
        SetField(level, "speedHolder", speed);
        SetField(level, "displayArea", displayArea);
        SetField(level, "displayGlow", dGlow);
        SetArray(level, "sparkles", sparkles.ToArray());
        SetField(level, "addButton", button);
        SetField(level, "addBody", body);
        SetField(level, "addArea", addArea);
        SetField(level, "sweat", sweat);
        SetField(level, "buttonBubble", bubble);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, 0.2f)));
        return SaveLevel(root, "V85DishonestHourglass");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V85A = new Dictionary<string, string[]>
    {
        { "dishonest_hourglass", new[] { "An hourglass works both ways.",
            "Turn your phone upside down." } },
    };
}
#endif
