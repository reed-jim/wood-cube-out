using System;
using Lean.Localization;
using NamPhuThuy;
using NamPhuThuy.Data;
using NamPhuThuy.GamePlay;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace NamPhuThuy.UI
{
    public class LevelSpecialTutorialPopup : BasePopup
    {
        [Header("Components")] [SerializeField]
        private VideoPlayer videoPlayer;

        [SerializeField] private LeanLocalizedTextMeshProUGUI descriptionLocalized;
        [SerializeField] private Button okButton;

        [Header("Data")] 
        [SerializeField] private LevelSpecialTutorialData levelSpecialTutorialData;
        
        [Header("Flags")] 
        [SerializeField] private LevelMode currentMode;

        protected override void RegisterMoreEvent()
        {
            base.RegisterMoreEvent();

            okButton.onClick.AddListener(Hide);
        }

        protected override void Show(Action onCompletedAction = null)
        {
            DebugLogger.LogSimple(context: this);
            base.Show(onCompletedAction: () =>
            {
                currentMode = GamePersistentVariable.currentTutLevelMode;
                SwitchVideoBasedOnLevelMode();
                videoPlayer.Play();
            });
        }

        protected override void Hide()
        {
            base.Hide();
            DebugLogger.LogSimple(context: this);

            videoPlayer.Stop();
        }
        
        #region Private Methods

        private void SwitchVideoBasedOnLevelMode()
        {
            
            DebugLogger.LogSimple(message:$"mode: {currentMode}", context:this);
            
            if (levelSpecialTutorialData != null && levelSpecialTutorialData.TryGet(currentMode, out var record))
            {
                // Set video clip
                if (record.videoClip != null)
                {
                    videoPlayer.clip = record.videoClip;
                }

                // Set localized description
                if (!string.IsNullOrEmpty(record.description))
                {
                    descriptionLocalized.TranslationName = record.description;
                }
            }
            else
            {
                DebugLogger.LogSimple(message:$"No tutorial data found for LevelMode: {currentMode}.", context: this);
            }
        }


        #endregion
    }
}
