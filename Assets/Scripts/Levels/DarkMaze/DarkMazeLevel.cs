using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 17 — labirinto al buio. Si vede solo intorno alla sfera. L'uscita (stella) è chiusa da un cancello
    /// rosso: prima bisogna trovare la chiave, nascosta in un vicolo cieco. Toccandola con la sfera la si prende
    /// e il cancello si apre.
    /// </summary>
    public class DarkMazeLevel : LevelController
    {
        [SerializeField] Rigidbody2D ball;
        [SerializeField] Transform darkness;
        [SerializeField] Transform key;
        [SerializeField] float keyRadius = 0.42f;
        [SerializeField] GameObject gate;
        [SerializeField] SmokeEmitter sparkle;
        [SerializeField] SpeechBubble bubble;

        GoalZone goal;
        bool hasKey, over;
        Vector3 keyScale;

        public override void OnLevelStart()
        {
            goal = GetComponentInChildren<GoalZone>(true);
            if (goal != null) goal.Reached += OnGoal;
            if (key != null) keyScale = key.localScale;
            if (bubble != null) bubble.Hide();
        }

        void OnDestroy() { if (goal != null) goal.Reached -= OnGoal; }

        void LateUpdate()
        {
            if (ball == null) return;
            Vector3 b = ball.transform.position;
            if (darkness != null) darkness.position = new Vector3(b.x, b.y, darkness.position.z);
            if (key == null || over) return;

            if (!hasKey)
            {
                key.localScale = keyScale * (1f + Mathf.Sin(Time.time * 4f) * 0.08f);
                if (Vector2.Distance(b, key.position) < keyRadius) StartCoroutine(TakeKey());
            }
            else
            {
                // la chiave segue la sfera, appesa un po' sopra
                Vector3 target = b + new Vector3(0.25f, 0.3f, 0f);
                key.position = Vector3.Lerp(key.position, new Vector3(target.x, target.y, key.position.z), 12f * Time.deltaTime);
            }
        }

        IEnumerator TakeKey()
        {
            hasKey = true;
            if (sparkle != null) { sparkle.transform.position = key.position; sparkle.Play(); }
            if (bubble != null) bubble.Show("Click!");
            key.localScale = keyScale * 0.7f;
            yield return new WaitForSeconds(0.25f);
            if (sparkle != null) sparkle.Stop();
            // il cancello si dissolve
            if (gate != null)
            {
                SpriteRenderer sr = gate.GetComponentInChildren<SpriteRenderer>();
                foreach (Collider2D c in gate.GetComponentsInChildren<Collider2D>()) c.enabled = false;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
                {
                    if (sr != null) sr.color = new Color(1f, 1f, 1f, 1f - k);
                    yield return null;
                }
                gate.SetActive(false);
            }
            yield return new WaitForSeconds(0.8f);
            if (bubble != null) bubble.Hide();
        }

        void OnGoal()
        {
            if (over || IsFinished || !hasKey) return;
            over = true;
            StartCoroutine(GoalRoutine());
        }

        IEnumerator GoalRoutine()
        {
            ball.simulated = false;
            Transform t = ball.transform;
            Vector3 from = t.position, to = new Vector3(goal.transform.position.x, goal.transform.position.y, from.z);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                t.position = Vector3.Lerp(from, to, k);
                if (darkness != null) darkness.localScale = Vector3.one * (1f + k * 4f);   // si accende la luce
                yield return null;
            }
            Win();
        }
    }
}
