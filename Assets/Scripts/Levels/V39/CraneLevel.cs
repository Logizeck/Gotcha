using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Sky Crane — un gatto è bloccato sul tetto. La gru fa oscillare una cassa: tocca per sganciarla sulla pila.
    /// La parte che sporge dalla cassa sotto viene tagliata e cade; un lancio "PERFECT!" (entro 0.08) non accorcia
    /// la cassa. Sei casse arrivano al tetto e il gatto scende dalla torre. Una cassa che manca la pila costa una
    /// delle 3 vite. Soluzione: toccare quando la cassa è proprio sopra la pila (l'oscillazione accelera).
    /// </summary>
    public class CraneLevel : LevelController
    {
        [SerializeField] SpriteRenderer crateTemplate;   // 9-slice, nascosto
        [SerializeField] Transform hook;
        [SerializeField] LineRenderer rope;
        [SerializeField] Transform stack;
        [SerializeField] Transform cat;
        [SerializeField] TextMesh perfectLabel;
        [SerializeField] TextMesh counter;
        [SerializeField] Transform[] hearts;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float stackX = -0.5f, baseTop = -3.6f, crateH = 0.72f, startWidth = 1.5f;
        [SerializeField] float hangY = 2.3f, jibY = 3.4f, amplitude = 1.45f, omega = 1.7f, speedUp = 1.1f, perfectTol = 0.08f;

        const int Needed = 6;
        SpriteRenderer hanging;
        float phase, width, prevL, prevR, topY;
        int placed, lives = 3, perfects;
        bool busy, done;

        public override void OnLevelStart()
        {
            crateTemplate.gameObject.SetActive(false);
            perfectLabel.gameObject.SetActive(false);
            width = startWidth;
            prevL = stackX - startWidth * 0.5f;
            prevR = stackX + startWidth * 0.5f;
            topY = baseTop;
            phase = Random.Range(0f, Mathf.PI * 2f);
            UpdateCounter();
            NewCrate();
            if (bubble != null) { bubble.Show("Help! Stack 6 crates up to me!"); StartCoroutine(HideLater(2.5f)); }
        }

        void UpdateCounter() { counter.text = placed + " / " + Needed; }

        void NewCrate()
        {
            GameObject go = Instantiate(crateTemplate.gameObject, stack);
            go.SetActive(true);
            hanging = go.GetComponent<SpriteRenderer>();
            hanging.size = new Vector2(width, crateH);
            hanging.sortingOrder = 6;
            Swing();
        }

        void Swing()
        {
            float x = stackX + amplitude * Mathf.Sin(phase);
            if (hanging != null) hanging.transform.localPosition = new Vector3(x, hangY, 0f);
            float top = hangY + crateH * 0.5f;
            hook.localPosition = new Vector3(x, top, 0f);
            rope.SetPosition(0, transform.TransformPoint(new Vector3(x, jibY, 0f)));
            rope.SetPosition(1, transform.TransformPoint(new Vector3(x, top + 0.5f, 0f)));
        }

        void Update()
        {
            if (done || IsFinished) return;
            phase += omega * Time.deltaTime;
            if (!busy) Swing();
            if (busy || hanging == null) return;
            if (PointerInput.AnyTouchBegan() && !PointerInput.IsOverUi()) StartCoroutine(Drop());
        }

        IEnumerator Drop()
        {
            busy = true;
            SpriteRenderer c = hanging;
            hanging = null;
            Sfx.Play(this, "tick", 0.6f);
            float x = c.transform.localPosition.x, y = hangY, v = 0f, landY = topY + crateH * 0.5f;
            float prevC = (prevL + prevR) * 0.5f;
            float l = x - width * 0.5f, r = x + width * 0.5f;
            float ol = Mathf.Max(l, prevL), or = Mathf.Min(r, prevR);
            bool miss = or - ol <= 0.02f && Mathf.Abs(x - prevC) > perfectTol;
            if (miss) landY = baseTop + crateH * 0.5f;   // niente sotto: arriva fino a terra
            while (y > landY)
            {
                v += 16f * Time.deltaTime;
                y = Mathf.Max(landY, y - v * Time.deltaTime);
                c.transform.localPosition = new Vector3(x, y, 0f);
                yield return null;
            }
            if (Mathf.Abs(x - prevC) <= perfectTol)
            {
                // lancio perfetto: si allinea e non si accorcia
                c.transform.localPosition = new Vector3(prevC, landY, 0f);
                perfects++;
                Haptics.Light();
                Sfx.Play(this, "ding", 0.5f);
                StartCoroutine(Perfect(prevC, landY + 0.7f));
                Land(prevL, prevR);
            }
            else if (miss)
            {
                // mancata: la cassa è caduta a terra e rotola via
                Haptics.Medium();
                Sfx.Play(this, "bonk", 0.8f);
                yield return Fall(c.transform, x < prevC ? -1f : 1f);
                lives--;
                if (lives >= 0 && lives < hearts.Length) hearts[lives].gameObject.SetActive(false);
                if (lives <= 0) { StartCoroutine(Lost()); yield break; }
                if (bubble != null) { bubble.Show(lives == 1 ? "Last crate... careful!" : "Missed! Try again.", true); StartCoroutine(HideLater(1.5f)); }
            }
            else
            {
                // taglia la parte che sporge
                float cutL = l < ol ? l : or, cutR = l < ol ? ol : r;
                c.size = new Vector2(or - ol, crateH);
                c.transform.localPosition = new Vector3((ol + or) * 0.5f, landY, 0f);
                GameObject piece = Instantiate(crateTemplate.gameObject, stack);
                piece.SetActive(true);
                SpriteRenderer ps = piece.GetComponent<SpriteRenderer>();
                ps.size = new Vector2(cutR - cutL, crateH);
                ps.sortingOrder = 7;
                piece.transform.localPosition = new Vector3((cutL + cutR) * 0.5f, landY, 0f);
                StartCoroutine(Fall(piece.transform, l < ol ? -1f : 1f));
                Haptics.Medium();
                Sfx.Play(this, "clunk", 0.7f);
                width = or - ol;
                Land(ol, or);
            }
            if (placed >= Needed) { StartCoroutine(Rescue()); yield break; }
            yield return new WaitForSeconds(0.35f);
            phase = Random.Range(0f, Mathf.PI * 2f);
            NewCrate();
            busy = false;
        }

        void Land(float l, float r)
        {
            prevL = l;
            prevR = r;
            topY += crateH;
            placed++;
            omega *= speedUp;
            UpdateCounter();
        }

        IEnumerator Fall(Transform t, float side)
        {
            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            Vector3 vel = new Vector3(side * 1.2f, 0.5f, 0f);
            float spin = side * -220f;
            for (float k = 0f; k < 1.4f; k += Time.deltaTime)
            {
                vel.y -= 14f * Time.deltaTime;
                t.localPosition += vel * Time.deltaTime;
                t.Rotate(0, 0, spin * Time.deltaTime);
                if (sr != null && k > 0.7f) { Color col = sr.color; col.a = Mathf.Clamp01((1.4f - k) / 0.7f); sr.color = col; }
                yield return null;
            }
            Destroy(t.gameObject);
        }

        IEnumerator Perfect(float x, float y)
        {
            perfectLabel.transform.localPosition = new Vector3(x, y, 0f);
            perfectLabel.gameObject.SetActive(true);
            for (float k = 0f; k < 0.8f; k += Time.deltaTime)
            {
                perfectLabel.transform.localScale = Vector3.one * (1f + Mathf.Sin(Mathf.Min(1f, k / 0.2f) * Mathf.PI) * 0.3f);
                perfectLabel.transform.localPosition = new Vector3(x, y + k * 0.4f, 0f);
                yield return null;
            }
            perfectLabel.gameObject.SetActive(false);
        }

        IEnumerator Rescue()
        {
            done = true;
            hook.gameObject.SetActive(false);
            rope.enabled = false;
            Sfx.Play(this, "ding", 0.8f);
            if (bubble != null) bubble.Show("My hero!");
            yield return new WaitForSeconds(0.6f);
            if (bubble != null) bubble.Hide();
            float cx = (prevL + prevR) * 0.5f;
            // salto sulla cima della torre, poi giù cassa per cassa, poi a terra
            yield return Hop(new Vector3(cx, topY, 0f), 0.45f, 0.6f);
            for (int i = 1; i < Needed; i++) yield return Hop(new Vector3(cx + (i % 2 == 0 ? 0.1f : -0.1f), topY - i * crateH, 0f), 0.25f, 0.25f);
            yield return Hop(new Vector3(stackX + 1.3f, baseTop, 0f), 0.4f, 0.4f);
            if (bubble != null) { bubble.transform.localPosition = new Vector3(0.6f, -1.6f, 0f); bubble.Show("Thanks, builder!"); }
            yield return new WaitForSeconds(1.3f);
            if (perfects >= Needed) WinWithEgg("crane_perfect");
            else Win();
        }

        IEnumerator Hop(Vector3 to, float height, float seconds)
        {
            Vector3 from = cat.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                cat.localPosition = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * height;
                yield return null;
            }
            cat.localPosition = to;
            Sfx.Play(this, "tick", 0.5f);
        }

        IEnumerator Lost()
        {
            done = true;
            hook.gameObject.SetActive(false);
            if (bubble != null) bubble.Show("I'll just live up here now.");
            yield return new WaitForSeconds(1.5f);
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
