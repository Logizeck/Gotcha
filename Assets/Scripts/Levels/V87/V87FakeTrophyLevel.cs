using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 129 della roadmap "The Fake Trophy" (id fake_trophy) — "WIN THE TROPHY." Una coppa grigia e anonima su un
    /// piedistallo, un omino deluso in tuta da atleta e una grande insegna luminosa con la parola WIN.
    /// Inganno: toccare la coppa fa solo "clink" ("Clink. Hollow."); trascinare la coppa sull'omino non serve, torna sul
    /// piedistallo ("It's not a real prize."); toccare l'insegna fa lampeggiare WIN e basta. Tutto reversibile, mai sconfitta.
    /// Soluzione: pressione LUNGA su WIN (0.7 s, un anello di puntini si riempie attorno al dito e la parola trema), WIN si
    /// stacca dall'insegna (sull'insegna resta l'alone) e segue il dito; quando arriva nella zona centrale della coppa scatta
    /// lì, la coppa diventa d'oro all'istante, l'omino la solleva trionfante; applauso, LevelReward "CHAMPION BY DEFINITION!"
    /// -> Win una volta sola. WIN lasciata altrove torna sull'insegna (serve un'altra pressione lunga).
    /// WIN è un oggetto indipendente (un testo): il bersaglio per il dito è ricavato dai limiti disegnati del testo, allargati
    /// (niente coordinate fisse: una traduzione più lunga o più corta funziona lo stesso); anche la scala con cui si posa
    /// sulla coppa è ricavata dalla sua larghezza. Le posizioni per la logica arrivano dagli oggetti della scena.
    /// Dita: un gesto alla volta (V64AFingers; un secondo dito non fa niente). Pausa, app in background o livello spento
    /// lasciano andare tutto (WIN torna sull'insegna, la coppa sul piedistallo, nessuna vittoria). La logica è in
    /// Gotcha.Levels.V64.FakeTrophyLogic.
    /// Tasti di prova nell'Editor: W soluzione (pressione lunga + WIN sulla coppa), H pressione breve su WIN, E WIN lasciata
    /// altrove, S tocco sull'insegna, T tocco sulla coppa, G coppa trascinata sull'omino, D coppa lasciata altrove,
    /// U tocco sull'omino, P pausa simulata (dita perse), R ricomincia.
    /// </summary>
    public class V87FakeTrophyLevel : LevelController
    {
        [Header("Insegna (WIN è un oggetto a parte)")]
        [SerializeField] Transform sign;
        [SerializeField] Transform signBody;
        [SerializeField] Collider2D signArea;
        [SerializeField] SpriteRenderer bulbsA;
        [SerializeField] SpriteRenderer bulbsB;
        [SerializeField] Transform wordGhost;
        [Header("La parola WIN (radice al centro del testo)")]
        [SerializeField] Transform word;
        [SerializeField] TextMesh wordText;
        [SerializeField] TextMesh wordShadow;
        [Header("La coppa (radice alla base) e gli effetti")]
        [SerializeField] Transform trophy;
        [SerializeField] Transform trophyBody;
        [SerializeField] Transform trophyGrey;
        [SerializeField] Transform trophyGold;
        [SerializeField] Collider2D trophyArea;
        [SerializeField] SpriteRenderer goldGlow;
        [SerializeField] SpriteRenderer sparkle;
        [SerializeField] SpriteRenderer clinkFx;
        [Header("L'omino (radice ai piedi)")]
        [SerializeField] Transform guy;
        [SerializeField] Transform guyBody;
        [SerializeField] Transform guySad;
        [SerializeField] Transform guyLift;
        [SerializeField] Collider2D guyArea;
        [Header("Anello della pressione lunga, fumetto, scritte e festa")]
        [SerializeField] Transform ring;
        [SerializeField] SpriteRenderer[] ringDots;
        [SerializeField] SpeechBubble guyBubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "CHAMPION BY DEFINITION!";
        const float TapSlop = FakeTrophyLogic.TapSlop;
        const float WordMinW = 1.4f, WordMinH = 0.9f, WordMargin = 0.3f;   // bersaglio di WIN: limiti del testo allargati
        const float HeldScale = 0.8f, PlateWidth = 0.8f, MaxSnapScale = 0.7f;
        const float BowlY = 0.92f, ZoneHalfW = 0.6f, ZoneHalfH = 0.6f;     // zona centrale della coppa (coordinate della coppa)
        const float LiftHandsY = 2.22f;                                     // dove l'omino a braccia alzate tiene la base della coppa
        const float ChaseSeconds = 0.35f;
        static readonly Color WordColor = new Color(1f, 0.89f, 0.36f), WordFlash = Color.white;
        static readonly Color ShadowColor = new Color(0.55f, 0.08f, 0.2f), ShadowFlash = new Color(1f, 0.25f, 0.45f);
        static readonly Color DotOff = new Color(1f, 1f, 1f, 0.35f), DotOn = new Color(1f, 0.82f, 0.2f, 1f);
        static readonly Vector2 BubbleTip = new Vector2(0.35f, 2.45f);     // dalla radice dell'omino: sopra la testa, a destra (sopra la coppa)
        static readonly Vector2 WinBubbleTip = new Vector2(0.9f, 1.9f);    // con la coppa sollevata: più a destra, fuori dalla coppa

        static readonly string[] ClinkLines = { "Clink. Hollow.", "Clink. Still hollow.", "Plastic. Painted grey.", "Clonk. Nothing inside." };
        static readonly string[] RejectLines = { "It's not a real prize.", "That's just a grey cup.", "No thanks. It's fake.", "I didn't WIN that." };
        static readonly string[] TrophyBackLines = { "Back on its pedestal.", "It hops back home.", "The pedestal wants it back." };
        static readonly string[] SignLines = { "Flashy. Still no prize.", "WIN! (It's just a sign.)", "Blinking won't help.", "Very shiny. Very useless." };
        static readonly string[] ReturnLines = { "WIN zips back to the sign.", "Not there.", "Back on the sign it goes.", "Snap! Home again." };
        static readonly string[] SighLines = { "I came second. To nobody.", "A grey trophy. For me?", "It doesn't even say anything.", "Sigh." };
        const string WinnerLine = "I'M A WINNER!";

        enum Kind { None, Press, Word, Trophy, Sign, Guy }

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

        FakeTrophyLogic logic;
        Camera cam;
        HwToast toast;
        HwSay say;
        readonly Dictionary<int, Grip> grips = new Dictionary<int, Grip>();
        readonly List<int> gone = new List<int>();
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        Transform wordParent;
        Vector3 wordHome, trophyHome, wordScale0 = Vector3.one;
        Vector2 wordVis, trophyVis;
        float chaseT, flashT = -1f, clinkT = -1f, trophyTilt, trophyVelX, glowT = -1f, sparkleT = -1f;
        int clinkLines, rejectLines, trophyBackLines, signLines, returnLines, sighLines, litDots;
        bool captured, finishing;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            if (!captured)
            {
                captured = true;
                wordParent = word.parent;
                wordHome = word.position;
                wordScale0 = word.localScale;
                trophyHome = trophy.position;
                logic = BuildLogic();
            }
            RestoreWordParent();
            logic.Reset();
            if (reward != null) reward.ResetReward();
            V64AFingers.Reset();
            grips.Clear();
            finishing = false;
            clinkLines = rejectLines = trophyBackLines = signLines = returnLines = sighLines = 0;
            toast = new HwToast(infoText, infoHolder);
            say = new HwSay(guyBubble);
            RestPose();
        }

        /// <summary>
        /// La logica con le posizioni prese dalla scena: WIN sull'insegna, base della coppa sul piedistallo, zona centrale
        /// della coppa (in proporzione alla sua scala) e area dell'omino (dal suo collider).
        /// </summary>
        FakeTrophyLogic BuildLogic()
        {
            float s = Mathf.Abs(trophy.lossyScale.y);
            float gx = FakeTrophyLogic.DefaultGuyX, gy = FakeTrophyLogic.DefaultGuyY, ghw = FakeTrophyLogic.DefaultGuyHalfW, ghh = FakeTrophyLogic.DefaultGuyHalfH;
            if (guyArea is BoxCollider2D box)
            {
                Vector2 c = guy.TransformPoint(box.offset);
                Vector2 e = Vector2.Scale(box.size * 0.5f, new Vector2(Mathf.Abs(guy.lossyScale.x), Mathf.Abs(guy.lossyScale.y)));
                gx = c.x; gy = c.y; ghw = e.x; ghh = e.y;
            }
            return new FakeTrophyLogic(wordHome.x, wordHome.y, trophyHome.x, trophyHome.y,
                trophyHome.x, trophyHome.y + BowlY * s, ZoneHalfW * s, ZoneHalfH * s, gx, gy, ghw, ghh);
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            say?.Hide();
            RestoreWordParent();
            wordVis = new Vector2(logic.WordX, logic.WordY);
            word.position = new Vector3(wordVis.x, wordVis.y, wordHome.z);
            word.localScale = wordScale0;
            word.localRotation = Quaternion.identity;
            SetWordColor(WordColor);
            HalloweenKit.Show(word, true);
            HalloweenKit.Show(wordGhost, false);
            trophyVis = new Vector2(logic.TrophyX, logic.TrophyY);
            trophy.position = new Vector3(trophyVis.x, trophyVis.y, trophyHome.z);
            trophy.localRotation = Quaternion.identity;
            trophyTilt = trophyVelX = 0f;
            if (trophyBody != null) { trophyBody.localPosition = Vector3.zero; trophyBody.localRotation = Quaternion.identity; trophyBody.localScale = Vector3.one; }
            Gold(false);
            if (signBody != null) { signBody.localRotation = Quaternion.identity; signBody.localScale = Vector3.one; }
            GuyPose(false);
            if (guyBody != null) { guyBody.localPosition = Vector3.zero; guyBody.localRotation = Quaternion.identity; guyBody.localScale = Vector3.one; }
            HideRing();
            flashT = clinkT = glowT = sparkleT = -1f;
            chaseT = 0f;
            HalloweenKit.Show(clinkFx, false);
            HalloweenKit.Show(goldGlow, false);
            HalloweenKit.Show(sparkle, false);
            Bulbs(true, false);
        }

        void RestoreWordParent()
        {
            if (word != null && captured && word.parent != wordParent) word.SetParent(wordParent, true);
        }

        void Gold(bool on)
        {
            HalloweenKit.Show(trophyGrey, !on);
            HalloweenKit.Show(trophyGold, on);
        }

        void GuyPose(bool lift)
        {
            HalloweenKit.Show(guySad, !lift);
            HalloweenKit.Show(guyLift, lift);
        }

        void Bulbs(bool a, bool b)
        {
            if (bulbsA != null) bulbsA.enabled = a;
            if (bulbsB != null) bulbsB.enabled = b;
        }

        void SetWordColor(Color c)
        {
            if (wordText != null) wordText.color = c;
            if (wordShadow != null) wordShadow.color = c == WordColor ? ShadowColor : ShadowFlash;   // anche l'ombra si accende
        }

        /// <summary>Il pezzo dell'insegna che si anima (Body; in mancanza l'insegna intera).</summary>
        Transform SignFx => signBody != null ? signBody : sign;

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: si molla tutto, si torna a uno stato valido
            CancelAll();
            StopFx();
            if (captured && !finishing && !IsFinished && logic != null && !logic.InputLocked) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == FakeTrophyStage.Celebrating) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelAll(); }

        /// <summary>Le dita spariscono: WIN torna sull'insegna (Pose la riporta), la coppa sul piedistallo, l'anello si spegne.</summary>
        void CancelAll()
        {
            grips.Clear();
            V64AFingers.Reset();
            HideRing();
            if (logic == null || logic.InputLocked) return;
            logic.Cancel();
            if (word != null) word.localRotation = Quaternion.identity;
        }

        void Update()
        {
            toast?.Tick();
            say?.Tick();
            if (IsFinished || toast == null || logic == null) return;
            if (finishing) return;   // la festa anima da sé (anche gli effetti)
            if (Time.timeScale <= 0f) { CancelAll(); return; }   // pausa o pubblicità: niente input
#if UNITY_EDITOR
            EditorKeys();
#endif
            if (!finishing) ReadFingers();
            if (!finishing) Pose();
        }

        /// <summary>WIN e la coppa seguono la logica (morbide); le lampadine si rincorrono; gli effetti brevi.</summary>
        void Pose()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            // WIN: tenuta segue il dito, lasciata torna sull'insegna
            Vector2 wt = new Vector2(logic.WordX, logic.WordY);
            wordVis = Vector2.Lerp(wordVis, wt, 1f - Mathf.Exp(-dt * (logic.WordHeld ? 30f : 12f)));
            if ((wordVis - wt).sqrMagnitude < 1e-6f) wordVis = wt;
            word.position = new Vector3(wordVis.x, wordVis.y, wordHome.z);
            Vector3 ws = logic.WordHeld ? wordScale0 * HeldScale : wordScale0;
            word.localScale = Vector3.Lerp(word.localScale, ws, 1f - Mathf.Exp(-dt * 14f));
            float rot = 0f;
            if (logic.Pressing) rot = 5f * logic.PressProgress * Mathf.Sin(Time.time * 55f);   // trema sempre di più
            else if (logic.WordHeld) rot = 4f * Mathf.Sin(Time.time * 7f);
            word.localRotation = Quaternion.Euler(0f, 0f, rot);
            bool home = !logic.WordHeld && (wordVis - (Vector2)wordHome).sqrMagnitude < 0.0004f;
            HalloweenKit.Show(wordGhost, !home);

            // la coppa
            Vector2 tt = new Vector2(logic.TrophyX, logic.TrophyY);
            float before = trophyVis.x;
            trophyVis = Vector2.Lerp(trophyVis, tt, 1f - Mathf.Exp(-dt * (logic.TrophyHeld ? 30f : 14f)));
            if ((trophyVis - tt).sqrMagnitude < 1e-6f) trophyVis = tt;
            trophy.position = new Vector3(trophyVis.x, trophyVis.y, trophyHome.z);
            trophyVelX = dt > 0f ? Mathf.Lerp(trophyVelX, (trophyVis.x - before) / dt, 1f - Mathf.Exp(-dt * 10f)) : 0f;
            trophyTilt = Mathf.Lerp(trophyTilt, logic.TrophyHeld ? Mathf.Clamp(-trophyVelX * 3f, -12f, 12f) : 0f, 1f - Mathf.Exp(-dt * 12f));
            trophy.localRotation = Quaternion.Euler(0f, 0f, trophyTilt);

            // le lampadine si rincorrono; durante il lampeggio tutte insieme
            if (flashT >= 0f)
            {
                flashT += dt;
                bool on = Mathf.FloorToInt(flashT / 0.08f) % 2 == 0;
                Bulbs(on, on);
                SetWordColor(on ? WordFlash : WordColor);
                if (flashT > 0.64f) { flashT = -1f; SetWordColor(WordColor); }
            }
            else
            {
                chaseT += dt;
                bool a = Mathf.FloorToInt(chaseT / ChaseSeconds) % 2 == 0;
                Bulbs(a, !a);
            }
            TickFx(dt);
        }

        /// <summary>Lo "clink", il bagliore dell'oro e la stellina (brevi, non si accavallano).</summary>
        void TickFx(float dt)
        {
            if (clinkFx != null && clinkT >= 0f)
            {
                clinkT += dt;
                float k = clinkT / 0.3f;
                if (k >= 1f) { clinkT = -1f; clinkFx.gameObject.SetActive(false); }
                else
                {
                    clinkFx.gameObject.SetActive(true);
                    clinkFx.transform.localScale = Vector3.one * (0.7f + 0.5f * k);
                    HalloweenKit.SetAlpha(clinkFx, 1f - k * k);
                }
            }
            if (goldGlow != null && glowT >= 0f)
            {
                glowT += dt;
                float k = glowT / 0.6f;
                if (k >= 1f) { glowT = -1f; goldGlow.gameObject.SetActive(false); }
                else
                {
                    goldGlow.gameObject.SetActive(true);
                    goldGlow.transform.position = trophy.position + new Vector3(0f, BowlY * Mathf.Abs(trophy.lossyScale.y), 0f);
                    goldGlow.transform.localScale = Vector3.one * (1.2f + 1.6f * k);
                    HalloweenKit.SetAlpha(goldGlow, 0.9f * (1f - k));
                }
            }
            if (sparkle != null && sparkleT >= 0f)
            {
                sparkleT += dt;
                float k = sparkleT / 0.7f;
                if (k >= 1f) { sparkleT = -1f; sparkle.gameObject.SetActive(false); }
                else
                {
                    sparkle.gameObject.SetActive(true);
                    sparkle.transform.position = trophy.position + new Vector3(0.45f, 1.3f * Mathf.Abs(trophy.lossyScale.y), 0f);
                    sparkle.transform.localScale = Vector3.one * (1.1f * Mathf.Sin(k * Mathf.PI));
                    sparkle.transform.localRotation = Quaternion.Euler(0f, 0f, k * 140f);
                }
            }
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73-0.86) ----------

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

        // ---------- L'anello della pressione lunga ----------

        void ShowRing(Vector2 at, float progress)
        {
            if (ring == null) return;
            if (!ring.gameObject.activeSelf)
            {
                ring.gameObject.SetActive(true);
                litDots = -1;
            }
            ring.position = new Vector3(Mathf.Clamp(at.x, -2.1f, 2.1f), at.y, ring.position.z);
            int n = ringDots != null ? ringDots.Length : 0;
            int lit = Mathf.Clamp(Mathf.FloorToInt(progress * n + 1e-4f), 0, n);
            if (lit == litDots) return;
            if (litDots >= 0 && lit > litDots) V87FakeTrophySound.Play(this, "tick", 0.12f + 0.02f * lit);
            litDots = lit;
            for (int i = 0; i < n; i++) if (ringDots[i] != null) ringDots[i].color = i < lit ? DotOn : DotOff;
        }

        void HideRing()
        {
            litDots = -1;
            HalloweenKit.Show(ring, false);
        }

        // ---------- Dita (un gesto alla volta) ----------

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
            if (fingers.Count == 0 && grips.Count > 0)
            {
                // dita sparite senza "Ended": si chiudono i gesti
                gone.Clear();
                foreach (int id in grips.Keys) gone.Add(id);
                foreach (int id in gone) if (!finishing && grips.TryGetValue(id, out Grip g)) End(id, g);
            }
        }

        static bool Over(Collider2D c, Vector2 p) => c != null && c.gameObject.activeInHierarchy && c.OverlapPoint(p);

        /// <summary>
        /// Il bersaglio di WIN: i limiti DISEGNATI del testo (qualunque parola, qualunque lunghezza), mai più piccoli di
        /// WordMinW x WordMinH, allargati di WordMargin per lato. Prima che il testo sia costruito: un riquadro di riserva.
        /// </summary>
        public Bounds WordHitBounds()
        {
            MeshRenderer mr = wordText != null ? wordText.GetComponent<MeshRenderer>() : null;
            Bounds b = mr != null ? mr.bounds : new Bounds(word.position, Vector3.zero);
            if (b.size.x < 1e-3f || b.size.y < 1e-3f) b = new Bounds(word.position, new Vector3(2.4f, 1f, 1f));
            Vector3 size = new Vector3(Mathf.Max(b.size.x, WordMinW) + 2f * WordMargin, Mathf.Max(b.size.y, WordMinH) + 2f * WordMargin, 10f);
            return new Bounds(new Vector3(b.center.x, b.center.y, 0f), size);
        }

        bool OverWord(Vector2 p)
        {
            if (word == null || !word.gameObject.activeInHierarchy) return false;
            return WordHitBounds().Contains(new Vector3(p.x, p.y, 0f));
        }

        bool AnyBusyGrip()
        {
            foreach (Grip g in grips.Values) if (g.kind != Kind.None) return true;
            return false;
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, kind = Kind.None };
            if (finishing || logic.InputLocked || logic.Busy || AnyBusyGrip()) return g;   // un gesto alla volta
            bool wordAtHome = !logic.WordHeld && (wordVis - (Vector2)wordHome).sqrMagnitude < 0.04f;
            if (wordAtHome && OverWord(w))
            {
                if (logic.PressWord())
                {
                    g.kind = Kind.Press;
                    ShowRing(w, 0f);
                    V87FakeTrophySound.Play(this, "tick", 0.1f);
                    Haptics.Tap();
                }
                return g;
            }
            if (Over(trophyArea, w)) { g.kind = Kind.Trophy; g.offset = new Vector3(logic.TrophyX, logic.TrophyY, 0f) - (Vector3)w; return g; }
            if (Over(guyArea, w)) { g.kind = Kind.Guy; return g; }
            if (Over(signArea, w)) { g.kind = Kind.Sign; return g; }
            return g;
        }

        void Drag(Grip g)
        {
            switch (g.kind)
            {
                case Kind.Press:
                {
                    FakeTrophyResult r = logic.HoldWord(Time.deltaTime, Vector2.Distance(g.last, g.start));
                    if (r == FakeTrophyResult.Pressing) { ShowRing(g.start, logic.PressProgress); return; }
                    HideRing();
                    if (r == FakeTrophyResult.Detached)
                    {
                        // WIN si stacca: da qui segue il dito
                        g.kind = Kind.Word;
                        g.moved = true;
                        g.offset = word.position - (Vector3)g.last;
                        g.offset.z = 0f;
                        word.localRotation = Quaternion.identity;
                        V87FakeTrophySound.Play(this, "pop", 0.5f);
                        Haptics.Medium();
                        FxWiggle(SignFx, 3f, 0.3f, 30f);
                        HalloweenKit.Show(wordGhost, true);
                        logic.MoveWord(g.last.x + g.offset.x, g.last.y + g.offset.y);
                    }
                    else if (r == FakeTrophyResult.SignFlash) { g.kind = Kind.None; Flash(); }
                    return;
                }
                case Kind.Word:
                {
                    if (!logic.WordHeld) return;
                    FakeTrophyResult r = logic.MoveWord(g.last.x + g.offset.x, g.last.y + g.offset.y);
                    if (r == FakeTrophyResult.Snapped) StartCoroutine(Finale());
                    return;
                }
                case Kind.Trophy:
                    if (!g.moved)
                    {
                        if (Vector2.Distance(g.last, g.start) <= TapSlop) return;
                        g.moved = true;
                        if (!logic.GrabTrophy()) { g.kind = Kind.None; return; }
                        Haptics.Tap();
                        V87FakeTrophySound.Play(this, "lift", 0.35f);
                    }
                    if (!logic.TrophyHeld) return;
                    logic.MoveTrophy(g.last.x + g.offset.x, g.last.y + g.offset.y);
                    return;
            }
        }

        void End(int id, Grip g)
        {
            grips.Remove(id);
            switch (g.kind)
            {
                case Kind.Press:
                    HideRing();
                    if (logic.ReleasePress() == FakeTrophyResult.SignFlash) Flash();
                    break;
                case Kind.Word:
                    Handle(logic.ReleaseWord());
                    break;
                case Kind.Trophy:
                    if (!g.moved) Handle(logic.TapTrophy(), g.last);
                    else Handle(logic.ReleaseTrophy());
                    break;
                case Kind.Sign:
                    if (logic.TapSign() == FakeTrophyResult.SignFlash) Flash();
                    break;
                case Kind.Guy:
                    Handle(logic.TapGuy());
                    break;
            }
        }

        // ---------- Risultati dei gesti ----------

        void Handle(FakeTrophyResult r, Vector2 at = default)
        {
            switch (r)
            {
                case FakeTrophyResult.Snapped:
                    StartCoroutine(Finale());
                    break;
                case FakeTrophyResult.ReturnedToSign:
                    V87FakeTrophySound.Play(this, "whoosh", 0.4f);
                    toast.Show(ReturnLines[returnLines++ % ReturnLines.Length], 1.5f);
                    break;
                case FakeTrophyResult.Clink:
                    V87FakeTrophySound.Play(this, "clink", 0.5f);
                    Haptics.Tap();
                    FxWiggle(trophyBody, 4f, 0.3f, 40f);
                    if (clinkFx != null)
                    {
                        clinkFx.transform.position = new Vector3(at.x, at.y, clinkFx.transform.position.z);
                        clinkT = 0f;
                    }
                    toast.Show(ClinkLines[clinkLines++ % ClinkLines.Length], 1.4f);
                    break;
                case FakeTrophyResult.TrophyRejected:
                    V87FakeTrophySound.Play(this, "nope", 0.4f);
                    V87FakeTrophySound.Play(this, "thud", 0.35f);
                    FxWiggle(guyBody, 8f, 0.5f, 20f);   // scuote la testa
                    Say(RejectLines[rejectLines++ % RejectLines.Length], 1.7f);
                    break;
                case FakeTrophyResult.TrophyReturned:
                    V87FakeTrophySound.Play(this, "thud", 0.35f);
                    if (new Vector2(logic.DropX - logic.TrophyHomeX, logic.DropY - logic.TrophyHomeY).magnitude >= TapSlop)
                        toast.Show(TrophyBackLines[trophyBackLines++ % TrophyBackLines.Length], 1.4f);
                    break;
                case FakeTrophyResult.GuySigh:
                    V87FakeTrophySound.Play(this, "sigh", 0.4f);
                    FxPop(guyBody, 1.05f, 0.25f);
                    Say(SighLines[sighLines++ % SighLines.Length], 1.7f);
                    break;
            }
        }

        /// <summary>WIN lampeggia (e le lampadine tutte insieme) con un "ding ding": e basta.</summary>
        void Flash()
        {
            flashT = 0f;
            V87FakeTrophySound.Play(this, "ding", 0.4f);
            FxPop(SignFx, 1.04f, 0.2f);
            toast.Show(SignLines[signLines++ % SignLines.Length], 1.4f);
        }

        void Say(string line, float seconds) => Say(line, seconds, BubbleTip);

        void Say(string line, float seconds, Vector2 tip)
        {
            if (guyBubble != null)
            {
                Vector3 p = guy.position + (Vector3)tip;
                guyBubble.transform.position = new Vector3(p.x, p.y, guyBubble.transform.position.z);
            }
            say.Show(line, seconds);
        }

        /// <summary>Scala con cui WIN si posa sulla coppa: la sua larghezza diventa PlateWidth (ricavata dal testo disegnato).</summary>
        float SnapScale()
        {
            MeshRenderer mr = wordText != null ? wordText.GetComponent<MeshRenderer>() : null;
            float w = mr != null ? mr.bounds.size.x : 0f;
            float sx = Mathf.Max(1e-4f, Mathf.Abs(word.lossyScale.x));
            if (w < 1e-3f) return 0.4f;
            float natural = w / sx;   // larghezza a scala 1
            return Mathf.Min(MaxSnapScale, PlateWidth * Mathf.Abs(trophy.lossyScale.x) / natural);
        }

        // ---------- La soluzione ----------

        /// <summary>
        /// WIN scatta al centro della coppa ("tunk"), la coppa diventa d'oro all'istante (bagliore, stellina, "shine"), l'omino
        /// la solleva trionfante sopra la testa e saltella ("I'M A WINNER!"), applauso; poi la festa comune e Win, una volta
        /// sola. Rifatta da capo se il livello si spegne a metà.
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            grips.Clear();
            V64AFingers.Reset();
            HideRing();
            StopFx();
            HalloweenKit.Show(infoHolder, false);
            say.Hide();
            flashT = -1f;
            SetWordColor(WordColor);
            // stato di partenza della festa (anche se rifatta dopo uno spegnimento)
            RestoreWordParent();
            trophy.position = trophyHome;
            trophy.localRotation = Quaternion.identity;
            trophyVis = trophyHome;
            if (trophyBody != null) { trophyBody.localPosition = Vector3.zero; trophyBody.localRotation = Quaternion.identity; trophyBody.localScale = Vector3.one; }
            GuyPose(false);
            if (guyBody != null) { guyBody.localPosition = Vector3.zero; guyBody.localRotation = Quaternion.identity; guyBody.localScale = Vector3.one; }
            word.localRotation = Quaternion.identity;
            HalloweenKit.Show(wordGhost, true);
            Bulbs(true, true);
            Haptics.Medium();

            // 1) WIN scatta al centro della coppa
            Vector3 w0 = word.position, w1 = new Vector3(logic.ZoneX, logic.ZoneY, wordHome.z);
            Vector3 s0 = word.localScale, s1 = wordScale0 * SnapScale();
            for (float t = 0f; t < 0.14f; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / 0.14f);
                word.position = Vector3.Lerp(w0, w1, k);
                word.localScale = Vector3.Lerp(s0, s1, k);
                yield return null;
            }
            word.position = w1;
            word.localScale = s1;
            V87FakeTrophySound.Play(this, "tunk", 0.5f);

            // 2) la coppa diventa d'oro all'istante
            Gold(true);
            V87FakeTrophySound.Play(this, "shine", 0.5f);
            Haptics.Success();
            glowT = 0f;
            sparkleT = 0f;
            word.SetParent(trophyBody != null ? trophyBody : trophy, true);   // da qui WIN viaggia con la coppa
            FxPop(trophyBody, 1.15f, 0.3f);
            for (float t = 0f; t < 0.35f; t += Time.deltaTime) { TickFx(Time.deltaTime); yield return null; }

            // 3) l'omino la solleva trionfante
            GuyPose(true);
            V87FakeTrophySound.Play(this, "yay", 0.5f);
            Vector3 hands = guy.position + new Vector3(0f, LiftHandsY * Mathf.Abs(guy.lossyScale.y), 0f);
            hands.z = trophyHome.z;
            Vector3 t0 = trophy.position;
            for (float t = 0f; t < 0.4f; t += Time.deltaTime)
            {
                float k = t / 0.4f, e = 1f - (1f - k) * (1f - k);
                trophy.position = Vector3.Lerp(t0, hands, e) + Vector3.up * (0.5f * Mathf.Sin(k * Mathf.PI));
                TickFx(Time.deltaTime);
                yield return null;
            }
            trophy.position = hands;
            Say(WinnerLine, 2.4f, WinBubbleTip);
            V87FakeTrophySound.Play(this, "applause", 0.55f);
            // due saltelli con la coppa in alto
            for (int j = 0; j < 2; j++)
            {
                for (float t = 0f; t < 0.26f; t += Time.deltaTime)
                {
                    float h = 0.18f * Mathf.Sin(Mathf.Clamp01(t / 0.26f) * Mathf.PI);
                    if (guyBody != null) guyBody.localPosition = new Vector3(0f, h, 0f);
                    trophy.position = hands + new Vector3(0f, h, 0f);
                    TickFx(Time.deltaTime);
                    yield return null;
                }
            }
            if (guyBody != null) guyBody.localPosition = Vector3.zero;
            trophy.position = hands;

            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            say.Hide();
            if (logic.TryComplete()) Win(Compliment);
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked || grips.Count > 0 || logic.Busy) return;
            if (Input.GetKeyDown(KeyCode.W))
            {
                if (KeyDetach() && logic.MoveWord(logic.ZoneX, logic.ZoneY) == FakeTrophyResult.Snapped) StartCoroutine(Finale());
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (logic.PressWord()) { logic.HoldWord(0.2f, 0f); if (logic.ReleasePress() == FakeTrophyResult.SignFlash) Flash(); }
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (KeyDetach()) { logic.MoveWord(-2f, -3f); Handle(logic.ReleaseWord()); }
            }
            if (Input.GetKeyDown(KeyCode.S) && logic.TapSign() == FakeTrophyResult.SignFlash) Flash();
            if (Input.GetKeyDown(KeyCode.T)) Handle(logic.TapTrophy(), trophy.position + new Vector3(0f, 0.9f, 0f));
            if (Input.GetKeyDown(KeyCode.G) && logic.GrabTrophy()) { logic.MoveTrophy(logic.GuyX, logic.GuyY); Handle(logic.ReleaseTrophy()); }
            if (Input.GetKeyDown(KeyCode.D) && logic.GrabTrophy()) { logic.MoveTrophy(-0.2f, 0.6f); Handle(logic.ReleaseTrophy()); }
            if (Input.GetKeyDown(KeyCode.U)) Handle(logic.TapGuy());
            if (Input.GetKeyDown(KeyCode.P)) CancelAll();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }

        /// <summary>Pressione lunga simulata: WIN staccata e tenuta.</summary>
        bool KeyDetach()
        {
            if (!logic.PressWord()) return false;
            for (int i = 0; i < 10 && logic.Pressing; i++) logic.HoldWord(0.1f, 0f);
            if (!logic.WordHeld) return false;
            V87FakeTrophySound.Play(this, "pop", 0.5f);
            return true;
        }
