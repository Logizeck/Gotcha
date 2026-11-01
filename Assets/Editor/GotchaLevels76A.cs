#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.76, livello 2 della roadmap (gruppo A): il pulsante timido. Lo sfondo semplice (v76_a_bg), il pulsantone
/// verde START (v76_a_button, scritta START come testo sul basamento) con le quattro facce sovrapposte (v76_a_face_*),
/// due gocce di sudore ("sweat", la seconda solo quando è bloccato), lo sbuffo della fuga ("puff"), la riga
/// dell'obiettivo e la scritta di commento (aiuti di Halloween: HW_Goal, HW_Obj, HW_Toast) e la festa comune
/// (RewardRig). Niente Easter egg e niente Toni. L'elenco dei livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 2: The Shy Button ----------

    static GameObject BuildV76ShyButton()
    {
        var root = new GameObject("Level_V76ShyButton");
        V76ShyButtonLevel level = root.AddComponent<V76ShyButtonLevel>();
        Transform r = root.transform;
        Art(r, "Wall", "v76_a_bg", -20, false);

        // ---- il pulsante: radice che scappa, corpo che trema, facce e sudore ----
        Transform button = Child(r, "Button", new Vector2(0f, -0.6f)).transform;
        button.localScale = Vector3.one * 1.2f;   // il pulsantone: largo 2.9 (i raggi di ShyButtonLogic sono per questa scala)
        Transform body = Child(button, "Body", Vector2.zero).transform;
        Art(body, "Art", "v76_a_button", 5, false);
        TextMesh start = Label(body, new Vector2(0f, DoodleSprites.V76ALabelY), 7, "START", 0.075f);
        start.color = Color.white;
        start.fontStyle = FontStyle.Bold;
        Fit(start, 1.8f, 0.34f);
        Transform faceNervous = HW_Obj(body, "FaceNervous", Vector2.zero, "v76_a_face_nervous", 6);
        Transform facePanic = HW_Obj(body, "FacePanic", Vector2.zero, "v76_a_face_panic", 6);
        Transform facePinned = HW_Obj(body, "FacePinned", Vector2.zero, "v76_a_face_pinned", 6);
        Transform faceWide = HW_Obj(body, "FaceWide", Vector2.zero, "v76_a_face_wide", 6);
        facePanic.gameObject.SetActive(false);
        facePinned.gameObject.SetActive(false);
        faceWide.gameObject.SetActive(false);
        SpriteRenderer sweat = Art(body, "Sweat", "sweat", 8, false, new Vector2(DoodleSprites.V76ASweatX, DoodleSprites.V76ASweatY)).GetComponent<SpriteRenderer>();
        sweat.transform.localScale = Vector3.one * 1.8f;
        sweat.gameObject.SetActive(false);
        SpriteRenderer sweatPinned = Art(body, "SweatPinned", "sweat", 8, false, new Vector2(-DoodleSprites.V76ASweatX + 0.05f, DoodleSprites.V76ASweatY + 0.05f)).GetComponent<SpriteRenderer>();
        sweatPinned.transform.localScale = Vector3.one * 1.6f;
        sweatPinned.gameObject.SetActive(false);

        // ---- lo sbuffo lasciato al posto di prima ----
        SpriteRenderer puff = Art(r, "Puff", "puff", 4, false).GetComponent<SpriteRenderer>();
        puff.gameObject.SetActive(false);

        HW_Goal(r, "PRESS START.");
        TextMesh info = HW_Toast(r, new Vector2(0f, -4.5f), out Transform infoHolder);
        SetField(level, "button", button);
        SetField(level, "body", body);
        SetField(level, "faceNervous", faceNervous);
        SetField(level, "facePanic", facePanic);
        SetField(level, "facePinned", facePinned);
        SetField(level, "faceWide", faceWide);
        SetField(level, "sweat", sweat);
        SetField(level, "sweatPinned", sweatPinned);
        SetField(level, "startLabel", start);
        SetField(level, "puff", puff);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(root.transform));
        return SaveLevel(root, "V76ShyButton");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V76A = new Dictionary<string, string[]>
    {
        { "shy_button", new[] { "Hold it down. Don't let go.",
            "Keep one finger on START and tap it with a second finger." } },
    };
}
#endif
