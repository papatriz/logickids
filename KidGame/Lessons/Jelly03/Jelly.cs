using System;
using DaikonForge.Tween;
using KidGame.UI;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Jelly03
{
    public class Jelly : CommonDrag
    {
        public Sprite[] Sprites; // Order have to be: RedForm1,RedForm2..RedForm4, GreenForm1.. etc

        private Image Image;
        private RectTransform RectTransform;
        private bool Placed;

        private ISoundManager SoundManager;
        private ITimers Timers;

        protected override void Awake()
        {
            base.Awake();

            SoundManager = CompositionRoot.GetSoundManager();
            Timers = CompositionRoot.GetTimers();

            Image = GetComponent<Image>();
            RectTransform = GetComponent<RectTransform>();
            Placed = false;

        }

        public bool IsPlaced()
        {
            return Placed;
        }

        public void SetPlaced(bool isPlaced)
        {
            Placed = isPlaced;
        }

        public EColors GetMatchType()
        {
            return (EColors)MatchType;
        }

        private void Setup()
        {
            var colorIndex = Convert.ToInt32(MatchType);
            var formIndex = Random.Range(0, 3);
            var sprite = Sprites[colorIndex * 4 + formIndex];
            if (Image == null) Image = GetComponent<Image>();
            Image.sprite = sprite;
            RectTransform.sizeDelta = sprite.rect.size;

            transform.localPosition = GetStartPosition();
        }

        public IPromise Appear()
        {
            var deferred = new Deferred();
            var duration = 0.8f;

            Setup();

            transform.localScale = 0.01f * Vector3.one;
            SetAlpha(Image, 1f);

            var rotationZ = Random.Range(-15f, 15f);
            var rotation = new Vector3(0, 0, rotationZ);

            transform.localEulerAngles = rotation;

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

        public IPromise DisAppear() //toDo: maybe use scale based disappear animation
        {
            var deferred = new Deferred();

            var duration = Random.Range(0.35f, 0.55f);
            var place = GetStartPosition();

            var targetY = place.y - 1200;
            var targerPos = new Vector2(place.x, targetY);

            transform.TweenPosition(true)
             .SetAutoCleanup(true)
             .SetDuration(duration)
             .SetEndValue(targerPos)
                 .SetEasing(TweenEasingFunctions.EaseInCubic)
             .OnCompleted((t) =>
             {
                 this.gameObject.SetActive(false);
                 deferred.Resolve();
             })
             .Play();

            return deferred;
        }

        private void SetAlpha(Image image, float alpha = 0f)
        {
            var tmp = image.color;
            tmp.a = alpha;
            image.color = tmp;
        }
    }
}