using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class PausePopup : BasePopup
{
    [SerializeField] private Button returnHomeButton;
    [SerializeField] private Button replayLevelButton;
    [SerializeField] private SaferioToggle turnMusicToggle;
    [SerializeField] private SaferioToggle turnSoundToggle;

    [SerializeField] private GameSetting gameSetting;

    public static event Action<bool> enableBackgroundMusicEvent;
    public static event Action<bool> enableGameSoundEvent;
    public static event Action replayLevelEvent;

    protected override void MoreActionInAwake()
    {
        turnMusicToggle.SetState(gameSetting.IsTurnOnBackgroundMusic);
        turnSoundToggle.SetState(gameSetting.IsTurnOnSound);
    }

    protected override void RegisterMoreEvent()
    {
        ScriptableObjectInitializer.gameSettingLoadedEvent += OnGameSettingLoaded;

        returnHomeButton.onClick.AddListener(ReturnHome);
        replayLevelButton.onClick.AddListener(Replay);
        turnMusicToggle.RegisterOnSwitchedEvent(EnableGameMusic);
        turnSoundToggle.RegisterOnSwitchedEvent(EnableGameSound);
    }

    protected override void UnregisterMoreEvent()
    {
        ScriptableObjectInitializer.gameSettingLoadedEvent -= OnGameSettingLoaded;
    }

    // protected override void AfterHide()
    // {
    //     Time.timeScale = 1;
    // }

    private void ReturnHome()
    {
        // Time.timeScale = 1;

        Addressables.LoadSceneAsync(GameConstants.MENU_SCENE);
    }

    private void Replay()
    {
        replayLevelEvent?.Invoke();

        Hide();
    }

    private void EnableGameMusic(bool isTurnOn)
    {
        gameSetting.IsTurnOnBackgroundMusic = isTurnOn;

        enableBackgroundMusicEvent?.Invoke(isTurnOn);
    }

    private void EnableGameSound(bool isTurnOn)
    {
        gameSetting.IsTurnOnSound = isTurnOn;

        enableGameSoundEvent?.Invoke(isTurnOn);
    }

    private void OnGameSettingLoaded()
    {
        turnMusicToggle.SetState(gameSetting.IsTurnOnBackgroundMusic);
        turnSoundToggle.SetState(gameSetting.IsTurnOnSound);
    }
}
