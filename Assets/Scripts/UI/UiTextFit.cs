using UnityEngine;
using UnityEngine.UI;

namespace Gotcha.UI
{
    /// <summary>
    /// Rimpicciolisce il carattere di un testo uGUI finché sta tutto dentro il suo riquadro (0.84). Il "best fit" di Unity
    /// a volte lascia sbordare le scritte su una riga sola o con gli effetti (Outline/Shadow) del genitore: qui si misura
    /// davvero il testo con il TextGenerator e si sceglie la dimensione più grande che entra, in larghezza e in altezza.
    /// singleLine = la frase resta su una riga (es. il nastro dorato dell'Easter egg). Ricalcola quando cambiano testo o
    /// misura del riquadro.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class UiTextFit : MonoBehaviour
    {
        public int maxSize = 44, minSize = 14;
        public bool singleLine;

        Text text;
        RectTransform rt;
        string lastText;
        Vector2 lastSize;

        void Awake()
        {
            text = GetComponent<Text>();
            rt = (RectTransform)transform;
        }

        void OnEnable() { lastText = null; }

        void LateUpdate()
        {
            if (text == null) return;
            Vector2 size = rt.rect.size;
            if (text.text == lastText && size == lastSize) return;
            lastText = text.text;
            lastSize = size;
            Fit(size);
        }

        void Fit(Vector2 size)
        {
            if (size.x <= 1f || size.y <= 1f || string.IsNullOrEmpty(text.text)) return;
            text.resizeTextForBestFit = false;
            text.horizontalOverflow = singleLine ? HorizontalWrapMode.Overflow : HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            TextGenerator gen = text.cachedTextGeneratorForLayout;
            int lo = minSize, hi = maxSize, best = minSize;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                TextGenerationSettings s = text.GetGenerationSettings(new Vector2(singleLine ? 0f : size.x, 0f));
                s.fontSize = mid;
                s.resizeTextForBestFit = false;
                s.horizontalOverflow = singleLine ? HorizontalWrapMode.Overflow : HorizontalWrapMode.Wrap;
                s.verticalOverflow = VerticalWrapMode.Overflow;
                s.generationExtents = new Vector2(singleLine ? 0f : size.x, 0f);
                float scale = text.pixelsPerUnit > 0f ? text.pixelsPerUnit : 1f;
                float w = gen.GetPreferredWidth(text.text, s) / scale;
                float h = gen.GetPreferredHeight(text.text, s) / scale;
                bool fits = (singleLine ? w <= size.x : true) && h <= size.y;
                if (fits) { best = mid; lo = mid + 1; }
                else hi = mid - 1;
            }
            text.fontSize = best;
        }
    }
}
