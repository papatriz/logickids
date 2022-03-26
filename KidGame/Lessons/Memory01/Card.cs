using System;

using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KidGame.Lessons.Memory01
{
   
    public class Card : MonoBehaviour, IPointerDownHandler
    {
        public enum AppearDirection
        {
            Up,
            Right,
            Left
        }

        public event Action<EFruits> CardChoosed; 

        public AppearDirection Direction;

        public GameObject[] Objects;
        public GameObject Hider;

        private Vector3 TopClosedPosition;
        private Vector3 TopOpenPosition;
        private Vector3 OnScreenPosition;
        private Vector3 OutScreenPosition;

        private const int TopShift = 270; 

        private ISoundManager SoundManager;
        private ITimers Timers;

        private EFruits CurrentFruit; //toDo: better make it via enum, but.. to think about
        private bool Interactable = false;

        private void Awake()
        {
            SoundManager = CompositionRoot.GetSoundManager();
            Timers = CompositionRoot.GetTimers();

            TopClosedPosition = Hider.transform.localPosition;
            TopOpenPosition = TopClosedPosition + new Vector3(0, TopShift, 0);

            OnScreenPosition = this.transform.localPosition;

            var canvasWidth = (int)FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;
            var canvasHeight = (int)FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.height;

            var shiftDir = new Vector3(0, 1, 0);
            var shiftAmount = shiftDir * (canvasHeight / 2 + 500);

            if (Direction != AppearDirection.Up)
            {
                shiftDir = (Direction == AppearDirection.Right) ? new Vector3(1, 0, 0) : new Vector3(-1, 0, 0);
                shiftAmount = shiftDir * (canvasWidth / 2 + 500);
            }

            OutScreenPosition = OnScreenPosition + shiftAmount;
            transform.localPosition = OutScreenPosition;

            Interactable = false;
        }

        public void SetObject(EFruits fruit)
        {
            CurrentFruit = fruit;

            for(int i=0; i<Objects.Length; i++)
            {
                Objects[i].SetActive(false);
            }

            Objects[(int)CurrentFruit].SetActive(true);
        }

        public EFruits GetObject()
        {
            return CurrentFruit;
        }

        public void SetInteractable(bool state)
        {
            Interactable = state;
        }

        public IPromise Appear()
        {
            var deferred = new Deferred();

            var duration = 0.6f;

            Hider.transform.localPosition = TopClosedPosition;

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

            var start = !close ? TopClosedPosition : TopOpenPosition;
            var end = !close ? TopOpenPosition : TopClosedPosition;

            var duration = 0.3f;

            Hider.transform.TweenPosition(true)
             .SetAutoCleanup(true)
             .SetDuration(duration)
             .SetStartValue(start)
             .SetEndValue(end)
            // .SetEasing(TweenEasingFunctions.EaseOutCubic)
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
                Debug.Log("Choosed box contain " + GetObject());
                CardChoosed(GetObject());
            }
        }
    }
}
