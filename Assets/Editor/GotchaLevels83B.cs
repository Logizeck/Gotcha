#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.83, livello 54 della roadmap (gruppo B): Luck Hack. Lo sfondo (v83_b_bg: parete verde col ferro di cavallo
/// capovolto e il quadrifoglio, tavolo da gioco col panno verde, pavimento scuro), la moneta (radice al centro della
/// moneta ferma con il suo collider generoso; dentro, il perno che si schiaccia con le due facce HEADS/TAILS, ognuna
/// con la sua parola come testo, e la moneta di taglio), l'ombra sul tavolo, il bagliore e la scintilla; il pulsante
/// FLIP (tasto v83_b_button, scritta come testo, collider generoso), la scritta "Flipping..." sul bordo del tavolo
/// (nascosta), la targhetta del punteggio sulla parete, la riga dell'obiettivo, la scritta di commento (aiuti di
/// Halloween) e la festa comune (RewardRig, sopra la moneta). Ogni TextMesh ha il suo Fit. Niente Easter egg e niente
/// Toni. L'elenco dei livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 54: Luck Hack ----------

    static readonly Color V83BCoinInk = new Color(0.45f, 0.27f, 0.05f);
    static readonly Color V83BCueInk = new Color(1f, 0.95f, 0.8f);
    static readonly Color V83BScoreInk = new Color(1f, 0.83f, 0.36f);

    static GameObject BuildV83LuckHack()
    {
        var root = new GameObject("Level_V83LuckHack");
        V83LuckHackLevel level = root.AddComponent<V83LuckHackLevel>();
        Transform r = root.transform;
        Art(r, "Room", "v83_b_bg", -20, false);

        // ---- la moneta (ferma sul panno, TAILS in su) ----
        float coinY = DoodleSprites.V83BTableY + 0.3f;
        SpriteRenderer shadow = Art(r, "CoinShadow", "ball_shadow", 8, false, new Vector2(0f, coinY - 0.57f)).GetComponent<SpriteRenderer>();
        shadow.transform.localScale = new Vector3(1.1f, 0.28f, 1f);
        SpriteRenderer glow = Art(r, "CoinGlow", "glow", 10, false, new Vector2(0f, coinY)).GetComponent<SpriteRenderer>();
        glow.transform.localScale = Vector3.one * 2.4f;
        glow.color = new Color(1f, 0.88f, 0.35f, 0f);
        Transform coin = Child(r, "Coin", new Vector2(0f, coinY)).transform;
        Transform spin = Child(coin, "Spin", Vector2.zero).transform;
        Transform tails = HW_Obj(spin, "Tails", Vector2.zero, "v83_b_coin_tails", 12);
        TextMesh tailsWord = Label(tails, new Vector2(0f, -0.3f), 13, "TAILS", 0.035f);
        tailsWord.color = V83BCoinInk;
        Fit(tailsWord, 0.78f, 0.24f);
        Transform heads = HW_Obj(spin, "Heads", Vector2.zero, "v83_b_coin_heads", 12);
        TextMesh headsWord = Label(heads, new Vector2(0f, -0.3f), 13, "HEADS", 0.035f);
        headsWord.color = V83BCoinInk;
        Fit(headsWord, 0.78f, 0.24f);
        heads.gameObject.SetActive(false);
        Transform edge = HW_Obj(spin, "Edge", Vector2.zero, "v83_b_coin_edge", 12);
        edge.gameObject.SetActive(false);
        BoxCollider2D coinArea = HW_Box(coin, new Vector2(1.7f, 1.7f));   // moneta larga 1.2: area generosa che la segue in volo
        SpriteRenderer sparkle = Art(r, "Sparkle", "sparkle_star", 14, false, new Vector2(0.55f, coinY + 0.55f)).GetComponent<SpriteRenderer>();
        sparkle.gameObject.SetActive(false);

        // ---- il pulsante FLIP (sul pavimento, sotto il tavolo) ----
        Transform button = Child(r, "FlipButton", new Vector2(0f, -3.55f)).transform;
        Transform body = Child(button, "Body", Vector2.zero).transform;
        Art(body, "Key", "v83_b_button", 20, false);
        TextMesh flipLabel = Label(body, new Vector2(0f, 0.07f), 21, "FLIP", 0.09f);
        flipLabel.color = Color.white;
        Fit(flipLabel, 1.9f, 0.6f);
        BoxCollider2D flipArea = HW_Box(button, new Vector2(3.0f, 1.5f));   // tasto largo 2.4: area generosa

        // ---- "Flipping..." sul bordo del tavolo (nascosta) ----
        Transform cue = Child(r, "Cue", new Vector2(0f, -2.53f)).transform;
        TextMesh cueText = Label(cue, Vector2.zero, 25, "Flipping...", 0.045f);
        cueText.color = V83BCueInk;
        Fit(cueText, 2.6f, 0.34f);
        cue.gameObject.SetActive(false);

        // ---- la targhetta del punteggio (in alto a destra, fuori dalla traiettoria della moneta) ----
        Transform plaque = Child(r, "Score", new Vector2(1.6f, 2.45f)).transform;
        Art(plaque, "Plaque", "v83_b_plaque", 5, false);
        TextMesh score = Label(plaque, Vector2.zero, 6, "HEADS 0   TAILS 0", 0.035f);
        score.color = V83BScoreInk;
        Fit(score, 1.4f, 0.34f);

        HW_Goal(r, "FLIP HEADS.");
        TextMesh info = HW_Toast(r, new Vector2(0f, -4.55f), out Transform infoHolder);
        SetField(level, "coin", coin);
        SetField(level, "coinSpin", spin);
        SetField(level, "faceHeads", heads);
        SetField(level, "faceTails", tails);
        SetField(level, "coinEdge", edge);
        SetField(level, "coinArea", coinArea);
        SetField(level, "shadow", shadow);
        SetField(level, "coinGlow", glow);
        SetField(level, "sparkle", sparkle);
        SetField(level, "flipButton", button);
        SetField(level, "flipBody", body);
        SetField(level, "flipArea", flipArea);
        SetField(level, "cueText", cueText);
        SetField(level, "cueHolder", cue);
        SetField(level, "scoreText", score);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, 0.75f)));
        return SaveLevel(root, "V83LuckHack");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V83B = new Dictionary<string, string[]>
    {
        { "luck_hack", new[] { "What if the whole world flipped while the coin was in the air?",
            "Press FLIP, then turn your phone upside down before the coin lands." } },
    };
}
#endif
