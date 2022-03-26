using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.Lessons.Memory04
{
    public class Countdown : MonoBehaviour
    {
        public Sprite[] Hands;

        private Image Hand;

        private ISoundManager SoundManager;
        private ITimers Timers;

        private Vector3 SmallScale = new Vector3(0.7f, 0.7f, 1f);

        private void Awake()
        {
            SoundManager = CompositionRoot.GetSoundManager();
            Timers = CompositionRoot.GetTimers();

            Hand = GetComponent<Image>();

            Hand.sprite = Hands[0];
            transform.localScale = 0.1f * Vector3.one;
            this.gameObject.SetActive(false);
        }


        public IPromise Appear()
        {
            var deferred = new Deferred();
            var duration = 0.3f;

            Debug.Log("INSIDE APPEAR");

            this.gameObject.SetActive(true);

            Hand.sprite = Hands[0];

            transform.TweenScale()
                     .SetAutoCleanup(true)
                     .SetDuration(duration)
                     .SetStartValue(0.1f * Vector3.one)
                     .SetEndValue(Vector3.one)
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
            var duration = 0.3f;

            transform.TweenScale()
                     .SetAutoCleanup(true)
                     .SetDuration(duration)
                     .SetStartValue(Vector3.one)
                     .SetEndValue(0.1f * Vector3.one)
                     .OnCompleted((t) =>
                     {
                         this.gameObject.SetActive(false);
                         deferred.Resolve();
                     })
                     .Play();


            return deferred;
        }

        public IPromise Begin()
        {

            var complete = Timers.Wait(0.7f)
                .Then(() => ChangeStep(1))
                .Then(() => Timers.Wait(0.7f))
                .Then(() => ChangeStep(2))
                .Then(() => Timers.Wait(0.7f))
                .Then(() => ChangeStep(3))
                .Then(() => Timers.Wait(0.7f))
                .Then(() => ChangeStep(4))
                .Then(() => Timers.Wait(0.7f))
                .Then(() => ChangeStep(5))
                .Done(() => SoundManager.Play(Sounds.ECommon.Beep01));

            return complete;
        }

        private IPromise ChangeStep(int step)
        {
            var deferred = new Deferred();
            var duration = 0.2f;

            SoundManager.Play(Sounds.ECommon.Beep00);

            transform.TweenScale()
                     .SetAutoCleanup(true)
                     .SetDuration(duration)
                     .SetStartValue(Vector3.one)
                     .SetEndValue(SmallScale)
                     .SetLoopType(TweenLoopType.Pingpong)
                     .SetLoopCount(2)
                     .OnLoopCompleted((t) =>
                     {
                         Hand.sprite = Hands[step];
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