using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Anti-gravity taxi" — il cliente aspetta su una piattaforma volante e il taxi non salta così in alto.
    /// Soluzione: ruota la freccia della gravità (trascinala attorno al medaglione) finché punta in SU: il taxi "cade"
    /// verso l'alto e si incolla sotto la piattaforma. Gravità di traverso = schianto contro i palazzi; tre schianti =
    /// sconfitta.
    /// </summary>
    public class AntiGravityTaxiLevel : LevelController
    {
        [SerializeField] Transform taxi, arrow, badge, platform, client, jumpButton;
        [SerializeField] Collider2D arrowArea, jumpArea;
        [SerializeField] TextMesh triesText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float gravity = 7f, ground = -3.6f, wallX = 2.35f, ceiling = 2.85f, halfLen = 0.72f, platformHalf = 1.0f;

        Camera cam;
        Vector3 taxiHome;
        Vector2 vel;
        float angle = -90f;      // direzione della gravità in gradi (-90 = giù)
        bool dragging, busy;
        int crashes;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            taxiHome = taxi.localPosition;
            SetArrow(angle);
            UpdateTries();
            S41GKit.Confetti(confetti, false);
            if (bubble != null) bubble.Show("TAXI! Up here! Chop chop!");
        }

        void UpdateTries() { triesText.text = "CRASHES: " + crashes + "/3"; }

        void SetArrow(float a) { arrow.localRotation = Quaternion.Euler(0, 0, a + 90f); }

        Vector2 Dir => new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        void Update()
        {
            if (IsFinished || busy) return;
            float dt = Time.deltaTime;
            HandleInput();
            if (dragging) return;   // mentre giri la freccia il mondo aspetta

            Vector2 g = Dir * gravity;
            vel += g * dt;
            Vector3 p = taxi.localPosition + (Vector3)(vel * dt);
            bool downish = Dir.y < -0.9f;
            // il taxi gira con le ruote verso la gravità
            taxi.localRotation = Quaternion.RotateTowards(taxi.localRotation, Quaternion.Euler(0, 0, angle + 90f), 360f * dt);

            if (downish && p.y <= ground) { p.y = ground; vel = Vector2.zero; }
            else if (p.y <= ground) { p.y = ground; vel.y = 0f; }   // scivola sulla strada fino al palazzo
            float under = platform.localPosition.y - 0.15f;
            if (Dir.y > 0.9f && Mathf.Abs(p.x - platform.localPosition.x) < platformHalf && p.y >= under - 0.02f)
            {
                p.y = under;
                taxi.localPosition = p;
                StartCoroutine(Arrived());
                return;
            }
            if (Mathf.Abs(p.x) + halfLen > wallX || p.y > ceiling) { taxi.localPosition = p; StartCoroutine(Crash()); return; }
            taxi.localPosition = p;
        }

        void HandleInput()
        {
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (dragging) ReleaseArrow(); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (arrowArea.OverlapPoint(w)) { dragging = true; Sfx.Play(this, "tick", 0.5f); }
                else if (jumpArea.OverlapPoint(w)) { Jump(); return; }
            }
            if (dragging)
            {
                Vector2 d = w - (Vector2)badge.position;
                if (d.sqrMagnitude > 0.01f) { angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg; SetArrow(angle); }
                if (ended) ReleaseArrow();
            }
        }

        void ReleaseArrow()
        {
            dragging = false;
            angle = Mathf.Round(angle / 45f) * 45f;
            SetArrow(angle);
            Sfx.Play(this, "clunk", 0.6f);
            Vector2 d = Dir;
            if (bubble != null)
            {
                if (d.y > 0.9f) bubble.Show("Whoa, the taxi is falling UP!");
                else if (d.y > -0.9f) bubble.Show("Uh oh, that's sideways...");
            }
        }

        void Jump()
        {
            StartCoroutine(S41GKit.Pop(jumpButton, 1.15f, 0.2f));
            if (Dir.y > -0.9f || taxi.localPosition.y > ground + 0.01f) return;
            vel = new Vector2(0f, 4.5f);
            Sfx.Play(this, "pop", 0.6f);
            if (bubble != null) bubble.Show("HONK! It can't jump that high!");
        }

        IEnumerator Crash()
        {
            busy = true;
            crashes++;
            UpdateTries();
            Sfx.Play(this, "bonk", 0.9f);
            Haptics.Heavy();
            if (bubble != null) bubble.Show(crashes >= 3 ? "Forget it, I'll take the stairs!" : "CRASH! Careful with my ride!");
            for (float k = 0f; k < 0.6f; k += Time.deltaTime) { taxi.localRotation *= Quaternion.Euler(0, 0, 900f * Time.deltaTime); yield return null; }
            yield return new WaitForSeconds(0.5f);
            if (crashes >= 3) { yield return new WaitForSeconds(0.8f); Fail(); yield break; }
            taxi.localPosition = taxiHome;
            taxi.localRotation = Quaternion.identity;
            vel = Vector2.zero;
            angle = -90f;
            SetArrow(angle);
            StartCoroutine(S41GKit.Pop(taxi, 1.2f, 0.3f));
            busy = false;
        }

        IEnumerator Arrived()
        {
            busy = true;
            taxi.localRotation = Quaternion.Euler(0, 0, 180f);
            Sfx.Play(this, "clunk", 0.9f);
            Haptics.Medium();
            yield return StartCoroutine(S41GKit.Shake(platform, 0.06f, 0.3f));
            if (bubble != null) bubble.Show("...Upside down? Whatever. Just drive.");
            yield return new WaitForSeconds(0.8f);
            // il cliente scavalca il bordo e si infila nel taxi
            Vector3 c = client.localPosition;
            yield return StartCoroutine(S41GKit.Move(client, new Vector3(platform.localPosition.x + platformHalf + 0.15f, c.y + 0.2f, 0f), 0.5f, 0.2f));
            Vector3 cs = client.localScale;
            yield return StartCoroutine(S41GKit.Move(client, taxi.localPosition + new Vector3(0f, -0.5f, 0f), 0.4f));
            client.localScale = cs * 0.01f;
            client.gameObject.SetActive(false);
            Sfx.Play(this, "woof", 0.6f);
            if (bubble != null) bubble.Hide();
            Vector3 t0 = taxi.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.0f) { taxi.localPosition = t0 + new Vector3(k * k * 5f, 0f, 0f); yield return null; }
            Sfx.Play(this, "ding", 0.9f);
            S41GKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.6f);
            S41GKit.Confetti(confetti, false);
            Win();
        }
    }
}
