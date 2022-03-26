using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.Maze
{
    public class RangeIndicator : MonoBehaviour
    {
        private Image Image;
        private Tween<float> AlphaUp;
        private Tween<float> AlphaDown;

        private float startAlpha;

        private void Awake()
        {
            Image = GetComponent<Image>();
            AlphaUp = new Tween<float>();
            AlphaDown = new Tween<float>();

            startAlpha = Image.color.a;

        }

        private void OnDestroy()
        {
            AlphaUp.Stop();
            AlphaDown.Stop();
            SetAlpha(Image, startAlpha);
        }

        public IPromise Flash()
        {
            var deffered = new Deferred();

            AlphaDown.SetAutoCleanup(true)
                 .SetStartValue(1f)
                 .SetEndValue(startAlpha)
                 .SetDuration(0.3f)
                 .OnExecute(current =>
                 {
                     SetAlpha(Image, current);
                 })
                 .OnCompleted((t) =>
                 {
                     Debug.Log("Twween complete, alpha =" + Image.color.a);
                     deffered.Resolve();
                 });

            AlphaUp.SetAutoCleanup(true)
                 .SetStartValue(startAlpha)
                 .SetEndValue(1f)
                 .SetDuration(0.3f)
                 .OnExecute(current =>
                 {
                     SetAlpha(Image, current);
                 })
                 .OnCompleted((t) =>
                 {
                     AlphaDown.Play();
                 })
                 .Play();

           // Alpha.Play();

            return deffered;
        }

        private void SetAlpha(Image image, float alpha = 0f)
        {
            var tmp = image.color;
            tmp.a = alpha;
            image.color = tmp;
        }
    }
}