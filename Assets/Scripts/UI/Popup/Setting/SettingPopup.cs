using System;
using System.Threading.Tasks;
using Lean.Localization;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class SettingPopup : BasePopup
{
    [SerializeField] private SaferioToggle turnMusicToggle;
    [SerializeField] private SaferioToggle turnSoundToggle;

    #region LANGUAGE
    [SerializeField] private RectTransform chooseLanguageContainer;
    [SerializeField] private Button openLanguageDropdownButton;
    [SerializeField] private Button[] chooseLanguageButtons;
    [SerializeField] private Image[] chooseLanguageButtonBackground;
    [SerializeField] private Button rateButton;
    #endregion

    [SerializeField] private GameSetting gameSetting;

    public static event Action<bool> enableBackgroundMusicEvent;
    public static event Action<bool> enableGameSoundEvent;
    public static event Action<ScreenRoute> switchRouteEvent;

    protected override void MoreActionInAwake()
    {
        turnMusicToggle.SetState(gameSetting.IsTurnOnBackgroundMusic);
        turnSoundToggle.SetState(gameSetting.IsTurnOnSound);

        chooseLanguageContainer.gameObject.SetActive(false);
    }

    protected override void RegisterMoreEvent()
    {
        turnMusicToggle.RegisterOnSwitchedEvent(SettingGameMusic);
        turnSoundToggle.RegisterOnSwitchedEvent(SettingGameSound);
        rateButton.onClick.AddListener(Rate);

        openLanguageDropdownButton.onClick.AddListener(OpenLanguageDropdown);

        for (int i = 0; i < GameConstants.AvailableLanguages.Length; i++)
        {
            string language = GameConstants.AvailableLanguages[i];

            chooseLanguageButtons[i].onClick.AddListener(() => ChangeLanguage(language));
        }
    }

    private void SettingGameMusic(bool isTurnOn)
    {
        gameSetting.IsTurnOnBackgroundMusic = isTurnOn;

        enableBackgroundMusicEvent?.Invoke(isTurnOn);
    }

    private void SettingGameSound(bool isTurnOn)
    {
        gameSetting.IsTurnOnSound = isTurnOn;

        enableGameSoundEvent?.Invoke(isTurnOn);
    }

    #region LANGUAGE
    private void OpenLanguageDropdown()
    {
        chooseLanguageContainer.gameObject.SetActive(true);

        chooseLanguageContainer.localScale = new Vector3(1, 0, 1);

        Tween.ScaleY(chooseLanguageContainer, 1, duration: 0.3f);

        openLanguageDropdownButton.onClick.RemoveAllListeners();
        openLanguageDropdownButton.onClick.AddListener(CloseLanguageDropdown);

        for (int i = 0; i < GameConstants.AvailableLanguages.Length; i++)
        {
            if (GameConstants.AvailableLanguages[i] == gameSetting.CurrentLanguage)
            {
                chooseLanguageButtonBackground[i].enabled = true;
            }
            else
            {
                chooseLanguageButtonBackground[i].enabled = false;
            }
        }
    }

    private void CloseLanguageDropdown()
    {
        Tween.ScaleY(chooseLanguageContainer, 0, duration: 0.3f)
        .OnComplete(() =>
        {
            chooseLanguageContainer.localScale = Vector3.one;

            chooseLanguageContainer.gameObject.SetActive(false);

            openLanguageDropdownButton.onClick.RemoveAllListeners();
            openLanguageDropdownButton.onClick.AddListener(OpenLanguageDropdown);
        });
    }

    private void ChangeLanguage(string language)
    {
        LeanLocalization.SetCurrentLanguageAll(language);

        CloseLanguageDropdown();

        gameSetting.CurrentLanguage = language;
    }
    #endregion

    #region RATE
    private void Rate()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Rate);
    }
    #endregion
}
