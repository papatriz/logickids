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

namespace KidGame.Lessons.Jelly01
{
    public class Jelly : MonoBehaviour, IPointerDownHandler
    {
        public event Action<Jelly> ObjectChoosed;

        public Sprite[] Sprites;

        private JellyType Type;
        private bool Interactable = false;

        private Image Image;
        private RectTransform RectTransform;

        private ISoundManager SoundManager;
        private ITimers Timers;

        private void Awake()
        {
            SoundManager = CompositionRoot.GetSoundManager();
            Timers = CompositionRoot.GetTimers();

            Image = GetComponent<Image>();
            RectTransform = GetComponent<RectTransform>();

            this.gameObject.SetActive(false);

        }

        public void Setup(JellyType type)
        {
            Sprite sprite;

            var spriteIndex = type.ID;

            sprite = Sprites[spriteIndex];

            Image.sprite = sprite;
            RectTransform.sizeDelta = sprite.rect.size;

            Type = type;

        }

        public JellyType GetJellyType()
        {
            return Type;
        }

        public IPromise Appear()
        {
            var deferred = new Deferred();
            var duration = 0.6f;

            transform.localScale = 0.01f * Vector3.one;
            SetAlpha(Image, 1f);
            transform.localEulerAngles = Vector3.zero;

            gameObject.SetActive(true);

            SoundManager.Play(Sounds.ECommon.WaterDrop);

            transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(duration)
                 .SetEndValue(Vector2.one)
                 .SetEasing(TweenEasingFunctions.Bounce) //EaseOutBack
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
                        .SetDuration(duration*2)
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
            var duration = 0.7f;

            Debug.Log("Animating right choice");
            SoundManager.Play(Sounds.ECommon.Pop1);

            var alphaQuestTween = new Tween<float>();

            alphaQuestTween.SetAutoCleanup(true)
                .SetStartValue(1f)
                .SetEndValue(0f)
                .SetDuration(duration-0.1f)
                .OnExecute(current =>
                {
                    SetAlpha(Image, current);
                })
                .Play();

            transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(duration)
                 .SetEndValue(2.0f * Vector2.one) // 2.0f
                 .SetEasing(TweenEasingFunctions.Punch)  //Punch - EaseOutCubic
                 .OnCompleted((t) =>
                 {
                     deferred.Resolve();
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
