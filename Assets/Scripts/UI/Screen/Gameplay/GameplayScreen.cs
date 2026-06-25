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
    [SerializeField] private Button openShopPopupButton;
    [SerializeField] private Button openDebugPopupButton;
    [SerializeField] private TMP_Text remainingScrewText;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedLevelText;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private GameSetting gameSetting;

    [Header("Game Speed")]
    [SerializeField] private Button upSpeedButton;
    [SerializeField] private CanvasGroup upSpeedActiveState;
    [SerializeField] private CanvasGroup upSpeedInactiveState;

    [SerializeField] private Button increaseGameSpeedButton;
    [SerializeField] private Button decreaseGameSpeedButton;
    [SerializeField] private TMP_Text gameSpeedText;

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
        int currentSpeed = UpSpeedAnt.Ins.SwitchGameSpeed();

        if (currentSpeed == 1)
        {
            SaferioTween.AlphaHide(upSpeedActiveState);
            SaferioTween.AlphaShow(upSpeedInactiveState);
        }
        else
        {
            SaferioTween.AlphaShow(upSpeedActiveState);
            SaferioTween.AlphaHide(upSpeedInactiveState);
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

        gameSpeedText.text = $"x{(int)Time.timeScale}";
    }
    #endregion      
}
