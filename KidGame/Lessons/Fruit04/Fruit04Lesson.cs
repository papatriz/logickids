using System;
using System.Collections.Generic;
using System.Linq;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;

using Random = UnityEngine.Random;

namespace KidGame.Lessons.Fruit04
{
    public enum EFruitType
    {
        GoldBig,
        GreenBig,
        RedBig,
        YellowBig,

        GoldSmall,
        GreenSmall,
        RedSmall,
        YellowSmall
    }

    public class Fruit04Lesson : ILesson
    {
        public event Action Ended;
        public event Action EndedWithReward;

        private IFruit04View View;

        private IGame Game;
        private IResourceManager Resources;

        private List<int> Heap;
        private EFruitType BowlBigType;
        private EFruitType BowlSmallType;
        private EFruitType FakeType;

        private int RightAnswerCount;
        private int MistakeCount;

        private bool ChestIsFilled;

        private ELessons LessonID;

        public Fruit04Lesson(Transform parent, ELessons lesson)
        {
            Game = CompositionRoot.GetGame();
            Resources = CompositionRoot.GetResourceManager();

            LessonID = lesson;

            var factory = CompositionRoot.GetViewFactory();
            View = factory.CreateFruit04View();
            View.SetParent(parent);

            View.TaskComplete += NextStep;
            View.MistakeHappened += WrongAnswerHandle;

            View.BackButtonPressed += BreakLesson;

            View.HideInstant();

        }

        public void Start()
        {
            RightAnswerCount = 0;
            View.Show();
            View.ResetProgress();
            SetScene();
        }

        private void GetBowlsColors()
        {
            Heap = new List<int> { 0, 1, 2, 3 };

            var bigColorIndex = Random.Range(0, Heap.Count);
            var bigColorIntValue = Heap[bigColorIndex];

           // Heap.Remove(bigColorIntValue);

            var smallColorIndex = Random.Range(0, Heap.Count);
            var smallColorIntValue = Heap[smallColorIndex];

            Heap.Remove(smallColorIntValue);
            Heap.Remove(bigColorIntValue);

            var fakeColorIndex = Random.Range(0, Heap.Count);
            var fakeColorIntValue = Heap[fakeColorIndex];

            BowlBigType = (EFruitType)bigColorIntValue;
            BowlSmallType = (EFruitType)(smallColorIntValue + 4);
            FakeType = (EFruitType)fakeColorIntValue;

        }

        private void NextStep()
        {
            View.LockAllFruits();

            View.CheckpointReached()
                .Done(() => View.RemoveAllFruits())
                //.Then(View.CloseBoxes)
                .Then(View.RemoveBowls)
                .Done(() =>
                {
                    View.DestroyAllFruits();

                    RightAnswerCount++;
                    if (RightAnswerCount == 1) Game.AddLessonStartCount(LessonID); // Add lesson start with at least one right answer given count

                    if (RightAnswerCount < 7)
                    {
                        View.TranslateToNextCheckpoint();
                        SetScene();
                    }
                    else
                    {
                        View.ShowWinDialog(Game.StarsForGivenMistakes(MistakeCount))
                             .Then(WinRoutine);
                    }
                });
        }



        private void SetScene()
        {
            GetBowlsColors();

            View.SetupAndShowBowls(BowlBigType, BowlSmallType);

            View.ShowAllFruits(FakeType)
                .Then(ShowTutorialIfNeed)
                .Done(View.UnlockUnplacedFruits);
        }

        private IPromise ShowTutorialIfNeed()
        {
            if ((RightAnswerCount == 0) && (Game.GetLessonStartCount(LessonID) == 0)) // Show Tutorial only if this is first start of lesson and this is first question
            {
                return View.ShowTutorial();
            }
            else
            {
                return new Deferred().Resolve();
            }

        }

        private void BreakLesson()
        {
            View.RemoveBowls();
            View.RemoveAllFruits()
                .Done(() =>
                {
                    View.DestroyAllFruits();
                    View.Hide();
                    Ended();
                });
        }

        private void WrongAnswerHandle()
        {
            MistakeCount++;
            Debug.Log("Mistake count: " + MistakeCount);
        }

        // ----------------- WIN ROUTINE METHODS ----------------
        private IPromise WinRoutine()
        {
            IPromise routine;

            AddEarnedStars();

            if (ChestIsFilled)
            {
                View.CloseAndAnimateChest();

                routine = View.WaitForWinDialogClose()
                    .Then(View.PrepareForTransit)
                    .Done(EndedWithReward)
                    .Done(View.HideInstant);
            }
            else
            {
                routine = View.WaitForWinDialogClose()
                        .Then(View.HideWinDialog)
                        .Then(View.Hide)
                        .Done(Ended);
            }

            return routine;
        }

        private void AddEarnedStars()
        {
            var amount = Game.StarsForGivenMistakes(MistakeCount);

            if ((Game.StarsInChest + amount) >= Game.StarsToNextUnlock)
            {
                ChestIsFilled = true;

                var stepsToUnlock = Game.StarsToNextUnlock - Game.StarsInChest;
                var overAmount = amount - stepsToUnlock;
                Game.StarsInChest = overAmount;
            }
            else
            {
                ChestIsFilled = false;
                Game.StarsInChest += amount;
            }
        }
        // ------------- END OF WIN ROUTINE METHODS ----------------
    }
}
