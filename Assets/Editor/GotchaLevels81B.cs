#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.81, livello 38 della roadmap (gruppo B): The Impossible Hug. Il parco ("park_bg"), i due amici tondi Bo (blu,
/// a sinistra) e Pip (rosa, a destra): radice posata dal livello con un collider a cerchio generoso, figlio Look per gli
/// effetti (quello di Pip rovesciato in x, così guarda verso Bo) con corpo, tre facce (arrabbiata, timida, abbraccio),
/// braccia conserte e braccia dell'abbraccio e il segno di rabbia sopra la testa dal lato esterno. In mezzo, sopra di
/// loro, il cuore spezzato (due metà con la stessa origine, il bagliore "glow" dietro, spento) e i cuoricini "heart" della
/// festa (nascosti). Un fumetto per amico (Bubble/SpeechBubble: si adattano al testo e restano nello schermo), la riga
/// dell'obiettivo e la scritta di commento (aiuti di Halloween, ognuna col suo Fit) e la festa comune (RewardRig, in
/// alto sopra il cuore). Disegni nuovi (prefisso v81_b_) in DoodleSpritesV81B. Niente Easter egg e niente Toni.
/// L'elenco dei livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 38: The Impossible Hug ----------

    static GameObject BuildV81ImpossibleHug()
    {
        var root = new GameObject("Level_V81ImpossibleHug");
        V81ImpossibleHugLevel level = root.AddComponent<V81ImpossibleHugLevel>();
        Transform r = root.transform;
        Art(r, "Park", "park_bg", -20, false);
        const float cy = -1.8f, buddyScale = 1.15f;
        float homeX = Gotcha.Levels.V64.ImpossibleHugLogic.HomeX;

        // ---- i due amici (0 = Bo a sinistra, 1 = Pip a destra, rovesciato) ----
        string[] names = { "Bo", "Pip" };
        string[] colours = { "blue", "pink" };
        var buddies = new List<Object>();
        var looks = new List<Object>();
        var areas = new List<Object>();
        var facesAngry = new List<Object>();
        var facesShy = new List<Object>();
        var facesHug = new List<Object>();
        var armsCross = new List<Object>();
        var armsHug = new List<Object>();
        var angers = new List<Object>();
        for (int i = 0; i < 2; i++)
        {
            int o = 10 + i * 4;   // Pip sopra Bo: nell'abbraccio le braccia di Bo passano dietro Pip, quelle di Pip davanti a Bo
            Transform buddy = Child(r, names[i], new Vector2(i == 0 ? -homeX : homeX, cy)).transform;
            buddy.localScale = Vector3.one * buddyScale;
            Transform look = Child(buddy, "Look", Vector2.zero).transform;
            look.localScale = new Vector3(i == 0 ? 1f : -1f, 1f, 1f);
            Art(look, "Body", "v81_b_body_" + colours[i], o, false);
            Transform fa = Art(look, "FaceAngry", "v81_b_face_angry", o + 1, false);
            Transform fs = Art(look, "FaceShy", "v81_b_face_shy", o + 1, false);
            fs.gameObject.SetActive(false);
            Transform fh = Art(look, "FaceHug", "v81_b_face_hug", o + 1, false);
            fh.gameObject.SetActive(false);
            Transform ac = Art(look, "ArmsCrossed", "v81_b_arms_cross_" + colours[i], o + 2, false);
            Transform ah = Art(look, "ArmsHug", "v81_b_arms_hug_" + colours[i], o + 2, false);
            ah.gameObject.SetActive(false);
            Transform anger = HW_Obj(look, "Anger", new Vector2(-0.45f, 0.5f), "v81_b_anger", o + 3);
            anger.gameObject.SetActive(false);
            CircleCollider2D col = buddy.gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.85f;   // circa 1 unità nel mondo: bersaglio generoso
            buddies.Add(buddy);
            looks.Add(look);
            areas.Add(col);
            facesAngry.Add(fa);
            facesShy.Add(fs);
            facesHug.Add(fh);
            armsCross.Add(ac);
            armsHug.Add(ah);
            angers.Add(anger);
        }

        // ---- il cuore spezzato (le due metà combaciano nello stesso punto: le apre il livello) ----
        Transform heart = Child(r, "Heart", new Vector2(0f, 1.1f)).transform;
        SpriteRenderer glow = Art(heart, "Glow", "glow", 4, false).GetComponent<SpriteRenderer>();
        glow.transform.localScale = Vector3.one * 2.2f;
        glow.color = new Color(1f, 0.55f, 0.65f, 0f);
        Transform heartL = HW_Obj(heart, "HalfLeft", Vector2.zero, "v81_b_heart_left", 5);
        Transform heartR = HW_Obj(heart, "HalfRight", Vector2.zero, "v81_b_heart_right", 5);

        // ---- i cuoricini della festa (nascosti) ----
        var hearts = new List<Object>();
        for (int i = 0; i < 8; i++)
        {
            Transform h = HW_Obj(r, "FloatHeart" + i, Vector2.zero, "heart", 20, 0.7f);
            h.gameObject.SetActive(false);
            hearts.Add(h);
        }

        // ---- un fumetto per amico (la punta sopra la testa, verso il centro) ----
        SpeechBubble bubbleBo = Bubble(r, new Vector2(-1.05f, cy + 0.75f));
        SpeechBubble bubblePip = Bubble(r, new Vector2(1.0f, cy + 0.75f));

        HW_Goal(r, "MAKE THE TWO FRIENDS MAKE UP.");
        TextMesh info = HW_Toast(r, new Vector2(0f, 2.45f), out Transform infoHolder);
        SetArray(level, "buddies", buddies.ToArray());
        SetArray(level, "looks", looks.ToArray());
        SetArray(level, "areas", areas.ToArray());
        SetArray(level, "facesAngry", facesAngry.ToArray());
        SetArray(level, "facesShy", facesShy.ToArray());
        SetArray(level, "facesHug", facesHug.ToArray());
        SetArray(level, "armsCrossed", armsCross.ToArray());
        SetArray(level, "armsHug", armsHug.ToArray());
        SetArray(level, "angerMarks", angers.ToArray());
        SetField(level, "heartLeft", heartL);
        SetField(level, "heartRight", heartR);
        SetField(level, "heartGlow", glow);
        SetArray(level, "floatHearts", hearts.ToArray());
        SetArray(level, "bubbles", new Object[] { bubbleBo, bubblePip });
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, 2.35f)));
        return SaveLevel(root, "V81ImpossibleHug");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V81B = new Dictionary<string, string[]>
    {
        { "impossible_hug", new[] { "What if they both move at the same time?",
            "Hold one friend with each finger and drag them together." } },
    };
}
#endif
