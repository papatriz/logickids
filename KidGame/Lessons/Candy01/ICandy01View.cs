using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;

namespace KidGame.Lessons.Candy01
{
    public interface ICandy01View : IView
    {
        event Action TaskComplete;
        event Action BackButtonPressed;
        event Action MistakeHappened;

        IPromise ShowAllObjects(List<ECandies> hand);
        IPromise RemoveAllObjects();
        void DestroyAllObjects();
        void LockAllObjects();
        void UnlockUnplacedObjects();

        IPromise SetupAndShowPlate(ECandies candyType);
        IPromise RemovePlate();
        IPromise TossPlate();

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
