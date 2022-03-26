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

namespace KidGame.Lessons.FruitOddOneOut
{
    public class Fruit : MonoBehaviour, IPointerDownHandler
    {
        public event Action<Fruit> ObjectChoosed;

        public Sprite[] Sprites;

        private FruitType Type;
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

        public void Setup(FruitType type)
        {
            Sprite sprite;

            var spriteIndex = type.ID;

            sprite = Sprites[spriteIndex];

            Image.sprite = sprite;
            RectTransform.sizeDelta = sprite.rect.size;

            Type = type;

        }

        public FruitType GetJellyType()
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

            var rotationZ = Random.Range(-10f, 10f);
            var rotation = new Vector3(0, 0, rotationZ);

            transform.localEulerAngles = rotation;
            SetAlpha(Image, 1f);

            this.gameObject.SetActive(true);

            Timers.Wait(0.27f)
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

            SoundManager.Play(Sounds.ECommon.UhUh);

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
            var durationUp = 0.25f; //0.7f
            var durationDown = 0.25f; //0.7f

            Timers.Wait(durationUp)
                .Done(() => SoundManager.Play(Sounds.ECommon.AM_Short));

            var alphaQuestTween = new Tween<float>();

            alphaQuestTween.SetAutoCleanup(true)
                .SetDelay(durationUp)
                .SetStartValue(1f)
                .SetEndValue(0f)
                .SetEasing(TweenEasingFunctions.EaseInExpo)
                .SetDuration(durationDown)
                .OnExecute(current =>
                {
                    SetAlpha(Image, current);
                })
                .Play();

            var scaleDown = transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(durationDown)
                 .SetStartValue(2.5f * Vector2.one)
                 .SetEndValue(0.05f * Vector2.one)
                 .SetEasing(TweenEasingFunctions.EaseInCirc)
                 .OnCompleted((t) =>
                 {
                     deferred.Resolve();
                 });

            transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(durationUp)
                 .SetEndValue(2.5f * Vector2.one) // 2.0f
                 .SetEasing(TweenEasingFunctions.EaseOutCirc)  //Punch - EaseOutCubic
                 .OnCompleted((t) =>
                 {
                     scaleDown.Play();
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
