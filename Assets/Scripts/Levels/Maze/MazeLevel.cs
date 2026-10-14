using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 2 — labirinto a inclinazione. La sfera parte fuori dal labirinto e va portata al centro
    /// inclinando il telefono (TiltGravity sulla radice del livello, frecce nell'Editor).
    /// </summary>
    public class MazeLevel : LevelController
    {
        [SerializeField] Rigidbody2D ball;

        GoalZone goal;

        public override void OnLevelStart()
        {
            if (ball == null) ball = GetComponentInChildren<Rigidbody2D>();
            goal = GetComponentInChildren<GoalZone>(true);
            if (goal != null) goal.Reached += OnGoalReached;
        }

        void OnDestroy()
        {
            if (goal != null) goal.Reached -= OnGoalReached;
        }

        void OnGoalReached()
        {
            if (IsFinished) return;
            StartCoroutine(GoalRoutine());
        }

        IEnumerator GoalRoutine()
        {
            if (ball != null)
            {
                ball.simulated = false;
                Transform t = ball.transform;
                Vector3 from = t.position;
                Vector3 to = new Vector3(goal.transform.position.x, goal.transform.position.y, from.z);
                Vector3 scale = t.localScale;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
                {
                    t.position = Vector3.Lerp(from, to, k);
                    t.localScale = scale * Mathf.Lerp(1f, 0.6f, k);
                    yield return null;
                }
            }
            Win();
        }
    }
}
