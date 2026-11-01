using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 276 "The Locked Bike" — "Take your bike home!" Una strada: la bici è legata con una grossa catena e un
    /// lucchetto a combinazione (4 rotelle e una toppa) a un palo CORTO, poco più alto della bici: il cartello "P" che
    /// c'era in cima è per terra lì accanto, col pezzo di palo spezzato. Un piccione sta sulla cima del palo. Sul
    /// marciapiede un mazzo di chiavi e una tronchese.
    /// False piste (nessuna azzera i progressi): le chiavi (trascinate sul lucchetto o toccate: "Wrong key." più volte, poi
    /// la chiave di casa, poi un apribottiglie), le rotelle (tocco sul lucchetto: girano su un codice a caso, "0000?
    /// Really?", mai giusto), calciare/scuotere il palo (tocco o strisciata sul palo, o scuotendo il telefono: ondeggia, il
    /// piccione vola via e torna), tirare la bici di lato (la catena la trattiene), sollevarla poco (ricade giù).
    /// Sconfitta (regola del proprietario): la tronchese è un'azione a tre colpi — 1° rimbalza e si sente un fischietto
    /// lontano, 2° si piega e il vigile si affaccia a sinistra ("Hey! What are you doing?", con l'avviso che al prossimo
    /// colpo ci si spiega col giudice), 3° il vigile arriva: Fail("Stealing your own bike? Tell it to the judge.").
    /// Indizio: il cartello caduto ("the pole has no top"), la cima del palo più bassa del manubrio quando si solleva la
    /// bici, il piccione proprio sulla cima.
    /// Soluzione: trascinare la bici in su (generoso: basta 1 unità) oltre la cima del palo: la catena si sfila dall'alto
    /// e resta chiusa sul telaio; poi portare la bici a destra -> la bici se ne va col lucchetto addosso, "THE LOCK WAS
    /// FINE.", "THE POLE WASN'T.", vittoria. Niente microfono. La logica è in Gotcha.Levels.V64.LockedBikeLogic.
    /// Tasti di prova nell'Editor: K chiavi, C combinazione, B tronchese, P calcio al palo (S scuote il telefono), G
    /// piccione, T cartello, X tocco sulla bici, Y strattone, U sollevamento basso, L bici sollevata oltre il palo, R a casa.
    /// </summary>
    public class V75LockedBikeLevel : V62Level
    {
        [Header("Bici")]
        [SerializeField] Transform bike;
        [SerializeField] Collider2D bikeArea;
        [SerializeField] Transform wheelRear;
        [SerializeField] Transform wheelFront;
        [SerializeField] Transform chainPivot;          // l'anello della catena (attorno al palo e al tubo sella)
        [SerializeField] Transform chainBack;           // la metà dietro al palo (sparisce quando la bici è libera)
        [SerializeField] Transform padlock;
        [SerializeField] Collider2D lockArea;           // lucchetto e catena (tocco: le rotelle)
        [SerializeField] TextMesh[] digits;
        [Header("Palo, cartello, piccione")]
        [SerializeField] Transform pole;
        [SerializeField] Collider2D poleArea;
        [SerializeField] Transform poleTop;
        [SerializeField] Transform sign;
        [SerializeField] Collider2D signArea;
        [SerializeField] Transform pigeon;
        [SerializeField] Transform pigeonSit;
        [SerializeField] Transform pigeonFly;
        [SerializeField] Collider2D pigeonArea;
        [Header("Attrezzi")]
        [SerializeField] Transform keys;
        [SerializeField] Collider2D keysArea;
        [SerializeField] Transform cutters;
        [SerializeField] Transform cuttersStraight;
        [SerializeField] Transform cuttersBent;
        [SerializeField] Collider2D cuttersArea;
        [Header("Vigile e festa")]
        [SerializeField] Transform officer;
        [SerializeField] Transform officerPeek;         // dove si affaccia dopo il 2° colpo di tronchese
        [SerializeField] Transform officerIn;           // dove arriva al 3° (l'arresto)
        [SerializeField] SpriteRenderer glow;
        [SerializeField] Transform sparkle;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float TapSlop = 0.25f, TapTime = 0.6f, ShakeLevel = 12f, ShakeCooldown = 1.0f, WheelRadius = 0.5f;
        const string BustedTaunt = "Stealing your own bike? Tell it to the judge.";
        static readonly Vector3 ChainFreeShift = new Vector3(0.19f, 0f, 0f), ChainFreeScale = new Vector3(0.42f, 1.3f, 1f);
        static readonly Vector3 PigeonAwayOffset = new Vector3(3.2f, 2.6f, 0f);

        static readonly string[] WrongKeyLines = { "Wrong key.", "Wrong key.", "Also wrong.", "Nope. Wrong key." };
        static readonly string[] LowLines = { "Clank. The chain slid up the pole... and back down.", "Heavy. Up it went, down it came.", "Clunk. Almost lost a toe." };
        static readonly string[] YankLines = { "It's locked to the pole. That's the point.", "Clank! The chain says no.", "The pole is holding on tight." };
        static readonly string[] CodeLines = { "{0}? Nope.", "{0}? Still locked.", "{0}? Not even close.", "{0}? The lock is laughing." };
        static readonly string[] KickLines = { "BONG. The pole wobbles. The bike doesn't care.", "Ow. The pole wins.", "Kicking a pole. Bold strategy." };
        static readonly string[] ShakeLines = { "You shake the pole. It shakes back.", "Wobble wobble. Still a pole.", "The pole is short, not loose." };
        static readonly string[] PigeonLines = { "Coo. It was sitting right on top of the pole.", "Flap flap. It'll be back. They always come back.", "Coo? It likes that pole top." };
        static readonly string[] SignLines = { "It fell off the pole. The pole has no top.", "\"BIKE PARKING\". The rest of the pole is down here.", "That pole used to be a lot taller." };
        static readonly string[] BikeLines = { "Your bike. Locked to a very short pole.", "Chained up like a sad metal dog.", "So close. So locked." };
        static readonly string[] FreeLines = { "Drag it home. To the right.", "Home is that way. →" };

        enum Kind { None, Bike, Keys, Cutters, Lock, Pole, Pigeon, Sign }

        sealed class Grip
        {
            public Kind kind;
            public Vector2 start, last;
            public Vector3 offset;
            public float t0, lift;
            public bool moved, yanked;
        }

        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        LockedBikeLogic logic;
        Camera cam;
        HwToast toast;
        HwDragger dragger;
        HwDragger.Item keysItem, cuttersItem;
        Grip grip;
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        Coroutine pigeonCo;
        Vector3 keysHome, cuttersHome, chainPivotHome, padlockHome, pigeonHome, officerHidden;
        Quaternion cuttersRot;
        float groundY, shakeReadyAt;
        int keyRound, lowLines, yankLines, codeLines, kickLines, shakeLines, pigeonLines, signLines, bikeLines, freeLines;
        bool busy, finishing, captured, freeTold;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            pigeonCo = null;
            if (!captured)
            {
                captured = true;
                logic = new LockedBikeLogic(bike.position.x);
                groundY = bike.position.y;
                keysHome = keys.position;
                cuttersHome = cutters.position;
                cuttersRot = cutters.localRotation;
                chainPivotHome = chainPivot.localPosition;
                padlockHome = padlock.localPosition;
                pigeonHome = poleTop != null ? new Vector3(poleTop.position.x, poleTop.position.y, pigeon.position.z) : pigeon.position;   // il piccione sta sulla cima del palo
                officerHidden = officer.position;
            }
            logic.Reset();
            ResetV62();
            grip = null;
            busy = finishing = freeTold = false;
            keyRound = lowLines = yankLines = codeLines = kickLines = shakeLines = pigeonLines = signLines = bikeLines = freeLines = 0;
            shakeReadyAt = 0f;
            toast = new HwToast(infoText, infoHolder);
            dragger = new HwDragger(this);
            keysItem = dragger.Add(keys, keysArea, 10);
            cuttersItem = dragger.Add(cutters, cuttersArea, 10);
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            if (dragger.Held != null) dragger.Release();
            // bici e catena
            bike.position = new Vector3(logic.RestX, groundY, bike.position.z);
            bike.localRotation = Quaternion.identity;
            ChainPose(logic.Free);
            ShowCode(logic.LastCode);
            // attrezzi
            dragger.Stop(keysItem);
            keys.position = keysHome;
            keys.localRotation = Quaternion.identity;
            keysItem.Home = keysHome;
            dragger.Stop(cuttersItem);
            cutters.position = cuttersHome;
            cutters.localRotation = cuttersRot;
            cuttersItem.Home = cuttersHome;
            HalloweenKit.Show(cuttersStraight, true);
            HalloweenKit.Show(cuttersBent, false);
            // palo, piccione, vigile
            pole.localRotation = Quaternion.identity;
            PigeonRest();
            officer.position = logic.Busted ? officerIn.position : logic.OfficerWatching ? officerPeek.position : officerHidden;
            HalloweenKit.SetAlpha(glow, 0f);
            HalloweenKit.Show(sparkle, false);
        }

        void ChainPose(bool free)
        {
            HalloweenKit.Show(chainBack, !free);
            chainPivot.localPosition = free ? chainPivotHome + ChainFreeShift : chainPivotHome;
            chainPivot.localScale = free ? ChainFreeScale : Vector3.one;
            padlock.localPosition = free ? padlockHome + ChainFreeShift + new Vector3(0f, -0.05f, 0f) : padlockHome;
            padlock.localRotation = Quaternion.identity;
        }

        void PigeonRest()
        {
            if (pigeonCo != null) { StopCoroutine(pigeonCo); pigeonCo = null; }
            bool away = logic.PigeonAway;
            HalloweenKit.Show(pigeon, !away);
            pigeon.position = pigeonHome;
            pigeon.localScale = Vector3.one;
            HalloweenKit.Show(pigeonSit, true);
            HalloweenKit.Show(pigeonFly, false);
        }

        void ShowCode(int code)
        {
            string s = LockedBikeLogic.CodeLabel(code);
            for (int i = 0; i < digits.Length && i < 4; i++) if (digits[i] != null) digits[i].text = s[i].ToString();
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: le animazioni si fermano, si torna a uno stato valido
            CancelGrip();
            busy = false;
            StopFx();
            if (captured && !finishing && dragger != null && !IsFinished) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || dragger == null) return;
            // spento a metà della festa o dell'arresto: si rifà il finale da capo
            if (logic.Stage == LockedBikeStage.Rolling) { finishing = false; StartCoroutine(Finale()); }
            else if (logic.Stage == LockedBikeStage.Busted) { finishing = false; StartCoroutine(Arrest()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrip(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrip(); }

        /// <summary>Il dito sparisce: la bici legata torna giù al suo posto, quella libera si posa dov'è (mai "a casa").</summary>
        void CancelGrip()
        {
            Grip g = grip;
            grip = null;
            if (g != null && g.kind == Kind.Bike && captured && !finishing && logic != null && !logic.InputLocked)
            {
                if (logic.Free) logic.Park(bike.position.x);
                bike.position = new Vector3(logic.RestX, groundY, bike.position.z);
                bike.localRotation = Quaternion.identity;
            }
            if (dragger != null && dragger.Held != null)
            {
                HwDragger.Item it = dragger.Release();
                if (it != null) dragger.SendHome(it, 0.2f);
            }
        }

        void Update()
        {
            toast?.Tick();
            if (IsFinished || toast == null || finishing) return;
            if (Time.timeScale <= 0f) { CancelGrip(); return; }   // pausa o pubblicità: niente input
            if (logic.Tick(Time.deltaTime) == LockedBikeTick.PigeonBack) PigeonReturns();
            Ambient();
            if (busy) { CancelGrip(); return; }
#if UNITY_EDITOR
            EditorKeys();
            if (busy || finishing) return;
#endif
            if (!logic.InputLocked && grip == null && Time.time >= shakeReadyAt && Inputs.Shake() > ShakeLevel)
            {
                shakeReadyAt = Time.time + ShakeCooldown;
                KickPole(true);
                return;
            }
            ReadPointer();
        }

        /// <summary>Le cose vive: il piccione che dondola la testa, il vigile che batte il piede, il lucchetto che oscilla.</summary>
        void Ambient()
        {
            if (pigeon.gameObject.activeSelf && pigeonCo == null)
                pigeon.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 5f) > 0.7f ? 6f : 0f);
            if (logic.OfficerWatching && !logic.Busted && !busy)
                officer.position = officerPeek.position + new Vector3(0f, Mathf.Abs(Mathf.Sin(Time.time * 3f)) * 0.03f, 0f);
            if (grip == null || grip.kind != Kind.Bike)
                padlock.localRotation = Quaternion.Euler(0f, 0f, 3f * Mathf.Sin(Time.time * 1.7f));
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73/0.74) ----------

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
            grip.last = w;
            if (Vector2.Distance(w, grip.start) > TapSlop) grip.moved = true;
            Move(grip, w);
            if (ended && grip != null) End(grip);
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, t0 = Time.time, kind = Kind.None };
            if (busy || finishing || logic.InputLocked) return g;
            HwDragger.Item it = dragger.TryPick(w);
            if (it != null)
            {
                StopFxOn(it.T);
                g.kind = it == keysItem ? Kind.Keys : Kind.Cutters;
                return g;
            }
            if (pigeon.gameObject.activeSelf && pigeonCo == null && pigeonArea.OverlapPoint(w)) { g.kind = Kind.Pigeon; return g; }
            if (signArea.OverlapPoint(w)) { g.kind = Kind.Sign; return g; }
            if (lockArea.OverlapPoint(w)) { g.kind = Kind.Lock; return g; }
            if (!logic.Free && poleArea.OverlapPoint(w)) { g.kind = Kind.Pole; return g; }
            if (bikeArea.OverlapPoint(w))
            {
                g.kind = Kind.Bike;
                g.offset = bike.position - (Vector3)w;
                StopFxOn(bike);
                Haptics.Tap();
                return g;
            }
            if (logic.Free && poleArea.OverlapPoint(w)) g.kind = Kind.Pole;
            return g;
        }

        void Move(Grip g, Vector2 w)
        {
            switch (g.kind)
            {
                case Kind.Keys:
                case Kind.Cutters:
                    if (g.moved) dragger.MoveHeld(w);
                    break;
                case Kind.Bike:
                    MoveBike(g, w);
                    break;
            }
        }

        void MoveBike(Grip g, Vector2 w)
        {
            float dx = w.x - g.start.x, dy = w.y - g.start.y;
            if (!logic.Free)
            {
                float lift = Mathf.Clamp(dy, 0f, LockedBikeLogic.MaxLift);
                g.lift = Mathf.Max(g.lift, lift);
                float x = logic.HomeX + Mathf.Clamp(dx, -LockedBikeLogic.Slack, LockedBikeLogic.Slack);
                bike.position = new Vector3(x, groundY + lift, bike.position.z);
                bike.localRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(dx, -0.5f, 0.5f) * -4f + lift * 2f);
                // la catena trattiene la bici: tirarla di lato è uno strattone
                if (!g.yanked && Mathf.Abs(dx) > LockedBikeLogic.YankDrag && lift < LockedBikeLogic.ClearLift * 0.5f)
                {
                    g.yanked = true;
                    Yank();
                }
                bool wasAway = logic.PigeonAway;
                LockedBikeLift r = logic.Lift(lift);
                if (!wasAway && logic.PigeonAway) PigeonFlies(false);
                if (r == LockedBikeLift.Cleared)
                {
                    g.offset = bike.position - (Vector3)w;
                    Freed();
                }
                return;
            }
            Vector3 p = (Vector3)w + g.offset;
            float nx = Mathf.Clamp(p.x, -2.3f, 2.7f);
            float ny = Mathf.Clamp(p.y, groundY, groundY + LockedBikeLogic.MaxLift + 0.3f);
            SpinWheels(nx - bike.position.x);
            bike.position = new Vector3(nx, ny, bike.position.z);
            bike.localRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(w.x - g.last.x, -0.2f, 0.2f) * -10f);
        }

        void SpinWheels(float dx)
        {
            float deg = -dx / WheelRadius * Mathf.Rad2Deg;
            wheelRear.localRotation *= Quaternion.Euler(0f, 0f, deg);
            wheelFront.localRotation *= Quaternion.Euler(0f, 0f, deg);
        }

        void End(Grip g)
        {
            grip = null;
            bool tap = !g.moved && Time.time - g.t0 <= TapTime + 0.4f;
            switch (g.kind)
            {
                case Kind.Keys:
                    if (dragger.Held == keysItem) dragger.Release();
                    if (tap || OnLock(keys.position, g.last)) StartCoroutine(TryKeys());
                    else dragger.SendHome(keysItem, 0.25f);
                    break;
                case Kind.Cutters:
                    if (dragger.Held == cuttersItem) dragger.Release();
                    if (tap || OnLock(cutters.position, g.last)) StartCoroutine(Cut());
                    else dragger.SendHome(cuttersItem, 0.25f);
                    break;
                case Kind.Bike: DropBike(g, tap); break;
                case Kind.Lock: if (tap) StartCoroutine(TryCode()); break;
                case Kind.Pole: KickPole(g.moved); break;
                case Kind.Pigeon: if (tap) TapPigeon(); break;
                case Kind.Sign: if (tap) ReadSign(); break;
            }
        }

        /// <summary>Un attrezzo lasciato sul lucchetto, sulla catena o sulla bici (generoso).</summary>
        bool OnLock(Vector3 item, Vector2 finger) =>
            lockArea.OverlapPoint(item) || lockArea.OverlapPoint(finger) || bikeArea.OverlapPoint(finger) || poleArea.OverlapPoint(finger);

        // ---------- La bici ----------

        void DropBike(Grip g, bool tap)
        {
            bike.localRotation = Quaternion.identity;
            if (tap)
            {
                bike.position = new Vector3(logic.RestX, groundY, bike.position.z);
                if (!logic.TapBike()) return;
                FxWiggle(bike, 2f, 0.3f, 25f);
                if (logic.Free) toast.Show(FreeLines[freeLines++ % FreeLines.Length], 1.6f);
                else toast.Show(BikeLines[bikeLines++ % BikeLines.Length], 1.7f);
                return;
            }
            float x = bike.position.x;
            switch (logic.Drop(x, g.lift))
            {
                case LockedBikeDrop.SnapBack:
                    StartCoroutine(SnapBack(g.lift));
                    break;
                case LockedBikeDrop.Parked:
                    StartCoroutine(Settle());
                    if (!freeTold) { freeTold = true; toast.Show("Free! The lock came along. Now take it home →", 2.2f); }
                    else if (x < logic.HomeX - 0.3f) toast.Show("Home is the other way. →", 1.6f);
                    break;
                case LockedBikeDrop.Rolled:
                    StartCoroutine(Finale());
                    break;
            }
        }

        /// <summary>La bici legata ricade giù: tonfo, la catena sbatte contro il palo.</summary>
        IEnumerator SnapBack(float lift)
        {
            busy = true;
            yield return HalloweenKit.Move(bike, new Vector3(logic.HomeX, groundY, bike.position.z), 0.14f);
            bike.localRotation = Quaternion.identity;
            V75LockedBikeSound.Play(this, "thud", 0.6f);
            V75LockedBikeSound.Play(this, "clank", 0.35f);
            Haptics.Light();
            FxShake(bike, 0.03f, 0.2f);
            if (lift >= LockedBikeLogic.MinLift) toast.Show(LowLines[lowLines++ % LowLines.Length], 1.9f);
            busy = false;
        }

        /// <summary>La bici libera si posa a terra dove la si è lasciata.</summary>
        IEnumerator Settle()
        {
            busy = true;
            Vector3 to = new Vector3(logic.ParkX, groundY, bike.position.z);
            SpinWheels(to.x - bike.position.x);
            yield return HalloweenKit.Move(bike, to, 0.15f);
            V75LockedBikeSound.Play(this, "thud", 0.45f);
            FxShake(bike, 0.02f, 0.15f);
            busy = false;
        }

        void Yank()
        {
            if (!logic.Yank()) return;
            V75LockedBikeSound.Play(this, "clank", 0.6f);
            Haptics.Light();
            FxWiggle(padlock, 14f, 0.4f, 30f);
            FxWiggle(pole, 2f, 0.3f, 30f);
            toast.Show(YankLines[yankLines++ % YankLines.Length], 1.8f);
        }

        /// <summary>La catena passa sopra la cima del palo: si sfila e resta chiusa sul telaio (la soluzione, prima metà).</summary>
        void Freed()
        {
            ChainPose(true);
            FxPop(padlock, 1.25f, 0.25f);
            V75LockedBikeSound.Play(this, "clink", 0.7f);
            Haptics.Success();
            toast.Show("Clink! Over the top!", 1.6f);
        }

        // ---------- Le false piste ----------

        IEnumerator TryKeys()
        {
            busy = true;
            Vector3 hole = padlock.position + new Vector3(0.28f, -0.45f, 0f);
            yield return HalloweenKit.Move(keys, hole, 0.2f, 0.2f);
            LockedBikeKey r = logic.TryKey();
            if (r == LockedBikeKey.None) { busy = false; dragger.SendHome(keysItem, 0.2f); yield break; }
            for (int i = 0; i < 3; i++)
            {
                V75LockedBikeSound.Play(this, "jiggle", 0.5f);
                keys.localRotation = Quaternion.Euler(0f, 0f, i % 2 == 0 ? 14f : -10f);
                FxWiggle(padlock, 6f, 0.12f, 40f);
                yield return new WaitForSeconds(0.12f);
            }
            keys.localRotation = Quaternion.identity;
            int i0 = (logic.KeyTries - 1) % LockedBikeLogic.KeyCount;
            if (i0 == 0 && logic.KeyTries > 1) keyRound++;
            string line = r == LockedBikeKey.HouseKey ? "That's your HOUSE key."
                        : r == LockedBikeKey.BottleOpener ? "That's a bottle opener."
                        : keyRound > 0 && i0 == 0 ? "Round two. Still the wrong keys."
                        : WrongKeyLines[i0 % WrongKeyLines.Length];
            toast.Show(line, 1.7f);
            yield return new WaitForSeconds(0.2f);
            dragger.SendHome(keysItem, 0.25f);
            busy = false;
        }

        IEnumerator TryCode()
        {
            busy = true;
            LockedBikeCode r = logic.TryCode();
            if (r == LockedBikeCode.None) { busy = false; yield break; }
            // le rotelle girano
            float end = Time.time + 0.45f;
            while (Time.time < end)
            {
                ShowCode(Random.Range(0, 10000));
                V75LockedBikeSound.Play(this, "click", 0.25f);
                yield return new WaitForSeconds(0.06f);
            }
            ShowCode(logic.LastCode);
            FxWiggle(padlock, 8f, 0.3f, 30f);
            V75LockedBikeSound.Play(this, "clank", 0.35f);
            string label = LockedBikeLogic.CodeLabel(logic.LastCode);
            if (r == LockedBikeCode.Zeros) toast.Show("0000? Really?", 1.7f);
            else if (r == LockedBikeCode.Obvious) toast.Show("1234? Even the thief tried that.", 1.9f);
            else toast.Show(string.Format(CodeLines[codeLines++ % CodeLines.Length], label), 1.7f);
            yield return new WaitForSeconds(0.2f);
            busy = false;
        }

        /// <summary>La tronchese: non taglia mai, e ogni colpo fa avvicinare il vigile (al terzo si perde).</summary>
        IEnumerator Cut()
        {
            busy = true;
            Vector3 chain = padlock.position + new Vector3(0.62f, 0.05f, 0f);
            cutters.localRotation = Quaternion.Euler(0f, 0f, 180f);
            yield return HalloweenKit.Move(cutters, chain, 0.2f, 0.2f);
            LockedBikeCut r = logic.Cut();
            if (r == LockedBikeCut.None) { busy = false; SendCuttersHome(); yield break; }
            // stretta... e niente
            Vector3 s0 = cutters.localScale;
            cutters.localScale = new Vector3(s0.x, s0.y * 0.75f, s0.z);
            V75LockedBikeSound.Play(this, "clank", 0.8f);
            Haptics.Medium();
            FxShake(bike, 0.03f, 0.2f);
            yield return new WaitForSeconds(0.15f);
            cutters.localScale = s0;
            switch (r)
            {
                case LockedBikeCut.Bounce:
                    yield return HalloweenKit.Move(cutters, chain + new Vector3(0.6f, -0.3f, 0f), 0.15f, 0.35f);
                    toast.Show("Hardened steel. Your arms are not.", 1.6f);
                    SendCuttersHome();
                    yield return new WaitForSeconds(1.0f);
                    V75LockedBikeSound.Play(this, "whistle", 0.2f);   // lontano
                    toast.Show("...a police whistle. Far away. For now.", 2.0f);
                    break;
                case LockedBikeCut.Bend:
                    HalloweenKit.Show(cuttersStraight, false);
                    HalloweenKit.Show(cuttersBent, true);
                    V75LockedBikeSound.Play(this, "boing", 0.6f);
                    toast.Show("They bent. The chain didn't.", 1.5f);
                    yield return new WaitForSeconds(0.9f);
                    HalloweenKit.Show(cuttersStraight, true);
                    HalloweenKit.Show(cuttersBent, false);
                    SendCuttersHome();
                    V75LockedBikeSound.Play(this, "whistle", 0.6f);
                    yield return HalloweenKit.Move(officer, officerPeek.position, 0.5f);
                    toast.Show("OFFICER: \"Hey! What are you doing?\"", 2.0f);
                    yield return new WaitForSeconds(2.0f);
                    toast.Show("One more snip and you'll explain it to a judge.", 2.4f);
                    break;
                case LockedBikeCut.Busted:
                    busy = false;
                    StartCoroutine(Arrest());
                    yield break;
            }
            busy = false;
        }

        void SendCuttersHome()
        {
            cutters.localRotation = cuttersRot;
            dragger.SendHome(cuttersItem, 0.25f);
        }

        /// <summary>Terzo colpo di tronchese: il vigile arriva e la partita è persa (una volta sola, niente festa dopo).</summary>
        IEnumerator Arrest()
        {
            finishing = true;
            busy = false;
            CancelGrip();
            StopFx();
            HalloweenKit.Show(cuttersStraight, true);
            HalloweenKit.Show(cuttersBent, false);
            cutters.localRotation = cuttersRot;
            V75LockedBikeSound.Play(this, "whistle", 0.9f);
            Haptics.Heavy();
            yield return HalloweenKit.Move(officer, officerIn.position, 0.6f);
            FxShake(officer, 0.03f, 0.3f);
            toast.Show("OFFICER: \"That's it. You're coming with me.\"", 2.2f);
            yield return new WaitForSeconds(1.3f);
            if (logic.TryFail()) Fail(BustedTaunt);
        }

        void KickPole(bool shake)
        {
            LockedBikePole r = logic.KickPole();
            if (r == LockedBikePole.None) return;
            FxWiggle(pole, 7f, 0.6f, 22f);
            FxShake(bike, 0.02f, 0.2f);
            V75LockedBikeSound.Play(this, "bong", 0.6f);
            Haptics.Light();
            if (r == LockedBikePole.WobblePigeonFlew) PigeonFlies(true);
            if (shake) toast.Show(ShakeLines[shakeLines++ % ShakeLines.Length], 1.7f);
            else toast.Show(KickLines[kickLines++ % KickLines.Length], 1.8f);
        }

        void TapPigeon()
        {
            if (!logic.TapPigeon()) return;
            PigeonFlies(true);
            toast.Show(PigeonLines[pigeonLines++ % PigeonLines.Length], 2.0f);
        }

        void ReadSign()
        {
            if (!logic.ReadSign()) return;
            FxWiggle(sign, 4f, 0.35f, 25f);
            V75LockedBikeSound.Play(this, "bong", 0.25f);
            toast.Show(SignLines[signLines++ % SignLines.Length], 2.2f);
        }

        // ---------- Il piccione ----------

        void PigeonFlies(bool coo)
        {
            if (pigeonCo != null) StopCoroutine(pigeonCo);
            pigeonCo = StartCoroutine(PigeonAway(coo));
        }

        IEnumerator PigeonAway(bool coo)
        {
            HalloweenKit.Show(pigeon, true);
            HalloweenKit.Show(pigeonSit, false);
            HalloweenKit.Show(pigeonFly, true);
            pigeon.localRotation = Quaternion.identity;
            V75LockedBikeSound.Play(this, "flap", 0.5f);
            if (coo) V75LockedBikeSound.Play(this, "coo", 0.4f);
            pigeon.localScale = new Vector3(-1f, 1f, 1f);   // si gira e vola a destra
            yield return HalloweenKit.Move(pigeon, pigeonHome + PigeonAwayOffset, 0.7f, 0.4f);
            HalloweenKit.Show(pigeon, false);
            pigeon.localScale = Vector3.one;
            pigeonCo = null;
        }

        void PigeonReturns()
        {
            if (pigeonCo != null) StopCoroutine(pigeonCo);
            pigeonCo = StartCoroutine(PigeonBack());
        }

        IEnumerator PigeonBack()
        {
            HalloweenKit.Show(pigeon, true);
            HalloweenKit.Show(pigeonSit, false);
            HalloweenKit.Show(pigeonFly, true);
            pigeon.localScale = Vector3.one;
            pigeon.position = pigeonHome + PigeonAwayOffset;
            V75LockedBikeSound.Play(this, "flap", 0.35f);
            yield return HalloweenKit.Move(pigeon, pigeonHome, 0.7f, 0.3f);
            HalloweenKit.Show(pigeonSit, true);
            HalloweenKit.Show(pigeonFly, false);
            V75LockedBikeSound.Play(this, "coo", 0.35f);
            pigeonCo = null;
        }

        // ---------- La soluzione ----------

        /// <summary>La bici libera, col lucchetto ancora chiuso sul telaio, se ne va a destra; il piccione torna sul palo.</summary>
        IEnumerator Finale()
        {
            finishing = true;
            busy = false;
            CancelGrip();
            StopFx();
            ChainPose(true);
            bike.localRotation = Quaternion.identity;
            // il piccione torna sulla cima del palo (ora tutta sua)
            logic.PigeonHome();
            if (pigeonCo != null) { StopCoroutine(pigeonCo); pigeonCo = null; }
            HalloweenKit.Show(pigeon, true);
            pigeon.position = pigeonHome;
            pigeon.localScale = Vector3.one;
            HalloweenKit.Show(pigeonSit, true);
            HalloweenKit.Show(pigeonFly, false);
            if (bike.position.y > groundY + 0.01f) yield return HalloweenKit.Move(bike, new Vector3(bike.position.x, groundY, bike.position.z), 0.12f);
            V75LockedBikeSound.Play(this, "bell", 0.6f);
            toast.Show("THE LOCK WAS FINE.", 1.6f);
            // via, a destra
            float t = 0f, x0 = bike.position.x, x1 = 4.6f, dur = 1.3f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                float x = Mathf.Lerp(x0, x1, k * k * (3f - 2f * k));
                SpinWheels(x - bike.position.x);
                bike.position = new Vector3(x, groundY + 0.02f * Mathf.Abs(Mathf.Sin(t * 18f)), bike.position.z);
                padlock.localRotation = Quaternion.Euler(0f, 0f, 12f * Mathf.Sin(t * 14f));
                yield return null;
            }
            V75LockedBikeSound.Play(this, "coo", 0.5f);
            FxPop(pigeon, 1.2f, 0.3f);
            if (sparkle != null) { HalloweenKit.Show(sparkle, true); FxPop(sparkle, 1.6f, 0.4f); }
            if (glow != null) StartCoroutine(HalloweenKit.Fade(glow, 0.8f, 0.25f));
            toast.Show("THE POLE WASN'T.", 2.2f);
            yield return Celebrate(1.2f);
            if (logic.TryComplete()) Win();
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked) return;
            if (Input.GetKeyDown(KeyCode.K)) { StartCoroutine(TryKeys()); return; }
            if (Input.GetKeyDown(KeyCode.C)) { StartCoroutine(TryCode()); return; }
            if (Input.GetKeyDown(KeyCode.B)) { StartCoroutine(Cut()); return; }
            if (Input.GetKeyDown(KeyCode.P)) KickPole(false);
            if (Input.GetKeyDown(KeyCode.G)) TapPigeon();
            if (Input.GetKeyDown(KeyCode.T)) ReadSign();
            if (Input.GetKeyDown(KeyCode.X)) DropBike(new Grip { kind = Kind.Bike }, true);
            if (Input.GetKeyDown(KeyCode.Y)) Yank();
            if (Input.GetKeyDown(KeyCode.U) && logic.Drop(logic.HomeX, 0.5f) == LockedBikeDrop.SnapBack) StartCoroutine(SnapBack(0.5f));
            if (Input.GetKeyDown(KeyCode.L) && logic.Lift(LockedBikeLogic.ClearLift) == LockedBikeLift.Cleared) { Freed(); RestPose(); }
            if (Input.GetKeyDown(KeyCode.R) && logic.Free && logic.Drop(LockedBikeLogic.ExitX + 0.1f, 0f) == LockedBikeDrop.Rolled) StartCoroutine(Finale());
        }
