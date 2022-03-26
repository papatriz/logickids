using System;
using KidGame.UI;
using Orbox.Async;

namespace KidGame.Lessons.Maze02
{
    public interface IMaze02View : IView
    {
        event Action TaskComplete;
        event Action BackButtonPressed;
        event Action<int> StepsTaken;

        int GetMinimumSteps();
        IPromise DrawMaze();
        IPromise DeleteMaze();
        void DeleteMazeInstant();
        void LockPlayerInteraction();
        void UnlockPlayerInteraction();

        // PROGRESS BAR METHODS:

        void SetupProgressBar(int maxSteps, int step5star, int step4star, int step3star, int step2star);

        // TUTORIAL
        IPromise ShowTutorial();

        // WIN ROUTINE
        IPromise ShowWinDialog(int starCount);
        IPromise WaitForWinDialogClose();
        IPromise HideWinDialog();
        IPromise PrepareForTransit();
        void CloseAndAnimateChest();
    }
}
