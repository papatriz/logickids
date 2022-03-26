using System;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DaikonForge.Tween;
using KidGame.UI;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Memory02
{
    public class Chest : MonoBehaviour, IPointerDownHandler
    {
        public enum AppearDirection
        {
            Up,
            Right,
            Left
        }

        public event Action<EGems> CardChoosed;

        public AppearDirection Direction;

        public GameObject[] Objects;

        public Image ChestFG;
        public Image ChestBG;
        public GameObject ChestTopClosed;

        public Sprite[] ChestFGSprites;
        public Sprite[] ChestBGSprites;

        private RectTransform ChestFGRect;
        private RectTransform ChestBGRect;

        private Vector3 OnScreenPosition;
        private Vector3 OutScreenPosition;

        private const int TopShift = 270;

        private ISoundManager SoundManager;
        private ITimers Timers;

        private EGems CurrentFruit; //toDo: better make it via enum, but.. to think about
        private bool Interactable = false;

        private void Awake()
        {
            SoundManager = CompositionRoot.GetSoundManager();
            Timers = CompositionRoot.GetTimers();

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

            ChestBGRect = ChestBG.GetComponent<RectTransform>();
            ChestFGRect = ChestFG.GetComponent<RectTransform>();

            Interactable = false;
        }

        public void SetObject(EGems fruit)
        {
            CurrentFruit = fruit;

            for (int i = 0; i < Objects.Length; i++)
            {
                Objects[i].SetActive(false);
            }

            Objects[(int)CurrentFruit].SetActive(true);
        }

        public EGems GetObject()
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

            ChestTopClosed.SetActive(true);
            ChestBG.gameObject.SetActive(false);
            ChestFG.gameObject.SetActive(false);

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

        public IPromise Close(bool closing = true)
        {
            var deferred = new Deferred();

            var frameTime = 0.05f;

            var frame = closing ? 0 : 4;
            var iterator = closing ? 1 : -1;


            var complete = Timers.Wait(frameTime)
                .Done(() =>
                {
                    if (!closing)
                    {
                        ChestTopClosed.SetActive(false);
                        ChestBG.gameObject.SetActive(true);
                        ChestFG.gameObject.SetActive(true);

                        Debug.Log("Activate FG & BG");
                    }
                    ChangeFrame(frame);
                    frame += iterator;
                })
                .Then(() => Timers.Wait(frameTime))
                .Done(() =>
                {
                    ChangeFrame(frame);
                    frame += iterator;
                })
                .Then(() => Timers.Wait(frameTime))
                .Done(() =>
                {
                    ChangeFrame(frame);
                    frame += iterator;
                })
                .Then(() => Timers.Wait(frameTime))
                .Done(() =>
                {
                    ChangeFrame(frame);
                    frame += iterator;
                })
                .Then(() => Timers.Wait(frameTime))
                .Done(() =>
                {
                    ChangeFrame(frame);
                    frame += iterator;
                });

            if (closing)
            {
                complete = complete.
                    Then(() => Timers.Wait(frameTime))
                     .Done(() =>
                     {
                             ChestTopClosed.SetActive(true);
                             ChestBG.gameObject.SetActive(false);
                             ChestFG.gameObject.SetActive(false);
                     });
            }



            return complete;
        }

        private void ChangeFrame(int n)
        {

            ChestBG.sprite = ChestBGSprites[n];
            ChestBGRect.sizeDelta = ChestBGSprites[n].rect.size;
            var yBGShift = ChestBGSprites[n].rect.size.y;

           // Debug.Log("Y = " + yShift);

            ChestFG.sprite = ChestFGSprites[n];
            ChestFGRect.sizeDelta = ChestFGSprites[n].rect.size;
           // var yFGShift = ChestFGSprites[n].rect.size.y / 2f;

            var yShift = -134.5f + yBGShift;

            ChestFG.transform.localPosition = new Vector3(0, yShift, 0);
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