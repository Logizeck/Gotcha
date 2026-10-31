#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.74, livello 274 (gruppo B): la cipolla che fa piangere. Una cucina: il frigo alto a sinistra (biglietto
/// sotto la calamita "COLD ONIONS DON'T BITE."), il cuoco dietro al bancone (corpo, testa da tingere di blu, occhi e
/// bocche a parte), gli occhialini da nuoto appesi al gancio a destra, il ventilatore da tavolo, la candela, il tagliere
/// con la grossa cipolla e i suoi fumi, la fetta di pane, il coltello. Riusa gli aiuti di Halloween/0.62 (HW_Goal, HW_Obj,
/// HW_Box, HW_Area, V62_Wire), hw_knife, hw_candle, flame, v67_fan_blades/v67_fan_grill, mouth_o, sweat (le lacrime),
/// wind_lines, glow, sparkle_star e puff; i disegni nuovi (prefisso v74_b_) sono in DoodleSpritesV74B. Niente Easter egg e
/// niente Toni. L'elenco dei livelli (Define) lo aggiunge il capo progetto in GotchaProjectBuilder.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 274 The Crying Onion ----------

    const int V74BTears = 16, V74BFumes = 3, V74BFlakes = 5;

    static GameObject BuildV74CryingOnion()
    {
        var root = new GameObject("Level_V74CryingOnion");
        V74CryingOnionLevel level = root.AddComponent<V74CryingOnionLevel>();
        Transform r = root.transform;
        Art(r, "Kitchen", "v74_b_kitchen_bg", -20, false);
        Art(r, "Counter", "v74_b_counter", 10, false);

        // ---- il frigo (origine al centro dello sportello grande) ----
        Transform fridge = HW_Obj(r, "Fridge", new Vector2(-1.55f, -1.6f), "v74_b_fridge", 11);
        BoxCollider2D fridgeCol = HW_Box(fridge, new Vector2(1.7f, 5.6f), new Vector2(0f, -0.65f));
        BoxCollider2D fridgeDropCol = HW_Area(fridge, "DropArea", new Vector2(2.3f, 6.0f), new Vector2(-0.1f, -0.6f));
        Transform fridgeSpot = Child(fridge, "OnionSpot", new Vector2(-0.05f, -0.17f)).transform;
        Transform door = HW_Obj(fridge, "Door", Vector2.zero, "v74_b_fridge_door", 14);
        Transform doorOpen = HW_Obj(fridge, "DoorOpen", Vector2.zero, "v74_b_fridge_door_open", 14);
        doorOpen.gameObject.SetActive(false);
        SpriteRenderer frost = Art(door, "Frost", "v74_b_fridge_frost", 15, false).GetComponent<SpriteRenderer>();
        frost.color = new Color(1f, 1f, 1f, 0f);
        // il biglietto sotto la calamita (l'indizio): si legge meglio toccandolo
        Transform note = HW_Obj(door, "Note", new Vector2(0.02f, 0.95f), "v74_b_note", 16);
        TextMesh noteA = Label(note, new Vector2(0f, -0.02f), 17, "COLD ONIONS", 0.022f);
        TextMesh noteB = Label(note, new Vector2(0.005f, -0.12f), 17, "DON'T BITE.", 0.022f);
        foreach (TextMesh t in new[] { noteA, noteB })
        {
            t.color = new Color(0.28f, 0.2f, 0.24f);
            t.transform.localRotation = Quaternion.Euler(0f, 0f, -4f);
            Fit(t, 0.5f, 0.1f);
        }
        BoxCollider2D noteCol = HW_Box(note, new Vector2(0.95f, 0.85f), new Vector2(0f, 0.02f));
        var flakes = new Object[V74BFlakes];
        for (int i = 0; i < V74BFlakes; i++)
        {
            Transform f = HW_Obj(fridge, "Flake" + i, new Vector2(-0.5f + 0.25f * i, 2.25f - 0.12f * (i % 2)), "sparkle_star", 18, 0.35f);
            f.gameObject.SetActive(false);
            flakes[i] = f;
        }

        // ---- il cuoco dietro al bancone ----
        Transform cook = Child(r, "Cook", new Vector2(0.35f, -0.6f)).transform;
        Art(cook, "Body", "v74_b_cook_body", 3, false);
        Transform head = Child(cook, "Head", Vector2.zero).transform;
        SpriteRenderer headArt = Art(head, "Art", "v74_b_cook_head", 4, false).GetComponent<SpriteRenderer>();
        Transform eyes = Child(head, "Eyes", new Vector2(0f, 0.7f)).transform;
        eyes.localScale = Vector3.one * 1.1f;
        Transform eyesWatery = Art(eyes, "Watery", "v74_b_eyes_watery", 5, false);
        Transform eyesDry = Art(eyes, "Dry", "v74_b_eyes_dry", 5, false);
        Transform eyesCry = Art(eyes, "Cry", "v74_b_eyes_cry", 5, false);
        Transform eyesHappy = Art(eyes, "Happy", "v74_b_eyes_happy", 5, false);
        Transform mouth = Child(head, "Mouth", new Vector2(0f, 0.28f)).transform;
        Transform mouthWobble = Art(mouth, "Wobble", "v74_b_mouth_wobble", 5, false);
        Transform mouthWail = Art(mouth, "Wail", "v74_b_mouth_wail", 5, false);
        Transform mouthPuff = Art(mouth, "Puff", "v74_b_mouth_puff", 5, false);
        Transform mouthSmile = Art(mouth, "Smile", "v74_b_mouth_smile", 5, false);
        Transform mouthGasp = Child(mouth, "Gasp", new Vector2(0f, -0.04f)).transform;
        mouthGasp.localScale = Vector3.one * 0.42f;
        Art(mouthGasp, "Art", "mouth_o", 5, false);
        foreach (Transform t in new[] { eyesDry, eyesCry, eyesHappy, mouthWail, mouthPuff, mouthSmile, mouthGasp }) t.gameObject.SetActive(false);
        Transform breadBite = HW_Obj(mouth, "Bread", new Vector2(0f, 0.0f), "v74_b_bread", 6, 0.62f);
        breadBite.gameObject.SetActive(false);
        BoxCollider2D cookCol = HW_Area(cook, "CookArea", new Vector2(1.6f, 2.3f), new Vector2(0f, 0.55f));
        BoxCollider2D mouthCol = HW_Area(cook, "MouthArea", new Vector2(0.9f, 0.5f), new Vector2(0f, 0.3f));

        // ---- gli occhialini appesi al gancio (lenti in 1.95, 0.95; la cinghia sale al gancio) ----
        Transform goggles = HW_Obj(r, "Goggles", new Vector2(1.95f, 0.95f), "v74_b_goggles", 7);
        Transform strap = Art(goggles, "Strap", "v74_b_goggles_strap", 6, false);
        SpriteRenderer fog = Art(goggles, "Fog", "v74_b_goggles_fog", 8, false).GetComponent<SpriteRenderer>();
        fog.color = new Color(1f, 1f, 1f, 0f);
        BoxCollider2D gogglesCol = HW_Box(goggles, new Vector2(1.1f, 1.0f), new Vector2(0f, 0.15f));
        Transform faceSpot = Child(eyes, "GogglesSpot", Vector2.zero).transform;

        // ---- il ventilatore da tavolo (testa in 0, 0.85) ----
        Transform fan = HW_Obj(r, "Fan", new Vector2(1.95f, -1.5f), "v74_b_fan_stand", 14);
        Transform fanHead = Child(fan, "Head", new Vector2(0f, 0.85f)).transform;
        Transform blades = Child(fanHead, "Blades", Vector2.zero).transform;
        blades.localScale = Vector3.one * 0.62f;
        Art(blades, "Art", "v67_fan_blades", 15, false);
        Transform grill = Child(fanHead, "Grill", Vector2.zero).transform;
        grill.localScale = Vector3.one * 0.62f;
        Art(grill, "Art", "v67_fan_grill", 16, false);
        BoxCollider2D fanCol = HW_Box(fan, new Vector2(1.0f, 1.45f), new Vector2(0f, 0.65f));
        SpriteRenderer wind = Art(r, "Wind", "wind_lines", 29, false, new Vector2(1.15f, -0.6f)).GetComponent<SpriteRenderer>();
        wind.transform.localScale = new Vector3(-0.55f, 0.55f, 1f);
        wind.color = new Color(0.55f, 0.8f, 1f, 0f);

        // ---- la candela (rimedio della nonna) ----
        Transform candle = HW_Obj(r, "Candle", new Vector2(-0.45f, -1.38f), "hw_candle", 15);
        Transform flame = HW_Obj(candle, "Flame", new Vector2(0f, 0.6f), "flame", 16);
        flame.gameObject.SetActive(false);
        BoxCollider2D candleCol = HW_Box(candle, new Vector2(0.65f, 1.05f), new Vector2(0f, 0.42f));

        // ---- il tagliere, la cipolla, i fumi ----
        HW_Obj(r, "Board", new Vector2(0.15f, -1.8f), "v74_b_board", 16);
        BoxCollider2D boardCol = HW_Area(r, "BoardArea", new Vector2(2.4f, 1.6f), new Vector2(0.15f, -1.6f));
        Transform onionHome = Child(r, "OnionHome", new Vector2(0.1f, -1.45f)).transform;
        Transform onion = Child(r, "Onion", new Vector2(0.1f, -1.45f)).transform;
        Transform whole = Art(onion, "Whole", "v74_b_onion", 18, false);
        SpriteRenderer onionFrost = Art(onion, "Frost", "v74_b_onion_frost", 19, false).GetComponent<SpriteRenderer>();
        onionFrost.color = new Color(1f, 1f, 1f, 0f);
        Transform halfL = HW_Obj(onion, "HalfL", Vector2.zero, "v74_b_onion_half", 18);
        Transform halfR = HW_Obj(onion, "HalfR", Vector2.zero, "v74_b_onion_half", 18);
        halfR.localScale = new Vector3(-1f, 1f, 1f);
        halfL.gameObject.SetActive(false);
        halfR.gameObject.SetActive(false);
        BoxCollider2D onionCol = HW_Box(onion, new Vector2(1.0f, 1.25f), new Vector2(0f, 0.05f));
        Transform diced = HW_Obj(r, "Diced", new Vector2(0.1f, -1.62f), "v74_b_onion_diced", 18);
        diced.gameObject.SetActive(false);
        var fumes = new Object[V74BFumes];
        for (int i = 0; i < V74BFumes; i++)
        {
            SpriteRenderer f = Art(r, "Fume" + i, "v74_b_fume", 20, false).GetComponent<SpriteRenderer>();
            f.transform.localPosition = new Vector3(0.1f, -1.0f, 0f);
            f.transform.localScale = Vector3.one * 1.15f;
            f.color = new Color(1f, 1f, 1f, 0f);
            fumes[i] = f;
        }

        // ---- il pane e il coltello ----
        Transform bread = HW_Obj(r, "Bread", new Vector2(1.15f, -1.5f), "v74_b_bread", 17);
        bread.localRotation = Quaternion.Euler(0f, 0f, -10f);
        BoxCollider2D breadCol = HW_Box(bread, new Vector2(0.7f, 0.7f));
        Transform knife = HW_Obj(r, "Knife", new Vector2(1.6f, -2.0f), "hw_knife", 22, 1.1f);
        knife.localRotation = Quaternion.Euler(0f, 0f, 172f);
        BoxCollider2D knifeCol = HW_Box(knife, new Vector2(1.35f, 0.5f));
        Transform knifeTip = Child(knife, "Tip", new Vector2(0.3f, 0f)).transform;

        // ---- le lacrime (una scorta fissa: niente oggetti creati mentre si gioca) ----
        Transform tearsRoot = Child(r, "Tears", Vector2.zero).transform;
        var tears = new Object[V74BTears];
        for (int i = 0; i < V74BTears; i++)
        {
            SpriteRenderer t = Art(tearsRoot, "Tear" + i, "sweat", 30, false).GetComponent<SpriteRenderer>();
            t.transform.localScale = Vector3.one * 1.4f;
            t.gameObject.SetActive(false);
            tears[i] = t;
        }

        // ---- la festa ----
        SpriteRenderer glow = Art(r, "Glow", "glow", 15, false, new Vector2(0.15f, -1.6f)).GetComponent<SpriteRenderer>();
        glow.transform.localScale = Vector3.one * 1.3f;
        glow.color = new Color(1f, 0.95f, 0.6f, 0f);
        Transform sparkle = HW_Obj(r, "Sparkle", new Vector2(0.2f, -1.0f), "sparkle_star", 30, 0.9f);
        sparkle.gameObject.SetActive(false);
        SmokeEmitter puffs = Emitter(onion, "ColdPuffs", new Vector2(0f, 0.2f), "puff", 2.5f, 0.9f, 0.35f, 0.25f, 0.25f, new Color(0.85f, 0.95f, 1f, 0.7f), 21);

        HW_Goal(r, "CHOP THE ONION WITHOUT CRYING!");
        SetField(level, "fridge", fridge);
        SetField(level, "fridgeArea", fridgeCol);
        SetField(level, "fridgeDropArea", fridgeDropCol);
        SetField(level, "fridgeSpot", fridgeSpot);
        SetField(level, "doorClosed", door);
        SetField(level, "doorOpen", doorOpen);
        SetField(level, "doorFrost", frost);
        SetField(level, "note", note);
        SetField(level, "noteArea", noteCol);
        SetArray(level, "flakes", flakes);
        SetField(level, "cook", cook);
        SetField(level, "head", head);
        SetField(level, "headArt", headArt);
        SetField(level, "eyes", eyes);
        SetField(level, "eyesWatery", eyesWatery);
        SetField(level, "eyesDry", eyesDry);
        SetField(level, "eyesCry", eyesCry);
        SetField(level, "eyesHappy", eyesHappy);
        SetField(level, "mouth", mouth);
        SetField(level, "mouthWobble", mouthWobble);
        SetField(level, "mouthWail", mouthWail);
        SetField(level, "mouthPuff", mouthPuff);
        SetField(level, "mouthSmile", mouthSmile);
        SetField(level, "mouthGasp", mouthGasp);
        SetField(level, "breadBite", breadBite);
        SetField(level, "cookArea", cookCol);
        SetField(level, "mouthArea", mouthCol);
        SetField(level, "goggles", goggles);
        SetField(level, "gogglesStrap", strap);
        SetField(level, "gogglesFog", fog);
        SetField(level, "gogglesArea", gogglesCol);
        SetField(level, "faceSpot", faceSpot);
        SetField(level, "fan", fan);
        SetField(level, "fanArea", fanCol);
        SetField(level, "fanHead", fanHead);
        SetField(level, "fanBlades", blades);
        SetField(level, "wind", wind);
        SetField(level, "candle", candle);
        SetField(level, "candleArea", candleCol);
        SetField(level, "flame", flame);
        SetField(level, "boardArea", boardCol);
        SetField(level, "onion", onion);
        SetField(level, "onionHome", onionHome);
        SetField(level, "onionWhole", whole);
        SetField(level, "onionFrost", onionFrost);
        SetField(level, "halfLeft", halfL);
        SetField(level, "halfRight", halfR);
        SetField(level, "onionArea", onionCol);
        SetField(level, "diced", diced);
        SetArray(level, "fumes", fumes);
        SetField(level, "bread", bread);
        SetField(level, "breadArea", breadCol);
        SetField(level, "knife", knife);
        SetField(level, "knifeArea", knifeCol);
        SetField(level, "knifeTip", knifeTip);
        SetArray(level, "tears", tears);
        SetField(level, "glow", glow);
        SetField(level, "sparkle", sparkle);
        SetField(level, "coldPuffs", puffs);
        V62_Wire(level, r, new Vector2(0f, -4.15f));
        return SaveLevel(root, "V74CryingOnion");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V74B = new Dictionary<string, string[]>
    {
        { "v74_crying_onion", new[] { "Read the note on the fridge.",
            "Chill the onion first, then chop." } },
    };
}
#endif
