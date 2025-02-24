#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using SearchEngine.Additions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SearchEngine.EditorViews.Tables
{
    [Serializable]
    public class AssetObjectAlg : AssetAlg2<string>
    {
        public AssetObjectAlg(IList<string> data, List<bool> dataToggles, string titleTemplate) : base(data, dataToggles, titleTemplate)
        {
        }

        protected override void ShowAsset(string asset)
        {
            HelperGUI.AssetField(asset);           
        }
    }
}

#endif