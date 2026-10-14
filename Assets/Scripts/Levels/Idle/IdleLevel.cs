using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 4 — il gatto affamato. All'inizio il gatto è sveglio e miagola: ha fame. La soluzione è prendere
    /// il pesce dal quadro e darglielo; dopo aver mangiato si addormenta, e a quel punto bisogna lasciarlo dormire
    /// per 5 secondi senza toccare lo schermo né muovere il telefono. Il pulsantone rosso "PRESS ME" è un'esca:
    /// toccarlo (o toccare qualsiasi cosa) lo sveglia.
    /// </summary>
    public class IdleLevel : LevelController
    {
        [SerializeField] Transform cat;
        [SerializeField] IdleTimer timer;
        [SerializeField] GameObject eyesOpen;
        [SerializeField] GameObject alert;
        [SerializeField] SmokeEmitter zzz;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] SpeechBubble meow;
        [SerializeField] Draggable2D fish;
        [SerializeField] Transform button;

        [Header("Coordinate locali del gatto")]
        [SerializeField] Vector2 mouth = new Vector2(-0.7f, 0.16f);
        [SerializeField] Vector2 headCenter = new Vector2(-0.72f, 0.33f);
        [SerializeField] float feedRadius = 0.85f;
        [SerializeField] float floorY = -2.75f;
        [SerializeField] float awakeSeconds = 1.2f;
        [SerializeField] float meowEvery = 3f;

        enum State { Hungry, Eating, Sleeping }

        State state;
        float awakeUntil, nextMeow;
        bool startled;
        Vector3 buttonScale;

        public override void OnLevelStart()
        {
            if (button != null) buttonScale = button.localScale;
            if (timer != null)
            {
                timer.enabled = false;   // l'attesa conta solo dopo che il gatto ha mangiato
                timer.Completed += OnCompleted;
                timer.Interrupted += OnInterrupted;
            }
            if (fish != null) fish.Dropped += OnFishDropped;
            if (meow != null) meow.Hide();
            state = State.Hungry;
            nextMeow = Time.time + 1f;
            SetAwakeLook(true, false);
        }

        void OnDestroy()
        {
            if (timer != null)
            {
                timer.Completed -= OnCompleted;
                timer.Interrupted -= OnInterrupted;
            }
            if (fish != null) fish.Dropped -= OnFishDropped;
        }

        void Update()
        {
            if (IsFinished) return;
            bool pressed = Input.touchCount > 0 || Input.GetMouseButton(0);
            if (button != null)   // il pulsante si abbassa se lo tocchi (tanto non serve a niente...)
            {
                Vector3 target = pressed ? new Vector3(buttonScale.x, buttonScale.y * 0.82f, buttonScale.z) : buttonScale;
                button.localScale = Vector3.Lerp(button.localScale, target, 20f * Time.deltaTime);
            }

            if (state == State.Hungry && Time.time >= nextMeow && meow != null)
            {
                nextMeow = Time.time + meowEvery;
                meow.Show("Meow!");
                StartCoroutine(HideLater(meow, 1.2f));
            }
            if (state == State.Sleeping && startled && Time.time >= awakeUntil) GoToSleep();
        }

        // ---------- Pesce ----------

        void OnFishDropped(Draggable2D item, DropSlot slot)
        {
            if (state != State.Hungry) return;
            Vector2 local = cat.InverseTransformPoint(item.transform.position);
            if (Vector2.Distance(local, headCenter) <= feedRadius)
                StartCoroutine(EatRoutine());
            else
                StartCoroutine(FallToFloor(item.transform));
        }

        IEnumerator EatRoutine()
        {
            state = State.Eating;
            fish.enabled = false;
            if (meow != null) meow.Hide();
            Transform f = fish.transform;
            Vector3 from = f.position, to = cat.TransformPoint(mouth);
            to.z = from.z;
            Vector3 scale = f.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                f.position = Vector3.Lerp(from, to, k);
                f.localScale = scale * (1f - k);
                yield return null;
            }
            f.gameObject.SetActive(false);

            if (hearts != null) hearts.Play();
            yield return new WaitForSeconds(0.8f);
            if (hearts != null) hearts.Stop();
            yield return new WaitForSeconds(0.4f);
            GoToSleep();
            if (timer != null) timer.enabled = true;
        }

        IEnumerator FallToFloor(Transform t)
        {
            float target = floorY + Random.Range(-0.3f, 0.2f);
            float v = 0f;
            while (t != null && t.position.y > target)
            {
                if (Draggable2D.Active != null && Draggable2D.Active.transform == t) yield break;
                v += 18f * Time.deltaTime;
                t.position += Vector3.down * (v * Time.deltaTime);
                yield return null;
            }
        }

        static IEnumerator HideLater(SpeechBubble b, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (b != null) b.Hide();
        }

        // ---------- Sonno ----------

        void GoToSleep()
        {
            state = State.Sleeping;
            startled = false;
            SetAwakeLook(false, false);
            if (zzz != null) zzz.Play();
        }

        void OnInterrupted()
        {
            if (IsFinished || state != State.Sleeping) return;
            awakeUntil = Time.time + awakeSeconds;
            if (startled) return;
            startled = true;
            SetAwakeLook(true, true);
            if (zzz != null) zzz.Stop();
        }

        void SetAwakeLook(bool eyes, bool exclamation)
        {
            if (eyesOpen != null) eyesOpen.SetActive(eyes);
            if (alert != null) alert.SetActive(exclamation);
        }

        void OnCompleted()
        {
            if (IsFinished) return;
            StartCoroutine(WinRoutine());
        }

        IEnumerator WinRoutine()
        {
            if (zzz != null) zzz.Stop();
            if (hearts != null) hearts.Play();
            yield return new WaitForSeconds(1.4f);
            if (hearts != null) hearts.Stop();
            Win();
        }
    }
}
