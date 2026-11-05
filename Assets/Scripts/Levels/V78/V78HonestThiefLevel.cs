using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 15 della roadmap "The Honest Thief" (id honest_thief) — "STEAL THE DIAMOND." Nella sala di un museo un
    /// diamante sta su un cuscino dentro una campana di vetro con la sirena in cima; il piedistallo ha la targhetta
    /// "PRICELESS". A destra un ladro da fumetto (maglia a righe, mascherina, coppola) con la mano tesa; a sinistra un
    /// tavolino col cestino dei volantini gratis e un cartellino appeso "FREE" (sobrio, stesso inchiostro del resto).
    /// Inganno: toccare diamante o teca fa suonare l'allarme (sirena rossa che gira, lampo, suono; si spegne da solo dopo
    /// 1,5 s); trascinare il diamante fa entrare la guardia da sinistra ("Hey! Hands off!"), il diamante resta al suo
    /// posto e la guardia se ne va dopo un attimo; il ladro toccato: "Psst. Any ideas?". Tutto reversibile, mai sconfitta.
    /// Soluzione: trascinare il cartellino FREE sulla teca (zona dai limiti della teca + margine generoso): si aggancia
    /// sopra la targhetta, la teca ora dice FREE, la sirena diventa verde e la campana si alza. Scelta documentata: BLOCCO
    /// AL POSTO (il cartellino agganciato non si stacca più; non crea vicoli ciechi). Solo allora il diamante si stacca:
    /// portato sul ladro -> lui lo prende, saluta togliendosi il cappello, la guardia arriva e lo saluta con la mano, il
    /// ladro se ne va; LevelReward "MASTER CRIMINAL. ZERO CRIMES!" -> Win una volta sola. Diamante lasciato prima del
    /// ladro: torna sul cuscino. Cartellino lasciato altrove: torna appeso al cestino.
    /// Un dito alla volta (HalloweenKit.ReadPointer); due dita, pausa, app in background o livello spento annullano la
    /// presa (quello che si teneva torna a casa). La logica è in Gotcha.Levels.V64.HonestThiefLogic.
    /// Tasti di prova nell'Editor: T tocco sul diamante, D tentativo di trascinare il diamante (prima del cartellino:
    /// guardia; dopo: lasciato corto), S cartellino lasciato altrove, F cartellino sulla teca, W diamante al ladro
    /// (soluzione dopo F), H tocco sul ladro, B tocco sul cestino, P pausa simulata (presa annullata), R ricomincia.
    /// </summary>
    public class V78HonestThiefLevel : LevelController
    {
        [Header("Cartellino FREE (genitore col collider, figli: disegno e TextMesh)")]
        [SerializeField] Transform card;
        [SerializeField] Collider2D cardArea;
        [SerializeField] TextMesh cardText;
        [Header("Teca")]
        [SerializeField] Transform diamond;
        [SerializeField] Collider2D diamondArea;
        [SerializeField] Transform[] diamondSparkles;
        [SerializeField] Transform caseCover;           // campana di vetro + sirena (si alza quando la teca è FREE)
        [SerializeField] BoxCollider2D caseArea;        // vetro + piedistallo: tocchi e zona di rilascio del cartellino
        [SerializeField] SpriteRenderer sirenDome;
        [SerializeField] SpriteRenderer sirenBeams;
        [SerializeField] Transform plaque;              // dove si aggancia il cartellino
        [SerializeField] TextMesh plaqueText;           // "PRICELESS"
        [SerializeField] SpriteRenderer flash;          // lampo rosso dell'allarme (tutto lo schermo)
        [Header("Ladro")]
        [SerializeField] Transform thief;
        [SerializeField] Transform thiefHat;
        [SerializeField] Transform thiefHand;           // punto della mano tesa (figlio del ladro)
        [SerializeField] BoxCollider2D thiefArea;       // tocchi e zona di consegna del diamante
        [Header("Guardia")]
        [SerializeField] Transform guard;               // all'inizio fuori scena a sinistra
        [SerializeField] Transform guardArm;            // perno sulla spalla (0° = giù)
        [SerializeField] float guardStandX = -1.1f;
        [Header("Cestino dei volantini")]
        [SerializeField] Collider2D standArea;
        [Header("Fumetti, scritte e festa")]
        [SerializeField] SpeechBubble guardBubble;
        [SerializeField] SpeechBubble thiefBubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "MASTER CRIMINAL. ZERO CRIMES!";
        const float TapSlop = 0.15f, Follow = 14f, Lift = 1.12f;
        const int HeldOrderBoost = 40;
        const float CoverLift = 0.9f, CardOnPlaqueScale = 1.12f, GuardSpeed = 9f, ExitX = 3.9f;
        static readonly Color SirenIdle = new Color(0.78f, 0.36f, 0.36f), SirenAlarm = new Color(1f, 0.15f, 0.12f), SirenFree = new Color(0.4f, 0.9f, 0.45f);
        static readonly Color BeamAlarm = new Color(1f, 0.25f, 0.2f, 0.9f), FlashColor = new Color(1f, 0.1f, 0.1f, 1f);

        static readonly string[] AlarmLines = { "WEE-OO! WEE-OO!", "The alarm hates fingers.", "Shh! Everyone heard that." };
        static readonly string[] GuardLines = { "Hey! Hands off!", "Not so fast!", "I'm watching you.", "That's not yours!" };
        static readonly string[] ThiefLines = { "Psst. Any ideas?", "I'm honest. I only take free stuff.", "Psst. Anything free around here?" };
        static readonly string[] ThiefFreeLines = { "FREE? Then it's not stealing!", "Pass it over. Legally." };
        static readonly string[] SignTapLines = { "Just a little FREE card.", "A card that says FREE.", "FREE... but free what?" };
        static readonly string[] SignBackLines = { "Back on its hook.", "Not there. It hangs back up.", "The card swings back." };
        static readonly string[] BasketLines = { "Free flyers. Nobody takes them.", "Free pens. How exciting.", "Help yourself. It's FREE." };
        static readonly string[] FreeTapLines = { "No alarm. It's FREE now!", "Free diamond. Who wants it?" };
        static readonly string[] DiamondBackLines = { "Back on the cushion.", "So close! Someone wants it.", "It slid back home." };
        const string PlacedLine = "PRICELESS? No. FREE!";
        const string ThiefThanks = "Much obliged!", GuardBye = "Come again!";

        enum Kind { None, Card, Diamond, Thief, Case, Stand }

        sealed class Grip
        {
            public Kind kind;
            public Vector2 start;
            public Vector3 offset;
            public bool moved, held;
        }

        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        HonestThiefLogic logic;
        Camera cam;
        HwToast toast;
        HwSay guardSay, thiefSay;
        Grip grip;
        Renderer[] cardRenderers, diamondRenderers;
        int[] cardOrders, diamondOrders;
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        Vector3[] sparkleScales;
        Vector3 cardHome, cardScale, diamondHome, diamondScale, coverHome, thiefHome, thiefScale, hatHome, guardHome;
        Quaternion hatRot;
        float flashA, lastSiren = -10f;
        int alarmLines, guardLines, thiefLines, thiefFreeLines, signTapLines, signBackLines, basketLines, freeTapLines, diamondBackLines;
        bool captured, finishing;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            if (!captured)
            {
                captured = true;
                logic = new HonestThiefLogic();
                cardHome = card.position;
                cardScale = card.localScale;
                diamondHome = diamond.position;
                diamondScale = diamond.localScale;
                coverHome = caseCover.position;
                thiefHome = thief.position;
                thiefScale = thief.localScale;
                hatHome = thiefHat.localPosition;
                hatRot = thiefHat.localRotation;
                guardHome = guard.position;
                cardRenderers = card.GetComponentsInChildren<Renderer>(true);
                diamondRenderers = diamond.GetComponentsInChildren<Renderer>(true);
                cardOrders = Orders(cardRenderers);
                sparkleScales = new Vector3[diamondSparkles != null ? diamondSparkles.Length : 0];
                for (int i = 0; i < sparkleScales.Length; i++) sparkleScales[i] = diamondSparkles[i] != null ? diamondSparkles[i].localScale : Vector3.one;
                diamondOrders = Orders(diamondRenderers);
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            grip = null;
            finishing = false;
            alarmLines = guardLines = thiefLines = thiefFreeLines = signTapLines = signBackLines = basketLines = freeTapLines = diamondBackLines = 0;
            toast = new HwToast(infoText, infoHolder);
            guardSay = new HwSay(guardBubble);
            thiefSay = new HwSay(thiefBubble);
            RestPose();
        }

        static int[] Orders(Renderer[] rs)
        {
            var o = new int[rs.Length];
            for (int i = 0; i < rs.Length; i++) o[i] = rs[i] != null ? rs[i].sortingOrder : 0;
            return o;
        }

        static void Raise(Renderer[] rs, int[] baseOrders, int boost)
        {
            if (rs == null) return;
            for (int i = 0; i < rs.Length; i++) if (rs[i] != null) rs[i].sortingOrder = baseOrders[i] + boost;
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            guardSay?.Hide();
            thiefSay?.Hide();
            Raise(cardRenderers, cardOrders, 0);
            Raise(diamondRenderers, diamondOrders, 0);
            card.position = CardTarget();
            card.localScale = CardScaleTarget();
            card.localRotation = Quaternion.identity;
            if (cardText != null) cardText.gameObject.SetActive(true);   // la scritta FREE resta sempre la stessa (anche sulla teca)
            diamond.position = diamondHome;
            diamond.localScale = diamondScale;
            diamond.localRotation = Quaternion.identity;
            caseCover.position = CoverTarget();
            thief.position = thiefHome;
            thief.localScale = thiefScale;
            thief.localRotation = Quaternion.identity;
            thiefHat.localPosition = hatHome;
            thiefHat.localRotation = hatRot;
            guard.position = new Vector3(logic.GuardOn ? guardStandX : guardHome.x, guardHome.y, guardHome.z);
            guard.localRotation = Quaternion.identity;
            if (guardArm != null) guardArm.localRotation = Quaternion.Euler(0f, 0f, logic.GuardOn ? 90f : 0f);
            for (int i = 0; i < sparkleScales.Length; i++) if (diamondSparkles[i] != null) diamondSparkles[i].localScale = sparkleScales[i];
            flashA = 0f;
            Siren(0f);
            Labels();
        }

        Vector3 CardTarget()
        {
            if (logic.SignAt == HonestThiefSignAt.Case && plaque != null)
                return new Vector3(plaque.position.x, plaque.position.y, cardHome.z);
            return cardHome;
        }

        Vector3 CardScaleTarget() => logic.SignAt == HonestThiefSignAt.Case ? cardScale * CardOnPlaqueScale : cardScale;

        Vector3 CoverTarget() => coverHome + (logic.CaseIsFree ? new Vector3(0f, CoverLift, 0f) : Vector3.zero);

        /// <summary>Targhetta e stelline: "PRICELESS" finché la teca non è FREE (poi il cartellino la copre).</summary>
        void Labels()
        {
            bool free = logic.CaseIsFree;
            if (plaqueText != null) plaqueText.gameObject.SetActive(!free);
            if (diamondSparkles != null) foreach (Transform s in diamondSparkles) HalloweenKit.Show(s, free && !finishing);
        }

        /// <summary>Sirena: rosso spento, rosso che lampeggia coi fasci che girano (allarme), verde (teca FREE).</summary>
        void Siren(float dt)
        {
            bool alarm = logic.AlarmOn && !finishing;
            if (sirenDome != null)
            {
                Color c = logic.CaseIsFree ? SirenFree : alarm ? Color.Lerp(SirenIdle, SirenAlarm, 0.5f + 0.5f * Mathf.Sin(Time.time * 30f)) : SirenIdle;
                sirenDome.color = c;
            }
            if (sirenBeams != null)
            {
                sirenBeams.gameObject.SetActive(alarm);
                if (alarm)
                {
                    sirenBeams.color = BeamAlarm;
                    sirenBeams.transform.localRotation = Quaternion.Euler(0f, 0f, Time.time * 540f);
                }
            }
            flashA = Mathf.Max(0f, flashA - dt * 2.2f);
            if (flash != null)
            {
                flash.gameObject.SetActive(flashA > 0.001f);
                flash.color = new Color(FlashColor.r, FlashColor.g, FlashColor.b, flashA);
            }
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: si molla tutto, si torna a uno stato valido
            CancelGrip();
            StopFx();
            if (captured && !finishing && !IsFinished && logic != null) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == HonestThiefStage.Celebrating) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrip(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrip(); }

        /// <summary>Il dito sparisce: quello che si teneva torna a casa (Pose lo fa scivolare), niente battute.</summary>
        void CancelGrip()
        {
            grip = null;
            if (logic != null && !logic.InputLocked) logic.Cancel();
            if (cardRenderers != null && !finishing)
            {
                Raise(cardRenderers, cardOrders, 0);
                Raise(diamondRenderers, diamondOrders, 0);
                card.localScale = CardScaleTarget();
                diamond.localScale = diamondScale;
            }
        }

        void Update()
        {
            toast?.Tick();
            guardSay?.Tick();
            thiefSay?.Tick();
            if (IsFinished || toast == null || logic == null) return;
            if (!finishing)
            {
                if (Time.timeScale <= 0f) { CancelGrip(); return; }   // pausa o pubblicità: niente input
                HonestThiefTick e = logic.Tick(Time.deltaTime);
                if ((e & HonestThiefTick.GuardLeft) != 0) guardSay.Hide();
#if UNITY_EDITOR
                EditorKeys();
#endif
                if (!finishing) ReadPointer();
            }
            if (!finishing) Pose();
            Siren(Time.deltaTime);
        }

        /// <summary>Cartellino, diamante, campana e guardia scivolano verso la posa voluta dalla logica.</summary>
        void Pose()
        {
            float f = 1f - Mathf.Exp(-Time.deltaTime * Follow);
            if (logic.SignAt != HonestThiefSignAt.Held)
            {
                card.position = Vector3.Lerp(card.position, CardTarget(), f);
                card.localScale = Vector3.Lerp(card.localScale, CardScaleTarget(), f);
                // appeso al gancio: dondola un pochino
                float sway = logic.SignAt == HonestThiefSignAt.Home ? 3f * Mathf.Sin(Time.time * 1.7f) : 0f;
                card.localRotation = Quaternion.Slerp(card.localRotation, Quaternion.Euler(0f, 0f, sway), f);
            }
            if (logic.DiamondAt == HonestThiefDiamondAt.Case && !FxBusy(diamond))
                diamond.position = Vector3.Lerp(diamond.position, diamondHome, f);
            caseCover.position = Vector3.Lerp(caseCover.position, CoverTarget(), f);

            // la guardia entra e resta finché la logica la tiene in scena, poi esce a sinistra
            float gx = logic.GuardOn ? guardStandX : guardHome.x;
            Vector3 gp = guard.position;
            float nx = Mathf.MoveTowards(gp.x, gx, GuardSpeed * Time.deltaTime);
            bool walking = Mathf.Abs(nx - gx) > 0.01f;
            guard.position = new Vector3(nx, guardHome.y + (walking ? Mathf.Abs(Mathf.Sin(Time.time * 22f)) * 0.06f : 0f), gp.z);
            if (guardArm != null)
            {
                float target = logic.GuardOn ? 90f + 8f * Mathf.Sin(Time.time * 9f) : 0f;   // mano alzata: "alt!"
                guardArm.localRotation = Quaternion.Slerp(guardArm.localRotation, Quaternion.Euler(0f, 0f, target), f);
            }
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73-0.77) ----------

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

        bool FxBusy(Transform t) => fxSlots[2].TryGetValue(t, out FxSlot s) && s.co != null;

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
            if (Input.touchCount >= 2) { CancelGrip(); return; }   // due dita: il gesto si annulla (si torna a casa)
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

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, kind = Kind.None };
            if (finishing || logic.InputLocked) return g;
            // prima il cartellino (bersaglio generoso), poi il diamante, il ladro, la teca e il cestino
            if (logic.SignAt == HonestThiefSignAt.Home && Over(cardArea, w)) g.kind = Kind.Card;
            else if (logic.DiamondAt == HonestThiefDiamondAt.Case && Over(diamondArea, w)) g.kind = Kind.Diamond;
            else if (Over(thiefArea, w)) g.kind = Kind.Thief;
            else if (Over(caseArea, w)) g.kind = Kind.Case;
            else if (Over(standArea, w)) g.kind = Kind.Stand;
            Transform t = g.kind == Kind.Card ? card : g.kind == Kind.Diamond ? diamond : null;
            if (t != null) g.offset = t.position - (Vector3)w;
            return g;
        }

        /// <summary>Il dito ha superato la soglia del tocco: si prova a staccare l'oggetto.</summary>
        void StartDrag(Grip g)
        {
            switch (g.kind)
            {
                case Kind.Card:
                    if (!logic.GrabSign()) { g.kind = Kind.None; return; }
                    Hold(g, card, cardRenderers, cardOrders);
                    break;
                case Kind.Diamond:
                    switch (logic.GrabDiamond())
                    {
                        case HonestThiefGrab.Guard:
                            g.kind = Kind.None;   // gesto consumato: il diamante non si muove
                            GuardCame();
                            break;
                        case HonestThiefGrab.Grabbed:
                            Hold(g, diamond, diamondRenderers, diamondOrders);
                            break;
                        default:
                            g.kind = Kind.None;
                            break;
                    }
                    break;
            }
        }

        void Hold(Grip g, Transform t, Renderer[] rs, int[] orders)
        {
            g.held = true;
            StopFxOn(t);
            t.localScale = (t == card ? cardScale : diamondScale) * Lift;
            t.localRotation = Quaternion.identity;
            Raise(rs, orders, HeldOrderBoost);
            Haptics.Tap();
            V78HonestThiefSound.Play(this, "pick", 0.4f);
        }

        void Drag(Grip g, Vector2 w)
        {
            Transform t = g.kind == Kind.Card ? card : g.kind == Kind.Diamond ? diamond : null;
            if (t == null) return;
            Vector2 p = HalloweenKit.ClampSafe(w + (Vector2)g.offset);
            t.position = new Vector3(p.x, p.y, t.position.z);
        }

        void End(Grip g)
        {
            grip = null;
            switch (g.kind)
            {
                case Kind.Card:
                    if (g.held) DropCard();
                    else TapSign();
                    break;
                case Kind.Diamond:
                    if (g.held) DropDiamond();
                    else TapCase();
                    break;
                case Kind.Case: TapCase(); break;
                case Kind.Thief: TapThief(); break;
                case Kind.Stand: TapBasket(); break;
            }
        }

        // ---------- Zone di rilascio (dai limiti dei collider, nel mondo) ----------

        static bool InZone(Vector3 p, BoxCollider2D area, float margin)
        {
            if (area == null) return false;
            Vector2 c = area.transform.TransformPoint(area.offset);
            Vector3 s = area.transform.lossyScale;
            return HonestThiefLogic.InZone(p.x, p.y, c.x, c.y, area.size.x * 0.5f * Mathf.Abs(s.x), area.size.y * 0.5f * Mathf.Abs(s.y), margin);
        }

        // ---------- Il cartellino ----------

        void DropCard()
        {
            Raise(cardRenderers, cardOrders, 0);
            bool onCase = InZone(card.position, caseArea, HonestThiefLogic.CaseMargin);
            switch (logic.ReleaseSign(onCase))
            {
                case HonestThiefSignDrop.Returned:
                    card.localScale = cardScale;
                    toast.Show(SignBackLines[signBackLines++ % SignBackLines.Length], 1.6f);
                    V78HonestThiefSound.Play(this, "swish", 0.35f);
                    break;
                case HonestThiefSignDrop.Placed:
                    SignPlaced();
                    break;
            }
        }

        /// <summary>Il cartellino si aggancia sulla targhetta: la teca è FREE, la sirena diventa verde, la campana si alza.</summary>
        void SignPlaced()
        {
            card.position = CardTarget();
            card.localScale = CardScaleTarget();
            card.localRotation = Quaternion.identity;
            Labels();
            FxPop(card, 1.18f, 0.25f);
            V78HonestThiefSound.Play(this, "click", 0.55f);
            V78HonestThiefSound.Play(this, "ding", 0.45f);
            Haptics.Light();
            flashA = 0f;
            toast.Show(PlacedLine, 2.0f);
            guardSay.Hide();
            thiefSay.Show(ThiefFreeLines[0], 2.0f);
            thiefFreeLines = 1;
            if (diamondSparkles != null) foreach (Transform s in diamondSparkles) if (s != null) StartCoroutine(Twinkle(s));
        }

        IEnumerator Twinkle(Transform s)
        {
            Vector3 baseScale = s.localScale;
            float phase = s.localPosition.x * 7f;
            while (s != null && s.gameObject.activeInHierarchy)
            {
                float k = 0.6f + 0.4f * Mathf.Abs(Mathf.Sin(Time.time * 3f + phase));
                s.localScale = baseScale * k;
                s.localRotation = Quaternion.Euler(0f, 0f, Time.time * 90f + phase * 10f);
                yield return null;
            }
            if (s != null) s.localScale = baseScale;
        }

        void TapSign()
        {
            if (!logic.TapSign()) return;
            FxWiggle(card, 10f, 0.45f, 26f);
            V78HonestThiefSound.Play(this, "pick", 0.25f);
            toast.Show(SignTapLines[signTapLines++ % SignTapLines.Length], 1.6f);
        }

        void TapBasket()
        {
            if (!logic.TapBasket()) return;
            toast.Show(BasketLines[basketLines++ % BasketLines.Length], 1.7f);
            V78HonestThiefSound.Play(this, "swish", 0.3f);
        }

        // ---------- Diamante, allarme e guardia ----------

        void TapCase()
        {
            switch (logic.TapCase())
            {
                case HonestThiefTap.Alarm:
                    flashA = 0.45f;
                    if (Time.time - lastSiren > 0.4f) { lastSiren = Time.time; V78HonestThiefSound.Play(this, "siren", 0.55f); }
                    Haptics.Medium();
                    FxShake(diamond, 0.04f, 0.3f);
                    toast.Show(AlarmLines[alarmLines++ % AlarmLines.Length], 1.6f);
                    break;
                case HonestThiefTap.AlarmAgain:
                    flashA = Mathf.Max(flashA, 0.25f);
                    if (Time.time - lastSiren > 0.4f) { lastSiren = Time.time; V78HonestThiefSound.Play(this, "siren", 0.45f); }
                    Haptics.Tap();
                    break;
                case HonestThiefTap.Free:
                    FxPop(diamond, 1.15f, 0.25f);
                    V78HonestThiefSound.Play(this, "ding", 0.3f);
                    toast.Show(FreeTapLines[freeTapLines++ % FreeTapLines.Length], 1.6f);
                    break;
            }
        }

        /// <summary>Prima del cartellino: la guardia entra da sinistra con la mano alzata, il diamante trema e resta lì.</summary>
        void GuardCame()
        {
            V78HonestThiefSound.Play(this, "whistle", 0.5f);
            Haptics.Light();
            FxShake(diamond, 0.05f, 0.3f);
            thiefSay.Hide();
            guardSay.Show(GuardLines[guardLines++ % GuardLines.Length], HonestThiefLogic.GuardSeconds);
        }

        void TapThief()
        {
            if (!logic.TapThief()) return;
            FxPop(thief, 1.05f, 0.25f);
            guardSay.Hide();
            string line = logic.CaseIsFree ? ThiefFreeLines[thiefFreeLines++ % ThiefFreeLines.Length] : ThiefLines[thiefLines++ % ThiefLines.Length];
            thiefSay.Show(line, 2.2f);
        }

        void DropDiamond()
        {
            Raise(diamondRenderers, diamondOrders, 0);
            bool onThief = InZone(diamond.position, thiefArea, HonestThiefLogic.ThiefMargin);
            switch (logic.ReleaseDiamond(onThief))
            {
                case HonestThiefDiamondDrop.Returned:
                    diamond.localScale = diamondScale;
                    toast.Show(DiamondBackLines[diamondBackLines++ % DiamondBackLines.Length], 1.6f);
                    V78HonestThiefSound.Play(this, "swish", 0.35f);
                    break;
                case HonestThiefDiamondDrop.Taken:
                    StartCoroutine(Finale());
                    break;
            }
        }

        // ---------- La soluzione ----------

        Vector3 HandWorld()
        {
            if (thiefHand != null) return thiefHand.position;
            return thief.position + new Vector3(-0.62f * Mathf.Sign(thief.localScale.x), 1.22f, 0f);
        }

        /// <summary>
        /// Il ladro prende il diamante, si toglie il cappello ("Much obliged!"), la guardia entra e lo saluta con la mano
        /// ("Come again!"), lui se ne va a destra col diamante. Poi la festa comune e Win, una volta sola. Rifatta da capo se
        /// il livello si spegne a metà.
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            grip = null;
            StopFx();
            HalloweenKit.Show(infoHolder, false);
            guardSay.Hide();
            thiefSay.Hide();
            flashA = 0f;
            Labels();
            Raise(diamondRenderers, diamondOrders, 0);
            Raise(cardRenderers, cardOrders, 0);
            card.position = CardTarget();
            card.localScale = CardScaleTarget();
            card.localRotation = Quaternion.identity;
            caseCover.position = CoverTarget();
            thief.position = thiefHome;
            thief.localScale = thiefScale;
            thiefHat.localPosition = hatHome;
            thiefHat.localRotation = hatRot;
            diamond.localScale = diamondScale;
            diamond.localRotation = Quaternion.identity;
            // il diamante finisce nel palmo (sopra il ladro)
            Raise(diamondRenderers, diamondOrders, 10);
            Vector3 from = diamond.position, hand = HandWorld() + new Vector3(0f, 0.22f, 0f);
            hand.z = from.z;
            for (float t = 0f; t < 0.2f; t += Time.deltaTime)
            {
                diamond.position = Vector3.Lerp(from, hand, t / 0.2f);
                yield return null;
            }
            diamond.position = hand;
            V78HonestThiefSound.Play(this, "click", 0.5f);
            Haptics.Light();

            // si toglie il cappello: "Much obliged!"
            thiefSay.Show(ThiefThanks, 1.2f);
            Vector3 hatUp = hatHome + new Vector3(-0.08f, 0.22f, 0f);
            Quaternion hatTilt = hatRot * Quaternion.Euler(0f, 0f, 28f);
            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                float k = Mathf.Sin(Mathf.Clamp01(t / 0.5f) * Mathf.PI);
                thiefHat.localPosition = Vector3.Lerp(hatHome, hatUp, k);
                thiefHat.localRotation = Quaternion.Slerp(hatRot, hatTilt, k);
                yield return null;
            }
            thiefHat.localPosition = hatHome;
            thiefHat.localRotation = hatRot;

            // la guardia arriva e saluta; il ladro se ne va a destra col diamante
            thiefSay.Hide();
            guardSay.Show(GuardBye, 1.4f);
            V78HonestThiefSound.Play(this, "ding", 0.5f);
            thief.localScale = new Vector3(-Mathf.Abs(thiefScale.x), thiefScale.y, thiefScale.z);   // si gira verso l'uscita
            Vector3 thiefFrom = thief.position;
            Vector3 handOffset = new Vector3(0f, 0.22f, 0f);   // il diamante passa nella mano girata verso l'uscita
            for (float t = 0f; t < 0.8f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.8f);
                // la guardia entra in fretta e agita la mano alzata
                Vector3 gp = guard.position;
                guard.position = new Vector3(Mathf.MoveTowards(gp.x, guardStandX, GuardSpeed * Time.deltaTime), guardHome.y, gp.z);
                if (guardArm != null) guardArm.localRotation = Quaternion.Euler(0f, 0f, 165f + 20f * Mathf.Sin(t * 16f));
                float bob = Mathf.Abs(Mathf.Sin(t * 18f)) * 0.08f;
                thief.position = new Vector3(Mathf.Lerp(thiefFrom.x, ExitX, k * k), thiefFrom.y + bob, thiefFrom.z);
                diamond.position = HandWorld() + handOffset;
                yield return null;
            }
            thief.position = new Vector3(ExitX, thiefFrom.y, thiefFrom.z);
            diamond.position = HandWorld() + handOffset;
            if (guardArm != null) guardArm.localRotation = Quaternion.Euler(0f, 0f, 165f);

            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            if (logic.TryComplete()) Win(Compliment);
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked || grip != null || logic.Holding) return;
            if (Input.GetKeyDown(KeyCode.T)) TapCase();
            if (Input.GetKeyDown(KeyCode.H)) TapThief();
            if (Input.GetKeyDown(KeyCode.B)) TapBasket();
            if (Input.GetKeyDown(KeyCode.D))
            {
                var g = new Grip { kind = Kind.Diamond, offset = Vector3.zero };
                StartDrag(g);
                if (g.held)
                {
                    diamond.position = diamondHome + new Vector3(0.8f, 0.4f, 0f);   // lasciato corto
                    DropDiamond();
                }
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                var g = new Grip { kind = Kind.Diamond, offset = Vector3.zero };
                StartDrag(g);
                if (g.held)
                {
                    Vector3 c = thiefArea.bounds.center;
                    diamond.position = new Vector3(c.x, c.y, diamond.position.z);
                    DropDiamond();
                }
            }
            if (Input.GetKeyDown(KeyCode.S) && logic.SignAt == HonestThiefSignAt.Home)
            {
                var g = new Grip { kind = Kind.Card };
                StartDrag(g);
                if (g.held) { card.position = new Vector3(0f, 2.0f, card.position.z); DropCard(); }
            }
            if (Input.GetKeyDown(KeyCode.F) && logic.SignAt == HonestThiefSignAt.Home)
            {
                var g = new Grip { kind = Kind.Card };
                StartDrag(g);
                if (g.held) { Vector3 c = caseArea.bounds.center; card.position = new Vector3(c.x, c.y, card.position.z); DropCard(); }
            }
            if (Input.GetKeyDown(KeyCode.P)) CancelGrip();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }
