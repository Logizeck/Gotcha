#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;
using Gotcha.Levels.V64;

/// <summary>
/// Versione 0.85, livello 91 della roadmap (gruppo B): Mirror Liar. Lo sfondo (v85_b_bg: stanza con la carta da parati
/// e il pavimento di legno), lo specchio verticale in mezzo (vetro, crepe nascoste, cornice dorata), i due personaggi
/// identici (radice ai piedi; dentro Body il disegno, le quattro facce di cui accesa solo quella normale, le crepe
/// nascoste; quello a destra ha la radice con scala x -1, così è il riflesso esatto) con le ombre, le schegge di vetro
/// e la scintilla (nascoste), i due fumetti (SpeechBubble: si adattano al testo, vanno a capo e restano nello schermo),
/// la riga dell'obiettivo e la scritta di commento (aiuti di Halloween) e la festa comune (RewardRig, in alto sopra le
/// teste). Le prese sono calcolate dalla logica (aree generose): niente collider. Ogni TextMesh ha il suo Fit (Label,
/// HW_Goal, HW_Toast e RewardRig); i fumetti si adattano da soli. Disegni nuovi (prefisso v85_b_) in
/// DoodleSpritesV85B. Niente Easter egg e niente Toni. L'elenco dei livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 91: Mirror Liar ----------

    static GameObject BuildV85MirrorLiar()
    {
        var root = new GameObject("Level_V85MirrorLiar");
        V85MirrorLiarLevel level = root.AddComponent<V85MirrorLiarLevel>();
        Transform r = root.transform;
        Art(r, "Room", "v85_b_bg", -20, false);

        // ---- lo specchio in mezzo (coordinate del mondo nei disegni) ----
        Transform mirror = Child(r, "Mirror", new Vector2(MirrorLiarLogic.MirrorX, 0f)).transform;
        Art(mirror, "Glass", "v85_b_mirror_glass", 2, false);
        SpriteRenderer mirrorCrack = Art(mirror, "Crack", "v85_b_mirror_crack", 3, false).GetComponent<SpriteRenderer>();
        mirrorCrack.gameObject.SetActive(false);
        Art(mirror, "Frame", "v85_b_mirror_frame", 4, false);

        // ---- i due personaggi (0 sinistra, 1 destra: lo stesso disegno girato) ----
        var figs = new List<Object>();
        var bodies = new List<Object>();
        var arts = new List<Object>();
        var open = new List<Object>();
        var blink = new List<Object>();
        var smile = new List<Object>();
        var worry = new List<Object>();
        var cracks = new List<Object>();
        var shadows = new List<Object>();
        var bubbles = new List<Object>();
        for (int s = 0; s < 2; s++)
        {
            float sign = s == 0 ? -1f : 1f;
            var feet = new Vector2(MirrorLiarLogic.MirrorX + sign * MirrorLiarLogic.StartAbsX, MirrorLiarLogic.StartY);
            Transform shadow = Art(r, s == 0 ? "ShadowL" : "ShadowR", "ball_shadow", 8, false, feet + new Vector2(0f, 0.02f));
            shadow.localScale = new Vector3(1.1f, 0.25f, 1f);
            shadow.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
            Transform fig = Child(r, s == 0 ? "TwinL" : "TwinR", feet).transform;
            fig.localScale = new Vector3(-sign, 1f, 1f);   // il disegno guarda verso +x (spilla e ciuffo): a sinistra resta così, a destra si gira
            Transform body = Child(fig, "Body", Vector2.zero).transform;
            arts.Add(Art(body, "Guy", "v85_b_guy", 10, false).GetComponent<SpriteRenderer>());
            open.Add(Art(body, "FaceOpen", "v85_b_face_open", 11, false).GetComponent<SpriteRenderer>());
            Transform fb = Art(body, "FaceBlink", "v85_b_face_blink", 11, false);
            Transform fs = Art(body, "FaceSmile", "v85_b_face_smile", 11, false);
            Transform fw = Art(body, "FaceWorry", "v85_b_face_worry", 11, false);
            fb.gameObject.SetActive(false);
            fs.gameObject.SetActive(false);
            fw.gameObject.SetActive(false);
            blink.Add(fb.GetComponent<SpriteRenderer>());
            smile.Add(fs.GetComponent<SpriteRenderer>());
            worry.Add(fw.GetComponent<SpriteRenderer>());
            Transform crack = Art(body, "Crack", "v85_b_guy_crack", 12, false);
            crack.gameObject.SetActive(false);
            cracks.Add(crack.GetComponent<SpriteRenderer>());
            figs.Add(fig);
            bodies.Add(body);
            shadows.Add(shadow);

            // il fumetto (il livello lo porta sopra la testa; a capo stretto: resta nella sua metà)
            SpeechBubble bubble = Bubble(r, new Vector2(s == 0 ? -1.15f : 1.65f, 0.3f));
            bubble.gameObject.name = s == 0 ? "BubbleL" : "BubbleR";
            bubble.transform.localScale = Vector3.one * 0.8f;   // fumetti un po' più piccoli: due affiancati stanno nello schermo
            bubble.maxTextWidth = 1.9f;
            bubbles.Add(bubble);
        }

        // ---- schegge e scintilla ----
        var shards = new List<Object>();
        for (int i = 0; i < 9; i++)
        {
            Transform sh = Art(r, "Shard" + i, "v85_b_shard", 20, false);
            sh.gameObject.SetActive(false);
            shards.Add(sh);
        }
        SpriteRenderer sparkle = Art(r, "Sparkle", "sparkle_star", 21, false).GetComponent<SpriteRenderer>();
        sparkle.gameObject.SetActive(false);

        HW_Goal(r, "FIND THE REAL ONE.");
        TextMesh info = HW_Toast(r, new Vector2(0f, -4.45f), out Transform infoHolder);
        SetArray(level, "figures", figs.ToArray());
        SetArray(level, "bodies", bodies.ToArray());
        SetArray(level, "guyArts", arts.ToArray());
        SetArray(level, "facesOpen", open.ToArray());
        SetArray(level, "facesBlink", blink.ToArray());
        SetArray(level, "facesSmile", smile.ToArray());
        SetArray(level, "facesWorry", worry.ToArray());
        SetArray(level, "guyCracks", cracks.ToArray());
        SetArray(level, "shadows", shadows.ToArray());
        SetField(level, "mirror", mirror);
        SetField(level, "mirrorCrack", mirrorCrack);
        SetArray(level, "shards", shards.ToArray());
        SetField(level, "sparkle", sparkle);
        SetArray(level, "bubbles", bubbles.ToArray());
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, 1.9f)));
        return SaveLevel(root, "V85MirrorLiar");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V85B = new Dictionary<string, string[]>
    {
        { "mirror_liar", new[] { "A reflection can't do its own thing.",
            "Grab both at once and pull them in different directions." } },
    };
}
#endif
