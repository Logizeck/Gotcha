using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 44 della roadmap "The Stupid Genius 2" (id stupid_genius_2) — "PROVE YOU'RE SMARTER THAN THE GENIUS." In
    /// aula un genio arrogante (testone, occhialoni, camice, mani sui fianchi) accanto alla lavagna con "2 + 2 = 5"; sotto,
    /// sulla console dei quiz, tre pulsanti-risposta: 3, 4 e 5.
    /// Inganno: prima della correzione ogni risposta è sbagliata. 4: "WRONG! The board says 5. I'm a GENIUS."; 5: "Ha! You
    /// just agreed with a wrong equation. Wrong!"; 3: "Not even close.". Il 2, il + e l'= della lavagna non si muovono
    /// (oscillano, il genio protesta); il 5 toccato senza trascinarlo: "Don't touch my 5."; trascinato e lasciato su 3 o
    /// su 5: torna sulla lavagna ("That doesn't help."); lasciato altrove: torna sulla lavagna. Tutto reversibile.
    /// Soluzione: trascinare il 5 della lavagna sul pulsante del 4. Il 5 rimbalza giù dalla console e si frantuma in
    /// polvere di gesso, una copia del 4 vola dal pulsante nell'equazione (la lavagna dice 2 + 2 = 4); il pulsante del 4
    /// resta un 4 e si illumina; il genio va nel panico ("W-wait... 2 + 2 = 4?!"). Poi si preme il pulsante il cui VALORE
    /// è uguale al risultato della lavagna (StupidGenius2Logic.Answer riceve il numero mostrato, mai l'indice): 3 o 5 dopo
    /// la correzione: "Wrong. And now the board agrees.".
    /// Vittoria: il genio resta di sasso ("IMPOSSIBLE!"), i capelli gli si drizzano, gli occhiali volano via, si gonfia e
    /// fa "pop" in una nuvoletta; LevelReward "SMARTER THAN A GENIUS!" -> Win una volta sola.
    /// Dita: V64AFingers (al massimo due id; un solo dito trascina il 5; le risposte non contano mentre il 5 è in mano).
    /// Pausa, app in background o livello spento: il 5 torna sulla lavagna; se lo scambio era a metà, si completa subito.
    /// Tasti di prova nell'Editor: 3/4/5 premono il pulsante con quel VALORE, F trascina il 5 sul 4, G sul 3, J sul 5,
    /// D lo trascina e lo lascia nel vuoto, T tocca il 5, Q tocca il +, X tocca il genio, H soluzione completa, P pausa
    /// simulata (dita perse), R ricomincia.
    /// </summary>
    public class V82StupidGenius2Level : LevelController
    {
        [Header("La lavagna: 0 \"2\", 1 \"+\", 2 \"2\", 3 \"=\", 4 il risultato (collider sui genitori)")]
        [SerializeField] Transform[] symbols;
        [SerializeField] Collider2D[] symbolAreas;
        [SerializeField] TextMesh resultText;
        [SerializeField] TextMesh newDigit;        // la copia del 4 che vola dal pulsante all'equazione
        [SerializeField] Transform sparkle;
        [Header("Il genio (radice a terra, testa sul collo)")]
        [SerializeField] Transform genius;
        [SerializeField] Collider2D geniusArea;
        [SerializeField] Transform head;
        [SerializeField] Transform[] faces;        // 0 sufficienza, 1 urlo, 2 panico, 3 stupore
        [SerializeField] Transform hair;
        [SerializeField] Transform hairUp;
        [SerializeField] Transform glasses;
        [Header("Pulsanti-risposta (il valore logico è buttonValues, l'etichetta lo mostra sempre)")]
        [SerializeField] Transform[] buttons;
        [SerializeField] Transform[] buttonBodies;
        [SerializeField] Collider2D[] buttonAreas;
        [SerializeField] TextMesh[] buttonLabels;
        [SerializeField] int[] buttonValues = { 3, 4, 5 };
        [SerializeField] SpriteRenderer answerGlow;
        [Header("Il 5 che si frantuma")]
        [SerializeField] SpriteRenderer puff;
        [SerializeField] Transform[] chalkBits;
        [Header("Fumetto, scritte e festa")]
        [SerializeField] SpeechBubble bubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "SMARTER THAN A GENIUS!";
        const int Res = StupidGenius2Logic.ResultSymbol;
        const int DragOrder = 45;
        const float Follow = 18f, LiftScale = 1.15f, HoverScale = 1.1f, ShoutTime = 0.9f, CrashY = -4.2f;
        const float MinX = -2.45f, MaxX = 2.45f, MinY = -4.3f, MaxY = 2.85f;

        static readonly string[] FourLines = { "WRONG! The board says 5. I'm a GENIUS.", "4? Pfff. Read the board: 5!", "WRONG! My board is never wrong." };
        static readonly string[] FiveLines = { "Ha! You just agreed with a wrong equation. Wrong!", "Copying my board? Still WRONG!", "Ha! 5 is wrong. And so are you." };
        static readonly string[] ThreeLines = { "Not even close.", "3? Seriously?", "Colder than my coffee." };
        static readonly string[] AfterLines = { "Wrong. And now the board agrees.", "Still wrong. Look at the board!" };
        static readonly string[] GrabLines = { "Hey! That's MY 5!", "Put my 5 down!", "Careful with my 5!" };
        static readonly string[] TapFiveLines = { "Don't touch my 5.", "That 5 is PERFECT." };
        static readonly string[] SymbolLines = { "That 2 is perfect.", "The plus is fine, thanks.", "That 2 is perfect too.", "Don't touch my equals!" };
        static readonly string[] FixedBoardLines = { "Leave the board alone!", "D-don't touch it. It's... correct." };
        static readonly string[] PokeLines = { "Don't poke a GENIUS.", "My IQ is bigger than this room.", "Answer the question, please." };
        static readonly string[] HelpLines = { "That doesn't help.", "Nope. That doesn't help either.", "The 5 bounces back to the board." };
        static readonly string[] BackLines = { "The 5 slides back onto the board.", "Back to the board it goes." };
        const string PanicLine = "W-wait... 2 + 2 = 4?!";
        const string FixedToast = "Equation fixed!";
        const string ShockLine = "IMPOSSIBLE!";

        enum Face { Smug = 0, Shout = 1, Panic = 2, Shock = 3 }

        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        StupidGenius2Logic logic;
        Camera cam;
        HwToast toast;
        HwSay say;
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        readonly HashSet<int> seen = new HashSet<int>();
        Vector3[] symbolHome, buttonScale;
        Vector3 geniusPos, geniusScale, headPos, glassesPos, glassesScale, resultScale, puffScale;
        Quaternion glassesRot;
        Transform glassesParent;
        string startResult;
        int resultOrder;
        Vector2 grabOffset, lastFinger;
        float shoutT;
        int fourLines, fiveLines, threeLines, afterLines, grabLines, tapLines, fixedLines, pokeLines, helpLines, backLines;
        bool captured, finishing, animatingFix, complained;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            if (!captured)
            {
                captured = true;
                logic = new StupidGenius2Logic();
                symbolHome = new Vector3[symbols.Length];
                for (int i = 0; i < symbols.Length; i++) if (symbols[i] != null) symbolHome[i] = symbols[i].localPosition;
                buttonScale = new Vector3[buttonBodies != null ? buttonBodies.Length : 0];
                for (int i = 0; i < buttonScale.Length; i++) if (buttonBodies[i] != null) buttonScale[i] = buttonBodies[i].localScale;
                geniusPos = genius.localPosition;
                geniusScale = genius.localScale;
                headPos = head.localPosition;
                if (glasses != null)
                {
                    glassesParent = glasses.parent;
                    glassesPos = glasses.localPosition;
                    glassesRot = glasses.localRotation;
                    glassesScale = glasses.localScale;
                }
                resultScale = symbols[Res].localScale;
                startResult = resultText.text;
                resultOrder = resultText.GetComponent<MeshRenderer>().sortingOrder;
                if (puff != null) puffScale = puff.transform.localScale;
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            V64AFingers.Reset();
            seen.Clear();
            finishing = animatingFix = complained = false;
            shoutT = 0f;
            fourLines = fiveLines = threeLines = afterLines = grabLines = tapLines = fixedLines = pokeLines = helpLines = backLines = 0;
            toast = new HwToast(infoText, infoHolder);
            say = new HwSay(bubble);
            RestoreGenius();
            for (int i = 0; buttonLabels != null && i < buttonLabels.Length; i++)
                if (buttonLabels[i] != null) buttonLabels[i].text = Value(i).ToString();   // l'etichetta mostra sempre il valore logico
            PoseBoard();
        }

        int Value(int button) => buttonValues != null && button >= 0 && button < buttonValues.Length ? buttonValues[button] : StupidGenius2Logic.NoTarget;

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestoreGenius()
        {
            say?.Hide();
            genius.localPosition = geniusPos;
            genius.localScale = geniusScale;
            genius.localRotation = Quaternion.identity;
            head.localPosition = headPos;
            head.localRotation = Quaternion.identity;
            head.localScale = Vector3.one;
            if (glasses != null)
            {
                glasses.SetParent(glassesParent, false);
                glasses.localPosition = glassesPos;
                glasses.localRotation = glassesRot;
                glasses.localScale = glassesScale;
                glasses.gameObject.SetActive(true);
            }
            HalloweenKit.Show(hair, true);
            HalloweenKit.Show(hairUp, false);
            SetFace(Face.Smug);
            HalloweenKit.Show(genius, true);
        }

        /// <summary>La lavagna e i pulsanti come dice la logica: risultato al suo posto, niente in volo, bagliore sul pulsante giusto.</summary>
        void PoseBoard()
        {
            resultText.text = logic.BoardFixed ? logic.BoardResult.ToString() : startResult;
            resultText.GetComponent<MeshRenderer>().sortingOrder = resultOrder;
            for (int i = 0; i < symbols.Length; i++)
            {
                if (symbols[i] == null) continue;
                symbols[i].localPosition = symbolHome[i];
                symbols[i].localRotation = Quaternion.identity;
            }
            symbols[Res].localScale = resultScale;
            HalloweenKit.Show(symbols[Res], true);
            if (newDigit != null) HalloweenKit.Show(newDigit.transform, false);
            HalloweenKit.Show(sparkle, false);
            if (puff != null) { HalloweenKit.Show(puff, false); puff.transform.localScale = puffScale; }
            if (chalkBits != null) foreach (Transform b in chalkBits) HalloweenKit.Show(b, false);
            for (int i = 0; i < buttonScale.Length; i++) if (buttonBodies[i] != null) buttonBodies[i].localScale = buttonScale[i];
            if (answerGlow != null) HalloweenKit.SetAlpha(answerGlow, 0f);
        }

        void SetFace(Face f)
        {
            if (faces == null) return;
            for (int i = 0; i < faces.Length; i++) HalloweenKit.Show(faces[i], i == (int)f);
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: il 5 torna a posto; uno scambio a metà si completa subito
            CancelAll();
            StopFx();
            animatingFix = false;
            if (!captured || logic == null || IsFinished || finishing) return;
            if (logic.Stage == StupidGenius2Stage.Fixing) logic.FinishFix();
            PoseBoard();
            say?.Hide();
            shoutT = 0f;
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == StupidGenius2Stage.Celebrating)
            {
                finishing = false;
                RestoreGenius();
                PoseBoard();
                StartCoroutine(Finale(logic.CorrectButton(buttonValues)));
            }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelAll(); }

        /// <summary>Le dita spariscono: il 5 torna sulla lavagna (Update lo fa scivolare), niente conta.</summary>
        void CancelAll()
        {
            V64AFingers.Reset();
            if (logic != null) logic.Cancel();
            complained = false;
        }

        void Update()
        {
            toast?.Tick();
            say?.Tick();
            if (IsFinished || toast == null || logic == null || finishing) return;
            if (Time.timeScale <= 0f) { CancelAll(); return; }   // pausa o pubblicità: niente input
#if UNITY_EDITOR
            EditorKeys();
#endif
            if (!finishing && !animatingFix) ReadFingers();
            if (!finishing) Pose();
        }

        /// <summary>Il 5 segue il dito (o torna a casa), il pulsante sotto il 5 si gonfia, il genio dondola la testa, il bagliore pulsa.</summary>
        void Pose()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            Transform five = symbols[Res];
            int hover = -1;
            if (!animatingFix)
            {
                if (logic.Dragging)
                {
                    Vector2 want = lastFinger + grabOffset;
                    want.x = Mathf.Clamp(want.x, MinX, MaxX);
                    want.y = Mathf.Clamp(want.y, MinY, MaxY);
                    five.position = Vector3.Lerp(five.position, new Vector3(want.x, want.y, five.position.z), 1f - Mathf.Exp(-dt * Follow * 2f));
                    five.localScale = Vector3.Lerp(five.localScale, resultScale * LiftScale, 1f - Mathf.Exp(-dt * 14f));
                    five.localRotation = Quaternion.Euler(0f, 0f, 8f * Mathf.Sin(Time.time * 6f));
                    hover = TargetUnder(five.position, lastFinger);
                }
                else
                {
                    five.localPosition = Vector3.Lerp(five.localPosition, symbolHome[Res], 1f - Mathf.Exp(-dt * Follow));
                    if (!Busy(five, 0)) five.localScale = Vector3.Lerp(five.localScale, resultScale, 1f - Mathf.Exp(-dt * 14f));
                    if (!Busy(five, 1)) five.localRotation = Quaternion.Slerp(five.localRotation, Quaternion.identity, 1f - Mathf.Exp(-dt * 14f));
                    if ((five.localPosition - symbolHome[Res]).sqrMagnitude < 0.0004f) resultText.GetComponent<MeshRenderer>().sortingOrder = resultOrder;
                }
            }
            int glowing = logic.Stage == StupidGenius2Stage.Fixed ? logic.CorrectButton(buttonValues) : -1;
            for (int i = 0; i < buttonScale.Length; i++)
            {
                if (buttonBodies[i] == null || Busy(buttonBodies[i])) continue;
                // sotto il 5 trascinato si gonfia; dopo la correzione il pulsante giusto (valore 4) pulsa piano
                float k = i == hover ? HoverScale : i == glowing ? 1f + 0.06f * Mathf.Abs(Mathf.Sin(Time.time * 4f)) : 1f;
                Vector3 want = buttonScale[i] * k;
                buttonBodies[i].localScale = Vector3.Lerp(buttonBodies[i].localScale, want, 1f - Mathf.Exp(-dt * 16f));
            }

            // il genio: testa che dondola tronfia, che trema quando urla
            if (shoutT > 0f) shoutT = Mathf.Max(0f, shoutT - dt);
            Face f = shoutT > 0f ? Face.Shout : logic.BoardFixed ? Face.Panic : Face.Smug;
            SetFace(f);
            float rot = f == Face.Shout ? 4f * Mathf.Sin(Time.time * 40f) : f == Face.Panic ? 2.5f * Mathf.Sin(Time.time * 25f) : 3f * Mathf.Sin(Time.time * 2f);
            head.localRotation = Quaternion.Euler(0f, 0f, rot);
            head.localPosition = headPos + new Vector3(0f, f == Face.Smug ? 0.02f * Mathf.Sin(Time.time * 2f) : 0f, 0f);

            // il bagliore sul pulsante giusto dopo la correzione
            if (answerGlow != null)
            {
                int c = logic.Stage == StupidGenius2Stage.Fixed ? logic.CorrectButton(buttonValues) : -1;
                if (c >= 0 && c < buttons.Length && buttons[c] != null)
                {
                    Vector3 p = buttons[c].position;
                    answerGlow.transform.position = new Vector3(p.x, p.y, answerGlow.transform.position.z);
                    HalloweenKit.SetAlpha(answerGlow, 0.45f + 0.25f * Mathf.Sin(Time.time * 5f));
                }
                else HalloweenKit.SetAlpha(answerGlow, 0f);
            }
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73-0.81) ----------

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

        /// <summary>Il pulsante sotto il 5 (o sotto il dito), -1 nessuno. Aree generose, mai sovrapposte.</summary>
        int TargetUnder(Vector2 digit, Vector2 finger)
        {
            if (buttonAreas == null) return -1;
            for (int i = 0; i < buttonAreas.Length; i++) if (Over(buttonAreas[i], digit)) return i;
            for (int i = 0; i < buttonAreas.Length; i++) if (Over(buttonAreas[i], finger)) return i;
            return -1;
        }

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            seen.Clear();
            foreach (V64AFinger f in fingers)
            {
                seen.Add(f.id);
                if (f.began) Begin(f.id, f.world);
                if (finishing || animatingFix) return;
                if (logic.Dragging && logic.DragFinger == f.id)
                {
                    lastFinger = f.world;
                    if (logic.Move(f.id, f.world.x, f.world.y) && logic.DragMoved && !complained)
                    {
                        // la prima volta che il 5 si stacca davvero: il genio protesta
                        complained = true;
                        Shout(GrabLines[grabLines++ % GrabLines.Length], 1.4f);
                    }
                    if (f.ended) End(f.id, f.world);
                    if (finishing || animatingFix) return;
                }
            }
            // il dito che trascinava è sparito senza "Ended": il 5 torna a posto, niente conta
            if (logic.Dragging && !seen.Contains(logic.DragFinger)) { logic.Cancel(); complained = false; }
        }

        void Begin(int id, Vector2 w)
        {
            if (finishing || animatingFix || logic.InputLocked || logic.Dragging) return;   // un dito alla volta
            // 1) il risultato della lavagna (ha la precedenza: area generosa)
            if (Pick(symbolAreas, Res) != null && Over(symbolAreas[Res], w))
            {
                StupidGenius2Grab g = logic.Grab(id, Res, w.x, w.y);
                if (g == StupidGenius2Grab.Dragging)
                {
                    lastFinger = w;
                    grabOffset = (Vector2)symbols[Res].position - w;
                    complained = false;
                    resultText.GetComponent<MeshRenderer>().sortingOrder = DragOrder;
                    V82StupidGenius2Sound.Play(this, "chalk", 0.35f);
                    Haptics.Tap();
                }
                else if (g == StupidGenius2Grab.NotDraggable) WiggleSymbol(Res);
                return;
            }
            // 2) gli altri simboli: oscillano
            for (int s = 0; s < Res; s++)
            {
                if (!Over(Pick(symbolAreas, s), w)) continue;
                if (logic.Grab(id, s, w.x, w.y) == StupidGenius2Grab.NotDraggable) WiggleSymbol(s);
                return;
            }
            // 3) i pulsanti-risposta (per valore)
            for (int i = 0; buttonAreas != null && i < buttonAreas.Length; i++)
            {
                if (!Over(buttonAreas[i], w)) continue;
                Press(i);
                return;
            }
            // 4) il genio
            if (Over(geniusArea, w))
            {
                FxPop(head, 1.08f, 0.25f);
                V82StupidGenius2Sound.Play(this, "boing", 0.35f);
                Shout(PokeLines[pokeLines++ % PokeLines.Length], 1.5f);
            }
        }

        static T Pick<T>(T[] a, int i) where T : class => a != null && i >= 0 && i < a.Length ? a[i] : null;

        void End(int id, Vector2 w)
        {
            if (logic.DragFinger != id) return;
            int target = TargetUnder(symbols[Res].position, w);
            StupidGenius2Drop drop = logic.Drop(id, target >= 0 ? Value(target) : StupidGenius2Logic.NoTarget);
            complained = false;
            switch (drop)
            {
                case StupidGenius2Drop.Tap:
                    WiggleSymbol(Res, false);
                    Shout(TapFiveLines[tapLines++ % TapFiveLines.Length], 1.4f);
                    break;
                case StupidGenius2Drop.Returned:
                    V82StupidGenius2Sound.Play(this, "chalk", 0.25f);
                    toast.Show(BackLines[backLines++ % BackLines.Length], 1.6f);
                    break;
                case StupidGenius2Drop.DoesntHelp:
                    FxWiggle(Pick(buttonBodies, target), 12f, 0.35f, 30f);
                    V82StupidGenius2Sound.Play(this, "buzz", 0.4f);
                    Haptics.Light();
                    toast.Show(HelpLines[helpLines++ % HelpLines.Length], 1.7f);
                    break;
                case StupidGenius2Drop.Fixed:
                    StartCoroutine(FixSequence(target));
                    break;
            }
        }

        void Shout(string line, float seconds)
        {
            shoutT = ShoutTime;
            say?.Show(line, seconds);
        }

        // ---------- Le false piste ----------

        void WiggleSymbol(int s, bool talk = true)
        {
            FxWiggle(Pick(symbols, s), 14f, 0.35f, 32f);
            V82StupidGenius2Sound.Play(this, "boing", 0.3f);
            if (!talk) return;
            if (logic.BoardFixed) Shout(FixedBoardLines[fixedLines++ % FixedBoardLines.Length], 1.5f);
            else Shout(SymbolLines[Mathf.Clamp(s, 0, SymbolLines.Length - 1)], 1.4f);
        }

        void Press(int i)
        {
            StupidGenius2Answer a = logic.Answer(Value(i));
            if (a == StupidGenius2Answer.None) return;
            if (a == StupidGenius2Answer.Correct) { StartCoroutine(Finale(i)); return; }
            FxPop(Pick(buttonBodies, i), 0.88f, 0.18f);
            V82StupidGenius2Sound.Play(this, "buzz", 0.45f);
            Haptics.Light();
            switch (a)
            {
                case StupidGenius2Answer.BoardSaysFive: Shout(FourLines[fourLines++ % FourLines.Length], 2.0f); break;
                case StupidGenius2Answer.AgreedWithBoard: Shout(FiveLines[fiveLines++ % FiveLines.Length], 2.0f); break;
                case StupidGenius2Answer.NotEvenClose: Shout(ThreeLines[threeLines++ % ThreeLines.Length], 1.6f); break;
                case StupidGenius2Answer.BoardAgreesWrong: Shout(AfterLines[afterLines++ % AfterLines.Length], 1.8f); break;
            }
        }

        // ---------- La correzione ----------

        /// <summary>
        /// Lo scambio: il 5 rimbalza giù dal pulsante del 4 e si frantuma in polvere di gesso; intanto una copia del 4 vola
        /// dal pulsante al posto del risultato (la lavagna dice 2 + 2 = 4); il pulsante resta un 4 e si illumina; il genio
        /// va nel panico. Input bloccato per meno di un secondo; spento a metà, OnDisable completa lo scambio.
        /// </summary>
        IEnumerator FixSequence(int button)
        {
            animatingFix = true;
            say?.Hide();
            shoutT = 0f;
            Transform five = symbols[Res];
            Transform btn = Pick(buttonBodies, button) ?? Pick(buttons, button);
            FxPop(btn, 1.18f, 0.25f);
            V82StupidGenius2Sound.Play(this, "boing", 0.45f);
            Haptics.Medium();

            Vector3 fivePos = five.position;
            Vector2 vel = new Vector2(fivePos.x > 0f ? 0.9f : -0.9f, 2.6f);
            float spin = fivePos.x > 0f ? -420f : 420f, ang = 0f;
            bool crashed = false;
            Vector3 flyFrom = Pick(buttonLabels, button) != null ? buttonLabels[button].transform.position : (btn != null ? btn.position : fivePos);
            Vector3 flyTo = five.parent != null ? five.parent.TransformPoint(symbolHome[Res]) : symbolHome[Res];
            Color inkCol = Pick(buttonLabels, button) != null ? buttonLabels[button].color : Color.black;
            Color chalkCol = resultText.color;
            Vector3 digitFrom = Vector3.one * 0.75f, digitTo = Vector3.one;
            if (newDigit != null)
            {
                newDigit.text = StupidGenius2Logic.TrueSum.ToString();
                newDigit.transform.position = new Vector3(flyFrom.x, flyFrom.y, newDigit.transform.position.z);
                newDigit.transform.localScale = digitFrom;
                newDigit.color = inkCol;
                HalloweenKit.Show(newDigit.transform, true);
            }
            const float total = 0.8f, flyStart = 0.12f, flyTime = 0.55f;
            for (float t = 0f; t < total; t += Time.deltaTime)
            {
                float dt = Mathf.Min(Time.deltaTime, 0.05f);
                if (!crashed)
                {
                    vel.y -= 14f * dt;
                    fivePos += new Vector3(vel.x * dt, vel.y * dt, 0f);
                    fivePos.x = Mathf.Clamp(fivePos.x, MinX, MaxX);
                    ang += spin * dt;
                    five.position = fivePos;
                    five.localRotation = Quaternion.Euler(0f, 0f, ang);
                    if (fivePos.y <= CrashY) { crashed = true; Crash(fivePos); }
                }
                if (newDigit != null && t >= flyStart)
                {
                    float k = Mathf.Clamp01((t - flyStart) / flyTime);
                    float e = Mathf.SmoothStep(0f, 1f, k);
                    Vector3 p = Vector3.Lerp(flyFrom, flyTo, e) + new Vector3(-0.9f * Mathf.Sin(k * Mathf.PI), 0f, 0f);
                    newDigit.transform.position = new Vector3(p.x, p.y, newDigit.transform.position.z);
                    newDigit.transform.localScale = Vector3.Lerp(digitFrom, digitTo, e);
                    newDigit.transform.localRotation = Quaternion.Euler(0f, 0f, 360f * e);
                    newDigit.color = Color.Lerp(inkCol, chalkCol, e);
                }
                yield return null;
            }
            if (!crashed) Crash(five.position);

            // la lavagna corretta: il 4 prende il posto del 5
            logic.FinishFix();
            PoseBoard();
            HalloweenKit.Show(sparkle, true);
            FxPop(sparkle, 1.5f, 0.35f);
            FxPop(symbols[Res], 1.3f, 0.3f);
            V82StupidGenius2Sound.Play(this, "chime", 0.5f);
            Haptics.Success();
            SetFace(Face.Panic);
            say?.Show(PanicLine, 2.2f);
            toast.Show(FixedToast, 1.6f);
            animatingFix = false;
            yield return new WaitForSeconds(0.45f);
            HalloweenKit.Show(sparkle, false);
        }

        /// <summary>Il 5 si frantuma: sparisce, nuvoletta di gesso e frammenti che schizzano.</summary>
        void Crash(Vector3 at)
        {
            HalloweenKit.Show(symbols[Res], false);
            V82StupidGenius2Sound.Play(this, "smash", 0.5f);
            if (puff != null)
            {
                puff.transform.position = new Vector3(at.x, at.y, puff.transform.position.z);
                HalloweenKit.Show(puff, true);
                FxPop(puff.transform, 1.4f, 0.3f);
            }
            if (chalkBits != null && chalkBits.Length > 0) StartCoroutine(ChalkBurst(at));
        }

        IEnumerator ChalkBurst(Vector3 at)
        {
            int n = chalkBits.Length;
            var v = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                if (chalkBits[i] == null) continue;
                float a = Mathf.Lerp(25f, 155f, n > 1 ? i / (float)(n - 1) : 0.5f) * Mathf.Deg2Rad;
                v[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (2.2f + 0.4f * (i % 2));
                chalkBits[i].position = new Vector3(at.x, at.y, chalkBits[i].position.z);
                chalkBits[i].localScale = Vector3.one;
                HalloweenKit.Show(chalkBits[i], true);
            }
            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                float dt = Mathf.Min(Time.deltaTime, 0.05f);
                for (int i = 0; i < n; i++)
                {
                    if (chalkBits[i] == null) continue;
                    v[i].y -= 9f * dt;
                    Vector3 p = chalkBits[i].position + new Vector3(v[i].x * dt, v[i].y * dt, 0f);
                    p.x = Mathf.Clamp(p.x, MinX, MaxX);
                    chalkBits[i].position = p;
                    chalkBits[i].localRotation = Quaternion.Euler(0f, 0f, t * 720f * (i % 2 == 0 ? 1f : -1f));
                    chalkBits[i].localScale = Vector3.one * (1f - t / 0.5f);
                }
                yield return null;
            }
            foreach (Transform b in chalkBits) HalloweenKit.Show(b, false);
            if (puff != null) HalloweenKit.Show(puff, false);
        }

        // ---------- La soluzione ----------

        /// <summary>
        /// Il 4 giusto: il genio resta di sasso ("IMPOSSIBLE!"), i capelli gli si drizzano, gli occhiali volano via girando,
        /// si gonfia e fa "pop" in una nuvoletta; festa comune; Win una volta sola. Rifatta da capo se il livello si spegne a
        /// metà.
        /// </summary>
        IEnumerator Finale(int button)
        {
            if (finishing) yield break;
            finishing = true;
            V64AFingers.Reset();
            StopFx();
            HalloweenKit.Show(infoHolder, false);
            if (answerGlow != null) HalloweenKit.SetAlpha(answerGlow, 0f);
            Transform btn = Pick(buttonBodies, button);
            FxPop(btn, 1.2f, 0.3f);
            V82StupidGenius2Sound.Play(this, "ding", 0.55f);
            Haptics.Medium();

            // 1) di sasso: faccia stupita, trema
            SetFace(Face.Shock);
            say?.Show(ShockLine, 0.9f);
            for (float t = 0f; t < 0.45f; t += Time.deltaTime)
            {
                head.localRotation = Quaternion.Euler(0f, 0f, 6f * Mathf.Sin(t * 70f));
                yield return null;
            }
            head.localRotation = Quaternion.identity;

            // 2) capelli dritti, occhiali che volano via
            HalloweenKit.Show(hair, false);
            HalloweenKit.Show(hairUp, true);
            FxPop(hairUp, 1.25f, 0.25f);
            V82StupidGenius2Sound.Play(this, "boing", 0.5f);
            Vector3 gFrom = Vector3.zero, gTo = Vector3.zero;
            if (glasses != null)
            {
                glasses.SetParent(transform, true);
                gFrom = glasses.position;
                gTo = new Vector3(Mathf.Clamp(gFrom.x + 1.6f, MinX, MaxX), Mathf.Min(gFrom.y + 1.0f, MaxY), gFrom.z);
            }
            Vector3 gScale = glasses != null ? glasses.localScale : Vector3.one;
            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.5f);
                if (glasses != null)
                {
                    Vector3 p = Vector3.Lerp(gFrom, gTo, k) + new Vector3(0f, 1.2f * Mathf.Sin(k * Mathf.PI), 0f);
                    glasses.position = p;
                    glasses.localRotation = Quaternion.Euler(0f, 0f, -540f * k);
                    glasses.localScale = gScale * (1f - 0.6f * k);
                }
                // il genio si gonfia (testa compresa)
                genius.localScale = geniusScale * Mathf.Lerp(1f, 1.18f, Mathf.SmoothStep(0f, 1f, k));
                head.localScale = Vector3.one * Mathf.Lerp(1f, 1.15f, k);
                yield return null;
            }
            if (glasses != null) glasses.gameObject.SetActive(false);

            // 3) "pop": sparisce in una nuvoletta
            say?.Hide();
            V82StupidGenius2Sound.Play(this, "pop", 0.6f);
            Haptics.Success();
            Vector3 headWorld = head.position + new Vector3(0f, 0.5f, 0f);
            Vector3 big = genius.localScale;
            for (float t = 0f; t < 0.14f; t += Time.deltaTime)
            {
                genius.localScale = Vector3.Lerp(big, Vector3.zero, t / 0.14f);
                yield return null;
            }
            HalloweenKit.Show(genius, false);
            genius.localScale = geniusScale;
            if (puff != null)
            {
                puff.transform.position = new Vector3(headWorld.x, headWorld.y, puff.transform.position.z);
                puff.transform.localScale = puffScale * 1.8f;
                HalloweenKit.Show(puff, true);
                StartCoroutine(HalloweenKit.ScaleTo(puff.transform, puffScale * 2.6f, 0.35f));
            }
            if (chalkBits != null && chalkBits.Length > 0) StartCoroutine(ChalkBurst(headWorld));
            yield return new WaitForSeconds(0.2f);

            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            if (logic.TryComplete()) Win(Compliment);
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        const int KeyFinger = 901;

        int ButtonWithValue(int v)
        {
            for (int i = 0; buttonValues != null && i < buttonValues.Length; i++) if (buttonValues[i] == v) return i;
            return -1;
        }

        void EditorKeys()
        {
            if (animatingFix || logic.InputLocked || logic.Dragging) return;
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) KeyPress(3);
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) KeyPress(4);
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) KeyPress(5);
            if (Input.GetKeyDown(KeyCode.F)) KeyDrag(ButtonWithValue(4));
            if (Input.GetKeyDown(KeyCode.G)) KeyDrag(ButtonWithValue(3));
            if (Input.GetKeyDown(KeyCode.J)) KeyDrag(ButtonWithValue(5));
            if (Input.GetKeyDown(KeyCode.D)) KeyDrag(-1);
            if (Input.GetKeyDown(KeyCode.T)) { Vector2 p = symbols[Res].position; Begin(KeyFinger, p); End(KeyFinger, p); }
            if (Input.GetKeyDown(KeyCode.Q)) Begin(KeyFinger, symbols[1].position);
            if (Input.GetKeyDown(KeyCode.X) && geniusArea != null) Begin(KeyFinger, geniusArea.bounds.center);
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (!logic.BoardFixed && logic.Grab(KeyFinger, Res, 0f, 0f) == StupidGenius2Grab.Dragging)
                {
                    logic.Move(KeyFinger, 1f, -3f);
                    logic.Drop(KeyFinger, StupidGenius2Logic.TrueSum);
                }
                if (logic.FinishFix()) PoseBoard();
                int b = ButtonWithValue(StupidGenius2Logic.TrueSum);
                if (b >= 0) Press(b);
            }
            if (Input.GetKeyDown(KeyCode.P)) CancelAll();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }

        void KeyPress(int value)
        {
            int b = ButtonWithValue(value);
            if (b >= 0) Press(b);
        }

        /// <summary>Un dito finto prende il 5, lo porta sul pulsante indicato (o in un punto vuoto) e lo lascia.</summary>
        void KeyDrag(int button)
        {
            Vector2 from = symbols[Res].position;
            Begin(KeyFinger, from);
            if (logic.DragFinger != KeyFinger) return;
            Vector2 to = button >= 0 && buttons[button] != null ? (Vector2)buttons[button].position : new Vector2(0.6f, -0.6f);
            lastFinger = to;
            logic.Move(KeyFinger, to.x, to.y);
            symbols[Res].position = new Vector3(to.x, to.y, symbols[Res].position.z);
            End(KeyFinger, to);
        }
