using System;
using UnityEngine;
using UnityEngine.UI;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.UI
{
    /// <summary>
    /// "SECRET FOUND!": la schermata epica che precede l'esito del livello quando si trova un Easter egg.
    /// Velo scuro, raggiera dorata che gira, uovo d'oro che cade e rimbalza con l'alone, titolo che sbatte sullo
    /// schermo (tremolio, flash, coriandoli, vibrazione forte), nome del segreto, poi il pezzo nuovo del puzzle di Toni
    /// che vola nell'anteprima con il contatore che sale. Il tutto sulla fanfara EasterEggFanfare (5 s), con la musica
    /// abbassata a zero e riportata su piano alla fine.
    /// Prima scoperta: versione completa (5 s). Segreto già trovato: versione breve (~2.2 s, la fanfara parte e sfuma).
    /// Un tocco (o il tasto Indietro) salta alla fine dopo 1.5 s (0.8 s nella breve): dissolvenza rapida di scena e
    /// fanfara. L'esito del livello compare sotto, all'inizio della dissolvenza, e da lì tutto prosegue come prima.
    /// Tutti gli oggetti sono costruiti una volta in Build (coriandoli e stelle in un pool): nessuna allocazione per
    /// fotogramma. I tempi stanno in EggCelebrationTimeline (logica pura, con i test).
    /// </summary>
    public partial class GameUI
    {
        [SerializeField] AudioClip eggFanfare;          // Assets/Audio/Sfx/EasterEggFanfare.ogg (facoltativa: senza, la scena è muta)
        [SerializeField] Sprite eggGlowSprite;          // glow (facoltativi: senza, disegni procedurali o quadratini)
        [SerializeField] Sprite eggSparkleSprite;       // sparkle_star
        [SerializeField] Sprite eggStarSprite;          // goal_star
        [SerializeField] Sprite eggConfettiSprite;      // confetti

        // misure del "palco" (unità del canvas 1080x1920): viene scalato per stare tutto nell'area sicura
        const float StageW = 1000f, StageH = 1700f;
        const float EggBaseY = 60f, EggSize = 420f, EggCenterY = EggBaseY + EggSize * 0.48f;
        const float TitleY = 690f, PreviewY = -390f, CellSize = 44f;
        const int ConfettiCount = 48, StarCount = 20, TwinkleCount = 8;

        static readonly Color Gold = new Color(1f, 0.8f, 0.2f);
        static readonly Color PaleGold = new Color(1f, 0.95f, 0.66f);
        static readonly Color[] ConfettiColors =
        {
            new Color(1f, 0.8f, 0.2f), new Color(1f, 0.36f, 0.42f), new Color(0.17f, 0.75f, 0.71f),
            new Color(0.58f, 0.36f, 0.9f), Color.white, new Color(1f, 0.95f, 0.66f),
        };
        // vibrazione all'impatto del titolo: colpo secco + rombo lungo
        static readonly long[] SlamTimings = { 0, 70, 45, 300 };
        static readonly int[] SlamAmplitudes = { 0, 255, 0, 210 };

        GameObject eggFx;
        CanvasGroup eggFxGroup;
        Image eggVeil, eggFlash;
        RectTransform eggContent, eggStage, eggRays, eggRays2, eggGlow, eggIcon, eggTitleRt, eggCaptionRt, eggNameRt;
        RectTransform eggPreview, eggCounterRt, eggHintRt, eggPiece, eggFxLayer;
        Image eggRaysImg, eggRays2Img, eggGlowImg;
        Text eggTitle, eggCaption, eggName, eggCounter, eggHint;
        CanvasGroup eggPreviewGroup;
        Image[] eggCellBack;
        RawImage[] eggCellPic;
        RawImage eggPieceImg;
        RectTransform[] eggTwinkles;
        Image[] eggTwinkleImgs;
        AudioSource eggAudio;

        // pool di particelle (coriandoli + stelle), in coordinate del palco
        RectTransform[] eggParts;
        Image[] eggPartImgs;
        Vector2[] eggPartPos, eggPartVel;
        float[] eggPartAge, eggPartLife, eggPartRot, eggPartSpin, eggPartFlip;
        int eggNextConfetti, eggNextStar;

        EggCelebrationClock eggClock;
        Action eggThen;
        bool eggResultShown, eggSlammed, eggLanded, eggSpat;
        int eggTotal, eggNewPiece = -1, eggShownCount = -1;
        bool eggMusicDucked;
        float eggMusicReturn = -1f;   // secondi dalla fine della scena (la musica risale); -1 = non ancora
        int eggsAtLevelStart = -1;    // Easter egg trovati all'inizio del livello: dice se quello di adesso è nuovo

        /// <summary>La scena è in corso (anche durante la dissolvenza finale).</summary>
        bool EggCelebrating => eggFx != null && eggFx.activeSelf;

        // ---------- costruzione (una volta sola) ----------

        void BuildEggCelebration()
        {
            RectTransform veil = CreateRect("EggCelebration", safeRoot.parent, Vector2.zero, Vector2.one);
            eggFx = veil.gameObject;
            eggVeil = eggFx.AddComponent<Image>();
            eggVeil.color = new Color(0.05f, 0.03f, 0.09f, 0.92f);
            Button tap = eggFx.AddComponent<Button>();   // tocco ovunque = salta (dopo SkipAfter)
            tap.transition = Selectable.Transition.None;
            tap.onClick.AddListener(SkipEggCelebration);
            eggFxGroup = eggFx.AddComponent<CanvasGroup>();

            eggContent = CreateRect("Content", veil, safeRoot.anchorMin, safeRoot.anchorMax);
            eggStage = CreateRect("Stage", eggContent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            eggStage.sizeDelta = new Vector2(StageW, StageH);

            // raggiera: due strati che girano in versi opposti, centrati sull'uovo (sbordano: è solo luce)
            // (disegnata da codice a 512 px: lo sprite burst_rays, piccolo, ingrandito 10 volte ha i bordi a scalini)
            eggRays2Img = StageImage("Rays2", RaysSprite(), new Vector2(0f, EggCenterY), new Vector2(2000f, 2000f), out eggRays2);
            eggRays2Img.color = new Color(1f, 0.72f, 0.2f, 0.55f);
            eggRaysImg = StageImage("Rays", RaysSprite(), new Vector2(0f, EggCenterY), new Vector2(2500f, 2500f), out eggRays);
            eggRaysImg.color = new Color(1f, 0.9f, 0.55f, 0.9f);
            eggGlowImg = StageImage("Glow", eggGlowSprite != null ? eggGlowSprite : GlowFallback(), new Vector2(0f, EggCenterY), new Vector2(1000f, 1000f), out eggGlow);
            eggGlowImg.color = new Color(1f, 0.82f, 0.3f, 0.9f);

            // stelline che luccicano attorno all'uovo
            eggTwinkles = new RectTransform[TwinkleCount];
            eggTwinkleImgs = new Image[TwinkleCount];
            for (int i = 0; i < TwinkleCount; i++)
            {
                float a = i * Mathf.PI * 2f / TwinkleCount + 0.3f;
                float r = i % 2 == 0 ? 300f : 240f;
                eggTwinkleImgs[i] = StageImage("Twinkle", eggSparkleSprite != null ? eggSparkleSprite : GlowFallback(),
                    new Vector2(Mathf.Cos(a) * r * 0.9f, EggCenterY + Mathf.Sin(a) * r), new Vector2(110f, 110f), out eggTwinkles[i]);
            }

            // l'uovo d'oro (disegnato da codice), con il perno in basso per schiacciarsi all'atterraggio
            Image egg = StageImage("GoldenEgg", GoldenEggSprite(), new Vector2(0f, EggBaseY), new Vector2(EggSize, EggSize), out eggIcon);
            egg.preserveAspect = true;
            eggIcon.pivot = new Vector2(0.5f, 0.03f);

            eggTitle = StageText("SECRET FOUND!", 136, Gold, true, new Vector2(0f, TitleY), new Vector2(990f, 210f), out eggTitleRt);
            Outline outline = eggTitle.gameObject.AddComponent<Outline>();
            outline.effectColor = Ink;
            outline.effectDistance = new Vector2(5f, -5f);
            Shadow sh = eggTitle.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0.55f, 0.3f, 0f, 1f);
            sh.effectDistance = new Vector2(0f, -14f);
            eggCaption = StageText("EASTER EGG UNLOCKED!", 52, Color.white, true, new Vector2(0f, 555f), new Vector2(940f, 80f), out eggCaptionRt);
            eggName = StageText("", 86, PaleGold, true, new Vector2(0f, -40f), new Vector2(960f, 150f), out eggNameRt);
            eggName.gameObject.AddComponent<Outline>().effectColor = Ink;

            // anteprima del puzzle di Toni: 5 x 10 caselle (pool fisso, riempito a ogni scena)
            eggPreview = CreateRect("PuzzlePreview", eggStage, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            eggPreview.anchoredPosition = new Vector2(0f, PreviewY);
            eggPreview.sizeDelta = new Vector2(EggPuzzle.Cols * CellSize + 26f, EggPuzzle.Rows * CellSize + 26f);
            Image frame = eggPreview.gameObject.AddComponent<Image>();
            SetSprite(frame);
            frame.color = Gold;
            frame.raycastTarget = false;
            eggPreviewGroup = eggPreview.gameObject.AddComponent<CanvasGroup>();
            eggPreviewGroup.blocksRaycasts = false;
            Texture tex = puzzleSprite != null ? puzzleSprite.texture : null;
            eggCellBack = new Image[EggPuzzle.Pieces];
            eggCellPic = new RawImage[EggPuzzle.Pieces];
            float cw = 1f / EggPuzzle.Cols, ch = 1f / EggPuzzle.Rows;
            for (int r = 0; r < EggPuzzle.Rows; r++)
                for (int c = 0; c < EggPuzzle.Cols; c++)
                {
                    int k = r * EggPuzzle.Cols + c;
                    RectTransform cell = CreateRect("Cell", eggPreview, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                    cell.anchoredPosition = CellPos(k) - new Vector2(0f, PreviewY);
                    cell.sizeDelta = new Vector2(CellSize - 3f, CellSize - 3f);
                    Image back = cell.gameObject.AddComponent<Image>();
                    back.color = new Color(Ink.r, Ink.g, Ink.b, 0.9f);
                    back.raycastTarget = false;
                    RectTransform picRt = CreateRect("Pic", cell, Vector2.zero, Vector2.one);
                    RawImage pic = picRt.gameObject.AddComponent<RawImage>();
                    pic.texture = tex;
                    pic.uvRect = new Rect(c * cw, 1f - (r + 1) * ch, cw, ch);
                    pic.raycastTarget = false;
                    eggCellBack[k] = back;
                    eggCellPic[k] = pic;
                }

            eggCounter = StageText("", 66, Color.white, true, new Vector2(0f, -700f), new Vector2(900f, 100f), out eggCounterRt);
            eggCounter.gameObject.AddComponent<Outline>().effectColor = Ink;
            eggHint = StageText("TAP TO CONTINUE", 40, Color.white, false, new Vector2(0f, -812f), new Vector2(900f, 70f), out eggHintRt);

            // il pezzo che vola dall'uovo all'anteprima
            eggPiece = CreateRect("FlyingPiece", eggStage, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            eggPiece.sizeDelta = new Vector2(CellSize, CellSize);
            eggPieceImg = eggPiece.gameObject.AddComponent<RawImage>();
            eggPieceImg.texture = tex;
            eggPieceImg.raycastTarget = false;
            eggPiece.gameObject.AddComponent<Outline>().effectColor = Gold;

            // particelle sopra a tutto (tranne il flash)
            eggFxLayer = CreateRect("Particles", eggStage, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            int n = ConfettiCount + StarCount;
            eggParts = new RectTransform[n];
            eggPartImgs = new Image[n];
            eggPartPos = new Vector2[n];
            eggPartVel = new Vector2[n];
            eggPartAge = new float[n];
            eggPartLife = new float[n];
            eggPartRot = new float[n];
            eggPartSpin = new float[n];
            eggPartFlip = new float[n];
            for (int i = 0; i < n; i++)
            {
                bool star = i >= ConfettiCount;
                RectTransform p = CreateRect(star ? "Star" : "Confetti", eggFxLayer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                p.sizeDelta = star ? new Vector2(80f, 80f) : new Vector2(36f, 22f);
                Image img = p.gameObject.AddComponent<Image>();
                img.sprite = star ? (eggStarSprite != null ? eggStarSprite : eggSparkleSprite != null ? eggSparkleSprite : GlowFallback()) : eggConfettiSprite;
                img.preserveAspect = star;
                img.raycastTarget = false;
                img.enabled = false;
                eggParts[i] = p;
                eggPartImgs[i] = img;
            }

            // lampo bianco all'impatto, a tutto schermo
            RectTransform flash = CreateRect("Flash", veil, Vector2.zero, Vector2.one);
            eggFlash = flash.gameObject.AddComponent<Image>();
            eggFlash.color = new Color(1f, 0.97f, 0.85f, 0f);
            eggFlash.raycastTarget = false;

            var audioGo = new GameObject("EggFanfare");
            audioGo.transform.SetParent(transform, false);
            eggAudio = audioGo.AddComponent<AudioSource>();
            eggAudio.playOnAwake = false;
            eggAudio.loop = false;
            eggAudio.spatialBlend = 0f;
            eggAudio.priority = 0;   // la più importante: non deve mai essere tagliata da altri suoni

            eggFx.SetActive(false);
        }

        Image StageImage(string name, Sprite sprite, Vector2 pos, Vector2 size, out RectTransform rt)
        {
            rt = CreateRect(name, eggStage, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Image img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;
            return img;
        }

        Text StageText(string value, int size, Color color, bool display, Vector2 pos, Vector2 box, out RectTransform rt)
        {
            Text t = CreateText(eggStage, value, size, color, display ? FontStyle.Bold : FontStyle.Normal);
            rt = (RectTransform)t.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = box;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;   // il best fit rimpicciolisce i nomi lunghi invece di sbordare
            return t;
        }

        /// <summary>Centro della casella k del puzzle, in coordinate del palco.</summary>
        static Vector2 CellPos(int k)
        {
            int c = k % EggPuzzle.Cols, r = k / EggPuzzle.Cols;
            return new Vector2((c - (EggPuzzle.Cols - 1) * 0.5f) * CellSize, PreviewY + ((EggPuzzle.Rows - 1) * 0.5f - r) * CellSize);
        }

        // ---------- avvio, salto, fine ----------

        /// <summary>
        /// Fa partire la scena; <paramref name="then"/> (l'esito del livello) arriva all'inizio della dissolvenza.
        /// Se qualcosa manca (scena non costruita) si passa subito all'esito: il flusso non si blocca mai.
        /// </summary>
        void PlayEggCelebration(string eggId, bool firstTime, Action then)
        {
            if (eggFx == null || eggStage == null) { then?.Invoke(); return; }
            if (EggCelebrating) FinishEggCelebration(true);

            eggTotal = EggPuzzle.Unlocked;
            // il pezzo nuovo: solo alla prima scoperta e se il puzzle non era già pieno (verificato con EggPuzzle)
            eggNewPiece = firstTime && SaveData.EasterEggCount <= EggPuzzle.Pieces ? EggCelebrationTimeline.NewestPiece(eggTotal, EggPuzzle.Pieces) : -1;
            if (eggNewPiece >= 0 && !EggPuzzle.IsUnlocked(eggNewPiece % EggPuzzle.Cols, eggNewPiece / EggPuzzle.Cols)) eggNewPiece = -1;
            if (puzzleSprite == null) eggNewPiece = -1;   // niente immagine: niente pezzo da far volare
            EggCelebrationPlan plan = firstTime ? EggCelebrationPlan.Complete(eggNewPiece >= 0) : EggCelebrationPlan.Short();
            if (eggClock == null) eggClock = new EggCelebrationClock(plan); else eggClock.Restart(plan);

            eggThen = then;
            eggResultShown = eggSlammed = eggLanded = eggSpat = false;
            eggShownCount = -1;
            eggCaption.text = firstTime ? "EASTER EGG UNLOCKED!" : "ALREADY IN YOUR COLLECTION";
            eggName.text = "\"" + EasterEggs.Name(eggId) + "\"";
            for (int k = 0; k < EggPuzzle.Pieces; k++)
            {
                bool open = EggPuzzle.IsUnlocked(k % EggPuzzle.Cols, k / EggPuzzle.Cols);
                eggCellPic[k].enabled = open && k != eggNewPiece && eggCellPic[k].texture != null;
                eggCellBack[k].color = open && eggCellPic[k].texture == null ? Gold : new Color(Ink.r, Ink.g, Ink.b, 0.9f);
                eggCellPic[k].transform.localScale = Vector3.one;
            }
            eggPiece.gameObject.SetActive(false);
            if (eggNewPiece >= 0)
            {
                int c = eggNewPiece % EggPuzzle.Cols, r = eggNewPiece / EggPuzzle.Cols;
                float cw = 1f / EggPuzzle.Cols, ch = 1f / EggPuzzle.Rows;
                eggPieceImg.uvRect = new Rect(c * cw, 1f - (r + 1) * ch, cw, ch);
            }
            for (int i = 0; i < eggParts.Length; i++) { eggPartImgs[i].enabled = false; eggPartLife[i] = 0f; }

            // contenuto nell'area sicura di adesso, palco scalato per starci tutto
            eggContent.anchorMin = safeRoot.anchorMin;
            eggContent.anchorMax = safeRoot.anchorMax;
            eggContent.offsetMin = eggContent.offsetMax = Vector2.zero;

            eggFxGroup.alpha = 0f;
            eggFxGroup.blocksRaycasts = true;
            eggFx.transform.SetAsLastSibling();   // sopra a tutto, anche al pannello dell'obiettivo giornaliero
            eggFx.SetActive(true);

            // fanfara a volume pieno degli effetti; la musica scende a zero per tutta la sua durata
            if (eggFanfare != null && eggAudio != null)
            {
                if (eggFanfare.loadState == AudioDataLoadState.Unloaded) eggFanfare.LoadAudioData();
                eggAudio.Stop();
                eggAudio.clip = eggFanfare;
                eggAudio.volume = 1f;
                eggAudio.Play();
            }
            if (MusicPlayer.Instance != null) MusicPlayer.Instance.Duck(eggFx, 0f);
            eggMusicDucked = true;
            eggMusicReturn = -1f;

            PoseEggCelebration(0f, 0f);
        }

        /// <summary>Tocco sulla scena o tasto Indietro: dopo SkipAfter la scena sfuma in fretta (fanfara compresa).</summary>
        void SkipEggCelebration()
        {
            if (!EggCelebrating || eggClock == null) return;
            if (eggClock.TrySkip()) Haptics.Tap();
        }

        /// <summary>
        /// Chiude la scena. showResult = false (nuovo livello o titolo nel frattempo): l'esito non si mostra più.
        /// </summary>
        void FinishEggCelebration(bool showResult)
        {
            if (eggFx == null) return;
            Action then = eggThen;
            eggThen = null;
            bool pending = !eggResultShown;
            eggResultShown = true;
            if (eggAudio != null) eggAudio.Stop();
            eggFx.SetActive(false);
            eggStage.anchoredPosition = Vector2.zero;
            if (eggMusicDucked && eggMusicReturn < 0f) eggMusicReturn = 0f;   // la musica risale piano
            if (showResult && pending) then?.Invoke();
        }

        void AbortEggCelebration()
        {
            if (EggCelebrating) FinishEggCelebration(false);
        }

        /// <summary>Da Update: avanza la scena e, finita, riporta su la musica.</summary>
        void UpdateEggCelebration()
        {
            float dt = Time.unscaledDeltaTime;
            if (EggCelebrating && eggClock != null)
            {
                float before = eggClock.T;
                float t = eggClock.Advance(dt);
                if (eggAudio != null && eggAudio.clip != null) eggAudio.volume = eggClock.FanfareVolume;
                if (eggClock.ResultDue && !eggResultShown)
                {
                    // l'esito compare sotto la dissolvenza; da qui i tocchi arrivano a lui
                    eggResultShown = true;
                    eggFxGroup.blocksRaycasts = false;
                    Action then = eggThen;
                    eggThen = null;
                    then?.Invoke();
                    eggFx.transform.SetAsLastSibling();
                }
                if (eggClock.Done) FinishEggCelebration(true);
                else PoseEggCelebration(t, t - before);
            }
            if (eggMusicDucked && eggMusicReturn >= 0f)
            {
                eggMusicReturn += Mathf.Min(dt, EggCelebrationTimeline.MaxStep);
                float k = EggCelebrationTimeline.MusicReturn(eggMusicReturn);
                if (MusicPlayer.Instance != null)
                {
                    if (k >= 1f) MusicPlayer.Instance.Unduck(eggFx);
                    else MusicPlayer.Instance.Duck(eggFx, k);
                }
                if (k >= 1f) { eggMusicDucked = false; eggMusicReturn = -1f; }
            }
        }

        // ---------- animazione ----------

        void PoseEggCelebration(float t, float dt)
        {
            EggCelebrationPlan p = eggClock.Plan;
            eggFxGroup.alpha = eggClock.SceneAlpha;

            // il palco si adatta all'area sicura (anche se cambia: rotazione, notch)
            Rect area = eggContent.rect;
            float fit = Mathf.Min(area.width / StageW, area.height / StageH);
            fit = Mathf.Clamp(fit > 0f ? fit : 1f, 0.3f, 1.25f);
            eggStage.localScale = new Vector3(fit, fit, 1f);
            EggCelebrationTimeline.Shake(t, p, 34f, out float sx, out float sy);
            eggStage.anchoredPosition = new Vector2(sx, sy);

            // raggiera
            float rays = EggCelebrationTimeline.SmoothStep(EggCelebrationTimeline.Seg(t, p.RaysStart, p.RaysEnd));
            eggRays.localScale = Vector3.one * (0.35f + 0.65f * rays);
            eggRays.localRotation = Quaternion.Euler(0f, 0f, -t * 20f);
            eggRays2.localScale = Vector3.one * (0.5f + 0.5f * rays);
            eggRays2.localRotation = Quaternion.Euler(0f, 0f, t * 12f + 9f);
            SetAlpha(eggRaysImg, 0.75f * rays);
            SetAlpha(eggRays2Img, 0.45f * rays);

            // uovo: cade, rimbalza, si schiaccia all'atterraggio e quando "sputa" il pezzo
            float drop = EggCelebrationTimeline.DropOffset(t, p);
            eggIcon.anchoredPosition = new Vector2(0f, EggBaseY + drop * 1100f);
            float squash = EggCelebrationTimeline.Spring(t, p.DropEnd - 0.12f, 0.16f, 20f, 7f);
            if (p.PieceFlight) squash += EggCelebrationTimeline.Spring(t, p.FlightStart, 0.14f, 24f, 9f);
            float bob = t > p.DropEnd ? Mathf.Sin((t - p.DropEnd) * 3.2f) * 4f : 0f;
            eggIcon.localScale = new Vector3(1f + squash, 1f - squash, 1f);
            eggIcon.localRotation = Quaternion.Euler(0f, 0f, bob);

            // alone: arriva con l'uovo, pulsa, e scoppia all'impatto del titolo
            float glowIn = EggCelebrationTimeline.Seg(t, p.DropEnd - 0.25f, p.DropEnd + 0.15f);
            float glowKick = t >= p.SlamEnd ? Mathf.Exp(-(t - p.SlamEnd) * 5f) * 0.45f : 0f;
            eggGlow.localScale = Vector3.one * ((0.6f + 0.4f * glowIn) * (1f + 0.07f * Mathf.Sin(t * 4.2f) + glowKick));
            SetAlpha(eggGlowImg, 0.9f * glowIn);
            for (int i = 0; i < eggTwinkles.Length; i++)
            {
                float ph = t * 3.1f + i * 1.7f;
                float tw = Mathf.Max(0f, Mathf.Sin(ph));
                eggTwinkles[i].localScale = Vector3.one * (0.3f + 0.9f * tw * tw);
                eggTwinkles[i].localRotation = Quaternion.Euler(0f, 0f, t * 40f + i * 20f);
                SetAlpha(eggTwinkleImgs[i], glowIn * tw);
            }

            // titolo che sbatte
            float slam = EggCelebrationTimeline.SlamScale(t, p);
            eggTitleRt.localScale = new Vector3(slam, slam, 1f);
            eggTitleRt.localRotation = Quaternion.Euler(0f, 0f, -3f + EggCelebrationTimeline.Spring(t, p.SlamEnd, 6f, 15f, 6f));
            SetAlpha(eggTitle, EggCelebrationTimeline.SlamAlpha(t, p));
            if (!eggSlammed && t >= p.SlamEnd) { eggSlammed = true; OnEggSlam(); }
            float flash = t >= p.SlamEnd ? Mathf.Clamp01(1f - (t - p.SlamEnd) / 0.28f) : 0f;
            eggFlash.color = new Color(1f, 0.97f, 0.85f, 0.55f * flash * flash);

            // didascalia e nome che salgono
            float nameIn = EggCelebrationTimeline.Seg(t, p.NameStart, p.NameEnd);
            float nameBack = EggCelebrationTimeline.EaseOutBack(nameIn, 2f);
            eggCaptionRt.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, nameBack);
            SetAlpha(eggCaption, nameIn);
            eggNameRt.anchoredPosition = new Vector2(0f, -40f - 60f * (1f - nameBack));
            eggNameRt.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, nameBack);
            SetAlpha(eggName, nameIn);

            // anteprima del puzzle e contatore
            float prev = EggCelebrationTimeline.Seg(t, p.PreviewStart, p.PreviewEnd);
            eggPreview.localScale = Vector3.one * Mathf.Max(0.01f, EggCelebrationTimeline.EaseOutBack(prev, 2.2f));
            eggPreviewGroup.alpha = prev;
            int count = EggCelebrationTimeline.CounterValue(t, p, eggTotal);
            if (count != eggShownCount)
            {
                eggShownCount = count;
                eggCounter.text = "PUZZLE " + count + " / " + EggPuzzle.Pieces;   // solo quando cambia
            }
            float countPop = p.PieceFlight ? EggCelebrationTimeline.Spring(t, p.FlightEnd, 0.35f, 16f, 6f) : 0f;
            eggCounterRt.localScale = Vector3.one * (1f + Mathf.Max(0f, countPop));
            eggCounter.color = new Color(Mathf.Lerp(1f, Gold.r, Mathf.Clamp01(countPop * 4f)), Mathf.Lerp(1f, Gold.g, Mathf.Clamp01(countPop * 4f)), Mathf.Lerp(1f, Gold.b, Mathf.Clamp01(countPop * 4f)), prev);

            // volo del pezzo: esce dall'uovo, si ingrandisce, vola ad arco nella sua casella
            if (p.PieceFlight && eggNewPiece >= 0)
            {
                bool flying = t >= p.FlightStart && t < p.FlightEnd;
                if (eggPiece.gameObject.activeSelf != flying) eggPiece.gameObject.SetActive(flying);
                if (flying)
                {
                    if (!eggSpat) { eggSpat = true; Sfx.Play(this, "whoosh", 0.35f); }
                    float x = EggCelebrationTimeline.Flight(t, p, out float arc);
                    Vector2 from = new Vector2(0f, EggCenterY), to = CellPos(eggNewPiece);
                    Vector2 at = Vector2.Lerp(from, to, x) + new Vector2(260f * arc, 160f * arc);
                    eggPiece.anchoredPosition = at;
                    float grow = Mathf.Sin(Mathf.Clamp01(x * 1.15f) * Mathf.PI);   // grande a metà volo, giusto alla fine
                    eggPiece.localScale = Vector3.one * (1f + 2.6f * grow);
                    eggPiece.localRotation = Quaternion.Euler(0f, 0f, (1f - x) * 540f);
                }
                if (!eggLanded && t >= p.FlightEnd) { eggLanded = true; OnEggPieceLanded(); }
                if (eggLanded)
                {
                    float pop = EggCelebrationTimeline.Spring(t, p.FlightEnd, 0.6f, 18f, 7f);
                    eggCellPic[eggNewPiece].transform.localScale = Vector3.one * (1f + Mathf.Max(0f, pop));
                }
            }

            // invito a toccare (solo quando il tocco funziona)
            float hint = eggClock.CanSkip ? EggCelebrationTimeline.Seg(t, p.HintAt, p.HintAt + 0.3f) * (0.55f + 0.45f * Mathf.Sin(t * 5f)) : 0f;
            SetAlpha(eggHint, hint);

            UpdateEggParticles(dt);
        }

        /// <summary>Impatto del titolo: vibrazione forte, coriandoli dall'alto, stelle dall'uovo.</summary>
        void OnEggSlam()
        {
            Haptics.Pattern(SlamTimings, SlamAmplitudes);
            for (int i = 0; i < 40; i++)
            {
                float a = UnityEngine.Random.Range(15f, 165f) * Mathf.Deg2Rad;
                float v = UnityEngine.Random.Range(700f, 1500f);
                SpawnEggParticle(false, new Vector2(UnityEngine.Random.Range(-380f, 380f), TitleY - 20f), new Vector2(Mathf.Cos(a) * v * 0.8f, Mathf.Sin(a) * v));
            }
            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI * 2f / 12f + UnityEngine.Random.Range(-0.2f, 0.2f);
                float v = UnityEngine.Random.Range(600f, 950f);
                SpawnEggParticle(true, new Vector2(0f, EggCenterY), new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * v);
            }
        }

        /// <summary>Il pezzo è nella sua casella: scintille, "pop" e una vibrazione media.</summary>
        void OnEggPieceLanded()
        {
            if (eggNewPiece < 0) return;
            eggCellPic[eggNewPiece].enabled = eggCellPic[eggNewPiece].texture != null;
            Vector2 at = CellPos(eggNewPiece);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 0.25f;
                SpawnEggParticle(true, at, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 420f);
            }
            for (int i = 0; i < 10; i++)
            {
                float a = UnityEngine.Random.Range(40f, 140f) * Mathf.Deg2Rad;
                SpawnEggParticle(false, at, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * UnityEngine.Random.Range(400f, 800f));
            }
            Haptics.Medium();
            Sfx.Play(this, "pop", 0.6f);
        }

        void SpawnEggParticle(bool star, Vector2 pos, Vector2 vel)
        {
            int i;
            if (star) { i = ConfettiCount + eggNextStar; eggNextStar = (eggNextStar + 1) % StarCount; }
            else { i = eggNextConfetti; eggNextConfetti = (eggNextConfetti + 1) % ConfettiCount; }
            eggPartPos[i] = pos;
            eggPartVel[i] = vel;
            eggPartAge[i] = 0f;
            eggPartLife[i] = star ? UnityEngine.Random.Range(0.7f, 1.1f) : UnityEngine.Random.Range(1.6f, 2.6f);
            eggPartRot[i] = UnityEngine.Random.Range(0f, 360f);
            eggPartSpin[i] = UnityEngine.Random.Range(-540f, 540f);
            eggPartFlip[i] = UnityEngine.Random.Range(6f, 16f);
            Image img = eggPartImgs[i];
            img.color = star ? new Color(1f, 0.93f, 0.55f, 1f) : ConfettiColors[UnityEngine.Random.Range(0, ConfettiColors.Length)];
            img.enabled = true;
            eggParts[i].anchoredPosition = pos;
        }

        void UpdateEggParticles(float dt)
        {
            if (dt <= 0f) return;
            for (int i = 0; i < eggParts.Length; i++)
            {
                if (eggPartLife[i] <= 0f) continue;
                bool star = i >= ConfettiCount;
                eggPartAge[i] += dt;
                float age = eggPartAge[i], life = eggPartLife[i];
                if (age >= life) { eggPartLife[i] = 0f; eggPartImgs[i].enabled = false; continue; }
                Vector2 v = eggPartVel[i];
                v *= Mathf.Max(0f, 1f - (star ? 3.2f : 1.6f) * dt);
                if (!star) v.y -= 1500f * dt;
                eggPartVel[i] = v;
                eggPartPos[i] += v * dt;
                eggPartRot[i] += eggPartSpin[i] * dt;
                RectTransform rt = eggParts[i];
                rt.anchoredPosition = eggPartPos[i];
                rt.localRotation = Quaternion.Euler(0f, 0f, eggPartRot[i]);
                float fade = Mathf.Clamp01((life - age) / 0.4f);
                if (star)
                {
                    float s = Mathf.Sin(Mathf.Clamp01(age / life) * Mathf.PI) * 1.2f + 0.2f;
                    rt.localScale = new Vector3(s, s, 1f);
                }
                else rt.localScale = new Vector3(Mathf.Cos(age * eggPartFlip[i]), 1f, 1f);   // il coriandolo "gira" in aria
                Color c = eggPartImgs[i].color;
                c.a = fade;
                eggPartImgs[i].color = c;
            }
        }

        static void SetAlpha(Graphic g, float a)
        {
            Color c = g.color;
            if (Mathf.Abs(c.a - a) < 0.002f) return;
            c.a = a;
            g.color = c;
        }

        // ---------- disegni procedurali (una volta sola, poi in cache) ----------

        static Sprite goldenEgg, raysSprite, glowFallback;

        /// <summary>Uovo di Pasqua d'oro: sfumatura metallica, fascia a zig-zag, puntini, riflesso e contorno a inchiostro.</summary>
        static Sprite GoldenEggSprite()
        {
            if (goldenEgg != null) return goldenEgg;
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "golden_egg" };
            var px = new Color32[size * size];
            Color ink = new Color(0.17f, 0.13f, 0.2f);
            Color dark = new Color(0.72f, 0.42f, 0.05f), mid = new Color(0.98f, 0.74f, 0.16f), light = new Color(1f, 0.93f, 0.55f);
            Color band = new Color(1f, 0.97f, 0.8f), dot = new Color(1f, 0.42f, 0.45f);
            for (int j = 0; j < size; j++)
                for (int i = 0; i < size; i++)
                {
                    // coordinate -1..1, y verso l'alto; l'uovo è più stretto in alto
                    float x = (i + 0.5f) / size * 2.1f - 1.05f, y = (j + 0.5f) / size * 2.1f - 1.05f;
                    float w = 0.74f * (1f - 0.2f * y);
                    float nx = x / w;
                    float f = Mathf.Sqrt(nx * nx + y * y);   // 1 = bordo
                    float aa = 0.012f;
                    if (f > 1f + aa) { px[j * size + i] = new Color32(0, 0, 0, 0); continue; }
                    float edge = Mathf.Clamp01((1f + aa - f) / (2f * aa));
                    Color col;
                    if (f > 0.93f) col = ink;   // contorno
                    else
                    {
                        // luce da in alto a sinistra
                        float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - f * f));
                        float lit = Mathf.Clamp01(-0.45f * nx + 0.55f * y + 0.7f * nz);
                        col = lit < 0.5f ? Color.Lerp(dark, mid, lit * 2f) : Color.Lerp(mid, light, (lit - 0.5f) * 2f);
                        // fascia a zig-zag in mezzo
                        float zig = Mathf.Abs(Mathf.Repeat(x * 5f, 2f) - 1f) * 0.16f - 0.08f;
                        float d = Mathf.Abs(y - 0.02f - zig);
                        if (d < 0.075f) col = Color.Lerp(band, col, d < 0.055f ? 0f : 0.6f) * (0.82f + 0.18f * lit);
                        else if (d < 0.095f) col = Color.Lerp(col, ink, 0.55f);
                        // due file di puntini
                        for (int row = 0; row < 2; row++)
                        {
                            float cy = row == 0 ? 0.44f : -0.42f;
                            float off = row == 0 ? 0f : 0.14f;   // file sfalsate
                            float cx = Mathf.Round((x - off) / 0.28f) * 0.28f + off;
                            float dd = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                            if (dd < 0.0042f) col = Color.Lerp(dot, Color.white, Mathf.Clamp01(lit - 0.4f));
                            else if (dd < 0.006f) col = Color.Lerp(col, ink, 0.5f);
                        }
                        // riflesso
                        float hx = (x + 0.26f) / 0.11f, hy = (y - 0.5f) / 0.2f;
                        float h = 1f - (hx * hx + hy * hy);
                        if (h > 0f) col = Color.Lerp(col, Color.white, Mathf.Clamp01(h * 2.5f) * 0.85f);
                    }
                    col.a = edge;
                    px[j * size + i] = col;
                }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            goldenEgg = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            goldenEgg.name = "golden_egg";
            return goldenEgg;
        }

        /// <summary>
        /// Raggiera dorata: 16 raggi dai bordi morbidi che sfumano verso l'esterno (gli angoli dello schermo restano
        /// scuri, effetto "riflettore").
        /// </summary>
        static Sprite RaysSprite()
        {
            if (raysSprite != null) return raysSprite;
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "egg_rays" };
            var px = new Color32[size * size];
            Color on = new Color(1f, 0.96f, 0.76f), off = new Color(1f, 0.79f, 0.24f);
            for (int j = 0; j < size; j++)
                for (int i = 0; i < size; i++)
                {
                    float x = (i + 0.5f) / size * 2f - 1f, y = (j + 0.5f) / size * 2f - 1f;
                    float r = Mathf.Sqrt(x * x + y * y);
                    float a = Mathf.Atan2(y, x);
                    float fade = Mathf.Pow(Mathf.Clamp01(1f - r), 1.3f) * Mathf.Clamp01(r / 0.1f);
                    // bordo del raggio sfumato su ~0.1 rad
                    float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((Mathf.Sin(a * 16f) - 0.05f) / 0.35f + 0.5f));
                    Color c = Color.Lerp(off, on, k);
                    c.a = Mathf.Lerp(0.22f, 0.9f, k) * fade;
                    px[j * size + i] = c;
                }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            raysSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            raysSprite.name = "egg_rays";
            return raysSprite;
        }

        /// <summary>Alone di riserva (se lo sprite glow non è stato assegnato dal builder).</summary>
        static Sprite GlowFallback()
        {
            if (glowFallback != null) return glowFallback;
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "egg_glow" };
            var px = new Color32[size * size];
            for (int j = 0; j < size; j++)
                for (int i = 0; i < size; i++)
                {
                    float x = (i + 0.5f) / size * 2f - 1f, y = (j + 0.5f) / size * 2f - 1f;
                    float r = Mathf.Sqrt(x * x + y * y);
                    px[j * size + i] = new Color(1f, 1f, 1f, Mathf.Clamp01(Mathf.Exp(-r * 3.6f) * (1f - r)));
                }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            glowFallback = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return glowFallback;
        }
    }
}
