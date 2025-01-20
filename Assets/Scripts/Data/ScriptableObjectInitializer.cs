using System;
using UnityEngine;

public class ScriptableObjectInitializer : MonoBehaviour
{
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private GameSetting gameSetting;
    [SerializeField] private UserResourcesObserver userResourcesObserver;

    public static event Action gameSettingLoadedEvent;

    void Awake()
    {
        currentLevel.Load();
        userResourcesObserver.Load();

        LoadGameSetting();
    }

    private async void LoadGameSetting()
    {
        await gameSetting.LoadAsync();

        gameSettingLoadedEvent?.Invoke();
    }
}
