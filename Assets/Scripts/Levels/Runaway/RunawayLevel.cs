using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 25 — il pulsante fifone. Bisogna premerlo 5 volte, ma ha le gambe e scappa: gironzola per la stanza
    /// e se il dito tocca vicino (ma non sopra) schiva di lato. Contro i muri è più lento: conviene metterlo all'angolo.
    /// </summary>
    public class RunawayLevel : LevelController
    {
        [SerializeField] Transform button;
        [SerializeField] Transform visual;
        [SerializeField] Transform legs;
        [SerializeField] TextMesh counter;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int hitsNeeded = 5;
        [SerializeField] float hitRadius = 0.5f;
        [SerializeField] float scareRadius = 1.6f;
        [SerializeField] float speed = 2.0f;
        [SerializeField] Vector2 boundsMin = new Vector2(-2.3f, -3.9f);
        [SerializeField] Vector2 boundsMax = new Vector2(2.3f, 2.6f);

        int hits;
        Vector2 target;
        float retarget, dash, squash;
        Camera cam;
        Vector3 visualScale;
        bool done;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            visualScale = visual.localScale;
            target = button.position;
            UpdateCounter();
            if (bubble != null) bubble.Hide();
        }

        void Update()
        {
            if (done || IsFinished) return;
            Vector2 pos = button.position;

            if (PointerInput.AnyTouchBegan() && !PointerInput.IsOverUi() && PointerInput.TryGetPrimary(out Vector2 sp, out _, out _))
            {
                Vector2 p = PointerInput.ToWorld(cam, sp);
                float d = Vector2.Distance(p, pos + new Vector2(0f, 0.1f));
                if (d < hitRadius) { Hit(); Flee(pos, p, 2.4f); }
                else if (d < scareRadius) Flee(pos, p, 1.8f);
            }

            retarget -= Time.deltaTime;
            if (retarget <= 0f || Vector2.Distance(pos, target) < 0.1f)
            {
                retarget = Random.Range(0.6f, 1.3f);
                target = Clamp(pos + Random.insideUnitCircle.normalized * Random.Range(1f, 2.5f));
            }
            // contro i muri è goffo e lento: è lì che lo si prende
            float wall = Mathf.Min(pos.x - boundsMin.x, boundsMax.x - pos.x, pos.y - boundsMin.y, boundsMax.y - pos.y);
            float v = (speed + hits * 0.35f) * (wall < 0.45f ? 0.45f : 1f) * (dash > 0f ? 4f : 1f);
            dash -= Time.deltaTime;
            Vector2 next = Vector2.MoveTowards(pos, target, v * Time.deltaTime);
            button.position = new Vector3(next.x, next.y, button.position.z);

            // gambe che corrono e pulsante che si schiaccia quando viene premuto
            float run = (next - pos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            if (legs != null) legs.localScale = new Vector3(next.x < pos.x ? -1f : 1f, 1f + Mathf.Sin(Time.time * 30f) * 0.15f * Mathf.Clamp01(run), 1f);
            squash -= Time.deltaTime;
            visual.localScale = Vector3.Lerp(visual.localScale, squash > 0f ? new Vector3(visualScale.x * 1.15f, visualScale.y * 0.7f, 1f) : visualScale, 20f * Time.deltaTime);
        }

        void Flee(Vector2 pos, Vector2 from, float distance)
        {
            Vector2 away = (pos - from).sqrMagnitude > 0.0001f ? (pos - from).normalized : Random.insideUnitCircle.normalized;
            away = Quaternion.Euler(0, 0, Random.Range(-35f, 35f)) * away;
            target = Clamp(pos + away * distance);
            dash = 0.3f;
            retarget = 0.8f;
        }

        void Hit()
        {
            hits++;
            squash = 0.15f;
            UpdateCounter();
            if (bubble != null) { bubble.Show(hits >= hitsNeeded ? "OK OK, you win!" : hits == 1 ? "Hey!" : "Ouch!"); }
            if (hits >= hitsNeeded) StartCoroutine(Surrender());
        }

        IEnumerator Surrender()
        {
            done = true;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                visual.localScale = new Vector3(visualScale.x * 1.2f, visualScale.y * 0.6f, 1f);
                yield return null;
            }
            visual.localScale = visualScale;
            yield return new WaitForSeconds(0.5f);
            Win();
        }

        void UpdateCounter() { if (counter != null) counter.text = hits + " / " + hitsNeeded; }

        Vector2 Clamp(Vector2 p) { return new Vector2(Mathf.Clamp(p.x, boundsMin.x, boundsMax.x), Mathf.Clamp(p.y, boundsMin.y, boundsMax.y)); }
    }
}
