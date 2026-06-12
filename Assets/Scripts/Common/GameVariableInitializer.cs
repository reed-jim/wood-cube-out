using System;
using System.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameVariableInitializer : MonoBehaviour
{
    [SerializeField] private RectTransform canvas;

    CanvasScaler canvasScaler;
    [SerializeField] private Vector2Variable canvasSize;
    [SerializeField] private Vector2Variable canvasSizeAfterCanvasScaler;
    [SerializeField] private Vector2Variable canvasScale;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private GameSetting gameSetting;
    [SerializeField] private GoldPassDataContainer goldPassDataContainer;

    private Vector2 _cachedCanvasSize;

    public static event Action currentLevelFetchedEvent;
    public static event Action gameSettingLoadedEvent;

    private void Awake()
    {
        ScriptableObjectInitializer.getCachedCanvasSizeEvent += GetCachedCanvasSize;

        PrimeTweenConfig.warnTweenOnDisabledTarget = false;
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        PrimeTweenConfig.warnZeroDuration = false;

        canvasSize.Value = canvas.sizeDelta;

        GamePersistentVariable.canvasSize = canvas.sizeDelta;

        currentLevel.Load();
        userResourcesObserver.Load();

        LoadGameSetting();

        _cachedCanvasSize = canvas.sizeDelta;

        if (currentLevel.Value > 1)
        {
            GamePersistentVariable.isPassedStepOneTutorial = true;
        }

        GamePersistentVariable.chestQueue = new System.Collections.Generic.Queue<string>();

        GamePersistentVariable.currentLevel = currentLevel.Value;
        GamePersistentVariable.doubleInterAdsConfig = new DoubleInterAdsConfig();
        GamePersistentVariable.livesConfig = new LivesConfig();
        GamePersistentVariable.dailyRewardConfig = new DailyRewardConfig();

        GamePersistentVariable.coinChallengeConfig = new CoinChallengeConfig();

        GamePersistentVariable.longTermUserDifficultyConfig = new LongTermUserDifficultyConfig();

        GamePersistentVariable.specialLevelConfig = new SpecialLevelConfig()
        {
            isEnable = true
        };

        GamePersistentVariable.gameplayBackgroundConfig = new GameplayBackgroundConfig();
    }

    private void Start()
    {
        canvasSizeAfterCanvasScaler.Value = canvas.sizeDelta;
        canvasScale.Value = canvas.localScale;

        GamePersistentVariable.canvasScale = canvasScale.Value;
        GamePersistentVariable.canvasSizeAfterCanvasScaler = canvas.sizeDelta;

        if (DataUtility.Load(GameConstants.IS_UNLOCKED_ALL_SCREW_BOXES, false))
        {
            GameConstants.DEFAULT_INITIAL_NUMBER_SCREW_BOXES = 4;
        }

        GoldPassData savedGoldPassData = DataUtility.Load(GameConstants.GOLD_PASS_DATA_CONTAINER, new GoldPassData());

        GamePersistentVariable.goldPassData = savedGoldPassData;

        if (savedGoldPassData.endTimestamp == 0)
        {
            savedGoldPassData.endTimestamp =
                new DateTimeOffset(DateTime.Now).AddDays(goldPassDataContainer.numDays).ToUnixTimeSeconds();

            GamePersistentVariable.goldPassData.goldPassMilestones = goldPassDataContainer.goldPassMilestones;

            DataUtility.Save(GameConstants.GOLD_PASS_DATA_CONTAINER, savedGoldPassData);

            GamePersistentVariable.goldPassData = savedGoldPassData;
        }

        currentLevelFetchedEvent?.Invoke();
    }

    void OnDestroy()
    {
        ScriptableObjectInitializer.getCachedCanvasSizeEvent -= GetCachedCanvasSize;
    }

    private async void LoadGameSetting()
    {
        await gameSetting.LoadAsync();

        Tween.Delay(1).OnComplete(() =>
        {
            SoundManager.Instance.OnGameSettingLoaded();
        });

        GamePersistentVariable.IsVibrate = gameSetting.IsVibrate;
    }

    private void GetCachedCanvasSize()
    {
        canvasSize.Value = _cachedCanvasSize;
    }
}
