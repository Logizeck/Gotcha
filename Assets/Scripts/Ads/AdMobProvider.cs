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
    /// define GOTCHA_ADMOB, che l'Editor aggiunge da solo quando trova il plugin (Assets/Editor/AdMobSetup).
    /// Prima chiede il consenso GDPR con UMP (obbligatorio in UE), poi avvia AdMob e tiene sempre caricati
    /// un interstitial e un rewarded: dopo ogni visione ne carica subito un altro.
    /// </summary>
    public class AdMobProvider : IAdProvider
    {
        // ID in Assets/Resources/AdMobIds.json (AdMobIds): vuoti o build di sviluppo = ID di prova di Google
        static string InterstitialId => AdMobIds.Interstitial;
        static string RewardedId => AdMobIds.Rewarded;

        static AdMobProvider instance;

        InterstitialAd interstitial;
        RewardedAd rewarded;
        RewardedInterstitialAd rewardedInterstitial;   // riserva per i pulsanti "WATCH AD" (dalla 0.76)
        bool started, loadingInterstitial, loadingRewarded, loadingRewardedInterstitial;
        float interstitialRetry = 5f, rewardedRetry = 5f, rewardedInterstitialRetry = 5f;
        float savedTimeScale = 1f;

        public bool IsReady => interstitial != null && interstitial.CanShowAd();
        public bool RewardedReady => (rewarded != null && rewarded.CanShowAd()) || RewardedInterstitialReady;
        bool RewardedInterstitialReady => rewardedInterstitial != null && rewardedInterstitial.CanShowAd();

        public AdMobProvider()
        {
            instance = this;
            ConsentGate.Pending = true;   // microfono e notifiche aspettano il modulo dei cookie (FirstLaunchPermissions)
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            // dopo un attimo dall'avvio: con la chiamata al primo frame su alcuni telefoni il modulo del consenso non compariva
            AdMobDispatcher.Later(1.5f, RequestConsent);
        }

        // ---------- Consenso (UMP) ----------

        int consentAttempts;
        bool consentAnswered;

        void RequestConsent()
        {
            consentAttempts++;
            consentAnswered = false;
            // se il consenso è già stato dato in una sessione precedente si parte subito, senza aspettare
            AdsDiagnostics.Last = "consent: checking (" + consentAttempts + ")" + (AdMobIds.UsingTestAds ? " TEST ads" : "");
            try
            {
                if (ConsentInformation.CanRequestAds()) StartAds();
                ConsentInformation.Update(new ConsentRequestParameters(), updateError =>
                {
                    // la risposta può arrivare da un altro thread: qui solo un segno, il resto sul thread di Unity
                    consentAnswered = true;
                    AdMobDispatcher.Post(() => OnConsentUpdated(updateError));
                });
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                AdsDiagnostics.Last = "consent exception: " + e.Message;
                ConsentGate.Pending = false;
            }
            // se Google non risponde entro 10 s si riprova (al massimo 3 tentativi in tutto)
            AdMobDispatcher.Later(10f, () =>
            {
                if (consentAnswered) return;
                if (consentAttempts >= 3) { ConsentGate.Pending = false; return; }
                RequestConsent();
            });
        }

        void OnConsentUpdated(FormError updateError)
        {
            if (updateError != null)
            {
                Debug.LogWarning("[Ads] Consenso non aggiornato: " + updateError.Message);
                AdsDiagnostics.Last = "consent error: " + updateError.Message;
                // con gli annunci di prova il consenso non serve: si parte lo stesso (per poter provare tutto)
                if (ConsentInformation.CanRequestAds() || AdMobIds.UsingTestAds) StartAds();
                ConsentGate.Pending = false;
                return;
            }
            AdsDiagnostics.Last = "consent status: " + ConsentInformation.ConsentStatus;
            ShowConsentFormIfRequired(true);
        }

        /// <summary>
        /// In UE il modulo compare al primo avvio (e quando il consenso scade); altrove di solito non serve. Se il modulo non
        /// riesce a comparire (Activity non pronta, rete lenta) si riprova una volta dopo qualche secondo.
        /// </summary>
        void ShowConsentFormIfRequired(bool retry)
        {
            ConsentForm.LoadAndShowConsentFormIfRequired(formError => AdMobDispatcher.Post(() =>
            {
                if (formError != null)
                {
                    Debug.LogWarning("[Ads] Modulo consenso: " + formError.Message);
                    AdsDiagnostics.Last = "consent form error: " + formError.Message;
                    if (retry && ConsentInformation.ConsentStatus == ConsentStatus.Required)
                    {
                        AdMobDispatcher.Later(4f, () => ShowConsentFormIfRequired(false));
                        return;
                    }
                }
                else AdsDiagnostics.Last = "consent status: " + ConsentInformation.ConsentStatus;
                ConsentGate.Pending = false;
                if (ConsentInformation.CanRequestAds() || AdMobIds.UsingTestAds) StartAds();
                else AdsDiagnostics.Last = "consent: ads not allowed yet (" + ConsentInformation.ConsentStatus + ")";
            }));
        }

        /// <summary>Il giocatore deve poter rivedere le scelte sulla privacy (pulsante nelle Impostazioni).</summary>
        public static bool PrivacyOptionsRequired =>
            ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;

        /// <summary>Esito dell'ultimo tocco su PRIVACY OPTIONS, da mostrare al giocatore (null = modulo aperto).</summary>
        public static System.Action<string> PrivacyFeedback;

        /// <summary>
        /// Pulsante PRIVACY OPTIONS (sempre visibile dalla 0.76). Se lo stato del consenso non è ancora noto lo si
        /// aggiorna prima; poi si apre il modulo delle opzioni privacy di Google. Se Google dice che qui non serve, si
        /// prova comunque il modulo del consenso; se non c'è nemmeno quello, il giocatore riceve un messaggio.
        /// </summary>
        public static void ShowPrivacyOptions()
        {
            AdsDiagnostics.Last = "privacy options: opening";
            try
            {
                ConsentInformation.Update(new ConsentRequestParameters(), updateError => AdMobDispatcher.Post(() =>
                {
                    if (updateError != null) AdsDiagnostics.Last = "privacy options update error: " + updateError.Message;
                    if (PrivacyOptionsRequired) OpenPrivacyForm();
                    else OpenConsentForm();
                }));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                PrivacyFeedback?.Invoke("Privacy options are not available right now. Check your connection and try again.");
            }
        }

        static void OpenPrivacyForm()
        {
            ConsentForm.ShowPrivacyOptionsForm(formError => AdMobDispatcher.Post(() =>
            {
                if (formError != null)
                {
                    Debug.LogWarning("[Ads] Opzioni privacy: " + formError.Message);
                    AdsDiagnostics.Last = "privacy options error: " + formError.Message;
                    OpenConsentForm();
                    return;
                }
                AdsDiagnostics.Last = "consent status: " + ConsentInformation.ConsentStatus;
                if (instance != null && ConsentInformation.CanRequestAds()) instance.StartAds();
            }));
        }

        static void OpenConsentForm()
        {
            if (!ConsentInformation.IsConsentFormAvailable())
            {
                PrivacyFeedback?.Invoke("No privacy choices are needed in your region.");
                return;
            }
            ConsentForm.Load((form, loadError) => AdMobDispatcher.Post(() =>
            {
                if (loadError != null || form == null)
                {
                    AdsDiagnostics.Last = "consent form load error: " + (loadError != null ? loadError.Message : "null");
                    PrivacyFeedback?.Invoke("Privacy options are not available right now. Check your connection and try again.");
                    return;
                }
                form.Show(showError => AdMobDispatcher.Post(() =>
                {
                    if (showError != null) AdsDiagnostics.Last = "consent form error: " + showError.Message;
                    else AdsDiagnostics.Last = "consent status: " + ConsentInformation.ConsentStatus;
                    if (instance != null && ConsentInformation.CanRequestAds()) instance.StartAds();
                }));
            }));
        }

        void StartAds()
        {
            if (started) return;
            started = true;
            AdsDiagnostics.Last = "initializing AdMob" + (AdMobIds.UsingTestAds ? " (TEST ads)" : "");
            MobileAds.Initialize(status => AdMobDispatcher.Post(() =>
            {
                AdsDiagnostics.Last = "AdMob started, loading ads" + (AdMobIds.UsingTestAds ? " (TEST ads)" : "");
                LoadInterstitial();
                LoadRewarded();
                LoadRewardedInterstitial();
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
                    AdsDiagnostics.Last = "interstitial load error: " + (error != null ? error.GetMessage() : "null");
                    AdMobDispatcher.Later(interstitialRetry, LoadInterstitial);
                    interstitialRetry = Mathf.Min(interstitialRetry * 2f, 120f);
                    return;
                }
                interstitialRetry = 5f;
                interstitial = ad;
                AdsDiagnostics.Last = "interstitial loaded";
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
                    AdsDiagnostics.Last = "rewarded load error: " + (error != null ? error.GetMessage() : "null");
                    AdMobDispatcher.Later(rewardedRetry, LoadRewarded);
                    rewardedRetry = Mathf.Min(rewardedRetry * 2f, 120f);
                    return;
                }
                rewardedRetry = 5f;
                rewarded = ad;
                AdsDiagnostics.Last = "rewarded loaded";
            }));
        }

        /// <summary>
        /// Rewarded interstitial: si usa solo quando il giocatore ha già scelto di guardare uno spot per il premio
        /// (pulsante "WATCH AD") e il rewarded normale non è pronto. Così c'è sempre il consenso esplicito al premio.
        /// </summary>
        void LoadRewardedInterstitial()
        {
            string id = AdMobIds.RewardedInterstitial;
            if (string.IsNullOrEmpty(id) || loadingRewardedInterstitial || rewardedInterstitial != null) return;
            loadingRewardedInterstitial = true;
            RewardedInterstitialAd.Load(id, new AdRequest(), (ad, error) => AdMobDispatcher.Post(() =>
            {
                loadingRewardedInterstitial = false;
                if (error != null || ad == null)
                {
                    Debug.LogWarning("[Ads] Rewarded interstitial non caricato: " + (error != null ? error.GetMessage() : "null"));
                    AdMobDispatcher.Later(rewardedInterstitialRetry, LoadRewardedInterstitial);
                    rewardedInterstitialRetry = Mathf.Min(rewardedInterstitialRetry * 2f, 120f);
                    return;
                }
                rewardedInterstitialRetry = 5f;
                rewardedInterstitial = ad;
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
            if (ad == null || !ad.CanShowAd())
            {
                LoadRewarded();
                if (RewardedInterstitialReady) { ShowRewardedInterstitial(onDone); return; }
                onDone?.Invoke(false);
                return;
            }
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

        void ShowRewardedInterstitial(Action<bool> onDone)
        {
            RewardedInterstitialAd ad = rewardedInterstitial;
            rewardedInterstitial = null;
            bool earned = false, done = false;
            Action finish = () =>
            {
                if (done) return;
                done = true;
                ad.Destroy();
                Resume();
                onDone?.Invoke(earned);
                LoadRewardedInterstitial();
            };
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
