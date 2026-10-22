using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The backward song" — un uccellino ha imparato la sua melodia di 4 note allo specchio e la canta al
    /// contrario (lo dice, e mentre canta le caselle della melodia si riempiono da DESTRA a sinistra). Bisogna ascoltarla
    /// (i tasti si illuminano) e suonare i quattro tasti in ordine INVERSO, senza fretta. Toccando l'uccellino la
    /// ricanta. Tre sequenze sbagliate = sconfitta.
    /// </summary>
    public class TheBackwardSongLevel : LevelController
    {
        [SerializeField] Transform bird, noteFx;
        [SerializeField] Collider2D birdArea;
        [SerializeField] Transform[] pads;
        [SerializeField] Collider2D[] padAreas;
        [SerializeField] SpriteRenderer[] padGlows, inputDots;
        [SerializeField] SpriteRenderer noteArt;
        [SerializeField] TextMesh missText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int missesAllowed = 3;

        static readonly float[] Hz = { 523f, 659f, 784f, 880f };
        static readonly Color[] PadColor = { new Color(1f, 0.42f, 0.42f), new Color(1f, 0.85f, 0.3f), new Color(0.45f, 0.85f, 0.4f), new Color(0.4f, 0.65f, 1f) };

        Camera cam;
        readonly int[] heard = new int[4];
        readonly int[] input = new int[4];
        int count, misses;
        bool busy;
        Vector3 birdHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            birdHome = bird.localPosition;
            // melodia casuale senza note ripetute di fila e non palindroma
            do
            {
                for (int i = 0; i < 4; i++)
                    do heard[i] = Random.Range(0, 4); while (i > 0 && heard[i] == heard[i - 1]);
            } while (heard[0] == heard[3] && heard[1] == heard[2]);
            foreach (SpriteRenderer g in padGlows) S41HKit.Alpha(g, 0f);
            noteFx.gameObject.SetActive(false);
            S41HKit.Confetti(confetti, false);
            UpdateTexts();
            StartCoroutine(Intro());
        }

        void UpdateTexts()
        {
            if (missText != null) missText.text = "WRONG TUNES " + misses + "/" + missesAllowed;
            for (int i = 0; i < inputDots.Length; i++)
                inputDots[i].color = i < count ? PadColor[input[i]] : new Color(1f, 1f, 1f, 0.35f);
        }

        IEnumerator Intro()
        {
            busy = true;
            S41HKit.Say(bubble, "Tweet! I learned my song in a mirror... so I sing it BACKWARDS!");
            yield return new WaitForSeconds(2.2f);
            yield return StartCoroutine(Sing(heard, false));
            S41HKit.Say(bubble, "Play it the RIGHT way round for me! (tap me to hear it again)");
            busy = false;
        }

        IEnumerator Sing(int[] seq, bool happy)
        {
            // cantando al contrario, le caselle della melodia si riempiono da destra a sinistra: si vede che va girata
            if (!happy) for (int d = 0; d < inputDots.Length; d++) inputDots[d].color = new Color(1f, 1f, 1f, 0.35f);
            for (int i = 0; i < seq.Length; i++)
            {
                if (!happy && inputDots.Length >= seq.Length) inputDots[inputDots.Length - 1 - i].color = PadColor[seq[i]];
                Light(seq[i], true);
                StartCoroutine(S41HKit.Move(bird, birdHome + Vector3.up * (happy ? 0.3f : 0.12f), 0.12f));
                yield return new WaitForSeconds(0.35f);
                bird.localPosition = birdHome;
                yield return new WaitForSeconds(0.25f);
            }
            if (!happy) { yield return new WaitForSeconds(0.8f); UpdateTexts(); }   // le caselle tornano per la risposta
        }

        /// <summary>Accende un tasto e fa partire la sua nota (e una notina dal becco se canta l'uccello).</summary>
        void Light(int pad, bool fromBird)
        {
            S41HKit.Note(this, Hz[pad], 0.55f, 0.4f);
            StartCoroutine(Glow(pad));
            if (fromBird) StartCoroutine(FlyNote(pad));
        }

        IEnumerator Glow(int pad)
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.45f)
            {
                S41HKit.Alpha(padGlows[pad], 1f - k);
                pads[pad].localScale = Vector3.one * (1f + Mathf.Sin(Mathf.Min(1f, k * 2.2f) * Mathf.PI) * 0.08f);
                yield return null;
            }
            S41HKit.Alpha(padGlows[pad], 0f);
            pads[pad].localScale = Vector3.one;
        }

        IEnumerator FlyNote(int pad)
        {
            noteFx.gameObject.SetActive(true);
            noteArt.color = PadColor[pad];
            Vector3 from = bird.position + new Vector3(0.55f, 0.25f, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                noteFx.position = from + new Vector3(k * 0.6f, k * 0.6f + Mathf.Sin(k * 9f) * 0.1f, 0f);
                S41HKit.Alpha(noteArt, 1f - k * k);
                yield return null;
            }
            noteFx.gameObject.SetActive(false);
        }

        void Update()
        {
            if (IsFinished || busy) return;
            foreach (S41HKit.Finger f in S41HKit.Fingers(cam))
            {
                if (!f.began) continue;
                if (birdArea.OverlapPoint(f.world)) { count = 0; UpdateTexts(); StartCoroutine(Replay()); return; }
                for (int p = 0; p < pads.Length; p++)
                {
                    if (!padAreas[p].OverlapPoint(f.world)) continue;
                    Light(p, false);
                    input[count++] = p;
                    UpdateTexts();
                    if (count == 4) StartCoroutine(Check());
                    return;
                }
            }
        }

        IEnumerator Replay()
        {
            busy = true;
            S41HKit.Say(bubble, "Again? Okay...");
            yield return StartCoroutine(Sing(heard, false));
            busy = false;
        }

        IEnumerator Check()
        {
            busy = true;
            yield return new WaitForSeconds(0.4f);
            bool reversed = true, same = true;
            for (int i = 0; i < 4; i++)
            {
                if (input[i] != heard[3 - i]) reversed = false;
                if (input[i] != heard[i]) same = false;
            }
            if (reversed)
            {
                Sfx.Play(this, "ding", 0.7f);
                S41HKit.Say(bubble, "THAT'S IT! That's my song!");
                yield return StartCoroutine(Sing(input, true));
                S41HKit.Confetti(confetti, true);
                for (float t = 0f; t < 2f; t += Time.deltaTime)
                {
                    bird.localPosition = birdHome + Vector3.up * Mathf.Abs(Mathf.Sin(t * 8f)) * 0.25f;
                    yield return null;
                }
                bird.localPosition = birdHome;
                S41HKit.Confetti(confetti, false);
                Win();
                yield break;
            }
            misses++;
            count = 0;
            UpdateTexts();
            Sfx.Play(this, "buzz", 0.5f);
            Haptics.Light();
            StartCoroutine(S41HKit.Shake(bird, 0.08f, 0.35f));
            S41HKit.Say(bubble, same ? "That's how I sang it... and it sounds WRONG!" : "Nope, that's not my song.");
            yield return new WaitForSeconds(1.4f);
            if (misses >= missesAllowed)
            {
                S41HKit.Say(bubble, "I give up. I'll just whistle.");
                yield return new WaitForSeconds(1.2f);
                Fail();
                yield break;
            }
            yield return StartCoroutine(Sing(heard, false));
            S41HKit.Say(bubble, misses == 1 ? "Maybe I sing it the wrong way round?" : "Start from my LAST note...");
            busy = false;
        }
    }
}
