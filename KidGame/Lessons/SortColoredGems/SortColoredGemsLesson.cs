using System;
using System.Collections.Generic;
using System.Linq;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.SortColoredGems
{
    public enum EGemType
    {
        Red,
        Green,
        Blue,
        Yellow
    }

    public class SortColoredGemsLesson : ILesson
    {
        public event Action Ended;
        public event Action EndedWithReward;

        private ISortColoredGemsView View;

        private IGame Game;
        private IResourceManager Resources;

        private List<EGemType> Hand;
        private List<EGemType> Heap;

        private int RightAnswerCount;
        private int MistakeCount;

        private bool ChestIsFilled;

        private ELessons LessonID;

        public SortColoredGemsLesson(Transform parent, ELessons lesson)
        {
            Game = CompositionRoot.GetGame();
            Resources = CompositionRoot.GetResourceManager();

            LessonID = lesson;

            var factory = CompositionRoot.GetViewFactory();
            View = factory.CreateSortColoredGemsView();
            View.SetParent(parent);

            View.BoxIsFilled += NextStep;
            View.MistakeHappened += WrongAnswerHandle;

            View.BackButtonPressed += BreakLesson;

            View.HideInstant();

        }


        private EGemType GetFakeColor(EGemType rightColor)
        {           
            Heap = Enum.GetValues(typeof(EGemType)).Cast<EGemType>().ToList();

            Heap.Remove(rightColor);
            var fakeTypeIndex = Random.Range(0, Heap.Count);

            return Heap[fakeTypeIndex];  
        }



        public void Start()
        {
            RightAnswerCount = 0;
            View.Show();
            View.ResetProgress();
            SetScene();
        }

        private void NextStep()
        {
            View.LockAllGems();

            View.CheckpointReached()
                .Done(() => View.RemoveAllGems())
                .Then(View.RemoveBox)
                .Done(() =>
                {
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

        private void SetScene()
        {
            var typesCount = Enum.GetValues(typeof(EGemType)).Length;

            var proper = (EGemType)Random.Range(0, typesCount);
            var fake = GetFakeColor(proper);

            View.SetupAndShowBox(proper);

            View.ShowAllGems(proper, fake)
                .Then(ShowTutorialIfNeed)
                .Done(View.UnlockUnsortedGems);
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
            View.RemoveBox();
            View.RemoveAllGems()
                .Done(() =>
                {
                    // View.ResetProgress();
                    View.Hide();
                    Ended();
                });
        }

        private void WrongAnswerHandle()
        {
            MistakeCount++;
            Debug.Log("Mistake count: " + MistakeCount);
        }
    }
}