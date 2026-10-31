using System;
using UnityEngine;

namespace Gotcha.Ads
{
    public interface IAdProvider
    {
        /// <summary>Pausa pubblicitaria (interstitial) pronta.</summary>
        bool IsReady { get; }
        /// <summary>Pubblicità con premio pronta.</summary>
        bool RewardedReady { get; }
        void Show(Action onClosed);
        /// <summary>Pubblicità con premio: onDone(true) se è stata guardata fino in fondo.</summary>
        void ShowRewarded(Action<bool> onDone);
    }

    /// <summary>Provider finto per Editor e test: nessuna pubblicità reale, chiude subito.</summary>
    public class LogAdProvider : IAdProvider
    {
        public bool IsReady => true;
        public bool RewardedReady => true;

        public void Show(Action onClosed)
        {
            Debug.Log("[Ads] Pausa pubblicitaria (simulata)");
            onClosed?.Invoke();
        }

        public void ShowRewarded(Action<bool> onDone)
        {
            Debug.Log("[Ads] Pubblicità con premio (simulata)");
            onDone?.Invoke(true);
        }
    }

    /// <summary>
    /// Regola: pubblicità ogni 5 livelli superati OPPURE dopo 140 secondi di gioco attivo (timer invisibile),
    /// quello che avviene prima. Entrambi i contatori si azzerano dopo ogni pubblicità mostrata.
    /// La pubblicità parte solo in TryShowBreak, cioè alla fine del livello in corso (superato o fallito): chi resta
    /// bloccato più di 140 s su un livello la vede al termine del tentativo. Se non è pronta, i contatori restano e
    /// si riprova alla fine del livello successivo.
    /// </summary>
    public class AdBreakManager
    {
        IAdProvider provider;

        public int LevelsInterval { get; set; } = 5;
        public float SecondsInterval { get; set; } = 140f;   // 140 secondi di gioco attivo
        public int LevelsSinceAd { get; private set; }
        public float PlaySecondsSinceAd { get; private set; }

        public AdBreakManager(IAdProvider provider) { this.provider = provider; }

        public void SetProvider(IAdProvider newProvider) { provider = newProvider; }

        /// <summary>Acquisto "niente pubblicità": le pause tra i livelli spariscono (quella con premio resta, è volontaria).</summary>
        public Func<bool> AdsRemoved { get; set; }

        /// <summary>C'è una pubblicità con premio da guardare (altrimenti la UI nasconde i pulsanti "WATCH AD").</summary>
        public bool RewardedReady => provider != null && provider.RewardedReady;

        public bool IsDue => (AdsRemoved == null || !AdsRemoved()) && (LevelsSinceAd >= LevelsInterval || PlaySecondsSinceAd >= SecondsInterval);

        public void Tick(float deltaTime, bool isPlaying)
        {
            if (isPlaying) PlaySecondsSinceAd += deltaTime;
        }

        public void RegisterLevelResult(bool solved)
        {
            if (solved) LevelsSinceAd++;
        }

        /// <summary>Pubblicità con premio (es. una vita). Conta anche come pausa pubblicitaria: azzera i contatori.</summary>
        public void ShowRewarded(Action<bool> onDone)
        {
            if (!RewardedReady) { onDone?.Invoke(false); return; }
            provider.ShowRewarded(ok =>
            {
                if (ok) { LevelsSinceAd = 0; PlaySecondsSinceAd = 0f; }
                onDone?.Invoke(ok);
            });
        }

        /// <summary>Stato per il pannello di prova: tipo di pubblicità, spot pronti, ultimo evento.</summary>
        public string Status =>
            (provider != null ? provider.GetType().Name : "none") + "\ninterstitial: " + (provider != null && provider.IsReady ? "READY" : "not ready") +
            "  rewarded: " + (provider != null && provider.RewardedReady ? "READY" : "not ready") + "\n" + AdsDiagnostics.Last;

        /// <summary>Pannello di prova: mostra subito una pausa pubblicitaria (ignora contatori e "niente pubblicità").</summary>
        public void ShowTestBreak(Action done)
        {
            if (provider != null && provider.IsReady) provider.Show(done);
            else done?.Invoke();
        }

        /// <summary>Pannello di prova: mostra subito uno spot con premio (il premio non viene dato).</summary>
        public void ShowTestRewarded(Action<bool> done)
        {
            if (provider != null && provider.RewardedReady) provider.ShowRewarded(done);
            else done?.Invoke(false);
        }

        public void TryShowBreak(Action proceed)
        {
            if (IsDue && provider != null && provider.IsReady)
            {
                provider.Show(() =>
                {
                    LevelsSinceAd = 0;
                    PlaySecondsSinceAd = 0f;
                    proceed?.Invoke();
                });
            }
            else
            {
                proceed?.Invoke();
            }
        }
    }
}
