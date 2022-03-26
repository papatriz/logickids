using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;

namespace KidGame.Lessons.Memory01
{
    public interface IMemory01View : IView
    {
        event Action<EFruits> ChoosedFruit;
        event Action BackButtonPressed;

        IPromise SetupAndShowContainers(List<EFruits> hand);
        IPromise CloseContainers();
        IPromise OpenContainers();
        IPromise RemoveContainers();
        IPromise ShowCountDown();
        IPromise RemoveCountDown();
        IPromise ShowQuestion(EFruits quest);
        IPromise HideQuestion();
        IPromise ProccessAnswer(bool right);
        void SetContainersInteractable(bool state);
        IPromise Wait(float t);
        void ChangeBackButtonState(bool state);

        // PROGRESS BAR METHODS:

        IPromise CheckpointReached();
        IPromise TranslateToNextCheckpoint();
        void ResetProgress();

        // TUTORIAL
        IPromise ShowTutorial(EFruits rightAnswer);

        // WIN ROUTINE
        IPromise ShowWinDialog(int starCount);
        IPromise WaitForWinDialogClose();
        IPromise HideWinDialog();
        IPromise PrepareForTransit();
        void CloseAndAnimateChest();
    }
}