#endif
    }

    /// <summary>Suoni sintetizzati di The Fake Trophy: "clink" metallico, "ding" dell'insegna, tic dell'anello, "pop" del distacco, "fiuu" del ritorno, coppa presa/posata, "no-no", sospiro, "tunk" dello scatto, luccichio dell'oro, "evviva", applauso.</summary>
    public static class V87FakeTrophySound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(2129);

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
        static float S(float t, float f) => Mathf.Sin(t * 2f * Mathf.PI * f);

        static AudioClip Clip(string kind)
        {
            if (cache.TryGetValue(kind, out AudioClip c) && c != null) return c;
            switch (kind)
            {
                case "clink":    // metallo vuoto: parziali non armoniche che si spengono
                    c = Make(kind, 0.45f, t => (S(t, 1870f) * 0.3f + S(t, 2930f) * 0.2f + S(t, 4410f) * 0.1f) * Mathf.Exp(-t * 9f) * Mathf.Min(1f, t * 800f));
                    break;
                case "ding":     // insegna: due "ding" rapidi
                    c = Make(kind, 0.4f, t =>
                    {
                        float a = S(t, 1320f) * Mathf.Exp(-t * 14f);
                        float u = t - 0.13f;
                        float b = u > 0f ? S(u, 1760f) * Mathf.Exp(-u * 10f) : 0f;
                        return (a + b) * 0.28f;
                    });
                    break;
                case "tick":     // un puntino dell'anello
                    c = Make(kind, 0.04f, t => S(t, 2200f) * 0.3f * Mathf.Exp(-t * 90f));
                    break;
                case "pop":      // WIN si stacca
                    c = Make(kind, 0.14f, t => N() * 0.35f * Mathf.Exp(-t * 60f) + S(t, Mathf.Lerp(400f, 1100f, t / 0.14f)) * 0.35f * Mathf.Exp(-t * 22f));
                    break;
                case "whoosh":   // WIN torna sull'insegna
                    c = Make(kind, 0.3f, t => (N() * 0.25f + S(t, Mathf.Lerp(300f, 900f, t / 0.3f)) * 0.15f) * Mathf.Sin(t / 0.3f * Mathf.PI));
                    break;
                case "lift":     // coppa presa
                    c = Make(kind, 0.08f, t => S(t, Mathf.Lerp(600f, 1000f, t / 0.08f)) * 0.35f * Mathf.Exp(-t * 30f));
                    break;
                case "thud":     // coppa di nuovo sul piedistallo
                    c = Make(kind, 0.15f, t => (N() * 0.3f + S(t, 220f) * 0.4f) * Mathf.Exp(-t * 35f) + S(t, 1500f) * 0.08f * Mathf.Exp(-t * 20f));
                    break;
                case "nope":     // "no-no" dell'omino
                    c = Make(kind, 0.4f, t =>
                    {
                        float f = t < 0.18f ? 330f : 260f;
                        float env = t < 0.18f ? Mathf.Sin(t / 0.18f * Mathf.PI) : Mathf.Sin((t - 0.2f) / 0.2f * Mathf.PI);
                        return (S(t, f) + 0.4f * S(t, 2f * f)) * 0.18f * Mathf.Max(0f, env);
                    });
                    break;
                case "sigh":     // sospiro che scende
                    c = Make(kind, 0.6f, t => (S(t, Mathf.Lerp(260f, 150f, t / 0.6f)) * 0.15f + N() * 0.06f) * Mathf.Sin(t / 0.6f * Mathf.PI));
                    break;
                case "tunk":     // WIN scatta sulla coppa
                    c = Make(kind, 0.2f, t => (N() * 0.2f + S(t, 180f) * 0.45f) * Mathf.Exp(-t * 25f) + S(t, 2400f) * 0.12f * Mathf.Exp(-t * 18f));
                    break;
                case "shine":    // la coppa diventa d'oro: glissando luccicante
                    c = Make(kind, 0.7f, t =>
                    {
                        float f = Mathf.Lerp(900f, 2600f, t / 0.7f);
                        return (S(t, f) * 0.2f + S(t, f * 1.5f) * 0.1f) * Mathf.Min(1f, t * 40f) * Mathf.Clamp01((0.7f - t) * 4f) * (0.6f + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * 18f));
                    });
                    break;
                case "yay":      // "evviva" che sale
                    c = Make(kind, 0.6f, t =>
                    {
                        float f = Mathf.Lerp(420f, 900f, t / 0.6f) + 25f * S(t, 7f);
                        return S(t, f) * 0.25f * Mathf.Min(1f, t * 30f) * Mathf.Clamp01((0.6f - t) * 6f);
                    });
                    break;
                case "applause": // applauso breve: tanti battiti di mani casuali che crescono e si spengono
                    c = MakeApplause(kind, 1.6f);
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
            AudioClip clip = AudioClip.Create("v87b_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip MakeApplause(string name, float seconds)
        {
            int n = (int)(seconds * Rate);
            var data = new float[n];
            int claps = 90;
            for (int k = 0; k < claps; k++)
            {
                // più fitti al centro: l'applauso cresce e poi cala
                float u = (float)rnd.NextDouble(), v = (float)rnd.NextDouble();
                float at = (u + v) * 0.5f * (seconds - 0.08f);
                int start = (int)(at * Rate), len = (int)(0.035f * Rate);
                float amp = 0.12f + 0.1f * (float)rnd.NextDouble();
                for (int i = 0; i < len && start + i < n; i++)
                    data[start + i] += N() * amp * Mathf.Exp(-i / (float)Rate * 120f);
            }
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                data[i] = Mathf.Clamp(data[i] * Mathf.Clamp01((seconds - t) * 3f), -1f, 1f);
            }
            AudioClip clip = AudioClip.Create("v87b_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
