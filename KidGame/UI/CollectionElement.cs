using System;
using System.Collections;
using System.Collections.Generic;
using DaikonForge.Tween;
using KidGame.Collection;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.UI
{
    public class CollectionElement : MonoBehaviour
    {
        public GameObject ThumbActive;
        public GameObject ThumbLocked;
        public CollectionBigImage BigImage;
        public Image ThumbBG;

        public ECollection Item;

        private Transform BigImageDefaultRoot;

        private string StatusKey;

        private Color32 BGActiveColor = new Color32(240, 240, 240, 255); //new Color32(250, 241, 181, 255); // toDO: подобрать цвета
        private Color32 BGLockedColor = Color.grey;

        private IPlayerPrefs PlayerPrefs;

        private TweenBase HighlightUnlockedItem;

        private void Awake()
        {
            PlayerPrefs = CompositionRoot.GetPlayerPrefs();
            StatusKey = Item.ToString();

            TweenEasingCallback easeUp = TweenEasingFunctions.EaseOutCirc;
            TweenEasingCallback easeDown = TweenEasingFunctions.EaseInCirc;
            TweenEasingCallback currentHighlightEase = easeUp;
            bool moveHighlightDown = false;

            HighlightUnlockedItem = transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(0.6f)
                 .SetStartValue(Vector2.one)
                 .SetEndValue(1.2f * Vector2.one)
                 .SetLoopType(TweenLoopType.Pingpong)
                 .SetLoopCount(0)
                 .SetEasing(currentHighlightEase)
                 .OnLoopCompleted((t) =>
                 {
                     currentHighlightEase = moveHighlightDown? easeUp : easeDown;
                     moveHighlightDown = !moveHighlightDown;
                 });
        }

        private bool IsUnlocked
        {
            get
            {
                int defaultVal = Convert.ToInt32(false);
                int value = PlayerPrefs.GetInt(StatusKey, defaultVal);
                bool isUnlocked = Convert.ToBoolean(value);

                return isUnlocked;
            }
            set
            {
                int intValue = Convert.ToInt32(value);
                PlayerPrefs.SetInt(StatusKey, intValue);
                PlayerPrefs.Save();
            }
        }

        public bool Unlocked()
        {
            return IsUnlocked;
        }

        public void SetAvailability(bool value) // Можно сделать просто анлок, так как пока не вижу сценариев, когда нужно было бы лочить разлоченный айтем
        {
            IsUnlocked = value;
        }

        public void SetupThumb()
        {
            ThumbBG.color = (IsUnlocked) ? BGActiveColor : BGLockedColor;
            ThumbActive.SetActive(IsUnlocked);
            ThumbLocked.SetActive(!IsUnlocked);
        }

        public IPromise AnimateUnlocking()
        {
            var deferred = new Deferred();
            var duration = 1f;

            ThumbActive.SetActive(true);

            var tweenScale = transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(0.2f)
                 .SetStartValue(Vector2.one)
                 .SetEndValue(1.5f * Vector2.one)
                 .SetLoopType(TweenLoopType.Pingpong)
                 .SetLoopCount(2)
                 .SetEasing(TweenEasingFunctions.EaseInOutCubic)
                 .OnLoopCompleted((t) =>
                 {
                     ThumbBG.color = BGActiveColor;
                 })
                 .OnCompleted((t) =>
                 {
                     deferred.Resolve();
                 });

            var tween = new Tween<float>();

            tween.SetStartValue(1f)
                .SetEndValue(0)
                .SetDuration(duration)
                .OnExecute(current =>
                {
                    ThumbLocked.GetComponent<Image>().fillAmount = current;
                })
                .OnCompleted(t =>
                {
                    tweenScale.Play();
                })
                .Play();

            return deferred;
        }

        public void HighlightThumb()
        {
            HighlightUnlockedItem.Play();
        }

        public void StopHighlighting()
        {
            HighlightUnlockedItem.Stop();
            transform.localScale = Vector3.one;
        }

        public void HighlightLockedItem()
        {
            TweenEasingCallback easeUp = TweenEasingFunctions.Bounce;
            TweenEasingCallback easeDown = TweenEasingFunctions.Bounce;

            float angle = 10f;
            float time = 0.1f;

            var animateLockedFinish = transform.TweenRotation()
                 .SetAutoCleanup(true)
                 .SetDuration(time*2)
                 .SetStartValue(new Vector3(0, 0, -angle/2))
                 .SetEndValue(Vector3.zero)
                 .SetEasing(easeDown);

            var animateLockedMainLoop = transform.TweenRotation(true)
                 .SetAutoCleanup(true)
                 .SetDuration(2 * time)
                 .SetStartValue(new Vector3(0, 0, angle))
                 .SetEndValue(new Vector3(0, 0, -angle/2))
                 //.SetEasing(currentLockedEase)
                 //.OnExecute((t) =>
                 //{
                 //    transform.localEulerAngles = t;
                 //    currentLockedEase = (t.z < 0) ? easeUp : easeDown;
                 //})
                 .OnCompleted((t) =>
                 {
                     animateLockedFinish.Play();
                 });

            var animateLockedItemStart = transform.TweenRotation()
                 .SetAutoCleanup(true)
                 .SetDuration(time)
                 .SetStartValue(Vector3.zero)
                 .SetEndValue(new Vector3(0, 0, angle))
                 .SetEasing(easeUp)
                 .OnCompleted((t) =>
                 {
                     Debug.Log("ANIMATE START PART FINISHED");
                     animateLockedMainLoop.Play();
                 })
                 .Play();
        }

        public IPromise ShowBigImage(Transform root)
        {
            GetComponent<Button>().enabled = false;
            BigImageDefaultRoot = BigImage.transform.parent;

            BigImage.GetComponent<Image>().color = BGActiveColor;

            ThumbActive.SetActive(false);
            ThumbBG.gameObject.SetActive(false);

            return BigImage.Show(root)
                .Then(BigImage.WaitForClick)
                .Then(() => BigImage.Hide(BigImageDefaultRoot))
                .Done(() =>
                {
                    ThumbActive.SetActive(true);
                    ThumbBG.gameObject.SetActive(true);
                    GetComponent<Button>().enabled = true;
                });

        }
        

    }
}
