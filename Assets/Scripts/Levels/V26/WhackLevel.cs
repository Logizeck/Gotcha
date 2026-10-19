using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 99 — acchiappa la talpa. Dalle buche escono talpe arrabbiate (colpiscile!), talpe con il cappellino
    /// da festa (sono innocenti: NON colpirle) e ogni tanto la talpa d'oro, che vale tre. 12 punti in 25 secondi.
    /// Easter egg `cat_bonk`: ogni tanto da una buca spunta il gatto. Che ci fa lì?!
    /// </summary>
    public class WhackLevel : LevelController
    {
        [SerializeField] Transform[] holes;           // figli "Pop" (che sale) con dentro "Angry", "Party", "Gold", "Cat"
        [SerializeField] TextMesh counter;
        [SerializeField] Transform timeBar;
        [SerializeField] Transform hammer;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int needed = 12;
        [SerializeField] float seconds = 25f;
        [SerializeField] string eggId = "cat_bonk";

        static readonly string[] kinds = { "Angry", "Party", "Gold", "Cat" };
        int[] kind;           // -1 = vuota
        float[] up;           // tempo rimasto fuori
        float clock, spawn = 0.8f;
        int score;
        bool done, catShown;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            kind = new int[holes.Length];
            up = new float[holes.Length];
            for (int i = 0; i < holes.Length; i++) { kind[i] = -1; Set(i, -1, 0f); }
            hammer.gameObject.SetActive(false);
            Count();
            if (bubble != null) { bubble.Show("Bonk the ANGRY ones. Party moles are friends!"); StartCoroutine(HideLater(2.4f)); }
        }

        void Count() { counter.text = score + " / " + needed; }

        void Set(int i, int k, float height)
        {
            Transform pop = holes[i].Find("Pop");
            for (int j = 0; j < kinds.Length; j++) pop.Find(kinds[j]).gameObject.SetActive(j == k);
            pop.localPosition = new Vector3(0f, Mathf.Lerp(-0.9f, 0f, height), 0f);
        }

        void Update()
        {
            if (done || IsFinished) return;
            float dt = Time.deltaTime;
            clock += dt;
            if (timeBar != null) timeBar.localScale = new Vector3(Mathf.Clamp01(1f - clock / seconds), 1f, 1f);
            if (clock >= seconds) { StartCoroutine(TimeUp()); return; }

            spawn -= dt;
            if (spawn <= 0f)
            {
                spawn = Mathf.Lerp(0.75f, 0.4f, clock / seconds);
                int i = Random.Range(0, holes.Length);
                if (kind[i] < 0)
                {
                    float r = Random.value;
                    int k = r < 0.55f ? 0 : r < 0.88f ? 1 : r < 0.95f ? 2 : 3;
                    if (k == 3 && catShown) k = 1;
                    if (k == 3) catShown = true;
                    kind[i] = k;
                    up[i] = Mathf.Lerp(1.1f, 0.7f, clock / seconds) + (k == 1 ? 0.4f : 0f);
                }
            }
            for (int i = 0; i < holes.Length; i++)
            {
                if (kind[i] < 0) continue;
                up[i] -= dt;
                float h = Mathf.Clamp01(Mathf.Min(up[i] * 6f, 1f));
                Set(i, kind[i], h);
                if (up[i] <= 0f) { kind[i] = -1; Set(i, -1, 0f); }
            }

            if (!PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            StartCoroutine(Swing(w));
            for (int i = 0; i < holes.Length; i++)
            {
                if (kind[i] < 0 || up[i] < 0.08f || Vector2.Distance(w, holes[i].position + Vector3.up * 0.45f) > 0.65f) continue;
                int k = kind[i];
                kind[i] = -1;
                Set(i, -1, 0f);
                if (k == 1) { StartCoroutine(Oops("HEY! It's my BIRTHDAY!", false)); return; }
                if (k == 3) { StartCoroutine(Oops("...the cat will remember this.", true)); return; }
                Sfx.Play(this, "bonk");
                Haptics.Light();
                score += k == 2 ? 3 : 1;
                Count();
                if (score >= needed) StartCoroutine(Cleared());
                return;
            }
        }

        IEnumerator Swing(Vector2 at)
        {
            hammer.gameObject.SetActive(true);
            hammer.position = at + new Vector2(0.3f, -0.65f);   // la testa del martello cade sul dito
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f) { hammer.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(30f, -50f, k)); yield return null; }
            yield return new WaitForSeconds(0.05f);
            hammer.gameObject.SetActive(false);
        }

        IEnumerator Oops(string message, bool egg)
        {
            done = true;
            Sfx.Play(this, "bonk", 1f);
            Haptics.Medium();
            if (bubble != null) bubble.Show(message);
            yield return new WaitForSeconds(1.8f);
            if (egg) FailWithEgg(eggId); else Fail();
        }

        IEnumerator Cleared()
        {
            done = true;
            if (bubble != null) bubble.Show("The garden is safe!");
            yield return new WaitForSeconds(1.4f);
            Win();
        }

        IEnumerator TimeUp()
        {
            done = true;
            if (bubble != null) bubble.Show("Time! The moles win this round.");
            yield return new WaitForSeconds(1.4f);
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
