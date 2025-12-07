using System;
using Lean.Localization;
using NamPhuThuy.UI;
using UnityEngine;

public class GameSettingManager : MonoBehaviour
{
    [SerializeField] private GameSetting gameSetting;

    public static event Action<bool> enableBackgroundMusicEvent;
    public static event Action<bool> enableGameSoundEvent;

    private void Awake()
    {
        SettingsPopup.enableBackgroundMusicEvent += EnableBackgroundMusic;
        SettingsPopup.enableGameSoundEvent += EnableGameSound;
        NamPhuThuy.UI.PausePopup.enableBackgroundMusicEvent += EnableBackgroundMusic;
        NamPhuThuy.UI.PausePopup.enableGameSoundEvent += EnableGameSound;

        EnableBackgroundMusic(gameSetting.IsTurnOnBackgroundMusic);
        EnableGameSound(gameSetting.IsTurnOnSound);

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        SettingsPopup.enableBackgroundMusicEvent -= EnableBackgroundMusic;
        SettingsPopup.enableGameSoundEvent -= EnableGameSound;
        NamPhuThuy.UI.PausePopup.enableBackgroundMusicEvent -= EnableBackgroundMusic;
        NamPhuThuy.UI.PausePopup.enableGameSoundEvent -= EnableGameSound;
    }

    private void EnableBackgroundMusic(bool isEnable)
    {
        enableBackgroundMusicEvent?.Invoke(isEnable);
    }

    private void EnableGameSound(bool isEnable)
    {
        enableGameSoundEvent?.Invoke(isEnable);
    }
}
