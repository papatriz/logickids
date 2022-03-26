using UnityEngine;
using Orbox.Async;
using UnityEngine.UI;
using System;

namespace KidGame.UI
{
    public class TweenScalable : MonoBehaviour
    {
        public float TweenTime;

        public virtual IPromise Show()
        {
            var deferred = new Deferred();
            gameObject.SetActive(true);

            transform.TweenScale()
                .SetStartValue(new Vector3(0.001f, 0.001f, 1f)) // Vector3.zero
                .SetEndValue(Vector3.one)
                .SetDuration(TweenTime)
                .OnCompleted(t => deferred.Resolve())
                .Play();

            return deferred;
        }

        public virtual IPromise Hide()
        {
            var deferred = new Deferred();

            transform.TweenScale()
                .SetStartValue(Vector3.one)
                .SetEndValue(new Vector3(0.001f, 0.001f, 1f))
                .SetDuration(TweenTime)
                .OnCompleted(t => DeactivateAndResolve(deferred))
                .Play();

            return deferred;
        }

        private void DeactivateAndResolve(Deferred deferred)
        {
            gameObject.SetActive(false);
            deferred.Resolve();
        }
    }
}