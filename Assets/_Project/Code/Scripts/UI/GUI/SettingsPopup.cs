using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.AnimateWithScripts;
using NamPhuThuy.Data;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{

    public class SettingsPopup : BasePopup
    {
        #region Private Serializable Fields

        [Header("Buttons")]
        [SerializeField] private ButtonClicky languageButton;
        [SerializeField] private ButtonClicky supportButton;
        [SerializeField] private ButtonClicky privacyPolicyButton;
        [SerializeField] private ButtonClicky termsOfServiceButton;
        [SerializeField] private ButtonClicky rateButton;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI versionText;

        [Header("Audio Settings")]
        [SerializeField] private ButtonClicky musicButton;
        [SerializeField] private ButtonClicky sfxButton;
        [SerializeField] private ButtonClicky vibrationButton;
        [SerializeField] private ButtonClicky notificationButton;

        [SerializeField] private GameSetting gameSetting;

        #endregion

        #region Private Fields

        #endregion

        public static event Action<bool> enableBackgroundMusicEvent;
        public static event Action<bool> enableGameSoundEvent;

        #region MonoBehaviour Callbacks

        private void Start()
        {
            versionText.text = $"Version {Application.version}";

        }

        void OnEnable()
        {
            rateButton.onClick.AddListener(OnClickRate);
            languageButton.onClick.AddListener(OnClickLanguage);
            supportButton.onClick.AddListener(OnClickSupport);
            privacyPolicyButton.onClick.AddListener(OnClickPrivacyPolicy);
            termsOfServiceButton.onClick.AddListener(OnClickTermsOfService);

            // AUDIO SETTINGS
            musicButton.onClick.AddListener(OnClickMusic);
            sfxButton.onClick.AddListener(OnClickSound);
            vibrationButton.onClick.AddListener(OnClickVibration);
            notificationButton.onClick.AddListener(OnClickNotification);

            SetStateForButton();
        }

        private void OnDisable()
        {
            closeButton.onClick.RemoveListener(OnClickClose);
            languageButton.onClick.RemoveAllListeners();
            supportButton.onClick.RemoveAllListeners();
            privacyPolicyButton.onClick.RemoveAllListeners();
            termsOfServiceButton.onClick.RemoveAllListeners();
            // AUDIO SETTINGS
            musicButton.onClick.RemoveAllListeners();
            sfxButton.onClick.RemoveAllListeners();
            vibrationButton.onClick.RemoveAllListeners();
            notificationButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Private Methods

        private void SetStateForButton()
        {
            DebugLogger.Log();
            bool tmp = gameSetting.IsTurnOnBackgroundMusic;
            ButtonSwitchFade musicSwitch = musicButton.GetComponent<ButtonSwitchFade>();
            if (musicSwitch != null)
            {
                musicSwitch?.SetState(tmp ? ButtonSwitchFade.State.ON : ButtonSwitchFade.State.OFF);
            }

            tmp = gameSetting.IsTurnOnSound;
            ButtonSwitchFade sfxSwitch = sfxButton.GetComponent<ButtonSwitchFade>();
            if (sfxSwitch != null)
            {
                sfxSwitch?.SetState(tmp ? ButtonSwitchFade.State.ON : ButtonSwitchFade.State.OFF);
            }

            tmp = gameSetting.IsVibrate;
            ButtonSwitchFade vibrateSwitch = vibrationButton.GetComponent<ButtonSwitchFade>();
            if (vibrateSwitch != null)
            {
                vibrateSwitch?.SetState(tmp ? ButtonSwitchFade.State.ON : ButtonSwitchFade.State.OFF);
            }
        }

        #endregion

        #region Public Methods
        #endregion

        #region Button Events

        private void OnClickClose()
        {
            Hide();
        }

        private void OnClickRate()
        {
            // GUIManager.Ins.ShowGUI(GUIManager.Ins.GUIRating);
            SwitchRoute(GameEnum.ScreenRoute.Rate, false);

        }

        private void OnClickLanguage()
        {
            // GUIManager.Ins.ShowGUI(GUIManager.Ins.GUILanguage);
            SwitchRoute(GameEnum.ScreenRoute.Language, false);
        }

        private void OnClickSupport()
        {
            throw new NotImplementedException();
        }

        private void OnClickPrivacyPolicy()
        {
            throw new NotImplementedException();
        }

        private void OnClickTermsOfService()
        {
            throw new NotImplementedException();
        }

        private void OnClickMusic()
        {
            gameSetting.IsTurnOnBackgroundMusic = !gameSetting.IsTurnOnBackgroundMusic;

            enableBackgroundMusicEvent?.Invoke(gameSetting.IsTurnOnBackgroundMusic);

            ButtonSwitchFade musicSwitch = musicButton.GetComponent<ButtonSwitchFade>();
            musicSwitch?.SetState(gameSetting.IsTurnOnBackgroundMusic ? ButtonSwitchFade.State.ON : ButtonSwitchFade.State.OFF);
        }

        private void OnClickSound()
        {
            DebugLogger.Log();
            gameSetting.IsTurnOnSound = !gameSetting.IsTurnOnSound;

            enableGameSoundEvent?.Invoke(gameSetting.IsTurnOnSound);

            ButtonSwitchFade sfxSwitch = sfxButton.GetComponent<ButtonSwitchFade>();
            sfxSwitch?.SetState(gameSetting.IsTurnOnSound ? ButtonSwitchFade.State.ON : ButtonSwitchFade.State.OFF);
        }

        private void OnClickVibration()
        {
            DebugLogger.Log();
            gameSetting.IsVibrate = !gameSetting.IsVibrate;

            ButtonSwitchFade vibrateSwitch = vibrationButton.GetComponent<ButtonSwitchFade>();
            vibrateSwitch?.SetState(gameSetting.IsVibrate ? ButtonSwitchFade.State.ON : ButtonSwitchFade.State.OFF);
        }

        private void OnClickNotification()
        {
            DebugLogger.Log();
            bool tmp = DataManager.Ins.PlayerSettingsData.notifyEnabled;
            tmp = !tmp;
            DataManager.Ins.PlayerSettingsData.notifyEnabled = tmp;
            DataManager.Ins.MarkDirty();

            ButtonSwitchFade notifySwitch = sfxButton.GetComponent<ButtonSwitchFade>();
            notifySwitch?.SetState(tmp ? ButtonSwitchFade.State.ON : ButtonSwitchFade.State.OFF);
        }

        #endregion
    }
}