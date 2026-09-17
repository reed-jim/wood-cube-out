using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameEnum;

public class MultiverseIdleMenuScreen : Singleton<MultiverseIdleMenuScreen>
{
    public Button playButton;
    public Button selectThemeButton;

    [SerializeField] private AudioListener menuScreenAudioListener;

    private bool _isGameStarted;

    public static event Action<ScreenRoute> switchRouteEvent;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        SceneManager.sceneLoaded += OnSceneLoaded;

        playButton.onClick.AddListener(ToGameplay);
        selectThemeButton.onClick.AddListener(OpenSelectThemePopup);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void ToGameplay()
    {
        if (_isGameStarted)
        {
            return;
        }

        Addressables.LoadSceneAsync(GameConstants.GAMEPLAY_SCENE, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        _isGameStarted = true;

        playButton.interactable = false;
    }

    private void OpenSelectThemePopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.SelectTheme);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameConstants.GAMEPLAY_SCENE)
        {
            menuScreenAudioListener.enabled = false;

            SceneManager.UnloadSceneAsync(GameConstants.MENU_SCENE);
        }
    }
}
