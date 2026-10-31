#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.73, livello 271 (gruppo A): il cassetto bloccato. Una camera moderna: il cassettone bianco a frontali
/// lisci (tre cassetti; in quello in alto la maniglietta decorativa e le chiavi della macchina, in quello in mezzo un
/// calzino), lo spray lubrificante e la lampada (bedside_lamp + glow) sul piano, il foglietto (sticky_note) sul muro
/// con la scritta minuscola e la sua versione ingrandita, il tappeto con sopra l'asciugacapelli (s41i_dryer) e il
/// cacciavite. Riusa gli aiuti di Halloween/0.62 (HW_Goal, HW_Obj, HW_Box, HW_Area, V62_Wire); i disegni nuovi
/// (prefisso v73_a_) sono in DoodleSpritesV73A. Niente Easter egg e niente Toni. L'elenco dei livelli (Define) lo
/// aggiunge il capo progetto in GotchaProjectBuilder.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 271 The Stuck Drawer ----------

    static GameObject BuildV73StuckDrawer()
    {
        var root = new GameObject("Level_V73StuckDrawer");
        V73StuckDrawerLevel level = root.AddComponent<V73StuckDrawerLevel>();
        Transform r = root.transform;
        Art(r, "Bedroom", "v73_a_bedroom_bg", -20, false);
        Transform rug = HW_Obj(r, "Rug", new Vector2(0f, -3.2f), "v73_a_rug", 1);
        BoxCollider2D rugCol = HW_Box(rug, new Vector2(4.4f, 1.25f));

        // ---- il foglietto sul muro (scritta minuscola) e la sua versione ingrandita ----
        Transform note = HW_Obj(r, "Note", new Vector2(-1.3f, 1.0f), "sticky_note", 3, 0.45f);
        note.localRotation = Quaternion.Euler(0f, 0f, 4f);
        TextMesh tiny = Label(note, new Vector2(0f, -0.04f), 4, "No handles!\nPush to open :)", 0.02f);
        tiny.color = InkColor;
        Fit(tiny, 1.4f, 0.6f);
        BoxCollider2D noteCol = HW_Box(note, new Vector2(2.4f, 1.7f));
        Transform popup = Child(r, "NotePopup", new Vector2(0f, 0.3f)).transform;
        Bar(popup, "Dim", new Vector2(0f, -0.3f), new Vector2(7f, 12f), new Color(0.05f, 0.03f, 0.08f, 0.6f), 80);
        Transform big = HW_Obj(popup, "Big", new Vector2(0f, 0.2f), "sticky_note", 81, 2.3f);
        big.localRotation = Quaternion.Euler(0f, 0f, 4f);
        TextMesh line1 = Label(popup, new Vector2(0f, 0.55f), 82, "No handles!", 0.06f);
        line1.color = InkColor;
        Fit(line1, 3.2f, 0.55f);
        TextMesh line2 = Label(popup, new Vector2(0f, -0.15f), 82, "Push to open :)", 0.05f);
        line2.color = InkColor;
        Fit(line2, 3.4f, 0.5f);
        TextMesh closeHint = Label(popup, new Vector2(0f, -1.55f), 82, "TAP TO CLOSE", 0.025f);
        closeHint.color = Color.white;
        Fit(closeHint, 2f, 0.16f);
        popup.gameObject.SetActive(false);

        // ---- il cassettone: corpo, tre frontali (con l'interno e il luccichio), la maniglia ----
        Transform dresser = Child(r, "Dresser", new Vector2(0f, -1.2f)).transform;
        Art(dresser, "Body", "v73_a_dresser", 2, false);
        var fronts = new Object[3];
        var frontAreas = new Object[3];
        var boxes = new Object[3];
        var shines = new Object[3];
        Transform keys = null, anchor = null, holes = null;
        for (int i = 0; i < 3; i++)
        {
            float y = 0.56f - 0.56f * i;
            Transform f = HW_Obj(dresser, "Front" + i, new Vector2(0f, y), "v73_a_drawer_front", 10 - 2 * i);
            fronts[i] = f;
            frontAreas[i] = HW_Area(r, "FrontArea" + i, new Vector2(3.8f, 0.56f), new Vector2(0f, -1.2f + y));
            Transform box = HW_Obj(f, "Box", new Vector2(0f, 0.26f), "v73_a_drawer_box", 3);
            boxes[i] = box;
            SpriteRenderer shine = Art(f, "Shine", "v73_a_shine", 11 - 2 * i, false).GetComponent<SpriteRenderer>();
            shine.color = new Color(1f, 1f, 1f, 0.85f);
            shine.gameObject.SetActive(false);
            shines[i] = shine;
            if (i == 0)
            {
                keys = HW_Obj(box, "Keys", new Vector2(0.35f, 0.2f), "v73_a_car_keys", 4, 1.4f);
                anchor = Child(f, "HandleAnchor", Vector2.zero).transform;
                holes = HW_Obj(f, "HandleHoles", Vector2.zero, "v73_a_handle_holes", 11, 1.4f);
                holes.gameObject.SetActive(false);
            }
            else if (i == 1)
                HW_Obj(box, "Sock", new Vector2(-0.7f, 0.22f), "sock", 4, 0.45f).localRotation = Quaternion.Euler(0f, 0f, 75f);
            box.gameObject.SetActive(false);
        }
        BoxCollider2D bodyCol = HW_Area(r, "BodyArea", new Vector2(4.2f, 2.5f), new Vector2(0f, -1.15f));
        Transform handle = HW_Obj(r, "Handle", new Vector2(0f, -0.64f), "v73_a_handle", 12, 1.4f);
        BoxCollider2D handleCol = HW_Box(handle, new Vector2(0.62f, 0.38f));
        Transform handleRest = Child(r, "HandleRest", new Vector2(0.35f, -2.85f)).transform;

        // ---- le chiavi: area generosa, stelline, la scritta del clic ----
        BoxCollider2D keysCol = HW_Area(r, "KeysArea", new Vector2(3.8f, 1.5f), new Vector2(0f, -0.72f));
        Vector2[] sp = { new Vector2(0.0f, -0.3f), new Vector2(0.8f, -0.25f), new Vector2(0.55f, -0.7f) };
        var sparkles = new Object[sp.Length];
        for (int i = 0; i < sp.Length; i++)
        {
            Transform s = HW_Obj(r, "Sparkle" + i, sp[i], "sparkle_star", 13, 0.6f);
            s.gameObject.SetActive(false);
            sparkles[i] = s;
        }
        TextMesh click = Label(r, new Vector2(1.3f, -0.05f), 14, "CLICK!", 0.04f);
        click.color = new Color(0.88f, 0.21f, 0.17f);
        Fit(click, 1.0f, 0.3f);
        click.gameObject.SetActive(false);

        // ---- sul piano: lo spray e la lampada ----
        Transform lube = HW_Obj(r, "Lube", new Vector2(-1.45f, -0.27f), "v73_a_lube_can", 9, 0.85f);
        BoxCollider2D lubeCol = HW_Box(lube, new Vector2(0.8f, 1.0f), new Vector2(0.03f, 0.55f));
        SpriteRenderer mist = Art(r, "Mist", "v73_a_mist", 15, false, new Vector2(0f, -0.5f)).GetComponent<SpriteRenderer>();
        mist.color = new Color(1f, 1f, 1f, 0f);
        Transform lamp = HW_Obj(r, "Lamp", new Vector2(1.4f, -0.27f), "bedside_lamp", 8, 0.55f);
        BoxCollider2D lampCol = HW_Box(lamp, new Vector2(1.7f, 2.0f), new Vector2(0f, 0.85f));
        SpriteRenderer glow = Art(r, "LampGlow", "glow", 7, false, new Vector2(1.4f, 0.4f)).GetComponent<SpriteRenderer>();
        glow.transform.localScale = Vector3.one * 2.4f;
        glow.color = new Color(1f, 0.85f, 0.5f, 0f);

        // ---- sul tappeto: il cacciavite e l'asciugacapelli ----
        Transform sd = Child(r, "Screwdriver", new Vector2(-0.95f, -3.3f)).transform;
        sd.localRotation = Quaternion.Euler(0f, 0f, 10f);
        Transform straight = HW_Obj(sd, "Straight", Vector2.zero, "v73_a_screwdriver", 9);
        Transform bent = HW_Obj(sd, "Bent", Vector2.zero, "v73_a_screwdriver_bent", 9);
        bent.gameObject.SetActive(false);
        BoxCollider2D sdCol = HW_Box(sd, new Vector2(1.45f, 0.65f));
        Transform dryer = HW_Obj(r, "Dryer", new Vector2(1.8f, -3.15f), "s41i_dryer", 9, 0.6f);
        dryer.localRotation = Quaternion.Euler(0f, 0f, 90f);
        BoxCollider2D dryerCol = HW_Box(dryer, new Vector2(1.7f, 1.7f), new Vector2(0.17f, 0.7f));
        SpriteRenderer hotAir = Art(dryer, "HotAir", "v73_a_hot_air", 10, false).GetComponent<SpriteRenderer>();
        hotAir.transform.localScale = Vector3.one * 1.5f;
        hotAir.gameObject.SetActive(false);

        HW_Goal(r, "OPEN THE DRAWER!");
        SetField(level, "dresser", dresser);
        SetField(level, "bodyArea", bodyCol);
        SetArray(level, "fronts", fronts);
        SetArray(level, "frontAreas", frontAreas);
        SetArray(level, "boxes", boxes);
        SetArray(level, "shines", shines);
        SetField(level, "handle", handle);
        SetField(level, "handleArea", handleCol);
        SetField(level, "handleAnchor", anchor);
        SetField(level, "handleHoles", holes);
        SetField(level, "handleRest", handleRest);
        SetField(level, "keys", keys);
        SetField(level, "keysArea", keysCol);
        SetArray(level, "sparkles", sparkles);
        SetField(level, "clickLabel", click);
        SetField(level, "lube", lube);
        SetField(level, "lubeArea", lubeCol);
        SetField(level, "mist", mist);
        SetField(level, "screwdriver", sd);
        SetField(level, "screwdriverArea", sdCol);
        SetField(level, "screwdriverStraight", straight);
        SetField(level, "screwdriverBent", bent);
        SetField(level, "dryer", dryer);
        SetField(level, "dryerArea", dryerCol);
        SetField(level, "hotAir", hotAir);
        SetField(level, "lamp", lamp);
        SetField(level, "lampArea", lampCol);
        SetField(level, "lampGlow", glow);
        SetField(level, "note", note);
        SetField(level, "noteArea", noteCol);
        SetField(level, "notePopup", popup);
        SetField(level, "rug", rug);
        SetField(level, "rugArea", rugCol);
        V62_Wire(level, r, new Vector2(0f, -4.15f));
        return SaveLevel(root, "V73StuckDrawer");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V73A = new Dictionary<string, string[]>
    {
        { "v73_stuck_drawer", new[] { "Read the note on the wall.",
            "Sometimes you open things by pushing them." } },
    };
}
#endif
