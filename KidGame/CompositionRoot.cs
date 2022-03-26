using UnityEngine;
using Orbox.Utils;
using Orbox.Signals;
using KidGame.UI;
using KidGame.Service;
using Orbox.Localization;
using KidGame.Localization;

namespace KidGame
{
    public class CompositionRoot : MonoBehaviour
    {
        private static IGame Game;

        private static ISoundManager SoundManager;
        private static ITimers Timers;

        private static IResourceManager ResourceManager;
        private static ILocalizationManager LocalizationManager;
        private static ILanguageProvider LanguageProvider;
        private static IViewFactory ViewFactory;

        private static IPlayerPrefs PlayerPrefs;
        private static IEventPublisher EventPublisher;

        private static IPaymentManager PaymentManager;

        private static SubsData SubsData;

        public void OnDestroy()
        {
            Debug.Log("CompositionRoot OnDestroy");

            SoundManager = null;
            Timers = null;
            EventPublisher = null;

            if (ResourceManager != null)
                ResourceManager.ClearPool();


            PlayerPrefs = null;

        }

        public static SubsData GetSubsData()
        {
            if (SubsData == null)
            {
               // SubsData = new SubsData();
                SubsData = BinarySaveSystem.Load();
            }

            return SubsData;
        }

        public static IGame GetGame()
        {
            if (Game == null)
            {
                var prefs = GetPlayerPrefs();

                Game = new Game(prefs);

            }

            return Game;
        }

        public static IEnumCache GetEnumCache()
        {
            //orbox: заменить 
            return new EnumCache();
        }

        public static ILanguageProvider GetLanguageProvider()
        {
            if (LanguageProvider == null)
            {
                LanguageProvider = new LanguageProvider();
            }

            return LanguageProvider;
        }

        public static IViewFactory GetViewFactory()
        {
            if (ViewFactory == null)
            {
                var resourceManager = GetResourceManager();
                var layout = new LayoutDetector(GetGame());

                ViewFactory = Platform.CreateViewFactory(layout, resourceManager); 
            }

            return ViewFactory;
        }

        public static IPaymentManager GetPaymentManager()
        {
            if (PaymentManager == null)
            {

                PaymentManager = Platform.CreatePaymentManager();
            }

            return PaymentManager;
        }


        public static ITimers GetTimers()
        {
            if (Timers == null)
                Timers = MonoExtensions.MakeComponent<Timers>();

            return Timers;
        }


        public static IResourceManager GetResourceManager()
        {
            if (ResourceManager == null)
            {
                ResourceManager = new ResourceManager();
            }

            return ResourceManager;
        }


        public static IEventPublisher GetEventPublisher()
        {
            if (EventPublisher == null)
            {
                EventPublisher = MonoExtensions.MakeComponent<MBEventPublisher>();
            }

            return EventPublisher;
        }

        public static ISoundManager GetSoundManager()
        {
            if (SoundManager == null)
            {
                var rm = GetResourceManager();
                var ep = GetEventPublisher();

                SoundManager = new SoundManager(rm, ep);
            }

            return SoundManager;
        }

        public static ILocalizationManager GetLocalizationManager()
        {


            if (LocalizationManager == null)
            {
                var cache = GetEnumCache();
                var langProvider = GetLanguageProvider();
                LocalizationManager = new LocalizationManager(cache, langProvider);

                //Dialog Modules
                LocalizationManager.Add(new DialogModuleRussian());
                LocalizationManager.Add(new DialogModuleEnglish());
                /*
                LocalizationManager.Add(new DialogModuleSpanish());
                LocalizationManager.Add(new DialogModuleGerman());
                LocalizationManager.Add(new DialogModuleItalian());
                LocalizationManager.Add(new DialogModuleChinese());
                LocalizationManager.Add(new DialogModuleKorean());
                LocalizationManager.Add(new DialogModuleBrazilian());
                LocalizationManager.Add(new DialogModuleJapanese());
                LocalizationManager.Add(new DialogModuleFrench());

                LocalizationManager.Add(new DialogModuleAfrikaans());
                LocalizationManager.Add(new DialogModuleBulgarian());
                LocalizationManager.Add(new DialogModuleCzech());
                LocalizationManager.Add(new DialogModuleDanish());
                LocalizationManager.Add(new DialogModuleDutch());
                LocalizationManager.Add(new DialogModuleFinnish());
                LocalizationManager.Add(new DialogModuleGreek());
                LocalizationManager.Add(new DialogModuleHungarian());
                LocalizationManager.Add(new DialogModuleIndonesian());
                LocalizationManager.Add(new DialogModuleNorwegian());
                LocalizationManager.Add(new DialogModulePolish());
                LocalizationManager.Add(new DialogModuleRomanian());
                LocalizationManager.Add(new DialogModuleSwedish());
                LocalizationManager.Add(new DialogModuleThai());
                LocalizationManager.Add(new DialogModuleTurkish());
                LocalizationManager.Add(new DialogModuleVietnamese());

                */

            }

            return LocalizationManager;
        }

        public static IPlayerPrefs GetPlayerPrefs()
        {
            if (PlayerPrefs == null)
            {
               PlayerPrefs = new PlayerPrefsWrapper();
            }

            return PlayerPrefs;
        }

 

    }
}