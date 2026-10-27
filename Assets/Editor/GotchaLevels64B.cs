#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.64 (parte B): livelli 255 "The Invisible Password" e 256 "The Incompetent Magician".
/// Riusano lo specchio ovale, il monitor, i tasti della tastiera, il mago/coniglio/cilindro/bacchetta/palco di
/// "The cheap magician" e gli aiuti dello speciale Halloween (HW_Goal, HW_Obj, HW_Box, HW_Area, HW_Toast) e della 0.62
/// (V62_Wire). Disegni nuovi (prefisso v64_pw_ / v64_mg_) in DoodleSpritesV64B. Niente Easter egg, niente Toni.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 255 The Invisible Password ----------

    static GameObject BuildV64InvisiblePassword()
    {
        var root = new GameObject("Level_V64InvisiblePassword");
        V64InvisiblePasswordLevel level = root.AddComponent<V64InvisiblePasswordLevel>();
        Transform r = root.transform;
        Art(r, "Corridor", "v64_pw_bg", -20, false);

        // dietro la porta: lo sgabuzzino al buio, col mocio appoggiato
        Vector2 doorAt = new Vector2(-0.5f, -0.85f);
        Art(r, "Closet", "v64_pw_closet", -8, false, doorAt);
        Transform mop = Child(r, "Mop", doorAt + new Vector2(0.15f, -1.8f)).transform;
        mop.localRotation = Quaternion.Euler(0f, 0f, -7f);
        SpriteRenderer mopArt = Art(mop, "Art", "v64_pw_mop", -7, false).GetComponent<SpriteRenderer>();
        SpriteRenderer darkness = Bar(r, "Darkness", doorAt, new Vector2(2.1f, 3.7f), new Color(0.03f, 0.03f, 0.05f, 0.94f), -6);

        // la porta: davanti e retro (si scambiano oltre i 90°), area per bussare
        Transform door = Child(r, "Door", doorAt).transform;
        Transform front = Child(door, "Front", Vector2.zero).transform;
        SpriteRenderer frontArt = Art(front, "Art", "v64_pw_door_front", 2, false).GetComponent<SpriteRenderer>();
        Transform back = Child(door, "Back", Vector2.zero).transform;
        SpriteRenderer backArt = Art(back, "Art", "v64_pw_door_back", 2, false).GetComponent<SpriteRenderer>();
        BoxCollider2D doorCol = HW_Area(r, "DoorArea", new Vector2(2.1f, 3.7f), doorAt);

        // i due cardini sul bordo sinistro, coi perni (quello sotto capovolto: si sfila verso il basso)
        Transform hingeTop = HW_Obj(r, "HingeTop", new Vector2(-1.55f, 0.45f), "v64_pw_hinge", 6, 1.3f);
        Transform pinTop = HW_Obj(r, "PinTop", new Vector2(-1.55f, 0.45f), "v64_pw_pin", 5, 1.3f);
        Transform hingeBottom = HW_Obj(r, "HingeBottom", new Vector2(-1.55f, -2.15f), "v64_pw_hinge", 6, 1.3f);
        Transform pinBottom = HW_Obj(r, "PinBottom", new Vector2(-1.55f, -2.15f), "v64_pw_pin", 5, 1.3f);
        pinBottom.localScale = new Vector3(1.3f, -1.3f, 1f);
        BoxCollider2D hingeTopCol = HW_Box(hingeTop, new Vector2(0.8f, 0.8f));       // ~1 unità nel mondo
        BoxCollider2D hingeBottomCol = HW_Box(hingeBottom, new Vector2(0.8f, 0.8f));

        // cartello, monitor della telecamera, specchio "decorativo" (col riflesso nascosto)
        Transform sign = HW_Obj(r, "Sign", new Vector2(-0.5f, 1.62f), "v64_pw_sign", 3, 0.8f);
        TextMesh signText = Label(sign, new Vector2(0f, 0.02f), 4, "TOP SECRET", 0.05f);
        Fit(signText, 1.5f, 0.3f);
        BoxCollider2D signCol = HW_Box(sign, new Vector2(2.0f, 0.8f));
        Transform cctv = HW_Obj(r, "Cctv", new Vector2(-1.85f, 2.3f), "s41c_monitor", 3, 0.3f);
        Transform feed = Art(cctv, "Feed", "v64_pw_door_front", 4, false, new Vector2(0.55f, 0.3f));
        feed.localScale = Vector3.one * 0.36f;
        TextMesh cctvText = Label(cctv, new Vector2(-0.55f, 0.3f), 5, "CAM 1  REC", 0.12f);
        cctvText.color = new Color(1f, 0.45f, 0.4f);
        Fit(cctvText, 1.2f, 0.6f);
        BoxCollider2D cctvCol = HW_Box(cctv, new Vector2(3.2f, 3.0f), new Vector2(0f, -0.1f));
        Transform mirror = HW_Obj(r, "Mirror", new Vector2(1.5f, 1.9f), "mirror", 1, 0.64f);
        SpriteRenderer reflect = Art(mirror, "Reflection", "v64_pw_mirror_reflect", 2, false).GetComponent<SpriteRenderer>();
        SpriteRenderer sparkle = Art(mirror, "Sparkle", "sparkle_star", 3, false, new Vector2(0.75f, 0.85f)).GetComponent<SpriteRenderer>();
        BoxCollider2D mirrorCol = HW_Box(mirror, new Vector2(2.6f, 3.2f));

        // console della tastiera in primo piano: display, spia, FORGOT PASSWORD, dieci tasti
        Art(r, "Console", "v64_pw_console", 10, false);
        TextMesh display = Label(r, new Vector2(-0.9f, -3.27f), 12, "_ _ _ _", 0.06f);
        display.color = new Color(0.45f, 1f, 0.55f);
        Fit(display, 2.8f, 0.36f);
        BoxCollider2D displayCol = HW_Area(r, "DisplayArea", new Vector2(3.0f, 0.5f), new Vector2(-0.9f, -3.27f));
        Transform lamp = HW_Obj(r, "StatusLamp", new Vector2(0.84f, -3.27f), "v64_pw_lamp", 11, 1.3f);
        SpriteRenderer lampArt = lamp.Find("Art").GetComponent<SpriteRenderer>();
        Transform forgot = HW_Obj(r, "Forgot", new Vector2(1.78f, -3.27f), "v64_pw_button", 11, 0.8f);
        forgot.Find("Art").GetComponent<SpriteRenderer>().color = new Color(0.75f, 0.86f, 1f);
        TextMesh forgotText = Label(forgot, Vector2.zero, 12, "FORGOT PASSWORD?", 0.035f);
        Fit(forgotText, 1.4f, 0.3f);
        BoxCollider2D forgotCol = HW_Box(forgot, new Vector2(1.75f, 0.65f));
        var keys = new Transform[10];
        var keyCols = new Collider2D[10];
        for (int i = 0; i < 10; i++)
        {
            int digit = (i + 1) % 10;
            Vector2 at = new Vector2(-1.9f + 0.95f * (i % 5), i < 5 ? -3.8f : -4.4f);
            Transform k = HW_Obj(r, "Key" + digit, at, "keypad_key", 11, 2.0f);
            TextMesh t = Label(k, Vector2.zero, 12, digit.ToString(), 0.03f);
            Fit(t, 0.3f, 0.26f);
            keys[digit] = k;
            keyCols[digit] = HW_Box(k, new Vector2(0.44f, 0.29f));
        }

        HW_Goal(r, "GUESS THE PASSWORD.");
        SetField(level, "door", door);
        SetField(level, "doorFront", front);
        SetField(level, "doorBack", back);
        SetField(level, "doorFrontArt", frontArt);
        SetField(level, "doorBackArt", backArt);
        SetField(level, "doorArea", doorCol);
        SetField(level, "hingeTop", hingeTop);
        SetField(level, "hingeBottom", hingeBottom);
        SetField(level, "hingeTopArea", hingeTopCol);
        SetField(level, "hingeBottomArea", hingeBottomCol);
        SetField(level, "pinTop", pinTop);
        SetField(level, "pinBottom", pinBottom);
        SetField(level, "mirror", mirror);
        SetField(level, "mirrorReflection", reflect);
        SetField(level, "mirrorArea", mirrorCol);
        SetField(level, "mirrorSparkle", sparkle);
        SetField(level, "cctvArea", cctvCol);
        SetField(level, "cctvText", cctvText);
        SetField(level, "signArea", signCol);
        SetField(level, "statusLamp", lampArt);
        SetArray(level, "keys", keys);
        SetArray(level, "keyAreas", keyCols);
        SetField(level, "forgotArea", forgotCol);
        SetField(level, "displayArea", displayCol);
        SetField(level, "display", display);
        SetField(level, "darkness", darkness);
        SetField(level, "mop", mop);
        SetField(level, "mopArt", mopArt);
        V62_Wire(level, r, new Vector2(0f, 2.62f));
        return SaveLevel(root, "V64InvisiblePassword");
    }

    // ---------- 256 The Incompetent Magician ----------

    static GameObject BuildV64IncompetentMagician()
    {
        var root = new GameObject("Level_V64IncompetentMagician");
        V64IncompetentMagicianLevel level = root.AddComponent<V64IncompetentMagicianLevel>();
        Transform r = root.transform;
        Art(r, "Stage", "s41b_stage_bg", -20, false);
        Transform spot = HW_Obj(r, "Spotlight", new Vector2(-1.35f, 3.0f), "light_cone", -12);
        spot.localScale = new Vector3(0.55f, 0.72f, 1f);
        spot.Find("Art").GetComponent<SpriteRenderer>().color = new Color(1f, 0.95f, 0.75f, 0.6f);

        // il mago (sotto il faro): occhi chiusi e guanto sopra il suo disegno, bacchetta in mano
        const float magScale = 1.55f;
        Transform mag = Child(r, "Magician", new Vector2(-1.35f, -0.45f)).transform;
        mag.localScale = Vector3.one * magScale;
        Art(mag, "Art", "s41b_magician", 4, false);
        SpriteRenderer eyes = Art(mag, "EyesClosed", "v64_mg_eyes_closed", 5, false).GetComponent<SpriteRenderer>();
        Transform glove = Child(mag, "Glove", new Vector2(0f, 0.6f)).transform;
        Art(glove, "Art", "v64_mg_glove", 6, false);
        Transform wand = Child(mag, "Wand", new Vector2(0.45f, 0.15f)).transform;
        Art(wand, "Art", "s41b_wand", 5, false);
        BoxCollider2D wandCol = HW_Box(wand, new Vector2(0.55f, 0.55f), new Vector2(0.22f, 0.22f));
        // zona degli occhi generosa (circa 1.4 x 1.1 nel mondo) e corpo
        BoxCollider2D faceCol = HW_Area(mag, "Face", new Vector2(1.4f / magScale, 1.1f / magScale), new Vector2(0f, 0.58f));
        BoxCollider2D bodyCol = HW_Area(mag, "Body", new Vector2(1.1f, 1.9f), new Vector2(0f, -0.25f));
        SmokeEmitter wandConfetti = Emitter(r, "WandConfetti", new Vector2(-0.03f, 0.4f), "confetti", 40f, 1.1f, 0.8f, 0.5f, 1.3f, new Color(1f, 0.85f, 0.35f), 40);
        wandConfetti.spinRange = 300f;
        wandConfetti.growth = 0f;

        // tre scatole magiche in fondo, il cilindro, il coniglio arrabbiato
        var boxes = new Transform[3];
        var boxCols = new Collider2D[3];
        Color[] tints = { new Color(1f, 0.62f, 0.62f), new Color(0.62f, 0.76f, 1f), new Color(0.66f, 1f, 0.62f) };
        for (int i = 0; i < 3; i++)
        {
            boxes[i] = HW_Obj(r, "Box" + i, new Vector2(0.05f + 0.8f * i, -1.2f), "v64_mg_box", 3, 0.75f);
            boxes[i].Find("Art").GetComponent<SpriteRenderer>().color = tints[i];
            boxCols[i] = HW_Box(boxes[i], new Vector2(1.0f, 1.05f), new Vector2(0.03f, 0.03f));
        }
        Vector2 hatAt = new Vector2(0.1f, -2.1f);
        Transform hatBack = HW_Obj(r, "HatBack", hatAt, "s41b_tophat_back", 5);
        Transform hat = HW_Obj(r, "Hat", hatAt, "s41b_tophat", 7);
        BoxCollider2D hatCol = HW_Box(hat, new Vector2(1.2f, 1.1f));
        Transform rabbit = HW_Obj(r, "Rabbit", new Vector2(1.25f, -2.2f), "s41b_rabbit", 8, 1.1f);
        Transform brows = Art(rabbit, "Brows", "v64_mg_rabbit_brows", 9, false);
        Transform paw = Child(rabbit, "Paw", new Vector2(0.15f, -0.18f)).transform;
        Art(paw, "Art", "v64_mg_rabbit_paw", 9, false);
        BoxCollider2D rabbitCol = HW_Box(rabbit, new Vector2(0.85f, 1.1f));
        SpriteRenderer puff = Art(r, "Puff", "puff", 12, false).GetComponent<SpriteRenderer>();

        // la botola nascosta tra due assi del palco
        Transform trap = Child(r, "Trapdoor", new Vector2(0.45f, -3.42f)).transform;
        SpriteRenderer hole = Art(trap, "Hole", "v64_mg_trap_hole", 2, false).GetComponent<SpriteRenderer>();
        Transform lid = Child(trap, "Lid", Vector2.zero).transform;
        Art(lid, "Art", "v64_mg_trapdoor", 3, false);
        SpriteRenderer glow = Art(trap, "Glow", "v64_mg_trap_glow", 4, false).GetComponent<SpriteRenderer>();
        glow.color = new Color(1f, 0.9f, 0.35f, 0.6f);
        BoxCollider2D trapCol = HW_Box(trap, new Vector2(1.4f, 0.8f));

        // il coniglio col cartello dietro il sipario destro, il pubblico in controluce
        Transform signRabbit = Child(r, "SignRabbit", new Vector2(2.95f, -1.3f)).transform;
        Art(signRabbit, "Rabbit", "s41b_rabbit", 9, false);
        Art(signRabbit, "Brows", "v64_mg_rabbit_brows", 10, false);
        Transform board = Child(signRabbit, "Sign", new Vector2(-0.1f, 0.95f)).transform;
        Art(board, "Art", "v64_mg_sign", 10, false);
        TextMesh boardText = Label(board, Vector2.zero, 11, "WHAT AN\nIDIOT", 0.045f);
        Fit(boardText, 0.95f, 0.45f);
        Art(r, "CurtainFront", "v64_mg_curtain", 12, false);
        Transform audience = Art(r, "Audience", "v64_mg_audience", 15, false);

        // alternativa senza microfono (compare solo a occhi coperti)
        Transform badge = HW_Obj(r, "MicBadge", new Vector2(1.3f, -0.45f), "v64_mg_mic_off", 20, 1.2f);
        TextMesh badgeText = Label(badge, new Vector2(0f, 0.42f), 21, "NO MIC? KEEP HOLDING...", 0.025f);
        badgeText.color = Color.white;
        Fit(badgeText, 1.8f, 0.2f);
        Transform gauge = GaugeBar(badge, "Gauge", new Vector2(0f, -0.4f), new Vector2(0.8f, 0.1f), new Color(0.1f, 0.06f, 0.16f, 0.8f), new Color(1f, 0.85f, 0.35f), 21);

        HW_Goal(r, "MAKE THE RABBIT DISAPPEAR.");
        SetField(level, "magician", mag);
        SetField(level, "faceArea", faceCol);
        SetField(level, "magicianArea", bodyCol);
        SetField(level, "eyesClosed", eyes);
        SetField(level, "glove", glove);
        SetField(level, "wand", wand);
        SetField(level, "wandArea", wandCol);
        SetField(level, "wandConfetti", wandConfetti);
        SetField(level, "rabbit", rabbit);
        SetField(level, "rabbitArea", rabbitCol);
        SetField(level, "rabbitBrows", brows);
        SetField(level, "rabbitPaw", paw);
        SetField(level, "hat", hat);
        SetField(level, "hatBack", hatBack);
        SetField(level, "hatArea", hatCol);
        SetArray(level, "boxes", boxes);
        SetArray(level, "boxAreas", boxCols);
        SetField(level, "puff", puff);
        SetField(level, "trapdoorLid", lid);
        SetField(level, "trapdoorArea", trapCol);
        SetField(level, "trapHole", hole);
        SetField(level, "trapGlow", glow);
        SetField(level, "audience", audience);
        SetField(level, "signRabbit", signRabbit);
        SetField(level, "micBadge", badge);
        SetField(level, "micGauge", gauge);
        SetField(level, "magicianBubble", SmallBubble(r, new Vector2(-0.25f, 1.75f)));
        SetField(level, "rabbitBubble", SmallBubble(r, new Vector2(1.25f, -0.55f)));
        V62_Wire(level, r, new Vector2(0f, 2.62f));
        return SaveLevel(root, "V64IncompetentMagician");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V64B = new Dictionary<string, string[]>
    {
        { "v64_invisible_password", new[] { "Security doors have hinges for a reason.",
            "The answer is easier to read from another perspective." } },
        { "v64_incompetent_magician", new[] { "The rabbit seems smarter than the magician.",
            "Maybe the magician shouldn't see how the trick works." } },
    };
}
#endif
