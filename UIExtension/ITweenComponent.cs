using Orbox.Async;

namespace KidGame.UI
{
    public interface ITweenComponent 
    {
        IPromise Show();
        IPromise Hide();
    }
}