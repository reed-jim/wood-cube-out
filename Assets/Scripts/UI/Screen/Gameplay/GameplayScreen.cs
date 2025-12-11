using System;
using System.Threading.Tasks;
using Lean.Localization;
using NamPhuThuy.Common;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class GameplayScreen : Singleton<GameplayScreen>
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button openShopButton;
    [SerializeField] private Button skipLevelButton;
    [SerializeField] private Button openDebugPopupButton;
    [SerializeField] private Button debugNextLevelButton;
    [SerializeField] private Button debugPrevLevelButton;
    [SerializeField] private TMP_Text remainingScrewText;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedLevelText;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private GameSetting gameSetting;

    [Header("HARD MODE")]
    [SerializeField] private CanvasGroup hardModeCanvasGroup;

    public static event Action<ScreenRoute> switchRouteEvent;
    public static event Action nextLevelEvent;
    public static event Action prevLevelEvent;
    public static event Action skipLevelEvent;
    
    public static event Action triggerUpdateBackgroundEvent;

    #region MonoBehaviour Callbacks

    protected override void MoreActionOnAwake()
    {
        YarnLevelLoader.startLevelEvent += OnLevelStarted;
        YarnLevelLoader.setLevelNumberEvent += SetLevelText;
        PieceManager.updateRemainingPieceEvent += UpdateRemainingScrew;
        ScrewsDataManager.updateRemainingScrewEvent += UpdateRemainingScrew;
        YarnLevelLoader.isTutAvailableEvent += CheckTutAvailable;

        pauseButton.onClick.AddListener(Pause);
        openShopButton.onClick.AddListener(OpenShop);
        skipLevelButton.onClick.AddListener(SkipLevel);
        openDebugPopupButton.onClick.AddListener(OpenDebugPopupButton);
        // debugNextLevelButton.onClick.AddListener(NextLevel);
        // debugPrevLevelButton.onClick.AddListener(PrevLevel);

        if (!gameSetting.IsDebug)
        {
            openDebugPopupButton.gameObject.SetActive(false);
        }

        skipLevelButton.gameObject.SetActive(false);
        triggerUpdateBackgroundEvent?.Invoke();
    }

    private void OnDestroy()
    {
        YarnLevelLoader.startLevelEvent -= OnLevelStarted;
        YarnLevelLoader.setLevelNumberEvent -= SetLevelText;
        PieceManager.updateRemainingPieceEvent -= UpdateRemainingScrew;
        ScrewsDataManager.updateRemainingScrewEvent -= UpdateRemainingScrew;
        YarnLevelLoader.isTutAvailableEvent -= CheckTutAvailable;
    }

    #endregion

    private void Pause()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Pause);
    }

    private void OpenShop()
    {
        switchRouteEvent?.Invoke(ScreenRoute.IAPShopPopup);
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
        if (GamePersistentVariable.isAllowedSkipLevel)
        {
            skipLevelButton.gameObject.SetActive(true);
        }
        else
        {
            skipLevelButton.gameObject.SetActive(false);
        }
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

    #region Events Listen

    private void CheckTutAvailable(bool isTutAvailable)
    {
        DebugLogger.Log(message:$"isTutAvailable: {isTutAvailable}", context:this);
        if (isTutAvailable)
        {
            pauseButton.gameObject.SetActive(false);
            openShopButton.gameObject.SetActive(false);
        }
        else
        {
            pauseButton.gameObject.SetActive(true);
            openShopButton.gameObject.SetActive(true);
        }
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
