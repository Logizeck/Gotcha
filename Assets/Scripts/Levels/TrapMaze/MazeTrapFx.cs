using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Trappole dei labirinti (buche e laser) condivise tra il labirinto con trappole e quello al buio:
    /// controllo della sfera e animazioni di caduta nella buca e di scossa del laser.
    /// </summary>
    public static class MazeTrapFx
    {
        public enum Hit { None, Pit, Laser }

        /// <summary>Che trappola tocca la sfera in p (centro della buca in pitAt).</summary>
        public static Hit Check(Vector2 p, Transform[] pits, float fallRadius, LaserBeam[] lasers, float ballRadius, out Vector2 pitAt)
        {
            pitAt = Vector2.zero;
            if (pits != null)
                foreach (Transform pit in pits)
                {
                    if (pit == null) continue;
                    if (Vector2.Distance(p, pit.position) < fallRadius) { pitAt = pit.position; return Hit.Pit; }
                }
            if (lasers != null)
                foreach (LaserBeam l in lasers)
                    if (l != null && l.IsOn && l.DistanceTo(p) < ballRadius + 0.04f) return Hit.Laser;
            return Hit.None;
        }

        /// <summary>La sfera gira nel bordo della buca e sparisce nel buio.</summary>
        public static IEnumerator FallIn(Component owner, Rigidbody2D ball, Vector2 center)
        {
            ball.simulated = false;
            Transform t = ball.transform;
            Vector3 from = t.position, scale = t.localScale;
            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            TrailRenderer trail = t.GetComponent<TrailRenderer>();
            if (trail != null) trail.emitting = false;
            Sfx.Play(owner, "whoosh", 0.5f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
            {
                float a = k * 9f, r = Vector2.Distance(from, center) * (1f - k);
                t.position = new Vector3(center.x + Mathf.Cos(a) * r, center.y + Mathf.Sin(a) * r, from.z);
                t.localScale = scale * Mathf.Lerp(1f, 0.1f, k * k);
                if (sr != null) sr.color = Color.Lerp(Color.white, new Color(0.2f, 0.2f, 0.25f), k);
                yield return null;
            }
            t.gameObject.SetActive(false);
            Sfx.Play(owner, "bonk", 0.4f);   // tonfo in fondo
            Haptics.Medium();
        }

        /// <summary>Scossa del laser: la sfera trema e lampeggia di rosso tra le scintille.</summary>
        public static IEnumerator Zap(Component owner, Rigidbody2D ball, SmokeEmitter sparks)
        {
            Haptics.Medium();
            Sfx.Play(owner, "buzz", 0.6f);
            ball.simulated = false;
            Transform t = ball.transform;
            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            if (sparks != null) { sparks.transform.position = t.position; sparks.Play(); }
            Vector3 start = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                t.position = start + (Vector3)(Random.insideUnitCircle * 0.05f);
                if (sr != null) sr.color = Mathf.Repeat(k * 10f, 1f) < 0.5f ? new Color(1f, 0.35f, 0.35f) : Color.white;
                yield return null;
            }
            t.position = start;
            if (sr != null) sr.color = new Color(0.55f, 0.5f, 0.5f);   // annerita
            if (sparks != null) sparks.Stop();
        }
    }
}
