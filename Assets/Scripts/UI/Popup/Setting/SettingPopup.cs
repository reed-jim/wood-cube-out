using System;
using System.Threading.Tasks;
using Lean.Localization;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingPopup : BasePopup
{
    [SerializeField] private Toggle turnMusicToggle;
    [SerializeField] private Toggle turnSoundToggle;

    #region LANGUAGE
    [SerializeField] private RectTransform chooseLanguageContainer;
    [SerializeField] private Button openLanguageDropdownButton;
    [SerializeField] private Button[] chooseLanguageButtons;
    #endregion

    [SerializeField] private GameSetting gameSetting;

    public static event Action<bool> enableBackgroundMusicEvent;
    public static event Action<bool> enableGameSoundEvent;

    protected override void MoreActionInAwake()
    {
        turnMusicToggle.isOn = gameSetting.IsTurnOnBackgroundMusic;
        turnSoundToggle.isOn = gameSetting.IsTurnOnSound;

        chooseLanguageContainer.gameObject.SetActive(false);
    }

    protected override void RegisterMoreEvent()
    {
        turnMusicToggle.onValueChanged.AddListener(SettingGameMusic);
        turnSoundToggle.onValueChanged.AddListener(SettingGameSound);

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
    }
    #endregion
}
