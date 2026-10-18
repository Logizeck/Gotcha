using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 68 — le rune magiche. La pergamena mostra una runa (13 possibili: cerchio, triangolo, quadrato, fulmine,
    /// stella, spunta, spirale, cuore, infinito, onda, rombo, luna, montagne; 3 a caso per partita): va ricopiata col
    /// dito, in un solo tratto, dentro il cerchio magico. Conta la forma, non da dove si parte né il verso. Tre rune giuste aprono il forziere.
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

        [SerializeField] float maxDistance = 1.6f;    // distanza massima dal modello (PointCloudRecognizer)
        [SerializeField] float vsBest = 1.3f;          // e non più di così peggio della forma più vicina

        // 13 rune: ogni partita ne pesca 3 diverse. Il primo modello è quello disegnato sulla pergamena,
        // gli altri sono varianti accettate (stella col contorno, quadrato un po' rettangolare).
        static readonly string[] Names = { "circle", "triangle", "square", "zigzag", "star", "check", "spiral", "heart", "infinity", "wave", "diamond", "moon", "mountains" };
        static List<Vector2>[][] shapes;
        static Vector2[][][] prepared;

        readonly List<int> order = new List<int>();
        int current, solved, mistakes;
        bool done, busy;

        static List<Vector2> Arc(int k, float r, Vector2 c, float a0, float a1)
        {
            var o = new List<Vector2>();
            for (int i = 0; i <= k; i++) { float a = a0 + (a1 - a0) * i / k; o.Add(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r); }
            return o;
        }

        static List<Vector2> Pts(params float[] xy)
        {
            var o = new List<Vector2>();
            for (int i = 0; i + 1 < xy.Length; i += 2) o.Add(new Vector2(xy[i], xy[i + 1]));
            return o;
        }

        static void Build()
        {
            if (shapes != null) return;
            var star = new List<Vector2>();
            for (int i = 0; i <= 10; i++) { float a = Mathf.PI / 2f + i * 4f * Mathf.PI / 5f; star.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a))); }
            var starOutline = new List<Vector2>();
            for (int i = 0; i <= 10; i++) { float a = Mathf.PI / 2f + i * Mathf.PI / 5f, r = i % 2 == 0 ? 1f : 0.42f; starOutline.Add(new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r)); }
            var spiral = new List<Vector2>();
            for (int i = 0; i <= 60; i++) { float t = i / 60f, r = 0.1f + 0.9f * t; spiral.Add(new Vector2(Mathf.Cos(t * 4f * Mathf.PI), Mathf.Sin(t * 4f * Mathf.PI)) * r); }
            var heart = new List<Vector2>();
            for (int i = 0; i <= 40; i++)
            {
                float t = i / 40f * Mathf.PI * 2f, sn = Mathf.Sin(t);
                heart.Add(new Vector2(sn * sn * sn, (13f * Mathf.Cos(t) - 5f * Mathf.Cos(2f * t) - 2f * Mathf.Cos(3f * t) - Mathf.Cos(4f * t)) / 16f));
            }
            var infinity = new List<Vector2>();
            for (int i = 0; i <= 48; i++) { float t = i / 48f * Mathf.PI * 2f, d = 1f + Mathf.Sin(t) * Mathf.Sin(t); infinity.Add(new Vector2(Mathf.Cos(t) / d, Mathf.Sin(t) * Mathf.Cos(t) / d)); }
            var wave = new List<Vector2>();
            for (int i = 0; i <= 40; i++) wave.Add(new Vector2(-1f + 2f * i / 40f, 0.45f * Mathf.Sin(i / 40f * 3f * Mathf.PI)));
            // falce di luna: arco esterno grande, poi arco interno più stretto per tornare alla punta
            List<Vector2> moon = Arc(20, 1f, Vector2.zero, 50f * Mathf.Deg2Rad, 310f * Mathf.Deg2Rad);
            Vector2 tip = moon[0];
            for (int i = 1; i < 16; i++) { float a = (-80f + 160f * i / 16f) * Mathf.Deg2Rad; moon.Add(new Vector2(0.55f * Mathf.Cos(a) - 0.05f, 0.77f * Mathf.Sin(a))); }
            moon.Add(tip);
            shapes = new[]
            {
                new[] { Arc(32, 1f, Vector2.zero, 0f, Mathf.PI * 2f) },
                new[] { Pts(0, 1, -0.87f, -0.5f, 0.87f, -0.5f, 0, 1) },
                new[] { Pts(-1, 1, -1, -1, 1, -1, 1, 1, -1, 1), Pts(-1.3f, 0.8f, -1.3f, -0.8f, 1.3f, -0.8f, 1.3f, 0.8f, -1.3f, 0.8f), Pts(-0.8f, 1.3f, -0.8f, -1.3f, 0.8f, -1.3f, 0.8f, 1.3f, -0.8f, 1.3f) },
                new[] { Pts(-1, 1, 1, 0.35f, -1, -0.35f, 1, -1) },
                new[] { star, starOutline },
                new[] { Pts(-1, 0.1f, -0.35f, -0.8f, 1, 1) },
                new[] { spiral },
                new[] { heart },
                new[] { infinity },
                new[] { wave },
                new[] { Pts(0, 1, -0.7f, 0, 0, -1, 0.7f, 0, 0, 1) },
                new[] { moon },
                new[] { Pts(-1, -1, -0.5f, 0.8f, 0, -0.3f, 0.5f, 0.8f, 1, -1) },
            };
            prepared = new Vector2[shapes.Length][][];
            for (int i = 0; i < shapes.Length; i++)
            {
                prepared[i] = new Vector2[shapes[i].Length][];
                for (int j = 0; j < shapes[i].Length; j++) prepared[i][j] = PointCloudRecognizer.Prepare(shapes[i][j]);
            }
        }

        public override void OnLevelStart()
        {
            Build();
            var pool = new List<int>();
            for (int i = 0; i < shapes.Length; i++) pool.Add(i);
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
            List<Vector2> t = shapes[current][0];
            float ext = 0.01f;   // la runa sta tutta nel riquadro della pergamena
            foreach (Vector2 q in t) ext = Mathf.Max(ext, Mathf.Abs(q.x), Mathf.Abs(q.y));
            runeLine.positionCount = t.Count;
            for (int i = 0; i < t.Count; i++) runeLine.SetPosition(i, runeCenter.position + (Vector3)(t[i] / ext * runeSize));
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
            float wanted = PointCloudRecognizer.Distance(pts, prepared[current]), best = wanted;
            for (int i = 0; i < prepared.Length; i++) if (i != current) best = Mathf.Min(best, PointCloudRecognizer.Distance(pts, prepared[i]));
            // la forma conta, non il verso né il punto di partenza; un po' storta va bene
            bool ok = wanted <= maxDistance && wanted <= best * vsBest;
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
            Haptics.Heavy();
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
