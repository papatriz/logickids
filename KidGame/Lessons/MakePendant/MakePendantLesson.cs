using System;
using System.Collections.Generic;
using System.Linq;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.MakePendant
{
    public enum EGemType
    {
        OctagonBig,
        HexagonBig,
        RhombBig,
        RoundedRhombBig,
        TriangleBig,
        HeartBig,
        LeafBig,
        DiamondBig,

        OctagonSmall,
        HexagonSmall,
        RhombSmall,
        RoundedRhombSmall,
        TriangleSmall,
        HeartSmall,
        LeafSmall,
        DiamondSmall
    }

    public class MakePendantLesson : ILesson  
    {
        public event Action Ended;
        public event Action EndedWithReward;

        private IMakePendantView View;

        private IGame Game;
        private IResourceManager Resources;

        private List<EGemType> Hand;
        private List<EGemType> Heap;

        private int RightAnswerCount;
        private int MistakeCount;

        private bool ChestIsFilled;

        private ELessons LessonID;

        public MakePendantLesson(Transform parent, ELessons lesson)
        {
            Game = CompositionRoot.GetGame();
            Resources = CompositionRoot.GetResourceManager();

            LessonID = lesson;

            var factory = CompositionRoot.GetViewFactory();
            View = factory.CreateMakePendantView();
            View.SetParent(parent);

            View.PendantsFilled += NextStep;
            View.MistakeHappened += WrongAnswerHandle;

            View.BackButtonPressed += BreakLesson;

            View.HideInstant();

        }


        private void MakeHand() 
        {
            Hand = new List<EGemType>();

            Heap = Enum.GetValues(typeof(EGemType)).Cast<EGemType>().ToList();

            var totalNum = Heap.Count;
            var smallShift = totalNum / 2;

            var frameBig1 = Random.Range(0, smallShift);
            var frameSmall1 = Random.Range(smallShift, totalNum);
            var frameBig2 = Random.Range(0, smallShift);
            var frameSmall2 = Random.Range(smallShift, totalNum);

            Hand.Add(Heap[frameBig1]);
            Hand.Add(Heap[frameSmall1]);
            Hand.Add(Heap[frameBig2]);
            Hand.Add(Heap[frameSmall2]);

            var el01 = Heap[frameBig1]; // toDo: use it for hand also
            var el02 = Heap[frameBig2];
            var el03 = Heap[frameSmall1];
            var el04 = Heap[frameSmall2];

            Heap.Remove(el01);
            Heap.Remove(el02);
            Heap.Remove(el03);
            Heap.Remove(el04);

            var fake1 = Random.Range(0, Heap.Count);
            var fake2 = Random.Range(0, Heap.Count);

            Hand.Add(Heap[fake1]);
            Hand.Add(Heap[fake2]);

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
                .Then(View.RemovePendants)
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

            View.SetupAndShowPendants(Hand[0], Hand[1], Hand[2], Hand[3]);

            View.ShowAllGems(Hand)
                .Then(ShowTutorialIfNeed)
                .Done(View.UnlockUnplacedGems);
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
            View.RemoveAllGems();
            View.RemovePendants()
                .Done(() =>
                {
                    View.DestroyAllGems();
                    View.Hide();
                    Ended();
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
