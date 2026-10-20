using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The invisible stairs" — l'omino deve raggiungere una porta che galleggia in cielo, ma non si vedono
    /// scale. Toccando lo spazio vuoto si "sente" cosa c'è: sui gradini invisibili il tocco dà un'onda azzurra e una
    /// vibrazione più forte, e il gradino resta segnato. Trovati i quattro gradini, trascina l'omino da uno all'altro
    /// fino alla porta. Tre salti nel vuoto = sconfitta.
    /// </summary>
    public class TheInvisibleStairsLevel : S41FLevelBase
    {
        [SerializeField] Transform climber;            // piedi dell'omino
        [SerializeField] Collider2D climberArea;
        [SerializeField] Transform[] steps;             // centro del piano dei gradini
        [SerializeField] SpriteRenderer[] stepArts;
        [SerializeField] SpriteRenderer ring;
        [SerializeField] Transform door;                // base della porta
        [SerializeField] SpriteRenderer doorArt;
        [SerializeField] Sprite doorOpen;
        [SerializeField] TextMesh triesText;
        [SerializeField] float groundY = -3.4f, maxUp = 1.4f, maxSide = 2.6f;

        static readonly Vector2[][] Layouts =
        {
            new[] { new Vector2(-0.9f, -2.3f), new Vector2(0.4f, -1.2f), new Vector2(-0.4f, -0.1f), new Vector2(0.8f, 0.85f) },
            new[] { new Vector2(0.3f, -2.35f), new Vector2(-0.9f, -1.3f), new Vector2(0.2f, -0.2f), new Vector2(1.1f, 0.6f) },
            new[] { new Vector2(-1.2f, -2.2f), new Vector2(-0.1f, -1.3f), new Vector2(1.0f, -0.4f), new Vector2(0.1f, 0.55f) },
        };

        const int MaxFalls = 3;
        Vector3 start, standing;
        Vector2 grabOffset;
        bool dragging;
        int falls, found;
        bool[] revealed;

        protected override void Begin()
        {
            Vector2[] l = Layouts[Random.Range(0, Layouts.Length)];
            revealed = new bool[steps.Length];
            for (int i = 0; i < steps.Length; i++)
            {
                steps[i].localPosition = new Vector3(l[i].x, l[i].y, 0f);
                Alpha(stepArts[i], 0f);
            }
            start = standing = climber.localPosition;
            if (ring != null) ring.gameObject.SetActive(false);
            UpdateTries();
            Say("A door up there... but no stairs?!", 2.5f);
        }

        void UpdateTries() { if (triesText != null) triesText.text = "STEPS FOUND " + found + "/4   FALLS " + falls + "/" + MaxFalls; }

        protected override void Tick()
        {
            if (busy) return;
            if (!Pointer(out Vector2 w, out bool began, out bool ended)) { if (dragging) Drop(climber.localPosition); return; }
            if (began)
            {
                if (climberArea != null && climberArea.OverlapPoint(w)) { dragging = true; grabOffset = (Vector2)climber.position - w; Sfx.Play(this, "pop", 0.4f); }
                else Probe(w);
            }
            if (dragging)
            {
                Vector2 p = w + grabOffset;
                climber.position = new Vector3(Mathf.Clamp(p.x, -2.5f, 2.5f), Mathf.Clamp(p.y, groundY, 2.6f), climber.position.z);
                if (ended) Drop(climber.localPosition);
            }
        }

        int StepAt(Vector2 p, float below, float above)
        {
            for (int i = 0; i < steps.Length; i++)
            {
                Vector2 s = steps[i].localPosition;
                if (Mathf.Abs(p.x - s.x) < 0.62f && p.y > s.y - below && p.y < s.y + above) return i;
            }
            return -1;
        }

        void Probe(Vector2 w)
        {
            Vector2 local = transform.InverseTransformPoint(w);
            int i = StepAt(local, 0.5f, 0.35f);
            if (i >= 0)
            {
                Haptics.Medium();
                Sfx.Play(this, "pop", 0.8f);
                StartCoroutine(Ripple(steps[i].position, new Color(0.45f, 0.85f, 1f), 1.1f));
                if (!revealed[i])
                {
                    revealed[i] = true;
                    found++;
                    UpdateTries();
                    StartCoroutine(Reveal(i));
                    if (found == steps.Length) Say("Four steps! Now walk him up!", 2.2f);
                }
            }
            else
            {
                Haptics.Light();
                Sfx.Play(this, "tick", 0.5f);
                StartCoroutine(Ripple(w, new Color(1f, 1f, 1f, 0.6f), 0.5f));
            }
        }

        IEnumerator Ripple(Vector2 at, Color c, float size)
        {
            if (ring == null) yield break;
            ring.gameObject.SetActive(true);
            ring.transform.position = new Vector3(at.x, at.y, ring.transform.position.z);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.45f)
            {
                ring.transform.localScale = Vector3.one * Mathf.Lerp(0.15f, size, k);
                ring.color = new Color(c.r, c.g, c.b, c.a * (1f - k));
                yield return null;
            }
            ring.gameObject.SetActive(false);
        }

        IEnumerator Reveal(int i)
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { Alpha(stepArts[i], k * 0.85f); yield return null; }
            Alpha(stepArts[i], 0.85f);
        }

        void Drop(Vector3 p)
        {
            dragging = false;
            Vector3 from = standing;
            Vector2 dl = (Vector2)door.localPosition;
            bool reach(Vector2 to) => to.y - from.y <= maxUp && Mathf.Abs(to.x - from.x) <= maxSide;
            if (Mathf.Abs(p.x - dl.x) < 0.75f && p.y > dl.y - 0.6f && p.y < dl.y + 1.4f)
            {
                if (reach(dl)) { StartCoroutine(EnterDoor()); return; }
                TooFar();
                return;
            }
            int i = StepAt(p, 0.55f, 0.6f);
            Vector3 target;
            if (i >= 0) target = steps[i].localPosition;
            else if (p.y < groundY + 0.5f) target = new Vector3(p.x, groundY, 0f);
            else
            {
                // salto nel vuoto (se raggiungibile), altrimenti torna indietro
                if (!reach(p)) { TooFar(); return; }
                StartCoroutine(Fall(p));
                return;
            }
            if (!reach(target)) { TooFar(); return; }
            StartCoroutine(Hop(target));
        }

        void TooFar()
        {
            Say("Too far to jump!", 1.4f);
            climber.localPosition = standing;
        }

        IEnumerator Hop(Vector3 target)
        {
            busy = true;
            climber.localPosition = standing;
            yield return Move(climber, target, 0.35f, 0.5f);
            standing = target;
            Sfx.Play(this, "tick", 0.7f);
            Haptics.Light();
            busy = false;
        }

        IEnumerator Fall(Vector3 p)
        {
            busy = true;
            falls++;
            UpdateTries();
            climber.localPosition = standing;
            yield return Move(climber, p, 0.35f, 0.4f);
            Say(falls >= MaxFalls ? "Nope. Nothing there. AGAIN." : "WHOA! Nothing there!", 1.8f);
            Sfx.Play(this, "whoosh", 0.8f);
            float v = 0f;
            while (climber.localPosition.y > groundY)
            {
                v += 14f * Time.deltaTime;
                climber.localPosition += Vector3.down * v * Time.deltaTime;
                climber.localRotation = Quaternion.Euler(0, 0, climber.localRotation.eulerAngles.z + 400f * Time.deltaTime);
                yield return null;
            }
            climber.localPosition = new Vector3(climber.localPosition.x, groundY, 0f);
            climber.localRotation = Quaternion.Euler(0, 0, 90f);
            Sfx.Play(this, "bonk", 1f);
            Haptics.Heavy();
            yield return Shake(climber, 0.08f, 0.4f);
            if (falls >= MaxFalls)
            {
                yield return new WaitForSeconds(1.0f);
                Fail();
                yield break;
            }
            yield return new WaitForSeconds(0.4f);
            climber.localRotation = Quaternion.identity;
            climber.localPosition = start;
            standing = start;
            busy = false;
        }

        IEnumerator EnterDoor()
        {
            busy = true;
            climber.localPosition = standing;
            for (int i = 0; i < steps.Length; i++) StartCoroutine(Reveal(i));
            yield return Move(climber, door.localPosition + Vector3.up * 0.02f, 0.4f, 0.5f);
            if (doorArt != null && doorOpen != null) doorArt.sprite = doorOpen;
            Sfx.Play(this, "clunk", 0.6f);
            Say("Who needs visible stairs anyway?", 2f);
            yield return new WaitForSeconds(0.5f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { climber.localScale = Vector3.one * (1f - k); yield return null; }
            climber.gameObject.SetActive(false);
            yield return Celebrate();
        }
    }
}
