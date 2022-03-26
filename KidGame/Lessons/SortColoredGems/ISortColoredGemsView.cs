using System;

using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;


namespace KidGame.Lessons.SortColoredGems
{
    public interface ISortColoredGemsView : IView
{
    event Action BoxIsFilled;
    event Action BackButtonPressed;
    event Action MistakeHappened;

    IPromise ShowAllGems(EGemType properColor, EGemType fakeColor);
    IPromise RemoveAllGems();
    void LockAllGems();
    void UnlockUnsortedGems();

    IPromise SetupAndShowBox(EGemType type);
    IPromise RemoveBox();

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