using Orbox.Async;
using UnityEngine;

namespace KidGame.UI
{

    public interface IView
    {
        void SetParent(Transform parent);

        IPromise Show();
        IPromise Hide();

        void ShowInstant();
        void HideInstant();

    }
}