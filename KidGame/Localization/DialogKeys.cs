using Orbox.Localization;
using UnityEngine;

namespace KidGame.Localization
{
    //orbox: TODO: Убрать все енумы из меты
    public enum DialogKeys
    {
        BuyTrialHeader,
        BuyPaidHeader,
        BuyTrialButton,
        BuyPaidButton,
        RestorePurchase,
        Sound,
        Music,
        Close

    }

    public static class DialogLocalizationExtension
    {
        public static string GetNative(this DialogKeys key)
        {
            var game = CompositionRoot.GetGame();
            var localization = CompositionRoot.GetLocalizationManager();
            var result = localization.Get(key, game.Language);

            return result;
        }

        public static string GetEnglish(this DialogKeys key)
        {
            var localization = CompositionRoot.GetLocalizationManager();
            var result = localization.Get(key, SystemLanguage.English);

            return result;
        }

        public static string GetName(this DialogKeys value)
        {
            var cache = CompositionRoot.GetEnumCache();
            return cache.GetName<DialogKeys>(value);
        }

    }
}