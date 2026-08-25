using System;
using NamPhuThuy;
using NamPhuThuy.Events;
using NamPhuThuy.GamePlay;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static GameEnum;

public class PausePopup : BasePopup
{
    [SerializeField] private Button returnHomeButton;
    [SerializeField] private Button replayLevelButton;
    [SerializeField] private Button openReviewIAPPopupButton;
    [SerializeField] private SaferioToggle turnMusicToggle;
    [SerializeField] private SaferioToggle turnSoundToggle;
    [SerializeField] private SaferioToggle vibrationToggle;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private GameSetting gameSetting;
    [SerializeField] private LevelObserver levelObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;

    public static event Action<bool> enableBackgroundMusicEvent;
    public static event Action<bool> enableGameSoundEvent;
    public static event Action replayLevelEvent;
    public static event Action<ScreenRoute> switchRouteEvent;

    
    // [SerializeField] private SwitchRouteChannelSO switchRouteChannel;
    protected override void MoreActionInAwake()
    {
        turnMusicToggle.SetState(gameSetting.IsTurnOnBackgroundMusic);
        turnSoundToggle.SetState(gameSetting.IsTurnOnSound);
        vibrationToggle.SetState(gameSetting.IsVibrate);
    }

    protected override void RegisterMoreEvent()
    {
        ScriptableObjectInitializer.gameSettingLoadedEvent += OnGameSettingLoaded;

        returnHomeButton.onClick.AddListener(ReturnHome);
        replayLevelButton.onClick.AddListener(Replay);
        turnMusicToggle.RegisterOnSwitchedEvent(EnableGameMusic);
        turnSoundToggle.RegisterOnSwitchedEvent(EnableGameSound);
        vibrationToggle.RegisterOnSwitchedEvent(EnableVibration);
    }

    protected override void UnregisterMoreEvent()
    {
        ScriptableObjectInitializer.gameSettingLoadedEvent -= OnGameSettingLoaded;
    }

    #region MonoBehaviour Callbacks

    private void OnEnable()
    {
        
    }

    #region Overrdide Methods

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);
        returnHomeButton.interactable = true;
        replayLevelButton.interactable = true;
    }

    protected override void Hide(Action onCompletedAction = null)
    {
        base.Hide(onCompletedAction);
    }

    #endregion

    #endregion

    private void ReturnHome()
    {
        if (GamePersistentVariable.isLevelDirty && GamePersistentVariable.livesConfig.isActive)
        {
            switchRouteEvent?.Invoke(ScreenRoute.ReturnHome);
            // switchRouteChannel.Invoke(ScreenRoute.ReturnHome, nameof(PausePopup));
        }
        else
        {
            // avoid spamming
            returnHomeButton.interactable = false;
            replayLevelButton.interactable = false;

            SaferioTracking.TrackLevelLose(currentLevel.Value, levelObserver.Progress, levelBoosterObserver, EndLevelReason.ReturnHome.ToString());

            // don't track loop level CostCenter
            if (currentLevel.Value <= LevelManager.Ins.LevelDataContainer.MaxLevel)
            {
                SaferioTracking.TrackLevelEnd(currentLevel.Value, isSuccess: false, EndLevelReason.ReturnHome.ToString());
            }
            Addressables.LoadSceneAsync(GameConstants.MENU_SCENE);
        }
    }

    private void Replay()
    {
        DebugLogger.LogSimple();
        if (GamePersistentVariable.isLevelDirty && GamePersistentVariable.livesConfig.isActive)
        {
            DebugLogger.LogSimple(message:$"first");
            switchRouteEvent?.Invoke(ScreenRoute.Replay);
            // switchRouteChannel.Invoke(ScreenRoute.Replay, nameof(PausePopup));
        }
        else
        {
            DebugLogger.LogSimple(message:$"second");
            bool isShowInterReplayLevel = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_SHOW_INTER_REPLAY_LEVEL, false);

            if (isShowInterReplayLevel)
            {
                
            }
            else
            {
                ActualReplay();
            }

            Hide();
        }
    }

    private void ActualReplay()
    {
        DebugLogger.LogSimple();
        SaferioTracking.TrackLevelLose(currentLevel.Value, levelObserver.Progress, levelBoosterObserver, EndLevelReason.Replay.ToString());

        // don't track loop level CostCenter
        if (currentLevel.Value <= LevelManager.Ins.LevelDataContainer.MaxLevel)
        {
            SaferioTracking.TrackLevelEnd(currentLevel.Value, isSuccess: false, EndLevelReason.Replay.ToString());
        }
        replayLevelEvent?.Invoke();
    }

    private void OnInterstitialAdToReplayCompleted()
    {
        SaferioTracking.TrackInterstitialAdCompleted(currentLevel.Value, "replay");

        ActualReplay();
    }

    private void EnableGameMusic(bool isTurnOn)
    {
        gameSetting.IsTurnOnBackgroundMusic = isTurnOn;

        enableBackgroundMusicEvent?.Invoke(isTurnOn);
    }

    private void EnableGameSound(bool isTurnOn)
    {
        gameSetting.IsTurnOnSound = isTurnOn;

        enableGameSoundEvent?.Invoke(isTurnOn);
    }

    private void EnableVibration(bool isTurnOn)
    {
        gameSetting.IsVibrate = isTurnOn;

        GamePersistentVariable.IsVibrate = isTurnOn;
    }

    private void OnGameSettingLoaded()
    {
        turnMusicToggle.SetState(gameSetting.IsTurnOnBackgroundMusic);
        turnSoundToggle.SetState(gameSetting.IsTurnOnSound);
        vibrationToggle.SetState(gameSetting.IsVibrate);
    }

    #region Review IAP
    private void OpenReviewIAPPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.IAPReview);
        // switchRouteChannel.Invoke(ScreenRoute.IAPReview, nameof(PausePopup));
    }
    #endregion
}
