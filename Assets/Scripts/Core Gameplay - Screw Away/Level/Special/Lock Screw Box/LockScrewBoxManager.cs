using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.GamePlay
{
    
    public class LockScrewBoxManager : BaseSpecialLevelManager
    {
        #region Private Serializable Fields

        #endregion

        #region Private Fields

        private Coroutine _createNewTaskCo;

        #endregion

        #region MonoBehaviour Callbacks


        #endregion

        #region Private Methods

        private void LockBoxes()
        {
            
        }
        
        private void LockBox(int id)
        {
            
        }

        private IEnumerator IECreateNewTask()
        {
            yield return null;
        }
        #endregion

        #region Override Methods

        protected override void OnScrewLoosed()
        {
            DebugLogger.LogSimple();
            
            // CHeck task progress
            // Update UI 
            /*_screwLeftToUnlock--;

            conditionTexts[0].text = $"{_screwLeftToUnlock}/{_requiredScrewToUnlock}";

            if (_screwLeftToUnlock == 0)
            {
                foreach (var text in conditionTexts)
                {
                    SaferioTween.ScaleDownWithPunch(text.transform);
                }

                SaferioTween.ScaleDownWithPunch(lockedGroup.transform);

                SaferioTween.ScaleUpWithPunch(tickIcon.transform, 1, delay: 0.3f);
                SaferioTween.ScaleUpWithPunch(timeCountingText.transform, 1, delay: 0.3f);

                _currentLockedFaction = null;

                if (_createNewTaskCo != null) StopCoroutine(_createNewTaskCo);
                _createNewTaskCo = StartCoroutine(IECreateNewTask());
            }*/
        }

        #endregion

        #region Public Methods
        #endregion

        #region Event Methods

        private void OnLevelLoaded()
        {
            LockBoxes();
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