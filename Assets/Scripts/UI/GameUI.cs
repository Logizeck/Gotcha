using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Gotcha.Core;
using Gotcha.Sensors;
using Gotcha.Levels;

namespace Gotcha.UI
{
    /// <summary>
    /// Interfaccia di gioco costruita da codice: schermata del titolo (New Game, Level Selection, Settings),
    /// barra in alto (Menu, titolo, Hint), pannelli Hint, Menu, Impostazioni, Esito livello e Fine gioco. Si aggancia agli eventi di GameFlow.
    /// Menu e Hint mettono in pausa il gioco; il tasto Indietro di Android apre/chiude il menu.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        /// <summary>Versione mostrata nel MENU: serve a capire al volo quale build è installata sul telefono.</summary>
        public const string Version = "0.25";

        [SerializeField] GameFlow flow;
        [SerializeField] Sprite roundedSprite;   // 9-slice, per pulsanti e pannelli
        [SerializeField] Sprite heartSprite;     // cuori delle vite
        [SerializeField] Sprite logoSprite;      // logo della schermata del titolo

        // Palette "doodle": inchiostro scuro, corallo, verde acqua, carta
        static readonly Color Accent = new Color(1f, 0.36f, 0.42f);
        static readonly Color Dark = new Color(0.17f, 0.75f, 0.71f);
        static readonly Color Ink = new Color(0.17f, 0.13f, 0.2f);
        static readonly Color CardColor = new Color(1f, 0.98f, 0.92f, 1f);
        static readonly Color EggColor = new Color(0.58f, 0.36f, 0.9f);
        static readonly Color LockedColor = new Color(0.72f, 0.7f, 0.75f);
        const int LevelsPerPage = 10;

        Font displayFont, bodyFont;
        RectTransform safeRoot;
        Rect lastSafeArea;

        Text titleText;
        GameObject hintPanel, menuPanel, resultPanel, completedPanel, titlePanel, settingsPanel;
        Text menuHeader, menuBackLabel, continueLabel, settingsMusicLabel, settingsInfo, resetLabel;
        GameObject continueButton, menuMusicRow;
        bool menuFromTitle, resetArmed;
        Text hintBody, menuEggs, resultTitle, resultBody, resultButton, musicLabel, shareLabel;
        Button shareButton;
        RectTransform pagesContent;
        PagerSwipe pager;
        RectTransform pageDots;
        Text pageLabel;

