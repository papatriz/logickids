
using KidGame.UI;
using Orbox.Utils;
using System.Collections.Generic;
using UnityEngine;

using KidGame.Lessons;
using KidGame.Lessons.MakePendant;
using KidGame.Lessons.RepairCrown;
using KidGame.Lessons.SortColoredGems;
using KidGame.Lessons.MakeMedal;
using KidGame.Lessons.Fruit01;
using KidGame.Lessons.Fruit02;
using KidGame.Lessons.Fruit03;
using KidGame.Lessons.Fruit04;
using KidGame.Lessons.Maze01;
using KidGame.Lessons.Maze02;
using KidGame.Lessons.Memory01;
using KidGame.Lessons.Memory02;
using KidGame.Lessons.Memory03;
using KidGame.Lessons.Memory04;
using KidGame.Lessons.Candy01;
using KidGame.Lessons.Candy02;
using KidGame.Lessons.Candy03;
using KidGame.Lessons.Candy04;
using KidGame.Lessons.Jelly01;
using KidGame.Lessons.GemOddOneOut;
using KidGame.Lessons.CandyOddOneOut;
using KidGame.Lessons.FruitOddOneOut;
using KidGame.Lessons.Jelly02;
using KidGame.Lessons.Jelly03;
using KidGame.Lessons.Jelly04;
using KidGame.Lessons.Jelly05;
using KidGame.Service;

namespace KidGame
{
    public class Menu : MonoBehaviour
    {

        public Transform UIRoot;
        //public PaymentManager PaymentManager;

        private IMenuView View;
        private IPaymentManager PaymentManager;

        private CollectionStore CollectionStore;

        private IGame Game;
        private IPlayerPrefs PlayerPrefs;
        private IResourceManager Resources;
        private ISoundManager SoundManager;

        private SubsData SubsData;

        //   private IPaymentManager Payments;

        private bool IsLessonsUnlocked = false;

        private Dictionary<ELessons, ILesson> LessonList = new Dictionary<ELessons, ILesson>();


        private void Awake()
        {
            Application.targetFrameRate = 60;

            PaymentManager = CompositionRoot.GetPaymentManager();
            var factory = CompositionRoot.GetViewFactory();

            PaymentManager.SubscriptionStatusUpdated += UpdateSubStatus;
            PaymentManager.StorePurchaseFlowFinished += CloseBuyDialogAfterStoreInteraction;

            Game = CompositionRoot.GetGame();
            SubsData = CompositionRoot.GetSubsData();

            PlayerPrefs = CompositionRoot.GetPlayerPrefs();

            SoundManager = CompositionRoot.GetSoundManager();
            Resources = CompositionRoot.GetResourceManager();

            IsLessonsUnlocked = SubsData.SubscriptionIsActive;


           // PlayerPrefs.DeleteAll(); // TEST
           // Game.CurrentCardUnlocked = 10;

            var layout = new LayoutDetector(Game);
            Game.Layout = layout.Layout;

            View = factory.CreateMenuView();
            View.StartLessonButtonClicked += StartLesson;
            View.CollectionButtonClicked += ShowCollection;
            View.SubscribeButtonClicked += BuyLessons;
            View.RestoreButtonClicked += RestorePurchases;
            View.TestButtonClicked += ExpireSubscription; // TEST
            View.ToggleMusicClicked += MusicToggle;
            View.ToggleSoundClicked += SoundToggle;
            View.ChangeLanguageClicked += ChangeLanguage;

            View.SetupBuyDialog(SubsData.FreeTrialAvailable, SubsData.LocalizedPrice, PaymentManager.IsInitialized());
            View.SetupOptionsDialog(Game.MusicEnabled, Game.SoundEnabled, Game.Language);

            View.SetParent(UIRoot);
            View.SetLessonsAvailability(IsLessonsUnlocked);
            View.EnableInput();

            CollectionStore = new CollectionStore(UIRoot);
            CollectionStore.Ended += ActivateMenuView;

            Game.StarsInChest = 18; // TEST ONLY toDo: REMOVE AFTER TESTS!!!

           // Game.Language = SystemLanguage.English; //  TEST ONLY toDo: REMOVE AFTER TESTS!!!
        }

        void BuyLessons()
        {
            PaymentManager.BuySubscription();
           // PaymentManager.FakeBuySubscription();
        }

        void CloseBuyDialogAfterStoreInteraction()
        {
            View.HideBuyDialog()
                .Done(UpdateLessonAvailability);
        }

        void RestorePurchases()
        {
            PaymentManager.Restore();
        }

        void UpdateSubStatus() // TEST
        {
            View.SetupBuyDialog(SubsData.FreeTrialAvailable, SubsData.LocalizedPrice, PaymentManager.IsInitialized());
            UpdateLessonAvailability();
        }

        void UpdateLessonAvailability()
        {
            View.DisableInput();
            View.SetLessonsAvailability(SubsData.SubscriptionIsActive);
            View.EnableInput();
        }

        void ExpireSubscription()
        {
            SubsData.SubscriptionIsActive = false;
            SubsData.ExpirationDate = System.DateTime.MinValue;
            BinarySaveSystem.Save(SubsData);
            UpdateLessonAvailability();
        }

