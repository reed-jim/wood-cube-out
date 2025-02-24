#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SearchEngine.Additions;
using UnityEditor;

namespace SearchEngine.EditorViews
{
    public class AssetPathsEV : IEditorView
    {
        private string assetsFolder = string.Empty;
        private GUIStyle asssetFolderTextFieldStyle;

        public void ShowGUI()
        {
            if (asssetFolderTextFieldStyle == null)
            {
                asssetFolderTextFieldStyle = (GUIStyle)HelperGUI.CopyObject(EditorStyles.textField, 1);
            }
            if (asssetFolderTextFieldStyle == null)
            {
                asssetFolderTextFieldStyle = new GUIStyle();
            }

            GUILayout.BeginHorizontal();
                string path = string.Format(@"{0}/{1}", Application.dataPath, assetsFolder);
                UpdateAsFolderTextFieldStyle(!Directory.Exists(path));
                GUILayout.Label("Path folder", GUILayout.ExpandWidth(false));
                assetsFolder = GUILayout.TextField(assetsFolder, asssetFolderTextFieldStyle);
            GUILayout.EndHorizontal();
        }

        private void UpdateAsFolderTextFieldStyle(bool validState)
        {
            if (validState)
            {
                asssetFolderTextFieldStyle.normal.textColor = Color.red;
                asssetFolderTextFieldStyle.active.textColor = Color.red;
                asssetFolderTextFieldStyle.focused.textColor = Color.red;
            }
            else
            {
                asssetFolderTextFieldStyle.normal.textColor = EditorStyles.textField.normal.textColor;
                asssetFolderTextFieldStyle.active.textColor = EditorStyles.textField.active.textColor;
                asssetFolderTextFieldStyle.focused.textColor = EditorStyles.textField.focused.textColor;
            }
        }

        public IEnumerable<string> FindAssets(string assetsFormat)
        {
            string path = null;
            if (string.IsNullOrEmpty(assetsFolder))
                path = "Assets";
            else
                path = string.Format(@"Assets/{0}", assetsFolder);
            return HelperGeneral.GetAssets(path, assetsFormat);
        }

        public void SetPathfolder(string newAssetsFolder)
        {
            assetsFolder = newAssetsFolder;
        }
    }
}

#endif