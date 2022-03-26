using System;
using UnityEngine.Purchasing;


namespace KidGame.Service
{
    public interface IPaymentManager : IStoreListener
    {
        event Action SubscriptionStatusUpdated;
        event Action StorePurchaseFlowFinished;

        bool IsInitialized();
        void BuySubscription();
        void Restore();
    }
}
