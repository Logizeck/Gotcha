using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The three idiots" — tre omini (visti dall'alto) devono spingere un masso nella buca, ma uno guarda
    /// dalla parte sbagliata. Soluzione: ruota quell'omino con due dita (gesto di rotazione) finché tutti guardano
    /// verso la buca, poi suona il fischietto per farli spingere insieme. Tre spinte discordi = sconfitta.
    /// </summary>
    public class ThreeIdiotsLevel : LevelController
    {
        [SerializeField] Transform[] guys;
        [SerializeField] Transform rock, pit, whistle;
        [SerializeField] Collider2D whistleArea;
        [SerializeField] TextMesh triesText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter dust;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float[] startAngles = { 0f, 180f, 0f };
        [SerializeField] float tolerance = 25f;

        Camera cam;
        float[] angle;
        Vector3[] home;
        Vector3 rockHome;
        int target = -1, discord;
        float lastTwist;
        bool busy, oneFingerTold;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            angle = new float[guys.Length];
            home = new Vector3[guys.Length];
            for (int i = 0; i < guys.Length; i++)
            {
                angle[i] = i < startAngles.Length ? startAngles[i] : 0f;
                home[i] = guys[i].localPosition;
            }
            rockHome = rock.localPosition;
            if (dust != null) dust.Stop();
            S41KKit.Confetti(confetti, false);
            ApplyAngles();
            UpdateTries();
            if (bubble != null) bubble.Show("We push! We push really hard! ...why isn't it moving?");
        }

        void UpdateTries() { triesText.text = "FAILED PUSHES: " + discord + "/3"; }

        void ApplyAngles()
        {
            for (int i = 0; i < guys.Length; i++)
            {
                float wob = busy ? 0f : Mathf.Sin(Time.time * 6f + i * 2f) * 4f;
                guys[i].localRotation = Quaternion.Euler(0f, 0f, angle[i] + wob);
            }
        }

        int Nearest(Vector2 w, float max)
        {
            int best = -1;
            float bd = max;
            for (int i = 0; i < guys.Length; i++)
            {
                float d = Vector2.Distance(w, guys[i].position);
                if (d < bd) { bd = d; best = i; }
            }
            return best;
        }

        void Update()
        {
            if (IsFinished) return;
            ApplyAngles();
            if (busy) return;
            List<S41KKit.Finger> fingers = S41KKit.Fingers(cam);
            var live = new List<S41KKit.Finger>();
            foreach (S41KKit.Finger f in fingers) if (!f.ended) live.Add(f);

            if (live.Count >= 2)
            {
                Vector2 a = live[0].world, b = live[1].world;
                float ang = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
                if (target < 0) { target = Nearest((a + b) * 0.5f, 1.6f); lastTwist = ang; }
                else
                {
                    angle[target] += Mathf.DeltaAngle(lastTwist, ang);
                    lastTwist = ang;
                }
                return;
            }
            if (target >= 0) { Snap(target); target = -1; return; }

#if UNITY_EDITOR
            // Editor: rotellina del mouse sopra un omino = rotazione a due dita
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f)
            {
                int g = Nearest(PointerInput.ToWorld(cam, Input.mousePosition), 1.2f);
                if (g >= 0) { angle[g] += wheel * 15f; Snap(g); }
            }
