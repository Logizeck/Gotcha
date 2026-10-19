using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Magnet mayhem" — una chiave di ferro deve attraversare un labirinto fino al lucchetto, ma non si può
    /// toccare. Sposta le tre calamite col dito: le rosse (+) attirano la chiave, la blu (−) la respinge; le forze sono
    /// dolci e limitate. Le barre elettriche gialle scottano: due scosse e la chiave è fritta.
    /// </summary>
    public class MagnetMayhemLevel : LevelController
    {
        [SerializeField] Transform key, keyStart;
        [SerializeField] Transform[] magnets;          // con Collider2D; le prime "redCount" attirano
        [SerializeField] SpriteRenderer[] fields;      // alone sotto ogni calamita
        [SerializeField] int redCount = 2;
        [SerializeField] Collider2D[] walls, traps;
        [SerializeField] SpriteRenderer[] trapArts;
        [SerializeField] Transform goal;
        [SerializeField] SpriteRenderer lockArt;
        [SerializeField] Sprite lockOpen;
        [SerializeField] TextMesh livesText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int zapsAllowed = 2;
        [SerializeField] float keyRadius = 0.24f, range = 1.7f, strength = 2.6f, damping = 3.5f, maxSpeed = 3.2f, goalRadius = 0.45f;

        Camera cam;
        Vector3[] magnetHomes;
        Vector2 vel, grabOffset;
        int grabbed = -1, zaps;
        bool busy;
        float clock, sayCooldown;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            magnetHomes = new Vector3[magnets.Length];
            for (int i = 0; i < magnets.Length; i++) magnetHomes[i] = magnets[i].position;
            key.position = keyStart.position;
            S41DKit.Confetti(confetti, false);
            UpdateLives();
            if (bubble != null) bubble.Show("Bring the key to the lock... without touching it!");
            sayCooldown = 3f;
        }

        void UpdateLives() { if (livesText != null) livesText.text = "SHOCKS " + zaps + "/" + zapsAllowed; }

        void Say(string t) { if (bubble != null && sayCooldown <= 0f) { bubble.Show(t); sayCooldown = 2.5f; } }

        void Update()
        {
            if (IsFinished) return;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            clock += dt;
            sayCooldown -= dt;
            if (sayCooldown < -1.5f && bubble != null && bubble.gameObject.activeSelf && !busy) bubble.Hide();
            for (int i = 0; i < trapArts.Length; i++) if (trapArts[i] != null) trapArts[i].color = new Color(1f, 1f, 1f, 0.75f + 0.25f * Mathf.Sin(clock * 25f + i * 2f));
            if (busy) return;
            HandleInput();
            for (int i = 0; i < fields.Length; i++)
                if (fields[i] != null) fields[i].color = new Color(i < redCount ? 1f : 0.35f, i < redCount ? 0.35f : 0.55f, i < redCount ? 0.35f : 1f, (grabbed == i ? 0.45f : 0.22f) + 0.05f * Mathf.Sin(clock * 6f));
            const int steps = 4;
            for (int s = 0; s < steps; s++) Step(dt / steps);
            Vector2 k = key.position;
            foreach (Collider2D t in traps)
            {
                if (t == null) continue;
                Bounds b = t.bounds;
                Vector2 c = new Vector2(Mathf.Clamp(k.x, b.min.x, b.max.x), Mathf.Clamp(k.y, b.min.y, b.max.y));
                if ((k - c).magnitude < keyRadius * 0.7f) { StartCoroutine(Zap()); return; }
            }
            if (Vector2.Distance(k, goal.position) < goalRadius) StartCoroutine(Unlock());
        }

        void HandleInput()
        {
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { grabbed = -1; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                grabbed = -1;
                float best = 0.6f;
                for (int i = 0; i < magnets.Length; i++)
                {
                    float d = Vector2.Distance(w, magnets[i].position);
                    if (d < best) { best = d; grabbed = i; }
                }
                if (grabbed >= 0) { grabOffset = (Vector2)magnets[grabbed].position - w; Sfx.Play(this, "tick", 0.5f); }
                else if (Vector2.Distance(w, key.position) < 0.5f) Say("Don't touch it! Use the magnets.");
            }
            if (grabbed >= 0)
            {
                Vector2 p = w + grabOffset;
                p.x = Mathf.Clamp(p.x, -2.55f, 2.55f);
                p.y = Mathf.Clamp(p.y, -4.3f, 2.8f);
                magnets[grabbed].position = new Vector3(p.x, p.y, magnets[grabbed].position.z);
            }
            if (ended) grabbed = -1;
        }

        void Step(float dt)
        {
            Vector2 k = key.position;
            Vector2 acc = Vector2.zero;
            for (int i = 0; i < magnets.Length; i++)
            {
                Vector2 d = (Vector2)magnets[i].position - k;
                float r = d.magnitude;
                if (r > range || r < 0.02f) continue;
                float sign = i < redCount ? 1f : -1f;
                float f = strength / Mathf.Max(r * r, 0.12f) * (1f - r / range);
                acc += d / r * Mathf.Min(f, 14f) * sign;
                if (sign > 0f && r < 0.25f) vel *= 1f - Mathf.Min(1f, dt * 10f);   // si "attacca" alla calamita
            }
            vel += acc * dt;
            vel -= vel * Mathf.Min(1f, damping * dt);
            vel = Vector2.ClampMagnitude(vel, maxSpeed);
            k += vel * dt;
            // urti coi muri
            foreach (Collider2D w in walls)
            {
                if (w == null) continue;
                Bounds b = w.bounds;
                Vector2 c = new Vector2(Mathf.Clamp(k.x, b.min.x, b.max.x), Mathf.Clamp(k.y, b.min.y, b.max.y));
                Vector2 diff = k - c;
                float dist = diff.magnitude;
                if (dist >= keyRadius) continue;
                Vector2 n = dist > 1e-4f ? diff / dist : Vector2.up;
                k = c + n * keyRadius;
                float vn = Vector2.Dot(vel, n);
                if (vn < 0f) vel -= n * vn * 1.3f;
            }
            key.position = new Vector3(k.x, k.y, key.position.z);
            key.localRotation = Quaternion.Euler(0, 0, Mathf.Clamp(vel.x * -12f, -30f, 30f));
        }

        IEnumerator Zap()
        {
            busy = true;
            grabbed = -1;
            zaps++;
            UpdateLives();
            Haptics.Heavy();
            Sfx.Play(this, "buzz", 1f);
            SpriteRenderer sr = key.GetComponentInChildren<SpriteRenderer>();
            Vector3 p = key.position;
            for (float k = 0f; k < 0.7f; k += Time.deltaTime)
            {
                key.position = p + (Vector3)(Random.insideUnitCircle * 0.08f);
                if (sr != null) sr.color = Mathf.PingPong(k * 12f, 1f) > 0.5f ? new Color(1f, 1f, 0.3f) : Color.black;
                yield return null;
            }
            if (sr != null) sr.color = Color.white;
            if (zaps >= zapsAllowed)
            {
                if (sr != null) sr.color = new Color(0.25f, 0.2f, 0.2f);
                if (bubble != null) bubble.Show("ZZZAP! The key is fried.");
                yield return new WaitForSeconds(1.4f);
                Fail();
                yield break;
            }
            if (bubble != null) bubble.Show("ZAP! Careful near the yellow bars!");
            sayCooldown = 2f;
            key.position = keyStart.position;
            vel = Vector2.zero;
            for (int i = 0; i < magnets.Length; i++) magnets[i].position = magnetHomes[i];
            busy = false;
        }

        IEnumerator Unlock()
        {
            busy = true;
            Vector3 from = key.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { key.position = Vector3.Lerp(from, goal.position, k); yield return null; }
            key.gameObject.SetActive(false);
            if (lockArt != null) lockArt.sprite = lockOpen;
            Sfx.Play(this, "clunk", 0.8f);
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Light();
            yield return StartCoroutine(S41DKit.Pop(goal, 1.3f, 0.3f));
            if (bubble != null) bubble.Show("CLICK! Magnetic personality!");
            S41DKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.8f);
            S41DKit.Confetti(confetti, false);
            Win();
        }
    }
}
