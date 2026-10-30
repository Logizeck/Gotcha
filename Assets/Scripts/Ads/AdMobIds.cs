using UnityEngine;

namespace Gotcha.Ads
{
    /// <summary>
    /// ID di AdMob in un solo posto: Assets/Resources/AdMobIds.json (appId, interstitialId, rewardedId). L'App ID lo
    /// legge anche l'Editor (Gotcha.AdMobSetup) per impostarlo nel plugin prima di ogni build.
    /// Finché un ID è vuoto, e sempre nelle build di sviluppo, si usano gli ID di PROVA ufficiali di Google: annunci
    /// finti, sicuri da cliccare (cliccare annunci veri sul proprio telefono può far sospendere l'account AdMob).
    /// </summary>
    public static class AdMobIds
    {
        public const string TestAppId = "ca-app-pub-3940256099942544~3347511713";
        public const string TestInterstitialId = "ca-app-pub-3940256099942544/1033173712";
        public const string TestRewardedId = "ca-app-pub-3940256099942544/5224354917";

        [System.Serializable]
        class Ids { public string appId, interstitialId, rewardedId; }

        static Ids ids;

        static Ids Load()
        {
            if (ids != null) return ids;
            TextAsset file = Resources.Load<TextAsset>("AdMobIds");
            try { ids = file != null ? JsonUtility.FromJson<Ids>(file.text) : null; }
            catch { ids = null; }
            if (ids == null) ids = new Ids();
            return ids;
        }

        /// <summary>Annunci di prova: build di sviluppo o ID reali non ancora inseriti.</summary>
        public static bool UsingTestAds =>
            Debug.isDebugBuild || string.IsNullOrEmpty(Load().interstitialId) || string.IsNullOrEmpty(Load().rewardedId);

        public static string Interstitial => UsingTestAds ? TestInterstitialId : Load().interstitialId.Trim();
        public static string Rewarded => UsingTestAds ? TestRewardedId : Load().rewardedId.Trim();
    }
}
