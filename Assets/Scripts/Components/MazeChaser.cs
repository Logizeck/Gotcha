using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Vespa che insegue la sfera DENTRO un labirinto: invece di puntare dritta (e restare incastrata dietro un muro)
    /// segue la strada più breve. All'avvio costruisce una griglia fine dell'area del labirinto (celle libere = dove la
    /// vespa ci sta senza toccare un muro); più volte al secondo calcola, partendo dalla sfera, la distanza a piedi di
    /// ogni cella (BFS) e la vespa scende sempre verso la cella vicina più vicina alla sfera. Quando vede la sfera senza
    /// muri in mezzo, le va dritta addosso. Si ferma ogni tanto a riprendere fiato (così la si può seminare).
    /// Il ronzio cresce man mano che la sfera si avvicina, anche attraverso i muri.
    /// </summary>
    public class MazeChaser : MonoBehaviour
    {
        public Rigidbody2D ball;
        public Transform maze;                  // origine del labirinto (coordinate locali dei muri)
        public Vector2 areaMin = new Vector2(-2.75f, -3.5f), areaMax = new Vector2(2.75f, 3.5f);
        public float speed = 1.7f;
        public float radius = 0.22f;
        public float stingDistance = 0.42f;
        public float startDelay = 1.5f;
        public float restEvery = 5f, restFor = 0.8f;
        [Tooltip("0 = insegue da subito; > 0 = dorme finché la sfera non arriva a questa distanza (labirinto al buio)")]
        public float wakeDistance = 0f;
        public AudioSource buzz;
        public float loudAt = 0.8f, quietAt = 5f;   // distanze per il volume massimo e minimo del ronzio
        public Transform visual;                    // si gira verso dove va

        public event Action<MazeChaser> Stung;
        public bool IsAwake { get; private set; }
        public bool Frozen { get; set; }

        const float Step = 0.25f;
        int cols, rows;
        bool[] free;
        int[] dist;
        readonly Queue<int> queue = new Queue<int>();
        float clock, replan, flip = 1f, restOffset;
        bool built;

        void Start()
        {
            IsAwake = wakeDistance <= 0f;
            restOffset = UnityEngine.Random.Range(0f, restEvery);   // più vespe non si riposano tutte insieme
            if (visual == null) visual = transform;
            flip = Mathf.Sign(visual.localScale.x);
        }

        /// <summary>Ricostruisce la griglia (es. dopo che si è aperto un cancello).</summary>
        public void Build()
        {
            cols = Mathf.CeilToInt((areaMax.x - areaMin.x) / Step) + 1;
            rows = Mathf.CeilToInt((areaMax.y - areaMin.y) / Step) + 1;
            free = new bool[cols * rows];
            dist = new int[cols * rows];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    free[r * cols + c] = !Blocked(CellWorld(c, r), radius * 0.9f);
            built = true;
        }

        Vector2 CellWorld(int c, int r)
        {
            Vector2 local = areaMin + new Vector2(c * Step, r * Step);
            return maze != null ? (Vector2)maze.TransformPoint(local) : local;
        }

        bool WorldToCell(Vector2 w, out int c, out int r)
        {
            Vector2 local = maze != null ? (Vector2)maze.InverseTransformPoint(w) : w;
            c = Mathf.RoundToInt((local.x - areaMin.x) / Step);
            r = Mathf.RoundToInt((local.y - areaMin.y) / Step);
            return c >= 0 && r >= 0 && c < cols && r < rows;
        }

        /// <summary>Muro = collider solido senza corpo rigido (la sfera, le altre vespe e i trigger non contano).</summary>
        static bool Blocked(Vector2 p, float r)
        {
            foreach (Collider2D h in Physics2D.OverlapCircleAll(p, r))
                if (!h.isTrigger && h.attachedRigidbody == null) return true;
            return false;
        }

        void Replan()
        {
            for (int i = 0; i < dist.Length; i++) dist[i] = int.MaxValue;
            if (!WorldToCell(ball.position, out int bc, out int br)) return;
            int start = NearestFree(bc, br);
            if (start < 0) return;
            dist[start] = 0;
            queue.Clear();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                int i = queue.Dequeue();
                int c = i % cols, r = i / cols;
                for (int dr = -1; dr <= 1; dr++)
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dc == 0 && dr == 0) continue;
                        int nc = c + dc, nr = r + dr;
                        if (nc < 0 || nr < 0 || nc >= cols || nr >= rows) continue;
                        int n = nr * cols + nc;
                        if (!free[n] || dist[n] != int.MaxValue) continue;
                        // in diagonale solo se entrambi i lati sono liberi (niente tagli d'angolo)
                        if (dc != 0 && dr != 0 && (!free[r * cols + nc] || !free[nr * cols + c])) continue;
                        dist[n] = dist[i] + (dc != 0 && dr != 0 ? 14 : 10);
                        queue.Enqueue(n);
                    }
            }
        }

        int NearestFree(int c, int r)
        {
            for (int k = 0; k <= 3; k++)
                for (int dr = -k; dr <= k; dr++)
                    for (int dc = -k; dc <= k; dc++)
                    {
                        int nc = c + dc, nr = r + dr;
                        if (nc < 0 || nr < 0 || nc >= cols || nr >= rows) continue;
                        if (free[nr * cols + nc]) return nr * cols + nc;
                    }
            return -1;
        }

        void Update()
        {
            if (ball == null || Frozen) return;
            if (!built) Build();   // al primo frame: il livello precedente (e i suoi muri) è già stato distrutto
            clock += Time.deltaTime;
            Vector2 pos = transform.position, target = ball.position;
            float d = Vector2.Distance(pos, target);
            if (buzz != null) buzz.volume = Mathf.Lerp(0.6f, 0.04f, Mathf.InverseLerp(loudAt, quietAt, d));
            if (!IsAwake)
            {
                if (d < wakeDistance) { IsAwake = true; clock = startDelay; }   // svegliata: parte subito
                else { Hover(pos); return; }
            }
            if (clock < startDelay) { Hover(pos); return; }
            if (Mathf.Repeat(clock + restOffset, restEvery) > restEvery - restFor) { Hover(pos); return; }

            replan -= Time.deltaTime;
            if (replan <= 0f) { replan = 0.2f; Replan(); }

            Vector2 want = ClearLine(pos, target) ? target - pos : NextStep(pos) - pos;
            if (want.sqrMagnitude < 1e-6f) want = target - pos;
            Vector2 step = want.normalized * speed * Time.deltaTime;
            Vector2 moved = Move(pos, step);
            if ((moved - pos).sqrMagnitude < step.sqrMagnitude * 0.25f)
            {
                // contro uno spigolo: scivola lungo gli assi
                Vector2 alt = Move(pos, new Vector2(step.x, 0f));
                if ((alt - pos).sqrMagnitude < 1e-7f) alt = Move(pos, new Vector2(0f, step.y));
                moved = alt;
            }
            transform.position = new Vector3(moved.x, moved.y, transform.position.z);
            if (Mathf.Abs(moved.x - pos.x) > 0.0005f)
            {
                float s = Mathf.Abs(visual.localScale.y);
                visual.localScale = new Vector3(moved.x > pos.x ? -s * flip : s * flip, s, 1f);
            }
            if (Vector2.Distance(moved, target) < stingDistance) Stung?.Invoke(this);
        }

        Vector2 NextStep(Vector2 pos)
        {
            if (!WorldToCell(pos, out int c, out int r)) return ball.position;
            int here = NearestFree(c, r);
            if (here < 0) return ball.position;
            int hc = here % cols, hr = here / cols, best = here, bestD = dist[here];
            for (int dr = -1; dr <= 1; dr++)
                for (int dc = -1; dc <= 1; dc++)
                {
                    int nc = hc + dc, nr = hr + dr;
                    if (nc < 0 || nr < 0 || nc >= cols || nr >= rows) continue;
                    int n = nr * cols + nc;
                    if (free[n] && dist[n] < bestD) { bestD = dist[n]; best = n; }
                }
            return CellWorld(best % cols, best / cols);
        }

        bool ClearLine(Vector2 a, Vector2 b)
        {
            Vector2 d = b - a;
            float len = d.magnitude;
            if (len < 1e-4f) return true;
            foreach (RaycastHit2D h in Physics2D.CircleCastAll(a, radius * 0.9f, d / len, len))
                if (!h.collider.isTrigger && h.rigidbody == null) return false;
            return true;
        }

        Vector2 Move(Vector2 pos, Vector2 step)
        {
            float len = step.magnitude;
            if (len < 1e-6f) return pos;
            Vector2 dir = step / len;
            float allowed = len;
            foreach (RaycastHit2D h in Physics2D.CircleCastAll(pos, radius, dir, len))
            {
                if (h.rigidbody != null || h.collider.isTrigger) continue;
                allowed = Mathf.Min(allowed, Mathf.Max(0f, h.distance - 0.01f));
            }
            return pos + dir * allowed;
        }

        void Hover(Vector2 pos)
        {
            transform.position = new Vector3(pos.x + Mathf.Sin(Time.time * 47f + restOffset) * 0.004f, pos.y + Mathf.Sin(Time.time * 9f) * 0.004f, transform.position.z);
        }
    }
}
