using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Hungry Snake" — il classico serpente. Swipe (o tocco a lato della testa) per girare, niente inversioni
    /// a U. Mangia 8 mele per vincere; ogni mela lo rende un po' più veloce. Muro o morso al corpo = una delle 3 vite.
    /// </summary>
    public class HungrySnakeLevel : LevelController
    {
        public const int Cols = 9, Rows = 12;
        public const float Cell = 0.6f, BoardY = -0.5f;

        /// <summary>Centro della casella (x da sinistra, y dal basso) in coordinate locali.</summary>
        public static Vector2 CellPos(int x, int y) => new Vector2((x - 4) * Cell, BoardY + (y - 5.5f) * Cell);

        [SerializeField] Transform head;
        [SerializeField] Transform tongue;
        [SerializeField] Transform bodyTemplate;   // modello (spento) dei segmenti del corpo
        [SerializeField] Transform tail;
        [SerializeField] Transform apple;
        [SerializeField] GameObject[] hearts;
        [SerializeField] TextMesh counter;
        [SerializeField] TextMesh prompt;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int applesNeeded = 8;
        [SerializeField] float startInterval = 0.22f, speedUp = 0.012f, minInterval = 0.12f;

        readonly List<Vector2Int> cells = new List<Vector2Int>();
        readonly List<Vector2Int> prevCells = new List<Vector2Int>();
        readonly List<Transform> bodies = new List<Transform>();
        readonly List<Vector2Int> queue = new List<Vector2Int>();
        Vector2Int dir, appleCell;
        int apples, lives = 3;
        float tick, interval;
        bool running, busy, done;
        Vector2 downPos;
        bool tracking, swiped;
        Camera cam;
        Vector3 headScale, appleScale;
        readonly System.Random rnd = new System.Random();

        public override void OnLevelStart()
        {
            cam = Camera.main;
            headScale = head.localScale;
            appleScale = apple.localScale;
            bodyTemplate.gameObject.SetActive(false);
            interval = startInterval;
            ResetSnake();
            SpawnApple();
            UpdateHud();
            if (bubble != null) { bubble.Show("So hungry... 8 apples please!"); StartCoroutine(HideLater(2.4f)); }
        }

        void ResetSnake()
        {
            cells.Clear();
            for (int i = 0; i < 3; i++) cells.Add(new Vector2Int(4, 3 - i));
            prevCells.Clear();
            prevCells.AddRange(cells);
            dir = Vector2Int.up;
            queue.Clear();
            running = false;
            tick = 0f;
            prompt.text = "SWIPE TO START";
            if (cells.Contains(appleCell)) SpawnApple();
            Render(1f);
        }

        void UpdateHud()
        {
            counter.text = "APPLES " + apples + "/" + applesNeeded;
            for (int i = 0; i < hearts.Length; i++) hearts[i].SetActive(i < lives);
        }

        void SpawnApple()
        {
            var free = new List<Vector2Int>();
            for (int x = 0; x < Cols; x++)
                for (int y = 0; y < Rows; y++)
                {
                    var c = new Vector2Int(x, y);
                    if (cells.Contains(c)) continue;
                    if (cells.Count > 0 && Mathf.Abs(c.x - cells[0].x) + Mathf.Abs(c.y - cells[0].y) < 3) continue;
                    free.Add(c);
                }
            if (free.Count == 0) return;
            appleCell = free[rnd.Next(free.Count)];
            apple.localPosition = CellPos(appleCell.x, appleCell.y);
        }

        // ---------- ciclo ----------

        void Update()
        {
            if (IsFinished || done) return;
            float t = Time.time;
            apple.localScale = appleScale * (1f + Mathf.Sin(t * 5f) * 0.06f);
            // lingua che guizza ogni tanto
            float flick = Mathf.Repeat(t, 1.3f);
            tongue.localScale = new Vector3(flick < 0.25f ? Mathf.Sin(flick / 0.25f * Mathf.PI) : 0f, 1f, 1f);
            if (busy) return;
            if (Time.timeScale > 0f) ReadInput();
            if (!running) { Render(1f); return; }
            tick += Time.deltaTime;
            if (tick >= interval)
            {
                tick -= interval;
                Step();
                if (busy || done) return;
            }
            Render(Mathf.Clamp01(tick / interval));
        }

        void Step()
        {
            while (queue.Count > 0)
            {
                Vector2Int q = queue[0];
                queue.RemoveAt(0);
                if (q != -dir && q != dir) { dir = q; break; }
            }
            Vector2Int next = cells[0] + dir;
            if (next.x < 0 || next.x >= Cols || next.y < 0 || next.y >= Rows) { StartCoroutine(Crash("Ouch! A wall!")); return; }
            bool eat = next == appleCell;
            int last = cells.Count - 1;
            for (int i = 0; i < cells.Count; i++)
                if (cells[i] == next && (i < last || eat)) { StartCoroutine(Crash("Ow! I bit myself!")); return; }

            prevCells.Clear();
            for (int i = 0; i <= cells.Count; i++) prevCells.Add(cells[Mathf.Max(0, i - 1)]);
            cells.Insert(0, next);
            if (!eat) { cells.RemoveAt(cells.Count - 1); prevCells.RemoveAt(prevCells.Count - 1); }
            else
            {
                prevCells[prevCells.Count - 1] = cells[cells.Count - 1];   // il nuovo segmento resta fermo
                apples++;
                Sfx.Play(this, "pop", 0.8f);
                Haptics.Light();
                StartCoroutine(Gulp());
                interval = Mathf.Max(minInterval, startInterval - speedUp * apples);
                UpdateHud();
                if (apples >= applesNeeded) { StartCoroutine(Full()); return; }
                SpawnApple();
            }
        }

        void Render(float f)
        {
            int n = cells.Count;
            while (bodies.Count < n - 2)
            {
                Transform b = Instantiate(bodyTemplate, bodyTemplate.parent);
                b.gameObject.SetActive(true);
                bodies.Add(b);
            }
            for (int i = 0; i < bodies.Count; i++) bodies[i].gameObject.SetActive(i < n - 2);
            for (int i = 0; i < n; i++)
            {
                Vector2 from = CellPos(prevCells[i].x, prevCells[i].y), to = CellPos(cells[i].x, cells[i].y);
                Vector2 p = Vector2.Lerp(from, to, f);
                if (i == 0) head.localPosition = p;
                else if (i == n - 1) tail.localPosition = p;
                else bodies[i - 1].localPosition = p;
            }
            head.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            Vector2 away = (Vector2)tail.localPosition - (Vector2)(n > 2 ? bodies[n - 3].localPosition : head.localPosition);
            if (away.sqrMagnitude > 0.0001f) tail.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(away.y, away.x) * Mathf.Rad2Deg);
        }

        // ---------- input ----------

        void Turn(Vector2Int d)
        {
            Vector2Int refDir = queue.Count > 0 ? queue[queue.Count - 1] : dir;
            if (d == refDir || d == -refDir) { if (!running && d == refDir) Begin(); return; }
            if (queue.Count < 2) queue.Add(d);
            Begin();
        }

        void Begin()
        {
            if (running) return;
            running = true;
            tick = 0f;
            prompt.text = "";
        }

        void ReadInput()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Turn(Vector2Int.left);
            if (Input.GetKeyDown(KeyCode.RightArrow)) Turn(Vector2Int.right);
            if (Input.GetKeyDown(KeyCode.UpArrow)) Turn(Vector2Int.up);
            if (Input.GetKeyDown(KeyCode.DownArrow)) Turn(Vector2Int.down);
