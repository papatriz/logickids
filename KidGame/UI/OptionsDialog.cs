using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KidGame.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.UI
{
    public class OptionsDialog : TweenScalable
    {
        public event Action<SystemLanguage> LangButtonClicked = (t) => { };
        public event Action MusicStateSwitched = () => { };
        public event Action SoundStateSwitched = () => { };
        public event Action CloseClicked;

        public Button SoundSwitcher;
        public Button MusicSwitcher;
        public Button CloseButton;

        public Sprite ButtonBackground;
        public Sprite SelectedButtonBackground;

        public Sprite MusicOnSprite;
        public Sprite MusicOffSprite;
        public Sprite SoundOnSprite;
        public Sprite SoundOffSprite;

        public GameObject LangGroup;

        private Button[] LangButtons;

        private Dictionary<SystemLanguage, string> Languages = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "English" },
            { SystemLanguage.Russian, "Русский" },
            { SystemLanguage.German, "Deutsch" },
            { SystemLanguage.French, "Français" },
            { SystemLanguage.Japanese, "日本の" },
            { SystemLanguage.Korean, "한국의" },
            { SystemLanguage.Italian, "Italiano" },
            { SystemLanguage.Spanish, "Español" },
            { SystemLanguage.Polish, "Polski" },
            { SystemLanguage.Portuguese, "Português" }
        };

        private void Awake()
        {
            LangButtons = LangGroup.GetComponentsInChildren<Button>();

            for (int i = 0; i < LangButtons.Length; i++)
            {
                var lang_i = Languages.ElementAt(i).Key;

                LangButtons[i].GetComponentInChildren<Text>().text = Languages.ElementAt(i).Value;
                LangButtons[i].onClick.AddListener(() => HandleLangButtonClick(lang_i));
            }

            SoundSwitcher.onClick.AddListener(HandleSoundButtonClicked);
            MusicSwitcher.onClick.AddListener(HandleMusicButtonClicked);
            CloseButton.onClick.AddListener(() => CloseClicked());
        }

        private void HandleLangButtonClick(SystemLanguage lang)
        {
            Debug.Log("Click at " + lang);
            LangButtonClicked(lang);
        }

        void HandleMusicButtonClicked()
        {
            if (MusicSwitcher.GetComponent<Image>().sprite == MusicOnSprite)
            {
                MusicSwitcher.GetComponent<Image>().sprite = MusicOffSprite;
            }
            else
            {
                MusicSwitcher.GetComponent<Image>().sprite = MusicOnSprite;
            }

            MusicStateSwitched();
        }

        void HandleSoundButtonClicked()
        {
            if (SoundSwitcher.GetComponent<Image>().sprite == SoundOnSprite)
            {
                SoundSwitcher.GetComponent<Image>().sprite = SoundOffSprite;
            }
            else
            {
                SoundSwitcher.GetComponent<Image>().sprite = SoundOnSprite;
            }

            SoundStateSwitched();
        }

        public void SetInitialState(bool musicEnable, bool soundEnable, SystemLanguage currentLang)
        {
            DeselectAllButton();
            ShowActiveLanguage(currentLang);

            var musicSprite = musicEnable ? MusicOnSprite : MusicOffSprite;
            var soundSprite = soundEnable ? SoundOnSprite : SoundOffSprite;

            SoundSwitcher.GetComponent<Image>().sprite = soundSprite;
            MusicSwitcher.GetComponent<Image>().sprite = musicSprite;

            UpdateViewLanguage();
        }

        private void UpdateViewLanguage()
        {
            SoundSwitcher.GetComponentInChildren<Text>().text = DialogKeys.Sound.GetNative();
            MusicSwitcher.GetComponentInChildren<Text>().text = DialogKeys.Music.GetNative();

            CloseButton.GetComponentInChildren<Text>().text = DialogKeys.Close.GetNative();
        }

        void ShowActiveLanguage(SystemLanguage lang)
        {
            var ind = Array.IndexOf(Languages.Keys.ToArray(), lang);
            Debug.Log("Lang index = " + ind);

            LangButtons[ind].GetComponent<Image>().sprite = SelectedButtonBackground;
            LangButtons[ind].GetComponentInChildren<Text>().color = Color.white;

        }

        private void DeselectAllButton()
        {
            for (int i = 0; i < LangButtons.Length; i++)
            {
                LangButtons[i].GetComponent<Image>().sprite = ButtonBackground;
                LangButtons[i].GetComponentInChildren<Text>().color = Color.gray;
            }
        }

    }
}