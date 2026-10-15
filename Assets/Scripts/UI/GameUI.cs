using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Gotcha.Core;
using Gotcha.Levels;

namespace Gotcha.UI
{
    /// <summary>
    /// Interfaccia di gioco costruita da codice: barra in alto (Menu, titolo, Hint),
    /// pannelli Hint, Menu, Esito livello e Fine gioco. Si aggancia agli eventi di GameFlow.
    /// Menu e Hint mettono in pausa il gioco; il tasto Indietro di Android apre/chiude il menu.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        /// <summary>Versione mostrata nel MENU: serve a capire al volo quale build è installata sul telefono.</summary>
        public const string Version = "0.11";

        [SerializeField] GameFlow flow;
        [SerializeField] Sprite roundedSprite;   // 9-slice, per pulsanti e pannelli

        // Palette "doodle": inchiostro scuro, corallo, verde acqua, carta
        static readonly Color Accent = new Color(1f, 0.36f, 0.42f);
        static readonly Color Dark = new Color(0.17f, 0.75f, 0.71f);
        static readonly Color Ink = new Color(0.17f, 0.13f, 0.2f);
        static readonly Color CardColor = new Color(1f, 0.98f, 0.92f, 1f);

        Font displayFont, bodyFont;
        RectTransform safeRoot;
        Rect lastSafeArea;

        Text titleText;
        GameObject hintPanel, menuPanel, resultPanel, completedPanel;
        Text hintBody, menuEggs, resultTitle, resultBody, resultButton, musicLabel, shareLabel;
        Button shareButton;
        RectTransform levelList;

        void Awake()
        {
            if (flow == null) flow = GameFlow.Instance;
            Font fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            displayFont = Resources.Load<Font>("Fonts/LuckiestGuy-Regular") ?? fallback;
            bodyFont = Resources.Load<Font>("Fonts/PatrickHand-Regular") ?? fallback;
            Build();

            flow.LevelStarted += OnLevelStarted;
            flow.LevelFinished += OnLevelFinished;
            flow.GameCompleted += OnGameCompleted;
        }

