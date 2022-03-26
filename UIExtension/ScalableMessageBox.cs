using Orbox.Async;
using UnityEngine.UI;


namespace KidGame.UI
{
    public class ScalableMessageBox : TweenScalable, ITweenComponent, IClickHandler
    {
        private Button FButton;

        public void Reset()
        {
            Button.onClick.RemoveAllListeners();
        }

        public override IPromise Show()
        {
            Reset();
            return base.Show();
        }

        public virtual IPromise WaitForClick()
        {
            var deferred = new Deferred();
            
            Button.onClick.AddListener(() =>
            {
                Button.onClick.RemoveAllListeners();
                deferred.Resolve();
            });

            return deferred;
        }

        // --- private ---
        private Button Button
        {
            get
            {
                if(FButton == null)
                {
                    FButton = GetComponent<Button>();
                }

                return FButton;
            }
        }
    }
}