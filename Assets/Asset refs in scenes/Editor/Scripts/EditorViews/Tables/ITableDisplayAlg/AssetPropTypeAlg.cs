#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using SearchEngine.Additions;
using UnityEditor;
using UnityEngine;

namespace SearchEngine.EditorViews.Tables
{
    [Serializable]
    public class AssetPropTypeAlg : AssetAlg2<string>
    {
        protected override void ShowAsset(string propType)
        {
            GUILayout.BeginHorizontal(HelperGUI.GUIStyleObjectField2());
            GUILayout.Label(propType);
            GUILayout.EndHorizontal();
        }

        public AssetPropTypeAlg(List<string> data, List<bool> dataToggles, string titleTemplate) 
            : base(data, dataToggles, titleTemplate)
        {
        }
    }
}

#endif