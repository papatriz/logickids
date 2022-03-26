using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;

namespace KidGame.Lessons.Fruit02
{
    public interface IFruit02View : IView
    {
        event Action TaskComplete;
        event Action BackButtonPressed;
        event Action MistakeHappened;

        IPromise ShowAllFruits(EFruitType properColor, EFruitType fakeColor);
        IPromise RemoveAllFruits();
        void DestroyAllFruits();
        void LockAllFruits();
        void UnlockUnplacedFruits();

        IPromise SetupAndShowBowl(EFruitType fruitType);
        IPromise RemoveBowl();
        IPromise TossBowl();

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
