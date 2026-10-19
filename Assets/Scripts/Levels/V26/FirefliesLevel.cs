using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 94 — le lucciole. Di notte nel bosco le lucciole lampeggiano in tre colori (giallo, verde, azzurro):
    /// il barattolo vuole un colore alla volta (lo dice l'etichetta, e cambia ogni due lucciole). Colore sbagliato:
    /// il barattolo si apre e una lucciola scappa. Lampeggiano poco e volano veloci. E tra i cespugli si accendono
    /// lucine gialle a COPPIE, ferme: sono occhi di lupo. Ogni tanto uno dei due fa l'occhiolino...
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
        [SerializeField] TextMesh wantedLabel;
        [SerializeField] int needed = 6;
        [SerializeField] float flySpeed = 0.2f;
        [SerializeField] Vector2 areaMin = new Vector2(-2.4f, -1.6f), areaMax = new Vector2(2.4f, 3.0f);

        float[] fClock, eClock, seed;
        bool[] caught;
        int[] tint;
        int count, wanted, sinceChange;
        static readonly Color[] colors = { new Color(1f, 0.95f, 0.4f), new Color(0.45f, 1f, 0.45f), new Color(0.45f, 0.85f, 1f) };
        static readonly string[] names = { "YELLOW", "GREEN", "BLUE" };
        bool done;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            fClock = new float[flies.Length];
            seed = new float[flies.Length];
            caught = new bool[flies.Length];
            tint = new int[flies.Length];
            for (int i = 0; i < flies.Length; i++)
            {
                fClock[i] = Random.Range(0f, 3f);
                seed[i] = Random.Range(0f, 100f);
                tint[i] = i % 3;
                flies[i].Find("Glow").GetComponent<SpriteRenderer>().color = colors[tint[i]];
            }
            wanted = Random.Range(0, 3);
            eClock = new float[eyes.Length];
            for (int i = 0; i < eyes.Length; i++) eClock[i] = Random.Range(0f, 4f);
            wolf.SetActive(false);
            Count();
            if (bubble != null) { bubble.Show("Only the colour on the jar! And only fireflies..."); StartCoroutine(HideLater(2.4f)); }
        }

        void Count()
        {
            counter.text = count + " / " + needed;
            Color c = colors[wanted];
            if (jarGlow != null) jarGlow.color = new Color(c.r, c.g, c.b, 0.25f + count / (float)needed * 0.6f);
            if (wantedLabel != null) { wantedLabel.text = "CATCH: " + names[wanted]; wantedLabel.color = c; }
        }

        static bool Lit(float clock) => Mathf.Repeat(clock, 2.4f) < 0.7f;
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
                    Mathf.Lerp(areaMin.x, areaMax.x, Mathf.PerlinNoise(s, t * flySpeed)),
                    Mathf.Lerp(areaMin.y, areaMax.y, Mathf.PerlinNoise(s + 50f, t * flySpeed)), 0f);
                flies[i].Find("Glow").gameObject.SetActive(Lit(fClock[i]));
            }
            for (int i = 0; i < eyes.Length; i++)
            {
                eClock[i] += dt;
                Transform open = eyes[i].Find("Open");
                open.gameObject.SetActive(EyesOpen(eClock[i]));
                // occhiolino: per un attimo resta acceso un occhio solo (sembra una lucciola gialla!)
                bool wink = Mathf.Repeat(eClock[i], 4.2f) > 0.5f && Mathf.Repeat(eClock[i], 4.2f) < 0.9f;
                open.localScale = new Vector3(wink ? 0.4f : 0.8f, 0.8f, 1f);
                open.localPosition = new Vector3(wink ? -0.1f : 0f, open.localPosition.y, 0f);
            }

            if (!PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            for (int i = 0; i < eyes.Length; i++)
                if (EyesOpen(eClock[i]) && Vector2.Distance(w, eyes[i].position) < 0.45f) { StartCoroutine(Wolf(eyes[i])); return; }
            for (int i = 0; i < flies.Length; i++)
                if (!caught[i] && Lit(fClock[i]) && Vector2.Distance(w, flies[i].position) < 0.5f)
                {
                    if (tint[i] == wanted) StartCoroutine(Catch(i));
                    else WrongColour();
                    return;
                }
        }

        void WrongColour()
        {
            Sfx.Play(this, "buzz", 0.7f);
            if (count > 0)
            {
                count--;
                for (int j = 0; j < flies.Length; j++)
                    if (caught[j]) { caught[j] = false; flies[j].gameObject.SetActive(true); fClock[j] = 1.2f; break; }   // ne scappa una
            }
            Count();
            if (bubble != null) { bubble.Show("Wrong colour! One got away..."); StartCoroutine(HideLater(1.4f)); }
        }

        IEnumerator Catch(int i)
        {
            caught[i] = true;
            Sfx.Play(this, "pop", 0.5f);
            Transform f = flies[i];
            Vector3 from = f.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { f.position = Vector3.Lerp(from, jar.position, k); yield return null; }
            f.gameObject.SetActive(false);
            Haptics.Light();
            count++;
            if (++sinceChange >= 2) { sinceChange = 0; wanted = (wanted + Random.Range(1, 3)) % 3; }   // ora un altro colore
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
            Haptics.Medium();
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
