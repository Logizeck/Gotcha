#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.80, livello 27 della roadmap (gruppo A): The Sad Balloon. Il parco ("park_bg"), il palloncino rosso a
/// sinistra del centro (radice posata dal livello, figlio Body con corpo, faccia triste e faccia felice nascosta; collider
/// a cerchio generoso), lo spago (LineRenderer disegnata dal livello), il sassolino sul prato col nodo (figlio a parte,
/// collider generoso su sasso + nodo), il ricciolo dello spago sciolto (nascosto), otto cuoricini "heart" per la scia
/// (nascosti), il fumetto del palloncino (Bubble/SpeechBubble, si adatta al testo), la riga dell'obiettivo e la scritta di
/// commento (aiuti di Halloween) e la festa comune (RewardRig, sotto il palloncino che sale). Ogni TextMesh ha il suo
/// Fit. Disegni nuovi (prefisso v80_a_) in DoodleSpritesV80A. Niente Easter egg e niente Toni. L'elenco dei livelli
/// (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 27: The Sad Balloon ----------

    static GameObject BuildV80SadBalloon()
    {
        var root = new GameObject("Level_V80SadBalloon");
        V80SadBalloonLevel level = root.AddComponent<V80SadBalloonLevel>();
        Transform r = root.transform;
        Art(r, "Park", "park_bg", -20, false);
        const float x0 = -0.7f;

        // ---- il palloncino (radice posata dal livello, Body per gli effetti) ----
        Transform balloon = Child(r, "Balloon", new Vector2(x0, 0.25f)).transform;
        Transform body = Child(balloon, "Body", Vector2.zero).transform;
        Art(body, "Art", "v80_a_balloon", 10, false);
        Transform faceSad = Art(body, "FaceSad", "v80_a_face_sad", 11, false);
        Transform faceHappy = Art(body, "FaceHappy", "v80_a_face_happy", 11, false);
        faceHappy.gameObject.SetActive(false);
        CircleCollider2D balloonCol = balloon.gameObject.AddComponent<CircleCollider2D>();
        balloonCol.radius = 1.05f;

        // ---- lo spago (dal nodino del palloncino al nodo sul sasso; i punti li mette il livello) ----
        LineRenderer str = Line(r, "String", 0.035f, new Color(0.62f, 0.47f, 0.27f), 9);
        str.positionCount = 20;
        Transform loose = HW_Obj(r, "LooseEnd", new Vector2(x0, -2.5f), "v80_a_loose_end", 9);
        loose.gameObject.SetActive(false);

        // ---- il sassolino sul prato col nodo (bersaglio generoso) ----
        Transform stone = HW_Obj(r, "Stone", new Vector2(x0, -3.55f), "v80_a_stone", 12);
        Transform knot = Art(stone, "Knot", "v80_a_knot", 13, false);
        BoxCollider2D stoneCol = HW_Box(stone, new Vector2(1.5f, 1.3f), new Vector2(0f, 0.1f));

        // ---- la scia di cuoricini (nascosti, li usa la festa) ----
        var hearts = new List<Object>();
        for (int i = 0; i < 8; i++)
        {
            Transform h = HW_Obj(r, "Heart" + i, new Vector2(x0, 0f), "heart", 8, 0.8f);
            h.gameObject.SetActive(false);
            hearts.Add(h);
        }

        // ---- il fumetto del palloncino (punta accanto al palloncino, a destra) ----
        SpeechBubble bubble = Bubble(r, new Vector2(x0 + 0.8f, 0.45f));

        HW_Goal(r, "MAKE THE BALLOON FLY.");
        TextMesh info = HW_Toast(r, new Vector2(0f, 2.45f), out Transform infoHolder);
        SetField(level, "balloon", balloon);
        SetField(level, "balloonArea", balloonCol);
        SetField(level, "balloonBody", body);
        SetField(level, "faceSad", faceSad);
        SetField(level, "faceHappy", faceHappy);
        SetFloat(level, "tieY", DoodleSprites.V80ATieY);
        SetField(level, "stringLine", str);
        SetField(level, "stone", stone);
        SetField(level, "stoneArea", stoneCol);
        SetField(level, "knot", knot);
        SetFloat(level, "knotY", DoodleSprites.V80AKnotY);
        SetField(level, "looseEnd", loose);
        SetArray(level, "hearts", hearts.ToArray());
        SetFloat(level, "flyY", 7f);
        SetField(level, "balloonBubble", bubble);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, -1.45f)));
        return SaveLevel(root, "V80SadBalloon");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V80A = new Dictionary<string, string[]>
    {
        { "sad_balloon", new[] { "Look at what's holding it down.",
            "Drag the little stone away to untie the string." } },
    };
}
#endif
