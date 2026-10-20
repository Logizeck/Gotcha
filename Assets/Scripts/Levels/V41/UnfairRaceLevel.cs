using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Unfair race" — lumaca contro lepre: la lepre parte a razzo e vince sempre. Non si può rendere più veloce
    /// la lumaca, ma si può cambiare la gara: prima del via trascina il TRAGUARDO nella corsia della lumaca, subito
    /// davanti a lei; la lepre corre fino in fondo senza trovare l'arrivo e la lumaca vince. Tre gare perse = sconfitta.
    /// </summary>
    public class UnfairRaceLevel : LevelController
    {
        [SerializeField] Transform hare, snail, finish, goButton;
        [SerializeField] Collider2D finishArea, goArea;
        [SerializeField] TextMesh countdown, triesText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float hareLaneY = 0.2f, snailLaneY = -1.6f, gateHalf = 1.15f, hareSpeed = 3.4f, snailSpeed = 0.55f, trackEnd = 2.45f;

        Camera cam;
        Vector3 hareHome, snailHome;
        Vector2 grabOffset;
        bool dragging, busy;
        int lost;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            hareHome = hare.localPosition;
            snailHome = snail.localPosition;
            countdown.text = "";
            UpdateTries();
            S41GKit.Confetti(confetti, false);
            if (bubble != null) bubble.Show("Ready to lose again, slimy?");
        }

        void UpdateTries() { triesText.text = "RACES LOST: " + lost + "/3"; }

        bool Covers(float laneY) => Mathf.Abs(finish.localPosition.y - laneY) < gateHalf;

        void Update()
        {
            if (IsFinished || busy) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { dragging = false; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (goArea.OverlapPoint(w)) { StartCoroutine(Race()); return; }
                if (finishArea.OverlapPoint(w)) { dragging = true; grabOffset = (Vector2)finish.position - w; Sfx.Play(this, "tick", 0.6f); }
            }
            if (dragging)
            {
                Vector2 p = w + grabOffset;
                finish.position = new Vector3(Mathf.Clamp(p.x, -2.5f, 2.55f), Mathf.Clamp(p.y, -2.6f, 1.4f), finish.position.z);
            }
            if (ended && dragging)
            {
                dragging = false;
                Sfx.Play(this, "clunk", 0.5f);
                if (Covers(snailLaneY) && !Covers(hareLaneY) && bubble != null) bubble.Show("Hey! That's... technically allowed?");
            }
        }

        IEnumerator Race()
        {
            busy = true;
            dragging = false;
            if (bubble != null) bubble.Hide();
            StartCoroutine(S41GKit.Pop(goButton, 1.2f, 0.2f));
            foreach (string s in new[] { "3", "2", "1" })
            {
                countdown.text = s;
                Sfx.Play(this, "tick", 0.8f);
                yield return new WaitForSeconds(0.55f);
            }
            countdown.text = "GO!";
            Sfx.Play(this, "ding", 0.6f);
            bool hareCan = Covers(hareLaneY) && finish.localPosition.x > hareHome.x + 0.3f;
            bool snailCan = Covers(snailLaneY) && finish.localPosition.x > snailHome.x + 0.3f;
            float gx = finish.localPosition.x, t = 0f, hareStopped = -1f;
            bool done = false, snailWon = false;
            while (!done)
            {
                float dt = Time.deltaTime;
                t += dt;
                if (t > 0.6f) countdown.text = "";
                // lepre
                if (hareStopped < 0f)
                {
                    Vector3 h = hare.localPosition;
                    h.x += hareSpeed * dt;
                    h.y = hareHome.y + Mathf.Abs(Mathf.Sin(t * 14f)) * 0.15f;
                    if (hareCan && h.x + 0.45f >= gx) { hare.localPosition = h; done = true; break; }
                    if (h.x >= trackEnd) { h.x = trackEnd; h.y = hareHome.y; hareStopped = t; if (bubble != null) bubble.Show("WHERE'S THE FINISH LINE?!"); Sfx.Play(this, "bonk", 0.5f); }
                    hare.localPosition = h;
                }
                else hare.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 6f) * 8f);
                // lumaca
                if (snailCan)
                {
                    Vector3 s = snail.localPosition;
                    s.x += snailSpeed * dt;
                    snail.localPosition = s;
                    if (s.x + 0.5f >= gx) { snailWon = true; done = true; }
                }
                else if (hareStopped >= 0f && t - hareStopped > 1.4f) done = true;
                yield return null;
            }
            countdown.text = "";
            hare.localRotation = Quaternion.identity;
            if (snailWon) { yield return StartCoroutine(SnailWins()); yield break; }

            // la lepre ha vinto (o nessuno ha trovato l'arrivo)
            lost++;
            UpdateTries();
            Sfx.Play(this, "buzz", 0.6f);
            Haptics.Medium();
            if (bubble != null) bubble.Show(hareCan ? "HA! Too easy. Again?" : "No finish line for the snail? Race cancelled!");
            yield return StartCoroutine(S41GKit.Pop(hare, 1.2f, 0.4f));
            yield return new WaitForSeconds(1.0f);
            if (lost >= 3)
            {
                if (bubble != null) bubble.Show("Three wins in a row! I'm the CHAMPION!");
                yield return new WaitForSeconds(1.4f);
                Fail();
                yield break;
            }
            StartCoroutine(S41GKit.Move(hare, hareHome, 0.5f, 0.3f));
            yield return StartCoroutine(S41GKit.Move(snail, snailHome, 0.5f));
            busy = false;
        }

        IEnumerator SnailWins()
        {
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Light();
            countdown.text = "SNAIL WINS!";
            yield return StartCoroutine(S41GKit.Pop(snail, 1.4f, 0.5f));
            if (bubble != null) bubble.Show("WHAT?! Who moved the finish line?!");
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f) { hare.localRotation = Quaternion.Euler(0, 0, 80f * k); yield return null; }
            Sfx.Play(this, "bonk", 0.6f);
            S41GKit.Confetti(confetti, true);
            yield return new WaitForSeconds(2.0f);
            S41GKit.Confetti(confetti, false);
            Win();
        }
    }
}
