using UnityEngine;
using Gotcha.Sensors;

namespace Gotcha.Components
{
    /// <summary>
    /// Mentre è attivo, la gravità 2D segue l'inclinazione del telefono (frecce nell'Editor).
    /// La risposta è più che lineare sulle piccole inclinazioni, così la sfera reagisce subito.
    /// </summary>
    public class TiltGravity : MonoBehaviour
    {
        public float strength = 32f;
        [Tooltip("1 = lineare; valori più bassi rendono più reattive le piccole inclinazioni.")]
        [Range(0.3f, 1f)] public float response = 0.65f;
        [Tooltip("Inclinazioni più piccole di così sono ignorate (telefono appoggiato).")]
        public float deadZone = 0.015f;
        [Tooltip("Filtro del rumore dell'accelerometro (secondi). 0 = nessun filtro (risposta immediata).")]
        public float smoothing = 0f;

        Vector2 saved, filtered;
        bool primed;

        void OnEnable() { saved = Physics2D.gravity; }
        void OnDisable() { Physics2D.gravity = saved; }

        void Update()
        {
            Vector3 t = SensorHub.Instance != null ? SensorHub.Instance.Tilt : Vector3.zero;
            Vector2 tilt = new Vector2(t.x, t.y);
            if (smoothing > 0f)
            {
                filtered = primed ? Vector2.Lerp(filtered, tilt, 1f - Mathf.Exp(-Time.deltaTime / smoothing)) : tilt;
                primed = true;
                tilt = filtered;
            }
            float m = tilt.magnitude;
            Physics2D.gravity = m < deadZone ? Vector2.zero : tilt / m * (Mathf.Pow(Mathf.Clamp01(m), response) * strength);
        }
    }
}
