using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Broken vase" — il vaso cade dalla mensola e va in cinque cocci, ognuno girato di un multiplo di 90°.
    /// Rimettili nella sagoma tratteggiata prima che arrivi la mamma: trascina un coccio per spostarlo, toccalo (senza
    /// trascinare) per girarlo di 90°. Si incastra solo se è vicino al suo posto E dritto. Il trucco: un coccio è
    /// scivolato sotto il gatto che dorme — tocca il gatto per farlo rotolare via.
    /// </summary>
    public class VaseLevel : LevelController
    {
        [SerializeField] Transform vase;              // vaso intero sulla mensola
        [SerializeField] Transform[] shards;          // cocci (con Collider2D)
        [SerializeField] Transform[] targets;         // posto giusto di ogni coccio
        [SerializeField] Transform[] landings;        // dove finisce ogni coccio dopo la caduta
        [SerializeField] int hiddenShard = 2;         // quello sotto il gatto
        [SerializeField] Transform cat;
        [SerializeField] Collider2D catArea;
        [SerializeField] Transform impact;            // dove il vaso tocca terra
        [SerializeField] SpriteRenderer outline, glue;
        [SerializeField] Transform mom;
        [SerializeField] Transform momStop;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float snapDistance = 0.35f, dragStart = 0.15f;

        int[] rot;
        bool[] placed;
        SpriteRenderer[] arts;
        Collider2D[] cols;
        Camera cam;
        bool ready, busy, catMoved, hiddenRevealed;
        int grabbed = -1, wrongTurns;
        Vector2 grabOffset, grabStart;
        bool dragging;
        float bubbleUntil, clock;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            int n = shards.Length;
            rot = new int[n];
            placed = new bool[n];
            arts = new SpriteRenderer[n];
            cols = new Collider2D[n];
            for (int i = 0; i < n; i++)
            {
                arts[i] = shards[i].GetComponentInChildren<SpriteRenderer>();
                cols[i] = shards[i].GetComponent<Collider2D>();
                shards[i].gameObject.SetActive(false);
                rot[i] = Random.Range(1, 4);   // tutti storti
            }
            if (glue != null) glue.color = new Color(1f, 1f, 1f, 0f);
            StartCoroutine(Fall());
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        IEnumerator Fall()
        {
            yield return new WaitForSeconds(0.6f);
            // vacilla sulla mensola...
            for (float k = 0f; k < 0.8f; k += Time.deltaTime) { vase.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 22f) * 8f * k); yield return null; }
            // ...e cade
            Vector3 p = vase.position;
            float vy = 0f, vx = 0.8f, spin = 0f;
            while (p.y > impact.position.y)
            {
                vy -= 18f * Time.deltaTime;
                p += new Vector3(vx, vy, 0f) * Time.deltaTime;
                spin -= 220f * Time.deltaTime;
                vase.position = p;
                vase.localRotation = Quaternion.Euler(0, 0, spin);
                yield return null;
            }
            vase.gameObject.SetActive(false);
            Haptics.Heavy();
            Sfx.Play(this, "clunk", 1f);
            Sfx.Play(this, "pop", 0.8f);
            Sfx.Play(this, "buzz", 0.3f);
            Vector3 from = new Vector3(p.x, impact.position.y, 0f);
            for (int i = 0; i < shards.Length; i++)
            {
                shards[i].gameObject.SetActive(true);
                shards[i].position = from;
                if (cols[i] != null) cols[i].enabled = i != hiddenShard;
            }
            float[] spins = new float[shards.Length];
            for (int i = 0; i < spins.Length; i++) spins[i] = (Random.value < 0.5f ? -1f : 1f) * 360f * Random.Range(1, 3);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                for (int i = 0; i < shards.Length; i++)
                {
                    Vector3 to = landings[i].position;
                    shards[i].position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.2f;
                    shards[i].localRotation = Quaternion.Euler(0, 0, rot[i] * 90f + spins[i] * (1f - k));
                }
                yield return null;
            }
            for (int i = 0; i < shards.Length; i++) { shards[i].position = landings[i].position; SetRot(i); }
            Sfx.Play(this, "tick", 0.8f);
            Say("Uh-oh. Fix it before Mom sees!", 2.4f);
            ready = true;
        }

        void SetRot(int i) { shards[i].localRotation = Quaternion.Euler(0, 0, rot[i] * 90f); }

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil && !busy) { bubble.Hide(); bubbleUntil = 0f; }
            if (!ready || busy) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (grabbed >= 0) Release(); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (PointerInput.IsOverUi()) return;
                grabbed = Pick(w);
                if (grabbed >= 0)
                {
                    grabOffset = (Vector2)shards[grabbed].position - w;
                    grabStart = w;
                    dragging = false;
                    arts[grabbed].sortingOrder = 30;
                    return;
                }
                if (catArea != null && catArea.OverlapPoint(w)) TapCat();
                return;
            }
            if (grabbed < 0) return;
            if (!dragging && (w - grabStart).magnitude > dragStart) dragging = true;
            if (dragging)
            {
                Vector2 p = w + grabOffset;
                p.x = Mathf.Clamp(p.x, -2.6f, 2.6f);
                p.y = Mathf.Clamp(p.y, -4.4f, 3.4f);
                shards[grabbed].position = new Vector3(p.x, p.y, shards[grabbed].position.z);
            }
            if (ended) Release();
        }

        int Pick(Vector2 w)
        {
            int best = -1;
            float bestD = float.MaxValue;
            for (int i = 0; i < shards.Length; i++)
            {
                if (placed[i] || cols[i] == null || !cols[i].enabled || !cols[i].OverlapPoint(w)) continue;
                float d = Vector2.Distance(w, shards[i].position);
                if (d < bestD) { bestD = d; best = i; }
            }
            return best;
        }

        void Release()
        {
            int i = grabbed;
            grabbed = -1;
            arts[i].sortingOrder = 6;
            if (!dragging)
            {
                rot[i] = (rot[i] + 3) % 4;   // un quarto di giro in senso orario
                Sfx.Play(this, "tick", 0.7f);
                StartCoroutine(Turn(i));
                return;
            }
            float d = Vector2.Distance(shards[i].position, targets[i].position);
            if (d <= snapDistance && rot[i] == 0) { StartCoroutine(Snap(i)); return; }
            if (d <= snapDistance * 2.2f)
            {
                // vicino ma storto (o quasi): rimbalza fuori
                Sfx.Play(this, "buzz", 0.5f);
                if (rot[i] != 0)
                {
                    wrongTurns++;
                    Say(wrongTurns <= 2 ? "Wrong way round! Tap it to turn it." : "Still crooked...", 1.8f);
                }
                StartCoroutine(BounceOut(i));
            }
        }

        IEnumerator Turn(int i)
        {
            Quaternion from = shards[i].localRotation, to = Quaternion.Euler(0, 0, rot[i] * 90f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.12f) { shards[i].localRotation = Quaternion.Slerp(from, to, k); yield return null; }
            SetRot(i);
        }

        IEnumerator BounceOut(int i)
        {
            Vector3 from = shards[i].position;
            Vector2 dir = (Vector2)(from - targets[i].position);
            if (dir.sqrMagnitude < 0.01f) dir = Vector2.down;
            Vector3 to = from + (Vector3)(dir.normalized * 0.9f);
            to.x = Mathf.Clamp(to.x, -2.5f, 2.5f);
            to.y = Mathf.Clamp(to.y, -4.3f, 3.3f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                shards[i].position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.25f;
                yield return null;
            }
            shards[i].position = to;
        }

        IEnumerator Snap(int i)
        {
            placed[i] = true;
            arts[i].sortingOrder = 4;
            Vector3 from = shards[i].position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.12f) { shards[i].position = Vector3.Lerp(from, targets[i].position, k); yield return null; }
            shards[i].position = targets[i].position;
            Sfx.Play(this, "clunk", 0.6f);
            Haptics.Light();
            foreach (bool b in placed) if (!b) yield break;
            StartCoroutine(Done());
        }

        void TapCat()
        {
            if (catMoved) { Sfx.Play(this, "snore", 0.5f); Say("Mrrr... zzz.", 1.2f); return; }
            StartCoroutine(RollCat());
        }

        IEnumerator RollCat()
        {
            busy = true;
            catMoved = true;
            Sfx.Play(this, "whoosh", 0.6f);
            Vector3 from = cat.localPosition, to = from + new Vector3(1.3f, 0f, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                cat.localPosition = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.3f;
                cat.localRotation = Quaternion.Euler(0, 0, -360f * k);
                yield return null;
            }
            cat.localPosition = to;
            cat.localRotation = Quaternion.identity;
            Say("Mrrrp. (still asleep)", 1.6f);
            // il coccio nascosto salta fuori
            if (!hiddenRevealed && hiddenShard >= 0 && hiddenShard < shards.Length)
            {
                hiddenRevealed = true;
                arts[hiddenShard].sortingOrder = 6;
                if (cols[hiddenShard] != null) cols[hiddenShard].enabled = true;
                Sfx.Play(this, "pop", 0.7f);
                Transform s = shards[hiddenShard];
                Vector3 p = s.position;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { s.position = p + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.3f; yield return null; }
                s.position = p;
            }
            busy = false;
        }

        IEnumerator Done()
        {
            busy = true;
            if (bubble != null) bubble.Hide();
            Sfx.Play(this, "ding", 0.8f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                if (outline != null) outline.color = new Color(1f, 1f, 1f, 1f - k);
                if (glue != null) glue.color = new Color(1f, 1f, 1f, k);
                yield return null;
            }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                if (glue != null) glue.color = new Color(1f, 1f, 1f, 0.7f + Mathf.Sin(k * Mathf.PI * 4f) * 0.3f);
                yield return null;
            }
            if (glue != null) glue.color = Color.white;
            // arriva la mamma... e non si accorge di niente
            if (mom != null && momStop != null)
            {
                Vector3 from = mom.position;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f) { mom.position = Vector3.Lerp(from, momStop.position, 1f - (1f - k) * (1f - k)); yield return null; }
            }
            if (bubble != null) bubble.Show("Ooh, gold trim! Was it always that fancy?");
            yield return new WaitForSeconds(2.2f);
            Win();
        }
    }
}
