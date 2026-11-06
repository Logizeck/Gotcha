using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 72 della roadmap "The Short Ladder" (id short_ladder) — "REACH THE WINDOW." Una facciata di mattoni con
    /// una finestra molto in alto, un omino col berretto sul marciapiede e una scala troppo corta (4 pioli) appoggiata al
    /// muro lontano dalla finestra.
    /// Inganno: la scala corta messa sotto la finestra non ci arriva: l'omino ci va, sale fino in cima, "Hmm. Not
    /// quite." e riscende (riprendere la scala mentre sale lo fa saltare giù: "Hey! I was using that!"); sollevare la
    /// scala non serve (posata torna con i piedi a terra); trascinare o sollevare l'omino è inutile: ricade a terra
    /// ("Jumping won't help."). Tutto reversibile, mai sconfitta.
    /// Soluzione: pizzico a due dita sulla scala per ALLUNGARLA (pioli interi da 0.5 che compaiono uno a uno, sempre
    /// equidistanti; da 4 a 13 pioli, ne servono 10: quando basta, scintilla e "ding"), poi trascinarla sotto la
    /// finestra (colonna x 0.9 ± 0.55). Posata lì (o pizzicata lì), l'omino ci corre, sale da solo ed entra dalla
    /// finestra; saluta dal davanzale; LevelReward "ENGINEERING GENIUS!" -> Win una volta sola.
    /// Dita: al massimo due (V64AFingers). Due dita sulla scala fanno il pizzico (agisce solo sulla scala); alzando un
    /// dito, l'altro continua a trascinarla (niente blocchi). Scala e omino possono essere tenuti da due dita diverse.
    /// Limiti: la scala resta nello schermo, non passa sotto il pavimento, la cima non supera la barra dell'obiettivo.
    /// Pausa, app in background o livello spento lasciano andare tutto (la lunghezza raggiunta resta).
    /// La logica è in Gotcha.Levels.V64.ShortLadderLogic.
    /// Tasti di prova nell'Editor: W soluzione (allunga a 11 e mette sotto la finestra), S scala corta sotto la
    /// finestra, L scala lunga lontano dalla finestra, E allunga di 2 pioli sul posto, X accorcia di 2, M omino
    /// sollevato e lasciato, T tocco sull'omino, K tocco sulla scala, P pausa simulata (dita perse), R ricomincia.
    /// Shift + clic pianta un secondo dito (V64AFingers) per il pizzico.
    /// </summary>
    public class V84ShortLadderLevel : LevelController
    {
        [Header("Scala (radice ai piedi, al centro; i tratti si accendono uno a uno)")]
        [SerializeField] Transform ladder;
        [SerializeField] Transform ladderBody;
        [SerializeField] Transform[] segments;
        [SerializeField] Transform ladderTop;
        [SerializeField] BoxCollider2D ladderArea;
        [SerializeField] SpriteRenderer sparkle;
        [Header("Omino (radice ai piedi; Body per gli effetti)")]
        [SerializeField] Transform man;
        [SerializeField] Transform manBody;
        [SerializeField] Transform manStand;
        [SerializeField] Transform manClimb;
        [SerializeField] Collider2D manArea;
        [SerializeField] Transform manShadow;
        [Header("Finestra")]
        [SerializeField] SpriteRenderer windowGlass;
        [SerializeField] Transform waveMan;
        [SerializeField] Transform waveArm;
        [Header("Fumetto, scritte e festa")]
        [SerializeField] SpeechBubble manBubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "ENGINEERING GENIUS!";
        const float TapSlop = 0.15f, PinchNear = 0.5f, PinchMinD = 0.3f;
        const float WalkSpeed = 3.5f, ClimbSpeed = 2.8f, WinWalkSpeed = 6f, WinClimbSpeed = 7f;
        const float SillY = 1.55f;                 // davanzale (DoodleSprites.V84BWindowY - V84BWindowHy)
        static readonly Vector2 BubbleFromFeet = new Vector2(1.05f, 2.1f);   // fumetto: la codina punta alla testa
        const float BubbleMaxY = 1.9f;

        static readonly string[] TooShortLines = { "Hmm. Not quite.", "So close. Not really.", "The window is up THERE.", "I need more ladder." };
        static readonly string[] JumpLines = { "Jumping won't help.", "I can't fly!", "Put me down!", "Still on the ground." };
        static readonly string[] ManTapLines = { "I need to get in there!", "My keys are inside.", "Help me reach the window!", "Up there. The window." };
        static readonly string[] WrongSpotLines = { "Nice ladder. Wrong spot.", "Long enough... for the bricks.", "The window is over there." };
        static readonly string[] ShortTapLines = { "A very short ladder.", "Tiny ladder. Big wall.", "That's barely a step." };
        static readonly string[] GrewLines = { "A bit longer. Not enough.", "Longer! Keep going.", "More rungs, please." };
        static readonly string[] ShrankLines = { "Shorter. Interesting choice.", "Wrong direction." };
        const string LongTapLine = "Now THAT's a ladder.";
        const string HeldShortLine = "Still too short.";
        const string InterruptLine = "Hey! I was using that!";
        const string InLine = "I'M IN!";

        enum Kind { None, Ladder, Man, Pinch }

        sealed class Grip
        {
            public Kind kind;
            public Vector2 start, last;
            public Vector3 offset;
            public bool moved;
        }

        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        ShortLadderLogic logic;
        Camera cam;
        HwToast toast;
        HwSay say;
        readonly Dictionary<int, Grip> grips = new Dictionary<int, Grip>();
        readonly List<int> gone = new List<int>();
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        Vector3 ladderZ, manZ, waveRest = Vector3.one, segScale = Vector3.one;
        Vector2 ladderVis, manVis;
        float ladderVelX, tilt, glassAlpha = 1f, pinchD0 = 1f, sparkleT = -1f, lastRung = -10f;
        int shownSegments, pinchSegs0;
        int tooShortLines, jumpLines, manTapLines, wrongSpotLines, shortTapLines, grewLines, shrankLines;
        bool captured, finishing, attempting;
        Coroutine attemptCo, waveCo;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            attemptCo = waveCo = null;
            StopFx();
            if (!captured)
            {
                captured = true;
                logic = new ShortLadderLogic();
                ladderZ = new Vector3(0f, 0f, ladder.position.z);
                manZ = new Vector3(0f, 0f, man.position.z);
                if (waveMan != null) waveRest = waveMan.localScale;
                if (segments != null && segments.Length > 0 && segments[0] != null && segments[0].localScale != Vector3.zero) segScale = segments[0].localScale;
                if (windowGlass != null) glassAlpha = windowGlass.color.a;
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            V64AFingers.Reset();
            grips.Clear();
            finishing = attempting = false;
            tooShortLines = jumpLines = manTapLines = wrongSpotLines = shortTapLines = grewLines = shrankLines = 0;
            toast = new HwToast(infoText, infoHolder);
            say = new HwSay(manBubble);
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            say?.Hide();
            ladderVis = new Vector2(logic.LadderX, logic.BottomY);
            ladderVelX = tilt = 0f;
            ladder.position = ladderZ + (Vector3)ladderVis;
            ladder.localRotation = Quaternion.identity;
            shownSegments = -1;
            ShowSegments(logic.Segments, false);
            manVis = new Vector2(logic.ManX, logic.ManY);
            PlaceMan(manVis);
            man.localRotation = Quaternion.identity;
            HalloweenKit.Show(man, true);
            ManPose(false);
            if (manBody != null) { manBody.localPosition = Vector3.zero; manBody.localRotation = Quaternion.identity; }
            HalloweenKit.Show(waveMan, false);
            if (waveMan != null) waveMan.localScale = waveRest;
            if (waveArm != null) waveArm.localRotation = Quaternion.identity;
            HalloweenKit.SetAlpha(windowGlass, glassAlpha);
            HalloweenKit.Show(windowGlass, true);
            sparkleT = -1f;
            HalloweenKit.Show(sparkle, false);
        }

        void ManPose(bool climbing)
        {
            HalloweenKit.Show(manStand, !climbing);
            HalloweenKit.Show(manClimb, climbing);
        }

        void PlaceMan(Vector2 feet)
        {
            man.position = manZ + (Vector3)feet;
            if (manShadow != null)
            {
                float h = Mathf.Max(0f, feet.y - ShortLadderLogic.FloorY);
                manShadow.position = new Vector3(feet.x, ShortLadderLogic.FloorY + 0.02f, manShadow.position.z);
                manShadow.localScale = new Vector3(0.9f, 0.22f, 1f) * Mathf.Clamp(1f - h * 0.25f, 0.35f, 1f);
            }
        }

        /// <summary>Accende i primi n tratti (i nuovi fanno un piccolo "pop"), mette i tappi in cima e adatta il bersaglio.</summary>
        void ShowSegments(int n, bool pop)
        {
            if (segments == null) return;
            n = Mathf.Clamp(n, 0, segments.Length);
            if (n == shownSegments) return;
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i] == null) continue;
                bool on = i < n;
                bool was = segments[i].gameObject.activeSelf;
                segments[i].gameObject.SetActive(on);
                if (on && !was && pop && shownSegments >= 0) FxPop(segments[i], 1.15f, 0.18f);
                else if (!on) segments[i].localScale = segScale;
            }
            shownSegments = n;
            float len = n * ShortLadderLogic.SegmentHeight;
            if (ladderTop != null) ladderTop.localPosition = new Vector3(0f, len, ladderTop.localPosition.z);
            if (ladderArea != null)
            {
                ladderArea.size = new Vector2(1.5f, len + 0.6f);
                ladderArea.offset = new Vector2(0f, len * 0.5f);
            }
        }

        Vector3 LadderTopWorld() => ladder.TransformPoint(new Vector3(0f, shownSegments * ShortLadderLogic.SegmentHeight, 0f));

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: si molla tutto, si torna a uno stato valido
            CancelAll();
            StopFx();
            attemptCo = waveCo = null;
            attempting = false;
            if (captured && !finishing && !IsFinished && logic != null) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == ShortLadderStage.Celebrating) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelAll(); }

        /// <summary>Le dita spariscono: scala e omino tornano a terra (Pose li fa scendere), la prova di salita si interrompe.</summary>
        void CancelAll()
        {
            grips.Clear();
            V64AFingers.Reset();
            if (logic == null || logic.InputLocked) return;
            logic.Cancel();
            if (attempting) StopAttempt();
        }

        void Update()
        {
            toast?.Tick();
            say?.Tick();
            if (IsFinished || toast == null || logic == null) return;
            if (finishing) return;
            if (Time.timeScale <= 0f) { CancelAll(); return; }   // pausa o pubblicità: niente input
#if UNITY_EDITOR
            EditorKeys();
#endif
            if (!finishing) ReadFingers();
            if (!finishing) Pose();
        }

        /// <summary>
        /// La scala: tenuta segue il dito (già limitata dalla logica) e si inclina un poco col movimento; lasciata scende
        /// con i piedi sul pavimento. L'omino (se non sta salendo): tenuto penzola, lasciato ricade. La scintilla in cima.
        /// </summary>
        void Pose()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            Vector2 target = new Vector2(logic.LadderX, logic.BottomY);
            float before = ladderVis.x;
            if (logic.LadderHeld) ladderVis = Vector2.Lerp(ladderVis, target, 1f - Mathf.Exp(-dt * 30f));
            else
            {
                ladderVis.x = Mathf.Lerp(ladderVis.x, target.x, 1f - Mathf.Exp(-dt * 20f));
                ladderVis.y = Mathf.Lerp(ladderVis.y, target.y, 1f - Mathf.Exp(-dt * 16f));
                if (Mathf.Abs(ladderVis.y - target.y) < 0.002f) ladderVis.y = target.y;
            }
            ladderVis.y = Mathf.Max(ladderVis.y, ShortLadderLogic.FloorY);   // mai sotto il pavimento, neanche nell'animazione
            ladderVelX = dt > 0f ? Mathf.Lerp(ladderVelX, (ladderVis.x - before) / dt, 1f - Mathf.Exp(-dt * 10f)) : 0f;
            float wantTilt = logic.LadderHeld ? Mathf.Clamp(-ladderVelX * 2.2f, -7f, 7f) : 0f;
            tilt = Mathf.Lerp(tilt, wantTilt, 1f - Mathf.Exp(-dt * 12f));
            ladder.position = ladderZ + (Vector3)ladderVis;
            ladder.localRotation = Quaternion.Euler(0f, 0f, tilt);
            ShowSegments(logic.Segments, true);

            if (!attempting)
            {
                Vector2 m = new Vector2(logic.ManX, logic.ManY);
                if (logic.ManHeld)
                {
                    manVis = Vector2.Lerp(manVis, m, 1f - Mathf.Exp(-dt * 30f));
                    man.localRotation = Quaternion.Euler(0f, 0f, 9f * Mathf.Sin(Time.time * 9f));   // scalcia, appeso al dito
                }
                else
                {
                    manVis.x = Mathf.Lerp(manVis.x, m.x, 1f - Mathf.Exp(-dt * 20f));
                    manVis.y = Mathf.Lerp(manVis.y, m.y, 1f - Mathf.Exp(-dt * 14f));
                    if (Mathf.Abs(manVis.y - m.y) < 0.002f) manVis.y = m.y;
                    man.localRotation = Quaternion.Slerp(man.localRotation, Quaternion.identity, 1f - Mathf.Exp(-dt * 14f));
                }
                PlaceMan(manVis);
            }
            TickSparkle(dt);
        }

        void TickSparkle(float dt)
        {
            if (sparkle == null) return;
            if (sparkleT < 0f) { if (sparkle.gameObject.activeSelf) sparkle.gameObject.SetActive(false); return; }
            sparkleT += dt;
            float k = sparkleT / 0.6f;
            if (k >= 1f) { sparkleT = -1f; sparkle.gameObject.SetActive(false); return; }
            sparkle.gameObject.SetActive(true);
            Vector3 top = LadderTopWorld();
            sparkle.transform.position = new Vector3(top.x + 0.2f, top.y + 0.1f, sparkle.transform.position.z);
            sparkle.transform.localScale = Vector3.one * (1.1f * Mathf.Sin(k * Mathf.PI));
            sparkle.transform.localRotation = Quaternion.Euler(0f, 0f, k * 120f);
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73-0.83) ----------

        void FxPop(Transform t, float peak, float seconds) => Fx(0, t, HalloweenKit.Pop(t, peak, seconds));
        void FxWiggle(Transform t, float degrees, float seconds, float speed) => Fx(1, t, HalloweenKit.Wiggle(t, degrees, seconds, speed));

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
                slot.v = t.localScale;
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
            else t.localRotation = slot.q;
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

        // ---------- Dita (al massimo due) ----------

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            foreach (V64AFinger f in fingers)
            {
                if (f.began) { if (grips.TryGetValue(f.id, out Grip old)) End(f.id, old); grips[f.id] = Begin(f.world); }
                if (!grips.TryGetValue(f.id, out Grip g)) continue;
                g.last = f.world;
                Drag(g);
                if (finishing) { grips.Clear(); return; }
                if (f.ended) End(f.id, g);
                if (finishing) { grips.Clear(); return; }
            }
            CheckPinch();
            if (fingers.Count == 0 && grips.Count > 0)
            {
                // dita sparite senza "Ended": si chiudono i gesti
                gone.Clear();
                foreach (int id in grips.Keys) gone.Add(id);
                foreach (int id in gone) if (!finishing && grips.TryGetValue(id, out Grip g)) End(id, g);
            }
        }

        static bool Over(Collider2D c, Vector2 p) => c != null && c.gameObject.activeInHierarchy && c.OverlapPoint(p);

        /// <summary>Il secondo dito conta sulla scala anche un po' fuori dal suo bersaglio (pizzico comodo).</summary>
        bool NearLadder(Vector2 p)
        {
            if (ladderArea == null) return false;
            Bounds b = ladderArea.bounds;
            b.Expand(new Vector3(PinchNear * 2f, PinchNear * 2f, 10f));
            return b.Contains(new Vector3(p.x, p.y, b.center.z));
        }

        Grip Find(Kind k)
        {
            foreach (Grip g in grips.Values) if (g.kind == k) return g;
            return null;
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, kind = Kind.None };
            if (finishing || logic.InputLocked) return g;
            if (Find(Kind.Pinch) != null) return g;   // pizzico già in corso: il dito non fa niente
            Grip other = Find(Kind.Ladder);
            if (other != null && NearLadder(w))
            {
                // secondo dito sulla scala: pizzico (agisce solo sulla scala; il trascinamento in corso diventa pizzico)
                if (!logic.StartPinch()) return g;
                if (attempting) InterruptAttempt();
                other.kind = Kind.Pinch;
                g.kind = Kind.Pinch;
                pinchD0 = Mathf.Max(PinchMinD, Vector2.Distance(w, other.last));
                pinchSegs0 = logic.Segments;
                return g;
            }
            // l'omino sta davanti alla scala: ha la precedenza (la scala è alta, la si prende anche più su)
            if (Find(Kind.Man) == null && !attempting && Over(manArea, w))
            {
                g.kind = Kind.Man;
                g.offset = man.position - (Vector3)w;
                return g;
            }
            if (other == null && Over(ladderArea, w))
            {
                g.kind = Kind.Ladder;
                g.offset = new Vector3(logic.LadderX, logic.BottomY, 0f) - (Vector3)w;
            }
            return g;
        }

        void Drag(Grip g)
        {
            switch (g.kind)
            {
                case Kind.Ladder:
                    if (!g.moved)
                    {
                        if (Vector2.Distance(g.last, g.start) <= TapSlop) return;
                        g.moved = true;
                        if (!logic.GrabLadder()) { g.kind = Kind.None; return; }
                        if (attempting) InterruptAttempt();
                        Haptics.Tap();
                        V84ShortLadderSound.Play(this, "pick", 0.35f);
                    }
                    if (!logic.LadderHeld) return;
                    Vector2 p = g.last + (Vector2)g.offset;
                    logic.MoveLadder(p.x, p.y);
                    break;
                case Kind.Man:
                    if (!g.moved)
                    {
                        if (Vector2.Distance(g.last, g.start) <= TapSlop) return;
                        g.moved = true;
                        if (!logic.GrabMan()) { g.kind = Kind.None; return; }
                        Haptics.Tap();
                        V84ShortLadderSound.Play(this, "whoa", 0.35f);
                    }
                    if (!logic.ManHeld) return;
                    Vector2 q = g.last + (Vector2)g.offset;
                    logic.MoveMan(q.x, q.y);
                    break;
            }
        }

        void CheckPinch()
        {
            if (!logic.Pinching) return;
            Grip a = null, b = null;
            foreach (Grip g in grips.Values)
            {
                if (g.kind != Kind.Pinch) continue;
                if (a == null) a = g; else b = g;
            }
            if (a == null || b == null) return;
            int before = logic.Segments;
            int now = logic.Pinch(Vector2.Distance(a.last, b.last) / pinchD0);
            if (now == before) return;
            if (Time.time - lastRung > 0.05f)
            {
                lastRung = Time.time;
                V84ShortLadderSound.Play(this, now > before ? "rung" : "unrung", 0.3f);
                Haptics.Light();
            }
            if (before < ShortLadderLogic.NeedSegments && now >= ShortLadderLogic.NeedSegments)
            {
                // abbastanza lunga: scintilla in cima e "ding" (riscontro discreto)
                sparkleT = 0f;
                V84ShortLadderSound.Play(this, "ding", 0.45f);
            }
        }

        void End(int id, Grip g)
        {
            grips.Remove(id);
            switch (g.kind)
            {
                case Kind.Ladder:
                    if (!g.moved) TapLadder();
                    else Handle(logic.ReleaseLadder());
                    break;
                case Kind.Man:
                    if (!g.moved) TapMan();
                    else Handle(logic.ReleaseMan());
                    break;
                case Kind.Pinch:
                    Grip rest = null;
                    foreach (Grip o in grips.Values) if (o.kind == Kind.Pinch) rest = o;
                    if (rest != null)
                    {
                        // dal pizzico al trascinamento: il dito rimasto continua a tenere la scala
                        logic.EndPinch(true);
                        rest.kind = Kind.Ladder;
                        rest.moved = true;
                        rest.start = rest.last;
                        rest.offset = new Vector3(logic.LadderX, logic.BottomY, 0f) - (Vector3)rest.last;
                    }
                    else Handle(logic.EndPinch(false));
                    break;
            }
        }

        // ---------- Risultati dei gesti ----------

        void Handle(ShortLadderDrop d)
        {
            switch (d)
            {
                case ShortLadderDrop.Reached:
                    StartCoroutine(Finale());
                    break;
                case ShortLadderDrop.TooShort:
                    V84ShortLadderSound.Play(this, "clack", 0.35f);
                    if (logic.ClimbAttempt) StartAttempt();
                    else toast.Show(HeldShortLine, 1.4f);
                    break;
                case ShortLadderDrop.WrongSpot:
                    V84ShortLadderSound.Play(this, "clack", 0.35f);
                    toast.Show(WrongSpotLines[wrongSpotLines++ % WrongSpotLines.Length], 1.6f);
                    break;
                case ShortLadderDrop.Placed:
                    V84ShortLadderSound.Play(this, "clack", 0.3f);
                    break;
                case ShortLadderDrop.Stretched:
                    if (logic.Segments >= pinchSegs0) toast.Show(GrewLines[grewLines++ % GrewLines.Length], 1.4f);
                    else toast.Show(ShrankLines[shrankLines++ % ShrankLines.Length], 1.4f);
                    break;
                case ShortLadderDrop.Jumped:
                    V84ShortLadderSound.Play(this, "boing", 0.4f);
                    FxPop(manBody, 0.88f, 0.25f);
                    Say(JumpLines[jumpLines++ % JumpLines.Length], 1.5f);
                    break;
            }
        }

        void TapLadder()
        {
            ShortLadderDrop d = logic.TapLadder();
            if (d == ShortLadderDrop.Reached) { Handle(d); return; }
            if (d != ShortLadderDrop.Unchanged) return;
            FxWiggle(ladderBody, 3f, 0.3f, 30f);
            V84ShortLadderSound.Play(this, "clack", 0.25f);
            toast.Show(logic.LongEnough ? LongTapLine : ShortTapLines[shortTapLines++ % ShortTapLines.Length], 1.4f);
        }

        void TapMan()
        {
            if (!logic.TapMan()) return;
            FxPop(manBody, 1.08f, 0.25f);
            V84ShortLadderSound.Play(this, "hmm", 0.35f);
            Say(ManTapLines[manTapLines++ % ManTapLines.Length], 1.6f);
        }

        void Say(string line, float seconds)
        {
            if (manBubble != null)
            {
                Vector3 p = man.position + (Vector3)BubbleFromFeet;
                manBubble.transform.position = new Vector3(p.x, Mathf.Min(p.y, BubbleMaxY), manBubble.transform.position.z);
            }
            say.Show(line, seconds);
        }

        // ---------- La falsa pista: l'omino prova la scala corta ----------

        void StartAttempt()
        {
            if (attempting) return;
            if (!isActiveAndEnabled) { logic.ClimbAttemptDone(man.position.x); return; }   // niente animazione: mai bloccati
            attempting = true;
            attemptCo = StartCoroutine(Attempt());
        }

        /// <summary>L'omino va alla scala corta, sale fino in cima, non arriva alla finestra, lo dice e riscende.</summary>
        IEnumerator Attempt()
        {
            say.Hide();
            float x = logic.LadderX;
            yield return WalkTo(x, WalkSpeed);
            ManPose(true);
            float feetTop = ShortLadderLogic.FloorY + Mathf.Max(0.3f, logic.Length - 1.15f);
            yield return ClimbTo(x, feetTop, ClimbSpeed);
            V84ShortLadderSound.Play(this, "hmm", 0.4f);
            Say(TooShortLines[tooShortLines++ % TooShortLines.Length], 1.4f);
            FxWiggle(manBody, 5f, 0.6f, 10f);
            yield return new WaitForSeconds(1.0f);
            yield return ClimbTo(x, ShortLadderLogic.FloorY, ClimbSpeed * 1.3f);
            ManPose(false);
            manVis = new Vector2(man.position.x, ShortLadderLogic.FloorY);
            logic.ClimbAttemptDone(manVis.x);
            attempting = false;
            attemptCo = null;
        }

        IEnumerator WalkTo(float x, float speed)
        {
            ManPose(false);
            man.localRotation = Quaternion.identity;
            float t = 0f;
            while (Mathf.Abs(man.position.x - x) > 0.01f)
            {
                float dt = Time.deltaTime;
                t += dt;
                float nx = Mathf.MoveTowards(man.position.x, x, speed * dt);
                float ny = Mathf.MoveTowards(man.position.y, ShortLadderLogic.FloorY, 6f * dt);
                PlaceMan(new Vector2(nx, ny));
                if (manBody != null) manBody.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(t * 14f)) * 0.05f, 0f);
                yield return null;
            }
            PlaceMan(new Vector2(x, ShortLadderLogic.FloorY));
            if (manBody != null) manBody.localPosition = Vector3.zero;
        }

        IEnumerator ClimbTo(float x, float feetY, float speed)
        {
            float t = 0f, lastStep = 0f;
            while (Mathf.Abs(man.position.y - feetY) > 0.005f)
            {
                float dt = Time.deltaTime;
                t += dt;
                float ny = Mathf.MoveTowards(man.position.y, feetY, speed * dt);
                PlaceMan(new Vector2(x, ny));
                if (manBody != null) manBody.localRotation = Quaternion.Euler(0f, 0f, 4f * Mathf.Sin(t * 16f));
                if (t - lastStep > 0.16f) { lastStep = t; V84ShortLadderSound.Play(this, "step", 0.2f); }
                yield return null;
            }
            PlaceMan(new Vector2(x, feetY));
            if (manBody != null) manBody.localRotation = Quaternion.identity;
        }

        /// <summary>La scala viene ripresa (o pizzicata) mentre l'omino sale: salta giù protestando.</summary>
        void InterruptAttempt()
        {
            StopAttempt();
            V84ShortLadderSound.Play(this, "boing", 0.35f);
            Say(InterruptLine, 1.4f);
        }

        /// <summary>Ferma la prova di salita: l'omino torna in piedi e ricade a terra lì dov'è (Pose lo fa scendere).</summary>
        void StopAttempt()
        {
            if (attemptCo != null) StopCoroutine(attemptCo);
            attemptCo = null;
            attempting = false;
            ManPose(false);
            if (manBody != null) { manBody.localPosition = Vector3.zero; manBody.localRotation = Quaternion.identity; }
            manVis = man.position;
            logic.PlaceMan(man.position.x);
        }

        // ---------- La soluzione ----------

        /// <summary>
        /// La scala lunga è sotto la finestra: si posa dritta, l'omino ci corre, sale veloce, apre la finestra (il vetro
        /// sparisce), entra e saluta dal davanzale ("I'M IN!"); poi la festa comune e Win, una volta sola. Rifatta da capo
        /// se il livello si spegne a metà.
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            grips.Clear();
            V64AFingers.Reset();
            if (attemptCo != null) StopCoroutine(attemptCo);
            attemptCo = null;
            attempting = false;
            StopFx();
            HalloweenKit.Show(infoHolder, false);
            say.Hide();
            // stato di partenza della festa (anche se rifatta dopo uno spegnimento)
            ladderVis = new Vector2(logic.LadderX, ShortLadderLogic.FloorY);
            ladder.position = ladderZ + (Vector3)ladderVis;
            ladder.localRotation = Quaternion.identity;
            ShowSegments(logic.Segments, false);
            HalloweenKit.Show(man, true);
            HalloweenKit.Show(manShadow, true);
            man.localRotation = Quaternion.identity;
            if (manBody != null) { manBody.localPosition = Vector3.zero; manBody.localRotation = Quaternion.identity; }
            PlaceMan(new Vector2(man.position.x, ShortLadderLogic.FloorY));
            HalloweenKit.Show(waveMan, false);
            HalloweenKit.SetAlpha(windowGlass, glassAlpha);
            HalloweenKit.Show(windowGlass, true);
            sparkleT = 0f;
            HalloweenKit.Show(sparkle, true);
            V84ShortLadderSound.Play(this, "ding", 0.5f);
            Haptics.Medium();

            // 1) l'omino corre alla scala e sale veloce
            float x = logic.LadderX;
            yield return WalkTo(x, WinWalkSpeed);
            ManPose(true);
            StartCoroutine(SparkleFade());
            yield return ClimbTo(x, SillY - 0.75f, WinClimbSpeed);

            // 2) apre la finestra ed entra
            V84ShortLadderSound.Play(this, "pop", 0.5f);
            if (windowGlass != null) yield return HalloweenKit.Fade(windowGlass, 0f, 0.18f);
            HalloweenKit.Show(man, false);
            HalloweenKit.Show(manShadow, false);
            if (waveMan != null)
            {
                // si affaccia dal lato libero della finestra (la scala copre l'altro)
                float side = logic.LadderX <= ShortLadderLogic.WindowX ? 1f : -1f;
                waveMan.position = new Vector3(ShortLadderLogic.WindowX + side * 0.22f, SillY, waveMan.position.z);
                waveMan.localScale = new Vector3(Mathf.Abs(waveRest.x) * side, waveRest.y, waveRest.z);
                HalloweenKit.Show(waveMan, true);
                FxPop(waveMan, 1.15f, 0.25f);
                waveCo = StartCoroutine(Wave());
            }
            V84ShortLadderSound.Play(this, "yay", 0.5f);
            if (manBubble != null) manBubble.transform.position = new Vector3(Mathf.Clamp(ShortLadderLogic.WindowX - 1.3f, -2f, 1f), SillY - 0.35f, manBubble.transform.position.z);
            say.Show(InLine, 0.9f);
            yield return new WaitForSeconds(0.6f);
            say.Hide();

            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            if (logic.TryComplete()) Win(Compliment);
        }

        IEnumerator SparkleFade()
        {
            for (float t = 0f; t < 0.6f; t += Time.deltaTime) { TickSparkle(Time.deltaTime); yield return null; }
            sparkleT = -1f;
            HalloweenKit.Show(sparkle, false);
        }

        /// <summary>Il braccio saluta avanti e indietro finché dura la festa.</summary>
        IEnumerator Wave()
        {
            for (float t = 0f; waveArm != null; t += Time.deltaTime)
            {
                waveArm.localRotation = Quaternion.Euler(0f, 0f, 28f * Mathf.Sin(t * 10f));
                yield return null;
            }
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked || grips.Count > 0 || logic.LadderHeld || logic.ManHeld || logic.Pinching) return;
            if (Input.GetKeyDown(KeyCode.W))
            {
                KeyPinch(11 - logic.Segments, false);
                if (!logic.InputLocked) KeyMove(ShortLadderLogic.WindowX);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (logic.LongEnough) KeyPinch(ShortLadderLogic.MinSegments - logic.Segments, false);
                if (!logic.InputLocked) KeyMove(ShortLadderLogic.WindowX);
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                if (logic.InColumn) KeyMove(-1.2f, true);
                KeyPinch(12 - logic.Segments, false);
            }
            if (Input.GetKeyDown(KeyCode.E)) KeyPinch(2, false);
            if (Input.GetKeyDown(KeyCode.X)) KeyPinch(-2, false);
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (logic.GrabMan() && logic.MoveMan(logic.ManX + 0.3f, ShortLadderLogic.FloorY + 1.5f))
                {
                    manVis = new Vector2(logic.ManX, logic.ManY);
                    Handle(logic.ReleaseMan());
                }
            }
            if (Input.GetKeyDown(KeyCode.T)) TapMan();
            if (Input.GetKeyDown(KeyCode.K)) TapLadder();
            if (Input.GetKeyDown(KeyCode.P)) CancelAll();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }

        /// <summary>Pizzico simulato di <paramref name="delta"/> pioli (la scala resta dov'è).</summary>
        void KeyPinch(int delta, bool keep)
        {
            if (!logic.StartPinch()) return;
            if (attempting) InterruptAttempt();
            pinchSegs0 = logic.Segments;
            logic.Pinch(1f + delta / ShortLadderLogic.PinchGain);
            ShortLadderDrop d = logic.EndPinch(keep);
            if (!keep) Handle(d);
        }

        void KeyMove(float x, bool silent = false)
        {
            if (!logic.GrabLadder()) return;
            if (attempting) StopAttempt();
            logic.MoveLadder(x, ShortLadderLogic.FloorY);
            ShortLadderDrop d = logic.ReleaseLadder();
            if (!silent) Handle(d);
            else logic.ClimbAttemptDone(man.position.x);
        }
