using System;
using System.Collections.Generic;
using System.Linq;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.MakeMedal
{
    public enum EGemType
    {
        ConeRed,
        HexagonRed,
        RhombRed,
        TriangleRed,
        HeartRed,

        ConeGreen,
        HexagonGreen,
        RhombGreen,
        TriangleGreen,
        HeartGreen,

        ConeBlue,
        HexagonBlue,
        RhombBlue,
        TriangleBlue,
        HeartBlue,

        ConeMagenta,
        HexagonMagenta,
        RhombMagenta,
        TriangleMagenta,
        HeartMagenta

    }

    public class MakeMedalLesson : ILesson
    {
        public event Action Ended;
        public event Action EndedWithReward;

        private IMakeMedalView View;

        private IGame Game;
        private IResourceManager Resources;

        private List<EGemType> Hand;
        private List<EGemType> Heap;

        private int RightAnswerCount;
        private int MistakeCount;

        private bool ChestIsFilled;

        private ELessons LessonID;

        public MakeMedalLesson(Transform parent, ELessons lesson)
        {
            Game = CompositionRoot.GetGame();
            Resources = CompositionRoot.GetResourceManager();

            LessonID = lesson;

            var factory = CompositionRoot.GetViewFactory();
            View = factory.CreateMakeMedalView();
            View.SetParent(parent);

            View.MedalFilled += NextStep;
            View.MistakeHappened += WrongAnswerHandle;

            View.BackButtonPressed += BreakLesson;

            View.HideInstant();

        }


        private void MakeHand() // toDo: генерим "руку" (4 gems) тут - так, чтобы в ней был гем нужного типа, потом передаем во вью
        {
            Hand = new List<EGemType>();

            Heap = Enum.GetValues(typeof(EGemType)).Cast<EGemType>().ToList();

            for (int i = 0; i < 6; i++)
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
            View.Show();
            //  .Done(ResetAndSetScene);
            ResetAndSetScene();
           
        }

        private void ResetAndSetScene()
        {
            View.ResetProgress();
            SetScene();
        }

        private void NextStep()
        {
            View.LockAllGems();

            View.CheckpointReached()
                 .Done(() => View.RemoveAllGems())
                 .Then(View.RemoveMedal)
                 .Done(() =>
                 {
                     View.DestroyAllGems();

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
            MakeHand();

            View.SetupAndShowMedal(Hand[0]);

            View.ShowAllGems(Hand)
                .Then(ShowTutorialIfNeed)
                .Done(View.UnlockUnplacedGems);
        }

        private void BreakLesson()
        {
            View.RemoveMedal();
            View.RemoveAllGems()
                .Done(() =>
                {
                    View.Hide();
                    Ended();
                });
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

        private void WrongAnswerHandle()
        {
            MistakeCount++;
            Debug.Log("Mistake count: " + MistakeCount);
        }
    }
}
