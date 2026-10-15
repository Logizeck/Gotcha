using System.Collections.Generic;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Scioglimento del pupazzo di neve, riutilizzabile (livelli 11 e 29).
    /// All'inizio le parti di neve separate vengono sostituite da un unico disegno (sm_body) con lo shader
    /// Gotcha/SnowMelt: la neve si affloscia verso terra a colonne irregolari, cola, si allarga e diventa bagnata,
    /// fino a restare una pozzanghera. Non è neve (e quindi non si scioglie) tutto ciò che è attaccato: sciarpa,
    /// cappello, braccia, carota, carbone... Questi "seguaci" scendono insieme alla neve sotto di loro e, quando
    /// arrivano giù, galleggiano nella pozzanghera. I "decalcomanie" (bocca, rossore) sbiadiscono.
    /// La formula dell'abbassamento è la stessa dello shader.
    /// </summary>
    public class SnowmanMelter : MonoBehaviour
    {
        [Tooltip("Radice del pupazzo, ai piedi.")]
        public Transform snowman;
        public SpriteRenderer[] parts;          // neve a pezzi (si nascondono quando parte lo scioglimento)
        public SpriteRenderer body;             // neve intera con lo shader di scioglimento
        public float bodyXMin = -2.6f, bodyXMax = 2.6f, bodyYMin = -0.4f, bodyYMax = 4.85f;
        public float topY = 4.7f;
        public float seed;
        public Transform puddle;
        public Vector3 puddleFullScale = new Vector3(1.2f, 1.1f, 1f);
        public SmokeEmitter drips;
        public SmokeEmitter steam;
        public float waterY = 0.06f;

        public float Progress { get; private set; }
        public bool Begun { get; private set; }

        static readonly int MeltId = Shader.PropertyToID("_Melt");
        static readonly int GroundId = Shader.PropertyToID("_Ground");
        static readonly int SeedId = Shader.PropertyToID("_Seed");

        class Follower
        {
            public Transform t;
            public Vector2 anchor;      // punto di neve a cui è attaccato (coordinate del pupazzo, prima dello scioglimento)
            public Vector3 offset;      // differenza tra oggetto e punto di attacco
            public bool decal;          // sbiadisce invece di cadere
            public SpriteRenderer[] renderers;
            public bool loose, floating;
            public Vector2 v;
            public float spin, phase, floatX;
        }

        readonly List<Follower> followers = new List<Follower>();
        MaterialPropertyBlock block;

        /// <summary>Oggetto non di neve attaccato al pupazzo: scende con la neve sotto <paramref name="anchorWorld"/> e poi galleggia.</summary>
        public void Attach(Transform t, bool decal = false) { Attach(t, t.position, decal); }

        public void Attach(Transform t, Vector3 anchorWorld, bool decal)
        {
            if (t == null) return;
            foreach (Follower f in followers) if (f.t == t) return;
            t.SetParent(snowman, true);
            Vector2 a = snowman.InverseTransformPoint(anchorWorld);
            followers.Add(new Follower
            {
                t = t, anchor = a, offset = t.localPosition - (Vector3)a, decal = decal,
                renderers = t.GetComponentsInChildren<SpriteRenderer>(true),
                spin = Random.Range(-200f, 200f), phase = Random.value * 6f
            });
            foreach (Draggable2D d in t.GetComponentsInChildren<Draggable2D>()) d.enabled = false;
        }

        public void Begin()
        {
            if (Begun) return;
            Begun = true;
            block = new MaterialPropertyBlock();
            foreach (SpriteRenderer r in parts) if (r != null) r.enabled = false;
            if (body != null) body.gameObject.SetActive(true);
            if (drips != null) drips.Play();
            if (steam != null) steam.Play();
            Apply();
        }

        public void SetProgress(float p)
        {
            if (!Begun) Begin();
            Progress = Mathf.Clamp01(Mathf.Max(Progress, p));
            Apply();
        }

        public void End()
        {
            if (drips != null) drips.Stop();
            if (steam != null) steam.Stop();
        }

        // ---------- stessa formula dello shader ----------

        float U(float x) { return (x - bodyXMin) / (bodyXMax - bodyXMin); }
        float V(float y) { return (y - bodyYMin) / (bodyYMax - bodyYMin); }

        float Noise(float u) { return 0.5f + 0.3f * Mathf.Sin(u * 19f + seed) + 0.2f * Mathf.Sin(u * 41f + 2f + seed * 0.7f); }

        /// <summary>Quanta altezza resta alla colonna di neve in x (1 = intatta, 0 = sciolta).</summary>
        public float Keep(float x)
        {
            float u = U(x), edge = Mathf.Abs(u - 0.5f) * 2f;
            return Mathf.Clamp(1f - Progress * (1f + 0.18f * Noise(u) + 0.7f * Mathf.Pow(edge, 1.5f)), 0.0001f, 1f);
        }

        /// <summary>Dove si trova ora un punto di neve che prima dello scioglimento stava in <paramref name="p"/>.</summary>
        public Vector2 Displace(Vector2 p)
        {
            float ground = 0f;
            float h = Mathf.Max(0f, p.y - ground) * Keep(p.x);
            float hv = h / (bodyYMax - bodyYMin);
            float spread = 1f + Progress * 1.6f * Mathf.Exp(-hv * 9f);
            return new Vector2(p.x * spread, ground + h);
        }

        void Apply()
        {
            if (body != null)
            {
                body.GetPropertyBlock(block);
                block.SetFloat(MeltId, Progress);
                block.SetFloat(GroundId, V(0f));
                block.SetFloat(SeedId, seed);
                body.SetPropertyBlock(block);
            }
            float top = Displace(new Vector2(-0.1f, topY)).y;
            bool any = top > 0.2f && Progress < 1f;
            if (drips != null)
            {
                drips.transform.localPosition = new Vector3(-0.1f, Mathf.Max(0.2f, top - 0.2f), 0f);
                drips.spread = Mathf.Lerp(0.6f, 1.6f, Progress);
                if (!any) drips.Stop();
            }
            if (steam != null)
            {
                steam.transform.localPosition = new Vector3(-0.1f, top, 0f);
                steam.spread = Mathf.Lerp(0.5f, 1.4f, Progress);
                if (!any) steam.Stop();
            }
            if (puddle != null)
            {
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(Progress * 1.3f));
                puddle.localScale = Vector3.Scale(puddleFullScale, new Vector3(Mathf.Lerp(0.3f, 1f, k), k, 1f));
            }

            foreach (Follower f in followers)
            {
                if (f.t == null || f.loose) continue;
                Vector2 d = Displace(f.anchor);
                if (f.decal)
                {
                    // la bocca e il rossore scendono con la neve e sbiadiscono man mano che la faccia si scioglie
                    f.t.localPosition = (Vector3)d + f.offset * Mathf.Max(0.3f, Keep(f.anchor.x));
                    float a = Mathf.Clamp01((Keep(f.anchor.x) - 0.25f) / 0.35f);
                    foreach (SpriteRenderer r in f.renderers) { Color c = r.color; c.a = a; r.color = c; }
                    continue;
                }
                f.t.localPosition = (Vector3)d + f.offset;
                // arrivato giù (o la neve sotto non lo regge più): si stacca e cade nella pozzanghera
                if (d.y < 0.45f || (Keep(f.anchor.x) < 0.12f)) Release(f);
            }
        }

        void Release(Follower f)
        {
            f.loose = true;
            f.v = new Vector2((f.t.localPosition.x >= -0.1f ? 1f : -1f) * Random.Range(0.3f, 0.9f), Random.Range(0.6f, 1.4f));
        }

        void Update()
        {
            float reach = 1.9f * puddleFullScale.x;
            foreach (Follower f in followers)
            {
                if (f.t == null || !f.loose) continue;
                if (f.floating)
                {
                    // galleggia, dondolando sull'acqua
                    float t = Time.time * 1.6f + f.phase;
                    f.t.localPosition = new Vector3(f.floatX + Mathf.Sin(t * 0.7f) * 0.05f, waterY + Mathf.Sin(t * 1.4f) * 0.03f, f.t.localPosition.z);
                    f.t.localRotation = Quaternion.Euler(0, 0, f.spin * 0.1f + Mathf.Sin(t) * 7f);
                    continue;
                }
                f.v.y -= 11f * Time.deltaTime;
                f.t.localPosition += (Vector3)(f.v * Time.deltaTime);
                f.t.Rotate(0, 0, f.spin * Time.deltaTime);
                if (f.t.localPosition.y <= waterY && f.v.y < 0f)
                {
                    f.floating = true;
                    f.floatX = Mathf.Clamp(f.t.localPosition.x, -reach, reach);
                }
            }
        }

        /// <summary>Smette di seguire la neve (es. gli occhi che schizzano via): lo muove qualcun altro.</summary>
        public void Detach(Transform t) { followers.RemoveAll(f => f.t == t); }

        /// <summary>Fa cadere subito un seguace (es. il cappello che salta via all'urlo).</summary>
        public void Drop(Transform t, Vector2 velocity)
        {
            foreach (Follower f in followers)
                if (f.t == t) { f.loose = true; f.v = velocity; return; }
        }
    }
}
