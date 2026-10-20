using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The silent disco" — quattro ballerini con le cuffie ballano senza musica, ognuno a modo suo. Solo quello
    /// blu tiene un ritmo regolare: bisogna toccare il pavimento tre volte con la sua stessa cadenza. Tre ritmi
    /// sbagliati = sconfitta.
    /// </summary>
    public class TheSilentDiscoLevel : LevelController
    {
        [SerializeField] Transform[] dancers;
        [SerializeField] Collider2D floorArea;
        [SerializeField] SpriteRenderer ring;
        [SerializeField] SpriteRenderer[] tapDots, lights;
        [SerializeField] TextMesh missText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int steadyIndex = 2, missesAllowed = 3;
        [SerializeField] float period = 0.8f, tolerance = 0.2f, sequenceGap = 1.8f;

        Camera cam;
        float clock;
        float[] lastBeat, nextBeat;
        Vector3[] home;
        readonly float[] taps = new float[3];
        int tapCount, misses;
        bool busy, synced;
        Coroutine ringCo;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            lastBeat = new float[dancers.Length];
            nextBeat = new float[dancers.Length];
            home = new Vector3[dancers.Length];
            for (int i = 0; i < dancers.Length; i++)
            {
                home[i] = dancers[i].localPosition;
                lastBeat[i] = -10f;
                nextBeat[i] = 0.3f + i * 0.17f;
            }
            S41HKit.Alpha(ring, 0f);
            S41HKit.Confetti(confetti, false);
            foreach (SpriteRenderer l in lights) S41HKit.Alpha(l, 0.25f);
            UpdateDots();
            UpdateMisses();
            S41HKit.Say(bubble, "Shh! Silent disco. Can you feel the beat?");
        }

        void UpdateDots()
        {
            for (int i = 0; i < tapDots.Length; i++)
                tapDots[i].color = i < tapCount ? new Color(0.45f, 0.75f, 1f) : new Color(1f, 1f, 1f, 0.3f);
        }

        void UpdateMisses() { if (missText != null) missText.text = "WRONG RHYTHMS " + misses + "/" + missesAllowed; }

        /// <summary>Intervallo fino al prossimo passo: solo il blu è regolare.</summary>
        float Interval(int i)
        {
            if (synced || i == steadyIndex) return period;
            switch (i)
            {
                case 0: return Random.Range(0.3f, 1.1f);                       // rosso: a caso
                case 1: return (Mathf.RoundToInt(clock * 10f) % 2 == 0) ? 0.35f : 0.65f; // giallo: zoppica
                default: return Random.Range(1.1f, 1.7f);                       // verde: lento e storto
            }
        }

        void Update()
        {
            clock += Time.deltaTime;
            AnimateDancers();
            if (IsFinished || busy) return;

            if (tapCount > 0 && clock - taps[tapCount - 1] > sequenceGap)
            {
                tapCount = 0;
                UpdateDots();
            }
            foreach (S41HKit.Finger f in S41HKit.Fingers(cam))
            {
                if (!f.began || !floorArea.OverlapPoint(f.world)) continue;
                Tap(f.world);
                break;
            }
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space)) Tap(new Vector2(0f, -3.5f));
#endif
        }

        void AnimateDancers()
        {
            for (int i = 0; i < dancers.Length; i++)
            {
                if (clock >= nextBeat[i])
                {
                    lastBeat[i] = synced ? nextBeat[i] : clock;
                    nextBeat[i] = lastBeat[i] + Interval(i);
                    if (synced && i == 0) S41HKit.Note(this, 110f, 0.45f, 0.25f);
                }
                float p = (clock - lastBeat[i]) / 0.32f;
                float hop = p < 1f ? Mathf.Sin(p * Mathf.PI) : 0f;
                float squash = p < 1.4f ? Mathf.Sin(Mathf.Clamp01(p / 1.4f) * Mathf.PI * 2f) * 0.08f : 0f;
                dancers[i].localPosition = home[i] + Vector3.up * hop * 0.28f;
                dancers[i].localScale = new Vector3(1f + squash, 1f - squash, 1f);
                dancers[i].localRotation = Quaternion.Euler(0, 0, Mathf.Sin((clock - lastBeat[i]) * 6f + i) * (synced ? 8f : 5f));
            }
            if (synced)
                for (int k = 0; k < lights.Length; k++)
                    S41HKit.Alpha(lights[k], 0.35f + 0.6f * Mathf.Abs(Mathf.Sin(clock * Mathf.PI / period + k)));
        }

        void Tap(Vector2 at)
        {
            if (tapCount >= taps.Length) tapCount = 0;
            taps[tapCount++] = clock;
            UpdateDots();
            S41HKit.Note(this, 196f, 0.5f, 0.15f);
            Sfx.Play(this, "tick", 0.5f);
            if (ringCo != null) StopCoroutine(ringCo);
            ringCo = StartCoroutine(RingFx(at));
            if (tapCount < taps.Length) return;
            float a = taps[1] - taps[0], b = taps[2] - taps[1];
            bool ok = Mathf.Abs(a - period) <= tolerance && Mathf.Abs(b - period) <= tolerance;
            StartCoroutine(ok ? Victory() : Wrong((a + b) * 0.5f, Mathf.Abs(a - b)));
        }

        IEnumerator RingFx(Vector2 at)
        {
            ring.transform.position = new Vector3(at.x, at.y, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                ring.transform.localScale = Vector3.one * (0.4f + k * 0.9f);
                S41HKit.Alpha(ring, 1f - k);
                yield return null;
            }
            S41HKit.Alpha(ring, 0f);
        }

        IEnumerator Wrong(float mean, float uneven)
        {
            busy = true;
            misses++;
            UpdateMisses();
            Sfx.Play(this, "buzz", 0.5f);
            Haptics.Light();
            string why = uneven > 0.3f ? "Wobbly! Like the red guy..." : mean < period - tolerance ? "Way too fast!" : mean > period + tolerance ? "Too slow, grandpa!" : "Almost... not quite.";
            S41HKit.Say(bubble, why);
            for (int i = 0; i < dancers.Length; i++) StartCoroutine(S41HKit.Shake(dancers[i], 0.06f, 0.3f));
            yield return new WaitForSeconds(1.2f);
            tapCount = 0;
            UpdateDots();
            if (misses >= missesAllowed)
            {
                S41HKit.Say(bubble, "Party's over. Everyone go home!");
                foreach (SpriteRenderer l in lights) S41HKit.Alpha(l, 0f);
                yield return new WaitForSeconds(1.2f);
                Fail();
                yield break;
            }
            S41HKit.Say(bubble, misses == 1 ? "Watch the dancers. Who never misses a step?" : "Copy the steady one!");
            busy = false;
        }

        IEnumerator Victory()
        {
            busy = true;
            Sfx.Play(this, "ding", 0.8f);
            S41HKit.Say(bubble, "THAT'S THE BEAT! DROP IT!");
            yield return new WaitForSeconds(0.3f);
            // tutti a tempo col blu
            float t0 = nextBeat[steadyIndex];
            for (int i = 0; i < dancers.Length; i++) nextBeat[i] = t0;
            synced = true;
            S41HKit.Confetti(confetti, true);
            yield return new WaitForSeconds(3f);
            S41HKit.Confetti(confetti, false);
            Win();
        }
    }
}
