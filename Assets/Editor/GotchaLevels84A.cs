#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.84, livello 63 della roadmap (gruppo A): The Tiny Crown. La sala del trono ("s41b_throne_bg"), il trono
/// grande ("s41b_throne"), il re (radice al collo sul trono: corpo seduto, corpo in ginocchio nascosto, testona con occhi
/// preoccupati e occhi felici nascosti; collider generoso su testa e corpo), il punto dove si inginocchia, il piedistallo
/// d'oro col cuscino rosso a destra e la corona minuscola ("s41b_crown", radice posata e scalata dal livello), quattro
/// scintille "sparkle_star" (nascoste), il bagliore dorato dietro la cima della testa ("glow", trasparente), il fumetto
/// del re (Bubble/SpeechBubble, si adatta al testo), la riga dell'obiettivo e la scritta di commento (aiuti di
/// Halloween) e la festa comune (RewardRig, sul pavimento sotto il re in ginocchio). Ogni TextMesh ha il suo Fit.
/// Disegni nuovi (prefisso v84_a_) in DoodleSpritesV84A. Niente Easter egg e niente Toni. L'elenco dei livelli (Define)
/// lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 63: The Tiny Crown ----------

    static GameObject BuildV84TinyCrown()
    {
        var root = new GameObject("Level_V84TinyCrown");
        V84TinyCrownLevel level = root.AddComponent<V84TinyCrownLevel>();
        Transform r = root.transform;
        Art(r, "Hall", "s41b_throne_bg", -20, false);
        const float kx = -0.5f, neckY = -0.15f;
        HW_Obj(r, "Throne", new Vector2(kx, -0.85f), "s41b_throne", 0, 2f);

        // ---- il re (radice al collo; la testa è un figlio a parte per chinarsi) ----
        Transform king = Child(r, "King", new Vector2(kx, neckY)).transform;
        Transform bodySit = Art(king, "BodySit", "v84_a_body_sit", 5, false);
        Transform bodyKneel = Art(king, "BodyKneel", "v84_a_body_kneel", 5, false);
        bodyKneel.gameObject.SetActive(false);
        Transform head = Child(king, "Head", new Vector2(0f, DoodleSprites.V84AHeadOnNeckY)).transform;
        head.localScale = Vector3.one * DoodleSprites.V84AHeadScale;
        Art(head, "Art", "v84_a_head", 7, false);
        Transform eyes = Art(head, "Eyes", "v84_a_eyes", 8, false);
        Transform eyesHappy = Art(head, "EyesHappy", "v84_a_eyes_happy", 8, false);
        eyesHappy.gameObject.SetActive(false);
        BoxCollider2D kingCol = HW_Box(king, new Vector2(2.5f, 4.2f), new Vector2(0f, 0.05f));
        Transform kneelSpot = Child(r, "KneelSpot", new Vector2(0.05f, -1.55f)).transform;

        // ---- piedistallo, cuscino e la corona minuscola ----
        const float sx = 1.65f, standY = -1.75f;
        HW_Obj(r, "Stand", new Vector2(sx, standY), "v84_a_stand", 3);
        HW_Obj(r, "Cushion", new Vector2(sx, standY + DoodleSprites.V84ACushionY), "v84_a_cushion", 4);
        Transform crown = HW_Obj(r, "Crown", new Vector2(sx, standY + DoodleSprites.V84ACushionY + DoodleSprites.V84ACrownOnCushionY), "s41b_crown", 20, 1.1f);

        // ---- le scintille (la corona grande abbastanza brilla) e il bagliore sulla testa ----
        var sparkles = new List<Object>();
        for (int i = 0; i < 4; i++)
        {
            Transform s = HW_Obj(r, "Sparkle" + i, crown.localPosition, "sparkle_star", 21, 0.32f);
            s.gameObject.SetActive(false);
            sparkles.Add(s);
        }
        float headTop = neckY + DoodleSprites.V84AHeadOnNeckY + DoodleSprites.V84AHeadTopY * DoodleSprites.V84AHeadScale;
        SpriteRenderer glow = Art(r, "HeadGlow", "glow", 6, false, new Vector2(kx, headTop + 0.05f)).GetComponent<SpriteRenderer>();
        glow.transform.localScale = Vector3.one * 1.4f;
        glow.color = new Color(1f, 0.85f, 0.35f, 0f);
        glow.enabled = false;

        // ---- il fumetto del re (punta a destra della testa; cresce verso destra e verso l'alto) ----
        SpeechBubble bubble = Bubble(r, new Vector2(kx + 1.1f, 0.55f));

        HW_Goal(r, "CROWN THE KING.");
        TextMesh info = HW_Toast(r, new Vector2(0f, 2.45f), out Transform infoHolder);
        SetField(level, "king", king);
        SetField(level, "kingArea", kingCol);
        SetField(level, "bodySit", bodySit);
        SetField(level, "bodyKneel", bodyKneel);
        SetField(level, "head", head);
        SetField(level, "eyes", eyes);
        SetField(level, "eyesHappy", eyesHappy);
        SetFloat(level, "headTopY", DoodleSprites.V84AHeadTopY);
        SetField(level, "kneelSpot", kneelSpot);
        SetField(level, "crown", crown);
        SetFloat(level, "crownBase", 1.1f);
        SetFloat(level, "crownLift", 0.08f);   // la base di "s41b_crown" è in y -0.08
        SetArray(level, "sparkles", sparkles.ToArray());
        SetField(level, "headGlow", glow);
        SetField(level, "kingBubble", bubble);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, -3.7f)));
        return SaveLevel(root, "V84TinyCrown");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V84A = new Dictionary<string, string[]>
    {
        { "tiny_crown", new[] { "Make the crown bigger with two fingers.",
            "Pinch the crown larger, then drag it onto the king's head." } },
    };
}
#endif
