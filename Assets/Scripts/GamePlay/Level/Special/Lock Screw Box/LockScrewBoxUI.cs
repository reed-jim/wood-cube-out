/*using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.GamePlay;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.GamePlay
{
    
    public class LockScrewBoxUI : /*BaseSpecialLevelUI#1# MonoBehaviour
    {
        #region Private Serializable Fields
        
        [SerializeField] private TextMeshProUGUI progressText;

        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks
        
        

        #endregion

        #region Private Methods

        private void UpdateUIProgress(SpecialLevelTaskRecord record)
        {
            int total = 0;
            int remaining = 0;
            foreach (var componentAmount in record.requiredComponents)
            {
                if (GameComponentType.Screw != componentAmount.componentType) continue;
                
                total += componentAmount.amount;
                remaining += componentAmount.remainingAmount;
            }

            progressText.text = $"{remaining}/{total}";
        }
        
        #endregion

        #region Public Methods
        #endregion

        #region Override Methods

        /*protected override void UpdateUI(SpecialLevelUIState state, object[] parameters)
        {
            base.UpdateUI(state, parameters);

            switch (state)
            {
                case SpecialLevelUIState.Setup:
                    Setup((LevelMode)parameters[0]);
                    break;
                case SpecialLevelUIState.Showing:
                    ShowUI();
                    break;
                case SpecialLevelUIState.ReActive:
                    ReActive();
                    break;
                case SpecialLevelUIState.UpdateProgress:
                    break;
                case SpecialLevelUIState.UpdateTimeCount:
                    UpdateTimeCount((int)parameters[0]);
                    break;
            }
        }#1#

        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(LockScrewBoxUI))]
    [CanEditMultipleObjects]
    public class LockScrewBoxUIEditor : Editor
    {
        private LockScrewBoxUI script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (LockScrewBoxUI)target;

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
    #endif
}*/