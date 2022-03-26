using System;
using System.Collections.Generic;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using UnityEngine.UI;

namespace KidGame.Service
{
    public class FakeGooglePaymentManager : IPaymentManager
    {
        private bool isInitialized;

        public event Action SubscriptionStatusUpdated;
        public event Action StorePurchaseFlowFinished;

        private readonly string SubscriptionMonthlyId = "com.genngoldman.logickid.monthly";

        IStoreController m_StoreController;
        IAppleExtensions m_AppleExtensions;

        private SubsData SubscriptionData;

        private ITimers Timers;

        public FakeGooglePaymentManager()
        {
            SubscriptionData = CompositionRoot.GetSubsData();
            Debug.Log("Load SubsData in PaymentManager\nSubscription is active = " + SubscriptionData.SubscriptionIsActive + "\nExpirationDate: " + SubscriptionData.ExpirationDate + "\nEligible for trial: " + SubscriptionData.FreeTrialAvailable);

            Timers = CompositionRoot.GetTimers();

            isInitialized = false;
            InitializePurchasing();
        }

        void InitializePurchasing()
        {
            isInitialized = true;
        }

        public void BuySubscription()
        {
            Debug.Log("INITIATE PURCHASE");
            Timers.Wait(5f)
                .Done(FakeProcessPurchase);

        }

        void FakeProcessPurchase()
        {
            var price = "$4.99";
            var isActive = true;
            var expDate = DateTime.Now + TimeSpan.FromDays(3);

            UpdateSubStatus(price, isActive, expDate);
            StorePurchaseFlowFinished();
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            
        }

        public void Restore()
        {

        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {

        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {


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

            //var receiptData = Convert.FromBase64String(appreceipt);
            //var parcer = new AppleReceiptParser();
            //AppleReceipt appleReceipt = parcer.Parse(receiptData);

            //foreach (var item in appleReceipt.inAppPurchaseReceipts)
            //{
            //    if (item.isFreeTrial > 0) isTrialAvailable = false;
            //}

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
