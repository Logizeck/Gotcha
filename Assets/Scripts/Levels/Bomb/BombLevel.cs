using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 7 — disinnesca la bomba. Due pulsanti (blu e rosso) e due cavi gialli aggrovigliati in modo casuale:
    /// un cavo finisce libero, l'altro arriva al timer della dinamite. Il timer parte da 10 secondi.
    /// Premere il pulsante collegato al timer lo ferma; quello sbagliato (o il tempo scaduto) fa esplodere tutto.
    /// </summary>
    public class BombLevel : LevelController
    {
        [SerializeField] TapButton blueButton;
        [SerializeField] TapButton redButton;
        [SerializeField] Transform blueStart, redStart;   // dove escono i cavi dai pulsanti
        [SerializeField] Transform timerTerminal;         // dove il cavo giusto entra nel timer
        [SerializeField] Transform wireEnd;               // estremità libera (fili sfilacciati)
        [SerializeField] TextMesh display;
        [SerializeField] GameObject bomb;
        [SerializeField] GameObject boom;
        [SerializeField] SmokeEmitter smoke;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] Material wireMaterial;
        [SerializeField] float seconds = 10f;

        static readonly Color WireYellow = new Color(1f, 0.85f, 0.15f);
        static readonly Color Ink = new Color(0.17f, 0.13f, 0.2f);
        static readonly Color LedRed = new Color(1f, 0.2f, 0.2f);
        static readonly Color LedGreen = new Color(0.3f, 1f, 0.45f);

        bool redIsConnected;
        float remaining;
        bool running;

        public override void OnLevelStart()
        {
            redIsConnected = Random.value < 0.5f;
            Vector3 looseEnd = new Vector3(Random.value < 0.5f ? -1.9f : 1.9f, Random.Range(-1.6f, -0.6f), 0f);
            Vector3 toTimer = timerTerminal.position;
            Transform connected = redIsConnected ? redStart : blueStart;
            Transform loose = redIsConnected ? blueStart : redStart;

            // i due cavi si incrociano: uno va a destra-sinistra-destra, l'altro il contrario
            float s = Random.value < 0.5f ? 1f : -1f;
            DrawWire("WireToTimer", Tangle(connected.position, toTimer, s));
            List<Vector3> free = Tangle(loose.position, looseEnd, -s);
            DrawWire("WireLoose", free);
            if (wireEnd != null)
            {
                wireEnd.position = looseEnd;
                Vector3 dir = free[free.Count - 1] - free[free.Count - 2];
                wireEnd.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            }

            if (boom != null) boom.SetActive(false);
            if (bubble != null) bubble.Hide();
            blueButton.Pressed += OnButton;
            redButton.Pressed += OnButton;
            remaining = seconds;
            running = true;
        }

        void OnDestroy()
        {
            if (blueButton != null) blueButton.Pressed -= OnButton;
            if (redButton != null) redButton.Pressed -= OnButton;
        }

        void Update()
        {
            if (!running) return;
            remaining -= Time.deltaTime;
            if (display != null)
            {
                display.text = Mathf.Max(0f, remaining).ToString("0.0");
                display.color = remaining < 3f && Mathf.Repeat(remaining, 0.3f) < 0.15f ? Color.white : LedRed;
            }
            if (remaining <= 0f) { running = false; StartCoroutine(Explode()); }
        }

        void OnButton(TapButton b)
        {
            if (!running) return;
            running = false;
            blueButton.interactable = redButton.interactable = false;
            bool pressedRed = b == redButton;
            StartCoroutine(pressedRed == redIsConnected ? Defuse() : Explode());
        }

        IEnumerator Defuse()
        {
            if (display != null) { display.text = "--.-"; display.color = LedGreen; }
            if (bubble != null) bubble.Show("Phew!");
            yield return new WaitForSeconds(1.3f);
            Win();
        }

        IEnumerator Explode()
        {
            if (display != null) display.text = "0.0";
            if (bomb != null) bomb.SetActive(false);
            if (boom != null)
            {
                boom.SetActive(true);
                Transform t = boom.transform;
                Vector3 full = t.localScale;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.18f) { t.localScale = full * k * 1.15f; yield return null; }
                t.localScale = full;
            }
            if (smoke != null) smoke.Play();
            Vector3 start = transform.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                transform.localPosition = start + (Vector3)Random.insideUnitCircle * 0.18f * (1f - k);
                yield return null;
            }
            transform.localPosition = start;
            yield return new WaitForSeconds(0.6f);
            if (smoke != null) smoke.Stop();
            Fail();
        }

        // ---------- Cavi ----------

        /// <summary>Percorso aggrovigliato da a verso b che ondeggia da un lato all'altro (side = ±1).</summary>
        static List<Vector3> Tangle(Vector3 a, Vector3 b, float side)
        {
            var ctrl = new List<Vector3> { a };
            int bends = 4;
            for (int i = 1; i <= bends; i++)
            {
                float t = i / (float)(bends + 1);
                Vector3 p = Vector3.Lerp(a, b, t);
                p.x = side * (i % 2 == 0 ? -1f : 1f) * Random.Range(0.8f, 1.7f);
                p.y += Random.Range(-0.15f, 0.15f);
                ctrl.Add(p);
            }
            ctrl.Add(b);

            var pts = new List<Vector3>();
            for (int i = 0; i < ctrl.Count - 1; i++)
            {
                Vector3 p0 = ctrl[Mathf.Max(0, i - 1)], p1 = ctrl[i], p2 = ctrl[i + 1], p3 = ctrl[Mathf.Min(ctrl.Count - 1, i + 2)];
                for (int j = 0; j < 12; j++)
                {
                    float t = j / 12f, t2 = t * t, t3 = t2 * t;
                    pts.Add(0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3));
                }
            }
            pts.Add(b);
            return pts;
        }

        void DrawWire(string name, List<Vector3> pts)
        {
            Vector3[] arr = pts.ToArray();
            Line(name + "Outline", arr, 0.15f, Ink, 3);
            Line(name, arr, 0.09f, WireYellow, 4);
        }

        void Line(string name, Vector3[] pts, float width, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.widthMultiplier = width;
            lr.numCapVertices = 5;
            lr.numCornerVertices = 4;
            lr.sharedMaterial = wireMaterial != null ? wireMaterial : new Material(Shader.Find("Sprites/Default"));
            lr.startColor = lr.endColor = color;
            lr.sortingOrder = order;
            lr.positionCount = pts.Length;
            lr.SetPositions(pts);
        }
    }
}
