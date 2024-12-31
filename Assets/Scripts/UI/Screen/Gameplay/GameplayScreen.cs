using System;
using System.Threading.Tasks;
using Lean.Localization;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class GameplayScreen : MonoBehaviour
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button openDebugPopupButton;
    [SerializeField] private Button debugNextLevelButton;
    [SerializeField] private Button debugPrevLevelButton;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedLevelText;

    [SerializeField] private IntVariable currentLevel;

    public static event Action<ScreenRoute> switchRouteEvent;
    public static event Action nextLevelEvent;
    public static event Action prevLevelEvent;

    void Awake()
    {
        pauseButton.onClick.AddListener(Pause);
        openDebugPopupButton.onClick.AddListener(OpenDebugPopupButton);
        debugNextLevelButton.onClick.AddListener(NextLevel);
        debugPrevLevelButton.onClick.AddListener(PrevLevel);

        localizedLevelText.textTranslatedEvent += OnLevelTextTranslated;
        LevelLoader.startLevelEvent += SetLevelText;
    }

    void OnDestroy()
    {
        localizedLevelText.textTranslatedEvent -= OnLevelTextTranslated;
        LevelLoader.startLevelEvent -= SetLevelText;
    }

    private async void Pause()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Pause);

        await Task.Delay(1000);

        // Time.timeScale = 0;
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

    private void OnLevelTextTranslated()
    {
        localizedLevelText.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{currentLevel.Value}");
    }

    private void SetLevelText()
    {
        localizedLevelText.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{currentLevel.Value}");
    }
}
