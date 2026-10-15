using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>Negli urti forti l'oggetto perde un po' di briciole (sbuffo breve di un SmokeEmitter).</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ImpactCrumbs : MonoBehaviour
    {
        public SmokeEmitter crumbs;
        public float minImpact = 2.5f;
        public AudioSource tick;

        float stopAt;

        void OnCollisionEnter2D(Collision2D c)
        {
            float speed = c.relativeVelocity.magnitude;
            if (speed < minImpact) return;
            if (crumbs != null)
            {
                crumbs.transform.position = c.GetContact(0).point;
                crumbs.Play();
                stopAt = Time.time + Mathf.Lerp(0.05f, 0.18f, Mathf.InverseLerp(minImpact, 10f, speed));
            }
            if (tick != null) { tick.volume = Mathf.InverseLerp(minImpact * 0.5f, 10f, speed) * 0.6f; tick.Play(); }
        }

        void Update()
        {
            if (crumbs != null && stopAt > 0f && Time.time >= stopAt) { crumbs.Stop(); stopAt = 0f; }
        }
    }
}
