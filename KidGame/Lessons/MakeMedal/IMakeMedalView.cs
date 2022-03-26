using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;


namespace KidGame.Lessons.MakeMedal
{
    public interface IMakeMedalView : IView
    {
        event Action MedalFilled;
        event Action BackButtonPressed;
        event Action MistakeHappened;

        IPromise ShowAllGems(List<EGemType> hand);
        IPromise RemoveAllGems();
        void DestroyAllGems();
        void LockAllGems();
        void UnlockUnplacedGems();

        IPromise SetupAndShowMedal(EGemType gemType);
        IPromise RemoveMedal();

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