using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;


namespace KidGame.Lessons.Fruit01
{
    public interface IFruit01View : IView
    {
        event Action TaskComplete;
        event Action BackButtonPressed;
        event Action MistakeHappened;

        IPromise ShowAllFruits(List<EFruitType> hand);
        IPromise RemoveAllFruits();
        void DestroyAllFruits();
        void LockAllFruits();
        void UnlockUnplacedFruits();

        IPromise SetupAndShowKid(EFruitType fruitType);
        IPromise RemoveKid();

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