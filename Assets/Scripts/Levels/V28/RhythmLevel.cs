using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 71 — il topo DJ. Le note scorrono verso il cerchio a tempo di musica (128 bpm): tocca lo schermo
    /// quando una nota è nel cerchio. Le note grigie con la X sono stonate: NON toccarle. Il topolino balla a ogni
    /// colpo giusto. Servono 18 note prese; con più di 5 errori la festa finisce.
    /// </summary>
    public class RhythmLevel : LevelController
    {
        [SerializeField] AudioSource music;           // il brano (128 bpm) suona da qui, così il tempo è esatto
        [SerializeField] SpriteRenderer noteTemplate;
        [SerializeField] SpriteRenderer ghostTemplate;
        [SerializeField] Transform hitRing;
        [SerializeField] Transform dancer;
        [SerializeField] TextMesh score;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float bpm = 128f;
        [SerializeField] float speed = 3f;            // unità al secondo
        [SerializeField] float window = 0.14f;        // secondi di tolleranza
        [SerializeField] int needed = 18, maxMisses = 5;

        // battute (in beat) delle note; negative = nota stonata da non toccare
        static readonly float[] pattern = { 4, 5, 6, 7, 8, 9, 10, 10.5f, 11, -12, 13, 14, 14.5f, 15, 16, 17, -18, 18.5f, 19, 20, 21, 21.5f, 22, -23, 24, 25, 26, 26.5f, 27, 28, -29, 29.5f, 30 };

        class Note { public float time; public bool ghost; public Transform t; public bool gone; }
        readonly List<Note> notes = new List<Note>();
        int hits, misses;
        bool done, started;
        float beat, bounce;

        public override void OnLevelStart()
        {
            beat = 60f / bpm;
            foreach (float b in pattern)
            {
                bool ghost = b < 0f;
                SpriteRenderer sr = Instantiate(ghost ? ghostTemplate : noteTemplate, transform);
                sr.gameObject.SetActive(true);
                notes.Add(new Note { time = Mathf.Abs(b) * beat, ghost = ghost, t = sr.transform });
            }
            if (MusicPlayer.Instance != null) MusicPlayer.Instance.Duck(this, 0f);
            Score();
            if (bubble != null) { bubble.Show("Tap on the beat! Never the grey notes."); StartCoroutine(HideLater(2.2f)); }
            StartCoroutine(Begin());
        }

        void OnDestroy() { if (MusicPlayer.Instance != null) MusicPlayer.Instance.Unduck(this); }

        IEnumerator Begin()
        {
            yield return new WaitForSeconds(0.6f);
            music.loop = false;
            music.Play();
            started = true;
        }

        float Now => music.clip != null && music.isPlaying ? music.timeSamples / (float)music.clip.frequency : (started ? 99f : -1f);

        void Score() { score.text = hits + " / " + needed + "    MISSES " + misses + "/" + maxMisses; }

        void Update()
        {
            if (done || IsFinished) return;
            float now = Now;
            Vector3 ring = hitRing.position;
            foreach (Note n in notes)
            {
                if (n.gone) continue;
                float dt = n.time - now;
                n.t.position = ring + Vector3.right * dt * speed;
                n.t.gameObject.SetActive(dt < 2f);
                if (dt < -window)
                {
                    n.gone = true;
                    n.t.gameObject.SetActive(false);
                    if (!n.ghost) Miss();
                }
            }
            // il topolino si muove a tempo
            bounce = Mathf.MoveTowards(bounce, 0f, Time.deltaTime * 4f);
            float pulse = now > 0f ? 1f - Mathf.Repeat(now / beat, 1f) : 0f;
            dancer.localScale = new Vector3(dancer.localScale.x, Mathf.Abs(dancer.localScale.x) * (1f + pulse * 0.06f + bounce * 0.25f), 1f);
            dancer.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(now / beat * Mathf.PI) * 8f * (1f + bounce));
            hitRing.localScale = Vector3.one * (1f + pulse * 0.08f);

            if (PointerInput.AnyTouchBegan() && !PointerInput.IsOverUi()) Tap(now);
            if (hits >= needed) { StartCoroutine(Party()); return; }
            if (misses > maxMisses) { StartCoroutine(Booed()); return; }
            if (started && !music.isPlaying && now > 0f) { if (hits >= needed) StartCoroutine(Party()); else StartCoroutine(Booed()); }
        }

        void Tap(float now)
        {
            Note best = null;
            float bestD = window;
            foreach (Note n in notes)
            {
                if (n.gone) continue;
                float d = Mathf.Abs(n.time - now);
                if (d <= bestD) { best = n; bestD = d; }
            }
            if (best == null) { Miss(); return; }
            best.gone = true;
            best.t.gameObject.SetActive(false);
            if (best.ghost) { Sfx.Play(this, "buzz", 0.8f); Miss(); return; }
            hits++;
            bounce = 1f;
            Sfx.Play(this, "pop", 0.4f);
            Score();
        }

        void Miss()
        {
            misses++;
            Score();
        }

        IEnumerator Party()
        {
            done = true;
            if (bubble != null) bubble.Show("WHAT A GROOVE!");
            yield return new WaitForSeconds(1.5f);
            Win();
        }

        IEnumerator Booed()
        {
            done = true;
            music.Stop();
            Sfx.Play(this, "buzz", 1f);
            if (bubble != null) bubble.Show("*record scratch* Off the beat...");
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
