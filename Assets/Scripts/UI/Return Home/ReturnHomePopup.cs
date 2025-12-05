using System;
using Lean.Localization;
using NamPhuThuy.Common;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static GameEnum;

public class ReturnHomePopup : BasePopup
{
    [SerializeField] private Button homeButton;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private GameSetting gameSetting;
    [SerializeField] private LevelDataContainer levelDataContainer;
    [SerializeField] private LevelObserver levelObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;

    public static event Action<int> changeLivesNumberEvent;
    public static event Action<ScreenRoute> switchRouteEvent;

    protected override void MoreActionInAwake()
    {
        LivesShopPopup.runPendingReturnHomeCommandEvent += ReturnHome;

        homeButton.onClick.AddListener(ReturnHome);
    }

    protected override void MoreActionOnDestroy()
    {
        LivesShopPopup.runPendingReturnHomeCommandEvent -= ReturnHome;
    }

    private void ReturnHome()
    {
        DebugLogger.Log();
        homeButton.interactable = false;

        if (GamePersistentVariable.livesData.CurrentLives > 0)
        {
            changeLivesNumberEvent?.Invoke(-1);
        }

        SaferioTracking.TrackLevelLose(currentLevel.Value, levelObserver.Progress, levelBoosterObserver, EndLevelReason.ReturnHome.ToString());

        if (currentLevel.Value <= levelDataContainer.MaxLevel)
        {
            SaferioTracking.TrackLevelEnd(currentLevel.Value, isSuccess: false, EndLevelReason.ReturnHome.ToString());
        }

        Addressables.LoadSceneAsync(GameConstants.MENU_SCENE);
    }
}
