using UnityEngine;
using Gotcha.Sensors;

namespace Gotcha.Components
{
    /// <summary>
    /// Inclinazione laterale "calma" per gli oggetti che si spostano inclinando il telefono (padella, cesto):
    /// filtra il tremolio dell'accelerometro, ignora i piccoli movimenti attorno alla posizione iniziale e, quando
    /// il telefono viene scosso (lancio, scossone), resta ferma un attimo: così il colpo non sposta l'oggetto.
    /// </summary>
    public class SteadyTilt
    {
        public float smoothing = 0.22f;   // secondi del filtro passa-basso
        public float deadZone = 0.035f;   // inclinazione minima perché l'oggetto si muova
        public float shakeLevel = 6f;     // oltre questa scossa l'inclinazione si congela...
        public float shakeHold = 0.4f;    // ...per questi secondi

        float filtered, baseline, hold;
        bool primed;

        /// <summary>Scostamento laterale rispetto a come era tenuto il telefono all'inizio.</summary>
        public float Update(float dt)
        {
            SensorHub hub = SensorHub.Instance;
            float x = hub != null ? hub.Tilt.x : 0f;
            if (!primed) { primed = true; baseline = filtered = x; }
            if (hub != null && hub.ShakeStrength > shakeLevel) hold = shakeHold;
            if (hold > 0f) hold -= dt;
            else filtered = Mathf.Lerp(filtered, x, 1f - Mathf.Exp(-dt / smoothing));
            float d = filtered - baseline;
            return Mathf.Sign(d) * Mathf.Max(0f, Mathf.Abs(d) - deadZone);
        }
    }
}