#endif
    }

    /// <summary>Suoni sintetizzati di The Short Ladder: presa, scala posata, piolo in più/in meno, "ding", passo sul piolo, "hmm", "uoh", rimbalzo, finestra, "evviva".</summary>
    public static class V84ShortLadderSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(272);

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
                case "pick":     // "plip"
                    c = Make(kind, 0.08f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(600f, 1100f, t / 0.08f)) * 0.4f * Mathf.Exp(-t * 30f));
                    break;
                case "clack":    // legno sul marciapiede
                    c = Make(kind, 0.12f, t => (N() * 0.35f + Mathf.Sin(t * 2f * Mathf.PI * 320f) * 0.35f) * Mathf.Exp(-t * 40f));
                    break;
                case "rung":     // un piolo in più: "tic" di legno che sale
                    c = Make(kind, 0.06f, t => Mathf.Sin(t * 2f * Mathf.PI * 1250f) * 0.35f * Mathf.Exp(-t * 60f) + N() * 0.1f * Mathf.Exp(-t * 120f));
                    break;
                case "unrung":   // un piolo in meno: "toc" più basso
                    c = Make(kind, 0.06f, t => Mathf.Sin(t * 2f * Mathf.PI * 700f) * 0.35f * Mathf.Exp(-t * 60f));
                    break;
                case "ding":     // abbastanza lunga
                    c = Make(kind, 0.5f, t => (Mathf.Sin(t * 2f * Mathf.PI * 1320f) * 0.3f + Mathf.Sin(t * 2f * Mathf.PI * 1980f) * 0.12f) * Mathf.Exp(-t * 6f) * Mathf.Min(1f, t * 300f));
                    break;
                case "step":     // passo sul piolo
                    c = Make(kind, 0.05f, t => (N() * 0.25f + Mathf.Sin(t * 2f * Mathf.PI * 220f) * 0.3f) * Mathf.Exp(-t * 80f));
                    break;
                case "hmm":      // "mmh" pensieroso
                    c = Make(kind, 0.45f, t =>
                    {
                        float f = 170f + 25f * Mathf.Sin(t / 0.45f * Mathf.PI);
                        float s = Mathf.Sin(t * 2f * Mathf.PI * f) + 0.4f * Mathf.Sin(t * 4f * Mathf.PI * f);
                        return s * 0.18f * Mathf.Sin(t / 0.45f * Mathf.PI);
                    });
                    break;
                case "whoa":     // "uoh!" preso in braccio
                    c = Make(kind, 0.25f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(300f, 520f, t / 0.25f)) * 0.25f * Mathf.Sin(t / 0.25f * Mathf.PI));
                    break;
                case "boing":    // ricade a terra
                    c = Make(kind, 0.35f, t =>
                    {
                        float f = 160f + 80f * Mathf.Sin(t * 2f * Mathf.PI * 9f) * Mathf.Exp(-t * 5f);
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.4f * Mathf.Exp(-t * 7f) * Mathf.Min(1f, t * 200f);
                    });
                    break;
                case "pop":      // la finestra si apre
                    c = Make(kind, 0.12f, t => N() * 0.5f * Mathf.Exp(-t * 60f) + Mathf.Sin(t * 2f * Mathf.PI * 800f) * 0.25f * Mathf.Exp(-t * 35f));
                    break;
                case "yay":      // "evviva" che sale
                    c = Make(kind, 0.6f, t =>
                    {
                        float f = Mathf.Lerp(420f, 900f, t / 0.6f) + 25f * Mathf.Sin(t * 2f * Mathf.PI * 7f);
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.25f * Mathf.Min(1f, t * 30f) * Mathf.Clamp01((0.6f - t) * 6f);
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
            AudioClip clip = AudioClip.Create("v84b_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
