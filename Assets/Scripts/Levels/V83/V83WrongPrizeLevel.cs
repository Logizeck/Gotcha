using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 47 della roadmap "The Wrong Prize" (id wrong_prize) — "WIN FIRST PRIZE." Allo stadio un podio a tre gradini:
    /// a sinistra il 2 con un rivale tronfio, al centro il più alto con l'1 e la coppa d'oro (avvitata), a destra il 3 con
    /// il corridore del giocatore (pettorina "YOU"). Sotto, sulla pista, l'arbitro col fischietto.
    /// Inganno: il corridore trascinato sul primo gradino torna da solo al suo (l'arbitro fischia: "Back to your place!");
    /// lasciato altrove torna al suo gradino. La coppa non si stacca (oscilla: "It's screwed on."). Tutto reversibile.
    /// Soluzione: i numeri sul davanti dei gradini sono oggetti a sé, trascinabili. L'1 trascinato sul gradino del
    /// corridore si scambia con il 3 (il 3 va sul vecchio primo gradino): il gradino basso è il nuovo primo posto. Conta il
    /// NUMERO lasciato sul gradino (WrongPrizeLogic), non la posizione del corridore. Il 2 portato sul gradino del
    /// corridore si scambia con il 3 ("Second place. Still not first."; il rivale resta di sasso) e il 3 riportato lì
    /// rimette le cose come prima: ogni scambio è reversibile e l'1 si trascina sempre. Un numero lasciato nel vuoto, sul
    /// suo gradino o su un altro gradino torna al suo posto.
    /// Vittoria: l'1 si incastra con una scintilla, l'arbitro fischia ("Rules are rules!"), i bulloni saltano, la
    /// coppa vola fra le mani del corridore che esulta a braccia alzate, la medaglia dell'1 gli cade al collo, il rivale
    /// resta a bocca aperta; LevelReward "FIRST PLACE! YOU CHANGED THE RULES!" -> Win una volta sola.
    /// Dita: V64AFingers (al massimo due id; un solo dito trascina). Pausa, app in background o livello spento: ciò che era
    /// in mano torna al suo posto, niente conta; spento a metà della festa, la festa ricomincia (vittoria una sola).
    /// Tasti di prova nell'Editor: 1/2/3 trascinano il numero con quel valore sul gradino del corridore, Q porta l'1 sul
    /// gradino del rivale, N lascia l'1 nel vuoto, C porta il corridore sul primo gradino, V lo lascia nel vuoto, T tocca il
    /// corridore, B tocca la coppa, K il rivale, L l'arbitro, H soluzione completa, P pausa simulata (dita perse),
    /// R ricomincia.
    /// </summary>
    public class V83WrongPrizeLevel : LevelController
    {
        [Header("Gradini per posizione: 0 sinistra, 1 centro, 2 destra (area di rilascio generosa sul gradino e sopra)")]
        [SerializeField] Transform[] steps;
        [SerializeField] Collider2D[] stepAreas;
        [Header("I numeri per VALORE: 0 = \"1\", 1 = \"2\", 2 = \"3\" (collider sui genitori)")]
        [SerializeField] Transform[] digits;
        [SerializeField] Collider2D[] digitAreas;
        [SerializeField] TextMesh[] digitTexts;
        [SerializeField] SpriteRenderer stepGlow;
        [SerializeField] Transform sparkle;
        [Header("Il corridore (radice fra i piedi)")]
        [SerializeField] Transform runner;
        [SerializeField] Collider2D runnerArea;
        [SerializeField] Transform runnerBody;
        [SerializeField] Transform runnerCheer;
        [SerializeField] Transform runnerHead;
        [SerializeField] Transform[] runnerFaces;      // 0 allegra, 1 delusa, 2 felicissima
        [SerializeField] Transform medal;
        [Header("La coppa avvitata")]
        [SerializeField] Transform trophy;
        [SerializeField] Collider2D trophyArea;
        [SerializeField] Transform[] bolts;
        [Header("Il rivale e l'arbitro")]
        [SerializeField] Transform rival;
        [SerializeField] Collider2D rivalArea;
        [SerializeField] Transform rivalSmug;
        [SerializeField] Transform rivalShock;
        [SerializeField] Transform referee;
        [SerializeField] Collider2D refereeArea;
        [SerializeField] Transform refereeIdle;
        [SerializeField] Transform refereeBlow;
        [SerializeField] TextMesh tweetText;
        [Header("Effetti, fumetti, scritte e festa")]
        [SerializeField] SpriteRenderer puff;
        [SerializeField] SpeechBubble runnerBubble;
        [SerializeField] SpeechBubble rivalBubble;
        [SerializeField] SpeechBubble refereeBubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "FIRST PLACE! YOU CHANGED THE RULES!";
        const int Steps = WrongPrizeLogic.StepCount, RunnerStep = WrongPrizeLogic.RunnerStep;
        const int DragOrder = 45;
        const float Follow = 18f, LiftScale = 1.18f, HandsUpY = 1.62f, RunnerMidY = 0.8f;
        const float MinX = -2.45f, MaxX = 2.45f, MinY = -4.3f, MaxY = 2.85f;

        static readonly string[] SentBackLines = { "Back to your place!", "No stepping up!", "Stay on your step!" };
        static readonly string[] ReturnLines = { "You hop back onto your step.", "Your step is waiting for you.", "Back on your step." };
        static readonly string[] RunnerTapLines = { "I want that trophy!", "So close...", "Ready! Ready!" };
        static readonly string[] TrophyLines = { "It's screwed on.", "Bolted down. Tight.", "It won't budge." };
        static readonly string[] DigitTapLines = { "A big wooden number.", "It wiggles a little." };
        static readonly string[] DigitBackLines = { "The number slides back.", "Back on its step." };
        static readonly string[] WrongStepLines = { "That's not your step.", "Wrong step. It slides back." };
        static readonly string[] RivalNoLines = { "Thanks, but no.", "Hands off my step." };
        static readonly string[] RivalPokeLines = { "Nice try, number {0}.", "Don't touch the champion.", "Eyes on your own step." };
        static readonly string[] RefereePokeLines = { "Hands off, please!", "I'm watching you." };
        static readonly string[] SecondLines = { "Second place. Still not first.", "Second! ...Still not first." };
        static readonly string[] ThirdLines = { "Back where you belong.", "Third again. Ha!" };
        const string RivalSecondLine = "Hey! I was second!";
        const string RulesLine = "Rules are rules!";
        const string NewFirstToast = "The lowest step is now first place!";

        enum Face { Happy = 0, Sad = 1, Joy = 2 }
        enum Speaker { Runner, Rival, Referee }

        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        WrongPrizeLogic logic;
        Camera cam;
        HwToast toast;
        HwSay sayRunner, sayRival, sayReferee;
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        readonly HashSet<int> seen = new HashSet<int>();
        readonly Vector3[] digitHome = new Vector3[Steps];
        Vector3[] digitScale, boltPos;
        int[] digitOrder;
        Vector3 runnerPos, runnerScale, trophyPos, trophyScale, medalPos, medalScale, puffScale;
        Transform trophyParent;
        Vector2 grabOffset, lastFinger;
        float whistleT, sadT, rivalShockT, hopT;
        int sentLines, returnLines, tapLines, trophyLines, digitTapLines, digitBackLines, wrongStepLines, rivalNoLines, rivalPokeLines, refPokeLines, secondLines, thirdLines;
        bool captured, finishing;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            if (!captured)
            {
                captured = true;
                logic = new WrongPrizeLogic();
                // la casa di ogni gradino è dove sta il suo numero all'inizio (2 - 1 - 3)
                for (int s = 0; s < Steps; s++)
                {
                    Transform d = Digit(WrongPrizeLogic.StartNumber(s));
                    digitHome[s] = d != null ? d.localPosition : Vector3.zero;
                }
                int n = digits != null ? digits.Length : 0;
                digitScale = new Vector3[n];
                digitOrder = new int[n];
                for (int i = 0; i < n; i++)
                {
                    if (digits[i] != null) digitScale[i] = digits[i].localScale;
                    TextMesh t = Pick(digitTexts, i);
                    if (t != null) digitOrder[i] = t.GetComponent<MeshRenderer>().sortingOrder;
                }
                runnerPos = runner.localPosition;
                runnerScale = runner.localScale;
                trophyParent = trophy.parent;
                trophyPos = trophy.localPosition;
                trophyScale = trophy.localScale;
                boltPos = new Vector3[bolts != null ? bolts.Length : 0];
                for (int i = 0; i < boltPos.Length; i++) if (bolts[i] != null) boltPos[i] = bolts[i].localPosition;
                if (medal != null) { medalPos = medal.localPosition; medalScale = medal.localScale; }
                if (puff != null) puffScale = puff.transform.localScale;
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            V64AFingers.Reset();
            seen.Clear();
            finishing = false;
            whistleT = sadT = rivalShockT = hopT = 0f;
            sentLines = returnLines = tapLines = trophyLines = digitTapLines = digitBackLines = wrongStepLines = rivalNoLines = rivalPokeLines = refPokeLines = secondLines = thirdLines = 0;
            toast = new HwToast(infoText, infoHolder);
            sayRunner = new HwSay(runnerBubble);
            sayRival = new HwSay(rivalBubble);
            sayReferee = new HwSay(refereeBubble);
            RestoreScene();
            PoseAll();
        }

        Transform Digit(int number) => Pick(digits, number - 1);
        static T Pick<T>(T[] a, int i) where T : class => a != null && i >= 0 && i < a.Length ? a[i] : null;

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        /// <summary>Corridore, coppa, rivale e arbitro come all'inizio (prima della festa).</summary>
        void RestoreScene()
        {
            HideBubbles();
            runner.localPosition = runnerPos;
            runner.localScale = runnerScale;
            runner.localRotation = Quaternion.identity;
            HalloweenKit.Show(runnerBody, true);
            HalloweenKit.Show(runnerCheer, false);
            if (runnerHead != null) runnerHead.localRotation = Quaternion.identity;
            SetFace(Face.Happy);
            if (medal != null) { medal.localPosition = medalPos; medal.localScale = medalScale; HalloweenKit.Show(medal, false); }
            trophy.SetParent(trophyParent, false);
            trophy.localPosition = trophyPos;
            trophy.localScale = trophyScale;
            trophy.localRotation = Quaternion.identity;
            for (int i = 0; i < boltPos.Length; i++)
            {
                if (bolts[i] == null) continue;
                bolts[i].localPosition = boltPos[i];
                bolts[i].localRotation = Quaternion.identity;
                HalloweenKit.Show(bolts[i], true);
            }
            HalloweenKit.Show(rivalSmug, true);
            HalloweenKit.Show(rivalShock, false);
            HalloweenKit.Show(refereeIdle, true);
            HalloweenKit.Show(refereeBlow, false);
            if (tweetText != null) HalloweenKit.Show(tweetText.transform, false);
            HalloweenKit.Show(sparkle, false);
            if (puff != null) { HalloweenKit.Show(puff, false); puff.transform.localScale = puffScale; }
            if (stepGlow != null) HalloweenKit.SetAlpha(stepGlow, 0f);
        }

        /// <summary>I numeri subito al loro gradino (secondo la logica), il corridore al suo posto.</summary>
        void PoseAll()
        {
            for (int n = 1; n <= Steps; n++)
            {
                Transform d = Digit(n);
                if (d == null) continue;
                int s = logic.StepOf(n);
                if (s >= 0) d.localPosition = digitHome[s];
                d.localRotation = Quaternion.identity;
                d.localScale = digitScale[n - 1];
                TextMesh t = Pick(digitTexts, n - 1);
                if (t != null) t.GetComponent<MeshRenderer>().sortingOrder = digitOrder[n - 1];
            }
            if (!logic.Won)
            {
                runner.localPosition = runnerPos;
                runner.localRotation = Quaternion.identity;
            }
            if (stepGlow != null) HalloweenKit.SetAlpha(stepGlow, 0f);
        }

        void SetFace(Face f)
        {
            if (runnerFaces == null) return;
            for (int i = 0; i < runnerFaces.Length; i++) HalloweenKit.Show(runnerFaces[i], i == (int)f);
        }

        void HideBubbles()
        {
            sayRunner?.Hide();
            sayRival?.Hide();
            sayReferee?.Hide();
        }

        /// <summary>Parla uno alla volta: gli altri fumetti si chiudono (non si sovrappongono mai).</summary>
        void Speak(Speaker who, string line, float seconds)
        {
            HideBubbles();
            switch (who)
            {
                case Speaker.Runner: sayRunner?.Show(line, seconds); break;
                case Speaker.Rival: sayRival?.Show(line, seconds); break;
                default: sayReferee?.Show(line, seconds); break;
            }
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: ciò che era in mano torna a posto, niente conta
            CancelAll();
            StopFx();
            if (!captured || logic == null || IsFinished || finishing) return;
            PoseAll();
            HideBubbles();
            whistleT = sadT = rivalShockT = hopT = 0f;
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == WrongPrizeStage.Celebrating)
            {
                finishing = false;
                RestoreScene();
                PoseAll();
                StartCoroutine(Finale());
            }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelAll(); }

        /// <summary>Le dita spariscono: ciò che era in mano torna al suo posto (Update lo fa scivolare), niente conta.</summary>
        void CancelAll()
        {
            V64AFingers.Reset();
            if (logic != null) logic.Cancel();
        }

        void Update()
        {
            toast?.Tick();
            sayRunner?.Tick();
            sayRival?.Tick();
            sayReferee?.Tick();
            if (IsFinished || toast == null || logic == null || finishing) return;
            if (Time.timeScale <= 0f) { CancelAll(); return; }   // pausa o pubblicità: niente input
#if UNITY_EDITOR
            EditorKeys();
#endif
            if (!finishing) ReadFingers();
            if (!finishing) Pose();
        }

        /// <summary>Ciò che è in mano segue il dito, il resto torna a casa; il gradino sotto si illumina; facce e fischietto.</summary>
        void Pose()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            float kFollow = 1f - Mathf.Exp(-dt * Follow * 2f), kHome = 1f - Mathf.Exp(-dt * Follow), kScale = 1f - Mathf.Exp(-dt * 14f);
            int hover = -1;
            Vector2 want = lastFinger + grabOffset;
            want.x = Mathf.Clamp(want.x, MinX, MaxX);
            want.y = Mathf.Clamp(want.y, MinY, MaxY);

            // il corridore
            if (logic.Held == WrongPrizeGrab.Runner)
            {
                runner.position = Vector3.Lerp(runner.position, new Vector3(want.x, want.y, runner.position.z), kFollow);
                runner.localRotation = Quaternion.Euler(0f, 0f, 6f * Mathf.Sin(Time.time * 9f));
                hover = StepUnder((Vector2)runner.position + new Vector2(0f, RunnerMidY), lastFinger);
            }
            else
            {
                if (hopT > 0f) hopT = Mathf.Max(0f, hopT - dt);
                Vector3 home = runnerPos + new Vector3(0f, 0.28f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(1f - hopT / 0.35f)) * (hopT > 0f ? 1f : 0f), 0f);
                runner.localPosition = Vector3.Lerp(runner.localPosition, home, kHome);
                if (!Busy(runner, 1)) runner.localRotation = Quaternion.Slerp(runner.localRotation, Quaternion.identity, kScale);
            }

            // i numeri
            for (int n = 1; n <= Steps; n++)
            {
                Transform d = Digit(n);
                if (d == null) continue;
                TextMesh t = Pick(digitTexts, n - 1);
                if (logic.Held == WrongPrizeGrab.Digit && logic.DragNumber == n)
                {
                    d.position = Vector3.Lerp(d.position, new Vector3(want.x, want.y, d.position.z), kFollow);
                    d.localScale = Vector3.Lerp(d.localScale, digitScale[n - 1] * LiftScale, kScale);
                    d.localRotation = Quaternion.Euler(0f, 0f, 8f * Mathf.Sin(Time.time * 6f));
                    hover = StepUnder(d.position, lastFinger);
                    continue;
                }
                int s = logic.StepOf(n);
                if (s < 0) continue;
                d.localPosition = Vector3.Lerp(d.localPosition, digitHome[s], kHome);
                if (!Busy(d, 0)) d.localScale = Vector3.Lerp(d.localScale, digitScale[n - 1], kScale);
                if (!Busy(d, 1)) d.localRotation = Quaternion.Slerp(d.localRotation, Quaternion.identity, kScale);
                if (t != null && (d.localPosition - digitHome[s]).sqrMagnitude < 0.0004f) t.GetComponent<MeshRenderer>().sortingOrder = digitOrder[n - 1];
            }

            // il gradino sotto ciò che si trascina si illumina
            if (stepGlow != null)
            {
                Transform st = hover >= 0 ? Pick(steps, hover) : null;
                if (st != null)
                {
                    Vector3 p = Pick(stepAreas, hover) != null ? stepAreas[hover].bounds.center : st.position;
                    stepGlow.transform.position = new Vector3(st.position.x, Mathf.Min(p.y, st.position.y + 0.9f), stepGlow.transform.position.z);
                    HalloweenKit.SetAlpha(stepGlow, 0.5f + 0.2f * Mathf.Sin(Time.time * 8f));
                }
                else HalloweenKit.SetAlpha(stepGlow, 0f);
            }

            // facce, rivale, arbitro
            if (sadT > 0f) sadT = Mathf.Max(0f, sadT - dt);
            if (rivalShockT > 0f) rivalShockT = Mathf.Max(0f, rivalShockT - dt);
            if (whistleT > 0f) whistleT = Mathf.Max(0f, whistleT - dt);
            SetFace(sadT > 0f ? Face.Sad : Face.Happy);
            HalloweenKit.Show(rivalSmug, rivalShockT <= 0f);
            HalloweenKit.Show(rivalShock, rivalShockT > 0f);
            HalloweenKit.Show(refereeIdle, whistleT <= 0f);
            HalloweenKit.Show(refereeBlow, whistleT > 0f);
            if (tweetText != null) HalloweenKit.Show(tweetText.transform, whistleT > 0f);
            if (runnerHead != null) runnerHead.localRotation = Quaternion.Euler(0f, 0f, sadT > 0f ? -8f : 3f * Mathf.Sin(Time.time * 3f));
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73-0.82) ----------

        void FxPop(Transform t, float peak, float seconds) => Fx(0, t, HalloweenKit.Pop(t, peak, seconds));
        void FxWiggle(Transform t, float degrees, float seconds, float speed) => Fx(1, t, HalloweenKit.Wiggle(t, degrees, seconds, speed));

        bool Busy(Transform t, int kind = 0) => t != null && fxSlots[kind].TryGetValue(t, out FxSlot s) && s.co != null;

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

        // ---------- Dita ----------

        static bool Over(Collider2D c, Vector2 p) => c != null && c.gameObject.activeInHierarchy && c.OverlapPoint(p);

        /// <summary>Il gradino sotto ciò che si lascia (o sotto il dito), -1 nessuno. Aree generose, mai sovrapposte.</summary>
        int StepUnder(Vector2 thing, Vector2 finger)
        {
            if (stepAreas == null) return WrongPrizeLogic.NoStep;
            for (int i = 0; i < stepAreas.Length && i < Steps; i++) if (Over(stepAreas[i], thing)) return i;
            for (int i = 0; i < stepAreas.Length && i < Steps; i++) if (Over(stepAreas[i], finger)) return i;
            return WrongPrizeLogic.NoStep;
        }

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            seen.Clear();
            foreach (V64AFinger f in fingers)
            {
                seen.Add(f.id);
                if (f.began) Begin(f.id, f.world);
                if (finishing) return;
                if (logic.Dragging && logic.DragFinger == f.id)
                {
                    lastFinger = f.world;
                    logic.Move(f.id, f.world.x, f.world.y);
                    if (f.ended) End(f.id, f.world);
                    if (finishing) return;
                }
            }
            // il dito che trascinava è sparito senza "Ended": torna tutto a posto, niente conta
            if (logic.Dragging && !seen.Contains(logic.DragFinger)) logic.Cancel();
        }

        void Begin(int id, Vector2 w)
        {
            if (finishing || logic.InputLocked || logic.Dragging) return;   // un dito alla volta
            // 1) i numeri dei gradini (hanno la precedenza: aree generose sul davanti dei gradini)
            for (int n = 1; n <= Steps; n++)
            {
                if (!Over(Pick(digitAreas, n - 1), w)) continue;
                if (logic.Grab(id, WrongPrizePart.Digit, logic.StepOf(n), w.x, w.y) == WrongPrizeGrab.Digit)
                {
                    lastFinger = w;
                    grabOffset = (Vector2)Digit(n).position - w;
                    TextMesh t = Pick(digitTexts, n - 1);
                    if (t != null) t.GetComponent<MeshRenderer>().sortingOrder = DragOrder;
                    V83WrongPrizeSound.Play(this, "slide", 0.3f);
                    Haptics.Tap();
                }
                return;
            }
            // 2) il corridore
            if (Over(runnerArea, w))
            {
                if (logic.Grab(id, WrongPrizePart.Runner, WrongPrizeLogic.NoStep, w.x, w.y) == WrongPrizeGrab.Runner)
                {
                    lastFinger = w;
                    grabOffset = (Vector2)runner.position - w;
                    Haptics.Tap();
                }
                return;
            }
            // 3) la coppa avvitata
            if (Over(trophyArea, w))
            {
                if (logic.Grab(id, WrongPrizePart.Trophy, WrongPrizeLogic.CentreStep, w.x, w.y) == WrongPrizeGrab.Bolted)
                {
                    FxWiggle(trophy, 7f, 0.4f, 40f);
                    if (bolts != null) foreach (Transform b in bolts) FxWiggle(b, 25f, 0.4f, 40f);
                    V83WrongPrizeSound.Play(this, "clunk", 0.5f);
                    Haptics.Light();
                    toast.Show(TrophyLines[trophyLines++ % TrophyLines.Length], 1.6f);
                }
                return;
            }
            // 4) il rivale e l'arbitro (solo battute)
            if (Over(rivalArea, w))
            {
                FxPop(rival, 1.06f, 0.2f);
                V83WrongPrizeSound.Play(this, "boing", 0.3f);
                Speak(Speaker.Rival, string.Format(RivalPokeLines[rivalPokeLines++ % RivalPokeLines.Length], logic.RunnerPlace), 1.6f);
                return;
            }
            if (Over(refereeArea, w))
            {
                FxPop(referee, 1.06f, 0.2f);
                Whistle(0.5f);
                Speak(Speaker.Referee, RefereePokeLines[refPokeLines++ % RefereePokeLines.Length], 1.5f);
            }
        }

        void End(int id, Vector2 w)
        {
            if (logic.DragFinger != id) return;
            WrongPrizeGrab what = logic.Held;
            int number = logic.DragNumber;
            int target = what == WrongPrizeGrab.Runner
                ? StepUnder((Vector2)runner.position + new Vector2(0f, RunnerMidY), w)
                : StepUnder(Digit(number) != null ? (Vector2)Digit(number).position : w, w);
            WrongPrizeDrop drop = logic.Drop(id, target);
            switch (drop)
            {
                case WrongPrizeDrop.RunnerTap:
                    hopT = 0.35f;
                    V83WrongPrizeSound.Play(this, "boing", 0.3f);
                    Speak(Speaker.Runner, RunnerTapLines[tapLines++ % RunnerTapLines.Length], 1.4f);
                    break;
                case WrongPrizeDrop.RunnerSentBack:
                    Whistle(0.9f);
                    sadT = 1.4f;
                    Haptics.Light();
                    Speak(Speaker.Referee, SentBackLines[sentLines++ % SentBackLines.Length], 1.6f);
                    break;
                case WrongPrizeDrop.RunnerReturned:
                    V83WrongPrizeSound.Play(this, "slide", 0.25f);
                    toast.Show(ReturnLines[returnLines++ % ReturnLines.Length], 1.5f);
                    break;
                case WrongPrizeDrop.DigitTap:
                    FxWiggle(Digit(number), 14f, 0.35f, 32f);
                    V83WrongPrizeSound.Play(this, "boing", 0.25f);
                    toast.Show(DigitTapLines[digitTapLines++ % DigitTapLines.Length], 1.4f);
                    break;
                case WrongPrizeDrop.DigitReturned:
                    V83WrongPrizeSound.Play(this, "slide", 0.25f);
                    toast.Show(DigitBackLines[digitBackLines++ % DigitBackLines.Length], 1.4f);
                    break;
                case WrongPrizeDrop.NotYourStep:
                    V83WrongPrizeSound.Play(this, "buzz", 0.35f);
                    Haptics.Light();
                    if (target == WrongPrizeLogic.LeftStep) Speak(Speaker.Rival, RivalNoLines[rivalNoLines++ % RivalNoLines.Length], 1.5f);
                    else toast.Show(WrongStepLines[wrongStepLines++ % WrongStepLines.Length], 1.5f);
                    break;
                case WrongPrizeDrop.SecondPlace:
                    Swapped(target);
                    rivalShockT = 1.3f;
                    Speak(Speaker.Runner, SecondLines[secondLines++ % SecondLines.Length], 1.8f);
                    toast.Show(RivalSecondLine, 1.5f);
                    break;
                case WrongPrizeDrop.ThirdAgain:
                    Swapped(target);
                    sadT = 1.0f;
                    Speak(Speaker.Rival, ThirdLines[thirdLines++ % ThirdLines.Length], 1.6f);
                    break;
                case WrongPrizeDrop.FirstPlace:
                    StartCoroutine(Finale());
                    break;
            }
        }

        /// <summary>Uno scambio di numeri (non vincente): il gradino del corridore fa "pop", i numeri scivolano al nuovo posto.</summary>
        void Swapped(int step)
        {
            FxPop(Pick(steps, step), 1.06f, 0.2f);
            V83WrongPrizeSound.Play(this, "swap", 0.4f);
            Haptics.Medium();
        }

        void Whistle(float seconds)
        {
            whistleT = seconds;
            if (tweetText != null) FxPop(tweetText.transform, 1.3f, 0.2f);
            V83WrongPrizeSound.Play(this, "whistle", 0.45f);
        }

        // ---------- La soluzione ----------

        /// <summary>
        /// L'1 sul gradino del corridore: i numeri si incastrano (scintilla sull'1), l'arbitro fischia ("Rules are rules!"),
        /// i bulloni saltano, la coppa vola ad arco fra le mani del corridore che esulta, la medaglia gli cade al
        /// collo e il rivale resta a bocca aperta; festa comune; Win una volta sola. Rifatta da capo se il livello si spegne a
        /// metà.
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            V64AFingers.Reset();
            StopFx();
            HalloweenKit.Show(infoHolder, false);
            if (stepGlow != null) HalloweenKit.SetAlpha(stepGlow, 0f);
            HideBubbles();

            // 1) i numeri al nuovo posto: l'1 sul gradino basso, il 3 sul vecchio primo gradino
            var from = new Vector3[Steps + 1];
            for (int n = 1; n <= Steps; n++) if (Digit(n) != null) from[n] = Digit(n).localPosition;
            for (float t = 0f; t < 0.25f; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.25f));
                for (int n = 1; n <= Steps; n++)
                {
                    Transform d = Digit(n);
                    int s = logic.StepOf(n);
                    if (d == null || s < 0) continue;
                    d.localPosition = Vector3.Lerp(from[n], digitHome[s], k);
                    d.localRotation = Quaternion.Slerp(d.localRotation, Quaternion.identity, k);
                    d.localScale = Vector3.Lerp(d.localScale, digitScale[n - 1], k);
                }
                runner.localPosition = Vector3.Lerp(runner.localPosition, runnerPos, k);
                yield return null;
            }
            PoseAll();
            runner.localPosition = runnerPos;
            runner.localRotation = Quaternion.identity;
            Transform one = Digit(1);
            if (sparkle != null && one != null)
            {
                sparkle.position = new Vector3(one.position.x, one.position.y, sparkle.position.z);
                HalloweenKit.Show(sparkle, true);
                FxPop(sparkle, 1.5f, 0.35f);
            }
            FxPop(one, 1.35f, 0.3f);
            FxPop(Pick(steps, RunnerStep), 1.06f, 0.2f);
            V83WrongPrizeSound.Play(this, "ding", 0.55f);
            Haptics.Medium();
            whistleT = 0f;
            HalloweenKit.Show(refereeIdle, false);
            HalloweenKit.Show(refereeBlow, true);
            if (tweetText != null) { HalloweenKit.Show(tweetText.transform, true); FxPop(tweetText.transform, 1.3f, 0.2f); }
            V83WrongPrizeSound.Play(this, "whistle", 0.45f);
            sayReferee?.Show(RulesLine, 1.3f);
            toast.Show(NewFirstToast, 1.4f);
            yield return new WaitForSeconds(0.35f);

            // 2) i bulloni saltano, la coppa vola fra le mani del corridore
            V83WrongPrizeSound.Play(this, "pop", 0.5f);
            Vector3 tBase = trophy.position;
            if (puff != null)
            {
                puff.transform.position = new Vector3(tBase.x, tBase.y + 0.1f, puff.transform.position.z);
                puff.transform.localScale = puffScale;
                HalloweenKit.Show(puff, true);
                FxPop(puff.transform, 1.4f, 0.3f);
            }
            var boltFrom = new Vector3[boltPos.Length];
            for (int i = 0; i < boltPos.Length; i++) if (bolts[i] != null) boltFrom[i] = bolts[i].position;
            HalloweenKit.Show(runnerBody, false);
            HalloweenKit.Show(runnerCheer, true);
            SetFace(Face.Joy);
            HalloweenKit.Show(rivalSmug, false);
            HalloweenKit.Show(rivalShock, true);
            trophy.SetParent(transform, true);
            Vector3 tFrom = trophy.position, sFrom = trophy.localScale;
            Vector3 sTo = trophyScale * 0.78f;
            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.6f), e = Mathf.SmoothStep(0f, 1f, k);
                Vector3 to = runner.position + new Vector3(0f, HandsUpY * runner.localScale.y - 0.05f, 0f);
                Vector3 p = Vector3.Lerp(tFrom, to, e) + new Vector3(0f, 0.9f * Mathf.Sin(k * Mathf.PI), 0f);
                trophy.position = new Vector3(p.x, p.y, tFrom.z);
                trophy.localScale = Vector3.Lerp(sFrom, sTo, e);
                trophy.localRotation = Quaternion.Euler(0f, 0f, -360f * e);
                for (int i = 0; i < boltPos.Length; i++)
                {
                    if (bolts[i] == null) continue;
                    float side = i % 2 == 0 ? -1f : 1f;
                    // i bulloni schizzano via girando e cadono
                    Vector3 bp = boltFrom[i] + new Vector3(side * 1.2f * k, 1.4f * k - 3.2f * k * k, 0f);
                    bp.x = Mathf.Clamp(bp.x, MinX, MaxX);
                    bolts[i].position = bp;
                    bolts[i].localRotation = Quaternion.Euler(0f, 0f, 720f * k * side);
                }
                yield return null;
            }
            trophy.localRotation = Quaternion.identity;
            foreach (Transform b in bolts ?? new Transform[0]) HalloweenKit.Show(b, false);
            if (puff != null) HalloweenKit.Show(puff, false);
            HalloweenKit.Show(sparkle, false);

            // 3) la medaglia cade al collo; il corridore saltella di gioia
            if (medal != null)
            {
                HalloweenKit.Show(medal, true);
                Vector3 mTo = medalPos, mFrom = medalPos + new Vector3(0f, 1.6f, 0f);
                for (float t = 0f; t < 0.3f; t += Time.deltaTime)
                {
                    float k = Mathf.Clamp01(t / 0.3f);
                    medal.localPosition = Vector3.Lerp(mFrom, mTo, k * k);
                    yield return null;
                }
                medal.localPosition = mTo;
                FxPop(medal, 1.3f, 0.25f);
            }
            V83WrongPrizeSound.Play(this, "fanfare", 0.55f);
            Haptics.Success();
            sayRunner?.Show("I WON!", 1.6f);
            StartCoroutine(Hops());

            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            if (logic.TryComplete()) Win(Compliment);
        }

        /// <summary>Saltelli di gioia del corridore con la coppa alzata (la coppa lo segue).</summary>
        IEnumerator Hops()
        {
            Vector3 tOff = trophy.position - runner.position;
            for (float t = 0f; t < 1.8f && !IsFinished; t += Time.deltaTime)
            {
                float y = 0.18f * Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2.5f));
                runner.localPosition = runnerPos + new Vector3(0f, y, 0f);
                trophy.position = runner.position + tOff;
                yield return null;
            }
            runner.localPosition = runnerPos;
            trophy.position = runner.position + tOff;
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        const int KeyFinger = 901;

        void EditorKeys()
        {
            if (logic.InputLocked || logic.Dragging) return;
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) KeyDragDigit(1, RunnerStep);
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) KeyDragDigit(2, RunnerStep);
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) KeyDragDigit(3, RunnerStep);
            if (Input.GetKeyDown(KeyCode.Q)) KeyDragDigit(1, WrongPrizeLogic.LeftStep);
            if (Input.GetKeyDown(KeyCode.N)) KeyDragDigit(1, WrongPrizeLogic.NoStep);
            if (Input.GetKeyDown(KeyCode.C)) KeyDragRunner(logic.FirstStep);
            if (Input.GetKeyDown(KeyCode.V)) KeyDragRunner(WrongPrizeLogic.NoStep);
            if (Input.GetKeyDown(KeyCode.T) && runnerArea != null) { Vector2 p = runnerArea.bounds.center; Begin(KeyFinger, p); End(KeyFinger, p); }
            if (Input.GetKeyDown(KeyCode.B) && trophyArea != null) Begin(KeyFinger, trophyArea.bounds.center);
            if (Input.GetKeyDown(KeyCode.K) && rivalArea != null) Begin(KeyFinger, rivalArea.bounds.center);
            if (Input.GetKeyDown(KeyCode.L) && refereeArea != null) Begin(KeyFinger, refereeArea.bounds.center);
            if (Input.GetKeyDown(KeyCode.H)) KeyDragDigit(1, RunnerStep);
            if (Input.GetKeyDown(KeyCode.P)) CancelAll();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }

        /// <summary>Il punto (mondo) in cui lasciare qualcosa sul gradino indicato, o un punto vuoto.</summary>
        Vector2 KeyTarget(int step)
        {
            Collider2D a = step >= 0 ? Pick(stepAreas, step) : null;
            return a != null ? (Vector2)a.bounds.center : new Vector2(0f, -3.4f);
        }

        /// <summary>Un dito finto prende il numero, lo porta sul gradino indicato (o nel vuoto) e lo lascia.</summary>
        void KeyDragDigit(int number, int step)
        {
            Transform d = Digit(number);
            if (d == null) return;
            Begin(KeyFinger, d.position);
            if (logic.DragFinger != KeyFinger) return;
            Vector2 to = KeyTarget(step);
            lastFinger = to;
            logic.Move(KeyFinger, to.x, to.y);
            d.position = new Vector3(to.x, to.y, d.position.z);
            End(KeyFinger, to);
        }

        void KeyDragRunner(int step)
        {
            if (runnerArea == null) return;
            Begin(KeyFinger, runnerArea.bounds.center);
            if (logic.DragFinger != KeyFinger) return;
            Vector2 to = KeyTarget(step);
            lastFinger = to;
            logic.Move(KeyFinger, to.x, to.y);
            runner.position = new Vector3(to.x, to.y - RunnerMidY, runner.position.z);
            End(KeyFinger, to);
        }
