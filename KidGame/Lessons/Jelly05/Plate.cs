using System;
using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Jelly05
{
    public class Plate : MonoBehaviour
    {
        public event Action<Plate> Filled = (t) => { };

        public Collider2D Collider;
        public Transform PlacedCandiesNest;

        public Transform Slot;
        public Transform QuestSlot;

        public Sprite[] Sprites;

        public bool AppearFromLeft = true;

        private EJellyType CurrentType;

        private GameObject CurrentQuest;

        private Vector2 OnScreenPosition;
        private Vector2 OutScreenPosition;

        private int OutScreenShift;

        private Image Image;

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

            Image = Collider.GetComponent<Image>();

        }

        public void SetCurrentQuest(Jelly quest)
        {
            var questTransform = quest.transform;

            var pos = QuestSlot.position;
            var rot = QuestSlot.rotation;

            questTransform.SetParent(PlacedCandiesNest, false);
            questTransform.position = pos;
            questTransform.rotation = rot;

            questTransform.gameObject.SetActive(true);
            CurrentQuest = quest.gameObject;

            CurrentType = quest.GetJType();

            Image.sprite = Sprites[(int)CurrentType % 4];
        }


        public Collider2D GetTargetCollider()
        {
            return Collider;
        }

        public EJellyType GetCurrentQuest()
        {
            return CurrentType;
        }

        public IPromise PutObjectInPlace(Jelly jelly)
        {
            var deferred = new Deferred();

            var duration = 0.3f;
            var start = jelly.transform.position;

            SoundManager.Play(Sounds.ECommon.SlimeFall);

            jelly.transform.SetParent(PlacedCandiesNest, false);

            jelly.transform.SetAsFirstSibling(); 

            var finish = Slot.position;
            var rotation = Slot.eulerAngles;

            jelly.transform.TweenRotation(true)
                .SetAutoCleanup(true)
                .SetEndValue(rotation)
                .SetDuration(duration)
                .Play();

            jelly.transform.TweenPosition(false)
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
                 Timers.Wait(0.8f)
                 .Done(() =>
                 {
                     deferred.Resolve();
                 });

             })
             .Play();

            return deferred;
        }

        public IPromise Toss() 
        {
            var defferedMain = new Deferred();

            var deferred01 = new Deferred();
            var deferred02 = new Deferred();

            SoundManager.Play(Sounds.ECommon.Hooray);

            var fruits = GetComponentsInChildren<Jelly>();

            var initPlateScale = transform.localScale;
            var plateScale = 1.3f;

            var tweenPos = transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(0.3f)
                 .SetStartValue(initPlateScale)
                 .SetEndValue(plateScale * initPlateScale)
                 .SetLoopType(TweenLoopType.Pingpong)
                 .SetLoopCount(2)
                 .SetEasing(TweenEasingFunctions.EaseOutSine)
                 .OnCompleted((t) =>
                 {
                     defferedMain.Resolve();
                 })
                 .Play();

            var scale01 = 2f;
            var scale02 = 1.8f;

            var dur01 = 0.35f;
            var dur02 = 0.45f;

            var tweenFruit01Down = fruits[0].transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(dur01 + 0.12f)
                 .SetStartValue(scale01 * Vector3.one)
                 .SetEndValue(Vector3.one)
                 .SetEasing(TweenEasingFunctions.Bounce)
                .OnCompleted((t) =>
                {
                    deferred01.Resolve();
                });

            var tweenFruit01Up = fruits[0].transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(dur01)
                 .SetStartValue(Vector3.one)
                 .SetEndValue(scale01 * Vector3.one)
                 .SetEasing(TweenEasingFunctions.EaseInSine)
                .OnCompleted((t) =>
                {
                    SoundManager.Play(Sounds.ECommon.SlimeFall);
                    tweenFruit01Down.Play();
                })
                .Play();

            var tweenFruit02Down = fruits[1].transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(dur02 + 0.12f)
                 .SetStartValue(scale02 * Vector3.one)
                 .SetEndValue(Vector3.one)
                 .SetEasing(TweenEasingFunctions.Bounce)
                .OnCompleted((t) =>
                {
                    deferred02.Resolve();
                });

            var tweenFruit02Up = fruits[1].transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(dur02)
                 .SetStartValue(Vector3.one)
                 .SetEndValue(scale02 * Vector3.one)
                 .SetEasing(TweenEasingFunctions.EaseInSine)
                .OnCompleted((t) =>
                {
                    SoundManager.Play(Sounds.ECommon.SlimeFall);
                    tweenFruit02Down.Play();
                })
                .Play();


            return Deferred.All(deferred01, deferred02, defferedMain);
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
                 Destroy(CurrentQuest);
                 deferred.Resolve();
             });

            tweenPos.Play();

            return deferred;
        }
    }
}