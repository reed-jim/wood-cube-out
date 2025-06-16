using System.Collections.Generic;
using UnityEngine;

public enum CloudStorageRequest
{
    Upload,
    Download
}

public static class GamePersistentVariable
{
    public static Vector2 canvasSize;
    public static Vector2 canvasSizeAfterCanvasScaler;
    public static Vector2 canvasScale;
    public static string iapInitializeFailReason;

    public static Vector2 screenSizeWorld;

    #region LEVEL OBSERVER
    public static int currentLevel;
    public static int levelPrefab;
    public static bool isFreshLevel;
    public static bool isLoadingOldLevelState;
    public static bool isPreventLoadLevelData;
    public static bool isRemoteLevelsDownloaded;
    public static int numScrewBoxUnlocked;
    public static int numOutsideScrew;
    public static int numHolesAdded;
    public static bool isAllowedSkipLevel;
    public static bool isPassedStepOneTutorial;
    public static float currentTargetOrthographicSize;
    public static Dictionary<int, List<Transform>> pieceDependencies;
    public static Vector3 levelOffset;
    public static Vector3 boxForOutsideScrewScreenPosition;
    public static RaycastHit lastScrewSelectedSurfaceHit;
    #endregion

    #region LEVEL IN PROGRESS
    public static bool isEnableLoadingLevelInProgress;
    public static bool isLoadingLevelInProgress;
    #endregion

    #region GAME FLOW
    public static bool hasAppJustLaunched;
    public static bool isLoadingComplete;
    public static bool isPendingReplay;
    public static bool isPendingReturnHome;
    public static bool isMenuLoading;
    public static bool hasPlayerReachedGameplay;
    #endregion

    #region LIVES
    public static LivesData livesData;
    public static bool isLevelDirty;
    #endregion

    #region AUTHENTICATION
    public static string FirebaseUserId;
    #endregion

    #region BACKUP
    public static CloudStorageRequest cloudStorageRequest;
    public static bool isCloudStorageOperationCompleted;
    #endregion

    #region AUTO TEST
    public static bool isAutoTestingLevel;
    #endregion

    #region SETTING
    public static bool IsVibrate;
    #endregion

    #region SOUND
    public static bool isSoundTightenScrewPlaying;
    #endregion

    #region FIREBASE
    public static bool isFirebaseAvailable;
    public static bool isFirebaseRemoteConfigLoaded;
    #endregion

    #region ADS
    public static bool isShowBanner;
    public static bool remoteConfigIsShowMREC;
    public static int bannerCooldown = 30;
    public static bool isEnableRewardedAds;
    public static int minLevelDisableRewardedAds;
    public static ActionWatchVideo currentReasonRewardedAd;
    public static DoubleInterAdsConfig doubleInterAdsConfig;
    #endregion

    #region LIVES SYSTEM
    public static LivesConfig livesConfig;
    #endregion

    #region IAP
    public static string IAPLocation;
    public static int IAPAddition;
    public static bool isUnlockedAllScrewBoxes;
    public static SaleIAPPackType saleIAPPackType;
    public static Dictionary<SaleIAPPackType, bool> isShowSalePacks;
    #endregion

    #region LEADERBOARD
    public static int selectedCountryIndex;
    public static GameEnum.LeaderboardScope leaderboardScope;
    #endregion

    #region EVENT
    public static Queue<string> chestQueue;
    public static bool isChestOpenning;
    #endregion

    #region DAILY REWARD
    public static int currentDailyRewardIndex;
    public static bool isCurrentDailyRewardIndexLoaded;
    public static DailyRewardConfig dailyRewardConfig;
    #endregion

    #region GOLD PASS
    public static bool isGoldPassUnlocked;
    public static int goldPassMultiplier = 10;
    public static GoldPassData goldPassData;
    #endregion

    #region MEDAL TOURNAMENT
    public static MedalTournamentData medalTournamentData;
    #endregion

    #region WEEKLY CHALLENGE
    public static WeeklyChallengeData weeklyChallengeData;
    #endregion

    #region SPACE RACE
    public static int selfSpaceRaceRank;
    #endregion

    #region COIN CHALLENGE
    public static int coinChallengeItemGainedInLevel;
    public static CoinChallengeConfig coinChallengeConfig;
    #endregion

    #region SORT PUZZLE
    public static int numItemsLeft;
    #endregion

    #region CHEST
    public static bool isStarChestClaimable;
    public static bool isLevelChestClaimable;
    public static string currentChestReason;
    public static bool isClaimedLevelChest;
    #endregion
}
