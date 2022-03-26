using System;
using System.Collections.Generic;
using System.Linq;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Candy04
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

    public enum ECandyFormAndSize
    {
        RoundBig,
        SquareBig,
        ChupaBig,
        RectangleBig,
        TruffelBig,
        RoundSmall,
        SquareSmall,
        ChupaSmall,
        RectangleSmall,
        TruffelSmall
    }


    public class Candy04Lesson : ILesson
    {
        public event Action Ended = () => { };
        public event Action EndedWithReward;

        private ICandy04View View;

        private IGame Game;
        private IResourceManager Resources;

        private int RightAnswerCount;
        private int MistakeCount;

        private bool ChestIsFilled;

        private ELessons LessonID;

        public Candy04Lesson(Transform parent, ELessons lesson)
        {
            Game = CompositionRoot.GetGame();
            Resources = CompositionRoot.GetResourceManager();

            LessonID = lesson;

            var factory = CompositionRoot.GetViewFactory();
            View = factory.CreateCandy04View();
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


        private List<ECandyFormAndSize> GetHand()
        {
            var hand = new List<ECandyFormAndSize>();
            var heap = Enum.GetValues(typeof(ECandyFormAndSize)).Cast<ECandyFormAndSize>().ToList();

            int bigQuestIndex = Random.Range(0, 5);
            int smallQuestIndex = Random.Range(5, 10);

            var bigQuest = heap[bigQuestIndex];
            var smallQuest = heap[smallQuestIndex];

            hand.Add(smallQuest);
            hand.Add(bigQuest);

            heap.Remove(bigQuest);
            heap.Remove(smallQuest);

            // generate rest of hand with random fakes
            for(int i=0; i<4; i++)
            {
                int fakeIndex = Random.Range(0, heap.Count);
                var fake = heap[fakeIndex];
                hand.Add(fake);
                heap.Remove(fake);
            }

            return hand;
        }


        private void SetScene()
        {
            var hand = GetHand();

            View.SetupAndShowCharacters(hand[0], hand[1]);

            View.ShowAllObjects(hand)
                .Then(ShowTutorialIfNeed);
               // .Done(View.UnlockUnplacedObjects);
        }

        private void NextStep()
        {
            View.LockAllObjects();

            View.CheckpointReached()
                .Done(() => View.RemoveAllObjects())
                .Then(View.RemoveCharacters)
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
            View.RemoveCharacters();
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