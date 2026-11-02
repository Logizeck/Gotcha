using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 10 della roadmap "Half Full" — "MAKE THE PESSIMIST HAPPY." Un omino triste guarda un bicchiere pieno a metà
    /// sul tavolino; il suo fumetto dice "HALF EMPTY!" (HALF, EMPTY e "!" sono parole separate); sul muro un cartellino
    /// "STAY FULL" (STAY e FULL separate, FULL si trascina ma ha lo stesso stile delle altre parole).
    /// Inganno: la brocca (trascinata sul bicchiere o toccata) o un tocco sul bicchiere versano acqua: "It's still half
    /// EMPTY. Just... higher."; all'orlo "Now it's going to spill.", trabocca e torna a metà da sola. Il bicchiere portato
    /// all'omino: "Now it's half empty AND wet." Niente di tutto questo cambia il suo umore, e niente si perde.
    /// Soluzione: trascinare EMPTY fuori dal fumetto (cade a terra, il fumetto mostra "HALF ___!"), poi FULL dal cartello
    /// nel buco -> "HALF FULL!" -> sorrisone esagerato, scintille, LevelReward "YOU FIXED HIS PERSONALITY!" -> Win (una
    /// volta sola). Regola documentata (ACCETTA): FULL lasciata sopra EMPTY ancora al suo posto la sostituisce (EMPTY cade).
    /// EMPTY lasciata dentro il fumetto torna al suo posto (anche ripresa da terra); FULL lasciata fuori dal buco torna al
    /// cartello; HALF, "!" e STAY non si staccano (oscillano). Le righe di parole, il fumetto, il cartellino e le zone di
    /// rilascio si calcolano ogni fotogramma dalle LARGHEZZE MISURATE dei testi (HalfFullLogic.Row/SlotWidth/InSlot): con
    /// una traduzione più lunga o più corta tutto si adatta. Dito perso (pausa, app in background, secondo dito): la parola
    /// tenuta torna da dove era partita, bicchiere e brocca tornano al loro posto. La logica è in HalfFullLogic.
    /// Tasti di prova nell'Editor: P versa, G bicchiere all'omino, M tocco sull'omino, S tocco sul cartello, H tocco su HALF,
    /// E EMPTY fuori dal fumetto, R EMPTY rimessa nel fumetto, F FULL lasciata fuori posto, W FULL nel buco (soluzione;
    /// con EMPTY ancora al suo posto la sostituisce).
    /// </summary>
    public class V77HalfFullLevel : LevelController
    {
        [Header("Omino")]
        [SerializeField] Transform man;
        [SerializeField] Collider2D manArea;
        [SerializeField] Transform manHead;
        [SerializeField] Transform headSad;
        [SerializeField] Transform headHappy;
        [SerializeField] Transform manSplash;           // lo schizzo quando gli si porta il bicchiere
        [SerializeField] Transform[] sparkles;
        [Header("Bicchiere e brocca")]
        [SerializeField] Transform glass;
        [SerializeField] Collider2D glassArea;
        [SerializeField] Transform water;               // "plain" tinto, scalato in altezza dentro il bicchiere
        [SerializeField] Transform splash;              // trabocca
        [SerializeField] Transform jug;
        [SerializeField] Collider2D jugArea;
        [Header("Fumetto e cartellino")]
        [SerializeField] Transform bubble;
        [SerializeField] SpriteRenderer bubbleArt;      // bubble_box 9-slice, misura calcolata sulle parole
        [SerializeField] Transform sign;
        [SerializeField] SpriteRenderer signPlate;      // 9-slice, misura calcolata sulle parole
        [SerializeField] BoxCollider2D signArea;
        [SerializeField] Transform signNail;
        [SerializeField] Transform floorMark;           // altezza del pavimento dove cade EMPTY
        [Header("Parole (TextMesh separati, ognuno col suo BoxCollider2D sul genitore)")]
        [SerializeField] TextMesh wordHalf;
        [SerializeField] TextMesh wordEmpty;
        [SerializeField] TextMesh wordBang;
        [SerializeField] TextMesh wordGap;              // "___" quando il posto è vuoto
        [SerializeField] TextMesh wordStay;
        [SerializeField] TextMesh wordFull;
        [SerializeField] LevelReward reward;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const string Compliment = "YOU FIXED HIS PERSONALITY!";
        const float TapSlop = 0.18f, Follow = 16f, WordLift = 1.12f;
        const float SpaceRatio = 0.4f, BangGap = 0.03f;                // spazio tra le parole (in righe), stacco del "!"
        const float BubblePadX = 0.24f, BubblePadY = 0.16f;             // margini del testo nel corpo del fumetto (come SpeechBubble)
        const float BubbleInsetX = 0.05f, BubbleInsetTop = 0.05f, BubbleInsetBottom = 0.30f;   // corpo di bubble_box dentro il rettangolo
        const float BubbleZoneMargin = 0.15f;
        const float SignPadX = 0.28f, SignPadY = 0.2f;
        const float WordHitPadX = 0.35f, WordHitPadY = 0.35f;
        const float FloorMinX = -2.0f, FloorMaxX = 2.0f, JugPourDistance = 1.25f;
        const float GlassInnerBottom = 0.06f, GlassInnerHeight = 0.84f;

        static readonly string[] HigherLines = { "It's still half EMPTY. Just... higher.", "More water. Same pessimist.", "He only sees the empty part." };
        static readonly string[] SpillingLines = { "It's already spilling!", "Wait for it to stop dripping." };
        static readonly string[] DrainedLines = { "...and it's back to half. Of course.", "Half again. He saw that coming." };
        static readonly string[] OfferLines = { "Now it's half empty AND wet.", "He won't drink it. It's half empty.", "He checked. Still half empty." };
        static readonly string[] ManLines = { "Sigh.", "Sigh...", "*long sigh*" };
        static readonly string[] SignLines = { "STAY FULL. Easy for you to say.", "STAY FULL. He never does.", "It's a sign. He ignores signs." };
        static readonly string[] StuckLines = { "That word won't budge.", "Glued on. Try another one.", "Stuck. Words can be stubborn." };
        static readonly string[] EmptyBackLines = { "Back to HALF EMPTY.", "EMPTY likes it there.", "He put it right back." };
        static readonly string[] EmptyFloorLines = { "HALF ___? Half WHAT?", "Something's missing now.", "He's lost for words." };
        static readonly string[] FullBackLines = { "FULL went back to its sign.", "That's not where FULL goes.", "Nope. Back on the wall." };

        enum Kind { None, Empty, Full, Stuck, Glass, Jug, Sign, Man }

        sealed class Grip
        {
            public Kind kind;
            public Vector2 start, last;
            public Vector3 offset;
            public bool moved;
            public Word stuck;
        }

        sealed class Word
        {
            public TextMesh text;
            public Transform t;
            public MeshRenderer mr;
            public BoxCollider2D col;
            public float width;
            public Vector3 baseScale, target;
            public float rot;
            public bool held;
        }

        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        HalfFullLogic logic;
        Camera cam;
        HwToast toast;
        Grip grip;
        Word half, empty, bang, gap, stay, full;
        Word[] words;
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        Coroutine jugCo, splashCo;
        Vector3 glassHome, jugHome, manHome, headScale, waterScale;
        Vector2 slotCenter, bubbleHalf;
        float slotWidth, lineHeight = 0.4f, waterShown = 0.5f, emptyFloorX, emptyFloorRot;
        int higherLines, spillingLines, drainedLines, offerLines, manLines, signLines, stuckLines, emptyBackLines, emptyFloorLines, fullBackLines;
        bool captured, finishing, jugBusy;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            jugCo = splashCo = null;
            if (!captured)
            {
                captured = true;
                logic = new HalfFullLogic();
                half = MakeWord(wordHalf);
                empty = MakeWord(wordEmpty);
                bang = MakeWord(wordBang);
                gap = MakeWord(wordGap);
                stay = MakeWord(wordStay);
                full = MakeWord(wordFull);
                words = new[] { half, empty, bang, gap, stay, full };
                glassHome = glass.position;
                jugHome = jug.position;
                manHome = man.position;
                headScale = manHead.localScale;
                waterScale = water.localScale;
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            grip = null;
            finishing = false;
            jugBusy = false;
            higherLines = spillingLines = drainedLines = offerLines = manLines = signLines = stuckLines = emptyBackLines = emptyFloorLines = fullBackLines = 0;
            toast = new HwToast(infoText, infoHolder);
            RestPose();
        }

        static Word MakeWord(TextMesh text)
        {
            Transform t = text.transform.parent != null ? text.transform.parent : text.transform;
            var w = new Word
            {
                text = text,
                t = t,
                mr = text.GetComponent<MeshRenderer>(),
                col = t.GetComponent<BoxCollider2D>(),
                baseScale = t.localScale,
            };
            // stima prima della prima misura vera (poi conta solo la larghezza misurata del testo)
            w.width = Mathf.Max(0.1f, (text.text != null ? text.text.Length : 1) * text.characterSize * 5.5f * Mathf.Abs(t.lossyScale.x));
            return w;
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            if (jugCo != null) { StopCoroutine(jugCo); jugCo = null; }
            if (splashCo != null) { StopCoroutine(splashCo); splashCo = null; }
            jugBusy = false;
            foreach (Word w in words) { w.held = false; w.t.localScale = w.baseScale; }
            emptyFloorX = 0f;
            emptyFloorRot = 0f;
            glass.position = glassHome;
            glass.localRotation = Quaternion.identity;
            jug.position = jugHome;
            jug.localRotation = Quaternion.identity;
            man.position = manHome;
            manHead.localScale = headScale;
            manHead.localRotation = Quaternion.identity;
            HalloweenKit.Show(headSad, true);
            HalloweenKit.Show(headHappy, false);
            HalloweenKit.Show(manSplash, false);
            HalloweenKit.Show(splash, false);
            if (sparkles != null) foreach (Transform s in sparkles) HalloweenKit.Show(s, false);
            waterShown = logic.WaterFraction;
            WaterPose();
            MeasureWords();
            Layout(true);
        }

        void WaterPose()
        {
            float h = GlassInnerHeight * Mathf.Clamp01(waterShown);
            water.localScale = new Vector3(waterScale.x, Mathf.Max(0.001f, h), waterScale.z);
            water.localPosition = new Vector3(water.localPosition.x, GlassInnerBottom + h * 0.5f, water.localPosition.z);
            if (splash != null && splash.gameObject.activeSelf != logic.Spilling) HalloweenKit.Show(splash, logic.Spilling);
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: si molla tutto, si torna a uno stato valido
            CancelGrip();
            StopFx();
            jugCo = splashCo = null;
            jugBusy = false;
            if (captured && !finishing && !IsFinished && logic != null) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa finale: la si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == HalfFullStage.Celebrating) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrip(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrip(); }

        /// <summary>Il dito sparisce: la parola tenuta torna da dove era partita, bicchiere e brocca tornano a posto (scivolando).</summary>
        void CancelGrip()
        {
            Grip g = grip;
            grip = null;
            if (logic != null) logic.Cancel();
            if (words != null) foreach (Word w in words) if (w.held) { w.held = false; w.t.localScale = w.baseScale; }
            if (g == null) return;
            if (g.kind == Kind.Glass && glass != null) glass.localRotation = Quaternion.identity;
        }

        void Update()
        {
            toast?.Tick();
            if (IsFinished || toast == null || logic == null) return;
            if (!finishing)
            {
                if (Time.timeScale <= 0f) { CancelGrip(); return; }   // pausa o pubblicità: niente input
                if (logic.Tick(Time.deltaTime)) toast.Show(DrainedLines[drainedLines++ % DrainedLines.Length], 1.6f);
#if UNITY_EDITOR
                EditorKeys();
#endif
                if (!finishing) ReadPointer();
            }
            MeasureWords();
            Layout(false);
            Ambient();
        }

        /// <summary>Acqua che sale/scende morbida, bicchiere e brocca che tornano a casa se nessuno li tiene, omino che respira.</summary>
        void Ambient()
        {
            float k = 1f - Mathf.Exp(-Time.deltaTime * 6f);
            waterShown = Mathf.Lerp(waterShown, logic.WaterFraction, k);
            WaterPose();
            float f = 1f - Mathf.Exp(-Time.deltaTime * Follow);
            if (grip == null || grip.kind != Kind.Glass)
            {
                glass.position = Vector3.Lerp(glass.position, glassHome, f);
                glass.localRotation = Quaternion.Slerp(glass.localRotation, Quaternion.identity, f);
            }
            if ((grip == null || grip.kind != Kind.Jug) && !jugBusy)
            {
                jug.position = Vector3.Lerp(jug.position, jugHome, f);
                jug.localRotation = Quaternion.Slerp(jug.localRotation, Quaternion.identity, f);
            }
            if (!finishing && logic.Stage == HalfFullStage.Playing)
                manHead.localRotation = Quaternion.Euler(0f, 0f, 2.5f * Mathf.Sin(Time.time * 1.3f) - 4f);   // testa china, dondola
        }

        // ---------- Misura e impaginazione (dalle larghezze vere dei testi) ----------

        /// <summary>Larghezza di ogni parola in coordinate del genitore, letta dal testo disegnato (solo se non è ruotata).</summary>
        void MeasureWords()
        {
            foreach (Word w in words)
            {
                if (w.mr == null || !w.t.gameObject.activeInHierarchy) continue;
                if (Quaternion.Angle(w.t.rotation, Quaternion.identity) > 0.5f) continue;
                float sx = Mathf.Abs(w.t.lossyScale.x) / Mathf.Max(1e-4f, Mathf.Abs(w.baseScale.x));   // senza il "sollevamento" della presa
                Vector3 size = w.mr.bounds.size;
                if (size.x > 0.01f && sx > 1e-4f) w.width = size.x / sx;
                if (w == half && size.y > 0.01f && sx > 1e-4f) lineHeight = size.y / sx;
            }
        }

        /// <summary>Righe del fumetto e del cartellino, misura dei disegni 9-slice, collider delle parole, posizioni di arrivo.</summary>
        void Layout(bool snap)
        {
            float space = lineHeight * SpaceRatio;
            HalfFullSlot slot = logic.Slot;
            slotWidth = HalfFullLogic.SlotWidth(slot, empty.width, full.width, gap.width);

            // fumetto: HALF [posto] !
            float[] c = HalfFullLogic.Row(new[] { half.width, slotWidth, bang.width }, new[] { 0f, space, BangGap }, out float total);
            float bodyW = total + 2f * BubblePadX, bodyH = lineHeight + 2f * BubblePadY;
            if (bubbleArt != null)
            {
                bubbleArt.size = new Vector2(bodyW + 2f * BubbleInsetX, bodyH + BubbleInsetTop + BubbleInsetBottom);
                // il corpo resta centrato sull'origine del fumetto (la codina sta nel bordo in basso)
                bubbleArt.transform.localPosition = new Vector3(0f, -(BubbleInsetBottom - BubbleInsetTop) * 0.5f, bubbleArt.transform.localPosition.z);
            }
            bubbleHalf = new Vector2(bodyW * 0.5f + BubbleZoneMargin, bodyH * 0.5f + BubbleZoneMargin);
            Vector3 bp = bubble.position;
            slotCenter = new Vector2(bp.x + c[1], bp.y);
            half.target = new Vector3(bp.x + c[0], bp.y, half.t.position.z);
            bang.target = new Vector3(bp.x + c[2], bp.y, bang.t.position.z);
            gap.target = new Vector3(slotCenter.x, bp.y, gap.t.position.z);
            SetAlpha(gap.text, slot == HalfFullSlot.Gap ? 1f : 0f);

            // cartellino: STAY FULL (il posto di FULL resta largo quanto FULL anche quando FULL è via)
            float[] s = HalfFullLogic.Row(new[] { stay.width, full.width }, new[] { 0f, space }, out float signTotal);
            Vector2 plate = new Vector2(signTotal + 2f * SignPadX, lineHeight + 2f * SignPadY);
            if (signPlate != null) signPlate.size = plate;
            if (signArea != null) signArea.size = plate;
            if (signNail != null) signNail.localPosition = new Vector3(0f, plate.y * 0.5f + 0.38f, signNail.localPosition.z);
            Vector3 sp = sign.position;
            stay.target = new Vector3(sp.x + s[0], sp.y, stay.t.position.z);
            Vector3 fullOnSign = new Vector3(sp.x + s[1], sp.y, full.t.position.z);

            // EMPTY: nel posto o a terra; FULL: sul cartello o nel posto
            Vector3 inSlot = new Vector3(slotCenter.x, bp.y, empty.t.position.z);
            float floorY = floorMark != null ? floorMark.position.y : -4.1f;
            empty.target = logic.EmptyAt == HalfFullEmptyAt.Floor ? new Vector3(emptyFloorX, floorY, empty.t.position.z) : inSlot;
            empty.rot = logic.EmptyAt == HalfFullEmptyAt.Floor ? emptyFloorRot : 0f;
            full.target = logic.FullAt == HalfFullFullAt.Bubble ? new Vector3(slotCenter.x, bp.y, full.t.position.z) : fullOnSign;
            full.rot = 0f;

            float f = snap ? 1f : 1f - Mathf.Exp(-Time.deltaTime * Follow);
            foreach (Word w in words)
            {
                if (w.col != null) w.col.size = new Vector2(w.width + WordHitPadX, lineHeight + WordHitPadY);
                if (w.held) continue;
                w.t.position = Vector3.Lerp(w.t.position, w.target, f);
                w.t.localRotation = Quaternion.Slerp(w.t.localRotation, Quaternion.Euler(0f, 0f, w.rot), f);
            }
        }

        static void SetAlpha(TextMesh t, float a)
        {
            if (t == null) return;
            Color c = t.color;
            if (Mathf.Approximately(c.a, a)) return;
            c.a = a;
            t.color = c;
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73-0.76) ----------

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
            if (Input.touchCount >= 2) { CancelGrip(); return; }   // due dita: il gesto si annulla (la parola torna a casa)
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (grip != null) End(grip);
                return;
            }
            if (began) { if (grip != null) End(grip); grip = Begin(w); }
            if (grip == null) return;
            grip.last = w;
            if (!grip.moved && Vector2.Distance(w, grip.start) > TapSlop) grip.moved = true;
            if (grip.moved) Drag(grip, w);
            if (ended && grip != null) End(grip);
        }

        bool Hit(Word w, Vector2 p) => w != null && w.col != null && w.t.gameObject.activeInHierarchy && w.col.OverlapPoint(p);

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, kind = Kind.None };
            if (finishing || logic.InputLocked) return g;
            // prima le parole che si staccano (bersagli generosi), poi quelle incollate, poi gli oggetti
            if (logic.FullAt == HalfFullFullAt.Sign && Hit(full, w) && logic.GrabFull()) return Hold(g, Kind.Full, full, w);
            if (logic.EmptyAt != HalfFullEmptyAt.Held && Hit(empty, w) && logic.GrabEmpty()) return Hold(g, Kind.Empty, empty, w);
            foreach (Word s in new[] { half, bang, stay })
                if (Hit(s, w)) { g.kind = Kind.Stuck; g.stuck = s; return g; }
            if (glassArea.OverlapPoint(w)) { g.kind = Kind.Glass; g.offset = glass.position - (Vector3)w; return g; }
            if (jugArea.OverlapPoint(w) && !jugBusy) { g.kind = Kind.Jug; g.offset = jug.position - (Vector3)w; return g; }
            if (signArea != null && signArea.OverlapPoint(w)) { g.kind = Kind.Sign; return g; }
            if (manArea.OverlapPoint(w)) g.kind = Kind.Man;
            return g;
        }

        Grip Hold(Grip g, Kind kind, Word word, Vector2 w)
        {
            g.kind = kind;
            g.offset = word.t.position - (Vector3)w;
            word.held = true;
            word.t.localScale = word.baseScale * WordLift;
            word.t.localRotation = Quaternion.identity;
            StopFxOn(word.t);
            Haptics.Tap();
            V77HalfFullSound.Play(this, "pick", 0.4f);
            return g;
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

        void Drag(Grip g, Vector2 w)
        {
            Transform t = g.kind == Kind.Full ? full.t : g.kind == Kind.Empty ? empty.t : g.kind == Kind.Glass ? glass : g.kind == Kind.Jug ? jug : null;
            if (t == null) return;
            Vector2 p = HalloweenKit.ClampSafe(w + (Vector2)g.offset);
            t.position = new Vector3(p.x, p.y, t.position.z);
            if (g.kind == Kind.Jug) jug.localRotation = Quaternion.Euler(0f, 0f, Vector2.Distance(p, glass.position) < JugPourDistance ? 35f : 0f);
        }

        void End(Grip g)
        {
            grip = null;
            switch (g.kind)
            {
                case Kind.Empty: DropEmpty(g.moved); break;
                case Kind.Full: DropFull(g.moved); break;
                case Kind.Stuck: TapStuck(g.stuck); break;
                case Kind.Glass:
                    if (!g.moved) Pour();
                    else if (manArea.OverlapPoint(g.last) || manArea.OverlapPoint(glass.position)) OfferGlass();
                    break;
                case Kind.Jug:
                    if (!g.moved || Vector2.Distance(jug.position, glass.position) < JugPourDistance) Pour();
                    break;
                case Kind.Sign: TapSign(); break;
                case Kind.Man: TapMan(); break;
            }
        }

        // ---------- Le parole ----------

        void DropEmpty(bool wasMoved)
        {
            empty.held = false;
            empty.t.localScale = empty.baseScale;
            Vector3 p = empty.t.position;
            Vector3 bp = bubble.position;
            bool inside = HalfFullLogic.InRect(p.x, p.y, bp.x, bp.y, bubbleHalf.x, bubbleHalf.y);
            switch (logic.ReleaseEmpty(inside))
            {
                case HalfFullEmptyDrop.Returned:
                    // torna al suo posto (Layout la fa scivolare); un tocco senza trascinare fa solo oscillare
                    if (wasMoved) toast.Show(EmptyBackLines[emptyBackLines++ % EmptyBackLines.Length], 1.5f);
                    FxWiggle(empty.t, 8f, 0.35f, 30f);
                    break;
                case HalfFullEmptyDrop.Floor:
                    emptyFloorX = Mathf.Clamp(p.x, FloorMinX, FloorMaxX);
                    emptyFloorRot = (emptyFloorX >= 0f ? -1f : 1f) * 12f;
                    V77HalfFullSound.Play(this, "thud", 0.5f);
                    Haptics.Light();
                    if (wasMoved) toast.Show(EmptyFloorLines[emptyFloorLines++ % EmptyFloorLines.Length], 1.8f);
                    FxShake(manHead, 0.03f, 0.3f);
                    break;
            }
        }

        void DropFull(bool wasMoved)
        {
            full.held = false;
            full.t.localScale = full.baseScale;
            Vector3 p = full.t.position;
            bool inSlot = HalfFullLogic.InSlot(p.x, p.y, slotCenter.x, slotCenter.y, slotWidth, full.width, lineHeight);
            switch (logic.ReleaseFull(inSlot))
            {
                case HalfFullFullDrop.Returned:
                    if (wasMoved) toast.Show(FullBackLines[fullBackLines++ % FullBackLines.Length], 1.5f);
                    FxWiggle(full.t, 8f, 0.35f, 30f);
                    break;
                case HalfFullFullDrop.Swapped:
                    // regola ACCETTA: EMPTY viene spinta giù dal fumetto e cade a terra
                    emptyFloorX = Mathf.Clamp(empty.t.position.x - 0.4f, FloorMinX, FloorMaxX);
                    emptyFloorRot = (emptyFloorX >= 0f ? -1f : 1f) * 12f;
                    V77HalfFullSound.Play(this, "thud", 0.5f);
                    StartCoroutine(Finale());
                    break;
                case HalfFullFullDrop.Placed:
                    StartCoroutine(Finale());
                    break;
            }
        }

        void TapStuck(Word w)
        {
            if (!logic.TapStuckWord()) return;
            FxWiggle(w.t, 10f, 0.4f, 34f);
            V77HalfFullSound.Play(this, "pick", 0.25f);
            toast.Show(StuckLines[stuckLines++ % StuckLines.Length], 1.5f);
        }

        // ---------- Le false piste ----------

        void Pour()
        {
            switch (logic.Pour())
            {
                case HalfFullPour.Higher:
                    PourAnim();
                    toast.Show(HigherLines[higherLines++ % HigherLines.Length], 2.0f);
                    break;
                case HalfFullPour.Brim:
                    PourAnim();
                    if (splashCo != null) StopCoroutine(splashCo);
                    splashCo = StartCoroutine(SpillShake());
                    toast.Show("Now it's going to spill.", 2.0f);
                    break;
                case HalfFullPour.Spilling:
                    FxWiggle(glass, 6f, 0.3f, 30f);
                    toast.Show(SpillingLines[spillingLines++ % SpillingLines.Length], 1.4f);
                    break;
            }
        }

        /// <summary>La brocca vola sopra il bicchiere, si inclina e versa, poi torna a casa (Ambient la riporta).</summary>
        void PourAnim()
        {
            if (jugCo != null) StopCoroutine(jugCo);
            jugCo = StartCoroutine(JugPour());
        }

        IEnumerator JugPour()
        {
            jugBusy = true;
            Vector3 above = glassHome + new Vector3(0.55f, 1.05f, 0f);
            above.z = jug.position.z;
            yield return HalloweenKit.Move(jug, above, 0.15f);
            jug.localRotation = Quaternion.Euler(0f, 0f, 40f);
            V77HalfFullSound.Play(this, "pour", 0.55f);
            yield return new WaitForSeconds(0.35f);
            jug.localRotation = Quaternion.identity;
            jugBusy = false;
            jugCo = null;
            FxShake(manHead, 0.02f, 0.25f);
        }

        IEnumerator SpillShake()
        {
            yield return new WaitForSeconds(0.25f);
            V77HalfFullSound.Play(this, "splash", 0.5f);
            Haptics.Light();
            FxWiggle(glass, 4f, 0.5f, 40f);
            splashCo = null;
        }

        void OfferGlass()
        {
            if (!logic.OfferGlass()) return;
            toast.Show(OfferLines[offerLines++ % OfferLines.Length], 2.0f);
            V77HalfFullSound.Play(this, "splash", 0.45f);
            FxShake(man, 0.04f, 0.35f);
            if (splashCo != null) StopCoroutine(splashCo);
            splashCo = StartCoroutine(ManSplash());
        }

        IEnumerator ManSplash()
        {
            HalloweenKit.Show(manSplash, true);
            yield return new WaitForSeconds(0.6f);
            HalloweenKit.Show(manSplash, false);
            splashCo = null;
        }

        void TapMan()
        {
            if (!logic.TapMan()) return;
            V77HalfFullSound.Play(this, "sigh", 0.5f);
            FxPop(manHead, 0.94f, 0.4f);
            toast.Show(ManLines[manLines++ % ManLines.Length], 1.3f);
        }

        void TapSign()
        {
            if (!logic.TapSign()) return;
            FxWiggle(sign, 5f, 0.45f, 22f);
            toast.Show(SignLines[signLines++ % SignLines.Length], 1.9f);
        }

        // ---------- La soluzione ----------

        /// <summary>
        /// "HALF FULL!": FULL scivola nel buco, l'omino si illumina con un sorrisone esagerato e un saltello, scintille;
        /// poi la festa comune e Win, una volta sola. Rifatta da capo se il livello si spegne a metà.
        /// </summary>
        IEnumerator Finale()
        {
            finishing = true;
            CancelGrip();
            StopFx();
            if (jugCo != null) { StopCoroutine(jugCo); jugCo = null; }
            if (splashCo != null) { StopCoroutine(splashCo); splashCo = null; }
            jugBusy = false;
            HalloweenKit.Show(manSplash, false);
            man.position = manHome;
            manHead.localScale = headScale;
            manHead.localRotation = Quaternion.identity;
            if (logic.EmptyAt == HalfFullEmptyAt.Floor && emptyFloorX == 0f && emptyFloorRot == 0f) { emptyFloorX = -0.6f; emptyFloorRot = 12f; }   // ripresa dopo uno spegnimento
            V77HalfFullSound.Play(this, "click", 0.5f);
            yield return new WaitForSeconds(0.35f);   // FULL si accomoda nel fumetto (Layout)
            FxPop(full.t, 1.25f, 0.3f);
            yield return new WaitForSeconds(0.2f);
            // il sorrisone esagerato
            HalloweenKit.Show(headSad, false);
            HalloweenKit.Show(headHappy, true);
            V77HalfFullSound.Play(this, "ding", 0.65f);
            Haptics.Medium();
            if (sparkles != null)
                for (int i = 0; i < sparkles.Length; i++)
                    if (sparkles[i] != null) { HalloweenKit.Show(sparkles[i], true); StartCoroutine(Sparkle(sparkles[i], i * 0.06f)); }
            StartCoroutine(HalloweenKit.ScaleTo(manHead, headScale * 1.35f, 0.18f));
            yield return HalloweenKit.Move(man, manHome + new Vector3(0f, 0.35f, 0f), 0.2f);
            yield return HalloweenKit.Move(man, manHome, 0.18f);
            yield return HalloweenKit.Move(man, manHome + new Vector3(0f, 0.18f, 0f), 0.14f);
            yield return HalloweenKit.Move(man, manHome, 0.12f);
            yield return HalloweenKit.ScaleTo(manHead, headScale * 1.2f, 0.15f);
            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            if (logic.TryComplete()) Win(Compliment);
        }

        IEnumerator Sparkle(Transform s, float delay)
        {
            Vector3 baseScale = Vector3.one;
            s.localScale = Vector3.zero;
            if (delay > 0f) yield return new WaitForSeconds(delay);
            for (float t = 0f; t < 2.6f; t += Time.deltaTime)
            {
                float pop = t < 0.25f ? t / 0.25f : 1f + 0.15f * Mathf.Sin(t * 9f);
                s.localScale = baseScale * pop;
                s.localRotation = Quaternion.Euler(0f, 0f, t * 140f);
                yield return null;
            }
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked || grip != null || logic.Holding) return;
            if (Input.GetKeyDown(KeyCode.P)) Pour();
            if (Input.GetKeyDown(KeyCode.G)) OfferGlass();
            if (Input.GetKeyDown(KeyCode.M)) TapMan();
            if (Input.GetKeyDown(KeyCode.S)) TapSign();
            if (Input.GetKeyDown(KeyCode.H)) TapStuck(half);
            if (Input.GetKeyDown(KeyCode.E) && logic.EmptyAt == HalfFullEmptyAt.Bubble && logic.GrabEmpty())
            {
                empty.t.position = bubble.position + new Vector3(0.6f, -2.4f, 0f);
                DropEmpty(true);
            }
            if (Input.GetKeyDown(KeyCode.R) && logic.EmptyAt == HalfFullEmptyAt.Floor && logic.GrabEmpty())
            {
                empty.t.position = new Vector3(slotCenter.x, slotCenter.y, empty.t.position.z);
                DropEmpty(true);
            }
            if (Input.GetKeyDown(KeyCode.F) && logic.GrabFull())
            {
                full.t.position = new Vector3(0f, -1f, full.t.position.z);
                DropFull(true);
            }
            if (Input.GetKeyDown(KeyCode.W) && logic.GrabFull())
            {
                full.t.position = new Vector3(slotCenter.x, slotCenter.y, full.t.position.z);
                DropFull(true);
            }
        }
