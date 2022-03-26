using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;


namespace KidGame.Lessons.Memory02
{
    public interface IMemory02View : IView
    {
        event Action<EGems> ChoosedFruit;
        event Action BackButtonPressed;

        IPromise SetupAndShowContainers(List<EGems> hand);
        IPromise CloseContainers();
        IPromise OpenContainers();
        IPromise RemoveContainers();
        IPromise ShowCountDown();
        IPromise RemoveCountDown();
        IPromise ShowQuestion(EGems quest);
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
        IPromise ShowTutorial(EGems question);

        // WIN ROUTINE
        IPromise ShowWinDialog(int starCount);
        IPromise WaitForWinDialogClose();
        IPromise HideWinDialog();
        IPromise PrepareForTransit();
        void CloseAndAnimateChest();
    }
}