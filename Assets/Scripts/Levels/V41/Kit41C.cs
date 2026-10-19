using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>Piccoli aiuti comuni ai livelli del pacchetto 41C (puntatore, tocchi multipli, cuori, rimbalzi).</summary>
    public static class Kit41C
    {
        /// <summary>Un dito sullo schermo (o il mouse nell'Editor), in coordinate mondo.</summary>
        public struct Finger
        {
            public int id;
            public Vector2 world;
            public bool began, ended;
        }

        static readonly List<Finger> fingers = new List<Finger>();

        /// <summary>Tutti i diti attivi in questo frame (il mouse conta come dito -1 quando non ci sono tocchi).</summary>
        public static List<Finger> Fingers(Camera cam)
        {
            fingers.Clear();
            if (cam == null) return fingers;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                fingers.Add(new Finger
                {
                    id = t.fingerId,
                    world = PointerInput.ToWorld(cam, t.position),
                    began = t.phase == TouchPhase.Began,
                    ended = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled,
                });
            }
            if (Input.touchCount == 0 && (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)))
                fingers.Add(new Finger { id = -1, world = PointerInput.ToWorld(cam, Input.mousePosition), began = Input.GetMouseButtonDown(0), ended = Input.GetMouseButtonUp(0) });
            return fingers;
        }

        /// <summary>Puntatore primario in coordinate mondo (ignora i tocchi che iniziano sopra l'interfaccia).</summary>
        public static bool Pointer(Camera cam, out Vector2 world, out bool began, out bool ended)
        {
            world = default;
            if (cam == null || !PointerInput.TryGetPrimary(out Vector2 sp, out began, out ended)) { began = ended = false; return false; }
            if (began && PointerInput.IsOverUi()) { began = false; return false; }
            world = PointerInput.ToWorld(cam, sp);
            return true;
        }

        public static void Hearts(SpriteRenderer[] hearts, int left) => E40Kit.Hearts(hearts, left);
        public static void Goal(TextMesh goal, string text) => E40Kit.Goal(goal, text);

        /// <summary>Rimbalzo di scala.</summary>
        public static IEnumerator Pop(Transform t, float amount = 0.18f, float seconds = 0.25f) => E40Kit.Pop(t, amount, seconds);

        /// <summary>Scossone orizzontale (errore).</summary>
        public static IEnumerator Shake(Transform t, float amount = 0.12f, float seconds = 0.35f) => E40Kit.Shake(t, amount, seconds);

        /// <summary>Sposta un oggetto (coordinate locali) con un'andatura morbida.</summary>
        public static IEnumerator Move(Transform t, Vector3 to, float seconds, float hop = 0f)
        {
            if (t == null) yield break;
            Vector3 from = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, k)) + Vector3.up * Mathf.Sin(k * Mathf.PI) * hop;
                yield return null;
            }
            t.localPosition = to;
        }

        /// <summary>Come Move, ma in coordinate mondo.</summary>
        public static IEnumerator MoveWorld(Transform t, Vector3 to, float seconds, float hop = 0f)
        {
            if (t == null) yield break;
            Vector3 from = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, k)) + Vector3.up * Mathf.Sin(k * Mathf.PI) * hop;
                yield return null;
            }
            t.position = to;
        }

        /// <summary>Piume/coriandoli: gli oggetti partono dal punto e si allargano svanendo.</summary>
        public static IEnumerator Burst(SpriteRenderer[] bits, Vector3 at, float radius, float seconds = 0.8f)
        {
            if (bits == null) yield break;
            var dirs = new Vector3[bits.Length];
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i] == null) continue;
                float a = (i + 0.3f) / bits.Length * Mathf.PI * 2f + Random.Range(-0.3f, 0.3f);
                dirs[i] = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * radius * Random.Range(0.6f, 1.1f);
                bits[i].gameObject.SetActive(true);
                bits[i].transform.position = at;
            }
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                for (int i = 0; i < bits.Length; i++)
                {
                    if (bits[i] == null) continue;
                    bits[i].transform.position = at + dirs[i] * Mathf.Sqrt(k) + Vector3.down * k * k * 0.4f;
                    bits[i].transform.localRotation = Quaternion.Euler(0f, 0f, k * 360f * (i % 2 == 0 ? 1f : -1f));
                    Color c = bits[i].color;
                    c.a = 1f - k * k;
                    bits[i].color = c;
                }
                yield return null;
            }
            foreach (SpriteRenderer b in bits) if (b != null) b.gameObject.SetActive(false);
        }
    }
}
