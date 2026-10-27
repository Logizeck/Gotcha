#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.62: cinque livelli "vecchi oggetti, azioni nuove" (251-255, subito dopo lo speciale Halloween).
/// Riusano gli sprite esistenti (canna da pesca, calamita, gatto, topo, palloncino, candela, specchio, ombrello...)
/// in modi inattesi; pochi disegni nuovi (prefisso v62_/v63_) in DoodleSpritesV62/V63. Aiuti comuni: quelli di
/// Halloween (HW_Goal, HW_Obj, HW_Box, HW_Toast, HW_Confetti).
/// 0.63: niente Easter egg in questi livelli e niente Toni (compare solo nei livelli multipli di 25): il 252 è
/// diventato "Wake up, Kitty!" e dal 254 è sparito Toni che dormiva sul tappeto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels62() => new List<LevelDefinition>
    {
        Define("V62WakeKitty", "v62_wake_kitty", "WAKE UP, KITTY!", "Noise and light won't do it. Something tastier might... if it gets there.",
            BuildV62WakeKitty()),
    };

    // ---------- Aiuti comuni ----------

    /// <summary>Campi comuni dei livelli 0.62/0.63: coriandoli e scritta di commento.</summary>
    static void V62_Wire(Object level, Transform root, Vector2 toastAt)
    {
        SetArray(level, "confetti", HW_Confetti(root));
        TextMesh info = HW_Toast(root, toastAt, out Transform infoHolder);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
    }

    // ---------- 252 Wake up, Kitty! ----------

    static GameObject BuildV62WakeKitty()
    {
        var root = new GameObject("Level_V62WakeKitty");
        V62WakeKittyLevel level = root.AddComponent<V62WakeKittyLevel>();
        Art(root.transform, "Night", "night_bg", -20, false);

        // il gatto che dorme sul suo cuscino, sul tappeto (la testa è a sinistra)
        Transform cat = Child(root.transform, "Cat", new Vector2(0.35f, -3.35f)).transform;
        cat.localScale = Vector3.one * 0.85f;
        Art(cat, "Cushion", "v63_cat_cushion", 2, false);
        Transform body = Child(cat, "Body", Vector2.zero).transform;
        SpriteRenderer catArt = Art(body, "Art", "cat", 3, false).GetComponent<SpriteRenderer>();
        Transform tailCover = Art(cat, "TailCover", "v63_cat_tail", 4, false);
        Transform nose = Child(cat, "Nose", new Vector2(-0.72f, 0.12f)).transform;
        BoxCollider2D catCol = HW_Box(cat, new Vector2(3.0f, 1.9f), new Vector2(0f, 0.1f));
        Transform awake = HW_Obj(root.transform, "AwakeCat", new Vector2(0.35f, -3.48f), "roof_cat", 5, 0.9f);
        SmokeEmitter zzz = Emitter(root.transform, "Zzz", new Vector2(-0.1f, -2.75f), "zzz", 1.2f, 2.2f, 0.45f, 0.1f, 0.6f, Color.white, 30);
        zzz.spinRange = 12f;
        zzz.growth = 0.9f;
        zzz.sideDrift = 0.15f;
        SmokeEmitter hearts = Emitter(root.transform, "Hearts", new Vector2(0.4f, -2.2f), "heart", 4f, 1.3f, 0.8f, 0.4f, 0.8f, Color.white, 35);

        // la tana del topo in fondo al muro (il topo si vede solo quando esce)
        Transform hole = HW_Obj(root.transform, "MouseHole", new Vector2(1.75f, -1.75f), "mouse_hole", 2, 0.8f);
        BoxCollider2D holeCol = HW_Box(hole, new Vector2(1.3f, 1.3f));
        Transform mouse = HW_Obj(root.transform, "Mouse", new Vector2(1.75f, -1.8f), "mouse", 11, 0.4f);

        // il pesce sulla mensola, il formaggio sul comodino, la piuma per terra
        Art(root.transform, "Shelf", "wall_shelf", 1, false, new Vector2(1.35f, 0.35f));
        Transform fish = HW_Obj(root.transform, "Fish", new Vector2(1.35f, 0.7f), "fish", 10, 1.2f);
        BoxCollider2D fishCol = HW_Box(fish, new Vector2(0.95f, 0.6f));
        HW_Obj(root.transform, "Nightstand", new Vector2(-1.6f, -0.88f), "hw_nightstand", 1);
        Transform cheese = HW_Obj(root.transform, "Cheese", new Vector2(-1.6f, -0.78f), "cheese_wedge", 8, 0.85f);
        BoxCollider2D cheeseCol = HW_Box(cheese, new Vector2(1.0f, 0.75f), new Vector2(0f, 0.2f));
        Transform feather = HW_Obj(root.transform, "Feather", new Vector2(-2.05f, -2.75f), "feather", 9, 0.7f);
        feather.localRotation = Quaternion.Euler(0f, 0f, 10f);
        BoxCollider2D featherCol = HW_Box(feather, new Vector2(1.9f, 1.0f), new Vector2(0.55f, 0.2f));

        // la lampada appesa, la sveglia per terra
        Transform lamp = HW_Obj(root.transform, "Lamp", new Vector2(0.15f, 1.55f), "lamp", 2, 0.75f);
        BoxCollider2D lampCol = HW_Box(lamp, new Vector2(1.5f, 1.3f), new Vector2(0f, 0.3f));
        SpriteRenderer lampGlow = Art(root.transform, "LampGlow", "glow", 3, false, new Vector2(0.15f, 1.5f)).GetComponent<SpriteRenderer>();
        lampGlow.transform.localScale = Vector3.one * 2.2f;
        lampGlow.color = new Color(1f, 0.92f, 0.55f, 0f);
        SpriteRenderer lampLight = Art(root.transform, "LampLight", "glow", 1, false, new Vector2(0.15f, -0.5f)).GetComponent<SpriteRenderer>();
        lampLight.transform.localScale = new Vector3(9f, 9f, 1f);
        lampLight.color = new Color(1f, 0.92f, 0.55f, 0f);
        Transform alarm = HW_Obj(root.transform, "Alarm", new Vector2(-1.85f, -4.4f), "alarm_clock", 7, 0.42f);
        BoxCollider2D alarmCol = HW_Box(alarm, new Vector2(2.4f, 2.1f), new Vector2(0f, 0.8f));
        SmokeEmitter waves = Emitter(root.transform, "AlarmWaves", new Vector2(-1.85f, -3.8f), "ring_waves", 6f, 0.5f, 0.3f, 0.05f, 0.35f, Color.white, 20);
        waves.growth = 0.8f;
        waves.spinRange = 0f;

        HW_Goal(root.transform, "WAKE THE CAT UP FOR DINNER!");
        SetField(level, "cat", cat);
        SetField(level, "catBody", body);
        SetField(level, "catArt", catArt);
        SetField(level, "catSleep", sprites["cat"]);
        SetField(level, "catArea", catCol);
        SetField(level, "nose", nose);
        SetField(level, "tailCover", tailCover);
        SetField(level, "awakeCat", awake);
        SetField(level, "zzz", zzz);
        SetField(level, "hearts", hearts);
        SetField(level, "mouse", mouse);
        SetField(level, "mouseHole", hole);
        SetField(level, "holeArea", holeCol);
        SetField(level, "fish", fish);
        SetField(level, "fishArea", fishCol);
        SetField(level, "cheese", cheese);
        SetField(level, "cheeseArea", cheeseCol);
        SetField(level, "feather", feather);
        SetField(level, "featherArea", featherCol);
        SetField(level, "alarm", alarm);
        SetField(level, "alarmArea", alarmCol);
        SetField(level, "alarmWaves", waves);
        SetField(level, "lampArea", lampCol);
        SetField(level, "lampGlow", lampGlow);
        SetField(level, "lampLight", lampLight);
        SetField(level, "catBubble", SmallBubble(root.transform, new Vector2(0.1f, -2.15f)));
        SetField(level, "mouseBubble", SmallBubble(root.transform, new Vector2(1.25f, -0.85f)));
        V62_Wire(level, root.transform, new Vector2(0f, 2.6f));
        return SaveLevel(root, "V62WakeKitty");
    }


}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V62 = new Dictionary<string, string[]>
    {
        { "v62_wake_kitty", new[] { "The fish is the answer, but the mouse is faster. Give the mouse something better to do first.",
            "Drag the cheese to the mouse hole, wait until the mouse takes it inside, then bring the fish right under the cat's nose." } },
    };
}
#endif
