using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Slow motion" — un cannoncino dispettoso spara una palla velocissima in una corsia dove la paratia NON c'è.
    /// Tieni due dita ferme sullo schermo per rallentare il tempo (oppure tieni premuto il pulsante SLOW), poi con un
    /// altro dito sposta la paratia nella corsia giusta e ferma la palla. Tre palle nella fossa = sconfitta.
    /// </summary>
    public class SlowMotionLevel : LevelController
    {
        [SerializeField] Transform cannon, ball, bumper, slowButton;
        [SerializeField] Collider2D bumperArea, slowArea;
        [SerializeField] SpriteRenderer tint;
        [SerializeField] TextMesh slowLabel, missesText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float[] lanes = { 1.2f, 0f, -1.2f, -2.4f };
        [SerializeField] float ballSpeed = 11f, slowFactor = 0.12f, pitX = 2.45f, stillRadius = 0.35f;

        enum Phase { Aim, Wait, Fly, Pause }

        Camera cam;
        readonly Dictionary<int, Vector2> starts = new Dictionary<int, Vector2>();
        Phase phase = Phase.Pause;
        float scale = 1f, phaseTime = 1.2f, targetLane, ballX;
        int dragId = int.MinValue, misses, lane;
        bool busy;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            ball.gameObject.SetActive(false);
            S41GKit.Alpha(tint, 0f);
            slowLabel.text = "";
            UpdateMisses();
            S41GKit.Confetti(confetti, false);
            if (bubble != null) bubble.Show("Bet you can't stop my fastball!");
        }

        void UpdateMisses() { missesText.text = "BALLS LOST: " + misses + "/3"; }

        int LaneUnder(float y)
        {
            for (int i = 0; i < lanes.Length; i++) if (Mathf.Abs(lanes[i] - y) < 0.6f) return i;
            return -1;
        }

        int PickLane()
        {
            int blocked = LaneUnder(bumper.localPosition.y), pick;
            do pick = Random.Range(0, lanes.Length); while (pick == blocked);
            return pick;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            float dt = Time.deltaTime;
            bool slow = ReadFingers();
            scale = Mathf.MoveTowards(scale, slow ? slowFactor : 1f, dt * 6f);
            S41GKit.Alpha(tint, (1f - scale) * 0.35f);
            slowLabel.text = slow ? "SLOW-MO" : "";
            float gdt = dt * scale;

            phaseTime -= gdt;
            switch (phase)
            {
                case Phase.Pause:
                    if (phaseTime <= 0f) { lane = PickLane(); targetLane = lanes[lane]; phase = Phase.Aim; phaseTime = 0.6f; }
                    break;
                case Phase.Aim:
                    cannon.localPosition = new Vector3(cannon.localPosition.x, Mathf.MoveTowards(cannon.localPosition.y, targetLane, 8f * gdt), 0f);
                    if (phaseTime <= 0f) { phase = Phase.Wait; phaseTime = 0.4f; }
                    break;
                case Phase.Wait:
                    if (phaseTime <= 0f)
                    {
                        // furbo: se la paratia è nella sua corsia, cambia mira all'ultimo istante
                        if (LaneUnder(bumper.localPosition.y) == lane) { lane = PickLane(); targetLane = lanes[lane]; }
                        cannon.localPosition = new Vector3(cannon.localPosition.x, targetLane, 0f);
                        Fire();
                    }
                    break;
                case Phase.Fly:
                    ballX += ballSpeed * gdt;
                    ball.localPosition = new Vector3(ballX, targetLane, 0f);
                    ball.localRotation = Quaternion.Euler(0, 0, -ballX * 200f);
                    if (ballX + 0.28f >= bumper.localPosition.x - 0.2f && ballX < bumper.localPosition.x && Mathf.Abs(bumper.localPosition.y - targetLane) < 0.6f) { StartCoroutine(Caught()); return; }
                    if (ballX >= pitX) { StartCoroutine(Lost()); return; }
                    break;
            }
        }

        void Fire()
        {
            phase = Phase.Fly;
            ballX = cannon.localPosition.x + 0.5f;
            ball.gameObject.SetActive(true);
            ball.localScale = Vector3.one;
            Sfx.Play(this, "pop", 0.8f);
            StartCoroutine(S41GKit.Pop(cannon, 1.15f, 0.15f));
        }

        /// <summary>Legge le dita: una trascina la paratia, due ferme (o il pulsante SLOW) rallentano il tempo.</summary>
        bool ReadFingers()
        {
            int still = 0;
            bool slowHeld = false, dragAlive = false;
            foreach (S41GKit.Finger f in S41GKit.Fingers(cam))
            {
                if (f.began)
                {
                    starts[f.id] = f.world;
                    if (dragId == int.MinValue && bumperArea.OverlapPoint(f.world)) { dragId = f.id; Sfx.Play(this, "tick", 0.5f); }
                }
                if (f.ended) { starts.Remove(f.id); if (f.id == dragId) dragId = int.MinValue; continue; }
                if (f.id == dragId)
                {
                    dragAlive = true;
                    bumper.localPosition = new Vector3(bumper.localPosition.x, Mathf.Clamp(f.world.y, lanes[lanes.Length - 1] - 0.2f, lanes[0] + 0.2f), 0f);
                    continue;
                }
                if (slowArea.OverlapPoint(f.world)) { slowHeld = true; continue; }
                if (!starts.TryGetValue(f.id, out Vector2 s)) { starts[f.id] = f.world; s = f.world; }
                if (Vector2.Distance(s, f.world) < stillRadius) still++;
            }
            if (!dragAlive) dragId = int.MinValue;
            if (Input.touchCount == 0 && !Input.GetMouseButton(0)) starts.Clear();
            slowButton.localScale = Vector3.one * (slowHeld ? 0.92f : 1f);
            bool slow = still >= 2 || slowHeld;
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.S)) slow = true;   // Editor: S = rallenta
#endif
            return slow;
        }

        IEnumerator Caught()
        {
            busy = true;
            Sfx.Play(this, "bonk", 0.9f);
            Haptics.Medium();
            ball.localPosition = new Vector3(bumper.localPosition.x - 0.48f, targetLane, 0f);
            S41GKit.Alpha(tint, 0f);
            slowLabel.text = "CAUGHT!";
            StartCoroutine(S41GKit.Shake(bumper, 0.08f, 0.3f));
            yield return StartCoroutine(S41GKit.Pop(ball, 1.4f, 0.3f));
            if (bubble != null) bubble.Show("WHAT?! Nobody's that fast!");
            yield return StartCoroutine(S41GKit.Shake(cannon, 0.1f, 0.6f));
            Sfx.Play(this, "ding", 0.9f);
            S41GKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.8f);
            S41GKit.Confetti(confetti, false);
            Win();
        }

        IEnumerator Lost()
        {
            busy = true;
            misses++;
            UpdateMisses();
            Sfx.Play(this, "whoosh", 0.6f);
            Vector3 p = ball.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                ball.localPosition = p + new Vector3(0.15f * k, -0.3f * k, 0f);
                ball.localScale = Vector3.one * (1f - k);
                yield return null;
            }
            ball.gameObject.SetActive(false);
            Sfx.Play(this, "bonk", 0.5f);
            Haptics.Light();
            if (bubble != null) bubble.Show(misses >= 3 ? "Three in the pit! Too slow, human!" : misses == 1 ? "Whoosh! Too fast for you?" : "Everything's so fast, right?");
            yield return new WaitForSeconds(misses >= 3 ? 1.5f : 0.6f);
            if (misses >= 3) { Fail(); yield break; }
            phase = Phase.Pause;
            phaseTime = 1.0f;
            busy = false;
        }
    }
}
