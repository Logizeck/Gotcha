using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>Singolo sbuffo creato da SmokeEmitter: sale, cresce, ruota piano e svanisce.</summary>
    public class SmokePuff : MonoBehaviour
    {
        float life, age, startScale, spin;
        Vector3 velocity;
        SpriteRenderer sr;
        Color baseColor;

        public void Init(float life, Vector3 velocity, float startScale, float spin)
        {
            this.life = life;
            this.velocity = velocity;
            this.startScale = startScale;
            this.spin = spin;
            sr = GetComponent<SpriteRenderer>();
            baseColor = sr.color;
            transform.localScale = Vector3.one * startScale;
        }

        void Update()
        {
            age += Time.deltaTime;
            float k = age / life;
            if (k >= 1f) { Destroy(gameObject); return; }
            transform.position += velocity * Time.deltaTime;
            transform.Rotate(0, 0, spin * Time.deltaTime);
            transform.localScale = Vector3.one * startScale * (1f + k * 1.4f);
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (1f - k * k));
        }
    }
}
