using System;
using Firebase.RemoteConfig;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static GameEnum;

public class LosePopup : BasePopup
{
    [SerializeField] private Button replayButton;
    [SerializeField] private Button returnHomeButton;
    [SerializeField] private Image levelProgressFill;
    [SerializeField] private TMP_Text progressText;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private LevelDataContainer levelDataContainer;
    [SerializeField] private LevelObserver levelObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;

    [Header("CUSTOMIZE")]
    [SerializeField] private float transitionDuration;

    public static event Action replayLevelEvent;
    public static event Action skipLevelEvent;
    public static event Action<int> changeLivesNumberEvent;

    protected override void RegisterMoreEvent()
    {
        // GameStateLose.loseLevelEvent += OnLevelLose;
        GameStateLose.loseLevelEvent += OnLevelLose;
        RevivePopup.reviveRefusedEvent += OnLevelLose;
        IAPShopPopup.reviveRefusedEvent += OnLevelLose;
        GameplayScreen.skipLevelEvent += SkipLevel;
        Application.quitting += OnAppClosed;

        replayButton.onClick.AddListener(Replay);
        returnHomeButton.onClick.AddListener(ReturnHome);
    }

    protected override void UnregisterMoreEvent()
    {
        // GameStateLose.loseLevelEvent -= OnLevelLose;
        GameStateLose.loseLevelEvent -= OnLevelLose;
        RevivePopup.reviveRefusedEvent -= OnLevelLose;
        IAPShopPopup.reviveRefusedEvent -= OnLevelLose;
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

        SaferioTracking.TrackLevelLose(currentLevel.Value, levelObserver.Progress, levelBoosterObserver, EndLevelReason.Lose.ToString());

        // don't track loop level CostCenter
        if (currentLevel.Value <= levelDataContainer.MaxLevel)
        {
            SaferioTracking.TrackLevelEnd(currentLevel.Value, isSuccess: false, EndLevelReason.Lose.ToString());
        }

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

        GamePersistentVariable.isLevelLost = true;
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
            AdmobAdsMax.Instance.ShowInterstitial(actionIniterClose: OnInterstitialAdCompleted);
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
        AdmobAdsMax.Instance.ShowVideoReward(
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
