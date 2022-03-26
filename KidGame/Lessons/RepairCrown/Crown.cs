
using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;

namespace KidGame.Lessons.RepairCrown
{
    public class Crown : MonoBehaviour
    {
        public GameObject[] GemPlaceHolders;

        private EGemType CurrentGemType = EGemType.Cone;

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
        }

        public int GetTargetsCount()
        {
            return GemPlaceHolders.Length;
        }

        public IPromise Appear(EGemType gemType)
        {
            var deferred = new Deferred();
            var duration = 0.6f;

            SetGemCanvas(gemType);

            SoundManager.Play(Sounds.ECommon.Whoosh);

            transform.TweenPosition(true)
             .SetAutoCleanup(true)
             .SetDuration(duration)
             .SetStartValue(OutScreenPosition)
             .SetEndValue(OnScreenPosition)
                 .SetEasing(TweenEasingFunctions.EaseOutCubic)
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

        public EGemType GetCurrentType()
        {
            return CurrentGemType;
        }

        public Collider2D GetTargetCollider()
        {
            return GemPlaceHolders[(int)CurrentGemType].GetComponent<Collider2D>();
        }

        private void SetGemCanvas(EGemType gemType)
        {
            for (int i = 0; i < GemPlaceHolders.Length; i++)
            {
                GemPlaceHolders[i].SetActive(false);
            }

            GemPlaceHolders[(int)gemType].SetActive(true);
            CurrentGemType = gemType;
        }
    }
}