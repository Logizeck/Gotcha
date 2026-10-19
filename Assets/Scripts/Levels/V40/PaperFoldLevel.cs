using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Fold It — la mappa del tesoro è stampata su un foglio con tre lembi (sinistra, destra, in alto) separati da
    /// pieghe tratteggiate. Si striscia il dito attraverso una piega per ripiegare quel lembo sul centro: sul retro
    /// dei lembi ci sono altri pezzi di sentiero. L'ordine conta (l'ultimo lembo piegato sta sopra). Soluzione:
    /// piegare prima il lembo DESTRO (il sentiero sale dalla bandierina) e poi quello in ALTO (porta la X proprio
    /// in cima al sentiero). Il lembo sinistro, con la sua grossa X rossa, è un tranello: copre la partenza.
    /// UNFOLD rimette tutto com'era.
    /// </summary>
    public class PaperFoldLevel : LevelController
    {
        [SerializeField] Transform[] pivots;          // 0 sinistra, 1 destra, 2 in alto: perno sulla linea di piega
        [SerializeField] GameObject[] fronts;
        [SerializeField] GameObject[] backs;
        [SerializeField] Transform[] swipeHints;      // frecce che suggeriscono di strisciare sulle pieghe
        [SerializeField] TapButton unfoldButton;
        [SerializeField] Transform xGlow;
        [SerializeField] Transform walker;
        [SerializeField] Transform[] walkPath;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int[] solution = { 1, 2 };
        [SerializeField] float lineHalf = 1.25f;
        [SerializeField] float foldTime = 0.45f;

        readonly List<int> folds = new List<int>();
        SpriteRenderer[][] flapArt;
        int[][] baseOrder;
        bool busy, done, swiping, hintsGone;
        Vector2 swipeStart;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            flapArt = new SpriteRenderer[pivots.Length][];
            baseOrder = new int[pivots.Length][];
            for (int i = 0; i < pivots.Length; i++)
            {
                flapArt[i] = pivots[i].GetComponentsInChildren<SpriteRenderer>(true);
                baseOrder[i] = new int[flapArt[i].Length];
                for (int k = 0; k < flapArt[i].Length; k++) baseOrder[i][k] = flapArt[i][k].sortingOrder;
                SetFlap(i, 1f);
            }
            xGlow.gameObject.SetActive(false);
            walker.gameObject.SetActive(false);
            unfoldButton.Pressed += _ => { if (!busy && !done && folds.Count > 0) StartCoroutine(Unfold()); };
            if (bubble != null) { bubble.Show("Fold the map so the path reaches the X!"); StartCoroutine(HideLater(2.4f)); }
        }

        bool Vertical(int i) => i < 2;

        /// <summary>s = 1 aperto, -1 piegato; in mezzo il lembo è "di taglio" (effetto 3D con le scale).</summary>
        void SetFlap(int i, float s)
        {
            float bulge = 1f + 0.08f * (1f - Mathf.Abs(s));
            pivots[i].localScale = Vertical(i) ? new Vector3(s, bulge, 1f) : new Vector3(bulge, s, 1f);
            fronts[i].SetActive(s >= 0f);
            backs[i].SetActive(s < 0f);
            float light = 0.68f + 0.32f * Mathf.Abs(s);
            foreach (SpriteRenderer r in flapArt[i]) r.color = new Color(light, light, light, 1f);
        }

        void Update()
        {
            if (IsFinished || done) return;
            // frecce suggerimento che pulsano finché non si piega qualcosa
            if (!hintsGone)
                foreach (Transform h in swipeHints) h.localScale = Vector3.one * (1f + 0.12f * Mathf.Sin(Time.time * 6f));
            if (busy) { swiping = false; return; }
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _)) { swiping = false; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began) { swiping = !PointerInput.IsOverUi(); swipeStart = w; return; }
            if (!swiping || (w - swipeStart).magnitude < 0.35f) return;
            int line = CrossedLine(swipeStart, w);
            if (line < 0) return;
            swiping = false;
            StartCoroutine(Fold(line));
        }

        /// <summary>La prima linea di piega (ancora aperta) attraversata dal tratto a→b, o -1.</summary>
        int CrossedLine(Vector2 a, Vector2 b)
        {
            int best = -1;
            float bestT = 2f;
            for (int i = 0; i < pivots.Length; i++)
            {
                if (folds.Contains(i)) continue;
                Vector2 h = pivots[i].position;
                float pa = Vertical(i) ? a.x - h.x : a.y - h.y, pb = Vertical(i) ? b.x - h.x : b.y - h.y;
                if (pa * pb > 0f || Mathf.Approximately(pa, pb)) continue;
                float t = pa / (pa - pb);
                Vector2 hit = Vector2.Lerp(a, b, t);
                float along = Vertical(i) ? hit.y - h.y : hit.x - h.x;
                if (Mathf.Abs(along) > lineHalf + 0.2f) continue;
                if (t < bestT) { bestT = t; best = i; }
            }
            return best;
        }

        void SetOrder(int i, int extra)
        {
            for (int k = 0; k < flapArt[i].Length; k++) flapArt[i][k].sortingOrder = baseOrder[i][k] + extra;
        }

        IEnumerator Fold(int i)
        {
            busy = true;
            if (!hintsGone) { hintsGone = true; foreach (Transform h in swipeHints) h.gameObject.SetActive(false); }
            if (bubble != null) bubble.Hide();
            SetOrder(i, 10 + folds.Count * 4);   // l'ultimo piegato sta sopra
            Sfx.Play(this, "whoosh", 0.6f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / foldTime)
            {
                SetFlap(i, Mathf.Cos(Mathf.SmoothStep(0f, 1f, k) * Mathf.PI));
                yield return null;
            }
            SetFlap(i, -1f);
            Sfx.Play(this, "tick", 0.8f);
            Haptics.Light();
            folds.Add(i);
            if (Solved()) { StartCoroutine(Treasure()); yield break; }
            if (folds.Count >= 2 && bubble != null)
            {
                string msg = folds.Contains(0) ? "Oops, that flap hid the start!" : "The path doesn't line up...";
                bubble.Show(msg + (folds.Count >= pivots.Length ? "\nTap UNFOLD!" : ""));
            }
            busy = false;
        }

        bool Solved()
        {
            if (folds.Count != solution.Length) return false;
            for (int k = 0; k < solution.Length; k++) if (folds[k] != solution[k]) return false;
            return true;
        }

        IEnumerator Unfold()
        {
            busy = true;
            if (bubble != null) bubble.Hide();
            Sfx.Play(this, "whoosh", 0.5f);
            for (int n = folds.Count - 1; n >= 0; n--)
            {
                int i = folds[n];
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
                {
                    SetFlap(i, -Mathf.Cos(k * Mathf.PI));
                    yield return null;
                }
                SetFlap(i, 1f);
                SetOrder(i, 0);
            }
            folds.Clear();
            busy = false;
        }

        IEnumerator Treasure()
        {
            done = true;
            unfoldButton.interactable = false;
            if (bubble != null) bubble.Show("The path joins up!");
            // l'esploratore percorre il sentiero fino alla X
            walker.gameObject.SetActive(true);
            walker.position = walkPath[0].position;
            for (int n = 1; n < walkPath.Length; n++)
            {
                Vector3 a = walkPath[n - 1].position, b = walkPath[n].position;
                float seconds = Vector3.Distance(a, b) / 1.4f;
                for (float k = 0f; k < 1f; k += Time.deltaTime / Mathf.Max(0.01f, seconds))
                {
                    walker.position = Vector3.Lerp(a, b, k) + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * 14f)) * 0.05f;
                    yield return null;
                }
                Sfx.Play(this, "tick", 0.3f);
            }
            walker.position = walkPath[walkPath.Length - 1].position;
            // la X si accende
            xGlow.gameObject.SetActive(true);
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Medium();
            Vector3 size = xGlow.localScale;
            for (float t = 0f; t < 1.6f; t += Time.deltaTime)
            {
                float pop = t < 0.25f ? Mathf.Lerp(0.2f, 1.25f, t / 0.25f) : 1f + 0.12f * Mathf.Sin(t * 10f);
                xGlow.localScale = size * pop;
                xGlow.localRotation = Quaternion.Euler(0, 0, t * 40f);
                yield return null;
            }
            if (bubble != null) bubble.Show("X marks the spot! Treasure!");
            yield return new WaitForSeconds(1.2f);
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done && !busy && folds.Count == 0) bubble.Hide();
        }
    }
}
