using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 110 della roadmap "The Imaginary Bridge" (id imaginary_bridge) — "CROSS THE CANYON." Due piattaforme di
    /// roccia separate da un canyon, un'asse di legno corta sulla piattaforma di sinistra e un esploratore col cappello.
    /// Inganno: l'asse corta posata sul canyon cade nel vuoto (passa dietro le pareti) e ricompare al suo posto con un
    /// "pop" ("Too short!"); un'asse lunga posata storta (un capo nel vuoto) cade allo stesso modo; un'asse lunga posata
    /// sulla roccia resta lì ("Bridges go OVER things."); trascinare l'esploratore lo porta fino al bordo ma non oltre:
    /// si spaventa, "Nope. Not jumping that." e fa un passo indietro. Tutto reversibile, mai sconfitta.
    /// Soluzione: pizzico a due dita sull'asse per ALLUNGARLA (in x, dal centro; da 1.0 a 3.5, ne serve almeno 1.9:
    /// quando basta, scintilla e "ding"), poi posarla sul canyon con i due capi sulle due rocce (zone di aggancio). L'asse
    /// scatta orizzontale e centrata (stato stabile), l'esploratore attraversa da solo, a metà guarda giù ("Whoa..."), arriva
    /// dall'altra parte, salta di gioia accanto alla bandierina; LevelReward "ENGINEERING DEGREE UNLOCKED!" -> Win una volta sola.
    /// Dita: al massimo due (V64AFingers). Due dita sull'asse fanno il pizzico (agisce solo sull'asse); alzando un dito,
    /// l'altro continua a trascinarla (niente blocchi). Asse ed esploratore possono essere tenuti da due dita diverse.
    /// Limiti: l'asse resta nello schermo, non scende sotto il piano delle rocce, si solleva al massimo di 2.6.
    /// Pausa, app in background o livello spento lasciano andare tutto (la lunghezza raggiunta resta; un'asse tenuta sul
    /// vuoto torna al suo posto, senza vincere). La logica è in Gotcha.Levels.V64.ImaginaryBridgeLogic.
    /// Tasti di prova nell'Editor: W soluzione (allunga a 2.6 e posa sul canyon), S asse corta sul canyon, L asse lunga
    /// sulla roccia, O asse lunga storta sul canyon, E allunga di 0.4, X accorcia di 0.4, M esploratore trascinato al bordo,
    /// T tocco sull'esploratore, K tocco sull'asse, P pausa simulata (dita perse), R ricomincia.
    /// Shift + clic pianta un secondo dito (V64AFingers) per il pizzico.
    /// </summary>
    public class V86ImaginaryBridgeLevel : LevelController
    {
        [Header("Asse (radice in basso al centro; Mid si allunga in x, i capi seguono)")]
        [SerializeField] Transform plank;
        [SerializeField] Transform plankBody;
        [SerializeField] Transform plankMid;
        [SerializeField] Transform plankCapL;
        [SerializeField] Transform plankCapR;
        [SerializeField] SpriteRenderer[] plankArts;
        [SerializeField] BoxCollider2D plankArea;
        [SerializeField] SpriteRenderer sparkle;
        [SerializeField] Transform flag;
        [Header("Esploratore (radice ai piedi; Body per gli effetti)")]
        [SerializeField] Transform man;
        [SerializeField] Transform manBody;
        [SerializeField] Transform manStand;
        [SerializeField] Transform manScared;
        [SerializeField] Transform manCheer;
        [SerializeField] Collider2D manArea;
        [SerializeField] Transform manShadow;
        [Header("Fumetto, scritte e festa")]
        [SerializeField] SpeechBubble manBubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "ENGINEERING DEGREE UNLOCKED!";
        const float TapSlop = 0.15f, PinchNear = 0.5f, PinchMinD = 0.3f;
        const float CapW = 0.16f, MidOverlap = 0.04f;
        const float WinWalkSpeed = 4.5f, FallSeconds = 0.6f;
        const float CliffY = ImaginaryBridgeLogic.CliffY;
        static readonly Vector2 BubbleFromFeet = new Vector2(0.95f, 1.4f);    // fumetto: la codina punta alla testa
        const float BubbleMaxY = 0.6f;                                        // sotto la festa e la riga dell'obiettivo

        static readonly string[] TooShortLines = { "Too short!", "Way too short!", "The canyon disagrees.", "That's a stick, not a bridge." };
        static readonly string[] SlipLines = { "Long enough. Line it up!", "Both ends on the rocks!", "Close! One end is hanging." };
        static readonly string[] WrongSpotLines = { "Nice plank. Wrong place.", "Bridges go OVER things.", "The canyon is right there." };
        static readonly string[] EdgeLines = { "Nope. Not jumping that.", "Are you crazy?!", "I'm not a goat!", "That's a long way down." };
        static readonly string[] ManTapLines = { "I need to get across.", "See that flag? I want it.", "How do I cross this?", "That plank won't do." };
        static readonly string[] ShortTapLines = { "A very short plank.", "Not even half a bridge.", "Tiny plank. Big canyon." };
        static readonly string[] GrewLines = { "Longer! Keep going.", "Growing... not enough.", "A bit more plank." };
        static readonly string[] ShrankLines = { "Shorter. Bold move.", "Wrong direction." };
        const string LongTapLine = "Now THAT's a plank.";
        const string LongEnoughLine = "That's a LONG plank.";
        const string PeekLine = "Whoa...";
        const string MadeItLine = "I MADE IT!";

        enum Kind { None, Plank, Man, Pinch }

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

        ImaginaryBridgeLogic logic;
        Camera cam;
        HwToast toast;
        HwSay say;
        readonly Dictionary<int, Grip> grips = new Dictionary<int, Grip>();
        readonly List<int> gone = new List<int>();
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        Vector3 plankZ, manZ, flagRest = Vector3.one;
        Vector2 plankVis;
        float manVisX, manLift, plankVelX, tilt, pinchD0 = 1f, pinchLen0 = 1f, sparkleT = -1f, lastCreak = -10f, scaredT;
        float shownLength = -1f;
        int tooShortLines, slipLines, wrongSpotLines, edgeLines, manTapLines, shortTapLines, grewLines, shrankLines;
        bool captured, finishing, falling;
        Coroutine fallCo;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            fallCo = null;
            StopFx();
            if (!captured)
            {
                captured = true;
                logic = new ImaginaryBridgeLogic();
                plankZ = new Vector3(0f, 0f, plank.position.z);
                manZ = new Vector3(0f, 0f, man.position.z);
                if (flag != null) flagRest = flag.localScale;
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            V64AFingers.Reset();
            grips.Clear();
            finishing = falling = false;
            tooShortLines = slipLines = wrongSpotLines = edgeLines = manTapLines = shortTapLines = grewLines = shrankLines = 0;
            toast = new HwToast(infoText, infoHolder);
            say = new HwSay(manBubble);
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            say?.Hide();
            plankVis = new Vector2(logic.PlankX, logic.PlankY);
            plankVelX = tilt = 0f;
            plank.position = plankZ + (Vector3)plankVis;
            plank.localRotation = Quaternion.identity;
            if (plankBody != null) { plankBody.localPosition = Vector3.zero; plankBody.localRotation = Quaternion.identity; plankBody.localScale = Vector3.one; }
            SetPlankAlpha(1f);
            HalloweenKit.Show(plank, true);
            shownLength = -1f;
            ShowLength(logic.Length);
            manVisX = logic.ManX;
            manLift = scaredT = 0f;
            PlaceMan(manVisX, CliffY);
            man.localRotation = Quaternion.identity;
            HalloweenKit.Show(man, true);
            ManPose(0);
            if (manBody != null) { manBody.localPosition = Vector3.zero; manBody.localRotation = Quaternion.identity; }
            if (flag != null) { flag.localScale = flagRest; flag.localRotation = Quaternion.identity; }
            sparkleT = -1f;
            HalloweenKit.Show(sparkle, false);
        }

        /// <summary>0 in piedi, 1 spaventato, 2 felice.</summary>
        void ManPose(int pose)
        {
            HalloweenKit.Show(manStand, pose == 0);
            HalloweenKit.Show(manScared, pose == 1);
            HalloweenKit.Show(manCheer, pose == 2);
        }

        void PlaceMan(float x, float feetY)
        {
            man.position = manZ + new Vector3(x, feetY, 0f);
            if (manShadow != null)
            {
                float ground = Mathf.Abs(x) < ImaginaryBridgeLogic.RightEdgeX ? CliffY + ImaginaryBridgeLogic.Thickness : CliffY;   // sopra il canyon: l'ombra sull'asse
                float h = Mathf.Max(0f, feetY - ground);
                manShadow.position = new Vector3(x, ground + 0.02f, manShadow.position.z);
                manShadow.localScale = new Vector3(0.8f, 0.2f, 1f) * Mathf.Clamp(1f - h * 0.4f, 0.35f, 1f);
            }
        }

        void SetPlankAlpha(float a)
        {
            if (plankArts == null) return;
            foreach (SpriteRenderer sr in plankArts) HalloweenKit.SetAlpha(sr, a);
        }

        /// <summary>Allunga il tratto centrale (in x: le venature orizzontali non si deformano), sposta i capi e adatta il bersaglio.</summary>
        void ShowLength(float len)
        {
            if (Mathf.Abs(len - shownLength) < 1e-4f) return;
            shownLength = len;
            float half = len * 0.5f;
            if (plankMid != null) plankMid.localScale = new Vector3(Mathf.Max(0.05f, len - 2f * CapW + MidOverlap), 1f, 1f);
            if (plankCapL != null) plankCapL.localPosition = new Vector3(-half, 0f, plankCapL.localPosition.z);
            if (plankCapR != null) plankCapR.localPosition = new Vector3(half, 0f, plankCapR.localPosition.z);
            if (plankArea != null)
            {
                plankArea.size = new Vector2(len + 0.6f, 0.9f);
                plankArea.offset = new Vector2(0f, 0.1f);
            }
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: si molla tutto, si torna a uno stato valido
            CancelAll();
            StopFx();
            fallCo = null;
            falling = false;
            if (captured && !finishing && !IsFinished && logic != null) { logic.FallDone(); RestPose(); }
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == ImaginaryBridgeStage.Celebrating) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelAll(); }

        /// <summary>Le dita spariscono: asse ed esploratore lasciati (Pose li posa), una caduta in corso finisce subito.</summary>
        void CancelAll()
        {
            grips.Clear();
            V64AFingers.Reset();
            if (logic == null || logic.InputLocked) return;
            logic.Cancel();
            if (falling) StopFall();
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
        /// L'asse: tenuta segue il dito (già limitata dalla logica) e si inclina un poco col movimento; lasciata scende sul
        /// piano delle rocce. L'esploratore: tenuto si solleva un pelo e scalcia, lasciato torna giù; spaventato per un attimo.
        /// </summary>
        void Pose()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            ShowLength(logic.Length);
            if (!falling)
            {
                Vector2 target = new Vector2(logic.PlankX, logic.PlankY);
                float before = plankVis.x;
                bool held = logic.PlankHeld || logic.Pinching;
                plankVis = held ? Vector2.Lerp(plankVis, target, 1f - Mathf.Exp(-dt * 30f))
                                : new Vector2(Mathf.Lerp(plankVis.x, target.x, 1f - Mathf.Exp(-dt * 20f)), Mathf.Lerp(plankVis.y, target.y, 1f - Mathf.Exp(-dt * 16f)));
                if (!held && Mathf.Abs(plankVis.y - target.y) < 0.002f) plankVis.y = target.y;
                plankVis.y = Mathf.Max(plankVis.y, CliffY);   // mai sotto il piano delle rocce, neanche nell'animazione
                plankVelX = dt > 0f ? Mathf.Lerp(plankVelX, (plankVis.x - before) / dt, 1f - Mathf.Exp(-dt * 10f)) : 0f;
                float wantTilt = logic.PlankHeld ? Mathf.Clamp(-plankVelX * 2f, -6f, 6f) : 0f;
                tilt = Mathf.Lerp(tilt, wantTilt, 1f - Mathf.Exp(-dt * 12f));
                plank.position = plankZ + (Vector3)plankVis;
                plank.localRotation = Quaternion.Euler(0f, 0f, tilt);
            }

            if (scaredT > 0f) { scaredT -= dt; if (scaredT <= 0f) ManPose(0); }
            manVisX = Mathf.Lerp(manVisX, logic.ManX, 1f - Mathf.Exp(-dt * (logic.ManHeld ? 30f : 10f)));
            manLift = Mathf.Lerp(manLift, logic.ManHeld ? 0.15f : 0f, 1f - Mathf.Exp(-dt * 16f));
            PlaceMan(manVisX, CliffY + manLift);
            float wobble = logic.ManHeld ? 8f * Mathf.Sin(Time.time * 9f) : scaredT > 0f ? 6f * Mathf.Sin(Time.time * 22f) : 0f;
            man.localRotation = Quaternion.Slerp(man.localRotation, Quaternion.Euler(0f, 0f, wobble), 1f - Mathf.Exp(-dt * 18f));
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
            Vector3 end = plank.TransformPoint(new Vector3(shownLength * 0.5f, ImaginaryBridgeLogic.Thickness, 0f));
            sparkle.transform.position = new Vector3(end.x, end.y + 0.1f, sparkle.transform.position.z);
            sparkle.transform.localScale = Vector3.one * (1.1f * Mathf.Sin(k * Mathf.PI));
            sparkle.transform.localRotation = Quaternion.Euler(0f, 0f, k * 120f);
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73-0.85) ----------

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

        /// <summary>Il secondo dito conta sull'asse anche un po' fuori dal suo bersaglio (pizzico comodo).</summary>
        bool NearPlank(Vector2 p)
        {
            if (plankArea == null) return false;
            Bounds b = plankArea.bounds;
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
            Grip other = Find(Kind.Plank);
            if (other != null && NearPlank(w))
            {
                // secondo dito sull'asse: pizzico (agisce solo sull'asse; il trascinamento in corso diventa pizzico)
                if (!logic.StartPinch()) return g;
                other.kind = Kind.Pinch;
                g.kind = Kind.Pinch;
                pinchD0 = Mathf.Max(PinchMinD, Vector2.Distance(w, other.last));
                pinchLen0 = logic.Length;
                return g;
            }
            // l'esploratore sta davanti all'asse: ha la precedenza, tranne in basso all'altezza dell'asse posata
            bool plankLow = other == null && !falling && Over(plankArea, w) && w.y < plank.position.y + 0.4f;
            if (Find(Kind.Man) == null && !plankLow && Over(manArea, w))
            {
                g.kind = Kind.Man;
                g.offset = new Vector3(logic.ManX - w.x, 0f, 0f);
                return g;
            }
            if (other == null && !falling && Over(plankArea, w))
            {
                g.kind = Kind.Plank;
                g.offset = new Vector3(logic.PlankX, logic.PlankY, 0f) - (Vector3)w;
            }
            return g;
        }

        void Drag(Grip g)
        {
            switch (g.kind)
            {
                case Kind.Plank:
                    if (!g.moved)
                    {
                        if (Vector2.Distance(g.last, g.start) <= TapSlop) return;
                        g.moved = true;
                        if (!logic.GrabPlank()) { g.kind = Kind.None; return; }
                        Haptics.Tap();
                        V86ImaginaryBridgeSound.Play(this, "pick", 0.35f);
                    }
                    if (!logic.PlankHeld) return;
                    Vector2 p = g.last + (Vector2)g.offset;
                    logic.MovePlank(p.x, p.y);
                    break;
                case Kind.Man:
                    if (!g.moved)
                    {
                        if (Vector2.Distance(g.last, g.start) <= TapSlop) return;
                        g.moved = true;
                        if (!logic.GrabMan()) { g.kind = Kind.None; return; }
                        scaredT = 0f;
                        ManPose(0);
                        Haptics.Tap();
                        V86ImaginaryBridgeSound.Play(this, "whoa", 0.35f);
                    }
                    if (!logic.ManHeld) return;
                    logic.MoveMan(g.last.x + g.offset.x);
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
            float before = logic.Length;
            float now = logic.Pinch(Vector2.Distance(a.last, b.last) / pinchD0);
            if (Mathf.Abs(now - before) < 0.01f) return;
            if (Time.time - lastCreak > 0.09f)
            {
                lastCreak = Time.time;
                V86ImaginaryBridgeSound.Play(this, now > before ? "creak" : "uncreak", 0.25f);
                Haptics.Light();
            }
            if (before < ImaginaryBridgeLogic.NeedLength && now >= ImaginaryBridgeLogic.NeedLength)
            {
                // abbastanza lunga: scintilla sul capo e "ding" (riscontro discreto)
                sparkleT = 0f;
                V86ImaginaryBridgeSound.Play(this, "ding", 0.45f);
            }
        }

        void End(int id, Grip g)
        {
            grips.Remove(id);
            switch (g.kind)
            {
                case Kind.Plank:
                    if (!g.moved) TapPlank();
                    else Handle(logic.ReleasePlank());
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
                        // dal pizzico al trascinamento: il dito rimasto continua a tenere l'asse
                        Handle(logic.EndPinch(true));
                        rest.kind = Kind.Plank;
                        rest.moved = true;
                        rest.start = rest.last;
                        rest.offset = new Vector3(logic.PlankX, logic.PlankY, 0f) - (Vector3)rest.last;
                    }
                    else Handle(logic.EndPinch(false));
                    break;
            }
        }

        // ---------- Risultati dei gesti ----------

        void Handle(ImaginaryBridgeDrop d)
        {
            switch (d)
            {
                case ImaginaryBridgeDrop.Bridged:
                    StartCoroutine(Finale());
                    break;
                case ImaginaryBridgeDrop.TooShort:
                    StartFall();
                    Say(TooShortLines[tooShortLines++ % TooShortLines.Length], 1.5f);
                    break;
                case ImaginaryBridgeDrop.Slipped:
                    StartFall();
                    toast.Show(SlipLines[slipLines++ % SlipLines.Length], 1.6f);
                    break;
                case ImaginaryBridgeDrop.WrongSpot:
                    V86ImaginaryBridgeSound.Play(this, "clack", 0.35f);
                    toast.Show(WrongSpotLines[wrongSpotLines++ % WrongSpotLines.Length], 1.6f);
                    break;
                case ImaginaryBridgeDrop.Placed:
                    V86ImaginaryBridgeSound.Play(this, "clack", 0.3f);
                    break;
                case ImaginaryBridgeDrop.Stretched:
                    if (logic.LongEnough && pinchLen0 < ImaginaryBridgeLogic.NeedLength) toast.Show(LongEnoughLine, 1.4f);
                    else if (logic.Length >= pinchLen0) toast.Show(GrewLines[grewLines++ % GrewLines.Length], 1.4f);
                    else toast.Show(ShrankLines[shrankLines++ % ShrankLines.Length], 1.4f);
                    pinchLen0 = logic.Length;
                    break;
                case ImaginaryBridgeDrop.Refused:
                    // al bordo: si spaventa, non salta e fa un passo indietro (Pose lo riporta)
                    V86ImaginaryBridgeSound.Play(this, "yelp", 0.4f);
                    Haptics.Light();
                    ManPose(1);
                    scaredT = 0.9f;
                    Say(EdgeLines[edgeLines++ % EdgeLines.Length], 1.6f);
                    break;
                case ImaginaryBridgeDrop.Walked:
                    V86ImaginaryBridgeSound.Play(this, "step", 0.25f);
                    break;
            }
        }

        void TapPlank()
        {
            if (!logic.TapPlank()) return;
            FxWiggle(plankBody, 3f, 0.3f, 30f);
            V86ImaginaryBridgeSound.Play(this, "clack", 0.25f);
            toast.Show(logic.LongEnough ? LongTapLine : ShortTapLines[shortTapLines++ % ShortTapLines.Length], 1.4f);
        }

        void TapMan()
        {
            if (!logic.TapMan()) return;
            FxPop(manBody, 1.08f, 0.25f);
            V86ImaginaryBridgeSound.Play(this, "hmm", 0.35f);
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

        // ---------- La falsa pista: l'asse cade nel canyon ----------

        void StartFall()
        {
            if (fallCo != null) StopCoroutine(fallCo);
            if (!isActiveAndEnabled) { StopFall(); return; }   // niente animazione: mai bloccati
            falling = true;
            fallCo = StartCoroutine(Fall());
        }

        /// <summary>
        /// L'asse scende sul vuoto, si inclina e precipita nel canyon (passa dietro le pareti, sfuma), poi ricompare al suo
        /// posto con un "pop". Intanto la logica la tiene già a casa e non la lascia prendere (Falling).
        /// </summary>
        IEnumerator Fall()
        {
            V86ImaginaryBridgeSound.Play(this, "whistle", 0.35f);
            Vector2 from = new Vector2(logic.FallFromX, logic.FallFromY);
            float side = from.x >= 0f ? 1f : -1f;   // si inclina verso il lato più lontano dalla roccia
            plank.localRotation = Quaternion.identity;
            for (float t = 0f; t < FallSeconds; t += Time.deltaTime)
            {
                float k = t / FallSeconds;
                float y = from.y - 2f * t - 10f * t * t;   // precipita accelerando (circa 4.8 in 0.6 s)
                plank.position = plankZ + new Vector3(from.x + side * 0.25f * k, y, 0f);
                plank.localRotation = Quaternion.Euler(0f, 0f, -side * 70f * Mathf.SmoothStep(0f, 1f, k * 1.4f));
                SetPlankAlpha(Mathf.Clamp01(1.6f - k * 1.6f));
                yield return null;
            }
            V86ImaginaryBridgeSound.Play(this, "splash", 0.3f);
            FinishFall();
            V86ImaginaryBridgeSound.Play(this, "pop", 0.4f);
            FxPop(plankBody, 1.15f, 0.25f);
        }

        void FinishFall()
        {
            fallCo = null;
            falling = false;
            plankVis = new Vector2(logic.PlankX, logic.PlankY);
            plank.position = plankZ + (Vector3)plankVis;
            plank.localRotation = Quaternion.identity;
            tilt = plankVelX = 0f;
            SetPlankAlpha(1f);
            logic.FallDone();
        }

        /// <summary>Ferma la caduta (pausa): l'asse è subito al suo posto.</summary>
        void StopFall()
        {
            if (fallCo != null) StopCoroutine(fallCo);
            FinishFall();
        }

        // ---------- La soluzione ----------

        /// <summary>
        /// L'asse lunga è sul canyon: scatta orizzontale e centrata ("tunk"), l'esploratore va al bordo, sale sull'asse,
        /// a metà guarda giù ("Whoa..."), corre dall'altra parte, salta di gioia accanto alla bandierina ("I MADE IT!");
        /// poi la festa comune e Win, una volta sola. Rifatta da capo se il livello si spegne a metà.
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            grips.Clear();
            V64AFingers.Reset();
            if (fallCo != null) StopCoroutine(fallCo);
            fallCo = null;
            falling = false;
            StopFx();
            HalloweenKit.Show(infoHolder, false);
            say.Hide();
            // stato di partenza della festa (anche se rifatta dopo uno spegnimento)
            ShowLength(logic.Length);
            SetPlankAlpha(1f);
            if (plankBody != null) { plankBody.localPosition = Vector3.zero; plankBody.localRotation = Quaternion.identity; plankBody.localScale = Vector3.one; }
            scaredT = 0f;
            ManPose(0);
            man.localRotation = Quaternion.identity;
            if (manBody != null) { manBody.localPosition = Vector3.zero; manBody.localRotation = Quaternion.identity; }
            float manX = Mathf.Clamp(man.position.x, ImaginaryBridgeLogic.ManMinX, ImaginaryBridgeLogic.ManEdgeX);
            PlaceMan(manX, CliffY);
            Haptics.Medium();

            // 1) l'asse scatta nello stato stabile: orizzontale, centrata sul canyon
            Vector3 p0 = plank.position, p1 = plankZ + new Vector3(logic.PlankX, CliffY, 0f);
            Quaternion q0 = plank.localRotation;
            for (float t = 0f; t < 0.15f; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / 0.15f);
                plank.position = Vector3.Lerp(p0, p1, k);
                plank.localRotation = Quaternion.Slerp(q0, Quaternion.identity, k);
                yield return null;
            }
            plank.position = p1;
            plank.localRotation = Quaternion.identity;
            V86ImaginaryBridgeSound.Play(this, "thunk", 0.5f);
            V86ImaginaryBridgeSound.Play(this, "ding", 0.45f);
            FxPop(plankBody, 1.08f, 0.2f);
            sparkleT = 0f;
            StartCoroutine(SparkleFade());

            // 2) attraversa: a metà guarda giù, poi corre dall'altra parte
            float half = logic.Length * 0.5f;
            yield return Walk(manX, -0.15f, half);
            ManPose(1);
            V86ImaginaryBridgeSound.Play(this, "hmm", 0.35f);
            Say(PeekLine, 0.5f);
            for (float t = 0f; t < 0.45f; t += Time.deltaTime)
            {
                if (plankBody != null) plankBody.localPosition = new Vector3(0f, -0.04f * Mathf.Sin(Mathf.Clamp01(t / 0.45f) * Mathf.PI), 0f);   // l'asse si flette
                yield return null;
            }
            if (plankBody != null) plankBody.localPosition = Vector3.zero;
            ManPose(0);
            say.Hide();
            yield return Walk(-0.15f, ImaginaryBridgeLogic.ManFinalX, half);

            // 3) salto di gioia accanto alla bandierina
            ManPose(2);
            V86ImaginaryBridgeSound.Play(this, "yay", 0.5f);
            if (flag != null) FxWiggle(flag, 12f, 0.6f, 18f);
            Say(MadeItLine, 0.9f);
            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                PlaceMan(ImaginaryBridgeLogic.ManFinalX, CliffY + 0.35f * Mathf.Sin(Mathf.Clamp01(t / 0.35f) * Mathf.PI));
                yield return null;
            }
            PlaceMan(ImaginaryBridgeLogic.ManFinalX, CliffY);

            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            say.Hide();
            if (logic.TryComplete()) Win(Compliment);
        }

        /// <summary>Cammina da x0 a x1 molleggiando; sopra l'asse (|x| ≤ metà lunghezza) i piedi sono sul legno.</summary>
        IEnumerator Walk(float x0, float x1, float half)
        {
            float x = x0, t = 0f, lastStep = 0f;
            while (Mathf.Abs(x - x1) > 0.01f)
            {
                float dt = Time.deltaTime;
                t += dt;
                x = Mathf.MoveTowards(x, x1, WinWalkSpeed * dt);
                float feet = Mathf.Abs(x) <= half - 0.05f ? CliffY + ImaginaryBridgeLogic.Thickness : CliffY;
                PlaceMan(x, feet + Mathf.Abs(Mathf.Sin(t * 16f)) * 0.05f);
                if (t - lastStep > 0.14f) { lastStep = t; V86ImaginaryBridgeSound.Play(this, "step", 0.18f); }
                yield return null;
            }
            float end = Mathf.Abs(x1) <= half - 0.05f ? CliffY + ImaginaryBridgeLogic.Thickness : CliffY;
            PlaceMan(x1, end);
        }

        IEnumerator SparkleFade()
        {
            for (float t = 0f; t < 0.6f; t += Time.deltaTime) { TickSparkle(Time.deltaTime); yield return null; }
            sparkleT = -1f;
            HalloweenKit.Show(sparkle, false);
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked || grips.Count > 0 || logic.PlankHeld || logic.ManHeld || logic.Pinching || falling) return;
            if (Input.GetKeyDown(KeyCode.W))
            {
                KeyPinch(2.6f / logic.Length);
                if (!logic.InputLocked) KeyMove(ImaginaryBridgeLogic.BridgeX);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (logic.LongEnough) KeyPinch(ImaginaryBridgeLogic.MinLength / logic.Length);
                if (!logic.InputLocked && !falling) KeyMove(ImaginaryBridgeLogic.BridgeX);
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                if (!logic.LongEnough) KeyPinch(2.6f / logic.Length);
                if (!logic.InputLocked) KeyMove(1.4f);
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                if (!logic.LongEnough) KeyPinch(2.6f / logic.Length);
                if (!logic.InputLocked) KeyMove(0.7f);
            }
            if (Input.GetKeyDown(KeyCode.E)) KeyPinch((logic.Length + 0.4f) / logic.Length);
            if (Input.GetKeyDown(KeyCode.X)) KeyPinch((logic.Length - 0.4f) / logic.Length);
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (logic.GrabMan() && logic.MoveMan(ImaginaryBridgeLogic.RightEdgeX)) Handle(logic.ReleaseMan());
            }
            if (Input.GetKeyDown(KeyCode.T)) TapMan();
            if (Input.GetKeyDown(KeyCode.K)) TapPlank();
            if (Input.GetKeyDown(KeyCode.P)) CancelAll();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }

        /// <summary>Pizzico simulato col rapporto dato (l'asse resta dov'è).</summary>
        void KeyPinch(float ratio)
        {
            if (falling || !logic.StartPinch()) return;
            pinchLen0 = logic.Length;
            logic.Pinch(ratio);
            Handle(logic.EndPinch(false));
        }

        void KeyMove(float x)
        {
            if (falling || !logic.GrabPlank()) return;
            logic.MovePlank(x, CliffY + 0.5f);
            Handle(logic.ReleasePlank());
        }
