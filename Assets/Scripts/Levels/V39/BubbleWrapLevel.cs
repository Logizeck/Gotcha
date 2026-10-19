using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Bubble wrap" — scoppia tutte le bolle del pluriball (6×7). Quando le hai scoppiate tutte il contatore
    /// dice ancora "1": l'ultima bolla è sul RETRO del foglio. Si gira con uno swipe orizzontale; dietro c'è una
    /// sola bolla intatta: scoppiala e vinci.
    /// </summary>
    public class BubbleWrapLevel : LevelController
    {
        [SerializeField] Transform sheet;             // si gira (scala x)
        [SerializeField] GameObject front, back;
        [SerializeField] SpriteRenderer[] bubbles;    // quelle davanti
        [SerializeField] SpriteRenderer backBubble;   // l'unica dietro
        [SerializeField] Sprite poppedSprite;
        [SerializeField] TextMesh counter;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float hitRadius = 0.38f, swipeDistance = 1.0f;

        bool[] popped;
        bool backPopped, showingBack, flipping, done, swiped, pressing;
        int left, emptyTaps;
        Vector2 pressStart;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            popped = new bool[bubbles.Length];
            left = bubbles.Length + 1;
            back.SetActive(false);
            foreach (SmokeEmitter c in confetti) if (c != null) c.Stop();
            Count();
            Say("Pop them ALL. Every single one.", 2.4f);
        }

        void Count() { counter.text = "BUBBLES LEFT: " + left; }

        void Update()
        {
            if (done || flipping || IsFinished) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { pressing = false; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                pressing = true;
                swiped = false;
                pressStart = w;
                Tap(w);
                return;
            }
            if (!pressing || swiped) { if (ended) pressing = false; return; }
            Vector2 d = w - pressStart;
            if (Mathf.Abs(d.x) > swipeDistance && Mathf.Abs(d.x) > Mathf.Abs(d.y) * 1.5f && Mathf.Abs(pressStart.y - sheet.position.y) < 3.2f)
            {
                swiped = true;
                StartCoroutine(Flip());
            }
            if (ended) pressing = false;
        }

        void Tap(Vector2 w)
        {
            // coordinate del foglio (non specchiate: il foglio è girato solo durante l'animazione)
            Vector2 local = w - (Vector2)sheet.position;
            if (Mathf.Abs(local.x) > 2.5f || Mathf.Abs(local.y) > 2.9f) return;
            if (showingBack)
            {
                if (!backPopped && Vector2.Distance(w, backBubble.transform.position) < hitRadius)
                {
                    backPopped = true;
                    Pop(backBubble);
                    return;
                }
            }
            else
            {
                int best = -1;
                float bestD = hitRadius;
                for (int i = 0; i < bubbles.Length; i++)
                {
                    if (popped[i]) continue;
                    float d = Vector2.Distance(w, bubbles[i].transform.position);
                    if (d < bestD) { bestD = d; best = i; }
                }
                if (best >= 0)
                {
                    popped[best] = true;
                    Pop(bubbles[best]);
                    return;
                }
            }
            // tocco a vuoto
            bool sideEmpty = showingBack ? backPopped : System.Array.TrueForAll(popped, p => p);
            if (!sideEmpty) return;
            emptyTaps++;
            Sfx.Play(this, "tick", 0.4f);
            if (emptyTaps == 3) Say("Have you checked everywhere?", 2.6f);
            else if (emptyTaps == 8) Say("Every sheet has two sides...", 2.6f);
        }

        void Pop(SpriteRenderer b)
        {
            b.sprite = poppedSprite;
            left--;
            Count();
            Sfx.Play(this, "pop", 0.8f);
            Haptics.Light();
            StartCoroutine(Punch(b.transform));
            if (left <= 0) StartCoroutine(Victory());
        }

        IEnumerator Punch(Transform t)
        {
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f)
            {
                t.localScale = s * (1f + Mathf.Sin(k * Mathf.PI) * 0.25f);
                yield return null;
            }
            t.localScale = s;
        }

        IEnumerator Flip()
        {
            flipping = true;
            Sfx.Play(this, "whoosh", 0.6f);
            Vector3 s = sheet.localScale;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.18f) { sheet.localScale = new Vector3(s.x * (1f - t), s.y, s.z); yield return null; }
            showingBack = !showingBack;
            front.SetActive(!showingBack);
            back.SetActive(showingBack);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.18f) { sheet.localScale = new Vector3(s.x * t, s.y, s.z); yield return null; }
            sheet.localScale = s;
            flipping = false;
            if (showingBack && !backPopped) Say("Oh. THERE you are.", 1.8f);
        }

        IEnumerator Victory()
        {
            done = true;
            yield return new WaitForSeconds(0.3f);
            Sfx.Play(this, "ding", 0.8f);
            foreach (SmokeEmitter c in confetti) if (c != null) c.Play();
            Say("POP-tastic! Not one bubble left.", 0f);
            yield return new WaitForSeconds(1.2f);
            foreach (SmokeEmitter c in confetti) if (c != null) c.Stop();
            yield return new WaitForSeconds(1.0f);
            Win();
        }

        int said;

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            int id = ++said;
            if (seconds > 0f) StartCoroutine(HideLater(seconds, id));
        }

        IEnumerator HideLater(float seconds, int id)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done && id == said) bubble.Hide();
        }
    }
}
