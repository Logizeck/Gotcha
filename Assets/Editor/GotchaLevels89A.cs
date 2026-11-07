#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.89, livello 299 della roadmap (gruppo A): The Impossible Print. Lo stesso ufficio di PAPERWORK NIGHTMARE
/// (s41c_office_bg), la scrivania vuota (v89_a_desk), la stampante (radice alla base, scala 1.3; dentro Body, che trema,
/// il vassoio d'ingresso vuoto DIETRO, il corpo, il pulsante PRINT con la scritta, lo schermino con il testo e il bagliore
/// della spia; aree generose per PRINT, vassoio, stampante e la zona in cui lasciare il foglio; i punti sopra la fessura,
/// dentro la fessura e alla fessura d'uscita), la cucitrice e il cestino (s41c_fancy_bin) con le loro aree, il foglio
/// portato da PAPERWORK NIGHTMARE (s41c_sheet, nascosto finché la preferenza non c'è; posto di riposo e punto di
/// partenza fuori a sinistra), il foglio stampato (v89_a_page con "You rock!", bagliore dorato e scintille), la riga
/// dell'obiettivo, la scritta di commento (aiuti di Halloween) e la festa comune (RewardRig, sopra il foglio mostrato).
/// Il foglio che entra passa fra il vassoio (ordine 8) e il corpo (ordine 10): niente SpriteMask. Ogni TextMesh ha il
/// suo Fit. L'elenco dei livelli (Define, con l'Easter egg "you_rock") lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 299: The Impossible Print ----------

    static GameObject BuildV89LostSheet()
    {
        var root = new GameObject("Level_V89LostSheet");
        V89LostSheetLevel level = root.AddComponent<V89LostSheetLevel>();
        Transform r = root.transform;
        Art(r, "Office", "s41c_office_bg", -20, false);
        Art(r, "Desk", "v89_a_desk", 2, false);

        // ---- la stampante (radice alla base sul piano della scrivania) ----
        const float ps = 1.3f;
        Vector2 printerAt = new Vector2(0.65f, -1.05f);
        Transform printer = Child(r, "Printer", printerAt).transform;
        printer.localScale = Vector3.one * ps;
        Transform body = Child(printer, "Body", Vector2.zero).transform;
        Transform tray = Child(body, "Tray", new Vector2(0f, 1.0f)).transform;   // perno in basso: il rimbalzo lo allunga in su
        Art(tray, "Art", "v89_a_printer_back", 8, false, new Vector2(0f, -1.0f));
        Art(body, "Art", "v89_a_printer", 10, false);
        Transform button = Child(body, "PrintButton", new Vector2(DoodleSprites.V89AButtonX, DoodleSprites.V89AButtonY)).transform;
        Art(button, "Art", "v89_a_button", 11, false);
        TextMesh printLabel = Label(button, Vector2.zero, 12, "PRINT", 0.03f);
        printLabel.color = Color.white;
        Fit(printLabel, 0.36f, 0.16f);
        TextMesh lcd = Label(body, new Vector2(DoodleSprites.V89ALcdX, DoodleSprites.V89ALcdY), 12, "PAPER EMPTY", 0.03f);
        lcd.color = new Color(0.1f, 0.3f, 0.14f);
        Fit(lcd, DoodleSprites.V89ALcdHalfW * 2f - 0.1f, 0.18f);
        SpriteRenderer lamp = Art(body, "Lamp", "glow", 12, false, new Vector2(DoodleSprites.V89ALightX, DoodleSprites.V89ALightY)).GetComponent<SpriteRenderer>();
        lamp.transform.localScale = Vector3.one * 0.32f;
        lamp.color = new Color(1f, 0.25f, 0.2f, 0.85f);
        BoxCollider2D printArea = HW_Area(printer, "PrintArea", new Vector2(0.75f, 0.55f), new Vector2(DoodleSprites.V89AButtonX, DoodleSprites.V89AButtonY));
        BoxCollider2D trayArea = HW_Area(printer, "TrayArea", new Vector2(1.5f, 0.8f), new Vector2(0f, 1.5f));
        BoxCollider2D printerArea = HW_Area(printer, "PrinterArea", new Vector2(2.1f, 1.35f), new Vector2(0f, 0.62f));
        // dove lasciare il foglio: tutta la stampante e parecchio sopra (x da -0.78 a 2.08 nel mondo: il foglio a riposo resta fuori)
        BoxCollider2D feedZone = HW_Area(printer, "FeedZone", new Vector2(2.2f, 2.8f), new Vector2(0f, 1.15f));
        float slitY = printerAt.y + DoodleSprites.V89AInSlotY * ps;
        Transform feedPoint = Child(r, "FeedPoint", new Vector2(printerAt.x, slitY + 0.5f)).transform;
        Transform slotPoint = Child(r, "SlotPoint", new Vector2(printerAt.x, slitY - 0.62f)).transform;
        Transform outPoint = Child(r, "OutPoint", new Vector2(printerAt.x, printerAt.y + DoodleSprites.V89AOutSlotY * ps)).transform;

        // ---- cucitrice e cestino ----
        Transform stapler = HW_Obj(r, "Stapler", new Vector2(-1.75f, -0.68f), "v89_a_stapler", 12, 0.8f);
        BoxCollider2D staplerArea = HW_Box(stapler, new Vector2(1.1f, 0.65f), new Vector2(0f, 0.18f));
        Transform bin = HW_Obj(r, "Bin", new Vector2(-1.5f, -3.55f), "s41c_fancy_bin", 20, 0.95f);
        BoxCollider2D binArea = HW_Box(bin, new Vector2(1.45f, 1.55f), new Vector2(0f, 0.05f));

        // ---- il foglio portato da PAPERWORK NIGHTMARE ----
        Transform rest = Child(r, "SheetRest", new Vector2(-1.5f, -1.25f)).transform;
        rest.localRotation = Quaternion.Euler(0f, 0f, 10f);
        Transform from = Child(r, "SheetFrom", new Vector2(-3.9f, 1.1f)).transform;
        Transform sheet = Child(r, "Sheet", new Vector2(-1.5f, -1.25f)).transform;
        sheet.localRotation = rest.localRotation;
        SpriteRenderer sheetArt = Art(sheet, "Art", "s41c_sheet", 30, false).GetComponent<SpriteRenderer>();
        BoxCollider2D sheetArea = HW_Box(sheet, new Vector2(1.1f, 1.4f));   // a riposo il bordo destro resta a sinistra della zona della stampante
        sheet.gameObject.SetActive(false);

        // ---- il foglio stampato: uovo d'oro e "You rock!" ----
        Transform page = Child(r, "Page", (Vector2)outPoint.localPosition).transform;
        SpriteRenderer pageGlow = Art(page, "Glow", "glow", 49, false).GetComponent<SpriteRenderer>();
        pageGlow.color = new Color(1f, 0.85f, 0.3f, 0f);
        pageGlow.gameObject.SetActive(false);
        Art(page, "Art", "v89_a_page", 50, false);
        TextMesh pageText = Label(page, new Vector2(0f, -0.36f), 51, "You rock!", 0.05f);
        pageText.color = new Color(0.8f, 0.5f, 0.05f);
        pageText.fontStyle = FontStyle.Bold;
        Fit(pageText, 0.78f, 0.2f);
        var sparkles = new List<Object>();
        for (int i = 0; i < 5; i++)
        {
            Transform sp = Art(page, "Sparkle" + i, "sparkle_star", 52, false);
            sp.localScale = Vector3.one * 0.45f;
            SpriteRenderer sr = sp.GetComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.85f, 0.3f, 0f);
            sp.gameObject.SetActive(false);
            sparkles.Add(sr);
        }
        page.gameObject.SetActive(false);
        Transform pageShow = Child(r, "PageShow", new Vector2(0f, -0.4f)).transform;

        HW_Goal(r, "PRINT SOMETHING.");
        TextMesh info = HW_Toast(r, new Vector2(0f, -4.45f), out Transform infoHolder);
        SetField(level, "printerBody", body);
        SetField(level, "tray", tray);
        SetField(level, "printButton", button);
        SetField(level, "lcd", lcd);
        SetField(level, "lamp", lamp);
        SetField(level, "printArea", printArea);
        SetField(level, "trayArea", trayArea);
        SetField(level, "printerArea", printerArea);
        SetField(level, "feedZone", feedZone);
        SetField(level, "feedPoint", feedPoint);
        SetField(level, "slotPoint", slotPoint);
        SetField(level, "outPoint", outPoint);
        SetField(level, "stapler", stapler);
        SetField(level, "staplerArea", staplerArea);
        SetField(level, "bin", bin);
        SetField(level, "binArea", binArea);
        SetField(level, "sheet", sheet);
        SetField(level, "sheetArea", sheetArea);
        SetField(level, "sheetArt", sheetArt);
        SetField(level, "sheetRest", rest);
        SetField(level, "sheetFrom", from);
        SetField(level, "page", page);
        SetField(level, "pageText", pageText);
        SetField(level, "pageGlow", pageGlow);
        SetArray(level, "sparkles", sparkles.ToArray());
        SetField(level, "pageShow", pageShow);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, 1.65f), new Color(1f, 0.85f, 0.3f, 1f)));
        return SaveLevel(root, "V89LostSheet");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V89A = new Dictionary<string, string[]>
    {
        { "lost_sheet", new[] { "Paperwork Nightmare has plenty of sheets. Send one this way.",
            "In Paperwork Nightmare drag a sheet off the right edge of the screen, then come back here and put it in the printer." } },
    };
}
#endif
