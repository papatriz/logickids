using System;

namespace KidGame.Service
{
    [System.Serializable]
    public class SubsData // toDO: Упростить все, навороты не нужны лишние, нужно экспирайшн дэйт, длительность периода, фри триал или нет. По максимум брать инфу из subscription manager
    {

        private DateTime expirationDate;
        private bool isSubscriptionActiveNow = false;
        private bool isFreeTrialAvailable = true;
        private string localPrice = "$0.99";


        public DateTime ExpirationDate
        {
            get { return expirationDate; }
            set { expirationDate = value; }
        }

        public bool SubscriptionIsActive
        {
            get
            {
                if (!isSubscriptionActiveNow) return false;

                return (DateTime.Now - TimeSpan.FromHours(3)) <= expirationDate;  // DEBUG 
              //  return DateTime.Now.Date <= expirationDate.Date + TimeSpan.FromDays(1); // toDO: RELEASE
            }

            set { isSubscriptionActiveNow = value; }
        }

        public String LocalizedPrice
        {
            get { return localPrice;  }
            set { localPrice = value; }
        }

        public bool FreeTrialAvailable
        {
            get { return isFreeTrialAvailable;  }
            set { isFreeTrialAvailable = value; }
        }

    }
}