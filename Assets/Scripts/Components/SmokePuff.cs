using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>Singolo elemento creato da SmokeEmitter (sbuffo, Z del sonno, cuoricino): sale, cresce, ruota e svanisce.</summary>
    public class SmokePuff : MonoBehaviour
    {
        float life, age, startScale, spin, growth;
        Vector3 velocity;
        SpriteRenderer sr;
        Color baseColor;

        public void Init(float life, Vector3 velocity, float startScale, float spin, float growth = 1.4f)
        {
            this.life = life;
            this.velocity = velocity;
            this.startScale = startScale;
            this.spin = spin;
            this.growth = growth;
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
            transform.localScale = Vector3.one * startScale * (1f + k * growth);
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (1f - k * k));
        }
    }
}
