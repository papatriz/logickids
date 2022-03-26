
using System;
using System.Collections.Generic;
using KidGame.Lessons;
using KidGame.UI;
using Orbox.Utils;
using UnityEngine;

namespace KidGame
{
	public class Game : IGame
	{
		private const string StarsInChestKey = "StarsInChest";
		private const string StarsToNextUnlockKey = "StarsToNextUnlock";
		private const string CurrentCardUnlockedKey = "CurrentCardUnlocked";
		private const string MusicKey = "Music";
		private const string SoundKey = "Sound";
		private const string LanguageKey = "Language";

		private const string LessonStartCountKeyPrefix = "LessonStartCount";

		private const int StarsToUnlockDefault = 21;
		private const int MistakesToHint = 2;

		private List<ELessons> FreeLessons = new List<ELessons> { ELessons.RepairCrown, ELessons.FruitOddOneOut, ELessons.Jelly03 };

		private Layouts layout;
		private float aspectRatio;

		private IPlayerPrefs PlayerPrefs;

        public Game(IPlayerPrefs playerPrefs)
        {
            PlayerPrefs = playerPrefs;

        }

		public float AspectRatio
        {
			get
            {
				return aspectRatio;
			}

			set
            {
				aspectRatio = value;
            }
        }

		public Layouts Layout
		{
			get
			{
				return layout;
			}

			set
			{
				layout = value;
			}
		}

		public bool MusicEnabled
		{
			get
			{
				int defaultVal = Convert.ToInt32(true);
				int value = PlayerPrefs.GetInt(MusicKey, defaultVal);

				bool isEnabled = Convert.ToBoolean(value);
				return isEnabled;
			}

			set
			{
				int intValue = Convert.ToInt32(value);
				PlayerPrefs.SetInt(MusicKey, intValue);
				PlayerPrefs.Save();
			}
		}

		public bool SoundEnabled
		{
			get
			{
				int defaultVal = Convert.ToInt32(true);
				int value = PlayerPrefs.GetInt(SoundKey, defaultVal);

				bool isEnabled = Convert.ToBoolean(value);
				return isEnabled;
			}

			set
			{
				int intValue = Convert.ToInt32(value);
				PlayerPrefs.SetInt(SoundKey, intValue);
				PlayerPrefs.Save();
			}
		}

		public bool IsLessonFree (ELessons lesson)
        {
			return FreeLessons.Contains(lesson);
        }

		public bool NeedHelp(int mistakesCount)
        {
			var result = mistakesCount > MistakesToHint;

			return result;
        }

		public int GetLessonStartCount(ELessons lesson)
		{
			var key = LessonStartCountKeyPrefix + lesson.ToString();
			int completeCount = PlayerPrefs.GetInt(key, 0);

			return completeCount;
		}

		public void AddLessonStartCount(ELessons lesson)
		{
			var key = LessonStartCountKeyPrefix + lesson.ToString();
			int startCount = PlayerPrefs.GetInt(key, 0);

			startCount++;

			PlayerPrefs.SetInt(key, startCount);
			PlayerPrefs.Save();
		}

		public int StarsInChest
		{
			get
			{
				return PlayerPrefs.GetInt(StarsInChestKey, 0);
			}

			set
			{
				PlayerPrefs.SetInt(StarsInChestKey, value);
			}
		}

		public int StarsToNextUnlock
		{
			get
			{
				return PlayerPrefs.GetInt(StarsToNextUnlockKey, StarsToUnlockDefault);
			}

			set
			{
				PlayerPrefs.SetInt(StarsToNextUnlockKey, value);
			}
		}

		public int CurrentCardUnlocked
		{
			get
			{
				return PlayerPrefs.GetInt(CurrentCardUnlockedKey, 0);
			}

			set
			{
				PlayerPrefs.SetInt(CurrentCardUnlockedKey, value);
			}
		}

        public int StarsForGivenMistakes(int mistakes)
        {
			var stars = 5;

			if (mistakes > 0) stars--;
			if (mistakes > 5) stars--;
			if (mistakes > 10) stars--;
			if (mistakes > 20) stars--;

			return stars;
		}

		public SystemLanguage Language
		{
			get
			{
				int defaultVal = (int)Application.systemLanguage;

				switch (Application.systemLanguage)
				{
					case SystemLanguage.English:
					case SystemLanguage.Russian:
					/*
					case SystemLanguage.Portuguese:
					case SystemLanguage.ChineseSimplified:
					case SystemLanguage.French:
					case SystemLanguage.German:
					case SystemLanguage.Italian:
					case SystemLanguage.Japanese:
					case SystemLanguage.Korean:			
					case SystemLanguage.Spanish:

					case SystemLanguage.Afrikaans:
					case SystemLanguage.Bulgarian:
					case SystemLanguage.Czech:
					case SystemLanguage.Danish:
					case SystemLanguage.Dutch:
					case SystemLanguage.Finnish:
					case SystemLanguage.Greek:
					case SystemLanguage.Hungarian:
					case SystemLanguage.Indonesian:
					case SystemLanguage.Norwegian:

					case SystemLanguage.Polish:
					case SystemLanguage.Romanian:
					case SystemLanguage.Swedish:
					case SystemLanguage.Thai:
					case SystemLanguage.Turkish:
					case SystemLanguage.Vietnamese:
					*/
						break;

					default:
						defaultVal = (int)SystemLanguage.English;
						break;
				}

				int value = PlayerPrefs.GetInt(LanguageKey, defaultVal);
				var language = (SystemLanguage)value;

				return language;
			}

			set
			{
				int intValue = (int)value;
				PlayerPrefs.SetInt(LanguageKey, intValue);
				PlayerPrefs.Save();
			}
		}

	}
}
