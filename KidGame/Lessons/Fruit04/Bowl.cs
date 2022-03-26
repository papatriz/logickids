using System;
using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Fruit04
{
    public class Bowl : MonoBehaviour
    {
        public event Action<EFruitType> Filled = (t) => { };

        public Image BowlFG;
        public Sprite[] BowlFGSprites;

        public Transform PlacedFruitsNest;
        public Transform[] SlotPosition;

        public bool AppearFromLeft = true;

        private EFruitType CurrentType;
        private int CurrentSlot = 0;

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

            var shiftDirection = (AppearFromLeft) ? -1 : 1;

            OutScreenShift = shiftDirection * (canvasWidth / 2 + 1024);

            OnScreenPosition = transform.localPosition;
            OutScreenPosition = new Vector2(OnScreenPosition.x + OutScreenShift, OnScreenPosition.y);

            transform.localPosition = OutScreenPosition;
        }

        public void SetCurrentType(EFruitType type)
        {
            CurrentType = type;

            var normalizedIndex = (int)type % BowlFGSprites.Length;

            BowlFG.sprite = BowlFGSprites[normalizedIndex];
        }

        public EFruitType GetCurrentType()
        {
            return CurrentType;
        }

        // FOR TUTORIAL NEEDS:

        //public Transform GetConditionTransform()
        //{
        //    return OpenBoxTop.transform;
        //}

        public Transform GetMiddlePlaceTransform()
        {
            return SlotPosition[1];
        }

        // ---

        public Collider2D GetTargetCollider()
        {
            return BowlFG.GetComponent<Collider2D>();
        }

        public IPromise PutFruitInPlace(Fruit fruit)
        {
            var deferred = new Deferred();

            var duration = 0.3f;
            var start = fruit.transform.position;

            SoundManager.Play(Sounds.ECommon.FruitToHand); // toDO: change sound

            fruit.transform.SetParent(PlacedFruitsNest, false);
            fruit.transform.SetAsLastSibling();

            var finish = SlotPosition[CurrentSlot].position;
            var rotation = SlotPosition[CurrentSlot].eulerAngles;

            fruit.transform.TweenRotation(true)
                .SetAutoCleanup(true)
                .SetEndValue(rotation)
                .SetDuration(duration)
                .Play();

            fruit.transform.TweenPosition(false)
               .SetAutoCleanup(true)
               .SetDuration(duration)
               .SetStartValue(start)
               .SetEndValue(finish)
               .SetEasing(TweenEasingFunctions.Bounce)
               .OnCompleted((t) =>
               {
               // fruit.transform.rotation = rotation;

               CurrentSlot++;
                   if (CurrentSlot >= SlotPosition.Length)
                   {
                       CurrentSlot = 0;
                       Filled(CurrentType);
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
          //  var deferred03 = new Deferred();

            SoundManager.Play(Sounds.ECommon.Hooray);

            var fruits = GetComponentsInChildren<Fruit>();

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
         //   var fruitPos03 = fruits[2].transform.localPosition;

            var rnd01 = 380 + Random.Range(0, 10);
            var shift01 = new Vector3(0, rnd01, 0);

            var rnd02 = 340 + Random.Range(0, 10);
            var shift02 = new Vector3(0, rnd02, 0);

            //var rnd03 = 370 + Random.Range(0, 10);
            //var shift03 = new Vector3(0, rnd03, 0);

            //var dur01 = 0.35f + Random.Range(0, 0.1f);
            //var dur02 = 0.35f + Random.Range(0, 0.1f);

            var dur01 = 0.35f;
            var dur02 = 0.45f;
            var dur03 = 0.4f;

            Debug.Log("Rnd1=" + rnd01 + "  Dur1=" + dur01);
            Debug.Log("Rnd2=" + rnd02 + "  Dur1=" + dur02);

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

            //var tweenFruit03Down = fruits[2].transform.TweenPosition(true)
            //     .SetAutoCleanup(true)
            //     .SetDuration(dur03 + 0.12f)
            //     .SetStartValue(fruitPos03 + shift03)
            //     .SetEndValue(fruitPos03)
            //     .SetEasing(TweenEasingFunctions.Bounce)
            //    .OnCompleted((t) =>
            //    {
            //        SoundManager.Play(Sounds.ECommon.FruitImpact);
            //        deferred03.Resolve();
            //    });

            //var tweenFruit03Up = fruits[2].transform.TweenPosition(true)
            //     .SetAutoCleanup(true)
            //     .SetDuration(dur03)
            //     .SetStartValue(fruitPos03)
            //     .SetEndValue(fruitPos03 + shift03)
            //     .SetEasing(TweenEasingFunctions.EaseInSine)
            //    .OnCompleted((t) =>
            //    {
            //        tweenFruit03Down.Play();
            //    })
            //    .Play();


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
                 deferred.Resolve();
             });

            tweenPos.Play();

            return deferred;
        }
    }
}