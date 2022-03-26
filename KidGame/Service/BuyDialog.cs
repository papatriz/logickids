using System.Collections;
using System.Collections.Generic;
using KidGame.Localization;
using KidGame.UI;
using Orbox.Async;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.Service
{
    public class BuyDialog : TweenScalable
    {
        public Button SubscribeButton;
        public Button CloseButton;

        public Button RestoreButton;

        public Text TrialButton;
        public Text PaidButton;

        public Text TrialHeader;
        public Text PaidHeader;

        public ParentalGate ParentalGate;
        public GameObject OfflineLock;

        public override IPromise Show()
        {
            RestoreButton.gameObject.SetActive(false);

#if UNITY_IOS 
            ParentalGate.Show();
            RestoreButton.gameObject.SetActive(true);
#endif
            return base.Show();
        }

        public void Setup(bool isEntitledForTrial, string localPrice, bool isPaymentInit)
        {
            string trialHeaderTemplate = DialogKeys.BuyTrialHeader.GetNative();
            string paidHeaderTemplate = DialogKeys.BuyPaidHeader.GetNative();

            var trialText = string.Format(trialHeaderTemplate, localPrice);
            var paidText = string.Format(paidHeaderTemplate, localPrice);

            TrialHeader.text = trialText;
            PaidHeader.text = paidText;

            TrialButton.text = DialogKeys.BuyTrialButton.GetNative();//.ToUpper();
            PaidButton.text = DialogKeys.BuyPaidButton.GetNative().ToUpper();

            TrialButton.gameObject.SetActive(isEntitledForTrial);
            TrialHeader.gameObject.SetActive(isEntitledForTrial);

            PaidButton.gameObject.SetActive(!isEntitledForTrial);
            PaidHeader.gameObject.SetActive(!isEntitledForTrial);

            OfflineLock.SetActive(!isPaymentInit);

        }

    }
}