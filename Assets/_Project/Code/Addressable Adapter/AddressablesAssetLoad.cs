using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.AddressablesAdapter
{
    
    public class AddressablesAssetLoad : MonoBehaviour
    {
        #region Private Serializable Fields

        [SerializeField] private string levelKey;
        [SerializeField] private string pictureKey;
        
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        
        private async void Start()
        {
            DebugLogger.LogFrog(message: $"Pre-loading levels {levelKey} 31 to 60...");
            // Initialize Addressables and load the level prefab once
            var managerInit = await AddressablesManager.InitializeAsync();
            if (managerInit.Succeeded)
            {
                DebugLogger.LogFrog(message: $"AddressablesManager initialized successfully.");
            }
            else
            {
                DebugLogger.LogFrog(message: $"AddressablesManager failed to initialize.");
            }

            for (int i = 31; i <= 60; i++)
            {
                string key = $"{levelKey} {i}";
                var loadResult = await AddressablesManager.LoadAssetAsync<GameObject>(key);
                
                if (!loadResult.Succeeded)
                {
                    DebugLogger.LogError($"Failed to pre-load level {key}");
                    return;
                }
                
            }
            
            /*for (int i = 0; i <= 69; i++)
            {
                string key = $"{pictureKey} {i}";
                var loadResult = await AddressablesManager.LoadAssetAsync<GameObject>(key);
                
                if (!loadResult.Succeeded)
                {
                    DebugLogger.LogError($"Failed to pre-load level {key}");
                    return;
                }
                
            }*/
            
            /*// Later, when starting the level:
            var instantiateResult = await AddressablesManager.InstantiateAsync(levelKey, transform);
            if (!instantiateResult.Succeeded)
            {
                DebugLogger.LogError($"Failed to instantiate level {levelKey}");
            }*/
        }

        #endregion

        #region Private Methods

        
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
    [CustomEditor(typeof(AddressablesAssetLoad))]
    [CanEditMultipleObjects]
    public class AddressablesAssetLoadEditor : Editor
    {
        private AddressablesAssetLoad script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (AddressablesAssetLoad)target;

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