using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 101 — il topo furtivo. Il gatto di guardia gira la torcia di qua e di là. Trascina il topolino fino al
    /// formaggio restando al buio: i mobili (sei, messi a caso a ogni partita) fanno ombra. E piano: se il topo corre,
    /// le zampette fanno rumore e la torcia scatta verso di lui. (Nel codice "cat" e "fish" sono il topo e il
    /// formaggio, "dog" è il gatto di guardia.)
    /// </summary>
    public class SneakyCatLevel : LevelController
    {
        [SerializeField] Transform cat;
        [SerializeField] Transform dog;
        [SerializeField] Transform cone;              // pivot sulla torcia, a 0° punta in basso
        [SerializeField] Transform fish;
        [SerializeField] Collider2D[] furniture;
        [SerializeField] SpriteRenderer noiseIcon;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float halfAngle = 20f, length = 7f;
        [SerializeField] float sweepRange = 65f, sweepSpeed = 0.8f;
        [Tooltip("Gradi al secondo della torcia: in perlustrazione e quando sente il topo.")]
        [SerializeField] float sweepTurn = 110f, chaseTurn = 200f;
        [Tooltip("Posti possibili dei mobili (coordinate del livello): ne vengono usati tanti quanti i mobili, a caso.")]
        [SerializeField] Vector2[] furnitureSlots =
        {
            new Vector2(-1.2f, 0.9f), new Vector2(0.9f, 0.95f), new Vector2(-0.1f, -0.15f), new Vector2(1.6f, -0.5f),
            new Vector2(-1.65f, -0.9f), new Vector2(0.45f, -1.6f), new Vector2(-0.8f, -2.4f), new Vector2(1.7f, -2.25f),
            new Vector2(-2.0f, 0.1f),
        };
        [SerializeField] float maxQuiet = 2.6f, catSpeed = 6f;

        float look, sweepClock, seen, noise;
        bool holding, done;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            sweepClock = Random.Range(0f, 6f);
            PlaceFurniture();
            if (bubble != null) { bubble.Show("Psst... the cheese. Quietly."); StartCoroutine(HideLater(2f)); }
        }

        /// <summary>Mobili in posti a caso; il divano (largo) solo dove c'è spazio ai lati.</summary>
        void PlaceFurniture()
        {
            var free = new System.Collections.Generic.List<Vector2>(furnitureSlots);
            for (int i = free.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (free[i], free[j]) = (free[j], free[i]); }
            foreach (Collider2D f in furniture)
            {
                if (f == null || free.Count == 0) continue;
                bool wide = f is BoxCollider2D b && b.size.x > 1.5f;
                int k = wide ? free.FindIndex(v => Mathf.Abs(v.x) <= 1.2f) : 0;
                if (k < 0) k = 0;
                f.transform.localPosition = new Vector3(free[k].x, free[k].y, f.transform.localPosition.z);
                free.RemoveAt(k);
            }
            Physics2D.SyncTransforms();
        }

        bool Lit(Vector2 p)
        {
            Vector2 o = cone.position, d = p - o;
            if (d.magnitude > length) return false;
            float a = Vector2.SignedAngle(Vector2.down, d);
            if (Mathf.Abs(Mathf.DeltaAngle(a, look)) > halfAngle) return false;
            foreach (RaycastHit2D h in Physics2D.LinecastAll(o, p))
                foreach (Collider2D f in furniture) if (h.collider == f) return false;
            return true;
        }

        void Update()
        {
            if (done || IsFinished) return;
            float dt = Time.deltaTime;
            // il gatto di guardia: spazza con la torcia, ma se sente rumore la punta verso il topo
            Vector2 toCat = cat.position - cone.position;
            float catAngle = Vector2.SignedAngle(Vector2.down, toCat);
            sweepClock += dt;
            float want = noise > 1f ? catAngle : Mathf.Sin(sweepClock * sweepSpeed) * sweepRange;
            look = Mathf.MoveTowardsAngle(look, want, (noise > 1f ? chaseTurn : sweepTurn) * dt);
            cone.localRotation = Quaternion.Euler(0, 0, look);

            float moved = 0f;
            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _))
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                if (began) holding = Vector2.Distance(w, cat.position) < 0.8f;
                if (holding)
                {
                    Vector2 p = Vector2.MoveTowards(cat.position, w, catSpeed * dt);
                    moved = Vector2.Distance(p, cat.position);
                    if (Mathf.Abs(p.x - cat.position.x) > 0.001f) cat.localScale = new Vector3(Mathf.Abs(cat.localScale.x) * (p.x > cat.position.x ? -1f : 1f), cat.localScale.y, 1f);
                    cat.position = new Vector3(p.x, p.y, cat.position.z);
                }
            }
            else holding = false;
            float speed = moved / Mathf.Max(dt, 1e-4f);
            noise = Mathf.Max(0f, noise + (speed > maxQuiet ? dt * 5f : -dt * 1.5f));   // sente subito il topo che corre
            if (noiseIcon != null) noiseIcon.color = new Color(1f, 1f, 1f, Mathf.Clamp01(noise));

            seen = Lit(cat.position) ? seen + dt : 0f;
            if (seen > 0.2f) { StartCoroutine(Woof()); return; }
            if (Vector2.Distance(cat.position, fish.position) < 0.5f) StartCoroutine(Yum());
        }

        IEnumerator Woof()
        {
            done = true;
            Sfx.Play(this, "bonk", 1f);
            Haptics.Medium();
            if (bubble != null) bubble.Show("MEOW! I SEE YOU, LITTLE MOUSE!");
            Vector3 d = dog.position;
            for (float k = 0f; k < 0.8f; k += Time.deltaTime) { dog.position = d + Vector3.up * Mathf.Abs(Mathf.Sin(k * 20f)) * 0.15f; yield return null; }
            yield return new WaitForSeconds(0.6f);
            Fail();
        }

        IEnumerator Yum()
        {
            done = true;
            fish.gameObject.SetActive(false);
            if (bubble != null) bubble.Show("Yoink. Ninja mouse.");
            yield return new WaitForSeconds(1.5f);
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
