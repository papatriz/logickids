using System;

namespace KidGame.Lessons
{
    public interface ILesson
    {
        event Action Ended;
        event Action EndedWithReward;

        void Start();
    }
}