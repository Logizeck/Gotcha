#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.72, livello 270 (gruppo B): il telecomando perso. Un soggiorno: il televisore a tubo sul mobiletto (i
/// tastini CH +/- sul fianco destro), la spina nella presa (v67_socket / v67_plug, filo come LineRenderer), l'orologio a
/// muro (v63_clock_face con le lancette fatte di barre), una pianta (pot + gs_plant), il tavolino col libretto delle
/// istruzioni (e la sua pagina aperta, nascosta), il gatto che dorme ("cat") col telecomando nascosto sotto il cuscino,
/// il divano con tre cuscini e sotto ognuno qualcosa (coin, sock, v40b_crumbs + mouse). Riusa gli aiuti di
/// Halloween/0.62 (HW_Goal, HW_Obj, HW_Box, HW_Area, V62_Wire) e il bagliore (glow); i disegni nuovi (prefisso v72_b_)
/// sono in DoodleSpritesV72B. Niente Easter egg e niente Toni. L'elenco dei livelli (Define) lo aggiunge il capo
/// progetto in GotchaProjectBuilder.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 270 The Lost Remote ----------

    static GameObject BuildV72LostRemote()
    {
        var root = new GameObject("Level_V72LostRemote");
        V72LostRemoteLevel level = root.AddComponent<V72LostRemoteLevel>();
        Transform r = root.transform;
        Art(r, "LivingRoom", "v72_b_living_bg", -20, false);

        // ---- il mobiletto e il televisore (coordinate locali del disegno: schermo in -0.04, 0.08; fianco x 1.35..1.62) ----
        HW_Obj(r, "Cabinet", new Vector2(-0.75f, 0.4f), "v72_b_cabinet", 1);
        BoxCollider2D cabinetCol = HW_Area(r, "CabinetArea", new Vector2(3.2f, 0.75f), new Vector2(-0.75f, 0.35f));
        Transform tv = HW_Obj(r, "TV", new Vector2(-0.75f, 1.68f), "v72_b_tv", 3);
        BoxCollider2D screenCol = HW_Box(tv, new Vector2(2.75f, 2.0f));
        Vector2 scr = new Vector2(-0.04f, 0.08f);
        SpriteRenderer paint = Art(tv, "ShowPaint", "v72_b_show_paint", 4, false, scr).GetComponent<SpriteRenderer>();
        Transform disco = HW_Obj(tv, "ShowDisco", scr, "v72_b_show_disco", 4);
        Transform dancer = HW_Obj(disco, "Dancer", new Vector2(0f, -0.12f), "v72_b_dancer", 5);
        disco.gameObject.SetActive(false);
        TextMesh title = Label(tv, scr + new Vector2(0f, 0.62f), 6, "PAINT DRYING LIVE — HOUR 37", 0.02f);
        title.color = Color.white;
        Fit(title, 1.9f, 0.15f);
        SpriteRenderer staticFx = Art(tv, "Static", "v72_b_static", 7, false, scr).GetComponent<SpriteRenderer>();
        staticFx.color = new Color(1f, 1f, 1f, 0f);
        SpriteRenderer led = Bar(tv, "PowerLed", new Vector2(1.17f, -0.86f), new Vector2(0.06f, 0.06f), new Color(1f, 0.25f, 0.2f), 4);
        // i tastini sul fianco: aree generose (metà alta = CH +, metà bassa = CH -)
        Transform chUp = Child(tv, "ChUp", new Vector2(1.47f, 0.28f)).transform;
        Transform chDown = Child(tv, "ChDown", new Vector2(1.47f, -0.12f)).transform;
        BoxCollider2D chUpCol = HW_Area(tv, "ChUpArea", new Vector2(0.9f, 1.0f), new Vector2(1.6f, 0.58f));
        BoxCollider2D chDownCol = HW_Area(tv, "ChDownArea", new Vector2(0.9f, 1.06f), new Vector2(1.6f, -0.45f));
        SpriteRenderer glint = Art(tv, "ButtonGlint", "glow", 8, false, new Vector2(1.47f, 0.08f)).GetComponent<SpriteRenderer>();
        glint.transform.localScale = Vector3.one * 0.35f;
        glint.color = new Color(1f, 1f, 0.85f, 0f);
        Transform cordAnchor = Child(tv, "CordAnchor", new Vector2(1.45f, -0.8f)).transform;

        // ---- la presa, la spina e il filo ----
        HW_Obj(r, "Socket", new Vector2(1.2f, 0.52f), "v67_socket", 2, 1f);
        LineRenderer cord = Line(r, "Cord", 0.045f, new Color(0.23f, 0.2f, 0.25f), 2);
        cord.positionCount = 4;
        Transform plug = HW_Obj(r, "Plug", new Vector2(0.96f, 0.52f), "v67_plug", 5, 1.1f);
        BoxCollider2D plugCol = HW_Box(plug, new Vector2(0.75f, 0.6f));
        Transform cordEnd = Child(plug, "CordEnd", new Vector2(-0.28f, -0.02f)).transform;
        cord.SetPositions(new[] { new Vector3(0.7f, 0.88f, 0f), new Vector3(0.66f, 0.62f, 0f), new Vector3(0.65f, 0.52f, 0f), new Vector3(0.65f, 0.5f, 0f) });

        // ---- l'orologio a muro (sbagliato di tre ore) con le pile nascoste ----
        Transform clock = HW_Obj(r, "Clock", new Vector2(1.78f, 2.4f), "v63_clock_face", 3, 0.8f);
        Transform hour = Child(clock, "Hour", Vector2.zero).transform;
        Bar(hour, "Bar", new Vector2(0f, 0.12f), new Vector2(0.07f, 0.26f), InkColor, 5);
        hour.localRotation = Quaternion.Euler(0f, 0f, -100f);
        Transform minute = Child(clock, "Minute", Vector2.zero).transform;
        Bar(minute, "Bar", new Vector2(0f, 0.18f), new Vector2(0.045f, 0.38f), InkColor, 5);
        minute.localRotation = Quaternion.Euler(0f, 0f, -210f);
        Transform second = Child(clock, "Second", Vector2.zero).transform;
        Bar(second, "Bar", new Vector2(0f, 0.17f), new Vector2(0.02f, 0.42f), new Color(0.88f, 0.21f, 0.17f), 6);
        CircleCollider2D clockCol = clock.gameObject.AddComponent<CircleCollider2D>();
        clockCol.radius = 0.6f;
        Transform batteries = Child(clock, "Batteries", Vector2.zero).transform;
        HW_Obj(batteries, "A", new Vector2(0f, 0.07f), "v72_b_battery", 12, 1.2f);
        HW_Obj(batteries, "B", new Vector2(0f, -0.07f), "v72_b_battery", 12, 1.2f);
        batteries.gameObject.SetActive(false);

        // ---- la pianta ----
        Transform plant = Child(r, "Plant", new Vector2(1.92f, 0.55f)).transform;
        HW_Obj(plant, "Pot", Vector2.zero, "pot", 3, 0.4f);
        HW_Obj(plant, "Leaves", Vector2.zero, "gs_plant", 2, 0.4f);
        BoxCollider2D plantCol = HW_Box(plant, new Vector2(0.8f, 1.3f), new Vector2(0f, 0.25f));

        // ---- il tavolino e il libretto delle istruzioni ----
        HW_Obj(r, "Table", new Vector2(-0.95f, -1.0f), "v72_b_table", 3);
        Transform manual = HW_Obj(r, "Manual", new Vector2(-1.2f, -0.86f), "v72_b_manual", 4);
        manual.localRotation = Quaternion.Euler(0f, 0f, -8f);
        TextMesh manualTitle = Label(manual, new Vector2(0.04f, 0.14f), 5, "TV MANUAL", 0.02f);
        manualTitle.color = InkColor;
        Fit(manualTitle, 0.5f, 0.09f);
        BoxCollider2D manualCol = HW_Box(manual, new Vector2(1.0f, 0.8f));

        // la pagina aperta (sopra a tutto, si chiude con un tocco qualsiasi)
        Transform popup = Child(r, "ManualPopup", new Vector2(0f, 0.3f)).transform;
        Bar(popup, "Dim", new Vector2(0f, -0.3f), new Vector2(7f, 12f), new Color(0.05f, 0.03f, 0.08f, 0.6f), 80);
        Transform page = HW_Obj(popup, "Page", new Vector2(0f, 0.1f), "v72_b_manual_page", 81);
        TextMesh pageTitle = Label(page, new Vector2(-0.85f, 0.98f), 82, "CHANGING CHANNELS", 0.03f);
        pageTitle.color = InkColor;
        Fit(pageTitle, 1.45f, 0.2f);
        TextMesh chLabel = Label(page, new Vector2(1.0f, -1.0f), 82, "CH +/-", 0.045f);
        chLabel.color = new Color(0.88f, 0.21f, 0.17f);
        Fit(chLabel, 0.75f, 0.26f);
        TextMesh pageNo = Label(page, new Vector2(-1.45f, -1.12f), 82, "P. 1", 0.02f);
        pageNo.color = new Color(0.55f, 0.5f, 0.45f);
        Fit(pageNo, 0.3f, 0.1f);
        TextMesh closeHint = Label(popup, new Vector2(0f, -1.45f), 82, "TAP TO CLOSE", 0.025f);
        closeHint.color = Color.white;
        Fit(closeHint, 2f, 0.16f);
        popup.gameObject.SetActive(false);

        // ---- il gatto che dorme, i "zzz" e il telecomando sotto il suo cuscino (ne spunta solo la punta) ----
        Transform cat = HW_Obj(r, "Cat", new Vector2(1.25f, -0.95f), "cat", 6, 0.62f);
        BoxCollider2D catCol = HW_Box(cat, new Vector2(3.0f, 1.9f), new Vector2(0f, 0.05f));
        Transform eyes = Art(cat, "Eyes", "v72_b_cat_eyes", 7, false);
        eyes.gameObject.SetActive(false);
        Transform zzz = HW_Obj(r, "Zzz", new Vector2(0.66f, -0.3f), "zzz", 7, 0.55f);
        Transform remote = HW_Obj(r, "Remote", new Vector2(1.88f, -1.42f), "v72_b_remote", 5, 1f);
        remote.localRotation = Quaternion.Euler(0f, 0f, 6f);
        BoxCollider2D remoteCol = HW_Box(remote, new Vector2(1.0f, 0.55f));
        Transform remoteHidden = Child(r, "RemoteHidden", new Vector2(1.88f, -1.42f)).transform;
        remoteHidden.localRotation = Quaternion.Euler(0f, 0f, 6f);
        Transform remoteRest = Child(r, "RemoteRest", new Vector2(1.0f, -1.6f)).transform;
        remoteRest.localRotation = Quaternion.Euler(0f, 0f, -12f);

        // ---- il divano: tre cuscini e cosa c'è sotto ----
        HW_Obj(r, "Sofa", new Vector2(0f, -2.6f), "v72_b_sofa", 8);
        float[] cx = { -1.22f, 0f, 1.22f };
        var cushions = new Object[3];
        var cushionAreas = new Object[3];
        var finds = new Object[3];
        for (int i = 0; i < 3; i++)
        {
            cushions[i] = HW_Obj(r, "Cushion" + i, new Vector2(cx[i], -2.72f), "v72_b_cushion", 10);
            cushionAreas[i] = HW_Area(r, "CushionArea" + i, new Vector2(1.22f, 0.95f), new Vector2(cx[i], -2.6f));
            Transform f = Child(r, "Find" + i, new Vector2(cx[i], -2.66f)).transform;
            finds[i] = f;
            switch (i)
            {
                case 0:
                    HW_Obj(f, "CoinA", new Vector2(-0.16f, 0f), "coin", 9, 0.42f);
                    HW_Obj(f, "CoinB", new Vector2(0.17f, -0.04f), "coin", 9, 0.36f);
                    break;
                case 1:
                    HW_Obj(f, "Sock", new Vector2(0f, 0.02f), "sock", 9, 0.5f).localRotation = Quaternion.Euler(0f, 0f, 75f);
                    break;
                default:
                    HW_Obj(f, "Crumbs", new Vector2(-0.32f, -0.05f), "v40b_crumbs", 9, 1f);
                    HW_Obj(f, "Mouse", new Vector2(0.12f, -0.02f), "mouse", 9, 0.28f);
                    HW_Obj(f, "MouseZzz", new Vector2(0.38f, 0.22f), "zzz", 9, 0.28f);
                    break;
            }
            f.gameObject.SetActive(false);
        }

        HW_Goal(r, "CHANGE THE CHANNEL!");
        SetField(level, "tv", tv);
        SetField(level, "screenArea", screenCol);
        SetField(level, "chUpArea", chUpCol);
        SetField(level, "chDownArea", chDownCol);
        SetField(level, "chUpButton", chUp);
        SetField(level, "chDownButton", chDown);
        SetField(level, "buttonGlint", glint);
        SetField(level, "showPaint", paint);
        SetField(level, "showDisco", disco);
        SetField(level, "dancer", dancer);
        SetField(level, "staticFx", staticFx);
        SetField(level, "showTitle", title);
        SetField(level, "powerLed", led);
        SetField(level, "cabinetArea", cabinetCol);
        SetField(level, "plug", plug);
        SetField(level, "plugArea", plugCol);
        SetField(level, "cordAnchor", cordAnchor);
        SetField(level, "cordEnd", cordEnd);
        SetField(level, "cord", cord);
        SetField(level, "clock", clock);
        SetField(level, "clockArea", clockCol);
        SetField(level, "secondHand", second);
        SetField(level, "clockBatteries", batteries);
        SetField(level, "manual", manual);
        SetField(level, "manualArea", manualCol);
        SetField(level, "manualPopup", popup);
        SetField(level, "plant", plant);
        SetField(level, "plantArea", plantCol);
        SetField(level, "cat", cat);
        SetField(level, "catArea", catCol);
        SetField(level, "catEyes", eyes);
        SetField(level, "zzz", zzz);
        SetField(level, "remote", remote);
        SetField(level, "remoteArea", remoteCol);
        SetField(level, "remoteHidden", remoteHidden);
        SetField(level, "remoteRest", remoteRest);
        SetArray(level, "cushions", cushions);
        SetArray(level, "cushionAreas", cushionAreas);
        SetArray(level, "finds", finds);
        V62_Wire(level, r, new Vector2(0f, -4.15f));
        return SaveLevel(root, "V72LostRemote");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V72B = new Dictionary<string, string[]>
    {
        { "v72_lost_remote", new[] { "Read the manual. Seriously.",
            "Look at the side of the TV." } },
    };
}
#endif
