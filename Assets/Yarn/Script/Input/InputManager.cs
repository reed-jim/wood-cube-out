using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Common;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    public class InputManager : MonoBehaviour
    {
        #region Private Serializable Fields

        [Header("Components")]
        [SerializeField] private YarnGameInput yarnGameInput;
        [SerializeField] private SwipeGesture swipeGesture;
        [SerializeField] private PinchGesture pinchGesture;
        
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            pinchGesture.isPinchingEvent += OnPinchingStateChanged;
            YarnLevelLoader.isTutAvailableEvent += CheckTutAvailable;
        }

        private void OnDestroy()
        {
            pinchGesture.isPinchingEvent -= OnPinchingStateChanged;
            YarnLevelLoader.isTutAvailableEvent -= CheckTutAvailable;
        }

       
        #endregion

        #region Private Methods
        
        private void OnPinchingStateChanged(bool isPinching)
        {
            yarnGameInput.enabled = !isPinching;
        }

        
        #endregion

        #region Public Methods
        #endregion

        #region Events Listen

        private void CheckTutAvailable(bool isTutAvailable, bool isDisableUI)
        {
            // DebugLogger.Log(message:$"isTutAvailable: {isTutAvailable}", context:this);
            pinchGesture.ToggleSlider(!(isTutAvailable && isDisableUI));
        }

        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion
    }

    /*#if UNITY_EDITOR
    [CustomEditor(typeof(InputManager))]
    [CanEditMultipleObjects]
    public class InputManagerEditor : Editor
    {
        private InputManager script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (InputManager)target;

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