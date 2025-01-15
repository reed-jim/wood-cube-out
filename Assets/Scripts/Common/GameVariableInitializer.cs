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
