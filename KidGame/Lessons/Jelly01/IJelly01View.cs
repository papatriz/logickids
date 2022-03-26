using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;

namespace KidGame.Lessons.Jelly01
{
    public interface IJelly01View : IView
    {
        event Action<JellyType> ChoosedObject;
        event Action BackButtonPressed;

        IPromise SetupAndShowObjects(List<JellyType> hand);

        IPromise RemoveObjects();

        IPromise ProccessAnswer(JellyType choosed, bool right);
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