using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 274 "The Crying Onion" — "Chop the onion without crying!" Una cucina: il cuoco dietro al bancone con gli
    /// occhi lucidi, il tagliere con una grossa cipolla che fuma, il coltello, gli occhialini da nuoto appesi al gancio, il
    /// ventilatore da tavolo, la candela, una fetta di pane e il frigo con un biglietto sotto la calamita.
    /// False piste (nessuna azzera i progressi, niente sconfitta): tagliare subito (strisciata sulla cipolla o coltello
    /// portato sulla cipolla: fontane di lacrime, "Too many feelings.", la cipolla si ricompone), gli occhialini (si
    /// appannano subito: "Now you're crying AND blind."), il ventilatore (le lacrime volano di lato), trattenere il fiato
    /// (dito tenuto sulla bocca: diventa blu e boccheggia), candela e pane in bocca (rimedi della nonna: lacrime appena
    /// più piccole, "Grandma lied."), il cuoco, il coltello, la cipolla toccati.
    /// Indizio: il biglietto sul frigo ("COLD ONIONS DON'T BITE.") e i fumi della cipolla, che si rimpiccioliscono
    /// avvicinandola al frigo (e il cuoco smette di lacrimare quando la cipolla è via).
    /// Soluzione: aprire il frigo, metterci la cipolla, chiudere, aspettare (5 s di brina sullo sportello; o riaprire
    /// dopo 3 s), riprenderla fredda (niente fumi) e tagliarla -> cubetti ordinati, occhi asciutti, "COLD ONIONS DON'T
    /// CRY.", "NEITHER DO YOU.", vittoria. Fuori dal frigo la cipolla fredda torna tiepida dopo 30 s (si rifà).
    /// Niente microfono. La logica è in Gotcha.Levels.V64.CryingOnionLogic (senza Unity).
    /// Tasti di prova nell'Editor: C taglio, F sportello del frigo, I cipolla nel frigo (aperto), B cipolla sul tagliere,
    /// G occhialini, N ventilatore, H fiato trattenuto (fino al blu), K candela, R pane, T biglietto, X cuoco.
    /// </summary>
    public class V74CryingOnionLevel : V62Level
    {
        [Header("Frigo")]
        [SerializeField] Transform fridge;
        [SerializeField] Collider2D fridgeArea;         // lo sportello (tocco: apre/chiude)
        [SerializeField] Collider2D fridgeDropArea;     // dove si lascia la cipolla (generosa, anche un po' fuori)
        [SerializeField] Transform fridgeSpot;          // il posto libero sul ripiano di mezzo
        [SerializeField] Transform doorClosed;
        [SerializeField] Transform doorOpen;
        [SerializeField] SpriteRenderer doorFrost;      // la brina sullo sportello: il "timer" del freddo
        [SerializeField] Transform note;
        [SerializeField] Collider2D noteArea;
        [SerializeField] Transform[] flakes;            // le stelline di ghiaccio sul freezer, una al secondo
        [Header("Cuoco")]
        [SerializeField] Transform cook;
        [SerializeField] Transform head;
        [SerializeField] SpriteRenderer headArt;        // si tinge di blu col fiato trattenuto
        [SerializeField] Transform eyes;
        [SerializeField] Transform eyesWatery;
        [SerializeField] Transform eyesDry;
        [SerializeField] Transform eyesCry;
        [SerializeField] Transform eyesHappy;
        [SerializeField] Transform mouth;
        [SerializeField] Transform mouthWobble;
        [SerializeField] Transform mouthWail;
        [SerializeField] Transform mouthPuff;
        [SerializeField] Transform mouthSmile;
        [SerializeField] Transform mouthGasp;
        [SerializeField] Transform breadBite;           // la fetta di pane in bocca
        [SerializeField] Collider2D cookArea;
        [SerializeField] Collider2D mouthArea;
        [Header("Occhialini")]
        [SerializeField] Transform goggles;
        [SerializeField] Transform gogglesStrap;
        [SerializeField] SpriteRenderer gogglesFog;
        [SerializeField] Collider2D gogglesArea;
        [SerializeField] Transform faceSpot;
        [Header("Ventilatore e candela")]
        [SerializeField] Transform fan;
        [SerializeField] Collider2D fanArea;
        [SerializeField] Transform fanHead;
        [SerializeField] Transform fanBlades;
        [SerializeField] SpriteRenderer wind;
        [SerializeField] Transform candle;
        [SerializeField] Collider2D candleArea;
        [SerializeField] Transform flame;
        [Header("Tagliere e cipolla")]
        [SerializeField] Collider2D boardArea;          // dove una strisciata che passa sulla cipolla la taglia
        [SerializeField] Transform onion;
        [SerializeField] Transform onionHome;
        [SerializeField] Transform onionWhole;
        [SerializeField] SpriteRenderer onionFrost;
        [SerializeField] Transform halfLeft;
        [SerializeField] Transform halfRight;
        [SerializeField] Collider2D onionArea;
        [SerializeField] Transform diced;
        [SerializeField] SpriteRenderer[] fumes;
        [SerializeField] Transform bread;
        [SerializeField] Collider2D breadArea;
        [SerializeField] Transform knife;
        [SerializeField] Collider2D knifeArea;
        [SerializeField] Transform knifeTip;
        [SerializeField] SpriteRenderer[] tears;
        [Header("Festa")]
        [SerializeField] SpriteRenderer glow;
        [SerializeField] Transform sparkle;
        [SerializeField] SmokeEmitter coldPuffs;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float TapSlop = 0.3f, TapTime = 0.6f, HoldStart = 0.3f, FlickTime = 0.32f, FlickLength = 0.55f, SwipeLength = 0.35f;
        const float TearGravity = 7.5f, WindPush = 11f, TearFloor = -2.5f, TearLife = 1.8f, WellEvery = 3.2f;
        const int OnionBoardOrder = 18, OnionFridgeOrder = 12, HeldOrder = 40;
        static readonly Color BlueFace = new Color(0.55f, 0.72f, 1f);

        static readonly string[] TearLines = { "Too many feelings.", "Sob. SOB. The onion wins.", "It's not you. It's the onion.", "Chopping cancelled. Crying instead." };
        static readonly string[] SidewaysLines = { "Sideways tears. Still tears.", "The fan just spreads the sadness.", "Now the kitchen is crying too." };
        static readonly string[] GrandmaLines = { "Grandma lied.", "Slightly fewer tears. Grandma still lied.", "Folk remedy: zero stars." };
        static readonly string[] GogglesLines = { "Now you're crying AND blind.", "Fogged up. Instantly.", "He can't see the onion. Or anything." };
        static readonly string[] FanOnLines = { "Whoosh. A nice breeze.", "Fan on. The onion doesn't care." };
        static readonly string[] FanOffLines = { "Fan off.", "Click. Silence." };
        static readonly string[] BlueLines = { "GASP! Breathing is not optional.", "Blue is not his colour.", "GASP! Lungs: 1. Plan: 0." };
        static readonly string[] GaspLines = { "Pfff. That just delayed the tears.", "Phew. Still has to breathe near it." };
        static readonly string[] CandleLines = { "Still lit. Still an onion.", "The candle is doing its best.", "Flickering. Not helping." };
        static readonly string[] WateryCookLines = { "Sniff. Those fumes sting.", "His eyes are already watering.", "He hasn't even started chopping." };
        static readonly string[] DryCookLines = { "Ahh. Dry eyes. For now.", "Sniff... better without the onion." };
        static readonly string[] KnifeLines = { "Sharp and ready. Swipe it across the onion.", "The knife is fine. The onion is the problem." };
        static readonly string[] WarmOnionLines = { "A big, angry onion.", "Even looking at it stings.", "It's fuming. Literally." };
        static readonly string[] ColdOnionLines = { "Frosty. Not a single fume.", "Cold and calm. Chop it!" };
        static readonly string[] EmptyFridgeLines = { "Cold in here.", "Milk, jam, cheese. And a lot of cold.", "Brrr." };
        static readonly string[] EarlyLines = { "Too soon. It's still warm.", "Patience. Onions chill slowly.", "Still room temperature." };
        static readonly string[] AirLines = { "You're chopping air. The onion is in the fridge.", "Nothing on the board." };
        static readonly string[] NoteLines = { "The note says: \"COLD ONIONS DON'T BITE.\"", "\"COLD ONIONS DON'T BITE.\" Hmm." };
        static readonly string[] BoardLines = { "Swipe across the onion to chop it.", "Swipe over the onion. If you dare." };

        enum Kind { None, Onion, Knife, Bread, Goggles, Note, Fridge, Mouth, Cook, Fan, Candle, Swipe }

        sealed class Grip
        {
            public Kind kind;
            public Vector2 start, last;
            public float t0, path;
            public bool moved, done, holding, fromBoard;
        }

        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        readonly CryingOnionLogic logic = new CryingOnionLogic();
        Camera cam;
        HwToast toast;
        HwDragger dragger;
        HwDragger.Item onionItem, knifeItem, breadItem, gogglesItem;
        Grip grip;
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        SpriteRenderer wholeArt, halfLeftArt, halfRightArt;
        Vector3 knifeHome, breadHome, gogglesHome, halfLeftPos, halfRightPos, noteScale, gogglesScale;
        Quaternion knifeRot, breadRot;
        Vector2[] tearVel;
        float[] tearAge;
        int nextTear;
        float frostShown, onionFrostShown, windShown, wellAt, shiverAt;
        int tearLines, sidewaysLines, grandmaLines, gogglesLines, fanOnLines, fanOffLines, blueLines, gaspLines, candleLines;
        int cookLines, knifeLines, onionLines, emptyLines, earlyLines, airLines, noteLines, boardLines;
        bool busy, finishing, captured, healedOnce, fridgeTold, dryTold;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            logic.Reset();
            ResetV62();
            grip = null;
            busy = finishing = healedOnce = fridgeTold = dryTold = false;
            tearLines = sidewaysLines = grandmaLines = gogglesLines = fanOnLines = fanOffLines = blueLines = gaspLines = candleLines = 0;
            cookLines = knifeLines = onionLines = emptyLines = earlyLines = airLines = noteLines = boardLines = 0;
            toast = new HwToast(infoText, infoHolder);
            if (!captured)
            {
                captured = true;
                wholeArt = onionWhole.GetComponent<SpriteRenderer>();
                halfLeftArt = halfLeft.GetComponentInChildren<SpriteRenderer>(true);
                halfRightArt = halfRight.GetComponentInChildren<SpriteRenderer>(true);
                halfLeftPos = halfLeft.localPosition;
                halfRightPos = halfRight.localPosition;
                knifeHome = knife.position;
                knifeRot = knife.localRotation;
                breadHome = bread.position;
                breadRot = bread.localRotation;
                gogglesHome = goggles.position;
                gogglesScale = goggles.localScale;
                noteScale = note.localScale;
                tearVel = new Vector2[tears.Length];
                tearAge = new float[tears.Length];
            }
            dragger = new HwDragger(this);
            onionItem = dragger.Add(onion, onionArea, 20);
            knifeItem = dragger.Add(knife, knifeArea, 15);
            breadItem = dragger.Add(bread, breadArea, 12);
            gogglesItem = dragger.Add(goggles, gogglesArea, 12);
            wellAt = Time.time + 1.5f;
            shiverAt = 0f;
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            // frigo
            HalloweenKit.Show(doorClosed, !logic.FridgeOpen);
            HalloweenKit.Show(doorOpen, logic.FridgeOpen);
            note.localScale = noteScale;
            frostShown = FrostTarget();
            HalloweenKit.SetAlpha(doorFrost, frostShown);
            Flakes();
            if (fridge != null) fridge.localRotation = Quaternion.identity;
            // cipolla
            OnionRest();
            HalloweenKit.Show(diced, false);
            onionFrostShown = OnionFrostTarget();
            HalloweenKit.SetAlpha(onionFrost, onionFrostShown);
            // coltello, pane, occhialini
            dragger.Stop(knifeItem);
            knife.position = knifeHome;
            knife.localRotation = knifeRot;
            knifeItem.Home = knifeHome;
            knifeItem.Enabled = true;
            dragger.Stop(breadItem);
            bread.position = breadHome;
            bread.localRotation = breadRot;
            breadItem.Home = breadHome;
            HalloweenKit.Show(bread, !logic.BreadInMouth);
            breadItem.Enabled = !logic.BreadInMouth;
            HalloweenKit.Show(breadBite, logic.BreadInMouth);
            dragger.Stop(gogglesItem);
            goggles.position = gogglesHome;
            goggles.localScale = gogglesScale;
            gogglesItem.Home = gogglesHome;
            gogglesItem.Enabled = true;
            HalloweenKit.Show(gogglesStrap, true);
            HalloweenKit.SetAlpha(gogglesFog, 0f);
            if (dragger.Held != null) dragger.Release();
            // ventilatore, candela
            windShown = logic.FanOn ? 0.6f : 0f;
            HalloweenKit.SetAlpha(wind, windShown);
            HalloweenKit.Show(flame, logic.CandleLit);
            // cuoco
            if (headArt != null) headArt.color = Color.white;
            head.localRotation = Quaternion.identity;
            RestFace();
            // lacrime e festa
            for (int i = 0; i < tears.Length; i++) HalloweenKit.Show(tears[i], false);
            HalloweenKit.SetAlpha(glow, 0f);
            HalloweenKit.Show(sparkle, false);
            Fumes(true);
            ColdPuffs();
        }

        void OnionRest()
        {
            dragger.Stop(onionItem);
            onion.position = OnionSpotPos();
            onion.localRotation = Quaternion.identity;
            HalloweenKit.Show(onion, true);
            HalloweenKit.Show(onionWhole, true);
            HalloweenKit.Show(halfLeft, false);
            HalloweenKit.Show(halfRight, false);
            halfLeft.localPosition = halfLeftPos;
            halfRight.localPosition = halfRightPos;
            OnionOrder(logic.Spot == OnionSpot.Fridge ? OnionFridgeOrder : OnionBoardOrder);
            onionItem.Home = OnionSpotPos();
            onionItem.Enabled = logic.CanGrabOnion;
        }

        Vector3 OnionSpotPos()
        {
            Vector3 p = logic.Spot == OnionSpot.Fridge ? fridgeSpot.position : onionHome.position;
            return new Vector3(p.x, p.y, onion.position.z);
        }

        void OnionOrder(int order)
        {
            if (wholeArt != null) wholeArt.sortingOrder = order;
            if (halfLeftArt != null) halfLeftArt.sortingOrder = order;
            if (halfRightArt != null) halfRightArt.sortingOrder = order;
            if (onionFrost != null) onionFrost.sortingOrder = order + 1;
        }

        /// <summary>La faccia a riposo: occhi lucidi con la cipolla tiepida sul tagliere, asciutti se è via o fredda.</summary>
        void RestFace()
        {
            bool watery = logic.EyesWatery;
            ShowEyes(watery ? eyesWatery : eyesDry);
            ShowMouth(mouthWobble);
            HalloweenKit.Show(breadBite, logic.BreadInMouth);
        }

        void ShowEyes(Transform on)
        {
            foreach (Transform t in new[] { eyesWatery, eyesDry, eyesCry, eyesHappy }) HalloweenKit.Show(t, t == on);
        }

        void ShowMouth(Transform on)
        {
            foreach (Transform t in new[] { mouthWobble, mouthWail, mouthPuff, mouthSmile, mouthGasp }) HalloweenKit.Show(t, t == on);
        }

        float FrostTarget() => logic.Spot == OnionSpot.Fridge && !logic.FridgeOpen ? 0.95f * logic.ChillProgress : 0f;

        float OnionFrostTarget()
        {
            if (!logic.Cold) return 0f;
            if (logic.Spot == OnionSpot.Fridge) return 1f;
            return Mathf.Clamp(logic.ColdLeft / CryingOnionLogic.ColdLasts * 1.6f, 0.3f, 1f);
        }

        void Flakes()
        {
            bool show = logic.Spot == OnionSpot.Fridge && !logic.FridgeOpen;
            int n = show ? Mathf.FloorToInt(logic.ChillProgress * flakes.Length + 0.001f) : 0;
            for (int i = 0; i < flakes.Length; i++)
            {
                bool on = i < n;
                if (flakes[i] != null && flakes[i].gameObject.activeSelf != on)
                {
                    flakes[i].gameObject.SetActive(on);
                    if (on && !finishing) { FxPop(flakes[i], 1.6f, 0.25f); V74CryingOnionSound.Play(this, "frost", 0.25f); }
                }
            }
        }

        void ColdPuffs()
        {
            if (coldPuffs == null) return;
            bool hidden = logic.Spot == OnionSpot.Fridge && !logic.FridgeOpen;
            bool on = logic.Cold && !hidden && !finishing && onionWhole.gameObject.activeInHierarchy;
            if (on && !coldPuffs.IsPlaying) coldPuffs.Play();
            if (!on && coldPuffs.IsPlaying) coldPuffs.Stop();
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: le animazioni si fermano, si torna a uno stato valido
            CancelGrip();
            busy = false;
            StopFx();
            if (coldPuffs != null) coldPuffs.Stop();
            if (captured && !finishing && dragger != null && !IsFinished) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || dragger == null) return;
            // spento a metà della festa: si rifà il finale da capo (la cipolla fredda è già sotto il coltello)
            if (logic.Stage == OnionStage.Chopping) { finishing = false; StartCoroutine(Finale(false)); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrip(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrip(); }

        /// <summary>Il dito sparisce (pausa, focus perso, animazione che parte): l'oggetto in mano torna a posto, il fiato riprende.</summary>
        void CancelGrip()
        {
            if (grip != null && grip.holding) BreathVisual(0f, false);
            grip = null;
            if (dragger != null && dragger.Held != null)
            {
                HwDragger.Item it = dragger.Release();
                SendHome(it, 0.2f);
            }
        }

        void SendHome(HwDragger.Item it, float seconds)
        {
            if (it == null) return;
            if (it == onionItem)
            {
                OnionOrder(logic.Spot == OnionSpot.Fridge ? OnionFridgeOrder : OnionBoardOrder);
                it.Home = OnionSpotPos();
            }
            if (it == knifeItem) knife.localRotation = knifeRot;
            dragger.SendTo(it, it.Home, seconds, 0.15f);
        }

        void Update()
        {
            toast?.Tick();
            if (IsFinished || toast == null) return;
            TickTears();
            if (finishing) return;
            if (Time.timeScale <= 0f) { CancelGrip(); return; }   // pausa o pubblicità: niente input
            switch (logic.Tick(Time.deltaTime))
            {
                case OnionTick.Chilled: Chilled(); break;
                case OnionTick.WarmedUp: WarmedUp(); break;
            }
            Ambient();
            if (busy) { CancelGrip(); return; }
#if UNITY_EDITOR
            EditorKeys();
            if (busy || finishing) return;
#endif
            ReadPointer();
        }

        /// <summary>Le cose vive: fumi, ventilatore, brina, candela, la lacrima che spunta ogni tanto.</summary>
        void Ambient()
        {
            Fumes(false);
            if (logic.FanOn && fanBlades != null) fanBlades.localRotation *= Quaternion.Euler(0f, 0f, -900f * Time.deltaTime);
            if (fanHead != null) fanHead.localRotation = Quaternion.Euler(0f, 0f, logic.FanOn ? 6f * Mathf.Sin(Time.time * 2.5f) : 0f);
            // il cuoco respira (col fiato trattenuto sta fermo)
            if (cook != null) cook.localScale = new Vector3(1f, grip != null && grip.holding ? 1f : 1f + 0.012f * Mathf.Sin(Time.time * 2.2f), 1f);
            windShown = Mathf.MoveTowards(windShown, logic.FanOn ? 0.55f + 0.2f * Mathf.Sin(Time.time * 9f) : 0f, Time.deltaTime * 3f);
            HalloweenKit.SetAlpha(wind, windShown);
            frostShown = Mathf.MoveTowards(frostShown, FrostTarget(), Time.deltaTime * 2f);
            HalloweenKit.SetAlpha(doorFrost, frostShown);
            onionFrostShown = Mathf.MoveTowards(onionFrostShown, OnionFrostTarget(), Time.deltaTime * 2f);
            HalloweenKit.SetAlpha(onionFrost, onionFrostShown);
            Flakes();
            ColdPuffs();
            if (logic.CandleLit && flame != null) flame.localScale = new Vector3(1f + 0.08f * Mathf.Sin(Time.time * 13f), 1f + 0.12f * Mathf.Sin(Time.time * 17f + 1f), 1f);
            // il frigo trema di freddo mentre raffredda la cipolla
            if (logic.Chilling && Time.time >= shiverAt)
            {
                shiverAt = Time.time + 1f;
                FxShake(fridge, 0.025f, 0.25f);
                V74CryingOnionSound.Play(this, "hum", 0.25f);
            }
            // gli occhi lucidi: ogni tanto scappa una lacrima
            if (logic.EyesWatery && !busy && Time.time >= wellAt)
            {
                wellAt = Time.time + WellEvery + Random.Range(0f, 1.5f);
                float s = Random.value < 0.5f ? -1f : 1f;
                SpawnTear(EyePos(s) + new Vector3(s * 0.08f, -0.12f, 0f), new Vector2(s * 0.15f, 0f), 0.8f);
            }
        }

        /// <summary>I fumi della cipolla: salgono verso il cuoco, più piccoli vicino al frigo, spariti se è fredda o chiusa in frigo.</summary>
        void Fumes(bool snap)
        {
            if (fumes == null || onion == null) return;
            bool hidden = (!onionWhole.gameObject.activeInHierarchy && !halfLeft.gameObject.activeInHierarchy)
                          || (logic.Spot == OnionSpot.Fridge && !logic.FridgeOpen && dragger.Held != onionItem)
                          || finishing;
            float strength = CryingOnionLogic.FumeStrength(logic.Cold, hidden, Vector2.Distance(onion.position, fridgeSpot.position));
            float drift = logic.FanOn ? -0.9f : 0.15f;
            for (int i = 0; i < fumes.Length; i++)
            {
                SpriteRenderer f = fumes[i];
                if (f == null) continue;
                float t = Mathf.Repeat(Time.time * 0.42f + i / (float)fumes.Length, 1f);
                Vector3 o = onion.position;
                float x = o.x + (i - 1) * 0.14f + drift * t + 0.06f * Mathf.Sin(Time.time * 3f + i * 2f);
                f.transform.position = new Vector3(x, o.y + 0.45f + 1.05f * t, f.transform.position.z);
                f.transform.localScale = Vector3.one * (0.75f + 0.55f * t) * (0.5f + 0.5f * strength) * 1.15f;
                f.transform.localRotation = Quaternion.Euler(0f, 0f, logic.FanOn ? 50f * t : 8f * Mathf.Sin(Time.time * 2f + i));
                float a = strength * 0.7f * Mathf.Sin(t * Mathf.PI);
                f.sortingOrder = dragger != null && dragger.Held == onionItem ? HeldOrder + 2 : 20;
                HalloweenKit.SetAlpha(f, snap ? a : Mathf.MoveTowards(f.color.a, a, Time.deltaTime * 4f));
            }
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73) ----------

        void FxPop(Transform t, float peak, float seconds) => Fx(0, t, HalloweenKit.Pop(t, peak, seconds));
        void FxWiggle(Transform t, float degrees, float seconds, float speed) => Fx(1, t, HalloweenKit.Wiggle(t, degrees, seconds, speed));
        void FxShake(Transform t, float amplitude, float seconds) => Fx(2, t, HalloweenKit.Shake(t, amplitude, seconds));

        void Fx(int kind, Transform t, IEnumerator e)
        {
            if (t == null || !isActiveAndEnabled) return;
            Dictionary<Transform, FxSlot> d = fxSlots[kind];
            if (d.TryGetValue(t, out FxSlot slot) && slot.co != null)
            {
                StopCoroutine(slot.co);
                Restore(kind, t, slot);
            }
            else
            {
                if (slot == null) { slot = new FxSlot(); d[t] = slot; }
                slot.v = kind == 0 ? t.localScale : t.localPosition;
                slot.q = t.localRotation;
            }
            slot.co = StartCoroutine(FxRun(slot, e));
        }

        static IEnumerator FxRun(FxSlot slot, IEnumerator e)
        {
            yield return e;
            slot.co = null;
        }

        static void Restore(int kind, Transform t, FxSlot slot)
        {
            if (t == null) return;
            if (kind == 0) t.localScale = slot.v;
            else if (kind == 1) t.localRotation = slot.q;
            else t.localPosition = slot.v;
        }

        void StopFx()
        {
            for (int kind = 0; kind < fxSlots.Length; kind++)
            {
                foreach (KeyValuePair<Transform, FxSlot> kv in fxSlots[kind])
                    if (kv.Value.co != null) { StopCoroutine(kv.Value.co); Restore(kind, kv.Key, kv.Value); kv.Value.co = null; }
                fxSlots[kind].Clear();
            }
        }

        void StopFxOn(Transform t)
        {
            for (int kind = 0; kind < fxSlots.Length; kind++)
                if (fxSlots[kind].TryGetValue(t, out FxSlot slot))
                {
                    if (slot.co != null) { StopCoroutine(slot.co); Restore(kind, t, slot); }
                    fxSlots[kind].Remove(t);
                }
        }

        // ---------- Lacrime (scorta fissa di gocce: archi con la gravità, il vento del ventilatore le spinge a sinistra) ----------

        Vector3 EyePos(float side) => eyes.TransformPoint(new Vector3(side * 0.19f, 0f, 0f));

        void SpawnTear(Vector3 pos, Vector2 vel, float scale)
        {
            if (tears == null || tears.Length == 0) return;
            int i = -1;
            for (int k = 0; k < tears.Length; k++)
            {
                int j = (nextTear + k) % tears.Length;
                if (tears[j] != null && !tears[j].gameObject.activeSelf) { i = j; break; }
            }
            if (i < 0) i = nextTear % tears.Length;   // tutte in volo: si ricicla la più vecchia
            nextTear = i + 1;
            SpriteRenderer t = tears[i];
            if (t == null) return;
            t.transform.position = new Vector3(pos.x, pos.y, t.transform.position.z);
            t.transform.localScale = Vector3.one * 1.4f * scale;
            HalloweenKit.SetAlpha(t, 1f);
            t.gameObject.SetActive(true);
            tearVel[i] = vel;
            tearAge[i] = 0f;
        }

        void TickTears()
        {
            if (tears == null || tearVel == null) return;
            float dt = Time.deltaTime;
            for (int i = 0; i < tears.Length; i++)
            {
                SpriteRenderer t = tears[i];
                if (t == null || !t.gameObject.activeSelf) continue;
                Vector2 v = tearVel[i];
                v.y -= TearGravity * dt;
                if (logic.FanOn && !finishing) { v.x -= WindPush * dt; v.y += TearGravity * 0.55f * dt; }
                tearVel[i] = v;
                tearAge[i] += dt;
                Vector3 p = t.transform.position + (Vector3)(v * dt);
                t.transform.position = p;
                // la punta della goccia guarda da dove viene
                t.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(-v.y, -v.x) * Mathf.Rad2Deg - 90f);
                if (tearAge[i] > TearLife * 0.7f) HalloweenKit.SetAlpha(t, Mathf.Clamp01((TearLife - tearAge[i]) / (TearLife * 0.3f)));
                if (p.y < TearFloor || p.x < -3.2f || p.x > 3.2f || tearAge[i] >= TearLife) t.gameObject.SetActive(false);
            }
        }

        void StopTears()
        {
            if (tears == null) return;
            for (int i = 0; i < tears.Length; i++) HalloweenKit.Show(tears[i], false);
        }

        // ---------- Dito ----------

        void ReadPointer()
        {
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (grip != null) End(grip);
                return;
            }
            if (began) { if (grip != null) End(grip); grip = Begin(w); }
            if (grip == null) return;
            grip.path += Vector2.Distance(w, grip.last);
            grip.last = w;
            if (Vector2.Distance(w, grip.start) > TapSlop) grip.moved = true;
            Move(grip, w);
            if (ended && grip != null) End(grip);
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, t0 = Time.time, kind = Kind.Swipe };
            if (busy || finishing) { g.kind = Kind.None; return g; }
            onionItem.Enabled = logic.CanGrabOnion;
            HwDragger.Item it = dragger.TryPick(w);
            if (it != null)
            {
                StopFxOn(it.T);
                if (it == onionItem)
                {
                    g.kind = Kind.Onion;
                    g.fromBoard = logic.Spot == OnionSpot.Board;
                    OnionOrder(HeldOrder);
                }
                else if (it == knifeItem) g.kind = Kind.Knife;
                else if (it == breadItem) g.kind = Kind.Bread;
                else g.kind = Kind.Goggles;
                return g;
            }
            if (!logic.FridgeOpen && noteArea.OverlapPoint(w)) { g.kind = Kind.Note; return g; }
            if (fridgeArea.OverlapPoint(w)) { g.kind = Kind.Fridge; return g; }
            if (candleArea.OverlapPoint(w)) { g.kind = Kind.Candle; return g; }
            if (fanArea.OverlapPoint(w)) { g.kind = Kind.Fan; return g; }
            if (mouthArea.OverlapPoint(w)) { g.kind = Kind.Mouth; return g; }
            if (cookArea.OverlapPoint(w) && !boardArea.OverlapPoint(w)) { g.kind = Kind.Cook; return g; }
            return g;   // tutto il resto: una strisciata (se passa sulla cipolla, la taglia)
        }

        void Move(Grip g, Vector2 w)
        {
            switch (g.kind)
            {
                case Kind.Onion:
                case Kind.Bread:
                case Kind.Goggles:
                    if (g.moved) dragger.MoveHeld(w);
                    break;
                case Kind.Knife:
                    if (!g.moved) break;
                    dragger.MoveHeld(w);
                    if (g.done) break;
                    bool onOnion = OnionOnBoard() && (onionArea.OverlapPoint(knifeTip.position) || onionArea.OverlapPoint(knife.position) || onionArea.OverlapPoint(w));
                    bool onBoard = !OnionOnBoard() && logic.Spot == OnionSpot.Fridge && (boardArea.OverlapPoint(knifeTip.position) || boardArea.OverlapPoint(w));
                    if (onOnion || onBoard) { g.done = true; grip = null; dragger.Release(); Chop(); }
                    break;
                case Kind.Swipe:
                    if (!g.done && g.moved && g.path >= SwipeLength && OnionOnBoard() && onionArea.OverlapPoint(w))
                    {
                        g.done = true;
                        grip = null;
                        Chop();
                    }
                    break;
                case Kind.Mouth:
                    if (g.moved && !g.holding) { g.kind = Kind.Swipe; break; }
                    float held = Time.time - g.t0;
                    if (!g.holding && held >= HoldStart && !g.moved) { g.holding = true; BreathVisual(0f, true); }
                    if (!g.holding) break;
                    float k = (held - HoldStart) / CryingOnionLogic.BlueAfter;
                    BreathVisual(k, true);
                    if (k >= 1f) { grip = null; Breath(held - HoldStart); }
                    break;
            }
        }

        void End(Grip g)
        {
            grip = null;
            bool tap = !g.moved && Time.time - g.t0 <= TapTime + 0.4f;
            switch (g.kind)
            {
                case Kind.Onion: DropOnion(g, tap); break;
                case Kind.Knife:
                    if (dragger.Held == knifeItem) dragger.Release();
                    SendHome(knifeItem, 0.25f);
                    if (tap) TapKnife();
                    break;
                case Kind.Bread:
                    if (dragger.Held == breadItem) dragger.Release();
                    if (tap || cookArea.OverlapPoint(bread.position) || cookArea.OverlapPoint(g.last)) Bread();
                    else SendHome(breadItem, 0.25f);
                    break;
                case Kind.Goggles:
                    if (dragger.Held == gogglesItem) dragger.Release();
                    if (tap || cookArea.OverlapPoint(goggles.position) || cookArea.OverlapPoint(g.last)) Goggles();
                    else SendHome(gogglesItem, 0.25f);
                    break;
                case Kind.Note: if (tap) ReadNote(); break;
                case Kind.Fridge: if (tap) TapFridge(); break;
                case Kind.Mouth:
                    if (g.holding) Breath(Time.time - g.t0 - HoldStart);
                    else if (tap) TapCook();
                    break;
                case Kind.Cook: if (tap) TapCook(); break;
                case Kind.Fan: if (tap) ToggleFan(); break;
                case Kind.Candle: if (tap) LightCandle(); break;
                case Kind.Swipe:
                    if (tap && boardArea.OverlapPoint(g.start)) toast.Show(BoardLines[boardLines++ % BoardLines.Length], 1.6f);
                    break;
            }
        }

        bool OnionOnBoard() => logic.Spot == OnionSpot.Board && dragger.Held != onionItem && onionWhole.gameObject.activeInHierarchy;

        // ---------- La cipolla ----------

        void DropOnion(Grip g, bool tap)
        {
            if (dragger.Held == onionItem) dragger.Release();
            float dt = Time.time - g.t0;
            bool overFridge = fridgeDropArea.OverlapPoint(onion.position) || fridgeDropArea.OverlapPoint(g.last);
            if (tap)
            {
                SendHome(onionItem, 0.12f);
                TapOnion();
                return;
            }
            // una strisciata veloce partita dalla cipolla sul tagliere (e non verso il frigo aperto) è un colpo di coltello
            if (g.fromBoard && dt <= FlickTime && g.path >= FlickLength && !(overFridge && logic.FridgeOpen))
            {
                dragger.Stop(onionItem);
                onion.position = OnionSpotPos();
                OnionOrder(OnionBoardOrder);
                Chop();
                return;
            }
            OnionDropResult r = logic.DropOnion(overFridge && logic.FridgeOpen);
            switch (r)
            {
                case OnionDropResult.None: SendHome(onionItem, 0.25f); return;
                case OnionDropResult.InFridge:
                    OnionOrder(OnionFridgeOrder);
                    onionItem.Home = OnionSpotPos();
                    dragger.SendTo(onionItem, onionItem.Home, 0.15f, 0.05f);
                    V74CryingOnionSound.Play(this, "boing", 0.4f);
                    Haptics.Light();
                    if (logic.Cold) toast.Show("Back in the cold. Still frosty.", 1.6f);
                    else if (!fridgeTold) { fridgeTold = true; toast.Show("In you go. Next to the cheese.", 1.8f); }
                    RestFace();
                    if (!dryTold) { dryTold = true; StartCoroutine(Relief()); }
                    return;
                case OnionDropResult.OnBoard:
                    OnionOrder(OnionBoardOrder);
                    onionItem.Home = OnionSpotPos();
                    dragger.SendTo(onionItem, onionItem.Home, 0.25f, 0.15f);
                    if (overFridge && !logic.FridgeOpen) toast.Show("The fridge is closed. Open it first.", 1.7f);
                    else if (logic.Cold) toast.Show(ColdOnionLines[onionLines++ % ColdOnionLines.Length], 1.6f);
                    RestFace();
                    return;
            }
        }

        /// <summary>La prima volta che la cipolla sparisce nel frigo il cuoco si asciuga gli occhi (un altro indizio).</summary>
        IEnumerator Relief()
        {
            yield return new WaitForSeconds(0.9f);
            if (busy || finishing || logic.EyesWatery) yield break;
            FxPop(eyes, 1.12f, 0.3f);
            toast.Show("Ahh. His eyes stopped stinging.", 1.8f);
        }

        void TapOnion()
        {
            FxWiggle(onion, 8f, 0.4f, 26f);
            if (logic.Cold) toast.Show(ColdOnionLines[onionLines++ % ColdOnionLines.Length], 1.6f);
            else toast.Show(WarmOnionLines[onionLines++ % WarmOnionLines.Length], 1.6f);
        }

        // ---------- Il coltello ----------

        void TapKnife()
        {
            if (!logic.TapKnife()) return;
            FxWiggle(knife, 6f, 0.35f, 30f);
            Sfx.Play(this, "tick", 0.5f);
            toast.Show(KnifeLines[knifeLines++ % KnifeLines.Length], 1.9f);
        }

        void Chop()
        {
            OnionChopResult r = logic.Chop();
            switch (r)
            {
                case OnionChopResult.None: return;
                case OnionChopResult.NoOnion: StartCoroutine(AirChop()); return;
                case OnionChopResult.Tears: StartCoroutine(Cry()); return;
                case OnionChopResult.Clean: StartCoroutine(Finale(true)); return;
            }
        }

        /// <summary>Il coltello va sul tagliere vuoto e taglia l'aria.</summary>
        IEnumerator AirChop()
        {
            busy = true;
            CancelGrip();
            StopFxOn(knife);
            Vector3 board = onionHome.position;
            knife.localRotation = Quaternion.Euler(0f, 0f, 200f);
            yield return HalloweenKit.Move(knife, board + new Vector3(0.25f, 0.4f, 0f), 0.12f);
            yield return HalloweenKit.Move(knife, board + new Vector3(0.2f, -0.05f, 0f), 0.07f);
            V74CryingOnionSound.Play(this, "chop", 0.5f);
            toast.Show(AirLines[airLines++ % AirLines.Length], 1.8f);
            yield return HalloweenKit.Move(knife, knifeHome, 0.25f, 0.2f);
            knife.localRotation = knifeRot;
            busy = false;
        }

        /// <summary>
        /// La cipolla tiepida: un colpo di coltello, la cipolla si apre, il cuoco esplode in fontane di lacrime (di lato col
        /// ventilatore, un po' meno coi rimedi della nonna), il taglio si ferma e la cipolla si ricompone da sola.
        /// </summary>
        IEnumerator Cry()
        {
            busy = true;
            CancelGrip();
            StopFxOn(knife);
            StopFxOn(onion);
            bool fanOn = logic.FanOn, remedy = logic.RemedyActive;
            Vector3 o = onion.position;
            knife.localRotation = Quaternion.Euler(0f, 0f, 200f);
            yield return HalloweenKit.Move(knife, o + new Vector3(0.3f, 0.5f, 0f), 0.12f);
            yield return HalloweenKit.Move(knife, o + new Vector3(0.2f, 0.05f, 0f), 0.07f);
            V74CryingOnionSound.Play(this, "chop", 0.7f);
            Haptics.Light();
            HalloweenKit.Show(onionWhole, false);
            HalloweenKit.Show(halfLeft, true);
            HalloweenKit.Show(halfRight, true);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f)
            {
                halfLeft.localPosition = halfLeftPos + new Vector3(-0.12f * k, 0f, 0f);
                halfRight.localPosition = halfRightPos + new Vector3(0.12f * k, 0f, 0f);
                yield return null;
            }
            // le lacrime
            ShowEyes(eyesCry);
            if (!logic.BreadInMouth) ShowMouth(mouthWail);
            V74CryingOnionSound.Play(this, "sob", 0.7f);
            Haptics.Medium();
            FxShake(head, 0.03f, 1.2f);
            string line = remedy ? GrandmaLines[grandmaLines++ % GrandmaLines.Length]
                        : fanOn ? SidewaysLines[sidewaysLines++ % SidewaysLines.Length]
                        : TearLines[tearLines++ % TearLines.Length];
            toast.Show(line, 2f);
            StartCoroutine(HalloweenKit.Move(knife, knifeHome, 0.35f, 0.25f));
            float every = remedy ? 0.11f : 0.055f, scale = remedy ? 0.75f : 1f;
            float next = 0f;
            for (float t = 0f; t < 1.3f; t += Time.deltaTime)
            {
                if (t >= next)
                {
                    next += every;
                    foreach (float s in new[] { -1f, 1f })
                        SpawnTear(EyePos(s) + new Vector3(s * 0.1f, -0.05f, 0f),
                                  new Vector2(s * Random.Range(0.9f, 1.7f) * (remedy ? 0.7f : 1f), Random.Range(2.2f, 3.2f) * (remedy ? 0.75f : 1f)), scale);
                }
                yield return null;
            }
            knife.position = knifeHome;
            knife.localRotation = knifeRot;
            yield return new WaitForSeconds(0.25f);
            // la cipolla si ricompone
            V74CryingOnionSound.Play(this, "boing", 0.5f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                halfLeft.localPosition = halfLeftPos + new Vector3(-0.12f * (1f - k), 0f, 0f);
                halfRight.localPosition = halfRightPos + new Vector3(0.12f * (1f - k), 0f, 0f);
                yield return null;
            }
            OnionRest();
            FxPop(onion, 1.15f, 0.25f);
            if (!healedOnce) { healedOnce = true; toast.Show("...and the onion pulls itself together.", 1.8f); }
            RestFace();
            wellAt = Time.time + 1.5f;
            busy = false;
        }

        // ---------- Il frigo ----------

        void TapFridge()
        {
            OnionFridgeResult r = logic.TapFridge();
            if (r == OnionFridgeResult.None) return;
            V74CryingOnionSound.Play(this, "door", 0.6f);
            Haptics.Light();
            HalloweenKit.Show(doorClosed, !logic.FridgeOpen);
            HalloweenKit.Show(doorOpen, logic.FridgeOpen);
            FxShake(fridge, 0.02f, 0.2f);
            onionItem.Enabled = logic.CanGrabOnion;
            switch (r)
            {
                case OnionFridgeResult.OpenedEmpty:
                    toast.Show(EmptyFridgeLines[emptyLines++ % EmptyFridgeLines.Length], 1.6f);
                    break;
                case OnionFridgeResult.OpenedWarm:
                    FxWiggle(onion, 6f, 0.4f, 24f);
                    toast.Show(EarlyLines[earlyLines++ % EarlyLines.Length], 1.8f);
                    break;
                case OnionFridgeResult.OpenedCold:
                    FxPop(onion, 1.2f, 0.3f);
                    V74CryingOnionSound.Play(this, "frost", 0.5f);
                    toast.Show("Brrr! A frosty onion.", 1.8f);
                    break;
                case OnionFridgeResult.Rushed:
                    onionFrostShown = 1f;
                    HalloweenKit.SetAlpha(onionFrost, 1f);
                    FxPop(onion, 1.25f, 0.3f);
                    V74CryingOnionSound.Play(this, "frost", 0.6f);
                    toast.Show("Cold enough! Brrr!", 1.8f);
                    break;
                case OnionFridgeResult.ClosedChilling:
                    shiverAt = Time.time + 0.6f;
                    toast.Show("Chill, onion. Chill.", 1.6f);
                    break;
            }
        }

        void Chilled()
        {
            FxShake(fridge, 0.04f, 0.4f);
            Sfx.Play(this, "ding", 0.4f);
            toast.Show("Ding! One frosty onion.", 1.8f);
        }

        void WarmedUp()
        {
            FxWiggle(onion, 6f, 0.5f, 20f);
            toast.Show("Uh-oh. The onion warmed up again.", 2f);
            RestFace();
            wellAt = Time.time + 1f;
        }

        void ReadNote()
        {
            if (!logic.ReadNote()) return;
            FxPop(note, 1.7f, 0.6f);
            Sfx.Play(this, "tick", 0.4f);
            toast.Show(NoteLines[noteLines++ % NoteLines.Length], 2.4f);
        }

        // ---------- Le false piste ----------

        /// <summary>Gli occhialini volano sul cuoco, si appannano subito, lui annaspa, poi tornano al gancio.</summary>
        void Goggles()
        {
            if (logic.Goggles() == OnionGogglesResult.None) { SendHome(gogglesItem, 0.25f); return; }
            StartCoroutine(GogglesOn());
        }

        IEnumerator GogglesOn()
        {
            busy = true;
            CancelGrip();
            HalloweenKit.Show(gogglesStrap, false);
            StartCoroutine(HalloweenKit.ScaleTo(goggles, gogglesScale * eyes.localScale.x, 0.22f));
            yield return HalloweenKit.Move(goggles, faceSpot.position, 0.22f, 0.15f);
            V74CryingOnionSound.Play(this, "fog", 0.6f);
            yield return HalloweenKit.Fade(gogglesFog, 1f, 0.12f);
            toast.Show(GogglesLines[gogglesLines++ % GogglesLines.Length], 2f);
            for (float t = 0f; t < 1.4f; t += Time.deltaTime)
            {
                head.localRotation = Quaternion.Euler(0f, 0f, 7f * Mathf.Sin(t * 9f));
                goggles.position = faceSpot.position;
                goggles.rotation = head.rotation;
                yield return null;
            }
            head.localRotation = Quaternion.identity;
            goggles.localRotation = Quaternion.identity;
            HalloweenKit.SetAlpha(gogglesFog, 0f);
            StartCoroutine(HalloweenKit.ScaleTo(goggles, gogglesScale, 0.3f));
            yield return HalloweenKit.Move(goggles, gogglesHome, 0.3f, 0.2f);
            goggles.localScale = gogglesScale;
            HalloweenKit.Show(gogglesStrap, true);
            busy = false;
        }

        void ToggleFan()
        {
            bool before = logic.FanOn;
            bool on = logic.ToggleFan();
            if (on == before) return;
            Sfx.Play(this, "clunk", 0.4f);
            if (on) V74CryingOnionSound.Play(this, "whoosh", 0.4f);
            FxShake(fan, 0.02f, 0.2f);
            toast.Show(on ? FanOnLines[fanOnLines++ % FanOnLines.Length] : FanOffLines[fanOffLines++ % FanOffLines.Length], 1.6f);
        }

        /// <summary>Il fiato trattenuto: guance gonfie e faccia che diventa blu (k 0..1).</summary>
        void BreathVisual(float k, bool holding)
        {
            if (headArt != null) headArt.color = Color.Lerp(Color.white, BlueFace, Mathf.Clamp01(k));
            if (holding) { ShowMouth(mouthPuff); ShowEyes(eyesDry); }
            else RestFace();
        }

        void Breath(float seconds)
        {
            OnionBreathResult r = logic.HoldBreath(seconds);
            if (r == OnionBreathResult.None) { BreathVisual(0f, false); return; }
            StartCoroutine(Gasp(r == OnionBreathResult.Blue));
        }

        IEnumerator Gasp(bool blue)
        {
            busy = true;
            ShowMouth(mouthGasp);
            ShowEyes(blue ? eyesCry : eyesWatery);
            V74CryingOnionSound.Play(this, "gasp", 0.7f);
            Haptics.Light();
            FxShake(head, blue ? 0.05f : 0.025f, 0.4f);
            toast.Show(blue ? BlueLines[blueLines++ % BlueLines.Length] : GaspLines[gaspLines++ % GaspLines.Length], 1.9f);
            Color from = headArt != null ? headArt.color : Color.white;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
            {
                if (headArt != null) headArt.color = Color.Lerp(from, Color.white, k);
                yield return null;
            }
            BreathVisual(0f, false);
            busy = false;
        }

        void LightCandle()
        {
            OnionRemedyResult r = logic.LightCandle();
            if (r == OnionRemedyResult.None) return;
            if (r == OnionRemedyResult.Applied)
            {
                HalloweenKit.Show(flame, true);
                V74CryingOnionSound.Play(this, "match", 0.5f);
                FxPop(flame, 1.5f, 0.3f);
                toast.Show("Grandma's trick: a candle eats the fumes.", 2f);
                return;
            }
            FxWiggle(candle, 4f, 0.3f, 30f);
            toast.Show(CandleLines[candleLines++ % CandleLines.Length], 1.6f);
        }

        void Bread()
        {
            if (logic.Bread() != OnionRemedyResult.Applied) { SendHome(breadItem, 0.25f); return; }
            StartCoroutine(BreadIn());
        }

        IEnumerator BreadIn()
        {
            busy = true;
            CancelGrip();
            yield return HalloweenKit.Move(bread, mouth.position, 0.2f, 0.15f);
            HalloweenKit.Show(bread, false);
            breadItem.Enabled = false;
            HalloweenKit.Show(breadBite, true);
            ShowMouth(null);
            V74CryingOnionSound.Play(this, "munch", 0.6f);
            FxPop(breadBite, 1.25f, 0.25f);
            toast.Show("Bread in mouth. Grandma swears by it.", 2f);
            yield return new WaitForSeconds(0.3f);
            busy = false;
        }

        void TapCook()
        {
            if (!logic.TapCook()) return;
            FxWiggle(head, 5f, 0.4f, 22f);
            V74CryingOnionSound.Play(this, "sniff", 0.5f);
            if (logic.EyesWatery) toast.Show(WateryCookLines[cookLines++ % WateryCookLines.Length], 1.7f);
            else toast.Show(DryCookLines[cookLines++ % DryCookLines.Length], 1.7f);
        }

        // ---------- La soluzione ----------

        /// <summary>La cipolla fredda: quattro colpi secchi, cubetti ordinati, occhi asciutti e un gran sorriso.</summary>
        IEnumerator Finale(bool fresh)
        {
            finishing = true;
            busy = false;
            CancelGrip();
            StopFx();
            StopTears();
            if (coldPuffs != null) coldPuffs.Stop();
            Fumes(true);
            onion.position = OnionSpotPos();
            OnionOrder(OnionBoardOrder);
            HalloweenKit.Show(onion, true);
            HalloweenKit.Show(halfLeft, false);
            HalloweenKit.Show(halfRight, false);
            HalloweenKit.Show(onionWhole, true);
            HalloweenKit.Show(diced, false);
            if (headArt != null) headArt.color = Color.white;
            goggles.position = gogglesHome;
            goggles.localScale = gogglesScale;
            goggles.localRotation = Quaternion.identity;
            head.localRotation = Quaternion.identity;
            HalloweenKit.Show(gogglesStrap, true);
            HalloweenKit.SetAlpha(gogglesFog, 0f);
            ShowEyes(eyesDry);
            ShowMouth(logic.BreadInMouth ? null : mouthWobble);
            Vector3 o = onion.position;
            knife.localRotation = Quaternion.Euler(0f, 0f, 200f);
            if (fresh) yield return HalloweenKit.Move(knife, o + new Vector3(0.3f, 0.5f, 0f), 0.15f);
            for (int i = 0; i < 4; i++)
            {
                Vector3 x = new Vector3(-0.15f + 0.12f * i, 0f, 0f);
                yield return HalloweenKit.Move(knife, o + x + new Vector3(0.3f, 0.45f, 0f), 0.07f);
                yield return HalloweenKit.Move(knife, o + x + new Vector3(0.22f, 0.02f, 0f), 0.06f);
                V74CryingOnionSound.Play(this, "chop", 0.6f);
                Haptics.Light();
                if (i == 1)
                {
                    HalloweenKit.Show(onionWhole, false);
                    HalloweenKit.Show(halfLeft, true);
                    HalloweenKit.Show(halfRight, true);
                    halfLeft.localPosition = halfLeftPos + new Vector3(-0.08f, 0f, 0f);
                    halfRight.localPosition = halfRightPos + new Vector3(0.08f, 0f, 0f);
                }
            }
            HalloweenKit.Show(onion, false);
            HalloweenKit.Show(diced, true);
            FxPop(diced, 1.2f, 0.3f);
            yield return HalloweenKit.Move(knife, knifeHome, 0.25f, 0.2f);
            knife.localRotation = knifeRot;
            // occhi asciutti, sorriso (il pane, se c'era, se lo mangia)
            if (logic.BreadInMouth) { HalloweenKit.Show(breadBite, false); V74CryingOnionSound.Play(this, "munch", 0.5f); }
            ShowEyes(eyesHappy);
            ShowMouth(mouthSmile);
            FxPop(head, 1.08f, 0.3f);
            Haptics.Success();
            if (sparkle != null) { HalloweenKit.Show(sparkle, true); FxPop(sparkle, 1.6f, 0.4f); }
            if (glow != null) StartCoroutine(HalloweenKit.Fade(glow, 0.85f, 0.25f));
            Sfx.Play(this, "ding", 0.5f);
            toast.Show("COLD ONIONS DON'T CRY.", 1.5f);
            yield return new WaitForSeconds(1.3f);
            toast.Show("NEITHER DO YOU.", 2.2f);
            yield return Celebrate(1.2f);
            if (logic.TryComplete()) Win();
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (Input.GetKeyDown(KeyCode.F)) TapFridge();
            if (Input.GetKeyDown(KeyCode.I) && logic.DropOnion(true) == OnionDropResult.InFridge) { OnionRest(); RestFace(); }
            if (Input.GetKeyDown(KeyCode.B) && logic.DropOnion(false) == OnionDropResult.OnBoard) { OnionRest(); RestFace(); }
            if (Input.GetKeyDown(KeyCode.N)) ToggleFan();
            if (Input.GetKeyDown(KeyCode.K)) LightCandle();
            if (Input.GetKeyDown(KeyCode.T)) ReadNote();
            if (Input.GetKeyDown(KeyCode.X)) TapCook();
            if (Input.GetKeyDown(KeyCode.C)) { Chop(); return; }
            if (Input.GetKeyDown(KeyCode.G)) { Goggles(); return; }
            if (Input.GetKeyDown(KeyCode.R)) { Bread(); return; }
            if (Input.GetKeyDown(KeyCode.H)) Breath(CryingOnionLogic.BlueAfter);
        }
