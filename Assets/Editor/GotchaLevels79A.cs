#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.79, livello 19 della roadmap (gruppo A): The Shy Monster. La tana (muro di pietra con le torce), il mostro
/// coi piedi in (0, -3.3): corpo, braccia sui perni delle spalle (la sinistra specchiata), sopracciglia su perni propri,
/// guance rosse trasparenti, braccia "timide" e simbolo della rabbia nascosti; sul pavimento la coccarda con la scritta
/// "BEST\nMONSTER" (TextMesh), la spadina e la fionda; il fumetto del mostro (Bubble/SpeechBubble, si adatta al testo), la
/// riga dell'obiettivo e la scritta di commento (aiuti di Halloween) e la festa comune (RewardRig). Ogni TextMesh ha il
/// suo Fit. Disegni nuovi (prefisso v79_a_) in DoodleSpritesV79A. Niente Easter egg e niente Toni. L'elenco dei livelli
/// (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 19: The Shy Monster ----------

    static GameObject BuildV79ShyMonster()
    {
        var root = new GameObject("Level_V79ShyMonster");
        V79ShyMonsterLevel level = root.AddComponent<V79ShyMonsterLevel>();
        Transform r = root.transform;
        Art(r, "Lair", "v79_a_lair_bg", -20, false);
        const float feetY = -3.3f;

        // ---- il mostro (origine ai piedi: cresce verso l'alto restando sul pavimento) ----
        Transform monster = Child(r, "Monster", new Vector2(0f, feetY)).transform;
        SpriteRenderer body = Art(monster, "Body", "v79_a_monster_body", 5, false).GetComponent<SpriteRenderer>();
        Transform armL = Child(monster, "ArmLeft", new Vector2(-DoodleSprites.V79AShoulderX, DoodleSprites.V79AShoulderY)).transform;
        armL.localScale = new Vector3(-1f, 1f, 1f);
        Art(armL, "Art", "v79_a_monster_arm", 4, false);
        Transform armR = Child(monster, "ArmRight", new Vector2(DoodleSprites.V79AShoulderX, DoodleSprites.V79AShoulderY)).transform;
        Art(armR, "Art", "v79_a_monster_arm", 4, false);
        Transform browL = Child(monster, "BrowLeft", new Vector2(-DoodleSprites.V79ABrowX, DoodleSprites.V79ABrowY)).transform;
        Art(browL, "Art", "v79_a_monster_brow", 6, false);
        Transform browR = Child(monster, "BrowRight", new Vector2(DoodleSprites.V79ABrowX, DoodleSprites.V79ABrowY)).transform;
        Art(browR, "Art", "v79_a_monster_brow", 6, false);
        SpriteRenderer blush = Art(monster, "Blush", "v79_a_monster_blush", 7, false, new Vector2(0f, DoodleSprites.V79ACheekY)).GetComponent<SpriteRenderer>();
        blush.color = new Color(1f, 1f, 1f, 0f);
        Transform shy = Art(monster, "ShyArms", "v79_a_monster_shy", 8, false);
        shy.gameObject.SetActive(false);
        Transform anger = Child(monster, "Anger", new Vector2(DoodleSprites.V79AAngerX, DoodleSprites.V79AAngerY)).transform;
        Art(anger, "Art", "v79_a_monster_anger", 9, false);
        anger.gameObject.SetActive(false);
        // colpi e "sopra il mostro": tutta la sagoma (corna e braccia comprese), cresce col mostro
        BoxCollider2D monsterCol = HW_Box(monster, new Vector2(3.0f, 4.6f), new Vector2(0f, 2.3f));

        // ---- sul pavimento: la coccarda BEST MONSTER, la spadina e la fionda (davanti al mostro) ----
        Transform medal = HW_Obj(r, "Medal", new Vector2(-1.65f, -3.72f), "v79_a_medal", 11);
        medal.localRotation = Quaternion.Euler(0f, 0f, 14f);
        TextMesh medalText = Label(medal, new Vector2(0f, 0f), 12, "BEST\nMONSTER", 0.03f);
        medalText.color = new Color(0.35f, 0.22f, 0.06f);
        medalText.lineSpacing = 0.9f;
        Fit(medalText, 0.5f, 0.4f);
        BoxCollider2D medalCol = HW_Box(medal, new Vector2(1.25f, 1.55f), new Vector2(0f, -0.2f));
        Transform sword = HW_Obj(r, "Sword", new Vector2(0.95f, -4.05f), "v79_a_sword", 11);
        sword.localRotation = Quaternion.Euler(0f, 0f, 18f);
        BoxCollider2D swordCol = HW_Box(sword, new Vector2(1.1f, 0.75f));
        Transform sling = HW_Obj(r, "Slingshot", new Vector2(1.95f, -3.8f), "v79_a_slingshot", 11);
        sling.localRotation = Quaternion.Euler(0f, 0f, -12f);
        BoxCollider2D slingCol = HW_Box(sling, new Vector2(0.9f, 1.0f));

        // ---- il fumetto del mostro (punta accanto alla testa, a destra; si adatta al testo e resta nello schermo) ----
        SpeechBubble bubble = Bubble(r, new Vector2(1.0f + 0.85f, 0.7f + 0.8f));

        HW_Goal(r, "DEFEAT THE MONSTER.");
        TextMesh info = HW_Toast(r, new Vector2(0f, 2.45f), out Transform infoHolder);
        SetField(level, "monster", monster);
        SetField(level, "monsterArea", monsterCol);
        SetField(level, "monsterBody", body);
        SetField(level, "armLeft", armL);
        SetField(level, "armRight", armR);
        SetField(level, "browLeft", browL);
        SetField(level, "browRight", browR);
        SetField(level, "blush", blush);
        SetField(level, "shyArms", shy);
        SetField(level, "anger", anger);
        SetFloat(level, "pinY", DoodleSprites.V79APinY);
        SetFloat(level, "exitX", 3.8f);
        SetField(level, "medal", medal);
        SetField(level, "medalArea", medalCol);
        SetField(level, "medalText", medalText);
        SetField(level, "sword", sword);
        SetField(level, "swordArea", swordCol);
        SetField(level, "slingshot", sling);
        SetField(level, "slingshotArea", slingCol);
        SetField(level, "monsterBubble", bubble);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r));
        return SaveLevel(root, "V79ShyMonster");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V79A = new Dictionary<string, string[]>
    {
        { "shy_monster", new[] { "Everyone likes to be appreciated.",
            "Drag the BEST MONSTER medal onto the monster's chest." } },
    };
}
#endif
