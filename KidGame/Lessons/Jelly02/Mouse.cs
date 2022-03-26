using System;
using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.Lessons.Jelly02
{
    public class Mouse : MonoBehaviour
    {
        public QuestBubble QuestBubble;

        public event Action Filled = () => { };

        public Collider2D Collider;
        public Transform PlacedCandiesNest;

        public Transform[] InHandPosition;

        public bool AppearFromLeft = true;
        public bool IsMouse = true;

        private EJellyForm CurrentType;

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

        private void SetupTarget(EJellyForm jellyFormSize)
        {
            CurrentType = jellyFormSize;
        }

        public Collider2D GetTargetCollider()
        {
            return Collider;
        }

        public EJellyForm GetCurrentType()
        {
            return CurrentType;
        }


        public IPromise PutObjectInPlace(Jelly jelly)
        {
            var deferred = new Deferred();
            var duration = 0.3f;

            QuestBubble.Hide();

            SoundManager.Play(Sounds.ECommon.SlimeFall); // toDO: change sound

            jelly.transform.SetParent(PlacedCandiesNest, true);
            
            int inHandSlot =  jelly.GetGeneralForm();

            jelly.transform.SetAsFirstSibling();
            jelly.transform.localScale = Vector3.one;

            var start = jelly.transform.localPosition;

            var slot = InHandPosition[inHandSlot]; 

            var finish = slot.localPosition;
            var rotation = slot.eulerAngles;

            jelly.transform.TweenRotation(true)
                .SetAutoCleanup(true)
                .SetEndValue(rotation)
                .SetDuration(duration)
                .Play();

            jelly.transform.TweenPosition(true)
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

        public IPromise Appear(EJellyForm form)
        {
            QuestBubble.InstantHide();

            SetupTarget(form);

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
                .Then(() => QuestBubble.Show(form));
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
