using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The missing second" — l'orologio digitale segna 11:59:59 ed è fermo; la porta del Cheese bar apre solo
    /// a mezzogiorno. Soluzione: scorri verso l'ALTO la cifra dei secondi come una rotella: +1 secondo = 12:00:00.
    /// Ogni cifra si può girare (su = avanti, giù = indietro, col suo valore di posizione); tre orari sbagliati e
    /// l'orologio torna a 11:59:59 (nessuna vita persa). Nessun legame con l'ora vera.
    /// </summary>
    public class TheMissingSecondLevel : LevelController
    {
        [SerializeField] TextMesh[] digits;        // H H M M S S
        [SerializeField] TextMesh[] ghosts;        // copie per l'animazione della rotella
        [SerializeField] Collider2D[] digitAreas;
        [SerializeField] Transform shutter, mouse, panel;
        [SerializeField] Collider2D doorArea;
        [SerializeField] TextMesh wrongText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float swipeDistance = 0.35f;

        const int StartTime = 11 * 3600 + 59 * 60 + 59, Noon = 12 * 3600, Day = 24 * 3600;
        static readonly int[] place = { 36000, 3600, 600, 60, 10, 1 };

        Camera cam;
        int time = StartTime, wrong, active = -1;
        float downY;
        bool swiped, busy;
        Color digitColor;
        Vector3[] digitHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            digitColor = digits[0].color;
            digitHome = new Vector3[digits.Length];
            for (int i = 0; i < digits.Length; i++) { digitHome[i] = digits[i].transform.localPosition; ghosts[i].text = ""; }
            S41EKit.Confetti(confetti, false);
            Show(time);
            UpdateWrong();
            S41EKit.Say(bubble, "Lunch at noon... the clock is STUCK!");
        }

        static string Text(int t)
        {
            int h = t / 3600, m = t / 60 % 60, s = t % 60;
            return h.ToString("00") + m.ToString("00") + s.ToString("00");
        }

        void Show(int t)
        {
            string s = Text(t);
            for (int i = 0; i < digits.Length; i++) digits[i].text = s[i].ToString();
        }

        void UpdateWrong() { if (wrongText != null) wrongText.text = "WRONG TIMES " + wrong + "/3"; }

        void Update()
        {
            if (IsFinished || busy) return;
            if (!S41EKit.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { active = -1; return; }
            if (began)
            {
                active = -1;
                swiped = false;
                for (int i = 0; i < digitAreas.Length; i++) if (digitAreas[i].OverlapPoint(w)) { active = i; downY = w.y; }
                if (active < 0 && doorArea != null && doorArea.OverlapPoint(w))
                {
                    Sfx.Play(this, "bonk", 0.6f);
                    StartCoroutine(S41EKit.Shake(shutter, 0.05f, 0.25f));
                    S41EKit.Say(bubble, "Locked until 12:00:00 sharp.");
                }
            }
            if (active < 0) return;
            float dy = w.y - downY;
            if (!swiped && Mathf.Abs(dy) > swipeDistance)
            {
                swiped = true;
                StartCoroutine(Roll(active, dy > 0f ? 1 : -1));
                active = -1;
                return;
            }
            if (ended)
            {
                if (!swiped)
                {
                    Sfx.Play(this, "tick", 0.5f);
                    StartCoroutine(S41EKit.Pop(digits[active].transform, 1.15f, 0.15f));
                    S41EKit.Say(bubble, "Tick? Tock? ...Nope. Still stuck.");
                }
                active = -1;
            }
        }

        IEnumerator Roll(int index, int dir)
        {
            busy = true;
            int before = time;
            time = ((time + dir * place[index]) % Day + Day) % Day;
            yield return AnimateTo(before, time, dir);
            if (time == Noon) { yield return Noontime(); yield break; }
            wrong++;
            UpdateWrong();
            Sfx.Play(this, "buzz", 0.6f);
            Haptics.Light();
            yield return Flash(new Color(1f, 0.35f, 0.35f));
            if (wrong >= 3)
            {
                S41EKit.Say(bubble, "Oops. The clock reset itself!");
                yield return new WaitForSeconds(0.5f);
                int from = time;
                time = StartTime;
                wrong = 0;
                yield return AnimateTo(from, time, -1);
                UpdateWrong();
            }
            else S41EKit.Say(bubble, wrong == 1 ? "That's not noon!" : "Hmm. Still not lunch time.");
            busy = false;
        }

        /// <summary>Le cifre cambiate scorrono come su una rotella.</summary>
        IEnumerator AnimateTo(int from, int to, int dir)
        {
            string a = Text(from), b = Text(to);
            Sfx.Play(this, "clunk", 0.5f);
            for (int i = 0; i < digits.Length; i++) { ghosts[i].text = a[i] != b[i] ? a[i].ToString() : ""; ghosts[i].color = digitColor; }
            for (int i = 0; i < digits.Length; i++) if (a[i] != b[i]) digits[i].text = b[i].ToString();
            const float travel = 0.45f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                for (int i = 0; i < digits.Length; i++)
                {
                    if (a[i] == b[i]) continue;
                    ghosts[i].transform.localPosition = digitHome[i] + Vector3.up * dir * travel * k;
                    S41EKit.SetAlpha(ghosts[i], 1f - k);
                    digits[i].transform.localPosition = digitHome[i] - Vector3.up * dir * travel * (1f - k);
                    S41EKit.SetAlpha(digits[i], k);
                }
                yield return null;
            }
            for (int i = 0; i < digits.Length; i++)
            {
                digits[i].transform.localPosition = digitHome[i];
                digits[i].color = digitColor;
                ghosts[i].text = "";
            }
        }

        IEnumerator Flash(Color c)
        {
            for (int n = 0; n < 2; n++)
            {
                foreach (TextMesh d in digits) d.color = c;
                yield return new WaitForSeconds(0.12f);
                foreach (TextMesh d in digits) d.color = digitColor;
                yield return new WaitForSeconds(0.1f);
            }
        }

        IEnumerator Noontime()
        {
            Sfx.Play(this, "ding", 0.9f);
            yield return Flash(new Color(1f, 0.95f, 0.4f));
            StartCoroutine(S41EKit.Pop(panel, 1.08f, 0.3f));
            S41EKit.Say(bubble, "IT'S NOON! LUNCH TIME!");
            Sfx.Play(this, "ding", 0.7f);
            // la saracinesca si arrotola verso l'alto
            Vector3 s = shutter.localScale;
            Sfx.Play(this, "whoosh", 0.7f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f) { shutter.localScale = new Vector3(s.x, s.y * (1f - k * 0.95f), 1f); yield return null; }
            shutter.localScale = new Vector3(s.x, s.y * 0.05f, 1f);
            // il topolino corre dentro
            yield return S41EKit.Move(mouse, mouse.localPosition + new Vector3(1.0f, 0f, 0f), 0.4f, 0.25f);
            yield return S41EKit.Move(mouse, mouse.localPosition + new Vector3(1.4f, 0.1f, 0f), 0.5f, 0.25f);
            S41EKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.6f);
            S41EKit.Confetti(confetti, false);
            Win();
        }
    }
}