#endif
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { tracking = false; return; }
            Vector2 w = transform.InverseTransformPoint(PointerInput.ToWorld(cam, sp));
            if (began) { tracking = !PointerInput.IsOverUi(); swiped = false; downPos = w; }
            if (!tracking) return;
            Vector2 d = w - downPos;
            if (d.magnitude > 0.35f)
            {
                Turn(Mathf.Abs(d.x) > Mathf.Abs(d.y) ? new Vector2Int(d.x > 0 ? 1 : -1, 0) : new Vector2Int(0, d.y > 0 ? 1 : -1));
                swiped = true;
                downPos = w;
            }
            if (ended)
            {
                tracking = false;
                if (swiped) return;
                // tocco: gira verso il dito (di lato rispetto alla direzione attuale)
                Vector2 rel = w - (Vector2)head.localPosition;
                Vector2Int cur = queue.Count > 0 ? queue[queue.Count - 1] : dir;
                if (!running) { Begin(); return; }
                if (cur.x == 0) Turn(new Vector2Int(rel.x > 0 ? 1 : -1, 0));
                else Turn(new Vector2Int(0, rel.y > 0 ? 1 : -1));
            }
        }

        // ---------- eventi ----------

        IEnumerator Gulp()
        {
            for (float t = 0f; t < 0.25f; t += Time.deltaTime)
            {
                head.localScale = headScale * (1f + Mathf.Sin(t / 0.25f * Mathf.PI) * 0.25f);
                yield return null;
            }
            head.localScale = headScale;
        }

        IEnumerator Crash(string line)
        {
            busy = true;
            lives--;
            UpdateHud();
            Haptics.Medium();
            Sfx.Play(this, "bonk", 0.9f);
            if (bubble != null) bubble.Show(lives > 0 ? line : "Out of lives...");
            Render(1f);
            // il serpente lampeggia
            for (int k = 0; k < 6; k++)
            {
                SetVisible(k % 2 == 1);
                yield return new WaitForSeconds(0.12f);
            }
            SetVisible(true);
            if (lives <= 0)
            {
                done = true;
                yield return new WaitForSeconds(0.6f);
                Fail();
                yield break;
            }
            yield return new WaitForSeconds(0.3f);
            ResetSnake();
            if (bubble != null) bubble.Hide();
            busy = false;
        }

        void SetVisible(bool on)
        {
            head.gameObject.SetActive(on);
            tail.gameObject.SetActive(on);
            for (int i = 0; i < bodies.Count; i++) bodies[i].gameObject.SetActive(on && i < cells.Count - 2);
        }


        IEnumerator Full()
        {
            done = true;
            Render(1f);
            apple.gameObject.SetActive(false);
            Sfx.Play(this, "ding", 0.8f);
            if (bubble != null) bubble.Show("Burp! So full and happy!");
            for (float t = 0f; t < 1.8f; t += Time.deltaTime)
            {
                head.localScale = headScale * (1f + Mathf.Abs(Mathf.Sin(t * 8f)) * 0.2f);
                for (int i = 0; i < cells.Count - 2; i++)
                    bodies[i].localScale = Vector3.one * bodyTemplate.localScale.x * (1f + Mathf.Sin(t * 10f - i * 0.8f) * 0.15f);
                yield return null;
            }
            Win();
        }

        IEnumerator HideLater(float s)
        {
            yield return new WaitForSeconds(s);
            if (bubble != null && !done && !busy) bubble.Hide();
        }
    }
}
