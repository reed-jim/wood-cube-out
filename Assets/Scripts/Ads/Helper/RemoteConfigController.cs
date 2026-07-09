using System;
using UnityEngine;

public class RemoteConfigController : MonoBehaviour
{
    public static event Action remoteConfigReadyEvent;

    private static RemoteConfigController Instance;
    public static bool InitSuccess = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        GamePersistentVariable.isFirebaseAvailable = false;

        // Initialize default configs immediately in Awake to prevent NullReferenceException in LevelLoader.Awake
        GamePersistentVariable.specialLevelConfig = new SpecialLevelConfig { isEnable = false };
        GamePersistentVariable.longTermUserDifficultyConfig = new LongTermUserDifficultyConfig();
    }

    private void Start()
    {
        GamePersistentVariable.isFirebaseRemoteConfigLoaded = true;
        GamePersistentVariable.isEnableLoadingLevelInProgress = true;
        GamePersistentVariable.remoteConfigIsShowMREC = false;
        GamePersistentVariable.bannerCooldown = 30;
        GamePersistentVariable.isShowBanner = false;
        GamePersistentVariable.screwHoleLeftToLose = 1;
        
        remoteConfigReadyEvent?.Invoke();
    }

    public static string GetStringConfig(string key, string defaultValue)
    {
        return defaultValue;
    }

    public static bool GetBoolConfig(string key, bool defaultValue)
    {
        if (key == FirebaseConfig.IS_ENABLE_LOAD_LEVEL_PROGRESS) return true;
        if (key == FirebaseConfig.IS_SHOW_BANNER) return false;
        return defaultValue;
    }

    public static float GetFloatConfig(string key, float defaultValue)
    {
        return defaultValue;
    }

    public static int GetIntConfig(string key, int defaultValue)
    {
        return defaultValue;
    }

    public static bool GetJsonConfig<T>(string key, out T result, bool debug = false)
    {
        result = default;
        return false;
    }
}
