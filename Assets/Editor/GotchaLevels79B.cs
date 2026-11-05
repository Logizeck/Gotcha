#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.79, livello 24 della roadmap (gruppo B): The Lazy Sun. Il cielo di notte con sopra quello dell'alba
/// (trasparente all'inizio), le stelline ("sparkle_star"), la luna che parla ("s41k_moon"), il sole DIETRO la collina
/// (bagliore "glow", corpo coi raggi, faccia addormentata e faccia sveglia, berretto da notte), i "Zzz" ("zzz", dietro la
/// collina: spuntano dal bordo), la grande collina tonda che si trascina in giù, il prato davanti con lo steccato
/// ("sheep_fence") e il gallo (tre pose), i tre fumetti che si adattano al testo (Bubble/SpeechBubble), la riga
/// dell'obiettivo e la scritta di commento (aiuti di Halloween, ognuna col suo Fit) e la festa comune (RewardRig).
/// Disegni nuovi (prefisso v79_b_) in DoodleSpritesV79B. Niente Easter egg e niente Toni. L'elenco dei livelli (Define)
/// lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 24: The Lazy Sun ----------

    static GameObject BuildV79LazySun()
    {
        var root = new GameObject("Level_V79LazySun");
        V79LazySunLevel level = root.AddComponent<V79LazySunLevel>();
        Transform r = root.transform;

        // ---- il cielo: notte, alba sopra (trasparente), stelline, luna ----
        Art(r, "SkyNight", "v79_b_sky_night", -30, false);
        SpriteRenderer dawn = Art(r, "SkyDawn", "v79_b_sky_dawn", -29, false).GetComponent<SpriteRenderer>();
        dawn.color = new Color(1f, 1f, 1f, 0f);
        var stars = new List<Transform>();
        Vector3[] starAt =
        {
            new Vector3(-2.3f, 1.2f, 0.32f), new Vector3(-0.35f, 2.75f, 0.42f), new Vector3(0.55f, 1.75f, 0.3f),
            new Vector3(1.35f, 2.85f, 0.38f), new Vector3(2.25f, 2.05f, 0.34f), new Vector3(1.85f, 0.95f, 0.28f),
            new Vector3(-1.2f, 1.05f, 0.26f), new Vector3(0.1f, 0.95f, 0.24f), new Vector3(-2.45f, 2.75f, 0.3f),
        };
        foreach (Vector3 s in starAt)
        {
            Transform st = HW_Obj(r, "Star", new Vector2(s.x, s.y), "sparkle_star", -28, s.z);
            st.GetComponentInChildren<SpriteRenderer>().color = new Color(1f, 0.96f, 0.75f);
            stars.Add(st);
        }
        Transform moon = HW_Obj(r, "Moon", new Vector2(-1.55f, 2.2f), "s41k_moon", -27, 0.7f);
        CircleCollider2D moonCol = moon.gameObject.AddComponent<CircleCollider2D>();
        moonCol.radius = 1.1f;   // 0.77 nel mondo: bersaglio generoso

        // ---- il sole, dietro la collina (centro in -0.45, -1.25: tutto coperto all'inizio) ----
        Vector2 sunAt = new Vector2(-0.45f, -1.25f);
        SpriteRenderer glow = Art(r, "SunGlow", "glow", -26, false, sunAt).GetComponent<SpriteRenderer>();
        glow.transform.localScale = Vector3.one * 4.2f;
        glow.color = new Color(1f, 0.85f, 0.45f, 0f);
        Transform sun = Child(r, "Sun", sunAt).transform;
        Transform body = Child(sun, "Body", Vector2.zero).transform;
        Art(body, "Art", "v79_b_sun_body", -25, false);
        Transform faceSleep = HW_Obj(sun, "FaceSleep", Vector2.zero, "v79_b_sun_face_sleep", -24);
        Transform faceAwake = HW_Obj(sun, "FaceAwake", Vector2.zero, "v79_b_sun_face_awake", -24);
        faceAwake.gameObject.SetActive(false);
        Transform cap = HW_Obj(sun, "Nightcap", new Vector2(DoodleSprites.V79BCapX, DoodleSprites.V79BCapY), "v79_b_nightcap", -23);
        cap.localRotation = Quaternion.Euler(0f, 0f, -8f);
        var zzz = new List<Transform>();
        for (int i = 0; i < 3; i++)
        {
            Transform z = HW_Obj(r, "Zzz", sunAt + new Vector2(0.45f, 0.75f), "zzz", -22, 1.3f);
            z.gameObject.SetActive(false);
            zzz.Add(z);
        }

        // ---- la collina (origine sulla cima, y 0.35) e il prato davanti con lo steccato e il gallo ----
        Transform hill = HW_Obj(r, "Hill", new Vector2(0f, 0.35f), "v79_b_hill", -10);
        SpriteRenderer meadow = Art(r, "Meadow", "v79_b_meadow", 0, false).GetComponent<SpriteRenderer>();
        Transform fence = HW_Obj(r, "Fence", new Vector2(1.5f, -3.95f), "sheep_fence", 2);
        Transform rooster = Child(r, "Rooster", new Vector2(1.5f, -3.15f)).transform;
        Transform rSleep = HW_Obj(rooster, "Sleep", Vector2.zero, "v79_b_rooster_sleep", 3);
        Transform rPeek = HW_Obj(rooster, "Peek", Vector2.zero, "v79_b_rooster_peek", 3);
        Transform rCrow = HW_Obj(rooster, "Crow", Vector2.zero, "v79_b_rooster_crow", 3);
        rPeek.gameObject.SetActive(false);
        rCrow.gameObject.SetActive(false);
        BoxCollider2D roosterCol = HW_Box(rooster, new Vector2(1.3f, 1.4f), new Vector2(0f, 0.55f));

        // ---- i fumetti (punta della codina = posizione - (0.85, 0.8)) ----
        SpeechBubble moonBubble = Bubble(r, new Vector2(-0.95f + 0.85f, 1.65f + 0.8f));
        SpeechBubble roosterBubble = Bubble(r, new Vector2(1.1f + 0.85f, -2.25f + 0.8f));
        SpeechBubble sunBubble = Bubble(r, new Vector2(0.3f + 0.85f, -0.35f + 0.8f));

        HW_Goal(r, "MAKE THE SUN RISE.");
        TextMesh info = HW_Toast(r, new Vector2(0f, 1.0f), out Transform infoHolder);
        SetField(level, "skyDawn", dawn);
        SetArray(level, "stars", stars.ToArray());
        SetField(level, "moon", moon);
        SetField(level, "moonArea", moonCol);
        SetField(level, "sun", sun);
        SetField(level, "sunBody", body);
        SetField(level, "sunFaceSleep", faceSleep);
        SetField(level, "sunFaceAwake", faceAwake);
        SetField(level, "nightcap", cap);
        SetField(level, "sunGlow", glow);
        SetFloat(level, "sunRadius", DoodleSprites.V79BSunRadius);
        SetArray(level, "zzz", zzz.ToArray());
        SetField(level, "hill", hill);
        SetFloat(level, "hillTravel", 3.1f);
        SetFloat(level, "hillHalfWidth", DoodleSprites.V79BHillHalfW);
        SetFloat(level, "hillHeight", DoodleSprites.V79BHillHeight);
        SetFloat(level, "meadowTop", DoodleSprites.V79BMeadowTop);
        SetArray(level, "landTint", new Object[] { hill.GetComponentInChildren<SpriteRenderer>(), meadow, fence.GetComponentInChildren<SpriteRenderer>() });
        SetArray(level, "softTint", new Object[] { rSleep.GetComponentInChildren<SpriteRenderer>(), rPeek.GetComponentInChildren<SpriteRenderer>(), rCrow.GetComponentInChildren<SpriteRenderer>() });
        SetField(level, "rooster", rooster);
        SetField(level, "roosterArea", roosterCol);
        SetField(level, "roosterSleep", rSleep);
        SetField(level, "roosterPeek", rPeek);
        SetField(level, "roosterCrow", rCrow);
        SetField(level, "moonBubble", moonBubble);
        SetField(level, "roosterBubble", roosterBubble);
        SetField(level, "sunBubble", sunBubble);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r));
        return SaveLevel(root, "V79LazySun");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V79B = new Dictionary<string, string[]>
    {
        { "lazy_sun", new[] { "Look where the snoring comes from.",
            "Drag the hill down to uncover the sun." } },
    };
}
#endif
