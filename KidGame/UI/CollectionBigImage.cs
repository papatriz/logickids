using System.Collections;
using System.Collections.Generic;
using DaikonForge.Tween;
using Orbox.Async;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.UI
{
    public class CollectionBigImage : MonoBehaviour
    {
        private Button FButton;
        private Vector3 StartScale;
        private Vector3 StartPos;
        private Vector3 EndPosition;

        private void Reset()
        {
            Button.onClick.RemoveAllListeners();
        }

        public IPromise Show(Transform outsideRoot)
        {
            Reset();

            EndPosition = outsideRoot.position;

            var deferred = new Deferred();
            var durationScale = 0.5f;
            var durationMove = 0.49f;

            gameObject.SetActive(true);

            StartScale = transform.localScale;
            StartPos = transform.position;
            transform.SetParent(outsideRoot);

            transform.TweenScale()
                .SetStartValue(StartScale) // Vector3.zero
                .SetEndValue(Vector3.one)
                .SetDuration(durationScale)
                .SetEasing(TweenEasingFunctions.EaseOutCubic)
                .OnCompleted(t => deferred.Resolve())
                .Play();

            transform.TweenPosition()
                .SetAutoCleanup(true)
                .SetStartValue(StartPos)
                .SetEndValue(EndPosition)
                .SetDuration(durationMove)
                .SetEasing(TweenEasingFunctions.EaseOutCubic)
                .Play();

            return deferred;
        }

        public IPromise Hide(Transform defaultRoot)
        {
            Reset();

            var deferred = new Deferred();
            var durationScale = 0.5f;
            var durationMove = 0.49f;

            Debug.Log("Start(HIDE): " + StartPos);

            transform.TweenScale()
                .SetEndValue(StartScale)
                .SetDuration(durationScale)
                .SetEasing(TweenEasingFunctions.EaseInCubic)
                .OnCompleted(t =>
                {
                    transform.SetParent(defaultRoot);
                    gameObject.SetActive(false);
                    deferred.Resolve();
                })
                .Play();

            transform.TweenPosition()
                .SetAutoCleanup(true)
                .SetEndValue(StartPos)
                .SetDuration(durationMove)
                .SetEasing(TweenEasingFunctions.EaseInCubic)
                .Play();

            return deferred;
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
                if (FButton == null)
                {
                    FButton = GetComponent<Button>();
                }

                return FButton;
            }
        }
    }
}