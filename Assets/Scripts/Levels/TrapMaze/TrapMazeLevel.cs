using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 16 — labirinto con trappole. Come il livello 2 (inclinazione), ma lungo la serpentina ci sono buche
    /// (ci cadi dentro: si riprova) e laser intermittenti (se ti toccano mentre sono accesi: zap!). Si arriva alla stella.
    /// </summary>
    public class TrapMazeLevel : LevelController
    {
        [SerializeField] Rigidbody2D ball;
        [SerializeField] Transform[] pits;
        [SerializeField] float pitRadius = 0.3f;
        [SerializeField] float fallInside = 0.68f;        // frazione del raggio: oltre il bordo la sfera "scivola" dentro
        [SerializeField] LaserBeam[] lasers;
        [SerializeField] float ballRadius = 0.2f;
        [SerializeField] SmokeEmitter sparks;
        [SerializeField] SpeechBubble bubble;

        GoalZone goal;
        bool over;

        public override void OnLevelStart()
        {
            goal = GetComponentInChildren<GoalZone>(true);
            if (goal != null) goal.Reached += OnGoal;
            if (bubble != null) bubble.Hide();
        }

        void OnDestroy() { if (goal != null) goal.Reached -= OnGoal; }

        void Update()
        {
            if (over || IsFinished || ball == null) return;
            Vector2 p = ball.position;
            foreach (Transform pit in pits)
            {
                if (pit == null) continue;
                if (Vector2.Distance(p, pit.position) < pitRadius * fallInside) { StartCoroutine(FallIn(pit.position)); return; }
            }
            foreach (LaserBeam l in lasers)
            {
                if (l != null && l.IsOn && l.DistanceTo(p) < ballRadius + 0.04f) { StartCoroutine(Zap()); return; }
            }
        }

        void OnGoal()
        {
            if (over || IsFinished) return;
            over = true;
            StartCoroutine(GoalRoutine());
        }

        IEnumerator GoalRoutine()
        {
            ball.simulated = false;
            Transform t = ball.transform;
            Vector3 from = t.position, to = new Vector3(goal.transform.position.x, goal.transform.position.y, from.z);
            Vector3 scale = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                t.position = Vector3.Lerp(from, to, k);
                t.localScale = scale * Mathf.Lerp(1f, 0.6f, k);
                yield return null;
            }
            Win();
        }

        IEnumerator FallIn(Vector2 center)
        {
            over = true;
            ball.simulated = false;
            Transform t = ball.transform;
            Vector3 from = t.position, scale = t.localScale;
            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            TrailRenderer trail = t.GetComponent<TrailRenderer>();
            if (trail != null) trail.emitting = false;
            // la sfera gira nel bordo della buca e sparisce nel buio
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
            {
                float a = k * 9f, r = Vector2.Distance(from, center) * (1f - k);
                t.position = new Vector3(center.x + Mathf.Cos(a) * r, center.y + Mathf.Sin(a) * r, from.z);
                t.localScale = scale * Mathf.Lerp(1f, 0.1f, k * k);
                if (sr != null) sr.color = Color.Lerp(Color.white, new Color(0.2f, 0.2f, 0.25f), k);
                yield return null;
            }
            t.gameObject.SetActive(false);
            Haptics.Medium();
            if (bubble != null) bubble.Show("Plop!");
            yield return new WaitForSeconds(0.8f);
            Fail();
        }

        IEnumerator Zap()
        {
            Haptics.Medium();
            over = true;
            ball.simulated = false;
            Transform t = ball.transform;
            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            if (sparks != null) { sparks.transform.position = t.position; sparks.Play(); }
            Vector3 start = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                t.position = start + (Vector3)(Random.insideUnitCircle * 0.05f);
                if (sr != null) sr.color = Mathf.Repeat(k * 10f, 1f) < 0.5f ? new Color(1f, 0.35f, 0.35f) : Color.white;
                yield return null;
            }
            if (sparks != null) sparks.Stop();
            if (bubble != null) bubble.Show("ZAP!");
            yield return new WaitForSeconds(0.7f);
            Fail();
        }
    }
}
