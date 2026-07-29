using System;
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;
using Firebase.RemoteConfig;
using NamPhuThuy.GamePlay;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static GameEnum;

public class LosePopup : BasePopup
{
    [SerializeField] private Button replayButton;
    [SerializeField] private Button skipLevelButton;
    [SerializeField] private Button returnHomeButton;
    [SerializeField] private Image levelProgressFill;
    [SerializeField] private TMP_Text progressText;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private LevelObserver levelObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;

    [Header("CUSTOMIZE")]
    [SerializeField] private float transitionDuration;

    #region PRIVATE FIELD
    private bool _isAllowedSkipLevel;
    #endregion

    public static event Action replayLevelEvent;
    public static event Action skipLevelEvent;
    public static event Action<int> changeLivesNumberEvent;

    protected override void RegisterMoreEvent()
    {
        // GameStateLose.loseLevelEvent += OnLevelLose;
        GameStateLose.loseLevelEvent += OnLevelLose;
        RevivePopup.reviveRefusedEvent += OnLevelLose;
        GameplayScreen.skipLevelEvent += SkipLevel;
        Application.quitting += OnAppClosed;

        replayButton.onClick.AddListener(Replay);
        skipLevelButton.onClick.AddListener(SkipLevel);
        returnHomeButton.onClick.AddListener(ReturnHome);

        _isAllowedSkipLevel = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_SHOW_SKIP_LEVEL_BUTTON, true);

        if (!_isAllowedSkipLevel)
        {
            skipLevelButton.gameObject.SetActive(false);
        }
    }

    protected override void UnregisterMoreEvent()
    {
        // GameStateLose.loseLevelEvent -= OnLevelLose;
        GameStateLose.loseLevelEvent -= OnLevelLose;
        RevivePopup.reviveRefusedEvent -= OnLevelLose;
        GameplayScreen.skipLevelEvent -= SkipLevel;
        Application.quitting -= OnAppClosed;
    }

    private void OnAppClosed()
    {
        SaferioTracking.TrackLevelLose(currentLevel.Value, levelObserver.Progress, levelBoosterObserver, EndLevelReason.Quit.ToString());
    }

    // private void OnApplicationQuit()
    // {
    //     SaferioTracking.TrackLevelLose(currentLevel.Value, levelObserver.Progress, levelBoosterObserver, EndLevelReason.Quit.ToString());
    // }

    private void OnLevelLose()
    {
        levelProgressFill.fillAmount = 0;

        progressText.text = $"{0}%";

        changeLivesNumberEvent?.Invoke(-1);

        Show(onCompletedAction: () =>
        {
            Tween.Custom(0, levelObserver.Progress, duration: transitionDuration, onValueChange: newVal =>
            {
                // +0.1f for better progress bar visual
                levelProgressFill.fillAmount = newVal;

                progressText.text = $"{(int)(newVal * 100)}%";
            });
        });

        SoundManager.Instance.PlaySoundLose();

        // COUNT LOSE LEVEL
        int timeLoseLevel = DataUtility.Load($"Time Lose Level {currentLevel.Value}", 0);

        // -Avoid cheating
        if (levelObserver.Progress > 0.2f)
        {
            timeLoseLevel++;

            DataUtility.Save($"Time Lose Level {currentLevel.Value}", timeLoseLevel);
        }

        #region SKIP LEVEL
        GamePersistentVariable.isAllowedSkipLevel = false;

        if (!_isAllowedSkipLevel)
        {
            return;
        }

        if (timeLoseLevel >= RemoteConfigController.GetIntConfig(FirebaseConfig.MIN_TIME_LOSE_LEVEL_TO_ALLOW_SKIP, 2))
        {
            if (GamePersistentVariable.isEnableRewardedAds)
            {
                skipLevelButton.gameObject.SetActive(true);
                GamePersistentVariable.isAllowedSkipLevel = true;
            }
            else
            {
                skipLevelButton.gameObject.SetActive(false);
            }
        }
        else
        {
            skipLevelButton.gameObject.SetActive(false);
        }
        #endregion

        GamePersistentVariable.isLevelLost = true;

        SaferioTracking.TrackLevelLose(currentLevel.Value, levelObserver.Progress, levelBoosterObserver, EndLevelReason.Lose.ToString());
        SaferioTracking.TrackLevelFail(currentLevel.Value, GamePersistentVariable.levelRetryTime);

        TimeSpan playDuration = TimeSpan.FromSeconds(Time.time - GamePersistentVariable.startLevelTime);
        new FLevelLog(currentLevel.Value, $"{GamePersistentVariable.levelItem.difficulty}", LevelStatus.Fail, playDuration, GamePersistentVariable.levelRetryTime).Send();
    }

    private void Replay()
    {
        if (
            GamePersistentVariable.livesConfig.isActive &&
            DataUtility.Load(GameConstants.USER_LIVES_DATA, new LivesData()).CurrentLives == 0
        )
        {
            GamePersistentVariable.isPendingReplay = true;

            SwitchRoute(ScreenRoute.LivesShop);

            return;
        }

        if (RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_SHOW_INTER_LOSE_LEVEL, false))
        {
            GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: OnInterstitialAdCompleted);
        }
        else
        {
            replayLevelEvent?.Invoke();

            SaferioTracking.TrackReplayLevel(currentLevel.Value);

            Hide();
        }
    }

    private void OnInterstitialAdCompleted()
    {
        replayLevelEvent?.Invoke();

        SaferioTracking.TrackReplayLevel(currentLevel.Value);
        SaferioTracking.TrackInterstitialAdCompleted(currentLevel.Value, "replay");

        Hide();
    }

    private void SkipLevel()
    {
        GameController.Instance.admobAds.ShowVideoReward(
           OnRewaredAdCompletedToSkipLevel, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: ActionWatchVideo.SkipLevel);
    }

    private void OnRewaredAdCompletedToSkipLevel()
    {
        skipLevelEvent?.Invoke();

        SaferioTracking.TrackSkipLevel(currentLevel.Value);
        SaferioTracking.TrackRewardedAdCompleted(currentLevel.Value, "SkipLevel");
        TrackLevelWin();

        Hide();
    }

    private void ShowAdsNotLoadedPopup()
    {
        InvokeShowAdNotLoadedPopupEvent();
    }

    private void ReturnHome()
    {
        Addressables.LoadSceneAsync(GameConstants.MENU_SCENE);
    }

    #region SKIP LEVEL
    private async void TrackLevelWin()
    {
        bool isFirstWinLevel = DataUtility.LoadAsync($"{GameConstants.IS_FIRST_WIN_LEVEL}_{currentLevel.Value}", true);

        if (isFirstWinLevel)
        {
            SaferioTracking.TrackLevelFirstWin(currentLevel.Value, GamePersistentVariable.levelPrefab, levelBoosterObserver);

            DataUtility.SaveAsync($"{GameConstants.IS_FIRST_WIN_LEVEL}_{currentLevel.Value}", false);
        }

        SaferioTracking.TrackLevelWin(currentLevel.Value, levelBoosterObserver);
    }
    #endregion  
}
