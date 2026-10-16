using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 80 — conta le pecore. Di notte le pecore saltano lo steccato: chi va a destra entra nel recinto, ma
    /// qualcuna salta indietro, qualcuna arriva fino allo steccato e ci ripensa, a volte ne passano due insieme.
    /// Alla fine: quante pecore ci sono nel recinto (a destra)? Rispondi con la tastiera (due cifre).
    /// Easter egg `fell_asleep`: se alla domanda non rispondi per un po'... contare le pecore fa dormire.
    /// </summary>
    public class SheepLevel : LevelController
    {
        [SerializeField] Transform sheepTemplate;
        [SerializeField] Transform fence;
        [SerializeField] GameObject keypad;
        [SerializeField] TapButton[] keys;
        [SerializeField] TextMesh display;
        [SerializeField] TextMesh question;
        [SerializeField] GameObject sleepOverlay;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float groundY = -2.6f, edgeX = 3.5f;
        [SerializeField] int events = 14;
        [SerializeField] float asleepAfter = 18f;
        [SerializeField] string eggId = "fell_asleep";

        int inPen;
        string typed = "";
        bool asking, done, busy;
        float idle;

        public override void OnLevelStart()
        {
            keypad.SetActive(false);
            question.gameObject.SetActive(false);
            sleepOverlay.SetActive(false);
            foreach (TapButton k in keys) k.Pressed += OnKey;
            if (bubble != null) { bubble.Show("Count the sheep in the pen (right side)!"); StartCoroutine(HideLater(2.4f)); }
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            yield return new WaitForSeconds(2.2f);
            for (int e = 0; e < events; e++)
            {
                float r = Random.value;
                bool fromLeft = inPen == 0 || r < 0.68f;
                if (r > 0.9f) { StartCoroutine(Jump(fromLeft, true)); }                  // ci ripensa
                else if (r > 0.8f && fromLeft) { StartCoroutine(Jump(true, false)); yield return new WaitForSeconds(0.25f); StartCoroutine(Jump(true, false)); }   // due insieme
                else StartCoroutine(Jump(fromLeft, false));
                yield return new WaitForSeconds(Random.Range(0.7f, 1.6f));
            }
            yield return new WaitForSeconds(2.2f);
            Ask();
        }

        IEnumerator Jump(bool fromLeft, bool balk)
        {
            if (!balk) inPen += fromLeft ? 1 : -1;   // si conta subito: il risultato è deciso dal salto
            Transform s = Instantiate(sheepTemplate, transform);
            s.gameObject.SetActive(true);
            float dir = fromLeft ? 1f : -1f;
            s.localScale = new Vector3(Mathf.Abs(s.localScale.x) * dir, s.localScale.y, 1f);
            float speed = Random.Range(2.6f, 3.6f);
            float x = -edgeX * dir, fx = fence.position.x;
            while ((fx - x) * dir > 0.9f)
            {
                x += dir * speed * Time.deltaTime;
                s.position = new Vector3(x, groundY + Mathf.Abs(Mathf.Sin(x * 6f)) * 0.06f, 0f);
                yield return null;
            }
            if (balk)
            {
                yield return new WaitForSeconds(0.35f);
                s.localScale = new Vector3(-s.localScale.x, s.localScale.y, 1f);   // si gira e torna indietro
                while (Mathf.Abs(x) < edgeX + 0.5f) { x -= dir * speed * Time.deltaTime; s.position = new Vector3(x, groundY, 0f); yield return null; }
                Destroy(s.gameObject);
                yield break;
            }
            float from = x, to = fx + dir * 0.9f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.55f)
            {
                s.position = new Vector3(Mathf.Lerp(from, to, k), groundY + Mathf.Sin(k * Mathf.PI) * 1.3f, 0f);
                s.localRotation = Quaternion.Euler(0, 0, -dir * Mathf.Lerp(15f, -15f, k));
                yield return null;
            }
            s.localRotation = Quaternion.identity;
            x = to;
            while (Mathf.Abs(x) < edgeX + 0.5f) { x += dir * speed * Time.deltaTime; s.position = new Vector3(x, groundY, 0f); yield return null; }
            Destroy(s.gameObject);
        }

        void Ask()
        {
            asking = true;
            question.gameObject.SetActive(true);
            keypad.SetActive(true);
            typed = "";
            Show();
        }

        void Show() { display.text = string.Join(" ", typed.PadRight(2, '_').ToCharArray()); }

        void OnKey(TapButton k)
        {
            if (!asking || done || busy) return;
            idle = 0f;
            typed += k.id;
            Show();
            if (typed.Length >= 2) StartCoroutine(Check());
        }

        void Update()
        {
            if (!asking || done || IsFinished) return;
            idle += Time.deltaTime;
            if (idle > asleepAfter) StartCoroutine(Asleep());
        }

        IEnumerator Check()
        {
            busy = true;
            done = true;
            yield return new WaitForSeconds(0.3f);
            int n = int.Parse(typed);
            keypad.SetActive(false);
            if (n == inPen)
            {
                if (bubble != null) bubble.Show(inPen + " sheep. Baaa-rilliant!");
                yield return new WaitForSeconds(1.5f);
                Win();
            }
            else
            {
                if (bubble != null) bubble.Show("Baaa! There were " + inPen + ".");
                yield return new WaitForSeconds(1.8f);
                Fail();
            }
        }

        /// <summary>Easter egg: a forza di contare pecore ti sei addormentato.</summary>
        IEnumerator Asleep()
        {
            done = true;
            keypad.SetActive(false);
            sleepOverlay.SetActive(true);
            if (bubble != null) bubble.Show("Zzz... it works every time.");
            yield return new WaitForSeconds(2.2f);
            FailWithEgg(eggId);
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
