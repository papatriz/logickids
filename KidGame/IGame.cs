
using KidGame.Lessons;
using KidGame.UI;
using UnityEngine;

namespace KidGame
{
    public interface IGame
    {
        int StarsToNextUnlock { get; set; }
        int CurrentCardUnlocked { get; set; }
        int StarsInChest { get; set; }

        Layouts Layout { get; set; }
        float AspectRatio { get; set; }
        bool MusicEnabled { get; set; }
        bool SoundEnabled { get; set; }
        SystemLanguage Language { get; set; }

        int StarsForGivenMistakes(int mistakes);
        int GetLessonStartCount(ELessons lesson);
        void AddLessonStartCount(ELessons lesson);
        bool IsLessonFree(ELessons lesson);

    }
}