using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Firebase.RemoteConfig;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine.Events;
using Firebase.Extensions;
public class RemoteConfigController : MonoBehaviour
{
    #region Variables

    //private static bool LoadedConfig;
    //private static bool LoadingConfig;
    private static bool isInit;

    private static Dictionary<string, string> playfabConfig = new Dictionary<string, string>();
    private static HashSet<string> firebaseRemoteKeys = new HashSet<string>();
    public static bool InitSuccess;
    public static event Action remoteConfigReadyEvent;
    #endregion Variables

    #region Public Methods

    public static bool isFetchDone;

    public static async Task FetchData()
    {
        //Context.Waiting.ShowWaiting("Fetching data...");
        DebugLog("Fetching data...");
        // FetchAsync only fetches new data if the current data is older than the provided
        // timespan.  Otherwise it assumes the data is "recent enough", and does nothing.
        // By default the timespan is 12 hours, and for production apps, this is a good
        // number.  For this example though, it's set to a timespan of zero, so that
        // changes in the console will always show up immediately.
#if TEST
        System.Threading.Tasks.Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(
            TimeSpan.FromSeconds(30));
#else
        System.Threading.Tasks.Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(
            TimeSpan.FromHours(6));
#endif
        await fetchTask.ContinueWith(FetchComplete);
    }

    public static void FetchDataNow()
    {
        //Context.Waiting.ShowWaiting("Fetching data...");
        DebugLog("Fetching data Now...");
        // FetchAsync only fetches new data if the current data is older than the provided
        // timespan.  Otherwise it assumes the data is "recent enough", and does nothing.
        // By default the timespan is 12 hours, and for production apps, this is a good
        // number.  For this example though, it's set to a timespan of zero, so that
        // changes in the console will always show up immediately.

        System.Threading.Tasks.Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(
            TimeSpan.FromSeconds(0));
        fetchTask.ContinueWith(FetchComplete);
    }


    public static string GetStringConfig(string key, string defaultValue)
    {
        if (!firebaseRemoteKeys.Contains(key)) return defaultValue;
        //if (!defaults.ContainsKey(key)) return defaultValue;+
        return FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
    }


    public static bool GetBoolConfig(string key, bool defaultValue)
    {
        if (!firebaseRemoteKeys.Contains(key)) return defaultValue;
        return FirebaseRemoteConfig.DefaultInstance.GetValue(key).BooleanValue;
    }


    public static float GetFloatConfig(string key, float defaultValue)
    {
        if (!firebaseRemoteKeys.Contains(key)) return defaultValue;
        string val = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
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
        string val = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
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
            string.IsNullOrEmpty(input = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue))
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

