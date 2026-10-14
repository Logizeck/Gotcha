using System;
using UnityEngine;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Components
{
    /// <summary>
    /// Scatta se il giocatore non tocca lo schermo (e, se richiesto, non muove il telefono) per N secondi.
    /// Ogni tocco o scossone azzera il timer e lancia Interrupted.
    /// </summary>
    public class IdleTimer : MonoBehaviour
    {
        public float seconds = 7f;
        public bool ignoreUi = true;   // i tocchi su Menu/Hint non contano
        [Tooltip("Anche muovere o scuotere il telefono azzera il timer.")]
        public bool resetOnMotion = false;
        [Tooltip("Soglia di movimento (SensorHub.ShakeStrength): 1 ≈ telefono sollevato, 3+ = scossone.")]
        public float motionThreshold = 1.2f;

        public float Elapsed { get; private set; }
        public float Progress => Mathf.Clamp01(Elapsed / seconds);
        public event Action Completed;
        public event Action Interrupted;

        bool done;

        void Update()
        {
            if (done) return;
            bool touching = Input.touchCount > 0 || Input.GetMouseButton(0);
            bool interrupted = touching && !(ignoreUi && PointerInput.IsOverUi());
            if (!interrupted && resetOnMotion && SensorHub.Instance != null)
                interrupted = SensorHub.Instance.ShakeStrength > motionThreshold;

            if (interrupted)
            {
                Elapsed = 0f;
                Interrupted?.Invoke();
            }
            else Elapsed += Time.deltaTime;

            if (Elapsed >= seconds) { done = true; Completed?.Invoke(); }
        }
    }
}