        void Start()
        {
            // Senza EventSystem i pulsanti non rispondono; di norma è già nella scena
            if (EventSystem.current == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        void OnDestroy()
        {
            if (flow == null) return;
            flow.LevelStarted -= OnLevelStarted;
            flow.LevelFinished -= OnLevelFinished;
            flow.GameCompleted -= OnGameCompleted;
        }

        void Update()
        {
            if (Screen.safeArea != lastSafeArea) ApplySafeArea();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (hintPanel.activeSelf) CloseOverlay(hintPanel);
                else if (menuPanel.activeSelf) CloseOverlay(menuPanel);
                else if (!resultPanel.activeSelf && !completedPanel.activeSelf) OpenMenu();
            }
        }

        // ---------- Eventi di GameFlow ----------

        void OnLevelStarted(LevelDefinition def, int index)
        {
            titleText.text = def.title;
            hintPanel.SetActive(false);
            menuPanel.SetActive(false);
            resultPanel.SetActive(false);
            completedPanel.SetActive(false);
        }

        void OnLevelFinished(LevelDefinition def, LevelResult result, string eggId)
        {
            hintPanel.SetActive(false);
            menuPanel.SetActive(false);
            flow.Paused = false;

            bool solved = result == LevelResult.Solved;
            resultTitle.text = solved ? "SOLVED!" : "GOTCHA!";
            if (solved) resultBody.text = "Nice one.";
            else if (!string.IsNullOrEmpty(eggId)) resultBody.text = "You found a secret!\nEaster eggs: " + SaveData.EasterEggCount;
            else resultBody.text = "Not quite. Try again.";
            resultButton.text = solved ? "NEXT" : "RETRY";
            // video da condividere (es. il pupazzo che si scioglie)
            bool clip = !string.IsNullOrEmpty(eggId) && ClipRecorder.Instance != null && ClipRecorder.Instance.HasClip;
            shareButton.gameObject.SetActive(clip);
            shareLabel.text = "SHARE VIDEO";
            shareButton.interactable = true;
            resultPanel.SetActive(true);
        }

        void ShareClip()
        {
            if (ClipRecorder.Instance == null || ClipRecorder.Instance.IsSharing) return;
            shareButton.interactable = false;
            shareLabel.text = "PREPARING...";
            ClipRecorder.Instance.Share("I just melted a snowman in Gotcha! Can you beat it? #GotchaGame", ok =>
            {
                shareLabel.text = ok ? "SHARE AGAIN" : "SHARE FAILED";
                shareButton.interactable = true;
            });
        }

        void OnGameCompleted()
        {
            resultPanel.SetActive(false);
            completedPanel.SetActive(true);
        }

        // ---------- Azioni ----------

        void OpenHint()
        {
            if (flow.CurrentDefinition == null || resultPanel.activeSelf) return;
            hintBody.text = flow.CurrentDefinition.hint;
            menuPanel.SetActive(false);
            hintPanel.SetActive(true);
            flow.Paused = true;
        }

        void OpenMenu()
        {
            if (resultPanel.activeSelf || completedPanel.activeSelf) return;
            RebuildLevelList();
            menuEggs.text = "Easter eggs found: " + SaveData.EasterEggCount + "   ·   v" + Version;
            RefreshMusicLabel();
            hintPanel.SetActive(false);
            menuPanel.SetActive(true);
            flow.Paused = true;
        }

        void ToggleMusic()
        {
            if (MusicPlayer.Instance == null) return;
            MusicPlayer.Instance.SetMuted(!MusicPlayer.Instance.Muted);
            RefreshMusicLabel();
        }

        void RefreshMusicLabel()
        {
            if (musicLabel == null) return;
            bool on = MusicPlayer.Instance != null && !MusicPlayer.Instance.Muted;
            musicLabel.text = on ? "MUSIC: ON" : "MUSIC: OFF";
        }

        void CloseOverlay(GameObject panel)
        {
            panel.SetActive(false);
            flow.Paused = false;
        }

        void RebuildLevelList()
        {
            for (int i = levelList.childCount - 1; i >= 0; i--) Destroy(levelList.GetChild(i).gameObject);

            int unlocked = Mathf.Min(SaveData.HighestUnlocked, flow.LevelCount - 1);
            for (int i = 0; i <= unlocked; i++)
            {
                int index = i;
                LevelDefinition def = flow.GetDefinition(i);
                string label = (i + 1) + ". " + (def != null ? def.title : "?");
                Button b = CreateButton(levelList, label, i == flow.CurrentIndex ? Accent : Dark, () =>
                {
                    menuPanel.SetActive(false);
                    flow.StartLevel(index);
                });
                b.gameObject.AddComponent<LayoutElement>().preferredHeight = 105;
            }
        }

        // ---------- Costruzione ----------

        void Build()
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            safeRoot = CreateRect("SafeArea", canvasGo.transform, Vector2.zero, Vector2.one);
            ApplySafeArea();

            // Barra in alto
            RectTransform bar = CreateRect("TopBar", safeRoot, new Vector2(0, 1), new Vector2(1, 1));
            bar.offsetMin = new Vector2(30, -170);
            bar.offsetMax = new Vector2(-30, -30);

            Button menu = CreateButton(bar, "MENU", Dark, OpenMenu);
            Place(menu.transform, new Vector2(0, 0), new Vector2(0.27f, 1));
            Button hint = CreateButton(bar, "HINT", Accent, OpenHint);
            Place(hint.transform, new Vector2(0.73f, 0), new Vector2(1, 1));
            titleText = CreateText(bar, "", 54, Ink, FontStyle.Bold);
            Place(titleText.transform, new Vector2(0.28f, 0), new Vector2(0.72f, 1));

            // Hint
            RectTransform hintCard;
            hintPanel = CreateOverlay("HintPanel", out hintCard);
            AddHeader(hintCard, "HINT");
            hintBody = CreateText(hintCard, "", 52, Ink, FontStyle.Normal);
            Place(hintBody.transform, new Vector2(0.08f, 0.3f), new Vector2(0.92f, 0.78f));
            Place(CreateButton(hintCard, "OK", Accent, () => CloseOverlay(hintPanel)).transform, new Vector2(0.25f, 0.06f), new Vector2(0.75f, 0.24f));

            // Menu
            RectTransform menuCard;
            menuPanel = CreateOverlay("MenuPanel", out menuCard, 1500);
            AddHeader(menuCard, "MENU");
            menuEggs = CreateText(menuCard, "", 46, Ink, FontStyle.Normal);
            Place(menuEggs.transform, new Vector2(0.08f, 0.76f), new Vector2(0.92f, 0.84f));
            levelList = CreateRect("Levels", menuCard, new Vector2(0.1f, 0.27f), new Vector2(0.9f, 0.75f));
            VerticalLayoutGroup layout = levelList.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            Button music = CreateButton(menuCard, "MUSIC: ON", Dark, ToggleMusic);
            musicLabel = music.GetComponentInChildren<Text>();
            Place(music.transform, new Vector2(0.25f, 0.155f), new Vector2(0.75f, 0.235f));
            Place(CreateButton(menuCard, "RESUME", Accent, () => CloseOverlay(menuPanel)).transform, new Vector2(0.25f, 0.05f), new Vector2(0.75f, 0.13f));

            // Esito livello
            RectTransform resultCard;
            resultPanel = CreateOverlay("ResultPanel", out resultCard);
            resultTitle = AddHeader(resultCard, "");
            resultTitle.fontSize = 90;
            resultTitle.color = Accent;
            resultBody = CreateText(resultCard, "", 52, Ink, FontStyle.Normal);
            Place(resultBody.transform, new Vector2(0.08f, 0.44f), new Vector2(0.92f, 0.72f));
            shareButton = CreateButton(resultCard, "SHARE VIDEO", Dark, ShareClip);
            shareLabel = shareButton.GetComponentInChildren<Text>();
            Place(shareButton.transform, new Vector2(0.15f, 0.27f), new Vector2(0.85f, 0.41f));
            shareButton.gameObject.SetActive(false);
            Button next = CreateButton(resultCard, "", Accent, () =>
            {
                resultPanel.SetActive(false);
                flow.Continue();
            });
            resultButton = next.GetComponentInChildren<Text>();
            Place(next.transform, new Vector2(0.25f, 0.06f), new Vector2(0.75f, 0.24f));

            // Fine gioco
            RectTransform doneCard;
            completedPanel = CreateOverlay("CompletedPanel", out doneCard);
            AddHeader(doneCard, "YOU DID IT!");
            Text doneBody = CreateText(doneCard, "All levels solved.\nMore coming soon!", 52, Ink, FontStyle.Normal);
            Place(doneBody.transform, new Vector2(0.08f, 0.3f), new Vector2(0.92f, 0.7f));
            Place(CreateButton(doneCard, "PLAY AGAIN", Accent, () =>
            {
                completedPanel.SetActive(false);
                flow.StartLevel(0);
            }).transform, new Vector2(0.2f, 0.06f), new Vector2(0.8f, 0.24f));
        }

