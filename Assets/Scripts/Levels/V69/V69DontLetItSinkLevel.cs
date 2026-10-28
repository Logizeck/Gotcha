using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 265 "Don't Let It Sink" — "DON'T LET THE BOAT SINK!" Mare aperto: una barchetta col capitano (il topo)
    /// imbarca acqua da un buco e l'indicatore sale piano (circa 75 secondi). Ma il "mare" è una vasca da bagno: il cielo
    /// è fatto di piastrelle, l'orizzonte è il bordo di smalto, c'è una paperella di gomma, la schiuma, le bolle e
    /// nell'angolo in basso a sinistra l'anello di una catenella che scende fino al tappo.
    /// False piste (nessuna azzera i progressi): il secchio sulla barchetta (il livello scende un po', poi l'acqua torna),
    /// il dito sul buco (tiene solo finché lo si tiene), il nastro adesivo (si inzuppa e si stacca), la paperella
    /// (squittisce e guarda la catenella), inclinare/scuotere il telefono (onde, mal di mare), toccare il mare e il cielo.
    /// Se la barchetta si riempie non si perde: va a fondo e torna su vuota ("Nope. Try again.").
    /// Soluzione: tirare la catenella (trascinarla via dall'angolo, in su o di lato): il tappo esce, il mare se ne va con
    /// un gorgoglio e la barchetta si posa sul fondo della vasca. Lasciata a metà, la catenella torna giù: niente si perde.
    /// Niente microfono. La logica è in Gotcha.Levels.V64.DontLetItSinkLogic (senza Unity): qui solo input e presentazione.
    /// Tasti di prova nell'Editor: K tira la catenella fino in fondo, J tirata a metà, 1 secchio, 2 nastro, 3 paperella,
    /// H (tenuto) dito sul buco, F tanta acqua di colpo (per vederla affondare); S = scossone, frecce = inclinazione.
    /// </summary>
    public class V69DontLetItSinkLevel : V62Level
    {
        [Header("Mare (vasca)")]
        [SerializeField] Transform water;               // perno sul fondo della vasca: la scala y è l'altezza dell'acqua
        [SerializeField] Transform surface;             // il pelo dell'acqua con schiuma e bolle
        [SerializeField] Transform waves;               // le onde davanti alla barchetta
        [SerializeField] SpriteRenderer wavesArt;
        [SerializeField] Collider2D seaArea;
        [SerializeField] Collider2D skyArea;
        [Header("Barchetta")]
        [SerializeField] Transform boat;                // 0,0 = linea di galleggiamento
        [SerializeField] Collider2D boatArea;
        [SerializeField] SpriteRenderer hull;
        [SerializeField] Transform captain;
        [SerializeField] SpriteRenderer captainArt;
        [SerializeField] Collider2D captainArea;
        [SerializeField] SpeechBubble captainBubble;
        [SerializeField] SmokeEmitter spit;
        [SerializeField] Transform gaugeFill;           // perno in basso: la scala y è il livello dell'acqua a bordo
        [SerializeField] SpriteRenderer gaugeBar;
        [SerializeField] Transform hole;
        [SerializeField] Collider2D holeArea;
        [SerializeField] Transform spurt;
        [SerializeField] Transform patch;
        [SerializeField] Transform finger;
        [Header("Oggetti")]
        [SerializeField] Transform bucket;
        [SerializeField] Collider2D bucketArea;
        [SerializeField] Transform tape;
        [SerializeField] Collider2D tapeArea;
        [SerializeField] Transform duck;
        [SerializeField] Collider2D duckArea;
        [Header("Catenella e tappo")]
        [SerializeField] Transform chainEnd;
        [SerializeField] Collider2D chainArea;
        [SerializeField] SpriteRenderer glint;
        [SerializeField] Transform[] links;
        [SerializeField] Transform plug;
        [SerializeField] Transform swirl;
        [SerializeField] SmokeEmitter drainBubbles;
        [SerializeField] SmokeEmitter spray;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float TapTime = 0.45f, TapSlop = 0.25f;
        const float Floor = -4.72f;
        const float BoatRise = 0.1f, BoatDraft = 0.3f;       // la barchetta vuota sta più su; piena pesca 0.3 di più
        const float BoatOnFloor = Floor + 0.42f;            // la chiglia (a -0.4) appoggiata sul fondo
        const float DuckOnFloor = Floor + 0.08f;
        const float GaugeHeight = 0.84f;
        const float PullDistance = 1.5f, LinkGrab = 0.35f;
        const float ShakeLevel = 12f, ShakeCooldown = 2f;
        const float DrainSeconds = 1.7f;
        static readonly Color GaugeLow = new Color(0.3f, 0.75f, 0.95f), GaugeHigh = new Color(0.95f, 0.3f, 0.25f);
        static readonly Color Seasick = new Color(0.62f, 0.9f, 0.55f);
        static readonly Color Underwater = new Color(0.55f, 0.75f, 0.95f, 0.75f);

        static readonly string[] BailLines = { "Bailing the ocean. Bold.", "The ocean has more where that came from.", "Bucket: 1. Ocean: everything." };
        static readonly string[] RushLines = { "...and it splashes right back in.", "Splash. Back in.", "The sea returns your water." };
        static readonly string[] FingerLines = { "Your finger can't stay there forever.", "It leaks the moment you let go.", "Great plan. Until you need that finger." };
        static readonly string[] TapeLines = { "Taped! Problem solved...?", "More tape. Surely this time.", "Tape: the answer to everything." };
        static readonly string[] PeelLines = { "Soggy tape. It peeled right off.", "Tape and water: not friends.", "It held for three whole seconds." };
        static readonly string[] DuckLines = { "Squeak.", "Squeak! It keeps staring at that corner.", "A rubber duck. At sea. Hmm." };
        static readonly string[] RockLines = { "Waves! The captain is seasick.", "Stop rocking the sea!", "The captain is turning green." };
        static readonly string[] SickSay = { "Blergh.", "I'm a captain, not a sailor!", "Make it stop..." };
        static readonly string[] CaptainSay = { "Captain's log: still sinking.", "I can't swim!", "Women and mice first!" };
        static readonly string[] SeaLines = { "The sea is... warm?", "It smells like lavender.", "Sea foam. Or bubble bath?" };
        static readonly string[] SkyLines = { "The sky has grout lines.", "Nice horizon. Very... shiny.", "Weather today: tiled." };
        static readonly string[] BoatLines = { "A very small boat. A very real hole.", "It says S.S. BUBBLES.", "Knock knock. Glub." };
        static readonly string[] ChainLines = { "A chain... going down to something.", "It leads down into the deep.", "Something's on the other end." };
        static readonly string[] SnapLines = { "It slipped back down. Pull it all the way.", "Almost... it sank back.", "Something down there is heavy. Pull harder." };

        enum Kind { None, Item, Hole, Chain, Other }

        sealed class Grip { public Kind kind; public Vector2 start, last; public float t0; public bool moved; }

        readonly DontLetItSinkLogic logic = new DontLetItSinkLogic();
        readonly Dictionary<int, Grip> grips = new Dictionary<int, Grip>();
        readonly List<int> gone = new List<int>(4);
        Camera cam;
        HwToast toast;
        HwSay say;
        HwDragger dragger;
        HwDragger.Item bucketItem, tapeItem;
        Vector3 boatHome, duckHome, duckScale, chainHome, plugHome, bucketHome, tapeHome, captainScale, patchScale, spurtScale;
        Vector3 plugTopOffset;
        Color hullColor, captainColor, patchColor, wavesColor;
        float seaY, sinkDrop, rockAmp, sickUntil, revealUntil, shakeReadyAt, dripAt, duckLookUntil, chainPull;
        int bailLines, fingerLines, tapeLines, peelLines, duckLines, rockLines, captainLines, seaLines, skyLines, boatLines, chainLines, snapLines;
        bool finishing, sinking, editorFinger, chainSnapping, peeling;
        Coroutine rockRun;
        SpriteRenderer[] linkArt;

        float Sea0 => surfaceHome.y;
        Vector3 surfaceHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            logic.Reset();
            ResetV62();
            V64AFingers.Reset();
            grips.Clear();
            finishing = sinking = editorFinger = chainSnapping = peeling = false;
            rockRun = null;
            bailLines = fingerLines = tapeLines = peelLines = duckLines = rockLines = captainLines = seaLines = skyLines = boatLines = chainLines = snapLines = 0;
            toast = new HwToast(infoText, infoHolder);
            say = new HwSay(captainBubble);
            if (links == null) links = new Transform[0];
            if (linkArt == null || linkArt.Length != links.Length)
            {
                linkArt = new SpriteRenderer[links.Length];
                for (int i = 0; i < links.Length; i++) linkArt[i] = links[i] != null ? links[i].GetComponentInChildren<SpriteRenderer>(true) : null;
            }
            if (spurtScale == Vector3.zero)
            {
                surfaceHome = surface.position;
                boatHome = boat.position;
                duckHome = duck.position;
                duckScale = duck.localScale;
                chainHome = chainEnd.position;
                plugHome = plug.position;
                plugTopOffset = new Vector3(0f, 0.22f, 0f);
                bucketHome = bucket.position;
                tapeHome = tape.position;
                captainScale = captain.localScale;
                patchScale = patch.localScale;
                spurtScale = spurt.localScale;
                hullColor = hull.color;
                captainColor = captainArt.color;
                SpriteRenderer pa = patch.GetComponentInChildren<SpriteRenderer>(true);
                patchColor = pa != null ? pa.color : Color.white;
                wavesColor = wavesArt.color;
            }
            dragger = new HwDragger(this);
            bucketItem = dragger.Add(bucket, bucketArea, 1);
            bucketItem.Home = bucketHome;
            tapeItem = dragger.Add(tape, tapeArea, 1);
            tapeItem.Home = tapeHome;
            shakeReadyAt = Time.time + 0.6f;
            dripAt = Time.time + 1.2f;
            RestPose();
            toast.Show("Uh oh. The boat has a hole.", 2f);
        }

        /// <summary>Tutto al suo posto secondo la logica (mare pieno, barchetta a galla, catenella a riposo).</summary>
        void RestPose()
        {
            seaY = Sea0;
            sinkDrop = rockAmp = chainPull = 0f;
            sickUntil = revealUntil = duckLookUntil = -1f;
            sinking = chainSnapping = false;
            hull.color = hullColor;
            captainArt.color = captainColor;
            captain.localScale = captainScale;
            captain.localRotation = Quaternion.identity;
            boat.localRotation = Quaternion.identity;
            duck.localScale = duckScale;
            duck.localRotation = Quaternion.identity;
            HalloweenKit.Show(surface, true);
            HalloweenKit.Show(waves, true);
            wavesArt.color = wavesColor;
            bucket.position = bucketHome;
            bucket.localRotation = Quaternion.identity;
            tape.position = tapeHome;
            if (bucketItem != null) bucketItem.Enabled = true;
            if (tapeItem != null) tapeItem.Enabled = true;
            patch.localScale = patchScale;
            patch.localPosition = Vector3.zero;
            patch.localRotation = Quaternion.identity;
            SetPatchColor(patchColor);
            HalloweenKit.Show(patch, logic.Patched);
            HalloweenKit.Show(finger, logic.FingerOnHole);
            plug.position = plugHome;
            plug.localRotation = Quaternion.identity;
            chainEnd.position = chainHome;
            HalloweenKit.Show(swirl, false);
            HalloweenKit.SetAlpha(glint, 0f);
            if (drainBubbles != null) { drainBubbles.Stop(); drainBubbles.transform.position = plugHome + new Vector3(0f, 0.25f, 0f); }
            if (spray != null) spray.Stop();
            if (spit != null) spit.Stop();
            say?.Hide();
            Pose(0f);
        }

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: le animazioni si fermano con l'oggetto, si torna a uno stato valido
            CancelGrips();
            rockRun = null;
            peeling = false;
            if (spurtScale == Vector3.zero) return;
            if (logic.Stage == SinkStage.Floating) RestPose();   // affondamento e svuotamento si chiudono in OnEnable
        }

        void OnEnable()
        {
            if (spurtScale == Vector3.zero || IsFinished) return;
            if (logic.Stage == SinkStage.Sinking) { logic.Resurface(); RestPose(); return; }
            // spento a metà dello svuotamento: la vasca è comunque vuota
            if (logic.Stage == SinkStage.Draining || logic.Stage == SinkStage.Landed)
            {
                logic.FinishDrain();
                DrainedPose();
                StartCoroutine(Landed());
            }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrips(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrips(); }

        /// <summary>Si lascia tutto: il dito si stacca dal buco, la catenella torna giù, l'oggetto preso torna a posto.</summary>
        void CancelGrips()
        {
            bool hadChain = HasChainGrip();
            grips.Clear();
            editorFinger = false;
            if (dragger != null && dragger.Held != null) dragger.Cancel();
            if (logic.Stage != SinkStage.Floating) return;
            logic.SetFinger(false);
            if (hadChain || logic.Pull > 0f || chainPull > 0f) ReleaseChain(false);
        }

        void Update()
        {
            toast?.Tick();
            say?.Tick();
            if (IsFinished || finishing) return;
            if (Time.timeScale <= 0f) { CancelGrips(); return; }   // pausa o pubblicità: niente input
            float dt = Time.deltaTime;
            Vector2 tilt = Inputs.Tilt();
            if (logic.FeedTilt(tilt.x, tilt.y, dt)) RockTheSea();
            if (Inputs.Shake() > ShakeLevel && Time.time >= shakeReadyAt) { shakeReadyAt = Time.time + ShakeCooldown; RockTheSea(); }
            switch (logic.Tick(dt))
            {
                case SinkEvent.PatchPeeled: StartCoroutine(PeelPatch()); break;
                case SinkEvent.BoatFull: StartCoroutine(SinkAndResurface()); break;
            }
            Pose(dt);
            if (logic.InputLocked || sinking) { CancelGrips(); return; }
#if UNITY_EDITOR
            EditorKeys();
            if (logic.InputLocked || finishing) return;
#endif
            ReadFingers();
        }

        // ---------- Posa di ogni fotogramma ----------

        void Pose(float dt)
        {
            float t = Time.time;
            // il mare (pelo, onde, acqua)
            surface.position = new Vector3(surfaceHome.x + Mathf.Sin(t * 0.7f) * 0.04f * (1f + rockAmp * 3f), seaY, surfaceHome.z);
            waves.position = new Vector3(surfaceHome.x - Mathf.Sin(t * 0.9f) * 0.06f * (1f + rockAmp * 3f), seaY, waves.position.z);
            water.localScale = new Vector3(1f, Mathf.Max(0.001f, seaY - Floor), 1f);
            // la barchetta: più è piena più pesca, dondola con le onde e con l'inclinazione del telefono
            float lvl = logic.Level;
            float bob = Mathf.Sin(t * 1.6f) * 0.035f * (1f + rockAmp * 2f);
            float y = Mathf.Max(BoatOnFloor, seaY + BoatRise - BoatDraft * lvl + bob) - sinkDrop;
            boat.position = new Vector3(boatHome.x, y, boatHome.z);
            if (!finishing)
            {
                float roll = -logic.TiltX * 6f + Mathf.Sin(t * 1.3f) * 1.5f + Mathf.Sin(t * 4.2f) * 9f * rockAmp;
                boat.localRotation = Quaternion.Euler(0f, 0f, roll);
            }
            // l'indicatore
            gaugeFill.localScale = new Vector3(1f, Mathf.Max(0.001f, lvl * GaugeHeight), 1f);
            gaugeBar.color = Color.Lerp(GaugeLow, GaugeHigh, Mathf.InverseLerp(0.45f, 0.9f, lvl));
            // il buco: getto, dito, toppa
            bool leaking = logic.Leaking && !sinking;
            HalloweenKit.Show(spurt, leaking);
            if (leaking) spurt.localScale = spurtScale * (0.85f + 0.15f * Mathf.Sin(t * 14f) + 0.25f * lvl);
            HalloweenKit.Show(finger, logic.FingerOnHole);
            bool patched = logic.Patched;
            if (!patched && !peeling) HalloweenKit.Show(patch, false);
            if (patched)
            {
                float soggy = 1f - Mathf.Clamp01(logic.PatchLeft / 1.5f);
                SetPatchColor(Color.Lerp(patchColor, new Color(0.45f, 0.5f, 0.58f, 0.95f), soggy));
                patch.localRotation = Quaternion.Euler(0f, 0f, -14f * soggy);
            }
            if (leaking && Time.time >= dripAt) { dripAt = Time.time + Mathf.Lerp(1.6f, 0.7f, lvl); V69DontLetItSinkSound.Play(this, "drip", 0.25f); }
            // la paperella
            duck.position = new Vector3(duckHome.x, Mathf.Max(DuckOnFloor, seaY - 0.02f + Mathf.Sin(t * 1.9f + 1f) * 0.03f * (1f + rockAmp * 2f)), duckHome.z);
            if (duckLookUntil > 0f && t >= duckLookUntil) { duckLookUntil = -1f; duck.localScale = duckScale; duck.localRotation = Quaternion.identity; }
            // il capitano col mal di mare
            if (sickUntil > 0f && t >= sickUntil) { sickUntil = -1f; captainArt.color = captainColor; }
            // la catenella: l'anello luccica ogni tanto; mentre la si segue si vede fino al tappo
            if (!finishing)
            {
                float g = Mathf.Repeat(t, 4.5f);
                HalloweenKit.SetAlpha(glint, g < 0.5f ? Mathf.Sin(g / 0.5f * Mathf.PI) * 0.9f : 0f);
                glint.transform.localRotation = Quaternion.Euler(0f, 0f, t * 60f);
            }
            PoseChain();
        }

        /// <summary>Le maglie dal tappo all'anello: pendono, e si tendono quanto più la catenella è tirata.</summary>
        void PoseChain()
        {
            if (links.Length == 0 || linkArt == null) return;
            Vector3 top = plug.position + plugTopOffset;
            Vector3 end = chainEnd.position;
            float reveal = finishing || Time.time < revealUntil || chainPull > 0f ? 1f : 0f;
            float sag = 0.35f * (1f - Mathf.Clamp01(chainPull * 1.4f));
            for (int i = 0; i < links.Length; i++)
            {
                if (links[i] == null) continue;
                float k = (i + 1f) / (links.Length + 1f);
                Vector3 p = Vector3.Lerp(top, end, k) + Vector3.down * (Mathf.Sin(k * Mathf.PI) * sag);
                links[i].position = new Vector3(p.x, p.y, links[i].position.z);
                Vector3 dir = end - top;
                float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                links[i].localRotation = Quaternion.Euler(0f, 0f, ang);
                links[i].localScale = new Vector3(1f, i % 2 == 0 ? 1f : 0.55f, 1f);
                // sott'acqua profondo si vede appena: seguendola si vede tutta
                float deep = Mathf.Lerp(0.12f, 0.85f, k);
                HalloweenKit.SetAlpha(linkArt[i], Mathf.Lerp(deep, 0.95f, reveal));
            }
        }

        void SetPatchColor(Color c)
        {
            SpriteRenderer sr = patch.GetComponentInChildren<SpriteRenderer>(true);
            if (sr != null) sr.color = c;
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
                if (g.kind == Kind.Item) dragger.MoveHeld(f.world);
                else if (g.kind == Kind.Chain) DragChain(g);
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

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, t0 = Time.time, kind = Kind.Other };
            if (finishing || logic.InputLocked) { g.kind = Kind.None; return g; }
            if (!HasChainGrip() && !chainSnapping && NearChain(w)) { g.kind = Kind.Chain; GrabChain(); return g; }
            if (dragger.Held == null && dragger.TryPick(w) != null) { g.kind = Kind.Item; return g; }
            if (holeArea.OverlapPoint(w) && !logic.FingerOnHole) { g.kind = Kind.Hole; PlugWithFinger(); return g; }
            return g;
        }

        void End(int id, Grip g)
        {
            grips.Remove(id);
            bool tap = !g.moved && Time.time - g.t0 <= TapTime;
            switch (g.kind)
            {
                case Kind.Item: DropItem(tap, g.last); break;
                case Kind.Hole: UnplugFinger(); break;
                case Kind.Chain: if (tap && chainPull < 0.1f) { ReleaseChain(false); FollowChain(); } else ReleaseChain(true); break;
                case Kind.Other: if (tap) TapScene(g.start); break;
            }
        }

        bool HasChainGrip()
        {
            foreach (Grip g in grips.Values) if (g.kind == Kind.Chain) return true;
            return false;
        }

        /// <summary>L'anello (area larga) o una qualunque maglia ben visibile (nella metà verso l'anello).</summary>
        bool NearChain(Vector2 w)
        {
            if (chainArea.OverlapPoint(w)) return true;
            for (int i = links.Length / 2; i < links.Length; i++)
                if (links[i] != null && Vector2.Distance(w, links[i].position) < LinkGrab) return true;
            return false;
        }

        void TapScene(Vector2 w)
        {
            if (captainArea.OverlapPoint(w)) { TapCaptain(); return; }
            if (duckArea.OverlapPoint(w)) { TapDuck(); return; }
            if (boatArea.OverlapPoint(w))
            {
                hull.transform.localScale = Vector3.one;
                StartCoroutine(HalloweenKit.Pop(hull.transform, 1.04f, 0.2f));
                V69DontLetItSinkSound.Play(this, "knock", 0.5f);
                toast.Show(BoatLines[boatLines++ % BoatLines.Length], 1.6f);
                return;
            }
            if (skyArea.OverlapPoint(w)) { Sfx.Play(this, "tick", 0.4f); toast.Show(SkyLines[skyLines++ % SkyLines.Length], 1.6f); return; }
            if (seaArea.OverlapPoint(w))
            {
                V64ASound.Play(this, "bubbles", 0.4f);
                if (spray != null) StartCoroutine(SprayAt(new Vector3(w.x, Mathf.Min(w.y, seaY), 0f), 0.15f));
                toast.Show(SeaLines[seaLines++ % SeaLines.Length], 1.6f);
            }
        }

        // ---------- False piste ----------

        void TapCaptain()
        {
            captain.localScale = captainScale;   // un tocco a metà del rimbalzo non lo ingrandisce per sempre
            StartCoroutine(HalloweenKit.Pop(captain, 1.1f, 0.25f));
            V64ASound.Play(this, "squeak", 0.35f);
            say.Show(CaptainSay[captainLines++ % CaptainSay.Length], 1.8f);
        }

        void TapDuck()
        {
            int n = logic.TapDuck();
            if (n == 0) return;
            V64ASound.Play(this, "squeak", 0.7f);
            Haptics.Tap();
            // la paperella si gira verso l'angolo della catenella (in basso a sinistra) e la fissa
            duck.localScale = new Vector3(Mathf.Abs(duckScale.x), duckScale.y, duckScale.z);
            duck.localRotation = Quaternion.Euler(0f, 0f, 22f);
            StartCoroutine(HalloweenKit.Pop(duck, 1.12f, 0.2f));
            duckLookUntil = Time.time + 2.2f;
            revealUntil = Mathf.Max(revealUntil, Time.time + 0.6f);
            StartCoroutine(Glint(1.2f));
            toast.Show(DuckLines[duckLines++ % DuckLines.Length], 1.8f);
        }

        IEnumerator Glint(float seconds)
        {
            for (float k = 0f; k < seconds; k += Time.deltaTime)
            {
                HalloweenKit.SetAlpha(glint, 0.6f + 0.4f * Mathf.Sin(k * 12f));
                glint.transform.localScale = Vector3.one * (0.45f + 0.15f * Mathf.Sin(k * 9f));
                yield return null;
            }
            glint.transform.localScale = Vector3.one * 0.45f;
        }

        void RockTheSea()
        {
            if (logic.Rock() == 0) return;
            if (rockRun != null) return;   // già in corso: una sola ondata alla volta
            rockRun = StartCoroutine(Rocking());
        }

        IEnumerator Rocking()
        {
            Haptics.Light();
            V69DontLetItSinkSound.Play(this, "slosh", 0.6f);
            toast.Show(RockLines[rockLines % RockLines.Length], 1.8f);
            if (spray != null) StartCoroutine(SprayAt(boat.position + new Vector3(0.4f, -0.1f, 0f), 0.3f));
            captainArt.color = Seasick;
            sickUntil = Time.time + 3f;
            say.Show(SickSay[rockLines++ % SickSay.Length], 1.8f);
            for (float k = 0f; k < 1.8f; k += Time.deltaTime)
            {
                rockAmp = Mathf.Sin(Mathf.Clamp01(k / 1.8f) * Mathf.PI);
                yield return null;
            }
            rockAmp = 0f;
            rockRun = null;
        }

        IEnumerator SprayAt(Vector3 at, float seconds)
        {
            spray.transform.position = new Vector3(at.x, at.y, spray.transform.position.z);
            yield return HalloweenKit.Burst(spray, seconds);
        }

        void PlugWithFinger()
        {
            if (!logic.SetFinger(true)) return;
            V69DontLetItSinkSound.Play(this, "squish", 0.6f);
            Haptics.Tap();
            toast.Show("Plugged! (Hold it...)", 1.2f);
        }

        void UnplugFinger()
        {
            if (!logic.SetFinger(false)) return;
            V69DontLetItSinkSound.Play(this, "plop", 0.5f);
            toast.Show(FingerLines[fingerLines++ % FingerLines.Length], 1.8f);
        }

        void DropItem(bool tap, Vector2 at)
        {
            HwDragger.Item it = dragger.Release();
            if (it == null) return;
            bool onBoat = tap || boatArea.OverlapPoint(at) || boatArea.OverlapPoint(it.T.position) || holeArea.OverlapPoint(at);
            if (!onBoat) { dragger.SendHome(it, 0.3f); return; }
            if (it == bucketItem) StartCoroutine(Bail());
            else if (it == tapeItem) TapeTheHole();
        }

        IEnumerator Bail()
        {
            bucketItem.Enabled = false;
            Vector3 over = boat.position + new Vector3(0.75f, 0.55f, 0f);
            yield return HalloweenKit.Move(bucket, over, 0.25f, 0.2f);
            if (logic.Stage != SinkStage.Floating) { BucketHome(); yield break; }
            // la secchiata: dentro, fuori, giù dal bordo
            V69DontLetItSinkSound.Play(this, "slosh", 0.7f);
            yield return HalloweenKit.Move(bucket, over + new Vector3(0f, -0.25f, 0f), 0.15f);
            float taken = logic.Bail();
            yield return HalloweenKit.Move(bucket, over + new Vector3(0.45f, 0.25f, 0f), 0.2f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { bucket.localRotation = Quaternion.Euler(0f, 0f, -110f * k); yield return null; }
            V69DontLetItSinkSound.Play(this, "splash", 0.6f);
            if (spray != null) StartCoroutine(SprayAt(bucket.position + new Vector3(0.3f, -0.2f, 0f), 0.25f));
            toast.Show(taken > 0.01f ? BailLines[bailLines % BailLines.Length] : "It's already dry. The ocean isn't.", 1.6f);
            yield return new WaitForSeconds(0.35f);
            bucket.localRotation = Quaternion.identity;
            BucketHome();
            if (taken <= 0.01f) { bailLines++; yield break; }
            // ...e l'acqua torna dentro con un'onda (la logica la rimette piano piano)
            yield return new WaitForSeconds(0.35f);
            if (logic.Stage != SinkStage.Floating) yield break;
            V69DontLetItSinkSound.Play(this, "splash", 0.8f);
            if (spray != null) StartCoroutine(SprayAt(boat.position + new Vector3(-0.2f, 0.1f, 0f), 0.3f));
            StartCoroutine(HalloweenKit.Wiggle(captain, 8f, 0.4f, 30f));
            toast.Show(RushLines[bailLines++ % RushLines.Length], 1.6f);
        }

        void BucketHome()
        {
            bucket.localRotation = Quaternion.identity;
            dragger.SendHome(bucketItem, 0.35f);
            bucketItem.Enabled = true;
        }

        void TapeTheHole()
        {
            if (logic.ApplyPatch() == 0)
            {
                dragger.SendHome(tapeItem, 0.3f);
                if (logic.Stage == SinkStage.Floating) toast.Show("Already taped. For now.", 1.4f);
                return;
            }
            V69DontLetItSinkSound.Play(this, "rip", 0.7f);
            Haptics.Tap();
            patch.localPosition = Vector3.zero;
            patch.localRotation = Quaternion.identity;
            patch.localScale = patchScale;
            SetPatchColor(patchColor);
            HalloweenKit.Show(patch, true);
            StartCoroutine(HalloweenKit.Pop(patch, 1.25f, 0.25f));
            tape.position = hole.position + new Vector3(0.3f, 0.2f, 0f);
            dragger.SendHome(tapeItem, 0.4f);
            toast.Show(TapeLines[tapeLines++ % TapeLines.Length], 1.6f);
        }

        IEnumerator PeelPatch()
        {
            peeling = true;
            V69DontLetItSinkSound.Play(this, "peel", 0.6f);
            toast.Show(PeelLines[peelLines++ % PeelLines.Length], 1.8f);
            Vector3 from = patch.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                if (logic.Patched) { peeling = false; yield break; }   // rimessa subito: quella nuova resta
                patch.localPosition = from + new Vector3(0.15f * k, -0.35f * k * k, 0f);
                patch.localRotation = Quaternion.Euler(0f, 0f, -14f - 80f * k);
                SetPatchColor(new Color(0.45f, 0.5f, 0.58f, 1f - k));
                yield return null;
            }
            peeling = false;
            if (logic.Patched) yield break;
            HalloweenKit.Show(patch, false);
            patch.localPosition = Vector3.zero;
            patch.localRotation = Quaternion.identity;
            SetPatchColor(patchColor);
        }

        // ---------- Piena fino all'orlo: giù e di nuovo su ----------

        IEnumerator SinkAndResurface()
        {
            sinking = true;
            CancelGrips();
            V64ASound.Play(this, "glug", 0.8f);
            toast.Show("Glub... glub... glub...", 1.6f);
            if (drainBubbles != null) { drainBubbles.transform.position = boat.position; drainBubbles.Play(); }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.2f)
            {
                sinkDrop = 1.6f * k * k;
                hull.color = Color.Lerp(hullColor, hullColor * Underwater, k);
                captainArt.color = Color.Lerp(captainColor, captainColor * Underwater, k);
                yield return null;
            }
            if (drainBubbles != null) { drainBubbles.Stop(); drainBubbles.transform.position = plugHome + new Vector3(0f, 0.25f, 0f); }
            yield return new WaitForSeconds(0.4f);
            logic.Resurface();
            // torna su di colpo, vuota, e il capitano sputa l'acqua
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                sinkDrop = 1.6f * (1f - k) - Mathf.Sin(k * Mathf.PI) * 0.25f;
                yield return null;
            }
            sinkDrop = 0f;
            hull.color = hullColor;
            captainArt.color = captainColor;
            V69DontLetItSinkSound.Play(this, "splash", 0.9f);
            Haptics.Medium();
            if (spray != null) StartCoroutine(SprayAt(boat.position, 0.3f));
            if (spit != null) StartCoroutine(HalloweenKit.Burst(spit, 0.35f));
            V69DontLetItSinkSound.Play(this, "spit", 0.7f);
            say.Show("Pfffft!", 1.4f);
            toast.Show("Nope. Try again.", 2f);
            sinking = false;
        }

        // ---------- La catenella ----------

        void GrabChain()
        {
            V69DontLetItSinkSound.Play(this, "chain", 0.5f);
            Haptics.Tap();
            revealUntil = Time.time + 2f;
        }

        /// <summary>Quanto il dito si è allontanato da dove ha preso la catenella: in su o di lato, conta tutto.</summary>
        void DragChain(Grip g)
        {
            if (logic.Stage != SinkStage.Floating) return;
            Vector2 d = g.last - g.start;
            float dist = Mathf.Min(d.magnitude, PullDistance * 1.05f);
            Vector2 dir = d.sqrMagnitude > 0.0001f ? d.normalized : Vector2.up;
            Vector2 p = HalloweenKit.ClampSafe((Vector2)chainHome + dir * dist);
            chainEnd.position = new Vector3(p.x, p.y, chainHome.z);
            chainPull = dist / PullDistance;
            // il tappo comincia a muoversi (bolle) prima di uscire
            float lift = Mathf.Clamp01((chainPull - 0.4f) / 0.6f);
            plug.position = plugHome + new Vector3(Mathf.Sin(Time.time * 40f) * 0.02f * lift, 0.08f * lift, 0f);
            if (drainBubbles != null) { if (chainPull > 0.35f) drainBubbles.Play(); else drainBubbles.Stop(); }
            PullResult r = logic.PullChain(chainPull);
            if (r == PullResult.Unplugged) StartCoroutine(Drain());
        }

        /// <summary>Lasciata: se era tirata torna giù (niente si perde).</summary>
        void ReleaseChain(bool react)
        {
            if (logic.Stage != SinkStage.Floating) return;
            float was = chainPull;
            logic.ReleaseChain();
            chainPull = 0f;
            if (drainBubbles != null) drainBubbles.Stop();
            if (!isActiveAndEnabled) { chainEnd.position = chainHome; plug.position = plugHome; return; }
            StartCoroutine(SnapBack(react && was > 0.15f));
        }

        IEnumerator SnapBack(bool react)
        {
            chainSnapping = true;
            Vector3 from = chainEnd.position, pf = plug.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                chainEnd.position = Vector3.Lerp(from, chainHome, k * k);
                plug.position = Vector3.Lerp(pf, plugHome, k);
                yield return null;
            }
            chainEnd.position = chainHome;
            plug.position = plugHome;
            chainSnapping = false;
            if (!react) yield break;
            V69DontLetItSinkSound.Play(this, "chain", 0.5f);
            V69DontLetItSinkSound.Play(this, "thunk", 0.35f);
            toast.Show(SnapLines[snapLines++ % SnapLines.Length], 1.8f);
        }

        void FollowChain()
        {
            revealUntil = Time.time + 2.5f;
            StartCoroutine(Glint(0.8f));
            toast.Show(ChainLines[chainLines++ % ChainLines.Length], 1.8f);
        }

        // ---------- La soluzione: senz'acqua non si affonda ----------

        IEnumerator Drain()
        {
            finishing = true;
            grips.Clear();
            if (dragger.Held != null) dragger.Cancel();
            if (drainBubbles != null) drainBubbles.Stop();
            // il tappo esce
            Haptics.Medium();
            V69DontLetItSinkSound.Play(this, "plop", 0.9f);
            Vector3 p0 = plug.position;
            Vector3 c0 = chainEnd.position;
            HalloweenKit.Show(swirl, true);
            V69DontLetItSinkSound.Play(this, "gurgle", 0.8f);
            toast.Show("GLUGLUGLUGLUG...", 1.6f);
            float s0 = seaY;
            for (float k = 0f; k < 1f; k += Time.deltaTime / DrainSeconds)
            {
                float e = k * k * (3f - 2f * k);
                seaY = Mathf.Lerp(s0, Floor, e);
                plug.position = p0 + new Vector3(0.35f * k, 0.45f * Mathf.Sin(Mathf.Min(1f, k * 2f) * Mathf.PI * 0.5f) * (1f - k) + 0.05f, 0f);
                plug.localRotation = Quaternion.Euler(0f, 0f, 70f * k);
                chainEnd.position = Vector3.Lerp(c0, new Vector3(c0.x, Floor + 0.2f, c0.z), e);
                swirl.localRotation = Quaternion.Euler(0f, 0f, -Time.time * 500f);
                swirl.localScale = new Vector3(0.75f * (1f - 0.5f * k), 0.22f, 1f);
                HalloweenKit.SetAlpha(wavesArt, wavesColor.a * (1f - Mathf.Clamp01((k - 0.6f) / 0.4f)));
                Pose(Time.deltaTime);
                yield return null;
            }
            logic.FinishDrain();
            DrainedPose();
            V69DontLetItSinkSound.Play(this, "thunk", 1f);
            Haptics.Medium();
            StartCoroutine(HalloweenKit.Shake(boat, 0.05f, 0.2f));
            yield return Landed();
        }

        /// <summary>La vasca vuota: barchetta e paperella sul fondo, niente onde.</summary>
        void DrainedPose()
        {
            finishing = true;
            seaY = Floor;
            sinkDrop = 0f;
            rockAmp = 0f;
            chainPull = 0f;
            HalloweenKit.Show(swirl, false);
            HalloweenKit.Show(waves, false);
            HalloweenKit.Show(surface, false);
            if (drainBubbles != null) drainBubbles.Stop();
            plug.position = plugHome + new Vector3(0.35f, 0.06f, 0f);
            plug.localRotation = Quaternion.Euler(0f, 0f, 70f);
            chainEnd.position = new Vector3(chainHome.x, Floor + 0.2f, chainHome.z);
            boat.localRotation = Quaternion.Euler(0f, 0f, 3f);
            hull.color = hullColor;
            captainArt.color = captainColor;
            Pose(0f);
            duck.position = new Vector3(duckHome.x, DuckOnFloor, duckHome.z);
            duck.localRotation = Quaternion.Euler(0f, 0f, -6f);
        }

        IEnumerator Landed()
        {
            // il capitano si guarda intorno, confuso
            Vector3 s = captainScale;
            yield return new WaitForSeconds(0.2f);
            captain.localScale = new Vector3(-s.x, s.y, s.z);
            yield return new WaitForSeconds(0.3f);
            captain.localScale = s;
            say.Show("...?", 1.6f);
            toast.Show("CAN'T SINK WITHOUT WATER.", 2.4f);
            yield return Celebrate(1.3f);
            if (logic.TryComplete()) Win();
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (Input.GetKeyDown(KeyCode.K)) { chainPull = 1f; if (logic.PullChain(1f) == PullResult.Unplugged) StartCoroutine(Drain()); return; }
            if (Input.GetKeyDown(KeyCode.J)) { var g = new Grip { kind = Kind.Chain, start = chainHome, last = (Vector2)chainHome + Vector2.up * 0.8f, moved = true }; DragChain(g); ReleaseChain(true); }
            if (Input.GetKeyDown(KeyCode.Alpha1) && bucketItem.Enabled) StartCoroutine(Bail());
            if (Input.GetKeyDown(KeyCode.Alpha2)) TapeTheHole();
            if (Input.GetKeyDown(KeyCode.Alpha3)) TapDuck();
            if (Input.GetKeyDown(KeyCode.F)) logic.AddWater(0.5f);
            if (Input.GetKeyDown(KeyCode.H)) { editorFinger = true; PlugWithFinger(); }
            if (Input.GetKeyUp(KeyCode.H) && editorFinger) { editorFinger = false; UnplugFinger(); }
        }
