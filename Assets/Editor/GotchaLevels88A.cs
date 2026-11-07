#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;
using Gotcha.Levels.V64;

/// <summary>
/// Versione 0.88, livello 138 della roadmap (gruppo A): The Broken Applause. Il palco vuoto ("s41b_stage_bg"), le due
/// grandi mani a guanto (radice posata dal livello con un collider generoso, figlio Look per gli effetti: quello della
/// destra rovesciato in x, così il pollice punta verso l'altra mano), la stella dell'impatto e il bagliore del battito
/// (nascosti), il pubblico (due file: 5 spettatori dietro e 4 davanti, ognuno con quattro facce e le manine che
/// applaudono, nascoste) con la fila di poltroncine rosse davanti a ogni fila, l'area del pubblico per i tocchi, il
/// fumetto degli spettatori (Bubble/SpeechBubble: si adatta al testo e resta nello schermo; il livello lo sposta sopra
/// chi parla), la parolina vicino alle mani ("pat." / "CLAP!", col suo Fit), la riga dell'obiettivo e la scritta di
/// commento (aiuti di Halloween, ognuna col suo Fit) e la festa comune (RewardRig). Disegni nuovi (prefisso v88_a_) in
/// DoodleSpritesV88A. Niente Easter egg e niente Toni. L'elenco dei livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 138: The Broken Applause ----------

    static GameObject BuildV88BrokenApplause()
    {
        var root = new GameObject("Level_V88BrokenApplause");
        V88BrokenApplauseLevel level = root.AddComponent<V88BrokenApplauseLevel>();
        Transform r = root.transform;
        Art(r, "Stage", "s41b_stage_bg", -20, false);
        const float handsY = 0.35f;
        float homeX = BrokenApplauseLogic.HomeX;

        // ---- le due mani (0 = sinistra, 1 = destra, rovesciata) ----
        string[] names = { "HandLeft", "HandRight" };
        var hands = new List<Object>();
        var looks = new List<Object>();
        var areas = new List<Object>();
        for (int i = 0; i < 2; i++)
        {
            Transform hand = Child(r, names[i], new Vector2(i == 0 ? -homeX : homeX, handsY)).transform;
            Transform look = Child(hand, "Look", Vector2.zero).transform;
            look.localScale = new Vector3(i == 0 ? 1f : -1f, 1f, 1f);
            Art(look, "Art", "v88_a_hand", 30 + i, false);   // la destra davanti: nel battito il suo pollice copre l'altra
            BoxCollider2D col = HW_Box(hand, new Vector2(1.6f, 2.3f), new Vector2(0f, 0.05f));   // bersaglio generoso (mano 1.35 x 2.1)
            hands.Add(hand);
            looks.Add(look);
            areas.Add(col);
        }
        SpriteRenderer clapGlow = Art(r, "ClapGlow", "glow", 33, false, new Vector2(0f, handsY + 0.35f)).GetComponent<SpriteRenderer>();
        clapGlow.transform.localScale = Vector3.one * 2.0f;
        clapGlow.color = new Color(1f, 0.92f, 0.55f, 0f);
        Transform impact = HW_Obj(r, "Impact", new Vector2(0f, handsY + 0.35f), "v88_a_impact", 34);
        impact.gameObject.SetActive(false);

        // ---- il pubblico: 5 dietro, 4 davanti (origine di ognuno al centro della testa) ----
        var fanPos = new List<Vector2>();
        foreach (float x in new[] { -2.0f, -1.0f, 0f, 1.0f, 2.0f }) fanPos.Add(new Vector2(x, -2.35f));
        foreach (float x in new[] { -1.5f, -0.5f, 0.5f, 1.5f }) fanPos.Add(new Vector2(x, -3.35f));
        var fans = new List<Object>();
        var bored = new List<Object>();
        var yawn = new List<Object>();
        var look2 = new List<Object>();
        var happy = new List<Object>();
        var claps = new List<Object>();
        for (int i = 0; i < fanPos.Count; i++)
        {
            int o = i < 5 ? 20 : 24;   // fila dietro 20-22 (poltroncine 23), fila davanti 24-26 (poltroncine 27)
            Transform fan = Child(r, "Fan" + i, fanPos[i]).transform;
            Art(fan, "Body", "v88_a_fan_" + ((i * 3 + 1) % 4), o, false);
            Transform fb = Art(fan, "FaceBored", "v88_a_face_bored", o + 1, false);
            Transform fy = Art(fan, "FaceYawn", "v88_a_face_yawn", o + 1, false);
            fy.gameObject.SetActive(false);
            Transform fl = Art(fan, "FaceLook", "v88_a_face_look", o + 1, false);
            fl.gameObject.SetActive(false);
            Transform fh = Art(fan, "FaceHappy", "v88_a_face_happy", o + 1, false);
            fh.gameObject.SetActive(false);
            Transform fc = Art(fan, "Clap", "v88_a_fan_clap", o + 2, false);
            fc.gameObject.SetActive(false);
            fans.Add(fan);
            bored.Add(fb);
            yawn.Add(fy);
            look2.Add(fl);
            happy.Add(fh);
            claps.Add(fc);
        }
        Art(r, "SeatsBack", "v88_a_seats", 23, false, new Vector2(0f, -2.95f));
        Art(r, "SeatsFront", "v88_a_seats", 27, false, new Vector2(0f, -3.95f));
        BoxCollider2D audienceArea = HW_Area(r, "AudienceArea", new Vector2(5.4f, 2.7f), new Vector2(0f, -3.0f));

        // ---- scritte ----
        SpeechBubble bubble = Bubble(r, new Vector2(0.05f, -1.9f));   // la punta sopra la testa di chi parla (la sposta il livello)
        TextMesh pop = Label(r, new Vector2(0f, 1.5f), 40, "pat.", 0.06f);
        pop.color = new Color(1f, 0.93f, 0.6f);
        pop.fontStyle = FontStyle.Bold;
        Fit(pop, 1.8f, 0.55f);
        pop.gameObject.SetActive(false);

        HW_Goal(r, "GET A ROUND OF APPLAUSE.");
        TextMesh info = HW_Toast(r, new Vector2(0f, 2.45f), out Transform infoHolder);
        SetArray(level, "hands", hands.ToArray());
        SetArray(level, "handLooks", looks.ToArray());
        SetArray(level, "handAreas", areas.ToArray());
        SetField(level, "impact", impact);
        SetField(level, "clapGlow", clapGlow);
        SetArray(level, "fans", fans.ToArray());
        SetArray(level, "facesBored", bored.ToArray());
        SetArray(level, "facesYawn", yawn.ToArray());
        SetArray(level, "facesLook", look2.ToArray());
        SetArray(level, "facesHappy", happy.ToArray());
        SetArray(level, "fanClaps", claps.ToArray());
        SetField(level, "audienceArea", audienceArea);
        SetField(level, "fanBubble", bubble);
        SetField(level, "popWord", pop);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, 1.9f)));
        return SaveLevel(root, "V88BrokenApplause");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V88A = new Dictionary<string, string[]>
    {
        { "broken_applause", new[] { "Use both hands... and both fingers.",
            "Hold one hand with each finger and bring them together." } },
    };
}
#endif
