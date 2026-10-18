using System;

namespace Gotcha.Store
{
    /// <summary>Prodotti in vendita (gli stessi ID vanno creati nella Google Play Console / App Store Connect).</summary>
    public static class Products
    {
        /// <summary>0,99 €: un'ora di vite infinite (consumabile, si può ricomprare).</summary>
        public const string InfiniteLives1h = "gotcha.infinite_lives_1h";
        /// <summary>4,99 €: niente più pubblicità tra un livello e l'altro, per sempre (non consumabile).</summary>
        public const string RemoveAds = "gotcha.remove_ads";
        /// <summary>0,39 €: 3 hint (consumabile).</summary>
        public const string Hints3 = "gotcha.hints_3";
        /// <summary>1,49 €: 15 hint (consumabile).</summary>
        public const string Hints15 = "gotcha.hints_15";
    }

    /// <summary>Negozio: Unity IAP sul telefono, simulato nell'Editor o se il pacchetto manca.</summary>
    public interface IStore
    {
        bool IsReady { get; }
        /// <summary>Avvia il negozio; <paramref name="onGranted"/> riceve ogni prodotto pagato (anche quelli ripristinati).</summary>
        void Initialize(Action<string> onGranted);
        void Buy(string productId, Action<bool, string> onDone);
        /// <summary>Prezzo nella valuta del giocatore (null se non ancora noto).</summary>
        string LocalizedPrice(string productId);
        void Restore(Action<bool> onDone);
    }
}
