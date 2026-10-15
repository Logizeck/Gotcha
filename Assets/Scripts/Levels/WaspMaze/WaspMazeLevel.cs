using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 36 — la vespa nel labirinto. Si inclina il telefono per portare la sfera alla stella, ma una vespa
    /// parte dall'alveare alle sue spalle e la insegue in linea retta. I muri fermano anche lei (ci scivola lungo): usali per seminarla. Se ti punge, si
    /// riprova. La vespa è un po' più lenta della sfera e ogni tanto si ferma a riprendere fiato.
    /// </summary>
    public class WaspMazeLevel : LevelController
    {
        [SerializeField] Rigidbody2D ball;
        [SerializeField] Transform wasp;
        [SerializeField] SpriteRenderer waspBody;
        [SerializeField] AudioSource buzz;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float speed = 1.75f;
        [SerializeField] float radius = 0.22f;
        [SerializeField] float stingDistance = 0.42f;
        [SerializeField] float startDelay = 1.5f;
        [SerializeField] float restEvery = 5f;
        [SerializeField] float restFor = 0.8f;

        GoalZone goal;
        float clock, stuckFor;
        bool over;
        Vector2 nudge;
        float nudgeFor, size = 1f;

        public override void OnLevelStart()
        {
            size = Mathf.Abs(wasp.localScale.y);
            goal = GetComponentInChildren<GoalZone>(true);
            if (goal != null) goal.Reached += OnGoal;
            if (buzz != null) { buzz.clip = WaspLevel.BuzzClip(); buzz.loop = true; buzz.volume = 0.3f; buzz.Play(); }
            if (bubble != null) { bubble.Show("BZZZ!"); StartCoroutine(HideLater(1.4f)); }
        }

        void OnDestroy()
        {
            if (goal != null) goal.Reached -= OnGoal;
            if (buzz != null && buzz.clip != null) Destroy(buzz.clip);
        }

        void Update()
        {
            if (over || IsFinished) return;
            clock += Time.deltaTime;
            Vector2 pos = wasp.position, target = ball.position;
            if (clock < startDelay) { Hover(pos); return; }
            // ogni tanto riprende fiato (ronza sul posto)
            if (Mathf.Repeat(clock - startDelay, restEvery) > restEvery - restFor) { Hover(pos); return; }

            Vector2 want = (target - pos).normalized;
            if (nudgeFor > 0f) { nudgeFor -= Time.deltaTime; want = (want + nudge).normalized; }
            Vector2 step = want * speed * Time.deltaTime;
            Vector2 moved = Move(pos, step);
            // bloccata contro un muro: prova a scivolare lungo gli assi
            if ((moved - pos).sqrMagnitude < step.sqrMagnitude * 0.25f)
            {
                Vector2 alt = Move(pos, new Vector2(step.x, 0f) * 1.2f);
                if ((alt - pos).sqrMagnitude < 1e-6f) alt = Move(pos, new Vector2(0f, step.y) * 1.2f);
                moved = alt;
            }
            stuckFor = (moved - pos).magnitude < speed * Time.deltaTime * 0.2f ? stuckFor + Time.deltaTime : 0f;
            if (stuckFor > 1.2f) { stuckFor = 0f; nudge = Random.insideUnitCircle.normalized; nudgeFor = 0.6f; }
            wasp.position = new Vector3(moved.x, moved.y, wasp.position.z);
            wasp.localScale = new Vector3(target.x < moved.x ? size : -size, size, 1f);

            if (Vector2.Distance(moved, target) < stingDistance) StartCoroutine(Sting());
        }

        void Hover(Vector2 pos)
        {
            wasp.position = new Vector3(pos.x + Mathf.Sin(Time.time * 47f) * 0.004f, pos.y + Mathf.Sin(Time.time * 9f) * 0.004f, wasp.position.z);
        }

        /// <summary>Muove la vespa fino al primo muro (i muri del labirinto, non la sfera).</summary>
        Vector2 Move(Vector2 pos, Vector2 step)
        {
            float len = step.magnitude;
            if (len < 1e-6f) return pos;
            Vector2 dir = step / len;
            float allowed = len;
            foreach (RaycastHit2D h in Physics2D.CircleCastAll(pos, radius, dir, len))
            {
                if (h.rigidbody != null || h.collider.isTrigger) continue;   // la sfera e il traguardo non fermano la vespa
                allowed = Mathf.Min(allowed, Mathf.Max(0f, h.distance - 0.01f));
            }
            return pos + dir * allowed;
        }

        void OnGoal()
        {
            if (over || IsFinished) return;
            over = true;
            StartCoroutine(Arrived());
        }

        IEnumerator Arrived()
        {
            ball.simulated = false;
            Transform t = ball.transform;
            Vector3 from = t.position, to = new Vector3(goal.transform.position.x, goal.transform.position.y, from.z);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { t.position = Vector3.Lerp(from, to, k); yield return null; }
            if (buzz != null) buzz.Stop();
            if (bubble != null) bubble.Show("Grrr... you got away!", true);
            yield return new WaitForSeconds(1.2f);
            Win();
        }

        IEnumerator Sting()
        {
            over = true;
            ball.simulated = false;
            if (waspBody != null) waspBody.color = new Color(1f, 0.6f, 0.6f);
            if (bubble != null) bubble.Show("STING!", true);
            SpriteRenderer sr = ball.GetComponent<SpriteRenderer>();
            for (float k = 0f; k < 0.6f; k += Time.deltaTime)
            {
                if (sr != null) sr.color = Mathf.Repeat(k * 10f, 1f) < 0.5f ? new Color(1f, 0.4f, 0.4f) : Color.white;
                yield return null;
            }
            if (buzz != null) buzz.Stop();
            yield return new WaitForSeconds(0.6f);
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !over) bubble.Hide();
        }
    }
}
