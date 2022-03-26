using System;
using System.Collections.Generic;
using Coffee.UIExtensions;
using KidGame.UI;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.FruitOddOneOut
{
    public class FruitOddOneOutView : MonoBehaviour, IFruitOddOneOutView
    {

        public event Action BackButtonPressed;
        public event Action<FruitType> ChoosedObject;

        public Fruit[] Fruits;

        public IconProgressBar ProgressBar;

        public Button BackButton;
        public Tutorial Tutorial;

        public ScreenShading Shading;

        private Fruit RightAnswer;
        private Fruit ChoosedJelly;

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

            WinDialog = (Game.Layout == Layouts.Tablet) ? ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogTablet) : ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogSmartphone);
            WinDialog.transform.SetParent(WinDialogRoot, false);
            WinDialog.gameObject.SetActive(false);

            BackButton.onClick.AddListener(() => BackButtonPressed());

            for (int i = 0; i < Fruits.Length; i++)
            {
                Fruits[i].transform.localScale = 0.001f * Vector3.one;
                Fruits[i].ObjectChoosed += (t) => HandleObjectClick(t);
            }

        }


        private void HandleObjectClick(Fruit target)
        {
            ChoosedJelly = target;

            ChoosedObject(target.GetJellyType());
        }

        public IPromise SetupAndShowObjects(List<FruitType> hand)
        {
            var deferred = new Deferred();
            float wait = 0;

            var posIndexHeap = new List<int> { 0, 1, 2, 3, 4 };

            for (int i = 0; i < hand.Count - 1; i++)
            {
                var posHeapRnd = Random.Range(0, posIndexHeap.Count);
                var posIndex = posIndexHeap[posHeapRnd];
                Fruits[posIndex].Setup(hand[i]);

                posIndexHeap.Remove(posIndex);
            }

            var posHeapRndLast = Random.Range(0, posIndexHeap.Count); // actually it must be 0
            var posIndexLast = posIndexHeap[posHeapRndLast];
            Fruits[posIndexLast].Setup(hand[4]);

            RightAnswer = Fruits[posIndexLast];

            for (int i = 0; i < Fruits.Length; i++)
            {
                wait += 0.15f; // 0.1f
                var j = i; // preventing closure

                deferred = (Deferred)Timers.Wait(wait)
                                           .Then(Fruits[j].Appear);
            }

            return deferred;
        }


        public IPromise RemoveObjects()
        {
            var deferred = new Deferred();
            float wait = 0;

            for (int i = 0; i < Fruits.Length; i++)
            {
                wait += 0.15f; // 0.1f
                var j = i; // preventing closure

                if (Fruits[j].gameObject.activeSelf) // not neccesary to remove already removed object (after right answer animation)
                    deferred = (Deferred)Timers.Wait(wait)
                                           .Then(Fruits[j].DisAppear);
            }

            return deferred;
        }

        public void ChangeBackButtonState(bool state)
        {
            BackButton.gameObject.SetActive(state);
        }


        public void SetObjectsInteractable(bool state)
        {
            for (int i = 0; i < Fruits.Length; i++)
            {
                Fruits[i].SetInteractable(state);
            }
        }

        public IPromise ProccessAnswer(FruitType choosed, bool right)
        {
            //var sound = (right) ? Sounds.ECommon.AM_Short : Sounds.ECommon.UhUh;
            //SoundManager.Play(sound);

            var jelly = ChoosedJelly;
            IPromise action;

            if (right) action = jelly.AnimateRightChoice();
            else action = jelly.AnimateWrongChoice();


            return action;
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

        public IPromise ShowTutorial()
        {
            BackButton.gameObject.SetActive(false);

            return Tutorial.Show(null, RightAnswer.transform, RightAnswer.transform, new Vector3(0f, 0f, 0), new Vector3(0f, 10f, 0), true)
                .Done(() => BackButton.gameObject.SetActive(true));
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