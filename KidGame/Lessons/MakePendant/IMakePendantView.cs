using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;


namespace KidGame.Lessons.MakePendant
{
    public interface IMakePendantView : IView
    {
        event Action PendantsFilled;
        event Action BackButtonPressed;
        event Action MistakeHappened;

        IPromise ShowAllGems(List<EGemType> hand);
        IPromise RemoveAllGems();
        void DestroyAllGems();
        void LockAllGems();
        void UnlockUnplacedGems();

        IPromise SetupAndShowPendants(EGemType bigFrame1, EGemType smallFrame1, EGemType bigFrame2, EGemType smallFrame2);
        IPromise RemovePendants();

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

