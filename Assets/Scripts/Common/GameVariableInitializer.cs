using System;
using System.Collections.Generic;
using NamPhuThuy.Common;
using NUnit.Framework.Constraints;
using PrimeTween;
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

    #region MonoBehaviour Callbacks

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

        GamePersistentVariable.chestQueue = new Queue<string>();

        GamePersistentVariable.currentLevel = currentLevel.Value;
        GamePersistentVariable.doubleInterAdsConfig = new DoubleInterAdsConfig()
        { 
            isEnable = true,
            isEnableMrec = true,
            isEnableDoubleInter = true, 
            isEnableInterPopupInteract = true,
        };
        
        #region Picture Reward Config
        
        GalleryRewardConfigItem[] items = new GalleryRewardConfigItem[2] {
            new GalleryRewardConfigItem(1, 7, 1),
            new GalleryRewardConfigItem(8, 64,3)
        };
        
        // Debug to console
        DebugLogger.LogFrog(message: $"Created GalleryRewardConfigItem array, length: {items.Length}");
        for (int i = 0; i < items.Length; i++)
        {
            GalleryRewardConfigItem item = items[i];
            // Adjust field/property names below to match your actual class definition
            DebugLogger.LogFrog(message: $"Item[{i}] - minLevel: {item.StartLevel}, maxLevel: {item.EndLevel}, rewardAmount: {item.RequiredNumberOfLevel}");
        }
        
        

        RemoteGalleryRewardConfig galleryRewardConfig = new RemoteGalleryRewardConfig(items);

        GamePersistentVariable.galleryRewardConfig = galleryRewardConfig;
        GamePersistentVariable.remoteConfigIsShowMREC = true;

        #endregion
        
        
        GamePersistentVariable.livesConfig = new LivesConfig();
        GamePersistentVariable.dailyRewardConfig = new DailyRewardConfig();

        GamePersistentVariable.isUseVideoBackground = true;
        
        GamePersistentVariable.coinChallengeConfig = new CoinChallengeConfig();

        GamePersistentVariable.longTermUserDifficultyConfig = new LongTermUserDifficultyConfig();
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

        Debug.Log("SAFERIO Remove Later");
        GamePersistentVariable.livesConfig = new LivesConfig()
        {
            isActive = true,
            maxLive = 5,
            secondReplenishLive = 20 * 60,
        };

        currentLevelFetchedEvent?.Invoke();
    }

    void OnDestroy()
    {
        ScriptableObjectInitializer.getCachedCanvasSizeEvent -= GetCachedCanvasSize;
    }

    #endregion

    private async void LoadGameSetting()
    {
        await gameSetting.LoadAsync();

        gameSettingLoadedEvent?.Invoke();

        GamePersistentVariable.IsVibrate = gameSetting.IsVibrate;
    }

    private void GetCachedCanvasSize()
    {
        canvasSize.Value = _cachedCanvasSize;
    }
}
