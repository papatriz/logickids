using UnityEngine;
using Orbox.Async;
using DaikonForge.Tween;
using System;
using Orbox.Utils;
using UnityEngine.UI;
using KidGame.Lessons;
using System.Collections.Generic;
using UnityEngine.Events;
using KidGame.Service;

namespace KidGame.UI
{

    public class MenuView : MonoBehaviour, IMenuView
    {
        public event Action<ELessons> StartLessonButtonClicked = (t) => { };
        public event Action CollectionButtonClicked;
        public event Action SubscribeButtonClicked;
        public event Action RestoreButtonClicked;

        public event Action ToggleMusicClicked;
        public event Action ToggleSoundClicked;
        public event Action<SystemLanguage> ChangeLanguageClicked;

        public event Action TestButtonClicked;
        
        public Button[] LessonButtons;

        // tmp:
        public Color ActiveColor;
        public Color LockedColor;
        // ---

        public Button CollectionButton;

        public ScreenShading Shading;

        public Button TestButton;
        public Button SettingsButton;

        public GameObject WaitIndicator;

        private BuyDialog BuyDialog;
        private OptionsDialog OptionsDialog;

        private static IResourceManager ResourceManager;
        private static IGame Game;
        private Awaker Awaker;

        private bool IsPaidLessonsAvailable = false;

        void Awake()
        {
            if (Awaker.IsAwaked)
                return;

            ResourceManager = CompositionRoot.GetResourceManager();
            Game = CompositionRoot.GetGame();

            BuyDialog = (Game.Layout == Layouts.Tablet) ? ResourceManager.CreatePrefabInstance<EWidgets, BuyDialog>(EWidgets.BuyDialogTablet) : ResourceManager.CreatePrefabInstance<EWidgets, BuyDialog>(EWidgets.BuyDialogSmartphone);
            BuyDialog.transform.SetParent(transform, false);
            BuyDialog.transform.SetSiblingIndex(2);
            BuyDialog.gameObject.SetActive(false);

            OptionsDialog = (Game.Layout == Layouts.Tablet) ? ResourceManager.CreatePrefabInstance<EWidgets, OptionsDialog>(EWidgets.OptionsDialogTablet) : ResourceManager.CreatePrefabInstance<EWidgets, OptionsDialog>(EWidgets.OptionsDialogSmartphone);
            OptionsDialog.transform.SetParent(transform, false);
            OptionsDialog.transform.SetSiblingIndex(3);
            OptionsDialog.gameObject.SetActive(false);

            OptionsDialog.LangButtonClicked += (lang) => ChangeLanguageClicked(lang);
            OptionsDialog.MusicStateSwitched += () => ToggleMusicClicked();
            OptionsDialog.SoundStateSwitched += () => ToggleSoundClicked();
            OptionsDialog.CloseClicked += () => HideOptionsDialog();

            ChangeLanguage();

            DisableInput();

        }

        public void SetParent(Transform parent)
        {
            Awake();

            transform.SetParent(parent, false);

        }

        public void ChangeLanguage()
        {
           
        }

        public void SetupBuyDialog(bool isTrialAvailable, string price, bool isInit)
        {
            BuyDialog.Setup(isTrialAvailable, price, isInit);
        }

        public void SetupOptionsDialog(bool musicEnable, bool soundEnable, SystemLanguage currentLang)
        {
            OptionsDialog.SetInitialState(musicEnable, soundEnable, currentLang);
        }

        public IPromise Hide()
        {
            return Shading.Shade(true, 0.5f)
                .Done(() => gameObject.SetActive(false));
        }

        public IPromise Show()
        {
            gameObject.SetActive(true);
            Shading.ShadeInstant();

            return Shading.Shade(false, 1.0f);
        }

        public void ShowInstant()
        {
            gameObject.SetActive(true);
            Shading.UnshadeInstant();
        }

        public void HideInstant()
        {
            gameObject.SetActive(false);
        }

