#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;
using Gotcha.Levels.V64;

/// <summary>
/// Versione 0.86, livello 101 della roadmap (gruppo A): The Upside-Down Rain. Lo sfondo (v86_a_bg: strada sotto la
/// pioggia), il sole nascosto dietro la nuvola (col suo bagliore spento), la nuvola nera (collider generoso), l'omino ai
/// piedi del marciapiede (corpo, tre facce, gocce addosso, collider, punto in cima alla testa), la pozzanghera, l'ombrello
/// con l'origine alla mano (aperto bucato + chiuso nascosto, area della tela, un punto e un bagliore nascosto per ogni
/// buco), la pioggia (piscina di 30 gocce "s41d_raindrop" nascoste, la goccia dedicata che passa dal buco, 10 schizzi
/// "splash"), il misuratore WET sulla strada, il fumetto dell'omino (Bubble/SpeechBubble, si adatta al testo), la riga
/// dell'obiettivo e la scritta di commento (aiuti di Halloween) e la festa comune (RewardRig). Ogni TextMesh ha il suo Fit.
/// Niente ruota con il telefono: la UI resta dritta. Niente Easter egg e niente Toni. L'elenco dei livelli (Define) lo
/// aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 101: The Upside-Down Rain ----------

    static GameObject BuildV86UpsideDownRain()
    {
        var root = new GameObject("Level_V86UpsideDownRain");
        V86UpsideDownRainLevel level = root.AddComponent<V86UpsideDownRainLevel>();
        Transform r = root.transform;
        Art(r, "Street", "v86_a_bg", -20, false);

        // ---- il sole (nascosto dietro la nuvola) e la nuvola ----
        Vector2 sunAt = new Vector2(1.15f, DoodleSprites.V86ACloudY + 0.05f);
        SpriteRenderer sunGlow = Art(r, "SunGlow", "glow", -15, false, sunAt).GetComponent<SpriteRenderer>();
        sunGlow.transform.localScale = Vector3.one * 3.2f;
        sunGlow.color = new Color(1f, 0.92f, 0.5f, 0f);
        Transform sun = HW_Obj(r, "Sun", sunAt, "sun", -14, 0.6f);
        sun.gameObject.SetActive(false);
        Transform cloud = Child(r, "Cloud", new Vector2(DoodleSprites.V86ACloudX, DoodleSprites.V86ACloudY)).transform;
        SpriteRenderer cloudArt = Art(cloud, "Art", "v86_a_cloud", -10, true).GetComponent<SpriteRenderer>();
        BoxCollider2D cloudArea = HW_Box(cloud, new Vector2(4.3f, 1.5f));   // nuvola larga 3.9: area generosa

        // ---- l'omino, la pozzanghera ----
        Transform man = Child(r, "Man", new Vector2(DoodleSprites.V86AManX, DoodleSprites.V86AGroundY)).transform;
        Art(man, "Body", "v86_a_man", 10, false);
        Vector2 face = new Vector2(0f, DoodleSprites.V86AHeadY);
        Transform faceSad = Art(man, "FaceSad", "v86_a_face_sad", 11, false, face);
        Transform faceShiver = Art(man, "FaceShiver", "v86_a_face_shiver", 11, false, face);
        Transform faceHappy = Art(man, "FaceHappy", "v86_a_face_happy", 11, false, face);
        faceShiver.gameObject.SetActive(false);
        faceHappy.gameObject.SetActive(false);
        SpriteRenderer drips = Art(man, "Drips", "v86_a_drips", 12, false).GetComponent<SpriteRenderer>();
        BoxCollider2D manArea = HW_Box(man, new Vector2(1.1f, 2.05f), new Vector2(0f, 1.0f));
        Transform headTop = Child(man, "HeadTop", new Vector2(0f, DoodleSprites.V86AHeadY + DoodleSprites.V86AHeadR)).transform;
        SpriteRenderer puddle = Art(r, "Puddle", "puddle", 5, false, new Vector2(DoodleSprites.V86AManX, DoodleSprites.V86AGroundY - 0.05f)).GetComponent<SpriteRenderer>();
        puddle.transform.localScale = new Vector3(0.32f, 0.6f, 1f);

        // ---- l'ombrello (origine alla mano dell'omino) ----
        Vector2 hand = new Vector2(DoodleSprites.V86AManX + DoodleSprites.V86AHandX, DoodleSprites.V86AGroundY + DoodleSprites.V86AHandY);
        Transform umbrella = Child(r, "Umbrella", hand).transform;
        Transform open = Child(umbrella, "Open", Vector2.zero).transform;
        Art(open, "Art", "v86_a_umbrella", 9, false);
        Transform folded = Child(umbrella, "Folded", Vector2.zero).transform;
        Art(folded, "Art", "v86_a_umbrella_folded", 9, false);
        folded.gameObject.SetActive(false);
        float rimY = DoodleSprites.V86ARimY, hx = DoodleSprites.V86ACanopyHx, ch = DoodleSprites.V86ACanopyH;
        BoxCollider2D canopyArea = HW_Area(umbrella, "CanopyArea", new Vector2(2 * hx + 0.4f, ch + 0.5f), new Vector2(0f, rimY + ch * 0.5f));
        var holes = new List<Object>();
        var glows = new List<Object>();
        float[] hd = DoodleSprites.V86AHoles;
        for (int i = 0; i < hd.Length / 3; i++)
        {
            Transform h = Child(umbrella, "Hole" + i, new Vector2(hd[i * 3], hd[i * 3 + 1])).transform;
            holes.Add(h);
            Transform g = HW_Obj(h, "Glow", Vector2.zero, "glow", 13, 0.4f);
            g.GetComponentInChildren<SpriteRenderer>().color = new Color(1f, 0.95f, 0.6f, 0.8f);
            g.gameObject.SetActive(false);
            glows.Add(g);
        }

        // ---- la pioggia ----
        Transform rain = Child(r, "Rain", Vector2.zero).transform;
        var drops = new List<Object>();
        for (int i = 0; i < 30; i++)
        {
            Transform d = HW_Obj(rain, "Drop", Vector2.zero, "s41d_raindrop", 14, 0.9f);
            d.gameObject.SetActive(false);
            drops.Add(d);
        }
        Transform drip = HW_Obj(rain, "Drip", Vector2.zero, "s41d_raindrop", 14, 1.0f);
        drip.gameObject.SetActive(false);
        var splashes = new List<Object>();
        for (int i = 0; i < 10; i++)
        {
            Transform s = HW_Obj(rain, "Splash", Vector2.zero, "splash", 15, 0.25f);
            s.gameObject.SetActive(false);
            splashes.Add(s);
        }

        // ---- il misuratore WET (sulla strada, sotto il marciapiede) ----
        const float meterW = 3.3f;
        Transform meter = Child(r, "Meter", new Vector2(0f, -3.75f)).transform;
        Bar(meter, "Back", new Vector2(0.3f, 0f), new Vector2(meterW + 0.14f, 0.36f), new Color(0.1f, 0.06f, 0.16f, 0.85f), 40);
        TextMesh wetLabel = Label(meter, new Vector2(-1.85f, 0f), 42, "WET", 0.05f);
        wetLabel.color = new Color(0.56f, 0.82f, 1f);
        Fit(wetLabel, 0.75f, 0.34f);
        Transform fill = Child(meter, "Fill", new Vector2(0.3f - meterW * 0.5f, 0f)).transform;
        SpriteRenderer fillArt = Bar(fill, "Art", new Vector2(0.5f, 0f), new Vector2(1f, 0.24f), new Color(0.56f, 0.82f, 1f), 41);
        fill.localScale = new Vector3(meterW * Mathf.Clamp01(UpsideDownRainLogic.StartWet), 1f, 1f);

        SpeechBubble bubble = Bubble(r, new Vector2(0.8f, -0.2f));   // la codina punta all'omino (a destra della testa), cresce in su e a destra

        HW_Goal(r, "KEEP HIM DRY.");
        TextMesh info = HW_Toast(r, new Vector2(0f, -4.55f), out Transform infoHolder);
        SetField(level, "cloud", cloud);
        SetField(level, "cloudArt", cloudArt);
        SetField(level, "cloudArea", cloudArea);
        SetField(level, "sun", sun);
        SetField(level, "sunGlow", sunGlow);
        SetField(level, "umbrella", umbrella);
        SetField(level, "umbrellaOpen", open);
        SetField(level, "umbrellaFolded", folded);
        SetField(level, "canopyArea", canopyArea);
        SetArray(level, "holes", holes.ToArray());
        SetArray(level, "holeGlows", glows.ToArray());
        SetFloat(level, "rimY", rimY);
        SetFloat(level, "canopyHx", hx);
        SetFloat(level, "canopyH", ch);
        SetFloat(level, "holeR", 0.11f);
        SetField(level, "man", man);
        SetField(level, "manArea", manArea);
        SetField(level, "headTop", headTop);
        SetField(level, "faceSad", faceSad);
        SetField(level, "faceShiver", faceShiver);
        SetField(level, "faceHappy", faceHappy);
        SetField(level, "drips", drips);
        SetField(level, "puddle", puddle);
        SetField(level, "manBubble", bubble);
        SetArray(level, "drops", drops.ToArray());
        SetField(level, "dripDrop", drip);
        SetArray(level, "splashes", splashes.ToArray());
        SetFloat(level, "groundY", DoodleSprites.V86AGroundY);
        SetFloat(level, "cloudBottom", DoodleSprites.V86ACloudBottom);
        SetField(level, "meterFill", fill);
        SetField(level, "meterFillArt", fillArt);
        SetField(level, "meter", meter);
        SetFloat(level, "meterWidth", meterW);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, 0.45f)));
        return SaveLevel(root, "V86UpsideDownRain");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V86A = new Dictionary<string, string[]>
    {
        { "upside_down_rain", new[] { "What if rain fell the other way?",
            "Turn your phone upside down and wait." } },
    };
}
#endif
