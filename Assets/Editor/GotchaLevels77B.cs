#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Levels;

/// <summary>
/// Versione 0.77, livello 10 della roadmap (gruppo B): Half Full. La stanza grigia, l'omino pessimista (corpo, testa
/// triste e testa felice, schizzo d'acqua, scintille), il tavolino col bicchiere (fondo, acqua "plain" tinta, vetro
/// davanti, schizzo quando trabocca) e la brocca, il fumetto "HALF EMPTY!" (bubble_box 9-slice) e il cartellino
/// "STAY FULL" (9-slice) col chiodo. Ogni parola è un oggetto a sé (TextMesh + BoxCollider2D sul genitore): le misure di
/// fumetto, cartellino, collider e zone di rilascio le calcola il livello dalle larghezze dei testi. La festa è quella
/// comune (RewardRig). Disegni nuovi (prefisso v77_b_) in DoodleSpritesV77B. Niente Easter egg e niente Toni.
/// L'elenco dei livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 10: Half Full ----------

    static GameObject BuildV77HalfFull()
    {
        var root = new GameObject("Level_V77HalfFull");
        V77HalfFullLevel level = root.AddComponent<V77HalfFullLevel>();
        Transform r = root.transform;
        Art(r, "Room", "v77_b_room_bg", -20, false);

        // ---- l'omino (origine ai piedi) ----
        Transform man = Child(r, "Man", new Vector2(-1.35f, -3.55f)).transform;
        man.localScale = Vector3.one * 1.05f;
        Art(man, "Body", "v77_b_man_body", 6, false);
        Transform head = Child(man, "Head", new Vector2(0f, 2.05f)).transform;
        Transform headSad = Art(head, "Sad", "v77_b_man_head_sad", 7, false);
        Transform headHappy = Art(head, "Happy", "v77_b_man_head_happy", 7, false);
        headHappy.gameObject.SetActive(false);
        BoxCollider2D manCol = HW_Area(man, "ManArea", new Vector2(1.5f, 2.7f), new Vector2(0f, 1.3f));
        Transform manSplash = HW_Obj(man, "Splash", new Vector2(0f, 2.75f), "v77_b_splash", 9, 1.1f);
        manSplash.gameObject.SetActive(false);
        var sparkles = new List<Transform>();
        foreach (Vector2 at in new[] { new Vector2(-0.75f, 2.55f), new Vector2(0.75f, 2.6f), new Vector2(-0.95f, 1.75f), new Vector2(0.95f, 1.8f), new Vector2(0f, 3.0f) })
        {
            Transform s = HW_Obj(man, "Sparkle", at, "sparkle_star", 12, 0.7f);
            s.gameObject.SetActive(false);
            sparkles.Add(s);
        }

        // ---- il tavolino, il bicchiere e la brocca ----
        Art(Child(r, "Table", new Vector2(1.0f, -2.2f)).transform, "Art", "v77_b_table", 3, false);
        Transform glass = Child(r, "Glass", new Vector2(0.55f, -2.2f)).transform;
        Art(glass, "Back", "v77_b_glass_back", 5, false);
        SpriteRenderer water = Bar(glass, "Water", new Vector2(0f, 0.48f), new Vector2(0.47f, 0.42f), new Color(0.56f, 0.83f, 0.95f, 0.85f), 6);
        Art(glass, "Front", "v77_b_glass_front", 7, false);
        Transform splash = HW_Obj(glass, "Splash", new Vector2(0f, 0.95f), "v77_b_splash", 8);
        splash.gameObject.SetActive(false);
        BoxCollider2D glassCol = HW_Box(glass, new Vector2(0.95f, 1.3f), new Vector2(0f, 0.5f));
        Transform jug = HW_Obj(r, "Jug", new Vector2(1.6f, -2.2f), "v77_b_jug", 5);
        BoxCollider2D jugCol = HW_Box(jug, new Vector2(1.1f, 1.4f), new Vector2(0f, 0.6f));

        // ---- il fumetto "HALF EMPTY!" (la misura la calcola il livello) ----
        Transform bubble = Child(r, "Bubble", new Vector2(-0.3f, 0.05f)).transform;
        SpriteRenderer bubbleArt = Art(bubble, "Art", "bubble_box", 28, false).GetComponent<SpriteRenderer>();
        bubbleArt.drawMode = SpriteDrawMode.Sliced;
        bubbleArt.size = new Vector2(3.1f, 1.17f);
        TextMesh half = V77B_Word(r, "WordHalf", "HALF", new Vector2(-1.2f, 0.05f), true);
        TextMesh emptyWord = V77B_Word(r, "WordEmpty", "EMPTY", new Vector2(0.0f, 0.05f), true);
        TextMesh bang = V77B_Word(r, "WordBang", "!", new Vector2(0.85f, 0.05f), true);
        TextMesh gap = V77B_Word(r, "WordGap", "_____", new Vector2(0.0f, 0.05f), false);
        gap.color = new Color(gap.color.r, gap.color.g, gap.color.b, 0f);

        // ---- il cartellino "STAY FULL" ----
        Transform sign = Child(r, "Sign", new Vector2(0.95f, 1.45f)).transform;
        SpriteRenderer plate = Art(sign, "Plate", "v77_b_sign", 26, false).GetComponent<SpriteRenderer>();
        plate.drawMode = SpriteDrawMode.Sliced;
        plate.size = new Vector2(2.6f, 0.8f);
        BoxCollider2D signCol = HW_Box(sign, new Vector2(2.6f, 0.8f));
        Transform nail = HW_Obj(sign, "Nail", new Vector2(0f, 0.78f), "v77_b_sign_nail", 25);
        TextMesh stay = V77B_Word(r, "WordStay", "STAY", new Vector2(0.45f, 1.45f), true);
        TextMesh fullWord = V77B_Word(r, "WordFull", "FULL", new Vector2(1.5f, 1.45f), true);

        Transform floor = Child(r, "FloorMark", new Vector2(0f, -4.15f)).transform;

        HW_Goal(r, "MAKE THE PESSIMIST HAPPY.");
        TextMesh info = HW_Toast(r, new Vector2(0f, 2.62f), out Transform infoHolder);
        SetField(level, "man", man);
        SetField(level, "manArea", manCol);
        SetField(level, "manHead", head);
        SetField(level, "headSad", headSad);
        SetField(level, "headHappy", headHappy);
        SetField(level, "manSplash", manSplash);
        SetArray(level, "sparkles", sparkles.ToArray());
        SetField(level, "glass", glass);
        SetField(level, "glassArea", glassCol);
        SetField(level, "water", water.transform);
        SetField(level, "splash", splash);
        SetField(level, "jug", jug);
        SetField(level, "jugArea", jugCol);
        SetField(level, "bubble", bubble);
        SetField(level, "bubbleArt", bubbleArt);
        SetField(level, "sign", sign);
        SetField(level, "signPlate", plate);
        SetField(level, "signArea", signCol);
        SetField(level, "signNail", nail);
        SetField(level, "floorMark", floor);
        SetField(level, "wordHalf", half);
        SetField(level, "wordEmpty", emptyWord);
        SetField(level, "wordBang", bang);
        SetField(level, "wordGap", gap);
        SetField(level, "wordStay", stay);
        SetField(level, "wordFull", fullWord);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r));
        return SaveLevel(root, "V77HalfFull");
    }

    /// <summary>
    /// Una parola come oggetto a sé: genitore (posizione, collider) e TextMesh figlio col font dei titoli, senza
    /// TextMeshFit (la larghezza vera serve al livello per impaginare). Tutte nello stesso stile: FULL non si distingue.
    /// </summary>
    static TextMesh V77B_Word(Transform root, string name, string value, Vector2 pos, bool collider)
    {
        Transform holder = Child(root, name, pos).transform;
        TextMesh t = Label(holder, Vector2.zero, 30, value, 0.05f, false);
        t.color = new Color(0.17f, 0.13f, 0.2f);
        if (collider) HW_Box(holder, new Vector2(value.Length * 0.3f + 0.35f, 0.75f));
        return t;
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V77B = new Dictionary<string, string[]>
    {
        { "half_full", new[] { "Read the sign on the wall.",
            "Replace EMPTY with the word FULL from the sign." } },
    };
}
#endif
