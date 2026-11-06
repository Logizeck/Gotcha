using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 63 della roadmap "The Tiny Crown" (id tiny_crown) — "CROWN THE KING." Nella sala del trono un re dalla
    /// testa ENORME (barbone bianco) è seduto sul trono; accanto, su un cuscino rosso sopra un piedistallo d'oro, c'è una
    /// corona minuscola.
    /// Inganno: trascinare la corona così com'è sulla testa non funziona: scivola giù dalla testona, cade e torna con un
    /// saltello sul cuscino ("It's a bit... small."; se era già un po' più grande: "Almost! Still too small."). Toccare il
    /// re: "Where is my crown?". Toccare la corona: "Fit for a mouse.". Tutto reversibile, mai sconfitta.
    /// Soluzione: pizzico a due dita SULLA corona (agisce solo sulla corona, mai sulla scena) per allargarla: scala
    /// limitata tra 1 e 3; da 2.2 in su brilla (scintille + "ting"). Poi trascinarla sulla testa: tenuta grande sopra la
    /// zona generosa della testa, la testa fa un bagliore dorato; lasciata lì si aggancia da sola al posto giusto e con la
    /// misura giusta. Dal pizzico al trascinamento: alzando un dito, l'altro continua a trascinare la corona. Corona grande
    /// lasciata altrove: resta dove è stata lasciata, con la sua misura ("Nice size! Wrong place.").
    /// Vittoria: la corona si incastra ("PERFECT FIT!"), il re scende dal trono e si inginocchia davanti al giocatore
    /// ("Your Majesty!"); LevelReward "ALL HAIL THE REAL KING!" -> Win una volta sola.
    /// Dita: al massimo due (V64AFingers). Pausa, app in background o livello spento lasciano andare tutto (la corona
    /// sopra la testa torna sul cuscino). La logica è in Gotcha.Levels.V64.TinyCrownLogic.
    /// Tasti di prova nell'Editor: W soluzione (pizzico al massimo e corona sulla testa), S corona piccola sulla testa,
    /// A corona quasi grande (x2) sulla testa, O corona grande lasciata lontano dalla testa, G pizzico al massimo e lasciata
    /// sul posto, T tocco sul re, C tocco sulla corona, P pausa simulata (dita perse), R ricomincia. Shift + clic pianta
    /// un secondo dito (V64AFingers) per il pizzico.
    /// </summary>
    public class V84TinyCrownLevel : LevelController
    {
        [Header("Re (radice sul trono; corpo seduto/in ginocchio; testa con gli occhi)")]
        [SerializeField] Transform king;
        [SerializeField] Collider2D kingArea;
        [SerializeField] Transform bodySit;
        [SerializeField] Transform bodyKneel;
        [SerializeField] Transform head;
        [SerializeField] Transform eyes;
        [SerializeField] Transform eyesHappy;
        [SerializeField] float headTopY = 1.05f;         // cima della testa (locale della testa)
        [SerializeField] Transform kneelSpot;            // dove si inginocchia (davanti al trono)
        [Header("Corona (radice posata dal livello: scala = crownBase * scala della logica)")]
        [SerializeField] Transform crown;
        [SerializeField] float crownBase = 1.1f;
        [SerializeField] float crownLift = 0.08f;        // metà altezza della base sotto l'origine del disegno (per scala 1)
        [SerializeField] Transform[] sparkles;
        [SerializeField] SpriteRenderer headGlow;
        [Header("Fumetto, scritte e festa")]
        [SerializeField] SpeechBubble kingBubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "ALL HAIL THE REAL KING!";
        const float TapSlop = 0.15f, PinchNear = 1.6f, CrownHitMin = 0.65f, Follow = 16f, HeldFollow = 35f, ScaleFollow = 18f;

        static readonly string[] KingLines = { "Where is my crown?", "A king needs a crown!", "My head feels cold.", "Crown, please!" };
        static readonly string[] SmallLines = { "It's a bit... small.", "Plink. Off it goes.", "That's a ring, not a crown.", "Too tiny for that head." };
        static readonly string[] AlmostLines = { "Almost! Still too small.", "Bigger... but not enough.", "So close. Bigger!" };
        static readonly string[] OffLines = { "Nice size! Wrong place.", "On the head, please!", "The head is over there." };
        static readonly string[] TinyTapLines = { "Fit for a mouse.", "Shiny! And tiny.", "A crown for an ant." };
        static readonly string[] BigTapLines = { "Big and shiny!", "Now that's a crown." };
        const string FitLine = "PERFECT FIT!", KneelLine = "Your Majesty!";

        enum Kind { None, Crown, King, Pinch }

        sealed class Grip
        {
            public Kind kind;
            public Vector2 start, last;
            public bool moved;
        }

        sealed class FxSlot
        {
            public Coroutine co;
            public Quaternion q;
        }

        TinyCrownLogic logic;
        Camera cam;
        HwToast toast;
        HwSay say;
        readonly Dictionary<int, Grip> grips = new Dictionary<int, Grip>();
        readonly List<int> gone = new List<int>();
        readonly Dictionary<Transform, FxSlot> wiggles = new Dictionary<Transform, FxSlot>();
        Vector3 kingRest, headRestPos, crownHome, sparkleScale = Vector3.one;
        Quaternion headRestRot;
        Transform crownParent;
        Coroutine fallCo;
        float vis = 1f, glowA, lastSqueak = -10f;
        int kingLines, smallLines, almostLines, offLines, tapLines, seenOffDrops;
        bool captured, finishing, falling, wasBig;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            fallCo = null;
            StopFx();
            if (!captured)
            {
                captured = true;
                kingRest = king.position;
                headRestPos = head.localPosition;
                headRestRot = head.localRotation;
                crownParent = crown.parent;
                crownHome = crown.position;
                if (sparkles != null && sparkles.Length > 0 && sparkles[0] != null) sparkleScale = sparkles[0].localScale;
                Vector2 t = CrownTarget();
                logic = new TinyCrownLogic(crownHome.x, crownHome.y, t.x, t.y);
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            V64AFingers.Reset();
            grips.Clear();
            finishing = falling = wasBig = false;
            kingLines = smallLines = almostLines = offLines = tapLines = seenOffDrops = 0;
            toast = new HwToast(infoText, infoHolder);
            say = new HwSay(kingBubble);
            RestPose();
        }

        /// <summary>Dove sta il centro della corona agganciata (cima della testa, con la misura giusta).</summary>
        Vector2 CrownTarget()
        {
            Vector3 top = head.TransformPoint(new Vector3(0f, headTopY, 0f));
            return new Vector2(top.x, top.y - 0.1f + crownLift * crownBase * TinyCrownLogic.CrownedScale);
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            say?.Hide();
            KingOnThrone();
            if (crown.parent != crownParent) crown.SetParent(crownParent, true);
            vis = logic != null ? logic.Scale : 1f;
            crown.position = logic != null ? new Vector3(logic.X, logic.Y, crownHome.z) : crownHome;
            crown.localRotation = Quaternion.identity;
            crown.localScale = Vector3.one * (crownBase * vis);
            glowA = 0f;
            SetGlow(0f);
            ShowSparkles(false);
        }

        void KingOnThrone()
        {
            king.position = kingRest;
            king.localRotation = Quaternion.identity;
            head.localPosition = headRestPos;
            head.localRotation = headRestRot;
            HalloweenKit.Show(bodySit, true);
            HalloweenKit.Show(bodyKneel, false);
            Happy(false);
        }

        void Happy(bool on)
        {
            HalloweenKit.Show(eyes, !on);
            HalloweenKit.Show(eyesHappy, on);
        }

        void SetGlow(float a)
        {
            if (headGlow == null) return;
            Color c = headGlow.color;
            c.a = a;
            headGlow.color = c;
            headGlow.enabled = a > 0.01f;
        }

        void ShowSparkles(bool on)
        {
            if (sparkles == null) return;
            foreach (Transform s in sparkles) HalloweenKit.Show(s, on);
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: si molla tutto, si torna a uno stato valido
            CancelAll();
            StopFall();
            StopFx();
            if (captured && !finishing && !IsFinished && logic != null) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == TinyCrownStage.Celebrating) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelAll(); }

        /// <summary>Le dita spariscono: si molla la corona (dove si trova; sopra la testa torna sul cuscino), niente battute.</summary>
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
            if (logic != null && crown != null) TickSparkles();
            if (IsFinished || toast == null || logic == null) return;
            if (finishing) return;
            if (Time.timeScale <= 0f) { CancelAll(); return; }   // pausa o pubblicità: niente input
