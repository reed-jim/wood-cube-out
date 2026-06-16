using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Gplay.Notifications
{
    public class NotificationEditor : MonoBehaviour
    {
        [MenuItem("Gplay/Notifications/Create Notification Settings")]
        public static void CreateNotificationSettings()
        {
            string assetPath = "Assets/Notification/Data/NotificationSettings.asset";
            NotificationSettings asset = AssetDatabase.LoadAssetAtPath<NotificationSettings>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<NotificationSettings>();
                AssetDatabase.CreateAsset(asset, assetPath); 
                AssetDatabase.SaveAssets();
                Debug.LogError("Create Notification Settings Success!!!");
            }
            else
            {
                Debug.LogError("Notification Settings already exists!");
            }
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }

}