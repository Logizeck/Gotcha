#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.78, livello 15 della roadmap (gruppo B): The Honest Thief. La sala del museo, il piedistallo con la
/// targhetta "PRICELESS" (TextMesh) e il cuscino, il diamante (con due stelline "sparkle_star"), la campana di vetro con
/// la sirena (base, cupola tinta dal livello, fasci di luce), il lampo rosso ("plain" tinto), il ladro (corpo, coppola a
/// parte, punto della mano), la guardia fuori scena a sinistra (corpo e braccio sulla spalla), il tavolino col cestino
/// dei volantini e il cartellino appeso "FREE" (TextMesh), i due fumetti che si adattano al testo (Bubble/SpeechBubble),
/// la riga dell'obiettivo e la scritta di commento (aiuti di Halloween) e la festa comune (RewardRig). Ogni TextMesh ha
/// il suo Fit. Disegni nuovi (prefisso v78_b_) in DoodleSpritesV78B. Niente Easter egg e niente Toni. L'elenco dei
/// livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 15: The Honest Thief ----------

    static GameObject BuildV78HonestThief()
    {
        var root = new GameObject("Level_V78HonestThief");
        V78HonestThiefLevel level = root.AddComponent<V78HonestThiefLevel>();
        Transform r = root.transform;
        Art(r, "Museum", "v78_b_room_bg", -20, false);
        const float floorY = -3.65f;

        // ---- il tavolino dei volantini gratis e il cartellino FREE appeso al gancio ----
        Transform stand = HW_Obj(r, "FreebieStand", new Vector2(-1.85f, floorY), "v78_b_freebie_stand", 2);
        BoxCollider2D standCol = HW_Area(stand, "StandArea", new Vector2(1.0f, 2.0f), new Vector2(0f, 1.0f));
        Transform card = Child(r, "FreeCard", new Vector2(-1.85f, floorY + DoodleSprites.V78BHookY - 0.4f)).transform;
        Art(card, "Art", "v78_b_card", 4, false);
        TextMesh cardText = Label(card, new Vector2(0f, -0.02f), 5, "FREE", 0.045f);
        cardText.color = new Color(0.17f, 0.13f, 0.2f);
        Fit(cardText, 0.66f, 0.3f);
        BoxCollider2D cardCol = HW_Box(card, new Vector2(1.3f, 1.0f), new Vector2(0f, 0.05f));

        // ---- la teca: piedistallo con targhetta, diamante, campana di vetro con la sirena ----
        Transform pedestal = HW_Obj(r, "Pedestal", new Vector2(0f, floorY), "v78_b_pedestal", 3);
        Transform plaque = Child(pedestal, "Plaque", new Vector2(0f, DoodleSprites.V78BPlaqueY)).transform;
        TextMesh plaqueText = Label(plaque, Vector2.zero, 4, "PRICELESS", 0.032f);
        plaqueText.color = new Color(0.35f, 0.24f, 0.08f);
        Fit(plaqueText, 0.78f, 0.22f);
        float top = floorY + DoodleSprites.V78BPedestalTop;
        Transform diamond = HW_Obj(r, "Diamond", new Vector2(0f, top + 0.4f), "v78_b_diamond", 5);
        BoxCollider2D diamondCol = HW_Box(diamond, new Vector2(1.0f, 0.9f));
        var sparkles = new List<Transform>();
        foreach (Vector2 at in new[] { new Vector2(0.28f, 0.22f), new Vector2(-0.24f, -0.12f) })
        {
            Transform s = HW_Obj(diamond, "Sparkle", at, "sparkle_star", 6, 0.45f);
            s.gameObject.SetActive(false);
            sparkles.Add(s);
        }
        Transform cover = Child(r, "CaseCover", new Vector2(0f, top)).transform;
        Art(cover, "Glass", "v78_b_case_glass", 7, false);
        float glassTop = DoodleSprites.V78BGlassHeight + 0.06f;
        Art(cover, "SirenBase", "v78_b_siren_base", 8, false, new Vector2(0f, glassTop));
        SpriteRenderer dome = Art(cover, "SirenDome", "v78_b_siren_dome", 9, false, new Vector2(0f, glassTop + 0.1f)).GetComponent<SpriteRenderer>();
        dome.color = new Color(0.78f, 0.36f, 0.36f);
        SpriteRenderer beams = Art(cover, "SirenBeams", "v78_b_siren_beams", 10, false, new Vector2(0f, glassTop + 0.2f)).GetComponent<SpriteRenderer>();
        beams.gameObject.SetActive(false);
        // tocchi sulla teca e zona di rilascio del cartellino: vetro + piedistallo (dai piedi alla cima del vetro)
        float caseTop = top + DoodleSprites.V78BGlassHeight;
        BoxCollider2D caseCol = HW_Area(r, "CaseArea", new Vector2(1.4f, caseTop - floorY), new Vector2(0f, (caseTop + floorY) * 0.5f));
        SpriteRenderer flash = Bar(r, "AlarmFlash", Vector2.zero, new Vector2(6.5f, 11f), new Color(1f, 0.1f, 0.1f, 0f), 50);
        flash.gameObject.SetActive(false);

        // ---- il ladro (guarda a sinistra, mano tesa) ----
        Transform thief = HW_Obj(r, "Thief", new Vector2(1.7f, floorY), "v78_b_thief_body", 12);
        Transform hat = HW_Obj(thief, "Hat", new Vector2(0f, DoodleSprites.V78BThiefHeadTop - 0.04f), "v78_b_thief_hat", 13);
        Transform hand = Child(thief, "Hand", new Vector2(DoodleSprites.V78BHandX, DoodleSprites.V78BHandY)).transform;
        BoxCollider2D thiefCol = HW_Box(thief, new Vector2(1.1f, 3.0f), new Vector2(0f, 1.5f));

        // ---- la guardia, fuori scena a sinistra ----
        Transform guard = HW_Obj(r, "Guard", new Vector2(-3.9f, floorY), "v78_b_guard_body", 20);
        Transform arm = HW_Obj(guard, "Arm", new Vector2(DoodleSprites.V78BGuardShoulderX, DoodleSprites.V78BGuardShoulderY), "v78_b_guard_arm", 21);

        // ---- i fumetti (si adattano al testo e restano nello schermo; la codina indica chi parla) ----
        SpeechBubble guardBubble = Bubble(r, new Vector2(-0.85f + 0.85f, floorY + 2.45f + 0.8f));
        SpeechBubble thiefBubble = Bubble(r, new Vector2(1.45f + 0.85f, floorY + 2.4f + 0.8f));

        HW_Goal(r, "STEAL THE DIAMOND.");
        TextMesh info = HW_Toast(r, new Vector2(0f, 2.45f), out Transform infoHolder);
        SetField(level, "card", card);
        SetField(level, "cardArea", cardCol);
        SetField(level, "cardText", cardText);
        SetField(level, "diamond", diamond);
        SetField(level, "diamondArea", diamondCol);
        SetArray(level, "diamondSparkles", sparkles.ToArray());
        SetField(level, "caseCover", cover);
        SetField(level, "caseArea", caseCol);
        SetField(level, "sirenDome", dome);
        SetField(level, "sirenBeams", beams);
        SetField(level, "plaque", plaque);
        SetField(level, "plaqueText", plaqueText);
        SetField(level, "flash", flash);
        SetField(level, "thief", thief);
        SetField(level, "thiefHat", hat);
        SetField(level, "thiefHand", hand);
        SetField(level, "thiefArea", thiefCol);
        SetField(level, "guard", guard);
        SetField(level, "guardArm", arm);
        SetFloat(level, "guardStandX", -1.1f);
        SetField(level, "standArea", standCol);
        SetField(level, "guardBubble", guardBubble);
        SetField(level, "thiefBubble", thiefBubble);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r));
        return SaveLevel(root, "V78HonestThief");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V78B = new Dictionary<string, string[]>
    {
        { "honest_thief", new[] { "Something on the side is FREE.",
            "Drag the FREE sign onto the diamond's case, then take the diamond." } },
    };
}
#endif
