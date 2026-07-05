using System;
using System.Collections.Generic;
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
    [SerializeField] private Button btShop;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button noAdsButton;
    [SerializeField] private Button openDebugPopupButton;
    [SerializeField] private Button openInventoryPopupButton;
    [SerializeField] private TMP_Text remainingScrewText;
    [SerializeField] private TMP_Text localizedLevelText;
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private GameSetting gameSetting;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject noLimited;
    [SerializeField] private CanvasGroup warningGroup;
    [SerializeField] private float warningTimeLimit = 15f;
    [Header("HARD MODE")]
    [SerializeField] private CanvasGroup hardModeCanvasGroup;
    [Header("Resources")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private Image coinImage;
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
        TileManager.winLevelEvent += OnLevelWin;
        ScrewBoxManager.loseLevelEvent += OnLevelLose;
        TileManager.loseLevelEvent += OnLevelLose;
        ScrewsDataManager.updateRemainingScrewEvent += UpdateRemainingScrew;
        ResourceEarnPopup.removeAdsEvent += OnAdsRemoved;
        SaferioIAPManager.removeAdsEvent += OnAdsRemoved;
        UserResourcesManager.updateCoinTextEvent += UpdateCoinText;

        if (userResourcesObserver != null)
        {
            userResourcesObserver.updateCoinEvent += UpdateCoinText;
        }

        pauseButton.onClick.AddListener(Pause);
        noAdsButton.onClick.AddListener(OpenNoAdsPopup);
        btShop.onClick.AddListener(OpenShopPopup);

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

        _tweens = new List<Tween>();

        if (timerText != null)
        {
            _originalTimerColor = timerText.color;
        }

        InitCoinText();
    }

    private List<Tween> _tweens;
    private float _displayedCoinValue;
    private Tween _numberTween;
    private Tween _scaleTween;
    private bool _isWaitingForCoinFlight;
    private Color _originalTimerColor = Color.white;
    private Tween _timerWarningTween;
    private Tween _screenWarning;


    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStarted;
        LevelLoader.setLevelNumberEvent -= SetLevelText;
        ScrewManager.updateRemainingScrewEvent -= UpdateRemainingScrew;
        ScrewManager.winLevelEvent -= OnLevelWin;
        TileManager.winLevelEvent -= OnLevelWin;
        ScrewBoxManager.loseLevelEvent -= OnLevelLose;
        TileManager.loseLevelEvent -= OnLevelLose;
        ScrewsDataManager.updateRemainingScrewEvent -= UpdateRemainingScrew;
        ResourceEarnPopup.removeAdsEvent -= OnAdsRemoved;
        SaferioIAPManager.removeAdsEvent -= OnAdsRemoved;
        UserResourcesManager.updateCoinTextEvent -= UpdateCoinText;

        if (userResourcesObserver != null)
        {
            userResourcesObserver.updateCoinEvent -= UpdateCoinText;
        }

        StopTimerWarning();
        CommonUtil.StopAllTweens(_tweens);
    }

    #endregion

    #region Private Methods

    private void OnLevelWin()
    {
        _isWaitingForCoinFlight = true;
        StopTimerWarning();
    }

    private void OnLevelLose()
    {
        StopTimerWarning();
    }

    #endregion

    #region Button Events

    private void Pause()
    {
        if (TileManager.Ins != null && TileManager.Ins.isCurrentLevelTutorialActive) return;
        switchRouteEvent?.Invoke(ScreenRoute.Pause);
    }

    private void OpenNoAdsPopup()
    {
        if (TileManager.Ins != null && TileManager.Ins.isCurrentLevelTutorialActive) return;
        switchRouteEvent?.Invoke(ScreenRoute.RemoveAd);
    }
    private void OpenShopPopup()
    {
        if (TileManager.Ins != null && TileManager.Ins.isCurrentLevelTutorialActive) return;
        switchRouteEvent?.Invoke(ScreenRoute.IAPShopPopup);
    }

    private void OpenInventoryPopup()
    {
        if (TileManager.Ins != null && TileManager.Ins.isCurrentLevelTutorialActive) return;
        switchRouteEvent?.Invoke(ScreenRoute.Inventory);
    }

    #endregion
    private void Update()
    {
        if (TileManager.Ins != null && TileManager.Ins.levelItem != null)
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

            bool hasTimeLimit = TileManager.Ins != null && TileManager.Ins.levelItem != null && TileManager.Ins.levelItem.timeLimit > 0;
            if (hasTimeLimit && TileManager.Ins.isLevelActive)
            {
                noLimited.SetActive(false);
                timerText.gameObject.SetActive(true);
                if (timeToDisplay <= warningTimeLimit && timeToDisplay > 0)
                {
                    if (!_timerWarningTween.isAlive)
                    {
                        timerText.color = Color.red;
                        _timerWarningTween = Tween.Scale(timerText.transform, 1.1f, duration: 0.5f, cycles: -1, cycleMode: CycleMode.Yoyo);
                        _screenWarning = Tween.Alpha(warningGroup, 1, duration: 0.5f, cycles: -1, cycleMode: CycleMode.Yoyo);
                    }
                }
                else
                {
                    StopTimerWarning();
                }
            }
            else
            {
                noLimited.SetActive(true);
                timerText.gameObject.SetActive(false);
                StopTimerWarning();
            }
        }
    }

    public void StopTimerWarning()
    {
        if (_timerWarningTween.isAlive)
        {
            _timerWarningTween.Stop();
        }
        if (timerText != null)
        {
            timerText.transform.localScale = Vector3.one;
            timerText.color = _originalTimerColor;
        }
        if (warningGroup != null)
        {
            Tween.Alpha(warningGroup, 0, duration: 0.5f);
            _screenWarning.Stop();
        }
    }
    private void OpenDebugPopupButton()
    {
        if (TileManager.Ins != null && TileManager.Ins.isCurrentLevelTutorialActive) return;
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
        _isWaitingForCoinFlight = false;
        StopTimerWarning();
        InitCoinText();
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

    private void InitCoinText()
    {
        if (userResourcesObserver != null)
        {
            userResourcesObserver.Load();
            if (userResourcesObserver.UserResources != null)
            {
                _displayedCoinValue = userResourcesObserver.UserResources.CoinQuantity;
                UpdateCoinText(_displayedCoinValue, isPlayAnimation: false);
            }
        }
    }

    private void UpdateCoinText()
    {
        if (_isWaitingForCoinFlight) return;

        if (userResourcesObserver != null)
        {
            userResourcesObserver.Load();
            if (userResourcesObserver.UserResources != null)
            {
                UpdateCoinText(userResourcesObserver.UserResources.CoinQuantity, isPlayAnimation: true);
            }
        }
    }

    private void UpdateCoinText(float value)
    {
        UpdateCoinText(value, isPlayAnimation: true);
    }

    private void UpdateCoinText(float value, bool isPlayAnimation)
    {
        if (coinText == null) return;

        if (userResourcesObserver != null && userResourcesObserver.UserResources != null)
        {
            if (value >= userResourcesObserver.UserResources.CoinQuantity)
            {
                _isWaitingForCoinFlight = false;
            }
        }

        if (isPlayAnimation && value > _displayedCoinValue)
        {
            if (_numberTween.isAlive)
            {
                _numberTween.Stop();
            }

            float startVal = _displayedCoinValue;
            _numberTween = Tween.Custom(startVal, value, duration: 0.15f, onValueChange: newVal =>
            {
                _displayedCoinValue = newVal;
                coinText.text = $"{(int)newVal}";
            });
            _tweens.Add(_numberTween);

            if (_scaleTween.isAlive)
            {
                _scaleTween.Stop();
            }
            coinText.transform.localScale = Vector3.one;
            _scaleTween = Tween.Scale(coinText.transform, 1.2f, cycles: 2, cycleMode: CycleMode.Yoyo, duration: 0.15f);
            _tweens.Add(_scaleTween);
        }
        else
        {
            if (_numberTween.isAlive)
            {
                _numberTween.Stop();
            }
            _displayedCoinValue = value;
            coinText.text = $"{(int)value}";

            if (isPlayAnimation)
            {
                if (_scaleTween.isAlive)
                {
                    _scaleTween.Stop();
                }
                coinText.transform.localScale = Vector3.one;
                _scaleTween = Tween.Scale(coinText.transform, 1.2f, cycles: 2, cycleMode: CycleMode.Yoyo, duration: 0.15f);
                _tweens.Add(_scaleTween);
            }
        }
    }

    public Vector2 GetCoinTextCanvasLocalPosition()
    {
        if (coinImage == null) return Vector2.zero;
        return ((Vector2)coinImage.transform.position - 0.5f * GamePersistentVariable.canvasSize) / GamePersistentVariable.canvasScale;
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
