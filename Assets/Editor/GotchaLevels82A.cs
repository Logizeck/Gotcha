#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.82, livello 42 della roadmap (gruppo A): The Polite Bomb. Lo sfondo dell'officina (v82_a_bg: parete,
/// pannello forato, banco col piano a y −1.15, pavimento), la bomba (radice nel centro del corpo con un collider a cerchio
/// per i colpetti, figlio Look per gli effetti) con corpo e papillon, cinque facce (una sola accesa), il display
/// (cornice, luce verde che si spegne, testo verde col suo Fit), la scintilla sulla punta della miccia, i tre fili
/// (origine nel punto d'uscita dal display, un collider generoso ciascuno sulla parte che penzola) e il bordo del vano
/// batteria; la batteria è un oggetto a sé (collider generoso, sporge dal vano a destra) con il punto dove si posa sul
/// banco una volta staccata. Sotto, sul fronte del banco, il pulsante STOP (basamento + calotta che si abbassa, scritta
/// STOP come testo col suo Fit, collider generoso). Lo sbuffo ("puff"), gli "zzz" e la coppa ("s41e_trophy") nascosti;
/// il fumetto (Bubble: si adatta al testo e resta nello schermo), la riga dell'obiettivo e la scritta di commento (aiuti
/// di Halloween, ognuna col suo Fit) e la festa comune (RewardRig, in alto sopra la miccia). Disegni nuovi (prefisso
/// v82_a_) in DoodleSpritesV82A. Niente Easter egg e niente Toni. L'elenco dei livelli (Define) lo aggiunge il capo
/// progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 42: The Polite Bomb ----------

    static readonly Color V82ADisplayInk = new Color(0.62f, 1f, 0.7f);

    static GameObject BuildV82PoliteBomb()
    {
        var root = new GameObject("Level_V82PoliteBomb");
        V82PoliteBombLevel level = root.AddComponent<V82PoliteBombLevel>();
        Transform r = root.transform;
        Art(r, "Workshop", "v82_a_bg", -20, false);
        Vector2 bombAt = new Vector2(0f, 0.15f);   // il fondo del corpo (raggio 1.25) appoggia sul banco a y −1.1

        // ---- la bomba (radice = centro del corpo; Look per pulsazione, tremolio e afflosciata) ----
        Transform bomb = Child(r, "Bomb", bombAt).transform;
        CircleCollider2D body = bomb.gameObject.AddComponent<CircleCollider2D>();
        body.radius = DoodleSprites.V82ABombR;
        Transform look = Child(bomb, "Look", Vector2.zero).transform;
        Art(look, "Body", "v82_a_bomb", 10, false);
        string[] moods = { "polite", "eager", "worried", "embarrassed", "sleepy" };
        var faces = new List<Object>();
        for (int i = 0; i < moods.Length; i++)
        {
            Transform f = Art(look, "Face_" + moods[i], "v82_a_face_" + moods[i], 11, false);
            f.gameObject.SetActive(i == 0);
            faces.Add(f);
        }
        Transform disp = Child(look, "Display", new Vector2(DoodleSprites.V82ADisplayX, DoodleSprites.V82ADisplayY)).transform;
        Art(disp, "Frame", "v82_a_display", 12, false);
        SpriteRenderer glow = Art(disp, "Glow", "v82_a_display_glow", 13, false).GetComponent<SpriteRenderer>();
        TextMesh digits = Label(disp, new Vector2(0f, -0.01f), 14, "00:10", 0.075f);
        digits.color = V82ADisplayInk;
        Fit(digits, 1.18f, 0.36f);   // anche "ERR 404:\nCLUE NOT FOUND" resta dentro lo schermo del display
        Transform spark = HW_Obj(look, "Spark", new Vector2(DoodleSprites.V82AFuseX, DoodleSprites.V82AFuseY), "v82_a_spark", 16);

        // ---- i fili (origine nel punto d'uscita; collider sulla parte che penzola, coordinate del filo) ----
        string[] wireNames = { "red", "blue", "yellow" };
        Vector2[] wireBoxAt = { new Vector2(-1.02f, -0.45f), new Vector2(-0.78f, -0.72f), new Vector2(-0.42f, -0.85f) };
        Vector2[] wireBoxSize = { new Vector2(0.62f, 1.05f), new Vector2(0.5f, 0.95f), new Vector2(0.5f, 0.9f) };
        var wires = new List<Object>();
        var wireAreas = new List<Object>();
        for (int i = 0; i < 3; i++)
        {
            Transform w = HW_Obj(look, "Wire_" + wireNames[i], new Vector2(DoodleSprites.V82AWireX, DoodleSprites.V82AWireY), "v82_a_wire_" + wireNames[i], 15 - i);
            wires.Add(w);
            wireAreas.Add(HW_Box(w, wireBoxSize[i], wireBoxAt[i]));
        }

        // ---- il vano e la batteria (sporge a destra; collider generoso, più largo verso l'esterno) ----
        Art(look, "SlotLip", "v82_a_slot", 18, false);
        Vector2 batteryAt = bombAt + new Vector2(DoodleSprites.V82ABatteryX, DoodleSprites.V82ABatteryY);
        Transform battery = HW_Obj(r, "Battery", batteryAt, "v82_a_battery", 17);
        BoxCollider2D batteryCol = HW_Box(battery, new Vector2(1.3f, 0.95f), new Vector2(0.2f, 0f));
        Transform batteryRest = Child(r, "BatteryRest", new Vector2(1.8f, -0.93f)).transform;

        // ---- il pulsante STOP sul fronte del banco ----
        Transform stop = Child(r, "Stop", new Vector2(0f, -2.55f)).transform;
        Art(stop, "Base", "v82_a_stop_base", 8, false);
        Transform cap = Child(stop, "Cap", Vector2.zero).transform;
        Art(cap, "Art", "v82_a_stop_cap", 9, false);
        TextMesh stopLabel = Label(cap, new Vector2(0f, DoodleSprites.V82AStopLabelY), 10, "STOP", 0.07f);
        stopLabel.color = Color.white;
        stopLabel.fontStyle = FontStyle.Bold;
        Fit(stopLabel, 0.95f, 0.3f);
        BoxCollider2D stopCol = HW_Box(stop, new Vector2(2.1f, 1.45f), new Vector2(0f, -0.05f));

        // ---- effetti nascosti: sbuffo, "zzz", coppa ----
        SpriteRenderer puff = Art(r, "Puff", "puff", 30, false).GetComponent<SpriteRenderer>();
        puff.transform.localPosition = new Vector3(0.35f, bombAt.y + 1.1f, 0f);
        puff.transform.localScale = Vector3.one * 2.4f;
        puff.gameObject.SetActive(false);
        Transform zzz = HW_Obj(r, "Zzz", bombAt + new Vector2(1.1f, 1.05f), "zzz", 30, 1.7f);
        zzz.gameObject.SetActive(false);
        Transform trophy = HW_Obj(r, "Trophy", new Vector2(-1.7f, -1.15f), "s41e_trophy", 25, 1.05f);
        trophy.gameObject.SetActive(false);

        SpeechBubble bubble = Bubble(r, new Vector2(-1.05f, bombAt.y + 1.05f));
        bubble.maxTextWidth = 3.6f;   // righe lunghe: al massimo due righe, il fumetto resta sotto la riga dell'obiettivo
        HW_Goal(r, "STOP THE COUNTDOWN.");
        TextMesh info = HW_Toast(r, new Vector2(0f, -4.25f), out Transform infoHolder);
        SetField(level, "look", look);
        SetField(level, "bodyArea", body);
        SetArray(level, "faces", faces.ToArray());
        SetField(level, "display", digits);
        SetField(level, "displayGlow", glow);
        SetField(level, "spark", spark);
        SetArray(level, "wires", wires.ToArray());
        SetArray(level, "wireAreas", wireAreas.ToArray());
        SetField(level, "battery", battery);
        SetField(level, "batteryArea", batteryCol);
        SetField(level, "batteryRest", batteryRest);
        SetFloat(level, "batteryMaxX", 2.2f);
        SetField(level, "stopCap", cap);
        SetField(level, "stopArea", stopCol);
        SetField(level, "puff", puff);
        SetField(level, "zzz", zzz);
        SetField(level, "trophy", trophy);
        SetField(level, "bubble", bubble);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, 2.35f)));
        return SaveLevel(root, "V82PoliteBomb");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V82A = new Dictionary<string, string[]>
    {
        { "polite_bomb", new[] { "Look at the back of the bomb.",
            "Drag the battery out." } },
    };
}
#endif
