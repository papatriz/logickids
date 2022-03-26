using DaikonForge.Tween;
using KidGame.UI;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Maze
{
    public class TargetIndicator : MonoBehaviour
    {

        private TweenBase Animation;
        private Tween<float> Alpha;
        private Image Image;

        private void Awake()
        {
            Image = GetComponent<Image>();

            var duration = 0.4f;
            var factor = 1.3f;

            Animation = transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(duration)
                 .SetStartValue(Vector2.one)
                 .SetEndValue(factor * Vector2.one)
                 .SetLoopType(TweenLoopType.Pingpong)
                 .SetLoopCount(0);
               //  .SetEasing(TweenEasingFunctions.EaseInOutCubic);

            Alpha = new Tween<float>();

            Alpha.SetAutoCleanup(true)
                 .SetStartValue(0.3f)
                 .SetEndValue(1f)
                 .SetDuration(duration)
                 .SetLoopType(TweenLoopType.Pingpong)
                 .SetLoopCount(0)
                 .OnExecute(current =>
                 {
                     SetAlpha(Image, current);
                 });


        }

        public void Activate(Vector2 pos)
        {
            transform.localPosition = pos;
            gameObject.SetActive(true);

            Animation.Play();
            Alpha.Play();
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);

            Animation.Stop();
            Alpha.Stop();
            Animation.Rewind();
            Alpha.Rewind();
        }

        private void SetAlpha(Image image, float alpha = 0f)
        {
            var tmp = image.color;
            tmp.a = alpha;
            image.color = tmp;
        }
    }
}
