using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 96 — le bolle numerate. Scoppia le bolle in ordine da 1 a 8. Ma ognuna scrive il suo numero a modo
    /// suo: cifre, numeri romani, parole, somme... e due bolle girano su se stesse: un 6 capovolto è un 9 (e il 9
    /// non serve!). La lineetta sotto il numero dice dov'è il "sotto". L'8 sdraiato sembra un infinito.
    /// </summary>
    public class BubbleNumbersLevel : LevelController
    {
        [SerializeField] Transform[] bubbles;         // figlio "Label" (TextMesh) che può ruotare
        [SerializeField] int[] values;                // valore di ogni bolla (9 = esca)
        [SerializeField] float[] spin;                // gradi al secondo dell'etichetta
        [SerializeField] TextMesh nextLabel;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float rise = 0.45f, topY = 3.6f, bottomY = -4.2f;

        int next = 1;
        float[] seed;
        bool done;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            seed = new float[bubbles.Length];
            for (int i = 0; i < bubbles.Length; i++)
            {
                seed[i] = Random.Range(0f, 10f);
                bubbles[i].position = new Vector3(Random.Range(-2.2f, 2.2f), Mathf.Lerp(bottomY, topY, i / (float)bubbles.Length) + Random.Range(-0.3f, 0.3f), 0f);
            }
            Show();
            if (bubble != null) { bubble.Show("Pop them from 1 to 8!"); StartCoroutine(HideLater(1.6f)); }
        }

        void Show() { if (nextLabel != null) nextLabel.text = "NEXT: " + next; }

        void Update()
        {
            if (done || IsFinished) return;
            float dt = Time.deltaTime;
            for (int i = 0; i < bubbles.Length; i++)
            {
                Transform b = bubbles[i];
                if (!b.gameObject.activeSelf) continue;
                Vector3 p = b.position;
                p.y += rise * dt;
                p.x += Mathf.Sin(Time.time * 0.9f + seed[i]) * 0.4f * dt;
                if (p.y > topY) p.y = bottomY;
                b.position = p;
                if (spin[i] != 0f) b.Find("Label").Rotate(0, 0, spin[i] * dt);
            }
            if (!PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            for (int i = 0; i < bubbles.Length; i++)
            {
                if (!bubbles[i].gameObject.activeSelf || Vector2.Distance(w, bubbles[i].position) > 0.55f) continue;
                if (values[i] == next) { StartCoroutine(Pop(bubbles[i])); next++; Show(); if (next > 8) StartCoroutine(Done()); }
                else StartCoroutine(Wrong(bubbles[i], values[i]));
                return;
            }
        }

        IEnumerator Pop(Transform b)
        {
            Sfx.Play(this, "pop");
            Vector3 s = b.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.12f) { b.localScale = s * (1f + k * 0.4f); yield return null; }
            b.gameObject.SetActive(false);
        }

        IEnumerator Wrong(Transform b, int value)
        {
            done = true;
            Sfx.Play(this, "buzz", 1f);
            if (bubble != null) bubble.Show(value == 9 ? "That was a 9. There's no 9!" : "That was a " + value + "!");
            Vector3 p = b.position;
            for (float k = 0f; k < 0.5f; k += Time.deltaTime) { b.position = p + (Vector3)Random.insideUnitCircle * 0.05f; yield return null; }
            yield return new WaitForSeconds(1f);
            Fail();
        }

        IEnumerator Done()
        {
            done = true;
            if (bubble != null) bubble.Show("Counting is HARD. Well done!");
            yield return new WaitForSeconds(1.5f);
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
