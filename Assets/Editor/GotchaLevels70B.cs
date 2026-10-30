#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.70, livello 268 (gruppo B): il ketchup testardo. Una tavola calda: il piatto di patatine, il flacone di
/// ketchup senza marca (tappo con beccuccio, sigillo di alluminio con la linguetta che spunta, adesivo "NEW!"), il
/// coltello (hw_knife) e il tovagliolo. Riusa gli aiuti di Halloween/0.62 (HW_Goal, HW_Obj, HW_Box, HW_Area, V62_Wire),
/// lo sbuffo (puff) e il bagliore (glow); i disegni nuovi (prefisso v70_b_) sono in DoodleSpritesV70B.
/// Niente Easter egg e niente Toni. L'elenco dei livelli (Define) lo aggiunge il capo progetto in GotchaProjectBuilder.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 268 The Stubborn Ketchup ----------

    static GameObject BuildV70StubbornKetchup()
    {
        var root = new GameObject("Level_V70StubbornKetchup");
        V70StubbornKetchupLevel level = root.AddComponent<V70StubbornKetchupLevel>();
        Transform r = root.transform;
        Art(r, "Diner", "v70_b_diner_bg", -20, false);

        // ---- il piatto di patatine, la zona "sopra le patatine" e il contenitore delle gocce di ketchup ----
        Vector2 friesAt = new Vector2(-0.85f, -0.75f);
        Transform fries = HW_Obj(r, "Fries", friesAt, "v70_b_fries", 2, 1.15f);
        BoxCollider2D friesCol = HW_Box(fries, new Vector2(2.5f, 1.3f), new Vector2(0f, 0.05f));
        BoxCollider2D pourZone = HW_Area(r, "PourZone", new Vector2(3.1f, 4.2f), new Vector2(friesAt.x, 0.45f));
        Transform ketchup = Child(r, "Ketchup", friesAt).transform;
        Transform blob = HW_Obj(ketchup, "BlobTemplate", Vector2.zero, "v70_b_blob", 4, 1.2f);
        blob.gameObject.SetActive(false);

        // ---- il coltello e il tovagliolo ----
        Transform knife = HW_Obj(r, "Knife", new Vector2(-1.45f, -2.75f), "hw_knife", 3, 1.6f);
        knife.localRotation = Quaternion.Euler(0f, 0f, 20f);
        BoxCollider2D knifeCol = HW_Box(knife, new Vector2(1.35f, 0.5f));
        Transform knifeTip = Child(knife, "Tip", new Vector2(0.55f, 0f)).transform;
        Transform napkin = HW_Obj(r, "Napkin", new Vector2(1.25f, -2.65f), "v70_b_napkin", 1, 1f);
        napkin.localRotation = Quaternion.Euler(0f, 0f, -6f);
        BoxCollider2D napkinCol = HW_Box(napkin, new Vector2(1.4f, 1.0f));
        Transform capRest = Child(r, "CapRest", new Vector2(0.95f, -2.45f)).transform;
        Transform scrapRest = Child(r, "ScrapRest", new Vector2(1.6f, -2.8f)).transform;

        // ---- il flacone (coordinate locali del disegno: fondo a -1.0, bocca a 0.78) ----
        Transform bottle = Child(r, "Bottle", new Vector2(1.35f, -0.25f)).transform;
        bottle.localScale = Vector3.one * 1.25f;
        Transform body = HW_Obj(bottle, "Body", Vector2.zero, "v70_b_bottle", 5, 1f);
        BoxCollider2D bottleCol = HW_Box(bottle, new Vector2(0.85f, 1.9f), new Vector2(0f, -0.12f));
        BoxCollider2D bottomCol = HW_Area(bottle, "BottomArea", new Vector2(0.95f, 0.5f), new Vector2(0f, -0.86f));
        TextMesh brand = Label(body, new Vector2(0f, -0.58f), 9, "KETCHUP", 0.02f);
        brand.color = new Color(0.62f, 0.12f, 0.1f);
        Fit(brand, 0.55f, 0.1f);
        Transform sticker = HW_Obj(bottle, "Sticker", new Vector2(0.2f, 0.22f), "v70_b_sticker", 8, 1f);
        sticker.localRotation = Quaternion.Euler(0f, 0f, 12f);
        TextMesh fresh = Label(sticker, Vector2.zero, 9, "NEW!", 0.02f);
        fresh.color = new Color(0.85f, 0.15f, 0.12f);
        Fit(fresh, 0.28f, 0.12f);
        // il sigillo sulla bocca (sotto il tappo: spunta solo la linguetta) e il luccichio della linguetta
        Transform foil = HW_Obj(bottle, "Foil", new Vector2(0f, 0.78f), "v70_b_foil", 6, 1f);
        BoxCollider2D tabCol = HW_Area(bottle, "TabArea", new Vector2(0.34f, 0.42f), new Vector2(0.43f, 0.69f));
        BoxCollider2D sealCol = HW_Area(bottle, "SealArea", new Vector2(0.95f, 0.5f), new Vector2(0.12f, 0.76f));
        SpriteRenderer glint = Art(bottle, "Glint", "glow", 9, false, new Vector2(0.42f, 0.71f)).GetComponent<SpriteRenderer>();
        glint.transform.localScale = Vector3.one * 0.22f;
        glint.color = new Color(1f, 1f, 1f, 0f);
        // il tappo (figlio del flacone quando è avvitato; il livello lo sposta sul tavolo quando viene via)
        Transform cap = HW_Obj(bottle, "Cap", new Vector2(0f, 0.76f), "v70_b_cap", 7, 1f);
        CircleCollider2D capCol = cap.gameObject.AddComponent<CircleCollider2D>();
        capCol.radius = 0.36f;
        capCol.offset = new Vector2(0f, 0.02f);
        Transform capSocket = Child(bottle, "CapSocket", new Vector2(0f, 0.76f)).transform;
        Transform nozzle = Child(bottle, "Nozzle", new Vector2(0f, 1.04f)).transform;
        SmokeEmitter pfff = Emitter(nozzle, "Pfff", Vector2.zero, "puff", 16f, 0.45f, 0.5f, 0.25f, 0.18f, new Color(0.95f, 0.85f, 0.82f, 0.85f), 12);
        pfff.growth = 1.6f;

        // ---- il sigillo strappato e il filo di ketchup (nascosti all'inizio) ----
        Transform scrap = HW_Obj(r, "FoilScrap", new Vector2(1.6f, -2.8f), "v70_b_foil_scrap", 6, 1.15f);
        scrap.gameObject.SetActive(false);
        Transform stream = HW_Obj(r, "Stream", new Vector2(-0.85f, 0f), "v70_b_stream", 4, 1.2f);
        stream.gameObject.SetActive(false);

        HW_Goal(r, "PUT KETCHUP ON THE FRIES.");
        SetField(level, "fries", fries);
        SetField(level, "friesArea", friesCol);
        SetField(level, "pourZone", pourZone);
        SetField(level, "ketchup", ketchup);
        SetField(level, "blobTemplate", blob);
        SetField(level, "bottle", bottle);
        SetField(level, "bottleBody", body);
        SetField(level, "bottleArea", bottleCol);
        SetField(level, "bottomArea", bottomCol);
        SetField(level, "sticker", sticker);
        SetField(level, "foil", foil);
        SetField(level, "tabArea", tabCol);
        SetField(level, "sealArea", sealCol);
        SetField(level, "glint", glint);
        SetField(level, "cap", cap);
        SetField(level, "capArea", capCol);
        SetField(level, "capSocket", capSocket);
        SetField(level, "capRest", capRest);
        SetField(level, "nozzle", nozzle);
        SetField(level, "pfff", pfff);
        SetField(level, "scrap", scrap);
        SetField(level, "scrapRest", scrapRest);
        SetField(level, "stream", stream);
        SetField(level, "knife", knife);
        SetField(level, "knifeArea", knifeCol);
        SetField(level, "knifeTip", knifeTip);
        SetField(level, "napkin", napkin);
        SetField(level, "napkinArea", napkinCol);
        V62_Wire(level, r, new Vector2(0f, -4.15f));
        return SaveLevel(root, "V70StubbornKetchup");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V70B = new Dictionary<string, string[]>
    {
        { "v70_stubborn_ketchup", new[] { "Brand new bottle. Look under the cap.",
            "Open it properly first: cap off, then peel the seal." } },
    };
}
#endif
