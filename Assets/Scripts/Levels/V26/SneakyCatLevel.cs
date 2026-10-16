using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 97 — il topo furtivo. Il cane di guardia gira la torcia di qua e di là. Trascina il topolino fino al
    /// formaggio restando al buio: i mobili fanno ombra (dietro al divano la luce non arriva). E piano: se il topo corre,
    /// le zampette fanno rumore e il cane si gira verso di lui. (Nel codice "cat" e "fish" sono il topo e il formaggio.)
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
        [SerializeField] float sweepRange = 65f, sweepSpeed = 0.55f;
        [SerializeField] float maxQuiet = 2.6f, catSpeed = 6f;

        float look, sweepClock, seen, noise;
        bool holding, done;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            sweepClock = Random.Range(0f, 6f);
            if (bubble != null) { bubble.Show("Psst... the cheese. Quietly."); StartCoroutine(HideLater(2f)); }
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
            // il cane: spazza, ma se sente rumore punta verso il gatto
            Vector2 toCat = cat.position - cone.position;
            float catAngle = Vector2.SignedAngle(Vector2.down, toCat);
            sweepClock += dt;
            float want = noise > 1f ? catAngle : Mathf.Sin(sweepClock * sweepSpeed) * sweepRange;
            look = Mathf.MoveTowardsAngle(look, want, (noise > 1f ? 90f : 60f) * dt);
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
            noise = Mathf.Max(0f, noise + (speed > maxQuiet ? dt * 3f : -dt * 1.5f));
            if (noiseIcon != null) noiseIcon.color = new Color(1f, 1f, 1f, Mathf.Clamp01(noise));

            seen = Lit(cat.position) ? seen + dt : 0f;
            if (seen > 0.2f) { StartCoroutine(Woof()); return; }
            if (Vector2.Distance(cat.position, fish.position) < 0.5f) StartCoroutine(Yum());
        }

        IEnumerator Woof()
        {
            done = true;
            Sfx.Play(this, "bonk", 1f);
            if (bubble != null) bubble.Show("WOOF! WOOF! I SEE YOU!");
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
