using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>Piccoli aiuti condivisi dai livelli del pacchetto 41D (coriandoli, rimbalzi, fumetti a tempo).</summary>
    public static class S41DKit
    {
        public static void Confetti(SmokeEmitter[] confetti, bool on)
        {
            if (confetti == null) return;
            foreach (SmokeEmitter c in confetti)
                if (c != null) { if (on) c.Play(); else c.Stop(); }
        }

        /// <summary>Rimbalzo di scala (da 1 a peak e ritorno) attorno alla scala attuale.</summary>
        public static IEnumerator Pop(Transform t, float peak, float seconds)
        {
            if (t == null) yield break;
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.localScale = s * (1f + (peak - 1f) * Mathf.Sin(k * Mathf.PI));
                yield return null;
            }
            t.localScale = s;
        }

        /// <summary>Scossa orizzontale breve (es. errore).</summary>
        public static IEnumerator Shake(Transform t, float amount, float seconds)
        {
            if (t == null) yield break;
            Vector3 p = t.localPosition;
            for (float k = 0f; k < seconds; k += Time.deltaTime)
            {
                t.localPosition = p + new Vector3(Mathf.Sin(k * 60f) * amount * (1f - k / seconds), 0f, 0f);
                yield return null;
            }
            t.localPosition = p;
        }

        /// <summary>Movimento morbido (ease in-out) verso una posizione locale.</summary>
        public static IEnumerator Move(Transform t, Vector3 to, float seconds, float hop = 0f)
        {
            if (t == null) yield break;
            Vector3 from = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                float e = k * k * (3f - 2f * k);
                t.localPosition = Vector3.Lerp(from, to, e) + Vector3.up * Mathf.Sin(k * Mathf.PI) * hop;
                yield return null;
            }
            t.localPosition = to;
        }

        public static Vector2 World(Camera cam, Vector2 screen) => Core.PointerInput.ToWorld(cam, screen);

        /// <summary>Metà larghezza visibile della camera (unità mondo).</summary>
        public static float HalfWidth(Camera cam) => cam != null ? cam.orthographicSize * cam.aspect : 2.9f;
    }
}
