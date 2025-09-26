using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.GamePlay
{
    
    public class ScrewBoxLocked : ScrewBox
    {
        #region Private Serializable Fields

        #endregion

        #region Private Fields

        #endregion

        #region Private Methods
        #endregion

        #region Public Methods
        #endregion

        #region Override Methods

        public override void Lock()
        {
            // base.Lock();
        }

        public override void Unlock()
        {
            // base.Unlock();
        }

        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(ScrewBoxLocked))]
    [CanEditMultipleObjects]
    public class ScrewBoxLockedEditor : Editor
    {
        private ScrewBoxLocked script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (ScrewBoxLocked)target;

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
}