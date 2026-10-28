#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.67, livello 262 (gruppo B): la candela immortale. Riusa lo sfondo della festa, la torta, la candelina, la
/// fiammella, gli sbuffi, le gocce, le linee del vento, l'alone, il pulsante del microfono della 0.64 e gli aiuti di
/// Halloween/0.62 (HW_Goal, HW_Obj, HW_Box, V62_Wire); i pochi disegni nuovi (prefisso v67_: ventilatore, bicchiere,
/// facce della fiammella) sono in DoodleSpritesV67B. Niente Easter egg e niente Toni.
/// L'elenco dei livelli (Define) lo aggiunge il capo progetto in GotchaProjectBuilder.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 262 The Immortal Candle ----------

    static GameObject BuildV67ImmortalCandle()
    {
        var root = new GameObject("Level_V67ImmortalCandle");
        V67ImmortalCandleLevel level = root.AddComponent<V67ImmortalCandleLevel>();
        Transform r = root.transform;
        Art(r, "Party", "party_bg", -20, false);   // il bordo del tavolo è a y = -2.2

        // la torta sul tavolo con una sola candelina al centro; lo stoppino finisce a y ≈ 0
        Transform cake = Child(r, "Cake", new Vector2(-0.3f, -1.95f)).transform;
        Art(cake, "Art", "cake", 3, false);
        Transform candle = Child(cake, "Candle", new Vector2(0f, 0.58f)).transform;
        candle.localScale = Vector3.one * 2.4f;
        Art(candle, "Art", "candle", 4, false);
        Transform wick = Child(candle, "Wick", new Vector2(0f, 0.57f)).transform;

        // la fiamma: oggetto separato (centro della fiamma), con l'area di presa generosa; il corpo si allunga dalla base
        Transform flame = Child(r, "Flame", new Vector2(-0.3f, 0.5f)).transform;
        SpriteRenderer glow = Art(flame, "Glow", "glow", 2, false).GetComponent<SpriteRenderer>();
        glow.transform.localScale = Vector3.one * 1.9f;
        glow.color = new Color(1f, 0.85f, 0.4f, 0.5f);
        Transform body = Child(flame, "Body", new Vector2(0f, -0.52f)).transform;
        body.localScale = Vector3.one * 4f;
        Art(body, "Art", "flame", 6, false);
        SpriteRenderer giggle = Art(body, "Giggle", "v67_flame_giggle", 7, false).GetComponent<SpriteRenderer>();
        CircleCollider2D flameCol = flame.gameObject.AddComponent<CircleCollider2D>();
        flameCol.radius = 1.1f;

        SmokeEmitter wickSmoke = Emitter(r, "WickSmoke", new Vector2(-0.3f, 0.05f), "puff", 5f, 1.6f, 0.55f, 0.03f, 0.18f, new Color(0.6f, 0.6f, 0.66f, 0.7f), 8);
        wickSmoke.growth = 1.2f;
        wickSmoke.sideDrift = 0.12f;
        wickSmoke.spinRange = 20f;
        SmokeEmitter steam = Emitter(r, "Steam", new Vector2(-0.3f, 0.55f), "puff", 14f, 1.0f, 1.1f, 0.35f, 0.4f, new Color(1f, 1f, 1f, 0.85f), 20);
        steam.growth = 1.6f;

        // il ventilatore a colonna sul tavolo, a destra: la testa è all'altezza della fiamma
        Transform fan = Child(r, "Fan", new Vector2(1.72f, -2.45f)).transform;
        fan.localScale = Vector3.one * 0.9f;
        Art(fan, "Body", "v67_fan_body", 3, false);
        Transform head = Child(fan, "Head", new Vector2(0f, Gotcha.EditorArt.DoodleSprites.V67FanHeadY)).transform;
        Transform blades = Child(head, "Blades", Vector2.zero).transform;
        Art(blades, "Art", "v67_fan_blades", 4, false);
        Art(head, "Grill", "v67_fan_grill", 5, false);
        BoxCollider2D fanCol = HW_Box(fan, new Vector2(1.5f, 3.5f), new Vector2(0f, 1.65f));
        Transform wind = Child(r, "Wind", new Vector2(0.75f, 0.12f)).transform;
        wind.localScale = new Vector3(-0.55f, 0.5f, 1f);   // soffia verso sinistra, verso la fiamma
        SpriteRenderer windArt = Art(wind, "Art", "wind_lines", 9, false).GetComponent<SpriteRenderer>();
        windArt.color = new Color(0.75f, 0.9f, 1f, 0f);

        // il bicchiere d'acqua, davanti a sinistra (si trascina sopra la candela per versare)
        Transform glass = Child(r, "Glass", new Vector2(-1.75f, -3.7f)).transform;
        glass.localScale = Vector3.one * 1.15f;
        Art(glass, "Art", "v67_glass", 12, false);
        BoxCollider2D glassCol = HW_Box(glass, new Vector2(1.3f, 1.6f), new Vector2(0f, 0.5f));
        SmokeEmitter drops = Emitter(glass, "PourDrops", new Vector2(0.36f, 0.95f), "drop", 30f, 0.6f, -2.6f, 0.06f, 0.7f, Color.white, 13);
        drops.growth = 0f;
        drops.spinRange = 0f;
        drops.sideDrift = 0.15f;

        // la fiammella che torna a sbirciare dal bordo in alto con la linguaccia (nascosta, dietro la fascia dell'obiettivo)
        Transform peek = Child(r, "Peek", new Vector2(0f, 3.75f)).transform;
        Transform peekBody = Child(peek, "Body", Vector2.zero).transform;
        peekBody.localScale = Vector3.one * 4.5f;
        Art(peekBody, "Art", "flame", 65, false);
        Art(peekBody, "Face", "v67_flame_cheeky", 66, false);

        // l'alternativa al microfono (compare solo se il microfono non c'è: il soffio non serve per vincere)
        Transform micBtn = HW_Obj(r, "MicButton", new Vector2(-0.6f, -4.25f), "v64_mic_btn", 30, 1f);
        TextMesh micLabel = Fit(Label(micBtn, new Vector2(1.0f, 0f), 31, "HOLD TO BLOW", 0.03f), 1.3f, 0.22f);
        micLabel.color = new Color(0.25f, 0.13f, 0.3f);
        CircleCollider2D micCol = micBtn.gameObject.AddComponent<CircleCollider2D>();
        micCol.radius = 0.6f;

        HW_Goal(r, "BLOW OUT THE CANDLE.");
        SetField(level, "wick", wick);
        SetField(level, "flame", flame);
        SetField(level, "flameBody", body);
        SetField(level, "flameArea", flameCol);
        SetField(level, "flameGlow", glow);
        SetField(level, "giggleFace", giggle);
        SetField(level, "wickSmoke", wickSmoke);
        SetField(level, "steam", steam);
        SetField(level, "fan", fan);
        SetField(level, "fanArea", fanCol);
        SetField(level, "fanBlades", blades);
        SetField(level, "fanWind", windArt);
        SetField(level, "glass", glass);
        SetField(level, "glassArea", glassCol);
        SetField(level, "pourDrops", drops);
        SetField(level, "peek", peek);
        SetField(level, "flameBubble", SmallBubble(r, new Vector2(0.95f, 1.5f)));
        SetField(level, "micButton", micBtn);
        SetField(level, "micButtonArea", micCol);
        V62_Wire(level, r, new Vector2(0f, 2.45f));
        return SaveLevel(root, "V67ImmortalCandle");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V67B = new Dictionary<string, string[]>
    {
        { "v67_immortal_candle", new[] { "That flame looks strangely solid.",
            "Maybe you should take the fire away instead of putting it out." } },
    };
}
#endif
