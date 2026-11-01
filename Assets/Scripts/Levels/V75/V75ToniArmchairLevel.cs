using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 275 "Toni's Armchair" (un livello di Toni) — "Get your armchair back!" Il soggiorno accogliente, di sera:
    /// due poltrone identiche una accanto all'altra (a sinistra la TUA preferita, col telecomando sul bracciolo e la
    /// tazza sul tavolino; a destra una uguale, vuota), la TV col pre-partita e il conto alla rovescia, e sul tappeto il
    /// cuscino "YOU" (sei tu), la coperta, l'aspirapolvere, la scatola dei biscotti e la paperella che squittisce. Toni
    /// (il solito rig: ToniPug) dorme raggomitolato sulla TUA poltrona e russa.
    /// False piste (nessuna azzera i progressi): trascinare/spingere Toni (diventa piatto e pesante, "Toni weighs 400 kg
    /// when he doesn't want to move.", e torna giù), il biscotto (se lo mangia senza aprire gli occhi), la paperella (gli
    /// si muove un orecchio, nient'altro), l'aspirapolvere (russa più forte e brontola), il tocco (brontola nel sonno),
    /// "YOU" sulla tua poltrona (occupata), la coperta addosso (ancora più comodo), tazza/telecomando sul cane.
    /// Indizio: ogni oggetto appoggiato sulla poltrona di DESTRA fa aprire a Toni un occhio solo, che lo fissa sospettoso:
    /// "...is that seat better than mine?".
    /// Soluzione: "YOU" sulla poltrona di destra (trascinandolo, o toccando la poltrona vuota) -> Toni si sveglia, salta
    /// giù e si prende il tuo nuovo posto buttandoti sul tappeto -> la tua poltrona è libera -> "YOU" sulla poltrona di
    /// sinistra -> col telecomando, la partita comincia: "PUGS ALWAYS WANT YOUR SEAT." -> "ENJOY IT WHILE IT LASTS." ->
    /// vittoria.
    /// Sconfitta (sempre annunciata): il conto alla rovescia sulla TV ("THE MATCH STARTS IN 60", gli ultimi 10 s pulsa
    /// rosso; a 0 senza di te sulla tua poltrona: "You missed the match. Toni didn't.") e l'aspirapolvere da tre strike
    /// (1° "Toni growls.", 2° "Toni is getting VERY grumpy.", 3° Toni si sveglia furioso e si prende il telecomando:
    /// "Never vacuum a sleeping pug."). Fail una volta sola; dopo non parte nient'altro.
    /// Niente microfono. Pausa/focus perso: il dito si annulla e la scena torna a una posa ricavata dalla logica (Toni
    /// a metà salto arriva subito sulla poltrona di destra; un finale o una sconfitta interrotti si rifanno da capo).
    /// La logica è in Gotcha.Levels.V64.ToniArmchairLogic (senza Unity).
    /// Tasti di prova nell'Editor: R siediti a destra, L siediti a sinistra, P spingi Toni, T biscotto, Q paperella,
    /// V aspirapolvere, B coperta sulla poltrona di destra (indizio), C coperta addosso a Toni, G tocca Toni,
    /// K conto alla rovescia a 5 secondi.
    /// </summary>
    public class V75ToniArmchairLevel : V62Level
    {
        [Header("Toni")]
        [SerializeField] ToniPug toni;
        [SerializeField] Transform toniEar;             // figlio di Toni: l'orecchio che si muove (sovrapposizione)
        [SerializeField] Transform toniLid;             // figlio di Toni: la palpebra chiusa (un occhio solo aperto)
        [Header("Poltrone")]
        [SerializeField] Transform leftSeat;            // piano di seduta della TUA poltrona
        [SerializeField] Transform rightSeat;           // piano di seduta dell'altra
        [SerializeField] Collider2D leftChairArea;
        [SerializeField] Collider2D rightChairArea;
        [Header("Tu")]
        [SerializeField] Transform you;                 // il cuscino "YOU"
        [SerializeField] Collider2D youArea;
        [Header("Oggetti")]
        [SerializeField] Transform treats;
        [SerializeField] Collider2D treatsArea;
        [SerializeField] Transform cookie;
        [SerializeField] Transform squeaky;
        [SerializeField] Collider2D squeakyArea;
        [SerializeField] Transform vacuum;
        [SerializeField] Collider2D vacuumArea;
        [SerializeField] Transform noise;               // le onde del rumore (figlio dell'aspirapolvere)
        [SerializeField] Transform blanket;
        [SerializeField] Collider2D blanketArea;
        [SerializeField] Transform mug;
        [SerializeField] Collider2D mugArea;
        [SerializeField] Transform remote;
        [SerializeField] Collider2D remoteArea;
        [Header("TV")]
        [SerializeField] Collider2D tvArea;
        [SerializeField] TextMesh tvTitle;              // "THE MATCH STARTS IN" / "KICK-OFF!"
        [SerializeField] TextMesh tvNumber;             // i secondi
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float TapSlop = 0.6f, MoveSlop = 0.2f, TapTime = 1.2f, MaxStep = 0.1f;
        const float SeatedScale = 0.85f, SnoreEvery = 5f;

        static readonly string[] PushLines = { "Toni weighs 400 kg when he doesn't want to move.", "He's not a pug. He's a paperweight.", "You pushed. He got heavier.", "Liquid when you push. Rock when you pull." };
        static readonly string[] TreatLines = { "He ate it. Without opening his eyes.", "Sleep-chewing. A pug superpower.", "Still asleep. Slightly rounder.", "Thank you, said nobody. Snore." };
        static readonly string[] SqueakLines = { "One ear twitched. That's it.", "Squeak. Twitch. Snore.", "The ear is awake. The rest of Toni isn't." };
        static readonly string[] ToniSays = { "Hrrmph...", "Mmmnope.", "Five more hours...", "Mrrf. Go 'way." };
        static readonly string[] OccupiedLines = { "Occupied. Very occupied.", "Your seat. His nap.", "He was here first. Apparently." };
        static readonly string[] CoverLines = { "Now he'll NEVER get up.", "Cosier. Heavier. Happier.", "You tucked in the enemy." };
        static readonly string[] YouLines = { "That's you. You need a seat.", "Yours, ideally. The left one.", "You. Standing. Before the match. Tragic." };
        static readonly string[] RemoteLines = { "Pre-match show. Kick-off soon!", "Volume up. Toni doesn't care.", "Every channel: the countdown." };
        static readonly string[] TvLines = { "Big match tonight. Don't miss it!", "Get comfy. In YOUR armchair.", "The countdown is real." };
        static readonly string[] SeatLines = { "...is that seat better than mine?", "...what's on THAT seat?", "...hmm. Suspicious seat." };

        enum Kind { None, Item, Toni, LeftChair, RightChair, Tv, Other }

        sealed class Grip
        {
            public Kind kind;
            public Vector2 start, last;
            public float t0, maxDist;
            public bool moved;
        }

        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        readonly ToniArmchairLogic logic = new ToniArmchairLogic();
        Camera cam;
        HwToast toast;
        HwDragger dragger;
        HwDragger.Item youItem, treatsItem, squeakyItem, vacuumItem, blanketItem, mugItem, remoteItem, seatItem;
        readonly Dictionary<HwDragger.Item, ToniArmchairItem> kinds = new Dictionary<HwDragger.Item, ToniArmchairItem>();
        Grip grip;
        Vector3 toniLeftPos, toniRightPos, toniScale, youScale, tvNumberScale, earRot;
        Vector3 youHome, treatsHome, squeakyHome, vacuumHome, blanketHome, mugHome, remoteHome;
        Quaternion remoteRot, vacuumRot;
        Color tvColor;
        float zzzRate, nextSnore;
        Coroutine earCo;
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        int pushLines, treatLines, squeakLines, toniSays, occupiedLines, coverLines, youLines, remoteLines, tvLines, seatLines;
        bool busy, finishing, captured;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            earCo = null;
            logic.Reset();
            ResetV62();
            grip = null;
            seatItem = null;
            busy = finishing = false;
            pushLines = treatLines = squeakLines = toniSays = occupiedLines = coverLines = youLines = remoteLines = tvLines = seatLines = 0;
            toast = new HwToast(infoText, infoHolder);
            if (!captured)
            {
                captured = true;
                toniLeftPos = toni.transform.localPosition;
                toniRightPos = toniLeftPos + toni.transform.parent.InverseTransformVector(rightSeat.position - leftSeat.position);
                toniScale = new Vector3(Mathf.Abs(toni.transform.localScale.x), toni.transform.localScale.y, toni.transform.localScale.z);
                youScale = you.localScale;
                youHome = you.position;
                treatsHome = treats.position;
                squeakyHome = squeaky.position;
                vacuumHome = vacuum.position;
                blanketHome = blanket.position;
                mugHome = mug.position;
                remoteHome = remote.position;
                remoteRot = remote.localRotation;
                vacuumRot = vacuum.localRotation;
                tvNumberScale = tvNumber.transform.localScale;
                tvColor = tvNumber.color;
                earRot = toniEar.localEulerAngles;
                zzzRate = toni.zzz != null ? toni.zzz.puffsPerSecond : 1f;
            }
            dragger = new HwDragger(this);
            kinds.Clear();
            youItem = Add(you, youArea, youHome, 3, null);
            treatsItem = Add(treats, treatsArea, treatsHome, 2, ToniArmchairItem.Treats);
            squeakyItem = Add(squeaky, squeakyArea, squeakyHome, 2, ToniArmchairItem.Squeaky);
            vacuumItem = Add(vacuum, vacuumArea, vacuumHome, 2, ToniArmchairItem.Vacuum);
            blanketItem = Add(blanket, blanketArea, blanketHome, 2, ToniArmchairItem.Blanket);
            mugItem = Add(mug, mugArea, mugHome, 2, ToniArmchairItem.Mug);
            remoteItem = Add(remote, remoteArea, remoteHome, 2, ToniArmchairItem.Remote);
            nextSnore = Time.time + 1.5f;
            RestPose();
        }

        HwDragger.Item Add(Transform t, Collider2D area, Vector3 home, int priority, ToniArmchairItem? kind)
        {
            HwDragger.Item it = dragger.Add(t, area, priority);
            it.Home = home;
            if (kind.HasValue) kinds[it] = kind.Value;
            return it;
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            ToniRest();
            foreach (HwDragger.Item it in new[] { youItem, treatsItem, squeakyItem, vacuumItem, blanketItem, mugItem, remoteItem })
            {
                dragger.Stop(it);
                it.T.position = it.Home;
                it.Enabled = true;
            }
            seatItem = null;
            YouRest();
            remote.localRotation = remoteRot;
            vacuum.localRotation = vacuumRot;
            HalloweenKit.Show(noise, false);
            HalloweenKit.Show(cookie, false);
            ShowClock();
        }

        /// <summary>Toni dove dice la logica: sdraiato e addormentato sulla sua poltrona, testa a sinistra.</summary>
        void ToniRest()
        {
            StopFxOn(toni.transform);
            if (earCo != null) { StopCoroutine(earCo); earCo = null; }
            toni.Stand(false);
            toni.transform.localScale = toniScale;
            toni.Face(false);
            toni.transform.localRotation = Quaternion.identity;
            toni.transform.localPosition = logic.ToniAt == ToniArmchairSpot.Right ? toniRightPos : toniLeftPos;
            toni.Alert(false);
            toni.MouthOpen(false);
            toni.TongueOut(false);
            toni.LookTarget = null;
            toni.LookPoint = null;
            if (toni.zzz != null) toni.zzz.puffsPerSecond = zzzRate;
            if (toni.hearts != null) toni.hearts.Stop();
            if (toni.crumbs != null) toni.crumbs.Stop();
            if (toni.bubble != null) toni.bubble.Hide();
            toni.Sleep();
            HalloweenKit.Show(toniLid, false);
            HalloweenKit.Show(toniEar, false);
            toniEar.localEulerAngles = earRot;
        }

        /// <summary>Il cuscino "YOU": sul tappeto, o seduto sulla poltrona che dice la logica.</summary>
        void YouRest()
        {
            if (logic.YouAt == ToniArmchairSpot.Floor)
            {
                you.position = youHome;
                you.localScale = youScale;
            }
            else
            {
                you.position = SeatPoint(logic.YouAt);
                you.localScale = youScale * SeatedScale;
            }
            you.localRotation = Quaternion.identity;
            dragger.SetBaseScale(youItem, you.localScale);
        }

        Vector3 SeatPoint(ToniArmchairSpot s)
        {
            Transform seat = s == ToniArmchairSpot.Right ? rightSeat : leftSeat;
            Vector3 p = seat.position + new Vector3(0f, 0.33f, 0f);
            p.z = you.position.z;
            return p;
        }

        /// <summary>Il conto alla rovescia sulla TV (gli ultimi secondi pulsa rosso).</summary>
        void ShowClock()
        {
            if (tvTitle == null || tvNumber == null) return;
            bool kickOff = logic.Stage == ToniArmchairStage.Settling || logic.Stage == ToniArmchairStage.Done
                || (logic.IsFailed && logic.FailReason == ToniArmchairFail.Match);
            if (kickOff)
            {
                tvTitle.text = "KICK-OFF!";
                HalloweenKit.Show(tvNumber, false);
                return;
            }
            tvTitle.text = "THE MATCH\nSTARTS IN";
            HalloweenKit.Show(tvNumber, true);
            tvNumber.text = logic.SecondsShown.ToString();
            if (logic.Urgent)
            {
                float k = Mathf.Abs(Mathf.Sin(Time.time * 6f));
                tvNumber.color = Color.Lerp(new Color(1f, 0.25f, 0.2f), new Color(1f, 0.6f, 0.55f), k);
                tvNumber.transform.localScale = tvNumberScale * (1f + 0.12f * k);
            }
            else
            {
                tvNumber.color = tvColor;
                tvNumber.transform.localScale = tvNumberScale;
            }
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            CancelGrip();
            busy = false;
            StopFx();
            earCo = null;
            if (!captured || dragger == null || IsFinished || finishing) return;
            // Toni a metà salto: arriva subito sulla poltrona di destra
            if (logic.ToniMoving) logic.ToniArrived();
            RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || dragger == null) return;
            // spento a metà del finale o della sconfitta: si rifà da capo
            if (logic.Stage == ToniArmchairStage.Settling) { finishing = false; StartCoroutine(Finale()); }
            else if (logic.IsFailed && !logic.FailReported) { finishing = false; StartCoroutine(FailRun()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrip(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrip(); }

        void CancelGrip()
        {
            Grip g = grip;
            grip = null;
            if (captured && g != null && g.kind == Kind.Toni && !finishing && toni != null && !busy)
            {
                toni.transform.localScale = toniScale;
                toni.transform.localPosition = logic.ToniAt == ToniArmchairSpot.Right ? toniRightPos : toniLeftPos;
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
            if (IsFinished || toast == null) return;
            if (!finishing)
            {
                // il conto alla rovescia (anche durante le animazioni: la TV non aspetta nessuno)
                if (logic.Tick(Mathf.Min(Time.deltaTime, MaxStep))) { StartFail(); return; }
            }
            ShowClock();
            if (finishing) return;
            if (Time.timeScale <= 0f) { CancelGrip(); return; }
            Ambient();
            if (busy) { CancelGrip(); return; }
#if UNITY_EDITOR
            EditorKeys();
            if (busy || finishing) return;
#endif
            ReadPointer();
        }

        void Ambient()
        {
            if (Time.time < nextSnore) return;
            nextSnore = Time.time + SnoreEvery + Random.Range(0f, 2f);
            if (!busy && !toni.IsAwake) Sfx.Play(this, "snore", 0.4f);
        }

        // ---------- Effetti che non si accavallano ----------

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
            float dist = Vector2.Distance(w, grip.start);
            grip.maxDist = Mathf.Max(grip.maxDist, dist);
            if (dist > MoveSlop) grip.moved = true;
            Drag(grip);
            if (grip != null && ended) End(grip);
        }

        /// <summary>Toni (sdraiato sulla sua poltrona) sotto il dito: area generosa attorno al corpo.</summary>
        bool OnToni(Vector2 w)
        {
            Vector2 c = (Vector2)toni.transform.position + new Vector2(0f, 0.12f);
            Vector2 d = w - c;
            return Mathf.Abs(d.x) <= 0.95f && d.y >= -0.45f && d.y <= 0.6f;
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, t0 = Time.time, kind = Kind.Other };
            if (busy || finishing || logic.InputLocked) { g.kind = Kind.None; return g; }
            if (dragger.TryPick(w) != null)
            {
                g.kind = Kind.Item;
                StopFxOn(dragger.Held.T);
                if (dragger.Held == seatItem) seatItem = null;   // ripreso dalla poltrona di destra
                return g;
            }
            if (OnToni(w)) { g.kind = Kind.Toni; StopFxOn(toni.transform); return g; }
            if (leftChairArea.OverlapPoint(w)) { g.kind = Kind.LeftChair; return g; }
            if (rightChairArea.OverlapPoint(w)) { g.kind = Kind.RightChair; return g; }
            if (tvArea.OverlapPoint(w)) { g.kind = Kind.Tv; return g; }
            return g;
        }

        void Drag(Grip g)
        {
            switch (g.kind)
            {
                case Kind.Item:
                    if (dragger.Held != null && g.moved) dragger.MoveHeld(g.last);
                    break;
                case Kind.Toni:
                    if (g.moved)
                    {
                        // si allunga un pochino verso il dito e si appiattisce: pesantissimo
                        Vector2 d = Vector2.ClampMagnitude(g.last - g.start, 1f) * 0.22f;
                        Vector3 home = logic.ToniAt == ToniArmchairSpot.Right ? toniRightPos : toniLeftPos;
                        toni.transform.localPosition = home + new Vector3(d.x, Mathf.Min(0f, d.y) * 0.3f, 0f);
                        float flat = Mathf.Clamp01(g.maxDist / 0.8f);
                        toni.transform.localScale = new Vector3(toniScale.x * (1f + 0.14f * flat), toniScale.y * (1f - 0.18f * flat), toniScale.z);
                    }
                    break;
            }
        }

        void End(Grip g)
        {
            grip = null;
            bool tap = Vector2.Distance(g.last, g.start) < TapSlop && Time.time - g.t0 <= TapTime;
            switch (g.kind)
            {
                case Kind.Item: DropItem(g); break;
                case Kind.Toni:
                    if (g.moved && g.maxDist >= TapSlop * 0.5f) StartCoroutine(PushRun());
                    else
                    {
                        toni.transform.localScale = toniScale;
                        toni.transform.localPosition = logic.ToniAt == ToniArmchairSpot.Right ? toniRightPos : toniLeftPos;
                        if (tap) TapToni();
                    }
                    break;
                case Kind.LeftChair: if (tap) Seat(ToniArmchairSpot.Left); break;
                case Kind.RightChair: if (tap) Seat(ToniArmchairSpot.Right); break;
                case Kind.Tv:
                    if (tap)
                    {
                        Sfx.Play(this, "tick", 0.5f);
                        toast.Show(TvLines[tvLines++ % TvLines.Length], 1.8f);
                    }
                    break;
            }
        }

        // ---------- Gli oggetti ----------

        void DropItem(Grip g)
        {
            HwDragger.Item it = dragger.Release();
            if (it == null) return;
            if (!g.moved) { TapItem(it); return; }
            Vector2 at = it.T.position;
            bool onToni = OnToni(g.last) || OnToni(at);
            if (it == youItem)
            {
                if (leftChairArea.OverlapPoint(g.last) || leftChairArea.OverlapPoint(at)) { Seat(ToniArmchairSpot.Left); return; }
                if (rightChairArea.OverlapPoint(g.last) || rightChairArea.OverlapPoint(at)) { Seat(ToniArmchairSpot.Right); return; }
                dragger.SendHome(it, 0.3f);
                return;
            }
            ToniArmchairItem kind = kinds[it];
            if (onToni)
            {
                switch (kind)
                {
                    case ToniArmchairItem.Treats: StartCoroutine(TreatRun()); return;
                    case ToniArmchairItem.Squeaky: dragger.SendHome(it, 0.3f); Squeak(); return;
                    case ToniArmchairItem.Vacuum: StartCoroutine(VacuumRun()); return;
                    case ToniArmchairItem.Blanket: StartCoroutine(CoverRun()); return;
                    default:
                        dragger.SendHome(it, 0.3f);
                        toast.Show(kind == ToniArmchairItem.Mug ? "Not on the dog." : "He's not a coffee table.", 1.5f);
                        return;
                }
            }
            bool onRight = rightChairArea.OverlapPoint(g.last) || rightChairArea.OverlapPoint(at);
            bool onLeft = leftChairArea.OverlapPoint(g.last) || leftChairArea.OverlapPoint(at);
            if (onRight && logic.ToniAt != ToniArmchairSpot.Right)
            {
                ToniArmchairPlace r = logic.PlaceOnRight(kind);
                if (r == ToniArmchairPlace.Suspicious) { PutOnSeat(it); StartCoroutine(StareRun(it)); return; }
                dragger.SendHome(it, 0.3f);
                return;
            }
            if (onLeft && logic.LeftFree)
            {
                toast.Show("That's YOUR seat. Sit in it yourself!", 1.8f);
                dragger.SendHome(it, 0.3f);
                return;
            }
            dragger.SendHome(it, 0.3f);
        }

        void TapItem(HwDragger.Item it)
        {
            dragger.SendHome(it, 0.15f);
            if (it == youItem)
            {
                FxPop(you, 1.08f, 0.25f);
                toast.Show(YouLines[youLines++ % YouLines.Length], 1.8f);
                return;
            }
            switch (kinds[it])
            {
                case ToniArmchairItem.Squeaky: Squeak(); return;
                case ToniArmchairItem.Vacuum: StartCoroutine(VacuumRun()); return;
                case ToniArmchairItem.Remote:
                    if (!logic.TapRemote()) return;
                    Sfx.Play(this, "tick", 0.5f);
                    FxPop(tvNumber.transform.parent, 1.06f, 0.2f);
                    toast.Show(RemoteLines[remoteLines++ % RemoteLines.Length], 1.8f);
                    return;
                case ToniArmchairItem.Treats:
                    Sfx.Play(this, "pop", 0.4f);
                    FxWiggle(treats, 8f, 0.35f, 26f);
                    toast.Show("Toni's biscuits. His favourite. (Even asleep.)", 1.8f);
                    return;
                case ToniArmchairItem.Blanket:
                    FxWiggle(blanket, 6f, 0.35f, 24f);
                    toast.Show("A cosy blanket. Match night essentials.", 1.7f);
                    return;
                case ToniArmchairItem.Mug:
                    FxWiggle(mug, 6f, 0.3f, 26f);
                    toast.Show("Hot chocolate. Getting cold.", 1.6f);
                    return;
            }
        }

        /// <summary>Un oggetto sulla poltrona di destra: quello di prima torna a posto.</summary>
        void PutOnSeat(HwDragger.Item it)
        {
            if (seatItem != null && seatItem != it) dragger.SendHome(seatItem, 0.3f);
            seatItem = it;
            Vector3 p = rightSeat.position + new Vector3(0f, it == vacuumItem ? 0.05f : 0.2f, 0f);
            p.z = it.T.position.z;
            dragger.SendTo(it, p, 0.2f, 0.1f);
            Sfx.Play(this, "clunk", 0.35f);
        }

        // ---------- Le reazioni di Toni ----------

        /// <summary>L'indizio: un occhio solo si apre e fissa l'oggetto sull'altra poltrona.</summary>
        IEnumerator StareRun(HwDragger.Item it)
        {
            busy = true;
            yield return new WaitForSeconds(0.35f);
            toni.WakeUp(2);
            HalloweenKit.Show(toniLid, true);
            toni.LookTarget = it.T;
            Sfx.Play(this, "tick", 0.4f);
            yield return new WaitForSeconds(0.35f);
            toni.Say(SeatLines[seatLines++ % SeatLines.Length], 2.0f);
            yield return new WaitForSeconds(1.9f);
            toni.LookTarget = null;
            HalloweenKit.Show(toniLid, false);
            toni.Sleep();
            busy = false;
        }

        IEnumerator PushRun()
        {
            busy = true;
            CancelGrip();
            StopFxOn(toni.transform);
            if (logic.Push() == 0) { ToniRestPlace(); busy = false; yield break; }
            Sfx.Play(this, "bonk", 0.7f);
            Haptics.Medium();
            toast.Show(PushLines[pushLines++ % PushLines.Length], 2.2f);
            // piatto come una frittella... e torna giù al suo posto
            Vector3 from = toni.transform.localPosition, fromS = toni.transform.localScale;
            Vector3 home = logic.ToniAt == ToniArmchairSpot.Right ? toniRightPos : toniLeftPos;
            Vector3 flat = new Vector3(toniScale.x * 1.18f, toniScale.y * 0.78f, toniScale.z);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f)
            {
                toni.transform.localScale = Vector3.Lerp(fromS, flat, k);
                yield return null;
            }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                float e = k * k * (3f - 2f * k);
                toni.transform.localPosition = Vector3.Lerp(from, home, e);
                yield return null;
            }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                float b = Mathf.Sin(k * Mathf.PI * 2f) * (1f - k) * 0.06f;
                toni.transform.localScale = Vector3.Lerp(flat, toniScale, k) + new Vector3(-b, b, 0f) * toniScale.y;
                yield return null;
            }
            ToniRestPlace();
            Sfx.Play(this, "snore", 0.5f);
            busy = false;
        }

        void ToniRestPlace()
        {
            StopFxOn(toni.transform);
            toni.transform.localScale = toniScale;
            toni.Face(false);
            toni.transform.localPosition = logic.ToniAt == ToniArmchairSpot.Right ? toniRightPos : toniLeftPos;
            toni.transform.localRotation = Quaternion.identity;
        }

        void TapToni()
        {
            if (logic.TapToni() == 0) return;
            V75AToniSound.Play(this, "grumble", 0.6f);
            FxPop(toni.transform, 1.04f, 0.25f);
            toni.Say(ToniSays[toniSays++ % ToniSays.Length], 1.4f);
        }

        IEnumerator TreatRun()
        {
            busy = true;
            CancelGrip();
            if (logic.GiveTreat() == 0) { dragger.SendHome(treatsItem, 0.3f); busy = false; yield break; }
            ToniRestPlace();
            Vector3 c0 = treats.position + new Vector3(0f, 0.3f, 0f);
            c0.z = cookie.position.z;
            cookie.position = c0;
            cookie.localScale = Vector3.one * 0.6f;
            HalloweenKit.Show(cookie, true);
            Sfx.Play(this, "pop", 0.5f);
            yield return new WaitForSeconds(0.15f);
            // se lo mangia a occhi chiusi (Chomp non apre gli occhi)
            yield return StartCoroutine(toni.Chomp(cookie));
            cookie.localScale = Vector3.one * 0.6f;
            HalloweenKit.Show(cookie, false);
            ToniRestPlace();
            toast.Show(TreatLines[treatLines++ % TreatLines.Length], 2f);
            dragger.SendHome(treatsItem, 0.3f);
            Sfx.Play(this, "snore", 0.5f);
            yield return new WaitForSeconds(0.2f);
            busy = false;
        }

        void Squeak()
        {
            if (logic.Squeak() == 0) return;
            V75AToniSound.Play(this, "squeak", 0.7f);
            FxPop(squeaky, 1.25f, 0.2f);
            if (earCo != null) StopCoroutine(earCo);
            earCo = StartCoroutine(EarTwitch());
            toast.Show(SqueakLines[squeakLines++ % SqueakLines.Length], 1.8f);
        }

        /// <summary>Si muove un orecchio (la sovrapposizione ruota sull'attacco), nient'altro.</summary>
        IEnumerator EarTwitch()
        {
            yield return new WaitForSeconds(0.15f);
            HalloweenKit.Show(toniEar, true);
            float[] angles = { 28f, -8f, 22f, 0f };
            float a0 = 0f;
            foreach (float a in angles)
            {
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.08f)
                {
                    toniEar.localEulerAngles = earRot + new Vector3(0f, 0f, Mathf.Lerp(a0, a, k));
                    yield return null;
                }
                a0 = a;
            }
            toniEar.localEulerAngles = earRot;
            HalloweenKit.Show(toniEar, false);
            earCo = null;
        }

        /// <summary>L'aspirapolvere: rumore, Toni russa più forte e brontola. Al terzo si sveglia furioso (sconfitta).</summary>
        IEnumerator VacuumRun()
        {
            busy = true;
            CancelGrip();
            ToniArmchairVacuum r = logic.Vacuum();
            if (r == ToniArmchairVacuum.None) { dragger.SendHome(vacuumItem, 0.3f); busy = false; yield break; }
            if (r == ToniArmchairVacuum.Furious) { StartFail(); yield break; }
            HalloweenKit.Show(noise, true);
            V75AToniSound.Play(this, "vroom", 0.6f);
            Haptics.Medium();
            FxShake(vacuum, 0.04f, 1.2f);
            FxPop(noise, 1.2f, 1.2f);
            // russa più forte: più Zzz, la pancia si schiaccia
            if (toni.zzz != null) toni.zzz.puffsPerSecond = zzzRate * 4f;
            toni.Say("SNOOOORE!", 1.0f, "snore");
            FxShake(toni.transform, 0.02f, 1.0f);
            yield return new WaitForSeconds(1.2f);
            HalloweenKit.Show(noise, false);
            if (toni.zzz != null) toni.zzz.puffsPerSecond = zzzRate;
            V75AToniSound.Play(this, "growl", r == ToniArmchairVacuum.Grumpy ? 0.8f : 0.55f);
            if (r == ToniArmchairVacuum.Growl)
            {
                toni.Say("Grrr...", 1.4f);
                toast.Show("Toni growls. (Strike 1 of 3)", 2.2f);
            }
            else
            {
                toni.Alert(true);
                toni.Say("GRRRRR!", 1.6f);
                toast.Show("Toni is getting VERY grumpy. (Strike 2 of 3)", 2.6f);
                FxShake(toni.transform, 0.03f, 0.5f);
            }
            dragger.SendHome(vacuumItem, 0.3f);
            yield return new WaitForSeconds(1.0f);
            toni.Alert(false);
            busy = false;
        }

        /// <summary>La coperta addosso a Toni: ancora più comodo. Poi la coperta torna a posto.</summary>
        IEnumerator CoverRun()
        {
            busy = true;
            CancelGrip();
            if (logic.CoverToni() == 0) { dragger.SendHome(blanketItem, 0.3f); busy = false; yield break; }
            Vector3 on = toni.transform.position + new Vector3(0.25f, 0.05f, 0f);
            on.z = blanket.position.z;
            yield return HalloweenKit.Move(blanket, on, 0.2f, 0.15f);
            toni.Wag = 0.6f;
            toni.Say("Mmm... cosy.", 1.3f);
            toast.Show(CoverLines[coverLines++ % CoverLines.Length], 1.9f);
            yield return new WaitForSeconds(1.3f);
            toni.Wag = 0.03f;
            dragger.SendHome(blanketItem, 0.35f);
            yield return new WaitForSeconds(0.35f);
            busy = false;
        }

        // ---------- Sedersi ----------

        void Seat(ToniArmchairSpot seat)
        {
            ToniArmchairSit r = logic.Sit(seat);
            switch (r)
            {
                case ToniArmchairSit.ToniTakesIt: StartCoroutine(SwitchRun()); return;
                case ToniArmchairSit.Won: StartCoroutine(Finale()); return;
                case ToniArmchairSit.Occupied:
                    StartCoroutine(BounceOff(seat));
                    return;
                default:
                    YouRest();
                    dragger.SetBaseScale(youItem, you.localScale);
                    return;
            }
        }

        /// <summary>"YOU" sulla poltrona di Toni: rimbalza via, lui brontola.</summary>
        IEnumerator BounceOff(ToniArmchairSpot seat)
        {
            busy = true;
            CancelGrip();
            Vector3 top = SeatPoint(seat) + new Vector3(0f, 0.35f, 0f);
            yield return HalloweenKit.Move(you, top, 0.15f);
            Sfx.Play(this, "bonk", 0.6f);
            V75AToniSound.Play(this, "grumble", 0.6f);
            toni.Say(ToniSays[toniSays++ % ToniSays.Length], 1.3f);
            FxPop(toni.transform, 1.05f, 0.25f);
            toast.Show(OccupiedLines[occupiedLines++ % OccupiedLines.Length], 1.8f);
            yield return HalloweenKit.Move(you, youHome, 0.35f, 0.6f);
            YouRest();
            busy = false;
        }

        /// <summary>
        /// Ti sei seduto sull'ALTRA poltrona: Toni apre un occhio, si sveglia, salta giù, sale sul tuo nuovo posto e ti
        /// butta sul tappeto, poi si gira due volte e si riaddormenta. La tua poltrona è libera.
        /// </summary>
        IEnumerator SwitchRun()
        {
            busy = true;
            CancelGrip();
            if (seatItem != null) { dragger.SendHome(seatItem, 0.25f); seatItem = null; }
            ToniRestPlace();
            Vector3 seatAt = SeatPoint(ToniArmchairSpot.Right);
            dragger.Stop(youItem);
            yield return HalloweenKit.Move(you, seatAt, 0.2f, 0.15f);
            you.localScale = youScale * SeatedScale;
            dragger.SetBaseScale(youItem, you.localScale);
            Sfx.Play(this, "pop", 0.5f);
            toast.Show("You sit in the OTHER armchair...", 1.6f);
            yield return new WaitForSeconds(0.45f);
            // un occhio solo... poi tutti e due
            toni.WakeUp(2);
            HalloweenKit.Show(toniLid, true);
            toni.LookTarget = you;
            toni.Say("...!", 0.8f);
            yield return new WaitForSeconds(0.7f);
            HalloweenKit.Show(toniLid, false);
            toni.Alert(true);
            toni.Wag = 1f;
            toni.Say("THAT seat looks better!", 1.4f, "woof");
            yield return new WaitForSeconds(1.0f);
            toni.Alert(false);
            // salto sulla poltrona di destra (sopra di te)
            toni.Stand(true);
            toni.Face(true);
            Vector3 lift = toni.transform.localPosition - toniLeftPos;
            Vector3 from = toni.transform.localPosition, to = toniRightPos + lift;
            Sfx.Play(this, "whoosh", 0.5f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                toni.transform.localPosition = Vector3.Lerp(from, to, k) + new Vector3(0f, 0.9f * Mathf.Sin(k * Mathf.PI), 0f);
                toni.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * Mathf.PI) * 10f);
                yield return null;
            }
            toni.transform.localPosition = to;
            toni.transform.localRotation = Quaternion.identity;
            Sfx.Play(this, "bonk", 0.7f);
            Haptics.Medium();
            // ...e ti butta giù, sul tappeto
            V75AToniSound.Play(this, "boing", 0.5f);
            StartCoroutine(YouPushedOff());
            yield return new WaitForSeconds(0.35f);
            // due giri su se stesso, poi giù a cuccia
            for (int i = 0; i < 4; i++)
            {
                toni.Face(i % 2 == 1);
                Sfx.Play(this, "tick", 0.4f);
                yield return new WaitForSeconds(0.16f);
            }
            toni.Stand(false);
            ToniRestPlace();
            logic.ToniArrived();
            toni.LookTarget = null;
            toni.Say("Mine.", 1.1f);
            yield return new WaitForSeconds(0.5f);
            toni.Sleep();
            toast.Show("Toni switched armchairs. Yours is FREE!", 2.4f);
            yield return new WaitForSeconds(0.2f);
            busy = false;
        }

        IEnumerator YouPushedOff()
        {
            Vector3 from = you.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                Vector3 p = Vector3.Lerp(from, youHome, k);
                p.y += 0.7f * Mathf.Sin(k * Mathf.PI);
                you.position = p;
                you.localRotation = Quaternion.Euler(0f, 0f, 360f * k);
                you.localScale = Vector3.Lerp(youScale * SeatedScale, youScale, k);
                yield return null;
            }
            you.position = youHome;
            you.localRotation = Quaternion.identity;
            you.localScale = youScale;
            dragger.SetBaseScale(youItem, youScale);
            Sfx.Play(this, "clunk", 0.4f);
        }

        // ---------- Il finale ----------

        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            busy = true;
            CancelGrip();
            if (seatItem != null) { dragger.SendHome(seatItem, 0.2f); seatItem = null; }
            ToniRest();
            Vector3 seatAt = SeatPoint(ToniArmchairSpot.Left);
            yield return HalloweenKit.Move(you, seatAt, 0.25f, 0.15f);
            you.localScale = youScale * SeatedScale;
            you.localRotation = Quaternion.identity;
            Sfx.Play(this, "pop", 0.6f);
            FxPop(you, 1.1f, 0.25f);
            // il telecomando in mano e la partita comincia
            Vector3 rAt = seatAt + new Vector3(0.38f, -0.22f, 0f);
            rAt.z = remote.position.z;
            yield return HalloweenKit.Move(remote, rAt, 0.3f, 0.3f);
            remote.localRotation = Quaternion.Euler(0f, 0f, -20f);
            Sfx.Play(this, "tick", 0.6f);
            ShowClock();
            FxPop(tvTitle.transform, 1.25f, 0.3f);
            // Toni apre un occhio, ti guarda, è contento lo stesso
            yield return new WaitForSeconds(0.3f);
            toni.WakeUp(2);
            HalloweenKit.Show(toniLid, true);
            toni.LookTarget = you;
            if (toni.hearts != null) { toni.hearts.transform.position = toni.HeadWorld + Vector3.up * 0.4f; toni.hearts.Play(); }
            toni.Wag = 0.8f;
            toast.Show("PUGS ALWAYS WANT YOUR SEAT.", 2f);
            yield return new WaitForSeconds(1.2f);
            HalloweenKit.Show(toniLid, false);
            toni.LookTarget = null;
            toni.Sleep();
            if (toni.hearts != null) toni.hearts.Stop();
            toast.Show("ENJOY IT WHILE IT LASTS.", 2.4f);
            yield return Celebrate(1.2f);
            if (logic.TryComplete() && !IsFinished) Win();
        }

        // ---------- La sconfitta ----------

        void StartFail()
        {
            if (!logic.IsFailed || IsFinished) return;
            StopAllCoroutines();
            StopFx();
            earCo = null;
            CancelGrip();
            busy = false;
            finishing = false;
            StartCoroutine(FailRun());
        }

        /// <summary>
        /// La partita comincia senza di te (Toni si gode la TV), oppure Toni si sveglia furioso per l'aspirapolvere e si
        /// prende anche il telecomando. Poi Fail, una volta sola.
        /// </summary>
        IEnumerator FailRun()
        {
            if (finishing) yield break;
            finishing = true;
            busy = true;
            bool vacuumFail = logic.FailReason == ToniArmchairFail.Vacuum;
            ToniRest();
            YouRest();
            HalloweenKit.Show(noise, vacuumFail);
            if (vacuumFail)
            {
                V75AToniSound.Play(this, "vroom", 0.6f);
                FxShake(vacuum, 0.05f, 0.8f);
                yield return new WaitForSeconds(0.6f);
                HalloweenKit.Show(noise, false);
                // sveglio, furioso
                toni.WakeUp(2);
                toni.Alert(true);
                toni.Stand(true);
                toni.MouthOpen(true);
                V75AToniSound.Play(this, "growl", 0.9f);
                toni.Say("GRRRRRRR!!", 1.6f, "woof");
                Haptics.Medium();
                FxShake(toni.transform, 0.05f, 0.5f);
                toast.Show("Strike 3. Toni is FURIOUS.", 2f);
                yield return new WaitForSeconds(0.7f);
                // ...e si prende il telecomando
                toni.MouthOpen(false);
                Vector3 m = toni.MouthWorld;
                m.z = remote.position.z;
                yield return HalloweenKit.Move(remote, m, 0.35f, 0.4f);
                Sfx.Play(this, "bonk", 0.6f);
                toni.Say("MINE. ALL MINE.", 1.4f, "woof");
                yield return new WaitForSeconds(1.0f);
                if (logic.TryReportFail() && !IsFinished) Fail(ToniArmchairRules.VacuumTaunt);
            }
            else
            {
                ShowClock();
                FxPop(tvTitle.transform, 1.3f, 0.3f);
                Sfx.Play(this, "ding", 0.6f);
                toast.Show("The match started. You're standing.", 1.8f);
                // Toni apre un occhio sulla TV, scodinzola: lui la partita non se la perde
                toni.WakeUp(2);
                HalloweenKit.Show(toniLid, true);
                toni.LookTarget = tvTitle.transform;
                toni.Wag = 1f;
                yield return new WaitForSeconds(1.2f);
                if (logic.TryReportFail() && !IsFinished) Fail(ToniArmchairRules.MatchTaunt);
            }
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked) return;
            if (Input.GetKeyDown(KeyCode.R)) { CancelGrip(); Seat(ToniArmchairSpot.Right); return; }
            if (Input.GetKeyDown(KeyCode.L)) { CancelGrip(); Seat(ToniArmchairSpot.Left); return; }
            if (Input.GetKeyDown(KeyCode.P)) { CancelGrip(); StartCoroutine(PushRun()); return; }
            if (Input.GetKeyDown(KeyCode.T)) { CancelGrip(); StartCoroutine(TreatRun()); return; }
            if (Input.GetKeyDown(KeyCode.V)) { CancelGrip(); StartCoroutine(VacuumRun()); return; }
            if (Input.GetKeyDown(KeyCode.C)) { CancelGrip(); StartCoroutine(CoverRun()); return; }
            if (Input.GetKeyDown(KeyCode.B) && logic.PlaceOnRight(ToniArmchairItem.Blanket) == ToniArmchairPlace.Suspicious)
            {
                CancelGrip();
                PutOnSeat(blanketItem);
                StartCoroutine(StareRun(blanketItem));
                return;
            }
            if (Input.GetKeyDown(KeyCode.Q)) Squeak();
            if (Input.GetKeyDown(KeyCode.G)) TapToni();
            if (Input.GetKeyDown(KeyCode.K) && logic.TimeLeft > 5f && logic.Tick(logic.TimeLeft - 5f)) StartFail();
        }
