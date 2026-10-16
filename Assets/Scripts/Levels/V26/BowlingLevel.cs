using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 98 — il bowling (visto dall'alto). Lancia la palla con uno swipe dal fondo della pista. In mezzo alla
    /// pista c'è un cono "NON TOCCARE": la palla dritta ci sbatte. Uno swipe CURVO dà effetto alla palla, che gira
    /// attorno al cono. Due tiri per buttare giù tutti i birilli.
    /// </summary>
    public class BowlingLevel : LevelController
    {
        [SerializeField] Rigidbody2D ball;
        [SerializeField] Rigidbody2D[] pins;
        [SerializeField] Collider2D throwArea;
        [SerializeField] TextMesh info;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float power = 1.4f, maxSpeed = 11f, spinForce = 9f;
        [SerializeField] float laneTop = 3.6f, laneHalf = 1.5f;

        readonly List<Vector2> trail = new List<Vector2>();
        readonly List<float> times = new List<float>();
        Vector3 ballHome;
        Vector3[] pinHome;
        bool[] down;
        float spin;
        int throwsLeft = 2;
        bool rolling, done;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            ballHome = ball.position;
            pinHome = new Vector3[pins.Length];
            down = new bool[pins.Length];
            for (int i = 0; i < pins.Length; i++) pinHome[i] = pins[i].position;
            Setup(ball, 0.8f, 3f);
            foreach (Rigidbody2D p in pins) Setup(p, 2.5f, 0.6f);
            ball.bodyType = RigidbodyType2D.Kinematic;
            Info();
            if (bubble != null) { bubble.Show("Swipe up to bowl. Curve it!"); StartCoroutine(HideLater(2f)); }
        }

        static void Setup(Rigidbody2D rb, float damping, float mass)
        {
            rb.gravityScale = 0f;
            rb.mass = mass;
#if UNITY_6000_0_OR_NEWER
            rb.linearDamping = damping;
            rb.angularDamping = 2f;
#else
            rb.drag = damping;
            rb.angularDrag = 2f;
#endif
        }

        static Vector2 Vel(Rigidbody2D rb)
        {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.velocity;
#endif
        }

        static void SetVel(Rigidbody2D rb, Vector2 v)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = v;
#else
            rb.velocity = v;
#endif
        }

        void Info() { if (info != null) info.text = "THROWS: " + throwsLeft; }

        void Update()
        {
            if (done || IsFinished || rolling) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began) { trail.Clear(); times.Clear(); if (!throwArea.OverlapPoint(w)) return; }
            if (trail.Count == 0 && !began) return;
            trail.Add(w);
            times.Add(Time.time);
            ball.position = new Vector2(Mathf.Clamp(w.x, -laneHalf + 0.3f, laneHalf - 0.3f), ballHome.y);
            if (ended && trail.Count >= 3) Throw();
        }

        void Throw()
        {
            Vector2 a = trail[0], b = trail[trail.Count - 1];
            float dt = Mathf.Max(0.05f, times[times.Count - 1] - times[0]);
            Vector2 v = (b - a) / dt * power;
            if (v.y < 1f) { trail.Clear(); return; }   // non era un lancio
            v = Vector2.ClampMagnitude(v, maxSpeed);
            // effetto: quanto il dito ha curvato rispetto alla linea dritta (positivo = curva a destra)
            Vector2 dir = (b - a).normalized, side = new Vector2(dir.y, -dir.x);
            float bend = 0f;
            foreach (Vector2 p in trail) { float s = Vector2.Dot(p - a, side); if (Mathf.Abs(s) > Mathf.Abs(bend)) bend = s; }
            spin = Mathf.Clamp(bend, -1f, 1f);
            v = Quaternion.Euler(0, 0, -spin * 28f) * v;   // parte verso la pancia della curva...
            ball.bodyType = RigidbodyType2D.Dynamic;
            SetVel(ball, v);
            Sfx.Play(this, "whoosh");
            StartCoroutine(Roll());
        }

        IEnumerator Roll()
        {
            rolling = true;
            throwsLeft--;
            Info();
            // ...e l'effetto la riporta indietro: segue l'arco disegnato dal dito
            for (float k = 0f; k < 4f; k += Time.fixedDeltaTime)
            {
                Vector2 v = Vel(ball);
                if (v.magnitude > 0.3f) ball.AddForce(new Vector2(v.y, -v.x).normalized * -spin * spinForce * Mathf.Clamp01(k * 1.5f));
                if (ball.position.y > laneTop + 0.5f || Mathf.Abs(ball.position.x) > laneHalf + 0.6f || (k > 0.5f && v.magnitude < 0.2f)) break;
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(1.2f);
            int standing = 0;
            for (int i = 0; i < pins.Length; i++)
            {
                if (!down[i] && (Vector2.Distance(pins[i].position, pinHome[i]) > 0.18f || Mathf.Abs(Mathf.DeltaAngle(pins[i].rotation, 0f)) > 25f))
                {
                    down[i] = true;
                    pins[i].gameObject.SetActive(false);
                }
                if (!down[i]) standing++;
            }
            if (standing == 0)
            {
                done = true;
                Sfx.Play(this, "ding", 0.8f);
                if (bubble != null) bubble.Show(throwsLeft == 1 ? "STRIKE!!!" : "Spare! Nice.");
                yield return new WaitForSeconds(1.5f);
                Win();
                yield break;
            }
            if (throwsLeft <= 0)
            {
                done = true;
                if (bubble != null) bubble.Show(standing + " left standing. So close!");
                yield return new WaitForSeconds(1.5f);
                Fail();
                yield break;
            }
            // seconda palla: i birilli in piedi tornano al loro posto
            for (int i = 0; i < pins.Length; i++)
                if (!down[i]) { pins[i].position = pinHome[i]; pins[i].rotation = 0f; SetVel(pins[i], Vector2.zero); pins[i].angularVelocity = 0f; }
            ball.bodyType = RigidbodyType2D.Kinematic;
            SetVel(ball, Vector2.zero);
            ball.angularVelocity = 0f;
            ball.position = ballHome;
            ball.transform.position = ballHome;
            if (bubble != null) { bubble.Show("One more ball!", true); StartCoroutine(HideLater(1f)); }
            rolling = false;
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
