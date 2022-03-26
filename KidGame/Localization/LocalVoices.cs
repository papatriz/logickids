using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KidGame.Localization
{
    public class LocalVoices 
    {
        public enum VoiceKeys
        {
            AskParents,
            UnlockFirstDyno,
            CollectMoreStars,
            Hello
        }

        private static Dictionary<SystemLanguage, Sounds.ELocalVoices>[] Phrase = new Dictionary<SystemLanguage, Sounds.ELocalVoices>[]
        {
            new Dictionary<SystemLanguage, Sounds.ELocalVoices>
            {
                {SystemLanguage.English, Sounds.ELocalVoices.AskParentsENG },
                {SystemLanguage.Russian, Sounds.ELocalVoices.AskParentsRU },
                {SystemLanguage.German, Sounds.ELocalVoices.AskParentsDE },
            },
            new Dictionary<SystemLanguage, Sounds.ELocalVoices>
            {
                {SystemLanguage.English, Sounds.ELocalVoices.FirstDynoENG },
                {SystemLanguage.Russian, Sounds.ELocalVoices.FirstDynoRU },
                {SystemLanguage.German, Sounds.ELocalVoices.FirstDynoDE },
            },
            new Dictionary<SystemLanguage, Sounds.ELocalVoices>
            {
                {SystemLanguage.English, Sounds.ELocalVoices.CollectMoreStarsENG },
                {SystemLanguage.Russian, Sounds.ELocalVoices.CollectMoreStarsRU },
                {SystemLanguage.German, Sounds.ELocalVoices.CollectMoreStarsDE },
            },
            new Dictionary<SystemLanguage, Sounds.ELocalVoices>
            {
                {SystemLanguage.English, Sounds.ELocalVoices.HelloENG },
                {SystemLanguage.Russian, Sounds.ELocalVoices.HelloRU },
                {SystemLanguage.German, Sounds.ELocalVoices.HelloDE },
            }
        };

        public static Sounds.ELocalVoices GetVoice(VoiceKeys key)
        {
            var lang = CompositionRoot.GetGame().Language;

            return Phrase[(int)key][lang];
        }


    }
}