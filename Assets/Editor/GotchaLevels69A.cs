#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.69, livello 265 (gruppo A): la barchetta che non deve affondare. Riusa il topo (il capitano), la
/// paperella, il secchio, le bolle di sapone, le gocce, lo spruzzo, il vortice, la stellina, il bagliore e gli aiuti di
/// Halloween/0.62 (HW_Goal, HW_Obj, HW_Box, HW_Area, V62_Wire); i disegni nuovi (prefisso v69_: la vasca, l'acqua,
/// le onde, la barchetta, l'indicatore, il cappello, il dito, il nastro, la toppa, la catenella, il tappo, la schiuma,
/// il getto) sono in DoodleSpritesV69A. Niente Easter egg e niente Toni.
/// L'elenco dei livelli (Define) lo aggiunge il capo progetto in GotchaProjectBuilder.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 265 Don't Let It Sink ----------

    /// <summary>Pelo dell'acqua all'inizio e fondo della vasca (gli stessi numeri di DoodleSpritesV69A e del livello).</summary>
    const float V69A_Sea = 0.3f, V69A_Floor = -4.72f;

    static GameObject BuildV69DontLetItSink()
    {
        var root = new GameObject("Level_V69DontLetItSink");
        V69DontLetItSinkLevel level = root.AddComponent<V69DontLetItSinkLevel>();
        Transform r = root.transform;
        Art(r, "Bath", "v69_bath_bg", -20, false);

        // ---- il fondo: lo scarico col tappo e la catenella (sotto l'acqua; si vede solo l'anello nell'angolo) ----
        Vector2 plugAt = new Vector2(0.9f, V69A_Floor + 0.06f);
        Transform plug = HW_Obj(r, "Plug", plugAt, "v69_bath_plug", -12, 1f);
        Transform swirl = HW_Obj(r, "Swirl", plugAt + new Vector2(0f, 0.15f), "s41b_swirl", -11, 1f);
        swirl.localScale = new Vector3(0.75f, 0.22f, 1f);
        SmokeEmitter drainBubbles = Emitter(r, "DrainBubbles", plugAt + new Vector2(0f, 0.25f), "sub_bubble", 9f, 1.6f, 1.4f, 0.25f, 0.9f, new Color(1f, 1f, 1f, 0.8f), -6);
        drainBubbles.growth = 0.3f;
        drainBubbles.sideDrift = 0.2f;

        // ---- l'acqua (il fondo è il perno: la si stira in verticale) e la superficie ----
        Transform water = Child(r, "Water", new Vector2(0f, V69A_Floor)).transform;
        Art(water, "Art", "v69_bath_water", -10, false);
        water.localScale = new Vector3(1f, V69A_Sea - V69A_Floor, 1f);
        Transform surface = Child(r, "Surface", new Vector2(0f, V69A_Sea)).transform;
        Art(surface, "Art", "v69_bath_surface", -9, false);
        HW_Obj(surface, "Foam_L", new Vector2(-2.35f, 0.01f), "v69_bath_foam", -4, 0.9f);
        HW_Obj(surface, "Foam_R", new Vector2(2.2f, 0.0f), "v69_bath_foam", -4, 1.1f);
        HW_Obj(surface, "Foam_M", new Vector2(-0.95f, 0.0f), "v69_bath_foam", -4, 0.55f);
        float[,] bubbles = { { -2.0f, 0.22f, 0.3f }, { -1.0f, 0.5f, 0.2f }, { 1.55f, 0.32f, 0.26f }, { 2.45f, 0.6f, 0.18f }, { -2.5f, 0.75f, 0.14f } };
        for (int i = 0; i < bubbles.GetLength(0); i++)
        {
            Transform b = HW_Obj(surface, "Bubble" + i, new Vector2(bubbles[i, 0], bubbles[i, 1]), "soap_bubble", -3, bubbles[i, 2]);
            b.GetComponentInChildren<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.85f);
        }

        // ---- la catenella: l'anello nell'angolo in basso a sinistra, le maglie fino al tappo ----
        Vector2 endAt = new Vector2(-1.85f, -3.3f);
        Transform chainEnd = HW_Obj(r, "ChainEnd", endAt, "v69_chain_end", -6, 1.1f);
        CircleCollider2D chainCol = chainEnd.gameObject.AddComponent<CircleCollider2D>();
        chainCol.radius = 0.6f;
        SpriteRenderer glint = Art(chainEnd, "Glint", "sparkle_star", -5, false, new Vector2(0.1f, 0.12f)).GetComponent<SpriteRenderer>();
        glint.transform.localScale = Vector3.one * 0.45f;
        glint.color = new Color(1f, 1f, 1f, 0f);
        const int LinkCount = 18;
        var links = new Transform[LinkCount];
        Vector2 plugTop = plugAt + new Vector2(0f, 0.22f);
        for (int i = 0; i < LinkCount; i++)
        {
            float t = (i + 1f) / (LinkCount + 1f);
            Vector2 p = Vector2.Lerp(plugTop, endAt, t) + Vector2.down * (Mathf.Sin(t * Mathf.PI) * 0.35f);
            links[i] = HW_Obj(r, "Link" + i, p, "v69_chain_link", -7, 1f);
        }

        // ---- la paperella ----
        Transform duck = HW_Obj(r, "Duck", new Vector2(-1.55f, V69A_Sea - 0.02f), "pond_duck", -2, 0.85f);
        duck.localScale = new Vector3(-0.85f, 0.85f, 1f);   // guarda la barchetta
        BoxCollider2D duckCol = HW_Box(duck, new Vector2(1.5f, 1.2f), new Vector2(0f, 0.35f));

        // ---- la barchetta (0,0 = linea di galleggiamento), il capitano, l'indicatore, il buco ----
        Transform boat = Child(r, "Boat", new Vector2(0.3f, V69A_Sea)).transform;
        Transform captain = HW_Obj(boat, "Captain", new Vector2(-0.45f, 0.45f), "mouse", 1, 0.4f);
        SpriteRenderer captainArt = captain.GetComponentInChildren<SpriteRenderer>();
        HW_Obj(captain, "Cap", new Vector2(0.02f, 0.8f), "v69_captain_cap", 2, 1.55f);
        BoxCollider2D captainCol = HW_Area(boat, "CaptainArea", new Vector2(0.8f, 0.9f), new Vector2(-0.45f, 0.55f));
        SpriteRenderer hull = Art(boat, "Hull", "v69_toy_boat", 3, false).GetComponent<SpriteRenderer>();
        TextMesh name = Label(boat, new Vector2(-0.12f, -0.17f), 4, "S.S. BUBBLES", 0.022f);
        name.color = new Color(1f, 0.93f, 0.85f);
        Fit(name, 1.0f, 0.14f);
        Art(boat, "Gauge", "v69_boat_gauge", 6, false, new Vector2(-0.88f, 0.8f));
        Transform gaugeFill = Child(boat, "GaugeFill", new Vector2(-0.88f, 0.38f)).transform;
        SpriteRenderer gaugeBar = Bar(gaugeFill, "Fill", new Vector2(0f, 0.5f), new Vector2(0.13f, 1f), new Color(0.3f, 0.75f, 0.95f), 5);
        gaugeFill.localScale = new Vector3(1f, 0.001f, 1f);
        Transform hole = Child(boat, "Hole", new Vector2(0.58f, -0.08f)).transform;
        BoxCollider2D holeCol = HW_Area(hole, "HoleArea", new Vector2(0.95f, 0.85f), new Vector2(0.1f, 0f));
        Transform spurt = HW_Obj(hole, "Spurt", Vector2.zero, "v69_leak_spurt", 4, 1.2f);
        Transform patch = HW_Obj(hole, "Patch", new Vector2(0.0f, -0.0f), "v69_tape_patch", 7, 1f);
        Transform finger = HW_Obj(hole, "Finger", new Vector2(0.03f, 0.02f), "v69_plug_finger", 9, 1f);
        BoxCollider2D boatCol = HW_Box(boat, new Vector2(2.4f, 2.0f), new Vector2(0.05f, 0.45f));
        SmokeEmitter spit = Emitter(captain, "Spit", new Vector2(-0.35f, 0.25f), "drop", 14f, 0.6f, 1.2f, 0.08f, 0.5f, new Color(0.55f, 0.82f, 1f, 0.9f), 8);
        spit.growth = 0f;
        spit.sideDrift = 0.6f;

        // ---- le onde davanti alla barchetta, lo spruzzo ----
        Transform waves = Child(r, "Waves", new Vector2(0f, V69A_Sea)).transform;
        SpriteRenderer wavesArt = Art(waves, "Art", "v69_bath_waves", 10, false).GetComponent<SpriteRenderer>();
        SmokeEmitter spray = Emitter(r, "Spray", new Vector2(0.3f, V69A_Sea + 0.1f), "s41d_raindrop", 20f, 0.7f, 1.6f, 0.9f, 0.7f, new Color(0.6f, 0.85f, 1f, 0.9f), 11);
        spray.growth = 0f;
        spray.sideDrift = 1.2f;

        // ---- il secchio e il nastro adesivo appoggiati sul bordo della vasca ("all'orizzonte") ----
        Transform bucket = HW_Obj(r, "Bucket", new Vector2(-1.95f, 1.62f), "mixer_bucket", 12, 0.42f);
        BoxCollider2D bucketCol = HW_Box(bucket, new Vector2(2.4f, 2.4f), new Vector2(0f, -0.2f));
        Transform tape = HW_Obj(r, "Tape", new Vector2(1.95f, 1.54f), "v69_duct_tape", 12, 1f);
        BoxCollider2D tapeCol = HW_Box(tape, new Vector2(1.0f, 1.0f), new Vector2(0f, -0.05f));

        // ---- dove si tocca il resto: l'acqua e il "cielo" ----
        BoxCollider2D seaCol = HW_Area(r, "SeaArea", new Vector2(5.6f, 5.4f), new Vector2(0f, -2.3f));
        BoxCollider2D skyCol = HW_Area(r, "SkyArea", new Vector2(5.6f, 2.6f), new Vector2(0f, 2.0f));

        HW_Goal(r, "DON'T LET THE BOAT SINK!");
        SetField(level, "water", water);
        SetField(level, "surface", surface);
        SetField(level, "waves", waves);
        SetField(level, "wavesArt", wavesArt);
        SetField(level, "seaArea", seaCol);
        SetField(level, "skyArea", skyCol);
        SetField(level, "boat", boat);
        SetField(level, "boatArea", boatCol);
        SetField(level, "hull", hull);
        SetField(level, "captain", captain);
        SetField(level, "captainArt", captainArt);
        SetField(level, "captainArea", captainCol);
        SetField(level, "captainBubble", SmallBubble(r, new Vector2(-0.9f, 2.25f)));
        SetField(level, "spit", spit);
        SetField(level, "gaugeFill", gaugeFill);
        SetField(level, "gaugeBar", gaugeBar);
        SetField(level, "hole", hole);
        SetField(level, "holeArea", holeCol);
        SetField(level, "spurt", spurt);
        SetField(level, "patch", patch);
        SetField(level, "finger", finger);
        SetField(level, "bucket", bucket);
        SetField(level, "bucketArea", bucketCol);
        SetField(level, "tape", tape);
        SetField(level, "tapeArea", tapeCol);
        SetField(level, "duck", duck);
        SetField(level, "duckArea", duckCol);
        SetField(level, "chainEnd", chainEnd);
        SetField(level, "chainArea", chainCol);
        SetField(level, "glint", glint);
        SetArray(level, "links", links);
        SetField(level, "plug", plug);
        SetField(level, "swirl", swirl);
        SetField(level, "drainBubbles", drainBubbles);
        SetField(level, "spray", spray);
        V62_Wire(level, r, new Vector2(0f, -1.55f));
        return SaveLevel(root, "V69DontLetItSink");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V69A = new Dictionary<string, string[]>
    {
        { "v69_dont_let_it_sink", new[] { "Why is there a rubber duck at sea?", "Every bathtub has a plug." } },
    };
}
#endif
