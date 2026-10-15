using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gotcha.UI
{
    /// <summary>
    /// Pagine scorrevoli col dito: il contenuto segue lo swipe e al rilascio scatta alla pagina vicina.
    /// I pulsanti dentro le pagine continuano a funzionare (i trascinamenti risalgono fino a qui).
    /// </summary>
    public class PagerSwipe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public RectTransform viewport;
        public RectTransform content;
        public int PageCount { get; set; } = 1;
        public int Page { get; private set; }
        public event Action<int> PageChanged;

        float dragOffset;
        bool dragging;

        float Width => viewport != null ? viewport.rect.width : 1f;

        public void SetPage(int page, bool instant = false)
        {
            Page = Mathf.Clamp(page, 0, Mathf.Max(0, PageCount - 1));
            if (instant && content != null) content.anchoredPosition = new Vector2(-Page * Width, 0f);
            PageChanged?.Invoke(Page);
        }

        public void OnBeginDrag(PointerEventData e) { dragging = true; dragOffset = 0f; }

        public void OnDrag(PointerEventData e)
        {
            dragOffset += e.delta.x / (transform.lossyScale.x > 0f ? transform.lossyScale.x : 1f);
            // resistenza ai bordi
            if ((Page == 0 && dragOffset > 0f) || (Page == PageCount - 1 && dragOffset < 0f)) dragOffset *= 0.92f;
        }

        public void OnEndDrag(PointerEventData e)
        {
            dragging = false;
            float threshold = Width * 0.18f;
            if (dragOffset < -threshold) SetPage(Page + 1);
            else if (dragOffset > threshold) SetPage(Page - 1);
            dragOffset = 0f;
        }

        void Update()
        {
            if (content == null) return;
            float target = -Page * Width + (dragging ? dragOffset : 0f);
            Vector2 p = content.anchoredPosition;
            p.x = dragging ? target : Mathf.Lerp(p.x, target, 1f - Mathf.Exp(-14f * Time.unscaledDeltaTime));
            content.anchoredPosition = p;
        }
    }
}