#endif
    }

    /// <summary>Suoni sintetizzati del livello 275: paperella, aspirapolvere, brontolio, ringhio, cuscino che rimbalza.</summary>
    public static class V75AToniSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(275);

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
                case "squeak":   // la paperella: due fischi che salgono
                    c = Make(kind, 0.32f, t =>
                    {
                        float u = t < 0.14f ? t : t - 0.16f;
                        if (t >= 0.14f && t < 0.16f) return 0f;
                        float f = Mathf.Lerp(1400f, 2300f, u / 0.14f);
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.4f * Mathf.Sin(Mathf.Clamp01(u / 0.14f) * Mathf.PI);
                    });
                    break;
                case "vroom":    // l'aspirapolvere: rombo e soffio
                    {
                        float lp = 0f;
                        c = Make(kind, 1.3f, t =>
                        {
                            lp += (N() - lp) * 0.3f;
                            float env = Mathf.Min(1f, t * 8f) * Mathf.Min(1f, (1.3f - t) * 6f);
                            return (lp * 0.5f + Mathf.Sin(t * 2f * Mathf.PI * 118f) * 0.25f + Mathf.Sin(t * 2f * Mathf.PI * 236f) * 0.1f) * env;
                        });
                    }
                    break;
                case "grumble":  // brontolio nel sonno
                    c = Make(kind, 0.45f, t => (Mathf.Sin(t * 2f * Mathf.PI * (95f + 15f * Mathf.Sin(t * 30f))) * 0.5f + N() * 0.12f) * Mathf.Sin(t / 0.45f * Mathf.PI));
                    break;
                case "growl":    // ringhio
                    c = Make(kind, 0.7f, t => (Mathf.Sin(t * 2f * Mathf.PI * 80f) * 0.45f + N() * 0.25f) * (0.6f + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * 28f)) * Mathf.Sin(t / 0.7f * Mathf.PI));
                    break;
                case "boing":    // il cuscino che vola giù
                    c = Make(kind, 0.45f, t => Mathf.Sin(t * 2f * Mathf.PI * (260f + 110f * Mathf.Sin(t * 38f))) * 0.4f * Mathf.Exp(-t * 6f));
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
            AudioClip clip = AudioClip.Create("v75a_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