#endif
    }

    /// <summary>Suoni sintetizzati del livello 274: coltello, singhiozzo, frigo, brina, ventilatore, fiato, pane.</summary>
    public static class V74CryingOnionSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(274);

        public static void Play(Component owner, string kind, float volume = 0.6f)
        {
            if (owner == null) return;
            AudioClip c = Clip(kind);
            if (c == null) { Sfx.Play(owner, kind, volume); return; }
            AudioSource src = owner.GetComponent<AudioSource>();
            if (src == null) { src = owner.gameObject.AddComponent<AudioSource>(); src.playOnAwake = false; }
            src.PlayOneShot(c, volume);
        }

        static float N() => (float)rnd.NextDouble() * 2f - 1f;

        static AudioClip Clip(string kind)
        {
            if (cache.TryGetValue(kind, out AudioClip c) && c != null) return c;
            switch (kind)
            {
                case "chop":     // lama sul tagliere: toc secco di legno
                    c = Make(kind, 0.12f, t => Mathf.Sin(t * 2f * Mathf.PI * 520f) * 0.5f * Mathf.Exp(-t * 55f) + N() * 0.4f * Mathf.Exp(-t * 120f));
                    break;
                case "sob":      // singhiozzo che scende: "uuuaaah"
                    c = Make(kind, 0.7f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(520f, 300f, t / 0.7f) * (1f + 0.03f * Mathf.Sin(t * 2f * Mathf.PI * 7f))) * 0.3f * Mathf.Sin(t / 0.7f * Mathf.PI) * (0.6f + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * 6f)));
                    break;
                case "door":     // sportello del frigo: risucchio della guarnizione e tonfo
                    c = Make(kind, 0.25f, t => Mathf.Sin(t * 2f * Mathf.PI * 110f) * 0.55f * Mathf.Exp(-t * 18f) + N() * 0.25f * Mathf.Exp(-t * 60f));
                    break;
                case "hum":      // il frigo che ronza
                    c = Make(kind, 0.35f, t => (Mathf.Sin(t * 2f * Mathf.PI * 60f) + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * 120f)) * 0.25f * Mathf.Sin(t / 0.35f * Mathf.PI));
                    break;
                case "frost":    // brina: tintinnio di cristalli
                    c = Make(kind, 0.4f, t => (Mathf.Sin(t * 2f * Mathf.PI * 2350f) + Mathf.Sin(t * 2f * Mathf.PI * 3130f) * 0.6f) * 0.18f * Mathf.Exp(-t * 9f));
                    break;
                case "whoosh":   // il ventilatore che parte
                    {
                        float lp = 0f;
                        c = Make(kind, 0.45f, t => { lp += (N() - lp) * 0.15f; return lp * 1.4f * Mathf.Sin(t / 0.45f * Mathf.PI); });
                    }
                    break;
                case "fog":      // occhialini che si appannano: soffio caldo
                    {
                        float lp = 0f;
                        c = Make(kind, 0.3f, t => { lp += (N() - lp) * 0.06f; return lp * 2f * Mathf.Exp(-t * 6f); });
                    }
                    break;
                case "gasp":     // boccata d'aria: soffio che sale
                    c = Make(kind, 0.3f, t => N() * 0.45f * Mathf.Sin(t / 0.3f * Mathf.PI) * (0.4f + t / 0.3f) + Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(300f, 600f, t / 0.3f)) * 0.08f);
                    break;
                case "sniff":    // tirata su col naso
                    {
                        float lp = 0f;
                        c = Make(kind, 0.2f, t => { lp += (N() - lp) * 0.3f; return lp * 0.9f * Mathf.Sin(t / 0.2f * Mathf.PI); });
                    }
                    break;
                case "munch":    // morso al pane
                    c = Make(kind, 0.18f, t => N() * 0.5f * Mathf.Exp(-t * 25f) * (0.5f + 0.5f * Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * 30f))));
                    break;
                case "match":    // fiammifero: sfregata e fiammata
                    c = Make(kind, 0.3f, t => N() * 0.4f * (t < 0.08f ? 1f : Mathf.Exp(-(t - 0.08f) * 12f) * 0.6f));
                    break;
                case "boing":    // la cipolla che si ricompone
                    c = Make(kind, 0.25f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(240f, 480f, t / 0.25f)) * 0.35f * Mathf.Exp(-t * 9f));
                    break;
                default:
                    return null;
            }
            cache[kind] = c;
            return c;
        }

        static AudioClip Make(string name, float seconds, System.Func<float, float> f)
        {
            int n = Mathf.Max(1, (int)(seconds * Rate));
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(f(i / (float)Rate), -1f, 1f);
            AudioClip clip = AudioClip.Create("v74b_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
