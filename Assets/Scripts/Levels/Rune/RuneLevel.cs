using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 66 — le rune magiche. La pergamena mostra una runa (cerchio, triangolo, quadrato, fulmine, stella o
    /// spunta): va ricopiata col dito, in un solo tratto, dentro il cerchio magico. Tre rune giuste aprono il forziere.
    /// Ogni runa sbagliata fa "pfff"; alla terza sbagliata l'incantesimo esplode e si riprova.
    /// </summary>
    public class RuneLevel : LevelController
    {
        [SerializeField] StrokeDrawer pen;
        [SerializeField] LineRenderer runeLine;       // la runa da copiare, disegnata sulla pergamena
        [SerializeField] Transform runeCenter;
        [SerializeField] float runeSize = 0.55f;
        [SerializeField] SpriteRenderer circle;
        [SerializeField] TextMesh status;
        [SerializeField] GameObject chestClosed, chestOpen;
        [SerializeField] SmokeEmitter sparkle, smoke;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int runesNeeded = 3;
        [SerializeField] int mistakesAllowed = 3;
        [SerializeField] float minScore = 0.75f;

        static readonly string[] Names = { "circle", "triangle", "square", "zigzag", "star", "check" };
        static List<Vector2>[] templates;

        readonly List<int> order = new List<int>();
        int current, solved, mistakes;
        bool done, busy;

        static List<Vector2>[] Templates()
        {
            if (templates != null) return templates;
            templates = new List<Vector2>[Names.Length];
            var circlePts = new List<Vector2>();
            for (int i = 0; i <= 24; i++) circlePts.Add(new Vector2(Mathf.Cos(i / 24f * Mathf.PI * 2f), Mathf.Sin(i / 24f * Mathf.PI * 2f)));
            templates[0] = circlePts;
            templates[1] = new List<Vector2> { new Vector2(0, 1), new Vector2(-0.87f, -0.5f), new Vector2(0.87f, -0.5f), new Vector2(0, 1) };
            templates[2] = new List<Vector2> { new Vector2(-1, 1), new Vector2(-1, -1), new Vector2(1, -1), new Vector2(1, 1), new Vector2(-1, 1) };
            templates[3] = new List<Vector2> { new Vector2(-1, 1), new Vector2(1, 0.35f), new Vector2(-1, -0.35f), new Vector2(1, -1) };
            var starPts = new List<Vector2>();
            for (int i = 0; i <= 10; i++) { float a = Mathf.PI / 2f + i * 4f * Mathf.PI / 5f; starPts.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a))); }
            templates[4] = starPts;
            templates[5] = new List<Vector2> { new Vector2(-1, 0.1f), new Vector2(-0.35f, -0.8f), new Vector2(1, 1) };
            return templates;
        }

        public override void OnLevelStart()
        {
            Templates();
            var pool = new List<int> { 0, 1, 2, 3, 4, 5 };
            for (int i = 0; i < runesNeeded && pool.Count > 0; i++) { int k = Random.Range(0, pool.Count); order.Add(pool[k]); pool.RemoveAt(k); }
            if (chestOpen != null) chestOpen.SetActive(false);
            if (sparkle != null) sparkle.Stop();
            if (smoke != null) smoke.Stop();
            pen.StrokeFinished += OnStroke;
            ShowRune();
            if (bubble != null) { bubble.Show("Copy the rune in ONE stroke!"); StartCoroutine(HideLater(2.2f)); }
        }

        void OnDestroy() { if (pen != null) pen.StrokeFinished -= OnStroke; }

        void ShowRune()
        {
            current = order[solved];
            List<Vector2> t = templates[current];
            runeLine.positionCount = t.Count;
            for (int i = 0; i < t.Count; i++) runeLine.SetPosition(i, runeCenter.position + (Vector3)(t[i] * runeSize));
            UpdateStatus();
        }

        void UpdateStatus()
        {
            if (status != null) status.text = "RUNE " + Mathf.Min(solved + 1, runesNeeded) + " / " + runesNeeded + "     MISTAKES LEFT: " + (mistakesAllowed - mistakes);
        }

        void OnStroke(StrokeDrawer d)
        {
            if (done || busy) return;
            var pts = new List<Vector2>();
            foreach (Vector3 p in d.LastStrokePoints) pts.Add(p);
            int best = -1;
            float bestScore = 0f;
            for (int i = 0; i < templates.Length; i++)
            {
                float s = ShapeRecognizer.Score(pts, templates[i]);
                if (s > bestScore) { bestScore = s; best = i; }
            }
            bool ok = best == current && bestScore >= minScore;
            StartCoroutine(ok ? Right() : Wrong());
        }

        IEnumerator Right()
        {
            busy = true;
            solved++;
            if (sparkle != null) { sparkle.Play(); }
            for (float k = 0f; k < 0.5f; k += Time.deltaTime)
            {
                if (circle != null) circle.color = Color.Lerp(new Color(1f, 0.95f, 0.5f), Color.white, k * 2f);
                yield return null;
            }
            if (sparkle != null) sparkle.Stop();
            pen.Clear();
            if (solved >= runesNeeded) { StartCoroutine(Open()); yield break; }
            if (bubble != null) { bubble.Show("Yes! Next rune..."); StartCoroutine(HideLater(1.2f)); }
            ShowRune();
            busy = false;
        }

        IEnumerator Wrong()
        {
            busy = true;
            mistakes++;
            UpdateStatus();
            if (smoke != null) smoke.Play();
            if (bubble != null) bubble.Show(mistakes >= mistakesAllowed ? "Uh oh..." : "Pfff! Wrong rune.");
            for (float k = 0f; k < 0.5f; k += Time.deltaTime)
            {
                if (circle != null) circle.color = Color.Lerp(new Color(1f, 0.4f, 0.4f), Color.white, k * 2f);
                yield return null;
            }
            if (smoke != null) smoke.Stop();
            pen.Clear();
            if (mistakes >= mistakesAllowed) { StartCoroutine(Kaboom()); yield break; }
            busy = false;
        }

        IEnumerator Open()
        {
            done = true;
            runeLine.positionCount = 0;
            if (status != null) status.text = "";
            if (chestClosed != null) chestClosed.SetActive(false);
            if (chestOpen != null) chestOpen.SetActive(true);
            if (sparkle != null) { sparkle.transform.position = chestOpen.transform.position + Vector3.up * 0.9f; sparkle.Play(); }
            if (bubble != null) bubble.Show("ABRACADABRA!");
            yield return new WaitForSeconds(1.6f);
            if (sparkle != null) sparkle.Stop();
            Win();
        }

        IEnumerator Kaboom()
        {
            done = true;
            if (smoke != null) { smoke.puffsPerSecond = 40f; smoke.startScale = 1.2f; smoke.Play(); }
            if (bubble != null) bubble.Show("KABOOM! *cough*");
            Transform t = transform;
            Vector3 home = t.localPosition;
            for (float k = 0f; k < 0.5f; k += Time.deltaTime) { t.localPosition = home + (Vector3)(Random.insideUnitCircle * 0.12f); yield return null; }
            t.localPosition = home;
            yield return new WaitForSeconds(0.8f);
            if (smoke != null) smoke.Stop();
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
