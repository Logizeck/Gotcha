using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The hungry printer" — una stampante sputa solo fogli bianchi e chiede carta per stampare una mappa.
    /// Sulla mensola ci sono calzino, pesce, biscotto e mela: niente carta. Il trucco: il foglio "INSTRUCTIONS" appeso
    /// in alto è carta! Trascinalo nella fessura: la stampante lo usa e stampa la mappa. Tre oggetti sbagliati
    /// inseriti = inceppamento (sconfitta).
    /// </summary>
    public class HungryPrinterLevel : LevelController
    {
        [SerializeField] Transform printer, feeder, mapSheet;
        [SerializeField] Transform[] items;               // 0 = il foglio delle istruzioni
        [SerializeField] Collider2D[] itemAreas;
        [SerializeField] Transform[] blankPages;          // fogli bianchi che escono a ripetizione
        [SerializeField] TextMesh display, jamText;
        [SerializeField] SmokeEmitter smoke;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float feedDistance = 0.9f, pageEvery = 2.6f;

        static readonly string[] spits = { "BLEH! THAT IS NOT PAPER!", "YUCK! PAPER, PLEASE!", "I SAID PAPER!!" };

        Camera cam;
        Vector3[] homes;
        Vector3 mapHome;
        int dragging = -1, jams, nextPage;
        Vector2 grab;
        bool busy;
        float pageTimer;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            homes = new Vector3[items.Length];
            for (int i = 0; i < items.Length; i++) homes[i] = items[i].localPosition;
            mapHome = mapSheet.localPosition;
            mapSheet.gameObject.SetActive(false);
            foreach (Transform p in blankPages) p.gameObject.SetActive(false);
            if (smoke != null) smoke.Stop();
            S41EKit.Confetti(confetti, false);
            display.text = "NEED PAPER";
            UpdateJams();
            S41EKit.Say(bubble, "FEED ME PAPER! I MUST PRINT THE MAP!");
        }

        void UpdateJams() { if (jamText != null) jamText.text = "JAMS " + jams + "/3"; }

        void Update()
        {
            if (IsFinished) return;
            if (!busy)
            {
                pageTimer += Time.deltaTime;
                if (pageTimer > pageEvery) { pageTimer = 0f; StartCoroutine(BlankPage()); }
            }
            if (busy) return;
            if (!S41EKit.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { if (dragging >= 0) Drop(); return; }
            if (began)
            {
                for (int i = 0; i < items.Length; i++)
                    if (items[i].gameObject.activeSelf && itemAreas[i].OverlapPoint(w)) { dragging = i; grab = (Vector2)items[i].position - w; Sfx.Play(this, "tick", 0.5f); break; }
            }
            if (dragging < 0) return;
            Vector2 p = w + grab;
            items[dragging].position = new Vector3(Mathf.Clamp(p.x, -2.6f, 2.6f), Mathf.Clamp(p.y, -4.4f, 3.0f), items[dragging].position.z);
            items[dragging].localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 8f) * 5f);
            if (ended) Drop();
        }

        void Drop()
        {
            int i = dragging;
            dragging = -1;
            items[i].localRotation = Quaternion.identity;
            if (Vector2.Distance(items[i].position, feeder.position) < feedDistance) StartCoroutine(Feed(i));
            else StartCoroutine(S41EKit.Move(items[i], homes[i], 0.25f));
        }

        IEnumerator BlankPage()
        {
            Transform p = blankPages[nextPage];
            nextPage = (nextPage + 1) % blankPages.Length;
            p.gameObject.SetActive(true);
            Vector3 from = printer.localPosition + new Vector3(0f, 0.55f, 0f);
            Vector3 to = from + new Vector3(Random.Range(-2.2f, 2.2f), Random.Range(-1.6f, -1.1f), 0f);
            Sfx.Play(this, "whoosh", 0.25f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                p.localPosition = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.5f;
                p.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * 9f) * 25f);
                yield return null;
            }
            yield return new WaitForSeconds(1.5f);
            p.gameObject.SetActive(false);
        }

        IEnumerator Swallow(Transform t)
        {
            Vector3 s = t.localScale;
            Vector3 to = t.parent.InverseTransformPoint(feeder.position);
            yield return S41EKit.Move(t, to, 0.2f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                t.localScale = new Vector3(s.x, s.y * (1f - k), s.z);
                t.localPosition = to + Vector3.down * 0.4f * k;
                yield return null;
            }
            t.gameObject.SetActive(false);
            t.localScale = s;
        }

        IEnumerator Feed(int i)
        {
            busy = true;
            yield return Swallow(items[i]);
            Sfx.Play(this, "clunk", 0.7f);
            // la stampante mastica
            display.text = "CHECKING...";
            for (int n = 0; n < 3; n++) { yield return StartCoroutine(S41EKit.Pop(printer, 1.06f, 0.15f)); Sfx.Play(this, "tick", 0.6f); }
            if (i == 0) { yield return PrintMap(); yield break; }

            jams++;
            UpdateJams();
            Haptics.Medium();
            Sfx.Play(this, "buzz", 0.7f);
            if (jams >= 3)
            {
                display.text = "PAPER JAM!!!";
                S41EKit.Say(bubble, "PAPER JAAAM! *KRRZZT*");
                if (smoke != null) smoke.Play();
                Haptics.Heavy();
                yield return StartCoroutine(S41EKit.Shake(printer, 0.15f, 1.0f));
                yield return new WaitForSeconds(0.6f);
                Fail();
                yield break;
            }
            display.text = "NOT PAPER!";
            S41EKit.Say(bubble, spits[(jams - 1) % spits.Length]);
            // sputa fuori l'oggetto
            Transform t = items[i];
            t.gameObject.SetActive(true);
            t.position = feeder.position;
            yield return S41EKit.Move(t, homes[i], 0.45f, 1.0f);
            yield return StartCoroutine(S41EKit.Shake(printer, 0.08f, 0.3f));
            display.text = "NEED PAPER";
            busy = false;
        }

        IEnumerator PrintMap()
        {
            display.text = "PRINTING MAP";
            S41EKit.Say(bubble, "MMM, INSTRUCTIONS! DELICIOUS PAPER!");
            for (int n = 0; n < 6; n++) { Sfx.Play(this, "tick", 0.7f); yield return StartCoroutine(S41EKit.Shake(printer, 0.04f, 0.12f)); }
            mapSheet.gameObject.SetActive(true);
            Vector3 big = mapSheet.localScale;
            Vector3 mouth = printer.localPosition + new Vector3(0f, 0.55f, 0f);
            mapSheet.localPosition = mouth;
            mapSheet.localScale = big * 0.5f;
            yield return S41EKit.Move(mapSheet, mouth + Vector3.down * 0.6f, 0.4f);
            Sfx.Play(this, "whoosh", 0.7f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                mapSheet.localPosition = Vector3.Lerp(mouth + Vector3.down * 0.6f, mapHome, k);
                mapSheet.localScale = Vector3.Lerp(big * 0.5f, big, k);
                yield return null;
            }
            mapSheet.localScale = big;
            mapSheet.localPosition = mapHome;
            display.text = "DONE :)";
            Sfx.Play(this, "ding", 0.9f);
            S41EKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.8f);
            S41EKit.Confetti(confetti, false);
            Win();
        }
    }
}
