using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The broken compass" — il topolino è a un incrocio di un labirinto di siepi avvolto nella nebbia; la
    /// bussola dovrebbe portarlo al formaggio ma l'ago punta al contrario e la ghiera è sottosopra. Soluzione: girare il
    /// telefono a testa in giù (l'ago si "aggiusta" e indica la strada giusta), poi ruotare col dito la ghiera finché la
    /// N coincide con l'ago: il topolino parte. Imboccare una strada senza la bussola sistemata porta in un vicolo cieco;
    /// tre volte → Fail(). Non serve la bussola magnetica. Senza accelerometro: tenere premuto il vetro per 2 s.
    /// (Editor: freccia su = capovolto.)
    /// </summary>
    public class BrokenCompassLevel : LevelController
    {
        [SerializeField] Transform compass;              // centro della bussola
        [SerializeField] Transform bezel;                // ghiera girevole con le lettere
        [SerializeField] Transform needle;
        [SerializeField] SpriteRenderer glow;
        [SerializeField] Collider2D[] paths;             // frecce N, E, O
        [SerializeField] Transform[] pathEnds;           // fondo dei corridoi N, E, O
        [SerializeField] SpriteRenderer[] fogs;          // nebbia sui corridoi N, E, O
        [SerializeField] Transform cheese;
        [SerializeField] Transform mouse;
        [SerializeField] TextMesh strikesLabel;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float tolerance = 20f, grabInner = 0.35f, grabOuter = 1.5f;

        static readonly float[] PathAngles = { 0f, -90f, 90f };   // N, E, O in gradi (rotazione z)

        Camera cam;
        int truth, wrong;
        bool fixedNeedle, dragging, busy;
        float bezelAngle, needleAngle, grabAngle, grabBezel, clock, bubbleUntil, upsideFor, glassHold;
        Vector3 mouseHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            truth = Random.Range(0, PathAngles.Length);
            cheese.position = pathEnds[truth].position;
            cheese.gameObject.SetActive(false);
            mouseHome = mouse.position;
            bezelAngle = 180f + Random.Range(-35f, 35f);
            bezel.localRotation = Quaternion.Euler(0f, 0f, bezelAngle);
            if (glow != null) glow.enabled = false;
            UpdateStrikes();
            Say("Which way to the cheese? This compass is weird...", 2.4f);
        }

        void UpdateStrikes() { if (strikesLabel != null) strikesLabel.text = "DEAD ENDS: " + wrong + "/3"; }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void Update()
        {
            if (IsFinished) return;
            float dt = Time.deltaTime;
            clock += dt;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }

            // l'ago: rotto punta al contrario e tremola; aggiustato indica la strada vera
            float want = fixedNeedle ? PathAngles[truth] + Mathf.Sin(clock * 2f) * 2f : PathAngles[truth] + 180f + Mathf.Sin(clock * 3.1f) * 25f + Mathf.Sin(clock * 11f) * 6f;
            needleAngle = Mathf.LerpAngle(needleAngle, want, 1f - Mathf.Exp(-dt * (fixedNeedle ? 5f : 8f)));
            needle.localRotation = Quaternion.Euler(0f, 0f, needleAngle);
            if (busy) return;

            if (!fixedNeedle)
            {
                upsideFor = Inputs.Tilt().y > 0.7f ? upsideFor + dt : 0f;
                bool noSensor = !SystemInfo.supportsAccelerometer;
                if (noSensor && PointerInput.TryGetPrimary(out Vector2 sp, out _, out _) && Vector2.Distance(PointerInput.ToWorld(cam, sp), compass.position) < grabInner + 0.2f)
                    glassHold += dt;
                else glassHold = 0f;
                if (upsideFor > 0.6f || glassHold > 2f) { StartCoroutine(Fix()); return; }
            }

            if (!S41AKit.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { dragging = false; return; }
            Vector2 rel = w - (Vector2)compass.position;
            if (began)
            {
                for (int i = 0; i < paths.Length; i++)
                    if (paths[i].OverlapPoint(w)) { StartCoroutine(Wander(i)); return; }
                if (rel.magnitude > grabInner && rel.magnitude < grabOuter)
                {
                    dragging = true;
                    grabAngle = Mathf.Atan2(rel.y, rel.x) * Mathf.Rad2Deg;
                    grabBezel = bezelAngle;
                }
                return;
            }
            if (!dragging) return;
            float a = Mathf.Atan2(rel.y, rel.x) * Mathf.Rad2Deg;
            float nb = grabBezel + Mathf.DeltaAngle(grabAngle, a);
            if (Mathf.FloorToInt(nb / 15f) != Mathf.FloorToInt(bezelAngle / 15f)) Sfx.Play(this, "tick", 0.35f);
            bezelAngle = nb;
            bezel.localRotation = Quaternion.Euler(0f, 0f, bezelAngle);
            if (!ended) return;
            dragging = false;
            if (fixedNeedle && Mathf.Abs(Mathf.DeltaAngle(bezelAngle, PathAngles[truth])) < tolerance) StartCoroutine(Calibrated());
            else if (!fixedNeedle && Mathf.Abs(Mathf.DeltaAngle(bezelAngle, needleAngle)) < tolerance) Say("Still wrong... the NEEDLE is broken!", 1.8f);
        }

        IEnumerator Fix()
        {
            busy = true;
            fixedNeedle = true;
            Sfx.Play(this, "clunk", 0.7f);
            Haptics.Medium();
            yield return S41AKit.Bounce(compass, 0.12f, 0.25f);
            Sfx.Play(this, "ding", 0.7f);
            Say("Whoa! The needle flipped! Now fix the dial!", 2.4f);
            busy = false;
        }

        IEnumerator Calibrated()
        {
            busy = true;
            bezelAngle = PathAngles[truth];
            bezel.localRotation = Quaternion.Euler(0f, 0f, bezelAngle);
            if (glow != null) glow.enabled = true;
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Light();
            Say("The compass works! This way!", 1.6f);
            yield return new WaitForSeconds(0.6f);
            // la nebbia si dirada e il topolino corre al formaggio
            cheese.gameObject.SetActive(true);
            SpriteRenderer fog = fogs[truth];
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f) { S41AKit.Alpha(fog, 1f - k); yield return null; }
            yield return S41AKit.Move(mouse, pathEnds[truth].position - (pathEnds[truth].position - mouseHome).normalized * 0.45f, 1.0f, 0.1f);
            Sfx.Play(this, "pop", 0.8f);
            yield return S41AKit.Bounce(cheese, 0.3f, 0.3f);
            Say("CHEESE!", 1.2f);
            yield return new WaitForSeconds(1.0f);
            Win();
        }

        IEnumerator Wander(int path)
        {
            busy = true;
            Say("No compass... I'll just guess!", 1.4f);
            Vector3 end = pathEnds[path].position;
            yield return S41AKit.Move(mouse, Vector3.Lerp(mouseHome, end, 0.85f), 0.8f, 0.08f);
            mouse.gameObject.SetActive(false);
            Sfx.Play(this, "bonk", 0.9f);
            Haptics.Medium();
            yield return new WaitForSeconds(0.5f);
            mouse.gameObject.SetActive(true);
            wrong++;
            UpdateStrikes();
            yield return S41AKit.Move(mouse, mouseHome, 0.7f, 0.08f);
            if (wrong >= 3)
            {
                Say("Lost in the fog. Forever.", 1.8f);
                Sfx.Play(this, "buzz", 0.8f);
                Haptics.Heavy();
                yield return new WaitForSeconds(1.4f);
                Fail();
                yield break;
            }
            Say(path == truth ? "Fog everywhere! Got lost..." : "Dead end! Ouch.", 1.6f);
            busy = false;
        }
    }
}
