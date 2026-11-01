#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Levels;

/// <summary>
/// Versione 0.75, livello 276 (gruppo B): la bici legata a un palo corto. Una strada: la facciata di mattoni, il
/// marciapiede, il palo mozzo (la cima in y -0.62) col piccione sopra, il cartello caduto per terra col pezzo di palo, la
/// bici (ruote a parte per farle girare) con la catena attorno al palo e al tubo sella (metà dietro al palo, metà
/// davanti) e il lucchetto a combinazione (4 cifre di testo), il mazzo di chiavi e la tronchese sul marciapiede, il vigile
/// nascosto fuori dallo schermo a sinistra (mu_guard: si affaccia dopo il 2° colpo di tronchese, arriva al 3°).
/// Riusa gli aiuti di Halloween/0.62 (HW_Goal, HW_Obj, HW_Box, HW_Area, V62_Wire), mu_guard, glow e sparkle_star; i
/// disegni nuovi (prefisso v75_b_) sono in DoodleSpritesV75B. Niente Easter egg e niente Toni. L'elenco dei livelli
/// (Define) lo aggiunge il capo progetto in GotchaProjectBuilder.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 276 The Locked Bike ----------

    static GameObject BuildV75LockedBike()
    {
        var root = new GameObject("Level_V75LockedBike");
        V75LockedBikeLevel level = root.AddComponent<V75LockedBikeLevel>();
        Transform r = root.transform;
        Art(r, "Street", "v75_b_street_bg", -20, false);

        // ---- il palo mozzo (origine alla base; cima in y +1.78) ----
        Transform pole = HW_Obj(r, "Pole", new Vector2(-0.1f, -2.4f), "v75_b_pole", 5);
        BoxCollider2D poleCol = HW_Area(pole, "PoleArea", new Vector2(0.7f, 1.1f), new Vector2(0f, 1.45f));
        Transform poleTop = Child(pole, "Top", new Vector2(0f, 1.78f)).transform;

        // ---- il piccione sulla cima ----
        Transform pigeon = Child(r, "Pigeon", new Vector2(-0.1f, -0.62f)).transform;
        Transform pigeonSit = Art(pigeon, "Sit", "v75_b_pigeon", 7, false);
        Transform pigeonFly = Art(pigeon, "Fly", "v75_b_pigeon_fly", 7, false);
        pigeonFly.gameObject.SetActive(false);
        BoxCollider2D pigeonCol = HW_Area(pigeon, "PigeonArea", new Vector2(0.8f, 0.75f), new Vector2(0f, 0.25f));

        // ---- il cartello caduto, sdraiato per terra a sinistra (il pezzo di palo spezzato guarda verso il palo) ----
        Transform sign = HW_Obj(r, "Sign", new Vector2(-1.62f, -2.3f), "v75_b_sign_fallen", 6);
        sign.localRotation = Quaternion.Euler(0f, 0f, 78f);
        BoxCollider2D signCol = HW_Box(sign, new Vector2(1.0f, 1.6f), new Vector2(0f, 0.05f));

        // ---- la bici (origine a terra fra le ruote) ----
        Transform bike = Child(r, "Bike", new Vector2(0.4f, -2.4f)).transform;
        Transform wheelRear = HW_Obj(bike, "WheelRear", new Vector2(-0.85f, 0.5f), "v75_b_wheel", 9);
        Transform wheelFront = HW_Obj(bike, "WheelFront", new Vector2(0.85f, 0.5f), "v75_b_wheel", 9);
        Art(bike, "Frame", "v75_b_bike", 10, false);
        BoxCollider2D bikeCol = HW_Area(bike, "BikeArea", new Vector2(2.9f, 1.75f), new Vector2(0f, 0.8f));
        // la catena: perno al centro dell'anello, le due metà disegnate in coordinate della bici
        Transform chainPivot = Child(bike, "Chain", new Vector2(-0.32f, 0.85f)).transform;
        Transform chainBack = Art(chainPivot, "Back", "v75_b_chain_back", 4, false, new Vector2(0.32f, -0.85f));
        Art(chainPivot, "Front", "v75_b_chain_front", 12, false, new Vector2(0.32f, -0.85f));
        Transform padlock = HW_Obj(bike, "Padlock", new Vector2(-0.32f, 0.77f), "v75_b_padlock", 13);
        var digits = new Object[4];
        for (int i = 0; i < 4; i++)
        {
            TextMesh d = Label(padlock, new Vector2(-0.165f + 0.11f * i, -0.365f), 14, "0", 0.028f);
            d.color = new Color(0.17f, 0.13f, 0.2f);
            Fit(d, 0.08f, 0.11f);
            digits[i] = d;
        }
        BoxCollider2D lockCol = HW_Area(bike, "LockArea", new Vector2(1.15f, 0.95f), new Vector2(-0.25f, 0.55f));

        // ---- gli attrezzi sul marciapiede ----
        Transform keys = HW_Obj(r, "Keys", new Vector2(-1.5f, -2.95f), "v75_b_keychain", 20);
        BoxCollider2D keysCol = HW_Box(keys, new Vector2(1.3f, 1.05f), new Vector2(0f, -0.28f));
        Transform cutters = Child(r, "Cutters", new Vector2(0.7f, -3.1f)).transform;
        cutters.localRotation = Quaternion.Euler(0f, 0f, 6f);
        Transform cuttersStraight = Art(cutters, "Straight", "v75_b_cutters", 20, false);
        Transform cuttersBent = Art(cutters, "Bent", "v75_b_cutters_bent", 20, false);
        cuttersBent.gameObject.SetActive(false);
        BoxCollider2D cuttersCol = HW_Box(cutters, new Vector2(1.8f, 0.7f), new Vector2(0.35f, 0f));

        // ---- il vigile (fuori schermo a sinistra; si affaccia, poi arriva) ----
        Transform officer = HW_Obj(r, "Officer", new Vector2(-3.9f, -2.75f), "mu_guard", 26, 0.95f);
        Transform officerPeek = Child(r, "OfficerPeek", new Vector2(-2.55f, -2.75f)).transform;
        Transform officerIn = Child(r, "OfficerIn", new Vector2(-1.65f, -2.75f)).transform;

        // ---- la festa ----
        SpriteRenderer glow = Art(r, "Glow", "glow", 3, false, new Vector2(-0.1f, -0.4f)).GetComponent<SpriteRenderer>();
        glow.transform.localScale = Vector3.one * 1.1f;
        glow.color = new Color(1f, 0.95f, 0.6f, 0f);
        Transform sparkle = HW_Obj(r, "Sparkle", new Vector2(0.15f, -0.35f), "sparkle_star", 30, 0.8f);
        sparkle.gameObject.SetActive(false);

        HW_Goal(r, "TAKE YOUR BIKE HOME!");
        SetField(level, "bike", bike);
        SetField(level, "bikeArea", bikeCol);
        SetField(level, "wheelRear", wheelRear);
        SetField(level, "wheelFront", wheelFront);
        SetField(level, "chainPivot", chainPivot);
        SetField(level, "chainBack", chainBack);
        SetField(level, "padlock", padlock);
        SetField(level, "lockArea", lockCol);
        SetArray(level, "digits", digits);
        SetField(level, "pole", pole);
        SetField(level, "poleArea", poleCol);
        SetField(level, "poleTop", poleTop);
        SetField(level, "sign", sign);
        SetField(level, "signArea", signCol);
        SetField(level, "pigeon", pigeon);
        SetField(level, "pigeonSit", pigeonSit);
        SetField(level, "pigeonFly", pigeonFly);
        SetField(level, "pigeonArea", pigeonCol);
        SetField(level, "keys", keys);
        SetField(level, "keysArea", keysCol);
        SetField(level, "cutters", cutters);
        SetField(level, "cuttersStraight", cuttersStraight);
        SetField(level, "cuttersBent", cuttersBent);
        SetField(level, "cuttersArea", cuttersCol);
        SetField(level, "officer", officer);
        SetField(level, "officerPeek", officerPeek);
        SetField(level, "officerIn", officerIn);
        SetField(level, "glow", glow);
        SetField(level, "sparkle", sparkle);
        V62_Wire(level, r, new Vector2(0f, -4.15f));
        return SaveLevel(root, "V75LockedBike");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V75B = new Dictionary<string, string[]>
    {
        { "v75_locked_bike", new[] { "Look at the top of the pole.",
            "Lift the bike over the pole." } },
    };
}
#endif
