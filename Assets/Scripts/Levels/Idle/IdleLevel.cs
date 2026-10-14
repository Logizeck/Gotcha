using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 4 — non fare niente. Un gatto dorme e c'è un grosso pulsante rosso "PRESS ME".
    /// Si vince aspettando 5 secondi senza toccare lo schermo e senza muovere il telefono.
    /// Ogni tocco o scossone sveglia il gatto (occhi spalancati, "!") e fa ripartire l'attesa.
    /// </summary>
    public class IdleLevel : LevelController
    {
        [SerializeField] IdleTimer timer;
        [SerializeField] GameObject eyesOpen;
        [SerializeField] GameObject alert;
        [SerializeField] SmokeEmitter zzz;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] Transform button;
        [SerializeField] float awakeSeconds = 1.2f;

        float awakeUntil;
        bool awake;
        Vector3 buttonScale;

        public override void OnLevelStart()
        {
            if (button != null) buttonScale = button.localScale;
            if (timer != null)
            {
                timer.Completed += OnCompleted;
                timer.Interrupted += OnInterrupted;
            }
            Sleep();
        }

        void OnDestroy()
        {
            if (timer == null) return;
            timer.Completed -= OnCompleted;
            timer.Interrupted -= OnInterrupted;
        }

        void Update()
        {
            if (IsFinished) return;
            if (awake && Time.time >= awakeUntil) Sleep();
            if (button != null)   // il pulsante si abbassa se lo tocchi (tanto non serve a niente...)
            {
                bool pressed = Input.touchCount > 0 || Input.GetMouseButton(0);
                Vector3 target = pressed ? new Vector3(buttonScale.x, buttonScale.y * 0.82f, buttonScale.z) : buttonScale;
                button.localScale = Vector3.Lerp(button.localScale, target, 20f * Time.deltaTime);
            }
        }

        void OnInterrupted()
        {
            if (IsFinished) return;
            awakeUntil = Time.time + awakeSeconds;
            if (awake) return;
            awake = true;
            if (eyesOpen != null) eyesOpen.SetActive(true);
            if (alert != null) alert.SetActive(true);
            if (zzz != null) zzz.Stop();
        }

        void Sleep()
        {
            awake = false;
            if (eyesOpen != null) eyesOpen.SetActive(false);
            if (alert != null) alert.SetActive(false);
            if (zzz != null) zzz.Play();
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
