#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;
using Gotcha.Levels.V64;

/// <summary>
/// Versione 0.84, livello 72 della roadmap (gruppo B): The Short Ladder. Lo sfondo (v84_b_bg: facciata di mattoni,
/// pluviale, zoccolo, marciapiede), la finestra in alto a destra (la stanza dietro, il busto dell'omino che saluta col
/// braccio a parte, nascosto; il vetro; la cornice col davanzale e la fioriera), la scala (radice ai piedi sul
/// pavimento; dentro Body i piedini, 13 tratti uguali da 0.5 di cui accesi i primi 4, i tappi in cima; collider generoso
/// che il livello adatta alla lunghezza), la scintilla (nascosta), l'omino (radice ai piedi, Body con la posa in piedi
/// e quella di spalle che sale, nascosta; collider generoso) con la sua ombra, il fumetto dell'omino (Bubble/
/// SpeechBubble, si adatta al testo e resta nello schermo), la riga dell'obiettivo e la scritta di commento (aiuti di
/// Halloween) e la festa comune (RewardRig, a metà schermo sotto la finestra). Ogni TextMesh ha il suo Fit. Disegni
/// nuovi (prefisso v84_b_) in DoodleSpritesV84B. Niente Easter egg e niente Toni. L'elenco dei livelli (Define) lo
/// aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 72: The Short Ladder ----------

    static GameObject BuildV84ShortLadder()
    {
        var root = new GameObject("Level_V84ShortLadder");
        V84ShortLadderLevel level = root.AddComponent<V84ShortLadderLevel>();
        Transform r = root.transform;
        Art(r, "Facade", "v84_b_bg", -20, false);
        const float floorY = ShortLadderLogic.FloorY, segH = ShortLadderLogic.SegmentHeight;

        // ---- la finestra (in alto, nella colonna x 0.9) ----
        var winPos = new Vector2(DoodleSprites.V84BWindowX, DoodleSprites.V84BWindowY);
        float sillY = DoodleSprites.V84BWindowY - DoodleSprites.V84BWindowHy;
        Transform window = Child(r, "Window", winPos).transform;
        Art(window, "Room", "v84_b_window_back", 2, false);
        // il busto che saluta (radice sul davanzale; il livello lo sposta sul lato libero e lo specchia)
        Transform wave = Child(r, "WaveMan", new Vector2(winPos.x, sillY)).transform;
        Art(wave, "Bust", "v84_b_man_bust", 3, false);
        Transform arm = Child(wave, "Arm", new Vector2(0.22f, 0.32f)).transform;
        Art(arm, "Art", "v84_b_man_arm", 4, false);
        wave.gameObject.SetActive(false);
        SpriteRenderer glass = Art(window, "Glass", "v84_b_window_glass", 5, false).GetComponent<SpriteRenderer>();
        Art(window, "Frame", "v84_b_window_frame", 6, false);

        // ---- la scala (radice ai piedi sul pavimento; tratti tutti uguali: pioli sempre equidistanti) ----
        Transform ladder = Child(r, "Ladder", new Vector2(ShortLadderLogic.LadderStartX, floorY)).transform;
        Transform lBody = Child(ladder, "Body", Vector2.zero).transform;
        var segs = new List<Object>();
        for (int i = 0; i < ShortLadderLogic.MaxSegments; i++)
        {
            Transform s = Child(lBody, "Seg" + i, new Vector2(0f, i * segH)).transform;
            Art(s, "Art", "v84_b_ladder_seg", 8, false);
            s.gameObject.SetActive(i < ShortLadderLogic.MinSegments);
            segs.Add(s);
        }
        Art(lBody, "Feet", "v84_b_ladder_foot", 8, false);
        Transform top = Child(lBody, "Top", new Vector2(0f, ShortLadderLogic.MinSegments * segH)).transform;
        Art(top, "Art", "v84_b_ladder_top", 8, false);
        float len0 = ShortLadderLogic.MinSegments * segH;
        BoxCollider2D ladderCol = HW_Box(ladder, new Vector2(1.5f, len0 + 0.6f), new Vector2(0f, len0 * 0.5f));   // scala larga 0.8: area generosa
        SpriteRenderer sparkle = Art(r, "Sparkle", "sparkle_star", 9, false, new Vector2(ShortLadderLogic.LadderStartX, floorY + len0)).GetComponent<SpriteRenderer>();
        sparkle.gameObject.SetActive(false);

        // ---- l'omino (radice ai piedi) con l'ombra ----
        Transform shadow = Art(r, "ManShadow", "ball_shadow", 9, false, new Vector2(ShortLadderLogic.ManStartX, floorY + 0.02f));
        shadow.localScale = new Vector3(0.9f, 0.22f, 1f);
        shadow.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        Transform man = Child(r, "Man", new Vector2(ShortLadderLogic.ManStartX, floorY)).transform;
        Transform mBody = Child(man, "Body", Vector2.zero).transform;
        Transform stand = Art(mBody, "Stand", "v84_b_man", 11, false);
        Transform climb = Art(mBody, "Climb", "v84_b_man_climb", 11, false);
        climb.gameObject.SetActive(false);
        BoxCollider2D manCol = HW_Box(man, new Vector2(1.1f, 1.7f), new Vector2(0f, 0.75f));   // omino largo 0.8 e alto 1.4: area generosa

        // ---- il fumetto dell'omino (il livello lo porta accanto alla testa) ----
        SpeechBubble bubble = Bubble(r, new Vector2(ShortLadderLogic.ManStartX + 1.05f, floorY + 2.1f));

        HW_Goal(r, "REACH THE WINDOW.");
        TextMesh info = HW_Toast(r, new Vector2(0f, -4.45f), out Transform infoHolder);
        SetField(level, "ladder", ladder);
        SetField(level, "ladderBody", lBody);
        SetArray(level, "segments", segs.ToArray());
        SetField(level, "ladderTop", top);
        SetField(level, "ladderArea", ladderCol);
        SetField(level, "sparkle", sparkle);
        SetField(level, "man", man);
        SetField(level, "manBody", mBody);
        SetField(level, "manStand", stand);
        SetField(level, "manClimb", climb);
        SetField(level, "manArea", manCol);
        SetField(level, "manShadow", shadow);
        SetField(level, "windowGlass", glass);
        SetField(level, "waveMan", wave);
        SetField(level, "waveArm", arm);
        SetField(level, "manBubble", bubble);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, -0.9f)));
        return SaveLevel(root, "V84ShortLadder");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V84B = new Dictionary<string, string[]>
    {
        { "short_ladder", new[] { "Ladders can stretch... in this game.",
            "Pinch the ladder longer, then put it under the window." } },
    };
}
#endif
