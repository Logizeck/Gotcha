using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 88 — l'uccellino a voce. L'uccellino vola all'altezza della tua voce: più forte gridi (o canti nel
    /// microfono), più sale; in silenzio scende. Passa tra i tubi senza toccarli. Easter egg `space_bird`: un urlo
    /// lunghissimo al massimo lo spara nello spazio. (Editor: B = voce media, N = urlo.)
    /// </summary>
    public class ScreamBirdLevel : LevelController
    {
        [SerializeField] Transform bird;
        [SerializeField] GameObject wingsUp, wingsDown;
        [SerializeField] Transform[] pipesTop, pipesBottom;   // coppie
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float lowY = -3.3f, highY = 2.8f;
        [SerializeField] float scroll = 1.5f;
        [SerializeField] float gap = 1.8f;
        [SerializeField] float pipeWidth = 0.7f;
        [SerializeField] float birdRadius = 0.22f;
        [SerializeField] float eggSeconds = 2.5f;
        [SerializeField] string eggId = "space_bird";

        float voice, maxFor, startDelay = 1.5f;
        float[] gapY;
        int passed;
        bool done;

        public override void OnLevelStart()
        {
            gapY = new float[pipesTop.Length];
            for (int i = 0; i < pipesTop.Length; i++)
            {
                gapY[i] = Random.Range(lowY + 1.3f, highY - 1.2f);
                SetPipe(i, 2.6f + i * 2.7f);
            }
            if (bubble != null) { bubble.Show("Sing to fly! Louder = higher."); StartCoroutine(HideLater(2.2f)); }
        }

        void OnDestroy() { Inputs.ReleaseMic(this); }

        void SetPipe(int i, float x)
        {
            float top = gapY[i] + gap * 0.5f, bottom = gapY[i] - gap * 0.5f;
            pipesTop[i].position = new Vector3(x, top, 0f);
            pipesBottom[i].position = new Vector3(x, bottom, 0f);
        }

        void Update()
        {
            if (done || IsFinished) return;
            float mic = Inputs.Mic(this);
            voice = Mathf.Lerp(voice, mic, 1f - Mathf.Exp(-6f * Time.deltaTime));
            float y = Mathf.Lerp(lowY, highY, Mathf.Clamp01(voice * 1.15f));
            Vector3 b = bird.position;
            b.y = Mathf.MoveTowards(b.y, y, 6f * Time.deltaTime);
            bird.position = b;
            bool up = Mathf.Repeat(Time.time * (4f + voice * 10f), 1f) < 0.5f;
            wingsUp.SetActive(up);
            wingsDown.SetActive(!up);

            maxFor = mic > 0.95f ? maxFor + Time.deltaTime : 0f;
            if (maxFor > eggSeconds) { StartCoroutine(ToSpace()); return; }

            startDelay -= Time.deltaTime;
            if (startDelay > 0f) return;
            for (int i = 0; i < pipesTop.Length; i++)
            {
                float x = pipesTop[i].position.x - scroll * Time.deltaTime;
                SetPipe(i, x);
                if (Mathf.Abs(x - b.x) < pipeWidth * 0.5f + birdRadius && (b.y > gapY[i] + gap * 0.5f - birdRadius || b.y < gapY[i] - gap * 0.5f + birdRadius))
                { StartCoroutine(Bonk()); return; }
            }
            int now = 0;
            foreach (Transform t in pipesTop) if (t.position.x < b.x - pipeWidth) now++;
            passed = now;
            if (passed >= pipesTop.Length) StartCoroutine(Through());
        }

        IEnumerator Bonk()
        {
            done = true;
            if (bubble != null) bubble.Show("BONK!");
            Vector3 p = bird.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
            {
                bird.position = p + new Vector3(-k * 0.5f, -k * k * 4f, 0f);
                bird.rotation = Quaternion.Euler(0, 0, k * 300f);
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
            Fail();
        }

        IEnumerator Through()
        {
            done = true;
            if (bubble != null) bubble.Show("What a voice!");
            yield return new WaitForSeconds(1.4f);
            Win();
        }

        /// <summary>Easter egg: urlo infinito. L'uccellino parte come un razzo e diventa una stellina.</summary>
        IEnumerator ToSpace()
        {
            done = true;
            if (bubble != null) bubble.Show("TO INFINITY!!!");
            Vector3 s = bird.localScale;
            for (float k = 0f; k < 1.5f; k += Time.deltaTime)
            {
                bird.position += Vector3.up * (4f + k * 10f) * Time.deltaTime;
                bird.localScale = s * Mathf.Max(0.1f, 1f - k * 0.5f);
                yield return null;
            }
            if (bubble != null) bubble.Show("...your neighbours hate you.");
            yield return new WaitForSeconds(1.5f);
            WinWithEgg(eggId);
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
