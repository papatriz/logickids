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

namespace KidGame.Lessons.GemOddOneOut
{
    public class Gem : MonoBehaviour, IPointerDownHandler
    {
        public event Action<Gem> ObjectChoosed;

        public Sprite[] Sprites;

        private GemType Type;
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

        public void Setup(GemType type)
        {
            Sprite sprite;

            var spriteIndex = type.ID;

            sprite = Sprites[spriteIndex];

            Image.sprite = sprite;
            RectTransform.sizeDelta = sprite.rect.size;

            Type = type;

        }

        public GemType GetJellyType()
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

            Timers.Wait(0.17f)
                .Done(() => SoundManager.Play(Sounds.ECommon.Tink2Times));

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
            var duration = 0.65f; //0.7f

           // SoundManager.Play(Sounds.ECommon.GlassBreak);

            var alphaQuestTween = new Tween<float>();

            alphaQuestTween.SetAutoCleanup(true)
                .SetStartValue(1f)
                .SetEndValue(0f)
                .SetEasing(TweenEasingFunctions.EaseInCubic)
                .SetDuration(duration - 0.05f)
                .OnExecute(current =>
                {
                    SetAlpha(Image, current);
                })
                .Play();

            transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(duration)
                 .SetEndValue(3.5f * Vector2.one) // 2.0f
                 .SetEasing(TweenEasingFunctions.EaseInBack)  //Punch - EaseOutCubic
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