#endif
    }

    /// <summary>Suoni sintetizzati del livello 265: gocce, gorgoglio dello scarico, tonfo sul fondo, spruzzi, nastro, catenella.</summary>
    public static class V69DontLetItSinkSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(69);

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
                case "drip":     // goccia: "plic" che sale
                    c = Make(kind, 0.12f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(900f, 1900f, t / 0.12f)) * 0.4f * Mathf.Exp(-t * 35f));
                    break;
                case "plop":     // tappo / dito che si stacca
                    c = Make(kind, 0.18f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(380f, 160f, t / 0.18f)) * 0.6f * Mathf.Exp(-t * 22f));
                    break;
                case "squish":   // dito nel buco bagnato
                    {
                        float lp = 0f;
                        c = Make(kind, 0.2f, t => { lp += (N() - lp) * 0.15f; return (lp * 1.2f + Mathf.Sin(t * 2f * Mathf.PI * 220f) * 0.2f) * Mathf.Sin(t / 0.2f * Mathf.PI); });
                    }
                    break;
                case "splash":   // spruzzo: rumore che si smorza
                    {
                        float lp = 0f;
                        c = Make(kind, 0.5f, t => { lp += (N() - lp) * 0.5f; return lp * 0.6f * Mathf.Exp(-t * 7f) * Mathf.Min(1f, t * 200f); });
                    }
                    break;
                case "slosh":    // acqua che sciaborda
                    {
                        float lp = 0f;
                        c = Make(kind, 0.7f, t => { lp += (N() - lp) * 0.07f; return lp * 2f * Mathf.Abs(Mathf.Sin(t * 2f * Mathf.PI * 2.2f)) * Mathf.Min(1f, (0.7f - t) * 6f); });
                    }
                    break;
                case "gurgle":   // lo scarico: gorgoglio lungo
                    c = Make(kind, 1.8f, t =>
                    {
                        float u = t / 1.8f;
                        float f = 140f + 180f * (t * 9f % 1f) + 60f * Mathf.Sin(t * 2f * Mathf.PI * 3f);
                        return (Mathf.Sin(t * 2f * Mathf.PI * f) * 0.35f + N() * 0.08f) * Mathf.Abs(Mathf.Sin(t * 2f * Mathf.PI * 4.5f)) * Mathf.Min(1f, t * 10f) * (1f - u * 0.6f);
                    });
                    break;
                case "thunk":    // la barchetta sul fondo della vasca: tonfo vuoto
                    c = Make(kind, 0.35f, t => (Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(160f, 70f, t / 0.35f)) * 0.9f + Mathf.Sin(t * 2f * Mathf.PI * 420f) * 0.2f) * Mathf.Exp(-t * 14f));
                    break;
                case "knock":    // nocche sullo scafo
                    c = Make(kind, 0.25f, t => (Mathf.Sin(t * 2f * Mathf.PI * 300f) * 0.5f + N() * 0.15f) * (Mathf.Exp(-t * 40f) + (t > 0.12f ? Mathf.Exp(-(t - 0.12f) * 40f) : 0f)));
                    break;
                case "rip":      // nastro adesivo strappato
                    c = Make(kind, 0.3f, t => N() * 0.45f * (0.5f + 0.5f * Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * 70f))) * Mathf.Sin(t / 0.3f * Mathf.PI));
                    break;
                case "peel":     // nastro bagnato che si stacca: "sccchh" molle e un plic
                    {
                        float lp = 0f;
                        c = Make(kind, 0.45f, t => { lp += (N() - lp) * 0.2f; return lp * 0.5f * Mathf.Sin(t / 0.45f * Mathf.PI) + (t > 0.35f ? Mathf.Sin(t * 2f * Mathf.PI * 700f) * 0.3f * Mathf.Exp(-(t - 0.35f) * 40f) : 0f); });
                    }
                    break;
                case "chain":    // catenella: tintinnio di maglie
                    c = Make(kind, 0.3f, t => (Mathf.Sin(t * 2f * Mathf.PI * 2900f) + Mathf.Sin(t * 2f * Mathf.PI * 4100f) * 0.6f) * 0.18f * Mathf.Exp(-(t * 16f % 1f) * 9f) * Mathf.Min(1f, (0.3f - t) * 10f));
                    break;
                case "spit":     // il capitano sputa l'acqua
                    c = Make(kind, 0.3f, t => (N() * 0.5f + Mathf.Sin(t * 2f * Mathf.PI * 260f) * 0.2f) * Mathf.Exp(-t * 9f) * Mathf.Min(1f, t * 80f));
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
            AudioClip clip = AudioClip.Create("v69_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
