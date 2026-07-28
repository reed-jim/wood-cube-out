using System;
using System.Threading.Tasks;
using Lean.Localization;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class GameplayScreen : Singleton<GameplayScreen>
{
    [Header("Buttons")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button noAdsButton;
    [SerializeField] private Button openShopPopupButton;
    [SerializeField] private Button openDebugPopupButton;
    [SerializeField] private TMP_Text remainingScrewText;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedLevelText;
    [SerializeField] private RectTransform noAdsPack;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private GameSetting gameSetting;

    [Header("Game Speed")]
    [SerializeField] private Button upSpeedButton;
    [SerializeField] private CanvasGroup upSpeedActiveState;
    [SerializeField] private CanvasGroup upSpeedInactiveState;

    [SerializeField] private Button increaseGameSpeedButton;
    [SerializeField] private Button decreaseGameSpeedButton;
    [SerializeField] private TMP_Text gameSpeedText;
    private bool _isSwitchingStateUpSpeed;

    [Header("HARD MODE")]
    [SerializeField] private CanvasGroup hardModeCanvasGroup;

    public static event Action<ScreenRoute> switchRouteEvent;
    public static event Action nextLevelEvent;
    public static event Action prevLevelEvent;
    public static event Action skipLevelEvent;

    [Header("Debug")]
    [SerializeField] private CanvasGroup debugContainer;
    [SerializeField] private GameObject mainUI;
    [SerializeField] private GameObject boosterUI;
    [SerializeField] private CanvasGroup mainUICanvasGroup;
    [SerializeField] private CanvasGroup boosterUICanvasGroup;
    [SerializeField] private SkeletonGraphic iconSkeleton;
    [SpineAnimation] public string iconLoadingAnimation;

    #region MonoBehaviour Callbacks

    protected override void MoreActionOnAwake()
    {
        LevelLoader.startLevelEvent += OnLevelStarted;
        LevelLoader.setLevelNumberEvent += SetLevelText;
        ScrewManager.updateRemainingScrewEvent += UpdateRemainingScrew;
        ScrewManager.winLevelEvent += OnLevelWin;
        ScrewBoxManager.loseLevelEvent += OnLevelLose;
        ScrewsDataManager.updateRemainingScrewEvent += UpdateRemainingScrew;
        ResourceEarnPopup.removeAdsEvent += OnAdsRemoved;
        SaferioIAPManager.removeAdsEvent += OnAdsRemoved;

        pauseButton.onClick.AddListener(Pause);
        noAdsButton.onClick.AddListener(OpenNoAdsPopup);
        upSpeedButton.onClick.AddListener(X2Speed);

        openShopPopupButton.onClick.AddListener(OpenShopPopupButton);
        openDebugPopupButton.onClick.AddListener(OpenDebugPopupButton);

        increaseGameSpeedButton.onClick.AddListener(IncreaseGameSpeed);
        decreaseGameSpeedButton.onClick.AddListener(DecreaseGameSpeed);

        if (UserData.IsRemoveAds)
        {
            noAdsButton.gameObject.SetActive(false);
        }

        if (!gameSetting.IsDebug)
        {
            openDebugPopupButton.gameObject.SetActive(false);
        }

        gameSpeedText.text = $"x{(int)Time.timeScale}";
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStarted;
        LevelLoader.setLevelNumberEvent -= SetLevelText;
        ScrewManager.updateRemainingScrewEvent -= UpdateRemainingScrew;
        ScrewManager.winLevelEvent -= OnLevelWin;
        ScrewBoxManager.loseLevelEvent -= OnLevelLose;
        ScrewsDataManager.updateRemainingScrewEvent -= UpdateRemainingScrew;
        ResourceEarnPopup.removeAdsEvent -= OnAdsRemoved;
        SaferioIAPManager.removeAdsEvent -= OnAdsRemoved;
    }

    #endregion

    #region Private Methods

    private void OnLevelWin()
    {

    }

    private void OnLevelLose()
    {

    }

    #endregion

    #region Near Win Level
    public void ShowUINearWinLevel()
    {
        SaferioTween.AlphaShow(mainUICanvasGroup);
        SaferioTween.AlphaShow(boosterUICanvasGroup);

        if (UserData.IsRemoveAds)
        {
            noAdsPack.gameObject.SetActive(false);
        }
    }

    public void HideUINearWinLevel(Action onCompleteAction)
    {
        SaferioTween.AlphaHide(mainUICanvasGroup);
        SaferioTween.AlphaHide(boosterUICanvasGroup, onCompleteAction: onCompleteAction);
    }

    public void ShowGameIcon(Action onCompleteAction)
    {
        iconSkeleton.gameObject.SetActive(true);
        iconSkeleton.color = Color.white;

        iconSkeleton.Skeleton.SetToSetupPose();
        iconSkeleton.AnimationState.ClearTracks();
        TrackEntry trackEntry = iconSkeleton.AnimationState.SetAnimation(0, iconLoadingAnimation, false);

        trackEntry.Complete += OnAnimationComplete;

        void OnAnimationComplete(TrackEntry entry)
        {
            trackEntry.Complete -= OnAnimationComplete;

            Tween.Alpha(iconSkeleton, 0, duration: 0.3f)
            .OnComplete(() =>
            {
                onCompleteAction?.Invoke();
            });
        }
    }
    #endregion

    #region Button Events

    private void Pause()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Pause);
    }

    private void OpenNoAdsPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.RemoveAd);
    }

    private void OnClickHelp()
    {
        switchRouteEvent?.Invoke(ScreenRoute.SpecialLevelTutorial);
    }

    #endregion

    public void HideDebug()
    {
        debugContainer.alpha = 0;
    }

    public void HideUI()
    {
        GamePersistentVariable.isHideUI = !GamePersistentVariable.isHideUI;

        mainUI.SetActive(!GamePersistentVariable.isHideUI);
        boosterUI.SetActive(!GamePersistentVariable.isHideUI);
    }

    private void OpenDebugPopupButton()
    {
        debugContainer.alpha = 1;

        switchRouteEvent?.Invoke(ScreenRoute.Debug);
    }

    private void OpenShopPopupButton()
    {
        switchRouteEvent?.Invoke(ScreenRoute.IAPShopPopup);
    }

    private void NextLevel()
    {
        nextLevelEvent?.Invoke();
    }

    private void PrevLevel()
    {
        prevLevelEvent?.Invoke();
    }

    private void SkipLevel()
    {
        skipLevelEvent?.Invoke();

        GamePersistentVariable.isAllowedSkipLevel = false;
    }

    private void OnLevelStarted()
    {
        ShowUINearWinLevel();
    }

    private void OnLevelTextTranslated()
    {
        localizedLevelText.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{GetDisplayedLevel()}");
    }

    private void SetLevelText()
    {
        localizedLevelText.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{GetDisplayedLevel()}");
    }

    private void UpdateRemainingScrew(int remainingScrew, int totalScrew)
    {
        remainingScrewText.text = $"{remainingScrew}/{totalScrew}";
    }

    private int GetDisplayedLevel()
    {
        return currentLevel.Value;
    }

    #region Remove Ads
    private void OnAdsRemoved()
    {
        SaferioTween.ScaleDownWithPunch(noAdsButton.transform);
    }
    #endregion

    public System.Collections.Generic.List<RewardRandom> GetItemYouMiss()
    {

        // if (BirdSort2.GamePlayController_BirdSort.Instance == null) return null;
        System.Collections.Generic.List<RewardRandom> rewardMiss = new System.Collections.Generic.List<RewardRandom>();
        rewardMiss.Add(new RewardRandom() { typeItem = GiftType.StarCampain, amount = -1 });

        //Challenge
        // var scoreTtemsChallegen = BirdSort2.GamePlayController_BirdSort.Instance.challengeGameplayController.PredictItems;
        // foreach (var it in scoreTtemsChallegen)
        // {
        //     if (it.Value > 0)
        //     {
        //         rewardMiss.Add(new RewardRandom() { typeItem = BirdSortGameController.Instance.challengeController.GetGiftTypeItemChallenge(it.Key), amount = it.Value });
        //     }
        // }

        // var scoreTtemsEvent = BirdSort2.GamePlayController_BirdSort.Instance.eventSaleController.PredictItems;
        // foreach (var it in scoreTtemsEvent)
        // {
        //     if (it.Value > 0)
        //     {
        //         rewardMiss.Add(new RewardRandom() { typeItem = GiftType.EndlessSaleItem, amount = it.Value });
        //     }
        // }


        return rewardMiss;
    }

    #region HARD MODE
    public void SwitchToHardMode()
    {
        hardModeCanvasGroup.gameObject.SetActive(true);

        hardModeCanvasGroup.alpha = 0;

        Tween.Alpha(hardModeCanvasGroup, 1, duration: 0.3f);
    }
    #endregion

    #region Game Speed
    public void X2Speed()
    {
        if (_isSwitchingStateUpSpeed)
        {
            return;
        }

        _isSwitchingStateUpSpeed = true;

        int currentSpeed = UpSpeedAnt.Ins.SwitchGameSpeed();

        UpdateUpSpeedState(currentSpeed);
    }

    public void UpdateUpSpeedState(int currentSpeed)
    {
        if (currentSpeed == 1)
        {
            SaferioTween.AlphaHide(upSpeedActiveState);
            SaferioTween.AlphaShow(upSpeedInactiveState, onCompleteAction: () =>
            {
                _isSwitchingStateUpSpeed = false;
            });
        }
        else
        {
            SaferioTween.AlphaShow(upSpeedActiveState);
            SaferioTween.AlphaHide(upSpeedInactiveState, onCompleteAction: () =>
            {
                _isSwitchingStateUpSpeed = false;
            });
        }
    }

    private void IncreaseGameSpeed()
    {
        ChangeGameSpeed(true);
    }

    private void DecreaseGameSpeed()
    {
        ChangeGameSpeed(false);
    }

    private void ChangeGameSpeed(bool isIncrease)
    {
        if (isIncrease)
        {
            if (Time.timeScale < 5)
            {
                Time.timeScale++;
            }
        }
        else
        {
            if (Time.timeScale > 1)
            {
                Time.timeScale--;
            }
        }

        GamePersistentVariable.isGameSpeedChangeInDebug = true;

        if (Time.timeScale == 1)
        {
            GamePersistentVariable.isGameSpeedChangeInDebug = false;
        }

        if (gameSpeedText != null)
        {
            gameSpeedText.text = $"x{(int)Time.timeScale}";
        }
    }
    #endregion      
}
