using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;


namespace KidGame.UI
{
    public class WinDialog : ScalableMessageBox
    {
        public StarChest Chest;
        public Image[] Stars;

        public Sprite On;
        public Sprite Off;

        public Transform[] StarTopFlyPositions;

        public Transform DefaultStarsRoot;

        private Vector3[] StarInitPositions = new Vector3[5];
        

        private IGame Game;
        private static ITimers Timers;
        private static ISoundManager SoundManager;

        private void Awake()
        {
            Game = CompositionRoot.GetGame();
            Timers = CompositionRoot.GetTimers();
            SoundManager = CompositionRoot.GetSoundManager();
            

            for (int i=0; i<StarTopFlyPositions.Length; i++)
            {
                StarTopFlyPositions[i].gameObject.SetActive(false);
                StarInitPositions[i] = Stars[i].transform.localPosition;

            }
 
          //  ResetToInitial();
        }

        public void ResetToInitial()
        {
            Chest.StopAnimation();
            DefaultStarsRoot.gameObject.SetActive(true);

            Chest.SwitchState(true);
            Chest.ShowPreviouslyEarnedStars();

            var i = 0;

            foreach (Image icon in Stars)
            {
                icon.sprite = Off;
                icon.transform.parent = DefaultStarsRoot;
                icon.transform.localPosition = StarInitPositions[i];
                icon.gameObject.SetActive(true);
                i++;
            }
        }

        public void CloseAndAnimateChest()
        {
            Chest.SwitchState(false);
            SoundManager.Play(Sounds.ECommon.ChestSlam);

            Chest.AnimateClosed();
        }

        public void PrepateForTransit()
        {
            DefaultStarsRoot.gameObject.SetActive(false);
            Chest.StopSound();
        }

        public IPromise WaitForClose()
        {
            var timeDeferred = Timers.Wait(1f); //3f
            var clickDeferred = this.WaitForClick();

            return Deferred.Race(timeDeferred, clickDeferred);
        }

        public IPromise SetStars(int amount)
        {
           // SetProgressInstant();

            return ShowStars(amount)
                .Then(() => FlashStars(amount))
                .Then(() => MoveStarsToChest(amount));
        }

        private IPromise ShowStars(int amount)
        {
            var deferred = new Deferred();

            deferred = (Deferred)Timers.Wait(1f); // todo: сделать вместо задержки анимацию на первую звезду типа её колбасит

            for (int i = 0; i < amount; i++)
            {
                var j = i;
                var star = Stars[j];

                deferred = (Deferred)deferred.Then(() => ActivateStar(star));
            }

            return deferred;
        }

        private IPromise MoveStarsToChest(int amount)
        {
            var deferred = new Deferred();

            if(amount < Stars.Length)
            {
                for (int i = amount; i<Stars.Length; i++)
                {
                    Stars[i].gameObject.SetActive(false);
                }
            }
            
            
            for (int i = 0; i < amount; i++)
            {
                var j = i;
                var star = Stars[j].transform;

                deferred = (Deferred)MoveStarToChest(star, j);
            }

            return deferred;
        }

        private IPromise MoveStarToChest(Transform star, int num)
        {
            var deferred = new Deferred();
            var durationUp = 0.6f; // 0.5f;
            var durationDown = 0.6f; //1f
            var inChestPos = Chest.GetStarPosition(num);

            var scaleToChest = star.transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(durationDown - 0.05f)
                 .SetStartValue(Vector2.one)
                 .SetEndValue(Chest.GetStarScale(num));

            var moveToChest = star.transform.TweenPosition(true)
                 .SetAutoCleanup(true)
                 .SetDuration(durationDown)
                 .SetStartValue(StarTopFlyPositions[num].localPosition)
                 .SetEndValue(inChestPos)
                 .SetEasing(TweenEasingFunctions.EaseInOutSine)
                 .OnCompleted((t) =>
                 {
                     
                     deferred.Resolve();
                 });

            star.transform.TweenPosition(true)
                 .SetAutoCleanup(true)
                 .SetDuration(durationUp)
                 .SetStartValue(StarInitPositions[num])
                 .SetEndValue(StarTopFlyPositions[num].localPosition)
                 .SetEasing(TweenEasingFunctions.EaseInOutSine)
                 .OnCompleted((t) =>
                 {
                     star.SetParent(Chest.GetStarRoot());
                     star.SetAsFirstSibling();
                     moveToChest.Play();
                     scaleToChest.Play();
                     var soundShift = Random.Range(0, 0.15f);
                     Timers.Wait(0.4f + soundShift)
                     .Done(() => SoundManager.Play(Sounds.ECommon.PutIceInGlass));
                     

                 })
                 .Play();

            return deferred;
        }

        private IPromise FlashStars(int amount)
        {
            var deferred = new Deferred();

            var duration = 0.2f;
            var factor = 1.6f;

            for (int i = 0; i < amount; i++)
            {
                var j = i;
                var star = Stars[j];

                star.transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(duration)
                 .SetStartValue(Vector2.one)
                 .SetEndValue(factor * Vector2.one)
                 .SetLoopType(TweenLoopType.Pingpong)
                 .SetLoopCount(6)
                 .SetEasing(TweenEasingFunctions.EaseInOutCubic)
                 .OnCompleted((t) =>
                 {
                    if(j == amount-1)   deferred.Resolve();
                 })
                 .Play();
            }

            return deferred;
        }


        private IPromise ActivateStar(Image star)
        {
            var deferred = new Deferred();

            var duration = 0.2f; //0.3
            var factor = 2f;

            SoundManager.Play(Sounds.ECommon.GetReward);

            star.transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(duration)
                 .SetStartValue(Vector2.one)
                 .SetEndValue(factor * Vector2.one)
                 .SetLoopType(TweenLoopType.Pingpong)
                 .SetLoopCount(2)
                 .SetEasing(TweenEasingFunctions.EaseOutCubic)
                 .OnLoopCompleted((t) =>
                 {
                     star.sprite = On;
                 })
                 .OnCompleted((t) =>
                 {
                     deferred.Resolve();
                 })
                 .Play();

            return deferred;
        }


    }
}