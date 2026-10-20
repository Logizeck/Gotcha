using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>Base comune dei livelli del pacchetto 41J: fumetto a tempo, coriandoli, dita multiple e piccole animazioni.</summary>
    public abstract class S41JLevelBase : LevelController
    {
        [SerializeField] protected SpeechBubble bubble;
        [SerializeField] protected SmokeEmitter[] confetti;

        protected Camera cam;
        protected bool busy;
        protected float clock;
        float sayUntil;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            Confetti(false);
            Begin();
        }

        /// <summary>Inizio specifico del livello.</summary>
        protected abstract void Begin();

        /// <summary>Logica per frame (chiamata solo a livello in corso).</summary>
        protected abstract void Tick();

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (bubble != null && sayUntil > 0f && clock > sayUntil) { bubble.Hide(); sayUntil = 0f; }
            Tick();
        }

        protected void Say(string text, float seconds = 2.2f)
        {
            if (bubble == null) return;
            bubble.Show(text);
            sayUntil = clock + seconds;
        }

        protected void Confetti(bool on)
        {
            if (confetti == null) return;
            foreach (SmokeEmitter c in confetti) if (c != null) { if (on) c.Play(); else c.Stop(); }
        }

        /// <summary>Festa finale e vittoria.</summary>
        protected IEnumerator Celebrate(float seconds = 1.6f)
        {
            Confetti(true);
            Sfx.Play(this, "ding", 0.8f);
            yield return new WaitForSeconds(seconds);
            Confetti(false);
            Win();
        }

        /// <summary>Puntatore primario in coordinate mondo (ignora i tocchi che iniziano sull'interfaccia).</summary>
        protected bool Pointer(out Vector2 world, out bool began, out bool ended)
        {
            world = default;
            if (cam == null || !PointerInput.TryGetPrimary(out Vector2 sp, out began, out ended)) { began = ended = false; return false; }
            if (began && PointerInput.IsOverUi()) { began = false; return false; }
            world = PointerInput.ToWorld(cam, sp);
            return true;
        }

        public struct Finger
        {
            public int id;
            public Vector2 world;
            public bool began, ended;
        }

        readonly List<Finger> fingers = new List<Finger>();

        /// <summary>Tutti i diti attivi (il mouse conta come dito -1 quando non ci sono tocchi).</summary>
        protected List<Finger> Fingers()
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

        protected static IEnumerator Pop(Transform t, float peak = 1.2f, float seconds = 0.25f)
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

        protected static IEnumerator Shake(Transform t, float amount = 0.12f, float seconds = 0.35f)
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

        /// <summary>Movimento morbido verso una posizione locale, con eventuale saltello.</summary>
        protected static IEnumerator Move(Transform t, Vector3 to, float seconds, float hop = 0f)
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

        /// <summary>Camminata a velocità costante con piccolo dondolio.</summary>
        protected static IEnumerator Walk(Transform t, Vector3 to, float speed, float bob = 0.05f)
        {
            if (t == null) yield break;
            Vector3 from = t.localPosition;
            float dur = Mathf.Max(0.05f, Vector3.Distance(from, to) / speed);
            for (float k = 0f; k < dur; k += Time.deltaTime)
            {
                t.localPosition = Vector3.Lerp(from, to, k / dur) + Vector3.up * Mathf.Abs(Mathf.Sin(k * 14f)) * bob;
                yield return null;
            }
            t.localPosition = to;
        }

        protected static void Alpha(SpriteRenderer r, float a)
        {
            if (r == null) return;
            Color c = r.color;
            c.a = a;
            r.color = c;
        }
    }
}
