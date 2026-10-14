using System;
using UnityEngine;

namespace Gotcha.Ads
{
    public interface IAdProvider
    {
        bool IsReady { get; }
        void Show(Action onClosed);
    }

    /// <summary>Provider finto per Editor e test: nessuna pubblicità reale, chiude subito.</summary>
    public class LogAdProvider : IAdProvider
    {
        public bool IsReady => true;

        public void Show(Action onClosed)
        {
            Debug.Log("[Ads] Pausa pubblicitaria (simulata)");
            onClosed?.Invoke();
        }
    }

    /// <summary>
    /// Regola: pubblicità ogni 5 livelli superati OPPURE dopo 3 minuti di gioco attivo (timer invisibile),
    /// quello che avviene prima. Entrambi i contatori si azzerano dopo ogni pubblicità mostrata.
    /// La pubblicità parte solo in TryShowBreak (cioè tra un livello e l'altro). Se non è pronta,
    /// i contatori restano e si riprova al livello successivo.
    /// </summary>
    public class AdBreakManager
    {
        IAdProvider provider;

        public int LevelsInterval { get; set; } = 5;
        public float SecondsInterval { get; set; } = 180f;   // 3 minuti di gioco attivo
        public int LevelsSinceAd { get; private set; }
        public float PlaySecondsSinceAd { get; private set; }

        public AdBreakManager(IAdProvider provider) { this.provider = provider; }

        public void SetProvider(IAdProvider newProvider) { provider = newProvider; }

        public bool IsDue => LevelsSinceAd >= LevelsInterval || PlaySecondsSinceAd >= SecondsInterval;

        public void Tick(float deltaTime, bool isPlaying)
        {
            if (isPlaying) PlaySecondsSinceAd += deltaTime;
        }

        public void RegisterLevelResult(bool solved)
        {
            if (solved) LevelsSinceAd++;
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
