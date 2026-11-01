using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Festa di vittoria comune ai livelli nuovi (dalla 0.76, roadmap 1-300): un solo componente, configurato per livello
    /// dal builder (GotchaReward.RewardRig). Mostra il complimento del livello con un "pop", un bagliore neon che pulsa e
    /// ruota, coriandoli, un piccolo arpeggio positivo sintetizzato (suono originale, nessun file) e la vibrazione.
    /// Durata 1,5-2,5 s; alla fine il livello chiama Win(complimento), che passa al livello successivo.
    /// Uso tipico:  yield return reward.Play("YOU OUTSMARTED A BUTTON!");  Win("YOU OUTSMARTED A BUTTON!");
    /// </summary>
    public class LevelReward : MonoBehaviour
    {
        [SerializeField] Transform holder;          // spento finché non si festeggia
        [SerializeField] TextMesh compliment;
        [SerializeField] SpriteRenderer glow;
        [SerializeField] SpriteRenderer rays;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] Color neon = new Color(0.35f, 1f, 0.75f, 1f);

        static AudioClip jingle;
        AudioSource source;
        bool playing;

        public bool Playing => playing;

        /// <summary>Da chiamare in OnLevelStart: tutto spento.</summary>
        public void ResetReward()
        {
            StopAllCoroutines();
            playing = false;
            if (holder != null) holder.gameObject.SetActive(false);
            HalloweenKit.Confetti(confetti, false);
            if (source != null) source.Stop();
        }

        void OnDisable()
        {
            // uscita dal livello a metà festa: niente coriandoli o suoni rimasti accesi
            if (source != null) source.Stop();
            HalloweenKit.Confetti(confetti, false);
            playing = false;
        }

        /// <summary>La festa: <paramref name="seconds"/> tra 1,5 e 2,5 (fuori da questi limiti viene corretta).</summary>
        public IEnumerator Play(string text, float seconds = 2f)
        {
            if (playing) yield break;
            playing = true;
            seconds = Mathf.Clamp(seconds, 1.5f, 2.5f);
            if (holder != null) holder.gameObject.SetActive(true);
            if (compliment != null) { compliment.text = text; compliment.color = Color.white; }
            HalloweenKit.Confetti(confetti, true);
            PlayJingle();
            Haptics.Success();
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float pop = t < 0.35f ? Mathf.Lerp(0.2f, 1.15f, t / 0.35f) : Mathf.Lerp(1.15f, 1f, Mathf.Clamp01((t - 0.35f) / 0.25f));
                if (compliment != null) compliment.transform.localScale = Vector3.one * pop;
                float pulse = 0.85f + 0.15f * Mathf.Sin(t * 10f);
                if (glow != null)
                {
                    glow.transform.localScale = Vector3.one * (3.2f * pulse);
                    glow.color = new Color(neon.r, neon.g, neon.b, 0.55f * Mathf.Clamp01(t * 4f));
                }
                if (rays != null)
                {
                    rays.transform.localRotation = Quaternion.Euler(0f, 0f, -t * 60f);
                    rays.color = new Color(1f, 0.95f, 0.6f, 0.45f * Mathf.Clamp01(t * 3f));
                }
                yield return null;
            }
            HalloweenKit.Confetti(confetti, false);
            playing = false;
        }

        void PlayJingle()
        {
            if (source == null)
            {
                source = GetComponent<AudioSource>();
                if (source == null) source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
            }
            if (jingle == null) jingle = MakeJingle();
            source.PlayOneShot(jingle, 0.7f);
        }

        /// <summary>Arpeggio maggiore in salita (do-mi-sol-do) con un piccolo luccichio: 0,7 s.</summary>
        static AudioClip MakeJingle()
        {
            const int rate = 22050;
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
            const float step = 0.11f, tail = 0.35f;
            int n = (int)(rate * (step * notes.Length + tail));
            var data = new float[n];
            for (int k = 0; k < notes.Length; k++)
            {
                int start = (int)(rate * step * k);
                int len = (int)(rate * (k == notes.Length - 1 ? step + tail : step * 1.6f));
                for (int i = 0; i < len && start + i < n; i++)
                {
                    float x = i / (float)rate;
                    float env = Mathf.Min(1f, x * 200f) * Mathf.Exp(-x * (k == notes.Length - 1 ? 5f : 9f));
                    float f = notes[k];
                    data[start + i] += env * 0.28f * (Mathf.Sin(2f * Mathf.PI * f * x) + 0.35f * Mathf.Sin(4f * Mathf.PI * f * x));
                }
            }
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(data[i], -1f, 1f);
            AudioClip clip = AudioClip.Create("gotcha_reward", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
