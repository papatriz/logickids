using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;

namespace KidGame.Lessons.Fruit03
{
    public interface IFruit03View : IView
    {
        event Action TaskComplete;
        event Action BackButtonPressed;
        event Action MistakeHappened;

        IPromise ShowAllFruits();
        IPromise RemoveAllFruits();
        void DestroyAllFruits();
        void LockAllFruits();
        void UnlockUnplacedFruits();

        IPromise SetupAndShowBoxes();
        IPromise RemoveBoxes();
        IPromise CloseBoxes();

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
