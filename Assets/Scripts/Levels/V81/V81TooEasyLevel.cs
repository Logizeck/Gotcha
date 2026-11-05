using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 34 della roadmap "Too Easy!" (id too_easy) — l'obiettivo "COMPLETE THIS LEVEL." È il rompicapo. Lo schermo
    /// è quasi vuoto: in alto la frase dell'obiettivo (costruita dal livello, NON la riga standard: "COMPLETE", "THIS",
    /// "LEVEL" e "." sono quattro TextMesh separati, ognuno col suo BoxCollider2D sul genitore), in mezzo un grosso
    /// pulsante verde WIN con una faccia furba.
    /// Inganno: ogni pressione di WIN fa apparire un timbro "TOO EASY!" e il pulsante salta in un altro punto (sequenza
    /// fissa, sempre dentro la zona sicura); dalla 3ª fa la linguaccia ("Missed me!"), dalla 4ª la scritta diventa "WIN?",
    /// dalla 6ª "WIN!!", dalla 5ª si dimena; all'8ª una frase-indizio ("Have you actually READ the objective?"). Il
    /// pulsante non fa MAI vincere. Toccare COMPLETE, THIS o il punto: la parola oscilla ("Nice try.").
    /// Soluzione: toccare la parola LEVEL. Lampeggia, si gonfia e sparisce con uno sbuffo ("COMPLETE THIS ___."): non c'è
    /// più nessun livello da completare. Il pulsante resta a bocca aperta e svanisce, arriva un cervellone rosa che
    /// applaude con le manine; LevelReward "YOUR IQ IS SUSPICIOUS!" -> Win una volta sola.
    /// La riga si impagina ogni fotogramma dalle LARGHEZZE MISURATE dei testi (TooEasyLogic.Row), si rimpicciolisce se
    /// non entra in ±2.5 (FitScale) e le aree di tocco arrivano a metà dello spazio fra le parole (HitSpans, generose,
    /// mai sovrapposte): una traduzione cambia solo le stringhe (e l'indice della parola giusta, targetWord).
    /// Dita: V64AFingers (al massimo due id; ogni dito che scende è un tocco: niente trascinamenti, niente da annullare).
    /// Pausa, app in background o livello spento chiudono il salto sul posto. Non si perde mai.
    /// La logica è in Gotcha.Levels.V64.TooEasyLogic.
    /// Tasti di prova nell'Editor: W preme WIN, X cinque pressioni nello stesso istante (conta solo la prima), C tocco su
    /// COMPLETE, T tocco su THIS, D tocco sul punto, L tocco su LEVEL (soluzione), P pausa simulata, R ricomincia.
    /// </summary>
    public class V81TooEasyLevel : LevelController
    {
        [Header("Pulsante WIN (radice che salta, corpo che si dimena)")]
        [SerializeField] Transform button;
        [SerializeField] Transform buttonBody;
        [SerializeField] Collider2D buttonArea;
        [SerializeField] Transform faceSmug;
        [SerializeField] Transform faceTongue;
        [SerializeField] Transform faceShock;
        [SerializeField] TextMesh winLabel;
        [SerializeField] SpriteRenderer puff;
        [Header("Limiti del centro del pulsante (pulsante in scala 1.15, largo 2.65: bordi entro x ±2.25)")]
        [SerializeField] float minX = -0.9f;
        [SerializeField] float maxX = 0.9f;
        [SerializeField] float minY = -2.9f;
        [SerializeField] float maxY = 1.4f;
        [Header("Timbro TOO EASY!")]
        [SerializeField] Transform stamp;
        [SerializeField] TextMesh stampText;
        [Header("Frase dell'obiettivo (una parola per TextMesh, collider sul genitore)")]
        [SerializeField] Transform sentenceRow;
        [SerializeField] TextMesh[] words;
        [SerializeField] int targetWord = 2;
        [SerializeField] TextMesh wordGap;              // "___" al posto di LEVEL sparita
        [SerializeField] float maxRowWidth = 5.0f;
        [Header("Il cervellone che applaude")]
        [SerializeField] Transform brain;
        [SerializeField] Transform handLeft;
        [SerializeField] Transform handRight;
        [SerializeField] Transform[] sparkles;
        [Header("Scritte e festa")]
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "YOUR IQ IS SUSPICIOUS!";
        static readonly string[] Labels = { "WIN", "WIN?", "WIN!!" };
        // battute del pulsante, per numero di pressione (1, 2, ...); poi si gira in tondo su LoopLines
        static readonly string[] PressLines = { null, "Again? Sure.", "Missed me!", "Still too easy!", "Are you even trying?", "WIN!! ...just kidding.", "I could do this all day." };
        static readonly string[] LoopLines = { "Missed me!", "Still too easy!", "Are you even trying?", "Nope. Too easy.", "Boring!" };
        const string NudgeLine = "Psst. Have you actually READ the objective?";
        static readonly string[][] WordLines =
        {
            new[] { "Nice try.", "COMPLETE is just a word.", "Completing... nope." },
            new[] { "THIS? This what?", "Nice try.", "Pointing at THIS won't help." },
            new[] { "Nice try." },
            new[] { "That's just a dot.", "Full stop. Nope.", "Dots don't win levels." },
        };
        const float SpaceRatio = 0.32f, PunctGap = 0.03f, OuterPad = 0.35f, MinHitWidth = 0.7f;
        const float HitPadY = 0.45f, MinHitHeight = 0.95f, TongueShow = 1.3f, StampTime = 1.05f;
        const float ClapRest = -18f, ClapShut = 54f;
        static readonly Color GapColor = new Color(1f, 0.86f, 0.45f, 0.9f), FlashColor = Color.white;

        sealed class Word
        {
            public TextMesh text;
            public Transform t;
            public MeshRenderer mr;
            public BoxCollider2D col;
            public Color color;
            public Vector3 baseScale;
            public float width;
        }

        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        TooEasyLogic logic;
        Camera cam;
        HwToast toast;
        Word[] w;
        Word gap;
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        float[] spanL = new float[0], spanR = new float[0];
        Vector3 bodyHome, rowScale, brainScale, stampScale, handLScale, handRScale;
        Color stampColor;
        Coroutine stampCo, clapCo;
        float lineHeight = 0.44f, tongueT, puffT = 1f;
        Vector3 puffAt;
        int loopLines;
        int[] wordLines;
        bool captured, finishing;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            stampCo = clapCo = null;
            if (!captured)
            {
                captured = true;
                int n = words != null ? words.Length : 0;
                w = new Word[n];
                for (int i = 0; i < n; i++) w[i] = MakeWord(words[i]);
                gap = wordGap != null ? MakeWord(wordGap) : null;
                logic = new TooEasyLogic(minX, maxX, minY, maxY, button.localPosition.x, button.localPosition.y, Mathf.Max(1, n), targetWord);
                bodyHome = buttonBody.localPosition;
                rowScale = sentenceRow.localScale;
                brainScale = brain != null ? brain.localScale : Vector3.one;
                handLScale = handLeft != null ? handLeft.localScale : Vector3.one;
                handRScale = handRight != null ? handRight.localScale : Vector3.one;
                stampScale = stamp != null ? stamp.localScale : Vector3.one;
                stampColor = stampText != null ? stampText.color : Color.red;
                wordLines = new int[Mathf.Max(1, n)];
            }
            logic.Reset();
            V64AFingers.Reset();
            if (reward != null) reward.ResetReward();
            finishing = false;
            loopLines = 0;
            for (int i = 0; i < wordLines.Length; i++) wordLines[i] = 0;
            toast = new HwToast(infoText, infoHolder);
            RestPose();
        }

        static Word MakeWord(TextMesh text)
        {
            Transform t = text.transform.parent != null ? text.transform.parent : text.transform;
            var word = new Word
            {
                text = text,
                t = t,
                mr = text.GetComponent<MeshRenderer>(),
                col = t.GetComponent<BoxCollider2D>(),
                color = text.color,
                baseScale = t.localScale,
            };
            // stima prima della prima misura vera (poi conta solo la larghezza misurata del testo)
            word.width = Mathf.Max(0.1f, (text.text != null ? text.text.Length : 1) * text.characterSize * 5.5f);
            return word;
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            foreach (Word x in w)
            {
                HalloweenKit.Show(x.t, true);
                x.t.localScale = x.baseScale;
                x.t.localRotation = Quaternion.identity;
                x.text.color = x.color;
            }
            if (gap != null) { HalloweenKit.Show(gap.t, false); gap.text.color = GapColor; }
            HalloweenKit.Show(button, true);
            button.localScale = Vector3.one * ButtonScale();
            button.localRotation = Quaternion.identity;
            buttonBody.localPosition = bodyHome;
            buttonBody.localScale = Vector3.one;
            buttonBody.localRotation = Quaternion.identity;
            tongueT = 0f;
            puffT = 1f;
            if (puff != null) puff.gameObject.SetActive(false);
            if (stamp != null) stamp.gameObject.SetActive(false);
            if (brain != null) { brain.gameObject.SetActive(false); brain.localScale = brainScale; }
            Hands(ClapRest);
            if (sparkles != null) foreach (Transform s in sparkles) HalloweenKit.Show(s, false);
            Pose(0f);
            MeasureWords();
            Layout();
        }

        float buttonScale = -1f;
        float ButtonScale()
        {
            if (buttonScale < 0f) buttonScale = button.localScale.x;
            return buttonScale;
        }

        void Hands(float degrees)
        {
            if (handRight != null) { handRight.localRotation = Quaternion.Euler(0f, 0f, degrees); handRight.localScale = handRScale; }
            if (handLeft != null) { handLeft.localRotation = Quaternion.Euler(0f, 0f, -degrees); handLeft.localScale = handLScale; }
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: dita annullate, il salto si chiude sul posto, effetti fermati
            CancelFingers();
            StopFx();
            stampCo = null;
            if (captured && !finishing && !IsFinished && logic != null) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola, la logica fa da guardia)
            if (logic.Stage == TooEasyStage.Celebrating) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelFingers(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelFingers(); }

        void CancelFingers()
        {
            V64AFingers.Reset();
            if (logic == null || logic.InputLocked) return;
            logic.Cancel();
        }

        void Update()
        {
            toast?.Tick();
            if (logic == null || IsFinished || toast == null) return;
            MeasureWords();
            Layout();
            if (finishing) return;
            if (Time.timeScale <= 0f) { CancelFingers(); return; }   // pausa o pubblicità: niente input
#if UNITY_EDITOR
            EditorKeys();
            if (finishing) return;
#endif
            ReadFingers();
            if (finishing) return;
            logic.Tick(Time.deltaTime);
            Pose(Time.deltaTime);
        }

        /// <summary>Ogni dito che scende è un tocco (prima le parole, poi il pulsante). Niente prese da annullare.</summary>
        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            for (int i = 0; i < fingers.Count && !finishing; i++)
                if (fingers[i].began) Tap(fingers[i].world);
        }

        void Tap(Vector2 p)
        {
            int word = WordAt(p);
            if (word >= 0) { TapWord(word); return; }
            if (buttonArea != null && button.gameObject.activeInHierarchy && buttonArea.OverlapPoint(p)) PressWin();
        }

        int WordAt(Vector2 p)
        {
            for (int i = 0; i < w.Length; i++)
                if (w[i].col != null && w[i].t.gameObject.activeInHierarchy && w[i].col.OverlapPoint(p)) return i;
            return -1;
        }

        // ---------- Il pulsante bugiardo ----------

        void PressWin()
        {
            if (logic.PressWin() != TooEasyPress.Jumped) return;
            int n = logic.Presses;
            ShowStamp(new Vector3(logic.FromX, logic.FromY + 0.15f, 0f), n);
            puffAt = new Vector3(logic.FromX, logic.FromY - 0.3f, button.localPosition.z);
            puffT = 0f;
            Sfx.Play(this, "whoosh", 0.6f);
            Haptics.Tap();
            string line = logic.TakeNudge() ? NudgeLine : n - 1 < PressLines.Length ? PressLines[n - 1] : LoopLines[loopLines++ % LoopLines.Length];
            if (line != null) toast.Show(line, line == NudgeLine ? 2.8f : 1.5f);
            if (logic.Tongue)
            {
                tongueT = TongueShow;
                Sfx.Play(this, "mock_laugh", 0.45f);
            }
            if (n >= 5) FxWiggle(buttonBody, 9f, 0.6f, 34f);
            if (winLabel != null) winLabel.text = Labels[Mathf.Clamp(logic.LabelStage, 0, Labels.Length - 1)];
        }

        /// <summary>Il timbro "TOO EASY!" nel punto lasciato: compare con un rimbalzo, inclinato, e sbiadisce.</summary>
        void ShowStamp(Vector3 at, int n)
        {
            if (stamp == null || stampText == null) return;
            if (stampCo != null) StopCoroutine(stampCo);
            stamp.localPosition = new Vector3(at.x, at.y, stamp.localPosition.z);
            stamp.localRotation = Quaternion.Euler(0f, 0f, n % 2 == 0 ? -9f : 9f);
            stampCo = StartCoroutine(StampRun());
        }

        IEnumerator StampRun()
        {
            stamp.gameObject.SetActive(true);
            for (float t = 0f; t < StampTime; t += Time.deltaTime)
            {
                float pop = t < 0.12f ? Mathf.Lerp(0.2f, 1.2f, t / 0.12f) : t < 0.22f ? Mathf.Lerp(1.2f, 1f, (t - 0.12f) / 0.1f) : 1f;
                stamp.localScale = stampScale * pop;
                Color c = stampColor;
                c.a = stampColor.a * Mathf.Clamp01((StampTime - t) / 0.35f);
                stampText.color = c;
                yield return null;
            }
            stamp.gameObject.SetActive(false);
            stampText.color = stampColor;
            stamp.localScale = stampScale;
            stampCo = null;
        }

        void Pose(float dt)
        {
            if (logic.Stage != TooEasyStage.Playing) return;
            float t = Time.time;
            Vector3 to = new Vector3(logic.X, logic.Y, button.localPosition.z);
            if (logic.Jumping)
            {
                float k = logic.JumpProgress;
                float e = 1f - (1f - k) * (1f - k);
                Vector3 from = new Vector3(logic.FromX, logic.FromY, to.z);
                // il saltello non porta mai il pulsante sotto la frase dell'obiettivo
                float arc = Mathf.Clamp(maxY + 0.15f - Mathf.Max(from.y, to.y), 0f, 0.5f);
                button.localPosition = Vector3.Lerp(from, to, e) + Vector3.up * (Mathf.Sin(k * Mathf.PI) * arc);
                buttonBody.localScale = new Vector3(0.88f, 1.12f, 1f);
            }
            else
            {
                button.localPosition = to;
                // dondolio soddisfatto
                buttonBody.localScale = new Vector3(1f + Mathf.Sin(t * 3f) * 0.015f, 1f - Mathf.Sin(t * 3f) * 0.015f, 1f);
                buttonBody.localPosition = bodyHome + new Vector3(0f, Mathf.Abs(Mathf.Sin(t * 2.2f)) * 0.03f, 0f);
            }
            if (tongueT > 0f) tongueT = Mathf.Max(0f, tongueT - dt);
            bool tongue = tongueT > 0f || (logic.Jumping && logic.Tongue);
            HalloweenKit.Show(faceTongue, tongue);
            HalloweenKit.Show(faceSmug, !tongue);
            HalloweenKit.Show(faceShock, false);
            if (winLabel != null)
            {
                string label = Labels[Mathf.Clamp(logic.LabelStage, 0, Labels.Length - 1)];
                if (winLabel.text != label) winLabel.text = label;
            }
            PuffPose(dt);
        }

        void PuffPose(float dt)
        {
            if (puff == null) return;
            if (puffT < 1f)
            {
                puffT = Mathf.Min(1f, puffT + dt / 0.45f);
                puff.gameObject.SetActive(true);
                puff.transform.localPosition = puffAt;
                puff.transform.localScale = Vector3.one * Mathf.Lerp(1.1f, 2.0f, puffT);
                HalloweenKit.SetAlpha(puff, 0.9f * (1f - puffT));
            }
            else puff.gameObject.SetActive(false);
        }

        // ---------- Le parole dell'obiettivo ----------

        void TapWord(int index)
        {
            switch (logic.TapWord(index))
            {
                case TooEasyTap.Wrong:
                    FxWiggle(w[index].t, 11f, 0.45f, 34f);
                    Sfx.Play(this, "tick", 0.5f);
                    string[] lines = WordLines[Mathf.Clamp(index, 0, WordLines.Length - 1)];
                    toast.Show(lines[wordLines[index]++ % lines.Length], 1.5f);
                    // il pulsante se la ride
                    if (!logic.Jumping) FxWiggle(buttonBody, 5f, 0.35f, 26f);
                    break;
                case TooEasyTap.Solved:
                    StartCoroutine(Finale());
                    break;
            }
        }

        static bool IsPunct(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s) if (char.IsLetterOrDigit(c)) return false;
            return true;
        }

        /// <summary>Larghezza di ogni parola in coordinate della riga, letta dal testo disegnato (solo se ferma e dritta).</summary>
        void MeasureWords()
        {
            foreach (Word x in w) Measure(x, true);
            if (gap != null) Measure(gap, false);
        }

        void Measure(Word x, bool lineRef)
        {
            if (x.mr == null || !x.t.gameObject.activeInHierarchy) return;
            if (Quaternion.Angle(x.t.localRotation, Quaternion.identity) > 0.5f) return;
            if ((x.t.localScale - x.baseScale).sqrMagnitude > 1e-6f) return;
            float sx = Mathf.Abs(x.t.lossyScale.x), sy = Mathf.Abs(x.t.lossyScale.y);
            Vector3 size = x.mr.bounds.size;
            if (size.x > 0.01f && sx > 1e-4f) x.width = size.x / sx;
            if (lineRef && x.text.text != null && x.text.text.Length > 1 && size.y > 0.01f && sy > 1e-4f) lineHeight = size.y / sy;
        }

        /// <summary>Riga centrata dalle larghezze misurate, rimpicciolita se serve, aree di tocco generose.</summary>
        void Layout()
        {
            int n = w.Length;
            if (n == 0) return;
            var widths = new float[n];
            var gaps = new float[n];
            float space = lineHeight * SpaceRatio;
            for (int i = 0; i < n; i++)
            {
                widths[i] = w[i].width;
                gaps[i] = i == 0 ? 0f : IsPunct(w[i].text.text) ? PunctGap : space;
            }
            float[] c = TooEasyLogic.Row(widths, gaps, out float total);
            float k = TooEasyLogic.FitScale(total, maxRowWidth);
            sentenceRow.localScale = rowScale * k;
            TooEasyLogic.HitSpans(c, widths, OuterPad, MinHitWidth, out spanL, out spanR);
            float rowK = Mathf.Max(1e-3f, Mathf.Abs(sentenceRow.lossyScale.y));
            float hitH = Mathf.Max(lineHeight + HitPadY, MinHitHeight / rowK);
            for (int i = 0; i < n; i++)
            {
                Vector3 lp = w[i].t.localPosition;
                w[i].t.localPosition = new Vector3(c[i], 0f, lp.z);
                if (w[i].col != null)
                {
                    w[i].col.size = new Vector2(Mathf.Max(0.05f, spanR[i] - spanL[i]), hitH);
                    w[i].col.offset = new Vector2((spanL[i] + spanR[i]) * 0.5f - c[i], 0f);
                }
            }
            if (gap != null)
            {
                int ti = Mathf.Clamp(logic.Target, 0, n - 1);
                Vector3 gp = gap.t.localPosition;
                gap.t.localPosition = new Vector3(c[ti], 0f, gp.z);
            }
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73-0.80) ----------

        void FxWiggle(Transform t, float degrees, float seconds, float speed) => Fx(0, t, HalloweenKit.Wiggle(t, degrees, seconds, speed));
        void FxPop(Transform t, float peak, float seconds) => Fx(1, t, HalloweenKit.Pop(t, peak, seconds));

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
            if (kind == 0) t.localRotation = slot.q;
            else t.localScale = slot.v;
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

        // ---------- La soluzione ----------

        /// <summary>
        /// LEVEL lampeggia, si gonfia e sparisce con uno sbuffo ("COMPLETE THIS ___."); il pulsante resta a bocca aperta e
        /// svanisce; il cervellone rosa arriva con un rimbalzo e applaude; poi la festa comune e Win, una volta sola.
        /// Rifatta da capo se il livello si spegne a metà (ogni passo parte da una posa fissa).
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            V64AFingers.Reset();
            StopFx();
            if (stampCo != null) { StopCoroutine(stampCo); stampCo = null; }
            if (stamp != null) stamp.gameObject.SetActive(false);
            if (stampText != null) stampText.color = stampColor;
            toast.Show("COMPLETE THIS ___.", 1.6f);
            int ti = Mathf.Clamp(logic.Target, 0, w.Length - 1);
            Word lv = w[ti];
            // posa di partenza fissa (anche dopo uno spegnimento a metà)
            foreach (Word x in w) { x.t.localRotation = Quaternion.identity; x.t.localScale = x.baseScale; x.text.color = x.color; }
            HalloweenKit.Show(lv.t, true);
            if (gap != null) HalloweenKit.Show(gap.t, false);
            HalloweenKit.Show(button, true);
            button.localPosition = new Vector3(logic.X, logic.Y, button.localPosition.z);
            button.localScale = Vector3.one * ButtonScale();
            buttonBody.localPosition = bodyHome;
            buttonBody.localScale = Vector3.one;
            buttonBody.localRotation = Quaternion.identity;
            if (brain != null) brain.gameObject.SetActive(false);
            Hands(ClapRest);

            // 1. LEVEL lampeggia (tre volte), il pulsante si accorge e resta a bocca aperta
            Sfx.Play(this, "tick", 0.6f);
            HalloweenKit.Show(faceSmug, false);
            HalloweenKit.Show(faceTongue, false);
            HalloweenKit.Show(faceShock, true);
            for (int i = 0; i < 6; i++)
            {
                lv.text.color = i % 2 == 0 ? FlashColor : lv.color;
                lv.t.localScale = lv.baseScale * (i % 2 == 0 ? 1.18f : 1.08f);
                yield return new WaitForSeconds(0.06f);
            }
            lv.text.color = FlashColor;
            // 2. si gonfia e scoppia: al suo posto "___"
            for (float t = 0f; t < 0.18f; t += Time.deltaTime)
            {
                float k = t / 0.18f;
                lv.t.localScale = lv.baseScale * (k < 0.4f ? Mathf.Lerp(1.08f, 1.45f, k / 0.4f) : Mathf.Lerp(1.45f, 0.05f, (k - 0.4f) / 0.6f));
                yield return null;
            }
            HalloweenKit.Show(lv.t, false);
            lv.t.localScale = lv.baseScale;
            lv.text.color = lv.color;
            Sfx.Play(this, "pop", 0.8f);
            Haptics.Medium();
            if (puff != null)
            {
                puff.gameObject.SetActive(true);
                puff.transform.position = new Vector3(lv.t.position.x, lv.t.position.y, puff.transform.position.z);
                puff.transform.localScale = Vector3.one * 1.2f;
                HalloweenKit.SetAlpha(puff, 0.85f);
            }
            if (gap != null) { HalloweenKit.Show(gap.t, true); gap.text.color = GapColor; }
            // 3. il pulsante (che non ha più niente da far vincere) si sgonfia e sparisce nello sbuffo
            FxWiggle(buttonBody, 12f, 0.3f, 40f);
            for (float t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                float k = t / 0.3f;
                if (puff != null) { puff.transform.localScale = Vector3.one * Mathf.Lerp(1.2f, 2.2f, k); HalloweenKit.SetAlpha(puff, 0.85f * (1f - k)); }
                yield return null;
            }
            if (puff != null)
            {
                puff.gameObject.SetActive(true);
                puff.transform.localPosition = button.localPosition;
                puff.transform.localScale = Vector3.one * 2.2f;
                HalloweenKit.SetAlpha(puff, 0.8f);
            }
            for (float t = 0f; t < 0.22f; t += Time.deltaTime)
            {
                float k = t / 0.22f;
                button.localScale = Vector3.one * ButtonScale() * Mathf.Lerp(1f, 0.05f, k * k);
                if (puff != null) { puff.transform.localScale = Vector3.one * Mathf.Lerp(2.2f, 3.2f, k); HalloweenKit.SetAlpha(puff, 0.8f * (1f - k)); }
                yield return null;
            }
            HalloweenKit.Show(button, false);
            if (puff != null) puff.gameObject.SetActive(false);
            // 4. il cervellone arriva con un rimbalzo e applaude (continua durante la festa)
            if (brain != null)
            {
                brain.gameObject.SetActive(true);
                Sfx.Play(this, "ding", 0.5f);
                for (float t = 0f; t < 0.3f; t += Time.deltaTime)
                {
                    float k = t / 0.3f;
                    brain.localScale = brainScale * (k < 0.7f ? Mathf.Lerp(0.1f, 1.12f, k / 0.7f) : Mathf.Lerp(1.12f, 1f, (k - 0.7f) / 0.3f));
                    yield return null;
                }
                brain.localScale = brainScale;
                if (clapCo != null) StopCoroutine(clapCo);
                clapCo = StartCoroutine(Clap());
            }
            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(2f);
            if (!IsFinished && logic.TryComplete()) Win(Compliment);
        }

        /// <summary>Le manine battono (rotazione attorno alle spalle), un "clap" e due scintille a ogni battuta.</summary>
        IEnumerator Clap()
        {
            int claps = 0;
            while (true)
            {
                for (float t = 0f; t < 0.11f; t += Time.deltaTime) { Hands(Mathf.Lerp(ClapRest, ClapShut, t / 0.11f)); yield return null; }
                Hands(ClapShut);
                V81TooEasySound.Play(this, "clap", 0.6f);
                if (brain != null) brain.localScale = brainScale * 1.04f;
                if (sparkles != null && sparkles.Length > 0)
                {
                    Transform s = sparkles[claps % sparkles.Length];
                    if (s != null) { HalloweenKit.Show(s, true); StartCoroutine(Sparkle(s)); }
                }
                claps++;
                for (float t = 0f; t < 0.16f; t += Time.deltaTime)
                {
                    Hands(Mathf.Lerp(ClapShut, ClapRest, t / 0.16f));
                    if (brain != null) brain.localScale = brainScale * Mathf.Lerp(1.04f, 1f, t / 0.16f);
                    yield return null;
                }
                Hands(ClapRest);
                yield return new WaitForSeconds(0.05f);
            }
        }

        IEnumerator Sparkle(Transform s)
        {
            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                float k = t / 0.5f;
                s.localScale = Vector3.one * (k < 0.3f ? k / 0.3f : 1f - (k - 0.3f) / 0.7f) * 0.9f;
                s.localRotation = Quaternion.Euler(0f, 0f, t * 300f);
                yield return null;
            }
            HalloweenKit.Show(s, false);
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked) return;
            if (Input.GetKeyDown(KeyCode.W)) PressWin();
            if (Input.GetKeyDown(KeyCode.X)) for (int i = 0; i < 5; i++) PressWin();   // tocchi rapidissimi: conta solo il primo
            if (Input.GetKeyDown(KeyCode.C)) TapWord(0);
            if (Input.GetKeyDown(KeyCode.T)) TapWord(1);
            if (Input.GetKeyDown(KeyCode.D)) TapWord(w.Length - 1);
            if (Input.GetKeyDown(KeyCode.P)) CancelFingers();
            if (Input.GetKeyDown(KeyCode.R)) { OnLevelStart(); return; }
            if (Input.GetKeyDown(KeyCode.L)) TapWord(logic.Target);
        }
