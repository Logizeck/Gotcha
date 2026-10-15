using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 39 — la pista per la biglia. Disegna col dito fino a 3 rampe, poi premi GO: la biglia parte dalla
    /// mensola in alto e deve finire nella tazza, oltre il muretto. Se cade fuori torna al via (le rampe restano):
    /// si corregge e si riprova quante volte si vuole.
    /// </summary>
    public class MarbleRunLevel : LevelController
    {
        [SerializeField] Rigidbody2D marble;
        [SerializeField] TapButton go;
        [SerializeField] Collider2D cupZone;
        [SerializeField] float lostBelow = -3.0f;   // sotto il piano di lavoro, fuori dalla tazza = mancata
        [SerializeField] SmokeEmitter confetti;
        [SerializeField] SpeechBubble bubble;

        Vector3 start;
        bool rolling, done;
        float inCup, rollTime;

        public override void OnLevelStart()
        {
            start = marble.transform.position;
            Hold();
            go.Pressed += OnGo;
            if (confetti != null) confetti.Stop();
            if (bubble != null) { bubble.Show("Draw a path, then GO!"); StartCoroutine(HideLater(2f)); }
        }

        void OnDestroy() { if (go != null) go.Pressed -= OnGo; }

        void Hold()
        {
            marble.bodyType = RigidbodyType2D.Kinematic;
            marble.transform.position = start;
            marble.position = start;
            marble.angularVelocity = 0f;
#if UNITY_6000_0_OR_NEWER
            marble.linearVelocity = Vector2.zero;
#else
            marble.velocity = Vector2.zero;
#endif
            rolling = false;
        }

        void OnGo(TapButton b)
        {
            if (done) return;
            Hold();
            marble.bodyType = RigidbodyType2D.Dynamic;
            marble.AddForce(new Vector2(1.2f, 0f) * marble.mass, ForceMode2D.Impulse);   // una spintarella
            rolling = true;
            rollTime = 0f;
            inCup = 0f;
        }

        void Update()
        {
            if (done || IsFinished || !rolling) return;
            rollTime += Time.deltaTime;
            Vector2 p = marble.position;
            bool cupped = cupZone.OverlapPoint(p);
            inCup = cupped ? inCup + Time.deltaTime : 0f;
            if (inCup > 0.5f) { StartCoroutine(Score()); return; }
            if ((p.y < lostBelow && !cupped) || Mathf.Abs(p.x) > 3.4f || rollTime > 12f)
            {
                Hold();
                if (bubble != null) { bubble.Show("Missed! Fix the path."); StartCoroutine(HideLater(1.4f)); }
            }
        }

        IEnumerator Score()
        {
            done = true;
            marble.bodyType = RigidbodyType2D.Kinematic;
            if (confetti != null) confetti.Play();
            if (bubble != null) bubble.Show("Hole in one!");
            yield return new WaitForSeconds(1.5f);
            if (confetti != null) confetti.Stop();
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
