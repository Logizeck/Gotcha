using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 82 della roadmap "The Dishonest Hourglass" (id dishonest_hourglass) — "GET 10 MORE SECONDS." Una clessidra
    /// quasi vuota sulla scrivania, un display digitale appeso alla parete che conta alla rovescia da 00:08 e un grosso
    /// pulsante verde ADD TIME che mente: ogni pressione TOGLIE 1.5 s e fa scendere la sabbia più in fretta ("ADD TIME?
    /// Sure. Adding... negative time."). Aspettare non serve: a 00:00 un "ding" triste, il display resta a zero ~1.5 s e
    /// poi la clessidra si riempie da sola e riparte (nessun Fail, nessuna vita persa). Toccare il vetro o il display non
    /// cambia niente (battute diverse). Tutto reversibile.
    /// Soluzione: capovolgere davvero il telefono (DishonestHourglassLogic: V64AGravity con le soglie/isteresi/armamento
    /// del gatto aristocratico e di Luck Hack, tenuto 0.3 s): la clessidra fa mezzo giro (SOLO lo sprite, ~0.5 s), la
    /// sabbia scende dall'altra parte e il display conta su di 10 secondi con un tic per secondo; il pulsante bugiardo
    /// suda e ammette "Okay. THAT is real time."; festa comune "YOU BEAT TIME ITSELF!" -> Win una volta sola.
    /// Display, scritte e pulsante NON ruotano mai. La sabbia è ritagliata con ClipArt (SpriteWindowClip, niente
    /// SpriteMask): due finestre figlie del perno della clessidra che il livello allunga.
    /// Dito: HalloweenKit.ReadPointer (un dito, solo tocchi). Pausa, app in background o livello spento: la tenuta si
    /// annulla, il telefono va rivisto dritto, il tempo resta valido.
    /// Tasti di prova nell'Editor: U capovolge/raddrizza il telefono (simulato), J inclinazione a metà strada (non vale),
    /// A o Spazio premono ADD TIME, G tocca la clessidra, D tocca il display, H soluzione (capovolto), P pausa simulata,
    /// R ricomincia.
    /// </summary>
    public class V85DishonestHourglassLevel : LevelController
    {
        [Header("La clessidra (radice al collo; il perno gira, dentro vetro e sabbia)")]
        [SerializeField] Transform hourglass;
        [SerializeField] Transform hourSpin;
        [SerializeField] Collider2D glassArea;
        [SerializeField] Transform topWindow;        // finestra della sabbia di sopra (sale dal collo; scala y = altezza)
        [SerializeField] Transform bottomWindow;     // finestra della sabbia di sotto (sale dal fondo)
        [SerializeField] Transform stream;           // filo di sabbia (scala y = lunghezza, dal collo in giù)
        [SerializeField] SpriteRenderer streamArt;
        [SerializeField] float bulbH = 1.58f;        // altezza interna di un bulbo (coordinate locali della clessidra)
        [Header("Il display")]
        [SerializeField] Transform display;
        [SerializeField] TextMesh displayText;
        [SerializeField] TextMesh speedText;
        [SerializeField] Transform speedHolder;      // etichetta "SPEED x1.5" sulla parete (accesa solo quando accelera)
        [SerializeField] Collider2D displayArea;
        [SerializeField] SpriteRenderer displayGlow;
        [SerializeField] Transform[] sparkles;
        [Header("Il pulsante ADD TIME")]
        [SerializeField] Transform addButton;
        [SerializeField] Transform addBody;
        [SerializeField] Collider2D addArea;
        [SerializeField] Transform sweat;
        [SerializeField] SpeechBubble buttonBubble;
        [Header("Scritte e festa")]
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "YOU BEAT TIME ITSELF!";
        const string ConcedeLine = "Okay. THAT is real time.";
        static readonly string[] AddLines =
        {
            "ADD TIME? Sure. Adding... negative time.",
            "There! Minus 1.5 seconds. You're welcome.",
            "More time! (Less time.)",
            "Adding time faster than ever!",
            "I'm a very honest button.",
        };
        static readonly string[] EmptyPressLines = { "Nothing left to add. Or take.", "Zero minus zero is... zero." };
        static readonly string[] RanOutLines = { "Ding. Time's up.", "Ding! Out of sand.", "Ding. Not a game over. Just sad." };
        static readonly string[] RefillLines = { "Refilled. 8 seconds. Again.", "Fine. Have your 8 seconds back.", "Fresh sand. Same 8 seconds." };
        static readonly string[] GlassLines = { "Tapping won't make more sand.", "The sand ignores you.", "Gentle! It's antique." };
        static readonly string[] DisplayLines = { "I just show the numbers.", "Don't blame the display.", "Still counting. Down." };

        static readonly Color LedRed = new Color(1f, 0.3f, 0.22f), LedDim = new Color(0.45f, 0.13f, 0.1f), LedGreen = new Color(0.4f, 1f, 0.45f);
        static readonly Color StreamColor = new Color(0.95f, 0.77f, 0.42f), StreamFast = new Color(1f, 0.6f, 0.3f);
        const float TopExp = 0.75f, BotExp = 1.1f;   // sabbia -> altezza (il collo è stretto, il fondo è largo)

        DishonestHourglassLogic logic;
        Camera cam;
        HwToast toast;
        HwSay say;
        Vector3 addScale, displayScale, spinScale, glassHome;
        Quaternion addRot;
        float shownTop, addPopT = 1f, displayPopT = 1f, wiggleT = 1f, streamT;
        int addLines, emptyLines, ranOutLines, refillLines, glassLines, displayLines;
        bool captured, finishing;
#if UNITY_EDITOR
        bool editorFlip, editorHalf;
#endif

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            if (!captured)
            {
                captured = true;
                logic = new DishonestHourglassLogic();
                addScale = addBody != null ? addBody.localScale : Vector3.one;
                addRot = addButton != null ? addButton.localRotation : Quaternion.identity;
                displayScale = display != null ? display.localScale : Vector3.one;
                spinScale = hourSpin != null ? hourSpin.localScale : Vector3.one;
                glassHome = hourglass != null ? hourglass.localPosition : Vector3.zero;
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            finishing = false;
            addLines = emptyLines = ranOutLines = refillLines = glassLines = displayLines = 0;
            addPopT = displayPopT = wiggleT = 1f;
            streamT = 0f;
            toast = new HwToast(infoText, infoHolder);
            say = new HwSay(buttonBubble);
            say.Hide();
#if UNITY_EDITOR
            editorFlip = editorHalf = false;
#endif
            shownTop = logic.TopSand01;
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        /// <summary>Clessidra dritta e ferma, pulsante e display a riposo, niente effetti.</summary>
        void RestPose()
        {
            if (hourSpin != null) { hourSpin.localRotation = Quaternion.identity; hourSpin.localScale = spinScale; }
            if (hourglass != null) hourglass.localPosition = glassHome;
            if (streamArt != null) streamArt.color = StreamColor;
            if (addBody != null) addBody.localScale = addScale;
            if (addButton != null) addButton.localRotation = addRot;
            if (display != null) display.localScale = displayScale;
            HalloweenKit.Show(sweat, false);
            if (displayGlow != null) HalloweenKit.SetAlpha(displayGlow, 0f);
            if (sparkles != null) foreach (Transform s in sparkles) HalloweenKit.Show(s, false);
            SetSand(shownTop, 1f - shownTop);
            ShowTime(logic.DisplaySeconds, LedRed);
            UpdateSpeed();
            ShowStream(logic.State == DishonestHourglassState.Running, 1f);
        }

        /// <summary>Sabbia di sopra e di sotto (0..1, nel sistema del perno: "sopra" = verso il tappo in alto).</summary>
        void SetSand(float top01, float bottom01)
        {
            float ht = bulbH * Mathf.Pow(Mathf.Clamp01(top01), TopExp);
            float hb = bulbH * 0.97f * Mathf.Pow(Mathf.Clamp01(bottom01), BotExp);
            if (topWindow != null) topWindow.localScale = new Vector3(1f, Mathf.Max(0f, ht), 1f);
            if (bottomWindow != null) bottomWindow.localScale = new Vector3(1f, Mathf.Max(0f, hb), 1f);
        }

        /// <summary>Il filo di sabbia dal collo alla sabbia di sotto (width: 1 normale, più grosso quando accelera).</summary>
        void ShowStream(bool on, float width)
        {
            if (stream == null) return;
            HalloweenKit.Show(stream, on);
            if (!on) return;
            float hb = bulbH * 0.97f * Mathf.Pow(Mathf.Clamp01(1f - shownTop), BotExp);
            float len = Mathf.Max(0.05f, bulbH - hb);
            stream.localScale = new Vector3(width, len, 1f);
        }

        void ShowTime(int seconds, Color c)
        {
            if (displayText == null) return;
            displayText.text = DishonestHourglassLogic.Format(seconds);
            displayText.color = c;
        }

        void UpdateSpeed()
        {
            if (speedText == null || logic == null) return;
            bool fast = logic.Speed > 1.01f && logic.State == DishonestHourglassState.Running;
            HalloweenKit.Show(speedHolder != null ? speedHolder : speedText.transform, fast);
            if (fast) speedText.text = "SPEED x" + logic.Speed.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            CancelAll();
            if (captured && !finishing && logic != null && !IsFinished && logic.State != DishonestHourglassState.Won)
            {
                shownTop = logic.TopSand01;
                RestPose();
                HalloweenKit.Show(infoHolder, false);
                say?.Hide();
            }
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola, la logica fa da guardia)
            if (logic.State == DishonestHourglassState.Won) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelAll(); }

        /// <summary>La tenuta capovolta si annulla, il filtro della gravità riparte (il tempo resta quello che era).</summary>
        void CancelAll()
        {
#if UNITY_EDITOR
            editorFlip = editorHalf = false;
#endif
            if (logic == null || logic.InputLocked) return;
            logic.Cancel();
        }

        void Update()
        {
            toast?.Tick();
            say?.Tick();
            if (logic == null || IsFinished || finishing || toast == null) return;
            if (Time.timeScale <= 0f) { CancelAll(); return; }   // pausa o pubblicità: niente input né sensori
#if UNITY_EDITOR
            EditorKeys();
            if (finishing) return;
#endif
            float dt = Time.deltaTime;
            Vector2 g = Gravity();
            React(logic.Tick(dt, g.x, g.y));
            if (finishing) return;
            ReadPointer();
            if (finishing) return;
            Pose(dt);
        }

        /// <summary>Gravità nello schermo (in g): accelerometro; nell'Editor anche i tasti U/J.</summary>
        Vector2 Gravity()
        {
            Vector2 g = Inputs.Tilt();
#if UNITY_EDITOR
            if (editorFlip) g = new Vector2(0f, 1f);
            else if (editorHalf) g = new Vector2(0f, 0.45f);
#endif
            return g;
        }

        // ---------- Dito (solo tocchi) ----------

        void ReadPointer()
        {
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool _)) return;
            if (began) Tap(w);
        }

        static bool Over(Collider2D c, Vector2 p) => c != null && c.gameObject.activeInHierarchy && c.OverlapPoint(p);

        void Tap(Vector2 w)
        {
            if (logic.InputLocked || finishing) return;
            if (Over(addArea, w)) { PressAdd(); return; }
            if (Over(displayArea, w)) { React(logic.TapDisplay()); return; }
            if (Over(glassArea, w)) React(logic.TapGlass());
        }

        void PressAdd()
        {
            addPopT = 0f;   // il pulsante si abbassa sempre
            Sfx.Play(this, "tick", 0.35f);
            React(logic.PressAdd());
        }

        // ---------- Reazioni ----------

        void React(DishonestHourglassEvent e)
        {
            switch (e)
            {
                case DishonestHourglassEvent.Subtracted:
                    say.Show(AddLines[addLines++ % AddLines.Length], 2.4f);
                    V85DishonestHourglassSound.Play(this, "zap", 0.45f);
                    displayPopT = 0f;
                    Haptics.Tap();
                    UpdateSpeed();
                    break;
                case DishonestHourglassEvent.NothingToTake:
                    say.Show(EmptyPressLines[emptyLines++ % EmptyPressLines.Length], 2.0f);
                    V85DishonestHourglassSound.Play(this, "buzz", 0.3f);
                    break;
                case DishonestHourglassEvent.RanOut:
                    toast.Show(RanOutLines[ranOutLines++ % RanOutLines.Length], 1.6f);
                    V85DishonestHourglassSound.Play(this, "ding", 0.6f);
                    displayPopT = 0f;
                    Haptics.Light();
                    UpdateSpeed();
                    break;
                case DishonestHourglassEvent.Refilled:
                    toast.Show(RefillLines[refillLines++ % RefillLines.Length], 1.8f);
                    V85DishonestHourglassSound.Play(this, "pour", 0.4f);
                    displayPopT = 0f;
                    UpdateSpeed();
                    break;
                case DishonestHourglassEvent.GlassTapped:
                    toast.Show(GlassLines[glassLines++ % GlassLines.Length], 1.6f);
                    V85DishonestHourglassSound.Play(this, "tink", 0.45f);
                    wiggleT = 0f;
                    break;
                case DishonestHourglassEvent.DisplayTapped:
                    toast.Show(DisplayLines[displayLines++ % DisplayLines.Length], 1.6f);
                    Sfx.Play(this, "tick", 0.3f);
                    displayPopT = 0f;
                    break;
                case DishonestHourglassEvent.Tipping:
                    V85DishonestHourglassSound.Play(this, "pour", 0.25f);
                    break;
                case DishonestHourglassEvent.Turned:
                    StartCoroutine(Finale());
                    break;
            }
        }

        // ---------- La sabbia che scende, il display, il pulsante ----------

        void Pose(float dt)
        {
            // il pulsante si abbassa e torna su
            if (addBody != null)
            {
                if (addPopT < 1f) addPopT = Mathf.Min(1f, addPopT + dt / 0.18f);
                float press = addPopT < 1f ? Mathf.Sin(addPopT * Mathf.PI) : 0f;
                addBody.localScale = new Vector3(addScale.x * (1f + 0.06f * press), addScale.y * (1f - 0.16f * press), addScale.z);
            }
            // il display sobbalza (ADD TIME, zero, ricarica, tocco)
            if (display != null)
            {
                if (displayPopT < 1f) displayPopT = Mathf.Min(1f, displayPopT + dt / 0.25f);
                display.localScale = displayScale * (1f + 0.1f * (displayPopT < 1f ? Mathf.Sin(displayPopT * Mathf.PI) : 0f));
            }
            // la sabbia segue il tempo (la ricarica risale in ~0.3 s)
            float target = logic.TopSand01;
            shownTop += (target - shownTop) * Mathf.Clamp01(dt * 14f);
            SetSand(shownTop, 1f - shownTop);
            bool empty = logic.State == DishonestHourglassState.Empty;
            if (empty)
            {
                // 00:00 lampeggia, la sabbia è tutta giù
                bool on = Mathf.Repeat(logic.EmptySeconds, 0.5f) < 0.3f;
                ShowTime(0, on ? LedRed : LedDim);
            }
            else ShowTime(logic.DisplaySeconds, logic.Speed > 1.01f ? Color.Lerp(LedRed, Color.white, 0.25f * Mathf.Abs(Mathf.Sin(Time.time * 10f))) : LedRed);
            // filo di sabbia: più grosso e tremolante se accelera; fermo mentre si tiene il telefono capovolto
            streamT += dt * logic.Speed;
            float w = (logic.Speed > 1.01f ? 1.6f : 1f) * (0.85f + 0.15f * Mathf.Sin(streamT * 40f));
            ShowStream(!empty && !logic.Tipping, w);
            if (streamArt != null) streamArt.color = logic.Speed > 1.01f ? StreamFast : StreamColor;
            // la clessidra: traballa se toccata, trema mentre è tenuta capovolta
            if (hourSpin != null)
            {
                float rot = 0f;
                if (wiggleT < 1f)
                {
                    wiggleT = Mathf.Min(1f, wiggleT + dt / 0.45f);
                    rot = 7f * Mathf.Sin(wiggleT * Mathf.PI * 4f) * (1f - wiggleT);
                }
                if (logic.Tipping) rot += 3f * Mathf.Sin(Time.time * 50f) * logic.HoldProgress;
                hourSpin.localRotation = Quaternion.Euler(0f, 0f, rot);
            }
        }

        // ---------- Vittoria ----------

        /// <summary>
        /// La clessidra fa mezzo giro (solo lo sprite, 0.5 s), la sabbia scende dall'altra parte, il display conta su di 10
        /// secondi (verde, un tic per secondo) mentre il pulsante bugiardo suda e ammette; festa comune; Win una volta sola.
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            HalloweenKit.Show(infoHolder, false);
            say?.Hide();
            HalloweenKit.Show(speedHolder, false);
            ShowStream(false, 1f);
            float top = shownTop, bottom = 1f - shownTop;
            SetSand(top, bottom);
            // 1) mezzo giro
            V85DishonestHourglassSound.Play(this, "whoosh", 0.45f);
            Haptics.Medium();
            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.5f);
                float ease = k * k * (3f - 2f * k);
                if (hourSpin != null)
                {
                    hourSpin.localRotation = Quaternion.Euler(0f, 0f, 180f * ease);
                    hourSpin.localScale = spinScale * (1f + 0.08f * Mathf.Sin(k * Mathf.PI));
                }
                if (hourglass != null) hourglass.localPosition = glassHome + new Vector3(0f, 0.35f * Mathf.Sin(k * Mathf.PI), 0f);   // si solleva per girarsi
                yield return null;
            }
            // la clessidra è simmetrica: torna a 0° con i bulbi scambiati (la sabbia di sotto ora è sopra)
            if (hourSpin != null) { hourSpin.localRotation = Quaternion.identity; hourSpin.localScale = spinScale; }
            if (hourglass != null) hourglass.localPosition = glassHome;
            shownTop = bottom;
            SetSand(bottom, top);
            V85DishonestHourglassSound.Play(this, "pour", 0.45f);
            // 2) +10: il display conta su, il pulsante suda e ammette
            HalloweenKit.Show(sweat, true);
            say?.Show(ConcedeLine, 2.0f);
            if (sparkles != null) foreach (Transform s in sparkles) HalloweenKit.Show(s, true);
            int from = logic.GainedFrom, to = logic.GainedTo;
            ShowTime(from, LedGreen);
            float countTime = 1.0f;
            int shown = from;
            for (float t = 0f; t < countTime; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / countTime);
                int now = from + Mathf.Min(to - from, Mathf.FloorToInt(k * (to - from + 1)));
                if (now != shown)
                {
                    shown = now;
                    ShowTime(shown, LedGreen);
                    V85DishonestHourglassSound.Play(this, "tock", 0.35f + 0.02f * (shown - from));
                    displayPopT = 0f;
                }
                if (displayPopT < 1f) displayPopT = Mathf.Min(1f, displayPopT + Time.deltaTime / 0.09f);
                if (display != null) display.localScale = displayScale * (1f + 0.08f * Mathf.Sin(displayPopT * Mathf.PI) + 0.06f * k);
                if (displayGlow != null) { HalloweenKit.SetAlpha(displayGlow, 0.55f * Mathf.Clamp01(t * 4f)); displayGlow.transform.localScale = Vector3.one * (3.4f + 0.3f * Mathf.Sin(t * 9f)); }
                streamT += Time.deltaTime;
                ShowStream(true, 0.85f + 0.15f * Mathf.Sin(streamT * 40f));
                if (addButton != null) addButton.localRotation = addRot * Quaternion.Euler(0f, 0f, -9f * Mathf.Clamp01(t * 3f));
                if (sweat != null) sweat.localPosition = new Vector3(sweat.localPosition.x, 0.55f - 0.2f * k, sweat.localPosition.z);
                if (sparkles != null)
                    for (int i = 0; i < sparkles.Length; i++)
                        if (sparkles[i] != null)
                        {
                            sparkles[i].localScale = Vector3.one * (0.25f + 0.15f * Mathf.Abs(Mathf.Sin(t * 7f + i)));
                            sparkles[i].localRotation = Quaternion.Euler(0f, 0f, t * 200f + i * 40f);
                        }
                yield return null;
            }
            ShowTime(to, LedGreen);
            V85DishonestHourglassSound.Play(this, "ting", 0.55f);
            Haptics.Success();
            if (display != null) display.localScale = displayScale * 1.06f;
            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            if (!IsFinished && logic.TryComplete()) Win(Compliment);
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked) return;
            if (Input.GetKeyDown(KeyCode.U)) { editorFlip = !editorFlip; editorHalf = false; }
            if (Input.GetKeyDown(KeyCode.J)) { editorHalf = !editorHalf; editorFlip = false; }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Space)) PressAdd();
            if (Input.GetKeyDown(KeyCode.G)) React(logic.TapGlass());
            if (Input.GetKeyDown(KeyCode.D)) React(logic.TapDisplay());
            if (Input.GetKeyDown(KeyCode.H)) { editorHalf = false; editorFlip = true; }   // soluzione: capovolto (vince se prima era dritto)
            if (Input.GetKeyDown(KeyCode.P)) CancelAll();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }
