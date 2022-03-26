using System;
using DaikonForge.Tween;
using Orbox.Async;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.Lessons.Memory04
{
    public class QuestBubble : MonoBehaviour
    {
        public Image[] Quests;

        private Image BubbleImage;
        private Image CurrentQuest;

        private void Awake()
        {
            BubbleImage = GetComponent<Image>();

            TurnOffQuests();

            SetAlpha(BubbleImage);
        }

        private void TurnOffQuests()
        {
            for (int i = 0; i < Quests.Length; i++)
            {
                Quests[i].gameObject.SetActive(false);

                SetAlpha(Quests[i]);
            }
        }

        public Transform GetTargetForTutorial()
        {
            return CurrentQuest.transform;
        }

        public IPromise Show(EJellies fruit)
        {
            var deferred = new Deferred();

            TurnOffQuests();

            var ind = Convert.ToInt32(fruit);
            CurrentQuest = Quests[ind];

            var duration = 0.7f;
            var startScale = new Vector3(0.01f, 0.01f, 1f);

            transform.localScale = startScale;
            SetAlpha(BubbleImage);

            CurrentQuest.gameObject.SetActive(true);

            var alphaMainTween = new Tween<float>();

            alphaMainTween.SetAutoCleanup(true)
                .SetStartValue(0f)
                .SetEndValue(0.75f)
                .SetDuration(duration)
                .OnExecute(current =>
                {
                    SetAlpha(BubbleImage, current);
                })
                .Play();

            var alphaQuestTween = new Tween<float>();

            alphaQuestTween.SetAutoCleanup(true)
                .SetStartValue(0f)
                .SetEndValue(1f)
                .SetDuration(duration)
                .OnExecute(current =>
                {
                    SetAlpha(CurrentQuest, current);
                })
                .Play();

            this.transform.TweenScale()
                     .SetAutoCleanup(true)
                     .SetDuration(duration)
                     .SetStartValue(startScale)
                     .SetEndValue(Vector3.one)
                     .OnCompleted((t) =>
                     {
                         deferred.Resolve();
                     })
                     .Play();

            return deferred;
        }

        public IPromise Hide()
        {
            var deferred = new Deferred();

            var duration = 0.4f;
            var endScale = new Vector3(0.01f, 0.01f, 1f);

            SetAlpha(BubbleImage);

            var alphaMainTween = new Tween<float>();

            alphaMainTween.SetAutoCleanup(true)
                .SetStartValue(0.75f)
                .SetEndValue(0f)
                .SetDuration(duration)
                .OnExecute(current =>
                {
                    SetAlpha(BubbleImage, current);
                })
                .Play();

            var alphaQuestTween = new Tween<float>();

            alphaQuestTween.SetAutoCleanup(true)
                .SetStartValue(1f)
                .SetEndValue(0f)
                .SetDuration(duration - 0.1f)
                .OnExecute(current =>
                {
                    SetAlpha(CurrentQuest, current);
                })
                .Play();

            this.transform.TweenScale()
                     .SetAutoCleanup(true)
                     .SetDuration(duration)
                     .SetStartValue(Vector3.one)
                     .SetEndValue(endScale)
                     .OnCompleted((t) =>
                     {
                         CurrentQuest.gameObject.SetActive(false);
                         deferred.Resolve();
                     })
                     .Play();

            return deferred;
        }

        private void SetAlpha(Image image, float alpha = 0f)
        {
            var tmp = image.color;
            tmp.a = alpha;
            image.color = tmp;
        }
    }
}
