using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 28 — "taglia il filo del colore del cielo". Tre fili (rosso, verde, nero, in ordine casuale) collegano
    /// la dinamite al timer. Si tagliano col dito (swipe). Il trucco: fuori dalla finestra è notte, quindi il filo
    /// giusto è il NERO (di giorno sarebbe azzurro, ma un filo azzurro non c'è). Un taglio sbagliato, o il tempo che scade: BOOM.
    /// </summary>
    public class SkyWireLevel : LevelController
    {
        [SerializeField] SwipeCutter[] cutters;      // uno per posizione (sinistra, centro, destra)
        [SerializeField] Transform[] tops;           // metà superiori (pivot in alto)
        [SerializeField] Transform[] bottoms;        // metà inferiori (pivot in basso)
        [SerializeField] SpriteRenderer[] topArt;
        [SerializeField] SpriteRenderer[] bottomArt;
        [SerializeField] TextMesh display;
        [SerializeField] GameObject bomb;
        [SerializeField] GameObject boom;
        [SerializeField] SmokeEmitter sparks;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float seconds = 15f;

        static readonly Color[] WireColors = { new Color(0.9f, 0.2f, 0.22f), new Color(0.25f, 0.78f, 0.3f), new Color(0.13f, 0.11f, 0.15f) };   // rosso, verde, nero
        const int Black = 2;

        int[] colorAt = new int[3];
        float left;
        bool over;

        public override void OnLevelStart()
        {
            left = seconds;
            // ordine dei colori casuale
            var order = new System.Collections.Generic.List<int> { 0, 1, 2 };
            for (int i = 0; i < 3; i++)
            {
                int j = Random.Range(0, order.Count);
                colorAt[i] = order[j];
                order.RemoveAt(j);
                Color c = WireColors[colorAt[i]];
                if (topArt[i] != null) topArt[i].color = c;
                if (bottomArt[i] != null) bottomArt[i].color = c;
                int index = i;
                cutters[i].Cut += (a, b) => OnCut(index);
            }
            if (boom != null) boom.SetActive(false);
            if (bubble != null) bubble.Hide();
        }

        void Update()
        {
            if (over || IsFinished) return;
            left -= Time.deltaTime;
            if (display != null)
            {
                display.text = "00:" + Mathf.CeilToInt(Mathf.Max(0f, left)).ToString("00");
                display.color = left < 5f && Mathf.Repeat(Time.time * 3f, 1f) < 0.5f ? new Color(1f, 0.3f, 0.3f) : new Color(0.36f, 1f, 0.6f);
            }
            if (left <= 0f) StartCoroutine(Boom());
        }

        void OnCut(int i)
        {
            if (over || IsFinished) return;
            StartCoroutine(Droop(i));
            if (sparks != null) { sparks.transform.position = cutters[i].transform.position; StartCoroutine(Burst()); }
            if (colorAt[i] == Black) StartCoroutine(Defused());
            else StartCoroutine(Boom());
        }

        IEnumerator Burst()
        {
            sparks.Play();
            yield return new WaitForSeconds(0.15f);
            sparks.Stop();
        }

        IEnumerator Droop(int i)
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                float e = Mathf.SmoothStep(0f, 1f, k);
                if (tops[i] != null) { tops[i].localRotation = Quaternion.Euler(0, 0, 18f * e); tops[i].localScale = new Vector3(1f, 1f - 0.15f * e, 1f); }
                if (bottoms[i] != null) { bottoms[i].localRotation = Quaternion.Euler(0, 0, -22f * e); bottoms[i].localScale = new Vector3(1f, 1f - 0.2f * e, 1f); }
                yield return null;
            }
        }

        IEnumerator Defused()
        {
            over = true;
            if (display != null) { display.text = "--:--"; display.color = new Color(0.36f, 1f, 0.6f); }
            if (bubble != null) bubble.Show("Phew! The sky IS black at night.");
            yield return new WaitForSeconds(1.8f);
            Win();
        }

        IEnumerator Boom()
        {
            over = true;
            yield return new WaitForSeconds(0.25f);
            if (bomb != null) bomb.SetActive(false);
            if (boom != null)
            {
                boom.SetActive(true);
                Transform t = boom.transform;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { t.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.2f, k); yield return null; }
            }
            if (bubble != null) bubble.Show("BOOM!");
            yield return new WaitForSeconds(1f);
            Fail();
        }
    }
}
