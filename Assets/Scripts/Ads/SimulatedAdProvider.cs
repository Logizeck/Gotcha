using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Gotcha.Ads
{
    /// <summary>
    /// Pubblicità simulata a schermo intero (in attesa di AdMob): un finto spot e un tasto di chiusura
    /// che diventa attivo dopo 3 secondi. Mentre è aperta il gioco è in pausa.
    /// </summary>
    public class SimulatedAdProvider : IAdProvider
    {
        public float closeAfterSeconds = 3f;

        public bool IsReady => true;

        public void Show(Action onClosed)
        {
            var go = new GameObject("SimulatedAd");
            go.AddComponent<SimulatedAdOverlay>().Show(closeAfterSeconds, onClosed);
        }
    }

    public class SimulatedAdOverlay : MonoBehaviour
    {
        static readonly Color Ink = new Color(0.17f, 0.13f, 0.2f);

        Action onClosed;
        Button closeButton;
        Text closeLabel;
        float previousTimeScale;

        public void Show(float closeAfter, Action onClosed)
        {
            this.onClosed = onClosed;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            Build();
            StartCoroutine(Countdown(closeAfter));
        }

        IEnumerator Countdown(float seconds)
        {
            closeButton.interactable = false;
            for (float t = seconds; t > 0f; t -= Time.unscaledDeltaTime)
            {
                closeLabel.text = "CLOSE IN " + Mathf.CeilToInt(t);
                yield return null;
            }
            closeLabel.text = "CLOSE  X";
            closeButton.interactable = true;
        }

        void Close()
        {
            Time.timeScale = previousTimeScale;
            AudioListener.pause = false;
            Destroy(gameObject);
            onClosed?.Invoke();
        }

        void Build()
        {
            Font display = Resources.Load<Font>("Fonts/LuckiestGuy-Regular") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Font body = Resources.Load<Font>("Fonts/PatrickHand-Regular") ?? display;

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;   // sopra tutto
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            RectTransform bg = Rect("Background", transform, Vector2.zero, Vector2.one);
            bg.gameObject.AddComponent<Image>().color = new Color(0.09f, 0.1f, 0.2f);

            Text tag = Label(bg, "ADVERTISEMENT", body, 40, new Color(1f, 1f, 1f, 0.6f));
            Place(tag.rectTransform, new Vector2(0.05f, 0.9f), new Vector2(0.95f, 0.96f));

            RectTransform card = Rect("Card", bg, new Vector2(0.08f, 0.3f), new Vector2(0.92f, 0.82f));
            card.gameObject.AddComponent<Image>().color = new Color(1f, 0.82f, 0.25f);
            Text title = Label(card, "SUPER SNACK!", display, 110, Ink);
            Place(title.rectTransform, new Vector2(0.05f, 0.6f), new Vector2(0.95f, 0.9f));
            Text pitch = Label(card, "The crunchiest snack in the universe.\n(This is a simulated ad.)", body, 54, Ink);
            Place(pitch.rectTransform, new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.55f));

            RectTransform btn = Rect("Close", bg, new Vector2(0.2f, 0.1f), new Vector2(0.8f, 0.2f));
            Image btnImage = btn.gameObject.AddComponent<Image>();
            btnImage.color = new Color(1f, 0.36f, 0.42f);
            closeButton = btn.gameObject.AddComponent<Button>();
            closeButton.targetGraphic = btnImage;
            ColorBlock colors = closeButton.colors;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.55f, 1f);
            colors.colorMultiplier = 1f;
            closeButton.colors = colors;
            closeButton.onClick.AddListener(Close);
            closeLabel = Label(btn, "", display, 60, Color.white);
            Place(closeLabel.rectTransform, Vector2.zero, Vector2.one);
        }

        static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            Place(rt, min, max);
            return rt;
        }

        static void Place(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static Text Label(Transform parent, string value, Font font, int size, Color color)
        {
            RectTransform rt = Rect("Text", parent, Vector2.zero, Vector2.one);
            Text t = rt.gameObject.AddComponent<Text>();
            t.font = font;
            t.text = value;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = 20;
            t.resizeTextMaxSize = size;
            t.raycastTarget = false;
            return t;
        }
    }
}
