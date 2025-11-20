using System;
using System.Collections;
using System.Collections.Generic;
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
        
        #endregion

        #region Events

        public static event Action<GameEnum.ScreenRoute> switchRouteEvent;
        public static event Action replayLevelEvent;

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
            bool tmp = DataManager.Ins.GameSetting.IsTurnOnBackgroundMusic;
            ButtonSwitchFade musicSwitch = musicButton.GetComponent<ButtonSwitchFade>();
            if (musicSwitch != null)
            {
                musicSwitch?.SetState(tmp ? ButtonSwitchFade.State.ON : ButtonSwitchFade.State.OFF);
            }
            
            
            tmp = DataManager.Ins.GameSetting.IsTurnOnSound;
            ButtonSwitchFade sfxSwitch = sfxButton.GetComponent<ButtonSwitchFade>();
            if (sfxSwitch != null)
            {
                sfxSwitch?.SetState(tmp ? ButtonSwitchFade.State.ON : ButtonSwitchFade.State.OFF);
            }
            
            tmp = DataManager.Ins.GameSetting.IsVibrate;
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
            if (GamePersistentVariable.isLevelDirty && GamePersistentVariable.livesConfig.isActive)
            {
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
            bool tmp = DataManager.Ins.GameSetting.IsTurnOnBackgroundMusic;
            tmp = !tmp;
            DataManager.Ins.GameSetting.IsTurnOnBackgroundMusic = tmp;
            DataManager.Ins.MarkDirty();
            
            ButtonSwitchFade musicSwitch = musicButton.GetComponent<ButtonSwitchFade>();
            musicSwitch?.SetState(tmp ? ButtonSwitchFade.State.ON : ButtonSwitchFade.State.OFF);
        }

        private void OnClickSound()
        {
            bool tmp = DataManager.Ins.GameSetting.IsTurnOnSound;
            tmp = !tmp;
            DataManager.Ins.GameSetting.IsTurnOnSound = tmp;
            DataManager.Ins.MarkDirty();
            
            ButtonSwitchFade sfxSwitch = sfxButton.GetComponent<ButtonSwitchFade>();
            sfxSwitch?.SetState(tmp ? ButtonSwitchFade.State.ON : ButtonSwitchFade.State.OFF);
        }

        private void OnClickVibration()
        {
            bool tmp = DataManager.Ins.GameSetting.IsVibrate;
            tmp = !tmp;
            DataManager.Ins.GameSetting.IsVibrate = tmp;
            DataManager.Ins.MarkDirty();
            
            ButtonSwitchFade vibrateSwitch = vibrationButton.GetComponent<ButtonSwitchFade>();
            vibrateSwitch?.SetState(tmp ? ButtonSwitchFade.State.ON : ButtonSwitchFade.State.OFF);
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

    /*#if UNITY_EDITOR
    [CustomEditor(typeof(PausePopup))]
    [CanEditMultipleObjects]
    public class PausePopupEditor : Editor
    {
        private PausePopup script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (PausePopup)target;

            ButtonResetValues();
        }

        private void ButtonResetValues()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Reset Values", frogIcon), GUILayout.Width(InspectorConst.BUTTON_WIDTH_MEDIUM)))
            {
                script.ResetValues();
                EditorUtility.SetDirty(script); // Mark the object as dirty
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
    #endif*/
}