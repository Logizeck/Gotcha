using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Knock knock" — una porta segreta: "SECRET KNOCK ONLY: knock... knock-knock". Toccare la porta sullo
    /// schermo non conta ("That's the GLASS, not the door!"). Soluzione: bussare con le nocche sul RETRO del telefono
    /// col ritmo giusto (un colpo, pausa, due colpi veloci). I colpi si riconoscono dagli scatti improvvisi
    /// dell'accelerometro; quelli che coincidono con un tocco dello schermo e lo scuotimento continuo non valgono.
    /// (Editor: K = un colpo.)
    /// </summary>
    public class KnockLevel : LevelController
    {
        [SerializeField] Transform door;              // cardine della porta (si apre scalando x)
        [SerializeField] Transform eye;               // occhio nello spioncino (nascosto)
        [SerializeField] Transform pupil;
        [SerializeField] Transform guest;             // chi apre (dietro la porta)
        [SerializeField] TextMesh[] knockTexts;       // scritte "KNOCK!" riutilizzate a turno
        [SerializeField] SpriteRenderer[] dots;       // avanzamento del ritmo (3 pallini)
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float jolt = 0.22f, debounce = 0.12f, touchGuard = 0.15f;
        [SerializeField] float pauseMin = 0.4f, quickMax = 0.5f, maxGap = 2.2f, hintAfter = 12f;

        static readonly string[] Replies =
        {
            "That's the GLASS, not the door!", "Ouch! That's my screen!", "Fingertips? Use your KNUCKLES.",
            "I can't hear you through the glass!", "Still glass. Still not a door.",
        };

        readonly List<float> pending = new List<float>();   // colpi in attesa di verifica (tocchi vicini?)
        readonly List<float> touches = new List<float>();   // istanti degli ultimi tocchi sullo schermo
        Vector3 lastAcc, doorBase;
        bool hasAcc, busy, hinted;
        float clock, lastCandidate = -9f, lastActive = -9f, activeFor, shakeCooldown, rattle;
        float t1, t2, bubbleUntil;
        int state, taps, textIndex;

        public override void OnLevelStart()
        {
            doorBase = door.localPosition;
            eye.gameObject.SetActive(false);
            foreach (TextMesh t in knockTexts) t.gameObject.SetActive(false);
            Dots(0);
            Say("Password, please.", 2.2f);
        }

        void Dots(int lit)
        {
            for (int i = 0; i < dots.Length; i++) dots[i].color = i < lit ? new Color(1f, 0.85f, 0.3f) : new Color(1f, 1f, 1f, 0.3f);
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void Update()
        {
            if (IsFinished) return;
            float dt = Time.deltaTime;
            clock += dt;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil && !busy) { bubble.Hide(); bubbleUntil = 0f; }

            // porta che trema dopo un colpo
            if (rattle > 0f)
            {
                rattle -= dt;
                door.localPosition = doorBase + (Vector3)(Random.insideUnitCircle * 0.04f * Mathf.Clamp01(rattle / 0.15f));
                if (rattle <= 0f) door.localPosition = doorBase;
            }
            if (busy) return;

            // tocchi sullo schermo: non contano come colpi
            bool tapped = false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                TouchPhase ph = Input.GetTouch(i).phase;
                if (ph == TouchPhase.Began || ph == TouchPhase.Ended || ph == TouchPhase.Canceled) touches.Add(clock);
                if (ph == TouchPhase.Began) tapped = true;
            }
            if (Input.touchCount == 0 && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))) touches.Add(clock);
            if (Input.touchCount == 0 && Input.GetMouseButtonDown(0)) tapped = true;
            while (touches.Count > 0 && clock - touches[0] > 1f) touches.RemoveAt(0);
            if (tapped && !PointerInput.IsOverUi()) ScreenTap();

            // scatti dell'accelerometro (tutti i campioni dall'ultimo frame)
            float j = 0f;
            int n = Input.accelerationEventCount;
            for (int i = 0; i < n; i++) Sample(Input.GetAccelerationEvent(i).acceleration, ref j);
            if (n == 0) Sample(Input.acceleration, ref j);
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.K)) j = 1f;
#endif
            // scuotimento continuo
            if (j > 0.12f) lastActive = clock;
            activeFor = clock - lastActive < 0.2f ? activeFor + dt : 0f;
            shakeCooldown -= dt;
            bool shaking = activeFor > 0.6f || (SensorHub.Instance != null && SensorHub.Instance.IsShaking);
            if (shaking)
            {
                pending.Clear();
                if (state > 0) { state = 0; Dots(0); }
                if (shakeCooldown <= 0f) { Say("Don't shake the door!", 1.6f); Sfx.Play(this, "buzz", 0.5f); shakeCooldown = 2f; }
                return;
            }
            if (j > jolt && clock - lastCandidate > debounce && activeFor < 0.35f)
            {
                lastCandidate = clock;
                pending.Add(clock);
                Haptics.Light();
            }

            // un colpo vale solo se nessun tocco è avvenuto entro ±touchGuard
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                float t = pending[i];
                bool nearTouch = false;
                foreach (float tt in touches) if (Mathf.Abs(tt - t) <= touchGuard) { nearTouch = true; break; }
                if (nearTouch) { pending.RemoveAt(i); continue; }
                if (clock - t >= touchGuard) { pending.RemoveAt(i); Knock(t); if (busy) return; }
            }

            // il ritmo scade
            if (state == 2 && clock - t2 > quickMax + touchGuard + 0.1f) { WrongPattern("Wrong knock! It's knock... knock-knock."); }
            else if (state == 1 && clock - t1 > maxGap + touchGuard) { WrongPattern("Knock... and then?"); }

            if (!hinted && clock > hintAfter) Hint();
        }

        void Sample(Vector3 a, ref float j)
        {
            if (hasAcc) j = Mathf.Max(j, (a - lastAcc).magnitude);
            lastAcc = a;
            hasAcc = true;
        }

        void ScreenTap()
        {
            taps++;
            Sfx.Play(this, "tick", 0.7f);
            if (!hinted && taps >= 4) { Hint(); return; }
            if (hinted && taps % 3 == 0) { Say("Knock on the BACK of your phone!", 2.2f); return; }
            Say(Replies[(taps - 1) % Replies.Length], 1.6f);
        }

        void Hint()
        {
            hinted = true;
            Say("Knock on the BACK of your phone!", 2.6f);
        }

        void WrongPattern(string msg)
        {
            state = 0;
            Dots(0);
            Sfx.Play(this, "buzz", 0.4f);
            Say(msg, 1.8f);
        }

        void Knock(float t)
        {
            Sfx.Play(this, "clunk", 0.9f);
            rattle = 0.15f;
            ShowKnockText();
            if (state == 0) { state = 1; t1 = t; }
            else if (state == 1)
            {
                float gap = t - t1;
                if (gap < pauseMin || gap > maxGap) { t1 = t; }   // troppo presto o troppo tardi: ricomincia da qui
                else { state = 2; t2 = t; }
            }
            else
            {
                float gap = t - t2;
                if (gap <= quickMax) { Dots(3); StartCoroutine(Open()); return; }
                // troppo lento: questo colpo diventa il secondo di un nuovo tentativo
                if (gap <= maxGap) { t1 = t2; t2 = t; } else { state = 1; t1 = t; }
            }
            Dots(state);
        }

        void ShowKnockText()
        {
            if (knockTexts.Length == 0) return;
            TextMesh tm = knockTexts[textIndex++ % knockTexts.Length];
            tm.gameObject.SetActive(true);
            tm.transform.localPosition = new Vector3(Random.Range(-1.6f, 1.6f), Random.Range(-1.5f, 1.8f), -0.01f);
            tm.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-18f, 18f));
            StartCoroutine(PopText(tm));
        }

        IEnumerator PopText(TextMesh tm)
        {
            Transform tr = tm.transform;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                tr.localScale = Vector3.one * (k < 0.2f ? Mathf.Lerp(0.4f, 1.2f, k / 0.2f) : 1.2f - (k - 0.2f) * 0.3f);
                Color c = tm.color; c.a = 1f - Mathf.Clamp01((k - 0.6f) / 0.4f); tm.color = c;
                yield return null;
            }
            tm.gameObject.SetActive(false);
        }

        IEnumerator Open()
        {
            busy = true;
            pending.Clear();
            yield return new WaitForSeconds(0.5f);
            Sfx.Play(this, "pop", 0.7f);
            eye.gameObject.SetActive(true);
            Vector3 es = eye.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { eye.localScale = new Vector3(es.x, es.y * k, es.z); yield return null; }
            eye.localScale = es;
            // l'occhio guarda a destra e a sinistra
            Vector3 pc = pupil.localPosition;
            foreach (float dx in new[] { -0.06f, 0.06f, 0f })
            {
                Vector3 from = pupil.localPosition, to = pc + new Vector3(dx, dx == 0f ? -0.02f : 0f, 0f);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.18f) { pupil.localPosition = Vector3.Lerp(from, to, k); yield return null; }
                yield return new WaitForSeconds(0.3f);
            }
            if (bubble != null) bubble.Show("...Correct knock. Come in!");
            yield return new WaitForSeconds(1.2f);
            eye.gameObject.SetActive(false);
            Sfx.Play(this, "whoosh", 0.7f);
            Haptics.Light();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                float e = 1f - (1f - k) * (1f - k);
                door.localScale = new Vector3(Mathf.Lerp(1f, -0.22f, e), 1f, 1f);
                yield return null;
            }
            door.localScale = new Vector3(-0.22f, 1f, 1f);
            // l'ospite saluta
            Vector3 g = guest.localPosition;
            for (float k = 0f; k < 1.4f; k += Time.deltaTime)
            {
                guest.localPosition = g + Vector3.up * Mathf.Abs(Mathf.Sin(k * 9f)) * 0.12f;
                guest.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 9f) * 4f);
                yield return null;
            }
            Sfx.Play(this, "ding", 0.8f);
            yield return new WaitForSeconds(0.6f);
            Win();
        }
    }
}