        // vite
        Image[] hearts;
        Text livesTimer;
        GameObject livesPanel;
        Text livesTitle, livesBody, playLabel;
        Button adButton, playButton;
        int pendingLevel = -1;
        int brokenHeart = -1;
        float brokenAt;
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
            flow.OutOfLives += OnOutOfLives;
            ShowTitle();
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
            flow.OutOfLives -= OnOutOfLives;
            if (flow.Lives != null) flow.Lives.Changed -= OnLivesChanged;
        }

        void Update()
        {
            if (Screen.safeArea != lastSafeArea) ApplySafeArea();
            if (!livesHooked && flow.Lives != null) { flow.Lives.Changed += OnLivesChanged; livesHooked = true; }
            UpdateHearts();
            if (livesPanel.activeSelf) RefreshLivesPanel();
            if (menuPanel.activeSelf || settingsPanel.activeSelf) RefreshMusicLabel();   // si aggiorna se alzi il volume col menu aperto

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (livesPanel.activeSelf) CloseLives();
                else if (settingsPanel.activeSelf) CloseSettings();
                else if (hintPanel.activeSelf) CloseOverlay(hintPanel);
                else if (menuPanel.activeSelf) CloseMenu();
                else if (titlePanel.activeSelf) { }
                else if (!resultPanel.activeSelf && !completedPanel.activeSelf) OpenMenu();
            }
        }

        // ---------- Eventi di GameFlow ----------

        void OnLevelStarted(LevelDefinition def, int index)
        {
            titleText.text = def.title;
            titlePanel.SetActive(false);
            settingsPanel.SetActive(false);
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
            if (solved) resultBody.text = string.IsNullOrEmpty(eggId) ? "Nice one." : "Solved... AND you found a secret!\nEaster eggs: " + SaveData.EasterEggCount;
            else if (!string.IsNullOrEmpty(eggId)) resultBody.text = "You found a secret!\nEaster eggs: " + SaveData.EasterEggCount;
            else
            {
                int lives = flow.Lives != null ? flow.Lives.Lives : 1;
                resultBody.text = lives > 0 ? "Not quite. Try again.\nLives left: " + lives : "Not quite...\nand you're out of lives!";
            }
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
            string msg = flow.CurrentDefinition != null && flow.CurrentDefinition.easterEggId == "mecha_robot"
                ? "My robot just evolved into a battle mech in Gotcha! #GotchaGame"
                : "I just found a secret in Gotcha! Can you find it too? #GotchaGame";
            ClipRecorder.Instance.Share(msg, ok =>
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

        // ---------- Schermata del titolo ----------

        void ShowTitle()
        {
            flow.StopLevel();
            titleText.text = "";
            hintPanel.SetActive(false);
            menuPanel.SetActive(false);
            resultPanel.SetActive(false);
            completedPanel.SetActive(false);
            settingsPanel.SetActive(false);
            RefreshTitle();
            titlePanel.SetActive(true);
        }

        void RefreshTitle()
        {
            int resume = flow.ResumeIndex;
            continueButton.SetActive(resume > 0);
            continueLabel.text = "CONTINUE (LEVEL " + (resume + 1) + ")";
        }

        void NewGame() { flow.TryStartLevel(0); }

        void ContinueGame() { flow.TryStartLevel(flow.ResumeIndex); }

        void OpenLevelSelection()
        {
            menuFromTitle = true;
            ShowMenu();
        }

        void OpenSettings()
        {
            resetArmed = false;
            resetLabel.text = "RESET PROGRESS";
            RefreshMusicLabel();
            settingsInfo.text = "Levels: " + Mathf.Min(SaveData.HighestUnlocked + 1, flow.LevelCount) + " / " + flow.LevelCount +
                "\nEaster eggs found: " + SaveData.EasterEggCount + "\nVersion " + Version;
            settingsPanel.SetActive(true);
            if (!titlePanel.activeSelf) flow.Paused = true;
        }

        void CloseSettings()
        {
            settingsPanel.SetActive(false);
            if (!titlePanel.activeSelf && !menuPanel.activeSelf) flow.Paused = false;
        }

        /// <summary>Primo tocco: chiede conferma; secondo tocco: azzera livelli sbloccati ed Easter egg.</summary>
        void ResetProgress()
        {
            if (!resetArmed) { resetArmed = true; resetLabel.text = "TAP AGAIN TO CONFIRM"; return; }
            SaveData.ResetProgress();
            OpenSettings();
            resetLabel.text = "PROGRESS RESET";
            RefreshTitle();
        }

        void CloseMenu()
        {
            menuPanel.SetActive(false);
            if (!menuFromTitle) flow.Paused = false;
        }

        void OpenMenu()
        {
            if (resultPanel.activeSelf || completedPanel.activeSelf || titlePanel.activeSelf) return;
            menuFromTitle = false;
            ShowMenu();
        }

        void ShowMenu()
        {
            menuHeader.text = menuFromTitle ? "LEVELS" : "MENU";
            menuBackLabel.text = menuFromTitle ? "BACK" : "RESUME";
            menuMusicRow.SetActive(!menuFromTitle);
            RebuildLevelList();
            menuEggs.text = "Easter eggs found: " + SaveData.EasterEggCount + "   ·   v" + Version;
            RefreshMusicLabel();
            hintPanel.SetActive(false);
            menuPanel.SetActive(true);
            if (!menuFromTitle) flow.Paused = true;
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
            // se la musica è accesa ma il telefono è a volume zero (es. dopo il livello del gatto) lo diciamo,
            // altrimenti sembra che l'interruttore funzioni al contrario
            bool phoneSilent = SensorHub.Instance != null && SensorHub.Instance.SystemVolume01 <= 0.01f;
            string label = !on ? "MUSIC: OFF" : phoneSilent ? "MUSIC: ON (PHONE VOLUME 0)" : "MUSIC: ON";
            musicLabel.text = label;
            if (settingsMusicLabel != null) settingsMusicLabel.text = label;
        }

        void CloseOverlay(GameObject panel)
        {
            panel.SetActive(false);
            flow.Paused = false;
        }

        // ---------- Selezione livelli a pagine ----------

        void RebuildLevelList()
        {
            for (int i = pagesContent.childCount - 1; i >= 0; i--) Destroy(pagesContent.GetChild(i).gameObject);
            for (int i = pageDots.childCount - 1; i >= 0; i--) Destroy(pageDots.GetChild(i).gameObject);

            int count = flow.LevelCount;
            int pages = Mathf.Max(1, (count + LevelsPerPage - 1) / LevelsPerPage);
            int unlocked = Mathf.Min(SaveData.HighestUnlocked, count - 1);
            for (int p = 0; p < pages; p++)
            {
                RectTransform page = CreateRect("Page" + p, pagesContent, new Vector2(p, 0f), new Vector2(p + 1, 1f));
                for (int k = 0; k < LevelsPerPage; k++)
                {
                    int index = p * LevelsPerPage + k;
                    if (index >= count) break;
                    int col = k % 2, row = k / 2;
                    RectTransform cell = LevelCell(page, index, GameFlow.UnlockAllLevels || index <= unlocked);
                    Place(cell, new Vector2(col * 0.5f + 0.015f, 1f - (row + 1) * 0.2f + 0.012f), new Vector2((col + 1) * 0.5f - 0.015f, 1f - row * 0.2f - 0.012f));
                }
                Image dot = CreateRect("Dot" + p, pageDots, Vector2.zero, Vector2.zero).gameObject.AddComponent<Image>();
                SetSprite(dot);
                dot.raycastTarget = false;
            }
            pager.PageCount = pages;
            int current = Mathf.Max(0, menuFromTitle ? flow.ResumeIndex : flow.CurrentIndex) / LevelsPerPage;
            pager.SetPage(current, true);
            LayoutDots();
        }

        RectTransform LevelCell(Transform page, int index, bool open)
        {
            LevelDefinition def = flow.GetDefinition(index);
            bool egg = def != null && !string.IsNullOrEmpty(def.easterEggId);
            bool found = egg && SaveData.HasEasterEgg(def.easterEggId);
            bool here = menuFromTitle ? index == flow.ResumeIndex : index == flow.CurrentIndex;
            Color color = !open ? LockedColor : here ? Accent : egg ? EggColor : Dark;
            Button b = CreateButton(page, "", color, () =>
            {
                menuPanel.SetActive(false);
                flow.Paused = false;
                flow.TryStartLevel(index);
            });
            b.interactable = open;
            b.transform.localRotation = Quaternion.identity;
            Text label = b.GetComponentInChildren<Text>();
            label.text = (index + 1).ToString();
            label.fontSize = 64;
            label.resizeTextMaxSize = 64;
            label.alignment = TextAnchor.MiddleLeft;
            Place(label.transform, new Vector2(0.06f, 0.05f), new Vector2(0.32f, 0.95f));
            Text title = CreateText(b.transform, open ? (def != null ? def.title : "?") : "LOCKED", 34, Color.white, FontStyle.Bold);
            title.alignment = TextAnchor.MiddleLeft;
            Place(title.transform, new Vector2(0.32f, egg ? 0.36f : 0.08f), new Vector2(0.97f, 0.92f));
            if (egg)
            {
                Text tag = CreateText(b.transform, found ? "EGG FOUND!" : "SECRET EGG", 24, new Color(1f, 0.92f, 0.5f), FontStyle.Normal);
                tag.alignment = TextAnchor.MiddleLeft;
                Place(tag.transform, new Vector2(0.32f, 0.06f), new Vector2(0.97f, 0.4f));
            }
            return (RectTransform)b.transform;
        }

        void OnPageChanged(int page)
        {
            if (pageLabel != null) pageLabel.text = "LEVELS " + (page * LevelsPerPage + 1) + "-" + Mathf.Min(flow.LevelCount, (page + 1) * LevelsPerPage);
            LayoutDots();
        }

        void LayoutDots()
        {
            int n = pageDots.childCount;
            for (int i = 0; i < n; i++)
            {
                RectTransform d = (RectTransform)pageDots.GetChild(i);
                bool on = pager != null && i == pager.Page;
                float size = on ? 34f : 24f;
                d.anchorMin = d.anchorMax = new Vector2(0.5f, 0.5f);
                d.sizeDelta = new Vector2(size, size);
                d.anchoredPosition = new Vector2((i - (n - 1) * 0.5f) * 50f, 0f);
                d.GetComponent<Image>().color = on ? Accent : new Color(Ink.r, Ink.g, Ink.b, 0.3f);
            }
        }

        // ---------- Vite ----------

        bool livesHooked;

        void UpdateHearts()
        {
            if (hearts == null || flow.Lives == null) return;
            int lives = flow.Lives.Lives;
            float t = Time.unscaledTime;
            for (int i = 0; i < hearts.Length; i++)
            {
                RectTransform rt = (RectTransform)hearts[i].transform;
                bool full = i < lives;
                if (i == brokenHeart && t - brokenAt < 0.8f)
                {
                    // il cuore appena perso: si gonfia, trema e si spegne
                    float k = (t - brokenAt) / 0.8f;
                    rt.localScale = Vector3.one * (1f + Mathf.Sin(k * Mathf.PI) * 0.7f);
                    rt.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 40f) * 20f * (1f - k));
                    hearts[i].color = Color.Lerp(Color.white, new Color(0.45f, 0.4f, 0.5f, 0.35f), k);
                    continue;
                }
                rt.localRotation = Quaternion.identity;
                hearts[i].color = full ? Color.white : new Color(0.45f, 0.4f, 0.5f, 0.35f);
                float beat = full ? Mathf.Pow(Mathf.Abs(Mathf.Sin(t * 2.6f + i * 0.35f)), 8f) * 0.18f : 0f;   // battito
                rt.localScale = Vector3.one * (1f + beat);
            }
            livesTimer.text = lives >= LivesManager.MaxLives ? "FULL" : "+1 IN " + FormatTime(flow.Lives.SecondsToNext);
        }

        static string FormatTime(float seconds)
        {
            int s = Mathf.CeilToInt(seconds);
            return (s / 60) + ":" + (s % 60).ToString("00");
        }

        void OnLivesChanged(int before, int after)
        {
            if (after < before) { brokenHeart = after; brokenAt = Time.unscaledTime; }
        }

        void OnOutOfLives(int level)
        {
            pendingLevel = level;
            resultPanel.SetActive(false);
            menuPanel.SetActive(false);
            hintPanel.SetActive(false);
            OpenLives(true);
        }

        void OpenLives(bool outOfLives)
        {
            livesTitle.text = outOfLives ? "OUT OF LIVES!" : "LIVES";
            RefreshLivesPanel();
            livesPanel.SetActive(true);
            flow.Paused = true;
        }

        void RefreshLivesPanel()
        {
            LivesManager lm = flow.Lives;
            if (lm == null) return;
            bool full = lm.Lives >= LivesManager.MaxLives;
            livesBody.text = "Lives: " + lm.Lives + " / " + LivesManager.MaxLives + "\n" +
                (full ? "Your hearts are full!" : "Next life in " + FormatTime(lm.SecondsToNext)) +
                "\nInvite a friend: +1 life when they join!";
            adButton.interactable = !full;
            bool canPlay = pendingLevel >= 0 && lm.CanPlay;
            bool atTitle = titlePanel.activeSelf;   // dal titolo si può sempre tornare indietro
            playButton.interactable = pendingLevel < 0 || canPlay || atTitle;
            playLabel.text = pendingLevel >= 0 ? (canPlay ? "PLAY!" : atTitle ? "BACK" : "WAITING...") : "CLOSE";
        }

        void WatchAdForLife()
        {
            adButton.interactable = false;
            flow.Ads.ShowRewarded(ok =>
            {
                if (ok && flow.Lives != null) flow.Lives.GrantLife();
                RefreshLivesPanel();
            });
        }

        void CloseLives()
        {
            if (pendingLevel >= 0)
            {
                if (!flow.Lives.CanPlay)
                {
                    if (!titlePanel.activeSelf) return;   // senza vite non si gioca
                    pendingLevel = -1;
                    CloseOverlay(livesPanel);
                    return;
                }
                int level = pendingLevel;
                pendingLevel = -1;
                livesPanel.SetActive(false);
                flow.Paused = false;
                flow.StartLevel(level);
                return;
            }
            CloseOverlay(livesPanel);
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

            // Vite: cuori pulsanti sotto la barra (toccandoli si apre il pannello delle vite)
            RectTransform heartsRow = CreateRect("Lives", safeRoot, new Vector2(0, 1), new Vector2(1, 1));
            heartsRow.offsetMin = new Vector2(30, -265);
            heartsRow.offsetMax = new Vector2(-30, -180);
            Image rowHit = heartsRow.gameObject.AddComponent<Image>();
            rowHit.color = new Color(0, 0, 0, 0);
            Button rowButton = heartsRow.gameObject.AddComponent<Button>();
            rowButton.transition = Selectable.Transition.None;
            rowButton.onClick.AddListener(() => { if (!resultPanel.activeSelf && !completedPanel.activeSelf) OpenLives(false); });
            hearts = new Image[LivesManager.MaxLives];
            for (int i = 0; i < hearts.Length; i++)
            {
                RectTransform h = CreateRect("Heart" + i, heartsRow, new Vector2(0, 0.5f), new Vector2(0, 0.5f));
                h.sizeDelta = new Vector2(76, 76);
                h.anchoredPosition = new Vector2(55 + i * 88, 0);
                hearts[i] = h.gameObject.AddComponent<Image>();
                hearts[i].sprite = heartSprite;
                hearts[i].preserveAspect = true;
                hearts[i].raycastTarget = false;
            }
            livesTimer = CreateText(heartsRow, "", 40, Ink, FontStyle.Bold);
            livesTimer.alignment = TextAnchor.MiddleRight;
            Place(livesTimer.transform, new Vector2(0.55f, 0), new Vector2(1, 1));

            // Schermata del titolo (sopra barra e cuori, sotto gli altri pannelli)
            RectTransform title = CreateRect("TitleScreen", safeRoot.parent, Vector2.zero, Vector2.one);
            Image paper = title.gameObject.AddComponent<Image>();
            paper.color = new Color(1f, 0.97f, 0.9f);
            titlePanel = title.gameObject;
            RectTransform titleArea = CreateRect("Content", title, Vector2.zero, Vector2.one);
            titleArea.anchorMin = safeRoot.anchorMin;
            titleArea.anchorMax = safeRoot.anchorMax;
            if (logoSprite != null)
            {
                Image logo = CreateRect("Logo", titleArea, new Vector2(0.24f, 0.62f), new Vector2(0.76f, 0.9f)).gameObject.AddComponent<Image>();
                logo.sprite = logoSprite;
                logo.preserveAspect = true;
                logo.raycastTarget = false;
            }
            Text big = CreateText(titleArea, "GOTCHA!", 150, Accent, FontStyle.Bold);
            Place(big.transform, new Vector2(0.05f, logoSprite != null ? 0.5f : 0.6f), new Vector2(0.95f, logoSprite != null ? 0.62f : 0.8f));
            big.transform.localRotation = Quaternion.Euler(0, 0, 4f);
            big.gameObject.AddComponent<Shadow>().effectColor = Ink;
            Text tagline = CreateText(titleArea, "Nothing is what it seems.", 50, Ink, FontStyle.Normal);
            Place(tagline.transform, new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.5f));
            Button cont = CreateButton(titleArea, "CONTINUE", Dark, ContinueGame);
            continueLabel = cont.GetComponentInChildren<Text>();
            continueButton = cont.gameObject;
            Place(cont.transform, new Vector2(0.17f, 0.355f), new Vector2(0.83f, 0.425f));
            Place(CreateButton(titleArea, "NEW GAME", Accent, NewGame).transform, new Vector2(0.17f, 0.255f), new Vector2(0.83f, 0.335f));
            Place(CreateButton(titleArea, "LEVEL SELECTION", Dark, OpenLevelSelection).transform, new Vector2(0.17f, 0.155f), new Vector2(0.83f, 0.235f));
            Place(CreateButton(titleArea, "SETTINGS", EggColor, OpenSettings).transform, new Vector2(0.17f, 0.055f), new Vector2(0.83f, 0.135f));
            titlePanel.SetActive(false);

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
            menuHeader = AddHeader(menuCard, "MENU");
            menuEggs = CreateText(menuCard, "", 46, Ink, FontStyle.Normal);
            Place(menuEggs.transform, new Vector2(0.08f, 0.76f), new Vector2(0.92f, 0.84f));
            // livelli a pagine da 10, scorrevoli con lo swipe (o con le frecce)
            pageLabel = CreateText(menuCard, "", 40, Ink, FontStyle.Bold);
            Place(pageLabel.transform, new Vector2(0.2f, 0.715f), new Vector2(0.8f, 0.765f));
            RectTransform viewport = CreateRect("Levels", menuCard, new Vector2(0.06f, 0.315f), new Vector2(0.94f, 0.71f));
            viewport.gameObject.AddComponent<RectMask2D>();
            Image hit = viewport.gameObject.AddComponent<Image>();
            hit.color = new Color(0, 0, 0, 0);
            pagesContent = CreateRect("Pages", viewport, Vector2.zero, Vector2.one);
            pager = viewport.gameObject.AddComponent<PagerSwipe>();
            pager.viewport = viewport;
            pager.content = pagesContent;
            pager.PageChanged += OnPageChanged;
            pageDots = CreateRect("Dots", menuCard, new Vector2(0.25f, 0.255f), new Vector2(0.75f, 0.305f));
            Button prev = CreateButton(menuCard, "<", Dark, () => pager.SetPage(pager.Page - 1));
            Place(prev.transform, new Vector2(0.06f, 0.25f), new Vector2(0.2f, 0.31f));
            Button nextPage = CreateButton(menuCard, ">", Dark, () => pager.SetPage(pager.Page + 1));
            Place(nextPage.transform, new Vector2(0.8f, 0.25f), new Vector2(0.94f, 0.31f));
            RectTransform musicRow = CreateRect("Row", menuCard, new Vector2(0.06f, 0.155f), new Vector2(0.94f, 0.235f));
            menuMusicRow = musicRow.gameObject;
            Button music = CreateButton(musicRow, "MUSIC: ON", Dark, ToggleMusic);
            musicLabel = music.GetComponentInChildren<Text>();
            Place(music.transform, new Vector2(0f, 0f), new Vector2(0.62f, 1f));
            Place(CreateButton(musicRow, "HOME", EggColor, ShowTitle).transform, new Vector2(0.66f, 0f), new Vector2(1f, 1f));
            Button back = CreateButton(menuCard, "RESUME", Accent, CloseMenu);
            menuBackLabel = back.GetComponentInChildren<Text>();
            Place(back.transform, new Vector2(0.25f, 0.05f), new Vector2(0.75f, 0.13f));

            // Impostazioni
            RectTransform setCard;
            settingsPanel = CreateOverlay("SettingsPanel", out setCard, 1100);
            AddHeader(setCard, "SETTINGS");
            Button setMusic = CreateButton(setCard, "MUSIC: ON", Dark, ToggleMusic);
            settingsMusicLabel = setMusic.GetComponentInChildren<Text>();
            Place(setMusic.transform, new Vector2(0.12f, 0.66f), new Vector2(0.88f, 0.77f));
            settingsInfo = CreateText(setCard, "", 46, Ink, FontStyle.Normal);
            Place(settingsInfo.transform, new Vector2(0.08f, 0.4f), new Vector2(0.92f, 0.63f));
            Button reset = CreateButton(setCard, "RESET PROGRESS", Accent, ResetProgress);
            resetLabel = reset.GetComponentInChildren<Text>();
            Place(reset.transform, new Vector2(0.12f, 0.22f), new Vector2(0.88f, 0.33f));
            Place(CreateButton(setCard, "BACK", Dark, CloseSettings).transform, new Vector2(0.25f, 0.06f), new Vector2(0.75f, 0.16f));

            // Vite: attesa, pubblicità con premio, invito
            RectTransform livesCard;
            livesPanel = CreateOverlay("LivesPanel", out livesCard, 1100);
            livesTitle = AddHeader(livesCard, "LIVES");
            livesBody = CreateText(livesCard, "", 50, Ink, FontStyle.Normal);
            Place(livesBody.transform, new Vector2(0.08f, 0.6f), new Vector2(0.92f, 0.82f));
            adButton = CreateButton(livesCard, "WATCH AD: +1 LIFE", Accent, WatchAdForLife);
            Place(adButton.transform, new Vector2(0.12f, 0.43f), new Vector2(0.88f, 0.55f));
            Place(CreateButton(livesCard, "INVITE A FRIEND", EggColor, InviteService.Share).transform, new Vector2(0.12f, 0.27f), new Vector2(0.88f, 0.39f));
            playButton = CreateButton(livesCard, "CLOSE", Dark, CloseLives);
            playLabel = playButton.GetComponentInChildren<Text>();
            Place(playButton.transform, new Vector2(0.25f, 0.07f), new Vector2(0.75f, 0.19f));

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
            Place(CreateButton(doneCard, "TITLE SCREEN", Accent, () =>
            {
                completedPanel.SetActive(false);
                ShowTitle();
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
