using System;
using Lean.Localization;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class ReplayPopup : BasePopup
{
    [SerializeField] private Button replayLevelButton;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private GameSetting gameSetting;
    [SerializeField] private LevelObserver levelObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;

    public static event Action replayLevelEvent;
    public static event Action<int> changeLivesNumberEvent;

    protected override void MoreActionInAwake()
    {
        LivesShopPopup.runPendingReplayCommandEvent += ActualReplay;

        replayLevelButton.onClick.AddListener(Replay);
    }

    protected override void MoreActionOnDestroy()
    {
        LivesShopPopup.runPendingReplayCommandEvent -= ActualReplay;
    }

    private void Replay()
    {
        DebugLogger.LogSimple();
        if (GamePersistentVariable.livesData.CurrentLives <= 0)
        {
            DebugLogger.LogSimple(message: $"first");
            GamePersistentVariable.isPendingReplay = true;

            SwitchRoute(ScreenRoute.LivesShop);

            return;
        }

        bool isShowInterReplayLevel = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_SHOW_INTER_REPLAY_LEVEL, false);

        if (isShowInterReplayLevel)
        {
            GameController.Instance.admobAds.ShowInterstitial(actionWatchLog: ActionInterAds.Replay.ToString(), actionIniterClose: ActualReplay);
        }
        else
        {
            ActualReplay();
        }

        Hide();
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
        changeLivesNumberEvent?.Invoke(-1);
    }
}
