using System;
using System.Threading.Tasks;
using Lean.Localization;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class GameplayScreen : Singleton<GameplayScreen>
{
    [Header("Buttons")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button noAdsButton;
    [SerializeField] private Button x2SpeedButton;
    [SerializeField] private Button openShopPopupButton;
    [SerializeField] private Button openDebugPopupButton;
    [SerializeField] private TMP_Text remainingScrewText;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedLevelText;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private GameSetting gameSetting;

    [Header("Game Speed")]
    [SerializeField] private Button increaseGameSpeedButton;
    [SerializeField] private Button decreaseGameSpeedButton;
    [SerializeField] private TMP_Text gameSpeedText;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedGameSpeedText;

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
        if (x2SpeedButton != null)
        {
            x2SpeedButton.onClick.AddListener(X2Speed);
        }

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

        if (gameSpeedText != null)
        {
            gameSpeedText.text = $"x{(int)Time.timeScale}";
        }
        if (localizedGameSpeedText != null)
        {
            localizedGameSpeedText.UpdateTranslationWithParameter("value", $"{(int)Time.timeScale}");
        }
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
        float prevValue = Time.timeScale;

        if (Time.timeScale == 1)
        {
            Time.timeScale = 2;
        }
        else
        {
            Time.timeScale = 1;
        }

        if (localizedGameSpeedText != null)
        {
            localizedGameSpeedText.UpdateTranslationWithParameter("value", $"{prevValue}");
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
