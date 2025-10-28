using System;
using Lean.Localization;
using NamPhuThuy;
using NamPhuThuy.Data;
using NamPhuThuy.GamePlay;
using TMPro;
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
        [SerializeField] private LeanLocalizedTextMeshProUGUI titleLocalized;
        [SerializeField] private TextMeshProUGUI descriptionText;
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
                    DebugLogger.LogSimple(message:$"record.description: {record.description}", context: this);
                    descriptionLocalized.TranslationName = record.descriptionTranslationName;
                    // descriptionLocalized.UpdateTranslationWithParameter(record.modeName, record.description);
                }
                
                if (record.modeName != null)
                {
                    DebugLogger.LogSimple(message:$"record.modeName: {record.modeName}", context: this);
                    titleLocalized.TranslationName = record.modeName;
                    // titleLocalized.UpdateLocalization();
                }
                
                /*if (record.description != null)
                {
                    descriptionText.text = record.description;
                }*/
            }
            else
            {
                DebugLogger.LogSimple(message:$"No tutorial data found for LevelMode: {currentMode}.", context: this);
            }
        }


        #endregion
    }
}
