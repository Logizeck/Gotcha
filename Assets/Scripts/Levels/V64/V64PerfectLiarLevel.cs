using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 257 "The perfect liar" — sala degli interrogatori: tre sospettati ("It wasn't me." / "I never lie." /
    /// "One of us is telling the truth.") e una macchina della verità montata a testa in giù (logo e scritta
    /// "THIS WAY UP" capovolti, il pesino del meccanismo in alto). TEST dà risultati contraddittori, scuotere fa solo
    /// picchi senza senso, urlare fa girare tutti ("UNSTABLE"). Soluzione: due dita sulle due estremità della macchina,
    /// telefono capovolto (conta l'orientamento, non le dita): il meccanismo scivola al suo posto e resta così. Si
    /// staccano le dita (spie verdi, la macchina si raddrizza), TEST ("AWAITING FINAL STATEMENT"), un suono qualsiasi:
    /// "PLAYER — LIAR". Senza microfono compare un megafono da tenere premuto.
    /// Logica in Gotcha.Levels.V64.PerfectLiarLogic (senza Unity): qui solo input e presentazione.
    /// </summary>
    public class V64PerfectLiarLevel : V62Level
    {
        [SerializeField] Transform[] suspects;          // 3 sospettati (si alzano alla fine)
        [SerializeField] Collider2D[] suspectAreas;
        [SerializeField] SpeechBubble[] suspectBubbles;
        [SerializeField] Transform[] pointers;          // le manine che indicano il giocatore (nascoste)
        [SerializeField] SpriteRenderer[] lamps;        // le tre spie davanti ai sospettati
        [SerializeField] SpriteRenderer[] lampGlows;
        [SerializeField] Transform detector;            // ruotato di 180° all'inizio
        [SerializeField] Transform needle;
        [SerializeField] Transform weight;              // il pesino del meccanismo (figlio della macchina)
        [SerializeField] Transform weightDown;          // dove va il pesino quando la macchina è dritta (coordinate locali della macchina)
        [SerializeField] Transform upSticker;           // la scritta "THIS WAY UP" (lampeggia quando si afferra)
        [SerializeField] Collider2D leftGrip, rightGrip;
        [SerializeField] SpriteRenderer leftGlow, rightGlow;
        [SerializeField] TextMesh readout;
        [SerializeField] Transform testButton;
        [SerializeField] Collider2D testArea;
        [SerializeField] Transform bigLamp;             // la quarta spia gigante "PLAYER — LIAR"
        [SerializeField] Transform micButton;           // il megafono (alternativa al microfono)
        [SerializeField] Collider2D micArea;
        [SerializeField] TextMesh micLabel;
        [SerializeField] Transform officer;
        [SerializeField] Transform cursorHand;
        [SerializeField] Transform cuffs;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        static readonly string[] Statements = { "It wasn't me.", "I never lie.", "One of us is telling the truth." };
        static readonly string[] Annoyed = { "I SAID it wasn't me!", "Never. Lied. Once. Stop poking.", "ONE of us. Not YOU." };
        static readonly string[] TestResults = { "LIAR: #1 #2 #3", "TRUTH: #1 #2 #3", "LIARS FOUND: 4 OF 3", "GUILTY: NOBODY + EVERYBODY" };
        static readonly string[] TestToasts =
        {
            "All three are lying. Also all three are honest.",
            "The machine is very sure. About everything.",
            "Four liars out of three suspects. Impressive.",
            "Results: inconclusive. Confidently.",
        };
        static readonly string[] ShakeToasts = { "Meaningless spikes. Very scientific.", "The graph now says: MOUNTAINS.", "Shaking it won't make it honest." };
        static readonly string[] ShoutLines = { "?!", "Who said that?", "Not me!" };

        const float ShakeLevel = 12f;
        const float MicHold = 0.6f;                     // secondi di pressione sul megafono
        const float MicFallbackAfter = 6f;              // in attesa da tanto senza suoni: compare il megafono

        readonly PerfectLiarLogic logic = new PerfectLiarLogic();
        readonly LiarMicGate gate = new LiarMicGate();
        readonly HashSet<int> alive = new HashSet<int>();
        Camera cam;
        HwToast toast;
        HwSay[] says;
        Vector3[] suspectHome;
        Vector3 officerHome, cursorScale;
        Vector2 lastTouch = new Vector2(0f, -3.9f);
        int leftId = int.MinValue, rightId = int.MinValue, testIdx, shakeIdx, micFinger = int.MinValue;
        float needleWild, shakeReadyAt, shoutReadyAt, awaitingSince, micHeld, clock, lampsResetAt = -1f;
        bool busy;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            gate.Reset();
            ResetV62();
            toast = new HwToast(infoText, infoHolder);
            says = new HwSay[suspectBubbles.Length];
            for (int i = 0; i < says.Length; i++) says[i] = new HwSay(suspectBubbles[i]);
            suspectHome = new Vector3[suspects.Length];
            for (int i = 0; i < suspects.Length; i++) suspectHome[i] = suspects[i].localPosition;
            officerHome = officer.position;
            cursorScale = cursorHand.localScale;
            detector.localRotation = Quaternion.Euler(0f, 0f, 180f);
            foreach (Transform p in pointers) HalloweenKit.Show(p, false);
            HalloweenKit.Show(bigLamp, false);
            HalloweenKit.Show(micButton, false);
            HalloweenKit.Show(cursorHand, false);
            HalloweenKit.Show(cuffs, false);
            HalloweenKit.SetAlpha(leftGlow, 0f);
            HalloweenKit.SetAlpha(rightGlow, 0f);
            SetLamps(new Color(0.55f, 0.55f, 0.6f), 0f);
            Readout("READY TO DETECT", new Color(0.5f, 1f, 0.7f));
            StartCoroutine(Intro());
        }

        /// <summary>Le tre dichiarazioni, una dopo l'altra.</summary>
        IEnumerator Intro()
        {
            for (int i = 0; i < says.Length; i++)
            {
                yield return new WaitForSeconds(0.5f);
                says[i].Show(Statements[i], 3.2f - i * 0.5f);
                Sfx.Play(this, "pop", 0.3f);
            }
        }

        void OnDisable()
        {
            // il microfono non resta mai acceso fuori dal livello
            Inputs.ReleaseMic(this);
        }

        void Update()
        {
            for (int i = 0; i < says.Length; i++) says[i].Tick();
            toast.Tick();
            if (IsFinished) return;
            float dt = Time.deltaTime;
            clock += dt;
            needleWild = Mathf.MoveTowards(needleWild, 0f, dt);
            needle.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(clock * (5f + needleWild * 40f)) * (8f + needleWild * 55f));
            if (lampsResetAt > 0f && Time.time >= lampsResetAt) { lampsResetAt = -1f; if (!logic.MachineCorrected) SetLamps(new Color(0.55f, 0.55f, 0.6f), 0f); }
            if (busy || logic.InputLocked) return;

            ReadMic(dt);
            if (logic.InputLocked) return;
            ReadFingers(out bool leftHeld, out bool rightHeld);
            if (logic.InputLocked) return;
            Glow(leftGlow, leftHeld && !logic.MachineCorrected);
            Glow(rightGlow, rightHeld && !logic.MachineCorrected);
            switch (logic.Tick(dt, Inputs.Tilt().y, leftHeld, rightHeld))
            {
                case LiarEvent.GripStarted: OnGrip(); break;
                case LiarEvent.GripLost: Readout("READY TO DETECT", new Color(0.5f, 1f, 0.7f)); break;
                case LiarEvent.Corrected: StartCoroutine(Correct()); break;
                case LiarEvent.Calibrated: StartCoroutine(Calibrate()); break;
            }
            if (Inputs.Shake() > ShakeLevel && Time.time >= shakeReadyAt && logic.Shake())
            {
                shakeReadyAt = Time.time + 1.5f;
                needleWild = 1f;
                Haptics.Light();
                Sfx.Play(this, "buzz", 0.4f);
                Readout("/\\/\\/\\/\\/\\/\\", new Color(1f, 0.85f, 0.4f));
                toast.Show(ShakeToasts[shakeIdx++ % ShakeToasts.Length], 1.8f);
                StartCoroutine(HalloweenKit.Shake(detector, 0.06f, 0.5f));
            }
        }

        // ---------- Dita ----------

        /// <summary>
        /// Al massimo due dita: un dito che comincia su un'estremità della macchina la "afferra" finché non si stacca
        /// (anche se scivola un po' mentre si gira il telefono). Gli altri tocchi brevi sono tap (sospettati, TEST,
        /// megafono). Editor: col mouse su un'estremità, Shift sinistro tiene l'altra.
        /// </summary>
        void ReadFingers(out bool leftHeld, out bool rightHeld)
        {
            alive.Clear();
            bool micDown = false;
            List<S41KKit.Finger> fingers = S41KKit.Fingers(cam);
            foreach (S41KKit.Finger f in fingers)
            {
                if (f.ended)
                {
                    if (f.id == leftId) leftId = int.MinValue;
                    if (f.id == rightId) rightId = int.MinValue;
                    if (f.id == micFinger) micFinger = int.MinValue;
                    continue;
                }
                alive.Add(f.id);
                lastTouch = HalloweenKit.ClampSafe(f.world);
                if (f.id == micFinger) micDown = micArea != null && micArea.OverlapPoint(f.world);
                if (!f.began || alive.Count > 2 || PointerInput.IsOverUi()) continue;
                bool canGrip = !logic.MachineCorrected;
                if (canGrip && leftId == int.MinValue && leftGrip.OverlapPoint(f.world)) { leftId = f.id; Haptics.Tap(); Sfx.Play(this, "tick", 0.5f); continue; }
                if (canGrip && rightId == int.MinValue && rightGrip.OverlapPoint(f.world)) { rightId = f.id; Haptics.Tap(); Sfx.Play(this, "tick", 0.5f); continue; }
                if (micButton.gameObject.activeSelf && micArea.OverlapPoint(f.world)) { micFinger = f.id; micDown = true; continue; }
                Tap(f.world);
            }
            if (leftId != int.MinValue && !alive.Contains(leftId)) leftId = int.MinValue;
            if (rightId != int.MinValue && !alive.Contains(rightId)) rightId = int.MinValue;
            if (micFinger != int.MinValue && !alive.Contains(micFinger)) micFinger = int.MinValue;
            leftHeld = leftId != int.MinValue;
            rightHeld = rightId != int.MinValue;
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.LeftShift) && (leftHeld || rightHeld)) { leftHeld = true; rightHeld = true; }   // Editor: seconda mano
#endif
            MicButton(micDown);
        }

        void Tap(Vector2 w)
        {
            if (testArea.OverlapPoint(w)) { PressTest(); return; }
            for (int i = 0; i < suspectAreas.Length; i++)
            {
                if (!suspectAreas[i].OverlapPoint(w)) continue;
                int n = logic.TapSuspect(i);
                Sfx.Play(this, "pop", 0.4f);
                if (logic.IsAnnoyed(i))
                {
                    says[i].Show(n > PerfectLiarLogic.AnnoyedAfter + 1 ? "*glares silently*" : Annoyed[i], 1.8f);
                    StartCoroutine(HalloweenKit.Shake(suspects[i], 0.05f, 0.35f));
                }
                else
                {
                    says[i].Show(Statements[i], 1.8f);
                    StartCoroutine(HalloweenKit.Pop(suspects[i], 1.06f, 0.2f));
                }
                return;
            }
            if (detector != null && Vector2.Distance(w, detector.position) < 0.9f)
            {
                Sfx.Play(this, "clunk", 0.4f);
                toast.Show(logic.MachineCorrected ? "Much better." : "Something about this machine looks... off.", 1.8f);
            }
        }

        // ---------- TEST ----------

        void PressTest()
        {
            StartCoroutine(HalloweenKit.Pop(testButton, 0.88f, 0.15f));
            Sfx.Play(this, "clunk", 0.6f);
            Haptics.Light();
            switch (logic.PressTest())
            {
                case LiarTest.Contradictory:
                    SetLamps(new Color(1f, 0.3f, 0.25f), 0.8f);
                    lampsResetAt = Time.time + 1.6f;
                    needleWild = 0.8f;
                    Readout(TestResults[testIdx % TestResults.Length], new Color(1f, 0.45f, 0.4f));
                    toast.Show(TestToasts[testIdx % TestToasts.Length], 2f);
                    testIdx++;
                    Sfx.Play(this, "buzz", 0.5f);
                    break;
                case LiarTest.LetGoFirst:
                    Readout("RELEASE THE HANDLES", new Color(1f, 0.85f, 0.4f));
                    break;
                case LiarTest.AwaitingStatement:
                    awaitingSince = Time.time;
                    Readout("AWAITING FINAL STATEMENT...", new Color(0.5f, 1f, 0.7f));
                    Sfx.Play(this, "ding", 0.4f);
                    break;
                case LiarTest.AlreadyWaiting:
                    Readout("AWAITING FINAL STATEMENT...", new Color(0.5f, 1f, 0.7f));
                    StartCoroutine(HalloweenKit.Pop(readout.transform, 1.15f, 0.2f));
                    break;
            }
        }

        // ---------- Microfono ----------

        void ReadMic(float dt)
        {
            float level = Inputs.Mic(this);
            gate.Feed(dt, level, out bool sound, out bool loud);
            if (sound || loud) OnSound(loud);
        }

        void OnSound(bool loud)
        {
            switch (logic.Sound(loud))
            {
                case LiarSound.PlayerIsLiar:
                    StartCoroutine(Accuse());
                    break;
                case LiarSound.Unstable:
                    if (Time.time < shoutReadyAt) return;
                    shoutReadyAt = Time.time + 2f;
                    // tutti si girano verso il giocatore
                    for (int i = 0; i < suspects.Length; i++)
                    {
                        says[i].Show(ShoutLines[i % ShoutLines.Length], 1.4f);
                        StartCoroutine(HalloweenKit.Pop(suspects[i], 1.12f, 0.3f));
                    }
                    needleWild = 1f;
                    Readout("UNSTABLE", new Color(1f, 0.3f, 0.3f));
                    StartCoroutine(Blink(readout.transform, 4));
                    Haptics.Light();
                    break;
            }
        }

        /// <summary>
        /// Il megafono (alternativa accessibile al microfono): compare solo all'ultimo passo, se il microfono non è
        /// disponibile o se dopo un po' non si sente niente. Tenerlo premuto per un attimo = "dire qualcosa".
        /// </summary>
        void MicButton(bool down)
        {
            bool need = logic.State == LiarState.Awaiting &&
                        (SensorHub.Instance == null || !SensorHub.Instance.MicActive || Time.time - awaitingSince > MicFallbackAfter);
            if (micButton.gameObject.activeSelf != need)
            {
                HalloweenKit.Show(micButton, need);
                if (need) StartCoroutine(HalloweenKit.Pop(micButton, 1.2f, 0.3f));
            }
            if (!need || !down) { micHeld = 0f; if (micLabel != null) micLabel.text = "HOLD TO SPEAK"; return; }
            micHeld += Time.deltaTime;
            if (micLabel != null) micLabel.text = micHeld < MicHold * 0.5f ? "AHEM..." : "AHEM... I...";
            if (micHeld >= MicHold) { micHeld = 0f; OnSound(false); }
        }

        // ---------- Correzione della macchina ----------

        void OnGrip()
        {
            Readout("GRIP DETECTED", new Color(1f, 0.85f, 0.4f));
            Sfx.Play(this, "clunk", 0.4f);
            StartCoroutine(HalloweenKit.Wiggle(detector, 3f, 0.4f, 30f));
            StartCoroutine(Blink(upSticker, 3));
        }

        IEnumerator Correct()
        {
            // il pesino scivola verso il basso "vero" (ora la macchina è dritta rispetto alla gravità)
            Sfx.Play(this, "whoosh", 0.5f);
            Readout("CLUNK! MECHANISM ALIGNED", new Color(0.5f, 1f, 0.7f));
            yield return SlideLocal(weight, weightDown.localPosition, 0.3f);
            Sfx.Play(this, "clunk", 0.9f);
            Haptics.Medium();
            StartCoroutine(HalloweenKit.Shake(detector, 0.04f, 0.3f));
            toast.Show("Clunk! Something fell into place.", 1.8f);
        }

        IEnumerator Calibrate()
        {
            busy = true;
            Glow(leftGlow, false);
            Glow(rightGlow, false);
            // la macchina si raddrizza sullo schermo (il pesino resta in basso)
            Quaternion from = detector.localRotation, to = Quaternion.identity;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                detector.localRotation = Quaternion.Slerp(from, to, k * k * (3f - 2f * k));
                yield return null;
            }
            detector.localRotation = to;
            Sfx.Play(this, "clunk", 0.6f);
            for (int i = 0; i < lamps.Length; i++)
            {
                SetLamp(i, new Color(0.4f, 1f, 0.45f), 0.75f);
                Sfx.Play(this, "tick", 0.7f);
                yield return new WaitForSeconds(0.2f);
            }
            Readout("ALL INNOCENT?! PRESS TEST", new Color(0.5f, 1f, 0.7f));
            toast.Show("Three green lights. Nobody is lying?", 2f);
            busy = false;
        }

        // ---------- Finale ----------

        IEnumerator Accuse()
        {
            busy = true;
            HalloweenKit.Show(micButton, false);
            Readout("SOURCE: OUTSIDE THE ROOM", new Color(1f, 0.35f, 0.3f));
            needleWild = 1f;
            yield return new WaitForSeconds(0.35f);
            // la quarta spia, enorme
            Vector3 big = bigLamp.localScale;
            bigLamp.localScale = big * 0.1f;
            HalloweenKit.Show(bigLamp, true);
            Sfx.Play(this, "buzz", 1f);
            Haptics.Heavy();
            yield return HalloweenKit.ScaleTo(bigLamp, big, 0.25f);
            StartCoroutine(HalloweenKit.Wiggle(bigLamp, 4f, 0.6f, 40f));
            yield return new WaitForSeconds(0.4f);
            // tutti in piedi, il dito puntato
            Inputs.ReleaseMic(this);
            for (int i = 0; i < suspects.Length; i++)
            {
                StartCoroutine(SlideLocal(suspects[i], suspectHome[i] + new Vector3(0f, 0.3f, 0f), 0.2f));
                HalloweenKit.Show(pointers[i], true);
                StartCoroutine(HalloweenKit.Pop(pointers[i], 1.3f, 0.25f));
                says[i].Hide();
            }
            Sfx.Play(this, "pop", 0.8f);
            toast.Show("IT WAS YOU!", 1.6f);
            yield return new WaitForSeconds(0.8f);
            // arriva il poliziotto e arresta... il cursore
            cursorHand.position = new Vector3(lastTouch.x, Mathf.Min(lastTouch.y, -2.6f), cursorHand.position.z);
            cursorHand.localScale = cursorScale;
            HalloweenKit.Show(cursorHand, true);
            Sfx.Play(this, "pop", 0.6f);
            yield return HalloweenKit.Pop(cursorHand, 1.4f, 0.2f);
            Vector3 stop = cursorHand.position + new Vector3(1.05f, 0.55f, 0f);
            yield return HalloweenKit.Move(officer, new Vector3(stop.x, stop.y, officer.position.z), 0.4f, 0.1f);
            cuffs.position = cursorHand.position + new Vector3(0f, -0.1f, -0.01f);
            HalloweenKit.Show(cuffs, true);
            Sfx.Play(this, "clunk", 0.9f);
            Haptics.Medium();
            yield return HalloweenKit.Pop(cuffs, 1.3f, 0.15f);
            cuffs.SetParent(cursorHand, true);
            cursorHand.SetParent(officer, true);
            StartCoroutine(Celebrate(1.6f));
            toast.Show("CASE CLOSED. SOMEHOW.", 2f);
            yield return HalloweenKit.Move(officer, officerHome + new Vector3(1.5f, 0f, 0f), 0.6f);
            yield return new WaitForSeconds(0.9f);
            if (logic.TryComplete()) Win();
        }

        // ---------- Aiuti di presentazione ----------

        void Readout(string text, Color color)
        {
            if (readout == null) return;
            readout.text = text;
            readout.color = color;
        }

        void SetLamps(Color c, float glow) { for (int i = 0; i < lamps.Length; i++) SetLamp(i, c, glow); }

        void SetLamp(int i, Color c, float glow)
        {
            if (i < lamps.Length && lamps[i] != null) lamps[i].color = c;
            if (lampGlows != null && i < lampGlows.Length && lampGlows[i] != null)
            {
                Color g = c;
                g.a = glow;
                lampGlows[i].color = g;
            }
        }

        void Glow(SpriteRenderer sr, bool on)
        {
            if (sr == null) return;
            float a = on ? 0.55f : 0f;
            if (Mathf.Abs(sr.color.a - a) > 0.01f) HalloweenKit.SetAlpha(sr, Mathf.MoveTowards(sr.color.a, a, Time.deltaTime * 4f));
        }

        static IEnumerator SlideLocal(Transform t, Vector3 to, float seconds)
        {
            if (t == null) yield break;
            Vector3 from = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / Mathf.Max(0.01f, seconds))
            {
                t.localPosition = Vector3.Lerp(from, to, k * k);
                yield return null;
            }
            t.localPosition = to;
        }

        static IEnumerator Blink(Transform t, int times)
        {
            if (t == null) yield break;
            for (int i = 0; i < times; i++)
            {
                t.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.1f);
                t.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.12f);
            }
        }
    }
}
