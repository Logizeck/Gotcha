using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Color tubes — metti in ordine le palline: quattro provette da 4, tre colori da 4 palline. Tocca una provetta
    /// per sollevare la pallina in cima, toccane un'altra per lasciarla: si può solo su una provetta vuota o sopra una
    /// pallina dello stesso colore (e non piena). Vinci quando ogni provetta è vuota o ha 4 palline uguali.
    /// Sorpresa: la "pallina grigia" è un criceto arrotolato: appena lo sollevi si srotola, protesta e scappa via,
    /// lasciando più spazio. La disposizione è casuale ma sempre risolvibile (verificata con una ricerca in ampiezza).
    /// </summary>
    public class ColorTubesLevel : LevelController
    {
        [SerializeField] Transform[] tubes;            // 4 provette: origine = fondo interno
        [SerializeField] SpriteRenderer[] balls;       // 12 palline: colore = indice / 4
        [SerializeField] SpriteRenderer hamster;       // il criceto arrotolato (indice 12)
        [SerializeField] Sprite hamsterRun;            // il criceto srotolato che scappa
        [SerializeField] float hamsterRunScale = 0.42f;
        [SerializeField] Transform resetButton;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float slotBase = 0.22f, slotStep = 0.6f, hoverHeight = 3.25f;

        const int Cap = 4, HamsterId = 12;
        List<int>[] tube;
        List<int>[] startLayout;
        int lifted = -1, liftedFrom = -1;
        bool busy, done, hamsterGone;
        Camera cam;

        static int ColorOf(int id) => id == HamsterId ? 3 : id / 4;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            startLayout = Generate(true);
            Load(startLayout);
            if (bubble != null) { bubble.Show("Sort the balls: one colour per tube!", true); StartCoroutine(HideLater(2.4f)); }
        }

        // ---------- disposizione ----------

        List<int>[] Generate(bool withHamster)
        {
            List<int>[] best = null;
            for (int attempt = 0; attempt < 300; attempt++)
            {
                var ids = new List<int>();
                for (int i = 0; i < 12; i++) ids.Add(i);
                if (withHamster) ids.Add(HamsterId);
                for (int i = ids.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (ids[i], ids[j]) = (ids[j], ids[i]); }
                var t = new List<int>[4];
                for (int i = 0; i < 4; i++) t[i] = new List<int>();
                foreach (int id in ids)
                {
                    int k;
                    do k = Random.Range(0, 4); while (t[k].Count >= Cap);
                    t[k].Add(id);
                }
                bool bad = false;
                foreach (List<int> l in t)
                {
                    if (withHamster && l.Count > 0 && l[l.Count - 1] == HamsterId) bad = true;   // il criceto non in cima
                    if (Complete(l)) bad = true;                                                    // nessuna provetta già fatta
                }
                if (bad) continue;
                int d = Distance(t);
                if (d < 0) continue;
                best = t;
                if (d >= 8 && d <= 11) break;   // né banale né estenuante
            }
            if (best == null)
            {
                // ripiego fatto a mano (risolvibile in 10 mosse): rosso 0-3, blu 4-7, giallo 8-11
                best = new[] { new List<int> { 8, 9, 0 }, new List<int> { 1, 10 }, new List<int> { 4, 5, 6, 2 }, new List<int> { 11, 7, 3 } };
                if (withHamster) best[0].Insert(0, HamsterId);
            }
            return best;
        }

        static bool Complete(List<int> l)
        {
            if (l.Count != Cap) return false;
            int c = ColorOf(l[0]);
            foreach (int id in l) if (ColorOf(id) != c || id == HamsterId) return false;
            return true;
        }

        static string Key(int[][] s)
        {
            var sb = new System.Text.StringBuilder();
            foreach (int[] l in s) { foreach (int c in l) sb.Append((char)('0' + c)); sb.Append('|'); }
            return sb.ToString();
        }

        /// <summary>Mosse minime per risolvere (ricerca in ampiezza sui colori), -1 se impossibile.</summary>
        static int Distance(List<int>[] t)
        {
            var s0 = new int[4][];
            for (int i = 0; i < 4; i++) { s0[i] = new int[t[i].Count]; for (int k = 0; k < t[i].Count; k++) s0[i][k] = ColorOf(t[i][k]); }
            var seen = new HashSet<string> { Key(s0) };
            var q = new Queue<(int[][], int)>();
            q.Enqueue((s0, 0));
            while (q.Count > 0 && seen.Count < 40000)
            {
                var (s, d) = q.Dequeue();
                bool solved = true;
                foreach (int[] l in s)
                {
                    if (l.Length == 0) continue;
                    if (l.Length != Cap) { solved = false; break; }
                    foreach (int c in l) if (c != l[0] || c == 3) { solved = false; break; }
                    if (!solved) break;
                }
                if (solved) return d;
                for (int i = 0; i < 4; i++)
                {
                    if (s[i].Length == 0) continue;
                    int top = s[i][s[i].Length - 1];
                    if (top == 3)   // il criceto scappa quando lo sollevi
                    {
                        var n = Copy(s);
                        n[i] = Pop(s[i]);
                        if (seen.Add(Key(n))) q.Enqueue((n, d + 1));
                        continue;
                    }
                    for (int j = 0; j < 4; j++)
                    {
                        if (j == i || s[j].Length >= Cap) continue;
                        if (s[j].Length > 0 && s[j][s[j].Length - 1] != top) continue;
                        var n = Copy(s);
                        n[i] = Pop(s[i]);
                        n[j] = Push(s[j], top);
                        if (seen.Add(Key(n))) q.Enqueue((n, d + 1));
                    }
                }
            }
            return -1;
        }

        static int[][] Copy(int[][] s) { var n = new int[s.Length][]; for (int i = 0; i < s.Length; i++) n[i] = s[i]; return n; }
        static int[] Pop(int[] a) { var n = new int[a.Length - 1]; System.Array.Copy(a, n, n.Length); return n; }
        static int[] Push(int[] a, int v) { var n = new int[a.Length + 1]; System.Array.Copy(a, n, a.Length); n[a.Length] = v; return n; }

        void Load(List<int>[] layout)
        {
            tube = new List<int>[4];
            for (int i = 0; i < 4; i++) tube[i] = new List<int>(layout[i]);
            lifted = liftedFrom = -1;
            for (int i = 0; i < 4; i++)
                for (int k = 0; k < tube[i].Count; k++) Obj(tube[i][k]).position = Slot(i, k);
        }

        Transform Obj(int id) => id == HamsterId ? hamster.transform : balls[id].transform;
        Vector3 Slot(int t, int k) => tubes[t].position + new Vector3(0f, slotBase + slotStep * k, 0f);
        Vector3 Hover(int t) => tubes[t].position + new Vector3(0f, hoverHeight, 0f);

        // ---------- input ----------

        void Update()
        {
            if (busy || done || IsFinished) return;
            if (!PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (Vector2.Distance(w, resetButton.position) < 0.6f) { StartCoroutine(ResetRoutine()); return; }
            int t = -1;
            for (int i = 0; i < tubes.Length; i++)
            {
                Vector2 p = tubes[i].position;
                if (Mathf.Abs(w.x - p.x) < 0.64f && w.y > p.y - 0.6f && w.y < p.y + hoverHeight + 0.6f) t = i;
            }
            if (t < 0) return;
            if (lifted < 0)
            {
                if (tube[t].Count == 0) return;
                lifted = tube[t][tube[t].Count - 1];
                liftedFrom = t;
                tube[t].RemoveAt(tube[t].Count - 1);
                Sfx.Play(this, "tick", 0.8f);
                if (lifted == HamsterId) { StartCoroutine(HamsterEscape()); return; }
                StartCoroutine(Move(Obj(lifted), Hover(t), 0.12f));
                return;
            }
            if (t == liftedFrom)
            {
                Drop(t);
                return;
            }
            List<int> dst = tube[t];
            bool ok = dst.Count < Cap && (dst.Count == 0 || ColorOf(dst[dst.Count - 1]) == ColorOf(lifted));
            if (ok) Drop(t);
            else StartCoroutine(Refuse(t));
        }

        void Drop(int t)
        {
            int id = lifted, from = liftedFrom;
            tube[t].Add(id);
            lifted = liftedFrom = -1;
            StartCoroutine(DropRoutine(Obj(id), from, t, tube[t].Count - 1));
        }

        IEnumerator DropRoutine(Transform ball, int from, int to, int slot)
        {
            busy = true;
            if (from != to) yield return Move(ball, Hover(to), 0.14f);
            yield return Move(ball, Slot(to, slot), 0.12f);
            Sfx.Play(this, "clunk", 0.4f);
            busy = false;
            if (Solved()) StartCoroutine(Sorted());
        }

        IEnumerator Refuse(int t)
        {
            busy = true;
            Sfx.Play(this, "buzz", 0.4f);
            Sfx.Buzz();
            Transform ball = Obj(lifted);
            yield return Move(ball, Hover(t), 0.12f);
            Vector3 p = ball.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                ball.position = p + Vector3.right * Mathf.Sin(k * Mathf.PI * 6f) * 0.12f * (1f - k);
                yield return null;
            }
            int from = liftedFrom;
            busy = false;
            Drop(from);   // torna nella sua provetta
        }

        IEnumerator Move(Transform t, Vector3 to, float seconds)
        {
            Vector3 a = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.position = Vector3.Lerp(a, to, 1f - (1f - k) * (1f - k));
                yield return null;
            }
            t.position = to;
        }

        bool Solved()
        {
            if (!hamsterGone) return false;
            foreach (List<int> l in tube) if (l.Count != 0 && !Complete(l)) return false;
            return true;
        }

        IEnumerator ResetRoutine()
        {
            busy = true;
            Sfx.Play(this, "pop", 0.7f);
            Vector3 s = resetButton.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.18f)
            {
                resetButton.localScale = s * (1f + Mathf.Sin(k * Mathf.PI) * 0.15f);
                yield return null;
            }
            resetButton.localScale = s;
            Load(startLayout);
            busy = false;
        }

        /// <summary>Il criceto si srotola, protesta e scappa: da qui in poi la partenza non lo contiene più.</summary>
        IEnumerator HamsterEscape()
        {
            busy = true;
            Transform h = hamster.transform;
            yield return Move(h, Hover(liftedFrom) + Vector3.up * 0.2f, 0.15f);
            hamster.sprite = hamsterRun;
            h.localScale = Vector3.one * hamsterRunScale;
            Sfx.Play(this, "pop", 0.9f);
            Haptics.Light();
            if (bubble != null) bubble.Show("Hey! I'm not a ball!", true);
            yield return new WaitForSeconds(1.1f);
            float dir = h.position.x >= 0f ? 1f : -1f;
            hamster.flipX = dir > 0f;   // il disegno guarda a sinistra
            Vector3 a = h.position, b = new Vector3(dir * 3.8f, -3.85f, a.z);
            Sfx.Play(this, "whoosh", 0.6f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                Vector3 p = Vector3.Lerp(a, b, k);
                p.y += Mathf.Abs(Mathf.Sin(k * Mathf.PI * 4f)) * 0.3f;
                h.position = p;
                yield return null;
            }
            h.gameObject.SetActive(false);
            hamsterGone = true;
            lifted = liftedFrom = -1;
            foreach (List<int> l in startLayout) l.Remove(HamsterId);
            if (Distance(startLayout) < 0) startLayout = Generate(false);   // RESET deve restare risolvibile
            if (bubble != null) bubble.Hide();
            busy = false;
            if (Solved()) StartCoroutine(Sorted());
        }

        IEnumerator Sorted()
        {
            done = true;
            Sfx.Play(this, "ding", 0.8f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                for (int i = 0; i < 4; i++)
                    for (int s = 0; s < tube[i].Count; s++)
                        Obj(tube[i][s]).position = Slot(i, s) + Vector3.up * Mathf.Abs(Mathf.Sin((k * 2f - s * 0.12f - i * 0.05f) * Mathf.PI)) * 0.15f;
                yield return null;
            }
            for (int i = 0; i < 4; i++) for (int s = 0; s < tube[i].Count; s++) Obj(tube[i][s]).position = Slot(i, s);
            // il criceto torna a curiosare
            Transform h = hamster.transform;
            h.gameObject.SetActive(true);
            hamster.flipX = false;
            Vector3 a = new Vector3(3.6f, -3.85f, h.position.z), b = new Vector3(1.9f, -3.85f, h.position.z);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                h.position = Vector3.Lerp(a, b, k) + Vector3.up * Mathf.Abs(Mathf.Sin(k * Mathf.PI * 3f)) * 0.15f;
                yield return null;
            }
            h.position = b;
            if (bubble != null) bubble.Show("So tidy! Can I nap in the empty tube?", true);
            yield return new WaitForSeconds(1.6f);
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done && !busy) bubble.Hide();
        }
    }
}
