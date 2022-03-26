using System;
using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.Lessons.MakePendant
{
    public class Pendant : MonoBehaviour
    {
        public Sprite[] FrameSprites;

        public GameObject FrameBig;
        public GameObject FrameSmall;

        public RectTransform Chain;

        private Collider2D BigCollider;
        private Collider2D SmallCollider;

        private RectTransform FrameBigRect;
        private RectTransform FrameSmallRect;

        private Image FrameBigImage;
        private Image FrameSmallImage;

        private const float FrameBigYAdjustment = 6f;
        private const float ChainYAdjustment = 13f;
        private const float FrameSmallYAdjustment = 9f;
        private const float HeartAdjustment = 20f;

        private Vector2 OnScreenPosition;
        private Vector2 OutScreenPosition;

        private int OutScreenShift;

        private ISoundManager SoundManager;
       

        private void Awake()
        {
            SoundManager = CompositionRoot.GetSoundManager();

            FrameBigRect = FrameBig.GetComponent<RectTransform>();
            FrameBigImage = FrameBig.GetComponent<Image>();
            BigCollider = FrameBig.GetComponent<Collider2D>();

            FrameSmallRect = FrameSmall.GetComponent<RectTransform>();
            FrameSmallImage = FrameSmall.GetComponent<Image>();
            SmallCollider = FrameSmall.GetComponent<Collider2D>();

            var canvasWidth = (int)FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;

            

            OutScreenShift = -(canvasWidth / 2 + 1024);

            OnScreenPosition = transform.localPosition;
            OutScreenPosition = new Vector2(OnScreenPosition.x + OutScreenShift, OnScreenPosition.y);

            transform.localPosition = OutScreenPosition;
            transform.localEulerAngles = new Vector3(0, 0, -15f);
        }

        public void GetTargetColliders(out Collider2D big, out Collider2D small)
        {
            big = BigCollider;
            small = SmallCollider;
        }


        public void Setup(EGemType big, EGemType small)
        {
            int num = Enum.GetValues(typeof(EGemType)).Length;
            int smallShift = num / 2;
            int smallSpriteIndex = (int)small - smallShift;

            if ((smallSpriteIndex < 0) || ((int)big >= smallShift)) // for Debug, remove after all things goes right
            {
                Debug.Log("Pendant Setup wrong input: shift=" + smallShift);
                return;
            }

            var spriteBig = FrameSprites[(int)big];
            var spriteSmall = FrameSprites[smallSpriteIndex];

            FrameBigImage.sprite = spriteBig;
            FrameBigRect.sizeDelta = spriteBig.rect.size;

            var frameBigHalfHeight = spriteBig.rect.height / 2; // toDo: change numeric constants to calculated values

            var bigY = 164f - frameBigHalfHeight + FrameBigYAdjustment; // 164f : ChainTop Y - ChainTop half height
            if (big == EGemType.HeartBig) bigY += HeartAdjustment;

            Debug.Log("Big Y: " + bigY);

            FrameBigRect.anchoredPosition = new Vector3(0, bigY, 0);

            var chainY = bigY - frameBigHalfHeight - 40f + ChainYAdjustment; // 40f : Chain half height
            Chain.anchoredPosition = new Vector3(0, chainY, 0);

            FrameSmallImage.sprite = spriteSmall;
            FrameSmallRect.sizeDelta = spriteSmall.rect.size;

            var frameSmallHalfHeight = (spriteSmall.rect.height / 2) * FrameSmallRect.localScale.y;

            var smallY = chainY - frameSmallHalfHeight - 40f + FrameSmallYAdjustment;
            if (small == EGemType.HeartSmall) smallY += HeartAdjustment * FrameSmallRect.localScale.y;

            FrameSmallRect.anchoredPosition = new Vector3(0, smallY, 0);

        }

        public IPromise Appear()
        {
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