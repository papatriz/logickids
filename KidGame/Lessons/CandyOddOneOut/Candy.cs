using System;
using System.Collections.Generic;
using System.Linq;
using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.CandyOddOneOut
{
    public class Candy : MonoBehaviour, IPointerDownHandler
    {
        public event Action<Candy> ObjectChoosed;

        public Sprite[] Sprites;

        private CandyType Type;
        private bool Interactable = false;

        private Image Image;
        private RectTransform RectTransform;
        private Vector2 OnScreenPosition;

        private ISoundManager SoundManager;
        private ITimers Timers;

        private void Awake()
        {
            SoundManager = CompositionRoot.GetSoundManager();
            Timers = CompositionRoot.GetTimers();

            Image = GetComponent<Image>();
            RectTransform = GetComponent<RectTransform>();
            OnScreenPosition = this.transform.localPosition;

            this.gameObject.SetActive(false);

        }

        public void Setup(CandyType type)
        {
            Sprite sprite;

            var spriteIndex = type.ID;

            sprite = Sprites[spriteIndex];

            Image.sprite = sprite;
            RectTransform.sizeDelta = sprite.rect.size;

            Type = type;

        }

        public CandyType GetCandyType()
        {
            return Type;
        }

        public IPromise Appear()
        {
            var deferred = new Deferred();
            var duration = 0.6f;

            var place = OnScreenPosition;

            var startY = place.y + 800;
            var startPos = new Vector2(place.x, startY);

            transform.localPosition = startPos;
            transform.localScale = Vector3.one;

            var rotationZ = Random.Range(-20f, 20f);
            var rotation = new Vector3(0, 0, rotationZ);

            transform.localEulerAngles = rotation;
            SetAlpha(Image, 1f);
            Image.fillAmount = 1f;

            this.gameObject.SetActive(true);

            Timers.Wait(0.17f)
                .Done(() => SoundManager.Play(Sounds.ECommon.FruitImpact));

            transform.TweenPosition(true)
             .SetAutoCleanup(true)
             .SetDuration(duration)
             .SetStartValue(startPos)
             .SetEndValue(place)
                 .SetEasing(TweenEasingFunctions.Bounce)
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

            var duration = 0.6f;

            SoundManager.Play(Sounds.ECommon.Whoosh);

            transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(duration)
                 .SetEndValue(0.01f * Vector2.one)
                 .SetEasing(TweenEasingFunctions.EaseOutCubic)
                 .OnCompleted((t) =>
                 {
                     gameObject.SetActive(false);
                     deferred.Resolve();
                 })
                 .Play();

            return deferred;
        }

        public IPromise AnimateWrongChoice()
        {
            var deferred = new Deferred();
            var duration = 0.1f;

            var startRot = new Vector3(0, 0, 40);
            var endRot = new Vector3(0, 0, -40);

            var returnTween = transform.TweenRotation(true)
                        .SetAutoCleanup(true)
                        .SetDuration(duration)
                        .SetEasing(TweenEasingFunctions.EaseInCubic)
                        .SetStartValue(endRot)
                        .SetEndValue(Vector3.zero)
                        .OnCompleted((t) =>
                        {
                            deferred.Resolve();
                        });

            var mainTween = transform.TweenRotation(true)
                        .SetAutoCleanup(true)
                        .SetDuration(duration * 2)
                        .SetStartValue(startRot)
                        .SetEndValue(endRot)
                        .OnCompleted((t) =>
                        {
                            returnTween.Play();
                        });

            var startTween = transform.TweenRotation(true)
            .SetAutoCleanup(true)
            .SetDuration(duration)
            .SetEasing(TweenEasingFunctions.EaseOutCubic)
            .SetEndValue(startRot)
            .OnCompleted((t) =>
            {
                mainTween.Play();
            })
            .Play();

            return deferred;
        }

        public IPromise AnimateRightChoice()
        {
            var deferred = new Deferred();
            var duration = 0.25f; //0.7f

            var tweenStart = new Tween<float>();
            var tweenMid = new Tween<float>();
            var tweenEnd = new Tween<float>();

            tweenEnd.SetStartValue(0.33f)
                .SetAutoCleanup(true)
                .SetEndValue(0)
                .SetDuration(duration)
                .SetEasing(TweenEasingFunctions.EaseInQuart)
                .OnExecute(current =>
                {
                    Image.fillAmount = current;
                })
                .OnCompleted(t =>
                {
                    deferred.Resolve();
                });

            tweenMid.SetStartValue(0.67f)
                .SetEndValue(0.33f)
                .SetAutoCleanup(true)
                .SetDuration(duration)
                .SetEasing(TweenEasingFunctions.EaseInQuart)
                .OnExecute(current =>
                {
                    Image.fillAmount = current;
                })
                .OnCompleted(t =>
                {
                    SoundManager.Play(Sounds.ECommon.AM_Short);
                    tweenEnd.Play();
                });

            tweenStart.SetStartValue(1f)
                .SetEndValue(0.67f)
                .SetAutoCleanup(true)
                .SetDuration(duration)
                .SetEasing(TweenEasingFunctions.EaseInQuart)
                .OnExecute(current =>
                {
                    Image.fillAmount = current;
                })
                .OnCompleted(t =>
                {
                    SoundManager.Play(Sounds.ECommon.AM_Short);
                    tweenMid.Play();
                })
                .Play();

            return deferred;
        }

        public void SetInteractable(bool state)
        {
            Interactable = state;
        }

        private void SetAlpha(Image image, float alpha = 0f)
        {
            var tmp = image.color;
            tmp.a = alpha;
            image.color = tmp;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Interactable)
            {
                ObjectChoosed(this);
            }
        }
    }
}
