using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livelli "Sting Run": labirinto aperto (più strade, qualche vicolo cieco) da attraversare inclinando il telefono
    /// mentre una o più vespe inseguono la sfera lungo i corridoi (<see cref="MazeChaser"/>: strada più breve, non
    /// linea retta). Le vespe sono un po' più lente della sfera e ogni tanto riprendono fiato: si seminano girando
    /// attorno ai blocchi di muri. Se una punge, si riprova.
    /// </summary>
    public class WaspMazeLevel : LevelController
    {
        [SerializeField] Rigidbody2D ball;
        [SerializeField] MazeChaser[] wasps;
        [SerializeField] SpeechBubble bubble;

        [Tooltip("Punture prima di perdere: la prima fa solo male (la vespa torna al suo posto), la seconda è fatale.")]
        [SerializeField] int stingsAllowed = 2;

        GoalZone goal;
        bool over, hurting;
        int stings;
        Vector3[] waspHome;

        public override void OnLevelStart()
        {
            goal = GetComponentInChildren<GoalZone>(true);
            if (goal != null) goal.Reached += OnGoal;
            waspHome = new Vector3[wasps.Length];
            for (int i = 0; i < wasps.Length; i++) waspHome[i] = wasps[i].transform.position;
            foreach (MazeChaser w in wasps)
            {
                w.Stung += OnStung;
                if (w.buzz != null) { w.buzz.clip = WaspLevel.BuzzClip(); w.buzz.loop = true; w.buzz.pitch = Random.Range(0.92f, 1.08f); w.buzz.Play(); }
            }
            if (bubble != null) { bubble.Show(wasps.Length > 1 ? "BZZZ! BZZZ!" : "BZZZ!"); StartCoroutine(HideLater(1.4f)); }
        }

        void OnDestroy()
        {
            if (goal != null) goal.Reached -= OnGoal;
            foreach (MazeChaser w in wasps)
                if (w != null && w.buzz != null && w.buzz.clip != null) Destroy(w.buzz.clip);
        }

        void Freeze(bool stopBuzz)
        {
            foreach (MazeChaser w in wasps)
            {
                w.Frozen = true;
                if (stopBuzz && w.buzz != null) w.buzz.Stop();
            }
        }

        void OnGoal()
        {
            if (over || IsFinished) return;
            over = true;
            Freeze(false);
            StartCoroutine(Arrived());
        }

        void OnStung(MazeChaser w)
        {
            if (over || hurting || IsFinished) return;
            stings++;
            Haptics.Medium();
            if (stings < stingsAllowed) { StartCoroutine(Ouch(w)); return; }
            over = true;
            Freeze(false);
            StartCoroutine(Sting(w));
        }

        /// <summary>Prima puntura: la sfera lampeggia, le vespe tornano al loro posto e riposano un attimo.</summary>
        IEnumerator Ouch(MazeChaser w)
        {
            hurting = true;
            if (bubble != null) { bubble.Show("OUCH! One more sting and it's over!", true); StartCoroutine(HideLater(1.8f)); }
            for (int i = 0; i < wasps.Length; i++) { wasps[i].Frozen = true; wasps[i].transform.position = waspHome[i]; }
            SpriteRenderer sr = ball.GetComponent<SpriteRenderer>();
            for (float k = 0f; k < 1.6f; k += Time.deltaTime)
            {
                if (sr != null) sr.color = Mathf.Repeat(k * 8f, 1f) < 0.5f ? new Color(1f, 0.5f, 0.5f) : Color.white;
                yield return null;
            }
            if (sr != null) sr.color = Color.white;
            if (!over) foreach (MazeChaser x in wasps) x.Frozen = false;
            hurting = false;
        }

        IEnumerator Arrived()
        {
            ball.simulated = false;
            Transform t = ball.transform;
            Vector3 from = t.position, to = new Vector3(goal.transform.position.x, goal.transform.position.y, from.z);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { t.position = Vector3.Lerp(from, to, k); yield return null; }
            Freeze(true);
            if (bubble != null) bubble.Show("Grrr... you got away!", true);
            yield return new WaitForSeconds(1.2f);
            Win();
        }

        IEnumerator Sting(MazeChaser w)
        {
            ball.simulated = false;
            SpriteRenderer body = w.GetComponentInChildren<SpriteRenderer>();
            if (body != null) body.color = new Color(1f, 0.6f, 0.6f);
            if (bubble != null) bubble.Show("STING!", true);
            SpriteRenderer sr = ball.GetComponent<SpriteRenderer>();
            for (float k = 0f; k < 0.6f; k += Time.deltaTime)
            {
                if (sr != null) sr.color = Mathf.Repeat(k * 10f, 1f) < 0.5f ? new Color(1f, 0.4f, 0.4f) : Color.white;
                yield return null;
            }
            Freeze(true);
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
