using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 6 — disinnesca la bomba. Due pulsanti (blu e rosso) e due cavi gialli aggrovigliati in modo casuale:
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
        [Header("Il tizio nel panico")]
        [SerializeField] GameObject guyPanic;
        [SerializeField] GameObject guyRelieved;
        [SerializeField] Tremble guyTremble;
        [SerializeField] SmokeEmitter sweat;
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
            Vector3 looseEnd = new Vector3(Random.value < 0.5f ? -1.9f : 1.9f, Random.Range(-0.6f, 0.8f), 0f);   // alto quanto il timer: non si capisce dalla lunghezza
            Transform connected = redIsConnected ? redStart : blueStart;
            Transform loose = redIsConnected ? blueStart : redStart;

            List<Vector3> toTimer = null, free = null;
            for (int attempt = 0; attempt < 60; attempt++)
            {
                Braid(connected.position, timerTerminal.position, loose.position, looseEnd, out toTimer, out free);
                if (Readable(toTimer, free)) break;
            }
            DrawWire("WireToTimer", toTimer);
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
            if (guyPanic != null) guyPanic.SetActive(true);
            if (guyRelieved != null) guyRelieved.SetActive(false);
            if (sweat != null) sweat.Play();
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
            // più il tempo stringe, più il tizio trema e suda
            float panic = 1f - Mathf.Clamp01(remaining / seconds);
            if (guyTremble != null) guyTremble.amplitude = Mathf.Lerp(0.015f, 0.06f, panic);
            if (sweat != null) sweat.puffsPerSecond = Mathf.Lerp(2f, 8f, panic);
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
            if (sweat != null) sweat.Stop();
            if (guyTremble != null) guyTremble.enabled = false;
            if (guyPanic != null) guyPanic.SetActive(false);
            if (guyRelieved != null) guyRelieved.SetActive(true);
            yield return new WaitForSeconds(1.3f);
            Win();
        }

        IEnumerator Explode()
        {
            if (display != null) display.text = "0.0";
            if (bomb != null) bomb.SetActive(false);
            if (sweat != null) sweat.Stop();
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

        // ---------- Cavi a treccia ----------

        const float OverlapDistance = 0.13f;   // centri più vicini di così = cavi visivamente sovrapposti
        const float MaxOverlapRun = 0.4f;      // tratto massimo sovrapposto ammesso (≈ 4-5 mm su un telefono)

        /// <summary>
        /// Due cavi a treccia attorno a una linea centrale comune: nel mezzo si scambiano di lato più volte con
        /// incroci netti (quasi perpendicolari), agli estremi si raccordano ai pulsanti e alle destinazioni.
        /// Così sono attorcigliati ma non corrono mai uno sopra l'altro.
        /// </summary>
        static void Braid(Vector3 a0, Vector3 a1, Vector3 b0, Vector3 b1, out List<Vector3> a, out List<Vector3> b)
        {
            float turns = Random.Range(2.2f, 3.0f);
            float width = 1.45f / turns * Random.Range(0.85f, 1f);   // larghezza e giri legati: incroci a ~60-90°
            float phase = Random.Range(0f, Mathf.PI * 2f);
            float wob = Random.Range(0f, Mathf.PI * 2f);
            Vector3 c0 = (a0 + b0) * 0.5f, c1 = (a1 + b1) * 0.5f;
            a = Strand(a0, a1, c0, c1, width, turns, phase, wob, 1f);
            b = Strand(b0, b1, c0, c1, width, turns, phase, wob, -1f);
        }

        static List<Vector3> Strand(Vector3 from, Vector3 to, Vector3 c0, Vector3 c1, float width, float turns, float phase, float wob, float sign)
        {
            var pts = new List<Vector3>();
            const int n = 160;
            for (int i = 0; i <= n; i++)
            {
                float t = i / (float)n;
                Vector3 straight = Vector3.Lerp(from, to, t);
                Vector3 braid = Vector3.Lerp(c0, c1, t);
                braid.x += sign * width * Mathf.Sin(2f * Mathf.PI * turns * t + phase);
                braid.y += 0.05f * Mathf.Sin(2f * Mathf.PI * 1.7f * t + wob);
                float m = Smooth(t / 0.2f) * Smooth((1f - t) / 0.2f);   // 0 agli attacchi, 1 nella treccia
                Vector3 p = Vector3.Lerp(straight, braid, m);
                p.x = Mathf.Clamp(p.x, -2.5f, 2.5f);
                pts.Add(p);
            }
            return pts;
        }

        static float Smooth(float x) { x = Mathf.Clamp01(x); return x * x * (3f - 2f * x); }

        /// <summary>
        /// Leggibile = si incrociano almeno 3 volte ma non restano mai sovrapposti per più di MaxOverlapRun
        /// (né tra loro né per tratti lunghi su se stessi).
        /// </summary>
        static bool Readable(List<Vector3> a, List<Vector3> b)
        {
            float run = 0f;
            for (int i = 1; i < a.Count; i++)
            {
                float d = DistanceToPolyline(a[i], b);
                run = d < OverlapDistance ? run + Vector3.Distance(a[i - 1], a[i]) : 0f;
                if (run > MaxOverlapRun) return false;
            }
            return Crossings(a, b) >= 3;
        }

        static float DistanceToPolyline(Vector3 p, List<Vector3> line)
        {
            float best = float.MaxValue;
            for (int i = 1; i < line.Count; i++)
            {
                Vector3 s0 = line[i - 1], s1 = line[i], d = s1 - s0;
                float t = Mathf.Clamp01(Vector3.Dot(p - s0, d) / Mathf.Max(1e-6f, d.sqrMagnitude));
                best = Mathf.Min(best, Vector3.Distance(p, s0 + d * t));
            }
            return best;
        }

        static int Crossings(List<Vector3> a, List<Vector3> b)
        {
            int count = 0;
            for (int i = 1; i < a.Count; i++)
                for (int j = 1; j < b.Count; j++)
                    if (SegmentsCross(a[i - 1], a[i], b[j - 1], b[j])) count++;
            return count;
        }

        static bool SegmentsCross(Vector3 p1, Vector3 p2, Vector3 q1, Vector3 q2)
        {
            float d1 = Cross(q2 - q1, p1 - q1), d2 = Cross(q2 - q1, p2 - q1);
            float d3 = Cross(p2 - p1, q1 - p1), d4 = Cross(p2 - p1, q2 - p1);
            return d1 * d2 < 0f && d3 * d4 < 0f;
        }

        static float Cross(Vector3 u, Vector3 v) { return u.x * v.y - u.y * v.x; }

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
