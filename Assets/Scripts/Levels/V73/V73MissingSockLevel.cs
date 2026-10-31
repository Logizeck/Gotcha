using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 272 "The Missing Sock" — "Find the matching sock!" Una lavanderia: la lavatrice a oblò (sportello chiuso,
    /// guarnizione di gomma), l'asciugatrice, la cesta con sette calzini spaiati tutti diversi, lo stendino con il calzino
    /// da accoppiare (verde a stelline gialle) e accanto una molletta vuota, il gatto che dorme, il detersivo.
    /// False piste (nessuna azzera i progressi, niente sconfitta): portare sullo stendino un calzino della cesta (critica
    /// di moda: "Bold. But no."), aprire l'asciugatrice (solo pelucchi e un coniglietto di pelucchi), toccare il gatto
    /// (si gira: dormiva su un calzino verde e giallo... a pois: "Nice try, cat."), scuotere il telefono (i calzini nella
    /// cesta saltellano), aprire la lavatrice ("Empty. Suspiciously empty."), il cestello, il detersivo, il pannello.
    /// Indizio: un pezzetto di stoffa a stelline spunta dalla guarnizione dello sportello (si vede anche a sportello
    /// chiuso, ogni tanto si muove; a sportello aperto è più chiaro).
    /// Soluzione: aprire lo sportello, tirare la guarnizione (trascinare dall'oblò aperto, o toccare la gomma o il
    /// pezzetto che spunta): il calzino gemello era nella piega; portarlo sullo stendino (zona generosa) -> i due calzini
    /// saltellano e si battono il cinque, "THE SEAL EATS SOCKS.", "EVERYONE KNOWS THAT.", vittoria.
    /// Niente microfono. La logica è in Gotcha.Levels.V64.MissingSockLogic (senza Unity).
    /// Tasti di prova nell'Editor: O sportello della lavatrice, P guarnizione, D asciugatrice, L pelucchi, T gatto,
    /// M gemello sullo stendino, 1-7 calzini della cesta sullo stendino, 8 il calzino del gatto, R calzino sullo stendino,
    /// G detersivo (S = scossone, da Inputs).
    /// </summary>
    public class V73MissingSockLevel : V62Level
    {
        [Header("Lavatrice")]
        [SerializeField] Transform washer;
        [SerializeField] Collider2D washerArea;         // tutta la lavatrice (pannello e fianchi)
        [SerializeField] Transform washerOpening;       // centro dell'oblò
        [SerializeField] Collider2D openingArea;        // l'oblò (generoso: anche la guarnizione e un po' fuori)
        [SerializeField] Transform doorClosed;
        [SerializeField] Transform doorOpen;
        [SerializeField] Transform sealPeel;            // la piega della guarnizione tirata (si vede solo mentre esce il calzino)
        [SerializeField] Transform peek;                // il pezzetto di calzino che spunta dalla guarnizione (l'indizio)
        [SerializeField] Collider2D peekArea;
        [SerializeField] Transform detergent;
        [SerializeField] Collider2D detergentArea;
        [Header("Asciugatrice")]
        [SerializeField] Transform dryer;
        [SerializeField] Collider2D dryerArea;
        [SerializeField] Transform dryerDoorClosed;
        [SerializeField] Transform dryerDoorOpen;
        [SerializeField] Transform lint;
        [SerializeField] Collider2D lintArea;
        [SerializeField] Transform lintRest;            // dove si siede il coniglietto, fuori dall'oblò
        [Header("Stendino")]
        [SerializeField] Transform rackSock;            // il calzino solitario
        [SerializeField] Collider2D rackSockArea;
        [SerializeField] Collider2D rackDropArea;       // dove si lascia un calzino per confrontarlo (generosa)
        [SerializeField] Transform pairSlot;            // accanto al gemello, sotto la molletta vuota
        [SerializeField] Transform sparkle;
        [SerializeField] SpriteRenderer pairGlow;
        [Header("Calzini")]
        [SerializeField] Transform[] basketSocks;
        [SerializeField] Collider2D[] basketSockAreas;
        [SerializeField] Transform basket;
        [SerializeField] Transform petSock;
        [SerializeField] Collider2D petSockArea;
        [SerializeField] Transform petSockRest;
        [SerializeField] Transform matchSock;
        [SerializeField] Collider2D matchSockArea;
        [SerializeField] Transform matchSockRest;       // dove resta il gemello una volta uscito (per terra davanti alla lavatrice)
        [Header("Gatto")]
        [SerializeField] Transform cat;
        [SerializeField] Collider2D catArea;
        [SerializeField] Transform catEyes;
        [SerializeField] Transform zzz;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float TapSlop = 0.6f, TapTime = 0.6f, DrumRadius = 0.42f, DoorEdge = 0.6f;
        const float ShakeLevel = 12f, ShakeCooldown = 1.4f, PeekEvery = 5.5f;
        const int HeldOrder = 40;
        static readonly Color PeekDim = new Color(0.78f, 0.8f, 0.78f), PeekBright = Color.white;
        static readonly Vector3 ZzzOffset = new Vector3(-0.95f, 1.05f, 0f);   // sopra la testa del gatto (coordinate del gatto)

        static readonly string[] WrongLines = { "Bold. But no.", "Daring. Still no.", "The fashion police said no.", "Mismatched is a style. Not THIS style.", "That's a crime in three countries." };
        static readonly string[] CloseLines = { "Dots. Not stars. Nice try, cat.", "Still dots. The cat knows.", "Close. But dots." };
        static readonly string[] EmptyLines = { "Empty. Suspiciously empty.", "Still empty. Suspiciously.", "Nothing in the drum. Allegedly." };
        static readonly string[] DrumLines = { "Hello? ...hello? ...hello?", "Just a damp echo.", "Not even a sock-shaped shadow." };
        static readonly string[] BlockedLines = { "Something's pinched under the door.", "It's squashed shut by the door.", "Can't reach it with the door closed." };
        static readonly string[] NothingLines = { "Nothing else in the seal. Just damp.", "The seal is empty now. For now." };
        static readonly string[] LintLines = { "Just lint. And a lint bunny.", "Still just lint.", "The dryer only makes fluff." };
        static readonly string[] BunnyLines = { "The lint bunny knows nothing.", "Fluffy. Useless.", "It refuses to talk." };
        static readonly string[] CatLines = { "Purr. Zzz.", "Do not disturb.", "The cat has done enough." };
        static readonly string[] ShakeLines = { "Bouncy socks. Nothing new.", "Boing. Same socks.", "Shaking won't make a pair." };
        static readonly string[] RackLines = { "Green with yellow stars. Lonely.", "It misses its twin.", "Green. Yellow stars. One of a kind. Sadly." };
        static readonly string[] BasketLines = { "Single. Like every sock in here.", "Another lonely sock.", "A sock with no future." };
        static readonly string[] DetergentLines = { "Detergent. It cleans socks. It doesn't find them.", "SUDS. Not a sock detector.", "Smells like lavender and failure." };
        static readonly string[] PanelLines = { "No program for finding socks.", "Spin cycle? Nothing to spin.", "The machine blinks innocently." };

        enum Kind { None, Sock, Peek, Opening, Washer, Dryer, Lint, Pet, RackSock, Detergent, Other }

        sealed class Grip
        {
            public Kind kind;
            public int sock = -1;
            public Vector2 start, last;
            public float t0;
            public bool moved;
        }

        /// <summary>Un effetto (rimbalzo, oscillazione, tremolio) in corso su un oggetto e il valore a cui tornare.</summary>
        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        readonly MissingSockLogic logic = new MissingSockLogic();
        Camera cam;
        HwToast toast;
        HwDragger dragger;
        HwDragger.Item[] sockItems;          // 0..6 cesta, 7 gatto, 8 gemello (gli indici della logica)
        Transform[] sockT;
        SpriteRenderer[] sockArt;
        int[] sockOrder;
        Vector3[] sockHome;
        Quaternion[] sockRot;
        Vector3 petSockHidden, catScale, lintScale, peekScale, matchScale, rackSockPos;
        Quaternion petSockHiddenRot, rackSockRot;
        SpriteRenderer peekArt;
        Collider2D[] sockAreas;              // le aree di presa, con gli stessi indici
        Grip grip;
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        int wrongLines, closeLines, emptyLines, drumLines, blockedLines, nothingLines, lintLines, bunnyLines, catLines;
        int shakeLines, rackLines, basketLines, detergentLines, panelLines;
        float shakeReadyAt, peekAt;
        bool busy, finishing, captured;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            logic.Reset();
            ResetV62();
            grip = null;
            busy = finishing = false;
            wrongLines = closeLines = emptyLines = drumLines = blockedLines = nothingLines = lintLines = bunnyLines = catLines = 0;
            shakeLines = rackLines = basketLines = detergentLines = panelLines = 0;
            toast = new HwToast(infoText, infoHolder);
            int n = MissingSockLogic.MatchSock + 1;
            if (!captured)
            {
                captured = true;
                sockT = new Transform[n];
                var areas = new Collider2D[n];
                for (int i = 0; i < MissingSockLogic.BasketSocks; i++) { sockT[i] = basketSocks[i]; areas[i] = basketSockAreas[i]; }
                sockT[MissingSockLogic.PetSock] = petSock;
                areas[MissingSockLogic.PetSock] = petSockArea;
                sockT[MissingSockLogic.MatchSock] = matchSock;
                areas[MissingSockLogic.MatchSock] = matchSockArea;
                sockArt = new SpriteRenderer[n];
                sockOrder = new int[n];
                sockHome = new Vector3[n];
                sockRot = new Quaternion[n];
                for (int i = 0; i < n; i++)
                {
                    sockArt[i] = sockT[i].GetComponentInChildren<SpriteRenderer>(true);
                    sockOrder[i] = sockArt[i] != null ? sockArt[i].sortingOrder : 0;
                    sockHome[i] = sockT[i].position;
                    sockRot[i] = sockT[i].localRotation;
                }
                // il calzino del gatto parte nascosto sotto il cuscino, il gemello parte dentro la guarnizione
                petSockHidden = petSock.position;
                petSockHiddenRot = petSock.localRotation;
                sockHome[MissingSockLogic.PetSock] = petSockRest.position;
                sockRot[MissingSockLogic.PetSock] = petSockRest.localRotation;
                sockHome[MissingSockLogic.MatchSock] = matchSockRest.position;
                sockRot[MissingSockLogic.MatchSock] = matchSockRest.localRotation;
                matchScale = matchSock.localScale;
                catScale = cat.localScale;
                lintScale = lint.localScale;
                peekScale = peek.localScale;
                peekArt = peek.GetComponentInChildren<SpriteRenderer>(true);
                rackSockPos = rackSock.localPosition;
                rackSockRot = rackSock.localRotation;
                sockAreas = areas;
            }
            dragger = new HwDragger(this);
            sockItems = new HwDragger.Item[n];
            for (int i = 0; i < n; i++) sockItems[i] = dragger.Add(sockT[i], sockAreas[i], i);
            sockItems[MissingSockLogic.MatchSock].Priority = 20;
            sockItems[MissingSockLogic.PetSock].Priority = 10;
            shakeReadyAt = Time.time + 0.5f;
            peekAt = Time.time + 2.5f;
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            // lavatrice
            HalloweenKit.Show(doorClosed, !logic.WasherOpen);
            HalloweenKit.Show(doorOpen, logic.WasherOpen);
            HalloweenKit.Show(sealPeel, false);
            HalloweenKit.Show(peek, logic.PeekVisible);
            peek.localScale = peekScale;
            PeekColor();
            if (washer != null) washer.localRotation = Quaternion.identity;
            // asciugatrice e coniglietto
            HalloweenKit.Show(dryerDoorClosed, !logic.DryerOpen);
            HalloweenKit.Show(dryerDoorOpen, logic.DryerOpen);
            HalloweenKit.Show(lint, logic.DryerOpen);
            lint.position = lintRest.position;
            lint.localScale = lintScale;
            // stendino
            rackSock.localPosition = rackSockPos;
            rackSock.localRotation = rackSockRot;
            HalloweenKit.Show(sparkle, false);
            HalloweenKit.SetAlpha(pairGlow, 0f);
            // gatto
            cat.localScale = new Vector3(catScale.x * (logic.PetRevealed ? -1f : 1f), catScale.y, catScale.z);
            cat.localRotation = Quaternion.identity;
            HalloweenKit.Show(catEyes, false);
            HalloweenKit.Show(zzz, true);
            // calzini
            for (int i = 0; i < sockT.Length; i++) SockRest(i);
        }

        void SockRest(int i)
        {
            Transform t = sockT[i];
            dragger.Stop(sockItems[i]);
            if (dragger.Held == sockItems[i]) dragger.Release();
            if (i == MissingSockLogic.MatchSock)
            {
                HalloweenKit.Show(t, logic.SockFound);
                t.localScale = matchScale;
                if (logic.Paired) { t.position = pairSlot.position; t.localRotation = Quaternion.identity; }
                else { t.position = sockHome[i]; t.localRotation = sockRot[i]; }
            }
            else if (i == MissingSockLogic.PetSock && !logic.PetRevealed)
            {
                t.position = petSockHidden;
                t.localRotation = petSockHiddenRot;
            }
            else
            {
                t.position = sockHome[i];
                t.localRotation = sockRot[i];
            }
            if (sockArt[i] != null) sockArt[i].sortingOrder = sockOrder[i];
            sockItems[i].Home = sockHome[i];
            sockItems[i].Enabled = logic.CanHold(i);
        }

        void PeekColor()
        {
            if (peekArt == null) return;
            peekArt.color = logic.WasherOpen ? PeekBright : PeekDim;
            peek.localScale = peekScale * (logic.WasherOpen ? 1.15f : 1f);
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: le animazioni si fermano, si torna a uno stato valido
            CancelGrip();
            busy = false;
            StopFx();
            if (captured && !finishing && dragger != null && !IsFinished) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || dragger == null) return;
            // spento a metà della festa: si rifà il finale da capo (il calzino è già sullo stendino)
            if (logic.Stage == SockStage.Matching) { finishing = false; StartCoroutine(Finale(false)); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrip(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrip(); }

        /// <summary>Il dito sparisce (pausa, focus perso, animazione che parte): il calzino in mano torna a casa.</summary>
        void CancelGrip()
        {
            grip = null;
            if (dragger != null && dragger.Held != null)
            {
                HwDragger.Item it = dragger.Release();
                int i = System.Array.IndexOf(sockItems, it);
                if (i >= 0) SendSockHome(i, 0.2f);
            }
        }

        void Update()
        {
            toast?.Tick();
            if (IsFinished || finishing || toast == null) return;
            if (Time.timeScale <= 0f) { CancelGrip(); return; }   // pausa o pubblicità: niente input
            Ambient();
            if (busy) { CancelGrip(); return; }
#if UNITY_EDITOR
            EditorKeys();
            if (busy || finishing) return;
#endif
            if (Inputs.Shake() > ShakeLevel && Time.time >= shakeReadyAt) { shakeReadyAt = Time.time + ShakeCooldown; Shake(); return; }
            ReadPointer();
        }

        /// <summary>Le piccole cose vive: i "zzz" del gatto e il pezzetto di calzino che ogni tanto si muove.</summary>
        void Ambient()
        {
            if (zzz != null && zzz.gameObject.activeSelf)
            {
                Vector3 p = cat.TransformPoint(ZzzOffset);   // segue la testa anche a gatto girato
                zzz.position = new Vector3(p.x, p.y + 0.06f * Mathf.Sin(Time.time * 2f), zzz.position.z);
            }
            if (logic.PeekVisible && Time.time >= peekAt)
            {
                peekAt = Time.time + PeekEvery + Random.Range(0f, 2f);
                FxWiggle(peek, logic.WasherOpen ? 22f : 14f, 0.45f, 38f);
            }
        }

        // ---------- Effetti che non si accavallano ----------

        // 0 = scala (rimbalzo), 1 = rotazione (oscillazione), 2 = posizione (tremolio/saltello): un effetto nuovo sullo
        // stesso oggetto ferma quello vecchio e riparte dal valore di riposo, così tocchi ripetuti non lasciano niente storto
        void FxPop(Transform t, float peak, float seconds) => Fx(0, t, HalloweenKit.Pop(t, peak, seconds));
        void FxWiggle(Transform t, float degrees, float seconds, float speed) => Fx(1, t, HalloweenKit.Wiggle(t, degrees, seconds, speed));
        void FxShake(Transform t, float amplitude, float seconds) => Fx(2, t, HalloweenKit.Shake(t, amplitude, seconds));
        void FxHop(Transform t, float height, float seconds, float delay) => Fx(2, t, Hop(t, height, seconds, delay));

        static IEnumerator Hop(Transform t, float height, float seconds, float delay)
        {
            if (t == null) yield break;
            Vector3 home = t.localPosition;
            if (delay > 0f) yield return new WaitForSeconds(delay);
            for (float k = 0f; k < 1f; k += Time.deltaTime / Mathf.Max(0.01f, seconds))
            {
                t.localPosition = home + new Vector3(0f, height * Mathf.Sin(k * Mathf.PI), 0f);
                yield return null;
            }
            t.localPosition = home;
        }

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

        /// <summary>Ferma tutti gli effetti (le coroutine sono già ferme o si fermano qui) e rimette i valori di riposo.</summary>
        void StopFx()
        {
            for (int kind = 0; kind < fxSlots.Length; kind++)
            {
                foreach (KeyValuePair<Transform, FxSlot> kv in fxSlots[kind])
                    if (kv.Value.co != null) { StopCoroutine(kv.Value.co); Restore(kind, kv.Key, kv.Value); kv.Value.co = null; }
                fxSlots[kind].Clear();
            }
        }

        /// <summary>Ferma gli effetti su un oggetto che sta per essere spostato (così non torna dove l'effetto l'aveva preso).</summary>
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
            if (Vector2.Distance(w, grip.start) > TapSlop * 0.4f) grip.moved = true;
            if (grip.kind == Kind.Sock && grip.moved && dragger.Held == sockItems[grip.sock]) dragger.MoveHeld(w);
            if (ended && grip != null) End(grip);
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, t0 = Time.time, kind = Kind.Other };
            if (busy || finishing) { g.kind = Kind.None; return g; }
            // prima i calzini che si prendono in mano (il gemello ha la priorità più alta), poi il resto
            HwDragger.Item it = dragger.TryPick(w);
            if (it != null)
            {
                int i = System.Array.IndexOf(sockItems, it);
                StopFxOn(it.T);
                if (sockArt[i] != null) sockArt[i].sortingOrder = HeldOrder;
                g.kind = Kind.Sock;
                g.sock = i;
                return g;
            }
            if (logic.PeekVisible && peekArea.OverlapPoint(w)) { g.kind = Kind.Peek; return g; }
            if (logic.DryerOpen && lintArea.OverlapPoint(w)) { g.kind = Kind.Lint; return g; }
            if (rackSockArea.OverlapPoint(w)) { g.kind = Kind.RackSock; return g; }
            if (openingArea.OverlapPoint(w)) { g.kind = Kind.Opening; return g; }
            if (detergentArea.OverlapPoint(w)) { g.kind = Kind.Detergent; return g; }
            if (washerArea.OverlapPoint(w)) { g.kind = Kind.Washer; return g; }
            if (dryerArea.OverlapPoint(w)) { g.kind = Kind.Dryer; return g; }
            if (catArea.OverlapPoint(w)) { g.kind = Kind.Pet; return g; }
            return g;
        }

        void End(Grip g)
        {
            grip = null;
            bool tap = Vector2.Distance(g.last, g.start) < TapSlop && Time.time - g.t0 <= TapTime + 0.6f;
            switch (g.kind)
            {
                case Kind.Sock: DropSock(g, tap); break;
                case Kind.Peek: PullSeal(); break;   // tocco o trascinamento: è la stoffa che spunta
                case Kind.Opening: Opening(g, tap); break;
                case Kind.Lint: if (tap) TapLint(); break;
                case Kind.RackSock: if (tap) TapRackSock(); break;
                case Kind.Detergent: if (tap) TapDetergent(); break;
                case Kind.Washer:
                    if (!tap) break;
                    // lo sportello aperto sporge oltre l'oblò: toccarlo lo richiude
                    if (logic.WasherOpen && g.start.x < washerOpening.position.x - DoorEdge && g.start.y < washerOpening.position.y + 0.75f) ToggleWasher();
                    else TapPanel();
                    break;
                case Kind.Dryer: if (tap) ToggleDryer(); break;
                case Kind.Pet: if (tap) TapPet(); break;
            }
        }

        /// <summary>
        /// L'oblò: a sportello chiuso un tocco lo apre; aperto, il lato dello sportello lo richiude, il centro è il cestello
        /// e la gomma tutt'intorno (o un trascinamento qualsiasi partito dall'oblò) è la guarnizione da tirare.
        /// </summary>
        void Opening(Grip g, bool tap)
        {
            if (!logic.WasherOpen) { if (tap) ToggleWasher(); return; }
            Vector2 o = washerOpening.position;
            if (g.moved && !tap) { PullSeal(); return; }
            if (!tap) return;
            if (g.start.x < o.x - DoorEdge) { ToggleWasher(); return; }
            if (Vector2.Distance(g.start, o) < DrumRadius) TapDrum();
            else PullSeal();
        }

        // ---------- Lavatrice ----------

        void ToggleWasher()
        {
            SockWasherResult r = logic.ToggleWasher();
            if (r == SockWasherResult.None) return;
            V73MissingSockSound.Play(this, "door", 0.6f);
            Haptics.Light();
            HalloweenKit.Show(doorClosed, !logic.WasherOpen);
            HalloweenKit.Show(doorOpen, logic.WasherOpen);
            PeekColor();
            FxShake(washer, 0.02f, 0.2f);
            if (r == SockWasherResult.OpenedEmpty)
            {
                FxPop(doorOpen, 1.06f, 0.2f);
                toast.Show(EmptyLines[emptyLines++ % EmptyLines.Length], 1.8f);
                if (logic.PeekVisible) peekAt = Time.time + 1.2f;   // appena aperto, il pezzetto si muove presto
            }
        }

        void TapDrum()
        {
            if (logic.TapDrum() == SockDrumResult.None) return;
            Sfx.Play(this, "tick", 0.5f);
            toast.Show(DrumLines[drumLines++ % DrumLines.Length], 1.6f);
        }

        void TapPanel()
        {
            Sfx.Play(this, "tick", 0.5f);
            FxShake(washer, 0.015f, 0.15f);
            toast.Show(PanelLines[panelLines++ % PanelLines.Length], 1.6f);
        }

        void TapDetergent()
        {
            if (!logic.TapDetergent()) return;
            FxWiggle(detergent, 7f, 0.4f, 28f);
            V73MissingSockSound.Play(this, "rattle", 0.5f);
            toast.Show(DetergentLines[detergentLines++ % DetergentLines.Length], 1.9f);
        }

        void PullSeal()
        {
            SockSealResult r = logic.PullSeal();
            switch (r)
            {
                case SockSealResult.None: return;
                case SockSealResult.Blocked:
                    FxWiggle(peek, 20f, 0.35f, 40f);
                    V73MissingSockSound.Play(this, "rubber", 0.4f);
                    toast.Show(BlockedLines[blockedLines++ % BlockedLines.Length], 1.8f);
                    return;
                case SockSealResult.Nothing:
                    V73MissingSockSound.Play(this, "rubber", 0.4f);
                    toast.Show(NothingLines[nothingLines++ % NothingLines.Length], 1.6f);
                    return;
                case SockSealResult.Found:
                    StartCoroutine(SockOut());
                    return;
            }
        }

        /// <summary>La guarnizione si piega, il calzino gemello esce stirandosi e cade per terra davanti alla lavatrice.</summary>
        IEnumerator SockOut()
        {
            busy = true;
            CancelGrip();
            StopFxOn(peek);
            HalloweenKit.Show(sealPeel, true);
            V73MissingSockSound.Play(this, "rubber", 0.7f);
            Haptics.Medium();
            toast.Show("WAIT. Something's in the seal...", 1.6f);
            yield return new WaitForSeconds(0.35f);
            HalloweenKit.Show(peek, false);
            int i = MissingSockLogic.MatchSock;
            HalloweenKit.Show(matchSock, true);
            if (sockArt[i] != null) sockArt[i].sortingOrder = HeldOrder;
            matchSock.position = new Vector3(peek.position.x, peek.position.y, matchSock.position.z);
            matchSock.localRotation = peek.localRotation;
            matchSock.localScale = matchScale * 0.3f;
            V73MissingSockSound.Play(this, "boing", 0.6f);
            StartCoroutine(HalloweenKit.ScaleTo(matchSock, matchScale, 0.45f));
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                matchSock.localRotation = Quaternion.Slerp(peek.localRotation, sockRot[i], k);
                yield return null;
            }
            yield return HalloweenKit.Move(matchSock, sockHome[i], 0.4f, 0.5f);
            matchSock.localScale = matchScale;
            matchSock.localRotation = sockRot[i];
            HalloweenKit.Show(sealPeel, false);
            if (sockArt[i] != null) sockArt[i].sortingOrder = sockOrder[i];
            sockItems[i].Home = sockHome[i];
            sockItems[i].Enabled = logic.CanHold(i);
            FxPop(matchSock, 1.25f, 0.3f);
            Sfx.Play(this, "ding", 0.45f);
            toast.Show("GREEN. YELLOW STARS. The seal was hiding it!", 2.2f);
            busy = false;
        }

        // ---------- Asciugatrice ----------

        void ToggleDryer()
        {
            SockDryerResult r = logic.ToggleDryer();
            if (r == SockDryerResult.None) return;
            V73MissingSockSound.Play(this, "door", 0.5f);
            Haptics.Light();
            HalloweenKit.Show(dryerDoorClosed, !logic.DryerOpen);
            HalloweenKit.Show(dryerDoorOpen, logic.DryerOpen);
            HalloweenKit.Show(lint, logic.DryerOpen);
            FxShake(dryer, 0.02f, 0.2f);
            if (r != SockDryerResult.OpenedLint) return;
            lint.position = lintRest.position;
            lint.localScale = lintScale;
            V73MissingSockSound.Play(this, "fluff", 0.5f);
            FxPop(lint, 1.3f, 0.3f);
            toast.Show(LintLines[lintLines++ % LintLines.Length], 1.8f);
        }

        void TapLint()
        {
            if (!logic.TapLint()) return;
            V73MissingSockSound.Play(this, "fluff", 0.45f);
            FxHop(lint, 0.12f, 0.3f, 0f);
            toast.Show(BunnyLines[bunnyLines++ % BunnyLines.Length], 1.6f);
        }

        // ---------- Gatto ----------

        void TapPet()
        {
            SockPetResult r = logic.TapPet();
            if (r == SockPetResult.None) return;
            if (r == SockPetResult.RevealsSock) { StartCoroutine(RollOver()); return; }
            FxWiggle(cat, 3f, 0.4f, 20f);
            Sfx.Play(this, "snore", 0.4f);
            toast.Show(CatLines[catLines++ % CatLines.Length], 1.4f);
        }

        IEnumerator RollOver()
        {
            busy = true;
            CancelGrip();
            Sfx.Play(this, "snore", 0.5f);
            toast.Show("Mrrrp. The cat rolls over...", 1.3f);
            Vector3 home = cat.localPosition;
            float s0 = cat.localScale.x;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.45f)
            {
                cat.localScale = new Vector3(Mathf.Lerp(s0, -s0, k), catScale.y * (1f + 0.08f * Mathf.Sin(k * Mathf.PI)), catScale.z);
                cat.localPosition = home + new Vector3(0f, 0.12f * Mathf.Sin(k * Mathf.PI), 0f);
                yield return null;
            }
            cat.localScale = new Vector3(-s0, catScale.y, catScale.z);
            cat.localPosition = home;
            // il calzino scivola fuori da sotto il cuscino
            int i = MissingSockLogic.PetSock;
            if (sockArt[i] != null) sockArt[i].sortingOrder = sockOrder[i] + 4;
            V73MissingSockSound.Play(this, "boing", 0.4f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                petSock.position = Vector3.Lerp(petSockHidden, sockHome[i], 1f - (1f - k) * (1f - k));
                petSock.localRotation = Quaternion.Slerp(petSockHiddenRot, sockRot[i], k);
                yield return null;
            }
            SockRest(i);
            FxPop(petSock, 1.25f, 0.3f);
            toast.Show("A green and yellow sock! ...with dots. Nice try, cat.", 2.4f);
            busy = false;
        }

        // ---------- Calzini e stendino ----------

        void Shake()
        {
            if (!logic.Shake()) return;
            Haptics.Medium();
            V73MissingSockSound.Play(this, "boing", 0.5f);
            for (int i = 0; i < MissingSockLogic.BasketSocks; i++) FxHop(basketSocks[i], 0.25f + 0.05f * (i % 3), 0.3f, i * 0.04f);
            FxShake(basket, 0.05f, 0.35f);
            toast.Show(ShakeLines[shakeLines++ % ShakeLines.Length], 1.6f);
        }

        void TapRackSock()
        {
            if (!logic.TapRackSock()) return;
            FxWiggle(rackSock, 8f, 0.5f, 18f);
            toast.Show(RackLines[rackLines++ % RackLines.Length], 1.8f);
        }

        bool OnRack(Vector2 sockPos, Vector2 finger) => rackDropArea.OverlapPoint(sockPos) || rackDropArea.OverlapPoint(finger);

        void DropSock(Grip g, bool tap)
        {
            int i = g.sock;
            if (dragger.Held == sockItems[i]) dragger.Release();
            if (!g.moved || tap && !OnRack(sockT[i].position, g.last))
            {
                SendSockHome(i, 0.15f);
                TapSock(i);
                return;
            }
            if (!OnRack(sockT[i].position, g.last)) { SendSockHome(i, 0.3f); return; }
            SockPairResult r = logic.TryPair(i);
            switch (r)
            {
                case SockPairResult.None: SendSockHome(i, 0.3f); return;
                case SockPairResult.Match: StartCoroutine(Finale(true)); return;
            }
            StartCoroutine(Critic(i, r));
        }

        /// <summary>Il calzino sbagliato appeso un attimo accanto al gemello, la critica di moda, poi torna a casa.</summary>
        IEnumerator Critic(int i, SockPairResult r)
        {
            busy = true;
            Transform t = sockT[i];
            if (sockArt[i] != null) sockArt[i].sortingOrder = HeldOrder;
            yield return HalloweenKit.Move(t, pairSlot.position, 0.15f);
            t.localRotation = Quaternion.identity;
            V73MissingSockSound.Play(this, "boing", 0.35f);
            FxWiggle(rackSock, 14f, 0.5f, 22f);   // il gemello si scosta, offeso
            Haptics.Light();
            if (r == SockPairResult.Close) toast.Show(CloseLines[closeLines++ % CloseLines.Length], 1.9f);
            else toast.Show(WrongLines[wrongLines++ % WrongLines.Length], 1.7f);
            yield return new WaitForSeconds(0.55f);
            busy = false;
            SendSockHome(i, 0.35f);
        }

        void TapSock(int i)
        {
            FxHop(sockT[i], 0.15f, 0.25f, 0f);
            if (i == MissingSockLogic.MatchSock) toast.Show("That's the one! Hang it next to its twin.", 1.8f);
            else if (i == MissingSockLogic.PetSock) toast.Show("Dots. Still warm from the cat.", 1.6f);
            else toast.Show(BasketLines[basketLines++ % BasketLines.Length], 1.5f);
        }

        void SendSockHome(int i, float seconds)
        {
            HwDragger.Item it = sockItems[i];
            it.Home = sockHome[i];
            sockT[i].localRotation = sockRot[i];
            if (sockArt[i] != null) sockArt[i].sortingOrder = sockOrder[i];
            dragger.SendTo(it, sockHome[i], seconds, 0.15f);
        }

        // ---------- La soluzione ----------

        IEnumerator Finale(bool fresh)
        {
            finishing = true;
            busy = false;
            CancelGrip();
            StopFx();
            int i = MissingSockLogic.MatchSock;
            HalloweenKit.Show(matchSock, true);
            if (sockArt[i] != null) sockArt[i].sortingOrder = sockOrder[i] + 4;
            matchSock.localScale = matchScale;
            rackSock.localPosition = rackSockPos;
            rackSock.localRotation = rackSockRot;
            if (fresh)
            {
                yield return HalloweenKit.Move(matchSock, pairSlot.position, 0.2f);
                Haptics.Medium();
            }
            matchSock.position = pairSlot.position;
            matchSock.localRotation = Quaternion.identity;
            V73MissingSockSound.Play(this, "boing", 0.6f);
            // saltellano a turno
            for (int k = 0; k < 4; k++)
            {
                Transform t = k % 2 == 0 ? rackSock : matchSock;
                V73MissingSockSound.Play(this, "boing", 0.35f + 0.05f * k);
                yield return Hop(t, 0.22f, 0.22f, 0f);
            }
            // il cinque: i piedi si toccano
            Quaternion r0 = rackSock.localRotation, m0 = matchSock.localRotation;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f)
            {
                rackSock.localRotation = r0 * Quaternion.Euler(0f, 0f, 28f * k);
                matchSock.localRotation = m0 * Quaternion.Euler(0f, 0f, -28f * k);
                yield return null;
            }
            V73MissingSockSound.Play(this, "clap", 0.8f);
            Haptics.Success();
            if (sparkle != null)
            {
                sparkle.position = new Vector3((rackSock.position.x + matchSock.position.x) * 0.5f, rackSock.position.y - 0.6f, sparkle.position.z);
                HalloweenKit.Show(sparkle, true);
                FxPop(sparkle, 1.6f, 0.4f);
            }
            if (pairGlow != null) StartCoroutine(HalloweenKit.Fade(pairGlow, 0.85f, 0.25f));
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                rackSock.localRotation = Quaternion.Slerp(r0 * Quaternion.Euler(0f, 0f, 28f), r0, k);
                matchSock.localRotation = Quaternion.Slerp(m0 * Quaternion.Euler(0f, 0f, -28f), m0, k);
                yield return null;
            }
            toast.Show("THE SEAL EATS SOCKS.", 1.5f);
            yield return new WaitForSeconds(1.3f);
            toast.Show("EVERYONE KNOWS THAT.", 2.2f);
            yield return Celebrate(1.4f);
            if (logic.TryComplete()) Win();
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (Input.GetKeyDown(KeyCode.O)) ToggleWasher();
            if (Input.GetKeyDown(KeyCode.P)) PullSeal();
            if (busy) return;
            if (Input.GetKeyDown(KeyCode.D)) ToggleDryer();
            if (Input.GetKeyDown(KeyCode.L)) TapLint();
            if (Input.GetKeyDown(KeyCode.T)) TapPet();
            if (Input.GetKeyDown(KeyCode.R)) TapRackSock();
            if (Input.GetKeyDown(KeyCode.G)) TapDetergent();
            if (busy) return;
            for (int i = 0; i <= MissingSockLogic.PetSock; i++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SockPairResult r = logic.TryPair(i);
                    if (r == SockPairResult.Wrong || r == SockPairResult.Close) StartCoroutine(Critic(i, r));
                    return;
                }
            if (Input.GetKeyDown(KeyCode.M) && logic.TryPair(MissingSockLogic.MatchSock) == SockPairResult.Match)
                StartCoroutine(Finale(true));
        }