#endif
    }

    /// <summary>Suoni sintetizzati di The Imaginary Bridge: presa, asse posata, legno che si allunga/accorcia, "ding", fischio della caduta, tonfo lontano, "pop", passo, "hmm", "uoh", strillo, "tunk" del ponte, "evviva".</summary>
    public static class V86ImaginaryBridgeSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(2110);

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
                case "clack":    // legno sulla roccia
                    c = Make(kind, 0.12f, t => (N() * 0.35f + Mathf.Sin(t * 2f * Mathf.PI * 300f) * 0.35f) * Mathf.Exp(-t * 40f));
                    break;
                case "creak":    // il legno si allunga: scricchiolio che sale
                    c = Make(kind, 0.09f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(380f, 620f, t / 0.09f)) * 0.3f * Mathf.Sin(t / 0.09f * Mathf.PI) + N() * 0.06f);
                    break;
                case "uncreak":  // il legno si accorcia: scricchiolio che scende
                    c = Make(kind, 0.09f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(560f, 330f, t / 0.09f)) * 0.3f * Mathf.Sin(t / 0.09f * Mathf.PI) + N() * 0.06f);
                    break;
                case "ding":     // abbastanza lunga / ponte
                    c = Make(kind, 0.5f, t => (Mathf.Sin(t * 2f * Mathf.PI * 1320f) * 0.3f + Mathf.Sin(t * 2f * Mathf.PI * 1980f) * 0.12f) * Mathf.Exp(-t * 6f) * Mathf.Min(1f, t * 300f));
                    break;
                case "whistle":  // fischio della caduta che scende
                    c = Make(kind, 0.55f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(1400f, 500f, t / 0.55f)) * 0.22f * Mathf.Min(1f, t * 40f) * Mathf.Clamp01((0.55f - t) * 8f));
                    break;
                case "splash":   // tonfo lontano in fondo al canyon
                    c = Make(kind, 0.25f, t => (N() * 0.3f * Mathf.Exp(-t * 14f) + Mathf.Sin(t * 2f * Mathf.PI * 140f) * 0.2f * Mathf.Exp(-t * 18f)));
                    break;
                case "pop":      // l'asse ricompare
                    c = Make(kind, 0.12f, t => N() * 0.4f * Mathf.Exp(-t * 60f) + Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(500f, 1000f, t / 0.12f)) * 0.3f * Mathf.Exp(-t * 30f));
                    break;
                case "thunk":    // l'asse si posa sulle due rocce
                    c = Make(kind, 0.2f, t => (N() * 0.3f + Mathf.Sin(t * 2f * Mathf.PI * 180f) * 0.5f) * Mathf.Exp(-t * 22f));
                    break;
                case "step":     // passo
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
                case "yelp":     // strillo al bordo
                    c = Make(kind, 0.3f, t => Mathf.Sin(t * 2f * Mathf.PI * (700f + 120f * Mathf.Sin(t * 2f * Mathf.PI * 18f))) * 0.22f * Mathf.Sin(t / 0.3f * Mathf.PI));
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
            AudioClip clip = AudioClip.Create("v86b_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
