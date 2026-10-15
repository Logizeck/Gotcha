using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 10 — la vespa furiosa. Una vespa svolazza per lo schermo: va colpita con un tocco 10 volte.
    /// A ogni colpo sbanda, lampeggia e diventa più veloce. Al decimo colpo cade a terra a zampe all'aria, stecchita.
    /// </summary>
    public class WaspLevel : LevelController
    {
        [SerializeField] Transform wasp;
        [SerializeField] GameObject alive;      // vespa viva (corpo + ali)
        [SerializeField] GameObject dead;       // vespa stecchita
        [SerializeField] SpriteRenderer body;
        [SerializeField] AudioSource buzz;
        [SerializeField] int hitsNeeded = 10;
        [SerializeField] float hitRadius = 0.6f;
        [SerializeField] float baseSpeed = 2.2f;
        [SerializeField] float speedPerHit = 0.35f;
        [SerializeField] Vector2 areaMin = new Vector2(-2.4f, -2.0f);
        [SerializeField] Vector2 areaMax = new Vector2(2.4f, 3.0f);
        [SerializeField] float groundY = -3.6f;

        Camera cam;
        Vector2 target;
        int hits;
        float flash, stun;
        bool falling;

        public int Hits => hits;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            if (dead != null) dead.SetActive(false);
            if (alive != null) alive.SetActive(true);
            PickTarget();
            if (buzz != null)
            {
                buzz.clip = BuzzClip();
                buzz.loop = true;
                buzz.volume = 0.35f;
                buzz.Play();
            }
        }

        void OnDestroy()
        {
            if (buzz != null && buzz.clip != null) Destroy(buzz.clip);
        }

        void Update()
        {
            if (falling || IsFinished) return;

            // volo a zig-zag verso punti casuali
            if (stun > 0f) stun -= Time.deltaTime;
            else
            {
                float speed = baseSpeed + hits * speedPerHit;
                Vector2 pos = wasp.localPosition;
                Vector2 next = Vector2.MoveTowards(pos, target, speed * Time.deltaTime);
                next.y += Mathf.Sin(Time.time * 9f) * 0.012f;
                wasp.localPosition = next;
                if (Vector2.Distance(next, target) < 0.1f) PickTarget();
                float dir = target.x - pos.x;
                if (Mathf.Abs(dir) > 0.01f) wasp.localScale = new Vector3(dir < 0f ? 1f : -1f, 1f, 1f);   // guarda dove va
            }

            if (flash > 0f) flash -= Time.deltaTime;
            if (body != null) body.color = flash > 0f ? new Color(1f, 0.45f, 0.45f) : Color.white;
            if (buzz != null) buzz.pitch = 1f + hits * 0.06f;

            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && began && !PointerInput.IsOverUi())
            {
                Vector2 world = PointerInput.ToWorld(cam, sp);
                if (Vector2.Distance(world, wasp.position) <= hitRadius) Hit();
            }
        }

        void PickTarget()
        {
            target = new Vector2(Random.Range(areaMin.x, areaMax.x), Random.Range(areaMin.y, areaMax.y));
        }

        void Hit()
        {
            hits++;
            flash = 0.15f;
            stun = 0.12f;
            wasp.localPosition += (Vector3)(Random.insideUnitCircle.normalized * 0.35f);   // sbanda
            PickTarget();
            if (hits >= hitsNeeded) StartCoroutine(Fall());
        }

        IEnumerator Fall()
        {
            falling = true;
            if (buzz != null) buzz.Stop();
            if (alive != null) alive.SetActive(false);
            if (dead != null) dead.SetActive(true);
            wasp.localScale = Vector3.one;
            float v = 0f;
            float spin = 0f;
            while (wasp.localPosition.y > groundY)
            {
                v += 14f * Time.deltaTime;
                spin += 720f * Time.deltaTime;
                wasp.localPosition += Vector3.down * (v * Time.deltaTime);
                wasp.localRotation = Quaternion.Euler(0, 0, spin);
                yield return null;
            }
            wasp.localPosition = new Vector3(wasp.localPosition.x, groundY, 0f);
            wasp.localRotation = Quaternion.identity;   // a pancia in su, zampe all'aria
            // piccolo rimbalzo
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                wasp.localPosition = new Vector3(wasp.localPosition.x, groundY + Mathf.Sin(k * Mathf.PI) * 0.15f, 0f);
                yield return null;
            }
            yield return new WaitForSeconds(1.0f);
            Win();
        }

        /// <summary>Ronzio: onda a dente di sega modulata, generata da codice.</summary>
        static AudioClip BuzzClip()
        {
            const int rate = 22050;
            int n = rate;   // un secondo in loop
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float f = 210f + Mathf.Sin(t * Mathf.PI * 2f * 6f) * 14f;
                float saw = 2f * Mathf.Repeat(t * f, 1f) - 1f;
                data[i] = saw * 0.35f * (0.75f + 0.25f * Mathf.Sin(t * Mathf.PI * 2f * 30f));
            }
            AudioClip clip = AudioClip.Create("buzz", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
