#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Versione 0.64, livelli 251, 253 e 254 (gruppo A): il ladro d'impronte, il pesce che ha paura dell'acqua e la
/// sveglia del vicino. Riusano il telefono, i tasti, il lettore d'impronte, il pesce, il secchio, la pompa, la camera
/// da letto, la sveglia, la biglia e gli aiuti di Halloween/0.62 (HW_Goal, HW_Obj, HW_Box, V62_Wire); i pochi disegni
/// nuovi (prefisso v64_) sono in DoodleSpritesV64A. Niente Easter egg e niente Toni.
/// L'elenco dei livelli (Define) lo aggiunge il capo progetto in GotchaProjectBuilder.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- 251 The Fingerprint Thief ----------

    static GameObject BuildV64FingerprintThief()
    {
        var root = new GameObject("Level_V64FingerprintThief");
        V64FingerprintThiefLevel level = root.AddComponent<V64FingerprintThiefLevel>();
        Transform r = root.transform;
        Art(r, "Table", "table_bg", -20, false);

        // il telefono bloccato al centro (schermo largo ±1.06, alto ±2.2 attorno al centro)
        Transform phone = Child(r, "Phone", new Vector2(0.75f, -0.9f)).transform;
        Transform body = Child(phone, "Body", Vector2.zero).transform;
        body.localScale = Vector3.one * 0.7f;
        Art(body, "Art", "phone_lock", 1, false);
        GameObject lockScreen = Child(phone, "LockScreen", Vector2.zero);
        TextMesh status = Fit(Label(lockScreen.transform, new Vector2(0f, 1.9f), 4, "3 attempts remaining", 0.03f), 1.9f, 0.3f);
        status.color = new Color(0.85f, 0.88f, 1f);
        TextMesh pin = Fit(Label(lockScreen.transform, new Vector2(0f, 1.45f), 4, "_ _ _ _", 0.06f), 1.6f, 0.4f);
        pin.color = Color.white;
        var keys = new Transform[10];
        var keyAreas = new Collider2D[10];
        for (int d = 0; d < 10; d++)
        {
            Vector2 at = d == 0 ? new Vector2(0f, -0.7f) : new Vector2(-0.6f + 0.6f * ((d - 1) % 3), 0.95f - 0.55f * ((d - 1) / 3));
            Transform k = Child(lockScreen.transform, "Key" + d, at).transform;
            k.localScale = Vector3.one * 1.3f;
            Art(k, "Art", "keypad_key", 3, false);
            Fit(Label(k, new Vector2(0f, -0.005f), 4, d.ToString(), 0.03f), 0.3f, 0.28f);
            keys[d] = k;
            keyAreas[d] = HW_Box(k, new Vector2(0.44f, 0.41f));
        }
        // lettore d'impronte sotto il tastierino, con il LED accanto
        Transform scanner = Child(phone, "Scanner", new Vector2(0f, -1.5f)).transform;
        SpriteRenderer scanGlow = Art(scanner, "Glow", "glow", 2, false).GetComponent<SpriteRenderer>();
        scanGlow.transform.localScale = Vector3.one * 1.05f;
        scanGlow.color = new Color(1f, 1f, 1f, 0f);
        Art(scanner, "Ring", "lock_ring", 3, false).localScale = Vector3.one * 1.35f;
        Transform print = Art(scanner, "Print", "s41g_fprint", 3, false);
        print.localScale = Vector3.one * 0.6f;
        print.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.55f);
        CircleCollider2D scanCol = scanner.gameObject.AddComponent<CircleCollider2D>();
        scanCol.radius = 0.5f;
        SpriteRenderer ledGlow = Art(phone, "LedGlow", "glow", 3, false, new Vector2(0.75f, -1.5f)).GetComponent<SpriteRenderer>();
        ledGlow.transform.localScale = Vector3.one * 0.45f;
        ledGlow.color = new Color(1f, 1f, 1f, 0f);
        SpriteRenderer led = Art(phone, "Led", "lock_dot", 4, false, new Vector2(0.75f, -1.5f)).GetComponent<SpriteRenderer>();
        led.transform.localScale = Vector3.one * 0.8f;
        led.color = new Color(0.35f, 0.35f, 0.42f);
        // lo schermo sbloccato (nascosto)
        GameObject unlock = Child(phone, "UnlockScreen", Vector2.zero);
        Bar(unlock.transform, "Back", new Vector2(0f, 0.3f), new Vector2(2.05f, 3.7f), new Color(0.6f, 0.86f, 1f), 2);
        TextMesh unlockText = Fit(Label(unlock.transform, new Vector2(0f, 1.6f), 5, "FACE ID ENABLED", 0.045f), 1.9f, 0.4f);
        Transform bigFace = HW_Obj(r, "BigFace", new Vector2(0.6f, -0.75f), "guy_panic", 45, 1.55f);

        // la foto del proprietario che saluta con la mano alzata (sul tavolo, in alto a sinistra)
        Transform photo = Child(r, "Photo", new Vector2(-1.3f, 1.5f)).transform;
        photo.localScale = Vector3.one * 1.2f;
        var photoArts = new List<SpriteRenderer> { Art(photo, "Frame", "v64_photo", 30, false).GetComponent<SpriteRenderer>() };
        Transform owner = Child(photo, "Owner", new Vector2(-0.2f, -0.14f)).transform;
        owner.localScale = Vector3.one * 0.42f;
        photoArts.Add(Art(owner, "Art", "guy_relieved", 31, false).GetComponent<SpriteRenderer>());
        photoArts.Add(Art(photo, "Hand", "v64_photo_hand", 32, false).GetComponent<SpriteRenderer>());
        Transform tip = Child(photo, "Tip", new Vector2(Gotcha.EditorArt.DoodleSprites.V64PhotoTipX, Gotcha.EditorArt.DoodleSprites.V64PhotoTipY)).transform;
        BoxCollider2D photoCol = HW_Box(photo, new Vector2(1.65f, 1.95f));

        HW_Goal(r, "UNLOCK THE PHONE WITHOUT KNOWING THE PIN.");
        SetField(level, "phone", phone);
        SetArray(level, "keys", keys);
        SetArray(level, "keyAreas", keyAreas);
        SetField(level, "pinText", pin);
        SetField(level, "statusText", status);
        SetField(level, "scanner", scanner);
        SetField(level, "scannerArea", scanCol);
        SetField(level, "scannerGlow", scanGlow);
        SetField(level, "led", led);
        SetField(level, "ledGlow", ledGlow);
        SetField(level, "photo", photo);
        SetField(level, "photoArea", photoCol);
        SetField(level, "photoTip", tip);
        SetArray(level, "photoArts", photoArts.ToArray());
        SetField(level, "lockScreen", lockScreen);
        SetField(level, "unlockScreen", unlock);
        SetField(level, "unlockText", unlockText);
        SetField(level, "bigFace", bigFace);
        V62_Wire(level, r, new Vector2(0f, 2.65f));
        return SaveLevel(root, "V64FingerprintThief");
    }

    // ---------- 253 The Fish That Is Afraid of Water ----------

    static GameObject BuildV64PlasticFish()
    {
        var root = new GameObject("Level_V64PlasticFish");
        V64PlasticFishLevel level = root.AddComponent<V64PlasticFishLevel>();
        Transform r = root.transform;
        Art(r, "Kitchen", "kitchen_bg", -20, false);

        // la boccia tonda col collo (raggio interno 1.5) sul piano della cucina
        Transform bowl = Child(r, "Bowl", new Vector2(0f, -0.2f)).transform;
        Art(bowl, "Back", "v64_bowl_back", 1, false);
        V40A_Mask(bowl, "Mask", "v64_bowl_mask", Vector2.zero, 1f);
        Transform pivot = Child(bowl, "WaterPivot", Vector2.zero).transform;
        SpriteRenderer water = Bar(pivot, "Water", new Vector2(0f, -0.3f), new Vector2(4f, 4f), new Color(0.32f, 0.62f, 0.95f, 0.55f), 2);
        water.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        SpriteRenderer line = Bar(pivot, "WaterLine", new Vector2(0f, 1.7f), new Vector2(4f, 0.06f), new Color(0.85f, 0.95f, 1f, 0.9f), 3);
        line.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        SmokeEmitter tankBubbles = Emitter(bowl, "TankBubbles", new Vector2(0.55f, -1.25f), "sub_bubble", 2.5f, 2.2f, 0.6f, 0.35f, 0.5f, Color.white, 4);
        tankBubbles.growth = 0.2f;
        tankBubbles.spinRange = 0f;
        Transform neck = Child(bowl, "Neck", new Vector2(0f, 1.8f)).transform;
        SmokeEmitter spill = Emitter(bowl, "Spill", new Vector2(0f, 1.8f), "drop", 10f, 0.8f, -1.6f, 0.4f, 0.5f, new Color(0.55f, 0.8f, 1f, 0.95f), 8);
        spill.growth = 0f;
        spill.spinRange = 0f;

        // il pesce (giocattolo gonfiabile): corpo rigido e la valvolina sulla schiena
        Transform fish = Child(r, "Fish", new Vector2(0f, 0.95f)).transform;
        Transform fishBody = Child(fish, "Body", Vector2.zero).transform;
        fishBody.localScale = Vector3.one * 1.6f;
        Art(fishBody, "Art", "fish", 5, false);
        Transform valve = Child(fishBody, "Valve", new Vector2(-0.1f, 0.11f)).transform;
        SpriteRenderer valveArt = Art(valve, "Art", "v64_valve", 6, false).GetComponent<SpriteRenderer>();
        BoxCollider2D fishCol = HW_Box(fish, new Vector2(1.3f, 0.95f));
        SmokeEmitter blow = Emitter(fish, "BlowBubbles", new Vector2(-0.45f, 0f), "sub_bubble", 9f, 1.0f, 0.8f, 0.15f, 0.45f, Color.white, 7);
        blow.growth = 0.3f;
        blow.spinRange = 0f;
        Art(bowl, "Front", "v64_bowl_front", 9, false);
        var shards = new List<Transform>();
        for (int i = 0; i < 4; i++)
        {
            Transform s = HW_Obj(r, "Shard" + i, new Vector2(0f, 1.6f), "glass_shard_a", 12, 0.45f + 0.1f * (i % 2));
            s.localRotation = Quaternion.Euler(0f, 0f, 70f * i);
            shards.Add(s);
        }

        // la pompa (aggiunge acqua) col tubo che entra nel collo, il secchio pieno d'acqua
        Transform pump = HW_Obj(r, "Pump", new Vector2(1.85f, -2.45f), "pump", 3, 0.75f);
        Transform handle = Child(pump, "Handle", new Vector2(0f, 1.3f)).transform;
        Art(handle, "Art", "pump_handle", 2, false);
        BoxCollider2D pumpCol = HW_Box(pump, new Vector2(1.4f, 2.4f), new Vector2(0f, 0.8f));
        LineRenderer hose = Line(r, "Hose", 0.09f, new Color(0.24f, 0.3f, 0.4f), 2);
        hose.positionCount = 4;
        hose.SetPositions(new[] { new Vector3(2.1f, -1.85f, 0f), new Vector3(2.2f, 0.4f, 0f), new Vector3(1.2f, 2.0f, 0f), new Vector3(0.3f, 1.62f, 0f) });
        Transform bucket = HW_Obj(r, "Bucket", new Vector2(-1.85f, -2.55f), "mixer_bucket", 3, 0.55f);
        BoxCollider2D bucketCol = HW_Box(bucket, new Vector2(2.1f, 2.1f), new Vector2(0f, -0.15f));

        // ossigeno (scende pianissimo), sul davanti del piano
        Transform o2 = GaugeBar(r, "Oxygen", new Vector2(0f, -3.05f), new Vector2(1.9f, 0.24f), new Color(0.1f, 0.1f, 0.18f, 0.8f), new Color(0.45f, 0.85f, 1f), 20);
        TextMesh o2Text = Fit(Label(r, new Vector2(0f, -3.04f), 22, "O2 100%", 0.03f), 1.7f, 0.21f);
        o2Text.color = Color.white;

        // l'alternativa al microfono (compare solo se serve)
        Transform micBtn = HW_Obj(r, "MicButton", new Vector2(-0.55f, -4.05f), "v64_mic_btn", 30, 1.1f);
        TextMesh micLabel = Fit(Label(micBtn, new Vector2(1.0f, 0f), 31, "HOLD TO BLOW", 0.03f), 1.3f, 0.22f);
        micLabel.color = Color.white;
        CircleCollider2D micCol = micBtn.gameObject.AddComponent<CircleCollider2D>();
        micCol.radius = 0.55f;

        HW_Goal(r, "SAVE THE FISH BEFORE IT DROWNS!");
        SetField(level, "bowl", bowl);
        SetField(level, "waterPivot", pivot);
        SetField(level, "water", water.transform);
        SetField(level, "waterLine", line.transform);
        SetField(level, "fish", fish);
        SetField(level, "fishBody", fishBody);
        SetField(level, "fishArea", fishCol);
        SetField(level, "valve", valve);
        SetField(level, "valveArt", valveArt);
        SetField(level, "valveClosed", sprites["v64_valve"]);
        SetField(level, "valveOpen", sprites["v64_valve_open"]);
        SetField(level, "blowBubbles", blow);
        SetField(level, "tankBubbles", tankBubbles);
        SetField(level, "spill", spill);
        SetField(level, "neck", neck);
        SetArray(level, "shards", shards.ToArray());
        SetField(level, "pump", pump);
        SetField(level, "pumpHandle", handle);
        SetField(level, "pumpArea", pumpCol);
        SetField(level, "bucket", bucket);
        SetField(level, "bucketArea", bucketCol);
        SetField(level, "oxygenFill", o2);
        SetField(level, "oxygenText", o2Text);
        SetField(level, "micButton", micBtn);
        SetField(level, "micButtonArea", micCol);
        V62_Wire(level, r, new Vector2(0f, 2.6f));
        return SaveLevel(root, "V64PlasticFish");
    }

    // ---------- 254 The Neighbour's Alarm ----------

    static GameObject BuildV64NeighboursAlarm()
    {
        var root = new GameObject("Level_V64NeighboursAlarm");
        V64NeighboursAlarmLevel level = root.AddComponent<V64NeighboursAlarmLevel>();
        Transform r = root.transform;
        Art(r, "Room", "hw_bedroom_bg", -20, false);

        // il gallo fuori dalla finestra (compare alla fine)
        Transform rooster = HW_Obj(r, "Rooster", new Vector2(0.2f, 1.62f), "v64_rooster", -12, 0.85f);

        // il letto con chi dorme, il cuscino
        Transform bed = Child(r, "Bed", new Vector2(-1.05f, -2.62f)).transform;
        SpriteRenderer sleeper = Art(bed, "Art", "kid_bed_sleep", 2, false).GetComponent<SpriteRenderer>();
        Transform head = Child(bed, "Head", new Vector2(-0.7f, 0.3f)).transform;
        SmokeEmitter zzz = Emitter(r, "Zzz", new Vector2(-1.75f, -1.95f), "zzz", 0.9f, 2.2f, 0.4f, 0.1f, 0.4f, Color.white, 30);
        zzz.spinRange = 12f;
        zzz.growth = 0.9f;
        zzz.sideDrift = 0.15f;
        Transform pillow = HW_Obj(r, "Pillow", new Vector2(-0.3f, -2.25f), "s41b_pillow", 7, 0.35f);
        BoxCollider2D pillowCol = HW_Box(pillow, new Vector2(3.4f, 1.7f));

        // la crepa nel pavimento, nascosta sotto il comodino
        Transform crack = Child(r, "Crack", new Vector2(0.95f, -3.47f)).transform;
        Bar(crack, "Gap", Vector2.zero, new Vector2(0.42f, 0.05f), new Color(0.12f, 0.07f, 0.05f, 0.9f), 1);
        Transform crackArt = Art(crack, "Art", "s41c_crack", 1, false);
        crackArt.localRotation = Quaternion.Euler(0f, 0f, 90f);
        crackArt.localScale = new Vector3(0.8f, 1f, 1f);
        crackArt.GetComponent<SpriteRenderer>().color = new Color(0.25f, 0.15f, 0.1f, 0.85f);
        BoxCollider2D crackCol = HW_Box(crack, new Vector2(0.9f, 0.6f));

        // il comodino con la sveglia (la sveglia è figlia del comodino: si sposta con lui)
        Transform table = Child(r, "Table", new Vector2(0.95f, -2.25f)).transform;
        Transform tableArt = HW_Obj(table, "Art", Vector2.zero, "hw_nightstand", 4);
        BoxCollider2D tableCol = HW_Box(table, new Vector2(1.5f, 2.35f), new Vector2(0f, -0.3f));
        Transform clock = Child(table, "Clock", new Vector2(0f, 0.02f)).transform;
        clock.localScale = Vector3.one * 0.55f;
        Art(clock, "Art", "alarm_clock", 5, false);
        SpriteRenderer face = Art(clock, "Face", "lock_dot", 6, false, new Vector2(0f, 0.72f)).GetComponent<SpriteRenderer>();
        face.transform.localScale = Vector3.one * 3.4f;
        face.color = new Color(1f, 0.97f, 0.91f);
        Transform hour = Child(clock, "Hour", new Vector2(0f, 0.72f)).transform;
        Bar(hour, "Bar", new Vector2(0f, 0.12f), new Vector2(0.08f, 0.26f), InkColor, 7);
        Transform minute = Child(clock, "Minute", new Vector2(0f, 0.72f)).transform;
        Bar(minute, "Bar", new Vector2(0f, 0.17f), new Vector2(0.05f, 0.36f), InkColor, 7);
        BoxCollider2D clockCol = HW_Box(clock, new Vector2(2.3f, 2.3f), new Vector2(0f, 0.8f));
        SmokeEmitter clockWaves = Emitter(r, "ClockWaves", new Vector2(0.95f, -1.7f), "ring_waves", 3f, 0.7f, 0.4f, 0.05f, 0.4f, Color.white, 9);
        clockWaves.growth = 0.8f;
        clockWaves.spinRange = 0f;
        clockWaves.sideDrift = 0f;
        SmokeEmitter floorWaves = Emitter(r, "FloorWaves", new Vector2(0.95f, -3.4f), "ring_waves", 1.5f, 0.8f, 0.35f, 0.2f, 0.28f, new Color(1f, 1f, 1f, 0.28f), 6);
        floorWaves.growth = 0.6f;
        floorWaves.spinRange = 0f;
        floorWaves.sideDrift = 0.05f;

        // la biglia per terra, vicino al letto
        Transform marble = HW_Obj(r, "Marble", new Vector2(-0.35f, -3.3f), "e40_marble", 8, 0.75f);
        CircleCollider2D marbleCol = marble.gameObject.AddComponent<CircleCollider2D>();
        marbleCol.radius = 0.6f;

        // lo spioncino sul piano di sotto: il telefono del vicino con SNOOZE (nascosto)
        Transform peep = Child(r, "Peephole", new Vector2(0.2f, -0.55f)).transform;
        Art(peep, "Art", "v64_peephole", 20, false);
        Vector2 crackLocal = new Vector2(0.95f - 0.2f, -3.45f + 0.55f);
        for (int i = 1; i <= 4; i++)
        {
            Vector2 at = Vector2.Lerp(crackLocal, new Vector2(0.45f, -1.15f), i / 5f);
            Art(peep, "Dot" + i, "lock_dot", 19, false, at).localScale = Vector3.one * (0.5f + 0.12f * i);
        }
        Transform nPhone = HW_Obj(peep, "Phone", new Vector2(0.25f, 0.08f), "hw_phone", 22, 1.5f);
        TextMesh nScreen = Fit(Label(nPhone, new Vector2(0f, 0.17f), 23, "7:00", 0.014f), 0.3f, 0.12f);
        Transform snooze = HW_Obj(nPhone, "Snooze", new Vector2(0f, -0.1f), "s41i_target", 23, 0.3f);
        Fit(Label(nPhone, new Vector2(0f, -0.27f), 24, "SNOOZE", 0.011f), 0.3f, 0.07f);
        Transform insetMarble = HW_Obj(peep, "Marble", new Vector2(-0.4f, -0.33f), "e40_marble", 23, 0.6f);
        Transform shock = HW_Obj(peep, "Shock", new Vector2(-0.4f, -0.33f), "shock_ring", 24, 1f);
        SmokeEmitter phoneWaves = Emitter(peep, "PhoneWaves", new Vector2(0.25f, 0.62f), "ring_waves", 3f, 0.6f, 0.3f, 0.05f, 0.22f, Color.white, 24);
        phoneWaves.growth = 0.8f;
        phoneWaves.spinRange = 0f;
        phoneWaves.sideDrift = 0f;

        // l'alternativa al microfono (compare solo nell'ultima fase, se serve)
        Transform micBtn = HW_Obj(r, "MicButton", new Vector2(-1.55f, -4.1f), "v64_mic_btn", 30, 1f);
        TextMesh micLabel = Fit(Label(micBtn, new Vector2(0.95f, 0f), 31, "TAP: BANG!", 0.03f), 1.2f, 0.25f);
        micLabel.color = Color.white;
        CircleCollider2D micCol = micBtn.gameObject.AddComponent<CircleCollider2D>();
        micCol.radius = 0.6f;

        HW_Goal(r, "TURN OFF THAT DAMN ALARM!");
        SetField(level, "sleeperArt", sleeper);
        SetField(level, "sleeperAsleep", sprites["kid_bed_sleep"]);
        SetField(level, "sleeperAwake", sprites["kid_bed"]);
        SetField(level, "sleeperHead", head);
        SetField(level, "zzz", zzz);
        SetField(level, "table", table);
        SetField(level, "tableArea", tableCol);
        SetField(level, "tableArt", tableArt);
        SetField(level, "clock", clock);
        SetField(level, "clockArea", clockCol);
        SetField(level, "hourHand", hour);
        SetField(level, "minuteHand", minute);
        SetField(level, "clockWaves", clockWaves);
        SetField(level, "floorWaves", floorWaves);
        SetField(level, "pillow", pillow);
        SetField(level, "pillowArea", pillowCol);
        SetField(level, "marble", marble);
        SetField(level, "marbleArea", marbleCol);
        SetField(level, "crack", crack);
        SetField(level, "crackArea", crackCol);
        SetField(level, "peephole", peep);
        SetField(level, "neighbourPhone", nPhone);
        SetField(level, "neighbourScreen", nScreen);
        SetField(level, "snoozeTarget", snooze);
        SetField(level, "insetMarble", insetMarble);
        SetField(level, "shockRing", shock);
        SetField(level, "phoneWaves", phoneWaves);
        SetField(level, "rooster", rooster);
        SetField(level, "roosterBubble", SmallBubble(r, new Vector2(-1.25f, 2.35f)));
        SetField(level, "micButton", micBtn);
        SetField(level, "micButtonArea", micCol);
        V62_Wire(level, r, new Vector2(0f, 0.95f));
        return SaveLevel(root, "V64NeighboursAlarm");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V64A = new Dictionary<string, string[]>
    {
        { "v64_fingerprint_thief", new[] { "The owner left something useful on the table.",
            "The scanner needs to see the owner's finger, not yours." } },
        { "v64_plastic_fish", new[] { "Real fish don't usually squeak.",
            "Find the valve. Open it. Give your fish some air." } },
        { "v64_neighbours_alarm", new[] { "The floor seems unusually noisy.",
            "Move the table. Something small might fit through that crack." } },
    };
}
#endif
