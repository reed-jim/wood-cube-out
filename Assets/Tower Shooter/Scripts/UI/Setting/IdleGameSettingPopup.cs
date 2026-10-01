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

    private Resolution[] availableResolutions;
    private SettingsData currentSettings;

    private const string SAVE_KEY = "settingsSaveData";

    private void Start()
    {
        SetupResolutions();
        LoadSettingsData();
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

        // Fallback default index if no settings exist yet
        if (currentSettings != null)
        {
            currentSettings.resolutionIndex = Mathf.Clamp(currentSettings.resolutionIndex, 0, availableResolutions.Length - 1);
        }
        else
        {
            currentSettings = GetDefaultSettingsData();
            currentSettings.resolutionIndex = currentResolutionIndex;
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        currentSettings.resolutionIndex = Mathf.Clamp(resolutionIndex, 0, availableResolutions.Length - 1);
        Resolution res = availableResolutions[currentSettings.resolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        currentSettings.isFullscreen = isFullscreen;
        Screen.fullScreen = isFullscreen;
    }

    #endregion

    #region Audio Controls

    public void SetMasterVolume(float volume)
    {
        currentSettings.masterVolume = volume;
        float dB = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        // mainAudioMixer.SetFloat("MasterVol", dB);
    }

    public void SetMusicVolume(float volume)
    {
        currentSettings.musicVolume = volume;
        float dB = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        // mainAudioMixer.SetFloat("MusicVol", dB);
    }

    public void SetSFXVolume(float volume)
    {
        currentSettings.sfxVolume = volume;
        float dB = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        // mainAudioMixer.SetFloat("SFXVol", dB);
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

        // Apply UI element values
        masterVolumeSlider.value = currentSettings.masterVolume;
        musicVolumeSlider.value = currentSettings.musicVolume;
        sfxVolumeSlider.value = currentSettings.sfxVolume;

        fullscreenToggle.isOn = currentSettings.isFullscreen;

        if (availableResolutions != null && availableResolutions.Length > 0)
        {
            resolutionDropdown.value = Mathf.Clamp(currentSettings.resolutionIndex, 0, availableResolutions.Length - 1);
            resolutionDropdown.RefreshShownValue();
        }

        // Apply physical changes
        SetMasterVolume(currentSettings.masterVolume);
        SetMusicVolume(currentSettings.musicVolume);
        SetSFXVolume(currentSettings.sfxVolume);
        SetFullscreen(currentSettings.isFullscreen);

        if (availableResolutions != null && availableResolutions.Length > currentSettings.resolutionIndex)
        {
            SetResolution(currentSettings.resolutionIndex);
        }
    }

    #endregion
}
