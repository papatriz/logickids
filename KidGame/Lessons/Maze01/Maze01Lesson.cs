using System;
using System.Collections.Generic;
using System.Linq;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;

using Random = UnityEngine.Random;


namespace KidGame.Lessons.Maze01
{
    public class Maze01Lesson : ILesson
    {
        public event Action Ended;
        public event Action EndedWithReward;

        private IMaze01View View;

        private IGame Game;
        private IResourceManager Resources;

        private ELessons LessonID;

        private const int MazesToCompleteLesson = 3;

        private int MazesCompleteCount;
        private List<int> StarsPerMaze;
        private int Star5, Star4, Star3, Star2; // Max steps for getting 5,4,3,2 stars, depends on generated maze
        private int TotalSteps;
        private int CurrentMazeSteps;

        private bool ChestIsFilled;

        public Maze01Lesson(Transform parent, ELessons lesson)
        {
            Game = CompositionRoot.GetGame();
            Resources = CompositionRoot.GetResourceManager();

            LessonID = lesson;

            var factory = CompositionRoot.GetViewFactory();
            View = factory.CreateMaze01View();
            View.SetParent(parent);

            View.TaskComplete += NextStep;
            View.StepsTaken += AddSteps;

            View.BackButtonPressed += BreakLesson;

            View.HideInstant();

        }

        public void Start()
        {
            StarsPerMaze = new List<int>();
            CurrentMazeSteps = 0;

            View.Show();
            SetScene();
        }

        private void SetStepsForStars(int bestway)
        {
            Star5 = bestway + 6;
            Star4 = Star5 + 8;
            Star3 = Star4 + 10;
            Star2 = Star3 + 12;
            TotalSteps = Star5 + 36;
        }

        private void AddSteps(int steps)
        {
            CurrentMazeSteps += steps;
            Debug.Log("Cur steps=" + CurrentMazeSteps);
        }

        private int StarsBySteps(int steps)
        {
            var stars = 1;

            if (steps <= Star5)
            {
                stars = 5;
            }
            else
                if (steps > Star5 && steps <= Star4)
            {
                stars = 4;
            }
            else
                if(steps > Star4 && steps <= Star3)
            {
                stars = 3;
            }
            else
                if(steps > Star3 && steps <= Star2)
            {
                stars = 2;
            }

            return stars;
        }

        private int StarsBySeries()
        {
            var result = (int)Mathf.Round((float)StarsPerMaze.Average());

            return result;
        }

        private void NextStep()
        {
            var stars = StarsBySteps(CurrentMazeSteps);
            StarsPerMaze.Add(stars);

            Debug.Log("Stars =" + stars);
            CurrentMazeSteps = 0;

            MazesCompleteCount++;

            if (MazesCompleteCount == 1) Game.AddLessonStartCount(LessonID); // Add lesson start with at least one right answer given count

            if (MazesCompleteCount < MazesToCompleteLesson)
            {
                View.DeleteMaze()
                    .Done(SetScene);
            }
            else
            {
                View.ShowWinDialog(StarsBySeries())
                    .Done(View.DeleteMazeInstant)
                     .Then(WinRoutine);
            }

        }

        private void SetScene()
        {
            View.DrawMaze()
                .Done(() =>
                {
                    SetStepsForStars(View.GetMinimumSteps());
                    View.SetupProgressBar(TotalSteps, Star5, Star4, Star3, Star2);
                    View.LockPlayerInteraction();
                })
                .Then(ShowTutorialIfNeed)
                .Done(View.UnlockPlayerInteraction);
        }

        private IPromise ShowTutorialIfNeed()
        {
            if ((MazesCompleteCount == 0) && (Game.GetLessonStartCount(LessonID) == 0)) // Show Tutorial only if this is first start of lesson and this is first question
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
            View.DeleteMazeInstant();
            View.Hide();
            Ended();

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
            var amount = StarsBySeries();

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