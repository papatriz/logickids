using System;
using UnityEngine;
using Orbox.Async;
using KidGame.Lessons;

namespace KidGame.UI
{

    public interface IMenuView : IView
    {
        event Action<ELessons> StartLessonButtonClicked;
        event Action CollectionButtonClicked;
        event Action SubscribeButtonClicked;
        event Action RestoreButtonClicked;

        event Action ToggleMusicClicked;
        event Action ToggleSoundClicked;
        event Action<SystemLanguage> ChangeLanguageClicked;

        event Action TestButtonClicked; //FOR TEST

        void SetLessonsAvailability(bool isAvailable);
        void SetupBuyDialog(bool isTrialAvailable, string price, bool isInit);
        void SetupOptionsDialog(bool musicEnable, bool soundEnable, SystemLanguage currentLang);
        void ChangeLanguage();
        void EnableInput();
        void DisableInput();
        IPromise HideBuyDialog();

    }
}