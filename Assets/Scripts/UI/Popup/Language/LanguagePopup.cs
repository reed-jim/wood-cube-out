using Lean.Localization;
using PrimeTween;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class LanguagePopup : BasePopup
{
    [SerializeField] private LanguageButton[] chooseLanguageButtons;
    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] private GameSetting gameSetting;

    public static event Action<string> changeLanguageEvent;

    protected override void RegisterMoreEvent()
    {
        for (int i = 0; i < GameConstants.AvailableLanguages.Length; i++)
        {
            string language = GameConstants.AvailableLanguages[i];

            chooseLanguageButtons[i].RegisterOnSelectAction(() => ChangeLanguage(language));
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(BackToSetting);
        }
    }

    private void BackToSetting()
    {
        SwitchRoute(ScreenRoute.Pause);
    }

    protected override void Show(Action onCompletedAction = null)
    {
        scrollRect.vertical = false;

        base.Show(onCompletedAction: () => ScrollToCurrentLanguage());

        foreach (var item in chooseLanguageButtons)
        {
            item.SetStateBasedOnLanguage(gameSetting.CurrentLanguage);
        }
    }

    private void ChangeLanguage(string language)
    {
        LeanLocalization.SetCurrentLanguageAll(language);

        // Hide();

        gameSetting.CurrentLanguage = language;

        changeLanguageEvent?.Invoke(language);
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

        float targetVerticalNormalizedPosition = 1 - index / chooseLanguageButtons.Length;

        Tween.Custom(scrollRect.verticalNormalizedPosition, targetVerticalNormalizedPosition, duration: 0.3f, onValueChange: newVal =>
        {
            scrollRect.verticalNormalizedPosition = newVal;
        })
        .OnComplete(() =>
        {
            scrollRect.vertical = true;
        });
    }
}
