using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>Emette sbuffi di fumo che salgono, si allargano e svaniscono (nessun sistema particellare).</summary>
    public class SmokeEmitter : MonoBehaviour
    {
        public Sprite puffSprite;
        public float puffsPerSecond = 7f;
        public float life = 1.6f;
        public float riseSpeed = 0.9f;
        public float startScale = 0.6f;
        public float spread = 0.35f;
        public int sortingOrder = 40;
        public Color tint = new Color(0.85f, 0.86f, 0.9f, 0.95f);
        public float spinRange = 60f;    // gradi al secondo, a caso tra -spinRange e +spinRange
        public float growth = 1.4f;      // quanto si allarga lo sbuffo durante la vita
        public float sideDrift = 0.25f;

        public bool IsPlaying { get; private set; }

        float timer;

        public void Play() { if (!IsPlaying) timer = 1f / puffsPerSecond; IsPlaying = true; }
        public void Stop() { IsPlaying = false; }

        void Update()
        {
            if (!IsPlaying || puffSprite == null) return;
            timer += Time.deltaTime;
            float step = 1f / puffsPerSecond;
            while (timer >= step)
            {
                timer -= step;
                Spawn();
            }
        }

        void Spawn()
        {
            var go = new GameObject("Puff");
            // gli sbuffi non seguono l'emettitore (se il pupazzo si scioglie restano nell'aria)
            Levels.LevelController level = GetComponentInParent<Levels.LevelController>();
            go.transform.SetParent(level != null ? level.transform : null, true);
            go.transform.position = transform.position + new Vector3(Random.Range(-spread, spread), Random.Range(-0.05f, 0.1f), 0);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = puffSprite;
            sr.color = tint;
            sr.sortingOrder = sortingOrder;
            SmokePuff puff = go.AddComponent<SmokePuff>();
            puff.Init(life, new Vector3(Random.Range(-sideDrift, sideDrift), riseSpeed * Random.Range(0.8f, 1.2f), 0),
                      startScale * Random.Range(0.8f, 1.2f), Random.Range(-spinRange, spinRange), growth);
        }
    }
}
