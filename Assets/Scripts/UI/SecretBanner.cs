using UnityEngine;
using UnityEngine.UI;
using Gotcha.Core;

namespace Gotcha.UI
{
    /// <summary>
    /// (0.83) Due tocchi di vita alla barra in alto:
    ///  - il nastro dorato "SECRET EGG HIDDEN IN THIS LEVEL!" che, all'inizio di un livello con un Easter egg, scende
    ///    da sotto i cuori, resta ~2,5 s e risale (un tocco lo fa risalire subito). Segreto già trovato: nastro più
    ///    tenue, "SECRET EGG ALREADY FOUND". Sta in alto nell'area sicura, la scheda dei consigli (TutorialTips) sta
    ///    in basso: non si sovrappongono. Riceve i tocchi solo su se stesso (il livello sotto resta giocabile);
    ///  - i cuori delle vite che, ognuno per conto suo, fanno ogni tanto un saltello, un battito doppio, una giravolta
    ///    o un dondolio scelti a caso (HeartIdle, logica pura con i test).
    /// I tempi e le curve stanno in SecretBannerLogic e HeartIdle; qui solo gli oggetti, costruiti una volta.
    /// </summary>
    public partial class GameUI
    {
        const float BannerHeight = 100f;
        const float BannerTop = 282f;   // dal bordo alto dell'area sicura: subito sotto la riga dei cuori (che finisce a 265)
        static readonly Color BannerGold = new Color(1f, 0.79f, 0.18f);
        static readonly Color BannerGoldDeep = new Color(0.78f, 0.5f, 0.05f);
        static readonly Color BannerSoft = new Color(1f, 0.93f, 0.7f);

        GameObject bannerGo;
        RectTransform bannerRect, bannerShine, bannerStarL, bannerStarR;
        CanvasGroup bannerGroup;
        Image bannerBg;
        Text bannerText;
        bool bannerActive;
        float bannerStart, bannerOutStart;

        // cuori
        HeartIdle[] heartIdle;
        Vector2[] heartHome;
        bool[] heartWasFull;

        void BuildSecretBanner()
        {
            bannerRect = CreateRect("SecretBanner", safeRoot, new Vector2(0f, 1f), new Vector2(1f, 1f));
            bannerRect.pivot = new Vector2(0.5f, 1f);
            bannerRect.offsetMin = new Vector2(80f, -BannerTop - BannerHeight);
            bannerRect.offsetMax = new Vector2(-80f, -BannerTop);
            bannerGo = bannerRect.gameObject;
            bannerBg = bannerGo.AddComponent<Image>();
            SetSprite(bannerBg);
            bannerBg.color = BannerGold;
            bannerBg.raycastTarget = true;   // l'unica cosa che riceve tocchi: il nastro stesso
            Shadow shadow = bannerGo.AddComponent<Shadow>();
            shadow.effectColor = BannerGoldDeep;
            shadow.effectDistance = new Vector2(0f, -8f);
            Outline outline = bannerGo.AddComponent<Outline>();
            outline.effectColor = new Color(Ink.r, Ink.g, Ink.b, 0.85f);
            outline.effectDistance = new Vector2(3f, -3f);
            bannerGo.AddComponent<RectMask2D>();   // il luccichio resta dentro il nastro
            Button tap = bannerGo.AddComponent<Button>();
            tap.transition = Selectable.Transition.None;
            tap.onClick.AddListener(DismissSecretBanner);
            bannerGroup = bannerGo.AddComponent<CanvasGroup>();

            // luccichio: una striscia chiara inclinata che attraversa il nastro
            bannerShine = CreateRect("Shine", bannerRect, new Vector2(0f, 0f), new Vector2(0f, 1f));
            bannerShine.sizeDelta = new Vector2(70f, 40f);
            bannerShine.localRotation = Quaternion.Euler(0f, 0f, -18f);
            Image shine = bannerShine.gameObject.AddComponent<Image>();
            shine.color = new Color(1f, 1f, 1f, 0.45f);
            shine.raycastTarget = false;

            bannerStarL = BannerStar("StarL", 46f);
            bannerStarR = BannerStar("StarR", -46f);

            bannerText = CreateText(bannerRect, "", 44, Ink, FontStyle.Bold);
            bannerText.resizeTextMinSize = 20;
            RectTransform tr = (RectTransform)bannerText.transform;
            float pad = eggStarSprite != null ? 92f : 24f;
            tr.offsetMin = new Vector2(pad, 8f);
            tr.offsetMax = new Vector2(-pad, -6f);
            bannerGo.SetActive(false);
        }

        RectTransform BannerStar(string name, float x)
        {
            RectTransform rt = CreateRect(name, bannerRect, new Vector2(x > 0f ? 0f : 1f, 0.5f), new Vector2(x > 0f ? 0f : 1f, 0.5f));
            rt.sizeDelta = new Vector2(64f, 64f);
            rt.anchoredPosition = new Vector2(x, 0f);
            Image img = rt.gameObject.AddComponent<Image>();
            img.sprite = eggStarSprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.color = Color.white;
            img.enabled = eggStarSprite != null;   // senza sprite: asterischi nella frase (SecretBannerLogic.Caption)
            return rt;
        }

