using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 30 della roadmap "The Invisible Treasure" (id invisible_treasure) — "FIND THE TREASURE." Un'isoletta di
    /// sabbia in mezzo al mare con una palma; in alto a sinistra una mappa del tesoro (un foglio piccolo nel mondo) che
    /// disegna la stessa isola con la palma e una X rossa accanto; sulla sabbia una pala.
    /// Schema di scavo (UNO solo, documentato): si TRASCINA la pala e la si LASCIA: si scava dove sta la PUNTA della lama
    /// (un anello tratteggiato sotto la punta mostra il posto: bianco sulla sabbia, rosso fuori, dorato sulla X a terra).
    /// Toccare la pala senza trascinarla dice solo "Drag me where you want to dig.".
    /// Inganno: scavare nel posto indicato dalla mappa (accanto alla palma) non trova niente ("Nothing. The map lied.", la
    /// mappa trema); scavare altrove trova a rotazione sabbia, uno scarpone, sabbia, una lattina, un granchio che pizzica
    /// la pala e scappa; le buche restano solo come decorazione (5 al massimo, la più vecchia viene riciclata) e non
    /// bloccano mai niente. La pala lasciata fuori dalla sabbia (mare, cielo, mappa) torna a posto senza scavare.
    /// Soluzione: trascinare la X DALLA MAPPA sulla SABBIA (dove si vuole; diventa grande come una X vera appena lascia la
    /// mappa) e poi scavare lì con la pala (tolleranza 0.8): il baule esce dalla buca, si apre con monete e scintille
    /// dorate; LevelReward "YOU DECIDED WHERE THE TREASURE WAS!" -> Win una volta sola.
    /// La X lasciata sulla mappa, in mare, in cielo o fuori dall'isola torna al suo posto sulla mappa; quella a terra si
    /// può riprendere e spostare ancora. Un dito alla volta (HalloweenKit.ReadPointer); due dita, pausa, app in background
    /// o livello spento mollano la presa: la X torna dov'era, la pala torna a posto senza scavare. Non si può perdere.
    /// La logica è in Gotcha.Levels.V64.InvisibleTreasureLogic.
    /// Tasti di prova nell'Editor: T tocco sulla sabbia, B mare, M mappa, X la X (tocco), K palma, S pala (tocco),
    /// D scava nel posto della mappa, E scava altrove, O pala lasciata in mare, Q X lasciata in mare, A X rimessa sulla
    /// mappa, Y X sulla sabbia (-0.9, -2.0), V X spostata ancora (1.0, -2.3), G scava sulla X, W soluzione completa,
    /// P pausa simulata (presa annullata), R ricomincia.
    /// </summary>
    public class V80InvisibleTreasureLevel : LevelController
    {
        [Header("Isola")]
        [SerializeField] Transform palm;
        [SerializeField] Collider2D palmArea;
        [SerializeField] float islandX = 0f, islandY = -1.45f, islandRx = 2.3f, islandRy = 2.0f;
        [SerializeField] float horizon = 1.2f;            // sotto c'è il mare (per i suoni: tuffo o "fruscio")
        [SerializeField] float spotX = 0.5f, spotY = -0.75f;   // il posto (terreno) indicato dalla mappa
        [Header("Buche e cianfrusaglie")]
        [SerializeField] Transform[] holes;               // riciclate, solo decorazione
        [SerializeField] Transform boot;
        [SerializeField] Transform can;
        [SerializeField] Transform crab;
        [SerializeField] Transform[] puffs;               // sbuffi di sabbia
        [Header("Il baule")]
        [SerializeField] Transform chest;                 // origine in basso
        [SerializeField] Transform chestClosed;
        [SerializeField] Transform chestOpen;
        [SerializeField] SpriteRenderer chestGlow;
        [SerializeField] Transform[] coins;
        [SerializeField] Transform[] sparkles;
        [Header("Mappa e X")]
        [SerializeField] Transform map;
        [SerializeField] Collider2D mapArea;
        [SerializeField] float mapHalfW = 1.1f, mapHalfH = 0.7f;
        [SerializeField] Transform mapXHome;              // il posto della X sulla mappa (figlio della mappa)
        [SerializeField] Transform xMark;
        [SerializeField] float xMapScale = 0.42f, xGroundScale = 0.9f;
        [Header("Pala")]
        [SerializeField] Transform shovel;                // origine sulla punta della lama
        [SerializeField] Collider2D shovelArea;
        [SerializeField] SpriteRenderer digRing;
        [Header("Scritte e festa")]
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "YOU DECIDED WHERE THE TREASURE WAS!";
        const float TapSlop = 0.12f, Follow = 14f, XGrabOnMap = 0.45f, XGrabOnGround = 0.6f, ChestMaxX = 1.8f;
        static readonly Color RingSand = new Color(1f, 1f, 1f, 0.9f), RingBad = new Color(1f, 0.4f, 0.35f, 0.9f), RingX = new Color(1f, 0.82f, 0.2f, 1f);

        static readonly string[] MapSpotLines = { "Nothing. The map lied.", "Still nothing. Liar map.", "Empty. The map is wrong." };
        static readonly string[] SandLines = { "Just sand.", "More sand. Lovely.", "Sand. Lots of sand." };
        static readonly string[] BootLines = { "An old boot. Not treasure.", "Another boot? No." };
        static readonly string[] CanLines = { "A rusty can. No.", "A can. Not gold." };
        static readonly string[] CrabLines = { "A crab! OUCH!", "Pinched again! OUCH!" };
        static readonly string[] NotSandLines = { "Dig on the sand.", "You can't dig the sea.", "Sand only, please." };
        const string DigMapLine = "Don't dig the map!";
        static readonly string[] XSeaLines = { "Splash! The X swims back.", "Not there. Back to the map." };
        const string XMapLine = "Back on the map.";
        const string XFirstSandLine = "The X is on the sand now.";
        static readonly string[] XSandLines = { "X moved.", "New spot for the X." };
        static readonly string[] MapTapLines = { "An old treasure map.", "X marks the spot. Supposedly." };
        static readonly string[] MapXTapLines = { "The X wiggles. It's loose!", "This X isn't glued down." };
        static readonly string[] GroundXTapLines = { "Your X. Dig here?", "X marks YOUR spot." };
        static readonly string[] ShovelTapLines = { "Drag me where you want to dig.", "Drag the shovel to dig." };
        static readonly string[] PalmLines = { "Just a palm tree.", "Coconuts. No gold." };
        static readonly string[] SandTapLines = { "Bare hands? Use the shovel.", "Use the shovel." };
        static readonly string[] SeaLines = { "Just water.", "The sea. No treasure." };

        enum Kind { None, X, Shovel, Map, Palm, Sand, Sea }

        sealed class Grip
        {
            public Kind kind;
            public Vector2 start, offset;
            public bool moved, held;
        }

        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        InvisibleTreasureLogic logic;
        Camera cam;
        HwToast toast;
        Grip grip;
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        Coroutine bootCo, canCo, crabCo, puffCo, digCo;
        Vector3 shovelHome, holeScale, bootScale, canScale, crabScale, puffScale, chestScale, coinScale, sparkleScale;
        Quaternion shovelHomeRot;
        int nextHole, mapSpotLines, sandLines, bootLines, canLines, crabLines, notSandLines, xSeaLines, xSandLines;
        int mapTapLines, mapXTapLines, groundXTapLines, shovelTapLines, palmLines, sandTapLines, seaLines;
        bool captured, finishing, busy;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            bootCo = canCo = crabCo = puffCo = digCo = null;
            StopFx();
            if (!captured)
            {
                captured = true;
                logic = new InvisibleTreasureLogic();
                logic.Configure(islandX, islandY, islandRx, islandRy, map.position.x, map.position.y, mapHalfW, mapHalfH, spotX, spotY);
                shovelHome = shovel.position;
                shovelHomeRot = shovel.localRotation;
                holeScale = holes != null && holes.Length > 0 && holes[0] != null ? holes[0].localScale : Vector3.one;
                bootScale = boot.localScale;
                canScale = can.localScale;
                crabScale = crab.localScale;
                puffScale = puffs != null && puffs.Length > 0 && puffs[0] != null ? puffs[0].localScale : Vector3.one;
                chestScale = chest.localScale;
                coinScale = coins != null && coins.Length > 0 && coins[0] != null ? coins[0].localScale : Vector3.one;
                sparkleScale = sparkles != null && sparkles.Length > 0 && sparkles[0] != null ? sparkles[0].localScale : Vector3.one;
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            grip = null;
            finishing = busy = false;
            nextHole = 0;
            mapSpotLines = sandLines = bootLines = canLines = crabLines = notSandLines = xSeaLines = xSandLines = 0;
            mapTapLines = mapXTapLines = groundXTapLines = shovelTapLines = palmLines = sandTapLines = seaLines = 0;
            toast = new HwToast(infoText, infoHolder);
            if (holes != null) foreach (Transform h in holes) HalloweenKit.Show(h, false);   // le buche spariscono solo qui
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            busy = false;
            HideFinds();
            HideTreasure();
            HalloweenKit.Show(digRing, false);
            HalloweenKit.Show(xMark, true);
            SetAlpha(xMark, 1f);
            xMark.position = XTarget();
            xMark.localScale = Vector3.one * XTargetScale();
            xMark.localRotation = Quaternion.identity;
            shovel.position = shovelHome;
            shovel.localRotation = shovelHomeRot;
            map.localRotation = Quaternion.identity;
            if (palm != null) palm.localRotation = Quaternion.identity;
        }

        Vector3 XTarget()
        {
            Vector3 p = logic.XOnGround ? new Vector3(logic.GroundX, logic.GroundY, 0f) : mapXHome.position;
            return new Vector3(p.x, p.y, xMark.position.z);
        }

        float XTargetScale() => logic.XOnGround ? xGroundScale : xMapScale;

        void HideFinds()
        {
            foreach (Transform t in new[] { boot, can, crab })
            {
                if (t == null) continue;
                HalloweenKit.Show(t, false);
                SetAlpha(t, 1f);
                t.localRotation = Quaternion.identity;
            }
            if (boot != null) boot.localScale = bootScale;
            if (can != null) can.localScale = canScale;
            if (crab != null) crab.localScale = crabScale;
            if (puffs != null) foreach (Transform p in puffs) { HalloweenKit.Show(p, false); if (p != null) p.localScale = puffScale; }
        }

        void HideTreasure()
        {
            HalloweenKit.Show(chest, false);
            chest.localScale = chestScale;
            chest.localRotation = Quaternion.identity;
            HalloweenKit.Show(chestClosed, true);
            HalloweenKit.Show(chestOpen, false);
            if (chestGlow != null) chestGlow.color = new Color(1f, 0.85f, 0.35f, 0f);
            if (coins != null) foreach (Transform c in coins) HalloweenKit.Show(c, false);
            if (sparkles != null) foreach (Transform s in sparkles) HalloweenKit.Show(s, false);
        }

        static void SetAlpha(Transform t, float a)
        {
            if (t == null) return;
            foreach (SpriteRenderer s in t.GetComponentsInChildren<SpriteRenderer>(true)) { Color c = s.color; c.a = a; s.color = c; }
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: si molla la presa, si fermano gli effetti, si torna a uno stato valido
            CancelGrip();
            StopAllCoroutines();
            bootCo = canCo = crabCo = puffCo = digCo = null;
            StopFx();
            if (captured && logic != null && !IsFinished)
            {
                if (logic.Stage == InvisibleTreasureStage.Playing) RestPose();
                finishing = false;   // la festa interrotta riparte da capo in OnEnable
            }
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == InvisibleTreasureStage.Celebrating) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrip(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrip(); }

        /// <summary>Il dito sparisce: la X torna dov'era, la pala torna a posto senza scavare, niente battute.</summary>
        void CancelGrip()
        {
            grip = null;
            HalloweenKit.Show(digRing, false);
            if (logic != null && !logic.InputLocked) logic.CancelHold();
        }

        void Update()
        {
            toast?.Tick();
            if (IsFinished || toast == null || logic == null) return;
            if (finishing) return;
            if (Time.timeScale <= 0f) { CancelGrip(); return; }   // pausa o pubblicità: niente input
#if UNITY_EDITOR
            EditorKeys();
#endif
            if (!finishing) ReadPointer();
            if (!finishing) Pose();
        }

        /// <summary>La X e la pala, se nessuno le tiene, scivolano verso il loro posto (dalla logica).</summary>
        void Pose()
        {
            float k = 1f - Mathf.Exp(-Time.deltaTime * Follow);
            if (!logic.HoldingX)
            {
                xMark.position = Vector3.Lerp(xMark.position, XTarget(), k);
                xMark.localScale = Vector3.Lerp(xMark.localScale, Vector3.one * XTargetScale(), k);
            }
            if (!logic.HoldingShovel && !busy)
            {
                shovel.position = Vector3.Lerp(shovel.position, shovelHome, k);
                shovel.localRotation = Quaternion.Slerp(shovel.localRotation, shovelHomeRot, k);
            }
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73-0.79) ----------

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

        // ---------- Dito ----------

        void ReadPointer()
        {
            if (Input.touchCount >= 2) { CancelGrip(); return; }   // due dita: il gesto si annulla
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (grip != null) End(grip);
                return;
            }
            if (began) { if (grip != null) End(grip); grip = Begin(w); }
            if (grip == null) return;
            if (!grip.moved && Vector2.Distance(w, grip.start) > TapSlop)
            {
                grip.moved = true;
                StartDrag(grip);
            }
            if (grip != null && grip.held) Drag(grip, w);
            if (ended && grip != null) End(grip);
        }

        static bool Over(Collider2D c, Vector2 p) => c != null && c.gameObject.activeInHierarchy && c.OverlapPoint(p);

        /// <summary>Cosa c'è sotto il dito: la X (cerchio generoso), la pala, la mappa, la palma, la sabbia o il mare.</summary>
        Kind Pick(Vector2 w)
        {
            float xr = logic.XOnGround ? XGrabOnGround : XGrabOnMap;
            if (Vector2.Distance(w, xMark.position) <= xr) return Kind.X;
            if (Over(shovelArea, w)) return busy ? Kind.None : Kind.Shovel;
            if (Over(mapArea, w)) return Kind.Map;
            if (Over(palmArea, w)) return Kind.Palm;
            return logic.IsSand(w.x, w.y) ? Kind.Sand : Kind.Sea;
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, kind = Kind.None };
            if (finishing || logic.InputLocked) return g;
            g.kind = Pick(w);
            return g;
        }

        /// <summary>Il dito ha superato la soglia del tocco: si prova a trascinare.</summary>
        void StartDrag(Grip g)
        {
            switch (g.kind)
            {
                case Kind.X:
                    if (!logic.GrabX()) { g.kind = Kind.None; return; }
                    g.held = true;
                    g.offset = (Vector2)xMark.position - g.start;
                    Haptics.Tap();
                    V80InvisibleTreasureSound.Play(this, "paper", 0.35f);
                    break;
                case Kind.Shovel:
                    if (busy || !logic.GrabShovel()) { g.kind = Kind.None; return; }
                    g.held = true;
                    g.offset = (Vector2)shovel.position - g.start;
                    shovel.localRotation = Quaternion.identity;
                    Haptics.Tap();
                    V80InvisibleTreasureSound.Play(this, "clink", 0.3f);
                    break;
            }
        }

        void Drag(Grip g, Vector2 w)
        {
            if (g.kind == Kind.X && logic.HoldingX)
            {
                Vector2 p = HalloweenKit.ClampSafe(w + g.offset);
                xMark.position = new Vector3(p.x, p.y, xMark.position.z);
                // sopra la mappa è piccola come un disegno, fuori è grande come una X vera sulla sabbia
                float target = logic.IsOnMap(p.x, p.y) ? xMapScale : xGroundScale;
                xMark.localScale = Vector3.Lerp(xMark.localScale, Vector3.one * target, 1f - Mathf.Exp(-Time.deltaTime * 18f));
            }
            else if (g.kind == Kind.Shovel && logic.HoldingShovel)
            {
                Vector2 p = HalloweenKit.ClampSafe(w + g.offset);
                shovel.position = new Vector3(p.x, p.y, shovel.position.z);
                Ring(p);
            }
        }

        /// <summary>L'anello sotto la punta della pala: bianco sulla sabbia, rosso fuori, dorato sulla X a terra.</summary>
        void Ring(Vector2 tip)
        {
            if (digRing == null) return;
            HalloweenKit.Show(digRing, true);
            digRing.transform.position = new Vector3(tip.x, tip.y, digRing.transform.position.z);
            digRing.color = !logic.IsSand(tip.x, tip.y) ? RingBad : logic.IsOnGroundX(tip.x, tip.y) ? RingX : RingSand;
        }

        void End(Grip g)
        {
            grip = null;
            switch (g.kind)
            {
                case Kind.X:
                    if (g.held) DropX();
                    else TapX();
                    break;
                case Kind.Shovel:
                    if (g.held) DropShovel();
                    else TapShovel();
                    break;
                case Kind.Map: TapMap(); break;
                case Kind.Palm: TapPalm(); break;
                case Kind.Sand: TapSand(); break;
                case Kind.Sea: TapSea(); break;
            }
        }

        // ---------- Tocchi (false piste) ----------

        void Say(string[] lines, ref int counter, float seconds = 1.6f) => toast.Show(lines[counter++ % lines.Length], seconds);

        void TapMap()
        {
            if (!logic.Tap(InvisibleTreasureSpot.Map)) return;
            FxWiggle(map, 3f, 0.35f, 22f);
            V80InvisibleTreasureSound.Play(this, "paper", 0.35f);
            Say(MapTapLines, ref mapTapLines);
        }

        void TapX()
        {
            bool ground = logic.XOnGround;
            if (!logic.Tap(ground ? InvisibleTreasureSpot.GroundX : InvisibleTreasureSpot.MapX)) return;
            FxWiggle(xMark, 14f, 0.4f, 26f);
            V80InvisibleTreasureSound.Play(this, "pop", 0.3f);
            if (ground) Say(GroundXTapLines, ref groundXTapLines);
            else Say(MapXTapLines, ref mapXTapLines);
        }

        void TapShovel()
        {
            if (!logic.Tap(InvisibleTreasureSpot.Shovel)) return;
            FxWiggle(shovel, 6f, 0.35f, 24f);
            V80InvisibleTreasureSound.Play(this, "clink", 0.3f);
            Say(ShovelTapLines, ref shovelTapLines, 2f);
        }

        void TapPalm()
        {
            if (!logic.Tap(InvisibleTreasureSpot.Palm)) return;
            FxWiggle(palm, 2.5f, 0.5f, 14f);
            V80InvisibleTreasureSound.Play(this, "rustle", 0.35f);
            Say(PalmLines, ref palmLines);
        }

        void TapSand()
        {
            if (!logic.Tap(InvisibleTreasureSpot.Sand)) return;
            V80InvisibleTreasureSound.Play(this, "rustle", 0.25f);
            Say(SandTapLines, ref sandTapLines);
        }

        void TapSea()
        {
            if (!logic.Tap(InvisibleTreasureSpot.Sea)) return;
            V80InvisibleTreasureSound.Play(this, "splash", 0.3f);
            Say(SeaLines, ref seaLines);
        }

        // ---------- La X ----------

        void DropX()
        {
            Vector3 at = xMark.position;
            switch (logic.DropX(at.x, at.y))
            {
                case InvisibleTreasureDrop.OnSand:
                    xMark.position = XTarget();
                    FxPop(xMark, 1.18f, 0.22f);
                    Haptics.Light();
                    V80InvisibleTreasureSound.Play(this, "pop", 0.4f);
                    if (logic.XPlacements == 1) toast.Show(XFirstSandLine, 1.8f);
                    else Say(XSandLines, ref xSandLines, 1.4f);
                    break;
                case InvisibleTreasureDrop.OnMap:
                    V80InvisibleTreasureSound.Play(this, "paper", 0.35f);
                    toast.Show(XMapLine, 1.4f);
                    break;
                case InvisibleTreasureDrop.OffIsland:
                    // torna sulla mappa (Pose la fa scivolare); in mare un piccolo tuffo
                    V80InvisibleTreasureSound.Play(this, at.y < horizon ? "splash" : "boing", 0.4f);
                    Haptics.Tap();
                    Say(XSeaLines, ref xSeaLines, 1.8f);
                    break;
            }
        }

        // ---------- La pala ----------

        void DropShovel()
        {
            HalloweenKit.Show(digRing, false);
            Vector2 tip = shovel.position;
            InvisibleTreasureDig found = logic.DropShovel(tip.x, tip.y);
            switch (found)
            {
                case InvisibleTreasureDig.None:
                    return;
                case InvisibleTreasureDig.NotSand:
                    V80InvisibleTreasureSound.Play(this, tip.y < horizon && !logic.IsOnMap(tip.x, tip.y) ? "splash" : "clink", 0.35f);
                    if (logic.IsOnMap(tip.x, tip.y)) { FxWiggle(map, 3f, 0.35f, 22f); toast.Show(DigMapLine, 1.6f); }
                    else Say(NotSandLines, ref notSandLines);
                    return;
                case InvisibleTreasureDig.Treasure:
                    StartCoroutine(Finale());
                    return;
                default:
                    digCo = StartCoroutine(Dig(tip, found));
                    return;
            }
        }

        /// <summary>Due colpi di pala, sbuffi di sabbia, la buca; poi quello che si trova (mai il tesoro qui).</summary>
        IEnumerator Dig(Vector2 tip, InvisibleTreasureDig found)
        {
            busy = true;
            yield return Jabs(tip, 2);
            busy = false;
            digCo = null;
            switch (found)
            {
                case InvisibleTreasureDig.MapSpot:
                    FxWiggle(map, 6f, 0.6f, 20f);   // la mappa trema "colpevole"
                    V80InvisibleTreasureSound.Play(this, "wahwah", 0.45f);
                    Say(MapSpotLines, ref mapSpotLines, 2f);
                    break;
                case InvisibleTreasureDig.Sand:
                    Say(SandLines, ref sandLines);
                    break;
                case InvisibleTreasureDig.Boot:
                    if (bootCo != null) StopCoroutine(bootCo);
                    bootCo = StartCoroutine(ShowFind(boot, bootScale, tip, false));
                    V80InvisibleTreasureSound.Play(this, "boing", 0.4f);
                    Say(BootLines, ref bootLines, 1.8f);
                    break;
                case InvisibleTreasureDig.Can:
                    if (canCo != null) StopCoroutine(canCo);
                    canCo = StartCoroutine(ShowFind(can, canScale, tip, false));
                    V80InvisibleTreasureSound.Play(this, "clink", 0.45f);
                    Say(CanLines, ref canLines, 1.8f);
                    break;
                case InvisibleTreasureDig.Crab:
                    if (crabCo != null) StopCoroutine(crabCo);
                    crabCo = StartCoroutine(ShowFind(crab, crabScale, tip, true));
                    V80InvisibleTreasureSound.Play(this, "pinch", 0.5f);
                    Haptics.Medium();
                    FxShake(shovel, 0.06f, 0.35f);
                    Say(CrabLines, ref crabLines, 1.8f);
                    break;
            }
        }

        /// <summary>La pala colpisce la sabbia <paramref name="n"/> volte nel punto <paramref name="tip"/>; buca e sbuffi.</summary>
        IEnumerator Jabs(Vector2 tip, int n)
        {
            Vector3 at = new Vector3(tip.x, tip.y, shovel.position.z);
            for (int i = 0; i < n; i++)
            {
                for (float t = 0f; t < 0.14f; t += Time.deltaTime)
                {
                    float s = Mathf.Sin(Mathf.Clamp01(t / 0.14f) * Mathf.PI);
                    shovel.position = at + new Vector3(0.05f * s, 0.18f * s, 0f);
                    shovel.localRotation = Quaternion.Euler(0f, 0f, -18f * s);
                    yield return null;
                }
                shovel.position = at;
                Haptics.Tap();
                V80InvisibleTreasureSound.Play(this, "dig", 0.45f);
                if (i == 0) SpawnHole(tip);
                if (puffCo != null) StopCoroutine(puffCo);
                puffCo = StartCoroutine(Puffs(tip));
            }
            shovel.localRotation = Quaternion.identity;
        }

        void SpawnHole(Vector2 at)
        {
            if (holes == null || holes.Length == 0) return;
            Transform h = holes[nextHole++ % holes.Length];   // la più vecchia viene riciclata
            if (h == null) return;
            h.position = new Vector3(at.x, at.y - 0.05f, h.position.z);
            h.localScale = holeScale;
            HalloweenKit.Show(h, true);
            FxPop(h, 1.15f, 0.2f);
        }

        IEnumerator Puffs(Vector2 at)
        {
            if (puffs == null) yield break;
            const float life = 0.4f;
            for (float t = 0f; t < life; t += Time.deltaTime)
            {
                float k = t / life;
                for (int i = 0; i < puffs.Length; i++)
                {
                    if (puffs[i] == null) continue;
                    HalloweenKit.Show(puffs[i], true);
                    float dir = i % 2 == 0 ? -1f : 1f;
                    float spread = 0.25f + 0.15f * (i / 2);
                    puffs[i].position = new Vector3(at.x + dir * spread * k * 1.6f, at.y + 0.1f + 0.45f * k - 0.3f * k * k, puffs[i].position.z);
                    puffs[i].localScale = puffScale * (0.6f + 0.6f * k);
                    SetAlpha(puffs[i], 1f - k);
                }
                yield return null;
            }
            foreach (Transform p in puffs) HalloweenKit.Show(p, false);
            puffCo = null;
        }

        /// <summary>Una cianfrusaglia esce dalla buca, resta un attimo e svanisce; il granchio pizzica e scappa di lato.</summary>
        IEnumerator ShowFind(Transform item, Vector3 baseScale, Vector2 at, bool scuttle)
        {
            if (item == null) yield break;
            HalloweenKit.Show(item, true);
            SetAlpha(item, 1f);
            item.localRotation = Quaternion.identity;
            Vector3 from = new Vector3(at.x, at.y - 0.1f, item.position.z), to = new Vector3(at.x, at.y + 0.3f, item.position.z);
            for (float t = 0f; t < 0.25f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.25f);
                item.position = Vector3.Lerp(from, to, 1f - (1f - k) * (1f - k));
                item.localScale = baseScale * Mathf.Lerp(0.4f, 1f, k);
                yield return null;
            }
            item.position = to;
            item.localScale = baseScale;
            if (scuttle)
            {
                // pizzica: chele che schioccano
                for (float t = 0f; t < 0.45f; t += Time.deltaTime)
                {
                    item.localScale = baseScale * (1f + 0.12f * Mathf.Abs(Mathf.Sin(t * 28f)));
                    yield return null;
                }
                item.localScale = baseScale;
                // e scappa verso il mare più vicino, di lato
                float dir = at.x <= islandX ? -1f : 1f;
                Vector3 start = item.position;
                for (float t = 0f; t < 0.9f; t += Time.deltaTime)
                {
                    float k = t / 0.9f;
                    item.position = start + new Vector3(dir * 1.4f * k, 0.06f * Mathf.Abs(Mathf.Sin(t * 30f)), 0f);
                    item.localRotation = Quaternion.Euler(0f, 0f, 8f * Mathf.Sin(t * 30f));
                    SetAlpha(item, Mathf.Clamp01((1f - k) / 0.35f));
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
                for (float t = 0f; t < 0.4f; t += Time.deltaTime)
                {
                    SetAlpha(item, 1f - t / 0.4f);
                    yield return null;
                }
            }
            HalloweenKit.Show(item, false);
            SetAlpha(item, 1f);
            item.localRotation = Quaternion.identity;
            item.localScale = baseScale;
        }

        // ---------- La soluzione ----------

        /// <summary>
        /// Tre colpi di pala sulla X, la X sparisce nella buca, il baule sale con un rimbalzo, si apre: bagliore dorato,
        /// scintille e monete. Poi la festa comune e Win, una volta sola. Rifatta da capo se il livello si spegne a metà.
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            busy = true;
            grip = null;
            StopFx();
            if (digCo != null) { StopCoroutine(digCo); digCo = null; }
            HalloweenKit.Show(infoHolder, false);
            HalloweenKit.Show(digRing, false);
            HideFinds();
            HideTreasure();
            Vector2 at = new Vector2(logic.GroundX, logic.GroundY);
            xMark.position = new Vector3(at.x, at.y, xMark.position.z);
            xMark.localScale = Vector3.one * xGroundScale;
            HalloweenKit.Show(xMark, true);
            SetAlpha(xMark, 1f);

            // tre colpi decisi sulla X
            yield return Jabs(at, 3);
            HalloweenKit.Show(xMark, false);
            Haptics.Medium();
            V80InvisibleTreasureSound.Play(this, "clunk", 0.55f);

            // il baule sale dalla buca (mai fuori dallo schermo) mentre la pala torna a posto
            Vector3 chestAt = new Vector3(Mathf.Clamp(at.x, -ChestMaxX, ChestMaxX), at.y - 0.3f, chest.position.z);
            HalloweenKit.Show(chest, true);
            Vector3 shovelFrom = shovel.position;
            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.35f);
                float e = 1f - (1f - k) * (1f - k);
                chest.position = chestAt + new Vector3(0f, -0.35f * (1f - e), 0f);
                chest.localScale = new Vector3(chestScale.x * Mathf.Lerp(0.5f, 1f, e), chestScale.y * Mathf.Lerp(0.2f, 1.08f, e), chestScale.z);
                shovel.position = Vector3.Lerp(shovelFrom, shovelHome, e);
                shovel.localRotation = Quaternion.Slerp(Quaternion.identity, shovelHomeRot, e);
                yield return null;
            }
            chest.position = chestAt;
            chest.localScale = chestScale;
            shovel.position = shovelHome;
            shovel.localRotation = shovelHomeRot;
            yield return new WaitForSeconds(0.12f);

            // si apre: bagliore, scintille e monete
            HalloweenKit.Show(chestClosed, false);
            HalloweenKit.Show(chestOpen, true);
            FxPop(chest, 1.12f, 0.25f);
            Haptics.Success();
            V80InvisibleTreasureSound.Play(this, "sparkle", 0.55f);
            Vector3 top = chestAt + new Vector3(0f, 1.0f * chestScale.y / 0.75f, 0f);
            if (chestGlow != null) chestGlow.transform.position = new Vector3(top.x, top.y - 0.2f, chestGlow.transform.position.z);
            int nc = coins != null ? coins.Length : 0, ns = sparkles != null ? sparkles.Length : 0;
            for (float t = 0f; t < 0.8f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.8f);
                if (chestGlow != null) chestGlow.color = new Color(1f, 0.85f, 0.35f, 0.8f * Mathf.Clamp01(k * 4f) * (0.85f + 0.15f * Mathf.Sin(t * 18f)));
                for (int i = 0; i < nc; i++)
                {
                    if (coins[i] == null) continue;
                    HalloweenKit.Show(coins[i], true);
                    float dir = (i % 2 == 0 ? -1f : 1f) * (0.35f + 0.18f * (i / 2));
                    float u = Mathf.Clamp01(k * 1.25f - i * 0.04f);
                    coins[i].position = new Vector3(top.x + dir * u, top.y + 1.1f * u - 1.25f * u * u, coins[i].position.z);
                    coins[i].localScale = coinScale * (0.8f + 0.2f * Mathf.Abs(Mathf.Sin(t * 14f + i)));
                }
                for (int i = 0; i < ns; i++)
                {
                    if (sparkles[i] == null) continue;
                    HalloweenKit.Show(sparkles[i], true);
                    float a = i * Mathf.PI * 2f / ns + t * 1.5f;
                    float rad = 0.55f + 0.35f * k;
                    sparkles[i].position = new Vector3(top.x + Mathf.Cos(a) * rad, top.y + 0.1f + Mathf.Sin(a) * rad * 0.7f, sparkles[i].position.z);
                    sparkles[i].localScale = sparkleScale * (0.5f + 0.7f * Mathf.Abs(Mathf.Sin(t * 9f + i * 1.3f)));
                }
                yield return null;
            }

            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            if (logic.TryComplete()) Win(Compliment);
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked || grip != null || logic.Holding) return;
            if (Input.GetKeyDown(KeyCode.T)) TapSand();
            if (Input.GetKeyDown(KeyCode.B)) TapSea();
            if (Input.GetKeyDown(KeyCode.M)) TapMap();
            if (Input.GetKeyDown(KeyCode.X)) TapX();
            if (Input.GetKeyDown(KeyCode.K)) TapPalm();
            if (Input.GetKeyDown(KeyCode.S)) TapShovel();
            if (Input.GetKeyDown(KeyCode.D)) EditorDig(new Vector2(spotX, spotY));
            if (Input.GetKeyDown(KeyCode.E)) EditorDig(new Vector2(-1.2f + 0.3f * (logic.JunkDigs % 3), -2.3f));
            if (Input.GetKeyDown(KeyCode.O)) EditorDig(new Vector2(0f, -4.4f));
            if (Input.GetKeyDown(KeyCode.Q)) EditorMoveX(new Vector2(2.4f, -4.0f));
            if (Input.GetKeyDown(KeyCode.A)) EditorMoveX(map.position);
            if (Input.GetKeyDown(KeyCode.Y)) EditorMoveX(new Vector2(-0.9f, -2.0f));
            if (Input.GetKeyDown(KeyCode.V)) EditorMoveX(new Vector2(1.0f, -2.3f));
            if (Input.GetKeyDown(KeyCode.G)) EditorDig(logic.XOnGround ? new Vector2(logic.GroundX + 0.3f, logic.GroundY - 0.2f) : new Vector2(-0.9f, -2.0f));
            if (Input.GetKeyDown(KeyCode.W)) { EditorMoveX(new Vector2(-0.9f, -2.0f)); EditorDig(new Vector2(-0.8f, -2.1f)); }
            if (Input.GetKeyDown(KeyCode.P)) CancelGrip();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }

        /// <summary>Prende la X dov'è, la porta in <paramref name="to"/> e la lascia.</summary>
        void EditorMoveX(Vector2 to)
        {
            Vector2 start = xMark.position;
            var g = new Grip { kind = Kind.X, start = start, moved = true };
            StartDrag(g);
            if (!g.held) return;
            Drag(g, to);
            End(g);
        }

        /// <summary>Prende la pala, porta la punta in <paramref name="tip"/> e la lascia (si scava lì).</summary>
        void EditorDig(Vector2 tip)
        {
            Vector2 start = shovel.position;
            var g = new Grip { kind = Kind.Shovel, start = start, moved = true };
            StartDrag(g);
            if (!g.held) return;
            Drag(g, tip);
            End(g);
        }
