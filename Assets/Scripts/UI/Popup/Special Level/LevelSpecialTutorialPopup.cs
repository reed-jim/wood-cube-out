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

        [Header("Data")] [SerializeField] private LevelSpecialTutorialData levelSpecialTutorialData;

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
                SwitchBasedOnLevelMode();
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

        private void SwitchBasedOnLevelMode()
        {
            LevelMode currentMode = GamePersistentVariable.currentTutLevelMode;
            
            if (levelSpecialTutorialData != null && levelSpecialTutorialData.TryGet(currentMode, out var record))
            {
                if (record.hasTutorial)
                {
                    // Set video clip
                    if (record.videoClip != null)
                    {
                        videoPlayer.clip = record.videoClip;
                    }

                    // Set localized description
                    if (!string.IsNullOrEmpty(record.descriptionKey))
                    {
                        descriptionLocalized.TranslationName = record.descriptionKey;
                    }
                }
                else
                {
                    Debug.LogWarning($"Tutorial record found for {currentMode} but hasTutorial is false.", this);
                }
            }
            else
            {
                Debug.LogWarning($"No tutorial data found for LevelMode: {currentMode}.", this);
            }
        }


        #endregion
    }
}
