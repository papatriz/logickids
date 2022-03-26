using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.Lessons.Fruit01
{
    public class Kid : MonoBehaviour
    {
        public QuestBubble QuestBubble;

        public Sprite[] KidSprite;

        public Image LeftHand;
        public Image RightHand;

        public Sprite BlackHand;
        public Sprite WhiteHand;

        private int KidCount=0;

        private Collider2D Collider;
        private EFruitType Type;

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

            Collider = GetComponent<Collider2D>();

            LeftHand.gameObject.SetActive(false);
            RightHand.gameObject.SetActive(false);
        }

        private void SetupTarget(EFruitType fruitType)
        {
            Type = fruitType;
        }

        public Collider2D GetTargetCollider()
        {
            return Collider;
        }

        public Transform GetTargetForTutorial()
        {
            return QuestBubble.GetTargetForTutorial();
        }

        public EFruitType GetCurrentType()
        {
            return Type;
        }

        public IPromise PutObjectIntoTarget(Fruit fruit)
        {
            var deferred = new Deferred();
            var duration = 0.3f;

            var newParent = this.transform;

            SoundManager.Play(Sounds.ECommon.FruitToHand);

            fruit.transform.SetParent(newParent, true);
            fruit.transform.SetAsLastSibling();

            var fruitSetting = fruit.GetSettings();

            var start = fruit.transform.localPosition;
            var finish = fruitSetting.MainPos;
            var rotation = new Vector3(0,0,fruitSetting.MainAngle);

            QuestBubble.Hide();

            fruit.transform.TweenRotation(true)
                .SetAutoCleanup(true)
                .SetEndValue(rotation)
                .SetDuration(duration)
                .Play();

            fruit.transform.TweenPosition(true)
               .SetAutoCleanup(true)
               .SetDuration(duration)
               .SetStartValue(start)
               .SetEndValue(finish)
               .SetEasing(TweenEasingFunctions.Bounce)
               .OnCompleted((t) =>
               {
                   ShowHands(fruit);
                   SoundManager.Play(Sounds.ECommon.Woohoo);
                   HoorayJump()
                   .Done(() => deferred.Resolve());
               })
            .Play();

            //toDo: Добавить анимацию радости ребенка (подпрыг, звук йехуу)

            return deferred;
        }

        private IPromise HoorayJump()
        {
            var deferred = new Deferred();
            var duration = 0.3f;

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

        private void ShowHands(Fruit fruit)
        {
            var sprite = (KidCount == 2) ? BlackHand : WhiteHand;
            LeftHand.sprite = sprite;
            RightHand.sprite = sprite;

            var fruitSetting = fruit.GetSettings();

            LeftHand.transform.SetParent(fruit.transform);
            LeftHand.transform.localPosition = fruitSetting.LeftHandPos;
            LeftHand.transform.localEulerAngles = new Vector3(0,0,fruitSetting.LeftHandAngle);

            RightHand.transform.SetParent(fruit.transform);
            RightHand.transform.localPosition = fruitSetting.RightHandPos;
            RightHand.transform.localEulerAngles = new Vector3(0, 0, fruitSetting.RightHandAngle);

            LeftHand.gameObject.SetActive(true);
            RightHand.gameObject.SetActive(true);

            LeftHand.transform.SetParent(this.transform);
            RightHand.transform.SetParent(this.transform);
        }

        public IPromise Appear(EFruitType fruitType)
        {
            SetupTarget(fruitType);

            var kidSprite = KidSprite[KidCount];
            GetComponent<Image>().sprite = kidSprite;
            GetComponent<RectTransform>().sizeDelta = kidSprite.rect.size;

            LeftHand.gameObject.SetActive(false);
            RightHand.gameObject.SetActive(false);

            var deferred = new Deferred();
            var duration = 0.6f;

            SoundManager.Play(Sounds.ECommon.Whoosh);

            transform.TweenPosition(true)
             .SetAutoCleanup(true)
             .SetDuration(duration)
             .SetStartValue(OutScreenPosition)
             .SetEndValue(OnScreenPosition)
             .SetEasing(TweenEasingFunctions.EaseInOutSine)
             .Play();

            var showQuest = Timers.Wait(0.3f)
                .Then(() => QuestBubble.Show(fruitType));

            KidCount++;
            if (KidCount > 2) KidCount = 0;

            return showQuest;
        }


        public IPromise DisAppear()
        {
            var deferred = new Deferred();
            var duration = 0.6f;


            SoundManager.Play(Sounds.ECommon.Whoosh);

            transform.TweenPosition(true)
             .SetAutoCleanup(true)
             .SetDuration(duration)
             .SetStartValue(OnScreenPosition)
             .SetEndValue(OutScreenPosition)
                 .SetEasing(TweenEasingFunctions.EaseInCubic)
             .OnCompleted((t) =>
             {
                 deferred.Resolve();
             })
             .Play();

            return deferred;
        }


    }
}
