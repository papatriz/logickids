using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;

namespace KidGame.Lessons.Fruit04
{
    public interface IFruit04View : IView
    {
        event Action TaskComplete;
        event Action BackButtonPressed;
        event Action MistakeHappened;

        IPromise ShowAllFruits(EFruitType fake);
        IPromise RemoveAllFruits();
        void DestroyAllFruits();
        void LockAllFruits();
        void UnlockUnplacedFruits();

        IPromise SetupAndShowBowls(EFruitType big, EFruitType small);
        IPromise RemoveBowls();

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
