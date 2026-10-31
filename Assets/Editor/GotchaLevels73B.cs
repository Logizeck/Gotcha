#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.73, livello 272 (gruppo B): il calzino perso. Una lavanderia: la lavatrice a oblò a sinistra (sportello
/// chiuso, guarnizione di gomma, il pezzetto di calzino a stelline che spunta in basso a destra, il detersivo sopra),
/// l'asciugatrice a destra (il coniglietto di pelucchi dentro), lo stendino con il calzino solitario e la molletta vuota
/// accanto, la cesta con sette calzini spaiati, il gatto che dorme ("cat", occhi v72_b_cat_eyes) sopra un calzino a pois.
/// Riusa gli aiuti di Halloween/0.62 (HW_Goal, HW_Obj, HW_Box, HW_Area, V62_Wire), glow, sparkle_star e zzz; i disegni
/// nuovi (prefisso v73_b_) sono in DoodleSpritesV73B. Niente Easter egg e niente Toni. L'elenco dei livelli (Define)
/// lo aggiunge il capo progetto in GotchaProjectBuilder.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 272 The Missing Sock ----------

    static GameObject BuildV73MissingSock()
    {
        var root = new GameObject("Level_V73MissingSock");
        V73MissingSockLevel level = root.AddComponent<V73MissingSockLevel>();
        Transform r = root.transform;
        Art(r, "Laundry", "v73_b_laundry_bg", -20, false);

        // ---- la lavatrice (coordinate locali del disegno: oblò in 0, -0.12; guarnizione r 0.56..0.74) ----
        Transform washer = HW_Obj(r, "Washer", new Vector2(-1.1f, 0.55f), "v73_b_washer", 3);
        BoxCollider2D washerCol = HW_Box(washer, new Vector2(1.9f, 2.16f));
        Vector2 hole = new Vector2(0f, -0.12f);
        Transform opening = Child(washer, "Opening", hole).transform;
        CircleCollider2D openingCol = opening.gameObject.AddComponent<CircleCollider2D>();
        openingCol.radius = 0.8f;
        Transform sealPeel = HW_Obj(washer, "SealPeel", hole, "v73_b_seal_peel", 5);
        sealPeel.gameObject.SetActive(false);
        Transform door = HW_Obj(washer, "Door", hole, "v73_b_washer_door", 6);
        Transform doorOpen = HW_Obj(washer, "DoorOpen", hole, "v73_b_door_open", 6);
        doorOpen.gameObject.SetActive(false);
        // il pezzetto di calzino che spunta dalla guarnizione (in basso a destra, a -55 gradi, raggio 0.69)
        Transform peek = HW_Obj(washer, "Peek", hole + new Vector2(0.396f, -0.565f), "v73_b_sock_peek", 7);
        peek.localRotation = Quaternion.Euler(0f, 0f, 35f);
        BoxCollider2D peekCol = HW_Box(peek, new Vector2(0.6f, 0.5f));
        // il detersivo sopra la lavatrice
        Transform detergent = HW_Obj(washer, "Detergent", new Vector2(-0.42f, 1.08f), "v73_b_detergent", 4);
        TextMesh suds = Label(detergent, new Vector2(0f, 0.5f), 5, "SUDS", 0.03f);
        suds.color = new Color(0.24f, 0.44f, 0.88f);
        Fit(suds, 0.34f, 0.13f);
        BoxCollider2D detergentCol = HW_Box(detergent, new Vector2(0.7f, 0.8f), new Vector2(0f, 0.34f));

        // ---- l'asciugatrice e il coniglietto di pelucchi ----
        Transform dryer = HW_Obj(r, "Dryer", new Vector2(1.1f, 0.55f), "v73_b_dryer", 3);
        BoxCollider2D dryerCol = HW_Box(dryer, new Vector2(1.9f, 2.16f));
        Transform dryerDoor = HW_Obj(dryer, "Door", hole, "v73_b_dryer_door", 6);
        Transform dryerOpen = HW_Obj(dryer, "DoorOpen", hole, "v73_b_door_open", 6);
        dryerOpen.gameObject.SetActive(false);
        Transform lintRest = Child(r, "LintRest", new Vector2(1.12f, 0.28f)).transform;
        Transform lint = HW_Obj(r, "Lint", new Vector2(1.12f, 0.28f), "v73_b_lint_bunny", 7);
        BoxCollider2D lintCol = HW_Box(lint, new Vector2(0.7f, 0.65f), new Vector2(0f, 0.04f));
        lint.gameObject.SetActive(false);

        // ---- lo stendino: il calzino solitario, la molletta vuota accanto (lì va il gemello) ----
        HW_Obj(r, "Rack", new Vector2(0.45f, -1.25f), "v73_b_rack", 8);
        SpriteRenderer glow = Art(r, "PairGlow", "glow", 7, false, new Vector2(0.45f, -1.65f)).GetComponent<SpriteRenderer>();
        glow.transform.localScale = Vector3.one * 1.4f;
        glow.color = new Color(1f, 0.95f, 0.6f, 0f);
        Transform rackSock = HW_Obj(r, "RackSock", new Vector2(0.05f, -1.22f), "v73_b_sock_stars", 10, 0.85f);
        HW_Obj(r, "PegA", new Vector2(0.05f, -1.2f), "v73_b_peg", 12);
        Transform slot = Child(r, "PairSlot", new Vector2(0.8f, -1.22f)).transform;
        HW_Obj(r, "PegB", new Vector2(0.8f, -1.2f), "v73_b_peg", 12);
        BoxCollider2D rackSockCol = HW_Area(r, "RackSockArea", new Vector2(0.75f, 1.0f), new Vector2(0.14f, -1.62f));
        BoxCollider2D rackDropCol = HW_Area(r, "RackDropArea", new Vector2(3.0f, 1.3f), new Vector2(0.45f, -1.45f));
        Transform sparkle = HW_Obj(r, "Sparkle", new Vector2(0.45f, -1.85f), "sparkle_star", 30, 0.8f);
        sparkle.gameObject.SetActive(false);

        // ---- la cesta e i sette calzini spaiati (origine di ogni calzino = il bordo in alto) ----
        Transform basket = Child(r, "Basket", new Vector2(-1.3f, -2.75f)).transform;
        Art(basket, "Back", "v73_b_basket_back", 12, false);
        Art(basket, "Front", "v73_b_basket_front", 21, false);
        string[] patterns = { "stripes", "argyle", "hearts", "zigzag", "rainbow", "checks", "hole" };
        Vector3[] pile =
        {
            new Vector3(-2.05f, -2.5f, 95f), new Vector3(-0.55f, -2.42f, -95f), new Vector3(-1.5f, -2.15f, 160f),
            new Vector3(-1.05f, -2.2f, -150f), new Vector3(-1.75f, -2.5f, 70f), new Vector3(-0.8f, -2.52f, -65f),
            new Vector3(-1.3f, -2.45f, 15f),
        };
        var socks = new Object[patterns.Length];
        var sockAreas = new Object[patterns.Length];
        for (int i = 0; i < patterns.Length; i++)
        {
            Transform s = HW_Obj(r, "Sock_" + patterns[i], new Vector2(pile[i].x, pile[i].y), "v73_b_sock_" + patterns[i], 13 + i, 0.8f);
            s.localRotation = Quaternion.Euler(0f, 0f, pile[i].z);
            socks[i] = s;
            sockAreas[i] = HW_Box(s, new Vector2(0.62f, 0.95f), new Vector2(0.1f, -0.38f));
        }

        // ---- il gatto che dorme sul calzino a pois (ne spunta la punta a sinistra del cuscino) ----
        Transform cat = HW_Obj(r, "Cat", new Vector2(1.35f, -3.3f), "cat", 22, 0.55f);
        BoxCollider2D catCol = HW_Box(cat, new Vector2(3.0f, 1.9f), new Vector2(0f, 0.05f));
        Transform eyes = Art(cat, "Eyes", "v72_b_cat_eyes", 23, false);
        eyes.gameObject.SetActive(false);
        Transform zzz = HW_Obj(r, "Zzz", new Vector2(0.85f, -2.7f), "zzz", 24, 0.5f);
        Transform petSock = HW_Obj(r, "PetSock", new Vector2(0.85f, -3.45f), "v73_b_sock_dots", 21, 0.8f);
        petSock.localRotation = Quaternion.Euler(0f, 0f, -90f);
        BoxCollider2D petSockCol = HW_Box(petSock, new Vector2(0.62f, 0.95f), new Vector2(0.1f, -0.38f));
        Transform petSockRest = Child(r, "PetSockRest", new Vector2(0.72f, -3.62f)).transform;
        petSockRest.localRotation = Quaternion.Euler(0f, 0f, -95f);

        // ---- il gemello (nascosto nella guarnizione; una volta fuori resta per terra davanti alla lavatrice) ----
        Transform matchRest = Child(r, "MatchSockRest", new Vector2(-1.25f, -0.8f)).transform;
        matchRest.localRotation = Quaternion.Euler(0f, 0f, -80f);
        Transform match = HW_Obj(r, "MatchSock", new Vector2(-1.25f, -0.8f), "v73_b_sock_stars", 24, 0.85f);
        match.localRotation = Quaternion.Euler(0f, 0f, -80f);
        BoxCollider2D matchCol = HW_Box(match, new Vector2(0.7f, 1.0f), new Vector2(0.1f, -0.38f));
        match.gameObject.SetActive(false);

        HW_Goal(r, "FIND THE MATCHING SOCK!");
        SetField(level, "washer", washer);
        SetField(level, "washerArea", washerCol);
        SetField(level, "washerOpening", opening);
        SetField(level, "openingArea", openingCol);
        SetField(level, "doorClosed", door);
        SetField(level, "doorOpen", doorOpen);
        SetField(level, "sealPeel", sealPeel);
        SetField(level, "peek", peek);
        SetField(level, "peekArea", peekCol);
        SetField(level, "detergent", detergent);
        SetField(level, "detergentArea", detergentCol);
        SetField(level, "dryer", dryer);
        SetField(level, "dryerArea", dryerCol);
        SetField(level, "dryerDoorClosed", dryerDoor);
        SetField(level, "dryerDoorOpen", dryerOpen);
        SetField(level, "lint", lint);
        SetField(level, "lintArea", lintCol);
        SetField(level, "lintRest", lintRest);
        SetField(level, "rackSock", rackSock);
        SetField(level, "rackSockArea", rackSockCol);
        SetField(level, "rackDropArea", rackDropCol);
        SetField(level, "pairSlot", slot);
        SetField(level, "sparkle", sparkle);
        SetField(level, "pairGlow", glow);
        SetArray(level, "basketSocks", socks);
        SetArray(level, "basketSockAreas", sockAreas);
        SetField(level, "basket", basket);
        SetField(level, "petSock", petSock);
        SetField(level, "petSockArea", petSockCol);
        SetField(level, "petSockRest", petSockRest);
        SetField(level, "matchSock", match);
        SetField(level, "matchSockArea", matchCol);
        SetField(level, "matchSockRest", matchRest);
        SetField(level, "cat", cat);
        SetField(level, "catArea", catCol);
        SetField(level, "catEyes", eyes);
        SetField(level, "zzz", zzz);
        V62_Wire(level, r, new Vector2(0f, -4.15f));
        return SaveLevel(root, "V73MissingSock");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V73B = new Dictionary<string, string[]>
    {
        { "v73_missing_sock", new[] { "Washing machines have hiding places.",
            "Look inside the rubber seal of the door." } },
    };
}
#endif
