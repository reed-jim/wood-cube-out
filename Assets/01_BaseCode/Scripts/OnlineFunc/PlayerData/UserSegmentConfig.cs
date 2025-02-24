using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ConfigGamePlayFab
{
    public ConfigGamePlayFab()
    {
        
    }
}


public class UserSegmentConfig
{
    public static ConfigGamePlayFab configPlayFab { get; private set; }
    public static bool isLoadedConfig;


    public static void LoadPlayFabConfig(UnityAction actionDone = null, Action onError = null)
    {
        if (isLoadedConfig)
        {
            actionDone?.Invoke();
            return;
        }

        RemoteConfigController.GetPlayFabConfig<ConfigGamePlayFab>("ConfigGame", dt =>
        {
            configPlayFab = new ConfigGamePlayFab
            {
                
            };
            isLoadedConfig = true;
            actionDone?.Invoke();
        }, e =>
        {
#if LOG_ERROR
            Debug.LogError("Get PlayFab Config Error: " + e.ErrorMessage);
#endif
            onError?.Invoke();
        }, 0);
    }
}