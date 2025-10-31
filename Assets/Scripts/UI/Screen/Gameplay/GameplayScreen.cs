using System;
using System.Threading.Tasks;
using Lean.Localization;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class GameplayScreen : Singleton<GameplayScreen>
{
    [Header("Buttons")]
    [SerializeField] private Button helpButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button skipLevelButton;
    [SerializeField] private Button openDebugPopupButton;
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

    #region MonoBehaviour Callbacks

    protected override void MoreActionOnAwake()
    {
        LevelLoader.startLevelEvent += OnLevelStarted;
        LevelLoader.setLevelNumberEvent += SetLevelText;
        ScrewManager.updateRemainingScrewEvent += UpdateRemainingScrew;
        ScrewManager.winLevelEvent += OnLevelWin;
        ScrewBoxManager.loseLevelEvent += OnLevelLose;
        ScrewsDataManager.updateRemainingScrewEvent += UpdateRemainingScrew;

        LevelLoader.specialTutIntroduceEvent += ToggleHelpButton;
        

        pauseButton.onClick.AddListener(Pause);
        helpButton.onClick.AddListener(OnClickHelp);
        
        skipLevelButton.onClick.AddListener(SkipLevel);
        openDebugPopupButton.onClick.AddListener(OpenDebugPopupButton);
        // debugNextLevelButton.onClick.AddListener(NextLevel);
        // debugPrevLevelButton.onClick.AddListener(PrevLevel);

        if (!gameSetting.IsDebug)
        {
            openDebugPopupButton.gameObject.SetActive(false);
        }

        skipLevelButton.gameObject.SetActive(false);
    }


    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStarted;
        LevelLoader.setLevelNumberEvent -= SetLevelText;
        ScrewManager.updateRemainingScrewEvent -= UpdateRemainingScrew;
        ScrewManager.winLevelEvent -= OnLevelWin;
        ScrewBoxManager.loseLevelEvent -= OnLevelLose;
        ScrewsDataManager.updateRemainingScrewEvent -= UpdateRemainingScrew;
        LevelLoader.specialTutIntroduceEvent -= ToggleHelpButton;

    }

   

    #endregion
    
    #region Private Methods

    private void OnLevelWin()
    {
        helpButton.gameObject.SetActive(false);
        
    }

    private void OnLevelLose()
    {
        helpButton.gameObject.SetActive(false);
    }
    
    private void ToggleHelpButton(bool isActive)
    {
        DebugLogger.LogSimple(message:$"isActive: {isActive}");
        // guard against missing reference or destroyed object
        if (helpButton == null)
        {
            DebugLogger.LogSimple(message: "helpButton is null. Assign it in the GameplayScreen inspector.", color: Color.yellow);
            return;
        }
        if (helpButton.gameObject == null)
        {
            DebugLogger.LogSimple(message: "helpButton GameObject is missing/destroyed.", color: Color.yellow);
            return;
        }

        // only toggle if state changes
        if (helpButton.gameObject.activeSelf != isActive)
        {
            helpButton.gameObject.SetActive(isActive);
        }
    }

    #endregion

    #region Button Events
    
    private async void Pause()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Pause);
    }
    
    private void OnClickHelp()
    {
        switchRouteEvent?.Invoke(ScreenRoute.SpecialLevelTutorial);
    }
    
    #endregion

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

    #region HARD MODE
    public void SwitchToHardMode()
    {
        hardModeCanvasGroup.gameObject.SetActive(true);

        hardModeCanvasGroup.alpha = 0;

        Tween.Alpha(hardModeCanvasGroup, 1, duration: 0.3f);
    }
    #endregion      
}