#endif
            if (live.Count == 1 && live[0].began)
            {
                Vector2 w = live[0].world;
                if (whistleArea.OverlapPoint(w)) { StartCoroutine(Blow()); return; }
                int g = Nearest(w, 0.6f);
                if (g >= 0)
                {
                    StartCoroutine(S41KKit.Shake(guys[g], 0.06f, 0.25f));
                    Sfx.Play(this, "bonk", 0.4f);
                    if (bubble != null && !oneFingerTold) { bubble.Show("Hey! One finger can't turn me. I'm stubborn!"); oneFingerTold = true; }
                }
            }
        }

        void Snap(int g)
        {
            float a = Mathf.Repeat(angle[g], 360f);
            float snapped = Mathf.Round(a / 45f) * 45f;
            angle[g] = Mathf.Abs(Mathf.DeltaAngle(a, snapped)) < 12f ? snapped : a;
            Sfx.Play(this, "tick", 0.5f);
            if (Aligned(g) && bubble != null) bubble.Show("Oh! THAT'S where the rock is!");
        }

        bool Aligned(int g) => Mathf.Abs(Mathf.DeltaAngle(angle[g], 0f)) < tolerance;

        IEnumerator Blow()
        {
            busy = true;
            StartCoroutine(S41KKit.Pop(whistle, 1.25f, 0.25f));
            S41KKit.Tone(this, 1900f, 0.5f, 0.5f);
            S41KKit.Tone(this, 2300f, 0.45f, 0.35f);
            yield return new WaitForSeconds(0.35f);
            ApplyAngles();
            bool all = true;
            Vector2 sum = Vector2.zero;
            for (int i = 0; i < guys.Length; i++)
            {
                if (!Aligned(i)) all = false;
                float r = angle[i] * Mathf.Deg2Rad;
                sum += new Vector2(Mathf.Cos(r), Mathf.Sin(r));
            }
            // tutti fanno un passo nella direzione in cui guardano
            for (int i = 0; i < guys.Length; i++)
            {
                float r = angle[i] * Mathf.Deg2Rad;
                StartCoroutine(S41KKit.Move(guys[i], home[i] + new Vector3(Mathf.Cos(r), Mathf.Sin(r), 0f) * 0.2f, 0.25f));
            }
            if (dust != null) dust.Play();
            Sfx.Play(this, "clunk", 0.7f);
            if (all)
            {
                if (bubble != null) bubble.Show("ONE, TWO... PUUUSH!");
                Vector3 dest = new Vector3(pit.localPosition.x, rock.localPosition.y, 0f);
                for (float t = 0f; t < 1f; t += Time.deltaTime / 1.6f)
                {
                    float e = t * t;
                    rock.localPosition = Vector3.Lerp(rockHome, dest, e);
                    rock.localRotation = Quaternion.Euler(0f, 0f, -e * 360f);
                    for (int i = 0; i < guys.Length; i++) guys[i].localPosition = home[i] + new Vector3((dest.x - rockHome.x) * e * 0.75f + 0.2f, 0f, 0f);
                    yield return null;
                }
                if (dust != null) dust.Stop();
                yield return StartCoroutine(S41KKit.ScaleTo(rock, new Vector3(0.35f, 0.35f, 1f), 0.35f));
                rock.gameObject.SetActive(false);
                Sfx.Play(this, "bonk", 0.9f);
                Haptics.Heavy();
                Sfx.Play(this, "ding", 0.8f);
                if (bubble != null) bubble.Show("WE DID IT! Teamwork! ...what's teamwork?");
                for (int i = 0; i < guys.Length; i++) StartCoroutine(S41KKit.Move(guys[i], guys[i].localPosition, 0.4f, 0.4f));
                S41KKit.Confetti(confetti, true);
                yield return new WaitForSeconds(2f);
                S41KKit.Confetti(confetti, false);
                Win();
                yield break;
            }

            // spinte discordi: il masso dondola e torna indietro
            Vector3 nudge = new Vector3(sum.x, sum.y, 0f) * 0.12f;
            yield return StartCoroutine(S41KKit.Move(rock, rockHome + nudge, 0.2f));
            yield return StartCoroutine(S41KKit.Move(rock, rockHome - new Vector3(0.25f, 0f, 0f), 0.25f));
            if (dust != null) dust.Stop();
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Medium();
            discord++;
            UpdateTries();
            for (int i = 0; i < guys.Length; i++) StartCoroutine(S41KKit.Move(guys[i], home[i], 0.3f));
            yield return StartCoroutine(S41KKit.Move(rock, rockHome, 0.4f));
            if (discord >= 3)
            {
                if (bubble != null) bubble.Show("Forget it. Let's just live here with the rock.");
                yield return new WaitForSeconds(1.5f);
                Fail();
                yield break;
            }
            if (bubble != null) bubble.Show(discord == 1 ? "Ouch! Somebody is pushing the wrong way!" : "Are we sure we're all facing the hole?");
            busy = false;
        }
    }
}
