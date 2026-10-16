using UnityEngine;
using Gotcha.Sensors;

namespace Gotcha.Components
{
    /// <summary>Scorciatoie per i sensori nei livelli (con i tasti dell'Editor per provarli).</summary>
    public static class Inputs
    {
        /// <summary>Volume del microfono 0..1 (Editor: B = soffio/voce normale, N = fortissimo).</summary>
        public static float Mic(Object owner)
        {
            float level = 0f;
            if (SensorHub.Instance != null) { SensorHub.Instance.UseMic(owner); level = SensorHub.Instance.MicLevel01; }
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.B)) level = Mathf.Max(level, 0.6f);
            if (Input.GetKey(KeyCode.N)) level = 1f;
#endif
            return level;
        }

        public static void ReleaseMic(Object owner) { if (SensorHub.Instance != null) SensorHub.Instance.ReleaseMic(owner); }

        /// <summary>Forza dello scuotimento (Editor: S = scossone).</summary>
        public static float Shake()
        {
            float s = SensorHub.Instance != null ? SensorHub.Instance.ShakeStrength : 0f;
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.S)) s = 20f;
#endif
            return s;
        }

        /// <summary>Inclinazione (x, y) del telefono; nell'Editor le frecce.</summary>
        public static Vector2 Tilt()
        {
            Vector3 t = SensorHub.Instance != null ? SensorHub.Instance.Tilt : Vector3.zero;
            return new Vector2(t.x, t.y);
        }
    }
}