        void ApplySafeArea()
        {
            lastSafeArea = Screen.safeArea;
            if (safeRoot == null || Screen.width <= 0 || Screen.height <= 0) return;
            safeRoot.anchorMin = new Vector2(lastSafeArea.xMin / Screen.width, lastSafeArea.yMin / Screen.height);
            safeRoot.anchorMax = new Vector2(lastSafeArea.xMax / Screen.width, lastSafeArea.yMax / Screen.height);
            safeRoot.offsetMin = safeRoot.offsetMax = Vector2.zero;
        }

        /// <summary>Velo scuro a tutto schermo (blocca i tocchi sul livello) con una scheda centrale.</summary>
        GameObject CreateOverlay(string name, out RectTransform card, float cardHeight = 800)
        {
            RectTransform veil = CreateRect(name, safeRoot.parent, Vector2.zero, Vector2.one);
            Image dim = veil.gameObject.AddComponent<Image>();
            dim.color = new Color(Ink.r, Ink.g, Ink.b, 0.55f);

            card = CreateRect("Card", veil, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            card.sizeDelta = new Vector2(900, cardHeight);
            Image bg = card.gameObject.AddComponent<Image>();
            SetSprite(bg);
            bg.color = CardColor;

            veil.gameObject.SetActive(false);
            return veil.gameObject;
        }

        Text AddHeader(RectTransform card, string label)
        {
            Text t = CreateText(card, label, 70, Ink, FontStyle.Bold);
            RectTransform rt = (RectTransform)t.transform;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(40, -190);
            rt.offsetMax = new Vector2(-40, -40);
            return t;
        }

        Button CreateButton(Transform parent, string label, Color color, Action onClick)
        {
            RectTransform rt = CreateRect("Button " + label, parent, Vector2.zero, Vector2.one);
            Image img = rt.gameObject.AddComponent<Image>();
            SetSprite(img);
            img.color = color;
            // ombra da cartone e leggera inclinazione "fatta a mano"
            Shadow shadow = rt.gameObject.AddComponent<Shadow>();
            shadow.effectColor = Ink;
            shadow.effectDistance = new Vector2(6, -7);
            rt.localRotation = Quaternion.Euler(0, 0, ((label.GetHashCode() & 7) - 3.5f) * 0.7f);
            Button b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            b.onClick.AddListener(() => onClick());

            Text t = CreateText(rt, label, 48, Color.white, FontStyle.Bold);
            Place(t.transform, Vector2.zero, Vector2.one);
            t.raycastTarget = false;
            return b;
        }

        Text CreateText(Transform parent, string value, int size, Color color, FontStyle style)
        {
            RectTransform rt = CreateRect("Text", parent, Vector2.zero, Vector2.one);
            Text t = rt.gameObject.AddComponent<Text>();
            t.font = style == FontStyle.Bold ? displayFont : bodyFont;
            t.text = value;
            t.fontSize = size;
            t.fontStyle = FontStyle.Normal;   // i font hanno già il loro peso
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

        void SetSprite(Image img)
        {
            if (roundedSprite == null) return;
            img.sprite = roundedSprite;
            img.type = Image.Type.Sliced;
        }

        static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        static void Place(Transform t, Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform rt = (RectTransform)t;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
