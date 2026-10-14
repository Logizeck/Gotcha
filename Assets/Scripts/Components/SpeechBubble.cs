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

        public void Show(string message)
        {
            Init();
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
