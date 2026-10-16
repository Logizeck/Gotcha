using System;
using UnityEngine;
using Gotcha.Store;

namespace Gotcha.Core
{
    /// <summary>
    /// Acquisti in-app del gioco free-to-play: 1 ora di vite infinite (0,99 €) e niente pubblicità per sempre (4,99 €).
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

        /// <summary>Prezzo da mostrare sul pulsante (quello del negozio, o quello di listino finché non arriva).</summary>
        public string Price(string productId)
        {
            string p = store != null ? store.LocalizedPrice(productId) : null;
            if (!string.IsNullOrEmpty(p)) return p;
            return productId == Products.RemoveAds ? "4,99 €" : "0,99 €";
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
