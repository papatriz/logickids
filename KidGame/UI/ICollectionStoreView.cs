using System;
using Orbox.Async;

namespace KidGame.UI
{
    public interface ICollectionStoreView : IView
    {
        event Action<int> ThumbnailClicked;
        event Action BackButtonClicked;

        void SwitchCollection(int collection); // toDO: make enum and use it as parameter
        void StartHighlightBackButton();
        void StopHighlightBackButton();

        IPromise ShowWithChest();
        IPromise ShowBigPicture(CollectionElement element);
        IPromise ScrollToNextLocked();


        IPromise UnlockNextItem();
        IPromise AddStars();
    }
}