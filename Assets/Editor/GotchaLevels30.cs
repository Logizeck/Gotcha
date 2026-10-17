#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>Versione 0.30: il carlino nero (prefab riutilizzabile) e il livello 101.</summary>
public static partial class GotchaProjectBuilder
{
    /// <summary>Pezzo del carlino: un pivot nell'articolazione e lo sprite spostato in modo che le coordinate del disegno restino quelle del carlino.</summary>
    static Transform PugPart(Transform parent, string name, string sprite, Vector2 pivot, Vector2 parentPivot, int order, bool wobble = false)
    {
        Transform t = Child(parent, name, pivot - parentPivot).transform;
        Art(t, "Art", sprite, order, wobble, -pivot);
        return t;
    }

    /// <summary>Carlino nero seduto, animato da PugRig (origine tra le zampe a terra, alto circa 2.3).</summary>
    static GameObject BuildPugPrefab()
    {
        var root = new GameObject("Pug");
        PugRig rig = root.AddComponent<PugRig>();

        Vector2 tailPivot = new Vector2(0.5f, 0.82f), neck = new Vector2(0f, 1.0f);
        Transform tail = PugPart(root.transform, "Tail", "pug_tail", tailPivot, Vector2.zero, 2);
        Transform body = PugPart(root.transform, "Body", "pug_body", Vector2.zero, Vector2.zero, 3, true);

        Transform head = PugPart(root.transform, "Head", "pug_head", neck, Vector2.zero, 5);
        Transform eyes = Child(head, "EyesOpen", new Vector2(0f, DoodleSprites.PugEyeY) - neck).transform;
        Vector2 eyesPivot = new Vector2(0f, DoodleSprites.PugEyeY);
        Art(eyes, "Left", "pug_eye_l", 6, false, -eyesPivot);
        Art(eyes, "Right", "pug_eye_r", 6, false, -eyesPivot);
        GameObject closed = PugPart(head, "EyesClosed", "pug_eyes_closed", eyesPivot, neck, 6).gameObject;
        Transform earL = PugPart(head, "EarL", "pug_ear_l", new Vector2(-0.3f, 2.1f), neck, 8);
        Transform earR = PugPart(head, "EarR", "pug_ear_r", new Vector2(0.3f, 2.1f), neck, 8);
        Transform mouth = PugPart(head, "MouthOpen", "pug_mouth_open", new Vector2(0f, 1.22f), neck, 7);
        Transform tongue = PugPart(head, "Tongue", "pug_tongue", new Vector2(0.2f, 1.08f), neck, 7);
        GameObject blep = PugPart(head, "Blep", "pug_blep", new Vector2(0f, 1.06f), neck, 7).gameObject;
        Transform nose = Child(head, "NoseTip", new Vector2(0.02f, 1.36f) - neck).transform;
        Transform snot = Child(head, "Snot", new Vector2(0.08f, 1.34f) - neck).transform;
        Art(snot, "Art", "pug_snot", 9, false);
        CircleCollider2D snotHit = snot.gameObject.AddComponent<CircleCollider2D>();
        snotHit.isTrigger = true;
        snotHit.offset = new Vector2(0.1f, 0f);
        snotHit.radius = 0.16f;

        // area da non toccare: testa e corpo
        CapsuleCollider2D area = root.AddComponent<CapsuleCollider2D>();
        area.isTrigger = true;
        area.offset = new Vector2(0f, 1.1f);
        area.size = new Vector2(1.7f, 2.3f);

        SmokeEmitter zzz = Emitter(head, "Zzz", new Vector2(0.45f, 1.0f), "zzz", 1.2f, 2.2f, 0.45f, 0.08f, 0.6f, Color.white);
        zzz.spinRange = 12f;
        zzz.growth = 0.9f;
        zzz.sideDrift = 0.2f;

        rig.body = body;
        rig.head = head;
        rig.earL = earL;
        rig.earR = earR;
        rig.eyesOpen = eyes;
        rig.eyesClosed = closed;
        rig.mouthOpen = mouth;
        rig.tongue = tongue;
        rig.blep = blep;
        rig.snot = snot;
        rig.tail = tail;
        rig.zzz = zzz;
        rig.noseTip = nose;
        rig.dropSprite = sprites["drop"];
        return SavePrefab(root, PrefabDir + "/Pug.prefab");
    }

