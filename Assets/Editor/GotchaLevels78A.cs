#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.78, livello 13 della roadmap (gruppo A): The Aristocratic Cat. Il salotto (v78_a_bg), l'armadio altissimo
/// (v78_a_wardrobe), il gatto snob in sei pose sotto un perno (sdraiato, seccato, aggrappato, in caduta, seduto, che si
/// liscia il pelo), la sua area di tocco e il fumetto (che si adatta al testo e resta nello schermo), la scala, la
/// ciotola e il giocattolo trascinabili con aree generose, il segnaposto della scala appoggiata, i graffi delle unghie,
/// lo sbuffo dell'atterraggio, il luccichio, la riga dell'obiettivo (HW_Goal) e la festa comune (RewardRig). Niente
/// Easter egg e niente Toni. L'elenco dei livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 13: The Aristocratic Cat ----------

    static GameObject BuildV78AristocraticCat()
    {
        var root = new GameObject("Level_V78AristocraticCat");
        V78AristocraticCatLevel level = root.AddComponent<V78AristocraticCatLevel>();
        Transform r = root.transform;
        Art(r, "Room", "v78_a_bg", -20, false);
        Art(r, "Wardrobe", "v78_a_wardrobe", -10, false);

        // ---- graffi sul bordo dell'armadio (invisibili finché il gatto non si aggrappa) ----
        SpriteRenderer scratches = Art(r, "Scratches", "v78_a_scratch", -8, false, new Vector2(0.42f, DoodleSprites.V78AWardTop - 0.12f)).GetComponent<SpriteRenderer>();
        scratches.color = new Color(1f, 1f, 1f, 0f);

        // ---- oggetti sul pavimento (la scala appoggiata al muro a sinistra) ----
        Transform ladder = HW_Obj(r, "Ladder", new Vector2(-1.78f, -1.25f), "v78_a_ladder", 4);
        ladder.localRotation = Quaternion.Euler(0f, 0f, -3f);
        BoxCollider2D ladderArea = HW_Box(ladder, new Vector2(1.0f, 3.6f));
        Transform ladderLean = Child(r, "LadderLean", new Vector2(-0.42f, -1.25f)).transform;
        ladderLean.localRotation = Quaternion.Euler(0f, 0f, -11f);
        Transform bowl = HW_Obj(r, "Bowl", new Vector2(-0.8f, -3.6f), "v78_a_bowl", 14);
        BoxCollider2D bowlArea = HW_Box(bowl, new Vector2(1.4f, 1.0f));
        Transform toy = HW_Obj(r, "Toy", new Vector2(0.95f, -3.85f), "v78_a_toy", 15);
        BoxCollider2D toyArea = HW_Box(toy, new Vector2(1.3f, 0.95f), new Vector2(0.05f, 0.05f));

        // ---- il gatto: radice ai piedi sul piano dell'armadio, perno al centro del corpo, sei pose ----
        Transform cat = Child(r, "Cat", new Vector2(1.12f, DoodleSprites.V78AWardTop)).transform;
        Transform pivot = Child(cat, "Pivot", new Vector2(0f, 0.5f)).transform;
        Vector2 down = new Vector2(0f, -0.5f);
        Transform poseLie = Art(pivot, "Lie", "v78_a_cat_lie", 20, false, down);
        Transform poseAnnoyed = Art(pivot, "Annoyed", "v78_a_cat_annoyed", 20, false, down);
        Transform poseCling = Art(pivot, "Cling", "v78_a_cat_cling", 20, false, down);
        Transform poseFall = Art(pivot, "Fall", "v78_a_cat_fall", 20, false, down);
        Transform poseSit = Art(pivot, "Sit", "v78_a_cat_sit", 20, false, down);
        Transform poseGroom = Art(pivot, "Groom", "v78_a_cat_groom", 20, false, down);
        foreach (Transform t in new[] { poseAnnoyed, poseCling, poseFall, poseSit, poseGroom }) t.gameObject.SetActive(false);
        BoxCollider2D catArea = HW_Box(cat, new Vector2(2.3f, 1.5f), new Vector2(-0.1f, 0.55f));

        // ---- effetti ----
        SpriteRenderer dust = Art(r, "Dust", "puff", 25, false).GetComponent<SpriteRenderer>();
        dust.color = new Color(0.95f, 0.9f, 0.85f, 0.85f);
        dust.gameObject.SetActive(false);
        SpriteRenderer sparkle = Art(r, "Sparkle", "sparkle_star", 26, false).GetComponent<SpriteRenderer>();
        sparkle.gameObject.SetActive(false);

        // ---- fumetto del gatto (si sposta il contenitore; il fumetto va a capo e rientra nello schermo da solo) ----
        Transform bubbleHolder = Child(r, "CatBubbleHolder", new Vector2(1.09f, DoodleSprites.V78AWardTop + 1.3f)).transform;
        SpeechBubble bubble = SmallBubble(bubbleHolder, Vector2.zero);
        bubble.catVoice = true;   // il gatto miagola quando parla
        bubble.maxTextWidth = 3.6f;   // righe più larghe (scala 0.5: 1.8 nel mondo): al massimo due righe, sotto la fascia dell'obiettivo

        HW_Goal(r, "GET THE CAT DOWN.");
        SetField(level, "cat", cat);
        SetField(level, "catPivot", pivot);
        SetField(level, "poseLie", poseLie);
        SetField(level, "poseAnnoyed", poseAnnoyed);
        SetField(level, "poseCling", poseCling);
        SetField(level, "poseFall", poseFall);
        SetField(level, "poseSit", poseSit);
        SetField(level, "poseGroom", poseGroom);
        SetField(level, "catArea", catArea);
        SetField(level, "catBubble", bubble);
        SetField(level, "bubbleHolder", bubbleHolder);
        SetField(level, "ladder", ladder);
        SetField(level, "ladderArea", ladderArea);
        SetField(level, "ladderLean", ladderLean);
        SetField(level, "bowl", bowl);
        SetField(level, "bowlArea", bowlArea);
        SetField(level, "toy", toy);
        SetField(level, "toyArea", toyArea);
        SetField(level, "scratches", scratches);
        SetField(level, "dust", dust);
        SetField(level, "sparkle", sparkle);
        SetFloat(level, "edgeX", DoodleSprites.V78AWardLeft + 0.4f);
        SetField(level, "reward", RewardRig(root.transform));
        return SaveLevel(root, "V78AristocraticCat");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V78A = new Dictionary<string, string[]>
    {
        { "aristocratic_cat", new[] { "Maybe gravity can convince it.",
            "Turn your phone upside down and hold it." } },
    };
}
#endif
