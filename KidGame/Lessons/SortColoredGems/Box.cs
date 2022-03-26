using System;
using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.Lessons.SortColoredGems
{
    public class Box : MonoBehaviour
    {
        public event Action BoxIsFull;

        public GameObject ClosedBox;
        public GameObject OpenBoxBG;
        public GameObject OpenBoxFG;

        public Image SmallGem;
        public Image OpenBoxTop;
        public Sprite[] TopSprites;
        public Sprite[] SmallGemSprites;

        public Transform PlacedGemsNest;
        public Transform[] SlotPosition;

        private EGemType CurrentType = EGemType.Red;
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

            OutScreenShift = -(canvasWidth / 2 + 1024);

            OnScreenPosition = transform.localPosition;
            OutScreenPosition = new Vector2(OnScreenPosition.x + OutScreenShift, OnScreenPosition.y);

            transform.localPosition = OutScreenPosition;

        }

        public void SetCurrentType(EGemType type)
        {
            CurrentType = type;
            OpenBoxTop.sprite = TopSprites[(int)type];
            SmallGem.sprite = SmallGemSprites[(int)type];
        }

        public EGemType GetCurrentType()
        {
            return CurrentType;
        }

        // FOR TUTORIAL NEEDS:

        public Transform GetConditionTransform()
        {
            return OpenBoxTop.transform;
        }

        public Transform GetMiddlePlaceTransform()
        {
            return SlotPosition[1];
        }

        // ---

        public Collider2D GetCurrentTargetCollider()
        {
            return OpenBoxBG.GetComponent<Collider2D>();
        }

        public IPromise PutGemInPlace(Gem gem)
        {
            var deferred = new Deferred();

            var duration = 0.3f;
            var start = gem.transform.position;

            SoundManager.Play(Sounds.ECommon.PutIceInGlass);

            gem.transform.SetParent(PlacedGemsNest, false);
            gem.transform.SetAsLastSibling();

            var finish = SlotPosition[CurrentSlot].position;
            var rotation = SlotPosition[CurrentSlot].rotation;

            gem.transform.TweenPosition(false)
               .SetAutoCleanup(true)
               .SetDuration(duration)
               .SetStartValue(start)
               .SetEndValue(finish)
               .SetEasing(TweenEasingFunctions.Bounce)
               .OnCompleted((t) =>
               {
                   gem.transform.rotation = rotation;

                   CurrentSlot++;
                   if (CurrentSlot >= SlotPosition.Length)
                   {
                       CurrentSlot = 0;
                       BoxIsFull();
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

            CloseBox(); 

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
                     OpenBox();
                     deferred.Resolve();
                 });
                 
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

            SoundManager.Play(Sounds.ECommon.ChestSlam);
            CloseBox();

            var factor = 1.2f;
            var scaleDuration = 0.3f;

           // SoundManager.Play(Sounds.ECommon.TicTac);

            ClosedBox.transform.TweenScale()
             .SetAutoCleanup(true)
             .SetDuration(scaleDuration)
             .SetStartValue(Vector2.one)
             .SetEndValue(factor * Vector2.one)
             .SetLoopType(TweenLoopType.Pingpong)
             .SetLoopCount(2)
             .SetEasing(TweenEasingFunctions.EaseOutCubic)
             .OnCompleted((t) =>
             {
                 SoundManager.Play(Sounds.ECommon.Whoosh);
                 tweenPos.Play();
             })
             .Play();

            return deferred;
        }

        private void OpenBox()
        {
            ClosedBox.SetActive(false);
            OpenBoxBG.SetActive(true);
            OpenBoxFG.SetActive(true);
            OpenBoxTop.gameObject.SetActive(true);
        }

        private void CloseBox()
        {
            ClosedBox.SetActive(true);
            OpenBoxBG.SetActive(false);
            OpenBoxFG.SetActive(false);
            OpenBoxTop.gameObject.SetActive(false);
        }

    }
}
