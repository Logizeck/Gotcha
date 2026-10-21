using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The two-finger pianist" — il leggio chiede tre accordi da due note. Il secondo tasto di ogni accordo è
    /// timido: se lo tocchi da solo sparisce. Soluzione: tieni premuto il primo tasto e, con un altro dito, premi il
    /// secondo: suonano insieme. Tre accordi sbagliati (due tasti tenuti insieme ma non quelli giusti) = sconfitta.
    /// </summary>
    public class TwoFingerPianistLevel : LevelController
    {
        [SerializeField] Transform[] keys;
        [SerializeField] SpriteRenderer[] keyArts;
        [SerializeField] Collider2D[] keyAreas;
        [SerializeField] Transform pianist;
        [SerializeField] TextMesh chordText, triesText;
        [SerializeField] SpriteRenderer[] progressDots;
        [SerializeField] SmokeEmitter notes;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;

        static readonly string[] Names = { "C", "D", "E", "F", "G", "A", "B" };
        static readonly float[] Freq = { 261.6f, 293.7f, 329.6f, 349.2f, 392f, 440f, 493.9f };
        // accordi: primo tasto (normale) e secondo tasto (timido)
        static readonly int[,] Chords = { { 0, 2 }, { 1, 5 }, { 3, 6 } };

        Camera cam;
        readonly Dictionary<int, int> fingerKey = new Dictionary<int, int>();
        float[] hiddenFor;
        Vector3[] keyHome;
        int chord, wrong;
        bool busy, pairJudged;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            hiddenFor = new float[keys.Length];
            keyHome = new Vector3[keys.Length];
            for (int i = 0; i < keys.Length; i++) keyHome[i] = keys[i].localPosition;
            if (notes != null) notes.Stop();
            S41KKit.Confetti(confetti, false);
            ShowChord();
            UpdateTries();
            if (bubble != null) bubble.Show("My piano is... a little shy. Play the chord!");
        }

        void ShowChord()
        {
            // dopo l'ultimo accordo non c'è un accordo da mostrare (prima andava fuori dalla tabella e il livello si
            // bloccava sull'ultima nota): restano solo i pallini tutti verdi
            if (chord < Chords.GetLength(0)) chordText.text = Names[Chords[chord, 0]] + " + " + Names[Chords[chord, 1]];
            for (int i = 0; i < progressDots.Length; i++)
                progressDots[i].color = i < chord ? new Color(0.45f, 0.85f, 0.4f) : new Color(1f, 1f, 1f, 0.35f);
        }

        void UpdateTries() { triesText.text = "WRONG CHORDS: " + wrong + "/3"; }

        int KeyAt(Vector2 w)
        {
            for (int i = 0; i < keyAreas.Length; i++)
                if (hiddenFor[i] <= 0f && keyAreas[i].OverlapPoint(w)) return i;
            return -1;
        }

        void Update()
        {
            if (IsFinished) return;
            for (int i = 0; i < keys.Length; i++)
            {
                if (hiddenFor[i] <= 0f) continue;
                hiddenFor[i] -= Time.deltaTime;
                if (hiddenFor[i] <= 0f) { S41KKit.Alpha(keyArts[i], 1f); StartCoroutine(S41KKit.Pop(keys[i], 1.08f, 0.2f)); }
            }
            if (busy) { fingerKey.Clear(); return; }

            var alive = new HashSet<int>();
            var pressed = new List<Vector2Int>();
            foreach (S41KKit.Finger f in S41KKit.Fingers(cam))
            {
                if (f.ended) { fingerKey.Remove(f.id); continue; }
                alive.Add(f.id);
                if (f.began)
                {
                    int k = KeyAt(f.world);
                    if (k >= 0) pressed.Add(new Vector2Int(f.id, k));
                }
            }
            // due tasti toccati nello stesso istante: il tasto timido per ultimo, così l'accordo vale
            int shyKey = Chords[chord, 1];
            pressed.Sort((a, b) => (a.y == shyKey ? 1 : 0).CompareTo(b.y == shyKey ? 1 : 0));
            foreach (Vector2Int p in pressed) if (!busy) Press(p.y, p.x);
            var gone = new List<int>();
            foreach (int id in fingerKey.Keys) if (id < 100 && !alive.Contains(id)) gone.Add(id);
            foreach (int id in gone) fingerKey.Remove(id);
#if UNITY_EDITOR
            // Editor: i tasti numerici 1..7 fanno da dita in più
            for (int i = 0; i < keys.Length && i < 9; i++)
            {
                KeyCode kc = KeyCode.Alpha1 + i;
                if (Input.GetKeyDown(kc) && hiddenFor[i] <= 0f) Press(i, 100 + i);
                if (!Input.GetKey(kc)) fingerKey.Remove(100 + i);
            }
#endif
            if (HeldKeys().Count < 2) pairJudged = false;
            for (int i = 0; i < keys.Length; i++)
                keys[i].localPosition = keyHome[i] + (fingerKey.ContainsValue(i) ? new Vector3(0f, -0.08f, 0f) : Vector3.zero);
        }

        List<int> HeldKeys()
        {
            var l = new List<int>();
            foreach (int k in fingerKey.Values) if (!l.Contains(k)) l.Add(k);
            return l;
        }

        void Press(int k, int fingerId)
        {
            int shy = Chords[chord, 1];
            if (k == shy && HeldKeys().Count == 0)
            {
                // il tasto timido scappa se lo tocchi da solo
                hiddenFor[k] = 1.3f;
                S41KKit.Alpha(keyArts[k], 0.08f);
                Sfx.Play(this, "whoosh", 0.6f);
                if (bubble != null) bubble.Show("Eek! That key is shy... it needs company!");
                return;
            }
            fingerKey[fingerId] = k;
            S41KKit.Tone(this, Freq[k], 0.9f, 0.6f);
            List<int> held = HeldKeys();
            if (held.Count >= 2 && !pairJudged)
            {
                pairJudged = true;
                int a = Chords[chord, 0], b = Chords[chord, 1];
                if (held.Count == 2 && held.Contains(a) && held.Contains(b)) StartCoroutine(GoodChord());
                else StartCoroutine(WrongChord());
            }
        }

        IEnumerator GoodChord()
        {
            busy = true;
            S41KKit.Tone(this, Freq[Chords[chord, 0]] * 2f, 1.2f, 0.35f);
            Sfx.Play(this, "ding", 0.6f);
            Haptics.Light();
            if (notes != null) notes.Play();
            StartCoroutine(S41KKit.Pop(pianist, 1.15f, 0.35f));
            chord++;
            ShowChord();
            if (chord >= Chords.GetLength(0))
            {
                if (bubble != null) bubble.Show("BRAVO! A two-finger masterpiece!");
                chordText.text = "BRAVO!";
                yield return new WaitForSeconds(0.4f);
                // gran finale: arpeggio su tutti i tasti
                for (int i = 0; i < keys.Length; i++)
                {
                    S41KKit.Tone(this, Freq[i] * 2f, 0.6f, 0.4f);
                    StartCoroutine(S41KKit.Move(keys[i], keyHome[i] + new Vector3(0f, -0.1f, 0f), 0.08f));
                    yield return new WaitForSeconds(0.1f);
                    keys[i].localPosition = keyHome[i];
                }
                S41KKit.Confetti(confetti, true);
                yield return new WaitForSeconds(1.8f);
                S41KKit.Confetti(confetti, false);
                if (notes != null) notes.Stop();
                Win();
                yield break;
            }
            if (bubble != null) bubble.Show(chord == 1 ? "Beautiful! Next chord!" : "One more! Encore!");
            yield return new WaitForSeconds(0.8f);
            if (notes != null) notes.Stop();
            busy = false;
        }

        IEnumerator WrongChord()
        {
            busy = true;
            wrong++;
            UpdateTries();
            Sfx.Play(this, "buzz", 0.6f);
            Haptics.Medium();
            foreach (SpriteRenderer a in keyArts) a.color = new Color(1f, 0.75f, 0.75f, a.color.a);
            yield return StartCoroutine(S41KKit.Shake(pianist, 0.12f, 0.45f));
            foreach (SpriteRenderer a in keyArts) a.color = new Color(1f, 1f, 1f, a.color.a);
            if (wrong >= 3)
            {
                if (bubble != null) bubble.Show("My ears! I quit music forever!");
                yield return new WaitForSeconds(1.4f);
                Fail();
                yield break;
            }
            if (bubble != null) bubble.Show("Wrong chord! Read the music stand!");
            busy = false;
        }
    }
}
