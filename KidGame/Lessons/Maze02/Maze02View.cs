using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using KidGame.Maze;
using KidGame.UI;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Maze02
{
    public class Maze02View : MonoBehaviour, IMaze02View
    {
        public event Action TaskComplete;
        public event Action BackButtonPressed;
        public event Action<int> StepsTaken;

        public MazeDrawer MazeDrawer;
        public MazeProgressBar MazeProgressBar;

        public Button BackButton;
        //  public Tutorial Tutorial; // toDO: Special tutorial needed.

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

            WinDialog = (Game.Layout == Layouts.Tablet) ? ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogTablet) : ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogSmartphone);
            WinDialog.transform.SetParent(WinDialogRoot, false);
            WinDialog.gameObject.SetActive(false);

            BackButton.onClick.AddListener(() => BackButtonPressed());

            MazeDrawer.AllItemsCollected += HandleTaskComplete;
            MazeDrawer.StepsTaken += HandleStepsTaken;

        }

        private void HandleStepsTaken(int s)
        {
            StepsTaken(s);
        }

        private void HandleTaskComplete()
        {
            TaskComplete();
        }

        public int GetMinimumSteps()
        {
            return MazeDrawer.GetMinimumSteps();
        }

        public IPromise DrawMaze()
        {
            return MazeDrawer.Generate()
                .Done(() => Debug.Log("MAZE GENERATION DONE"))
                .Done(MazeDrawer.SetupPlayer)
                .Done(() => Shading.Shade(false, 1f));
        }

        public IPromise DeleteMaze()
        {
            //  MazeDrawer.EraseMaze();
            return Shading.Shade(true, 1f)
                .Done(MazeDrawer.EraseMaze);

            // .Then(() => Shading.Shade(false, 1f));

        }

        public void DeleteMazeInstant()
        {
            MazeDrawer.EraseMaze();
        }

        public void LockPlayerInteraction()
        {
            MazeDrawer.LockPlayer(true);
        }

        public void UnlockPlayerInteraction()
        {
            MazeDrawer.LockPlayer(false);
        }

        public void SetupProgressBar(int maxSteps, int step5star, int step4star, int step3star, int step2star)
        {
            MazeProgressBar.Setup(maxSteps, step5star, step4star, step3star, step2star);
        }


        public IPromise ShowTutorial() // toDo: MAKE SPECIAL TUTORIAL
        {
            BackButton.gameObject.SetActive(false);


            return new Deferred().Resolve()
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

        private void StartFirework()
        {
            var values = Enum.GetValues(typeof(EFireworks));

            int index = Random.Range(0, values.Length);
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


    }
}