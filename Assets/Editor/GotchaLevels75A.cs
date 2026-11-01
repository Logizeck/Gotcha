#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.75, livello 275 (gruppo A): la poltrona di Toni (un livello di Toni). Il soggiorno (v75_a_living_bg),
/// due poltrone identiche (v75_a_armchair) col tavolino in mezzo (v75_a_side_table) e la tazza (v75_a_mug), il
/// telecomando (v72_b_remote) sul bracciolo della tua, la TV (v72_b_tv) sul mobiletto (v72_b_cabinet) col pre-partita
/// (v75_a_tv_prematch) e il conto alla rovescia, Toni costruito col solito rig (ToniRig: pose, occhi, pupille, bocca,
/// lingua, coda, fumetto, Zzz, cuoricini, briciole) più due sovrapposizioni che non cambiano il rig (l'orecchio
/// v75_a_toni_ear e la palpebra v75_a_toni_lid); sul tappeto il cuscino "YOU" (v75_a_you_cushion), la coperta
/// (v75_a_blanket), l'aspirapolvere (v75_a_vacuum, con le onde v75_a_noise), la scatola dei biscotti (hw_biscuit_box,
/// col biscotto "cookie") e la paperella (v75_a_squeaky). Riusa gli aiuti di Halloween/0.62 (HW_Goal, HW_Obj, HW_Box,
/// HW_Area, V62_Wire). Niente Easter egg. L'elenco dei livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 275 Toni's Armchair ----------

    static GameObject BuildV75ToniArmchair()
    {
        var root = new GameObject("Level_V75ToniArmchair");
        V75ToniArmchairLevel level = root.AddComponent<V75ToniArmchairLevel>();
        Transform r = root.transform;
        Art(r, "Room", "v75_a_living_bg", -20, false);

        // ---- la TV sul mobiletto, col pre-partita e il conto alla rovescia ----
        HW_Obj(r, "Cabinet", new Vector2(0f, -0.71f), "v72_b_cabinet", -9, 0.7f);
        Transform tv = HW_Obj(r, "Tv", new Vector2(0f, 0.39f), "v72_b_tv", -8, 0.75f);
        Transform screen = Child(tv, "Screen", new Vector2(-0.04f, 0.08f)).transform;
        Art(screen, "Art", "v75_a_tv_prematch", -7, false);
        TextMesh live = Label(screen, new Vector2(-0.74f, 0.55f), -6, "LIVE", 0.022f);
        live.color = Color.white;
        Fit(live, 0.26f, 0.11f);
        TextMesh tvTitle = Label(screen, new Vector2(0f, 0.26f), -6, "THE MATCH\nSTARTS IN", 0.04f);
        tvTitle.color = Color.white;
        Fit(tvTitle, 1.8f, 0.6f);
        TextMesh tvNumber = Label(screen, new Vector2(0f, -0.33f), -6, "60", 0.09f);
        tvNumber.color = new Color(1f, 0.9f, 0.4f);
        Fit(tvNumber, 1.4f, 0.62f);
        BoxCollider2D tvCol = HW_Area(r, "TvArea", new Vector2(2.5f, 2.0f), new Vector2(0f, 0.45f));

        // ---- le due poltrone identiche e il tavolino ----
        const float chairY = -2.85f, chairX = 1.3f;
        HW_Obj(r, "LeftChair", new Vector2(-chairX, chairY), "v75_a_armchair", 0);
        HW_Obj(r, "RightChair", new Vector2(chairX, chairY), "v75_a_armchair", 0);
        float seatY = chairY + DoodleSprites.V75ASeatY;
        Transform leftSeat = Child(r, "LeftSeat", new Vector2(-chairX, seatY)).transform;
        Transform rightSeat = Child(r, "RightSeat", new Vector2(chairX, seatY)).transform;
        BoxCollider2D leftCol = HW_Area(r, "LeftChairArea", new Vector2(2.0f, 2.0f), new Vector2(-chairX, chairY + 0.95f));
        BoxCollider2D rightCol = HW_Area(r, "RightChairArea", new Vector2(2.0f, 2.0f), new Vector2(chairX, chairY + 0.95f));
        HW_Obj(r, "SideTable", new Vector2(0f, chairY), "v75_a_side_table", 1);
        Transform mug = HW_Obj(r, "Mug", new Vector2(0f, chairY + 0.92f), "v75_a_mug", 4);
        BoxCollider2D mugCol = HW_Box(mug, new Vector2(0.6f, 0.65f), new Vector2(0.02f, 0.05f));
        Transform remote = HW_Obj(r, "Remote", new Vector2(-chairX + 0.8f, chairY + 1.12f), "v72_b_remote", 6, 0.45f);
        remote.localRotation = Quaternion.Euler(0f, 0f, 8f);
        BoxCollider2D remoteCol = HW_Box(remote, new Vector2(1.5f, 1.1f));

        // ---- Toni, col suo rig, addormentato sulla TUA poltrona ----
        ToniPug toni = ToniRig(r, new Vector2(-chairX, seatY + 0.2f), 0.6f, r);
        Transform ear = HW_Obj(toni.transform, "TwitchEar", new Vector2(DoodleSprites.V75AEarX, DoodleSprites.V75AEarY), "v75_a_toni_ear", 3);
        ear.gameObject.SetActive(false);
        Transform lid = HW_Obj(toni.transform, "OneEyeLid", new Vector2(DoodleSprites.PugEyeLX, DoodleSprites.PugEyeY), "v75_a_toni_lid", 5);
        lid.gameObject.SetActive(false);

        // ---- sul tappeto: tu (il cuscino "YOU") e le cose della stanza ----
        Transform you = HW_Obj(r, "You", new Vector2(0.05f, -3.4f), "v75_a_you_cushion", 10);
        TextMesh youText = Label(you, new Vector2(0f, -0.02f), 11, "YOU", 0.07f);
        youText.color = new Color(0.36f, 0.2f, 0.12f);
        Fit(youText, 0.95f, 0.42f);
        BoxCollider2D youCol = HW_Box(you, new Vector2(1.5f, 1.1f));
        Transform vacuum = HW_Obj(r, "Vacuum", new Vector2(-1.95f, -4.05f), "v75_a_vacuum", 8);
        BoxCollider2D vacuumCol = HW_Box(vacuum, new Vector2(0.95f, 1.65f), new Vector2(0.03f, 0.78f));
        Transform noise = HW_Obj(vacuum, "Noise", new Vector2(0.3f, 0.6f), "v75_a_noise", 9);
        noise.gameObject.SetActive(false);
        Transform blanket = HW_Obj(r, "Blanket", new Vector2(-0.95f, -3.75f), "v75_a_blanket", 8);
        BoxCollider2D blanketCol = HW_Box(blanket, new Vector2(1.3f, 0.75f));
        Transform treats = HW_Obj(r, "Treats", new Vector2(1.15f, -3.6f), "hw_biscuit_box", 8, 0.55f);
        BoxCollider2D treatsCol = HW_Box(treats, new Vector2(1.6f, 1.4f));
        Transform cookie = HW_Obj(r, "Cookie", new Vector2(1.15f, -3.3f), "cookie", 12, 0.6f);
        cookie.gameObject.SetActive(false);
        Transform squeaky = HW_Obj(r, "Squeaky", new Vector2(1.9f, -3.5f), "v75_a_squeaky", 8, 0.9f);
        BoxCollider2D squeakyCol = HW_Box(squeaky, new Vector2(0.95f, 0.85f));

        HW_Goal(r, "GET YOUR ARMCHAIR BACK!");
        SetField(level, "toni", toni);
        SetField(level, "toniEar", ear);
        SetField(level, "toniLid", lid);
        SetField(level, "leftSeat", leftSeat);
        SetField(level, "rightSeat", rightSeat);
        SetField(level, "leftChairArea", leftCol);
        SetField(level, "rightChairArea", rightCol);
        SetField(level, "you", you);
        SetField(level, "youArea", youCol);
        SetField(level, "treats", treats);
        SetField(level, "treatsArea", treatsCol);
        SetField(level, "cookie", cookie);
        SetField(level, "squeaky", squeaky);
        SetField(level, "squeakyArea", squeakyCol);
        SetField(level, "vacuum", vacuum);
        SetField(level, "vacuumArea", vacuumCol);
        SetField(level, "noise", noise);
        SetField(level, "blanket", blanket);
        SetField(level, "blanketArea", blanketCol);
        SetField(level, "mug", mug);
        SetField(level, "mugArea", mugCol);
        SetField(level, "remote", remote);
        SetField(level, "remoteArea", remoteCol);
        SetField(level, "tvArea", tvCol);
        SetField(level, "tvTitle", tvTitle);
        SetField(level, "tvNumber", tvNumber);
        V62_Wire(level, r, new Vector2(0f, -4.5f));
        return SaveLevel(root, "V75ToniArmchair");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V75A = new Dictionary<string, string[]>
    {
        { "v75_toni_armchair", new[] { "Watch what happens when something lands on the other armchair.",
            "Sit in the other armchair. Toni will want it." } },
    };
}
#endif
