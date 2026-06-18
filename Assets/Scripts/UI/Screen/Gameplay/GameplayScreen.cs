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
    [SerializeField] private Button openDebugPopupButton;
    [SerializeField] private Button openInventoryPopupButton;
    [SerializeField] private TMP_Text remainingScrewText;
    [SerializeField] private TMP_Text localizedLevelText;
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private GameSetting gameSetting;
    [SerializeField] private TMP_Text timerText;
    [Header("HARD MODE")]
    [SerializeField] private CanvasGroup hardModeCanvasGroup;
    [Header("Resources")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private TMP_Text coinText;
    public static event Action<int> coinTextChangedEvent;



    public static event Action<ScreenRoute> switchRouteEvent;
    public static event Action nextLevelEvent;
    public static event Action prevLevelEvent;
    public static event Action skipLevelEvent;

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

        if (userResourcesObserver != null)
        {
            userResourcesObserver.updateCoinEvent += UpdateCoinText;
        }

        pauseButton.onClick.AddListener(Pause);
        noAdsButton.onClick.AddListener(OpenNoAdsPopup);

        if (openInventoryPopupButton != null)
        {
            openInventoryPopupButton.onClick.AddListener(OpenInventoryPopup);
        }

        openDebugPopupButton.onClick.AddListener(OpenDebugPopupButton);

        if (UserData.IsRemoveAds)
        {
            noAdsButton.gameObject.SetActive(false);
        }

        if (!gameSetting.IsDebug)
        {
            openDebugPopupButton.gameObject.SetActive(false);
        }

        UpdateCoinText();
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

        if (userResourcesObserver != null)
        {
            userResourcesObserver.updateCoinEvent -= UpdateCoinText;
        }
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

    private void OpenInventoryPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Inventory);
    }

    #endregion
    private void Update()
    {
        if (TileManager.Ins != null && TileManager.Ins.isLevelActive)
        {
            UpdateTimerDisplay(TileManager.Ins.currentLevelTime);
        }
    }
    private void UpdateTimerDisplay(float timeToDisplay)
    {
        if (timeToDisplay < 0) timeToDisplay = 0;
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);

        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    private void OpenDebugPopupButton()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Debug);
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
        UpdateCoinText();
        //bool hasTimeLimit = TileManager.Ins.levelItem.timeLimit > 0;
        //if (timerText != null)
        //{
        //    timerText.gameObject.SetActive(hasTimeLimit);
        //    if (timerText.transform.parent != null)
        //    {
        //        // Optionally hide parent if it's a dedicated timer container
        //        if (timerText.transform.parent.name.ToLower().Contains("time"))
        //        {
        //            timerText.transform.parent.gameObject.SetActive(hasTimeLimit);
        //        }
        //    }
        //}
    }

    private void UpdateCoinText()
    {
        if (coinText != null && userResourcesObserver != null && userResourcesObserver.UserResources != null)
        {
            coinText.text = $"{userResourcesObserver.UserResources.CoinQuantity}";
        }
    }

    private void OnLevelTextTranslated()
    {
        //localizedLevelText.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{GetDisplayedLevel()}");
    }

    private void SetLevelText()
    {
        //localizedLevelText.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{GetDisplayedLevel()}");
        localizedLevelText.text = string.Format("LV {0}", GetDisplayedLevel());
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

    #region HARD MODE
    public void SwitchToHardMode()
    {
        hardModeCanvasGroup.gameObject.SetActive(true);

        hardModeCanvasGroup.alpha = 0;

        Tween.Alpha(hardModeCanvasGroup, 1, duration: 0.3f);
    }
    #endregion      
}
