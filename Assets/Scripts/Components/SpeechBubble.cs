using System.Collections;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>Fumetto con testo (TextMesh figlio). Show() lo fa apparire con un "pop"; il testo si adatta alla larghezza.</summary>
    public class SpeechBubble : MonoBehaviour
    {
        public TextMesh text;
        public float maxTextWidth = 2.4f;
        public float popSeconds = 0.25f;
        [Tooltip("Fumetto di un gatto: quando parla si sente il verso (miagolio, soffio, fusa...).")]
        public bool catVoice;

        static AudioSource voice;   // sorgente unica e persistente: il fumetto si nasconde ma il verso non si interrompe

        Vector3 baseScale = Vector3.one;
        Vector3 textBaseScale = Vector3.one;
        bool initialized;

        void Init()
        {
            if (initialized) return;
            initialized = true;
            baseScale = transform.localScale;
            if (text != null) textBaseScale = text.transform.localScale;
        }

        /// <summary>Mostra il fumetto. Con <paramref name="silent"/> il gatto non fa il verso (es. parla la vespa).</summary>
        public void Show(string message, bool silent = false)
        {
            Init();
            if (catVoice && !silent && !string.IsNullOrEmpty(message))
            {
                if (voice == null)
                {
                    var go = new GameObject("CatVoice");
                    DontDestroyOnLoad(go);
                    voice = go.AddComponent<AudioSource>();
                    voice.playOnAwake = false;
                }
                voice.pitch = Random.Range(0.93f, 1.08f);
                voice.PlayOneShot(CatVoice.For(message), 0.9f);
            }
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(ShowRoutine(message));
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        IEnumerator ShowRoutine(string message)
        {
            if (text != null)
            {
                text.text = message;
                text.transform.localScale = textBaseScale;
            }
            transform.localScale = Vector3.zero;
            yield return null;   // il mesh del testo è pronto dal frame successivo

            if (text != null)
            {
                transform.localScale = baseScale;
                float w = text.GetComponent<MeshRenderer>().bounds.size.x;
                if (w > maxTextWidth && w > 0f) text.transform.localScale = textBaseScale * (maxTextWidth / w);
            }

            for (float k = 0f; k < 1f; k += Time.deltaTime / popSeconds)
            {
                float s = 1f + Mathf.Sin(k * Mathf.PI) * 0.25f;   // piccolo rimbalzo
                transform.localScale = baseScale * (k * s);
                yield return null;
            }
            transform.localScale = baseScale;
        }
    }
}
