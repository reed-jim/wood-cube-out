using System;
using System.Linq;
using Lean.Localization;
using NamPhuThuy.UI;
using UnityEngine;
using UnityEngine.UI;

public class LanguagePopup : BasePopup
{
    [SerializeField] private LanguageButton[] chooseLanguageButtons;
    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] private GameSetting gameSetting;

    protected override void RegisterMoreEvent()
    {
        for (int i = 0; i < GameConstants.AvailableLanguages.Length; i++)
        {
            string language = GameConstants.AvailableLanguages[i];

            chooseLanguageButtons[i].RegisterOnSelectAction(() => ChangeLanguage(language));
        }
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction: () => ScrollToCurrentLanguage());
        
        scrollRect.LockVerticalScrollALittle();

        foreach (var item in chooseLanguageButtons)
        {
            item.SetStateBasedOnLanguage(gameSetting.CurrentLanguage);
        }
    }

    private void ChangeLanguage(string language)
    {
        LeanLocalization.SetCurrentLanguageAll(language);

        Hide();

        gameSetting.CurrentLanguage = language;
    }

    private void ScrollToCurrentLanguage()
    {
        float index = 0;

        for (int i = 0; i < chooseLanguageButtons.Length; i++)
        {
            if (chooseLanguageButtons[i].IsThisLanguageSelected(gameSetting.CurrentLanguage))
            {
                index = i;

                break;
            }
        }

        scrollRect.horizontalNormalizedPosition = index / chooseLanguageButtons.Length;
    }
}
