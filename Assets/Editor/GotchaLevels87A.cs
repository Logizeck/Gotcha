#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.87, livello 119 della roadmap (gruppo A): The Fireless Dragon. Lo sfondo (v87_a_bg: prato al tramonto),
/// il falò (catasta "logs", tre fiamme "flame" che tremolano, bagliore), la fiammella trascinabile sospesa sopra il falò
/// (radice al centro; il disegno dentro un contenitore che tremola; area generosa che copre anche le fiamme del falò), il
/// draghetto (radice ai piedi; perno Body per respiro e schiacciamenti; corpo v87_a_dragon_body e cinque teste al collo;
/// segnaposto della bocca; area di tocco generosa) con la sua ombra, le bolle di sapone (insieme fisso di sprite
/// "soap_bubble"), gli sbuffi di fumo grigio della tosse, lo sbuffo della fiammella spenta, il bagliore della bocca, la
/// grande fiammata (perno alla bocca, verso destra, sotto la barra in alto), il fumetto del drago (SpeechBubble: va a capo
/// e resta nello schermo), la riga dell'obiettivo e la scritta di commento (HW_Goal, HW_Toast, entrambe con Fit) e la
/// festa comune (RewardRig). Disegni nuovi (prefisso v87_a_) in DoodleSpritesV87A. Niente Easter egg e niente Toni.
/// L'elenco dei livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 119: The Fireless Dragon ----------

    static GameObject BuildV87FirelessDragon()
    {
        var root = new GameObject("Level_V87FirelessDragon");
        V87FirelessDragonLevel level = root.AddComponent<V87FirelessDragonLevel>();
        Transform r = root.transform;
        Art(r, "Meadow", "v87_a_bg", -20, false);

        // ---- il falò: bagliore, catasta e tre fiamme (ognuna dentro un perno alla base, così tremola dal basso) ----
        Vector2 fire = new Vector2(DoodleSprites.V87AFireX, DoodleSprites.V87AFireY);
        SpriteRenderer fireGlow = Art(r, "FireGlow", "glow", 1, false, fire + new Vector2(0f, 0.55f)).GetComponent<SpriteRenderer>();
        fireGlow.transform.localScale = Vector3.one * 2.6f;
        fireGlow.color = new Color(1f, 0.6f, 0.2f, 0.5f);
        Transform logs = Child(r, "Campfire", fire).transform;
        logs.localScale = Vector3.one * 0.75f;
        Art(logs, "Logs", "logs", 2, false);
        var fireFlames = new List<Object>();
        float[][] flames = { new[] { -0.3f, 0.12f, 2.6f, 3f }, new[] { 0.3f, 0.12f, 2.6f, 3f }, new[] { 0f, 0.15f, 3.6f, 4f } };
        for (int i = 0; i < flames.Length; i++)
        {
            Transform f = Child(r, "FireFlame" + i, fire + new Vector2(flames[i][0], flames[i][1])).transform;
            f.localScale = Vector3.one * flames[i][2];
            Art(f, "Art", "flame", (int)flames[i][3], false);
            fireFlames.Add(f);
        }

        // ---- la fiammella trascinabile (radice al centro, sopra il falò) ----
        Transform flame = Child(r, "Flame", new Vector2(DoodleSprites.V87AFlameX, DoodleSprites.V87AFlameY)).transform;
        SpriteRenderer flameGlow = Art(flame, "Glow", "glow", 18, false).GetComponent<SpriteRenderer>();
        flameGlow.transform.localScale = Vector3.one * 1.5f;
        flameGlow.color = new Color(1f, 0.8f, 0.3f, 0.6f);
        Transform flameArt = Child(flame, "Art", new Vector2(0f, -0.3f)).transform;   // base del disegno: la fiammella è alta 0.26 x 2.4
        flameArt.localScale = Vector3.one * 2.4f;
        SpriteRenderer flameSprite = Art(flameArt, "Flame", "flame", 19, false).GetComponent<SpriteRenderer>();
        BoxCollider2D flameArea = HW_Box(flame, new Vector2(1.2f, 1.6f), new Vector2(0f, -0.35f));   // copre anche le fiamme del falò

        // ---- il draghetto: radice ai piedi, perno Body, corpo e cinque teste al collo ----
        Vector2 feet = new Vector2(DoodleSprites.V87ADragonX, DoodleSprites.V87ADragonY);
        Transform shadow = Art(r, "DragonShadow", "ball_shadow", 5, false, feet + new Vector2(0f, 0.02f));
        shadow.localScale = new Vector3(1.8f, 0.35f, 1f);
        shadow.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.45f);
        Transform dragon = Child(r, "Dragon", feet).transform;
        Transform body = Child(dragon, "Body", Vector2.zero).transform;
        Art(body, "Art", "v87_a_dragon_body", 6, false);
        Transform head = Child(body, "Head", new Vector2(DoodleSprites.V87ANeckX, DoodleSprites.V87ANeckY)).transform;
        Transform headIdle = Art(head, "Idle", "v87_a_head_idle", 7, false);
        Transform headPuff = Art(head, "Puff", "v87_a_head_puff", 7, false);
        Transform headRoar = Art(head, "Roar", "v87_a_head_roar", 7, false);
        Transform headCough = Art(head, "Cough", "v87_a_head_cough", 7, false);
        Transform headHappy = Art(head, "Happy", "v87_a_head_happy", 7, false);
        foreach (Transform t in new[] { headPuff, headRoar, headCough, headHappy }) t.gameObject.SetActive(false);
        Transform mouth = Child(head, "Mouth", new Vector2(DoodleSprites.V87AMouthX, DoodleSprites.V87AMouthY)).transform;
        BoxCollider2D dragonArea = HW_Box(dragon, new Vector2(2.9f, 3.5f), new Vector2(-0.15f, 1.65f));   // corpo e testa, area generosa
        Vector2 mouthW = feet + new Vector2(DoodleSprites.V87ANeckX + DoodleSprites.V87AMouthX, DoodleSprites.V87ANeckY + DoodleSprites.V87AMouthY);

        // ---- effetti: bolle, fumo della tosse, sbuffo della fiammella spenta, bagliore della bocca, fiammata ----
        Transform bubbles = Child(r, "SoapBubbles", Vector2.zero).transform;
        var soap = new List<Object>();
        for (int i = 0; i < 10; i++)
        {
            SpriteRenderer b = Art(bubbles, "Bubble" + i, "soap_bubble", 12, false, mouthW).GetComponent<SpriteRenderer>();
            b.gameObject.SetActive(false);
            soap.Add(b);
        }
        var puffs = new List<Object>();
        for (int i = 0; i < 3; i++)
        {
            SpriteRenderer p = Art(r, "CoughPuff" + i, "puff", 14, false, mouthW).GetComponent<SpriteRenderer>();
            p.color = new Color(0.55f, 0.55f, 0.6f, 0.9f);
            p.gameObject.SetActive(false);
            puffs.Add(p);
        }
        SpriteRenderer fizzle = Art(r, "FizzlePuff", "puff", 20, false).GetComponent<SpriteRenderer>();
        fizzle.color = new Color(0.5f, 0.5f, 0.55f, 0.9f);
        fizzle.gameObject.SetActive(false);
        SpriteRenderer mouthGlow = Art(r, "MouthGlow", "glow", 13, false, mouthW).GetComponent<SpriteRenderer>();
        mouthGlow.color = new Color(1f, 0.6f, 0.2f, 0f);
        mouthGlow.gameObject.SetActive(false);
        Transform burst = Child(r, "Burst", mouthW).transform;
        burst.localRotation = Quaternion.Euler(0f, 0f, 8f);   // un filo verso l'alto: la punta resta sotto y 0.5, lontana dalla barra in alto
        SpriteRenderer burstSprite = Art(burst, "Art", "v87_a_burst", 15, false).GetComponent<SpriteRenderer>();
        burst.gameObject.SetActive(false);

        // ---- il fumetto del drago (sopra la testa, a destra; va a capo e rientra nello schermo da solo) ----
        Transform bubbleHolder = Child(r, "DragonBubbleHolder", mouthW + new Vector2(0.1f, 0.95f)).transform;
        SpeechBubble bubble = SmallBubble(bubbleHolder, Vector2.zero);
        bubble.maxTextWidth = 3.6f;   // scala 0.5: 1.8 nel mondo, al massimo due righe

        HW_Goal(r, "LIGHT THE DRAGON'S FIRE.");
        TextMesh info = HW_Toast(r, new Vector2(0f, -4.45f), out Transform infoHolder);
        SetField(level, "dragonBody", body);
        SetField(level, "headIdle", headIdle);
        SetField(level, "headPuff", headPuff);
        SetField(level, "headRoar", headRoar);
        SetField(level, "headCough", headCough);
        SetField(level, "headHappy", headHappy);
        SetField(level, "mouth", mouth);
        SetField(level, "dragonArea", dragonArea);
        SetField(level, "dragonBubble", bubble);
        SetField(level, "flame", flame);
        SetField(level, "flameArt", flameArt);
        SetField(level, "flameArea", flameArea);
        SetField(level, "flameSprite", flameSprite);
        SetField(level, "flameGlow", flameGlow);
        SetArray(level, "fireFlames", fireFlames.ToArray());
        SetField(level, "fireGlow", fireGlow);
        SetArray(level, "soapBubbles", soap.ToArray());
        SetArray(level, "smokePuffs", puffs.ToArray());
        SetField(level, "fizzlePuff", fizzle);
        SetField(level, "mouthGlow", mouthGlow);
        SetField(level, "burst", burst);
        SetField(level, "burstSprite", burstSprite);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, 1.6f)));
        return SaveLevel(root, "V87FirelessDragon");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V87A = new Dictionary<string, string[]>
    {
        { "fireless_dragon", new[] { "The dragon needs a spark.",
            "Drag a flame from the campfire into the dragon's mouth." } },
    };
}
#endif
