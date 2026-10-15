using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Raggio laser intermittente: acceso per onTime, spento per offTime. Prima di accendersi tremola (avviso).
    /// Il raggio è un segmento verticale lungo <see cref="length"/> centrato sull'oggetto.
    /// </summary>
    public class LaserBeam : MonoBehaviour
    {
        public SpriteRenderer beam;
        public SpriteRenderer[] lenses;
        public float length = 1.5f;
        public float onTime = 1.4f;
        public float offTime = 1.6f;
        public float phase;
        public float warnTime = 0.35f;

        public bool IsOn { get; private set; }

        float elapsed;

        void Update()
        {
            elapsed += Time.deltaTime;
            float cycle = onTime + offTime;
            float t = Mathf.Repeat(elapsed + phase, cycle);
            IsOn = t < onTime;
            bool warning = !IsOn && t > cycle - warnTime;
            if (beam != null)
            {
                beam.enabled = IsOn || (warning && Mathf.Repeat(Time.time * 18f, 1f) < 0.5f);
                Color c = beam.color;
                c.a = IsOn ? 0.85f + Mathf.Sin(Time.time * 40f) * 0.15f : 0.3f;
                beam.color = c;
            }
            if (lenses != null)
                foreach (SpriteRenderer l in lenses) if (l != null) l.color = IsOn ? Color.white : new Color(0.6f, 0.6f, 0.6f);
        }

        /// <summary>Distanza di un punto (mondo) dal raggio.</summary>
        public float DistanceTo(Vector2 p)
        {
            Vector2 c = transform.position;
            float half = length * 0.5f * transform.lossyScale.y;
            float y = Mathf.Clamp(p.y, c.y - half, c.y + half);
            return Vector2.Distance(p, new Vector2(c.x, y));
        }
    }
}
