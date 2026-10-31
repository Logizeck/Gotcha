#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;
using Gotcha.Levels.V64;

/// <summary>
/// Versione 0.72, livello 269 (gruppo A): il gatto scassinatore. Riusa la stanza col tappeto (vase_room_bg), il gatto
/// seduto (roof_cat), il puntatore laser, il cuscino, la nuvoletta dei pensieri e gli aiuti di Halloween/0.62 (HW_Goal,
/// HW_Obj, HW_Box, HW_Area, V62_Wire); i disegni nuovi (prefisso v72_a_: sedia, tavolino, credenza, mensola, barattolo e
/// cocci, scaletta, libri, puntino e raggio, occhi e zampa del gatto) sono in DoodleSpritesV72A. I ripiani sono messi
/// esattamente dove li vuole CatBurglarLogic (stessi numeri). Niente Easter egg e niente Toni.
/// L'elenco dei livelli (Define) lo aggiunge il capo progetto in GotchaProjectBuilder.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 269 The Cat Burglar ----------

    static GameObject BuildV72CatBurglar()
    {
        var root = new GameObject("Level_V72CatBurglar");
        V72CatBurglarLevel level = root.AddComponent<V72CatBurglarLevel>();
        Transform r = root.transform;
        // la stanza abbassata di 1: il muro arriva al pavimento in y -3.4, il tappeto è davanti
        Art(r, "Room", "vase_room_bg", -20, false, new Vector2(0f, -1f));

        // ---- la "scala" dei mobili (i piani coincidono con i ripiani della logica) ----
        HW_Obj(r, "Chair", new Vector2(-1.98f, CatBurglarLogic.SurfaceY(CatPerch.Chair) - 0.9f), "v72_a_chair", -7);
        HW_Obj(r, "Table", new Vector2(-0.95f, CatBurglarLogic.SurfaceY(CatPerch.Table)), "v72_a_table", -9);
        HW_Obj(r, "Cupboard", new Vector2(0.55f, CatBurglarLogic.SurfaceY(CatPerch.Cupboard)), "v72_a_cupboard", -8);
        HW_Obj(r, "Shelf", new Vector2(1.1f, CatBurglarLogic.SurfaceY(CatPerch.Shelf)), "v72_a_shelf", -6);

        // ---- il barattolo (perno sul fondo: si inclina e cade) e i cocci ----
        Transform jar = Child(r, "CookieJar", new Vector2(CatBurglarLogic.JarStartX, CatBurglarLogic.SurfaceY(CatPerch.Shelf))).transform;
        Transform jarTilt = Child(jar, "Tilt", Vector2.zero).transform;
        jarTilt.localScale = Vector3.one * 1.15f;
        Art(jarTilt, "Art", "v72_a_cookie_jar", 6, false);
        BoxCollider2D jarCol = HW_Box(jar, new Vector2(1.3f, 1.5f), new Vector2(0f, 0.45f));
        Transform shards = HW_Obj(r, "Shards", new Vector2(CatBurglarLogic.LandX, -3.5f), "v72_a_jar_shards", 3);

        // ---- davanti, sul pavimento: il cuscino, la scaletta, la pila di libri ----
        Transform cushion = HW_Obj(r, "Cushion", new Vector2(-0.45f, -4.3f), "s41b_pillow", 4, 0.45f);
        BoxCollider2D cushionCol = HW_Box(cushion, new Vector2(3.4f, 1.9f));
        Transform ladder = HW_Obj(r, "Ladder", new Vector2(-1.85f, -4.85f), "v72_a_stepladder", 5);
        BoxCollider2D ladderCol = HW_Box(ladder, new Vector2(1.2f, 1.35f), new Vector2(0f, 0.48f));
        Transform books = Child(r, "Books", new Vector2(0.8f, -4.75f)).transform;
        var bookArt = new Transform[3];
        string[] bookNames = { "v72_a_book_red", "v72_a_book_blue", "v72_a_book_green" };
        Vector2[] bookPos = { new Vector2(0f, 0.08f), new Vector2(0.05f, 0.24f), new Vector2(-0.04f, 0.4f) };
        float[] bookTilt = { 0f, 3f, -2f };
        for (int i = 0; i < 3; i++)
        {
            bookArt[i] = HW_Obj(books, "Book" + i, bookPos[i], bookNames[i], 5 + i);
            bookArt[i].localRotation = Quaternion.Euler(0f, 0f, bookTilt[i]);
        }
        BoxCollider2D booksCol = HW_Box(books, new Vector2(1.15f, 0.95f), new Vector2(0f, 0.28f));

        // ---- il puntatore laser sul tavolino (punta in 0,0), il posto "in mano", il raggio, il puntino ----
        Transform pen = HW_Obj(r, "LaserPen", new Vector2(-0.45f, CatBurglarLogic.SurfaceY(CatPerch.Table) + 0.14f), "laser_pen", -4, 0.65f);
        BoxCollider2D penCol = HW_Box(pen, new Vector2(2.2f, 1.4f), new Vector2(-0.45f, 0f));
        Transform penTip = Child(pen, "Tip", new Vector2(0.08f, 0f)).transform;
        Transform penHand = Child(r, "PenHand", new Vector2(1.95f, -4.5f)).transform;
        Transform beamRoot = Child(r, "Beam", Vector2.zero).transform;
        SpriteRenderer beam = Art(beamRoot, "Art", "v72_a_beam", 29, false).GetComponent<SpriteRenderer>();
        Transform dot = HW_Obj(r, "RedDot", Vector2.zero, "v72_a_red_dot", 30);
        SpriteRenderer dotArt = dot.GetComponentInChildren<SpriteRenderer>();

        // ---- il gatto (piedi in 0,0): corpo, occhi mobili, zampa, il "!" ----
        Transform cat = Child(r, "Cat", new Vector2(CatBurglarLogic.CatStartX, CatBurglarLogic.SurfaceY(CatPerch.Floor))).transform;
        cat.localScale = Vector3.one * 0.8f;
        Transform body = Child(cat, "Body", Vector2.zero).transform;
        Art(body, "Art", "roof_cat", 10, false);
        Art(body, "Eyes", "v72_a_cat_eyes", 11, false);
        Transform pupilL = HW_Obj(body, "PupilL", new Vector2(-0.11f, 0.82f), "v72_a_cat_pupil", 12);
        Transform pupilR = HW_Obj(body, "PupilR", new Vector2(0.11f, 0.82f), "v72_a_cat_pupil", 12);
        Transform paw = HW_Obj(cat, "Paw", new Vector2(0.2f, 0.3f), "v72_a_paw", 13);
        TextMesh mark = Label(cat, new Vector2(0.02f, 1.42f), 14, "!", 0.09f);
        mark.color = new Color(0.9f, 0.2f, 0.25f);
        BoxCollider2D catCol = HW_Box(cat, new Vector2(1.5f, 1.8f), new Vector2(0.05f, 0.6f));
        // la nuvoletta: il gatto sogna il barattolo che cade
        Transform thought = Child(r, "Thought", new Vector2(0f, 0f)).transform;
        thought.localScale = Vector3.one * 0.3f;
        Art(thought, "Cloud", "dream_cloud", 22, false);
        Transform miniJar = Child(thought, "Jar", new Vector2(0.25f, -0.55f)).transform;
        miniJar.localScale = Vector3.one * 1.5f;
        miniJar.localRotation = Quaternion.Euler(0f, 0f, -35f);
        Art(miniJar, "Art", "v72_a_cookie_jar", 23, false);
        Transform miniShelf = Child(thought, "Shelf", new Vector2(-0.45f, -0.6f)).transform;
        miniShelf.localScale = new Vector3(0.75f, 1f, 1f);
        Art(miniShelf, "Art", "v72_a_shelf", 23, false);

        HW_Goal(r, "GET THE COOKIE JAR DOWN!");
        SetField(level, "cat", cat);
        SetField(level, "catBody", body);
        SetField(level, "catArea", catCol);
        SetField(level, "pupilL", pupilL);
        SetField(level, "pupilR", pupilR);
        SetField(level, "paw", paw);
        SetField(level, "catMark", mark);
        SetField(level, "catBubble", SmallBubble(r, new Vector2(0f, 0.5f)));
        SetField(level, "thought", thought);
        SetField(level, "jar", jar);
        SetField(level, "jarArt", jarTilt);
        SetField(level, "jarArea", jarCol);
        SetField(level, "shards", shards);
        SetField(level, "cushion", cushion);
        SetField(level, "cushionArea", cushionCol);
        SetField(level, "ladder", ladder);
        SetField(level, "ladderArea", ladderCol);
        SetField(level, "books", books);
        SetArray(level, "bookArt", bookArt);
        SetField(level, "booksArea", booksCol);
        SetField(level, "pen", pen);
        SetField(level, "penArea", penCol);
        SetField(level, "penTip", penTip);
        SetField(level, "penHand", penHand);
        SetField(level, "beamRoot", beamRoot);
        SetField(level, "beam", beam);
        SetField(level, "dot", dot);
        SetField(level, "dotArt", dotArt);
        V62_Wire(level, r, new Vector2(0f, 2.55f));
        return SaveLevel(root, "V72CatBurglar");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V72A = new Dictionary<string, string[]>
    {
        { "v72_cat_burglar", new[] { "Cats love two things: red dots and pushing stuff off shelves.", "Put something soft under the shelf, then lead the cat up with the laser." } },
    };
}
#endif
