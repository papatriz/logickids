using System;

using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KidGame.Lessons.Memory04
{

    public class Bowl : MonoBehaviour, IPointerDownHandler
    {
        public enum AppearDirection
        {
            Up,
            Down,
            Right,
            Left
        }

        public event Action<EJellies> BowlChoosed;

        public AppearDirection Direction;
        public Image BowlFG;

        public GameObject[] Objects;

        private Vector3 OnScreenPosition;
        private Vector3 OutScreenPosition;
    

        private ISoundManager SoundManager;
        private ITimers Timers;

        private EJellies CurrentJelly; //toDo: better make it via enum, but.. to think about
        private bool Interactable = false;

        private void Awake()
        {
            SoundManager = CompositionRoot.GetSoundManager();
            Timers = CompositionRoot.GetTimers();

            OnScreenPosition = this.transform.localPosition;

            var canvasWidth = (int)FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;
            var canvasHeight = (int)FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.height;

            Vector3 shiftDir = Vector3.down;
            float canvasShift = canvasHeight;

            switch (Direction)
            {
                case AppearDirection.Up:
                    shiftDir = Vector3.up;
                    break;

                case AppearDirection.Left:
                    shiftDir = Vector3.left;
                    canvasShift = canvasWidth;
                    break;

                case AppearDirection.Right:
                    shiftDir = Vector3.right;
                    canvasShift = canvasWidth;
                    break;
            }

            var shiftAmount = shiftDir * (canvasShift / 2 + 500);

            OutScreenPosition = OnScreenPosition + shiftAmount;
            transform.localPosition = OutScreenPosition;

            Interactable = false;
        }

        public void SetObject(EJellies fruit)
        {
            CurrentJelly = fruit;

            for (int i = 0; i < Objects.Length; i++)
            {
                Objects[i].SetActive(false);
            }

            Objects[(int)CurrentJelly].SetActive(true);
        }

        public EJellies GetObject()
        {
            return CurrentJelly;
        }

        public void SetInteractable(bool state)
        {
            Interactable = state;
        }

        public IPromise Appear()
        {
            var deferred = new Deferred();
            var duration = 0.6f;

            SetAlpha(BowlFG, 1f);
            SoundManager.Play(Sounds.ECommon.Whoosh);

            transform.TweenPosition(true)
             .SetAutoCleanup(true)
             .SetDuration(duration)
             .SetStartValue(OutScreenPosition)
             .SetEndValue(OnScreenPosition)
             .SetEasing(TweenEasingFunctions.EaseOutCubic)
             .OnCompleted((t) =>
             {
                 Timers.Wait(0.3f)
                 .Done(() =>
                 {
                     deferred.Resolve();

                 });

             })
             .Play();

            return deferred.Then(() => Close(false));
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
             .SetEasing(TweenEasingFunctions.EaseOutCubic)
             .OnCompleted((t) =>
             {
                 deferred.Resolve();
             })
             .Play();

            return deferred;
        }

        public IPromise Close(bool close = true)
        {
            var deferred = new Deferred();

            float duration = 0.4f;
            float startAlpha = close ? 0.37f : 1f;
            float finishAlpha = close ? 1f : 0.37f;

            var alphaTween = new Tween<float>();

            alphaTween.SetAutoCleanup(true)
                .SetStartValue(startAlpha)
                .SetEndValue(finishAlpha)
                .SetDuration(duration)
                .OnExecute(current =>
                {
                    SetAlpha(BowlFG, current);
                })
                .OnCompleted((t) =>
                {
                    deferred.Resolve();
                })
                .Play();

            return deferred;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Interactable)
            {
                Close(false);
                BowlChoosed(GetObject());
            }
        }

        private void SetAlpha(Image image, float alpha = 0f)
        {
            var tmp = image.color;
            tmp.a = alpha;
            image.color = tmp;
        }
    }
}
