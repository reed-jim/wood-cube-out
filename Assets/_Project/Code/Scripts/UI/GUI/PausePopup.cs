using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{
    public class PausePopup : BasePopup
    {
        #region Private Serializable Fields

        [Header("Buttons")]
        [SerializeField] private ButtonClicky continueButton;
        [SerializeField] private ButtonClicky replayButton;
        [SerializeField] private ButtonClicky homeButton;


        [Header("Audio Settings")]
        [SerializeField] private Button musicButton;
        [SerializeField] private Button sfxButton;
        [SerializeField] private Button vibrationButton;

        [SerializeField] private GameSetting gameSetting;

        #endregion

        #region Events

        public static event Action<GameEnum.ScreenRoute> switchRouteEvent;
        public static event Action replayLevelEvent;
        public static event Action<bool> enableBackgroundMusicEvent;
        public static event Action<bool> enableGameSoundEvent;

        #endregion


        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        protected override void MoreActionInAwake()
        {
            base.MoreActionInAwake();

            continueButton.onClick.AddListener(OnClickContinue);
            replayButton.onClick.AddListener(OnClickReplay);
            homeButton.onClick.AddListener(OnClickHome);

            // AUDIO SETTINGS
            musicButton.onClick.AddListener(OnClickMusic);
            sfxButton.onClick.AddListener(OnClickSound);
            vibrationButton.onClick.AddListener(OnClickVibration);
        }

        private void OnValidate()
        {
            route = GameEnum.ScreenRoute.Pause;
        }

        protected override void MoreActionOnDestroy()
        {
            base.MoreActionOnDestroy();
            continueButton.onClick.RemoveAllListeners();
            replayButton.onClick.RemoveAllListeners();
            homeButton.onClick.RemoveAllListeners();
            // AUDIO SETTINGS
            musicButton.onClick.RemoveAllListeners();
            sfxButton.onClick.RemoveAllListeners();
            vibrationButton.onClick.RemoveAllListeners();
        }

        void OnEnable()
        {
            SetStateForButton();
        }


        #endregion

        #region Private Methods

        private void SetStateForButton()
        {
            DebugLogger.Log();
            // SECOND WAY: WORKS
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

        private void ActualReplay()
        {
            SaferioTracking.TrackLevelLose(DataManager.Ins.CurrentLevel.Value, DataManager.Ins.LevelObserver.Progress, DataManager.Ins.LevelBoosterObserver, GameEnum.EndLevelReason.Replay.ToString());

            // don't track loop level CostCenter
            if (DataManager.Ins.CurrentLevel.Value <= DataManager.Ins.LevelDataContainer.MaxLevel)
            {
                SaferioTracking.TrackLevelEnd(DataManager.Ins.CurrentLevel.Value, isSuccess: false, GameEnum.EndLevelReason.Replay.ToString());
            }

            replayLevelEvent?.Invoke();
        }

        private void OnInterstitialAdToReplayCompleted()
        {
            SaferioTracking.TrackInterstitialAdCompleted(DataManager.Ins.CurrentLevel.Value, "replay");

            ActualReplay();
        }
        #endregion

        #region Button Events

        private void OnClickContinue()
        {
            Hide();
        }

        private void OnClickHome()
        {
            if (GamePersistentVariable.isLevelDirty && GamePersistentVariable.livesConfig.isActive)
            {
                switchRouteEvent?.Invoke(GameEnum.ScreenRoute.ReturnHome);
            }
            else
            {
                // avoid spamming
                homeButton.interactable = false;
                replayButton.interactable = false;

                SaferioTracking.TrackLevelLose(DataManager.Ins.CurrentLevel.Value, DataManager.Ins.LevelObserver.Progress, DataManager.Ins.LevelBoosterObserver, GameEnum.EndLevelReason.ReturnHome.ToString());

                // don't track loop level CostCenter
                if (DataManager.Ins.CurrentLevel.Value <= DataManager.Ins.LevelDataContainer.MaxLevel)
                {
                    SaferioTracking.TrackLevelEnd(DataManager.Ins.CurrentLevel.Value, isSuccess: false, GameEnum.EndLevelReason.ReturnHome.ToString());
                }

                Addressables.LoadSceneAsync(GameConstants.MENU_SCENE);
            }
        }

        private void OnClickReplay()
        {
            DebugLogger.Log();
            if (GamePersistentVariable.isLevelDirty && GamePersistentVariable.livesConfig.isActive)
            {
                DebugLogger.Log(message: $"About to open Replay Popup");
                switchRouteEvent?.Invoke(GameEnum.ScreenRoute.Replay);
            }
            else
            {
                bool isShowInterReplayLevel = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_SHOW_INTER_REPLAY_LEVEL, false);

                if (isShowInterReplayLevel)
                {
                    AdmobAdsMax.Instance.ShowInterstitial(actionIniterClose: OnInterstitialAdToReplayCompleted);
                }
                else
                {
                    ActualReplay();
                }

                Hide();
            }
        }

        private void OnClickMusic()
        {
            DebugLogger.Log();
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
            bool isVibrate = gameSetting.IsVibrate;
            gameSetting.IsVibrate = !isVibrate;
            GamePersistentVariable.IsVibrate = !isVibrate;
        
            DebugLogger.Log(message: $"GamePersistentVariable.IsVibrate: {GamePersistentVariable.IsVibrate}", context: this);
            DebugLogger.Log(message: $"gameSetting.IsVibrate: {gameSetting.IsVibrate}", context: this);

            ButtonSwitchFade vibrateSwitch = vibrationButton.GetComponent<ButtonSwitchFade>();
            vibrateSwitch?.SetState(gameSetting.IsVibrate ? ButtonSwitchFade.State.ON : ButtonSwitchFade.State.OFF);
        }

        #endregion

        #region Public Methods
        #endregion

        #region Editor Methods

        public void ResetValues()
        {

        }

        #endregion
    }
}