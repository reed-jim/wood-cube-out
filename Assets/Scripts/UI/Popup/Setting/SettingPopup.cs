using System;
using System.Threading.Tasks;
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;
using Lean.Localization;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class SettingPopup : BasePopup
{
    [SerializeField] private SettingItemUI turnMusicToggle;
    [SerializeField] private SettingItemUI turnSoundToggle;
    [SerializeField] private SettingItemUI vibrationToggle;
    [SerializeField] private SettingItemUI notificationToggle;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button openCompensatePopupButton;
    [SerializeField] private Button openBackupPopupButton;
    [SerializeField] private Button adPreferencesButton;
    [SerializeField] private Button openFacebookGroupButton;

    #region LANGUAGE
    [SerializeField] private RectTransform chooseLanguageContainer;
    [SerializeField] private CanvasGroup chooseLanguageContainerCanvasGroup;
    [SerializeField] private Button openLanguageDropdownButton;
    [SerializeField] private Button[] chooseLanguageButtons;
    [SerializeField] private Image[] chooseLanguageButtonBackground;
    [SerializeField] private Button rateButton;
    #endregion

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private GameSetting gameSetting;
    [SerializeField] private LevelObserver levelObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;

    public static event Action<bool> enableBackgroundMusicEvent;
    public static event Action<bool> enableGameSoundEvent;
    public static event Action<ScreenRoute> switchRouteEvent;
    public static event Action replayLevelEvent;
    public static event Action<bool> pauseNavmeshEvent;

    protected override void MoreActionInAwake()
    {
        turnMusicToggle.SetState(gameSetting.IsTurnOnBackgroundMusic);
        turnSoundToggle.SetState(gameSetting.IsTurnOnSound);
        vibrationToggle.SetState(gameSetting.IsVibrate);
        notificationToggle.SetState(gameSetting.isNotification);

        chooseLanguageContainer.gameObject.SetActive(false);
    }

    protected override void RegisterMoreEvent()
    {
        GameVariableInitializer.gameSettingLoadedEvent += OnGameSettingLoaded;

        turnMusicToggle.RegisterOnSwitchedEvent(SettingGameMusic);
        turnSoundToggle.RegisterOnSwitchedEvent(SettingGameSound);
        vibrationToggle.RegisterOnSwitchedEvent(EnableVibration);
        notificationToggle.RegisterOnSwitchedEvent(EnableNotification);

        replayButton.onClick.AddListener(Replay);
        // rateButton.onClick.AddListener(Rate);
        // openCompensatePopupButton.onClick.AddListener(OpenCompensatePopup);
        // openBackupPopupButton.onClick.AddListener(OpenBackupPopup);
        // adPreferencesButton.onClick.AddListener(OpenUserConsentPopup);
        openLanguageDropdownButton.onClick.AddListener(OpenLanguageDropdown);
        // openFacebookGroupButton.onClick.AddListener(OpenFacebookGroup);

        for (int i = 0; i < GameConstants.AvailableLanguages.Length; i++)
        {
            string language = GameConstants.AvailableLanguages[i];

            chooseLanguageButtons[i].onClick.AddListener(() => ChangeLanguage(language));
        }
    }

    protected override void UnregisterMoreEvent()
    {
        GameVariableInitializer.gameSettingLoadedEvent -= OnGameSettingLoaded;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        pauseNavmeshEvent?.Invoke(true);

        GamePersistentVariable.isPauseGame = true;

        base.Show(onCompletedAction);
    }

    protected override void Hide()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        pauseNavmeshEvent?.Invoke(false);

        GamePersistentVariable.isPauseGame = false;

        base.Hide();
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

    private void EnableVibration(bool isTurnOn)
    {
        gameSetting.IsVibrate = isTurnOn;
        GamePersistentVariable.IsVibrate = isTurnOn;
    }

    private void EnableNotification(bool isTurnOn)
    {
        gameSetting.isNotification = isTurnOn;
    }

    private void OnGameSettingLoaded()
    {
        turnMusicToggle.SetState(gameSetting.IsTurnOnBackgroundMusic);
        turnSoundToggle.SetState(gameSetting.IsTurnOnSound);
        vibrationToggle.SetState(gameSetting.IsVibrate);
        notificationToggle.SetState(gameSetting.isNotification);
    }

    #region LANGUAGE
    private void OpenLanguageDropdown()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Language);

        // chooseLanguageContainer.gameObject.SetActive(true);

        // chooseLanguageContainer.localScale = new Vector3(1, 0, 1);

        // Tween.ScaleY(chooseLanguageContainer, 1, duration: 0.3f);

        // openLanguageDropdownButton.onClick.RemoveListener(OpenLanguageDropdown);
        // openLanguageDropdownButton.onClick.AddListener(CloseLanguageDropdown);

        // for (int i = 0; i < GameConstants.AvailableLanguages.Length; i++)
        // {
        //     if (GameConstants.AvailableLanguages[i] == gameSetting.CurrentLanguage)
        //     {
        //         chooseLanguageButtonBackground[i].enabled = true;
        //     }
        //     else
        //     {
        //         chooseLanguageButtonBackground[i].enabled = false;
        //     }
        // }

        // isAbleToHideOnClickOutside = false;
    }

    private void CloseLanguageDropdown()
    {
        Tween.Custom(1, 0, duration: 0.2f, onValueChange: newVal =>
        {
            chooseLanguageContainerCanvasGroup.alpha = newVal;
        })
        .OnComplete(() =>
        {
            chooseLanguageContainerCanvasGroup.alpha = 1;

            chooseLanguageContainer.gameObject.SetActive(false);

            openLanguageDropdownButton.onClick.RemoveListener(CloseLanguageDropdown);
            openLanguageDropdownButton.onClick.AddListener(OpenLanguageDropdown);
        });

        Tween.ScaleY(chooseLanguageContainer, 0, duration: 0.3f)
        .OnComplete(() =>
        {
            chooseLanguageContainer.localScale = Vector3.one;
        });

        isAbleToHideOnClickOutside = true;
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
        // switchRouteChannel.Invoke(ScreenRoute.Rate, nameof(SettingPopup));
    }
    #endregion

    #region Compensate
    private void OpenCompensatePopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Compensate);
        // switchRouteChannel.Invoke(ScreenRoute.Compensate, nameof(SettingPopup));
    }
    #endregion

    private void Replay()
    {
        bool isShowInterReplayLevel = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_SHOW_INTER_REPLAY_LEVEL, false);

        if (isShowInterReplayLevel)
        {
            GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: OnInterstitialAdToReplayCompleted);
        }
        else
        {
            ActualReplay();
        }

        Hide();
    }

    private void OnInterstitialAdToReplayCompleted()
    {
        SaferioTracking.TrackInterstitialAdCompleted(currentLevel.Value, "replay");

        ActualReplay();
    }

    private void ActualReplay()
    {
        replayLevelEvent?.Invoke();

        GamePersistentVariable.isPreventSelectAntBox = false;

        SaferioTracking.TrackLevelLose(currentLevel.Value, levelObserver.Progress, levelBoosterObserver, EndLevelReason.Replay.ToString());

        TimeSpan playDuration = TimeSpan.FromSeconds(Time.time - GamePersistentVariable.startLevelTime);
        new FLevelLog(currentLevel.Value, $"{GamePersistentVariable.levelItem.difficulty}", LevelStatus.Fail, playDuration, GamePersistentVariable.levelRetryTime).Send();
    }

    private void OpenBackupPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Backup);
    }

    private void OpenUserConsentPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.UserConsent);
    }

    private void OpenFacebookGroup()
    {
        Application.OpenURL("https://www.facebook.com/groups/2018540212314555");
    }
}
