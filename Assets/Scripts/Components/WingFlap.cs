using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>Battito d'ali: l'ala si schiaccia e si allarga velocemente.</summary>
    public class WingFlap : MonoBehaviour
    {
        public float speed = 40f;
        public float minScale = 0.35f;

        Vector3 baseScale;

        void Awake() { baseScale = transform.localScale; }

        void Update()
        {
            float k = Mathf.Lerp(minScale, 1f, (Mathf.Sin(Time.time * speed) + 1f) * 0.5f);
            transform.localScale = new Vector3(baseScale.x, baseScale.y * k, baseScale.z);
        }
    }
}
