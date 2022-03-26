using Orbox.Async;

namespace KidGame.UI
{
    public interface IClickHandler
    {
        void Reset();
        IPromise WaitForClick();
    }
}