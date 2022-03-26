using System;
using System.Collections.Generic;
using System.Linq;
using Coffee.UIExtensions;
using KidGame.UI;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Memory02
{
    public enum EGems
    {
        Heart,
        Cone,
        Leaf,
        Hexagon,
        Romb,
        Diamond
    }

    public class Memory02Lesson : ILesson
    {
        public event Action Ended;
        public event Action EndedWithReward;

        private IMemory02View View;

        private IGame Game;
        private IResourceManager Resources;

        private EGems Question;

        private int RightAnswerCount;
        private int MistakeCount;

        private bool ChestIsFilled;

        private ELessons LessonID;

        public Memory02Lesson(Transform parent, ELessons lesson)
        {
            Game = CompositionRoot.GetGame();
            Resources = CompositionRoot.GetResourceManager();

            LessonID = lesson;

            var factory = CompositionRoot.GetViewFactory();
            View = factory.CreateMemory02View();
            View.SetParent(parent);

            View.ChoosedFruit += (fruit) => HandleChoiceMade(fruit);

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

        private List<EGems> GetHand()
        {
            var hand = new List<EGems>();

            var heap = Enum.GetValues(typeof(EGems)).Cast<EGems>().ToList();

            for (int i = 0; i < 3; i++)
            {
                var fIndex = Random.Range(0, heap.Count);
                var fruit = heap[fIndex];
                hand.Add(fruit);
                heap.Remove(fruit);
            }

            return hand;
        }

        private void HandleChoiceMade(EGems fruit)
        {
            View.SetContainersInteractable(false);
            View.ChangeBackButtonState(false);

            if (fruit == Question)
            {
                ProcessRightAnswer();
            }
            else
            {
                ProcessWrongAnswer();
            }
        }


        private void ProcessWrongAnswer()
        {
            MistakeCount++;

            //toDo: make some cool animation in addition to sound
            View.ProccessAnswer(false)
                .Then(() => Deferred.All(View.HideQuestion(), View.RemoveContainers()))
                .Done(SetScene);
        }


        private void ProcessRightAnswer()
        {
            //toDo: make some cool animation in addition to sound
            View.ProccessAnswer(true)
                .Then(View.CheckpointReached)
                .Done(() => View.HideQuestion())
                .Then(View.RemoveContainers)
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


        private void SetScene()
        {
            var hand = GetHand();

            var questIndex = Random.Range(0, hand.Count);
            Question = hand[questIndex];

            View.SetContainersInteractable(false);
            View.ChangeBackButtonState(false);

            View.SetupAndShowContainers(hand)
                .Then(View.ShowCountDown)
                .Then(View.RemoveCountDown)
                .Then(View.CloseContainers)
                .Then(() => View.Wait(1f))
                .Then(() => View.ShowQuestion(Question))
                .Then(ShowTutorialIfNeed)
                .Done(() => View.SetContainersInteractable(true))
                .Done(() => View.ChangeBackButtonState(true));

        }

        private IPromise ShowTutorialIfNeed()
        {
            if ((RightAnswerCount == 0) && (Game.GetLessonStartCount(LessonID) == 0)) // Show Tutorial only if this is first start of lesson and this is first question
            {
                return View.ShowTutorial(Question);
            }
            else
            {
                return new Deferred().Resolve();
            }

        }

        private void BreakLesson()
        {
            View.ChangeBackButtonState(false);

            var removeAll = Deferred.All(View.RemoveContainers(), View.HideQuestion());

            removeAll
                .Done(() =>
                {
                    View.Hide();
                    Ended();
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
    }
}