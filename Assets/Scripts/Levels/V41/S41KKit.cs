using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>Aiuti condivisi dai livelli del pacchetto 41K (dita multiple, coriandoli, animazioni, note).</summary>
    public static class S41KKit
    {
        /// <summary>Un dito (o il mouse, id -1) in coordinate mondo.</summary>
        public struct Finger
        {
            public int id;
            public Vector2 world;
            public bool began, ended;
        }

        static readonly List<Finger> buffer = new List<Finger>();
        static readonly Dictionary<int, AudioClip> tones = new Dictionary<int, AudioClip>();

        /// <summary>Tutte le dita attive in questo frame (il mouse conta come un dito quando non ci sono tocchi).</summary>
        public static List<Finger> Fingers(Camera cam)
        {
            buffer.Clear();
            if (cam == null) return buffer;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                buffer.Add(new Finger
                {
                    id = t.fingerId,
                    world = PointerInput.ToWorld(cam, t.position),
                    began = t.phase == TouchPhase.Began,
                    ended = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled,
                });
            }
            if (Input.touchCount == 0 && (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)))
                buffer.Add(new Finger { id = -1, world = PointerInput.ToWorld(cam, Input.mousePosition), began = Input.GetMouseButtonDown(0), ended = Input.GetMouseButtonUp(0) });
            return buffer;
        }

        public static void Confetti(SmokeEmitter[] confetti, bool on)
        {
            if (confetti == null) return;
            foreach (SmokeEmitter c in confetti)
                if (c != null) { if (on) c.Play(); else c.Stop(); }
        }

        /// <summary>Rimbalzo di scala (da 1 a peak e ritorno).</summary>
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

        /// <summary>Scossa orizzontale che si smorza.</summary>
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

        /// <summary>Movimento morbido verso una posizione locale, con saltello opzionale.</summary>
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

        /// <summary>Scala morbida verso un valore.</summary>
        public static IEnumerator ScaleTo(Transform t, Vector3 to, float seconds)
        {
            if (t == null) yield break;
            Vector3 from = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.localScale = Vector3.Lerp(from, to, k * k * (3f - 2f * k));
                yield return null;
            }
            t.localScale = to;
        }

        public static void Alpha(SpriteRenderer s, float a) { if (s != null) { Color c = s.color; c.a = a; s.color = c; } }

        /// <summary>Suona una nota sintetizzata (tipo carillon) di frequenza hz.</summary>
        public static void Tone(Component owner, float hz, float seconds, float volume)
        {
            if (owner == null) return;
            AudioSource src = owner.GetComponent<AudioSource>();
            if (src == null) { src = owner.gameObject.AddComponent<AudioSource>(); src.playOnAwake = false; }
            src.PlayOneShot(MakeTone(hz, seconds), volume);
        }

        static AudioClip MakeTone(float hz, float seconds)
        {
            int key = Mathf.RoundToInt(hz) * 1000 + Mathf.RoundToInt(seconds * 100f);
            if (tones.TryGetValue(key, out AudioClip cached) && cached != null) return cached;
            const int rate = 22050;
            int n = Mathf.Max(1, (int)(seconds * rate));
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Min(1f, t * 200f) * Mathf.Exp(-t * 4.5f);
                data[i] = (Mathf.Sin(t * 2f * Mathf.PI * hz) * 0.7f + Mathf.Sin(t * 4f * Mathf.PI * hz) * 0.2f) * env * 0.8f;
            }
            AudioClip c = AudioClip.Create("s41k_tone", n, 1, rate, false);
            c.SetData(data, 0);
            tones[key] = c;
            return c;
        }
    }
}
