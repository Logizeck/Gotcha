using System;

namespace Gotcha.Ads
{
    /// <summary>
    /// Build per lo store senza AdMob: nessuna pubblicità (niente spot finti in una versione pubblicata).
    /// Le pause tra i livelli non partono e i pulsanti "WATCH AD" restano nascosti.
    /// </summary>
    public class NoAdsProvider : IAdProvider
    {
        public bool IsReady => false;
        public bool RewardedReady => false;
        public void Show(Action onClosed) { onClosed?.Invoke(); }
        public void ShowRewarded(Action<bool> onDone) { onDone?.Invoke(false); }
    }
}
