using System;
using System.Threading.Tasks;
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

    public static event Action currentLevelFetchedEvent;
    public static event Action gameSettingLoadedEvent;

    private void Awake()
    {
        canvasSize.Value = canvas.sizeDelta;

        currentLevel.Load();
        userResourcesObserver.Load();

        LoadGameSetting();

        currentLevelFetchedEvent?.Invoke();

        FetchRemoteConfigAsync();
    }

    private async Task FetchRemoteConfigAsync()
    {
        // RemoteConfigController.FetchDataAsync();

        // Debug.Log("SAFERIO RC " + RemoteConfigController.GetStringConfig(FirebaseConfig.DIFFICULTY_MULTIPLIER, ""));
        // Debug.Log("SAFERIO RC " + RemoteConfigController.GetStringConfig(FirebaseConfig.MINIMUM_TIME_SHOW_COLLAPSE, ""));

        // Debug.Log("SAFERIO RC " + RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_INTERSTITIAL, false));
        // Debug.Log("SAFERIO RC " + RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_APP_OPEN_ADS, false));
        // Debug.Log("SAFERIO RC " + RemoteConfigController.GetFloatConfig(FirebaseConfig.DIFFICULTY_MULTIPLIER, 0));

        // Debug.Log("SAFERIO RC " + RemoteConfigController.GetFloatConfig(FirebaseConfig.MINIMUM_TIME_SHOW_COLLAPSE, 0));
        // Debug.Log("SAFERIO RC " + RemoteConfigController.GetFloatConfig(FirebaseConfig.COOL_DOWN_SHOW_COLLAP_BANNER, 0));
        // Debug.Log("SAFERIO RC " + RemoteConfigController.GetFloatConfig(FirebaseConfig.RELOAD_BANNER_COLLAPSE_TIME, 0));
        // Debug.Log("SAFERIO RC " + RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_ADMOB_BANNER_COLLAP, false));
        // Debug.Log("SAFERIO RC " + RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_ADMOB_BANNER, false));
        // Debug.Log("SAFERIO RC " + RemoteConfigController.GetFloatConfig(FirebaseConfig.COOLDOWN_ADMOB_REFRESH, 0));
    }

    private void Start()
    {
        canvasSizeAfterCanvasScaler.Value = canvas.sizeDelta;
        canvasScale.Value = canvas.localScale;
    }

    private async void LoadGameSetting()
    {
        await gameSetting.LoadAsync();

        gameSettingLoadedEvent?.Invoke();
    }
}
