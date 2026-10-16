using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 94 — le lucciole. Di notte nel bosco le lucciole si accendono e si spengono: toccale mentre brillano
    /// per metterle nel barattolo. Ma tra i cespugli si accendono anche altre lucine gialle... a COPPIE, e ferme.
    /// Quelle sono occhi. Non toccarle.
    /// </summary>
    public class FirefliesLevel : LevelController
    {
        [SerializeField] Transform[] flies;           // figlio "Glow"
        [SerializeField] Transform[] eyes;            // figlio "Open"
        [SerializeField] Transform jar;
        [SerializeField] SpriteRenderer jarGlow;
        [SerializeField] TextMesh counter;
        [SerializeField] GameObject wolf;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int needed = 6;
        [SerializeField] Vector2 areaMin = new Vector2(-2.4f, -1.6f), areaMax = new Vector2(2.4f, 3.0f);

        float[] fClock, eClock, seed;
        bool[] caught;
        int count;
        bool done;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            fClock = new float[flies.Length];
            seed = new float[flies.Length];
            caught = new bool[flies.Length];
            for (int i = 0; i < flies.Length; i++) { fClock[i] = Random.Range(0f, 3f); seed[i] = Random.Range(0f, 100f); }
            eClock = new float[eyes.Length];
            for (int i = 0; i < eyes.Length; i++) eClock[i] = Random.Range(0f, 4f);
            wolf.SetActive(false);
            Count();
            if (bubble != null) { bubble.Show("Catch six glowing fireflies. Only fireflies."); StartCoroutine(HideLater(2.4f)); }
        }

        void Count() { counter.text = count + " / " + needed; if (jarGlow != null) jarGlow.color = new Color(1f, 0.95f, 0.4f, count / (float)needed * 0.8f); }

        static bool Lit(float clock) => Mathf.Repeat(clock, 2.6f) < 1.0f;
        static bool EyesOpen(float clock) => Mathf.Repeat(clock, 4.2f) < 1.3f;

        void Update()
        {
            if (done || IsFinished) return;
            float dt = Time.deltaTime, t = Time.time;
            for (int i = 0; i < flies.Length; i++)
            {
                if (caught[i]) continue;
                fClock[i] += dt;
                float s = seed[i];
                flies[i].position = new Vector3(
                    Mathf.Lerp(areaMin.x, areaMax.x, Mathf.PerlinNoise(s, t * 0.12f)),
                    Mathf.Lerp(areaMin.y, areaMax.y, Mathf.PerlinNoise(s + 50f, t * 0.12f)), 0f);
                flies[i].Find("Glow").gameObject.SetActive(Lit(fClock[i]));
            }
            for (int i = 0; i < eyes.Length; i++) { eClock[i] += dt; eyes[i].Find("Open").gameObject.SetActive(EyesOpen(eClock[i])); }

            if (!PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            for (int i = 0; i < eyes.Length; i++)
                if (EyesOpen(eClock[i]) && Vector2.Distance(w, eyes[i].position) < 0.45f) { StartCoroutine(Wolf(eyes[i])); return; }
            for (int i = 0; i < flies.Length; i++)
                if (!caught[i] && Lit(fClock[i]) && Vector2.Distance(w, flies[i].position) < 0.5f) { StartCoroutine(Catch(i)); return; }
        }

        IEnumerator Catch(int i)
        {
            caught[i] = true;
            Sfx.Play(this, "pop", 0.5f);
            Transform f = flies[i];
            Vector3 from = f.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { f.position = Vector3.Lerp(from, jar.position, k); yield return null; }
            f.gameObject.SetActive(false);
            count++;
            Count();
            if (count >= needed && !done)
            {
                done = true;
                if (bubble != null) bubble.Show("A lamp that never needs batteries!");
                yield return new WaitForSeconds(1.5f);
                Win();
            }
        }

        IEnumerator Wolf(Transform at)
        {
            done = true;
            wolf.transform.position = at.position + Vector3.down * 0.4f;
            wolf.SetActive(true);
            Vector3 s = wolf.transform.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { wolf.transform.localScale = s * Mathf.Lerp(0.3f, 1.4f, k); yield return null; }
            if (bubble != null) bubble.Show("Those were NOT fireflies.");
            yield return new WaitForSeconds(1.5f);
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
