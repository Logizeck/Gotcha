#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.62: cinque livelli "vecchi oggetti, azioni nuove" (251-255, subito dopo lo speciale Halloween).
/// Riusano gli sprite esistenti (canna da pesca, calamita, Toni, gatto, palloncino, candela, specchio, ombrello...)
/// in modi inattesi; pochi disegni nuovi (prefisso v62_) in DoodleSpritesV62. Aiuti comuni: quelli di Halloween
/// (HW_Goal, HW_Obj, HW_Box, HW_Toast, HW_Confetti).
/// </summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels62() => new List<LevelDefinition>
    {
        Define("V62FishingKeys", "v62_fishing_keys", "FISHING FOR KEYS", "The key is made of metal. What sticks to metal?",
            BuildV62FishingKeys(), "v62_fish_in_sink"),
        Define("V62WakeToni", "v62_wake_toni", "WAKE UP, TONI!", "Noise and light won't do it. Something tastier might.",
            BuildV62WakeToni(), "v62_feather_tickle"),
        Define("V62AirMail", "v62_air_mail", "AIR MAIL", "Paper planes can't fly that high. What floats?",
            BuildV62AirMail(), "v62_balloon_cat"),
        Define("V62MirrorCat", "v62_mirror_cat", "MIRROR, MIRROR", "That cat ignores everything... except another cat.",
            BuildV62MirrorCat(), "v62_mirror_pug"),
        Define("V62Umbrella", "v62_umbrella_bucket", "UPSIDE-DOWN UMBRELLA", "The glass roof keeps the rain out. Bring the rain in.",
            BuildV62Umbrella(), "v62_umbrella_hat"),
    };

    // ---------- Aiuti comuni ----------

    /// <summary>Fascia "EASTER EGG UNLOCKED" col nome del segreto (lo scrive il livello), nascosta all'avvio.</summary>
    static GameObject V62_EggBanner(Transform root, out TextMesh name)
    {
        Transform banner = Child(root, "EggBanner", new Vector2(0f, 1.85f)).transform;
        Bar(banner, "Back", Vector2.zero, new Vector2(5.4f, 1.2f), new Color(0.32f, 0.12f, 0.45f, 0.95f), 85);
        TextMesh b1 = Label(banner, new Vector2(0f, 0.25f), 86, "EASTER EGG UNLOCKED", 0.06f);
        b1.color = new Color(1f, 0.86f, 0.3f);
        Fit(b1, 5.1f, 0.5f);
        name = Label(banner, new Vector2(0f, -0.27f), 86, "SOMETHING FISHY!", 0.05f);
        name.color = Color.white;
        Fit(name, 5.1f, 0.42f);
        return banner.gameObject;
    }

    /// <summary>Campi comuni dei livelli 0.62: fascia dell'Easter egg, coriandoli, scritta di commento.</summary>
    static void V62_Wire(Object level, Transform root, Vector2 toastAt)
    {
        GameObject banner = V62_EggBanner(root, out TextMesh eggName);
        SetField(level, "eggBanner", banner);
        SetField(level, "eggName", eggName);
        SetArray(level, "confetti", HW_Confetti(root));
        TextMesh info = HW_Toast(root, toastAt, out Transform infoHolder);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
    }

    /// <summary>Toni sdraiato sul suo cuscino (guarda a sinistra), con occhi aperti e codina da accendere dopo.</summary>
    static Transform V62_SleepingToni(Transform parent, Vector2 pos, float scale, int order, out Transform body, out GameObject eyesOpen)
    {
        Transform toni = Child(parent, "Toni", pos).transform;
        toni.localScale = Vector3.one * scale;
        Art(toni, "Cushion", "pug_cushion", order - 1, false);
        body = Child(toni, "Body", Vector2.zero).transform;
        Transform tail = Child(body, "Tail", new Vector2(0.98f, 0.36f)).transform;
        Art(tail, "Art", "pug_tail", order, false);
        Art(body, "Art", "toni_lie", order + 1);
        eyesOpen = Child(body, "EyesOpen", Vector2.zero);
        Art(eyesOpen.transform, "Eyes", "pug_eyes", order + 2, false);
        Art(eyesOpen.transform, "PupilL", "pug_pupil", order + 3, false, new Vector2(Gotcha.EditorArt.DoodleSprites.PugEyeLX - 0.02f, Gotcha.EditorArt.DoodleSprites.PugEyeY));
        Art(eyesOpen.transform, "PupilR", "pug_pupil", order + 3, false, new Vector2(Gotcha.EditorArt.DoodleSprites.PugEyeRX - 0.02f, Gotcha.EditorArt.DoodleSprites.PugEyeY));
        // dorme: occhi chiusi (quelli di "toni_lie") e codina nascosta finché non si alza
        eyesOpen.SetActive(false);
        tail.gameObject.SetActive(false);
        return toni;
    }

    // ---------- 251 Fishing for keys ----------

    static GameObject BuildV62FishingKeys()
    {
        var root = new GameObject("Level_V62FishingKeys");
        V62FishingKeysLevel level = root.AddComponent<V62FishingKeysLevel>();
        Art(root.transform, "Kitchen", "kitchen_bg", -20, false);

        // mobile col lavandino e lo scarico (la chiave si intravede)
        Bar(root.transform, "Cabinet", new Vector2(-0.75f, -1.32f), new Vector2(2.6f, 1.36f), new Color(0.8f, 0.55f, 0.36f), 1);
        Bar(root.transform, "CabinetLine", new Vector2(-0.75f, -1.32f), new Vector2(0.04f, 1.2f), new Color(0.55f, 0.36f, 0.24f), 2);
        Bar(root.transform, "Worktop", new Vector2(-0.75f, -0.6f), new Vector2(2.8f, 0.14f), new Color(0.55f, 0.36f, 0.24f), 2);
        HW_Obj(root.transform, "Sink", new Vector2(-0.75f, -0.68f), "drain_sink", 3, 1.15f);
        Transform drain = Child(root.transform, "Drain", new Vector2(-0.75f, -0.91f)).transform;
        Transform glint = HW_Obj(root.transform, "KeyGlint", new Vector2(-0.66f, -0.93f), "key", 4, 0.22f);
        glint.localRotation = Quaternion.Euler(0f, 0f, -25f);
        SmokeEmitter shine = Emitter(glint, "Shine", new Vector2(-0.3f, 0.1f), "sparkle_star", 1.5f, 0.6f, 0.1f, 0.25f, 0.6f, new Color(1f, 0.95f, 0.5f), 5);
        shine.spinRange = 90f;
        shine.Play();
        BoxCollider2D drainTap = HW_Area(root.transform, "DrainTap", new Vector2(1.7f, 0.7f), new Vector2(-0.75f, -0.85f));
        BoxCollider2D drainZone = HW_Area(root.transform, "DrainZone", new Vector2(2.2f, 1.9f), new Vector2(-0.75f, -0.05f));
        SmokeEmitter splash = Emitter(root.transform, "Splash", new Vector2(-0.75f, -0.85f), "puff", 20f, 0.6f, 0.9f, 0.4f, 0.35f, new Color(0.65f, 0.85f, 1f, 0.9f), 12);

        // la porta chiusa a destra, con la serratura sull'anta
        HW_Obj(root.transform, "DoorFrame", new Vector2(1.75f, -0.64f), "door_frame", 0, 0.7f);
        Transform panel = Child(root.transform, "DoorPanel", new Vector2(1.75f - 1.1f * 0.7f, -0.64f)).transform;
        panel.localScale = Vector3.one * 0.7f;
        Art(panel, "Art", "door_panel", 1, false);
        BoxCollider2D doorCol = HW_Box(panel, new Vector2(2.2f, 3.6f), new Vector2(1.1f, 0f));
        Transform keyhole = Child(panel, "Keyhole", new Vector2(1.8f, 0f)).transform;
        keyhole.localScale = Vector3.one * 1.6f;
        Art(keyhole, "Art", "keyhole", 2, false);
        BoxCollider2D lockCol = HW_Box(keyhole, new Vector2(0.6f, 0.7f));

        // la canna appoggiata al muro: l'origine è la punta, da cui pende la lenza con l'amo
        Transform rod = Child(root.transform, "Rod", new Vector2(-2.3f, 0.8f)).transform;
        // il disegno ha la punta in (0,0): ruotandolo attorno alla punta la canna si appoggia al muro
        Transform rodArt = Art(rod, "Art", "fishing_rod", 5, false);
        rodArt.localScale = Vector3.one * 1.1f;
        rodArt.localRotation = Quaternion.Euler(0f, 0f, 58f);
        rodArt.localPosition = Quaternion.Euler(0f, 0f, 58f) * (Vector3)(centers["fishing_rod"] * 1.1f);
        BoxCollider2D rodCol = HW_Box(rod, new Vector2(0.7f, 2.3f), new Vector2(-0.12f, -1.0f));
        Transform hook = Child(rod, "Hook", new Vector2(0f, -0.7f)).transform;
        HW_Obj(hook, "Art", Vector2.zero, "v62_hook", 7, 1.3f);
        Transform baitSpot = Child(hook, "BaitSpot", new Vector2(0f, -0.4f)).transform;
        LineRenderer line = Line(root.transform, "FishingLine", 0.022f, new Color(0.17f, 0.13f, 0.2f, 0.9f), 6);

        // calamita attaccata alla padella, pesce nella ciotola del gatto (che dorme lì vicino)
        Transform magnet = HW_Obj(root.transform, "Magnet", new Vector2(1.9f, 1.45f), "magnet", 8, 0.9f);
        BoxCollider2D magnetCol = HW_Box(magnet, new Vector2(0.85f, 0.8f), new Vector2(0f, -0.02f));
        Transform bowl = HW_Obj(root.transform, "CatBowl", new Vector2(0.6f, -3.9f), "cat_bowl", 5, 0.75f);
        Transform fish = HW_Obj(root.transform, "Fish", new Vector2(0.6f, -3.62f), "fish", 8, 0.9f);
        BoxCollider2D fishCol = HW_Box(fish, new Vector2(0.95f, 0.65f));
        Transform cat = HW_Obj(root.transform, "Cat", new Vector2(-1.3f, -3.75f), "cat", 4, 0.6f);
        BoxCollider2D catCol = HW_Box(cat, new Vector2(3.0f, 1.9f), new Vector2(0f, 0.1f));
        SmokeEmitter zzz = Emitter(root.transform, "Zzz", new Vector2(-1.55f, -3.2f), "zzz", 0.8f, 2.2f, 0.4f, 0.1f, 0.55f, Color.white, 9);
        zzz.spinRange = 12f;
        zzz.growth = 0.9f;
        zzz.Play();

        // quello che si può tirare su: la chiave vera, lo stivale, il piranha
        Transform key = HW_Obj(root.transform, "Key", new Vector2(-0.75f, -0.91f), "key", 9, 0.5f);
        BoxCollider2D keyCol = HW_Box(key, new Vector2(1.5f, 0.9f), new Vector2(-0.6f, 0f));
        Transform boot = HW_Obj(root.transform, "Boot", new Vector2(-0.75f, -0.91f), "fish_boot", 9, 0.7f);
        Transform piranha = HW_Obj(root.transform, "Piranha", new Vector2(-0.75f, -0.91f), "piranha", 9, 0.75f);

        HW_Goal(root.transform, "GET THE KEY OUT OF THE DRAIN!");
        SetField(level, "rod", rod);
        SetField(level, "rodArea", rodCol);
        SetField(level, "hook", hook);
        SetField(level, "baitSpot", baitSpot);
        SetField(level, "fishingLine", line);
        SetField(level, "magnet", magnet);
        SetField(level, "magnetArea", magnetCol);
        SetField(level, "fish", fish);
        SetField(level, "fishArea", fishCol);
        SetField(level, "drainZone", drainZone);
        SetField(level, "drainTapArea", drainTap);
        SetField(level, "drainPoint", drain);
        SetField(level, "keyGlint", glint.gameObject);
        SetField(level, "key", key);
        SetField(level, "keyArea", keyCol);
        SetField(level, "boot", boot);
        SetField(level, "piranha", piranha);
        SetField(level, "lockArea", lockCol);
        SetField(level, "lockPoint", keyhole);
        SetField(level, "doorPanel", panel);
        SetField(level, "doorArea", doorCol);
        SetField(level, "catArea", catCol);
        SetField(level, "splash", splash);
        SetField(level, "bubble", SmallBubble(root.transform, new Vector2(-0.9f, -2.55f)));
        V62_Wire(level, root.transform, new Vector2(0f, 2.45f));
        return SaveLevel(root, "V62FishingKeys");
    }

    // ---------- 252 Wake up, Toni! ----------

    static GameObject BuildV62WakeToni()
    {
        var root = new GameObject("Level_V62WakeToni");
        V62WakeToniLevel level = root.AddComponent<V62WakeToniLevel>();
        Art(root.transform, "Bedroom", "bedroom_bg", -20, false);

        // Toni che dorme sul cuscino, sul pavimento
        Transform toni = V62_SleepingToni(root.transform, new Vector2(0.55f, -3.6f), 1f, 3, out Transform body, out GameObject eyesOpen);
        GameObject eyesHalf = Art(body, "EyesHalf", "pug_eyes_half", 5, false).gameObject;
        Transform nose = Child(toni, "Nose", new Vector2(-0.8f, 0.22f)).transform;
        BoxCollider2D toniCol = HW_Box(toni, new Vector2(2.6f, 1.4f), new Vector2(0f, 0.15f));
        SmokeEmitter sneeze = Emitter(toni, "Sneeze", new Vector2(-1.05f, 0.2f), "puff", 22f, 0.7f, 0.3f, 0.25f, 0.5f, new Color(0.95f, 0.95f, 1f, 0.9f), 20);
        sneeze.sideDrift = 0.8f;
        SmokeEmitter zzz = Emitter(root.transform, "Zzz", new Vector2(-0.05f, -2.85f), "zzz", 1.2f, 2.2f, 0.45f, 0.1f, 0.7f, Color.white, 30);
        zzz.spinRange = 12f;
        zzz.growth = 0.9f;
        zzz.sideDrift = 0.15f;
        SmokeEmitter hearts = Emitter(root.transform, "Hearts", new Vector2(0.4f, -2.4f), "heart", 4f, 1.3f, 0.8f, 0.4f, 0.8f, Color.white, 35);

        // il gatto sveglio sul letto, la piuma sul materasso
        Transform cat = HW_Obj(root.transform, "Cat", new Vector2(1.2f, -1.95f), "roof_cat", 6, 0.95f);
        BoxCollider2D catCol = HW_Box(cat, new Vector2(1.3f, 1.35f), new Vector2(0.05f, 0.6f));
        Transform playSpot = Child(cat, "PlaySpot", new Vector2(-0.45f, 0.25f)).transform;
        Transform feather = HW_Obj(root.transform, "Feather", new Vector2(-1.75f, -1.75f), "feather", 9, 0.75f);
        feather.localRotation = Quaternion.Euler(0f, 0f, 15f);
        BoxCollider2D featherCol = HW_Box(feather, new Vector2(1.9f, 1.0f), new Vector2(0.55f, 0.2f));

        // il biscotto sulla mensola, la lampada appesa, la sveglia per terra
        Art(root.transform, "Shelf", "wall_shelf", 1, false, new Vector2(1.55f, 0.75f));
        Transform cookie = HW_Obj(root.transform, "Cookie", new Vector2(1.55f, 1.12f), "cookie", 10, 1.1f);
        CircleCollider2D cookieCol = cookie.gameObject.AddComponent<CircleCollider2D>();
        cookieCol.radius = 0.36f;
        Transform lamp = HW_Obj(root.transform, "Lamp", new Vector2(-0.3f, 2.05f), "lamp", 2, 0.75f);
        BoxCollider2D lampCol = HW_Box(lamp, new Vector2(1.5f, 1.3f), new Vector2(0f, 0.3f));
        SpriteRenderer lampGlow = Art(root.transform, "LampGlow", "glow", 3, false, new Vector2(-0.3f, 2.0f)).GetComponent<SpriteRenderer>();
        lampGlow.transform.localScale = Vector3.one * 2.2f;
        lampGlow.color = new Color(1f, 0.92f, 0.55f, 0f);
        SpriteRenderer lampLight = Art(root.transform, "LampLight", "glow", 1, false, new Vector2(-0.3f, 0.8f)).GetComponent<SpriteRenderer>();
        lampLight.transform.localScale = new Vector3(9f, 9f, 1f);
        lampLight.color = new Color(1f, 0.92f, 0.55f, 0f);
        Transform alarm = HW_Obj(root.transform, "Alarm", new Vector2(-1.95f, -4.45f), "alarm_clock", 7, 0.42f);
        BoxCollider2D alarmCol = HW_Box(alarm, new Vector2(2.4f, 2.1f), new Vector2(0f, 0.8f));
        SmokeEmitter waves = Emitter(root.transform, "AlarmWaves", new Vector2(-1.95f, -3.85f), "ring_waves", 6f, 0.5f, 0.3f, 0.05f, 0.35f, Color.white, 20);
        waves.growth = 0.8f;
        waves.spinRange = 0f;

        HW_Goal(root.transform, "WAKE TONI UP FOR HIS WALK!");
        SetField(level, "toni", toni);
        SetField(level, "toniBody", body);
        SetField(level, "toniArt", body.Find("Art").GetComponent<SpriteRenderer>());
        SetField(level, "toniLie", sprites["toni_lie"]);
        SetField(level, "toniStand", sprites["toni_stand"]);
        SetField(level, "toniArea", toniCol);
        SetField(level, "nose", nose);
        SetField(level, "eyesHalf", eyesHalf);
        SetField(level, "eyesOpen", eyesOpen);
        SetField(level, "tail", body.Find("Tail"));
        SetField(level, "zzz", zzz);
        SetField(level, "sneezePuff", sneeze);
        SetField(level, "hearts", hearts);
        SetField(level, "cat", cat);
        SetField(level, "catArea", catCol);
        SetField(level, "catPlaySpot", playSpot);
        SetField(level, "cookie", cookie);
        SetField(level, "cookieArea", cookieCol);
        SetField(level, "feather", feather);
        SetField(level, "featherArea", featherCol);
        SetField(level, "alarm", alarm);
        SetField(level, "alarmArea", alarmCol);
        SetField(level, "alarmWaves", waves);
        SetField(level, "lampArea", lampCol);
        SetField(level, "lampGlow", lampGlow);
        SetField(level, "lampLight", lampLight);
        SetField(level, "toniBubble", SmallBubble(root.transform, new Vector2(0.25f, -2.35f)));
        SetField(level, "catBubble", SmallBubble(root.transform, new Vector2(1.2f, -0.4f)));
        V62_Wire(level, root.transform, new Vector2(0f, 2.45f));
        return SaveLevel(root, "V62WakeToni");
    }

    // ---------- 253 Air mail ----------

    static GameObject BuildV62AirMail()
    {
        var root = new GameObject("Level_V62AirMail");
        V62AirMailLevel level = root.AddComponent<V62AirMailLevel>();
        Art(root.transform, "Park", "park_bg", -20, false);

        // la casetta col gatto sul tetto (in alto a destra)
        HW_Obj(root.transform, "House", new Vector2(1.65f, 1.45f), "v62_roof", -5, 0.85f);
        Transform roofCat = HW_Obj(root.transform, "RoofCat", new Vector2(1.75f, 1.47f), "roof_cat", 2, 0.8f);
        Transform paw = Child(roofCat, "Paw", new Vector2(-0.1f, 0.3f)).transform;
        BoxCollider2D roofZone = HW_Area(root.transform, "RoofZone", new Vector2(2.7f, 2.3f), new Vector2(1.65f, 2.1f));
        SmokeEmitter hearts = Emitter(root.transform, "Hearts", new Vector2(1.75f, 2.4f), "heart", 4f, 1.2f, 0.7f, 0.4f, 0.7f, Color.white, 35);

        // il palloncino legato al paletto
        Bar(root.transform, "Post", new Vector2(-2.0f, -3.62f), new Vector2(0.16f, 0.9f), new Color(0.55f, 0.37f, 0.24f), 3);
        Bar(root.transform, "PostCap", new Vector2(-2.0f, -3.17f), new Vector2(0.26f, 0.08f), new Color(0.4f, 0.26f, 0.17f), 4);
        Transform tie = Child(root.transform, "TiePoint", new Vector2(-2.0f, -3.15f)).transform;
        Transform balloon = Child(root.transform, "Balloon", new Vector2(-2.0f, -2.55f)).transform;
        GameObject balloonArt = Art(balloon, "Art", "balloon", 7, false).gameObject;
        balloonArt.transform.localScale = Vector3.one * 0.55f;
        balloonArt.transform.localPosition = centers["balloon"] * 0.55f;
        BoxCollider2D balloonCol = HW_Box(balloon, new Vector2(1.0f, 1.3f), new Vector2(0f, 0.62f));
        Transform letterSpot = Child(balloon, "LetterSpot", new Vector2(0.05f, -0.45f)).transform;
        BoxCollider2D knotCol = HW_Area(root.transform, "KnotArea", new Vector2(0.8f, 0.85f), new Vector2(-2.0f, -2.95f));
        LineRenderer str = Line(root.transform, "String", 0.02f, new Color(0.17f, 0.13f, 0.2f, 0.9f), 6);

        // tavolino con la lettera e la candela accesa
        HW_Obj(root.transform, "Table", new Vector2(0.55f, -2.95f), "hw_table", 2, 0.32f);
        Transform letter = HW_Obj(root.transform, "Letter", new Vector2(0.2f, -2.62f), "paper_sheet", 8, 0.12f);
        BoxCollider2D letterCol = HW_Box(letter, new Vector2(6.5f, 5.5f));
        Transform candle = HW_Obj(root.transform, "Candle", new Vector2(1.0f, -2.92f), "candle", 4, 1.5f);
        Transform flame = Art(candle, "Flame", "flame", 5, false, new Vector2(0f, 0.58f));
        Transform cglow = Art(candle, "Glow", "glow", 3, false, new Vector2(0f, 0.68f));
        cglow.localScale = Vector3.one * 0.9f;
        cglow.GetComponent<SpriteRenderer>().color = new Color(1f, 0.8f, 0.35f, 0.7f);
        BoxCollider2D candleCol = HW_Box(candle, new Vector2(0.45f, 0.95f), new Vector2(0f, 0.4f));
        Transform flamePoint = Child(candle, "FlamePoint", new Vector2(0f, 0.62f)).transform;

        // aeroplanino per terra, topo dietro il cespuglio
        Transform plane = HW_Obj(root.transform, "Plane", new Vector2(-0.65f, -4.2f), "paper_plane", 6, 0.8f);
        BoxCollider2D planeCol = HW_Box(plane, new Vector2(1.4f, 0.9f));
        Transform mouse = HW_Obj(root.transform, "Mouse", new Vector2(1.55f, -3.72f), "mouse", 4, 0.5f);
        BoxCollider2D mouseCol = HW_Box(mouse, new Vector2(1.6f, 1.4f), new Vector2(0f, 0.25f));
        HW_Obj(root.transform, "Bush", new Vector2(1.65f, -4.45f), "bush", 5, 0.9f);
        Transform ground = Child(root.transform, "Ground", new Vector2(0f, -4.25f)).transform;
        SmokeEmitter pop = Emitter(root.transform, "Pop", new Vector2(-2.0f, -1.9f), "puff", 30f, 0.5f, 0.5f, 0.35f, 0.5f, new Color(1f, 0.45f, 0.45f, 0.9f), 30);

        HW_Goal(root.transform, "DELIVER THE LETTER TO THE CAT ON THE ROOF!");
        SetField(level, "balloon", balloon);
        SetField(level, "balloonArt", balloonArt);
        SetField(level, "balloonArea", balloonCol);
        SetField(level, "letterSpot", letterSpot);
        SetField(level, "tiePoint", tie);
        SetField(level, "knotArea", knotCol);
        SetField(level, "balloonString", str);
        SetField(level, "letter", letter);
        SetField(level, "letterArea", letterCol);
        SetField(level, "plane", plane);
        SetField(level, "planeArea", planeCol);
        SetField(level, "flame", flamePoint);
        SetField(level, "candleArea", candleCol);
        SetField(level, "roofZone", roofZone);
        SetField(level, "roofCat", roofCat);
        SetField(level, "catPaw", paw);
        SetField(level, "mouse", mouse);
        SetField(level, "mouseArt", mouse.Find("Art").GetComponent<SpriteRenderer>());
        SetField(level, "mouseArea", mouseCol);
        SetField(level, "groundLeft", ground);
        SetField(level, "popPuff", pop);
        SetField(level, "hearts", hearts);
        SetField(level, "catBubble", SmallBubble(root.transform, new Vector2(0.75f, 2.55f)));
        SetField(level, "mouseBubble", SmallBubble(root.transform, new Vector2(1.1f, -2.6f)));
        V62_Wire(level, root.transform, new Vector2(-0.3f, 0.25f));
        return SaveLevel(root, "V62AirMail");
    }

    // ---------- 254 Mirror, mirror ----------

    static GameObject BuildV62MirrorCat()
    {
        var root = new GameObject("Level_V62MirrorCat");
        V62MirrorCatLevel level = root.AddComponent<V62MirrorCatLevel>();
        Art(root.transform, "Room", "room_bg", -20, false);

        // la mensola col vaso prezioso e il gatto che lo spinge verso il bordo sinistro
        HW_Obj(root.transform, "Shelf", new Vector2(0.25f, 0.4f), "wall_shelf", 1, 1.65f);
        Transform vase = HW_Obj(root.transform, "Vase", new Vector2(0.1f, 0.6f + 1.12f * 0.55f), "fancy_vase", 3, 0.55f);
        BoxCollider2D vaseCol = HW_Box(vase, new Vector2(1.5f, 2.2f));
        Transform edge = Child(root.transform, "VaseEdge", new Vector2(-1.3f, 0.6f + 1.12f * 0.55f)).transform;
        Transform cat = HW_Obj(root.transform, "Cat", new Vector2(1.05f, 0.6f), "roof_cat", 4, 1.0f);
        BoxCollider2D catCol = HW_Box(cat, new Vector2(1.15f, 1.25f), new Vector2(0.07f, 0.6f));
        Transform catFace = Child(cat, "Face", new Vector2(0f, 0.75f)).transform;
        SmokeEmitter fur = Emitter(cat, "Fur", new Vector2(0.05f, 0.6f), "puff", 18f, 0.6f, 0.4f, 0.45f, 0.35f, new Color(1f, 0.7f, 0.35f, 0.9f), 6);
        fur.sideDrift = 0.7f;

        // il vaso rotto (nascosto): cocci sparsi sul pavimento
        Transform floor = Child(root.transform, "FloorSpot", new Vector2(-1.55f, -2.9f)).transform;
        Transform shards = Child(root.transform, "Shards", new Vector2(-1.55f, -3.0f)).transform;
        Vector2[] spots = { new Vector2(-0.5f, 0.05f), new Vector2(0.1f, -0.15f), new Vector2(0.6f, 0.1f), new Vector2(-0.15f, 0.25f), new Vector2(0.35f, -0.35f) };
        for (int i = 0; i < 5; i++)
        {
            string name = "vase_shard" + i;
            Transform s = Child(shards, "Shard" + i, spots[i]).transform;
            s.localScale = Vector3.one * 0.55f;
            s.localRotation = Quaternion.Euler(0f, 0f, i * 47f - 60f);
            Art(s, "Art", name, 6, false, -centers[name]);
        }

        // il mobiletto: specchio, campanello, piatto col pesce
        HW_Obj(root.transform, "Table", new Vector2(-0.55f, -2.15f), "hw_table", 1, 0.38f);
        Transform mirror = HW_Obj(root.transform, "Mirror", new Vector2(-1.3f, -1.6f), "mirror", 8, 0.3f);
        BoxCollider2D mirrorCol = HW_Box(mirror, new Vector2(2.8f, 3.4f));
        GameObject catRefl = Art(mirror, "CatReflection", "roof_cat", 9, false, new Vector2(0.05f, -1.05f)).gameObject;
        catRefl.transform.localScale = new Vector3(-1.7f, 1.7f, 1f);
        catRefl.transform.localPosition = new Vector3(-centers["roof_cat"].x * 1.7f, -1.05f + centers["roof_cat"].y * 1.7f, 0f);
        catRefl.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.7f);
        GameObject toniRefl = Art(mirror, "ToniReflection", "toni_lie", 9, false).gameObject;
        toniRefl.transform.localScale = new Vector3(-0.85f, 0.85f, 1f);
        toniRefl.transform.localPosition = new Vector3(-centers["toni_lie"].x * 0.85f, -0.4f + centers["toni_lie"].y * 0.85f, 0f);
        toniRefl.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.7f);
        Transform bell = HW_Obj(root.transform, "Bell", new Vector2(-0.45f, -2.1f), "bell", 4, 0.75f);
        BoxCollider2D bellCol = HW_Box(bell, new Vector2(1.1f, 1.0f), new Vector2(0f, 0.4f));
        SmokeEmitter bellWaves = Emitter(root.transform, "BellWaves", new Vector2(-0.45f, -1.35f), "ring_waves", 8f, 0.5f, 0.2f, 0.05f, 0.3f, Color.white, 20);
        bellWaves.growth = 0.8f;
        bellWaves.spinRange = 0f;
        HW_Obj(root.transform, "Plate", new Vector2(0.4f, -1.95f), "cat_bowl", 3, 0.55f);
        Transform fish = HW_Obj(root.transform, "Fish", new Vector2(0.4f, -1.78f), "fish", 9, 0.85f);
        BoxCollider2D fishCol = HW_Box(fish, new Vector2(0.95f, 0.7f));

        // Toni che dorme sul tappeto
        Transform toni = V62_SleepingToni(root.transform, new Vector2(1.0f, -3.9f), 0.85f, 3, out Transform body, out GameObject eyes);
        Transform toniFace = Child(toni, "Face", new Vector2(-0.85f, 0.35f)).transform;
        BoxCollider2D toniCol = HW_Box(toni, new Vector2(2.6f, 1.4f), new Vector2(0f, 0.15f));
        SmokeEmitter zzz = Emitter(root.transform, "Zzz", new Vector2(0.45f, -3.1f), "zzz", 1.2f, 2.2f, 0.45f, 0.1f, 0.6f, Color.white, 30);
        zzz.spinRange = 12f;
        zzz.growth = 0.9f;

        HW_Goal(root.transform, "SAVE THE VASE!");
        SetField(level, "cat", cat);
        SetField(level, "catArea", catCol);
        SetField(level, "catFace", catFace);
        SetField(level, "vase", vase);
        SetField(level, "vaseArea", vaseCol);
        SetField(level, "vaseEdge", edge);
        SetField(level, "floorSpot", floor);
        SetField(level, "shards", shards.gameObject);
        SetField(level, "mirror", mirror);
        SetField(level, "mirrorArea", mirrorCol);
        SetField(level, "catReflection", catRefl);
        SetField(level, "toniReflection", toniRefl);
        SetField(level, "bell", bell);
        SetField(level, "bellArea", bellCol);
        SetField(level, "bellWaves", bellWaves);
        SetField(level, "fish", fish);
        SetField(level, "fishArea", fishCol);
        SetField(level, "toni", body);
        SetField(level, "toniArea", toniCol);
        SetField(level, "toniFace", toniFace);
        SetField(level, "toniEyes", eyes);
        SetField(level, "zzz", zzz);
        SetField(level, "fur", fur);
        SetField(level, "catBubble", SmallBubble(root.transform, new Vector2(0.55f, 2.25f)));
        SetField(level, "toniBubble", SmallBubble(root.transform, new Vector2(1.25f, -2.85f)));
        V62_Wire(level, root.transform, new Vector2(0f, -0.55f));
        return SaveLevel(root, "V62MirrorCat");
    }

    // ---------- 255 Upside-down umbrella ----------

    static GameObject BuildV62Umbrella()
    {
        var root = new GameObject("Level_V62Umbrella");
        V62UmbrellaLevel level = root.AddComponent<V62UmbrellaLevel>();
        Art(root.transform, "Garden", "gs_bg", -20, false);

        // la serra col tetto di vetro e la pianta assetata dentro
        Transform plant = HW_Obj(root.transform, "Plant", new Vector2(1.25f, -2.4f), "gs_plant", -3, 0.5f);
        HW_Obj(root.transform, "Greenhouse", new Vector2(1.25f, -2.6f), "gs_greenhouse", -2, 1.15f);
        BoxCollider2D plantCol = HW_Area(root.transform, "PlantArea", new Vector2(1.2f, 1.1f), new Vector2(1.25f, -1.95f));
        // l'ombrello va portato DENTRO la serra, sopra la pianta (sopra il tetto di vetro l'acqua non passa)
        BoxCollider2D plantZone = HW_Area(root.transform, "PlantZone", new Vector2(2.0f, 1.6f), new Vector2(1.25f, -0.95f));
        SmokeEmitter sparkle = Emitter(root.transform, "Sparkle", new Vector2(1.25f, -1.8f), "sparkle_star", 12f, 1.0f, 0.5f, 0.6f, 0.5f, new Color(1f, 0.95f, 0.5f), 30);
        sparkle.spinRange = 200f;
        SmokeEmitter hearts = Emitter(root.transform, "Hearts", new Vector2(1.25f, -1.0f), "heart", 4f, 1.2f, 0.8f, 0.4f, 0.7f, Color.white, 35);

        // la nuvola che piove a sinistra
        HW_Obj(root.transform, "Cloud", new Vector2(-1.45f, 1.75f), "cloud", 4, 0.9f, true);
        SmokeEmitter rain = Emitter(root.transform, "Rain", new Vector2(-1.45f, 1.25f), "drop", 14f, 1.25f, -3.6f, 1.0f, 0.9f, Color.white, 3);
        rain.growth = 0f;
        rain.spinRange = 0f;
        rain.sideDrift = 0.03f;
        BoxCollider2D rainZone = HW_Area(root.transform, "RainZone", new Vector2(2.5f, 4.1f), new Vector2(-1.45f, -0.8f));

        // l'ombrello chiuso, appoggiato: origine = centro della cupola
        Transform umbrella = Child(root.transform, "Umbrella", new Vector2(0.05f, -3.1f)).transform;
        umbrella.localScale = Vector3.one * 0.85f;
        Transform umbrellaArt = Child(umbrella, "Pivot", Vector2.zero).transform;
        // aperto e chiuso hanno lo stesso riquadro di disegno: si scambiano senza spostare niente
        SpriteRenderer umbrellaSprite = Art(umbrellaArt, "Art", "v62_umbrella_closed", 8, false).GetComponent<SpriteRenderer>();
        BoxCollider2D umbrellaCol = HW_Box(umbrella, new Vector2(2.0f, 2.1f), new Vector2(0f, -0.2f));
        Transform water = Child(umbrella, "Water", new Vector2(0f, -0.5f)).transform;
        Art(water, "Art", "v62_water", 9, false);
        Transform flip = HW_Obj(umbrella, "FlipButton", new Vector2(1.2f, 0.6f), "hw_undo", 12, 1.15f);
        CircleCollider2D flipCol = flip.gameObject.AddComponent<CircleCollider2D>();
        flipCol.radius = 0.34f;
        SmokeEmitter stream = Emitter(root.transform, "PourStream", new Vector2(0.6f, -1.2f), "drop", 30f, 0.6f, -3.2f, 0.12f, 0.9f, Color.white, 10);
        stream.growth = 0f;
        stream.spinRange = 0f;

        // rubinetto senza manopola su un paletto, talpa nella buca
        Bar(root.transform, "FaucetPost", new Vector2(-2.35f, -3.05f), new Vector2(0.14f, 1.4f), new Color(0.55f, 0.37f, 0.24f), 1);
        Transform faucet = HW_Obj(root.transform, "Faucet", new Vector2(-1.95f, -2.5f), "v62_faucet_bare", 2);
        BoxCollider2D faucetCol = HW_Box(faucet, new Vector2(1.0f, 0.95f), new Vector2(-0.3f, 0.05f));
        HW_Obj(root.transform, "MoleHole", new Vector2(0.95f, -4.2f), "gs_molehole", 2, 0.9f);
        Transform mole = HW_Obj(root.transform, "Mole", new Vector2(0.95f, -4.25f), "gs_mole", 3, 0.75f);
        BoxCollider2D moleCol = HW_Box(mole, new Vector2(1.7f, 1.9f), new Vector2(0f, 0.6f));
        Transform hatSpot = Child(mole, "HatSpot", new Vector2(0f, 1.35f)).transform;

        HW_Goal(root.transform, "WATER THE PLANT!");
        SetField(level, "umbrella", umbrella);
        SetField(level, "umbrellaArt", umbrellaArt);
        SetField(level, "umbrellaSprite", umbrellaSprite);
        SetField(level, "closedSprite", sprites["v62_umbrella_closed"]);
        SetField(level, "openSprite", sprites["gs_umbrella"]);
        SetField(level, "umbrellaArea", umbrellaCol);
        SetField(level, "water", water);
        SetField(level, "flipButton", flip);
        SetField(level, "flipArea", flipCol);
        SetField(level, "rainZone", rainZone);
        SetField(level, "plantZone", plantZone);
        SetField(level, "plant", plant);
        SetField(level, "plantArt", plant.Find("Art").GetComponent<SpriteRenderer>());
        SetField(level, "plantArea", plantCol);
        SetField(level, "faucetArea", faucetCol);
        SetField(level, "mole", mole);
        SetField(level, "moleArea", moleCol);
        SetField(level, "moleHatSpot", hatSpot);
        SetField(level, "rain", rain);
        SetField(level, "pourStream", stream);
        SetField(level, "sparkle", sparkle);
        SetField(level, "hearts", hearts);
        SetField(level, "moleBubble", SmallBubble(root.transform, new Vector2(1.6f, -2.95f)));
        V62_Wire(level, root.transform, new Vector2(0.3f, 2.6f));
        return SaveLevel(root, "V62Umbrella");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V62 = new Dictionary<string, string[]>
    {
        { "v62_fishing_keys", new[] { "Your fingers are too big, but a fishing rod reaches down there. It just needs the right bait.",
            "Drag the magnet from the frying pan onto the hook, then drag the rod over the sink and let go. Bring the key to the lock." } },
        { "v62_wake_toni", new[] { "The cookie is the answer, but the cat is too quick. Keep the cat busy first.",
            "Drag the feather onto the cat, then bring the cookie right under Toni's nose." } },
        { "v62_air_mail", new[] { "The balloon could carry the letter... if it weren't tied to the post.",
            "Drag the letter onto the balloon, tap the knot on the post, then tilt the phone to steer it to the roof. Avoid the candle!" } },
        { "v62_mirror_cat", new[] { "What scares a cat more than anything? Another cat staring back.",
            "Quick: drag the mirror onto the shelf, right next to the cat, before the vase falls." } },
        { "v62_umbrella_bucket", new[] { "An umbrella keeps the rain out... unless you double-tap it (or press the round arrow) and turn it upside down.",
            "Tap the umbrella to open it, flip it upside down, fill it under the rain, carry it into the greenhouse over the plant, then tilt the phone (or tap it) to pour." } },
    };
}
#endif
