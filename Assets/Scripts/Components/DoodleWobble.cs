using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Effetto "disegno a mano che vibra" (line boil dei cartoni): a scatti regolari il disegno
    /// ruota e si deforma di pochissimo. Da mettere solo sugli oggetti grafici, non su quelli con fisica.
    /// </summary>
    public class DoodleWobble : MonoBehaviour
    {
        public float interval = 0.15f;
        public float maxAngle = 1.2f;
        public float maxStretch = 0.015f;

        Vector3 baseScale;
        Quaternion baseRotation;
        float timer;

        void Start()
        {
            baseScale = transform.localScale;
            baseRotation = transform.localRotation;
            timer = Random.value * interval;
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer < interval) return;
            timer = 0f;
            transform.localRotation = baseRotation * Quaternion.Euler(0, 0, Random.Range(-maxAngle, maxAngle));
            transform.localScale = new Vector3(
                baseScale.x * (1f + Random.Range(-maxStretch, maxStretch)),
                baseScale.y * (1f + Random.Range(-maxStretch, maxStretch)),
                baseScale.z);
        }
    }
}
