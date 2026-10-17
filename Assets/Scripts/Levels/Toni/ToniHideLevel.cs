using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 75 — "WHERE'S TONI?". Nascondino: Toni si è nascosto (dietro il divano, nello scatolone, nel cesto
    /// del bucato o dietro la tenda) e si è pure addormentato. Non si vede, ma si SENTE: muovendo il dito per la
    /// stanza il russare è più forte e più fitto vicino al nascondiglio (e il telefono vibra), e il nascondiglio
    /// giusto respira piano. Ogni nascondiglio sbagliato ha la sua sorpresa, e dopo ogni errore Toni può cambiare
    /// posto. 4 errori e vince lui.
    /// </summary>
    public class ToniHideLevel : LevelController
    {
        [SerializeField] ToniPug toni;
        [SerializeField] TapButton[] spots;
        [SerializeField] Transform[] spotArt;
        [SerializeField] GameObject[] gags;          // la sorpresa di ogni nascondiglio sbagliato
        [SerializeField] Vector2[] toniAt;           // dove spunta Toni da ogni nascondiglio (coordinate del livello)
        [SerializeField] SpeechBubble bubble;
        [SerializeField] string[] gagLines = { "Just socks...", "Squeak!", "HSSSS!", "Bzzz!" };
        [SerializeField] int maxWrong = 4;
        [SerializeField] float hearRange = 4.5f;

        int hideAt, wrong;
        bool busy;
        float nextSnore;
        Camera cam;
        Vector3[] artPos, artScale;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            hideAt = Random.Range(0, spots.Length);
            toni.gameObject.SetActive(false);
            artPos = new Vector3[spotArt.Length];
            artScale = new Vector3[spotArt.Length];
            for (int i = 0; i < spotArt.Length; i++) { artPos[i] = spotArt[i].localPosition; artScale[i] = spotArt[i].localScale; }
            foreach (GameObject g in gags) if (g != null) g.SetActive(false);
            foreach (TapButton b in spots) b.Pressed += OnSpot;
            if (bubble != null) { bubble.Show("Ready or not... here I come!"); StartCoroutine(HideBubble(2.2f)); }
            nextSnore = Time.time + 2.5f;
        }

        void OnDestroy() { if (spots != null) foreach (TapButton b in spots) if (b != null) b.Pressed -= OnSpot; }

        void Update()
        {
            if (IsFinished || busy) return;
            // il nascondiglio giusto "respira" (il russare di Toni lo fa muovere appena)
            Transform a = spotArt[hideAt];
            a.localScale = new Vector3(artScale[hideAt].x, artScale[hideAt].y * (1f + Mathf.Max(0f, Mathf.Sin(Time.time * 2.2f)) * 0.025f), artScale[hideAt].z);

            float d = hearRange;
            Vector2 sp = Vector2.zero;
            bool finger = cam != null && PointerInput.TryGetPrimary(out sp, out _, out _);
            if (finger) d = Vector2.Distance(PointerInput.ToWorld(cam, sp), spots[hideAt].transform.position);
            float closeness = 1f - Mathf.Clamp01(d / hearRange);
            if (Time.time >= nextSnore)
            {
                // più sei vicino, più il russare è forte e fitto (senza dito: un russare lontano ogni tanto)
                nextSnore = Time.time + (finger ? Mathf.Lerp(1.1f, 0.22f, closeness) : 3.5f);
                Sfx.Play(this, "snore", finger ? Mathf.Lerp(0.12f, 1f, closeness) : 0.15f);
                if (finger && closeness > 0.65f) Haptics.Light();
            }
        }

        void OnSpot(TapButton b)
        {
            if (busy || IsFinished) return;
            int i = System.Array.IndexOf(spots, b);
            if (i < 0) return;
            StartCoroutine(i == hideAt ? Found(i) : Wrong(i));
        }

        IEnumerator Lift(int i, bool up)
        {
            Transform a = spotArt[i];
            Vector3 lifted = artPos[i] + new Vector3(0.35f, 0.55f, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f)
            {
                float t = up ? k : 1f - k;
                a.localPosition = Vector3.Lerp(artPos[i], lifted, t);
                a.localRotation = Quaternion.Euler(0, 0, -14f * t);
                yield return null;
            }
            a.localPosition = up ? lifted : artPos[i];
            a.localRotation = Quaternion.Euler(0, 0, up ? -14f : 0f);
            a.localScale = artScale[i];
        }

        IEnumerator Wrong(int i)
        {
            busy = true;
            wrong++;
            Sfx.Play(this, "whoosh", 0.5f);
            yield return Lift(i, true);
            if (gags[i] != null) gags[i].SetActive(true);
            if (bubble != null) bubble.Show(gagLines[Mathf.Min(i, gagLines.Length - 1)]);
            Sfx.Play(this, "buzz", 0.35f);
            yield return new WaitForSeconds(1f);
            if (gags[i] != null) gags[i].SetActive(false);
            yield return Lift(i, false);
            if (wrong >= maxWrong) { yield return Found(hideAt, false); yield break; }
            // Toni ridacchia e forse cambia nascondiglio
            if (bubble != null) bubble.Show(wrong == maxWrong - 1 ? "*snicker* Last chance!" : "*snicker*");
            if (Random.value < 0.5f)
            {
                int to = Random.Range(0, spots.Length - 1);
                if (to >= hideAt) to++;
                if (to == i) to = hideAt;   // non si nasconde proprio dove hai appena guardato
                spotArt[hideAt].localScale = artScale[hideAt];
                hideAt = to;
                Sfx.Play(this, "tick", 0.4f);
            }
            yield return new WaitForSeconds(0.8f);
            if (bubble != null) bubble.Hide();
            busy = false;
        }

        IEnumerator Found(int i, bool won = true)
        {
            busy = true;
            yield return Lift(i, true);
            toni.gameObject.SetActive(true);
            toni.transform.localPosition = toniAt[i];
            toni.GroundY = toniAt[i].y;
            toni.Stand(false);
            toni.WakeUp(2);
            toni.Wag = 1f;
            yield return toni.Jump(0.6f, 0.4f);
            if (won)
            {
                toni.Bark("PEEKABOO!");
                yield return toni.Happy(1.2f);
                Sfx.Play(this, "ding", 0.8f);
                yield return new WaitForSeconds(0.8f);
                Win();
            }
            else
            {
                toni.Say("Hehe! I win!", 1.6f, "woof");
                yield return new WaitForSeconds(1.8f);
                Fail();
            }
        }

        IEnumerator HideBubble(float s)
        {
            yield return new WaitForSeconds(s);
            if (!busy && bubble != null) bubble.Hide();
        }
    }
}