#endif
    }

    /// <summary>Suoni sintetizzati di The Wrong Prize: fischietto, bullone, "boing", fruscio, scambio, ronzio, "ding", "pop" e fanfara.</summary>
    public static class V83WrongPrizeSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(472);

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
                case "whistle":   // fischietto dell'arbitro: tono acuto con il trillo della pallina
                    c = Make(kind, 0.5f, t =>
                    {
                        float trill = 1f + 0.04f * Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * 28f));
                        float env = Mathf.Min(1f, t * 60f) * Mathf.Clamp01((0.5f - t) * 12f);
                        return (Mathf.Sin(t * 2f * Mathf.PI * 2700f * trill) * 0.22f + N() * 0.03f) * env;
                    });
                    break;
                case "clunk":     // la coppa avvitata che non si muove
                    c = Make(kind, 0.22f, t => (Mathf.Sin(t * 2f * Mathf.PI * 180f) * 0.4f + N() * 0.15f) * Mathf.Exp(-t * 22f));
                    break;
                case "boing":
                    c = Make(kind, 0.3f, t => Mathf.Sin(t * 2f * Mathf.PI * (300f + 120f * Mathf.Sin(t * 60f))) * 0.3f * Mathf.Exp(-t * 9f));
                    break;
                case "slide":     // legno che striscia
                    {
                        float lp = 0f;
                        c = Make(kind, 0.18f, t =>
                        {
                            lp += (N() - lp) * 0.25f;
                            return lp * 0.45f * Mathf.Sin(t / 0.18f * Mathf.PI);
                        });
                        break;
                    }
                case "swap":      // due note che si scambiano
                    c = Make(kind, 0.3f, t => Mathf.Sin(t * 2f * Mathf.PI * (t < 0.15f ? 523f : 392f)) * 0.22f * Mathf.Exp(-(t % 0.15f) * 12f));
                    break;
                case "buzz":
                    c = Make(kind, 0.28f, t =>
                    {
                        float saw = 2f * ((t * 140f) % 1f) - 1f;
                        return saw * 0.2f * Mathf.Min(1f, t * 80f) * Mathf.Clamp01((0.28f - t) * 20f);
                    });
                    break;
                case "ding":
                    c = Make(kind, 0.5f, t => (Mathf.Sin(t * 2f * Mathf.PI * 1320f) + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 1980f)) * 0.2f * Mathf.Exp(-t * 6f));
                    break;
                case "pop":       // i bulloni saltano
                    c = Make(kind, 0.18f, t => N() * 0.5f * Mathf.Exp(-t * 40f) + Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(500f, 180f, t / 0.18f)) * 0.35f * Mathf.Exp(-t * 18f));
                    break;
                case "fanfare":   // ta-ta-taaa
                    c = Make(kind, 0.9f, t =>
                    {
                        float f = t < 0.15f ? 523f : t < 0.3f ? 659f : 784f;
                        float s = t < 0.15f ? t : t < 0.3f ? t - 0.15f : t - 0.3f;
                        float sq = Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * f)) * 0.5f + Mathf.Sin(t * 2f * Mathf.PI * f);
                        return sq * 0.12f * Mathf.Min(1f, s * 60f) * (t < 0.3f ? Mathf.Exp(-s * 8f) : Mathf.Exp(-s * 2.5f));
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
            AudioClip clip = AudioClip.Create("v83a_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
