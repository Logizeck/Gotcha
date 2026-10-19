using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 89 — il puzzle scorrevole. Il ritratto di Toni (il carlino nero) è a pezzi: tocca un tassello vicino
    /// al buco per farlo scorrere. Rimettilo in ordine prima che Toni si svegli (barra del tempo). Ci sono i numerini
    /// negli angoli. Finito il ritratto, Toni prende vita, si spiaccica contro il vetro e LECCA lo schermo.
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
        [SerializeField] GameObject mouthOpen;        // bocca aperta del ritratto (figlio del disegno)
        [SerializeField] Transform tongue;            // perno della linguona, sulla bocca
        [SerializeField] SpriteRenderer smearTemplate;
        [SerializeField] float tongueLength = 2.2f;

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
            if (mouthOpen != null) mouthOpen.SetActive(false);
            if (tongue != null) tongue.gameObject.SetActive(false);
            if (smearTemplate != null) smearTemplate.gameObject.SetActive(false);
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
            if (bubble != null) bubble.Show("In " + moveCount + " moves! Wait... it's alive?!");
            // il ritratto prende vita: due saltelli, poi Toni si spiaccica contro il vetro
            Transform pic = picture.transform;
            Vector3 size = pic.localScale, home = pic.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                pic.localScale = size * (1f + 0.07f * Mathf.Abs(Mathf.Sin(k * Mathf.PI * 2f)));
                yield return null;
            }
            Sfx.Play(this, "woof", 0.9f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                pic.localScale = size * Mathf.Lerp(1f, 1.22f, Mathf.SmoothStep(0f, 1f, k));
                pic.position = home + Vector3.up * 0.25f * k;
                yield return null;
            }
            if (bubble != null) bubble.Hide();
            if (mouthOpen != null) mouthOpen.SetActive(true);
            if (tongue != null && smearTemplate != null) yield return Lick();
            if (mouthOpen != null) mouthOpen.SetActive(false);
            if (bubble != null) bubble.Show("SLURP! Woof!");
            yield return new WaitForSeconds(1.4f);
            Win();
        }

        /// <summary>La linguona esce e spazza lo schermo tre volte, lasciando strisciate di bava che svaniscono.</summary>
        IEnumerator Lick()
        {
            var smears = new System.Collections.Generic.List<SpriteRenderer>();
            tongue.position = mouthOpen.transform.position;
            tongue.gameObject.SetActive(true);
            float big = 1.45f;
            tongue.localRotation = Quaternion.Euler(0, 0, 60f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { tongue.localScale = Vector3.one * big * k; yield return null; }
            tongue.localScale = Vector3.one * big;
            float from = 60f;
            for (int sweep = 0; sweep < 3; sweep++)
            {
                float to = -from;
                Sfx.Play(this, "whoosh", 0.8f);
                Haptics.Light();
                float drop = 0f;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
                {
                    float a = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, k));
                    tongue.localRotation = Quaternion.Euler(0, 0, a);
                    // la punta si allarga un po' quando preme sul vetro
                    tongue.localScale = new Vector3(big * (1f + 0.25f * Mathf.Sin(k * Mathf.PI)), big, 1f);
                    drop -= Time.deltaTime;
                    if (drop <= 0f)
                    {
                        drop = 0.04f;
                        SpriteRenderer sm = Instantiate(smearTemplate, transform);
                        sm.gameObject.SetActive(true);
                        sm.transform.position = tongue.position + tongue.up * tongueLength * big * 0.85f;
                        sm.transform.rotation = Quaternion.Euler(0, 0, a + 90f + Random.Range(-10f, 10f));
                        sm.transform.localScale = Vector3.one * Random.Range(1.0f, 1.5f);
                        smears.Add(sm);
                    }
                    yield return null;
                }
                from = to;
            }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { tongue.localScale = Vector3.one * big * (1f - k); yield return null; }
            tongue.gameObject.SetActive(false);
            // la bava sfuma via
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.2f)
            {
                foreach (SpriteRenderer sm in smears) sm.color = new Color(1f, 1f, 1f, 1f - k);
                yield return null;
            }
            foreach (SpriteRenderer sm in smears) Destroy(sm.gameObject);
        }

        IEnumerator Woke()
        {
            done = true;
            if (bubble != null) bubble.Show("WOOF?! Who broke my portrait?!");
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
