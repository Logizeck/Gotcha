#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.69, livello 266 (gruppo B): il quadro storto. Un soggiorno (vase_room_bg) con il quadro del gatto nobile
/// (heist_painting) appeso a un chiodo, la finestra, la mensola con il vasetto e la biglia, il tavolino con il bicchiere
/// d'acqua e la livella, la lampada a stelo, il nastro adesivo e il martello sul tappeto. Tutto è figlio di "Room",
/// ruotata di CrookedPictureLogic.RoomAngle: il quadro è dritto nella stanza, è la stanza a essere storta nello schermo.
/// Riusa gli aiuti di Halloween/0.62 (HW_Goal, HW_Obj, HW_Box, HW_Area, V62_Wire) e la maschera di V40A; i disegni nuovi
/// (prefisso v69_cp_: chiodo, filo, occhiolino, bicchiere, livella, bolla, nastro, martello) sono in DoodleSpritesV69B.
/// Niente Easter egg e niente Toni. L'elenco dei livelli (Define) lo aggiunge il capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 266 The Crooked Picture ----------

    static GameObject BuildV69CrookedPicture()
    {
        var root = new GameObject("Level_V69CrookedPicture");
        V69CrookedPictureLevel level = root.AddComponent<V69CrookedPictureLevel>();
        Transform r = root.transform;

        // ---- la stanza intera, ruotata (lo sfondo 9 x 14 copre lo schermo anche girato) ----
        Transform room = Child(r, "Room", new Vector2(0f, -0.2f)).transform;
        room.localRotation = Quaternion.Euler(0f, 0f, Gotcha.Levels.V64.CrookedPictureLogic.RoomAngle);
        Art(room, "Wall", "vase_room_bg", -20, false);
        // margini oltre lo sfondo (schermi larghi, es. tablet: gli angoli della stanza girata restano coperti)
        Bar(room, "WallMore", new Vector2(0f, 5.25f), new Vector2(18f, 13.5f), new Color(0.914f, 0.949f, 0.863f), -21);
        Bar(room, "FloorMore", new Vector2(0f, -9.2f), new Vector2(18f, 13.6f), new Color(0.851f, 0.627f, 0.416f), -21);
        HW_Obj(room, "Window", new Vector2(-1.85f, 0.85f), "window_frame", -5, 0.45f);

        // il quadro: il perno è il chiodo, il disegno pende 1.85 più in basso (coordinate di heist_painting)
        Vector2 nailAt = new Vector2(-0.1f, 2.45f);
        Transform swing = Child(room, "PictureSwing", nailAt).transform;
        swing.localScale = Vector3.one * 0.7f;
        Art(swing, "Wire", "v69_cp_picture_wire", 1, false, new Vector2(0f, -1.85f));
        Art(swing, "Painting", "heist_painting", 2, false, new Vector2(0f, -1.85f));
        SpriteRenderer wink = Art(swing, "Wink", "v69_cp_wink", 3, false, new Vector2(0f, -1.85f)).GetComponent<SpriteRenderer>();
        BoxCollider2D pictureCol = HW_Area(swing, "PictureArea", new Vector2(3.9f, 3.2f), new Vector2(0f, -1.75f));
        var strips = new List<Transform>();
        foreach (float s in new[] { -1f, 1f })
        {
            Transform strip = HW_Obj(swing, "Tape", new Vector2(s * 1.55f, -0.62f), "v69_cp_tape_strip", 4, 1.5f);
            strip.localRotation = Quaternion.Euler(0f, 0f, -s * 40f);
            strips.Add(strip);
        }
        HW_Obj(room, "Nail", nailAt, "v69_cp_nail", 5, 1f);
        Transform extraNail = HW_Obj(room, "ExtraNail", new Vector2(1.0f, 2.1f), "v69_cp_nail", 5, 1f);

        // la mensola con il vasetto (lato alto) e la biglia che rotola verso il lato basso
        const float ShelfY = -0.45f, ShelfTop = ShelfY + 0.03f;
        HW_Obj(room, "Shelf", new Vector2(1.5f, ShelfY), "wall_shelf", 1, 0.9f);
        HW_Obj(room, "Vase", new Vector2(0.82f, ShelfTop + 0.45f * 0.5f), "vase", 2, 0.5f);
        Transform track = Child(room, "MarbleTrack", new Vector2(1.55f, ShelfTop + 0.14f)).transform;
        Transform marble = HW_Obj(room, "Marble", new Vector2(1.55f + 0.85f * 0.48f, ShelfTop + 0.14f), "e40_marble", 3, 0.7f);
        CircleCollider2D marbleCol = marble.gameObject.AddComponent<CircleCollider2D>();
        marbleCol.radius = 0.6f;   // 0.42 nel mondo: molto più grande della biglia

        // il tavolino con il bicchiere d'acqua e la livella
        const float TableTop = -2.03f;
        HW_Obj(room, "Table", new Vector2(-0.85f, -2.05f), "hw_table", 1, 0.4f);
        Transform glass = Child(room, "Glass", new Vector2(-1.6f, TableTop)).transform;
        glass.localScale = Vector3.one * 1.3f;
        Art(glass, "Back", "v69_cp_glass_back", 2, false);
        V40A_Mask(glass, "Mask", "v69_cp_glass_mask", Vector2.zero, 1f);
        Transform waterPivot = Child(glass, "WaterPivot", new Vector2(0f, 0.47f)).transform;
        SpriteRenderer water = Bar(waterPivot, "Water", new Vector2(0f, -0.6f), new Vector2(1.4f, 1.2f), new Color(0.42f, 0.7f, 0.97f, 0.7f), 3);
        water.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        SpriteRenderer surface = Bar(waterPivot, "Surface", Vector2.zero, new Vector2(1.4f, 0.035f), new Color(0.88f, 0.96f, 1f, 0.95f), 4);
        surface.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        Art(glass, "Front", "v69_cp_glass_front", 5, false);
        BoxCollider2D glassCol = HW_Box(glass, new Vector2(0.95f, 1.25f), new Vector2(0f, 0.42f));

        Transform spirit = HW_Obj(room, "SpiritLevel", new Vector2(-0.45f, TableTop), "v69_cp_spirit_level", 2, 1.1f);
        Transform bubble = HW_Obj(spirit, "Bubble", new Vector2(0f, 0.115f), "v69_cp_level_bubble", 3, 1f);
        BoxCollider2D levelCol = HW_Box(spirit, new Vector2(1.3f, 0.7f), new Vector2(0f, 0.15f));

        // la lampada a stelo e gli attrezzi sul tappeto
        HW_Obj(room, "FloorLamp", new Vector2(1.8f, -3.0f), "s41f_floor_lamp", 1, 1f);
        Transform tape = HW_Obj(room, "TapeRoll", new Vector2(-0.2f, -3.85f), "v69_cp_tape_roll", 6, 1f);
        BoxCollider2D tapeCol = HW_Box(tape, new Vector2(1.0f, 0.95f), new Vector2(0.05f, 0.26f));
        Transform hammer = HW_Obj(room, "Hammer", new Vector2(0.8f, -3.75f), "v69_cp_hammer", 6, 1f);
        hammer.localRotation = Quaternion.Euler(0f, 0f, 8f);
        BoxCollider2D hammerCol = HW_Box(hammer, new Vector2(1.35f, 0.8f));

        HW_Goal(r, "STRAIGHTEN THE PICTURE.");
        SetField(level, "room", room);
        SetField(level, "pictureSwing", swing);
        SetField(level, "pictureArea", pictureCol);
        SetField(level, "wink", wink);
        SetArray(level, "tapeStrips", strips.ToArray());
        SetField(level, "extraNail", extraNail);
        SetField(level, "waterPivot", waterPivot);
        SetField(level, "glass", glass);
        SetField(level, "glassArea", glassCol);
        SetField(level, "spiritLevel", spirit);
        SetField(level, "levelBubble", bubble);
        SetField(level, "levelArea", levelCol);
        SetField(level, "marble", marble);
        SetField(level, "marbleTrack", track);
        SetField(level, "marbleArea", marbleCol);
        SetField(level, "tapeRoll", tape);
        SetField(level, "tapeArea", tapeCol);
        SetField(level, "hammer", hammer);
        SetField(level, "hammerArea", hammerCol);
        V62_Wire(level, r, new Vector2(0f, 2.72f));
        return SaveLevel(root, "V69CrookedPicture");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V69B = new Dictionary<string, string[]>
    {
        { "v69_crooked_picture", new[] { "Look at the water in the glass.",
            "Don't fix the picture. Fix your point of view: tilt the phone." } },
    };
}
#endif
