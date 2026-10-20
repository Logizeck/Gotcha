using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>Piccoli aiuti comuni ai livelli del pacchetto 41L (puntatore, animazioni, coriandoli).</summary>
    public static class S41LKit
    {
        /// <summary>Puntatore primario in coordinate mondo (ignora i tocchi che iniziano sopra l'interfaccia).</summary>
        public static bool Pointer(Camera cam, out Vector2 world, out bool began, out bool ended)
        {
            world = default;
            if (cam == null || !PointerInput.TryGetPrimary(out Vector2 sp, out began, out ended)) { began = ended = false; return false; }
            if (began && PointerInput.IsOverUi()) { began = false; return false; }
            world = PointerInput.ToWorld(cam, sp);
            return true;
        }

        /// <summary>Accelerometro presente? (nell'Editor sì: le frecce fanno da inclinazione)</summary>
        public static bool HasTilt()
        {
#if UNITY_EDITOR
            return true;
#else
            return SystemInfo.supportsAccelerometer;
#endif
        }

        public static void Confetti(SmokeEmitter[] confetti, bool on)
        {
            if (confetti == null) return;
            foreach (SmokeEmitter c in confetti)
                if (c != null) { if (on) c.Play(); else c.Stop(); }
        }

        public static void Say(SpeechBubble b, string text) { if (b != null) b.Show(text); }

        public static void Alpha(SpriteRenderer sr, float a)
        {
            if (sr == null) return;
            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }

        /// <summary>Rimbalzo di scala.</summary>
        public static IEnumerator Pop(Transform t, float amount = 0.18f, float seconds = 0.25f)
        {
            if (t == null) yield break;
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.localScale = s * (1f + Mathf.Sin(k * Mathf.PI) * amount);
                yield return null;
            }
            t.localScale = s;
        }

        /// <summary>Scossone orizzontale (errore).</summary>
        public static IEnumerator Shake(Transform t, float amount = 0.12f, float seconds = 0.35f)
        {
            if (t == null) yield break;
            Vector3 p = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.localPosition = p + Vector3.right * Mathf.Sin(k * 40f) * amount * (1f - k);
                yield return null;
            }
            t.localPosition = p;
        }

        /// <summary>Sposta un oggetto (coordinate locali) con un'andatura morbida e un eventuale saltello.</summary>
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

        /// <summary>Porta l'opacità di uno sprite a "to".</summary>
        public static IEnumerator Fade(SpriteRenderer sr, float to, float seconds)
        {
            if (sr == null) yield break;
            float from = sr.color.a;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                Alpha(sr, Mathf.Lerp(from, to, k));
                yield return null;
            }
            Alpha(sr, to);
        }

        /// <summary>Scala verso un valore in un tempo dato.</summary>
        public static IEnumerator ScaleTo(Transform t, Vector3 to, float seconds)
        {
            if (t == null) yield break;
            Vector3 from = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.localScale = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, k));
                yield return null;
            }
            t.localScale = to;
        }
    }
}
