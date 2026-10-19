#if GOTCHA_ADMOB
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;

namespace Gotcha.Ads
{
    /// <summary>
    /// Pubblicità vere con AdMob (plugin "Google Mobile Ads for Unity" v9 o successivo). Si compila solo con il
    /// define GOTCHA_ADMOB (Player Settings > Scripting Define Symbols), da aggiungere dopo aver importato il plugin.
    /// Prima chiede il consenso GDPR con UMP (obbligatorio in UE), poi avvia AdMob e tiene sempre caricati
    /// un interstitial e un rewarded: dopo ogni visione ne carica subito un altro.
    /// </summary>
    public class AdMobProvider : IAdProvider
    {
        // TODO prima della pubblicazione: sostituire con gli ID veri (AdMob > App > Unità pubblicitarie).
        // Questi sono gli ID di PROVA ufficiali di Google: mostrano annunci finti e sono sicuri da usare nei test.
        // Anche l'App ID va messo in Assets > Google Mobile Ads > Settings (non qui).
        const string InterstitialId = "ca-app-pub-3940256099942544/1033173712";
        const string RewardedId = "ca-app-pub-3940256099942544/5224354917";

        static AdMobProvider instance;

        InterstitialAd interstitial;
        RewardedAd rewarded;
        bool started, loadingInterstitial, loadingRewarded;
        float interstitialRetry = 5f, rewardedRetry = 5f;
        float savedTimeScale = 1f;

        public bool IsReady => interstitial != null && interstitial.CanShowAd();
        public bool RewardedReady => rewarded != null && rewarded.CanShowAd();

        public AdMobProvider()
        {
            instance = this;
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            AdMobDispatcher.Later(0f, RequestConsent);   // al primo frame: l'Activity è pronta per il modulo
        }

        // ---------- Consenso (UMP) ----------

        void RequestConsent()
        {
            // se il consenso è già stato dato in una sessione precedente si parte subito, senza aspettare
            if (ConsentInformation.CanRequestAds()) StartAds();
            ConsentInformation.Update(new ConsentRequestParameters(), updateError => AdMobDispatcher.Post(() =>
            {
                if (updateError != null)
                {
                    Debug.LogWarning("[Ads] Consenso non aggiornato: " + updateError.Message);
                    if (ConsentInformation.CanRequestAds()) StartAds();
                    return;
                }
                // in UE il modulo compare la prima volta (e quando scade); altrove di solito non serve
                ConsentForm.LoadAndShowConsentFormIfRequired(formError => AdMobDispatcher.Post(() =>
                {
                    if (formError != null) Debug.LogWarning("[Ads] Modulo consenso: " + formError.Message);
                    if (ConsentInformation.CanRequestAds()) StartAds();
                }));
            }));
        }

        /// <summary>Il giocatore deve poter rivedere le scelte sulla privacy (pulsante nelle Impostazioni).</summary>
        public static bool PrivacyOptionsRequired =>
            ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;

        public static void ShowPrivacyOptions()
        {
            ConsentForm.ShowPrivacyOptionsForm(formError => AdMobDispatcher.Post(() =>
            {
                if (formError != null) Debug.LogWarning("[Ads] Opzioni privacy: " + formError.Message);
                if (instance != null && ConsentInformation.CanRequestAds()) instance.StartAds();
            }));
        }

        void StartAds()
        {
            if (started) return;
            started = true;
            MobileAds.Initialize(status => AdMobDispatcher.Post(() =>
            {
                LoadInterstitial();
                LoadRewarded();
            }));
        }

        // ---------- Caricamento ----------

        void LoadInterstitial()
        {
            if (loadingInterstitial || interstitial != null) return;
            loadingInterstitial = true;
            InterstitialAd.Load(InterstitialId, new AdRequest(), (ad, error) => AdMobDispatcher.Post(() =>
            {
                loadingInterstitial = false;
                if (error != null || ad == null)
                {
                    // nuovo tentativo con attesa crescente (5 s, 10 s... fino a 2 minuti)
                    Debug.LogWarning("[Ads] Interstitial non caricato: " + (error != null ? error.GetMessage() : "null"));
                    AdMobDispatcher.Later(interstitialRetry, LoadInterstitial);
                    interstitialRetry = Mathf.Min(interstitialRetry * 2f, 120f);
                    return;
                }
                interstitialRetry = 5f;
                interstitial = ad;
            }));
        }

