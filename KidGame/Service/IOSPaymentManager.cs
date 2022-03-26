using System;
using System.Collections.Generic;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using UnityEngine.UI;

namespace KidGame.Service
{
    public class IOSPaymentManager :IPaymentManager
    {
        private bool isInitialized;

        public event Action SubscriptionStatusUpdated;
        public event Action StorePurchaseFlowFinished;

        private readonly string SubscriptionMonthlyId = "com.genngoldman.logickid.monthly";

        IStoreController m_StoreController;
        IAppleExtensions m_AppleExtensions;

        private SubsData SubscriptionData;

        public IOSPaymentManager()
        {
            SubscriptionData = CompositionRoot.GetSubsData();
            Debug.Log("Load SubsData in PaymentManager\nSubscription is active = " + SubscriptionData.SubscriptionIsActive + "\nExpirationDate: " + SubscriptionData.ExpirationDate + "\nEligible for trial: " + SubscriptionData.FreeTrialAvailable);

            isInitialized = false;
            InitializePurchasing();
        }

        void InitializePurchasing()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            builder.AddProduct(SubscriptionMonthlyId, ProductType.Subscription);
            var appReceipt = builder.Configure<IAppleConfiguration>().appReceipt;

            if (!string.IsNullOrEmpty(appReceipt)) CheckTrialEligibilityFromAppReceipt(appReceipt);

            Debug.Log("Product ID sended to builder: " + SubscriptionMonthlyId);

            UnityPurchasing.Initialize(this, builder);
        }

        public void BuySubscription()
        {
            m_StoreController.InitiatePurchase(SubscriptionMonthlyId);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {

            m_StoreController = controller;
            m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();

            isInitialized = true;
            Debug.Log("In-App Purchasing successfully initialized");

            Dictionary<string, string> introductory_info_dict = m_AppleExtensions.GetIntroductoryPriceDictionary();

            var product = controller.products.WithID(SubscriptionMonthlyId);
            var price = product.metadata.localizedPriceString;

            if (product.receipt != null)
            {
                SubscriptionManager subscriptionManager = new SubscriptionManager(product, null);

                var info = subscriptionManager.getSubscriptionInfo();

                var isActive = info.isSubscribed() == Result.True;
                var expDate = info.getExpireDate();

                UpdateSubStatus(price, isActive, expDate);
            }
            else
            {
                UpdateSubStatus(price, SubscriptionData.SubscriptionIsActive, SubscriptionData.ExpirationDate);
            }

            SubscriptionStatusUpdated();

        }

        public void Restore()
        {
            m_AppleExtensions.RestoreTransactions((s) => { Debug.Log("Restoring complete, with result:" + s); });
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            isInitialized = false;
            Debug.Log($"In-App Purchasing initialize failed: {error}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var product = args.purchasedProduct;

            Debug.Log($"Purchase Complete - Product: {product.definition.id}");

            SubscriptionManager subscriptionManager = new SubscriptionManager(product, null);

            var info = subscriptionManager.getSubscriptionInfo();

            var price = product.metadata.localizedPriceString;
            var isActive = info.isSubscribed() == Result.True;
            var expDate = info.getExpireDate();

            UpdateSubStatus(price, isActive, expDate);
            StorePurchaseFlowFinished();

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            StorePurchaseFlowFinished();

            Debug.Log($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");
        }


        void CheckTrialEligibilityFromAppReceipt(string appreceipt) // toDo: remove checking expiration date after test
        {
            bool isTrialAvailable = true;

            var receiptData = Convert.FromBase64String(appreceipt);
            var parcer = new AppleReceiptParser();
            AppleReceipt appleReceipt = parcer.Parse(receiptData);

            foreach (var item in appleReceipt.inAppPurchaseReceipts)
            {
                if (item.isFreeTrial > 0) isTrialAvailable = false;
            }

            SetTrialEligibility(isTrialAvailable);
        }


        void SetTrialEligibility(bool value)
        {
            SubscriptionData.FreeTrialAvailable = value;
            BinarySaveSystem.Save(SubscriptionData);
        }

        void UpdateSubStatus(string localPrice, bool isActive, DateTime expDate)
        {
            SubscriptionData.LocalizedPrice = localPrice;
            SubscriptionData.SubscriptionIsActive = isActive;
            SubscriptionData.ExpirationDate = expDate;
            BinarySaveSystem.Save(SubscriptionData);
        }

        public bool IsInitialized()
        {
            return isInitialized;
        }
    }
}
