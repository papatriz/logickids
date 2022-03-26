using Orbox.Localization;
using System.Collections.Generic;
using UnityEngine;


public class LanguageProvider : ILanguageProvider
{
    private SystemLanguage DefaultLanguage => SystemLanguage.English;
    private HashSet<SystemLanguage> SupportedLanguagesHashset = new HashSet<SystemLanguage>()
        {
            SystemLanguage.English,
            SystemLanguage.Russian
        };

    public SystemLanguage GetLanguage()
    {
        var systemLanguage = Application.systemLanguage;

        return systemLanguage;

        //orbox: todo: fill Supported languages hashset and use code below
        /*
        if (SupportedLanguagesHashset.Contains(systemLanguage))
        {
            return systemLanguage;
        }
        else
        {
            return DefaultLanguage;
        }
        */
    }
}
