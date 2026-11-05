#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.81, livello 34 della roadmap (gruppo A): Too Easy! Lo sfondo quasi vuoto (v81_a_bg, foglio a quadretti),
/// la frase dell'obiettivo costruita qui (NON la riga standard HW_Goal): fascia scura e quattro parole separate
/// "COMPLETE", "THIS", "LEVEL", "." (TextMesh + BoxCollider2D sul genitore; posizioni, collider e scala della riga le
/// calcola il livello dalle larghezze misurate) più il "___" che prende il posto di LEVEL. In mezzo il pulsantone verde
/// (riusato: v76_a_button) con la scritta WIN e tre facce (furba e linguaccia nuove, stupita v76_a_face_wide), lo sbuffo
/// ("puff"), il timbro "TOO EASY!", il cervellone rosa (v81_a_brain) con le due manine (v81_a_hand, la sinistra
/// specchiata) e le scintille ("sparkle_star"), la scritta di commento (HW_Toast) e la festa comune (RewardRig, sopra il
/// cervellone). Ogni TextMesh ha il suo Fit. Niente Easter egg e niente Toni. L'elenco dei livelli (Define) lo aggiunge
/// il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 34: Too Easy! ----------

    static readonly Color V81AGoalInk = new Color(1f, 0.86f, 0.45f);

    static GameObject BuildV81TooEasy()
    {
        var root = new GameObject("Level_V81TooEasy");
        V81TooEasyLevel level = root.AddComponent<V81TooEasyLevel>();
        Transform r = root.transform;
        Art(r, "Paper", "v81_a_bg", -20, false);

        // ---- la frase dell'obiettivo: è il rompicapo (parole separate, impaginate dal livello) ----
        Transform goal = Child(r, "Goal", new Vector2(0f, 3.2f)).transform;
        Bar(goal, "Back", Vector2.zero, new Vector2(5.6f, 0.66f), new Color(0.1f, 0.06f, 0.16f, 0.82f), 70);
        Transform row = Child(goal, "Sentence", Vector2.zero).transform;
        var words = new List<Object>
        {
            V81A_Word(row, "WordComplete", "COMPLETE", true),
            V81A_Word(row, "WordThis", "THIS", true),
            V81A_Word(row, "WordLevel", "LEVEL", true),
            V81A_Word(row, "WordDot", ".", true),
        };
        TextMesh gap = V81A_Word(row, "WordGap", "___", false);
        gap.transform.parent.gameObject.SetActive(false);

        // ---- il pulsantone WIN (radice che salta, corpo che si dimena) ----
        Transform button = Child(r, "Button", new Vector2(0f, -0.9f)).transform;
        button.localScale = Vector3.one * 1.15f;   // largo 2.65: con il centro entro x ±0.9 i bordi restano entro ±2.25
        Transform body = Child(button, "Body", Vector2.zero).transform;
        Art(body, "Art", "v76_a_button", 5, false);
        TextMesh label = Label(body, new Vector2(0f, DoodleSprites.V76ALabelY), 7, "WIN", 0.075f);
        label.color = Color.white;
        label.fontStyle = FontStyle.Bold;
        Fit(label, 1.8f, 0.34f);
        Transform faceSmug = HW_Obj(body, "FaceSmug", Vector2.zero, "v81_a_face_smug", 6);
        Transform faceTongue = HW_Obj(body, "FaceTongue", Vector2.zero, "v81_a_face_tongue", 6);
        Transform faceShock = HW_Obj(body, "FaceShock", Vector2.zero, "v76_a_face_wide", 6);
        faceTongue.gameObject.SetActive(false);
        faceShock.gameObject.SetActive(false);
        BoxCollider2D buttonCol = HW_Box(button, new Vector2(2.5f, 1.9f), new Vector2(0f, -0.05f));
        SpriteRenderer puff = Art(r, "Puff", "puff", 4, false).GetComponent<SpriteRenderer>();
        puff.gameObject.SetActive(false);

        // ---- il timbro "TOO EASY!" (lo posa il livello dove stava il pulsante) ----
        Transform stamp = Child(r, "Stamp", Vector2.zero).transform;
        TextMesh stampText = Label(stamp, Vector2.zero, 20, "TOO EASY!", 0.1f);
        stampText.color = new Color(0.9f, 0.16f, 0.22f);
        stampText.fontStyle = FontStyle.Bold;
        Fit(stampText, 3.2f, 0.8f);
        stamp.gameObject.SetActive(false);

        // ---- il cervellone che applaude (nascosto fino alla vittoria) ----
        Transform brain = Child(r, "Brain", new Vector2(0f, -1.0f)).transform;
        brain.localScale = Vector3.one * 1.25f;
        Art(brain, "Art", "v81_a_brain", 30, false);
        Transform handRight = HW_Obj(brain, "HandRight", new Vector2(DoodleSprites.V81AShoulderX, DoodleSprites.V81AShoulderY), "v81_a_hand", 31);
        Transform handLeft = HW_Obj(brain, "HandLeft", new Vector2(-DoodleSprites.V81AShoulderX, DoodleSprites.V81AShoulderY), "v81_a_hand", 31);
        handLeft.localScale = new Vector3(-1f, 1f, 1f);
        var sparkles = new List<Object>();
        foreach (Vector2 at in new[] { new Vector2(0f, -0.1f), new Vector2(-0.4f, -0.25f), new Vector2(0.4f, -0.25f), new Vector2(0f, 0.05f) })
        {
            Transform s = HW_Obj(brain, "Sparkle", at, "sparkle_star", 32, 0.9f);
            s.gameObject.SetActive(false);
            sparkles.Add(s);
        }
        brain.gameObject.SetActive(false);

        TextMesh info = HW_Toast(r, new Vector2(0f, -4.5f), out Transform infoHolder);
        SetField(level, "button", button);
        SetField(level, "buttonBody", body);
        SetField(level, "buttonArea", buttonCol);
        SetField(level, "faceSmug", faceSmug);
        SetField(level, "faceTongue", faceTongue);
        SetField(level, "faceShock", faceShock);
        SetField(level, "winLabel", label);
        SetField(level, "puff", puff);
        SetFloat(level, "minX", -0.9f);
        SetFloat(level, "maxX", 0.9f);
        SetFloat(level, "minY", -2.9f);
        SetFloat(level, "maxY", 1.4f);
        SetField(level, "stamp", stamp);
        SetField(level, "stampText", stampText);
        SetField(level, "sentenceRow", row);
        SetArray(level, "words", words.ToArray());
        SetField(level, "wordGap", gap);
        SetFloat(level, "maxRowWidth", 5.0f);
        SetField(level, "brain", brain);
        SetField(level, "handLeft", handLeft);
        SetField(level, "handRight", handRight);
        SetArray(level, "sparkles", sparkles.ToArray());
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, 1.75f)));
        return SaveLevel(root, "V81TooEasy");
    }

    /// <summary>
    /// Una parola della frase dell'obiettivo: genitore (posizione e collider, misurati dal livello) e TextMesh figlio col
    /// font dei titoli, nello stile della riga dell'obiettivo. Fit di sicurezza (la riga intera la rimpicciolisce il
    /// livello entro ±2.5; TextMeshFit la tiene comunque dentro lo schermo).
    /// </summary>
    static TextMesh V81A_Word(Transform row, string name, string value, bool collider)
    {
        Transform holder = Child(row, name, Vector2.zero).transform;
        TextMesh t = Label(holder, Vector2.zero, 71, value, 0.055f);
        t.color = V81AGoalInk;
        Fit(t, 5.0f, 0.6f);
        if (collider) HW_Box(holder, new Vector2(value.Length * 0.3f + 0.4f, 0.95f));
        return t;
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V81A = new Dictionary<string, string[]>
    {
        { "too_easy", new[] { "Which word in the objective could disappear?",
            "Tap the word LEVEL in the objective." } },
    };
}
#endif
