using System;
using UnityEngine;
using Gotcha.Sensors;

namespace Gotcha.Components
{
    public enum Comparison { Above, Below }

    /// <summary>
    /// Condizione generica su un sensore: "il canale X resta sopra/sotto la soglia Y per Z secondi".
    /// Esempi: volume sotto 0.01 (silenzio), microfono sopra 0.5 (soffiare), shake sopra soglia.
    /// </summary>
    public class SensorCondition : MonoBehaviour
    {
        public SensorChannel channel;
        public Comparison comparison = Comparison.Above;
        public float threshold = 0.5f;
        public float holdSeconds = 0f;
        [Tooltip("Scatta solo dopo aver visto la condizione NON soddisfatta almeno una volta (evita vittorie istantanee).")]
        public bool armOnlyAfterUnmet = true;

        public event Action Met;

        bool armed;
        bool done;
        float held;

        void OnEnable()
        {
            armed = !armOnlyAfterUnmet;
            if (channel == SensorChannel.MicLevel && SensorHub.Instance != null) SensorHub.Instance.EnableMic();
        }

        void OnDisable()
        {
            if (channel == SensorChannel.MicLevel && SensorHub.Instance != null) SensorHub.Instance.DisableMic();
        }

        void Update()
        {
            if (done || SensorHub.Instance == null) return;
            if (channel == SensorChannel.MicLevel) SensorHub.Instance.EnableMic();

            float v = SensorHub.Instance.Read(channel);
            bool ok = comparison == Comparison.Above ? v > threshold : v < threshold;

            if (!armed) { if (!ok) armed = true; return; }

            held = ok ? held + Time.deltaTime : 0f;
            if (held >= holdSeconds && ok) { done = true; Met?.Invoke(); }
        }
    }
}
