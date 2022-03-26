using System;
using System.Collections.Generic;
using System.Linq;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Jelly03
{
    public enum EColors
    {
         Blue,
         Green,
         Red,
         Yellow
    }


    public class Jelly03Lesson : ILesson
    {
        public event Action Ended = () => { };
        public event Action EndedWithReward;

        private IJelly03View View;

        private IGame Game;
        private IResourceManager Resources;

        private int RightAnswerCount;
        private int MistakeCount;

        private bool ChestIsFilled;

        private ELessons LessonID;

        public Jelly03Lesson(Transform parent, ELessons lesson)
        {
            Game = CompositionRoot.GetGame();
            Resources = CompositionRoot.GetResourceManager();

            LessonID = lesson;

            var factory = CompositionRoot.GetViewFactory();
            View = factory.CreateJelly03View();
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

        private List<EColors> GetHand()
        {
            var hand = new List<EColors>();
            var heap = Enum.GetValues(typeof(EColors)).Cast<EColors>().ToList();

            int firstQuestIndex = Random.Range(0, heap.Count);

            var firstQuest = heap[firstQuestIndex];
            hand.Add(firstQuest);
            heap.Remove(firstQuest);

            int secondQuestIndex = Random.Range(0, heap.Count);

            var secondQuest = heap[secondQuestIndex];
            hand.Add(secondQuest);
            heap.Remove(secondQuest);

           // hand.AddRange(heap);

            return hand;
        }

        private void NextStep()
        {
            View.LockAllObjects();

            View.CheckpointReached()
                .Done(() => View.RemoveAllObjects())
                .Then(View.RemoveEggs)
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
            var hand = GetHand();
            View.SetupAndShowEggs(hand);

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
            View.RemoveEggs();
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