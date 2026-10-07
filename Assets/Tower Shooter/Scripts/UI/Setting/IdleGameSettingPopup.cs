using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class IdleGameSettingPopup : BasePopup
{
    [Header("Audio")]
    [SerializeField] private AudioMixer mainAudioMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Graphics & Display")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Action Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button discardButton;

    [SerializeField] private Button saveSlotButton;

    private Resolution[] availableResolutions;
    private SettingsData currentSettings;  // Currently active & saved settings
    private SettingsData pendingSettings;  // Temporary settings waiting for confirmation

    private const string SAVE_KEY = "settingsSaveData";

    public static Action<string, Action, Action> showConfirmPopupEvent;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }

        if (discardButton != null)
        {
            discardButton.onClick.AddListener(DiscardChanges);
        }

        saveSlotButton.onClick.AddListener(OpenSaveSlotPopup);
    }

    private void Start()
    {
        SetupResolutions();
        LoadSettingsData();
    }

    protected override void Hide()
    {
        if (IsDirty())
        {
            showConfirmPopupEvent?.Invoke(
            "You have unsaved changes. Do you want to discard them and close?",
            () =>
            {
                DiscardChanges();
                base.Hide();
            },
            null
        );
        }
        else
        {
            base.Hide();
        }
    }

    #region Resolution & Display Controls

    private void SetupResolutions()
    {
        availableResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            string option = availableResolutions[i].width + " x " + availableResolutions[i].height;
            options.Add(option);

            if (availableResolutions[i].width == Screen.currentResolution.width &&
                availableResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        if (currentSettings == null)
        {
            currentSettings = GetDefaultSettingsData();
            currentSettings.resolutionIndex = currentResolutionIndex;
        }

        ResetPendingSettingsToCurrent();
    }

    public void SetResolution(int resolutionIndex)
    {
        if (availableResolutions == null || availableResolutions.Length == 0) return;
        pendingSettings.resolutionIndex = Mathf.Clamp(resolutionIndex, 0, availableResolutions.Length - 1);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        pendingSettings.isFullscreen = isFullscreen;
    }

    #endregion

    #region Audio Controls

    public void SetMasterVolume(float volume)
    {
        pendingSettings.masterVolume = volume;
    }

    public void SetMusicVolume(float volume)
    {
        pendingSettings.musicVolume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        pendingSettings.sfxVolume = volume;
    }

    #endregion

    #region Confirmation, Discard & Application

    public void OnConfirmButtonClicked()
    {
        showConfirmPopupEvent?.Invoke("Are you sure you want to apply these settings changes?", ApplyAndSaveSettings, null);
    }

    /// <summary>
    /// Reverts pending settings back to currently active settings and refreshes UI controls.
    /// </summary>
    public void DiscardChanges()
    {
        ResetPendingSettingsToCurrent();
        UpdateUIFromSettings(currentSettings);
    }

    private void ResetPendingSettingsToCurrent()
    {
        pendingSettings = new SettingsData
        {
            masterVolume = currentSettings.masterVolume,
            musicVolume = currentSettings.musicVolume,
            sfxVolume = currentSettings.sfxVolume,
            isFullscreen = currentSettings.isFullscreen,
            resolutionIndex = currentSettings.resolutionIndex
        };
    }

    private void ApplyAndSaveSettings()
    {
        currentSettings.masterVolume = pendingSettings.masterVolume;
        currentSettings.musicVolume = pendingSettings.musicVolume;
        currentSettings.sfxVolume = pendingSettings.sfxVolume;
        currentSettings.isFullscreen = pendingSettings.isFullscreen;
        currentSettings.resolutionIndex = pendingSettings.resolutionIndex;

        ApplySettingsToSystem(currentSettings);
        SaveSettingsData();
    }

    private void ApplySettingsToSystem(SettingsData settings)
    {
        if (availableResolutions != null && availableResolutions.Length > settings.resolutionIndex)
        {
            Resolution res = availableResolutions[settings.resolutionIndex];
            Screen.SetResolution(res.width, res.height, settings.isFullscreen);
        }
        else
        {
            Screen.fullScreen = settings.isFullscreen;
        }

        ApplyAudioMixerVolume("MasterVol", settings.masterVolume);
        ApplyAudioMixerVolume("MusicVol", settings.musicVolume);
        ApplyAudioMixerVolume("SFXVol", settings.sfxVolume);
    }

    private void ApplyAudioMixerVolume(string parameterName, float volume)
    {
        if (mainAudioMixer == null) return;
        float dB = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        mainAudioMixer.SetFloat(parameterName, dB);
    }

    public bool IsDirty()
    {
        if (pendingSettings == null || currentSettings == null) return false;

        return pendingSettings.resolutionIndex != currentSettings.resolutionIndex
            || pendingSettings.isFullscreen != currentSettings.isFullscreen
            || !Mathf.Approximately(pendingSettings.masterVolume, currentSettings.masterVolume)
            || !Mathf.Approximately(pendingSettings.musicVolume, currentSettings.musicVolume)
            || !Mathf.Approximately(pendingSettings.sfxVolume, currentSettings.sfxVolume);
    }

    #endregion

    #region Save & Load Logic

    public void LoadSettingsData()
    {
        SettingsData saveData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey(SAVE_KEY), GetDefaultSettingsData());
        LoadSaveData(saveData);
    }

    public void SaveSettingsData()
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey(SAVE_KEY), GetSaveData());
    }

    public SettingsData GetDefaultSettingsData()
    {
        return new SettingsData
        {
            masterVolume = 0.8f,
            musicVolume = 0.8f,
            sfxVolume = 0.8f,
            isFullscreen = true,
            resolutionIndex = 0
        };
    }

    public SettingsData GetSaveData()
    {
        return currentSettings;
    }

    public void LoadSaveData(SettingsData saveData)
    {
        currentSettings = saveData ?? GetDefaultSettingsData();
        ResetPendingSettingsToCurrent();

        UpdateUIFromSettings(currentSettings);
        ApplySettingsToSystem(currentSettings);
    }

    private void UpdateUIFromSettings(SettingsData settings)
    {
        // Temporarily disable listeners or directly set values to prevent triggering pending updates
        masterVolumeSlider.value = settings.masterVolume;
        musicVolumeSlider.value = settings.musicVolume;
        sfxVolumeSlider.value = settings.sfxVolume;

        fullscreenToggle.isOn = settings.isFullscreen;

        if (availableResolutions != null && availableResolutions.Length > 0)
        {
            resolutionDropdown.value = Mathf.Clamp(settings.resolutionIndex, 0, availableResolutions.Length - 1);
            resolutionDropdown.RefreshShownValue();
        }
    }

    #endregion

    private void OpenSaveSlotPopup()
    {
        SwitchRoute(GameEnum.ScreenRoute.SaveSlot);
    }
}
