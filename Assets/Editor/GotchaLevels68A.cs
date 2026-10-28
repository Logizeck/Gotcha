#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.68, livello 263 (gruppo A): la barretta incastrata. Riusa il corridoio (hall_bg), la moneta, la
/// caramella, il biscotto, la mela, il bagliore, lo sbuffo e gli aiuti di Halloween/0.62 (HW_Goal, HW_Obj, HW_Box,
/// HW_Area, V62_Wire); i disegni nuovi (prefisso v68_: distributore, vetro, spirale, barretta, patatine, sportello,
/// adesivo, falena, mano, braccio) sono in DoodleSpritesV68A. Niente Easter egg e niente Toni.
/// L'elenco dei livelli (Define) lo aggiunge il capo progetto in GotchaProjectBuilder.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 263 The Stuck Snack ----------

    static GameObject BuildV68StuckSnack()
    {
        var root = new GameObject("Level_V68StuckSnack");
        V68StuckSnackLevel level = root.AddComponent<V68StuckSnackLevel>();
        Transform r = root.transform;
        Art(r, "Hall", "hall_bg", -20, false);

        // ---- il distributore: perno in basso al centro (dondola), dentro "Body" le coordinate locali = quelle del mondo ----
        Vector2 pivotAt = new Vector2(-0.35f, -3.15f);
        Transform pivot = Child(r, "Machine", pivotAt).transform;
        Transform body = Child(pivot, "Body", -pivotAt).transform;
        Art(body, "Cabinet", "v68_vending", 0, false);
        BoxCollider2D bodyCol = HW_Box(body, new Vector2(3.7f, 5.95f), new Vector2(-0.35f, -0.175f));
        TextMesh brand = Label(body, new Vector2(-0.35f, 2.53f), 1, "SNACKS", 0.05f);
        brand.color = new Color(0.62f, 0.16f, 0.13f);
        Fit(brand, 3.0f, 0.3f);

        // ---- gli scomparti: righe A, B, C (dall'alto), colonne 1-3; la barretta viola è in A3 e (incastrata) in B3 ----
        const float WinL = -2.0f, WinR = 0.62f, WinB = -0.75f, WinT = 2.2f;
        float colW = (WinR - WinL) / 3f, rowH = (WinT - WinB) / 3f;
        string[] art = { "v68_chips", "hw_candy", "v68_choco", "apple", "v68_chips", "v68_choco", "hw_candy", "hw_biscuit", "v68_chips" };
        float[] scale = { 1.1f, 1.65f, 1.22f, 0.34f, 1.1f, 1.22f, 1.65f, 0.95f, 1.1f };
        float[] lift = { 0.56f, 0.47f, 0.48f, 0.52f, 0.56f, 0.48f, 0.47f, 0.52f, 0.56f };
        var snacks = new Transform[9];
        var coils = new Transform[9];
        var cells = new Collider2D[9];
        for (int row = 0; row < 3; row++)
            for (int col = 0; col < 3; col++)
            {
                int i = row * 3 + col;
                float cx = WinL + colW * (col + 0.5f), by = WinT - rowH * (row + 1);
                string slot = (char)('A' + row) + (col + 1).ToString();
                coils[i] = HW_Obj(body, "Coil_" + slot, new Vector2(cx, by + 0.28f), "v68_coil", 2, 1f);
                snacks[i] = HW_Obj(body, "Snack_" + slot, new Vector2(cx, by + lift[i]), art[i], 3, scale[i]);
                TextMesh tag = Label(body, new Vector2(cx - 0.13f, by + 0.08f), 4, slot, 0.022f);
                Fit(tag, 0.3f, 0.09f);
                // tutto lo scomparto (barretta + cartellino + bottoncino) è il tasto
                cells[i] = HW_Area(body, "Cell_" + slot, new Vector2(colW - 0.03f, rowH - 0.03f), new Vector2(cx, by + rowH * 0.5f));
            }
        Art(body, "Glass", "v68_glass", 6, false);

        // ---- il pannello: display, fessura della moneta, vaschetta del resto ----
        TextMesh display = Label(body, new Vector2(1.07f, 1.95f), 2, "INSERT\nCOIN", 0.03f);
        display.color = new Color(0.45f, 1f, 0.55f);
        Fit(display, 0.48f, 0.28f);
        TextMesh coinLbl = Label(body, new Vector2(1.07f, 1.08f), 2, "COIN", 0.022f);
        Fit(coinLbl, 0.5f, 0.12f);
        Transform slotT = Child(body, "CoinSlot", new Vector2(1.07f, 0.6f)).transform;
        BoxCollider2D slotCol = HW_Box(slotT, new Vector2(0.72f, 0.9f));
        TextMesh changeLbl = Label(body, new Vector2(1.07f, -0.07f), 2, "CHANGE", 0.02f);
        Fit(changeLbl, 0.5f, 0.1f);
        Transform cup = Child(body, "ChangeCup", new Vector2(1.07f, -0.38f)).transform;
        BoxCollider2D cupCol = HW_Box(cup, new Vector2(0.72f, 0.6f));

        // ---- la parte bassa davanti alle barrette che cadono, lo sportello, l'adesivo ----
        Art(body, "Low", "v68_vending_low", 8, false);
        SpriteRenderer flapGlow = Art(body, "FlapGlow", "glow", 9, false, new Vector2(-0.7f, -1.9f)).GetComponent<SpriteRenderer>();
        flapGlow.transform.localScale = new Vector3(2.4f, 1.1f, 1f);
        flapGlow.color = new Color(1f, 0.92f, 0.5f, 0.5f);
        Transform flap = Child(body, "Flap", new Vector2(-0.7f, -1.42f)).transform;
        Art(flap, "Art", "v68_flap", 10, false);
        TextMesh push = Label(flap, new Vector2(0f, -0.5f), 11, "PUSH", 0.04f);
        push.color = new Color(0.78f, 0.8f, 0.86f);
        Fit(push, 1.0f, 0.25f);
        BoxCollider2D flapCol = HW_Area(body, "FlapArea", new Vector2(2.7f, 1.4f), new Vector2(-0.7f, -1.9f));
        SmokeEmitter dust = Emitter(body, "Dust", new Vector2(-0.7f, -2.2f), "puff", 18f, 0.5f, 0.5f, 0.9f, 0.25f, new Color(0.85f, 0.82f, 0.78f, 0.8f), 12);
        dust.growth = 1.2f;
        Transform stickerT = HW_Obj(body, "Sticker", new Vector2(1.06f, -1.78f), "v68_sticker", 10, 1.15f);
        stickerT.localRotation = Quaternion.Euler(0f, 0f, -4f);
        SpriteRenderer stickerArt = stickerT.GetComponentInChildren<SpriteRenderer>();
        TextMesh warn = Label(stickerT, new Vector2(0f, -0.1f), 11, "DO NOT ROCK\nTHE MACHINE", 0.02f);
        Fit(warn, 0.46f, 0.26f);

        // ---- la mano del giocatore con UNA moneta ----
        Transform palm = HW_Obj(r, "Palm", new Vector2(2.0f, -3.05f), "v68_palm", 20, 1f);
        BoxCollider2D palmCol = HW_Box(palm, new Vector2(1.2f, 0.7f), new Vector2(0.1f, -0.15f));
        Transform coin = HW_Obj(r, "Coin", new Vector2(1.93f, -2.86f), "coin", 21, 0.9f);
        CircleCollider2D coinCol = coin.gameObject.AddComponent<CircleCollider2D>();
        coinCol.radius = 0.55f;

        // ---- il braccio che si infila nello sportello (con le due barrette alla fine) e la falena ----
        Transform arm = HW_Obj(r, "Arm", new Vector2(-0.7f, -1.95f), "v68_arm", 9, 1f);
        Transform loot = Child(arm, "Loot", new Vector2(0f, 0.25f)).transform;
        HW_Obj(loot, "BarA", new Vector2(-0.12f, 0.06f), "v68_choco", 11, 0.9f).localRotation = Quaternion.Euler(0f, 0f, 70f);
        HW_Obj(loot, "BarB", new Vector2(0.14f, 0.02f), "v68_choco", 11, 0.9f).localRotation = Quaternion.Euler(0f, 0f, 105f);
        Transform moth = HW_Obj(r, "Moth", new Vector2(1.07f, -0.38f), "v68_moth", 30, 0.8f);

        HW_Goal(r, "GET THE SNACK.");
        SetField(level, "machinePivot", pivot);
        SetField(level, "bodyArea", bodyCol);
        SetArray(level, "snacks", snacks);
        SetArray(level, "coils", coils);
        SetArray(level, "cellAreas", cells);
        SetField(level, "display", display);
        SetField(level, "sticker", stickerArt);
        SetField(level, "coinSlot", slotT);
        SetField(level, "coinSlotArea", slotCol);
        SetField(level, "changeCup", cup);
        SetField(level, "changeArea", cupCol);
        SetField(level, "flapPivot", flap);
        SetField(level, "flapArea", flapCol);
        SetField(level, "flapGlow", flapGlow);
        SetField(level, "dust", dust);
        SetField(level, "coin", coin);
        SetField(level, "coinArea", coinCol);
        SetField(level, "palm", palm);
        SetField(level, "palmArea", palmCol);
        SetField(level, "arm", arm);
        SetField(level, "loot", loot);
        SetField(level, "moth", moth);
        V62_Wire(level, r, new Vector2(0f, -4.15f));
        return SaveLevel(root, "V68StuckSnack");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V68A = new Dictionary<string, string[]>
    {
        { "v68_stuck_snack", new[] { "Look at what's right above it.",
            "Gravity can push too. Buy the same snack again, from the row above." } },
    };
}
#endif
