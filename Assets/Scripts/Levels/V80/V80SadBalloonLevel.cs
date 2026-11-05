using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 27 della roadmap "The Sad Balloon" (id sad_balloon) — "MAKE THE BALLOON FLY." Nel parco un palloncino
    /// rosso con la faccia triste è legato con uno spago a un sassolino sul prato (il nodo intorno al sasso si vede).
    /// Inganno: trascinare il palloncino in su tende lo spago (più sottile, frenato: non vola mai) e al rilascio ricade
    /// con un rimbalzo ("It wants to... it can't."); pizzicarlo con due dita lo gonfia un po' e poi si sgonfia
    /// ("Bigger, but still stuck."); toccarlo lo fa sospirare. Niente microfono (scelta voluta: bastano i gesti). Tutto
    /// reversibile, mai sconfitta.
    /// Soluzione: trascinare il SASSO lontano dalla sua casa: appena supera 1 unità DURANTE il trascinamento il nodo si
    /// scioglie subito (il nodo sparisce, lo spago si stacca e penzola dal palloncino col ricciolo in fondo, il sasso
    /// resta a terra da solo). Lasciato prima, il sasso torna al suo posto e il nodo resta legato. Sciolto: il palloncino
    /// sorride, "I'M FREE!", sale lasciando una scia di cuoricini; LevelReward "NOTHING HOLDS YOU BACK!" -> Win una volta sola.
    /// Dita: al massimo due (V64AFingers). Palloncino e sasso possono essere tenuti da due dita diverse; due dita sul
    /// palloncino fanno il pizzico. Pausa, app in background o livello spento lasciano andare tutto (il nodo resta).
    /// La logica è in Gotcha.Levels.V64.SadBalloonLogic.
    /// Tasti di prova nell'Editor: U sasso trascinato oltre la soglia (soluzione), J sasso trascinato poco e lasciato,
    /// B palloncino tirato su e lasciato, G pizzico che lo gonfia e lasciato, T tocco sul palloncino, K tocco sul sasso,
    /// P pausa simulata (dita perse), R ricomincia. Shift + clic pianta un secondo dito (V64AFingers) per il pizzico.
    /// </summary>
    public class V80SadBalloonLevel : LevelController
    {
        [Header("Palloncino (radice posata dal livello; Body per gli effetti; collider generoso)")]
        [SerializeField] Transform balloon;
        [SerializeField] Collider2D balloonArea;
        [SerializeField] Transform balloonBody;
        [SerializeField] Transform faceSad;
        [SerializeField] Transform faceHappy;
        [SerializeField] float tieY = -1.08f;          // punto dello spago sotto il palloncino (locale)
        [Header("Spago e sasso")]
        [SerializeField] LineRenderer stringLine;
        [SerializeField] Transform stone;
        [SerializeField] Collider2D stoneArea;         // generoso: sasso + nodo
        [SerializeField] Transform knot;
        [SerializeField] float knotY = 0.27f;          // punto dello spago sul sasso (locale)
        [SerializeField] Transform looseEnd;           // ricciolo in fondo allo spago sciolto
        [Header("Festa")]
        [SerializeField] Transform[] hearts;
        [SerializeField] float flyY = 7f;
        [Header("Fumetto, scritte e festa")]
        [SerializeField] SpeechBubble balloonBubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "NOTHING HOLDS YOU BACK!";
        const float TapSlop = 0.15f, PinchNear = 1.5f, PinchMinD = 0.25f, SquashFollow = 12f;
        const float Spring = 70f, Damping = 5.5f, StoneFollow = 12f, StonePull = 0.3f;
        const int StringPoints = 20;
        const float HeartSize = 0.8f;   // "heart" è largo 0.48: cuoricini da circa 0.38

        static readonly string[] FellLines = { "It wants to... it can't.", "Boing. Back down.", "The string says no.", "Up, up... and down again." };
        static readonly string[] SighLines = { "Sigh...", "I just want to fly.", "Everyone else is flying.", "Balloons should float..." };
        static readonly string[] BiggerLines = { "Bigger, but still stuck.", "Puffed up. Still stuck.", "Size isn't the problem." };
        static readonly string[] SlidLines = { "It slides back.", "Plonk. Back in place.", "The stone rolls home." };
        static readonly string[] StoneTapLines = { "Just a little stone.", "A tiny stone. A big knot.", "Clack." };
        const string FreeLine = "I'M FREE!";

        enum Kind { None, Balloon, Stone, Pinch }

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

        SadBalloonLogic logic;
        Camera cam;
        HwToast toast;
        HwSay say;
        readonly Dictionary<int, Grip> grips = new Dictionary<int, Grip>();
        readonly List<int> gone = new List<int>();
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        readonly Vector3[] pts = new Vector3[StringPoints];
        Vector3 balloonRest, balloonScale, stoneHome;
        Vector2 offset, vel;
        float vis = 1f, restLength = 3f, stringWidth = 0.035f, freeK, pinchD0 = 1f, lastStretch = -10f, lastSqueak = -10f, squeakAt = 1f;
        int fellLines, sighLines, biggerLines, slidLines, stoneTapLines;
        bool captured, finishing;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            if (!captured)
            {
                captured = true;
                logic = new SadBalloonLogic();
                balloonRest = balloon.position;
                balloonScale = balloon.localScale;
                stoneHome = stone.position;
                if (stringLine != null)
                {
                    stringWidth = stringLine.widthMultiplier;
                    stringLine.positionCount = StringPoints;
                    stringLine.useWorldSpace = true;
                }
                restLength = Vector3.Distance(TieWorld(), KnotWorld());
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            V64AFingers.Reset();
            grips.Clear();
            finishing = false;
            fellLines = sighLines = biggerLines = slidLines = stoneTapLines = 0;
            toast = new HwToast(infoText, infoHolder);
            say = new HwSay(balloonBubble);
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            say?.Hide();
            offset = vel = Vector2.zero;
            vis = 1f;
            freeK = 0f;
            balloon.position = balloonRest;
            balloon.localRotation = Quaternion.identity;
            balloon.localScale = balloonScale;
            stone.position = stoneHome;
            stone.localRotation = Quaternion.identity;
            HalloweenKit.Show(knot, true);
            HalloweenKit.Show(looseEnd, false);
            Faces(false);
            HideHearts();
            DrawString();
        }

        void Faces(bool happy)
        {
            HalloweenKit.Show(faceSad, !happy);
            HalloweenKit.Show(faceHappy, happy);
        }

        void HideHearts()
        {
            if (hearts == null) return;
            foreach (Transform h in hearts) HalloweenKit.Show(h, false);
        }

        Vector3 TieWorld() => balloon.TransformPoint(new Vector3(0f, tieY, 0f));
        Vector3 KnotWorld() => stone.TransformPoint(new Vector3(0f, knotY, 0f));

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: si molla tutto, si torna a uno stato valido (il nodo resta com'è)
            CancelAll();
            StopFx();
            if (captured && !finishing && !IsFinished && logic != null) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == SadBalloonStage.Celebrating) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelAll(); }

        /// <summary>Le dita spariscono: palloncino e sasso tornano a posto (Pose li fa scivolare), niente battute.</summary>
        void CancelAll()
        {
            grips.Clear();
            V64AFingers.Reset();
            if (logic != null && !logic.InputLocked) logic.Cancel();
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
        /// Il palloncino: tenuto segue la tirata (già limitata dallo spago), lasciato torna giù con una molla che rimbalza;
        /// dondola piano; il pizzico lo gonfia/sgonfia morbido. Il sasso non tenuto torna a casa. Poi lo spago.
        /// </summary>
        void Pose()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            Vector2 target = Vector2.zero;
            if (logic.BalloonHeld) target = new Vector2(logic.BalloonX, logic.BalloonY);
            if (logic.StoneHeld) target.x += StonePull * (stone.position.x - stoneHome.x);   // lo spago tira il palloncino di lato
            target.x = Mathf.Clamp(target.x, -SadBalloonLogic.MaxSway, SadBalloonLogic.MaxSway);
            if (logic.BalloonHeld)
            {
                Vector2 before = offset;
                offset = Vector2.Lerp(offset, target, 1f - Mathf.Exp(-dt * 25f));
                vel = dt > 0f ? (offset - before) / dt : Vector2.zero;
            }
            else
            {
                vel += (-(offset - target) * Spring - vel * Damping) * dt;
                offset += vel * dt;
            }
            // mai oltre lo spago (anche col rimbalzo)
            offset.y = Mathf.Min(offset.y, SadBalloonLogic.MaxStretch);
            vis = Mathf.Lerp(vis, logic.PinchScale, 1f - Mathf.Exp(-dt * SquashFollow));
            float bob = 0.05f * Mathf.Sin(Time.time * 1.6f);
            balloon.position = balloonRest + new Vector3(offset.x, offset.y + bob, 0f);
            balloon.localRotation = Quaternion.Euler(0f, 0f, -offset.x * 14f + 3f * Mathf.Sin(Time.time * 1.1f));
            balloon.localScale = balloonScale * vis;
            if (!logic.StoneHeld) stone.position = Vector3.Lerp(stone.position, stoneHome, 1f - Mathf.Exp(-dt * StoneFollow));
            DrawString();
        }

        /// <summary>
        /// Lo spago. Legato: dal palloncino al nodo sul sasso; se il palloncino è vicino si incurva, se è tirato si tende e
        /// si assottiglia. Sciolto (<see cref="freeK"/> 0 -> 1): l'estremo lascia il sasso e penzola sotto il palloncino
        /// ondeggiando, col ricciolo in fondo.
        /// </summary>
        void DrawString()
        {
            if (stringLine == null) return;
            Vector3 a = TieWorld();
            Vector3 b = KnotWorld();
            float t = Time.time;
            float d = Vector3.Distance(a, b);
            float slack = Mathf.Max(0f, restLength - d);
            Vector3 dir = d > 1e-4f ? (b - a) / d : Vector3.down;
            Vector3 side = new Vector3(-dir.y, dir.x, 0f);
            float hang = restLength * 0.85f;
            for (int i = 0; i < StringPoints; i++)
            {
                float u = i / (float)(StringPoints - 1);
                // legato: retta + pancia sul lato (spago allentato) + un filo di vento
                Vector3 tied = Vector3.Lerp(a, b, u) + side * (Mathf.Sin(u * Mathf.PI) * (slack * 0.45f + 0.03f * Mathf.Sin(t * 2f + u * 5f)));
                // sciolto: pende sotto il palloncino, l'onda cresce verso l'estremo libero
                Vector3 free = a + new Vector3(Mathf.Sin(t * 4f - u * 5f) * 0.28f * u, -hang * u, 0f);
                Vector3 p = Vector3.Lerp(tied, free, Mathf.SmoothStep(0f, 1f, freeK));
                p.z = a.z;
                pts[i] = p;
            }
            stringLine.SetPositions(pts);
            float stretch = d > restLength ? Mathf.Clamp(restLength / d, 0.55f, 1f) : 1f;
            stringLine.widthMultiplier = stringWidth * Mathf.Lerp(stretch, 1f, freeK);
            if (looseEnd != null && looseEnd.gameObject.activeSelf)
            {
                Vector3 e = pts[StringPoints - 1], e0 = pts[StringPoints - 2];
                looseEnd.position = new Vector3(e.x, e.y, looseEnd.position.z);
                Vector3 v = e - e0;
                looseEnd.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg + 90f);
            }
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73-0.79) ----------

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
            }
            CheckPinch();
            if (fingers.Count == 0 && grips.Count > 0)
            {
                // dita sparite senza "Ended": si chiudono i gesti
                gone.Clear();
                foreach (int id in grips.Keys) gone.Add(id);
                foreach (int id in gone) if (grips.TryGetValue(id, out Grip g)) End(id, g);
            }
        }

        static bool Over(Collider2D c, Vector2 p) => c != null && c.gameObject.activeInHierarchy && c.OverlapPoint(p);

        Grip Find(Kind k)
        {
            foreach (Grip g in grips.Values) if (g.kind == k) return g;
            return null;
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, kind = Kind.None };
            if (finishing || logic.InputLocked) return g;
            // il sasso (bersaglio generoso col nodo) ha la precedenza
            if (Find(Kind.Stone) == null && Over(stoneArea, w))
            {
                g.kind = Kind.Stone;
                g.offset = stone.position - (Vector3)w;
                return g;
            }
            if (Find(Kind.Pinch) != null) return g;   // pizzico già in corso: il dito non fa niente
            Grip other = Find(Kind.Balloon);
            float toBalloon = Vector2.Distance(w, balloon.position);
            if (other != null && (Over(balloonArea, w) || toBalloon < PinchNear))
            {
                // secondo dito sul palloncino: pizzico (la tirata in corso si lascia, senza contarla)
                if (!logic.StartPinch()) return g;
                other.kind = Kind.Pinch;
                g.kind = Kind.Pinch;
                pinchD0 = Mathf.Max(PinchMinD, Vector2.Distance(w, other.last));
                squeakAt = 1.08f;
                return g;
            }
            if (other == null && Over(balloonArea, w)) g.kind = Kind.Balloon;
            return g;
        }

        void Drag(Grip g)
        {
            switch (g.kind)
            {
                case Kind.Balloon:
                    if (!g.moved)
                    {
                        if (Vector2.Distance(g.last, g.start) <= TapSlop) return;
                        g.moved = true;
                        if (!logic.GrabBalloon()) { g.kind = Kind.None; return; }
                        Haptics.Tap();
                    }
                    if (!logic.BalloonHeld) return;
                    logic.PullBalloon(g.last.x - g.start.x, g.last.y - g.start.y);
                    if (logic.BalloonY > 0.45f && Time.time - lastStretch > 0.6f)
                    {
                        lastStretch = Time.time;
                        V80SadBalloonSound.Play(this, "stretch", 0.35f);
                        Haptics.Light();
                    }
                    break;
                case Kind.Stone:
                    if (!g.moved)
                    {
                        if (Vector2.Distance(g.last, g.start) <= TapSlop) return;
                        g.moved = true;
                        if (!logic.GrabStone()) { g.kind = Kind.None; return; }
                        Haptics.Tap();
                        V80SadBalloonSound.Play(this, "pick", 0.35f);
                    }
                    if (!logic.StoneHeld) return;
                    Vector2 p = HalloweenKit.ClampSafe(g.last + (Vector2)g.offset);
                    stone.position = new Vector3(p.x, p.y, stone.position.z);
                    if (logic.MoveStone(p.x - stoneHome.x, p.y - stoneHome.y) == SadBalloonStone.Untied) StartCoroutine(Finale());
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
            float s = logic.Pinch(Vector2.Distance(a.last, b.last) / pinchD0);
            if (s >= squeakAt && Time.time - lastSqueak > 0.15f)
            {
                lastSqueak = Time.time;
                squeakAt = s + 0.07f;
                V80SadBalloonSound.Play(this, "squeak", 0.35f);
                Haptics.Light();
            }
        }

        void End(int id, Grip g)
        {
            grips.Remove(id);
            switch (g.kind)
            {
                case Kind.Balloon:
                    if (!g.moved) TapBalloon();
                    else LetBalloonGo();
                    break;
                case Kind.Stone:
                    if (!g.moved) TapStone();
                    else LetStoneGo();
                    break;
                case Kind.Pinch:
                    // basta un dito che si alza: il pizzico finisce, l'altro dito non fa più niente
                    foreach (Grip o in grips.Values) if (o.kind == Kind.Pinch) o.kind = Kind.None;
                    EndPinch();
                    break;
            }
        }

        // ---------- Le false piste ----------

        void TapBalloon()
        {
            if (!logic.TapBalloon()) return;
            FxPop(balloonBody, 0.93f, 0.35f);   // si affloscia un attimo
            V80SadBalloonSound.Play(this, "sigh", 0.45f);
            say.Show(SighLines[sighLines++ % SighLines.Length], 1.5f);
        }

        void LetBalloonGo()
        {
            if (logic.ReleaseBalloon() != SadBalloonDrop.FellBack) return;
            V80SadBalloonSound.Play(this, "boing", 0.45f);
            Haptics.Light();
            toast.Show(FellLines[fellLines++ % FellLines.Length], 1.6f);
        }

        void EndPinch()
        {
            if (logic.EndPinch() != SadBalloonDrop.BiggerStuck) return;
            V80SadBalloonSound.Play(this, "deflate", 0.4f);
            say.Show(BiggerLines[biggerLines++ % BiggerLines.Length], 1.6f);
        }

        void TapStone()
        {
            if (!logic.TapStone()) return;
            FxWiggle(stone, 8f, 0.35f, 28f);
            V80SadBalloonSound.Play(this, "clack", 0.35f);
            toast.Show(StoneTapLines[stoneTapLines++ % StoneTapLines.Length], 1.5f);
        }

        void LetStoneGo()
        {
            if (logic.ReleaseStone() != SadBalloonDrop.SlidBack) return;
            V80SadBalloonSound.Play(this, "clack", 0.3f);
            toast.Show(SlidLines[slidLines++ % SlidLines.Length], 1.5f);
        }

        // ---------- La soluzione ----------

        /// <summary>
        /// Il nodo si scioglie: sparisce, l'estremo dello spago lascia il sasso e penzola (ricciolo in fondo), il sasso cade
        /// a terra da solo; il palloncino sorride ("I'M FREE!") e sale lasciando cuoricini; durante la festa comune continua
        /// a salire fino a uscire dallo schermo. Poi Win, una volta sola. Rifatta da capo se il livello si spegne a metà.
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            grips.Clear();
            V64AFingers.Reset();
            StopFx();
            HalloweenKit.Show(infoHolder, false);
            say.Hide();
            vis = 1f;
            balloon.localScale = balloonScale;
            HideHearts();

            // 1) il nodo si scioglie: lo spago si stacca e penzola, il sasso cade giù da solo
            HalloweenKit.Show(knot, false);
            HalloweenKit.Show(looseEnd, true);
            V80SadBalloonSound.Play(this, "pop", 0.55f);
            Haptics.Medium();
            Vector3 stoneFrom = stone.position;
            Vector3 stoneTo = new Vector3(stoneFrom.x, Mathf.Min(stoneFrom.y, stoneHome.y), stoneFrom.z);
            Vector3 from = balloon.position;
            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.35f);
                freeK = k;
                stone.position = Vector3.Lerp(stoneFrom, stoneTo, k * k);
                // il palloncino torna dritto sopra il suo posto
                balloon.position = Vector3.Lerp(from, balloonRest, k);
                balloon.localRotation = Quaternion.Slerp(balloon.localRotation, Quaternion.identity, k);
                DrawString();
                yield return null;
            }
            freeK = 1f;
            stone.position = stoneTo;
            stone.localRotation = Quaternion.identity;

            // 2) sorride
            Faces(true);
            FxPop(balloonBody, 1.15f, 0.3f);
            say.Show(FreeLine, 0.9f);
            V80SadBalloonSound.Play(this, "whee", 0.5f);

            // 3) sale con la scia di cuori (e continua a salire durante la festa)
            StartCoroutine(Rise());
            yield return new WaitForSeconds(0.9f);
            say.Hide();

            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            if (logic.TryComplete()) Win(Compliment);
        }

        /// <summary>Il palloncino sale ondeggiando fino a uscire dallo schermo; ogni tanto lascia un cuoricino che sale e svanisce.</summary>
        IEnumerator Rise()
        {
            Vector3 start = balloon.position;
            float speed = 0f, nextHeart = 0f;
            int heartIdx = 0;
            var heartT = new float[hearts != null ? hearts.Length : 0];
            var heartX = new float[heartT.Length];
            for (int i = 0; i < heartT.Length; i++) heartT[i] = -1f;
            for (float t = 0f; t < 6f; t += Time.deltaTime)
            {
                float dt = Time.deltaTime;
                speed = Mathf.Min(2.6f, speed + dt * 3.2f);
                float y = balloon.position.y < flyY ? balloon.position.y + speed * dt : flyY;
                balloon.position = new Vector3(start.x + 0.25f * Mathf.Sin(t * 2.4f), y, start.z);
                balloon.localRotation = Quaternion.Euler(0f, 0f, 6f * Mathf.Sin(t * 2.4f + 0.6f));
                DrawString();
                // i cuoricini
                if (heartT.Length > 0 && t >= nextHeart && y < flyY - 1f)
                {
                    nextHeart = t + 0.14f;
                    int i = heartIdx++ % heartT.Length;
                    heartT[i] = 0f;
                    heartX[i] = (i % 2 == 0 ? -1f : 1f) * (0.2f + 0.1f * (i % 3));
                    hearts[i].position = new Vector3(balloon.position.x + heartX[i], y - 0.6f, hearts[i].position.z);
                    HalloweenKit.Show(hearts[i], true);
                }
                for (int i = 0; i < heartT.Length; i++)
                {
                    if (heartT[i] < 0f || hearts[i] == null) continue;
                    heartT[i] += dt;
                    float k = heartT[i] / 1.1f;
                    if (k >= 1f) { heartT[i] = -1f; HalloweenKit.Show(hearts[i], false); continue; }
                    Transform h = hearts[i];
                    h.position += new Vector3(Mathf.Sin(heartT[i] * 6f + i) * 0.4f * dt, 0.5f * dt, 0f);
                    h.localScale = Vector3.one * (HeartSize * Mathf.Sin(Mathf.Min(1f, k * 3f) * Mathf.PI * 0.5f) * (1f - k * k));
                }
                yield return null;
            }
            HideHearts();
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked || grips.Count > 0 || logic.BalloonHeld || logic.StoneHeld || logic.Pinching) return;
            if (Input.GetKeyDown(KeyCode.U)) KeyStone(1.2f);
            if (Input.GetKeyDown(KeyCode.J)) KeyStone(0.5f);
            if (Input.GetKeyDown(KeyCode.B))
            {
                if (logic.GrabBalloon() && logic.PullBalloon(0f, 2f))
                {
                    offset = new Vector2(logic.BalloonX, logic.BalloonY);
                    vel = Vector2.zero;
                    LetBalloonGo();
                }
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                if (logic.StartPinch()) { logic.Pinch(1.6f); vis = logic.PinchScale; EndPinch(); }
            }
            if (Input.GetKeyDown(KeyCode.T)) TapBalloon();
            if (Input.GetKeyDown(KeyCode.K)) TapStone();
            if (Input.GetKeyDown(KeyCode.P)) CancelAll();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }

        void KeyStone(float dx)
        {
            if (!logic.GrabStone()) return;
            Vector3 p = stoneHome + new Vector3(dx, 0f, 0f);
            stone.position = p;
            if (logic.MoveStone(dx, 0f) == SadBalloonStone.Untied) StartCoroutine(Finale());
            else LetStoneGo();
        }
