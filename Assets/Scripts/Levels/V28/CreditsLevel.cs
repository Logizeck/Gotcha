using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 100 — THE END? Partono i titoli di coda... ma il topolino non vuole che il gioco finisca: si nasconde
    /// dietro i nomi e sbuca per un attimo. Prendilo 3 volte prima che i titoli finiscano. Easter egg `credits_skip`:
    /// premere SKIP. Non si saltano i titoli di coda del TUO gioco.
    /// </summary>
    public class CreditsLevel : LevelController
    {
        [SerializeField] Transform roll;              // contenitore delle righe, sale
        [SerializeField] TextMesh[] lines;
        [SerializeField] Transform mouse;             // sbuca dietro una riga
        [SerializeField] TapButton skip;
        [SerializeField] GameObject finale;           // "THE REAL END" + trofeo
        [SerializeField] SmokeEmitter confetti;
        [SerializeField] TextMesh counter;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float speed = 0.55f, endY = 14f;
        [SerializeField] int needed = 3;
        [SerializeField] string eggId = "credits_skip";

        int caught;
        float peek, nextPeek = 2f;
        bool done, showing;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            finale.SetActive(false);
            mouse.gameObject.SetActive(false);
            if (confetti != null) confetti.Stop();
            skip.Pressed += _ => { if (!done) StartCoroutine(Skipped()); };
            Count();
        }

        void Count() { counter.text = caught + " / " + needed; }

        void Update()
        {
            if (done || IsFinished) return;
            roll.position += Vector3.up * speed * Time.deltaTime;
            if (roll.position.y > endY) { StartCoroutine(Missed()); return; }

            if (showing)
            {
                peek -= Time.deltaTime;
                if (peek <= 0f) { showing = false; mouse.gameObject.SetActive(false); nextPeek = Random.Range(1.2f, 2.5f); }
            }
            else
            {
                nextPeek -= Time.deltaTime;
                if (nextPeek <= 0f) Peek();
            }
            if (!PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (showing && Vector2.Distance(w, mouse.position) < 0.55f) StartCoroutine(Caught());
        }

        void Peek()
        {
            // sbuca dall'estremità di una riga visibile
            for (int tries = 0; tries < 10; tries++)
            {
                TextMesh l = lines[Random.Range(0, lines.Length)];
                Vector3 p = l.transform.position;
                if (p.y < -3f || p.y > 3f || string.IsNullOrEmpty(l.text)) continue;
                float half = l.GetComponent<MeshRenderer>().bounds.extents.x;
                float side = Random.value < 0.5f ? -1f : 1f;
                mouse.SetParent(l.transform, true);
                mouse.position = new Vector3(p.x + side * (half + 0.25f), p.y + 0.05f, 0f);
                mouse.localScale = new Vector3(Mathf.Abs(mouse.localScale.x) * -side, mouse.localScale.y, 1f);
                mouse.gameObject.SetActive(true);
                showing = true;
                peek = Mathf.Lerp(0.9f, 0.6f, caught / (float)needed);
                return;
            }
        }

        IEnumerator Caught()
        {
            showing = false;
            caught++;
            Count();
            Sfx.Play(this, "pop", 0.8f);
            if (bubble != null) { bubble.Show(caught < needed ? "Eek! You saw me!" : "OK OK, the game can end..."); }
            mouse.gameObject.SetActive(false);
            nextPeek = Random.Range(1.2f, 2.2f);
            if (caught >= needed)
            {
                done = true;
                yield return new WaitForSeconds(0.8f);
                finale.SetActive(true);
                if (confetti != null) confetti.Play();
                Sfx.Play(this, "ding", 1f);
                if (bubble != null) bubble.Show("THANK YOU FOR PLAYING!");
                yield return new WaitForSeconds(3f);
                Win();
            }
        }

        IEnumerator Missed()
        {
            done = true;
            if (bubble != null) bubble.Show("The mouse is still hiding... play again?");
            yield return new WaitForSeconds(1.8f);
            Fail();
        }

        /// <summary>Easter egg: hai provato a saltare i titoli di coda.</summary>
        IEnumerator Skipped()
        {
            done = true;
            if (bubble != null) bubble.Show("SKIP?! We worked SO hard on these names!");
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f) { roll.position -= Vector3.up * 12f * Time.deltaTime; yield return null; }   // si riavvolgono, offesi
            yield return new WaitForSeconds(1.2f);
            FailWithEgg(eggId);
        }
    }
}
