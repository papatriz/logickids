using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;


namespace KidGame.Lessons.Jelly02
{
    public interface IJelly02View : IView
    {
        event Action TaskComplete;
        event Action BackButtonPressed;
        event Action MistakeHappened;

        IPromise ShowAllJellys(List<EJellyForm> hand);
        IPromise RemoveAllJellys();
        void DestroyAllJellys();
        void LockAllJellys();
        void UnlockUnplacedJellys();

        IPromise SetupAndShowMouse(EJellyForm JellyType);
        IPromise RemoveMouse();

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