using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 92 — pesca con la calamita. La chiave è caduta nello stagno. La calamita (trascinala col dito) attira
    /// tutto ciò che è di ferro: la chiave, ma anche barattoli e ferri di cavallo. La calamita regge UNA sola cosa:
    /// se hai preso la cosa sbagliata, scuoti il telefono per farla cadere. E occhio al piranha: se morde il filo...
    /// </summary>
    public class MagnetLevel : LevelController
    {
        [SerializeField] Transform anchor;            // punta della canna
        [SerializeField] Transform magnet;
        [SerializeField] LineRenderer wire;
        [SerializeField] Transform key;
        [SerializeField] Transform[] junk;
        [SerializeField] Transform piranha;
        [SerializeField] float waterY = 1.2f, bottomY = -3.5f;
        [SerializeField] float pull = 1.1f, stick = 0.3f;
        [SerializeField] float fishSpeed = 1.3f;
        [SerializeField] SpeechBubble bubble;

        readonly List<Transform> items = new List<Transform>();
        readonly Dictionary<Transform, Vector3> rest = new Dictionary<Transform, Vector3>();
        Transform held;
        float fishDir = 1f, fishY, fishTurn, dropCooldown;
        bool done;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            items.Add(key);
            items.AddRange(junk);
            foreach (Transform t in items) rest[t] = t.position;
            fishY = Random.Range(bottomY + 1f, waterY - 1f);
            if (bubble != null) { bubble.Show("My key! Fish it out. Mind the piranha!"); StartCoroutine(HideLater(2.2f)); }
        }

        void Update()
        {
            if (done || IsFinished) return;
            float dt = Time.deltaTime;
            if (PointerInput.TryGetPrimary(out Vector2 sp, out _, out _) && !PointerInput.IsOverUi())
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                w.y = Mathf.Clamp(w.y, bottomY, anchor.position.y - 0.4f);
                w.x = Mathf.Clamp(w.x, -2.5f, 2.5f);
                magnet.position = Vector3.Lerp(magnet.position, new Vector3(w.x, w.y, magnet.position.z), 1f - Mathf.Exp(-8f * dt));
            }
            wire.SetPosition(0, anchor.position);
            wire.SetPosition(1, magnet.position + Vector3.up * 0.2f);

            dropCooldown -= dt;
            if (held != null && Inputs.Shake() > 9f && dropCooldown <= 0f)
            {
                Sfx.Play(this, "bonk");
                rest[held] = new Vector3(held.position.x, rest[held].y, held.position.z);
                held = null;
                dropCooldown = 1f;
            }

            Vector2 m = magnet.position;
            foreach (Transform t in items)
            {
                if (t == held) { t.position = magnet.position + Vector3.down * 0.3f; continue; }
                Vector3 r = rest[t];
                float d = Vector2.Distance(t.position, m);
                if (held == null && dropCooldown <= 0f && d < pull && m.y < waterY)
                {
                    t.position = Vector3.MoveTowards(t.position, magnet.position, (pull - d + 0.4f) * 2.5f * dt);
                    if (d < stick) { held = t; Sfx.Play(this, "clunk", 0.6f); }
                }
                else t.position = Vector3.MoveTowards(t.position, r, 1.2f * dt);   // ricade sul fondo
            }

            // piranha: va avanti e indietro, ogni tanto cambia profondità; punta al filo se è vicino
            fishTurn -= dt;
            if (fishTurn <= 0f) { fishTurn = Random.Range(1.5f, 3f); fishY = Random.Range(bottomY + 0.6f, waterY - 0.6f); }
            Vector3 f = piranha.position;
            f.x += fishDir * fishSpeed * dt;
            if (Mathf.Abs(f.x) > 2.6f) { fishDir = -Mathf.Sign(f.x); f.x = Mathf.Sign(f.x) * 2.6f; }
            f.y = Mathf.MoveTowards(f.y, fishY, 0.8f * dt);
            piranha.position = f;
            piranha.localScale = new Vector3(Mathf.Abs(piranha.localScale.x) * fishDir, piranha.localScale.y, 1f);

            // morso: la bocca tocca il filo sott'acqua
            Vector2 a = anchor.position, b = magnet.position, mouth = (Vector2)f + new Vector2(fishDir * 0.35f, 0f);
            if (mouth.y < waterY)
            {
                Vector2 ab = b - a;
                float t2 = Mathf.Clamp01(Vector2.Dot(mouth - a, ab) / Mathf.Max(1e-4f, ab.sqrMagnitude));
                if (Vector2.Distance(mouth, a + ab * t2) < 0.18f) { StartCoroutine(Snap()); return; }
            }

            if (held == key && magnet.position.y > waterY + 0.3f) StartCoroutine(GotIt());
        }

        IEnumerator Snap()
        {
            Haptics.Medium();
            done = true;
            Sfx.Play(this, "bonk", 1f);
            if (bubble != null) bubble.Show("CHOMP! The line snapped!");
            Vector3 from = magnet.position;
            wire.SetPosition(1, anchor.position + Vector3.down * 0.6f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                magnet.position = Vector3.Lerp(from, new Vector3(from.x, bottomY, from.z), k);
                if (held != null) held.position = magnet.position + Vector3.down * 0.3f;
                yield return null;
            }
            yield return new WaitForSeconds(0.6f);
            Fail();
        }

        IEnumerator GotIt()
        {
            done = true;
            if (bubble != null) bubble.Show("My key! Slightly fishy, but mine!");
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
