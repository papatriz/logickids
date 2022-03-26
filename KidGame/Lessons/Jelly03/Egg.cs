using System;
using DaikonForge.Tween;
using KidGame.UI;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Jelly03
{
    public class Egg : MonoBehaviour
    {
        public event Action<Egg> Filled = (t) => { };

        public Transform EggTop;
        public Image FGImage;

        public Transform PlacedJellyNest;

        public bool AppearFromLeft = true;

        public Sprite[] TopSprites;
        public Sprite[] FGSprites;

        private Image TopImage;

        private Vector2 OnScreenPosition;
        private Vector2 OutScreenPosition;

        private int OutScreenShift;

        private Vector3 TopClosePosition;
        private Vector3 TopOpenPosition = new Vector3(312,57,0);
        private Vector3 TopOpenRotation;

        private EColors CurrentType;

        private ISoundManager SoundManager;
        private ITimers Timers;

        private void Awake()
        {
            SoundManager = CompositionRoot.GetSoundManager();
            Timers = CompositionRoot.GetTimers();

            var canvasWidth = (int)FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;

            var shiftDirection = (AppearFromLeft) ? -1 : 1;

            OutScreenShift = shiftDirection * (canvasWidth / 2 + 1024);

            OnScreenPosition = transform.localPosition;
            OutScreenPosition = new Vector2(OnScreenPosition.x + OutScreenShift, OnScreenPosition.y);

            transform.localPosition = OutScreenPosition;

            TopClosePosition = EggTop.localPosition;

            var openShiftX = 258f;//312f;
            var openZDegree = 55f;// 60f;
            var posSign = AppearFromLeft ? -1 : 1;
            TopOpenPosition = new Vector3(posSign * openShiftX, 68, 0); //y=57
            TopOpenRotation = new Vector3(0, 0, -posSign * openZDegree);

            TopImage = EggTop.GetComponent<Image>();
        }


        // ---

        public Collider2D GetTargetCollider()
        {
            return GetComponent<Collider2D>();
        }

        public EColors GetCurrentType()
        {
            return CurrentType;
        }

        public IPromise PutObjectInPlace(Jelly jelly)
        {
            var deferred = new Deferred();

            var duration = 0.3f;
            

            SoundManager.Play(Sounds.ECommon.SlimeFall);

            jelly.transform.SetParent(PlacedJellyNest, true);
            jelly.transform.SetAsLastSibling();

            var start = jelly.transform.localPosition;
            var finish = new Vector3(0, 50, 0);

            jelly.transform.TweenRotation(true)
                .SetAutoCleanup(true)
                .SetEndValue(Vector3.zero)
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
                   Filled(this);
                   deferred.Resolve();
               })
            .Play();

            return deferred;
        }

        public void Setup(EColors color)
        {
            TopImage.sprite = TopSprites[(int)color];
            FGImage.sprite = FGSprites[(int)color];
            CurrentType = color;
        }

        public IPromise Appear()
        {
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
                 Timers.Wait(0.1f)
                 .Done(() =>
                 {
                     deferred.Resolve();

                 });

             })
             .Play();

            return deferred.Then(Open);
        }

        public IPromise Close(bool instant = false)
        {
            var deferred = new Deferred();
            var duration = 0.4f;

            if (instant)
            {
                EggTop.localPosition = TopClosePosition;
                EggTop.localEulerAngles = Vector3.zero;

                return deferred.Resolve();
            }

            Timers.Wait(0.2f)
                .Done(() => SoundManager.Play(Sounds.ECommon.OpenShort));

            EggTop.TweenRotation(true)
                .SetAutoCleanup(true)
                .SetEndValue(Vector3.zero)
               .SetEasing(TweenEasingFunctions.EaseInCubic)
                .SetDuration(duration)
                .Play();

            EggTop.TweenPosition(true)
               .SetAutoCleanup(true)
               .SetDuration(duration)
               .SetEndValue(TopClosePosition)
               .SetEasing(TweenEasingFunctions.EaseInCubic)
               .OnCompleted((t) =>
               {
                   deferred.Resolve();
               })
            .Play();


            return deferred;
        }

        private IPromise Open()
        {
            var deferred = new Deferred();
            var duration = 0.4f;

            SoundManager.Play(Sounds.ECommon.OpeningCan); // toDo: find proper sound

            EggTop.TweenRotation(true)
                .SetAutoCleanup(true)
                .SetDelay(0.2f)
                .SetEndValue(TopOpenRotation)
               .SetEasing(TweenEasingFunctions.EaseInCubic)
                .SetDuration(duration)
                .Play();

            EggTop.TweenPosition(true)
               .SetAutoCleanup(true)
               .SetDelay(0.2f)
               .SetDuration(duration)
               .SetEndValue(TopOpenPosition)
               .SetEasing(TweenEasingFunctions.EaseInCubic)
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
