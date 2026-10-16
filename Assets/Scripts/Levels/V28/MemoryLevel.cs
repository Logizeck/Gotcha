using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 95 — memory. Sedici carte coperte, otto coppie: girane due alla volta. Ma il gatto è un baro: ogni tre
    /// errori allunga la zampa e scambia due carte coperte (si vede, se guardi bene!). Al decimo errore, perso.
    /// </summary>
    public class MemoryLevel : LevelController
    {
        [SerializeField] Transform[] cards;           // figli "Back" e "Face" (con dentro l'icona)
        [SerializeField] Sprite[] icons;              // 8 icone
        [SerializeField] Transform paw;
        [SerializeField] TextMesh info;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int maxMistakes = 10, cheatEvery = 3;

        int[] value;
        bool[] open, matched;
        int first = -1, mistakes, pairs;
        bool busy, done;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            int n = cards.Length;
            value = new int[n];
            open = new bool[n];
            matched = new bool[n];
            var deck = new List<int>();
            for (int i = 0; i < n / 2; i++) { deck.Add(i); deck.Add(i); }
            for (int i = 0; i < n; i++) { int j = Random.Range(0, deck.Count); value[i] = deck[j]; deck.RemoveAt(j); }
            for (int i = 0; i < n; i++) { SetIcon(i); Show(i, false); }
            paw.gameObject.SetActive(false);
            Info();
            if (bubble != null) { bubble.Show("Find the pairs. And watch my paws..."); StartCoroutine(HideLater(2.2f)); }
        }

        void SetIcon(int i)
        {
            SpriteRenderer sr = cards[i].Find("Face/Icon").GetComponent<SpriteRenderer>();
            sr.sprite = icons[value[i]];
            Vector3 size = sr.sprite.bounds.size;
            sr.transform.localScale = Vector3.one * (0.62f / Mathf.Max(size.x, size.y));   // icone tutte della stessa misura
            sr.transform.localPosition = -sr.sprite.bounds.center * sr.transform.localScale.x;
        }

        void Show(int i, bool face)
        {
            open[i] = face;
            cards[i].Find("Back").gameObject.SetActive(!face);
            cards[i].Find("Face").gameObject.SetActive(face);
        }

        void Info() { info.text = "PAIRS " + pairs + "/" + cards.Length / 2 + "     MISTAKES " + mistakes + "/" + maxMistakes; }

        void Update()
        {
            if (done || busy || IsFinished) return;
            if (!PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            for (int i = 0; i < cards.Length; i++)
            {
                if (open[i] || matched[i] || Vector2.Distance(w, cards[i].position) > 0.5f) continue;
                StartCoroutine(Flip(i));
                return;
            }
        }

        IEnumerator Turn(int i, bool face)
        {
            Transform c = cards[i];
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.08f) { c.localScale = new Vector3(1f - k, 1f, 1f); yield return null; }
            Show(i, face);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.08f) { c.localScale = new Vector3(k, 1f, 1f); yield return null; }
            c.localScale = Vector3.one;
        }

        IEnumerator Flip(int i)
        {
            busy = true;
            Sfx.Play(this, "tick");
            yield return Turn(i, true);
            if (first < 0) { first = i; busy = false; yield break; }
            int a = first;
            first = -1;
            if (value[a] == value[i])
            {
                matched[a] = matched[i] = true;
                pairs++;
                Sfx.Play(this, "ding", 0.4f);
                Info();
                if (pairs >= cards.Length / 2) { StartCoroutine(Done()); yield break; }
                busy = false;
                yield break;
            }
            yield return new WaitForSeconds(0.7f);
            StartCoroutine(Turn(a, false));
            yield return Turn(i, false);
            mistakes++;
            Info();
            if (mistakes >= maxMistakes) { StartCoroutine(Lost()); yield break; }
            if (mistakes % cheatEvery == 0) yield return Cheat();
            busy = false;
        }

        /// <summary>La zampa del gatto scambia due carte coperte.</summary>
        IEnumerator Cheat()
        {
            var hidden = new List<int>();
            for (int i = 0; i < cards.Length; i++) if (!matched[i]) hidden.Add(i);
            if (hidden.Count < 4) yield break;
            int a = hidden[Random.Range(0, hidden.Count)], b;
            do b = hidden[Random.Range(0, hidden.Count)]; while (b == a);
            if (bubble != null) { bubble.Show("Hehe... *swap*"); StartCoroutine(HideLater(1.2f)); }
            paw.gameObject.SetActive(true);
            Vector3 pa = cards[a].position, pb = cards[b].position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { paw.position = Vector3.Lerp(new Vector3(pa.x, 4.5f, 0), pa, k); yield return null; }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                cards[a].position = Vector3.Lerp(pa, pb, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.5f;
                cards[b].position = Vector3.Lerp(pb, pa, k) - Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.5f;
                paw.position = cards[a].position;
                yield return null;
            }
            // le carte tornano al loro posto, ma con il valore scambiato: per chi guarda, si sono scambiate
            cards[a].position = pa;
            cards[b].position = pb;
            (value[a], value[b]) = (value[b], value[a]);
            SetIcon(a);
            SetIcon(b);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { paw.position = Vector3.Lerp(pa, new Vector3(pa.x, 4.5f, 0), k); yield return null; }
            paw.gameObject.SetActive(false);
        }

        IEnumerator Done()
        {
            done = true;
            if (bubble != null) bubble.Show("Hmph. You were watching.");
            yield return new WaitForSeconds(1.4f);
            Win();
        }

        IEnumerator Lost()
        {
            done = true;
            if (bubble != null) bubble.Show("Ten mistakes. The cat wins!");
            yield return new WaitForSeconds(1.4f);
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
