using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        
        
        [Header("UI")]
        [SerializeField] private GameObject noInternetPopup;
        #endregion

        #region Private Fields

        private static bool HasInternet() =>
            Application.internetReachability != NetworkReachability.NotReachable;

        
        #endregion

        #region MonoBehaviour Callbacks

        
        private async void Start()
        {
            DebugLogger.LogFrog(message: $"Pre-loading levels {levelKey} 31 to 60...");
            using var cts = new CancellationTokenSource();

            try
            {
                await WaitForInternetAsync(cts.Token);
                await PreloadLevelsWithRetry(cts.Token);
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
        }

        #endregion

        #region Private Methods
        
        // Wait until internet is reachable, showing the popup while offline.
        private async Task WaitForInternetAsync(CancellationToken ct)
        {
            while (!HasInternet())
            {
                ShowNoInternetPopup(true);
                await Task.Delay(500, ct); // poll
            }

            ShowNoInternetPopup(false);
        }
        
        private async Task PreloadLevelsWithRetry(CancellationToken ct)
        {
            DebugLogger.LogFrog(message: $"Pre-loading levels {levelKey} 31 to 60...");

            await WaitForInternetAsync(ct);

            var managerInit = await AddressablesManager.InitializeAsync();
            if (!managerInit.Succeeded)
            {
                DebugLogger.LogFrog(message: $"AddressablesManager failed to initialize.");
                return;
            }

            for (int i = 31; i <= 60; i++)
            {
                await WaitForInternetAsync(ct);

                string key = $"{levelKey} {i}";
                var loadResult = await AddressablesManager.LoadAssetAsync<GameObject>(key);

                if (!loadResult.Succeeded)
                {
                    DebugLogger.LogError($"Failed to pre-load level {key}");
                    return;
                }
            }
        }

        private void ShowNoInternetPopup(bool show)
        {
            if (noInternetPopup != null)
            {
                noInternetPopup.SetActive(show);
            }
        }
        
        private async Task PreloadLevels()
        {
            DebugLogger.LogFrog(message: $"Pre-loading levels {levelKey} 31 to 60...");

            var managerInit = await AddressablesManager.InitializeAsync();
            if (!managerInit.Succeeded)
            {
                DebugLogger.LogFrog(message: $"AddressablesManager failed to initialize.");
                return;
            }

            for (int i = 31; i <= 60; i++)
            {
                if (!HasInternet())
                {
                    ShowNoInternetPopup(true);
                    return;
                }

                string key = $"{levelKey} {i}";
                var loadResult = await AddressablesManager.LoadAssetAsync<GameObject>(key);

                if (!loadResult.Succeeded)
                {
                    DebugLogger.LogError($"Failed to pre-load level {key}");
                    return;
                }
            }
        }
        
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