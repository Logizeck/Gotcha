#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;
using Gotcha.Levels.V64;

/// <summary>
/// Versione 0.87, livello 129 della roadmap (gruppo B): The Fake Trophy. Lo sfondo (v87_b_bg: sala delle premiazioni
/// spenta, pavimento di assi), l'insegna (radice al centro; dentro Body il disegno senza scritta, le due serie di
/// lampadine accese che il livello alterna e l'alone che resta quando WIN si stacca; collider generoso), la parola WIN
/// come oggetto INDIPENDENTE (un testo giallo con la sua ombra, Fit dentro il pannello dell'insegna; il bersaglio per il
/// dito lo calcola il livello dai limiti disegnati del testo), il piedistallo, la coppa (radice alla base; dentro Body la
/// coppa grigia e quella d'oro; collider generoso), il bagliore dell'oro, la stellina, lo "clink", l'omino (radice ai
/// piedi; Body con le pose deluso e a braccia alzate; collider generoso) con la sua ombra, l'anello della pressione lunga
/// (12 puntini), il fumetto dell'omino (SpeechBubble, si adatta al testo e resta nello schermo), la riga dell'obiettivo,
/// la scritta di commento e la festa comune (RewardRig, sopra l'insegna). Ogni TextMesh ha il suo Fit (WIN e la sua
/// ombra, HW_Goal, HW_Toast, RewardRig); il fumetto si adatta da solo. Disegni nuovi (prefisso v87_b_) in
/// DoodleSpritesV87B. Niente Easter egg e niente Toni. L'elenco dei livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 129: The Fake Trophy ----------

    static GameObject BuildV87FakeTrophy()
    {
        var root = new GameObject("Level_V87FakeTrophy");
        V87FakeTrophyLevel level = root.AddComponent<V87FakeTrophyLevel>();
        Transform r = root.transform;
        Art(r, "Hall", "v87_b_bg", -20, false);
        Art(r, "Pedestal", "v87_b_pedestal", 2, false);
        const float floorY = DoodleSprites.V87BFloorY;

        // ---- l'insegna (radice al centro) ----
        Transform sign = Child(r, "Sign", new Vector2(DoodleSprites.V87BSignX, DoodleSprites.V87BSignY)).transform;
        Transform sBody = Child(sign, "Body", Vector2.zero).transform;
        Art(sBody, "Board", "v87_b_sign", 3, false);
        SpriteRenderer bulbsA = Art(sBody, "BulbsA", "v87_b_bulbs_a", 4, false).GetComponent<SpriteRenderer>();
        SpriteRenderer bulbsB = Art(sBody, "BulbsB", "v87_b_bulbs_b", 4, false).GetComponent<SpriteRenderer>();
        bulbsB.enabled = false;
        Transform ghost = Art(sBody, "WordGhost", "v87_b_word_ghost", 4, false);
        ghost.gameObject.SetActive(false);
        BoxCollider2D signCol = HW_Box(sign, new Vector2(DoodleSprites.V87BSignHalfW * 2f + 0.2f, DoodleSprites.V87BSignHalfH * 2f + 0.2f));

        // ---- la parola WIN: oggetto indipendente (testo), sopra tutto il resto della scena ----
        Transform word = Child(r, "Word", new Vector2(DoodleSprites.V87BSignX, DoodleSprites.V87BSignY)).transform;
        TextMesh shadow = Label(word, new Vector2(0.06f, -0.06f), 20, "WIN", 0.15f);
        shadow.color = new Color(0.55f, 0.08f, 0.2f);
        Fit(shadow, 2.8f, 1.0f);
        TextMesh wordText = Label(word, Vector2.zero, 21, "WIN", 0.15f);
        wordText.color = new Color(1f, 0.89f, 0.36f);
        Fit(wordText, 2.8f, 1.0f);

        // ---- la coppa (radice alla base, sul piedistallo) ----
        Transform trophy = Child(r, "Trophy", new Vector2(DoodleSprites.V87BPedestalX, DoodleSprites.V87BPedestalTop)).transform;
        Transform tBody = Child(trophy, "Body", Vector2.zero).transform;
        Transform grey = Art(tBody, "Grey", "v87_b_trophy_grey", 5, false);
        Transform gold = Art(tBody, "Gold", "v87_b_trophy_gold", 5, false);
        gold.gameObject.SetActive(false);
        BoxCollider2D trophyCol = HW_Box(trophy, new Vector2(1.5f, 1.7f), new Vector2(0f, 0.7f));   // coppa larga 1.2 e alta 1.35: area generosa
        SpriteRenderer goldGlow = Art(r, "GoldGlow", "glow", 9, false, new Vector2(DoodleSprites.V87BPedestalX, DoodleSprites.V87BPedestalTop + DoodleSprites.V87BBowlY)).GetComponent<SpriteRenderer>();
        goldGlow.color = new Color(1f, 0.9f, 0.45f, 0.9f);
        goldGlow.gameObject.SetActive(false);
        SpriteRenderer sparkle = Art(r, "Sparkle", "sparkle_star", 10, false, new Vector2(DoodleSprites.V87BPedestalX + 0.45f, DoodleSprites.V87BPedestalTop + 1.3f)).GetComponent<SpriteRenderer>();
        sparkle.gameObject.SetActive(false);
        SpriteRenderer clink = Art(r, "Clink", "v87_b_clink", 10, false, new Vector2(DoodleSprites.V87BPedestalX, DoodleSprites.V87BPedestalTop + 1f)).GetComponent<SpriteRenderer>();
        clink.gameObject.SetActive(false);

        // ---- l'omino (radice ai piedi) con l'ombra ----
        const float guyX = FakeTrophyLogic.DefaultGuyX;
        Transform gShadow = Art(r, "GuyShadow", "ball_shadow", 6, false, new Vector2(guyX, floorY + 0.02f));
        gShadow.localScale = new Vector3(1.1f, 0.25f, 1f);
        gShadow.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.45f);
        Transform guy = Child(r, "Guy", new Vector2(guyX, floorY)).transform;
        Transform gBody = Child(guy, "Body", Vector2.zero).transform;
        Transform sad = Art(gBody, "Sad", "v87_b_guy_sad", 8, false);
        Transform lift = Art(gBody, "Lift", "v87_b_guy_lift", 8, false);
        lift.gameObject.SetActive(false);
        // area dell'omino (la stessa per il tocco e per la coppa rifiutata): larga 1.5, alta 2.1 dai piedi
        BoxCollider2D guyCol = HW_Box(guy, new Vector2(FakeTrophyLogic.DefaultGuyHalfW * 2f, FakeTrophyLogic.DefaultGuyHalfH * 2f),
            new Vector2(0f, FakeTrophyLogic.DefaultGuyY - floorY));

        // ---- l'anello della pressione lunga: 12 puntini in cerchio (il livello lo porta sul dito) ----
        Transform ring = Child(r, "HoldRing", new Vector2(DoodleSprites.V87BSignX, DoodleSprites.V87BSignY)).transform;
        var dots = new Object[12];
        for (int i = 0; i < dots.Length; i++)
        {
            float a = Mathf.PI * 0.5f - i * 2f * Mathf.PI / dots.Length;   // da mezzogiorno in senso orario
            SpriteRenderer d = Art(ring, "Dot" + i, "v87_b_ring_dot", 30, false, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 0.6f).GetComponent<SpriteRenderer>();
            d.color = new Color(1f, 1f, 1f, 0.35f);
            dots[i] = d;
        }
        ring.gameObject.SetActive(false);

        // ---- il fumetto dell'omino (il livello lo porta accanto alla testa) ----
        SpeechBubble bubble = Bubble(r, new Vector2(guyX + 0.35f, floorY + 2.45f));

        HW_Goal(r, "WIN THE TROPHY.");
        TextMesh info = HW_Toast(r, new Vector2(0f, -4.45f), out Transform infoHolder);
        SetField(level, "sign", sign);
        SetField(level, "signBody", sBody);
        SetField(level, "signArea", signCol);
        SetField(level, "bulbsA", bulbsA);
        SetField(level, "bulbsB", bulbsB);
        SetField(level, "wordGhost", ghost);
        SetField(level, "word", word);
        SetField(level, "wordText", wordText);
        SetField(level, "wordShadow", shadow);
        SetField(level, "trophy", trophy);
        SetField(level, "trophyBody", tBody);
        SetField(level, "trophyGrey", grey);
        SetField(level, "trophyGold", gold);
        SetField(level, "trophyArea", trophyCol);
        SetField(level, "goldGlow", goldGlow);
        SetField(level, "sparkle", sparkle);
        SetField(level, "clinkFx", clink);
        SetField(level, "guy", guy);
        SetField(level, "guyBody", gBody);
        SetField(level, "guySad", sad);
        SetField(level, "guyLift", lift);
        SetField(level, "guyArea", guyCol);
        SetField(level, "ring", ring);
        SetArray(level, "ringDots", dots);
        SetField(level, "guyBubble", bubble);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, DoodleSprites.V87BSignY)));
        return SaveLevel(root, "V87FakeTrophy");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V87B = new Dictionary<string, string[]>
    {
        { "fake_trophy", new[] { "Hold the word on the sign.",
            "Long-press WIN, then drag it onto the trophy." } },
    };
}
#endif
