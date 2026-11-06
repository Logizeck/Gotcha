#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.83, livello 47 della roadmap (gruppo A): The Wrong Prize. Lo sfondo (v83_a_bg: stadio, tribune, pista), il
/// podio con tre gradini separati (sinistra medio col 2, centro alto con l'1, destra basso col 3; ogni gradino ha la sua
/// area di rilascio generosa, dal piede del podio fino a sopra chi ci sta in piedi, mai sovrapposte), i tre numeri come
/// oggetti a sé (TextMesh su un genitore col suo BoxCollider2D sul davanti del gradino: si trascinano), il bagliore del
/// gradino sotto il dito e la scintilla. Il corridore "YOU" sul gradino basso (corpo pronto, corpo che esulta, testa con
/// tre facce, scritta della pettorina, medaglia dell'1 nascosta), la coppa avvitata sul gradino alto con i due bulloni,
/// il rivale sul gradino medio, l'arbitro sulla pista col "TWEEET!"; tre fumetti che si adattano al testo (corridore,
/// rivale, arbitro). La riga dell'obiettivo, la scritta di commento (aiuti di Halloween) e la festa comune (RewardRig, sopra
/// il podio). Ogni TextMesh ha il suo Fit. Niente Easter egg e niente Toni. L'elenco dei livelli (Define) lo aggiunge il
/// capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 47: The Wrong Prize ----------

    static readonly Color V83ADigitInk = new Color(0.16f, 0.22f, 0.45f);

    static GameObject BuildV83WrongPrize()
    {
        var root = new GameObject("Level_V83WrongPrize");
        V83WrongPrizeLevel level = root.AddComponent<V83WrongPrizeLevel>();
        Transform r = root.transform;
        Art(r, "Stadium", "v83_a_bg", -20, false);
        float gy = DoodleSprites.V83AGroundY;

        // ---- il podio: 0 sinistra (2), 1 centro (1), 2 destra (3) ----
        float[] xs = { -1.45f, 0f, 1.45f };
        float[] hs = { DoodleSprites.V83AStepMid, DoodleSprites.V83AStepHigh, DoodleSprites.V83AStepLow };
        string[] sprites = { "v83_a_step_mid", "v83_a_step_high", "v83_a_step_low" };
        string[] names = { "StepLeft", "StepCentre", "StepRight" };
        int[] startNumbers = { 2, 1, 3 };
        var steps = new List<Object>();
        var stepAreas = new List<Object>();
        var digitHolders = new Object[3];
        var digitAreas = new Object[3];
        var digitTexts = new Object[3];
        SpriteRenderer stepGlow = Art(r, "StepGlow", "glow", 19, false).GetComponent<SpriteRenderer>();
        stepGlow.transform.localScale = Vector3.one * 2.3f;
        stepGlow.color = new Color(1f, 0.92f, 0.4f, 0f);
        for (int s = 0; s < 3; s++)
        {
            Transform step = Child(r, names[s], new Vector2(xs[s], gy)).transform;
            Art(step, "Block", sprites[s], 20, false);
            // area di rilascio: tutta la colonna del gradino, dal piede fino a 1.6 sopra la cima (chi ci sta in piedi)
            float top = hs[s] + 1.6f, bottom = -0.15f;
            stepAreas.Add(HW_Box(step, new Vector2(1.4f, top - bottom), new Vector2(0f, (top + bottom) / 2f)));
            steps.Add(step);

            int n = startNumbers[s];
            Transform holder = Child(r, "Digit" + n, new Vector2(xs[s], gy + hs[s] - DoodleSprites.V83APlaqueDown)).transform;
            TextMesh t = Label(holder, Vector2.zero, 25, n.ToString(), 0.07f);
            t.color = V83ADigitInk;
            Fit(t, 0.55f, 0.6f);
            digitAreas[n - 1] = HW_Box(holder, new Vector2(1.25f, 0.9f));   // larghi 1.25 a passo 1.45: generosi, mai sovrapposti
            digitHolders[n - 1] = holder;
            digitTexts[n - 1] = t;
        }
        Transform sparkle = HW_Obj(r, "Sparkle", new Vector2(xs[2], gy + hs[2] - DoodleSprites.V83APlaqueDown), "sparkle_star", 24, 1.4f);   // dietro il numero, sopra la targa
        sparkle.gameObject.SetActive(false);

        // ---- il corridore "YOU" sul gradino basso ----
        Transform runner = Child(r, "Runner", new Vector2(xs[2], gy + hs[2])).transform;
        Transform body = Art(runner, "Body", "v83_a_runner_body", 30, false);
        Transform cheer = Art(runner, "Cheer", "v83_a_runner_cheer", 30, false);
        cheer.gameObject.SetActive(false);
        Transform head = Child(runner, "Head", new Vector2(0f, DoodleSprites.V83ANeckY)).transform;
        Art(head, "Skull", "v83_a_runner_head", 31, false);
        var faces = new List<Object>();
        foreach (string mood in new[] { "happy", "sad", "joy" })
        {
            Transform f = Art(head, "Face_" + mood, "v83_a_runner_face_" + mood, 32, false);
            f.gameObject.SetActive(mood == "happy");
            faces.Add(f);
        }
        TextMesh bib = Label(runner, new Vector2(0f, DoodleSprites.V83ABibY), 33, "YOU", 0.03f);
        bib.color = new Color(0.17f, 0.13f, 0.2f);
        Fit(bib, 0.32f, 0.18f);
        Transform medal = Child(runner, "Medal", new Vector2(0f, DoodleSprites.V83ABibY - 0.02f)).transform;
        medal.localScale = Vector3.one * 0.32f;
        Transform medalArt = Art(medal, "Art", "v79_a_medal", 34, false);
        medalArt.localRotation = Quaternion.Euler(0f, 0f, 180f);   // i nastri salgono al collo
        medalArt.localPosition = -(Vector3)medalArt.localPosition;
        TextMesh medalText = Label(medal, Vector2.zero, 35, "1", 0.08f);
        medalText.color = new Color(0.55f, 0.3f, 0.05f);
        Fit(medalText, 0.5f, 0.5f);
        medal.gameObject.SetActive(false);
        BoxCollider2D runnerCol = HW_Box(runner, new Vector2(1.1f, 1.65f), new Vector2(0f, 0.8f));
        SpeechBubble runnerBubble = Bubble(r, new Vector2(xs[2] - 0.15f, gy + hs[2] + 1.68f));
        runnerBubble.maxTextWidth = 3.4f;   // al massimo due righe: il fumetto resta sotto la riga dell'obiettivo

        // ---- la coppa avvitata sul gradino alto ----
        Transform trophy = HW_Obj(r, "Trophy", new Vector2(xs[1], gy + hs[1]), "s41e_trophy", 28, 1.35f);
        var bolts = new List<Object>();
        foreach (float bx in new[] { -0.18f, 0.18f }) bolts.Add(HW_Obj(trophy, "Bolt", new Vector2(bx, 0.07f), "v83_a_bolt", 29));
        BoxCollider2D trophyCol = HW_Box(trophy, new Vector2(0.85f, 0.85f), new Vector2(0f, 0.42f));

        // ---- il rivale sul gradino medio ----
        Transform rival = Child(r, "Rival", new Vector2(xs[0], gy + hs[0])).transform;
        Transform rivalSmug = Art(rival, "Smug", "v83_a_rival_smug", 30, false);
        Transform rivalShock = Art(rival, "Shock", "v83_a_rival_shock", 30, false);
        rivalShock.gameObject.SetActive(false);
        BoxCollider2D rivalCol = HW_Box(rival, new Vector2(1.1f, 1.6f), new Vector2(0f, 0.8f));
        SpeechBubble rivalBubble = Bubble(r, new Vector2(xs[0] + 0.25f, gy + hs[0] + 1.6f));
        rivalBubble.maxTextWidth = 3.6f;   // al massimo due righe

        // ---- l'arbitro sulla pista ----
        Transform referee = Child(r, "Referee", new Vector2(-1.75f, -4.15f)).transform;
        Transform refIdle = Art(referee, "Idle", "v83_a_referee", 40, false);
        Transform refBlow = Art(referee, "Blow", "v83_a_referee_blow", 40, false);
        refBlow.gameObject.SetActive(false);
        BoxCollider2D refereeCol = HW_Box(referee, new Vector2(1.1f, 1.6f), new Vector2(0f, 0.8f));
        TextMesh tweet = Label(Child(r, "Tweet", new Vector2(-0.45f, -2.88f)).transform, Vector2.zero, 42, "TWEEET!", 0.05f);
        tweet.color = new Color(0.9f, 0.18f, 0.2f);
        Fit(tweet, 1.3f, 0.4f);
        tweet.transform.localRotation = Quaternion.Euler(0f, 0f, 10f);
        tweet.gameObject.SetActive(false);
        SpeechBubble refereeBubble = Bubble(r, new Vector2(-1.45f, -2.6f));
        refereeBubble.maxTextWidth = 4.2f;   // battute corte su una riga: il fumetto resta sotto il podio, non copre i numeri

        SpriteRenderer puff = Art(r, "Puff", "puff", 47, false).GetComponent<SpriteRenderer>();
        puff.gameObject.SetActive(false);

        HW_Goal(r, "WIN FIRST PRIZE.");
        TextMesh info = HW_Toast(r, new Vector2(0f, -4.55f), out Transform infoHolder);
        SetArray(level, "steps", steps.ToArray());
        SetArray(level, "stepAreas", stepAreas.ToArray());
        SetArray(level, "digits", digitHolders);
        SetArray(level, "digitAreas", digitAreas);
        SetArray(level, "digitTexts", digitTexts);
        SetField(level, "stepGlow", stepGlow);
        SetField(level, "sparkle", sparkle);
        SetField(level, "runner", runner);
        SetField(level, "runnerArea", runnerCol);
        SetField(level, "runnerBody", body);
        SetField(level, "runnerCheer", cheer);
        SetField(level, "runnerHead", head);
        SetArray(level, "runnerFaces", faces.ToArray());
        SetField(level, "medal", medal);
        SetField(level, "trophy", trophy);
        SetField(level, "trophyArea", trophyCol);
        SetArray(level, "bolts", bolts.ToArray());
        SetField(level, "rival", rival);
        SetField(level, "rivalArea", rivalCol);
        SetField(level, "rivalSmug", rivalSmug);
        SetField(level, "rivalShock", rivalShock);
        SetField(level, "referee", referee);
        SetField(level, "refereeArea", refereeCol);
        SetField(level, "refereeIdle", refIdle);
        SetField(level, "refereeBlow", refBlow);
        SetField(level, "tweetText", tweet);
        SetField(level, "puff", puff);
        SetField(level, "runnerBubble", runnerBubble);
        SetField(level, "rivalBubble", rivalBubble);
        SetField(level, "refereeBubble", refereeBubble);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, 2.35f)));
        return SaveLevel(root, "V83WrongPrize");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V83A = new Dictionary<string, string[]>
    {
        { "wrong_prize", new[] { "Look at the numbers on the steps.",
            "Drag the number 1 onto your step." } },
    };
}
#endif
