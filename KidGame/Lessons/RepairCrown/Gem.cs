using KidGame.UI;
using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;

namespace KidGame.Lessons.RepairCrown
{
    public class Gem : CommonDrag
    {
        public EGemType Type;

        private ISoundManager SoundManager;
        private ITimers Timers;

        protected override void Awake()
        {
            base.Awake();

            SoundManager = CompositionRoot.GetSoundManager();
            Timers = CompositionRoot.GetTimers();

            SetType(Type);
        }



        public IPromise Appear()
        {
            var place = GetStartPosition();

            var deferred = new Deferred();
            var duration = 0.8f;

            var startY = place.y + 800;
            var startPos = new Vector2(place.x, startY);

            transform.localPosition = startPos;
            this.gameObject.SetActive(true);

            Timers.Wait(0.27f)
                .Done(() => SoundManager.Play(Sounds.ECommon.Tink2Times));

            transform.TweenPosition(true)
             .SetAutoCleanup(true)
             .SetDuration(duration)
             .SetStartValue(startPos)
             .SetEndValue(place)
                 .SetEasing(TweenEasingFunctions.Bounce)
             .OnCompleted((t) =>
             {
                 
                 SetStartPosition(place);
                 deferred.Resolve();
             })
             .Play();

            return deferred;
        }

        public IPromise DisAppear()
        {
            var deferred = new Deferred();

            var duration = Random.Range(0.35f, 0.55f); 
            var place = GetStartPosition();

            var targetY = place.y - 1200;
            var targerPos = new Vector2(place.x, targetY);

            transform.TweenPosition(true)
             .SetAutoCleanup(true)
             .SetDuration(duration)
             .SetEndValue(targerPos)
                 .SetEasing(TweenEasingFunctions.EaseInCubic)
             .OnCompleted((t) =>
             {
                 this.gameObject.SetActive(false);
                 deferred.Resolve();
             })
             .Play();


            return deferred;
        }

        public IPromise InsertInCrown()
        {

            var deferred = new Deferred();
            var duration = 0.12f;
            var factor = 1.3f;

            SoundManager.Play(Sounds.ECommon.TicTac);

            transform.TweenScale()
             .SetAutoCleanup(true)
             .SetDuration(duration)
             .SetStartValue(Vector2.one)
             .SetEndValue(factor * Vector2.one)
             .SetLoopType(TweenLoopType.Pingpong)
             .SetLoopCount(2)
             .SetEasing(TweenEasingFunctions.EaseInSine)
             .OnCompleted((t) =>
             {
                 deferred.Resolve();
             })
             .Play();

            return deferred;
        }

        public void AttachToCrown(Transform crownTransform)
        {
            this.transform.SetParent(crownTransform);
        }


    }
}