        /// <summary>All'inizio di un livello con un Easter egg.</summary>
        void ShowSecretBanner(bool alreadyFound)
        {
            if (bannerGo == null) return;
            bannerText.text = SecretBannerLogic.Caption(alreadyFound, eggStarSprite != null);
            bannerBg.color = alreadyFound ? BannerSoft : BannerGold;
            bannerText.color = alreadyFound ? new Color(Ink.r, Ink.g, Ink.b, 0.75f) : Ink;
            bannerShine.gameObject.SetActive(!alreadyFound);
            bannerStart = Time.unscaledTime;
            bannerOutStart = SecretBannerLogic.DefaultOutStart;
            bannerActive = true;
            bannerRect.SetAsLastSibling();
            bannerGo.SetActive(true);
            UpdateSecretBanner();   // posa iniziale subito (nascosto: c'è la breve attesa)
        }

        void HideSecretBanner()
        {
            bannerActive = false;
            if (bannerGo != null && bannerGo.activeSelf) bannerGo.SetActive(false);
        }

        void DismissSecretBanner()
        {
            if (!bannerActive) return;
            bannerOutStart = SecretBannerLogic.OutStartAfterTap(Time.unscaledTime - bannerStart, bannerOutStart);
        }

        void UpdateSecretBanner()
        {
            if (!bannerActive) return;
            bool gone = flow.Paused || titlePanel.activeSelf || resultPanel.activeSelf || completedPanel.activeSelf ||
                        flow.CurrentDefinition == null || (flow.Current != null && flow.Current.IsFinished);
            float age = Time.unscaledTime - bannerStart;
            if (gone || SecretBannerLogic.Finished(age, bannerOutStart)) { HideSecretBanner(); return; }
            float v = SecretBannerLogic.Visible(age, bannerOutStart);
            bannerGroup.alpha = Mathf.Clamp01(v * 1.6f);
            bannerGroup.blocksRaycasts = SecretBannerLogic.Tappable(age, bannerOutStart);
            // scende da sotto i cuori (parte 70 px più su) e si "srotola" un poco in larghezza
            bannerRect.anchoredPosition = new Vector2(0f, (1f - v) * 70f);
            bannerRect.localScale = new Vector3(0.85f + 0.15f * v, 1f, 1f);
            float spin = age * 90f;
            bannerStarL.localRotation = Quaternion.Euler(0f, 0f, spin);
            bannerStarR.localRotation = Quaternion.Euler(0f, 0f, -spin);
            float twinkle = 1f + Mathf.Sin(age * 7f) * 0.1f;
            bannerStarL.localScale = bannerStarR.localScale = new Vector3(twinkle, twinkle, 1f);
            if (bannerShine.gameObject.activeSelf)
            {
                float k = SecretBannerLogic.Shine(age);
                float w = bannerRect.rect.width;
                bannerShine.anchoredPosition = new Vector2(k < 0f ? -200f : -80f + k * (w + 160f), 0f);
            }
        }

        // ---------- cuori "vivi" ----------

        void InitHeartIdle()
        {
            heartIdle = new HeartIdle[hearts.Length];
            heartHome = new Vector2[hearts.Length];
            heartWasFull = new bool[hearts.Length];
            int seed = System.Environment.TickCount;
            for (int i = 0; i < hearts.Length; i++)
            {
                heartIdle[i] = new HeartIdle(unchecked(seed + i * 7919));
                heartHome[i] = ((RectTransform)hearts[i].transform).anchoredPosition;
            }
        }

        /// <summary>Posa di un cuore pieno (o dorato con le vite infinite): la sua mossa del momento.</summary>
        void PoseHeart(int i, RectTransform rt, bool full, float t)
        {
            if (heartIdle == null) InitHeartIdle();
            if (!full)
            {
                if (heartWasFull[i]) heartIdle[i].Rest(t);
                heartWasFull[i] = false;
                rt.anchoredPosition = heartHome[i];
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;
                return;
            }
            if (!heartWasFull[i]) { heartIdle[i].Rest(t); heartWasFull[i] = true; }
            HeartPose p = heartIdle[i].Sample(t);
            rt.anchoredPosition = heartHome[i] + new Vector2(0f, p.OffsetY);
            rt.localScale = new Vector3(p.ScaleX, p.ScaleY, 1f);
            rt.localRotation = Quaternion.Euler(0f, 0f, p.RotationZ);
        }

        /// <summary>Il cuore appena perso non è in una mossa: torna a casa (la sua animazione di rottura lo scala e ruota).</summary>
        void HomeHeart(int i, RectTransform rt)
        {
            if (heartIdle == null) InitHeartIdle();
            rt.anchoredPosition = heartHome[i];
            heartWasFull[i] = false;
        }
    }
}