#endif
    }

    /// <summary>Suoni sintetizzati di The Invisible Treasure: pala nella sabbia, carta, "pop", tuffo, "boing", lattina, granchio, "wah-wah", tonfo del baule, scintille d'oro, fruscio.</summary>
    public static class V80InvisibleTreasureSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(294);

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
                case "dig":      // "sciuff" di sabbia: rumore che si chiude in fretta
                    {
                        float lp = 0f;
                        c = Make(kind, 0.18f, t =>
                        {
                            lp += (N() - lp) * 0.35f;
                            return lp * 0.9f * Mathf.Exp(-t * 16f) * Mathf.Min(1f, t * 400f);
                        });
                        break;
                    }
                case "rustle":   // fruscio leggero (foglie, sabbia)
                    {
                        float lp = 0f;
                        c = Make(kind, 0.3f, t =>
                        {
                            lp += (N() - lp) * 0.6f;
                            return lp * 0.35f * Mathf.Sin(t / 0.3f * Mathf.PI) * (0.6f + 0.4f * Mathf.Sin(t * 90f));
                        });
                        break;
                    }
                case "paper":    // carta: crepitio
                    c = Make(kind, 0.22f, t => (rnd.NextDouble() < 0.12 ? N() : N() * 0.15f) * 0.5f * Mathf.Sin(t / 0.22f * Mathf.PI));
                    break;
                case "pop":
                    c = Make(kind, 0.1f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(500f, 900f, t / 0.1f)) * 0.4f * Mathf.Exp(-t * 28f));
                    break;
                case "splash":   // tuffo: rumore che sale e scende con un "plop" basso
                    {
                        float lp = 0f;
                        c = Make(kind, 0.4f, t =>
                        {
                            lp += (N() - lp) * 0.25f;
                            float plop = Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(380f, 160f, Mathf.Clamp01(t / 0.12f))) * Mathf.Exp(-t * 30f);
                            return lp * 0.6f * Mathf.Sin(Mathf.Clamp01(t / 0.4f) * Mathf.PI) + plop * 0.4f;
                        });
                        break;
                    }
                case "boing":
                    c = Make(kind, 0.35f, t => Mathf.Sin(t * 2f * Mathf.PI * (180f + 120f * Mathf.Sin(t * 40f) * Mathf.Exp(-t * 6f))) * 0.4f * Mathf.Exp(-t * 7f));
                    break;
                case "clink":    // metallo: due parziali acute
                    c = Make(kind, 0.3f, t => (Mathf.Sin(t * 2f * Mathf.PI * 1850f) * 0.25f + Mathf.Sin(t * 2f * Mathf.PI * 2730f) * 0.15f) * Mathf.Exp(-t * 14f) * Mathf.Min(1f, t * 600f));
                    break;
                case "pinch":    // granchio: due "clic" secchi
                    c = Make(kind, 0.22f, t =>
                    {
                        float u = t < 0.1f ? t : t - 0.11f;
                        if (u < 0f) return 0f;
                        return Mathf.Sin(u * 2f * Mathf.PI * 2400f) * 0.45f * Mathf.Exp(-u * 70f);
                    });
                    break;
                case "wahwah":   // tromba triste: tre note che scendono
                    {
                        float ph = 0f;
                        c = Make(kind, 0.9f, t =>
                        {
                            float f = t < 0.25f ? 311f : t < 0.5f ? 294f : Mathf.Lerp(277f, 262f, (t - 0.5f) / 0.4f);
                            f += 6f * Mathf.Sin(t * 2f * Mathf.PI * 6f);
                            ph += f / Rate;
                            float saw = (ph - Mathf.Floor(ph)) * 2f - 1f;
                            float u = t < 0.25f ? t : t < 0.5f ? t - 0.25f : t - 0.5f;
                            float env = Mathf.Min(1f, u * 40f) * (t < 0.5f ? 1f : Mathf.Clamp01((0.9f - t) / 0.4f));
                            return saw * 0.22f * env;
                        });
                        break;
                    }
                case "clunk":    // la pala urta il baule: tonfo di legno
                    c = Make(kind, 0.3f, t => (Mathf.Sin(t * 2f * Mathf.PI * 140f) * 0.6f + N() * 0.2f * Mathf.Exp(-t * 60f)) * Mathf.Exp(-t * 14f));
                    break;
                case "sparkle":  // scintille d'oro: arpeggio veloce in salita
                    c = Make(kind, 0.7f, t =>
                    {
                        int n = Mathf.Min(4, (int)(t / 0.07f));
                        float f = 1319f * Mathf.Pow(1.26f, n);
                        float u = t - n * 0.07f;
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.28f * Mathf.Exp(-u * 8f) * Mathf.Min(1f, u * 400f);
                    });
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
            AudioClip clip = AudioClip.Create("v80b_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
