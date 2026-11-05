#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.82, livello 44 della roadmap (gruppo B): The Stupid Genius 2. Lo sfondo (v82_b_bg: aula con la lavagna
/// grande murata, la console dei quiz), l'equazione "2 + 2 = 5" scritta col gesso (un TextMesh per simbolo, ognuno con il
/// suo genitore e il suo BoxCollider2D; il risultato ha l'area più generosa), la copia del 4 che vola nell'equazione
/// (nascosta) e la scintilla della correzione. Il genio a pezzi (radice a terra in scala 0.85, corpo, testa sul collo con
/// quattro facce, capelli in ordine e dritti, occhialoni) con il suo collider; un fumetto che si adatta al testo. Sotto, i
/// tre pulsanti-risposta (tasto v82_b_key, numero come testo, collider generosi e mai sovrapposti; il valore logico è
/// buttonValues del livello, le etichette le riscrive il livello) e il bagliore del pulsante giusto. La nuvoletta e i
/// frammenti di gesso (nascosti), la riga dell'obiettivo, la scritta di commento (aiuti di Halloween) e la festa comune
/// (RewardRig, sopra la lavagna). Ogni TextMesh ha il suo Fit. Niente Easter egg e niente Toni. L'elenco dei livelli
/// (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 44: The Stupid Genius 2 ----------

    static readonly Color V82BChalkInk = new Color(0.96f, 0.96f, 0.92f);
    static readonly Color V82BKeyInk = new Color(0.17f, 0.13f, 0.2f);

    static GameObject BuildV82StupidGenius2()
    {
        var root = new GameObject("Level_V82StupidGenius2");
        V82StupidGenius2Level level = root.AddComponent<V82StupidGenius2Level>();
        Transform r = root.transform;
        Art(r, "Classroom", "v82_b_bg", -20, false);

        // ---- l'equazione sulla lavagna: 2 + 2 = 5 (un simbolo per oggetto) ----
        Transform eq = Child(r, "Equation", new Vector2(0f, DoodleSprites.V82BBoardY + 0.05f)).transform;
        string[] glyphs = { "2", "+", "2", "=", "5" };
        string[] names = { "TwoA", "Plus", "TwoB", "Equals", "Result" };
        var symbols = new List<Object>();
        var areas = new List<Object>();
        TextMesh result = null;
        for (int i = 0; i < glyphs.Length; i++)
        {
            Transform holder = Child(eq, names[i], new Vector2(-1.6f + 0.8f * i, 0f)).transform;
            TextMesh t = Label(holder, Vector2.zero, 5, glyphs[i], 0.11f);
            t.color = V82BChalkInk;
            Fit(t, 0.8f, 1.0f);
            bool res = i == glyphs.Length - 1;
            // il risultato ha l'area più grande (ha la precedenza sull'"=" accanto); gli altri bastano per farli oscillare
            areas.Add(HW_Box(holder, res ? new Vector2(1.2f, 1.5f) : new Vector2(0.7f, 1.2f)));
            symbols.Add(holder);
            if (res) result = t;
        }
        Transform sparkle = HW_Obj(eq, "Sparkle", new Vector2(1.6f, 0f), "sparkle_star", 4, 1.6f);
        sparkle.gameObject.SetActive(false);
        TextMesh newDigit = Label(Child(r, "NewDigitHolder", Vector2.zero).transform, Vector2.zero, 46, "4", 0.11f);
        newDigit.color = V82BChalkInk;
        Fit(newDigit, 0.8f, 1.0f);
        newDigit.gameObject.SetActive(false);

        // ---- il genio (a sinistra, sotto la lavagna) ----
        Transform genius = Child(r, "Genius", new Vector2(-1.35f, -2.6f)).transform;
        genius.localScale = Vector3.one * 0.85f;
        Art(genius, "Body", "v82_b_body", 10, false);
        Transform head = Child(genius, "Head", new Vector2(0f, DoodleSprites.V82BNeckY)).transform;
        Art(head, "Skull", "v82_b_head", 11, false);
        var faces = new List<Object>();
        foreach (string mood in new[] { "smug", "shout", "panic", "shock" })
        {
            Transform f = Art(head, "Face_" + mood, "v82_b_face_" + mood, 12, false);
            f.gameObject.SetActive(mood == "smug");
            faces.Add(f);
        }
        Transform hair = Art(head, "Hair", "v82_b_hair", 13, false);
        Transform hairUp = Art(head, "HairUp", "v82_b_hair_up", 13, false);
        hairUp.gameObject.SetActive(false);
        Transform glasses = HW_Obj(head, "Glasses", new Vector2(0f, DoodleSprites.V82BEyeY), "v82_b_glasses", 14);
        BoxCollider2D geniusCol = HW_Box(genius, new Vector2(1.5f, 2.9f), new Vector2(0f, 1.45f));
        SpeechBubble bubble = Bubble(r, new Vector2(-0.65f, -1.45f));
        bubble.maxTextWidth = 2.9f;   // righe più larghe: al massimo tre righe, il fumetto non copre l'equazione

        // ---- i pulsanti-risposta sulla console ----
        int[] values = { 3, 4, 5 };
        var buttons = new List<Object>();
        var bodies = new List<Object>();
        var buttonAreas = new List<Object>();
        var labels = new List<Object>();
        SpriteRenderer answerGlow = Art(r, "AnswerGlow", "glow", 19, false).GetComponent<SpriteRenderer>();
        answerGlow.transform.localScale = Vector3.one * 2.7f;
        answerGlow.color = new Color(1f, 0.9f, 0.35f, 0f);
        for (int i = 0; i < values.Length; i++)
        {
            Transform b = Child(r, "Answer" + values[i], new Vector2(-1.55f + 1.55f * i, -3.5f)).transform;
            Transform body = Child(b, "Body", Vector2.zero).transform;
            Art(body, "Key", "v82_b_key", 20, false);
            TextMesh label = Label(body, new Vector2(0f, 0.05f), 21, values[i].ToString(), 0.085f);
            label.color = V82BKeyInk;
            Fit(label, 0.9f, 0.72f);
            buttonAreas.Add(HW_Box(b, new Vector2(1.45f, 1.3f)));   // larghi 1.45 a passo 1.55: generosi, mai sovrapposti
            buttons.Add(b);
            bodies.Add(body);
            labels.Add(label);
        }

        // ---- il 5 che si frantuma (nascosti) ----
        SpriteRenderer puff = Art(r, "Puff", "puff", 47, false).GetComponent<SpriteRenderer>();
        puff.gameObject.SetActive(false);
        var bits = new List<Object>();
        for (int i = 0; i < 5; i++)
        {
            Transform bit = HW_Obj(r, "ChalkBit" + i, Vector2.zero, "v82_b_chalk_bit", 48);
            bit.gameObject.SetActive(false);
            bits.Add(bit);
        }

        HW_Goal(r, "PROVE YOU'RE SMARTER THAN THE GENIUS.");
        TextMesh info = HW_Toast(r, new Vector2(0f, -4.55f), out Transform infoHolder);
        SetArray(level, "symbols", symbols.ToArray());
        SetArray(level, "symbolAreas", areas.ToArray());
        SetField(level, "resultText", result);
        SetField(level, "newDigit", newDigit);
        SetField(level, "sparkle", sparkle);
        SetField(level, "genius", genius);
        SetField(level, "geniusArea", geniusCol);
        SetField(level, "head", head);
        SetArray(level, "faces", faces.ToArray());
        SetField(level, "hair", hair);
        SetField(level, "hairUp", hairUp);
        SetField(level, "glasses", glasses);
        SetArray(level, "buttons", buttons.ToArray());
        SetArray(level, "buttonBodies", bodies.ToArray());
        SetArray(level, "buttonAreas", buttonAreas.ToArray());
        SetArray(level, "buttonLabels", labels.ToArray());
        SetField(level, "answerGlow", answerGlow);
        SetField(level, "puff", puff);
        SetArray(level, "chalkBits", bits.ToArray());
        SetField(level, "bubble", bubble);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, DoodleSprites.V82BBoardY)));
        return SaveLevel(root, "V82StupidGenius2");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V82B = new Dictionary<string, string[]>
    {
        { "stupid_genius_2", new[] { "Fix the equation first.",
            "Drag the 5 onto the 4, then press 4." } },
    };
}
#endif