#endif
    }

    /// <summary>Suoni sintetizzati del livello 276: catena, lucchetto, chiavi, rotelle, palo, piccione, fischietto, campanello.</summary>
    public static class V75LockedBikeSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(276);

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
                case "clank":    // catena d'acciaio che sbatte
                    c = Make(kind, 0.3f, t => (Mathf.Sin(t * 2f * Mathf.PI * 1180f) * 0.5f + Mathf.Sin(t * 2f * Mathf.PI * 1730f) * 0.35f) * Mathf.Exp(-t * 14f) + N() * 0.3f * Mathf.Exp(-t * 60f));
                    break;
                case "clink":    // la catena che si sfila dalla cima del palo
                    c = Make(kind, 0.5f, t => (Mathf.Sin(t * 2f * Mathf.PI * 2200f) + Mathf.Sin(t * 2f * Mathf.PI * 3300f) * 0.5f) * 0.3f * Mathf.Exp(-t * 7f));
                    break;
                case "thud":     // la bici che ricade
                    c = Make(kind, 0.25f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(120f, 60f, t / 0.25f)) * 0.7f * Mathf.Exp(-t * 16f) + N() * 0.2f * Mathf.Exp(-t * 40f));
                    break;
                case "bong":     // il palo calciato
                    c = Make(kind, 0.7f, t => (Mathf.Sin(t * 2f * Mathf.PI * 420f) + 0.6f * Mathf.Sin(t * 2f * Mathf.PI * 1090f)) * 0.35f * Mathf.Exp(-t * 5f) * (1f + 0.3f * Mathf.Sin(t * 2f * Mathf.PI * 6f)));
                    break;
                case "jiggle":   // la chiave che gira a vuoto
                    c = Make(kind, 0.1f, t => N() * 0.5f * Mathf.Exp(-t * 50f) + Mathf.Sin(t * 2f * Mathf.PI * 2600f) * 0.2f * Mathf.Exp(-t * 40f));
                    break;
                case "click":    // una rotella che scatta
                    c = Make(kind, 0.04f, t => N() * 0.5f * Mathf.Exp(-t * 150f));
                    break;
                case "boing":    // la tronchese che si piega
                    c = Make(kind, 0.4f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(380f, 160f, t / 0.4f) * (1f + 0.08f * Mathf.Sin(t * 2f * Mathf.PI * 18f))) * 0.4f * Mathf.Exp(-t * 6f));
                    break;
                case "coo":      // il piccione: "cuuu-cu"
                    c = Make(kind, 0.55f, t => Mathf.Sin(t * 2f * Mathf.PI * (t < 0.35f ? Mathf.Lerp(320f, 260f, t / 0.35f) : 300f)) * 0.35f * (t < 0.35f ? Mathf.Sin(t / 0.35f * Mathf.PI) : Mathf.Sin((t - 0.35f) / 0.2f * Mathf.PI)));
                    break;
                case "flap":     // battito d'ali
                    c = Make(kind, 0.4f, t => N() * 0.5f * Mathf.Max(0f, Mathf.Sin(t * 2f * Mathf.PI * 12f)) * Mathf.Exp(-t * 4f));
                    break;
                case "whistle":  // il fischietto del vigile: trillo
                    c = Make(kind, 0.6f, t => (Mathf.Sin(t * 2f * Mathf.PI * 2800f * (1f + 0.04f * Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * 30f)))) * 0.35f + N() * 0.05f) * Mathf.Min(1f, t * 30f) * Mathf.Min(1f, (0.6f - t) * 10f));
                    break;
                case "bell":     // il campanello della bici: drin drin
                    c = Make(kind, 0.6f, t => { float u = t % 0.25f; return (Mathf.Sin(u * 2f * Mathf.PI * 2100f) + 0.5f * Mathf.Sin(u * 2f * Mathf.PI * 3150f)) * 0.3f * Mathf.Exp(-u * 12f); });
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
            AudioClip clip = AudioClip.Create("v75b_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
