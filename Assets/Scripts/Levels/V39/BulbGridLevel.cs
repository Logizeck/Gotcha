using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Bulb grid — "Lights Out" in soffitta. Il topolino vuole leggere la mappa ma è buio: nove lampadine 3x3.
    /// Toccarne una accende/spegne lei E le vicine sopra/sotto/destra/sinistra. Accendile tutte e nove.
    /// La partenza si ottiene premendo 3-4 lampadine diverse da "tutto acceso": la soluzione è premere di nuovo
    /// proprio quelle (ogni pressione si annulla da sola). RESET riporta alla partenza; oltre 25 mosse il topolino
    /// si arrende ridendo e si ricomincia.
    /// </summary>
    public class BulbGridLevel : LevelController
    {
        [SerializeField] SpriteRenderer[] bulbs;        // indice riga*3+colonna, riga 0 in alto
        [SerializeField] SpriteRenderer[] glows;
        [SerializeField] Sprite bulbOn, bulbOff;
        [SerializeField] SpriteRenderer darkness;
        [SerializeField] Transform resetButton;
        [SerializeField] TextMesh moves;
        [SerializeField] Transform map;
        [SerializeField] Transform mouse;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] Vector2 gridCenter = new Vector2(0f, 0.6f);
        [SerializeField] float cell = 1.4f;
        [SerializeField] int maxMoves = 25;

        readonly bool[] lit = new bool[9];
        readonly bool[] start = new bool[9];
        int moveCount;
        bool busy, done;
        Camera cam;
        Vector3 mapScale;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            mapScale = map.localScale;
            map.gameObject.SetActive(false);
            for (int i = 0; i < 9; i++) lit[i] = true;
            // 3 o 4 pressioni diverse partendo da "tutto acceso": mai risolto in partenza (la matrice 3x3 è invertibile)
            int presses = Random.Range(3, 5);
            var used = new bool[9];
            for (int k = 0; k < presses; k++)
            {
                int c;
                do c = Random.Range(0, 9); while (used[c]);
                used[c] = true;
                Press(c);
            }
            for (int i = 0; i < 9; i++) start[i] = lit[i];
            moveCount = 0;
            Refresh();
            if (bubble != null) { bubble.Show("Too dark to read my map! Light ALL the bulbs!", true); StartCoroutine(HideLater(2.6f)); }
        }

        void Press(int c)
        {
            int r = c / 3, k = c % 3;
            Toggle(r, k); Toggle(r - 1, k); Toggle(r + 1, k); Toggle(r, k - 1); Toggle(r, k + 1);
        }

        void Toggle(int r, int k)
        {
            if (r < 0 || r > 2 || k < 0 || k > 2) return;
            lit[r * 3 + k] = !lit[r * 3 + k];
        }

        int LitCount()
        {
            int n = 0;
            foreach (bool b in lit) if (b) n++;
            return n;
        }

        void Refresh()
        {
            for (int i = 0; i < 9; i++)
            {
                bulbs[i].sprite = lit[i] ? bulbOn : bulbOff;
                glows[i].enabled = lit[i];
            }
            Color d = darkness.color;
            d.a = Mathf.Lerp(0.82f, 0.38f, LitCount() / 9f);
            darkness.color = d;
            moves.text = "MOVES: " + moveCount;
        }

        void Update()
        {
            if (busy || done || IsFinished) return;
            if (!PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (Vector2.Distance(w, resetButton.position) < 0.6f) { ResetBoard(); return; }
            Vector2 g = (w - gridCenter) / cell;
            int col = Mathf.RoundToInt(g.x) + 1, row = 1 - Mathf.RoundToInt(g.y);
            if (col < 0 || col > 2 || row < 0 || row > 2) return;
            if (Mathf.Abs(g.x - (col - 1)) > 0.48f || Mathf.Abs(g.y - (1 - row)) > 0.48f) return;
            int c = row * 3 + col;
            Press(c);
            moveCount++;
            Refresh();
            Sfx.Play(this, "tick", 0.8f);
            StartCoroutine(Pop(bulbs[c].transform.parent));
            if (LitCount() == 9) { StartCoroutine(Bright()); return; }
            if (moveCount > maxMoves) StartCoroutine(GiveUp());
        }

        void ResetBoard()
        {
            for (int i = 0; i < 9; i++) lit[i] = start[i];
            moveCount = 0;
            Refresh();
            Sfx.Play(this, "pop", 0.7f);
            StartCoroutine(Pop(resetButton));
        }

        IEnumerator Pop(Transform t)
        {
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.18f)
            {
                t.localScale = s * (1f + Mathf.Sin(k * Mathf.PI) * 0.15f);
                yield return null;
            }
            t.localScale = s;
        }

        IEnumerator GiveUp()
        {
            busy = true;
            Sfx.Play(this, "buzz", 0.5f);
            if (bubble != null) bubble.Show("Blink blink... I'm getting a disco headache. Again!", true);
            yield return new WaitForSeconds(1.8f);
            ResetBoard();
            if (bubble != null) bubble.Hide();
            busy = false;
        }

        IEnumerator Bright()
        {
            done = true;
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Light();
            Color d = darkness.color;
            float a0 = d.a;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
            {
                d.a = Mathf.Lerp(a0, 0f, k);
                darkness.color = d;
                yield return null;
            }
            d.a = 0f;
            darkness.color = d;
            map.gameObject.SetActive(true);
            Sfx.Play(this, "pop", 0.8f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                map.localScale = mapScale * (Mathf.Sin(k * Mathf.PI * 0.75f) / Mathf.Sin(Mathf.PI * 0.75f));
                yield return null;
            }
            map.localScale = mapScale;
            if (bubble != null) bubble.Show("Light! And the map says... CHEESE this way!", true);
            Vector3 m0 = mouse.localPosition;
            for (float k = 0f; k < 1.4f; k += Time.deltaTime)
            {
                mouse.localPosition = m0 + Vector3.up * Mathf.Abs(Mathf.Sin(k * 9f)) * 0.15f;   // salta di gioia
                yield return null;
            }
            mouse.localPosition = m0;
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done && !busy) bubble.Hide();
        }
    }
}
