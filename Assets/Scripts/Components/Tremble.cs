using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>Tremolio nervoso: piccoli scatti casuali attorno alla posizione di partenza (ampiezza regolabile da codice).</summary>
    public class Tremble : MonoBehaviour
    {
        public float amplitude = 0.03f;
        public float rotation = 2f;

        Vector3 basePos;
        Quaternion baseRot;

        void Awake()
        {
            basePos = transform.localPosition;
            baseRot = transform.localRotation;
        }

        void Update()
        {
            transform.localPosition = basePos + (Vector3)(Random.insideUnitCircle * amplitude);
            transform.localRotation = baseRot * Quaternion.Euler(0, 0, Random.Range(-rotation, rotation) * amplitude / 0.03f);
        }

        void OnDisable()
        {
            transform.localPosition = basePos;
            transform.localRotation = baseRot;
        }
    }
}
