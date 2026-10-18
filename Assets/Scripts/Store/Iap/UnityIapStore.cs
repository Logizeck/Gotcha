#if GOTCHA_IAP
using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using Unity.Services.Core;

namespace Gotcha.Store
{
    /// <summary>
    /// Negozio vero con Unity IAP (pacchetto com.unity.purchasing). Questo assembly esiste solo se il pacchetto è
    /// installato; il gioco lo cerca per nome (StoreManager) e altrimenti usa il negozio simulato.
    /// </summary>
    public class UnityIapStore : IStore, IDetailedStoreListener
    {
        IStoreController controller;
        IExtensionProvider extensions;
        Action<string> granted;
        Action<bool, string> pending;
        string pendingId;

        public bool IsReady => controller != null;

        public async void Initialize(Action<string> onGranted)
        {
            granted = onGranted;
            try { await UnityServices.InitializeAsync(); }
            catch (Exception e) { Debug.LogWarning("[Gotcha] Unity Services non inizializzati: " + e.Message); }
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct(Products.InfiniteLives1h, ProductType.Consumable);
            builder.AddProduct(Products.RemoveAds, ProductType.NonConsumable);
            builder.AddProduct(Products.Hints3, ProductType.Consumable);
            builder.AddProduct(Products.Hints15, ProductType.Consumable);
            UnityPurchasing.Initialize(this, builder);
        }

        public void Buy(string productId, Action<bool, string> onDone)
        {
            if (controller == null) { onDone?.Invoke(false, "Store not ready"); return; }
            pending = onDone;
            pendingId = productId;
            controller.InitiatePurchase(productId);
        }

        public string LocalizedPrice(string productId)
        {
            Product p = controller != null ? controller.products.WithID(productId) : null;
            return p != null && p.availableToPurchase ? p.metadata.localizedPriceString : null;
        }

        public void Restore(Action<bool> onDone)
        {
            IAppleExtensions apple = extensions != null ? extensions.GetExtension<IAppleExtensions>() : null;
            if (apple == null) { onDone?.Invoke(true); return; }   // Android: gli acquisti tornano da soli all'avvio
            apple.RestoreTransactions((ok, message) => onDone?.Invoke(ok));
        }

        public void OnInitialized(IStoreController c, IExtensionProvider e)
        {
            controller = c;
            extensions = e;
            Product noAds = c.products.WithID(Products.RemoveAds);
            if (noAds != null && noAds.hasReceipt) granted?.Invoke(Products.RemoveAds);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogWarning("[Gotcha] Negozio non disponibile: " + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogWarning("[Gotcha] Negozio non disponibile: " + error + " " + message);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            string id = args.purchasedProduct.definition.id;
            granted?.Invoke(id);
            if (pendingId == id) Finish(true, null);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Finish(false, reason.ToString());
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription description)
        {
            Finish(false, description.message);
        }

        void Finish(bool ok, string message)
        {
            Action<bool, string> cb = pending;
            pending = null;
            pendingId = null;
            cb?.Invoke(ok, message);
        }
    }
}
#endif
