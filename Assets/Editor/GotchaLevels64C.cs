#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;
using Gotcha.Levels.V64;

/// <summary>
/// Versione 0.64 (parte C): livelli 257 "The perfect liar" e 258 "The rigged three-card game".
/// Riusano la stanza degli interrogatori e la macchina della verità di LieDetector, le carte di CardHouse, il mago e il
/// cilindro del livello del coniglio, la guardia, le manette, le monete e la strada di notte; disegni nuovi (prefisso
/// v64_) in DoodleSpritesV64C. Aiuti comuni: quelli di Halloween (HW_*) e V62_Wire (coriandoli e scritta di commento).
/// Niente Easter egg (il Jolly nel cilindro del 258 è solo una gag) e niente Toni.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 257 The perfect liar ----------

    static GameObject BuildV64PerfectLiar()
    {
        var root = new GameObject("Level_V64PerfectLiar");
        V64PerfectLiarLevel level = root.AddComponent<V64PerfectLiarLevel>();
        Art(root.transform, "Room", "s41f_interro_bg", -20, false);

        // tre sospettati dietro il tavolo (statement 1, 2, 3 da sinistra)
        string[] looks = { "s41c_worker", "s41f_suspect", "mu_burglar" };
        Vector2[] at = { new Vector2(-1.65f, 0.62f), new Vector2(0f, 0.08f), new Vector2(1.65f, 0.2f) };
        float[] scale = { 0.66f, 0.8f, 0.8f };
        var suspects = new List<Object>();
        var areas = new List<Object>();
        var bubbles = new List<Object>();
        var pointers = new List<Object>();
        var lamps = new List<Object>();
        var glows = new List<Object>();
        Vector2 cursorAt = new Vector2(0f, -3.9f);
        for (int i = 0; i < 3; i++)
        {
            Transform s = HW_Obj(root.transform, "Suspect" + (i + 1), at[i], looks[i], 2, scale[i]);
            var breathe = s.Find("Art").gameObject.AddComponent<Pulse>();
            breathe.amount = 0.012f;
            breathe.speed = 2f + i * 0.4f;
            suspects.Add(s);
            areas.Add(HW_Area(root.transform, "SuspectArea" + (i + 1), new Vector2(1.55f, 1.5f), new Vector2(at[i].x, 0.95f)));
            bubbles.Add(SmallBubble(root.transform, new Vector2(at[i].x * 0.97f, 2.05f)));
            // la manina che indica il giocatore (verso il basso, al centro)
            Vector2 hand = new Vector2(at[i].x + (i == 1 ? 0.55f : 0.3f), 0.75f);
            Transform p = HW_Obj(root.transform, "Pointer" + (i + 1), hand, "v64_hand_cursor", 13, 0.7f);
            Vector2 d = cursorAt - hand;
            p.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg - 90f);
            pointers.Add(p);
        }

        // il tavolo con le spie e il display, i fili degli elettrodi
        Bar(root.transform, "TableTop", new Vector2(0f, 0.02f), new Vector2(5.8f, 0.26f), new Color(0.55f, 0.38f, 0.25f), 5);
        Bar(root.transform, "TableFront", new Vector2(0f, -0.33f), new Vector2(5.8f, 0.48f), new Color(0.36f, 0.25f, 0.17f), 5);
        for (int i = 0; i < 3; i++)
        {
            SpriteRenderer g = Art(root.transform, "LampGlow" + (i + 1), "glow", 6, false, new Vector2(at[i].x, 0.08f)).GetComponent<SpriteRenderer>();
            g.transform.localScale = Vector3.one * 0.7f;
            g.color = new Color(1f, 1f, 1f, 0f);
            glows.Add(g);
            SpriteRenderer l = Art(root.transform, "Lamp" + (i + 1), "bs_button", 7, false, new Vector2(at[i].x, 0.08f)).GetComponent<SpriteRenderer>();
            l.transform.localScale = Vector3.one * 0.2f;
            lamps.Add(l);
            Vector2 a = new Vector2(at[i].x * 0.25f, -0.95f), b = new Vector2(at[i].x, 0.32f);
            SpriteRenderer wire = Bar(root.transform, "Wire" + (i + 1), (a + b) * 0.5f, new Vector2(Vector2.Distance(a, b), 0.035f), new Color(0.15f, 0.12f, 0.18f), 4);
            wire.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
            Bar(root.transform, "Electrode" + (i + 1), b, new Vector2(0.16f, 0.1f), new Color(0.85f, 0.75f, 0.3f), 6);
        }
        Transform ro = Child(root.transform, "Readout", new Vector2(0f, -0.35f)).transform;
        Bar(ro, "Back", Vector2.zero, new Vector2(3.4f, 0.36f), new Color(0.05f, 0.08f, 0.1f, 0.92f), 11);
        TextMesh readout = Fit(Label(ro, Vector2.zero, 12, "", 0.04f), 3.2f, 0.3f);

        // la macchina della verità, montata capovolta (la ruota il livello); le maniglie alle due estremità
        Transform det = Child(root.transform, "Detector", new Vector2(0f, -1.85f)).transform;
        det.localScale = Vector3.one * 1.5f;
        Art(det, "Art", "s41f_polygraph", 8, false);
        Bar(det, "HandleL", new Vector2(-0.9f, 0.05f), new Vector2(0.14f, 0.55f), new Color(0.3f, 0.3f, 0.36f), 7);
        Bar(det, "HandleR", new Vector2(0.9f, 0.05f), new Vector2(0.14f, 0.55f), new Color(0.3f, 0.3f, 0.36f), 7);
        Transform needle = Child(det, "Needle", new Vector2(0.0f, -0.05f)).transform;
        Art(needle, "Art", "s41f_poly_needle", 9, false);
        Transform weight = HW_Obj(det, "Weight", new Vector2(0.5f, 0.38f), "s41i_gear", 9, 0.2f);
        Transform weightDown = Child(det, "WeightDown", new Vector2(0.5f, -0.02f)).transform;
        TextMesh logo = Fit(Label(det, new Vector2(0.3f, -0.37f), 10, "LIE-O-MATIC", 0.03f), 0.56f, 0.14f);
        logo.color = new Color(1f, 0.95f, 0.8f);
        Transform sticker = Child(det, "UpSticker", new Vector2(-0.3f, 0.42f)).transform;
        Bar(sticker, "Back", Vector2.zero, new Vector2(0.7f, 0.18f), new Color(1f, 0.85f, 0.25f), 10);
        Fit(Label(sticker, Vector2.zero, 11, "THIS WAY UP", 0.026f), 0.66f, 0.15f);
        BoxCollider2D gripL = HW_Area(root.transform, "GripL", new Vector2(1.35f, 2.0f), new Vector2(-1.55f, -1.85f));
        BoxCollider2D gripR = HW_Area(root.transform, "GripR", new Vector2(1.35f, 2.0f), new Vector2(1.55f, -1.85f));
        SpriteRenderer glowL = Art(root.transform, "GripGlowL", "glow", 7, false, new Vector2(-1.35f, -1.85f)).GetComponent<SpriteRenderer>();
        SpriteRenderer glowR = Art(root.transform, "GripGlowR", "glow", 7, false, new Vector2(1.35f, -1.85f)).GetComponent<SpriteRenderer>();
        foreach (SpriteRenderer g in new[] { glowL, glowR }) { g.transform.localScale = Vector3.one * 1.1f; g.color = new Color(1f, 0.9f, 0.5f, 0f); }

        // il pulsante TEST e il megafono (alternativa al microfono, nascosto)
        Transform test = HW_Obj(root.transform, "Test", new Vector2(0f, -3.65f), "bs_button", 10, 0.62f);
        test.Find("Art").GetComponent<SpriteRenderer>().color = new Color(0.95f, 0.25f, 0.22f);
        TextMesh testLabel = Fit(Label(test, Vector2.zero, 11, "TEST", 0.09f), 1.0f, 0.4f);
        testLabel.color = Color.white;
        CircleCollider2D testCol = test.gameObject.AddComponent<CircleCollider2D>();
        testCol.radius = 0.85f;
        Transform mic = HW_Obj(root.transform, "MicButton", new Vector2(1.8f, -3.6f), "s41l_megaphone", 12, 0.55f);
        TextMesh micLabel = Fit(Label(mic, new Vector2(0f, -0.95f), 13, "HOLD TO SPEAK", 0.07f), 2.4f, 0.4f);
        micLabel.color = Color.white;
        BoxCollider2D micCol = HW_Box(mic, new Vector2(2.4f, 2.2f), new Vector2(0f, -0.2f));

        // la quarta spia gigante (nascosta)
        Transform big = Child(root.transform, "BigLamp", new Vector2(0f, 1.95f)).transform;
        SpriteRenderer bigGlow = Art(big, "Glow", "glow", 60, false).GetComponent<SpriteRenderer>();
        bigGlow.transform.localScale = new Vector3(4.5f, 2.2f, 1f);
        bigGlow.color = new Color(1f, 0.25f, 0.2f, 0.6f);
        Bar(big, "Back", Vector2.zero, new Vector2(4.7f, 0.85f), new Color(0.35f, 0.04f, 0.06f, 0.95f), 61);
        TextMesh bigText = Fit(Label(big, Vector2.zero, 62, "PLAYER — LIAR", 0.1f), 4.4f, 0.65f);
        bigText.color = new Color(1f, 0.4f, 0.35f);

        // il finale: il poliziotto, il cursore-manina e le manette (nascosti / fuori scena)
        Transform officer = HW_Obj(root.transform, "Officer", new Vector2(4.1f, -3.4f), "mu_guard", 20, 0.8f);
        officer.localScale = new Vector3(-0.8f, 0.8f, 1f);
        Transform cursor = HW_Obj(root.transform, "Cursor", cursorAt, "v64_hand_cursor", 22, 0.55f);
        Transform cuffs = HW_Obj(root.transform, "Cuffs", cursorAt, "v40b_handcuffs", 23, 0.3f);

        HW_Goal(root.transform, "FIND THE LIAR!");
        SetArray(level, "suspects", suspects.ToArray());
        SetArray(level, "suspectAreas", areas.ToArray());
        SetArray(level, "suspectBubbles", bubbles.ToArray());
        SetArray(level, "pointers", pointers.ToArray());
        SetArray(level, "lamps", lamps.ToArray());
        SetArray(level, "lampGlows", glows.ToArray());
        SetField(level, "detector", det);
        SetField(level, "needle", needle);
        SetField(level, "weight", weight);
        SetField(level, "weightDown", weightDown);
        SetField(level, "upSticker", sticker);
        SetField(level, "leftGrip", gripL);
        SetField(level, "rightGrip", gripR);
        SetField(level, "leftGlow", glowL);
        SetField(level, "rightGlow", glowR);
        SetField(level, "readout", readout);
        SetField(level, "testButton", test);
        SetField(level, "testArea", testCol);
        SetField(level, "bigLamp", big);
        SetField(level, "micButton", mic);
        SetField(level, "micArea", micCol);
        SetField(level, "micLabel", micLabel);
        SetField(level, "officer", officer);
        SetField(level, "cursorHand", cursor);
        SetField(level, "cuffs", cuffs);
        V62_Wire(level, root.transform, new Vector2(0f, 2.65f));
        return SaveLevel(root, "V64PerfectLiar");
    }

    // ---------- 258 The rigged three-card game ----------

    static GameObject BuildV64RiggedCards()
    {
        var root = new GameObject("Level_V64RiggedCards");
        V64RiggedCardsLevel level = root.AddComponent<V64RiggedCardsLevel>();
        Art(root.transform, "Street", "s41h_street_bg", -20, false);

        // il mazziere (si china dietro al tavolo) col suo cilindro e il mazzo in mano (nascosto)
        Transform dealer = Child(root.transform, "Dealer", new Vector2(0f, 0.55f)).transform;
        Art(dealer, "Art", "v64_dealer", 2, false);
        Transform hat = Art(dealer, "Hat", "v64_dealer_hat", 3, false);
        Transform deck = HW_Obj(dealer, "MiniDeck", new Vector2(0.5f, 0.25f), "v64_card_back", 4, 0.55f);
        BoxCollider2D dealerCol = HW_Area(root.transform, "DealerArea", new Vector2(1.5f, 2.0f), new Vector2(0f, 1.0f));

        // il tavolo: panno verde (lati aperti a destra e a sinistra), bordo rialzato sul retro, gambe
        Bar(root.transform, "LegL", new Vector2(-2.3f, -3.9f), new Vector2(0.25f, 1.4f), new Color(0.35f, 0.22f, 0.13f), 3);
        Bar(root.transform, "LegR", new Vector2(2.3f, -3.9f), new Vector2(0.25f, 1.4f), new Color(0.35f, 0.22f, 0.13f), 3);
        Bar(root.transform, "Felt", new Vector2(0f, -1.78f), new Vector2(5.3f, 2.55f), new Color(0.13f, 0.4f, 0.24f), 4);
        Bar(root.transform, "Cloth", new Vector2(0f, -1.78f), new Vector2(4.9f, 2.2f), new Color(0.17f, 0.48f, 0.29f), 4);
        Bar(root.transform, "Trim", new Vector2(0f, -2.88f), new Vector2(4.9f, 0.04f), new Color(0.95f, 0.78f, 0.3f), 4);
        Bar(root.transform, "BackEdge", new Vector2(0f, -0.45f), new Vector2(5.5f, 0.28f), new Color(0.45f, 0.28f, 0.16f), 5);
        Bar(root.transform, "BackEdgeShine", new Vector2(0f, -0.37f), new Vector2(5.4f, 0.05f), new Color(0.65f, 0.45f, 0.28f), 5);
        Bar(root.transform, "Apron", new Vector2(0f, -3.17f), new Vector2(5.5f, 0.32f), new Color(0.32f, 0.2f, 0.12f), 5);

        // la macchiolina a corona sotto la posizione di mezzo (se ne vede appena il bordo sotto la carta)
        Transform stain = HW_Obj(root.transform, "Stain", new Vector2(0.05f, -2.36f), "v64_ink_crown", 6);
        BoxCollider2D stainCol = HW_Box(stain, new Vector2(0.9f, 0.7f));

        // le tre carte (posizioni prima del mescolamento)
        var cards = new List<Object>();
        var arts = new List<Object>();
        var areas = new List<Object>();
        for (int c = 0; c < RiggedCardsLogic.Cards; c++)
        {
            int slot = System.Array.IndexOf(RiggedCardsLogic.StartSlots, c);
            Transform t = Child(root.transform, "Card" + c, new Vector2(RiggedCardsLogic.SlotX[slot], -1.75f)).transform;
            t.localScale = Vector3.one * 1.5f;
            SpriteRenderer sr = Art(t, "Art", "v64_card_back", 10 + c, false).GetComponent<SpriteRenderer>();
            cards.Add(t);
            arts.Add(sr);
            areas.Add(HW_Box(t, new Vector2(0.72f, 0.95f)));
        }

        // le monete del mazziere
        var coins = new List<Object>();
        Vector2 pile = new Vector2(-1.95f, -0.95f);
        Vector2[] offs = { new Vector2(0f, 0f), new Vector2(0.16f, 0.07f), new Vector2(-0.13f, 0.1f), new Vector2(0.03f, 0.18f) };
        for (int i = 0; i < offs.Length; i++) coins.Add(HW_Obj(root.transform, "Coin" + i, pile + offs[i], "coin", 7 + i, 0.42f));
        BoxCollider2D coinsCol = HW_Area(root.transform, "CoinsArea", new Vector2(1.0f, 0.8f), pile + new Vector2(0f, 0.1f));

        // il cilindro abbandonato col Jolly dentro (tutto nascosto finché il mazziere non scappa)
        Transform dropped = Child(root.transform, "DroppedHat", new Vector2(1.85f, -0.95f)).transform;
        dropped.localScale = Vector3.one * 0.75f;
        Art(dropped, "Back", "s41b_tophat_back", 7, false);
        Transform joker = Child(dropped, "Joker", new Vector2(0f, 0.12f)).transform;
        Art(joker, "Art", "v64_card_joker", 8, false);
        TextMesh jokerText = Fit(Label(joker, new Vector2(0f, -0.28f), 9, "JOKER", 0.03f), 0.42f, 0.12f);
        jokerText.color = new Color(0.85f, 0.2f, 0.2f);
        Art(dropped, "Front", "s41b_tophat", 10, false);
        BoxCollider2D hatCol = HW_Box(dropped, new Vector2(1.8f, 1.7f));

        HW_Goal(root.transform, "FIND THE QUEEN!");
        SetArray(level, "cards", cards.ToArray());
        SetArray(level, "cardArts", arts.ToArray());
        SetArray(level, "cardAreas", areas.ToArray());
        SetField(level, "backSprite", sprites["v64_card_back"]);
        SetField(level, "twoSprite", sprites["v64_card_two_spades"]);
        SetField(level, "queenInkSprite", sprites["v64_card_queen_ink"]);
        SetArray(level, "introFaces", new Object[] { sprites["v64_card_queen"], sprites["v64_card_three_clubs"], sprites["v64_card_three_hearts"] });
        SetField(level, "stain", stain);
        SetField(level, "stainArea", stainCol);
        SetField(level, "dealer", dealer);
        SetField(level, "dealerHat", hat);
        SetField(level, "dealerArea", dealerCol);
        SetField(level, "dealerBubble", SmallBubble(root.transform, new Vector2(1.3f, 2.0f)));
        SetField(level, "miniDeck", deck);
        SetArray(level, "coins", coins.ToArray());
        SetField(level, "coinsArea", coinsCol);
        SetField(level, "droppedHat", dropped);
        SetField(level, "hatArea", hatCol);
        SetField(level, "joker", joker);
        V62_Wire(level, root.transform, new Vector2(0f, 2.65f));
        return SaveLevel(root, "V64RiggedCards");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V64C = new Dictionary<string, string[]>
    {
        { "v64_perfect_liar", new[] { "Look at how the detector was installed.",
            "Turn the detector's world upside down before testing anyone." } },
        { "v64_rigged_cards", new[] { "What if the Queen was never there?",
            "Look under the cards. Perhaps you can make your own Queen." } },
    };
}
#endif