        void LoadRewarded()
        {
            if (loadingRewarded || rewarded != null) return;
            loadingRewarded = true;
            RewardedAd.Load(RewardedId, new AdRequest(), (ad, error) => AdMobDispatcher.Post(() =>
            {
                loadingRewarded = false;
                if (error != null || ad == null)
                {
                    Debug.LogWarning("[Ads] Rewarded non caricato: " + (error != null ? error.GetMessage() : "null"));
                    AdMobDispatcher.Later(rewardedRetry, LoadRewarded);
                    rewardedRetry = Mathf.Min(rewardedRetry * 2f, 120f);
                    return;
                }
                rewardedRetry = 5f;
                rewarded = ad;
            }));
        }

        // ---------- Visione ----------

        public void Show(Action onClosed)
        {
            InterstitialAd ad = interstitial;
            if (ad == null || !ad.CanShowAd()) { onClosed?.Invoke(); LoadInterstitial(); return; }
            interstitial = null;
            bool done = false;
            Action finish = () =>
            {
                if (done) return;
                done = true;
                ad.Destroy();
                Resume();
                onClosed?.Invoke();
                LoadInterstitial();
            };
            ad.OnAdFullScreenContentClosed += () => AdMobDispatcher.Post(finish);
            ad.OnAdFullScreenContentFailed += error => AdMobDispatcher.Post(finish);
            Pause();
            ad.Show();
        }

        public void ShowRewarded(Action<bool> onDone)
        {
            RewardedAd ad = rewarded;
            if (ad == null || !ad.CanShowAd()) { onDone?.Invoke(false); LoadRewarded(); return; }
            rewarded = null;
            bool earned = false, done = false;
            Action finish = () =>
            {
                if (done) return;
                done = true;
                ad.Destroy();
                Resume();
                onDone?.Invoke(earned);
                LoadRewarded();
            };
            // il premio di solito arriva prima della chiusura; un attimo di margine per i telefoni che lo mandano dopo
            ad.OnAdFullScreenContentClosed += () => AdMobDispatcher.Post(() => AdMobDispatcher.Later(0.3f, finish));
            ad.OnAdFullScreenContentFailed += error => AdMobDispatcher.Post(finish);
            Pause();
            ad.Show(reward => AdMobDispatcher.Post(() => earned = true));
        }

        // mentre lo spot è aperto il gioco è fermo e muto (come nella pubblicità simulata)
        void Pause()
        {
            savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }

        void Resume()
        {
            Time.timeScale = savedTimeScale;
            AudioListener.pause = false;
        }
    }

    /// <summary>Riporta le callback di AdMob/UMP sul thread di Unity e gestisce i tentativi ritardati.</summary>
    public class AdMobDispatcher : MonoBehaviour
    {
        static AdMobDispatcher instance;
        static readonly Queue<Action> queue = new Queue<Action>();

        static AdMobDispatcher Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("AdMobDispatcher");
                    DontDestroyOnLoad(go);
                    instance = go.AddComponent<AdMobDispatcher>();
                }
                return instance;
            }
        }

        /// <summary>Da qualsiasi thread: l'azione gira al prossimo Update.</summary>
        public static void Post(Action action)
        {
            lock (queue) queue.Enqueue(action);
        }

        /// <summary>Solo dal thread di Unity: l'azione gira dopo <paramref name="seconds"/> secondi reali.</summary>
        public static void Later(float seconds, Action action)
        {
            Instance.StartCoroutine(Instance.Delay(seconds, action));
        }

        IEnumerator Delay(float seconds, Action action)
        {
            if (seconds > 0f) yield return new WaitForSecondsRealtime(seconds);
            else yield return null;
            action();
        }

        void Update()
        {
            while (true)
            {
                Action a;
                lock (queue)
                {
                    if (queue.Count == 0) return;
                    a = queue.Dequeue();
                }
                try { a(); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }
    }
}
#endif
