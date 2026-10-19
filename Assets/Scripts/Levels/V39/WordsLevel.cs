using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Word salad" — tre indovinelli da risolvere con le tessere delle lettere. Tocca una tessera per mandarla
    /// nella prima casella libera, tocca una lettera in una casella per rimandarla indietro; a caselle piene la parola
    /// si controlla da sola. Due indovinelli a caso da una lista di otto; il terzo è sempre "What word is always spelled
    /// WRONG?" — e la risposta è proprio WRONG (le tessere permettono anche RIGHT, che ovviamente è... sbagliato).
    /// </summary>
    public class WordsLevel : LevelController
    {
        [SerializeField] TextMesh riddleText;
        [SerializeField] TextMesh progress;
        [SerializeField] Transform[] slots;           // 6 caselle
        [SerializeField] Transform[] tiles;           // 9 tessere (con Collider2D)
        [SerializeField] TextMesh[] tileLetters;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float slotY = -0.35f, slotGap = 0.8f, tileGap = 0.88f;
        [SerializeField] float[] tileRows = { -2.1f, -3.2f };

        static readonly string[] Questions =
        {
            "What has keys but can't open locks?", "What gets wetter the more it dries?", "What has hands but can't clap?",
            "What has a neck but no head?", "What has an eye but can't see?", "What can you catch but not throw?",
            "What has teeth but can't bite?", "What goes up but never comes down?",
        };
        static readonly string[] Answers = { "PIANO", "TOWEL", "CLOCK", "BOTTLE", "NEEDLE", "COLD", "COMB", "AGE" };
        const string LastQuestion = "What word is always spelled WRONG?";
        const string LastAnswer = "WRONG";

        readonly List<int> order = new List<int>();
        Vector3[] home;
        int[] slotTile;       // tessera in ogni casella (-1 = vuota)
        int[] tileSlot;       // casella di ogni tessera (-1 = nella pila)
        string answer;
        int round, length, wrongTries, locked;
        bool busy;
        float bubbleUntil, clock;
        Camera cam;
        Collider2D[] cols;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            cols = new Collider2D[tiles.Length];
            for (int i = 0; i < tiles.Length; i++) cols[i] = tiles[i].GetComponent<Collider2D>();
            home = new Vector3[tiles.Length];
            slotTile = new int[slots.Length];
            tileSlot = new int[tiles.Length];
            var pool = new List<int>();
            for (int i = 0; i < Questions.Length; i++) pool.Add(i);
            for (int k = 0; k < 2; k++) { int j = Random.Range(0, pool.Count); order.Add(pool[j]); pool.RemoveAt(j); }
            Setup();
        }

        /// <summary>Ogni hint dà la parola giusta di un indovinello (nell'ordine in cui escono).</summary>
        public override string[] DynamicHints => order.Count < 2 ? null : new[]
        {
            "Riddle 1: the answer is " + Answers[order[0]] + ".",
            "Riddle 2: the answer is " + Answers[order[1]] + ".",
            "Riddle 3: the answer is " + LastAnswer + ".",
        };

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void Setup()
        {
            bool last = round >= order.Count;
            answer = last ? LastAnswer : Answers[order[round]];
            riddleText.text = Wrap(last ? LastQuestion : Questions[order[round]]);
            progress.text = "RIDDLE " + (round + 1) + " / 3";
            length = answer.Length;
            wrongTries = 0;
            locked = 0;
            busy = true;   // aspetta che le tessere arrivino
            StartCoroutine(Ready(0.7f));
            // lettere: risposta + esche
            var letters = new List<char>(answer);
            if (last) letters.AddRange("IHT");   // con queste si scrive anche RIGHT...
            else
            {
                int decoys = Mathf.Min(length <= 5 ? 3 : 2, tiles.Length - length);
                const string abc = "ABCDEFGHIKLMNOPRSTUWY";
                while (decoys > 0)
                {
                    char c = abc[Random.Range(0, abc.Length)];
                    if (answer.IndexOf(c) >= 0 || letters.Contains(c)) continue;
                    letters.Add(c);
                    decoys--;
                }
            }
            for (int i = letters.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (letters[i], letters[j]) = (letters[j], letters[i]); }

            for (int s = 0; s < slots.Length; s++)
            {
                slotTile[s] = -1;
                bool on = s < length;
                slots[s].gameObject.SetActive(on);
                if (on) slots[s].localPosition = new Vector3((s - (length - 1) * 0.5f) * slotGap, slotY, 0f);
            }
            int count = letters.Count, firstRow = Mathf.Min(5, count);
            for (int i = 0; i < tiles.Length; i++)
            {
                tileSlot[i] = -1;
                bool on = i < count;
                tiles[i].gameObject.SetActive(on);
                if (!on) continue;
                tileLetters[i].text = letters[i].ToString();
                int row = i < firstRow ? 0 : 1, inRow = row == 0 ? firstRow : count - firstRow, idx = row == 0 ? i : i - firstRow;
                home[i] = new Vector3((idx - (inRow - 1) * 0.5f) * tileGap, tileRows[row], 0f);
                tiles[i].localPosition = home[i] + Vector3.down * 3f;
                StartCoroutine(Move(tiles[i], home[i], 0.25f + i * 0.04f));
            }
        }

        /// <summary>Va a capo nel punto più vicino a metà (le frasi lunghe stanno su due righe).</summary>
        static string Wrap(string s)
        {
            if (s.Length < 22) return s;
            int mid = s.Length / 2, best = -1;
            for (int i = 0; i < s.Length; i++) if (s[i] == ' ' && (best < 0 || Mathf.Abs(i - mid) < Mathf.Abs(best - mid))) best = i;
            return best < 0 ? s : s.Substring(0, best) + "\n" + s.Substring(best + 1);
        }

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil && !busy) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) || !began || PointerInput.IsOverUi()) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            for (int i = 0; i < tiles.Length; i++)
            {
                if (!tiles[i].gameObject.activeSelf || cols[i] == null || !cols[i].OverlapPoint(w)) continue;
                TapTile(i);
                return;
            }
        }

        void TapTile(int i)
        {
            if (tileSlot[i] >= 0)
            {
                if (tileSlot[i] < locked) return;   // lettera regalata: resta lì
                slotTile[tileSlot[i]] = -1;
                tileSlot[i] = -1;
                Sfx.Play(this, "tick", 0.6f);
                StartCoroutine(Move(tiles[i], home[i], 0.12f));
                return;
            }
            int free = -1;
            for (int s = 0; s < length; s++) if (slotTile[s] < 0) { free = s; break; }
            if (free < 0) return;
            slotTile[free] = i;
            tileSlot[i] = free;
            Sfx.Play(this, "pop", 0.5f);
            StartCoroutine(Move(tiles[i], slots[free].localPosition, 0.12f));
            for (int s = 0; s < length; s++) if (slotTile[s] < 0) return;
            StartCoroutine(Check());
        }

        IEnumerator Move(Transform t, Vector3 to, float seconds)
        {
            Vector3 from = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds) { t.localPosition = Vector3.Lerp(from, to, 1f - (1f - k) * (1f - k)); yield return null; }
            t.localPosition = to;
        }

        IEnumerator Check()
        {
            busy = true;
            yield return new WaitForSeconds(0.2f);
            string word = "";
            for (int s = 0; s < length; s++) word += tileLetters[slotTile[s]].text;
            if (word == answer)
            {
                Sfx.Play(this, "ding", 0.8f);
                Haptics.Light();
                for (int s = 0; s < length; s++) StartCoroutine(Hop(tiles[slotTile[s]], s * 0.06f));
                bool last = round >= order.Count;
                Say(last ? "W-R-O-N-G. Spelled wrong every time!" : "Correct! It's " + answer + ".", 1.6f);
                yield return new WaitForSeconds(1.5f);
                round++;
                if (round > order.Count)
                {
                    riddleText.text = "GENIUS!";
                    progress.text = "3 / 3";
                    Sfx.Play(this, "ding", 0.9f);
                    yield return new WaitForSeconds(1.2f);
                    Win();
                    yield break;
                }
                Setup();
                yield break;
            }
            // sbagliato: le caselle tremano e le lettere tornano a posto
            wrongTries++;
            Sfx.Play(this, "buzz", 0.6f);
            Haptics.Light();
            if (answer == LastAnswer && word == "RIGHT") Say("RIGHT? Nope. That's WRONG!", 2f);
            else if (wrongTries == 3) Say("Here's a free letter!", 1.6f);
            else Say("Nope!", 1f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                float dx = Mathf.Sin(k * Mathf.PI * 6f) * 0.12f * (1f - k);
                for (int s = 0; s < length; s++)
                {
                    slots[s].localPosition = new Vector3((s - (length - 1) * 0.5f) * slotGap + dx, slotY, 0f);
                    tiles[slotTile[s]].localPosition = slots[s].localPosition;
                }
                yield return null;
            }
            for (int s = 0; s < length; s++) slots[s].localPosition = new Vector3((s - (length - 1) * 0.5f) * slotGap, slotY, 0f);
            for (int s = locked; s < length; s++)
            {
                int t = slotTile[s];
                slotTile[s] = -1;
                tileSlot[t] = -1;
                StartCoroutine(Move(tiles[t], home[t], 0.2f));
            }
            // dopo 3 errori regala la lettera successiva (fissa nella sua casella)
            if (wrongTries >= 3 && locked < length - 1)
            {
                for (int i = 0; i < tiles.Length; i++)
                {
                    if (!tiles[i].gameObject.activeSelf || tileSlot[i] >= 0 || tileLetters[i].text[0] != answer[locked]) continue;
                    slotTile[locked] = i;
                    tileSlot[i] = locked;
                    StartCoroutine(Move(tiles[i], slots[locked].localPosition, 0.25f));
                    locked++;
                    break;
                }
            }
            yield return new WaitForSeconds(0.25f);
            busy = false;
        }

        IEnumerator Ready(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            busy = false;
        }

        IEnumerator Hop(Transform t, float delay)
        {
            yield return new WaitForSeconds(delay);
            Vector3 p = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { t.localPosition = p + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.3f; yield return null; }
            t.localPosition = p;
        }
    }
}
