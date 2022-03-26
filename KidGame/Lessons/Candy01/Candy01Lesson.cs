using System;
using System.Collections.Generic;
using System.Linq;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Candy01
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

    public enum ECandyTexture
    {
        Flat,
        Strips,
        Rounds,
        Stars
    }

    public class Candy01Lesson : ILesson
    {
        public event Action Ended;
        public event Action EndedWithReward;

        private ICandy01View View;

        private IGame Game;
        private IResourceManager Resources;

        // private List<ECandies> Heap;
        private ECandies Quest;

        private int RightAnswerCount;
        private int MistakeCount;

        private bool ChestIsFilled;

        private ELessons LessonID;

        public Candy01Lesson(Transform parent, ELessons lesson)
        {
            Game = CompositionRoot.GetGame();
            Resources = CompositionRoot.GetResourceManager();

            LessonID = lesson;

            var factory = CompositionRoot.GetViewFactory();
            View = factory.CreateCandy01View();
            View.SetParent(parent);

            View.TaskComplete += NextStep;
            View.MistakeHappened += WrongAnswerHandle;
            View.BackButtonPressed += BreakLesson;

            View.HideInstant();

        }

        private ECandies GetCandyByFormAndTexture(ECandyForm type, ECandyTexture texture)
        {
            var resultInt = (int)type * 4 + (int)texture;

            return (ECandies)resultInt;
        }

        private List<ECandies> GetHand() // toDo: 
        {
            var hand = new List<ECandies>();

            var heapTextures = Enum.GetValues(typeof(ECandyTexture)).Cast<ECandyTexture>().ToList();

            var questTexture = (ECandyTexture)Random.Range(0, heapTextures.Count);
            var questType = (ECandyForm)Random.Range(0, 5);

            Quest = GetCandyByFormAndTexture(questType, questTexture);

            var proper1Type = (ECandyForm)Random.Range(0, 5);
            var proper2Type = (ECandyForm)Random.Range(0, 5);

            var proper1 = GetCandyByFormAndTexture(proper1Type, questTexture);
            var proper2 = GetCandyByFormAndTexture(proper2Type, questTexture);

            hand.Add(proper1);
            hand.Add(proper2);

            heapTextures.Remove(questTexture);

            for (int i = 0; i < 4; i++)
            {
                var fakeType = (ECandyForm)Random.Range(0, 5);

                var fakeTextureIndex = Random.Range(0, heapTextures.Count);
                var fakeTexture = heapTextures[fakeTextureIndex];

                var fake = GetCandyByFormAndTexture(fakeType, fakeTexture);

                hand.Add(fake);
            }

            return hand;
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
                .Then(View.TossPlate)
                .Then(View.RemovePlate)
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

            View.SetupAndShowPlate(Quest);

            View.ShowAllObjects(hand)
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
            View.RemovePlate();
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
