using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.Lessons.MakeMedal
{
    public class Medal : MonoBehaviour
    {
        public Image Frame;
        public Image Ribbon;

        public Sprite[] FrameSprites;
        public Sprite[] RibbonSprites;

        private Collider2D Collider;
        private EGemType Type;

        private Vector2 OnScreenPosition;
        private Vector2 OutScreenPosition;

        private int OutScreenShift;

        private ISoundManager SoundManager;

        private void Awake()
        {
            SoundManager = CompositionRoot.GetSoundManager();

            var canvasWidth = (int)FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;

            

            OutScreenShift = -(canvasWidth / 2 + 1024);

            OnScreenPosition = transform.localPosition;
            OutScreenPosition = new Vector2(OnScreenPosition.x + OutScreenShift, OnScreenPosition.y);

            transform.localPosition = OutScreenPosition;

            Collider = Frame.GetComponent<Collider2D>();
        }

        private void SetupTarget(EGemType gemType)
        {
            Debug.Log("Target is " + gemType);

            Type = gemType;

            var gemInt = (int)gemType;
            var form = gemInt % 5;
            var color = Mathf.FloorToInt((float)gemInt / 5);

            var sprite = FrameSprites[form];

            Frame.sprite = sprite;
            Frame.GetComponent<RectTransform>().sizeDelta = sprite.rect.size;

            var ribbonSprite = RibbonSprites[color];
            Ribbon.sprite = ribbonSprite;
        }

        public Transform GetRibbonTransform() // For tutorial needs
        {
            return Ribbon.transform;
        }

        public Collider2D GetTargetCollider()
        {
            return Collider;
        }

        public EGemType GetCurrentType()
        {
            return Type;
        }

        public IPromise PutGemIntoFrame(Gem gem)
        {
            var deferred = new Deferred();
            var duration = 0.3f;

            var start = gem.transform.position;

            var matchedTransform = Collider.transform;
            var newParent = matchedTransform.parent;

            SoundManager.Play(Sounds.ECommon.PutIceInGlass);

            gem.transform.SetParent(newParent, false);
            gem.transform.SetAsLastSibling(); // looks like we need set as last

            var finish = matchedTransform.position;
            var rotation = matchedTransform.eulerAngles;

            gem.transform.TweenRotation(true)
                .SetAutoCleanup(true)
                .SetEndValue(rotation)
                .SetDuration(duration)
                .Play();

            gem.transform.TweenPosition(false)
               .SetAutoCleanup(true)
               .SetDuration(duration)
               .SetStartValue(start)
               .SetEndValue(finish)
               .SetEasing(TweenEasingFunctions.Bounce)
               .OnCompleted((t) =>
               {
                   gem.transform.eulerAngles = matchedTransform.eulerAngles;
                   gem.transform.position = matchedTransform.position;

                   deferred.Resolve();
               })
            .Play();

            return deferred;
        }

        public IPromise Appear(EGemType gemType)
        {
            SetupTarget(gemType);

            var deferred = new Deferred();
            var duration = 0.6f;


            SoundManager.Play(Sounds.ECommon.Whoosh);

            var swingTweenFinal = transform.TweenRotation(true)
                .SetAutoCleanup(true)
                .SetStartValue(new Vector3(0, 0, 5f))
                .SetEndValue(new Vector3(0, 0, 0))
                .SetDuration(0.6f)
                .SetEasing(TweenEasingFunctions.EaseInOutSine);

            var swingTweenForward4 = transform.TweenRotation(true)
                .SetAutoCleanup(true)
                .SetStartValue(new Vector3(0, 0, -10f))
                .SetEndValue(new Vector3(0, 0, 5f))
                .SetDuration(0.6f)
                .SetEasing(TweenEasingFunctions.EaseInOutSine)
                .OnCompleted((t) =>
                {
                    swingTweenFinal.Play();
                }); ;

            var swingTweenBackward7 = transform.TweenRotation(true)
                .SetAutoCleanup(true)
                .SetStartValue(new Vector3(0, 0, 20f))
                .SetEndValue(new Vector3(0, 0, -10f))
                .SetDuration(0.6f)
                .SetEasing(TweenEasingFunctions.EaseInOutSine)
                .OnCompleted((t) =>
                {
                    swingTweenForward4.Play();
                });

            var swingTweenForward15 = transform.TweenRotation(true)
                .SetAutoCleanup(true)
                .SetDelay(0.3f)
                .SetStartValue(new Vector3(0, 0, -20f))
                .SetEndValue(new Vector3(0, 0, 20f))
                .SetDuration(duration)
                .SetEasing(TweenEasingFunctions.EaseInOutSine)
                .OnCompleted((t) =>
                {
                    swingTweenBackward7.Play();
                })
                .Play();

            transform.TweenPosition(true)
             .SetAutoCleanup(true)
             .SetDuration(duration)
             .SetStartValue(OutScreenPosition)
             .SetEndValue(OnScreenPosition)
                 .SetEasing(TweenEasingFunctions.EaseInOutSine)
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
            var duration = 0.8f;


            SoundManager.Play(Sounds.ECommon.Whoosh);

            var swingTween = transform.TweenRotation(true)
                .SetAutoCleanup(true)
                .SetStartValue(Vector3.zero)
                .SetEndValue(new Vector3(0, 0, 20f))
                .SetDuration(0.5f)
                .Play();

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
