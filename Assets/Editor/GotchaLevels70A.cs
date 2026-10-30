#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.70, livello 267 (gruppo A): la mosca che non si lascia schiacciare. Riusa la cucina del mattino, la mosca,
/// la paletta, lo spray, la torta, la nuvoletta dei pensieri con l'albero, la nebbia dello specchio, la stellina, lo sbuffo
/// e gli aiuti di Halloween/0.62 (HW_Goal, HW_Obj, HW_Box, HW_Area, V62_Wire); i disegni nuovi (prefisso v70_a_: la
/// finestra a ghigliottina col paesaggio, la cornice, i due vetri, il gancio, la mascherina antigas, la torta
/// spiaccicata) sono in DoodleSpritesV70A. Niente Easter egg e niente Toni.
/// L'elenco dei livelli (Define) lo aggiunge il capo progetto in GotchaProjectBuilder.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 267 The Unswattable Fly ----------

    static GameObject BuildV70UnswattableFly()
    {
        var root = new GameObject("Level_V70UnswattableFly");
        V70UnswattableFlyLevel level = root.AddComponent<V70UnswattableFlyLevel>();
        Transform r = root.transform;
        // (la finestrella dello sfondo resta coperta dalla finestra grande)
        Art(r, "Kitchen", "morning_kitchen_bg", -20, false);

        // ---- la finestra a ghigliottina (origine al centro dell'apertura, come in DoodleSpritesV70A) ----
        Transform window = Child(r, "Window", new Vector2(-1.2f, 1.5f)).transform;
        Art(window, "View", "v70_a_window_view", -15, false);
        Art(window, "PaneTop", "v70_a_pane_top", -13, false);
        Transform sash = Child(window, "Sash", Vector2.zero).transform;
        Art(sash, "Art", "v70_a_sash", -12, false);
        BoxCollider2D sashCol = HW_Area(sash, "SashArea", new Vector2(2.5f, 1.55f), new Vector2(0f, -0.58f));
        Transform latch = Child(sash, "Latch", new Vector2(-0.06f, 0.03f)).transform;
        Art(latch, "Art", "v70_a_latch", -11, false);
        SpriteRenderer latchGlint = Art(latch, "Glint", "sparkle_star", -9, false, new Vector2(0.06f, 0.08f)).GetComponent<SpriteRenderer>();
        latchGlint.transform.localScale = Vector3.one * 0.6f;
        latchGlint.color = new Color(1f, 1f, 1f, 0f);
        BoxCollider2D latchCol = HW_Area(sash, "LatchArea", new Vector2(0.9f, 0.62f), new Vector2(0.05f, 0.04f));
        Art(window, "Frame", "v70_a_window_frame", -10, false);
        BoxCollider2D paneCol = HW_Area(window, "PaneArea", new Vector2(2.3f, 1.15f), new Vector2(0f, 0.62f));
        SpriteRenderer bonkStar = Art(r, "BonkStar", "sparkle_star", -9, false).GetComponent<SpriteRenderer>();
        bonkStar.transform.localScale = Vector3.one * 0.6f;
        bonkStar.color = new Color(1f, 1f, 1f, 0f);
        TextMesh bonkText = Label(r, new Vector2(-1.2f, 1.9f), -8, "", 0.035f);
        bonkText.color = new Color(0.17f, 0.13f, 0.2f);

        // ---- sul tavolo: la torta (perno sul fondo del piatto), la paletta sdraiata, lo spray ----
        Transform cake = Child(r, "Cake", new Vector2(0.95f, -1.4f)).transform;
        cake.localScale = Vector3.one * 0.55f;
        Art(cake, "Art", "cake", 2, false, new Vector2(0f, 0.95f));
        Transform cakeTop = Child(cake, "Top", new Vector2(0f, 1.78f)).transform;
        BoxCollider2D cakeCol = HW_Area(r, "CakeArea", new Vector2(1.9f, 1.35f), new Vector2(0.95f, -0.8f));
        Transform splat = HW_Obj(r, "Splat", new Vector2(0.95f, -1.28f), "v70_a_cake_splat", 3, 0.9f);

        Transform swatter = Child(r, "Swatter", new Vector2(-1.75f, -1.2f)).transform;
        swatter.localRotation = Quaternion.Euler(0f, 0f, 84f);
        swatter.localScale = Vector3.one * 0.7f;
        Art(swatter, "Art", "swatter", 6, false);
        Transform swatterHead = Child(swatter, "Head", Vector2.zero).transform;
        BoxCollider2D swatterCol = HW_Area(swatter, "Area", new Vector2(1.5f, 2.9f), new Vector2(0f, -0.75f));

        Transform can = Child(r, "SprayCan", new Vector2(2.0f, -0.55f)).transform;
        can.localScale = Vector3.one * 0.62f;
        Art(can, "Art", "spray_can", 6, false);
        BoxCollider2D canCol = HW_Box(can, new Vector2(1.6f, 2.2f), new Vector2(0f, -0.62f));
        SmokeEmitter mist = Emitter(can, "Mist", new Vector2(0f, 0.15f), "puff", 50f, 0.55f, 2.4f, 0.22f, 0.32f, new Color(0.78f, 1f, 0.72f, 0.75f), 35);
        mist.growth = 2.2f;

        // ---- la mosca (con la mascherina nascosta), il suo pensiero, il ronzio ----
        Transform fly = Child(r, "Fly", new Vector2(0.4f, 0.9f)).transform;
        fly.localScale = Vector3.one * 0.68f;
        SpriteRenderer flyArt = Art(fly, "Art", "house_fly", 20, false).GetComponent<SpriteRenderer>();
        Transform mask = HW_Obj(fly, "GasMask", Vector2.zero, "v70_a_gas_mask", 21, 1f);
        Transform thought = Child(r, "Thought", new Vector2(1.0f, 1.5f)).transform;
        thought.localScale = Vector3.one * 0.38f;
        Art(thought, "Cloud", "dream_cloud", 22, false);
        Transform tree = Child(thought, "Tree", new Vector2(0.05f, 0.17f)).transform;
        tree.localScale = Vector3.one * 0.22f;
        Art(tree, "Art", "apple_tree", 23, false);
        AudioSource buzz = Child(r, "Buzz", Vector2.zero).AddComponent<AudioSource>();
        buzz.playOnAwake = false;
        Transform screenFly = HW_Obj(r, "ScreenFly", Vector2.zero, "house_fly", 60, 1f);
        SpriteRenderer screenFlyArt = screenFly.GetComponentInChildren<SpriteRenderer>();

        // ---- la nebbia dello spray (davanti a tutto, trasparente all'inizio) ----
        SpriteRenderer fog = Art(r, "Fog", "mirror_fog", 40, false, new Vector2(0f, 0.2f)).GetComponent<SpriteRenderer>();
        fog.transform.localScale = new Vector3(3.2f, 4.2f, 1f);
        fog.color = new Color(1f, 1f, 1f, 0f);

        HW_Goal(r, "GET RID OF THE FLY!");
        SetField(level, "fly", fly);
        SetField(level, "flyArt", flyArt);
        SetField(level, "gasMask", mask);
        SetField(level, "thought", thought);
        SetField(level, "buzz", buzz);
        SetField(level, "flyBubble", SmallBubble(r, new Vector2(0.6f, 2.3f)));
        SetField(level, "screenFly", screenFly);
        SetField(level, "screenFlyArt", screenFlyArt);
        SetField(level, "sash", sash);
        SetField(level, "sashArea", sashCol);
        SetField(level, "latch", latch);
        SetField(level, "latchArea", latchCol);
        SetField(level, "latchGlint", latchGlint);
        SetField(level, "paneArea", paneCol);
        SetField(level, "windowCenter", window);
        SetField(level, "bonkStar", bonkStar);
        SetField(level, "bonkText", bonkText);
        SetField(level, "cake", cake);
        SetField(level, "cakeArea", cakeCol);
        SetField(level, "cakeTop", cakeTop);
        SetField(level, "splat", splat);
        SetField(level, "swatter", swatter);
        SetField(level, "swatterArea", swatterCol);
        SetField(level, "swatterHead", swatterHead);
        SetField(level, "can", can);
        SetField(level, "canArea", canCol);
        SetField(level, "mist", mist);
        SetField(level, "fog", fog);
        V62_Wire(level, r, new Vector2(0f, -2.75f));
        return SaveLevel(root, "V70UnswattableFly");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V70A = new Dictionary<string, string[]>
    {
        { "v70_unswattable_fly", new[] { "Listen to where it keeps bumping.", "Maybe it wants to leave as much as you want it gone." } },
    };
}
#endif
