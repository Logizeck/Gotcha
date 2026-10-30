using System;
using UnityEngine;
using Gotcha.Store;

namespace Gotcha.Core
{
    /// <summary>
    /// Acquisti in-app del gioco free-to-play: 1 ora di vite infinite (0,99 €), niente pubblicità per sempre (4,99 €),
    /// pacchetti di hint (3 a 0,39 €, 15 a 1,49 €).
    /// Sul telefono usa Unity IAP se il pacchetto è installato (assembly Gotcha.Iap); altrimenti un negozio simulato
    /// che nell'Editor "compra" subito (per le prove) e in una build vera risponde "non disponibile".
    /// </summary>
    public class StoreManager : MonoBehaviour
    {
        public static StoreManager Instance { get; private set; }

        public event Action Changed;   // un acquisto è andato a buon fine (o è stato ripristinato)

        IStore store;

        public bool AdsRemoved => SaveData.AdsRemoved;
        public bool IsReady => store != null && store.IsReady;

        void Awake()
        {
            Instance = this;
            store = CreateStore();
            store.Initialize(Grant);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        static IStore CreateStore()
        {
            // nell'Editor sempre il negozio simulato: niente Unity Services (che senza un Project ID collegato
            // riempirebbero la console di avvisi) e acquisti provabili subito
            if (Application.isEditor) return new SimulatedStore();
            Type real = Type.GetType("Gotcha.Store.UnityIapStore, Gotcha.Iap");
            if (real != null && Activator.CreateInstance(real) is IStore s) return s;
            return new SimulatedStore();
        }

        /// <summary>
        /// Prezzo da mostrare sul pulsante: quello di Google Play se c'è (simboli resi leggibili), altrimenti il listino
        /// in euro convertito nella valuta del giocatore e arrotondato (LocalPrice).
        /// </summary>
        public string Price(string productId)
        {
            string p = store != null ? store.LocalizedPrice(productId) : null;
            if (!string.IsNullOrEmpty(p)) return LocalPrice.Readable(p);
            switch (productId)
            {
                case Products.RemoveAds: return LocalPrice.FromEuro(4.99);
                case Products.Hints3: return LocalPrice.FromEuro(0.39);
                case Products.Hints15: return LocalPrice.FromEuro(1.49);
                default: return LocalPrice.FromEuro(0.99);
            }
        }

        public void Buy(string productId, Action<bool, string> onDone)
        {
            if (productId == Products.RemoveAds && SaveData.AdsRemoved) { onDone?.Invoke(true, null); return; }
            store.Buy(productId, onDone);
        }

        public void Restore(Action<bool> onDone) { store.Restore(onDone); }

        void Grant(string productId)
        {
            if (productId == Products.InfiniteLives1h)
            {
                if (LivesManager.Instance != null) LivesManager.Instance.GrantInfinite(3600f);
            }
            else if (productId == Products.RemoveAds)
            {
                SaveData.AdsRemoved = true;
            }
            else if (productId == Products.Hints3) HintWallet.Add(3);
            else if (productId == Products.Hints15) HintWallet.Add(15);
            Changed?.Invoke();
        }

        /// <summary>Negozio finto: nell'Editor ogni acquisto riesce (per provare il gioco), altrove non è disponibile.</summary>
        class SimulatedStore : IStore
        {
            Action<string> granted;
            public bool IsReady => Application.isEditor;
            public void Initialize(Action<string> onGranted) { granted = onGranted; }
            public string LocalizedPrice(string productId) { return null; }
            public void Restore(Action<bool> onDone) { onDone?.Invoke(true); }

            public void Buy(string productId, Action<bool, string> onDone)
            {
                if (!Application.isEditor) { onDone?.Invoke(false, "Store not available"); return; }
                Debug.Log("[Gotcha] Acquisto simulato: " + productId);
                granted?.Invoke(productId);
                onDone?.Invoke(true, null);
            }
        }
    }
}