#if UNITY_EDITOR
            EditorKeys();
#endif
            if (!finishing) ReadFingers();
            if (!finishing) Pose();
        }

        /// <summary>La corona segue la logica (posizione e scala, morbide); il bagliore sulla testa; il "ting" quando diventa grande.</summary>
        void Pose()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            bool inHand = logic.Held || logic.Pinching;
            vis = Mathf.Lerp(vis, logic.Scale, 1f - Mathf.Exp(-dt * ScaleFollow));
            crown.localScale = Vector3.one * (crownBase * vis);
            if (!falling)
            {
                Vector3 to = new Vector3(logic.X, logic.Y, crown.position.z);
                crown.position = Vector3.Lerp(crown.position, to, 1f - Mathf.Exp(-dt * (inHand ? HeldFollow : Follow)));
                crown.localRotation = Quaternion.identity;
            }
            // bagliore dorato: corona grande tenuta sopra la testa (si può lasciare)
            float want = inHand && logic.BigEnough && logic.OverHead ? 0.55f + 0.15f * Mathf.Sin(Time.time * 8f) : 0f;
            glowA = Mathf.MoveTowards(glowA, want, dt * 3f);
            SetGlow(glowA);
            bool big = logic.BigEnough;
            if (big && !wasBig)
            {
                V84TinyCrownSound.Play(this, "ting", 0.5f);
                Haptics.Light();
            }
            wasBig = big;
        }

        /// <summary>Scintille intorno alla corona grande abbastanza (anche durante la festa): brillano a turno.</summary>
        void TickSparkles()
        {
            if (sparkles == null) return;
            bool on = !falling && (logic.BigEnough || finishing);
            float s = Mathf.Abs(crown.lossyScale.x) / crownBase;   // scala attuale della corona (1 = minuscola)
            for (int i = 0; i < sparkles.Length; i++)
            {
                Transform sp = sparkles[i];
                if (sp == null) continue;
                if (sp.gameObject.activeSelf != on) sp.gameObject.SetActive(on);
                if (!on) continue;
                float a = i * 1.7f;
                Vector3 off = new Vector3(Mathf.Cos(a) * 0.2f, 0.05f + Mathf.Sin(a) * 0.12f, 0f) * s;
                sp.position = crown.position + off + new Vector3(0f, 0f, -0.01f);
                float tw = 0.5f + 0.5f * Mathf.Sin(Time.time * 7f + a * 2.3f);
                sp.localScale = sparkleScale * (0.35f + 0.65f * tw);
                sp.localRotation = Quaternion.Euler(0f, 0f, Time.time * 40f + a * 30f);
            }
        }

        // ---------- Effetti che non si accavallano ----------

        void FxWiggle(Transform t, float degrees, float seconds, float speed)
        {
            if (t == null || !isActiveAndEnabled) return;
            if (wiggles.TryGetValue(t, out FxSlot slot) && slot.co != null)
            {
                StopCoroutine(slot.co);
                t.localRotation = slot.q;
            }
            else
            {
                if (slot == null) { slot = new FxSlot(); wiggles[t] = slot; }
                slot.q = t.localRotation;
            }
            slot.co = StartCoroutine(FxRun(slot, HalloweenKit.Wiggle(t, degrees, seconds, speed)));
        }

        static IEnumerator FxRun(FxSlot slot, IEnumerator e)
        {
            yield return e;
            slot.co = null;
        }

        void StopFx()
        {
            foreach (KeyValuePair<Transform, FxSlot> kv in wiggles)
                if (kv.Value.co != null) { StopCoroutine(kv.Value.co); if (kv.Key != null) kv.Key.localRotation = kv.Value.q; kv.Value.co = null; }
            wiggles.Clear();
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
                foreach (int id in gone) if (grips.TryGetValue(id, out Grip g)) End(id, g);
                if (finishing) grips.Clear();
            }
        }

        static bool Over(Collider2D c, Vector2 p) => c != null && c.gameObject.activeInHierarchy && c.OverlapPoint(p);

        float CrownDistance(Vector2 w) => Vector2.Distance(w, crown.position);
        /// <summary>Bersaglio generoso: almeno 0.65 di raggio, cresce con la corona.</summary>
        bool OverCrown(Vector2 w) => !falling && CrownDistance(w) <= Mathf.Max(CrownHitMin, 0.2f * crownBase * vis + 0.3f);

        Grip Find(Kind k)
        {
            foreach (Grip g in grips.Values) if (g.kind == k) return g;
            return null;
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, kind = Kind.None };
            if (finishing || logic.InputLocked) return g;
            if (Find(Kind.Pinch) != null) return g;   // pizzico già in corso: il terzo gesto non fa niente
            bool onCrown = OverCrown(w);
            bool nearCrown = !falling && (onCrown || CrownDistance(w) < PinchNear);
            // secondo dito vicino alla corona tenuta (o primo dito vicino e secondo sulla corona): pizzico
            Grip other = Find(Kind.Crown);
            if (other == null && onCrown)
                foreach (Grip o in grips.Values)
                    if (o.kind == Kind.None && !falling && CrownDistance(o.last) < PinchNear) { other = o; break; }
            if (other != null && nearCrown)
            {
                if (!logic.StartPinch(Vector2.Distance(w, other.last))) return g;
                other.kind = Kind.Pinch;
                g.kind = Kind.Pinch;
                Haptics.Tap();
                return g;
            }
            if (other == null && onCrown) { g.kind = Kind.Crown; return g; }
            if (Over(kingArea, w) && Find(Kind.King) == null) g.kind = Kind.King;
            return g;
        }

        void Drag(Grip g)
        {
            if (g.kind != Kind.Crown) return;
            if (!g.moved)
            {
                if (Vector2.Distance(g.last, g.start) <= TapSlop) return;
                g.moved = true;
                if (!logic.Held && !logic.Grab(g.start.x, g.start.y)) { g.kind = Kind.None; return; }
                Haptics.Tap();
                V84TinyCrownSound.Play(this, "pick", 0.35f);
            }
            if (logic.Held) logic.Drag(g.last.x, g.last.y);
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
            float before = logic.Scale;
            float s = logic.Pinch(Vector2.Distance(a.last, b.last));
            if (Mathf.Abs(s - before) > 0.001f && Time.time - lastSqueak > 0.12f)
            {
                lastSqueak = Time.time;
                V84TinyCrownSound.Play(this, "squeak", 0.2f);
            }
        }

        void End(int id, Grip g)
        {
            grips.Remove(id);
            switch (g.kind)
            {
                case Kind.Crown:
                    if (!g.moved) TapCrown();
                    else Dropped(logic.Release());
                    break;
                case Kind.King:
                    if (Vector2.Distance(g.last, g.start) <= 0.35f) TapKing();
                    break;
                case Kind.Pinch:
                    // dal pizzico al trascinamento: se resta un dito del pizzico, continua a trascinare la corona
                    Grip rest = Find(Kind.Pinch);
                    if (rest != null)
                    {
                        rest.kind = Kind.Crown;
                        rest.moved = true;
                        rest.start = rest.last;
                        Dropped(logic.EndPinch(true, rest.last.x, rest.last.y));
                    }
                    else Dropped(logic.EndPinch(false));
                    break;
            }
        }

        // ---------- Reazioni ----------

        void Dropped(TinyCrownDrop d)
        {
            switch (d)
            {
                case TinyCrownDrop.TooSmall:
                    Fall();
                    break;
                case TinyCrownDrop.Placed:
                    if (logic.OffHeadDrops > seenOffDrops)
                    {
                        seenOffDrops = logic.OffHeadDrops;
                        toast.Show(OffLines[offLines++ % OffLines.Length], 1.6f);
                    }
                    V84TinyCrownSound.Play(this, "tink", 0.3f);
                    break;
                case TinyCrownDrop.Crowned:
                    StartCoroutine(Finale());
                    break;
            }
        }

        void TapKing()
        {
            if (!logic.TapKing()) return;
            FxWiggle(head, 5f, 0.4f, 22f);
            V84TinyCrownSound.Play(this, "hmm", 0.45f);
            say.Show(KingLines[kingLines++ % KingLines.Length], 1.6f);
        }

        void TapCrown()
        {
            if (!logic.TapCrown()) return;
            V84TinyCrownSound.Play(this, "tink", 0.4f);
            if (logic.BigEnough) toast.Show(BigTapLines[tapLines++ % BigTapLines.Length], 1.4f);
            else toast.Show(TinyTapLines[tapLines++ % TinyTapLines.Length], 1.4f);
        }

        /// <summary>Corona troppo piccola sulla testa: scivola giù dalla testona girando, cade e torna con un saltello sul cuscino.</summary>
        void Fall()
        {
            bool almost = logic.LastTryScale >= 1.6f;
            toast.Show(almost ? AlmostLines[almostLines++ % AlmostLines.Length] : SmallLines[smallLines++ % SmallLines.Length], 1.7f);
            V84TinyCrownSound.Play(this, "plink", 0.45f);
            Haptics.Light();
            FxWiggle(head, 3f, 0.35f, 26f);
            StopFall();
            if (isActiveAndEnabled) fallCo = StartCoroutine(FallRun());
        }

        IEnumerator FallRun()
        {
            falling = true;
            Vector3 c = head.position;
            float rx = HeadRadius() * 0.95f;
            Vector3 from = crown.position;
            float side = from.x >= c.x ? 1f : -1f;
            Vector3 edge = new Vector3(c.x + side * rx, c.y + 0.15f, from.z);
            Vector3 floor = new Vector3(Mathf.Clamp(edge.x + side * 0.35f, -2.2f, 2.2f), -2.6f, from.z);
            Vector3 home = new Vector3(logic.X, logic.Y, from.z);
            // 1) scivola lungo la testa (arco) girando
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.35f)
            {
                float a = Mathf.Lerp(Mathf.Atan2(from.y - c.y, from.x - c.x), Mathf.Atan2(edge.y - c.y, edge.x - c.x), t * t);
                float r = Mathf.Lerp(Vector2.Distance(from, c), rx, t);
                crown.position = new Vector3(c.x + Mathf.Cos(a) * r, c.y + Mathf.Sin(a) * r, from.z);
                crown.localRotation = Quaternion.Euler(0f, 0f, -side * 60f * t);
                yield return null;
            }
            // 2) cade giù
            Vector3 p0 = crown.position;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f)
            {
                crown.position = Vector3.Lerp(p0, floor, t * t);
                crown.localRotation = Quaternion.Euler(0f, 0f, -side * (60f + 200f * t));
                yield return null;
            }
            V84TinyCrownSound.Play(this, "tink", 0.35f);
            // 3) saltello fino al cuscino
            crown.localRotation = Quaternion.identity;
            yield return HalloweenKit.Move(crown, home, 0.4f, 0.9f);
            falling = false;
            fallCo = null;
        }

        /// <summary>Raggio della testona nel mondo (il disegno ha raggio 1).</summary>
        float HeadRadius() => Mathf.Abs(head.lossyScale.x);

        void StopFall()
        {
            if (fallCo != null) StopCoroutine(fallCo);
            fallCo = null;
            if (falling && crown != null && logic != null)
            {
                crown.position = new Vector3(logic.X, logic.Y, crown.position.z);
                crown.localRotation = Quaternion.identity;
            }
            falling = false;
        }

        // ---------- La soluzione ----------

        /// <summary>
        /// La corona si aggancia sulla testa con la misura giusta ("PERFECT FIT!", occhi felici), poi il re scende dal
        /// trono con un saltello e si inginocchia davanti al giocatore chinando la testona ("Your Majesty!"), la corona
        /// resta sulla testa. Poi la festa comune e Win, una volta sola. Rifatta da capo se il livello si spegne a metà.
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            grips.Clear();
            V64AFingers.Reset();
            StopFall();
            StopFx();
            HalloweenKit.Show(infoHolder, false);
            say.Hide();
            SetGlow(0f);
            KingOnThrone();
            if (crown.parent != crownParent) crown.SetParent(crownParent, true);

            // 1) la corona si incastra al posto giusto
            Vector3 from = crown.position, to = (Vector3)CrownTarget() + new Vector3(0f, 0f, crown.position.z);
            float s0 = vis;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.25f)
            {
                float e = 1f - (1f - t) * (1f - t);
                crown.position = Vector3.Lerp(from, to, e);
                vis = Mathf.Lerp(s0, TinyCrownLogic.CrownedScale, e);
                crown.localScale = Vector3.one * (crownBase * vis);
                crown.localRotation = Quaternion.identity;
                yield return null;
            }
            vis = TinyCrownLogic.CrownedScale;
            crown.position = to;
            crown.localScale = Vector3.one * (crownBase * vis);
            V84TinyCrownSound.Play(this, "clunk", 0.55f);
            Haptics.Medium();
            Happy(true);
            say.Show(FitLine, 0.6f);
            yield return new WaitForSeconds(0.55f);

            // 2) scende dal trono e si inginocchia davanti a te, la corona va con la testa
            crown.SetParent(head, true);
            Vector3 spot = kneelSpot != null ? kneelSpot.position : kingRest + new Vector3(0.5f, -1.4f, 0f);
            yield return HalloweenKit.Move(king, new Vector3(spot.x, spot.y, kingRest.z), 0.35f, 0.6f);
            HalloweenKit.Show(bodySit, false);
            HalloweenKit.Show(bodyKneel, true);
            V84TinyCrownSound.Play(this, "fanfare", 0.5f);
            Haptics.Light();
            Vector3 hp = headRestPos, hpDown = headRestPos + new Vector3(0f, -0.25f, 0f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.25f)
            {
                head.localPosition = Vector3.Lerp(hp, hpDown, t);
                head.localRotation = headRestRot * Quaternion.Euler(0f, 0f, -6f * t);
                yield return null;
            }
            head.localPosition = hpDown;
            head.localRotation = headRestRot * Quaternion.Euler(0f, 0f, -6f);
            say.Show(KneelLine, 2.4f);
            yield return new WaitForSeconds(0.45f);

            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            if (logic.TryComplete()) Win(Compliment);
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked || grips.Count > 0 || logic.Held || logic.Pinching || falling) return;
            Vector2 top = CrownTarget();
            if (Input.GetKeyDown(KeyCode.W)) { KeyPinch(3f); KeyDrop(top); }
            if (Input.GetKeyDown(KeyCode.S)) KeyDrop(top + new Vector2(0.2f, -0.3f));
            if (Input.GetKeyDown(KeyCode.A)) { KeyPinch(2f); KeyDrop(top); }
            if (Input.GetKeyDown(KeyCode.O)) { KeyPinch(3f); KeyDrop(new Vector2(1.5f, 0.6f)); }
            if (Input.GetKeyDown(KeyCode.G)) KeyPinch(3f);
            if (Input.GetKeyDown(KeyCode.T)) TapKing();
            if (Input.GetKeyDown(KeyCode.C)) TapCrown();
            if (Input.GetKeyDown(KeyCode.P)) CancelAll();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }

        /// <summary>Pizzico simulato che porta la corona alla scala voluta (rispetto a quella attuale) e lo chiude sul posto.</summary>
        void KeyPinch(float targetScale)
        {
            float s = logic.Scale;
            if (!logic.StartPinch(1f)) return;
            logic.Pinch(targetScale / Mathf.Max(0.01f, s));
            Dropped(logic.EndPinch(false));
        }

        void KeyDrop(Vector2 at)
        {
            if (logic.InputLocked || !logic.Grab(logic.X, logic.Y)) return;
            logic.Drag(at.x, at.y);
            crown.position = new Vector3(logic.X, logic.Y, crown.position.z);
            Dropped(logic.Release());
        }
