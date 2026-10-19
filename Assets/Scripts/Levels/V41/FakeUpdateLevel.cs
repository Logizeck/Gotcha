using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Fake update" — una finta (e molto da cartone) schermata di aggiornamento bloccata al 99%. Soluzione:
    /// trascinare il "99" verso l'alto: sotto c'è un piccolo "1"; portarlo davanti ai due 9, che si ricompongono in
    /// "100" → aggiornamento finito. Nessuna vita persa: dopo 30 s senza progressi compare un finto "UPDATE FAILED"
    /// che invita a usare un suggerimento, poi si torna alla schermata.
    /// </summary>
    public class FakeUpdateLevel : LevelController
    {
        [SerializeField] Transform nines;                // "99" con la sua toppa (si trascina)
        [SerializeField] Collider2D ninesArea;
        [SerializeField] TextMesh ninesText;
        [SerializeField] Transform one;                  // il piccolo "1" nascosto sotto
        [SerializeField] Collider2D oneArea;
        [SerializeField] TextMesh hundred;               // "100" finale (nascosto)
        [SerializeField] TextMesh percent;
        [SerializeField] Transform barFill;              // pivot a sinistra
        [SerializeField] TextMesh status;
        [SerializeField] Collider2D speedButton;
        [SerializeField] Transform gear;
        [SerializeField] GameObject failedScreen;
        [SerializeField] Collider2D okButton;
        [SerializeField] SpriteRenderer[] confetti;
        [SerializeField] float idleSeconds = 30f, snapRadius = 0.6f, oneGap = 0.85f, oneGrow = 1.7f;

        static readonly string[] Messages = { "Installing important stuff...", "Almost there...", "Seriously, almost...", "Polishing pixels...", "Teaching the 9s to count...", "Do not turn off your brain..." };

        Camera cam;
        Vector3 ninesHome, oneHome;
        Transform dragged;
        Vector2 grab;
        bool oneMoved, ninesMoved, busy, failedShown;
        float idle, clock, msgT;
        int msg;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            ninesHome = nines.position;
            oneHome = one.position;
            hundred.gameObject.SetActive(false);
            failedScreen.SetActive(false);
            foreach (SpriteRenderer c in confetti) c.enabled = false;
            SetBar(0.99f);
            status.text = Messages[0];
        }

        void SetBar(float v) { barFill.localScale = new Vector3(v, 1f, 1f); }

        void Update()
        {
            if (IsFinished) return;
            float dt = Time.deltaTime;
            clock += dt;
            if (gear != null && !busy) gear.localRotation = Quaternion.Euler(0f, 0f, -clock * 40f);
            if (busy) return;

            if (failedShown)
            {
                if (S41AKit.Pointer(cam, out Vector2 fw, out bool fb, out _) && fb && okButton.OverlapPoint(fw))
                {
                    Sfx.Play(this, "pop", 0.6f);
                    failedScreen.SetActive(false);
                    failedShown = false;
                    idle = 0f;
                }
                return;
            }

            idle += dt;
            msgT += dt;
            if (msgT > 2.6f) { msgT = 0f; msg = (msg + 1) % Messages.Length; status.text = Messages[msg]; }
            if (idle > idleSeconds && dragged == null) { ShowFailed(); return; }

            if (!S41AKit.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { dragged = null; return; }
            if (began)
            {
                bool oneVisible = Vector2.Distance(nines.position, oneHome) > 0.5f || Vector2.Distance(one.position, oneHome) > 0.05f;
                if (oneVisible && oneArea.OverlapPoint(w)) dragged = one;
                else if (ninesArea.OverlapPoint(w)) dragged = nines;
                else if (speedButton.OverlapPoint(w)) { Sfx.Play(this, "tick", 0.6f); status.text = "Speeding up... 99%. Nope."; msgT = -1f; StartCoroutine(S41AKit.Bounce(speedButton.transform, 0.12f, 0.2f)); }
                if (dragged != null) { grab = (Vector2)dragged.position - w; Sfx.Play(this, "tick", 0.5f); }
                return;
            }
            if (dragged == null) return;
            Vector2 to = w + grab;
            to.x = Mathf.Clamp(to.x, -2.4f, 2.4f);
            to.y = Mathf.Clamp(to.y, -3.2f, 3.4f);
            dragged.position = new Vector3(to.x, to.y, dragged.position.z);
            if (dragged == nines && !ninesMoved && Vector2.Distance(to, ninesHome) > 0.6f) { ninesMoved = true; idle = 0f; Sfx.Play(this, "pop", 0.5f); status.text = "Hey! Put that back!"; msgT = -1f; }
            if (dragged == one && !oneMoved && Vector2.Distance(to, oneHome) > 0.3f) { oneMoved = true; idle = 0f; }
            if (!ended) return;
            Transform d = dragged;
            dragged = null;
            if (d == nines && Vector2.Distance(nines.position, ninesHome) < 0.4f && Vector2.Distance(one.position, oneHome) < 0.05f) { StartCoroutine(S41AKit.Move(nines, ninesHome, 0.1f)); return; }
            Vector3 slot = nines.position + new Vector3(-oneGap * nines.localScale.x, 0f, 0f);
            if (Vector2.Distance(one.position, slot) < snapRadius && Vector2.Distance(one.position, oneHome) > 0.3f) StartCoroutine(Complete());
        }

        void ShowFailed()
        {
            failedShown = true;
            failedScreen.SetActive(true);
            Sfx.Play(this, "buzz", 0.6f);
            Haptics.Light();
            StartCoroutine(S41AKit.Bounce(failedScreen.transform, 0.08f, 0.25f));
        }

        IEnumerator Complete()
        {
            busy = true;
            idle = 0f;
            Vector3 slot = nines.position + new Vector3(-oneGap * nines.localScale.x, 0f, 0f);
            yield return S41AKit.Move(one, slot, 0.12f);
            Vector3 os = one.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { one.localScale = os * Mathf.Lerp(1f, oneGrow, k); yield return null; }
            one.localScale = os * oneGrow;
            Sfx.Play(this, "clunk", 0.7f);
            // i due 9 si "srotolano" in due 0
            Vector3 s = nines.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { nines.localScale = new Vector3(s.x, s.y * (1f - k), 1f); yield return null; }
            ninesText.text = "00";
            Sfx.Play(this, "pop", 0.8f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { nines.localScale = new Vector3(s.x, s.y * k, 1f); yield return null; }
            nines.localScale = s;
            yield return new WaitForSeconds(0.3f);
            // tutti al loro posto: "100%"
            Vector3 off = one.position - nines.position;
            Vector3 from = nines.position;
            Vector3 center = ninesHome + new Vector3(oneGap * 0.5f, 0f, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                nines.position = Vector3.Lerp(from, center, Mathf.SmoothStep(0f, 1f, k));
                one.position = nines.position + off;
                yield return null;
            }
            nines.gameObject.SetActive(false);
            one.gameObject.SetActive(false);
            hundred.gameObject.SetActive(true);
            StartCoroutine(S41AKit.Bounce(hundred.transform, 0.3f, 0.3f));
            if (percent != null) StartCoroutine(S41AKit.Move(percent.transform, percent.transform.position + new Vector3(oneGap * 0.45f, 0f, 0f), 0.2f));
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { SetBar(Mathf.Lerp(0.99f, 1f, k)); yield return null; }
            SetBar(1f);
            status.text = "UPDATE COMPLETE! (finally)";
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Light();
            foreach (SpriteRenderer c in confetti)
            {
                c.enabled = true;
                StartCoroutine(Fall(c.transform));
            }
            if (gear != null) yield return S41AKit.Bounce(gear, 0.3f, 0.35f);
            yield return new WaitForSeconds(1.4f);
            Win();
        }

        IEnumerator Fall(Transform c)
        {
            Vector3 p = c.position;
            float sp = Random.Range(1.2f, 2.4f), sw = Random.Range(0f, 6f);
            for (float t = 0f; t < 2.5f; t += Time.deltaTime)
            {
                c.position = p + new Vector3(Mathf.Sin(t * 3f + sw) * 0.2f, -t * sp, 0f);
                c.localRotation = Quaternion.Euler(0f, 0f, t * 360f + sw * 50f);
                yield return null;
            }
        }
    }
}
