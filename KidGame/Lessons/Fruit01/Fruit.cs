using System;
using DaikonForge.Tween;
using KidGame.UI;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Fruit01
{
    public struct FruitSettings
    {
        public float MainAngle;
        public float LeftHandAngle;
        public float RightHandAngle;

        public Vector3 MainPos;
        public Vector3 LeftHandPos;
        public Vector3 RightHandPos;

        public FruitSettings(float mainAngle, float leftAngle, float rightAngle, Vector3 main, Vector3 leftHand, Vector3 rightHand)
        {
            MainAngle = mainAngle;
            LeftHandAngle = leftAngle;
            RightHandAngle = rightAngle;
            MainPos = main;
            LeftHandPos = leftHand;
            RightHandPos = rightHand;
        }
    }

    public class Fruit : CommonDrag
    {
        public Sprite[] Sprites;

        private Image Image;
        private RectTransform RectTransform;
        private bool Placed;

        private FruitSettings[] Settings = new FruitSettings[6];

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

            InitSettings();
        }

        private void InitSettings()
        {
            Settings[(int)EFruitType.Apple] = new FruitSettings(-45f, 120f, 25f, new Vector3(30, -126,0), new Vector3(117, -17,0), new Vector3(-21, -154,0));
            Settings[(int)EFruitType.Grape] = new FruitSettings(-66f, 137f, 95f, new Vector3(5, -142, 0), new Vector3(110, 88, 0), new Vector3(25, -132, 0));
            Settings[(int)EFruitType.Pear] = new FruitSettings(-59f, 154f, 28f, new Vector3(46, -127, 0), new Vector3(90, 25, 0), new Vector3(-4, -185, 0));
            Settings[(int)EFruitType.Lemon] = new FruitSettings(9f, 65f, -9f, new Vector3(31, -145, 0), new Vector3(74, -82, 0), new Vector3(-134, -72, 0));
            Settings[(int)EFruitType.Raspberry] = new FruitSettings(43f, 67f, -42f, new Vector3(1, -154, 0), new Vector3(62, -105, 0), new Vector3(-110, -10, 0));
            Settings[(int)EFruitType.Strawberry] = new FruitSettings(43f, 67f, -54f, new Vector3(1, -154, 0), new Vector3(62, -105, 0), new Vector3(-110, -10, 0));
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
            Sprite sprite;

            var enumInt = Convert.ToInt32(MatchType);

            sprite = Sprites[enumInt];

            Image.sprite = sprite;
            RectTransform.sizeDelta = sprite.rect.size;

        }

        public FruitSettings GetSettings()
        {
            var enumInt = Convert.ToInt32(MatchType);

            return Settings[enumInt];
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
