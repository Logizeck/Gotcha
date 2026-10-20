using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Shadow thief" — un ladro invisibile passeggia tra cinque lampioni spenti; al chiaro di luna si vede solo
    /// la sua ombra. Bisogna accendere (toccandolo) il lampione verso cui l'ombra sta andando, così il ladro entra nella
    /// luce e viene scoperto. Il percorso è sempre lo stesso; tre lampioni accesi a vuoto = il ladro scappa.
    /// </summary>
    public class ShadowThiefLevel : LevelController
    {
        [SerializeField] Transform[] lamps;
        [SerializeField] Collider2D[] lampAreas;
        [SerializeField] SpriteRenderer[] cones, bulbs;
        [SerializeField] Transform thief, shadow;
        [SerializeField] SpriteRenderer thiefArt, exclaim;
        [SerializeField] TextMesh missText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int missesAllowed = 3;
        [SerializeField] float litSeconds = 1.6f, catchRange = 0.5f;

        // percorso fisso: x di arrivo, velocità, pausa dopo
        static readonly Vector3[] Path =
        {
            new Vector3(-0.4f, 1.1f, 0.7f), new Vector3(-1.5f, 1.5f, 0.5f), new Vector3(1.4f, 1.0f, 1.0f),
            new Vector3(0.3f, 1.8f, 0.4f), new Vector3(2.4f, 1.2f, 0.8f), new Vector3(3.6f, 1.4f, 0f),
        };

        Camera cam;
        float[] litT;
        int step, misses;
        float pause;
        bool busy;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            litT = new float[lamps.Length];
            for (int i = 0; i < lamps.Length; i++) SetLamp(i, false);
            thief.localPosition = new Vector3(-3.6f, thief.localPosition.y, 0f);
            S41HKit.Alpha(thiefArt, 0f);
            exclaim.gameObject.SetActive(false);
            S41HKit.Confetti(confetti, false);
            UpdateMisses();
            S41HKit.Say(bubble, "An invisible thief! But... see that shadow?");
        }

        void UpdateMisses() { if (missText != null) missText.text = "WASTED LAMPS " + misses + "/" + missesAllowed; }

        void SetLamp(int i, bool on)
        {
            S41HKit.Alpha(cones[i], on ? 0.55f : 0f);
            bulbs[i].color = on ? new Color(1f, 0.95f, 0.6f) : new Color(0.45f, 0.45f, 0.55f);
        }

        void Update()
        {
            if (IsFinished || busy) return;
            Walk();
            for (int i = 0; i < lamps.Length; i++)
            {
                if (litT[i] <= 0f) continue;
                if (Mathf.Abs(thief.position.x - lamps[i].position.x) < catchRange) { StartCoroutine(Caught(i)); return; }
                litT[i] -= Time.deltaTime;
                if (litT[i] <= 0f) { StartCoroutine(Wasted(i)); return; }
            }
            foreach (S41HKit.Finger f in S41HKit.Fingers(cam))
            {
                if (!f.began) continue;
                for (int i = 0; i < lamps.Length; i++)
                {
                    if (litT[i] > 0f || !lampAreas[i].OverlapPoint(f.world)) continue;
                    litT[i] = litSeconds;
                    SetLamp(i, true);
                    Sfx.Play(this, "clunk", 0.6f);
                    break;
                }
            }
        }

        void Walk()
        {
            if (pause > 0f) { pause -= Time.deltaTime; shadow.localScale = Vector3.one; return; }
            Vector3 p = thief.localPosition;
            Vector3 s = Path[step];
            float dir = Mathf.Sign(s.x - p.x);
            p.x = Mathf.MoveTowards(p.x, s.x, s.y * Time.deltaTime);
            thief.localPosition = p;
            shadow.localScale = new Vector3(1f, 1f + Mathf.Abs(Mathf.Sin(Time.time * 9f)) * 0.08f, 1f);   // la luna è a sinistra: ombra sempre a destra
            thiefArt.flipX = dir < 0f;
            if (Mathf.Approximately(p.x, s.x))
            {
                pause = s.z;
                step++;
                if (step >= Path.Length)
                {
                    // esce a destra e rientra da sinistra: stesso giro
                    step = 0;
                    thief.localPosition = new Vector3(-3.6f, p.y, 0f);
                    S41HKit.Say(bubble, "He's back for another round...");
                }
            }
        }

        IEnumerator Wasted(int i)
        {
            busy = true;
            misses++;
            UpdateMisses();
            Sfx.Play(this, "buzz", 0.5f);
            for (int k = 0; k < 3; k++) { SetLamp(i, false); yield return new WaitForSeconds(0.08f); SetLamp(i, true); yield return new WaitForSeconds(0.08f); }
            SetLamp(i, false);
            litT[i] = 0f;
            if (misses >= missesAllowed)
            {
                S41HKit.Say(bubble, "Hee hee! You'll never see me!");
                for (float t = 0f; t < 1.4f; t += Time.deltaTime)
                {
                    thief.localPosition += Vector3.right * 3.5f * Time.deltaTime;
                    yield return null;
                }
                Fail();
                yield break;
            }
            S41HKit.Say(bubble, misses == 1 ? "Missed! Light the lamp he's walking to." : "Watch where the shadow is heading...");
            busy = false;
        }

        IEnumerator Caught(int i)
        {
            busy = true;
            for (int k = 0; k < lamps.Length; k++) if (k != i) SetLamp(k, false);
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Medium();
            exclaim.gameObject.SetActive(true);
            yield return StartCoroutine(S41HKit.Fade(thiefArt, 1f, 0.25f));
            S41HKit.Say(bubble, "GOTCHA! Caught in the spotlight!");
            StartCoroutine(S41HKit.Pop(thief, 0.2f, 0.3f));
            S41HKit.Confetti(confetti, true);
            for (float t = 0f; t < 2.2f; t += Time.deltaTime)
            {
                S41HKit.Alpha(cones[i], 0.5f + Mathf.Sin(t * 12f) * 0.15f);
                yield return null;
            }
            S41HKit.Confetti(confetti, false);
            Win();
        }
    }
}
