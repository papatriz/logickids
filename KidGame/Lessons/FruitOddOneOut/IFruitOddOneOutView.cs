using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;

namespace KidGame.Lessons.FruitOddOneOut
{
    public interface IFruitOddOneOutView : IView
    {
        event Action<FruitType> ChoosedObject;
        event Action BackButtonPressed;

        IPromise SetupAndShowObjects(List<FruitType> hand);

        IPromise RemoveObjects();

        IPromise ProccessAnswer(FruitType choosed, bool right);
        void SetObjectsInteractable(bool state);
        void ChangeBackButtonState(bool state);

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