#endif
    }

    /// <summary>Suono sintetizzato di Too Easy!: il battimani del cervellone (gli altri sono quelli comuni di Sfx).</summary>
    public static class V81TooEasySound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(341);

        public static void Play(Component owner, string kind, float volume = 0.6f)
        {
            if (owner == null) return;
            AudioClip c = Clip(kind);
            if (c == null) { Sfx.Play(owner, kind, volume); return; }
            AudioSource src = owner.GetComponent<AudioSource>();
            if (src == null) { src = owner.gameObject.AddComponent<AudioSource>(); src.playOnAwake = false; }
            src.PlayOneShot(c, volume);
        }

        static AudioClip Clip(string kind)
        {
            if (cache.TryGetValue(kind, out AudioClip c) && c != null) return c;
            if (kind != "clap") return null;
            // battimani: rumore filtrato con tre micro-colpi ravvicinati
            int n = (int)(0.12f * Rate);
            var data = new float[n];
            float lp = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float noise = (float)rnd.NextDouble() * 2f - 1f;
                lp += (noise - lp) * 0.55f;
                float env = Mathf.Exp(-t * 55f) + 0.5f * Mathf.Exp(-Mathf.Abs(t - 0.008f) * 400f) + 0.35f * Mathf.Exp(-Mathf.Abs(t - 0.017f) * 400f);
                data[i] = Mathf.Clamp((noise - lp) * 0.8f * env, -1f, 1f);
            }
            c = AudioClip.Create("v81a_clap", n, 1, Rate, false);
            c.SetData(data, 0);
            cache[kind] = c;
            return c;
        }
    }
}
