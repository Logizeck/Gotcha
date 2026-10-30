using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 267 "The Unswattable Fly" — "GET RID OF THE FLY!" Una cucina: una mosca ronza a zig-zag e continua a posarsi
    /// sulla torta; sul tavolo ci sono la paletta e lo spray, sul muro una finestra a ghigliottina chiusa col gancio.
    /// False piste (nessuna azzera niente, niente sconfitta): toccare la mosca (schiva un attimo prima che il dito arrivi),
    /// la paletta (schiva sempre, e si spiaccica la torta), lo spray (si mette una mascherina antigas, la stanza si
    /// annebbia, poi starnutisce), coprire la torta tenendoci il dito (la mosca si posa sul vetro del telefono e ci
    /// passeggia), bussare sul vetro, scuotere il vetro agganciato.
    /// Indizio: ogni qualche secondo la mosca va a sbattere contro il vetro ("bonk... bonk") e pensa a un albero.
    /// Soluzione: sganciare il gancio (tocco) e alzare il vetro (trascinarlo in su, o toccarlo); oppure un solo
    /// trascinamento che parte dal gancio. Dopo un attimo la mosca esce da sola e saluta: "IT JUST WANTED TO GO HOME."
    /// Richiudere il vetro prima che esca è permesso: resta dentro e si riapre quando si vuole.
    /// Niente microfono né sensori. La logica è in Gotcha.Levels.V64.UnswattableFlyLogic (senza Unity): qui solo input e
    /// presentazione.
    /// Tasti di prova nell'Editor: K sgancia e apre la finestra, C la chiude, L gancio, 1 tocco sulla mosca,
    /// 2 paletta sulla torta, 3 spray, 4 torta coperta, B bussa sul vetro (la mosca ci sbatte subito).
    /// </summary>
    public class V70UnswattableFlyLevel : V62Level
    {
        [Header("Mosca")]
        [SerializeField] Transform fly;
        [SerializeField] SpriteRenderer flyArt;
        [SerializeField] Transform gasMask;
        [SerializeField] Transform thought;             // nuvoletta col pensiero dell'albero
        [SerializeField] AudioSource buzz;
        [SerializeField] SpeechBubble flyBubble;
        [SerializeField] Transform screenFly;           // la mosca grande che cammina sul vetro del telefono
        [SerializeField] SpriteRenderer screenFlyArt;
        [Header("Finestra")]
        [SerializeField] Transform sash;                // il vetro di sotto (scorre in su)
        [SerializeField] Collider2D sashArea;
        [SerializeField] Transform latch;               // la mezzaluna (figlia del vetro di sotto)
        [SerializeField] Collider2D latchArea;
        [SerializeField] SpriteRenderer latchGlint;
        [SerializeField] Collider2D paneArea;           // il vetro di sopra (fisso): ci si bussa
        [SerializeField] Transform windowCenter;
        [SerializeField] SpriteRenderer bonkStar;
        [SerializeField] TextMesh bonkText;
        [Header("Tavolo")]
        [SerializeField] Transform cake;
        [SerializeField] Collider2D cakeArea;
        [SerializeField] Transform cakeTop;             // dove si posa la mosca
        [SerializeField] Transform splat;
        [SerializeField] Transform swatter;
        [SerializeField] Collider2D swatterArea;
        [SerializeField] Transform swatterHead;
        [SerializeField] Transform can;
        [SerializeField] Collider2D canArea;
        [SerializeField] SmokeEmitter mist;
        [SerializeField] SpriteRenderer fog;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float TapTime = 0.45f, TapSlop = 0.25f, CoverHold = 0.45f;
        const float SashTravel = 1.1f, Sill = -1.2f;     // corsa del vetro e fondo dell'apertura (coordinate della finestra)
        const float FlySpeed = 2.8f, DodgeSpeed = 13f, SwatterThreat = 1.3f, ScreenFlyScale = 2.4f;
        const float FogMax = 0.82f;

        static readonly string[] TapLines = { "Too slow.", "It saw that coming.", "Missed. It's faster than you.", "It dodged before you even moved." };
        static readonly string[] MissLines = { "Whoosh. Nothing.", "Missed. Obviously.", "It's laughing at you." };
        static readonly string[] CakeLines = { "You got the cake. Not the fly.", "Cake: destroyed. Fly: fine.", "That cake had a family." };
        static readonly string[] SprayLines = { "It brought a gas mask. Of course.", "Safety first. For the fly.", "Now it's foggy AND buzzy." };
        static readonly string[] CoverLines = { "It landed on your screen.", "Now it's walking on YOUR glass.", "Ew. Tiny feet." };
        static readonly string[] ShooLines = { "Shoo! Off the screen!", "It flew back to the cake.", "Back to the kitchen it goes." };
        static readonly string[] RattleLines = { "It's latched.", "Rattle rattle. Locked.", "There's a latch on it." };
        static readonly string[] KnockLines = { "Knock knock.", "The fly seems interested in the glass.", "Nice view out there." };
        static readonly string[] CakeTapLines = { "The fly's favourite cake.", "Mmm. Cake. The fly agrees.", "Guarding the cake won't work." };
        static readonly string[] LeavingTapLines = { "Let it go. It's leaving!", "Don't scare it now!" };

        enum Mode { Wander, ToCake, OnCake, Bonking, Dodging, Screen, Leaving, Outside }
        enum Kind { None, Item, Sash, Latch, Cake, Other }

        sealed class Grip { public Kind kind; public Vector2 start, last; public float t0; public bool moved, rattled, covered, flyTapped; public float open0; }

        readonly UnswattableFlyLogic logic = new UnswattableFlyLogic();
        readonly Dictionary<int, Grip> grips = new Dictionary<int, Grip>();
        readonly List<int> gone = new List<int>(4);
        Camera cam;
        HwToast toast;
        HwSay say;
        HwDragger dragger;
        HwDragger.Item swatterItem, canItem;
        Mode mode;
        Vector2 flyPos, flyTarget, dodgeTo, leaveFrom, screenPos, screenGoal, jink;
        Vector3 sashHome, flyScale, cakeScale, splatScale, swatterHome, canHome, bubbleHome, thoughtScale;
        Quaternion swatterRot, canRot;
        Color screenFlyColor;
        int flyOrder, maskOrder;
        float nextHopAt, sitUntil, screenStepAt, fogAlpha;
        int hops, hopsToCake, bonkSide;
        int tapLines, missLines, cakeLines, sprayLines, coverLines, shooLines, rattleLines, knockLines, cakeTapLines, leavingTapLines;
        bool finishing, sashMoving, ready;
        Coroutine flyRun, sashRun;

        Vector2 CakeTop => cakeTop.position;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            logic.Reset();
            ResetV62();
            V64AFingers.Reset();
            grips.Clear();
            finishing = sashMoving = false;
            flyRun = sashRun = null;
            tapLines = missLines = cakeLines = sprayLines = coverLines = shooLines = rattleLines = knockLines = cakeTapLines = leavingTapLines = 0;
            bonkSide = 0;
            toast = new HwToast(infoText, infoHolder);
            say = new HwSay(flyBubble);
            if (!ready)
            {
                ready = true;
                sashHome = sash.localPosition;
                flyScale = fly.localScale;
                flyOrder = flyArt.sortingOrder;
                SpriteRenderer m = gasMask.GetComponentInChildren<SpriteRenderer>(true);
                maskOrder = m != null ? m.sortingOrder : flyOrder + 1;
                cakeScale = cake.localScale;
                splatScale = splat.localScale;
                swatterHome = swatter.position;
                swatterRot = swatter.rotation;
                canHome = can.position;
                canRot = can.rotation;
                bubbleHome = flyBubble != null ? flyBubble.transform.position : Vector3.zero;
                thoughtScale = thought.localScale;
                screenFlyColor = screenFlyArt.color;
            }
            dragger = new HwDragger(this);
            swatterItem = dragger.Add(swatter, swatterArea, 1);
            swatterItem.Home = swatterHome;
            canItem = dragger.Add(can, canArea, 1);
            canItem.Home = canHome;
            RestPose();
            flyPos = new Vector2(0.4f, 0.9f);
            mode = Mode.Wander;
            hops = 0;
            hopsToCake = 2;
            PickTarget();
            StartBuzz();
            toast.Show("Bzzzzz.", 1.6f);
        }

        /// <summary>Tutto al suo posto secondo la logica (finestra, torta, oggetti a casa, niente nebbia).</summary>
        void RestPose()
        {
            sash.localPosition = sashHome + Vector3.up * (logic.Opening * SashTravel);
            latch.localRotation = Quaternion.Euler(0f, 0f, logic.Latched ? 0f : 160f);
            HalloweenKit.SetAlpha(latchGlint, 0f);
            int sq = logic.CakeSquash;
            cake.localScale = new Vector3(cakeScale.x * (1f + 0.08f * sq), cakeScale.y * (1f - 0.2f * sq), cakeScale.z);
            HalloweenKit.Show(splat, sq > 0);
            splat.localScale = splatScale * (0.75f + 0.12f * sq);
            swatter.position = swatterHome;
            swatter.rotation = swatterRot;
            can.position = canHome;
            can.rotation = canRot;
            bool free = logic.Stage == FlyStage.Buzzing;   // mentre la mosca esce gli oggetti restano fermi
            if (swatterItem != null) swatterItem.Enabled = free;
            if (canItem != null) canItem.Enabled = free;
            if (mist != null) mist.Stop();
            fogAlpha = logic.Foggy ? FogMax : 0f;
            HalloweenKit.SetAlpha(fog, fogAlpha);
            HalloweenKit.Show(gasMask, logic.Masked);
            HalloweenKit.Show(thought, false);
            HalloweenKit.SetAlpha(bonkStar, 0f);
            if (bonkText != null) bonkText.text = "";
            HalloweenKit.Show(screenFly, logic.OnScreen);
            HalloweenKit.Show(fly, !logic.OnScreen && logic.Stage != FlyStage.Done);
            SetFlyOrder(false);
            fly.localScale = flyScale;
            fly.localRotation = Quaternion.identity;
            flyArt.color = Color.white;
            jink = Vector2.zero;
            say?.Hide();
        }

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: le animazioni si fermano con l'oggetto, si torna a uno stato valido
            CancelGrips();
            flyRun = null;
            sashRun = null;
            sashMoving = false;
            if (!ready) return;
            if (!logic.InputLocked)
            {
                // il vetro a metà corsa finisce chiuso o aperto, come si stava muovendo
                if (!logic.Latched) SetWindow(logic.WindowOpen ? 1f : 0f, false);
                RestPose();
                if (mode == Mode.Bonking || mode == Mode.Dodging) mode = Mode.Wander;
            }
        }

        void OnEnable()
        {
            if (!ready || IsFinished) return;
            if (logic.InputLocked)
            {
                // spenta mentre salutava: è comunque uscita
                finishing = true;
                HalloweenKit.Show(fly, false);
                if (buzz != null) buzz.Stop();
                if (logic.TryComplete()) Win();
                return;
            }
            StartBuzz();
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrips(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrips(); }

        /// <summary>Si lascia tutto: l'oggetto preso torna a casa, il vetro si ferma chiuso o aperto.</summary>
        void CancelGrips()
        {
            bool hadSash = false;
            foreach (Grip g in grips.Values) if (g.kind == Kind.Sash) hadSash = true;
            grips.Clear();
            if (dragger != null && dragger.Held != null) dragger.Cancel();
            if (hadSash && ready && !logic.InputLocked && !logic.Latched && !sashMoving) SetWindow(logic.WindowOpen ? 1f : 0f, false);
        }

        void StartBuzz()
        {
            if (buzz == null) return;
            buzz.clip = V70UnswattableFlySound.Clip("buzz_loop");
            buzz.loop = true;
            buzz.volume = 0.22f;
            if (!buzz.isPlaying) buzz.Play();
        }

        void Update()
        {
            toast?.Tick();
            say?.Tick();
            if (IsFinished || finishing) return;
            if (Time.timeScale <= 0f) { CancelGrips(); return; }
            float dt = Time.deltaTime;
            switch (logic.Tick(dt))
            {
                case FlyEvent.Bonk: if (mode == Mode.Wander || mode == Mode.ToCake || mode == Mode.OnCake) Run(Bonk()); break;
                case FlyEvent.ScreenWalkOver: LeaveScreen(); break;
                case FlyEvent.MaskOff: Sneeze(); break;
                case FlyEvent.NoticedWindow: Noticed(); break;
                case FlyEvent.FlewOut: StartCoroutine(GoHome()); return;
            }
            Fog(dt);
            MoveFly(dt);
            if (logic.InputLocked) { CancelGrips(); return; }
#if UNITY_EDITOR
            EditorKeys();
            if (logic.InputLocked || finishing) return;
#endif
            ReadFingers();
        }

        // ---------- La mosca ----------

        void PickTarget()
        {
            flyTarget = new Vector2(Random.Range(UnswattableFlyLogic.MinX + 0.2f, UnswattableFlyLogic.MaxX - 0.2f),
                Random.Range(UnswattableFlyLogic.MinY + 1.2f, UnswattableFlyLogic.MaxY - 0.2f));
            nextHopAt = Time.time + Random.Range(0.5f, 1.1f);
        }

        void MoveFly(float dt)
        {
            float t = Time.time;
            jink = Vector2.MoveTowards(jink, Vector2.zero, dt * 2.5f);
            Vector2 before = flyPos;
            switch (mode)
            {
                case Mode.Wander:
                    flyPos = Vector2.MoveTowards(flyPos, flyTarget, FlySpeed * dt);
                    if (Vector2.Distance(flyPos, flyTarget) < 0.05f || t >= nextHopAt)
                    {
                        if (++hops >= hopsToCake) { mode = Mode.ToCake; hops = 0; hopsToCake = Random.Range(3, 6); }
                        else PickTarget();
                    }
                    break;
                case Mode.ToCake:
                    flyPos = Vector2.MoveTowards(flyPos, CakeTop, FlySpeed * dt);
                    if (Vector2.Distance(flyPos, CakeTop) < 0.04f) { mode = Mode.OnCake; sitUntil = t + Random.Range(1.6f, 3.2f); }
                    break;
                case Mode.OnCake:
                    flyPos = CakeTop;
                    if (t >= sitUntil) { mode = Mode.Wander; PickTarget(); }
                    break;
                case Mode.Dodging:
                    flyPos = Vector2.MoveTowards(flyPos, dodgeTo, DodgeSpeed * dt);
                    if (Vector2.Distance(flyPos, dodgeTo) < 0.03f) { mode = Mode.Wander; PickTarget(); nextHopAt = t + Random.Range(0.3f, 0.7f); }
                    break;
                case Mode.Leaving:
                    LeavePath();
                    break;
                case Mode.Screen:
                    ScreenWalk(dt);
                    break;
            }
            if (mode == Mode.Outside || mode == Mode.Bonking) { PoseFly(before, false); return; }
            PoseFly(before, mode == Mode.OnCake);
        }

        /// <summary>Posa del fotogramma: ronzio (tremolio), verso del muso, volume del ronzio.</summary>
        void PoseFly(Vector2 before, bool sitting)
        {
            float t = Time.time;
            Vector2 buzzJitter = sitting ? Vector2.zero : new Vector2(Mathf.Sin(t * 23f) * 0.03f + Mathf.Sin(t * 7.1f) * 0.05f, Mathf.Sin(t * 19f) * 0.04f + Mathf.Cos(t * 5.3f) * 0.06f);
            fly.position = new Vector3(flyPos.x + buzzJitter.x + jink.x, flyPos.y + buzzJitter.y + jink.y, fly.position.z);
            float vx = flyPos.x - before.x;
            if (Mathf.Abs(vx) > 0.002f) fly.localScale = new Vector3(Mathf.Sign(vx) * Mathf.Abs(fly.localScale.x), fly.localScale.y, fly.localScale.z);
            fly.localRotation = Quaternion.Euler(0f, 0f, sitting ? Mathf.Sin(t * 9f) * 6f : Mathf.Sin(t * 13f) * 8f);
            if (buzz != null)
            {
                buzz.volume = Mathf.MoveTowards(buzz.volume, mode == Mode.Screen ? 0.32f : sitting ? 0.06f : 0.22f, Time.deltaTime);
                buzz.pitch = mode == Mode.Dodging ? 1.35f : sitting ? 0.9f : 1f + Mathf.Sin(t * 3f) * 0.05f;
            }
        }

        /// <summary>Un dito (o la paletta) arriva in "from": la mosca scatta via prima che arrivi.</summary>
        void DodgeFrom(Vector2 from)
        {
            if (mode == Mode.Screen || mode == Mode.Outside) return;
            if (mode == Mode.Leaving)
            {
                // sta uscendo: uno scarto di lato e continua per la sua strada
                Vector2 d = (Vector2)fly.position - from;
                jink = (d.sqrMagnitude > 0.001f ? d.normalized : Vector2.up) * 0.45f;
                return;
            }
            StopFlyRun();
            UnswattableFlyLogic.Dodge(from.x, from.y, flyPos.x, flyPos.y, out float x, out float y);
            dodgeTo = new Vector2(x, y);
            mode = Mode.Dodging;
            HalloweenKit.Show(thought, false);
            V70UnswattableFlySound.Play(this, "zip", 0.5f);
        }

        void Run(IEnumerator routine)
        {
            StopFlyRun();
            flyRun = StartCoroutine(routine);
        }

        void StopFlyRun()
        {
            if (flyRun != null) StopCoroutine(flyRun);
            flyRun = null;
            if (mode == Mode.Bonking) mode = Mode.Wander;
        }

        // ---------- L'indizio: bonk... bonk ----------

        IEnumerator Bonk()
        {
            mode = Mode.Bonking;
            Vector2 glass = (Vector2)windowCenter.position + (bonkSide++ % 2 == 0 ? new Vector2(0.3f, -0.55f) : new Vector2(-0.35f, 0.6f));
            Vector2 back = glass + new Vector2(0.55f, -0.35f);
            // ci arriva ronzando
            while (Vector2.Distance(flyPos, back) > 0.05f)
            {
                flyPos = Vector2.MoveTowards(flyPos, back, FlySpeed * 1.2f * Time.deltaTime);
                yield return null;
            }
            for (int i = 0; i < 2; i++)
            {
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.18f) { flyPos = Vector2.Lerp(back, glass, k * k); yield return null; }
                flyPos = glass;
                V70UnswattableFlySound.Play(this, "bonk", 0.7f);
                StartCoroutine(BonkMark(glass, i == 0 ? "bonk..." : "bonk."));
                StartCoroutine(HalloweenKit.Pop(fly, 0.8f, 0.15f));
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { flyPos = Vector2.Lerp(glass, back, Mathf.Sin(k * Mathf.PI * 0.5f)); yield return null; }
                yield return new WaitForSeconds(0.25f);
            }
            // ...e pensa a casa sua
            yield return Think(1.6f);
            mode = Mode.Wander;
            PickTarget();
            flyRun = null;
        }

        IEnumerator Think(float seconds)
        {
            HalloweenKit.Show(thought, true);
            for (float k = 0f; k < seconds; k += Time.deltaTime)
            {
                thought.position = new Vector3(Mathf.Min(fly.position.x + 0.65f, 1.9f), Mathf.Min(fly.position.y + 0.62f, 2.3f), thought.position.z);   // sotto la riga dell'obiettivo
                thought.localScale = thoughtScale * Mathf.Min(1f, k / 0.15f);
                yield return null;
            }
            HalloweenKit.Show(thought, false);
            thought.localScale = thoughtScale;
        }

        IEnumerator BonkMark(Vector2 at, string word)
        {
            bonkStar.transform.position = new Vector3(at.x + 0.12f, at.y + 0.1f, bonkStar.transform.position.z);
            if (bonkText != null)
            {
                bonkText.text = word;
                bonkText.transform.position = new Vector3(at.x - 0.15f, at.y + 0.42f, bonkText.transform.position.z);
                bonkText.transform.localScale = Vector3.one;
                StartCoroutine(HalloweenKit.Pop(bonkText.transform, 1.3f, 0.2f));
            }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
            {
                HalloweenKit.SetAlpha(bonkStar, 1f - k);
                bonkStar.transform.localRotation = Quaternion.Euler(0f, 0f, k * 90f);
                if (bonkText != null) bonkText.color = new Color(bonkText.color.r, bonkText.color.g, bonkText.color.b, 1f - k * k);
                yield return null;
            }
            HalloweenKit.SetAlpha(bonkStar, 0f);
            if (bonkText != null) bonkText.text = "";
        }

        // ---------- Dita ----------

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            foreach (V64AFinger f in fingers)
            {
                if (f.began) grips[f.id] = Begin(f.world);
                if (!grips.TryGetValue(f.id, out Grip g)) continue;
                g.last = f.world;
                if (Vector2.Distance(f.world, g.start) > TapSlop) g.moved = true;
                Drag(g);
                if (f.ended) End(f.id, g);
                if (finishing || logic.InputLocked) { grips.Clear(); return; }
            }
            if (fingers.Count == 0 && grips.Count > 0)
            {
                gone.Clear();
                foreach (int id in grips.Keys) gone.Add(id);
                foreach (int id in gone) End(id, grips[id]);
            }
        }

        bool Holding(Kind kind)
        {
            foreach (Grip g in grips.Values) if (g.kind == kind) return true;
            return false;
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, t0 = Time.time, kind = Kind.Other, open0 = logic.Opening };
            if (finishing || logic.InputLocked) { g.kind = Kind.None; return g; }
            // la mosca sente arrivare il dito: scappa prima che tocchi (ovunque sia il dito, se è vicino)
            bool near = mode != Mode.Screen && UnswattableFlyLogic.Threatens(w.x, w.y, fly.position.x, fly.position.y);
            if (mode == Mode.Screen && Vector2.Distance(w, screenPos) < 1.3f) { ShooFromScreen(); g.kind = Kind.None; return g; }
            if (!Holding(Kind.Latch) && !Holding(Kind.Sash) && !sashMoving && latchArea.OverlapPoint(w)) { g.kind = Kind.Latch; if (near) DodgeFrom(w); return g; }
            if (!Holding(Kind.Sash) && !Holding(Kind.Latch) && !sashMoving && sashArea.OverlapPoint(w)) { g.kind = Kind.Sash; if (near) DodgeFrom(w); return g; }
            if (dragger.Held == null && logic.Stage == FlyStage.Buzzing && dragger.TryPick(w) != null)
            {
                g.kind = Kind.Item;
                if (dragger.Held == swatterItem) swatter.rotation = Quaternion.Euler(0f, 0f, -15f);
                if (near) DodgeFrom(w);
                return g;
            }
            // la torta (anche con la mosca sopra: scappa, ma tenendo il dito la si "copre")
            if (logic.Stage == FlyStage.Buzzing && cakeArea.OverlapPoint(w) && !Holding(Kind.Cake))
            {
                g.kind = Kind.Cake;
                if (near) { TapFly(w); g.flyTapped = true; }
                return g;
            }
            if (near) { TapFly(w); g.kind = Kind.None; return g; }
            return g;
        }

        void Drag(Grip g)
        {
            switch (g.kind)
            {
                case Kind.Item:
                    dragger.MoveHeld(g.last);
                    // la paletta che si avvicina: la mosca scappa (sempre)
                    if (dragger.Held == swatterItem && swatterHead != null && mode != Mode.Dodging &&
                        UnswattableFlyLogic.Threatens(swatterHead.position.x, swatterHead.position.y, fly.position.x, fly.position.y, SwatterThreat))
                        DodgeFrom(swatterHead.position);
                    break;
                case Kind.Latch:
                    // un trascinamento in su che parte dal gancio: lo sgancia e diventa una presa del vetro
                    if (g.last.y - g.start.y > TapSlop)
                    {
                        if (logic.Latched) ToggleLatch();
                        g.kind = Kind.Sash;
                        g.open0 = logic.Opening;
                        DragSash(g);
                    }
                    break;
                case Kind.Sash:
                    DragSash(g);
                    break;
                case Kind.Cake:
                    if (!g.covered && !g.moved && Time.time - g.t0 >= CoverHold) { g.covered = true; CoverCake(g.start); }
                    break;
            }
        }

        void End(int id, Grip g)
        {
            grips.Remove(id);
            bool tap = !g.moved && Time.time - g.t0 <= TapTime;
            switch (g.kind)
            {
                case Kind.Item: DropItem(tap, g.last); break;
                case Kind.Latch: if (tap) ToggleLatch(); break;
                case Kind.Sash: ReleaseSash(g, tap); break;
                case Kind.Cake: if (tap && !g.covered && !g.flyTapped) { Sfx.Play(this, "tick", 0.4f); toast.Show(CakeTapLines[cakeTapLines++ % CakeTapLines.Length], 1.6f); } break;
                case Kind.Other: if (tap && paneArea.OverlapPoint(g.start)) KnockGlass(); break;
            }
        }

        // ---------- False piste ----------

        void TapFly(Vector2 w)
        {
            if (logic.TapFly() == 0) return;
            DodgeFrom(w);
            Haptics.Tap();
            if (mode == Mode.Leaving) { toast.Show(LeavingTapLines[leavingTapLines++ % LeavingTapLines.Length], 1.4f); return; }
            V70UnswattableFlySound.Play(this, "taunt", 0.45f);
            toast.Show(TapLines[tapLines++ % TapLines.Length], 1.5f);
        }

        void DropItem(bool tap, Vector2 at)
        {
            HwDragger.Item it = dragger.Release();
            if (it == null) return;
            if (logic.Stage != FlyStage.Buzzing) { ItemHome(it); return; }
            if (it == swatterItem) StartCoroutine(Slap(tap));
            else if (it == canItem) StartCoroutine(SprayRoutine());
        }

        void ItemHome(HwDragger.Item it)
        {
            if (it == swatterItem) swatter.rotation = swatterRot;
            if (it == canItem) can.rotation = canRot;
            dragger.SendHome(it, 0.3f);
            it.Enabled = true;
        }

        /// <summary>Colpo di paletta dove si trova la testa (un tocco: la paletta va da sola verso la mosca). Schiva sempre.</summary>
        IEnumerator Slap(bool tap)
        {
            swatterItem.Enabled = false;
            if (tap)
            {
                // un tocco: la paletta parte verso la mosca (che naturalmente se ne va)
                swatter.rotation = Quaternion.Euler(0f, 0f, -15f);
                Vector3 off = swatter.position - swatterHead.position;
                Vector3 to = new Vector3(fly.position.x, fly.position.y, swatter.position.z) + off;
                to = (Vector3)HalloweenKit.ClampSafe(to) + Vector3.forward * swatter.position.z;
                Vector2 headTo = to - off;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.18f)
                {
                    swatter.position = Vector3.Lerp(swatter.position, to, k);
                    if (mode != Mode.Dodging && UnswattableFlyLogic.Threatens(swatterHead.position.x, swatterHead.position.y, fly.position.x, fly.position.y, SwatterThreat))
                        DodgeFrom(headTo);
                    yield return null;
                }
            }
            Vector2 head = swatterHead.position;
            if (UnswattableFlyLogic.Threatens(head.x, head.y, fly.position.x, fly.position.y, SwatterThreat)) DodgeFrom(head);
            // giù!
            Quaternion r0 = swatter.rotation;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.08f) { swatter.rotation = r0 * Quaternion.Euler(0f, 0f, 25f * (1f - k)); yield return null; }
            swatter.rotation = r0;
            bool onCake = cakeArea.OverlapPoint(head) || Vector2.Distance(head, CakeTop) < 0.75f;
            SwatResult r = logic.Swat(onCake);
            Haptics.Medium();
            StartCoroutine(HalloweenKit.Shake(swatter, 0.05f, 0.15f));
            if (r == SwatResult.SmashedCake)
            {
                V70UnswattableFlySound.Play(this, "splat", 0.85f);
                SquashCake();
                toast.Show(CakeLines[cakeLines++ % CakeLines.Length], 1.8f);
            }
            else if (r == SwatResult.Miss)
            {
                V70UnswattableFlySound.Play(this, "thwack", 0.75f);
                toast.Show(MissLines[missLines++ % MissLines.Length], 1.5f);
            }
            yield return new WaitForSeconds(0.35f);
            ItemHome(swatterItem);
        }

        void SquashCake()
        {
            int sq = logic.CakeSquash;
            Vector3 to = new Vector3(cakeScale.x * (1f + 0.08f * sq), cakeScale.y * (1f - 0.2f * sq), cakeScale.z);
            cake.localScale = new Vector3(to.x * 1.15f, to.y * 0.6f, to.z);
            StartCoroutine(HalloweenKit.ScaleTo(cake, to, 0.2f));
            HalloweenKit.Show(splat, true);
            splat.localScale = splatScale * 0.3f;
            StartCoroutine(HalloweenKit.ScaleTo(splat, splatScale * (0.75f + 0.12f * sq), 0.15f));
        }

        IEnumerator SprayRoutine()
        {
            canItem.Enabled = false;
            // la bomboletta si gira verso la mosca e spruzza
            Vector2 d = (Vector2)fly.position - (Vector2)can.position;
            float ang = Mathf.Clamp(-Mathf.Atan2(d.x, Mathf.Max(0.2f, d.y)) * Mathf.Rad2Deg, -60f, 60f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.12f) { can.rotation = Quaternion.Euler(0f, 0f, ang * k); yield return null; }
            if (logic.Spray() == 0) { ItemHome(canItem); yield break; }
            V70UnswattableFlySound.Play(this, "hiss", 0.7f);
            Haptics.Light();
            if (mist != null) StartCoroutine(HalloweenKit.Burst(mist, 0.6f));
            yield return new WaitForSeconds(0.25f);
            // ...e la mosca tira fuori la mascherina
            HalloweenKit.Show(gasMask, true);
            StartCoroutine(HalloweenKit.Pop(gasMask, 1.4f, 0.25f));
            V70UnswattableFlySound.Play(this, "snap", 0.5f);
            toast.Show(SprayLines[sprayLines++ % SprayLines.Length], 1.8f);
            yield return new WaitForSeconds(0.4f);
            ItemHome(canItem);
        }

        void Fog(float dt)
        {
            float target = logic.Foggy ? FogMax * Mathf.Clamp01(logic.FogLeft / 0.8f) : 0f;
            fogAlpha = Mathf.MoveTowards(fogAlpha, target, dt * 2.5f);
            HalloweenKit.SetAlpha(fog, fogAlpha);
        }

        void Sneeze()
        {
            HalloweenKit.Show(gasMask, false);
            if (mode == Mode.Outside || mode == Mode.Screen) return;
            V70UnswattableFlySound.Play(this, "sneeze", 0.6f);
            StartCoroutine(HalloweenKit.Pop(fly, 1.3f, 0.2f));
            BubbleNearFly("Achoo!", 1.2f);
        }

        void BubbleNearFly(string text, float seconds)
        {
            if (flyBubble == null) return;
            Vector2 p = HalloweenKit.ClampSafe((Vector2)fly.position + new Vector2(fly.position.x > 0.8f ? -0.7f : 0.7f, 0.6f));
            flyBubble.transform.position = new Vector3(p.x, Mathf.Min(p.y, 2.6f), bubbleHome.z);
            say.Show(text, seconds);
        }

        void CoverCake(Vector2 at)
        {
            if (logic.CoverCake() == 0) return;
            StopFlyRun();
            mode = Mode.Screen;
            HalloweenKit.Show(thought, false);
            V70UnswattableFlySound.Play(this, "zip", 0.6f);
            Haptics.Light();
            // la mosca "vola verso di te" e si posa sul vetro, accanto al dito
            HalloweenKit.Show(fly, false);
            screenPos = HalloweenKit.ClampSafe(at + new Vector2(at.x > 0.5f ? -0.9f : 0.9f, 0.8f));
            screenGoal = screenPos;
            screenStepAt = Time.time + 0.35f;
            HalloweenKit.Show(screenFly, true);
            screenFly.position = new Vector3(screenPos.x, screenPos.y, screenFly.position.z);
            screenFly.localScale = Vector3.one * 0.5f;
            screenFlyArt.color = screenFlyColor;
            StartCoroutine(HalloweenKit.ScaleTo(screenFly, Vector3.one * ScreenFlyScale, 0.2f));
            toast.Show(CoverLines[coverLines++ % CoverLines.Length], 1.8f);
        }

        /// <summary>Due passetti alla volta sul vetro del telefono, girandosi qua e là.</summary>
        void ScreenWalk(float dt)
        {
            float t = Time.time;
            if (t >= screenStepAt)
            {
                screenStepAt = t + Random.Range(0.35f, 0.7f);
                screenGoal = HalloweenKit.ClampSafe(screenPos + Random.insideUnitCircle * 0.6f);
            }
            Vector2 before = screenPos;
            screenPos = Vector2.MoveTowards(screenPos, screenGoal, 1.1f * dt);
            screenFly.position = new Vector3(screenPos.x, screenPos.y, screenFly.position.z);
            Vector2 v = screenPos - before;
            if (v.sqrMagnitude > 1e-6f) screenFly.localRotation = Quaternion.Slerp(screenFly.localRotation, Quaternion.Euler(0f, 0f, Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg), dt * 8f);
            if (v.sqrMagnitude > 1e-6f && Mathf.Repeat(t, 0.12f) < 0.06f) screenFly.localScale = Vector3.one * ScreenFlyScale * 1.03f;
            else screenFly.localScale = Vector3.one * ScreenFlyScale;
        }

        void ShooFromScreen()
        {
            if (logic.TapFly() == 0) return;
            Haptics.Tap();
            V70UnswattableFlySound.Play(this, "taunt", 0.45f);
            toast.Show(ShooLines[shooLines++ % ShooLines.Length], 1.5f);
        }

        /// <summary>Finita la passeggiata (o scacciata): rimpicciolisce "allontanandosi" e torna a volare in cucina.</summary>
        void LeaveScreen()
        {
            if (mode != Mode.Screen) return;
            HalloweenKit.Show(screenFly, false);
            HalloweenKit.Show(fly, true);
            flyPos = new Vector2(Mathf.Clamp(screenPos.x, UnswattableFlyLogic.MinX, UnswattableFlyLogic.MaxX), Mathf.Clamp(screenPos.y, UnswattableFlyLogic.MinY, UnswattableFlyLogic.MaxY));
            fly.localScale = flyScale * 2f;
            StartCoroutine(HalloweenKit.ScaleTo(fly, flyScale, 0.25f));
            V70UnswattableFlySound.Play(this, "zip", 0.5f);
            if (logic.Stage == FlyStage.Leaving) { StartLeaving(); return; }
            mode = Mode.ToCake;
        }

        void KnockGlass()
        {
            if (logic.KnockGlass() == 0) return;
            V70UnswattableFlySound.Play(this, "knock", 0.6f);
            toast.Show(KnockLines[knockLines++ % KnockLines.Length], 1.5f);
        }

        // ---------- La finestra ----------

        void ToggleLatch()
        {
            switch (logic.ToggleLatch())
            {
                case LatchResult.Unlatched:
                    V70UnswattableFlySound.Play(this, "click", 0.7f);
                    Haptics.Tap();
                    StartCoroutine(TurnLatch(160f));
                    toast.Show("Click.", 1.0f);
                    break;
                case LatchResult.Latched:
                    V70UnswattableFlySound.Play(this, "click", 0.6f);
                    StartCoroutine(TurnLatch(0f));
                    toast.Show("Click. Locked again.", 1.2f);
                    break;
                case LatchResult.Blocked:
                    Sfx.Play(this, "tick", 0.4f);
                    toast.Show("Close it first.", 1.2f);
                    break;
            }
        }

        IEnumerator TurnLatch(float to)
        {
            float from = latch.localEulerAngles.z;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f)
            {
                latch.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(from, to, k));
                yield return null;
            }
            latch.localRotation = Quaternion.Euler(0f, 0f, to);
        }

        void DragSash(Grip g)
        {
            float dy = g.last.y - g.start.y;
            if (logic.Latched)
            {
                if (dy > TapSlop * 0.6f && !g.rattled) { g.rattled = true; RattleSash(); }
                return;
            }
            SetWindow(g.open0 + dy / SashTravel, true);
        }

        void ReleaseSash(Grip g, bool tap)
        {
            if (logic.Latched)
            {
                if (tap || !g.rattled) RattleSash();
                return;
            }
            if (tap)
            {
                if (!logic.WindowOpen) MoveSash(1f);
                else if (logic.Opening < 0.99f) MoveSash(1f);
                else { Sfx.Play(this, "tick", 0.4f); toast.Show("Fresh air!", 1.2f); }
                return;
            }
            bool up = g.last.y >= g.start.y;
            float target = up ? (logic.Opening > 0.15f ? 1f : 0f) : (logic.Opening < 0.85f ? 0f : 1f);
            MoveSash(target);
        }

        void RattleSash()
        {
            if (!logic.Rattle()) return;
            V70UnswattableFlySound.Play(this, "rattle", 0.7f);
            Haptics.Light();
            StartCoroutine(HalloweenKit.Shake(sash, 0.035f, 0.25f));
            StartCoroutine(HalloweenKit.Wiggle(latch, 10f, 0.35f, 30f));
            StartCoroutine(LatchGlint());
            toast.Show(RattleLines[rattleLines++ % RattleLines.Length], 1.5f);
        }

        IEnumerator LatchGlint()
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                HalloweenKit.SetAlpha(latchGlint, Mathf.Sin(k * Mathf.PI) * 0.95f);
                latchGlint.transform.localRotation = Quaternion.Euler(0f, 0f, k * 120f);
                yield return null;
            }
            HalloweenKit.SetAlpha(latchGlint, 0f);
        }

        void MoveSash(float to)
        {
            if (sashRun != null) StopCoroutine(sashRun);
            sashRun = StartCoroutine(SashTo(to));
        }

        IEnumerator SashTo(float to)
        {
            sashMoving = true;
            float from = logic.Opening;
            if (Mathf.Abs(to - from) > 0.05f) V70UnswattableFlySound.Play(this, "slide", 0.6f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                SetWindow(Mathf.Lerp(from, to, k * (2f - k)), true);
                if (logic.InputLocked) break;
                yield return null;
            }
            if (!logic.InputLocked) SetWindow(to, true);
            if (to <= 0f && from > 0.05f) V70UnswattableFlySound.Play(this, "thud", 0.5f);
            sashMoving = false;
            sashRun = null;
        }

        /// <summary>Porta il vetro a "o" (0..1) attraverso la logica e reagisce a cosa fa la mosca.</summary>
        void SetWindow(float o, bool react)
        {
            float progress = logic.LeaveProgress;
            WindowEvent ev = logic.SetOpening(o);
            sash.localPosition = sashHome + Vector3.up * (logic.Opening * SashTravel);
            if (!react) { if (ev == WindowEvent.LeaveCancelled) CancelLeaving(false, progress); else if (ev == WindowEvent.StartedLeaving) StartLeaving(); return; }
            switch (ev)
            {
                case WindowEvent.StartedLeaving: StartLeaving(); break;
                case WindowEvent.LeaveCancelled: CancelLeaving(true, progress); break;
            }
        }

        // ---------- La soluzione: la finestra aperta ----------

        /// <summary>La finestra è aperta: la mosca si ferma un attimo, ci pensa, poi va verso il varco.</summary>
        void StartLeaving()
        {
            if (mode == Mode.Screen)
            {
                // era sul vetro del telefono: ne scende al volo
                HalloweenKit.Show(screenFly, false);
                HalloweenKit.Show(fly, true);
                flyPos = new Vector2(Mathf.Clamp(screenPos.x, UnswattableFlyLogic.MinX, UnswattableFlyLogic.MaxX), Mathf.Clamp(screenPos.y, UnswattableFlyLogic.MinY, UnswattableFlyLogic.MaxY));
                fly.localScale = flyScale;
            }
            StopFlyRun();
            mode = Mode.Leaving;
            leaveFrom = flyPos;
            HalloweenKit.Show(thought, false);
            if (dragger != null && dragger.Held != null) dragger.Cancel();
            if (swatterItem != null) swatterItem.Enabled = false;
            if (canItem != null) canItem.Enabled = false;
            if (!isActiveAndEnabled) return;   // (spento: ci pensa Update quando torna)
            V70UnswattableFlySound.Play(this, "breeze", 0.5f);
            StartCoroutine(Think(UnswattableFlyLogic.NoticeSeconds));
        }

        void Noticed()
        {
            if (mode != Mode.Leaving) return;
            leaveFrom = flyPos;
            V70UnswattableFlySound.Play(this, "zip", 0.5f);
        }

        /// <summary>Il varco sotto il vetro alzato (in coordinate del mondo): ci passa la mosca.</summary>
        Vector2 Gap()
        {
            Vector2 c = windowCenter.position;
            return new Vector2(c.x + 0.15f, c.y + Sill + Mathf.Max(0.12f, logic.Opening * SashTravel * 0.5f));
        }

        void LeavePath()
        {
            float nf = UnswattableFlyLogic.NoticeSeconds / UnswattableFlyLogic.LeaveSeconds;
            float p = logic.LeaveProgress;
            if (p < nf)
            {
                // si ferma a mezz'aria e guarda la finestra
                flyPos = Vector2.MoveTowards(flyPos, leaveFrom, Time.deltaTime);
                return;
            }
            float u = Mathf.Clamp01((p - nf) / (1f - nf));
            float e = u * u * (3f - 2f * u);
            Vector2 gap = Gap();
            Vector2 mid = (leaveFrom + gap) * 0.5f + new Vector2(0.4f, 0.6f);
            Vector2 a = Vector2.Lerp(leaveFrom, mid, e), b = Vector2.Lerp(mid, gap, e);
            flyPos = Vector2.Lerp(a, b, e) + new Vector2(0f, Mathf.Sin(u * 18f) * 0.06f * (1f - u));
        }

        /// <summary>Richiusa prima che uscisse: se era già sul varco sbatte contro il vetro; resta dentro, nessun blocco.</summary>
        void CancelLeaving(bool react, float progress)
        {
            if (swatterItem != null) swatterItem.Enabled = true;
            if (canItem != null) canItem.Enabled = true;
            HalloweenKit.Show(thought, false);
            mode = Mode.Wander;
            PickTarget();
            if (!react) return;
            if (progress > 0.55f)
            {
                V70UnswattableFlySound.Play(this, "bonk", 0.8f);
                StartCoroutine(BonkMark(flyPos, "BONK!"));
                DodgeFrom(Gap() + Vector2.down * 0.4f);
                toast.Show("Hey! It was leaving!", 1.8f);
            }
            else toast.Show("It was about to leave...", 1.6f);
        }

        /// <summary>È passata dal varco: vola verso l'albero, rimpicciolisce, saluta. Vittoria.</summary>
        IEnumerator GoHome()
        {
            finishing = true;
            mode = Mode.Outside;
            grips.Clear();
            if (dragger.Held != null) dragger.Cancel();
            StopFlyRun();
            mode = Mode.Outside;
            HalloweenKit.Show(gasMask, false);
            HalloweenKit.Show(screenFly, false);
            HalloweenKit.Show(fly, true);
            SetFlyOrder(true);
            Vector2 from = flyPos;
            Vector2 tree = (Vector2)windowCenter.position + new Vector2(-0.45f, 0.15f);
            Vector3 s0 = fly.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                float e = k * (2f - k);
                Vector2 before = flyPos;
                flyPos = Vector2.Lerp(from, tree, e) + new Vector2(0f, Mathf.Sin(k * Mathf.PI) * 0.35f);
                float sc = Mathf.Lerp(1f, 0.7f, e);
                fly.localScale = new Vector3(Mathf.Sign(flyPos.x - before.x + 1e-5f) * Mathf.Abs(flyScale.x) * sc, flyScale.y * sc, flyScale.z);
                fly.position = new Vector3(flyPos.x, flyPos.y, fly.position.z);
                if (buzz != null) buzz.volume = Mathf.Lerp(0.22f, 0.04f, e);
                yield return null;
            }
            // si gira e saluta
            fly.localScale = new Vector3(Mathf.Abs(flyScale.x) * 0.7f, flyScale.y * 0.7f, flyScale.z);
            if (flyBubble != null) flyBubble.transform.position = new Vector3(windowCenter.position.x + 1.35f, windowCenter.position.y + 0.75f, bubbleHome.z);
            say.Show("bzzz... bye!", 2.6f);
            V70UnswattableFlySound.Play(this, "bye", 0.6f);
            StartCoroutine(HalloweenKit.Wiggle(fly, 25f, 1.1f, 22f));
            toast.Show("IT JUST WANTED TO GO HOME.", 2.6f);
            yield return new WaitForSeconds(0.4f);
            if (buzz != null) buzz.Stop();
            yield return Celebrate(1.3f);
            yield return HalloweenKit.Fade(flyArt, 0f, 0.3f);
            HalloweenKit.Show(fly, false);
            flyArt.color = Color.white;
            if (logic.TryComplete()) Win();
        }

        /// <summary>Dentro la stanza la mosca sta davanti a tutto; uscita, sta dietro ai vetri e alla cornice.</summary>
        void SetFlyOrder(bool outside)
        {
            flyArt.sortingOrder = outside ? -14 : flyOrder;
            foreach (SpriteRenderer sr in gasMask.GetComponentsInChildren<SpriteRenderer>(true)) sr.sortingOrder = outside ? -14 : maskOrder;
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (Input.GetKeyDown(KeyCode.K)) { if (logic.Latched) ToggleLatch(); MoveSash(1f); }
            if (Input.GetKeyDown(KeyCode.C) && !logic.Latched) MoveSash(0f);
            if (Input.GetKeyDown(KeyCode.L)) ToggleLatch();
            if (Input.GetKeyDown(KeyCode.Alpha1)) TapFly((Vector2)fly.position + new Vector2(0.3f, -0.2f));
            if (Input.GetKeyDown(KeyCode.Alpha2) && swatterItem.Enabled && logic.Stage == FlyStage.Buzzing)
            {
                swatter.rotation = Quaternion.Euler(0f, 0f, -15f);
                swatter.position += (Vector3)(CakeTop - (Vector2)swatterHead.position);
                StartCoroutine(Slap(false));
            }
            if (Input.GetKeyDown(KeyCode.Alpha3) && canItem.Enabled && logic.Stage == FlyStage.Buzzing) StartCoroutine(SprayRoutine());
            if (Input.GetKeyDown(KeyCode.Alpha4)) CoverCake(cake.position);
            if (Input.GetKeyDown(KeyCode.B)) KnockGlass();
        }
