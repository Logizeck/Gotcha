#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.68, livello 264 (gruppo B): l'uccellino che odia le foto. Lo schermo è l'app della fotocamera: un mirino
/// sul parco (cielo, nuvole, sole, alberi col ramo, siepe, panchina, lampione) che si sposta e si ingrandisce, le icone
/// del flash, dello zoom e del giro della fotocamera, il pulsante di scatto e l'anteprima dell'ultima foto. Riusa
/// l'uccellino, il ramo, l'albero, il cespuglio, le nuvole, il sole, l'omino generico s41b_gr_man (l'avatar del selfie,
/// non Toni), le linee del vento, l'alone e gli aiuti di Halloween/0.62 (HW_Goal, HW_Obj, HW_Box, V62_Wire); i disegni
/// nuovi (prefisso v68_: pose dell'uccellino, occhialini, occhi chiusi, panchina, lampione, icone) sono in
/// DoodleSpritesV68B. Niente Easter egg e niente Toni. L'elenco dei livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 264 The Camera-Shy Bird ----------

    static GameObject BuildV68CameraShyBird()
    {
        var root = new GameObject("Level_V68CameraShyBird");
        V68CameraShyBirdLevel level = root.AddComponent<V68CameraShyBirdLevel>();
        Transform r = root.transform;
        // il mirino occupa y -2.55..2.95 (centro 0.2); sopra e sotto le fasce scure dell'app
        const float VfY = 0.2f, VfTop = 2.95f, VfBottom = -2.55f;
        Color sky = new Color(0.62f, 0.83f, 0.98f), farGrass = new Color(0.56f, 0.8f, 0.44f), grass = new Color(0.47f, 0.75f, 0.37f);

        Transform views = Child(r, "Views", Vector2.zero).transform;   // si schiaccia in x quando si gira la fotocamera

        // ---- il parco (fotocamera posteriore): coordinate del panorama, origine al centro del mirino ----
        Transform park = Child(views, "Park", new Vector2(0f, VfY)).transform;
        Bar(park, "Sky", new Vector2(0f, 1.3f), new Vector2(40f, 3.8f), sky, -12);
        Bar(park, "FarGrass", new Vector2(0f, -0.75f), new Vector2(40f, 0.5f), farGrass, -11);
        Bar(park, "Grass", new Vector2(0f, -2.05f), new Vector2(40f, 2.5f), grass, -10);
        Transform sun = HW_Obj(park, "Sun", new Vector2(-6.6f, 2.0f), "sun", -9, 0.5f);
        foreach (Vector3 c in new[] { new Vector3(-4.6f, 2.2f, 0.7f), new Vector3(-1.4f, 2.35f, 0.55f), new Vector3(2.4f, 2.05f, 0.75f), new Vector3(6.1f, 2.3f, 0.6f) })
            HW_Obj(park, "Cloud", new Vector2(c.x, c.y), "cloud", -9, c.z);
        // tre alberi dietro la siepe, ognuno con un ramo basso all'altezza della siepe (il primo è quello dell'uccellino)
        var landmarks = new List<Transform>();
        foreach (Vector2 t in new[] { new Vector2(-0.3f, -1f), new Vector2(-5.6f, 1f), new Vector2(4.6f, 1f) })
        {
            HW_Obj(park, "Tree", new Vector2(t.x, 1.3f), "apple_tree", -6, 0.6f);
            Transform branch = HW_Obj(park, "Branch", new Vector2(t.x + t.y * 0.9f, 0.02f), "s41h_branch", -4, 0.7f);
            if (t.y > 0f) branch.localScale = new Vector3(-0.7f, 0.7f, 1f);
            landmarks.Add(branch);
        }
        // la siepe: lungo la cima ci si posa l'uccellino (y = 0 nel panorama)
        int n = 0;
        for (float x = -8.5f; x <= 8.6f; x += 1.25f)
            HW_Obj(park, "Hedge", new Vector2(x, -0.88f), "bush", 2 + (n++ % 2), 0.8f);
        Transform bench = HW_Obj(park, "Bench", new Vector2(-3.4f, -1.55f), "v68_bench", 12, 1f);
        Transform lamp = HW_Obj(park, "Lamp post", new Vector2(2.2f, -2.25f), "v68_lamp_post", 12, 0.9f);
        landmarks.Add(bench);
        landmarks.Add(lamp);
        landmarks.Add(sun);

        // l'uccellino: il perno è ai piedi (sulla siepe/ramo); "Pose" si gira verso l'obiettivo e cambia disegno
        Transform bird = Child(park, "Bird", new Vector2(Gotcha.Levels.V64.CameraShyBirdLogic.StartBirdX, 0f)).transform;
        bird.localScale = Vector3.one * 0.62f;
        Transform pose = Child(bird, "Pose", new Vector2(0f, 0.44f)).transform;
        SpriteRenderer birdArt = Art(pose, "Art", "v68_bird_peek", 8, false).GetComponent<SpriteRenderer>();
        SpriteRenderer shades = Art(pose, "Shades", "v68_bird_shades", 9, false).GetComponent<SpriteRenderer>();
        Transform leaves = HW_Obj(bird, "Leaves", new Vector2(0f, -0.05f), "bush", 10, 0.75f);
        TextMesh mark = Label(bird, new Vector2(0.15f, 1.35f), 11, "!", 0.07f);
        mark.color = new Color(1f, 0.85f, 0.2f);

        // ---- il selfie (fotocamera anteriore): l'omino nel parco ----
        Transform selfie = Child(views, "Selfie", Vector2.zero).transform;
        Bar(selfie, "Sky", new Vector2(0f, 1.6f), new Vector2(12f, 3.4f), sky, -12);
        Bar(selfie, "Grass", new Vector2(0f, -1.6f), new Vector2(12f, 3.2f), grass, -11);
        HW_Obj(selfie, "Cloud", new Vector2(-1.7f, 2.35f), "cloud", -9, 0.6f);
        HW_Obj(selfie, "Cloud", new Vector2(1.9f, 2.1f), "cloud", -9, 0.5f);
        HW_Obj(selfie, "Tree", new Vector2(-1.9f, 1.7f), "apple_tree", -7, 0.55f);
        for (float x = -4.4f; x <= 4.5f; x += 1.25f)
            HW_Obj(selfie, "Hedge", new Vector2(x, -0.55f), "bush", -5, 0.8f);
        Transform avatar = HW_Obj(selfie, "Avatar", new Vector2(0f, -1.0f), "s41b_gr_man", 10, 4.5f);
        SpriteRenderer blink = Art(avatar, "Blink", "v68_blink", 11, false).GetComponent<SpriteRenderer>();
        // l'uccellino del selfie: atterra sulla testa (la cima della testa è a y ≈ 0.98)
        Transform sBird = Child(selfie, "SelfieBird", new Vector2(3.4f, 2.4f)).transform;
        sBird.localScale = Vector3.one * 0.8f;
        Transform sPose = Child(sBird, "Pose", Vector2.zero).transform;
        SpriteRenderer sBirdArt = Art(sPose, "Art", "v68_bird_peace", 14, false).GetComponent<SpriteRenderer>();
        SpriteRenderer sShades = Art(sPose, "Shades", "v68_bird_shades", 15, false).GetComponent<SpriteRenderer>();
        Transform perch = Child(selfie, "Perch", new Vector2(0.3f, 0.98f + 0.42f * 0.8f)).transform;
        Transform sFrom = Child(selfie, "ArriveFrom", new Vector2(3.6f, 2.5f)).transform;
        Transform sTo = Child(selfie, "LeaveTo", new Vector2(-3.6f, 2.6f)).transform;

        // ---- sopra il mirino: griglia, messa a fuoco, scie, riflesso del flash, lampo dello scatto ----
        Transform grid = Child(r, "Grid", Vector2.zero).transform;   // regola dei terzi (sparisce nella foto)
        foreach (float x in new[] { -0.93f, 0.93f })
            Bar(grid, "GridV", new Vector2(x, VfY), new Vector2(0.02f, VfTop - VfBottom), new Color(1f, 1f, 1f, 0.28f), 30);
        foreach (float y in new[] { VfY - 0.92f, VfY + 0.92f })
            Bar(grid, "GridH", new Vector2(0f, y), new Vector2(6f, 0.02f), new Color(1f, 1f, 1f, 0.28f), 30);
        Transform focus = HW_Obj(r, "Focus", new Vector2(0f, VfY), "v68_focus", 31, 0.9f);
        Transform speed = Child(r, "Speed", Vector2.zero).transform;
        speed.localScale = new Vector3(0.5f, 0.45f, 1f);
        Art(speed, "Art", "wind_lines", 32, false);
        SpriteRenderer glare = Art(r, "Glare", "glow", 33, false, new Vector2(0f, 0.5f)).GetComponent<SpriteRenderer>();
        glare.transform.localScale = Vector3.one * 7.5f;
        glare.color = new Color(1f, 1f, 0.97f, 0f);
        SpriteRenderer flashWhite = Bar(r, "ShutterFlash", new Vector2(0f, VfY), new Vector2(12f, VfTop - VfBottom), new Color(1f, 1f, 1f, 0f), 34);

        // ---- la polaroid (cornice bianca dentro il mirino, sopra le icone; la foto è quello che c'è dietro, fermo) ----
        Transform frame = Child(r, "PhotoFrame", new Vector2(0f, VfY)).transform;
        Color ink = new Color(0.17f, 0.13f, 0.2f), paper = new Color(0.99f, 0.98f, 0.94f);
        // bordo: x ±2.45, y -2.6..2.6 (rispetto al centro); foro x ±2.2, y -1.95..2.35
        foreach (var b in new[] { (0f, 2.475f, 4.9f, 0.25f), (0f, -2.275f, 4.9f, 0.65f), (-2.325f, 0.2f, 0.25f, 4.3f), (2.325f, 0.2f, 0.25f, 4.3f) })
        {
            Bar(frame, "Ink", new Vector2(b.Item1, b.Item2), new Vector2(b.Item3 + 0.08f, b.Item4 + 0.08f), ink, 44);
            Bar(frame, "Paper", new Vector2(b.Item1, b.Item2), new Vector2(b.Item3, b.Item4), paper, 45);
        }
        TextMesh caption = Label(frame, new Vector2(0f, -2.28f), 46, "Lovely branch.", 0.045f);
        caption.color = ink;
        Fit(caption, 4.3f, 0.42f);

        // ---- l'app della fotocamera: fasce scure, icone e pulsanti ----
        Color appDark = new Color(0.07f, 0.07f, 0.09f);
        Bar(r, "TopBar", new Vector2(0f, VfTop + 3f), new Vector2(12f, 6f), appDark, 40);
        Bar(r, "BottomBar", new Vector2(0f, VfBottom - 3f), new Vector2(12f, 6f), appDark, 40);
        Transform flashBtn = HW_Obj(r, "Flash", new Vector2(-1.95f, 2.48f), "v68_flash_icon", 41, 1f);
        SpriteRenderer flashIcon = flashBtn.GetComponentInChildren<SpriteRenderer>();
        SpriteRenderer flashSlash = Bar(flashBtn, "Slash", Vector2.zero, new Vector2(0.06f, 0.62f), new Color(1f, 1f, 1f, 0.95f), 42);
        flashSlash.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        CircleCollider2D flashCol = flashBtn.gameObject.AddComponent<CircleCollider2D>();
        flashCol.radius = 0.6f;

        // zoom: pillola "1x" (tocco = 1x/2x) e i pulsanti -/+ (alternativa al pizzico)
        Transform zoomRow = Child(r, "Zoom", new Vector2(0f, VfBottom + 0.35f)).transform;
        Bar(zoomRow, "Pill", Vector2.zero, new Vector2(0.72f, 0.42f), new Color(0.1f, 0.1f, 0.12f, 0.6f), 41);
        TextMesh zoomText = Label(zoomRow, Vector2.zero, 42, "1x", 0.04f);
        zoomText.color = new Color(1f, 0.85f, 0.3f);
        Fit(zoomText, 0.62f, 0.3f);
        BoxCollider2D zoomPillCol = HW_Area(zoomRow, "PillArea", new Vector2(0.75f, 0.65f), Vector2.zero);
        var zoomCols = new BoxCollider2D[2];
        for (int i = 0; i < 2; i++)
        {
            float x = i == 0 ? -0.82f : 0.82f;
            Bar(zoomRow, i == 0 ? "MinusBack" : "PlusBack", new Vector2(x, 0f), new Vector2(0.48f, 0.42f), new Color(0.1f, 0.1f, 0.12f, 0.6f), 41);
            TextMesh sign = Label(zoomRow, new Vector2(x, 0.01f), 42, i == 0 ? "-" : "+", 0.05f);
            sign.color = Color.white;
            zoomCols[i] = HW_Area(zoomRow, i == 0 ? "MinusArea" : "PlusArea", new Vector2(0.85f, 0.7f), new Vector2(x, 0f));
        }

        // le modalità (decorative), lo scatto, il giro della fotocamera, l'anteprima dell'ultima foto
        foreach (var m in new[] { (-1.25f, "VIDEO", false), (0f, "PHOTO", true), (1.35f, "PORTRAIT", false) })
        {
            TextMesh t = Label(r, new Vector2(m.Item1, VfBottom - 0.32f), 43, m.Item2, 0.03f);
            t.color = m.Item3 ? new Color(1f, 0.85f, 0.3f) : new Color(0.75f, 0.75f, 0.78f);
        }
        Transform shutter = HW_Obj(r, "Shutter", new Vector2(0f, -3.6f), "v68_shutter_btn", 42, 1.15f);
        CircleCollider2D shutterCol = shutter.gameObject.AddComponent<CircleCollider2D>();
        shutterCol.radius = 0.68f;
        Transform flip = HW_Obj(r, "FlipCamera", new Vector2(1.7f, -3.6f), "v68_flip_icon", 42, 1f);
        CircleCollider2D flipCol = flip.gameObject.AddComponent<CircleCollider2D>();
        flipCol.radius = 0.62f;
        Transform thumb = Child(r, "Thumb", new Vector2(-1.7f, -3.6f)).transform;
        Bar(thumb, "Border", Vector2.zero, new Vector2(0.78f, 0.78f), new Color(0.92f, 0.92f, 0.95f), 41);
        SpriteRenderer thumbFill = Bar(thumb, "Fill", Vector2.zero, new Vector2(0.66f, 0.66f), new Color(0.22f, 0.22f, 0.26f), 42);
        BoxCollider2D thumbCol = HW_Box(thumb, new Vector2(1.05f, 1.05f));

        // area del mirino (trascinare = inquadrare, pizzico = zoom, tocco = mettere a fuoco)
        BoxCollider2D viewCol = HW_Area(r, "ViewArea", new Vector2(12f, VfTop - VfBottom), new Vector2(0f, VfY));

        // fumetto dell'uccellino ("No paparazzi.", "CHEESE!"): il livello lo sposta sopra l'uccellino
        SpeechBubble birdBubble = SmallBubble(r, new Vector2(0f, 1.6f));

        HW_Goal(r, "TAKE A PHOTO OF THE BIRD.");
        SetField(level, "views", views);
        SetField(level, "park", park);
        SetField(level, "selfieView", selfie);
        SetArray(level, "landmarks", landmarks.ToArray());
        SetField(level, "bird", bird);
        SetField(level, "birdPose", pose);
        SetField(level, "birdArt", birdArt);
        SetField(level, "birdShades", shades);
        SetField(level, "birdLeaves", leaves);
        SetField(level, "birdMark", mark);
        SetField(level, "peekSprite", sprites["v68_bird_peek"]);
        SetField(level, "stareSprite", sprites["v68_bird_stare"]);
        SetField(level, "hideSprite", sprites["v68_bird_hide"]);
        SetField(level, "flySprite", sprites["v68_bird_fly"]);
        SetField(level, "peaceSprite", sprites["v68_bird_peace"]);
        SetField(level, "avatar", avatar);
        SetField(level, "avatarBlink", blink);
        SetField(level, "selfieBird", sBird);
        SetField(level, "selfieBirdPose", sPose);
        SetField(level, "selfieBirdArt", sBirdArt);
        SetField(level, "selfieShades", sShades);
        SetField(level, "selfiePerch", perch);
        SetField(level, "selfieFrom", sFrom);
        SetField(level, "selfieTo", sTo);
        SetField(level, "grid", grid);
        SetField(level, "focusBox", focus);
        SetField(level, "speedLines", speed);
        SetField(level, "glare", glare);
        SetField(level, "shutterFlash", flashWhite);
        SetField(level, "photoFrame", frame);
        SetField(level, "photoCaption", caption);
        SetField(level, "flashButton", flashBtn);
        SetField(level, "flashIcon", flashIcon);
        SetField(level, "flashSlash", flashSlash);
        SetField(level, "flashArea", flashCol);
        SetField(level, "zoomText", zoomText);
        SetField(level, "zoomPillArea", zoomPillCol);
        SetField(level, "zoomOutArea", zoomCols[0]);
        SetField(level, "zoomInArea", zoomCols[1]);
        SetField(level, "shutterButton", shutter);
        SetField(level, "shutterArea", shutterCol);
        SetField(level, "flipButton", flip);
        SetField(level, "flipArea", flipCol);
        SetField(level, "thumb", thumb);
        SetField(level, "thumbFill", thumbFill);
        SetField(level, "thumbArea", thumbCol);
        SetField(level, "viewArea", viewCol);
        SetField(level, "birdBubble", birdBubble);
        V62_Wire(level, r, new Vector2(0f, -1.3f));
        return SaveLevel(root, "V68CameraShyBird");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V68B = new Dictionary<string, string[]>
    {
        { "v68_camera_shy_bird", new[] { "It loves being in photos. Just not the ones you aim at it.",
            "Turn the camera around." } },
    };
}
#endif
