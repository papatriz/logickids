using System;
using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.Lessons.Candy04
{
    public class Character : MonoBehaviour
    {
        public QuestBubble QuestBubble;

        public event Action Filled = () => { };

        public Collider2D Collider;
        public Transform PlacedCandiesNest;

        public Transform[] InHandPosition;

        public bool AppearFromLeft = true;
        public bool IsMouse = true;

        private ECandyFormAndSize CurrentType;

        private Vector2 OnScreenPosition;
        private Vector2 OutScreenPosition;

        private int OutScreenShift;

        private ISoundManager SoundManager;
        private ITimers Timers;

        private void Awake()
        {
            SoundManager = CompositionRoot.GetSoundManager();
            Timers = CompositionRoot.GetTimers();

            var shiftDirection = AppearFromLeft ? -1 : 1;

            var canvasWidth = (int)FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;

            OutScreenShift = shiftDirection * (canvasWidth / 2 + 1024);

            OnScreenPosition = transform.localPosition;
            OutScreenPosition = new Vector2(OnScreenPosition.x + OutScreenShift, OnScreenPosition.y);

            transform.localPosition = OutScreenPosition;
        }

        private void SetupTarget(ECandyFormAndSize candyFormSize)
        {
            CurrentType = candyFormSize;
        }

        public Collider2D GetTargetCollider()
        {
            return Collider;
        }

        public Transform GetTargetForTutorial()
        {
            return QuestBubble.GetTargetForTutorial();
        }

        public ECandyFormAndSize GetCurrentType()
        {
            return CurrentType;
        }


        public IPromise PutObjectInPlace(Candy candy)
        {
            var deferred = new Deferred();
            var duration = 0.3f;

            QuestBubble.Hide();

            SoundManager.Play(Sounds.ECommon.FruitToHand); // toDO: change sound

            candy.transform.SetParent(PlacedCandiesNest, true);

            var formsize = candy.GetFormAndSize();
            int inHandSlot = (int)formsize % 5;

            candy.transform.SetAsFirstSibling();
            candy.transform.localScale = Vector3.one;

            var slot = InHandPosition[inHandSlot];

            var start = candy.transform.position;

            var finish = slot.position;
            var rotation = slot.eulerAngles;

            candy.transform.TweenRotation(true)
                .SetAutoCleanup(true)
                .SetEndValue(rotation)
                .SetDuration(duration)
                .Play();

            candy.transform.TweenPosition(false)
               .SetAutoCleanup(true)
               .SetDuration(duration)
               .SetStartValue(start)
               .SetEndValue(finish)
               .SetEasing(TweenEasingFunctions.Bounce)
               .OnCompleted((t) =>
               {    
                   HoorayJump()
                    .Done(() =>
                    {
                        deferred.Resolve();
                        Filled();
                    });

               })
            .Play();

            return deferred;
        }

        public IPromise Appear(ECandyFormAndSize formAndSize)
        {
            QuestBubble.InstantHide();

            SetupTarget(formAndSize);

            var deferred = new Deferred();
            var duration = 0.6f;

            SoundManager.Play(Sounds.ECommon.Whoosh);

            transform.TweenPosition(true)
             .SetAutoCleanup(true)
             .SetDuration(duration)
             .SetStartValue(OutScreenPosition)
             .SetEndValue(OnScreenPosition)
             .SetEasing(TweenEasingFunctions.EaseOutCubic)
             .OnCompleted((t) =>
             {
                 Timers.Wait(0.8f)
                 .Done(() =>
                 {
                     deferred.Resolve();
                 });

             })
             .Play();

            return deferred
                .Then(() => QuestBubble.Show(formAndSize));
        }

        private IPromise HoorayJump()
        {
            var deferred = new Deferred();
            var duration = 0.3f;

            var sound = IsMouse ? Sounds.ECommon.MouseYe : Sounds.ECommon.Woohoo;
            SoundManager.Play(sound); // toDo: make different sounds for Mouse and Kid

            var start = transform.localPosition;
            var finish = start + new Vector3(0, 60, 0);

            transform.TweenPosition(true)
               .SetAutoCleanup(true)
               .SetDuration(duration)
               .SetStartValue(start)
               .SetEndValue(finish)
               .SetLoopType(TweenLoopType.Pingpong)
               .SetLoopCount(2)
               .SetEasing(TweenEasingFunctions.EaseOutCubic)
               .OnCompleted((t) =>
               {
                   deferred.Resolve();
               })
            .Play();

            return deferred;
        }

        public IPromise DisAppear()
        {
            var deferred = new Deferred();

            var moveDuration = 0.6f;

            var tweenPos = transform.TweenPosition(true)
             .SetAutoCleanup(true)
             .SetDuration(moveDuration)
             .SetStartValue(OnScreenPosition)
             .SetEndValue(OutScreenPosition)
             .SetEasing(TweenEasingFunctions.EaseInCubic)
             .OnCompleted((t) =>
             {
                 deferred.Resolve();
             });

            tweenPos.Play();

            return deferred;
        }


    }
}
