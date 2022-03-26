using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;

namespace KidGame.Lessons.Candy04
{
    public interface ICandy04View : IView
    {
        event Action TaskComplete;
        event Action BackButtonPressed;
        event Action MistakeHappened;

        IPromise ShowAllObjects(List<ECandyFormAndSize> hand);
        IPromise RemoveAllObjects();
        void DestroyAllObjects();
        void LockAllObjects();
        void UnlockUnplacedObjects();

        IPromise SetupAndShowCharacters(ECandyFormAndSize candySmall, ECandyFormAndSize candyBig);
        IPromise RemoveCharacters();

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
