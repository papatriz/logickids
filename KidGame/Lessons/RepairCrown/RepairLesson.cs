using System;
using System.Collections.Generic;
using System.Linq;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.RepairCrown
{
    public enum EGemType  
    {
        Cone,
        Octagon,
        Hexagon,
        Rhomb,
        RoundedRhomb,
        Triangle,
        Heart,
        Leaf,
        Diamond
    }

    public class RepairLesson : ILesson  
    {
        public event Action Ended;
        public event Action EndedWithReward;

        private IRepairCrownView View;

        private IGame Game;
        private IResourceManager Resources;

        private List<EGemType> Hand;
        private List<EGemType> Heap;

        private int RightAnswerCount;
        private int MistakeCount;

        private bool ChestIsFilled;

        private ELessons LessonID;

        public RepairLesson(Transform parent, ELessons lesson)
        {
            Game = CompositionRoot.GetGame();
            Resources = CompositionRoot.GetResourceManager();

            LessonID = lesson;

            var factory = CompositionRoot.GetViewFactory();
            View = factory.CreateRepairCrownView();
            View.SetParent(parent);

            View.GemInPlace += NextStep;
            View.MistakeHappened += WrongAnswerHandle;

            View.BackButtonPressed += BreakLesson;

            View.HideInstant();

        }


        private void MakeHand(EGemType rightGem) // toDo: генерим "руку" (4 gems) тут - так, чтобы в ней был гем нужного типа, потом передаем во вью
        {
            Hand = new List<EGemType>();

            Heap = Enum.GetValues(typeof(EGemType)).Cast<EGemType>().ToList();

            Hand.Add(rightGem);
            Heap.Remove(rightGem);

            for (int i=0; i<3; i++)
            {
                var gemIndex = Random.Range(0, Heap.Count);
                var gem = Heap[gemIndex];
                Hand.Add(gem);
                Heap.Remove(gem);
            }
        }



        public void Start()
        {
            RightAnswerCount = 0;
            MistakeCount = 0;
            View.Show();
            View.ResetProgress();
            SetScene();
        }

        private void NextStep(int placedGemIndex, EGemType gemType)
        {
            View.LockAllGems();

            View.InsertGemToCrown(placedGemIndex)
                .Then(View.CheckpointReached)
                .Done(() => View.RemoveAllGems())
                .Then(View.RemoveCrown)
                .Done(() =>
                {
                    RightAnswerCount++;

                    if (RightAnswerCount == 1) Game.AddLessonStartCount(LessonID); // Add lesson start with at least one right answer given count

                    if (RightAnswerCount < 7)  // <7 for release, 2 only for test
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
            var properType = (EGemType)Random.Range(0, View.GetTargetTypeCount());
            View.SetupCrown(properType);
            MakeHand(properType);

            View.ShowAllGems(Hand)
                .Then(ShowTutorialIfNeed)
                .Done(View.SetTargetTypeForGems)
                .Done(View.UnlockAllGems);
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
            View.RemoveCrown();
            View.RemoveAllGems()
                .Done(() =>
                {
                    View.Hide()
                        .Done(Ended);
                });
        }

        private void WrongAnswerHandle()
        {
            MistakeCount++;
            Debug.Log("Mistake count: " + MistakeCount);
        }

        // DEBUG TOOLS

        private void _debugShowListValue(List<EGemType> list)
        {
            var s = "";
            foreach (EGemType g in list)
            {
                s += g.ToString() + " : ";
            }

            Debug.Log(s);
        }

    }
}