#endif
    }

    /// <summary>Suoni sintetizzati di The Stupid Genius 2: ronzio di risposta sbagliata, gesso, "boing", gesso che si frantuma, rintocco, "ding" e "pop".</summary>
    public static class V82StupidGenius2Sound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(442);

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
                case "buzz":    // ronzio da quiz: risposta sbagliata
                    c = Make(kind, 0.32f, t =>
                    {
                        float saw = 2f * ((t * 140f) % 1f) - 1f;
                        float saw2 = 2f * ((t * 147f) % 1f) - 1f;
                        return (saw + saw2) * 0.14f * Mathf.Min(1f, t * 80f) * Mathf.Clamp01((0.32f - t) * 20f);
                    });
                    break;
                case "chalk":   // gesso che stride
                    {
                        float lp = 0f;
                        c = Make(kind, 0.16f, t =>
                        {
                            lp += (N() - lp) * 0.5f;
                            return (lp * 0.3f + Mathf.Sin(t * 2f * Mathf.PI * 2400f) * 0.05f) * Mathf.Sin(t / 0.16f * Mathf.PI);
                        });
                        break;
                    }
                case "boing":   // molla: tono che oscilla
                    c = Make(kind, 0.3f, t => Mathf.Sin(t * 2f * Mathf.PI * (300f + 120f * Mathf.Sin(t * 60f))) * 0.3f * Mathf.Exp(-t * 9f));
                    break;
                case "smash":   // il 5 si frantuma: crepitio
                    c = Make(kind, 0.3f, t => N() * 0.45f * Mathf.Exp(-t * 14f) * (0.6f + 0.4f * Mathf.Sign(Mathf.Sin(t * 260f))));
                    break;
                case "chime":   // la lavagna corretta: arpeggio
                    c = Make(kind, 0.6f, t =>
                    {
                        float[] notes = { 523f, 659f, 784f, 1047f };
                        float v = 0f;
                        for (int k = 0; k < notes.Length; k++)
                        {
                            float s = t - k * 0.07f;
                            if (s < 0f) continue;
                            v += Mathf.Sin(s * 2f * Mathf.PI * notes[k]) * Mathf.Exp(-s * 7f) * 0.18f;
                        }
                        return v;
                    });
                    break;
                case "ding":    // risposta giusta
                    c = Make(kind, 0.5f, t => (Mathf.Sin(t * 2f * Mathf.PI * 1320f) + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 1980f)) * 0.2f * Mathf.Exp(-t * 6f));
                    break;
                case "pop":     // il genio scoppia
                    c = Make(kind, 0.18f, t => N() * 0.5f * Mathf.Exp(-t * 40f) + Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(500f, 180f, t / 0.18f)) * 0.35f * Mathf.Exp(-t * 18f));
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
            AudioClip clip = AudioClip.Create("v82b_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
