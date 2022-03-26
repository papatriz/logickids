using System;
using System.Collections.Generic;
using KidGame.UI;
using Orbox.Async;

namespace KidGame.Lessons.Memory04
{
    public interface IMemory04View : IView
    {
        event Action<EJellies> ChoosedFruit;
        event Action BackButtonPressed;

        IPromise SetupAndShowContainers(List<EJellies> hand);
        IPromise CloseContainers();
        IPromise OpenContainers();
        IPromise RemoveContainers();
        IPromise ShowCountDown();
        IPromise RemoveCountDown();
        IPromise ShowQuestion(EJellies quest);
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
        IPromise ShowTutorial(EJellies question);

        // WIN ROUTINE
        IPromise ShowWinDialog(int starCount);
        IPromise WaitForWinDialogClose();
        IPromise HideWinDialog();
        IPromise PrepareForTransit();
        void CloseAndAnimateChest();
    }
}