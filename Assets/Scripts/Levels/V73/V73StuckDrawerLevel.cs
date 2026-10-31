using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 271 "The Stuck Drawer" — "Open the drawer!" Una camera moderna: un cassettone bianco a frontali lisci con
    /// tre cassetti; solo quello in alto ha una maniglietta d'ottone (decorativa) e dentro ci sono le chiavi della
    /// macchina. Intorno: lo spray lubrificante e la lampada sul piano, il foglietto giallo sul muro, il tappeto con
    /// sopra l'asciugacapelli e il cacciavite.
    /// False piste (nessuna azzera i progressi, niente sconfitta): tirare la maniglia (non si muove; tirando forte, o
    /// alla quarta tirata, resta in mano: "Decorative. Of course." — cade sul tappeto e si rimette trascinandola al suo
    /// posto), lo spray portato sul cassettone (cigolio, "Now it's stuck AND shiny.", i frontali restano lucidi), il
    /// cacciavite nella fessura (si piega a zig-zag e resta piegato), l'asciugacapelli ("It's not humid. It's
    /// stubborn."), strisciare le dita sul frontale liscio, una botta al mobile (tocco sul fianco/piano o scossone: il
    /// mobile traballa e il cassetto in basso scatta fuori un attimo e rientra da solo — l'indizio), la lampada, il
    /// tappeto. Un tocco leggero su un frontale lo fa cedere un pelo verso dentro (altro indizio); tenere premuto su un
    /// altro cassetto lo fa aprire da solo (calzini / vuoto) e poi si richiude.
    /// Indizio principale: il foglietto sul muro (un tocco lo ingrandisce) "No handles! Push to open :)".
    /// Soluzione: tenere premuto (spinta decisa, 0.3 s, area generosa: tutto il frontale, maniglia compresa) il
    /// cassetto in alto -> clic, esce da solo, le chiavi luccicano, "IT WAS PUSH-TO-OPEN."; un tocco sulle chiavi ->
    /// "THE HANDLE WAS A LIE.", festa, vittoria. Niente microfono. Pausa/focus perso: il dito si annulla e la scena
    /// torna a una posa ricavata dalla logica (un'apertura o un finale interrotti si completano/rifanno).
    /// La logica è in Gotcha.Levels.V64.StuckDrawerLogic (senza Unity).
    /// Tasti di prova nell'Editor: P spinge il cassetto giusto, O spinge quello in mezzo, Q tocco leggero, H tirata
    /// forte, J rimette la maniglia, L spray, X cacciavite, G asciugacapelli, K botta (S = scossone, da Inputs),
    /// M foglietto, T prende le chiavi.
    /// </summary>
    public class V73StuckDrawerLevel : V62Level
    {
        [Header("Cassettone")]
        [SerializeField] Transform dresser;             // gruppo: corpo e cassetti (traballa alla botta)
        [SerializeField] Collider2D bodyArea;           // tutto il mobile: tocco fuori dai frontali = botta; zona degli attrezzi
        [SerializeField] Transform[] fronts;            // i tre frontali (0 in alto)
        [SerializeField] Collider2D[] frontAreas;
        [SerializeField] Transform[] boxes;             // l'interno di ogni cassetto (figlio del frontale, si vede solo da aperto)
        [SerializeField] SpriteRenderer[] shines;       // il luccichio del lubrificante sui frontali
        [Header("Maniglia")]
        [SerializeField] Transform handle;              // la maniglietta (oggetto a sé: segue l'aggancio finché è attaccata)
        [SerializeField] Collider2D handleArea;
        [SerializeField] Transform handleAnchor;        // figlio del frontale 0
        [SerializeField] Transform handleHoles;         // i due buchini (figli del frontale 0)
        [SerializeField] Transform handleRest;          // dove cade la maniglia staccata (sul tappeto)
        [Header("Chiavi")]
        [SerializeField] Transform keys;
        [SerializeField] Collider2D keysArea;           // generosa: cassetto aperto e frontale
        [SerializeField] Transform[] sparkles;
        [SerializeField] TextMesh clickLabel;
        [Header("Attrezzi")]
        [SerializeField] Transform lube;
        [SerializeField] Collider2D lubeArea;
        [SerializeField] SpriteRenderer mist;
        [SerializeField] Transform screwdriver;
        [SerializeField] Collider2D screwdriverArea;
        [SerializeField] Transform screwdriverStraight;
        [SerializeField] Transform screwdriverBent;
        [SerializeField] Transform dryer;
        [SerializeField] Collider2D dryerArea;
        [SerializeField] SpriteRenderer hotAir;         // figlio dell'asciugacapelli (davanti al beccuccio)
        [Header("Stanza")]
        [SerializeField] Transform lamp;
        [SerializeField] Collider2D lampArea;
        [SerializeField] SpriteRenderer lampGlow;
        [SerializeField] Transform note;
        [SerializeField] Collider2D noteArea;
        [SerializeField] Transform notePopup;           // il foglietto ingrandito (si chiude con un tocco qualsiasi)
        [SerializeField] Transform rug;
        [SerializeField] Collider2D rugArea;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float TapSlop = 0.6f, MoveSlop = 0.24f, TapTime = 1.2f;
        const float OpenDrop = 0.3f, OpenScale = 1.06f, PopDrop = 0.16f, HandleSnap = 0.65f, PullGive = 0.12f;
        const float ShakeLevel = 12f, ShakeCooldown = 1.4f, GrabNagAfter = 5f;

        static readonly string[] BudgeLines = { "It doesn't budge.", "Pull harder? Sure...", "Not even a wiggle." };
        static readonly string[] NudgeLines = { "Hm. It gave a little. Inward.", "It moves IN. Not out.", "A tiny click... almost." };
        static readonly string[] ScratchLines = { "No grip. It's perfectly flat.", "Fingernails: 0. Drawer: 1.", "Flat front. Nothing to pull." };
        static readonly string[] ShinierLines = { "Shinier. Still stuck.", "You can see your face in it. Still stuck.", "Maximum shine. Minimum progress." };
        static readonly string[] BentLines = { "It's already bent. Like your patience.", "Still bent. The gap is still thin.", "That's not a tool anymore. That's art." };
        static readonly string[] DryerLines = { "It's not humid. It's stubborn.", "Warm drawer. Still stuck.", "Nice and toasty. Still closed." };
        static readonly string[] BumpLines = { "Wobble... the bottom drawer popped out! Then closed itself.", "Boing. It springs back by itself.", "The bottom drawer waves at you." };
        static readonly string[] SockLines = { "It popped open by itself! Socks. Not keys.", "Push. Pop. Socks again.", "Still socks. Wrong drawer." };
        static readonly string[] EmptyLines = { "Popped open by itself! Empty. Then it closed again.", "Push. Pop. Nothing.", "Empty. Bouncy, though." };
        static readonly string[] RugLines = { "Nothing under the rug. Not even dust.", "Still just a rug.", "The rug is innocent." };

        enum Kind { None, Front, Handle, HandleLoose, Tool, Keys, Body, Lamp, Note, Rug, Other }

        sealed class Grip
        {
            public Kind kind;
            public int index;
            public Vector2 start, last;
            public float t0, maxPull;
            public bool moved;
        }

        /// <summary>Un effetto (rimbalzo, oscillazione, tremolio) in corso su un oggetto e il valore a cui tornare.</summary>
        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        readonly StuckDrawerLogic logic = new StuckDrawerLogic();
        Camera cam;
        HwToast toast;
        HwDragger dragger;
        HwDragger.Item lubeItem, screwItem, dryerItem;
        Grip grip;
        Vector3[] frontHome, frontScale, boxScale, sparkleHome;
        SpriteRenderer keysArt;
        int keysOrder;
        Vector3 dresserPos, keysHome, keysScale, handleScale, dryerScale, popupScale;
        Quaternion dresserRot, keysRot, dryerRot, handleRot;
        Coroutine[] popCo;
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        int budgeLines, nudgeLines, scratchLines, shinierLines, bentLines, dryerLines, bumpLines, sockLines, emptyLines, rugLines;
        float shakeReadyAt, openSince;
        bool busy, finishing, captured, noteOpen, nagged, handleHeld;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            logic.Reset();
            ResetV62();
            grip = null;
            busy = finishing = noteOpen = nagged = handleHeld = false;
            budgeLines = nudgeLines = scratchLines = shinierLines = bentLines = dryerLines = bumpLines = sockLines = emptyLines = rugLines = 0;
            toast = new HwToast(infoText, infoHolder);
            int n = fronts != null ? fronts.Length : 0;
            if (!captured)
            {
                captured = true;
                dresserPos = dresser.localPosition;
                dresserRot = dresser.localRotation;
                frontHome = new Vector3[n];
                frontScale = new Vector3[n];
                boxScale = new Vector3[n];
                for (int i = 0; i < n; i++)
                {
                    frontHome[i] = fronts[i].localPosition;
                    frontScale[i] = fronts[i].localScale;
                    boxScale[i] = boxes != null && i < boxes.Length && boxes[i] != null ? boxes[i].localScale : Vector3.one;
                }
                sparkleHome = new Vector3[sparkles.Length];
                for (int i = 0; i < sparkles.Length; i++) sparkleHome[i] = sparkles[i].localPosition;
                keysArt = keys.GetComponentInChildren<SpriteRenderer>();
                keysOrder = keysArt != null ? keysArt.sortingOrder : 0;
                keysHome = keys.localPosition;
                keysScale = keys.localScale;
                keysRot = keys.localRotation;
                handleScale = handle.localScale;
                handleRot = handle.localRotation;
                dryerScale = dryer.localScale;
                dryerRot = dryer.localRotation;
                popupScale = notePopup.localScale;
            }
            popCo = new Coroutine[n];
            dragger = new HwDragger(this);
            lubeItem = dragger.Add(lube, lubeArea, 2);
            screwItem = dragger.Add(screwdriver, screwdriverArea, 2);
            dryerItem = dragger.Add(dryer, dryerArea, 2);
            shakeReadyAt = Time.time + 0.5f;
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            dresser.localPosition = dresserPos;
            dresser.localRotation = dresserRot;
            for (int i = 0; i < fronts.Length; i++)
            {
                if (popCo != null && popCo[i] != null) { StopCoroutine(popCo[i]); popCo[i] = null; }
                FrontPose(i, i == StuckDrawerLogic.Target && logic.DrawerOpen ? 1f : 0f);
                if (shines != null && i < shines.Length) HalloweenKit.Show(shines[i], logic.Lubed);
            }
            // chiavi
            keys.localPosition = keysHome;
            keys.localScale = keysScale;
            keys.localRotation = keysRot;
            HalloweenKit.Show(keys, !logic.KeysTaken);
            if (keysArt != null) keysArt.sortingOrder = keysOrder;
            for (int i = 0; i < sparkles.Length; i++)
            {
                sparkles[i].localPosition = sparkleHome[i];
                HalloweenKit.Show(sparkles[i], logic.Stage == DrawerStage.Open);
            }
            HalloweenKit.Show(clickLabel, false);
            // maniglia
            handleHeld = false;
            handle.localScale = handleScale;
            handle.localRotation = handleRot;
            if (logic.HandleOn) handle.position = handleAnchor.position;
            else handle.position = handleRest.position;
            HalloweenKit.Show(handleHoles, !logic.HandleOn);
            // attrezzi
            HalloweenKit.Show(screwdriverStraight, !logic.ScrewdriverBent);
            HalloweenKit.Show(screwdriverBent, logic.ScrewdriverBent);
            HalloweenKit.SetAlpha(mist, 0f);
            HalloweenKit.Show(hotAir, false);
            dryer.localScale = dryerScale;
            dryer.localRotation = dryerRot;
            foreach (HwDragger.Item it in new[] { lubeItem, screwItem, dryerItem })
            {
                dragger.Stop(it);
                it.T.position = it.Home;
                it.Enabled = true;
            }
            // stanza
            HalloweenKit.SetAlpha(lampGlow, logic.LampOn ? 0.55f : 0f);
            HalloweenKit.Show(notePopup, noteOpen);
            notePopup.localScale = popupScale;
        }

        /// <summary>Un frontale tra chiuso (k = 0) e tutto fuori (k = 1): scende, si ingrandisce, si vede l'interno.</summary>
        void FrontPose(int i, float k)
        {
            fronts[i].localPosition = frontHome[i] + new Vector3(0f, -OpenDrop * k, 0f);
            fronts[i].localScale = frontScale[i] * Mathf.Lerp(1f, OpenScale, k);
            fronts[i].localRotation = Quaternion.identity;
            if (boxes == null || i >= boxes.Length || boxes[i] == null) return;
            HalloweenKit.Show(boxes[i], k > 0.01f);
            // l'interno è figlio del frontale (si ingrandisce con lui): qui si apre solo in altezza
            boxes[i].localScale = new Vector3(boxScale[i].x, boxScale[i].y * k, boxScale[i].z);
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: niente resta a metà, si torna a una posa ricavata dalla logica
            CancelGrip();
            busy = false;
            StopFx();
            if (popCo != null) for (int i = 0; i < popCo.Length; i++) popCo[i] = null;
            noteOpen = false;
            if (captured && dragger != null && !IsFinished && !finishing)
            {
                // un'apertura interrotta si completa: il cassetto resta aperto con le chiavi dentro
                if (logic.FinishOpening()) openSince = Time.time;
                RestPose();
            }
        }

        void OnEnable()
        {
            if (!captured || IsFinished || dragger == null) return;
            // spento a metà del finale: si rifà da capo
            if (logic.Stage == DrawerStage.Taking) { finishing = false; StartCoroutine(Finale()); }
            else if (logic.Stage == DrawerStage.Open) openSince = Time.time;
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrip(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrip(); }

        /// <summary>Il dito sparisce (pausa, focus perso, animazione che parte): niente resta a metà.</summary>
        void CancelGrip()
        {
            Grip g = grip;
            grip = null;
            if (g != null && g.kind == Kind.Front && fronts != null && g.index < fronts.Length && captured && popCo != null && popCo[g.index] == null)
                FrontPose(g.index, g.index == StuckDrawerLogic.Target && logic.DrawerOpen ? 1f : 0f);
            if (g != null && g.kind == Kind.Handle && captured && !logic.DrawerOpen && popCo != null && popCo[0] == null) FrontPose(0, 0f);
            if (handleHeld || (g != null && g.kind == Kind.Handle)) SettleHandle();
            if (dragger != null && dragger.Held != null)
            {
                HwDragger.Item it = dragger.Release();
                if (it != null) dragger.SendHome(it, 0.2f);
            }
        }

        /// <summary>La maniglia dove dice la logica (attaccata al frontale o per terra sul tappeto).</summary>
        void SettleHandle()
        {
            handleHeld = false;
            if (handle == null) return;
            handle.localScale = handleScale;
            handle.localRotation = handleRot;
            handle.position = logic.HandleOn ? handleAnchor.position : handleRest.position;
        }

        void Update()
        {
            toast?.Tick();
            if (IsFinished || toast == null) return;
            if (logic.HandleOn && !handleHeld && handle != null && handleAnchor != null && (grip == null || grip.kind != Kind.Handle))
                handle.position = handleAnchor.position;   // segue il frontale (botta, apertura)
            if (finishing) return;
            if (Time.timeScale <= 0f) { CancelGrip(); return; }   // pausa o pubblicità: niente input
            Ambient();
            if (busy) { CancelGrip(); return; }
#if UNITY_EDITOR
            EditorKeys();
            if (busy || finishing) return;
#endif
            if (noteOpen) { ReadPointer(); return; }
            if (!logic.InputLocked && Inputs.Shake() > ShakeLevel && Time.time >= shakeReadyAt && grip == null)
            {
                shakeReadyAt = Time.time + ShakeCooldown;
                Bump();
                return;
            }
            ReadPointer();
            HoldCheck();
        }

        /// <summary>Le piccole cose vive: le stelline sulle chiavi, il richiamo a prenderle.</summary>
        void Ambient()
        {
            if (logic.Stage != DrawerStage.Open) return;
            for (int i = 0; i < sparkles.Length; i++)
            {
                if (sparkles[i] == null) continue;
                float s = 0.6f + 0.4f * Mathf.Abs(Mathf.Sin(Time.time * 3f + i * 2.1f));
                sparkles[i].localScale = Vector3.one * s;
                sparkles[i].localRotation = Quaternion.Euler(0f, 0f, Time.time * 40f + i * 30f);
            }
            if (!nagged && Time.time - openSince > GrabNagAfter)
            {
                nagged = true;
                FxPop(keys, 1.25f, 0.35f);
                toast.Show("Grab the keys!", 1.8f);
            }
        }

        // ---------- Effetti che non si accavallano ----------

        // 0 = scala (rimbalzo), 1 = rotazione (oscillazione), 2 = posizione (tremolio): un effetto nuovo sullo stesso
        // oggetto ferma quello vecchio e riparte dal valore di riposo, così tocchi ripetuti non lasciano niente storto
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
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (grip != null) End(grip);
                return;
            }
            if (began) { if (grip != null) End(grip); grip = Begin(w); }
            if (grip == null) return;
            grip.last = w;
            if (Vector2.Distance(w, grip.start) > MoveSlop) grip.moved = true;
            Drag(grip);
            if (ended && grip != null) End(grip);
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, t0 = Time.time, kind = Kind.Other };
            if (busy || finishing) { g.kind = Kind.None; return g; }
            if (noteOpen) { CloseNote(); g.kind = Kind.None; return g; }
            if (logic.Stage == DrawerStage.Open)
            {
                if (keysArea.OverlapPoint(w)) g.kind = Kind.Keys;
                return g;
            }
            if (logic.InputLocked) { g.kind = Kind.None; return g; }
            // prima la maniglia (attaccata o per terra), poi gli attrezzi, i frontali e il resto
            if (handleArea.OverlapPoint(w))
            {
                g.kind = logic.HandleOn ? Kind.Handle : Kind.HandleLoose;
                if (!logic.HandleOn) { handleHeld = true; FxPop(handle, 1.2f, 0.15f); }
                Haptics.Tap();
                return g;
            }
            HwDragger.Item it = dragger.TryPick(w);
            if (it != null) { g.kind = Kind.Tool; return g; }
            for (int i = 0; i < frontAreas.Length; i++)
                if (frontAreas[i] != null && frontAreas[i].OverlapPoint(w)) { g.kind = Kind.Front; g.index = i; return g; }
            if (noteArea.OverlapPoint(w)) { g.kind = Kind.Note; return g; }
            if (lampArea.OverlapPoint(w)) { g.kind = Kind.Lamp; return g; }
            if (bodyArea.OverlapPoint(w)) { g.kind = Kind.Body; return g; }
            if (rugArea.OverlapPoint(w)) { g.kind = Kind.Rug; return g; }
            return g;
        }

        void Drag(Grip g)
        {
            switch (g.kind)
            {
                case Kind.Tool:
                    if (dragger.Held != null && g.moved) dragger.MoveHeld(g.last);
                    break;
                case Kind.HandleLoose:
                    if (g.moved)
                    {
                        Vector2 p = HalloweenKit.ClampSafe(g.last);
                        handle.position = new Vector3(p.x, p.y, handle.position.z);
                    }
                    break;
                case Kind.Handle:
                    {
                        // la maniglia cede un pelo verso il dito, il cassetto trema; tirando forte si stacca
                        Vector2 d = g.last - g.start;
                        g.maxPull = Mathf.Max(g.maxPull, d.magnitude);
                        Vector2 give = Vector2.ClampMagnitude(d * 0.15f, PullGive);
                        handle.position = handleAnchor.position + new Vector3(give.x, give.y, 0f);
                        if (g.moved && fronts.Length > 0)
                            fronts[0].localPosition = frontHome[0] + new Vector3(Random.Range(-0.012f, 0.012f), Random.Range(-0.008f, 0.008f), 0f);
                        if (g.maxPull >= StuckDrawerLogic.HardPull) SnapHandle(g);
                    }
                    break;
            }
        }

        /// <summary>Una pressione ferma su un frontale (maniglia compresa) che dura abbastanza è una spinta decisa.</summary>
        void HoldCheck()
        {
            Grip g = grip;
            if (g == null || g.moved || (g.kind != Kind.Front && g.kind != Kind.Handle)) return;
            int i = g.kind == Kind.Handle ? StuckDrawerLogic.Target : g.index;
            float held = Time.time - g.t0;
            // il frontale affonda un pelo mentre lo si preme
            if (popCo[i] == null)
                fronts[i].localScale = frontScale[i] * (1f - 0.03f * Mathf.Clamp01(held / StuckDrawerLogic.FirmPress));
            if (held < StuckDrawerLogic.FirmPress) return;
            grip = null;
            if (g.kind == Kind.Handle) SettleHandle();
            Push(i, held);
        }

        void End(Grip g)
        {
            grip = null;
            bool tap = Vector2.Distance(g.last, g.start) < TapSlop && Time.time - g.t0 <= TapTime;
            switch (g.kind)
            {
                case Kind.Front:
                    if (popCo[g.index] == null) FrontPose(g.index, 0f);
                    if (g.moved) Scratch(g.index);
                    else Push(g.index, Time.time - g.t0);
                    break;
                case Kind.Handle:
                    if (fronts.Length > 0) fronts[0].localPosition = frontHome[0];
                    Pull(g.moved ? g.maxPull : 0f);
                    break;
                case Kind.HandleLoose: DropHandle(); break;
                case Kind.Tool: DropTool(g); break;
                case Kind.Keys: if (tap) TakeKeys(); break;
                case Kind.Body: if (tap) Bump(); break;
                case Kind.Lamp: if (tap) TapLamp(); break;
                case Kind.Note: if (tap) OpenNote(); break;
                case Kind.Rug:
                    if (!tap || !logic.TapRug()) break;
                    FxWiggle(rug, 2f, 0.4f, 25f);
                    Sfx.Play(this, "whoosh", 0.3f);
                    toast.Show(RugLines[rugLines++ % RugLines.Length], 1.6f);
                    break;
            }
        }

        // ---------- I frontali ----------

        void Push(int i, float seconds)
        {
            DrawerPush r = logic.Press(i, seconds);
            switch (r)
            {
                case DrawerPush.Click: StartCoroutine(OpenDrawer()); break;
                case DrawerPush.Nudge:
                    V73AStuckDrawerSound.Play(this, "tock", 0.5f);
                    Haptics.Tap();
                    Fx(0, fronts[i], NudgeRun(i));
                    toast.Show(NudgeLines[nudgeLines++ % NudgeLines.Length], 1.6f);
                    break;
                case DrawerPush.OtherPops:
                    PopOther(i, 1.3f);
                    if (i == 1) toast.Show(SockLines[sockLines++ % SockLines.Length], 2f);
                    else toast.Show(EmptyLines[emptyLines++ % EmptyLines.Length], 2f);
                    break;
            }
        }

        /// <summary>Il tocco leggero: il frontale affonda di un pelo e torna.</summary>
        IEnumerator NudgeRun(int i)
        {
            Vector3 s = frontScale[i];
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                fronts[i].localScale = s * (1f - 0.04f * Mathf.Sin(k * Mathf.PI));
                yield return null;
            }
            fronts[i].localScale = s;
        }

        void Scratch(int i)
        {
            if (!logic.Scratch()) return;
            V73AStuckDrawerSound.Play(this, "scratch", 0.5f);
            FxShake(fronts[i], 0.02f, 0.2f);
            toast.Show(ScratchLines[scratchLines++ % ScratchLines.Length], 1.6f);
        }

        /// <summary>Un cassetto (non quello giusto) scatta fuori da solo, resta un attimo e rientra con un clic.</summary>
        void PopOther(int i, float stay, float depth = 1f)
        {
            if (popCo[i] != null) StopCoroutine(popCo[i]);
            popCo[i] = StartCoroutine(PopRun(i, stay, depth));
        }

        IEnumerator PopRun(int i, float stay, float depth)
        {
            V73AStuckDrawerSound.Play(this, "click", 0.6f);
            Haptics.Light();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.08f) { fronts[i].localScale = frontScale[i] * (1f - 0.03f * k); yield return null; }
            V73AStuckDrawerSound.Play(this, "glide", 0.45f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.22f)
            {
                float e = 1f - (1f - k) * (1f - k);
                FrontPose(i, depth * e * (1f + 0.15f * Mathf.Sin(k * Mathf.PI)));
                yield return null;
            }
            FrontPose(i, depth);
            yield return new WaitForSeconds(stay);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { FrontPose(i, depth * (1f - k * k)); yield return null; }
            FrontPose(i, 0f);
            V73AStuckDrawerSound.Play(this, "click", 0.4f);
            popCo[i] = null;
        }

        void Bump()
        {
            if (!logic.Bump()) return;
            Sfx.Play(this, "bonk", 0.6f);
            Haptics.Medium();
            FxWiggle(dresser, 2.5f, 0.5f, 30f);
            FxWiggle(lamp, 4f, 0.5f, 28f);
            FxWiggle(lube, 5f, 0.5f, 26f);
            // l'indizio: il cassetto in basso scatta fuori un attimo e rientra da solo
            if (fronts.Length > 2) PopOther(2, 0.35f, PopDrop / OpenDrop);
            toast.Show(BumpLines[bumpLines++ % BumpLines.Length], 2.1f);
        }

        // ---------- La maniglia ----------

        void Pull(float distance)
        {
            DrawerPull r = logic.PullHandle(distance);
            switch (r)
            {
                case DrawerPull.Budge:
                    V73AStuckDrawerSound.Play(this, "rattle", 0.5f);
                    Haptics.Light();
                    handle.position = handleAnchor.position;
                    FxShake(handle, 0.02f, 0.2f);
                    toast.Show(BudgeLines[budgeLines++ % BudgeLines.Length], 1.5f);
                    break;
                case DrawerPull.HandleOff:
                    // la quarta tirata debole: la maniglia resta in mano e cade sul tappeto
                    HandleCameOff();
                    StartCoroutine(FallHandle());
                    break;
                default:
                    SettleHandle();
                    break;
            }
        }

        /// <summary>Tirata forte durante il trascinamento: la maniglia si stacca e resta attaccata al dito.</summary>
        void SnapHandle(Grip g)
        {
            if (logic.PullHandle(g.maxPull) != DrawerPull.HandleOff) return;
            HandleCameOff();
            g.kind = Kind.HandleLoose;
            g.moved = true;
            handleHeld = true;
        }

        void HandleCameOff()
        {
            V73AStuckDrawerSound.Play(this, "snap", 0.7f);
            Haptics.Medium();
            if (fronts.Length > 0) fronts[0].localPosition = frontHome[0];
            HalloweenKit.Show(handleHoles, true);
            FxPop(handle, 1.3f, 0.2f);
            toast.Show("Decorative. Of course.", 2f);
        }

        IEnumerator FallHandle()
        {
            handleHeld = true;
            yield return HalloweenKit.Move(handle, handleRest.position, 0.4f, 0.3f);
            Sfx.Play(this, "tick", 0.5f);
            handleHeld = false;
        }

        /// <summary>La maniglia staccata rilasciata: vicino al suo posto si riattacca, altrimenti cade sul tappeto.</summary>
        void DropHandle()
        {
            handleHeld = false;
            if (Vector2.Distance(handle.position, handleAnchor.position) < HandleSnap && logic.PutHandleBack())
            {
                handle.position = handleAnchor.position;
                HalloweenKit.Show(handleHoles, false);
                V73AStuckDrawerSound.Play(this, "click", 0.5f);
                FxPop(handle, 1.25f, 0.2f);
                toast.Show("Back on. Still decorative.", 1.7f);
                return;
            }
            if (Vector2.Distance(handle.position, handleRest.position) > 0.05f) StartCoroutine(FallHandle());
            else toast.Show("Two tiny holes on the drawer. That's where it goes.", 1.8f);
        }

        // ---------- Gli attrezzi ----------

        void DropTool(Grip g)
        {
            HwDragger.Item it = dragger.Release();
            if (it == null) return;
            DrawerTool tool = it == lubeItem ? DrawerTool.Lube : it == screwItem ? DrawerTool.Screwdriver : DrawerTool.Hairdryer;
            if (!g.moved)
            {
                dragger.SendHome(it, 0.15f);
                FxWiggle(it.T, 6f, 0.3f, 25f);
                toast.Show(tool == DrawerTool.Lube ? "Lubricant. Fixes everything. Allegedly."
                    : tool == DrawerTool.Screwdriver ? (logic.ScrewdriverBent ? "It used to be straight." : "A screwdriver. Basically a tiny crowbar.")
                    : "A hairdryer. Point it at something.", 1.7f);
                return;
            }
            bool onDresser = bodyArea.OverlapPoint(g.last) || bodyArea.OverlapPoint(it.T.position);
            if (!onDresser) { dragger.SendHome(it, 0.3f); return; }
            int drawer = NearestDrawer(g.last.y);
            StartCoroutine(ToolRun(it, tool, drawer));
        }

        int NearestDrawer(float y)
        {
            int best = 0;
            float bd = float.MaxValue;
            for (int i = 0; i < fronts.Length; i++)
            {
                float d = Mathf.Abs(fronts[i].position.y - y);
                if (d < bd) { bd = d; best = i; }
            }
            return best;
        }

        IEnumerator ToolRun(HwDragger.Item it, DrawerTool tool, int drawer)
        {
            busy = true;
            CancelGrip();
            Vector3 f = fronts[drawer].position;
            DrawerToolResult r = logic.UseTool(tool);
            switch (r)
            {
                case DrawerToolResult.Squeak:
                case DrawerToolResult.Shinier:
                    yield return HalloweenKit.Move(lube, f + new Vector3(1.0f, 0.1f, 0f), 0.2f);
                    V73AStuckDrawerSound.Play(this, "spray", 0.5f);
                    if (mist != null)
                    {
                        mist.transform.position = new Vector3(f.x + 0.4f, f.y + 0.1f, mist.transform.position.z);
                        yield return HalloweenKit.Fade(mist, 0.9f, 0.15f);
                    }
                    for (int i = 0; i < shines.Length; i++) HalloweenKit.Show(shines[i], true);
                    yield return new WaitForSeconds(0.35f);
                    StartCoroutine(HalloweenKit.Fade(mist, 0f, 0.4f));
                    FxShake(fronts[drawer], 0.02f, 0.25f);
                    V73AStuckDrawerSound.Play(this, "squeak", 0.6f);
                    toast.Show(r == DrawerToolResult.Squeak ? "Squeak. Now it's stuck AND shiny." : ShinierLines[shinierLines++ % ShinierLines.Length], 2f);
                    break;
                case DrawerToolResult.Bends:
                case DrawerToolResult.AlreadyBent:
                    {
                        // la punta nella fessura sopra il cassetto
                        Vector3 gap = f + new Vector3(-0.2f, 0.29f, 0f);
                        yield return HalloweenKit.Move(screwdriver, gap + new Vector3(-0.85f, 0.05f, 0f), 0.2f);
                        for (int j = 0; j < 2; j++)
                        {
                            yield return HalloweenKit.Move(screwdriver, gap + new Vector3(-0.6f, 0f, 0f), 0.08f);
                            V73AStuckDrawerSound.Play(this, "tock", 0.5f);
                            yield return HalloweenKit.Move(screwdriver, gap + new Vector3(-0.85f, 0.05f, 0f), 0.1f);
                        }
                        if (r == DrawerToolResult.Bends)
                        {
                            V73AStuckDrawerSound.Play(this, "boing", 0.7f);
                            HalloweenKit.Show(screwdriverStraight, false);
                            HalloweenKit.Show(screwdriverBent, true);
                            FxWiggle(screwdriver, 10f, 0.5f, 30f);
                            toast.Show("The gap is too thin. The screwdriver bends.", 2f);
                        }
                        else
                        {
                            V73AStuckDrawerSound.Play(this, "boing", 0.4f);
                            toast.Show(BentLines[bentLines++ % BentLines.Length], 1.8f);
                        }
                        yield return new WaitForSeconds(0.4f);
                    }
                    break;
                case DrawerToolResult.Blows:
                    {
                        // l'asciugacapelli sopra il cassetto, col beccuccio in giù
                        dryer.localRotation = Quaternion.identity;
                        dryer.localScale = Vector3.one * Mathf.Abs(dryerScale.x);
                        yield return HalloweenKit.Move(dryer, f + new Vector3(0.3f, 0.75f, 0f), 0.2f);
                        HalloweenKit.Show(hotAir, true);
                        V73AStuckDrawerSound.Play(this, "hum", 0.45f);
                        for (float k = 0f; k < 1.1f; k += Time.deltaTime)
                        {
                            if (hotAir != null)
                            {
                                hotAir.flipX = Mathf.Repeat(k * 8f, 2f) > 1f;
                                hotAir.color = new Color(1f, 1f, 1f, 0.6f + 0.4f * Mathf.Sin(k * 20f));
                            }
                            yield return null;
                        }
                        HalloweenKit.Show(hotAir, false);
                        dryer.localRotation = dryerRot;
                        dryer.localScale = dryerScale;
                        toast.Show(DryerLines[dryerLines++ % DryerLines.Length], 1.8f);
                    }
                    break;
            }
            dragger.SendHome(it, 0.3f);
            yield return new WaitForSeconds(0.2f);
            busy = false;
        }

        // ---------- Lampada e foglietto ----------

        void TapLamp()
        {
            bool on = logic.TapLamp();
            Sfx.Play(this, "tick", 0.6f);
            HalloweenKit.SetAlpha(lampGlow, on ? 0.55f : 0f);
            FxWiggle(lamp, 3f, 0.3f, 25f);
            toast.Show(on ? "Better light. Same drawer." : "Mood lighting. Still stuck.", 1.5f);
        }

        void OpenNote()
        {
            if (busy || logic.InputLocked) return;
            logic.ReadNote();
            noteOpen = true;
            CancelGrip();
            HalloweenKit.Show(notePopup, true);
            notePopup.localScale = popupScale;
            FxPop(notePopup, 1.06f, 0.25f);
            FxPop(note, 1.15f, 0.2f);
            V73AStuckDrawerSound.Play(this, "paper", 0.5f);
        }

        void CloseNote()
        {
            if (!noteOpen) return;
            noteOpen = false;
            HalloweenKit.Show(notePopup, false);
            V73AStuckDrawerSound.Play(this, "paper", 0.35f);
        }

        // ---------- La soluzione ----------

        IEnumerator OpenDrawer()
        {
            CancelGrip();
            CloseNote();
            int i = StuckDrawerLogic.Target;
            if (popCo[i] != null) { StopCoroutine(popCo[i]); popCo[i] = null; }
            // clic: il frontale affonda...
            V73AStuckDrawerSound.Play(this, "click", 0.8f);
            Haptics.Medium();
            if (clickLabel != null)
            {
                HalloweenKit.Show(clickLabel, true);
                FxPop(clickLabel.transform, 1.4f, 0.3f);
            }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.1f) { fronts[i].localScale = frontScale[i] * (1f - 0.04f * k); yield return null; }
            yield return new WaitForSeconds(0.12f);
            // ...e scivola fuori da solo
            V73AStuckDrawerSound.Play(this, "glide", 0.6f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.55f)
            {
                float e = 1f - (1f - k) * (1f - k) * (1f - k);
                FrontPose(i, e);
                yield return null;
            }
            FrontPose(i, 1f);
            HalloweenKit.Show(clickLabel, false);
            foreach (Transform s in sparkles) HalloweenKit.Show(s, true);
            V73AStuckDrawerSound.Play(this, "jingle", 0.6f);
            FxPop(keys, 1.3f, 0.35f);
            toast.Show("IT WAS PUSH-TO-OPEN.", 2.4f);
            if (logic.FinishOpening()) openSince = Time.time;
            nagged = false;
        }

        void TakeKeys()
        {
            if (!logic.TakeKeys()) return;
            StartCoroutine(Finale());
        }

        IEnumerator Finale()
        {
            finishing = true;
            busy = false;
            CancelGrip();
            CloseNote();
            FrontPose(StuckDrawerLogic.Target, 1f);
            keys.localPosition = keysHome;
            keys.localScale = keysScale;
            keys.localRotation = keysRot;
            HalloweenKit.Show(keys, true);
            if (keysArt != null) keysArt.sortingOrder = 60;
            for (int i = 0; i < sparkles.Length; i++) sparkles[i].localPosition = sparkleHome[i];
            V73AStuckDrawerSound.Play(this, "jingle", 0.7f);
            Haptics.Light();
            // le chiavi saltano fuori, al centro, grandi
            Vector3 to = new Vector3(0f, 0.9f, keys.position.z);
            Vector3 s0 = keys.localScale;
            Vector3 p0 = keys.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                float e = 1f - (1f - k) * (1f - k);
                keys.position = Vector3.Lerp(p0, to, e) + new Vector3(0f, 0.6f * Mathf.Sin(k * Mathf.PI), 0f);
                keys.localScale = s0 * Mathf.Lerp(1f, 2.2f, e);
                keys.localRotation = Quaternion.Euler(0f, 0f, 360f * e);
                yield return null;
            }
            keys.position = to;
            keys.localRotation = Quaternion.identity;
            for (int i = 0; i < sparkles.Length; i++)
                if (sparkles[i] != null) sparkles[i].position = to + new Vector3(Mathf.Cos(i * 2.1f) * 0.75f, Mathf.Sin(i * 2.1f) * 0.45f, 0f);
            FxWiggle(keys, 8f, 1.2f, 14f);
            toast.Show("THE HANDLE WAS A LIE.", 2.6f);
            yield return Celebrate(1.4f);
            if (logic.TryComplete()) Win();
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (Input.GetKeyDown(KeyCode.M)) { if (noteOpen) CloseNote(); else OpenNote(); return; }
            if (noteOpen) return;
            if (Input.GetKeyDown(KeyCode.T)) { TakeKeys(); return; }
            if (logic.InputLocked) return;
            if (Input.GetKeyDown(KeyCode.P)) { CancelGrip(); Push(StuckDrawerLogic.Target, 1f); return; }
            if (Input.GetKeyDown(KeyCode.O)) Push(1, 1f);
            if (Input.GetKeyDown(KeyCode.Q)) Push(StuckDrawerLogic.Target, 0f);
            if (Input.GetKeyDown(KeyCode.H) && logic.HandleOn) { CancelGrip(); Pull(StuckDrawerLogic.HardPull); }
            if (Input.GetKeyDown(KeyCode.J) && !logic.HandleOn) { CancelGrip(); handle.position = handleAnchor.position; DropHandle(); }
            if (Input.GetKeyDown(KeyCode.K)) Bump();
            if (Input.GetKeyDown(KeyCode.L)) { CancelGrip(); StartCoroutine(ToolRun(lubeItem, DrawerTool.Lube, 0)); return; }
            if (Input.GetKeyDown(KeyCode.X)) { CancelGrip(); StartCoroutine(ToolRun(screwItem, DrawerTool.Screwdriver, 0)); return; }
            if (Input.GetKeyDown(KeyCode.G)) { CancelGrip(); StartCoroutine(ToolRun(dryerItem, DrawerTool.Hairdryer, 0)); }
        }
