using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The broken internet" — il computerino dice NO INTERNET: il cavo è tagliato in tre pezzi sparsi sulla
    /// scrivania. Soluzione: trascina i tre spezzoni nelle tre sedi tra computer e router, allineando i connettori
    /// con la stessa forma e colore (un tocco su uno spezzone lo gira), poi premi CONNECT. Tre configurazioni
    /// incompatibili = sconfitta. Tutto finto e offline.
    /// </summary>
    public class BrokenInternetLevel : LevelController
    {
        [SerializeField] Transform[] segments, slots;
        [SerializeField] SpriteRenderer[] cableArts;
        [SerializeField] Transform computer, connectButton, spark;
        [SerializeField] SpriteRenderer computerArt;
        [SerializeField] Sprite onlineSprite;
        [SerializeField] TextMesh screenText, triesText;
        [SerializeField] Collider2D connectArea;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int[] leftShape = { 0, 1, 2 }, rightShape = { 1, 2, 3 };
        [SerializeField] bool[] startFlipped = { false, true, false };
        [SerializeField] int computerShape = 0, routerShape = 3;
        [SerializeField] float snapRadius = 0.75f;

        Camera cam;
        int[] slotOf;
        bool[] flipped;
        int held = -1, wrong;
        Vector2 grabOffset, grabStart;
        bool busy;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            slotOf = new int[segments.Length];
            flipped = new bool[segments.Length];
            for (int i = 0; i < segments.Length; i++)
            {
                slotOf[i] = -1;
                flipped[i] = i < startFlipped.Length && startFlipped[i];
                segments[i].localScale = new Vector3(flipped[i] ? -1f : 1f, 1f, 1f);
            }
            spark.gameObject.SetActive(false);
            S41KKit.Confetti(confetti, false);
            UpdateTries();
            if (bubble != null) bubble.Show("NO INTERNET?! How will I watch cat videos?!");
        }

        void UpdateTries() { triesText.text = "BAD CONNECTIONS: " + wrong + "/3"; }

        int Left(int s) => flipped[s] ? rightShape[s] : leftShape[s];
        int Right(int s) => flipped[s] ? leftShape[s] : rightShape[s];

        void Update()
        {
            if (IsFinished || busy) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (held >= 0) Release(Vector2.zero, false); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (connectArea.OverlapPoint(w)) { StartCoroutine(Connect()); return; }
                for (int i = segments.Length - 1; i >= 0; i--)
                {
                    Vector2 d = w - (Vector2)segments[i].position;
                    if (Mathf.Abs(d.x) < 0.75f && Mathf.Abs(d.y) < 0.45f)
                    {
                        held = i;
                        grabOffset = (Vector2)segments[i].position - w;
                        grabStart = w;
                        slotOf[i] = -1;
                        Sfx.Play(this, "tick", 0.5f);
                        break;
                    }
                }
            }
            if (held >= 0)
            {
                Vector2 p = w + grabOffset;
                segments[held].position = new Vector3(Mathf.Clamp(p.x, -2.2f, 2.2f), Mathf.Clamp(p.y, -3.7f, -0.6f), 0f);
                if (ended) Release(w, true);
            }
        }

        void Release(Vector2 w, bool real)
        {
            int i = held;
            held = -1;
            if (real && Vector2.Distance(w, grabStart) < 0.15f) { StartCoroutine(FlipSeg(i)); }
            int best = -1;
            float bd = snapRadius;
            for (int s = 0; s < slots.Length; s++)
            {
                if (Occupied(s)) continue;
                float d = Vector2.Distance(segments[i].position, slots[s].position);
                if (d < bd) { bd = d; best = s; }
            }
            if (best < 0) return;
            slotOf[i] = best;
            segments[i].position = slots[best].position;
            Sfx.Play(this, "clunk", 0.5f);
        }

        bool Occupied(int s) { foreach (int o in slotOf) if (o == s) return true; return false; }

        IEnumerator FlipSeg(int i)
        {
            flipped[i] = !flipped[i];
            Sfx.Play(this, "whoosh", 0.4f);
            float to = flipped[i] ? -1f : 1f;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.2f)
            {
                segments[i].localScale = new Vector3(Mathf.Lerp(-to, to, t), 1f + 0.15f * Mathf.Sin(t * Mathf.PI), 1f);
                yield return null;
            }
            segments[i].localScale = new Vector3(to, 1f, 1f);
        }

        int SegAt(int slot) { for (int i = 0; i < slotOf.Length; i++) if (slotOf[i] == slot) return i; return -1; }

        IEnumerator Connect()
        {
            busy = true;
            StartCoroutine(S41KKit.Pop(connectButton, 1.2f, 0.2f));
            for (int s = 0; s < slots.Length; s++)
                if (SegAt(s) < 0)
                {
                    if (bubble != null) bubble.Show("The cable still has gaps! Put all three pieces in place.");
                    Sfx.Play(this, "tick", 0.5f);
                    yield return new WaitForSeconds(0.3f);
                    busy = false;
                    yield break;
                }
            // controlla ogni giunzione da sinistra a destra
            int bad = -1;
            int prev = computerShape;
            for (int s = 0; s <= slots.Length; s++)
            {
                int next = s < slots.Length ? Left(SegAt(s)) : routerShape;
                if (next != prev) { bad = s; break; }
                if (s < slots.Length) prev = Right(SegAt(s));
            }
            if (bad < 0) { yield return StartCoroutine(WinRoutine()); yield break; }

            wrong++;
            UpdateTries();
            Vector3 at = bad < slots.Length ? slots[bad].position + Vector3.left * 0.68f : slots[slots.Length - 1].position + Vector3.right * 0.68f;
            spark.position = at;
            spark.gameObject.SetActive(true);
            Sfx.Play(this, "buzz", 0.8f);
            Haptics.Medium();
            StartCoroutine(S41KKit.Shake(computer, 0.1f, 0.5f));
            yield return StartCoroutine(S41KKit.Pop(spark, 1.6f, 0.5f));
            spark.gameObject.SetActive(false);
            if (wrong >= 3)
            {
                screenText.text = "NO INTERNET\nEVER AGAIN";
                if (bubble != null) bubble.Show("*fizz* ...I'll just read a book. Ugh.");
                yield return new WaitForSeconds(1.5f);
                Fail();
                yield break;
            }
            if (bubble != null) bubble.Show(wrong == 1 ? "Zap! Those plugs don't match!" : "Same shape, same color. Tap a piece to turn it!");
            busy = false;
        }

        IEnumerator WinRoutine()
        {
            Sfx.Play(this, "clunk", 0.8f);
            Color lit = new Color(0.6f, 1f, 0.6f);
            foreach (SpriteRenderer c in cableArts) { c.color = lit; Sfx.Play(this, "tick", 0.6f); yield return new WaitForSeconds(0.2f); }
            computerArt.sprite = onlineSprite;
            screenText.text = "ONLINE!";
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Light();
            if (bubble != null) bubble.Show("WE'RE ONLINE! 3,000 new cat videos!");
            yield return StartCoroutine(S41KKit.Pop(computer, 1.15f, 0.4f));
            S41KKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.9f);
            S41KKit.Confetti(confetti, false);
            Win();
        }
    }
}
