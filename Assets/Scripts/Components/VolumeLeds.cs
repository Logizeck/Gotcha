using UnityEngine;
using Gotcha.Sensors;

namespace Gotcha.Components
{
    /// <summary>
    /// Barre LED dello stereo: mostrano il volume del telefono, dal verde al rosso. Le barre accese tremolano
    /// leggermente a tempo di musica; quelle spente restano appena visibili.
    /// </summary>
    public class VolumeLeds : MonoBehaviour
    {
        public SpriteRenderer[] bars;
        [Range(0f, 1f)] public float offAlpha = 0.16f;
        public bool dance = true;

        Color[] colors;

        void Awake()
        {
            colors = new Color[bars.Length];
            for (int i = 0; i < bars.Length; i++) if (bars[i] != null) colors[i] = bars[i].color;
        }

        void Update()
        {
            float v = SensorHub.Instance != null ? SensorHub.Instance.SystemVolume01 : 1f;
            // ritmo finto ma credibile: l'ultima barra accesa "balla" un po'
            float beat = dance && v > 0.001f ? (Mathf.PerlinNoise(Time.time * 6f, 0.3f) - 0.5f) * 1.6f : 0f;
            float lit = v * bars.Length + beat;
            for (int i = 0; i < bars.Length; i++)
            {
                if (bars[i] == null) continue;
                bool on = v > 0.001f && i < Mathf.Max(1f, lit);
                Color c = colors[i];
                bars[i].color = on ? c : new Color(c.r * 0.6f, c.g * 0.6f, c.b * 0.6f, offAlpha);
            }
        }
    }
}
