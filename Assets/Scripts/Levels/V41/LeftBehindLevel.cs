using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Left behind" — una freccia gigante punta a destra e il cartello dice "GO RIGHT"; il topolino sta su un
    /// isolotto con una porta a sinistra (chiusa col lucchetto) e una a destra. Soluzione: trascinare via la parola
    /// RIGHT: sotto c'è scritto LEFT, il lucchetto cade e la freccia si gira; poi si tocca la porta di sinistra.
    /// La porta di destra riporta sempre all'inizio (in modi buffi); alla terza volta → Fail().
    /// </summary>
    public class LeftBehindLevel : LevelController
    {
        [SerializeField] Transform rightTag;             // la parola RIGHT con la sua toppa (si trascina)
        [SerializeField] Collider2D rightTagArea;
        [SerializeField] TextMesh leftWord;
        [SerializeField] Transform arrow;
        [SerializeField] Collider2D leftDoor, rightDoor;
        [SerializeField] Transform leftPanel, rightPanel;   // ante delle porte (si aprono scalando x)
        [SerializeField] SpriteRenderer leftLight, rightLight;
        [SerializeField] Transform padlock;
        [SerializeField] Transform mouse;
        [SerializeField] TextMesh triesLabel;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float tearDistance = 1.1f;

        Camera cam;
        Vector3 tagHome, mouseHome, arrowScale;
        Vector2 grab;
        bool dragging, revealed, busy;
        int rightTrips;
        float clock, bubbleUntil;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            tagHome = rightTag.position;
            mouseHome = mouse.position;
            arrowScale = arrow.localScale;
            leftWord.gameObject.SetActive(true);
            leftLight.enabled = false;
            rightLight.enabled = false;
            UpdateTries();
            Say("Which way? The sign says RIGHT...", 2.2f);
        }

        void UpdateTries() { if (triesLabel != null) triesLabel.text = rightTrips > 0 ? "BACK TO START: " + rightTrips + "/3" : ""; }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) return;
            if (!S41AKit.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { dragging = false; return; }

            if (began)
            {
                if (!revealed && rightTagArea.OverlapPoint(w)) { dragging = true; grab = (Vector2)rightTag.position - w; Sfx.Play(this, "tick", 0.6f); return; }
                if (rightDoor.OverlapPoint(w)) { StartCoroutine(GoRight()); return; }
                if (leftDoor.OverlapPoint(w)) { StartCoroutine(revealed ? GoLeft() : Locked()); return; }
                return;
            }
            if (!dragging) return;
            Vector2 to = w + grab;
            rightTag.position = new Vector3(to.x, to.y, rightTag.position.z);
            if (!ended) return;
            dragging = false;
            if (Vector2.Distance(rightTag.position, tagHome) > tearDistance) StartCoroutine(Reveal());
            else StartCoroutine(S41AKit.Move(rightTag, tagHome, 0.15f));
        }

        IEnumerator Reveal()
        {
            busy = true;
            revealed = true;
            Sfx.Play(this, "whoosh", 0.8f);
            // la parola cade giù dallo schermo girando
            StartCoroutine(Tumble(rightTag));
            yield return S41AKit.Bounce(leftWord.transform, 0.3f, 0.3f);
            Say("LEFT?! It was LEFT all along!", 1.8f);
            // la freccia si gira
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                arrow.localScale = new Vector3(arrowScale.x * Mathf.Cos(k * Mathf.PI), arrowScale.y, 1f);
                yield return null;
            }
            arrow.localScale = new Vector3(-arrowScale.x, arrowScale.y, 1f);
            Sfx.Play(this, "ding", 0.6f);
            // il lucchetto cade
            Sfx.Play(this, "clunk", 0.8f);
            yield return S41AKit.Move(padlock, padlock.position + new Vector3(-0.3f, -3.5f, 0f), 0.5f, 0.3f);
            padlock.gameObject.SetActive(false);
            Haptics.Light();
            busy = false;
        }

        IEnumerator Tumble(Transform t)
        {
            Vector2 v = new Vector2(Random.Range(-1f, 1f), 1.5f);
            float spin = 0f;
            while (t.position.y > -7f)
            {
                v.y -= 9f * Time.deltaTime;
                t.position += (Vector3)(v * Time.deltaTime);
                spin += Time.deltaTime * 300f;
                t.localRotation = Quaternion.Euler(0f, 0f, spin);
                yield return null;
            }
            t.gameObject.SetActive(false);
        }

        IEnumerator Locked()
        {
            busy = true;
            Sfx.Play(this, "clunk", 0.6f);
            Vector3 p = padlock.position;
            for (float t = 0f; t < 0.3f; t += Time.deltaTime) { padlock.position = p + new Vector3(Mathf.Sin(t * 60f) * 0.06f, 0f, 0f); yield return null; }
            padlock.position = p;
            Say("Locked. The sign says GO RIGHT!", 1.6f);
            busy = false;
        }

        IEnumerator OpenDoor(Transform panel, SpriteRenderer light, bool open)
        {
            light.enabled = true;
            float a = open ? 1f : 0.15f, b = open ? 0.15f : 1f;
            Vector3 s = panel.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                panel.localScale = new Vector3(Mathf.Lerp(a, b, k), s.y, 1f);
                yield return null;
            }
            panel.localScale = new Vector3(b, s.y, 1f);
            if (!open) light.enabled = false;
            Sfx.Play(this, open ? "tick" : "clunk", 0.6f);
        }

        IEnumerator Walk(float x)
        {
            Vector3 p = mouse.position;
            Vector3 sc = mouse.localScale;
            mouse.localScale = new Vector3(Mathf.Abs(sc.x) * (x < p.x ? -1f : 1f), sc.y, 1f);
            float dur = Mathf.Abs(x - p.x) / 2.4f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / Mathf.Max(dur, 0.05f))
            {
                mouse.position = new Vector3(Mathf.Lerp(p.x, x, k), p.y + Mathf.Abs(Mathf.Sin(k * dur * 14f)) * 0.06f, p.z);
                yield return null;
            }
            mouse.position = new Vector3(x, p.y, p.z);
        }

        IEnumerator GoRight()
        {
            busy = true;
            if (bubble != null) bubble.Hide();
            float doorX = rightDoor.transform.position.x;
            yield return Walk(doorX - 0.6f);
            yield return OpenDoor(rightPanel, rightLight, true);
            yield return Walk(doorX);
            mouse.gameObject.SetActive(false);
            yield return OpenDoor(rightPanel, rightLight, false);
            rightTrips++;
            UpdateTries();
            Sfx.Play(this, "whoosh", 0.8f);
            yield return new WaitForSeconds(0.5f);
            mouse.gameObject.SetActive(true);
            mouse.localScale = new Vector3(Mathf.Abs(mouse.localScale.x), mouse.localScale.y, 1f);
            if (rightTrips == 1)
            {
                // ricade dal cielo nel punto di partenza
                mouse.position = mouseHome + Vector3.up * 7f;
                yield return S41AKit.Move(mouse, mouseHome, 0.45f);
                Sfx.Play(this, "bonk", 0.9f);
                Haptics.Medium();
                Say("Ouch! ...back at the start?", 1.8f);
            }
            else if (rightTrips == 2)
            {
                // rispunta dalla stessa porta, tutto stordito, e torna indietro di corsa
                mouse.position = new Vector3(doorX, mouseHome.y, mouseHome.z);
                yield return OpenDoor(rightPanel, rightLight, true);
                yield return Walk(mouseHome.x);
                StartCoroutine(OpenDoor(rightPanel, rightLight, false));
                for (int i = 0; i < 3; i++) { mouse.Rotate(0f, 0f, 120f); yield return new WaitForSeconds(0.08f); }
                mouse.localRotation = Quaternion.identity;
                Say("Dizzy... I've been here before!", 1.8f);
            }
            else
            {
                mouse.position = mouseHome + Vector3.up * 7f;
                yield return S41AKit.Move(mouse, mouseHome, 0.4f);
                Sfx.Play(this, "bonk", 1f);
                Haptics.Heavy();
                Say("AGAIN?! I give up!", 1.6f);
                yield return new WaitForSeconds(1.4f);
                Fail();
                yield break;
            }
            mouse.position = mouseHome;
            busy = false;
        }

        IEnumerator GoLeft()
        {
            busy = true;
            if (bubble != null) bubble.Hide();
            float doorX = leftDoor.transform.position.x;
            yield return Walk(doorX + 0.6f);
            yield return OpenDoor(leftPanel, leftLight, true);
            Sfx.Play(this, "ding", 0.8f);
            yield return Walk(doorX);
            mouse.gameObject.SetActive(false);
            Say("FREEDOM! (left was right)", 1.6f);
            Haptics.Light();
            yield return new WaitForSeconds(1.3f);
            Win();
        }
    }
}
