using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 8 della roadmap "Love the Bug" (id love_the_bug) — "GET RID OF THE BUG." Un insettino verde cammina sulla
    /// finestra di una app di fantasia che funziona male ("MY APP v0.1": blocchi colorati che sfarfallano, barra di
    /// caricamento storta, faccina-icona rotta). Tappare l'insetto lo SDOPPIA con un "pop": più insetti, più difetti, la
    /// barra torna indietro (al massimo 8 insetti; al tetto un tocco li fa solo sparpagliare ridacchiando).
    /// Soluzione: tenere il dito su un insetto (uno qualsiasi) per 2 secondi: si ferma, il cuore sopra di lui si riempie,
    /// arrossisce sempre di più; a 2 s diventa un cuore, gli altri scoppiano in cuoricini, la app si ripara (difetti via,
    /// barra piena, faccina sorridente), LevelReward "BUG FIXED WITH LOVE!", Win. Una pressione più lunga di 0,25 s non è
    /// mai un tocco: alzare il dito prima dei 2 s o farlo scivolare via annulla e basta. Il secondo dito è ignorato.
    /// Mai sconfitta. Dita: V64AFingers (al massimo due id; il mouse è un dito). La logica è in
    /// Gotcha.Levels.V64.LoveTheBugLogic.
    /// Tasti di prova nell'Editor: T tocco veloce su un insetto (a turno), H appoggia/alza il dito che tiene il primo
    /// insetto, M fa scivolare via il dito che tiene (e lo alza), J tocco veloce di un secondo dito sull'ultimo insetto
    /// (ignorato mentre H tiene; senza H è un tocco normale), P pausa simulata
    /// (dita annullate), R ricomincia il livello.
    /// </summary>
    public class V77LoveTheBugLevel : LevelController
    {
        [Header("Insetti (8, già pronti: si accendono man mano)")]
        [SerializeField] Transform[] bugs;           // radice che cammina
        [SerializeField] Transform[] bugFlips;       // figlio che si gira (scala x ±1) e saltella
        [SerializeField] SpriteRenderer[] bugArts;
        [SerializeField] SpriteRenderer[] bugCheeks; // guance rosse (alfa = rossore)
        [SerializeField] SpriteRenderer[] pops;      // sbuffi dello sdoppiamento (a turno)
        [Header("Cuore-misuratore")]
        [SerializeField] Transform meter;
        [SerializeField] Transform meterFill;
        [Header("App che si rompe")]
        [SerializeField] SpriteRenderer[] glitches;  // blocchi colorati (si accendono con gli insetti)
        [SerializeField] Transform barFill;          // riempimento della barra (ancorato a sinistra via codice)
        [SerializeField] SpriteRenderer barFillArt;
        [SerializeField] Transform smileyBroken;
        [SerializeField] Transform smileyHappy;
        [SerializeField] TextMesh titleText;
        [SerializeField] TextMesh statusText;
        [Header("Finale")]
        [SerializeField] Transform bigHeart;
        [SerializeField] Transform[] smallHearts;
        [Header("Limiti del centro degli insetti (insetto largo 1.1: bordi entro x ±2.25, dentro la finestra)")]
        [SerializeField] float minX = -1.6f;
        [SerializeField] float maxX = 1.6f;
        [SerializeField] float minY = -2.55f;
        [SerializeField] float maxY = 1.45f;
        [SerializeField] Vector2 startAt = new Vector2(-0.95f, -1.35f);
        [SerializeField] float barWidth = 3.5f;
        [Header("Scritte e festa")]
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "BUG FIXED WITH LOVE!";
        const string HugLine = "Maybe it just needs a hug.";
        static readonly string[] SplitLines = { "Oops. Now there are two.", "Stop tapping! They LIKE it.", "Bug count: rising." };
        static readonly string[] ScatterLines = { "Squashing makes MORE bugs.", "They're multiplying!", "Hee hee hee!" };
        static readonly string[] LetGoLines = { "Almost... it was starting to like you.", "Aww. It was blushing.", "Almost... don't stop now." };
        const string TitleBroken = "MY APP v0.1", TitleFixed = "MY APP v1.0";
        static readonly Color BarGood = new Color(0.36f, 0.82f, 0.45f), BarBad = new Color(1f, 0.45f, 0.3f);
        static readonly Color BlushTint = new Color(1f, 0.72f, 0.8f);
        const float MeterScale = 1.5f, MeterLift = 0.8f;

        LoveTheBugLogic logic;
        Camera cam;
        HwToast toast;
        Vector3[] glitchHome;
        Vector3 bigHeartScale = Vector3.one;
        float shownBar, repair;
        int popNext, splitLines, scatterLines, letGoLines;
        bool captured, finishing;
#if UNITY_EDITOR
        const int EditorHoldId = 800, EditorTapId = 801, EditorSecondId = 802;
        bool editorHolding;
        int editorTapBug;
#endif

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            if (!captured)
            {
                captured = true;
                logic = new LoveTheBugLogic(minX, maxX, minY, maxY, startAt.x, startAt.y);
                glitchHome = new Vector3[glitches != null ? glitches.Length : 0];
                for (int i = 0; i < glitchHome.Length; i++) glitchHome[i] = glitches[i] != null ? glitches[i].transform.localPosition : Vector3.zero;
                if (bigHeart != null) bigHeartScale = bigHeart.localScale;
            }
            logic.Reset();
            V64AFingers.Reset();
            finishing = false;
            splitLines = scatterLines = letGoLines = popNext = 0;
            repair = 0f;
            shownBar = logic.Progress;
            toast = new HwToast(infoText, infoHolder);
            if (reward != null) reward.ResetReward();
            if (titleText != null) titleText.text = TitleBroken;
            HalloweenKit.Show(bigHeart, false);
            if (smallHearts != null) foreach (Transform h in smallHearts) HalloweenKit.Show(h, false);
            if (pops != null) foreach (SpriteRenderer p in pops) HalloweenKit.Show(p, false);
            for (int i = 0; i < bugs.Length; i++) if (bugs[i] != null) bugs[i].localScale = Vector3.one;
#if UNITY_EDITOR
            editorHolding = false;
            editorTapBug = 0;
#endif
            Pose(0f);
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: dita annullate, nessuno sdoppiamento, stato valido
            CancelFingers();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: si rifà il finale da capo (Win una volta sola, la logica fa da guardia)
            if (logic.State == LoveTheBugState.Won) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelFingers(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelFingers(); }

        void CancelFingers()
        {
            V64AFingers.Reset();
#if UNITY_EDITOR
            editorHolding = false;
#endif
            if (logic == null || logic.InputLocked) return;
            logic.Cancel();
        }

        void Update()
        {
            toast?.Tick();
            if (logic == null || IsFinished || finishing || toast == null) return;
            if (Time.timeScale <= 0f) { CancelFingers(); return; }   // pausa o pubblicità: niente input
#if UNITY_EDITOR
            EditorKeys();
            if (finishing) return;
#endif
            ReadFingers();
            if (finishing) return;
            React(logic.Tick(Time.deltaTime));
            if (finishing) return;
            Pose(Time.deltaTime);
        }

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            // prima le dita che scendono, poi quelle che si muovono, poi quelle che salgono
            for (int i = 0; i < fingers.Count && !finishing; i++)
                if (fingers[i].began) React(logic.Down(fingers[i].id, fingers[i].world.x, fingers[i].world.y));
            for (int i = 0; i < fingers.Count && !finishing; i++)
                if (!fingers[i].began && !fingers[i].ended) React(logic.Move(fingers[i].id, fingers[i].world.x, fingers[i].world.y));
            for (int i = 0; i < fingers.Count && !finishing; i++)
                if (fingers[i].ended) React(logic.Up(fingers[i].id));
        }

        void React(LoveTheBugEvent e)
        {
            switch (e)
            {
                case LoveTheBugEvent.Pressed:
                    Sfx.Play(this, "tick", 0.4f);
                    Haptics.Tap();
                    break;
                case LoveTheBugEvent.Split:
                    {
                        int n = logic.Count - 1;
                        Pop(new Vector3(logic.BugX(n), logic.BugY(n), 0f));
                        if (bugs[n] != null) bugs[n].localScale = Vector3.one * 0.3f;   // nasce piccolo e cresce (Pose)
                        bool hint = logic.TakeHint();
                        toast.Show(hint ? HugLine : SplitLines[splitLines++ % SplitLines.Length], hint ? 2.6f : 1.3f);
                        Sfx.Play(this, "pop", 0.7f);
                        Haptics.Light();
                        break;
                    }
                case LoveTheBugEvent.Scatter:
                    {
                        bool hint = logic.TakeHint();
                        toast.Show(hint ? HugLine : ScatterLines[scatterLines++ % ScatterLines.Length], hint ? 2.6f : 1.3f);
                        Sfx.Play(this, "whoosh", 0.5f);
                        Haptics.Tap();
                        break;
                    }
                case LoveTheBugEvent.LetGo:
                    toast.Show(LetGoLines[letGoLines++ % LetGoLines.Length], 1.6f);
                    Sfx.Play(this, "whoosh", 0.3f);
                    break;
                case LoveTheBugEvent.Loved:
                    StartCoroutine(Finale());
                    break;
            }
        }

        void Pop(Vector3 at)
        {
            if (pops == null || pops.Length == 0) return;
            SpriteRenderer p = pops[popNext++ % pops.Length];
            if (p != null) StartCoroutine(PopPuff(p, at));
        }

        IEnumerator PopPuff(SpriteRenderer p, Vector3 at)
        {
            p.gameObject.SetActive(true);
            p.transform.localPosition = at;
            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.35f);
                p.transform.localScale = Vector3.one * Mathf.Lerp(0.6f, 1.5f, k);
                HalloweenKit.SetAlpha(p, 0.9f * (1f - k));
                yield return null;
            }
            p.gameObject.SetActive(false);
        }

        // ---------- Pose (sempre ricavata dalla logica) ----------

        void Pose(float dt)
        {
            float t = Time.time;
            bool scatter = logic.Scattering;
            for (int i = 0; i < bugs.Length; i++)
            {
                if (bugs[i] == null) continue;
                bool on = i < logic.Count;
                bugs[i].gameObject.SetActive(on);
                if (!on) continue;
                bugs[i].localPosition = new Vector3(logic.BugX(i), logic.BugY(i), bugs[i].localPosition.z);
                // nasce piccolo e cresce
                bugs[i].localScale = Vector3.MoveTowards(bugs[i].localScale, Vector3.one, dt * 3f);
                float vx = logic.BugVX(i), vy = logic.BugVY(i);
                bool walking = logic.Walking(i);
                float face = vx >= 0f ? 1f : -1f;
                float tilt = walking ? Mathf.Clamp(Mathf.Atan2(vy, Mathf.Abs(vx)) * Mathf.Rad2Deg * 0.4f, -25f, 25f) : 0f;
                bugs[i].localRotation = Quaternion.Euler(0f, 0f, face * tilt);
                if (bugFlips != null && i < bugFlips.Length && bugFlips[i] != null)
                {
                    float speed = scatter ? 26f : 13f;
                    float bob = walking ? Mathf.Abs(Mathf.Sin(t * speed + i * 1.7f)) * 0.05f : 0f;
                    // tenuto: trema un pochino di timidezza
                    float shy = walking ? 0f : Mathf.Sin(t * 35f) * 0.01f * (0.5f + logic.Blush(i));
                    bugFlips[i].localPosition = new Vector3(shy, bob, 0f);
                    // sparpagliati al tetto: dondolano ridacchiando
                    bugFlips[i].localRotation = Quaternion.Euler(0f, 0f, scatter && walking ? Mathf.Sin(t * 30f + i) * 12f : 0f);
                    Vector3 s = bugFlips[i].localScale;
                    bugFlips[i].localScale = new Vector3(face * Mathf.Abs(s.x == 0f ? 1f : s.x), s.y, s.z);
                }
                float blush = logic.Blush(i);
                if (bugArts != null && i < bugArts.Length && bugArts[i] != null) bugArts[i].color = Color.Lerp(Color.white, BlushTint, blush * 0.7f);
                if (bugCheeks != null && i < bugCheeks.Length && bugCheeks[i] != null)
                {
                    bugCheeks[i].gameObject.SetActive(blush > 0.01f);
                    HalloweenKit.SetAlpha(bugCheeks[i], Mathf.Clamp01(blush * 1.4f));
                }
            }

            // cuore-misuratore sopra l'insetto tenuto (o quello appena lasciato, mentre si svuota)
            int m = logic.MeterBug;
            bool showMeter = logic.State == LoveTheBugState.Pressing || (m >= 0 && m < logic.Count && logic.MeterFill > 0.001f);
            if (meter != null)
            {
                meter.gameObject.SetActive(showMeter);
                if (showMeter)
                {
                    int b = logic.State == LoveTheBugState.Pressing ? logic.PressedBug : m;
                    float fill = logic.State == LoveTheBugState.Pressing ? logic.HoldProgress : logic.MeterFill;
                    meter.localPosition = new Vector3(logic.BugX(b), logic.BugY(b) + MeterLift, meter.localPosition.z);
                    float beat = 1f + 0.06f * fill * Mathf.Abs(Mathf.Sin(t * (4f + 6f * fill)));
                    meter.localScale = Vector3.one * MeterScale * beat;
                    if (meterFill != null) meterFill.localScale = Vector3.one * Mathf.Max(0.001f, fill);
                }
            }

            PoseApp(dt);
        }

        /// <summary>La finestra della app: difetti, barra storta che va avanti/indietro, faccina, scritte.</summary>
        void PoseApp(float dt)
        {
            float t = Time.time;
            float glitch = logic.Glitch * (1f - repair);
            float target = Mathf.Lerp(logic.Progress, 1f, repair);
            shownBar = dt <= 0f ? target : Mathf.MoveTowards(shownBar, target, dt * (repair > 0f ? 2.5f : 0.5f));
            int lit = repair >= 1f ? 0 : 2 + Mathf.RoundToInt(logic.Glitch * ((glitches != null ? glitches.Length : 0) - 2));
            if (glitches != null)
                for (int i = 0; i < glitches.Length; i++)
                {
                    SpriteRenderer g = glitches[i];
                    if (g == null) continue;
                    bool on = i < lit;
                    g.gameObject.SetActive(on);
                    if (!on) continue;
                    // sfarfallio: scatti orizzontali a gradini, più forti con più insetti
                    float step = Mathf.Floor(t * (6f + 10f * glitch) + i * 3.1f);
                    float jx = (Mathf.PerlinNoise(step * 0.37f, i * 1.3f) - 0.5f) * (0.08f + 0.35f * glitch);
                    g.transform.localPosition = glitchHome[i] + new Vector3(jx, 0f, 0f);
                    Color c = g.color;
                    c.a = (0.55f + 0.35f * Mathf.PerlinNoise(step * 0.5f, i)) * (1f - repair);
                    g.color = c;
                }
            if (barFill != null)
            {
                float w = barWidth * Mathf.Clamp01(shownBar);
                barFill.localScale = new Vector3(Mathf.Max(0.001f, w), barFill.localScale.y, 1f);
                barFill.localPosition = new Vector3(-barWidth * 0.5f + w * 0.5f, barFill.localPosition.y, barFill.localPosition.z);
                Transform holder = barFill.parent;
                if (holder != null) holder.localRotation = Quaternion.Euler(0f, 0f, (2f + 5f * glitch) * (1f - repair) * (1f + 0.15f * Mathf.Sin(t * 3f)));
            }
            if (barFillArt != null) barFillArt.color = Color.Lerp(BarGood, BarBad, glitch);
            bool happy = repair >= 0.5f;
            HalloweenKit.Show(smileyBroken, !happy);
            HalloweenKit.Show(smileyHappy, happy);
            if (smileyBroken != null && !happy)
                smileyBroken.localRotation = Quaternion.Euler(0f, 0f, -6f + Mathf.Sin(t * (2f + 8f * glitch)) * (2f + 6f * glitch));
            if (statusText != null)
            {
                int pct = Mathf.RoundToInt(Mathf.Clamp01(shownBar) * 100f);
                statusText.text = repair >= 1f ? "ALL FIXED! 100%" : "LOADING... " + pct + "%   BUGS: " + logic.Count;
            }
        }

        // ---------- Vittoria ----------

        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            V64AFingers.Reset();
            HalloweenKit.Show(infoHolder, false);
            int loved = Mathf.Clamp(logic.PressedBug, 0, logic.Count - 1);
            Vector3 at = new Vector3(logic.BugX(loved), logic.BugY(loved), 0f);
            repair = 0f;
            Pose(0f);
            HalloweenKit.Show(meter, false);
            // 1) l'insetto si stringe tutto rosso... e diventa un cuore
            Sfx.Play(this, "pop", 0.8f);
            Haptics.Light();
            float t = 0f;
            while (t < 0.18f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.18f);
                if (bugs[loved] != null) bugs[loved].localScale = new Vector3(1f + 0.25f * k, 1f - 0.3f * k, 1f);
                yield return null;
            }
            HalloweenKit.Show(bugs[loved], false);
            if (bigHeart != null)
            {
                bigHeart.gameObject.SetActive(true);
                bigHeart.localPosition = new Vector3(at.x, at.y, bigHeart.localPosition.z);
                bigHeart.localScale = Vector3.zero;
            }
            // 2) gli altri scoppiano in cuoricini e la app si ripara
            for (int i = 0; i < logic.Count && i < bugs.Length; i++)
            {
                if (i == loved) continue;
                Vector3 p = new Vector3(logic.BugX(i), logic.BugY(i), 0f);
                HalloweenKit.Show(bugs[i], false);
                Pop(p);
                if (smallHearts != null && i < smallHearts.Length && smallHearts[i] != null)
                {
                    smallHearts[i].gameObject.SetActive(true);
                    smallHearts[i].localPosition = new Vector3(p.x, p.y, smallHearts[i].localPosition.z);
                    smallHearts[i].localScale = Vector3.zero;
                }
            }
            if (logic.Count > 1) Sfx.Play(this, "pop", 0.6f);
            if (titleText != null) titleText.text = TitleFixed;
            t = 0f;
            while (t < 0.45f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.45f);
                repair = k;
                float pop = k < 0.6f ? Mathf.Lerp(0f, 1.2f, k / 0.6f) : Mathf.Lerp(1.2f, 1f, (k - 0.6f) / 0.4f);
                if (bigHeart != null) bigHeart.localScale = bigHeartScale * pop;
                PoseHearts(k * 0.4f, loved);
                PoseApp(Time.deltaTime);
                yield return null;
            }
            repair = 1f;
            PoseApp(0f);
            Sfx.Play(this, "ding", 0.7f);
            // 3) il cuore sale fluttuando, i cuoricini volano via
            Vector3 from = bigHeart != null ? bigHeart.localPosition : Vector3.zero;
            Vector3 to = new Vector3(0f, -1.7f, from.z);   // sotto la faccina riparata e la fascia del complimento
            t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.6f);
                float e = 1f - (1f - k) * (1f - k);
                if (bigHeart != null)
                {
                    bigHeart.localPosition = Vector3.Lerp(from, to, e) + Vector3.right * Mathf.Sin(k * Mathf.PI * 2f) * 0.12f;
                    bigHeart.localScale = bigHeartScale * (1f + 0.25f * e + 0.06f * Mathf.Sin(t * 14f));
                }
                PoseHearts(0.4f + 0.6f * k, loved);
                yield return null;
            }
            if (reward != null) yield return reward.Play(Compliment);
            if (!IsFinished && logic.TryComplete()) Win(Compliment);
        }

        /// <summary>Cuoricini del finale: crescono, salgono ondeggiando e svaniscono (k 0..1).</summary>
        void PoseHearts(float k, int loved)
        {
            if (smallHearts == null) return;
            for (int i = 0; i < smallHearts.Length && i < logic.Count; i++)
            {
                if (i == loved || smallHearts[i] == null) continue;
                float s = k < 0.25f ? Mathf.Lerp(0f, 1.6f, k / 0.25f) : Mathf.Lerp(1.6f, 0.9f, (k - 0.25f) / 0.75f);
                smallHearts[i].localScale = Vector3.one * s;
                smallHearts[i].localPosition = new Vector3(logic.BugX(i) + Mathf.Sin(k * 9f + i) * 0.15f, logic.BugY(i) + k * 1.4f, smallHearts[i].localPosition.z);
                SpriteRenderer sr = smallHearts[i].GetComponentInChildren<SpriteRenderer>();
                if (sr != null) HalloweenKit.SetAlpha(sr, k < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (k - 0.6f) / 0.4f));
            }
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked) return;
            if (Input.GetKeyDown(KeyCode.T))
            {
                int b = editorTapBug++ % logic.Count;
                React(logic.Down(EditorTapId, logic.BugX(b), logic.BugY(b)));
                React(logic.Up(EditorTapId));
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (!editorHolding) { editorHolding = true; React(logic.Down(EditorHoldId, logic.BugX(0), logic.BugY(0))); }
                else { editorHolding = false; React(logic.Up(EditorHoldId)); }
            }
            if (Input.GetKeyDown(KeyCode.M) && editorHolding && logic.PressedBug >= 0)
            {
                React(logic.Move(EditorHoldId, logic.BugX(logic.PressedBug) + LoveTheBugLogic.SlipRadius + 0.3f, logic.BugY(logic.PressedBug)));
                React(logic.Up(EditorHoldId));
                editorHolding = false;
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                int b = logic.Count > 1 ? logic.Count - 1 : 0;
                React(logic.Down(EditorSecondId, logic.BugX(b), logic.BugY(b)));
                React(logic.Up(EditorSecondId));
            }
            if (Input.GetKeyDown(KeyCode.P)) CancelFingers();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }
#endif
    }
}
