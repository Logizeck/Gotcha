#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.74, livello 273 (gruppo A): il salvadanaio. La cameretta (v74_a_room_bg, con la scrivania e la
/// finestra sulla strada): il salvadanaio Mr. Oink sulla scrivania (con la fessura, il tappo di gomma sotto la pancia e
/// il buco che lascia), le monetine raccolte e il contatore, il coltello da burro e il martello (v69_cp_hammer) sul
/// tappeto, l'orsetto (teddy_bear), il bambino (preoccupato / "NO!" / esulta), il biglietto
/// (sticky_note) "Ice cream truck in 30 seconds!", il furgone dei gelati che arriva nella finestra nel finale, la
/// pioggia di monete (coin) e il mucchio. Riusa gli aiuti di Halloween/0.62 (HW_Goal, HW_Obj, HW_Box, HW_Area,
/// V62_Wire); i disegni nuovi (prefisso v74_a_) sono in DoodleSpritesV74A. Niente Easter egg e niente Toni. L'elenco
/// dei livelli (Define) lo aggiunge il capo progetto in GotchaProjectBuilder.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 273 The Piggy Bank ----------

    static GameObject BuildV74PiggyBank()
    {
        var root = new GameObject("Level_V74PiggyBank");
        V74PiggyBankLevel level = root.AddComponent<V74PiggyBankLevel>();
        Transform r = root.transform;
        Art(r, "Room", "v74_a_room_bg", -20, false);

        // ---- il biglietto sul muro e la finestra (il furgone arriva solo nel finale) ----
        Transform note = HW_Obj(r, "Note", new Vector2(-1.3f, 1.55f), "sticky_note", 3, 0.62f);
        note.localRotation = Quaternion.Euler(0f, 0f, 4f);
        TextMesh noteText = Label(note, new Vector2(0f, -0.04f), 4, "ICE CREAM TRUCK\nIN 30 SECONDS!", 0.03f);
        noteText.color = InkColor;
        Fit(noteText, 1.55f, 0.75f);
        BoxCollider2D noteCol = HW_Box(note, new Vector2(2.1f, 1.5f));
        BoxCollider2D windowCol = HW_Area(r, "WindowArea", new Vector2(1.9f, 1.6f), new Vector2(1.15f, 1.5f));
        Transform truck = HW_Obj(r, "Truck", new Vector2(1.6f, 0.88f), "v74_a_icecream_truck", -10, 0.55f);
        truck.gameObject.SetActive(false);

        // ---- il salvadanaio sulla scrivania: fessura, tappo (sotto la pancia), buco ----
        Transform piggy = HW_Obj(r, "Piggy", new Vector2(0.3f, -0.24f), "v74_a_piggy", 6, 1.2f);
        BoxCollider2D piggyCol = HW_Area(r, "PiggyArea", new Vector2(2.6f, 1.9f), new Vector2(0.35f, -0.1f));
        BoxCollider2D slotCol = HW_Area(piggy, "SlotArea", new Vector2(0.9f, 0.5f), new Vector2(-0.05f, 0.6f));
        Transform slotMouth = Child(piggy, "SlotMouth", new Vector2(-0.05f, 0.62f)).transform;
        Transform plug = HW_Obj(piggy, "Plug", new Vector2(0.05f, -0.49f), "v74_a_plug", 5);
        plug.gameObject.SetActive(false);
        BoxCollider2D plugCol = HW_Area(piggy, "PlugArea", new Vector2(1.0f, 0.9f), new Vector2(0.05f, -0.65f));
        Transform hole = HW_Obj(piggy, "PlugHole", new Vector2(0.05f, -0.49f), "v74_a_plug_hole", 7);
        hole.gameObject.SetActive(false);
        Transform plugRest = Child(r, "PlugRest", new Vector2(1.9f, -0.86f)).transform;

        // ---- le monetine raccolte, il contatore, la monetina che esce, la pioggia e il mucchio ----
        Vector2[] stackAt =
        {
            new Vector2(1.72f, -0.89f), new Vector2(1.93f, -0.89f), new Vector2(2.14f, -0.89f),
            new Vector2(1.82f, -0.71f), new Vector2(2.03f, -0.71f), new Vector2(1.93f, -0.53f),
        };
        var stack = new Object[stackAt.Length];
        for (int i = 0; i < stackAt.Length; i++)
        {
            Transform c = HW_Obj(r, "Coin" + i, stackAt[i], "coin", 7, 0.5f);
            c.gameObject.SetActive(false);
            stack[i] = c;
        }
        TextMesh cents = Label(r, new Vector2(1.93f, -0.3f), 8, "0 CENTS", 0.03f);
        cents.color = InkColor;
        Fit(cents, 0.6f, 0.22f);
        cents.gameObject.SetActive(false);
        Transform drop = HW_Obj(r, "DropCoin", Vector2.zero, "coin", 9, 0.5f);
        drop.gameObject.SetActive(false);
        var shower = new Object[14];
        for (int i = 0; i < shower.Length; i++)
        {
            Transform c = HW_Obj(r, "Shower" + i, Vector2.zero, "coin", 10, 0.55f);
            c.gameObject.SetActive(false);
            shower[i] = c;
        }
        Transform pile = HW_Obj(r, "CoinPile", new Vector2(0.3f, -1.0f), "v74_a_coin_pile", 5, 1.1f);
        pile.gameObject.SetActive(false);
        Vector2[] sp = { new Vector2(-0.4f, -0.65f), new Vector2(1.0f, -0.6f), new Vector2(0.3f, -0.45f) };
        var sparkles = new Object[sp.Length];
        for (int i = 0; i < sp.Length; i++)
        {
            Transform s = HW_Obj(r, "Sparkle" + i, sp[i], "sparkle_star", 12, 0.6f);
            s.gameObject.SetActive(false);
            sparkles[i] = s;
        }

        // ---- il bambino e il "NO!" ----
        Transform kid = Child(r, "Kid", new Vector2(-1.6f, -3.1f)).transform;
        kid.localScale = Vector3.one * 1.2f;
        SpriteRenderer kidArt = Art(kid, "Art", "v74_a_kid_worried", 8, false).GetComponent<SpriteRenderer>();
        BoxCollider2D kidCol = HW_Area(r, "KidArea", new Vector2(1.5f, 3.1f), new Vector2(-1.55f, -1.6f));
        Transform no = HW_Obj(r, "NoBurst", new Vector2(0.35f, 1.0f), "v74_a_no_burst", 20);
        TextMesh noText = Label(no, Vector2.zero, 21, "NO!", 0.05f);
        noText.color = new Color(0.88f, 0.21f, 0.17f);
        Fit(noText, 0.75f, 0.4f);
        no.gameObject.SetActive(false);

        // ---- gli attrezzi sul tappeto: il coltello da burro e il martello; l'orsetto ----
        Transform knife = HW_Obj(r, "Knife", new Vector2(0.7f, -3.0f), "v74_a_butter_knife", 12);
        knife.localRotation = Quaternion.Euler(0f, 0f, 8f);
        BoxCollider2D knifeCol = HW_Box(knife, new Vector2(1.2f, 0.5f));
        Transform hammer = HW_Obj(r, "Hammer", new Vector2(-0.4f, -3.65f), "v69_cp_hammer", 12, 1.5f);
        hammer.localRotation = Quaternion.Euler(0f, 0f, 12f);
        BoxCollider2D hammerCol = HW_Box(hammer, new Vector2(1.2f, 0.6f));
        Transform teddy = HW_Obj(r, "Teddy", new Vector2(1.85f, -2.95f), "teddy_bear", 4, 0.9f);
        BoxCollider2D teddyCol = HW_Box(teddy, new Vector2(1.2f, 1.3f));

        HW_Goal(r, "GET THE MONEY OUT!");
        SetField(level, "piggy", piggy);
        SetField(level, "piggyArea", piggyCol);
        SetField(level, "slotArea", slotCol);
        SetField(level, "slotMouth", slotMouth);
        SetField(level, "plug", plug);
        SetField(level, "plugArea", plugCol);
        SetField(level, "plugHole", hole);
        SetField(level, "plugRest", plugRest);
        SetArray(level, "stackCoins", stack);
        SetField(level, "dropCoin", drop);
        SetArray(level, "showerCoins", shower);
        SetField(level, "coinPile", pile);
        SetArray(level, "sparkles", sparkles);
        SetField(level, "centsLabel", cents);
        SetField(level, "kid", kid);
        SetField(level, "kidArea", kidCol);
        SetField(level, "kidArt", kidArt);
        SetField(level, "kidWorried", sprites["v74_a_kid_worried"]);
        SetField(level, "kidNo", sprites["v74_a_kid_no"]);
        SetField(level, "kidCheer", sprites["v74_a_kid_cheer"]);
        SetField(level, "noBurst", no);
        SetField(level, "hammer", hammer);
        SetField(level, "hammerArea", hammerCol);
        SetField(level, "knife", knife);
        SetField(level, "knifeArea", knifeCol);
        SetField(level, "teddy", teddy);
        SetField(level, "teddyArea", teddyCol);
        SetField(level, "note", note);
        SetField(level, "noteArea", noteCol);
        SetField(level, "windowArea", windowCol);
        SetField(level, "truck", truck);
        V62_Wire(level, r, new Vector2(0f, -4.15f));
        return SaveLevel(root, "V74PiggyBank");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V74A = new Dictionary<string, string[]>
    {
        { "v74_piggy_bank", new[] { "Look at the piggy from every side.",
            "Turn it over: there's a plug underneath." } },
    };
}
#endif
