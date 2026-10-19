using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Slam Dunk" — tiro a canestro con un colpo di dito. La direzione dello swipe mira (punta al canestro),
    /// la velocità decide la forza: la palla vola "in profondità" (rimpicciolisce) e scende nel ferro se è allineata,
    /// altrimenti rimbalza sul ferro o sul tabellone. 3 canestri con 6 palloni; dopo ogni canestro il ferro si sposta
    /// e l'ultimo scivola avanti e indietro (bisogna anticipare il tiro).
    /// </summary>
    public class SlamDunkLevel : LevelController
    {
        public static readonly Vector2 BallStart = new Vector2(0f, -3.3f);
        public static readonly Vector2[] HoopSpots = { new Vector2(0f, 1.9f), new Vector2(-1.1f, 1.6f), new Vector2(0.2f, 2.0f) };

        [SerializeField] Transform ball;
        [SerializeField] SpriteRenderer ballArt;
        [SerializeField] Transform hoop;            // centro del ferro
        [SerializeField] Transform net;             // metà davanti del ferro + retina
        [SerializeField] TextMesh swishText;
        [SerializeField] TextMesh counter;
        [SerializeField] TextMesh prompt;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int ballsTotal = 6, basketsNeeded = 3;
        [SerializeField] float gravity = 14f, minScale = 0.55f, slideAmp = 1.2f, slideSpeed = 0.9f;
        [SerializeField] int frontOrder = 12, behindOrder = 5;

        const float RimHalf = 0.48f, SwishTol = 0.3f, BoardTop = 1.15f, BoardHalf = 0.9f;

        int ballsLeft, baskets, bounces;
        bool flying, scored, done, busy, tracking;
        Vector2 vel, hoopBase;
        float scaleK, slideT;
        Vector3 ballScale, netScale;
        readonly List<Vector3> samples = new List<Vector3>();   // x, y, tempo
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            ballScale = ball.localScale;
            netScale = net.localScale;
            ballsLeft = ballsTotal;
            hoopBase = HoopSpots[0];
            hoop.localPosition = hoopBase;
            swishText.gameObject.SetActive(false);
            ResetBall();
            UpdateHud();
            prompt.text = "FLICK THE BALL UP!";
            if (bubble != null) { bubble.Show("3 baskets, 6 balls. Flick it!"); StartCoroutine(HideLater(2.4f)); }
        }

        void UpdateHud() { counter.text = "BASKETS " + baskets + "/" + basketsNeeded + "   BALLS " + ballsLeft; }

        void ResetBall()
        {
            flying = scored = false;
            bounces = 0;
            scaleK = 0f;
            ball.localPosition = BallStart;
            ball.localRotation = Quaternion.identity;
            ball.localScale = ballScale;
            ballArt.color = Color.white;
            ballArt.sortingOrder = frontOrder;
        }

        float RimY => hoop.localPosition.y;

        void Update()
        {
            if (IsFinished) return;
            // il terzo canestro scivola a destra e sinistra
            if (baskets >= 2 && !done)
            {
                slideT += Time.deltaTime;
                hoop.localPosition = new Vector3(hoopBase.x + Mathf.Sin(slideT * slideSpeed) * slideAmp, hoopBase.y, 0f);
            }
            if (done || busy) return;
            if (flying) { Fly(Time.deltaTime); return; }
            if (Time.timeScale > 0f) ReadFlick();
        }

        // ---------- lancio ----------

        void ReadFlick()
        {
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { tracking = false; return; }
            Vector2 w = transform.InverseTransformPoint(PointerInput.ToWorld(cam, sp));
            if (began)
            {
                tracking = w.y < -0.8f && !PointerInput.IsOverUi();
                samples.Clear();
            }
            if (!tracking) return;
            samples.Add(new Vector3(w.x, w.y, Time.time));
            while (samples.Count > 2 && Time.time - samples[0].z > 0.12f) samples.RemoveAt(0);
            // la palla segue appena il dito
            ball.localPosition = BallStart + Vector2.ClampMagnitude(w - BallStart, 0.25f) * (Vector2.Distance(w, BallStart) < 1.5f ? 1f : 0f);
            if (!ended) return;
            tracking = false;
            Vector3 a = samples[0], b = samples[samples.Count - 1];
            float dt = Mathf.Max(0.016f, b.z - a.z);
            Vector2 flick = new Vector2(b.x - a.x, b.y - a.y) / dt;
            ball.localPosition = BallStart;
            if (flick.y < 2.5f || new Vector2(b.x - a.x, b.y - a.y).magnitude < 0.3f) return;
            Throw(flick);
        }

        /// <summary>
        /// Swipe → tiro: la forza viene dalla velocità del dito (attorno a quella "giusta" per il ferro), la direzione
        /// dall'angolo dello swipe: puntare dritto al canestro lo centra.
        /// </summary>
        void Throw(Vector2 flick)
        {
            float dy = RimY - BallStart.y;
            float ideal = Mathf.Sqrt(2f * gravity * (dy + 0.7f));
            float factor = Mathf.Lerp(0.82f, 1.2f, Mathf.InverseLerp(6f, 30f, flick.magnitude));
            float vy = ideal * factor;
            float disc = vy * vy - 2f * gravity * dy;
            float tStar = (vy + Mathf.Sqrt(Mathf.Max(0f, disc))) / gravity;
            float vx = Mathf.Clamp(flick.x / flick.y, -1.5f, 1.5f) * dy / tStar;
            vel = new Vector2(vx, vy);
            flying = true;
            prompt.text = "";
            Sfx.Play(this, "whoosh", 0.6f);
        }

        void Fly(float dt)
        {
            Vector2 p = ball.localPosition;
            float prevY = p.y;
            p += vel * dt;
            vel.y -= gravity * dt;
            float hx = hoop.localPosition.x, rim = RimY;

            // finta profondità: più sale verso il ferro, più si allontana (e non torna più vicina)
            scaleK = Mathf.Max(scaleK, Mathf.Clamp01((p.y - BallStart.y) / (rim - BallStart.y)));
            ball.localScale = ballScale * Mathf.Lerp(1f, minScale, scaleK);
            ball.localRotation = Quaternion.Euler(0, 0, ball.localEulerAngles.z - vel.x * 120f * dt - 200f * dt);
            if (vel.y < 0f) ballArt.sortingOrder = behindOrder;   // in discesa è alla profondità del ferro

            // tabellone: tiro troppo forte → bonk, la palla ricade sul ferro
            if (vel.y > 0f && p.y > rim + BoardTop && Mathf.Abs(p.x - hx) < BoardHalf)
            {
                vel = new Vector2(vel.x * 0.4f, -1.5f);
                p.y = rim + BoardTop;
                Bonk();
            }

            // attraversa l'altezza del ferro scendendo
            if (!scored && vel.y < 0f && prevY >= rim && p.y < rim)
            {
                float dx = p.x - hx;
                if (Mathf.Abs(dx) < SwishTol) { scored = true; vel = new Vector2(vel.x * 0.15f, Mathf.Min(vel.y, -2f)); p.x = Mathf.Lerp(p.x, hx, 0.5f); StartCoroutine(Swish()); }
                else if (Mathf.Abs(dx) < RimHalf + 0.22f && bounces < 3)
                {
                    bounces++;
                    p.y = rim + 0.01f;
                    vel = new Vector2(Mathf.Sign(dx) * 1.6f + (-dx) * 1.5f, Mathf.Abs(vel.y) * 0.35f + 1.2f);
                    Bonk();
                }
            }
            if (scored) p.x = Mathf.MoveTowards(p.x, hoop.localPosition.x, 2f * dt);   // segue il ferro che scivola
            ball.localPosition = p;
            if (p.y < -5.6f || (vel.y < 0f && p.y < rim - 3.2f)) StartCoroutine(Resolve());
        }

        void Bonk()
        {
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Light();
        }

        IEnumerator Swish()
        {
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Light();
            swishText.gameObject.SetActive(true);
            swishText.transform.localPosition = (Vector2)hoop.localPosition + new Vector2(0f, 1.7f);
            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                float k = t / 0.6f;
                net.localScale = new Vector3(netScale.x * (1f - Mathf.Sin(k * Mathf.PI) * 0.15f), netScale.y * (1f + Mathf.Sin(k * Mathf.PI) * 0.3f), 1f);
                swishText.transform.localScale = Vector3.one * (0.6f + Mathf.Min(1f, k * 3f) * 0.4f);
                yield return null;
            }
            net.localScale = netScale;
        }

        IEnumerator Resolve()
        {
            flying = false;
            busy = true;
            ballsLeft--;
            if (scored) baskets++;
            UpdateHud();
            for (float t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                ballArt.color = new Color(1f, 1f, 1f, 1f - t / 0.3f);
                yield return null;
            }
            swishText.gameObject.SetActive(false);
            if (baskets >= basketsNeeded) { StartCoroutine(Champion()); yield break; }
            if (baskets + ballsLeft < basketsNeeded)
            {
                done = true;
                if (bubble != null) bubble.Show(ballsLeft == 0 ? "Out of balls!" : "Not enough balls left...");
                yield return new WaitForSeconds(1.3f);
                Fail();
                yield break;
            }
            if (scored)
            {
                // canestro: il ferro si sposta
                Vector2 from = hoop.localPosition, to = HoopSpots[Mathf.Min(baskets, HoopSpots.Length - 1)];
                for (float t = 0f; t < 0.6f; t += Time.deltaTime)
                {
                    hoop.localPosition = Vector2.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / 0.6f));
                    yield return null;
                }
                hoopBase = to;
                slideT = 0f;
                hoop.localPosition = to;
                if (baskets == basketsNeeded - 1 && bubble != null) { bubble.Show("Uh oh... the hoop is moving!"); StartCoroutine(HideLater(2f)); }
            }
            else if (bubble != null && ballsLeft <= 2) { bubble.Show(ballsLeft == 1 ? "Last ball!" : "Careful, " + ballsLeft + " balls left."); StartCoroutine(HideLater(1.6f)); }
            ResetBall();
            for (float t = 0f; t < 0.2f; t += Time.deltaTime)
            {
                ball.localScale = ballScale * (t / 0.2f);
                yield return null;
            }
            ball.localScale = ballScale;
            busy = false;
        }

        IEnumerator Champion()
        {
            done = true;
            Sfx.Play(this, "ding", 1f);
            if (bubble != null) bubble.Show("SLAM DUNK CHAMPION!");
            for (float t = 0f; t < 1.5f; t += Time.deltaTime)
            {
                hoop.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 16f) * 4f * (1.5f - t));
                yield return null;
            }
            hoop.localRotation = Quaternion.identity;
            Win();
        }

        IEnumerator HideLater(float s)
        {
            yield return new WaitForSeconds(s);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
