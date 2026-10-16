using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 17 — labirinto al buio. Si vede solo intorno alla sfera. L'uscita (stella) è chiusa da un cancello
    /// rosso: prima bisogna trovare la chiave, nascosta in un vicolo cieco. Toccandola con la sfera la si prende
    /// e il cancello si apre. Nelle varianti nel buio dorme una vespa: se la luce della sfera la raggiunge si sveglia e
    /// insegue (il ronzio si sente sempre più forte avvicinandosi, anche attraverso i muri). Se punge, si riprova.
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
        [SerializeField] MazeChaser[] wasps;

        GoalZone goal;
        bool hasKey, over;
        Vector3 keyScale, darkScale = Vector3.one;

        public override void OnLevelStart()
        {
            goal = GetComponentInChildren<GoalZone>(true);
            if (goal != null) goal.Reached += OnGoal;
            if (key != null) keyScale = key.localScale;
            if (bubble != null) bubble.Hide();
            if (darkness != null) darkScale = darkness.localScale;
            if (wasps != null)
                foreach (MazeChaser w in wasps)
                {
                    w.Stung += OnStung;
                    if (w.buzz != null) { w.buzz.clip = WaspLevel.BuzzClip(); w.buzz.loop = true; w.buzz.Play(); }
                }
        }

        void OnDestroy()
        {
            if (goal != null) goal.Reached -= OnGoal;
            if (wasps != null)
                foreach (MazeChaser w in wasps)
                    if (w != null && w.buzz != null && w.buzz.clip != null) Destroy(w.buzz.clip);
        }

        void StopWasps()
        {
            if (wasps == null) return;
            foreach (MazeChaser w in wasps) { w.Frozen = true; if (w.buzz != null) w.buzz.Stop(); }
        }

        void OnStung(MazeChaser w)
        {
            if (over || IsFinished) return;
            over = true;
            StartCoroutine(Stung());
        }

        IEnumerator Stung()
        {
            ball.simulated = false;
            if (wasps != null) foreach (MazeChaser w in wasps) w.Frozen = true;
            if (bubble != null) bubble.Show("STING!", true);
            SpriteRenderer sr = ball.GetComponent<SpriteRenderer>();
            for (float k = 0f; k < 0.6f; k += Time.deltaTime)
            {
                if (sr != null) sr.color = Mathf.Repeat(k * 10f, 1f) < 0.5f ? new Color(1f, 0.4f, 0.4f) : Color.white;
                yield return null;
            }
            StopWasps();
            yield return new WaitForSeconds(0.6f);
            Fail();
        }

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
                if (wasps != null) foreach (MazeChaser w in wasps) w.Build();   // il varco ora è aperto anche per le vespe
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
            StopWasps();
            Transform t = ball.transform;
            Vector3 from = t.position, to = new Vector3(goal.transform.position.x, goal.transform.position.y, from.z);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                t.position = Vector3.Lerp(from, to, k);
                if (darkness != null) darkness.localScale = darkScale * (1f + k * 4f);   // si accende la luce
                yield return null;
            }
            Win();
        }
    }
}