#endif
    }

    /// <summary>Suoni sintetizzati del livello 272: sportello, gomma della guarnizione, saltelli, pelucchi, il cinque.</summary>
    public static class V73MissingSockSound
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
                case "door":     // sportello dell'oblò: clac della chiusura e tonfo sordo
                    c = Make(kind, 0.22f, t => (Mathf.Sin(t * 2f * Mathf.PI * 140f) * 0.6f * Mathf.Exp(-t * 22f)) + N() * 0.35f * Mathf.Exp(-t * 90f));
                    break;
                case "rubber":   // gomma tirata: un cigolio che sale
                    c = Make(kind, 0.35f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(380f, 720f, t / 0.35f) * (1f + 0.04f * Mathf.Sin(t * 180f))) * 0.3f * Mathf.Sin(t / 0.35f * Mathf.PI));
                    break;
                case "boing":    // saltello del calzino
                    c = Make(kind, 0.25f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(260f, 520f, t / 0.25f)) * 0.35f * Mathf.Exp(-t * 9f));
                    break;
                case "fluff":    // pelucchi: soffio morbido
                    {
                        float lp = 0f;
                        c = Make(kind, 0.3f, t => { lp += (N() - lp) * 0.05f; return lp * 2.2f * Mathf.Sin(t / 0.3f * Mathf.PI); });
                    }
                    break;
                case "rattle":   // detersivo in polvere nella scatola
                    c = Make(kind, 0.3f, t => N() * 0.3f * (0.5f + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 18f)) * Mathf.Sin(t / 0.3f * Mathf.PI));
                    break;
                case "clap":     // il cinque: schiocco secco
                    c = Make(kind, 0.15f, t => N() * 0.6f * Mathf.Exp(-t * 45f) + Mathf.Sin(t * 2f * Mathf.PI * 1200f) * 0.15f * Mathf.Exp(-t * 60f));
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
            AudioClip clip = AudioClip.Create("v73_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
