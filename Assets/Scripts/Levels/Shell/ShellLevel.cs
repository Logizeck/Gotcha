using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 27 — il gioco delle tre scatole. Il topolino si nasconde sotto la scatola di mezzo, poi le scatole
    /// si scambiano di posto sempre più veloci. Il topolino resta ben nascosto (si muove con la sua scatola) ma ogni
    /// 3-7 secondi gli sfugge la codina da sotto. Tocca la scatola giusta: se la sbagli, si riprova.
    /// </summary>
    public class ShellLevel : LevelController
    {
        [SerializeField] Transform[] boxes;
        [SerializeField] Transform mouse;
        [SerializeField] Transform tail;             // spunta da sotto la scatola che nasconde il topolino
        [SerializeField] Vector2 tailSpot = new Vector2(0.15f, 0f);
        [SerializeField] Vector2 tailEvery = new Vector2(3f, 7f);
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int swaps = 9;
        [SerializeField] float firstSwap = 0.55f;
        [SerializeField] float lastSwap = 0.26f;
        [SerializeField] float lift = 1.1f;

        int hiding;          // indice della scatola che nasconde il topolino
        bool picking, done;
        float tailSize = 1f;
        Camera cam;
        float baseY;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            baseY = boxes[0].localPosition.y;
            hiding = 1;
            if (hearts != null) hearts.Stop();
            if (tail != null) { tailSize = tail.localScale.y; tail.localScale = new Vector3(tailSize, 0f, 1f); }
            StartCoroutine(Play());
            StartCoroutine(TailPeeks());
        }

        IEnumerator Play()
        {
            // le scatole sono alzate: si vede il topolino sotto quella di mezzo
            foreach (Transform b in boxes) b.localPosition = new Vector3(b.localPosition.x, baseY + lift, 0f);
            PlaceMouse();
            if (bubble != null) bubble.Show("Find me!");
            yield return new WaitForSeconds(1.3f);
            yield return Lift(boxes, false);
            Haptics.Light();   // le scatole toccano il tavolo
            mouse.gameObject.SetActive(false);   // ora è nascosto davvero
            FollowBox();
            if (bubble != null) bubble.Hide();
            yield return new WaitForSeconds(0.4f);
            for (int i = 0; i < swaps; i++)
            {
                int a = Random.Range(0, 3), b = (a + Random.Range(1, 3)) % 3;
                float t = Mathf.Lerp(firstSwap, lastSwap, i / (float)(swaps - 1));
                yield return Swap(a, b, t);
            }
            if (bubble != null) bubble.Show("Where am I?");
            picking = true;
        }

        void PlaceMouse()
        {
            Transform b = boxes[hiding];
            mouse.localPosition = new Vector3(b.localPosition.x, baseY, mouse.localPosition.z);
        }

        /// <summary>Topolino e codina stanno sotto la scatola che lo nasconde e si muovono con lei.</summary>
        void FollowBox()
        {
            Transform b = boxes[hiding];
            mouse.SetParent(b, true);
            if (tail != null) { tail.SetParent(b, false); tail.localPosition = tailSpot; }
        }

        IEnumerator TailPeeks()
        {
            while (!done)
            {
                yield return new WaitForSeconds(Random.Range(tailEvery.x, tailEvery.y));
                // solo a mescolamento finito, mentre si sceglie: mai durante gli scambi né dopo la scoperta
                if (done || tail == null || !picking || mouse.gameObject.activeSelf) continue;
                // la codina scivola fuori, si agita un attimo e rientra
                for (float k = 0f; k < 1f && !done; k += Time.deltaTime / 0.25f) { tail.localScale = new Vector3(tailSize, k * tailSize, 1f); yield return null; }
                for (float k = 0f; k < 0.7f && !done; k += Time.deltaTime) { tail.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 25f) * 5f); yield return null; }
                tail.localRotation = Quaternion.identity;
                for (float k = 1f; k > 0f; k -= Time.deltaTime / 0.2f) { tail.localScale = new Vector3(tailSize, k * tailSize, 1f); yield return null; }
                tail.localScale = new Vector3(tailSize, 0f, 1f);
            }
        }

        IEnumerator Swap(int a, int b, float seconds)
        {
            Transform ta = boxes[a], tb = boxes[b];
            Vector3 pa = ta.localPosition, pb = tb.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                float e = Mathf.SmoothStep(0f, 1f, k), arc = Mathf.Sin(k * Mathf.PI) * 0.45f;
                ta.localPosition = Vector3.Lerp(pa, pb, e) + Vector3.up * arc;      // una passa sopra...
                tb.localPosition = Vector3.Lerp(pb, pa, e) - Vector3.up * arc * 0.5f;   // ...l'altra sotto
                yield return null;
            }
            ta.localPosition = pb;
            tb.localPosition = pa;
            // le scatole hanno cambiato posto: l'indice segue la scatola, non la posizione
            Transform tmp = boxes[a]; boxes[a] = boxes[b]; boxes[b] = tmp;
            if (hiding == a) hiding = b; else if (hiding == b) hiding = a;
            FollowBox();   // il topolino è figlio della sua scatola: si è spostato con lei
        }

        IEnumerator Lift(Transform[] which, bool up)
        {
            float from = up ? baseY : baseY + lift, to = up ? baseY + lift : baseY;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                foreach (Transform b in which) b.localPosition = new Vector3(b.localPosition.x, Mathf.Lerp(from, to, k), 0f);
                yield return null;
            }
            foreach (Transform b in which) b.localPosition = new Vector3(b.localPosition.x, to, 0f);
        }

        void Update()
        {
            if (!picking || done || IsFinished) return;
            if (!PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            for (int i = 0; i < boxes.Length; i++)
            {
                Collider2D c = boxes[i].GetComponent<Collider2D>();
                if (c != null && c.OverlapPoint(w)) { StartCoroutine(Reveal(i)); return; }
            }
        }

        IEnumerator Reveal(int i)
        {
            done = true;
            if (bubble != null) bubble.Hide();
            if (tail != null) tail.localScale = new Vector3(tailSize, 0f, 1f);
            mouse.SetParent(transform, true);   // la scatola si alza, il topolino resta a terra
            mouse.gameObject.SetActive(true);
            yield return Lift(new[] { boxes[i] }, true);
            if (i == hiding)
            {
                if (hearts != null) { hearts.transform.position = mouse.position + Vector3.up * 0.6f; hearts.Play(); }
                if (bubble != null) bubble.Show("Squeak! Found me!");
                yield return new WaitForSeconds(1.4f);
                if (hearts != null) hearts.Stop();
                Win();
            }
            else
            {
                if (bubble != null) bubble.Show("Nope!");
                yield return new WaitForSeconds(0.5f);
                yield return Lift(new[] { boxes[hiding] }, true);
                yield return new WaitForSeconds(0.9f);
                Fail();
            }
        }
    }
}