#endif
    }

    /// <summary>Suoni sintetizzati di Half Full: presa della parola, tonfo, acqua versata, schizzo, sospiro, clic, "ding" felice.</summary>
    public static class V77HalfFullSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(279);

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
                case "pick":     // la parola che si stacca: "plip"
                    c = Make(kind, 0.08f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(600f, 1100f, t / 0.08f)) * 0.4f * Mathf.Exp(-t * 30f));
                    break;
                case "thud":     // la parola che cade a terra
                    c = Make(kind, 0.22f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(140f, 60f, t / 0.22f)) * 0.6f * Mathf.Exp(-t * 18f) + N() * 0.2f * Mathf.Exp(-t * 45f));
                    break;
                case "pour":     // acqua che scende: gorgoglio
                    {
                        float lp = 0f;
                        c = Make(kind, 0.45f, t =>
                        {
                            lp += (N() - lp) * 0.25f;
                            float env = Mathf.Min(1f, t * 20f) * Mathf.Clamp01((0.45f - t) * 6f);
                            return (lp * 0.6f + 0.25f * Mathf.Sin(t * 2f * Mathf.PI * (380f + 120f * Mathf.Sin(t * 2f * Mathf.PI * 9f)))) * env;
                        });
                        break;
                    }
                case "splash":   // schizzo
                    c = Make(kind, 0.3f, t => N() * 0.45f * Mathf.Exp(-t * 12f) + Mathf.Sin(t * 2f * Mathf.PI * 520f) * 0.15f * Mathf.Exp(-t * 20f));
                    break;
                case "sigh":     // sospiro: soffio che scende
                    {
                        float lp = 0f;
                        c = Make(kind, 0.7f, t =>
                        {
                            lp += (N() - lp) * 0.1f;
                            return (lp * 0.8f + 0.12f * Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(220f, 140f, t / 0.7f))) * Mathf.Sin(t / 0.7f * Mathf.PI);
                        });
                        break;
                    }
                case "click":    // FULL che si incastra nel fumetto
                    c = Make(kind, 0.06f, t => N() * 0.5f * Mathf.Exp(-t * 110f) + Mathf.Sin(t * 2f * Mathf.PI * 1200f) * 0.25f * Mathf.Exp(-t * 70f));
                    break;
                case "ding":     // il sorriso: due note in salita
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
            AudioClip clip = AudioClip.Create("v77b_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
