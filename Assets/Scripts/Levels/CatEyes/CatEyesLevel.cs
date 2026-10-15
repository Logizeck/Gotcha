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

        Vector3[] pupilHome;
        Vector2 baseline, gaze;
        float held;
        bool done, primed;

        public override void OnLevelStart()
        {
            pupilHome = new Vector3[pupils.Length];
            for (int i = 0; i < pupils.Length; i++) pupilHome[i] = pupils[i].localPosition;
            Transform spot = spots[Random.Range(0, spots.Length)];
            mouse.position = spot.position;
            mouse.localScale = new Vector3(spot.position.x < face.position.x ? 1f : -1f, 1f, 1f) * Mathf.Abs(mouse.localScale.y);
            if (progress != null) progress.localScale = new Vector3(0f, 1f, 1f);
            if (mouseBubble != null) mouseBubble.transform.position = mouse.position + new Vector3(spot.position.x < 0f ? 0.8f : -0.8f, 0.75f, 0f);
            if (mouseBubble != null) mouseBubble.Hide();
            if (catBubble != null) { catBubble.Show("Mrr?"); StartCoroutine(Hide(catBubble, 1.2f)); }
        }

        void Update()
        {
            if (done || IsFinished) return;
            Vector3 t = SensorHub.Instance != null ? SensorHub.Instance.Tilt : Vector3.zero;
            Vector2 tilt = new Vector2(t.x, t.y);
            if (!primed) { primed = true; baseline = tilt; }   // "dritto" = come tieni il telefono all'inizio
            Vector2 target = Vector2.ClampMagnitude((tilt - baseline) * sensitivity, 1f);
            gaze = Vector2.Lerp(gaze, target, 1f - Mathf.Exp(-10f * Time.deltaTime));
            for (int i = 0; i < pupils.Length; i++) pupils[i].localPosition = pupilHome[i] + (Vector3)(gaze * maxOffset);

            Vector2 toMouse = ((Vector2)mouse.position - (Vector2)face.position).normalized;
            bool looking = gaze.magnitude > 0.45f && Vector2.Angle(gaze, toMouse) < maxAngle;
            held = looking ? held + Time.deltaTime : Mathf.Max(0f, held - Time.deltaTime * 2f);
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