#endif
    }

    /// <summary>Suoni sintetizzati della clessidra bugiarda: "ding" triste, zap che toglie, sabbia che scorre, tic del conteggio, tintinnio del vetro.</summary>
    public static class V85DishonestHourglassSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(851);

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
                case "ding":    // campanello triste: due note che scendono
                    c = Make(kind, 1.0f, t =>
                    {
                        float f = t < 0.35f ? 880f : 660f;
                        float tt = t < 0.35f ? t : t - 0.35f;
                        return (Mathf.Sin(t * 2f * Mathf.PI * f) + 0.3f * Mathf.Sin(t * 2f * Mathf.PI * f * 2.76f)) * 0.22f * Mathf.Exp(-tt * 5f);
                    });
                    break;
                case "zap":     // "bwoop" che scende: il tempo che se ne va
                    c = Make(kind, 0.35f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(900f, 180f, t / 0.35f)) * 0.25f * Mathf.Clamp01((0.35f - t) * 12f));
                    break;
                case "pour":    // sabbia che scorre: fruscio
                    c = Make(kind, 0.6f, t => N() * 0.12f * Mathf.Sin(t / 0.6f * Mathf.PI) * (0.7f + 0.3f * Mathf.Sin(t * 90f)));
                    break;
                case "tock":    // tic del conteggio
                    c = Make(kind, 0.08f, t => Mathf.Sin(t * 2f * Mathf.PI * 1500f) * 0.3f * Mathf.Exp(-t * 60f));
                    break;
                case "tink":    // dito sul vetro
                    c = Make(kind, 0.3f, t => (Mathf.Sin(t * 2f * Mathf.PI * 3100f) + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * 4700f)) * 0.18f * Mathf.Exp(-t * 18f));
                    break;
                case "whoosh":  // mezzo giro
                    c = Make(kind, 0.45f, t => N() * 0.22f * Mathf.Sin(t / 0.45f * Mathf.PI));
                    break;
                case "ting":    // luccichio finale
                    c = Make(kind, 0.5f, t => (Mathf.Sin(t * 2f * Mathf.PI * 1760f) + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 2637f)) * 0.18f * Mathf.Exp(-t * 7f));
                    break;
                case "buzz":    // "no"
                    c = Make(kind, 0.25f, t => (2f * ((t * 130f) % 1f) - 1f) * 0.18f * Mathf.Min(1f, t * 80f) * Mathf.Clamp01((0.25f - t) * 20f));
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
            AudioClip clip = AudioClip.Create("v85a_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
