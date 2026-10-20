using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Finger maze" — un piccolo labirinto coperto da un'enorme impronta digitale semitrasparente. Soluzione:
    /// tieni un dito fermo sull'impronta (diventa trasparente) e con un secondo dito guida il topolino all'uscita. Se
    /// alzi il primo dito mentre muovi il topolino il labirinto "si chiude" e lui torna al via; tre urti coi muri =
    /// sconfitta. (Editor: tieni premuto Spazio come primo dito, trascina col mouse.)
    /// </summary>
    public class FingerMazeLevel : LevelController
    {
        [SerializeField] Transform mover, startPoint, exitPoint;
        [SerializeField] Collider2D[] walls;
        [SerializeField] Collider2D printArea;
        [SerializeField] SpriteRenderer printArt, moverArt;
        [SerializeField] TextMesh bumpText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float radius = 0.17f, grabRadius = 0.6f, exitRadius = 0.45f, printAlpha = 0.93f;

        Camera cam;
        int holdId = -1, dragId = -1, bumps;
        Vector2 grab;
        bool busy, hintedHold, mouseDrag;
        float alpha;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            mover.position = startPoint.position;
            alpha = printAlpha;
            S41EKit.SetAlpha(printArt, alpha);
            S41EKit.Confetti(confetti, false);
            UpdateBumps();
            S41EKit.Say(bubble, "Ugh, who left a giant fingerprint on my maze?");
        }

        void UpdateBumps() { if (bumpText != null) bumpText.text = "BUMPS " + bumps + "/3"; }

        bool Holding()
        {
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.Space)) return true;
#endif
            return holdId >= 0;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            bool wasHolding = Holding();
            bool wasDragging = dragId >= 0 || mouseDrag;
            ReadTouches();
            bool holding = Holding();
            if (wasHolding && !holding && wasDragging && (dragId >= 0 || mouseDrag)) { StartCoroutine(MazeClosed()); return; }

            alpha = Mathf.MoveTowards(alpha, holding ? 0f : printAlpha, Time.deltaTime * 4f);
            S41EKit.SetAlpha(printArt, alpha);
            if (Vector2.Distance(mover.position, exitPoint.position) < exitRadius) StartCoroutine(Escape());
        }

        void ReadTouches()
        {
            if (Input.touchCount > 0)
            {
                mouseDrag = false;
                bool holdAlive = false, dragAlive = false;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    int id = Input.GetTouch(i).fingerId;
                    if (id == holdId) holdAlive = true;
                    if (id == dragId) dragAlive = true;
                }
                if (!holdAlive) holdId = -1;
                if (!dragAlive) dragId = -1;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch t = Input.GetTouch(i);
                    Vector2 w = PointerInput.ToWorld(cam, t.position);
                    if (t.phase == TouchPhase.Began)
                    {
                        if (dragId < 0 && Vector2.Distance(w, mover.position) < grabRadius) { dragId = t.fingerId; grab = (Vector2)mover.position - w; Sfx.Play(this, "tick", 0.5f); BlindHint(); }
                        else if (holdId < 0 && printArea.OverlapPoint(w)) holdId = t.fingerId;
                    }
                    bool over = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
                    if (t.fingerId == dragId && !over) MoveTo(w + grab);
                    if (over && t.fingerId == holdId) holdId = -1;
                    if (over && t.fingerId == dragId) dragId = -1;
                }
                return;
            }
            holdId = -1;
            dragId = -1;
            // mouse (Editor): trascina il topolino
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 w = PointerInput.ToWorld(cam, Input.mousePosition);
                if (Vector2.Distance(w, mover.position) < grabRadius) { mouseDrag = true; grab = (Vector2)mover.position - w; BlindHint(); }
            }
            if (mouseDrag && Input.GetMouseButton(0)) MoveTo(PointerInput.ToWorld(cam, Input.mousePosition) + grab);
            if (!Input.GetMouseButton(0)) mouseDrag = false;
        }

        void BlindHint()
        {
            if (Holding() || hintedHold) return;
            hintedHold = true;
            S41EKit.Say(bubble, "I can't see a thing under this print!");
        }

        void MoveTo(Vector2 target)
        {
            if (busy) return;
            Vector2 p = mover.position;
            Vector2 d = target - p;
            float len = d.magnitude;
            if (len < 0.001f) return;
            if (len > 1.0f) { d = d / len * 1.0f; len = 1.0f; }
            int steps = Mathf.CeilToInt(len / 0.04f);
            Vector2 step = d / steps;
            for (int i = 0; i < steps; i++)
            {
                Vector2 n = p + step;
                if (Hits(n)) { mover.position = new Vector3(p.x, p.y, mover.position.z); StartCoroutine(Bump()); return; }
                p = n;
            }
            mover.position = new Vector3(p.x, p.y, mover.position.z);
        }

        bool Hits(Vector2 p)
        {
            foreach (Collider2D w in walls)
            {
                Vector2 c = w.ClosestPoint(p);
                if ((c - p).sqrMagnitude < radius * radius) return true;
            }
            return false;
        }

        IEnumerator Bump()
        {
            busy = true;
            dragId = -1;
            mouseDrag = false;
            bumps++;
            UpdateBumps();
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Light();
            moverArt.color = new Color(1f, 0.5f, 0.5f);
            yield return StartCoroutine(S41EKit.Shake(mover, 0.06f, 0.25f));
            moverArt.color = Color.white;
            if (bumps >= 3)
            {
                S41EKit.Say(bubble, "Ouch... I'm seeing stars.");
                Haptics.Medium();
                yield return new WaitForSeconds(1.0f);
                Fail();
                yield break;
            }
            S41EKit.Say(bubble, bumps == 1 ? "Bonk! A wall!" : "Ow! Careful!");
            busy = false;
        }

        IEnumerator MazeClosed()
        {
            busy = true;
            dragId = -1;
            mouseDrag = false;
            alpha = printAlpha;
            S41EKit.SetAlpha(printArt, alpha);
            Sfx.Play(this, "whoosh", 0.6f);
            S41EKit.Say(bubble, "The print is back! Back to the start...");
            yield return S41EKit.Move(mover, mover.parent.InverseTransformPoint(startPoint.position), 0.5f, 0.3f);
            busy = false;
        }

        IEnumerator Escape()
        {
            busy = true;
            S41EKit.SetAlpha(printArt, 0f);
            Sfx.Play(this, "ding", 0.8f);
            S41EKit.Say(bubble, "CHEESE! I'm out!");
            yield return S41EKit.Move(mover, mover.parent.InverseTransformPoint(exitPoint.position), 0.3f, 0.3f);
            S41EKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.8f);
            S41EKit.Confetti(confetti, false);
            Win();
        }
    }
}
