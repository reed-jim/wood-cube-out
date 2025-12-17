using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.RemoteConfig;
using System.Threading.Tasks;
using System.Globalization;
using Firebase.Extensions;

public class RemoteConfigController : MonoBehaviour
{
    #region EVENT
    public static event Action remoteConfigReadyEvent;
    #endregion  

    private static RemoteConfigController Instance;

    FirebaseRemoteConfig _remoteConfig;

    private Task FetchAsync()
    {
        var task = FirebaseRemoteConfig.DefaultInstance.FetchAsync(System.TimeSpan.Zero);
        return task.ContinueWithOnMainThread(FetchComplete);
    }

    private void FetchComplete(Task task)
    {
        if (!task.IsCompleted)
        {
            Debug.LogError("Retrieval hasn't finished.");
            return;
        }

        _remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        var info = _remoteConfig.Info;
        if (info.LastFetchStatus != LastFetchStatus.Success)
        {
            Debug.LogError($"{nameof(FetchComplete)} was unsuccessful\n{nameof(info.LastFetchStatus)}: {info.LastFetchStatus}");
            return;
        }

        // Fetch successful. Parameter values must be activated to use.
        _remoteConfig.ActivateAsync()
            .ContinueWithOnMainThread(task =>
            {
                ReloadFirebaseKeys();
            });
    }

    #region Variables
    private static Dictionary<string, string> playfabConfig = new Dictionary<string, string>();
    private static HashSet<string> firebaseRemoteKeys = new HashSet<string>();
    public static bool InitSuccess;
    #endregion Variables

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

