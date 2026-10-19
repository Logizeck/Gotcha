using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Untangle — il gatto ha giocato col gomitolo: sette bottoni legati da fili di lana che si incrociano.
    /// Trascina i bottoni finché nessun filo ne incrocia un altro (i fili incrociati sono rossi, quelli liberi verdi).
    /// Il grafo è planare (un esagono con un bottone al centro e quattro raggi); all'inizio i bottoni sono messi in
    /// cerchio in ordine casuale. Soluzione: un bottone al centro, gli altri sei in giro nell'ordine dell'anello.
    /// </summary>
    public class UntangleLevel : LevelController
    {
        [SerializeField] Transform[] nodes;            // 7 bottoni
        [SerializeField] LineRenderer[] strings;       // un filo per lato
        // estremi dei fili: anello 1-2-3-4-5-6 più quattro raggi dal bottone 0 (grafo planare)
        [SerializeField] int[] edgeA = { 1, 2, 3, 4, 5, 6, 0, 0, 0, 0 };
        [SerializeField] int[] edgeB = { 2, 3, 4, 5, 6, 1, 1, 2, 4, 5 };
        [SerializeField] Vector2 circleCenter = new Vector2(0f, 0.4f);
        [SerializeField] float circleRadius = 2.1f;
        [SerializeField] Rect area = new Rect(-2.45f, -2.6f, 4.9f, 5.9f);
        [SerializeField] float grabRadius = 0.5f;
        [SerializeField] Color woolFree = new Color(0.45f, 0.78f, 0.45f), woolCrossed = new Color(0.92f, 0.27f, 0.25f), woolGlow = new Color(1f, 0.85f, 0.3f);
        [SerializeField] Transform cat;
        [SerializeField] SpeechBubble bubble;

        int dragging = -1;
        Vector2 grabOffset;
        bool done;
        bool[] crossed;
        Camera cam;
        Vector3 nodeScale;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            crossed = new bool[strings.Length];
            nodeScale = nodes[0].localScale;
            // bottoni in cerchio in ordine casuale, con abbastanza incroci
            int[] perm = new int[nodes.Length], best = null;
            int bestCount = -1;
            float a0 = Random.Range(0f, 360f);
            for (int attempt = 0; attempt < 200; attempt++)
            {
                for (int i = 0; i < perm.Length; i++) perm[i] = i;
                for (int i = perm.Length - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (perm[i], perm[j]) = (perm[j], perm[i]); }
                Place(perm, a0);
                int n = CountCrossings();
                if (n > bestCount) { bestCount = n; best = (int[])perm.Clone(); }
                if (n >= 5 && n <= 9) { best = (int[])perm.Clone(); break; }   // ingarbugliato ma non troppo
            }
            Place(best, a0);
            Redraw();
            if (bubble != null) { bubble.Show("Oops. Untangle my yarn? No strings may cross!", true); StartCoroutine(HideLater(2.6f)); }
        }

        void Place(int[] perm, float a0)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                float a = (a0 + i * 360f / nodes.Length) * Mathf.Deg2Rad;
                nodes[perm[i]].position = (Vector3)(circleCenter + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * circleRadius);
            }
        }

        static bool Cross(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float d1 = Orient(c, d, a), d2 = Orient(c, d, b), d3 = Orient(a, b, c), d4 = Orient(a, b, d);
            return ((d1 > 0f && d2 < 0f) || (d1 < 0f && d2 > 0f)) && ((d3 > 0f && d4 < 0f) || (d3 < 0f && d4 > 0f));
        }

        static float Orient(Vector2 a, Vector2 b, Vector2 c) => (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);

        int CountCrossings()
        {
            int n = 0;
            for (int e = 0; e < strings.Length; e++) crossed[e] = false;
            for (int e = 0; e < strings.Length; e++)
                for (int f = e + 1; f < strings.Length; f++)
                {
                    if (edgeA[e] == edgeA[f] || edgeA[e] == edgeB[f] || edgeB[e] == edgeA[f] || edgeB[e] == edgeB[f]) continue;
                    if (!Cross(nodes[edgeA[e]].position, nodes[edgeB[e]].position, nodes[edgeA[f]].position, nodes[edgeB[f]].position)) continue;
                    crossed[e] = crossed[f] = true;
                    n++;
                }
            return n;
        }

        void Redraw()
        {
            CountCrossings();
            for (int e = 0; e < strings.Length; e++)
            {
                strings[e].SetPosition(0, nodes[edgeA[e]].position);
                strings[e].SetPosition(1, nodes[edgeB[e]].position);
                if (!done) strings[e].startColor = strings[e].endColor = crossed[e] ? woolCrossed : woolFree;
            }
        }

        void Update()
        {
            if (done || IsFinished) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended))
            {
                if (dragging >= 0) Drop();
                return;
            }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                float best = grabRadius;
                dragging = -1;
                for (int i = 0; i < nodes.Length; i++)
                {
                    float d = Vector2.Distance(w, nodes[i].position);
                    if (d < best) { best = d; dragging = i; }
                }
                if (dragging >= 0)
                {
                    grabOffset = (Vector2)nodes[dragging].position - w;
                    nodes[dragging].localScale = nodeScale * 1.2f;
                    Sfx.Play(this, "tick", 0.6f);
                }
            }
            if (dragging < 0) return;
            Vector2 p = w + grabOffset;
            p.x = Mathf.Clamp(p.x, area.xMin, area.xMax);
            p.y = Mathf.Clamp(p.y, area.yMin, area.yMax);
            nodes[dragging].position = p;
            Redraw();
            if (ended) Drop();
        }

        void Drop()
        {
            nodes[dragging].localScale = nodeScale;
            dragging = -1;
            Sfx.Play(this, "pop", 0.4f);
            if (CountCrossings() == 0) StartCoroutine(Purr());
        }

        IEnumerator Purr()
        {
            done = true;
            Sfx.Play(this, "ding", 0.8f);
            if (bubble != null) bubble.Show("Purrrrr... so neat!", true);
            float w0 = strings[0].widthMultiplier;
            Vector3 c0 = cat.localScale;
            for (float k = 0f; k < 2f; k += Time.deltaTime)
            {
                float g = 0.5f + 0.5f * Mathf.Sin(k * 8f);
                foreach (LineRenderer s in strings)
                {
                    s.startColor = s.endColor = Color.Lerp(woolFree, woolGlow, g);
                    s.widthMultiplier = w0 * (1f + 0.4f * g);
                }
                cat.localScale = new Vector3(c0.x, c0.y * (1f + Mathf.Sin(k * 30f) * 0.015f), c0.z);   // fusa
                if (Mathf.Repeat(k, 0.6f) < Time.deltaTime) Sfx.Play(this, "snore", 0.35f);
                yield return null;
            }
            cat.localScale = c0;
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
