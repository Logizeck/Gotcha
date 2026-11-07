#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;
using Gotcha.Levels.V64;

/// <summary>
/// Versione 0.86, livello 110 della roadmap (gruppo B): The Imaginary Bridge. Lo sfondo (v86_b_bg: cielo, sole, mesas,
/// interno del canyon), l'asse (radice in basso al centro sul piano delle rocce; dentro Body il tratto centrale che il
/// livello allunga in x e i due capi; collider generoso che il livello adatta alla lunghezza) DIETRO le rocce in primo
/// piano (v86_b_cliffs: l'asse che cade nel canyon passa dietro le pareti, niente SpriteMask), la bandierina sulla
/// piattaforma di destra, la scintilla (nascosta), l'esploratore (radice ai piedi, Body con le pose in piedi,
/// spaventato e felice; collider generoso) con la sua ombra, il fumetto dell'esploratore (Bubble/SpeechBubble, si adatta
/// al testo e resta nello schermo), la riga dell'obiettivo e la scritta di commento (aiuti di Halloween) e la festa
/// comune (RewardRig, in alto sopra le teste). Ogni TextMesh ha il suo Fit (HW_Goal, HW_Toast, RewardRig); il fumetto
/// si adatta da solo. Disegni nuovi (prefisso v86_b_) in DoodleSpritesV86B. Niente Easter egg e niente Toni. L'elenco
/// dei livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 110: The Imaginary Bridge ----------

    static GameObject BuildV86ImaginaryBridge()
    {
        var root = new GameObject("Level_V86ImaginaryBridge");
        V86ImaginaryBridgeLevel level = root.AddComponent<V86ImaginaryBridgeLevel>();
        Transform r = root.transform;
        Art(r, "Canyon", "v86_b_bg", -20, false);
        const float cliffY = ImaginaryBridgeLogic.CliffY, len0 = ImaginaryBridgeLogic.MinLength, cap = DoodleSprites.V86BPlankCap;

        // ---- l'asse (radice in basso al centro; il livello allunga Mid e sposta i capi) ----
        Transform plank = Child(r, "Plank", new Vector2(ImaginaryBridgeLogic.PlankStartX, cliffY)).transform;
        Transform pBody = Child(plank, "Body", Vector2.zero).transform;
        Transform mid = Art(pBody, "Mid", "v86_b_plank_mid", 3, false);
        mid.localScale = new Vector3(len0 - 2f * cap + 0.04f, 1f, 1f);
        Transform capL = Child(pBody, "CapL", new Vector2(-len0 * 0.5f, 0f)).transform;
        Transform capLArt = Art(capL, "Art", "v86_b_plank_end", 3, false);
        Transform capR = Child(pBody, "CapR", new Vector2(len0 * 0.5f, 0f)).transform;
        capR.localScale = new Vector3(-1f, 1f, 1f);
        Transform capRArt = Art(capR, "Art", "v86_b_plank_end", 3, false);
        BoxCollider2D plankCol = HW_Box(plank, new Vector2(len0 + 0.6f, 0.9f), new Vector2(0f, 0.1f));   // asse alta 0.2: area generosa
        var plankArts = new Object[] { mid.GetComponent<SpriteRenderer>(), capLArt.GetComponent<SpriteRenderer>(), capRArt.GetComponent<SpriteRenderer>() };

        // ---- le rocce in primo piano (davanti all'asse che cade) e la bandierina ----
        Art(r, "Cliffs", "v86_b_cliffs", 4, false);
        Transform flag = Child(r, "Flag", new Vector2(2.1f, cliffY)).transform;
        Art(flag, "Art", "v86_b_flag", 5, false);
        SpriteRenderer sparkle = Art(r, "Sparkle", "sparkle_star", 9, false, new Vector2(ImaginaryBridgeLogic.PlankStartX, cliffY + 0.3f)).GetComponent<SpriteRenderer>();
        sparkle.gameObject.SetActive(false);

        // ---- l'esploratore (radice ai piedi) con l'ombra ----
        Transform shadow = Art(r, "ManShadow", "ball_shadow", 6, false, new Vector2(ImaginaryBridgeLogic.ManStartX, cliffY + 0.02f));
        shadow.localScale = new Vector3(0.8f, 0.2f, 1f);
        shadow.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        Transform man = Child(r, "Man", new Vector2(ImaginaryBridgeLogic.ManStartX, cliffY)).transform;
        Transform mBody = Child(man, "Body", Vector2.zero).transform;
        Transform stand = Art(mBody, "Stand", "v86_b_man", 8, false);
        Transform scared = Art(mBody, "Scared", "v86_b_man_scared", 8, false);
        scared.gameObject.SetActive(false);
        Transform cheer = Art(mBody, "Cheer", "v86_b_man_cheer", 8, false);
        cheer.gameObject.SetActive(false);
        BoxCollider2D manCol = HW_Box(man, new Vector2(1.0f, 1.6f), new Vector2(0f, 0.7f));   // omino largo 0.7 e alto 1.3: area generosa

        // ---- il fumetto dell'esploratore (il livello lo porta accanto alla testa) ----
        SpeechBubble bubble = Bubble(r, new Vector2(ImaginaryBridgeLogic.ManStartX + 0.95f, cliffY + 1.4f));

        HW_Goal(r, "CROSS THE CANYON.");
        TextMesh info = HW_Toast(r, new Vector2(0f, -4.45f), out Transform infoHolder);
        SetField(level, "plank", plank);
        SetField(level, "plankBody", pBody);
        SetField(level, "plankMid", mid);
        SetField(level, "plankCapL", capL);
        SetField(level, "plankCapR", capR);
        SetArray(level, "plankArts", plankArts);
        SetField(level, "plankArea", plankCol);
        SetField(level, "sparkle", sparkle);
        SetField(level, "flag", flag);
        SetField(level, "man", man);
        SetField(level, "manBody", mBody);
        SetField(level, "manStand", stand);
        SetField(level, "manScared", scared);
        SetField(level, "manCheer", cheer);
        SetField(level, "manArea", manCol);
        SetField(level, "manShadow", shadow);
        SetField(level, "manBubble", bubble);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, 1.95f)));
        return SaveLevel(root, "V86ImaginaryBridge");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V86B = new Dictionary<string, string[]>
    {
        { "imaginary_bridge", new[] { "Things can grow in this game.",
            "Pinch the plank longer, then lay it across the canyon." } },
    };
}
#endif
