using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 16 — labirinto con trappole. Come il livello 2 (inclinazione), ma lungo la serpentina ci sono buche
    /// (ci cadi dentro: si riprova) e laser intermittenti (se ti toccano mentre sono accesi: zap!). Si arriva alla stella.
    /// Nel livello 16 (labirinto circolare) le trappole stanno sul percorso obbligato, una ogni 45°: buche alternate contro
    /// le pareti (si passa dall'altro lato), una sulla bocca di ogni varco (si entra di sbieco) e un laser per corridoio.
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
            switch (MazeTrapFx.Check(ball.position, pits, pitRadius * fallInside, lasers, ballRadius, out Vector2 pitAt))
            {
                case MazeTrapFx.Hit.Pit: StartCoroutine(FallIn(pitAt)); break;
                case MazeTrapFx.Hit.Laser: StartCoroutine(Zap()); break;
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
            yield return MazeTrapFx.FallIn(this, ball, center);
            if (bubble != null) bubble.Show("Plop!");
            yield return new WaitForSeconds(0.8f);
            Fail();
        }

        IEnumerator Zap()
        {
            over = true;
            yield return MazeTrapFx.Zap(this, ball, sparks);
            if (bubble != null) bubble.Show("ZAP!");
            yield return new WaitForSeconds(0.7f);
            Fail();
        }
    }
}
