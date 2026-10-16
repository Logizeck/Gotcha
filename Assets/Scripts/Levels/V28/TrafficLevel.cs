using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 98 — l'ingorgo. Il furgoncino rosso deve uscire dal parcheggio (a destra, nella sua fila). Trascina le
    /// auto avanti e indietro nella loro direzione per liberargli la strada. Due minuti prima che arrivi il vigile.
    /// </summary>
    public class TrafficLevel : LevelController
    {
        [SerializeField] Transform[] cars;            // la prima è quella rossa
        [SerializeField] int[] col, row, length;      // cella in alto a sinistra (riga 0 in alto)
        [SerializeField] bool[] horizontal;
        [SerializeField] Vector2 origin = new Vector2(-2.25f, 2.25f);   // centro della cella (0,0)
        [SerializeField] float cell = 0.9f;
        [SerializeField] Transform timeBar;
        [SerializeField] TextMesh moves;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float seconds = 150f;

        int dragging = -1, moveCount;
        float grabOffset, minPos, maxPos, clock;
        bool done;
        Camera cam;

        Vector3 Place(int i, float c, float r)
        {
            float x = origin.x + (c + (horizontal[i] ? (length[i] - 1) * 0.5f : 0f)) * cell;
            float y = origin.y - (r + (horizontal[i] ? 0f : (length[i] - 1) * 0.5f)) * cell;
            return new Vector3(x, y, 0f);
        }

        public override void OnLevelStart()
        {
            cam = Camera.main;
            for (int i = 0; i < cars.Length; i++) cars[i].position = Place(i, col[i], row[i]);
            Moves();
            if (bubble != null) { bubble.Show("Get my red van out! (right side)"); StartCoroutine(HideLater(2f)); }
        }

        bool Occupied(int c, int r, int except)
        {
            for (int i = 0; i < cars.Length; i++)
            {
                if (i == except) continue;
                for (int k = 0; k < length[i]; k++)
                    if ((horizontal[i] ? col[i] + k : col[i]) == c && (horizontal[i] ? row[i] : row[i] + k) == r) return true;
            }
            return false;
        }

        void Range(int i)
        {
            int lo = horizontal[i] ? col[i] : row[i], hi = lo;
            while (lo > 0 && !(horizontal[i] ? Occupied(lo - 1, row[i], i) : Occupied(col[i], lo - 1, i))) lo--;
            while (hi + length[i] < 6 && !(horizontal[i] ? Occupied(hi + length[i], row[i], i) : Occupied(col[i], hi + length[i], i))) hi++;
            minPos = lo; maxPos = hi;
        }

        void Moves() { if (moves != null) moves.text = "MOVES: " + moveCount; }

        void Update()
        {
            if (done || IsFinished) return;
            clock += Time.deltaTime;
            if (timeBar != null) timeBar.localScale = new Vector3(Mathf.Clamp01(1f - clock / seconds), 1f, 1f);
            if (clock >= seconds) { StartCoroutine(Ticket()); return; }
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { Release(); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                for (int i = 0; i < cars.Length; i++)
                {
                    Vector3 p = cars[i].position;
                    float hw = (horizontal[i] ? length[i] : 1) * cell * 0.5f, hh = (horizontal[i] ? 1 : length[i]) * cell * 0.5f;
                    if (Mathf.Abs(w.x - p.x) > hw || Mathf.Abs(w.y - p.y) > hh) continue;
                    dragging = i;
                    Range(i);
                    grabOffset = horizontal[i] ? (w.x - origin.x) / cell - col[i] : -(w.y - origin.y) / cell - row[i];
                    break;
                }
            }
            if (dragging >= 0)
            {
                int i = dragging;
                float pos = (horizontal[i] ? (w.x - origin.x) / cell : -(w.y - origin.y) / cell) - grabOffset;
                pos = Mathf.Clamp(pos, minPos, maxPos);
                cars[i].position = horizontal[i] ? Place(i, pos, row[i]) : Place(i, col[i], pos);
                if (ended) Release();
            }
        }

        void Release()
        {
            if (dragging < 0) return;
            int i = dragging;
            dragging = -1;
            float pos = horizontal[i] ? (cars[i].position.x - origin.x) / cell - (length[i] - 1) * 0.5f : -(cars[i].position.y - origin.y) / cell - (length[i] - 1) * 0.5f;
            int snapped = Mathf.RoundToInt(pos);
            int old = horizontal[i] ? col[i] : row[i];
            if (horizontal[i]) col[i] = snapped; else row[i] = snapped;
            cars[i].position = Place(i, col[i], row[i]);
            if (snapped != old) { moveCount++; Moves(); Sfx.Play(this, "tick"); }
            if (i == 0 && col[0] + length[0] >= 6) StartCoroutine(Out());
        }

        IEnumerator Out()
        {
            done = true;
            Sfx.Play(this, "whoosh");
            Vector3 p = cars[0].position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f) { cars[0].position = p + Vector3.right * k * k * 4f; yield return null; }
            if (bubble != null) bubble.Show("Free! " + moveCount + " moves.");
            yield return new WaitForSeconds(1.3f);
            Win();
        }

        IEnumerator Ticket()
        {
            done = true;
            if (bubble != null) bubble.Show("*TWEET* Parking ticket!");
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
