using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.GamePlay
{
    
    public class LockScrewBoxManager : Singleton<LockScrewBoxManager>
    {
        #region Private Serializable Fields

        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        protected override void MoreActionOnAwake()
        {
            LevelLoader.startLevelEvent += OnLevelLoaded;
            BaseScrew.screwLoosenedEvent += OnScrewLoosed;
        }

        private void OnDestroy()
        {
            LevelLoader.startLevelEvent -= OnLevelLoaded;
            BaseScrew.screwLoosenedEvent -= OnScrewLoosed;
        } 

        #endregion

        #region Private Methods

        private void LockBoxes()
        {
            
        }
        
        #endregion

        #region Public Methods
        #endregion

        #region Event Methods

        private void OnLevelLoaded()
        {
            LockBoxes();
        }
        
        private void OnScrewLoosed()
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(LockScrewBoxManager))]
    [CanEditMultipleObjects]
    public class LockScrewBoxManagerEditor : Editor
    {
        private LockScrewBoxManager script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (LockScrewBoxManager)target;

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