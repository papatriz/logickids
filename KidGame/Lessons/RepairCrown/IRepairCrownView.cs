using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;


namespace KidGame.Lessons.RepairCrown
{
    public interface IRepairCrownView : IView
    {
        event Action<int, EGemType> GemInPlace;
        event Action BackButtonPressed;
        event Action MistakeHappened;

        IPromise ShowAllGems(List<EGemType> hand);
        IPromise RemoveAllGems();
        void LockAllGems();
        void UnlockAllGems();

        IPromise InsertGemToCrown(int i);
        void SetTargetTypeForGems();
        int GetTargetTypeCount();

        IPromise SetupCrown(EGemType gemType);
        IPromise RemoveCrown();

        // PROGRESS BAR METHODS:

        IPromise CheckpointReached();
        IPromise TranslateToNextCheckpoint();
        void ResetProgress();

        // TUTORIAL

        IPromise ShowTutorial();
        IPromise HighlightAnswer();

        // WIN ROUTINE
        IPromise ShowWinDialog(int starCount);
        IPromise WaitForWinDialogClose();
        IPromise HideWinDialog();
        IPromise PrepareForTransit();
        void CloseAndAnimateChest();
    }
}