using System;
using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Candy03
{
    public class Plate : MonoBehaviour
    {
        public event Action<ECandyShape> Filled = (t) => { };

        public Collider2D Collider;
        public Transform PlacedCandiesNest;

        public Transform[] Slot01Position;
        public Transform[] Slot02Position;
        public Transform[] Slot03Position;

        public ECandyShape CurrentShape;
        public bool AppearFromLeft = true;



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

            var shiftDirection = AppearFromLeft ? -1 : 1;

            var canvasWidth = (int)FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;

            OutScreenShift = shiftDirection * (canvasWidth / 2 + 1024);

            OnScreenPosition = transform.localPosition;
            OutScreenPosition = new Vector2(OnScreenPosition.x + OutScreenShift, OnScreenPosition.y);

            transform.localPosition = OutScreenPosition;

        }

        // FOR TUTORIAL NEEDS:

        //public Transform GetConditionTransform()
        //{
        //    return OpenBoxTop.transform;
        //}

        //public Transform GetMiddlePlaceTransform()
        //{
        //    return SlotPosition[1];
        //}

        // ---

        public Collider2D GetTargetCollider()
        {
            return Collider;
        }

        public IPromise PutObjectInPlace(Candy candy)
        {
            var deferred = new Deferred();

            var duration = 0.3f;
            var start = candy.transform.position;

            SoundManager.Play(Sounds.ECommon.FruitToHand); // toDO: change sound

            candy.transform.SetParent(PlacedCandiesNest, false);
            candy.transform.SetAsFirstSibling();

            var form = candy.GetForm();
            int posIndex = ((int)form - (int)CurrentShape) / 2;

            Debug.Log("Form: " + form + " Shape:" + CurrentShape+" index="+posIndex);

            var slot = (CurrentSlot == 0) ? Slot01Position[posIndex] : (CurrentSlot == 1) ? Slot02Position[posIndex] : Slot03Position[posIndex];

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
                   CurrentSlot++;
                   if (CurrentSlot > 2)
                   {
                       CurrentSlot = 0;
                       Filled(CurrentShape);
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

        public IPromise Toss()
        {
            var defferedMain = new Deferred();

            var deferred01 = new Deferred();
            var deferred02 = new Deferred();
            var deferred03 = new Deferred();

            SoundManager.Play(Sounds.ECommon.Hooray);

            var fruits = GetComponentsInChildren<Candy>();

            var tweenPos = transform.TweenPosition(true)
                 .SetAutoCleanup(true)
                 .SetDuration(0.3f)
                 .SetStartValue(OnScreenPosition)
                 .SetEndValue(OnScreenPosition + new Vector2(0, 130f))
                 .SetLoopType(TweenLoopType.Pingpong)
                 .SetLoopCount(2)
                 .SetEasing(TweenEasingFunctions.EaseOutSine)
                 .OnCompleted((t) =>
                 {
                     defferedMain.Resolve();
                 })
                 .Play();

            var fruitPos01 = fruits[0].transform.localPosition;
            var fruitPos02 = fruits[1].transform.localPosition;
            var fruitPos03 = fruits[2].transform.localPosition;

            var rnd01 = 380 + Random.Range(0, 10);
            var shift01 = new Vector3(0, rnd01, 0);

            var rnd02 = 340 + Random.Range(0, 10);
            var shift02 = new Vector3(0, rnd02, 0);

            var rnd03 = 340 + Random.Range(0, 10);
            var shift03 = new Vector3(0, rnd03, 0);

            var dur01 = 0.35f;
            var dur02 = 0.45f;
            var dur03 = 0.38f;

            var tweenFruit01Down = fruits[0].transform.TweenPosition(true)
                 .SetAutoCleanup(true)
                 .SetDuration(dur01 + 0.12f)
                 .SetStartValue(fruitPos01 + shift01)
                 .SetEndValue(fruitPos01)
                 .SetEasing(TweenEasingFunctions.Bounce)
                .OnCompleted((t) =>
                {
                    SoundManager.Play(Sounds.ECommon.FruitImpact);
                    deferred01.Resolve();
                });

            var tweenFruit01Up = fruits[0].transform.TweenPosition(true)
                 .SetAutoCleanup(true)
                 .SetDuration(dur01)
                 .SetStartValue(fruitPos01)
                 .SetEndValue(fruitPos01 + shift01)
                 .SetEasing(TweenEasingFunctions.EaseInSine)
                .OnCompleted((t) =>
                {
                    tweenFruit01Down.Play();
                })
                .Play();

            var tweenFruit02Down = fruits[1].transform.TweenPosition(true)
                 .SetAutoCleanup(true)
                 .SetDuration(dur02 + 0.12f)
                 .SetStartValue(fruitPos02 + shift02)
                 .SetEndValue(fruitPos02)
                 .SetEasing(TweenEasingFunctions.Bounce)
                .OnCompleted((t) =>
                {
                    SoundManager.Play(Sounds.ECommon.FruitImpact);
                    deferred02.Resolve();
                });

            var tweenFruit02Up = fruits[1].transform.TweenPosition(true)
                 .SetAutoCleanup(true)
                 .SetDuration(dur02)
                 .SetStartValue(fruitPos02)
                 .SetEndValue(fruitPos02 + shift02)
                 .SetEasing(TweenEasingFunctions.EaseInSine)
                .OnCompleted((t) =>
                {
                    tweenFruit02Down.Play();
                })
                .Play();

            var tweenFruit03Down = fruits[2].transform.TweenPosition(true)
                 .SetAutoCleanup(true)
                 .SetDuration(dur03 + 0.12f)
                 .SetStartValue(fruitPos03 + shift03)
                 .SetEndValue(fruitPos03)
                 .SetEasing(TweenEasingFunctions.Bounce)
                .OnCompleted((t) =>
                {
                    SoundManager.Play(Sounds.ECommon.FruitImpact);
                    deferred03.Resolve();
                });

            var tweenFruit03Up = fruits[2].transform.TweenPosition(true)
                 .SetAutoCleanup(true)
                 .SetDuration(dur03)
                 .SetStartValue(fruitPos03)
                 .SetEndValue(fruitPos03 + shift03)
                 .SetEasing(TweenEasingFunctions.EaseInSine)
                .OnCompleted((t) =>
                {
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