            return;
        }
        else
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
            
        }

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(async task =>
        {
            var dependencyStatus = task.Result;

            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                var app = Firebase.FirebaseApp.DefaultInstance;

                Firebase.Crashlytics.Crashlytics.ReportUncaughtExceptionsAsFatal = true;

                FetchAsync();

                SetupAdvancedDataTrackingCostCenter();

                GamePersistentVariable.isFirebaseAvailable = true;
            }
            else
            {
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    #region UTIL
    public static string GetStringConfig(string key, string defaultValue)
    {
        if (!firebaseRemoteKeys.Contains(key)) return defaultValue;
        //if (!defaults.ContainsKey(key)) return defaultValue;+
        return Instance._remoteConfig.GetValue(key).StringValue;
    }

    public static bool GetBoolConfig(string key, bool defaultValue)
    {
        if (!firebaseRemoteKeys.Contains(key)) return defaultValue;
        return Instance._remoteConfig.GetValue(key).BooleanValue;
    }

    public static float GetFloatConfig(string key, float defaultValue)
    {
        if (!firebaseRemoteKeys.Contains(key)) return defaultValue;
        string val = Instance._remoteConfig.GetValue(key).StringValue;
        try
        {
            return float.Parse(val, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    public static int GetIntConfig(string key, int defaultValue)
    {
        if (!firebaseRemoteKeys.Contains(key)) return defaultValue;
        string val = Instance._remoteConfig.GetValue(key).StringValue;
        try
        {
            return int.Parse(val);
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    public static bool GetJsonConfig<T>(string key, out T result, bool debug = false)
    {
        string input;

        if (!firebaseRemoteKeys.Contains(key) ||
            string.IsNullOrEmpty(input = Instance._remoteConfig.GetValue(key).StringValue))
        {
            result = default;
            return false;
        }

        try
        {
            result = JsonUtility.FromJson<T>(input);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"GetJsonConfig {typeof(T)} , key {key}, exception: {ex.Message}");
            result = default;
            return false;
        }
    }
    #endregion

    #region SAFERIO
    public static void ReloadFirebaseKeys()
    {
        firebaseRemoteKeys.Clear();

        foreach (var key in FirebaseRemoteConfig.DefaultInstance.Keys)
        {
            firebaseRemoteKeys.Add(key);
        }

        GamePersistentVariable.isFirebaseRemoteConfigLoaded = true;
        GamePersistentVariable.isEnableLoadingLevelInProgress = GetBoolConfig(FirebaseConfig.IS_ENABLE_LOAD_LEVEL_PROGRESS, true);
        GamePersistentVariable.remoteConfigIsShowMREC = GetBoolConfig(FirebaseConfig.IS_SHOW_MREC, false);
        GamePersistentVariable.bannerCooldown = GetIntConfig(FirebaseConfig.BANNER_COOLDOWN, 30);
        GamePersistentVariable.isShowBanner = GetBoolConfig(FirebaseConfig.IS_SHOW_BANNER, true);
        GamePersistentVariable.levelToSkipMenu = GetIntConfig(FirebaseConfig.LEVEL_TO_SKIP_MENU, 5);

        #region CORE GAMEPLAY CONFIG
        GamePersistentVariable.screwHoleLeftToLose = GetIntConfig(FirebaseConfig.SCREW_HOLE_LEFT_TO_LOSE, 1);
        #endregion

        #region DIFFICULTY
        LongTermUserDifficultyConfig longTermUserDifficultyConfig = new LongTermUserDifficultyConfig();

        GetJsonConfig(FirebaseConfig.longTermUserDifficultyMultiplier, out longTermUserDifficultyConfig);

        if (longTermUserDifficultyConfig != null)
        {
            GamePersistentVariable.longTermUserDifficultyConfig = longTermUserDifficultyConfig;
        }
        #endregion

        #region DOUBLE INTER
        DoubleInterAdsConfig doubleInterAdsConfig = new DoubleInterAdsConfig();

        GetJsonConfig(FirebaseConfig.DOUBLE_INTER_ADS_CONFIG, out doubleInterAdsConfig);

        if (doubleInterAdsConfig != null)
        {
            GamePersistentVariable.doubleInterAdsConfig = doubleInterAdsConfig;
        }
        #endregion

        #region LIVES SYSTEM
        LivesConfig livesConfig = new LivesConfig();

        GetJsonConfig(FirebaseConfig.LIVES_SYSTEM_CONFIG, out livesConfig);

        if (livesConfig != null)
        {
            GamePersistentVariable.livesConfig = livesConfig;
        }
        #endregion

        #region DAILY REWARD
        DailyRewardConfig dailyRewardConfig = new DailyRewardConfig();

        GetJsonConfig(FirebaseConfig.DAILY_REWARD_CONFIG, out dailyRewardConfig);

        if (dailyRewardConfig != null)
        {
            GamePersistentVariable.dailyRewardConfig = dailyRewardConfig;
        }
        #endregion

        #region COIN CHALLENGE
        CoinChallengeConfig coinChallengeConfig = new CoinChallengeConfig();

        GetJsonConfig(FirebaseConfig.COIN_CHALLENGE_CONFIG, out coinChallengeConfig);

        if (coinChallengeConfig != null)
        {
            GamePersistentVariable.coinChallengeConfig = coinChallengeConfig;
        }
        #endregion

        #region LEADERBOARD
        // GamePersistentVariable.isEnableLeaderboard = GetBoolConfig(FirebaseConfig.IS_ENABLE_LEADERBOARD, false);
        // GamePersistentVariable.isEnableLeaderboard = false;
        GamePersistentVariable.useFakeLeaderboardScore = GetBoolConfig(FirebaseConfig.IS_ENABLE_LEADERBOARD, false);
        #endregion

        #region GALLERY

        GamePersistentVariable.isUseVideoBackground = GetBoolConfig(FirebaseConfig.isUseVideoBackground, true);

        #endregion

        remoteConfigReadyEvent?.Invoke();

        int targetFPS = GetIntConfig(FirebaseConfig.TARGET_FPS, 120);

        Application.targetFrameRate = targetFPS;

        SetRemoteConfigRewardedAds();
    }

    private static void SetRemoteConfigRewardedAds()
    {
        GamePersistentVariable.isEnableRewardedAds = GetBoolConfig(FirebaseConfig.ENABLE_REWARDED_ADS, true);
        GamePersistentVariable.minLevelDisableRewardedAds = GetIntConfig(FirebaseConfig.MIN_LEVEL_DISABLE_REWARDED_ADS, 20);

        if (!GamePersistentVariable.isEnableRewardedAds)
        {
            GamePersistentVariable.doubleInterAdsConfig.isEnable = false;
            GamePersistentVariable.isShowBanner = false;
        }
    }
    #endregion

    #region COST CENTER
    private void SetupAdvancedDataTrackingCostCenter()
    {
        string attributionId = AppsFlyerSDK.AppsFlyer.getAppsFlyerId();

        Firebase.Analytics.FirebaseAnalytics.SetUserProperty("attribution_id", attributionId);
    }
    #endregion
}
