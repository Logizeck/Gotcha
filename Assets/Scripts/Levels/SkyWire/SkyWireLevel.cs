using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 28 — il filo giusto. Tre fili di colori presi a caso collegano la dinamite al timer; il biglietto dice
    /// con un indovinello quale tagliare (anche quello scelto a caso: "il colore del cielo a mezzanotte" = nero,
    /// "la pancia di un pupazzo di neve" = bianco...). Si tagliano col dito (swipe). Sbagliato, o tempo scaduto: BOOM.
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

        [SerializeField] TextMesh note;

        // rosso, verde, nero, blu, giallo, bianco
        static readonly Color[] WireColors =
        {
            new Color(0.9f, 0.2f, 0.22f), new Color(0.25f, 0.78f, 0.3f), new Color(0.13f, 0.11f, 0.15f),
            new Color(0.25f, 0.45f, 0.95f), new Color(1f, 0.85f, 0.2f), new Color(0.96f, 0.96f, 0.96f),
        };
        // indovinelli "sibillini", due per colore
        static readonly string[][] Riddles =
        {
            new[] { "CUT THE WIRE\nAS SHY AS\nA RIPE TOMATO", "CUT THE WIRE\nTHE COLOR\nOF A STOP SIGN" },
            new[] { "CUT THE WIRE\nOF GRASS\nAND ENVY", "CUT THE WIRE\nA FROG\nWOULD WEAR" },
            new[] { "CUT THE WIRE\nTHE COLOR OF\nTHE SKY TONIGHT", "CUT THE WIRE\nOF A CAT\nNOBODY SEES" },
            new[] { "CUT THE WIRE\nOF THE SEA\nAT NOON", "CUT THE WIRE\nTHE COLOR\nOF SAD MUSIC" },
            new[] { "CUT THE WIRE\nOF A BANANA'S\nJACKET", "CUT THE WIRE\nTHE SUN\nWOULD PICK" },
            new[] { "CUT THE WIRE\nOF A SNOWMAN'S\nBELLY", "CUT THE WIRE\nAS PALE AS\nA GHOST" },
        };

        int[] colorAt = new int[3];
        int target;
        float left;
        bool over;

        public override void OnLevelStart()
        {
            left = seconds;
            // tre colori a caso (tutti diversi) e uno di loro è quello giusto
            var order = new System.Collections.Generic.List<int> { 0, 1, 2, 3, 4, 5 };
            for (int i = 0; i < 3; i++)
            {
                int j = Random.Range(0, order.Count);
                colorAt[i] = order[j];
                order.RemoveAt(j);
            }
            target = colorAt[Random.Range(0, 3)];
            if (note != null) note.text = Riddles[target][Random.Range(0, 2)];
            for (int i = 0; i < 3; i++)
            {
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
            if (colorAt[i] == target) StartCoroutine(Defused());
            else StartCoroutine(Boom());
        }

        IEnumerator Burst()
        {
            Haptics.Light();   // snip
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
            if (bubble != null) bubble.Show("Phew! Riddle solved.");
            yield return new WaitForSeconds(1.8f);
            Win();
        }

        IEnumerator Boom()
        {
            over = true;
            yield return new WaitForSeconds(0.25f);
            Haptics.Heavy();
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
