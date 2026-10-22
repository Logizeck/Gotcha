using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The endless staircase" — l'omino sale una scala infinita (si tiene premuto per salire) ma la cima si
    /// allontana a ogni gradino. Il trucco: trascinare la parola "TOP" giù fino all'omino: la scala si accorcia e la
    /// vetta compare subito sopra di lui. Tre salite complete fino allo sfinimento = sconfitta.
    /// </summary>
    public class EndlessStaircaseLevel : LevelController
    {
        [SerializeField] Transform[] steps;
        [SerializeField] Transform climber, topSign, summit, staminaPivot;
        [SerializeField] Collider2D topArea;
        [SerializeField] TextMesh stepsText, attemptsText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] Vector2 stepDelta = new Vector2(0.5f, 0.55f);
        [SerializeField] int stepsPerAttempt = 10, attemptsAllowed = 3;
        [SerializeField] float stepSeconds = 0.32f, dropRange = 1.3f;

        Camera cam;
        bool busy, draggingTop, climbing;
        int stamina, attempts, stepsLeft = 12, climbedThisRun;
        Vector3 topHome, climberHome;
        Vector2 dragOffset;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            topHome = topSign.position;
            climberHome = climber.localPosition;
            summit.gameObject.SetActive(false);
            stamina = stepsPerAttempt;
            S41LKit.Confetti(confetti, false);
            Refresh();
            S41LKit.Say(bubble, "Hold the screen to climb to the TOP!");
        }

        void Refresh()
        {
            stepsText.text = "STEPS TO THE TOP: " + stepsLeft;
            attemptsText.text = "CLIMBS " + attempts + "/" + attemptsAllowed;
            staminaPivot.localScale = new Vector3(Mathf.Max(0.001f, stamina / (float)stepsPerAttempt), 1f, 1f);
        }

        void Update()
        {
            if (IsFinished || busy) return;
            // la cima ondeggia lassù, irraggiungibile
            if (!draggingTop) topSign.position = topHome + Vector3.up * Mathf.Sin(Time.time * 2f) * 0.06f;
            bool down = S41LKit.Pointer(cam, out Vector2 w, out bool began, out bool ended);
            if (began)
            {
                if (topArea.OverlapPoint(w)) { draggingTop = true; dragOffset = (Vector2)topSign.position - w; Sfx.Play(this, "tick", 0.5f); }
                else if (bubble != null) bubble.Hide();
            }
            if (draggingTop)
            {
                Vector2 p = w + dragOffset;
                topSign.position = new Vector3(Mathf.Clamp(p.x, -2.3f, 2.3f), Mathf.Clamp(p.y, -4.2f, 3.0f), topSign.position.z);
                if (ended || !down) DropTop();
                return;
            }
            if (down && !ended && !climbing) StartCoroutine(ClimbStep());
        }

        void DropTop()
        {
            draggingTop = false;
            if (Vector2.Distance(topSign.position, climber.position + Vector3.up * 0.6f) < dropRange) { StartCoroutine(Summit()); return; }
            Sfx.Play(this, "whoosh", 0.4f);
            StartCoroutine(FloatBack());
        }

        IEnumerator FloatBack()
        {
            Vector3 from = topSign.position;
            for (float k = 0f; k < 1f && !draggingTop; k += Time.deltaTime / 0.4f)
            {
                topSign.position = Vector3.Lerp(from, topHome, Mathf.SmoothStep(0f, 1f, k));
                yield return null;
            }
        }

        /// <summary>Un gradino: la scala scorre in giù (l'omino resta al centro) e la cima... si allontana di uno.</summary>
        IEnumerator ClimbStep()
        {
            climbing = true;
            Vector3[] from = new Vector3[steps.Length];
            for (int i = 0; i < steps.Length; i++) from[i] = steps[i].localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / stepSeconds)
            {
                float e = Mathf.SmoothStep(0f, 1f, k);
                for (int i = 0; i < steps.Length; i++) steps[i].localPosition = from[i] - (Vector3)(stepDelta * e);
                climber.localPosition = climberHome + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.18f;
                yield return null;
            }
            climber.localPosition = climberHome;
            // i gradini usciti in basso ricompaiono in cima
            for (int i = 0; i < steps.Length; i++)
            {
                Vector3 p = from[i] - (Vector3)stepDelta;
                if (p.y < -6f) p += (Vector3)(stepDelta * steps.Length);
                steps[i].localPosition = p;
            }
            Sfx.Play(this, "tick", 0.5f);
            stamina--;
            climbedThisRun++;
            if (climbedThisRun % 2 == 0) stepsLeft += 1;   // più sale, più è lontana
            Refresh();
            if (stamina == 4) S41LKit.Say(bubble, "Wait... wasn't it 12 steps?");
            if (stamina <= 0) { StartCoroutine(Slide()); yield break; }
            climbing = false;
        }

        IEnumerator Slide()
        {
            busy = true;
            attempts++;
            Refresh();
            S41LKit.Say(bubble, attempts >= attemptsAllowed ? "I give up. Stairs win." : "Phew... too tired!");
            yield return new WaitForSeconds(0.7f);
            Sfx.Play(this, "whoosh", 0.8f);
            Haptics.Medium();
            // scivola giù fino alla base: la scala scorre all'indietro
            int back = climbedThisRun;
            Vector3[] from = new Vector3[steps.Length];
            for (int i = 0; i < steps.Length; i++) from[i] = steps[i].localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                float e = k * k;
                for (int i = 0; i < steps.Length; i++)
                {
                    Vector3 p = from[i] + (Vector3)(stepDelta * back * e);
                    float span = stepDelta.y * steps.Length;
                    while (p.y > -6f + span) p -= (Vector3)(stepDelta * steps.Length);
                    steps[i].localPosition = p;
                }
                climber.localRotation = Quaternion.Euler(0, 0, -25f * Mathf.Sin(k * Mathf.PI));
                yield return null;
            }
            climber.localRotation = Quaternion.identity;
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Heavy();
            if (attempts >= attemptsAllowed) { yield return new WaitForSeconds(0.8f); Fail(); yield break; }
            climbedThisRun = 0;
            stepsLeft = 12;
            stamina = stepsPerAttempt;
            Refresh();
            climbing = false;
            busy = false;
        }

        static IEnumerator HideAfter(System.Collections.Generic.List<Transform> list, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            foreach (Transform t in list) if (t != null) t.gameObject.SetActive(false);
        }

        IEnumerator Summit()
        {
            busy = true;
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Medium();
            stepsText.text = "STEPS TO THE TOP: 1";
            // la scala si comprime: i gradini sopra l'omino spariscono e la vetta arriva lì
            Vector3 top = climber.position + (Vector3)stepDelta;
            var vanishing = new System.Collections.Generic.List<Transform>();
            foreach (Transform s in steps)
                if (s.position.x > climber.position.x + 0.3f) { vanishing.Add(s); StartCoroutine(S41LKit.ScaleTo(s, new Vector3(1f, 0.01f, 1f), 0.4f)); }
            StartCoroutine(HideAfter(vanishing, 0.45f));   // schiacciati restava una riga d'inchiostro: poi spariscono del tutto
            summit.position = top;
            summit.gameObject.SetActive(true);
            Vector3 ss = summit.localScale;
            summit.localScale = ss * 0.2f;
            StartCoroutine(S41LKit.ScaleTo(summit, ss, 0.4f));
            Vector3 from = topSign.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                topSign.position = Vector3.Lerp(from, top + new Vector3(-0.1f, 1.6f, 0f), k);
                yield return null;
            }
            yield return new WaitForSeconds(0.3f);
            yield return StartCoroutine(S41LKit.Move(climber, climber.parent.InverseTransformPoint(top), 0.45f, 0.35f));
            Sfx.Play(this, "ding", 1f);
            stepsText.text = "STEPS TO THE TOP: 0";
            S41LKit.Say(bubble, "THE TOP! Finally!");
            S41LKit.Confetti(confetti, true);
            yield return StartCoroutine(S41LKit.Pop(climber, 0.25f, 0.4f));
            yield return new WaitForSeconds(1.8f);
            S41LKit.Confetti(confetti, false);
            Win();
        }
    }
}