#endif
    }

    /// <summary>Suoni sintetizzati di The Tiny Crown: "ting" della corona che brilla, presa, "tink" metallico, "plink" della caduta, squittio del pizzico, "hmm" del re, "clunk" dell'aggancio, fanfara.</summary>
    public static class V84TinyCrownSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(281);

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
        static float Sin(float t, float f) => Mathf.Sin(t * 2f * Mathf.PI * f);

        static AudioClip Clip(string kind)
        {
            if (cache.TryGetValue(kind, out AudioClip c) && c != null) return c;
            switch (kind)
            {
                case "ting":     // brilla: due campanellini
                    c = Make(kind, 0.5f, t => (Sin(t, 1760f) * Mathf.Exp(-t * 9f) + (t > 0.08f ? Sin(t, 2637f) * Mathf.Exp(-(t - 0.08f) * 9f) : 0f)) * 0.3f);
                    break;
                case "pick":     // "plip"
                    c = Make(kind, 0.08f, t => Sin(t, Mathf.Lerp(600f, 1100f, t / 0.08f)) * 0.4f * Mathf.Exp(-t * 30f));
                    break;
                case "tink":     // metallo leggero
                    c = Make(kind, 0.18f, t => (Sin(t, 2200f) * 0.5f + Sin(t, 3300f) * 0.25f) * 0.35f * Mathf.Exp(-t * 28f));
                    break;
                case "plink":    // scivola giù: nota che scende
                    c = Make(kind, 0.35f, t => Sin(t, Mathf.Lerp(1400f, 500f, t / 0.35f)) * 0.3f * Mathf.Exp(-t * 6f));
                    break;
                case "squeak":   // metallo che si allarga
                    c = Make(kind, 0.06f, t => Sin(t, Mathf.Lerp(900f, 1200f, t / 0.06f)) * 0.2f * Mathf.Sin(t / 0.06f * Mathf.PI));
                    break;
                case "hmm":      // "hmm?" del re: vocale bassa che sale
                    c = Make(kind, 0.4f, t =>
                    {
                        float f = Mathf.Lerp(110f, 150f, t / 0.4f);
                        return (Sin(t, f) * 0.5f + Sin(t, f * 2f) * 0.25f + Sin(t, f * 3f) * 0.1f) * 0.45f * Mathf.Sin(t / 0.4f * Mathf.PI);
                    });
                    break;
                case "clunk":    // si incastra: colpo + campanella
                    c = Make(kind, 0.4f, t => N() * 0.5f * Mathf.Exp(-t * 60f) + Sin(t, 1318f) * 0.3f * Mathf.Exp(-t * 7f));
                    break;
                case "fanfare":  // tre note di tromba
                    c = Make(kind, 0.75f, t =>
                    {
                        float f = t < 0.18f ? 523f : t < 0.36f ? 659f : 784f;
                        float lt = t < 0.18f ? t : t < 0.36f ? t - 0.18f : t - 0.36f;
                        float env = Mathf.Min(1f, lt * 60f) * (t < 0.36f ? 1f : Mathf.Exp(-(t - 0.36f) * 4f));
                        return (Sin(t, f) * 0.5f + Sin(t, f * 2f) * 0.3f + Sin(t, f * 3f) * 0.15f) * 0.35f * env;
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
            AudioClip clip = AudioClip.Create("v84a_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
