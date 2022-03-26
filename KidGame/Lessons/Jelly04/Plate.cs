using System;
using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Jelly04
{
    public class Plate : MonoBehaviour
    {
        public event Action Filled;

        public Collider2D Collider;
        public Transform PlacedCandiesNest;

        public Transform Slot01Position;
        public Transform Slot02Position;

        public Transform QuestSlotPosition;

        private EJellyFeature CurrentFeature;

        private int CurrentSlot = 0;
        private GameObject CurrentQuest;

        private Vector2 OnScreenPosition;
        private Vector2 OutScreenPosition;

        private int OutScreenShift;

        private ISoundManager SoundManager;
        private ITimers Timers;

        private void Awake()
        {
            SoundManager = CompositionRoot.GetSoundManager();
            Timers = CompositionRoot.GetTimers();

            var canvasWidth = (int)FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;

            OutScreenShift = -(canvasWidth / 2 + 1024);

            OnScreenPosition = transform.localPosition;
            OutScreenPosition = new Vector2(OnScreenPosition.x + OutScreenShift, OnScreenPosition.y);

            transform.localPosition = OutScreenPosition;

        }

        public void SetCurrentQuest(Jelly quest)
        {
            var questTransform = quest.transform;

            var pos = QuestSlotPosition.position;
            var rot = QuestSlotPosition.rotation;

            questTransform.SetParent(PlacedCandiesNest, false);
            questTransform.position = pos;
            questTransform.rotation = rot;

            questTransform.gameObject.SetActive(true);
            CurrentQuest = quest.gameObject;

            CurrentFeature = quest.GetFeature();
        }


        public Collider2D GetTargetCollider()
        {
            return Collider;
        }

        public EJellyFeature GetCurrentQuest()
        {
            return CurrentFeature;
        }

        public IPromise PutObjectInPlace(Jelly jelly)
        {
            var deferred = new Deferred();

            var duration = 0.3f;
            var start = jelly.transform.position;

            SoundManager.Play(Sounds.ECommon.SlimeFall);

            jelly.transform.SetParent(PlacedCandiesNest, false);


            if (CurrentSlot == 0)
            { jelly.transform.SetAsFirstSibling(); }
            else
            { jelly.transform.SetAsLastSibling(); }

            var slot = (CurrentSlot == 0) ? Slot01Position : Slot02Position;

            var finish = slot.position;
            var rotation = slot.eulerAngles;

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
                   CurrentSlot++;
                   if (CurrentSlot >= 2)
                   {
                       CurrentSlot = 0;
                       Filled();
                   }

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
                     CurrentSlot = 0;
                     deferred.Resolve();
                 });

             })
             .Play();

            return deferred;
        }

        public IPromise Toss() // toDo: Change animation to top view (scale up/down instead of move)
        {
            var defferedMain = new Deferred();

            var deferred01 = new Deferred();
            var deferred02 = new Deferred();
            var deferred03 = new Deferred();

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
            var scale03 = 1.8f;



            var dur01 = 0.35f;
            var dur02 = 0.45f;
            var dur03 = 0.38f;

            var tweenFruit01Down = fruits[0].transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(dur01 + 0.12f)
                 .SetStartValue(scale01 * Vector3.one)
                 .SetEndValue(Vector3.one)
                 .SetEasing(TweenEasingFunctions.Bounce)
                .OnCompleted((t) =>
                {
                 //   SoundManager.Play(Sounds.ECommon.SlimeFall);
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
                  //  SoundManager.Play(Sounds.ECommon.SlimeFall);
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

            var tweenFruit03Down = fruits[2].transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(dur03 + 0.12f)
                 .SetStartValue(scale03 * Vector3.one)
                 .SetEndValue(Vector3.one)
                 .SetEasing(TweenEasingFunctions.Bounce)
                .OnCompleted((t) =>
                {
                   // SoundManager.Play(Sounds.ECommon.SlimeFall);
                    deferred03.Resolve();
                });

            var tweenFruit03Up = fruits[2].transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(dur03)
                 .SetStartValue(Vector3.one)
                 .SetEndValue(scale03 * Vector3.one)
                 .SetEasing(TweenEasingFunctions.EaseInSine)
                .OnCompleted((t) =>
                {
                    SoundManager.Play(Sounds.ECommon.SlimeFall);
                    tweenFruit03Down.Play();
                })
                .Play();


            return Deferred.All(deferred01, deferred02, deferred03, defferedMain);
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