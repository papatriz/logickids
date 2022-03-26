using KidGame.Service;
using KidGame.UI;
using Orbox.Utils;
using UnityEngine;

namespace KidGame
{

    public static class Platform
    {
        public static IViewFactory CreateViewFactory(ILayoutDetector layoutDetector, IResourceManager resourceManager)
        {
            IViewFactory factory;

            if (layoutDetector.Layout == Layouts.Tablet)
                factory = new TabletViewFactory(resourceManager);
            else

                factory = new SmartphoneViewFactory(resourceManager);

            return factory;
        }

        
        public static IPaymentManager CreatePaymentManager()
        {
            IPaymentManager payments;

#if UNITY_IOS
            // payments = new IOSPaymentManager();
            payments = new FakeGooglePaymentManager();

#elif UNITY_ANDROID
       //payments = new GooglePaymentManager();
         payments = new FakeGooglePaymentManager();
#else
        payments = new FakeGooglePaymentManager();
#endif

            return payments;
        }
        
    }
}