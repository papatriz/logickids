using System;
using System.Collections.Generic;
using Coffee.UIExtensions;
using KidGame.UI;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.Lessons.Memory03
{
    public class Memory03View : MonoBehaviour, IMemory03View
    {

        public event Action BackButtonPressed;
        public event Action<ECandies> ChoosedFruit;

        public Egg Egg01;
        public Egg Egg02;
        public Egg Egg03;

        public CountDown CountDown;
        public QuestBubble Quest;

        public IconProgressBar ProgressBar;

        public Button BackButton;
        public Tutorial Tutorial;

        public ScreenShading Shading;

        public Transform FireworkRoot;
        public Transform WinDialogRoot;

        private WinDialog WinDialog;

        private static ITimers Timers;
        private static ISoundManager SoundManager;
        private static IResourceManager ResourceManager;
        private static IGame Game;

        private UIParticle Firework;


        private void Awake()
        {
            Game = CompositionRoot.GetGame();
            Timers = CompositionRoot.GetTimers();
            SoundManager = CompositionRoot.GetSoundManager();
            ResourceManager = CompositionRoot.GetResourceManager();

            // Game.Layout = Layouts.Smartphone; //todo: FOR TEST ONLY!!

            WinDialog = (Game.Layout == Layouts.Tablet) ? ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogTablet) : ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogSmartphone);
            WinDialog.transform.SetParent(WinDialogRoot, false);
            WinDialog.gameObject.SetActive(false);

            BackButton.onClick.AddListener(() => BackButtonPressed());

            Egg01.EggChoosed += (t) => HandleCardClick(t);
            Egg02.EggChoosed += (t) => HandleCardClick(t);
            Egg03.EggChoosed += (t) => HandleCardClick(t);
        }


        private void HandleCardClick(ECandies target)
        {
            Debug.Log("We choose " + target);

            ChoosedFruit(target);
        }

        public IPromise SetupAndShowContainers(List<ECandies> hand)
        {
            Egg01.SetObject(hand[0]);
            Egg02.SetObject(hand[1]);
            Egg03.SetObject(hand[2]);

            return Deferred.All(Egg01.Appear(), Egg02.Appear(), Egg03.Appear());
        }

        public IPromise CloseContainers()
        {
            return Deferred.All(Egg01.Close(), Egg02.Close(), Egg03.Close());
        }

        public IPromise OpenContainers()
        {
            return Deferred.All(Egg01.Close(false), Egg02.Close(false), Egg03.Close(false));
        }

        public IPromise RemoveContainers()
        {

            return Deferred.All(Egg01.DisAppear(), Egg02.DisAppear(), Egg03.DisAppear());
        }

        public void ChangeBackButtonState(bool state)
        {
            BackButton.gameObject.SetActive(state);
        }

        public IPromise ShowCountDown()
        {
            return CountDown.Appear()
                .Then(CountDown.Begin);
        }

        public IPromise RemoveCountDown()
        {
            return CountDown.DisAppear();
        }

        public IPromise ShowQuestion(ECandies quest)
        {

            return Quest.Show(quest);
        }

        public IPromise HideQuestion()
        {

            return Quest.Hide();
        }

        public void SetContainersInteractable(bool state)
        {
            Egg01.SetInteractable(state);
            Egg02.SetInteractable(state);
            Egg03.SetInteractable(state);
        }

        public IPromise ProccessAnswer(bool right)
        {
            var sound = (right) ? Sounds.ECommon.Woohoo : Sounds.ECommon.UhUh;
            SoundManager.Play(sound);

            return Timers.Wait(1f);
        }

        public IPromise Wait(float t)
        {
            return Timers.Wait(t);
        }

        // MOSTLY COMMON CODE:

        private void StartFirework()
        {
            var values = Enum.GetValues(typeof(EFireworks));

            int index = UnityEngine.Random.Range(0, values.Length);
            var firework = (EFireworks)values.GetValue(index);

            Firework = ResourceManager.CreatePrefabInstance<EFireworks, UIParticle>(firework);
            Firework.transform.SetParent(FireworkRoot, false);
            Firework.gameObject.SetActive(true);

            Firework.Play();
        }

        private void StopFirework()
        {
            if (Firework != null)
            {
                Firework.Stop();
                Firework.gameObject.SetActive(false);
                Destroy(Firework.gameObject);
            }
        }

        public IPromise ShowTutorial(ECandies right)
        {
            BackButton.gameObject.SetActive(false);

            var handTarget = GetProperContainer(right);

            return Tutorial.Show(null, handTarget, handTarget, new Vector3(0f, 80f, 0), new Vector3(0f, 40f, 0), true)
                .Done(() => BackButton.gameObject.SetActive(true));
        }

        private Transform GetProperContainer(ECandies rightAnswer)
        {
            Transform result = null;

            if (Egg01.GetObject() == rightAnswer) result = Egg01.transform;
            if (Egg02.GetObject() == rightAnswer) result = Egg02.transform;
            if (Egg03.GetObject() == rightAnswer) result = Egg03.transform;

            return result;
        }

        public IPromise ShowWinDialog(int starCount)
        {

            Shading.Shade(true, 1, 0.5f);
            StartFirework();

            WinDialog.ResetToInitial();
            SoundManager.Play(Sounds.ECommon.TaDam);

            return WinDialog.Show()
                        .Then(() => WinDialog.SetStars(starCount));
        }

        public IPromise WaitForWinDialogClose()
        {
            return WinDialog.WaitForClose();
        }

        public IPromise HideWinDialog()
        {
            return WinDialog.Hide()
                .Always(StopFirework);
        }

        public void CloseAndAnimateChest()
        {
            WinDialog.CloseAndAnimateChest();
        }

        public IPromise PrepareForTransit()
        {
            WinDialog.PrepateForTransit();
            StopFirework();

            return Shading.Shade(true, 0.5f)
                .Done(() => WinDialog.gameObject.SetActive(false));
        }

        public IPromise TranslateToNextCheckpoint()
        {
            return ProgressBar.TranslateToNext();
        }

        public void ResetProgress()
        {
            ProgressBar.Reset();
        }

        public IPromise CheckpointReached()
        {
            return ProgressBar.ActivateNext();
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

            return Shading.Shade(false, 1.5f);
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

        public void SetParent(Transform parent)
        {
            transform.SetParent(parent, false);
        }


    }
}