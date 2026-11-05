using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gotcha.UI
{
    /// <summary>
    /// Barra dei puntini del selettore livelli usata come cursore (0.81): toccando o trascinando il dito lungo la
    /// barra si salta subito alla pagina corrispondente (da 1-10 a 271-280 in un gesto), con un piccolo "tic" a ogni
    /// pagina. La posizione viene da <see cref="Logic.PageScrub"/> (senza Unity, testata).
    /// </summary>
    public class PageScrubber : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public PagerSwipe pager;
        public event Action<bool> ScrubbingChanged;

        RectTransform rt;

        void Awake() { rt = (RectTransform)transform; }

        public void OnPointerDown(PointerEventData e) { ScrubbingChanged?.Invoke(true); Jump(e); }
        public void OnDrag(PointerEventData e) => Jump(e);
        public void OnPointerUp(PointerEventData e) => ScrubbingChanged?.Invoke(false);

        void Jump(PointerEventData e)
        {
            if (pager == null || rt == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, e.position, e.pressEventCamera, out Vector2 local)) return;
            Rect r = rt.rect;
            int page = Logic.PageScrub.PageAt(local.x - r.xMin, r.width, pager.PageCount);
            if (page == pager.Page) return;
            pager.SetPage(page, true);
            Gotcha.Components.Haptics.Tap();
        }
    }

    /// <summary>
    /// Tieni premuto una freccia del selettore: dopo un attimo le pagine scorrono da sole, sempre più veloci.
    /// Il tocco singolo resta quello del pulsante (una pagina).
    /// </summary>
    public class HoldRepeat : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Action step;
        bool held, repeated;
        float timer, interval;

        /// <summary>Il click del pulsante va ignorato se la pressione ha già fatto scorrere le pagine.</summary>
        public bool ConsumeClick()
        {
            bool skip = repeated;
            repeated = false;
            return skip;
        }

        public void OnPointerDown(PointerEventData e) { held = true; repeated = false; timer = 0.4f; interval = 0.16f; }
        public void OnPointerUp(PointerEventData e) => held = false;
        public void OnPointerExit(PointerEventData e) => held = false;
        void OnDisable() { held = false; repeated = false; }

        void Update()
        {
            if (!held) return;
            timer -= Time.unscaledDeltaTime;
            if (timer > 0f) return;
            repeated = true;
            step?.Invoke();
            interval = Mathf.Max(0.05f, interval * 0.8f);
            timer = interval;
        }
    }
}