#endif
    }

    /// <summary>Suoni sintetizzati del livello 267: ronzio, bonk sul vetro, paletta, torta spiaccicata, spray, starnuto, gancio, vetro che scorre.</summary>
    public static class V70UnswattableFlySound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(70);

        public static void Play(Component owner, string kind, float volume = 0.6f)
        {
            if (owner == null) return;
            if (owner is Behaviour b && !b.isActiveAndEnabled) return;
            AudioClip c = Clip(kind);
            if (c == null) { Sfx.Play(owner, kind, volume); return; }
            AudioSource src = owner.GetComponent<AudioSource>();
            if (src == null) { src = owner.gameObject.AddComponent<AudioSource>(); src.playOnAwake = false; }
            src.PlayOneShot(c, volume);
        }

        static float N() => (float)rnd.NextDouble() * 2f - 1f;

        public static AudioClip Clip(string kind)
        {
            if (cache.TryGetValue(kind, out AudioClip c) && c != null) return c;
            switch (kind)
            {
                case "buzz_loop":   // ronzio della mosca: dente di sega a ~190 Hz con tremolio d'ali (dura un numero intero di cicli: si ripete senza scatti)
                    c = Make(kind, 1f, t =>
                    {
                        float f = 190f + 12f * Mathf.Sin(t * 2f * Mathf.PI * 3f);
                        float ph = (t * f) % 1f;
                        return (ph * 2f - 1f) * 0.32f * (0.75f + 0.25f * Mathf.Sin(t * 2f * Mathf.PI * 40f));
                    });
                    break;
                case "bonk":     // la mosca contro il vetro: "tic" secco con un po' di vetro
                    c = Make(kind, 0.16f, t => (Mathf.Sin(t * 2f * Mathf.PI * 2400f) * 0.35f + Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(600f, 300f, t / 0.16f)) * 0.5f) * Mathf.Exp(-t * 38f));
                    break;
                case "zip":      // scatto veloce: ronzio che sale
                    c = Make(kind, 0.22f, t => Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(220f, 420f, t / 0.22f))) * 0.22f * Mathf.Sin(t / 0.22f * Mathf.PI));
                    break;
                case "taunt":    // "nyeh nyeh": due ronzii acuti
                    c = Make(kind, 0.4f, t => Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * (t < 0.2f ? 330f : 280f))) * 0.18f * Mathf.Sin((t % 0.2f) / 0.2f * Mathf.PI));
                    break;
                case "thwack":   // paletta sul tavolo
                    c = Make(kind, 0.2f, t => (N() * 0.6f + Mathf.Sin(t * 2f * Mathf.PI * 150f) * 0.5f) * Mathf.Exp(-t * 30f));
                    break;
                case "splat":    // paletta nella torta: schiaffo molle
                    {
                        float lp = 0f;
                        c = Make(kind, 0.35f, t => { lp += (N() - lp) * 0.25f; return (lp * 1.4f + Mathf.Sin(t * 2f * Mathf.PI * 90f) * 0.4f) * Mathf.Exp(-t * 12f) * Mathf.Min(1f, t * 300f); });
                    }
                    break;
                case "hiss":     // lo spray
                    c = Make(kind, 0.7f, t => N() * 0.35f * Mathf.Min(1f, t * 40f) * Mathf.Min(1f, (0.7f - t) * 8f));
                    break;
                case "snap":     // l'elastico della mascherina
                    c = Make(kind, 0.12f, t => (Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(700f, 200f, t / 0.12f)) * 0.5f + N() * 0.2f) * Mathf.Exp(-t * 35f));
                    break;
                case "sneeze":   // "a... ciù!" in miniatura
                    c = Make(kind, 0.45f, t => t < 0.25f
                        ? Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(500f, 800f, t / 0.25f)) * 0.15f * (t / 0.25f)
                        : N() * 0.55f * Mathf.Exp(-(t - 0.25f) * 18f));
                    break;
                case "click":    // il gancio
                    c = Make(kind, 0.08f, t => (Mathf.Sin(t * 2f * Mathf.PI * 1800f) * 0.5f + N() * 0.3f) * Mathf.Exp(-t * 70f));
                    break;
                case "rattle":   // vetro agganciato scosso
                    c = Make(kind, 0.35f, t => (Mathf.Sin(t * 2f * Mathf.PI * 900f) * 0.3f + N() * 0.25f) * Mathf.Exp(-((t * 14f) % 1f) * 9f) * Mathf.Min(1f, (0.35f - t) * 10f));
                    break;
                case "slide":    // il vetro che scorre nel telaio
                    {
                        float lp = 0f;
                        c = Make(kind, 0.35f, t => { lp += (N() - lp) * 0.12f; return lp * 0.9f * Mathf.Sin(t / 0.35f * Mathf.PI); });
                    }
                    break;
                case "thud":     // vetro chiuso
                    c = Make(kind, 0.2f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(180f, 90f, t / 0.2f)) * 0.7f * Mathf.Exp(-t * 22f));
                    break;
                case "knock":    // nocche sul vetro
                    c = Make(kind, 0.3f, t => (Mathf.Sin(t * 2f * Mathf.PI * 1100f) * 0.4f + N() * 0.15f) * (Mathf.Exp(-t * 45f) + (t > 0.15f ? Mathf.Exp(-(t - 0.15f) * 45f) : 0f)));
                    break;
                case "breeze":   // l'aria di fuori
                    {
                        float lp = 0f;
                        c = Make(kind, 1.0f, t => { lp += (N() - lp) * 0.04f; return lp * 1.6f * Mathf.Sin(t * Mathf.PI); });
                    }
                    break;
                case "bye":      // il ronzio che saluta e si allontana
                    c = Make(kind, 0.8f, t => Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(260f, 330f, Mathf.Abs(Mathf.Sin(t * 8f))))) * 0.15f * (1f - t / 0.8f));
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
            AudioClip clip = AudioClip.Create("v70_a_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