#endif
    }

    /// <summary>Suoni sintetizzati di The Honest Thief: sirena "wee-oo", fischietto della guardia, presa, clic, fruscio, "ding".</summary>
    public static class V78HonestThiefSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(285);

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
                case "siren":    // "wee-oo wee-oo": due note che si alternano, onda quasi quadra
                    {
                        float ph = 0f;
                        c = Make(kind, 0.9f, t =>
                        {
                            float f = (int)(t / 0.225f) % 2 == 0 ? 960f : 720f;
                            ph += f / Rate;
                            float s = Mathf.Sin(ph * 2f * Mathf.PI);
                            return Mathf.Clamp(s * 2.2f, -1f, 1f) * 0.32f * Mathf.Min(1f, t * 40f) * Mathf.Clamp01((0.9f - t) * 10f);
                        });
                        break;
                    }
                case "whistle":  // fischietto: trillo acuto
                    {
                        float ph = 0f;
                        c = Make(kind, 0.4f, t =>
                        {
                            float f = 2300f + 140f * Mathf.Sin(t * 2f * Mathf.PI * 32f);
                            ph += f / Rate;
                            return (Mathf.Sin(ph * 2f * Mathf.PI) * 0.35f + N() * 0.05f) * Mathf.Min(1f, t * 60f) * Mathf.Clamp01((0.4f - t) * 12f);
                        });
                        break;
                    }
                case "pick":     // oggetto che si stacca: "plip"
                    c = Make(kind, 0.08f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(600f, 1100f, t / 0.08f)) * 0.4f * Mathf.Exp(-t * 30f));
                    break;
                case "click":    // cartellino agganciato / diamante nel palmo
                    c = Make(kind, 0.06f, t => N() * 0.5f * Mathf.Exp(-t * 110f) + Mathf.Sin(t * 2f * Mathf.PI * 1200f) * 0.25f * Mathf.Exp(-t * 70f));
                    break;
                case "swish":    // torna a casa scivolando
                    {
                        float lp = 0f;
                        c = Make(kind, 0.22f, t =>
                        {
                            lp += (N() - lp) * 0.3f;
                            return lp * 0.5f * Mathf.Sin(t / 0.22f * Mathf.PI);
                        });
                        break;
                    }
                case "ding":     // due note in salita
                    c = Make(kind, 0.6f, t =>
                    {
                        float f = t < 0.12f ? 880f : 1318.5f;
                        float u = t < 0.12f ? t : t - 0.12f;
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.35f * Mathf.Exp(-u * 6f) * Mathf.Min(1f, u * 300f);
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
            AudioClip clip = AudioClip.Create("v78b_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
