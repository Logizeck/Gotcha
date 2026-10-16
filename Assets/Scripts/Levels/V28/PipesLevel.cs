using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 77 — l'idraulico. Il fiore sta appassendo: tocca i tubi per ruotarli e porta l'acqua dal rubinetto
    /// (a sinistra) al vaso (a destra). L'acqua avanza da sola nei tubi collegati: si vede subito fin dove arriva.
    /// </summary>
    public class PipesLevel : LevelController
    {
        [SerializeField] SpriteRenderer[] tiles;      // righe dal basso, colonne da sinistra (cols x rows)
        [SerializeField] Sprite straight, corner, tee;
        [SerializeField] int cols = 4, rows = 5;
        [SerializeField] Vector2 origin = new Vector2(-1.65f, -2.4f);
        [SerializeField] float cell = 1.1f;
        [SerializeField] int sourceRow = 1, targetRow = 2;
        [SerializeField] GameObject plantDry, plantBloom;
        [SerializeField] Transform wiltBar;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float seconds = 50f;

        const int N = 1, E = 2, S = 4, W = 8;
        int[] type, rot;     // tipo 0 dritto (W-E), 1 curva (N-E), 2 a T (W-N-E)
        float clock;
        bool done;
        Camera cam;

        static int Base(int t) => t == 0 ? (W | E) : t == 1 ? (N | E) : (W | N | E);
        static int Rotate(int m, int r)
        {
            for (int i = 0; i < r; i++) m = ((m << 1) | (m >> 3)) & 15;   // N→E→S→W
            return m;
        }
        int Mask(int i) => Rotate(Base(type[i]), rot[i]);

        public override void OnLevelStart()
        {
            cam = Camera.main;
            int n = cols * rows;
            type = new int[n];
            rot = new int[n];
            // percorso garantito: da (0,sourceRow) entrando da ovest a (cols-1,targetRow) uscendo a est
            // colonna per colonna: un tratto in verticale fino a un'altezza a caso, poi un passo a destra
            var path = new List<Vector2Int>();
            Vector2Int c = new Vector2Int(0, sourceRow);
            path.Add(c);
            for (int x = 0; x < cols; x++)
            {
                int ty = x == cols - 1 ? targetRow : Random.Range(0, rows);
                while (c.y != ty) { c += ty > c.y ? Vector2Int.up : Vector2Int.down; path.Add(c); }
                if (x < cols - 1) { c += Vector2Int.right; path.Add(c); }
            }
            for (int i = 0; i < n; i++) { type[i] = Random.Range(0, 3); rot[i] = Random.Range(0, 4); }
            for (int k = 0; k < path.Count; k++)
            {
                Vector2Int p = path[k];
                Vector2Int a = k == 0 ? p + Vector2Int.left : path[k - 1];
                Vector2Int b = k == path.Count - 1 ? p + Vector2Int.right : path[k + 1];
                int need = Dir(p, a) | Dir(p, b);
                int i = p.y * cols + p.x;
                type[i] = (need == (W | E) || need == (N | S)) ? 0 : 1;
                // rotazione iniziale sbagliata apposta
                int good = 0;
                for (int r = 0; r < 4; r++) if (Rotate(Base(type[i]), r) == need) { good = r; break; }
                rot[i] = (good + Random.Range(1, 4)) % 4;
            }
            for (int i = 0; i < n; i++) Show(i);
            Flow();
            plantBloom.SetActive(false);
            if (bubble != null) { bubble.Show("Water! Turn the pipes!"); StartCoroutine(HideLater(1.8f)); }
        }

        static int Dir(Vector2Int from, Vector2Int to)
        {
            Vector2Int d = to - from;
            return d.y > 0 ? N : d.x > 0 ? E : d.y < 0 ? S : W;
        }

        void Show(int i)
        {
            tiles[i].sprite = type[i] == 0 ? straight : type[i] == 1 ? corner : tee;
            tiles[i].transform.localRotation = Quaternion.Euler(0, 0, -90f * rot[i]);
        }

        /// <summary>Riempie d'acqua i tubi collegati al rubinetto; true se l'acqua esce verso il vaso.</summary>
        bool Flow()
        {
            var wet = new bool[tiles.Length];
            var q = new Queue<Vector2Int>();
            int s = sourceRow * cols;
            bool reached = false;
            if ((Mask(s) & W) != 0) { wet[s] = true; q.Enqueue(new Vector2Int(0, sourceRow)); }
            while (q.Count > 0)
            {
                Vector2Int p = q.Dequeue();
                int m = Mask(p.y * cols + p.x);
                if (p.x == cols - 1 && p.y == targetRow && (m & E) != 0) reached = true;
                foreach (var (bit, d, back) in new[] { (N, Vector2Int.up, S), (E, Vector2Int.right, W), (S, Vector2Int.down, N), (W, Vector2Int.left, E) })
                {
                    if ((m & bit) == 0) continue;
                    Vector2Int o = p + d;
                    if (o.x < 0 || o.y < 0 || o.x >= cols || o.y >= rows) continue;
                    int j = o.y * cols + o.x;
                    if (wet[j] || (Mask(j) & back) == 0) continue;
                    wet[j] = true;
                    q.Enqueue(o);
                }
            }
            for (int i = 0; i < tiles.Length; i++) tiles[i].color = wet[i] ? new Color(0.45f, 0.75f, 1f) : Color.white;
            return reached;
        }

        void Update()
        {
            if (done || IsFinished) return;
            clock += Time.deltaTime;
            if (wiltBar != null) wiltBar.localScale = new Vector3(Mathf.Clamp01(1f - clock / seconds), 1f, 1f);
            if (clock >= seconds) { StartCoroutine(Wilted()); return; }
            if (!PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            int cx = Mathf.RoundToInt((w.x - origin.x) / cell), cy = Mathf.RoundToInt((w.y - origin.y) / cell);
            if (cx < 0 || cy < 0 || cx >= cols || cy >= rows) return;
            int i = cy * cols + cx;
            rot[i] = (rot[i] + 1) % 4;
            Show(i);
            Sfx.Play(this, "tick");
            if (Flow()) StartCoroutine(Bloom());
        }

        IEnumerator Bloom()
        {
            done = true;
            Sfx.Play(this, "ding", 0.6f);
            yield return new WaitForSeconds(0.4f);
            plantDry.SetActive(false);
            plantBloom.SetActive(true);
            if (bubble != null) bubble.Show("Ahhh, refreshing!");
            yield return new WaitForSeconds(1.4f);
            Win();
        }

        IEnumerator Wilted()
        {
            done = true;
            if (bubble != null) bubble.Show("Too late... *wilts*");
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
