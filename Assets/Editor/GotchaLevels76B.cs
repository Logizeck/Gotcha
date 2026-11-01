#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Levels;

/// <summary>
/// Versione 0.76, livello 5 della roadmap (gruppo B): il leone fifone. La savana, il "leone" (tutto sotto Lion, che si
/// gonfia dai piedi quando ruggisce): il costume seduto, la testolina del gattino nascosta dietro la criniera, la criniera
/// trascinabile (col muso feroce, il muso che ruggisce, l'elastico e l'etichetta "COSTUME"), le onde del ruggito; il
/// bottone ROAR; il gattino spaventato e quello in fuga (spenti); la coppetta che cade dall'alto (s41e_trophy). La festa è
/// quella comune (RewardRig). Disegni nuovi (prefisso v76_b_) in DoodleSpritesV76B. Niente Easter egg e niente Toni.
/// L'elenco dei livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 5: The Cowardly Lion ----------

    static GameObject BuildV76CowardlyLion()
    {
        var root = new GameObject("Level_V76CowardlyLion");
        V76CowardlyLionLevel level = root.AddComponent<V76CowardlyLionLevel>();
        Transform r = root.transform;
        Art(r, "Savanna", "v76_b_savanna_bg", -20, false);

        // ---- il leone (origine a terra sotto di lui: si gonfia dai piedi) ----
        Transform lion = Child(r, "Lion", new Vector2(0f, -2.6f)).transform;
        lion.localScale = Vector3.one * 1.2f;   // scala di riposo (il livello la moltiplica quando ruggisce)
        Transform body = Child(lion, "Body", Vector2.zero).transform;
        Art(body, "Art", "v76_b_lion_body", 6, false);
        BoxCollider2D bodyCol = HW_Area(lion, "BodyArea", new Vector2(2.9f, 2.0f), new Vector2(0.15f, 0.95f));
        Transform kittenHead = HW_Obj(lion, "KittenHead", new Vector2(0f, 2.2f), "v76_b_kitten_head", 7);

        // ---- la criniera (col muso: è una maschera) ----
        Transform mane = Child(lion, "Mane", new Vector2(0f, 2.5f)).transform;
        Art(mane, "Art", "v76_b_mane", 9, false);
        Transform face = Art(mane, "Face", "v76_b_face", 10, false);
        Transform faceRoar = Art(mane, "FaceRoar", "v76_b_face_roar", 10, false);
        faceRoar.gameObject.SetActive(false);
        CircleCollider2D maneCol = mane.gameObject.AddComponent<CircleCollider2D>();
        maneCol.radius = 1.32f;
        CircleCollider2D faceCol = Child(mane, "FaceArea", Vector2.zero).AddComponent<CircleCollider2D>();
        faceCol.radius = 0.6f;
        Transform tag = Child(mane, "Tag", new Vector2(0.78f, -0.72f)).transform;
        tag.localScale = Vector3.one * 1.35f;
        Art(tag, "Art", "v76_b_tag", 12, false);
        TextMesh tagText = Label(tag, new Vector2(0.2f, -0.4f), 13, "COSTUME", 0.02f);
        tagText.color = new Color(0.17f, 0.13f, 0.2f);
        tagText.transform.localRotation = Quaternion.Euler(0f, 0f, -18f);
        Fit(tagText, 0.34f, 0.1f);
        SpriteRenderer waves = Art(mane, "RoarWaves", "ring_waves", 14, false).GetComponent<SpriteRenderer>();
        waves.transform.localScale = Vector3.one * 1.25f;
        waves.color = new Color(1f, 1f, 1f, 0f);

        // ---- il bottone ROAR ----
        Transform button = HW_Obj(r, "RoarButton", new Vector2(0f, -3.75f), "v76_b_roar_btn", 20);
        TextMesh roarText = Label(button, new Vector2(0f, 0.02f), 21, "ROAR!", 0.06f);
        roarText.color = Color.white;
        roarText.fontStyle = FontStyle.Bold;
        Fit(roarText, 1.3f, 0.42f);
        BoxCollider2D buttonCol = HW_Box(button, new Vector2(2.1f, 1.1f), new Vector2(0f, -0.04f));

        // ---- il gattino che si spaventa e scappa (spenti finché la criniera non viene via) ----
        Transform kittenScared = HW_Obj(r, "KittenScared", new Vector2(0f, -0.55f), "v76_b_kitten_scared", 15, 1.2f);
        kittenScared.gameObject.SetActive(false);
        Transform kittenRun = HW_Obj(r, "KittenRun", new Vector2(0.9f, -2.6f), "v76_b_kitten_run", 15, 1.2f);
        kittenRun.gameObject.SetActive(false);

        // ---- la coppetta che cade dall'alto dove sedeva il "re" ----
        Transform trophy = HW_Obj(r, "Trophy", new Vector2(0f, 5.6f), "s41e_trophy", 30, 1.2f);
        trophy.gameObject.SetActive(false);
        Transform trophyLanding = Child(r, "TrophyLanding", new Vector2(0f, -2.6f)).transform;

        HW_Goal(r, "SCARE THE LION.");
        TextMesh info = HW_Toast(r, new Vector2(0f, 2.62f), out Transform infoHolder);
        SetField(level, "rig", lion);
        SetField(level, "body", body);
        SetField(level, "bodyArea", bodyCol);
        SetField(level, "kittenHead", kittenHead);
        SetField(level, "mane", mane);
        SetField(level, "maneArea", maneCol);
        SetField(level, "faceArea", faceCol);
        SetField(level, "face", face);
        SetField(level, "faceRoar", faceRoar);
        SetField(level, "maneTag", tag);
        SetField(level, "roarWaves", waves);
        SetField(level, "button", button);
        SetField(level, "buttonArea", buttonCol);
        SetField(level, "kittenScared", kittenScared);
        SetField(level, "kittenRun", kittenRun);
        SetField(level, "trophy", trophy);
        SetField(level, "trophyLanding", trophyLanding);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r));
        return SaveLevel(root, "V76CowardlyLion");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V76B = new Dictionary<string, string[]>
    {
        { "cowardly_lion", new[] { "Take a closer look at that mane.",
            "Drag the mane off the lion." } },
    };
}
#endif