#endif
    }

    /// <summary>Suoni sintetizzati del livello 271: clic del push-to-open, scivolata, cigolio, spray, boing, phon.</summary>
    public static class V73AStuckDrawerSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(271);

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
                case "click":    // lo scatto del push-to-open: clic secco e acuto
                    c = Make(kind, 0.07f, t => (Mathf.Sin(t * 2f * Mathf.PI * 2600f) * 0.5f + N() * 0.4f) * Mathf.Exp(-t * 90f));
                    break;
                case "tock":     // nocche sul legno
                    c = Make(kind, 0.1f, t => (Mathf.Sin(t * 2f * Mathf.PI * 420f) * 0.7f + N() * 0.2f) * Mathf.Exp(-t * 50f));
                    break;
                case "glide":    // il cassetto che scivola sulle guide
                    {
                        float lp = 0f;
                        c = Make(kind, 0.5f, t => { lp += (N() - lp) * 0.15f; return lp * 0.9f * Mathf.Sin(t / 0.5f * Mathf.PI); });
                    }
                    break;
                case "rattle":   // la maniglia che sbatacchia
                    c = Make(kind, 0.25f, t => (Mathf.Repeat(t * 28f, 1f) < 0.25f ? N() * 0.6f : 0f) * Mathf.Exp(-t * 6f));
                    break;
                case "snap":     // la maniglia che si stacca
                    c = Make(kind, 0.15f, t => (N() * 0.7f + Mathf.Sin(t * 2f * Mathf.PI * 1500f) * 0.3f) * Mathf.Exp(-t * 35f));
                    break;
                case "squeak":   // cigolio
                    c = Make(kind, 0.4f, t => Mathf.Sin(t * 2f * Mathf.PI * (900f + 400f * Mathf.Sin(t * 14f))) * 0.3f * Mathf.Sin(t / 0.4f * Mathf.PI));
                    break;
                case "spray":    // psssh
                    {
                        float lp = 0f;
                        c = Make(kind, 0.55f, t => { float n = N(); float hp = n - lp; lp += (n - lp) * 0.3f; return hp * 0.35f * Mathf.Min(1f, t * 30f) * Mathf.Min(1f, (0.55f - t) * 10f); });
                    }
                    break;
                case "boing":    // il cacciavite che si piega
                    c = Make(kind, 0.6f, t => Mathf.Sin(t * 2f * Mathf.PI * (180f + 90f * Mathf.Sin(t * 30f))) * 0.45f * Mathf.Exp(-t * 4f));
                    break;
                case "hum":      // il phon
                    c = Make(kind, 1.1f, t => (Mathf.Sin(t * 2f * Mathf.PI * 140f) * 0.15f + N() * 0.2f) * Mathf.Min(1f, t * 10f) * Mathf.Min(1f, (1.1f - t) * 8f));
                    break;
                case "paper":    // il foglietto
                    c = Make(kind, 0.2f, t => N() * 0.3f * Mathf.Sin(t / 0.2f * Mathf.PI) * (0.5f + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 30f)));
                    break;
                case "jingle":   // le chiavi che tintinnano
                    c = Make(kind, 0.6f, t =>
                    {
                        float v = 0f;
                        for (int i = 0; i < 4; i++)
                        {
                            float u = t - i * 0.09f;
                            if (u > 0f) v += Mathf.Sin(u * 2f * Mathf.PI * (3100f + i * 470f)) * Mathf.Exp(-u * 16f);
                        }
                        return v * 0.22f;
                    });
                    break;
                case "scratch":  // unghie sul laccato
                    c = Make(kind, 0.3f, t => N() * 0.25f * (0.5f + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 45f)) * Mathf.Sin(t / 0.3f * Mathf.PI));
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
            AudioClip clip = AudioClip.Create("v73a_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