#endif
    }

    /// <summary>Suoni sintetizzati di The Sad Balloon: sospiro, spago teso, rimbalzo, squittio di gomma, sgonfiata, nodo che si scioglie, "uiii", presa, sasso.</summary>
    public static class V80SadBalloonSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(287);

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
                case "sigh":     // sospiro: soffio filtrato che scende
                    {
                        float lp = 0f;
                        c = Make(kind, 0.7f, t =>
                        {
                            lp += (N() - lp) * Mathf.Lerp(0.35f, 0.08f, t / 0.7f);
                            float tone = Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(260f, 170f, t / 0.7f)) * 0.08f;
                            return (lp * 0.5f + tone) * Mathf.Sin(t / 0.7f * Mathf.PI);
                        });
                        break;
                    }
                case "stretch":  // spago teso: cigolio che sale
                    c = Make(kind, 0.25f, t =>
                    {
                        float f = Mathf.Lerp(380f, 620f, t / 0.25f) * (1f + 0.03f * Mathf.Sin(t * 2f * Mathf.PI * 40f));
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.25f * Mathf.Sin(t / 0.25f * Mathf.PI);
                    });
                    break;
                case "boing":    // ricade: "boing" con la molla
                    c = Make(kind, 0.45f, t =>
                    {
                        float f = 180f + 90f * Mathf.Sin(t * 2f * Mathf.PI * 9f) * Mathf.Exp(-t * 5f);
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.45f * Mathf.Exp(-t * 6f) * Mathf.Min(1f, t * 200f);
                    });
                    break;
                case "squeak":   // gomma che si stira
                    c = Make(kind, 0.12f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(700f, 1050f, t / 0.12f)) * 0.3f * Mathf.Sin(t / 0.12f * Mathf.PI));
                    break;
                case "deflate":  // si sgonfia: "pfff" che scende
                    {
                        float lp = 0f;
                        c = Make(kind, 0.4f, t =>
                        {
                            lp += (N() - lp) * Mathf.Lerp(0.5f, 0.1f, t / 0.4f);
                            return lp * 0.4f * Mathf.Sin(t / 0.4f * Mathf.PI);
                        });
                        break;
                    }
                case "pop":      // il nodo si scioglie: schiocco + "tic"
                    c = Make(kind, 0.12f, t => N() * 0.6f * Mathf.Exp(-t * 70f) + Mathf.Sin(t * 2f * Mathf.PI * 900f) * 0.3f * Mathf.Exp(-t * 40f));
                    break;
                case "whee":     // "uiii" felice che sale
                    c = Make(kind, 0.7f, t =>
                    {
                        float f = Mathf.Lerp(500f, 1200f, t / 0.7f) + 30f * Mathf.Sin(t * 2f * Mathf.PI * 7f);
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.28f * Mathf.Min(1f, t * 30f) * Mathf.Clamp01((0.7f - t) * 6f);
                    });
                    break;
                case "pick":     // "plip"
                    c = Make(kind, 0.08f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(600f, 1100f, t / 0.08f)) * 0.4f * Mathf.Exp(-t * 30f));
                    break;
                case "clack":    // sasso sul prato
                    c = Make(kind, 0.1f, t => (N() * 0.4f + Mathf.Sin(t * 2f * Mathf.PI * 240f) * 0.3f) * Mathf.Exp(-t * 45f));
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
            AudioClip clip = AudioClip.Create("v80a_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
