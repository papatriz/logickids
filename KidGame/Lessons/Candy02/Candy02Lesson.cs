using System;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Candy02
{
    public enum ECandies
    {
        Candy01Flat,
        Candy01Strips,
        Candy01Rounds,
        Candy01Stars,

        Candy02Flat,
        Candy02Strips,
        Candy02Rounds,
        Candy02Stars,

        Candy03Flat,
        Candy03Strips,
        Candy03Rounds,
        Candy03Stars,

        Candy04Flat,
        Candy04Strips,
        Candy04Rounds,
        Candy04Stars,

        Candy05Flat,
        Candy05Strips,
        Candy05Rounds,
        Candy05Stars

    }

    public enum ECandyForm
    {
        Round,
        Square,
        Chupa,
        Rectangle,
        Truffel
    }

    public enum ECandySize
    {
        Big,
        Small
    }

    public class Candy02Lesson : ILesson
    {
        public event Action Ended = () => { };
        public event Action EndedWithReward;

        private ICandy02View View;

        private IGame Game;
        private IResourceManager Resources;

        private int RightAnswerCount;
        private int MistakeCount;

        private bool ChestIsFilled;

        private ELessons LessonID;

        public Candy02Lesson(Transform parent, ELessons lesson)
        {
            Game = CompositionRoot.GetGame();
            Resources = CompositionRoot.GetResourceManager();

            LessonID = lesson;

            var factory = CompositionRoot.GetViewFactory();
            View = factory.CreateCandy02View();
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

        private void NextStep()
        {
            View.LockAllObjects();

            View.CheckpointReached()
                .Done(() => View.RemoveAllObjects())
                .Then(View.RemovePlates)
                .Done(() =>
                {
                    View.DestroyAllObjects();

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

            View.SetupAndShowPlates();

            View.ShowAllObjects()
                .Then(ShowTutorialIfNeed)
                .Done(View.UnlockUnplacedObjects);
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
            View.RemovePlates();
            View.RemoveAllObjects()
                .Done(() =>
                {
                    View.DestroyAllObjects();
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