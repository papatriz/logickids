using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;

namespace KidGame.Lessons.CandyOddOneOut
{
    public interface ICandyOddOneOutView : IView
    {
        event Action<CandyType> ChoosedObject;
        event Action BackButtonPressed;

        IPromise SetupAndShowObjects(List<CandyType> hand);

        IPromise RemoveObjects();

        IPromise ProccessAnswer(CandyType choosed, bool right);
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