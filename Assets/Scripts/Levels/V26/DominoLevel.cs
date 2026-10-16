using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 90 — il domino. Una fila di tessere deve far suonare il campanello in fondo, ma ne mancano due.
    /// Nel vassoio ci sono tre tessere: bassa, normale, alta. Un pezzo che cade arriva lontano quanto è alto:
    /// nel buco grande serve la più alta, ma sotto la mensola la più alta non ci sta. Metti le tessere e tocca la prima.
    /// </summary>
    public class DominoLevel : LevelController
    {
        [SerializeField] Transform[] row;             // tessere fisse, pivot alla base
        [SerializeField] Transform[] gaps;            // posti vuoti
        [SerializeField] float[] gapClearance;        // altezza massima che ci sta (0 = nessun limite)
        [SerializeField] Draggable2D[] pieces;        // itemId = altezza ("0.45", "1", "1.5")
        [SerializeField] Collider2D firstTile;
        [SerializeField] Transform bell;
        [SerializeField] SpeechBubble bubble;

        readonly Dictionary<Draggable2D, Vector3> home = new Dictionary<Draggable2D, Vector3>();
        Draggable2D[] inGap;
        bool done, running;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            inGap = new Draggable2D[gaps.Length];
            foreach (Draggable2D p in pieces)
            {
                home[p] = p.transform.position;
                p.returnOnMiss = false;
                p.Dropped += OnDropped;
            }
            if (bubble != null) { bubble.Show("Ring the bell!"); StartCoroutine(HideLater(1.6f)); }
        }

        static float Height(Draggable2D d) => float.Parse(d.itemId, System.Globalization.CultureInfo.InvariantCulture);

        void OnDropped(Draggable2D d, DropSlot slot)
        {
            for (int i = 0; i < inGap.Length; i++) if (inGap[i] == d) inGap[i] = null;
            int best = -1;
            float bestD = 0.7f;
            for (int i = 0; i < gaps.Length; i++)
            {
                float dist = Mathf.Abs(d.transform.position.x - gaps[i].position.x);
                if (inGap[i] == null && dist < bestD && Mathf.Abs(d.transform.position.y - gaps[i].position.y) < 1.6f) { best = i; bestD = dist; }
            }
            if (best >= 0 && gapClearance[best] > 0f && Height(d) > gapClearance[best])
            {
                if (bubble != null) { bubble.Show("Too tall for under the shelf!"); StartCoroutine(HideLater(1.4f)); }
                Sfx.Play(this, "bonk");
                best = -1;
            }
            if (best < 0) { d.transform.position = home[d]; return; }
            inGap[best] = d;
            d.transform.position = new Vector3(gaps[best].position.x, gaps[best].position.y, d.transform.position.z);
            Sfx.Play(this, "tick");
        }

        void Update()
        {
            if (done || running || IsFinished || !Gotcha.Core.PointerInput.AnyTouchBegan()) return;
            if (!Gotcha.Core.PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            if (firstTile.OverlapPoint(Gotcha.Core.PointerInput.ToWorld(cam, sp))) StartCoroutine(Topple());
        }

        IEnumerator Topple()
        {
            running = true;
            foreach (Draggable2D p in pieces) p.enabled = false;
            // la fila completa, ordinata da sinistra a destra
            var tiles = new List<(Transform t, float h)>();
            foreach (Transform t in row) tiles.Add((t, t.localScale.y));
            for (int i = 0; i < gaps.Length; i++) if (inGap[i] != null) tiles.Add((inGap[i].transform, Height(inGap[i])));
            tiles.Sort((a, b) => a.t.position.x.CompareTo(b.t.position.x));

            for (int i = 0; i < tiles.Count; i++)
            {
                (Transform t, float h) = tiles[i];
                float nextX = i + 1 < tiles.Count ? tiles[i + 1].t.position.x : bell.position.x;
                float reach = h * 0.95f;
                float gap = nextX - t.position.x;
                bool hits = gap <= reach;
                float endAngle = hits ? Mathf.Acos(Mathf.Clamp01(gap / h)) * Mathf.Rad2Deg : 0f;   // si appoggia sul prossimo
                float to = 90f - endAngle;
                Sfx.Play(this, "tick", 0.8f);
                for (float k = 0f; k < 1f; k += Time.deltaTime / (0.12f + h * 0.12f))
                {
                    t.localRotation = Quaternion.Euler(0, 0, -to * k * k);
                    yield return null;
                }
                t.localRotation = Quaternion.Euler(0, 0, -to);
                if (!hits) { StartCoroutine(Stopped()); yield break; }
            }
            done = true;
            Sfx.Play(this, "ding", 1f);
            for (float k = 0f; k < 0.8f; k += Time.deltaTime) { bell.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 40f) * 20f * (1f - k)); yield return null; }
            if (bubble != null) bubble.Show("DING DING! Beautiful.");
            yield return new WaitForSeconds(1.2f);
            Win();
        }

        IEnumerator Stopped()
        {
            done = true;
            if (bubble != null) bubble.Show("Clack... and silence.");
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
