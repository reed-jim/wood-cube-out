using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.PuzzleTutorial;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{
    
    public class TutorialPopup : BasePopup
    {
        #region Private Serializable Fields
        
        [Header("UGUI Elements")]
        [SerializeField] private Image boosterImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;

        [Header("Components")] 
        [SerializeField] private IntVariable currentLLevelId; 

        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        protected override void MoreActionInAwake()
        {
            base.MoreActionInAwake();
            
            confirmButton.onClick.AddListener(OnClickConfirm);
            closeButton.onClick.AddListener(OnClickClose);
        }

        protected override void MoreActionOnDestroy()
        {
            base.MoreActionOnDestroy();
        }

        #endregion

        #region Button Methods
        
        private void OnClickConfirm()
        {
            throw new NotImplementedException();
        }

        private void OnClickClose()
        {
            throw new NotImplementedException();
        }
        
        #endregion

        #region Override Methods

        protected override void Show(Action onCompletedAction = null)
        {
            base.Show(onCompletedAction);
            // GameEnum.BoosterType boosterType = TutorialManager.Ins.Data.GetTutRecord(currentLLevelId.Value).
            
        }

        protected override void Hide(Action onCompletedAction = null)
        {
            base.Hide(onCompletedAction);
        }

        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion
    }

    /*#if UNITY_EDITOR
    [CustomEditor(typeof(TutorialPopup))]
    [CanEditMultipleObjects]
    public class TutorialPopupEditor : Editor
    {
        private TutorialPopup script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (TutorialPopup)target;

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