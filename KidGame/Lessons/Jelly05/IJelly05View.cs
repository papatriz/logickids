using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;

namespace KidGame.Lessons.Jelly05
{
    public interface IJelly05View : IView
    {
        event Action TaskComplete;
        event Action BackButtonPressed;
        event Action MistakeHappened;

        IPromise ShowAllObjects(List<EJellyType> hand);
        IPromise RemoveAllObjects();
        void DestroyAllObjects();
        void LockAllObjects();
        void UnlockUnplacedObjects();

        IPromise SetupAndShowPlates(EJellyType plate1, EJellyType plate2);
        IPromise RemovePlates();

        // PROGRESS BAR METHODS:

        IPromise CheckpointReached();
        IPromise TranslateToNextCheckpoint();
        void ResetProgress();

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
