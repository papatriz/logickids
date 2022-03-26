using System.Collections;
using System.Collections.Generic;
using KidGame.Localization;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.UI
{
    public class ParentalGate : MonoBehaviour
    {
        public Button Fake01;
        public Button Fake02;
        public Button Right;

        public Text Fake01Text;
        public Text Fake02Text;
        public Text RightText;

        public Text Quest;

        public Button SpeechButton;

        private static ISoundManager SoundManager;
        private static ITimers Timers;

        private void Awake()
        {
            SoundManager = CompositionRoot.GetSoundManager();
            Timers = CompositionRoot.GetTimers();

            Fake01.onClick.AddListener(WrongAnswer);
            Fake02.onClick.AddListener(WrongAnswer);
            Right.onClick.AddListener(Close);

           // SpeechButton.onClick.AddListener(HandleSoundButtonClick);

            Debug.Log("PARENTAL GATE AWAKE");
        }

        public void Show()
        {
            Init();
            this.gameObject.SetActive(true);


        }

        private void OnEnable()
        {
            SpeechButton.onClick.RemoveAllListeners();

            Debug.Log("PARENTAL GATE OnEnable, Timers is " + Timers?.ToString());
            Timers.Wait(1f)
                .Then(PlayVoice)
                .Done(() => SpeechButton.onClick.AddListener(HandleSoundButtonClick)); 
        }

        void Init()
        {
            int a = Random.Range(5, 10);
            int b = Random.Range(5, 9);

            Quest.text = a.ToString() + " + " + b.ToString() + " = ?";

            int answer = a + b;

            Fake01Text.text = (answer - 1).ToString();
            Fake02Text.text = (answer + 1).ToString();
            RightText.text = answer.ToString();
        }

        void Close()
        {
            this.gameObject.SetActive(false);
        }

        void WrongAnswer()
        {
            Init();
        }

        void HandleSoundButtonClick()
        {
            SpeechButton.onClick.RemoveAllListeners();

            PlayVoice()
                .Done(() => SpeechButton.onClick.AddListener(HandleSoundButtonClick));

            //Timers.Wait(2.3f)
            //    .Done(() => SpeechButton.onClick.AddListener(HandleSoundButtonClick));


        }

        IPromise PlayVoice()
        {
            var sound = LocalVoices.GetVoice(LocalVoices.VoiceKeys.AskParents);

            return SoundManager.PlayAndNotify(sound);
        }
    }
}