    // ---------- 101: il carlino che dorme sulla chiave ----------

    static GameObject BuildPugLevel()
    {
        GameObject pugPrefab = BuildPugPrefab();
        GameObject keyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/Key.prefab");
        GameObject doorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/Door.prefab");

        var root = new GameObject("Level_Pug");
        PugLevel level = root.AddComponent<PugLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);

        GameObject door = Instance(doorPrefab, root.transform, new Vector2(1.55f, -1.1f));
        door.name = "Door";
        door.transform.localScale = Vector3.one * 0.72f;

        // mensola con la pepiera
        Bar(root.transform, "Shelf", new Vector2(-1.75f, 0.55f), new Vector2(1.3f, 0.12f), new Color(0.55f, 0.36f, 0.22f), 2);
        Bar(root.transform, "Bracket", new Vector2(-1.75f, 0.38f), new Vector2(0.08f, 0.3f), new Color(0.45f, 0.3f, 0.18f), 1);
        var pepperGo = new GameObject("Pepper");
        pepperGo.transform.SetParent(root.transform, false);
        pepperGo.transform.localPosition = new Vector3(-1.75f, 1.03f, 0);
        BoxCollider2D pepperHit = pepperGo.AddComponent<BoxCollider2D>();
        pepperHit.offset = new Vector2(0f, 0.04f);
        pepperHit.size = new Vector2(0.7f, 1.0f);
        Draggable2D pepper = pepperGo.AddComponent<Draggable2D>();
        pepper.itemId = "pepper";
        pepper.returnOnMiss = true;
        Art(pepperGo.transform, "Art", "pepper_shaker", 22);
        Transform spout = Child(pepperGo.transform, "Spout", new Vector2(0f, 0.45f)).transform;
        SmokeEmitter dust = Emitter(spout, "Dust", Vector2.zero, "pepper_grain", 30f, 0.8f, -1.6f, 0.08f, 1f, Color.white, 46);
        dust.growth = 0f;
        dust.spinRange = 0f;
        dust.sideDrift = 0.15f;

        // il carlino sul cuscino, con la chiave appesa al collare
        Art(root.transform, "Cushion", "pug_cushion", 1, true, new Vector2(-0.9f, -3.02f));
        GameObject pugGo = Instance(pugPrefab, root.transform, new Vector2(-0.9f, -3.05f));
        pugGo.transform.localScale = Vector3.one * 1.15f;
        PugRig rig = pugGo.GetComponent<PugRig>();
        SpeechBubble pugBubble = SmallBubble(root.transform, new Vector2(0.35f, 0.05f));
        SetField(rig, "bubble", pugBubble);

        GameObject key = Instance(keyPrefab, root.transform, new Vector2(-0.58f, -2.69f));   // appesa alla catena, accanto alla targhetta
        key.name = "Key";
        key.transform.localScale = Vector3.one * 0.45f;
        key.transform.localRotation = Quaternion.Euler(0, 0, -90f);   // appesa per l'anello, punta in giù

        SetField(level, "pug", rig);
        SetField(level, "pugArea", pugGo.GetComponent<Collider2D>());
        SetField(level, "snotArea", rig.snot.GetComponent<Collider2D>());
        SetField(level, "key", key.GetComponent<Draggable2D>());
        SetField(level, "pepper", pepper);
        SetField(level, "pepperSpout", spout);
        SetField(level, "pepperDust", dust);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(0f, 2.9f)));
        return SaveLevel(root, "Pug");
    }
}
#endif
