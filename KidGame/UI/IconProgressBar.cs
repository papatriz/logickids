using System.Collections;
using System.Collections.Generic;
using DaikonForge.Tween;
using Orbox.Async;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.UI
{
    public class IconProgressBar : MonoBehaviour
    {
        public Image[] Icons;
        public Image[] Lines;

        public Sprite On;
        public Sprite Off;

        private int CurrentValue;

        public void Reset()
        {
            CurrentValue = 0;

            foreach (Image icon in Icons)
            {
                icon.sprite = Off;
            }

            foreach (Image line in Lines)
            {
                line.fillAmount = 0;
            }
        }

        public IPromise TranslateToNext()
        {
            var deferred = new Deferred();

            if (CurrentValue > Lines.Length) return deferred.Resolve();

            var duration = 0.6f;

            var tween = new Tween<float>();

            tween.SetStartValue(0)
                .SetEndValue(1f)
                .SetDuration(duration)
                .OnExecute(current =>
                {
                    Lines[CurrentValue - 1].fillAmount = current;
                })
                .OnCompleted(t =>
                {
                    deferred.Resolve();
                })
                .Play();

            return deferred;

        }

        public IPromise ActivateNext()
        {
            var deferred = new Deferred();

            var duration = 0.12f;
            var factor = 1.3f;

            if (CurrentValue >= Icons.Length) return deferred.Resolve();

            var icon = Icons[CurrentValue];

            icon.transform.TweenScale()
             .SetAutoCleanup(true)
             .SetDuration(duration)
             .SetStartValue(Vector2.one)
             .SetEndValue(factor * Vector2.one)
             .SetLoopType(TweenLoopType.Pingpong)
             .SetLoopCount(2)
             .SetEasing(TweenEasingFunctions.EaseOutCubic)
             .OnLoopCompleted((t) =>
             {
                 icon.sprite = On;
             })
             .OnCompleted((t) =>
             {
                 CurrentValue++;
                 deferred.Resolve();
             })
             .Play();

            return deferred;

        }

    }
}