using System;
using DaikonForge.Tween;
using KidGame.UI;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Fruit03
{
    public class Fruit : CommonDrag
    {
        public Sprite[] Sprites;

        private Image Image;
        private RectTransform RectTransform;
        private bool Placed;

        private Vector2 BigSize = new Vector2(220, 280);
        private Vector2 SmallSize = new Vector2(170, 220);

        private ISoundManager SoundManager;
        private ITimers Timers;

        protected override void Awake()
        {
            base.Awake();

            SoundManager = CompositionRoot.GetSoundManager();
            Timers = CompositionRoot.GetTimers();

            RectTransform = GetComponent<RectTransform>();
            Image = GetComponent<Image>();
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

        private void Setup()
        {
            var enumInt = Convert.ToInt32(MatchType);

            RectTransform.sizeDelta = (enumInt == (int)EFruitType.Big) ? BigSize : SmallSize;

        }

        public void SetSprite(int sprite)
        {
            Image.sprite = Sprites[sprite];
        }

        public IPromise Appear()
        {
            var deferred = new Deferred();
            var duration = 0.8f;

            Setup();

            var place = GetStartPosition();

            var startY = place.y + 800;
            var startPos = new Vector2(place.x, startY);

            transform.localPosition = startPos;

            var rotationZ = Random.Range(-10f, 10f);
            var rotation = new Vector3(0, 0, rotationZ);

            transform.localEulerAngles = rotation;

            this.gameObject.SetActive(true);

            //Timers.Wait(0.27f)
            //    .Done(() => SoundManager.Play(Sounds.ECommon.FruitImpact));

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
            this.gameObject.SetActive(false);

            return new Deferred().Resolve();
        }
    }
}