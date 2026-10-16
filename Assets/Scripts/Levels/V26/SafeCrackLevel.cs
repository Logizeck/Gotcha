using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 87 — lo scassinatore. La combinazione non è scritta da nessuna parte: si gira la manopola col dito
    /// (orario, poi antiorario, poi orario, come nelle casseforti vere) e si ASCOLTA. Sul numero giusto, girando nel
    /// verso giusto, la serratura fa CLUNK (e il telefono vibra). Fermati lì e la lucetta si accende.
    /// Hai un minuto prima che passi la guardia.
    /// </summary>
    public class SafeCrackLevel : LevelController
    {
        [SerializeField] Transform dial;
        [SerializeField] Collider2D dialArea;
        [SerializeField] SpriteRenderer[] leds;
        [SerializeField] TextMesh readout;
        [SerializeField] TextMesh hint;
        [SerializeField] GameObject safeClosed, safeOpen;
        [SerializeField] Transform guardBar;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int numbers = 40;
        [SerializeField] float timeLimit = 60f;
        [SerializeField] float settle = 0.45f;

        int[] combo;
        int step, lastNumber = -1, lastDir;
        float angle, lastFingerAngle, still, clock;
        bool turning, done, clunked;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            combo = new int[3];
            for (int i = 0; i < 3; i++)
            {
                int n;
                do n = Random.Range(3, numbers); while (i > 0 && Mathf.Abs(n - combo[i - 1]) < 6);
                combo[i] = n;
            }
            foreach (SpriteRenderer l in leds) l.color = new Color(0.25f, 0.2f, 0.2f);
            safeOpen.SetActive(false);
            ShowHint();
            if (bubble != null) { bubble.Show("Listen for the CLUNK..."); StartCoroutine(HideLater(2.2f)); }
        }

        int Required => step % 2 == 0 ? -1 : 1;   // -1 = orario
        void ShowHint() { if (hint != null) hint.text = step < 3 ? (Required < 0 ? "TURN RIGHT  >" : "<  TURN LEFT") : ""; }
        int Number => ((Mathf.RoundToInt(-angle / (360f / numbers)) % numbers) + numbers) % numbers;

        void Update()
        {
            if (done || IsFinished) return;
            clock += Time.deltaTime;
            if (guardBar != null) guardBar.localScale = new Vector3(Mathf.Clamp01(1f - clock / timeLimit), 1f, 1f);
            if (clock >= timeLimit) { StartCoroutine(Caught()); return; }

            bool held = PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended);
            if (held)
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                float a = Mathf.Atan2(w.y - dial.position.y, w.x - dial.position.x) * Mathf.Rad2Deg;
                if (began && dialArea.OverlapPoint(w)) { turning = true; lastFingerAngle = a; }
                if (turning)
                {
                    float d = Mathf.DeltaAngle(lastFingerAngle, a);
                    lastFingerAngle = a;
                    if (Mathf.Abs(d) > 0.01f)
                    {
                        angle += d;
                        lastDir = d < 0f ? -1 : 1;
                        still = 0f;
                        dial.localRotation = Quaternion.Euler(0, 0, angle);
                        Turned();
                    }
                    else still += Time.deltaTime;
                }
                if (ended) turning = false;
            }
            else { turning = false; still += Time.deltaTime; }

            // fermo sul numero giusto dopo averci girato nel verso giusto: scatta il perno
            if (step < 3 && clunked && still > settle && Number == combo[step] && lastDir == Required)
            {
                leds[step].color = new Color(0.36f, 1f, 0.45f);
                Sfx.Play(this, "ding", 0.35f);
                step++;
                clunked = false;
                ShowHint();
                if (step >= 3) StartCoroutine(Open());
            }
        }

        void Turned()
        {
            int n = Number;
            if (readout != null) readout.text = n.ToString();
            if (n == lastNumber) return;
            lastNumber = n;
            if (step < 3 && n == combo[step] && lastDir == Required)
            {
                clunked = true;
                Sfx.Play(this, "clunk", 1f);
                Sfx.Buzz();
                StartCoroutine(Jolt());
            }
            else
            {
                if (n != combo[Mathf.Min(step, 2)]) clunked = false;
                Sfx.Play(this, "tick", 0.5f);
            }
        }

        IEnumerator Jolt()
        {
            Vector3 p = dial.localPosition;
            for (float k = 0f; k < 0.15f; k += Time.deltaTime) { dial.localPosition = p + (Vector3)Random.insideUnitCircle * 0.04f; yield return null; }
            dial.localPosition = p;
        }

        IEnumerator Open()
        {
            done = true;
            yield return new WaitForSeconds(0.4f);
            safeClosed.SetActive(false);
            safeOpen.SetActive(true);
            dial.gameObject.SetActive(false);
            if (bubble != null) bubble.Show("Smooth. Very smooth.");
            yield return new WaitForSeconds(1.5f);
            Win();
        }

        IEnumerator Caught()
        {
            done = true;
            if (bubble != null) bubble.Show("HEY! Hands where I can see them!");
            yield return new WaitForSeconds(1.6f);
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
