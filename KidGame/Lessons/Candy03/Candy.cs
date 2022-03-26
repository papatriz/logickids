using System;
using DaikonForge.Tween;
using KidGame.UI;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Candy03
{
    public class Candy : CommonDrag
    {
        public Sprite[] Sprites;

        private Image Image;
        private RectTransform RectTransform;
        private bool Placed;

        private ECandyForm Form;

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

        public void SetForm(ECandyForm form)
        {
            Form = form;
        }

        public ECandyForm GetForm()
        {
            return Form;
        }

        public void Setup(ECandies candy, ECandyForm form, ECandyShape shape)
        {
            Sprite sprite;

            var enumInt = Convert.ToInt32(candy);

            sprite = Sprites[enumInt];

            Image.sprite = sprite;
            RectTransform.sizeDelta = sprite.rect.size;

            SetType(shape);
            SetForm(form);
        }

        public IPromise Appear()
        {
            var deferred = new Deferred();
            var duration = 0.8f;

            var place = GetStartPosition();

            var startY = place.y + 800;
            var startPos = new Vector2(place.x, startY);

            transform.localPosition = startPos;

            var rotationZ = Random.Range(-10f, 10f);
            var rotation = new Vector3(0, 0, rotationZ);

            transform.localEulerAngles = rotation;

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

                 SetStartPosition(place);
                 deferred.Resolve();
             })
             .Play();

            return deferred;
        }

        public IPromise DisAppear()
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
    }
}