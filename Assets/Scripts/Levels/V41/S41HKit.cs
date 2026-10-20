using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>Piccoli aiuti comuni ai livelli del pacchetto 41H (diti multipli, toni, rimbalzi, coriandoli).</summary>
    public static class S41HKit
    {
        /// <summary>Un dito sullo schermo (o il mouse nell'Editor), in coordinate mondo.</summary>
        public struct Finger
        {
            public int id;
            public Vector2 world;
            public bool began, ended;
        }

        static readonly List<Finger> fingers = new List<Finger>();
        static readonly Dictionary<int, AudioClip> tones = new Dictionary<int, AudioClip>();

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
            if (Input.touchCount == 0 && (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0) || Input.GetMouseButtonDown(0)))
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

        /// <summary>Nota sintetica (frequenza Hz) dalla sorgente audio dell'oggetto.</summary>
        public static void Note(Component owner, float hz, float volume = 0.5f, float seconds = 0.3f)
        {
            if (owner == null) return;
            AudioSource src = owner.GetComponent<AudioSource>();
            if (src == null) { src = owner.gameObject.AddComponent<AudioSource>(); src.playOnAwake = false; }
            src.PlayOneShot(Tone(hz, seconds), volume);
        }

        static AudioClip Tone(float hz, float seconds)
        {
            int key = Mathf.RoundToInt(hz) * 1000 + Mathf.RoundToInt(seconds * 100f);
            if (tones.TryGetValue(key, out AudioClip cached) && cached != null) return cached;
            const int rate = 22050;
            int n = Mathf.Max(1, (int)(seconds * rate));
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Min(1f, t * 150f) * Mathf.Min(1f, (seconds - t) * 30f) * Mathf.Exp(-t * 4f);
                data[i] = (Mathf.Sin(t * 2f * Mathf.PI * hz) * 0.7f + Mathf.Sin(t * 4f * Mathf.PI * hz) * 0.18f) * env * 0.6f;
            }
            AudioClip c = AudioClip.Create("s41h_tone", n, 1, rate, false);
            c.SetData(data, 0);
            tones[key] = c;
            return c;
        }
    }
}
