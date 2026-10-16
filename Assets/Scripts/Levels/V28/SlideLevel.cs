using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 89 — il puzzle scorrevole. Il ritratto del gatto è a pezzi: tocca un tassello vicino al buco per farlo
    /// scorrere. Rimettilo in ordine prima che il gatto si svegli (barra del tempo). Ci sono i numerini negli angoli.
    /// </summary>
    public class SlideLevel : LevelController
    {
        [SerializeField] SpriteRenderer picture;      // disegno intero (nascosto): da qui si ritagliano i tasselli
        [SerializeField] SpriteRenderer tileTemplate;
        [SerializeField] int size = 3;
        [SerializeField] float tileSize = 1.4f;
        [SerializeField] Vector2 center = new Vector2(0, 0.2f);
        [SerializeField] Transform timeBar;
        [SerializeField] TextMesh moves;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float seconds = 120f;

        int[] board;                                  // board[cella] = tassello (size*size-1 = buco)
        Transform[] tiles;
        int hole, moveCount;
        float clock;
        bool done;
        Camera cam;

        Vector3 Pos(int cellIndex)
        {
            int x = cellIndex % size, y = cellIndex / size;
            return new Vector3(center.x + (x - (size - 1) * 0.5f) * tileSize, center.y + ((size - 1) * 0.5f - y) * tileSize, 0f);
        }

        public override void OnLevelStart()
        {
            cam = Camera.main;
            int n = size * size;
            board = new int[n];
            tiles = new Transform[n];
            Sprite whole = picture.sprite;
            Rect r = whole.textureRect;
            float w = r.width / size, h = r.height / size;
            for (int i = 0; i < n - 1; i++)
            {
                int x = i % size, y = i / size;
                Sprite part = Sprite.Create(whole.texture, new Rect(r.x + x * w, r.y + (size - 1 - y) * h, w, h), new Vector2(0.5f, 0.5f), w / tileSize);
                SpriteRenderer sr = Instantiate(tileTemplate, transform);
                sr.gameObject.SetActive(true);
                sr.sprite = part;
                TextMesh num = sr.GetComponentInChildren<TextMesh>();
                if (num != null) num.text = (i + 1).ToString();
                tiles[i] = sr.transform;
            }
            picture.gameObject.SetActive(false);
            for (int i = 0; i < n; i++) board[i] = i;
            hole = n - 1;
            // mescola con mosse valide (così è sempre risolvibile)
            int last = -1;
            for (int k = 0; k < 120; k++)
            {
                int[] nb = Neighbours(hole);
                int pick;
                do pick = nb[Random.Range(0, nb.Length)]; while (pick == last && nb.Length > 1);
                last = hole;
                Swap(pick);
            }
            if (Solved()) Swap(Neighbours(hole)[0]);
            for (int i = 0; i < n; i++) if (board[i] < n - 1) tiles[board[i]].position = Pos(i);
            moveCount = 0;
            Moves();
            if (bubble != null) { bubble.Show("Fix my portrait before I wake up!"); StartCoroutine(HideLater(2.2f)); }
        }

        int[] Neighbours(int c)
        {
            var list = new System.Collections.Generic.List<int>();
            int x = c % size, y = c / size;
            if (x > 0) list.Add(c - 1);
            if (x < size - 1) list.Add(c + 1);
            if (y > 0) list.Add(c - size);
            if (y < size - 1) list.Add(c + size);
            return list.ToArray();
        }

        void Swap(int cellIndex)
        {
            board[hole] = board[cellIndex];
            board[cellIndex] = size * size - 1;
            hole = cellIndex;
        }

        bool Solved()
        {
            for (int i = 0; i < board.Length; i++) if (board[i] != i) return false;
            return true;
        }

        void Moves() { if (moves != null) moves.text = "MOVES: " + moveCount; }

        void Update()
        {
            if (done || IsFinished) return;
            clock += Time.deltaTime;
            if (timeBar != null) timeBar.localScale = new Vector3(Mathf.Clamp01(1f - clock / seconds), 1f, 1f);
            for (int i = 0; i < board.Length; i++)
                if (board[i] < board.Length - 1) tiles[board[i]].position = Vector3.Lerp(tiles[board[i]].position, Pos(i), 1f - Mathf.Exp(-18f * Time.deltaTime));
            if (clock >= seconds) { StartCoroutine(Woke()); return; }
            if (!PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            foreach (int c in Neighbours(hole))
            {
                if (Vector2.Distance(w, Pos(c)) > tileSize * 0.5f) continue;
                Swap(c);
                moveCount++;
                Moves();
                Sfx.Play(this, "tick");
                if (Solved()) StartCoroutine(Complete());
                return;
            }
        }

        IEnumerator Complete()
        {
            done = true;
            yield return new WaitForSeconds(0.3f);
            picture.gameObject.SetActive(true);
            foreach (Transform t in tiles) if (t != null) t.gameObject.SetActive(false);
            Sfx.Play(this, "ding", 0.6f);
            if (bubble != null) bubble.Show("Purrfect. In " + moveCount + " moves!");
            yield return new WaitForSeconds(1.5f);
            Win();
        }

        IEnumerator Woke()
        {
            done = true;
            if (bubble != null) bubble.Show("MEOW?! Who broke my portrait?!");
            yield return new WaitForSeconds(1.5f);
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
