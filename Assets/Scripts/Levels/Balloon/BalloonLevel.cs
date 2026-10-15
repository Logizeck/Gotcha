using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 33 — gonfia il palloncino fino alla sagoma tratteggiata, non di più.
    /// Due modi: col fiato (microfono) oppure con la pompa (10 colpi esatti). Raggiunta la sagoma bisogna aspettare
    /// 2 secondi senza esagerare: un colpo in più o troppo fiato e... PLOP! (Editor: tasto B per soffiare.)
    /// </summary>
    public class BalloonLevel : LevelController
    {
        [SerializeField] Transform balloon;
        [SerializeField] SpriteRenderer outline;
        [SerializeField] TapButton pump;
        [SerializeField] Transform pumpHandle;
        [SerializeField] SmokeEmitter scraps;
        [SerializeField] GameObject boom;
        [SerializeField] TextMesh counter;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int pumpsNeeded = 10;
        [SerializeField] float minSize = 0.22f;
        [SerializeField] float blowRate = 0.16f;      // crescita al secondo soffiando forte
        [SerializeField] float blowThreshold = 0.22f;
        [SerializeField] float fullAt = 0.985f;
        [SerializeField] float popAt = 1.03f;
        [SerializeField] float waitSeconds = 2f;

        float size;
        int pumps;
        float fullFor;
        bool done;
        Vector3 handleHome;
        AudioSource sfx;

        public override void OnLevelStart()
        {
            size = minSize;
            if (pumpHandle != null) handleHome = pumpHandle.localPosition;
            if (pump != null) pump.Pressed += OnPump;
            if (boom != null) boom.SetActive(false);
            if (SensorHub.Instance != null) SensorHub.Instance.EnableMic();
            sfx = gameObject.AddComponent<AudioSource>();
            Apply();
            if (bubble != null) { bubble.Show("Up to the dotted line!"); StartCoroutine(HideLater(2f)); }
        }

        void OnDestroy()
        {
            if (pump != null) pump.Pressed -= OnPump;
            if (SensorHub.Instance != null) SensorHub.Instance.DisableMic();
        }

        void OnPump(TapButton b)
        {
            if (done) return;
            pumps++;
            size = minSize + (1f - minSize) * pumps / (float)pumpsNeeded;
            if (pumpHandle != null) StartCoroutine(PumpStroke());
            sfx.PlayOneShot(Noise("pfff", 0.18f, 0.25f, 0.5f));
            Apply();
        }

        IEnumerator PumpStroke()
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.18f)
            {
                pumpHandle.localPosition = handleHome + Vector3.down * Mathf.Sin(k * Mathf.PI) * 0.45f;
                yield return null;
            }
            pumpHandle.localPosition = handleHome;
        }

        void Update()
        {
            if (done || IsFinished) return;
            float level = SensorHub.Instance != null ? SensorHub.Instance.MicLevel01 : 0f;
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.B)) level = 0.8f;
#endif
            if (level > blowThreshold) { size += blowRate * Mathf.Lerp(0.5f, 1.3f, level) * Time.deltaTime; Apply(); }

            // tensione: vicino al limite il palloncino trema
            float tension = Mathf.Clamp01((size - 0.85f) / 0.15f);
            balloon.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 40f) * tension * 2.5f + Mathf.Sin(Time.time * 1.3f) * 4f);

            if (size > popAt || pumps > pumpsNeeded) { StartCoroutine(Pop()); return; }
            fullFor = size >= fullAt ? fullFor + Time.deltaTime : 0f;
            if (outline != null) outline.color = size >= fullAt ? new Color(0.25f, 0.8f, 0.35f, 0.9f) : new Color(1f, 1f, 1f, 0.9f);
            if (fullFor >= waitSeconds) StartCoroutine(Done());
        }

        void Apply()
        {
            balloon.localScale = Vector3.one * size;
            if (counter != null) counter.text = pumps > 0 ? pumps.ToString() : "";
        }

        IEnumerator Pop()
        {
            done = true;
            balloon.gameObject.SetActive(false);
            sfx.PlayOneShot(Noise("pop", 0.25f, 0.9f, 0.05f));
            if (scraps != null) { scraps.transform.position = balloon.position + Vector3.up * 1.1f * size; scraps.Play(); }
            if (boom != null)
            {
                boom.SetActive(true);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f) { boom.transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 0.6f, k); yield return null; }
                boom.SetActive(false);
            }
            if (bubble != null) bubble.Show("PLOP!");
            yield return new WaitForSeconds(0.25f);
            if (scraps != null) scraps.Stop();
            yield return new WaitForSeconds(1f);
            Fail();
        }

        IEnumerator Done()
        {
            done = true;
            if (bubble != null) bubble.Show("Perfect!");
            Vector3 p = balloon.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.2f)
            {
                balloon.localPosition = p + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.25f;   // un saltello di gioia
                yield return null;
            }
            balloon.localPosition = p;
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }

        /// <summary>Soffio della pompa o scoppio: rumore con inviluppo (attacco, decadimento, quanto è "sordo").</summary>
        static AudioClip Noise(string name, float seconds, float gain, float softness)
        {
            const int rate = 22050;
            int n = Mathf.RoundToInt(seconds * rate);
            var data = new float[n];
            var rnd = new System.Random(name.GetHashCode());
            float lp = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                float w = (float)rnd.NextDouble() * 2f - 1f;
                lp += (w - lp) * (1f - softness);
                data[i] = lp * gain * Mathf.Pow(1f - t, 2.5f) * Mathf.Min(1f, i / 30f);
            }
            AudioClip clip = AudioClip.Create(name, n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