    public static bool PlayfabJsonConfig<T>(string key, out T result)
    {
        string input;
        if (!playfabConfig.ContainsKey(key) ||
            string.IsNullOrEmpty(input = playfabConfig[key]))
        {
            result = default;
            return false;

        }

        try
        {
            result = JsonConvert.DeserializeObject<T>(input);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"GetJsonConfig {typeof(T)} , key {key}, exception: {ex.Message}");
            result = default;
            return false;
        }
    }

    public static string PlayfabStringConfig(string key, string defaultValue)
    {
        return playfabConfig.ContainsKey(key) ? playfabConfig[key] : defaultValue;
    }

    public static int PlayfabIntConfig(string key, int defaultVal)
    {
        try
        {
            return playfabConfig.ContainsKey(key) ? int.Parse(playfabConfig[key]) : defaultVal;
        }
        catch (Exception)
        {
            return defaultVal;
        }
    }

    public static float PlayfabFloatConfig(string key, float defaultVal)
    {
        try
        {
            return playfabConfig.ContainsKey(key) ? float.Parse(playfabConfig[key], CultureInfo.InvariantCulture) : defaultVal;
        }
        catch (Exception)
        {
            return defaultVal;
        }
    }

    public static bool PlayfabBoolConfig(string key, bool defaultVal)
    {
        try
        {
            return playfabConfig.ContainsKey(key) ? int.Parse(playfabConfig[key]) == 1 : defaultVal;
        }
        catch (Exception)
        {
            return defaultVal;
        }
    }

    public static bool GetRocketJsonConfig<T>(string key, out T result)
    {
        string input;
        if (!playfabConfig.ContainsKey(key) ||
            string.IsNullOrEmpty(input = playfabConfig[key]))
        {
            result = default;
            return false;
        }

        try
        {
            result = JsonConvert.DeserializeObject<T>(input);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"GetJsonConfig {typeof(T)} , key {key}, exception: {ex.Message}");
            result = default;
            return false;
        }
    }

    #endregion Public Methods

    #region Private Methods

    public static void OnLoadConfig(Dictionary<string, string> Data)
    {
        //if (Data != null)
        //{
        //    playfabConfig = Data;
        //    MessageBroker.Default.Publish(new ReloadPlayfabConfig());
        //    RocketObservable.RocketConfigReload.OnNext(Unit.Default);
        //    //PlayerPrefs.SetString("CONFIG_CACHED", JsonUtility.ToJson(Data));
        //}
        //MzU.ConfigOnlineServer(GetIntConfig("Photon_Server", 2));
    }

    private static void FetchComplete(Task fetchTask)
    {
        if (fetchTask.IsCanceled)
        {
            DebugLog("Fetch canceled.");
            isFetchDone = true;
        }
        else if (fetchTask.IsFaulted)
        {
            DebugLog("Fetch encountered an error.");
            isFetchDone = true;
        }
        else if (fetchTask.IsCompleted)
        {
            DebugLog("Fetch completed successfully!");
        }
        //Context.Waiting.HideWaiting();
        var info = FirebaseRemoteConfig.DefaultInstance.Info;
        switch (info.LastFetchStatus)
        {
            case LastFetchStatus.Success:
                FetchAndActivateAsync(fetchTask, info);
                break;

            case LastFetchStatus.Failure:
                switch (info.LastFetchFailureReason)
                {
                    case FetchFailureReason.Error:
                        DebugLog("Fetch failed for unknown reason");
                        break;

                    case FetchFailureReason.Throttled:
                        DebugLog("Fetch throttled until " + info.ThrottledEndTime);
                        break;
                }
                isFetchDone = true;
                break;

            case LastFetchStatus.Pending:
                DebugLog("Latest Fetch call still pending.");
                isFetchDone = true;
                break;
        }

        //AdmobAds.Instance.Init();
    }

    public static async void FetchAndActivateAsync(Task fetchTask, ConfigInfo info)
    {
        await FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync();
        if (!fetchTask.IsFaulted && fetchTask.IsCompleted)
        {
            ReloadFirebaseKeys();
        }
        isFetchDone = true;

        DebugLog(String.Format("Remote data loaded and ready (last fetch time {0}).", info.FetchTime));
    }


    //static Dictionary<string, object> defaults = new Dictionary<string, object>();
    public static void RemoteConfigFirebaseInit()
    {
        if (isInit) return;
        isInit = true;
        InitSuccess = true;
        // These are the values that are used if we haven't fetched data from the
        // server
        // yet, or if we ask for values that the server doesn't have:

        //FirebaseRemoteConfig.SetDefaults(defaults);
        DebugLog("RemoteConfig configured and ready!");

        //try
        //{
        //    string config_cached = PlayerPrefs.GetString("CONFIG_CACHED", string.Empty);
        //    if (!string.IsNullOrEmpty(config_cached))
        //    {
        //        Debug.Log("RemoteConfigFirebaseInit" + config_cached);
        //        playfabConfig = JsonUtility.FromJson<Dictionary<string, string>>(config_cached);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    DebugManager.LogError(ex.Message);
        //}
    }

    private static void DebugLog(string s)
    {
        Debug.Log(s);
    }

    #endregion Private Methods


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
        GamePersistentVariable.levelShowBanner = GetIntConfig(FirebaseConfig.LEVEL_UNLOCK_BANNER, 3);
        GamePersistentVariable.levelShowCollapseBanner = GetIntConfig(FirebaseConfig.LEVEL_UNLOCK_COLLAPSE_BANNER, 5);

        #region CORE GAMEPLAY CONFIG
        GamePersistentVariable.screwHoleLeftToLose = /*GetIntConfig(FirebaseConfig.SCREW_HOLE_LEFT_TO_LOSE, 1)*/1;
        #endregion

        #region DIFFICULTY
        LongTermUserDifficultyConfig longTermUserDifficultyConfig = new LongTermUserDifficultyConfig();

        GetJsonConfig(FirebaseConfig.longTermUserDifficultyConfig, out longTermUserDifficultyConfig);

        if (longTermUserDifficultyConfig != null)
        {
            GamePersistentVariable.longTermUserDifficultyConfig = longTermUserDifficultyConfig;
        }
        #endregion

        #region SPECIAL LEVEL
        SpecialLevelConfig specialLevelConfig = new SpecialLevelConfig();

        GetJsonConfig(FirebaseConfig.specialLevelConfig, out specialLevelConfig);

        if (specialLevelConfig != null)
        {
            GamePersistentVariable.specialLevelConfig = specialLevelConfig;
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

        #region LOOP LEVEL
        GamePersistentVariable.isDisableLoopLevel = GetBoolConfig(FirebaseConfig.isDisableLoopLevel, false);

        // if (Application.systemLanguage == SystemLanguage.Vietnamese)
        // {
        //     GamePersistentVariable.isDisableLoopLevel = true;
        // }
        #endregion

        #region Remote Config Gameplay
        GameplayBackgroundConfig gameplayBackgroundConfig = new GameplayBackgroundConfig();

        GetJsonConfig(FirebaseConfig.gameplayBackgroundConfig, out gameplayBackgroundConfig);

        if (gameplayBackgroundConfig != null)
        {
            GamePersistentVariable.gameplayBackgroundConfig = gameplayBackgroundConfig;
        }
        #endregion

        remoteConfigReadyEvent?.Invoke();

        int targetFPS = GetIntConfig(FirebaseConfig.TARGET_FPS, 120);

        Application.targetFrameRate = targetFPS;

        SetRemoteConfigRewardedAds();
    }

    private static void SetRemoteConfigRewardedAds()
    {
        GamePersistentVariable.isEnableRewardedAds = GetBoolConfig(FirebaseConfig.IS_ENABLE_REWARDED_ADS, true);

        GamePersistentVariable.minLevelDisableRewardedAds = GetIntConfig(FirebaseConfig.MIN_LEVEL_DISABLE_REWARDED_ADS, 20);

        if (!GamePersistentVariable.isEnableRewardedAds)
        {
            GamePersistentVariable.doubleInterAdsConfig.isEnable = false;
            GamePersistentVariable.isShowBanner = false;
        }
    }
    #endregion
}


