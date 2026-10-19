using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 54 — il puntatore laser. Il gatto dorme e l'unico modo di svegliarlo è il puntino rosso. Il raggio
    /// parte dalla penna laser e rimbalza sugli specchietti: tocca uno specchio per girarlo ( / oppure \ ). I vasi
    /// fermano il raggio. Le batterie della penna bastano per un numero limitato di mosse.
    /// </summary>
    public class LaserLevel : LevelController
    {
        [SerializeField] Transform[] mirrors;         // in celle della griglia
        [SerializeField] Transform[] vases;
        [SerializeField] Transform cat;
        [SerializeField] GameObject catEyes;
        [SerializeField] Transform emitter;           // a sinistra della griglia, spara verso destra
        [SerializeField] LineRenderer beam;
        [SerializeField] Transform dot;
        [SerializeField] TextMesh battery;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] Vector2 origin = new Vector2(-2f, -2.6f);   // centro della cella (0,0)
        [SerializeField] float cell = 1f;
        [SerializeField] int cols = 5, rows = 6;
        [SerializeField] int movesAllowed = 16;
        [SerializeField] int[] solution;               // orientamento giusto degli specchi sul percorso (-1 = qualunque)

        readonly Dictionary<Vector2Int, int> mirrorAt = new Dictionary<Vector2Int, int>();
        readonly HashSet<Vector2Int> blocked = new HashSet<Vector2Int>();
        int[] state;                                    // 0 = "/", 1 = "\"
        int moves;
        bool done;
        Camera cam;
        float onCat;

        Vector2Int CellOf(Vector3 w) => new Vector2Int(Mathf.RoundToInt((w.x - origin.x) / cell), Mathf.RoundToInt((w.y - origin.y) / cell));
        Vector3 Center(Vector2Int c) => new Vector3(origin.x + c.x * cell, origin.y + c.y * cell, 0f);

        public override void OnLevelStart()
        {
            cam = Camera.main;
            state = new int[mirrors.Length];
            for (int i = 0; i < mirrors.Length; i++)
            {
                mirrorAt[CellOf(mirrors[i].position)] = i;
                state[i] = Random.Range(0, 2);
                if (solution != null && i < solution.Length && solution[i] >= 0 && Random.value < 0.75f) state[i] = 1 - solution[i];   // quasi tutti storti
                Apply(i);
            }
            foreach (Transform v in vases) blocked.Add(CellOf(v.position));
            catEyes.SetActive(false);
            Battery();
            Trace();
            if (bubble != null) { bubble.Show("Zzz... (only a red dot wakes this cat)"); StartCoroutine(HideLater(2.2f)); }
        }

        void Apply(int i) => mirrors[i].localRotation = Quaternion.Euler(0, 0, state[i] == 0 ? 0f : 90f);
        void Battery() { if (battery != null) battery.text = "BATTERY: " + (movesAllowed - moves); }

        /// <summary>Segue il raggio di cella in cella; true se arriva al gatto.</summary>
        bool Trace()
        {
            var pts = new List<Vector3> { emitter.position };
            Vector2Int c = CellOf(emitter.position);
            Vector2Int d = new Vector2Int(1, 0);
            Vector2Int catCell = CellOf(cat.position);
            bool hit = false;
            for (int step = 0; step < 60; step++)
            {
                c += d;
                if (c.x < 0 || c.y < 0 || c.x >= cols || c.y >= rows) { pts.Add(Center(c) - new Vector3(d.x, d.y, 0) * cell * 0.5f); break; }
                if (blocked.Contains(c)) { pts.Add(Center(c) - new Vector3(d.x, d.y, 0) * cell * 0.3f); break; }
                if (c == catCell) { pts.Add(Center(c)); hit = true; break; }
                if (mirrorAt.TryGetValue(c, out int m))
                {
                    pts.Add(Center(c));
                    d = state[m] == 0 ? new Vector2Int(d.y, d.x) : new Vector2Int(-d.y, -d.x);
                }
            }
            beam.positionCount = pts.Count;
            beam.SetPositions(pts.ToArray());
            dot.position = pts[pts.Count - 1];
            return hit;
        }

        void Update()
        {
            if (done || IsFinished) return;
            if (Trace()) { onCat += Time.deltaTime; if (onCat > 0.4f) StartCoroutine(Pounce()); return; }
            onCat = 0f;
            if (!PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2Int c = CellOf(PointerInput.ToWorld(cam, sp));
            if (!mirrorAt.TryGetValue(c, out int i)) return;
            if (moves >= movesAllowed) return;
            state[i] = 1 - state[i];
            Apply(i);
            moves++;
            Battery();
            Sfx.Play(this, "tick");
            if (moves >= movesAllowed && !Trace()) StartCoroutine(Flat());
        }

        IEnumerator Pounce()
        {
            done = true;
            catEyes.SetActive(true);
            if (bubble != null) bubble.Show("!!! RED DOT !!!");
            yield return new WaitForSeconds(0.5f);
            Vector3 from = cat.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { cat.position = from + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.6f; yield return null; }
            cat.position = from;
            Haptics.Light();   // il gatto atterra sul puntino
            yield return new WaitForSeconds(1f);
            Win();
        }

        IEnumerator Flat()
        {
            done = true;
            beam.positionCount = 0;
            dot.gameObject.SetActive(false);
            if (bubble != null) bubble.Show("Battery dead. Zzz...");
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