        public IPromise HideBuyDialog()
        {
            BuyDialog.transform.SetSiblingIndex(2);
            WaitIndicator.SetActive(false);

            var unshade = Shading.Shade(false, 0.5f);
            var winHide = BuyDialog.Hide();

            return Deferred.All(winHide, unshade);
        }

        public IPromise HideOptionsDialog()
        {
            var unshade = Shading.Shade(false, 0.5f);
            var winHide = OptionsDialog.Hide();

            return Deferred.All(winHide, unshade).Done(EnableInput);
        }

        public void EnableInput()
        {
            for (int i = 0; i < LessonButtons.Length; i++)
            {
                var j = i;
                var lesson = (ELessons)j;

                if (IsPaidLessonsAvailable || Game.IsLessonFree(lesson))
                {
                    LessonButtons[i].onClick.AddListener(() => HandleStartLessonButtonClicked(lesson));
                    LessonButtons[i].GetComponent<Image>().color = ActiveColor;
                }
                else
                {
                    LessonButtons[i].onClick.AddListener(() => HandleLockedLessonClicked(lesson));
                    LessonButtons[i].GetComponent<Image>().color = LockedColor;
                }
            }

            CollectionButton.onClick.AddListener(HandleCollectionButtonClicked);
            SettingsButton.onClick.AddListener(HandleSettingsButtonClicked);


            //DEBUG TOOLS:
            TestButton.onClick.AddListener(() => HandleTestButton());
            //
            
        }

        // DEBUG: ==============
        void HandleTestButton()
        {
            TestButtonClicked();
        }
        // =====================

        void HandleSettingsButtonClicked()
        {
            DisableInput();
            Shading.Shade(true, 0.5f, 0.5f);

            OptionsDialog.Show();
        }

        public void DisableInput()
        {
            for (int i = 0; i < LessonButtons.Length; i++)
            {
               LessonButtons[i].onClick.RemoveAllListeners();
            }

            CollectionButton.onClick.RemoveAllListeners();
            SettingsButton.onClick.RemoveAllListeners();
        }

        void DisableBuyDialogInteraction()
        {
            BuyDialog.SubscribeButton.onClick.RemoveAllListeners();
            BuyDialog.CloseButton.onClick.RemoveAllListeners();
            BuyDialog.RestoreButton.onClick.RemoveAllListeners();
        }

        void EnableBuyDialogInteraction()
        {
            BuyDialog.Show();
            BuyDialog.SubscribeButton.onClick.AddListener(() => HandleSubscribeButtonClicked());
            BuyDialog.CloseButton.onClick.AddListener(() => HandleCloseBuyDialogButtonClicked());
            BuyDialog.RestoreButton.onClick.AddListener(() => HandleRestoreButtonClicked());
        }

        void HandleCloseBuyDialogButtonClicked()
        {
            Shading.Shade(false, 0.5f);
            BuyDialog.Hide();
            EnableInput();
        }

        void HandleSubscribeButtonClicked()
        {
            DisableBuyDialogInteraction();
            BuyDialog.transform.SetSiblingIndex(1);
            WaitIndicator.SetActive(true);

            SubscribeButtonClicked();
        }

        void HandleLockedLessonClicked(ELessons lesson)
        {
            DisableInput();

            //toDO: incapsulate all this trash in BuyDialog
            DisableBuyDialogInteraction();

            Shading.Shade(true, 0.5f, 0.5f);

            BuyDialog.Show();
            EnableBuyDialogInteraction();

            //Debug.Log("Toss a coin to your witcher to unlock lesson "+lesson);
        }

        void HandleRestoreButtonClicked()
        {
            RestoreButtonClicked();
            Shading.Shade(false, 0.5f);
            BuyDialog.Hide();
        }

        void HandleCollectionButtonClicked()
        {
            CollectionButtonClicked();
        }

        void HandleStartLessonButtonClicked(ELessons lesson)
        {
            StartLessonButtonClicked(lesson);
        }

        public void SetLessonsAvailability(bool isAvailable)
        {
            IsPaidLessonsAvailable = isAvailable;
        }
    }
}