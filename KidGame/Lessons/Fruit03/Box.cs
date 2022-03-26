using System;
using DaikonForge.Tween;
using KidGame.UI;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Fruit03
{
    public class Box : MonoBehaviour
    {
        public event Action<EFruitType> Filled = (t) => { };
        
        public GameObject BoxBG;
        public GameObject BoxBG_Closing;

        public GameObject BoxFG;
        public GameObject BoxClosed;

        public Transform PlacedFruitsNest;
        public Transform[] SlotPosition;

        public EFruitType CurrentType = EFruitType.Big;
        public bool AppearFromLeft = true;

        private enum BoxState
        {
            Open,
            Closing,
            Closed
        }

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

            SetBoxState(BoxState.Closed);

        }


        public EFruitType GetCurrentType()
        {
            return CurrentType;
        }


        public Transform GetMiddlePlaceTransform()
        {
            return SlotPosition[1];
        }

        // ---

        public Collider2D GetTargetCollider()
        {
            return BoxFG.GetComponent<Collider2D>();
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

            SetBoxState(BoxState.Closed);

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

            return deferred.Then(OpenBox);
        }

        public IPromise CloseBox()
        {
            var deffered = new Deferred();

            SetBoxState(BoxState.Closing);
            SoundManager.Play(Sounds.ECommon.ChestSlam); // toDo: find lighter sound

            Timers.Wait(0.3f)
                .Done(() =>
                {
                    SetBoxState(BoxState.Closed);
                    deffered.Resolve();
                });


            return deffered;
        }

        private IPromise OpenBox()
        {
            var deffered = new Deferred();

            SetBoxState(BoxState.Closing);
            SoundManager.Play(Sounds.ECommon.FruitImpact); // toDo: find lighter sound

            Timers.Wait(0.2f)
                .Done(() =>
                {
                    SetBoxState(BoxState.Open);
                    deffered.Resolve();
                });


            return deffered;
        }

        private void SetBoxState(BoxState state)
        {
            switch (state)
            {
                case BoxState.Open:
                    BoxBG.SetActive(true);
                    BoxFG.SetActive(true);
                    BoxBG_Closing.SetActive(false);
                    BoxClosed.SetActive(false);
                    break;

                case BoxState.Closing:
                    BoxBG.SetActive(false);
                    BoxFG.SetActive(true);
                    BoxBG_Closing.SetActive(true);
                    BoxClosed.SetActive(false);
                    break;

                case BoxState.Closed:
                    BoxBG.SetActive(false);
                    BoxFG.SetActive(false);
                    BoxBG_Closing.SetActive(false);
                    BoxClosed.SetActive(true);
                    break;
            }
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
