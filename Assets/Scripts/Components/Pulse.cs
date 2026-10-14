using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>Pulsazione continua della scala (es. traguardo luminoso).</summary>
    public class Pulse : MonoBehaviour
    {
        public float amount = 0.12f;
        public float speed = 3f;

        Vector3 baseScale;

        void Start() { baseScale = transform.localScale; }

        void Update()
        {
            transform.localScale = baseScale * (1f + Mathf.Sin(Time.time * speed) * amount);
        }
    }
}