        void CreateLessons()
        {
            LessonList.Add(ELessons.RepairCrown, new RepairLesson(UIRoot, ELessons.RepairCrown));
            LessonList.Add(ELessons.SortColoredGems, new SortColoredGemsLesson(UIRoot, ELessons.SortColoredGems));
            LessonList.Add(ELessons.MakePendant, new MakePendantLesson(UIRoot, ELessons.MakePendant));
            LessonList.Add(ELessons.MakeMedal, new MakeMedalLesson(UIRoot, ELessons.MakeMedal));
            LessonList.Add(ELessons.Fruit01, new Fruit01Lesson(UIRoot, ELessons.Fruit01));
            LessonList.Add(ELessons.Fruit02, new Fruit02Lesson(UIRoot, ELessons.Fruit02));
            LessonList.Add(ELessons.Fruit03, new Fruit03Lesson(UIRoot, ELessons.Fruit03));
            LessonList.Add(ELessons.Fruit04, new Fruit04Lesson(UIRoot, ELessons.Fruit04));
            LessonList.Add(ELessons.Maze01, new Maze01Lesson(UIRoot, ELessons.Maze01));
            LessonList.Add(ELessons.Maze02, new Maze02Lesson(UIRoot, ELessons.Maze02));
            LessonList.Add(ELessons.Memory01, new Memory01Lesson(UIRoot, ELessons.Memory01));
            LessonList.Add(ELessons.Memory02, new Memory02Lesson(UIRoot, ELessons.Memory02));
            LessonList.Add(ELessons.Memory03, new Memory03Lesson(UIRoot, ELessons.Memory03));
            LessonList.Add(ELessons.Candy01, new Candy01Lesson(UIRoot, ELessons.Candy01));
            LessonList.Add(ELessons.Candy02, new Candy02Lesson(UIRoot, ELessons.Candy02));
            LessonList.Add(ELessons.Candy03, new Candy03Lesson(UIRoot, ELessons.Candy03));
            LessonList.Add(ELessons.Candy04, new Candy04Lesson(UIRoot, ELessons.Candy04));
            LessonList.Add(ELessons.Jelly01, new Jelly01Lesson(UIRoot, ELessons.Jelly01));
            LessonList.Add(ELessons.GemOddOneOut, new GemOddOneOutLesson(UIRoot, ELessons.GemOddOneOut));
            LessonList.Add(ELessons.CandyOddOneOut, new CandyOddOneOutLesson(UIRoot, ELessons.CandyOddOneOut));
            LessonList.Add(ELessons.Memory04, new Memory04Lesson(UIRoot, ELessons.Memory04));
            LessonList.Add(ELessons.FruitOddOneOut, new FruitOddOneOutLesson(UIRoot, ELessons.FruitOddOneOut));
            LessonList.Add(ELessons.Jelly02, new Jelly02Lesson(UIRoot, ELessons.Jelly02));
            LessonList.Add(ELessons.Jelly03, new Jelly03Lesson(UIRoot, ELessons.Jelly03));
            LessonList.Add(ELessons.Jelly04, new Jelly04Lesson(UIRoot, ELessons.Jelly04));
            LessonList.Add(ELessons.Jelly05, new Jelly05Lesson(UIRoot, ELessons.Jelly05));
        }

        void Start()
        {
            CreateLessons();
            SubscribeToLessons();
            SoundManager.Play(Sounds.ECommon.Music01);
            UpdateMusic();
        }


        void StartLesson(ELessons lessonId)
        {
            Debug.Log("Start lesson in menu, ID "+lessonId);

            var lesson = LessonList[lessonId];

            View.DisableInput();
            View.Hide()
                .Done(() => lesson.Start());
        }


        private void SubscribeToLessons()
        {
            foreach (var lesson in LessonList.Values)
            {
                lesson.Ended += ActivateMenuView;
                lesson.EndedWithReward += EndLessonWithReward;
            }
        }

        void ShowCollection()
        {
            View.DisableInput();
            View.Hide()
                .Done(() => CollectionStore.Start());
        }

        void EndLessonWithReward()
        {
            CollectionStore.StartWithChest();
        }

        void ActivateMenuView()
        {
            View.Show();
            View.SetupBuyDialog(SubsData.FreeTrialAvailable, SubsData.LocalizedPrice, PaymentManager.IsInitialized());
            View.SetupOptionsDialog(Game.MusicEnabled, Game.SoundEnabled, Game.Language);

            View.SetLessonsAvailability(SubsData.SubscriptionIsActive);
            View.EnableInput();
        }

        void ChangeLanguage(SystemLanguage lang)
        {
            Game.Language = lang;
            View.SetupBuyDialog(SubsData.FreeTrialAvailable, SubsData.LocalizedPrice, PaymentManager.IsInitialized());
            View.SetupOptionsDialog(Game.MusicEnabled, Game.SoundEnabled, Game.Language);
        }

        void MusicToggle()
        {
            Game.MusicEnabled = !Game.MusicEnabled;
            UpdateMusic();
        }

        void SoundToggle()
        {
            Game.SoundEnabled = !Game.SoundEnabled;
            UpdateMusic();
        }

        void UpdateMusic()
        {
            SoundManager.SetMute(!Game.SoundEnabled);
            SoundManager.SetMute(Sounds.ECommon.Music01, !Game.MusicEnabled);

            //if (Game.MusicEnabled == true)
            //{
            //    SoundManager.Play(Sounds.ECommon.Music01);
            //    Debug.Log("Music ON");
            //}
            //else
            //{
            //    SoundManager.Stop(Sounds.ECommon.Music01);
            //    Debug.Log("Music OFF");
            //}
        }

    }
}