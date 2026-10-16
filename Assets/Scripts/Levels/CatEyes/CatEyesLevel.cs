using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 34 — gli occhi del gatto. Le pupille seguono l'inclinazione del telefono (rispetto a come lo tieni
    /// all'inizio). Un topolino sbuca da una tana a caso: il gatto deve fissarlo per 3 secondi di fila.
    /// (Editor: frecce.)
    /// Variante "a salti": il topolino cambia tana ogni <see cref="hopEvery"/> secondi; il tempo di sguardo si somma
    /// (cala piano se lo perdi) e va raggiunto prima che il topolino abbia fatto troppi salti, altrimenti scappa.
    /// </summary>
    public class CatEyesLevel : LevelController
    {
        [SerializeField] Transform[] pupils;
        [SerializeField] float maxOffset = 0.17f;
        [SerializeField] Transform face;
        [SerializeField] Transform mouse;
        [SerializeField] Transform[] spots;           // tane dove può comparire il topolino
        [SerializeField] Transform progress;          // barra (scala x da 0 a 1)
        [SerializeField] SpeechBubble catBubble;
        [SerializeField] SpeechBubble mouseBubble;
        [SerializeField] float holdSeconds = 3f;
        [SerializeField] float sensitivity = 2.2f;
        [SerializeField] float maxAngle = 24f;
        [Header("Topolino che cambia tana (0 = resta fermo)")]
        [SerializeField] float hopEvery = 0f;
        [SerializeField] int maxHops = 6;

        Vector3[] pupilHome;
        Vector2 baseline, gaze;
        float held, hopTimer;
        int hops, spotIndex;
        bool done, primed, hopping;

        public override void OnLevelStart()
        {
            pupilHome = new Vector3[pupils.Length];
            for (int i = 0; i < pupils.Length; i++) pupilHome[i] = pupils[i].localPosition;
            mouseSize = Mathf.Abs(mouse.localScale.y);
            spotIndex = Random.Range(0, spots.Length);
            PlaceMouse(spots[spotIndex]);
            hopTimer = hopEvery;
            if (progress != null) progress.localScale = new Vector3(0f, 1f, 1f);
            if (mouseBubble != null) mouseBubble.Hide();
            if (catBubble != null) { catBubble.Show("Mrr?"); StartCoroutine(Hide(catBubble, 1.2f)); }
        }

        float mouseSize = 1f;

        void PlaceMouse(Transform spot)
        {
            mouse.position = spot.position;
            mouse.localScale = new Vector3(spot.position.x < face.position.x ? 1f : -1f, 1f, 1f) * mouseSize;
            if (mouseBubble != null) mouseBubble.transform.position = mouse.position + new Vector3(spot.position.x < 0f ? 0.8f : -0.8f, 0.75f, 0f);
        }

        /// <summary>Il topolino si infila nella tana e sbuca da un'altra (mai la stessa).</summary>
        IEnumerator Hop()
        {
            hopping = true;
            if (mouseBubble != null) mouseBubble.Hide();
            Vector3 s = mouse.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { mouse.localScale = s * (1f - k); yield return null; }
            int next = (spotIndex + Random.Range(1, spots.Length)) % spots.Length;
            spotIndex = next;
            PlaceMouse(spots[next]);
            Vector3 full = mouse.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { mouse.localScale = full * k; yield return null; }
            mouse.localScale = full;
            hopping = false;
        }

        IEnumerator Escaped()
        {
            done = true;
            if (mouseBubble != null) mouseBubble.Show("Too slow, kitty!");
            if (catBubble != null) catBubble.Show("Mrr...");
            Vector3 s = mouse.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { mouse.localScale = s * (1f - k); yield return null; }
            mouse.gameObject.SetActive(false);
            yield return new WaitForSeconds(1.0f);
            if (mouseBubble != null) mouseBubble.Hide();
            Fail();
        }

        void Update()
        {
            if (done || IsFinished) return;
            if (hopEvery > 0f && !hopping)
            {
                hopTimer -= Time.deltaTime;
                if (hopTimer <= 0f)
                {
                    hops++;
                    if (hops > maxHops) { StartCoroutine(Escaped()); return; }
                    hopTimer = hopEvery;
                    StartCoroutine(Hop());
                }
            }
            Vector3 t = SensorHub.Instance != null ? SensorHub.Instance.Tilt : Vector3.zero;
            Vector2 tilt = new Vector2(t.x, t.y);
            if (!primed) { primed = true; baseline = tilt; }   // "dritto" = come tieni il telefono all'inizio
            Vector2 target = Vector2.ClampMagnitude((tilt - baseline) * sensitivity, 1f);
            gaze = Vector2.Lerp(gaze, target, 1f - Mathf.Exp(-10f * Time.deltaTime));
            for (int i = 0; i < pupils.Length; i++) pupils[i].localPosition = pupilHome[i] + (Vector3)(gaze * maxOffset);

            Vector2 toMouse = ((Vector2)mouse.position - (Vector2)face.position).normalized;
            bool looking = !hopping && gaze.magnitude > 0.45f && Vector2.Angle(gaze, toMouse) < maxAngle;
            // fermo: va fissato di fila; a salti: il tempo si somma e cala solo piano
            float decay = hopEvery > 0f ? 0.25f : 2f;
            held = looking ? held + Time.deltaTime : Mathf.Max(0f, held - Time.deltaTime * decay);
            if (progress != null) progress.localScale = new Vector3(Mathf.Clamp01(held / holdSeconds), 1f, 1f);
            if (looking && held > 0.6f && mouseBubble != null && !mouseBubble.gameObject.activeSelf) mouseBubble.Show("Eek!");
            if (!looking && mouseBubble != null && mouseBubble.gameObject.activeSelf) mouseBubble.Hide();
            if (held >= holdSeconds) StartCoroutine(Caught());
        }

        IEnumerator Caught()
        {
            done = true;
            if (mouseBubble != null) mouseBubble.Show("EEEK!");
            if (catBubble != null) catBubble.Show("Mrrrow!");
            Vector3 from = mouse.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                mouse.position = from + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.4f;   // salto di paura e via nella tana
                mouse.localScale = mouse.localScale.normalized * Mathf.Lerp(Mathf.Abs(mouse.localScale.y), 0.01f, k * k);
                yield return null;
            }
            mouse.gameObject.SetActive(false);
            if (mouseBubble != null) mouseBubble.Hide();
            yield return new WaitForSeconds(1.2f);
            Win();
        }

        static IEnumerator Hide(SpeechBubble b, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (b != null) b.Hide();
        }
    }
}
