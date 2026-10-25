using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V63;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 257 "The lying clocks" — la porta si apre solo con l'ora esatta (tastierino HH:MM con le frecce e OK).
    /// Il tempo di gioco scorre a un minuto ogni 3 secondi. Tre orologi, tutti bugiardi: il cucù è indietro (ma canta
    /// allo scoccare dell'ora vera, tante volte quante l'ora), la sveglia è ferma, l'orologio da timbro corre il doppio.
    /// La radio dice solo sciocchezze. Soluzione: aspettare il cucù, ricavare il ritardo (lui segna xx:(60 - ritardo)
    /// quando l'ora vera è HH:00) e impostare l'ora vera corrente (±1 minuto). Al terzo errore si perde.
    /// </summary>
    public class V63LyingClocksLevel : V62Level
    {
        [SerializeField] Transform cuckooHour, cuckooMinute;
        [SerializeField] Transform stoppedHour, stoppedMinute;
        [SerializeField] Transform fastHour, fastMinute;
        [SerializeField] Transform cuckooBird;           // esce dalla casetta quando canta
        [SerializeField] Transform cuckooClock;
        [SerializeField] Collider2D cuckooArea;
        [SerializeField] Transform stoppedClock;
        [SerializeField] Collider2D stoppedArea;
        [SerializeField] Transform fastClock;
        [SerializeField] Collider2D fastArea;
        [SerializeField] Transform radio;
        [SerializeField] Collider2D radioArea;
        [SerializeField] TextMesh display;
        [SerializeField] Collider2D hourUp, hourDown, minuteUp, minuteDown, okArea;
        [SerializeField] Transform okButton;
        [SerializeField] SpriteRenderer[] mistakeLights;
        [SerializeField] Transform doorPanel;
        [SerializeField] Collider2D doorArea;
        [SerializeField] SpeechBubble cuckooBubble;
        [SerializeField] SpeechBubble doorBubble;
        [SerializeField] SpeechBubble radioBubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float RepeatDelay = 0.45f, RepeatEvery = 0.08f;

        readonly LyingClocksLogic logic = new LyingClocksLogic();
        Camera cam;
        HwSay cuckooSay, doorSay, radioSay;
        HwToast toast;
        Vector3 birdIn, birdOut, doorScale;
        Collider2D held;
        float nextRepeat, tapReadyAt;
        int radioLines;
        bool busy, singing;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset(Random.Range(0, int.MaxValue));
            ResetV62();
            cuckooSay = new HwSay(cuckooBubble);
            doorSay = new HwSay(doorBubble);
            radioSay = new HwSay(radioBubble);
            toast = new HwToast(infoText, infoHolder);
            birdOut = cuckooBird.localPosition;
            birdIn = birdOut + Vector3.down * 0.35f;
            cuckooBird.localPosition = birdIn;
            HalloweenKit.Show(cuckooBird, false);
            doorScale = doorPanel.localScale;
            foreach (SpriteRenderer l in mistakeLights) if (l != null) l.color = new Color(0.35f, 0.35f, 0.4f);
            radioLines = Random.Range(0, 6);
            UpdateClocks();
            UpdateDisplay();
            toast.Show("Watch the clocks...", 2f);
        }

        void Update()
        {
            cuckooSay.Tick();
            doorSay.Tick();
            radioSay.Tick();
            toast.Tick();
            if (IsFinished) return;
            int calls = logic.Tick(Time.deltaTime);
            if (calls > 0 && !singing) StartCoroutine(Cuckoo(calls));
            UpdateClocks();
            if (busy || logic.InputLocked) return;
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                held = null;
                return;
            }
            if (began) Press(w);
            else if (held != null && held.OverlapPoint(w) && Time.time >= nextRepeat)
            {
                Step(held);
                nextRepeat = Time.time + RepeatEvery;
            }
            if (ended) held = null;
        }

        void UpdateClocks()
        {
            SetHands(cuckooHour, cuckooMinute, logic.CuckooTime);
            SetHands(stoppedHour, stoppedMinute, logic.StoppedTime);
            SetHands(fastHour, fastMinute, logic.FastTime);
        }

        static void SetHands(Transform hour, Transform minute, float t)
        {
            if (minute != null) minute.localRotation = Quaternion.Euler(0f, 0f, -(t % 60f) * 6f);
            if (hour != null) hour.localRotation = Quaternion.Euler(0f, 0f, -(t / 60f % 12f) * 30f);
        }

        void UpdateDisplay()
        {
            if (display != null) display.text = LyingClocksLogic.Format(logic.Hour, logic.Minute);
        }

        void Press(Vector2 w)
        {
            foreach (Collider2D c in new[] { hourUp, hourDown, minuteUp, minuteDown })
            {
                if (c == null || !c.OverlapPoint(w)) continue;
                Step(c);
                held = c;
                nextRepeat = Time.time + RepeatDelay;
                return;
            }
            if (okArea.OverlapPoint(w)) { Ok(); return; }
            // i tocchi "di colore" hanno una piccola pausa: le animazioni non si sovrappongono
            if (Time.time < tapReadyAt) return;
            tapReadyAt = Time.time + 0.4f;
            if (radioArea.OverlapPoint(w))
            {
                Sfx.Play(this, "buzz", 0.35f);
                StartCoroutine(HalloweenKit.Shake(radio, 0.03f, 0.3f));
                radioSay.Show(LyingClocksLogic.RadioLine(radioLines++), 2.2f);
                return;
            }
            if (doorArea.OverlapPoint(w))
            {
                Sfx.Play(this, "clunk", 0.5f);
                StartCoroutine(HalloweenKit.Shake(doorPanel, 0.03f, 0.25f));
                doorSay.Show("Type the time. The RIGHT time.", 1.8f);
                return;
            }
            if (cuckooArea.OverlapPoint(w)) { Sfx.Play(this, "tick", 0.5f); toast.Show("Tick, tock. Wait for the bird.", 1.6f); return; }
            if (stoppedArea.OverlapPoint(w)) { Sfx.Play(this, "tick", 0.3f); StartCoroutine(HalloweenKit.Shake(stoppedClock, 0.03f, 0.25f)); toast.Show("...not a single tick.", 1.6f); return; }
            if (fastArea.OverlapPoint(w)) { Sfx.Play(this, "tick", 0.7f); StartCoroutine(HalloweenKit.Shake(fastClock, 0.02f, 0.25f)); toast.Show("Ticktickticktick!", 1.4f); }
        }

        void Step(Collider2D c)
        {
            bool ok = c == hourUp ? logic.StepHour(1) : c == hourDown ? logic.StepHour(-1)
                : c == minuteUp ? logic.StepMinute(1) : logic.StepMinute(-1);
            if (!ok) return;
            Sfx.Play(this, "tick", 0.4f);
            UpdateDisplay();
        }

        void Ok()
        {
            held = null;
            ClockAnswer a = logic.TryOk();
            if (a == ClockAnswer.None) return;
            StartCoroutine(HalloweenKit.Pop(okButton, 0.88f, 0.15f));
            if (a == ClockAnswer.Correct) { StartCoroutine(Open()); return; }
            StartCoroutine(Wrong(a));
        }

        IEnumerator Wrong(ClockAnswer a)
        {
            busy = true;
            for (int i = 0; i < mistakeLights.Length; i++)
                if (mistakeLights[i] != null && i < logic.Mistakes) mistakeLights[i].color = new Color(1f, 0.3f, 0.25f);
            Sfx.Play(this, "mock_laugh", 0.8f);
            Haptics.Medium();
            doorSay.Show("HA HA! " + LyingClocksLogic.Answer(a), 2.2f);
            yield return HalloweenKit.Shake(doorPanel, 0.05f, 0.6f);
            if (logic.State == ClockState.Failed)
            {
                yield return new WaitForSeconds(1.2f);
                Fail(LyingClocksLogic.FailTaunt);
                yield break;
            }
            toast.Show((LyingClocksLogic.MaxMistakes - logic.Mistakes) == 1 ? "Last chance!" : "Two chances left.", 1.6f);
            busy = false;
        }

        IEnumerator Cuckoo(int calls)
        {
            singing = true;
            HalloweenKit.Show(cuckooBird, true);
            StartCoroutine(HalloweenKit.Shake(cuckooClock, 0.02f, 0.3f));
            for (int i = 0; i < calls; i++)
            {
                yield return MoveLocal(cuckooBird, birdOut, 0.1f);
                Sfx.Play(this, "ding", 0.45f);
                cuckooSay.Show("CUCKOO!", 0.4f);
                yield return new WaitForSeconds(0.22f);
                yield return MoveLocal(cuckooBird, birdOut + Vector3.down * 0.12f, 0.08f);
                yield return new WaitForSeconds(0.12f);
            }
            yield return MoveLocal(cuckooBird, birdIn, 0.15f);
            HalloweenKit.Show(cuckooBird, false);
            singing = false;
        }

        IEnumerator Open()
        {
            busy = true;
            Sfx.Play(this, "ding", 0.8f);
            doorSay.Show(LyingClocksLogic.Answer(ClockAnswer.Correct), 2f);
            yield return new WaitForSeconds(0.5f);
            Sfx.Play(this, "clunk", 0.8f);
            Haptics.Medium();
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.7f)
            {
                float e = 1f - (1f - t) * (1f - t);
                doorPanel.localScale = new Vector3(Mathf.Lerp(doorScale.x, doorScale.x * 0.12f, e), doorScale.y, doorScale.z);
                yield return null;
            }
            toast.Show("The door is open!", 1.8f);
            yield return Celebrate(2f);
            if (logic.TryCompleteOpen()) Win();
        }

        static IEnumerator MoveLocal(Transform t, Vector3 to, float seconds)
        {
            Vector3 from = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / Mathf.Max(0.01f, seconds))
            {
                t.localPosition = Vector3.Lerp(from, to, k);
                yield return null;
            }
            t.localPosition = to;
        }
    }
}
