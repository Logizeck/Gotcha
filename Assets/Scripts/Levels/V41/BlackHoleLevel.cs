using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Black hole" — un buco nero sta inghiottendo tre pianetini che gli girano attorno a spirale, e intanto
    /// cresce; il comando dice "STOP THE BLACK HOLE". Soluzione: RIMPICCIOLIRLO stringendo due dita sopra (pizzico
    /// verso l'interno) finché collassa in un puntino e sparisce con un "pop": i pianeti superstiti sono salvi. Allargare
    /// le dita lo fa crescere e i pianeti spariscono prima. Sconfitta: tutti e tre i pianeti inghiottiti.
    /// (Editor: rotella del mouse.)
    /// </summary>
    public class BlackHoleLevel : LevelController
    {
        [SerializeField] Transform hole;
        [SerializeField] Transform swirl;
        [SerializeField] Transform[] planets;
        [SerializeField] float[] planetStart = { 1.45f, 2.05f, 2.6f };
        [SerializeField] float[] planetDoom = { 9f, 15f, 21f };     // secondi prima di essere inghiottiti
        [SerializeField] SpriteRenderer whiteout;
        [SerializeField] TextMesh escapedText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SpriteRenderer[] hearts;                   // un cuore per pianeta
        [SerializeField] float startRadius = 0.55f, collapseRadius = 0.13f;
        [Tooltip("Quanto cresce da solo il buco nero (unità al secondo).")]
        [SerializeField] float growRate = 0.035f;

        Camera cam;
        float radius, clock, pinchStart, radiusStart;
        bool pinching, busy, hinted;
        float[] angle, orbit, speed, rate;
        bool[] gone;
        float[] scales;
        int natural;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            radius = startRadius;
            int n = planets.Length;
            angle = new float[n]; orbit = new float[n]; speed = new float[n]; rate = new float[n]; gone = new bool[n];
            scales = new float[n];
            for (int i = 0; i < n; i++)
            {
                angle[i] = i * 2.1f + 0.4f;
                scales[i] = planets[i].localScale.y;
                orbit[i] = planetStart[i];
                speed[i] = 1.4f / Mathf.Sqrt(orbit[i]);
                rate[i] = (orbit[i] - startRadius * 0.9f) / planetDoom[i];
            }
            if (whiteout != null) whiteout.color = new Color(1f, 1f, 1f, 0f);
            if (escapedText != null) escapedText.gameObject.SetActive(false);
            if (bubble != null) bubble.Hide();
            Kit41B.Hearts(hearts, n);
            ApplyRadius();
        }

        void ApplyRadius()
        {
            hole.localScale = Vector3.one * radius;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            float dt = Time.deltaTime;
            clock += dt;
            if (swirl != null) swirl.Rotate(0f, 0f, -90f * dt);

            // il buco nero cresce da solo; il pizzico lo ridimensiona (esponenziale, così basta un gesto o due)
            if (!pinching) radius = Mathf.Min(radius + growRate * dt, 3f);
            if (PointerInput.TryGetPinch(out float d))
            {
                if (!pinching) { pinching = true; pinchStart = Mathf.Max(d, 1f); radiusStart = radius; }
                else radius = Mathf.Clamp(radiusStart * Mathf.Pow(d / pinchStart, 1.8f), 0.05f, 3f);
            }
            else
            {
                pinching = false;
                if (PointerInput.AnyTouchBegan() && !hinted && clock > 3f) { hinted = true; Kit41B.Say(bubble, "Tapping won't stop it!"); }
            }
#if UNITY_EDITOR
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f) radius = Mathf.Clamp(radius * (1f + wheel * 0.15f), 0.05f, 3f);
#endif
            ApplyRadius();

            for (int i = 0; i < planets.Length; i++)
            {
                if (gone[i]) continue;
                orbit[i] -= rate[i] * dt;
                angle[i] += speed[i] * dt * (1f + 1.5f / Mathf.Max(0.4f, orbit[i]));
                Vector3 p = new Vector3(Mathf.Cos(angle[i]), Mathf.Sin(angle[i]), 0f) * orbit[i];
                planets[i].localPosition = hole.localPosition + p;
                float stretch = Mathf.Clamp01(1f - (orbit[i] - radius) / 1.2f);
                planets[i].localRotation = Quaternion.Euler(0f, 0f, angle[i] * Mathf.Rad2Deg);
                planets[i].localScale = new Vector3(1f - stretch * 0.3f, 1f + stretch * 0.4f, 1f) * scales[i];
                if (orbit[i] < radius * 0.9f) StartCoroutine(Swallow(i, true));
            }

            if (radius <= collapseRadius) { StartCoroutine(Collapse()); return; }
            if (natural >= planets.Length) StartCoroutine(Doomed());
        }


        IEnumerator Swallow(int i, bool byTheSpiral)
        {
            gone[i] = true;
            if (byTheSpiral) natural++;
            Kit41B.Hearts(hearts, planets.Length - natural);
            Sfx.Play(this, "whoosh", 0.6f);
            Haptics.Light();
            Transform t = planets[i];
            Vector3 s = t.localScale, p = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                t.localScale = new Vector3(s.x * (1f - k), s.y * (1f - k) * (1f + k), 1f);
                t.localPosition = Vector3.Lerp(p, hole.localPosition, k * k);
                yield return null;
            }
            t.gameObject.SetActive(false);
            if (byTheSpiral && natural < planets.Length) Kit41B.Say(bubble, natural == 1 ? "Gulp! One planet gone!" : "Only one left! DO something!");
        }

        IEnumerator Doomed()
        {
            busy = true;
            Kit41B.Say(bubble, "BURP.");
            Haptics.Heavy();
            Sfx.Play(this, "bonk", 0.8f);
            yield return Kit41B.Pop(hole, 0.25f, 0.4f);
            yield return new WaitForSeconds(0.8f);
            Fail();
        }

        /// <summary>Il buco nero collassa in un puntino e sparisce: i pianeti rimasti tornano tranquilli in orbita.</summary>
        IEnumerator Collapse()
        {
            busy = true;
            if (bubble != null) bubble.Hide();
            Haptics.Heavy();
            Sfx.Play(this, "pop", 1f);
            float r0 = radius;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { radius = Mathf.Lerp(r0, 0f, k); ApplyRadius(); yield return null; }
            hole.gameObject.SetActive(false);
            if (swirl != null) swirl.gameObject.SetActive(false);
            Sfx.Play(this, "ding", 0.8f);
            // i pianeti salvi si allontanano piano e smettono di deformarsi
            var from = new Vector3[planets.Length];
            for (int i = 0; i < planets.Length; i++) from[i] = planets[i].localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.2f)
            {
                for (int i = 0; i < planets.Length; i++)
                {
                    if (gone[i]) continue;
                    Vector3 dir = (from[i] - hole.localPosition).normalized;
                    planets[i].localPosition = from[i] + dir * Mathf.SmoothStep(0f, 1f, k) * 0.8f;
                    planets[i].localScale = Vector3.Lerp(planets[i].localScale, Vector3.one * scales[i], k);
                }
                yield return null;
            }
            if (escapedText != null)
            {
                escapedText.text = "BLACK HOLE STOPPED!";
                escapedText.color = new Color(1f, 0.93f, 0.55f);   // sullo spazio buio (niente più schermo bianco)
                escapedText.gameObject.SetActive(true);
                Transform e = escapedText.transform;
                Vector3 s = e.localScale;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { e.localScale = s * Mathf.Lerp(0.3f, 1f, 1f - (1f - k) * (1f - k)); yield return null; }
                e.localScale = s;
            }
            yield return new WaitForSeconds(1.4f);
            Win();
        }
    }
}
